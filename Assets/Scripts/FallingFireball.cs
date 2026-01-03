using UnityEngine;

/// <summary>
/// 화면 위에서 떨어지는 불덩어리를 제어하는 클래스
/// 플레이어와 충돌 시 게임 오버를 트리거하며, 화면 밖으로 나가면 자동으로 제거됩니다.
/// </summary>
public class FallingFireball : MonoBehaviour
{
    #region Constants
    /// <summary>기본 낙하 속도 (초당 유닛)</summary>
    private const float DEFAULT_FALL_SPEED = 5f;
    
    /// <summary>화면 하단 경계 계산 시 추가할 여유 공간</summary>
    private const float BOUNDARY_OFFSET = 1f;
    
    /// <summary>카메라를 찾지 못했을 때 사용할 기본 하단 경계값</summary>
    private const float DEFAULT_BOTTOM_BOUNDARY = -10f;
    #endregion

    #region Fields
    /// <summary>불덩어리의 낙하 속도 (Inspector에서 설정 가능)</summary>
    [Header("속도 설정")]
    [SerializeField] private float fallSpeed = DEFAULT_FALL_SPEED;

    /// <summary>메인 카메라 참조 (화면 경계 계산에 사용)</summary>
    private Camera mainCamera;
    
    /// <summary>화면 하단 경계값 (이 값보다 아래로 가면 제거됨)</summary>
    private float bottomBoundary;
    
    /// <summary>이미 충돌했는지 여부 (중복 충돌 방지용)</summary>
    private bool hasCollided = false;
    #endregion

    #region Events
    /// <summary>불덩어리가 제거될 때 호출되는 이벤트</summary>
    public System.Action OnDestroyed;
    #endregion

    #region Unity Lifecycle
    /// <summary>
    /// 게임 오브젝트가 활성화될 때 호출됩니다.
    /// 초기화 작업을 수행합니다.
    /// </summary>
    void Start()
    {
        Initialize();
    }

    /// <summary>
    /// 매 프레임마다 호출됩니다.
    /// 불덩어리를 아래로 이동시키고, 화면 경계를 체크합니다.
    /// </summary>
    void Update()
    {
        // 이동을 중지해야 하는 조건 체크
        if (ShouldStopMovement())
        {
            return;
        }

        // 아래로 이동
        MoveDown();
        
        // 화면 밖으로 나갔는지 체크
        CheckBoundary();
    }

    /// <summary>
    /// 게임 오브젝트가 파괴될 때 호출됩니다.
    /// 제거 이벤트를 발생시킵니다.
    /// </summary>
    void OnDestroy()
    {
        OnDestroyed?.Invoke();
    }

    /// <summary>
    /// 트리거 콜라이더에 다른 오브젝트가 진입했을 때 호출됩니다.
    /// 플레이어와의 충돌을 감지합니다.
    /// </summary>
    /// <param name="other">충돌한 다른 오브젝트의 콜라이더</param>
    void OnTriggerEnter2D(Collider2D other)
    {
        HandleCollision(other);
    }

    /// <summary>
    /// 트리거 콜라이더 안에 다른 오브젝트가 계속 있을 때 호출됩니다.
    /// OnTriggerEnter2D가 호출되지 않는 경우를 대비한 백업 충돌 감지입니다.
    /// </summary>
    /// <param name="other">충돌한 다른 오브젝트의 콜라이더</param>
    void OnTriggerStay2D(Collider2D other)
    {
        HandleCollision(other);
    }
    #endregion

    #region Initialization
    /// <summary>
    /// 불덩어리의 초기화 작업을 수행합니다.
    /// 카메라 설정, 경계 계산, 콜라이더 및 리지드바디 설정을 진행합니다.
    /// </summary>
    private void Initialize()
    {
        // 오브젝트가 비활성화되어 있으면 활성화
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        // 카메라 초기화
        InitializeCamera();
        
        // 화면 하단 경계 계산
        CalculateBottomBoundary();
        
        // 콜라이더 자동 설정
        SetupCollider();
        
        // 리지드바디 자동 설정
        SetupRigidbody();
        
        // 속도 유효성 검사
        ValidateFallSpeed();
    }

