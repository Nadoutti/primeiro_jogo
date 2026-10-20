using UnityEngine;

namespace NeonSurvivor
{
    /// <summary>Chip de dados (XP) ou kit médico; é puxado pelo ímã do jogador.</summary>
    public class Pickup : MonoBehaviour
    {
        public enum Kind { Xp, Health }

        public Kind kind;
        public int value = 1;
        bool magnetized;
        float speed;
        float bob;

        public static void SpawnXp(Vector2 pos, int value)
        {
            Spawn(value >= 3 ? "xp_big" : "xp_small", pos, Kind.Xp, value);
        }

        public static void SpawnHealth(Vector2 pos) => Spawn("health_pack", pos, Kind.Health, 25);

        static void Spawn(string sprite, Vector2 pos, Kind kind, int value)
        {
            var go = new GameObject(kind == Kind.Xp ? "XpChip" : "HealthPack");
            go.transform.position = pos + Random.insideUnitCircle * 0.2f;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = Art.Get(sprite);
            sr.sortingOrder = SortOrder.Pickup;
            var p = go.AddComponent<Pickup>();
            p.kind = kind;
            p.value = value;
            p.bob = Random.value * 6f;
        }

        void Update()
        {
            var player = PlayerController.I;
            if (player == null || !GameManager.IsPlaying) return;
            var to = (Vector2)(player.transform.position - transform.position);
            float dist = to.magnitude;
            if (!magnetized && dist < player.magnetRadius) magnetized = true;
            if (magnetized)
            {
                speed += 30f * Time.deltaTime;
                transform.position += (Vector3)(to.normalized * Mathf.Min(speed * Time.deltaTime, dist));
            }
            else
            {
                bob += Time.deltaTime * 4f;
                transform.localScale = Vector3.one * (1f + Mathf.Sin(bob) * 0.08f);
            }
            if (dist < 0.4f) Collect(player);
        }

        void Collect(PlayerController player)
        {
            if (kind == Kind.Xp)
            {
                player.AddXp(value);
                GameManager.I.AddScore(value);
                AudioManager.Play("xp", 0.4f, 0.15f);
            }
            else
            {
                player.Heal(value);
                AudioManager.Play("pickup_health", 0.8f, 0f);
                Fader.Burst(transform.position, Color.white, 8, 3f);
            }
            Destroy(gameObject);
        }
    }
}
