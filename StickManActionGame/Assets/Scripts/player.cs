using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    #region // インスペクターで設定
    [Header("移動速度")] public float speed;
    [Header("ジャンプ速度")] public float jumpSpeed;
    [Header("ジャンプ高さ")] public float jumpHeight;
    [Header("ジャンプする長さ")] public float jumpLimitTime;
    [Header("踏みつけ判定の高さの割合")] public float stepOnRate;
    [Header("重力")] public float gravity;
    [Header("接地判定")] public GroundCheck ground;
    [Header("頭をぶつけた判定")] public GroundCheck head;
    [Header("ダッシュ加減速")] public AnimationCurve dashCurve;
    [Header("ジャンプ加減速")] public AnimationCurve jumpCurve;
    [Header("ジャンプの時に鳴らすSE")] public AudioClip jumpSE;
    [Header("ダメージSE")] public AudioClip loseSE;
    [Header("落下SE")] public AudioClip dropSE;
    #endregion

    #region // プライベート変数
    private Animator anim = null;
    private Rigidbody2D rb = null;
    private CapsuleCollider2D capcol = null;
    private SpriteRenderer sr = null;
    private MoveObject moveObj = null;
    private bool isGround = false;
    private bool isHead = false;
    private bool isJump = false;
    private bool isRun = false;
    private bool isLose = false;
    private bool isOtherJump = false;
    private bool isContinue = false;
    private bool nonLoseAnim = false;
    private bool isClearMotion = false;
    private float continueTime = 0.0f;
    private float blinkTime = 0.0f;
    private float jumpPos = 0.0f;
    private float otherJumpHeight = 0.0f;
    private float jumpTime = 0.0f;
    private float dashTime = 0.0f;
    private float beforeKey = 0.0f;
    private string enemyTag = "Enemy";
    private string deadAreaTag = "DeadArea";
    private string hitAreaTag = "HitArea";
    private string moveFloorTag = "MoveFloor";
    private string fallFloorTag = "FallFloor";
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        // コンポーネントのインスタンスを捕まえる
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        capcol = GetComponent<CapsuleCollider2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (isContinue)
        {
            if (blinkTime > 0.2f)
            {
                sr.enabled = true;
                blinkTime = 0.0f;
            }
            else if (blinkTime > 0.1f)
            {
                sr.enabled = false;
            }
            else
            {
                sr.enabled = true;
            }

            if (continueTime > 1.0f)
            {
                isContinue = false;
                blinkTime = 0.0f;
                continueTime = 0.0f;
                sr.enabled = true;
            }
            else
            {
                blinkTime += Time.deltaTime;
                continueTime += Time.deltaTime;
            }
        }
    }

    #region //fixedupdate
    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isLose && !GManager.instance.isGameOver && !GManager.instance.isStageClear)
        {
            // 接地判定を受け取る
            isGround = ground.IsGround();
            isHead = head.IsGround();

            // 各種座標軸の速度を求める

            float xSpeed = GetXSpeed();
            float ySpeed = GetYSpeed();


            // アニメーション適用
            SetAnimation();

            // 移動速度を設定
            Vector2 addVelocity = Vector2.zero;
            if(moveObj != null)
            {
                addVelocity = moveObj.GetVelocity();
            }
            rb.velocity = new Vector2(xSpeed, ySpeed) + addVelocity;
        }
        else
        {
            if (!isClearMotion && GManager.instance.isStageClear)
            {
                anim.Play("player_clear");
                isClearMotion = true;
            }
            rb.velocity = new Vector2(0, -gravity);
        }
    }
    #endregion

    /// <summary>
    /// Y成分の計算をし、速度を返す
    /// </summary>
    /// <returns>Y軸速度</returns>
    private float GetYSpeed()
    {
        float verticalKey = Input.GetAxis("Vertical");
        float ySpeed = -gravity;

        // ジャンプ中
        if (isOtherJump)
        {
            // 現在の高さが飛べる高さより下か
            bool canHeight = jumpPos + otherJumpHeight > transform.position.y;
            // ジャンプ時間が長くないか
            bool canTime = jumpLimitTime > jumpTime;
            if (canHeight && canTime && !isHead)
            {
                ySpeed = jumpSpeed;
                jumpTime += Time.deltaTime;
            }
            else
            {
                isOtherJump = false;
                jumpTime = 0.0f;
            }
        }
        // 地面にいる時
        else if (isGround)
        {
            if (verticalKey > 0)
            {
                if  (!isJump)
                {
                    GManager.instance.PlaySE(jumpSE);
                }
                ySpeed = jumpSpeed;
                jumpPos = transform.position.y; //ジャンプした位置を記録
                isJump = true;
                jumpTime = 0.0f;
            }
            else
            {
                isJump = false;
            }
        }
        // ジャンプ中
        else if (isJump)
        {
            // 上キーを押しているか
            bool pushUpKey = verticalKey > 0;
            // 現在の高さが飛べる高さより下か
            bool canHeight = jumpPos + jumpHeight > transform.position.y;
            // ジャンプ時間が長くないか
            bool canTime = jumpLimitTime > jumpTime;
            if (pushUpKey && canHeight && canTime && !isHead)
            {
                ySpeed = jumpSpeed;
                jumpTime += Time.deltaTime;
            }
            else
            {
                isJump = false;
                jumpTime = 0.0f;
                GManager.instance.PlaySE(dropSE);
            }
        }

        // アニメーションカーブ適用
        if (isJump || isOtherJump)
        {
            ySpeed *= jumpCurve.Evaluate(jumpTime);
        }

        return ySpeed;
    }

    /// <summary>
    /// X成分の計算をし、速度を返す
    /// </summary>
    /// <returns>X軸速度</returns>
    private float GetXSpeed()
    {
        float horizontalKey = Input.GetAxis("Horizontal");

        float xSpeed = 0.0f;

                if(horizontalKey > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
            isRun = true;
            dashTime += Time.deltaTime;
            xSpeed = speed;
        }
        else if(horizontalKey < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            isRun = true;
            dashTime += Time.deltaTime;
            xSpeed = -speed;
        }
        else
        {
            isRun = false;
            xSpeed = 0.0f;
            dashTime = 0.0f;
        }

        // 前回の入力から反転したらリセット
        if (horizontalKey > 0 && beforeKey < 0)
        {
            dashTime = 0.0f;
        }
        else if(horizontalKey < 0 && beforeKey > 0)
        {
            dashTime = 0.0f;
        }
        xSpeed *= dashCurve.Evaluate(dashTime);
        beforeKey = horizontalKey;
        return xSpeed;
    }

    /// <summary>
    /// アニメーションを設定する
    /// </summary>
    private void SetAnimation()
    {
        anim.SetBool("jump", isJump || isOtherJump);
        anim.SetBool("ground", isGround);
        anim.SetBool("run", isRun);
    }

    public bool IsContinueWaiting()
    {
        if (GManager.instance.isGameOver)
        {
            return false;
        }
        else
        {
        return IsDownAnimEnd() || nonLoseAnim;
        }
    }

    // ダウンアニメーションが完了しているか
    private bool IsDownAnimEnd()
    {
        if (isLose && anim != null)
        {
            AnimatorStateInfo currentState = anim.GetCurrentAnimatorStateInfo(0);
            if (currentState.IsName("player_lose"))
            {
                if (currentState.normalizedTime >= 1)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void ContinuePlayer()
    {
        isLose = false;
        anim.Play("player_stand");
        isJump = false;
        isOtherJump = false;
        isRun = false;
        isContinue = true;
        nonLoseAnim = false;
    }

    private void ReceiveDamage(bool loseAnim)
    {
        if (isLose || GManager.instance.isStageClear)
        {
            return;
        }
        else
        {
            if (loseAnim)
            {
                anim.Play("player_lose");
                GManager.instance.PlaySE(loseSE);
            }
            else
            {
                nonLoseAnim = true;
            }
            isLose = true;
            GManager.instance.SubHeartNum();
        }
    }

    #region //接触判定
    private void OnCollisionEnter2D(Collision2D collision) 
    {
        bool enemy = (collision.collider.tag == enemyTag);
        bool moveFloor = (collision.collider.tag == moveFloorTag);
        bool fallFloor = (collision.collider.tag == fallFloorTag);

        if (enemy || moveFloor || fallFloor)
        {
            foreach (ContactPoint2D p in collision.contacts)
            {
                // 踏みつけ判定になる高さ
                float stepOnHeight = (capcol.size.y * (stepOnRate / 100f));

                // 踏みつけ判定のワールド座標
                float judgePos = transform.position.y - (capcol.size.y / 2f) + stepOnHeight;

                if (p.point.y < judgePos ) // 衝突した位置が自分の中心より下だったら
                {
                    if (enemy || fallFloor)
                    {
                        // もう一度跳ねる
                        ObjectCollision o = collision.gameObject.GetComponent<ObjectCollision>();
                        if (o != null)
                        {
                            if (enemy)
                            {
                                otherJumpHeight = o.boundHeight;
                                o.playerStepOn = true;
                                jumpPos =transform.position.y;
                                isOtherJump = true;
                                isJump = false;
                                jumpTime = 0.0f;
                            }
                            else if (fallFloor)
                            {
                                o.playerStepOn = true;
                            }
                        }
                        else
                        {
                            Debug.Log("ObjectCollisionが付いてないよ!");
                        }
                    }
                    else if (moveFloor)
                    {
                        moveObj = collision.gameObject.GetComponent<MoveObject>();
                    }
                }
                else
                {
                    if (enemy)
                    {
                        // ダウン
                        ReceiveDamage(true);
                        break;
                    }
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.tag == moveFloorTag)
        {
            // 動く床から離れた
            moveObj = null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == deadAreaTag)
        {
            ReceiveDamage(false);
        }
        else if (collision.tag == hitAreaTag)
        {
            ReceiveDamage(true);
        }
    }
    #endregion
}