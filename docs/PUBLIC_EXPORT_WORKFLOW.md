# Public Export Workflow

운영용 vault에서 공개 가능한 노트만 선별해 public-vault로 복사하고 GitHub public repo에 수동 push하는 절차.

---

## 목적

- 운영용 `shared/data` 전체를 공개하지 않는다.
- 사람이 내용을 검수한 노트만 선택적으로 공개한다.
- 자동 push는 초기 제외한다.

---

## 기본 흐름

```text
1. 운영용 vault에서 Gemini가 생성한 노트 확인
   shared/data/Notes/References/YYYY-MM-DD-*.md

2. 노트 내용 검수 (PUBLIC_NOTE_REVIEW_CHECKLIST.md 참조)

3. export-public-note.sh 실행

4. public-vault에서 git diff/status로 변경 내용 확인

5. 수동 git add / commit / push

6. GitHub 웹에서 공개 결과 확인
```

---

## 스크립트 사용법

```bash
# 절대 경로
bash ~/apps/llm-wiki/current/scripts/mac/export-public-note.sh \
  /Users/boseong/apps/llm-wiki/shared/data/Notes/References/2026-05-19-example.md

# 상대 경로 (OPERATING_VAULT 기준)
bash ~/apps/llm-wiki/current/scripts/mac/export-public-note.sh \
  Notes/References/2026-05-19-example.md
```

스크립트가 수행하는 것:
- 입력 경로 검증
- 금지 경로 거부 (`Inbox/`, `Sources/`, `System/private/` 등)
- 허용 경로 확인 (`Notes/`, `Index/`, `Templates/`, `System/public/`)
- 상대 경로 유지하며 public-vault에 복사
- 대상 디렉터리 자동 생성
- `git status`, `git diff` 출력
- 수동 commit 명령 안내
- 공개 전 검수 체크리스트 안내

스크립트가 수행하지 않는 것:
- `git add`
- `git commit`
- `git push`

---

## 복사 후 수동 commit

```bash
cd ~/apps/llm-wiki/public-vault

# 변경 내용 확인
git status
git diff Notes/References/2026-05-19-example.md

# 검수 완료 후 commit
git add Notes/References/2026-05-19-example.md
git commit -m "Publish note: 2026-05-19-example"
git push
```

---

## 허용 / 금지 경로

| 구분 | 경로 |
|---|---|
| 허용 | `Notes/`, `Index/`, `Templates/`, `System/public/` |
| 금지 | `Inbox/`, `Sources/`, `System/private/`, `logs/`, `.env`, `*.log`, `*.tmp` |

---

## 금지 명령

운영용 `shared/data` 전체를 public-vault로 복사하지 않는다.

````bash
# 다음 명령은 절대 실행하지 않는다
cp -r ~/apps/llm-wiki/shared/data/* ~/apps/llm-wiki/public-vault/
````

---

## 모바일 Obsidian

public-vault는 GitHub public repo로 운영한다.
모바일 Obsidian은 public repo를 pull해서 읽기 중심으로 사용한다.
초기에는 모바일에서 편집하지 않는다.

자세한 계획: [`docs/OBSIDIAN_MOBILE_GIT_SYNC.md`](OBSIDIAN_MOBILE_GIT_SYNC.md)

---

## 참조

- 검수 체크리스트: [`docs/PUBLIC_NOTE_REVIEW_CHECKLIST.md`](PUBLIC_NOTE_REVIEW_CHECKLIST.md)
- public vault 정책: [`docs/PUBLIC_VAULT_POLICY.md`](PUBLIC_VAULT_POLICY.md)
- export 스크립트: `scripts/mac/export-public-note.sh`
