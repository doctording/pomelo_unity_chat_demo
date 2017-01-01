
前言
===
本分支项目是一个简单的pomelo聊天小实例，采用unity5.4.1版本，可以学习下如何在unity中是用pomelo，与服务器完成交互。

关于如何clone非master分支的仓库：
===
利用-b指令选择合适的分支

例：git clone -b unity5.4.1 git@github.com:**

上面的unity5.4.1就是你想要选择的分支，后面还是clone的地址

关于如何保持自己fork的项目与源仓库同步更新
===
1.首先用git remote -v确定一下是否建立了主repo的远程源,如果只有origin的两个fetch和push，则需要第2步,否则直接第3步开始
2.git remote add upstream 源仓库的URL （这个命令用来添加源仓库）
3.git fetch upstream 
4.git merge upstream/unity5.4.1
5.git push orgin 5.4.1
（注意，到第4步的时候其实你本地已经更新好了，第5步就是更新的网络上你自己的本地仓库）

服务端
===
NetEase chatofpomelo

只需要安装模块，修改配置，运行game-server


客户端
===
unity3d5.4.1 使用pomelo的各种库文件，实现聊天功能


* ip采用了本地127.0.0.1,可能需要自行修改。

* 代码中游戏方法是过时的，也可能有错误，请自行debug修改

* Unity vs Debug 可以baidu、google


截图
===
可以在场景中看到运行界面，而不是采用OnGUI的画图的方法
![](http://i.imgur.com/856mpvb.png)

![](http://i.imgur.com/FZkFee1.png)


注
===

* 网上代码太多，但是很不友好，包括只有客户端，或只有服务端，或客户端与服务端不匹配等情况。（不过，确实要考验下自己搜索能力和Debug能力）

* 文档不更新，写的很不简洁明了的

* 本次代码是我网上搜到的，代码只调整了一个小地方。

* 欢迎交流，myblog: http://blog.csdn.net/qq_26437925
