using System.Collections.Generic;
using Alteruna;
using Unity.VisualScripting;
using UnityEngine;
using Avatar = Alteruna.Avatar;

public class PrepareRoundGameState : GameState
{
    private const float DelayBetweenChecksSeconds = 2f;
    private float _nextCheck = DelayBetweenChecksSeconds;

    // ReSharper disable Unity.PerformanceAnalysis
    public override void Update()
    {
        if (_nextCheck <= 0)
        {
            CheckIfCanStart();
            _nextCheck = DelayBetweenChecksSeconds;
        }

        _nextCheck -= Time.deltaTime;
    }

    public override void Run()
    {
        GameManager.Instance.ChangeIfInRound(true);
        _nextCheck = DelayBetweenChecksSeconds;
    }

    private void CheckIfCanStart()
    {
        if (GameManager.Instance.CheckIfEnoughPlayers())
        {
            if (Multiplayer.Instance.Me.Index != 0)
                return;
            StartButtonEnable();
        }
        else
        {
            UiManager.Instance.CanStart(false);
            GameManager.Instance.ChangeState(GameManager.State.LookingForPlayers);
        }
    }

    private void StartButtonEnable()
    {
#if UNITY_EDITOR
        GameManager.Instance.PrintDebug("GameManager - ", "TRYING TO ENABLE START BUTTON");
#endif
        if (GameManager.Instance.CheckIfEveryoneSameState(GameManager.State.PrepareRound))
            UiManager.Instance.CanStart(true);
        else
        {
            UiManager.Instance.CanStart(false);
#if UNITY_EDITOR
            GameManager.Instance.PrintDebug("GameManager - ", "UNABLE TO ENABLE START BUTTON (PEOPLE NOT SYNCED).");
#endif
        }
    }

}