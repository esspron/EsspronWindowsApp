#!/usr/bin/env python3
"""
Alcohol Tester Communication Tool - Advanced Version
Based on reverse engineering of AlcoholTesterTool_2025_06_04.exe

Protocol Discovery:
- The binary contains strings "FAF5" and "A50D0A" which appear to be ASCII-encoded hex
- FAF5 appears to be a header/prefix (0xFA 0xF5 in binary or "FAF5" as ASCII hex)
- A50D0A could be a command sequence or terminator
- Uses Qt5SerialPort for communication

This version tries both binary and ASCII hex encoding.
"""

import serial
import serial.tools.list_ports
import time
import struct
from datetime import datetime

class AlcoholTester:
    # Protocol constants - trying both interpretations
    HEADER_BINARY = bytes([0xFA, 0xF5])
    HEADER_ASCII = b"FAF5"
    TERMINATOR_ASCII = b"A50D0A"  # Could also be interpreted as command
    
    # Common baud rates
    BAUD_RATES = [9600, 115200, 57600, 38400, 19200, 4800]
    
    def __init__(self, port=None, baudrate=9600):
        self.port = port
        self.baudrate = baudrate
        self.serial = None
        self.responses = []
        
    def connect(self, port=None, baudrate=None):
        if port:
            self.port = port
        if baudrate:
            self.baudrate = baudrate
            
        try:
            self.serial = serial.Serial(
                port=self.port,
                baudrate=self.baudrate,
                bytesize=serial.EIGHTBITS,
                parity=serial.PARITY_NONE,
                stopbits=serial.STOPBITS_ONE,
                timeout=2,
                rtscts=False,
                dsrdtr=False
            )
            # Try different flow control settings
            self.serial.setRTS(True)
            self.serial.setDTR(True)
            time.sleep(0.5)  # Give device time to initialize
            print(f"Connected to {self.port} at {self.baudrate} baud")
            return True
        except Exception as e:
            print(f"Connection failed: {e}")
            return False
    
    def disconnect(self):
        if self.serial and self.serial.is_open:
            self.serial.close()
            
    def send_and_receive(self, data, description="", wait_time=0.5):
        """Send data and receive response"""
        if not self.serial or not self.serial.is_open:
            return None
            
        try:
            self.serial.reset_input_buffer()
            self.serial.reset_output_buffer()
            
            self.serial.write(data)
            self.serial.flush()
            
            time.sleep(wait_time)
            
            response = b""
            while self.serial.in_waiting > 0:
                response += self.serial.read(self.serial.in_waiting)
                time.sleep(0.1)
            
            if response:
                self.responses.append({
                    'sent': data,
                    'received': response,
                    'description': description
                })
                
            return response
        except Exception as e:
            print(f"Error: {e}")
            return None
    
    def print_response(self, data, response, description=""):
        """Pretty print sent/received data"""
        print(f"  [{description}]" if description else "")
        print(f"    Sent ({len(data)} bytes): {data.hex()} | {repr(data)}")
        if response:
            print(f"    Recv ({len(response)} bytes): {response.hex()}")
            print(f"    ASCII: {repr(response)}")
            # Try to parse as ASCII hex
            try:
                if all(c in b'0123456789ABCDEFabcdef' for c in response):
                    decoded = bytes.fromhex(response.decode('ascii'))
                    print(f"    Decoded hex: {decoded.hex()}")
            except:
                pass
        else:
            print("    No response")
        print()
        
    def try_ascii_hex_commands(self):
        """Try commands using ASCII hex encoding (like 'FAF5' instead of 0xFAF5)"""
        print("\n=== Trying ASCII Hex Encoded Commands ===\n")
        
        # ASCII hex encoded commands
        commands = [
            # Connection/Handshake
            (b"FAF5", "Header only"),
            (b"FAF5\r\n", "Header + CRLF"),
            (b"FAF501\r\n", "Connect command"),
            (b"FAF500\r\n", "Init command"),
            (b"FAF5A50D0A\r\n", "Full sequence"),
            
            # Read commands
            (b"FAF505\r\n", "Read records"),
            (b"FAF510\r\n", "Read history"),
            (b"FAF511\r\n", "Read all"),
            (b"FAF520\r\n", "Read time"),
            (b"FAF530\r\n", "Read device ID"),
            
            # A5 prefix commands (from A50D0A pattern)
            (b"A5\r\n", "A5 only"),
            (b"A501\r\n", "A5 + 01"),
            (b"A505\r\n", "A5 + 05"),
            (b"A50D\r\n", "A5 + 0D"),
            (b"A50D0A\r\n", "A5 + 0D0A (CR LF in hex)"),
            
            # Combined formats
            (b"FAF5A501\r\n", "FAF5 + A5 + 01"),
            (b"FAF5A505\r\n", "FAF5 + A5 + 05 (read)"),
        ]
        
        for cmd, desc in commands:
            response = self.send_and_receive(cmd, desc)
            self.print_response(cmd, response, desc)
            
    def try_binary_commands(self):
        """Try commands using binary encoding"""
        print("\n=== Trying Binary Commands ===\n")
        
        # Binary commands
        commands = [
            # With FA F5 header
            (bytes([0xFA, 0xF5]), "Header only"),
            (bytes([0xFA, 0xF5, 0x01]), "Connect"),
            (bytes([0xFA, 0xF5, 0x00]), "Init"),
            (bytes([0xFA, 0xF5, 0x05]), "Read records"),
            (bytes([0xFA, 0xF5, 0x10]), "Get history"),
            (bytes([0xFA, 0xF5, 0x11]), "Get all records"),
            (bytes([0xFA, 0xF5, 0x20]), "Get time"),
            (bytes([0xFA, 0xF5, 0x30]), "Get device ID"),
            
            # With length byte
            (bytes([0xFA, 0xF5, 0x01, 0x01]), "Connect with len"),
            (bytes([0xFA, 0xF5, 0x01, 0x05]), "Read with len"),
            
            # A5 prefix
            (bytes([0xA5, 0x01]), "A5 connect"),
            (bytes([0xA5, 0x05]), "A5 read"),
            (bytes([0xA5, 0x0D, 0x0A]), "A5 + CRLF"),
            (bytes([0xA5, 0x01, 0x0D, 0x0A]), "A5 connect + CRLF"),
            
            # Combined
            (bytes([0xFA, 0xF5, 0xA5, 0x01]), "FAF5 + A5 + 01"),
            (bytes([0xFA, 0xF5, 0xA5, 0x05]), "FAF5 + A5 + 05"),
            (bytes([0xFA, 0xF5, 0xA5, 0x0D, 0x0A]), "FAF5 + A50D0A"),
            
            # With checksums (XOR)
            (bytes([0xFA, 0xF5, 0x01, 0xFA ^ 0xF5 ^ 0x01]), "Connect + XOR checksum"),
            (bytes([0xFA, 0xF5, 0x05, 0xFA ^ 0xF5 ^ 0x05]), "Read + XOR checksum"),
            
            # With sum checksum
            (bytes([0xFA, 0xF5, 0x01, (0xFA + 0xF5 + 0x01) & 0xFF]), "Connect + SUM checksum"),
        ]
        
        for cmd, desc in commands:
            response = self.send_and_receive(cmd, desc)
            self.print_response(cmd, response, desc)
    
    def try_wake_sequences(self):
        """Try various wake-up sequences"""
        print("\n=== Trying Wake Sequences ===\n")
        
        wake_sequences = [
            # Empty/null bytes
            (bytes([0x00]), "Null byte"),
            (bytes([0x00, 0x00]), "Double null"),
            
            # Break signal simulation
            (bytes([0xFF]), "Break"),
            (bytes([0xFF] * 10), "Long break"),
            
            # Common wake patterns
            (bytes([0x55, 0xAA]), "55 AA"),
            (bytes([0xAA, 0x55]), "AA 55"),
            (bytes([0x5A, 0xA5]), "5A A5"),
            (bytes([0xA5, 0x5A]), "A5 5A"),
            
            # AT commands
            (b"AT\r\n", "AT command"),
            (b"AT+VER\r\n", "AT version"),
            
            # Query patterns
            (b"?\r\n", "Query"),
            (b"ID?\r\n", "ID query"),
        ]
        
        for cmd, desc in wake_sequences:
            response = self.send_and_receive(cmd, desc, wait_time=1)
            self.print_response(cmd, response, desc)
    
    def continuous_read(self, duration=30):
        """Continuously read from port for specified duration"""
        print(f"\n=== Continuous Read Mode ({duration}s) ===")
        print("Please interact with the device (press buttons, etc.)")
        print("-" * 50)
        
        start_time = time.time()
        
        while time.time() - start_time < duration:
            if self.serial and self.serial.in_waiting > 0:
                data = self.serial.read(self.serial.in_waiting)
                timestamp = time.strftime("%H:%M:%S")
                print(f"[{timestamp}] Received: {data.hex()} | {repr(data)}")
            time.sleep(0.1)
            
    def scan_all_baudrates(self):
        """Scan through all baud rates"""
        print("\n=== Scanning All Baud Rates ===\n")
        
        test_cmd = bytes([0xFA, 0xF5, 0x01])
        
        for baud in self.BAUD_RATES:
            if self.serial:
                self.disconnect()
            
            if self.connect(baudrate=baud):
                print(f"\nTesting {baud} baud...")
                
                # Try a few commands
                for cmd in [test_cmd, b"FAF501\r\n", bytes([0x55, 0xAA])]:
                    response = self.send_and_receive(cmd)
                    if response:
                        print(f"  GOT RESPONSE at {baud} baud!")
                        print(f"  Command: {cmd.hex()}")
                        print(f"  Response: {response.hex()}")
                        return baud
                        
                self.disconnect()
        
        return None

