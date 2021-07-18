using UnityEngine;

namespace Assets.Scripts.Client.UI {
    public class SpinAnimation : MonoBehaviour {
        [SerializeField] private float _spinRate = 50;

        private void Update()
        {
            transform.Rotate(new Vector3(0, 0, _spinRate * Time.deltaTime));
        }
    }
}