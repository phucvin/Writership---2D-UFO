﻿using UnityEngine;
using Writership;

public class CompleteGun : MonoBehaviour
{
    [SerializeField]
    private GameObject bullet = null;

    [SerializeField]
    private float delay = 0.2f;

    [SerializeField]
    private CompletePlayerAi ai = null;

    private El<bool> isFiring;
    private El<bool> isManualFiring;
    private El<float> currentDelay;
    private Op<Empty> fire;

    private readonly CompositeDisposable cd = new CompositeDisposable();

    private void Awake()
    {
        isFiring = G.Engine.El(false);
        isManualFiring = G.Engine.El(false);
        currentDelay = G.Engine.El(0f);
        fire = G.Engine.Op<Empty>();
    }

    private void OnEnable()
    {
        G.Engine.Computer(cd, new object[] { isFiring, currentDelay, G.Tick }, () =>
        {
            if (isFiring)
            {
                float tmp = currentDelay - G.Tick.Reduced;
                while (tmp < 0)
                {
                    tmp += delay;
                    fire.Fire(Empty.Instance);
                }
                currentDelay.Write(tmp);
            }
            else currentDelay.Write(Mathf.Max(0, currentDelay - G.Tick.Reduced));
        });
        G.Engine.Computer(cd, new object[] { isManualFiring, ai.IsGunFiring }, () =>
        {
            isFiring.Write(isManualFiring || ai.IsGunFiring);
        });

        G.Engine.Reader(cd, new object[] { fire }, () =>
        {
            for (int i = 0, n = fire.Count; i < n; ++i)
            {
                var b = Instantiate(bullet, transform.position, transform.rotation);
                W.Mark(b, "active");
                b.SetActive(true);
            }
        });
    }

    private void OnDisable()
    {
        cd.Dispose();
    }

    private void Update()
    {
        if (!G.IsGameRunning) return;

        bool f = Input.GetButton("Fire1");
        isManualFiring.Write(f);
    }
}
