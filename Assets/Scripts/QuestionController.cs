using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class QuizManager : MonoBehaviour
{
    // --- API設定 ---
    private const string API_URL = "http://Quizgame-env.eba-ia2gsrsb.ap-southeast-2.elasticbeanstalk.com/quiz";

    // --- UI参照 ---
    [Header("UI References")]
    public Text questionText;
    public Text choiceAText;
    public Text choiceBText;
    public Text explanationText_A;
    public Text explanationText_B;

    [Header("Result Display")]
    public Image resultImage;
    public Sprite correctSprite;
    public Sprite incorrectSprite;
    [Tooltip("結果を表示してから次の問題に移るまでの時間（秒）")]
    public float resultDisplayTime = 3.0f;

    [Header("UI Buttons")]
    public Button choiceAButton;
    public Button choiceBButton;

    // --- 内部データ ---
    private QuizData currentQuiz;
    private bool isAnswered = false;

    // (QuizData, OptionsDataクラスは変更なし)
    [System.Serializable] private class QuizData { public string question; public OptionsData options; public string explanation_A; public string explanation_B; public string answer; }
    [System.Serializable] private class OptionsData { public string A; public string B; }


    void Start()
    {
        // UIの初期化
        resultImage.gameObject.SetActive(false);
        explanationText_A.gameObject.SetActive(false);
        explanationText_B.gameObject.SetActive(false);
        SetButtonsInteractable(false);

        StartCoroutine(FetchAndDisplayNewQuiz());
    }

    private IEnumerator FetchAndDisplayNewQuiz()
    {
        questionText.text = "クイズを読み込み中...";
        choiceAText.text = "";
        choiceBText.text = "";

        yield return StartCoroutine(GetQuizDataCoroutine());

        DisplayQuiz();
        ResetForNextAnswer();
    }

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

        // ▼▼▼【修正】問題と一緒に、両方の解説も表示する ▼▼▼
        explanationText_A.text = currentQuiz.explanation_A;
        explanationText_B.text = currentQuiz.explanation_B;
        explanationText_A.gameObject.SetActive(true);
        explanationText_B.gameObject.SetActive(true);
        // ▲▲▲ ここまで修正 ▲▲▲
    }

    /// <summary>
    /// 選択肢ボタンが押されたときに呼び出される
    /// </summary>
    public void AnswerButtonPressed(string selectedChoice)
    {
        if (isAnswered) return;
        isAnswered = true;
        if (currentQuiz == null) return;

        // ▼▼▼【修正】回答したら、まず解説を非表示にする ▼▼▼
        explanationText_A.gameObject.SetActive(false);
        explanationText_B.gameObject.SetActive(false);
        // ▲▲▲ ここまで修正 ▲▲▲

        SetButtonsInteractable(false);

        // 正誤判定と結果画像の表示
        if (selectedChoice == currentQuiz.answer)
        {
            resultImage.sprite = correctSprite;
        }
        else
        {
            resultImage.sprite = incorrectSprite;
        }
        resultImage.gameObject.SetActive(true);

        // ▼削除：ここにあった解説表示ロジックは不要

        StartCoroutine(TransitionToNextQuiz());
    }

    /// <summary>
    /// 結果表示後、自動で次の問題へ遷移するコルーチン
    /// </summary>
    private IEnumerator TransitionToNextQuiz()
    {
        IEnumerator fetchCoroutine = GetQuizDataCoroutine();
        StartCoroutine(fetchCoroutine);
        yield return new WaitForSeconds(resultDisplayTime);
        yield return fetchCoroutine;

        // 画面更新時にUIをリセットする
        resultImage.gameObject.SetActive(false);
        // 解説はすでに非表示だが、念のためここでも非表示にしておくと安全
        explanationText_A.gameObject.SetActive(false);
        explanationText_B.gameObject.SetActive(false);

        DisplayQuiz();
        ResetForNextAnswer();
    }

    private void ResetForNextAnswer()
    {
        isAnswered = false;
        SetButtonsInteractable(true);
    }

    private void SetButtonsInteractable(bool interactable)
    {
        choiceAButton.interactable = interactable;
        choiceBButton.interactable = interactable;
    }
}