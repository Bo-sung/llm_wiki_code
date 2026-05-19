# Python Prototype to C# Migration Report

작성일: 2026-05-19

## 1. 수행 결과

- **완료**: Python 산출물 아카이브 이동, C# 솔루션 생성, 전체 기능 1차 포팅, 테스트 18개, README 갱신, `docs/DECISIONS.md` 생성, 빌드 및 테스트 통과
- **부분 완료**: `watch` 명령 — 뼈대 구현 완료, Mac mini 환경 검증 미수행
- **미완료**: 카테고리 자동 분류 (결정 전), 처리 후 원본 파일 이동 (결정 전)

## 2. 이동/보존한 파일

| 경로 | 처리 | 이유 |
|---|---|---|
| `pyproject.toml` | `archive/prototypes/python/`으로 이동 | Python 프로토타입 보존 |
| `src/llm_wiki/` | `archive/prototypes/python/src/`로 이동 | 동일 |
| `scripts/` | `archive/prototypes/python/scripts/`로 이동 | 동일 |
| `tests/` (Python) | `archive/prototypes/python/tests/`로 이동 | 동일 |
| `data/` | 루트 유지 | C# 구현에서 그대로 사용 |
| `data/System/prompts/refine_note.md` | 내용 유지 | C# Gemini 클라이언트가 동일 파일 사용 |
| `.env.example` | 루트 유지 + `GEMINI_MODEL` 추가 | 모델명 환경 변수 정책 반영 |
| `reports/` | 루트 유지 | 보고서 누적 보관 |
| `.gitignore` | 루트 유지, C# 항목 추가 | 동일 |

## 3. 생성/수정 파일

| 파일 | 목적 |
|---|---|
| `llm-wiki.slnx` | .NET 솔루션 파일 |
| `src/LlmWiki.Core/Config/AppConfig.cs` | 설정 모델 |
| `src/LlmWiki.Core/Config/ConfigLoader.cs` | .env + 환경 변수 로딩 |
| `src/LlmWiki.Core/Utils/SlugGenerator.cs` | 파일명 slug 생성 |
| `src/LlmWiki.Core/Utils/FileNameSanitizer.cs` | 파일명 특수문자 제거 |
| `src/LlmWiki.Core/Markdown/WikiNote.cs` | 노트 데이터 모델 |
| `src/LlmWiki.Core/Markdown/FrontmatterWriter.cs` | YAML frontmatter 생성 |
| `src/LlmWiki.Core/Markdown/MarkdownWriter.cs` | .md 파일 생성 |
| `src/LlmWiki.Core/Gemini/GeminiClient.cs` | Gemini REST API 호출 |
| `src/LlmWiki.Core/Gemini/GeminiRequest.cs` | 요청 DTO |
| `src/LlmWiki.Core/Gemini/GeminiResponse.cs` | 응답 DTO |
| `src/LlmWiki.Core/Gemini/GeminiNoteDraft.cs` | Gemini 응답 JSON 역직렬화 모델 |
| `src/LlmWiki.Core/Inbox/InboxItem.cs` | Inbox 파일 모델 |
| `src/LlmWiki.Core/Inbox/InboxProcessor.cs` | Inbox 1회 처리 로직 |
| `src/LlmWiki.Core/Inbox/InboxWatcher.cs` | FileSystemWatcher 뼈대 |
| `src/LlmWiki.Cli/Program.cs` | CLI 진입점 (`process-once`, `watch`) |
| `tests/LlmWiki.Tests/SlugGeneratorTests.cs` | slug 단위 테스트 7개 |
| `tests/LlmWiki.Tests/FrontmatterWriterTests.cs` | frontmatter 단위 테스트 5개 |
| `tests/LlmWiki.Tests/MarkdownWriterTests.cs` | Markdown 작성 단위 테스트 6개 |
| `README.md` | C#/.NET 기준으로 갱신 |
| `docs/DECISIONS.md` | 프로젝트 결정사항 기록 |
| `.env.example` | `GEMINI_MODEL` 항목 추가 |

## 4. 실행 명령

```bash
# 빌드
dotnet build

# 테스트
dotnet test

# Inbox 일괄 처리
dotnet run --project src/LlmWiki.Cli -- process-once

# 감시 모드
dotnet run --project src/LlmWiki.Cli -- watch
```

## 5. 테스트 결과

```text
총 테스트 수: 18
     통과: 18
 총 시간: 1.15초
```

## 6. 현재 동작 방식

- **입력**: `data/Inbox/links/`, `data/Inbox/raw_clips/`, `data/Inbox/mobile/`에 `.txt`/`.url`/`.md` 파일 배치
- **처리**: `process-once` 실행 → 파일 수집 → Gemini 설정 여부 확인 → API 호출 또는 fallback → 노트 생성
- **출력**: `data/Notes/References/YYYY-MM-DD-slug.md` (Obsidian에서 바로 열기 가능)

## 7. 결정사항 반영 체크

| 결정사항 | 반영 여부 | 비고 |
|---|---|---|
| C# 사용 | 완료 | .NET 8 LTS |
| .NET LTS | 완료 | net8.0 대상 |
| Mac mini 로컬 기준 | 완료 (설계) | 실제 환경 검증 필요 |
| Markdown 저장 | 완료 | `.md` 파일 출력 |
| Gemini API Key | 완료 | `GEMINI_API_KEY` 환경 변수 |
| OAuth 제외 | 완료 | 미구현 |
| DB 제외 | 완료 | 파일 기반만 |
| 서버 제외 | 완료 | 미구현 |
| Context Caching 제외 | 완료 | 미구현 |
| 자동 Git push 제외 | 완료 | 미구현 |
| 모델명 하드코딩 금지 | 완료 | `GEMINI_MODEL` 비어 있으면 fallback |

## 8. 결정 필요

| 항목 | 선택지 | 권장안 | 이유 |
|---|---|---|---|
| 처리 후 원본 파일 처리 | 삭제 / `processed/` 이동 / 유지 | `processed/YYYY-MM-DD/` 이동 | 중복 처리 방지와 원본 보존 균형 |
| 카테고리 자동 분류 | Gemini 분류 / 사용자 지정 / 항상 References | 보류 | 정리 품질 확인 후 결정 |
| Mac mini 상시 실행 | launchd / 수동 / cron | launchd | macOS 상시 실행에 적합 |
| GitHub 백업 | 수동 push / 자동 commit / 자동 push | 보류 | 백업 정책 확정 필요 |

## 9. 리스크 / 주의점

- `InboxWatcher`는 Windows에서 테스트됨. macOS의 FSEvents 기반 FileSystemWatcher 동작 차이 검증 필요.
- `process-once`는 이미 처리된 파일과 미처리 파일을 구분하지 않음. 원본 파일 처리 정책 미확정 상태.
- Gemini 응답 JSON 파싱 실패 시 fallback 노트 생성. 프롬프트가 JSON 이외 형식을 반환하면 항상 fallback. `data/System/prompts/refine_note.md` 내용 품질이 Gemini 응답 포맷에 영향을 줌.
- .NET SDK 10.0.204가 기본 SDK로 설정되어 있으나 프로젝트 대상 프레임워크는 net8.0. `global.json`으로 SDK 버전 고정을 추후 검토할 수 있음.

## 10. 다음 작업 후보

1. Mac mini에서 end-to-end 실행 검증 (Gemini API 키 설정 후)
2. 처리 후 원본 파일 `processed/` 이동 구현 (결정 후)
3. `watch` 명령 Mac mini 환경 검증
