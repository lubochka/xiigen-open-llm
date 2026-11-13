# REFERENCE-PRESERVATION-ROW12-CURSOR-HOST-PROPAGATION-FIX-01

## Что было неправильно

Cursor Row12 Path A runtime-proof запрос передавал `marker`, `scenario_id` и `pair_id` только как `redactedInput`/metadata. Core строит `CheckSetResults.references` из `request.CheckSetInputs`, поэтому metadata и текст prompt не могли заполнить `references.marker`, `references.scenario_id` и `references.pair_id`. Из-за этого в raw core response получались `not-provided`.

## Что изменено

В Cursor extension добавлен узкий structured pass-through `checkSetInputs`:

- `src/extension.ts` нормализует `checkSetInputs` из runtime-proof input и добавляет их в `XiigenExtensionRequest.CheckSetInputs` перед вызовом core.
- `dist/extension.js` синхронизирован с тем же runtime path, потому что Cursor загружает `dist/extension.js`.
- `test/host-qa/FRDP-row12-hostVisibleRuntimeProofQA.js` теперь отправляет `marker`, `scenario_id`, `pair_id` как structured `checkSetInputs`, проверяет outgoing request и парсит core response `CheckSetResults.references` плюс `row12_core_api_quality_projection.references`.
- Test harness теперь может брать core root из `XIIGEN_ROW12_CORE_ROOT`, чтобы проверять primary sync repo.

Алгоритмы semantic/model logic не менялись. Response/evidence после факта не редактировались для имитации успеха.

## Как проверено

Команды и результат:

- `node --check dist\extension.js` в `C:\Projects\cursor-xiigen-smart-skills` -> exit 0.
- `node --check test\host-qa\FRDP-row12-hostVisibleRuntimeProofQA.js` в `C:\Projects\cursor-xiigen-smart-skills` -> exit 0.
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\run-frdp-row12-host-visible.ps1 -EvidenceRoot C:\Projects\llm_mvp_core_codex_sync_20260628\docs\plans\evidence\extension-full-readiness-dual-path\reference-preservation-row12-cursor-fix-20260628T153936Z -TimeoutSeconds 180` with `XIIGEN_ROW12_CORE_ROOT=C:\Projects\llm_mvp_core_codex_sync_20260628` -> PASS.
- Parsed `row12-host-visible-runner-report.json`, core request JSON, and core response JSON -> request and response references preserved:
  - `marker = FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1`
  - `scenario_id = frdp-p01-cursor-token-savings-baseline`
  - `pair_id = frdp-p01-row12-cursor-token-baseline-v1`
- `git diff --check -- src/extension.ts dist/extension.js test/host-qa/FRDP-row12-hostVisibleRuntimeProofQA.js` in Cursor repo -> exit 0.
- `git diff --check` in primary sync repo -> exit 0.

Primary runner report:
`C:\Projects\llm_mvp_core_codex_sync_20260628\docs\plans\evidence\extension-full-readiness-dual-path\reference-preservation-row12-cursor-fix-20260628T153936Z\row12-host-visible-runner-report.json`

## Что осталось

Эта работа доказывает только Row12 Path A Cursor host-runner/reference propagation into core request/response. Она не доказывает product readiness, token savings, semantic quality, training, checkpoint promotion, locator update, или full Path B local desktop UI proof.

Следующий bounded work order: rerun/re-evaluate Row12 semantic quality classification using the new Path A packet with preserved structured references, and pair it with any required Path B UI evidence if the active readiness chain still requires dual-path proof.

## Honesty block

claim_scope: Row12 Cursor Path A host-runner structured reference propagation fix only.
evidence_scope: code diff in Cursor extension request path/test; successful Row12 host-runner packet; parsed outgoing request and core response references.
count_claimed: 1 implementation fix, 1 Row12 Path A host-runner verification, 2 syntax checks, 2 diff checks.
independent_evidence_count: 6 command/evidence records.
subagent_id_or_NONE: NONE.
downgrade_needed: no for reference propagation; yes for product readiness/semantic quality/full dual-path readiness because those were not verified here.
