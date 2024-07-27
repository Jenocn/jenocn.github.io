---
title: 使用GameScript与命令模式在Unity中构建剧情模块
description: 使用脚本来构建剧情模块,不需要过多的涉及代码改动,支持热更新
published: 2019-10-09 09:43:48
category: '编程'
tags: [Unity]
---

剧情的文本和展示条件往往需要频繁的修改,不管是交给剧情策划,还是自己独立开发写剧情,都会希望剧情能够方便的修改,不需要过多的涉及代码改动,所以结合脚本来构建剧情模块,是最快捷的方案.

本文:
- 剧情管理器
- 命令模式实现UI展示的调用
- GameScript接入
- 脚本函数与命令绑定
- 剧情触发器

<!-- more -->

## 剧情管理器  
剧情管理器的作用是对剧情的调用方式做一个抽象,一方面提供更方便的接口给调用端使用,比如触发剧情第一章第一节,触发NPC某某某的对话等等,另一方面是对接脚本函数

**接口设计**  
先设计接口,具体实现暂时不管,保持模块的一个大概思路,后面的实现就是一步步完成就好了
```csharp
public class StoryManager : Singleton<StoryManager> {
	// 执行主线故事(参数为:章,节))
	public void ExecuteMainStory(int chapter, int part);
	// 执行支线故事
	public void ExecuteSubSotry(int chapter, int part);
	// 执行NPC对话
	public void ExecuteNpcTalk(int npcID);
	// 根据游戏的需要,还可以扩展更多的方法
	// more ...
}
```

调用:
```csharp
// 执行第一章第一节的剧情
StoryManager.GetInstance().ExecuteMainStory(1, 1);
```

实现这几个方法,就需要结合脚本.

## 命令模式实现UI展示的调用  
具体的展示如何做,每个游戏都不一样,这一块不是本文的重点,这里做了一个简单的显示方式,如下图:
![display](talk_box.png)

**命令模式结合消息控制UI的展示**
一个命令基类:
```csharp
public abstract class GameCommandBase {
	// 用于标记命令是否结束
	protected bool _bFinish = true;
	// 执行命令的方法,子类去实现
	public abstract void Execute();

	public virtual void OnEnter() {}
	public virtual void OnExit() {}

	public bool IsFinish() {
		return _bFinish;
	}
}
```

