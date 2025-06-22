using UnityEngine;
using UnityEngine.UI; // SliderやButtonなどのUIを扱うために必要
using UnityEngine.Networking; // Webリクエストを扱うために必要
using System.Collections;

public class QuizManager : MonoBehaviour
{
    // --- 定数 ---
    private const string API_URL = "http://Quizgame-env.eba-ia2gsrsb.ap-southeast-2.elasticbeanstalk.com/quiz";

    // --- UI参照 ---
    [Header("UI References")]
    [Tooltip("問題文を表示するText")]
    public Text questionText;
    [Tooltip("選択肢Aのボタン内Text")]
    public Text choiceAText;
    [Tooltip("選択肢Bのボタン内Text")]
    public Text choiceBText;
    [Tooltip("選択肢Aの解説を表示するText")]
    public Text explanationText_A;
    [Tooltip("選択肢Bの解説を表示するText")]
    public Text explanationText_B;

    [Header("Result Display")]
    [Tooltip("〇×画像を表示するImage")]
    public Image resultImage;
    [Tooltip("正解時に表示する〇の画像")]
    public Sprite correctSprite;
    [Tooltip("不正解時に表示する×の画像")]
    public Sprite incorrectSprite;
    [Tooltip("結果を表示してから次の問題に移るまでの時間（秒）")]
    public float resultDisplayTime = 3.0f;

    [Header("HP System")]
    [Tooltip("モンスターのHPバー（Slider）")]
    public Slider monsterHPSlider;
    [Tooltip("選択肢Aが正解だった場合のダメージ量")]
    public float damageForAnswerA = 30f; // ▼追加
    [Tooltip("選択肢Bが正解だった場合のダメージ量")]
    public float damageForAnswerB = 20f; // ▼追加
    [Tooltip("プレイヤーのHPバー（Slider）")]
    public Slider playerHPSlider;
    [Tooltip("不正解だった場合にプレイヤーが受けるダメージ量")]
    public float damageOnIncorrect = 10f;

    [Header("UI Buttons")]
    public Button choiceAButton;
    public Button choiceBButton;

    // --- 内部データ ---
    private QuizData currentQuiz;
    private bool isAnswered = false;
    private float maxMonsterHP;
    private float currentMonsterHP;
    private float maxPlayerHP;
    private float currentPlayerHP;

    // --- JSONの構造に対応するC#クラス ---
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

    /// <summary>
    /// ゲーム開始時に一度だけ呼ばれる初期化処理
    /// </summary>
    void Start()
    {
        // UIの初期化
        resultImage.gameObject.SetActive(false);
        explanationText_A.gameObject.SetActive(false);
        explanationText_B.gameObject.SetActive(false);
        SetButtonsInteractable(false); //最初はボタンを押せないように

        // HPシステムの初期化
        if (monsterHPSlider != null)
        {
            // SliderのInspectorで設定した最大値を、このゲームでの最大HPとして採用します
            maxMonsterHP = monsterHPSlider.maxValue;
            // ゲーム開始と同時にHPを満タンにします
            currentMonsterHP = maxMonsterHP;
            // Sliderの見た目を現在のHPに合わせます
            monsterHPSlider.value = currentMonsterHP;
        }

        if (playerHPSlider != null)
        {
            maxPlayerHP = playerHPSlider.maxValue;
            currentPlayerHP = maxPlayerHP;
            playerHPSlider.value = currentPlayerHP;
        }

        // 最初のクイズを非同期で読み込む
        StartCoroutine(FetchAndDisplayNewQuiz());
    }

    /// <summary>
    /// クイズをAPIから取得して画面に表示する一連の処理
    /// </summary>
    private IEnumerator FetchAndDisplayNewQuiz()
    {
        questionText.text = "クイズを読み込み中...";
        choiceAText.text = "";
        choiceBText.text = "";

        yield return StartCoroutine(GetQuizDataCoroutine());

        DisplayQuiz();
        ResetForNextAnswer();
    }

