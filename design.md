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
*   **Fan Speed Curve:** A temperature-to-fan-speed-percentage curve will be stored in the Pico's onboard flash memory, making it persistent across power cycles. There can be multiple curves, cursponding to different hardware (CPU GPU etc.) temperature data. One curve can control multiple fans, and one fan also can be controlled by multiple curves. The fan speed change should respond quickly to the temperature raising, and delay to the decreasing. The `hardware/flash.h` library allows for erasing and programming the flash memory.
*   **PWM Generation:** The firmware will configure multiple GPIO pins as PWM outputs to control the fans. The Pico's `hardware/pwm.h` library provides functions to set the frequency and duty cycle of the PWM signals. Each fan will be controlled by a dedicated PWM channel. 
*   **USB Communication:** The Pico will act as a USB CDC (Communication Device Class) device, creating a virtual serial port over the USB connection. This allows for straightforward serial communication with the host PC. 
*   **Connection Check** At start up all fans should be set in safety speed. If for a certain time Pico doesn't receive any data, then it will send a connection check signal requesting temperature data to the host. If there is no response with time out, it will enter disconnected state: Control all the fans linearly into the safety speed (say 30%. If last speed higher than safety speed, just keep), and try to reconnect with host in every certain seconds (requesting temperature data).
*   **States Response:** Some states and configurations can be read by the host configration program. The details can reference host setting program part. ~~They basically include fan states (ID, Name, Corresponding temperature sensor name, current sensor temperature, current fan speed presentage), fan speed curve, heartbeat signal interval, led switch state~~
*   **Configure:** Some configurations should be stored in the flash memory, can be update by the host configration program. The details can reference host setting program part.
*   **States LED:** There is a RGB LED ws2812, whose DIN is connected with Pico GP16. Red always on means disconnected with host program; Green blink (quickly) once means received temperature data or state reading request from the host (has minimum blink interval); Blue blink (little longer) means update configuration successfully. In other time it will keep off. It shoud be dim light. 
*   **Others** In the first version implementation, 2 curve should be set, for CPU and GPU. 3 PWM outputs should be set, 2 for CPU and 1 for GPU. They can be added easily latter.



### **II. Host PC Application (Windows C#)**

The host-side application will contains a temperature data sender program designed to run in the background with minimal resource usage, and a console-based program used to modify and update configeration and query host and pico application status.

#### **1. Core Functionality**

*   **Hardware Monitoring:** The background application will read host hardware temperatures, including Intel CPU package, Nvidia GPU and RAM. This can be achieved using a third-party library to abstract the hardware-specific details. `hwinfo` is a good cross-platform C# option. Alternatively, `OpenHardwareMonitorLib` can be used, although it may require C++/CLI for interoperability with C#.
*   **Serial Communication:** The program will communicate with the Pico over the virtual COM port. Standard Windows file I/O on the COM port or a library like Boost.Asio can be used.
*   **Command-Line Interface:** The application will be configurable via command-line arguments to set parameters without a GUI. A lightweight parsing library like `args++`, `TCLAP`, or `cargs` is recommended.
*   **Background Operation:** The program can be configured to run as a background process. For true background operation, creating a Windows Service is the most robust method. A simpler approach is to run a console application without a visible window.




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