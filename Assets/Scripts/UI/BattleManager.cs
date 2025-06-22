// ファイル名: BattleManager.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BattleManager : MonoBehaviour
{
    [SerializeField] private SceneTransitionManager sceneTransitionManager;
    [SerializeField] private GameDataManager gameDataManager; // GameDataManagerへの参照を追加

    [SerializeField] private Button goToResultButton; // オプション
    [SerializeField] private Text battleStatusText; // オプション

    [SerializeField] private float battleDuration = 5.0f; 

    void Start()
    {
        // SceneTransitionManagerのインスタンスを取得
        if (sceneTransitionManager == null)
        {
            sceneTransitionManager = SceneTransitionManager.Instance;
            if (sceneTransitionManager == null)
            {
                Debug.LogError("SceneTransitionManagerのインスタンスが見つかりません．");
                return;
            }
        }
        // GameDataManagerのインスタンスを取得
        if (gameDataManager == null)
        {
            gameDataManager = GameDataManager.Instance;
            if (gameDataManager == null)
            {
                Debug.LogError("GameDataManagerのインスタンスが見つかりません．");
                // GameDataManagerがないと結果を渡せないので、これも致命的
                return;
            }
        }

        if (goToResultButton != null)
        {
            goToResultButton.onClick.AddListener(OnGoToResultButtonClick);
            goToResultButton.gameObject.SetActive(false);
        }

        if (battleStatusText != null)
        {
            battleStatusText.text = "バトル開始！";
        }

        // バトル終了処理を開始
        StartCoroutine(EndBattleAfterDelay(battleDuration));
    }

    public void OnGoToResultButtonClick()
    {
        Debug.Log("手動でリザルトへ．");
        // 例として、手動ボタンの場合は常に成功と仮定
        // gameDataManager.SetBattleResult(true);
        // sceneTransitionManager.LoadSuccessResultScene();
    }

    private IEnumerator EndBattleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // ここでバトルの勝敗を判定し、GameDataManagerに設定する
        // 例: ランダムで成功か失敗かを決める (本来はゲームロジックに基づく)
        bool didSucceed = Random.Range(0, 2) == 0; // 0なら成功、1なら失敗

        gameDataManager.SetBattleResult(didSucceed);
        Debug.Log($"バトル結果: {(didSucceed ? "成功" : "失敗")}");

        if (battleStatusText != null)
        {
            battleStatusText.text = "バトル終了！リザルトへ...";
        }

        // 結果に基づいて適切なリザルトシーンへ遷移
        if (sceneTransitionManager != null)
        {
            if (didSucceed)
            {
                sceneTransitionManager.LoadSuccessResultScene();
            }
            else
            {
                sceneTransitionManager.LoadFailureResultScene();
            }
        }
    }
}