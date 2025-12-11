namespace EsspronAlcoholTester.Models
{
    public enum DeviceConnectionState
    {
        Disconnected,
        Detecting,
        Found,
        Connecting,
        Connected,
        ReadingData,
        DataReady,
        Importing,
        ImportComplete,
        Error
    }
}
