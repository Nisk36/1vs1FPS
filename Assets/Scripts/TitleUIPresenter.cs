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
            Debug.LogError("View��null�ł�");
        }

        // �{�^���C�x���g�o�^
        _view.QuitGameButton.onClick.AddListener(() => OnClickQuitGameButton());
        _view.ShowOperateButton.onClick.AddListener(() => OnClickExplainButton());
    }

    /// <summary>
    /// �Q�[���I���{�^���̃C�x���g
    /// </summary>
    public void OnClickQuitGameButton()
    {
        // �����ɃQ�[���I���R�[�h������
    }

    /// <summary>
    /// ��������{�^���̃C�x���g
    /// </summary>
    public void OnClickExplainButton()
    {
        _view.SetActiveExplainPanel(true);
    }



}
