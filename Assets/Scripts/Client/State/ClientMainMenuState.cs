using Assets.Scripts.Shared.State;
using UnityEngine;

namespace Assets.Scripts.Client.State {
    public class ClientMainMenuState : GameStateBehaviour {
        public override GameState ActiveState => GameState.MainMenu;
    }
}