using UnityEngine;

namespace NeonSurvivor
{
    /// <summary>Pistola de plasma: atira sozinha no inimigo mais próximo.</summary>
    public class PlasmaGun : Weapon
    {
        static readonly float[] Cooldown = { 0, 0.55f, 0.42f, 0.32f };
        static readonly int[] Shots = { 0, 1, 2, 3 };
        static readonly int[] Pierce = { 0, 0, 0, 1 };
        public float baseDamage = 10f;
        public float range = 9f;
        public float bulletSpeed = 12f;
        float timer;

        public override string DisplayName => "Pistola de Plasma";
        public override string IconName => "icon_pistol";

        public override string DescribeNext() => Level switch
        {
            0 => "Atira no inimigo mais próximo.",
            1 => "+1 projétil, dispara mais rápido.",
            _ => "+1 projétil, mais cadência e perfura inimigos.",
        };

        void Update()
        {
            if (!Ready) return;
            timer -= Time.deltaTime;
            if (timer > 0) return;
            var target = Nearest(transform.position, range);
            if (target == null) return;
            timer = Cooldown[Level];

            var dir = ((Vector2)(target.transform.position - transform.position)).normalized;
            float baseAngle = Mathf.Atan2(dir.y, dir.x);
            int n = Shots[Level];
            for (int i = 0; i < n; i++)
            {
                float a = baseAngle + (i - (n - 1) / 2f) * 0.16f;
                var v = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * bulletSpeed;
                Projectile.Spawn("bullet_plasma", (Vector2)transform.position + v.normalized * 0.4f, v,
                    Damage(baseDamage), false, 1.1f, Pierce[Level]);
            }
            AudioManager.Play("shoot", 0.35f, 0.1f);
        }
    }
}
