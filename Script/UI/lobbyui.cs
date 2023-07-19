using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;

public class lobbyui : MonoBehaviour
{
    [SerializeField] GameObject MainMenu;
    [SerializeField] GameObject Settings;
    [SerializeField] GameObject LobbyPanel;
    [SerializeField] GameObject CreateMenu;
    [SerializeField] TMP_InputField lobbyNameInputField;
    [SerializeField] Button lobbyButtonPrefab;
    [SerializeField] Transform Content;
    [SerializeField] GameObject LoadingUI;
    [SerializeField] Button JoinRelayButton;
    [SerializeField]TMP_InputField RelayCode; 


    void Awake()
    {
        LobbyManager.InitializeUnityAuthentication();
    }
    public void OnSettings()
    {
        MainMenu.SetActive(false);
        Settings.SetActive(true);
    }

    public void SettingsBack()
    {
        Settings.SetActive(false);
        MainMenu.SetActive(true);
    }

    public void JoinLobby()
    {
        MainMenu.SetActive(false);
        LobbyPanel.SetActive(true);
        LobbyManager.ListLobbies(this);
    }

    public void refresh()
    {
        LobbyManager.ListLobbies(this);
    }

    public void LobbyBack()
    {
        LobbyPanel.SetActive(false);
        MainMenu.SetActive(true);
    }

    public void CreateUI()
    {
        MainMenu.SetActive(false);
        CreateMenu.SetActive(true);
    }

    public void CreateUIBack()
    {
        CreateMenu.SetActive(false);
        MainMenu.SetActive(true);
    }

    public void CreateLobby()
    {
        string lobbyName = lobbyNameInputField.text;
        if (string.IsNullOrEmpty(lobbyName)) return;
        CreateMenu.SetActive(false);
        LobbyManager.CreateLobby(lobbyName);
        LoadingUI.SetActive(true);
    }

    public void UpdateLobbyList(List<Lobby> lobbies)
    {
        // Clear existing lobby names
        foreach (Transform child in Content)
        {
            Destroy(child.gameObject);
        }

        // Add lobby names to the scrollable view
        foreach (Lobby lobby in lobbies)
        {   
            Button lobbyButton = Instantiate(lobbyButtonPrefab, Content);
            lobbyButton.GetComponentInChildren<TextMeshProUGUI>().text = lobby.Name;
            lobbyButton.onClick.AddListener(() => JoinLobbyByName(lobby.Id)); // Add a listener to join the lobby when the button is clicked
            
        }


    }

    private void JoinLobbyByName(string lobbycode)
    {   
        LobbyPanel.SetActive(false);
        LobbyManager.JoinLobbyWithCode(lobbycode);
        LoadingUI.SetActive(true);

    }

    public void JoinRelayViaCode()
    {

        string Code = RelayCode.text;
        if (!string.IsNullOrEmpty(Code))
        {
            LobbyManager.JoinRelayViaCode(Code);
            LoadingUI.SetActive(true);
            

        }



    }

}
