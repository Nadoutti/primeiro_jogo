using Platformer.Core;
using Platformer.Mechanics;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Fired when a bullet hits an enemy.
    /// </summary>
    /// <typeparam name="EnemyHit"></typeparam>
    public class EnemyHit : Simulation.Event<EnemyHit>
    {
        public EnemyController enemy;

        public override void Execute()
        {
            var enemyHealth = enemy.GetComponent<Health>();
            if (enemyHealth != null)
            {
                enemyHealth.Decrement();
                if (enemyHealth.IsAlive)
                {
                    var animator = enemy.GetComponent<Animator>();
                    if (animator) animator.SetTrigger("hurt");
                    return;
                }
            }
            Schedule<EnemyDeath>().enemy = enemy;
        }
    }
}
