# Claude scenario coverage current state

## Человеческий итог

Проверен один сценарий Claude из prep-пакета `claude-full-extension-runtime-20260627-223159`.

Path A: `ACCEPTED_DIRECT_CLAUDE_API_CORE_BOUNDARY_FALLBACK`.

Fix-loop rerun был выполнен. Канонические `request.json`, `response.json`, `trace.json` теперь относятся к rerun, где request и trace содержат `HostName=Claude` и `HostType=claude`. Core ответил успешно, `core_boundary_executed=true`.

Граница утверждения по Path A: это direct local API/core boundary fallback для Claude scenario. Это не live Claude extension host и не UI-доказательство.

Path B: `BLOCKED_ROUTE_NOT_PROVEN`.

Safe preflight ранее нашёл один candidate window процесса `claude`, но корректный thread/project/extension route не был доказан. Prompt не вводился, submit не нажимался, screenshots не создавались.

Оставшееся действие: отдельный Claude UI route/session pass с явным route, screenshot before submit, submit, screenshot after response, и correlation note без секретов.

## Evidence

- `request.json`: corrected Claude-host request with `HostName=Claude`, `HostType=claude`.
- `response.json`: corrected core response.
- `trace.json`: corrected trace with `host_name=Claude`, `host_type=claude`.
- `window-inventory.json`: safe local window preflight, raw titles were not persisted.
- `logs/core-api-claude-host.stdout.txt`: stdout for corrected direct core API rerun.
- `logs/core-api-claude-host.stderr.txt`: stderr for corrected direct core API rerun.
- `logs/adapter-core.stdout.txt`: historical stdout for the earlier Codex-host adapter run.
- `logs/adapter-core.stderr.txt`: historical stderr for the earlier Codex-host adapter run.
- `action-log.md`: action and downgrade history.
- `redaction-log.md`: redaction notes.

## Исправление mismatch

Parent review correctly found that the previous canonical request had `HostName=Codex` and `HostType=codex`, while only `HostFacts.host=Claude`. That previous state is not accepted as clean Claude Path A.

The fix-loop replaced canonical `request.json`, `response.json`, and `trace.json` with a direct core API rerun using `HostName=Claude` and `HostType=claude`.

## Доказано

1. Prep для Claude существует.
2. Existing Claude execution scan did not find an accepted same-scenario Path A response capture before this packet.
3. Corrected direct Claude API/core boundary fallback ran with exit code 0.
4. Corrected request and trace contain `Claude/claude` host fields.
5. Core response success is true.
6. Safe window inventory found one candidate process `claude`, without typing prompt or capturing secrets.

## Не доказано

1. Live Claude extension host.
2. Видимый Claude UI answer.
3. Корректный Claude thread/project/extension route.
4. Token savings, quality improvement, learnability, training, eval, checkpoint mutation.
