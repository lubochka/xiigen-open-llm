# Plan Review Cycle 05 - User-Side Adaptive Lifecycle

## Человеческий пакет

Стадия: цикл 5 из 10 перед любым продолжением runtime/code/training/host.

Фокус: local adaptive learning lifecycle для пользовательской стороны. Проверка закрывает риск, что план засчитает "local model learned" по DPO row, training log, свежему checkpoint file или scoped QSG evidence, но не докажет полный user-owned lifecycle: backup, export, import в другой device/root, restore, load вместо retrain, continue-training, DPO из AI result gaps и user corrections, safety/redaction/provenance и rollback.

Reviewer/sub-agent id: `PLAN-REVIEW-CYCLE-05-USER-SIDE-ADAPTIVE-LIFECYCLE`.

Model/reasoning evidence: `model=gpt-5.5`, `reasoning_effort=xhigh`, `downgrade_warning_or_NONE=NONE`.

Ограничения выполнены: агентов не спавнил, ветку не менял, commit/push не делал, runtime/source code не редактировал, runtime/training/host probes не запускал.

## Загруженные gates

- Full Extension Definition Gate.
- `client-adaptive-learning-loop-SKILL`.
- plan-review model/set split.
- no hidden determinism.
- output readability.
- 10 Review Cycle Gate.

## Что проверено

- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md` sections 0, 3, 4.4, 4.4.1, 4.5, 6, 7, 8, 9, 9.1, 9.2, 10, 11.
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json` top-level cycle gate, active completion gate, QSG-08/QSG-13/QSG-16/QSG-19 entries and final claim booleans.
- `plan-review-cycle-01..04.md/json`.
- QSG-8, QSG-13, QSG-16 and QSG-19 reports/validations as supporting evidence only.
- `docs/skills/client-adaptive-learning-loop-SKILL.md`.

## Находки и исправления

1. PASS_WITH_PLAN_FIX: Section 4.4 already required DPO, privacy/source/license, user-local store, continue-training, fresh-load, held-out, ablation and rollback, but it did not yet require a concrete portability lifecycle acceptance gate for every Claude/Cursor/Codex full-extension path.
   Fix: added Section 4.4.2 `User-side adaptive portability lifecycle acceptance gate`.
2. PASS_WITH_HOST_COVERAGE_MATRIX: the plan now requires a per-host lifecycle matrix for Claude, Cursor and Codex. Each host must prove DPO from host output/correction/result gap, backup/export, import into another approved device/root, restore, load-without-retrain, continue-training from imported/existing checkpoint, redaction/provenance and rollback.
3. PASS_WITH_LOCAL_COMMON_SPLIT: the plan now states that common logic, common sets, common manifests, common checkpoints, DPO schemas, token/quality logic and orchestration stay in `llm_mvp_core`, while user prompts, private corrections, user-local DPO rows, adaptive checkpoints, imported overlays, backup bundles, raw traces and provenance ledgers stay in user-owned `.xiigen` or another approved user-owned root.
4. PASS_WITH_DPO_GAP_SOURCE: the DPO gate now accepts both explicit user corrections and AI result gaps/missing results, but a gap-only DPO row must include expected behavior source and reviewer approval before train-ready use.
5. PASS_WITH_LOAD_NOT_RETRAIN_GATE: import/restore must prove `load_source=imported_user_local`, `loaded_existing_checkpoint=true`, `retrained_from_zero=false`, and a behavior probe before any training command. File parse, hash-only proof, exact prompt replay or deterministic lookup is not PASS.
6. PASS_WITH_ROLLBACK_GATE: rollback must cover both imported overlay rollback and rollback after continue-training, without locator/store drift and without common asset mutation.
7. PASS_WITH_ARTIFACTS_AND_FINAL_GATES: updated required artifacts, LEARN-02/04/05 phase gates, review pass 6, final acceptance matrix, full functionality closure and final readiness pass 7/8 wording.
8. PASS_WITH_STATE_FIX: execution state now advances to `PLAN-REVIEW-CYCLE-05_OF_10`, records cycle 05 artifacts, keeps `implementation_authorized=false`, and sets the next action to cycle 6.

