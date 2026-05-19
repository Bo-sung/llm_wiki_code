# Quartz Experiment Planning Report

## 1. 수행 결과

- 완료:
  - Git repo 초기화, .gitignore/.gitattributes 정비, 민감정보 점검
  - 1차 commit: Capture API + Firefox Extension + 전체 소스 (124 files)
  - Content Contract 문서 4종 작성
  - Quartz 실험 문서 3종 작성
  - Quartz 스크립트 3종 작성 (syntax checked)
  - 2차 commit: Quartz 계획 문서 + 스크립트 (10 files)
- 부분 완료:
  - remote push 불가 (remote 미설정)
- 미완료:
  - Quartz 실제 실행 (Mac mini Node 확인 후 수행)
  - DECISIONS.md 갱신 (기존 파일 구조 확인 필요)

---

## 2. Git 저장 결과

| 항목 | 결과 |
|---|---|
| pre-commit status 확인 | `.env`, `bin/`, `obj/`, `artifacts/`, `data/` 모두 gitignore 처리 확인 |
| secret scan | `AIza...`, 실제 API 키 패턴 없음. 스캔 일치 항목은 모두 placeholder 또는 `.venv/` (gitignored) 내 문서 텍스트 |
| 1차 commit | `1a2c1de` — 124 files, 9319 insertions |
| 2차 commit | `05674b8` — 10 files, 784 insertions |
| push | remote 미설정 — local commit 완료 상태 |

---

## 3. 생성/수정 파일

| 파일 | 변경 내용 |
|---|---|
| `.gitignore` | 전면 정비 — `.env`, `artifacts/`, `data/`, `shared/`, `bin/`, `obj/` 등 |
| `.gitattributes` | 신규 — `* text=auto eol=lf` |
| `docs/wiki/CONTENT_CONTRACT.md` | 신규 |
| `docs/wiki/FRONTMATTER_SCHEMA.md` | 신규 |
| `docs/wiki/LINKING_RULES.md` | 신규 |
| `docs/wiki/RENDERING_COMPATIBILITY.md` | 신규 |
| `docs/quartz/QUARTZ_EXPERIMENT_PLAN.md` | 신규 |
| `docs/quartz/QUARTZ_SETUP_GUIDE.md` | 신규 |
| `docs/quartz/QUARTZ_MAUI_COMPATIBILITY.md` | 신규 |
| `scripts/mac/setup-quartz-experiment.sh` | 신규 |
| `scripts/mac/build-quartz-experiment.sh` | 신규 |
| `scripts/mac/serve-quartz-experiment.sh` | 신규 |

---

## 4. Quartz 실험 결정

| 항목 | 결정 |
|---|---|
| Quartz 사용 목적 | public-vault 웹 뷰어 실험 — 최종 종속 대상 아님 |
| 대상 vault | `~/apps/llm-wiki/public-vault` |
| MAUI 호환 방식 | Markdown content contract 공유 (코드 공유 없음) |
| 권장 구조 | 후보 B: 별도 `quartz-site/`, `contentDir`로 public-vault 지정 |

---

## 5. Content Contract 요약

- Markdown 원본은 특정 렌더러에 종속되지 않는다
- YAML frontmatter 필수: `title`, `created`, `visibility`, `publishable`, `review_status`
- Wikilink: `[[Note Title]]`, `[[Note Title|Alias]]`만 허용
- Mermaid: fenced code block만 사용
- 절대 로컬 경로 링크 금지
- `Inbox/`, `System/private/`, `.env` vault 포함 금지

---

## 6. 테스트 결과

```
dotnet build: 경고 0, 오류 0
dotnet test:  통과 108, 실패 0, 건너뜀 0
스크립트 syntax check (bash -n): OK (3/3)
```

---

## 7. 실제 Mac mini 검증 필요

| 항목 | 상태 |
|---|---|
| Node/npm 설치 확인 | 미검증 |
| `setup-quartz-experiment.sh` 실행 | 미검증 |
| Quartz build | 미검증 |
| Quartz serve (localhost:8080) | 미검증 |
| 모바일 브라우저 접속 확인 | 미검증 |
| Mermaid 렌더링 확인 | 미검증 |
| Wikilink 동작 확인 | 미검증 |

---

## 8. 다음 작업 후보

1. Mac mini에서 Node 설치 확인 후 `setup-quartz-experiment.sh` 실행
2. Git remote 추가 후 push
3. Quartz 실험 결과에 따라 GitHub Pages / Cloudflare Pages 연결 검토
