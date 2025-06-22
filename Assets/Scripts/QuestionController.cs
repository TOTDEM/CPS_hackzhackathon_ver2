using UnityEngine;
using UnityEngine.UI; // Slider��Button�Ȃǂ�UI���������߂ɕK�v
using UnityEngine.Networking; // Web���N�G�X�g���������߂ɕK�v
using System.Collections;

public class QuizManager : MonoBehaviour
{
    // --- �萔 ---
    private const string API_URL = "http://Quizgame-env.eba-ia2gsrsb.ap-southeast-2.elasticbeanstalk.com/quiz";

    // --- UI�Q�� ---
    [Header("UI References")]
    [Tooltip("��蕶��\������Text")]
    public Text questionText;
    [Tooltip("�I����A�̃{�^����Text")]
    public Text choiceAText;
    [Tooltip("�I����B�̃{�^����Text")]
    public Text choiceBText;
    [Tooltip("�I����A�̉����\������Text")]
    public Text explanationText_A;
    [Tooltip("�I����B�̉����\������Text")]
    public Text explanationText_B;

    [Header("Result Display")]
    [Tooltip("�Z�~�摜��\������Image")]
    public Image resultImage;
    [Tooltip("�������ɕ\������Z�̉摜")]
    public Sprite correctSprite;
    [Tooltip("�s�������ɕ\������~�̉摜")]
    public Sprite incorrectSprite;
    [Tooltip("���ʂ�\�����Ă��玟�̖��Ɉڂ�܂ł̎��ԁi�b�j")]
    public float resultDisplayTime = 3.0f;

    [Header("HP System")]
    [Tooltip("�����X�^�[��HP�o�[�iSlider�j")]
    public Slider monsterHPSlider;
    [Tooltip("�I����A�������������ꍇ�̃_���[�W��")]
    public float damageForAnswerA = 30f; // ���ǉ�
    [Tooltip("�I����B�������������ꍇ�̃_���[�W��")]
    public float damageForAnswerB = 20f; // ���ǉ�
    [Tooltip("�v���C���[��HP�o�[�iSlider�j")]
    public Slider playerHPSlider;
    [Tooltip("�s�����������ꍇ�Ƀv���C���[���󂯂�_���[�W��")]
    public float damageOnIncorrect = 10f;

    [Header("UI Buttons")]
    public Button choiceAButton;
    public Button choiceBButton;

    // --- �����f�[�^ ---
    private QuizData currentQuiz;
    private bool isAnswered = false;
    private float maxMonsterHP;
    private float currentMonsterHP;
    private float maxPlayerHP;
    private float currentPlayerHP;

    // --- JSON�̍\���ɑΉ�����C#�N���X ---
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

    /// <summary>
    /// �Q�[���J�n���Ɉ�x�����Ă΂�鏉��������
    /// </summary>
    void Start()
    {
        // UI�̏�����
        resultImage.gameObject.SetActive(false);
        explanationText_A.gameObject.SetActive(false);
        explanationText_B.gameObject.SetActive(false);
        SetButtonsInteractable(false); //�ŏ��̓{�^���������Ȃ��悤��

        // HP�V�X�e���̏�����
        if (monsterHPSlider != null)
        {
            // Slider��Inspector�Őݒ肵���ő�l���A���̃Q�[���ł̍ő�HP�Ƃ��č̗p���܂�
            maxMonsterHP = monsterHPSlider.maxValue;
            // �Q�[���J�n�Ɠ�����HP�𖞃^���ɂ��܂�
            currentMonsterHP = maxMonsterHP;
            // Slider�̌����ڂ����݂�HP�ɍ��킹�܂�
            monsterHPSlider.value = currentMonsterHP;
        }

        if (playerHPSlider != null)
        {
            maxPlayerHP = playerHPSlider.maxValue;
            currentPlayerHP = maxPlayerHP;
            playerHPSlider.value = currentPlayerHP;
        }

        // �ŏ��̃N�C�Y��񓯊��œǂݍ���
        StartCoroutine(FetchAndDisplayNewQuiz());
    }

    /// <summary>
    /// �N�C�Y��API����擾���ĉ�ʂɕ\�������A�̏���
    /// </summary>
    private IEnumerator FetchAndDisplayNewQuiz()
    {
        questionText.text = "�N�C�Y��ǂݍ��ݒ�...";
        choiceAText.text = "";
        choiceBText.text = "";

        yield return StartCoroutine(GetQuizDataCoroutine());

        DisplayQuiz();
        ResetForNextAnswer();
    }

