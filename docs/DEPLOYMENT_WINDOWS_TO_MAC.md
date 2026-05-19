# Windows → Mac mini 배포 가이드

개발: Windows PC  
실행: Mac mini (SSH 접근 가능)

---

## 1. 전제 조건

### Windows (개발 환경)
- .NET 8 SDK 이상
- OpenSSH 클라이언트 (`ssh`, `scp` 명령 사용 가능)
  - Windows 10/11: 설정 → 앱 → 선택적 기능 → OpenSSH 클라이언트
- PowerShell 5.1 이상

### Mac mini (실행 환경)
- .NET 8 Runtime 이상 (프레임워크 의존 배포 시)
  - self-contained 배포를 선택하면 불필요
- SSH 서버 활성화 (시스템 설정 → 공유 → 원격 로그인)
- 포트포워딩 또는 Tailscale로 외부 접근 가능한 상태

---

## 2. Mac mini SSH 접속 확인

Windows PowerShell에서:

```powershell
ssh -p {PORT} {USER}@{HOST}
```

SSH 키 기반 로그인이 설정되어 있어야 한다:

```powershell
# Windows에서 키 생성 (이미 있으면 생략)
ssh-keygen -t ed25519

# 공개키를 Mac mini에 복사
ssh-copy-id -p {PORT} {USER}@{HOST}
# 또는 수동으로 ~/.ssh/authorized_keys에 추가
```

---

## 3. Mac mini `.env` 생성

배포 스크립트는 `.env`를 생성하지 않는다. Mac mini에서 직접 생성한다.

```bash
mkdir -p ~/apps/llm-wiki/shared
nano ~/apps/llm-wiki/shared/.env
```

내용:

```env
GEMINI_API_KEY=your_key_here
GEMINI_MODEL=gemini-1.5-flash
LLM_WIKI_ROOT=~/apps/llm-wiki/shared/data
```

`GEMINI_API_KEY` 또는 `GEMINI_MODEL`이 비어 있으면 fallback 모드로 동작한다.

---

## 4. Windows에서 배포 실행

프로젝트 루트에서:

```powershell
.\scripts\deploy\deploy-to-mac.ps1 -RemoteHost {HOST} -User {USER}
```

### 주요 파라미터

| 파라미터 | 기본값 | 설명 |
|---|---|---|
| `-RemoteHost` | 필수 | Mac mini 호스트명 또는 IP |
| `-User` | 필수 | SSH 사용자명 |
| `-Port` | `22` | SSH 포트 |
| `-Runtime` | `osx-arm64` | Apple Silicon이면 `osx-arm64`, Intel이면 `osx-x64` |
| `-RemoteBase` | `~/apps/llm-wiki` | Mac mini 배포 기본 경로 |
| `-Configuration` | `Release` | 빌드 구성 |
| `-SelfContained` | `$false` | Mac에 .NET 미설치 시 `$true` |
| `-SkipTests` | `$false` | 로컬 테스트 건너뜀 |
| `-SkipSmokeTest` | `$false` | 원격 smoke test 건너뜀 |

### Apple Silicon vs Intel

| Mac mini 종류 | `-Runtime` 값 |
|---|---|
| Apple Silicon (M1/M2/M3) | `osx-arm64` (기본값) |
| Intel | `osx-x64` |

확인 방법 (Mac mini에서):
```bash
uname -m
# arm64 → Apple Silicon
# x86_64 → Intel
```

### 사용 예

```powershell
# 기본 배포 (Apple Silicon, 포트 22)
.\scripts\deploy\deploy-to-mac.ps1 -RemoteHost 192.168.1.100 -User myuser

# 포트포워딩, Intel Mac
.\scripts\deploy\deploy-to-mac.ps1 -RemoteHost my-mac.example.com -User myuser -Port 2222 -Runtime osx-x64

# self-contained 배포 (Mac에 .NET 미설치)
.\scripts\deploy\deploy-to-mac.ps1 -RemoteHost my-mac.local -User myuser -SelfContained

# 테스트 건너뛰고 빠른 배포
.\scripts\deploy\deploy-to-mac.ps1 -RemoteHost my-mac.local -User myuser -SkipTests

# public vault Git 정보 전달 및 초기화
.\scripts\deploy\deploy-to-mac.ps1 -RemoteHost my-mac.local -User myuser `
  -PublicVaultRepoUrl "git@github.com:user/llm-wiki-vault-public.git" `
  -GitAuthorName "boseong" `
  -GitAuthorEmail "public@example.com" `
  -SetupPublicVault
```

