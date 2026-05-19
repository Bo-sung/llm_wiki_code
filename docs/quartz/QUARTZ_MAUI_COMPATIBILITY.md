# Quartz and MAUI Reader Compatibility

## 핵심 원칙

Quartz와 MAUI Reader는 **코드를 공유하지 않는다.**
대신 **Markdown content contract**를 공유한다.

vault의 `.md` 파일은 두 클라이언트 중 어느 것이 읽어도 동일하게 파싱 가능해야 한다.

---

## 공유 대상

| 항목 | 설명 |
|---|---|
| Markdown 파일 | `.md` 원본 |
| YAML frontmatter | `id`, `title`, `created`, `tags`, `visibility` 등 |
| Wikilink 규칙 | `[[Note Title]]`, `[[Note Title\|Alias]]` |
| Mermaid code block | ` ```mermaid ``` ` |
| Tags | frontmatter `tags` 또는 인라인 `#tag` |
| Source metadata | `source_url`, `source_type` |
| Link graph 모델 | 노트 간 링크 관계 (각자 독립 구현) |

---

## 공유하지 않는 대상

| 항목 | 이유 |
|---|---|
| Quartz React component | MAUI와 무관한 웹 전용 |
| Quartz plugin | Quartz 전용 빌드 시스템 |
| Quartz config (`quartz.config.ts`) | TypeScript/Node 기반 |
| Quartz 검색 구현 | flexsearch 기반 웹 전용 |
| Quartz graph 구현 | D3.js 기반 웹 전용 |

---

## MAUI Reader 예상 구현

| 기능 | 예상 구현 방법 |
|---|---|
| Markdown 파싱 | C# Markdig |
| YAML frontmatter 파싱 | C# YamlDotNet |
| Wikilink resolve | 자체 link index (C#) |
| Markdown 렌더링 | MAUI BlazorWebView 또는 WebView |
| Mermaid 렌더링 | WebView + mermaid.js |
| Full-text 검색 | 자체 색인 (C#) 또는 SQLite FTS |
| Graph view | 자체 구현 또는 생략 |
| Tag 필터 | frontmatter tags 색인 |

---

## 마이그레이션 리스크 평가

현재 vault 파일이 content contract를 준수하는 경우, Quartz → MAUI 전환 또는 병행 사용 시:

| 항목 | 리스크 |
|---|---|
| 표준 Markdown | 없음 |
| YAML frontmatter | 없음 |
| `[[wikilink]]` | 낮음 — MAUI에서 별도 구현 필요하나 파일 수정 불필요 |
| Mermaid | 낮음 — 렌더러만 교체 |
| Quartz shortcode 사용 없음 | 없음 |
| Quartz `![[embed]]` 사용 시 | 중간 — MAUI 별도 구현 필요 |

---

---

## 검증된 구조 (2026-05-20)

```
public-vault/          ← MAUI Reader가 읽을 vault (순수 Markdown)
  index.md
  Notes/
quartz-site/           ← Quartz 전용 (MAUI와 무관)
  content -> ../public-vault   ← symlink
  quartz.config.ts
```

이 구조에서:
- MAUI Reader는 `public-vault/`만 읽으면 된다
- Quartz는 `quartz-site/content` symlink 경유로 같은 파일을 읽는다
- 두 클라이언트 모두 동일한 `.md` 원본을 사용한다

---

## 결론

vault를 content contract 기준으로 유지하면 Quartz와 MAUI Reader는 동일한 파일을 각자의 방식으로 렌더링할 수 있다. 어느 한 쪽에 vault를 lock-in하지 않는다.
