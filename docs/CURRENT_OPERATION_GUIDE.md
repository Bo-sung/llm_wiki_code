# Current Operation Guide

실사용 기준 운영 명령 모음. Mac mini에서 실행한다.

---

## 1. 새 메모 넣기

```bash
echo "정리할 내용이나 URL" \
  > /Users/boseong/apps/llm-wiki/shared/data/Inbox/raw_clips/example.txt
```

launchd watch가 실행 중이면 자동 처리된다. 수동 처리는 아래 참조.

---

## 2. 결과 확인

```bash
# 생성된 노트 목록
find /Users/boseong/apps/llm-wiki/shared/data/Notes/References \
  -type f -name "*.md" | sort

# 처리 완료된 원본 파일 확인
find /Users/boseong/apps/llm-wiki/shared/data/Inbox/processed \
  -type f | sort
```

---

## 3. launchd 상태 확인

```bash
cd /Users/boseong/apps/llm-wiki/current
bash scripts/mac/status-launchd-watch.sh
```

또는 직접:

```bash
launchctl print gui/$(id -u)/com.llmwiki.watch
tail -n 50 /Users/boseong/apps/llm-wiki/shared/logs/watch.out.log
tail -n 50 /Users/boseong/apps/llm-wiki/shared/logs/watch.err.log
```

---

## 4. 로그 확인

```bash
tail -n 100 /Users/boseong/apps/llm-wiki/shared/logs/watch.out.log
tail -n 100 /Users/boseong/apps/llm-wiki/shared/logs/watch.err.log
```

실시간 모니터링:

```bash
tail -f /Users/boseong/apps/llm-wiki/shared/logs/watch.out.log
```

---

## 5. 수동 처리 (launchd 없을 때)

```bash
cd /Users/boseong/apps/llm-wiki/current

LLM_WIKI_ENV_FILE=/Users/boseong/apps/llm-wiki/shared/.env \
LLM_WIKI_ROOT=/Users/boseong/apps/llm-wiki/shared/data \
/Users/boseong/.dotnet/dotnet LlmWiki.Cli.dll process-once
```

dry-run (파일 변경 없이 스캔만):

```bash
LLM_WIKI_ENV_FILE=/Users/boseong/apps/llm-wiki/shared/.env \
LLM_WIKI_ROOT=/Users/boseong/apps/llm-wiki/shared/data \
/Users/boseong/.dotnet/dotnet LlmWiki.Cli.dll process-once --dry-run
```

---

## 6. 공개 노트 export

```bash
cd /Users/boseong/apps/llm-wiki/current

bash scripts/mac/export-public-note.sh \
  Notes/References/YYYY-MM-DD-example.md
```

스크립트가 실행하는 것: 경로 검증 → 금지 경로 거부 → public-vault 복사 → git status/diff 출력 → commit 명령 안내.

스크립트가 실행하지 않는 것: git add / commit / push.

---

## 7. public-vault push

```bash
cd /Users/boseong/apps/llm-wiki/public-vault

git status
git diff Notes/References/YYYY-MM-DD-example.md

# 검수 완료 후
git add Notes/References/YYYY-MM-DD-example.md
git commit -m "Publish note: YYYY-MM-DD-example"
git push
```

---

## 8. launchd 재시작

설정 변경 후 재시작:

```bash
cd /Users/boseong/apps/llm-wiki/current
bash scripts/mac/uninstall-launchd-watch.sh
bash scripts/mac/install-launchd-watch.sh
```

---

## 9. 주의사항

- `shared/data` 전체를 `public-vault`로 복사하지 않는다.
- `Inbox/`, `Sources/`, `.env`, 로그 파일은 공개하지 않는다.
- 공개 전 [`docs/PUBLIC_NOTE_REVIEW_CHECKLIST.md`](PUBLIC_NOTE_REVIEW_CHECKLIST.md)를 확인한다.
- API 키는 채팅, 문서, commit 메시지에 넣지 않는다.
- Gemini API 키가 노출된 경우 즉시 재발급한다.

---

## 10. 관련 문서

| 문서 | 내용 |
|---|---|
| [`docs/DEPLOYMENT_WINDOWS_TO_MAC.md`](DEPLOYMENT_WINDOWS_TO_MAC.md) | 배포 절차 |
| [`docs/LAUNCHD_WATCH.md`](LAUNCHD_WATCH.md) | launchd 설치/제거/상태 |
| [`docs/PUBLIC_EXPORT_WORKFLOW.md`](PUBLIC_EXPORT_WORKFLOW.md) | export 흐름 전체 절차 |
| [`docs/PUBLIC_NOTE_REVIEW_CHECKLIST.md`](PUBLIC_NOTE_REVIEW_CHECKLIST.md) | 공개 전 검수 체크리스트 |
| [`docs/PUBLIC_VAULT_POLICY.md`](PUBLIC_VAULT_POLICY.md) | vault 분리 정책 |
| [`reports/system_validation_report.md`](../reports/system_validation_report.md) | 전체 시스템 검증 결과 |
