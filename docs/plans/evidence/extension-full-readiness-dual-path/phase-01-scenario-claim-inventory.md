# Phase 01.01 - Scenario And Claim Inventory

Фаза 2 из 17: инвентаризация сценариев и claims. Смысл фазы: превратить broad readiness claims по Claude/Cursor/Codex в конкретные host/scenario/claim rows.
Шаг 01.01 из 01.02: матрица сценариев и claims. Смысл шага: создать список того, что потом проверять двумя путями, без runtime-запусков сейчас.
Статус: DONE. Матрица создана как inventory-only artifact: она перечисляет будущие проверки, но не выполняет runtime, host UI, token, quality, training, checkpoint, locator или readiness verification.
Сделано: создано 28 inventory rows: Claude 9, Cursor 9, Codex 10. В процессе: ничего не запущено; этот шаг завершен как evidence/status inventory. Осталось: Phase 01.02/следующий bounded шаг должен превратить этот список в конкретные work orders или Phase 02 schema work, если Luba отдельно разрешит.
Review нашёл: ни одна строка не имеет свежего runtime evidence; поэтому все execution/result поля оставлены `NOT_RUN`, `NEEDS_VERIFICATION`, `NOT_DISCOVERED_YET` или `NONE_COLLECTED_IN_PHASE_01_01`.
Что исправлено: прежний Phase 01 статус `NOT_STARTED` заменен этим отдельным inventory artifact and latest state/log update; broad claims разложены по host/scenario/claim rows.
Что осталось: выполнить Automated boundary и Local desktop paths в будущих фазах, с реальными командами, host activation, marker evidence, screenshots/logs, token/quality/training/checkpoint/locator probes only after explicit authorization.
Следующее действие: parent/reviewer accepts or rejects this inventory; implementation/runtime continuation requires a separate bounded work order.
Commit/continuation allowed: git write/ref actions are forbidden; continuation beyond inventory is not authorized by this artifact.
opaque codes: `FRDP-P01`, `NOT_RUN`, `NEEDS_VERIFICATION`, `NOT_DISCOVERED_YET`, and `NONE_COLLECTED_IN_PHASE_01_01` appear only after the human explanation above.

## CURRENT TASK

CURRENT TASK: Execute Phase 01.01 of `docs/plans/EXTENSION-FULL-READINESS-DUAL-PATH-PLAN-v1.md` as a bounded sub-agent: create the first scenario and claim inventory matrix for Claude, Cursor, Codex and the required claim families.

active_task_sentence_source: Luba latest unquoted instruction in this work order: "Execute Phase 01.01 of `docs/plans/EXTENSION-FULL-READINESS-DUAL-PATH-PLAN-v1.md` as a bounded sub-agent: create the first scenario and claim inventory matrix for Claude, Cursor, Codex and the required claim families."

NOT THE TASK: Phase 02 schema implementation, Claude/Cursor/Codex host UI tests, product code, training, DPO construction, checkpoint mutation, checkpoint promotion, locator update, browser checks, external AI, web tools, product readiness claims, extension readiness claims, token savings claims, semantic quality claims, learnability/training claims, or git write/ref actions.

EVIDENCE-ONLY: old packets, quoted transcripts, previous plans, review ledger rows, existing host evidence examples, and Section 7 candidate references. They seed candidate reference fields only and do not prove current runtime.

## Execution Metadata

| Field | Value |
|---|---|
| subagent_id_or_nickname | `codex-bounded-executor-phase-01-01` |
| receiving_actor | Current Codex session acting under the bounded work order; no nested sub-agent was spawned. |
| requested_model | `gpt-5.5` if available |
| runtime_model_or_unavailable | `unavailable`; no concrete runtime/request metadata exposed a mismatch. |
| requested_reasoning_effort | `xhigh` if available |
| runtime_reasoning_effort_or_unavailable | `unavailable`; no concrete runtime/request metadata exposed a mismatch. |
| marker_prefix_for_future_runs | `FRDP-P01` |
| marker_shown_in_host | `NOT_RUN`; no marker was shown in Claude, Cursor, or Codex. |
| security_privacy_redaction_required | `true` for future screenshots/videos/logs; none were captured in this slice. |
| docs_skills_guides_agent_propagation_required | Not in this slice; future Phase 16 dependency only. |
| git_write_or_ref_actions_used | `false` |

Read-before-write proof for existing files touched by this slice: the execution state file already ended with the prior reporting-contract repair verification, and the execution log ended by stating no git add/commit/push/checkout/reset/clean was used.

## Inventory Counts

| Count type | Count | Explanation |
|---|---:|---|
| Total inventory rows | 28 | One row per relevant host/claim family scenario. |
| Claude rows | 9 | Claude receives every common claim family except Codex-only chat UI. |
| Cursor rows | 9 | Cursor receives every common claim family except Codex-only chat UI. |
| Codex rows | 10 | Codex receives the common claim families plus full Codex chat UI readiness. |
| Rows with Automated boundary placeholder | 28 | Every row has `Automated boundary status: NOT_RUN`. |
| Rows with Local desktop placeholder | 28 | Every row has `Local desktop status: NOT_RUN`. |
| Runtime/host/token/quality/training/checkpoint/locator results | 0 | None were authorized or executed. |

## Claim Family Counts

| claim_id | Rows | Relevant hosts | Future proof shape |
|---|---:|---|---|
| `product-readiness` | 3 | Claude, Cursor, Codex | Joined product workflow evidence only. |
| `full-extension` | 3 | Claude, Cursor, Codex | Per-host full extension gate; no host substitution. |
| `codex-chat-ui` | 1 | Codex | Real Codex chat UI marker or exact blocker; helper-only evidence downgraded. |
| `token-savings` | 3 | Claude, Cursor, Codex | Same workload/tokenizer before-after plus quality guard and host-visible access. |
| `semantic-quality` | 3 | Claude, Cursor, Codex | Held-out rubric, leakage check, and host-visible answer evidence. |
| `learnability-training` | 3 | Claude, Cursor, Codex | Local train/eval/continue-training/checkpoint/rollback plus host-visible behavior. |
| `dpo-quality` | 3 | Claude, Cursor, Codex | Provenance, split, leakage, and allowed-use evidence only. |
| `asset-lifecycle` | 3 | Claude, Cursor, Codex | Backup/export/import/share/continue-training/rollback roundtrip. |
| `checkpoint-promotion` | 3 | Claude, Cursor, Codex | Candidate hash, compatibility/load, quality, rollback, and host reload. |
| `locator-update` | 3 | Claude, Cursor, Codex | Locator diff/fresh-load/rollback and per-host reload. |

