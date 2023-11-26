# TimelineExtensions
English | [中文](README_CN.md)

## Introduction

Some extensions for Unity Timeline. 
You can edit `Volume` or `GameObject`'s properties in the Timeline more easily **without writing code**,<br> 
or help you quickly develop prototypes.

[//]: # (This project was originally developed mainly to expand the post-processing volume, 
and will gradually improve other types in the future.)

At present, there are some extensions to the original post-processing volume of Unity URP, which are used to dynamically adjust the volume in the timeline<br>
It can be directly imported into the project for use, or quickly expand through the "**MAO Timeline playable Wizard**" tool.

![](https://r2.youngmoe.com/ym-r2-bucket/2023/11/fb552984c57c7f0d554303d97d4387c6.gif)

## Features
### Now tested parameters that work fine in Volume Mode:
- `FloatParameter`
- `IntParameter`
- `BoolParameter`
- `Vector2Parameter`
- `Vector3Parameter`
- `Vector4Parameter`
- `ColorParameter`
- `TextureParameter`


### Parameters not yet supported or tested:
- `Enum`(Example: `Film Grain's Type, Motion Blur's Quality, Tonemapping's Mode`)
- `LayerMaskParameter`
- `FloatRangeParameter`
- `RenderTextureParameter`
- `CubemapParameter`
- `ObjectParameter`
- `AnimationCurveParameter`
- `TextureCurveParameter`



[//]: # (Currently supported:)

[//]: # (- Bloom)

## Usage

[//]: # (### Download/Installation)

[//]: # ()
[//]: # (Get it from one of the following sources:)

[//]: # ()
[//]: # (- Download the latest release from the [releases page]&#40;&#41;.)

[//]: # (- Clone the repository: `git clone https://xx.git`.)

### Typical usecase

1. Open the Timeline window and create a new Timeline.
2. Create a new Global Volume, add `TimelineExtensionVolumeSettings` component.
3. Add a new Track which starts with "MAO", such as `MAOBloom`.
4. Set TrackBinding to the `TimelineExtensionVolumeSettings` component.
5. Add a new Clip to the Track, edit properties in the Clip or mix with other Clips.<br>

#### `TimelineExtensionVolumeSettings` component settings:
- VolumeAccessType:
   - `Profile`: Access a copy of the profile, which will not affect the original volume profile file (but if you adjust the Volume property through Timeline in Editor mode and then manually adjust it, this modification cannot be saved)
   - `Shared Profile`: Access a reference to the profile, which will directly affect the original `volume profile`. The settings cannot be reset after exiting play mode
   
> [!TIP]
> It is recommended to use `Shared Profile` in Editor mode and `Profile` in Play mode.<br>
> If you need to use this switching method, you can check `AutoSwitchType` in `TimelineExtensionVolumeSettings`<br>
> For more information, please refer to [Unity Documentation](https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@16.0/manual/Volumes-API.html)

### Wizard Usage
This is a tool that can quickly generate Timeline extensions for you. <br>
It can directly obtain all classes which under current AppDomain and get the required fields through reflection.<br>
You no longer need to fill in manually.


#### VolumeComponent:
1. You can find it in the menu bar `Window/MAO Timeline Playable Wizard`

2. Switch `WorkType` to VolumeComponent, select the `Track Binding Type`

   <img src="https://r2.youngmoe.com/ym-r2-bucket/2023/11/19e8b6032028290d224b7fadef049284.png" width="50%">

3. Set `Default Values` to the Volume

   <img src="https://r2.youngmoe.com/ym-r2-bucket/2023/11/7a228f2972434178c205c8aaf67a6b0b.png" width="50%">

4. Add the properties

   <img src="https://r2.youngmoe.com/ym-r2-bucket/2023/11/14b3980e06f8d6cb0b87f9e74eb025e4.png" width="50%">

5. Finally, click `Create`, wait for the compilation to complete and start enjoying~<br>
You can find it in `Assets/TimelineExtensions`

> [!IMPORTANT]
> The Enum type is currently not supported. When you need it (such as Gaussian or Boken's DOF), it is recommended to create it in the following way.<br>
> <img src="https://r2.youngmoe.com/ym-r2-bucket/2023/11/48d3bda1b26b762ac0477f2b94fc2a75.png" width="50%">
> <img src="https://r2.youngmoe.com/ym-r2-bucket/2023/11/8d325d458c0209b9068427474dce6377.png" width="50%">



## TODO
- [x] Add attributes like `[Range()]`, `[Min()]`, `[Max()]` to the properties of the Clip.
- [x] Optimize attribute adding method in "MAO Timeline Playable Wizard".
- [ ] Add support for more parameters.
- [ ] Support high-level settings such as `Blend Curves`, `Easing-in and Easing-out`.

## License
[MIT License](https://github.com/ShiinaRinne/TimelineExtensions/blob/master/LICENSE)

## Credits
- [Default Playables - Unity Technologies](https://assetstore.unity.com/packages/essentials/default-playables-95266)