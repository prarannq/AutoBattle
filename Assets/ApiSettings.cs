using UnityEngine;

[System.Serializable]
public class ApiConfig
{
    public string baseUrl;
    public string registerPath;
    public string insertRoundPath;
    public string logDataPath;
    public string logActionPath;
    public string getRoundPath;
    public string runPythonPath;
    public string unitWinRatePath;
    public string statsPath;
    public string predictPath;
}

public static class ApiSettings
{
    private static ApiConfig _cfg;

    public static ApiConfig Cfg
    {
        get
        {
            if (_cfg == null)
            {
                Load();
            }
            return _cfg;
        }
    }

    public static void Load()
    {
        

        TextAsset textAsset = Resources.Load<TextAsset>("server_config");

        if (textAsset == null)
        {
            Debug.LogWarning("[ApiSettings] server_config.json not found in Resources. Using empty defaults.");
            _cfg = new ApiConfig();
            return;
        }

        _cfg = JsonUtility.FromJson<ApiConfig>(textAsset.text);

        if (_cfg == null)
        {
            Debug.LogError("[ApiSettings] Failed to parse server_config.json");
            _cfg = new ApiConfig();
            return;
        }

        Debug.Log("[ApiSettings] Loaded config from Resources/server_config.json");
    }

    public static string Build(string path)
    {
        string baseUrl = Cfg.baseUrl ?? "";
        string p = path ?? "";

        baseUrl = baseUrl.TrimEnd('/');
        p = p.TrimStart('/');

        return $"{baseUrl}/{p}";
    }
}