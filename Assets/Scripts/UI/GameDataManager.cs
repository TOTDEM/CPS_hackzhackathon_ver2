// ファイル名: GameDataManager.cs
using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    public bool IsBattleSuccess { get; private set; } // バトルが成功したかどうか

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーンを跨いでデータを保持
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// バトルの結果を設定します．
    /// </summary>
    /// <param name="isSuccess">成功した場合はtrue，失敗した場合はfalse</param>
    public void SetBattleResult(bool isSuccess)
    {
        IsBattleSuccess = isSuccess;
        Debug.Log($"バトルの結果を設定しました: 成功 = {IsBattleSuccess}");
    }
}