def list_ports():
    """List available serial ports"""
    ports = serial.tools.list_ports.comports()
    print("Available serial ports:")
    for p in ports:
        print(f"  {p.device}: {p.description} [{p.hwid}]")
    return ports

def main():
    print("=" * 60)
    print("  Alcohol Tester Communication Tool - Advanced")
    print("=" * 60)
    
    # List ports
    ports = list_ports()
    
    # Find USB serial
    usb_port = None
    for p in ports:
        if 'usbserial' in p.device.lower():
            usb_port = p.device
            break
    
    if not usb_port:
        print("\nNo USB serial device found!")
        return
    
    print(f"\nUsing port: {usb_port}")
    
    tester = AlcoholTester(usb_port)
    
    # Try different baud rates
    for baudrate in [9600, 115200]:
        print(f"\n{'='*60}")
        print(f"TESTING AT {baudrate} BAUD")
        print('='*60)
        
        if tester.connect(baudrate=baudrate):
            # Try wake sequences first
            tester.try_wake_sequences()
            
            # Try binary commands
            tester.try_binary_commands()
            
            # Try ASCII hex commands
            tester.try_ascii_hex_commands()
            
            tester.disconnect()
    
    # Print summary of any responses
    if tester.responses:
        print("\n" + "=" * 60)
        print("RESPONSES RECEIVED:")
        print("=" * 60)
        for r in tester.responses:
            print(f"  Sent: {r['sent'].hex()}")
            print(f"  Recv: {r['received'].hex()}")
            print(f"  Desc: {r['description']}")
            print()
    else:
        print("\nNo responses received from device.")
        print("\nPossible issues:")
        print("  1. Device may need to be powered on or in a specific mode")
        print("  2. May require button press on device to initiate communication")
        print("  3. USB cable may be charge-only (no data wires)")
        print("  4. Different baud rate required")
        print("\nTry running serial_monitor.py and interacting with the device")

if __name__ == "__main__":
    main()
