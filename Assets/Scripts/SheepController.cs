using UnityEngine;

/// <summary>
/// 양(플레이어)의 이동과 입력을 제어하는 클래스
/// 좌우 방향키 입력을 받아 양을 이동시키고, 카메라 경계 내에서만 이동하도록 제한합니다.
/// </summary>
public class SheepController : MonoBehaviour
{
    #region Constants
    /// <summary>기본 이동 속도 (초당 유닛)</summary>
    private const float DEFAULT_MOVE_SPEED = 5f;
    
    /// <summary>플레이어 태그 이름</summary>
    private const string PLAYER_TAG = "Player";
    #endregion

    #region Serialized Fields
    /// <summary>양의 이동 속도 (Inspector에서 설정 가능)</summary>
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = DEFAULT_MOVE_SPEED;
    #endregion

    #region Private Fields
    /// <summary>스프라이트 렌더러 컴포넌트 (스프라이트 뒤집기용)</summary>
    private SpriteRenderer spriteRenderer;
    
    /// <summary>수평 입력 값 (-1: 왼쪽, 0: 없음, 1: 오른쪽)</summary>
    private float horizontalInput;
    
    /// <summary>메인 카메라 참조 (경계 계산용)</summary>
    private Camera mainCamera;
    
    /// <summary>이동 가능한 최소 X 좌표 (카메라 경계)</summary>
    private float minX, maxX;
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
    /// 입력을 받아 양을 이동시키고, 스프라이트 방향을 조정합니다.
    /// </summary>
    void Update()
    {
        // 게임 오버 상태면 입력 무시
        if (IsGameOver())
        {
            return;
        }

        // 입력 처리
        HandleInput();
        
        // 이동 처리
        Move();
        
        // 스프라이트 방향 전환
        FlipSprite();
    }
    #endregion

    #region Initialization
    /// <summary>
    /// 양의 초기화 작업을 수행합니다.
    /// 컴포넌트 초기화, 카메라 초기화, 경계 계산, 콜라이더 및 리지드바디 설정을 진행합니다.
    /// </summary>
    private void Initialize()
    {
        // 컴포넌트 초기화
        InitializeComponents();
        
        // 카메라 초기화
        InitializeCamera();
        
        // 카메라 경계 계산
        CalculateCameraBounds();
        
        // 콜라이더 자동 설정
        SetupCollider();
        
        // 리지드바디 자동 설정
        SetupRigidbody();
    }

    /// <summary>
    /// 필요한 컴포넌트를 가져옵니다.
    /// </summary>
    private void InitializeComponents()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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
    #endregion

    #region Setup Methods
    /// <summary>
    /// Rigidbody2D 컴포넌트를 설정합니다.
    /// 없으면 자동으로 추가하고, Kinematic 타입으로 설정하여 물리 영향 없이 이동합니다.
    /// </summary>
    private void SetupRigidbody()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            // 리지드바디가 없으면 추가
            rb = gameObject.AddComponent<Rigidbody2D>();
            
            // Kinematic으로 설정하여 물리 영향 없이 이동
            rb.bodyType = RigidbodyType2D.Kinematic;
            
            // 중력 비활성화
            rb.gravityScale = 0;
            
