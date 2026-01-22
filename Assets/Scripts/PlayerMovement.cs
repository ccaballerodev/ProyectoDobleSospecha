using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    void Update()
    {
        if (!IsOwner) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        transform.Translate(new Vector3(h, 0, v) * 5f * Time.deltaTime);
    }
}
