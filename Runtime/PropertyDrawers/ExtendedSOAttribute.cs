using UnityEngine;

namespace Kutil {
    public class ExtendedSOAttribute : System.Attribute {

        public bool allowCreation { get; set; } = true;

        public ExtendedSOAttribute() { }
    }
}