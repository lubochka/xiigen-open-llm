# Claude Path B route-preflight

## Человеческий итог

Статус: `CLAUDE_ROUTE_READY` для узкого route-preflight.

Я проверял только локальный маршрут входа для `frdp-p01-claude-full-extension-entrypoint`: есть ли на этом компьютере локальная поверхность Claude и виден ли в ней XIIGen route/entrypoint без отправки prompt во внешний Claude.

Что видно по evidence:

- Claude Desktop установлен как `Claude_1.15962.0.0_x64__pzs8sxrjxfjjc`.
- На рабочем столе есть видимое окно `Claude`, не свернутое, размером 1216x677.
- Redacted UI Automation summary для этого окна показывает Claude/chat/auth-related категории и точные XIIGen labels, включая `Idle XIIGen Smart Skills UI proof run`, `llm_mvp_core, xiigen-mvp, xiigen-planning`, `xiigen mvp`, `xiigen flows`, `xiigen platform`.
- Prompt не отправлялся: `prompt_sent=false`.
- Screenshot не снимался, потому что окно может содержать приватные чаты или account context. Доказательство route visibility здесь основано на redacted UI Automation labels/categories, а не на bitmap screenshot.

Что доказано:

- Локальная Claude surface найдена.
- Локальное окно Claude найдено и видимо.
- XIIGen route/entrypoint виден или доказан в локальном Claude window через redacted accessibility evidence.
- Этот packet не использовал старый readiness gate.

Что остается:

- Не доказан ответ Claude на сценарный prompt.
- Не доказана core/adapter correlation для отправленного Claude UI prompt, потому что prompt не отправлялся.
- Не доказаны token savings, semantic quality, training, DPO, checkpoint promotion или locator mutation.
- Следующий минимальный шаг: отдельным bounded work order выполнить no-secret Path B scenario execution или сначала снять review-safe cropped screenshot, если Луба захочет bitmap evidence перед submit.

## Evidence files

- `result.json` - итоговый machine-readable verdict.
- `action-log.md` - человеческий лог шагов.
- `logs/inspection-summary.json` - итог runner inspection.
- `logs/window-inventory.json` - Claude process/window inventory.
- `logs/uia-redacted-summary.json` - redacted UI Automation summary.
- `logs/appx-discovery.json` - package discovery.
- `logs/command-discovery.json` - command discovery.
- `logs/runner-stdout.log` and `logs/runner-stderr.log` - runner execution capture.
- `run/claude-route-preflight-inspect.ps1` - scoped inspection runner used for this packet.

## Redaction note

Raw accessibility names were not stored except exact XIIGen labels that were needed to prove the route. Other matching UI labels are category-only redacted. No screenshot was captured, and no credential, account identity, chat content, or prompt text was intentionally stored.
