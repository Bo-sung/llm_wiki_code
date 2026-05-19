# Capture Roadmap

## 완료 (2026-05-20)

- Capture API (link / clip / note)
- Bearer Token 인증
- Inbox JSON 파일 저장
- InboxProcessor .json 처리
- InboxEventFilter .json 허용
- Capture API launchd 스크립트
- Chrome Extension MVP 스캐폴드
- Firefox Extension MVP 스캐폴드 (MV2, shared/ 모듈 구조)
- Firefox 패키징 스크립트
- Windows Firefox → Mac mini 접근 방법 문서화

---

## 다음 단계 후보

### 1순위 — Mac mini 실제 검증 + Firefox 로드 테스트

- Capture API 배포 및 launchd 등록
- curl health / link / clip / note 검증
- watcher JSON 처리 확인
- Firefox Extension `about:debugging` 로드 및 동작 확인
- LAN IP 또는 Tailscale 경유 Windows → Mac mini 접근 검증

### 2순위 — Chrome Extension 완성

- 아이콘 제작 (16px, 48px, 128px)
- 오류 알림 개선 (badge, notification)
- 저장 성공 시 팝업 자동 닫기
- 단축키 지원

### 3순위 — Readability / 본문 추출

- `@mozilla/readability` 도입
- 페이지 본문 자동 추출 → clip에 포함
- 현재는 선택 텍스트만 지원

### 4순위 — Firefox 패키징

- `browser_specific_settings` 추가
- Firefox Add-ons 로컬 설치 검증
- WebExtension API 호환성 확인

### 5순위 — 모바일 / 원격 접근

- iOS Shortcut 또는 Scriptable 연동
- SSH 터널 / Tailscale 경유 원격 capture
- HTTPS 지원 (리버스 프록시)
