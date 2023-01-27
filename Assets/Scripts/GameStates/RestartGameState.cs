using Alteruna;
using UnityEngine;

public class RestartGameState : GameState
{
    private const float Delay = 1f;
    private float _countdown = Delay;

    // ReSharper disable Unity.PerformanceAnalysis
    public override void Update()
    {
        if (_countdown <= 0)
        {
            GameManager.Instance.ChangeState(GameManager.State.LookingForPlayers);
        }

        _countdown -= Time.deltaTime;
    }

    public override void Run()
    {
        _countdown = Delay;
        WorldManager.Instance.ResetHexGrid();
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        int i = 0;
        foreach (var player in players)
        {
            player.transform.position = Multiplayer.Instance.AvatarSpawnLocations[i].position;
            i++;
        }
        UiManager.Instance.ShowLobby(true);
    }
}
