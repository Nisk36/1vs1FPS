using UnityEngine;
using UnityEngine.UI;

public class PlayerInformation : MonoBehaviour
{
    public Text playerNameText, kilesText, deathText;//���O�ƃL���f�X����\������e�L�X�g

    public void SetPlayerDetailes(string name, int kill, int death)
    {
        playerNameText.text = name;
        kilesText.text = kill.ToString();
        deathText.text = death.ToString();
    }
}
