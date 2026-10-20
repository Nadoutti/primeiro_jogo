using System.Collections.Generic;
using UnityEngine;

namespace NeonSurvivor
{
    /// <summary>Lâminas monofio que orbitam o jogador cortando quem encostar.</summary>
    public class OrbitBlades : Weapon
    {
        static readonly int[] Count = { 0, 2, 3, 4 };
        static readonly float[] BladeDamage = { 0, 9f, 13f, 18f };
        static readonly float[] Radius = { 0, 1.4f, 1.55f, 1.8f };
        public float degreesPerSecond = 220f;

        readonly List<Transform> blades = new();
        float angle;

        public override string DisplayName => "Lâmina Monofio";
        public override string IconName => "icon_blade";

        public override string DescribeNext() => Level switch
        {
            0 => "2 lâminas giram ao seu redor.",
            1 => "+1 lâmina, mais dano.",
            _ => "+1 lâmina, mais dano e alcance.",
        };

        protected override void OnLevelChanged()
        {
            while (blades.Count < Count[Level]) blades.Add(CreateBlade());
        }

        Transform CreateBlade()
        {
            var go = new GameObject("Blade");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = Art.Get("blade");
            sr.sortingOrder = SortOrder.Projectile - 1;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.35f;
            go.AddComponent<BladeHitbox>().owner = this;
            return go.transform;
        }

        public float CurrentDamage => Damage(BladeDamage[Level]);

        void Update()
        {
            if (Level == 0) return;
            if (GameManager.IsPlaying) angle += degreesPerSecond * Time.deltaTime;
            for (int i = 0; i < blades.Count; i++)
            {
                float a = (angle + i * 360f / blades.Count) * Mathf.Deg2Rad;
                blades[i].position = transform.position + new Vector3(Mathf.Cos(a), Mathf.Sin(a)) * Radius[Level];
                blades[i].rotation = Quaternion.Euler(0, 0, angle * 3f);
            }
        }
    }

    /// <summary>Hitbox de uma lâmina: acerta cada inimigo no máximo a cada 0,35 s.</summary>
    public class BladeHitbox : MonoBehaviour
    {
        public OrbitBlades owner;
        readonly Dictionary<Enemy, float> nextHit = new();

        void OnTriggerStay2D(Collider2D other)
        {
            if (!GameManager.IsPlaying || !other.TryGetComponent(out Enemy e) || e.IsDead) return;
            if (nextHit.TryGetValue(e, out var t) && Time.time < t) return;
            nextHit[e] = Time.time + 0.35f;
            var push = ((Vector2)(e.transform.position - owner.transform.position)).normalized * 4f;
            e.TakeDamage(owner.CurrentDamage, push);
            AudioManager.Play("blade", 0.5f, 0.2f);
            if (nextHit.Count > 64) nextHit.Clear();
        }
    }
}
