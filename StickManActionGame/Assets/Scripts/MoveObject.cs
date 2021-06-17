using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObject : MonoBehaviour
{
    [Header("移動経路")] public GameObject[] movePoint;
    [Header("速度")] public float speed = 1.0f;

    private Rigidbody2D rb = null;
    private int nowPoint = 0;
    private bool returnPoint = false;
    private Vector2 oldPos = Vector2.zero;
    private Vector2 myVelocity = Vector2.zero;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (movePoint != null && movePoint.Length > 0 && rb != null)
        {
            rb.position = movePoint[0].transform.position;
            oldPos = rb.position;
        }
    }

    public Vector2 GetVelocity()
    {
        return myVelocity;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (movePoint != null && movePoint.Length > 1 && rb != null)
        {
            // 通常進行
            if (!returnPoint)
            {
                int nextPoint = nowPoint + 1;

                // 目標ポイントに近づくまで移動
                if (Vector2.Distance(transform.position, movePoint[nextPoint].transform.position) > 0.1f)
                {
                    //現在地から次のポイントへのベクトルを作成
                    Vector2 toVector = Vector2.MoveTowards(transform.position, movePoint[nextPoint].transform.position, speed * Time.deltaTime);

                    // 次のポイントへ移動
                    rb.MovePosition(toVector);
                }
                // 次のポイントを１つ進める
                else
                {
                    rb.MovePosition(movePoint[nextPoint].transform.position);
                    ++nowPoint;

                    // 現在地が最後だった場合
                    if (nowPoint + 1 >= movePoint.Length)
                    {
                        returnPoint = true;
                    }
                }
            }
            else
            {
                int nextPoint = nowPoint - 1;

                // 目標ポイントに近づくまで移動
                if (Vector2.Distance(transform.position, movePoint[nextPoint].transform.position) > 0.1f)
                {
                    //現在地から次のポイントへのベクトルを作成
                    Vector2 toVector = Vector2.MoveTowards(transform.position, movePoint[nextPoint].transform.position, speed * Time.deltaTime);

                    // 次のポイントへ移動
                    rb.MovePosition(toVector);
                }
                // 次のポイントを１つ進める
                else
                {
                    rb.MovePosition(movePoint[nextPoint].transform.position);
                    --nowPoint;

                    // 現在地が最初だった場合
                    if (nowPoint　<= 0)
                    {
                        returnPoint = false;
                    }
                }
            }
        }
        myVelocity = (rb.position - oldPos) / Time.deltaTime;
        oldPos = rb.position;
    }
}
