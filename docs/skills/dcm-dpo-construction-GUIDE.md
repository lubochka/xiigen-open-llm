# DCM (DesignConversionMachine) - DPO Pair Construction Guide

## Luba Why / Loophole Repair-First Rule

When Luba asks "why" (or its operator-language equivalent, e.g. "почему"), challenges an answer, or says a failure came
from a loophole in plans, guides, skills, agent files, prompts, or governance
rules, this is an answer-first repair trigger, not a normal status question and
not permission to continue product work first.

Required order:
1. Answer Luba's why/loophole question directly.
2. Classify the previous behavior or claim as verified, overclaim, false,
   misread, or governance gap.
3. Identify the exact missing, weak, stale, or contradictory rule that allowed
   the failure.
4. Repair the governing plan, guide, skill, agent file, or start prompt first.
5. Verify the repair with direct readback, marker search, diff check, or another
   evidence-bound check.
6. Only after the repair is verified, resume the nearest already-authorized
   product task.

Do not substitute apology, status, a plan-quality note, or "I will continue" for
this repair. If no file repair is needed, state the exact evidence proving that
before continuing.

## Luba Global Plan-Quality / Skill-Stack Rule

This file inherits the global project rule: when Luba asks which plan is better,
judge by Luba's criteria: human clarity, readable architecture, architectural
reliability, evidence honesty, requirement fidelity, execution-control quality,
and low overclaim risk. Newer status tables, file counts, ledgers, count
freshness, or claimed pass counts are delta evidence only; use valid deltas to
patch the stronger plan instead of treating bookkeeping freshness as plan
superiority.

No executor session may speak as the active product executor until SK-INTENT,
SK-PLAN, SK-REVIEW, and SK-ARCH when applicable are loaded for the current task.
If the stack is not loaded, say "not loaded yet" and load it before plan
comparison, plan choice, status, executor work order, or implementation.

**Version:** 1.1
**Applies to:** DCM server-side learning pipeline for "Figma/Canva design -> HTML+CSS"
**Audience:** architect, implementer, DPO-data reviewer
**Language:** the operator's language (plain language; Russian for the original author); code fields in English
**Status:** plan-only; no implementation

---

## 0. How this skill is used

Use this guide when you need to design, review, or reject DCM DPO pairs for `CriticUnit` and `ImproverUnit`.
It defines a human-readable contract for the reviewer/architect: which fields are mandatory, who has authority over `chosen/rejected`,
which pairs go to quarantine, and what evidence is required before data is handed to training.

This guide does not authorize implementation on its own. Any DPO record, batch, or claim about model quality must be checked
against this document, the measurement guide, and the training guide. Without a source/license gate, teacher evidence,
Render-QA/eval evidence, or a 50+50 train/held-out split, a record cannot enter `Train*`.

---

## 1. Scope And Evidence Boundary

This guide defines the DCM-specific contract for constructing DPO pairs for the model's two trainable roles: `CriticUnit` and `ImproverUnit`. DPO pairs teach the model to prefer the better result of converting a design into HTML+CSS, or the better verdict about the quality of such a result.

The new data and artifact identity uses the `dcm-` prefix: `dcm-critic-*`, `dcm-improver-*`, `dcm-source-*`, `dcm-batch-*`. Translation and old `design-conversion-*` artifacts are evidence-only, not foundation. They may be read as format analogies, but must not be used as a source of architecture, authority, tier semantics, or DPO acceptance rules for DCM.

Class names `DesignConversionPersonal` and `DesignConversionGlobal` remain the exact types of the DCM architecture. This does not permit returning to the old `design-conversion-*` artifact identities.

---

## 2. Architecture Contract

Exact architecture: `ILearningMachinePair<DesignConversionPersonal, DesignConversionGlobal>`.

`ILearningMachinePair<DesignConversionPersonal, DesignConversionGlobal>` contains two legs:

| Leg | Purpose | Ownership |
|---|---|---|
| `Personal leg` / `DesignConversionPersonal` | Adaptation to the user, their projects, private design patterns, and edit style | User area; does not become shared knowledge without explicit permission |
| `Global leg` / `DesignConversionGlobal` | The shared DCM quality model, trained on permitted shared data and anonymized signals | Server-side shared area |

Each leg must pass through the same runtime chain:

```text
ILearningMachinePair<DesignConversionPersonal, DesignConversionGlobal>
  -> leg.GetOrchestratorAsync()
  -> ILogicalOrchestrator
  -> ILogicalFlow
  -> CriticUnit / ImproverUnit
```

