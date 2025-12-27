# Cangjie.TypeSharp

Target: Eval type script with csharp runtime.

# Road Map

## 2024

目标：支持typescript标准语法，但是不包括包管理、class、interface等代码组织。

# To Do

1. 在()中出现:时，支持将:后续的{}识别为类型
2. 支持this ？？？
3. 当try中return时，并不会执行finally的bug
4. 支持[].push(...[1,2,3])中的...
5. 支持let [a,b]=[1,2];
6. 支持let {a,b}=obj;

