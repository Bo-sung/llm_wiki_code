# Public Vault Policy

## Vault 분리 구조

| Vault | 경로 | 용도 |
|---|---|---|
| 운영용 (private) | `~/apps/llm-wiki/shared/data` | 전체 노트, Inbox, Sources 포함. Git 비공개. |
| 공개용 (public) | `~/apps/llm-wiki/public-vault` | 선택적 공개 노트. GitHub Public Repo. |

운영용 `shared/data` 전체를 public repo로 만들지 않는다.
`export-public-note.sh`를 사용해 검수한 노트만 `public-vault`로 export한다.

---

## 공개용 vault Git 정책

| 항목 | 정책 |
|---|---|
| repo URL | 배포 시 `-PublicVaultRepoUrl` 파라미터로 전달 가능 |
| branch | 배포 시 `-PublicVaultBranch` 파라미터로 전달 가능 (기본: main) |
| author name/email | 배포 시 `-GitAuthorName`, `-GitAuthorEmail`으로 전달 가능 |
| GitHub token | 전달 금지 — Mac mini에서 직접 설정 |
| SSH private key | 전달 금지 — Mac mini에서 직접 생성 및 등록 |
| 자동 push | 초기 제외 |
| 자동 export | 초기 제외 — `export-public-note.sh` + 수동 commit/push |
| 운영용 data 복사 | 금지 |

---

## 배포 스크립트 연동

`deploy-to-mac.ps1`의 `-SetupPublicVault` 플래그로 vault 초기화 수행:

```powershell
.\scripts\deploy\deploy-to-mac.ps1 `
  -RemoteHost "..." -User "..." `
  -PublicVaultRepoUrl "git@github.com:user/llm-wiki-vault-public.git" `
  -PublicVaultBranch "main" `
  -GitAuthorName "boseong" `
  -GitAuthorEmail "public@example.com" `
  -SetupPublicVault
```

수행 내용:
1. `~/apps/llm-wiki/shared/config/public-vault.env` 생성/갱신
2. `~/apps/llm-wiki/public-vault/` 디렉터리 생성
3. `git init -b main`
4. `origin` remote 설정
5. `.gitignore` 생성 (없을 때만)
6. `README.md` 생성 (없을 때만)

수행하지 않는 것: git push, git pull, 자동 commit, `shared/data` 복사.

---

## Mac mini Git 인증 설정

배포 스크립트 실행 후, Mac mini에서 직접 수행:

```bash
# SSH key 생성 (이미 있으면 생략)
ssh-keygen -t ed25519 -C "public@example.com"

# GitHub에 public key 등록 후
git -C ~/apps/llm-wiki/public-vault push -u origin main
```

---

## public-vault .gitignore

배포 시 생성되는 `.gitignore`:

```gitignore
Inbox/
Sources/
System/private/
.env
*.log
.DS_Store
.obsidian/workspace*
.obsidian/cache/
.trash/
```

---

## 설정 파일

`~/apps/llm-wiki/shared/config/public-vault.env`:

```env
PUBLIC_VAULT_REPO_URL=git@github.com:<user>/llm-wiki-vault-public.git
PUBLIC_VAULT_BRANCH=main
PUBLIC_VAULT_PATH=/Users/boseong/apps/llm-wiki/public-vault
GIT_AUTHOR_NAME=boseong
GIT_AUTHOR_EMAIL=<public-email>
```

이 파일은 비밀 정보를 포함하지 않는다. GitHub token, SSH private key, password를 기록하지 않는다.
