# ailia-android-uvc-camera

Unity on Android cannot handle external USB cameras. You can use an external USB camera by using UVC4UnityAndroid.

## Architecture

In UVC4UnityAndroid, you can get the camera image by dropping UVCDrawer.prefab to Scene and giving UI/RawImage to RenderTargets of prefab.

GetPixels32 can be executed by blitting the RawImage texture once and giving it Read/Write attributes. Infer the obtained pixel value with ailia.

## Setup

Install ailia SDK unity package (Plugins folder only)

https://axinc.jp/trial/

Install 'NuGet'(NuGetForUnity).
- Select Manage NuGet Packages from NuGet menu.
- Search System.Text.Json from search box.
- Select System.Text.Json and install it.

https://github.com/GlitchEnzo/NuGetForUnity/releases

Import release package of UVC4UnityAndroid

https://github.com/saki4510t/UVC4UnityAndroid/tree/master/release

## Tutorial

The folder structure after import is as follows.

![Tutorial/folder.png](Tutorial/folder.png)

When I open the project, it looks like this.

![Tutorial/project.png](Tutorial/project.png)

The image of the camera is drawn to RawImage.

![Tutorial/rawimage.png](Tutorial/rawimage.png)

Set your Render settings to OpenGL, as it won't work with Vulkan.

![Tutorial/render.png](Tutorial/render.png)

## Run

Open and Run unity scene.

/Assets/AXIP/AILIA/Samples/Scenes/ailia_detector.unity


## Prebuilt app

You can download prebuilt binary here.

[ailia_uvc_camera.apk](Release/ailia_uvc_camera.apk)
