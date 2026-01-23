using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RelayManager : MonoBehaviour
{
    public static string joinCodeGame;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI joinCodeText;

    async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

            if (joinCodeText != null) joinCodeText.gameObject.SetActive(false);

            // --- ESTO ES LO QUE TE FALTA PARA DESBLOQUEAR AL CLIENTE ---
            NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;

            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
        catch (Exception e) { Debug.LogError("Error en Inicialización: " + e.Message); }
    }

    // Esta función le da permiso al cliente para entrar
    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // Aceptamos la conexión
        response.Approved = true;
        response.CreatePlayerObject = true;
        // Importante: No dejar pendiente para que la escena cargue de inmediato
        response.Pending = false;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    public async void Host()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(2);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            joinCodeGame = joinCode;

            Debug.Log("Codigo de unión generado: " + joinCode);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));

            if (NetworkManager.Singleton.StartHost())
            {
                // El Host cambia de escena y arrastra a los clientes
                NetworkManager.Singleton.SceneManager.LoadScene("InitScene", LoadSceneMode.Single);
            }
        }
        catch (Exception e) { Debug.LogError(e); }
    }

    public async void Client(string joinCode)
    {
        try
        {
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));

            // Iniciamos el cliente
            if (!NetworkManager.Singleton.StartClient())
            {
                Debug.LogError("El NetworkManager no pudo arrancar.");
            }
            else
            {
                Debug.Log("<color=cyan>Cliente iniciado, esperando aprobación del Host...</color>");
            }
        }
        catch (Exception e) { Debug.LogError($"Error al unirse: {e.Message}"); }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"<color=green>¡CONECTADO! ID: {clientId}</color>");

        if (NetworkManager.Singleton.IsHost && NetworkManager.Singleton.ConnectedClients.Count >= 2)
        {
            if (joinCodeText != null) joinCodeText.gameObject.SetActive(false);
        }
    }
}