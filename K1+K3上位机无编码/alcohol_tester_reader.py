#!/usr/bin/env python3
"""
Alcohol Tester Communication Tool
Reverse engineered from AlcoholTesterTool_2025_06_04.exe

Protocol Analysis:
- Header: FAF5 (0xFA 0xF5)
- Commands use A5 prefix
- Response ends with 0D 0A (CR LF)

Based on Qt5SerialPort communication patterns observed in the binary.
"""

import serial
import serial.tools.list_ports
import time
import struct
from datetime import datetime

# Protocol constants discovered from binary analysis
HEADER = bytes([0xFA, 0xF5])
CMD_PREFIX = 0xA5
LINE_END = bytes([0x0D, 0x0A])

# Common baud rates for such devices
BAUD_RATES = [9600, 115200, 57600, 38400, 19200, 4800]

class AlcoholTesterReader:
    def __init__(self, port=None, baudrate=9600):
        self.port = port
        self.baudrate = baudrate
        self.serial = None
        
    def find_device(self):
        """Find available serial ports"""
        ports = serial.tools.list_ports.comports()
        print("Available serial ports:")
        for p in ports:
            print(f"  - {p.device}: {p.description}")
        return ports
    
    def connect(self, port=None, baudrate=None):
        """Connect to the alcohol tester"""
        if port:
            self.port = port
        if baudrate:
            self.baudrate = baudrate
            
        if not self.port:
            print("Error: No port specified")
            return False
            
        try:
            self.serial = serial.Serial(
                port=self.port,
                baudrate=self.baudrate,
                bytesize=serial.EIGHTBITS,
                parity=serial.PARITY_NONE,
                stopbits=serial.STOPBITS_ONE,
                timeout=2
            )
            print(f"Connected to {self.port} at {self.baudrate} baud")
            return True
        except Exception as e:
            print(f"Connection failed: {e}")
            return False
    
    def disconnect(self):
        """Disconnect from the device"""
        if self.serial and self.serial.is_open:
            self.serial.close()
            print("Disconnected")
    
    def calculate_checksum(self, data):
        """Calculate checksum - common methods"""
        # Try simple XOR checksum
        xor_sum = 0
        for b in data:
            xor_sum ^= b
        return xor_sum
    
    def calculate_sum_checksum(self, data):
        """Calculate sum checksum"""
        return sum(data) & 0xFF
    
    def build_command(self, cmd_code, data=None):
        """Build a command packet"""
        if data is None:
            data = []
        
        # Various command formats to try
        commands = []
        
        # Format 1: HEADER + CMD + DATA + CHECKSUM
        packet1 = list(HEADER) + [cmd_code] + data
        packet1.append(self.calculate_checksum(packet1))
        commands.append(('Format1_XOR', bytes(packet1)))
        
        # Format 2: HEADER + LENGTH + CMD + DATA + CHECKSUM
        length = len(data) + 1  # cmd + data
        packet2 = list(HEADER) + [length, cmd_code] + data
        packet2.append(self.calculate_checksum(packet2))
        commands.append(('Format2_LEN_XOR', bytes(packet2)))
        
        # Format 3: A5 + CMD + DATA + 0D 0A (based on A50D0A pattern)
        packet3 = [0xA5, cmd_code] + data + list(LINE_END)
        commands.append(('Format3_A5', bytes(packet3)))
        
        # Format 4: FA F5 + CMD + DATA + 0D 0A
        packet4 = list(HEADER) + [cmd_code] + data + list(LINE_END)
        commands.append(('Format4_HEADER_CRLF', bytes(packet4)))
        
        # Format 5: Simple CMD + DATA
        packet5 = [cmd_code] + data
        commands.append(('Format5_SIMPLE', bytes(packet5)))
        
        return commands
    
    def send_raw(self, data):
        """Send raw bytes and receive response"""
        if not self.serial or not self.serial.is_open:
            print("Not connected")
            return None
        
        try:
            # Clear buffers
            self.serial.reset_input_buffer()
            self.serial.reset_output_buffer()
            
            # Send data
            self.serial.write(data)
            self.serial.flush()
            
            # Wait for response
            time.sleep(0.3)
            
            # Read response
            response = self.serial.read(self.serial.in_waiting or 256)
            return response
        except Exception as e:
            print(f"Communication error: {e}")
            return None
    
    def probe_commands(self):
        """Probe various commands to find working ones"""
        print("\n=== Probing for working commands ===\n")
        
        # Command codes to try based on typical device commands
        command_codes = {
            0x01: "Connect/Handshake",
            0x02: "Get Device Info",
            0x03: "Get Status",
            0x04: "Read Record Count",
            0x05: "Read Records",
            0x06: "Read Record by Index",
            0x10: "Get History",
            0x11: "Get All Records",
            0x20: "Get Time",
            0x21: "Set Time",
            0x30: "Get Device ID",
            0x40: "Read Data",
            0x50: "Get Alcohol Level",
            0x55: "Handshake",
            0x5A: "Connect",
            0xA5: "Special Command",
            0xAA: "Sync",
            0xF0: "Get Info",
            0xFA: "Status Query",
            0xFF: "Reset/Init",
        }
        
        working_commands = []
        
        for cmd_code, cmd_name in command_codes.items():
            print(f"Testing command 0x{cmd_code:02X} ({cmd_name})...")
            
            commands = self.build_command(cmd_code)
            
            for format_name, cmd_bytes in commands:
                response = self.send_raw(cmd_bytes)
                
                if response and len(response) > 0:
                    print(f"  [{format_name}] Sent: {cmd_bytes.hex()}")
                    print(f"  [{format_name}] Received: {response.hex()}")
                    print(f"  [{format_name}] ASCII: {response}")
                    working_commands.append({
                        'code': cmd_code,
                        'name': cmd_name,
                        'format': format_name,
                        'request': cmd_bytes,
                        'response': response
                    })
                    print()
                    
                time.sleep(0.1)
        
        return working_commands
    
    def try_connection_sequence(self):
        """Try common connection sequences"""
        print("\n=== Trying connection sequences ===\n")
        
        sequences = [
            # Based on FAF5 and A50D0A patterns
            ("FAF5 A5 0D 0A", bytes([0xFA, 0xF5, 0xA5, 0x0D, 0x0A])),
            ("FA F5 01", bytes([0xFA, 0xF5, 0x01])),
            ("FA F5 00 01", bytes([0xFA, 0xF5, 0x00, 0x01])),
            ("A5 01 0D 0A", bytes([0xA5, 0x01, 0x0D, 0x0A])),
            ("A5 00 0D 0A", bytes([0xA5, 0x00, 0x0D, 0x0A])),
            ("FA F5 A5 01 0D 0A", bytes([0xFA, 0xF5, 0xA5, 0x01, 0x0D, 0x0A])),
            
            # Common handshake patterns
            ("AT", b"AT\r\n"),
            ("55 AA", bytes([0x55, 0xAA])),
            ("AA 55", bytes([0xAA, 0x55])),
            ("5A A5", bytes([0x5A, 0xA5])),
            
            # Read records commands
            ("FA F5 05", bytes([0xFA, 0xF5, 0x05])),
            ("FA F5 10", bytes([0xFA, 0xF5, 0x10])),
            ("FA F5 11", bytes([0xFA, 0xF5, 0x11])),
            
            # With checksums
            ("FA F5 01 + XOR", bytes([0xFA, 0xF5, 0x01, 0xFA ^ 0xF5 ^ 0x01])),
            ("FA F5 05 + XOR", bytes([0xFA, 0xF5, 0x05, 0xFA ^ 0xF5 ^ 0x05])),
        ]
        
        for name, seq in sequences:
            response = self.send_raw(seq)
            print(f"Sent [{name}]: {seq.hex()}")
            if response:
                print(f"  Response: {response.hex()}")
                try:
                    print(f"  ASCII: {response.decode('utf-8', errors='replace')}")
                except:
                    pass
            else:
                print("  No response")
            print()
            time.sleep(0.2)
    
    def read_records(self):
        """Try to read alcohol test records"""
        print("\n=== Attempting to read records ===\n")
        
        # Try various record reading commands
        record_commands = [
            # Read all records
            bytes([0xFA, 0xF5, 0x05]),
            bytes([0xFA, 0xF5, 0x10]),
            bytes([0xFA, 0xF5, 0x11]),
            bytes([0xFA, 0xF5, 0x20]),
            
            # With index parameter (record 0, 1, etc.)
            bytes([0xFA, 0xF5, 0x06, 0x00]),
            bytes([0xFA, 0xF5, 0x06, 0x01]),
            
            # A5 prefix format
            bytes([0xA5, 0x05, 0x0D, 0x0A]),
            bytes([0xA5, 0x10, 0x0D, 0x0A]),
        ]
        
        for cmd in record_commands:
            response = self.send_raw(cmd)
            print(f"Command: {cmd.hex()}")
            if response:
                print(f"  Response ({len(response)} bytes): {response.hex()}")
                self.parse_response(response)
            else:
                print("  No response")
            print()
            time.sleep(0.2)
    
    def parse_response(self, data):
        """Try to parse response data"""
        if not data:
            return
        
        print(f"  Raw bytes: {' '.join(f'{b:02X}' for b in data)}")
        
        # Try to decode as ASCII
        try:
            ascii_str = data.decode('utf-8', errors='replace')
            if ascii_str.isprintable() or '\r' in ascii_str or '\n' in ascii_str:
                print(f"  ASCII: {repr(ascii_str)}")
        except:
            pass
        
        # Check for common response headers
        if len(data) >= 2:
            if data[0] == 0xFA and data[1] == 0xF5:
                print("  Header: FA F5 detected")
                if len(data) > 2:
                    print(f"  Command response: 0x{data[2]:02X}")
                    if len(data) > 3:
                        print(f"  Data: {data[3:].hex()}")
        
        # Try to find alcohol concentration values
        # Often stored as 16-bit integers (mg/L or mg/100mL)
        if len(data) >= 4:
            for i in range(len(data) - 1):
                val_le = struct.unpack('<H', data[i:i+2])[0]
                val_be = struct.unpack('>H', data[i:i+2])[0]
                if 0 < val_le < 5000:  # Reasonable range for alcohol readings
                    print(f"  Possible value at offset {i} (LE): {val_le}")
                if 0 < val_be < 5000:
                    print(f"  Possible value at offset {i} (BE): {val_be}")

    def auto_detect_baudrate(self):
        """Try to auto-detect the correct baud rate"""
        print("\n=== Auto-detecting baud rate ===\n")
        
        test_cmd = bytes([0xFA, 0xF5, 0x01])  # Simple connect command
        
        for baud in BAUD_RATES:
            print(f"Trying {baud} baud...")
            if self.serial:
                self.disconnect()
            
            if self.connect(baudrate=baud):
                response = self.send_raw(test_cmd)
                if response and len(response) > 0:
                    print(f"  Got response at {baud} baud!")
                    print(f"  Response: {response.hex()}")
                    return baud
                self.disconnect()
        
        return None