## Candidate Reference Legend

These references are candidates from the plan, not current proof:

- Extension boundary candidates: `Tests\XiigenExtensionApiContractsTests.cs`.
- Token candidates: `Tests\ExtensionTokenTraceAccountingTests.cs`, `tools\token-quality-runner.ps1`, `tools\token-quality-full-host-scorer.ps1`.
- Semantic quality candidates: `Tests\ExtensionQualityScoringTests.cs`, `tools\semantic-quality-fixture-qa.js`, `tools\quality-product-semantic-product-readiness-evidence-repair.js`.
- Learnability candidates: `Tests\ExtensionAdaptiveLearnability01Tests.cs`, `Tests\ExtensionAdaptiveEffectGap02Tests.cs`, `14.ContinuousLearning\Training\IncrementalTrainer.cs`.
- Training candidates: `04.TrainingLoop\Trainer.cs`, `04.TrainingLoop\TrainingConfig.cs`.
- DPO candidates: `11.DPO\DpoTrainer.cs`, `11.DPO\DpoDataset.cs`, `Tests\DpoTrainerTests.cs`, `Tests\DpoDatasetTests.cs`.
- Checkpoint candidates: `models\prompt-to-task\prompt-to-task-v1\prompt-to-task-v1-checkpoint.json`, `models\orchestration-reasoning\orchestration-reasoning-v1\checkpoint.json`.
- Locator candidates: `artifacts\smart-skills-semantic-quality-completion\p17e-skillselection-lifecycle-import-export\p17e-manifest-locator-map.json` and related locator artifacts.
- Cursor local evidence examples: `artifacts\extension-runtime-token-quality-learnability\phase-10-cursor-full-extension-live-proof\...`; currency, relevance, redaction, and join status are `NEEDS_VERIFICATION`.
- Claude local host path: `NOT_DISCOVERED_YET`.
- Codex local host/chat UI path: `NOT_DISCOVERED_YET`.

## Scenario Inventory Rows

### Row 01 - Claude / product-readiness

- `scenario_id`: `frdp-p01-claude-product-readiness-product-workflow`
- `host`: Claude
- `claim_id`: `product-readiness`
- `short_workflow`: `product-workflow`
- `marker`: `FRDP-P01-frdp-p01-claude-product-readiness-product-workflow`
- `Automated boundary status`: NOT_RUN
- `Automated boundary candidate_refs`: Extension boundary candidates plus token, quality, lifecycle, checkpoint, and locator candidates as applicable.
- `Automated boundary remaining`: NEEDS_VERIFICATION of adapter/API/core/model/token/quality/lifecycle traces before any product readiness claim.
- `Local desktop status`: NOT_RUN
- `Local desktop activation`: NOT_DISCOVERED_YET for real Claude host entrypoint, launch/control method, and extension route.
- `Local desktop evidence required`: screenshot/video/log with visible marker; NOT_CAPTURED.
- `Local desktop remaining`: NEEDS_VERIFICATION of real installed Claude workflow behavior.
- `evidence`: NONE_COLLECTED_IN_PHASE_01_01
- `remaining`: both paths must be executed and joined in later phases.
- `downgraded_claim`: NEEDS_VERIFICATION; no product readiness claim is open.

### Row 02 - Claude / full-extension

- `scenario_id`: `frdp-p01-claude-full-extension-entrypoint`
- `host`: Claude
- `claim_id`: `full-extension`
- `short_workflow`: `entrypoint`
- `marker`: `FRDP-P01-frdp-p01-claude-full-extension-entrypoint`
- `Automated boundary status`: NOT_RUN
- `Automated boundary candidate_refs`: Extension boundary candidates; plugin runtime reality gate rows for host entrypoint, debug surface, core API path, prompt-to-task, orchestration, skills, learning, lifecycle, and QA.
- `Automated boundary remaining`: NEEDS_VERIFICATION of Claude adapter/API/core request-response and thin-adapter boundary.
- `Local desktop status`: NOT_RUN
- `Local desktop activation`: NOT_DISCOVERED_YET for real Claude plugin/host command or installed runtime entrypoint.
- `Local desktop evidence required`: screenshot/video/log with visible marker; NOT_CAPTURED.
- `Local desktop remaining`: NEEDS_VERIFICATION of real Claude host activation and response display.
- `evidence`: NONE_COLLECTED_IN_PHASE_01_01
- `remaining`: prove all full-extension definition rows for Claude or downgrade.
- `downgraded_claim`: NEEDS_VERIFICATION; no full Claude extension readiness claim is open.

### Row 03 - Claude / token-savings

- `scenario_id`: `frdp-p01-claude-token-savings-baseline`
- `host`: Claude
- `claim_id`: `token-savings`
- `short_workflow`: `baseline`
- `marker`: `FRDP-P01-frdp-p01-claude-token-savings-baseline`
- `Automated boundary status`: NOT_RUN
- `Automated boundary candidate_refs`: Token candidates plus quality guard candidates.
- `Automated boundary remaining`: NEEDS_VERIFICATION of same workload, tokenizer/version, baseline/candidate traces, repeat counts, and quality guard.
- `Local desktop status`: NOT_RUN
- `Local desktop activation`: NOT_DISCOVERED_YET for Claude host access to the candidate behavior.
- `Local desktop evidence required`: screenshot/video/log with visible marker and candidate behavior; NOT_CAPTURED.
- `Local desktop remaining`: NEEDS_VERIFICATION of host-visible candidate path.
- `evidence`: NONE_COLLECTED_IN_PHASE_01_01
- `remaining`: execute token measurement and local Claude evidence in later phases.
- `downgraded_claim`: NEEDS_VERIFICATION; no token savings claim is open.

