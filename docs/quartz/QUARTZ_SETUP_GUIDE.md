# Quartz Setup Guide

## 전제 조건

| 항목 | 확인 명령 |
|---|---|
| Node.js 20+ | `node --version` |
| npm | `npm --version` |
| public-vault 존재 | `ls ~/apps/llm-wiki/public-vault/Notes/` |
| Git | `git --version` |

Node가 없으면:

```bash
# Mac mini — Homebrew 경유
brew install node

# 또는 nvm
curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.39.7/install.sh | bash
nvm install 20
nvm use 20
```

---

## Mac mini 실험 절차

### 1. setup 스크립트 실행

```bash
bash ~/apps/llm-wiki/current/scripts/mac/setup-quartz-experiment.sh
```

수행 내용:
- Node/npm 존재 확인
- Quartz v4 clone (`~/apps/llm-wiki/quartz-site`)
- npm 의존성 설치
- `quartz-site/content` → `public-vault` symlink 생성
- `public-vault/index.md` 없으면 기본 파일 생성

정상 출력 예:

```text
[OK] Node: v20.x.x
[OK] public-vault found: /Users/boseong/apps/llm-wiki/public-vault
[OK] content -> /Users/boseong/apps/llm-wiki/public-vault
[OK] index.md created
=== Setup complete ===
```

### 2. build 스크립트 실행

```bash
bash ~/apps/llm-wiki/current/scripts/mac/build-quartz-experiment.sh
```

빌드 전 체크:
- `content` symlink 확인
- `content/index.md` 존재 확인
- `content/Notes/` 존재 확인 (없으면 경고만)

### 3. serve 스크립트 실행

```bash
# 기본 (localhost:8080)
bash ~/apps/llm-wiki/current/scripts/mac/serve-quartz-experiment.sh

# 포트 변경
bash ~/apps/llm-wiki/current/scripts/mac/serve-quartz-experiment.sh 8081

# LAN/Tailscale 접근 허용 (0.0.0.0 바인딩)
bash ~/apps/llm-wiki/current/scripts/mac/serve-quartz-experiment.sh 8080 0.0.0.0
```

브라우저에서 `http://localhost:8080` 접속.

### 4. 외부 접속

| 방법 | 명령/URL |
|---|---|
| localhost 전용 | `http://localhost:8080` (Mac mini 내부) |
| SSH 터널 (Windows) | `ssh -L 8080:localhost:8080 boseong@<mac-ip>` 후 `http://localhost:8080` |
| LAN 직접 (0.0.0.0 바인딩) | `http://<mac-lan-ip>:8080` |
| Tailscale | `http://<tailscale-ip>:8080` (0.0.0.0 바인딩 시) |

---

## 실제 검증된 성공 절차 (2026-05-20, Quartz v4.5.2)

수동 조치 후 성공한 절차:

```bash
cd /Users/boseong/apps/llm-wiki/quartz-site

# content symlink 생성
rm -rf content
ln -s /Users/boseong/apps/llm-wiki/public-vault content

# index.md 생성
cat > /Users/boseong/apps/llm-wiki/public-vault/index.md <<'EOF'
---
title: LLM Wiki
---
# LLM Wiki
## Notes
- [[Notes/References/...]]
EOF

# serve
npx quartz build --serve
```

성공 로그:

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

위 절차는 `setup-quartz-experiment.sh`에 자동화되어 있다.

---

## 확인할 것

```text
1. Notes/가 사이드바 또는 목록에 보이는가?
2. 노트 열었을 때 Markdown이 렌더링되는가?
3. Mermaid 코드블록이 다이어그램으로 렌더링되는가?
4. [[Wikilink]]가 동작하거나 broken link로 표시되는가?
5. 검색(Search)이 동작하는가?
6. Graph view가 쓸 만한가?
7. 모바일 브라우저에서 가독성이 괜찮은가?
```

---

## 주의사항

- `public-vault/index.md`가 홈페이지다. `README.md`는 별도 페이지로 접근된다.
- `content` symlink가 없으면 Quartz가 `Found 0 input files`를 출력한다.
- `index.md`가 없으면 `/` 접근 시 404가 발생한다.
- public-vault에 `Inbox/`, `System/private/`, `.env` 가 없는지 사전에 확인한다.
- Node/npm 버전이 낮으면 Quartz가 빌드를 거부할 수 있다 (Node 20+ 권장).
- `quartz-site/` 자체는 public-vault GitHub repo에 포함하지 않는다.

---

## GitHub Pages 배포 (실험 이후)

```bash
# quartz-site에서 정적 빌드
npx quartz build
# output: quartz-site/public/

# GitHub Actions로 자동 배포 가능 (별도 설정)
```

이번 작업에서는 로컬 검증까지만 수행.
