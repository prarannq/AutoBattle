using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDragDrop : MonoBehaviour
{
    private GameObject draggedObject;
    private Vector3 startPosition;

    void Update()
    {
        if (draggedObject != null)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            draggedObject.transform.position = new Vector3(mousePosition.x, mousePosition.y, startPosition.z);
        }
    }

    void OnMouseDown()
    {
        if(this.tag == "ShopItem")
        {

        }
        Debug.Log("Clicked,Item");

        startPosition = transform.position;
        draggedObject = gameObject;
        GetComponent<Collider2D>().enabled = false;
    }

    void OnMouseUp()
    {

        Debug.Log("Drop,Item");
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.zero);
        GetComponent<Collider2D>().enabled = true;
        if (hit.collider != null)
        {
            Debug.Log("Hit,Item");
            GameObject targetObject = hit.collider.gameObject;
            HandleDrop(targetObject);
        }
        else
        {
            transform.position = startPosition; // Reset to original position
        }
        draggedObject = null;
    }

    void HandleDrop(GameObject target)
    {
        string parentTag = transform.parent.tag;
        string targetTag = target.tag;

        Debug.Log(parentTag);
        Debug.Log(targetTag);
        Debug.Log(target);


        // Handle different cases based on parentTag and targetTag
        if (parentTag == "ShopItem" && targetTag == "BoxItem")
        {
            Debug.Log("Box,Item");
            if (target.transform.childCount == 0) // BoxItemに子オブジェクトがない
            {
                Debug.Log("BOX!!,Item");
                transform.SetParent(target.transform);
                transform.position = target.transform.position;
            }
        }
        else if (parentTag == "BoxItem" && targetTag == "BaseItem")
        {
            if (target.transform.childCount > 0) // BaseItemに子オブジェクトがある
            {
                GameObject existingItem = target.transform.GetChild(0).gameObject;
                SwapItems(existingItem, target);
            }
            else // BaseItemに子オブジェクトがない
            {
                transform.SetParent(target.transform);
                transform.position = target.transform.position;
            }
        }
        else if (parentTag == "BoxItem" && targetTag == "SellItem")
        {
            Destroy(gameObject); // Itemオブジェクトを削除
        }
        else if (parentTag == "BaseItem" && targetTag == "BoxItem")
        {
            if (target.transform.childCount > 0) // BoxItemに子オブジェクトがある
            {
                GameObject existingItem = target.transform.GetChild(0).gameObject;
                SwapItems(existingItem, target);
            }
            else // BoxItemに子オブジェクトがない
            {
                transform.SetParent(target.transform);
                transform.position = target.transform.position;
            }
        }
        else if (parentTag == "BaseItem" && targetTag == "SellItem")
        {
            Destroy(gameObject); // Itemオブジェクトを削除
        }
        else
        {
            transform.position = startPosition; // Reset to original position
        }
    }

    void SwapItems(GameObject existingItem, GameObject target)
    {
        // Swap positions and parent-child relationship
        Transform originalParent = existingItem.transform.parent;
        existingItem.transform.SetParent(transform.parent);
        existingItem.transform.position = transform.position;

        transform.SetParent(target.transform);
        transform.position = target.transform.position;

        originalParent.DetachChildren(); // Detach any children from the original parent
    }
}
