using UnityEngine;

namespace NeonSurvivor
{
    /// <summary>Torreta parada que mira e atira no jogador.</summary>
    public class TurretEnemy : Enemy
    {
        public static int Count { get; private set; }

        public Transform barrel;
        public float fireInterval = 2.2f;
        public float range = 11f;
        public float bulletSpeed = 5f;
        public float bulletDamage = 12f;
        float fireTimer = 1.5f;

        void Start() => Count++;
        void OnDestroy() => Count--;

        protected override void Update()
        {
            base.Update();
            if (!GameManager.IsPlaying) return;
            var to = ToPlayer;
            if (barrel != null) barrel.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(to.y, to.x) * Mathf.Rad2Deg);
            fireTimer -= Time.deltaTime;
            if (fireTimer <= 0 && to.sqrMagnitude < range * range)
            {
                fireTimer = fireInterval;
                var dir = to.normalized;
                Projectile.Spawn("bullet_enemy", (Vector2)transform.position + dir * 0.8f, dir * bulletSpeed, bulletDamage, true, 4f);
                AudioManager.Play("enemy_shoot", 0.5f);
            }
        }

        protected override Vector2 Steer() => Vector2.zero;
    }
}
