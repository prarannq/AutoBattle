using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class BattleRecord
{
    public int playerId;
    public int roundNumber;
    public int playerHp;
    public int gold;
    public List<UnitData> units;
    public bool result;
}

[System.Serializable]
public class UnitData
{
    public string unitId;
    public int fieldPos;
    public List<string> items;
    public List<int> cost;
}

[System.Serializable]
public class StateLog
{
    public BattleRecord battleRecord;
    public List<string> boxItem;
    public List<string> shopItem;
}

[System.Serializable]
public class ActionLog
{
    public int playerId;
    public int roundNumber;
    public int stateId;
    public string actionType;

    public string unit1;
    public string unit2;
    public string item1;
    public string item2;
    public int position1;
    public int position2;
}

[System.Serializable]
public class EnemyUnitsResponse
{
    public string status;
    public string message;
    public List<UnitData> units;
}

[System.Serializable]
public class PlayerIdResponse
{
    public int player_id;
}

[System.Serializable]
public class StateResponse
{
    public string status;
    public int state_id;
}

[System.Serializable]
public class BestActionResponse
{
    public ActionLog bestAction;
    public int bestActionIndex;
    public float score;
}

public class ServerController : MonoBehaviour
{
    private FieldController fieldController;
    public GameController gameController;
    public BattleController battleController;
    public Box box;
    public Reroll reroll;

    // ★ URLはApiSettingsから取得（直書きしない）
    private string IdUrl => ApiSettings.Build(ApiSettings.Cfg.registerPath);
    private string ResultUrl => ApiSettings.Build(ApiSettings.Cfg.insertRoundPath);
    private string LogUrl => ApiSettings.Build(ApiSettings.Cfg.logDataPath);
    private string ActionLogUrl => ApiSettings.Build(ApiSettings.Cfg.logActionPath);
    private string GetRoundPath => ApiSettings.Cfg.getRoundPath;

    public int playerId;
    public int stateId;

    void Start()
    {
        fieldController = GetComponent<FieldController>();
    }

    // 敵の情報を受け取り、生成する
    public void SetEnemy(int round)
    {
        if (gameController.useServerEnemy == true)
        {
            StartCoroutine(LoadEnemyUnits(round));
        }
        else
        {
            List<UnitData> unitDataList = new List<UnitData>(6);
            UnitData unitData = new UnitData();
            unitDataList.Add(unitData);
            unitData.items = new List<string>(9);
            unitData.cost = new List<int>(9);
            unitDataList[0].unitId = "DarkMage";
            unitDataList[0].fieldPos = 1;
            fieldController.SetEnemyUnit(unitDataList);
        }
    }

