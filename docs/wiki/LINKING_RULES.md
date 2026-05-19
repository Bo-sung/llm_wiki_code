# LLM Wiki Linking Rules

## 지원 링크 subset

모든 클라이언트(Quartz, MAUI Reader)가 처리할 수 있는 링크 형식만 사용한다.

### 허용

| 형식 | 예시 | 설명 |
|---|---|---|
| `[[Note Title]]` | `[[AI Basics]]` | 제목 기반 wikilink |
| `[[Note Title\|Alias]]` | `[[AI Basics\|기초]]` | alias를 표시 텍스트로 사용 |
| `[[folder/note]]` | `[[Notes/AI Basics]]` | 경로 포함 wikilink |
| `[Markdown Link](path.md)` | `[AI Basics](../Notes/AI Basics.md)` | 상대 Markdown 링크 |
| `#tag` | `#ai` | 인라인 태그 |

### 금지

| 형식 | 이유 |
|---|---|
| `file:///Users/...` | 절대 로컬 경로 — 다른 환경에서 무효 |
| `C:\...` | Windows 절대 경로 |
| Quartz 전용 link syntax | 다른 클라이언트에서 파싱 불가 |
| JavaScript 링크 (`javascript:...`) | 보안 |

---

## Wikilink resolve 규칙

### MAUI Reader 기준

```text
1. [[Note Title]] → title 필드가 일치하는 노트 검색
2. 없으면 → 파일명 slug로 재시도 (파일명에서 확장자 제거, 공백 → 하이픈)
3. 그래도 없으면 → broken link로 표시
4. alias [[Note Title|표시텍스트]] → 표시 텍스트만 사용, resolve는 Note Title 기준
```

### Quartz 기준

```text
기본 wikilink resolve 동작 사용.
단, Quartz 전용 embed (![[...]]) 또는 transclude는 가능하면 회피한다.
```

---

## 이미지 링크

```markdown
![alt text](images/screenshot.png)
```

- 상대 경로만 사용한다.
- vault 내 `assets/` 또는 `images/` 서브디렉터리를 권장한다.
- 절대 경로 또는 외부 URL 임베드는 최소화한다.

---

## 외부 링크

```markdown
[링크 텍스트](https://example.com)
```

- 일반 Markdown 외부 링크는 허용한다.
- 단, `source_url`에 해당하는 링크는 frontmatter에 기록하는 것을 우선한다.

---

## Backlink

- Quartz는 자동으로 backlink를 수집한다.
- MAUI Reader는 link index를 빌드해 backlink를 제공한다.
- Markdown 원문에 backlink를 직접 기록하지 않는다.