## QSG evidence used as supporting context

- QSG-08: 108 continue-training proof rows were accepted and component continue-training gates passed, but `semantic_quality_claimed=false`, `learnability_claimed=false`, `training_started=false`, `checkpoint_mutation_started=false`, and readiness claims stayed closed.
- QSG-13: adaptive display and token projection improved across 54 pairs, but token savings, semantic quality, learnability, product readiness, promotion and final readiness claims stayed closed.
- QSG-16: target/rollback/locator candidate and probe artifacts were created, but target promotion, locator update, rollback closure, product runtime semantic quality and final readiness stayed closed.
- QSG-19: evidence level was handler/test-only; live UI/chat readiness, runtime extension readiness, production locator promotion, production rollback closure and final readiness stayed closed.

Conclusion: QSG-8/13/16/19 are useful evidence trail, but they do not prove the user-side lifecycle. The plan now requires future packet-backed proof for backup/export/import/restore/load-without-retrain/continue-training/rollback per host path.

## Host coverage result

| Host | Cycle 05 lifecycle requirement | Counts for three full extensions in this dimension |
|---|---|---|
| Claude | Must produce `claude-user-local-lifecycle-report.*` proving capture/DPO, user-local store, backup/export, import into another root/device, restore/load-without-retrain, continue-training and rollback. | Yes, plan now requires it. Product proof now: no. |
| Cursor | Must produce `cursor-user-local-lifecycle-report.*` with the same lifecycle and proof that imported user-local overlay is used before repo/common fallback. | Yes, plan now requires it. Product proof now: no. |
| Codex | Must produce `codex-user-local-lifecycle-report.*` with the same lifecycle and no package-smoke/deterministic shortcut substitution. | Yes, plan now requires it. Product proof now: no. |

## Before / After

Before:

- Section 4.4 had a strong local learnability chain but no explicit import-to-another-device/root acceptance gate.
- Required artifacts had store and backup/export/import/share reports, but no host coverage matrix, export bundle manifest, import/restore report or load-without-retrain report.
- Final readiness pass 7/8 did not explicitly fail missing import-to-another-root, restore, load-without-retrain or common/user-owned split.

After:

- Section 4.4.2 defines the user-side adaptive portability lifecycle acceptance gate.
- Section 6 LEARN phases require user-side lifecycle host coverage, imported/restored load source and load-without-retrain proof.
- Section 7 requires `user-local-portability-host-coverage-matrix.*`, backup/export bundle manifest, import/restore report and load-without-retrain report.
- Sections 8, 9, 9.1 and 9.2 require per-host lifecycle proof, AI gap/user correction DPO, safety/redaction/provenance, rollback and no common asset mutation.
- State records cycle 5 and keeps runtime/code/training/host execution locked.

## Recheck evidence

Post-patch verification status: PASS.

Проверки:

- PASS: execution state JSON parse.
- PASS: cycle 05 JSON parse.
- PASS: `git diff --check`.
- PASS: targeted `rg` for `backup`, `export`, `import`, `restore`, `load-without-retrain`, `continue-training`, `DPO`, `user-local`, `user-owned`, `common logic`, `Claude`, `Cursor`, `Codex`.

No `dotnet test` was run because this phase authorizes only plan/state/artifact review verification, not runtime/source code verification.

## Verdict

PASS for cycle 5. Этот цикл засчитывается в requested 10 real review/fix cycles.

cycle_counts_toward_10: true.

cycles_completed_after_this_packet: 5.

remaining_cycles_before_execution: 5.

proves_three_full_extensions_in_this_dimension: true, as a plan gate for future Claude/Cursor/Codex lifecycle evidence.

product_readiness_proven_now: false.

Runtime/code/training/host execution, QSG-20, production locator update, checkpoint mutation, commit and push remain forbidden in this sub-agent slice.
