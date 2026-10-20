using UnityEngine;

namespace NeonSurvivor
{
    /// <summary>Pulso EMP: onda periódica em área que causa dano, empurra inimigos e apaga tiros inimigos.</summary>
    public class EmpPulse : Weapon
    {
        static readonly float[] Interval = { 0, 4f, 3.2f, 2.4f };
        static readonly float[] Radius = { 0, 2.6f, 3.3f, 4.2f };
        static readonly float[] PulseDamage = { 0, 16f, 22f, 30f };
        public float knockback = 9f;
        float timer = 1f;

        public override string DisplayName => "Pulso EMP";
        public override string IconName => "icon_emp";

        public override string DescribeNext() => Level switch
        {
            0 => "Onda em área que empurra inimigos e apaga tiros.",
            1 => "Raio maior, mais dano e frequência.",
            _ => "Raio enorme, dano alto, pulso a cada 2,4 s.",
        };

        void Update()
        {
            if (!Ready) return;
            timer -= Time.deltaTime;
            if (timer > 0) return;
            timer = Interval[Level];
            Pulse();
        }

        void Pulse()
        {
            Vector2 center = transform.position;
            float r = Radius[Level];
            float dmg = Damage(PulseDamage[Level]);
            // copia a lista porque inimigos podem morrer durante o laço
            foreach (var e in Enemy.All.ToArray())
            {
                if (e == null || e.IsDead) continue;
                var to = (Vector2)e.transform.position - center;
                if (to.sqrMagnitude <= r * r) e.TakeDamage(dmg, to.normalized * knockback);
            }
            foreach (var b in Projectile.Hostile.ToArray())
                if (b != null && ((Vector2)b.transform.position - center).sqrMagnitude <= r * r) b.Pop();

            // anel visual: o sprite tem 6 unidades de diâmetro
            Fader.Spawn(Art.Get("emp_ring"), center, new Color(0.3f, 1.5f, 2f, 0.9f), 0.35f, default, 0.2f, r / 3f);
            AudioManager.Play("emp", 0.7f, 0.05f);
            CameraFollow.Shake(0.06f, 0.12f);
        }
    }
}
