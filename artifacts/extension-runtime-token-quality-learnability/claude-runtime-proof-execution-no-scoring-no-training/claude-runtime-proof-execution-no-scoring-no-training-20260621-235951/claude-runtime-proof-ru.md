# Claude runtime proof

Вердикт: PASS для ограниченного Claude plugin/hook -> llm_mvp_core core/API proof. Это не full interactive Claude UI/chat proof и не readiness всего расширения.

## Что запускалось

- Claude CLI validation: `C:/Users/lubao/AppData/Roaming/Claude/claude-code/2.1.181/claude.exe plugin validate .` из `C:/Users/lubao/Documents/xiigen/extensions reasearch/claude-xiigen-smart-skills`. Validation прошла.
- Positive hook command: `powershell.exe -NoProfile -ExecutionPolicy Bypass -File "C:\Users\lubao\Documents\xiigen\extensions reasearch\claude-xiigen-smart-skills\src\claude-entrypoint.ps1" -ConcreteTask <redacted> -Prompt <redacted> -CheckSetPath "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\claude-runtime-proof-execution-no-scoring-no-training\claude-runtime-proof-execution-no-scoring-no-training-20260621-235951\positive-check-set.input.json" -CoreRoot "C:\Projects\llm_mvp_core" -OutputPath "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\claude-runtime-proof-execution-no-scoring-no-training\claude-runtime-proof-execution-no-scoring-no-training-20260621-235951\positive-entrypoint-response.json" -TracePath "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\claude-runtime-proof-execution-no-scoring-no-training\claude-runtime-proof-execution-no-scoring-no-training-20260621-235951\positive-entrypoint-trace.json"`.
- Negative hook command: `powershell.exe -NoProfile -ExecutionPolicy Bypass -File "C:\Users\lubao\Documents\xiigen\extensions reasearch\claude-xiigen-smart-skills\src\claude-entrypoint.ps1" -ConcreteTask '' -Prompt '' -CheckSetPath "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\claude-runtime-proof-execution-no-scoring-no-training\claude-runtime-proof-execution-no-scoring-no-training-20260621-235951\negative-check-set.input.json" -CoreRoot "C:\Projects\llm_mvp_core" -OutputPath "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\claude-runtime-proof-execution-no-scoring-no-training\claude-runtime-proof-execution-no-scoring-no-training-20260621-235951\negative-entrypoint-response.json" -TracePath "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\claude-runtime-proof-execution-no-scoring-no-training\claude-runtime-proof-execution-no-scoring-no-training-20260621-235951\negative-entrypoint-trace.json"`.
- Claude plugin manifest: `C:/Users/lubao/Documents/xiigen/extensions reasearch/claude-xiigen-smart-skills/.claude-plugin/plugin.json`. В нем hook `SessionStart` указывает на `src/claude-entrypoint.ps1`.
- Core API path: `C:/Projects/llm_mvp_core/tools/phase40-extension-core-api.ps1`.

## Почему это Claude path

Запуск не был dashboard/API-only симуляцией: локальный `claude.exe plugin validate .` распознал Claude plugin manifest, а positive сценарий запускал тот же PowerShell hook entrypoint, который объявлен в Claude plugin manifest. Entrypoint сформировал `Phase40ExtensionApiRequest`, вызвал core-owned `llm_mvp_core` API script и получил fresh response. При этом Claude chat/model invocation не запускался, поэтому full UI/chat runtime не заявлен.

## Positive request

- Input: redacted small Claude XIIGen proof prompt с `run_id=claude-runtime-proof-20260621-235951`, без private data.
- Output: `positive-entrypoint-response.json` показывает `status=core_invocation_completed`, `passed=true`, `runtimeEvidence=true`, `coreExitCode=0`.
- Core output: `positive-core-response.snapshot.json` показывает `passed=true`, `runtimeEvidence=true`, `evidenceScope=core_api_check_set_runtime_not_full_host`, check ids `positive-core-boundary` и `positive-trace-refs`.

## Negative request

- Input: redacted missing required field: пустой `ConcreteTask` и пустой `Prompt`.
- Output: command exited with code `1` and surfaced a local missing-argument error for `ConcreteTask` before core invocation.
- Это доказывает safe error path без side effects; core-level negative validation для unsupported method не доказан, потому что текущий Claude hook wrapper не принимает method field и останавливает missing `ConcreteTask` на параметрах.

## Negative artifact situation

`negative-entrypoint-response.json` не существует, потому что negative scenario остановился на required-argument validation для `ConcreteTask` до выполнения тела entrypoint и до core invocation. Фактические negative output evidence artifacts: `negative-command-stderr.txt`, `negative-command-stdout.txt`, `negative-request-trace.json`. Runtime artifact не был создан и не подменяется искусственным файлом.

## Correlation

- Same run id: `claude-runtime-proof-20260621-235951`.
- Positive correlation id: `claude-runtime-proof-20260621-235951:positive`.
- Negative correlation id: `claude-runtime-proof-20260621-235951:negative`.

## Fresh artifacts

Fresh hook/core artifacts written under `C:/Projects/llm_mvp_core/artifacts/extension-runtime-token-quality-learnability/claude-runtime-proof-execution-no-scoring-no-training/claude-runtime-proof-execution-no-scoring-no-training-20260621-235951`:

- `positive-entrypoint-response.json`
- `positive-entrypoint-trace.json`
- `positive-core-request.snapshot.json`
- `positive-core-response.snapshot.json`
- `phase40-claude-core-request.json`
- `phase40-claude-core-response.json`
- `negative-command-stderr.txt` and `negative-command-stdout.txt`

## RequestedSkills / AvailableSkills

Strict `RequestedSkills`/`AvailableSkills` binding is not applicable to the current Claude Phase40 hook wrapper: `src/claude-entrypoint.ps1` emits `skillSelectionRequest.checkSetItems`, not the newer `XiigenExtensionRequest.RequestedSkills` / `AvailableSkills` arrays. Equivalent binding proven in this run: redacted hook `skillSelectionRequest.checkSetItems` reached `llm_mvp_core` and returned matching `checkSetResults` for the same run.

## No side effects

No git, scoring, eval quality comparison, training, DPO, model mutation, checkpoint promotion, locator/target mutation, Cursor proof, Codex proof, dotnet build/test, external AI, web, or Claude chat/model call was performed.

## Что остается недоказанным

Scope phrases for human review:

- не доказывает token savings;
- не доказывает улучшение качества;
- не доказывает learnability;
- не доказывает semantic quality;


- token savings;
- quality improvement;
- learnability;
- semantic quality;
- model readiness;
- checkpoint promotion;
- full interactive Claude UI/chat runtime;
- Cursor runtime proof;
- Codex runtime proof;
- strict RequestedSkills/AvailableSkills arrays through the current Claude wrapper.
