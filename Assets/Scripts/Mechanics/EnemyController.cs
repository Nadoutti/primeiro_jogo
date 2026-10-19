using System.Collections;
using System.Collections.Generic;
using Platformer.Gameplay;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics
{
    /// <summary>
    /// A simple controller for enemies. Provides movement control over a patrol path.
    /// </summary>
    [RequireComponent(typeof(AnimationController), typeof(Collider2D))]
    public class EnemyController : MonoBehaviour
    {
        public PatrolPath path;
        public AudioClip ouch;

        internal PatrolPath.Mover mover;
        internal AnimationController control;
        internal Collider2D _collider;
        internal AudioSource _audio;
        internal Health health;
        SpriteRenderer spriteRenderer;
        Animator animator;
        Vector3 spawnPosition;

        public Bounds Bounds => _collider.bounds;

        void Awake()
        {
            control = GetComponent<AnimationController>();
            _collider = GetComponent<Collider2D>();
            _audio = GetComponent<AudioSource>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            health = GetComponent<Health>();
            animator = GetComponent<Animator>();
            spawnPosition = transform.position;
        }

        /// <summary>
        /// Is this enemy dead? Dead enemies have their collider and movement disabled.
        /// </summary>
        public bool IsDead => !_collider.enabled;

        /// <summary>
        /// Bring the enemy back to life at its starting position, with full health.
        /// </summary>
        public void Respawn()
        {
            if (health != null) health.Revive();
            _collider.enabled = true;
            control.enabled = true;
            control.Teleport(spawnPosition);
            animator.Rebind();
            mover = null;
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            var player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                var ev = Schedule<PlayerEnemyCollision>();
                ev.player = player;
                ev.enemy = this;
            }
        }

        void Update()
        {
            if (path != null)
            {
                if (mover == null) mover = path.CreateMover(control.maxSpeed * 0.5f);
                control.move.x = Mathf.Clamp(mover.Position.x - transform.position.x, -1, 1);
            }
        }

    }
}