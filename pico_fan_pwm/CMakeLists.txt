# Generated Cmake Pico project file

cmake_minimum_required(VERSION 3.13)

set(CMAKE_C_STANDARD 11)
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

# Initialise pico_sdk from installed location
# (note this can come from environment, CMake cache etc)

# == DO NOT EDIT THE FOLLOWING LINES for the Raspberry Pi Pico VS Code Extension to work ==
if(WIN32)
    set(USERHOME $ENV{USERPROFILE})
else()
    set(USERHOME $ENV{HOME})
endif()
set(sdkVersion 2.1.1)
set(toolchainVersion 14_2_Rel1)
set(picotoolVersion 2.1.1)
set(picoVscode ${USERHOME}/.pico-sdk/cmake/pico-vscode.cmake)
if (EXISTS ${picoVscode})
    include(${picoVscode})
endif()
# ====================================================================================
set(PICO_BOARD waveshare_rp2040_zero CACHE STRING "Board type")

# Pull in Raspberry Pi Pico SDK (must be before project) 
include(pico_sdk_import.cmake)

project(ws2812 C CXX ASM)

# Initialise the Raspberry Pi Pico SDK
pico_sdk_init()

# Add executable. Default name is the project name, version 0.1

# add_executable(Fan_PWM Fan_PWM.cpp )
add_executable(ws2812 ws2812.cpp)

pico_set_program_name(ws2812 "ws2812")
pico_set_program_version(ws2812 "0.1")

# Generate PIO header
# pico_generate_pio_header(blink ${CMAKE_CURRENT_LIST_DIR}/blink.pio)
pico_generate_pio_header(ws2812 ${CMAKE_CURRENT_LIST_DIR}/ws2812.pio)

# Modify the below lines to enable/disable output over UART/USB
# pico_enable_stdio_uart(Fan_PWM 0)
# pico_enable_stdio_usb(Fan_PWM 1)
pico_enable_stdio_uart(ws2812 1)
pico_enable_stdio_usb(ws2812 1)

# Add the standard library to the build
# target_link_libraries(Fan_PWM
#         pico_stdlib)
target_link_libraries(ws2812 pico_stdlib)

# create map/bin/hex file etc.
pico_add_extra_outputs(ws2812)

# add url via pico_set_program_url
# example_auto_set_url(blink)

# Add the standard include files to the build
# target_include_directories(Fan_PWM PRIVATE
#         ${CMAKE_CURRENT_LIST_DIR}
# )

# Add any user requested libraries
# target_link_libraries(Fan_PWM 
#         hardware_dma
#         hardware_pio
#         hardware_watchdog
#         hardware_clocks
#         )

target_link_libraries(ws2812
        # hardware_dma
        hardware_pio
        # hardware_watchdog
        hardware_clocks
        hardware_pwm
        hardware_irq
        )

pico_add_extra_outputs(ws2812)

