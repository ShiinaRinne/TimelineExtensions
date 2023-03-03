# TimelineExtensions
English | [简体中文](README_CN.md)

## Introduction

Some extensions for Unity Timeline. 
You can edit properties in the Timeline more easily **without writing code**,<br> 
or help you quickly develop prototypes.

[//]: # (This project was originally developed mainly to expand the post-processing volume, 
and will gradually improve other types in the future.)

At present, I have expanded Unity's original Volume, you can use it directly, 
or use "**MAO Timeline Playable Wizard**" to quickly expand.

![](https://pic.youngmoe.com/1668700075_202211172347274/637657ab5d00f.gif)

## Features
### Now tested parameters that work fine:
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

2. Switch `WorkType` to VolumeComponent, select the VolumeComponent

   ![](https://pic.youngmoe.com/1668613341_202211162342576/637504dd561ec.png)

3. Set `Default Values` to the Volume

   ![](https://pic.youngmoe.com/1668614619_202211170003969/637509dbbd789.png)

4. Add the properties

   ![](https://pic.youngmoe.com/1668613472_202211162344770/63750560bcd75.png)

5. Finally, click `Create`, wait for the compilation to complete and start enjoying~<br>
You can find it in `Assets/TimelineExtensions`

<details>
<summary>The Enum type is currently not supported. When you need it (such as Gaussian or Boken's DOF), it is recommended to create it in the following way</summary>

![](https://pic.youngmoe.com/1668615739_202211170022942/63750e3bb10b2.png)

![](https://pic.youngmoe.com/1668615893_202211170024445/63750ed564189.png)
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