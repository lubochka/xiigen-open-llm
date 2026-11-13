<!-- OCT-2026-SUPERSESSION-REVIEW-FOR-JULY-TP1 -->
**2026-10-14 SOURCE AUTHORITY CORRECTION (not a legal signoff, no real training performed):** The `CODE_FIRST → TP1_LOCAL_EVAL_ONLY → G_DPO_FINAL → DPO_TRAINING` and `DPO_LAST` statements below are a **July 2026 historical plan**, not verified binding restrictions for October. Their authorship/correctness as user-approved durable limitations was not proved; they could be a prior-model mistake. October D/09.09.26WorkLeft-02-teaching-chinese-model.md (R16, 13 Oct) and D/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-V6-FAST-LEARNING-CONTRACT-2026-10-14.md require starting a genuine lawfully sourced teacher-authored two-sided DPO lesson and Student update promptly. **Do NOT elevate the old `TP1_LOCAL_EVAL_ONLY` or `G_DPO_FINAL` to a fresh training prohibition merely because this file says 'current'.** Preserve this section as historical design evidence; investigate its original reason separately, without blocking first E1 on speculation. ACTUAL safety still requires lawful model release/output use, authorised source/context/privacy, true provider identity and exact currently enforced K technical admission, immutable judged pair and one-use optimizer journal. User-facing human selection and exact pair judging are separate **CODEX GPT-6 Astra UltraCode** acts; **approved external open Chinese model** writes BOTH final sides. This amendment does not remove code-enforced safeguards or claim a model call was performed.
<!-- /OCT-2026-SUPERSESSION-REVIEW-FOR-JULY-TP1 -->

## Current Code-First Reconciliation Overlay (2026-07-14)

current_implementation_authorization: YES_CODE_FIRST_ONLY
teacher_dpo_training_authorization: NO_TEACHER_DPO_TRAINING_UNTIL_G_DPO_FINAL
dpo_status: NOT_STARTED
dpo_priority: LAST
current_counters: 12 phases / 27 subphases / 64 substeps
canonical_sequence: CODE_FIRST -> TP1_LOCAL_EVAL_ONLY -> TP2_HOSTED_EVAL_ONLY (only after G-TP-4) -> G_DPO_FINAL -> DPO_TRAINING
current_effective_train_ready: false; neither TP1 nor TP2 starts DPO, emits a train-ready pair, calls `TrainBatchAsync`, or promotes a checkpoint.
business_vs_technical_gate: `APPROVED_BY_LUBA_2026-07-07` is the separate business approval for the future hosted Qwen route; it is not `ProviderTermsProof`. TP2 may start only after `G-TP-4`, and DPO authoring/training may start only after `G_DPO_FINAL`.
authoring_boundary: DPO chosen/rejected material may be authored only by the project's own deterministic graph/self-best route or by a permitted open-weight model after the exact license, provider/output-use, provenance, data, evaluation, reference-checkpoint, margin, lifecycle, fresh-load/continue-training, and quantization gates pass. Closed proprietary APIs may judge/evaluate only where allowed; they never author `chosen`, `rejected`, a DPO label, or a training target.
current_open_gate_status: reference-checkpoint, reward-margin, held-out/source-disjoint evaluation, lifecycle/fresh-load/continue-training, and quantization gates remain OPEN; no DPO execution readiness or whole-plan readiness is claimed.

## Hosted Qwen Business Approval Record (2026-07-07) — current business decision, not current DPO authorization

decision: consumer mirror of owner `active-codex-execution-plan-08-legal-teacher-gate.md` §3.5.1b.
The hosted `qwen3-together-v1` route has the separate business decision `APPROVED_BY_LUBA_2026-07-07`.
The historical approval-carrier encoded `ProviderTermsChecked=true`, `TermsSnapshotRef="LUBA_DECISION_2026-07-07(...)"`,
`terms_basis=LUBA_DECISION_2026-07-07`, `teacher_output_used_for_training=true`,
`closed_proprietary_model=false`, `model_license=Apache-2.0`. That encoding is business-decision evidence,
not the technical `ProviderTermsProof`: current TP2 remains blocked until `G-TP-4`, and current DPO remains
blocked until `G_DPO_FINAL`. The generic C6 machine rule is unchanged for other hosted routes.
scope_fence: SPECIFIC to `qwen3-together-v1` only. DeepSeek(MIT) same-clean-license footing but NOT
blanket-approved; Kimi(modified-MIT)/GLM/Mistral keep per-model verify-before-use; closed models
(Claude/OpenAI/Gemini) stay FORBIDDEN.
operational_preconditions_for_future_TP2: ZDR-first, provenance JSON per call, pin serving id at cutover,
recommended verbatim archive; these do not replace `G-TP-4` or `G_DPO_FINAL`.
basis_recorded: legal-part whitelist (Qwen3 Apache-2.0 = training/distillation permitted, Luba primary
teacher); Together ToS 7.2 (customer owns Output, no no-distillation clause found); ZDR ToS 3.3; Luba
«Выведи маршрут из карантина». Recorded as a Luba product decision, not a lawyer sign-off. This overlay
does NOT declare the whole plan READY and does NOT lift implementation authorization.

## Two-Phase Local Teacher Profile Overlay (2026-07-13) — qwen35-local-v1 (consumer mirror, владелец plan-08 §3.5.2)

decision: зеркало владельца `active-codex-execution-plan-08-legal-teacher-gate.md` §3.5.2. Профиль Этапа 1
`qwen35-local-v1` (TP1_LOCAL): `HostingProvider=LocalOllama`, `HostRole=self_hosted`, `ModelFamily=Qwen3`
(явный алиас Qwen3.5->Qwen3), `ModelId=Qwen/Qwen3.5-9B`, `ModelLicense=Apache-2.0`
(PROVISIONAL_FROM_FAMILY_CANON до per-release проверки, execution-гейт G-TP-2/SI_GATE_10),
`ServiceMode=plain`, `GroundingUsed=false`, `ClosedProprietaryModel=false`,
`FutureTeacherOutputUsedForTrainingIntent=true`, effective `TeacherOutputUsedForTraining=false` until
`G_DPO_FINAL`, `ExternalDataTransfer=false`, `Role=teacher`,
`TeacherAccessMode=local_self_hosted_open_weight`, `TermsBasis=SELF_HOSTED_NO_PROVIDER_TERMS`.
approval_separation: основание — `LUBA_TWO_PHASE_DECISION_2026-07-13` (выбор маршрута); одобрение
`APPROVED_BY_LUBA_2026-07-07` относится только к `qwen3-together-v1` и НЕ переносится.
terms_axis: провайдерских ToS нет; вместо них локальная retention policy
(`SELF_HOSTED_LOCAL_RETENTION_POLICY_v1`) + пин весов по sha256-digest + deployment proof.
c6_scoping: `terms_snapshot_missing` НЕ применяется к self-hosted маршруту; его отказная нога —
`self_hosted_retention_or_pin_missing` (владелец: plan-08 §2.2 шаг 4 / §4 C6). Readiness не меняется:
TP1 остаётся `EVAL_ONLY`; teacher/DPO authorization = `NO_TEACHER_DPO_TRAINING_UNTIL_G_DPO_FINAL`.

## Historical Fable Coverage Repair Overlay (2026-07-05)

