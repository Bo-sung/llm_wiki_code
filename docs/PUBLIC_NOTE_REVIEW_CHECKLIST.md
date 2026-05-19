# Public Note Review Checklist

공개 vault에 commit하기 전에 다음 항목을 확인한다.

---

## 공개 전 필수 확인

- [ ] API 키, 토큰, 비밀번호가 없는가
- [ ] 개인 이메일, 전화번호, 주소가 없는가
- [ ] 로컬 절대경로가 공개되어도 괜찮은가
- [ ] 비공개 프로젝트명이나 내부 정보가 없는가
- [ ] 원문 전문을 그대로 복사하지 않았는가
- [ ] 저작권 있는 자료를 과도하게 포함하지 않았는가
- [ ] 테스트 노트가 아닌가
- [ ] 공개해도 되는 주제인가
- [ ] frontmatter에 private 표시가 남아 있지 않은가
- [ ] 출처 URL이 필요한 경우 포함되어 있는가

---

## 공개 금지 예시

- Inbox 원본 파일
- `processed/`, `failed/` 원본
- Sources 원문 전문
- API 응답 로그
- `.env` 파일
- 비공개 회의록
- 개인 작업 로그
- 테스트용으로 만든 노트 (제목에 "test" 포함된 것 확인)

---

## 공개 가능 예시

- 직접 정리한 요약 노트
- 공개 자료 기반의 짧은 요약과 해설
- 개인 의견이지만 공개해도 되는 글
- LLM Wiki 구축 과정 기록

---

## export 후 확인 명령

```bash
cd ~/apps/llm-wiki/public-vault
git diff Notes/References/파일명.md   # 변경 내용 확인
git status                            # 전체 변경 목록 확인
```

---

## 확인 후 commit

```bash
cd ~/apps/llm-wiki/public-vault
git add Notes/References/파일명.md
git commit -m "Publish note: 파일명"
git push
```
