using UnityEngine;

namespace NeonSurvivor
{
    /// <summary>
    /// Cronômetro da partida e linha do tempo da dificuldade:
    /// drones desde o início, motos a partir de 0:45, torretas a partir de 1:30,
    /// boss aos 2:00 e extração (vitória) aos 3:00.
    /// </summary>
    public class WaveDirector : MonoBehaviour
    {
        public float runDuration = 180f;
        public float bikersAt = 45f;
        public float turretsAt = 90f;
        public float bossAt = 120f;
        public int maxEnemies = 260;
        public Rect arena = new(-24, -15, 48, 30);

        public float Elapsed { get; private set; }
        public float Remaining => Mathf.Max(0, runDuration - Elapsed);
        public BossEnemy Boss { get; private set; }

        float droneTimer, bikerTimer = 2f, turretTimer, healthTimer = 30f;
        bool bikersAnnounced, turretsAnnounced, bossSpawned, extractionAnnounced;

        /// <summary>Multiplicador de vida dos inimigos: cresce com o tempo.</summary>
        public float HpMultiplier => 1f + Elapsed / 80f;

        void Update()
        {
            if (!GameManager.IsPlaying) return;
            Elapsed += Time.deltaTime;
            float t01 = Mathf.Clamp01(Elapsed / runDuration);
            var hud = GameManager.I.hud;

            droneTimer -= Time.deltaTime;
            if (droneTimer <= 0)
            {
                droneTimer = Mathf.Lerp(1.0f, 0.16f, t01);
                int batch = 1 + (Elapsed > 60 ? 1 : 0) + (Elapsed > 140 ? 1 : 0);
                for (int i = 0; i < batch; i++) TrySpawn(p => EnemyFactory.Drone(p, HpMultiplier));
            }

            if (Elapsed >= bikersAt)
            {
                if (!bikersAnnounced) { bikersAnnounced = true; hud.Banner("GANGUE DE MOTOS NA ÁREA!", Art.Pink); }
                bikerTimer -= Time.deltaTime;
                if (bikerTimer <= 0)
                {
                    bikerTimer = Mathf.Lerp(7f, 3.5f, t01);
                    SpawnBikerSquad();
                }
            }

            if (Elapsed >= turretsAt)
            {
                if (!turretsAnnounced) { turretsAnnounced = true; hud.Banner("TORRETAS DE SEGURANÇA ONLINE", Art.Red); }
                turretTimer -= Time.deltaTime;
                if (turretTimer <= 0)
                {
                    turretTimer = 9f;
                    if (TurretEnemy.Count < 5) TrySpawn(p => EnemyFactory.Turret(p, HpMultiplier), 5f, 9f);
                }
            }

            if (!bossSpawned && Elapsed >= bossAt)
            {
                bossSpawned = true;
                hud.Banner("ALERTA: MECH DE SEGURANÇA!", Art.Red);
                AudioManager.Play("boss_alert", 1f, 0f);
                CameraFollow.Shake(0.2f, 0.6f);
                var pos = SpawnPoint(10f, 12f);
                Boss = EnemyFactory.Boss(pos, 1f);
            }

            healthTimer -= Time.deltaTime;
            if (healthTimer <= 0)
            {
                healthTimer = 25f;
                Pickup.SpawnHealth(SpawnPoint(3f, 7f));
            }

            if (!extractionAnnounced && Remaining <= 15f)
            {
                extractionAnnounced = true;
                hud.Banner("EXTRAÇÃO EM 15 SEGUNDOS — AGUENTE!", Art.Cyan);
            }

            if (Elapsed >= runDuration) GameManager.I.Victory();
        }

        void TrySpawn(System.Func<Vector2, Enemy> spawn, float minR = 11.5f, float maxR = 14f)
        {
            if (Enemy.All.Count >= maxEnemies) return;
            spawn(SpawnPoint(minR, maxR));
        }

        /// <summary>Ponto aleatório num anel ao redor do jogador, dentro da arena.</summary>
        public Vector2 SpawnPoint(float minR, float maxR)
        {
            var center = (Vector2)PlayerController.I.transform.position;
            var p = center;
            for (int i = 0; i < 8; i++)
            {
                p = center + Random.insideUnitCircle.normalized * Random.Range(minR, maxR);
                p.x = Mathf.Clamp(p.x, arena.xMin + 1.5f, arena.xMax - 1.5f);
                p.y = Mathf.Clamp(p.y, arena.yMin + 1.5f, arena.yMax - 1.5f);
                if ((p - center).sqrMagnitude > minR * minR * 0.4f) break;
            }
            return p;
        }

        /// <summary>Três motos em fila cruzando a arena numa direção fixa, com aviso antes.</summary>
        void SpawnBikerSquad()
        {
            var player = (Vector2)PlayerController.I.transform.position;
            bool horizontal = Random.value < 0.5f;
            float sign = Random.value < 0.5f ? 1f : -1f;
            Vector2 dir = horizontal ? new Vector2(sign, 0) : new Vector2(0, sign);
            Vector2 lane = horizontal ? new Vector2(0, player.y) : new Vector2(player.x, 0);
            Vector2 start = horizontal
                ? new Vector2(sign > 0 ? arena.xMin - 1 : arena.xMax + 1, lane.y)
                : new Vector2(lane.x, sign > 0 ? arena.yMin - 1 : arena.yMax + 1);
            BikerEnemy.Warn(lane, horizontal, arena);
            for (int i = 0; i < 3; i++)
            {
                var offset = horizontal ? new Vector2(0, (i - 1) * 1.1f) : new Vector2((i - 1) * 1.1f, 0);
                EnemyFactory.Biker(start + offset - dir * (i * 1.5f), dir, HpMultiplier, 1.0f);
            }
        }
    }
}
