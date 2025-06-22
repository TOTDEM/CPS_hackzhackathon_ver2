using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class QuizManager : MonoBehaviour
{
    // --- API�ݒ� ---
    private const string API_URL = "http://Quizgame-env.eba-ia2gsrsb.ap-southeast-2.elasticbeanstalk.com/quiz";

    // --- UI�Q�� ---
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
    [Tooltip("���ʂ�\�����Ă��玟�̖��Ɉڂ�܂ł̎��ԁi�b�j")]
    public float resultDisplayTime = 3.0f;

    [Header("UI Buttons")]
    public Button choiceAButton;
    public Button choiceBButton;

    // --- �����f�[�^ ---
    private QuizData currentQuiz;
    private bool isAnswered = false;

    // (QuizData, OptionsData�N���X�͕ύX�Ȃ�)
    [System.Serializable] private class QuizData { public string question; public OptionsData options; public string explanation_A; public string explanation_B; public string answer; }
    [System.Serializable] private class OptionsData { public string A; public string B; }


    void Start()
    {
        // UI�̏�����
        resultImage.gameObject.SetActive(false);
        explanationText_A.gameObject.SetActive(false);
        explanationText_B.gameObject.SetActive(false);
        SetButtonsInteractable(false);

        StartCoroutine(FetchAndDisplayNewQuiz());
    }

    private IEnumerator FetchAndDisplayNewQuiz()
    {
        questionText.text = "�N�C�Y��ǂݍ��ݒ�...";
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
                questionText.text = "�N�C�Y�̎擾�Ɏ��s���܂����B";
            }
            else
            {
                string jsonResponse = webRequest.downloadHandler.text;
                currentQuiz = JsonUtility.FromJson<QuizData>(jsonResponse);
            }
        }
    }

    /// <summary>
    /// �擾�����N�C�Y�f�[�^��UI�ɕ\������
    /// </summary>
    private void DisplayQuiz()
    {
        if (currentQuiz == null) return;
        questionText.text = currentQuiz.question;
        choiceAText.text = currentQuiz.options.A;
        choiceBText.text = currentQuiz.options.B;

        // �������y�C���z���ƈꏏ�ɁA�����̉�����\������ ������
        explanationText_A.text = currentQuiz.explanation_A;
        explanationText_B.text = currentQuiz.explanation_B;
        explanationText_A.gameObject.SetActive(true);
        explanationText_B.gameObject.SetActive(true);
        // ������ �����܂ŏC�� ������
    }

    /// <summary>
    /// �I�����{�^���������ꂽ�Ƃ��ɌĂяo�����
    /// </summary>
    public void AnswerButtonPressed(string selectedChoice)
    {
        if (isAnswered) return;
        isAnswered = true;
        if (currentQuiz == null) return;

        // �������y�C���z�񓚂�����A�܂�������\���ɂ��� ������
        explanationText_A.gameObject.SetActive(false);
        explanationText_B.gameObject.SetActive(false);
        // ������ �����܂ŏC�� ������

        SetButtonsInteractable(false);

        // ���딻��ƌ��ʉ摜�̕\��
        if (selectedChoice == currentQuiz.answer)
        {
            resultImage.sprite = correctSprite;
        }
        else
        {
            resultImage.sprite = incorrectSprite;
        }
        resultImage.gameObject.SetActive(true);

        // ���폜�F�����ɂ���������\�����W�b�N�͕s�v

        StartCoroutine(TransitionToNextQuiz());
    }

    /// <summary>
    /// ���ʕ\����A�����Ŏ��̖��֑J�ڂ���R���[�`��
    /// </summary>
    private IEnumerator TransitionToNextQuiz()
    {
        IEnumerator fetchCoroutine = GetQuizDataCoroutine();
        StartCoroutine(fetchCoroutine);
        yield return new WaitForSeconds(resultDisplayTime);
        yield return fetchCoroutine;

        // ��ʍX�V����UI�����Z�b�g����
        resultImage.gameObject.SetActive(false);
        // ����͂��łɔ�\�������A�O�̂��߂����ł���\���ɂ��Ă����ƈ��S
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