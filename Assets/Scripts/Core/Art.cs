using System.Collections.Generic;
using UnityEngine;

namespace NeonSurvivor
{
    /// <summary>Cache dos sprites em Assets/Resources/Sprites.</summary>
    public static class Art
    {
        static readonly Dictionary<string, Sprite> cache = new();

        public static Sprite Get(string name)
        {
            if (cache.TryGetValue(name, out var s) && s != null) return s;
            s = Resources.Load<Sprite>("Sprites/" + name);
            if (s == null) Debug.LogWarning($"Sprite não encontrado: {name}");
            cache[name] = s;
            return s;
        }

        public static readonly Color Pink = new(1f, 0.165f, 0.427f);
        public static readonly Color Cyan = new(0.02f, 0.85f, 0.91f);
        public static readonly Color Yellow = new(0.976f, 0.973f, 0.443f);
        public static readonly Color Violet = new(0.48f, 0.17f, 0.75f);
        public static readonly Color Red = new(0.9f, 0.08f, 0.2f);
    }
}
