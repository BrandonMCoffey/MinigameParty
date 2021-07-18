using UnityEngine;

namespace Assets.Scripts {
    public class DontDestroy : MonoBehaviour {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}