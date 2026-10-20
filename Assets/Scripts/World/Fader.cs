using UnityEngine;

namespace NeonSurvivor
{
    /// <summary>Sprite temporário que se move, escala e desaparece (partículas, rastros, ondas).</summary>
    public class Fader : MonoBehaviour
    {
        public float life = 0.4f;
        public Vector2 velocity;
        public float drag = 4f;
        public float scaleFrom = 1f, scaleTo = 1f;

        SpriteRenderer sr;
        Color color;
        float t;

        public static Fader Spawn(Sprite sprite, Vector2 pos, Color color, float life,
            Vector2 velocity = default, float scaleFrom = 1f, float scaleTo = 1f, int order = SortOrder.Fx)
        {
            var go = new GameObject("Fx");
            go.transform.position = pos;
            var f = go.AddComponent<Fader>();
            f.sr = go.AddComponent<SpriteRenderer>();
            f.sr.sprite = sprite;
            f.sr.sortingOrder = order;
            f.sr.color = color;
            f.color = color;
            f.life = life;
            f.velocity = velocity;
            f.scaleFrom = scaleFrom;
            f.scaleTo = scaleTo;
            go.transform.localScale = Vector3.one * scaleFrom;
            return f;
        }

        /// <summary>Explosão de pixels coloridos.</summary>
        public static void Burst(Vector2 pos, Color color, int count, float speed = 5f)
        {
            var px = Art.Get("pixel");
            for (int i = 0; i < count; i++)
            {
                var v = Random.insideUnitCircle.normalized * Random.Range(speed * 0.4f, speed);
                Spawn(px, pos, color, Random.Range(0.25f, 0.5f), v, 1.5f, 0.5f);
            }
        }

        void Update()
        {
            t += Time.deltaTime;
            float k = t / life;
            if (k >= 1f) { Destroy(gameObject); return; }
            transform.position += (Vector3)(velocity * Time.deltaTime);
            velocity *= Mathf.Exp(-drag * Time.deltaTime);
            transform.localScale = Vector3.one * Mathf.Lerp(scaleFrom, scaleTo, k);
            var c = color;
            c.a *= 1f - k;
            sr.color = c;
        }
    }
}
