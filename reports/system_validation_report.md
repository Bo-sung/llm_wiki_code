# System Validation Report

기준일: 2026-05-19
상태: MVP 운영 가능

---

## 1. 검증 요약

| 영역 | 상태 | 비고 |
|---|---|---|
| Windows build/test | 통과 | dotnet build 오류 0 / test 66/66 |
| Windows → Mac 배포 | 통과 | deploy-to-mac.ps1 전 단계 통과 |
| Mac mini dotnet 실행 | 통과 | dotnet-install.sh로 자동 설치 후 확인 |
| process-once dry-run | 통과 | 파일 변화 없음 확인 |
| fallback note 생성 | 통과 | Gemini 미설정 상태 |
| Gemini 실제 호출 | 통과 | gemini-2.5-flash |
| watch 수동 실행 | 통과 | macOS FileSystemWatcher 정상 동작 |
| watcher 안정화 | 통과 | debounce 1500ms, file-ready check |
| launchd 설치 | 통과 | com.llmwiki.watch user agent |
| launchd end-to-end | 통과 | 파일 감지 → Gemini 호출 → 노트 생성 → processed 이동 |
| public-vault 초기화 | 통과 | git init, origin 설정 |
| public GitHub push | 통과 | git@github.com:Bo-sung/my_ldea_wiki.git |
| export-public-note.sh | 통과 | 금지 경로 거부, 허용 경로 복사 확인 |

---

## 2. 운영 경로

| 항목 | 경로 |
|---|---|
| app base | `/Users/boseong/apps/llm-wiki` |
| current | `/Users/boseong/apps/llm-wiki/current` (symlink → releases/{timestamp}/osx-arm64) |
| shared data | `/Users/boseong/apps/llm-wiki/shared/data` |
| shared env | `/Users/boseong/apps/llm-wiki/shared/.env` |
| public vault | `/Users/boseong/apps/llm-wiki/public-vault` |
| logs | `/Users/boseong/apps/llm-wiki/shared/logs` |
| launchd plist | `~/Library/LaunchAgents/com.llmwiki.watch.plist` |
| public vault config | `/Users/boseong/apps/llm-wiki/shared/config/public-vault.env` |
| GitHub public repo | `git@github.com:Bo-sung/my_ldea_wiki.git` |

---

## 3. 주요 명령

### 배포 (Windows)

```powershell
.\scripts\deploy\deploy-to-mac.ps1 `
  -RemoteHost "8eh1ndy0u.iptime.org" `
  -User "boseong" `
  -Port 2222 `
  -Runtime "osx-arm64" `
  -RemoteBase "/Users/boseong/apps/llm-wiki"
```

### launchd 상태 확인 (Mac mini)

```bash
bash /Users/boseong/apps/llm-wiki/current/scripts/mac/status-launchd-watch.sh
# 또는 직접 확인
launchctl print gui/$(id -u)/com.llmwiki.watch
```

### 수동 process-once (Mac mini)

```bash
cd /Users/boseong/apps/llm-wiki/current
LLM_WIKI_ENV_FILE=/Users/boseong/apps/llm-wiki/shared/.env \
LLM_WIKI_ROOT=/Users/boseong/apps/llm-wiki/shared/data \
/Users/boseong/.dotnet/dotnet LlmWiki.Cli.dll process-once
```

### 수동 watch (Mac mini)

```bash
cd /Users/boseong/apps/llm-wiki/current
LLM_WIKI_ENV_FILE=/Users/boseong/apps/llm-wiki/shared/.env \
LLM_WIKI_ROOT=/Users/boseong/apps/llm-wiki/shared/data \
/Users/boseong/.dotnet/dotnet LlmWiki.Cli.dll watch
```

### 공개 export (Mac mini)

```bash
bash /Users/boseong/apps/llm-wiki/current/scripts/mac/export-public-note.sh \
  Notes/References/YYYY-MM-DD-example.md
```

### public-vault push (Mac mini)

```bash
cd /Users/boseong/apps/llm-wiki/public-vault
git add Notes/References/YYYY-MM-DD-example.md
git commit -m "Publish note: YYYY-MM-DD-example"
git push
```

---

## 4. 검증된 데이터 흐름

```text
Inbox 입력 (.txt / .md / .url)
  → launchd watch 파일 감지 (debounce 1500ms)
  → file-ready 확인 (크기 안정화 2회)
  → Gemini API 호출 (gemini-2.5-flash)
  → Notes/References/YYYY-MM-DD-slug.md 생성
  → 원본 Inbox/processed/YYYY-MM-DD/ 이동
  ↓ (사람 검수 후)
  export-public-note.sh 실행
  → public-vault/Notes/References/ 복사
  → git add / commit / push (수동)
  → GitHub public repo 반영
```

---

## 5. 현재 보류 항목

| 항목 | 상태 | 이유 |
|---|---|---|
| 자동 public push | 보류 | 공개 전 사람 검수 필요 |
| 데이터 전체 백업 | 보류 | public-vault와 운영 vault 분리 유지 |
| 카테고리 자동 분류 | 보류 | 노트 품질 안정화 후 검토 |
| Context Caching | 보류 | 비용/복잡도 |
| 모바일 앱 | 논의 중 | Obsidian + public-vault 대체 가능성 검토 |
| 브라우저 익스텐션 | 보류 | 수집 UX 개선 단계에서 검토 |
| launchd 재부팅 자동 실행 | 미검증 | 별도 검증 필요 |
| Obsidian 모바일 동기화 | 미검증 | public-vault pull 방식 검토 중 |

---

## 6. 확인된 리스크

- Gemini API 키 노출 이력 있음 — 키 재발급 권장.
- 공개 repo에 올리는 노트는 수동 검수 필요. 민감정보 자동 필터 미구현.
- launchd 로그(`watch.out.log`, `watch.err.log`) rotation 없음. 장기 운영 시 파일 크기 관리 필요.
- Obsidian 모바일 동기화 미검증.
- public-vault는 운영용 vault 전체 백업이 아님.
- Mac mini 재부팅 후 launchd 자동 실행 검증 미수행. (추정: KeepAlive + RunAtLoad 설정으로 동작할 것)

---

## 7. 다음 작업 후보

1. Gemini API 키 재발급 및 `shared/.env` 갱신
2. launchd 재부팅 후 자동 실행 검증
3. Obsidian 모바일 → public-vault pull 검증
