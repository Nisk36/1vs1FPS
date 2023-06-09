using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleUIView : MonoBehaviour
{
    [SerializeField]
    private Button _quitGameButton = null;

    [SerializeField]
    private Button _showOperateButton = null;

    [SerializeField]
    private GameObject _explainPanel = null;

    public Button QuitGameButton => _quitGameButton;

    public Button ShowOperateButton => _showOperateButton;

    /// <summary>
    /// 操作説明パネルの表示設定
    /// </summary>
    /// <param name="isActive"></param>
    public void SetActiveExplainPanel(bool isActive)
    {
        _explainPanel.SetActive(isActive);
    }
}
