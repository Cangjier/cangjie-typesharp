# Cangjie.TypeSharp

Target: Eval type script with csharp runtime.

# Road Map

## 2024

目标：支持typescript标准语法，但是不包括包管理、class、interface等代码组织。

# To Do

1. || 或者 && 需要在Text期间就分割好，目前有BUG，如果||两侧存在更高优先级的操作符，会先进入steps
2. 在()中出现:时，支持将:后续的{}识别为类型

