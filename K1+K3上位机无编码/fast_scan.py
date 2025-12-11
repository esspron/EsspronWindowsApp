import serial
import time
import sys

# Common baud rates
BAUD_RATES = [9600, 115200, 57600, 38400, 19200, 4800]

# Commands to try (based on reverse engineering)
COMMANDS = [
    (b'\xFA\xF5\x01', "Connect"),
    (b'\xFA\xF5\x03', "Status"),
    (b'\xFA\xF5\x05', "Read Records"),
    (b'\xA5\x01\x0D\x0A', "A5 Connect"),
    (b'\xA5\x05\x0D\x0A', "A5 Read Records"),
]

def scan():
    port = "/dev/cu.usbserial-1130"
    print(f"Scanning on {port}...")
    
    for baud in BAUD_RATES:
        print(f"Trying {baud}...")
        try:
            with serial.Serial(port, baud, timeout=0.2) as ser:
                ser.dtr = True
                ser.rts = True
                # Flush
                ser.reset_input_buffer()
                ser.reset_output_buffer()
                
                for cmd_bytes, cmd_name in COMMANDS:
                    ser.write(cmd_bytes)
                    ser.flush()
                    
                    time.sleep(0.15)
                    
                    if ser.in_waiting:
                        resp = ser.read(ser.in_waiting)
                        print(f"SUCCESS! {baud} baud, Cmd: {cmd_name}")
                        print(f"Response: {resp.hex()}")
                        try:
                            print(f"ASCII: {resp.decode('utf-8', errors='ignore')}")
                        except:
                            pass
                        return baud, cmd_bytes, resp
        except Exception as e:
            print(f"Error at {baud}: {e}")
            
    print("Scan complete. No response found.")
    return None

if __name__ == "__main__":
    scan()
