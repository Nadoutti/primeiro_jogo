using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;
using UnityEngine.InputSystem;

namespace Platformer.Mechanics
{
    /// <summary>
    /// This is the main class used to implement control of the player.
    /// It is a superset of the AnimationController class, but is inlined to allow for any kind of customisation.
    /// </summary>
    public class PlayerController : KinematicObject
    {
        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;

        /// <summary>
        /// Max horizontal speed of the player.
        /// </summary>
        public float maxSpeed = 7;
        /// <summary>
        /// Initial jump velocity at the start of a jump.
        /// </summary>
        public float jumpTakeOffSpeed = 7;
        /// <summary>
        /// How many extra jumps the player can do while in the air (1 = double jump).
        /// </summary>
        public int maxAirJumps = 1;
        /// <summary>
        /// Extra gravity multiplier while rising with the jump button released.
        /// Gives a smooth short hop instead of an abrupt stop.
        /// </summary>
        public float shortJumpGravityModifier = 2f;

        public JumpState jumpState = JumpState.Grounded;
        /*internal new*/ public Collider2D collider2d;
        /*internal new*/ public AudioSource audioSource;
        public Health health;
        public bool controlEnabled = true;

        bool jump;
        bool jumpHeld;
        bool airJump;
        int airJumpsLeft;
        Vector2 move;
        SpriteRenderer spriteRenderer;
        internal Animator animator;
        readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        private InputAction m_MoveAction;
        private InputAction m_JumpAction;

        public Bounds Bounds => collider2d.bounds;

        void Awake()
        {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();

            m_MoveAction = InputSystem.actions.FindAction("Player/Move");
            m_JumpAction = InputSystem.actions.FindAction("Player/Jump");
            
            m_MoveAction.Enable();
            m_JumpAction.Enable();
        }

        protected override void Update()
        {
            if (controlEnabled)
            {
                move.x = m_MoveAction.ReadValue<Vector2>().x;
                jumpHeld = m_JumpAction.IsPressed();
                if (IsGrounded)
                    airJumpsLeft = maxAirJumps;
                if (jumpState == JumpState.Grounded && IsGrounded && m_JumpAction.WasPressedThisFrame())
                    jumpState = JumpState.PrepareToJump;
                else if (!IsGrounded && airJumpsLeft > 0 && m_JumpAction.WasPressedThisFrame())
                {
                    airJumpsLeft--;
                    airJump = true;
                    //also covers walking off a ledge, so landing is detected afterwards.
                    jumpState = JumpState.InFlight;
                    Schedule<PlayerJumped>().player = this;
                }
                else if (m_JumpAction.WasReleasedThisFrame())
                    Schedule<PlayerStopJump>().player = this;
            }
            else
            {
                move.x = 0;
                jumpHeld = false;
            }
            UpdateJumpState();
            base.Update();
        }

        void UpdateJumpState()
        {
            jump = false;
            switch (jumpState)
            {
                case JumpState.PrepareToJump:
                    jumpState = JumpState.Jumping;
                    jump = true;
                    break;
                case JumpState.Jumping:
                    if (!IsGrounded)
                    {
                        Schedule<PlayerJumped>().player = this;
                        jumpState = JumpState.InFlight;
                    }
                    break;
                case JumpState.InFlight:
                    if (IsGrounded)
                    {
                        Schedule<PlayerLanded>().player = this;
                        jumpState = JumpState.Landed;
                    }
                    break;
                case JumpState.Landed:
                    jumpState = JumpState.Grounded;
                    break;
            }
        }

        protected override void ComputeVelocity()
        {
            if (jump && IsGrounded)
            {
                velocity.y = jumpTakeOffSpeed * model.jumpModifier;
                jump = false;
            }
            else if (airJump)
            {
                velocity.y = jumpTakeOffSpeed * model.jumpModifier;
                airJump = false;
            }

            if (move.x > 0.01f)
                spriteRenderer.flipX = false;
            else if (move.x < -0.01f)
                spriteRenderer.flipX = true;

            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);

            targetVelocity = move * maxSpeed;
        }

        protected override float CurrentGravityModifier =>
            velocity.y > 0 && !jumpHeld ? base.CurrentGravityModifier * shortJumpGravityModifier : base.CurrentGravityModifier;

        public enum JumpState
        {
            Grounded,
            PrepareToJump,
            Jumping,
            InFlight,
            Landed
        }
    }
}