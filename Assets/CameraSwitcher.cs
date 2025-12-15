using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public Camera startCamera;
    public Camera gameCamera;

    private void Start()
    {
        // Start画面用カメラを有効化、ゲーム用カメラを無効化
        startCamera.enabled = true;
        gameCamera.enabled = false;
    }

    public void StartGame()
    {
        // カメラを切り替え
        startCamera.enabled = false;
        gameCamera.enabled = true;
    }
}
