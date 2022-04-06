using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kutil {
    public class AnimPlayer : MonoBehaviour {
        [System.Serializable]
        public class AnimTrigger {
            public enum ParamType {
                TRIGGER, BOOL, INT, FLOAT
            }
            // [ReadOnly] public int animId;
            [SerializeField]
            private string _paramName = "";
            public string paramName {
                get => _paramName;
                set {
                    _paramName = value; //animId = Animator.StringToHash(_paramName);
                }
            }
            public ParamType paramType;
            [ConditionalHide(nameof(paramType), (int)ParamType.BOOL)]
            public bool boolValue;
            [ConditionalHide(nameof(paramType), (int)ParamType.INT)]
            public int intValue;
            [ConditionalHide(nameof(paramType), (int)ParamType.FLOAT)]
            public float floatValue;
            // private AnimPlayer _animPlayer;

            public AnimTrigger() { }

            public AnimTrigger(ParamType paramType) {
                this.paramType = paramType;
            }
            public AnimTrigger(string paramName, ParamType paramType) {
                this.paramName = paramName;
                this.paramType = paramType;
            }

            public static AnimTrigger BoolAnim => new AnimTrigger(ParamType.BOOL);
            public static AnimTrigger IntAnim => new AnimTrigger(ParamType.INT);
            public static AnimTrigger FloatAnim => new AnimTrigger(ParamType.FLOAT);
            public static AnimTrigger TriggetAnim => new AnimTrigger(ParamType.TRIGGER);

        }

        public List<AnimTrigger> animTriggers = new List<AnimTrigger>();
        [SerializeField] Animator anim;

        private void Awake() {
            GetAnim();
            UpdateHashIds();
        }
        void GetAnim() {
            anim ??= GetComponent<Animator>();
        }
        private void OnValidate() {
            GetAnim();
            UpdateHashIds();
        }
        public void UpdateHashIds() {
            // ? not working
            // foreach (var trig in animTriggers) {
            //     trig.animId = Animator.StringToHash(trig.paramName);
            // }
        }
        public void Play() {
            Play(0);
        }
        public void Play(int index = 0) {
            if (index >= 0 && index < animTriggers.Count) {
                PlayAnim(animTriggers[index]);
            } else {
                Debug.LogWarning("Invalid anim index " + index);
            }
        }
        public void PlayAnim(AnimTrigger ev) {
            if (!anim) {
                Debug.Log("No anim for " + name, this);
                return;
            }
            switch (ev.paramType) {
                case AnimTrigger.ParamType.TRIGGER:
                    anim.SetTrigger(ev.paramName);
                    break;
                case AnimTrigger.ParamType.BOOL:
                    anim.SetBool(ev.paramName, ev.boolValue);
                    break;
                case AnimTrigger.ParamType.INT:
                    anim.SetInteger(ev.paramName, ev.intValue);
                    break;
                case AnimTrigger.ParamType.FLOAT:
                    anim.SetFloat(ev.paramName, ev.floatValue);
                    break;
            }
        }
        public void PlayAnim(AnimTrigger animt, bool bval) {
            animt.boolValue = bval;
            PlayAnim(animt);
        }
        public void PlayAnim(AnimTrigger animt, int ival) {
            animt.intValue = ival;
            PlayAnim(animt);
        }
        public void PlayAnim(AnimTrigger animt, float fval) {
            animt.floatValue = fval;
            PlayAnim(animt);
        }
    }
}