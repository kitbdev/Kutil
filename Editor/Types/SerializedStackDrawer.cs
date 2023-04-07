using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Kutil {
    [UnityEditor.CustomPropertyDrawer(typeof(SerializedStack<>))]
    public class SerializedStackDrawer : ShowAsChildPropertyDrawer {
        // public override string childName => nameof(SerializedStack<int> .serialized);
        public override string childName => "serialized";
    }

}