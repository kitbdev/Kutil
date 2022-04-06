using UnityEngine;

namespace Kutil {
    public class ReadOnlyAttribute : PropertyAttribute {
        public ReadOnlyAttribute() { 
            order = -100;
        }
    }
}