# Firefox Extension Scaffold Report

## 1. 수행 결과

- 완료:
  - `extensions/browser-capture-firefox/` 생성
  - Firefox MV2 manifest 작성 (gecko ID, strict_min_version 91.0)
  - `shared/browserCompat.js` — `ext = browser ?? chrome` shim
  - `shared/captureClient.js` — `postCapture()` 공통 함수
  - `src/background.js` — `ext.*` API, context menu, capture 요청
  - `src/popup.html` / `popup.js` — Save Page / Selection / Note
  - `src/options.html` / `options.js` — URL/Token 설정 (password input)
  - `src/content.js` — 선택 텍스트 추출
  - `extensions/browser-capture-firefox/README.md`
  - `scripts/extensions/package-firefox-extension.ps1` — zip 패키징
  - `docs/capture/FIREFOX_EXTENSION_LOCAL_TEST.md`
  - `docs/capture/BROWSER_EXTENSION_MVP_SPEC.md` 갱신
  - `docs/capture/CAPTURE_ROADMAP.md` 갱신
- 부분 완료:
  - 아이콘 미제작 (icons/ 디렉터리만 생성)
- 미완료:
  - Firefox 실제 로드 검증 (후속 작업)
  - Mac mini Capture API 실환경 연동 검증 (후속 작업)

---

## 2. 생성/수정 파일

| 파일 | 변경 내용 |
|---|---|
| `extensions/browser-capture-firefox/manifest.json` | 신규 — MV2, gecko ID |
| `extensions/browser-capture-firefox/shared/browserCompat.js` | 신규 — browser/chrome shim |
| `extensions/browser-capture-firefox/shared/captureClient.js` | 신규 — postCapture() 공통 함수 |
| `extensions/browser-capture-firefox/src/background.js` | 신규 — ext.* API |
| `extensions/browser-capture-firefox/src/popup.html` | 신규 |
| `extensions/browser-capture-firefox/src/popup.js` | 신규 — ext.* API, shared 사용 |
| `extensions/browser-capture-firefox/src/options.html` | 신규 — URL 주의사항 포함 |
| `extensions/browser-capture-firefox/src/options.js` | 신규 — Promise 기반 |
| `extensions/browser-capture-firefox/src/content.js` | 신규 |
| `extensions/browser-capture-firefox/README.md` | 신규 |
| `scripts/extensions/package-firefox-extension.ps1` | 신규 |
| `docs/capture/FIREFOX_EXTENSION_LOCAL_TEST.md` | 신규 |
| `docs/capture/BROWSER_EXTENSION_MVP_SPEC.md` | 갱신 — Firefox 포함 |
| `docs/capture/CAPTURE_ROADMAP.md` | 갱신 — Firefox 완료 항목 추가 |

---

## 3. Firefox Extension 기능

| 기능 | 상태 |
|---|---|
| Save Page | 구현 완료 |
| Save Selection | 구현 완료 |
| Save Note | 구현 완료 |
| Options (URL + Token) | 구현 완료 |
| Context menu (우클릭) | 구현 완료 |
| 아이콘 | 미제작 |

---

## 4. Chrome / Firefox 공통화

| 항목 | 처리 |
|---|---|
| Capture API client (`postCapture`) | Firefox: `shared/captureClient.js` 공통 함수 / Chrome: 인라인 (미공통화) |
| Storage wrapper | Firefox: `ext.storage.sync` Promise 체인 / Chrome: `chrome.storage.sync` |
| Browser API wrapper | Firefox: `shared/browserCompat.js` (`ext = browser ?? chrome`) |

Chrome extension은 현재 `chrome.*` 인라인 방식을 유지한다.
향후 Chrome도 `shared/` 모듈을 참조하도록 마이그레이션 가능하나 이번 작업 범위 외.

---

## 5. 디렉터리 구조

```
extensions/
  browser-capture/                — Chrome MV3 (기존, 변경 없음)
  browser-capture-firefox/        — Firefox MV2 (신규)
    manifest.json
    shared/
      browserCompat.js
      captureClient.js
    src/
      background.js
      popup.html / popup.js
      options.html / options.js
      content.js
    icons/
    README.md
scripts/
  extensions/
    package-firefox-extension.ps1
```

---

## 6. Manifest 비교

| 항목 | Chrome (browser-capture/) | Firefox (browser-capture-firefox/) |
|---|---|---|
| manifest_version | 3 | 2 |
| background | `service_worker` | `scripts` 배열 |
| popup | `action.default_popup` | `browser_action.default_popup` |
| host permissions | `host_permissions` 필드 | `permissions` 배열 포함 |
| Firefox ID | 없음 | `browser_specific_settings.gecko.id` |

---

## 7. 로컬 테스트 방법

```
1. Firefox → about:debugging → This Firefox
2. Load Temporary Add-on → extensions/browser-capture-firefox/manifest.json 선택
3. Extension 아이콘 → Options → Capture API URL / Token 입력
4. 임의 페이지에서 Save Page / Save Selection / Save Note 테스트
5. Mac mini Inbox 확인
```

Capture API URL 주의:
- Mac mini 바인딩이 127.0.0.1이면 Windows Firefox에서 LAN IP / Tailscale IP 사용 필요
- 상세: `docs/capture/FIREFOX_EXTENSION_LOCAL_TEST.md`

패키징:
```powershell
.\scripts\extensions\package-firefox-extension.ps1
# → artifacts/extensions/llm-wiki-capture-firefox.zip
```

---

## 8. 테스트 결과

```
dotnet build: 경고 0, 오류 0
dotnet test:  통과 89, 실패 0, 건너뜀 0
```

JavaScript extension 파일은 브라우저 내 실행 환경이 필요해 dotnet test 범위 외.
manifest.json JSON 구문: 수동 확인 완료.

---

## 9. 실제 검증 필요

| 항목 | 상태 |
|---|---|
| Firefox `about:debugging` 로드 | 미검증 |
| Options 저장 | 미검증 |
| Save Page 전송 | 미검증 |
| Save Selection 전송 | 미검증 |
| Capture API 수신 (Mac mini) | 미검증 |
| watcher JSON 처리 | 미검증 |
| LAN IP / Tailscale 접근 | 미검증 |

---

## 10. 다음 작업 후보

1. Mac mini Capture API 배포 → curl 검증 → Firefox 로컬 로드 테스트
2. Firefox options에서 LAN/Tailscale IP 입력 후 end-to-end 검증
3. 아이콘 제작 (16px / 48px / 128px)
