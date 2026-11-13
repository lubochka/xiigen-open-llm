# Row15 Prep: Cursor DPO Quality Provenance

subagent_id: ROW15_PREP_DPO_QUALITY_PROVENANCE_READ_ONLY_01
generated_at_utc: 2026-06-26T11:18:02Z
scenario_id: frdp-p01-cursor-dpo-quality-provenance
scenario_number: 15
host: Cursor
artifact_type: prep evidence only

## Honesty Guard

claim_scope: Row15 prep only; no Row15 eval, UI, training, checkpoint mutation, locator update, promotion, git action, product readiness, or full semantic quality was performed or claimed.
evidence_scope: Existing board/state files, Row13/Row14 DPO row evidence, Phase 03 DPO dataset output, Phase 10 DPO downgrade, and DPO quality/provenance docs were read.
count_claimed: 28 total scenarios, 3 scoped accepted, product readiness full pass 0.
independent_evidence_count: 8 read/create evidence groups before write; final JSON parse and phrase readback must be recorded after file creation.
subagent_id_or_NONE: ROW15_PREP_DPO_QUALITY_PROVENANCE_READ_ONLY_01
downgrade_needed: no, because this packet is explicitly prep-only.

## Row15 Scope

Row15 DPO quality and provenance must prove only that existing DPO candidate rows and related Row13/Row14 evidence are traceable, source-disjoint, non-leaky, non-vendor-distilled, correctly split between train and eval, and safe for a later bounded decision about whether any row can remain training input.

Row15 must not prove product readiness, full semantic quality, token savings, common model readiness, checkpoint promotion, locator update, or host behavior readiness. JSONL presence, DPO schema validity, DPO loader tests, one staged continue-training export, or one held-out/fresh-load selection are not enough for a full readiness claim.

The evidence supports only: DPO quality/provenance prep can define a non-mutating audit over existing evidence. It does not support readiness/full semantic quality.

Required fixed counts:

| count | value |
|---|---:|
| Total scenarios | 28 |
| Scoped accepted rows | 3 |
| Product readiness full pass | 0 |

Literal count readback: product readiness full pass 0.

## Existing Evidence Inventory

| evidence | what can be referenced | narrow claim only |
|---|---|---|
| `product-readiness-scenario-board.json` | Row15 board row says DPO dataset loader tests passed but train/eval quality metrics were not run; Cursor local DPO-derived behavior was not run; no joined pass. | Row15 is not moved and needs non-mutating DPO provenance/eval metrics first. |
| `phase-03-command-output/05-dotnet-dpo-dataset.txt` | Existing `DpoDatasetTests` command output reports 7 passed loader/shuffle tests. | Loader/schema utility evidence only, not DPO quality or training proof. |
| `phase-10-learnability-dpo-intake-downgrade.json` | Existing downgrade says DPO schema/guards are not DPO quality, and training/eval/fresh-load/continue evidence were missing at that time. | Useful overclaim lock; partly superseded by later Row14 scoped evidence. |
| `row13-status.json` | Row13 scoped accepted; 12 candidate-only DPO rows, 4 held-out rows, staged candidate improved 0/4 to 4/4, proxy shortcut audit, Cursor TargetLocator UI proof. | Row13 semantic-quality scope only; not Row15 DPO provenance completion. |
| `row13-1-training-report.json` and `row13-1-candidate-only-dpo-rows.jsonl` | 12 candidate-only rows, four target skills, universalOnly true, vendorOutputDistillationIssue false, quarantine accepted-for-candidate-only wording, no common/prod promotion. | Good source for provenance audit but rows need explicit schema/provenance metric readout. |
| `row14-status.json` | Row14 scoped accepted; Row14.2 DPO rows, Row14.3 staged continue-training/export, Row14.4 held-out/fresh-load eval. | Row14 scoped only; product readiness remains 0. |
| `row14-2-dpo-summary.json` and `row14-2-skillselection-dpo-rows.jsonl` | 2 usable rows for `client-adaptive-learning-loop`; source-disjoint true; universal-only true; no vendor output true; quarantine row count 0; held-out probe not copied into training. | Best existing Row15 candidate set for a non-mutating provenance audit. |
| `row14-3-staged-continue-training-export-report.json` | Reviewed Row14.2 rows trained into staged export from Row14.1 backup base; base pair count 8 to staged 10; source inputs unchanged; no UI, locator update, promotion, production mutation, or readiness claim. | Staged evidence only; not product readiness. |
| `row14-4-heldout-fresh-load-report.json` | Eval-only held-out/fresh-load selected `client-adaptive-learning-loop`; accuracy 1.0; staged hashes unchanged; trainingPerformed=false; UI not run. | Scoped eval evidence only; not UI proof and not full semantic quality. |
| `dpo-triple-format-SKILL.md`, `dcm-dpo-construction-GUIDE.md`, `docs/projects/11-dpo.md`, `claude-ILEARNINGDYNAMICGRAPH-DPO-PLAN-v1.md` | Define required fields, source/license gates, quarantine reasons, source-disjoint split, no closed API distillation, chosen/rejected contrast, and DPO metric boundaries. | Contract/source for Row15 metrics, not execution proof. |

