# HTML CSS Render QA Automation Guide

## Luba Why / Loophole Repair-First Rule

When Luba asks "why", "почему", challenges an answer, or says a failure came
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

Этот guide задает server-side render QA (Quality Assurance) contract для
будущего DesignConversionDomain. Render QA проверяет, что HTML (HyperText
Markup Language) / CSS (Cascading Style Sheets) candidate можно воспроизвести
в controlled renderer и сравнить с design source. Это evidence layer, не
обучаемая final authority.

## When to use / Когда использовать

Используй этот guide, когда план или work order упоминает HTML/CSS rendering,
screenshot capture, visual diff, responsive QA, artifact contract, server-side
rendering, browser automation или DPO (Direct Preference Optimization) rows,
которые зависят от скриншотов результата.

## Inputs / Входы

- Candidate HTML/CSS bundle with declared entrypoint and asset references.
- Normalized design source packet with original screenshot refs.
- Viewport matrix: width, height, device pixel ratio, orientation, theme/state,
  and interaction/state variant when applicable.
- Asset bundle: images, fonts, icons, masks, videos, and fallback markers.
- Render environment: engine name/version, operating system, font policy,
  network policy, timeout policy, and deterministic seed if used.
- Expected artifact destination and retention policy.

## Outputs/artifacts / Выходы и артефакты

Render QA must emit a multi-resolution screenshot artifact contract:

- `render_run_id`: stable run identifier.
- `candidate_ref`: HTML/CSS candidate hash and artifact path.
- `source_refs`: source tree and original screenshot hashes.
- `viewport_results`: one row per viewport/state.
- `rendered_screenshot_ref`: PNG (Portable Network Graphics) or approved image
  artifact reference for the rendered candidate.
- `dom_snapshot_ref`: DOM (Document Object Model) snapshot if captured.
- `computed_style_ref`: computed style artifact when typography/color/spacing
  evaluation needs it.
- `asset_resolution_report`: missing, substituted, loaded, blocked, or
  external asset markers.
- `render_timing_ms`: wall-clock render duration per viewport.
- `stability_result`: pass/fail after repeat render or pixel stability check.
- `qa_status`: `passed_for_eval`, `failed_render`, `failed_assets`,
  `unstable`, `needs_review`, or `quarantine`.

## Required gates / Обязательные gates

- Multi-resolution gate: at minimum the future plan must define desktop,
  tablet, and mobile viewports; production gates may add theme/state variants.
- Server-side gate: screenshots used for training/eval must come from a
  controlled server-side render path, not an ad hoc local browser capture.
- Artifact completeness gate: no visual diff or DPO row without candidate hash,
  source hash, viewport metadata, rendered screenshot, and asset report.
- Stability gate: repeated render of the same candidate/source/viewport must be
  stable within the configured tolerance before the screenshot becomes eval
  evidence.
- Timeout/cost gate: render time is measured and can stop the visual loop when
  tier budget policy says stop.
- Isolation gate: external network calls during render are blocked or recorded
  explicitly; missing assets are evidence, not silently ignored.
- Internal authority gate: render QA can block promotion or create correction
  evidence, but it cannot prove trained model quality by itself.

## Forbidden substitutes / Запрещенные подмены

- A developer's manual browser screenshot is not a server-side QA artifact.
- A single desktop screenshot is not responsive evidence.
- A successful render is not proof that visual quality is good.
- A visual AI (Artificial Intelligence) provider verdict is not a replacement
  for the local render artifact contract and requires explicit approval.
- Ignoring missing fonts/assets is forbidden; missing assets must be reported.
- Render retry loops cannot hide unstable HTML/CSS output.

## How it feeds the future plan / Как это питает будущий план

Детали 8a должны add a render QA work order before visual metrics, DPO authoring,
training, and promotion. The plan must define the viewport matrix, screenshot
artifact schema, asset contract, render timing metric, stability tolerance,
quarantine statuses, and policy link to tier budget loop limits. This guide
keeps render automation as evidence generation, not as hidden deterministic
model authority.
