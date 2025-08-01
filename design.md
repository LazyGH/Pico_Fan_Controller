### **Framework for a Raspberry Pi Pico-Based Computer Fan Controller**

This document outlines the design framework for a computer fan controller using a Raspberry Pi Pico. The system contains 2 parts: Host windows program part and Pico porgram part. The Pico will communicate with the host Windows PC via USB to monitor hardware temperatures, enabling dynamic fan speed adjustment based on a user-defined temperature-to-fan-speed curve.

#### **System Overview**

The solution consists of two main components:

1.  **Raspberry Pi Pico Firmware:** A C++ program running on the Pico responsible for receiving temperature data and configurations, storing and interpreting the fan control curve, and generating PWM signals to control the fans.
2.  **Host PC Application:** It contains 2 parts C# programs. One is a lightweight application running in the background on a Windows PC. It will read hardware temperatures, send them to the Pico. Antother is a command-line program used for read and update application configurations. 

The fans are to be powered by the motherboard or a dedicated fan hub, with the Pico only providing the PWM control signal.

---

### **I. Raspberry Pi Pico Firmware (C++)**

The Pico firmware will be developed using the Raspberry Pi Pico C/C++ SDK.

#### **1. Core Functionality**


*   **Control Loop:** The main loop of the program will continuously listen for temperature updates from the host PC. Upon receiving new temperature data, it will calculate the appropriate fan speed based on the stored curve and update the PWM duty cycles accordingly. Pico will also listen states reading or configuration command from host. 
*   **Fan Speed Control Strategy:** Temperature-to-fan-speed-percentage curves will be stored in the Pico's onboard flash memory, making them persistent across power cycles. There can be multiple curves, cursponding to different hardware (CPU GPU etc.) temperature data. One curve can control multiple fans, and one fan also can be controlled by multiple curves. Moreover, the fan speed change should respond quickly to the temperature raising, and delay for the decreasing. In the first version implementation, 2 curves should be set, for the CPU and GPU. 3 PWM outputs should be set, 2 for CPU and 1 for GPU. And more curves and PWM outputs can be added easily latter.
*   **PWM Generation:** The firmware will configure multiple GPIO pins as PWM outputs to control the fans. Each fan will be controlled by a dedicated PWM channel. The frequency shoud be 25k Hz to fit general PC case fans.
*   **USB Communication:** The Pico will act as a USB CDC (Communication Device Class) device, creating a virtual serial port (COM) over the USB connection. This allows for straightforward serial communication with the host PC. 
*   **Connection Check** At start up all fans should be set in the safety speed. If for a certain time Pico doesn't receive any data (little longer than the minimum sending data interval of the host), then it will send a connection check signal requesting temperature data to the host. If there is no response with timeout, it will enter disconnected state: Control all the fans linearly into the safety speed (say 30%. If last speed higher than safety speed, just keep), and try to reconnect with host at certain intervals. The host may also send connection check signal to Pico, Pico need response. 
*   **States Response:** Some states and configurations can be read by the host configration program. The details can reference host setting program part. ~~They basically include fan states (ID, Name, Corresponding temperature sensor name, current sensor temperature, current fan speed presentage), fan speed curve, heartbeat signal interval, led switch state~~
*   **Configure:** Some configurations should be stored in the flash memory, which can be used for the future power on/off cycle. It also can be updated or readed by the host configration program. The details can reference host setting program part.
*   **States LED:** There is a RGB LED ws2812, whose DIN is connected with Pico GP16. Normally it will keep off. Red always on means program stucks. Yellow always on means disconnected with host. Green quickly flash once means received temperature data. Blue flash once means connection check or handshake. Purple flash once means host read states or update configuration of the host successfully.
*   **Log:** ~~WHAT SHOUD LOG CONTENT BE?~~ It will record pico runing state and events. Last 3 times power on running log will be kept.
*   **Others** 



### **II. Host PC Application (Windows C#)**

The host-side application will contain a temperature data sender program designed to run in the background with minimal resource usage, and a console-based program used to read and update configeration and query host and pico application states.

#### **1. Core Functionality**

##### a. Background Data Sender Application

