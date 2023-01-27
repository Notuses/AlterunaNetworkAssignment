using Alteruna;
using UnityEngine;

public class VelocityRemoteSync : Synchronizable
{
    private Rigidbody rb;
    private float _velMagnitude;
    private float _oldVelMagnitude;
    public float VelocityMagnitude { get { return _velMagnitude; } }
    public float OldVelocityMagnitude { get { return _oldVelMagnitude; } }

    // Start is called before the first frame update

    private void Awake()
    {
        rb = GetComponentInParent<Rigidbody>();
    }

    void Start()
    {
        _oldVelMagnitude = _velMagnitude;
    }

    // Update is called once per frame
    void Update()
    {
        if (_oldVelMagnitude != _velMagnitude)
        {
            _oldVelMagnitude = _velMagnitude;
            Commit();
        }
        SyncUpdate();
    }

    private void FixedUpdate()
    {
        _velMagnitude = rb.velocity.magnitude;
    }

    public override void AssembleData(Writer writer, byte LOD = 100)
    {
        writer.Write(_velMagnitude);
    }

    public override void DisassembleData(Reader reader, byte LOD = 100)
    {
        _velMagnitude = reader.ReadFloat();
        _oldVelMagnitude = _velMagnitude;
    }
}
