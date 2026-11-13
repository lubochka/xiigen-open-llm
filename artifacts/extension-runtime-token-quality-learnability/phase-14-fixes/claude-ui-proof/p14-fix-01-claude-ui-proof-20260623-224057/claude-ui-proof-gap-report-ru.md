# Phase 14 FR-01 / P14-FIX-01: Claude Desktop/app UI proof-gap packet

Вердикт: **NEEDS_FIX** для FR-01.

Это не PASS и не final readiness claim. В этом запуске удалось подтвердить только наличие Claude Desktop/app на хосте и наличие redacted visible window handle. Не удалось подтвердить обязательную цепочку FR-01: prompt accepted by Claude UI -> thin adapter -> `llm_mvp_core` -> user-visible answer/error/debug with correlation id -> redacted input/output/freshness -> controlled negative path.

## Что проверено

| Check | Result | Что это доказывает |
|---|---:|---|
| `Get-Command claude -ErrorAction SilentlyContinue` | `NOT_FOUND` | Shell-resolvable Claude CLI command не найден. |
| `Get-CimInstance Win32_Process ...` | `FOUND` | Claude Desktop/app и Claude Code-related процессы присутствуют. Command lines не сохранены raw; только redacted flag classification. |
| `Get-StartApps ...` | `FOUND` | Windows Start app entries для Claude присутствуют. |
| `Test-Path` common Claude app/config locations | `FOUND` | Локальные Claude app/config директории присутствуют. |
| `Get-Process -Name claude` with redacted title check | `WINDOW_FOUND` | Есть Claude window handle. Это не доказывает prompt acceptance или adapter/core invocation. |
| Remote-debugging flag discovery | `NOT_FOUND` | Не найден безопасный browser-style remote-debugging control path для UI proof. |
| Claude URI protocol registry check | `FOUND` | URI protocol key есть, но он не доказывает prompt submission, visible answer или `llm_mvp_core` call. |
| Existing handler/package reports | `FOUND_UI_NON_CLAIMS` | Ранее созданные Claude handler/package proofs прямо говорят, что это не full interactive Claude UI/chat proof. |

## Почему не PASS

FR-01 требует actual Claude Desktop/app UI runtime proof. Наличие установленного/запущенного Claude app и даже visible window handle не закрывает это требование.

В этом окружении Codex не имеет безопасного контролируемого Claude UI/app surface, чтобы:

- отправить test prompt через видимый Claude UI;
- захватить user-visible answer/error/debug;
- связать тот же запуск с `llm_mvp_core` adapter/core correlation id;
- сохранить redacted input/output и freshness evidence;
- выполнить controlled negative path через тот же Claude UI.

Prompt в Claude не отправлялся. External AI calls не выполнялись. Web browsing не выполнялся.

## Evidence boundary

| Evidence item | Status |
|---|---:|
| Existing Claude handler/package proof | Evidence-only, insufficient for FR-01 |
| Claude Desktop/app installed/running | Proven |
| Visible Claude window handle | Proven, title redacted |
| Prompt accepted by Claude UI | Not verified |
| Same-run thin adapter call into `llm_mvp_core` | Not verified |
| User-visible answer/error/debug with correlation id | Not verified |
| Redacted input/output/freshness | Not verified |
| Controlled negative path in Claude UI | Not verified |

## Changed files

- `C:\Projects\llm_mvp_core\tools\phase14-claude-ui-proof-diagnostic.ps1`
- `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-14-fixes\claude-ui-proof\p14-fix-01-claude-ui-proof-20260623-224057\claude-ui-proof-gap-diagnostic.json`
- `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-14-fixes\claude-ui-proof\p14-fix-01-claude-ui-proof-20260623-224057\claude-ui-proof-gap-packet.json`
- `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-14-fixes\claude-ui-proof\p14-fix-01-claude-ui-proof-20260623-224057\claude-ui-proof-gap-report-ru.md`

## Evidence artifacts

- `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-14-fixes\claude-ui-proof\p14-fix-01-claude-ui-proof-20260623-224057\claude-ui-proof-gap-diagnostic.json`
- `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-14-fixes\claude-ui-proof\p14-fix-01-claude-ui-proof-20260623-224057\claude-ui-proof-gap-packet.json`
- `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-14-fixes\claude-ui-proof\p14-fix-01-claude-ui-proof-20260623-224057\claude-ui-proof-gap-report-ru.md`

## Exact next action

Issue a bounded, operator-approved Claude Desktop/app UI capture work order in the active Claude app session:

1. Send one test-only XIIGen positive prompt through visible Claude UI.
2. Send one controlled negative prompt through the same Claude UI.
3. Capture redacted prompt/answer/error/debug with timestamp and correlation id.
4. Collect same-run `llm_mvp_core` adapter/core trace refs.
5. Save artifacts under `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\phase-14-fixes\claude-ui-proof\`.
6. Forbid product edits and git.
7. Do not claim PASS unless all FR-01 evidence fields are present.

## Confirmations

- Git commands run: **none**.
- External AI calls run: **none**.
- Web browsing run: **none**.
- Product extension/repo files edited: **none**.
