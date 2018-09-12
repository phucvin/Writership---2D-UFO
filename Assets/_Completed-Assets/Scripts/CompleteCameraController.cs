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
    }

    private void OnEnable()
    {
        cd.Add(G.Engine.RegisterListener(
            new object[] { player.Position },
            () =>
            {
                W.Mark(transform, "position");
                transform.position = player.Position.Read() + offset;
            }
        ));
    }

    private void OnDisable()
    {
        cd.Dispose();
    }
}