            // 회전 방지
            rb.freezeRotation = true;
        }
    }

    /// <summary>
    /// Collider2D 컴포넌트를 설정합니다.
    /// 없으면 BoxCollider2D를 자동으로 추가하고, 스프라이트 크기에 맞게 조정합니다.
    /// </summary>
    private void SetupCollider()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            // 박스 콜라이더 추가 (양에 적합)
            BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
            boxCollider.isTrigger = true;

            // 스프라이트 크기에 맞게 콜라이더 크기 조정
            AdjustColliderSize(boxCollider);
        }
        else
        {
            // 이미 있으면 Trigger로 설정
            collider.isTrigger = true;
        }

        // Player 태그 설정
        SetPlayerTag();
    }

    /// <summary>
    /// 콜라이더 크기를 스프라이트 크기에 맞게 조정합니다.
    /// </summary>
    /// <param name="collider">크기를 조정할 BoxCollider2D</param>
    private void AdjustColliderSize(BoxCollider2D collider)
    {
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            // 스프라이트의 실제 크기를 콜라이더 크기로 설정
            collider.size = spriteRenderer.bounds.size;
        }
    }

    /// <summary>
    /// 게임 오브젝트의 태그를 "Player"로 설정합니다.
    /// 다른 오브젝트에서 플레이어를 식별할 수 있도록 합니다.
    /// </summary>
    private void SetPlayerTag()
    {
        if (!gameObject.CompareTag(PLAYER_TAG))
        {
            gameObject.tag = PLAYER_TAG;
        }
    }
    #endregion

    #region Camera Bounds
    /// <summary>
    /// 카메라의 경계를 계산합니다.
    /// Orthographic 카메라의 크기를 기반으로 양이 이동할 수 있는 X 좌표 범위를 계산합니다.
    /// 스프라이트 크기를 고려하여 화면 밖으로 나가지 않도록 합니다.
    /// </summary>
    private void CalculateCameraBounds()
    {
        // 카메라가 없거나 Orthographic이 아니면 계산하지 않음
        if (mainCamera == null || !mainCamera.orthographic)
        {
            return;
        }

        // 카메라의 Orthographic 크기와 종횡비 가져오기
        float orthographicSize = mainCamera.orthographicSize;
        float aspect = mainCamera.aspect;
        Vector3 cameraPos = mainCamera.transform.position;

        // 카메라의 실제 높이와 너비 계산
        float cameraHeight = orthographicSize * 2f;
        float cameraWidth = cameraHeight * aspect;

        // 스프라이트의 너비 가져오기 (경계 계산 시 스프라이트가 화면 밖으로 나가지 않도록)
        float spriteWidth = spriteRenderer != null ? spriteRenderer.bounds.size.x : 0.5f;
        
        // 이동 가능한 X 좌표 범위 계산
        // 카메라 왼쪽 경계 + 스프라이트 반 너비 (왼쪽 절반이 화면 밖으로 나가지 않도록)
        minX = cameraPos.x - cameraWidth / 2f + spriteWidth / 2f;
        
        // 카메라 오른쪽 경계 - 스프라이트 반 너비 (오른쪽 절반이 화면 밖으로 나가지 않도록)
        maxX = cameraPos.x + cameraWidth / 2f - spriteWidth / 2f;
    }
    #endregion

    #region Input & Movement
    /// <summary>
    /// 게임 오버 상태인지 확인합니다.
    /// </summary>
    /// <returns>게임 오버 상태이면 true, 아니면 false</returns>
    private bool IsGameOver()
    {
        return GameManager.Instance != null && GameManager.Instance.IsGameOver;
    }

    /// <summary>
    /// 플레이어의 입력을 처리합니다.
    /// 수평 방향키(좌우 화살표, A/D 키) 입력을 받아 horizontalInput에 저장합니다.
    /// </summary>
    private void HandleInput()
    {
        // GetAxisRaw는 -1, 0, 1 값만 반환 (부드러운 가속/감속 없음)
        horizontalInput = Input.GetAxisRaw("Horizontal");
    }

    /// <summary>
    /// 양을 이동시킵니다.
    /// 입력값에 따라 좌우로 이동하며, 카메라 경계 내에서만 이동하도록 제한합니다.
    /// </summary>
    private void Move()
    {
        // 이동 벡터 계산 (X축만 이동, Y와 Z는 0)
        Vector3 movement = new Vector3(horizontalInput * moveSpeed * Time.deltaTime, 0, 0);
        
        // 새로운 위치 계산
        Vector3 newPosition = transform.position + movement;

        // 카메라 경계 내로 제한
        if (mainCamera != null)
        {
            // Clamp를 사용하여 minX와 maxX 사이로 제한
            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
        }

        // 위치 적용
        transform.position = newPosition;
    }

    /// <summary>
    /// 입력 방향에 따라 스프라이트를 뒤집습니다.
    /// 오른쪽 입력 시 정방향, 왼쪽 입력 시 좌우 반전합니다.
    /// </summary>
    private void FlipSprite()
    {
        // 스프라이트 렌더러가 없으면 처리하지 않음
        if (spriteRenderer == null)
        {
            return;
        }

        // 오른쪽 입력 시 정방향 (flipX = false)
        if (horizontalInput > 0)
        {
            spriteRenderer.flipX = false;
        }
        // 왼쪽 입력 시 좌우 반전 (flipX = true)
        else if (horizontalInput < 0)
        {
            spriteRenderer.flipX = true;
        }
        // 입력이 없으면 이전 상태 유지
    }
    #endregion
}
