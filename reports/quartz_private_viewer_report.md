# Quartz Private Viewer Report

## 1. 수행 결과

- 완료:
  - `setup-quartz-experiment.sh` private viewer 기준으로 전면 재작성
  - `build-quartz-experiment.sh` private content 기준으로 갱신
  - `QUARTZ_PRIVATE_VIEWER.md` 신규 작성
  - `QUARTZ_STATIC_HOSTING.md` content source 갱신
  - `DECISIONS.md` private viewer 결정 기록
  - syntax check 2/2, dotnet test 108/108
- 미완료:
  - Mac mini 실제 재설치 검증

---

## 2. 구조 변경

| 항목 | 이전 | 변경 |
|---|---|---|
| Quartz content source | `public-vault` | `quartz-private-content` |
| 노출 데이터 | 공개 검수 노트 | 운영용 `Notes`, `Index` |
| Inbox/Sources 노출 | 없음 (public-vault에 없었음) | 명시적 금지 (setup이 연결 안 함) |
| Auth | 없음 | 없음 (후속 TODO) |

---

## 3. 생성/수정 파일

| 파일 | 변경 내용 |
|---|---|
| `scripts/mac/setup-quartz-experiment.sh` | private-content 구성, Notes/Index symlink, content 재연결 |
| `scripts/mac/build-quartz-experiment.sh` | private-content 기준 sanity check |
| `docs/quartz/QUARTZ_PRIVATE_VIEWER.md` | 신규 |
| `docs/quartz/QUARTZ_STATIC_HOSTING.md` | content source 갱신 |
| `docs/DECISIONS.md` | private viewer 결정 추가 |

---

## 4. 노출 정책

| 항목 | 정책 |
|---|---|
| `shared/data/Notes` | 노출 (Notes symlink) |
| `shared/data/Index` | 노출 (Index symlink, 있는 경우) |
| `shared/data/Inbox` | 금지 |
| `shared/data/Sources` | 금지 |
| `shared/data/System` | 금지 |
| `shared/data` 전체 | 절대 금지 |
| `public-vault` | GitHub 공개용 유지, Private Viewer 미사용 |

---

## 5. Mac mini 재설치 명령

```bash
cd /Users/boseong/apps/llm-wiki/current

# Caddy 설치 (미설치 시)
brew install caddy

# private content + quartz-site 재구성
bash scripts/mac/setup-quartz-experiment.sh

# Quartz 빌드
bash scripts/mac/build-quartz-experiment.sh

# launchd 재등록
bash scripts/mac/uninstall-launchd-quartz-static.sh
bash scripts/mac/install-launchd-quartz-static.sh
bash scripts/mac/status-launchd-quartz-static.sh
```

---

## 6. 검증 URL

```text
Internal: http://127.0.0.1:8080/
External: http://8eh1ndy0u.iptime.org:8081/
Notes:    http://8eh1ndy0u.iptime.org:8081/Notes/
Note:     http://8eh1ndy0u.iptime.org:8081/Notes/References/<slug>
```

---

## 7. 후속 보안 작업

| 항목 | 우선순위 |
|---|---|
| Caddy `basic_auth` | 1순위 (즉시 추가 가능) |
| Tailscale 접근 제한 | 2순위 |
| HTTPS (Let's Encrypt) | 3순위 |
| Cloudflare Access | 4순위 |
| 자체 계정 시스템 | 장기 |
