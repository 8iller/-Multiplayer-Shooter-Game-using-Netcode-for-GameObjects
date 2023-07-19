using QFSW.QC;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LobbyManager
{
    private const string KEY_RELAY_JOIN_CODE = "RelayJoinCode";
    public static event EventHandler OnCreateLobbyStarted;
    public class OnLobbyListChangedEventArgs : EventArgs
    {
        public List<Lobby> lobbyList;
    }
    public static string relayJoinCode;
    private static Lobby joinedLobby;
    private static float heartbeatTimer;
    private static float listLobbiesTimer;

    public static async void InitializeUnityAuthentication()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            InitializationOptions initializationOptions = new InitializationOptions();
            initializationOptions.SetProfile(UnityEngine.Random.Range(0, 10000).ToString());

            await UnityServices.InitializeAsync(initializationOptions);

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    public static void HandlePeriodicListLobbies(float deltaTime)
    {
        if (joinedLobby == null &&
            UnityServices.State == ServicesInitializationState.Initialized &&
            AuthenticationService.Instance.IsSignedIn)
        {
            listLobbiesTimer -= deltaTime;
            if (listLobbiesTimer <= 0f)
            {
                float listLobbiesTimerMax = 120f;
                listLobbiesTimer = listLobbiesTimerMax;

            }
        }
    }
    public static void HandleHeartbeat(float deltaTime)
    {
        if (IsLobbyHost())
        {
            heartbeatTimer -= deltaTime;
            if (heartbeatTimer <= 0f)
            {
                float heartbeatTimerMax = 240f;
                heartbeatTimer = heartbeatTimerMax;

                LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
            }
        }
    }

    private static bool IsLobbyHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    [Command]
    public static async void ListLobbies(lobbyui lobbyui)
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                }
            };
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryLobbiesOptions);

            Debug.Log("Lobbies found: " + queryResponse.Results.Count);
            foreach (Lobby lobby in queryResponse.Results)
            {
                Debug.Log(lobby.Name + " " + lobby.Id);
            }

            lobbyui.UpdateLobbyList(queryResponse.Results); // Call UpdateLobbyList from lobbyui script
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private static async Task<Allocation> AllocateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);

            return allocation;
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);

            return default;
        }
    }

    private static async Task<string> GetRelayJoinCode(Allocation allocation)
    {
        try
        {
            relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            return relayJoinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            return default;
        }
    }

    private static async Task<JoinAllocation> JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            return joinAllocation;
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            return default;
        }
    }

    public static async void JoinRelayViaCode(string joinCode)
    {
        try
        {   
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            relayJoinCode = joinCode;
            SceneManager.sceneLoaded += (scene, mode) =>
            {
                if (scene.name == "GameScene")
                {
                    NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));
                    NetworkManager.Singleton.StartClient();
                    //Debug.Log("creating the lobby: " + joinedLobby.Id);
                }
            };

            SceneManager.LoadSceneAsync("GameScene");

            


        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            
        }


    }

   

    public static async void CreateLobby(string lobbyName)
    {   
        
        OnCreateLobbyStarted?.Invoke(null, EventArgs.Empty);
        try
        {
            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, 4, new CreateLobbyOptions
            {

            });

            Allocation allocation = await AllocateRelay();

            string relayJoin = await GetRelayJoinCode(allocation);


            await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                     { KEY_RELAY_JOIN_CODE , new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
                 }
            });

            SceneManager.sceneLoaded += (scene, mode) =>
            {
                if (scene.name == "GameScene")
                {
                    NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));
                    NetworkManager.Singleton.StartHost();
                    Debug.Log("creating the lobby: " + joinedLobby.Id);

                }
            };

            SceneManager.LoadSceneAsync("GameScene");
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);

        }
    }

    [Command]
    public static async void JoinLobbyWithCode(string lobbyCode)
    {

        try
        {
            joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyCode); 
            string relayJoin = joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;
            JoinAllocation joinAllocation = await JoinRelay(relayJoin);
            relayJoinCode = relayJoin;
            SceneManager.sceneLoaded += (scene, mode) =>
            {
                if (scene.name == "GameScene")
                {   
                    
                    NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));
                    NetworkManager.Singleton.StartClient();
                    Debug.Log("creating the lobby: " + joinedLobby.Id);
                    // Perform any additional actions or reset variables here
                    // ...
                }
            };
            SceneManager.LoadScene("GameScene");

        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);

        }
    }

    public static async void LeaveLobby()
    {   
       
        if (joinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);

                joinedLobby = null;

                NetworkManager.Singleton.Shutdown();
                SceneManager.LoadScene("Menu");
                Debug.Log("Left");

            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }
}