Mandatory shorthand for review packets: `GetOrchestratorAsync -> ILogicalOrchestrator -> ILogicalFlow -> CriticUnit/ImproverUnit`.

Units live inside the Flow; they are not independent owners of training or DPO authority.

| Unit | Contract | Input | Output |
|---|---|---|---|
| `CriticUnit` | `(tree, html, renderImages, diff) -> QualityVerdict(score+defects)` | Normalized design tree, current HTML+CSS, render images at agreed resolutions, visual diff | `QualityVerdict`: score + defects with type/location/evidence |
| `ImproverUnit` | `(tree, html, defects) -> ImprovedHtmlCss` | Normalized design tree, current HTML+CSS, defects from `QualityVerdict` | `ImprovedHtmlCss`: full HTML+CSS result, not a patch |

`Optional SourceEncoder` allowed, but not required now. If it appears, its role is limited to encoding `source_design_snapshot(figma/canva)` for the Flow; it does not change authority over `chosen/rejected`, does not create DPO pairs on its own, and does not replace `CriticUnit` or `ImproverUnit`.

Allowed trainable levels for describing DCM: `ILearningMachinePair`, `ILearningMachine`, `ILogicalOrchestrator`, `ILogicalFlow`, `ILogicalUnit`.
Training, ownership, or promotion status must not be introduced on an arbitrary helper, deterministic lookup, regex/BM25 layer, static validator, or ad-hoc service.
A deterministic shortcut may be bootstrap/scaffold evidence, but must not masquerade as a trained trainable unit. `Train*` calls,
checkpoint lifecycle, and promotion remain Machine-owned.

---

## 3. Model Role

The model holds both roles: `CriticUnit` and `ImproverUnit`.

`CriticUnit` evaluates the result: it compares source design, HTML+CSS, render images, and diff, then returns `QualityVerdict(score+defects)`.

`ImproverUnit` fixes the result: it takes source design, current HTML+CSS, and defects, then returns `ImprovedHtmlCss`.

Flow owns ordering. `ILogicalFlow` decides the call order, the number of iterations, and when the improvement loop stops. Units fulfill their contracts, but do not choose sequencing, do not change tier policy, and do not assign acceptance status to a DPO record.

---

## 4. DPO Vector For DCM

DPO vector = internal unit state + source figma/canva + result.

A DCM DPO record must be DCM-native, not a translation of `TranslationTrainReadyPair`. Minimal DPO vector:

| Field | Meaning |
|---|---|
| `id` | `dcm-{unit}-{pair_family}-{run_id}` |
| `unit` | `CriticUnit` or `ImproverUnit` |
| `unit_internal_state_snapshot` | Snapshot of the specific unit/leg internal state before the candidate result; without private leakage fields |
| `source_design_snapshot(figma/canva)` | Normalized source design snapshot: tree, tokens, layout constraints, permitted image/diff references |
| `candidate_result` | The unit output under review: `QualityVerdict` for Critic or `ImprovedHtmlCss` for Improver |
| `teacher_labels` | Render-QA labels + external AI labels where used: defect evidence, preference reason, quality class |
| `chosen` | The candidate/result that teacher labels preferred for training |
| `rejected` | A high-quality rejected candidate/result that loses to chosen by teacher labels |
| `teaching_point` | One concrete sentence explaining why chosen is preferable to rejected |
| `pair_family` | Difference class: `spacing`, `color`, `typography`, `overflow`, `missing_element`, `z_order`, `responsive`, etc. |
| `curriculum_tier` | Learning difficulty of the defect 1-5; independent of the user subscription tier |
| `validation_outcome` | `accepted`, `quarantine`, `rejected` |

Example shape:

```json
{
  "id": "dcm-critic-spacing-run0042",
  "unit": "CriticUnit",
  "unit_internal_state_snapshot": "{...}",
  "source_design_snapshot(figma/canva)": "{tree,tokens,breakpoints,diff_refs}",
  "candidate_result": "{\"score\":0.82,\"defects\":[...]}",
  "teacher_labels": {
    "render_qa": {"chosen": "candidate_a", "rejected": "candidate_b"},
    "external_ai": {"chosen": "candidate_a", "rejected": "candidate_b"},
    "reason": "candidate_a names the real spacing defect and candidate_b misses it"
  },
  "chosen": "{...candidate_a...}",
  "rejected": "{...candidate_b...}",
  "teaching_point": "Rejected misses the actual spacing defect while chosen localizes it with render evidence.",
  "pair_family": "spacing",
  "curriculum_tier": 2,
  "validation_outcome": "accepted"
}
```

