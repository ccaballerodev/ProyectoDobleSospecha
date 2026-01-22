using UnityEngine;
using Unity.Netcode;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using System.Threading.Tasks;
using Unity.Services.Multiplayer;

public class RelayManager : MonoBehaviour
{
    async void Start()
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void Host()
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        Debug.Log("JOIN CODE: " + joinCode);

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        
        transport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls")); 

        NetworkManager.Singleton.StartHost();
    }

    public async void Client(string joinCode)
    {
        JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));

        NetworkManager.Singleton.StartClient();
    }
}