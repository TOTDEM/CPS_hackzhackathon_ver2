using UnityEngine;
using UnityEngine.UI; // UI要素を扱うために必要
using UnityEngine.Networking; // Webリクエストを扱うために必要
using System.Collections;

// --- このスクリプトの使い方 ---
// 1. Unityで空のGameObjectを作成し、「QuizManager」と名付けます。
// 2. このスクリプトを、作成した「QuizManager」GameObjectにアタッチします。
// 3. 画面に問題文、選択肢A/Bのボタン、解説などを表示するためのUI TextとButtonを作成します。
// 4. インスペクター上で、このスクリプトの各UI参照（questionTextなど）に、作成したUI要素をドラッグ＆ドロップで設定します。
// 5. 選択肢A/BのボタンのOnClick()イベントに、このスクリプトのAnswerButtonPressed("A") / AnswerButtonPressed("B")をそれぞれ設定します。

public class QuizManager : MonoBehaviour
{
    // --- API設定 ---
    // あなたがデプロイしたAPIのURLを設定します
    private const string API_URL = "http://Quizgame-env.eba-ia2gsrsb.ap-southeast-2.elasticbeanstalk.com/quiz";

    // --- UI参照 ---
    // Unityエディタのインスペクターから、対応するUI要素をアタッチしてください
    [Header("UI References")]
    public Text questionText;       // 問題文を表示するText
    public Text choiceAText;        // 選択肢Aのボタン内Text
    public Text choiceBText;        // 選択肢Bのボタン内Text
    public Text resultText;         // 正解/不正解を表示するText
    public Text explanationText;    // 解説文を表示するText

    [Header("UI Buttons")]
    public Button choiceAButton;
    public Button choiceBButton;
    public Button nextQuizButton;   // 次の問題へ進むボタン

    // --- 内部データ ---
    private QuizData currentQuiz; // 現在表示しているクイズのデータ
    private bool isAnswered = false; // 回答済みかどうかのフラグ

    // --- JSONの構造に対応するC#クラス ---
    // JSONをパースするために、キー名と変数名を一致させる必要があります
    [System.Serializable]
    private class QuizData
    {
        public string question;
        public OptionsData options;
        public string explanation_A;
        public string explanation_B;
        public string answer;
    }

    [System.Serializable]
    private class OptionsData
    {
        public string A;
        public string B;
    }


    // --- メイン処理 ---

    void Start()
    {
        // ゲーム開始時に最初のクイズを取得
        FetchNewQuiz();
    }

    /// <summary>
    /// 新しいクイズを取得して画面を更新する
    /// </summary>
    public void FetchNewQuiz()
    {
        // UIを初期状態に戻す
        resultText.text = "";
        explanationText.text = "";
        isAnswered = false;
        SetButtonsInteractable(true); // ボタンを押せるようにする
        nextQuizButton.gameObject.SetActive(false); // 「次の問題へ」ボタンを隠す

        // API通信をコルーチンで開始
        StartCoroutine(GetQuizDataCoroutine());
    }

    /// <summary>
    /// APIからクイズデータを非同期で取得するコルーチン
    /// </summary>
    private IEnumerator GetQuizDataCoroutine()
    {
        // GETリクエストを作成
        using (UnityWebRequest webRequest = UnityWebRequest.Get(API_URL))
        {
            // リクエストを送信し、応答を待つ
            yield return webRequest.SendWebRequest();

            // エラーハンドリング
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("API Error: " + webRequest.error);
                questionText.text = "クイズの取得に失敗しました。\nネットワーク接続を確認してください。";
            }
            else
            {
                // 成功した場合、受信したJSONテキストをパース
                string jsonResponse = webRequest.downloadHandler.text;
                Debug.Log("Received JSON: " + jsonResponse); // 受信したJSONをログに出力
                currentQuiz = JsonUtility.FromJson<QuizData>(jsonResponse);

                // UIにクイズ内容を表示
                DisplayQuiz();
            }
        }
    }

    /// <summary>
    /// 取得したクイズデータをUIに表示する
    /// </summary>
    private void DisplayQuiz()
    {
        questionText.text = currentQuiz.question;
        choiceAText.text = currentQuiz.options.A;
        choiceBText.text = currentQuiz.options.B;
    }

    /// <summary>
    /// 選択肢ボタンが押されたときに呼び出される
    /// </summary>
    /// <param name="selectedChoice">押された選択肢 ("A" or "B")</param>
    public void AnswerButtonPressed(string selectedChoice)
    {
        // --- ▼▼▼ デバッグログを追加 ▼▼▼ ---
        Debug.Log($"ボタンが押されました！ 選んだ選択肢: '{selectedChoice}'");

        if (isAnswered)
        {
            Debug.LogWarning("すでに回答済みのため処理をスキップします。");
            return;
        }
        isAnswered = true;

        if (currentQuiz == null)
        {
            Debug.LogError("クイズデータ(currentQuiz)がありません。回答を処理できません。");
            return;
        }

        Debug.Log($"正解は '{currentQuiz.answer}' です。'{selectedChoice}' と比較します。");
        // --- ▲▲▲ デバッグログを追加 ▲▲▲ ---

        // ボタンを無効化する
        SetButtonsInteractable(false);

        // 正誤判定
        if (selectedChoice == currentQuiz.answer)
        {
            resultText.text = "正解！";
            resultText.color = Color.green;
        }
        else
        {
            resultText.text = "不正解…";
            resultText.color = Color.red;
        }

        // 解説を表示
        // 正解の選択肢の解説を表示する
        if (currentQuiz.answer == "A")
        {
            explanationText.text = currentQuiz.explanation_A;
        }
        else
        {
            explanationText.text = currentQuiz.explanation_B;
        }

        // 「次の問題へ」ボタンを表示
        nextQuizButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// 選択肢ボタンの有効/無効を切り替える
    /// </summary>
    private void SetButtonsInteractable(bool interactable)
    {
        choiceAButton.interactable = interactable;
        choiceBButton.interactable = interactable;
    }
}
