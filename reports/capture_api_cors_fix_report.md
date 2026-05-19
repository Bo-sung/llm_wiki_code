# Capture API CORS Fix Report

## 1. 문제

`OPTIONS /api/capture/link` (및 clip, note)가 405로 응답해 Firefox Extension fetch가 실패했다.

---

## 2. 원인

| 증상 | 원인 |
|---|---|
| OPTIONS → 405 Method Not Allowed | `CAPTURE_API_CORS_MODE`가 없으면 `AddCors`에 policy가 등록되지 않아 `UseCors()`가 no-op였다 |
| curl/PowerShell POST → 성공 | preflight 없이 직접 POST → CORS 무관 |

이전 구현에서 `AddCors`는 `CAPTURE_API_CORS_MODE=Development` 조건 분기 안에 있었다.
Mac mini `.env`에 해당 값이 없어서 CORS policy가 등록되지 않았고, `UseCors()`는 아무 것도 하지 않았다.
ASP.NET Core 라우터가 OPTIONS를 알 수 없는 메서드로 판단해 405를 반환했다.

---

## 3. 수정

| 파일 | 변경 내용 |
|---|---|
| `src/LlmWiki.CaptureApi/Program.cs` | CORS를 항상 활성화, named policy "CaptureCors", OPTIONS endpoint 명시 추가 |
| `tests/LlmWiki.Tests/CaptureApiCorsTests.cs` | OPTIONS 204 검증 강화, 팩토리에서 CAPTURE_API_CORS_MODE 제거 |
| `docs/capture/CAPTURE_SECURITY_POLICY.md` | CORS 정책 설명 갱신 |
| `docs/capture/CAPTURE_API_OPERATION_GUIDE.md` | CORS 항상 활성 명시 |
| `docs/capture/FIREFOX_EXTENSION_LOCAL_TEST.md` | 트러블슈팅 원인 3 갱신 |

핵심 변경:

```csharp
// 항상 등록 (env var 조건 제거)
builder.Services.AddCors(options =>
{
    options.AddPolicy("CaptureCors", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

app.UseCors("CaptureCors");  // endpoint 매핑 전

// CORS middleware 외에 명시적 OPTIONS endpoint 추가 (belt-and-suspenders)
app.MapMethods("/api/capture/link",  new[] { "OPTIONS" }, () => Results.NoContent());
app.MapMethods("/api/capture/clip",  new[] { "OPTIONS" }, () => Results.NoContent());
app.MapMethods("/api/capture/note",  new[] { "OPTIONS" }, () => Results.NoContent());
```

---

## 4. CORS 정책

| 항목 | 값 |
|---|---|
| 등록 방식 | Named policy "CaptureCors" |
| 활성화 조건 | 항상 (env var 불필요) |
| AllowedOrigin | `*` |
| AllowedHeaders | `*` |
| AllowedMethods | `*` |
| OPTIONS 인증 | 없음 (preflight에는 Authorization 헤더가 없음) |
| POST 인증 | Bearer Token 유지 |

---

## 5. 검증 명령

Mac mini에서 OPTIONS preflight 응답 확인:

```bash
curl -i -X OPTIONS http://127.0.0.1:5055/api/capture/link \
  -H "Origin: moz-extension://test" \
  -H "Access-Control-Request-Method: POST" \
  -H "Access-Control-Request-Headers: authorization,content-type"
```

기대 응답:

```text
HTTP/1.1 204 No Content
Access-Control-Allow-Origin: *
Access-Control-Allow-Headers: *
Access-Control-Allow-Methods: *
```

---

## 6. 테스트 결과

```
통과: 108, 실패: 0, 건너뜀: 0
신규/갱신: CaptureApiCorsTests
  - OPTIONS /api/capture/{link|clip|note} → 204
  - OPTIONS 응답에 Access-Control-Allow-Origin 헤더 확인
  - OPTIONS 응답에 CORS 헤더 확인 (authorization/content-type)
  - POST 인증 없음 → 401 (3개 endpoint)
  - POST 잘못된 토큰 → 401 (3개 endpoint)
  - POST 유효한 토큰 → 200 (3개 endpoint)
  - GET /health → 200
```

---

## 7. 사용자 재검증 항목

| 항목 | 상태 |
|---|---|
| Mac mini 재배포 | 필요 |
| Capture API 재시작 | 필요 |
| curl OPTIONS 204 확인 | 필요 |
| Firefox extension reload | 필요 |
| Save Page | 필요 |
| Save Selection | 필요 |
| Save Note | 필요 |
