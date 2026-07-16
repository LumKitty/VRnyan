## VRnyan - A camera sync plugin for VNyan and VR Mixed reality (formerly LIVnyan)
Syncs your VNyan camera over to OnAirTap, or LIV mixed reality  
Allows VNyan to be your VTuber renderer allowing for high quality models and full redeem support - no more VRMs or separate models  

Please read these instructions carefully before trying to set this up!

<img width="739" height="687" alt="image" src="https://github.com/user-attachments/assets/fd3f06aa-c5dc-40f6-a8c5-22af1a10fa53" />

## Overview
It is important to understand how this plugin works. Normally LIV will split the VR scene into effectively three image layers, background, glow and foreground. It will then build up a composite image: Background, VRM avatar, glow layer and foreground. This setup works by capturing each layer separately, and compositing them in OBS, with VNyan inserted between them, which requires OBS knowledge and a one-time initial setup

## Prerequisites
* VNyan
  * SteamVR tracking configured and working
  * Spout2 output configured and working
* OBS
  * [Spout2](https://github.com/Off-World-Live/obs-spout2-plugin) plugin
  * [Advanced Mask](https://obsproject.com/forum/resources/advanced-masks.1856/) plugin
* OnAirTap, or LIV VR
  * OnAirTap specific
    * Understanding of how to mod your chosen game (BepInEx or BSIPA, more to come soon)
    * Your game uses the LIV 1.5 SDK (If LIV_Bridge.dll is present it should work
    * Support for LIV 2.0 SDK (LIV_Native.dll) is currently experimental (tested in Beat Saber URP beta)
  * LIV VR specific
    * Mixed Reality Avatar mode configured and working
    * (Optional) A VRM file with the same rigging as your VSFAvatar
* General
  * A sufficiently powerful PC
  * Understanding of OBS filters
  * Understanding of the Windows filesystem, DLL files, JSON files etc.

# Installation

If you will be using VRnyan with LIV as opposed to OnAirTap, see the [old readme](https://github.com/LumKitty/VRnyan/blob/75c16d1740194ccde33a34325ea701e50ad6b1f8/README.md)

## VNyan plugin installation
* Ensure that plugins are enabled in VNyan (Settings -> Misc -> Additional Settings - Allow 3rd party mods)
* Download the latest version of VRnyan from the releases page
* Copy VRnyan.dll into VNyan\Items\Assemblies
* If you are upgrading from LIVnyan, delete vnyan-liv.dll. This version will not function until you remove it, but has full backwards compatability with LIVnyan
* Configure SteamVR and calibrate your trackers in the usual way
* Ensure the plugin is active by one of the following methods
  * Click the button in the VNyan plugins screen to toggle enable/disable
  * Call the ```_lum_vr_enable``` trigger
  * Edit VRnyan.cfg to set ActiveOnStart = true and restart VNyan
* If activated successfully, your model should appear slightly more zoomed out than normal. This is expected behaviour. When deactivated it will revert to default.

## OnAirTap installation - BepInEx
* Download the latest version of [OnAirTap](https://github.com/milkydelta/OnAirTap) - Get the OpenBrush build
* Install the latest version of [BepInEx](https://github.com/bepinex/bepinex) following its instructions
* Install the mod by unpacking the OpenBrush build. If you are attempting to use this with a different game you will need to use a different directory for OAT_KlakSpout.dll instead of OpenBrush_Data. Typically it will be GameName_Data
* Start the game once and quit
* The config file is located in: <GAMEDIR>\BepInEx\config\OnAirTap.cfg

## OnAirTap installation - Beat Saber
* Download the latest version of [OnAirTap](https://github.com/milkydelta/OnAirTap) - Get the BSIPA build
* Install the mod by unpacking it to your Beat Saber directory. If installed correctly, you should have OnAirTap.dll in the plugins directory, and OAT_KlakSpout.dll in Beat Saber_Data\Plugins\x86_64
* Start the game once and quit
* The config file is located in UserData\OnAirTap.json

# Configuration - VNyan
Edit VRnyan.cfg and set the following:
```
"BoneClip": "Hips",
"BoneClipDistanceAdjust": "-0.15",
"BoneClipDistanceAdjust2DOnly": "True"
```

# Configuration - OnAirTap
For full details of OnAirTap's setups see the github page. This document only covers the two most common scenarios. Three-pass is required for games that use glowing emissions (e.g. Beat Saber walls), if your game does not have these, two-pass will work and is more efficient.

## Three-pass configuration - for games that use glow effects (use this for Beat Saber)
Set the following settings in OnAirTap's config file
```
"ShouldRenderBG": true,
"ShouldRenderFG": true,
"ShouldRenderOptimised": true,
"ShouldReadResolutionFromMMF": true,
"ShouldReadTrackerFromMMF": true,
"ShouldSendBG": true,
"ShouldSendFG": true,
"ShouldSendOptimised": true,
"BlankSpoutOnRenderDispose": false,
"MMFProtocolMinorVersion": 1,
```

## Two-pass configuration - for games that do not use glow effects (do not use this for Beat Saber)
Set the following settings in OnAirTap's config file
```
ReadWindowResolution = true
ReadClipPlaneLocation = true
ProtocolMinorVersion = 1
RenderBackground = true
RenderForeground = true
RenderOptimised = false
SendBackground = true
SendForeground = true
SendOptimised = false
BlankSendersOnRenderDispose = true
```

## Configuration - OBS
You will need Spout2 and Advanced Mask installed, then:
* Create a Spout2 capture named "OnAirTap-BG". Choose the SpoutSender "OnAirTap Background", with Composite Mode set to Opaque
* Immediately above this, create a Spout2 capture for VNyan with Composite Mode set to default
* Create a Spout2 capture named "OnAirTap-FG". Choose the SpoutSender "OnAirTap Foreground", with Composite Mode set to Opaque
* Right click OnAirTap-FG -> Blending Mode -> Add
* Right click OnAirTap-FG -> Filters
* Hit the + button and add an effect filter "Advanced Mask". Set the following settings:  
  Mask Effect: Alpha Mask  
  Mask Type: Source  
  Source: VNyan  
  Scaling: Manual  
  Scale By: Percent (preserve aspect ratio)  
  Scale: 100%  
  Filter On: Alpha Channel  
  (all other settings should be default)
* If you are using the two-pass setup: Create a Spout2 capture named "OnAirTap-FG2". Choose the SpoutSender "OnAirTap Foreground", with Composite Mode set to Default
* If you are using the three-pass setup: Create a Spout2 capture named "OnAirTap-OP". Choose the SpoutSender "OnAirTap Optimised", with composite mode set to default
* (Optional) group these up.

The end result should look like this: (3-pass setup shown)  
<img width="173" height="85" alt="image" src="https://github.com/user-attachments/assets/56afc95d-6619-42cd-a6a6-529d4c503db9" />

The FG layer advanced mask config should look like this:
<img width="1090" height="859" alt="image" src="https://github.com/user-attachments/assets/08c96e61-891d-496e-a917-560ad72f4084" />

## Notes on calibration
Many VRoid models have their arms too short. The closer your model matches your IRL height and proportions the less likely you are to have weapons fly away from your hand when your arms are outstretched. Elbow IK should also work better.  
For games that don't show objects/weapons in your hand, these calibration issues will be less noticible.  
The zoom out effect is because VNyan uses a "physical camera" setting that emulates the distortion curved lens of an IRL camera to give a more natural look. LIV does not do this and cannot easily be changed. While this could theoretically be changed in OnAirTap, generally games look better without it, and different versions of Unity may behave differently

## Use in VNyan
Clicking the plugin button toggles the system on and off. While it is off in VNyan, OnAirTap will not render any spout frames, therefore it is not necessary to remove OnAirTap when not streaming  
### Triggers:  
```_lum_vr_enable``` - enable camera sync  
```_lum_vr_disable``` - disable camera sync  
```_lum_vr_setbone``` - Choose the bone to be used for dynamic clip plane adjustment  
Text1 - Name of the bone to use. Must be a bone from [this list](https://docs.unity3d.com/ScriptReference/HumanBodyBones.html) and is case sensitive  
Text2 - Adjustment in meters. Negative values will move the clip plane closer to the camera, positive will be further away.
See the "Dynamic Clip Adjustment" section for more details on this

# Final Checks before going live 
* Both game and VTuber are displaying correctly
* Check your OBS scene and ensure that your model appears in the correct place, and that all four layers are in the same location
* Drag the camera around in the VNyan window, and check that the game world in OBS follows it
* Are foreground objects and glowing objects correctly passing in front of your model (if using three pass)
* Are your feet being chopped off (See the troubleshooting section for this)
* Have any physics on your model glitched while you were doing all this? (reload avatar if so)

# Lum's recommendations
These are completely optional, but are how I do things
* Other plugins that may be useful:
  * [NyanSaber](https://github.com/LumKitty/NyanSaber) - Get BeatSaber data as VNyan triggers
  * [VRCFTnyan](https://github.com/LumKitty/VRCFTnyan) - Get face tracking data from the VRCFaceTracking app
  * [Sjatar's Stylistic Screen Light plugin](https://github.com/Sjatar/StylisticScreenLight) - Configure it to use OnAirTap's Optimised spout sender as its light source
  * [Lum's Extras](https://github.com/LumKitty/LumsExtras/) - has an option to disable manually resizing the VNyan window, which is otherwise easy to do by accident in VR!
* Have an "EnableVR" websocket trigger that enables SteamVR and LipSync, disables ARKit & LeapMotion. Also have a "DisableVR" websocket that does the opposite. Put these on a toggle button on your stream deck.
* If you have physics, especially skirts, and will be taking stream breaks: have a scene change button for your VR scene that waits for ~4 seconds, then reloads your avatar, waits another second and then switches scene. This will minimise any issues with skirt clipping caused by pretzeling, giving you time to get to a neutral position in your VR space before forcing a physics reset
* If you use VoiceMeeter, have both your regular microphone and your VR headset microphone routed to the same virtual audio cable. Connect OBS, discord etc. to this virtual cable, then have a stream deck button that mutes one and unmutes the other, and vice versa
* Also in VoiceMeeter, have a virtual audio cable for discord comms and/or redeems, have this routed to both your regular headphones and your VR headset
* VoiceMeeter again: Have a "Rescan audio" button that runs ```Voicemeeter.exe -r``` Since the Index headset is generally not connected until you launch SteamVR, VoiceMeeter will not pick it up as a valid sound target on launch, but forcing a rescan like this fixes that without having to manually kill and re-open it
* If your are playing a rhythm game and are routing your audio through VoiceMeeter, carefully check your audio calibration. For me the lag created by voicemeeter is 120ms, which is quite significant
* Configure VNyan to switch to your preferred resolution on launch, then use my [Extras Plugin](https://github.com/LumKitty/LumsExtras/) to lock VNyan's window size. Preventing accidental resizing errors when you're trying to do an emergency recalibrate
* Use XSOverlay or similar to show your OBS preview window within VR, and check it frequently, especially if you have physics on your model
* Take full advantage of VNyan's features. Go wild with redeems, Poiyomi shaders, Magica cloth. You put all the effort in to set this up, so make use of it!
* Follow LumKitty on https://twitch.tv/LumKitty :3

## Troubleshooting
### In-game objects move before your VTuber
Add a Render Delay filter to all OnAirTap captures in OBS (see the Alignment timing section below)
### During camera pans, your avatar moves before the game world
Enable the "Cursed Camera" delay feature (see the "Alignment timing" seciong below)

# Advanced configuration

## VNyan Configuration options
Settings are stored in VRnyan.cfg inside your VNyan profile directory (default %APPDATA%\..\LocalLow\Suvidriel\VNyan). This file will appear the first time you start VNyan with VRnyan installed. Any changes will only take effect after restarting VNyan  
```ActiveOnStart``` - Camera sync will start as soon as VNyan loads  
```LogEnabled``` - The VNyan plugin will log to player.log in the main VNyan profile directory. The LIV plugin will log to
%USERPROFILE%\Documents\LIV\Plugins\CameraBehaviours\LIVnyan.log  
```LogSpam``` - Both plugins will log sent/recieved camera position, rotation & FOV, plus settings info every single frame.  
These logs will get very big very quickly. Only enable this for troubleshooting!  
```CursedCamera``` - Adds a delay to all camera movements in VNyan, while still updating LIV in realtime. Fixes the issue during fast pans where your model moves ahead of the VR world. See the Advanced Timing section before changing this option
```BoneClip``` - Will instruct OnAirTap to use the specified bone for determining where the split between foreground and background is. This allows you to correctly pass in front or behind objects, even in large VR spaces. Must be a bone from [this list](https://docs.unity3d.com/ScriptReference/HumanBodyBones.html) and is case sensitive  
```BoneClipDistanceAdjust``` - Add this value (in meters) to the bone position. Negative numbers will be closer to the camera. Generally you will want to set this to the radius of your model's pelvis so that clipping lines up with your bum
```BoneClipDistanceAdjust2DOnly``` - If true then the vertical position of the camera will not be considered, only front/back and side to side, generally this is what you want

## Alignment timing
Because this setup is merging two separate video feeds from two different sources, there is going to be some manual sync needed to get the best results.  
I recommend using a game where you are always holding an object, such as Beat Saber, and recording these tests using OBS
### Aligning hands to weapons
First move your hands around and observe that your saber moves ahead of your hands.  
Using a video player that allows you to step through frame by frame (I use AVIDemux), make a note of how many frames it takes for your hand to reach the saber. Multiply this by 16.667
For each OnAirTap layer in OBS, add a "Render Delay" filter set to this value
Test and record again, adjust camera latency as necessary, repeat until you are 100% happy with the result. Do not go on to the next step until then.  
My personal setting is 100. YMMV
### Aligning camera pans
If you will only be using a static camera this step is not strictly necessary.  
Create a temporary set of nodes that switch between camera positions with a transition time of around 1000ms, start recording and fire these nodes
If your model moves ahead of the VR world then you will need to use the "Cursed Camera" feature. This feature intercepts VNyan camera movement, sends it to OnAirTap and then forces a camera movement delay in VNyan. An unavoidable side effect of enabling this feature is that manual camera movements will feel laggy
As before, count how many frames it takes for LIV's camera to catch up to VNyan's. The result will likely be the same (or very close) to the LIV camera delay you set in the previous step.
call ```_lum_vr_enable``` passing this result into value1 and move the camera again. Hopefully it now lines up, but if necessary re-record and adjust until it's perfect
Once you have the final value, close VNyan, edit VRnyan.cfg and set Cursed camera there to make the change permanent 
Remember that if you change the render delay, you will also need to adjust the Cursed Camera latency by an equivalent amount  
My personal setting is 100 for LIV, 72 for OAT. Again YMMV

## Dynamic clip adjustment
The best way to describe this, imagine the 3D world of your game were sliced in half directly in front of the camera, and then the video feed from VNyan is inserted into this slice. Whether you appear in front or behind an object depends on exactly where this slice is, normally this is static. With dynamic clip adjustment configured, as you move around inside VNyan this slice position will be updated every frame, meaning you will always correctly appear in front of or behind objects in your game world. To do this we need to send position information over to OnAirTap. The settings that control this are the BoneClip options.  
The BoneClip settings tells VRnyan which bone to read position from, typically you will want to use hips, however the bone is inside your body so you will want to adjust it slightly towards the camera, with a little overhead for e.g. walking animations. -0.15 is a good starting value

## Support
Either open an issue here (preferred), or ask in the LIVnyan thread on [Suvi's Discord](https://discord.com/channels/714814460010823690/1373846127975207002)

## Shameless plug
### https://twitch.tv/LumKitty
If you find this plugin useful, please consider sending a follow or a raid my way. If you somehow make millions using it, consider sharing some of that with me :3
