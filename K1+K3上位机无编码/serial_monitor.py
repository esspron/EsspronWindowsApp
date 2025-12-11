#!/usr/bin/env python3
"""
Serial Port Monitor - Listen for any data from the device
"""

import serial
import serial.tools.list_ports
import time
import sys

def monitor_port(port, baudrate=9600, timeout=30):
    """Monitor serial port for incoming data"""
    print(f"Monitoring {port} at {baudrate} baud for {timeout} seconds...")
    print("Please interact with the alcohol tester device (press button, etc.)")
    print("-" * 60)
    
    try:
        ser = serial.Serial(
            port=port,
            baudrate=baudrate,
            bytesize=serial.EIGHTBITS,
            parity=serial.PARITY_NONE,
            stopbits=serial.STOPBITS_ONE,
            timeout=0.5
        )
        
        start_time = time.time()
        buffer = bytearray()
        
        while time.time() - start_time < timeout:
            if ser.in_waiting > 0:
                data = ser.read(ser.in_waiting)
                buffer.extend(data)
                timestamp = time.strftime("%H:%M:%S")
                print(f"[{timestamp}] Received {len(data)} bytes:")
                print(f"  Hex: {data.hex()}")
                print(f"  Raw: {' '.join(f'{b:02X}' for b in data)}")
                try:
                    print(f"  ASCII: {data.decode('utf-8', errors='replace')}")
                except:
                    pass
                print()
            time.sleep(0.1)
        
        ser.close()
        
        if buffer:
            print("\n" + "=" * 60)
            print("Total received data:")
            print(f"  Hex: {buffer.hex()}")
            print(f"  Length: {len(buffer)} bytes")
        else:
            print("\nNo data received")
            
    except Exception as e:
        print(f"Error: {e}")

def try_all_baudrates(port):
    """Try monitoring at different baud rates"""
    baudrates = [9600, 115200, 57600, 38400, 19200, 4800, 2400, 1200]
    
    for baud in baudrates:
        print(f"\n{'='*60}")
        print(f"Trying {baud} baud...")
        print('='*60)
        monitor_port(port, baud, timeout=10)
        
        response = input("\nDid you see any data? (y/n/q to quit): ").lower()
        if response == 'y':
            print(f"Found working baud rate: {baud}")
            return baud
        elif response == 'q':
            break
    
    return None

def main():
    ports = serial.tools.list_ports.comports()
    
    usb_port = None
    for p in ports:
        if 'usbserial' in p.device.lower():
            usb_port = p.device
            break
    
    if not usb_port:
        print("No USB serial device found!")
        return
    
    print(f"Found device: {usb_port}")
    print("\nOptions:")
    print("1. Monitor at 9600 baud")
    print("2. Monitor at 115200 baud")
    print("3. Try all baud rates")
    print("4. Custom baud rate")
    
    choice = input("\nSelect option (1-4): ").strip()
    
    if choice == '1':
        monitor_port(usb_port, 9600, 60)
    elif choice == '2':
        monitor_port(usb_port, 115200, 60)
    elif choice == '3':
        try_all_baudrates(usb_port)
    elif choice == '4':
        baud = int(input("Enter baud rate: "))
        monitor_port(usb_port, baud, 60)
    else:
        # Default: monitor at 9600
        monitor_port(usb_port, 9600, 60)

if __name__ == "__main__":
    main()
