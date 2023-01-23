# 3d-Editor

## About
This was my adventure in trying to make some sort of 3D level editing environment using Monogame/XNA.

I usually lean on a feature rich 2D framework called [NEZ](https://github.com/prime31/Nez) when using monogame, however it wasn't going to work for as-is for what I was trying to accomplish. I basically just ripped it apart, cherry picked the stuff I would need, and modified the source code to work somewhat well with 3D. I modified and used their IMGUI extension aswell. Along with the modified NEZ source code, I found and modified a 3d transform gizmo utility as well as an old terrain library that could paint/modify heightmaps and textures.

The real magic here is all my modifications to the various libraries/frameworks to get them to somewhat play nice together, and use monogame/nez (a typically 2D environment) along with IMGUI to design an environment where you could drag and drop transformable models (position, rotation, scale), aswell as mold and paint terrain. 

It's hard to tell where my code starts and the other's end, but I take no credit for the majority of the code here which has been picked from other places. This was more of a sandbox experiment that grew, than an attempt to build a real usable editor.

## How

There is a window full of models below, you can drag a model into the scene and move it using the transoform gizmo. You can also add a terraincomponent to an entity and be provided a terrain to modify. Moving through the scene is pretty typical: holding right mouse button will aim the view angle, <kbd>WASD</kdb> will move back and forth or strafe left and right, middle mouse will pan the camera.

### Transform

- <kbd>Z</kbd> for position
- <kbd>X</kbd> for rotation
- <kbd>C</kbd> for unified scale
- <kbd>Z</kbd> for scale

Here is a video demonstration:
[![screenshot](https://res.cloudinary.com/dadxsisd4/image/upload/v1674512553/screenshots/editorss_vraplt.png)](https://drive.google.com/file/d/1C-TyYbT2XyTO3HNtCqH23lY3O7ab0d56/view)

### Terrain Component

When edit mode in the component is checked (enabled) you can Paint, Smooth, Raise, or Lower the terrain, with the option to change your brush size and brush strength.

## Issues
After coming back to this from a long leave, I found that I had broken the ability to reselect an entity after it loses focus. Probably broke it the last time I was working on it to implement something new, and my lack of commenting/version control has me a little lost trying to comprehend where that change happened.

