# 프로젝트 결정사항 (DECISIONS.md)

이 문서는 확정된 아키텍처 및 기술 결정사항을 기록한다.
변경 시 날짜와 이유를 함께 기록한다.

---

## 프로젝트 목적

개인 LLM 위키 시스템. 브라우저/모바일/수동 입력으로 들어온 URL 또는 텍스트를 Inbox에 저장하면, 로컬 프로세스가 Gemini API를 호출해 정리된 Markdown 노트를 생성한다.

---

## 역할 분리

| 역할 | 담당 |
|---|---|
| 프로젝트 코드 구현 | Claude Code |
| 정보 정리 / 요약 / 태깅 | Gemini API |
| 아키텍처 의사결정 | ChatGPT와 사용자 논의 후 확정 |

Claude Code는 아키텍처 의사결정을 임의로 확정하지 않는다. 결정이 필요한 항목은 `reports/` 보고서에 기록한다.

---

## 확정된 결정사항

### 언어: C# / .NET 8 LTS

결정일: 2026-05-19

이유:
- Mac mini 로컬 실행 환경에서 단일 실행 파일로 배포 가능
- Python 프로토타입은 아이디어 검증 완료 후 아카이브로 이동

### 실행 환경: Mac mini 로컬

초기 MVP는 클라우드 배포 없이 Mac mini 로컬에서만 실행한다.

### 저장 방식: Markdown 파일

DB를 사용하지 않는다. 모든 노트는 로컬 `.md` 파일로 저장한다.

### 초기 뷰어: Obsidian

생성된 노트는 Obsidian에서 바로 열 수 있는 frontmatter 포맷을 따른다.

### LLM 정리 엔진: Gemini API

API Key 방식 사용. OAuth는 초기 제외.

### 모델명 하드코딩 금지

`GEMINI_MODEL` 환경 변수를 통해 모델명을 주입한다.
`GEMINI_MODEL`이 비어 있으면 Gemini 호출을 수행하지 않고 fallback 노트를 생성한다.
코드에 특정 모델명을 기본값으로 하드코딩하지 않는다.

### Google AI Pro를 API 무제한 사용권으로 간주하지 않음

Google AI Pro 구독은 Gemini API의 무료/무제한 사용권이 아니다.
API 호출량은 보수적으로 관리한다:
- 대량 문서 자동 반복 처리 금지
- 자동 재시도 루프 금지
- Context Caching 초기 미구현
- 처리 대상 파일 수 및 Gemini 호출 여부를 항상 로그에 출력

### OAuth: 초기 제외

### DB: 초기 제외

파일 기반 파이프라인만 사용.

### 서버: 초기 제외

웹앱, REST API 서버 미구현.

### Context Caching: 초기 제외

### 자동 Git push: 초기 제외

백업 정책 확정 전까지 자동 push 미구현.

### 모바일 앱 / 브라우저 익스텐션: 초기 제외

### 처리 후 원본 파일 처리 정책 (2026-05-19 확정)

- 처리 성공: `data/Inbox/processed/YYYY-MM-DD/` 로 이동
- 처리 실패: `data/Inbox/failed/YYYY-MM-DD/` 로 이동
- 원본 삭제 금지
- 파일명 충돌 시 `-1`, `-2` 접미사로 처리

Gemini 미설정 / 호출 실패 / 응답 파싱 실패는 실패로 처리하지 않음.
fallback 노트를 생성하고 성공으로 처리함.

### .NET 버전 전환 계획

현재 MVP는 .NET 8 LTS 기준으로 구현한다.
장기 운영 시 .NET 10 LTS 전환을 검토한다.
전환 시점은 MVP 안정화 이후로 한다.

### Public Vault 분리 운영 (2026-05-19 확정)

- 운영용 vault (`shared/data`)와 공개용 vault (`public-vault`)를 분리한다.
- 공개용 vault는 GitHub Public Repo로 운영할 계획.
- `shared/data` 전체를 public repo로 만들지 않는다.
- 배포 스크립트(`-SetupPublicVault`)는 vault 디렉터리 초기화만 수행한다.
- Git 인증 정보(token, SSH key, password)는 배포 스크립트로 전달하지 않는다.
- 자동 push 초기 제외.
- 설정 파일: `~/apps/llm-wiki/shared/config/public-vault.env` (비밀 정보 없음)
- 공개 노트 반영 방식: 운영용 vault → `export-public-note.sh` → public-vault → 수동 commit/push
- 민감정보 자동 필터 초기 제외 — 사람이 검수 후 수동 export

