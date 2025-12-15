using UnityEngine;
using TMPro;

public class TooltipController : MonoBehaviour
{
    public TextMeshProUGUI tooltipText;         // or TMP_Text
    public GameObject panel;         // tooltipのPanel本体

    public static TooltipController Instance;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void ShowTooltip(string message, Vector3 pos)
    {
        tooltipText.text = message;
        panel.SetActive(true);
        // panelの位置をマウス付近にしたい場合は
        // panel.transform.position = pos;
    }

    public void HideTooltip()
    {
        panel.SetActive(false);
    }
}