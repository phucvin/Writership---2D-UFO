using UnityEngine;
using Writership;

public class CompleteCameraController : MonoBehaviour
{
    [SerializeField]
    private CompletePlayerController player = null;

    private Vector3 offset;

    private readonly CompositeDisposable cd = new CompositeDisposable();

    private void Awake()
    {
        offset = transform.position - player.transform.position;

        W.Mark(transform, "position");
    }

    private void OnEnable()
    {
        G.Engine.Reader(cd, new object[] { player.Position }, () =>
        {
            transform.position = player.Position.Read() + offset;
        });
    }

    private void OnDisable()
    {
        cd.Dispose();
    }
}
