using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kutil {
    public class BuildPlatformEnable : MonoBehaviour {

        public bool enabledOnWeb = true;
        public bool enabledOnStandalone = true;
        public bool enabledOnMobile = true;

        private void Awake() {
            if (!enabledOnWeb) {
#if UNITY_WEBGL
        gameObject.SetActive(false);
#endif
            }
            if (!enabledOnStandalone) {
#if UNITY_STANDALONE
        gameObject.SetActive(false);
#endif
            }
            if (!enabledOnMobile) {
#if UNITY_ANDROID || UNITY_IOS
        gameObject.SetActive(false);
#endif
            }

        }
    }
}