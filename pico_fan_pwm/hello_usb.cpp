/**
 * Copyright (c) 2020 Raspberry Pi (Trading) Ltd.
 *
 * SPDX-License-Identifier: BSD-3-Clause
 */

// #include <stdio.h>
// #include "pico/stdlib.h"

// int main() {
//     stdio_init_all();
//     while (true) {
//         printf("Hello, world!\n");
//         sleep_ms(500);
//     }
// }



#include <stdio.h>
#include "pico/stdlib.h"
#include <string.h> // Required for strlen()

// Define a buffer to store the incoming message
#define BUFFER_SIZE 256
char buffer[BUFFER_SIZE];

/**
 * @brief Reverses a C-style string in place.
 * 
 * @param str The null-terminated string to reverse.
 */
void reverse_string(char *str) {
    if (!str) {
        return; // Handle null pointer
    }
    int start = 0;
    int end = strlen(str) - 1;
    char temp;

    while (start < end) {
        // Swap characters
        temp = str[start];
        str[start] = str[end];
        str[end] = temp;

        // Move towards the middle
        start++;
        end--;
    }
}

int main() {
    stdio_init_all();

    // Print a welcome message to the serial terminal
    printf("Pico Reverse Echo Ready!\n");
    printf("Type a message and press Enter.\n");

    int index = 0;

    // Main loop
    while (true) {
        // getchar() will wait until a character is received
        int c = getchar();

        // Check if it's a valid character
        if (c != PICO_ERROR_TIMEOUT) {
            
            // Check for the end of a line (Enter key)
            // CR (Carriage Return) and LF (Line Feed) are common line endings
            if (c == '\n' || c == '\r') {
                
                if (index > 0) {
                    // Null-terminate the string in the buffer
                    buffer[index] = '\0';
                    
                    // Reverse the received string
                    reverse_string(buffer);
                    
                    // Print the reversed string back to the PC
                    printf("%s\n", buffer);
                    
                    // Reset the buffer index for the next message
                    index = 0;
                }
            } 
            else {
                // Add the character to the buffer if there's space
                if (index < BUFFER_SIZE - 1) {
                    buffer[index++] = (char)c;
                }
                // If buffer is full, we process it as is
                else {
                    buffer[index] = '\0';
                    reverse_string(buffer);
                    printf("%s\n", buffer);
                    index = 0; // Reset
                }
            }
        }
    }
    return 0; // Should not be reached
}