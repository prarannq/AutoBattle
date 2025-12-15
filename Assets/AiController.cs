using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// UnityEngine.Debug を Debug として使う
using Debug = UnityEngine.Debug;
using System.Diagnostics;
using System.IO;
using System.Text;

[System.Serializable]
public class UnitWinRateRequest
{
    public List<string> unitIdList;
    public List<string> unitIdShopList;

    public UnitWinRateRequest(List<string> unitIdList, List<string> unitIdShopList)
    {
        this.unitIdList = unitIdList;
        this.unitIdShopList = unitIdShopList;
    }
}

[System.Serializable]
public class UnitWinRateResponse
{
    public string status;
    public List<UnitWinRateData> predictions;
}

[System.Serializable]
public class UnitWinRateData
{
    public string unit;
    public float winRate;
}

[System.Serializable]
public class StatsRoot
{
    public string status;
    public StatsData data;
}
[System.Serializable]
public class StatsData
{
    public List<UnitWinRate> unit_win_rates;
    public List<ItemWinRate> item_win_rates;
    public List<UnitCombination> unit_combinations;
    public List<UnitItemCombination> unit_item_combinations;
    public List<ItemCombination> item_combinations;
}

[System.Serializable]
public class UnitWinRate
{
    public string unit_name;
    public string win_rate;
}
[System.Serializable]
public class ItemWinRate
{
    public string item_name;
    public int item_cost;
    public string win_rate;
}
[System.Serializable]
public class UnitCombination
{
    public string unit1;
    public string unit2;
    public string win_rate;
}
[System.Serializable]
public class UnitItemCombination
{
    public string unit_name;
    public string item_name;
    public int item_cost;
    public string win_rate;
}
[System.Serializable]
public class ItemCombination
{
    public string item1;
    public string item2;
    public int item_cost;
    public string win_rate;
}

[System.Serializable]
public class PythonResponse
{
    public string status;
    public float winRate;
}

public class AiController : MonoBehaviour
{
    public GameController gameController;
    public BattleController battleController;
    public ServerController serverController;
    public List<UnitShopButton> unitShopList;
    public Box box;
    public BattleButton battleButton;
    public BattleEndButton battleEndButton;
    public Reroll rerollButton;
    public List<Shop> shopList;

    public List<GameObject> fieldList = new List<GameObject>(6);
    public List<Unit> unitList = new List<Unit>(6);
    public List<Item> boxItemList = new List<Item>(15);
    List<ActionLog> actionChoiseList = new List<ActionLog>();
    ActionLog actionLog = new ActionLog();

    private int gold;
    private int round;

    private string winRateUnit;

    public float aiWinRate;

    //  URLはApiSettingsから取得（直書きしない）
    private string PythonApiUrl => ApiSettings.Build(ApiSettings.Cfg.runPythonPath);

    // パース後に使いたいデータ
    public StatsRoot statsRoot;

    void Start() { }
    void Update() { }

    /// <summary>
    /// サーバー経由で Python に推論リクエストを送る
    /// </summary>
    public IEnumerator GetWinRate()
    {
        Debug.Log("IN: GetWinRate()");

        // 1) BattleRecord を作成
        BattleRecord battleRecord = battleController.SetBattleRecord();

        // 2) JSON にシリアライズ
        string json = JsonUtility.ToJson(battleRecord);
        Debug.Log("battleRecord JSON: " + json);
        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);

        // 3) using で必ず破棄 + ハンドラも一緒に破棄するフラグを有効化
        using (var request = new UnityWebRequest(PythonApiUrl, UnityWebRequest.kHttpVerbPOST))
        {
            request.disposeDownloadHandlerOnDispose = true;
            request.disposeUploadHandlerOnDispose = true;

            request.uploadHandler = new UploadHandlerRaw(jsonBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // 4) 送信
            yield return request.SendWebRequest();

            // 5) エラーチェック
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + request.error);
                yield break;
            }

            // 6) レスポンス解析
            string responseText = request.downloadHandler.text;
            Debug.Log("Server Response: " + responseText);

            PythonResponse response = null;
            try { response = JsonUtility.FromJson<PythonResponse>(responseText); }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to parse server response: " + e);
                yield break;
            }

            if (response != null && response.status == "success")
            {
                aiWinRate = response.winRate;
                Debug.Log($"推定勝率: {(aiWinRate * 100f):F2}%");
            }
            else
            {
                Debug.LogError("Failed to parse server response (status != success).");
            }
        } // ← ここで request / handlers が確実に破棄
    }

}
