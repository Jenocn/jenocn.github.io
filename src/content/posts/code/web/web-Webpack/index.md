---
title: Webpack初始化
published: 2019-05-01 16:52:15
category: '编程'
tags: [前端]
---

引用官网对 webpack 的解释:
> 本质上，webpack 是一个现代 JavaScript 应用程序的静态模块打包器(module bundler)。

<!-- more -->

### 为什么要使用Webpack

前端开发,有很多优秀的第三方库可以使用,使用`npm`工具,轻松的下载并使用这些库,但是呢,直接`require`这些库,在浏览器中是无法找到的,于是就需要这样一个工具,将`require`引用到的库打包到一起,这样,我们的应用才可以被浏览器正确打开.

如果要使用`npm`工具,就势必要使用`Webpack`,我因为要使用`Electron`和`Vue`开发桌面应用,还有一些`node.js`中很好用的方法,所以学习一下`Webpack`,这里记录一下,以便后期回顾.

### Webpack初始化

##### 获取`Webpack`到工程
假设你已经安装了`npm`,使用命令:
```node
npm i webpack --save-dev
```
安装`webpack`到开发环境下

##### 添加配置文件
在根目录下新建文件,命名为`webpack.config.js`,并写上
```js
const path = require('path');
module.exports = {
    // 指定入口js文件
    entry: {
        index: './js/index.js'
    },
    // 指定输出的js文件名和路径
    output: {
        filename: 'index.bundle.js',
        path: path.resolve(__dirname, 'dist')
    }
};
```
创建`js/index.js`文件

##### 在`package.json`中添加`build`命令
```json
"scripts": {
    "build": "webpack --config webpack.config.js"
  },
```
然后使用命令`npm run build`就会看到工程目录下生成了一个`dist`文件夹,里面有一个`index.bundle.js`文件,这个就可以拿到html中去使用了

到这里,简单的配置就完毕了,下面结合`html`和`vue`来使用`webpack`

##### 通过`npm`安装`vue`
```node
npm i vue --save
```

##### 新建`index.html`文件
```html
<html lang="en">
<head>
    <meta charset="UTF-8">
    <title>Document</title>
</head>
<body>
    <div id="app">
        <p>{{ message }}</p>
    </div>
    <script src="./dist/index.bundle.js"></script>
</body>
</html>
```
这个页面中使用了Vue的语法`{{ message }}`
然后引用的是`webpack`导出的`index.bundle.js`文件

##### 在`js/index.js`中写入
```js
import Vue from 'vue';

new Vue({
    el: '#app',
    data: {
      message: 'Hello World!',
    }
});
```

创建vue实例,然后初始`message`为"Hello World!"

关于`vue`上篇文章<a href="https://jenocn.github.io/2019/04/Vue-Good/">《Vue 超棒的!》</a>有简单的介绍

##### 在`webpack`中配置vue
在`webpack.config.js`中添加配置:
```js
module.exports = {
    entry: {
        index: './js/index.js'
    },
    output: {
        filename: 'index.bundle.js',
        path: path.resolve(__dirname, 'dist')
    },
    // 新增加的
    resolve: {
        alias: { 'vue': 'vue/dist/vue.js' }
    }
};
```

然后执行`npm run build`,在dis目录下重新生成`index.bundle.js`,打开`index.html`就可以看到vue生效了

##### 开启调试(定位错误到源代码位置)

因为上面html中使用的是导出的js文件,这样当出现错误的时候,很难找到错误的位置,所以开启调试,通过source-map定位错误的位置,下面有两种做法

- 内联的方式
直接在webpack的配置文件`webpack.config.js`中,添加新的属性
```js
module.exports = {
    entry: {
        index: './js/index.js'
    },
    output: {
        filename: 'index.bundle.js',
        path: path.resolve(__dirname, 'dist')
    },
    resolve: {
        alias: { 'vue': 'vue/dist/vue.js' }
    },
    // 新添加的
    devtool: 'inline-source-map'
};
```
这样就可以了,当出现错误的时候,在浏览器调试界面的控制台中就可以看到具体出错的位置了
这种方式其实是把数据都写入了`index.bundle.js`中

- 生成.map文件的方式
就是将`devtool: 'inline-source-map'`改为`devtool: 'source-map'`就可以了,这样会在输出的文件目录下,生成一个同名的.map文件. 在出错的时候,也是可以看到出错位置的
这种方式可以看到`index.bundle.js`文件容量小了很多,一般推荐这种方式

总之,调试信息在正式发布的时候一般是不使用的.

##### 再增加一个vue实例
情况稍微复杂一点了,新增了一个js文件,命名为`component.js`
```js
import Vue from 'vue';
new Vue({
    el: '#component',
    data: {
      message: 'Hello Component!',
    }
});
```
相应的,在html中新增:
```html
<div id="component">
    <p>{{ message }}</p>
</div>
<script src="./dist/component.bundle.js"></script>
```
然后在配置中配置新的js入口和输出
```js
module.exports = {
    entry: {
        index: './js/index.js',
        // 新添加的
        component: './js/component.js',
    },
    devtool: 'source-map',
    output: {
        // 这里通过入口的属性name来分别命名
        filename: '[name].bundle.js',
        path: path.resolve(__dirname, 'dist')
    },
    resolve: {
        alias: { 'vue': 'vue/dist/vue.js' }
    }
};
```

然后执行命令`npm run build`可以看到dist下面导出的文件,直接打开html可以看到效果.

##### 解决公共依赖
上面两个js文件中都`import Vue from 'vue'`,这样导出的两个`bundle.js`文件中都各自包含了一份vue的js代码,这样无疑是重复了,使得容量非常大,在`webpack`中增加一个配置,就可以解决这个问题.

`webpack.config.js`中:
```js
module.exports = {
    entry: {
        index: './js/index.js',
        component: './js/component.js',
    },
    devtool: 'source-map',
    output: {
        filename: '[name].bundle.js',
        path: path.resolve(__dirname, 'dist')
    },
    resolve: {
        alias: { 'vue': 'vue/dist/vue.js' }
    },
    // 新添加的
    optimization: {
        splitChunks: {
            // 表示公共依赖导出到'common.bundle.js'中
            name: 'common',
            // 将所有公共部分导出到一处
            chunks: 'all'
        }
    }
};
```

然后在执行`npm run build`查看`dist`下的文件,多了一个`common.bundle.js`并且另外两个js文件的容量小了很多

### 最后  

本文就到这里了,目前呢,基本上是满足我的需求了,webpack还有很多功能,比如导出资源,还有很多插件,都是提高开发效率的,待以后慢慢深入!本文呢作为一个记录和入门,一方面自己以后可以回头看看,另一方面希望能够帮到正在学习的朋友.  

关于Vue:《Vue 超棒的!》
<a href="https://jenocn.github.io/2019/04/Vue-Good/" target="_blank">https://jenocn.github.io/2019/04/Vue-Good/</a>  

