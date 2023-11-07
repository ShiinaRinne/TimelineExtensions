# TimelineExtensions
English | [中文](README_CN.md)

## Introduction

Some extensions for Unity Timeline. 
You can edit properties in the Timeline more easily **without writing code**,<br> 
or help you quickly develop prototypes.

[//]: # (This project was originally developed mainly to expand the post-processing volume, 
and will gradually improve other types in the future.)

At present, I have expanded Unity's original Volume, you can use it directly, 
or use "**MAO Timeline Playable Wizard**" to quickly expand.

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

>`BoolParameter` and `TextureParameter` may not be of much use, more like my bad taste, their mixing method is: <br>
when the **mixing weight > 0.5**, it becomes the value of the next Clip, usually you can ignore it.


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
2. Create a new Global Volume.
3. Add a new Track which starts with "MAO", such as `MAOBloom`.
4. Set TrackBinding to the Volume you need. (**Very important**, after binding, the newly created Clip will get
   the parameters in the Volume as default values).
5. Add a new Clip to the Track, edit properties in the Clip or mix with other Clips.<br>


### Wizard Usage
This is a tool that can quickly generate Timeline extensions for you. <br>
It can directly obtain all classes which under current AppDomain and get the required fields through reflection.<br>
You no longer need to fill in manually.

>Currently Post-Processing Volume is supported, and the **`Component type` has not been developed yet !!!**.

#### VolumeComponent:
1. You can find it in the menu bar `Window/MAO Timeline Playable Wizard`

2. Switch `WorkType` to VolumeComponent, select the `Track Binding Type`

   ![](https://r2.youngmoe.com/ym-r2-bucket/2023/11/19e8b6032028290d224b7fadef049284.png)

3. Set `Default Values` to the Volume

   ![](https://r2.youngmoe.com/ym-r2-bucket/2023/11/7a228f2972434178c205c8aaf67a6b0b.png)

4. Add the properties

   ![](https://r2.youngmoe.com/ym-r2-bucket/2023/11/14b3980e06f8d6cb0b87f9e74eb025e4.png)

5. Finally, click `Create`, wait for the compilation to complete and start enjoying~<br>
You can find it in `Assets/TimelineExtensions`

<details>
<summary>The Enum type is currently not supported. When you need it (such as Gaussian or Boken's DOF), it is recommended to create it in the following way</summary>
</details>


**If you have any problems during use, please report an issue.**



## TODO
- [x] Add attributes like `[Range()]`, `[Min()]`, `[Max()]` to the properties of the Clip.
- [ ] Optimize attribute adding method in "MAO Timeline Playable Wizard".
- [ ] Add support for more parameters.
- [ ] Support high-level settings such as `Blend Curves`, `Easing-in and Easing-out`.

## License
[MIT License](https://github.com/ShiinaRinne/TimelineExtensions/blob/master/LICENSE)

## Credits
- [Default Playables - Unity Technologies](https://assetstore.unity.com/packages/essentials/default-playables-95266)