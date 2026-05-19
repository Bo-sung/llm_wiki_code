# Public Vault Deploy Config Report

작성일: 2026-05-19

## 1. 수행 결과

- **완료**: 배포 파라미터 6개 추가, `public-vault.env` 원격 생성 로직, `-SetupPublicVault` 초기화 로직, 문서 3종 신규, `DECISIONS.md`·`DEPLOYMENT_WINDOWS_TO_MAC.md` 갱신, dotnet build·test 통과
- **부분 완료**: 없음
- **미완료**: Mac mini 실환경 검증 (사용자가 실행 필요)

## 2. 생성/수정 파일

| 파일 | 변경 내용 |
|---|---|
| [`scripts/deploy/deploy-to-mac.ps1`](../scripts/deploy/deploy-to-mac.ps1) | 파라미터 6개 추가, optional public vault 블록 추가 |
| [`docs/PUBLIC_VAULT_POLICY.md`](PUBLIC_VAULT_POLICY.md) | 신규 — vault 분리 구조, Git 정책, 설정 파일 형식 |
| [`docs/OBSIDIAN_MOBILE_GIT_SYNC.md`](OBSIDIAN_MOBILE_GIT_SYNC.md) | 신규 — 모바일 동기화 방향 및 미결정 사항 |
| [`docs/DEPLOYMENT_WINDOWS_TO_MAC.md`](DEPLOYMENT_WINDOWS_TO_MAC.md) | public vault 파라미터 표 및 사용 예 추가 |
| [`docs/DECISIONS.md`](DECISIONS.md) | Public Vault 분리 결정 기록 추가, 미결정 사항 갱신 |

## 3. 추가된 배포 파라미터

| 파라미터 | 목적 |
|---|---|
| `-PublicVaultRepoUrl` | GitHub remote URL 지정 (git@github.com:...) |
| `-PublicVaultBranch` | 기본 브랜치 (기본값: main) |
| `-PublicVaultPath` | Mac mini 내 public vault 경로 (기본값: $RemoteBase/public-vault) |
| `-GitAuthorName` | vault repo용 git user.name |
| `-GitAuthorEmail` | vault repo용 git user.email |
| `-SetupPublicVault` | 스위치 — vault 초기화 수행 여부 |

## 4. Git 정보 전달 정책

| 항목 | 정책 |
|---|---|
| repo URL | 배포 시 전달 가능 |
| branch | 배포 시 전달 가능 |
| author name/email | 배포 시 전달 가능 |
| GitHub token | 전달 금지 |
| SSH private key | 전달 금지 |
| 자동 push | 초기 제외 |

## 5. SetupPublicVault 동작

`-SetupPublicVault` 지정 시 수행:
1. `public-vault.env` → `~/apps/llm-wiki/shared/config/public-vault.env` 기록 (기존 파일 백업 후 덮어쓰기)
2. `PublicVaultPath` 디렉터리 생성
3. `git init -b $branch` (`.git` 없을 때만)
4. `git config user.name/email` (값 있을 때만)
5. `origin` remote 설정 (URL 있을 때만; 기존 remote는 `set-url`로 갱신)
6. `.gitignore` 생성 (없을 때만)
7. `README.md` 생성 (없을 때만)

수행하지 않는 것: git push, git pull, 자동 commit, `shared/data` 복사, 인증 설정

## 6. 테스트 결과

```text
Syntax: OK (PSParser 검증)
New vault params: PublicVaultRepoUrl, PublicVaultBranch, PublicVaultPath,
                  GitAuthorName, GitAuthorEmail, SetupPublicVault
dotnet build: 경고 0개 오류 0개
dotnet test:  66/66 통과
```

## 7. 다음 작업 후보

1. Mac mini에서 `-SetupPublicVault` 실행 및 `public-vault.env` 생성 확인
2. Mac mini SSH key 생성 및 GitHub 등록 후 첫 push 수행
3. public vault 자동 export 기준 결정 (frontmatter `status: public` 필터 방식 검토)
