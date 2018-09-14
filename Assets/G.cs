using System;
using System.Collections.Generic;
using UnityEngine;
using Writership;

public class G : MonoBehaviour
{
    public static G Instance { get; private set; }

    public static readonly IEngine Engine = new MultithreadEngine();
    public static readonly IEl<bool> IsTutorialInfoShowing = Engine.El(false);
    public static readonly IOp<Empty> StartGame = Engine.Op<Empty>(allowWriters: true);
    public static readonly IEl<bool> IsGameRunning = Engine.El(false);
    public static readonly IOp<Empty> Restart = Engine.Op<Empty>(allowWriters: true);
    public static readonly IOp<Ops.PickUp> PickUp = Engine.Op<Ops.PickUp>();
    public static readonly IOp<float> Tick = Engine.Op<float>();
    public static readonly IEl<int> TotalItemCount = Engine.El(0);
    public static readonly IOp<Empty> AddItem = Engine.Op<Empty>();
    public static readonly IOp<GameObject> RequestDestroy = Engine.Op<GameObject>();
    public static readonly IOp<Ops.Hit> Hit = Engine.Op<Ops.Hit>();
    public static readonly IOp<Empty> TouchEnemy = Engine.Op<Empty>();
    public static readonly IEl<bool> IsWinning = Engine.El(false);

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
        Engine.Computer(cd,
            new object[] { StartGame, Restart },
            () =>
            {
                bool i = IsGameRunning.Read();
                if (StartGame.Read().Count > 0) i = true;
                else if (Restart.Read().Count > 0) i = false;
                if (i != IsGameRunning.Read()) IsGameRunning.Write(i);
            }
        );
        Engine.Computer(cd,
            new object[] { AddItem },
            () =>
            {
                int t = TotalItemCount.Read();
                t += AddItem.Read().Count;
                if (t != TotalItemCount.Read()) TotalItemCount.Write(t);
            }
        );

        Engine.Transformer(cd,
            new object[] { TouchEnemy },
            () =>
            {
                if (TouchEnemy.Read().Count > 0 && !IsWinning.Read())
                {
                    Restart.Fire(Empty.Instance);
                }
            }
        );

        {
            var requested = new List<GameObject>();
            Engine.Guarder(cd,
                new object[] { Tick, RequestDestroy },
                () =>
                {
                    if (Tick.Read().Count > 0)
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
        Engine.Guarder(cd,
            new object[] { Restart, IsGameRunning },
            () =>
            {
                if (Restart.Read().Count > 0 && !IsGameRunning.Read())
                {
                    throw new InvalidOperationException();
                }
            }
        );
    }

    private void OnDisable()
    {
        cd.Dispose();
        Engine.Dispose();
    }

    private void LateUpdate()
    {
        if (IsGameRunning.Read()) Tick.Fire(Time.deltaTime);

        Engine.Update();
        W.Cull();
    }
}