### Row 04 - Claude / semantic-quality

- `scenario_id`: `frdp-p01-claude-semantic-quality-heldout`
- `host`: Claude
- `claim_id`: `semantic-quality`
- `short_workflow`: `heldout`
- `marker`: `FRDP-P01-frdp-p01-claude-semantic-quality-heldout`
- `Automated boundary status`: NOT_RUN
- `Automated boundary candidate_refs`: Semantic quality candidates; hidden-determinism review remains future work.
- `Automated boundary remaining`: NEEDS_VERIFICATION of held-out rubric, scorer, leakage controls, and baseline/candidate comparison.
- `Local desktop status`: NOT_RUN
- `Local desktop activation`: NOT_DISCOVERED_YET for Claude host scenario execution.
- `Local desktop evidence required`: screenshot/video/log with visible marker and answer evidence; NOT_CAPTURED.
- `Local desktop remaining`: NEEDS_VERIFICATION of host-visible answer quality for the same held-out scenario.
- `evidence`: NONE_COLLECTED_IN_PHASE_01_01
- `remaining`: execute quality scoring and local Claude marker evidence later.
- `downgraded_claim`: NEEDS_VERIFICATION; no semantic quality improvement claim is open.

### Row 05 - Claude / learnability-training

- `scenario_id`: `frdp-p01-claude-learnability-training-continue`
- `host`: Claude
- `claim_id`: `learnability-training`
- `short_workflow`: `continue-training`
- `marker`: `FRDP-P01-frdp-p01-claude-learnability-training-continue`
- `Automated boundary status`: NOT_RUN
- `Automated boundary candidate_refs`: Learnability candidates and training candidates.
- `Automated boundary remaining`: NEEDS_VERIFICATION of train/eval/continue-training/checkpoint diff/rollback and no-shortcut evidence.
- `Local desktop status`: NOT_RUN
- `Local desktop activation`: NOT_DISCOVERED_YET for Claude before/after behavior path.
- `Local desktop evidence required`: screenshot/video/log showing before/after marked behavior if later authorized; NOT_CAPTURED.
- `Local desktop remaining`: NEEDS_VERIFICATION of host-visible learned behavior after local load.
- `evidence`: NONE_COLLECTED_IN_PHASE_01_01
- `remaining`: execute local training probes and local Claude before/after evidence later.
- `downgraded_claim`: NEEDS_VERIFICATION; no learnability/training claim is open.

### Row 06 - Claude / dpo-quality

- `scenario_id`: `frdp-p01-claude-dpo-quality-provenance`
- `host`: Claude
- `claim_id`: `dpo-quality`
- `short_workflow`: `provenance`
- `marker`: `FRDP-P01-frdp-p01-claude-dpo-quality-provenance`
- `Automated boundary status`: NOT_RUN
- `Automated boundary candidate_refs`: DPO candidates.
- `Automated boundary remaining`: NEEDS_VERIFICATION of provenance, allowed-use, train/eval split, leakage gates, and DPO output metrics.
- `Local desktop status`: NOT_RUN
- `Local desktop activation`: NOT_DISCOVERED_YET for Claude behavior from DPO-derived candidate.
- `Local desktop evidence required`: screenshot/video/log with visible marker and candidate behavior; NOT_CAPTURED.
- `Local desktop remaining`: NEEDS_VERIFICATION of host-visible behavior tied to accepted DPO candidate.
- `evidence`: NONE_COLLECTED_IN_PHASE_01_01
- `remaining`: execute DPO validation and local Claude marker evidence later.
- `downgraded_claim`: NEEDS_VERIFICATION; no DPO quality claim is open.

### Row 07 - Claude / asset-lifecycle

- `scenario_id`: `frdp-p01-claude-asset-lifecycle-roundtrip`
- `host`: Claude
- `claim_id`: `asset-lifecycle`
- `short_workflow`: `roundtrip`
- `marker`: `FRDP-P01-frdp-p01-claude-asset-lifecycle-roundtrip`
- `Automated boundary status`: NOT_RUN
- `Automated boundary candidate_refs`: Phase 11 lifecycle package/readback/continue/rollback probes; exact implementation references are NOT_DISCOVERED_YET.
- `Automated boundary remaining`: NEEDS_VERIFICATION of backup/export/import/share/continue-training/rollback artifacts and load probes.
- `Local desktop status`: NOT_RUN
- `Local desktop activation`: NOT_DISCOVERED_YET for Claude restored/rolled-back behavior path.
- `Local desktop evidence required`: screenshot/video/log showing restored or rolled-back marked behavior; NOT_CAPTURED.
- `Local desktop remaining`: NEEDS_VERIFICATION of Claude host-visible lifecycle result.
- `evidence`: NONE_COLLECTED_IN_PHASE_01_01
- `remaining`: execute lifecycle package probes and local Claude evidence later.
- `downgraded_claim`: NEEDS_VERIFICATION; no lifecycle claim is open.

### Row 08 - Claude / checkpoint-promotion

- `scenario_id`: `frdp-p01-claude-checkpoint-promotion-reload`
- `host`: Claude
- `claim_id`: `checkpoint-promotion`
- `short_workflow`: `reload`
- `marker`: `FRDP-P01-frdp-p01-claude-checkpoint-promotion-reload`
- `Automated boundary status`: NOT_RUN
- `Automated boundary candidate_refs`: Checkpoint candidates.
- `Automated boundary remaining`: NEEDS_VERIFICATION of checkpoint hash, compatibility/load, quality, promotion owner, rollback, and fresh-load evidence.
- `Local desktop status`: NOT_RUN
- `Local desktop activation`: NOT_DISCOVERED_YET for Claude reload from promoted checkpoint.
- `Local desktop evidence required`: screenshot/video/log proving host reload uses candidate checkpoint; NOT_CAPTURED.
- `Local desktop remaining`: NEEDS_VERIFICATION of Claude reload behavior.
- `evidence`: NONE_COLLECTED_IN_PHASE_01_01
- `remaining`: execute checkpoint gates and local Claude reload proof later.
- `downgraded_claim`: NEEDS_VERIFICATION; no checkpoint mutation/promotion claim is open.

