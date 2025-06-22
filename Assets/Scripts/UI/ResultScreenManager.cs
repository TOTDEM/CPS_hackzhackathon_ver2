// ファイル名: ResultScreenManager.cs
using UnityEngine;
using UnityEngine.UI; // UI要素を操作するために必要
using TMPro;

public class ResultScreenManager : MonoBehaviour
{
    [SerializeField] private TMP_Text resultText; // 結果表示用テキスト
    [SerializeField] private Button returnToTitleButton; // タイトルに戻るボタン

    // シーン遷移とデータ管理のマネージャーへの参照
    [SerializeField] private SceneTransitionManager sceneTransitionManager;
    [SerializeField] private GameDataManager gameDataManager;

    void Start()
    {
        // マネージャーのインスタンスを取得
        sceneTransitionManager = SceneTransitionManager.Instance;
        gameDataManager = GameDataManager.Instance;

        if (sceneTransitionManager == null)
        {
            Debug.LogError("SceneTransitionManagerが見つかりません．");
            return;
        }
        if (gameDataManager == null)
        {
            Debug.LogError("GameDataManagerが見つかりません．");
            return;
        }

        // ボタンのリスナー設定
        if (returnToTitleButton != null)
        {
            returnToTitleButton.onClick.AddListener(OnReturnToTitleButtonClick);
        }

        // GameDataManagerから結果を取得し、テキストに表示
        if (resultText != null)
        {
            if (gameDataManager.IsBattleSuccess)
            {
                resultText.text = "ゲームクリア！\nおめでとう！";
                Debug.Log("リザルト画面：バトル成功");
            }
            else
            {
                resultText.text = "ゲームオーバー...\n残念！";
                Debug.Log("リザルト画面：バトル失敗");
            }
        }
    }

    /// <summary>
    /// 「タイトルに戻る」ボタンが押されたときに呼ばれるメソッド
    /// </summary>
    public void OnReturnToTitleButtonClick()
    {
        Debug.Log("タイトルに戻るボタンが押されました．スタート画面へ遷移します．");
        if (sceneTransitionManager != null)
        {
            sceneTransitionManager.LoadStartScene(); // Startシーンへ戻る
        }
    }
}