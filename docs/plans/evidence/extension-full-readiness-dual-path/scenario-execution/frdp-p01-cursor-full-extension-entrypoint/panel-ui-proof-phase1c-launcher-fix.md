# FRDP-P01 Cursor Panel UI Proof Phase 1C Launcher Fix

Status: `ACCEPTABLE_EVIDENCE`

CURRENT TASK: исправить запуск Cursor panel proof так, чтобы локальный Extension Host либо писал `frdp-p01-cursor-panel-ui-proof-validation.json`, либо оставлял точный failure/log packet.
NOT THE TASK: готовность продукта, полная готовность Cursor, full joined pass, Claude/Codex, governance/skills/plans, git stage/commit/push.
EVIDENCE-ONLY: предыдущий `run-20260624T220700` использован только как blocker evidence.

## Что было сломано

Предыдущий прямой запуск `Cursor.exe` завершился с shell exit code `0`, но не создал `frdp-p01-cursor-panel-ui-proof-validation.json`. Это значит, что `exit 0` доказывал только bootstrap/GUI process behavior, а не выполнение Extension Host test harness.

## Что изменено

- `frdp-p01-cursor-panel-ui-proof.js`: добавлены отдельные `harness-started` и `harness-failure` JSON markers. Реальный validation JSON теперь пишется только после успешной проверки всех panel fields.
- `frdp-p01-cursor-panel-ui-proof-launcher.js`: добавлен локальный launcher, который создает fresh run folder, запускает Cursor один раз, пишет stdout/stderr logs, ждет validation JSON, и классифицирует missing validation как `NEEDS_FIX` или `BLOCKED_WITH_NEXT_ACTION`.

## Результат rerun

Run folder:

```text
docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-cursor-full-extension-entrypoint/panel-ui-proof-phase1c/run-20260624T221604
```

Команда `node frdp-p01-cursor-panel-ui-proof-launcher.js` вернула:

```text
ACCEPTABLE_EVIDENCE
```

Ключевые артефакты:

- `frdp-p01-cursor-panel-ui-proof-launch-summary.json`
- `frdp-p01-cursor-panel-ui-proof-validation.json`
- `frdp-p01-cursor-panel-ui-proof-harness-started.json`
- `cursor-panel-ui-proof.stdout.log`
- `cursor-panel-ui-proof.stderr.log`

## Проверенные поля

- command invoked: `xiigenSmartSkills.panel`
- marker: `FRDP-P01-frdp-p01-cursor-full-extension-entrypoint`
- correlation: `FRDP-P01-frdp-p01-cursor-full-extension-entrypoint-extension-host-no-send`
- same marker in prompt: `true`
- same marker in correlation: `true`
- same shared path as `runtimeProof.invoke`: `true`
- visible/panel text non-empty: `true`
- online prompt sent by harness: `false`
- product readiness claimed: `false`

## Команды

- `node --check frdp-p01-cursor-panel-ui-proof.js`: PASS
- `node --check frdp-p01-cursor-panel-ui-proof-launcher.js`: PASS
- `node frdp-p01-cursor-panel-ui-proof-launcher.js`: PASS, `ACCEPTABLE_EVIDENCE`
- `Get-CimInstance Win32_Process` filtered by `run-20260624T221604`: PASS, matching Cursor processes `0`

## Что это доказывает

Доказано узко: команда `xiigenSmartSkills.panel` была реально вызвана из локального Cursor Extension Host, прошла через общий runtime-compatible evidence path, записала panel `hostPanelUiRef`, сохранила тот же marker/correlation и дала непустой visibleText/panel text.

Не доказано: full Cursor readiness, visible desktop UI/render proof, product readiness, joined pass, semantic quality, token savings, online AI behavior.

## Scenario Counter Effect

- Total: `28`.
- Full joined pass before/after: unchanged. Причина: этот run дает JSON/Extension Host panel evidence, но не отдельное visible UI/render proof.
- Partial before/after: локально для scenario 11 состояние launcher blocker перешло в acceptable narrow panel evidence. Canonical global partial count этим sub-agent не редактировался.
- Not moved before/after: unchanged; master counter file не редактировался.
- Blocked before/after: локальный blocker `run-20260624T220700` снят для panel launch harness. Canonical global blocked count этим sub-agent не редактировался.

Git: stage/commit/push не выполнялись.
