using System;
using System.IO;
using UnityEngine;

[Serializable]
public class ApiConfig
{
    public string baseUrl;           // e.g. "https://example.com"
    public string registerPath;      // "Api/register_player.php"
    public string insertRoundPath;   // "Api/insert_round.php"
    public string logDataPath;       // "Api/log_data.php"
    public string logActionPath;     // "Api/log_action.php"
    public string getRoundPath;      // "Api/get_round.php?round_number={round}"
    public string runPythonPath;     // "Api/run_python.php"
}

public static class ApiSettings
{
    private static ApiConfig _cfg;
    public static ApiConfig Cfg
    {
        get
        {
            if (_cfg == null) Load();
            return _cfg;
        }
    }

    public static void Load()
    {
        try
        {
            string path = Path.Combine(Application.streamingAssetsPath, "server_config.json");
            if (!File.Exists(path))
            {
                Debug.LogWarning("[ApiSettings] server_config.json not found. Using empty defaults.");
                _cfg = new ApiConfig();
                return;
            }
            string json = File.ReadAllText(path);
            _cfg = JsonUtility.FromJson<ApiConfig>(json);
        }
        catch (Exception e)
        {
            Debug.LogError("[ApiSettings] Load error: " + e.Message);
            _cfg = new ApiConfig();
        }
    }

    public static string Build(string pathOrQuery)
    {
        if (string.IsNullOrEmpty(pathOrQuery)) return Cfg.baseUrl ?? "";
        if (string.IsNullOrEmpty(Cfg.baseUrl)) return pathOrQuery;
        if (pathOrQuery.StartsWith("http")) return pathOrQuery;
        string sep = Cfg.baseUrl.EndsWith("/") || pathOrQuery.StartsWith("/") ? "" : "/";
        return Cfg.baseUrl + sep + pathOrQuery;
    }
}
