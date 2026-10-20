using System.Collections.Generic;
using UnityEngine;

namespace NeonSurvivor
{
    /// <summary>Base dos inimigos: vida, dano de contato, flash ao levar dano, drop de XP.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class Enemy : MonoBehaviour
    {
        public static readonly List<Enemy> All = new();

        public float maxHp = 20f;
        public float contactDamage = 10f;
        public float speed = 2f;
        public int xp = 1;
        public int score = 10;
        public Color burstColor = Color.white;

        protected Rigidbody2D rb;
        protected SpriteRenderer sr;
        protected float hp;
        protected bool dead;
        float flash;
        Vector2 knock;

        public float Hp => hp;
        public bool IsDead => dead;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            sr = GetComponent<SpriteRenderer>();
        }

        public virtual void Init(float hpMultiplier) => hp = maxHp * hpMultiplier;

        void OnEnable() => All.Add(this);
        void OnDisable() => All.Remove(this);

        protected Vector2 ToPlayer => PlayerController.I.transform.position - transform.position;

        protected virtual void Update()
        {
            if (flash > 0)
            {
                flash -= Time.deltaTime;
                sr.color = flash > 0 ? new Color(4f, 4f, 4f, 1f) : Color.white;
            }
        }

        protected virtual void FixedUpdate()
        {
            if (!GameManager.IsPlaying) { rb.linearVelocity = Vector2.zero; return; }
            rb.linearVelocity = Steer() + knock;
            knock = Vector2.MoveTowards(knock, Vector2.zero, 30f * Time.fixedDeltaTime);
        }

        /// <summary>Velocidade desejada do inimigo (comportamento).</summary>
        protected abstract Vector2 Steer();

        public virtual void TakeDamage(float amount, Vector2 knockback)
        {
            if (dead) return;
            hp -= amount;
            flash = 0.08f;
            knock += knockback;
            AudioManager.Play("hit", 0.5f);
            if (hp <= 0) Die(true);
        }

        protected virtual void Die(bool killedByPlayer)
        {
            if (dead) return;
            dead = true;
            if (killedByPlayer)
            {
                GameManager.I.AddKill(score);
                Pickup.SpawnXp(transform.position, xp);
                if (Random.value < 0.012f) Pickup.SpawnHealth(transform.position);
                AudioManager.Play("enemy_die", 0.6f);
                Fader.Burst(transform.position, burstColor, 8);
            }
            Destroy(gameObject);
        }

        protected virtual void OnCollisionStay2D(Collision2D c)
        {
            if (c.collider.TryGetComponent(out PlayerController p)) p.TakeDamage(contactDamage);
        }
    }
}