def main():
    print("=" * 60)
    print("  Alcohol Tester History Reader")
    print("  Reverse engineered from AlcoholTesterTool_2025_06_04.exe")
    print("=" * 60)
    
    reader = AlcoholTesterReader()
    
    # Find available ports
    ports = reader.find_device()
    
    # Look for USB serial device
    usb_port = None
    for p in ports:
        if 'usbserial' in p.device.lower() or 'usb' in p.device.lower():
            usb_port = p.device
            break
    
    if not usb_port:
        print("\nNo USB serial device found!")
        print("Please connect the alcohol tester via USB-C")
        return
    
    print(f"\nUsing port: {usb_port}")
    
    # Try different baud rates
    for baudrate in BAUD_RATES:
        print(f"\n{'='*60}")
        print(f"Trying baud rate: {baudrate}")
        print('='*60)
        
        if reader.connect(usb_port, baudrate):
            # Try connection sequences
            reader.try_connection_sequence()
            
            # Try to read records
            reader.read_records()
            
            # Probe commands
            working = reader.probe_commands()
            
            if working:
                print("\n=== WORKING COMMANDS FOUND ===")
                for w in working:
                    print(f"  Command 0x{w['code']:02X} ({w['name']})")
                    print(f"    Format: {w['format']}")
                    print(f"    Request: {w['request'].hex()}")
                    print(f"    Response: {w['response'].hex()}")
            
            reader.disconnect()
    
    print("\n" + "=" * 60)
    print("Scan complete!")
    print("=" * 60)

if __name__ == "__main__":
    main()
