using HidSharp;
using System;
using System.Linq;

namespace FFBeast.SimHubPlugin
{
    /// <summary>
    /// Client for communicating with FFBeast wheelbase via HID
    /// </summary>
    public class FFBeastWheelApiClient : IDisposable
    {
        private const int USB_VID = 1115;
        private const int WHEEL_PID_FS = 22999;
        
        // Report types and commands from wheel_api.h
        private const byte REPORT_GENERIC_INPUT_OUTPUT = 0xA3;
        private const byte DATA_COMMAND_RESET_CENTER = 0x04;
        private const byte DATA_COMMAND_REBOOT = 0x01;
        
        private HidDevice? _device;
        private HidStream? _stream;
        
        public bool IsConnected => _stream != null && _device != null;
        public string DeviceName => _device?.GetProductName() ?? "Not connected";

        /// <summary>
        /// Connect to the FFBeast wheelbase
        /// </summary>
        public bool Connect()
        {
            try
            {
                // Dispose any existing connection first
                Dispose();
                
                var deviceList = DeviceList.Local;
                var hidDevices = deviceList.GetHidDevices(USB_VID, WHEEL_PID_FS);
                
                SimHub.Logging.Current.Info($"[FFBeast Plugin] Found {hidDevices.Count()} device(s) with VID:{USB_VID}, PID:{WHEEL_PID_FS}");
                
                foreach (var device in hidDevices)
                {
                    // We need the vendor interface (interface 0) which has 65-byte reports
                    // The joystick interface has smaller reports
                    var maxOutput = device.GetMaxOutputReportLength();
                    SimHub.Logging.Current.Info($"[FFBeast Plugin] Checking device: {device.GetProductName()}, Max Output: {maxOutput}, Path: {device.DevicePath}");
                    
                    if (maxOutput < 65)
                    {
                        SimHub.Logging.Current.Info($"[FFBeast Plugin] Skipping device (wrong interface, output length {maxOutput} < 65)");
                        continue;
                    }
                    
                    try
                    {
                        _device = device;
                        _stream = device.Open();
                        SimHub.Logging.Current.Info($"[FFBeast Plugin] ✓ Connected to wheelbase: {device.GetProductName()} (Max Output: {maxOutput})");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        SimHub.Logging.Current.Warn($"[FFBeast Plugin] Failed to open device: {ex.Message}");
                        continue;
                    }
                }
                
                SimHub.Logging.Current.Warn("[FFBeast Plugin] Wheelbase not found or unable to open vendor interface. Make sure it's connected.");
                return false;
            }
            catch (Exception ex)
            {
                SimHub.Logging.Current.Error($"[FFBeast Plugin] Error connecting to device: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Send the reset center command to the wheelbase
        /// </summary>
        public bool ResetCenter()
        {
            if (!IsConnected)
            {
                SimHub.Logging.Current.Warn("[FFBeast Plugin] Cannot reset center: Device not connected!");
                return false;
            }

            try
            {
                // Create HID report buffer (65 bytes: Report ID + 64 bytes data)
                byte[] buffer = new byte[65];
                buffer[0] = REPORT_GENERIC_INPUT_OUTPUT;
                buffer[1] = DATA_COMMAND_RESET_CENTER;
                
                _stream!.Write(buffer, 0, buffer.Length);
                SimHub.Logging.Current.Info("[FFBeast Plugin] Reset center command sent successfully!");
                return true;
            }
            catch (Exception ex)
            {
                SimHub.Logging.Current.Error($"[FFBeast Plugin] Error sending reset center command: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Reboot the wheelbase controller
        /// </summary>
        public bool Reboot()
        {
            if (!IsConnected)
            {
                SimHub.Logging.Current.Warn("[FFBeast Plugin] Cannot reboot: Device not connected!");
                return false;
            }

            try
            {
                byte[] buffer = new byte[65];
                buffer[0] = REPORT_GENERIC_INPUT_OUTPUT;
                buffer[1] = DATA_COMMAND_REBOOT;
                
                _stream!.Write(buffer, 0, buffer.Length);
                SimHub.Logging.Current.Info("[FFBeast Plugin] Reboot command sent successfully!");
                return true;
            }
            catch (Exception ex)
            {
                SimHub.Logging.Current.Error($"[FFBeast Plugin] Error sending reboot command: {ex.Message}");
                return false;
            }
        }

        public void Dispose()
        {
            try
            {
                _stream?.Close();
                _stream?.Dispose();
            }
            catch { }
            finally
            {
                _stream = null;
                _device = null;
            }
        }
    }
}
