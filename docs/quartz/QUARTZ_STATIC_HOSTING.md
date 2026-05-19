# Quartz Static Hosting

## 목적

MAUI Reader 제작 전까지 Quartz를 운영용 웹 뷰어로 사용한다.
`npx quartz build --serve`는 개발 서버로 터미널 세션 종료 시 함께 종료된다.
운영 환경에서는 **정적 빌드 + launchd static server** 방식을 사용한다.

---

## 운영 구조

```
public-vault/Markdown
    ↓ npx quartz build
quartz-site/public/ (정적 HTML/CSS/JS)
    ↓ launchd com.llmwiki.quartz-static
python3 -m http.server 8081 --bind 0.0.0.0
    ↓
브라우저 http://<mac-ip>:8081/
```

| 항목 | 값 |
|---|---|
| content source | `~/apps/llm-wiki/public-vault` |
| quartz site | `~/apps/llm-wiki/quartz-site` |
| 정적 출력 | `~/apps/llm-wiki/quartz-site/public` |
| 서버 포트 | 8081 |
| 서버 바인드 | 0.0.0.0 (LAN/Tailscale 접근 가능) |
| launchd label | `com.llmwiki.quartz-static` |

---

## 왜 dev server를 쓰지 않는가

| 항목 | dev server (`--serve`) | static server (운영) |
|---|---|---|
| 터미널 세션 | 필요 | 불필요 (launchd 상시 실행) |
| Mac mini 재시작 후 | 수동 재시작 필요 | 자동 재시작 |
| 목적 | 개발/미리보기 | 운영 |
| 포트 | 8080 | 8081 |

---

## 최초 설치

```bash
# 1. Quartz 빌드
bash scripts/mac/build-quartz-experiment.sh

# 2. launchd 등록
bash scripts/mac/install-launchd-quartz-static.sh
```

---

## 상태 확인

```bash
bash scripts/mac/status-launchd-quartz-static.sh
```

또는 직접 확인:

```bash
launchctl list com.llmwiki.quartz-static
curl -I http://127.0.0.1:8081/
tail -20 ~/apps/llm-wiki/shared/logs/quartz-static.out.log
```

---

## 새 노트 반영

노트는 static server를 재시작하지 않고 rebuild만으로 반영된다.

```bash
# 1. public-vault에 노트 export
bash scripts/mac/export-public-note.sh Notes/References/example.md

# 2. public-vault commit/push
cd ~/apps/llm-wiki/public-vault
git add Notes/References/example.md
git commit -m "Publish note: example"
git push

# 3. Quartz rebuild
bash scripts/mac/build-quartz-experiment.sh

# 4. 브라우저 새로고침 (서버 재시작 불필요)
```

---

## 접속

| 접근 위치 | URL |
|---|---|
| Mac mini 내부 | `http://127.0.0.1:8081/` |
| LAN | `http://<mac-lan-ip>:8081/` |
| Tailscale | `http://<tailscale-ip>:8081/` |
| 포트포워딩 | `http://<공인IP>:<외부포트>/` |
| Windows (SSH 터널) | `ssh -L 8081:localhost:8081 boseong@<mac-ip>` 후 `http://localhost:8081/` |

---

## 재시작 / 제거

```bash
# 재시작
launchctl bootout gui/$(id -u) ~/Library/LaunchAgents/com.llmwiki.quartz-static.plist
launchctl bootstrap gui/$(id -u) ~/Library/LaunchAgents/com.llmwiki.quartz-static.plist

# 제거
bash scripts/mac/uninstall-launchd-quartz-static.sh
```

---

## 주의

- 운영용 `shared/data`를 Quartz content로 연결하지 않는다.
- `public-vault`만 Quartz content로 사용한다.
- 민감정보가 들어간 노트는 public-vault에 넣지 않는다.
- `quartz-site/public/`은 gitignore 대상이다 — build artifact.
- 자동 rebuild는 후속 작업으로 남겨둔다.
