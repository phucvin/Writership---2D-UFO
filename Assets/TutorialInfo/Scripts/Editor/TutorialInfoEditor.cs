using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TutorialInfo))]
public class TutorialInfoEditor : Editor
{
    void OnEnable()
    {
        if (PlayerPrefs.HasKey(TutorialInfo.ShowAtStartPrefsKey))
        {
            ((TutorialInfo)target).ShowAtStartEditorProxy =
                PlayerPrefs.GetInt(TutorialInfo.ShowAtStartPrefsKey) == 1;
        }
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        base.OnInspectorGUI();

        if (EditorGUI.EndChangeCheck())
        {
            W.Mark(typeof(PlayerPrefs), TutorialInfo.ShowAtStartPrefsKey);
            PlayerPrefs.SetInt(TutorialInfo.ShowAtStartPrefsKey,
                ((TutorialInfo)target).ShowAtStartEditorProxy ? 1 : 0);
        }
    }
}
