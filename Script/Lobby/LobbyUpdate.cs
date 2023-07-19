using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyUpdate : MonoBehaviour
{
    // Start is called before the first frame update
     private void Awake() {
        DontDestroyOnLoad(gameObject);

    }

    void Update()
    {
        LobbyManager.HandleHeartbeat(Time.deltaTime);
        LobbyManager.HandlePeriodicListLobbies(Time.deltaTime);
    }
}
