---
title: 多对多绑定的手柄/键盘输入系统
description: 如何开发能够支持用户自定义按钮绑定的输入系统?
published: 2020-04-10 13:32:04
category: '编程'
tags: [Unity]
---

为了解决一些需求,比如上下左右在地图上是控制角色移动,在菜单界面是控制选项,又比如同样是攻击操作,在键盘上是`J`键,在手柄上是`X`键,再比如可以让玩家自定义按键关系,所以就有了这样一个多对多的输入系统.

- 设计思路  
- 调用方式  
- 实现  
- 完整源码  

<!-- more -->

---

## 设计思路  
1. 一个数组表示按键与命令的绑定关系  
2. 一个键值对容器表示命令与命令对象的关系  
3. 使用时通过命令获取状态  

## InputSystem 调用三步走  

**1. 定义命令**  
自定义的命令枚举:  
```csharp
public enum InputCommand {
	Attack, // 攻击
	MoveUp, // 向上移动
	MenuUp, // 菜单上一个
}
```
<br>  

**2. 注册命令和按键绑定关系**  
```csharp
private void Start() {
	// 添加绑定关系 (键盘按键J - Attack命令)
	InputSystem.AddButtonBind(KeyCode.J, (int) InputCommand.Attack);
	// 添加绑定关系 (按键名attack) - Attack命令)
	InputSystem.AddButtonBind("attack", (int) InputCommand.Attack);

	// 添加绑定关系 (键盘按键W - 向上移动命令)
	InputSystem.AddButtonBind(KeyCode.W, (int) InputCommand.MoveUp);
	// 添加绑定关系 (键盘按键W - 菜单上一个命令)
	InputSystem.AddButtonBind(KeyCode.W, (int) InputCommand.MenuUp);
	// 添加绑定关系 (键盘按键↑箭头 - 菜单上一个命令)
	InputSystem.AddButtonBind(KeyCode.UpArrow, (int) InputCommand.MenuUp);
}
```
<br>

**3. Uppublished判断命令状态,执行逻辑**  
```csharp
private void Uppublished() {
	// 调用Uppublished方法,因为是摘取片段因此放到这里,实际上全局只用调用一次即可
	InputSystem.Uppublished();
	
	// 获取命令的键值
	var attackValue = InputSystem.GetButtonValue((int) InputCommand.Attack);
	var moveUpValue = InputSystem.GetButtonValue((int) InputCommand.MoveUp);
	var menuUpValue = InputSystem.GetButtonValue((int) InputCommand.MenuUp);

	// 是否按下
	if (attackValue.down) {
		Debug.Log("attack down");
	}
	// 是否松开
	if (attackValue.up) {
		Debug.Log("attack up");
	}
	// 是否保持
	if (attackValue.hold) {
		Debug.Log("attack hold");
	}

	// 其他同理...
}
```
<br>

## 实现  
1. Unity提供了Input类来监听按键的消息,并且提供了两种方式来指定按键键值,一种是通过`KeyCode`,另一种是通过在编辑器中提前配置好的`button-name`  
2. 命令采用int类型,主要是和enum兼容比较好,也方便了自定义命令  

**绑定关系的数组:**
按键对应命令
```csharp
// keycode
List<KeyValuePair<KeyCode, int>> _keyboardBinds;
// button-name
List<KeyValuePair<string, int>> _binds;
```
<br>  

##### 添加绑定关系的方法   
```csharp
/// <summary>
/// 添加按键绑定,多对多
/// <para>支持一个按键绑定多个命令</para>
/// <para>支持多个按键绑定一个命令</para>
/// </summary>
/// <param name="eventName">事件名称</param>
/// <param name="command">命令</param>
public static ButtonValue AddButtonBind(string eventName, int command) {
	_binds.Add(new KeyValuePair<string, int>(eventName, command));
	return GetButtonValue(command);
}

/// <summary>
/// 添加按键绑定,多对多
/// <para>支持一个按键绑定多个命令</para>
/// <para>支持多个按键绑定一个命令</para>
/// </summary>
/// <param name="code">按键code</param>
/// <param name="command">命令</param>
public static ButtonValue AddButtonBind(KeyCode code, int command) {
	_keyboardBinds.Add(new KeyValuePair<KeyCode, int>(code, command));
	return GetButtonValue(command);
}
```
<br>  

##### 命令对象 ButtonValue  

`ButtonValue`类,就是用来保存和管理按键状态的,有down(按下),up(弹起),hold(保持)三种状态  
提供两个方法,`OrState`用于设置按键状态,`Reset`用于初始化状态  

```csharp
public class ButtonValue {
	public bool down { get; private set; } = false;
	public bool up { get; private set; } = false;
	public bool hold { get; private set; } = false;
	// OrState使用或关系来赋值状态,因为可能多个按键绑定了同一个命令,都会触发
	public void OrState(bool down, bool up, bool hold) {
		this.down |= down;
		this.up |= up;
		this.hold |= hold;
	}
	public void Reset() {
		down = false;
		up = false;
		hold = false;
	}
}
```
<br>

一个命令则对应一个ButtonValue
```csharp
Dictionary<int, ButtonValue> _commands;
```
<br>

##### 实现`GetButtonValue`方法  
凡是存在一个`GetButtonValue(command)`的调用,就认定这个命令是存在的,这样的机制会更加的自由,并且会使得调用层不需要去考虑这个对象是否存在  
```csharp
/// <summary>
/// 获取命令的按键值
/// </summary>
/// <param name="command">命令</param>
public static ButtonValue GetButtonValue(int command) {
	ButtonValue ret = null;
	// 从Dictionary中查找,如果没有就创建一个
	if (!_commands.TryGetValue(command, out ret)) {
		ret = new ButtonValue();
		_commands.Add(command, ret);
	}
	return ret;
}
```
<br>

##### 实现Uppublished方法,更新所有命令的状态  
```csharp
public static void Uppublished() {
	// 首先重置所有命令的状态
	foreach (var item in _commands) {
		item.Value.Reset();
	}
	// 遍历所有的绑定关系,并更新对应命令的按键状态
	foreach (var item in _binds) {
		GetButtonValue(item.Value).OrState(
			Input.GetButtonDown(item.Key),
			Input.GetButtonUp(item.Key),
			Input.GetButton(item.Key));
	}
	foreach (var item in _keyboardBinds) {
		GetButtonValue(item.Value).OrState(
			Input.GetKeyDown(item.Key),
			Input.GetKeyUp(item.Key),
			Input.GetKey(item.Key));
	}
}
```
这个`Uppublished`在使用时,只需要在一个总的管理系统内调用一次  

## 最后  
整个输入系统的代码其实就这么简单,下面将提供完整的源码,包含了对摇杆的支持,希望能够帮助到需要的人.

源码链接:
[http://github.com/Jenocn/InputSystem](http://github.com/Jenocn/InputSystem)
