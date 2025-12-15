using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class ActionLogListWrapper
{
    public List<ActionLog> actionLogs;

    public ActionLogListWrapper(List<ActionLog> logs)
    {
        actionLogs = logs;
    }
}

[System.Serializable]
public class StateAndActionLog
{
    public StateLog stateLog;
    public List<ActionLog> actionLogs;
}

public class CPU : MonoBehaviour
{
    public GameController gameController;
    public BattleController battleController;
    public ServerController serverController;
    //public UnitShop unitShop;
    public List<UnitShopButton> unitShopList;
    public GameObject box;
    public BattleButton battleButton;
    public BattleEndButton battleEndButton;
    public Reroll rerollButton;
    public List<Shop> shopList;

    public List<GameObject> fieldList = new List<GameObject>(6);
    List<Unit> unitList = new List<Unit>(6);
    public List<Item> boxItemList = new List<Item>(15);
    List<ActionLog> actionChoiseList = new List<ActionLog>();
    ActionLog actionLog = new ActionLog();

    private int gold;
    private int round;

    // Start is called before the first frame update
    void Start()
    {

    }
    // Update is called once per frame
    void Update()
    {

    }




    /// <summary>
    /// 機械学習によるプレイ
    /// </summary>
    /// 
    /*
     * 
    private void GetState()
    {
        //gold round
        gold = gameController.gold;
        round = gameController.round;

        //現在のUnit
        List<Unit> unitList = new List<Unit>(6);
        for (int i = 0; i < fieldList.Count; i++)
        {
            foreach (Transform child in fieldList[i].transform)
            {
                if (child.CompareTag("Unit"))
                {
                    Unit unit = child.GetComponent<Unit>();
                    unitList.Add(unit);
                }
            }
        }
        //Unitが所持するItem(未実装)


        //Boxが所持するItem
        boxItemList = new List<Item>(15);
        foreach (Transform child in box.transform)
        {
            if (child.CompareTag("BoxItem"))
            {
                Item item = child.GetComponent<Item>();
                boxItemList.Add(item);
            }
        }
    }
    // GamePhaseを確認して次の操作を行う
    public IEnumerator ChangeAiPhase(GamePhase phase)
    {
        GetState();

        switch (phase)
        {
            case GamePhase.Init:
                break;

            case GamePhase.SelectUnit:
                yield return new WaitForSeconds(1);

                actionChoiseList = new List<ActionLog>();
                // Unitの選択肢を取得
                for (int i = 0; i < unitShopList.Count; i++)
                {
                    actionLog = new ActionLog();
                    actionLog.actionType = "SelectUnit";

                    foreach (Transform child in unitShopList[i].transform)
                    {
                        if (child.tag == "Unit")
                        {
                            UnitVisual unit = child.GetComponent<UnitVisual>();
                            if (actionLog.unit1 == null)
                            {
                                actionLog.unit1 = unit.unitId;
                            }
                            else
                            {
                                actionLog.unit2 = unit.unitId;
                            }
                        }
                    }
                    actionChoiseList.Add(actionLog);
                }
                //通信
                StartCoroutine(UploadLog(actionChoiseList));

                break;

            case GamePhase.Build:
                yield return new WaitForSeconds(1);

                actionChoiseList = new List<ActionLog>();
                // Boxにあるitemを売る選択肢
                for (int i = 0; i < boxItemList.Count; i++)
                {
                    actionLog = new ActionLog();
                    actionLog.actionType = "SellItem";
                    Item item = boxItemList[i].GetComponent<Item>();
                    actionLog.item1 = item.itemId;
                    actionChoiseList.Add(actionLog);
                }
                // 通信
                if (actionChoiseList.Count != 0)
                {
                    yield return UploadLog(actionChoiseList);
                }

                for (int j = 0; j < 15; j++)
                {
                    GetState();
                    actionChoiseList = new List<ActionLog>();
                    // Shopにあるitemを買う選択肢
                    for (int i = 0; i < shopList.Count; i++)
                    {
                        foreach (Transform child in shopList[i].transform)
                        {
                            if (child.CompareTag("ShopItem"))
                            {
                                Item item = child.GetComponent<Item>();

                                actionLog = new ActionLog();
                                actionLog.actionType = "BuyItem";
                                actionLog.item1 = item.itemId;
                                actionChoiseList.Add(actionLog);
                            }
                        }
                    }
                    // Rerollをする選択肢
                    actionLog = new ActionLog();
                    actionLog.actionType = "Reroll";
                    actionChoiseList.Add(actionLog);
                    // 通信
                    yield return UploadLog(actionChoiseList);
                }

                //////// ビルド



                //////// バトル開始
                //battleButton.BattleStart();
                break;
            case GamePhase.Battle:
                // 待つ(バトル中は操作できない)
                break;
            case GamePhase.BattleEnd:
                //次へ進む
                battleEndButton.BattleEnd();
                break;
            case GamePhase.GameEnd:
                // ゲーム終了処理
                break;
        }
    }


    private IEnumerator UploadLog(List<ActionLog> actionChoiseList)
    {
        // 現在の状態を取得
        StateAndActionLog stateAndActionLog = new StateAndActionLog();
        stateAndActionLog.actionLogs = actionChoiseList;
        stateAndActionLog.stateLog = serverController.GetLog();
        string uploadLog = JsonUtility.ToJson(stateAndActionLog);
        Debug.Log(uploadLog);
        using (UnityWebRequest request = new UnityWebRequest("http://localhost:5000/predict", "POST"))
        {

            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(uploadLog);

            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Log sent successfully: " + request.downloadHandler.text);

                // ② 受け取ったJSONを C#クラスに変換
                string responseJson = request.downloadHandler.text;
                BestActionResponse bestActionResp = JsonUtility.FromJson<BestActionResponse>(responseJson);

                // ③ AiAction(...) に渡す
                yield return AiAction(bestActionResp);
            }
            else
            {
                Debug.LogError("Failed to send log: " + request.error);
                Debug.LogError("Response: " + request.downloadHandler.text);
            }
        }
    }

    public IEnumerator AiAction(BestActionResponse bestActionResp)
    {
        switch (bestActionResp.bestAction.actionType)
        {

            case "SelectUnit":
                unitShopList[bestActionResp.bestActionIndex].SelectUnit();
                break;
            case "SellItem":
                boxItemList[bestActionResp.bestActionIndex].SellItem();
                break;
            case "BuyItem":
                for (int i = 0; i < shopList.Count; i++)
                {
                    foreach (Transform child in shopList[i].transform)
                    {
                        Item item = child.GetComponent<Item>();
                        if (item.itemId == bestActionResp.bestAction.item1)
                        {
                            item.BuyItem();
                        }
                    }
                }
                break;
            case "Reroll":
                rerollButton.RerollItem(2);
                break;
                
            case :
                break;
            case :
                break;
            case :
                break;
                
        }
        yield break;
    }

    */
}
