using System;
using UnityEngine;

public class Ticker : MonoBehaviour
{
    public class OnTickArgs : EventArgs
    {
        public int ticks;
    }
    public static event EventHandler<OnTickArgs> OnTick;

    private const float TickTimerMAX = .2f;
    private int _tick;
    private float _tickTimer;

    private void Awake() {
        _tick = 0;
    }

    // Update is called once per frame
    void Update()
    {
        _tickTimer += Time.deltaTime;

        if (_tickTimer >= TickTimerMAX) {
            _tickTimer = 0;
            _tick++;
            if (OnTick != null) OnTick(this, new OnTickArgs {ticks = _tick});
            
        //    Debug.Log("ticks: " + tick);
        }
    }
}
