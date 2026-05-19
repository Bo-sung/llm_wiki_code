# Public Export Script Report

작성일: 2026-05-19

## 1. 수행 결과

- **완료**: `export-public-note.sh` 작성, bash -n 검사, 문서 3종 신규, `PUBLIC_VAULT_POLICY.md`·`DECISIONS.md` 갱신, dotnet build·test 통과
- **부분 완료**: 없음
- **미완료**: Mac mini 실환경 실행 검증 (사용자가 실행 필요)

## 2. 생성/수정 파일

| 파일 | 변경 내용 |
|---|---|
| [`scripts/mac/export-public-note.sh`](../scripts/mac/export-public-note.sh) | 신규 — 경로 검증, 허용/금지 필터, 복사, git status/diff 출력, commit 안내 |
| [`docs/PUBLIC_NOTE_REVIEW_CHECKLIST.md`](PUBLIC_NOTE_REVIEW_CHECKLIST.md) | 신규 — 공개 전 체크리스트 |
| [`docs/PUBLIC_EXPORT_WORKFLOW.md`](PUBLIC_EXPORT_WORKFLOW.md) | 신규 — 전체 export 흐름 절차 문서 |
| [`docs/PUBLIC_VAULT_POLICY.md`](PUBLIC_VAULT_POLICY.md) | `export-public-note.sh` 사용 방식 반영 |
| [`docs/DECISIONS.md`](DECISIONS.md) | 공개 노트 반영 방식 결정 기록 추가 |

## 3. export 정책

| 항목 | 정책 |
|---|---|
| 운영용 vault | `/Users/boseong/apps/llm-wiki/shared/data` (기본값, 환경변수로 오버라이드 가능) |
| 공개용 vault | `public-vault.env`의 `PUBLIC_VAULT_PATH` 또는 기본값 |
| 허용 경로 | `Notes/`, `Index/`, `Templates/`, `System/public/` |
| 금지 경로 | `Inbox/`, `Sources/`, `System/private/`, `logs/`, `.env`, `*.log`, `*.tmp`, `.DS_Store` |
| 자동 git add | 없음 |
| 자동 commit | 없음 |
| 자동 push | 없음 |

## 4. 사용 예시

```bash
# 절대 경로
bash ~/apps/llm-wiki/current/scripts/mac/export-public-note.sh \
  /Users/boseong/apps/llm-wiki/shared/data/Notes/References/2026-05-19-example.md

# 상대 경로 (OPERATING_VAULT 기준)
bash ~/apps/llm-wiki/current/scripts/mac/export-public-note.sh \
  Notes/References/2026-05-19-example.md

# 복사 후 수동 commit
cd ~/apps/llm-wiki/public-vault
git add Notes/References/2026-05-19-example.md
git commit -m "Publish note: 2026-05-19-example"
git push
```

## 5. 테스트 결과

```text
bash -n scripts/mac/export-public-note.sh: Syntax OK
dotnet build: 경고 0개 오류 0개
dotnet test:  66/66 통과
```

## 6. 남은 리스크

- Mac mini 실환경에서 `export-public-note.sh` 실행 검증 미수행. (추정: bash 문법은 검증됨)
- `git diff` 출력은 파일이 이미 공개 vault에 있을 때만 내용이 나온다. 새 파일이면 `(New untracked file)` 안내만 출력.
- `PUBLIC_VAULT_PATH`가 `~` 홈 축약 형태로 설정된 경우 bash에서 확장되지 않아 경로 오류 가능. (추정: `config/public-vault.env`에 절대 경로로 저장 권장)
- 민감정보 자동 필터 미구현 — 사람이 직접 확인해야 함.

## 7. 다음 작업 후보

1. Mac mini에서 `export-public-note.sh` 실행 검증
2. `public-vault.env`의 `PUBLIC_VAULT_PATH` `~` 확장 처리 (필요 시)
3. frontmatter `status: public` 기준 export 대상 자동 필터링 (검토 단계)
