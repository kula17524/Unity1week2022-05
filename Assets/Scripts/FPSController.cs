using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSController : MonoBehaviour
{
    // 移動用の座標
    float x, z;
    // スピード調整
    float speed = 0.1f;

    // カメラ
    public GameObject cam;
    public GameObject subcam;
    Quaternion cameraRot, characterRot;
    // 視点の角度制限
    float minX = -90f, maxX = 90f;

    // マウス感度の調整
    float Xsensityvity = 3f, Ysensityvity = 3f;
    // カーソルの非表示(true=非表示)
    bool cursorLock = true;

    // アニメーション
    public Animator animator;

    // 所持弾薬
    int ammunition = 50, maxAmmunition = 50;
    // マガジン内の弾数
    int ammoClip = 10, maxAmmoClip = 10;

    // 体力
    int playerHP = 100, maxPlayerHP = 100;
    // 体力バー
    public Slider hpBer;
    // 弾薬テキスト
    public Text ammoText;

    // サウンド
    public AudioSource playerFootStep;
    public AudioClip WalkFootStepSE, RunFootStepSE;


    // Start is called before the first frame update
    void Start()
    {
        // 最初にカメラとキャラクターの向きを取得
        cameraRot = cam.transform.localRotation;
        characterRot = transform.localRotation;

        // 銃撃できるようにする
            GameState.canShoot = true;

            // 体力の反映
            hpBer.value = playerHP;
            // 弾薬テキストの反映
            ammoText.text = ammoClip + "/" + ammunition;
    }

    // Update is called once per frame
    void Update()
    {
        // マウス感度の調整
        float xRot = Input.GetAxis("Mouse X") * Ysensityvity;
        float yRot = Input.GetAxis("Mouse Y") * Xsensityvity;

        cameraRot *= Quaternion.Euler(-yRot, 0, 0);
        characterRot *= Quaternion.Euler(0, xRot, 0);

        // 関数を使って視点制限
        cameraRot = ClampRotation(cameraRot);
        
        cam.transform.localRotation = cameraRot;
        transform.localRotation = characterRot;

        // カーソルの表示・非表示の関数呼び出し
        UpdateCursorLock();

        // アニメーション
        // 射撃
        if (Input.GetMouseButton(0) && GameState.canShoot)
        {
            // マガジンに弾があるかどうか
            if (ammoClip > 0)
            {
                animator.SetTrigger("Fire");
                // 連続でアニメーション再生されるのを防ぐ
                GameState.canShoot = false;
                // 弾数を減らす
                ammoClip--;
                ammoText.text = ammoClip + "/" + ammunition;
            }
            else
            {
                Weapon.instance.TriggerSE();
            }
            
        }
        // リロード
        if (Input.GetKeyDown(KeyCode.R))
        {
            // マガジンに補充する弾数
            int amountNeed = maxAmmoClip - ammoClip;
            // 実際に補充する弾数(持ち弾数と比較する)
            int ammoAvailable = amountNeed < ammunition ? amountNeed : ammunition;

            // 弾薬が満タンかつ弾数も満タン
            if (amountNeed != 0 && ammunition != 0)
            {
                animator.SetTrigger("Reload");
                ammunition -= ammoAvailable;
                ammoClip += ammoAvailable;
                ammoText.text = ammoClip + "/" + ammunition;
            }

            
        }
        // 歩く
        // 前後移動に対応するため絶対値で判定(Mathf.Abs)
        if (Mathf.Abs(x) > 0 || Mathf.Abs(z) > 0)
        {
            if (!animator.GetBool("Walk"))
            {
                animator.SetBool("Walk", true);
                PlayerWalkFootStep(WalkFootStepSE);
            }
        }
        else if (animator.GetBool("Walk"))
        {
            animator.SetBool("Walk", false);
            StopFootStep();
        }
        // 走る
        // 後ろ向き移動の時は走らせない
        if (z > 0 && Input.GetKey(KeyCode.LeftShift))
        {
            if (!animator.GetBool("Run"))
            {
                animator.SetBool("Run", true);
                speed = 0.25f;

                PlayerRunFootStep(RunFootStepSE);
            }
        }
        else if (animator.GetBool("Run"))
        {
            animator.SetBool("Run", false);
            speed = 0.1f;

            StopFootStep();
        }

        // カメラの交代
        if (Input.GetMouseButton(1))
        {
            subcam.SetActive(true);
            cam.GetComponent<Camera>().enabled = false;
            Weapon.instance.TriggerSE();
        }
        else if (subcam.activeSelf)
        {
            subcam.SetActive(false);
            cam.GetComponent<Camera>().enabled = true;
        }
    }

    private void FixedUpdate()
    {
        // 座標を0とする
        x = 0;
        z = 0;

        // マウスの入力に応じて移動
        x = Input.GetAxisRaw("Horizontal") * speed;
        z = Input.GetAxisRaw("Vertical") * speed;
        // カメラの向きを正面として移動
        transform.position += cam.transform.forward * z + cam.transform.right * x;
    }

    // カーソルの表示設定用関数
    public void UpdateCursorLock()
    {
        //カーソルの表示・非表示
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            cursorLock = false;
        }
        else if(Input.GetMouseButton(0))
        {
            cursorLock = true;
        }

        if(cursorLock)
        {
            Cursor.lockState = CursorLockMode.Locked; 
        }
        else if(!cursorLock)
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    // 視点の角度制限用関数
    // void(返り値なし)でなく、Quaternion型の返り値を返す
    public Quaternion ClampRotation(Quaternion q)
    {
        //qのx, y, zは量と向きを持つ(座標)
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        //qのwは量しか持たない(回転量)
        q.w = 1f;

        // オイラー角に変換
        float angleX = Mathf.Atan(q.x) * Mathf.Rad2Deg * 2f;
        // 視点制限
        angleX = Mathf.Clamp(angleX, minX, maxX);
        // 角度を反映
        q.x = Mathf.Tan(angleX * Mathf.Deg2Rad * 0.5f);

        return q;
    }

    // 足音
    public void PlayerWalkFootStep(AudioClip clip)
    {
        playerFootStep.loop = true;
        playerFootStep.pitch = 1f;
        playerFootStep.clip = clip;
        playerFootStep.Play();
    }
    public void PlayerRunFootStep(AudioClip clip)
    {
        playerFootStep.loop = true;
        playerFootStep.pitch = 1.3f;
        playerFootStep.clip = clip;
        playerFootStep.Play();
    }
    public void StopFootStep()
    {
        playerFootStep.Stop();
        playerFootStep.loop = false;
        playerFootStep.pitch = 1f;
    }

    //体力管理
    public void TakeHit(float damage)
    {
        playerHP = (int)Mathf.Clamp(playerHP - damage, 0, playerHP);

        hpBer.value = playerHP;

        if (playerHP <= 0 && !GameState.GameOver)
        {
            GameState.GameOver = true;
        }
    }
}
