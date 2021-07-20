using Assets.Scripts.Shared.State;
using UnityEngine;

namespace Assets.Scripts.Client.State {
    public class ClientGameMenuState : GameStateBehaviour {
        public override GameState ActiveState => GameState.GameMenu;
    }
}