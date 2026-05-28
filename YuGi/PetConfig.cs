using System;
using System.IO;
using System.Text.Json;

namespace DesktopPet
{
    public class PetConfig
    {
        public double WindowLeft { get; set; }
        public double WindowTop { get; set; }
        public int AnimationSpeed { get; set; } = 100; // 默认 10 FPS，适合14帧动画
        public bool AutoMove { get; set; } = true;
        public int MoveIntervalMin { get; set; } = 3;
        public int MoveIntervalMax { get; set; } = 7;
        public int InitialMoveDuration { get; set; } = 18; // 初始移动持续时间（秒）
        public int RandomMoveDurationMin { get; set; } = 1; // 随机移动最小持续时间（秒）
        public int RandomMoveDurationMax { get; set; } = 3; // 随机移动最大持续时间（秒）
        
        // 设置窗口大小
        public double SettingsWindowWidth { get; set; } = 450; // 设置窗口宽度
        public double SettingsWindowHeight { get; set; } = 500; // 设置窗口高度
        
        // 开机自启动
        public bool AutoStart { get; set; } = false; // 是否开机自启动

        private static readonly string ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DesktopPet",
            "config.json"
        );

        public static PetConfig Load()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    string json = File.ReadAllText(ConfigPath);
                    return JsonSerializer.Deserialize<PetConfig>(json) ?? new PetConfig();
                }
            }
            catch (Exception)
            {
                // 如果加载失败，返回默认配置
            }
            return new PetConfig();
        }

        public void Save()
        {
            try
            {
                string directory = Path.GetDirectoryName(ConfigPath)!;
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                File.WriteAllText(ConfigPath, json);
            }
            catch (Exception)
            {
                // 静默失败，不影响程序运行
            }
        }
    }
}
