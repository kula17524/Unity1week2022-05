using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSController : MonoBehaviour
{
    // 体力
    int playerHP = 100;
    // 体力バー
    public Slider hpBer;

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
