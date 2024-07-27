---
title: Unity表面着色器笔记(二)
published: 2019-08-16 13:39:53
category: '编程'
tags: [Unity, Shader]
---

上一篇的内容量很少,但是疑问却有不少,比如属性,变量,输入/输出结构体,这一篇专门做一个解释.


本文:
- Properties属性类型
- Shader变量类型
- SurfaceOutput结构体
- Input结构体

<!-- more -->

---

上一篇:[Unity表面着色器笔记(一)](https://jenocn.github.io/2019/06/UnitySurfaceShader1/)

---

## Properties属性类型

```CG
Properties {
    // 基本数值类型
    _Int ("Int", Int) = 0
    _Float ("Float", Float) = 0
    // 数值范围类型,限定了数值的范围,可以在Unity Inspector窗口中通过滑动条赋值
    _Range ("Range", Range(0, 1)) = 0.5
    // 表示颜色
    _Color ("Color", Color) = (1, 1, 1, 1)
    // 表示一个思维向量
    _Vector ("Vector", Vector) = (0, 0, 0, 0)
    // 下面皆为纹理类型
    _2D ("2D", 2D) = "white" {}
    _3D ("3D", 3D) = "white" {}
    _Cube ("Cube", Cube) = "white" {}
}
```

## Shader变量类型  

数值类型有三类
- `float`表示高精度浮点数
- `half`表示中精度浮点数
- `fixed`表示低精度浮点数
- `int`整数类型
- `bool`bool类型
在以上数值类型的基础上,又有组合类型,例如:
- `float2,float3,float4`分别表示float类型的二维,三维,四维向量,其他类型同理
- `float4x4`表示4x4矩阵
表示纹理的类型
- `sampler2D, sampler3D, samplerCUBE`分别对应Properties中的2D,3D,Cube

## SurfaceOutput结构体  

```CG
struct SurfaceOutput {
    // 漫反射颜色
    fixed3 Albedo;
    // 法线向量
    fixed3 Normal;
    // 自发光颜色
    fixed3 Emission;
    // 高光值,范围是(0~1)
    half Specular;
    // 高光强度
    fixed Gloss;
    // 透明度
    fixed Alpha;
};
```

## Input结构体

```CG
struct Input {
    // 表示纹理坐标,必须uv打头
    float2 uv_MainTex;
    // 视图方向
    float3 viewDir;
    // 屏幕坐标
    float4 screenPos;
    // 世界坐标
    float3 worldPos;
    // 世界中的反射向量
    float3 worldRefl;
    // 世界中的法线向量
    float3 worldNormal;
};
```

参考文章:<a href="http://www.cnblogs.com/-867259206/p/5596698.html">http://www.cnblogs.com/-867259206/p/5596698.html</a>

---

下一篇:[Unity表面着色器笔记(三)](https://jenocn.github.io/2019/08/UnitySurfaceShader3/)

