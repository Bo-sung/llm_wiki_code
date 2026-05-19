# Setup Report — v0.1 초기 세팅

작성일: 2026-05-19

## 수행 결과

- **완료**: 디렉터리 구조 생성, Python 패키지 설정, 환경 변수 로딩, Gemini 클라이언트 뼈대, Markdown 노트 생성 유틸, Frontmatter 유틸, Slug 유틸, Inbox 처리 프로세서, Watcher 뼈대, 단일 실행 스크립트, 기본 테스트 17개, README
- **부분 완료**: 없음
- **미완료**: 모바일/브라우저 입력 경로 (범위 외), Inbox → Notes 자동 이동 후 원본 삭제 정책 (결정 필요)

## 생성/수정 파일

| 파일 | 목적 |
|---|---|
| `pyproject.toml` | Python 패키지 설정, 의존성 |
| `.env.example` | 환경 변수 템플릿 |
| `.gitignore` | git 제외 파일 설정 |
| `README.md` | 설치 및 실행 방법 |
| `src/llm_wiki/__init__.py` | 패키지 진입점 |
| `src/llm_wiki/config.py` | 환경 변수 로딩, 경로 상수 |
| `src/llm_wiki/paths.py` | 카테고리별 출력 경로 해석 |
| `src/llm_wiki/gemini_client.py` | Gemini API 호출 래퍼 (실패 시 fallback) |
| `src/llm_wiki/markdown_writer.py` | 구조화된 dict → .md 파일 |
| `src/llm_wiki/inbox_watcher.py` | watchdog 기반 Inbox 감시 |
| `src/llm_wiki/utils/slug.py` | 파일명 slug 생성 |
| `src/llm_wiki/utils/frontmatter.py` | YAML frontmatter 생성/직렬화 |
| `src/llm_wiki/processors/link_processor.py` | links/ 폴더 파일 처리 |
| `src/llm_wiki/processors/text_processor.py` | raw_clips/, mobile/ 파일 처리 |
| `scripts/process_once.py` | Inbox 현재 파일 일괄 처리 |
| `scripts/run_watcher.py` | Watcher 시작 스크립트 |
| `tests/test_slug.py` | slug 유틸 테스트 |
| `tests/test_frontmatter.py` | frontmatter 유틸 테스트 |
| `tests/test_markdown_writer.py` | Markdown 작성 테스트 |
| `data/Templates/note_template.md` | 노트 템플릿 |
| `data/Templates/source_template.md` | 원문 소스 템플릿 |
| `data/System/prompts/refine_note.md` | Gemini에 전달할 프롬프트 |
| `data/System/rules.md` | 위키 운영 규칙 |
| `data/System/taxonomy.md` | 태그 분류 체계 |
| `data/Index/master_index.md` | 마스터 인덱스 (수동) |
| `data/Index/tag_index.md` | 태그 인덱스 플레이스홀더 |
| `data/Index/source_index.md` | 소스 인덱스 플레이스홀더 |

## 실행 명령

```bash
# 가상환경 생성 및 활성화
python -m venv .venv
.venv\Scripts\activate  # Windows

# 설치
pip install -e ".[dev]"

# 테스트
pytest

# 단일 처리
python scripts/process_once.py

# 감시 모드
python scripts/run_watcher.py
```

## 테스트 결과

```text
============================= test session starts =============================
platform win32 -- Python 3.13.5, pytest-9.0.3
collected 17 items

tests/test_frontmatter.py::test_build_frontmatter_defaults PASSED
tests/test_frontmatter.py::test_build_frontmatter_date_obj PASSED
tests/test_frontmatter.py::test_build_frontmatter_date_str PASSED
tests/test_frontmatter.py::test_render_frontmatter_simple_contains_delimiters PASSED
tests/test_frontmatter.py::test_render_frontmatter_simple_list PASSED
tests/test_frontmatter.py::test_frontmatter_to_str_returns_string PASSED
tests/test_markdown_writer.py::test_write_note_creates_file PASSED
tests/test_markdown_writer.py::test_write_note_contains_frontmatter PASSED
tests/test_markdown_writer.py::test_write_note_contains_sections PASSED
tests/test_markdown_writer.py::test_write_note_filename_has_date PASSED
tests/test_markdown_writer.py::test_write_note_fallback_title PASSED
tests/test_slug.py::test_make_slug_basic PASSED
tests/test_slug.py::test_make_slug_special_chars PASSED
tests/test_slug.py::test_make_slug_empty PASSED
tests/test_slug.py::test_make_slug_max_len PASSED
tests/test_slug.py::test_dated_slug_format PASSED
tests/test_slug.py::test_dated_slug_default_date PASSED

17 passed in 0.16s
```

## 현재 동작 방식

- **입력**: `data/Inbox/links/`, `data/Inbox/raw_clips/`, `data/Inbox/mobile/` 에 `.txt`/`.url`/`.md` 파일 배치
- **처리**: `process_once.py` 실행 → 파일 유형 판별 → Gemini API 호출 (키 없으면 fallback dict 반환) → Markdown 노트 생성
- **출력**: `data/Notes/{카테고리}/YYYY-MM-DD-slug.md` 파일 생성 (Obsidian에서 바로 열기 가능)

## 결정 필요

| 항목 | 선택지 | 권장안 | 이유 |
|---|---|---|---|
| Gemini 모델명 | `gemini-1.5-flash`, `gemini-1.5-pro`, `gemini-2.0-flash` 등 | `gemini-1.5-flash` (현재 코드 기본값) | 비용/속도 균형. 단, 고정 시 모델 deprecated 시 코드 수정 필요 |
| 처리 후 Inbox 원본 파일 처리 | (1) 삭제, (2) processed/ 폴더로 이동, (3) 그대로 유지 | 결정 필요 | 중복 처리 방지 vs. 원본 보존 트레이드오프 |
| 카테고리 자동 분류 | (1) Gemini가 태그 보고 자동 분류, (2) 사용자가 파일 배치 시 지정, (3) 항상 References에 저장 | 결정 필요 | 자동화 수준 결정 |
| Mac mini 실행 지속성 | (1) launchd plist, (2) cron, (3) 수동 실행 | 결정 필요 | 현재는 수동 실행 뼈대만 존재. 자동 시작 방식 미결정 |
| GitHub 연동 | (1) 수동 push, (2) 처리 후 자동 commit, (3) 자동 push | 결정 필요 | 백업 정책 결정 필요 |

## 다음 작업 후보

1. `.env` 파일 생성 후 실제 Gemini API 키로 end-to-end 실행 검증
2. 처리 후 Inbox 원본 파일 이동/삭제 로직 추가 (결정 후)
3. `process_once.py`에 카테고리 자동 분류 로직 연결

## 리스크 / 주의점

- `gemini_client.py`의 Gemini 모델명이 `gemini-1.5-flash`로 하드코딩됨. 모델명을 `.env`로 추출할지 결정 필요.
- `process_once.py`는 이미 처리된 파일과 미처리 파일을 구분하지 않음. 중복 처리 가능성 있음.
- Windows 환경에서 개발/테스트 완료. Mac mini 실행 환경에서 경로 구분자(`\` vs `/`) 및 watchdog 동작 차이 검증 필요.
- Gemini API 응답이 JSON이 아닐 경우 `json.loads` 실패 → fallback 처리됨. 응답 포맷 불안정 시 재시도 로직 추가 검토 필요.
