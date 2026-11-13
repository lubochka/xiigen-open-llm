# Row19 Path A human report

Статус отчета: `ROW19_PATH_A_REPORT_CLEANUP_DONE_REVIEW_REQUIRED`.
Статус исходного Path A evidence: `ROW19_PATH_A_RERUN_PASS_REVIEW_REQUIRED`.

## Область доказательства

Доказан только newest Path A adapter/core boundary run через установленный Codex XIIGen plugin. Это не UI proof, не joined proof и не product readiness.

Run folder: `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-product-readiness-product-workflow\run-20260626T145202Z`

Path A folder: `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-product-readiness-product-workflow\run-20260626T145202Z\path-a-adapter`

## Cleanup note

Этот текстовый cleanup не перезапускал adapter. Командная сводка ниже взята из `logs\run-command.json` в этом Path A folder. Полный пользовательский prompt не раскрыт в human report; вместо него указан явный redaction note с sha256 из run manifest.

## Прочитанные инструкции и входные данные, зафиксированные в validation

- Skill: `C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500\skills\xiigen-smart-skills\SKILL.md`
- Prep JSON: `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-product-readiness-product-workflow\row19-prep-codex-product-workflow.json`
- Adapter entrypoint: `C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500\src\codex-entrypoint.js`
- Core API script: `C:\Projects\llm_mvp_core\tools\xiigen-extension-core-api.ps1`

## Командная сводка newest Path A

- `cwd`: `C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500`
- Executable: `C:\Program Files\nodejs\node.exe`
- Entrypoint: `C:\Users\lubao\.codex\plugins\cache\xiigen-local\codex-xiigen-smart-skills\0.1.0+codex.runtime04e-20260616-231500\src\codex-entrypoint.js`
- Method: `prompt-to-task`
- Prompt argument: raw prompt intentionally redacted in the run manifest; manifest records `prompt_sha256=858fad70c310395640bbf0c0ab4aa04c78da2a03b3eb2684db4a6fc2e61996eb`.
- Host facts JSON: `{"surface":"installed-codex-skill","host":"Codex","scope":"ROW19","workspace":"C:\\Projects\\llm_mvp_core","local_user_model_root":"C:\\Projects\\llm_mvp_core","mutation_allowed":"false"}`
- Requested skills JSON: `["xiigen-smart-skills"]`
- Available skills JSON: `["xiigen-smart-skills"]`
- Check-set inputs JSON: `{"row19":"codex-product-workflow-prep","non_mutating":true}`
- Provenance JSON: `{"source":"installed-plugin-skill","plugin":"codex-xiigen-smart-skills","scenario":"frdp-p01-codex-product-readiness-product-workflow"}`
- Correlation id: `ROW19-CODEX-PRODUCT-WORKFLOW-001`
- Debug enabled: `true`
- Core root: `C:\Projects\llm_mvp_core`
- Extension root: `.`
- Request path: `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-product-readiness-product-workflow\run-20260626T145202Z\path-a-adapter\request.json`
- Response path: `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-product-readiness-product-workflow\run-20260626T145202Z\path-a-adapter\response.json`
- Trace path: `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-codex-product-readiness-product-workflow\run-20260626T145202Z\path-a-adapter\trace.json`
- Started UTC: `2026-06-26T14:52:02.055Z`
- Completed UTC: `2026-06-26T14:52:07.466Z`
- Exit code: `0`
- Signal: `null`
- Spawn error: `null`
- Host facts note: `mutation_allowed` был передан строкой `false`, чтобы соответствовать строковому контракту core HostFacts; scenario prompt и correlation id не менялись.

## Предыдущая failed attempt из validation context

Validation также фиксирует предыдущую попытку в `run-20260626T145040Z`: exit code `1`, semantic execution не начался, blocker был `System.Text.Json.JsonException` из-за JSON boolean `HostFacts.mutation_allowed`, который core не смог преобразовать в `System.String`. Точный список аргументов той попытки не восстанавливался в этом cleanup, потому что разрешенный newest manifest `logs\run-command.json` относится к run `20260626T145202Z`.

## Проверка prompt/hash

- Scenario id: `frdp-p01-codex-product-readiness-product-workflow`
- Correlation id: `ROW19-CODEX-PRODUCT-WORKFLOW-001`
- Prep canonical UTF-8 prompt sha256: `858fad70c310395640bbf0c0ab4aa04c78da2a03b3eb2684db4a6fc2e61996eb`
- Request prompt sha256: `858fad70c310395640bbf0c0ab4aa04c78da2a03b3eb2684db4a6fc2e61996eb`
- Trace user_prompt_sha256: `858fad70c310395640bbf0c0ab4aa04c78da2a03b3eb2684db4a6fc2e61996eb`
- Response contains canonical prompt hash: `true`
- Prep/request exact prompt text match: `true`
- Verdict: `PASS`

## Adapter/core result

- Response Success: `true`
- Response Status: `core_boundary_partial_no_semantic_quality_claim`
- Trace endpoint: `xiigen-extension-core-api`
- Trace call target: `SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest`
- Trace claim scope: `core-owned endpoint invocation only; no host runtime claim`
- External hidden AI used: `false`
- Training started: `false`

## Selected/planned skills

- Requested skills: `xiigen-smart-skills`
- Available skills: `xiigen-smart-skills`
- Response selected skills: `xiigen-smart-skills`
- `client-adaptive-learning-loop`: `not_proven`

## Non-claims

- Row19 accepted: `false`
- Path B run: `false`
- product readiness: `not_proven`
- full Codex chat/UI readiness: `not_proven`
- full joined pass: `not_proven`
- token savings: `not_proven`
- semantic quality improvement: `not_proven`
- learnability: `not_proven`
- training/eval/DPO: `not_started`
- checkpoint mutation/promotion: `not_started`
- locator mutation/update: `not_started`
- git/code edits: `not_started`

## Узкая человеческая формулировка

Newest Path A adapter/core boundary run через установленный Codex XIIGen plugin выполнен до exit code `0`, request/response/trace/log manifest сохранены, UTF-8 prompt hash совпал между prep, request, trace и response hash evidence. Это только adapter/core evidence. Row19 остается не accepted и требует review; Path B не запускался; UI/product readiness, joined readiness и output contract для полного Row19 не доказаны; `client-adaptive-learning-loop` остается `not_proven`.
