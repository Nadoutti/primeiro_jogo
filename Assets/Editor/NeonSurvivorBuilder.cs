using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace NeonSurvivor.EditorTools
{
    /// <summary>
    /// Monta a cena Main.unity inteira (câmera, arena, jogador, managers, HUD).
    /// Menu: Tools > Build Neon Survivor. Assim ninguém precisa editar o YAML da cena na mão.
    /// </summary>
    public static class NeonSurvivorBuilder
    {
        const string ScenePath = "Assets/Scenes/Main.unity";
        const string VolumePath = "Assets/Settings/NeonVolume.asset";
        static readonly Rect Arena = new(-24, -15, 48, 30);

        [MenuItem("Tools/Build Neon Survivor")]
        public static void Build()
        {
            ReimportSprites();
            ConfigurePlayer();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var player = BuildPlayer();
            var cam = BuildCamera(player.transform);
            BuildVolume();
            BuildArena();

            var managers = new GameObject("GameManager");
            var gm = managers.AddComponent<GameManager>();
            var waves = managers.AddComponent<WaveDirector>();
            waves.arena = Arena;
            managers.AddComponent<AudioManager>();
            var upgrades = managers.AddComponent<UpgradeSystem>();
            upgrades.player = player;

            // a HUD monta os elementos de UI no Awake (em runtime)
            var hud = new GameObject("HUD").AddComponent<HUD>();
            hud.player = player;
            hud.waves = waves;

            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<InputSystemUIInputModule>();

            gm.player = player;
            gm.waves = waves;
            gm.upgrades = upgrades;
            gm.hud = hud;

            cam.GetComponent<CameraFollow>().bounds = Arena;

            Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            foreach (var old in new[] { "Assets/Scenes/SampleScene.unity", "Assets/Scenes/SampleSceneSettings.lighting" })
                if (File.Exists(old)) AssetDatabase.DeleteAsset(old);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("Neon Survivor: cena montada em " + ScenePath);
        }

        static void ReimportSprites()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Resources/Sprites" }))
                AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(guid), ImportAssetOptions.ForceUpdate);
        }

        static void ConfigurePlayer()
        {
            PlayerSettings.productName = "Neon Survivor";
            PlayerSettings.companyName = "Pedro Nadotti";
            PlayerSettings.defaultScreenWidth = 960;
            PlayerSettings.defaultScreenHeight = 600;
            PlayerSettings.defaultWebScreenWidth = 960;
            PlayerSettings.defaultWebScreenHeight = 600;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = false;
            PlayerSettings.runInBackground = true;
        }

        static Sprite S(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(PixelArtImporter.Folder + name + ".png");

        static SpriteRenderer Sprite(GameObject go, string sprite, int order = 0)
        {
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = S(sprite);
            sr.sortingOrder = order;
            return sr;
        }

        static PlayerController BuildPlayer()
        {
            var go = new GameObject("Player");
            Sprite(go, "player_0");
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.mass = 5;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            go.AddComponent<CircleCollider2D>().radius = 0.35f;
            var pc = go.AddComponent<PlayerController>();
            pc.walkFrames = new[] { S("player_0"), S("player_1") };
            go.AddComponent<PlasmaGun>();
            go.AddComponent<OrbitBlades>();
            go.AddComponent<EmpPulse>();
            go.AddComponent<YSort>().footOffset = -0.5f;
            return pc;
        }

        static Camera BuildCamera(Transform target)
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            go.transform.position = new Vector3(0, 0, -10);
            var cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6.25f; // 600 px / 12,5 unidades = 48 px por unidade = pixel art 3x
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.02f, 0.08f);
            go.AddComponent<AudioListener>();
            var data = go.AddComponent<UniversalAdditionalCameraData>();
            data.renderPostProcessing = true;
            go.AddComponent<CameraFollow>().target = target;
            return cam;
        }

        static void BuildVolume()
        {
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(VolumePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, VolumePath);
                var bloom = profile.Add<Bloom>(true);
                bloom.intensity.Override(1.4f);
                bloom.threshold.Override(0.75f);
                bloom.scatter.Override(0.7f);
                var vignette = profile.Add<Vignette>(true);
                vignette.intensity.Override(0.32f);
                vignette.color.Override(new Color(0.15f, 0f, 0.25f));
                var ca = profile.Add<ChromaticAberration>(true);
                ca.intensity.Override(0.1f);
                foreach (var c in profile.components) AssetDatabase.AddObjectToAsset(c, profile);
                AssetDatabase.SaveAssets();
            }
            var go = new GameObject("Neon Post FX");
            var volume = go.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = profile;
        }

        // ------------------------------------------------------------------ arena

        static GameObject Tiled(Transform parent, string name, string sprite, Vector2 pos, Vector2 size, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            var sr = Sprite(go, sprite, order);
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = size;
            return go;
        }

        /// <summary>Retângulo de cor sólida (sprite de 1 pixel esticado).</summary>
        static GameObject Block(Transform parent, string name, Vector2 pos, Vector2 size, Color color, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = new Vector3(size.x / 0.125f, size.y / 0.125f, 1f);
            Sprite(go, "pixel", order).color = color;
            return go;
        }

        static void BuildArena()
        {
            var root = new GameObject("Arena").transform;
            Tiled(root, "Floor", "floor", Arena.center, Arena.size, SortOrder.Floor);

            // faixas de rua
            var decals = new GameObject("Road Markings").transform;
            decals.SetParent(root, false);
            foreach (var y in new[] { -3f, 3f })
                Tiled(decals, "Lane", "lane", new Vector2(0, y), new Vector2(Arena.width - 2, 0.125f), SortOrder.FloorDecal);
            foreach (var x in new[] { -9f, 9f })
            {
                var lane = Tiled(decals, "Lane", "lane", new Vector2(x, 0), new Vector2(Arena.height - 2, 0.125f), SortOrder.FloorDecal);
                lane.transform.rotation = Quaternion.Euler(0, 0, 90);
            }
            for (int i = 0; i < 6; i++) // faixa de pedestre
            {
                Block(decals, "Crosswalk", new Vector2(-1.5f + i * 0.6f, -6.5f), new Vector2(0.3f, 1.6f), new Color(1, 1, 1, 0.18f), SortOrder.FloorDecal);
            }

            // muros com prédios iluminados
            var walls = new GameObject("Walls").transform;
            walls.SetParent(root, false);
            float t = 1f;
            Wall(walls, new Vector2(Arena.center.x, Arena.yMax - t / 2), new Vector2(Arena.width, t));
            Wall(walls, new Vector2(Arena.center.x, Arena.yMin + t / 2), new Vector2(Arena.width, t));
            Wall(walls, new Vector2(Arena.xMin + t / 2, Arena.center.y), new Vector2(t, Arena.height - 2 * t));
            Wall(walls, new Vector2(Arena.xMax - t / 2, Arena.center.y), new Vector2(t, Arena.height - 2 * t));

            // obstáculos: carros e pilares
            var props = new GameObject("Obstacles").transform;
            props.SetParent(root, false);
            Car(props, "car_pink", new Vector2(-14, 8), 0);
            Car(props, "car_cyan", new Vector2(12, 9), 0);
            Car(props, "car_cyan", new Vector2(-10, -9), 0);
            Car(props, "car_pink", new Vector2(15, -7), 0);
            Car(props, "car_pink", new Vector2(3, 11), 90);
            Car(props, "car_cyan", new Vector2(-19, -2), 90);
            Car(props, "car_pink", new Vector2(19, 2), 90);
            Car(props, "car_cyan", new Vector2(-4, -12), 0);
            foreach (var p in new[] { new Vector2(-6, 6), new Vector2(6, 6), new Vector2(-6, -6), new Vector2(6, -6),
                                      new Vector2(-17, 11), new Vector2(17, -11), new Vector2(-17, -11), new Vector2(17, 11) })
                Pillar(props, p);

            // poças eletrificadas (perigo)
            var hazards = new GameObject("Electric Puddles").transform;
            hazards.SetParent(root, false);
            Puddle(hazards, new Vector2(0, 7.5f), 2);
            Puddle(hazards, new Vector2(-13, 0), 1);
            Puddle(hazards, new Vector2(13, 0), 1);
            Puddle(hazards, new Vector2(6, -10), 2);
            Puddle(hazards, new Vector2(-19, 10), 1);
            Puddle(hazards, new Vector2(20, -12), 1);
            Puddle(hazards, new Vector2(-11, 12), 1);
        }

        static void Wall(Transform parent, Vector2 pos, Vector2 size)
        {
            var go = Tiled(parent, "Wall", "wall", pos, size, -3000);
            go.AddComponent<BoxCollider2D>().size = size;
            Block(go.transform, "Neon Edge", Vector2.zero, size + new Vector2(0.15f, 0.15f), new Color(1f, 0.16f, 0.43f, 0.9f), -3001);
        }

        static void Car(Transform parent, string sprite, Vector2 pos, float angle)
        {
            var go = new GameObject("Car");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.rotation = Quaternion.Euler(0, 0, angle);
            Sprite(go, sprite);
            go.AddComponent<BoxCollider2D>().size = new Vector2(2.0f, 1.0f);
            go.AddComponent<YSort>().isStatic = true;
        }

        static void Pillar(Transform parent, Vector2 pos)
        {
            var go = new GameObject("Pillar");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            Sprite(go, "pillar");
            go.AddComponent<CircleCollider2D>().radius = 0.5f;
            go.AddComponent<YSort>().isStatic = true;
        }

        static void Puddle(Transform parent, Vector2 pos, int scale)
        {
            var go = new GameObject("Electric Puddle");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = Vector3.one * scale;
            Sprite(go, "puddle_0", SortOrder.Hazard);
            var col = go.AddComponent<CapsuleCollider2D>();
            col.direction = CapsuleDirection2D.Horizontal;
            col.size = new Vector2(2.1f, 1.1f);
            col.isTrigger = true;
            var p = go.AddComponent<ElectricPuddle>();
            p.frames = new[] { S("puddle_0"), S("puddle_1") };
        }

        // ------------------------------------------------------------------ build por linha de comando

        public static void BuildLinux()
        {
            Build();
            var report = BuildPipeline.BuildPlayer(new[] { ScenePath }, "Build/Linux/NeonSurvivor.x86_64",
                BuildTarget.StandaloneLinux64, BuildOptions.None);
            Debug.Log("Build result: " + report.summary.result);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) EditorApplication.Exit(1);
        }

        /// <summary>Build para navegador, pronto para o itch.io (zipar a pasta Build/WebGL).</summary>
        public static void BuildWebGL()
        {
            Build();
            // o itch.io não manda header Content-Encoding, então o loader descompacta sozinho
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.template = "APPLICATION:Default";
            var report = BuildPipeline.BuildPlayer(new[] { ScenePath }, "Build/WebGL",
                BuildTarget.WebGL, BuildOptions.None);
            Debug.Log("Build result: " + report.summary.result);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) EditorApplication.Exit(1);
        }
    }
}
