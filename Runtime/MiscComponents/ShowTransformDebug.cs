using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kutil {
    public class ShowTransformDebug : MonoBehaviour {
        void Update() {
            Debug.DrawRay(transform.position, transform.forward, Color.blue);
            Debug.DrawRay(transform.position, transform.up, Color.green);
            Debug.DrawRay(transform.position, transform.right, Color.red);
        }
    }
}