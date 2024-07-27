---
title: C++实现矩阵模板类
description: 在计算机中，矩阵有着广泛的应用
published: 2024-02-29 19:00:00
category: '编程'
tags: [C++]
---

在计算机中，矩阵有着广泛的应用，例如计算机图形学中，使用矩阵可以轻松的解决缩放、移动、旋转的变换问题。下面使用C++模板实现矩阵类，其中，数据类型，矩阵行列都通过模板参数来指定。

<!-- more -->

```cpp
#pragma once

#include <cassert>
#include <vector>

template <typename TNUMBER, int ROW_, int COL_>
class Matrix {
public:
	Matrix();
	Matrix(const Matrix<TNUMBER, ROW_, COL_>& target);
	Matrix(Matrix<TNUMBER, ROW_, COL_>&& target);
	Matrix(std::initializer_list<TNUMBER>&& data);
	Matrix(std::vector<TNUMBER>&& data);
	virtual ~Matrix() = default;

	bool operator==(const Matrix<TNUMBER, ROW_, COL_>& target) const;
	bool operator!=(const Matrix<TNUMBER, ROW_, COL_>& target) const;
	Matrix<TNUMBER, ROW_, COL_>& operator=(const Matrix<TNUMBER, ROW_, COL_>& target);

	Matrix<TNUMBER, ROW_, COL_> operator+(TNUMBER num) const;
	Matrix<TNUMBER, ROW_, COL_> operator-(TNUMBER num) const;
	Matrix<TNUMBER, ROW_, COL_> operator*(TNUMBER num) const;
	Matrix<TNUMBER, ROW_, COL_> operator/(TNUMBER num) const;

	Matrix<TNUMBER, ROW_, COL_> operator+(const Matrix<TNUMBER, ROW_, COL_>& target) const;
	Matrix<TNUMBER, ROW_, COL_> operator-(const Matrix<TNUMBER, ROW_, COL_>& target) const;

	template <int TARGET_COL_>
	Matrix<TNUMBER, ROW_, TARGET_COL_> Multiply(const Matrix<TNUMBER, COL_, TARGET_COL_>& target) const;

	template <int X_, int Y_>
	void Set(TNUMBER data);
	template <int X_, int Y_>
	TNUMBER Get() const;
	void Set(int x, int y, TNUMBER data);
	TNUMBER Get(int x, int y) const;

	template <typename TContainer>
	void Reset(TContainer& data);
	template <typename TContainer>
	void Reset(TContainer&& data);
	const std::vector<TNUMBER>& Data() const;
	constexpr int Row() const;
	constexpr int Column() const;
	constexpr int Size() const;

private:
	std::vector<TNUMBER> _data;
	static constexpr int _SIZE{ ROW_ * COL_ };
};

template <typename TNUMBER, int ROW_, int COL_>
Matrix<TNUMBER, ROW_, COL_>::Matrix() {
	static_assert(!std::is_class_v<TNUMBER>, "TNUMBER type error");
	_data.resize(_SIZE, 0);
}
template <typename TNUMBER, int ROW_, int COL_>
Matrix<TNUMBER, ROW_, COL_>::Matrix(const Matrix<TNUMBER, ROW_, COL_>& target) {
	_data = target._data;
}
template <typename TNUMBER, int ROW_, int COL_>
Matrix<TNUMBER, ROW_, COL_>::Matrix(Matrix<TNUMBER, ROW_, COL_>&& target) {
	_data = std::move(target._data);
}

template <typename TNUMBER, int ROW_, int COL_>
Matrix<TNUMBER, ROW_, COL_>::Matrix(std::initializer_list<TNUMBER>&& data) {
	Reset(std::move(data));
}

template <typename TNUMBER, int ROW_, int COL_>
Matrix<TNUMBER, ROW_, COL_>::Matrix(std::vector<TNUMBER>&& data) {
	Reset(std::move(data));
}

template <typename TNUMBER, int ROW_, int COL_>
bool Matrix<TNUMBER, ROW_, COL_>::operator==(const Matrix<TNUMBER, ROW_, COL_>& target) const {
	return _data == target._data;
}
template <typename TNUMBER, int ROW_, int COL_>
bool Matrix<TNUMBER, ROW_, COL_>::operator!=(const Matrix<TNUMBER, ROW_, COL_>& target) const {
	return _data != target._data;
}
template <typename TNUMBER, int ROW_, int COL_>
Matrix<TNUMBER, ROW_, COL_>& Matrix<TNUMBER, ROW_, COL_>::operator=(const Matrix<TNUMBER, ROW_, COL_>& target) {
	_data = target._data;
	return *this;
}
template <typename TNUMBER, int ROW_, int COL_>
Matrix<TNUMBER, ROW_, COL_> Matrix<TNUMBER, ROW_, COL_>::operator+(TNUMBER num) const {
	auto ret = *this;
	for (int i = 0; i < _SIZE; ++i) {
		ret._data[i] += num;
	}
	return ret;
}
template <typename TNUMBER, int ROW_, int COL_>
Matrix<TNUMBER, ROW_, COL_> Matrix<TNUMBER, ROW_, COL_>::operator-(TNUMBER num) const {
	auto ret = *this;
	for (int i = 0; i < _SIZE; ++i) {
		ret._data[i] -= num;
	}
	return ret;
}
template <typename TNUMBER, int ROW_, int COL_>
Matrix<TNUMBER, ROW_, COL_> Matrix<TNUMBER, ROW_, COL_>::operator*(TNUMBER num) const {
	auto ret = *this;
	for (int i = 0; i < _SIZE; ++i) {
		ret._data[i] *= num;
	}
	return ret;
}
template <typename TNUMBER, int ROW_, int COL_>
Matrix<TNUMBER, ROW_, COL_> Matrix<TNUMBER, ROW_, COL_>::operator/(TNUMBER num) const {
	auto ret = *this;
	for (int i = 0; i < _SIZE; ++i) {
		ret._data[i] /= num;
	}
	return ret;
}

template <typename TNUMBER, int ROW_, int COL_>
Matrix<TNUMBER, ROW_, COL_> Matrix<TNUMBER, ROW_, COL_>::operator+(const Matrix<TNUMBER, ROW_, COL_>& target) const {
	auto ret = *this;
	for (int i = 0; i < _SIZE; ++i) {
		ret._data[i] += target._data[i];
	}
	return ret;
}
template <typename TNUMBER, int ROW_, int COL_>
Matrix<TNUMBER, ROW_, COL_> Matrix<TNUMBER, ROW_, COL_>::operator-(const Matrix<TNUMBER, ROW_, COL_>& target) const {
	auto ret = *this;
	for (int i = 0; i < _SIZE; ++i) {
		ret._data[i] += target._data[i];
	}
	return ret;
}

template <typename TNUMBER, int ROW_, int COL_>
template <int TARGET_COL_>
Matrix<TNUMBER, ROW_, TARGET_COL_> Matrix<TNUMBER, ROW_, COL_>::Multiply(const Matrix<TNUMBER, COL_, TARGET_COL_>& target) const {
	constexpr int tsize = ROW_ * TARGET_COL_;
	std::vector<TNUMBER> retData;
	retData.reserve(tsize);
	for (int i = 0; i < ROW_; ++i) {
		for (int j = 0; j < TARGET_COL_; ++j) {
			TNUMBER val = (TNUMBER)0;
			for (int k = 0; k < COL_; ++k) {
				int indexA = i * COL_ + k;
				int indexB = k * TARGET_COL_ + j;
				val += (_data[indexA] * target.Data()[indexB]);
			}
			retData.emplace_back(val);
		}
	}
	return Matrix<TNUMBER, ROW_, TARGET_COL_>(retData);
}

template <typename TNUMBER, int ROW_, int COL_>
template <int X_, int Y_>
void Matrix<TNUMBER, ROW_, COL_>::Set(TNUMBER data) {
	static_assert(X_ >= 0 && X_ < COL_ && Y_ >= 0 && Y_ < ROW_, "out of range");
	constexpr int index = Y_ * ROW_ + X_;
	_data[index] = data;
}

template <typename TNUMBER, int ROW_, int COL_>
template <int X_, int Y_>
TNUMBER Matrix<TNUMBER, ROW_, COL_>::Get() const {
	static_assert(X_ >= 0 && X_ < COL_ && Y_ >= 0 && Y_ < ROW_, "out of range");
	constexpr int index = Y_ * ROW_ + X_;
	return _data[index];
}

template <typename TNUMBER, int ROW_, int COL_>
void Matrix<TNUMBER, ROW_, COL_>::Set(int x, int y, TNUMBER data) {
	assert(x < COL_ && y < ROW_ && "out of range");
	int index = y * ROW_ + x;
	_data[index] = data;
}

template <typename TNUMBER, int ROW_, int COL_>
TNUMBER Matrix<TNUMBER, ROW_, COL_>::Get(int x, int y) const {
	assert(x < COL_ && y < ROW_ && "out of range");
	int index = y * ROW_ + x;
	return _data[index];
}

template <typename TNUMBER, int ROW_, int COL_>
template <typename TContainer>
void Matrix<TNUMBER, ROW_, COL_>::Reset(TContainer& data) {
	Reset(TContainer(data));
}

template <typename TNUMBER, int ROW_, int COL_>
template <typename TContainer>
void Matrix<TNUMBER, ROW_, COL_>::Reset(TContainer&& data) {
	static_assert(!std::is_class_v<TNUMBER>, "TNUMBER type error");
	if (data.size() == _SIZE) {
		_data = std::move(data);
	}
	else if (data.size() > _SIZE) {
		assert(!"Matrix init warning!");
		_data.reserve(_SIZE);
		_data.insert(_data.end(), data.begin(), data.begin() + _SIZE);
	}
	else if (data.size() < _SIZE) {
		assert(!"Matrix init warning!");
		_data.reserve(_SIZE);
		_data = std::move(data);
		for (int i = 0; i < _SIZE - data.size(); ++i) {
			_data.emplace_back((TNUMBER)0);
		}
	}
}

template <typename TNUMBER, int ROW_, int COL_>
const std::vector<TNUMBER>& Matrix<TNUMBER, ROW_, COL_>::Data() const {
	return _data;
}

template <typename TNUMBER, int ROW_, int COL_>
constexpr int Matrix<TNUMBER, ROW_, COL_>::Row() const {
	return ROW_;
}
template <typename TNUMBER, int ROW_, int COL_>
constexpr int Matrix<TNUMBER, ROW_, COL_>::Column() const {
	return COL_;
}
template <typename TNUMBER, int ROW_, int COL_>
constexpr int Matrix<TNUMBER, ROW_, COL_>::Size() const {
	return _SIZE;
}

```