using System.Collections.Generic;
using UnityEngine;
using Writership;

public class G : MonoBehaviour
{
    public static G Instance { get; private set; }

    public static readonly IEngine Engine = new MultithreadEngine();
    public static readonly IEl<float> TimeScale = Engine.El(1f);
    public static readonly IEl<bool> IsTutorialInfoShowing = Engine.El(false);
    public static readonly IOp<Empty> StartGame = Engine.Op<Empty>();
    public static readonly IEl<bool> IsGameRunning = Engine.El(false);
    public static readonly IOp<Empty> Restart = Engine.Op<Empty>();
    public static readonly IOp<Ops.PickUp> PickUp = Engine.Op<Ops.PickUp>();
    public static readonly IOp<float> Tick = Engine.Op<float>();
    public static readonly IEl<int> TotalItemCount = Engine.El(0);
    public static readonly IOp<Empty> AddItem = Engine.Op<Empty>();
    public static readonly IOp<GameObject> RequestDestroy = Engine.Op<GameObject>();
    public static readonly IOp<Ops.Hit> Hit = Engine.Op<Ops.Hit>();
    public static readonly IOp<Empty> TouchEnemy = Engine.Op<Empty>();

    private readonly CompositeDisposable cd = new CompositeDisposable();

    private void Awake()
    {
        W.Mark(typeof(Time), "timeScale");

        Instance = this;
    }

    private void OnDestroy()
    {
        Engine.Dispose();
    }

    private void OnEnable()
    {
        cd.Add(Engine.RegisterComputer(
            new object[] { StartGame },
            () =>
            {
                bool i = IsGameRunning.Read();
                if (StartGame.Read().Count > 0 && !i) i = true;
                if (i != IsGameRunning.Read()) IsGameRunning.Write(i);
            }
        ));
        cd.Add(Engine.RegisterComputer(
            new object[] { IsTutorialInfoShowing, IsGameRunning },
            () =>
            {
                float t = TimeScale.Read();
                if (IsGameRunning.Read()) t = 1;
                else if (IsTutorialInfoShowing.Read()) t = 0;
                if (t != TimeScale.Read()) TimeScale.Write(t);
            }
        ));
        cd.Add(Engine.RegisterComputer(
            new object[] { AddItem },
            () =>
            {
                int t = TotalItemCount.Read();
                t += AddItem.Read().Count;
                if (t != TotalItemCount.Read()) TotalItemCount.Write(t);
            }
        ));
        cd.Add(Engine.RegisterComputer(
            new object[] { TouchEnemy },
            () =>
            {
                if (TouchEnemy.Read().Count > 0)
                {
                    Restart.Fire(Empty.Instance);
                }
            }
        ));

        cd.Add(Engine.RegisterListener(
            new object[] { TimeScale },
            () =>
            {
                Time.timeScale = TimeScale.Read();
            }
        ));
        {
            var requested = new List<GameObject>();
            cd.Add(Engine.RegisterListener(
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
            ));
        }
    }

    private void OnDisable()
    {
        cd.Dispose();
        Engine.Dispose();
    }

    private void LateUpdate()
    {
        Tick.Fire(Time.deltaTime);

        Engine.Update();
        W.Cull();
    }
}