---

## 5. Chosen/Rejected Authority Path

Server-side ILearningMachine sets `chosen/rejected` for DPO. More explicitly: server-side ILearningMachine pipeline owns chosen/rejected construction.

Authority path:

```text
source design + generated candidates
  -> render-QA
  -> optional external AI teacher
  -> teacher labels chosen/rejected
  -> local DB stores labelled evidence
  -> local DB builds per-unit DPO for CriticUnit/ImproverUnit
  -> Train* on ILearningMachine only
```

Teacher = external AI + render-QA labels chosen/rejected and fills DB. The external AI is a teacher/labeler, not a runtime foundation and not a production element of the user's conversion loop. Render-QA provides visual evidence; the external AI teacher adds structured labels for hard cases, ambiguity, and near-miss pairs.

Local DB builds per-unit DPO:

| Unit | Local DB builds |
|---|---|
| `CriticUnit` | `QualityVerdict` vs `QualityVerdict` pairs, where quality means correctness of labels/defects/score relative to render-QA + teacher evidence |
| `ImproverUnit` | `ImprovedHtmlCss` vs `ImprovedHtmlCss` pairs, where quality means improvement of the render result relative to source design and defects |

`Train*` only on Machine. `TrainCriticUnit`, `TrainImproverUnit`, or local training on a unit as owner must not be introduced. Permitted training forms are invoked only on `ILearningMachine` / `ILearningMachinePair` through the server-side pipeline.

---

## 6. Free/Paid/Pro Tier Policy

`free/paid/pro` = budget layer around improvement loop only; it must not change core DPO acceptance, chosen/rejected semantics, model quality thresholds, or `CurriculumTier`.

A subscription tier may limit:

| Tier | Budget / iteration policy |
|---|---|
| `free` | Fewer improvement iterations, fewer or zero paid external AI teacher calls, more reliance on deterministic render-QA labels |
| `paid` | More improvement iterations, a limited external AI teacher budget |
| `pro` | Maximum improvement-loop budget and an expanded teacher review quota |

A subscription tier may not change:

- the acceptance rule for a DPO record;
- the value of `chosen` or `rejected`;
- core model quality threshold;
- `CurriculumTier`;
- the meaning of `QualityVerdict.score`;
- the eligibility of a pair for train/held-out.

If the same source/candidate receives different subscription budgets, the DPO semantics stay the same. Only the number of improvement attempts and the cost of teacher labeling differ.

---

## 7. Negative-Pair Quality Rules

Negative pairs must be high-quality. `rejected` must not be a garbage, trivially broken, or irrelevant candidate. The best DPO signals come from high-quality near-miss pairs, where rejected looks plausible but teacher evidence shows a concrete way it loses to chosen.

Batch `50+50`: each training batch must have 50 train pairs + 50 held-out pairs, disjoint by source design.

General rules:

- `chosen` and `rejected` are built only from candidates/results with sufficient evidence.
- A high-quality near-miss rejected is valid even when both results are above the product threshold, if teacher labels show a stable preference.
- Do not accept a pair where chosen/rejected differ only by order/index/name leakage.
- Do not accept a pair where the external teacher and render-QA give no explainable preference reason.
- For `ImproverUnit`, chosen must better eliminate defects or better preserve source design constraints.
- For `CriticUnit`, quality is correctness of labels/defects/score, not necessarily a higher self-reported score.

CriticUnit clarification:

```text
CriticUnit quality is label/defect correctness, not necessarily higher self-reported score.
```

Example of a valid near-miss CriticUnit pair:

| Candidate | Self-reported score | Defects | Teacher judgement |
|---|---:|---|---|
| chosen | 0.72 | correctly names real color defect with location | Accepted because labels match render diff |
| rejected | 0.91 | misses real color defect and reports clean layout | Rejected despite higher self-reported score |

Product-quality threshold lines do not forbid such a pair: the score inside `QualityVerdict` is part of the candidate output, while the DPO preference is set by server-side teacher evidence.

---

## 8. Taxonomy Of DCM Pair Families

