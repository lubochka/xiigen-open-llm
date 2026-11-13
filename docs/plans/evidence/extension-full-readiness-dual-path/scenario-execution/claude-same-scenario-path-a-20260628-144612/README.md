# Claude same-scenario Path A evidence

## Коротко

Этот пакет фиксирует только автоматический Path A для Claude по тому же сценарию, который уже был проверен вручную в Path B.

Использованный локальный путь:

1. `C:\Projects\claude-xiigen-smart-skills\src\claude-entrypoint.js`
2. `C:\Projects\llm_mvp_core\tools\xiigen-extension-core-api.ps1`
3. `SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest`

Команда завершилась с exit code `0`. Ответ `response.json` содержит `Success=true`, статус `core_boundary_partial_no_semantic_quality_claim`, выбранные навыки `xiigen-smart-skills`, `api-documentation`, `planning`, и видимый ответ:

```text
Core task+plan+skills ready. Prompt-to-task produced task type deliverable_task with 1 deliverable refs. Skills: xiigen-smart-skills, api-documentation, planning. Semantic quality is not claimed.
```

## Точный prompt

```text
XIIGen Smart Skills proof: translate this user request into a universal task, choose relevant skills, and return a short visible answer. Request: create a safe plan for documenting an API dashboard.
```

## Что это закрывает

Этот пакет закрывает только узкое доказательство: Claude thin adapter смог локально передать тот же prompt в core API, core API выполнил `prompt-to-task`, вернул task/plan/skills, и трасса зафиксировала границу adapter/API/core.

## Что это не доказывает

Это не доказывает full Claude readiness, host UI readiness, token savings, semantic quality, learning, training, DPO, checkpoint promotion, checkpoint mutation, locator update, or external-AI behavior.

## Основные файлы

- `response.json` - ответ core API.
- `trace.json` - trace core endpoint.
- `xiigen-claude-host-trace.json` - trace Claude adapter boundary.
- `xiigen-claude-core-request.json` - request payload; содержит raw prompt по контракту адаптера.
- `stdout.log` / `stderr.log` - вывод команды.
- `result.json` - статус пакета и narrow claim.
- `command-log.md` - команды, exit codes, сводка вывода.
- `redaction-note.md` - правила и результаты redaction check.
