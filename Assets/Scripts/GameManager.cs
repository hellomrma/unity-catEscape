using UnityEngine;

/// <summary>
/// 게임의 전반적인 상태를 관리하는 싱글톤 클래스
/// 게임 오버, 재시작 등의 게임 상태를 관리하며, 다른 스크립트에서 접근할 수 있는 중앙 관리자 역할을 합니다.
/// </summary>
public class GameManager : MonoBehaviour
{
    #region Singleton
    /// <summary>
    /// GameManager의 싱글톤 인스턴스
    /// 어디서든 GameManager.Instance로 접근할 수 있습니다.
    /// </summary>
    public static GameManager Instance { get; private set; }
    #endregion

    #region Events
    /// <summary>게임 오버가 발생했을 때 호출되는 이벤트</summary>
    public System.Action OnGameOverEvent;
    
    /// <summary>게임이 재시작될 때 호출되는 이벤트</summary>
    public System.Action OnGameRestartEvent;
    #endregion

    #region Fields
    /// <summary>게임 오버 상태 여부 (읽기 전용으로 외부에 노출)</summary>
    [Header("게임 상태")]
    [SerializeField] private bool isGameOver = false;
    #endregion

    #region Properties
    /// <summary>
    /// 게임 오버 상태를 확인하는 프로퍼티
    /// 외부에서는 읽기만 가능하고, 내부에서만 수정할 수 있습니다.
    /// </summary>
    public bool IsGameOver => isGameOver;
    #endregion

    #region Unity Lifecycle
    /// <summary>
    /// 게임 오브젝트가 생성될 때 호출됩니다.
    /// 싱글톤 패턴을 초기화합니다.
    /// </summary>
    void Awake()
    {
        InitializeSingleton();
    }
    #endregion

    #region Initialization
    /// <summary>
    /// 싱글톤 패턴을 초기화합니다.
    /// 인스턴스가 없으면 현재 오브젝트를 인스턴스로 설정하고,
    /// 이미 있으면 중복 생성을 방지하기 위해 현재 오브젝트를 파괴합니다.
    /// </summary>
    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            // 첫 번째 인스턴스이면 설정
            Instance = this;
            
            // 씬이 변경되어도 파괴되지 않도록 설정
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 이미 인스턴스가 있으면 중복 방지를 위해 파괴
            Destroy(gameObject);
        }
    }
    #endregion

    #region Game State Management
    /// <summary>
    /// 게임 오버를 처리합니다.
    /// 이미 게임 오버 상태면 중복 실행을 방지하고,
    /// 아니면 게임 오버 상태로 전환하고 관련 처리를 수행합니다.
    /// </summary>
    public void GameOver()
    {
        // 이미 게임 오버면 중복 실행 방지
        if (isGameOver)
        {
            return;
        }

        // 게임 오버 상태로 전환
        isGameOver = true;
        Debug.Log("Game Over!");

        // 게임 오버 처리 실행
        HandleGameOver();
    }

    /// <summary>
    /// 게임 오버 시 실제 처리를 수행합니다.
    /// 시간을 정지시키고 게임 오버 이벤트를 발생시킵니다.
    /// </summary>
    private void HandleGameOver()
    {
        // 시간 정지 (게임 일시정지 효과)
        Time.timeScale = 0f;
        
        // 게임 오버 이벤트 발생 (구독자가 있으면 호출)
        OnGameOverEvent?.Invoke();
    }

    /// <summary>
    /// 게임을 재시작합니다.
    /// 게임 상태를 리셋하고, 시간을 정상화한 후, 현재 씬을 다시 로드합니다.
    /// </summary>
    public void RestartGame()
    {
        // 게임 오버 상태 해제
        isGameOver = false;
        
        // 시간 정상화
        Time.timeScale = 1f;

        // 재시작 이벤트 발생
        OnGameRestartEvent?.Invoke();
        
        // 씬 재로드
        ReloadScene();
    }

    /// <summary>
    /// 현재 씬을 다시 로드합니다.
    /// SceneManager를 사용하여 현재 활성 씬의 이름으로 다시 로드합니다.
    /// </summary>
    private void ReloadScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    /// <summary>
    /// 게임 상태를 리셋합니다.
    /// 게임 오버 상태를 해제하고 시간을 정상화합니다.
    /// 씬을 재로드하지 않고 상태만 리셋할 때 사용합니다.
    /// </summary>
    public void ResetGameState()
    {
        isGameOver = false;
        Time.timeScale = 1f;
    }
    #endregion
}
