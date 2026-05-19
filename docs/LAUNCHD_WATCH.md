# launchd Watch Agent

Mac mini에서 `watch` 명령을 상시 실행하기 위한 launchd user agent 구성.

---

## 목적

- Mac mini 재시작 후 자동으로 `watch` 가 시작된다.
- 프로세스가 종료되면 launchd가 자동으로 재시작한다 (`KeepAlive`).
- `sudo` 없이 user agent로 실행된다.

---

## 전제 조건

다음이 완료된 상태여야 한다:

- `deploy-to-mac.ps1` 실행 완료
- `/Users/boseong/apps/llm-wiki/current/LlmWiki.Cli.dll` 존재
- `/Users/boseong/.dotnet/dotnet` 실행 가능 (배포 스크립트 Step 4에서 자동 설치)
- `/Users/boseong/apps/llm-wiki/shared/.env` 존재 (없으면 fallback 모드)

---

## 설치

```bash
bash scripts/mac/install-launchd-watch.sh
```

동작:
1. dotnet 실행 파일 탐색 (`~/.dotnet/dotnet` → `/usr/local/share/dotnet/dotnet` → `/opt/homebrew/bin/dotnet`)
2. 필요한 경로 존재 여부 확인
3. `~/apps/llm-wiki/shared/logs/` 생성
4. `~/Library/LaunchAgents/com.llmwiki.watch.plist` 생성
5. `launchctl bootstrap gui/$(id -u)` 로 등록

재배포(`deploy-to-mac.ps1`) 후에는 install 스크립트를 재실행해 plist를 새 경로로 갱신한다.

---

## 상태 확인

```bash
bash scripts/mac/status-launchd-watch.sh
```

출력:
- plist 존재 여부
- `launchctl print` 결과 (PID, 마지막 종료 코드 등)
- stdout 로그 최근 50줄
- stderr 로그 최근 50줄

직접 확인:
```bash
launchctl print gui/$(id -u)/com.llmwiki.watch
```

---

## 제거

```bash
bash scripts/mac/uninstall-launchd-watch.sh
```

제거되는 것:
- launchd job (bootout)
- `~/Library/LaunchAgents/com.llmwiki.watch.plist`

제거되지 않는 것:
- `~/apps/llm-wiki/shared/data/`
- `~/apps/llm-wiki/shared/.env`
- `~/apps/llm-wiki/shared/logs/`

---

## 로그 위치

| 로그 | 경로 |
|---|---|
| stdout | `~/apps/llm-wiki/shared/logs/watch.out.log` |
| stderr | `~/apps/llm-wiki/shared/logs/watch.err.log` |

실시간 확인:
```bash
tail -f ~/apps/llm-wiki/shared/logs/watch.out.log
tail -f ~/apps/llm-wiki/shared/logs/watch.err.log
```

---

## 테스트 방법

설치 후 테스트 파일을 Inbox에 추가한다:

```bash
echo "launchd watch test" \
  > ~/apps/llm-wiki/shared/data/Inbox/raw_clips/launchd-test.txt
```

결과 확인:
```bash
# 노트 생성 확인
find ~/apps/llm-wiki/shared/data/Notes/References -type f | sort

# 원본 파일 이동 확인
find ~/apps/llm-wiki/shared/data/Inbox/processed -type f | sort

# 로그 확인
tail -n 50 ~/apps/llm-wiki/shared/logs/watch.out.log
tail -n 50 ~/apps/llm-wiki/shared/logs/watch.err.log
```

예상 로그 출력:
```text
[Watcher] Detected: .../Inbox/raw_clips/launchd-test.txt
[Inbox] scanned=1 gemini_configured=True dry_run=False
[Gemini] calling for launchd-test.txt
[OK] note=.../Notes/References/YYYY-MM-DD-launchd-watch-test.md
     moved_to=.../Inbox/processed/YYYY-MM-DD/launchd-test.txt
[Summary] processed=1 failed=0 gemini_calls=1 fallbacks=0
```

---

## 검증 절차

1. `install-launchd-watch.sh` 실행
2. `status-launchd-watch.sh` 실행 — PID 확인
3. 테스트 파일 생성
4. `Notes/References` 노트 생성 확인
5. `Inbox/processed` 파일 이동 확인
6. 로그 확인
7. 재부팅 후 자동 실행 확인 (별도 검증)

---

## 문제 해결

### job이 시작되지 않음

```bash
launchctl print gui/$(id -u)/com.llmwiki.watch
```

`last exit code` 확인. 0이 아니면 stderr 로그 확인:
```bash
cat ~/apps/llm-wiki/shared/logs/watch.err.log
```

### dotnet not found

`shared/.env`에 `DOTNET_ROOT`가 설정되어 있어도 launchd의 `EnvironmentVariables`가 우선한다. install 스크립트가 탐지한 경로가 plist에 기록된다.

확인:
```bash
grep -A2 "DOTNET_ROOT" ~/Library/LaunchAgents/com.llmwiki.watch.plist
```

### plist 재생성 필요 시

```bash
bash scripts/mac/uninstall-launchd-watch.sh
bash scripts/mac/install-launchd-watch.sh
```

### 구형 macOS fallback (launchctl bootstrap 미지원 시)

```bash
launchctl load ~/Library/LaunchAgents/com.llmwiki.watch.plist
launchctl unload ~/Library/LaunchAgents/com.llmwiki.watch.plist
```

---

## launchd 구성 요약

| 항목 | 값 |
|---|---|
| Label | `com.llmwiki.watch` |
| plist 경로 | `~/Library/LaunchAgents/com.llmwiki.watch.plist` |
| 실행 파일 | `~/.dotnet/dotnet LlmWiki.Cli.dll watch` |
| 작업 디렉터리 | `~/apps/llm-wiki/current` |
| KeepAlive | true |
| RunAtLoad | true |
| stdout | `~/apps/llm-wiki/shared/logs/watch.out.log` |
| stderr | `~/apps/llm-wiki/shared/logs/watch.err.log` |
