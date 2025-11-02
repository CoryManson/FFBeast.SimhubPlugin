using GameReaderCommon;
using SimHub.Plugins;
using System;
using System.Windows.Controls;

namespace FFBeast.SimHubPlugin
{
    [PluginDescription("Plugin to control FFBeast wheelbase functions")]
    [PluginAuthor("FFBeast Community")]
    [PluginName("FFBeast Wheelbase Control")]
    public class FFBeastPlugin : IPlugin, IDataPlugin, IWPFSettings
    {
        private FFBeastWheelApiClient _wheelClient;
        private bool _autoConnect = true;
        private DateTime _lastConnectionAttempt = DateTime.MinValue;
        private readonly TimeSpan _reconnectInterval = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Instance of the current plugin manager
        /// </summary>
        public PluginManager PluginManager { get; set; }

        /// <summary>
        /// Returns the settings control
        /// </summary>
        public Control GetWPFSettingsControl(PluginManager pluginManager)
        {
            return new SettingsControl(this);
        }

        /// <summary>
        /// Called one time per game data update
        /// </summary>
        public void DataUpdate(PluginManager pluginManager, ref GameData data)
        {
            // Try to auto-reconnect if enabled and not connected
            if (_autoConnect && !_wheelClient.IsConnected)
            {
                if ((DateTime.Now - _lastConnectionAttempt) > _reconnectInterval)
                {
                    _lastConnectionAttempt = DateTime.Now;
                    _wheelClient.Connect();
                }
            }
        }

        /// <summary>
        /// Called at plugin manager stop
        /// </summary>
        public void End(PluginManager pluginManager)
        {
            SimHub.Logging.Current.Info("[FFBeast Plugin] Shutting down...");
            _wheelClient?.Dispose();
        }

        /// <summary>
        /// Called once after plugins startup
        /// </summary>
        public void Init(PluginManager pluginManager)
        {
            SimHub.Logging.Current.Info("[FFBeast Plugin] Starting FFBeast Wheelbase Control Plugin");

            _wheelClient = new FFBeastWheelApiClient();
            
            // Try initial connection
            _wheelClient.Connect();

            // Declare actions that can be mapped to controller buttons in SimHub
            this.AddAction(
                actionName: "FFBeast.ResetCenter",
                actionStart: (a, b) =>
                {
                    if (_wheelClient.IsConnected)
                    {
                        _wheelClient.ResetCenter();
                    }
                    else
                    {
                        SimHub.Logging.Current.Warn("[FFBeast Plugin] Cannot reset center: Wheelbase not connected. Attempting to reconnect...");
                        if (_wheelClient.Connect())
                        {
                            _wheelClient.ResetCenter();
                        }
                    }
                });

            this.AddAction(
                actionName: "FFBeast.Reboot",
                actionStart: (a, b) =>
                {
                    if (_wheelClient.IsConnected)
                    {
                        _wheelClient.Reboot();
                    }
                    else
                    {
                        SimHub.Logging.Current.Warn("[FFBeast Plugin] Cannot reboot: Wheelbase not connected");
                    }
                });

            // Expose connection status as properties that can be used in formulas/overlays
            this.AttachDelegate(
                name: "FFBeast.IsConnected",
                valueProvider: () => _wheelClient?.IsConnected ?? false);

            this.AttachDelegate(
                name: "FFBeast.DeviceName",
                valueProvider: () => _wheelClient?.DeviceName ?? "Not connected");

            SimHub.Logging.Current.Info("[FFBeast Plugin] Initialization complete");
            SimHub.Logging.Current.Info("[FFBeast Plugin] Available actions:");
            SimHub.Logging.Current.Info("  - FFBeast.ResetCenter: Reset the wheelbase center position");
            SimHub.Logging.Current.Info("  - FFBeast.Reboot: Reboot the wheelbase controller");
        }
    }
}
