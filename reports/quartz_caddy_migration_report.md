# Quartz Caddy Migration Report

## 1. 수행 결과

- 완료:
  - Python http.server → Caddy 전환
  - Caddyfile.quartz-static 작성 (try_files 규칙 포함)
  - plist template: `__CADDY_PATH__` placeholder, caddy run 방식으로 교체
  - install 스크립트: caddy 존재 확인, path 치환, Caddyfile 배포
  - status 스크립트: extensionless URL 검증 추가
  - serve 스크립트: static 모드 → caddy, dev 모드 유지
  - QUARTZ_STATIC_HOSTING.md 전면 갱신
  - DECISIONS.md 갱신
  - syntax check 4/4, dotnet test 108/108

---

## 2. 문제와 원인

| 증상 | 원인 |
|---|---|
| 노트 링크 클릭 시 Not Found | Quartz가 `/Notes/References/note`(extensionless) URL 생성, Python http.server가 `.html` fallback 불가 |
| root `/`는 정상 | `index.html`은 디렉터리 서빙으로 동작 |

---

## 3. 변경 파일

| 파일 | 변경 내용 |
|---|---|
| `scripts/mac/Caddyfile.quartz-static` | 신규 — `try_files {path} {path}.html {path}/index.html` |
| `scripts/mac/com.llmwiki.quartz-static.plist.template` | python3 → caddy, `__CADDY_PATH__` placeholder |
| `scripts/mac/install-launchd-quartz-static.sh` | caddy 확인, sed 치환, Caddyfile 배포 추가 |
| `scripts/mac/status-launchd-quartz-static.sh` | extensionless URL curl 검증 추가 |
| `scripts/mac/serve-quartz-experiment.sh` | static 모드: python3 → caddy (임시 Caddyfile 생성) |
| `docs/quartz/QUARTZ_STATIC_HOSTING.md` | 구조 다이어그램, 서버 설명 갱신 |
| `docs/DECISIONS.md` | Caddy 전환 결정 기록 |

---

## 4. Caddyfile 내용

```
:8080 {
    root * /Users/boseong/apps/llm-wiki/quartz-site/public
    file_server
    try_files {path} {path}.html {path}/index.html
}
```

---

## 5. 테스트 결과

```
스크립트 syntax check (bash -n): OK (4/4)
dotnet build: 경고 0, 오류 0
dotnet test:  통과 108, 실패 0
```

---

## 6. Mac mini 재설치 명령

```bash
# Caddy 설치 (미설치 시)
brew install caddy

# 재설치
cd /Users/boseong/apps/llm-wiki/current
bash scripts/mac/uninstall-launchd-quartz-static.sh
bash scripts/mac/build-quartz-experiment.sh
bash scripts/mac/install-launchd-quartz-static.sh
bash scripts/mac/status-launchd-quartz-static.sh
```

---

## 7. 검증 URL

```bash
# Mac mini 내부에서
curl -I http://127.0.0.1:8080/
curl -I http://127.0.0.1:8080/Notes/
curl -I http://127.0.0.1:8080/Notes/References/2026-05-19-gemini-25-flash
```

기대 결과: 세 URL 모두 HTTP/1.1 200 OK

외부:
```
http://8eh1ndy0u.iptime.org:8081/Notes/References/2026-05-19-gemini-25-flash
```

---

## 8. 남은 작업

| 항목 | 상태 |
|---|---|
| Mac mini Caddy 설치 및 launchd 재설치 | 미검증 |
| extensionless URL 200 확인 | 미검증 |
| 외부 접속 확인 | 미검증 |
