using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    //　トータル制限時間
    private float totalTime;
    //　制限時間（分）
    [SerializeField]
    private int minute_default;
    //　制限時間（秒）
    [SerializeField]
    private float seconds_default;
    
    // 変数用の分
    private int minute;
    //　変数用の秒
    private float seconds;
    //　前回Update時の秒数
    private float oldSeconds;
    
    private readonly string[] COUNT_ARRAY = { "2", "1", "GO!" };

    private int mCurrentIndex;
    private float mDurationTime;

    private bool isTimeUp;
    public bool IsTimeUp => isTimeUp;
    
    // Start is called before the first frame update
    public void Initilize(Text timerText)
    {
        minute = minute_default;
        seconds = seconds_default;
        totalTime = minute * 60 + seconds;
        oldSeconds = 0f;
        timerText.text = minute.ToString() + ":" + ((int) seconds).ToString();
        isTimeUp = false;
    }

    public void UpdateTimer(Text timerText)
    {
        //　制限時間が0秒以下なら何もしない
        if (totalTime <= 0f) {
            return;
        }
        //　一旦トータルの制限時間を計測；
        totalTime = minute * 60 + seconds;
        totalTime -= Time.deltaTime;
 
        //　再設定
        minute = (int) totalTime / 60;
        seconds = totalTime - minute * 60;
        
        //　タイマー表示用UIテキストに時間を表示する
        if((int)seconds != (int)oldSeconds) {
            timerText.text = minute.ToString("00") + ":" + ((int) seconds).ToString("00");
        }
        oldSeconds = seconds;
        //　制限時間以下になったらコンソールに『制限時間終了』という文字列を表示する
        if(totalTime <= 0f)
        {
            isTimeUp = true;
        }
        
    }
    
    
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //データの送信
            stream.SendNext(minute);
            stream.SendNext(seconds);
            stream.SendNext(oldSeconds);
            stream.SendNext(totalTime);
        }
        else
        {
            //データの受信
            this.minute = (int)(float)stream.ReceiveNext();
            this.seconds = (int)(float)stream.ReceiveNext();
            this.oldSeconds = (int)(float)stream.ReceiveNext();
            this.totalTime = (int)(float)stream.ReceiveNext();
        }
    }
    public bool UpdateCountDown(Text countDownText)
    {
        switch (mCurrentIndex)
        {
            case 0: // カウントダウンテキストを表示
                countDownText.gameObject.SetActive(true);
                mDurationTime = 1.0f;
                mCurrentIndex++;
                break;
            case 1: // 2を表示
                mDurationTime -= Time.deltaTime;
                if (mDurationTime <= 0.0f)
                {
                    countDownText.text = COUNT_ARRAY[0];
                    mDurationTime += 1.0f;
                    mCurrentIndex++;
                }
                break;
            case 2: // 1を表示
                mDurationTime -= Time.deltaTime;
                if (mDurationTime <= 0.0f)
                {
                    countDownText.text = COUNT_ARRAY[1];
                    mDurationTime += 1.0f;
                    mCurrentIndex++;
                }
                break;
            case 3: // Fire!を表示
                mDurationTime -= Time.deltaTime;
                if (mDurationTime <= 0.0f)
                {
                    countDownText.text = COUNT_ARRAY[2];
                    StartCoroutine(HideCountDownText(countDownText));
                    return true;
                }
                break;
        }
        return false;
    }
    private IEnumerator HideCountDownText(Text countDownText)
    {
        yield return new WaitForSeconds(1.0f);
        countDownText.gameObject.SetActive(false);
    }
    
    
}
