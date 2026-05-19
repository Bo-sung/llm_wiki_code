# Capture API Product Spec

## 목적

웹 페이지, 선택 텍스트, 메모를 LLM Wiki Inbox로 빠르게 저장하는 로컬 HTTP API.
Chrome Extension과 curl이 이 API를 통해 Inbox에 파일을 생성한다.

---

## 엔드포인트

| Method | Path | 인증 | 역할 |
|---|---|---|---|
| GET | `/health` | 불필요 | 상태 확인 |
| POST | `/api/capture/link` | Bearer Token | 페이지 URL + 제목 저장 |
| POST | `/api/capture/clip` | Bearer Token | URL + 선택 텍스트 저장 |
| POST | `/api/capture/note` | Bearer Token | 수동 메모 저장 |

---

## 저장 위치

| 타입 | 저장 경로 |
|---|---|
| link | `{LLM_WIKI_ROOT}/Inbox/links/` |
| clip | `{LLM_WIKI_ROOT}/Inbox/raw_clips/` |
| note | `{LLM_WIKI_ROOT}/Inbox/raw_clips/` |

파일명: `YYYY-MM-DD-HHMMSS-slug.json`

---

## 파일 포맷

```json
{
  "type": "clip",
  "title": "페이지 제목",
  "url": "https://example.com/article",
  "selectedText": "선택한 텍스트",
  "text": null,
  "capturedAt": "2026-05-19T10:00:00Z",
  "source": "chrome-extension"
}
```

`type` 값: `link`, `clip`, `note`

---

## 인증

```
Authorization: Bearer <CAPTURE_API_TOKEN>
```

- `CAPTURE_API_TOKEN` 미설정 시 모든 capture 엔드포인트 503 반환
- 토큰 누락: 401
- 토큰 불일치: 401
- 토큰 값은 로그에 출력하지 않는다

---

## 환경 변수

| 변수 | 기본값 | 설명 |
|---|---|---|
| `CAPTURE_API_TOKEN` | (없으면 503) | Bearer 인증 토큰 |
| `CAPTURE_API_BIND_URL` | `http://127.0.0.1:5055` | 바인딩 주소 |
| `LLM_WIKI_ROOT` | `./data` | Wiki 루트 경로 |
| `LLM_WIKI_ENV_FILE` | 없음 | .env 파일 경로 |

`0.0.0.0` 바인딩은 명시적으로 `CAPTURE_API_BIND_URL`에 설정한 경우에만 허용한다.
