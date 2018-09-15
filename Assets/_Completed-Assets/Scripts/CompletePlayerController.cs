using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Writership;

public class CompletePlayerController : MonoBehaviour
{
    [SerializeField]
    private float speed = 10;
    [SerializeField]
    private Text countText = null;
    [SerializeField]
    private Text winText = null;

    private Rigidbody2D rb2d;
    private Vector3 orgPos;

    public El<Vector3> Position { get; private set; }
    public El<int> Score { get; private set; }

    private El<Vector2> movement;

    private readonly CompositeDisposable cd = new CompositeDisposable();

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        orgPos = transform.position;

        W.Mark(rb2d);
        W.Mark(transform, "position");
        W.Mark(countText, "text");
        W.Mark(winText, "text");

        Position = G.Engine.El(transform.position);
        Score = G.Engine.El(0);
        movement = G.Engine.El(Vector2.zero);
    }

    private void OnEnable()
    {
        G.Engine.Computer(cd, new object[] { G.PickUp, G.Restart }, () =>
        {
            if (G.Restart) Score.Write(0);
            else Score.Write(Score + G.PickUp.Count);
        });
        G.Engine.Computer(cd, new object[] { Score, G.TotalItemCount }, () =>
        {
            G.IsWinning.Write(Score >= G.TotalItemCount);
        });

        G.Engine.Reader(cd, new object[] { Score, G.TotalItemCount }, () =>
        {
            countText.text = string.Format("Count: {0} / {1}",
                Score, G.TotalItemCount);
        });
        G.Engine.Reader(cd, new object[] { Score, G.TotalItemCount }, () =>
        {
            if (Score >= G.TotalItemCount) winText.text = "You win!";
            else winText.text = "";
        });
        {
            bool canRestart = false;
            Coroutine lastCoroutine = null;
            cd.Add(new DisposableAction(() =>
            {
                if (lastCoroutine != null) G.Instance.StopCoroutine(lastCoroutine);
            }));
            G.Engine.Reader(cd, new object[] { Score, G.TotalItemCount }, () =>
            {
                // TODO Maybe wrong if call multiple times
                if (Score < G.TotalItemCount) canRestart = true;
                if (Score >= G.TotalItemCount && canRestart)
                {
                    canRestart = false;
                    if (lastCoroutine != null) G.Instance.StopCoroutine(lastCoroutine);
                    lastCoroutine = G.Instance.StartCoroutine(WaitThenRestart(G.Restart));
                }
            });
        }

        G.Engine.Writer(cd, new object[] { G.Tick, G.Restart }, () =>
        {
            if (G.Restart) transform.position = orgPos;
            Position.Write(transform.position);
        });
    }

    private void OnApplicationQuit()
    {
        cd.Dispose();
    }

    private void OnDisable()
    {
        cd.Dispose();
    }

    private void FixedUpdate()
    {
        if (G.IsGameRunning)
        {
            rb2d.AddForce(movement.Read() * speed);
        }
        else
        {
            rb2d.velocity = Vector2.zero;
            rb2d.angularVelocity = 0;
        }
    }

    private void Update()
    {
        // TODO Fix this if this inside a writer
        var v = Vector2.zero;
        if (G.IsGameRunning)
        {
            v.x = Input.GetAxis("Horizontal");
            v.y = Input.GetAxis("Vertical");
        }
        movement.Write(v);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var pickUp = collision.gameObject.GetComponent<CompletePickUp>();
        if (!pickUp) return;

        G.PickUp.Fire(new Ops.PickUp
        {
            Player = this,
            Item = pickUp
        });
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var enemy = collision.gameObject.GetComponent<CompleteEnemy>();
        if (!enemy) return;

        G.TouchEnemy.Fire(Empty.Instance);
    }

    private static IEnumerator WaitThenRestart(IOp<Empty> restart)
    {
        yield return new WaitForSeconds(1f);
        restart.Fire(Empty.Instance);
    }

    private static IEnumerator LoopFixedUpdate(Rigidbody2D rb2d, bool needStop, Vector2 forceToAdd)
    {
        if (needStop)
        {
            rb2d.velocity = Vector2.zero;
            rb2d.angularVelocity = 0;
        }
        else
        {
            while (true)
            {
                yield return new WaitForFixedUpdate();
                rb2d.AddForce(forceToAdd);
            }
        }
    }
}