historical_fable_plan_readiness: PLAN_NOT_READY / FABLE_GAP_REPAIR_QUEUE
historical_fable_implementation_authorization: NO until fresh Luba approval after review
historical_fable_verdict_basis: Newer Fable coverage evidence `claude-fable-plan-check-v1.md` and attachment `98cbe69a` superseded the previous final-readiness overlay. Fable reported 127 findings: 91 COVERED, 30 PARTIAL, 6 MISSING. Mental implementation from active-codex would still hit plan-level design/source/dependency blockers: R-B 20/40 knowledge-library artifacts were not accepted built artifacts; R-G source addressability was incomplete; several shared contracts/readability/lifecycle carriers needed plan-family repair. This is historical plan-repair evidence only, not product implementation, tests/builds/runtime/provider/training/checkpoint/git proof.
historical_fable_verdict_classes: DESIGN_BLOCKER_CURRENT=FABLE_MISSING_AND_PARTIAL_ITEMS; CURRENT_REPAIR_QUEUE=FABLE_RB_RG_AND_PARTIAL_REPAIR; FABLE_EVIDENCE=claude-fable-plan-check-v1.md lines 35-153 and attachment 98cbe69a lines 32-79; CURRENT_LEDGER_ACCEPTANCE_BOUNDED_EVIDENCE=previous 50/50 planning-section acceptance was historical/process evidence only; IMPLEMENTATION_AUTHORIZATION_GATE=NO until fresh Luba approval after review.
historical_fable_classification_rule: This block is historical evidence and cannot override the 2026-07-14 code-first overlay.
historical_fable_gap_repair_addendum: active-codex-execution-42-fable-gap-repair-additions-20260705.md was the repair map for R-B/R-G and partial contract/readability/lifecycle additions.
## Historical Pre-Fable Final Readiness Review Overlay (2026-07-05)
historical_pre_fable_plan_readiness: PLAN_READY_FOR_LUBA_REVIEW
historical_pre_fable_implementation_authorization: NO until fresh Luba approval after review
historical_pre_fable_final_readiness_review_scope: one bounded planning/governance readiness review only; no product implementation, tests, builds, runtime, browser, provider/API, model/training/checkpoint, git, branch, mergeability, PR, or product-readiness proof was run.
historical_pre_fable_current_verdict_basis: This final review read the required governance/skill gate markers, the two unique original-task/reference attachments, the minimum Claude baseline surfaces, the active-codex plan/board/matrix/source-map/legal/gate/ledger surfaces, and current status/search evidence. End-to-end mental simulation from the active plan found no unresolved design/dependency decision that an executor would have to choose before coding/testing. The prior PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE state after 0aa/Laplace/Kepler/Singer was retained for parent final-review acceptance; this overlay supplies that final bounded review. Remaining items are execution or approval gates only: fresh Luba implementation approval, local self-authored seed file creation, code implementation, tests/builds/runtime/training/checkpoint/fresh-load/ablation proof, same-scenario Path A/Path B host proof, mock/replacement-ledger proof, branch/mergeability/git proof if later authorized, final product review, and route-specific hosted TogetherAI/Qwen3 terms/output-use proof before hosted train/common use.
historical_pre_fable_current_verdict_classes: DESIGN_BLOCKER_CURRENT=NONE_IDENTIFIED_BY_FINAL_READINESS_REVIEW; CURRENT_REPAIR_QUEUE=NONE_IDENTIFIED_BY_FINAL_READINESS_REVIEW; CURRENT_LEDGER_ACCEPTANCE_BOUNDED_EVIDENCE=50/50 accepted planning sections after 2026-07-05 Pass37-Pass50/Singer repair, not 50 cycles, not 50 product reviews, not product proof, not test/build/runtime/provider/training/checkpoint/git proof; HOSTED_PROVIDER_PRE_USE_GATE_ROUTE_SPECIFIC=TogetherAI/Qwen3 proof required before hosted train/common use only, not a local self-authored v1 planning blocker; IMPLEMENTATION_AUTHORIZATION_GATE=NO until fresh Luba approval after review.
historical_pre_fable_classification_rule: This pre-Fable review is historical evidence only and cannot override the 2026-07-14 code-first overlay.
## Historical 43c2 Readiness Downgrade Overlay (2026-07-02; superseded by 2026-07-14 overlay)

Historical boundary only: this section cannot override the current 2026-07-14 code-first overlay.

historical_pre_final_review_plan_readiness: PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE
historical_43c2_implementation_authorization: NO
latest_attachment: C:\Users\lubao\.codex\attachments\43c2bc75-9134-4cb2-8ad3-8985d2689bb6\pasted-text.txt
historical_43c2_evidence_scope: Attachment 43c2 reports newer Claude PLAN_PARTIAL / NOT READY evidence with two plan-level residues. It is evidence-only, not this worker's pass count, not a sub-agent packet, and not implementation proof.
historical_43c2_residue_queue:
- plan-09 B-15 / IGraphWeaknessDetectorUnit edge/graph wording: v1 train-ready DPO rows remain unit-level only; edge/graph training, scoring, acceptance, and promotion are deferred unless a later explicit Luba-approved plan expands scope.
- plan-06 fan-in merger wording: current canon is IEdgeFanInMerger.Merge(GraphFanInMergeInput input) -> OperationResult<GraphFanInMergeResult>, not a bare LogicalFlowResult return.
historical_pre_final_current_verdict_basis: Newer evidence attachment 0aa5730c, the Erdos bounded worker packet, and the 2026-07-05 Pass37-Pass50/Singer correction supersede stale READY and stale acceptance-count overlays. Historical/pre-Fable evidence supported PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE: current-ledger 50-pass acceptance is repaired as bounded planning-section evidence (50 headings exist and strict acceptance-field/readability evidence is 50/50 after the 2026-07-05 Pass37-Pass50/Singer repair). This queue-classification repair finds no current plan/design blocker in the stale CURRENT_REPAIR_QUEUE labels: F0 locks are recorded with implementation proof pending; legal/provenance is propagated in active plan, policy, and source-map surfaces; hosted-provider proof is a route-specific external pre-use gate before hosted Qwen3/Together train/common use, not a local self-authored v1 planning blocker; future execution proof remains after approval; fresh Luba post-review approval is implementation authorization, not a plan-design blocker. This is not 50 cycles, not product readiness, not PLAN_READY, not B-22 closure, and not implementation authorization.
historical_pre_final_current_verdict_classes: DESIGN_BLOCKER_CURRENT=NONE_IDENTIFIED_BY_THIS_QUEUE_CLASSIFICATION; CURRENT_LEDGER_ACCEPTANCE_BOUNDED_EVIDENCE=50/50 accepted planning sections after 2026-07-05 Pass37-Pass50/Singer repair, not cycles/readiness/product/implementation proof; CURRENT_REPAIR_QUEUE=NONE_IDENTIFIED_BY_THIS_QUEUE_CLASSIFICATION; F0_LOCKS_RECORDED_IMPLEMENTATION_PROOF_PENDING=Q3/Q6/Q8/Q11/Q14/Q30/Q38 recorded, Q37/Q-host implementation checks only; LEGAL_PROVENANCE_PROPAGATED_IN_PLAN_EXECUTION_PROOF_PENDING=20-field TeacherProvenance, ProviderProof, LicenseProof, LegalVerdict, C16/C17 quarantine/block/rebuild behavior present in active plan/policy/source-map surfaces; HOSTED_PROVIDER_PRE_USE_GATE_ROUTE_SPECIFIC=TogetherAI/Qwen3 proof required before hosted train/common use, not local self-authored v1 planning blocker; IMPLEMENTATION_AUTHORIZATION_GATE=fresh Luba post-review approval required before execution, not plan-design blocker; EXECUTION_PROOF_PENDING_AFTER_APPROVAL=code implementation, seed file creation, tests/builds/runtime, mock execution/replacement, training/checkpoint/fresh-load/ablation, same-scenario dual-path proof, branch/mergeability/git; readiness_candidate_for_parent_review=ZERO_CURRENT_DESIGN_BLOCKERS_FOUND_BY_THIS_QUEUE_CLASSIFICATION; parent must run final readiness review before any PLAN_READY wording; HISTORICAL_PRE_0AA_EVIDENCE=older PLAN_READY_FOR_LUBA_REVIEW / READY and older NOT_READY records unless a later evidence-bound repair proves current status.
## Historical/pre-43c2 Pass50 Readiness Verdict Overlay (2026-07-02)

