using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace FFBeast.SimHubPlugin
{
    public partial class SettingsControl : UserControl
    {
        public FFBeastPlugin Plugin { get; }
        private DispatcherTimer _updateTimer;

        public SettingsControl()
        {
            InitializeComponent();
        }

        public SettingsControl(FFBeastPlugin plugin) : this()
        {
            this.Plugin = plugin;
            
            // Setup timer to update status
            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = TimeSpan.FromSeconds(1);
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();
            
            UpdateStatus();
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            try
            {
                // Access the wheel client directly from the plugin
                // This is more reliable than using GetPropertyValue
                var wheelClient = typeof(FFBeastPlugin)
                    .GetField("_wheelClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.GetValue(Plugin) as FFBeastWheelApiClient;
                
                if (wheelClient != null && wheelClient.IsConnected)
                {
                    StatusText.Text = "Connected";
                    StatusText.Foreground = System.Windows.Media.Brushes.Green;
                    DeviceText.Text = wheelClient.DeviceName;
                }
                else
                {
                    StatusText.Text = "Disconnected";
                    StatusText.Foreground = System.Windows.Media.Brushes.Red;
                    DeviceText.Text = "Not connected";
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error";
                DeviceText.Text = $"Error: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[FFBeast UI] Error updating status: {ex}");
            }
        }

        private void ReconnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var wheelClient = typeof(FFBeastPlugin)
                    .GetField("_wheelClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.GetValue(Plugin) as FFBeastWheelApiClient;
                
                if (wheelClient != null)
                {
                    wheelClient.Connect();
                    UpdateStatus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reconnecting: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TestRecenterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Access the wheel client directly
                var wheelClient = typeof(FFBeastPlugin)
                    .GetField("_wheelClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.GetValue(Plugin) as FFBeastWheelApiClient;
                
                if (wheelClient == null)
                {
                    MessageBox.Show("Wheel client not initialized!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                if (!wheelClient.IsConnected)
                {
                    MessageBox.Show("Wheelbase not connected!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                bool success = wheelClient.ResetCenter();
                
                if (success)
                {
                    MessageBox.Show("Recenter command sent successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Failed to send recenter command. Check SimHub logs for details.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending recenter command: {ex.Message}\n\nCheck SimHub logs for details.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TestRebootButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "This will reboot the wheelbase controller. Continue?", 
                "Confirm Reboot", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Warning);
            
            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                // Access the wheel client directly
                var wheelClient = typeof(FFBeastPlugin)
                    .GetField("_wheelClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.GetValue(Plugin) as FFBeastWheelApiClient;
                
                if (wheelClient == null)
                {
                    MessageBox.Show("Wheel client not initialized!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                if (!wheelClient.IsConnected)
                {
                    MessageBox.Show("Wheelbase not connected!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                bool success = wheelClient.Reboot();
                
                if (success)
                {
                    MessageBox.Show("Reboot command sent. The wheelbase will restart.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Failed to send reboot command. Check SimHub logs for details.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending reboot command: {ex.Message}\n\nCheck SimHub logs for details.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
