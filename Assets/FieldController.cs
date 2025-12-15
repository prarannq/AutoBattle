using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldController : MonoBehaviour
{
    public UnitDataManager unitDataManager;
    public ServerController serverController;
    public List<GameObject> fieldList = new List<GameObject>(6);
    public List<GameObject> enemyList = new List<GameObject>(6);
    public List<GameObject> unitList = new List<GameObject>(12);

    public List<GameObject> cost1List;
    public List<GameObject> cost2List;
    public List<GameObject> cost3List;
    public List<GameObject> cost4List;
    public List<GameObject> cost5List;
    public List<GameObject> cost6List;
    public List<GameObject> cost7List;
    public List<GameObject> cost8List;
    public List<GameObject> cost9List;

    private Dictionary<string, GameObject> itemDictionary
        = new Dictionary<string, GameObject>();

    void Awake()
    {
        // cost1List ~ cost9List を走査し、すべてのアイテムを辞書に登録
        AddItemsToDictionary(cost1List);
        AddItemsToDictionary(cost2List);
        AddItemsToDictionary(cost3List);
        AddItemsToDictionary(cost4List);
        AddItemsToDictionary(cost5List);
        AddItemsToDictionary(cost6List);
        AddItemsToDictionary(cost7List);
        AddItemsToDictionary(cost8List);
        AddItemsToDictionary(cost9List);
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SwitchCollider(bool handle)
    {
        for (int i = 0; fieldList.Count > i; i++){
            fieldList[i].GetComponent<Collider2D>().enabled = handle;
        }
    }

    public void SetFieldUnit(int unitIndex1, int unitIndex2)
    {
        int unitIndex;
        for(int j = 0; j < 2; j++)
        {
            if(j == 0)
            {
                unitIndex = unitIndex1;
            }
            else
            {
                unitIndex = unitIndex2;
            }
            
            for (int i = 0; fieldList.Count > i; i++)
            {
                if (fieldList[i].transform.childCount == 0)
                {
                    Vector3 position = fieldList[i].transform.position + new Vector3(+1.0f, -0.45f, 0);
                    GameObject unit = Instantiate(unitList[unitIndex], position, Quaternion.identity);
                    unit.transform.SetParent(fieldList[i].transform, worldPositionStays: true);
                    break;
                }
            }
        }

        //Log用
        Unit unit1 = unitList[unitIndex1].GetComponent<Unit>();
        Unit unit2 = unitList[unitIndex2].GetComponent<Unit>();
        serverController.SelectUnitLog(unit1.unitId, unit2.unitId);
    }

    public void SetEnemyUnit(List<UnitData> units)
    {
        for (int i = 0; units.Count > i; i++)
        {
            GameObject u;
            switch (units[i].unitId)
            {
                case "King":
                    u = unitList[0];
                    break;
                case "Hero":
                    u = unitList[1];
                    break;
                case "Knight":
                    u = unitList[2];
                    break;
                case "Skeleton":
                    u = unitList[3];
                    break;
                case "Warrior":
                    u = unitList[4];
                    break;
                case "Goblin":
                    u = unitList[5];
                    break;
                case "Assassin":
                    u = unitList[6];
                    break;
                case "Samurai":
                    u = unitList[7];
                    break;
                case "Witch":
                    u = unitList[8];
                    break;
                case "BloodMage":
                    u = unitList[9];
                    break;
                case "DarkMage":
                    u = unitList[10];
                    break;
                case "FireMage":
                    u = unitList[11];
                    break;
                default:
                    Debug.LogError($"No matching unit found for {units[i].unitId}.");
                    u = null;
                    break;
            }


            Vector3 position = enemyList[units[i].fieldPos].transform.position + new Vector3(+1.0f, -0.45f, 0);
            GameObject unitObj = Instantiate(u, position, Quaternion.identity);
            unitObj.transform.SetParent(enemyList[units[i].fieldPos].transform, worldPositionStays: true);

            // Item
            for (int j = 0; units[i].items.Count > j; j++)
            {
                string itemId = units[i].items[j];
                if (itemDictionary.TryGetValue(itemId, out GameObject itemPrefab))
                {
                    // (B) アイテムをInstantiateしてユニットの子要素に
                    GameObject itemObj = Instantiate(
                        itemPrefab, unitObj.transform.position, Quaternion.identity);

                    itemObj.transform.SetParent(unitObj.transform);

                    Item item = itemObj.GetComponent<Item>();
                    Unit unit = unitObj.GetComponent<Unit>();
                    item.transform.SetParent(unit.equipList[j].transform);
                    item.transform.position = unit.equipList[j].transform.position;
                    //Debug.Log($"Created item {itemId} on {units[i].unitId}");
                }
                else
                {
                    Debug.LogWarning($"Item ID '{itemId}' not found in dictionary!");
                }
                //  生成


            }

        }
    }

    private void AddItemsToDictionary(List<GameObject> itemList)
    {
        foreach (var itemPrefab in itemList)
        {
            if (itemPrefab == null) continue;
            // プレハブの名前(GameObject.name)をkeyとする
            string itemName = itemPrefab.name;
            if (!itemDictionary.ContainsKey(itemName))
            {
                itemDictionary.Add(itemName, itemPrefab);
            }
            else
            {
                Debug.LogWarning(
                  $"Item {itemName} is duplicated in dictionary! (Skipping)");
            }
        }
    }

}