    private IEnumerator LoadEnemyUnits(int round)
    {
        string templ = GetRoundPath; 
        string path = (templ ?? "").Replace("{round}", round.ToString());
        string url = ApiSettings.Build(path);

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            www.disposeDownloadHandlerOnDispose = true;
            www.disposeUploadHandlerOnDispose = true;

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Received JSON Data: " + www.downloadHandler.text);
                EnemyUnitsResponse response = JsonUtility.FromJson<EnemyUnitsResponse>(www.downloadHandler.text);

                if (response.status == "success")
                {
                    fieldController.SetEnemyUnit(response.units);
                }
                else
                {
                    Debug.LogError("Error retrieving units: " + response.message);
                }
            }
            else
            {
                Debug.LogError("Error fetching enemy units: " + www.error);
            }
        }
    }

    // プレイヤーIDを取得する
    public IEnumerator RegisterPlayer()
    {
        if (gameController.useServerData == true)
        {
            yield return StartCoroutine(GetPlayerId());
        }
        else
        {
            gameController.playerId = 0;
            yield break;
        }
    }

    private IEnumerator GetPlayerId()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(IdUrl))
        {
            www.disposeDownloadHandlerOnDispose = true;
            www.disposeUploadHandlerOnDispose = true;

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string json = www.downloadHandler.text;
                PlayerIdResponse response = JsonUtility.FromJson<PlayerIdResponse>(json);
                playerId = response.player_id;
                gameController.playerId = playerId;
                Debug.Log("Player ID: " + playerId);
            }
            else
            {
                Debug.LogError("Error: " + www.error);
            }
        }
    }

    // バトル時のデータを送信
    public void SendRoundData(BattleRecord battleRecord)
    {
        if (gameController.useServerData == true)
        {
            StartCoroutine(UploadData(battleRecord));
        }
    }

    private IEnumerator UploadData(BattleRecord battleRecord)
    {
        string jsonData = JsonUtility.ToJson(battleRecord);
        Debug.Log("Sending JSON Data: " + jsonData);

        using (UnityWebRequest www = new UnityWebRequest(ResultUrl, "POST"))
        {
            www.disposeDownloadHandlerOnDispose = true;
            www.disposeUploadHandlerOnDispose = true;

            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();
            Debug.Log($"POST {ResultUrl} :: {jsonData}");
            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Data uploaded successfully: " + www.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Error uploading data: " + www.error);
            }
        }
    }

    // 状態ログ送信
    public void SendLog()
    {
        /*
        if (gameController.useServerData == true)
        {
            StateLog stateLog = GetLog();
            StartCoroutine(UploadLog(stateLog));
        }
        */
    }

    public StateLog GetLog()
    {
        StateLog stateLog = new StateLog();
        stateLog.battleRecord = battleController.SetBattleRecord();
        stateLog.boxItem = box.GetBoxItemString();
        stateLog.shopItem = reroll.GetShopItem();
        return stateLog;
    }

    private IEnumerator UploadLog(StateLog stateLog)
    {
        string jsonData = JsonUtility.ToJson(stateLog);

        using (UnityWebRequest request = new UnityWebRequest(LogUrl, "POST"))
        {
            request.disposeDownloadHandlerOnDispose = true;
            request.disposeUploadHandlerOnDispose = true;

            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Log sent successfully: " + request.downloadHandler.text);
                string responseText = request.downloadHandler.text;
                StateResponse response = JsonUtility.FromJson<StateResponse>(responseText);
                stateId = response.state_id;

                if (response.status == "success")
                {
                    Debug.Log("Received State ID: " + response.state_id);
                }
                else
                {
                    Debug.LogError("Failed to log state: " + responseText);
                }
            }
            else
            {
                Debug.LogError("Failed to send log: " + request.error);
            }
        }
    }

    // アクションログ送信
    public void SelectUnitLog(string unit1, string unit2)
    {
        ActionLog actionLog = new ActionLog();
        actionLog.playerId = gameController.playerId;
        actionLog.roundNumber = gameController.round;
        actionLog.stateId = stateId;
        actionLog.actionType = "SelectUnit";
        actionLog.unit1 = unit1;
        actionLog.unit2 = unit2;
        StartCoroutine(UploadActionLog(actionLog));
    }

    public void BuyItemLog(string item)
    {
        ActionLog actionLog = new ActionLog();
        actionLog.playerId = gameController.playerId;
        actionLog.roundNumber = gameController.round;
        actionLog.stateId = stateId;
        actionLog.actionType = "BuyItem";
        actionLog.item1 = item;
        StartCoroutine(UploadActionLog(actionLog));
    }

    public void RerollLog()
    {
        ActionLog actionLog = new ActionLog();
        actionLog.playerId = gameController.playerId;
        actionLog.roundNumber = gameController.round;
        actionLog.stateId = stateId;
        actionLog.actionType = "Reroll";
        StartCoroutine(UploadActionLog(actionLog));
    }

    public void SellItemLog(string item)
    {
        ActionLog actionLog = new ActionLog();
        actionLog.playerId = gameController.playerId;
        actionLog.roundNumber = gameController.round;
        actionLog.stateId = stateId;
        actionLog.actionType = "SellItem";
        actionLog.item1 = item;
        StartCoroutine(UploadActionLog(actionLog));
    }

    public void EquipItemLog(string item, string unit)
    {
        ActionLog actionLog = new ActionLog();
        actionLog.playerId = gameController.playerId;
        actionLog.roundNumber = gameController.round;
        actionLog.stateId = stateId;
        actionLog.actionType = "EquipItem";
        actionLog.item1 = item;
        actionLog.unit1 = unit;
        StartCoroutine(UploadActionLog(actionLog));
    }

    public void SwapItemLog(string item1, string item2, string unit)
    {
        ActionLog actionLog = new ActionLog();
        actionLog.playerId = gameController.playerId;
        actionLog.roundNumber = gameController.round;
        actionLog.stateId = stateId;
        actionLog.actionType = "SwapItem";
        actionLog.item1 = item1;
        actionLog.item2 = item2;
        actionLog.unit1 = unit;
        StartCoroutine(UploadActionLog(actionLog));
    }

    public void PosUnitLog(string unit, int position1, int position2)
    {
        ActionLog actionLog = new ActionLog();
        actionLog.playerId = gameController.playerId;
        actionLog.roundNumber = gameController.round;
        actionLog.stateId = stateId;
        actionLog.actionType = "PosUnit";
        actionLog.unit1 = unit;
        actionLog.position1 = position1;
        actionLog.position2 = position2;
        StartCoroutine(UploadActionLog(actionLog));
    }

    public void SwapUnitLog(string unit1, int position1, string unit2, int position2)
    {
        ActionLog actionLog = new ActionLog();
        actionLog.playerId = gameController.playerId;
        actionLog.roundNumber = gameController.round;
        actionLog.stateId = stateId;
        actionLog.actionType = "SwapUnit";
        actionLog.unit1 = unit1;
        actionLog.unit2 = unit2;
        actionLog.position1 = position1;
        actionLog.position2 = position2;
        StartCoroutine(UploadActionLog(actionLog));
    }

    private IEnumerator UploadActionLog(ActionLog actionLog)
    {
        if (gameController.useServerData == true)
        {
            string jsonData = JsonUtility.ToJson(actionLog);

            using (UnityWebRequest request = new UnityWebRequest(ActionLogUrl, "POST"))
            {
                request.disposeDownloadHandlerOnDispose = true;
                request.disposeUploadHandlerOnDispose = true;

                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("Log sent successfully: " + request.downloadHandler.text);
                }
                else
                {
                    Debug.LogError("Failed to send log: " + request.error);
                    Debug.LogError("Response: " + request.downloadHandler.text);
                }
            }
            SendLog();
        }
    }
}
