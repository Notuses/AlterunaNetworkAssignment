using System;
using System.Collections.Generic;
using Alteruna;
using Alteruna.Trinity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Application = UnityEngine.Application;

public class UiManager : AttributesSync
{
    [SerializeField] private GameObject _uiCanvas;
    [SerializeField] private GameObject _mainMenu;
    [SerializeField] private List<GameObject> _alterunaSearch;
    [SerializeField] private GameObject _lobby;
    
    // Lobby references
    [SerializeField] private Button _startGameButton;
    [SerializeField] private TextMeshProUGUI _lobbyHeader;
    [SerializeField] private GameObject[] _playerPanels;
    
    private static UiManager _instance;
    
    private GameManager.State _currentState;
    
    public static UiManager Instance
    {
        get
        {
            if (_instance == null)
            {
                throw new NullReferenceException("_instance is null!");
            }
            return _instance;
        }
    }
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    private void Start()
    {
        Multiplayer.RegisterRemoteProcedure("UpdateLobbyUiRemote", UpdateLobbyUiRemote);
    }
    
    public void ShowMainMenu(bool active)
    {
        _mainMenu.SetActive(active);
    }
    
    public void ShowLobby(bool active)
    {
        _lobby.SetActive(active);
        if (active)
            UpdateLobbyUi();
    }

    public void ShowAlterunaSearch(bool active)
    {
        foreach (var go in _alterunaSearch)
            go.SetActive(active);
    }
    
    public void ToggleUiOn()
    {
        _uiCanvas.SetActive(true);
    }    
    
    public void ToggleUiOff()
    {
        _uiCanvas.SetActive(false);
    }
    
    public void SetLobbyName()
    {
        _lobbyHeader.text = Multiplayer.Instance.CurrentRoom.Name +"'s Room";
    }

    public void ShowPanel(GameObject panel)
    {
        panel.SetActive(true);
    }

    public void HidePanel(GameObject panel)
    {
        panel.SetActive(false);
    }

    public void QuitApplication()
    {
        Application.Quit();
    }
    
    public void LeaveRoom()
    {
        Multiplayer.Instance.CurrentRoom.Leave();
    }

    public void CanStart(bool canStart)
    {
        _startGameButton.interactable = canStart;
    }
    
    public void UpdateLobbyUi()
    {
        var userCount = GameManager.Instance.GetUserListInRoom().Count;
        if (userCount > 1)
            Multiplayer.InvokeRemoteProcedure("UpdateLobbyUiRemote", UserId.AllInclusive);
        else
            UpdateLobbyUiLocal();
    }
    
    private void UpdateLobbyUiLocal()
    {
        _playerPanels[Multiplayer.Instance.Me.Index].GetComponentInChildren<TextMeshProUGUI>().text = Multiplayer.Instance.Me.Name;
        _playerPanels[Multiplayer.Instance.Me.Index].SetActive(true);
    }
    
    private void UpdateLobbyUiRemote(ushort fromUser, ProcedureParameters parameters, uint callId, ITransportStreamReader processor)
    {
        foreach (var user in GameManager.Instance.GetUserListInRoom())
        {
            _playerPanels[user.Index].GetComponentInChildren<TextMeshProUGUI>().text = user.Name;
            _playerPanels[user.Index].SetActive(true);
        }
    }
}
