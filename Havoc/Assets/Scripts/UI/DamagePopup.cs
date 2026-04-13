using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    public TMP_Text textMesh;
    private float disappearTimer = 1.5f;
    private Color textColor;
    private Vector3 moveVector;
    private bool floatUpward;

    public void Setup(string text, Color color, bool isStatus = false)
    {
        if (textMesh == null) textMesh = GetComponent<TMP_Text>();

        textMesh.text = text;
        textMesh.color = color;
        textColor = textMesh.color;
        floatUpward = isStatus;

        if (isStatus)
        {
            moveVector = new Vector3(Random.Range(-0.5f, 0.5f), 7f, 0f);
        }
        else
        {
            moveVector = new Vector3(Random.Range(-1f, 1f), 3f, 0f);
        }
    }

    private void Update()
    {
        transform.position += moveVector * Time.deltaTime;

        if (!floatUpward)
        {
            moveVector -= new Vector3(0, 25f, 0) * Time.deltaTime;
        }

        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0)
        {
            float fadeSpeed = 3f;
            textColor.a -= fadeSpeed * Time.deltaTime;
            textMesh.color = textColor;
            if (textColor.a <= 0) Destroy(gameObject);
        }
    }
}