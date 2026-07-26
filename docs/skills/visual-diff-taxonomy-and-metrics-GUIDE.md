# Visual Diff Taxonomy And Metrics Guide

## Luba Why / Loophole Repair-First Rule

When Luba asks "why" (or the same in the operator's language, e.g. "почему"), challenges an answer, or says a failure came
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

This guide defines the visual taxonomy and metrics for DesignConversionDomain:
how to describe the difference between the source design and the rendered
HTML/CSS candidate, which numbers to collect, and where the boundary runs
between QA/eval evidence and trainable model quality.

## When to use

Use this guide for any plan/review/work order that involves a visual diff,
visual quality, screenshot comparison, design-to-code accuracy, held-out eval,
ablation, or DPO (Direct Preference Optimization) correction for HTML/CSS
conversion.

## Inputs

- Original screenshot refs and normalized design tree.
- Rendered candidate screenshot refs from server-side render QA.
- DOM (Document Object Model) snapshot and computed style refs when available.
- Viewport/state metadata.
- Asset resolution report.
- Human visual verdict and correction when available.
- Optional external visual AI (Artificial Intelligence) provider evidence only
  after explicit Luba approval.

## Outputs/artifacts

Visual evaluation must produce a taxonomy-based verdict:

- `overall_visual_quality_score`: configured numeric score with threshold.
- `taxonomy_findings`: list of findings using the categories below.
- `dominant_error_category`: one primary reason for rejected/negative DPO rows.
- `severity`: `minor`, `moderate`, `major`, `blocking`.
- `viewport_coverage`: which viewport/state rows were checked.
- `metric_breakdown`: local numeric metrics and human-readable notes.
- `external_evidence_ref`: optional, approval-bound external provider output.
- `verdict`: `accept_candidate`, `reject_candidate`, `quarantine`,
  `needs_human_review`, or `insufficient_evidence`.

## Required gates

- Taxonomy coverage gate: every visual verdict must classify findings using:
  layout, typography, color, spacing, responsiveness, image/crop, asset missing,
  and interaction/state when applicable.
- Metric traceability gate: every score must link to source screenshot,
  rendered screenshot, viewport, candidate hash, and taxonomy row.
- Dominant-error gate: a rejected candidate used for negative DPO must have one
  dominant visual/layout error rather than a bundle of unrelated failures.
- Held-out gate: visual metrics must run on source-disjoint held-out examples
  before any trained quality claim.
- Ablation/no-hidden-determinism gate: deterministic visual metrics may support
  eval and promotion decisions, but the plan must prove the trained model is not
  secretly succeeding because of deterministic source ids, file paths, template
  ids, or fixed diff shortcuts.
- External provider approval gate: a visual AI provider is an external-evidence
  step requiring explicit Luba approval. It is not itself proof of trained
  model quality.
- Exact proof phrase for review: external visual AI evidence is not itself proof of trained model quality.

## Taxonomy

- Layout: hierarchy, grid, alignment, order, nesting, constraints, overflow,
  z-index, and component placement.
- Typography: font family, size, weight, line height, letter spacing, wrapping,
  text overflow, and text hierarchy.
- Color: fill, stroke, background, opacity, gradients, shadows, contrast, and
  theme/state colors.
- Spacing: margins, padding, gaps, rhythm, baseline spacing, and density.
- Responsiveness: breakpoint behavior, resize flow, orientation, scroll area,
  and mobile/tablet/desktop consistency.
- Image/crop: object fit, focal point, mask, aspect ratio, crop, resolution, and
  asset scaling.
- Asset missing: missing fonts, icons, images, masks, videos, blocked network
  assets, and fallback substitutions.
- Interaction/state: hover, focus, active, disabled, selected, open/closed,
  loading, error, and other state variants when applicable.

## Metrics

- Layout match rate per viewport.
- Typography match rate per text group.
- Color delta and contrast compliance per semantic token.
- Spacing deviation by component and container.
- Responsive breakpoint pass rate.
- Image/crop match rate.
- Missing asset count and severity.
- State coverage pass rate.
- Human correction acceptance rate.
- DPO chosen/rejected reward margin after training.
- Held-out visual quality before/after training.

## Forbidden substitutes

- Pixel similarity alone is not visual quality.
- A visual AI provider response is not a trained-model quality proof.
- A taxonomy label without screenshot refs and hashes is not evidence.
- A broad "looks good" note cannot become a DPO verdict.
- A high render success rate cannot replace held-out visual quality metrics.
- A deterministic diff threshold cannot be described as semantic learning.

## How it feeds the future plan

The 8a details must define the visual taxonomy and metrics before DPO generation
and training. The plan must require local metric artifacts, human-readable
findings, source-disjoint held-out eval, ablation gates, and explicit approval
for external visual AI. This guide supplies the categories and proof boundary
for measuring speed, accuracy, and quality of design conversion learning.
