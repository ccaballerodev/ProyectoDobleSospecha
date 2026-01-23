using Unity.Netcode;
using UnityEngine;

public class PlayerCamera : NetworkBehaviour
{
    public Camera playerCamera;

    void Start()
    {
        if (!IsOwner)
        {
            playerCamera.enabled = false;
            playerCamera.GetComponent<AudioListener>().enabled = false;
        }
    }
}