### Row 09 - Claude / locator-update

- `scenario_id`: `frdp-p01-claude-locator-update-reload`
- `host`: Claude
- `claim_id`: `locator-update`
- `short_workflow`: `reload`
- `marker`: `FRDP-P01-frdp-p01-claude-locator-update-reload`
- `Automated boundary status`: NOT_RUN
- `Automated boundary candidate_refs`: Locator candidates.
- `Automated boundary remaining`: NEEDS_VERIFICATION of actual runtime locator source of truth, locator diff, fresh-load, rollback path, and ownership.
- `Local desktop status`: NOT_RUN
- `Local desktop activation`: NOT_DISCOVERED_YET for Claude reload from updated locator.
- `Local desktop evidence required`: screenshot/video/log proving host uses locator-selected candidate; NOT_CAPTURED.
- `Local desktop remaining`: NEEDS_VERIFICATION of Claude host reload from locator.
- `evidence`: NONE_COLLECTED_IN_PHASE_01_01
- `remaining`: execute locator gates and local Claude reload proof later.
- `downgraded_claim`: NEEDS_VERIFICATION; no locator update claim is open.

### Row 10 - Cursor / product-readiness

- `scenario_id`: `frdp-p01-cursor-product-readiness-product-workflow`
- `host`: Cursor
- `claim_id`: `product-readiness`
- `short_workflow`: `product-workflow`
- `marker`: `FRDP-P01-frdp-p01-cursor-product-readiness-product-workflow`
- `Automated boundary status`: NOT_RUN
- `Automated boundary candidate_refs`: Extension boundary candidates plus token, quality, lifecycle, checkpoint, and locator candidates as applicable.
- `Automated boundary remaining`: NEEDS_VERIFICATION of adapter/API/core/model/token/quality/lifecycle traces before any product readiness claim.
- `Local desktop status`: NOT_RUN
- `Local desktop activation`: NOT_DISCOVERED_YET for real Cursor extension host command/chat/panel/command palette path.
- `Local desktop evidence required`: screenshot/video/log with visible marker; NOT_CAPTURED.
- `Local desktop remaining`: NEEDS_VERIFICATION of real installed Cursor workflow behavior.
- `evidence`: NONE_COLLECTED_IN_PHASE_01_01
- `remaining`: both paths must be executed and joined in later phases.
- `downgraded_claim`: NEEDS_VERIFICATION; no product readiness claim is open.

### Row 11 - Cursor / full-extension

- `scenario_id`: `frdp-p01-cursor-full-extension-entrypoint`
- `host`: Cursor
- `claim_id`: `full-extension`
- `short_workflow`: `entrypoint`
- `marker`: `FRDP-P01-frdp-p01-cursor-full-extension-entrypoint`
- `Automated boundary status`: NOT_RUN
- `Automated boundary candidate_refs`: Extension boundary candidates; plugin runtime reality gate rows for host entrypoint, debug surface, core API path, prompt-to-task, orchestration, skills, learning, lifecycle, and QA.
- `Automated boundary remaining`: NEEDS_VERIFICATION of Cursor adapter/API/core request-response and thin-adapter boundary.
- `Local desktop status`: NOT_RUN
- `Local desktop activation`: NOT_DISCOVERED_YET for VS Code/Cursor extension host command, chat participant, panel, or command palette entry.
- `Local desktop evidence required`: screenshot/video/log with visible marker; NOT_CAPTURED.
- `Local desktop remaining`: NEEDS_VERIFICATION of real Cursor host activation and response display.
- `evidence`: NONE_COLLECTED_IN_PHASE_01_01
- `remaining`: prove all full-extension definition rows for Cursor or downgrade.
- `downgraded_claim`: NEEDS_VERIFICATION; no full Cursor extension readiness claim is open.

### Row 12 - Cursor / token-savings

- `scenario_id`: `frdp-p01-cursor-token-savings-baseline`
- `host`: Cursor
- `claim_id`: `token-savings`
- `short_workflow`: `baseline`
- `marker`: `FRDP-P01-frdp-p01-cursor-token-savings-baseline`
- `Automated boundary status`: NOT_RUN
- `Automated boundary candidate_refs`: Token candidates plus quality guard candidates.
- `Automated boundary remaining`: NEEDS_VERIFICATION of same workload, tokenizer/version, baseline/candidate traces, repeat counts, and quality guard.
- `Local desktop status`: NOT_RUN
- `Local desktop activation`: NOT_DISCOVERED_YET for Cursor host access to the candidate behavior.
- `Local desktop evidence required`: screenshot/video/log with visible marker and candidate behavior; NOT_CAPTURED.
- `Local desktop remaining`: NEEDS_VERIFICATION of host-visible candidate path.
- `evidence`: NONE_COLLECTED_IN_PHASE_01_01
- `remaining`: execute token measurement and local Cursor evidence in later phases.
- `downgraded_claim`: NEEDS_VERIFICATION; no token savings claim is open.

### Row 13 - Cursor / semantic-quality

- `scenario_id`: `frdp-p01-cursor-semantic-quality-heldout`
- `host`: Cursor
- `claim_id`: `semantic-quality`
- `short_workflow`: `heldout`
- `marker`: `FRDP-P01-frdp-p01-cursor-semantic-quality-heldout`
- `Automated boundary status`: NOT_RUN
- `Automated boundary candidate_refs`: Semantic quality candidates; hidden-determinism review remains future work.
- `Automated boundary remaining`: NEEDS_VERIFICATION of held-out rubric, scorer, leakage controls, and baseline/candidate comparison.
- `Local desktop status`: NOT_RUN
- `Local desktop activation`: NOT_DISCOVERED_YET for Cursor host scenario execution.
- `Local desktop evidence required`: screenshot/video/log with visible marker and answer evidence; NOT_CAPTURED.
- `Local desktop remaining`: NEEDS_VERIFICATION of host-visible answer quality for the same held-out scenario.
- `evidence`: NONE_COLLECTED_IN_PHASE_01_01
- `remaining`: execute quality scoring and local Cursor marker evidence later.
- `downgraded_claim`: NEEDS_VERIFICATION; no semantic quality improvement claim is open.

