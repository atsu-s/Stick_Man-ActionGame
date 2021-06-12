using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectCollision : MonoBehaviour
{
    [Header("これを踏んだ時にはねる高さ")]public float boundHeight;

    [HideInInspector]public bool playerStepOn;
}
