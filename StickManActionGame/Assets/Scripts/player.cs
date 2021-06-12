using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class player : MonoBehaviour
{
    #region // インスペクターで設定
    [Header("移動速度")]public float speed;
    [Header("ジャンプ速度")]public float jumpSpeed;
    [Header("ジャンプ高さ")]public float jumpHeight;
    [Header("ジャンプする長さ")]public float jumpLimitTime;
    [Header("踏みつけ判定の高さの割合")]public float stepOnRate;
    [Header("重力")]public float gravity;
    [Header("接地判定")]public GroundCheck ground;
    [Header("頭をぶつけた判定")]public GroundCheck head;
    [Header("ダッシュ加減速")]public AnimationCurve dashCurve;
    [Header("ジャンプ加減速")]public AnimationCurve jumpCurve;
    #endregion

    #region // プライベート変数
    private Animator anim = null;
    private Rigidbody2D rb = null;
    private CapsuleCollider2D capcol = null;
    private bool isGround = false;
    private bool isHead = false;
    private bool isJump = false;
    private bool isRun = false;
    private bool isLose = false;
    private bool isOtherJump = false;
    private float jumpPos = 0.0f;
    private float otherJumpHeight = 0.0f;
    private float jumpTime = 0.0f;
    private float dashTime = 0.0f;
    private float beforeKey = 0.0f;
    private string enemyTag = "Enemy";
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        // コンポーネントのインスタンスを捕まえる
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        capcol = GetComponent<CapsuleCollider2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isLose)
        {
        // 接地判定を受け取る
        isGround = ground.IsGround();
        isHead = head.IsGround();

        // 各種座標軸の速度を求める
        
        float ySpeed = GetYSpeed();
        float xSpeed = GetXSpeed();

        // アニメーション適用
        SetAnimation();

    // 移動速度を設定
        rb.velocity = new Vector2(xSpeed, ySpeed);
        }
        else
        {
            rb.velocity = new Vector2(0, -gravity);
        }
    }

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
            dashTime = 0.0f;
            xSpeed = 0.0f;
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
        beforeKey = horizontalKey;

        // アニメーションカーブ適用
        xSpeed *= dashCurve.Evaluate(dashTime);

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

 private void OnCollisionEnter2D(Collision2D collision) 
    {
        if (collision.collider.tag == enemyTag)
        {
            foreach (ContactPoint2D p in collision.contacts)
            {
                // 踏みつけ判定になる高さ
                float stepOnHeight = (capcol.size.y * (stepOnRate / 100f));

                // 踏みつけ判定のワールド座標
                float judgePos = transform.position.y - (capcol.size.y / 2f) + stepOnHeight;

                if (p.point.y < judgePos ) // 衝突した位置が自分の中心より下だったら
                {
                    // もう一度跳ねる
                    ObjectCollision o = collision.gameObject.GetComponent<ObjectCollision>();
                    if (o != null)
                    {
                        otherJumpHeight = o.boundHeight;
                        o.playerStepOn = true;
                        jumpPos =transform.position.y;
                        isOtherJump = true;
                        isJump = false;
                        jumpTime = 0.0f;
                    }
                }
                else
                {
                    // ダウン
                    anim.Play("player_lose");
                    isLose = true;
                    break;
                }
            }
        }
    }
}
