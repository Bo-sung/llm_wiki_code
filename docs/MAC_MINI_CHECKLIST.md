# Mac mini 검증 체크리스트

Mac mini 실환경에서 처음 실행할 때 수행할 확인 단계.
결과는 `reports/mac_mini_validation_report.md`에 기록한다.

---

## 1. 사전 확인

- [ ] .NET 런타임 설치 확인 (배포 스크립트가 미설치 시 자동 설치)
  ```bash
  /usr/local/share/dotnet/dotnet --list-runtimes
  # 또는
  ~/.dotnet/dotnet --list-runtimes
  ```
- [ ] `shared/.env` 생성 확인
  ```bash
  ls ~/apps/llm-wiki/shared/.env
  ```
- [ ] `shared/.env` 내용: `GEMINI_API_KEY`, `GEMINI_MODEL=gemini-2.5-flash`, `LLM_WIKI_ROOT` 설정 여부
  - `gemini-1.5-flash`는 404 반환 — `gemini-2.5-flash` 사용
- [ ] `LLM_WIKI_ENV_FILE` 환경변수 방식으로 .env 로딩 (shell source 불필요)

---

## 2. 빌드

```bash
dotnet build
```

예상 출력: `빌드했습니다. 경고 0개 오류 0개`

---

## 3. 테스트

```bash
dotnet test
```

예상 출력: `총 테스트 수: 30  통과: 30`

---

## 4. fallback 처리 검증

목적: Gemini 미설정 상태에서 fallback 노트 생성 및 파일 이동 확인

조건:
- `.env`에서 `GEMINI_API_KEY` 또는 `GEMINI_MODEL` 비워둠

준비:
```bash
echo "https://example.com" > data/Inbox/links/test-link.txt
echo "테스트 클립 내용" > data/Inbox/raw_clips/test-clip.txt
```

실행:
```bash
dotnet run --project src/LlmWiki.Cli -- process-once
```

확인 항목:
- [ ] 로그에 `scanned=2` 출력
- [ ] 로그에 `gemini_calls=0 fallbacks=2` 출력
- [ ] `data/Notes/References/` 에 `.md` 파일 2개 생성
- [ ] `data/Inbox/processed/YYYY-MM-DD/` 에 원본 파일 이동
- [ ] `data/Inbox/links/test-link.txt` 삭제됨

---

## 5. dry-run 검증

```bash
echo "https://example.com" > data/Inbox/links/dry-test.txt
dotnet run --project src/LlmWiki.Cli -- process-once --dry-run
```

확인 항목:
- [ ] `dry_run=True` 로그 출력
- [ ] 노트 파일 생성 없음
- [ ] 원본 파일 이동 없음
- [ ] `data/Inbox/links/dry-test.txt` 그대로 존재

---

## 6. Gemini 실제 호출 검증

목적: API 키와 모델명 설정 후 실제 Gemini 호출 확인

조건:
- `shared/.env`에 `GEMINI_API_KEY` 설정
- `shared/.env`에 `GEMINI_MODEL=gemini-2.5-flash` 설정 (`gemini-1.5-flash` 사용 금지 — 404)

```bash
echo "Claude Code is Anthropic's CLI for AI-assisted development." \
  > ~/apps/llm-wiki/shared/data/Inbox/raw_clips/gemini-test.txt

cd ~/apps/llm-wiki/current
LLM_WIKI_ENV_FILE=~/apps/llm-wiki/shared/.env \
LLM_WIKI_ROOT=~/apps/llm-wiki/shared/data \
~/.dotnet/dotnet LlmWiki.Cli.dll process-once
```

확인 항목:
- [ ] 로그에 `[Gemini] calling for gemini-test.txt` 출력
- [ ] `gemini_calls=1` 로그 출력
- [ ] `fallbacks=0` 또는 파싱 실패 시 `fallbacks=1`
- [ ] `data/Notes/References/` 에 `.md` 파일 생성
- [ ] 생성된 노트의 frontmatter `status: "processed"` (성공) 또는 `"fallback"` (파싱 실패)
- [ ] 원본 파일이 `data/Inbox/processed/YYYY-MM-DD/` 로 이동

---

## 7. watch 검증

```bash
cd ~/apps/llm-wiki/current
LLM_WIKI_ENV_FILE=~/apps/llm-wiki/shared/.env \
LLM_WIKI_ROOT=~/apps/llm-wiki/shared/data \
~/.dotnet/dotnet LlmWiki.Cli.dll watch
```

다른 터미널에서:
```bash
echo "Watch mode test content." \
  > ~/apps/llm-wiki/shared/data/Inbox/raw_clips/watch-test.txt
```

확인 항목:
- [x] `[Watcher] Detected:` 로그 출력 ← **통과 (2026-05-19)**
- [x] `[Watcher] Debounced:` 로그 출력 (macOS 중복 이벤트 억제 확인) ← **통과**
- [x] Gemini 호출 후 노트 생성 확인 ← **통과**
- [x] 원본 파일 `processed/YYYY-MM-DD/` 이동 확인 ← **통과**
- [ ] Ctrl+C 로 정상 종료 (별도 확인 불필요)

watcher 안정화 사항:
- debounce 1500ms: 동일 파일 중복 이벤트 억제
- `processed/`, `failed/` 경로 이벤트 자동 무시
- 파일 안정성 확인 후 처리 (file-ready check)

---

## 8. 실패 케이스 검증

### 중복 파일명 테스트

```bash
echo "https://a.com" > data/Inbox/links/dup.txt
dotnet run --project src/LlmWiki.Cli -- process-once

echo "https://b.com" > data/Inbox/links/dup.txt
dotnet run --project src/LlmWiki.Cli -- process-once
```

확인 항목:
- [ ] `data/Inbox/processed/YYYY-MM-DD/` 에 `dup.txt` 와 `dup-1.txt` 존재
- [ ] 기존 파일 덮어쓰기 없음

---

## 9. 결과 기록

검증 완료 후 `reports/mac_mini_validation_report.md` 작성:

```markdown
# Mac mini Validation Report

날짜:
환경: Mac mini (Apple Silicon / Intel)
.NET SDK 버전:
GEMINI_MODEL:

## 통과 항목

## 실패 항목

## 비고
```
