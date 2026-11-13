# Plan Review Cycle 07 - DPO/source/license/material quality

## Человеческий пакет

Стадия: цикл 7 из 10 перед любым продолжением runtime/code/training/host.

Фокус: DPO/source/license/material quality. Проверка закрывает риск, что common
training начнёт учить project-specific или stack-specific execution рецепты,
частные исправления Luba, локальные пути, host install mechanics или schema-only
DPO rows вместо универсальных orchestration/skill semantics с проверенной
лицензией и конкретными positive/rejected/quarantine решениями.

Reviewer/sub-agent id: `PLAN-REVIEW-CYCLE-07-DPO-SOURCE-LICENSE-MATERIAL-QUALITY`.

Model/reasoning evidence: `model=gpt-5.5`, `reasoning_effort=xhigh`,
`downgrade_warning_or_NONE=NONE`.

Ограничения выполнены: агентов не спавнил, ветку не менял, commit/push не
делал, runtime/source code не редактировал, runtime/training/host probes не
запускал.

## Что проверено

- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md`
  sections 0, 4.4, 4.4.1, 4.4.2, 4.5.1, 7, 8, 9, 9.1, 9.2, 10, 11 and 11.1.
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`
  top-level active cycle gate, active completion gate, historical plan_review
  entries, and QSG/source/license/readiness claim fields.
- `plan-review-cycle-01..06.md/json` artifacts.
- DPO guide discovery: the plan does not directly reference
  `DPO-KNOWLEDGE-MAP-v1.md` or the other DPO guide filenames, but a targeted
  context check confirmed the same expected themes: chosen/rejected schema,
  plausible rejected rows, universal SkillSelection semantics, no raw private
  prompt copying, held-out/fresh-load/continue-training evidence.

## Находки и исправления

1. PASS_WITH_PLAN_FIX: Section 4.4 allowed common rows through broad
   source/license approval wording and still mentioned explicit Luba approval
   near MIT/Apache. That could let private or project-specific material leak
   into common training.
   Fix: Section 4.4 now requires MIT/Apache-2.0 or another verified permissive
   open-source license plus universal orchestration/skill semantics.
2. PASS_WITH_COMMON_SOURCE_CLASSIFICATION: the plan needed a concrete source
   classification matrix.
   Fix: added Section 4.4.3 with `COMMON_UNIVERSAL_PERMISSIVE`,
   `COMMON_OPEN_SOURCE_REVIEW_REQUIRED`, `PRIVATE_USER_CORRECTION`,
   `PROJECT_SPECIFIC_RUNTIME_OR_PATH`, `UNKNOWN_OR_LICENSE_MISSING`, and
   `HELDOUT_OR_ABLATION_SOURCE` routes.
3. PASS_WITH_DPO_TRIPLE_MATRIX: DPO acceptance could still be read as
   schema-only if a row had fields but no concrete positive/rejected/quarantine
   content.
   Fix: Section 4.4.3 now requires concrete `positive_chosen_ref`,
   `rejected_ref`, human-readable why fields, and a quarantine/adaptive/common
   decision. It adds per-machine acceptance rows for `PromptToTaskMachine`,
   `OrchestrationReasoningMachine`, and `SkillSelectionMachine`.
4. PASS_WITH_UNIVERSAL_VS_STACK_SPLIT: common training now teaches universal
   prompt-to-task, orchestration, and skill-selection semantics only. Exact
   repo paths, host stack commands, branch names, QSG history, local skill
   inventory, and private workflows are adaptive-only, evidence-only, or
   quarantine unless rewritten and reviewed as universal material.
5. PASS_WITH_AI_GAP_CORRECTION_SAFETY: AI-gap and user-correction DPO rows now
   require expected-behavior source, reviewer approval, redacted provenance,
   and no raw private text in common artifacts.
6. PASS_WITH_HELDOUT_ABLATION_SOURCE_DISJOINT: fresh-load, continue-training,
   held-out, and ablation gates now point to the same material-quality manifest
   and require source-disjoint rows by source ref, sha, prompt/correction hash,
   path/title, nearest-neighbour source, and material class.
7. PASS_WITH_ARTIFACT_AND_FINAL_GATES: required artifacts now include
   `common-training-source-classification.*` and
   `dpo-triple-acceptance-matrix.*`; final acceptance/readiness/no-overclaim
   wording blocks common training from private/project/stack rows and blocks
   schema-only DPO claims.
8. PASS_WITH_STATE_FIX: execution state advanced to
   `PLAN-REVIEW-CYCLE-07_OF_10`, records cycle 07 artifacts, keeps
   `implementation_authorized=false`, and sets next action to cycle 8.

## Acceptance matrix added to the plan

| Material route | Common result | Local/adaptive result |
|---|---|---|
| Universal permissive open-source material | Common accepted only with license refs, hashes, redaction, provenance, source-disjoint split, and material-quality PASS. | May be referenced as common baseline. |
| Private user correction or raw prompt | Common rejected. | User-local/adaptive-only with private refs and redacted summaries. |
| Project-specific runtime/path/host stack recipe | Common rejected as stack-specific. | Adaptive-only/evidence-only/quarantine unless rewritten as universal semantics. |
| Unknown or license-missing source | Common rejected. | Evidence-only until source/license is restored. |
| Held-out/ablation source | Eval-only. | Never reused as training material for the same target. |

## Before / after

Before:

- Common training wording allowed `MIT/Apache или explicit Luba approval`.
- Source/license gate did not separately classify universal common material,
  project-specific runtime/path material, private correction material, unknown
  license material, and held-out/ablation material.
- DPO row schema was explicit, but the plan did not require a separate concrete
  positive/rejected/quarantine acceptance matrix artifact.

After:

- Common training requires verified MIT, Apache-2.0, or another verified
  permissive open-source license plus universal orchestration/skill semantics.
- Private/project-specific/stack-specific material is user-local, adaptive-only,
  evidence-only, or quarantined.
- DPO triples must include concrete positive/chosen and rejected near-miss refs,
  why fields, quarantine/adaptive/common route, redacted provenance, source
  material class, and source-disjoint held-out/ablation refs.
- State records cycle 7 and keeps runtime/code/training/host execution locked.

## Recheck evidence

Post-patch verification status: PASS.

Проверки:

- PASS: execution state JSON parse.
- PASS: cycle 07 JSON parse.
- PASS: `git diff --check`.
- PASS: targeted `rg` for `universal`, `common`, `adaptive`,
  `MIT`, `Apache-2.0`, `open-source`, `DPO`, `held-out`, `ablation`,
  `source-disjoint`, `COMMON_ACCEPTED`, `ADAPTIVE_ONLY`, `EVIDENCE_ONLY`,
  `QUARANTINE_STACK_SPECIFIC`, `dpo-triple-acceptance-matrix`, and
  `common-training-source-classification`.

No `dotnet test` was run because this phase authorizes only
plan/state/artifact review verification, not runtime/source code verification.

## Verdict

PASS for cycle 7. Этот цикл засчитывается в requested 10 real review/fix
cycles.

cycle_counts_toward_10: true.

cycles_completed_after_this_packet: 7.

remaining_cycles_before_execution: 3.

proves_three_full_extensions_in_this_dimension: true as a plan gate for future
Claude/Cursor/Codex DPO/source/license/material-quality evidence.

product_readiness_proven_now: false.

Runtime/code/training/host execution, QSG-20, production locator update,
checkpoint mutation, commit and push remain forbidden in this sub-agent slice.
