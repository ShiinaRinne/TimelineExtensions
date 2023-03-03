[English](README.md) | 简体中文

## Introduction

这个库包含一些对Unity Timeline的扩展。<br>
可以在不编写代码的情况下更轻松的通过Timeline编辑属性，或者快速开发原型。

在这个repo里，我扩展了一些Unity原有的后处理Volume，可以直接导入项目使用,<br>
也可以通过”**MAO Timeline Playable Wizard**”这个工具来自行扩展。

![](https://pic.youngmoe.com/1668700075_202211172347274/637657ab5d00f.gif)

## Features

### 目前在Volume中支持的、可用的参数：

- `FloatParameter`
- `IntParameter`
- `BoolParameter`
- `Vector2Parameter`
- `Vector3Parameter`
- `Vector4Parameter`
- `ColorParameter`
- `TextureParameter`

> `BoolParameter`和`TextureParameter` 可能没什么用处，是我闲的没事硬加上去的，
> 混合方式为：当Clip的混合权重>0.5时，会变为下一个Clip的值，一般来说不必管它~

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
2. 在Scene中创建一个Volume
3. 在Timeline中添加一个新的Track。如果是直接使用的这个repo，它的名字应该以`MAO`开头，例如`MAOBloom`
4. 将你创建的Volume绑定到这个Track上
5. 在Track中添加新的Clip，编辑属性，或者与其他的Clip进行混合即可

### Wizard Usage

这是一个可以帮你快速生成Timeline扩展的工具

它可以直接获取当前AppDomain下的所有类，并通过C#反射来获取需要的字段，这样你就不再需要自己写扩展了~

> 现在仅支持 `VolumeComponent`，`Component`模式尚未开发完成
>

**Volume Component：**

1. 在 `Window/MAO Timeline Playable Wizard`打开
2. 切换 `WorkType`为 `VolumeComponent`，选择需要的 `Track Binding Type`

   ![](https://pic.youngmoe.com/1668613341_202211162342576/637504dd561ec.png)

3. 将Default Values设置为Volume

   ![](https://pic.youngmoe.com/1668614619_202211170003969/637509dbbd789.png)

4. 添加属性

   ![](https://pic.youngmoe.com/1668613472_202211162344770/63750560bcd75.png)

5. 最后点 `Create`就可以了，等编译完之后就可以使用，你可以在 `Assets/TimelineExtensions`找到生成的脚本

> 使用过程中有任何问题，都可以在Github发个issue~
>

## TODO

- [x]  在生成时，自动获取`[Range()]`,`[Min()]`,`[Max()]`这些属性
- [ ]  优化添加属性时候的操作
- [ ]  添加对更多类型参数的支持
- [ ]  支持一些高级设置，例如`Blend Curves`,`Easing-in and Easing-out`.

## License

[MIT License](https://github.com/ShiinaRinne/TimelineExtensions/blob/master/LICENSE)

## Credits

• [Default Playables - Unity Technologies](https://assetstore.unity.com/packages/essentials/default-playables-95266)


[//]: # (## 彩蛋)

[//]: # (我不是在给爱莉生日做视频吗！为什么最后做了这个东西出来！我的爱莉呢！！！)