historical_pre_43c2_plan_readiness: PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE
historical_pre_43c2_implementation_authorization: NO until fresh Luba approval after review
historical_pre_43c2_pass_chain: 50 canonical pass headings exist and strict current-ledger acceptance-field/readability evidence is now 50/50 after the 2026-07-05 Pass37-Pass50 bounded acceptance repair recorded in active-codex-execution-41-current-ledger-50-pass-acceptance-index.md. This is 50 accepted planning pass sections, not 50 cycles, not product readiness, not PLAN_READY, not B-22 closure, and not implementation authorization.

Historical/pre-43c2 Pass50 basis:
- Original-task R1-R10 are FULLY_COVERED_PLAN_LEVEL by Pass46 and this Pass50 adversarial review.
- B-1..B-22 have no remaining implementable active-codex design/dependency blocker under the Pass39/Pass43/Pass44 current overlays. Hosted provider proof remains required only before hosted train/common use.
- Source-to-Plan inventory is 64/64 by exact canonical row-id audit, with 0 noncanonical row-like offenders.
- Claude/latest-baseline details are carried in active-codex: five participating repos, DPOLegalLearning work name, C:\Projects workdir boundary, latest-updated source-branch authority as active/Codex delta, xiigen-platform facade plus xiigen-flows seam, FigmaToCode/CanvaToCode as clients/callers, unit-level 14-field GraphDpoPair, 20-field TeacherProvenance, exactly one active language-translate pilot, and provider/terms boundary.
- QA, tests, builds, screenshots, same-scenario dual-path proof, mock replacement, provider/API proof, runtime/training/checkpoint proof, branch/mergeability, git, PR, and product review remain future execution-continuation gates after fresh Luba approval. They are not design decisions and were not run in Pass50.
- This Pass50 overlay is historical/pre-43c2 evidence only. Newer attachment 43c2 supersedes it; READY wording in this block is not current readiness, not B-22 closure, and not implementation authorization.

Readback: This Pass50 paragraph is historical/pre-43c2 evidence only. Attachment 43c2 triggered a downgrade, the residues were repaired, and the historical/pre-0aa post-repair-review active-codex verdict was PLAN_READY_FOR_LUBA_REVIEW; historical/pre-Fable 2026-07-02 bounded-repair verdict was PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE; implementation_authorization remains NO.
---
artifact_type: dpo_legal_model_policy
authority_source: latest unquoted Luba work order for DPO Legal Learning active execution chain
status: blocker_4_closed_in_plan_provider_terms_future_evidence
last_updated: 2026-06-29
write_scope: C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build\active-codex-execution-*.md
---

# DPO Legal Model Policy

## Policy Scope

This policy governs model use for DPO data generation, DPO training, judging, and evaluation for `ILearningDynamicGraph`. It is a planning policy, not legal advice and not provider approval. Provider terms and model licenses were not checked in this session.

## Core Rule

Closed proprietary API output is never an author of `chosen`, `rejected`, a DPO label, or a training target. It may be retained only as judge/evaluation evidence where the exact service terms, privacy mode, no-grounding boundary, and separate `JudgeProvenance` permit that use. The project's own deterministic graph/self-best route, or a permitted open-weight model, may author DPO material only after the exact model-license, provider/output-use (when hosted), provenance, data, evaluation, reference-checkpoint, reward-margin, lifecycle, fresh-load/continue-training, quantization, and `G_DPO_FINAL` gates pass. Until then every route is evaluation/evidence-only and `DpoTrainReady=false`.

### NO OVERRIDE — closed-output authoring prohibition

No license proof, provider terms proof, business approval, user selection, judge score, or later gate may
convert closed proprietary output into authored/training material. Closed proprietary output is
permanently judge/evaluation-only and may never author or supply `chosen`, `rejected`, a DPO label,
a goal used as training text, a distillation target, or any training row. Proof can permit only the
judge/evaluation use itself. Hosted open-weight/permissive output is a different route and may author
only with exact model/license/provider/output-use/privacy/provenance proof plus the full predicate and
`G_DPO_FINAL`; otherwise it remains Candidate/eval/quarantine with `DpoTrainReady=false`.

## PASS25_RR6 Owner-Canon Overlay (2026-06-30)

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.
This policy is a consumer of owner block 08, not a second field-list authority.
The current owner canon is `active-codex-execution-plan-08-legal-teacher-gate.md`
mirroring Claude Repair2/RR6:

- `TeacherProvenance` has 20 fields: 13 base + 5 NEW + 2 RR6
  (`service_mode`, `grounding_used`).
- `provenance_field_presence_rate==1.0` means all 20 fields, not 18.
- C17 blocks `used_for_training && (service_mode!="plain" || grounding_used==true)`
  with reason `service_mode_grounding_for_training`; judge grounding/search is
  `judge_grounding_used`.
- Gemini Grounding / Google Search / any web retrieval is blocked for training
  even when the model family is allowlisted and denylist does not trigger.
