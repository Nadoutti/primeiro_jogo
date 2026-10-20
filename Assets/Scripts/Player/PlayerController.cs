using UnityEngine;

namespace NeonSurvivor
{
    /// <summary>Movimento, dash com stamina, vida e experiência do jogador.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController I { get; private set; }
        /// <summary>Modo de teste: registra o dano mas não morre.</summary>
        public static bool God;
        public float DamageTaken { get; private set; }

        [Header("Movimento")]
        public float moveSpeed = 4.5f;
        public float dashSpeed = 15f;
        public float dashDuration = 0.18f;

        [Header("Atributos")]
        public float maxHealth = 120f;
        public float healthRegen = 0.6f;
        public float maxStamina = 100f;
        public float staminaRegen = 22f;
        public float dashCost = 35f;
        public float magnetRadius = 1.8f;
        public float damageMultiplier = 1f;
        public float hurtInvulnerability = 0.8f;

        public Sprite[] walkFrames;

        public float Health { get; private set; }
        public float Stamina { get; private set; }
        public int Level { get; private set; } = 1;
        public int Xp { get; private set; }
        public int XpToNext { get; private set; }
        public int PendingLevelUps { get; set; }
        public Vector2 Facing { get; private set; } = Vector2.right;
        public bool IsDashing => dashTimer > 0;
        public bool IsDead { get; private set; }

        Rigidbody2D rb;
        SpriteRenderer sr;
        Vector2 input, dashDir;
        float dashTimer, invulnerable, animT, ghostTimer;

        void Awake()
        {
            I = this;
            rb = GetComponent<Rigidbody2D>();
            sr = GetComponent<SpriteRenderer>();
            Health = maxHealth;
            Stamina = maxStamina;
            XpToNext = XpFor(Level);
        }

        static int XpFor(int level) => 4 + level * 3;

        void Update()
        {
            if (!GameManager.IsPlaying) { input = Vector2.zero; return; }

            input = GameInput.Move();
            if (input.sqrMagnitude > 0.01f) Facing = input.normalized;

            Heal(healthRegen * Time.deltaTime);
            if (!IsDashing) Stamina = Mathf.Min(maxStamina, Stamina + staminaRegen * Time.deltaTime);
            if (GameInput.DashPressed() && !IsDashing && Stamina >= dashCost)
            {
                Stamina -= dashCost;
                dashTimer = dashDuration;
                dashDir = Facing;
                invulnerable = Mathf.Max(invulnerable, dashDuration + 0.1f);
                AudioManager.Play("dash", 0.7f);
            }

            if (IsDashing)
            {
                dashTimer -= Time.deltaTime;
                ghostTimer -= Time.deltaTime;
                if (ghostTimer <= 0)
                {
                    ghostTimer = 0.03f;
                    var ghost = Fader.Spawn(sr.sprite, transform.position, new Color(0.02f, 0.85f, 0.91f, 0.6f), 0.25f);
                    ghost.GetComponent<SpriteRenderer>().flipX = sr.flipX;
                    ghost.GetComponent<SpriteRenderer>().sortingOrder = sr.sortingOrder - 1;
                }
            }

            // animação de caminhada + piscar quando invulnerável
            if (input.sqrMagnitude > 0.01f) animT += Time.deltaTime * 8f;
            if (walkFrames != null && walkFrames.Length > 0) sr.sprite = walkFrames[(int)animT % walkFrames.Length];
            if (Mathf.Abs(input.x) > 0.1f) sr.flipX = input.x < 0;
            invulnerable -= Time.deltaTime;
            bool blink = invulnerable > 0 && !IsDashing && Mathf.Repeat(Time.time * 16f, 1f) < 0.5f;
            sr.color = blink ? new Color(1f, 1f, 1f, 0.35f) : Color.white;
        }

        void FixedUpdate()
        {
            rb.linearVelocity = IsDashing ? dashDir * dashSpeed : input * moveSpeed;
        }

        public void TakeDamage(float amount)
        {
            if (IsDead || invulnerable > 0 || !GameManager.IsPlaying) return;
            DamageTaken += amount;
            if (!God) Health -= amount;
            invulnerable = hurtInvulnerability;
            AudioManager.Play("player_hurt", 0.9f);
            CameraFollow.Shake(0.15f, 0.2f);
            if (!God) GameManager.I.hud.FlashDamage();
            Fader.Burst(transform.position, Art.Red, 6, 4f);
            if (Health <= 0)
            {
                Health = 0;
                IsDead = true;
                Fader.Burst(transform.position, Art.Cyan, 40, 8f);
                sr.enabled = false;
                GameManager.I.GameOver();
            }
        }

        public void Heal(float amount) => Health = Mathf.Min(maxHealth, Health + amount);

        public void AddMaxHealth(float amount)
        {
            maxHealth += amount;
            Heal(amount);
        }

        public void AddXp(int amount)
        {
            Xp += amount;
            while (Xp >= XpToNext)
            {
                Xp -= XpToNext;
                Level++;
                XpToNext = XpFor(Level);
                PendingLevelUps++;
            }
            if (PendingLevelUps > 0) GameManager.I.OpenLevelUp();
        }
    }
}
