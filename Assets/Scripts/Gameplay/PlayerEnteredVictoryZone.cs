using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;
using Platformer.UI;
using UnityEngine;

namespace Platformer.Gameplay
{

    /// <summary>
    /// This event is triggered when the player character enters a trigger with a VictoryZone component.
    /// </summary>
    /// <typeparam name="PlayerEnteredVictoryZone"></typeparam>
    public class PlayerEnteredVictoryZone : Simulation.Event<PlayerEnteredVictoryZone>
    {
        public VictoryZone victoryZone;

        PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public override void Execute()
        {
            //the player only wins after collecting every token.
            var tokens = Object.FindFirstObjectByType<TokenController>();
            if (tokens != null && !tokens.AllCollected)
                return;

            model.player.animator.SetTrigger("victory");
            model.player.controlEnabled = false;

            var hud = Object.FindFirstObjectByType<GameHUD>();
            if (hud != null) hud.ShowVictory();
        }
    }
}