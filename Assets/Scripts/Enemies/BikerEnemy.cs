using UnityEngine;

namespace NeonSurvivor
{
    /// <summary>Moto kamikaze: atravessa a arena em linha reta numa direção pré-determinada.</summary>
    public class BikerEnemy : Enemy
    {
        public Vector2 direction = Vector2.right;
        public float delay;
        public Rect arena;
        float trailTimer;

        /// <summary>Faixa vermelha piscando no caminho que as motos vão passar.</summary>
        public static void Warn(Vector2 lane, bool horizontal, Rect arena)
        {
            var size = horizontal ? new Vector2(arena.width * 8f, 3.4f * 8f) : new Vector2(3.4f * 8f, arena.height * 8f);
            var pos = horizontal ? new Vector2(arena.center.x, lane.y) : new Vector2(lane.x, arena.center.y);
            var f = Fader.Spawn(Art.Get("pixel"), pos, new Color(1f, 0.1f, 0.2f, 0.2f), 1.3f, default, 1f, 1f, SortOrder.FloorDecal + 5);
            f.transform.localScale = new Vector3(size.x, size.y, 1f);
            f.scaleFrom = f.scaleTo = 1f;
            f.gameObject.AddComponent<WarnStretch>().size = size;
            AudioManager.Play("boss_alert", 0.25f, 0.2f);
        }

        protected override void Update()
        {
            base.Update();
            if (delay > 0) return;
            trailTimer -= Time.deltaTime;
            if (trailTimer <= 0)
            {
                trailTimer = 0.03f;
                Fader.Spawn(Art.Get("pixel"), (Vector2)transform.position - direction * 0.6f,
                    new Color(1f, 0.16f, 0.43f, 0.8f), 0.35f, default, 3f, 1f, SortOrder.FloorDecal + 10);
            }
            var p = (Vector2)transform.position;
            bool gone = (direction.x > 0 && p.x > arena.xMax + 2) || (direction.x < 0 && p.x < arena.xMin - 2)
                     || (direction.y > 0 && p.y > arena.yMax + 2) || (direction.y < 0 && p.y < arena.yMin - 2);
            if (gone) Die(false);
        }

        protected override Vector2 Steer()
        {
            if (delay > 0)
            {
                delay -= Time.fixedDeltaTime;
                return Vector2.zero;
            }
            return direction * speed;
        }

        void OnTriggerStay2D(Collider2D c)
        {
            if (delay <= 0 && c.TryGetComponent(out PlayerController p)) p.TakeDamage(contactDamage);
        }
    }

    /// <summary>Mantém a escala do aviso fixa enquanto o Fader cuida do alpha.</summary>
    public class WarnStretch : MonoBehaviour
    {
        public Vector2 size;
        void LateUpdate() => transform.localScale = new Vector3(size.x, size.y, 1f);
    }
}
