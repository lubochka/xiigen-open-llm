# Row19 Path A adapter/core corrected evidence

Статус: ROW19_PATH_A_CORRECTED_NEEDS_FIX

Папка run: C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-product-readiness-product-workflow\run-20260626T143633Z
Path A папка: C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-product-readiness-product-workflow\run-20260626T143633Z\path-a-adapter

## Что выполнено

Выполнен только Path A adapter/core smoke через установленный Codex XIIGen plugin adapter. Команда вызвала C:/Users/lubao/.codex/plugins/cache/xiigen-local/codex-xiigen-smart-skills/0.1.0+codex.runtime04e-20260616-231500/src/codex-entrypoint.js, который создал request.json и вызвал C:/Projects/llm_mvp_core/tools/xiigen-extension-core-api.ps1. Codex UI Path B не запускался. Git, code edits, training, eval, DPO, checkpoint/locator mutation и production locator update не выполнялись.

Correlation id из prep: ROW19-CODEX-PRODUCT-WORKFLOW-001.
Scenario id из prep: frdp-p01-codex-product-readiness-product-workflow.

## Команды

1. node ./src/codex-entrypoint.js with argv array from logs/row19-path-a-corrected-runner.mjs
   - cwd: C:/Users/lubao/.codex/plugins/cache/xiigen-local/codex-xiigen-smart-skills/0.1.0+codex.runtime04e-20260616-231500
   - exit code: 0
   - output: C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-product-readiness-product-workflow\run-20260626T143633Z\path-a-adapter\adapter-command-output.txt
2. powershell.exe -NoProfile -ExecutionPolicy Bypass -Command <default Get-Content prompt hash probe>
   - output: C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-product-readiness-product-workflow\run-20260626T143633Z\path-a-adapter\logs\powershell-default-json-read-hash.json

## Prompt/hash identity

- Prep prompt canonical UTF-8 hash: 858fad70c310395640bbf0c0ab4aa04c78da2a03b3eb2684db4a6fc2e61996eb
- Request prompt canonical UTF-8 hash: 858fad70c310395640bbf0c0ab4aa04c78da2a03b3eb2684db4a6fc2e61996eb
- Prep == request exact text: true
- Response prompt hashes found: 858fad70c310395640bbf0c0ab4aa04c78da2a03b3eb2684db4a6fc2e61996eb
- Response hashes match prep/request: true
- Trace user_prompt_sha256: 8d8687c5e8f9f75ac1afc6108523693873f0f7952ed32495694f1ca937b5dc8e
- Trace hash matches prep/request/response: false
- PowerShell default Get-Content probe: 8d8687c5e8f9f75ac1afc6108523693873f0f7952ed32495694f1ca937b5dc8e (length 1983)
- Canonical Node/dotnet prompt length: 1545

Вывод: это не другой prompt field by design. Trace hash берется из того же request JSON поля UserPromptOrTaskText, но core PowerShell script читает JSON через Get-Content -Raw без -Encoding UTF8. Для UTF-8 request.json без BOM и не-ASCII prompt это дает другую decoded string перед trace hash. Dotnet core response читает request как UTF-8, поэтому response hash совпадает с prep/request. Полная prep/request/response/trace prompt identity не доказана.

## Output contract

Response output contract status: not_proven.
Причина: adapter/core response является redacted baseline core response. Он показывает prompt->task, task->plan и task/plan->skills evidence, но не возвращает полный русский user-visible contract со всеми полями scenario_id, normalized_task, task_plan_steps, mutation_flags, non_claims и final_user_visible_response как отдельный финальный ответ.

## Selected/planned skills

- Requested skills: xiigen-smart-skills
- Available skills: xiigen-smart-skills
- Response selected skills: xiigen-smart-skills
- client-adaptive-learning-loop: not_proven

client-adaptive-learning-loop остается not_proven: он есть в exact prompt, но adapter/core response выбрал только xiigen-smart-skills и не доказал выбор или планирование client-adaptive-learning-loop.

## Что доказано узко

- request.json, response.json, trace.json и adapter-command-output.txt созданы в C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-product-readiness-product-workflow\run-20260626T143633Z\path-a-adapter.
- Adapter/core boundary выполнен: trace указывает tools/xiigen-extension-core-api.ps1 и SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest.
- Hidden external AI usage, training и host runtime claim в trace не отмечены.

## Что не доказано

- Полная prompt identity через trace hash не доказана.
- Response output contract: not_proven.
- client-adaptive-learning-loop: not_proven.
- Path B Codex UI: not_proven, не запускался.
- Token savings, semantic quality improvement, learnability, training/eval/DPO, checkpoint mutation/promotion, locator update, production locator update, full Codex chat UI и product readiness: not_proven.

## Narrow claim

Path A invoked the installed Codex XIIGen plugin adapter/core and produced request/response/trace, but corrected evidence remains NEEDS_FIX because trace prompt hash identity does not match the canonical prep/request/response prompt hash. No product readiness or host UI readiness is claimed.

Это подготовка/маршрутизация Row19; readiness не доказана без dual-path same-scenario evidence.
