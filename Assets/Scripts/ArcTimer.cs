// ファイル名: ArcTimer.cs (もしファイル名を変更していない場合は、Unityの慣習に従い変更を推奨します)
using UnityEngine;
using UnityEngine.UI; // UIコンポーネントを使用するために必要
using System.Collections;

public class ArcTimer : MonoBehaviour
{
    public float totalTime = 9.0f; // タイマーの合計時間（秒）
    private float currentTime; // 現在の残り時間

    private Image timerImage; // Imageコンポーネント

    // SceneTransitionManagerへの参照を追加
    private SceneTransitionManager sceneTransitionManager;

    void Start()
    {
        timerImage = GetComponent<Image>();
        if (timerImage == null)
        {
            Debug.LogError("Imageコンポーネントが見つかりません。このスクリプトはImageコンポーネントを持つGameObjectにアタッチしてください。");
            enabled = false; // スクリプトを無効にする
            return;
        }

        // SceneTransitionManagerのインスタンスを取得
        sceneTransitionManager = SceneTransitionManager.Instance;
        if (sceneTransitionManager == null)
        {
            Debug.LogError("SceneTransitionManagerのインスタンスが見つかりません．タイマー終了時の画面遷移ができません．");
            // ここでenabled = false; にするとタイマー自体も止まるので注意
            // ただし、画面遷移ができない状態ではゲームが進行しなくなるため、エラーログを出すだけでも良いでしょう。
        }

        // Image TypeをFilledに設定
        timerImage.type = Image.Type.Filled;
        timerImage.fillMethod = Image.FillMethod.Radial360;
        timerImage.fillOrigin = (int)Image.Origin360.Top;
        timerImage.fillClockwise = false; // 時計回りと反対方向に減少

        currentTime = totalTime;
    }

    void Update()
    {
        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;

            // fillAmountを更新 (0.0～1.0)
            timerImage.fillAmount = currentTime / totalTime;

            if (currentTime <= 0)
            {
                currentTime = 0;
                Debug.Log("タイマーが終了しました！");
                // タイマー終了時の処理
                gameObject.SetActive(false); // タイマーImageを非表示にする

                // FailureResultSceneに画面遷移する処理を追加
                if (sceneTransitionManager != null)
                {
                    sceneTransitionManager.LoadFailureResultScene();
                }
                else
                {
                    Debug.LogError("SceneTransitionManagerが利用できないため、FailureResultSceneへの遷移ができません。");
                }
            }
        }
    }

    // タイマーをリセットする public メソッド
    public void ResetTimer()
    {
        currentTime = totalTime;
        timerImage.fillAmount = 1.0f; // 満タンにする
        gameObject.SetActive(true);
    }
}