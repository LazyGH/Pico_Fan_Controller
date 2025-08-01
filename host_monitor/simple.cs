using System;
using System.Threading;
using LibreHardwareMonitor.Hardware;
 
class TestProgram
{
    // 函数：GetHardwareInfo
    // 描述：递归获取硬件信息并打印到控制台
    // 参数：IHardware hardware - 当前硬件对象
    // 返回值：void
    static void GetHardwareInfo(IHardware hardware)
    {
        try
        {
            hardware.Update(); // 更新硬件数据
 
            // 根据硬件类型执行不同的操作
            switch (hardware.HardwareType)
            {
                case HardwareType.Cpu:
                    Console.WriteLine("CPU Information:");
                    break;
 
                case HardwareType.GpuIntel:
                case HardwareType.GpuNvidia:
                case HardwareType.GpuAmd:
                    Console.WriteLine("GPU Information:");
                    break;
 
                case HardwareType.Memory:
                    Console.WriteLine("Memory Information:");
                    break;
 
                case HardwareType.Motherboard:
                    Console.WriteLine("Motherboard Information:");
                    break;
 
                case HardwareType.Storage:
                    Console.WriteLine("Storage Information:");
                    break;
 
                case HardwareType.Network:
                    Console.WriteLine("Network Information:");
                    break;
 
                case HardwareType.SuperIO:
                    Console.WriteLine("SuperIO Information:");
                    break;
 
                default:
                    Console.WriteLine("Unknown Hardware Type.");
                    break;
            }
 
            // 遍历所有传感器，获取并打印传感器名称和值
            foreach (var sensor in hardware.Sensors)
            {
                string name = sensor.Name;
                float? value = sensor.Value;
 
                if (value.HasValue)
                {
                    Console.WriteLine($"  {name}: {value.Value}");
                }
            }
 
            // 递归处理子硬件信息
            foreach (var subHardware in hardware.SubHardware)
            {
                GetHardwareInfo(subHardware);
            }
        }
        catch (Exception ex)
        {
            // 捕获并打印异常信息
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
 
    static void tMain() // 如需运行，改为Main()
    {
        // 创建 Computer 对象，用于管理硬件信息
        Computer computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
            IsMotherboardEnabled = true,
            IsControllerEnabled = true,
            IsNetworkEnabled = true,
            IsStorageEnabled = true,
            IsBatteryEnabled = true
        };
 
        computer.Open(); // 打开硬件监控

        while (true)
        {
            // 遍历所有硬件并获取信息
            DateTime now = DateTime.Now;
            Console.WriteLine($"------  {now:HH:mm:ss}  ------");
            foreach (var hardware in computer.Hardware)
            {
                GetHardwareInfo(hardware);
            }
            // 延迟 5 秒后继续监控
            Thread.Sleep(5000);
            
        }
    }
}