### Row 14 - Cursor / learnability-training

- `scenario_id`: `frdp-p01-cursor-learnability-training-continue`
- `host`: Cursor
- `claim_id`: `learnability-training`
- `short_workflow`: `continue-training`
- `marker`: `FRDP-P01-frdp-p01-cursor-learnability-training-continue`
- `Automated boundary status`: NOT_RUN
- `Automated boundary candidate_refs`: Learnability candidates and training candidates.
- `Automated boundary remaining`: NEEDS_VERIFICATION of train/eval/continue-training/checkpoint diff/rollback and no-shortcut evidence.
- `Local desktop status`: NOT_RUN
- `Local desktop activation`: NOT_DISCOVERED_YET for Cursor before/after behavior path.
- `Local desktop evidence required`: screenshot/video/log showing before/after marked behavior if later authorized; NOT_CAPTURED.
- `Local desktop remaining`: NEEDS_VERIFICATION of host-visible learned behavior after local load.
- `evidence`: NONE_COLLECTED_IN_PHASE_01_01
- `remaining`: execute local training probes and local Cursor before/after evidence later.
- `downgraded_claim`: NEEDS_VERIFICATION; no learnability/training claim is open.

### Row 15 - Cursor / dpo-quality

- `scenario_id`: `frdp-p01-cursor-dpo-quality-provenance`
- `host`: Cursor
- `claim_id`: `dpo-quality`
- `short_workflow`: `provenance`
- `marker`: `FRDP-P01-frdp-p01-cursor-dpo-quality-provenance`
- `Automated boundary status`: NOT_RUN
- `Automated boundary candidate_refs`: DPO candidates.
- `Automated boundary remaining`: NEEDS_VERIFICATION of provenance, allowed-use, train/eval split, leakage gates, and DPO output metrics.
- `Local desktop status`: NOT_RUN
- `Local desktop activation`: NOT_DISCOVERED_YET for Cursor behavior from DPO-derived candidate.
- `Local desktop evidence required`: screenshot/video/log with visible marker and candidate behavior; NOT_CAPTURED.
- `Local desktop remaining`: NEEDS_VERIFICATION of host-visible behavior tied to accepted DPO candidate.
- `evidence`: NONE_COLLECTED_IN_PHASE_01_01
- `remaining`: execute DPO validation and local Cursor marker evidence later.
- `downgraded_claim`: NEEDS_VERIFICATION; no DPO quality claim is open.

### Row 16 - Cursor / asset-lifecycle

- `scenario_id`: `frdp-p01-cursor-asset-lifecycle-roundtrip`
- `host`: Cursor
- `claim_id`: `asset-lifecycle`
- `short_workflow`: `roundtrip`
- `marker`: `FRDP-P01-frdp-p01-cursor-asset-lifecycle-roundtrip`
- `Automated boundary status`: NOT_RUN
- `Automated boundary candidate_refs`: Phase 11 lifecycle package/readback/continue/rollback probes; exact implementation references are NOT_DISCOVERED_YET.
- `Automated boundary remaining`: NEEDS_VERIFICATION of backup/export/import/share/continue-training/rollback artifacts and load probes.
- `Local desktop status`: NOT_RUN
- `Local desktop activation`: NOT_DISCOVERED_YET for Cursor restored/rolled-back behavior path.
- `Local desktop evidence required`: screenshot/video/log showing restored or rolled-back marked behavior; NOT_CAPTURED.
- `Local desktop remaining`: NEEDS_VERIFICATION of Cursor host-visible lifecycle result.
- `evidence`: NONE_COLLECTED_IN_PHASE_01_01
- `remaining`: execute lifecycle package probes and local Cursor evidence later.
- `downgraded_claim`: NEEDS_VERIFICATION; no lifecycle claim is open.

### Row 17 - Cursor / checkpoint-promotion

- `scenario_id`: `frdp-p01-cursor-checkpoint-promotion-reload`
- `host`: Cursor
- `claim_id`: `checkpoint-promotion`
- `short_workflow`: `reload`
- `marker`: `FRDP-P01-frdp-p01-cursor-checkpoint-promotion-reload`
- `Automated boundary status`: NOT_RUN
- `Automated boundary candidate_refs`: Checkpoint candidates.
- `Automated boundary remaining`: NEEDS_VERIFICATION of checkpoint hash, compatibility/load, quality, promotion owner, rollback, and fresh-load evidence.
- `Local desktop status`: NOT_RUN
- `Local desktop activation`: NOT_DISCOVERED_YET for Cursor reload from promoted checkpoint.
- `Local desktop evidence required`: screenshot/video/log proving host reload uses candidate checkpoint; NOT_CAPTURED.
- `Local desktop remaining`: NEEDS_VERIFICATION of Cursor reload behavior.
- `evidence`: NONE_COLLECTED_IN_PHASE_01_01
- `remaining`: execute checkpoint gates and local Cursor reload proof later.
- `downgraded_claim`: NEEDS_VERIFICATION; no checkpoint mutation/promotion claim is open.

### Row 18 - Cursor / locator-update

- `scenario_id`: `frdp-p01-cursor-locator-update-reload`
- `host`: Cursor
- `claim_id`: `locator-update`
- `short_workflow`: `reload`
- `marker`: `FRDP-P01-frdp-p01-cursor-locator-update-reload`
- `Automated boundary status`: NOT_RUN
- `Automated boundary candidate_refs`: Locator candidates.
- `Automated boundary remaining`: NEEDS_VERIFICATION of actual runtime locator source of truth, locator diff, fresh-load, rollback path, and ownership.
- `Local desktop status`: NOT_RUN
- `Local desktop activation`: NOT_DISCOVERED_YET for Cursor reload from updated locator.
- `Local desktop evidence required`: screenshot/video/log proving host uses locator-selected candidate; NOT_CAPTURED.
- `Local desktop remaining`: NEEDS_VERIFICATION of Cursor host reload from locator.
- `evidence`: NONE_COLLECTED_IN_PHASE_01_01
- `remaining`: execute locator gates and local Cursor reload proof later.
- `downgraded_claim`: NEEDS_VERIFICATION; no locator update claim is open.

