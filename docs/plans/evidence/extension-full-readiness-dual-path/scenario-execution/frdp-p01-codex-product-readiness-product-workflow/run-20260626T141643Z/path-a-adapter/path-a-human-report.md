# Row19 Path A adapter/core report

Статус: ROW19_PATH_A_ADAPTER_CORE_PASS_REVIEW_REQUIRED

Папка run: $runRoot
Path A папка: $pathARoot

## Что выполнено

Выполнен только Path A: установленный Codex XIIGen plugin adapter/core smoke через src/codex-entrypoint.js, который вызвал C:/Projects/llm_mvp_core/tools/xiigen-extension-core-api.ps1. Codex UI Path B не запускался.

Использован correlation id: ROW19-CODEX-PRODUCT-WORKFLOW-001.
Prompt в финальном
equest.json совпадает с exact_same_scenario_prompt из prep JSON: $samePrompt.

## Команды и попытки

1. Attempt 1: PowerShell native
ode .\src\codex-entrypoint.js ... завершился exit 0, но не принят как pass: PowerShell обрезал prompt на внутренней кавычке и сломал JSON-аргументы. Evidence сохранен в logs/attempt1-*.
2. Attempt 2: Node wrapper сохранил exact prompt, но передал mutation_allowed:false boolean; core endpoint упал на HostFacts string dictionary contract. Evidence сохранен в logs/attempt2-*.
3. Attempt 3: Node wrapper запустил
ode ./src/codex-entrypoint.js с prompt из prep JSON и mutation_allowed:"false" как string host fact. Exit code 0; текущие primary artifacts являются финальными для проверки.

## Файлы

-
equest.json
-
esponse.json
- 	race.json
- dapter-command-output.txt
- path-a-validation.json
- path-a-human-report.md
- logs/attempt1-*
- logs/attempt2-*

## Что доказано

- Installed skill surface xiigen-smart-skills был загружен и использован как requested/available skill surface.
- Adapter/core boundary выполнен: trace показывает 	ools/xiigen-extension-core-api.ps1 и SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest.
-
equest.json,
esponse.json, 	race.json, command output созданы и непустые.
- Exact prompt сохранен в финальном request.
- Response non-empty, redacted/debug-safe: raw prompt redacted, secrets not reported.
- Prompt->task, task->plan, task/plan->skills evidence exposed в
esponse.json как baseline/internal projection.

## Что не доказано

- client-adaptive-learning-loop есть в exact prompt, но response выбрал только xiigen-smart-skills; поэтому client-adaptive-learning-loop = not_proven.
- Full Codex chat UI не доказан, потому что Path B запрещен этим work order и не запускался.
- Token savings, semantic quality improvement, learnability, training/eval/DPO, checkpoint mutation/promotion, locator update и production locator update не доказаны.
- Product readiness не доказана.

## Claim boundary

Н narrow claim: Path A proves only installed Codex XIIGen plugin adapter/core boundary execution for the exact Row19 prompt and correlation id. It does not prove Path B UI, product readiness, token savings, semantic quality improvement, learnability, training, eval, checkpoint promotion, locator update, or full Codex chat/app readiness.

Это подготовка/маршрутизация Row19; readiness не доказана без dual-path same-scenario evidence.
