using UnityEngine;
using UnityEngine.UI;
using Writership;

public class TutorialInfo : MonoBehaviour
{
    public const string ShowAtStartPrefsKey = "showLaunchScreen";

    [SerializeField]
    private bool showAtStart = true;
    [SerializeField]
    private string url = null;
    [SerializeField]
    private GameObject overlay = null;
    [SerializeField]
    private AudioListener mainListener = null;
    [SerializeField]
    private Toggle showAtStartToggle = null;

    private Animator animator;

    public El<bool> ShowAtStart { get; private set; }
    public Op<Empty> ToggleShowAtStart { get; private set; }

    private readonly CompositeDisposable cd = new CompositeDisposable();

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (!animator) throw new MissingComponentException("Animator");

        W.Mark(typeof(PlayerPrefs), ShowAtStartPrefsKey);
        W.Mark(showAtStartToggle, "isOn");
        W.Mark(overlay, "active");
        W.Mark(mainListener, "enabled");
        W.Mark(animator, "setTrigger_close");
        W.Mark(animator, "setTrigger_open");

        if (PlayerPrefs.HasKey(ShowAtStartPrefsKey))
        {
            showAtStart = PlayerPrefs.GetInt(ShowAtStartPrefsKey) == 1;
        }

        ShowAtStart = G.Engine.El(showAtStart);
        ToggleShowAtStart = G.Engine.Op<Empty>();

        if (!showAtStart)
        {
            G.StartGame.Fire(Empty.Instance);
        }
    }

    private void OnEnable()
    {
        G.Engine.Computer(cd, new object[] { ToggleShowAtStart }, () =>
        {
            if (ToggleShowAtStart.Count % 2 == 1) ShowAtStart.Write(!ShowAtStart);
        });
        G.Engine.Computer(cd, new object[] { G.IsGameRunning, G.Restart }, () =>
        {
            if (G.Restart) G.IsTutorialInfoShowing.Write(true);
            else G.IsTutorialInfoShowing.Write(!G.IsGameRunning);
        });

        G.Engine.Reader(cd, new object[] { ShowAtStart }, () =>
        {
            PlayerPrefs.SetInt(ShowAtStartPrefsKey, ShowAtStart ? 1 : 0);
            showAtStartToggle.isOn = ShowAtStart;
        });
        G.Engine.Reader(cd, new object[] { G.IsTutorialInfoShowing }, () =>
        {
            overlay.SetActive(G.IsTutorialInfoShowing);
            mainListener.enabled = !G.IsTutorialInfoShowing;
        });
        G.Engine.Reader(cd, new object[] { G.Restart }, () =>
        {
            if (G.Restart) animator.SetTrigger("open");
        });
    }

    private void OnDisable()
    {
        cd.Dispose();
    }

    public void LaunchTutorial()
    {
        Application.OpenURL(url);
    }

    public void StartGame()
    {
        animator.SetTrigger("close");
    }

    public void Closed()
    {
        G.StartGame.Fire(Empty.Instance);
    }

    public void ToggleShowAtLaunch()
    {
        // Should be able to remove this condition, if op contains true/false instead of empty
        if (showAtStartToggle.isOn != ShowAtStart)
        {
            ToggleShowAtStart.Fire(Empty.Instance);
        }
    }

#if UNITY_EDITOR
    public bool ShowAtStartEditorProxy
    {
        get { return showAtStart; }
        set { showAtStart = value; }
    }
#endif
}
