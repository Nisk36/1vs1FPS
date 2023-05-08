using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleUIPresenter : MonoBehaviour
{
    [SerializeField]
    private TitleUIView _view = null;

    private void Awake()
    {
        if (_view == null)
        {
            Debug.LogError("Viewがnullです");
        }

        // ボタンイベント登録
        _view.QuitGameButton.onClick.AddListener(() => OnClickQuitGameButton());
        _view.ShowOperateButton.onClick.AddListener(() => OnClickExplainButton());
    }

    /// <summary>
    /// ゲーム終了ボタンのイベント
    /// </summary>
    public void OnClickQuitGameButton()
    {
        // ここにゲーム終了コードを書く
    }

    /// <summary>
    /// 操作説明ボタンのイベント
    /// </summary>
    public void OnClickExplainButton()
    {
        _view.SetActiveExplainPanel(true);
    }



}
