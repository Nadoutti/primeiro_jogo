using UnityEngine;

namespace NeonSurvivor
{
    /// <summary>Monta os inimigos em código (sem prefabs).</summary>
    public static class EnemyFactory
    {
        static GameObject Body(string name, Sprite sprite, Vector2 pos, float mass)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.mass = mass;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            return go;
        }

        static void SpawnFx(Vector2 pos, Color color)
        {
            Fader.Spawn(Art.Get("emp_ring"), pos, color, 0.35f, default, 0.4f, 0.05f);
        }

        public static Enemy Drone(Vector2 pos, float hpMul)
        {
            var go = Body("Drone", Art.Get("drone_0"), pos, 0.6f);
            go.AddComponent<CircleCollider2D>().radius = 0.35f;
            var e = go.AddComponent<DroneEnemy>();
            e.frames = new[] { Art.Get("drone_0"), Art.Get("drone_1") };
            e.maxHp = 14f; e.speed = Random.Range(1.9f, 2.5f); e.contactDamage = 6f;
            e.xp = 1; e.score = 10; e.burstColor = Art.Pink;
            e.Init(hpMul);
            go.AddComponent<YSort>();
            return e;
        }

        public static Enemy Biker(Vector2 pos, Vector2 dir, float hpMul, float delay)
        {
            var go = Body("Biker", Art.Get("biker"), pos, 2f);
            go.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.2f, 0.4f);
            col.isTrigger = true;
            var e = go.AddComponent<BikerEnemy>();
            e.direction = dir;
            e.delay = delay;
            e.arena = GameManager.I.waves.arena;
            e.maxHp = 22f; e.speed = 9f; e.contactDamage = 18f;
            e.xp = 3; e.score = 25; e.burstColor = Art.Yellow;
            e.Init(hpMul);
            go.AddComponent<YSort>();
            return e;
        }

        public static Enemy Turret(Vector2 pos, float hpMul)
        {
            var go = Body("Turret", Art.Get("turret_base"), pos, 50f);
            go.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
            go.AddComponent<BoxCollider2D>().size = new Vector2(0.9f, 0.9f);
            var barrel = new GameObject("Barrel");
            barrel.transform.SetParent(go.transform, false);
            var bsr = barrel.AddComponent<SpriteRenderer>();
            bsr.sprite = Art.Get("turret_barrel");
            bsr.sortingOrder = 1;
            var e = go.AddComponent<TurretEnemy>();
            e.barrel = barrel.transform;
            e.maxHp = 55f; e.contactDamage = 10f;
            e.xp = 5; e.score = 50; e.burstColor = Art.Red;
            e.Init(hpMul);
            go.AddComponent<YSort>();
            SpawnFx(pos, Art.Red);
            return e;
        }

        public static BossEnemy Boss(Vector2 pos, float hpMul)
        {
            var go = Body("Boss Mech", Art.Get("boss"), pos, 40f);
            go.transform.localScale = Vector3.one * 2f;
            go.AddComponent<BoxCollider2D>().size = new Vector2(1.4f, 1.5f);
            var e = go.AddComponent<BossEnemy>();
            e.maxHp = 1600f; e.speed = 1.5f; e.contactDamage = 22f;
            e.xp = 10; e.score = 0; e.burstColor = Art.Yellow;
            e.Init(hpMul);
            go.AddComponent<YSort>();
            SpawnFx(pos, Art.Yellow);
            return e;
        }
    }
}