    /// <summary>
    /// API����N�C�Y�f�[�^��񓯊��Ŏ擾����R���[�`��
    /// </summary>
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

        // ���ƈꏏ�ɁA�����̉�����\������
        explanationText_A.text = currentQuiz.explanation_A;
        explanationText_B.text = currentQuiz.explanation_B;
        explanationText_A.gameObject.SetActive(true);
        explanationText_B.gameObject.SetActive(true);
    }

    /// <summary>
    /// �I�����{�^���������ꂽ�Ƃ��ɌĂяo�����
    /// </summary>
    public void AnswerButtonPressed(string selectedChoice)
    {
        if (isAnswered) return;
        isAnswered = true;
        if (currentQuiz == null) return;

        // �񓚂�����A�܂�������\���ɂ���
        explanationText_A.gameObject.SetActive(false);
        explanationText_B.gameObject.SetActive(false);

        SetButtonsInteractable(false);

        // ���딻��
        if (selectedChoice == currentQuiz.answer)
        {
            // �����̏ꍇ
            resultImage.sprite = correctSprite;

            float damageToDeal = 0f; //�^����_���[�W�ʂ��ꎞ�I�Ɋi�[����ϐ�

            if (currentQuiz.answer == "A")
            {
                damageToDeal = damageForAnswerA;
                Debug.Log("����(A)�I�����X�^�[�� " + damageToDeal + " �_���[�W�I");
            }
            else // ������ "B" �̏ꍇ
            {
                damageToDeal = damageForAnswerB;
                Debug.Log("����(B)�I�����X�^�[�� " + damageToDeal + " �_���[�W�I");
            }
            // HP�����炷����
            if (monsterHPSlider != null)
            {
                currentMonsterHP -= damageToDeal; // �v�Z�����_���[�W�ʂ�K�p
                currentMonsterHP = Mathf.Max(0, currentMonsterHP);
                monsterHPSlider.value = currentMonsterHP;
                Debug.Log("�����X�^�[�̎c��HP: " + currentMonsterHP);

                if (currentMonsterHP <= 0)
                {
                    Debug.Log("�����X�^�[��|�����I�����I");
                    //�������o�Ȃ�
                }
            }
        }
        else
        {
            // �s�����̏ꍇ
            resultImage.sprite = incorrectSprite;
            Debug.Log("�s����...");

            if (playerHPSlider != null)
            {
                currentPlayerHP -= damageOnIncorrect;
                currentPlayerHP = Mathf.Max(0, currentPlayerHP); // HP��0�����ɂȂ�Ȃ��悤��
                playerHPSlider.value = currentPlayerHP;
                Debug.Log("�v���C���[�̎c��HP: " + currentPlayerHP);

                if (currentPlayerHP <= 0)
                {
                    Debug.Log("�v���C���[��HP��0�ɂȂ���...�Q�[���I�[�o�[�I");
                    // �����ɃQ�[���I�[�o�[���o�⃊�g���C�����Ȃǂ��������Ƃ��ł��܂�
                }
            }
        }
        resultImage.gameObject.SetActive(true);

        // �����Ŏ��̖��֑J�ڂ���R���[�`�����J�n
        StartCoroutine(TransitionToNextQuiz());
    }

    /// <summary>
    /// ���ʕ\����A�����Ŏ��̖��֑J�ڂ���R���[�`��
    /// </summary>
    private IEnumerator TransitionToNextQuiz()
    {
        // 1. ���̃N�C�Y�𗠂œǂݍ��݊J�n
        IEnumerator fetchCoroutine = GetQuizDataCoroutine();
        StartCoroutine(fetchCoroutine);

        // 2. ���ʂ𐔕b�ԕ\��
        yield return new WaitForSeconds(resultDisplayTime);

        // 3. �ǂݍ��݂��I���܂ő҂�
        yield return fetchCoroutine;

        // 4. UI�����Z�b�g���āA���̖���\��
        resultImage.gameObject.SetActive(false);
        explanationText_A.gameObject.SetActive(false);
        explanationText_B.gameObject.SetActive(false);

        DisplayQuiz();
        ResetForNextAnswer();
    }

    /// <summary>
    /// �񓚉\�ȏ�Ԃ�UI�����Z�b�g����
    /// </summary>
    private void ResetForNextAnswer()
    {
        isAnswered = false;
        SetButtonsInteractable(true);
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