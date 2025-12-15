using System.IO;
using UnityEngine;

public class LogToFile : MonoBehaviour
{
    public string logFilePath;

    void Awake()
    {
        // ログファイルのパスを設定
        logFilePath = Path.Combine(Application.dataPath, "Resources/game_log.txt");

        // テスト用にファイルを初期化（必要に応じて削除）
        if (File.Exists(logFilePath))
        {
            File.Delete(logFilePath);
        }

        // ログイベントを設定
        Application.logMessageReceived += HandleLog;
    }

    void OnDestroy()
    {
        // イベントの解除
        Application.logMessageReceived -= HandleLog;
    }

    // ログイベントハンドラー
    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        string logEntry = $"{System.DateTime.Now}: [{type}] {logString}\n";
        File.AppendAllText(logFilePath, logEntry);
    }
}
