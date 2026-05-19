# LLM Wiki Browser Capture Extension

Chrome Extension MVP for capturing pages, clips, and notes to LLM Wiki via Capture API.

## 상태

로컬 개발용 스캐폴드. Chrome Web Store 미배포.

## 설치 (Chrome 로컬 로드)

1. Chrome → `chrome://extensions/` 열기
2. 우측 상단 **개발자 모드** 활성화
3. **압축 해제된 확장 프로그램 로드** 클릭
4. 이 디렉토리(`extensions/browser-capture/`) 선택

## 초기 설정

확장 아이콘 → Options (또는 `chrome://extensions/` → Details → Extension options)

| 항목 | 설명 |
|---|---|
| Capture API URL | `http://127.0.0.1:5055` (기본값) |
| Capture API Token | `.env`의 `CAPTURE_API_TOKEN` 값 |

## 기능

| 버튼 / 메뉴 | 동작 | API |
|---|---|---|
| Save Page | 현재 페이지 URL + 제목 저장 | `POST /api/capture/link` |
| Save Selection | 선택 텍스트 + URL 저장 | `POST /api/capture/clip` |
| Save Note | 팝업 텍스트 입력 저장 | `POST /api/capture/note` |
| 우클릭 → Save page to LLM Wiki | Save Page와 동일 | `POST /api/capture/link` |
| 우클릭 → Save selection to LLM Wiki | Save Selection과 동일 | `POST /api/capture/clip` |

## host_permissions

`http://127.0.0.1:5055/*` — Capture API 로컬 접근에 필요.

원격 서버로 변경 시 Options에서 URL 수정 후 `manifest.json`의 `host_permissions`도 갱신 필요.

## Firefox 호환성 (미검증)

Manifest V3 + WebExtensions API 사용이므로 이론적으로 호환 가능.
`browser_specific_settings` 추가 및 실제 패키징은 후속 작업.
