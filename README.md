# Esspron Alcohol Tester Manager

A Windows desktop application for managing alcohol tester devices, importing test data, and syncing with cloud storage via Supabase.

## Features

- **USB Device Detection**: Automatically detects when an Esspron alcohol tester is connected
- **License Management**: Activate devices using license keys (2000-3000 INR/month per device)
- **Data Import**: Download test records from the device to the cloud
- **Dashboard**: View statistics and recent test records
- **Data Management**: Filter, search, and export test data
- **Cloud Sync**: All data is synced to Supabase for backup and cross-device access

## Prerequisites

- Windows 10/11
- .NET 8.0 SDK
- Visual Studio 2022 (recommended) or VS Code with C# extension

## Installation

1. Clone or download this repository
2. Open `EsspronAlcoholTester.sln` in Visual Studio
3. Restore NuGet packages
4. Build and run the application

## Configuration

### Supabase Setup

1. Create a Supabase project at https://supabase.com
2. Update the credentials in `Services/SupabaseService.cs`:

```csharp
private const string SUPABASE_URL = "https://your-project.supabase.co";
private const string SUPABASE_KEY = "your-anon-key";
```

### Database Schema

Create the following tables in your Supabase database:

```sql
-- Users table (handled by Supabase Auth)

-- Licenses table
CREATE TABLE licenses (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    license_key VARCHAR(50) UNIQUE NOT NULL,
    user_id UUID REFERENCES auth.users(id),
    allowed_devices INT DEFAULT 1,
    activated_devices INT DEFAULT 0,
    expiry_date TIMESTAMP NOT NULL,
    is_active BOOLEAN DEFAULT true,
    monthly_price DECIMAL(10,2),
    plan_name VARCHAR(50),
    created_at TIMESTAMP DEFAULT NOW()
);

-- Devices table
CREATE TABLE devices (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    device_id VARCHAR(100) UNIQUE NOT NULL,
    device_name VARCHAR(255),
    serial_number VARCHAR(100),
    firmware_version VARCHAR(50),
    user_id UUID REFERENCES auth.users(id),
    license_id UUID REFERENCES licenses(id),
    is_activated BOOLEAN DEFAULT false,
    activated_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT NOW()
);

-- Test Records table
CREATE TABLE test_records (
    id SERIAL PRIMARY KEY,
    device_id VARCHAR(100) NOT NULL,
    device_name VARCHAR(255),
    test_time TIMESTAMP NOT NULL,
    alcohol_level DECIMAL(10,2) NOT NULL,
    alcohol_unit VARCHAR(20) DEFAULT 'mg/100ml',
    result VARCHAR(20),
    subject_id VARCHAR(100),
    notes TEXT,
    user_id UUID REFERENCES auth.users(id),
    imported_at TIMESTAMP DEFAULT NOW()
);
```

## USB Device Integration

The USB device communication protocols need to be reverse-engineered. Update the following in `Services/UsbDeviceService.cs`:

```csharp
// TODO: Update these values after reverse engineering the device
private const int TARGET_VENDOR_ID = 0x0000;  // Replace with actual VID
private const int TARGET_PRODUCT_ID = 0x0000; // Replace with actual PID
```

### Steps to Reverse Engineer

1. Connect the device to your computer
2. Use USBlyzer, Wireshark with USBPcap, or similar tools to capture USB traffic
3. Identify the VID/PID from Device Manager
4. Analyze the communication protocol
5. Implement the actual data reading logic in `ImportDataAsync()`

## Workflow

1. Customer purchases an Esspron Alcohol Tester
2. Tests 200-300 drivers/workers per day
3. At end of day, connects device to Windows PC
4. Runs the application and signs in
5. Application detects the device
6. First-time users activate with a license key
7. Clicks "Connect" then "Import Data"
8. Data (alcohol level, time, BAC, device name) is imported to cloud

## Pricing Model

- **Basic**: ₹2,000/month per device (1 device, CSV export)
- **Professional**: ₹2,500/month per device (up to 5 devices, Excel export, analytics)
- **Enterprise**: ₹3,000/month per device (unlimited devices, PDF reports, API access)

## Project Structure

```
EsspronAlcoholTester/
├── App.xaml                    # Application entry point
├── Models/                     # Data models
│   ├── AlcoholTestRecord.cs
│   ├── DeviceInfo.cs
│   ├── DeviceConnectionState.cs
│   ├── License.cs
│   └── User.cs
├── Services/                   # Business logic services
│   ├── ISupabaseService.cs
│   ├── SupabaseService.cs
│   ├── IUsbDeviceService.cs
│   ├── UsbDeviceService.cs
│   ├── ILicenseService.cs
│   ├── LicenseService.cs
│   ├── IDataService.cs
│   └── DataService.cs
├── ViewModels/                 # MVVM ViewModels
│   ├── LoginViewModel.cs
│   ├── MainViewModel.cs
│   ├── DeviceViewModel.cs
│   ├── DataViewModel.cs
│   └── LicenseViewModel.cs
├── Views/                      # XAML Views
│   ├── MainWindow.xaml
│   ├── LoginView.xaml
│   ├── DashboardView.xaml
│   ├── DeviceConnectionView.xaml
│   ├── DataView.xaml
│   └── LicenseView.xaml
├── Converters/                 # Value converters for UI
│   └── ValueConverters.cs
└── Themes/                     # Styling
    ├── Colors.xaml
    └── Styles.xaml
```

## Tech Stack

- **Framework**: WPF (.NET 8.0)
- **MVVM**: CommunityToolkit.Mvvm
- **Backend**: Supabase
- **USB Communication**: LibUsbDotNet, System.IO.Ports

## Demo Mode

For testing without a physical device, the application includes simulated device detection and data generation. The simulated device appears after 3 seconds of scanning.

## License

Proprietary - Esspron Technologies

## Support

Contact: support@esspron.com
