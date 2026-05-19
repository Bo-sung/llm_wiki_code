# Capture API Start Report

## 1. 수행 결과

- 완료:
  - LlmWiki.CaptureApi 프로젝트 생성 및 솔루션 추가
  - Capture API 3개 엔드포인트 구현 (link / clip / note)
  - Bearer Token 인증 구현
  - CaptureWriter — Inbox JSON 파일 저장
  - CaptureFileName — 타임스탬프 slug + 충돌 방지
  - InboxEventFilter .json 허용 추가
  - InboxProcessor .json 처리 (CaptureJsonReader)
  - 유효하지 않은 JSON → failed/ 이동
  - Capture API launchd 스크립트 4종
  - deploy-to-mac.ps1 CaptureApi publish 추가
  - .env.example 갱신
  - Chrome Extension MVP 스캐폴드 (manifest, popup, options, background, content)
  - 테스트 추가 (CaptureFileName, CaptureWriter, CaptureAuth, CaptureJsonReader, InboxProcessor JSON, InboxEventFilter .json)
  - 문서 5종 + 보고서 작성
- 부분 완료:
  - Chrome Extension 아이콘 미제작 (icons/ 디렉토리만 생성)
- 미완료:
  - Mac mini 실제 배포 및 curl 검증 (다음 단계)

---

## 2. 생성/수정 파일

| 파일 | 변경 내용 |
|---|---|
| `src/LlmWiki.CaptureApi/LlmWiki.CaptureApi.csproj` | 신규 생성 |
| `src/LlmWiki.CaptureApi/Program.cs` | 신규 생성 |
| `src/LlmWiki.CaptureApi/Capture/CaptureRequest.cs` | 신규 생성 |
| `src/LlmWiki.CaptureApi/Capture/CaptureAuth.cs` | 신규 생성 |
| `src/LlmWiki.CaptureApi/Capture/CaptureFileName.cs` | 신규 생성 |
| `src/LlmWiki.CaptureApi/Capture/CaptureWriter.cs` | 신규 생성 |
| `src/LlmWiki.Core/Inbox/CaptureJsonReader.cs` | 신규 생성 |
| `src/LlmWiki.Core/Inbox/InboxEventFilter.cs` | .json 확장자 추가 |
| `src/LlmWiki.Core/Inbox/InboxProcessor.cs` | .json 처리, ReadTextItem/ReadJsonItem 분리, parseFailures 처리 |
| `llm-wiki.slnx` | CaptureApi 프로젝트 추가 |
| `.env.example` | CAPTURE_API_TOKEN, CAPTURE_API_BIND_URL 추가 |
| `scripts/deploy/deploy-to-mac.ps1` | CaptureApi publish, smoke test 추가 |
| `scripts/mac/com.llmwiki.captureapi.plist.template` | 신규 생성 |
| `scripts/mac/install-launchd-captureapi.sh` | 신규 생성 |
| `scripts/mac/uninstall-launchd-captureapi.sh` | 신규 생성 |
| `scripts/mac/status-launchd-captureapi.sh` | 신규 생성 |
| `extensions/browser-capture/manifest.json` | 신규 생성 |
| `extensions/browser-capture/src/background.js` | 신규 생성 |
| `extensions/browser-capture/src/popup.html` | 신규 생성 |
| `extensions/browser-capture/src/popup.js` | 신규 생성 |
| `extensions/browser-capture/src/options.html` | 신규 생성 |
| `extensions/browser-capture/src/options.js` | 신규 생성 |
| `extensions/browser-capture/src/content.js` | 신규 생성 |
| `extensions/browser-capture/README.md` | 신규 생성 |
| `tests/LlmWiki.Tests/LlmWiki.Tests.csproj` | CaptureApi 참조 추가 |
| `tests/LlmWiki.Tests/CaptureFileNameTests.cs` | 신규 생성 |
| `tests/LlmWiki.Tests/CaptureWriterTests.cs` | 신규 생성 |
| `tests/LlmWiki.Tests/CaptureAuthTests.cs` | 신규 생성 |
| `tests/LlmWiki.Tests/CaptureJsonReaderTests.cs` | 신규 생성 |
| `tests/LlmWiki.Tests/InboxEventFilterTests.cs` | .json 케이스 추가 |
| `tests/LlmWiki.Tests/InboxProcessorTests.cs` | JSON 처리 테스트 2개 추가 |
| `docs/capture/CAPTURE_API_PRODUCT_SPEC.md` | 신규 생성 |
| `docs/capture/CAPTURE_API_ARCHITECTURE.md` | 신규 생성 |
| `docs/capture/CAPTURE_SECURITY_POLICY.md` | 신규 생성 |
| `docs/capture/CAPTURE_API_OPERATION_GUIDE.md` | 신규 생성 |
| `docs/capture/BROWSER_EXTENSION_MVP_SPEC.md` | 신규 생성 |
| `docs/capture/CAPTURE_ROADMAP.md` | 신규 생성 |

