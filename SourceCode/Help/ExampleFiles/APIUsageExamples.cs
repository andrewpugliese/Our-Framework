// ConfigManager.GetValue() – Looks up for a give key. Throws an exception if config key NOT found
string keyValue = ConfigManager.GetValue("ConfigKeyName");
             
// ConfigManager.GetValue() with convertor function
int experianTimeout = AppConfigMgr.GetValue("ExperianTimeout", val => Convert.ToInt32(val));
System.Net.IPAddress ipAddress = ConfigManager.GetValue( "VehicleServer_IPAddress", val => System.Net.IPAddress.Parse( val ) );
        
// Helper functions
Int32 keyIntValue = ConfigManager.GetValueAsInt32("ConfigKeyName");
IEnumerable<string> configKeys = ConfigManager.GetKeys();
Dictionary<string, string> configSettings = ConfigManager.GetAllSettings();
             
// ConfigManager.GetValueOrDefault() – Looks up value for a given key. Returns default value if key NOT found.
string keyValue = ConfigManager.GetValueOrDefault("ConfigKeyName", null);             
Int32 keyIntValue = ConfigManager.GetValueOrDefault("ConfigKeyName", 0, val => Convert.ToInt32(val));
             
// *********************************************** //

// Use SetRuntimeValue to make available to the entire application any custom key and value for example
// a trace level from a UI edit box. You can provide any function which knows where and how to get the
// data.
ConfigManager.SetRuntimeValue ("TraceLevel", () => Convert.ToInt32(txtTraceLevel.Text));
             
// Use the GetRuntimeValue in any place to get the TraceLevel from the UI text box.
int traceLevel = ConfigManager.GetRuntimeValue <int>("TraceLevel");
             
// *********************************************** //

// Access the global configuration setting
string globalKeyValue = ConfigManager.Global.GetValue("ConfigKeyName");
             
// Access the global configuration setting
string globalKeyValue = ConfigManager.Global.GetValueOrDefault("ConfigKeyName", "DEF");

// *********************************************** //