*   **Hardware Temperature Monitoring:** The background application will looply read host hardware temperatures at certain intervals. these should include Intel CPU package temperature and Nvidia GPU temperature. These can be achieved using third-party libraries `LibreHardwareMonitorLib`.
*   **Connection Check:** At start up, three-way handshake connection check should processed between the host and the Pico. If connection failed, it will notify the user and try to reconnection at certain intervals with limited time, and then wait for mannually connect if still can't connect. And periodically host will check the connection between the Pico with two-way handshake.
*   **Serial Communication:** The application will communicate with the Pico over the virtual COM port by USB connection. There may be multiple COM ports on the Windows, the application should automatically find which one is the Pico (when the Pico is connected to the host). Parameters: 115200 Baud Rate, 8 data bits, no parity, and one stop bit, DtrEnable Ture, RtsEnable True. 
*   **Data Filtraion and Sending:** Normally the host application will send readed temperature data to the Pico continuously at certain intervals. The readed temperature data may have fluctuations due to the noise or sensor unsatability. The host should filter these fluctuations (don't send these data), but still should be sensitive to continuous or significant temperature rising. If the temperature always keeps in a same level, Host doesn't need to send data to save communication overhead. But there is still a minimum sending interval (little shorter than the minimum connection checking interval of the Pico).  
*   **Overheat Alert:** When CPU/GPU continuously exceeds a certain temperature, alert user, giving hardware name and tempurature. For example, CPU exceeds 80℃ for 10s, GPU exceeds 60℃ for 10s. Every overheat round it only alert once, and there is an minimum interval between 2 overheat alerts, say, 30min.
*   **Control Nvidia GPU Fans:** Detect the Nvidia GPU device and use `nvapi` to control GPU fans based on readed GPU temperature and use a temperature-fan-speed curve, which can be set by user. This function has no communication with Pico.  
*   **Windows Notification:** The application can send Windows Toast Notifications in following conditions: 
1. Start up successfully.
2. Connect successfully, Can't connect or Disconnected with Pico.
3. CPU/GPU Overheat Alert.
*   **Log:** ~~WHAT SHOUD LOG CONTENT BE?~~ Record app actions and timestemps in the log files. Create and write a new log for each run. Only keep last 5 logs. 



##### b. Command-Line Configuration Application

*   **Command-Line Interface:** The application will be configurable via command-line arguments to set both host and pico application parameters.
*   **Background Operation:** The application can be configured to run as a background process. For true background operation, creating a Windows Service is the most robust method. A simpler approach is to run a console application without a visible window.
*   **Configuration File** 




#### **3. Communication Protocol (Pico <-> PC)**

A simple, text-based command protocol will be used over the virtual serial port.

*   **PC to Pico:**
    *   `TEMP:<cpu_temp>,<gpu_temp>`: Sends the latest CPU and GPU temperatures.
    *   `SET_CURVE:<sensor_type>,<temp1>,<speed1>,<temp2>,<speed2>,...`: Sets a new fan curve for the specified sensor (`CPU`, `GPU`, etc.).
    *   `GET_CURVE`: Requests the current fan curve from the Pico.

*   **Pico to PC:**
    *   `OK`: Acknowledges a successful command.
    *   `ERROR`: Indicates an error in the received command.
    *   `CURVE:<sensor_type>,<temp1>,<speed1>,...`: Sends the currently stored fan curve.

#### **4. Tunable Parameters (Pico)**

*   **PWM Frequency:** Can be adjusted to match the specifications of the connected fans.
*   **Fan Curve Points:** The number and values of temperature/speed points in the curve can be customized.
*   **Update Interval:** The rate at which the Pico expects temperature updates from the host.

---


#### **4. Tunable Parameters (Host PC)**

*   **COM Port:** The virtual serial port of the Raspberry Pi Pico.
*   **Polling Interval:** The frequency at which hardware temperatures are read and sent to the Pico.
*   **Fan Curve:** The temperature/fan speed points can be specified via command-line arguments.
*   **Sensor Selection:** The specific hardware sensors to monitor can be configured.

---

### **Implementation Notes**

*   **Pico Flash Wear:** The fan curve should not be updated excessively to avoid wear on the Pico's flash memory. Updates should only happen when the user explicitly changes the configuration. The flash memory can endure approximately 100,000 write cycles per sector.
*   **Error Handling:** Both the Pico firmware and the host application should implement robust error handling for communication failures, invalid commands, and hardware read errors.
*   **Resource Usage:** Both applications should be designed to be lightweight. The Pico has limited resources, and the host application should not impact system performance while running in the background. On the host, using a polling mechanism with a reasonable delay (e.g., 1-2 seconds) is sufficient and will keep CPU usage low. The Pico's main loop should be event-driven or have appropriate sleep intervals to minimize power consumption.