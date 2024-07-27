---
title: 多层if嵌套的优化
published: 2019-06-02 21:48:18
category: '编程'
tags: [经验]
---

在实际开发中,写了很多功能,也改了无数bug了,这次讲一讲代码书写方面的技巧.遇到就写一点,这次讲一讲如何写利于扩展的代码.

<!-- more -->

## 需求

有这样一个需求,有3个物品的掉落池,分别为普通池,稀有池,传说池,概率分别为100%, 20%, 1.5%,一般上概率为了便于修改,会写在配置中,故有如下配置


```json
{
    "normal_percent":1,
    "rare_percent":0.2,
    "legend_percent":0.015
}
```

然后需求中要求,先随机是否为传说池,不是则随机是否为稀有池,最后是普通池

## 直接编码

```csharp
// 标记一个结果
int ret = -1;
// 获取传说池概率
var legend = Config.GetFloat("legend_percent");
// 随机工具判断是否命中概率
if (RandomTool.HitWithPercent(legend)) {
    ret = 2;
} else {
    // 以下同上,根据逻辑逐次判断
    var rare = Config.GetFloat("rare_percent");
    if (RandomTool.HitWithPercent(rare)) {
        ret = 1;
    } else {
        var normal = Config.GetFloat("normal_percent");
        if (RandomTool.HitWithPercent(normal)) {
            ret = 0;
        }
    }
}
```

显然上面的书写方式不太利于扩展,假设需求变更,要在稀有和传说中间加一个史诗池,那么会怎样?

```csharp
int ret = -1;
var legend = Config.GetFloat("legend_percent");
if (RandomTool.HitWithPercent(legend)) {
    ret = 3;
} else {
    // 要将史诗池的判断加在这里,然后后面的代码全部都要挪到else里面去
    // 而且嵌套层次越来越多
    var epic = Config.GetFloat("epic_percent");
    if (RandomTool.HitWithPercent(rare)) {
        ret = 2;
    } else {
        var rare = Config.GetFloat("rare_percent");
        if (RandomTool.HitWithPercent(rare)) {
            ret = 1;
        } else {
            var normal = Config.GetFloat("normal_percent");
            if (RandomTool.HitWithPercent(normal)) {
                ret = 0;
            }
        }
    }
}
```

## 换一种写法

下面将换一种方式书写,但并不是最优方式,而是一个小技巧,这个小技巧用的还蛮多的,但是这里有更好的方式,最后再讲.

```csharp
// 利用do {} while(false)的方式,减少代码缩进,易于阅读,修改也更方便
int ret = -1;
do {
    // 先判断传说的概率,满足了就break跳出循环,否则会继续后面的逻辑
    var legend = Config.GetFloat("legend_percent");
    if (RandomTool.HitWithPercent(legend)) {
        ret = 2;
        break;
    }
    var rare = Config.GetFloat("rare_percent");
    if (RandomTool.HitWithPercent(rare)) {
        ret = 1;
        break;
    }
    var normal = Config.GetFloat("normal_percent");
    if (RandomTool.HitWithPercent(normal)) {
        ret = 0;
        break;
    }
} while (false);
```

像上面这样的书写,再加入一个史诗概率池进去,也不用改动其他部分的代码,而是在需要的位置,插入一段史诗的代码就可以了,非常简单的一个书写技巧,但是却使得后期维护和阅读都更加轻松.

##### 这个技巧使用的地方有很多,像嵌套很多的代码,都可以通过这个方式减少嵌套,非常推荐掌握

## 本例子更好的方式
那么本例子其实还有一种更好的做法,当增加池的时候,甚至不需要改动随机部分的代码,只需要改动配置就达到需求了,主要是结合容器的使用

首先是配置部分需要换一种方式写:

```json
{
    "drop_percent_0":1,
    "drop_percent_1":0.2,
    "drop_percent_2":0.015
}
```

将代码分为两个部分,初始化时载入所有概率到List中,其实也可以直接在处理随机的地方加载,但是这种数据是不会变的,所以可以放到这个类初始化的位置

```csharp
// 存放概率配置的容器
List<float> _percentList = new List<float>();
// 这里循环使用100表示上限支持最多100个池,也可以使用int max
for (int i = 0; i < 100; ++i) {
    // -1是默认值,当获取不到这个配置的时候,返回-1,则break跳出循环
    var percent = Config.GetFloat("drop_percent_" + i, -1);
    if (percent == -1) { break; }
    _percentList.Add(percent);
}
```

```csharp
// 随机部分代码
// 从列表尾端开始循环,逐个判断,命中概率后获得当前的index即为结果然后break循环
int ret = -1;
for (int i = _percentList.Count - 1; i >= 0; --i) {
    var percent = _percentList[i];
    if (RandomTool.HitWithPercent(percent)) {
        ret = i;
        break;
    }
}
```

使用上面这种方法,无论是加池减池,都不用再改动代码了,这个技巧也可以用在很多其他地方

本文就到这里了,以后当遇到合适的技巧,再继续写哈哈.


