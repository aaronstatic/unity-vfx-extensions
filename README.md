# unity-vfx-extensions
Extensions for the Unity Visual Effect Graph and Shader graph

Mostly centered around Audio visualization

# How to use:

Basic setup:
1. Add a "Audio Spectrum" behaviour to any Game Object
1. Assign an AudioSource object to it

Shader graph (or any shader really):
1. Add a "Audio Spectrum Shader Binder" to any object with a material
1. Assign your AudioSpectrum object to it
1. Set the shader property name to update and set options

VFX Graph:
1. Add a "VFX Property Binder" to your Visual Effect Graph object
1. Click the "+" button and choose "Audio Spectrum"
1. Assign your AudioSpectrum object to it
1. Set your exposed property names (all floats except for Spectrum and Spectrum history, which are Texture2D)
1. Set any options
