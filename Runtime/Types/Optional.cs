using System;
using UnityEngine;

namespace Kutil {
    [Serializable]
    /// Requires Unity 2020.1+ (generic serialization)
    /// <summary>
    /// Store an optional value
    /// </summary>
    public struct Optional<T> : System.IEquatable<Optional<T>> {

    [SerializeField] private bool _enabled;
    [SerializeField] private T _value;

    public bool Enabled {
        get { return _enabled; }
        set { _enabled = value; }
    }
    public T Value {
        get { return _value; }
        set { _value = value; }
    }

    public Optional(T initialValue, bool enabled = true) {
        _enabled = enabled;
        _value = initialValue;
    }

    public bool Equals(Optional<T> other) {
        // do two disabled optionals equal each other? and type matches?
        if (!_enabled && !other.Enabled
            && this.GetType().Equals(other.GetType())) return true;
        return _enabled == other._enabled && _value.Equals(other._value);
    }

    public override bool Equals(object obj) {
        if (obj is Optional<T> o) {
            return Equals(o);
        }
        return base.Equals(obj);
    }
    public override int GetHashCode() {
        if (!Enabled) {
            return GetType().GetHashCode() + 1;
        }
        return base.GetHashCode();
    }
    public override string ToString() {
        if (Enabled) {
            return $"{(_value?.ToString() ?? "null")}?";
        } else {
            return "None";
        }
    }

    // todo? other util stuff
    public static Optional<T> None = new Optional<T>(default, false);
}
}