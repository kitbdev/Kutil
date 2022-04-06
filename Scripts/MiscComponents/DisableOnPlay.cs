using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kutil {
    public class DisableOnPlay : MonoBehaviour {
        private void OnEnable() {
            gameObject.SetActive(false);
        }
    }
}