using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Client {
    [CreateAssetMenu]
    public class RandomNames : ScriptableObject {
        [SerializeField] private List<string> _firstWordList = new List<string>();
        [SerializeField] private List<string> _secondWordList = new List<string>();

        public string GenerateName()
        {
            var firstWord = _firstWordList[Random.Range(0, _firstWordList.Count - 1)];
            var secondWord = _secondWordList[Random.Range(0, _secondWordList.Count - 1)];

            return firstWord + " " + secondWord;
        }
    }
}