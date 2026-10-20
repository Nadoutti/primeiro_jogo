using UnityEngine;

namespace NeonSurvivor
{
    /// <summary>Ordena sprites pela altura (quem está mais embaixo desenha na frente).</summary>
    public class YSort : MonoBehaviour
    {
        public bool isStatic;
        public float footOffset;
        SpriteRenderer[] renderers;
        int[] baseOrders;

        void Awake()
        {
            renderers = GetComponentsInChildren<SpriteRenderer>(true);
            baseOrders = new int[renderers.Length];
            for (int i = 0; i < renderers.Length; i++) baseOrders[i] = renderers[i].sortingOrder;
            Apply();
        }

        void LateUpdate()
        {
            if (!isStatic) Apply();
        }

        void Apply()
        {
            int order = -Mathf.RoundToInt((transform.position.y + footOffset) * 10f);
            for (int i = 0; i < renderers.Length; i++) renderers[i].sortingOrder = order + baseOrders[i];
        }
    }

    /// <summary>Camadas fixas de desenho.</summary>
    public static class SortOrder
    {
        public const int Floor = -5000;
        public const int FloorDecal = -4900;
        public const int Hazard = -4800;
        public const int Pickup = -4000;
        public const int Projectile = 3000;
        public const int Fx = 3500;
    }
}
