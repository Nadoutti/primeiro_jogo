using Platformer.Gameplay;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics
{
    /// <summary>
    /// A simple projectile fired by the player. Moves in a straight line, kills
    /// the first enemy it touches and is destroyed when it hits anything solid.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class Bullet : MonoBehaviour
    {
        /// <summary>
        /// Horizontal speed of the bullet.
        /// </summary>
        public float speed = 15;

        /// <summary>
        /// Seconds before the bullet is destroyed if it hits nothing.
        /// </summary>
        public float lifetime = 2;

        /// <summary>
        /// Fire the bullet. Direction should be 1 (right) or -1 (left).
        /// </summary>
        public void Launch(float direction)
        {
            GetComponent<Rigidbody2D>().linearVelocity = new Vector2(direction * speed, 0);
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null) spriteRenderer.flipX = direction < 0;
            Destroy(gameObject, lifetime);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            //ignore the shooter and other triggers (tokens, zones).
            if (other.isTrigger || other.GetComponent<PlayerController>() != null)
                return;

            var enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
                Schedule<EnemyHit>().enemy = enemy;

            Destroy(gameObject);
        }
    }
}
