using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ショップの１マスごとの処理

public class Shop : MonoBehaviour
{
    public GameController gameController;
    public Box box;
    public string itemName;

    public void UpdateItem(GameObject newItem)
    {
        // 既存の子オブジェクトを全て削除します
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        //Debug.Log(newItem);
        Item item = newItem.GetComponent<Item>();
        itemName = item.itemId;
        // 新しいアイテムを生成し、Shopの子として設定します。
        Instantiate(newItem, transform.position, Quaternion.identity, transform);
    }

    public bool Buy(int cost)
    {
        if (box.SearchSpace() != null)
        {
            if (gameController.Buy(cost)) 
            {
                itemName = null;
                return true;
            }
            
        }
        return false;
    }


}