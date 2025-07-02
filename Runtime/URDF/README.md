# URDF Importing Points of Failure
## You can not right-click a urdf here and import
Because unity can/does not crawl the right folders... You need to move a folder to the project's Assets folder, and do it there.

## Be careful when moving robot folders back here!
Do this from inside unity editor, so the urdf-imported robot's meshes and such get updated properly!
Otherwise object ID madness will happen and the imported robot will lose all of its meshes.