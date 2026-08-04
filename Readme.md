# Monocle Continued
Monocle (Celeste Game Engine) + FNA, .NET 10

Written by Maddy Thorson and Noel Berry

adapted by EllaTAS

also check out my [Monocle Game Template](https://github.com/ella-TAS/monocle-game-template)

### Updates over original Monocle
- update to .NET 10
- add documentation comments and #nullable to a few important classes
- port to FNA
- clean .csproj file
- reformat all files
- SaveLoad rewrite
- add Logger and update ErrorLogger
- open the console with F12 and close it with Escape
- remove obsolete methods and outsource most extensions from Calc
- mouse position fix with black bars in a non-16:9 window
- extend PixelText component to work like the Text one
- RenderBuffer, a resizable wrapper of RenderTarget2D
- builtin Scene Transitions between two scenes
- NineSliceBox component
- add Camera.Bounds rectangle
- Renderer.Dispose for renderers with target buffers
- SceneEntity<Scene> to designate an entity for that scene
- option for Camera to unlock the pixel grid
- Image.FlipX/Y respects the original dimensions of trimmed MTextures
- Add a default Camera to Scene
