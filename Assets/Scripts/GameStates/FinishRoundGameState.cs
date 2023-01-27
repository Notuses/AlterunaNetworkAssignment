using Alteruna;
using Unity.VisualScripting;
using UnityEngine;
using Avatar = Alteruna.Avatar;

public class FinishRoundGameState : GameState
{
    public override void Update()
    { }

    public override void Run()
    {
        GameManager.Instance.ChangeState(GameManager.State.Restart);
    }
}
