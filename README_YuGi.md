# YuGi 桌面宠物

一个基于 WPF 开发的可爱桌面宠物应用，使用 YuGi 角色的 14 帧闲置动画。

## 功能特性

### 核心功能
- ✨ **流畅动画**: 使用 14 帧序列帧实现流畅的闲置动画（10 FPS）
- 🎬 **登场动画**: 从屏幕左下角登场，缓慢向右移动
- 🖱️ **自由拖拽**: 左键点击宠物可以拖动到屏幕任意位置
- 🎯 **智能移动**: 宠物会随机在屏幕上移动，带有平滑的缓动效果
- 🔝 **窗口置顶**: 宠物始终显示在其他窗口之上
- 👻 **透明背景**: 完全透明的窗口背景，只显示角色本身

### 交互功能
- 🖱️ **右键菜单**: 右键点击宠物显示快捷菜单
  - 隐藏宠物
  - 退出程序
- 📍 **系统托盘**: 最小化到系统托盘
  - 左键单击：显示/隐藏宠物
  - 右键菜单：完整控制选项
  - 自动移动开关
- 💾 **位置记忆**: 自动保存和恢复宠物的屏幕位置

## 技术实现

### 开发环境
- **框架**: .NET 8.0
- **UI框架**: WPF (Windows Presentation Foundation)
- **语言**: C# 12
- **IDE**: Visual Studio 2022

### 核心技术
1. **透明窗口**: 使用 `AllowsTransparency` 和 `WindowStyle="None"` 实现无边框透明窗口
2. **动画系统**: `DispatcherTimer` 控制帧率，循环播放 14 帧序列图
3. **平滑移动**: `DoubleAnimation` 配合 `QuadraticEase` 缓动函数实现自然移动
4. **拖拽功能**: WPF 内置的 `DragMove()` 方法
5. **系统托盘**: 使用 `System.Windows.Forms.NotifyIcon` 实现托盘图标
6. **配置持久化**: JSON 格式保存用户配置到 AppData

### 项目结构
```
DesktopPet/
├── Assets/
│   └── YuGi_Idle/          # 14 帧序列图 (01.png - 14.png)
├── MainWindow.xaml          # 主窗口 UI 定义
├── MainWindow.xaml.cs       # 主窗口逻辑
├── PetConfig.cs             # 配置管理类
├── App.xaml                 # 应用程序定义
└── DesktopPet.csproj        # 项目文件
```

## 使用说明

### 运行程序
1. 双击 `run.bat` 或直接运行编译后的 exe 文件
2. 宠物会从屏幕左下角登场
3. 自动缓慢向右移动（约 18 秒）
4. 移动完成后开始随机移动模式
5. 程序会自动最小化到系统托盘

### 基本操作
- **移动宠物**: 左键点击并拖动
- **显示菜单**: 右键点击宠物
- **托盘控制**: 点击系统托盘图标
- **退出程序**: 右键菜单选择"退出"或托盘菜单选择"退出"

### 配置文件
配置文件自动保存在：
```
%AppData%\DesktopPet\config.json
```

配置项说明：
```json
{
  "WindowLeft": 1720.0,        // 窗口 X 坐标
  "WindowTop": 880.0,          // 窗口 Y 坐标
  "AnimationSpeed": 100,       // 动画速度（毫秒/帧）
  "AutoMove": true,            // 是否自动移动
  "MoveIntervalMin": 3,        // 移动间隔最小值（秒）
  "MoveIntervalMax": 7         // 移动间隔最大值（秒）
}
```

## 开发指南

### 构建项目
```bash
# 使用 .NET CLI
dotnet build

# 或使用 Visual Studio
# 打开 DesktopPet.csproj，按 F5 运行
```

### 自定义动画
如果要更换其他角色的序列帧：

1. 准备序列帧图片（PNG 格式，透明背景）
2. 将图片放入 `Assets/YuGi_Idle/` 文件夹
3. 命名格式：`01.png`, `02.png`, ..., `14.png`
4. 修改 `MainWindow.xaml.cs` 中的帧数：
```csharp
idleFrames = new BitmapImage[14]; // 修改为实际帧数
```

### 调整动画速度
在 `PetConfig.cs` 中修改默认值：
```csharp
public int AnimationSpeed { get; set; } = 100; // 毫秒/帧
```
- 值越小，动画越快
- 推荐范围：80-150ms
- 10 FPS = 100ms，12 FPS ≈ 83ms

### 调整窗口大小
在 `MainWindow.xaml` 中修改：
```xml
Width="250"
Height="250"
```

## 性能优化

- ✅ 使用 `DispatcherTimer` 而非高频率定时器
- ✅ 动画帧率控制在 10 FPS，降低 CPU 占用
- ✅ 使用 `BitmapScalingMode.HighQuality` 保证图像质量
- ✅ 移动动画使用缓动函数，减少计算量
- ✅ 配置文件异步保存，不阻塞 UI

## 已知问题

- 在某些高 DPI 显示器上可能需要调整窗口大小
- 多显示器环境下，宠物可能移动到其他屏幕

## 未来计划

- [ ] 添加多种动画状态（行走、跑步、睡觉等）
- [ ] 支持多个宠物同时运行
- [ ] 添加宠物互动功能（点击反馈）
- [ ] 支持自定义皮肤切换
- [ ] 添加声音效果
- [ ] 屏幕边缘检测和反弹效果优化

## 许可证

本项目仅供学习和个人使用。

## 致谢

- YuGi 角色素材来自游戏资源
- 开发指南参考：`DesktopPet_DevGuide.md`
