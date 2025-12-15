using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleEndButton : MonoBehaviour
{
    public GameController gameController;
    const int phaseBattleEnd = 5;
    public List<GameObject> resultList;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnMouseDown()
    {
        BattleEnd();
    }

    public void BattleEnd()
    {
        if(gameController.currentPhase != GamePhase.GameEnd)
        {
            gameController.ChangeRound();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        
    }

    public void SetResult(string result)
    {
        if(result == "WIN")
        {
            resultList[0].SetActive(true);
            resultList[1].SetActive(false);
            resultList[2].SetActive(false);
        }
        else if (result == "LOSE")
        {
            resultList[0].SetActive(false);
            resultList[1].SetActive(true);
            resultList[2].SetActive(false);
        }
        else if (result == "END")
        {
            resultList[0].SetActive(false);
            resultList[1].SetActive(false);
            resultList[2].SetActive(true);
        }
    }
}
