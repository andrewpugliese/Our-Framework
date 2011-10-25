// AppConfigMgr.GetValue() – Looks up for a give key. Throws an exception if config key NOT found
string keyValue = ConfigManager.GetValue("ConfigKeyName");
             
// AppConfigMgr.GetValue() with convertor function
int experianTimeout = AppConfigMgr.GetValue("ExperianTimeout", val => Convert.ToInt32(val));
System.Net.IPAddress ipAddress = AppConfigMgr.GetValue( "AppServer_IPAddress", val => System.Net.IPAddress.Parse( val ) );
        
// Helper functions
Int32 keyIntValue = AppConfigMgr.GetValueAsInt32("ConfigKeyName");
IEnumerable<string> configKeys = AppConfigMgr.GetKeys();
             
// AppConfigMgr.GetValueOrDefault() – Looks up value for a given key. Returns default value if key NOT found.
string keyValue = AppConfigMgr.GetValueOrDefault("ConfigKeyName", null);             
Int32 keyIntValue = AppConfigMgr.GetValueOrDefault("ConfigKeyName", 0, val => Convert.ToInt32(val));
             
// *********************************************** //

// Use SetRuntimeValue to make available to the entire application any custom key and value for example
// a trace level from a UI edit box. You can provide any function which knows where and how to get the
// data.
AppConfigMgr.SetRuntimeValue ("TraceLevel", () => Convert.ToInt32(txtTraceLevel.Text));
             
// Use the GetRuntimeValue in any place to get the TraceLevel from the UI text box.
int traceLevel = AppConfigMgr.GetRuntimeValue <int>("TraceLevel");
             
// *********************************************** //

