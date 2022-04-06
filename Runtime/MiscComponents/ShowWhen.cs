using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Kutil {
    public class ShowWhen : MonoBehaviour {
        public ValueRef<bool> showRef;
        public bool invert = false;
        public GameObject target;
        public bool controlTargetActive = true;
        public bool controlChildren = false;
        public bool controlImageAndText = false;
        public float updatePollInterval = 0.1f;
        float updatePollLastTime = 0;

        private void OnEnable() {
            UpdateVis();
        }
        private void Update() {
            if (updatePollInterval > 0) {
                if (Time.unscaledTime > updatePollLastTime + updatePollInterval) {
                    updatePollLastTime = Time.unscaledTime;
                    UpdateVis(true);
                }
            }
        }

        [ContextMenu("Update")]
        public void UpdateVis(bool mustChange = false) {
            if (mustChange ? showRef.TryGetValueChanged(out bool show) : showRef.TryGetValue(out show)) {
                if (invert) show = !show;

                if (controlTargetActive) {
                    target.SetActive(show);
                } else if (controlChildren) {
                    // for (int i = transform.childCount - 1; i >= 0; i--)
                    // {
                    //     transform.GetChild(i).gameObject.SetActive(show);
                    // }
                }
                if (controlImageAndText) {
                    if (TryGetComponent<Image>(out var img)) {
                        img.enabled = show;
                    }
                    if (TryGetComponent<Text>(out var text)) {
                        text.enabled = show;
                    }
                    if (TryGetComponent<TextMeshPro>(out var tmptext)) {
                        tmptext.enabled = show;
                    }
                }
            }
        }
    }
}