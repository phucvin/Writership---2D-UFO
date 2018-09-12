using System.Collections;
using UnityEngine;

public class StopThenDestroyParticleSystem : MonoBehaviour
{
    [SerializeField]
    private float delay = 1f;

    private ParticleSystem ps;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        if (!ps) throw new MissingComponentException("ParticleSystem");
        
        W.Mark(ps);
    }

    public IEnumerator Start()
    {
        yield return new WaitForSeconds(delay);
        ps.Stop();
        while (ps.particleCount > 0) yield return null;
        // Not actually destroy, but make a request
        // to avoid conflicting in W.Mark
        G.RequestDestroy.Fire(gameObject);
    }
}
