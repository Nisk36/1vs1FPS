using UnityEngine.UI;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    //ammoText
    public Text bulletText;

    //HPスライダー格納
    public Slider hpSlider;

    //DeathPanel
    public GameObject deathPanel;
    //Deathテキスト
    public Text deathText;

    public GameObject scoreBoard;

    public PlayerInformation info;

    //終了パネル
    public GameObject endPanel;

    //リロードテキスト
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

    //Deathパネル更新して開く
    public void UpdateDeathUI(string name)
    {
        deathPanel.SetActive(true);

        deathText.text = "Killed by " +
            "" + name;

        //デスパネルを閉じる;
        Invoke("CloseDeathUI", 5f);
    }

    //デスパネルを非表示
    public void CloseDeathUI()
    {
        deathPanel.SetActive(false);
    }

    //スコアボードを開く関数
    public void ChangeScoreUI()
    {
        //表示非表示を切り替える
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
