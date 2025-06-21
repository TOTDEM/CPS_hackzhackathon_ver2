using UnityEngine;
using UnityEngine.UI; // UIコンポーネントを使用するために必要
using System.Collections;

public class ArcTimer : MonoBehaviour
{
    public float totalTime = 90.0f; // タイマーの合計時間（秒）
    private float currentTime; // 現在の残り時間

    private Image timerImage; // Imageコンポーネント

    void Start()
    {
        timerImage = GetComponent<Image>();
        if (timerImage == null)
        {
            Debug.LogError("Imageコンポーネントが見つかりません。このスクリプトはImageコンポーネントを持つGameObjectにアタッチしてください。");
            enabled = false; // スクリプトを無効にする
            return;
        }

        // Image TypeをFilledに設定
        timerImage.type = Image.Type.Filled;
        timerImage.fillMethod = Image.FillMethod.Radial360; // ここを修正！
        timerImage.fillOrigin = (int)Image.Origin360.Top; // 上から開始
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
                gameObject.SetActive(false);
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