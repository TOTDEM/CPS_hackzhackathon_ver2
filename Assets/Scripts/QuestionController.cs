using UnityEngine;
using UnityEngine.UI; // UI�v�f���������߂ɕK�v
using UnityEngine.Networking; // Web���N�G�X�g���������߂ɕK�v
using System.Collections;

// --- ���̃X�N���v�g�̎g���� ---
// 1. Unity�ŋ��GameObject���쐬���A�uQuizManager�v�Ɩ��t���܂��B
// 2. ���̃X�N���v�g���A�쐬�����uQuizManager�vGameObject�ɃA�^�b�`���܂��B
// 3. ��ʂɖ�蕶�A�I����A/B�̃{�^���A����Ȃǂ�\�����邽�߂�UI Text��Button���쐬���܂��B
// 4. �C���X�y�N�^�[��ŁA���̃X�N���v�g�̊eUI�Q�ƁiquestionText�Ȃǁj�ɁA�쐬����UI�v�f���h���b�O���h���b�v�Őݒ肵�܂��B
// 5. �I����A/B�̃{�^����OnClick()�C�x���g�ɁA���̃X�N���v�g��AnswerButtonPressed("A") / AnswerButtonPressed("B")�����ꂼ��ݒ肵�܂��B

public class QuizManager : MonoBehaviour
{
    // --- API�ݒ� ---
    // ���Ȃ����f�v���C����API��URL��ݒ肵�܂�
    private const string API_URL = "http://Quizgame-env.eba-ia2gsrsb.ap-southeast-2.elasticbeanstalk.com/quiz";

    // --- UI�Q�� ---
    // Unity�G�f�B�^�̃C���X�y�N�^�[����A�Ή�����UI�v�f���A�^�b�`���Ă�������
    [Header("UI References")]
    public Text questionText;       // ��蕶��\������Text
    public Text choiceAText;        // �I����A�̃{�^����Text
    public Text choiceBText;        // �I����B�̃{�^����Text
    public Text resultText;         // ����/�s������\������Text
    public Text explanationText;    // �������\������Text

    [Header("UI Buttons")]
    public Button choiceAButton;
    public Button choiceBButton;
    public Button nextQuizButton;   // ���̖��֐i�ރ{�^��

    // --- �����f�[�^ ---
    private QuizData currentQuiz; // ���ݕ\�����Ă���N�C�Y�̃f�[�^
    private bool isAnswered = false; // �񓚍ς݂��ǂ����̃t���O

    // --- JSON�̍\���ɑΉ�����C#�N���X ---
    // JSON���p�[�X���邽�߂ɁA�L�[���ƕϐ�������v������K�v������܂�
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


    // --- ���C������ ---

    void Start()
    {
        // �Q�[���J�n���ɍŏ��̃N�C�Y���擾
        FetchNewQuiz();
    }

    /// <summary>
    /// �V�����N�C�Y���擾���ĉ�ʂ��X�V����
    /// </summary>
    public void FetchNewQuiz()
    {
        // UI��������Ԃɖ߂�
        resultText.text = "";
        explanationText.text = "";
        isAnswered = false;
        SetButtonsInteractable(true); // �{�^����������悤�ɂ���
        nextQuizButton.gameObject.SetActive(false); // �u���̖��ցv�{�^�����B��

        // API�ʐM���R���[�`���ŊJ�n
        StartCoroutine(GetQuizDataCoroutine());
    }

    /// <summary>
    /// API����N�C�Y�f�[�^��񓯊��Ŏ擾����R���[�`��
    /// </summary>
    private IEnumerator GetQuizDataCoroutine()
    {
        // GET���N�G�X�g���쐬
        using (UnityWebRequest webRequest = UnityWebRequest.Get(API_URL))
        {
            // ���N�G�X�g�𑗐M���A������҂�
            yield return webRequest.SendWebRequest();

            // �G���[�n���h�����O
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("API Error: " + webRequest.error);
                questionText.text = "�N�C�Y�̎擾�Ɏ��s���܂����B\n�l�b�g���[�N�ڑ����m�F���Ă��������B";
            }
            else
            {
                // ���������ꍇ�A��M����JSON�e�L�X�g���p�[�X
                string jsonResponse = webRequest.downloadHandler.text;
                Debug.Log("Received JSON: " + jsonResponse); // ��M����JSON�����O�ɏo��
                currentQuiz = JsonUtility.FromJson<QuizData>(jsonResponse);

                // UI�ɃN�C�Y���e��\��
                DisplayQuiz();
            }
        }
    }

    /// <summary>
    /// �擾�����N�C�Y�f�[�^��UI�ɕ\������
    /// </summary>
    private void DisplayQuiz()
    {
        questionText.text = currentQuiz.question;
        choiceAText.text = currentQuiz.options.A;
        choiceBText.text = currentQuiz.options.B;
    }

    /// <summary>
    /// �I�����{�^���������ꂽ�Ƃ��ɌĂяo�����
    /// </summary>
    /// <param name="selectedChoice">�����ꂽ�I���� ("A" or "B")</param>
    public void AnswerButtonPressed(string selectedChoice)
    {
        // --- ������ �f�o�b�O���O��ǉ� ������ ---
        Debug.Log($"�{�^����������܂����I �I�񂾑I����: '{selectedChoice}'");

        if (isAnswered)
        {
            Debug.LogWarning("���łɉ񓚍ς݂̂��ߏ������X�L�b�v���܂��B");
            return;
        }
        isAnswered = true;

        if (currentQuiz == null)
        {
            Debug.LogError("�N�C�Y�f�[�^(currentQuiz)������܂���B�񓚂������ł��܂���B");
            return;
        }

        Debug.Log($"������ '{currentQuiz.answer}' �ł��B'{selectedChoice}' �Ɣ�r���܂��B");
        // --- ������ �f�o�b�O���O��ǉ� ������ ---

        // �{�^���𖳌�������
        SetButtonsInteractable(false);

        // ���딻��
        if (selectedChoice == currentQuiz.answer)
        {
            resultText.text = "�����I";
            resultText.color = Color.green;
        }
        else
        {
            resultText.text = "�s�����c";
            resultText.color = Color.red;
        }

        // �����\��
        // �����̑I�����̉����\������
        if (currentQuiz.answer == "A")
        {
            explanationText.text = currentQuiz.explanation_A;
        }
        else
        {
            explanationText.text = currentQuiz.explanation_B;
        }

        // �u���̖��ցv�{�^����\��
        nextQuizButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// �I�����{�^���̗L��/������؂�ւ���
    /// </summary>
    private void SetButtonsInteractable(bool interactable)
    {
        choiceAButton.interactable = interactable;
        choiceBButton.interactable = interactable;
    }
}
