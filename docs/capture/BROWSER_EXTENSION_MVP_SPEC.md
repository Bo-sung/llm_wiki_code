# Browser Extension MVP Spec

## 범위

Chrome + Firefox용 Capture Extension. 로컬 개발 및 개인 사용 전용.
스토어 배포 없음. Firefox가 주 검증 대상.

---

## Extension 목록

| Extension | 위치 | Manifest | 상태 |
|---|---|---|---|
| Chrome | `extensions/browser-capture/` | MV3 | scaffold |
| Firefox | `extensions/browser-capture-firefox/` | MV2 | scaffold |

---

## 파일 구성

### Chrome (browser-capture/)

```
browser-capture/
  manifest.json        — Chrome Manifest V3
  src/
    background.js      — service_worker, chrome.* API
    popup.html / popup.js
    options.html / options.js
    content.js
  icons/
```

### Firefox (browser-capture-firefox/)

```
browser-capture-firefox/
  manifest.json        — Firefox MV2, gecko ID 포함
  shared/
    browserCompat.js   — browser.*/chrome.* 정규화 shim (ext)
    captureClient.js   — postCapture() 공통 함수
  src/
    background.js      — background scripts, ext.* API
    popup.html / popup.js
    options.html / options.js
    content.js
  icons/
  README.md
```

---

## Chrome / Firefox 비교

| 항목 | Chrome | Firefox |
|---|---|---|
| Manifest 버전 | MV3 | MV2 |
| Background | `service_worker` | `scripts` 배열 |
| API 접두사 | `chrome.*` | `browser.*` (`ext` shim 경유) |
| Host permissions | 별도 `host_permissions` 필드 | `permissions` 배열에 포함 |
| 공통 모듈 | 없음 (인라인) | `shared/` 서브디렉터리 |
| source 값 | `"chrome-extension"` | `"firefox-extension"` |

---

## 권한

### Chrome

| 권한 | 용도 |
|---|---|
| `activeTab` | 현재 탭 URL/제목 접근 |
| `contextMenus` | 우클릭 메뉴 등록 |
| `storage` | Options 설정 저장 |
| `scripting` | content script 실행 |
| `host_permissions: http://127.0.0.1:5055/*` | Capture API 접근 |

### Firefox

| 권한 | 용도 |
|---|---|
| `activeTab` | 현재 탭 접근 |
| `contextMenus` | 우클릭 메뉴 |
| `storage` | Options 설정 저장 |
| `tabs` | 탭 정보 및 메시지 |
| `http://127.0.0.1:5055/*` | 로컬 Capture API 접근 |
| `http://localhost:5055/*` | 로컬 Capture API 접근 |

LAN/Tailscale IP 사용 시 해당 주소 패턴을 `permissions`에 추가 후 재로드 필요.

---

## 기능

| 버튼 / 메뉴 | API |
|---|---|
| Save Page | `POST /api/capture/link` |
| Save Selection | `POST /api/capture/clip` |
| Save Note | `POST /api/capture/note` |
| 우클릭 → Save page to LLM Wiki | `POST /api/capture/link` |
| 우클릭 → Save selection to LLM Wiki | `POST /api/capture/clip` |

---

## 설치

### Chrome

1. `chrome://extensions/` → 개발자 모드 활성화
2. 압축 해제된 확장 프로그램 로드 → `extensions/browser-capture/` 선택
3. Options에서 Capture API URL / Token 입력

### Firefox

1. `about:debugging` → This Firefox → Load Temporary Add-on
2. `extensions/browser-capture-firefox/manifest.json` 선택
3. Options에서 Capture API URL / Token 입력

---

## Capture API URL (Windows Firefox → Mac mini)

Mac mini가 `http://127.0.0.1:5055`로 바인딩된 경우 Windows Firefox에서 직접 접근 불가.

| 방법 | 예시 |
|---|---|
| LAN IP | `http://192.168.1.10:5055` |
| Tailscale | `http://100.x.x.x:5055` |
| SSH tunnel | `ssh -L 5055:127.0.0.1:5055 user@mac` |

상세 절차: [FIREFOX_EXTENSION_LOCAL_TEST.md](FIREFOX_EXTENSION_LOCAL_TEST.md)

---

## 패키징

```powershell
.\scripts\extensions\package-firefox-extension.ps1
# 출력: artifacts/extensions/llm-wiki-capture-firefox.zip
```
