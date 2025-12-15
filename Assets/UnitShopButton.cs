using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitShopButton : MonoBehaviour
{
    public List<GameObject> UnitList;
    public UnitShop unitShop;

    int unitIndex1;
    int unitIndex2;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnMouseDown()
    {
        SelectUnit();
    }

    public void SelectUnit()
    {
        unitShop.SetUnit(unitIndex1, unitIndex2);
    }

    public void UpdateUnit(int unitNum1, int unitNum2)
    {
        // 既存の子オブジェクトを全て削除します
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        unitIndex1 = unitNum1;
        unitIndex2 = unitNum2;

        // 現在の位置から左右に配置
        Vector3 position1 = transform.position + new Vector3(-0.7f, -0.7f, 0); // 左に1ユニット
        Vector3 position2 = transform.position + new Vector3(0.7f, -0.7f, 0);  // 右に1ユニット

        // 新しいアイテムを生成し、Shopの子として設定します。
        // オブジェクトを生成（親を設定しない）
        GameObject unit1 = Instantiate(UnitList[unitNum1], position1, Quaternion.identity);
        GameObject unit2 = Instantiate(UnitList[unitNum2], position2, Quaternion.identity);

        // 必要であれば、親を設定しつつスケールを固定
        unit1.transform.SetParent(transform, worldPositionStays: true);
        unit2.transform.SetParent(transform, worldPositionStays: true);
    }

}
