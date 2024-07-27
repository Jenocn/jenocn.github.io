---
title: 如何在VSCode中配置C++开发环境
description: 由于CMake语法晦涩难记,VSCode可以通过配置任务的方式来实现自动化
published: 2020-04-21 23:28:22
category: '编程'
tags: [VSCode, C++]
---

VSCode本身是一个文本编辑器,VSCode中开发C++本质上是通过调用编译器的命令来编译和生成可执行文件,由于CMake语法晦涩难记,VSCode可以通过配置任务的方式来实现自动化,简化操作,一键调用配置好的命令,从而提高效率.

<!-- more -->

## g++/clang++  
在配置之前需要知道,g++是c++的编译器,我的Mac上因为安装了Xcode所以已经被安装了,如果系统上没有需要手动安装
另:clang++也是一种c++编译器

在安装完c++编译器之后,就可以通过命令行调用它来编译C++项目了,但是这样显然不太方便,这就需要vscode了

## vscode配置  
**C/C++插件**  
在vscode的插件一栏搜索`C++`,找到作者为Microsoft的那一个插件并安装,一般是第一个`C/C++`
这个插件的作用是什么呢,目前我知道的是,代码提示,代码跳转,调试

**新建项目**  
新建一个文件夹,这里新建几个cpp文件,随便写点什么,例如:
**`Student.h`**
```C++
#include <string>
class Student
{
public:
	void Show();
	void Set(int age, const std::string& name);
private:
	int _age{ 20 };
	std::string _name;
};
```

**`Student.cpp`**
```C++
#include "student.h"
#include <iostream>

void Student::Show()
{
	std::cout << _name << ":" << _age << std::endl;
}

void Student::Set(int age, const std::string &name)
{
	_age = age;
	_name = name;
}

```

**`main.cpp`**
```C++
#include "student.h"

int main(int argc, char **argb)
{
	auto stu = new Student();
	stu->Set(27, "Jenocn");
	stu->Show();

	return 0;
}
```

**编译配置**  
项目创建之后,就需要编译了,在vscode下按`F1`或者`Ctrl/Cmd+Shift+P`,输入`Tasks: Configure Task`确定,然后如果有选项选择`c/c++:g++`那一项,这时会在`.vscode`目录下新建一个`tasks.json`文件,文件默认内容如下:
```json
{
	"type": "shell", // "shell"表示这个任务是一个shell命令
	"label": "C/C++: g++ build active file", // 这个任务的名称
	"command": "/usr/bin/g++", // g++编译器的可执行文件位置
	"args": [ // g++命令后跟的参数
		"-g", // -g我的理解是省略一系列例如编译,汇编,链接等命令,直接生成机器码
		"${file}", // 待编译的目标文件
		"-o", // 生成可执行文件到下面指定的文件中
		"${fileDirname}/${fileBasenameNoExtension}" // 可执行文件名
	],
	"options": {
		"cwd": "/usr/bin" // 默认的根目录
	},
	"problemMatcher": [
		"$gcc"
	],
	"group": "build"
}
```
我们要修改其中的某些参数(仅列出修改的对象):
```json
{
	"args": [
		"-g",
		"-std=c++17", // 增加这行,表示支持c++17
		// "${file}", // 删除这行,这里的${file}表示当前打开的文件,我们不需要
		"**/*.cpp", // 增加这行,表示当前目录下的所有cpp文件都参与编译(可手动配置)
		"-o",
		"${workspaceRootFolderName}" // 设置为项目文件夹名称
	],
	"options": {
		"cwd": "${workspaceFolder}" // 修改为当前项目根目录
	},
}
```
只有当`options.cwd`设置为`${workspaceFolder}`时,`args`中的文件名则不用设置为绝对路径,这样比较方便

**编译生成可执行文件**  
上面的配置设置完毕之后,只要按`Ctrl/Cmd+Shift+B`执行任务之后,就会在当前目录下生成一个文件夹同名的可执行文件,直接运行就可以了

**调试**  
在vscode调试选项下点击创建`launch.json`,会弹出一个菜单项,选择`C++ (GDB/LLDB)`,会在`.vscode`目录下创建一个`launch.json`文件,内容如下:
```json
"configurations": [
	{
		"name": "(lldb) 启动",
		"type": "cppdbg",
		"request": "launch",
		// "program": "输入程序名称，例如 ${workspaceFolder}/a.out", // 删除这行
		"program": "${workspaceFolder}/${workspaceRootFolderName}", // 修改成这行,这个对象的含义是指定可执行文件
		"args": [],
		"stopAtEntry": false,
		"cwd": "${workspaceFolder}",
		"environment": [],
		"externalConsole": false,
		"MIMode": "lldb"
	}
]
```

配置完成后,就可以按`F5`运行和调试了

那么本篇就到这里了

