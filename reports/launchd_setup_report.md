# launchd Setup Report

작성일: 2026-05-19

## 1. 수행 결과

- **완료**: plist 템플릿, install/uninstall/status 스크립트, 문서, bash -n 문법 검사, dotnet build·test 통과
- **부분 완료**: 없음
- **미완료**: Mac mini 실환경 설치 검증 (사용자가 실행 필요)

## 2. 생성/수정 파일

| 파일 | 변경 내용 |
|---|---|
| [`scripts/mac/com.llmwiki.watch.plist.template`](../scripts/mac/com.llmwiki.watch.plist.template) | 신규 — plist 구조 참조용 템플릿 |
| [`scripts/mac/install-launchd-watch.sh`](../scripts/mac/install-launchd-watch.sh) | 신규 — dotnet 탐색, 경로 확인, plist 생성, launchctl bootstrap |
| [`scripts/mac/uninstall-launchd-watch.sh`](../scripts/mac/uninstall-launchd-watch.sh) | 신규 — launchctl bootout, plist 삭제 |
| [`scripts/mac/status-launchd-watch.sh`](../scripts/mac/status-launchd-watch.sh) | 신규 — launchctl print + 로그 tail |
| [`docs/LAUNCHD_WATCH.md`](LAUNCHD_WATCH.md) | 신규 — 설치/상태/제거/테스트/문제해결 가이드 |

## 3. launchd 구성

| 항목 | 값 |
|---|---|
| Label | `com.llmwiki.watch` |
| plist path | `~/Library/LaunchAgents/com.llmwiki.watch.plist` |
| working directory | `~/apps/llm-wiki/current` |
| stdout log | `~/apps/llm-wiki/shared/logs/watch.out.log` |
| stderr log | `~/apps/llm-wiki/shared/logs/watch.err.log` |
| env file | `~/apps/llm-wiki/shared/.env` (LLM_WIKI_ENV_FILE) |
| data root | `~/apps/llm-wiki/shared/data` (LLM_WIKI_ROOT) |
| KeepAlive | true |
| RunAtLoad | true |

## 4. 실행 명령

```bash
# 설치
bash scripts/mac/install-launchd-watch.sh

# 상태 확인
bash scripts/mac/status-launchd-watch.sh

# 제거
bash scripts/mac/uninstall-launchd-watch.sh

# 설치 후 테스트
echo "launchd watch test" \
  > ~/apps/llm-wiki/shared/data/Inbox/raw_clips/launchd-test.txt
```

## 5. 테스트 결과

```text
# bash -n 문법 검사
install-launchd-watch.sh:  OK
uninstall-launchd-watch.sh: OK
status-launchd-watch.sh:   OK

# dotnet build / test
빌드: 경고 0개 오류 0개
테스트: 66/66 통과
```

## 6. 검증 필요

| 항목 | 상태 | 비고 |
|---|---|---|
| Mac mini 설치 | 미검증 | `install-launchd-watch.sh` 실행 필요 |
| launchd 상태 확인 | 미검증 | `status-launchd-watch.sh` 실행 필요 |
| 테스트 파일 처리 | 미검증 | Inbox 파일 투입 후 Notes/processed 확인 필요 |
| 재부팅 후 자동 실행 | 미검증 | 추후 별도 검증 |

## 7. 리스크 / 주의점

- install 스크립트는 dotnet을 `$HOME/.dotnet/dotnet` 우선 탐색한다. 다른 경로에 설치된 경우 올바르게 탐색되는지 확인 필요. (추정: `/usr/local/share/dotnet/dotnet`, `/opt/homebrew/bin/dotnet` 순으로 fallback)
- 재배포 (`deploy-to-mac.ps1`) 후 `current` symlink가 새 release를 가리키면 plist의 `WorkingDirectory` 는 symlink를 따라가므로 재설치 불필요. (추정: macOS에서 launchd가 symlink resolved path가 아닌 symlink 자체 경로를 사용)
- plist에 `shared/.env` 경로가 하드코딩되므로 `.env` 파일 경로 변경 시 plist 재생성 필요.
- 로그 파일은 자동 rotation 없음. 장기 운영 시 로그 크기 관리 필요.

## 8. 다음 작업 후보

1. Mac mini에서 install-launchd-watch.sh 실행 및 검증
2. 재부팅 후 자동 실행 검증
3. 로그 rotation 방안 검토 (장기 운영 시)
