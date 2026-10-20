using System.Collections.Generic;
using UnityEngine;

namespace NeonSurvivor
{
    /// <summary>Projétil do jogador (acerta inimigos) ou hostil (acerta o jogador).</summary>
    public class Projectile : MonoBehaviour
    {
        public static readonly List<Projectile> Hostile = new();

        public bool hostile;
        public float damage;
        public int pierce;
        public float knockback = 2f;
        float life;
        Vector2 velocity;

        public static Projectile Spawn(string sprite, Vector2 pos, Vector2 velocity, float damage, bool hostile, float life, int pierce = 0)
        {
            var go = new GameObject(hostile ? "EnemyBullet" : "Plasma");
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = Art.Get(sprite);
            sr.sortingOrder = SortOrder.Projectile;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = velocity;
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.2f;
            var p = go.AddComponent<Projectile>();
            p.hostile = hostile;
            p.damage = damage;
            p.pierce = pierce;
            p.life = life;
            p.velocity = velocity;
            return p;
        }

        void Start() { if (hostile) Hostile.Add(this); }
        void OnDisable() => Hostile.Remove(this);

        void Update()
        {
            life -= Time.deltaTime;
            if (life <= 0) Destroy(gameObject);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (hostile)
            {
                if (other.TryGetComponent(out PlayerController player))
                {
                    player.TakeDamage(damage);
                    Pop();
                }
                return;
            }
            if (other.TryGetComponent(out Enemy enemy) && !enemy.IsDead)
            {
                enemy.TakeDamage(damage, velocity.normalized * knockback);
                if (--pierce < 0) Pop();
            }
        }

        public void Pop()
        {
            Fader.Burst(transform.position, hostile ? Art.Pink : Art.Cyan, 3, 3f);
            Destroy(gameObject);
        }
    }
}
