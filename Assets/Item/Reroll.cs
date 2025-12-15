using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// クリックするとショップのアイテムが変わる
// 変更内容はショップ側で処理する

public class Reroll : MonoBehaviour
{
    public List<List<GameObject>> costList;
    public GameController gameController;
    private ServerController serverController;
    private int round;
    private int rerollCost = 2;

    public List<GameObject> cost1List;
    public List<GameObject> cost2List;
    public List<GameObject> cost3List;
    public List<GameObject> cost4List;
    public List<GameObject> cost5List;
    public List<GameObject> cost6List;
    public List<GameObject> cost7List;
    public List<GameObject> cost8List;
    public List<GameObject> cost9List;

    public List<Shop> shopList;

    void Start()
    {
        gameController = GetComponentInParent<GameController>();
        serverController = GetComponentInParent<ServerController>();
        costList = new List<List<GameObject>> { cost1List, cost2List, cost3List, cost4List, cost5List, cost6List, cost7List, cost8List, cost9List };
    }

    // Reroll ボタンがクリックされたときに呼ばれる
    void OnMouseDown()
    {
        RerollItem(rerollCost);
    }

    public void RerollItem(int cost)
    {
        round = gameController.round;
        if (gameController.gold >= cost)
        {
            gameController.ChangeGold(-cost);

            foreach (Transform child in transform)
            {
                Shop shop = child.GetComponent<Shop>();
                if (shop != null)
                {
                    GameObject newItem = GetRandomItem();
                    if (newItem != null)
                    {
                        shop.UpdateItem(newItem);
                    }
                    else
                    {
                        Debug.LogWarning("No new item available to update.");
                    }
                }
            }
            serverController.RerollLog();
        }
    }

    private int GetWeightedRandomIndex(float[] weights)
    {
        // 重みの合計を求める
        float totalWeight = 0f;
        for (int i = 0; i < weights.Length; i++)
        {
            totalWeight += weights[i];
        }

        // 0~totalWeight までの乱数
        float rand = Random.Range(0f, totalWeight);

        // rand を順に引いていき、どのコストに当たるかを判定
        for (int i = 0; i < weights.Length; i++)
        {
            if (rand < weights[i])
            {
                return i;
            }
            rand -= weights[i];
        }
        // 万が一全て超えたら最後を返す
        return weights.Length - 1;
    }

    public GameObject GetRandomItem()
    {
        // (1) ラウンドに応じた cost=1~9 重み配列
        float[] weights = GetCostWeights(round);

        // (2) 重み付きランダムで costIndex を求める
        int costIndex = GetWeightedRandomIndex(weights);
        // costIndex は 0..8 (cost1List-cost9List のインデックス)

        // costList[costIndex] を取得
        if (costIndex < 0 || costIndex >= costList.Count)
        {
            Debug.LogWarning("Invalid costIndex: " + costIndex);
            return null;
        }
        List<GameObject> itemList = costList[costIndex];

        if (itemList.Count == 0)
        {
            Debug.LogWarning("Item list is empty for costIndex=" + costIndex);
            return null;
        }

        // (3) その costList からランダムに1つ
        int randomIndex = Random.Range(0, itemList.Count);
        return itemList[randomIndex];
    }

    // ラウンドを受け取り、cost=1..9 の出現重みを返す
    private float[] GetCostWeights(int round)
    {
        // 9要素 (cost1~9の重み)
        float[] weights = new float[9];

        // 例: round 1~3 は主に 1~3コスト → 大きい重み
        //     4~5コスト 少し
        //     6~9コスト 超少ない
        // roundが大きいほど高コストの重みを上げる

        if (round <= 1)
        {
            // round1用: [1,2,3コストメイン]
            weights[0] = 50f; // cost1
            weights[1] = 30f; // cost2
            weights[2] = 15f; // cost3
            weights[3] = 3f;  // cost4
            weights[4] = 1f;  // cost5
            weights[5] = 0.5f;// cost6
            weights[6] = 0.3f;// cost7
            weights[7] = 0.2f;// cost8
            weights[8] = 0.1f;// cost9
        }
        else if (round <= 3)
        {
            // round2~3
            weights[0] = 30f; // cost1
            weights[1] = 25f; // cost2
            weights[2] = 20f; // cost3
            weights[3] = 10f; // cost4
            weights[4] = 5f;  // cost5
            weights[5] = 3f;  // cost6
            weights[6] = 2f;  // cost7
            weights[7] = 1f;  // cost8
            weights[8] = 0.5f;// cost9
        }
        else if (round <= 5)
        {
            // round4~5
            weights[0] = 10f;
            weights[1] = 15f;
            weights[2] = 15f;
            weights[3] = 15f;
            weights[4] = 10f;
            weights[5] = 10f;
            weights[6] = 8f;
            weights[7] = 5f;
            weights[8] = 2f; // 9コスト少し
        }
        else
        {
            // round6 以降: 高コストもそこそこ出る
            weights[0] = 5f;
            weights[1] = 8f;
            weights[2] = 10f;
            weights[3] = 10f;
            weights[4] = 15f;
            weights[5] = 15f;
            weights[6] = 10f;
            weights[7] = 8f;
            weights[8] = 5f;
        }

        return weights;
    }

    public List<string> GetShopItem()
    {
        List<string> shopItemList = new List<string>(5);
        for (int i = 0; i < shopList.Count; i++)
        {
            if(shopList[i].itemName != null)
            {
                shopItemList.Add(shopList[i].itemName);
            }
            
            /*
            foreach (Transform child in shopList[i].transform)
            {
                if (child.CompareTag("ShopItem"))
                {
                    Item item = child.GetComponent<Item>();
                    shopItemList.Add(item.itemId);
                }
            }
            */
        }
        Debug.Log(shopItemList);
        return shopItemList;
    }
}
