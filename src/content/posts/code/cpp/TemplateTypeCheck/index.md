---
title: C++泛型编程之类型的静态与动态检查
description: 假如某个模板我希望传入的参数必须是int而不能是float时,如果不对就不允许通过编译怎么做?
published: 2020-08-11 11:15:41
category: '编程'
tags: [C++]
---

著名C++大师Scott在`<<Effective C++>>`一书中说过,C++可以看成是一个语言联邦,由4部分组成,分别是C语言,面向对象,STL标准模板库和泛型(模板).由此可见泛型在C++中的地位.
使用模板可以减少大量代码重复,可以使方法在使用上更加通用和舒适.但使用模板仍然不可避免会出现一些问题,比如我希望传入的参数必须是const char*而不能是std::string时.所以本篇文章讲解如何实现模板类型检查.

<!-- more -->

## 模板特化  
要想实现对类型的检查,这里就得用到模板特化.模板特化就是给模板特例,针对你指定的类型生成不同的代码.

例如:
```c++
// 模板方法
template <typename T>
bool Equal(T a, T b) {
    return a == b;
}
// 特化版本
template <>
bool Equal<char*>(char* a, char* b) {
    return (strcmp(a, b) == 0);
}
```
`Equal`方法用于判断两个值是否相等,但是对于C字符串时,我们不能直接使用`==`来判断,所以需要特化一个版本,当类型为char*的时候,就会调用这个特化版本.

## 静态检查  
静态检查就是在编译期间检查,若有问题则无法通过编译,并提示目标位置,一般原则是能在编译期间解决的问题则不应该放到运行时解决,所以最好是能够在编译期就阻止一切出错的可能.

那么对于模板类型检查,也是可以放到编译期间来检查的,当传入的参数不符合要求时,则不能通过编译,下面以一个简单的例子来说明:
```c++
// 一个模板类
template <typename T>
struct CheckType { };
// 特化一个int版本
template <>
struct CheckType<int> {
    enum { IsInt = 1 };
};
```
针对上面的例子,就可以静态检查类型是否为`int`了,例如有一个模板,我们限制参数必须为int时,就可以这么用了:
```c++
template <typename T>
void Test(T value) {
    static_assert(CheckType<T>::IsInt, "The type isn't int");
}
```
当类型不为int时,因为根本没有`IsInt`这个值,因此就会报错,并打印信息,根据编译器的报错就可以轻松找到出错的位置了,那对于其他类型,就需要一一特化,这里就不详细写了

## 动态检查  
有时候,静态检查并不能满足需求,需要在运行时检查,代码如下:
```c++
template <typename T1, typename T2>
struct CheckTypeSame {
	enum { value = 0 };
};
// 特化版本
template <typename T>
struct CheckTypeSame<T, T> {
	enum { value = 1 };
};
```
当两个类型不同时,会调用通用版本,因此`value`的值为0, 当类型相同时则会走特化版本,此时`value`值为1,所以通过判断`value`的值就可以确定两个类型是否一样了,调用方法如下:
```c++
// 这里用一个简单例子说明动态判断类型
template <typename T>
void Print(T value) {
    if (CheckTypeSame<T, int>::value) {
        printf("value is int");
    } else {
        printf("value isn't int");
    }
}
```

