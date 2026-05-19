# llm-wiki

개인 LLM 위키 파이프라인. URL 또는 텍스트를 `data/Inbox`에 넣으면 Gemini API로 정리된 Markdown 노트를 생성한다.

## 현재 MVP 범위

- Inbox 파일 1회 일괄 처리 (`process-once`)
- Inbox 파일 감시 모드 (`watch`)
- Gemini API 미설정 시 fallback 노트 생성
- 출력: Obsidian에서 바로 열 수 있는 `.md` 파일

## 제외된 기능 (초기 MVP 외)

- 모바일 앱 / 브라우저 익스텐션
- 서버 / 클라우드 배포
- DB
- OAuth
- Context Caching
- 자동 Git push
- 카테고리 자동 분류 (현재 모든 노트 `data/Notes/References/` 저장)

## 요구사항

- .NET 8 SDK (LTS)
- Gemini API 키 (없어도 실행 가능 — fallback 노트 생성)

## 환경 설정

```bash
cp .env.example .env
```

`.env` 파일:

```env
GEMINI_API_KEY=your_key_here
LLM_WIKI_ROOT=./data
GEMINI_MODEL=gemini-1.5-flash
```

`GEMINI_API_KEY` 또는 `GEMINI_MODEL`이 비어 있으면 Gemini 호출 없이 fallback 노트를 생성한다.

## 실행

### Inbox 일괄 처리 (1회)

```bash
dotnet run --project src/LlmWiki.Cli -- process-once
```

### Inbox 감시 모드

```bash
dotnet run --project src/LlmWiki.Cli -- watch
```

## 테스트

```bash
dotnet test
```

## Inbox 사용법

| 경로 | 용도 |
|---|---|
| `data/Inbox/links/` | URL이 담긴 `.txt` 또는 `.url` 파일 |
| `data/Inbox/raw_clips/` | 복사한 텍스트 클립 `.txt` 파일 |
| `data/Inbox/mobile/` | 모바일에서 보낸 텍스트 `.txt` 파일 |

처리 결과는 `data/Notes/References/YYYY-MM-DD-slug.md`에 저장된다.

## 디렉터리 구조

```
src/
  LlmWiki.Core/    ← 핵심 로직 (Gemini, Markdown, Inbox, Utils)
  LlmWiki.Cli/     ← CLI 진입점
tests/
  LlmWiki.Tests/   ← 단위 테스트
data/
  Inbox/           ← 입력
  Notes/           ← 출력 (Obsidian에서 열기)
  System/          ← 프롬프트, 규칙, 분류 체계
docs/
  DECISIONS.md     ← 프로젝트 결정사항 기록
archive/
  prototypes/python/   ← 초기 Python 프로토타입 보관
```

## Python 프로토타입

초기 Python 프로토타입은 `archive/prototypes/python/`에 보관되어 있다.
본 구현으로 간주하지 않는다.