## Missing Non-Mutating Metrics

Row15 execution must produce a machine-readable audit with these metrics:

| metric | required check | pass condition |
|---|---|---|
| Row schema validity | Parse JSON/JSONL and count rows by schemaVersion, rowId, pairId, target machine, target adapter, prompt/task, available skills, chosen, rejected, teaching point, flags. | Every referenced row parses and every required field is present or explicitly reported missing. |
| Source-disjoint | Compare Row13 rows, Row14.2 rows, Row14.4 held-out probe, row ids, pair ids, sourcePromptEntryIds, prompt fingerprints, skill targets, file/path tokens, and candidate orders. | No train/eval overlap; held-out probe text not copied into training; all overlaps classified. |
| Universal-only | Check `universalOnly`, `universalOnlyRationale`, and absence of host UI behavior, repo path, locator mechanics, project command detail, or candidate order as the learned signal. | Rows train a reusable boundary, not a project/host shortcut. |
| No vendor output | Check `noVendorOutputDistillationNote`, `vendorAndPrivacy.noVendorOutput`, `externalAiUsed`, raw private text flags, and closed API distillation claims. | no vendor output, no copied vendor answer, no external AI training evidence unless explicitly allowed. |
| Quarantine decisions | Count KEEP, EVAL_ONLY, QUARANTINE, REWRITE, REMOVE decisions; verify held-out rows are not train rows. | Every row has a quarantine decision and reason; any missing/unsafe row is not train-ready. |
| Chosen/rejected contrast | Verify chosen/rejected are non-identical, plausible near-miss rejected exists, chosen wins for a stated teaching point, and confidence/second-pass fields are present when available. | The contrast teaches one semantic decision boundary and not row id/path/order leakage. |
| Provenance fields | Check sourcePromptEntryId, source root or synthetic-source note, source/license/private leg, source hash when available, provider/model/request/terms fields when applicable, train/eval role, leakage flags, decision boundary, teaching point. | Missing fields are counted and blocked from broad train-ready claims. |
| Privacy/distillation | Confirm rawPrivateTextCopied=false, no private prompt leakage, no local path leakage in trainable features, no closed API distillation, and source/license route is explicit. | Any private/vendor/unknown-license issue becomes quarantine or evidence-only. |
| Training/eval separation | Verify Row14.2 train rows, Row14.3 staged training step, Row14.4 eval-only held-out probe, and staged hash unchanged after eval. | Evaluation rows are not copied into training; Row15 itself performs no training/eval/UI. |

## Cursor UI Proof Decision

Row15.A can be non-UI provenance-only. DPO quality/provenance is a dataset and evidence audit, so it can be prepared and executed without Cursor UI.

