using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NeonSurvivor
{
    /// <summary>Atalhos para montar UI em código (referência 960x600).</summary>
    public static class UIKit
    {
        public static RectTransform Rect(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = anchor;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        public static RectTransform Stretch(Transform parent, string name)
        {
            var rt = Rect(parent, name, Vector2.zero, Vector2.zero, Vector2.zero);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            return rt;
        }

        public static Image Image(RectTransform rt, Color color, Sprite sprite = null)
        {
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            img.sprite = sprite;
            img.raycastTarget = false;
            if (sprite != null) img.preserveAspect = true;
            return img;
        }

        public static TextMeshProUGUI Text(Transform parent, string name, string text, float size, Color color,
            Vector2 anchor, Vector2 pos, Vector2 box, TextAlignmentOptions align = TextAlignmentOptions.Center)
        {
            var rt = Rect(parent, name, anchor, pos, box);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.fontStyle = FontStyles.Bold;
            t.raycastTarget = false;
            t.textWrappingMode = TextWrappingModes.Normal;
            t.outlineWidth = 0.18f;
            t.outlineColor = new Color32(10, 6, 20, 255);
            return t;
        }

        public class Bar
        {
            public RectTransform Fill;
            public void Set(float fraction) => Fill.anchorMax = new Vector2(Mathf.Clamp01(fraction), 1f);
        }

        public static Bar MakeBar(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size, Color color)
        {
            var frame = Rect(parent, name, anchor, pos, size);
            Image(frame, new Color(0.04f, 0.02f, 0.08f, 0.9f));
            var bg = Stretch(frame, "Back");
            bg.offsetMin = new Vector2(2, 2);
            bg.offsetMax = new Vector2(-2, -2);
            Image(bg, new Color(color.r * 0.25f, color.g * 0.25f, color.b * 0.25f, 1f));
            var fill = Stretch(bg, "Fill");
            Image(fill, color);
            return new Bar { Fill = fill };
        }
    }
}
