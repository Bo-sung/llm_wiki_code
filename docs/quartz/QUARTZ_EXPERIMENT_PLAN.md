# Quartz Experiment Plan

## 목적

`public-vault`의 Markdown 노트를 웹 브라우저에서 바로 읽을 수 있도록 Quartz를 실험한다.
GitHub만으로는 Mermaid 렌더링, backlink, graph view, full-text search 등이 부족하다.

---

## 왜 Quartz인가

| 기준 | 이유 |
|---|---|
| Obsidian-compatible wikilink | `[[Note Title]]` 기본 지원 |
| Markdown-native | 파일 원본 수정 불필요 |
| Static site | GitHub Pages / Cloudflare Pages 무료 배포 가능 |
| Backlink/graph | Obsidian 유사 경험 |
| 오픈소스 | 종속성 낮음 |

---

## 왜 MAUI Reader와 호환성을 고려하는가

Quartz는 웹 전용 viewer다.
향후 오프라인/네이티브 클라이언트로 MAUI Reader가 필요해질 수 있다.
지금 Quartz 전용 문법에 의존하면 나중에 MAUI 구현 시 vault를 재작업해야 한다.
따라서 **콘텐츠 원본을 표준 Markdown으로 유지**하고, 렌더러만 교체 가능하게 설계한다.

---

## 채택한 구조 (후보 B, 검증 완료)

```
/Users/boseong/apps/llm-wiki/
  public-vault/              ← 순수 Markdown vault (git repo)
    index.md                 ← Quartz 홈페이지 (필수)
    Notes/
    README.md
  quartz-site/               ← Quartz 설정만 (vault와 분리)
    quartz.config.ts
    package.json
    content -> ../public-vault   ← symlink
    public/                  ← build output (gitignored)
```

### 채택 이유

- public-vault는 순수 Markdown으로 유지됨
- MAUI Reader가 읽을 vault에 Node/TS 설정 파일이 없음
- Quartz 설정 변경이 vault에 영향 없음
- content symlink로 연결 — 파일 복사 불필요

### 주의

- `content/index.md` 필수 — 없으면 `/` 404
- `content` symlink 필수 — 없으면 `Found 0 input files`
- `quartz-site/` 는 public-vault repo에 포함하지 않음

---

## 검증 결과 (2026-05-20, Quartz v4.5.2)

```text
Found 4 input files from `content`
Parsed 4 Markdown files
Emitted 33 files to `public`
[200] /
[200] /Notes/
[200] /Notes/References/2026-05-19-gemini-25-flash
[200] /Notes/References/2026-05-19-launchd-watch-test
[200] /README
```

---

## 테스트할 기능

| 기능 | 확인 항목 | 상태 |
|---|---|---|
| Markdown 렌더링 | 헤더, 표, 코드블록, bold/italic | 확인 완료 |
| 노트 목록 | `/Notes/` 접근 가능 | 확인 완료 |
| Wikilink | `[[Note Title]]` 링크 동작 | 미확인 |
| Backlink | 노트 하단 backlink 목록 | 미확인 |
| Search | full-text 검색 | 미확인 |
| Graph | link graph view | 미확인 |
| Mermaid | flowchart 렌더링 | 미확인 |
| Mobile browser | 모바일에서 가독성 | 미확인 |
| Tag | 태그 필터 | 미확인 |

---

## 성공 기준

```text
quartz build가 에러 없이 완료 ✓
로컬 서버에서 Notes/ 노트 접근 가능 ✓
Mermaid 코드블록이 다이어그램으로 렌더링됨 (미확인)
Wikilink가 동작하거나 broken link 표시 (미확인)
모바일 브라우저에서 기본 가독성 확보 (미확인)
```

## 실패 기준

```text
Quartz가 public-vault 파일을 인식하지 못함
Mermaid가 전혀 렌더링되지 않음
빌드 시간이 비현실적으로 길다 (>5분 for 100 notes)
```