### Row 19 - Codex / product-readiness

- `scenario_id`: `frdp-p01-codex-product-readiness-product-workflow`
- `host`: Codex
- `claim_id`: `product-readiness`
- `short_workflow`: `product-workflow`
- `marker`: `FRDP-P01-frdp-p01-codex-product-readiness-product-workflow`
- `Automated boundary status`: NOT_RUN
- `Automated boundary candidate_refs`: Extension boundary candidates plus token, quality, lifecycle, checkpoint, and locator candidates as applicable.
- `Automated boundary remaining`: NEEDS_VERIFICATION of adapter/API/core/model/token/quality/lifecycle traces before any product readiness claim.
- `Local desktop status`: NOT_RUN
- `Local desktop activation`: NOT_DISCOVERED_YET for real Codex plugin/app runtime entrypoint.
- `Local desktop evidence required`: screenshot/video/log with visible marker; NOT_CAPTURED.
- `Local desktop remaining`: NEEDS_VERIFICATION of real installed Codex workflow behavior.
- `evidence`: NONE_COLLECTED_IN_PHASE_01_01
- `remaining`: both paths must be executed and joined in later phases.
- `downgraded_claim`: NEEDS_VERIFICATION; no product readiness claim is open.

### Row 20 - Codex / full-extension

- `scenario_id`: `frdp-p01-codex-full-extension-entrypoint`
- `host`: Codex
- `claim_id`: `full-extension`
- `short_workflow`: `entrypoint`
- `marker`: `FRDP-P01-frdp-p01-codex-full-extension-entrypoint`
- `Automated boundary status`: NOT_RUN
- `Automated boundary candidate_refs`: Extension boundary candidates; plugin runtime reality gate rows for host entrypoint, debug surface, core API path, prompt-to-task, orchestration, skills, learning, lifecycle, and QA.
- `Automated boundary remaining`: NEEDS_VERIFICATION of Codex adapter/API/core request-response and thin-adapter boundary.
- `Local desktop status`: NOT_RUN
- `Local desktop activation`: NOT_DISCOVERED_YET for Codex plugin/app runtime entrypoint.
- `Local desktop evidence required`: screenshot/video/log with visible marker; NOT_CAPTURED.
- `Local desktop remaining`: NEEDS_VERIFICATION of real Codex host activation and response display.
- `evidence`: NONE_COLLECTED_IN_PHASE_01_01
- `remaining`: prove all full-extension definition rows for Codex or downgrade.
- `downgraded_claim`: NEEDS_VERIFICATION; no full Codex extension readiness claim is open.

### Row 21 - Codex / codex-chat-ui

- `scenario_id`: `frdp-p01-codex-codex-chat-ui-marker`
- `host`: Codex
- `claim_id`: `codex-chat-ui`
- `short_workflow`: `marker`
- `marker`: `FRDP-P01-frdp-p01-codex-codex-chat-ui-marker`
- `Automated boundary status`: NOT_RUN
- `Automated boundary candidate_refs`: Codex host/chat UI evidence candidate path is NOT_DISCOVERED_YET; plugin/app runtime boundary references must be discovered later.
- `Automated boundary remaining`: NEEDS_VERIFICATION of Codex boundary trace, plugin/app runtime data, and helper-vs-real-chat-UI separation.
- `Local desktop status`: NOT_RUN
- `Local desktop activation`: NOT_DISCOVERED_YET for real Codex app/chat UI control path.
- `Local desktop evidence required`: screenshot/video/log with visible marker in real Codex chat UI; NOT_CAPTURED.
- `Local desktop remaining`: NEEDS_VERIFICATION of real Codex chat UI evidence or exact blocker.
- `evidence`: NONE_COLLECTED_IN_PHASE_01_01
- `remaining`: discover and execute real Codex chat UI scenario later.
- `downgraded_claim`: NEEDS_VERIFICATION; helper-only proof would downgrade to boundary readiness, not full Codex chat UI readiness.

### Row 22 - Codex / token-savings

- `scenario_id`: `frdp-p01-codex-token-savings-baseline`
- `host`: Codex
- `claim_id`: `token-savings`
- `short_workflow`: `baseline`
- `marker`: `FRDP-P01-frdp-p01-codex-token-savings-baseline`
- `Automated boundary status`: NOT_RUN
- `Automated boundary candidate_refs`: Token candidates plus quality guard candidates.
- `Automated boundary remaining`: NEEDS_VERIFICATION of same workload, tokenizer/version, baseline/candidate traces, repeat counts, and quality guard.
- `Local desktop status`: NOT_RUN
- `Local desktop activation`: NOT_DISCOVERED_YET for Codex host access to the candidate behavior.
- `Local desktop evidence required`: screenshot/video/log with visible marker and candidate behavior; NOT_CAPTURED.
- `Local desktop remaining`: NEEDS_VERIFICATION of host-visible candidate path.
- `evidence`: NONE_COLLECTED_IN_PHASE_01_01
- `remaining`: execute token measurement and local Codex evidence in later phases.
- `downgraded_claim`: NEEDS_VERIFICATION; no token savings claim is open.

### Row 23 - Codex / semantic-quality

- `scenario_id`: `frdp-p01-codex-semantic-quality-heldout`
- `host`: Codex
- `claim_id`: `semantic-quality`
- `short_workflow`: `heldout`
- `marker`: `FRDP-P01-frdp-p01-codex-semantic-quality-heldout`
- `Automated boundary status`: NOT_RUN
- `Automated boundary candidate_refs`: Semantic quality candidates; hidden-determinism review remains future work.
- `Automated boundary remaining`: NEEDS_VERIFICATION of held-out rubric, scorer, leakage controls, and baseline/candidate comparison.
- `Local desktop status`: NOT_RUN
- `Local desktop activation`: NOT_DISCOVERED_YET for Codex host scenario execution.
- `Local desktop evidence required`: screenshot/video/log with visible marker and answer evidence; NOT_CAPTURED.
- `Local desktop remaining`: NEEDS_VERIFICATION of host-visible answer quality for the same held-out scenario.
- `evidence`: NONE_COLLECTED_IN_PHASE_01_01
- `remaining`: execute quality scoring and local Codex marker evidence later.
- `downgraded_claim`: NEEDS_VERIFICATION; no semantic quality improvement claim is open.

