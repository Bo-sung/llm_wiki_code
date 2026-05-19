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
cd ~/apps/llm-wiki
bash ~/apps/llm-wiki/current/scripts/mac/setup-quartz-experiment.sh
```

### 2. build 스크립트 실행

```bash
bash ~/apps/llm-wiki/current/scripts/mac/build-quartz-experiment.sh
```

### 3. serve 스크립트 실행 (로컬 preview)

```bash
bash ~/apps/llm-wiki/current/scripts/mac/serve-quartz-experiment.sh
```

브라우저에서 `http://localhost:8080` 또는 안내된 주소 접속.

### 4. 모바일/외부 접속

로컬 네트워크에서 접근 시:

```bash
# Mac mini LAN IP 확인
ipconfig getifaddr en0

# 브라우저에서
# http://<mac-lan-ip>:8080
```

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

## quartz.config.ts 핵심 설정

```typescript
// quartz-site/quartz.config.ts 예시
const config: QuartzConfig = {
  configuration: {
    pageTitle: "LLM Wiki",
    contentDir: "/Users/boseong/apps/llm-wiki/public-vault",
    // ...
  },
  plugins: {
    transformers: [
      // Mermaid 렌더링
      Plugin.Mermaid(),
      // ...
    ],
    // ...
  },
}
```

`contentDir`를 public-vault 절대 경로로 지정한다.

---

## GitHub Pages 배포 (실험 이후)

```bash
# quartz-site에서
npx quartz build
# output/에 정적 파일 생성

# GitHub Actions로 자동 배포 가능
# 단, 이번 작업에서는 로컬 검증까지만 수행
```

---

## 주의

- public-vault에 `Inbox/`, `System/private/`, `.env`, `*.log`가 없는지 확인한다.
- Node/npm 버전이 낮으면 Quartz가 빌드를 거부할 수 있다 (Node 20+ 권장).
- quartz-site는 public-vault GitHub repo에 포함하지 않는다.
