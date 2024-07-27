---
title: 介绍一下自己写的GameScript
published: 2019-07-26 17:10:08
category: '编程'
tags: [Script]
---

因为想在项目中使用脚本,目前最常见的就是使用Lua,但是像XLua,NLua之类的太庞大了,多平台支持也很依赖官方给的lib库,因为在我的项目中使用脚本只是一小部分,比如剧情这部分.所以我只需要一个能够执行命令的脚本就已经满足我的需求,但是,有时候设计的时候,想的太多,写着写着就.....

<!-- more -->

---

## 介绍 
- 函数式结构 
- 基于.net开发,跨平台,支持Unity 
- 嵌入式,接入方便,简单易用
- 类C语法 

## 使用场合 
&emsp;&emsp;脚本不需要编译,被当做资源使用,所以优点是支持生产环境下修改,当某些内容需要频繁修改的时候,适合使用脚本,但是脚本的运行效率比较低,不适合高性能场合使用.  
&emsp;&emsp;我觉得,脚本更适合需要大量内容生产的环境使用,因为需要频繁修改,使用脚本可以提高开发效率,减少工作流程,比如游戏开发中的剧情部分.  
&emsp;&emsp;其实很多内容使用良好的结构+配置就已经可以做到很好的效果,不过使用脚本,更明显的优势是更容易的支持逻辑判断.  

## 函数式结构 
任何被执行的代码片段都看作为一个函数调用,因此一个脚本文件为一个函数,函数内可以嵌套定义函数,一个函数可看作为一个空间作用域,子作用域可以调用父作用域中的方法和变量

## 数据类型 
GameScript是动态类型,但本质上会根据值来动态确定类型
- `null`
- `number`
- `string`
- `bool`

## 脚本代码 
```csharp
// 定义一个变量,变量必须要赋初值
var value1 = null;
var value2 = 10;
var value3 = "HelloWorld";
var value4 = true;
var value5 = false;

// 赋值
value1 = 1234;
value1 = "string";

// 表达式 
var num1 = 100 * (5 + 3);
var num2 = num1 / 100;

// 条件语句
if (num2 > 1000) {
    print("1000");
} else if (num2 > 100) {
    print("100");
} else {
    print("no");
}

// 循环语句
var x = 3;
while(x > 0) {
    print(x);
    x = x - 1;
}

// 定义一个无参的函数
Method0() {
    // print为内置方法
    print("Hello World");
}

// 带一个参数的函数
Method1(message) {
    print(message);
}

// 函数的调用 
Method0(); // print "Hello World"
Method1("Hello Method1");
Method1(100 * 5);

// 带返回值的函数
// 如果不写return语句,则默认返回null
Method2() {
    return 0;
}

// 实现一个返回最大数的函数
Max(a, b) {
    if (a >= b) {
        return a;
    }
    return b;
}

// 函数空间 
SpaceMethod1() {
    SpaceMethod2() {
        SpaceMethod3() {
            // ...
            // 可调用所有上层空间内的变量和方法
        }
    }
}
```

## C#中 

### 注册C#方法 

```csharp
/// <summary>
/// 注册C#方法的方法 RegisterFunction
/// </summary>
/// <param name="name">方法名</param>
/// <param name="func">回调</param>
/// <result>注册成功则返回true</result>
public static bool RegisterFunction(string name, Func<List<VMValue>, VMValue> func);
```

```csharp
// 在C#中
// 例如注册一个求和方法 sum
gs.VM.RegisterFunction("sum", (List<gs.VMValue> tempArgs) => {
    double ret = 0;
    foreach (var num in tempArgs) {
        if (num.IsNumber()) {
            ret += num.GetNumber();
        }
    }
    return new gs.VMValue(ret);
});
```

```csharp
// 在 GameScript 脚本中就可以使用了
var value = sum(10, 20, 30);
print(value);
```

### 调用脚本 
新建一个文本文件,例如 main.gs
```csharp
// C#代码中
// 读取脚本的全部内容
var src = File.ReadAllText("main.gs");
// Load方法加载一个脚本,并返回此函数对象(一个脚本本身是一个函数)
VMFunction gsFunc = gs.VM.Load(src);
// 执行
gsFunc.Execute();
```

#### VMValue
`VMValue`是一个兼容的值类型,提供了一系列方法便于对脚本值和C#值之间的操作,通过`new VMValue(object)`可以创建对象.

#### VMFunction
`VMFunction`表示一个脚本函数,提供在C#中对脚本中的函数调用,提供`GetFunction(name)`方法可以获取当前函数空间下的子函数

## 最后

在C#工程或者Unity中都可以直接使用,感兴趣的可以下载玩玩,可能有人觉得是在做无用功,但是我觉得很有意义,而且我会在我自己的项目中使用,最主要的写这些东西让我找到了编程的乐趣.

github: <a href="https://github.com/Jenocn/GameScript" target="_blank">https://github.com/Jenocn/GameScript</a>
