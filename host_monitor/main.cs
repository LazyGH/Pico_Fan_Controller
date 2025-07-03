using System;
using System.Threading;
 
class Program
{
    static void tMain()
    {
        // 创建硬件监控实例
        HardwareMonitor hardwareMonitor = HardwareMonitor.Instance;
 
        hardwareMonitor.GetComputer().Open(); // 打开硬件监控

        int updateInterval = 5000; // 设置更新间隔为 5 秒
 
        while (true)
        {
            // 重置所有值
            hardwareMonitor.ResetAllValues();
            // 遍历所有硬件并获取信息
            foreach (var hardware in hardwareMonitor.GetComputer().Hardware)
            {
                hardwareMonitor.GetHardwareInfo(hardware);
            }
            // 输出所有值
            hardwareMonitor.PrintAllValues();
            // 延迟 5 秒后继续监控
            Thread.Sleep(updateInterval);
        }
        // 关闭硬件监控
        hardwareMonitor.GetComputer().Close();
    }
}