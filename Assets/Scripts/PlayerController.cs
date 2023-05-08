using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPunCallbacks
{
    public Transform viewPoint;
    public float mouseSensitivity = 1.0f;
    private Vector2 mouseInput;
    private float verticalMouseInput;
    private Camera playerCam;
    public float fov = 60f;

    private Vector3 moveInput;
    private Vector3 moveDirection;
    public float moveSpeed = 4.0f;

    public Vector3 jumpForce = new Vector3(0, 5, 0);
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

    UIManager uiManager;

    SpawnManager spawnManager;

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

    //GameManager
    GameManager gameManager;


    private void Awake()
    {
        uiManager = GameObject.FindGameObjectWithTag("UIManager").GetComponent<UIManager>();

        spawnManager = GameObject.FindGameObjectWithTag("SpawnManager").GetComponent<SpawnManager>();

        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
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

        //transform.position = spawnManager.GetSpawnPoint().position;

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
    }

    private void Update()
    {
        //自キャラだけ操作
        if (!photonView.IsMine)
        {
            return;
        }

        PlayerRotate();

        PlayerMove();

        if (IsOnGround())
        {
            Run();

            Jump();
        }

        CursorLock();

        SwitchingGuns();

        Aim();

        Fire();

        Reload();

        AnimatorSet();

        if(Input.GetMouseButtonUp(0) || ammoClip[1] <= 0)
        {
            photonView.RPC("SoundStop", RpcTarget.All);
        }
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
        return Physics.Raycast(groundCheckPoint.position, Vector3.down, 0.20f, groundLayers);
    }

    public void Jump()
    {
        if(IsOnGround() && Input.GetKeyDown(KeyCode.Space)) rb.AddForce(jumpForce, ForceMode.Impulse);
    }

    public void Run()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift)) moveSpeed = 8.0f;

        else moveSpeed = 4.0f;
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
            Debug.Log("スクロールup");

            gunIndex++;

            if (gunIndex >= guns.Count) gunIndex = 0;

            //SwitchGun();
            photonView.RPC("SetGun", RpcTarget.All, gunIndex);
        }
        else if(Input.GetAxisRaw("Mouse ScrollWheel") < -0.1f)
        {
            uiManager.CloseReloadText();
            Debug.Log("スクロールdown");

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
            playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, guns[gunIndex].adsZoom, guns[gunIndex].adsSpeed * Time.deltaTime);
        }
        else
        {
            playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, fov, guns[gunIndex].adsSpeed * Time.deltaTime);
        }
    }

    public void Fire()
    {
        if (Input.GetMouseButton(0) && ammoClip[gunIndex] > 0 && Time.time > shotTimer)
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
        if (Input.GetKeyDown(KeyCode.R))
        {
            uiManager.CloseReloadText();
            int amountNeed = maxAmmoClip[gunIndex] - ammoClip[gunIndex];
            int amountAvailable = amountNeed <= ammunition[gunIndex] ? amountNeed : ammunition[gunIndex];

            if(amountAvailable != 0 && ammunition[gunIndex] != 0)
            {
                ammunition[gunIndex] -= amountAvailable;
                ammoClip[gunIndex] += amountAvailable;
            }
        }
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
                Death(name,actor);
            }

            uiManager.UpdateHP(maxHP, currentHP);
        }
    }

    public void Death(string name,int actor)
    {
        currentHP = 0;

        uiManager.UpdateDeathUI(name);

        spawnManager.Die();

        gameManager.ScoreGet(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1);//自分死亡時のイベント呼び出し

        gameManager.ScoreGet(actor, 0, 1);
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
        if(gunIndex == 1)
        {
            guns[gunIndex].LoopOnARGun();
        }
        else
        {
            guns[gunIndex].SoundGunShot();
        }
    }

    //音を止める関数
    [PunRPC]
    public void SoundStop()
    {
        guns[1].LoopOFF_ARGun();
    }
}
