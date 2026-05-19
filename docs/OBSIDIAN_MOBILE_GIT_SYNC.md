# Obsidian Mobile Git Sync

공개용 vault를 모바일 Obsidian에서 열거나 동기화하는 방법.

**현재 상태**: 미구현. 이 문서는 계획 및 방향만 기록한다.

---

## 전제

- `public-vault`가 GitHub Public Repo로 운영 중
- `~/apps/llm-wiki/public-vault/` 초기화 완료 (deploy-to-mac.ps1 `-SetupPublicVault` 실행)
- Mac mini에서 git push 인증 완료

---

## 모바일 접근 방식 후보

| 방식 | 특징 | 현재 상태 |
|---|---|---|
| Obsidian Git 플러그인 | iOS/Android에서 Git clone + pull/push | 미검토 |
| Working Copy (iOS) | iOS Git 클라이언트, Obsidian 연동 가능 | 미검토 |
| iCloud Drive 동기화 | vault를 iCloud에 두고 Mac/iOS 공유 | public-vault 분리 필요 |
| GitHub 웹 뷰어 | 읽기 전용, 설정 불필요 | 즉시 가능 |

---

## 수동 동기화 흐름 (현재 권장)

1. Mac mini에서 노트를 `public-vault`에 복사
2. `git add . && git commit -m "update" && git push`
3. 모바일에서 GitHub 웹 또는 Working Copy로 확인

자동화는 launchd/watch 안정화 이후 검토한다.

---

## 자동 export 정책

자동 export는 초기 제외한다.

이유:
- 공개할 노트 선별 기준 미확정
- `shared/data` 전체 공개 금지
- 자동 commit/push 정책 미결정

구현 시 검토할 방식:
- `status: public` frontmatter 태그 기준 필터링
- `process-once` 완료 후 특정 카테고리를 public-vault로 복사하는 후처리 단계

---

## 미결정 사항

| 항목 | 현재 상태 |
|---|---|
| 모바일 Git 클라이언트 선택 | 미결정 |
| 자동 export 기준 (frontmatter 필터 등) | 미결정 |
| public-vault 업데이트 자동화 | 미결정 |
| iCloud vs Git 동기화 선택 | 미결정 |
