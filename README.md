b站弹幕姬(旬
=======

哔哩哔哩非官方弹幕姬软件十周年特别版。

## Q&A

### “旬”是什么意思？

就是十（decade）的意思，表示这是十周年特别版(ry

### 开发十周年特别版的动机是什么？有什么特殊的地方吗？

十年前，也就是 2014 年的 8 月 31 日，copyliu 发布了弹幕姬的第一个公开版本，自那之后的十年间发生了很多事情，但弹幕姬本身的技术架构却基本没什么大改动……这十年间偶尔也会有一些好心人贡献一些新特性新功能，但总体来说它确实欠缺了很多当今的软件普遍该有的特点，无论是功能、健壮性还是易用性和可扩展性（以及卖相）；之前我们也试图对其进行一些较大的改动，但因为时间仓促缺少 QA 导致当时未能及时发现一些影响既有用户使用的潜在问题，于是不得不退回修改前的版本。

本次十周年特别版将作为一个独立分支进行开发，以便进行功能特性方面的快速迭代；当合适的时机来临时再研究怎么合并回主干。

### 十周年特别版都会有什么样的功能特性？

目前已经计划开发如下功能/特性：
* 一个正常能用的皮肤管理器
* 一个正经的可扩展架构
* 脚本编程支持
* 一个支持多个分发源的插件下载和管理模块
* 网页互操作功能

其中，第一期会先实现 VBScript 和 JScript(ECMAScript 3)、Chakra(ECMAScript 5) 支持，其它语言、版本的支持将作为未来的长期研究目标。

### 有可能支持 macOS 吗？

当前弹幕姬的技术架构是以 .NET Framework 为基础的，并且强依赖若干 Windows 特性；向其它平台移植需要投入调研成本，目前已经有了大致方向，但仍需要一定的时间和精力进行研究试错。

### 考虑过使用 Web 前端技术来进行重构吗？

既有的插件都是基于 .NET Framework 并且存在相当数量强烈依赖对 WPF 进行反射访问的，使用 Web 前端技术来进行重构不现实。
