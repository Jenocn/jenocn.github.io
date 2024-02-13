/*
	By Jenocn
	https://jenocn.github.io/
*/

using System.Collections.Generic;
using UnityEngine;

public static class InputSystem {
	// 摇杆
	private static Dictionary<System.Tuple<string, string>, int> _axisBinds = new Dictionary<System.Tuple<string, string>, int>();
	private static Dictionary<int, AxisValue> _axisDict = new Dictionary<int, AxisValue>();
	// 按键
	private static List<KeyValuePair<string, int>> _binds = new List<KeyValuePair<string, int>>();
	private static List<KeyValuePair<KeyCode, int>> _keyboardBinds = new List<KeyValuePair<KeyCode, int>>();
	private static Dictionary<int, ButtonValue> _commands = new Dictionary<int, ButtonValue>();

	public static void Update() {

		foreach (var item in _axisBinds) {
			GetAxisValue(item.Value).UpdateState(
				Input.GetAxis(item.Key.Item1),
				Input.GetAxis(item.Key.Item2));
		}
		foreach (var item in _axisDict) {
			item.Value.UpdateStateEnd();
		}

		foreach (var item in _commands) {
			item.Value.Reset();
		}
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

	/// <summary>
	/// 添加摇杆绑定
	/// </summary>
	/// <param name="horizontal">水平事件名称</param>
	/// <param name="vertical">垂直事件名称</param>
	public static AxisValue AddAxisBind(string horizontal, string vertical, int axis) {
		foreach (var item in _axisBinds.Keys) {
			if (item.Item1 == horizontal && item.Item2 == vertical) {
				_axisBinds[item] = axis;
				return GetAxisValue(axis);;
			}
		}
		_axisBinds[new System.Tuple<string, string>(horizontal, vertical)] = axis;
		return GetAxisValue(axis);
	}

	/// <summary>
	/// 删除一个摇杆绑定
	/// </summary>
	public static void RemoveAxisBind(string horizontal, string vertical) {
		System.Tuple<string, string> findKey = null;
		foreach (var item in _axisBinds.Keys) {
			if (item.Item1 == horizontal && item.Item2 == vertical) {
				findKey = item;
				break;
			}
		}
		int axis = _axisBinds[findKey];
		_axisBinds.Remove(findKey);

		bool bReset = true;
		foreach (var item in _axisBinds.Values) {
			if (item == axis) {
				bReset = false;
				break;
			}
		}
		if (bReset) {
			GetAxisValue(axis).Reset();
		}
	}

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

	/// <summary>
	/// 删除绑定
	/// </summary>
	public static void RemoveBind(string name, int command) {
		for (int i = _binds.Count - 1; i >= 0; --i) {
			var item = _binds[i];
			if (item.Key == name && item.Value == command) {
				_binds.RemoveAt(i);
			}
		}
	}

	/// <summary>
	/// 删除绑定
	/// </summary>
	public static void RemoveBind(KeyCode code, int command) {
		for (int i = _keyboardBinds.Count - 1; i >= 0; --i) {
			var item = _keyboardBinds[i];
			if (item.Key == code && item.Value == command) {
				_keyboardBinds.RemoveAt(i);
			}
		}
	}

	/// <summary>
	/// 清空所有绑定
	/// </summary>
	public static void ClearBind() {
		ResetState();
		_binds.Clear();
		_keyboardBinds.Clear();
		_axisBinds.Clear();
		_commands.Clear();
		_axisDict.Clear();
	}

	/// <summary>
	/// 获取命令的按键值
	/// </summary>
	/// <param name="command">命令</param>
	public static ButtonValue GetButtonValue(int command) {
		ButtonValue ret = null;
		if (!_commands.TryGetValue(command, out ret)) {
			ret = new ButtonValue();
			_commands.Add(command, ret);
		}
		return ret;
	}

	/// <summary>
	/// 获取摇杆的值
	/// </summary>
	/// <param name="axis">摇杆id</param>
	public static AxisValue GetAxisValue(int axis) {
		AxisValue ret = null;
		if (!_axisDict.TryGetValue(axis, out ret)) {
			ret = new AxisValue();
			_axisDict.Add(axis, ret);
		}
		return ret;
	}

	/// <summary>
	/// 重置所有命令状态
	/// </summary>
	public static void ResetState() {
		foreach (var item in _axisDict) {
			item.Value.Reset();
		}
		foreach (var item in _commands) {
			item.Value.Reset();
		}
	}

	public static List<KeyValuePair<string, int>> GetBindList() {
		return _binds;
	}
	public static List<KeyValuePair<KeyCode, int>> GetKeyCodeBindList() {
		return _keyboardBinds;
	}
	public static Dictionary<System.Tuple<string, string>, int> GetAxisBindList() {
		return _axisBinds;
	}

	// 按键值
	public class ButtonValue {
		public bool down { get; private set; } = false;
		public bool up { get; private set; } = false;
		public bool hold { get; private set; } = false;
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

	// 摇杆值
	public class AxisValue {
		public Vector2 cur { get; private set; } = Vector2.zero;
		public Vector2 prev { get; private set; } = Vector2.zero;
		public Vector2 delta { get; private set; } = Vector2.zero;
		public bool active { get; private set; } = false;

		public void UpdateState(float x, float y) {
			prev = new Vector2(x, y);
		}
		public void UpdateStateEnd() {
			var temp = prev;
			prev = cur;
			cur = temp;
			delta = cur - prev;

			active = false;
			active |= Mathf.Abs(cur.x) > 0.01f;
			active |= Mathf.Abs(cur.y) > 0.01f;
		}
		public void Reset() {
			cur.Set(0, 0);
			prev.Set(0, 0);
			delta.Set(0, 0);
		}
	}
}