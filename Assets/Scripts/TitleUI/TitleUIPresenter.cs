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

        // �{�^���C�x���g�o�^
        view.QuitGameButton.onClick.AddListener(() => OnClickQuitGameButton());
        view.ShowOperateButton.onClick.AddListener(() => OnClickExplainButton());
    }

    /// <summary>
    /// �Q�[���I���{�^���̃C�x���g
    /// </summary>
    public void OnClickQuitGameButton()
    {
        // �����ɃQ�[���I���R�[�h������
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;//�Q�[���v���C�I��
#else
        Application.Quit();//�Q�[���v���C�I��
#endif
    }

    /// <summary>
    /// ��������{�^���̃C�x���g
    /// </summary>
    public void OnClickExplainButton()
    {
        view.SetActiveExplainPanel(true);
    }
}
