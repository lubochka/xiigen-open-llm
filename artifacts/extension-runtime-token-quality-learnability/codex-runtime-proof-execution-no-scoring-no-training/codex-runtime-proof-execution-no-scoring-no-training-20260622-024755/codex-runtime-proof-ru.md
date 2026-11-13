# Codex runtime proof

Вердикт: PASS.

## Запущенные команды и пути

Positive command:

```powershell
cd "C:/Users/lubao/.codex/plugins/cache/xiigen-local/codex-xiigen-smart-skills/0.1.0+codex.runtime04e-20260616-231500"
node .\src\codex-entrypoint.js `
  --method prompt-to-task `
  --user-prompt-or-task-text "[REDACTED Codex installed skill smoke task]" `
  --host-facts-json '{"surface":"installed-codex-skill","scope":"RUNTIME-04E","scenario":"positive_request","dashboard_only_shortcut":"false"}' `
  --requested-skills-json '["xiigen-smart-skills"]' `
  --available-skills-json '["xiigen-smart-skills"]' `
  --check-set-inputs-json '{"runtime04e":"installed-skill-smoke","codex_runtime_proof_scenario":"positive_request","codex_requested_skills_bound":"true","codex_available_skills_bound":"true","qsg19_target_locator_required":"false"}' `
  --provenance-json '{"source":"installed-plugin-skill","plugin":"codex-xiigen-smart-skills","work_order":"DOCS-QA-CODEX-RUNTIME-PROOF-EXECUTION-NO-SCORING-NO-TRAINING-01"}'
  --correlation-id "codex-runtime-proof-execution-no-scoring-no-training-20260622-024755-positive"
  --debug-enabled `
  --core-root "C:\Projects\llm_mvp_core" `
  --extension-root "." `
  --request-path "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\codex-runtime-proof-execution-no-scoring-no-training\codex-runtime-proof-execution-no-scoring-no-training-20260622-024755\positive\request.json"
  --response-path "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\codex-runtime-proof-execution-no-scoring-no-training\codex-runtime-proof-execution-no-scoring-no-training-20260622-024755\positive\response.json"
  --trace-path "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\codex-runtime-proof-execution-no-scoring-no-training\codex-runtime-proof-execution-no-scoring-no-training-20260622-024755\positive\trace.json"
```

Preliminary negative empty-input attempt:

```powershell
cd "C:/Users/lubao/.codex/plugins/cache/xiigen-local/codex-xiigen-smart-skills/0.1.0+codex.runtime04e-20260616-231500"
node .\src\codex-entrypoint.js ... --user-prompt-or-task-text "" ... --correlation-id "codex-runtime-proof-execution-no-scoring-no-training-20260622-024755-negative" ...
exit code 1; superseded by final negative evidence in the same negative request/response/trace paths
```

Accepted negative command:

```powershell
cd "C:/Users/lubao/.codex/plugins/cache/xiigen-local/codex-xiigen-smart-skills/0.1.0+codex.runtime04e-20260616-231500"
node .\src\codex-entrypoint.js `
  --method prompt-to-task `
  --user-prompt-or-task-text "[REDACTED Codex negative target-locator smoke task]" `
  --host-facts-json '{"surface":"installed-codex-skill","scope":"RUNTIME-04E","scenario":"negative_error_request","dashboard_only_shortcut":"false"}' `
  --requested-skills-json '["xiigen-smart-skills"]' `
  --available-skills-json '["xiigen-smart-skills"]' `
  --check-set-inputs-json '{"runtime04e":"installed-skill-smoke","codex_runtime_proof_scenario":"negative_error_request","codex_requested_skills_bound":"true","codex_available_skills_bound":"true","qsg19_target_locator_required":"true"}' `
  --provenance-json '{"source":"installed-plugin-skill","plugin":"codex-xiigen-smart-skills","work_order":"DOCS-QA-CODEX-RUNTIME-PROOF-EXECUTION-NO-SCORING-NO-TRAINING-01","negative_path":"target_locator_required_without_locator"}'
  --correlation-id "codex-runtime-proof-execution-no-scoring-no-training-20260622-024755-negative"
  --debug-enabled `
  --target-locator-required `
  --core-root "C:\Projects\llm_mvp_core" `
  --extension-root "." `
  --request-path "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\codex-runtime-proof-execution-no-scoring-no-training\codex-runtime-proof-execution-no-scoring-no-training-20260622-024755\negative\request.json"
  --response-path "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\codex-runtime-proof-execution-no-scoring-no-training\codex-runtime-proof-execution-no-scoring-no-training-20260622-024755\negative\response.json"
  --trace-path "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\codex-runtime-proof-execution-no-scoring-no-training\codex-runtime-proof-execution-no-scoring-no-training-20260622-024755\negative\trace.json"
