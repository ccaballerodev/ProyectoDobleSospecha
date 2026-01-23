using UnityEngine;

public class FastRotate : MonoBehaviour
{
    [Header("Velocidad de rotación en grados por segundo")]
    public Vector3 rotationSpeed = new Vector3(0f, 360f, 0f); // rota sobre Y por defecto

    void Update()
    {
        // Rota el objeto cada frame según rotationSpeed
        transform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);
    }
}