| Pair family | Rejected near-miss example | Why chosen is better |
|---|---|---|
| `spacing` | Button shifted by 8px or card padding 12px instead of 16px | Chosen localizes the spacing defect and preserves expected layout tokens |
| `color` | `#0056D2` instead of `#0052CC` with plausible component structure | Chosen detects or fixes the exact token mismatch |
| `typography` | Correct text but wrong font weight/line-height | Chosen follows typography tokens and visual render evidence |
| `overflow` | Works at desktop but clips at 360px | Chosen handles all required breakpoints |
| `missing_element` | Omits icon/footer while rest of layout is strong | Chosen keeps all required design tree elements |
| `z_order` | Dropdown mostly correct but hidden behind content | Chosen restores layer hierarchy |
| `responsive` | Correct at 1280px but not at 360px/768px | Chosen preserves source constraints across breakpoints |

---

## 9. Concrete DPO Examples

### 9.1 CriticUnit Pair

Scenario: source card has `padding:16px`; rendered HTML has `padding:12px`.

```json
{
  "id": "dcm-critic-spacing-run0001",
  "unit": "CriticUnit",
  "unit_internal_state_snapshot": "{critic_leg:'global', flow_step:'critic_pass_1'}",
  "source_design_snapshot(figma/canva)": "{card:{padding:'16px', width:'320px'}, breakpoints:[360,768,1280,1920]}",
  "candidate_result": "{\"score\":0.91,\"defects\":[{\"type\":\"spacing\",\"element\":\"card\",\"expected\":\"padding:16px\",\"actual\":\"padding:12px\"}]}",
  "teacher_labels": {
    "render_qa": {"chosen": "verdict_a", "rejected": "verdict_b"},
    "reason": "verdict_a identifies the real padding mismatch"
  },
  "chosen": "{\"score\":0.91,\"defects\":[{\"type\":\"spacing\",\"element\":\"card\",\"expected\":\"padding:16px\",\"actual\":\"padding:12px\"}]}",
  "rejected": "{\"score\":0.96,\"defects\":[]}",
  "teaching_point": "Rejected reports a cleaner result but misses the real spacing defect found by render-QA.",
  "pair_family": "spacing",
  "curriculum_tier": 2,
  "validation_outcome": "accepted"
}
```

Why valid: rejected is plausible and high-confidence, but wrong. The DPO signal teaches CriticUnit to prefer correct defect evidence over self-reported score inflation.

### 9.2 ImproverUnit Pair

Scenario: CriticUnit found `overflow` at 360px.

```json
{
  "id": "dcm-improver-overflow-run0003",
  "unit": "ImproverUnit",
  "unit_internal_state_snapshot": "{personal_leg:'user-abstracted', flow_step:'improve_pass_2'}",
  "source_design_snapshot(figma/canva)": "{card:{width:'320px', padding:'16px'}, breakpoints:[360,768,1280,1920]}",
  "candidate_result": "<div style=\"width:100%; max-width:320px; padding:16px; box-sizing:border-box;\"><h2 style=\"word-break:break-word\">...</h2></div>",
  "teacher_labels": {
    "render_qa": {"chosen": "html_a", "rejected": "html_b"},
    "reason": "html_a removes overflow at 360px while preserving source width constraints"
  },
  "chosen": "<div style=\"width:100%; max-width:320px; padding:16px; box-sizing:border-box;\"><h2 style=\"word-break:break-word\">...</h2></div>",
  "rejected": "<div style=\"width:320px; padding:16px;\"><h2>...</h2></div>",
  "teaching_point": "Rejected keeps a fixed-width layout that still overflows on mobile, while chosen preserves the design width as max-width.",
  "pair_family": "overflow",
  "curriculum_tier": 2,
  "validation_outcome": "accepted"
}
```

---

## 10. Anti-Leakage And Quarantine

Forbidden training features:

- user names, project identifiers, session identifiers;
- local render file paths or candidate filenames;
- candidate order/index;
- exact user prompt;
- raw private `.fig` / `.canva` filenames;
- subscription tier as a model feature;
- Translation-domain ids or legacy `design-conversion-*` ids as DCM foundations.

Allowed features:

- normalized source design tree;
- normalized tokens and constraints;
- render images/diff references without local path leakage;
- breakpoints;
- abstracted defects;
- unit internal state snapshot with private identifiers removed.

Quarantine required when:

- `chosen` / `rejected` preference changes under candidate permutation;
- teacher evidence is missing or contradictory;
- pair uses low-quality rejected that gives no useful preference signal;
- source design leaks private identifiers;
- source license does not allow the intended leg (`Personal leg` vs `Global leg`);
- subscription tier changes DPO semantics;
- old Translation or `design-conversion-*` artifact identity is used as foundation.

Quarantine pairs stay in DB for audit, but never enter `Train*`.

### Source And License Gates

Before `accepted`, every DPO pair must pass the source/license gate:

