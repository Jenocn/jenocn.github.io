---
title: 资源与逻辑分离,枚举法管理音频资源的播放
published: 2019-09-30 11:24:36
category: '编程'
tags: [经验]
---

执着于写一个思路清晰,便于维护和后期修改的代码是一个程序员的追求和技术素养. 这一篇讲一讲音频调用方式的书写技巧

<!-- more -->

## 例子  

播放一个背景音乐和按需播放音效是再普遍不过的功能需求了,最简单的方式就是直接播放音频资源,一般引擎都有提供播放音乐的功能

例如播放一个背景音乐:
```c++
AudioEngine::GetInstance()->PlayBGM("xxx/xxx/xxx/xxxxx.mp3");
```

例如播放一个音效:
```c++
AudioEngine::GetInstance()->PlaySound("xxx/xxx/xxx/xxxxx.mp3");
```

这种直接调用的方式,可以实现功能,但是对于后期修改音频文件名,或者替换音效的情况时,就需要在整个代码中去寻找并修改,是非常低效的

## 技巧  

对于处理这种情况,有很多解决方法,这里提供一个方法

对于音效和背景音乐,提供一个枚举,例如`开火声`,`跳跃`,`获得道具`等等具体的音效,放到这个枚举中

```c++
enum class eAudioBGM {
	None = 0,
	TitleScene, // 标题场景的bgm
};
enum class eAudioSound {
	None = 0,
	Fire, // 开火
	Jump, // 跳跃
	ItemGet, // 获得道具
};
```

有了以上的枚举,一切就方便起来了,到这里,大家肯定都明白了

**音乐播放器类**
```c++
class AudioPlayer {
public:
	void InitAudio();
	void PlayBGM(eAudioBGM bgm);
	int PlaySound(eAudioSound sound);
	// 更多的功能
	// ...
private:
	// 用于存放背景音乐路径的map
	std::map<eAudioBGM, std::string> _audioBgmMap;
	// 用于存放音效路径的map
	std::map<eAudioSound, std::string> _audioSoundMap;
	// 游戏引擎提供的底层音频类
	GameEngine::AudioEngine* _audioEngine { nullptr };
};
```
**实现**
```c++
void AudioPlayer::InitAudio() {
	// 这里就可以通过一个json/xml的配置文件来载入,并且标记更多的配置
	// 这里仅使用代码中配好的方式
	_audioBgmMap[eAudioBGM::TitleScene] = "music/title_scene.mp3";

	_audioSoundMap[eAudioSound::Fire] = "sound/fire.mp3";
	_audioSoundMap[eAudioSound::Jump] = "sound/jump.mp3";
	_audioSoundMap[eAudioSound::ItemGet] = "sound/item_get.mp3";

	// 获取单例,是为了后面方便调用,减少GetInstance()调用,这也是一个细节
	_audioEngine = GameEngine::AudioEngine::GetInstance();

	// 这里可以根据需要预加载
	// 例如音效我需要预加载
	for (const auto& pair : _audioSoundMap) {
		_audioEngine->Preload(pair.second);
	}
}
```
以上初始化,在游戏加载时调用一次就好了

```c++
void AudioPlayer::PlayBGM(eAudioBGM bgm) {
	auto ite = _audioBgmMap.find(bgm);
	if (ite != _audioBgmMap.end()) {
		_audioEngine->PlayBGM(ite->second)
	}
}
```
```c++
int AudioPlayer::PlaySound(eAudioSound sound) {
	auto ite = _audioSoundMap.find(sound);
	if (ite != _audioSoundMap.end()) {
		return _audioEngine->PlaySound(ite->second)
	}
	return -1;
}
```

**调用**

```c++
// bgm
AudioPlayer::GetInstance()->PlayBGM(eAudioBGM::TitleScene);
```

```c++
// sound
AudioPlayer::GetInstance()->PlaySound(eAudioSound::Fire);
```

以上,就完整的将音频模块包装了一下,如果再要添加或者修改一个音频,都只需要在InitAudio中修改就好了,即使是删除一个音效,删除枚举,调用的地方IDE也会提示

