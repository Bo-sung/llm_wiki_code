# Firefox Extension Local Test

## 목적

Firefox에서 LLM Wiki Capture Extension을 임시 로드해 Capture API와 연동을 검증한다.

---

## 전제 조건

- Capture API가 Mac mini에서 실행 중 (launchd `com.llmwiki.captureapi` 등록 완료)
- `CAPTURE_API_TOKEN` 설정 완료
- Firefox 91+ 설치 완료
- Mac mini 접근 주소 확인 (아래 참고)

---

## Capture API URL: Windows Firefox → Mac mini

Mac mini의 Capture API가 `http://127.0.0.1:5055`로 바인딩된 경우,
Windows Firefox에서 `http://127.0.0.1:5055`를 입력하면 **Windows 자신**에 접속하게 된다.

Mac mini에 접근하려면 다음 중 하나를 사용한다.

| 방법 | URL 예시 | 비고 |
|---|---|---|
| LAN IP | `http://192.168.1.10:5055` | 같은 네트워크 필요 |
| Tailscale | `http://100.x.x.x:5055` | Tailscale 설치 필요 |
| 포트포워딩 | `http://<공인IP>:<외부포트>` | Bearer token 필수, 신중히 사용 |
| SSH tunnel | 아래 참고 | 개발/검증용 |

### SSH tunnel 방법

Windows에서:

```powershell
ssh -L 5055:127.0.0.1:5055 boseong@<mac-mini-ip>
```

유지 중에는 Firefox에서 `http://127.0.0.1:5055` 사용 가능.

---

## host_permissions 조정

기본 manifest의 `permissions`에는 `http://127.0.0.1:5055/*`와 `http://localhost:5055/*`만 포함한다.

다른 주소를 사용하는 경우 `manifest.json` 수정 후 extension을 재로드한다.

```json
"permissions": [
  "activeTab", "contextMenus", "storage", "tabs",
  "http://127.0.0.1:5055/*",
  "http://localhost:5055/*",
  "http://192.168.1.10:5055/*"
]
```

---

## 임시 로드 절차

1. Firefox → `about:debugging` 열기
2. **This Firefox** 선택
3. **Load Temporary Add-on...** 클릭
4. `extensions/browser-capture-firefox/manifest.json` 선택

임시 로드는 Firefox 재시작 시 해제된다.

---

## Options 설정

Extension 아이콘 클릭 → Options 링크, 또는 `about:addons` → LLM Wiki Capture → Options

| 항목 | 값 |
|---|---|
| Capture API URL | Mac mini 접근 주소 (위 표 참고) |
| Capture API Token | `.env`의 `CAPTURE_API_TOKEN` 값 |

저장 버튼 클릭 후 "저장됨" 표시 확인.

---

## 기능 테스트

```text
1. 임의 페이지 열기 (예: https://example.com)
2. 팝업 → Save Page
3. Mac mini Inbox/links/ 에 .json 파일 생성 확인
4. 텍스트 선택 후 팝업 → Save Selection
5. Mac mini Inbox/raw_clips/ 에 .json 파일 생성 확인
6. 팝업 텍스트 입력 후 Save Note
7. Mac mini Inbox/raw_clips/ 에 .json 파일 생성 확인
8. 우클릭 메뉴 Save page / Save selection 동작 확인
```

---

## Mac mini 확인 명령

```bash
# Inbox 파일 확인
find /Users/boseong/apps/llm-wiki/shared/data/Inbox -type f | sort

# watcher가 처리한 Notes 확인
find /Users/boseong/apps/llm-wiki/shared/data/Notes/References -type f | sort

# Capture API 로그
tail -n 100 /Users/boseong/apps/llm-wiki/shared/logs/captureapi.out.log

# watcher 로그
tail -n 100 /Users/boseong/apps/llm-wiki/shared/logs/watch.out.log
```

---

## 트러블슈팅: NetworkError 발생 시

### 원인 1 — 127.0.0.1은 Windows 자기 자신

Options에 `http://127.0.0.1:5055`를 입력하면 Windows 자신에 접속한다.
Mac mini의 Capture API가 아니다. LAN IP 또는 Tailscale IP를 사용한다.

### 원인 2 — manifest permissions에 접근 주소가 없음

Firefox extension이 fetch를 보내는 URL은 manifest의 `permissions`에 등록되어 있어야 한다.
등록되지 않은 URL로 fetch하면 NetworkError가 발생한다.

현재 manifest에는 개발 편의를 위해 `"<all_urls>"`가 포함되어 있어 모든 URL에 접근 가능하다.

```json
"permissions": [
  ...,
  "<all_urls>"
]
```

**배포 전 최소 권한으로 변경**: 실제 사용하는 Capture API 주소 패턴만 남긴다.

```json
"permissions": [
  "activeTab", "contextMenus", "storage", "tabs",
  "http://192.168.1.10:5055/*"
]
```

변경 후 `about:debugging`에서 **Reload** 클릭.

### 원인 3 — CORS 미처리 (OPTIONS 405)

Capture API 로그에 다음이 보이면 CORS preflight가 처리되지 않은 것이다.

```text
OPTIONS /api/capture/link -> 405 Method Not Supported
```

Firefox와 Chrome Extension은 `Authorization` + `Content-Type` 헤더를 포함하므로
모든 POST 전에 OPTIONS preflight를 먼저 보낸다.

최신 Capture API는 CORS를 항상 활성화한다 (`CAPTURE_API_CORS_MODE` 설정 불필요).
OPTIONS 405가 발생하면 **이전 버전**이 배포된 것이므로 재배포 후 재시작한다.

```bash
launchctl unload ~/Library/LaunchAgents/com.llmwiki.captureapi.plist
launchctl load   ~/Library/LaunchAgents/com.llmwiki.captureapi.plist
```

### 원인 4 — Capture API가 실행 중이지 않음

```bash
# Mac mini에서
launchctl list com.llmwiki.captureapi
curl http://127.0.0.1:5055/health
```

PID가 없거나 curl이 실패하면 Capture API가 중지된 것이다.

```bash
launchctl load ~/Library/LaunchAgents/com.llmwiki.captureapi.plist
```

---

## CORS 동작 확인 (curl)

Mac mini에서 OPTIONS preflight 응답을 직접 확인할 수 있다.

```bash
curl -v -X OPTIONS http://127.0.0.1:5055/api/capture/link \
  -H "Origin: http://localhost:1234" \
  -H "Access-Control-Request-Method: POST" \
  -H "Access-Control-Request-Headers: authorization,content-type"
```

`Access-Control-Allow-Origin: *` 헤더가 응답에 포함되면 CORS가 정상 작동하는 것이다.

---

## curl로 먼저 확인

Firefox에서 동작하지 않으면 먼저 curl로 Capture API를 직접 검증한다.

```bash
# Mac mini 내부에서
curl http://127.0.0.1:5055/health

# Windows에서 (LAN IP 예시)
curl http://192.168.1.10:5055/health
```

---

## 주의

- 토큰을 문서, 로그, git commit에 기록하지 않는다.
- 공개 인터넷 포트포워딩 시 Bearer token은 필수다.
- 임시 로드 extension은 Firefox 재시작 시 사라진다.
