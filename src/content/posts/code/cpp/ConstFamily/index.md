---
title: C++中const、constexpr、consteval、constinit的区别
description: const，用来定义常量，即值在创建后不会改变
published: 2024-05-19 11:26:49
category: '编程'
tags: [C++]
---

const，用来定义常量，即值在创建后不会改变，但const声明的常量发生在运行时，因此编译期是无法知道这个值的。于是，C++11中增加了constexpr，C++20中又增加了consteval和constinit。

<!-- more -->

## constexpr 
constexpr可以用来修饰函数和常量，与const不同的地方在于，constexpr修饰的常量必须是编译期明确知道的值。
constexpr修饰的函数，若在编译期可以得出返回值，则可以作为编译期可获得结果的常量使用，否则为运行时获得结果的函数。
下面简单举例说明：
```c++
constexpr int Sum(int a, int b) {
	return a + b;
}

void main() {
	// 若参数为编译期常量，则该函数可以作为编译期可获得结果的常量
	Sum(1, 2);

	// 若参数为变量，则该函数不能在编译期获得结果，即不可当作编译期常量使用。
	int a = 1;
	int b = 2;
	Sum(a, b);
}
```

## consteval 
consteval只能用于修饰函数，使用该函数时必须在编译期能够获取返回值，否则编译不能通过。

## constinit 
constinit与上述都不同，用来约束目标必须是在static生命周期下的目标，用以确保变量在程序初始化时就已经能够获得其值。
