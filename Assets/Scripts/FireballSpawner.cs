using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 불덩어리를 생성하고 관리하는 클래스
/// 카메라 범위를 기반으로 불덩어리를 랜덤하게 생성하며, 시간차로 여러 개를 스폰합니다.
/// </summary>
public class FireballSpawner : MonoBehaviour
{
    #region Constants
    /// <summary>기본 스폰 높이 (카메라 범위 미사용 시)</summary>
    private const float DEFAULT_SPAWN_HEIGHT = 8f;
    
    /// <summary>기본 최소 스폰 간격 (초)</summary>
    private const float DEFAULT_SPAWN_DELAY_MIN = 0.5f;
    
    /// <summary>기본 최대 스폰 간격 (초)</summary>
    private const float DEFAULT_SPAWN_DELAY_MAX = 2f;
    
    /// <summary>기본 불덩어리 낙하 속도</summary>
    private const float DEFAULT_FIREBALL_SPEED = 5f;
    
    /// <summary>기본 최소 X 좌표 (카메라 범위 미사용 시)</summary>
    private const float DEFAULT_MIN_X = -5f;
    
    /// <summary>기본 최대 X 좌표 (카메라 범위 미사용 시)</summary>
    private const float DEFAULT_MAX_X = 5f;
    
    /// <summary>카메라 상단에서 스폰 위치까지의 오프셋</summary>
    private const float CAMERA_TOP_OFFSET = 0.5f;
    #endregion

    #region Static Fields
    /// <summary>이미 불덩어리를 생성했는지 여부 (중복 생성 방지)</summary>
    public static bool hasSpawned = false;
    
    /// <summary>생성된 모든 불덩어리 오브젝트를 추적하는 리스트</summary>
    public static List<GameObject> spawnedFireballs = new List<GameObject>();
    #endregion

    #region Serialized Fields
    /// <summary>생성할 불덩어리 프리팹</summary>
    [Header("Fireball 설정")]
    public GameObject fireballPrefab;
    
    /// <summary>불덩어리를 생성할 높이 (Y 좌표)</summary>
    public float spawnHeight = DEFAULT_SPAWN_HEIGHT;
    
    /// <summary>생성할 불덩어리의 총 개수</summary>
    public int fireballCount = 10;
    
    /// <summary>불덩어리 생성 간격의 최소값 (초)</summary>
    public float spawnDelayMin = DEFAULT_SPAWN_DELAY_MIN;
    
    /// <summary>불덩어리 생성 간격의 최대값 (초)</summary>
    public float spawnDelayMax = DEFAULT_SPAWN_DELAY_MAX;

    /// <summary>불덩어리의 낙하 속도</summary>
    [Header("불덩어리 속도 설정")]
    public float fireballSpeed = DEFAULT_FIREBALL_SPEED;

    /// <summary>카메라 범위를 사용하여 스폰 위치를 계산할지 여부</summary>
    [Header("스폰 범위")]
    public bool useCameraBounds = true;
    
    /// <summary>카메라 범위 미사용 시 최소 X 좌표</summary>
    public float minX = DEFAULT_MIN_X;
    
    /// <summary>카메라 범위 미사용 시 최대 X 좌표</summary>
    public float maxX = DEFAULT_MAX_X;
    #endregion

    #region Private Fields
    /// <summary>메인 카메라 참조</summary>
    public Camera mainCamera;
    
    /// <summary>실제 스폰 범위의 최소 X 좌표 (계산된 값)</summary>
    public float spawnMinX, spawnMaxX;
    #endregion

    #region Unity Lifecycle
    /// <summary>
    /// 게임 오브젝트가 활성화될 때 호출됩니다.
    /// 이미 생성했으면 중복 실행을 방지하고, 아니면 초기화를 진행합니다.
    /// </summary>
    void Start()
    {
        // 이미 생성했으면 중복 실행 방지
        if (hasSpawned)
        {
            return;
        }

        Initialize();
    }

    /// <summary>
    /// 게임 오브젝트가 파괴될 때 호출됩니다.
    /// 스폰 상태를 리셋하여 다음 씬에서 다시 생성할 수 있도록 합니다.
    /// </summary>
    void OnDestroy()
    {
        ResetSpawnState();
    }
    #endregion

