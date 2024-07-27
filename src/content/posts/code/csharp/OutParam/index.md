---
title: 'C#中使用out参数时需要注意的点'
published: 2020-06-08 10:15:55
category: '编程'
tags: ['CSharp']
---

说说C#中out关键字的作用和需要注意的地方

<!-- more -->

---

## out关键字
表示这个参数由函数来**决定其值**,类似一个返回值,一般应用在某个函数需要多个返回值的场景,例如常用的内置方法`TryGetValue<TK, TV>(TK key, out TV value)`  
```csharp  
var _valueDict = new Dictionary<int, int>();
int value;
if (_valueDict.TryGetValue(10, out value)) {
	// todo...
	// print(value);
}
```

## 注意
划重点,out的机制是由函数决定其值,所以在执行函数之前的赋值将无效,视为额外的代码开销,下面展示一段错误的书写方式:  
```csharp
var _valueDict = new Dictionary<int, int>();
// ...
int GetValue(int key) {
	int value = -1; // 假设我希望默认值是-1
	if (_valueDict.TryGetValue(key, out value)) {
		// todo...
	}
	return value;
}
// ...
int ret = GetValue(10);
if (ret == -1) {
	// todo...
	// ret的值是多少呢?
}
```
根据out的特性,由函数内部决定其值,所以如果`TryGetValue`没有找到时,value并不等于之前的值,而是其他值,可能为0,但我更愿意理解为未定义的值,假设你的参数是一个class或者struct呢?假设是其他第三方库写的方法呢?这完全依赖于内部实现,所以最好的办法是:
```csharp
var _valueDict = new Dictionary<int, int>();
int value;
if (!_valueDict.TryGetValue(10, out value)) {
	value = -1;
}
```
对于一个书写习惯良好的程序员来说,任何声明都应该赋初值,我也有这个习惯,所以上面的`int value;`显得非常不舒服,但在这里的确不建议赋初值,因为赋值没有意义且会增加额外的开销,所以C#提供了另一种写法,解决了这个问题,out后可跟声明符:   
```csharp
var _valueDict = new Dictionary<int, int>();
if (!_valueDict.TryGetValue(10, out var value)) {
	value = -1;
}
```

## 最后谈谈ref关键字
其实out与ref的意义不同,但是同样作为参数修饰,可以放到一起谈谈,ref更像是通常意义上(C++中)的引用,它表示一个参数作为引用传递到函数内部,函数内部可以使用该值也可以对其进行赋值,当然也可以不赋值,所以在使用ref时,这个参数在函数调用之前的赋值是有效的,那么说到区别,out是强调这个值一定是会被赋值的,而ref则更通用一些.



