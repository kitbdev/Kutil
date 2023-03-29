using System;
using UnityEngine;
using Kutil.Ref;

// originally from: https://github.com/KyleBanks/scene-ref-attribute

namespace Kutil {

    /// <summary>
    /// Optional flags offering additional functionality.
    /// </summary>
    [Flags]
    public enum ComponentRefFlag {
        /// <summary>
        /// Default behaviour.
        /// </summary>
        None = 0,
        /// <summary>
        /// Allow empty (or null in the case of non-array types) results.
        /// </summary>
        Optional = 1,
        /// <summary>
        /// Include inactive components in the results (only applies to Child, Scene, and Parent). 
        /// </summary>
        IncludeInactive = 2,
        /// <summary>
        /// Allow the field to be editable in the inspector
        /// </summary>
        Editable = 4,
        /// <summary>
        /// Don't display the field in the inspector. Doesn't hide the HelpBox
        /// </summary>
        Hidden = 8,
    }
    namespace Ref {
        /// <summary>
        /// RefLoc indicates the expected location of the reference.
        /// </summary>
        public enum RefLoc {
            /// <summary>
            /// Anywhere will only validate the reference isn't null, but relies on you to 
            /// manually assign the reference yourself.
            /// </summary>
            Anywhere = -1,
            /// <summary>
            /// Self looks for the reference on the same game object as the attributed component
            /// using GetComponent(s)()
            /// </summary>
            Self = 0,
            /// <summary>
            /// Parent looks for the reference on the parent hierarchy of the attributed components game object
            /// using GetComponent(s)InParent()
            /// </summary>
            Parent = 1,
            /// <summary>
            /// Child looks for the reference on the child hierarchy of the attributed components game object
            /// using GetComponent(s)InChildren()
            /// </summary>
            Child = 2,
            /// <summary>
            /// Scene looks for the reference anywhere in the scene
            /// using GameObject.FindAnyObjectByType() and GameObject.FindObjectsOfType()
            /// </summary>
            Scene = 4,
        }

        /// <summary>
        /// Attribute allowing you to decorate component reference fields with their search criteria. 
        /// </summary>
        [AttributeUsage(AttributeTargets.Field)]
        public abstract class ComponentRefAttribute : PropertyAttribute {
            public RefLoc Loc { get; }
            public ComponentRefFlag Flags { get; }

            public ComponentRefAttribute(RefLoc loc, ComponentRefFlag flags = ComponentRefFlag.None) {
                this.Loc = loc;
                this.Flags = flags;
            }

            public bool HasFlags(ComponentRefFlag flags)
                => (this.Flags & flags) == flags;
        }
    }

    /// <summary>
    /// Anywhere will only validate the reference isn't null, but relies on you to 
    /// manually assign the reference yourself.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class GetAnywhereAttribute : ComponentRefAttribute {
        public GetAnywhereAttribute(ComponentRefFlag flags = ComponentRefFlag.Editable)
            : base(RefLoc.Anywhere, flags: flags) { }
    }

    /// <summary>
    /// GetOnSelf looks for the reference on the same game object as the attributed component
    /// using GetComponent(s)()
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class GetOnSelfAttribute : ComponentRefAttribute {
        public GetOnSelfAttribute(ComponentRefFlag flags = ComponentRefFlag.None)
            : base(RefLoc.Self, flags: flags) { }
    }

    /// <summary>
    /// GetOnParent looks for the reference on the parent hierarchy of the attributed components game object
    /// using GetComponent(s)InParent()
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class GetOnParentAttribute : ComponentRefAttribute {
        public GetOnParentAttribute(ComponentRefFlag flags = ComponentRefFlag.None)
            : base(RefLoc.Parent, flags: flags) { }
    }

    /// <summary>
    /// GetOnChild looks for the reference on the child hierarchy of the attributed components game object
    /// using GetComponent(s)InChildren()
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class GetOnChildAttribute : ComponentRefAttribute {
        public GetOnChildAttribute(ComponentRefFlag flags = ComponentRefFlag.None)
            : base(RefLoc.Child, flags: flags) { }
    }

    /// <summary>
    /// GetInScene looks for the reference anywhere in the scene
    /// using GameObject.FindAnyObjectByType() and GameObject.FindObjectsOfType()
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class GetInSceneAttribute : ComponentRefAttribute {
        public GetInSceneAttribute(ComponentRefFlag flags = ComponentRefFlag.None)
            : base(RefLoc.Scene, flags: flags) { }
    }
}