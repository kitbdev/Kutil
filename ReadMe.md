Some utilty scripts for unity

# Runtime

Types
- Layer
- Easing - helpful Easing functions
- GridMap - stores 3d data
- Direction - 4 way
- SerializableDictionary
- SerializableStack
- SerializedType
- TypeChoice - shows a type in the inspector

Systems
- Save System - easy way to save to a file
- Pause Manager - manage pause state, time scale
- Cursor Manager - lock and unlock cursor
- Audio Manager
- Object Pool
- UI
  - Menu Screen - fade in and out, manage multiple screens
  - Draggable Window

Misc
- fps camera
- top-down camera
- singleton
- generic trigger
- follow transform
- face camera
- etc

Extensions
- BoundsInt
- Mathf
- Enumerable
- Reflection
- misc

# Editor


Decorator Drawers
- ReadOnly - makes a field readonly
- GetOnSelf, GetOnChild, GetOnParent (ComponentRefAttributes) - tries to automatically get a component. [inspired by scene ref attibute](https://github.com/KyleBanks/scene-ref-attribute)
- Required - show an error if this field is not set
- Add button - adds a button above the field, like space or header
- Add note - adds a label note
- Collapsable - makes this field and those below it into a collapsable foldout in the inspector
- ConditionalHide - hide this field contditionally
- PostFieldDecorator - moves decorators after this after the field
- DecoratorGroup - groups together decorators to display them horizontally
- CustomDropDownDrawer - show a custom drop down drawer in place of this field
- ExtendedDecorator - inherit to easily make a custom decorator drawer with the functionality of a property drawer
- BoundsEditorTool - shows box collider like handles for any Bounds or BoundsInt in the scene
- Vector2DDraw - draws a vector in the inspector, for debugging


Other
- build script
- auto version increment
- other



# updates

works in Unity 2022.2


