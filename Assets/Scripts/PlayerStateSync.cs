using Alteruna;
using UnityEngine;

public class PlayerStateSync : Synchronizable
{

    public byte currentGameState;
    private byte _oldGameState;
    public bool isAlive = true;
    private bool _oldIsAlive = true;

    public bool inRound = false;
    private bool _oldInRound = false;

    public override void AssembleData(Writer writer, byte LOD = 100)
    {
#if UNITY_EDITOR
        GameManager.Instance.PrintDebug("Assembling data", this.name);
#endif
        writer.Write(currentGameState);
        writer.Write(isAlive);
        writer.Write(inRound);
    }

    public override void DisassembleData(Reader reader, byte LOD = 100)
    {
#if UNITY_EDITOR
        GameManager.Instance.PrintDebug("Disassembling data", this.name);
#endif
        currentGameState = reader.ReadByte();
        isAlive = reader.ReadBool();
        inRound = reader.ReadBool();

        _oldIsAlive = isAlive;
        _oldGameState = currentGameState;
        _oldInRound = inRound;
    }

    private void Update()
    {
        if (currentGameState != _oldGameState || _oldIsAlive != isAlive)
        {
            _oldGameState = currentGameState;
            _oldIsAlive = isAlive;
            _oldInRound = inRound;
            
            Commit();           
        }
        base.SyncUpdate();
    }

    public void SyncMyState()
    {
        Commit();
    }
}
