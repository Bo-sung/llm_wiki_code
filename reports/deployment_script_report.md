# Deployment Script Report

작성일: 2026-05-19

## 1. 수행 결과

- **완료**: PowerShell 배포 스크립트, smoke test 셸 스크립트, 배포 문서, 파라미터 검증, 빌드·테스트 통과, `..` 파라미터 바인딩 버그 수정
- **부분 완료**: 없음
- **미완료**: 실제 SSH 배포 검증 (Mac mini 미접근 환경)

## 버그 수정 이력

### 2차 수정: `'publish'` PositionalParameterNotFound (2026-05-19)

**현상**: `..` 수정 후에도 동일 오류, 인수만 `'publish'`로 바뀜

**원인**: PS5.1에서 `Join-Path`는 인수 2개만 허용 (`-Path`, `-ChildPath`). 스크립트 전반에 3~4개 인수 호출이 다수 존재. 세 번째 인수가 위치 파라미터로 처리되어 `PositionalParameterNotFound` 발생.

트리거 순서: `$ArtifactDir = Join-Path $ProjectRoot "artifacts" "publish" $Runtime` → 세 번째 인수 `"publish"`가 오류

추가로 동일 문제였던 호출들:
- `Join-Path $ProjectRoot "src" "LlmWiki.Cli" "LlmWiki.Cli.csproj"`
- `Join-Path $LocalData "System" "prompts" "refine_note.md"` (5곳)

**수정**: 모든 다중 인수 `Join-Path`를 2-인수 형태로 수정. 경로 세그먼트를 단일 문자열로 결합:

```powershell
# 이전 (PS5.1 실패)
$ArtifactDir = Join-Path $ProjectRoot "artifacts" "publish" $Runtime
$CliProject  = Join-Path $ProjectRoot "src" "LlmWiki.Cli" "LlmWiki.Cli.csproj"

# 수정 (PS5.1 호환)
$ArtifactDir = Join-Path $ProjectRoot "artifacts\publish\$Runtime"
$CliProject  = Join-Path $ProjectRoot "src\LlmWiki.Cli\LlmWiki.Cli.csproj"
```

수정 대상: 7곳 (ArtifactDir, CliProject, 5개 seed 경로)

**검증**: Syntax OK / Params OK / 경로 계산 정확 / dotnet build·test 30/30 통과

---

### 1차 수정: `..` PositionalParameterNotFound (2026-05-19)

### 현상

```text
'..' 인수를 허용하는 위치 매개 변수를 찾을 수 없습니다.
FullyQualifiedErrorId : PositionalParameterNotFound,deploy-to-mac.ps1
```

### 원인

`Join-Path` 호출에서 경로 세그먼트를 세 개의 분리된 인수로 전달:

```powershell
# 버그: PS5.1에서 Join-Path의 -ChildPath는 단일 문자열만 허용
$ProjectRoot = (Resolve-Path (Join-Path $ScriptDir ".." "..")).Path
```

PowerShell 5.1의 `Join-Path` 시그니처:
- `-Path <String[]>`
- `-ChildPath <String>` ← 단일 문자열

세 번째 인수 `".."` 가 위치 파라미터로 처리되어 바인딩 실패. `Set-StrictMode -Version Latest` 환경에서 이 오류가 호출 스크립트(`deploy-to-mac.ps1`) 이름으로 전파됨.

### 수정

```powershell
# 수정: 단일 문자열로 경로 세그먼트 결합
$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\.." )).Path
```

`"..\.."`는 단일 `string` 인수이므로 `-ChildPath` 바인딩 성공. `$ScriptDir` 변수도 불필요해져 제거.

### 추가 수정

`@"..."@` here-string을 배열 조인 방식으로 교체:

```powershell
# 변경 전: here-string (닫힘 "@ 의 열 위치에 의존)
Invoke-SSH @"
mkdir -p \
  $SharedData/Inbox/links \
  ...
"@

# 변경 후: 배열 조인 (PS 버전 무관)
$dataDirs = @("$SharedData/Inbox/links", ...)
Invoke-SSH ("mkdir -p " + ($dataDirs -join " "))
```

### 검증 결과

```text
- Syntax OK
- 베어 '..' 토큰: 없음
- Get-Command 파라미터 바인딩: RemoteHost, User, Port, Runtime, RemoteBase,
  Configuration, SelfContained, SkipTests, SkipSmokeTest 전부 인식
- ProjectRoot 계산: J:\prj\LLM_Wiki (정확)
- dotnet build: 경고 0개 오류 0개
- dotnet test: 30/30 통과
```

## 2. 생성/수정 파일

| 파일 | 목적 |
|---|---|
| [`scripts/deploy/deploy-to-mac.ps1`](../scripts/deploy/deploy-to-mac.ps1) | Windows → Mac mini 배포 자동화 |
| [`scripts/deploy/remote-smoke-test.sh`](../scripts/deploy/remote-smoke-test.sh) | Mac mini 원격 smoke test (셸 스크립트) |
| [`docs/DEPLOYMENT_WINDOWS_TO_MAC.md`](DEPLOYMENT_WINDOWS_TO_MAC.md) | 배포 전체 가이드 |

## 3. 배포 구조

