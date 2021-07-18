using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Client {
    public class StartupTransition : MonoBehaviour {
        [SerializeField] private int _mainMenuBuildIndex = 1;

        private void Start()
        {
            SceneManager.LoadScene(_mainMenuBuildIndex);
        }
    }
}