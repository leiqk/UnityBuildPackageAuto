# UnityBuildPackageAuto

unity 实现一键打包功能

1、批处理调用shell；（可选）
2、Shell程序接收参数，并执行c#脚本函数（unity）调用；
3、C#脚本函数，执行打包相关的功能实现，包括 
	a、Build assetbundle
	b、重编代码（游戏代码作为dll供unity使用）
	c、生成lua代码（此代码未调用）
	d、打包选项设置
	e、打包
	f、对打出来的包进行相关处理（加密、重签，此示例未实现）

平台：windows（mac也可以参考）
依赖工具：bash（git），unity，visual studio

