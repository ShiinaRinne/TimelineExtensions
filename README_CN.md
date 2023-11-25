[English](README.md) | 简体中文

## Introduction

这个库包含一些对Unity Timeline的扩展。<br>
可以在不编写代码的情况下更轻松的通过Timeline编辑Volume或Object属性，或者快速开发原型。

目前这个repo里有一些Unity URP原有的后处理Volume的扩展，用来在Timeline中动态调节Volume<br>
可以直接导入项目使用, 也可以通过”**MAO Timeline Playable Wizard**”这个工具自行扩展。

![](https://r2.youngmoe.com/ym-r2-bucket/2023/11/fb552984c57c7f0d554303d97d4387c6.gif)

## Features

### 目前在Volume模式中支持的、可用的参数：

- `FloatParameter`
- `IntParameter`
- `BoolParameter`
- `Vector2Parameter`
- `Vector3Parameter`
- `Vector4Parameter`
- `ColorParameter`
- `TextureParameter`

### 目前不支持或没有经过完全测试的：

- `Enum`(Example:`Film Grain's Type, Motion Blur's Quality, Tonemapping's Mode`)
- `LayerMaskParameter`
- `FloatRangeParameter`
- `RenderTextureParameter`
- `CubemapParameter`
- `ObjectParameter`
- `AnimationCurveParameter`
- `TextureCurveParameter`

## Usage

### Typical usecase

1. 打开Timeline窗口，创建一个新的Timeline
2. 在Scene中创建一个Volume，添加 `TimelineExtensionVolumeSettings` 组件
3. 在Timeline中添加一个新的Track。如果是直接使用的这个repo，它的名字应该以`MAO`开头，例如`MAOBloom`
4. 将创建的 `TimelineExtensionVolumeSettings` 组件绑定到这个Track上
5. 在Track中添加新的Clip，编辑属性，或者与其他的Clip进行混合即可

#### `TimelineExtensionVolumeSettings` 组件设置:
- `VolumeAccessType`:
   - `Profile`: 访问 `profile` 的副本，不会影响原本的 `volume profile`文件（但编辑模式下通过 Timeline 控制之后，手动调节的 Volume 参数无法保存）
   - `Shared Profile`：访问 `profile` 的引用，做的修改会直接影响到原本的 `volume profile` 文件，类似于 Editor 模式下修改 Volume 属性。当退出运行模式后无法重置设置
   
   推荐在 Editor 模式下使用 `Shared Profile`，在 `Play` 模式下使用 `Profile`<br>
   如果需要使用这种方式，可以勾选 `AutoSwitchType` 自动切换<br>
   更多信息可以参考 [Unity官方文档](https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@16.0/manual/Volumes-API.html#access-a-shared-volume-profile)

### Wizard Usage

这是一个可以快速生成Timeline扩展的工具<br>
它可以直接获取当前AppDomain下的所有类，并通过C#反射来获取需要的字段，这样就不再需要自己写扩展了~

> 现在仅支持 `VolumeComponent`，`Component`模式尚未开发完成
>

**Volume Component：**

1. 在 `Window/MAO Timeline Playable Wizard`打开
2. 切换 `WorkType`为 `VolumeComponent`，选择需要的 `Track Binding Type`

   ![](https://r2.youngmoe.com/ym-r2-bucket/2023/11/19e8b6032028290d224b7fadef049284.png)

3. 将Default Values设置为Volume

   ![](https://r2.youngmoe.com/ym-r2-bucket/2023/11/7a228f2972434178c205c8aaf67a6b0b.png)

4. 添加属性

   ![](https://r2.youngmoe.com/ym-r2-bucket/2023/11/14b3980e06f8d6cb0b87f9e74eb025e4.png)

5. 最后点 `Create`就可以了，等编译完之后就可以使用，你可以在 `Assets/TimelineExtensions`找到生成的脚本

> 使用过程中有任何问题请发issue~
>

## TODO

- [x]  在生成时，自动获取`[Range()]`,`[Min()]`,`[Max()]`这些属性
- [x]  优化添加属性时候的操作
- [ ]  添加对更多类型参数的支持
- [ ]  支持一些高级设置，例如`Blend Curves`,`Easing-in and Easing-out`.

## License

[MIT License](https://github.com/ShiinaRinne/TimelineExtensions/blob/master/LICENSE)

## Credits

• [Default Playables - Unity Technologies](https://assetstore.unity.com/packages/essentials/default-playables-95266)


[//]: # (## 彩蛋)
[//]: # (我不是在给爱莉生日做视频吗！为什么最后做了这个东西出来！我的爱莉呢！！！)

[//]: # (## 彩蛋2)
[//]: # (一年过去了，爱莉还是没有来到我身边QAQ)