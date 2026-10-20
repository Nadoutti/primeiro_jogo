using UnityEngine;

namespace NeonSurvivor
{
    /// <summary>Drone que persegue o jogador.</summary>
    public class DroneEnemy : Enemy
    {
        public Sprite[] frames;
        float animT;
        float wobbleSeed;

        protected override void Awake()
        {
            base.Awake();
            wobbleSeed = Random.value * 10f;
        }

        protected override void Update()
        {
            base.Update();
            animT += Time.deltaTime * 10f;
            if (frames != null && frames.Length > 0) sr.sprite = frames[(int)animT % frames.Length];
            sr.flipX = ToPlayer.x < 0;
        }

        protected override Vector2 Steer()
        {
            var dir = ToPlayer.normalized;
            // leve zigue-zague pra não andarem todos em linha reta
            var side = new Vector2(-dir.y, dir.x) * Mathf.Sin(Time.time * 3f + wobbleSeed) * 0.35f;
            return (dir + side) * speed;
        }
    }
}
