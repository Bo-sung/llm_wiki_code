# Capture API Operation Guide

## 1. Mac mini .env 설정

`/Users/boseong/apps/llm-wiki/shared/.env`에 다음 항목을 추가한다.

```env
CAPTURE_API_TOKEN=<생성한 토큰>
CAPTURE_API_BIND_URL=http://127.0.0.1:5055

# 브라우저 Extension 사용 시 CORS 활성화 필요
CAPTURE_API_CORS_MODE=Development
```

CORS는 항상 활성화된다 (MVP 기본값). `CAPTURE_API_CORS_MODE` 설정 여부와 무관하다.
Bearer Token 인증은 모든 capture 엔드포인트에서 유지된다.

토큰 생성:

```bash
openssl rand -hex 32
```

생성된 토큰 값은 `.env`에만 기록한다. 문서, 로그, git commit에 포함하지 않는다.

---

## 2. Capture API launchd 설치

```bash
cd ~/apps/llm-wiki/current
bash scripts/mac/install-launchd-captureapi.sh
```

확인:

```bash
launchctl list com.llmwiki.captureapi
```

PID 컬럼이 숫자이면 실행 중. `"-"` 또는 오류이면 `captureapi.err.log` 확인.

---

## 3. 상태 확인

```bash
bash ~/apps/llm-wiki/current/scripts/mac/status-launchd-captureapi.sh
```

또는 개별 확인:

```bash
launchctl list com.llmwiki.captureapi
tail -f ~/apps/llm-wiki/shared/logs/captureapi.out.log
tail -f ~/apps/llm-wiki/shared/logs/captureapi.err.log
```

---

## 4. curl 테스트

### health (인증 불필요)

```bash
curl http://127.0.0.1:5055/health
```

예상 응답:

```json
{"status":"ok","app":"LlmWiki.CaptureApi"}
```

### link capture

```bash
curl -X POST http://127.0.0.1:5055/api/capture/link \
  -H "Authorization: Bearer $CAPTURE_API_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Example Article",
    "url": "https://example.com/article",
    "capturedAt": "2026-05-19T10:00:00Z",
    "source": "curl"
  }'
```

### clip capture

```bash
curl -X POST http://127.0.0.1:5055/api/capture/clip \
  -H "Authorization: Bearer $CAPTURE_API_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Example Clip",
    "url": "https://example.com/article",
    "selectedText": "selected text here",
    "capturedAt": "2026-05-19T10:00:00Z",
    "source": "curl"
  }'
```

### note capture

```bash
curl -X POST http://127.0.0.1:5055/api/capture/note \
  -H "Authorization: Bearer $CAPTURE_API_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "My Note",
    "text": "note content here",
    "capturedAt": "2026-05-19T10:00:00Z",
    "source": "manual"
  }'
```

---

## 5. Inbox 파일 생성 확인

```bash
ls ~/apps/llm-wiki/shared/data/Inbox/links/
ls ~/apps/llm-wiki/shared/data/Inbox/raw_clips/
```

link capture → `Inbox/links/YYYY-MM-DD-HHMMSS-slug.json`
clip/note → `Inbox/raw_clips/YYYY-MM-DD-HHMMSS-slug.json`

---

## 6. watcher 처리 확인

watcher가 실행 중이면 `.json` 파일을 감지하고 Gemini로 처리한다.

처리 완료 후:
- 성공: `Inbox/processed/YYYY-MM-DD/`로 이동
- 실패: `Inbox/failed/YYYY-MM-DD/`로 이동

```bash
ls ~/apps/llm-wiki/shared/data/Inbox/processed/
ls ~/apps/llm-wiki/shared/data/Inbox/failed/
```

---

## 7. 로그 확인

```bash
tail -50 ~/apps/llm-wiki/shared/logs/captureapi.out.log
tail -50 ~/apps/llm-wiki/shared/logs/captureapi.err.log
tail -50 ~/apps/llm-wiki/shared/logs/watch.out.log
```

---

## 8. 재시작 / 중지

```bash
# 재시작
launchctl unload ~/Library/LaunchAgents/com.llmwiki.captureapi.plist
launchctl load   ~/Library/LaunchAgents/com.llmwiki.captureapi.plist

# 제거
bash ~/apps/llm-wiki/current/scripts/mac/uninstall-launchd-captureapi.sh
```
