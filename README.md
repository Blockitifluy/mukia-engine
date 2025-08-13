<div align="center">

<h3>Mukia</h3>
An open-source Game Engine

<i>Engine Test</i>

</div>

# Scenes

## Loading Demo Scene

```cmd
ZombieSurvial demo
```

When getting an error check:

1. Does the scene folder have any malformed text
2. Is this `resource\scene\` directories is created
3. Forgot to add the `SaveNode` attribute, or uses the wrong name
4. Make an issue on the github project

## Build Project

To build the C# use:

```CMD
./make-build <path-to-destination>
```

## Loading Scenes

```cmd
ZombieSurvial load <path-to-scene>
```

When recieving an error, do the same steps in the [Loading Demo Scene](#loading-demo-scene).

## Scene Formating

> [!NOTE]
> Editing a file scene directly is not suggested

### Node should always come first

A node declearation should be the first thing in scene, with all of the properties under it.

e.g.

```scene
# Node
[engine.camera local-id='1' parent='0']
	# Properties of Node
	MukiaEngine.Vector3 Position={"X":0,"Y":0,"Z":0}
	MukiaEngine.Vector3 Rotation={"X":0,"Y":0,"Z":0}
	MukiaEngine.Vector3 Scale={"X":1,"Y":1,"Z":1}
	System.String Name="PlayerCamera"
```

### Type Name=Value

Node declearation properties should be as follows:

- Type: as in full name (so `float` becomes `System.Single`) and be or inherit the type in the node class.
- Name: The name should also be in a valid name, in the node's type
- Value: in the JSON format (allows for number literals like `NaN` or infinity)

### local-ids should not duplicate

Local id's should be unique and should not be duplicate in multiple Node declearations. For example:

```scene
# fine
[engine.node local-id"1" parent="parent"]
# results in error, or unknown behaviour
[engine.node local-id"1" parent="parent"]
```

### No parent is a 0

To declear that node is parented to tree, add 32 0s.

```scene
[engine.node local-id"1" parent="0"]
```

# Meshes

To convert a .obj to a mesh usable in the Engine.

1. Export the mesh as an .obj

![In blender, File/Export/Wavefront (.obj)](/docs/export-mesh-example.png).

2. Use the command:

```bat
py tools/mesh_converter.py [obj-file-here] [export-file-here]
```

To be noted, the script only supports .obj files and is surport to handle one object.
