using UnityEngine;

namespace Kutil {
    public class DontDestroyOnLoad : MonoBehaviour {
        private void Awake() {
            DontDestroyOnLoad(gameObject);
        }
    }
}