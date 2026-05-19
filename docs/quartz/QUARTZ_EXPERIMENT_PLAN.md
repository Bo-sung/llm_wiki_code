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

## 후보 구조

### 후보 A: public-vault 내부에 Quartz 설정

```
public-vault/
  Notes/
  Index/
  quartz.config.ts
  package.json
```

장점:
- GitHub Pages 연결 단순
- 하나의 repo로 관리

단점:
- Markdown vault에 Node/TS 설정 파일이 섞임
- MAUI Reader는 이 파일들을 무시해야 함

### 후보 B: 별도 quartz-site (권장)

```
/Users/boseong/apps/llm-wiki/
  public-vault/          ← Markdown 원본
  quartz-site/           ← Quartz 설정만
    quartz.config.ts
    package.json
    content -> ../public-vault  (symlink 또는 contentDir 설정)
```

장점:
- public-vault는 순수 Markdown vault로 유지
- MAUI Reader가 읽을 vault가 깔끔함
- Quartz 설정 변경이 vault에 영향 없음

단점:
- 별도 디렉터리 관리
- 배포 파이프라인이 약간 복잡

---

## 권장 구조

**후보 B 우선 검토**

public-vault를 Quartz의 `contentDir`로 지정한다.
Quartz 자체 파일은 `quartz-site/`에 격리한다.

```bash
# quartz.config.ts 설정 예시
contentDir: "/Users/boseong/apps/llm-wiki/public-vault"
```

---

## 테스트할 기능

| 기능 | 확인 항목 |
|---|---|
| Markdown 렌더링 | 헤더, 표, 코드블록, bold/italic |
| Wikilink | `[[Note Title]]` 링크 동작 |
| Backlink | 노트 하단 backlink 목록 |
| Search | full-text 검색 |
| Graph | link graph view |
| Mermaid | flowchart 렌더링 |
| Mobile browser | 모바일에서 가독성 |
| Tag | 태그 필터 |

---

## 성공 기준

```text
quartz build가 에러 없이 완료
로컬 서버에서 Notes/ 노트 접근 가능
Mermaid 코드블록이 다이어그램으로 렌더링됨
Wikilink가 동작하거나 broken link 표시
모바일 브라우저에서 기본 가독성 확보
```

## 실패 기준

```text
Quartz가 public-vault 파일을 인식하지 못함
Mermaid가 전혀 렌더링되지 않음
wikilink가 전부 broken link
빌드 시간이 비현실적으로 길다 (>5분 for 100 notes)
```
