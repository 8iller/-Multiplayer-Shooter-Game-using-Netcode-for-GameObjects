using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class NetworkManagerUI : MonoBehaviour
{
    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();

    }
    public void StartClint()
    {
        NetworkManager.Singleton.StartClient();

    }
    public void StartHost()
    {

        NetworkManager.Singleton.StartHost();
    }
    public void Leave()
    {
        LobbyManager.LeaveLobby();
        

    }
}
