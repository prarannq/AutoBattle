using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WinRateButton : MonoBehaviour
{
    public AiController aiController;
    private float winRate;

    // Tooltip用のUI Text(またはTextMeshPro)をアサインしておく
    public TextMeshProUGUI tooltipText;
    // TooltipのPanel（あるいはGameObject自体）
    public GameObject tooltipPanel;

    void Start()
    {
        // 最初はTooltipを非表示にしておく
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnMouseDown()
    {
        StartCoroutine(StartWintate());
        
    }
    
    private IEnumerator StartWintate()
    {
        yield return aiController.GetWinRate();
        winRate = aiController.aiWinRate;
        if (tooltipText != null)
        {
            tooltipText.text = "Winning rate prediction using machine learning: " + winRate;
        }
    }

    // カーソルがオブジェクトに乗ったとき
    private void OnMouseEnter()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(true);
        }
        if (tooltipText != null)
        {
            tooltipText.text = "Winning rate prediction using machine learning: " + winRate;
        }
    }

    // カーソルがオブジェクトから外れたとき
    private void OnMouseExit()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }
}
