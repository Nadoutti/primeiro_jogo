using UnityEngine;

namespace NeonSurvivor
{
    /// <summary>Poça eletrificada: causa dano ao jogador enquanto ele estiver em cima (o dash atravessa).</summary>
    [RequireComponent(typeof(Collider2D))]
    public class ElectricPuddle : MonoBehaviour
    {
        public float damage = 5f;
        public Sprite[] frames;
        SpriteRenderer sr;
        float t;

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            t = Random.value;
        }

        void Update()
        {
            t += Time.deltaTime * 6f;
            if (frames != null && frames.Length > 0) sr.sprite = frames[(int)t % frames.Length];
        }

        void OnTriggerStay2D(Collider2D other)
        {
            if (other.TryGetComponent(out PlayerController p) && !p.IsDashing) p.TakeDamage(damage);
        }
    }
}
