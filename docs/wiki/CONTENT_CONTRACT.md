# LLM Wiki Content Contract

## 목적

Quartz 웹 뷰어와 향후 MAUI Reader가 같은 Markdown vault를 읽을 수 있도록 콘텐츠 규격을 고정한다.
어느 한 렌더러에 종속된 문법 사용을 금지하고, 모든 클라이언트가 동일한 파일을 파싱할 수 있게 한다.

---

## 적용 대상

- 운영용 vault에서 공개 검수 후 export된 Markdown
- `public-vault/Notes/`, `public-vault/Index/`
- Quartz 웹 뷰어 (실험 중)
- 향후 MAUI Reader

---

## 기본 원칙

1. Markdown 원문은 특정 렌더러에 종속되지 않는다.
2. YAML frontmatter를 사용한다. 자세한 스키마는 [FRONTMATTER_SCHEMA.md](FRONTMATTER_SCHEMA.md).
3. Mermaid는 fenced code block만 사용한다.
4. Wikilink는 제한된 subset만 사용한다. 자세한 규칙은 [LINKING_RULES.md](LINKING_RULES.md).
5. 원문 전문, 비밀정보, 개인 경로는 포함하지 않는다.
6. 렌더링 호환 범위는 [RENDERING_COMPATIBILITY.md](RENDERING_COMPATIBILITY.md) 참고.

---

## 디렉터리 구조

```
public-vault/
  Notes/          — 공개 처리된 노트
  Index/          — 카테고리/태그 인덱스
  Templates/      — 선택, 노트 템플릿
  System/public/  — 선택, 공개 시스템 파일
```

---

## 금지 대상

vault에 포함하지 않는다.

| 항목 | 이유 |
|---|---|
| `Inbox/` | 미처리 원본 — 공개 불가 |
| `Sources/` | 원문 전체 — 저작권/공개 정책 |
| `System/private/` | 내부 시스템 파일 |
| `.env` | 민감정보 |
| `*.log` | 로그 |
| 개인 식별 정보 | 개인정보 보호 |
| 실제 API 키/토큰 | 보안 |

---

## Quartz 비종속 원칙

Quartz를 사용하더라도 다음 규칙을 지킨다.

```text
Quartz 전용 shortcode 남용 금지
Quartz 플러그인에만 의존하는 콘텐츠 문법 금지
React component 삽입 금지
MAUI Reader가 파싱할 수 없는 비표준 문법 금지
```

---

## 호환 대상 정리

| 기능 | Quartz | MAUI Reader |
|---|---|---|
| Markdown 파싱 | 자체 | C# Markdig 등 |
| YAML frontmatter | ✓ | C# YamlDotNet 등 |
| `[[wikilink]]` | ✓ | 자체 구현 |
| Mermaid | 플러그인 | WebView + mermaid.js |
| 표준 Markdown 표/코드 | ✓ | ✓ |
| 이미지 (상대 경로) | ✓ | ✓ |
