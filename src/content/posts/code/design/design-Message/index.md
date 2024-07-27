---
title: 消息机制
description: 正确使用消息,能够降低代码耦合,提高代码可维护性
published: 2019-04-22 22:16:36
category: '编程'
tags: [设计模式, C++]
---

本文将从以下几点去讲解  
- 介绍消息机制
- 缺点和优点
- 推荐一些使用场合
- 实现一个轻量版本的消息机制(C++)
- 附源码

<!-- more -->

## 介绍消息机制
开门见山,消息机制,就是处理代码之间的通信问题.当一个模块需要调用另一个模块时,就可以使用消息机制,发送一个消息,告诉另一个模块,该工作了.消息机制的使用非常广泛,很多地方都有使用  

## 缺点和优点
### 缺点
* 不太容易掌握使用的时机  
* 代码中会定义很多消息,都需要管理  
* 可能会出现消息循环调用导致的问题  

### 优点  
* 降低耦合  
* 从而提高代码安全性  
* 提高代码的可维护性  
* 有利于后续代码的迭代.  

## 推荐一些使用场合  
* 最经典的使用场合就是数据层向UI层发送消息,即当数据发生变化时,通知UI更新显示  
* 游戏开发中,对象与对象之间的调用,你是否有时候因为获取游戏对象而感到繁琐和麻烦,使用消息机制,发个消息,它自己做  
* 游戏中类似成就的功能需要根据多个模块的功能来判断是否达成,这就造成了成就功能与多个模块产生依赖,使用消息机制是再好不过了

可以使用消息机制的地方还有很多,那么在什么情况下使用消息机制合适呢?  
`当你需要调用另一个模块的功能时,另一个模块不一定会存在的情况下或者你无法优雅的获取对方的实例时`,就是使用消息机制最佳的场合.总之消息机制主要解决的就是耦合的问题,两个不想关的模块之间需要互相调用也都可以使用  

## 实现一个轻量版本的消息机制(C++)  

这里先实现一个轻量的版本,主要是通俗易懂,且可以直接使用,后面再写一篇进阶版本 ***(进阶版本带强类型消息,自动定义消息ID,按需派发消息,消息队列,内存管理等)***  

轻量版本,需要一个消息类,一个消息管理类  
### 消息基类  
```C++
class MessageBase
{
public:
    virtual ~MessageBase() {}
};
```

### 管理消息的中枢系统,负责消息的派发以及管理监听消息的监听者,一般情况下,这个类作为单例比较合适(单例本身就是全局对象,所以直接使用static方法)  
```C++
// MessageCenter.h
// 消息中枢用于派发消息和管理监听者
class MessageCenter
{
public:
    /**
    * 添加监听者
    * 参数1:监听者的名字,用来定位这个监听者
    * 参数2:收到消息时触发的回调函数
    */
    static void AddListener(const std::string& name, std::function<void(int messageID, MessageBase* message)> func);
    /**
    * 删除监听者
    * 参数:通过name删除已存在的监听者
    */
    static void RemoveListener(const std::string& name);
    /**
    * 发送消息
    * 参数1:给这个消息指定一个ID
    * 参数2:具体的消息
    */
    static void Send(int messageID, MessageBase* message);
private:
    /**
    存放所有的监听者
    */
    static std::map<std::string, std::function<void(int messageID, MessageBase*)>> _listenerMap;
}
```

```C++
// MessageCenter.cpp
void MessageCenter::AddListener(const std::string& name, std::function<void(int, MessageBase*)> func)
{
    // 确保参数的可靠性
    if (func == nullptr) { return; }
    auto ite = _listenerMap.find(name);
    if (ite != _listenerMap.end()) { return; }
    _listenerMap.emplace(name, func);
}
void MessageCenter::RemoveListener(const std::string& name)
{
    auto ite = _listenerMap.find(name);
    if (ite == _listenerMap.end()) { return; }
    _listenerMap.erase(ite);
}
void MessageCenter::Send(int messageID, MessageBase* message)
{
    // 回调所有的监听者函数
    for (auto& pair : _listenerMap)
    {
        pair.second(messageID, message);
    }
}
```

### 具体调用的实例
```C++
// MessageDefine.h
// 消息ID
enum class MessageID {
    None = 0,
    MyMessage,
};
// 具体的消息类
class MyMessage : public MessageBase
{
public:
    // 一些参数
    std::string param1;
    int param2;
};
```

```C++
// 这里简单做一个测试,就将监听和派发放在main中处理了,实际上,监听和派发可以在任意处
// main.cpp
void main()
{
    // 添加一个监听者
    MessageCenter.AddListener("Listener", (int id, MessageBase* message) {
        if (id == (int)MessageID.MyMessage)
        {
            auto msg = static_cast<MyMessage*>(message);
            std::cout << msg->param1 << ":" << msg->param2 << std::endl;
        }
    });

    // 发送消息
    MyMessage msg;
    msg.param1 = "Hello";
    msg.param2 = 1001;
    MessageCenter.send(MessageID.MyMessage, &msg);

    // 删除监听者
    MessageCenter.RemoveListener("Listener");
}
```

## 附源码  
源码中模拟了一个经典使用场景(Data-View)  
源码下载地址  
<a href="https://pan.baidu.com/s/1NeOo5VnNjwzw2atZ8pUBtA" target="_blank">https://pan.baidu.com/s/1NeOo5VnNjwzw2atZ8pUBtA</a>  
提取码：bfi8  

这篇文章到这里就结束了,希望能够帮到大家,如有问题可以邮我,一起交流学习!  