    /// <summary>
    /// APIからクイズデータを非同期で取得するコルーチン
    /// </summary>
    private IEnumerator GetQuizDataCoroutine()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(API_URL))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("API Error: " + webRequest.error);
                questionText.text = "クイズの取得に失敗しました。";
            }
            else
            {
                string jsonResponse = webRequest.downloadHandler.text;
                currentQuiz = JsonUtility.FromJson<QuizData>(jsonResponse);
            }
        }
    }

    /// <summary>
    /// 取得したクイズデータをUIに表示する
    /// </summary>
    private void DisplayQuiz()
    {
        if (currentQuiz == null) return;
        questionText.text = currentQuiz.question;
        choiceAText.text = currentQuiz.options.A;
        choiceBText.text = currentQuiz.options.B;

        // 問題と一緒に、両方の解説も表示する
        explanationText_A.text = currentQuiz.explanation_A;
        explanationText_B.text = currentQuiz.explanation_B;
        explanationText_A.gameObject.SetActive(true);
        explanationText_B.gameObject.SetActive(true);
    }

    /// <summary>
    /// 選択肢ボタンが押されたときに呼び出される
    /// </summary>
    public void AnswerButtonPressed(string selectedChoice)
    {
        if (isAnswered) return;
        isAnswered = true;
        if (currentQuiz == null) return;

        // 回答したら、まず解説を非表示にする
        explanationText_A.gameObject.SetActive(false);
        explanationText_B.gameObject.SetActive(false);

        SetButtonsInteractable(false);

        // 正誤判定
        if (selectedChoice == currentQuiz.answer)
        {
            // 正解の場合
            resultImage.sprite = correctSprite;

            float damageToDeal = 0f; //与えるダメージ量を一時的に格納する変数

            if (currentQuiz.answer == "A")
            {
                damageToDeal = damageForAnswerA;
                Debug.Log("正解(A)！モンスターに " + damageToDeal + " ダメージ！");
            }
            else // 正解が "B" の場合
            {
                damageToDeal = damageForAnswerB;
                Debug.Log("正解(B)！モンスターに " + damageToDeal + " ダメージ！");
            }
            // HPを減らす処理
            if (monsterHPSlider != null)
            {
                currentMonsterHP -= damageToDeal; // 計算したダメージ量を適用
                currentMonsterHP = Mathf.Max(0, currentMonsterHP);
                monsterHPSlider.value = currentMonsterHP;
                Debug.Log("モンスターの残りHP: " + currentMonsterHP);

                if (currentMonsterHP <= 0)
                {
                    Debug.Log("モンスターを倒した！勝利！");
                    //勝利演出など
                }
            }
        }
        else
        {
            // 不正解の場合
            resultImage.sprite = incorrectSprite;
            Debug.Log("不正解...");

            if (playerHPSlider != null)
            {
                currentPlayerHP -= damageOnIncorrect;
                currentPlayerHP = Mathf.Max(0, currentPlayerHP); // HPが0未満にならないように
                playerHPSlider.value = currentPlayerHP;
                Debug.Log("プレイヤーの残りHP: " + currentPlayerHP);

                if (currentPlayerHP <= 0)
                {
                    Debug.Log("プレイヤーのHPが0になった...ゲームオーバー！");
                    // ここにゲームオーバー演出やリトライ処理などを書くことができます
                }
            }
        }
        resultImage.gameObject.SetActive(true);

        // 自動で次の問題へ遷移するコルーチンを開始
        StartCoroutine(TransitionToNextQuiz());
    }

    /// <summary>
    /// 結果表示後、自動で次の問題へ遷移するコルーチン
    /// </summary>
    private IEnumerator TransitionToNextQuiz()
    {
        // 1. 次のクイズを裏で読み込み開始
        IEnumerator fetchCoroutine = GetQuizDataCoroutine();
        StartCoroutine(fetchCoroutine);

        // 2. 結果を数秒間表示
        yield return new WaitForSeconds(resultDisplayTime);

        // 3. 読み込みが終わるまで待つ
        yield return fetchCoroutine;

        // 4. UIをリセットして、次の問題を表示
        resultImage.gameObject.SetActive(false);
        explanationText_A.gameObject.SetActive(false);
        explanationText_B.gameObject.SetActive(false);

        DisplayQuiz();
        ResetForNextAnswer();
    }

    /// <summary>
    /// 回答可能な状態にUIをリセットする
    /// </summary>
    private void ResetForNextAnswer()
    {
        isAnswered = false;
        SetButtonsInteractable(true);
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