# vr_assets_common
Shared assets to be included in Unity scenes

The file structure for this repository is:

## Environments

Contains complete assemblies to put in a scene, so that a scene can be as simple as an environment prefab and a user prefab.

The Environment folder only contains prefabs and does not contain nested folders.

## Terrain

Contains terrains to be used in environments. This includes ground, walls and ceilings. It is the map in which we navigate, without objects.

Toplevel contains the prefabs to be used in the environments.

It also contains these subfolders:

- FBX
- Textures
- Materials (optional)
- Scripts (optional)

## Objects

Contains objects to be included in environments. Objects are grouped by theme, e.g., Nature, Street, etc. which is the first level of the folder.

Each subfolder contains prefabs to be used in the environments.

It also contains these subfolders:

- FBX
- Textures (optional)
- Materials (optional)
- Scripts (optional)
- Shaders (optional)
- Prefabs (optional, construction prefabs that are used to build the usable prefabs but are not meant to be included in an environment)