```text
~/apps/llm-wiki/
  releases/
    {yyyyMMdd-HHmmss}/    ← publish 결과물 (dotnet publish)
  current -> releases/{최신}   ← 심볼릭 링크
  shared/
    data/                 ← 영구 데이터 (Inbox, Notes, System 등)
    .env                  ← Mac mini에서 직접 관리
    logs/                 ← 로그 (향후)
```

## 4. 실행 명령

```powershell
# 기본 배포 (Apple Silicon, 포트 22)
.\scripts\deploy\deploy-to-mac.ps1 -RemoteHost {HOST} -User {USER}

# Intel Mac, 비표준 SSH 포트
.\scripts\deploy\deploy-to-mac.ps1 -RemoteHost {HOST} -User {USER} -Port 2222 -Runtime osx-x64

# self-contained 배포
.\scripts\deploy\deploy-to-mac.ps1 -RemoteHost {HOST} -User {USER} -SelfContained

# 테스트 건너뛰고 배포
.\scripts\deploy\deploy-to-mac.ps1 -RemoteHost {HOST} -User {USER} -SkipTests
```

```bash
# dotnet build / test (C# 소스 검증)
dotnet build
dotnet test
```

## 5. 테스트 결과

```text
# 빌드
경고 0개  오류 0개

# 단위/통합 테스트
총 테스트 수: 30  통과: 30  총 시간: 0.54초

# PowerShell 파라미터 검증
- 문법 검사: Syntax OK
- 필수 파라미터 누락 시: "Cannot process command because of one or more missing mandatory parameters: RemoteHost User."
- 잘못된 Runtime 값 시: "Cannot validate argument on parameter 'Runtime'. The argument 'linux-x64' does not belong to the set 'osx-arm64,osx-x64'..."
```

## 6. 실제 SSH 배포 검증 여부

- **수행 여부**: 미수행
- **대상**: Mac mini (현재 환경에서 접근 불가)
- **결과**: N/A
- **미수행 사유**: 현재 개발 환경에서 Mac mini SSH 접근 불가

## 7. 데이터/비밀정보 보호 체크

| 항목 | 반영 여부 | 비고 |
|---|---|---|
| .env 미복사 | 완료 | 스크립트에 scp .env 코드 없음. 없으면 경고만 출력 |
| shared/data 보존 | 완료 | `mkdir -p`만 수행. 기존 데이터 삭제·덮어쓰기 없음 |
| release 분리 | 완료 | `releases/{timestamp}/`에 배포. 기존 release 유지 |
| current symlink 사용 | 완료 | `ln -sfn` 으로 현재 릴리즈 포인터만 갱신 |
| rollback 가능 | 완료 | `releases/` 보존, `ln -sfn {previous}` 롤백 절차 문서화 |

## Seed Data 초기화 정책

| 항목 | 정책 |
|---|---|
| 기본 디렉터리 | 배포 시 생성 |
| seed 파일 | 없을 때만 복사 |
| 기존 seed 파일 | 덮어쓰기 금지 |
| data 전체 복사 | 금지 |
| 사용자 수정 파일 | 보존 |

seed 대상 파일 (Step 4/8):
- `System/prompts/refine_note.md`
- `System/rules.md`
- `System/taxonomy.md`
- `Templates/note_template.md`
- `Templates/source_template.md`

## 8. 결정 필요

| 항목 | 선택지 | 권장안 | 이유 |
|---|---|---|---|
| launchd 등록 | 수동 실행 / launchd | launchd | `watch` 상시 실행 필요. Mac mini 검증 후 결정 |
| SSH 접근 방식 | 포트포워딩 / Tailscale | Tailscale 검토 | 공인 IP 직접 노출 없이 안전하게 접근 가능 |
| self-contained 배포 | false (프레임워크 의존) / true | false 우선 | Mac에 .NET 설치 시 배포 용량 감소. 미설치 시 true |
| ConfigLoader `.env` 경로 | 현재 작업 디렉터리 / 절대 경로 주입 | 절대 경로 주입 검토 | `shared/.env`를 자동 로딩하려면 경로 파라미터화 필요 |
| 오래된 releases 정리 | 수동 / 자동 cleanup | 수동 우선 | 안정화 후 결정 |

## 9. 리스크 / 주의점

- 실제 배포 검증 미수행. Mac mini에서 처음 배포 시 scp 디렉터리 복사 구조 확인 필요. (추정: scp -r이 디렉터리명을 포함한 서브디렉터리를 생성할 수 있음)
- ConfigLoader가 현재 작업 디렉터리의 `.env`를 찾으므로, `cd ~/apps/llm-wiki/current` 후 `.env`가 없으면 환경 변수 로딩 실패. `shared/.env`를 `current/` 경로에서 찾도록 경로 처리 개선 검토 필요.
- self-contained 배포 선택 시 실행 파일명이 `LlmWiki.Cli` (확장자 없음)로 생성됨. smoke test 및 수동 실행 명령이 달라짐.
- SSH BatchMode=yes 설정으로 비밀번호 로그인 불가. SSH 키 설정이 사전에 완료되어야 스크립트 실행 가능.

## 10. 다음 작업 후보

1. Mac mini 실환경 배포 검증 (`deploy-to-mac.ps1` 실제 실행)
2. ConfigLoader에 `.env` 경로 파라미터 추가 (shared/.env 자동 로딩)
3. launchd plist 파일 생성 및 등록 절차 문서화 (Mac mini 검증 이후)