- `source_origin` is known: a user's private source, a permitted open-source sample, a synthetic sample, or a licensed dataset.
- For the `Personal leg`, a private source is allowed only in the user area and does not become Global knowledge without explicit consent.
- For the `Global leg`, the source must have a compatible license or permission to train on and distribute derived signals; questionable sources go to `quarantine`.
- External AI teacher labels are allowed only if the source/provider terms permit using the labels as training evidence.
- Raw private `.fig` / `.canva` files, local paths, project URLs, and exact prompts do not enter trainable features.
- Missing license evidence blocks `accepted`, even if Render-QA and teacher preference look strong.

---

## 11. Training And Measurement

After local DB builds per-unit DPO and validates a `50+50` batch, training may be invoked only through Machine-owned APIs:

```text
ILearningMachinePair<DesignConversionPersonal, DesignConversionGlobal>
  -> Train* on Machine
  -> held-out measurement
  -> SaveCheckpointAsync + backup
  -> fresh-load verification
  -> ExportAsync metadata / ImportAsync metadata-only sync
  -> continue-training check
  -> locator update
```

Expected measurement:

- held-out preference accuracy;
- reward/preference margin before vs after training;
- defect-family coverage;
- regression checks by source design;
- separate Personal leg and Global leg reporting.

Do not make a semantic/product claim like "trained", "improves layout", "ready for a tier", or "replaces Bootstrap" until QA/eval evidence exists:
numeric held-out metrics, ablation without checkpoint, leakage/permutation checks, fresh-load reproduction, and a continue-training result.
If the evidence is incomplete, the status stays `OPEN`, `PASS_AS_HONEST_UNTRAINED_SCAFFOLD`, or `PROMOTION_BLOCKED`, but not `PASS_AS_TRAINED_FEATURE`.

This guide does not authorize implementation. It is a plan-only content artifact describing the DCM DPO construction contract.

## Pass40/41 Real-S3 Approval-Validation Boundary

Before a real-S3 DPO construction worker creates or consumes `source-manifest.json`,
`dpo-rows.jsonl`, `heldout-measurements.json`, or `checkpoint-manifest.json`, it
must run the local `approval-validation` route classifier over the supplied
approval packet.

Route meanings:

| route | meaning |
|---|---|
| `approval_packet_missing` | no approval packet was supplied; real-S3 source/DPO/checkpoint construction remains blocked |
| `template_not_approval` | the packet is a template, scaffold, placeholder, or checklist; it is not approval |
| `approval_validation_failed` | required approval fields, rights, scope, or provenance authority failed validation |
| `approval_valid_but_not_product_ready` | approval is structurally valid only for resuming real-data artifact construction |

Even after `approval_valid_but_not_product_ready`, `real_data_still_required`
remains true until the actual `source-manifest.json`, `dpo-rows.jsonl`,
`heldout-measurements.json`, and `checkpoint-manifest.json` exist and pass their
own gates. `schema/template/scaffold/focused tests do not prove` source quality,
DPO readiness, held-out quality, checkpoint promotion, upload authority, or
product readiness.

---

## 12. DPO-Pair Usefulness Metric For The User (R-09)

Each accepted DPO pair gets a `usefulness_score` to show the user in the "Training Quality" panel (requirement R-09). The metric tells the user how useful the pair is for training the model.

Formula:

```
usefulness_score = preference_margin × difficulty_weight × novelty_weight   (range 0..~2)
```

- `preference_margin` (0..1) — the strength of preference for chosen over rejected by teacher labels:
  - for `ImproverUnit` = the difference in render visual accuracy between chosen and rejected;
  - for `CriticUnit` = the difference in critique correctness (F1 over defects): `F1(chosen) − F1(rejected)`.
- `difficulty_weight` — by `curriculum_tier`: tier1 = 0.5, tier3 = 1.0, tier5 = 1.5.
- `novelty_weight` — a rare `pair_family` (< 5 records in storage) = 1.3, otherwise 1.0.

Example (pair `dcm-improver-overflow-run0003`): preference_margin = 0.27, tier 2 → difficulty = 1.0, novelty = 1.3 ⇒ `usefulness_score = 0.27 × 1.0 × 1.3 ≈ 0.35`.

Shown to the user as a human sentence, for example: "The pair raised preference by 27%, rare defect class (overflow). Usefulness: 0.35".

Do not show the user (see §10): internal pair ids, file and render paths, candidate names, candidate index, exact-prompt. The metric only informs the user — it does NOT affect acceptance, does NOT affect `chosen`/`rejected`, and is NOT a training feature.