- Model license is not provider terms approval by itself. **BUSINESS APPROVAL ONLY (current consumer
  mirror of owner block 08 §3.5.1b):** the hosted `qwen3-together-v1` route carries
  `APPROVED_BY_LUBA_2026-07-07`, but that decision is not a technical `ProviderTermsProof` and does
  not make `teacher_output_used_for_training=true`. `SelectProvider` may expose it only in
  `TP2_HOSTED_EVAL_ONLY` after `G-TP-4`; train/common authoring stays disabled until the separate
  full gate predicate and `G_DPO_FINAL` pass. (Historical/pre-approval,
  SUPERSEDED 2026-07-07: "Hosted Qwen3/Together remains `ProviderTermsChecked=false` with
  `TermsSnapshotRef=PENDING_LUBA_OR_EXTERNAL_TERMS_REVIEW(...)`; `SelectProvider` must not
  choose it for train/common use until a real terms snapshot replaces pending.") The generic
  C6 rule still applies to any other hosted route whose terms are not approved.
- Closed/hosted judge C11 reads only separate `JudgeProvenance` fields:
  `judge_provider_terms_checked==true`, `judge_terms_proof_status==POSITIVE`, immutable
  `judge_terms_snapshot_ref` + `judge_terms_snapshot_sha256`, `judge_service_mode=="plain"`,
  `judge_grounding_used==false`, complete judge privacy/retention fields,
  `judge_used_for_training==false`, and `judge_did_not_author_chosen_rejected==true`.
  Missing/invalid judge proof is C11-blocked QUARANTINE; judge grounding is C17 BLOCK;
  teacher provenance fields never substitute for judge proof.
- **Pass02 (2026-07-13, зеркало владельца §3.1/§2.2):** `teacher_access_mode` — 6 значений
  (`first_party_contract | open_weight_hosted | local_self_hosted_open_weight | public_domain_standard | human_authored | closed_proprietary`);
  C6 hosted-нога оценивается только при `teacher_access_mode==open_weight_hosted`; self-hosted профиль
  `qwen35-local-v1` не кварантинится по `terms_snapshot_missing` — его terms-ось = retention policy + пин digest
  (отказная нога `self_hosted_retention_or_pin_missing`); C9 использует производный предикат
  `external_data_transfer(prov)` (локальный маршрут не триггерит C9; privacy-защита common сохранена через
  `privacy_scope`/`redaction_status`).

## Closed API Prohibitions

| prohibited use | reason | disposition / required evidence |
|---|---|---|
| Train common model on closed API outputs | Closed output authoring/training is an absolute boundary | FORBIDDEN; no proof or approval overrides it |
| Use closed API as hidden teacher for common DPO rows | Closed output cannot be teacher authoring material | FORBIDDEN; no proof or approval overrides it |
| Use closed API as judge without terms review | Judge use still requires its own legal/privacy proof | Evaluation-only terms, retention, and complete `JudgeProvenance`; proof enables judge/eval only |
| Store raw project code or private text in hosted provider logs | Conflicts with local and privacy boundary | Redaction plus provider retention approval |
| Treat public model access as training permission | Access is not the same as derivative training rights | License and terms proof |
| Use grounding/web-search/retrieval service mode for train/judge call | Output may mix third-party web content outside the model license and provider terms proof | `service_mode=="plain"` and `grounding_used==false` (C17) |

## Candidate Conditions

| candidate type | allowed now | possible future allowed use | required proof |
|---|---|---|---|
| Self-hosted open-weight model | план-профиль `qwen35-local-v1` (TP1_LOCAL, владелец plan-08 §3.5.2); execution-гейты pending | Teacher Этапа 1 после G-TP-1/G-TP-2 (deployment proof + per-release license) | Model license (per-release, SI_GATE_10), model card, version, **запиненный sha256-digest весов**, allowed derivative use, локальная retention policy, deployment proof |
| Hosted open model endpoint | not training source now | Evaluation-only or teacher only if provider terms allow | Hosted terms, retention policy, output-use permission |
| Closed API provider | blocked for training now | Limited judge or evidence route only after explicit terms | Terms snapshot, no-distillation proof, Luba approval |
| Human or Luba labels | allowed planning route | Gold chosen and rejected examples | Consent, provenance, source id |
| Project-local deterministic output | allowed as rejected or baseline candidate | Low-quality local rejected rows or baseline comparison | Local source id, generation config, no private leak |

## Provenance Fields

Every teacher/source Candidate uses the block-08 `TeacherProvenance` owner list. Every judge result
uses separate `JudgeProvenance`; the legacy `TeacherProvenance.judge_model` field is only a sentinel,
never judge terms/privacy/provenance proof. Historical generic lists below are not a current owner override.
Current 20 teacher fields:

```text
teacher_access_mode
hosting_provider
model_id
model_license
model_version
request_id
provider_terms_checked
teacher_output_used_for_training
closed_proprietary_model
source_ref
source_sha256
contamination_markers
judge_model
terms_snapshot_ref
geography_export_constraint
source_category
raw_private_text_copied
redaction_policy_ref
service_mode
grounding_used
```

Current separate judge fields (exact owner names; not part of the 20 teacher fields):

```text
judge_access_mode
judge_hosting_provider
judge_model_id
judge_model_version
judge_request_id
judge_provider_terms_checked
judge_terms_proof_ref
judge_terms_snapshot_ref
judge_terms_snapshot_sha256
judge_terms_proof_status
judge_service_mode
judge_grounding_used
judge_external_data_transfer
judge_raw_private_text_copied
judge_redaction_policy_ref
judge_privacy_terms_ref
judge_zdr_status
judge_retention_policy_ref
judge_used_for_training
judge_did_not_author_chosen_rejected
judge_output_ref
judge_output_sha256
```

For any closed/hosted judge, missing required fields routes to
`C11-blocked QUARANTINE(judge_provenance_missing)`; the judge result references this
`JudgeProvenance` record and never reuses `TeacherProvenance` as its carrier.

## Provider Terms Gate

| gate | pass condition | fail condition |
|---|---|---|
| provider identity | Provider and model id are exact | Provider or model unknown |
| terms snapshot | Terms snapshot ref stored with date | No terms snapshot |
| output-use rights | Terms explicitly allow planned training or judging use | Terms silent or restrictive |
| privacy retention | Raw project text handling is allowed or avoided | Provider may retain sensitive content without approval |
| license compatibility | Open-weight license permits derivative training route | License absent or incompatible |
| service mode (RR6) | `service_mode=="plain"` and `grounding_used==false` | grounding/web_search/retrieval/code_exec/unknown mode for train or judge |
| self-hosted retention (Pass02, TP1_LOCAL) | локальная retention policy записана + weights digest запинен + deployment proof present; `ExternalDataTransfer=false` | retention policy / пин digest / deployment proof отсутствует → `QUARANTINE` `self_hosted_retention_or_pin_missing` (НЕ `terms_snapshot_missing`) |
| Luba approval | Current instruction approves exact route | Old plan or transcript approval only |

## Provider/Provenance Proof Contract - Blocker 4 Closure

Current blocker 4 verdict: `CLOSED_IN_PLAN_EVIDENCE_GATE`.

This policy is specified in plan now. Current provider terms, model licenses, and
output-use rights were not verified in this planning repair. Real proof
collection is therefore a future execution/evidence gate before any real
training, common dataset publication, or training-readiness claim. Missing proof
is not a design blocker and not a runtime question for Luba: implementation must
deterministically choose quarantine, eval-only, block, or rebuild-required
according to this contract.

### TeacherProvenance Storage Pattern

`TeacherProvenance` remains a sidecar/reference contract, consistent with the
Q3/Q6 plan decisions:

- `LearningBatch.SourceIds` references sidecar JSONL manifests with
  `schema_version`.
- Trainable pair records carry `TeacherProvenanceRef`, `ProviderProofRef`, and
  `LicenseProofRef`, not only provider/model names.
- Common/global evidence and sidecars live under the shared `llm_mvp_core`
  training/model evidence area selected by implementation.
- Personal/adaptive evidence and sidecars live under the client/project
  `.xiigen` area until common clearance exists.
- Closed proprietary output NEVER enters any trainable `GraphDpoPair`, JSONL, or dataset,
  regardless of provider/license proof, business approval, or requested scope. Proof can permit
  only separate judge/evaluation evidence.
- Hosted external output may enter a trainable Pair/dataset only when the exact model is
  open-weight/permissive, separately invoked as teacher, and every provider/output-use/license/
  privacy/provenance/data/evaluation/lifecycle gate plus `G_DPO_FINAL` passes.

### ProviderProof Fields

Future implementation must persist a `ProviderProof` record before any hosted or
external provider output can become train-ready:

| field | required meaning |
|---|---|
| `providerProofId` | Stable id referenced by `ProviderProofRef`. |
| `providerName` | Hosting or API provider name. |
| `providerServiceOrSourceUrl` | Official provider terms/source URL or local source reference. |
| `termsSnapshotRef` | Immutable terms snapshot/evidence file reference. |
| `termsSnapshotHash` | Hash of the captured terms snapshot. |
| `snapshotCapturedAtUtc` | Timestamp for the captured terms proof. |
| `checkedBy` | Tool or human verifier id. |
| `requestedUse` | `JudgeOnly`, `EvalOnly`, `TrainPersonalAdaptive`, or `TrainCommon`. |
| `outputUseAllowed` | Positive/negative/unknown output-use result. |
| `trainingUseAllowed` | Positive/negative/unknown training/distillation result. |
| `retentionPolicyRef` | Provider retention/privacy policy reference when applicable. |
| `redactionPolicyRef` | Redaction policy applied before provider call or storage. |
| `geoExportConstraint` | Export/geography restriction if present; otherwise explicit none/unknown. |
| `proofStatus` | `POSITIVE`, `PENDING`, `REJECTED`, or `EXPIRED`. |
| `allowedUse` | Narrowest permitted use after terms evaluation. |

### Pass35 Storage/Privacy Scope Fields

Every row that requests training or promotion must carry:

| field | required meaning |
|---|---|
| `storageScope` | `CommonGlobal`, `PersonalAdaptive`, `EvalOnly`, or `Quarantine`; v1 common training accepts only `CommonGlobal` rows with positive proof. |
| `privacyScope` | `common_allowed`, `personal_adaptive_only`, `redacted_common_candidate`, or `forbidden`. |
| `dataClass` | `synthetic_self_authored`, `project_internal`, `client_private`, `client_unverified`, `third_party`, `closed_provider_output`, or `hosted_open_weight_pending_terms`. |
| `commonTrainReady` | True only when legal/provenance/license/terms/output-use/redaction/split proof is positive and no Luba cross-boundary approval is missing. |
| `clientAdaptiveReady` | True only for a later activated personal/adaptive route; it must never mutate common/global assets. |
| `quarantineReason` | Required for any blocked, eval-only, quarantine, or common-denied row. |

Missing, pending, contradictory, mock-only, private/client/third-party, or approval-missing proof returns `commonTrainReady=false` and routes to `PersonalAdaptive`, `EvalOnly`, `Quarantine`, `BLOCK_FOR_COMMON`, or `DATASET_REBUILD_REQUIRED`, never silent common/global training.

### LicenseProof Fields

Future implementation must persist a `LicenseProof` record before local,
self-hosted, or open-weight model output can become train-ready:

| field | required meaning |
|---|---|
| `licenseProofId` | Stable id referenced by `LicenseProofRef`. |
| `modelId` | Exact model id used as teacher/judge/source. |
| `modelVersion` | Exact revision, release, tag, or checkpoint version. |
| `modelArtifactHash` | Local artifact hash or official checkpoint hash where available. |
| `modelSourceRef` | Official repository/model-card/license source reference. |
| `licenseName` | License identifier/name. |
| `licenseSnapshotRef` | Immutable license snapshot/evidence file reference. |
| `licenseSnapshotHash` | Hash of captured license proof. |
| `derivativeTrainingAllowed` | Positive/negative/unknown derivative-training result. |
| `outputUseAllowed` | Positive/negative/unknown output-use result. |
| `redistributionConstraint` | Dataset/output redistribution constraint if present. |
| `checkedBy` | Tool or human verifier id. |
| `checkedAtUtc` | Verification timestamp. |
| `proofStatus` | `POSITIVE`, `PENDING`, `REJECTED`, or `EXPIRED`. |

### LegalVerdict Values And Transitions

The legal gate uses the plan-08 verdict set:
`ACCEPT`, `QUARANTINE`, `BLOCK`, `DATASET_REBUILD_REQUIRED`,
`BLOCK_FOR_COMMON`, and `EVAL_ONLY`.

| scenario | required transition |
|---|---|
| Hosted/open-weight teacher output with missing provider terms or model license proof | `LegalVerdict=QUARANTINE`, `allowedUse=EvalOnly` or `EvidenceOnly`, `dpoTrainReady=false`. |
| Hosted `qwen3-together-v1` (Qwen3-Apache-2.0-via-Together), business-approved by `APPROVED_BY_LUBA_2026-07-07` | Business approval is separate from `ProviderTermsProof`. Before `G-TP-4`: `QUARANTINE/BLOCK`, not selectable. After `G-TP-4`: only `TP2_HOSTED_EVAL_ONLY`, with `TeacherOutputUsedForTraining=false`. It becomes authoring/train-eligible only after all legal/provider/provenance/data/evaluation/lifecycle gates and `G_DPO_FINAL`; current `DpoTrainReady=false`. |
| Local self-hosted `qwen35-local-v1` (TP1_LOCAL, owner §3.5.2) with pinned digest, retention policy, deployment proof, per-release license verified, `service_mode="plain"`, `grounding_used=false`, full 20-field provenance and triad | `TP1_LOCAL_EVAL_ONLY`; `LegalVerdict=ACCEPT` may mean legally/evidentially eligible for the proven scope, but never train-ready before the complete predicate and `G_DPO_FINAL`. `TeacherOutputUsedForTraining=false`; `ExternalDataTransfer=false`. |
| Local self-hosted `qwen35-local-v1` без пина digest / retention policy / deployment proof, или с непроверенной per-release лицензией | `QUARANTINE` `self_hosted_retention_or_pin_missing` (лицензионная ось: `BLOCK(license_family_mismatch)` при расхождении declared vs canonical); не train-ready, не common-ready. |
| Historical Qwen3/Together pending-proof record | Its business selection is preserved, but the technical rule remains current: `ProviderTermsChecked=false`, `TermsSnapshotRef=PENDING_POSITIVE_HOSTED_TERMS_PROOF`; before `G-TP-4` BLOCK/QUARANTINE, after it TP2 eval-only. C6 has no business-decision exception. |
| Allowlist model with `service_mode!="plain"` or `grounding_used=true` | `LegalVerdict=BLOCK`, reason `service_mode_grounding_for_training` (C17), even when denylist does not trigger. |
| Closed/best hosted provider used only as judge/eval and not as chosen/rejected training content | Keep judge/eval result only with complete separate `JudgeProvenance`: positive `judge_*` terms proof, plain/no-grounding, privacy/retention proof, `judge_used_for_training=false`, and non-authoring true. Otherwise C11-blocked `QUARANTINE`, judge-C9 `BLOCK_FOR_COMMON`, or C17 `BLOCK`. Closed output can never author a Pair regardless of proof. |
| Closed/proprietary provider output used to author chosen/rejected trainable content | `LegalVerdict=DATASET_REBUILD_REQUIRED`; quarantine affected rows; rebuild from allowed teacher/source. |
| Self/local/open-weight/permissive teacher with full `TeacherProvenanceRef`, applicable `ProviderProofRef`, `LicenseProofRef`, hashes, redaction, and positive output/training rights | `LegalVerdict=ACCEPT` is necessary but not sufficient. Train-ready only after data/split/evaluation/reference/margin/lifecycle/fresh-load/continue-training/quantization gates and `G_DPO_FINAL`; current `DpoTrainReady=false`. |
| Client or third-party output without common clearance | Personal/adaptive/quarantine by default; common use requires positive proof, redaction, per-client terms snapshot, and Luba approval where the plan already requires it. |
| Mocked provider metadata or `codex-external-calls-mock.md` placeholder evidence | Treat as `PENDING`; return quarantine/eval-only; never train-ready or common-ready from mock metadata alone. |

### Future Evidence Files Before Training/Common-Use Claims

Before claiming real training readiness or common dataset readiness, future
implementation must produce evidence files equivalent to:

- `provider-proof-manifest.jsonl`
- `license-proof-manifest.jsonl`
- `teacher-provenance-manifest.jsonl`
- `legal-verdict-ledger.jsonl`
- `dpo-training-batch-manifest.jsonl`
- `self-hosted-retention-policy-qwen35-local-v1.json (+hash)`
- `self-hosted-deployment-proof-qwen35-local-v1.json (Ollama/Docker versions, model digest, readback, date)`
- external-call/mock-debt ledger proving which calls were mocked and which were
  replaced by verified provider/license evidence
- unit/integration test output for quarantine, eval-only, train-ready, and
  rebuild-required decisions

### Required Future Unit Tests

Future implementation must add tests for at least these decision cases:

- `MissingTermsHostedOpenWeight_ReturnsQuarantine_DpoTrainReadyFalse`
- `ClosedJudgeEvalOnly_DoesNotCreateTrainablePair`
- `ClosedTeacherTraining_ReturnsDatasetRebuildRequired`
- `SelfLocalPermissiveFullProof_ReturnsTrainReadyForProvenScope`
- `ClientThirdPartyCommonWithoutClearance_ReturnsBlockForCommonOrQuarantine`
- `ClientOutput_DefaultsAdaptiveQuarantine_NoCommonTrain`
- `PromotionToCommon_RequiresProvenanceLicenseConsentHeldOutLubaApproval`
- `ProviderMetadataMockPending_BlocksTrainingCommonUse`
- `TeacherProvenanceAndProofRefs_RequiredForGraphDpoPair`
- `SelfHostedLocalTeacher_FullPinRetentionProof_ReturnsTrainReadyForProvenScope`
- `SelfHostedLocalTeacher_MissingPinOrRetention_ReturnsQuarantine_NotTermsSnapshotMissing`
- `SelfHostedProfile_DoesNotInheritTogetherApproval`
- `FamilyOf_Qwen35Alias_MapsToQwen3_NearMissQuarantined`

## Allowed Planning Defaults

1. Prefer self-hosted open-weight candidates after license verification.
2. Prefer human or project-local labels for initial gold data.
3. Treat closed APIs as blocked for common training until proven otherwise.
4. Treat hosted APIs as conditional even when the model weights are open.
5. Separate `eval_only`, `evidence_only`, `train_personal`, and `train_common` in provenance.
6. Keep raw code and private project text local unless a future privacy gate explicitly allows external use.
---
---

# Active Codex Execution Addendum - DPO Legal Learning

## Current Live Authority And Evidence Boundary

This file is an active execution copy of the Claude baseline artifact. The original `claude-*` file remains unchanged. Claude remains the baseline because it is stronger by Luba criteria: human clarity, readable architecture, architectural reliability, evidence honesty, requirement fidelity, execution-control quality, and low overclaim risk. `codex-*` files are delta/source-map evidence only, not the baseline and not product-readiness proof.

Current active task source is Luba's latest live unquoted instruction for the DPO Legal Learning active plan chain. Prior sub-agent packets are evidence to verify, not live commands. The Heisenberg/no-branches/git-realignment branch of work is explicitly NOT THE TASK and must not be revived as a blocker, cleanup scope, branch operation, commit/push reason, or planning-repo git task.

Codex delta inputs retained for this addendum:

- `C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build\codex-EXECUTION-CONTROL-BOARD.md`: matching branch/source-map notes for `DPOLegalLearning`; not repo authority and not a source for expanding scope.
- `C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build\codex-REQUIREMENTS-COVERAGE-MATRIX.md`: matching notes for Claude latest-updated branch authority and `DPOLegalLearning` working branch/work name; not independent authority.
- `C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build\codex-ILEARNINGDYNAMICGRAPH-PLAN-v1.md`: crosswalk and plan-only branch authority wording.

Do not claim Codex 50/50 cycles as proven product readiness. Ledger rows, checklist rows, generated tables, and summaries are not completed cycles unless independently inspectable review/fix evidence records exist for the exact claim.

## Active Requirement Count Strategy

The active execution requirement count strategy is:

- original Claude `R1-R20` / master `R-01..R-20` baseline remains the quality and honesty baseline;
- Claude baseline, with matching non-contradictory Codex delta evidence, defines `R21`: latest-updated implementation source-branch authority plus the `DPOLegalLearning` work name rule for implementation repo copies/workdirs under `C:\Projects`;
- current Luba execution requirements are live addendum requirements and must be checked by pre-implementation, pre-commit, pre-push, and before-stop gates;
- the prior no-branch planning-repo policy packet is quarantined evidence only and is not an active task, stop gate, branch-cleanup requirement, or permission to touch planning-repo git state.

This strategy is an execution-control strategy, not product-readiness proof.

## R21 - Source Authority, Working Directory, And DPOLegalLearning

For each participating repository/source role before implementation:

1. Identify the source authority as the most advanced/latest-updated branch in that repository at the time work starts, using read-only git commands.
2. Treat the source authority branch as read-only authority, not as the implementation work branch.
3. Because `C:\Project` does not exist and `C:\Projects` exists in this environment, use `C:\Projects` as the implementation root. Create or use a separate working directory for the work name `DPOLegalLearning`, with repo copies under `C:\Projects\DPOLegalLearning\<repo-name>` unless an existing active plan row names a more specific safe `C:\Projects` path.
4. The controlling repo-by-repo authority/source-to-plan table is `active-codex-execution-02-source-inventory-and-reference-map.md` R21.1. During future implementation, record the concrete repo path or URL, branch-list source, selected latest-updated branch, selected branch timestamp/ref evidence, divergence note, separate `C:\Projects` working directory, and work name `DPOLegalLearning`.
5. Participating Claude-baseline route/source identifiers are `llm_mvp_core` (core implementation), `xiigen-platform-dot-net-angular` (public API facade for R16/R17), `xiigen-flows-dot-net-angular` (FLOW-31/FLOW-38 internal seam), and client/caller labels `FigmaToCode-design-gen` + `CanvaToCode` (R16 html/css clients). FigmaToCode/CanvaToCode are not implementation-repository authority for the platform host unless a later exact source proves that. Codex may be used only as matching delta/source-map evidence. This list must not be expanded from discovered/copied workdirs unless Luba explicitly expands repo scope.
6. Do not switch, create, delete, rebase, merge, cherry-pick, push, or open PRs in `C:\Users\lubao\Documents\xiigen`. That planning location may be read only as an evidence source when needed; no git realignment or branch cleanup is part of this chain.

## Execution Actor And Stop Gate

Future implementation, if later authorized, must run through bounded sub-agent work packets, requested model/mode `gpt-5.5` / `xhigh` (user wording: gpt 5.5 extra high). Parent orchestrates and no unbounded executor is allowed. Each packet must name exact scope, allowed files/repos, max time, required report packet, accepted evidence, and forbidden actions. A spawned bounded sub-agent is already the delegated execution actor for its allowed scope and must not invoke nested Codex/Claude/CLI agents unless the work order explicitly asks for nested delegation. Ledger rows, tables, summaries, and self-review are not fake cycles and cannot be reported as sub-agent passes.

Stop is allowed only when the product is fully ready, or when a true external blocker remains after mitigation/discovery and decision-prep. Before any stop, the worker must check whether the active plan is complete. If incomplete, the worker must identify the loophole that would cause premature stop, repair the governing plan/gate/skill text inside the allowed scope, and continue the nearest safe authorized work. NOT READY, NEEDS_FIX, review findings, partial evidence, and missing verification are bounded work queues, not completion claims.

## Pre-Commit And Pre-Push Merge/PR Conflict Gate

Before every commit and before every push during implementation:

1. Re-read the current source authority branch for each participating implementation repo because the source branch may advance in parallel.
2. Validate that merge/PR into the original source authority branch copied from has no conflicts.
3. Record the exact command or tool evidence used for the no-conflict result.
4. If conflicts exist, stop the commit/push path, repair or prepare a decision packet, then re-check against the current source authority.
5. Do not create PRs and do not merge unless Luba gives a separate future instruction.

This gate does not authorize any branch operation, commit, push, merge, PR, or cleanup in `C:\Users\lubao\Documents\xiigen`.

## External APIs And Mock-Only Technical Debt Gate

Future implementation, if later authorized, must handle every external API/resource path through an explicit seam/interface/adapter boundary, mock the expected external response instead of making the real external call, and add a unit test that asserts exactly the expected mock response/result. Record replacement of the mock with the real external resource call only in:

`C:\Users\lubao\Documents\xiigen\llm mvp\DPO LEARNING\codex-external-calls-mock.md`

This mock replacement record is the only allowed technical debt. No other technical debt may be introduced under this active plan. Mock-only evidence must not be reported as real external integration readiness.

## Historical 2026-06-29 Planning-Repair Boundary (superseded by 2026-07-14 overlay)

Historical boundary only: this section cannot override the current 2026-07-14 code-first overlay.

At that historical time (2026-06-29), Luba's then-current instruction was `План не готов к выполнению - нужно починить`, and that planning-repair worker was authorized only to repair plan files. Product implementation, workdir setup, tests/builds, and git operations were not authorized for that historical task. This record is superseded and cannot override the 2026-07-14 code-first overlay.

At that historical time, source `claude-*` and `codex-*` files were treated as read-only and PRs, merges, commits, pushes, branch switches, workdir copying, and implementation edits were forbidden for that old planning-repair task. Those old prohibitions do not govern current execution; only the 2026-07-14 overlay and current Luba instructions do.

## Historical Pass36 / Pass37 Qwen3/Together Provider-Terms Classification (superseded)

The Pass36/Pass37 readiness verdicts and their old `NO until fresh Luba approval` implementation
boundary are historical evidence only. The 2026-07-14 overlay now authorizes code-first
implementation, but it does not authorize teacher/DPO authoring, Pair projection/materialization,
or training before the full predicate and `G_DPO_FINAL`.

Pass36 was a plan-only classification and did not establish legal advice, provider API proof, model/API
calls, product/test/runtime proof, or readiness. Its 2026-07-01 evidence references were Qwen GitHub
`https://github.com/QwenLM/Qwen3` lines 525-531, Qwen blog
`https://qwenlm.github.io/blog/qwen3/` lines 29-40, Together ToS
`https://www.together.ai/terms-of-service` lines 174-199, and Together privacy/security
`https://docs.together.ai/docs/privacy-and-security` lines 136-166.

Current hosted route status is `QWEN3_SELECTED_RECOMMENDED_PROVIDER_TERMS_PROOF_PENDING`.
Qwen3 model-license evidence is model-license evidence only; it is not hosted provider output-use
proof, legal approval, or train-ready proof.

The business route selection `APPROVED_BY_LUBA_2026-07-07` is already recorded separately. It is
business/consent evidence only and is **not** technical proof: it cannot flip
`provider_terms_checked`, satisfy `G-TP-4`, remove C6/quarantine, or authorize teacher authoring,
Pair creation, dataset materialization, or training. Before positive `G-TP-4` technical proof, the
hosted route remains `BLOCK` / `QUARANTINE`. After positive `G-TP-4`, it may advance only to
`TP2_HOSTED_EVAL_ONLY`; authoring/training authority remains blocked until the full predicate and
`G_DPO_FINAL`.

## Historical Pass37 Legal Policy Sync (superseded by current two-phase overlay)

Pass37 historically superseded the Pass36 optional-disabled wording. The current technical
requirement is only a positive `G-TP-4` proof bundle for advancement from `BLOCK` / `QUARANTINE`
to `TP2_HOSTED_EVAL_ONLY`. No future Luba approval is requested or required: the already-recorded
business approval remains a separate non-proof field and cannot substitute for any technical gate.
Qwen3 Apache-2.0 remains model-license evidence only, not hosted provider-output-use proof and not
legal advice.

Required positive `G-TP-4` `ProviderTermsProof` bundle before TP2 hosted evaluation:

- `ProviderTermsProof.Provider=TogetherAI`
- `ModelFamily=Qwen3`
- exact `ModelId`
- official source URLs and snapshot timestamp/date
- Qwen model license source showing Apache-2.0
- Together ToS snapshot ref
- Together privacy/security/ZDR organization-setting snapshot, not only a personal setting
- `passthrough` disabled, or upstream passthrough policy captured if enabled
- `service_mode=plain`
- `grounding_used=false`
- `provider_terms_checked=true`
- `terms_snapshot_ref` non-pending
- `data_retention_mode=ZDR_or_no_store`
- `output_ownership_checked=true`
- `third_party_model_terms_checked=true`
- `business_approval_ref=APPROVED_BY_LUBA_2026-07-07` as already-recorded separate non-proof
  business/consent evidence; it never satisfies `provider_terms_checked` or `G-TP-4`

Route outcomes until the proof bundle passes:

| route | current outcome |
|---|---|
| hosted Qwen3/Together with missing, pending, personal-only, passthrough-uncaptured, non-plain, grounding/search, or third-party-terms-missing proof | `BLOCK` or `QUARANTINE`; no TP2 call, authoring, Pair, dataset, train-ready, or common-ready use |
| hosted Qwen3/Together after positive `G-TP-4` | `TP2_HOSTED_EVAL_ONLY`; if used as judge, also requires complete separate `JudgeProvenance` with `judge_used_for_training=false`, positive immutable judge terms proof, organization privacy/ZDR/retention proof, no passthrough gap, `judge_service_mode=plain`, `judge_grounding_used=false`, and no unredacted private transfer; never authoring/training at this stage |
| self-authored/local v1 path | TP1 local evaluation may proceed independently with complete local provenance; no hosted Qwen3/Together dependency and no train-ready authority before the full predicate and `G_DPO_FINAL` |

Pass36/Pass37 labels are historical evidence only. The current rule is route-specific and fail-closed:
missing positive `G-TP-4` proof blocks TP2 but does not block independent TP1 local evaluation.
Business approval is already recorded and is never technical proof; code-first implementation may
continue, while teacher/DPO authoring, Pair materialization, and training remain blocked until the
full predicate and `G_DPO_FINAL`.

## Historical BK5 DRAFT terms_snapshot Consumer Mirror (business-decision input only)

> **Historical business-decision input only.** This mirror fed the 2026-07-07 route selection but
> never became technical proof. Current owner state remains `provider_terms_checked=false`,
> `terms_snapshot_ref=PENDING_POSITIVE_HOSTED_TERMS_PROOF`, C6 active, and `G-TP-4` open.

This policy is a consumer of owner block 08; the owner of the DRAFT terms snapshot is plan-08 §3.5.1a. The draft does not flip any proof bool, authorize a hosted call, remove C6, or create a second authority. Business approval and technical proof remain separate.

DRAFT terms_snapshot values for provider profile `qwen3-together-v1` (each labeled):

| field | DRAFT value | label | one-line rationale | grounding citation |
|---|---|---|---|---|
| `terms_snapshot_ref` | `DRAFT-together-qwen3-terms-20260707` | DRAFT_NEEDS_CONFIRM | replaces the Pass37 PENDING ref ONLY after a Luba/legal-confirmed verbatim snapshot exists | this pass (not a captured verbatim archive) |
| `provider` | `TogetherAI` (HostRole=`neutral_reseller`) | DERIVED | TogetherAI hosting a non-Together family (Qwen3) is a neutral-reseller route; reseller ToS is separate from first-party ToS | block-08 §3.5 Provider-name role disambiguation; ADD-08 Mirror |
| `model_id` | `Qwen/Qwen3-235B-A22B` (pin exact serving id at cutover) | PROPOSED_DEFAULT empirically-tunable | exact id to pin; re-pin to the provider's exact serving id at cutover and cross-check ObservedModelId (served_model_mismatch → BLOCK) | HF model card `Qwen/Qwen3-235B-A22B`; block-08 §3.5.1a |
| `model_license` | `Apache-2.0` | DERIVED forced | confirmed live on the HF Qwen3 model card; matches CanonicalLicenseOf(FamilyOf(Qwen3))=Apache-2.0 so C5 license axis passes; license != provider ToS approval | HF Qwen3 model card license; Pass02R-P07 family→canonical map |
| `output_use_permitted` | `PRELIMINARY_FAVORABLE_UNCONFIRMED` | RECOMMENDATION Luba-confirm | preliminary favorable reading only; NOT a confirmed output-use grant and NOT no-distillation clearance | Together ToS 7.2 (customer owns Output); no explicit no-distillation clause observed; Together ZDR ToS 3.3 |
| `enable_zdr_before_first_request` | `true` (ZDR=Zero Data Retention, forward-only) | RECOMMENDATION Luba-confirm | ZDR is forward-only; enable at org level BEFORE the first request (no retroactive purge) | Together ZDR ToS 3.3; Pass37 org-setting (not personal) requirement |

**Current technical effect of the historical default:** hosted Qwen3/Together remains BLOCK/QUARANTINE before positive `G-TP-4` proof and TP2 eval-only afterward. The business decision did not remove C6. The local route proceeds independently only as TP1 eval-only.

**Current residual:** business approval exists separately; technical positive proof does not. The immutable hash-bound ToS/privacy/ZDR archive and exact output-use fields remain required by `G-TP-4`; `provider_terms_checked=false` until they pass.

## Pass44 Legal Policy Hosted-Proof Boundary

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

The legal policy now distinguishes three routes:

| route | current legal boundary |
|---|---|
| local self-authored/self-best v1 | Deterministic candidate/evaluation work may proceed in `CODE_FIRST`; no dataset/training promotion occurs before the complete gate predicate and `G_DPO_FINAL`. No hosted provider output is required. |
| local self-hosted open-weight teacher (`qwen35-local-v1`, TP1_LOCAL) | Separate from self-authored: `TP1_LOCAL_EVAL_ONLY` after its deployment, per-release license, digest, retention, and provenance gates. Neither TP1 nor a legal `ACCEPT` verdict starts DPO. |
| hosted Qwen3/Together (`qwen3-together-v1`, TP2_HOSTED) | `APPROVED_BY_LUBA_2026-07-07` is business approval only. Start `TP2_HOSTED_EVAL_ONLY` only after `G-TP-4` technical `ProviderTermsProof`; keep `TeacherOutputUsedForTraining=false`. No authoring/train/common promotion before the complete predicate and `G_DPO_FINAL`. |
| hosted judge/eval/mock-only | Allowed only where `used_for_training=false`, separate `JudgeProvenance` is valid, no sensitive/private raw data is sent, terms/privacy proof is non-pending, and output cannot author chosen/rejected, mutate common, or become train-ready. |

## Pass02R ADD-08 Legal Safeguards Consumer Mirror

Historical/pre-post-repair status at that pass was `PLAN_NOT_READY`; historical/pre-0aa post-repair review readiness was `PLAN_READY_FOR_LUBA_REVIEW`; historical/pre-Fable 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; implementation authorization remains `NO until fresh Luba approval after review`.

This policy consumes the latest Claude block 08 ADD-08 safeguards mirrored in `active-codex-execution-plan-08-legal-teacher-gate.md`:

- train/common acceptance requires `FamilyOf`/`AllowlistMatch` plus `CanonicalLicense` family-license coherence, not a free-form declared license;
- C5 permissive set is exact: `Apache-2.0`, `MIT`, `ProjectInternal`, `Synthetic`; no ellipsis. `RESOLVED_FROM_LEGAL_PART`: the legal part «Kimi K2 modified MIT … надо проверить до использования» makes `modified-MIT` a **verify-before-use** disposition, so Kimi is NOT pre-admitted to C5 (defaults to `BLOCK`/`EVAL_ONLY` until per-model license+terms verified) — this is a documented legal-part mandate, not an open Luba question;
- `RESOLVED_FROM_LEGAL_PART`: Qwen3 (Apache-2.0) is the **primary permitted teacher** and the only v1 instantiated teacher family (`qwen3-together-v1`, terms proof pending). Per «Kimi K2 modified MIT … надо проверить до использования» and «проверить лицензию конкретной модели» (GLM/Mistral), DeepSeek, Kimi/Moonshot, GLM, and Mistral are verify-before-use / not train-ready for v1 until exact per-release provider/config/license/terms/output-use proof is verified; disposition is documented in the legal part (`active-codex-execution-plan-09-auto-dpo-loop.md:1272-1274 (allowed teacher families / Разрешённые teacher-семейства)`; audit `active-codex-execution-original-task-authority-audit-20260629.md:57`), not an open Luba decision;
- provider names that can be first-party vendor and host/reseller must be role-split into `ModelFamily/Vendor` and `HostingProvider/Transport`; host proof never replaces canonical-license proof, and family license proof never replaces hosted output-use terms proof;
- provider response metadata must include `ObservedModelId`; served-model family mismatch or closed-model substitution is `BLOCK(served_model_mismatch)` or the closed-teacher route;
- `used_for_training` must be reconciled against effective chosen origin; false judge/teacher labels cannot make a training target judge-only;
- closed judge output cannot author chosen/rejected text; the `AntiAuthoringGuard` 280-char/unparsable-output rule is a policy requirement;
- first-party/self-best baseline rows require `BaselineProvenanceWriter` 20-field sentinel provenance rather than empty teacher-only fields.

These are not future choices. Future unit tests must prove them, and failing tests route to bounded repair, but implementation may not invent weaker legal policy.

## Pass02R-P07 Family→Canonical-License Map Consumer Mirror

Historical/pre-Fable 2026-07-02 bounded-repair verdict was `PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE`; current verdict after this Fable-partial P-07 repair remains `PLAN_NOT_READY`; implementation authorization remains `NO until fresh Luba approval after review`.

This policy consumes the explicit `family_canonical_license` map now instantiated on `AllowedTeacherRegistry` in `active-codex-execution-plan-08-legal-teacher-gate.md` (owner block; see its `Pass02R-P07 Family→Canonical-License Invariant Instantiation`). The prior consumer bullet ("`CanonicalLicense` family-license coherence, not a free-form declared license") named the invariant but did not carry the values; the values are carried here for single-owner parity:

- `Qwen3 → Apache-2.0` (`RESOLVED_FROM_LEGAL_PART`: **primary permitted teacher**; member of the C5 permissive set; hosted terms proof still separate/pending);
- `DeepSeek → MIT` (member; not v1-instantiated; per-model provider/output-use proof still required before use);
- `Kimi (Moonshot) → modified-MIT` (`RESOLVED_FROM_LEGAL_PART`, verify-before-use: legal part «Kimi K2 modified MIT … надо проверить до использования»; **NOT** the plain `MIT` of the C5 set; `modified-MIT ≠ MIT`; **NOT pre-admitted to C5**; defaults to `BLOCK`/`EVAL_ONLY` until per-model license+terms are verified before use, including whether the attribution/use notice affects distillation into the common model — this per-model verification is the documented legal-part mandate, not an open Luba question);
- `GLM (Zhipu/Z.ai) → per-model` (`RESOLVED_FROM_LEGAL_PART`, verify-before-use: legal part «проверить лицензию конкретной модели»; heterogeneous; no single canonical license; heterogeneity flag; C5 `BLOCK` until a per-release canonical row is verified before use);
- `Mistral (Mistral/Mixtral) → per-model` (`RESOLVED_FROM_LEGAL_PART`, verify-before-use: legal part «проверить лицензию конкретной модели»; heterogeneous; mix of Apache-2.0 and non-permissive releases; heterogeneity flag; C5 `BLOCK` until a per-release canonical row is verified before use).

Policy consequence: C5 cross-checks the declared `TeacherProvenance.model_license` against `CanonicalLicenseOf(FamilyOf(model_id))` (family match). A permissive-looking declared license does not pass C5 on its own when the canonical license disagrees or is per-model/unresolved; a mismatch is `BLOCK(license_family_mismatch)`. This map is a gate input (data), not a provider/terms approval; hosted output-use terms proof remains a separate blocking gate on top of a resolved canonical license. The already-recorded Luba business approval is separate non-proof evidence and is not a remaining gate.

