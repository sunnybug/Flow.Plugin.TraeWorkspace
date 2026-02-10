# 将Flow\.Plugin.TraeWorkspace改造成支持Trae的插件

## 1. 项目结构调整

* 创建必要的目录结构，包括：

  * Images/ - 存放图标文件

  * Properties/ - 存放资源文件和本地化文件

  * RemoteMachinesHelper/ - 远程机器相关代码

  * SshConfigParser/ - SSH配置解析器

  * TraeHelper/ - Trae编辑器相关代码

  * WorkspacesHelper/ - 工作区相关代码

## 2. 核心文件修改

* **Main.cs**：

  * 实现IPlugin、IPluginI18n、ISettingProvider、IContextMenu接口

  * 添加Trae工作区和远程机器的查询逻辑

  * 实现工作区和远程机器的启动功能

* **plugin.json**：

  * 更新插件信息，确保描述准确

  * 可能需要更新ActionKeyword

## 3. 实现Trae相关类

* **TraeHelper/TraeInstance.cs**：

  * 类似VSCodeInstance，用于表示Trae编辑器实例

* **TraeHelper/TraeInstances.cs**：

  * 类似VSCodeInstances，用于检测和加载Trae编辑器实例

  * 修改路径检测逻辑，适配Trae的安装路径

* **WorkspacesHelper/TraeWorkspacesApi.cs**：

  * 类似CursorWorkspacesApi，用于加载Trae的最近工作区

  * 修改存储文件路径，适配Trae的数据存储位置

## 4. 资源和本地化

* 添加资源文件（Resources.resx）

* 添加本地化文件，支持多语言

## 5. 设置面板

* 创建设置视图（SettingsView\.xaml和SettingsView\.xaml.cs）

* 实现设置存储和加载逻辑

## 6. 图标文件

* 添加必要的图标文件到Images目录

  * folder.png - 文件夹图标

  * monitor.png - 远程连接图标

## 7. 功能实现

* 工作区检测和加载

* 远程机器（SSH）检测和连接

* 工作区和远程机器的搜索功能

* 启动Trae编辑器打开工作区或连接远程机器

## 8. 测试和调试

* 确保插件能正确检测Trae编辑器

* 确保能正确加载和显示最近工作区

* 确保能正确启动Trae打开工作区

* 确保远程连接功能正常工作

## 9. 构建和发布

* 更新.csproj文件，确保依赖项正确

* 确保构建过程正常

## 注意事项

* 所有与Cursor相关的代码都需要修改为与Trae相关

* 需要适配Trae的安装路径和数据存储位置

* 需要确保插件能正确处理Trae的工作区格式

* 需要保持与参考插件相似的功能和用户体验

