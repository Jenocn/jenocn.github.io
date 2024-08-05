---
title: 进阶版消息机制实现
description: 如何实现一个自动定义消息ID,按需派发消息,强类型,支持消息队列,内存安全的消息中枢系统
published: 2019-04-24 11:27:45
category: '编程'
tags: [设计模式, C++]
---

**实现一个自动定义消息ID,按需派发消息,强类型,支持消息队列，内存安全的消息中枢系统**  

上篇文章<a href="https://jenocn.github.io/code/pattern/Message" target="_blank">《代码中的消息机制》</a>已经简单的讲解了消息机制,并附带了源码和实际使用场景,这篇文章将直接讲解如何实现进阶版消息机制  

本文将从以下几点讲解：  
- 实现自定义消息ID
- 实现按需派发消息
- 实现强类型
- 实现消息队列
- 实现内存安全
- 完善消息队列
- 附源码

<!-- more -->

## 实现自定义消息ID
- **使消息的管理更方便,开发时只需要关注具体的消息即可**  

在上一篇的简单消息机制里,每定义一个新的消息,都要定义一个与之匹配的ID用来辨认是哪个消息,我见过的很多项目中也是如此,这样就会使我们多维护一个消息ID,即我们需要维护一个消息ID列表和一个具体消息数据的集合,这使得收发消息都不那么方便,代码也不那么优雅.那么现在就来实现一个自定义消息ID吧,将ID与类绑定,并且自增长,不需要人工去定义它.  

这里结合模板来实现这个功能  

```C++
// 计数器, T表示当前计数器的归属
template<typename T>
class ClassTypeCounter final
{
public:
    // 一个static的int变量用于保存数量
    static int _count;
    // 一个成员变量,用于标记自己的位置
    int _index{ 0 };
public:
    // 每当实例化一个该对象,_index保留当前计数,然后计数器加1
    ClassTypeCounter()
    {
        _index = _count;
        ++_count;
    }
};
```
```C++
/**
* 一个模板的ClassType基类(叫ClassType是因为ID这个说法在子类中可能会被用到,所以改成ClassType区分)
* TClass:需要计数的类
* TCounter:计数器的归属
*/
template<typename TClass, typename TCounter>
class ClassType
{
private:
    /**
    * 一个具体的计数器
    * 这样每产生一个新的TClass,都会产生一个ClassTypeCounter<TCounter>的具体对象,从而实现id增长
    */
    static ClassTypeCounter<TCounter> __counter;
public:
    // 获取自己的ClassType
    static int GetClassType() { return __counter._index; }
};
```

有了上面两个模板,自定义消息就OK啦,下面看怎么用

```C++
/**
* 消息基类,上篇中讲过的
* 这里改成模板了,需要一个类来作为消息的计数器归属,这里用一个IMessage
* 嗯? 为什么加了个IMessage,后面说,这里先加上
*/
template<typename T>
class MessageBase : public ClassType<T, IMessage>
{
public:
    // 这里需要实现一下IMessage里的接口
    virtual int GetMessageID() const
    {
        // 因为有了ClassType计数器了,哈哈,是不是很方便
        return MessageBase<T>::GetClassType();
    }
};
```

```C++
// IMessage需要定义一下
class IMessage {
public:
    virtual ~IMessage() {}
    // 这里需要这个接口,来给实例调用
    virtual int GetMessageID() const = 0;
};
```

具体的使用就像这样,跟之前使用一样哦,就是继承时多传个类型,把自己传过去,记一下数:
```C++
class MyMessage : public Message<MyMessage>
{
    // some parameters...
};
```
然后那个MessageType的枚举就可以干掉了,改成这样使用
```C++
int main()
{
    MessageCenter::AddListener("listener", [](IMessage* message){
        if (message->GetMessageID() == MyMessage::GetClassType())
        {
            // todo...
        }
    });
}
```

到这里,自定义ID就已经完成啦.  

## 实现按需派发消息  
- **监听者只监听自己关心的消息**  
- **监听消息时不用再判断消息是不是自己想要的,提高效率,代码也好看**  

按需派发消息的好处是很多的,一个监听者只负责监听一个消息,没有自己的消息时就不回调,这样使得大型开发中使用消息也能管理的井井有条,功能开发模块化,需要监听就添加,不需要就删除  

有了上面的自定义消息ID,这个就很简单了,但是呢,这里先说简单的思路,但不具体实现,后面强类型那里再具体实现,为什么这样做呢,是因为结合强类型,可以不用单独去关心ID,只关心具体的消息就行了  

