using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGame : MonoBehaviour
{
    public GameController gameController;
    public CameraSwitcher cameraSwitcher;
    public ServerController serverController;

    private bool isPlaying;
    // Start is called before the first frame update
    void Start()
    {
        isPlaying = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnMouseDown()
    {
        // Ç∑Ç≈Ç…çƒê∂íÜÇ»ÇÁâΩÇ‡ÇµÇ»Ç¢
        if (isPlaying) return;

        StartCoroutine(Play());
    }

    public IEnumerator Play()
    {
    isPlaying = true;

    yield return serverController.RegisterPlayer();
        cameraSwitcher.StartGame();
        gameController.ChangePhase(GamePhase.SelectUnit);
    }
}
