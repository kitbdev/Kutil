using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//https://www.dylanwolf.com/2018/11/24/stupid-unity-ui-navigation-tricks/
/// <summary>
/// Prevent UI deselection.
/// </summary>
// /// Must disable when there are no selectables
public class PreventDeselection : MonoBehaviour {

    EventSystem evt;
    GameObject sel;

    private void Start() {
        evt = EventSystem.current;
    }

    private void Update() {
        if (evt.currentSelectedGameObject != null && evt.currentSelectedGameObject != sel) {
            sel = evt.currentSelectedGameObject;
        } else if (sel != null && evt.currentSelectedGameObject == null) {
            evt.SetSelectedGameObject(sel);
        }
    }
}