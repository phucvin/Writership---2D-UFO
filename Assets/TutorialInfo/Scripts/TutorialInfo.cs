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

    public IEl<bool> ShowAtStart { get; private set; }
    public IOp<Empty> ToggleShowAtStart { get; private set; }

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

        if (PlayerPrefs.HasKey(ShowAtStartPrefsKey))
        {
            showAtStart = PlayerPrefs.GetInt(ShowAtStartPrefsKey) == 1;
        }

        ShowAtStart = G.Engine.El(showAtStart);
        ToggleShowAtStart = G.Engine.Op<Empty>();
    }

    private void OnEnable()
    {
        cd.Add(G.Engine.RegisterComputer(
            new object[] { ToggleShowAtStart },
            () =>
            {
                bool s = ShowAtStart.Read();
                if (ToggleShowAtStart.Read().Count % 2 == 1) s = !s;
                if (s != ShowAtStart.Read()) ShowAtStart.Write(s);
            }
        ));
        cd.Add(G.Engine.RegisterComputer(
            new object[] { G.IsGameRunning, ShowAtStart },
            () =>
            {
                bool i = G.IsTutorialInfoShowing.Read();
                if (G.IsGameRunning.Read()) i = false;
                else if (ShowAtStart.Read()) i = true;
                if (i != G.IsTutorialInfoShowing.Read()) G.IsTutorialInfoShowing.Write(i);
            }
        ));

        cd.Add(G.Engine.RegisterListener(
            new object[] { ShowAtStart },
            () =>
            {
                bool s = ShowAtStart.Read();
                PlayerPrefs.SetInt(ShowAtStartPrefsKey, s ? 1 : 0);
                if (showAtStartToggle.isOn != s) showAtStartToggle.isOn = s;
            }
        ));
        cd.Add(G.Engine.RegisterListener(
            new object[] { G.IsTutorialInfoShowing },
            () =>
            {
                bool i = G.IsTutorialInfoShowing.Read();
                overlay.SetActive(i);
                mainListener.enabled = !i;
            }
        ));
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
        if (showAtStartToggle.isOn != ShowAtStart.Read())
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
