using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Camera))]
public class CameraShaker : MonoBehaviour
{
    private Transform _t;
    private Vector3 _startPosition;
    private bool _shaking = false;
    private void Awake()
    {
        _t = GetComponent<Transform>();
    }
    
    void OnEnable()
    {
        _startPosition = _t.localPosition;
        _shaking = false;
    }
    
    public void Shake(float shakeTime, float strength)
    {
        if (_shaking) return;
        StartCoroutine(ShakeRoutine(shakeTime, strength));
    }
    
    private IEnumerator ShakeRoutine(float shakeTime, float strength)
    {
        _shaking = true;
        float elapsed = 0;
        while (elapsed <= shakeTime)
        {
            elapsed += Time.deltaTime;
            _t.transform.localPosition = _startPosition + Random.insideUnitSphere * strength;
            yield return null;
        }
        _t.position = _startPosition;
        _shaking = false;
    }
}
