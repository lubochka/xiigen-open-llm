# Phase 14 real-program visible-result proof, p14-fix-09

Дата запуска: 2026-06-24 03:08:43 UTC.
Папка пакета: `p14-fix-09-real-program-visible-result-20260624-060843`.

Короткий вывод: Phase 14.04 не закрыт. Cursor закрыт в рамках этого среза, но Claude и Codex остаются PARTIAL/BLOCKED. Финальная готовность не заявляется, потому что 14.04 не PASS и 14.12 не закрыт.

## Что было сделано

- Найдены реальные установленные приложения Claude, Cursor, Codex и Visual Studio Code через `Get-StartApps`.
- Найдены живые окна Claude, Cursor и Codex через `Get-Process`.
- Для каждого хоста создан fresh correlation id.
- Для каждого хоста выполнен same-run вызов установленного `codex-xiigen-smart-skills` adapter/core route через `src/codex-entrypoint.js`.
- Для Cursor открыт свежий реальный Cursor window со scratch markdown-файлом, где виден correlation id и bounded result marker.
- Для Codex использовалась текущая Codex app/sub-agent visible surface. Сделаны попытки вывести свежий marker на экран через foreground, `Ctrl+End`, click+wheel. Marker в скриншот не попал.
- Для Claude внешний/private Claude chat не использовался. Локальный `claude` command в этом запуске не найден. Окно Claude было обнаружено, но safe local Claude Code/runtime surface с marker не был доступен.

## Статус по хостам

| Host | Status | Tier | Что видно на screenshot | Что доказывает trace | Что не доказано |
|---|---|---|---|---|---|
| Claude | PARTIAL/BLOCKED | Tier C supporting only | Есть private raw screenshot окна Claude и redacted review-safe copy, но нет safe visible correlation id/result marker. | `raw/claude/request.json`, `response.json`, `trace.json` показывают same-run adapter/core boundary с `P14-FIX-09-CLAUDE-20260624T030843Z`, `response_success=true`, external AI не использовался. | Не доказан Claude UI/runtime visible result. Не использован внешний Claude chat. Нет локального `claude` command. |
| Cursor | DONE_WITH_SCOPE | Tier B | `raw/cursor/window-cursor-visible-marker.png` показывает реальное окно Cursor со scratch markdown и `P14-FIX-09-CURSOR-20260624T030843Z`. | `raw/cursor/request.json`, `response.json`, `trace.json` показывают same-run adapter/core boundary с тем же correlation id и успешным response. | Trace доказывает core boundary через заданный adapter route, а не отдельную native Cursor extension implementation. |
| Codex | PARTIAL/BLOCKED | Tier C supporting only | Private raw screenshots показывают Codex app window, но свежий `P14-FIX-09-CODEX-20260624T030843Z` не виден. Review-safe copies сохранены с редактированием private content. | `raw/codex/request.json`, `response.json`, `trace.json` показывают same-run installed adapter/core boundary с `P14-FIX-09-CODEX-20260624T030843Z`, `response_success=true`, external AI не использовался. | Не доказан Codex visible UI result, потому что marker не удалось вывести в captured UI surface. Codex CLI не использовался. |

## Итоговые ворота

| Gate | Verdict | Причина |
|---|---|---|
| 14.04 overall | NEEDS_FIX | PASS разрешен только если Claude, Cursor и Codex имеют Tier A или Tier B visible result/correlation плюс same-run trace. Это выполнено только для Cursor. |
| 14.12 final readiness | NOT_CLOSED | 14.04 не PASS, поэтому финальная готовность не закрыта. |
| final_readiness_claimed | false | Финальная готовность не заявлялась. |

## Evidence files

Discovery:

- `raw/discovery/start-apps.txt`
- `raw/discovery/process-windows.txt`
- `raw/discovery/claude-command.txt`

Claude:

- `raw/claude/request.json`
- `raw/claude/response.json`
- `raw/claude/trace.json`
- `raw/claude/adapter-stdout.txt`
- `raw/claude/adapter-stderr.txt`
- `raw/claude/private-window-claude-no-marker-attempt.png`
- `raw/claude/review-safe-window-claude-redacted.png`

Cursor:

- `raw/cursor/request.json`
- `raw/cursor/response.json`
- `raw/cursor/trace.json`
- `raw/cursor/adapter-stdout.txt`
- `raw/cursor/adapter-stderr.txt`
- `raw/cursor/P14-FIX-09-CURSOR-20260624T030843Z-PASS.md`
- `raw/cursor/window-cursor-visible-marker.png`
- `raw/cursor/desktop-after-cursor-foreground.png`

Codex:

- `raw/codex/request.json`
- `raw/codex/response.json`
- `raw/codex/trace.json`
- `raw/codex/adapter-stdout.txt`
- `raw/codex/adapter-stderr.txt`
- `raw/codex/private-window-codex-visible-marker-attempt.png`
- `raw/codex/private-window-codex-after-ctrl-end.png`
- `raw/codex/private-window-codex-after-click-wheel.png`
- `raw/codex/review-safe-window-codex-redacted.png`
- `raw/codex/review-safe-window-codex-after-ctrl-end-redacted.png`
- `raw/codex/review-safe-window-codex-after-click-wheel-redacted.png`

## Blockers and next actions

Claude next action: открыть безопасную local Claude Code/runtime surface, где можно показать `P14-FIX-09-CLAUDE-*` без отправки во внешний Claude chat, затем снять screenshot и связать его с same-run trace.

Codex next action: обеспечить видимость текущего Codex sub-agent marker в captured Codex surface. В этом запуске текущая Codex app показывала другой участок thread, а `Ctrl+End` и click+wheel не вывели fresh marker на экран. Нужен ручной/технический способ открыть текущую нижнюю часть этого thread или другой безопасный visible Codex runtime surface с тем же correlation id.

Cursor next action: не требуется для этого среза; Cursor row закрыт как DONE_WITH_SCOPE.

## Claim limits

- adapter/core boundary executed: true for Claude, Cursor and Codex traces.
- full Codex chat UI proof: false.
- token savings proven: false.
- semantic quality improvement proven: false.
- learnability proven: false.
- training started: false.
- eval started: false.
- checkpoint mutation started: false.
