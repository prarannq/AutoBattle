using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BattleButton : MonoBehaviour
{
    public GameController gameController;
    


    // Start is called before the first frame update
    void Start()
    {
        
    }


    void OnMouseDown()
    {
        BattleStart();
    }

    public void BattleStart()
    {
        gameController.ChangePhase(GamePhase.Battle);
    }
}
