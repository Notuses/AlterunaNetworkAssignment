using UnityEngine;

public class Timer 
{
    public float curTime;
    private bool running = false;
    
    void Start()
    {
        curTime = 0;
    }

    public void Run()
    {
        running = true;
    }
    
    public void Reset()
    {
        running = false;
        curTime = 0;
    }

    public void Stop()
    {
        running = false;
    }
    
    public void Update()
    {
        if (!running)
            return;
        curTime += Time.deltaTime;
    }
}
