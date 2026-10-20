using UnityEngine;

namespace NeonSurvivor
{
    /// <summary>Câmera que segue o jogador, presa aos limites da arena, com tremida de tela.</summary>
    [RequireComponent(typeof(Camera))]
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        public Rect bounds = new(-24, -15, 48, 30);
        public float smooth = 10f;

        static float shakeAmount, shakeTime;
        Camera cam;

        public static void Shake(float amount, float duration)
        {
            shakeAmount = Mathf.Max(shakeAmount, amount);
            shakeTime = Mathf.Max(shakeTime, duration);
        }

        void Awake() => cam = GetComponent<Camera>();

        void LateUpdate()
        {
            if (target == null) return;
            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;
            var goal = (Vector2)target.position;
            goal.x = Mathf.Clamp(goal.x, bounds.xMin + halfW, bounds.xMax - halfW);
            goal.y = Mathf.Clamp(goal.y, bounds.yMin + halfH, bounds.yMax - halfH);

            var pos = Vector2.Lerp(transform.position, goal, 1f - Mathf.Exp(-smooth * Time.unscaledDeltaTime));
            if (shakeTime > 0 && Time.timeScale > 0)
            {
                shakeTime -= Time.deltaTime;
                pos += Random.insideUnitCircle * shakeAmount;
                if (shakeTime <= 0) shakeAmount = 0;
            }
            transform.position = new Vector3(pos.x, pos.y, -10f);
        }
    }
}