```C++
// 简单做法就是添加监听者的时候给一个ID参数
// 在Send消息的时候判断一下messageID是不是与Message的GetClassType()相同就行了
static void AddListener(const std::string& name, int messageID, std::function<void(IMessage*)> func);
// 使用时就这样,但是这样无疑增加了一个不必要的参数,后面结合强类型来实现
MessageCenter::AddListener("Listener", MyMessage::GetClassType(), [](IMessage* message){ });
```

```C++
// 还有一个方法就是使用模板
template<typename T>
static void AddListener(const std::string& name, std::function<void(IMessage*)> func) {
    // T::GetClassType();就可以获取到这个消息的ID了,这也体现了自定义ID的好处,减少参数是好的
}
// 这样调用的时候就是
MessageCenter::AddListener<MyMessage>("Listener", [](IMessage* message){ });
```

按需派发消息就先到这里了.

## 实现强类型  
- **省去了类型的转换**  
- **更安全,具体逻辑不再需要转换类型**  
- **即拿即用,很方便**  

强类型就是指,消息在接收时,能够保持消息原本的类型,而不是一个基类类型.这样做的好处就是接收到的消息不用转换,编译器会强制你使用正确的类型,提高了代码的健壮性,用起来也很舒服  

```C++
// 需要一个MessageListener<T>类来保存类型
// 这里需要一个IMessageListener,是因为MessageCenter需要统一保存所有Listener
template<typename T>
class MessageListener : public IMessageListener
{
public:
    MessageListener(std::function<void(T*)> func)
        : _func(func)
    {}
public:
    // 提供给MessageCenter调用
    virtual void Invoke(IMessage* message)
    {
        // 因为有类型,所以可以转换啦
        // 然后因为是按需派发,所以这里的message就是T*类型
        _func(static_cast<T*>(message));
    }

    // 返回一下ID,这是实现自IMessageListener的,MessageCenter需要用
    virtual int GetMessageID() const
    {
        return T::GetMessageID();
    }
private:
    std::function<void(T*)> _func;
};
```

```C++
// IMessageListener
class IMessageListener
{
public:
    virtual ~IMessageListener() {}
    virtual void Invoke(IMessage* message) = 0;
    virtual int GetMessageID() const = 0;
};
```
MessageListener在实际使用时是不需要关心的  

```C++
// MessageCenter这边的处理以及按需派发的实现  
// 这里只贴关键代码,全部源码请在文章最后查看
// MessageCenter类
class MessageCenter
{
public:
    // 使用模板,还有function里面的参数也使用的T
    template<typename T>
    static void AddListener(const std::string& name, std::function<void(T*)> func)
    {
        // ... 此处省略参数有效性的判断
        // 关键代码一行
        IMessage* listener = new MessageListener<T>(func);
        // 添加到监听器集合
        _listenerMap.emplace(name, listener);
    }

    // 派发消息,按需派发
    static void Send(IMessage* message)
    {
        if (!message) { return; }
        // 遍历监听者
        for (auto& pair : _listenerMap)
        {
            auto listener = pair.second;
            // 判断消息ID是否一致,如果是则回调
            if (message->GetMessageID() == listener->GetMessageID())
            {
                listener->Invoke(message);
            }
        }
    }
};
```
```C++
// 调用方式
MessageCenter::AddListener<MyMessage>("Listener", [](MyMessage* message){
    // message->param1; 就可以直接拿到属性了
});
```

到这里,按需派发和强类型就搞定啦.

## 实现消息队列  
- **先将消息入队,当调用派发的时候,再统一发送**  
- **游戏开发时,可以在下一帧发送消息,好处坏处都有,看需要**  

消息队列可以先Push消息,然后在合适的时机再Dispatch派发消息,因为不是立即调用,好处和坏处并存,看需要  

```C++
// MessageCenter类
// 定义一个消息队列
std::list<IMessage*> _messageQueue;
// 增加一个Push方法
static void Push(IMessage* message)
{
    if (!message) { return; }
    // 首先这里会有内存隐患,后面内存安全部分讲解如何处理
    // 隐患:你不知道message是一个动态创建的对象还是一个局部变量的地址
    _messageQueue.emplace_back(message);
}
// 增加一个OnDispatch方法
static void OnDispatch()
{
    for (auto message : _messageQueue)
    {
        Send(message);
        // 安全隐患
        delete message;
    }
    _messageQueue.clear();
}
```
很多项目的做法是拷贝一个消息,但是我不建议这样做,而且无法做到完美深拷贝,而且使用的过程可能还会出很多问题,性能也会大打折扣,所以后面的安全部分讲解如何使用更好的方法解决  