```

Plugin root: C:/Users/lubao/.codex/plugins/cache/xiigen-local/codex-xiigen-smart-skills/0.1.0+codex.runtime04e-20260616-231500
Entrypoint: C:/Users/lubao/.codex/plugins/cache/xiigen-local/codex-xiigen-smart-skills/0.1.0+codex.runtime04e-20260616-231500/src/codex-entrypoint.js
Core API: C:/Projects/llm_mvp_core/tools/xiigen-extension-core-api.ps1
Run root: C:/Projects/llm_mvp_core/artifacts/extension-runtime-token-quality-learnability/codex-runtime-proof-execution-no-scoring-no-training/codex-runtime-proof-execution-no-scoring-no-training-20260622-024755

## Почему это installed Codex plugin/adapter path

Запуск выполнен из установленного Codex plugin cache root, где `.codex-plugin/plugin.json` объявляет `codex-xiigen-smart-skills`, `package.json` указывает `main: src/codex-entrypoint.js`, а `SKILL.md` описывает smoke через этот entrypoint. `src/codex-entrypoint.js` создал `XiigenExtensionRequest`, вызвал `tools/xiigen-extension-core-api.ps1`, а trace показывает `SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest`. Это не dashboard-only и не API-only доказательство: использован установленный plugin adapter и локальный core boundary.

## Positive redacted input/output

Correlation id: codex-runtime-proof-execution-no-scoring-no-training-20260622-024755-positive. Вход: method=prompt-to-task, HostName=Codex, HostType=codex, prompt хранится как redacted smoke text. RequestedSkills=["xiigen-smart-skills"], AvailableSkills=["xiigen-smart-skills"]. Выход: Success=true, Status=core_boundary_partial_no_semantic_quality_claim, core_boundary_executed=true, semantic_quality_claimed=false, runtime_extension_claimed=false.

## Negative redacted input/output

Correlation id: codex-runtime-proof-execution-no-scoring-no-training-20260622-024755-negative. Вход: тот же adapter/core path, method=prompt-to-task, prompt redacted, RequestedSkills=["xiigen-smart-skills"], AvailableSkills=["xiigen-smart-skills"], qsg19_target_locator_required=true без TargetLocator. Выход: Success=false, Status=invalid_request, Errors=["target_locator_required"]. Это controlled negative/error path без scoring/training/model mutation.

## Same-run correlation ids

Оба correlation id начинаются с одного run id codex-runtime-proof-execution-no-scoring-no-training-20260622-024755: positive=codex-runtime-proof-execution-no-scoring-no-training-20260622-024755-positive, negative=codex-runtime-proof-execution-no-scoring-no-training-20260622-024755-negative.

## Fresh artifacts written

- artifacts/extension-runtime-token-quality-learnability/codex-runtime-proof-execution-no-scoring-no-training/codex-runtime-proof-execution-no-scoring-no-training-20260622-024755
- artifacts/extension-runtime-token-quality-learnability/codex-runtime-proof-execution-no-scoring-no-training/codex-runtime-proof-execution-no-scoring-no-training-20260622-024755/codex-runtime-proof-ru.md
- artifacts/extension-runtime-token-quality-learnability/codex-runtime-proof-execution-no-scoring-no-training/codex-runtime-proof-execution-no-scoring-no-training-20260622-024755/codex-runtime-proof.json
- artifacts/extension-runtime-token-quality-learnability/codex-runtime-proof-execution-no-scoring-no-training/codex-runtime-proof-execution-no-scoring-no-training-20260622-024755/positive-request-trace.json
- artifacts/extension-runtime-token-quality-learnability/codex-runtime-proof-execution-no-scoring-no-training/codex-runtime-proof-execution-no-scoring-no-training-20260622-024755/negative-request-trace.json
- artifacts/extension-runtime-token-quality-learnability/codex-runtime-proof-execution-no-scoring-no-training/codex-runtime-proof-execution-no-scoring-no-training-20260622-024755/fresh-artifacts-manifest.json
- artifacts/extension-runtime-token-quality-learnability/codex-runtime-proof-execution-no-scoring-no-training/codex-runtime-proof-execution-no-scoring-no-training-20260622-024755/side-effect-audit.json
- artifacts/extension-runtime-token-quality-learnability/codex-runtime-proof-execution-no-scoring-no-training/codex-runtime-proof-execution-no-scoring-no-training-20260622-024755/selected-follow-up-work-order.md
- artifacts/extension-runtime-token-quality-learnability/codex-runtime-proof-execution-no-scoring-no-training/codex-runtime-proof-execution-no-scoring-no-training-20260622-024755/selected-follow-up-work-order.json
- artifacts/extension-runtime-token-quality-learnability/codex-runtime-proof-execution-no-scoring-no-training/codex-runtime-proof-execution-no-scoring-no-training-20260622-024755/state-entry.json
- artifacts/extension-runtime-token-quality-learnability/codex-runtime-proof-execution-no-scoring-no-training/codex-runtime-proof-execution-no-scoring-no-training-20260622-024755/positive/request.json
- artifacts/extension-runtime-token-quality-learnability/codex-runtime-proof-execution-no-scoring-no-training/codex-runtime-proof-execution-no-scoring-no-training-20260622-024755/positive/response.json
- artifacts/extension-runtime-token-quality-learnability/codex-runtime-proof-execution-no-scoring-no-training/codex-runtime-proof-execution-no-scoring-no-training-20260622-024755/positive/trace.json
- artifacts/extension-runtime-token-quality-learnability/codex-runtime-proof-execution-no-scoring-no-training/codex-runtime-proof-execution-no-scoring-no-training-20260622-024755/negative/request.json
- artifacts/extension-runtime-token-quality-learnability/codex-runtime-proof-execution-no-scoring-no-training/codex-runtime-proof-execution-no-scoring-no-training-20260622-024755/negative/response.json
- artifacts/extension-runtime-token-quality-learnability/codex-runtime-proof-execution-no-scoring-no-training/codex-runtime-proof-execution-no-scoring-no-training-20260622-024755/negative/trace.json

## RequestedSkills/AvailableSkills binding evidence

В raw request обоих сценариев присутствуют `RequestedSkills: ["xiigen-smart-skills"]` и `AvailableSkills: ["xiigen-smart-skills"]`; positive debug input также содержит `requestedSkills:["xiigen-smart-skills"]` и `availableSkillCount:1`.

## No-side-effect statement

No git commands, scoring/eval/token-savings measurement, training/DPO, checkpoint promotion, model/locator/target mutation, external AI/web/model calls, Claude runtime proof, Cursor runtime proof, or repo-wide tests were run by this bounded proof. Writes were limited to the timestamped Codex runtime proof artifact folder and the execution state file update.

## Что не доказано

- Этот proof не доказывает token savings.
- Этот proof не доказывает quality improvement.
- Этот proof не доказывает learnability.
- Этот proof не доказывает semantic quality.
- Этот proof не доказывает model readiness.
- Этот proof не доказывает full Codex chat UI/app readiness.
- Этот proof не доказывает Claude/Cursor runtime.

## Next selected work order

DOCS-QA-CODEX-RUNTIME-PROOF-REVIEW-NO-SCORING-NO-TRAINING-01
