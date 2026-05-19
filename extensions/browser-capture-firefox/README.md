# LLM Wiki Capture — Firefox Extension

Firefox용 LLM Wiki Capture Extension. `about:debugging`으로 로컬 로드 가능.

## 상태

로컬 개발용. Mozilla Add-ons Store 미배포.

## 설치 (Firefox 임시 로드)

1. Firefox → `about:debugging` 열기
2. **This Firefox** 선택
3. **Load Temporary Add-on...** 클릭
4. `extensions/browser-capture-firefox/manifest.json` 선택

임시 로드는 Firefox 재시작 시 해제된다. 영구 설치는 AMO 서명이 필요하다.

## 초기 설정

Extensions toolbar icon 또는 `about:addons` → LLM Wiki Capture → Options

| 항목 | 설명 |
|---|---|
| Capture API URL | Mac mini 접근 주소 (아래 주의사항 참고) |
| Capture API Token | `.env`의 `CAPTURE_API_TOKEN` 값 |

### Capture API URL 주의사항

Capture API가 Mac mini에서 `http://127.0.0.1:5055`로 바인딩된 경우,
**Windows Firefox에서는 직접 접근할 수 없다.** (`127.0.0.1`은 Windows 자신을 가리킨다.)

접근 방법:

| 방법 | URL 예시 |
|---|---|
| LAN IP | `http://192.168.1.10:5055` |
| Tailscale | `http://100.x.x.x:5055` |
| 포트포워딩 | `http://<공인IP>:<포트>` (Bearer token 필수) |
| SSH tunnel | `ssh -L 5055:127.0.0.1:5055 user@mac` 후 `http://127.0.0.1:5055` |

LAN IP 또는 Tailscale IP 이외의 주소를 사용할 경우 `manifest.json`의 `permissions`에
해당 주소 패턴을 추가해야 한다.

```json
"permissions": [
  ...
  "http://192.168.1.10:5055/*"
]
```

수정 후 `about:debugging`에서 Reload 필요.

## 기능

| 버튼 / 메뉴 | 동작 | API |
|---|---|---|
| Save Page | 현재 URL + 제목 저장 | `POST /api/capture/link` |
| Save Selection | 선택 텍스트 + URL 저장 | `POST /api/capture/clip` |
| Save Note | 팝업 입력 텍스트 저장 | `POST /api/capture/note` |
| 우클릭 → Save page to LLM Wiki | Save Page와 동일 | `POST /api/capture/link` |
| 우클릭 → Save selection to LLM Wiki | Save Selection과 동일 | `POST /api/capture/clip` |

## 파일 구성

```
browser-capture-firefox/
  manifest.json          MV2, Firefox gecko ID 포함
  shared/
    browserCompat.js     browser.*/chrome.* 정규화 shim
    captureClient.js     Capture API fetch 공통 함수
  src/
    background.js        context menu, 우클릭 capture
    popup.html / popup.js
    options.html / options.js
    content.js           선택 텍스트 추출
  icons/                 아이콘 파일 위치 (별도 제작 필요)
```

## Chrome extension과의 차이

| 항목 | Chrome (browser-capture/) | Firefox (browser-capture-firefox/) |
|---|---|---|
| Manifest 버전 | MV3 | MV2 |
| Background | service_worker | scripts 배열 |
| API 접두사 | chrome.* | browser.* (ext shim 경유) |
| Host permissions | manifest host_permissions | permissions 배열에 포함 |
| 공통 모듈 | 없음 (인라인) | shared/ 서브디렉터리 |

## 패키징

```powershell
.\scripts\extensions\package-firefox-extension.ps1
# 출력: artifacts/extensions/llm-wiki-capture-firefox.zip
```
