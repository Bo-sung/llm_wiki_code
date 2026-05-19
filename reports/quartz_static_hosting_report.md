# Quartz Static Hosting Report

## 1. 수행 결과

- 완료:
  - launchd static server 스크립트 4종 생성
  - build 스크립트 갱신 (public 디렉터리 존재 확인 추가)
  - serve 스크립트 갱신 (static/dev 모드 분리)
  - `QUARTZ_STATIC_HOSTING.md` 문서 작성
  - `DECISIONS.md` 갱신 (Quartz 운영 결정 기록)
  - syntax check 5/5 통과, dotnet test 108/108 통과
- 부분 완료:
  - `QUARTZ_SETUP_GUIDE.md` 갱신 미수행 (기존 내용으로 충분)
- 미완료:
  - Mac mini launchd 설치 실제 검증
  - 외부 접속 확인
  - 자동 rebuild

---

## 2. 생성/수정 파일

| 파일 | 변경 내용 |
|---|---|
| `scripts/mac/com.llmwiki.quartz-static.plist.template` | 신규 — python3 http.server, 포트 8081, 0.0.0.0 |
| `scripts/mac/install-launchd-quartz-static.sh` | 신규 — bootstrap 방식 load |
| `scripts/mac/uninstall-launchd-quartz-static.sh` | 신규 — bootout |
| `scripts/mac/status-launchd-quartz-static.sh` | 신규 — launchctl + curl -I + tail log |
| `scripts/mac/build-quartz-experiment.sh` | 갱신 — public 디렉터리 존재 확인, 안내 메시지 추가 |
| `scripts/mac/serve-quartz-experiment.sh` | 갱신 — static/dev 모드, 포트/호스트 인자 |
| `docs/quartz/QUARTZ_STATIC_HOSTING.md` | 신규 |
| `docs/DECISIONS.md` | 갱신 — Quartz 운영 결정, Capture API 결정 추가 |

---

## 3. 운영 구조

| 항목 | 값 |
|---|---|
| content source | `~/apps/llm-wiki/public-vault` |
| quartz site | `~/apps/llm-wiki/quartz-site` |
| 정적 출력 | `~/apps/llm-wiki/quartz-site/public` |
| 서버 | `python3 -m http.server` |
| 포트 | 8081 |
| 바인드 | 0.0.0.0 |
| launchd label | `com.llmwiki.quartz-static` |
| stdout log | `shared/logs/quartz-static.out.log` |
| stderr log | `shared/logs/quartz-static.err.log` |

---

## 4. dev server vs static server

| 항목 | dev (`--serve`) | static (운영) |
|---|---|---|
| 터미널 세션 | 필요 | 불필요 |
| Mac 재시작 후 | 수동 재시작 | 자동 (KeepAlive) |
| 포트 | 8080 | 8081 |
| 서버 | npx quartz | python3 http.server |
| 사용 목적 | 개발/미리보기 | 운영 |

---

## 5. 핵심 명령

```bash
# 최초 설치
bash scripts/mac/build-quartz-experiment.sh
bash scripts/mac/install-launchd-quartz-static.sh

# 새 노트 반영 (서버 재시작 불필요)
bash scripts/mac/build-quartz-experiment.sh

# 상태 확인
bash scripts/mac/status-launchd-quartz-static.sh

# serve 스크립트 사용
bash scripts/mac/serve-quartz-experiment.sh                        # static, localhost:8081
bash scripts/mac/serve-quartz-experiment.sh static 8081 0.0.0.0   # static, LAN
bash scripts/mac/serve-quartz-experiment.sh dev                    # dev, localhost:8080
```

---

## 6. 테스트 결과

```
스크립트 syntax check (bash -n): OK (5/5)
dotnet build: 경고 0, 오류 0
dotnet test:  통과 108, 실패 0
```

---

## 7. 남은 작업

| 항목 | 상태 |
|---|---|
| Mac mini launchd 설치 | 미검증 |
| `curl -I http://127.0.0.1:8081/` 확인 | 미검증 |
| 외부(LAN/Tailscale) 접속 확인 | 미검증 |
| 자동 rebuild | 후속 |
| Mermaid/Wikilink/Search/Graph 검증 | 후속 |
