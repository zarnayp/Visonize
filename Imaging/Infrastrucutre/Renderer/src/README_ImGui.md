# ImGui Renderer

This directory contains the ImGui vertex renderer implementation for the DupploPulse UltrasoundImaging system.

## Overview

The ImGui renderer (`ImGuiVertexRenderer`) enables rendering of ImGui.NET vertices and draw commands using the DiligentEngine graphics API. This allows ImGui UI elements to be rendered as part of the ultrasound imaging scene.

## Components

### IImGuiSceneObject
Interface that defines the contract for ImGui scene objects:
- `DrawData`: ImGui draw data containing vertices, indices, and draw commands
- `FontTexture`: Font texture atlas used by ImGui
- `FontTextureWidth`/`FontTextureHeight`: Dimensions of the font texture

### ImGuiVertexRenderer
The main renderer class that:
- Processes ImGui DrawData structures
- Creates and manages vertex/index buffers
- Sets up proper pipeline state with alpha blending
- Renders ImGui vertices using DiligentEngine

### SimpleImGuiSceneObject
A basic implementation of `IImGuiSceneObject` that can be used as-is or as a base class.

## Usage Example

```csharp
// 1. Create an ImGui scene object
var imguiSceneObject = new SimpleImGuiSceneObject();

// 2. Set up ImGui font texture
var io = ImGui.GetIO();
io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height);
imguiSceneObject.SetFontTexture(pixels, width, height);

// 3. In your render loop, after ImGui.Render():
imguiSceneObject.UpdateDrawData(ImGui.GetDrawData());

// 4. The renderer will automatically be created by SceneObjectRenderersCreator
// when the scene object is added to the scene
```

## Shaders

The renderer uses two HLSL shaders:
- `imguiVS.vsh`: Vertex shader that transforms ImGui vertices
- `imguiPS.psh`: Pixel shader that samples the font texture and applies colors

## Requirements

- ImGui.NET package (version 1.89.7.1 or compatible)
- DiligentEngine for graphics rendering
- Support for alpha blending and scissor testing