    #region Initialization
    /// <summary>
    /// 스포너의 초기화 작업을 수행합니다.
    /// 기존 불덩어리 정리, 카메라 초기화, 프리팹 찾기, 스폰 범위 계산, 스폰 시작을 진행합니다.
    /// </summary>
    private void Initialize()
    {
        // 기존 불덩어리 모두 제거
        CleanupAllExistingFireballs();
        
        // 카메라 초기화
        InitializeCamera();
        
        // 프리팹이 없으면 자동으로 찾기
        FindFireballPrefabIfNeeded();
        
        // 스폰 범위 계산
        CalculateSpawnBounds();
        
        // 불덩어리 생성 시작
        StartSpawning();
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
    /// 프리팹이 설정되지 않았으면 자동으로 찾아서 설정합니다.
    /// </summary>
    private void FindFireballPrefabIfNeeded()
    {
        if (fireballPrefab == null)
        {
            TryFindFireballPrefab();
        }
    }

    /// <summary>
    /// 불덩어리 생성 코루틴을 시작합니다.
    /// 프리팹이 설정되어 있으면 스폰을 시작하고, hasSpawned 플래그를 설정합니다.
    /// </summary>
    private void StartSpawning()
    {
        if (fireballPrefab != null)
        {
            // 코루틴으로 시간차 생성 시작
            StartCoroutine(SpawnFireballsWithDelay());
            
            // 생성 플래그 설정 (중복 생성 방지)
            hasSpawned = true;
        }
    }
    #endregion

    #region Cleanup
    /// <summary>
    /// 씬에 있는 모든 기존 불덩어리를 제거합니다.
    /// 프리팹 오브젝트는 제외하고, 생성된 불덩어리만 제거합니다.
    /// </summary>
    public void CleanupAllExistingFireballs()
    {
        // 씬에 있는 모든 FallingFireball 컴포넌트 찾기
        FallingFireball[] existingFireballs = FindObjectsOfType<FallingFireball>();

        foreach (var fireball in existingFireballs)
        {
            // 프리팹이 아니고 유효한 오브젝트만 제거
            if (fireball != null && fireball.gameObject != null && fireball.gameObject != fireballPrefab)
            {
#if UNITY_EDITOR
                // 에디터에서는 즉시 제거
                DestroyImmediate(fireball.gameObject);
#else
                // 런타임에서는 일반 Destroy 사용
                Destroy(fireball.gameObject);
#endif
            }
        }

        // 생성된 불덩어리 리스트 초기화
        spawnedFireballs.Clear();
    }

    /// <summary>
    /// 스폰 상태를 리셋합니다.
    /// 씬이 다시 로드될 때 다시 생성할 수 있도록 플래그와 리스트를 초기화합니다.
    /// </summary>
    private void ResetSpawnState()
    {
        hasSpawned = false;
        spawnedFireballs.Clear();
    }
    #endregion

    #region Prefab Finding
    /// <summary>
    /// 씬에서 불덩어리 프리팹을 자동으로 찾아서 설정합니다.
    /// 먼저 특정 이름("@asset_fire")의 오브젝트를 찾고,
    /// 없으면 비활성화된 FallingFireball 컴포넌트가 있는 오브젝트를 찾습니다.
    /// </summary>
    public void TryFindFireballPrefab()
    {
        // 씬에서 특정 이름의 불덩어리 오브젝트 찾기
        GameObject fireballInScene = GameObject.Find("@asset_fire");
        if (fireballInScene != null && !fireballInScene.activeInHierarchy)
        {
            fireballPrefab = fireballInScene;
            return;
        }

        // 비활성화된 FallingFireball 컴포넌트가 있는 오브젝트 찾기
        // FindObjectsOfType의 두 번째 매개변수를 true로 설정하여 비활성화된 오브젝트도 포함
        FallingFireball[] allFireballs = FindObjectsOfType<FallingFireball>(true);
        foreach (var fireball in allFireballs)
        {
            if (fireball != null && fireball.gameObject != null && !fireball.gameObject.activeInHierarchy)
            {
                fireballPrefab = fireball.gameObject;
                return;
            }
        }
    }
    #endregion

    #region Spawn Bounds
    /// <summary>
    /// 불덩어리를 생성할 범위를 계산합니다.
    /// useCameraBounds가 true이고 카메라가 Orthographic이면 카메라 범위를 기반으로 계산하고,
    /// 아니면 수동으로 설정한 minX, maxX 값을 사용합니다.
    /// </summary>
    public void CalculateSpawnBounds()
    {
        if (useCameraBounds && mainCamera != null && mainCamera.orthographic)
        {
            // 카메라 범위 기반으로 계산
            CalculateCameraBasedBounds();
        }
        else
        {
            // 수동 설정 값 사용
            UseManualBounds();
        }
    }

    /// <summary>
    /// 카메라의 Orthographic 크기를 기반으로 스폰 범위를 계산합니다.
    /// 카메라의 너비와 높이를 계산하여 화면 전체 너비를 스폰 범위로 사용합니다.
    /// </summary>
    private void CalculateCameraBasedBounds()
    {
        // 카메라의 Orthographic 크기와 종횡비 가져오기
        float orthographicSize = mainCamera.orthographicSize;
        float aspect = mainCamera.aspect;
        Vector3 cameraPos = mainCamera.transform.position;

        // 카메라의 실제 너비 계산 (높이 * 종횡비)
        float cameraWidth = orthographicSize * 2f * aspect;
        
        // 스폰 범위의 X 좌표 계산 (카메라 중심 기준)
        spawnMinX = cameraPos.x - cameraWidth / 2f;
        spawnMaxX = cameraPos.x + cameraWidth / 2f;

        // 스폰 높이를 카메라 상단 기준으로 설정
        float cameraTop = cameraPos.y + orthographicSize;
        spawnHeight = cameraTop + CAMERA_TOP_OFFSET;
    }

    /// <summary>
    /// 수동으로 설정한 minX, maxX 값을 스폰 범위로 사용합니다.
    /// </summary>
    private void UseManualBounds()
    {
        spawnMinX = minX;
        spawnMaxX = maxX;
    }
    #endregion

    #region Spawning
    /// <summary>
    /// 불덩어리를 시간차로 생성하는 코루틴입니다.
    /// fireballCount만큼 불덩어리를 생성하며, 각각 spawnDelayMin과 spawnDelayMax 사이의 랜덤한 시간 간격으로 생성합니다.
    /// </summary>
    /// <returns>코루틴 이터레이터</returns>
    public IEnumerator SpawnFireballsWithDelay()
    {
        // 이미 생성된 불덩어리가 있으면 생성하지 않음
        if (spawnedFireballs.Count > 0)
        {
            yield break;
        }

        // 지정된 개수만큼 불덩어리 생성
        for (int i = 0; i < fireballCount; i++)
        {
            // 불덩어리 하나 생성
            SpawnSingleFireball();

            // 마지막 불덩어리가 아니면 랜덤한 시간만큼 대기
            if (i < fireballCount - 1)
            {
                // 최소값과 최대값 사이의 랜덤한 지연 시간
                float delay = Random.Range(spawnDelayMin, spawnDelayMax);
                
                // WaitForSecondsRealtime을 사용하여 Time.timeScale의 영향을 받지 않음
                yield return new WaitForSecondsRealtime(delay);
            }
        }
    }

    /// <summary>
    /// 불덩어리 하나를 생성합니다.
    /// 랜덤한 X 위치에 불덩어리를 생성하고, 속도를 설정한 후 리스트에 추가합니다.
    /// </summary>
    private void SpawnSingleFireball()
    {
        // 랜덤한 스폰 위치 계산
        Vector3 spawnPosition = GetRandomSpawnPosition();
        
        // 프리팹을 인스턴스화하여 생성
        GameObject fireball = Instantiate(fireballPrefab, spawnPosition, Quaternion.identity);
        
        // 오브젝트 활성화
        fireball.SetActive(true);
        
        // 생성된 불덩어리를 리스트에 추가 (추적용)
        spawnedFireballs.Add(fireball);

        // 불덩어리 속도 설정
        ConfigureFireball(fireball);
    }

    /// <summary>
    /// 랜덤한 스폰 위치를 계산합니다.
    /// spawnMinX와 spawnMaxX 사이의 랜덤한 X 좌표와 spawnHeight의 Y 좌표를 사용합니다.
    /// </summary>
    /// <returns>랜덤한 스폰 위치</returns>
    private Vector3 GetRandomSpawnPosition()
    {
        // X 좌표를 랜덤하게 선택
        float randomX = Random.Range(spawnMinX, spawnMaxX);
        
        // Y 좌표는 설정된 스폰 높이 사용, Z 좌표는 0
        return new Vector3(randomX, spawnHeight, 0);
    }

    /// <summary>
    /// 생성된 불덩어리의 속도를 설정합니다.
    /// FallingFireball 컴포넌트가 있으면 SetFallSpeed 메서드를 호출하여 속도를 설정합니다.
    /// </summary>
    /// <param name="fireball">설정할 불덩어리 오브젝트</param>
    private void ConfigureFireball(GameObject fireball)
    {
        FallingFireball fallingFireball = fireball.GetComponent<FallingFireball>();
        if (fallingFireball != null)
        {
            // 불덩어리의 낙하 속도 설정
            fallingFireball.SetFallSpeed(fireballSpeed);
        }
    }
    #endregion
}
