using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace DesktopPet
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer animationTimer = null!;
        private DispatcherTimer movementTimer = null!;
        private int currentFrame = 0;
        private BitmapImage[] idleFrames = null!;
        private Random random = new Random();
        private bool isMoving = false;
        private NotifyIcon? trayIcon;
        private PetConfig config = null!;

        public MainWindow()
        {
            InitializeComponent();
            
            // 加载配置
            config = PetConfig.Load();
            
            // 设置窗口初始位置在屏幕左下角
            this.Left = 10; // 左边距 10 像素
            this.Top = SystemParameters.PrimaryScreenHeight - this.Height - 50; // 底部边距 50 像素

            // 初始化动画和移动
            InitializeAnimation();
            InitializeMovement();
            InitializeTrayIcon();
            
            // 启动后立即开始向右缓慢移动
            StartInitialRightMovement();
        }

        private void InitializeAnimation()
        {
            // 加载 14 帧 YuGi 闲置动画
            idleFrames = new BitmapImage[14];
            for (int i = 0; i < idleFrames.Length; i++)
            {
                // YuGi_Idle 文件夹中的图片命名为 01.png 到 14.png
                string frameNumber = (i + 1).ToString("D2");
                idleFrames[i] = new BitmapImage(new Uri($"pack://application:,,,/Assets/YuGi_Idle/{frameNumber}.png"));
            }

            // 设置动画定时器，使用配置的速度
            animationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(config.AnimationSpeed)
            };
            animationTimer.Tick += AnimationTimer_Tick;
            animationTimer.Start();
        }

        private void InitializeMovement()
        {
            // 每 3-6 秒随机决定是否移动（使用配置）
            movementTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(random.Next(config.MoveIntervalMin, config.MoveIntervalMax))
            };
            movementTimer.Tick += MovementTimer_Tick;
            
            if (config.AutoMove)
            {
                movementTimer.Start();
            }
        }

        private void StartInitialRightMovement()
        {
            // 从左下角缓慢向右移动到屏幕右侧
            isMoving = true;

            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double targetX = screenWidth - this.Width - 10; // 移动到右边，留 10 像素边距

            // 创建缓慢的向右移动动画（使用配置的持续时间）
            Duration duration = new Duration(TimeSpan.FromSeconds(config.InitialMoveDuration));

            DoubleAnimation moveX = new DoubleAnimation
            {
                To = targetX,
                Duration = duration,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            // 动画完成后重置移动状态，并开始随机移动
            moveX.Completed += (s, e) => 
            { 
                isMoving = false;
                // 初始移动完成后，如果启用了自动移动，则开始随机移动
                if (config.AutoMove && !movementTimer.IsEnabled)
                {
                    movementTimer.Start();
                }
            };

            this.BeginAnimation(Window.LeftProperty, moveX);
        }

        private void InitializeTrayIcon()
        {
            try
            {
                // 创建系统托盘图标
                trayIcon = new NotifyIcon
                {
                    Icon = SystemIcons.Application,
                    Visible = true,
                    Text = "桌面宠物 - YuGi"
                };

                // 创建右键菜单（使用 ContextMenuStrip 替代旧的 ContextMenu）
                var trayMenu = new ContextMenuStrip();
                
                var showItem = new ToolStripMenuItem("显示宠物");
                showItem.Click += (s, e) => { this.Dispatcher.Invoke(() => { this.Show(); this.WindowState = WindowState.Normal; }); };
                
                var hideItem = new ToolStripMenuItem("隐藏宠物");
                hideItem.Click += (s, e) => { this.Dispatcher.Invoke(() => { this.Hide(); }); };
                
                var toggleMoveItem = new ToolStripMenuItem("自动移动")
                {
                    Checked = config.AutoMove,
                    CheckOnClick = true
                };
                toggleMoveItem.Click += (s, e) => 
                { 
                    this.Dispatcher.Invoke(() => 
                    { 
                        config.AutoMove = toggleMoveItem.Checked;
                        if (config.AutoMove)
                            movementTimer.Start();
                        else
                            movementTimer.Stop();
                        config.Save();
                    }); 
                };
                
                var settingsItem = new ToolStripMenuItem("设置");
                settingsItem.Click += (s, e) => { this.Dispatcher.Invoke(() => { ShowSettingsDialog(); }); };
                
                var exitItem = new ToolStripMenuItem("退出");
                exitItem.Click += (s, e) => { this.Dispatcher.Invoke(() => { System.Windows.Application.Current.Shutdown(); }); };

                trayMenu.Items.Add(showItem);
                trayMenu.Items.Add(hideItem);
                trayMenu.Items.Add(new ToolStripSeparator());
                trayMenu.Items.Add(toggleMoveItem);
                trayMenu.Items.Add(settingsItem);
                trayMenu.Items.Add(new ToolStripSeparator());
                trayMenu.Items.Add(exitItem);

                trayIcon.ContextMenuStrip = trayMenu;

                // 双击托盘图标显示窗口
                trayIcon.DoubleClick += (s, e) => { this.Dispatcher.Invoke(() => { this.Show(); this.WindowState = WindowState.Normal; }); };
                
                // 单击托盘图标也显示窗口（更方便）
                trayIcon.Click += (s, e) => 
                { 
                    if (((System.Windows.Forms.MouseEventArgs)e).Button == MouseButtons.Left)
                    {
                        this.Dispatcher.Invoke(() => 
                        { 
                            if (this.Visibility == Visibility.Visible)
                                this.Hide();
                            else
                            {
                                this.Show(); 
                                this.WindowState = WindowState.Normal;
                            }
                        }); 
                    }
                };

                // 移除启动提示，改为静默启动
                // System.Windows.MessageBox.Show("托盘图标已创建！请查看系统托盘（任务栏右下角）", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"托盘图标创建失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            // 循环播放帧
            currentFrame = (currentFrame + 1) % idleFrames.Length;
            PetImage.Source = idleFrames[currentFrame];
        }

        private void MovementTimer_Tick(object? sender, EventArgs e)
        {
            // 50% 概率移动
            if (random.Next(0, 2) == 0 && !isMoving)
            {
                MoveToRandomPosition();
            }

            // 重置定时器间隔（使用配置）
            movementTimer.Interval = TimeSpan.FromSeconds(random.Next(config.MoveIntervalMin, config.MoveIntervalMax));
        }

        private void MoveToRandomPosition()
        {
            isMoving = true;

            // 计算屏幕可用区域
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            // 随机目标位置（留出边距）
            double targetX = random.Next(0, (int)(screenWidth - this.Width));
            double targetY = random.Next(0, (int)(screenHeight - this.Height));

            // 边界检测和反弹效果
            if (targetX < 0) targetX = 10;
            if (targetY < 0) targetY = 10;
            if (targetX > screenWidth - this.Width) targetX = screenWidth - this.Width - 10;
            if (targetY > screenHeight - this.Height) targetY = screenHeight - this.Height - 10;

            // 创建平滑移动动画（使用配置的持续时间）
            Duration duration = new Duration(TimeSpan.FromSeconds(random.Next(config.RandomMoveDurationMin, config.RandomMoveDurationMax + 1)));

            DoubleAnimation moveX = new DoubleAnimation
            {
                To = targetX,
                Duration = duration,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            DoubleAnimation moveY = new DoubleAnimation
            {
                To = targetY,
                Duration = duration,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            // 动画完成后重置移动状态
            moveX.Completed += (s, e) => { isMoving = false; };

            this.BeginAnimation(Window.LeftProperty, moveX);
            this.BeginAnimation(Window.TopProperty, moveY);
        }

        private void Pet_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                // 左键点击显示后台运行的程序
                ShowRunningProcesses();
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                // 右键显示菜单
                ShowContextMenu();
            }
        }

        private void ShowRunningProcesses()
        {
            try
            {
                // 获取所有正在运行的进程
                var processes = Process.GetProcesses()
                    .Where(p => !string.IsNullOrEmpty(p.MainWindowTitle)) // 只显示有窗口标题的进程
                    .OrderBy(p => p.ProcessName)
                    .Take(20) // 限制显示前20个
                    .ToList();

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("🖥️ 当前运行的程序：");
                sb.AppendLine();

                foreach (var process in processes)
                {
                    try
                    {
                        sb.AppendLine($"📌 {process.ProcessName}");
                        sb.AppendLine($"   窗口: {process.MainWindowTitle}");
                        sb.AppendLine($"   内存: {process.WorkingSet64 / 1024 / 1024} MB");
                        sb.AppendLine();
                    }
                    catch
                    {
                        // 某些进程可能无法访问，跳过
                    }
                }

                if (processes.Count == 0)
                {
                    sb.AppendLine("未找到有窗口的运行程序");
                }

                System.Windows.MessageBox.Show(sb.ToString(), "后台运行程序", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"获取进程信息失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowContextMenu()
        {
            try
            {
                var contextMenu = new System.Windows.Controls.ContextMenu();

                var dragItem = new System.Windows.Controls.MenuItem { Header = "拖动宠物" };
                dragItem.Click += (s, e) => 
                { 
                    // 停止自动移动动画
                    this.BeginAnimation(Window.LeftProperty, null);
                    this.BeginAnimation(Window.TopProperty, null);
                    isMoving = false;
                    
                    // 提示用户可以拖动
                    System.Windows.MessageBox.Show("现在可以拖动宠物了！\n按住左键拖动到想要的位置。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                };

                var processItem = new System.Windows.Controls.MenuItem { Header = "查看运行程序" };
                processItem.Click += (s, e) => { ShowRunningProcesses(); };

                var settingsItem = new System.Windows.Controls.MenuItem { Header = "设置" };
                settingsItem.Click += (s, e) => { ShowSettingsDialog(); };

                var hideItem = new System.Windows.Controls.MenuItem { Header = "隐藏宠物" };
                hideItem.Click += (s, e) => { this.Hide(); };

                var exitItem = new System.Windows.Controls.MenuItem { Header = "退出" };
                exitItem.Click += (s, e) => { System.Windows.Application.Current.Shutdown(); };

                contextMenu.Items.Add(dragItem);
                contextMenu.Items.Add(processItem);
                contextMenu.Items.Add(settingsItem);
                contextMenu.Items.Add(new System.Windows.Controls.Separator());
                contextMenu.Items.Add(hideItem);
                contextMenu.Items.Add(new System.Windows.Controls.Separator());
                contextMenu.Items.Add(exitItem);

                contextMenu.PlacementTarget = PetImage;
                contextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
                contextMenu.IsOpen = true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"菜单显示失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowSettingsDialog()
        {
            try
            {
                var settingsWindow = new Window
                {
                    Title = "YuGi 桌面宠物 - 设置",
                    Width = 450,
                    Height = 500,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    ResizeMode = ResizeMode.NoResize
                };

                var stackPanel = new System.Windows.Controls.StackPanel
                {
                    Margin = new Thickness(20)
                };

                // 帧率设置
                var fpsLabel = new System.Windows.Controls.Label { Content = $"动画帧率 (当前: {1000 / config.AnimationSpeed} FPS)" };
                var fpsSlider = new System.Windows.Controls.Slider
                {
                    Minimum = 50,
                    Maximum = 200,
                    Value = config.AnimationSpeed,
                    TickFrequency = 10,
                    IsSnapToTickEnabled = true
                };
                fpsSlider.ValueChanged += (s, e) =>
                {
                    fpsLabel.Content = $"动画帧率 (当前: {1000 / (int)fpsSlider.Value} FPS)";
                };

                // 初始移动速度
                var initialSpeedLabel = new System.Windows.Controls.Label { Content = $"初始移动时间: {config.InitialMoveDuration} 秒" };
                var initialSpeedSlider = new System.Windows.Controls.Slider
                {
                    Minimum = 5,
                    Maximum = 60,
                    Value = config.InitialMoveDuration,
                    TickFrequency = 1,
                    IsSnapToTickEnabled = true
                };
                initialSpeedSlider.ValueChanged += (s, e) =>
                {
                    initialSpeedLabel.Content = $"初始移动时间: {(int)initialSpeedSlider.Value} 秒";
                };

                // 随机移动速度（最小）
                var randomMinLabel = new System.Windows.Controls.Label { Content = $"随机移动最小时间: {config.RandomMoveDurationMin} 秒" };
                var randomMinSlider = new System.Windows.Controls.Slider
                {
                    Minimum = 1,
                    Maximum = 10,
                    Value = config.RandomMoveDurationMin,
                    TickFrequency = 1,
                    IsSnapToTickEnabled = true
                };
                randomMinSlider.ValueChanged += (s, e) =>
                {
                    randomMinLabel.Content = $"随机移动最小时间: {(int)randomMinSlider.Value} 秒";
                };

                // 随机移动速度（最大）
                var randomMaxLabel = new System.Windows.Controls.Label { Content = $"随机移动最大时间: {config.RandomMoveDurationMax} 秒" };
                var randomMaxSlider = new System.Windows.Controls.Slider
                {
                    Minimum = 1,
                    Maximum = 10,
                    Value = config.RandomMoveDurationMax,
                    TickFrequency = 1,
                    IsSnapToTickEnabled = true
                };
                randomMaxSlider.ValueChanged += (s, e) =>
                {
                    randomMaxLabel.Content = $"随机移动最大时间: {(int)randomMaxSlider.Value} 秒";
                };

                // 按钮面板
                var buttonPanel = new System.Windows.Controls.StackPanel
                {
                    Orientation = System.Windows.Controls.Orientation.Horizontal,
                    Margin = new Thickness(0, 20, 0, 0)
                };
                buttonPanel.SetValue(System.Windows.FrameworkElement.HorizontalAlignmentProperty, System.Windows.HorizontalAlignment.Center);

                var saveButton = new System.Windows.Controls.Button
                {
                    Content = "保存",
                    Width = 80,
                    Height = 30,
                    Margin = new Thickness(5)
                };
                saveButton.Click += (s, e) =>
                {
                    config.AnimationSpeed = (int)fpsSlider.Value;
                    config.InitialMoveDuration = (int)initialSpeedSlider.Value;
                    config.RandomMoveDurationMin = (int)randomMinSlider.Value;
                    config.RandomMoveDurationMax = (int)randomMaxSlider.Value;
                    config.Save();

                    // 更新动画定时器
                    animationTimer.Interval = TimeSpan.FromMilliseconds(config.AnimationSpeed);

                    System.Windows.MessageBox.Show("设置已保存！\n部分设置将在下次移动时生效。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    settingsWindow.Close();
                };

                var cancelButton = new System.Windows.Controls.Button
                {
                    Content = "取消",
                    Width = 80,
                    Height = 30,
                    Margin = new Thickness(5)
                };
                cancelButton.Click += (s, e) => { settingsWindow.Close(); };

                buttonPanel.Children.Add(saveButton);
                buttonPanel.Children.Add(cancelButton);

                // 添加所有控件
                stackPanel.Children.Add(new System.Windows.Controls.Label { Content = "⚙️ 动画设置", FontWeight = FontWeights.Bold, FontSize = 14 });
                stackPanel.Children.Add(fpsLabel);
                stackPanel.Children.Add(fpsSlider);
                stackPanel.Children.Add(new System.Windows.Controls.Label { Content = "提示: 值越小帧率越高，动画越流畅", FontSize = 10, Foreground = System.Windows.Media.Brushes.Gray });

                stackPanel.Children.Add(new System.Windows.Controls.Separator { Margin = new Thickness(0, 15, 0, 15) });

                stackPanel.Children.Add(new System.Windows.Controls.Label { Content = "🚀 移动速度设置", FontWeight = FontWeights.Bold, FontSize = 14 });
                stackPanel.Children.Add(initialSpeedLabel);
                stackPanel.Children.Add(initialSpeedSlider);
                stackPanel.Children.Add(new System.Windows.Controls.Label { Content = "提示: 启动时从左到右移动的时间", FontSize = 10, Foreground = System.Windows.Media.Brushes.Gray });

                stackPanel.Children.Add(new System.Windows.Controls.Separator { Margin = new Thickness(0, 15, 0, 15) });

                stackPanel.Children.Add(new System.Windows.Controls.Label { Content = "🎲 随机移动时间范围", FontWeight = FontWeights.Bold, FontSize = 14 });
                stackPanel.Children.Add(randomMinLabel);
                stackPanel.Children.Add(randomMinSlider);
                stackPanel.Children.Add(randomMaxLabel);
                stackPanel.Children.Add(randomMaxSlider);
                stackPanel.Children.Add(new System.Windows.Controls.Label { Content = "提示: 每次随机移动的持续时间范围", FontSize = 10, Foreground = System.Windows.Media.Brushes.Gray });

                stackPanel.Children.Add(buttonPanel);

                settingsWindow.Content = stackPanel;
                settingsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"设置对话框显示失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // 保存窗口位置
            config.WindowLeft = this.Left;
            config.WindowTop = this.Top;
            config.Save();
            
            // 清理托盘图标
            if (trayIcon != null)
            {
                trayIcon.Visible = false;
                trayIcon.Dispose();
            }
            base.OnClosing(e);
        }
    }
}