The broader Row15 board scenario still includes "host behavior tied to candidate." Therefore a later Row15.B Cursor UI proof is needed only after Row15.A passes and a DPO-derived candidate behavior is selected for host-visible comparison. The smallest UI step later is: run a no-mutation Cursor host-visible check for the same DPO-derived behavior or held-out probe used by the non-UI path, capture the actual UI result, and compare it to the automated path. A JSON/log/marker screenshot alone is not UI proof.

UI is not authorized in this prep packet and should not be bundled into the next immediate work order.

## Overclaim Locks

- product_readiness_claimed=false
- product readiness full pass 0
- full_semantic_quality_claimed=false
- token_savings_claimed=false
- training_readiness_claimed=false
- ui_run_by_this_packet=false
- eval_run_by_this_packet=false
- training_run_by_this_packet=false
- checkpoint_mutation_or_promotion_claimed=false
- locator_update_claimed=false
- full_extension_readiness_claimed=false
- ready_true_claimed=false
- git_action=false

## Exact Next Execution Gate

Next immediate gate: `ROW15_DPO_QUALITY_PROVENANCE_NON_MUTATING_EXECUTION_01`.

Execution may start only as a bounded non-mutating provenance audit. It may parse existing JSON/JSONL and write audit MD/JSON under a new Row15 timestamped folder. It must not run eval/UI/training, mutate checkpoints/locators/models, promote, stage, commit, or push.

Pass condition: the audit reports row-level metrics for schema validity, source-disjoint, universal-only, no vendor output, quarantine, chosen/rejected, provenance fields, privacy/distillation, and training/eval separation, with all missing fields and unsafe rows downgraded.

Fail/needs-fix condition: any required row file cannot parse, any required metric is missing, any train/eval overlap is found, any vendor/private/unknown-license issue is not quarantined, or any artifact claims product readiness/full semantic quality.

## Next Bounded Work Order

```text
work_order_id: ROW15_DPO_QUALITY_PROVENANCE_NON_MUTATING_EXECUTION_01
requested_artifact_type: evidence audit MD/JSON only
current_luba_quote: "Prepare Row15 `frdp-p01-cursor-dpo-quality-provenance` by identifying existing DPO provenance evidence, missing non-mutating provenance metrics, required Cursor host-visible proof, overclaim locks, and exact next execution gate."
current_authority_basis: this prep packet after parent review/acceptance
do_not_delegate: true
implementation_authorized_now: no, this is the next work order text only
allowed_read_scope:
- product-readiness-scenario-board.json
- row13-status.json and Row13 DPO evidence listed in this packet
- row14-status.json and Row14.2/14.3/14.4 evidence listed in this packet
- phase-03 DPO dataset output
- phase-10 DPO downgrade
- DPO quality/provenance docs needed for metric definitions
allowed_write_scope:
- new timestamped evidence folder under docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-cursor-dpo-quality-provenance/
- audit MD/JSON only
allowed_verification:
- parse generated JSON
- parse referenced JSON/JSONL files read-only
- targeted readback for: DPO quality, provenance, source-disjoint, no vendor output, quarantine, chosen/rejected, product readiness full pass 0
forbidden_verification:
- broad filesystem sweeps outside named evidence/doc scopes
- dotnet test/build
- eval/UI/training
- checkpoint/locator/model mutation
- promotion
- git stage/commit/push/status/diff actions
required_metrics:
- row schema validity
- source-disjoint
- universal-only
- no vendor output
- quarantine decisions
- chosen/rejected contrast
- provenance fields
- privacy/distillation checks
- training/eval separation
required_counts:
- total scenarios 28
- scoped accepted 3
- product readiness full pass 0
cursor_ui_policy:
- Row15.A is non-UI provenance-only.
- Row15.B Cursor UI proof is a separate later gate only after Row15.A passes.
commit_push_boundary_owner: parent after review only
git_authority: forbidden
max_active_time: 10 minutes
parent_will_perform_commit_push_after_review: true
quoted_transcripts_state_old_plans_policy: evidence only, not current authority
direct_luba_questions_policy: answer first and interrupt work order
```

## Required Phrase Readback

DPO quality
provenance
source-disjoint
no vendor output
quarantine
chosen/rejected
product readiness full pass 0
