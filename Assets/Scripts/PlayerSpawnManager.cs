using Unity.Netcode;
using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    public Transform hostSpawn;
    public Transform clientSpawn;

    private void Start()
    {
        // Nos suscribimos al proceso de aprobación de conexión
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
        }
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // 1. Aceptamos la conexión
        response.Approved = true;
        response.CreatePlayerObject = true;

        // 2. Determinamos la posición según quién se conecta
        // El Host siempre es el primero en conectar (id 0)
        if (NetworkManager.Singleton.ConnectedClientsIds.Count == 0)
        {
            response.Position = hostSpawn.position;
            response.Rotation = hostSpawn.rotation;
            Debug.Log("Posición de Host asignada");
        }
        else
        {
            response.Position = clientSpawn.position;
            response.Rotation = clientSpawn.rotation;
            Debug.Log("Posición de Cliente asignada");
        }

        // 3. Importante: Si tienes un prefab específico, puedes asignarlo aquí
        response.PlayerPrefabHash = null;
    }
}