### Public Vault 관련 파라미터

| 파라미터 | 기본값 | 설명 |
|---|---|---|
| `-PublicVaultRepoUrl` | 빈 값 | GitHub remote URL (git@github.com:...) |
| `-PublicVaultBranch` | `main` | 기본 브랜치 |
| `-PublicVaultPath` | `$RemoteBase/public-vault` | Mac mini 내 vault 경로 |
| `-GitAuthorName` | 빈 값 | git config user.name |
| `-GitAuthorEmail` | 빈 값 | git config user.email |
| `-SetupPublicVault` | `$false` | vault 초기화 수행 여부 |

**Git 인증 정보는 전달하지 않는다.** token, SSH key, password는 Mac mini에서 별도 설정한다.

참조: [`docs/PUBLIC_VAULT_POLICY.md`](PUBLIC_VAULT_POLICY.md)

---

## 5. 배포 구조

Mac mini의 디렉터리 구조:

```
~/apps/llm-wiki/
  releases/
    20260519-143022/      ← 배포 타임스탬프
      LlmWiki.Cli.dll
      LlmWiki.Core.dll
      ...
    20260520-090100/
      ...
  current -> releases/20260520-090100   ← 현재 활성 릴리즈 (심볼릭 링크)
  shared/
    data/                 ← 영구 데이터 (배포 스크립트가 절대 삭제하지 않음)
      Inbox/links/
      Inbox/raw_clips/
      Inbox/mobile/
      Inbox/processed/    ← InboxMover가 처리 시점에 YYYY-MM-DD/ 서브디렉터리 생성
      Inbox/failed/       ← 동일
      Notes/References/   ← 출력 노트
      System/prompts/     ← Gemini 프롬프트 (seed 파일로 초기화)
      System/rules.md     ← seed 파일
      System/taxonomy.md  ← seed 파일
      Templates/          ← seed 파일
    .env                  ← 비밀 정보 (Mac mini에서 직접 관리, 배포 시 덮어쓰기 금지)
    logs/                 ← 로그 파일 (향후)
```

### 배포 스크립트의 shared/data 처리 정책

| 동작 | 정책 |
|---|---|
| 기본 디렉터리 생성 | 배포 시 `mkdir -p`로 생성 (이미 있으면 유지) |
| seed 파일 초기화 | 원격에 파일이 없을 때만 업로드 |
| 기존 seed 파일 | 덮어쓰기 금지 (사용자 수정 내용 보존) |
| `data/` 전체 복사 | 금지 (`scp -r data/` 사용 안 함) |
| `shared/data` 삭제 | 금지 |

seed 파일 목록:

```text
data/System/prompts/refine_note.md  →  shared/data/System/prompts/refine_note.md
data/System/rules.md                →  shared/data/System/rules.md
data/System/taxonomy.md             →  shared/data/System/taxonomy.md
data/Templates/note_template.md     →  shared/data/Templates/note_template.md
data/Templates/source_template.md   →  shared/data/Templates/source_template.md
```

---

## 6. 배포 후 smoke test

배포 스크립트가 자동으로 수행한다. 수동으로 실행할 경우:

```bash
# Mac mini에서
bash ~/apps/llm-wiki/current/remote-smoke-test.sh
```

또는 스크립트를 업로드한 경우:

```bash
bash scripts/deploy/remote-smoke-test.sh ~/apps/llm-wiki false
```

확인 항목:
- `current` 심볼릭 링크 존재
- `shared/data` 존재
- `shared/.env` 존재 여부 (없으면 경고, 실패 아님)
- `LlmWiki.Cli.dll` 존재
- `process-once --dry-run` 정상 실행

---

## 7. Mac mini 수동 실행

`LLM_WIKI_ENV_FILE`로 `shared/.env`를 명시적으로 지정한다. shell sourcing 불필요.

### 프레임워크 의존 배포 (기본)

