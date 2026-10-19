using UnityEngine;
using UnityEngine.InputSystem;

namespace Platformer.Mechanics
{
    /// <summary>
    /// Lets the player fire bullets in the direction they are facing.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerShooter : MonoBehaviour
    {
        public Bullet bulletPrefab;
        public AudioClip shootAudio;

        /// <summary>
        /// Where the bullet spawns, relative to the player when facing right.
        /// </summary>
        public Vector2 muzzleOffset = new Vector2(0.25f, -0.1f);

        /// <summary>
        /// Minimum seconds between shots.
        /// </summary>
        public float fireCooldown = 0.2f;

        PlayerController player;
        SpriteRenderer spriteRenderer;
        float nextFireTime;

        private InputAction m_FireAction;

        void Awake()
        {
            player = GetComponent<PlayerController>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            m_FireAction = InputSystem.actions.FindAction("Player/Fire");
            m_FireAction.Enable();
        }

        void Update()
        {
            if (!player.controlEnabled || bulletPrefab == null) return;

            if (m_FireAction.WasPressedThisFrame() && Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + fireCooldown;
                Fire();
            }
        }

        void Fire()
        {
            var direction = spriteRenderer.flipX ? -1f : 1f;
            var offset = new Vector3(muzzleOffset.x * direction, muzzleOffset.y, 0);
            var bullet = Instantiate(bulletPrefab, transform.position + offset, Quaternion.identity);
            bullet.Launch(direction);

            if (player.audioSource && shootAudio)
                player.audioSource.PlayOneShot(shootAudio);
        }
    }
}
