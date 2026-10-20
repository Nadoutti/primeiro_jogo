using UnityEngine;

namespace NeonSurvivor
{
    /// <summary>Base das armas automáticas. Nível 0 = arma ainda não adquirida.</summary>
    public abstract class Weapon : MonoBehaviour
    {
        public const int MaxLevel = 3;

        public int Level { get; private set; }
        public abstract string DisplayName { get; }
        public abstract string IconName { get; }

        /// <summary>Texto do que o próximo nível faz.</summary>
        public abstract string DescribeNext();

        protected PlayerController Owner => PlayerController.I;
        protected bool Ready => Level > 0 && GameManager.IsPlaying;
        protected float Damage(float baseDamage) => baseDamage * Owner.damageMultiplier;

        public void LevelUp()
        {
            if (Level >= MaxLevel) return;
            Level++;
            OnLevelChanged();
        }

        protected virtual void OnLevelChanged() { }

        protected static Enemy Nearest(Vector2 from, float maxRange)
        {
            Enemy best = null;
            float bestSq = maxRange * maxRange;
            foreach (var e in Enemy.All)
            {
                if (e.IsDead) continue;
                float sq = ((Vector2)e.transform.position - from).sqrMagnitude;
                if (sq < bestSq) { bestSq = sq; best = e; }
            }
            return best;
        }
    }
}
