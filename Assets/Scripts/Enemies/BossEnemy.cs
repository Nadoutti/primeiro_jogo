using UnityEngine;

namespace NeonSurvivor
{
    /// <summary>Mech de segurança: persegue o jogador e alterna entre rajada em leque e anel de tiros.</summary>
    public class BossEnemy : Enemy
    {
        public float attackInterval = 2.6f;
        public float bulletSpeed = 5.5f;
        public float bulletDamage = 14f;
        float attackTimer = 2f;
        int pattern;

        public float HpFraction => Mathf.Clamp01(hp / maxHp);

        protected override void Update()
        {
            base.Update();
            if (!GameManager.IsPlaying) return;
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0)
            {
                attackTimer = HpFraction < 0.5f ? attackInterval * 0.7f : attackInterval;
                if (pattern++ % 2 == 0) Fan(); else Ring();
                AudioManager.Play("enemy_shoot", 0.9f);
            }
        }

        void Fan()
        {
            float baseAngle = Mathf.Atan2(ToPlayer.y, ToPlayer.x);
            for (int i = -2; i <= 2; i++) Shoot(baseAngle + i * 0.22f);
        }

        void Ring()
        {
            int n = HpFraction < 0.5f ? 16 : 10;
            float offset = Random.value * Mathf.PI;
            for (int i = 0; i < n; i++) Shoot(offset + i * Mathf.PI * 2f / n);
        }

        void Shoot(float angle)
        {
            var dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Projectile.Spawn("bullet_enemy", (Vector2)transform.position + dir * 1.2f, dir * bulletSpeed, bulletDamage, true, 5f);
        }

        protected override Vector2 Steer() => ToPlayer.normalized * speed;

        public override void Init(float hpMultiplier)
        {
            base.Init(hpMultiplier);
            maxHp = hp;
        }

        public override void TakeDamage(float amount, Vector2 knockback)
        {
            // boss quase não sofre empurrão
            base.TakeDamage(amount, knockback * 0.1f);
        }

        protected override void Die(bool killedByPlayer)
        {
            if (dead) return;
            if (killedByPlayer)
            {
                GameManager.I.OnBossKilled();
                CameraFollow.Shake(0.35f, 0.6f);
                for (int i = 0; i < 12; i++)
                    Pickup.SpawnXp((Vector2)transform.position + Random.insideUnitCircle * 1.5f, 5);
                Fader.Burst(transform.position, Art.Yellow, 40, 9f);
                Fader.Burst(transform.position, Art.Pink, 30, 7f);
            }
            base.Die(killedByPlayer);
        }
    }
}
