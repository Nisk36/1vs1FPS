using UnityEngine.UI;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    //ammoText
    [SerializeField]
    private Text bulletText = null;

    //HPスライダー格納
    [SerializeField]
    private Slider hpSlider = null;

    //DeathPanel
    [SerializeField]
    private GameObject deathPanel = null;
    //Deathテキスト
    [SerializeField]
    private Text deathText = null;
    
    [SerializeField]
    private GameObject scoreBoard = null;

    public PlayerInformation info = null;

    //終了パネル
    [SerializeField]
    private GameObject endPanel = null;

    //リロードテキスト
    [SerializeField]
    private GameObject reloadText = null;
    
    //Player1のText
    [SerializeField]
    private Text player1NameText = null;
    //Player1の取得ラウンド数
    [SerializeField]
    private Text player1RoundText = null;
    
    //Player2のText
    [SerializeField]
    private Text player2NameText = null;
    //Player2の取得ラウンド数
    [SerializeField]
    private Text player2RoundText = null;
    
    //Timer
    [SerializeField] 
    private Text timerText = null;
    public Text TimerText => timerText;
    
    //CountDownText
    [SerializeField] 
    private Text countDownText = null;
    public Text CountDownText => countDownText;
    



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

    public void SetPlayerText(string player1Name, string player2Name)
    {
        player1NameText.text = player1Name;
        player2NameText.text = player2Name;
    }

    public void SetPlayer1Text(string player1Name)
    {
        player1NameText.text = player1Name;
    }

    public void SetPlayer1Round(int player1Round)
    {
        player1RoundText.text = player1Round.ToString("0");
    }

    public void SetPlayer2Text(string player2Name)
    {
        player2NameText.text = player2Name;
    }
    
    public void SetPlayer2Round(int player2Round)
    {
        player2RoundText.text = player2Round.ToString("0");
    }
    
}
