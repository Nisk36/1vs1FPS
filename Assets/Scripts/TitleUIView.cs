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
    /// ‘€ìà–¾ƒpƒlƒ‹‚Ì•\¦İ’è
    /// </summary>
    /// <param name="isActive"></param>
    public void SetActiveExplainPanel(bool isActive)
    {
        _explainPanel.SetActive(isActive);
    }

}
