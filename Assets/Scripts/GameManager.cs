using System;
using System.Collections.Generic;
using Alteruna;
using Alteruna.Trinity;
using Unity.VisualScripting;
using UnityEngine;
using Avatar = Alteruna.Avatar;

public class GameManager : AttributesSync
{
    [SerializeField] private bool _showDebugLogs = true;

    [Header("Game Settings")]
    [SerializeField] private int _minUsersToStart = 2;

    private Multiplayer _multiplayer;
    private static GameManager _instance;
    public static GameManager Instance
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
    
    private List<User> _users;

    private State _state;
    public enum State
    {
        Idle,
        LookingForPlayers,
        PrepareRound,
        StartRound,
        FinishRound,
        Restart
    }

    private GameState _currentState;
    private readonly IdleGameState _idleGameState = new IdleGameState();
    private readonly LookingForPlayerGameState _lookingForPlayerGameState = new LookingForPlayerGameState();
    private readonly PrepareRoundGameState _prepareRoundGameState = new PrepareRoundGameState();
    private readonly StartRoundGameState _startRoundGameState = new StartRoundGameState();
    private readonly FinishRoundGameState _finishRoundGameState = new FinishRoundGameState();
    private readonly RestartGameState _restartGameState = new RestartGameState();
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        _multiplayer = Multiplayer.Instance;
    }

    void Start()
    {
        Multiplayer.RegisterRemoteProcedure("ChangeMyStateProcedure", ChangeMyStateProcedure);
        _currentState = _idleGameState;
        ChangeState(State.Idle);
    }
    
    public void CallChangeMyState(State state)
    {
        byte stateIndex = (byte)state;
        ProcedureParameters parameters = new ProcedureParameters();
        parameters.Set("stateIndex", stateIndex);
        Multiplayer.InvokeRemoteProcedure("ChangeMyStateProcedure", UserId.All, parameters);
        ChangeState(stateIndex);
    }
    
    public void ChangeMyStateProcedure(ushort fromUser, ProcedureParameters parameters, uint callId, ITransportStreamReader processor)
    {
#if UNITY_EDITOR
        PrintDebug("GameManager - ", "RPC TO CHANGE STATE");
#endif
        byte stateIndex = (byte)parameters.Get("stateIndex", (byte)0);
        ChangeState(stateIndex);
    }
    
    private void Update()
    {
        _currentState.Update();
    }

    public void StartRound()
    {
        if (CheckIfEnoughPlayers() && CheckIfEveryoneSameState(State.PrepareRound))
        {
#if UNITY_EDITOR
            PrintDebug("GameManager - ", "STARTING GAME");
#endif
            CallChangeMyState(State.StartRound);
        }
        else
        {
#if UNITY_EDITOR
            PrintDebug("GameManager - ", "UNABLE TO START GAME");
#endif
        }
        UiManager.Instance.CanStart(false);
    }
    public void UpdateUsersInRoomList()
    {
        _users = _multiplayer.GetUsers();

#if UNITY_EDITOR
        PrintDebug("GameManager - Users in room: ", _users.Count);
#endif
    }

    public List<User> GetUserListInRoom()
    {
        UpdateUsersInRoomList();

        return _users;
    }
    
    public int AmountOfPlayersInRoom()
    {
        UpdateUsersInRoomList();
        int amount = _users.Count;
        return amount;
    }
    
    public bool CheckIfEnoughPlayers()
    {
        int amountOfPlayersInRoom = AmountOfPlayersInRoom();
        bool enoughPlayers = amountOfPlayersInRoom >= _minUsersToStart;
        
#if UNITY_EDITOR
        PrintDebug("GameManager - Check if enough players: ", enoughPlayers);
#endif

        return enoughPlayers;
    }
    public void JoinedRoom()
    {
        if (_state == State.Idle)
            ChangeState(State.LookingForPlayers);
        
#if UNITY_EDITOR
        PrintDebug("GameManager - Joined room: ", _multiplayer.CurrentRoom.Name);
#endif
    }
    
    public void OtherJoinedRoom()
    {
        SyncTheStates();
#if UNITY_EDITOR
        PrintDebug("GameManager - ", "Other player joined the room.");
#endif
    }
    
    public void LeftRoom()
    {
        ChangeState(State.Idle);

#if UNITY_EDITOR
        PrintDebug("GameManager - ", "Left room.");
#endif
    }
    
    public void OtherLeftRoom()
    {
#if UNITY_EDITOR
        PrintDebug("GameManager - ", "Other player left the room.");
#endif
    }

    // CHANGING STATES
    public void ChangeState(byte stateIndex)
    {
        AssignState((State)stateIndex);
    }
    public void ChangeState(State state)
    {
        AssignState(state);
    }
    private void AssignState(State state)
    {
        _state = state;

        _currentState = _state switch
        {
            State.Idle => _idleGameState,
            State.LookingForPlayers => _lookingForPlayerGameState,
            State.PrepareRound => _prepareRoundGameState,
            State.StartRound => _startRoundGameState,
            State.FinishRound => _finishRoundGameState,
            State.Restart => _restartGameState,
            _ => _currentState
        };
        _currentState.Run();

        UpdateSyncState();

#if UNITY_EDITOR
        PrintDebug("GameManager - State Changed to: ", _state);
#endif
    }

    public void UpdateSyncState()
    {
        Avatar avatar = _multiplayer.GetAvatar();
        if (avatar != null)
        {
            GameObject player = avatar.gameObject;
            PlayerStateSync playerStateSync = player.GetComponentInChildren<PlayerStateSync>();
                playerStateSync.currentGameState = (byte)_state;
        }
    }

    private void SyncTheStates()
    {
        Avatar avatar = _multiplayer.GetAvatar();
        if (avatar != null)
        {
            GameObject player = avatar.GameObject();
            if (player != null)
            {
                PlayerStateSync playerStateSync = player.GetComponentInChildren<PlayerStateSync>();
                playerStateSync.SyncMyState();
            }
        }
    }
    
    public void ChangeIfInRound(bool inRound)
    {
        Avatar avatar = _multiplayer.GetAvatar();
        if (avatar != null)
        {
            GameObject player = avatar.GameObject();
            if (player != null)
            {
                PlayerStateSync playerStateSync = player.GetComponentInChildren<PlayerStateSync>();
                playerStateSync.inRound = inRound;
            }
        }
    }
    
    public void ChangeIfAlive(bool pulse)
    {
        Avatar avatar = _multiplayer.GetAvatar();
        if (avatar != null)
        {
            GameObject player = avatar.GameObject();
            if (player != null)
            {
                PlayerStateSync playerStateSync = player.GetComponentInChildren<PlayerStateSync>();
                playerStateSync.isAlive = pulse;
                if (pulse)
                {
                    DamageableComponent damageableComponent = player.GetComponentInChildren<DamageableComponent>(); 
                    damageableComponent.ResetPlayerHealth();
                }
            }
        }
    }

    public bool CheckIfEveryoneSameState(State state)
    {
        bool allSameState = true;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (var player in players)
        {
            PlayerStateSync playerStateSync = player.GetComponentInChildren<PlayerStateSync>();
            if (playerStateSync.currentGameState != (byte)state)
                allSameState = false;
        }
        
        return allSameState;
    }
    // DEBUG PRINT
#if UNITY_EDITOR
    public void PrintDebug<T> (string text, T debugData)
    {
        if (_showDebugLogs)
            Debug.Log("<color=olive>" + text + "</color><color=teal>" + debugData.ToString() + "</color>");
    }
#endif
}
