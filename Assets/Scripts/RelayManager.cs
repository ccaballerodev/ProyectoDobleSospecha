using UnityEngine;
using Unity.Netcode;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using System.Threading.Tasks;
using TMPro; // Necesario para el texto

public class RelayManager : MonoBehaviour
{
    public static string joinCodeGame;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI joinCodeText; // Arrastra tu texto de UI aquí

    async void Start()
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        // Ocultar el texto al iniciar
        if (joinCodeText != null) joinCodeText.gameObject.SetActive(false);

        // Suscribirse al evento de conexión
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnDestroy()
    {
        // Limpieza de eventos
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
            Debug.Log("JOIN CODE: " + joinCode);

            // Mostrar el código en la UI
            if (joinCodeText != null)
            {
                joinCodeText.text = "Código de Sala: " + joinCode;
                joinCodeText.gameObject.SetActive(true);
            }

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));

            NetworkManager.Singleton.StartHost();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
        }
    }

    public async void Client(string joinCode)
    {
        try
        {
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));

            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        // Si somos el Host y se conecta alguien más (el cliente suele ser ID > 0)
        // O simplemente si el conteo de clientes llega a 2
        if (NetworkManager.Singleton.IsHost && NetworkManager.Singleton.ConnectedClients.Count >= 2)
        {
            if (joinCodeText != null)
            {
                joinCodeText.gameObject.SetActive(false);
                Debug.Log("Jugador conectado: Ocultando código de unión");
            }
        }
    }
}