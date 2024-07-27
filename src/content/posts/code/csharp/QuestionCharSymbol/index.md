---
title: C#中问号“?”在语法中的作用以及在Unity中的注意事项
published: 2021-10-15 13:42:34
category: '编程'
tags: [Unity]
---

在C#语法中,问号”?“用来判断一个对象是否为空,如果不为空就继续调用,否则不执行任何操作

<!-- more -->

**使用方法**
假设有一个对象
```csharp
public class MyObject {
    public string value { get; set; } = "MyObject";
    public void Print() {
        Debug.Log(value);
    }
}
```
模拟一个情景
```csharp
MyObject obj = null;
// 这里假如随机到0则创建对象,否则不创建
if (Random.Range(0, 2) == 0) {
    obj = new MyObject();
}
// 如果obj不为null则调用Print
if (obj != null) {
    obj.Print();
}
```
使用问号“?”语法替代上面的情景
```csharp
// 如果obj不为null则调用Print
obj?.Print();
```
这样起到一个简化代码的作用

**“??”的使用**
还是上面的`MyObject`
```csharp
string tempValue = obj?.value??"temp";
```
上面的代码相当于
```csharp
string tempValue;
if (obj != null) {
    tempValue = obj.value;
} else {
    tempValue = "temp";
}
```

**在Unity中使用时需要注意**
```csharp
public class MyAnimation : MonoBehaviour {
    public void Play() {
        // ...
    }
}
```
假如我们需要获取一个自定义组件,但并不知道这个组件是否存在,例如:
```csharp
var myAnimation = GetComponent<MyAnimation>();
myAnimation?.Play(); // 如果存在则调用Play方法
```

上面的调用是没有问题的,但是需要注意的是,在Unity中调用引擎自带的组件时,那么问题就来了,例如:
```csharp
var ps = GetComponent<ParticleSystem>();
ps?.Play(); // 这里会报错
```
报错如下:
![error_img](img1.png)

按道理说,`ps?.Play();`的调用,如果没有找到则不执行,但问题就在,实际上这个`ps`对象并不是真正的`null`

**Unity在使用`GetComponent`调用内部组件时,即使当前对象没有包含该组件,也不会返回`null`,而是返回一个UnityEngine.Object意义上的"空"对象,而UnityEngine.Object重写了==,!=相关的操作符,使得我们使用if去判断时可以正确执行,而使用`?`符号去执行时并不会调用重写的操作符因此就出错了**

**所以在使用`GetComponent`调用内部组件时,不要再使用`?`符号了,对于自己写的脚本则可以继续使用**