```bash
cd ~/apps/llm-wiki/current

# 단일 처리
LLM_WIKI_ENV_FILE=~/apps/llm-wiki/shared/.env \
LLM_WIKI_ROOT=~/apps/llm-wiki/shared/data \
/Users/boseong/.dotnet/dotnet LlmWiki.Cli.dll process-once

# dry-run
LLM_WIKI_ENV_FILE=~/apps/llm-wiki/shared/.env \
LLM_WIKI_ROOT=~/apps/llm-wiki/shared/data \
/Users/boseong/.dotnet/dotnet LlmWiki.Cli.dll process-once --dry-run

# 감시 모드
LLM_WIKI_ENV_FILE=~/apps/llm-wiki/shared/.env \
LLM_WIKI_ROOT=~/apps/llm-wiki/shared/data \
/Users/boseong/.dotnet/dotnet LlmWiki.Cli.dll watch
```

dotnet 경로(`/Users/boseong/.dotnet/dotnet`)는 환경에 따라 다를 수 있다. 배포 스크립트 로그의 "dotnet found:" 줄에서 확인한다.

### self-contained 배포

```bash
cd ~/apps/llm-wiki/current

LLM_WIKI_ENV_FILE=~/apps/llm-wiki/shared/.env \
LLM_WIKI_ROOT=~/apps/llm-wiki/shared/data \
./LlmWiki.Cli process-once

LLM_WIKI_ENV_FILE=~/apps/llm-wiki/shared/.env \
LLM_WIKI_ROOT=~/apps/llm-wiki/shared/data \
./LlmWiki.Cli watch
```

### shared/.env 형식

```env
GEMINI_API_KEY=your_key_here
GEMINI_MODEL=gemini-2.5-flash
LLM_WIKI_ROOT=~/apps/llm-wiki/shared/data
```

- `GEMINI_MODEL`에 `models/` prefix를 붙이지 않는다 (`models/gemini-2.5-flash` → 내부 정규화됨, 권장하지 않음).
- `gemini-1.5-flash`는 404를 반환한다. `gemini-2.5-flash` 사용.

---

## 8. 데이터 보존 정책

| 항목 | 정책 |
|---|---|
| `shared/data/` | 배포 스크립트가 절대 삭제하지 않음 |
| `shared/.env` | 배포 스크립트가 절대 덮어쓰지 않음 |
| `releases/{timestamp}/` | 배포 시 새로 생성. 기존 releases 유지 |
| `current` 심볼릭 링크 | 배포 시 최신 release로 갱신 |

---

## 9. 롤백 방법

Mac mini에서:

```bash
# 사용 가능한 릴리즈 목록 확인
ls ~/apps/llm-wiki/releases

# 이전 릴리즈로 롤백
ln -sfn ~/apps/llm-wiki/releases/{이전-타임스탬프} ~/apps/llm-wiki/current

# 확인
ls -la ~/apps/llm-wiki/current
```

---

## 10. 보안 주의사항

- **SSH 비밀번호 로그인 비활성화**: `/etc/ssh/sshd_config`에서 `PasswordAuthentication no` 설정
- **SSH 키 기반 로그인 사용**: `~/.ssh/authorized_keys`에 공개키 등록
- **포트포워딩 사용 시**: 가능하면 특정 IP에서만 접근 허용하도록 라우터/방화벽 설정
- **Tailscale 전환 검토**: 포트포워딩 대신 Tailscale을 사용하면 공인 IP 노출 없이 안전하게 접근 가능
- **`GEMINI_API_KEY`**: 절대 저장소에 커밋하지 않음. Mac mini의 `shared/.env`에서만 관리

---

## 11. 향후 작업 (미확정)

### launchd 등록

Mac mini에서 `watch` 명령을 상시 실행하려면 launchd를 등록한다.
현재 미구현. 수동 검증 이후 결정.

plist 파일 예시 위치: `docs/llm-wiki-watch.plist.example`

### ConfigLoader `.env` 경로 파라미터화

현재 ConfigLoader는 현재 작업 디렉터리의 `.env`를 찾는다.
`shared/.env`를 자동으로 로딩하도록 경로 주입 기능 추가를 검토한다.

### 오래된 releases 자동 정리

`releases/` 하위에 오래된 릴리즈가 누적될 수 있다.
일정 개수 이상이면 오래된 것을 삭제하는 cleanup 로직 추가를 검토한다.