### Row 24 - Codex / learnability-training

- `scenario_id`: `frdp-p01-codex-learnability-training-continue`
- `host`: Codex
- `claim_id`: `learnability-training`
- `short_workflow`: `continue-training`
- `marker`: `FRDP-P01-frdp-p01-codex-learnability-training-continue`
- `Automated boundary status`: NOT_RUN
- `Automated boundary candidate_refs`: Learnability candidates and training candidates.
- `Automated boundary remaining`: NEEDS_VERIFICATION of train/eval/continue-training/checkpoint diff/rollback and no-shortcut evidence.
- `Local desktop status`: NOT_RUN
- `Local desktop activation`: NOT_DISCOVERED_YET for Codex before/after behavior path.
- `Local desktop evidence required`: screenshot/video/log showing before/after marked behavior if later authorized; NOT_CAPTURED.
- `Local desktop remaining`: NEEDS_VERIFICATION of host-visible learned behavior after local load.
- `evidence`: NONE_COLLECTED_IN_PHASE_01_01
- `remaining`: execute local training probes and local Codex before/after evidence later.
- `downgraded_claim`: NEEDS_VERIFICATION; no learnability/training claim is open.

### Row 25 - Codex / dpo-quality

- `scenario_id`: `frdp-p01-codex-dpo-quality-provenance`
- `host`: Codex
- `claim_id`: `dpo-quality`
- `short_workflow`: `provenance`
- `marker`: `FRDP-P01-frdp-p01-codex-dpo-quality-provenance`
- `Automated boundary status`: NOT_RUN
- `Automated boundary candidate_refs`: DPO candidates.
- `Automated boundary remaining`: NEEDS_VERIFICATION of provenance, allowed-use, train/eval split, leakage gates, and DPO output metrics.
- `Local desktop status`: NOT_RUN
- `Local desktop activation`: NOT_DISCOVERED_YET for Codex behavior from DPO-derived candidate.
- `Local desktop evidence required`: screenshot/video/log with visible marker and candidate behavior; NOT_CAPTURED.
- `Local desktop remaining`: NEEDS_VERIFICATION of host-visible behavior tied to accepted DPO candidate.
- `evidence`: NONE_COLLECTED_IN_PHASE_01_01
- `remaining`: execute DPO validation and local Codex marker evidence later.
- `downgraded_claim`: NEEDS_VERIFICATION; no DPO quality claim is open.

### Row 26 - Codex / asset-lifecycle

- `scenario_id`: `frdp-p01-codex-asset-lifecycle-roundtrip`
- `host`: Codex
- `claim_id`: `asset-lifecycle`
- `short_workflow`: `roundtrip`
- `marker`: `FRDP-P01-frdp-p01-codex-asset-lifecycle-roundtrip`
- `Automated boundary status`: NOT_RUN
- `Automated boundary candidate_refs`: Phase 11 lifecycle package/readback/continue/rollback probes; exact implementation references are NOT_DISCOVERED_YET.
- `Automated boundary remaining`: NEEDS_VERIFICATION of backup/export/import/share/continue-training/rollback artifacts and load probes.
- `Local desktop status`: NOT_RUN
- `Local desktop activation`: NOT_DISCOVERED_YET for Codex restored/rolled-back behavior path.
- `Local desktop evidence required`: screenshot/video/log showing restored or rolled-back marked behavior; NOT_CAPTURED.
- `Local desktop remaining`: NEEDS_VERIFICATION of Codex host-visible lifecycle result.
- `evidence`: NONE_COLLECTED_IN_PHASE_01_01
- `remaining`: execute lifecycle package probes and local Codex evidence later.
- `downgraded_claim`: NEEDS_VERIFICATION; no lifecycle claim is open.

### Row 27 - Codex / checkpoint-promotion

- `scenario_id`: `frdp-p01-codex-checkpoint-promotion-reload`
- `host`: Codex
- `claim_id`: `checkpoint-promotion`
- `short_workflow`: `reload`
- `marker`: `FRDP-P01-frdp-p01-codex-checkpoint-promotion-reload`
- `Automated boundary status`: NOT_RUN
- `Automated boundary candidate_refs`: Checkpoint candidates.
- `Automated boundary remaining`: NEEDS_VERIFICATION of checkpoint hash, compatibility/load, quality, promotion owner, rollback, and fresh-load evidence.
- `Local desktop status`: NOT_RUN
- `Local desktop activation`: NOT_DISCOVERED_YET for Codex reload from promoted checkpoint.
- `Local desktop evidence required`: screenshot/video/log proving host reload uses candidate checkpoint; NOT_CAPTURED.
- `Local desktop remaining`: NEEDS_VERIFICATION of Codex reload behavior.
- `evidence`: NONE_COLLECTED_IN_PHASE_01_01
- `remaining`: execute checkpoint gates and local Codex reload proof later.
- `downgraded_claim`: NEEDS_VERIFICATION; no checkpoint mutation/promotion claim is open.

### Row 28 - Codex / locator-update

- `scenario_id`: `frdp-p01-codex-locator-update-reload`
- `host`: Codex
- `claim_id`: `locator-update`
- `short_workflow`: `reload`
- `marker`: `FRDP-P01-frdp-p01-codex-locator-update-reload`
- `Automated boundary status`: NOT_RUN
- `Automated boundary candidate_refs`: Locator candidates.
- `Automated boundary remaining`: NEEDS_VERIFICATION of actual runtime locator source of truth, locator diff, fresh-load, rollback path, and ownership.
- `Local desktop status`: NOT_RUN
- `Local desktop activation`: NOT_DISCOVERED_YET for Codex reload from updated locator.
- `Local desktop evidence required`: screenshot/video/log proving host uses locator-selected candidate; NOT_CAPTURED.
- `Local desktop remaining`: NEEDS_VERIFICATION of Codex host reload from locator.
- `evidence`: NONE_COLLECTED_IN_PHASE_01_01
- `remaining`: execute locator gates and local Codex reload proof later.
- `downgraded_claim`: NEEDS_VERIFICATION; no locator update claim is open.

