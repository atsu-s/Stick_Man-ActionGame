using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeActiveUGUI : MonoBehaviour
{
    [Header("フェードスピード")] public float speed = 1.0f;
    [Header("上昇量")] public float moveDis = 10.0f;
    [Header("上昇時間")] public float moveTime = 1.0f;
    [Header("キャンバスグループ")] public CanvasGroup cg;
    [Header("プレイヤー判定")] public PlayerTriggerCheck trigger;

    private Vector3 defaltPos;
    private float timer = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        //初期化
        if (cg == null && trigger == null)
        {
            Debug.Log("インスペクターの設定が足りません");
            Destroy(this);
        }
        else
        {
            cg.alpha = 0.0f;
            defaltPos = cg.transform.position;
            cg.transform.position = defaltPos - Vector3.up * moveDis;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //プレイヤーが範囲内に入った
        if (trigger.isOn)
        {
            //上昇しながらフェードインする
            if (cg.transform.position.y < defaltPos.y || cg.alpha < 1.0f)
            {
                cg.alpha = timer / moveTime;
                cg.transform.position += Vector3.up * (moveDis / moveTime) * speed * Time.deltaTime;
                timer += speed * Time.deltaTime;
            }
            //フェードイン完了
            else
            {
                cg.alpha = 1.0f;
                cg.transform.position = defaltPos;
            }
        }
        //プレイヤーが範囲内にいない
        else
        {
            //下降しながらフェードアウトする
            if (cg.transform.position.y > defaltPos.y - moveDis || cg.alpha > 0.0f)
            {
                cg.alpha = timer / moveTime;
                cg.transform.position -= Vector3.up * (moveDis / moveTime) * speed * Time.deltaTime;
                timer -= speed * Time.deltaTime;
            }
            //フェードアウト完了
            else
            {
                timer = 0.0f;
                cg.alpha = 0.0f;
                cg.transform.position = defaltPos - Vector3.up * moveDis;
            }
        }
    }
}
