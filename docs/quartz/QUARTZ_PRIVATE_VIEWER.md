# Quartz Private Viewer

## 목적

GitHub public repo(`public-vault`)와 별개로 운영용 Notes를 외부에서 웹으로 볼 수 있는 뷰어.
Capture API + watcher가 생성한 노트를 브라우저에서 바로 확인한다.

---

## 현재 접근 정책

현재는 인증 없이 외부 접속 허용.

이유:
- 현재 private 노트에 민감정보 없음
- 개발 PC와 Mac mini가 같은 로컬 네트워크가 아님 (원격 접속 필요)
- GitHub public-vault를 거치지 않고 운영용 노트를 바로 확인해야 함

---

## 후속 보안 TODO

우선순위 순:

1. Caddy `basic_auth` — 단순 사용자명/비밀번호 (즉시 추가 가능)
2. Tailscale 접근 제한 — Caddy를 Tailscale IP에만 바인딩
3. HTTPS — Caddy 자동 TLS (Let's Encrypt)
4. Cloudflare Access — Zero Trust SSO
5. 자체 계정 시스템 — 장기 과제

---

## 데이터 소스 정책

| 디렉터리 | 정책 |
|---|---|
| `shared/data/Notes` | 포함 (Notes symlink로 연결) |
| `shared/data/Index` | 포함 (Index symlink로 연결, 있는 경우) |
| `shared/data/Inbox` | **금지** |
| `shared/data/Sources` | **금지** |
| `shared/data/System` | **금지** |
| `shared/data` 전체 | **절대 금지** |
| `public-vault` | GitHub 공개용으로 유지, Private Viewer에 미사용 |

---

## 디렉터리 구조

```
shared/data/
  Notes/          ← 실제 노트 (watcher 생성)
  Index/          ← 인덱스 (선택)
  Inbox/          ← 제외
  ...

quartz-private-content/
  index.md
  Notes  -> shared/data/Notes    ← symlink
  Index  -> shared/data/Index    ← symlink (있는 경우)

quartz-site/
  content -> quartz-private-content  ← symlink
  public/  <- npx quartz build 출력

Caddy (launchd)
  root = quartz-site/public
  port = 8080 (내부) / 8081 (외부 포트포워딩)
  try_files {path} {path}.html {path}/index.html
```

---

## 접속

| 위치 | URL |
|---|---|
| Mac mini 내부 | `http://127.0.0.1:8080/` |
| 외부 (포트포워딩) | `http://8eh1ndy0u.iptime.org:8081/` |
| LAN | `http://<mac-lan-ip>:8080/` |
| Tailscale | `http://<tailscale-ip>:8080/` |

---

## 설치

```bash
# Caddy 설치 (미설치 시)
brew install caddy

# 1. Private content 구성 + quartz-site 설정
bash scripts/mac/setup-quartz-experiment.sh

# 2. Quartz 빌드
bash scripts/mac/build-quartz-experiment.sh

# 3. launchd static server 등록
bash scripts/mac/install-launchd-quartz-static.sh

# 4. 상태 확인
bash scripts/mac/status-launchd-quartz-static.sh
```

---

## 새 노트 반영

Capture API 또는 watcher가 `shared/data/Notes`에 노트를 생성한 후:

```bash
bash scripts/mac/build-quartz-experiment.sh
# Caddy가 즉시 새 파일을 서빙 (재시작 불필요)
```

---

## public-vault와의 구분

| 항목 | Quartz Private Viewer | public-vault |
|---|---|---|
| content source | `quartz-private-content` | `public-vault` |
| 데이터 | `shared/data/Notes` (전체) | 검수된 노트만 |
| 접근 | 현재 공개, 추후 인증 | GitHub public repo |
| 목적 | 운영용 개인 뷰어 | 공개 배포 |