---

## 3. 추가된 프로젝트

| 프로젝트 | 역할 |
|---|---|
| `LlmWiki.CaptureApi` | ASP.NET Core Minimal API — 웹 capture 수신, Inbox JSON 저장 |

---

## 4. API 엔드포인트

| Method | Path | 인증 | 역할 |
|---|---|---|---|
| GET | `/health` | 없음 | 상태 확인 |
| POST | `/api/capture/link` | Bearer Token | URL + 제목 → `Inbox/links/` |
| POST | `/api/capture/clip` | Bearer Token | URL + 선택 텍스트 → `Inbox/raw_clips/` |
| POST | `/api/capture/note` | Bearer Token | 수동 메모 → `Inbox/raw_clips/` |

---

## 5. Inbox JSON 처리

| 항목 | 상태 |
|---|---|
| .json watcher 허용 | 구현 완료 |
| link JSON 처리 | 구현 완료 |
| clip JSON 처리 | 구현 완료 |
| note JSON 처리 | 구현 완료 |
| invalid JSON 실패 처리 | 구현 완료 (failed/ 이동) |

---

## 6. launchd 구성

| 항목 | 값 |
|---|---|
| label | `com.llmwiki.captureapi` |
| plist | `~/Library/LaunchAgents/com.llmwiki.captureapi.plist` |
| stdout | `~/apps/llm-wiki/shared/logs/captureapi.out.log` |
| stderr | `~/apps/llm-wiki/shared/logs/captureapi.err.log` |
| RunAtLoad | true |
| KeepAlive | true |

---

## 7. Browser Extension MVP

| 항목 | 상태 |
|---|---|
| manifest (MV3) | 구현 완료 |
| popup (Save Page / Selection / Note) | 구현 완료 |
| options (URL + Token) | 구현 완료 |
| background (context menu) | 구현 완료 |
| content (선택 텍스트 추출) | 구현 완료 |
| icons | 미제작 (디렉토리만 생성) |

---

## 8. 테스트 결과

```
통과: 89, 실패: 0, 건너뜀: 0
```

신규 추가 테스트:
- `CaptureFileNameTests` — 슬러그 생성, 충돌 방지
- `CaptureWriterTests` — link/clip/note JSON 저장 경로 및 내용
- `CaptureAuthTests` — Bearer Token 검증
- `CaptureJsonReaderTests` — link/clip/note 읽기, invalid JSON 예외
- `InboxProcessorTests` — JSON link 처리, invalid JSON failed 이동
- `InboxEventFilterTests` — .json 확장자 허용

---

## 9. 실제 Mac mini 검증 필요

| 항목 | 상태 |
|---|---|
| Capture API 배포 | 미검증 |
| launchd 등록 | 미검증 |
| curl health | 미검증 |
| curl capture link | 미검증 |
| watcher JSON 처리 | 미검증 |
| Chrome extension 로컬 로드 | 미검증 |

---

## 10. 다음 작업 후보

1. Mac mini 배포 및 curl end-to-end 검증
2. Chrome Extension 아이콘 제작 및 로컬 로드 테스트
3. watcher JSON → Gemini → Notes 처리 확인
