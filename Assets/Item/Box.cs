using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour
{
    public List<GameObject> boxList;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObject SearchSpace()
    {
        for(int i=0; i < boxList.Count; i++)
        {
            GameObject storage = boxList[i];
            Storage storageScript = storage.GetComponent<Storage>();

            if (storageScript.CheckStorage() == true)
            {
                return storage;
            }
        }
        return null;
    }

    public List<string> GetBoxItemString()
    {
        List<Item> boxItemList = new List<Item>(15);
        List<string> boxItemStrList = new List<string>(15);
        for (int i = 0; i < boxItemList.Count; i++)
        {
            boxItemList = GetBoxItemList();
            boxItemStrList.Add(boxItemList[i].itemId);
        }
        return boxItemStrList;
    }

    public List<Item> GetBoxItemList()
    {
        List<Item> boxItemList = new List<Item>(15);
        for (int i = 0; i < boxList.Count; i++)
        {
            foreach (Transform child in boxList[i].transform)
            {
                if (child.CompareTag("BoxItem"))
                {
                    Item item = child.GetComponent<Item>();
                    boxItemList.Add(item);
                }
            }
        }
        return boxItemList;
    }
}
