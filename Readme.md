# Monocle Continued
Monocle (Celeste Game Engine) + FNA, .NET 10

Written by Maddy Thorson

adapted by EllaTAS

also check out my [Monocle Game Template](https://github.com/ella-TAS/monocle-game-template)

### Updates over original Monocle
- update to .NET 10
- port to FNA
- clean .csproj file
- reformat all files
- path fix for Atlas CrunchXML mode
- SaveLoad rewrite
- add #nullable to a few important classes
- add a Logger class
- open the console with F12 (because it wasn't working on my keyboard layout) and close it with Escape
- remove obsolete methods from Calc
- mouse position fix (it wouldn't respect black bars on non-16:9 screens in fullscreen)
- extend PixelText component to work like the Text one
- RenderBuffer, a resizable wrapper of RenderTarget2D
- builtin Scene Transitions between two scenes
