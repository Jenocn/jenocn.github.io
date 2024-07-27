---
title: Vue简单使用
published: 2019-04-30 11:17:47
category: '编程'
tags: [前端]
---

最近在看前端,然后理所当然的接触到了目前最流行的Vue,Vue超棒的!  

<!-- more -->

## 我对Vue的理解  
[Vue.js](https://cn.vuejs.org/) 是国人大神尤雨溪开发的前端框架,使用Vue能够轻松与Html进行交互,这样就能更方便的创建我们想要的界面和效果  
- 能够通过绑定名称直接获取dom元素  
- 能够直接将属性与value进行绑定,且当属性值发生变化时value对应的视图也会立即更新  
- dom属性支持条件和循环语句,这样能做到很多效果,显示或者隐藏,根据数组动态创建列表等等,简直不要太棒  
- button按钮绑定回调函数  
- 组件式开发html页面,使得页面模块化,可复用  

## 使用Vue

##### 在页面中引入Vue

最简单的方法就是直接引用
```Html
<script src="https://cdn.jsdelivr.net/npm/vue/dist/vue.js"></script>
```

或者下载下来,放到js目录下 [Vue.js 右键另存为](https://vuejs.org/js/vue.min.js)
```Html
<script src="./js/vue.min.js"></script>
```

##### Hello World

新建一个`index.html`文件,引入Vue,写一个`<p>`标签,标签内容为`{{ text }}`

```Html { .line-numbers, highlight=[8, 11, 14-17, 20] }
<html>
<head>
    <meta charset="UTF-8">
    <title>Document</title>
</head>
<body>
<div id="first">
    <p>{{ text }}</p>
</div>
</body>
<script src="https://cdn.jsdelivr.net/npm/vue/dist/vue.js"></script>
<script>
    // 创建一个vue实例
    var vm = new Vue({
        el: "#first", // 指定实例从id=first的范围内创建
        data: { // data下是{{}}声明的变量
            text: "Hello World" // 给这个变量赋初值
        }
    });
    // vm.text = "Change Text"; // 这里就可以通过vm.text获取到刚刚定义的变量
</script>
</html>
```
输出结果:  
<p>Hello World</p>

##### input 双向绑定,获取输入的文本  

`v-model`用于将变量与内容绑定,在刚刚HelloWorld程序的基础上,在`<p>`下面新增一行代码,创建一个`<input>`,变量名仍然使用`text`,其他地方都无需改动
```Html {highlight=3}
<div id="first">
    <p id>{{ text }}</p>
    <input v-model="text">
</div>
```

现在在输入框里输入内容就会立即改变`<p>`标签的内容,并且通过`vm.text`可以获取到当前文本  

##### 条件语句  

`v-if`用于根据其值来决定当前元素是否显示,值可以是一个表达式或者变量,在上面的`<p>`标签中加入`v-if="text != ''"`表示当`text`为空的时候不显示  

```Html
<p v-if="text != ''">{{ text }}</p>
```

##### 循环语句  

`v-for`用于根据其值来决定当前元素创建多少个,值的写法为`item in items`,表示在items中遍历并返回item,`items`即为声明的数组,依然在上面的基础上修改,在`<p>`标签中加入` v-for="item in items"`,在vue的`data`中添加`items: [1, 2, 3]`表示数组items并初始化3个数值为`1,2,3`  
```Html
<p v-if="text != ''" v-for="item in items">{{ item }}: {{ text }}</p>
```

```js { .line-numbers highlight=5 }
var vm = new Vue({
    el: "#first",
    data: {
        text: "Hello World",
        items: [1, 2, 3] // items数组
    }
});
```

结果将会输出3个`<p>`元素  

##### 按钮点击事件  

`v-on:click`用于绑定一个函数,当被绑定的元素点击的时候会触发,在上面的代码基础上,新增一个按钮`<button>`并添加`v-on:click="OnShow()"`,在Vue中新增`methods:`并在其下实现`OnShow()`方法  

```Html
<button v-on:click="OnShow()">弹出</button>
```

```js { .line-numbers highlight=7-11 }
var vm = new Vue({
    el: "#first",
    data: {
        text: "Hello World",
        items: [1, 2, 3]
    },
    methods: { // 方法都在这个methods下面
        OnShow: function() { // 实现OnShow方法
            alert(this.text); // 弹出当前的文本
        }
    }
});
```

当点击按钮的时候将会弹出对话框,并显示当前input中的文本

##### 完整代码

```html
<html>
<head>
    <meta charset="UTF-8">
    <title>Document</title>
</head>
<body>
<div id="first">
    <p v-if="text != ''" v-for="item in items">{{ item }}: {{ text }}</p>
    <input v-model="text">
    <button v-on:click="OnShow()">弹出</button>
</div>
</body>
<script src="https://cdn.jsdelivr.net/npm/vue/dist/vue.js"></script>
<script>
    var vm = new Vue({
        el: "#first",
        data: {
            text: "Hello World",
            items: [1, 2, 3]
        },
        methods: {
            OnShow: function() {
                alert(this.text);
            }
        }
    });
</script>
</html>
```

## 最后

以上是最简单常用的语法,Vue很强大,还有很多更高级的用法还在学习中.如有问题可以一起讨论学习!
