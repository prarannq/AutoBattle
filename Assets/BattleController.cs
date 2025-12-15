using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class BattleController : MonoBehaviour
{
    public GameController gameController;
    public ServerController serverController;
    public GameObject canvas;
    public GameObject enemyPrefab;
    public List<GameObject> fieldList = new List<GameObject>(12);

    public List<GameObject> battleUnitList = new List<GameObject>(12);

    public TargetingManager targetingManager;
    public EffectManager effectManager;
    public EventManager eventManager;

    private BattleRecord currentRecord;

    private const int PLAYER_COUNT = 6;
    private const int ENEMY_COUNT = 6;

    void Start()
    {
        // 初期化
        serverController = GetComponentInParent<ServerController>();
        eventManager = GetComponent<EventManager>();
        effectManager = GetComponent<EffectManager>();
        targetingManager = GetComponent<TargetingManager>();
    }

    public IEnumerator StartBattle()
    {
        serverController.SetEnemy(gameController.round);

        yield return new WaitForSeconds(1);

        //SetEnemyUnits();
        SetPlayerUnits();
        //targetingManager.SetTargetingManager(battleUnitList);
        // イベントやエフェクトの初期化
        eventManager.Initialize();
        effectManager.Initialize(targetingManager);

        

        for (int i = 0; i < battleUnitList.Count; i++)
        {
            if (battleUnitList[i] != null)
            {
                UnitDragDrop dad = battleUnitList[i].GetComponent<UnitDragDrop>();
                if (dad != null)
                {
                    dad.enabled = false;
                }
            }

        }
        targetingManager.SetTargetingManager(battleUnitList);

        yield return new WaitForSeconds(2);
        // 各ユニットにEventManager, EffectManager, TargetingManagerの参照を渡したい場合、
        // Unit側でBattleControllerからもらうなどして設定する。
        foreach (GameObject unitObj in battleUnitList)
        {
            if (unitObj != null)
            {
                Unit u = unitObj.GetComponent<Unit>();
                u.InitializeBattle(eventManager, effectManager, targetingManager);
            }
        }
        OnBattleSetup();
    }

    public void SetPlayerUnits()
    {
        battleUnitList = new List<GameObject>(new GameObject[fieldList.Count]);
        // フィールド上のUnitを取得
        for (int i = 0; i < fieldList.Count; i++)
        {
            foreach (Transform child in fieldList[i].transform)
            {
                if (child.CompareTag("Unit"))
                {
                    battleUnitList[i] = child.gameObject;
                    Unit unit = child.GetComponent<Unit>();
                    // バトル用初期化はInitializeBattleで後ほど設定
                    unit.SetFieldNum(i);
                }
            }
        }
    }

    private void SetEnemyUnits()
    {

        // 仮：敵を3体配置
        Instantiate(enemyPrefab, fieldList[6].transform.position, Quaternion.identity, fieldList[6].transform);
        //Instantiate(enemyPrefab, fieldList[7].transform.position, Quaternion.identity, fieldList[7].transform);
        //Instantiate(enemyPrefab, fieldList[8].transform.position, Quaternion.identity, fieldList[8].transform);

    }

    public void DieUnit(int fieldIndex)
    {
        battleUnitList[fieldIndex] = null;
        CheckBattleEnd();
    }

    private void CheckBattleEnd()
    {
        // プレイヤー全滅
        bool playerDefeated = true;
        for (int i = 0; i < PLAYER_COUNT; i++)
        {
            if (battleUnitList[i] != null)
            {
                playerDefeated = false;
                break;
            }
        }

        if (playerDefeated)
        {
            EndBattle(false);
            gameController.PlayerResult(false);
            return;
        }

        // 敵全滅
        bool enemyDefeated = true;
        for (int i = PLAYER_COUNT; i < PLAYER_COUNT + ENEMY_COUNT; i++)
        {
            if (battleUnitList[i] != null)
            {
                enemyDefeated = false;
                break;
            }
        }
        if (enemyDefeated)
        {
            EndBattle(true);
            gameController.PlayerResult(true);
        }
    }

    private void EndBattle(bool playerWin)
    {
        currentRecord.result = playerWin;
        serverController.SendRoundData(currentRecord);

        gameController.ChangePhase(GamePhase.BattleEnd);
        foreach (var u in battleUnitList)
        {
            if (u != null)
            {
                Unit unit = u.GetComponent<Unit>();
                unit.StopAllActions();
            }
        }
    }

    public void RoundEnd()
    {
        // StatusBar削除
        if (canvas != null)
        {
            foreach (Transform child in canvas.transform)
            {
                if(child.gameObject.CompareTag("HpMp"))
                {
                    Destroy(child.gameObject);
                }
                
            }
        }

        // 敵削除
        for (int i = 6; i < fieldList.Count; i++)
        {
            foreach (Transform child in fieldList[i].transform)
            {
                Destroy(child.gameObject);
            }
        }

        // 味方ユニット再有効化
        for (int i = 0; i < fieldList.Count; i++)
        {
            if (fieldList[i] != null)
            {
                foreach (Transform child in fieldList[i].transform)
                {
                    child.gameObject.SetActive(true);
                }
            }
        }

        // ステータスリセット & DragDrop有効化
        for (int i = 0; i < fieldList.Count; i++)
        {
            if (fieldList[i] != null)
            {
                foreach (Transform child in fieldList[i].transform)
                {
                    if (child.CompareTag("Unit"))
                    {
                        Unit unit = child.GetComponent<Unit>();
                        unit.InitializeFromData();
                        UnitDragDrop d = child.GetComponent<UnitDragDrop>();
                        if (d != null) d.enabled = true;
                        // アイテムのコリジョン有効化
                        unit.BattleEnd();
                    }
                }
            }
        }

        eventManager.ResetAllListeners();
    }

    public void OnBattleSetup()
    {
        // currentRecordを作成
        currentRecord = SetBattleRecord();
    }


    public BattleRecord SetBattleRecord()
    {
        List<Unit> units = new List<Unit>(6);

        for (int i = 0; i < 6; i++)
        {
            if(battleUnitList.Count > 0)
            {
                if (battleUnitList[i] != null)
                {
                    Unit u = battleUnitList[i].GetComponent<Unit>();
                    units.Add(u);
                }
            }
            
        }

        BattleRecord battleRecord = new BattleRecord();

        battleRecord.playerId = gameController.playerId;
        battleRecord.roundNumber = gameController.round;
        battleRecord.playerHp = gameController.playerHp;
        battleRecord.gold = gameController.gold;
        battleRecord.result = false; // まだ結果わからない

        battleRecord.units = new List<UnitData>();

        // ユニット配置情報をcurrentRecordに詰める
        foreach (Unit u in units)
        {

            if(units.Count > 0)
            {
                u.SetItem();
                UnitData ud = new UnitData();
                ud.unitId = u.unitId;
                ud.fieldPos = u.field;
                ud.items = new List<string>();
                ud.cost = new List<int>();
                foreach (var item in u.itemList)
                {
                    if (item != null) ud.items.Add(item.itemId); // itemIdを格納
                    if (item != null) ud.cost.Add(item.cost); //costを格納
                }
                battleRecord.units.Add(ud);
            }
            
        }

        return battleRecord;
    }
}
