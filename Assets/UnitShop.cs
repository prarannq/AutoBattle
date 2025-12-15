using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UnitShop : MonoBehaviour
{
    public List<UnitShopButton> unitShopList;
    public FieldController fieldController;
    public GameController gameController;

    const int phaseBuild = 3;

    [SerializeField] List<bool> unitList = new List<bool>();
    [SerializeField] List<bool> copyUnitList = new List<bool>();
    // Start is called before the first frame update
    void Start()
    {
        UpdateUnitShop();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateUnitShop()
    {
        for (int i = 0; i < unitList.Count; i++)
        {
            copyUnitList[i] = unitList[i];
        }
        
        for (int i = 0; i < unitShopList.Count; i++)
        {
            int index1;
            int index2;
            // trueなインデックスをランダムに取得
            do
            {
                index1 = Random.Range(0, copyUnitList.Count);
            } while (!copyUnitList[index1]);
            copyUnitList[index1] = false;

            do
            {
                index2 = Random.Range(0, copyUnitList.Count);
            } while (!copyUnitList[index2]);
            copyUnitList[index2] = false;

            unitShopList[i].UpdateUnit(index1, index2);

            
        }
        
    }

    public void SetUnit(int unitIndex1, int unitIndex2)
    {
        
        unitList[unitIndex1] = false;
        unitList[unitIndex2] = false;

        fieldController.SetFieldUnit(unitIndex1, unitIndex2);
        

        gameController.ChangePhase(GamePhase.Build);
        
    }
}
