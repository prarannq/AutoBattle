using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitDragDrop : MonoBehaviour
{

    public GameObject draggedObject;
    private Vector3 startPosition;
    private Vector3 mousePosition;
    public FieldController fieldController;
    private ServerController serverController;
    private BattleController battleController;

    private int layerMask = 0;
     

    // Start is called before the first frame update
    void Start()
    {
        battleController = GetComponentInParent<BattleController>();
        fieldController = GetComponentInParent<FieldController>();
        serverController = GetComponentInParent<ServerController>();
    }
    void Update()
    {
        if (draggedObject != null)
        {
            mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            draggedObject.transform.position = new Vector3(mousePosition.x + 1.0f, mousePosition.y - 0.4f, startPosition.z);
        }

    }

    public void OnMouseDown()
    {
        fieldController.SwitchCollider(true);
        startPosition = transform.position;
        draggedObject = gameObject;
        GetComponent<Collider2D>().enabled = false;
    }

    public void OnMouseUp()
    {
        Collider2D[] hit = Physics2D.OverlapPointAll(mousePosition);
        
        GetComponent<Collider2D>().enabled = true;
        if (hit != null)
        {
            bool handled = false;
            foreach (Collider2D collider in hit)
            {
                GameObject targetObject = collider.gameObject;
                if (targetObject.tag == "UnitField")
                {
                    HandleDrop(targetObject);
                    handled = true;
                    break;
                }
            }
            if (!handled)
            {
                transform.position = startPosition; // UnitFieldが見つからなかった場合のみ元に戻す
            }
        }
        else
        {
            transform.position = startPosition; // Reset to original position
        }
        draggedObject = null;
        fieldController.SwitchCollider(false);
    }

    void HandleDrop(GameObject target)
    {
        GameObject childUnit = HasUnitTagChild(target);
        if (childUnit == this.gameObject)
        {
            // 何もしない
            transform.position = startPosition;
        }
        else if (childUnit == null)
        {
            //Log 
            Field field = target.GetComponent<Field>();

            Unit unit = GetComponent<Unit>();
            serverController.PosUnitLog(unit.unitId, unit.field, field.fieldNum);

            transform.SetParent(target.transform);
            transform.position = new Vector3(target.transform.position.x + 1.0f, target.transform.position.y - 0.4f, target.transform.position.z);

            battleController.SetPlayerUnits();
        }
        else if(childUnit != null)
        {
            SwapUnits(childUnit, target);
        }
        draggedObject = null;
    }

    void SwapUnits(GameObject existingUnit, GameObject target)
    {
        //Log
        Unit unit1 = GetComponent<Unit>();
        Unit unit2 = existingUnit.GetComponent<Unit>();
        serverController.SwapUnitLog(unit1.unitId, unit1.field, unit2.unitId, unit2.field);
        battleController.SetPlayerUnits();

        // Swap positions and parent-child relationship
        Transform originalParent = existingUnit.transform.parent;
        existingUnit.transform.SetParent(transform.parent);
        existingUnit.transform.position = startPosition;

        transform.SetParent(target.transform);
        transform.position = new Vector3(target.transform.position.x + 1.0f, target.transform.position.y - 0.4f, target.transform.position.z);

    }

    GameObject HasUnitTagChild(GameObject target)
    {
        // target の全ての子オブジェクトを取得
        foreach (Transform child in target.transform)
        {
            // 子オブジェクトのタグが "Unit" であるかどうかをチェック
            if (child.CompareTag("Unit"))
            {
                return child.gameObject; // "Unit" タグの子オブジェクトが見つかった場合
            }
        }
        return null; // "Unit" タグの子オブジェクトが見つからなかった場合
    }
}
