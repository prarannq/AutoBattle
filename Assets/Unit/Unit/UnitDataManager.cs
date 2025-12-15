using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class UnitDataManager : MonoBehaviour
{
    public static UnitDataManager Instance;
    private Dictionary<string, UnitDefinition> unitDict = new Dictionary<string, UnitDefinition>();

    void Awake()
    {
        if (Instance == null) { Instance = this;}

        LoadUnitsData();
    }

    void LoadUnitsData()
    {
        TextAsset jsonText = Resources.Load<TextAsset>("UnitsData");
        // UnitsData.json‚ðAssets/Resources/‚É“ü‚ê‚Ä‚¨‚­
        UnitDataCollection data = JsonUtility.FromJson<UnitDataCollection>(jsonText.text);

        foreach (var u in data.units)
        {
            unitDict[u.id] = u;
        }
    }

    public UnitDefinition GetUnitDefinition(string id)
    {
        return unitDict[id];
    }
}
