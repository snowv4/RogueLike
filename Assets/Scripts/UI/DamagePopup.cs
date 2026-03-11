using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    [Header("Use um dos dois (TMP tem prioridade)")]
    public TMP_Text tmpText;
    public TextMesh textMesh;

    public float lifetime = 0.6f;
    public float floatSpeed = 1.5f;

    Color startColor = Color.red;
    Color endColor = new Color(1f, 0f, 0f, 0f); // vermelho transparente
    float timer;

    void Awake()
    {
        if (tmpText == null) tmpText = GetComponentInChildren<TMP_Text>();
        if (textMesh == null) textMesh = GetComponentInChildren<TextMesh>();
    }

    public void Setup(int damageAmount)
    {
        if (tmpText == null) tmpText = GetComponentInChildren<TMP_Text>();
        if (textMesh == null) textMesh = GetComponentInChildren<TextMesh>();

        string damageStr = damageAmount.ToString();

        if (tmpText != null)
        {
            tmpText.text = damageStr;
            tmpText.color = startColor;
        }
        else if (textMesh != null)
        {
            textMesh.text = damageStr;
            textMesh.color = startColor;
        }
        else return;

        timer = 0f;
    }

    void Update()
    {
        timer += Time.deltaTime;

        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        float t = Mathf.Clamp01(timer / lifetime);
        Color currentColor = Color.Lerp(startColor, endColor, t);

        if (tmpText != null)
            tmpText.color = currentColor;
        else if (textMesh != null)
            textMesh.color = currentColor;

        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}

