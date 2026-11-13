# Диагностика reference preservation для Cursor Row12

Статус: `NEEDS_PRODUCT_CODE_WORK_ORDER`.

Вердикт: я не исправлял evidence layer и не менял product code. Узкий вывод такой: `CheckSetResults.references.marker`, `CheckSetResults.references.scenario_id` и `CheckSetResults.references.pair_id` стали `not-provided`, потому что в Path A core request эти значения не попали как структурные `CheckSetInputs`.

## Что именно отсутствует

В raw Path A core response поле `CheckSetResults.references` уже содержит:

- `marker = not-provided`
- `scenario_id = not-provided`
- `pair_id = not-provided`

Ожидаемые значения Row12:

- `marker = FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1`
- `scenario_id = frdp-p01-cursor-token-savings-baseline`
- `pair_id = frdp-p01-row12-cursor-token-baseline-v1`

Это не ошибка старой semantic eval-сводки. Eval только увидела то, что уже было в raw core response.

## Откуда это берется

Path A request содержит Row12 marker/scenario/pair в тексте prompt и `CorrelationId`, но не содержит эти значения как ключи `CheckSetInputs.marker`, `CheckSetInputs.scenario_id`, `CheckSetInputs.pair_id`.

Cursor host QA runner передает эти значения в команду как `redactedInput` и `correlationId`. Но read-only inspection показал, что request builder в `C:/Projects/cursor-xiigen-smart-skills/src/extension.ts` создает `CheckSetInputs` только из фиксированных `cursor_*` ключей и не переносит туда `marker`, `scenario_id`, `pair_id`.

Core-side projection в `43.SmartSkillsDomain/XiigenExtensionApiService.cs` строит `CheckSetResults.references` именно из `request.CheckSetInputs`. Если ключей нет, core использует fallback `not-provided`.

При этом прямой core contract test уже показывает правильную ожидаемую форму: когда `CheckSetInputs` содержит `scenario_id`, `marker`, `pair_id`, Row12 projection включает эти значения. Значит, проблема уже не в post-hoc evidence capture, а в host/runtime-proof reference propagation.

## Classification

`evidence_capture_issue = false`

`product_code_or_reference_propagation_issue = true`

Более точное имя: `Cursor host/runtime-proof request propagation gap`.

Слой evidence/test может зафиксировать диагноз и будущую проверку, но не должен переписывать старые raw evidence files. Чтобы исправить результат, нужен fresh Row12 Path A rerun после изменения host/product request propagation.

## Почему не FIXED_EVIDENCE_LAYER

- Старый raw core response уже содержит `not-provided`; менять старую evidence папку запрещено и было бы нечестной подменой поведения запуска.
- Runner report сохранил marker/scenario/pair как validation metadata, но это не доказывает, что они дошли в core `CheckSetInputs`.
- Core строит `CheckSetResults.references` из структурных `CheckSetInputs`, а не из runner report и не из post-hoc eval.
- Минимальный реальный fix требует изменения Cursor host request builder или расширения shared host/core contract, затем fresh Path A evidence rerun.

## Evidence used

- `docs/plans/evidence/extension-full-readiness-dual-path/row12-cursor-timeout-fix-final-20260628T134935Z/xiigen-cursor-xiigenSmartSkills.runtimeProof.invoke-frdp-p01-row12-cursor-token-baseline-v1_FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1_frdp-p01-cursor-token-savings-baseline-positive_request-request-2026-06-28T13-50-15-212Z.json`
- `docs/plans/evidence/extension-full-readiness-dual-path/row12-cursor-timeout-fix-final-20260628T134935Z/xiigen-cursor-xiigenSmartSkills.runtimeProof.invoke-frdp-p01-row12-cursor-token-baseline-v1_FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1_frdp-p01-cursor-token-savings-baseline-positive_request-core-response-2026-06-28T13-50-15-212Z.json`
- `docs/plans/evidence/extension-full-readiness-dual-path/row12-cursor-timeout-fix-final-20260628T134935Z/row12-host-visible-runner-report.json`
- `docs/plans/evidence/extension-full-readiness-dual-path/quality-semantic-row12-cursor-eval-20260628T151416Z/eval.json`
- `C:/Projects/cursor-xiigen-smart-skills/test/host-qa/FRDP-row12-hostVisibleRuntimeProofQA.js`
- `C:/Projects/cursor-xiigen-smart-skills/src/extension.ts`
- `43.SmartSkillsDomain/XiigenExtensionApiService.cs`
- `Tests/XiigenExtensionApiContractsTests.cs`

## Next bounded worker order

`REFERENCE-PRESERVATION-ROW12-CURSOR-HOST-PROPAGATION-FIX-01`

Goal: make Row12 `marker`, `scenario_id`, and `pair_id` reach `llm_mvp_core` as structured `CheckSetInputs`, then prove a fresh Path A run has those values inside `CheckSetResults.references`.

Recommended scope:

1. In `C:/Projects/cursor-xiigen-smart-skills/src/extension.ts`, extend runtime-proof request construction so `marker`, `scenario_id`, and `pair_id` from command input are copied into `XiigenExtensionRequest.CheckSetInputs` when present.
2. Keep raw prompt redaction unchanged; do not copy private prompt text outside the request payload.
3. Update or add host QA assertions in `C:/Projects/cursor-xiigen-smart-skills/test/host-qa/FRDP-row12-hostVisibleRuntimeProofQA.js` so the runner fails unless parsed `CheckSetResults.references.marker`, `scenario_id`, and `pair_id` equal the Row12 constants.
4. If the chosen design changes shared core/host contract instead, add or adjust `llm_mvp_core` tests so `CheckSetResults.references` is asserted directly, not only by string containment in `row12_core_api_quality_projection`.
5. Rerun Row12 Path A into a new evidence folder; do not modify previous evidence folders.

Suggested verification:

- Parse the new Row12 request JSON and assert `CheckSetInputs.marker`, `CheckSetInputs.scenario_id`, and `CheckSetInputs.pair_id` are present.
- Parse the new Row12 core-response JSON and assert `CheckSetResults.references` has the expected Row12 values.
- Run the updated host QA runner command that generates Path A evidence.
- Run the narrow product/host tests touched by the implementation.
- Run `git diff --check` and `git status --short` for changed scope.

## Honesty block

claim_scope: one Row12 Cursor reference-preservation diagnosis packet; no product fix and no evidence-layer fix claimed
evidence_scope: eight inspected source/evidence records: existing eval packet, Row12 request JSON, Row12 core-response JSON, Row12 runner report JSON, Cursor host QA runner JS, Cursor extension source snippet, core service snippet, and core contract test snippet
count_claimed: 1 diagnosis packet; 1 verdict; 0 code fixes; 0 reruns; 0 token savings, semantic quality, product readiness, training, checkpoint, locator, commit, or push claims
independent_evidence_count: 8
subagent_id_or_NONE: NONE
downgrade_needed: yes for any fixed/readiness claim; no for the narrow diagnosis verdict
