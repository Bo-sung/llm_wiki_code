# Quartz Static Port Update Report

## 1. 수행 결과

- 완료:
  - plist template 포트 8081 → 8080
  - install 스크립트 URL 안내 갱신 (내부/외부 분리)
  - status 스크립트 HTTP check URL 8081 → 8080, 포트포워딩 안내 추가
  - serve 스크립트 static 기본 포트 8081 → 8080
  - QUARTZ_STATIC_HOSTING.md 포트 정책 전면 갱신
  - DECISIONS.md 포트 정보 추가
  - syntax check 4/4 통과, dotnet test 108/108 통과

---

## 2. 변경 파일

| 파일 | 변경 내용 |
|---|---|
| `scripts/mac/com.llmwiki.quartz-static.plist.template` | `8081` → `8080` |
| `scripts/mac/install-launchd-quartz-static.sh` | URL 안내: 내부 8080 / 외부 8081 포트포워딩 |
| `scripts/mac/status-launchd-quartz-static.sh` | HTTP check `8081` → `8080`, 포트포워딩 안내 추가 |
| `scripts/mac/serve-quartz-experiment.sh` | static 기본 포트 `8081` → `8080` |
| `docs/quartz/QUARTZ_STATIC_HOSTING.md` | 내부 8080 / 외부 8081 분리, 구조 다이어그램 갱신 |
| `docs/DECISIONS.md` | 포트 정책 1줄 추가 |

---

## 3. 변경된 포트 정책

| 항목 | 값 |
|---|---|
| 내부 포트 | 8080 |
| 외부 포트 | 8081 |
| 포트포워딩 | 공유기 외부 8081 → Mac mini 내부 8080 |

---

## 4. 테스트 결과

```
스크립트 syntax check (bash -n): OK (4/4)
dotnet build: 경고 0, 오류 0
dotnet test:  통과 108, 실패 0
```

---

## 5. Mac mini 재설치 명령

```bash
cd /Users/boseong/apps/llm-wiki/current
bash scripts/mac/build-quartz-experiment.sh
bash scripts/mac/uninstall-launchd-quartz-static.sh
bash scripts/mac/install-launchd-quartz-static.sh
bash scripts/mac/status-launchd-quartz-static.sh
```

---

## 6. 검증 URL

```text
Internal: http://127.0.0.1:8080/
External: http://8eh1ndy0u.iptime.org:8081/
```