## Proof That Every Row Has Both Placeholders

Every scenario block above contains these placeholders: Automated boundary status
as `NOT_RUN`, Automated boundary remaining work as `NEEDS_VERIFICATION`, Local
desktop status as `NOT_RUN`, Local desktop activation as `NOT_DISCOVERED_YET`,
Local desktop evidence requirement as screenshot/video/log not captured, Local
desktop remaining work as `NEEDS_VERIFICATION`, evidence as
`NONE_COLLECTED_IN_PHASE_01_01`, and downgraded claim wording as
`NEEDS_VERIFICATION` or a narrower future downgrade rule.

Targeted verification after writing this file must count 28 `scenario_id` rows, 28 `Automated boundary status` rows, 28 `Local desktop status` rows, and 28 `downgraded_claim` rows.

## Bridge To Phase 02 Evidence Contract

This bridge fixes `BLOCKING-01 - Missing explicit Phase 02 schema/artifact bridge`.
It derives the Phase 02 schema/artifact needs from the existing 28 inventory
rows only. It does not add, remove, or renumber rows.

Phase 02 must consume every inventory row as one planned scenario contract. The
required row identity fields are:

- `scenario_id`: stable row id and primary scenario key.
- `host`: one of Claude, Cursor, or Codex.
- `claim_id`: the claim family being tested or downgraded.
- `marker`: visible marker that must join the Automated boundary path to the
  Local desktop path.

Phase 02 must define two packet types per row:

- `automated_boundary_packet`: copied identity fields plus `Automated boundary
  status`, `Automated boundary evidence`, and `Automated boundary remaining`.
- `local_desktop_packet`: copied identity fields plus `Local desktop status`,
  `Local desktop activation`, `Local desktop evidence`, and `Local desktop
  remaining`.

Phase 02 must also preserve the result/downgrade fields needed by later reports:
`evidence`, `remaining`, `downgraded_claim`, `non_claims`, `blocker_or_NONE`,
and a non-claim status that keeps planned rows separate from proven runtime
readiness.

Per-claim typed payload needs:

- `token-savings` rows need token payloads for workload identity,
  tokenizer/version, baseline tokens, candidate tokens, repeat counts, savings
  calculation, quality guard, evidence, remaining work, and downgraded_claim.
- `semantic-quality` rows need semantic quality payloads for held-out case id,
  rubric, baseline score, candidate score, leakage checks, answer evidence,
  remaining work, and downgraded_claim.
- `learnability-training` and `dpo-quality` rows need learnability, training,
  and DPO payloads for dataset provenance, allowed-use status, train/eval split,
  leakage gates, before/after behavior, checkpoint diff, rollback evidence,
  remaining work, and downgraded_claim.
- `asset-lifecycle` rows need asset lifecycle payloads for backup/export/import,
  share, continue-training, rollback, package manifest, readback evidence,
  remaining work, and downgraded_claim.
- `checkpoint-promotion` rows need checkpoint payloads for candidate hash,
  compatibility/load result, quality gate, promotion owner, rollback evidence,
  host reload evidence, remaining work, and downgraded_claim.
- `locator-update` rows need locator payloads for locator source of truth,
  locator diff, fresh-load result, rollback locator, per-host reload evidence,
  remaining work, and downgraded_claim.
- `codex-chat-ui` rows need Codex chat UI payloads for real Codex app/chat UI
  activation, visible marker, screenshot/video/log evidence, helper-vs-real-UI
  separation, exact blocker if unavailable, remaining work, and
  downgraded_claim.

Required Phase 02 join keys and downgrade rules:

- Join `automated_boundary_packet` and `local_desktop_packet` by `scenario_id`
  plus `marker`.
- Reject join attempts when `host` or `claim_id` does not match on both packets.
- Keep `NOT_RUN`, `NEEDS_VERIFICATION`, `NOT_DISCOVERED_YET`, `NOT_CAPTURED`,
  and exact future blocker statuses as non-claim states until real evidence
  exists.
- If one path is blocked or partial, Phase 02 must require an exact blocker,
  write a downgraded_claim, keep `remaining` work explicit, and continue other
  scenarios instead of stopping the whole phase.
- A single Automated boundary packet, Local desktop packet, ledger row,
  screenshot-only record, helper-only Codex proof, or broad checklist row must
  not open full readiness claims.

Required Phase 02 validation expectations that should become schema/tests:

- schema rejects missing `scenario_id`, `host`, `claim_id`, or `marker`.
- schema rejects duplicate `scenario_id` + `marker` pairs across the 28 rows.
- schema rejects packets missing path status, evidence, remaining, blocker, or
  downgraded_claim fields.
- schema rejects a joined report that lacks both Automated boundary and Local
  desktop packet references or exact blocker/downgrade records.
- tests must verify token, semantic quality, learnability, DPO, asset lifecycle,
  checkpoint, locator, and Codex chat UI typed payload fields are present for
  their relevant `claim_id` rows.
- tests must verify no schema default can convert `NOT_RUN` or
  `NEEDS_VERIFICATION` into a readiness pass without evidence.

This bridge still does not run or prove runtime readiness. It is only the
human-readable contract that Phase 02 should turn into schema and tests before
later runtime, host UI, token, semantic quality, learnability, DPO, asset
lifecycle, checkpoint, locator, or Codex chat UI evidence can be joined.

## Explicit Non-Claims

- I did not run product code.
- I did not run Claude, Cursor, or Codex host UI tests.
- I did not run browser checks, external AI, web tools, training, DPO construction, checkpoint mutation, checkpoint promotion, locator update, token measurement, or semantic quality evaluation.
- I did not capture screenshots, videos, or logs.
- I did not verify any host is installed or controllable.
- I did not prove product readiness, full extension readiness, full Codex chat UI readiness, token savings, semantic quality improvement, learnability/training, DPO quality, checkpoint promotion, locator update, or host runtime pass.
- This is an inventory and status artifact only, not a completed runtime cycle.