    /// <summary>
    /// 메인 카메라를 찾아서 초기화합니다.
    /// Camera.main이 없으면 씬에서 모든 카메라를 검색합니다.
    /// </summary>
    private void InitializeCamera()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
    }

    /// <summary>
    /// 낙하 속도가 유효한지 검사하고, 유효하지 않으면 기본값으로 설정합니다.
    /// </summary>
    private void ValidateFallSpeed()
    {
        if (fallSpeed <= 0)
        {
            fallSpeed = DEFAULT_FALL_SPEED;
        }
    }
    #endregion

    #region Setup Methods
    /// <summary>
    /// Rigidbody2D 컴포넌트를 설정합니다.
    /// 없으면 자동으로 추가하고, Kinematic 타입으로 설정하여 물리 영향 없이 충돌만 감지합니다.
    /// </summary>
    private void SetupRigidbody()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        // Kinematic으로 설정하여 물리 영향 없이 이동
        rb.bodyType = RigidbodyType2D.Kinematic;
        
        // 중력 비활성화
        rb.gravityScale = 0;
        
        // 회전 방지
        rb.freezeRotation = true;
        
        // 시뮬레이션 활성화 (충돌 감지를 위해 필요)
        rb.simulated = true;
        
        // 보간 비활성화 (성능 향상)
        rb.interpolation = RigidbodyInterpolation2D.None;
    }

    /// <summary>
    /// Collider2D 컴포넌트를 설정합니다.
    /// 없으면 CircleCollider2D를 자동으로 추가하고, 스프라이트 크기에 맞게 조정합니다.
    /// </summary>
    private void SetupCollider()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            // 원형 콜라이더 추가 (불덩어리에 적합)
            CircleCollider2D circleCollider = gameObject.AddComponent<CircleCollider2D>();
            circleCollider.isTrigger = true;

            // 스프라이트 크기에 맞게 콜라이더 크기 조정
            AdjustColliderSize(circleCollider);
        }
        else
        {
            // 이미 있으면 Trigger로 설정
            collider.isTrigger = true;
        }
    }

    /// <summary>
    /// 콜라이더 크기를 스프라이트 크기에 맞게 조정합니다.
    /// </summary>
    /// <param name="collider">크기를 조정할 CircleCollider2D</param>
    private void AdjustColliderSize(CircleCollider2D collider)
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            // 스프라이트의 가로, 세로 중 더 큰 값을 사용
            float spriteSize = Mathf.Max(spriteRenderer.bounds.size.x, spriteRenderer.bounds.size.y);
            // 반지름을 스프라이트 크기의 절반으로 설정
            collider.radius = spriteSize / 2f;
        }
    }
    #endregion

    #region Movement
    /// <summary>
    /// 이동을 중지해야 하는 조건을 체크합니다.
    /// 게임 오버 상태, 일시정지 상태, 속도가 0인 경우 이동을 중지합니다.
    /// </summary>
    /// <returns>이동을 중지해야 하면 true, 아니면 false</returns>
    private bool ShouldStopMovement()
    {
        // 게임 오버 상태면 이동 중지
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            return true;
        }

        // Time.timeScale이 0이면 이동하지 않음 (게임 일시정지)
        if (Time.timeScale <= 0)
        {
            return true;
        }

        // 속도가 0이면 이동하지 않음
        if (fallSpeed <= 0)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 불덩어리를 아래로 이동시킵니다.
    /// Rigidbody2D가 Kinematic이면 MovePosition을 사용하고, 아니면 transform을 직접 변경합니다.
    /// </summary>
    private void MoveDown()
    {
        float deltaTime = Time.deltaTime;
        if (deltaTime <= 0)
        {
            return;
        }

        // 아래 방향으로 이동 벡터 계산
        Vector3 movement = Vector3.down * fallSpeed * deltaTime;
        Vector3 newPosition = transform.position + movement;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null && rb.bodyType == RigidbodyType2D.Kinematic)
        {
            // Rigidbody2D가 Kinematic이면 MovePosition 사용 (물리 시뮬레이션과 호환)
            rb.MovePosition(newPosition);
        }
        else
        {
            // Rigidbody2D가 없거나 Kinematic이 아니면 transform 직접 변경
            transform.position = newPosition;
        }
    }

    /// <summary>
    /// 불덩어리가 화면 하단 경계를 벗어났는지 체크합니다.
    /// 경계를 벗어나면 불덩어리를 제거합니다.
    /// </summary>
    private void CheckBoundary()
    {
        if (transform.position.y < bottomBoundary)
        {
            DestroyFireball();
        }
    }
    #endregion

    #region Collision
    /// <summary>
    /// 충돌을 처리합니다.
    /// 플레이어와 충돌했는지 확인하고, 충돌했으면 게임 오버를 트리거합니다.
    /// </summary>
    /// <param name="other">충돌한 다른 오브젝트의 콜라이더</param>
    private void HandleCollision(Collider2D other)
    {
        // 이미 충돌했으면 무시 (중복 충돌 방지)
        if (hasCollided)
        {
            return;
        }

        // 플레이어와 충돌했는지 확인
        if (IsPlayer(other))
        {
            hasCollided = true;
            TriggerGameOver();
        }
    }

    /// <summary>
    /// 충돌한 오브젝트가 플레이어인지 확인합니다.
    /// "Player" 태그를 가지고 있거나 SheepController 컴포넌트가 있으면 플레이어로 판단합니다.
    /// </summary>
    /// <param name="other">확인할 콜라이더</param>
    /// <returns>플레이어이면 true, 아니면 false</returns>
    private bool IsPlayer(Collider2D other)
    {
        return other.CompareTag("Player") || other.GetComponent<SheepController>() != null;
    }

    /// <summary>
    /// 게임 오버를 트리거합니다.
    /// GameManager의 GameOver 메서드를 호출합니다.
    /// </summary>
    private void TriggerGameOver()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }
    #endregion

    #region Boundary Calculation
    /// <summary>
    /// 화면 하단 경계를 계산합니다.
    /// 카메라가 Orthographic이면 카메라 위치와 크기를 기반으로 계산하고,
    /// 아니면 기본값을 사용합니다.
    /// </summary>
    private void CalculateBottomBoundary()
    {
        if (mainCamera != null && mainCamera.orthographic)
        {
            // Orthographic 카메라의 하단 경계 계산
            float orthographicSize = mainCamera.orthographicSize;
            Vector3 cameraPos = mainCamera.transform.position;
            
            // 카메라 하단 위치에서 여유 공간을 뺀 값
            bottomBoundary = cameraPos.y - orthographicSize - BOUNDARY_OFFSET;
        }
        else
        {
            // 기본값 (카메라를 찾지 못한 경우)
            bottomBoundary = DEFAULT_BOTTOM_BOUNDARY;
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// 불덩어리의 낙하 속도를 설정합니다.
    /// </summary>
    /// <param name="speed">설정할 속도 (0보다 작거나 같으면 기본값 사용)</param>
    public void SetFallSpeed(float speed)
    {
        fallSpeed = speed > 0 ? speed : DEFAULT_FALL_SPEED;
    }

    /// <summary>
    /// 현재 낙하 속도를 가져옵니다.
    /// </summary>
    /// <returns>현재 낙하 속도</returns>
    public float GetFallSpeed()
    {
        return fallSpeed;
    }
    #endregion

    #region Destruction
    /// <summary>
    /// 불덩어리를 제거합니다.
    /// 제거 이벤트를 발생시키고 게임 오브젝트를 파괴합니다.
    /// </summary>
    private void DestroyFireball()
    {
        if (gameObject != null)
        {
            // 제거 이벤트 호출
            OnDestroyed?.Invoke();
            
            // 게임 오브젝트 파괴
            Destroy(gameObject);
        }
    }
    #endregion
}
