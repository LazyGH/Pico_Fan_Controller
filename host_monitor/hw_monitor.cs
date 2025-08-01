using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LibreHardwareMonitor.Hardware;
 
public class HardwareMonitor
{
    // 私有静态变量，用于存储单例实例
    private static HardwareMonitor _instance;
 
    // 锁对象，用于线程安全
    private static readonly object _lock = new object();
 
    // 私有构造函数，防止外部实例化
    private HardwareMonitor()
    {
        // 创建 Computer 对象，用于管理硬件信息
        computer = new Computer
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
    }
 
    // 公共静态方法，返回唯一的实例
    public static HardwareMonitor Instance
    {
        get
        {
            // 确保线程安全
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new HardwareMonitor();
                }
                return _instance;
            }
        }
    }
 
    private Computer computer; // Computer 对象，用于管理硬件信息
    public Computer GetComputer() { return computer; }
 
    private float m_cpu_usage = -1; // CPU利用率
    private float m_cpu_freq = -1; // CPU频率
    private float m_cpu_temperature = -1; // CPU温度
    private float m_memory_usage = -1; // 内存利用率
    private float m_gpu_temperature = -1; // GPU温度
    private float m_hdd_temperature = -1; // 硬盘温度（所有硬盘中最高温度）
    private float m_hdd_usage = -1; // 硬盘利用率（所有硬盘中占用率最高的）
    private float m_main_board_temperature = -1; // 主板温度
    private float m_gpu_usage = -1; // GPU利用率
    private float m_fan_speed = -1; // 风扇转速（所有风扇中转速最大一个）
    private float m_out_speed = -1; // 上传速率
    private float m_in_speed = -1; // 下载速率
 
    private Dictionary<string, float> m_all_hdd_temperature = new(); // 所有硬盘温度
    private Dictionary<string, float> m_all_cpu_temperature = new(); // 所有 CPU 核心温度
    private Dictionary<string, float> m_all_cpu_clock = new(); // 所有 CPU 核心频率
    private Dictionary<string, float> m_all_hdd_usage = new(); // 所有硬盘利用率
    private Dictionary<string, float> m_all_fan_speed = new(); // 所有风扇转速
 
    // 重置所有值
    public void ResetAllValues()
    {
        m_cpu_usage = -1;
        m_cpu_freq = -1;
        m_cpu_temperature = -1;
        m_memory_usage = -1;
        m_gpu_temperature = -1;
        m_hdd_temperature = -1;
        m_main_board_temperature = -1;
        m_gpu_usage = -1;
        m_hdd_usage = -1;
        m_fan_speed = -1;
        m_out_speed = -1;
        m_in_speed = -1;
 
        m_all_hdd_temperature.Clear();
        m_all_hdd_usage.Clear();
        m_all_fan_speed.Clear();
    }
 
    // 数据大小格式化显示
    public string DataSizeToString(float size, bool withSpace)
    {
        string formattedSize;
        if (size < 1024 * 10)
            formattedSize = $"{size / 1024.0f:F2} KB";
        else if (size < 1024 * 1024)
            formattedSize = $"{size / 1024.0f:F1} KB";
        else if (size < 1024 * 1024 * 1024)
            formattedSize = $"{size / 1024.0f / 1024.0f:F2} MB";
        else if (size < 1024L * 1024 * 1024 * 1024)
            formattedSize = $"{size / 1024.0f / 1024.0f / 1024.0f:F2} GB";
        else
            formattedSize = $"{size / 1024.0f / 1024.0f / 1024.0f / 1024.0f:F2} TB";
 
        return withSpace ? formattedSize : formattedSize.Replace(" ", "");
    }
 
    // 输出所有值
    public void PrintAllValues()
    {
        DateTime now = DateTime.Now;
        Console.WriteLine($"------  {now:HH:mm:ss}  ------");
        Console.WriteLine($"CPU负载: {m_cpu_usage} %");
        Console.WriteLine($"CPU频率: {m_cpu_freq} GHz");
        Console.WriteLine($"CPU温度: {m_cpu_temperature} ℃");
        Console.WriteLine($"内存负载: {m_memory_usage} %");
        Console.WriteLine($"GPU温度: {m_gpu_temperature} ℃");
        Console.WriteLine($"GPU负载: {m_gpu_usage} %");
        Console.WriteLine($"主板温度: {m_main_board_temperature} ℃");
        Console.WriteLine($"硬盘最高温度: {m_hdd_temperature} ℃");
        foreach (var pair in m_all_hdd_temperature)
            Console.WriteLine($"   硬盘【{pair.Key}】温度: {pair.Value} ℃");
        Console.WriteLine($"硬盘最高使用率: {m_hdd_usage} %");
        foreach (var pair in m_all_hdd_usage)
            Console.WriteLine($"   硬盘【{pair.Key}】使用率: {pair.Value} %");
        Console.WriteLine($"风扇最大转速: {m_fan_speed} 转/分");
 
        foreach (var pair in m_all_fan_speed)
            Console.WriteLine($"   风扇【{pair.Key}】转速: {pair.Value} 转/分");
        Console.WriteLine($"上传速率: {DataSizeToString(m_out_speed, true)}/s");
        Console.WriteLine($"下载速率: {DataSizeToString(m_in_speed, true)}/s");
        Console.WriteLine();
    }
 
    // 将值插入到字典中，如果键已存在，则在末尾添加" #1"，如果已经存在" #1"，则将其加1
    public void InsertValueToDictionary(Dictionary<string, float> valueMap, string key, float value)
    {
        if (valueMap.ContainsKey(key))
        {
            string existingKey = key;
            int index = existingKey.LastIndexOf('#');
            if (index != -1 && int.TryParse(existingKey[(index + 1)..], out int num))
            {
                existingKey = existingKey[..index] + $"#{++num}";
            }
            else
            {
                existingKey += " #1";
            }
            valueMap[existingKey] = value;
        }
        else
        {
            valueMap[key] = value;
        }
    }
 
    // 将 C# 的 String 类型转换为 C# 的标准 wstring 类型
    public string ClrStringToStdWstring(string str)
    {
        return str ?? string.Empty;
    }
 
    // 计算 CPU 利用率
    public bool GetCpuUsage(IHardware hardware, ref float cpuUsage)
    {
        foreach (var sensor in hardware.Sensors)
        {
            // 找到负载传感器
            if (sensor.SensorType == SensorType.Load)
            {
                // 检查传感器名称是否为 "CPU Total"
                if (sensor.Name == "CPU Total")
                {
                    cpuUsage = (float)(sensor.Value ?? -1);
                    return true;
                }
            }
        }
        return false;
    }
 
    // 计算 CPU 温度（所有核心温度的平均值）
    public bool GetCpuTemperature(IHardware hardware, ref float temperature)
    {
        var allCpuTemperatures = new Dictionary<string, float>();
 
        foreach (var sensor in hardware.Sensors)
        {
            // 找到温度传感器
            if (sensor.SensorType == SensorType.Temperature)
            {
                string name = sensor.Name;
                // 保存每个 CPU 传感器的温度
                if (sensor.Value.HasValue)
                {
                    allCpuTemperatures[ClrStringToStdWstring(name)] = (float)sensor.Value.Value;
                }
            }
        }
 
        // 计算平均温度
        if (allCpuTemperatures.Count > 0)
        {
            float sum = 0;
            foreach (var temp in allCpuTemperatures.Values)
            {
                sum += temp;
            }
            temperature = sum / allCpuTemperatures.Count;
        }
 
        return temperature > 0;
    }
 
    // 计算 CPU 频率（所有核心时钟频率的平均值）
    public bool GetCPUFreq(IHardware hardware, ref float freq)
    {
        freq = 0;
        m_all_cpu_clock.Clear();
 
        foreach (var sensor in hardware.Sensors)
        {
            if (sensor.SensorType == SensorType.Clock)
            {
                string name = sensor.Name;
                if (name != "Bus Speed")
                {
                    if (sensor.Value.HasValue)
                    {
                        m_all_cpu_clock[name] = (float)sensor.Value;
                    }
                }
            }
        }
 
        if (m_all_cpu_clock.Count > 0)
        {
            float sum = 0;
            foreach (var clock in m_all_cpu_clock.Values)
            {
                sum += clock;
            }
            freq = sum / m_all_cpu_clock.Count / 1000.0f; // 转换为 GHz
        }
        return true;
    }
 
    // 计算内存利用率
    public bool GetMemoryUsage(IHardware hardware, ref float memoryUsage)
    {
        foreach (var sensor in hardware.Sensors)
        {
            if (sensor.SensorType == SensorType.Load && sensor.Name == "Memory")
            {
                if (sensor.Value.HasValue)
                {
                    memoryUsage = (float)sensor.Value;
                    return true;
                }
            }
        }
 
        return false;
    }
 
    // 计算硬件温度（GPU、主板、硬盘）
    public bool GetHardwareTemperature(IHardware hardware, ref float temperature)
    {
        var allTemperatures = new List<float>();
        float coreTemperature = -1;
        string temperatureName = null;
 
        switch (hardware.HardwareType)
        {
            case HardwareType.Cpu:
                temperatureName = "Core Average";
                break;
            case HardwareType.GpuNvidia:
            case HardwareType.GpuAmd:
            case HardwareType.GpuIntel:
                temperatureName = "GPU Core";
                break;
            default:
                break;
        }
 
        foreach (var sensor in hardware.Sensors)
        {
            if (sensor.SensorType == SensorType.Temperature)
            {
                if (sensor.Value.HasValue)
                {
                    float currentTemperature = (float)sensor.Value;
                    allTemperatures.Add(currentTemperature);
 
                    if (sensor.Name == temperatureName)
                    {
                        coreTemperature = currentTemperature;
                    }
                }
            }
        }
 
        if (coreTemperature >= 0)
        {
            temperature = coreTemperature;
            return true;
        }
 
        if (allTemperatures.Count > 0)
        {
            float sum = 0;
            foreach (var temp in allTemperatures)
            {
                sum += temp;
            }
            temperature = sum / allTemperatures.Count;
            return true;
        }
 
        // 如果没有找到温度传感器，在子硬件中寻找
        foreach (var subHardware in hardware.SubHardware)
        {
            if (GetHardwareTemperature(subHardware, ref temperature))
            {
                return true;
            }
        }
 
        return false;
    }
 
    // 计算 GPU 利用率
    public bool GetGpuUsage(IHardware hardware, ref float gpuUsage)
    {
        foreach (var sensor in hardware.Sensors)
        {
            if (sensor.SensorType == SensorType.Load && sensor.Name == "GPU Core")
            {
                float currentGpuUsage = Convert.ToSingle(sensor.Value);
                if (gpuUsage < currentGpuUsage)
                {
                    gpuUsage = currentGpuUsage;
                }
            }
        }
        return gpuUsage > 0;
    }
 
    // 计算硬盘利用率
    public bool GetHddUsage(IHardware hardware, ref float hddUsage)
    {
        foreach (var sensor in hardware.Sensors)
        {
            if (sensor.SensorType == SensorType.Load && sensor.Name == "Used Space")
            {
                hddUsage = Convert.ToSingle(sensor.Value);
                return true;
            }
        }
        return false;
    }
 
    // 计算风扇转速
    public bool GetFanSpeed(IHardware hardware, ref float fanSpeed)
    {
        m_all_fan_speed.Clear();
 
        foreach (var sensor in hardware.Sensors)
        {
            if (sensor.SensorType == SensorType.Fan)
            {
                string name = sensor.Name;
                float speed = Convert.ToSingle(sensor.Value);
                m_all_fan_speed[name] = speed;
 
                if (speed > fanSpeed)
                {
                    fanSpeed = speed;
                }
            }
        }
        return fanSpeed > 0;
    }
 
    // 计算网络上下行速率
    public bool GetNetworkSpeed(IHardware hardware, ref float outSpeed, ref float inSpeed)
    {
        bool flag = false;
 
        foreach (var sensor in hardware.Sensors)
        {
            if (sensor.SensorType == SensorType.Throughput)
            {
                if (sensor.Name == "Upload Speed")
                {
                    float speed = Convert.ToSingle(sensor.Value);
                    if (outSpeed < speed)
                    {
                        outSpeed = speed;
                    }
                    flag = true;
                }
                else if (sensor.Name == "Download Speed")
                {
                    float speed = Convert.ToSingle(sensor.Value);
                    if (inSpeed < speed)
                    {
                        inSpeed = speed;
                    }
                    flag = true;
                }
            }
        }
        return flag;
    }
 
    // 函数：GetHardwareInfo
    // 描述：递归地获取硬件信息并打印到控制台
    // 参数：IHardware hardware - 当前硬件对象
    // 返回值：int - 成功返回0，失败返回-1
    public int GetHardwareInfo(IHardware hardware)
    {
        try
        {
            hardware.Update(); // 更新硬件数据
 
            // 根据硬件类型执行不同的操作
            switch (hardware.HardwareType)
            {
                case HardwareType.Cpu: // CPU 信息
                    {
                        // 获取 CPU 利用率
                        if (m_cpu_usage < 0)
                            GetCpuUsage(hardware, ref m_cpu_usage);
 
                        // 获取 CPU 温度
                        if (m_cpu_temperature < 0)
                            GetCpuTemperature(hardware, ref m_cpu_temperature);
 
                        // 获取 CPU 频率
                        if (m_cpu_freq < 0)
                            GetCPUFreq(hardware, ref m_cpu_freq);
                        break;
                    }
 
                case HardwareType.GpuIntel: // Intel GPU 信息
                case HardwareType.GpuNvidia: // NVIDIA GPU 信息
                case HardwareType.GpuAmd: // AMD GPU 信息
                    {
                        // 获取 GPU 温度
                        if (m_gpu_temperature < 0)
                            GetHardwareTemperature(hardware, ref m_gpu_temperature);
 
                        // 获取 GPU 利用率
                        if (m_gpu_usage < 0)
                            GetGpuUsage(hardware, ref m_gpu_usage);
                        break;
                    }
 
                case HardwareType.Memory: // 内存信息
                    {
                        // 获取内存利用率
                        if (m_memory_usage < 0)
                            GetMemoryUsage(hardware, ref m_memory_usage);
                        break;
                    }
 
                case HardwareType.Motherboard: // 主板信息
                    {
                        // 获取主板温度
                        if (m_main_board_temperature < 0)
                            GetHardwareTemperature(hardware, ref m_main_board_temperature);
                        break;
                    }
 
                case HardwareType.Storage: // 存储设备信息
                    {
                        // 获取硬盘温度
                        float curHddTemperature = -1;
                        GetHardwareTemperature(hardware, ref curHddTemperature);
                        InsertValueToDictionary(m_all_hdd_temperature,
                            ClrStringToStdWstring(hardware.Name), curHddTemperature);
                        if (m_hdd_temperature < curHddTemperature)
                            m_hdd_temperature = curHddTemperature;
 
                        // 获取硬盘利用率
                        float curHddUsage = -1;
                        GetHddUsage(hardware, ref curHddUsage);
                        InsertValueToDictionary(m_all_hdd_usage,
                            ClrStringToStdWstring(hardware.Name), curHddUsage);
                        if (m_hdd_usage < curHddUsage)
                            m_hdd_usage = curHddUsage;
                        break;
                    }
 
                case HardwareType.Network: // 网络设备信息
                    {
                        // 获取网络速率
                        GetNetworkSpeed(hardware, ref m_out_speed, ref m_in_speed);
                        break;
                    }
 
                case HardwareType.SuperIO: // SuperIO 信息
                    {
                        // 计算风扇转速
                        if (m_fan_speed < 0)
                            GetFanSpeed(hardware, ref m_fan_speed);
                        break;
                    }
 
                default: // 未知硬件类型
                    {
                        // 未知硬件类型
                        break;
                    }
            }
 
            // 如果硬件有子硬件，递归调用该函数处理子硬件信息
            foreach (var subHardware in hardware.SubHardware)
            {
                GetHardwareInfo(subHardware);
            }
        }
        catch (Exception ex)
        {
            // 捕获并打印异常信息
            Console.WriteLine("Error: " + ex.Message);
            return -1;
        }
 
        return 0; // 操作成功
    }
}