各类具体的命令:
这里使用到了消息模式,关于消息模式可以参考我的另一篇文章[<<进阶版消息机制实现>>](https://jenocn.github.io/2019/04/MessageCenter/)
```csharp
public class StoryCommand : GameCommandBase {

	// 对话的文本
	public string content = "";

	public override void OnEnter() {
		_bFinish = false;

		// 这里监听剧情对话的结束消息
		MessageCenter.AddListener(this, (OnStoryTalkClose msg) => {
			_bFinish = true;
		});
	}

	public override void OnExit() {
		MessageCenter.RemoveListener<OnStoryTalkClose>(this);
	}

	public override void Execute() {
		// 发送一个剧情对话的播放消息,会在负责显示对话UI的类中监听这个消息
		var msg = new OnCommandStoryTalk();
		msg.content = content;
		MessageCenter.Send(msg);
	}
}
```

对于其他播放音效等命令就不再贴代码了,实现思路与这个一样.

**命令管理器**  
命令管理器是负责批处理命令的中枢系统

```csharp
public static class CommandSystem {
	// 一个用于存放一组命令的队列,后面会从队列中按顺序取出并执行
	public static Queue<GameCommandBase> _cmdQueue = new Queue<GameCommandBase>();
	// 当前的命令
	public static GameCommandBase _currentCmd = null;

	// 入队一个命令
	public static void PushCMD(GameCommandBase cmd) {
		_cmdQueue.Enqueue(cmd);
	}

	// 执行
	public static void Execute() {
		// 当前命令存在的话不能重复执行
		if (_currentCmd != null) { return; }
		// 队列中不存在命令也无法执行
		if (_cmdQueue.Count == 0) { return; }
		// 取出命令,执行命令
		_currentCmd = _cmdQueue.Dequeue();
		_currentCmd.OnEnter();
		_currentCmd.Execute();
	}

	// Uppublished函数,用于检测命令的结束,并自动进入下一个命令
	// Uppublished方法需要放在一个场景的Uppublished中去执行
	public static void Uppublished() {
		// 当前命令不存在则不继续
		if (_currentCmd == null) { return; }
		// 当前命令未完成则不继续
		if (!_currentCmd.IsFinish()) { return; }
		// 执行当前命令退出
		_currentCmd.OnExit();
		_currentCmd = null;
		// 继续执行下一个命令
		Execute();
	}
}
```

以上就打通了UI展示部分的调用,只要在任意地方调用如下:
```csharp
// 创建一个命令
var cmd = new StoryCommand();
// 显示的文本
cmd.content = "这里很危险,快回去!";
// 入队
CommandSystem.PushCMD(cmd);
// 如果有多个命令
// more push ...
// 执行
CommandSystem.Execute();
```

## GameScript接入 

> Github:
[https://github.com/Jenocn/GameScript](https://github.com/Jenocn/GameScript)  

GameScript是我自己写的一个基于字符串解析的脚本语言,优点就是接入方便,直接将DLL拖入Unity,无需配置,就可以直接使用.

*(这里使用其他脚本语言,思路都是一样的)*  

**设计剧情脚本的管理方式**   
我的设计思路是一个片段为一个脚本文件,这样也是方便后期的查找和修改
例如主线第一章第一节的剧情文件为:`main_1_1.gs.txt`
支线则为:`sub_1_1.gs.txt`这种形式
id为10001的npc的对话为:`npc_10001.gs.txt`

**设计脚本函数**  
播放对话:
`StoryTalk(content)`
播放音频:
`Audio(kindID, audioName, volume)`
播放BGM:
`Music(musicName, volume)`
等待second秒:
`Wait(second)`
执行操作:
`Execute()`

**具体内容编写**
```csharp
// main_1_1.gs.txt
entry() {
	// 播放一个bgm
	Music("sotry_bgm", 0.8);
	// 播放一个对话音效
	Audio(1, "voice_1", 1);
	// 播放对话
	StoryTalk("这里很危险,快回去!");
	// 执行
	Execute();
}
```

## 脚本函数与命令绑定  

在StoryManager中增加一个初始化方法,并注册脚本函数:

```csharp
// gs为GameScript的命名空间
using gs;

public class StoryManager : Singleton<StoryManager> {
	// 定义一个公共函数(GameScript的思路是函数看成一个空间,函数内可以包含函数)
	private VMFunction _storyDefine = null;

	public void Init() {
		// 加载公共空间的脚本,可以写一些公共的内容
		// 这里的 GSSystem.LoadScript 本质上如下,后面的内容将不再解释
		// GSSystem.LoadScript 会做一些安全判断
		/*
		var textAsset = Resources.Load<TextAsset>("story/define.gs");
		_storyDefine = VM.Load(textAsset.text);
		*/
		_storyDefine = GSSystem.LoadScript("story/define.gs");
		// 执行一遍,创建公共对象
		_storyDefine.Execute();
		
		// 注册 StoryTalk 函数
		_storyDefine.RegisterFunction("StoryTalk", (List<VMValue> args) => {
			// 参数验证
			if (args.Count < 1) { return; }
			// 创建命令
			var cmd = new GameCommand.StoryCommand();
			cmd.content = args[0].ToString();
			CommandSystem.PushCMD(cmd);
		});
		// 注册 Execute 函数
		_storyDefine.RegisterFunction("Execute", (List<VMValue> args) => {
			CommandSystem.Execute();
		});

		// 注册其他函数,就不再详细写,思路一样
		// more ...
	}

	// other ...
}
```

**实现调用剧情的方法**  

```csharp
public void ExecuteMainStory(int chapter, int part) {
	_ExecuteStory(0, chapter, part);
}
public void ExecuteSubStory(int chapter, int part) {
	_ExecuteStory(1, chapter, part);
}

/**
	type: 0表示主线,1表示支线
*/
private void _ExecuteStory(int type, int chapter, int part) {
	string storySign = "";
	if (type == 0) {
		storySign = "main";
	} else if (type == 1) {
		storySign = "sub";
	} else {
		return;
	}

	// 加载脚本文件,并且将_storyDefine作为父空间,这样就可以调用父空间注册的函数了
	var func = GSSystem.LoadScript(string.Format("story/{0}_{1}_{2}.gs", storySign, chapter, part), _storyDefine);
	if (func == null) { return; }
	// 执行文件
	func.Execute();
	// 获取其中的entry方法
	var entryFunc = func.GetFunction("entry");
	if (entryFunc != null) {
		// 执行entry()
		entryFunc.Execute();
	}
}
```

## 剧情触发器  

触发器就是在游戏中发生某些条件的时候,我们接收到这个条件做什么事情,这里将触发条件回调到脚本中,就可以在脚本中处理具体的条件,并播放剧情了

这个仍然可以在剧情管理器中实现:

```csharp
using gs;
public class StoryManager : Singleton<StoryManager> {
	private VMFunction _storyTrigger = null;

	public void Init() {
		// 需要创建story/trigger.gs这个文件
		_storyTrigger = GSSystem.LoadScript("story/trigger.gs", _storyDefine);
		_storyTrigger.Execute();

		// 注册一个主线剧情回调的方法
		_storyDefine.RegisterFunction("MainStory", (List<VMValue> args) => {
			if (args.Count < 2) { return; }
			if (args[0].IsNumber() && args[1].IsNumber()) {
				ExecuteMainStory(args[0].GetInt(), args[1].GetInt());
			}
		});

		// other ...
	}
	// other ...
}
```

然后例如收到某个事件,比如角色升级:
在StoryManager中添加触发器
```csharp
public void OnLevelUp(int cur, int to) {
	// 回调触发器脚本的OnLevelUp方法
	var func = _storyTrigger.GetFunction("OnLevelUp");
	if (func) {
		List<VMValue> args = new List<VMValue>();
		args.Add(new VMValue(cur));
		args.Add(new VMValue(to));
		func.Execute(args);
	}
}
```

触发器脚本:
`trigger.gs.txt`
```csharp
OnLevelUp(cur, to) {
	// 判断到达5级,触发主线第一章第一节
	if (to == 5) {
		// 调用主线剧情
		MainStory(1, 1);
	}
}
```

到这里,整个就完成了,剩下的就是慢慢完善和填充内容,本文提供的是一个思路,希望能够帮助到需要的人.
