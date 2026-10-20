using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace NeonSurvivor
{
    /// <summary>
    /// Teste automático: rode o build com "-autotest [-god] [-shots pasta]".
    /// Um bot anda pela arena, escolhe upgrades, tira screenshots e registra estatísticas no log.
    /// </summary>
    public class AutoTest : MonoBehaviour
    {
        float[] shotTimes = { 2, 20, 47, 75, 95, 123, 140, 165 };
        string dir;
        int nextShot;
        int chooseAtFrame = -1;
        int frames;
        float lastLog, endAt = -1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var args = Environment.GetCommandLineArgs();
            if (!args.Contains("-autotest")) return;
            var go = new GameObject("AutoTest");
            var t = go.AddComponent<AutoTest>();
            int i = Array.IndexOf(args, "-shots");
            t.dir = i >= 0 && i + 1 < args.Length ? args[i + 1] : Application.persistentDataPath;
            Directory.CreateDirectory(t.dir);
            PlayerController.God = args.Contains("-god");
            int st = Array.IndexOf(args, "-shottimes");
            if (st >= 0 && st + 1 < args.Length)
                t.shotTimes = args[st + 1].Split(',').Select(s => float.Parse(s, System.Globalization.CultureInfo.InvariantCulture)).ToArray();
        }

        void Update()
        {
            var gm = GameManager.I;
            if (gm == null) return;
            // o screenshot só sai no fim do frame, então espera alguns frames antes de fechar a tela
            if (gm.State == GameState.Title)
            {
                frames++;
                if (frames == 30) Shot("00_title");
                if (frames > 40) gm.StartRun();
            }
            if (gm.State == GameState.LevelUp && gm.hud.LevelUpOpen)
            {
                if (chooseAtFrame < 0)
                {
                    chooseAtFrame = Time.frameCount + 3;
                    if (gm.player.Level >= 3) Shot("levelup");
                }
                else if (Time.frameCount >= chooseAtFrame)
                {
                    chooseAtFrame = -1;
                    gm.hud.ChooseOption(UnityEngine.Random.Range(0, 3));
                }
            }
            if (gm.State == GameState.Playing)
            {
                Time.timeScale = 2f;
                float t = gm.waves.Elapsed;
                // anda em círculo largo pela arena
                var p = (Vector2)PlayerController.I.transform.position;
                var center = new Vector2(Mathf.Cos(t * 0.15f), Mathf.Sin(t * 0.15f)) * 9f;
                var to = center - p;
                var tangent = new Vector2(-to.y, to.x).normalized;
                GameInput.BotMove = Vector2.ClampMagnitude(to * 0.4f + tangent * 0.6f, 1f);

                if (nextShot < shotTimes.Length && t >= shotTimes[nextShot])
                    Shot($"{++nextShot:00}_t{Mathf.FloorToInt(t):000}");
                if (t - lastLog >= 10f)
                {
                    lastLog = t;
                    var pl = PlayerController.I;
                    Debug.Log($"[AutoTest] t={t:0} hp={pl.Health:0}/{pl.maxHealth:0} dmgTaken={pl.DamageTaken:0} " +
                              $"lvl={pl.Level} kills={gm.Kills} score={gm.Score} enemies={Enemy.All.Count} " +
                              $"weapons={string.Join(",", pl.GetComponents<Weapon>().Select(w => w.Level))} fps={1f / Time.unscaledDeltaTime:0}");
                }
            }
            if ((gm.State == GameState.GameOver || gm.State == GameState.Victory) && endAt < 0)
            {
                endAt = Time.unscaledTime;
                Debug.Log($"[AutoTest] END state={gm.State} t={gm.waves.Elapsed:0} score={gm.Score} kills={gm.Kills} dmgTaken={PlayerController.I.DamageTaken:0}");
            }
            if (endAt > 0 && Time.unscaledTime - endAt > 1.5f)
            {
                Shot("99_end");
                if (Time.unscaledTime - endAt > 2.5f) Application.Quit();
            }
        }

        void Shot(string name)
        {
            var path = Path.Combine(dir, name + ".png");
            if (File.Exists(path)) return;
            ScreenCapture.CaptureScreenshot(path);
        }
    }
}