참조: [`docs/PUBLIC_VAULT_POLICY.md`](PUBLIC_VAULT_POLICY.md), [`docs/PUBLIC_EXPORT_WORKFLOW.md`](PUBLIC_EXPORT_WORKFLOW.md)

### MVP 운영 가능 상태 달성 (2026-05-19)

다음 항목이 실환경에서 검증 완료되어 MVP 운영 가능 상태에 도달했다:

- Windows → Mac mini 배포
- Gemini 실제 호출 (gemini-2.5-flash)
- launchd 상시 watch
- public-vault GitHub push
- export-public-note.sh 수동 export 흐름

다음 큰 의사결정 주제는 **모바일 앱의 역할과 범위**다.
Obsidian + public-vault pull 방식을 먼저 검증하고, 추가 앱 개발 여부를 결정한다.

참조: [`reports/system_validation_report.md`](../reports/system_validation_report.md)

---

### Capture API + Browser Extension (2026-05-20)

- Capture API(ASP.NET Core Minimal API)를 1차 수집 UX로 채택한다.
- Firefox Extension이 주 브라우저 대상이다.
- CORS는 MVP에서 항상 AllowAny로 활성화한다 (Bearer Token 인증은 유지).
- 환경변수 `CAPTURE_API_CORS_MODE`는 문서화용으로 남기며, 미설정 시에도 CORS가 비활성화되지 않는다.

### Quartz 웹 뷰어 운영 (2026-05-20)

- 별도 MAUI Reader 제작 전까지 Quartz를 운영용 웹 뷰어로 사용한다.
- Quartz는 dev server(`--serve`)가 아니라 **정적 build + Caddy static server (launchd)** 방식으로 운영한다.
- Python `http.server`는 Quartz extensionless pretty URL을 처리하지 못해 Not Found 발생 — Caddy로 대체.
- 내부 포트 8080, 외부 접속 8081(공유기 포트포워딩: 외부 8081 → 내부 8080).
- Quartz는 public-vault viewer가 아니라 **운영용 private viewer**로 사용한다. 현재 단계에서는 인증 없이 외부 접속 허용.
- content source는 `shared/data` 전체가 아닌 `quartz-private-content` (Notes/Index symlink만 포함)로 필터링한다.
- `Inbox`, `Sources`, `System`, `.env`는 절대 Quartz content에 포함하지 않는다.
- `public-vault`는 GitHub 공개용으로 유지한다. Quartz Private Viewer content로 사용하지 않는다.
- 접근 제어는 후속 작업: Caddy basic_auth → Tailscale → HTTPS → Cloudflare Access.
- 새 노트 반영은 초기에는 수동 `quartz build`로 처리한다. 자동 rebuild는 후속 작업.
- Quartz는 최종 종속 대상이 아니다. MAUI Reader와의 호환은 Markdown content contract로 보장한다.

참조: [`docs/quartz/QUARTZ_STATIC_HOSTING.md`](quartz/QUARTZ_STATIC_HOSTING.md)

---

## 미결정 사항

| 항목 | 현재 상태 |
|---|---|
| 카테고리 자동 분류 | 미결정. 현재 모든 노트 `References/`에 저장 |
| 모바일 Git 클라이언트 선택 | 미결정. 후보: Obsidian Git 플러그인, Working Copy |
| public vault 자동 export 기준 | 미결정. frontmatter 필터링 방식 검토 예정 |
| GitHub 백업 자동화 | 미결정. 후보: 수동 push, launchd cron |

---

## Claude Code 보고 규칙

- 응답은 건조하게 작성
- 미사여구, 칭찬, 과장 금지
- 수행한 작업, 생성/수정 파일, 실행 명령, 테스트 결과, 리스크, 결정 필요 질문만 포함
- 임의 의사결정 확정 금지
