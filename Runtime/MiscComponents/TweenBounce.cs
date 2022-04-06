using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if DOTween
using DG.Tweening;
#endif

namespace Kutil {
    // todo make better
    public class TweenBounce : MonoBehaviour {
// requires dotween
#if DOTween // is this the right symbol?
        // bob up and down
        // or shake around?
        [SerializeField] Vector3 targetPos = Vector3.up * 0.1f;
        [SerializeField] float duration = 0.1f;
        [SerializeField] Ease easing = Ease.InOutSine;
        [SerializeField] bool playOnStart;

        [SerializeField, ReadOnly] Tween tween;

        // private void OnDestroy() {
        //     Stop();
        // }
        private void Start() {
            if (playOnStart) {
                PlayTween();
            }
        }
        [ContextMenu("play")]
        public void PlayTween() {
            if (tween != null) {
                if (!tween.IsPlaying()) {
                    // Debug.Log("Restarting");
                    tween.Restart();
                }
                return;
            }
            PlayNew();
        }

        [ContextMenu("play new")]
        private void PlayNew() {
            if (tween != null) {
                tween.Kill();
            }
            tween = transform.DOLocalMove(targetPos, duration)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetEase(easing)
                        .SetLink(gameObject, LinkBehaviour.PauseOnDisablePlayOnEnable)
                        ;
            tween.Play();
        }

        [ContextMenu("stop")]
        public void StopTween() {
            if (tween != null) {
                // Debug.Log("Stopping");
                tween.Complete();
                tween.Pause();
                // tween.Kill(true);
                // tween = null;
            }
        }
#endif
    }
}