// ファイル名: StartScreenManager.cs
using UnityEngine;
using UnityEngine.UI; // UI要素を操作するために必要

public class StartScreenManager : MonoBehaviour
{
    // シーン遷移を管理するマネージャーへの参照は、シングルトンを使用するため不要です。
    // [SerializeField] private SceneTransitionManager sceneTransitionManager; // この行は削除またはコメントアウト

    // 「バトルへ進む」ボタンの参照 (On Clickイベントをエディタで設定する場合は不要)
    // 今回はエディタで設定するので、このフィールドは必須ではありませんが、
    // 残しておいても動作には影響しません。
    // [SerializeField] private Button goBattleButton; // この行は削除またはコメントアウトしてもOK

    void Start()
    {
        // SceneTransitionManagerのインスタンスがnullではないかだけ確認
        // シングルトンパターンなので、AwakeでInstanceが設定されているはずです。
        if (SceneTransitionManager.Instance == null)
        {
            Debug.LogError("StartScreenManager: SceneTransitionManagerのインスタンスが見つかりません．" +
                           "StartシーンにGameManagersオブジェクトとSceneTransitionManagerが正しく設定されているか確認してください．");
        }

        // goBattleButtonへのリスナー追加は、エディタでOnClickを設定するなら不要です。
        // もしコードから追加するなら、このブロックを有効にします。
        // if (goBattleButton != null)
        // {
        //     goBattleButton.onClick.AddListener(OnGoBattleButtonClick);
        // }
    }

    /// <summary>
    /// 「バトルへ進む」ボタンが押されたときに呼ばれるメソッド
    /// このメソッドをUnityエディタのボタンのOnClickイベントに設定します。
    /// </summary>
    public void OnGoBattleButtonClick()
    {
        Debug.Log("「バトルへ進む」ボタンが押されました．バトル画面へ遷移を要求します．");

        // シングルトンインスタンスを通じて、バトルシーンへの遷移を指示する
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadBattleScene();
        }
        else
        {
            Debug.LogError("StartScreenManager: SceneTransitionManagerのインスタンスが見つからないため，画面遷移できません．");
        }
    }
}