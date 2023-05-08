using UnityEngine.UI;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    //ammoText
    public Text bulletText;

    //HP�X���C�_�[�i�[
    public Slider hpSlider;

    //DeathPanel
    public GameObject deathPanel;
    //Death�e�L�X�g
    public Text deathText;

    public GameObject scoreBoard;

    public PlayerInformation info;

    //�I���p�l��
    public GameObject endPanel;

    //�����[�h�e�L�X�g
    public GameObject reloadText;



    public void SetBulletText(int ammoClip,int ammunition)
    {
        bulletText.text = ammoClip + "/" + ammunition;
    }

    public void UpdateHP(int maxHP, int currentHP)
    {
        hpSlider.maxValue = maxHP;
        hpSlider.value = currentHP;
    }

    //Death�p�l���X�V���ĊJ��
    public void UpdateDeathUI(string name)
    {
        deathPanel.SetActive(true);

        deathText.text = "Killed by " +
            "" + name;

        //�f�X�p�l�������;
        Invoke("CloseDeathUI", 5f);
    }

    //�f�X�p�l�����\��
    public void CloseDeathUI()
    {
        deathPanel.SetActive(false);
    }

    //�X�R�A�{�[�h���J���֐�
    public void ChangeScoreUI()
    {
        //�\����\����؂�ւ���
        scoreBoard.SetActive(!scoreBoard.activeInHierarchy);
    }

    public void OpenEndPanel()
    {
        endPanel.SetActive(true);
    }

    public void ShowReloadText()
    {
        reloadText.SetActive(true);
    }

    public void CloseReloadText()
    {
        reloadText.SetActive(false);
    }
}
