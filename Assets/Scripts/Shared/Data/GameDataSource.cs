using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Shared.Data {
    public class GameDataSource : MonoBehaviour {
        [SerializeField] private List<CharacterClass> _characterClasses;
        [SerializeField] private List<CharacterAction> _characterActions;
    }
}