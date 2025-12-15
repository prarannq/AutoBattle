using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDataManager : MonoBehaviour
{
    public static ItemDataManager Instance;
    private Dictionary<string, ItemDefinition> itemDict = new Dictionary<string, ItemDefinition>();

    void Awake()
    {
        if (Instance == null) { Instance = this;}

        LoadItemsData();
    }

    void LoadItemsData()
    {
        TextAsset jsonText = Resources.Load<TextAsset>("ItemsData");
        ItemDataCollection data = JsonUtility.FromJson<ItemDataCollection>(jsonText.text);
        foreach (var it in data.items)
        {
            itemDict[it.id] = it;
        }
    }

    public ItemDefinition GetItemDefinition(string id)
    {
        if (itemDict.TryGetValue(id, out var def))
        {
            return def;
        }
        return null;
    }
}
