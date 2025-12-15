using UnityEngine;
using UnityEngine.UI; // UI関連のクラスを使用

public class ToggleButton : MonoBehaviour
{
    public Toggle toggle; // Inspectorで割り当てるToggle
    public GameObject targetObject; // 操作対象のオブジェクト

    public GameController gameController;
    public string ButtonName;

    void Start()
    {
        // Toggleの状態が変更されたときに呼び出すイベントを設定
        toggle.onValueChanged.AddListener(OnToggleChanged);

        // 初期状態を設定
        targetObject.SetActive(toggle.isOn);
    }

    // Toggleの状態が変更されたときに呼ばれる関数
    void OnToggleChanged(bool isOn)
    {
        targetObject.SetActive(isOn);
        Debug.Log("Toggle state: " + isOn);

        if(ButtonName == "ai")
        {
            gameController.useAi = isOn;
        }
        else if (ButtonName == "serverEnemy")
        {
            gameController.useServerEnemy = isOn;
        }
        else if (ButtonName == "serverData")
        {
            gameController.useServerData = isOn;
        }
    }

    void OnDestroy()
    {
        // メモリリーク防止のため、イベントリスナーを解除
        toggle.onValueChanged.RemoveListener(OnToggleChanged);
    }
}
