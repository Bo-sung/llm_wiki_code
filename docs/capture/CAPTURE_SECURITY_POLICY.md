# Capture Security Policy

## 인증

- 모든 capture 엔드포인트는 Bearer Token 인증 필수
- `GET /health`만 인증 제외
- `CAPTURE_API_TOKEN` 미설정 시 capture 엔드포인트 503 반환 (서버 시작 차단 안 함)
- 토큰 값을 로그, 에러 메시지, 환경 변수 출력에 포함하지 않는다

## 바인딩

- 기본 바인딩: `http://127.0.0.1:5055` (루프백 전용)
- `0.0.0.0` 바인딩은 `CAPTURE_API_BIND_URL` 명시 설정 시에만 허용
- 외부 공개 용도로 운영하지 않는다 (MVP 범위 외)

## 토큰 관리

- 토큰 생성: `openssl rand -hex 32`
- 토큰은 `.env` 파일에만 보관
- `.env` 파일은 git에 포함하지 않는다
- 토큰 노출 시 즉시 새 토큰으로 교체하고 `.env`를 갱신한다

## HTTPS

- MVP는 localhost 전용이므로 HTTP 사용
- 원격 접근이 필요한 경우 SSH 터널 또는 리버스 프록시(Nginx + TLS) 사용

## CORS

- Firefox/Chrome Extension은 `Authorization` + `Content-Type` 헤더를 포함하므로 모든 POST 전에 OPTIONS preflight를 보낸다
- Capture API는 MVP에서 CORS를 항상 활성화한다 (AllowAnyOrigin/AllowAnyHeader/AllowAnyMethod)
- `OPTIONS /api/capture/*`는 인증 없이 허용 (preflight에는 Authorization 헤더가 없음)
- `POST /api/capture/*`는 Bearer Token 인증이 CORS 설정과 무관하게 유지된다
- OPTIONS 명시 endpoint (`MapMethods(..., ["OPTIONS"], ...)`)와 `UseCors("CaptureCors")`를 함께 사용해 확실히 처리한다
- `CAPTURE_API_CORS_MODE`는 문서화용으로 남겨두며, 미설정 시에도 CORS는 비활성화되지 않는다
- 향후 origin 제한이 필요한 경우 `CAPTURE_API_ALLOWED_ORIGINS`를 사용해 `WithOrigins()`으로 전환한다

## Chrome Extension 토큰 저장

- `chrome.storage.sync`에 저장 (브라우저 암호화 스토리지)
- Options 페이지에서 password input으로 입력
- 토큰을 콘솔 로그에 출력하지 않는다
