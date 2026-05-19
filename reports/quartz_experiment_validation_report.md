# Quartz Experiment Validation Report

## 1. 수행 결과

- 완료:
  - Quartz v4.5.2 Mac mini 로컬 실행 검증
  - setup 스크립트: content symlink 생성, index.md 자동 생성 추가
  - build 스크립트: content symlink / index.md 사전 검증 추가
  - serve 스크립트: 포트/호스트 인자 지원, SSH 터널 안내 추가
  - QUARTZ_SETUP_GUIDE.md: 실제 성공 절차 및 성공 로그 기록
  - QUARTZ_EXPERIMENT_PLAN.md: 검증 결과 반영, 구조 확정
  - QUARTZ_MAUI_COMPATIBILITY.md: 검증된 디렉터리 구조 추가
  - 신규 보고서 작성
- 부분 완료:
  - Wikilink 동작 / Mermaid / Search / Graph 확인 미완료 (노트 수 4개)
- 미완료:
  - 모바일 브라우저 확인
  - GitHub Pages / Cloudflare Pages 연결

---

## 2. 확인된 문제와 처리

| 문제 | 원인 | 처리 |
|---|---|---|
| `Found 0 input files from content` | `quartz-site/content`가 public-vault와 연결되지 않음 | setup 스크립트에서 symlink 자동 생성 |
| `[404] /` | `public-vault/index.md` 없음 | setup 스크립트에서 기본 index.md 자동 생성 |
| `/README` 가 별도 페이지로 뜸 | README.md도 Markdown 파일로 포함됨 | 허용. index.md가 홈페이지임을 문서화 |

---

## 3. 성공 로그

```text
Quartz v4.5.2
Found 4 input files from `content`
Parsed 4 Markdown files
Emitted 33 files to `public`
Started a Quartz server listening at http://localhost:8080
[200] /
[200] /Notes/
[200] /Notes/References/2026-05-19-gemini-25-flash
[200] /Notes/References/2026-05-19-launchd-watch-test
[200] /README
```

---

## 4. 반영한 스크립트 변경

### setup-quartz-experiment.sh

| 변경 | 내용 |
|---|---|
| content symlink 로직 | symlink 존재/대상/빈 디렉터리/비어있지 않은 디렉터리 케이스 처리 |
| index.md 생성 | 없으면 기본 index.md 생성, 있으면 skip |
| contentDir MANUAL STEP 제거 | symlink 방식으로 대체 |

### build-quartz-experiment.sh

| 변경 | 내용 |
|---|---|
| symlink 존재 확인 | 없으면 setup 실행 안내 후 exit |
| index.md 존재 확인 | 없으면 exit |
| Notes/ 존재 확인 | 없으면 경고만 (vault가 비어있을 수 있음) |

### serve-quartz-experiment.sh

| 변경 | 내용 |
|---|---|
| 포트 인자 | `$1` (기본 8080) |
| 호스트 인자 | `$2` (기본 localhost, 0.0.0.0 지원) |
| SSH 터널 안내 | localhost 모드에서 외부 접속 방법 출력 |
| LAN IP 출력 | 0.0.0.0 모드에서 LAN IP 자동 표시 |

---

## 5. MAUI 호환성 영향

- public-vault는 순수 Markdown vault로 유지 — 영향 없음
- Quartz 설정은 quartz-site에 완전 분리 — vault 파일 오염 없음
- content symlink 방식으로 파일 복사 없음
- MAUI Reader는 `public-vault/`만 직접 읽으면 됨
- index.md는 표준 YAML frontmatter + 표준 Markdown — 모든 클라이언트 호환

---

## 6. 테스트 결과

```
스크립트 syntax check (bash -n): OK (3/3)
dotnet build: 경고 0, 오류 0
dotnet test:  통과 108, 실패 0
```

---

## 7. 다음 작업 후보

1. Mermaid 렌더링 확인 (노트에 Mermaid 포함 후 build/serve)
2. Wikilink 동작 확인 (상호 링크된 노트 2개 이상으로 테스트)
3. Graph view 유용성 평가
4. GitHub Pages 또는 Cloudflare Pages 연결 실험
