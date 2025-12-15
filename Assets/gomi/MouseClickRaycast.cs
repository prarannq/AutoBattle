using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseClickRaycast : MonoBehaviour
{
    int layerMask;
    // Start is called before the first frame update
    void Start()
    {
        layerMask = LayerMask.GetMask("Field");
    }
    /*
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 左クリック
        {
            //Debug.Log("mouse!!!!!!!!!!!");
            //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //RaycastHit hit;
            // 特定のレイヤーを無視する場合、レイヤーマスクを設定
            //int layerMask = LayerMask.GetMask("LayerToIgnore"); // 無視するレイヤーを指定

            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, ~layerMask);
            Debug.Log(hit);
            // 無視するレイヤーを反転して Raycast に渡す (~layerMask)
            if (hit.collider != null)
            {
                Debug.Log("HIt!!!!!!!!!!!");
                if (hit.collider.tag == "Unit")
                {
                    Debug.Log("UNit!!!!!!!!!!!");
                    UnitDragDrop unit = hit.collider.GetComponent<UnitDragDrop>();
                    unit.HitMouseDown();
                }else if (hit.collider.tag == "BoxItem" || hit.collider.tag == "EquipItem" || hit.collider.tag == "ShopItem")
                {
                    Debug.Log("item!!!!!!!!!!!");
                    Item item = hit.collider.GetComponent<Item>();
                    item.HitMouseDown();
                }
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            Debug.Log("Up!!!!!!!!!!!");
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, ~layerMask);
            Debug.Log(hit.collider);
            // 無視するレイヤーを反転して Raycast に渡す (~layerMask)
            if (hit.collider != null)
            {
                Debug.Log("HIt!!!!!!!!!!!");
                if (hit.collider.tag == "Unit")
                {
                    Debug.Log("UNit!!!!!!!!!!!");
                    UnitDragDrop unit = hit.collider.GetComponent<UnitDragDrop>();
                    unit.HitMouseUp();
                }
                else if (hit.collider.tag == "BoxItem" || hit.collider.tag == "EquipItem" || hit.collider.tag == "ShopItem")
                {
                    Debug.Log("item!!!!!!!!!!!");
                    Item item = hit.collider.GetComponent<Item>();
                    item.HitMouseUp();
                }
            }
        }
    }
    */
}
