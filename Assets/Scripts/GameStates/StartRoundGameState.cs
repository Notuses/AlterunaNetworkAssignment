using Alteruna;
using UnityEngine;
using Avatar = Alteruna.Avatar;

public class StartRoundGameState : GameState
{
    private const float DelayBetweenChecksSeconds = 2f;
    private float _nextCheck = DelayBetweenChecksSeconds;
    private bool _finishRound = false;

    // ReSharper disable Unity.PerformanceAnalysis
    public override void Update()
    {
        if (Multiplayer.Instance.Me.Index != 0)
            return;
        
        if (_nextCheck <= 0)
        {
            if (!_finishRound)
            {
                CheckIfCanFinish();
                _nextCheck = DelayBetweenChecksSeconds;
            }
            else
                FinishRound();
        }
        
        _nextCheck -= Time.deltaTime;
    }

    public override void Run()
    {
        _finishRound = false;
        _nextCheck = DelayBetweenChecksSeconds;
        WorldManager.Instance.StartShrinkGrid();
        
        UiManager.Instance.ShowLobby(false);
        
#if UNITY_EDITOR
        GameManager.Instance.PrintDebug("GameManager - ", "Game started! Shrinking of map enabled");
#endif
    }

    private void CheckIfCanFinish()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        int playersAlive = 0;

        foreach (var player in players)
        {
            PlayerStateSync playerStateSync = player.GetComponentInChildren<PlayerStateSync>();
            if (playerStateSync.inRound && playerStateSync.isAlive)
                playersAlive++;
        }

        if (playersAlive < 2)
        {
            _finishRound = true; 
            _nextCheck = 1.0f;
        }
    }

    private void FinishRound()
    {
#if UNITY_EDITOR
        GameManager.Instance.PrintDebug("GameManager - ", "Round finished!");
#endif
        
        GameManager.Instance.CallChangeMyState(GameManager.State.FinishRound);
    }
}