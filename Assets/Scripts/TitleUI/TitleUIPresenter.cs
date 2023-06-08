using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class TitleUIPresenter : MonoBehaviour
{
    [SerializeField]
    private TitleUIView view = null;

    private void Awake()
    {
        Assert.IsNotNull(view);

        // ボタンイベント登録
        view.QuitGameButton.onClick.AddListener(() => OnClickQuitGameButton());
        view.ShowOperateButton.onClick.AddListener(() => OnClickExplainButton());
    }

    /// <summary>
    /// ゲーム終了ボタンのイベント
    /// </summary>
    public void OnClickQuitGameButton()
    {
        // ここにゲーム終了コードを書く
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;//ゲームプレイ終了
#else
        Application.Quit();//ゲームプレイ終了
#endif
    }

    /// <summary>
    /// 操作説明ボタンのイベント
    /// </summary>
    public void OnClickExplainButton()
    {
        view.SetActiveExplainPanel(true);
    }
}
