# Capture API Architecture

## 프로젝트 구성

```
src/LlmWiki.CaptureApi/
  Program.cs                 — Minimal API 진입점, 환경 변수 로드, 라우팅
  Capture/
    CaptureRequest.cs        — 요청 DTO
    CaptureAuth.cs           — Bearer Token 검증 헬퍼
    CaptureFileName.cs       — 타임스탬프 + slug 파일명 생성, 충돌 방지
    CaptureWriter.cs         — JSON 파일을 Inbox에 기록
```

## 의존 관계

```
LlmWiki.CaptureApi → LlmWiki.Core (ConfigLoader, SlugGenerator)
```

## 요청 흐름

```
Chrome Extension / curl
  → POST /api/capture/{link|clip|note}
  → CaptureAuth.IsAuthorized()
  → CaptureWriter.Write{Link|Clip|Note}()
  → CaptureFileName.Generate()
  → File.WriteAllText() → Inbox/{links|raw_clips}/YYYY-MM-DD-HHMMSS-slug.json
  → watcher가 .json 감지 → CaptureJsonReader.Read() → Gemini → Notes/
```

## JSON Inbox 처리 흐름

```
InboxProcessor.CollectItems()
  → .json 파일: CaptureJsonReader.Read()
      성공: InboxItem (RawText = Markdown 형식 텍스트)
      실패: parseFailures 리스트 → InboxMover.MoveToFailed()
  → .txt/.md/.url: 기존 텍스트 읽기
```

## 파일명 충돌 방지

```
YYYY-MM-DD-HHMMSS-slug.json
YYYY-MM-DD-HHMMSS-slug-1.json
YYYY-MM-DD-HHMMSS-slug-2.json
...
```

## launchd 구성

```
com.llmwiki.captureapi (별도 agent)
  ↑ 독립 프로세스 — com.llmwiki.watch와 별개로 동작
```

두 agent 모두 동일 Inbox 디렉토리를 사용한다.
`CaptureApi`가 파일을 쓰고, `watch`가 파일 시스템 이벤트를 감지한다.
