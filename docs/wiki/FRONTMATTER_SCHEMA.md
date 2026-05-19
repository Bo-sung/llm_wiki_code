# LLM Wiki Frontmatter Schema

## 표준 frontmatter 예시

```yaml
---
id: 2026-05-20-example-note
title: Example Note
created: 2026-05-20
updated: 2026-05-20
tags:
  - ai
  - llm
source_url: https://example.com/article
source_type: webpage
visibility: public
publishable: true
review_status: reviewed
---
```

---

## 필드 정책

| 필드 | 필수 | 타입 | 설명 |
|---|---|---|---|
| `id` | 권장 | string | 안정적인 노트 식별자. 형식: `YYYY-MM-DD-slug` |
| `title` | 필수 | string | 표시 제목 |
| `created` | 필수 | date | 생성일 (`YYYY-MM-DD`) |
| `updated` | 권장 | date | 최종 수정일 |
| `tags` | 권장 | list | 검색/분류용 태그 목록 |
| `source_url` | 선택 | string | 원 출처 URL |
| `source_type` | 선택 | string | `webpage` / `youtube` / `pdf` / `manual` |
| `visibility` | 필수 | string | `private` / `public` |
| `publishable` | 필수 | bool | 공개 가능 여부 |
| `review_status` | 필수 | string | `unreviewed` / `reviewed` / `rejected` |

---

## 기본값 정책

자동 생성 노트(Gemini 처리 완료 직후):

```yaml
visibility: private
publishable: false
review_status: unreviewed
```

public-vault로 export할 노트는 사람이 검수 후 변경:

```yaml
visibility: public
publishable: true
review_status: reviewed
```

---

## 렌더러 호환성

| 필드 | Quartz | MAUI Reader |
|---|---|---|
| `title` | 페이지 제목으로 사용 | 노트 헤더 |
| `tags` | 태그 필터/그래프 | 검색 인덱스 |
| `created` | 정렬/표시 | 정렬 |
| `source_url` | 링크로 표시 | 출처 표시 |
| `visibility` | 공개 필터 | 필터 |
| `publishable` | 공개 필터 | 필터 |

---

## 주의

- `id` 필드는 파일명과 일치하는 것이 권장되지만 필수는 아니다.
- `tags`는 소문자, 하이픈 구분자를 권장한다.
- `source_type`은 정해진 값 목록 외 임의 문자열도 허용하되 가능하면 표준 값을 사용한다.
