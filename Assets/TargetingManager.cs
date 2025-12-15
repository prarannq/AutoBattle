using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TargetingManager : MonoBehaviour
{
    public List<Unit> battleUnitList = new List<Unit>(new Unit[12]);

    public void SetTargetingManager(List<GameObject> unitList)
    {
        battleUnitList = new List<Unit>(new Unit[12]);
        for (int i = 0; i < unitList.Count; i++)
        {
            if(unitList[i] != null)
            {
                battleUnitList[i] = unitList[i].GetComponent<Unit>();
            }
        }
    }

    public void DeleteUnit(int field)
    {
        battleUnitList[field] = null;
    }

    //指定した番号のfieldにいるUnitを取得する
    public Unit GetUnit(int field)
    {
        if (battleUnitList[field] != null) return battleUnitList[field];
        return null;
    }

    public Unit FrontUnit(int field)
    {
        // 簡易ロジック（例：敵側はfield>=6）
        bool isPlayer = (field < 6);
        bool isOdd = (field % 2) == 1;
        if (isPlayer)
        {
            if (isOdd)
            {
                for (int i = 6; i < 12; i = i + 2)
                {
                    if (battleUnitList[i+1] != null)
                    {
                        return battleUnitList[i+1];
                    }
                    else if (battleUnitList[i] != null)
                    {
                        return battleUnitList[i];
                    }
                    
                }
            }
            else
            {
                for (int i = 6; i < 12; i++)
                {
                    if (battleUnitList[i] != null)
                    {
                        return battleUnitList[i];
                    }
                        
                }
            }

        }
        else
        {
            if (isOdd)
            {
                for (int i = 0; i < 6; i = i+2)
                {
                    if (battleUnitList[i + 1] != null)
                    {
                        return battleUnitList[i + 1];
                    }
                    else if (battleUnitList[i] != null)
                    {
                        return battleUnitList[i];
                    }
                    
                }
            }
            else
            {
                for (int i = 0; i < 6; i++)
                {
                    if (battleUnitList[i] != null)
                    {
                        return battleUnitList[i];
                    }
                        
                }
            }
        }
        return null;
    }


    public List<Unit> GetFrontlineEnemies(int field)
    {
        List<Unit> targetList = new List<Unit>(2);
        if (FrontUnit(field) == null) return null;
        targetList.Add(FrontUnit(field));
        if (targetList[0].field % 2 == 1)
        {
            if (GetUnit(targetList[0].field - 1) != null)
            {
                targetList.Add(FrontUnit(targetList[0].field - 1));
            }
        }
        else
        {
            if (GetUnit(targetList[0].field + 1) != null)
            {
                targetList.Add(GetUnit(targetList[0].field + 1));
            }
        }
        return targetList;
    }

    public List<Unit> GetFrontAlly(int field)
    {
        List<Unit> targetList = new List<Unit>(1);
        if(field < 6)
        {
            targetList.Add(FrontUnit(field % 2 + 6));
        }
        else
        {
            targetList.Add(FrontUnit(field % 2));
        }
        return targetList;
    }


    public List<Unit> GetAllAllies(int field)
    {
        List<Unit> targetList = new List<Unit>(6);

        bool isPlayer = (field < 6);
        if (isPlayer)
        {
            for (int i = 0; i < 6; i++)
            {
                if (battleUnitList[i] != null) targetList.Add(battleUnitList[i]);
            }
        }
        else
        {
            for (int i = 6; i < 12; i++)
            {
                if (battleUnitList[i] != null) targetList.Add(battleUnitList[i]);
            }
        }
        return targetList;
    }
    public List<Unit> GetAllEnemies(int field)
    {
        List<Unit> targetList = new List<Unit>(6);
        
        bool isPlayer = (field < 6);
        if (isPlayer)
        {
            for (int i = 6; i < 12; i++)
            {
                if (battleUnitList[i] != null) targetList.Add(battleUnitList[i]);
            }
        }
        else
        {
            for (int i = 0; i < 6; i++)
            {
                if (battleUnitList[i] != null) targetList.Add(battleUnitList[i]);
            }
        }
        return targetList;
    }


    public List<Unit> GetRandomAllies(int field, int count)
    {
        List<Unit> targetList = new List<Unit>(6);
        List<Unit> alliesList = GetAllAllies(field);
        alliesList = alliesList.OrderBy(x => Random.value).ToList();
        for (int i = 0; i < count; i++)
        {
            if(i < alliesList.Count)
            {
                if (alliesList[i] != null)
                {
                    targetList.Add(alliesList[i]);
                }
            }
        }
        return targetList;
    }
    public List<Unit> GetRandomEnemies(int field, int count)
    {
        List<Unit> targetList = new List<Unit>(6);
        List<Unit> enemiesList = GetAllEnemies(field);
        enemiesList = enemiesList.OrderBy(x => Random.value).ToList();
        for (int i = 0; i < count; i++)
        {
            if (i < enemiesList.Count)
            {
                if (enemiesList[i] != null)
                {
                    targetList.Add(enemiesList[i]);
                }

            }

        }
        return targetList;
    }


    public List<Unit> GetAllyBehind(int field)
    {
        List<Unit> targetList = new List<Unit>(1);
        if(field == 4 || field == 5 || field == 10 || field == 11)
        {
            return null;
        }
        else
        {
            if (GetUnit(field + 2) != null)
            {
                targetList.Add(GetUnit(field + 2));
            }
            else
            {
                return null;
            }
            
        }
        return targetList;
    }
    public List<Unit> GetEnemyBehindTarget(int field)
    {
        List<Unit> targetList = new List<Unit>(2);
        targetList[0] = FrontUnit(field);
        if (GetUnit(targetList[0].field + 2) != null)
        {
            targetList[1] = GetUnit(targetList[0].field + 2);
        }
        return targetList;
    }

}
