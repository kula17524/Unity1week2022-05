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
    public int count;

    void Start(){
        count = 0;
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

    public void TakePotion()
    {
        playerHP = (int)Mathf.Clamp(playerHP + 10, 0, 100);

        hpBer.value = playerHP;
    }

    public void TakeKey()
    {
        count++;
        if(count == 3) {
            GameState.GameClear = true;
        }
        
    }
}
