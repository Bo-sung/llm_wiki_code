# Stabilization Report

작성일: 2026-05-19

## 1. 수행 결과

- **완료**: 처리 후 원본 파일 이동 정책 구현, 중복 처리 방지 (파일 이동), 파일명 충돌 처리, 실패 파일 분리, 로그 출력 강화, `--dry-run` 옵션, 테스트 30개 통과, `docs/MAC_MINI_CHECKLIST.md` 작성, `.NET 10 전환 계획` 문서화, `docs/DECISIONS.md` 갱신
- **부분 완료**: 없음
- **미완료**: Mac mini 실환경 검증 (현재 Windows에서만 테스트)

## 2. 생성/수정 파일

| 파일 | 목적 |
|---|---|
| `src/LlmWiki.Core/Inbox/InboxMover.cs` | 파일 이동 유틸 (processed/failed, 충돌 방지) |
| `src/LlmWiki.Core/Inbox/ProcessOptions.cs` | 처리 옵션 (dry-run 플래그) |
| `src/LlmWiki.Core/Inbox/InboxProcessor.cs` | 원본 파일 이동, 로그 강화, dry-run 지원 |
| `src/LlmWiki.Core/Inbox/InboxWatcher.cs` | `ProcessOptions.Default` 전달로 서명 일치 |
| `src/LlmWiki.Cli/Program.cs` | `--dry-run` 파싱, 확장 로그 출력 |
| `tests/LlmWiki.Tests/InboxMoverTests.cs` | InboxMover 단위 테스트 6개 |
| `tests/LlmWiki.Tests/InboxProcessorTests.cs` | InboxProcessor 통합 테스트 6개 |
| `docs/MAC_MINI_CHECKLIST.md` | Mac mini 실환경 검증 체크리스트 |
| `docs/DECISIONS.md` | 원본 파일 처리 정책 확정, .NET 10 전환 계획 추가 |

## 3. 실행 명령

```bash
# 빌드
dotnet build

# 테스트
dotnet test

# 일반 처리
dotnet run --project src/LlmWiki.Cli -- process-once

# dry-run
dotnet run --project src/LlmWiki.Cli -- process-once --dry-run

# 감시 모드
dotnet run --project src/LlmWiki.Cli -- watch
```

## 4. 테스트 결과

```text
총 테스트 수: 30
     통과: 30
 총 시간: 0.52초
```

## 5. 현재 동작 방식

- **입력**: `data/Inbox/links/`, `raw_clips/`, `mobile/`에 `.txt`/`.url`/`.md` 파일 배치
- **처리**: `process-once` → 파일 수집 → Gemini 설정 확인 → API 호출 또는 fallback → 노트 생성
- **출력**: `data/Notes/References/YYYY-MM-DD-slug.md`
- **원본 파일 처리**:
  - 성공 → `data/Inbox/processed/YYYY-MM-DD/{파일명}`
  - 실패 → `data/Inbox/failed/YYYY-MM-DD/{파일명}`
  - 파일명 충돌 시 `-1`, `-2` 접미사로 처리
  - 원본 삭제 없음

## 6. 처리 정책 반영 체크

| 항목 | 반영 여부 | 비고 |
|---|---|---|
| 성공 시 processed 이동 | 완료 | `InboxMover.MoveToProcessed` |
| 실패 시 failed 이동 | 완료 | `InboxMover.MoveToFailed` |
| 원본 삭제 금지 | 완료 | `File.Move` 사용, Delete 없음 |
| 파일명 충돌 방지 | 완료 | `-1`, `-2` 접미사 자동 부여 |
| Gemini 미설정 fallback 성공 처리 | 완료 | fallback 생성 후 processed로 이동 |
| 로그 출력 | 완료 | scanned, gemini_calls, fallbacks, moved_to 출력 |
| dry-run | 완료 | `--dry-run` 플래그로 스캔만 실행 |

## 7. 결정 필요

| 항목 | 선택지 | 권장안 | 이유 |
|---|---|---|---|
| 카테고리 자동 분류 | Gemini 분류 / 사용자 지정 / References 유지 | 보류 | Gemini 정리 품질 확인 후 결정 |
| Mac mini 상시 실행 | launchd / 수동 / cron | launchd | Mac mini 검증 후 결정 |
| GitHub 백업 | 수동 push / 자동 commit / 자동 push | 보류 | 파일 처리 안정화 후 결정 |

## 8. 리스크 / 주의점

- 전체 테스트는 Windows에서 수행. macOS 경로 구분자 및 FileSystemWatcher 동작은 검증 필요. (추정)
- `watch` 모드는 파일 생성 이벤트에만 반응. 파일을 직접 복사하면 인식하지 못하는 경우가 있음. (추정: macOS FSEvents 동작에 따라 다를 수 있음)
- `process-once`는 매 실행 시 현재 `links/`, `raw_clips/`, `mobile/` 하위의 모든 파일을 처리. 이전 실행에서 `processed/`로 이동된 파일은 재처리되지 않음.

## 9. 다음 작업 후보

1. Mac mini 실환경 검증 수행 → `reports/mac_mini_validation_report.md` 작성
2. launchd 등록 구성 (Mac mini 검증 이후)
3. 카테고리 자동 분류 구현 (Gemini 정리 품질 확인 후)
