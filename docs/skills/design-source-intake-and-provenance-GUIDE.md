# Design Source Intake And Provenance Guide

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

This guide describes how to intake a design source for the future
`DesignConversionDomain` so that DPO (Direct Preference Optimization),
render QA (Quality Assurance), lifecycle, and privacy gates can verify
every training row.

## When to use

Use this guide for plans and work orders where a design source comes from
Figma, Canva, HTML/CSS snapshots, image reference, manual edit, synthetic
fixture, or user-approved correction. It is mandatory before any claim that
a row may enter the training corpus, held-out set, DPO batch, checkpoint
promotion, or user-adaptive state.

## Inputs

- Source payload from Figma, Canva, local file, screenshot capture, manual
  correction, or approved fixture.
- Original design tree: frames, layers, components, constraints, typography,
  colors, effects, assets, states, variants, and breakpoints when available.
- Original screenshots: viewport size, device pixel ratio, theme/state, capture
  time, source tool version, and crop bounds.
- Candidate HTML/CSS output and rendered screenshots when candidate evaluation
  exists.
- Human or automated visual verdict, correction, chosen/rejected/quarantine
  decision, and reviewer/provenance references.
- Hash fields: payload hash, normalized tree hash, screenshot hash, candidate
  hash, rendered screenshot hash, verdict hash, correction hash, and row hash.

## Outputs / artifacts

Every accepted intake packet must produce a typed provenance bundle:

- `source_kind`: `figma`, `canva`, `html_css_snapshot`, `image_reference`,
  `manual_correction`, `synthetic_fixture`, or `other_approved`.
- `source_license_or_user_approval`: explicit license, user approval, or
  private/adaptive-only marker.
- `source_disjoint_group`: stable group key used to keep training and held-out
  examples source-disjoint.
- `original_tree_ref`: normalized design tree artifact reference.
- `original_screenshot_refs`: one or more screenshot artifact references.
- `candidate_html_css_ref`: candidate output reference when available.
- `rendered_screenshot_refs`: server-rendered candidate screenshots.
- `visual_verdict_ref`: taxonomy-based verdict artifact.
- `correction_ref`: human or approved correction artifact.
- `dpo_decision`: `chosen`, `rejected`, `quarantine`, or `needs_review`.
- `privacy_scope`: `common_allowed`, `personal_adaptive_only`,
  `redacted_common_candidate`, or `forbidden`.
- `hashes`: payload, tree, screenshot, candidate, render, verdict, correction,
  and row hashes.

## Required gates

- License/approval gate: common training rows require approved source/license;
  private user designs default to personal/adaptive only.
- Provenance completeness gate: no DPO row without source kind, source group,
  original tree, original screenshots, candidate, rendered screenshots, verdict,
  correction, decision, and hashes.
- Source-disjoint gate: held-out evaluation must not share source document,
  project, template, export id, file path, or screenshot family with training
  examples.
- Privacy gate: raw user identifiers, project paths, account identifiers, and
  proprietary payloads must not enter common/global assets unless explicitly
  redacted and approved.
- Reproducibility gate: every screenshot/render artifact must name viewport,
  device pixel ratio, theme/state, browser/render engine, timestamp, and
  normalization version.
- Quarantine gate: missing asset, ambiguous source, bad screenshot, unstable
  render, incomplete tree, or unclear correction sends the row to quarantine,
  not to training.

## Forbidden substitutes

- File names, folder paths, source ids, project ids, or export ids cannot be
  model features or DPO decision shortcuts.
- A screenshot without normalized source tree is not enough for common training.
- A source tree without screenshot evidence is not enough for visual quality
  evaluation.
- A human note without artifact hashes is not a provenance record.
- A synthetic fixture cannot be mixed into approved real-source evaluation
  without a separate synthetic scope marker.

## How it feeds the future plan

Details 8a must include an intake/provenance work order before DPO authoring,
training, or promotion. The plan must define source artifact schema, privacy
scope, source-disjoint split, hash computation, quarantine handling, and how
Figma/Canva payloads become normalized design trees. This guide is the source
for all plan gates that decide whether a design example is trainable, held-out,
personal-only, or forbidden.
