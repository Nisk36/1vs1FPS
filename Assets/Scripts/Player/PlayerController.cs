using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PlayerController : MonoBehaviourPunCallbacks
{
    public Transform viewPoint;
    public float mouseSensitivity = 1.0f;
    private Vector2 mouseInput;
    private float verticalMouseInput;
    //
    private Camera playerCam;
    private Camera gunCam;//銃描画カメラ
    public float fov = 60f;

    private Vector3 moveInput;
    private Vector3 moveDirection;
    private float moveSpeed = 4.0f;
    private const float defaultMoveSpeed = 4.0f;
    private const float dashSpeed = 8.0f;

    public Vector3 jumpForce = new Vector3(0, 3, 0);
    public Transform groundCheckPoint;
    public LayerMask groundLayers;
    Rigidbody rb;

    private bool isCursorAppear = false;

    public List<GunScript> guns = new List<GunScript>();
    private int gunIndex = 0;

    private float shotTimer;

    public int[] ammunition;
    public int[] maxAmmunition;
    public int[] ammoClip;
    public int[] maxAmmoClip;

    public GameObject bulletImpact;

    private UIManager uiManager;

    //Animator
    public Animator animator;

    //プレイヤーモデルを格納
    public GameObject[] playerModel;

    //銃ホルダー(自分用)
    public GunScript[] gunsHolder;

    //銃ホルダー(他人用)
    public GunScript[] otherGunsHolder;

    //最大HP
    public int maxHP = 100;

    //effect
    public GameObject hitEffect;

    //現在HP
    [SerializeField]
    private int currentHP;

    //保存する位置情報数
    [SerializeField] 
    private int maxRecallData = 5;
    //保存する時間間隔
    [SerializeField] 
    private float secondsBetweenData = 1.0f;
    // 間隔
    [SerializeField] 
    private float recallDuration = 1.25f;
    private bool canCollectRecallData = true;
    private float currentDataTimer = 0f;

    [System.Serializable]
    private class RecallData
    {
        public Vector3 pos;
        public Quaternion rot;
        public Quaternion camRot;
    }
    [SerializeField] private List<RecallData> recallData = new List<RecallData>();
    //recallできるかどうか(1ラウンド1回想定のためbool)
    private bool canRecall = false;
    //Playerのステートマシン
    public enum PlayerState
    {
        Wait,
        Play,
        Recall,
        Dash,
        ADS,
        Dead,
    }

    private PlayerState playerState = PlayerState.Wait;
    public PlayerState _PlayerState => playerState;


    private void Awake()
    {
        uiManager = GameObject.FindGameObjectWithTag("UIManager").GetComponent<UIManager>();
    }


    private void Start()
    {
        Initailize();

        CursorLock();
    }

    public void Initailize()
    {
        playerCam = Camera.main;

        rb = GetComponent<Rigidbody>();

        uiManager.SetBulletText(ammoClip[gunIndex], ammunition[gunIndex]);

        //銃を扱うリストの初期化
        guns.Clear();

        //モデルや銃の表示切替
        if (photonView.IsMine)
        {
            //3Dモデル非表示
            foreach(var model in playerModel)
            {
                model.SetActive(false);
            }

            //表示する方の銃を設定
            foreach(GunScript gun in gunsHolder)
            {
                guns.Add(gun);
            }
        }
        else
        {
            //表示する方の銃を設定
            foreach (GunScript gun in otherGunsHolder)
            {
                guns.Add(gun);
            }
        }
        //銃を表示z
        //SwitchGun();

        photonView.RPC("SetGun", RpcTarget.All, gunIndex);

        //現在HPに最大HP代入
        currentHP = maxHP;

        //HPをスライダーに表示
        uiManager.UpdateHP(maxHP, currentHP);
        //リコール用データ集める
        canCollectRecallData = true;
        //recall可能にする
        canRecall = true;
        //recall可能表示UI
        uiManager.ApplyRecallImageAlphaValue(canRecall);
    }

    private void Update()
    {
        //自キャラだけ操作
        if (!photonView.IsMine)
        {
            return;
        }

        if (playerState == PlayerState.Wait) return;
        StoreRecallData();
        for (int i = 0; i < recallData.Count - 1; i++)
        {
            Debug.DrawLine(recallData[i].pos, recallData[i + 1].pos);
        }

        RecallInput();
        AnimatorSet();
        if (playerState == PlayerState.Recall) return;
        PlayerRotate();
        PlayerMove();
        if (IsOnGround())
        {
            Run();//ADS中に走れないように   
            Jump();
        }
        CursorLock();
        SwitchingGuns();
        if (playerState != PlayerState.Dash)
        {
            Aim();//Dash中にAimできなくする
        }
        Fire();
        Reload();
        Debug.Log(playerState);
    }

    public void FixedUpdate()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        uiManager.SetBulletText(ammoClip[gunIndex], ammunition[gunIndex]);
    }


    public void LateUpdate()
    {
        if (!photonView.IsMine)
        {
            return;
            return;
        }

        CameraControll();
    }

    public void PlayerRotate()
    {
        mouseInput = new Vector2(Input.GetAxisRaw("Mouse X") * mouseSensitivity, 
            Input.GetAxisRaw("Mouse Y") * mouseSensitivity);

        transform.rotation = Quaternion.Euler(transform.eulerAngles.x,
            transform.eulerAngles.y + mouseInput.x,
            transform.eulerAngles.z);

        verticalMouseInput += mouseInput.y;
        verticalMouseInput = Mathf.Clamp(verticalMouseInput, -50f, 50f);

        viewPoint.rotation = Quaternion.Euler(-verticalMouseInput,
            viewPoint.transform.rotation.eulerAngles.y,
            viewPoint.transform.rotation.eulerAngles.z);
    }


    public void PlayerMove()
    {
        moveInput = new Vector3(Input.GetAxisRaw("Horizontal"),
            0,
            Input.GetAxisRaw("Vertical"));

        moveDirection = ((transform.forward * moveInput.z) + (transform.right * moveInput.x)).normalized;

        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    public bool IsOnGround()
    {
        return Physics.Raycast(groundCheckPoint.position, Vector3.down, 0.30f, groundLayers);//ここがうまく動いていない？
    }

    public void Jump()
    {
        if(IsOnGround() && Input.GetKeyDown(KeyCode.Space)) rb.AddForce(jumpForce, ForceMode.Impulse);
    }

    public void Run()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if(playerState == PlayerState.ADS) ResetAim();
            playerState = PlayerState.Dash;
            moveSpeed = dashSpeed;
        }

        else
        {
            playerState = PlayerState.Play;
            moveSpeed = defaultMoveSpeed;
        }
    }

    public void CursorLock()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) isCursorAppear = true; 
        else if (Input.GetMouseButton(0)) isCursorAppear = false;

        if (!isCursorAppear) Cursor.lockState = CursorLockMode.Locked;
        else Cursor.lockState = CursorLockMode.None;
    }

    public void CameraControll()
    {
        playerCam.transform.position = viewPoint.position;
        playerCam.transform.rotation = viewPoint.rotation;
    }

    public void SwitchingGuns()
    {
        if(Input.GetAxisRaw("Mouse ScrollWheel") > 0.1f)
        {
            uiManager.CloseReloadText();

            gunIndex++;

            if (gunIndex >= guns.Count) gunIndex = 0;

            //SwitchGun();
            photonView.RPC("SetGun", RpcTarget.All, gunIndex);
        }
        else if(Input.GetAxisRaw("Mouse ScrollWheel") < -0.1f)
        {
            uiManager.CloseReloadText();

            gunIndex--;

            if (gunIndex < 0) gunIndex = guns.Count - 1;

            //SwitchGun();
            photonView.RPC("SetGun", RpcTarget.All, gunIndex);
        }
        for (int i = 0; i < guns.Count; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))//ループの数値＋１をして文字列に変換。その後、押されたか判定
            {
                uiManager.CloseReloadText();
                gunIndex = i;//銃を扱う数値を設定

                //実際に武器を切り替える関数
                //SwitchGun();
                photonView.RPC("SetGun", RpcTarget.All, gunIndex);
            }
        }
        uiManager.ApplyGunImageAlphaValue(gunIndex);
    }

    public void SwitchGun()
    {
        foreach(GunScript gun in guns)
        {
            gun.gameObject.SetActive(false);
        }

        guns[gunIndex].gameObject.SetActive(true);
    }
    public void Aim()
    {
        if (Input.GetMouseButton(1))
        {
            playerState = PlayerState.ADS;
            playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, guns[gunIndex].adsZoom, guns[gunIndex].adsSpeed * Time.deltaTime);
        }
        else
        {
            playerState = PlayerState.Play;
            playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, fov, guns[gunIndex].adsSpeed * Time.deltaTime);
        }
    }

    private void ResetAim()
    {
        playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, fov, guns[gunIndex].adsSpeed * Time.deltaTime);
    }

    public void Fire()
    {
        if (Input.GetMouseButton(0) && ammoClip[gunIndex] > 0 && Time.time > shotTimer && !guns[gunIndex].isReloading)
        {
            FiringBullet();
        }
        if(ammoClip[gunIndex] <= 0)
        {
            uiManager.ShowReloadText();
        }
    }

    private void FiringBullet()
    {
        ammoClip[gunIndex]--;

        Ray ray = playerCam.ViewportPointToRay(new Vector2(.5f, .5f));

        if(Physics.Raycast(ray, out RaycastHit hit))
        {
            //当たったオブジェクトは、hit.collider.gameobject
            //playerに当たったらダメージ処理
            //そうでなければ、弾痕を生成
            if(hit.collider.gameObject.tag == "Player")
            {
                PhotonNetwork.Instantiate(hitEffect.name,hit.point,Quaternion.identity);
                hit.collider.gameObject.GetPhotonView().RPC("Hit",
                    RpcTarget.All,
                    guns[gunIndex].shootDamage,
                    photonView.Owner.NickName,
                    PhotonNetwork.LocalPlayer.ActorNumber);
            }
            else
            {
                //弾痕を当たった場所に生成する
                GameObject bulletImpactObject = Instantiate(guns[gunIndex].bulletImpact,
                    hit.point + (hit.normal * .002f),
                    Quaternion.LookRotation(hit.normal, Vector3.up));

                Destroy(bulletImpactObject, 10f);
            }
        }

        shotTimer = Time.time + guns[gunIndex].shootInterval;

        photonView.RPC("SoundGenerate", RpcTarget.All);
    }

    public void Reload()
    {
        if (Input.GetKeyDown(KeyCode.R) && !guns[gunIndex].isReloading)
        {
            uiManager.CloseReloadText();
            int reloadGunIndex = gunIndex;
            int amountNeed = maxAmmoClip[reloadGunIndex] - ammoClip[reloadGunIndex];//今のマガジンの必要な段数
            int amountAvailable = amountNeed <= ammunition[reloadGunIndex] ? amountNeed : ammunition[reloadGunIndex];//
            if (amountAvailable != 0 && ammunition[reloadGunIndex] != 0)
            {
                uiManager.SetActiveReloadingText(reloadGunIndex, true);
                guns[reloadGunIndex].isReloading = true;
                StartCoroutine(ReplenishmentGunAmmo(reloadGunIndex, amountNeed, amountAvailable));
            }
        }
    }

    private IEnumerator ReplenishmentGunAmmo(int reloadGunIndex, int amountNeed, int amountAvailable)
    {
        yield return new WaitForSeconds(guns[reloadGunIndex].reloadTime);
        ammunition[reloadGunIndex] -= amountAvailable;
        ammoClip[reloadGunIndex] += amountAvailable;
        uiManager.SetActiveReloadingText(reloadGunIndex, false);
        guns[reloadGunIndex].isReloading = false;
    }

    public void AnimatorSet()
    {
        //walk判定
        if (moveDirection != Vector3.zero)
        {
            animator.SetBool("walk", true);
        }
        else
        {
            animator.SetBool("walk", false);
        }

        //run判定
        if (Input.GetKey(KeyCode.LeftShift))
        {
            animator.SetBool("run", true);
        }
        else
        {
            animator.SetBool("run", false);
        }
    }

    //リモート呼び出し可能な銃切り替え関数
    [PunRPC]//他のユーザーからも呼び出せるようになる
    public void SetGun(int gunNo)
    {
        if(gunNo < guns.Count)
        {
            gunIndex = gunNo;

            SwitchGun();
        }
    }

    //被弾関数
    [PunRPC]
    public void Hit(int damage, string name, int actor)
    {
        //HPを減らす関数
        ReceiveDamage(name, damage, actor);
    }

    //HPを減らす関数
    public void ReceiveDamage(string name, int damage, int actor)
    {
        if (photonView.IsMine)
        {
            //HPを減らす
            currentHP -= damage;

            //0以下になったか判定をする
            if(currentHP <= 0)
            {
                //死亡関数
                Death();
            }

            uiManager.UpdateHP(maxHP, currentHP);
        }
    }

    public void Death()
    {
        currentHP = 0;
        playerState = PlayerState.Dead;
    }

    public override void OnDisable()
    {
        //マウス表示
        isCursorAppear = true;
        Cursor.lockState = CursorLockMode.None;
    }

    //音を鳴らす関数
    [PunRPC]
    public void SoundGenerate()
    {
        guns[gunIndex].SoundGunShot();
    }

    public void WaittoPlay()
    {
        playerState = PlayerState.Play;
    }
    
    private void RecallInput()
    {
        if (Input.GetKeyDown(KeyCode.E) && canRecall)
        {
            canRecall = false;
            uiManager.ApplyRecallImageAlphaValue(canRecall);
            StartCoroutine(Recall());
        }
    }
    
    private IEnumerator Recall()
    {
        playerState = PlayerState.Recall;
        canCollectRecallData = false;
        
        //ADSしているならReset
        ResetAim();

        float secondsForEachData = recallDuration / recallData.Count;
        Vector3 currentDataPlayerStartPos = transform.position;
        Quaternion currentDataPlayerStartRot = transform.rotation;
        Quaternion currentDataCamaraStartRot = playerCam.transform.rotation;

        while (recallData.Count > 0)
        {
            float t = 0f;
            while (t < secondsForEachData)
            {
                transform.position = Vector3.Lerp(currentDataPlayerStartPos,
                    recallData[recallData.Count - 1].pos,
                    t / secondsForEachData);
                transform.rotation = Quaternion.Lerp(currentDataPlayerStartRot,
                    recallData[recallData.Count - 1].rot,
                    t / secondsForEachData);
                playerCam.transform.rotation = Quaternion.Lerp(currentDataCamaraStartRot,
                    recallData[recallData.Count - 1].camRot,
                    t / secondsForEachData);
                t += Time.deltaTime;

                yield return null;
            }

            currentDataPlayerStartPos = recallData[recallData.Count - 1].pos;
            currentDataPlayerStartRot = recallData[recallData.Count - 1].rot;
            currentDataCamaraStartRot = recallData[recallData.Count - 1].camRot;
            
            recallData.RemoveAt(recallData.Count - 1);
        }
        
        playerState = PlayerState.Play;
        canCollectRecallData = true;
    }
    private void StoreRecallData()
    {
        currentDataTimer += Time.deltaTime;

        if (canCollectRecallData)
        {
            if (currentDataTimer >= secondsBetweenData)
            {
                if (recallData.Count >= maxRecallData)
                {
                    recallData.RemoveAt(0);
                }
                recallData.Add(GetRecallData());
                currentDataTimer = 0;
            }
        }
    }

    private RecallData GetRecallData()
    {
        return new RecallData()
        {
            pos = transform.position,
            rot = transform.rotation,
            camRot = playerCam.transform.rotation
        };
    }
}
