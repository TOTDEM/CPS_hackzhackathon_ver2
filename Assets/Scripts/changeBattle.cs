using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;//Unityエンジンのシーン管理プログラムを利用する

public class changeBattle : MonoBehaviour //changeBattleという名前にします
{
    public void change_button_forBattle() //change_button_forBattleという名前にします
    {
        SceneManager.LoadScene("Battle");//Battleを呼び出します
    }
}
