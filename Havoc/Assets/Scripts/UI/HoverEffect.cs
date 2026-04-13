using UnityEngine;

public class HoverEffect : MonoBehaviour
{
    public float speed = 5f;
    public float amount = 0.15f; 
    private Vector3 startPos;

    void OnEnable()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        transform.localPosition = startPos + new Vector3(Mathf.Sin(Time.time * speed) * amount, 0, 0);
    }
}