## 内存安全  

C++开发的其中一个难点就是需要管理内存,当你开发一个通用工具时,最重要的就是它是安全可靠的,这样才用的放心,消息机制是需要被大量使用的,一个好的消息机制不应该在使用时来管理内存,这里的实现借助于std::shared_ptr

std::shared_ptr内部基于引用计数,重写了拷贝构造,在拷贝时就会增加引用,std::shared_ptr销毁时就会减少引用  

```C++
// MessageBase中
// 使用一个名称表示std::shared_ptr<IMessage>
using MessagePtr = std::shared_ptr<IMessage>;
// MessageBase增加方法,这样可以简化创建std::shared_ptr<T>
static std::shared_ptr<T> Create()
{
        return std::shared_ptr<T>(new T());
}
```
```C++
// MessageCenter中
// 消息队列
static std::list<MessagePtr> _messageQueue;
// 派发消息
static void Send(MessagePtr message);
static void Push(MessagePtr message);
// 添加监听者
template<typename T>
static void AddListener(const std::string& name, std::function<void(std::shared_ptr<T> message)> func);
```
```C++
// MessageListener
// _func需要保留类型,使用std::shared_ptr<T>
std::function<void(std::shared_ptr<T>)> _func;
// 参数使用 MessagePtr
virtual void Invoke(MessagePtr message)
{
    // 这里使用std::static_pointer_cast<T>
    // 将std::shared_ptr<IMessage>转换为std::shared_ptr<T>
    _func(std::static_pointer_cast<T>(message));
}
```

具体的代码可以看文章最后附带的源码  

# 完善消息队列  

处理完安全隐患之后,可以把注意力再回到消息队列,在OnDispatch回调时继续Push消息会出现问题,因为在遍历一个列表时不建议往里面添加新的元素,虽然有一些其他的解决方法,这里我使用双缓冲的方法  

```C++
// 需要两个消息队列
// 一个true标记的队列,一个false标记的队列
static std::map<bool, std::list<MessagePtr>> _messageQueue;
// 用于标记当前活跃的队列
static bool _activeQueueSign;
// 私有方法,获取活跃队列
static std::list<MessagePtr>& _GetActiveQueue();
// 私有方法,获取闲置队列
static std::list<MessagePtr>& _GetIdleQueue();
// OnDispatch
void OnDispatch()
{
    // 切换标记
    _activeQueueSign = !_activeQueueSign;
    // 从活跃队列中派发消息
    for (auto message : _GetActiveQueue())
    {
        Send(message);
    }
    _GetActiveQueue().clear();
}
// 添加到闲置队列中
void Push(MessagePtr message)
{
    if (!message) { return; }
    _GetIdleQueue().emplace_back(message);
}
```

## 使用方法
新建一个MessageDefine.h文件,存放所有消息
```C++
// MessageDefine.h 

// 包含MessageCenter.h头文件即可
#include "MessageCenter.h"

// 新增一个具体消息
class MyMessage : public MessageBase<MyMessage>
{
public:
    // some parameters
    int value1;
    float value2;
    std::string value3;
    // ...
};
```
使用的地方
```C++
// main.cpp

// 包含你自己的消息头文件
#include "MessageDefine.h"

void main()
{
    // 监听
    MessageCenter::AddListener<MyMessage>("main", [](std::shared_ptr<MyMessage> msg){
        // todo...
    });

    // 创建一个消息
    auto message = MyMessage::Create();
    // 赋值
    message->value1 = 1;
    message->value2 = 3.14f;
    message->value3 = "Hello":
    // 发送
    MessageCenter::Send(message);
    /**
    // 入队
    MessageCenter::Push(message);
    // 派发
    MessageCenter::OnDispatch();
    */

    // 删除监听
    MessageCenter::RemoveListener("main");
}
```

## 源码仓库
[https://github.com/Jenocn/MessageCenter](https://github.com/Jenocn/MessageCenter)

这篇文章到这里就结束啦,至此,整个消息机制也告一段落啦,希望看过之后对你能有帮助,如有问题可以邮我,一起交流学习!  
