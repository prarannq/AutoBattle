using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public enum GamePhase
{
    Init = 1,
    SelectUnit = 2,
    Build = 3,
    Battle = 4,
    BattleEnd = 5,
    GameEnd = 6
}

public class GameController : MonoBehaviour
{
    public GameObject battleObject;
    public GameObject selectUnitObject;
    public GameObject buildObject;
    public GameObject battleEndObject;
    public GameObject startObject;
    public GameObject startUi;
    public BattleController battleController;
    public ServerController serverController;
    public UnitShop unitShop;
    public AiController aiController;
    public Reroll reroll;

    public List<GameObject> numberList = new List<GameObject>(10);
    public List<GameObject> healthList = new List<GameObject>(3);
    public int playerId;
    public int playerHp = 3;
    public int gold = 0;
    private int initGold = 50;
    public int round = 1;
    public GamePhase currentPhase = GamePhase.SelectUnit;

    public GameObject roundNum;
    public List<GameObject> goldNum = new List<GameObject>(3);

    public bool useAi;
    public bool useServerEnemy;
    public bool useServerData;

    void Start()
    {
        ChangeGold(initGold);
        aiController = GetComponent<AiController>();
        serverController = GetComponent<ServerController>();
        
        useAi = false;
        useServerEnemy = true;
        useServerData = true;
    }

    public bool Buy(int cost)
    {
        if (gold >= cost)
        {
            ChangeGold(-cost);
            return true;
        }
        return false;
    }

    public void ChangePhase(GamePhase phase)
    {
        currentPhase = phase;
        switch (phase)
        {
            case GamePhase.Init:
                break;
            case GamePhase.SelectUnit:
                ShowSelectUnitPhase();
                serverController.SendLog();
                break;
            case GamePhase.Build:
                battleController.SetPlayerUnits();
                reroll.RerollItem(0);
                ChangeGold(round * 5);
                ShowBuildPhase();
                
                if (round > 0 && round < 10)
                {
                    foreach (Transform child in roundNum.transform)
                    {
                        Destroy(child.gameObject);
                    }
                    Instantiate(numberList[round], roundNum.transform.position, Quaternion.identity, roundNum.transform);
                }
                break;
            case GamePhase.Battle:
                ShowBattlePhase();
                
                // バトル開始処理
                StartCoroutine(battleController.StartBattle());
                break;
            case GamePhase.BattleEnd:
                ShowBattleEndPhase();
                break;
            case GamePhase.GameEnd:
                // ゲーム終了処理
                break;
        }
        /*
        if(useAi == true)
        {
            StartCoroutine(aiController.ChangeCpuPhase(phase));
        }
        */
    }

    public void ChangeRound()
    {
        battleController.RoundEnd();
        round++;
        // ラウンド間ゴールド付与など
        if (round == 1 || round == 4 || round == 7)
        {
            ChangePhase(GamePhase.SelectUnit);
        }
        else
        {
            ChangePhase(GamePhase.Build);
        }
    }

    private void ShowSelectUnitPhase()
    {
        selectUnitObject.SetActive(true);
        buildObject.SetActive(false);
        battleObject.SetActive(false);
        battleEndObject.SetActive(false);
        startObject.SetActive(false);
        startUi.SetActive(false);
        unitShop.UpdateUnitShop();
    }

    private void ShowBuildPhase()
    {
        buildObject.SetActive(true);
        selectUnitObject.SetActive(false);
        battleObject.SetActive(false);
        battleEndObject.SetActive(false);
    }

    private void ShowBattlePhase()
    {
        battleObject.SetActive(true);
        selectUnitObject.SetActive(false);
        buildObject.SetActive(false);
        battleEndObject.SetActive(false);
    }

    private void ShowBattleEndPhase()
    {
        battleEndObject.SetActive(true);
        battleObject.SetActive(true);
        selectUnitObject.SetActive(false);
        buildObject.SetActive(false);
    }

    public void PlayerResult(bool result)
    {
        BattleEndButton button = battleEndObject.GetComponent<BattleEndButton>();
        if (round != 9)
        {
            
            if (result)
            {
                //勝ち
                button.SetResult("WIN");
            }
            else
            {
                //負け
                playerHp--;
                Destroy(healthList[playerHp]);

                if (playerHp < 1)
                {
                    // ゲーム終了
                    ChangePhase(GamePhase.GameEnd);
                    button.SetResult("END");
                }
                else
                {
                    button.SetResult("LOSE");
                }
            }
        }
        else
        {
            // ゲーム終了
            ChangePhase(GamePhase.GameEnd);
            button.SetResult("END");
        }
        
        
    }

    public void ChangeGold(int cost)
    {
        gold += cost;
        for(int i = 0;i < goldNum.Count; i++)
        {
            foreach (Transform child in goldNum[i].transform)
            {
                Destroy(child.gameObject);
            }
        }

        Instantiate(numberList[gold % 10], goldNum[0].transform.position, Quaternion.identity, goldNum[0].transform);
        Instantiate(numberList[gold % 100 / 10], goldNum[1].transform.position, Quaternion.identity, goldNum[1].transform);
        Instantiate(numberList[gold / 100], goldNum[2].transform.position, Quaternion.identity, goldNum[2].transform);
    }
}
