# Unity Sheep Escape

Unity로 제작된 2D 플랫포머 게임입니다. 플레이어는 양을 조작하여 위에서 떨어지는 불덩어리를 피해야 합니다.

## 🎮 게임 소개

**Sheep Escape**는 간단하고 재미있는 2D 회피 게임입니다. 좌우로 이동하는 양을 조작하여 화면 위에서 떨어지는 불덩어리를 피하세요. 불덩어리에 맞으면 게임 오버입니다!

## ✨ 주요 기능

- **간단한 조작**: 좌우 방향키 또는 A/D 키로 양을 이동
- **동적 스폰 시스템**: 카메라 범위를 기반으로 불덩어리가 랜덤하게 생성
- **게임 상태 관리**: 싱글톤 패턴을 사용한 중앙 게임 관리 시스템
- **자동 컴포넌트 설정**: 콜라이더와 리지드바디가 자동으로 설정됨
- **카메라 경계 제한**: 플레이어가 화면 밖으로 나가지 않도록 자동 제한

## 🎯 게임 플레이 방법

1. **이동**: 좌우 방향키 또는 A/D 키를 사용하여 양을 이동합니다
2. **목표**: 떨어지는 불덩어리를 피하세요
3. **게임 오버**: 불덩어리에 맞으면 게임이 종료됩니다

## 📁 프로젝트 구조

```
Assets/
└── Scripts/
    ├── FallingFireball.cs      # 떨어지는 불덩어리 제어
    ├── FireballSpawner.cs      # 불덩어리 생성 및 관리
    ├── GameManager.cs          # 게임 상태 관리 (싱글톤)
    └── SheepController.cs      # 플레이어(양) 컨트롤러
```

## 🔧 스크립트 설명

### FallingFireball.cs
- 화면 위에서 떨어지는 불덩어리를 제어하는 클래스
- 플레이어와 충돌 시 게임 오버 트리거
- 화면 밖으로 나가면 자동 제거
- Rigidbody2D와 Collider2D 자동 설정

### FireballSpawner.cs
- 불덩어리를 생성하고 관리하는 클래스
- 카메라 범위를 기반으로 랜덤 위치에 스폰
- 시간차로 여러 개의 불덩어리를 생성
- 중복 생성 방지 메커니즘 포함

### GameManager.cs
- 게임의 전반적인 상태를 관리하는 싱글톤 클래스
- 게임 오버 및 재시작 기능
- 이벤트 시스템 지원 (OnGameOverEvent, OnGameRestartEvent)
- 씬 전환 시에도 유지됨 (DontDestroyOnLoad)

### SheepController.cs
- 플레이어(양)의 이동과 입력을 제어하는 클래스
- 좌우 이동 및 스프라이트 방향 전환
- 카메라 경계 내에서만 이동 가능
- Rigidbody2D와 Collider2D 자동 설정

## 🛠️ 기술 스택

- **Unity Engine**: 2D 게임 개발
- **C#**: 스크립트 언어
- **Unity 2D Physics**: 충돌 감지 및 물리 시뮬레이션

## 📋 요구 사항

- Unity 2021.3 이상 (또는 호환 버전)
- 2D Pixel Art Platformer Biome - Plains 에셋 (선택사항)

## 🚀 설치 및 실행 방법

1. 이 저장소를 클론합니다:
   ```bash
   git clone https://github.com/your-username/unity-catEscape.git
   ```

2. Unity Hub에서 프로젝트를 엽니다

3. `Assets/Scenes/SampleScene.unity` 씬을 엽니다

4. Play 버튼을 눌러 게임을 실행합니다

## 🎨 설정 방법

### FireballSpawner 설정
- `fireballPrefab`: 생성할 불덩어리 프리팹 할당
- `fireballCount`: 생성할 불덩어리 개수
- `spawnDelayMin/Max`: 불덩어리 생성 간격 (초)
- `fireballSpeed`: 불덩어리 낙하 속도
- `useCameraBounds`: 카메라 범위 사용 여부

### SheepController 설정
- `moveSpeed`: 양의 이동 속도

## 📝 라이선스

이 프로젝트는 개인 프로젝트입니다.

## 👨‍💻 개발자

프로젝트 개발자 정보를 여기에 추가하세요.

## 📞 문의

문제가 발생하거나 제안사항이 있으면 이슈를 등록해주세요.

---

**즐거운 게임 되세요! 🎮**
