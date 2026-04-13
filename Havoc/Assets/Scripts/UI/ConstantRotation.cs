using UnityEngine;

public class ConstantRotation : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 90f;

    public Vector3 rotationAxis = Vector3.forward;

    public bool isRotating = true;

    void Update()
    {
        if (!isRotating) return;

        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
    }
}