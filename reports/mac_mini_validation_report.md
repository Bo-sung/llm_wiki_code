# Mac mini Validation Report

최초 작성: 2026-05-19 / watch 갱신: 2026-05-19

## 1. 검증 결과

| 항목 | 결과 | 비고 |
|---|---|---|
| 배포 구조 | 통과 | current symlink, shared/data, releases/ 확인 |
| dotnet 실행 | 통과 | Mac mini에 미설치 → SSH에서 dotnet-install.sh로 자동 설치 |
| dry-run | 통과 | `process-once --dry-run` 실행 후 파일 변화 없음 확인 |
| fallback 실제 처리 | 통과 | GEMINI_MODEL 미설정 상태에서 fallback note 생성 및 processed/ 이동 확인 |
| Gemini 실제 호출 | 통과 | `gemini-2.5-flash` 모델로 정상 호출, 노트 생성 확인 |
| watch 수동 검증 | 통과 | macOS FileSystemWatcher 감지, Gemini 호출, processed 이동 확인 |

## 2. 확인된 문제 및 처리

| 문제 | 원인 | 처리 |
|---|---|---|
| `zsh: command not found: dotnet` | Mac mini에 .NET 미설치 | 배포 스크립트 Step 4에서 dotnet-install.sh 자동 실행 |
| `dotnet --version` 실패 | runtime만 설치 시 SDK 없음 | 검증 명령을 `--list-runtimes`로 교체 |
| shared/.env 자동 로딩 실패 | ConfigLoader가 작업 디렉터리 .env만 탐색 | `LLM_WIKI_ENV_FILE` 환경변수 지원 추가 |
| `gemini-1.5-flash` 404 Not Found | 모델 지원 중단 또는 접근 제한 | `gemini-2.5-flash` 사용으로 전환 |
| watch 동일 파일 이벤트 2회 감지 | macOS FSEvents 특성 | debounce 1500ms 추가로 중복 억제 |

## 3. 현재 운영 환경

| 항목 | 값 |
|---|---|
| App path | `/Users/boseong/apps/llm-wiki/current` |
| Shared data | `/Users/boseong/apps/llm-wiki/shared/data` |
| Shared env | `/Users/boseong/apps/llm-wiki/shared/.env` |
| dotnet 경로 | `/Users/boseong/.dotnet/dotnet` (dotnet-install.sh로 설치) |
| .NET 버전 | 8.0 runtime (framework-dependent 배포) |
| Gemini model | `gemini-2.5-flash` |

## 4. 실행 명령 (검증 완료)

```bash
cd /Users/boseong/apps/llm-wiki/current

# 단일 처리
LLM_WIKI_ENV_FILE=/Users/boseong/apps/llm-wiki/shared/.env \
LLM_WIKI_ROOT=/Users/boseong/apps/llm-wiki/shared/data \
/Users/boseong/.dotnet/dotnet LlmWiki.Cli.dll process-once

# dry-run
LLM_WIKI_ENV_FILE=/Users/boseong/apps/llm-wiki/shared/.env \
LLM_WIKI_ROOT=/Users/boseong/apps/llm-wiki/shared/data \
/Users/boseong/.dotnet/dotnet LlmWiki.Cli.dll process-once --dry-run

# watch
LLM_WIKI_ENV_FILE=/Users/boseong/apps/llm-wiki/shared/.env \
LLM_WIKI_ROOT=/Users/boseong/apps/llm-wiki/shared/data \
/Users/boseong/.dotnet/dotnet LlmWiki.Cli.dll watch
```

## 5. Gemini 호출 검증 로그

```text
[Inbox] scanned=1 gemini_configured=True dry_run=False
[Gemini] calling for gemini-25-test.txt
[OK] note=.../Notes/References/2026-05-19-gemini-25-flash.md
     moved_to=.../Inbox/processed/2026-05-19/gemini-25-test.txt
[Summary] processed=1 failed=0 gemini_calls=1 fallbacks=0
```

## 6. watch 검증 로그

```text
[Watcher] Watching .../Inbox (debounce=1500ms) ... (Ctrl+C to stop)
[Watcher] Detected: .../Inbox/raw_clips/watch-test.txt
[Inbox] scanned=1 gemini_configured=True dry_run=False
[Gemini] calling for watch-test.txt
[OK] note=.../Notes/References/2026-05-19-watch-mode-test.md
     moved_to=.../Inbox/processed/2026-05-19/watch-test.txt
[Summary] processed=1 failed=0 gemini_calls=1 fallbacks=0
[Watcher] Debounced: watch-test.txt   ← 2차 이벤트 억제 확인
```

## 7. 특이사항

macOS에서 동일 파일 이벤트가 2회 감지됐다. FSEvents 특성으로 Created 이후 Changed 이벤트가 추가 발생한다.
watcher debounce(1500ms)를 추가해 억제했다. 2차 이벤트 감지 시 `[Watcher] Debounced:` 로그가 출력되고 처리를 건너뛴다.

## 8. 다음 단계

1. launchd 등록 검토 (watch 상시 실행, watch 검증 완료로 전제 충족)
2. 데이터 repo 생성 시점 검토 (GitHub Private Repo 백업 정책 확정 후)
3. 카테고리 자동 분류 검토 (Gemini 정리 품질 확인 중)

## 9. 미결정 사항

| 항목 | 현재 상태 |
|---|---|
| watch launchd 등록 | **검토 가능** — watch 검증 완료 |
| GitHub 백업 | 미결정. 데이터 안정화 후 결정 |
| 카테고리 자동 분류 | 미결정. Gemini 정리 품질 확인 중 |
