using System;
using System.Collections.Generic;
using UnityEngine;
using Writership;

public class G : MonoBehaviour
{
    public static G Instance { get; private set; }

    public static readonly IEngine Engine = new MultithreadEngine();
    public static readonly El<bool> IsTutorialInfoShowing = Engine.El(false);
    public static readonly Op<Empty> StartGame = Engine.Op<Empty>(allowWriters: true);
    public static readonly El<bool> IsGameRunning = Engine.El(false);
    public static readonly Op<Empty> Restart = Engine.Op<Empty>(allowWriters: true);
    public static readonly Op<Ops.PickUp> PickUp = Engine.Op<Ops.PickUp>();
    public static readonly Op<float> Tick = Engine.Op<float>(reducer: (a, b) => a + b);
    public static readonly El<int> TotalItemCount = Engine.El(0);
    public static readonly Op<Empty> AddItem = Engine.Op<Empty>();
    public static readonly Op<GameObject> RequestDestroy = Engine.Op<GameObject>();
    public static readonly Op<Ops.Hit> Hit = Engine.Op<Ops.Hit>();
    public static readonly Op<Empty> TouchEnemy = Engine.Op<Empty>();
    public static readonly El<bool> IsWinning = Engine.El(false);

    private readonly CompositeDisposable cd = new CompositeDisposable();

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        Engine.Dispose();
    }

    private void OnEnable()
    {
        Engine.Computer(cd, new object[] { StartGame, Restart }, () =>
        {
            if (StartGame) IsGameRunning.Write(true);
            else if (Restart) IsGameRunning.Write(false);
        });
        Engine.Computer(cd, new object[] { AddItem }, () =>
        {
            TotalItemCount.Write(TotalItemCount + AddItem.Count);
        });

        Engine.Firer(cd, new object[] { TouchEnemy }, () =>
        {
            if (TouchEnemy && !IsWinning) Restart.Fire(Empty.Instance);
        });

        {
            var requested = new List<GameObject>();
            Engine.Guarder(cd,
                new object[] { Tick, RequestDestroy },
                () =>
                {
                    if (Tick)
                    {
                        GameObject stillAlive = null;
                        for (int i = 0, n = requested.Count; i < n; ++i)
                        {
                            if (requested[i])
                            {
                                stillAlive = requested[i];
                                break;
                            }
                        }
                        if (stillAlive)
                        {
                            Debug.LogWarning("Requested to destroy but still alive", stillAlive);
                        }
                        requested.Clear();
                    }

                    requested.AddRange(RequestDestroy.Read());
                }
            );
        }
        Engine.Guarder(cd, new object[] { Restart, IsGameRunning }, () =>
        {
            if (Restart && !IsGameRunning) throw new InvalidOperationException();
        });
    }

    private void OnDisable()
    {
        cd.Dispose();
        Engine.Dispose();
    }

    private void LateUpdate()
    {
        if (IsGameRunning) Tick.Fire(Time.deltaTime);

        Engine.Update();
        W.Cull();
    }
}
