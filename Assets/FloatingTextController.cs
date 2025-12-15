using UnityEngine;
using UnityEngine.UI; // or TMPro;
using TMPro;

public class FloatingTextController : MonoBehaviour
{
    public TMP_Text text; // or TMP_Text text;
    public float moveSpeed = 0.1f;
    public float lifetime = 1.0f;
    private float elapsed = 0f;

    void Start()
    {
        
    }

    void Update()
    {
        // ŠÈˆÕ“I‚Éã•ûŒü‚ÉˆÚ“®
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
        elapsed += Time.deltaTime;
        if (elapsed >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    public void SetText(string message)
    {
        text = GetComponent<TMP_Text>();
        text.text = message;
    }
}
