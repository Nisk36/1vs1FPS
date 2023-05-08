using UnityEngine;
using UnityEngine.UI;

public class PlayerInformation : MonoBehaviour
{
    public Text playerNameText, kilesText, deathText;//名前とキルデス数を表示するテキスト

    public void SetPlayerDetailes(string name, int kill, int death)
    {
        playerNameText.text = name;
        kilesText.text = kill.ToString();
        deathText.text = death.ToString();
    }
}
