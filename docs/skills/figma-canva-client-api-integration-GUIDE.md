# Figma Canva Client API Integration Guide

## Luba Why / Loophole Repair-First Rule

When Luba asks "why" (in any language), challenges an answer, or says a failure came
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

This guide fixes the boundary for the FigmaToCode and CanvaToCode clients. The
clients are thin: they send a payload, receive result/artifacts, show status, and
contain no training/core model logic.

## When to use

Use this guide when a plan mentions Figma plugin, Canva app, FigmaToCode,
CanvaToCode, client API (Application Programming Interface), design source
upload, result download, artifact viewer, settings/debug surface, or extension
boundary for DesignConversionDomain.

## Inputs

- Client source payload: Figma export, Canva export, screenshot, user-selected
  frame/page, and permitted metadata.
- User settings: tier, quality target, allowed visual loops, privacy scope,
  external provider approval state, and upload/retrain approval.
- Core API endpoint for `llm_mvp_core` DesignConversion request.
- Authentication/session context approved by the host integration.

## Outputs/artifacts

Thin client request should contain:

- `request_id`
- `source_kind`: `figma` or `canva`
- `payload_ref_or_inline_payload`
- `selected_frame_or_page`
- `viewport_state_request`
- `tier_policy_id`
- `privacy_scope`
- `user_approval_refs`
- `callback_or_artifact_delivery_hint`

Core response should contain:

- `request_id`
- `status`: accepted, running, completed, needs_review, failed, or quarantine.
- `html_css_result_ref`
- `artifact_refs`: source tree, screenshots, rendered screenshots, visual
  verdict, correction/DPO candidate when available.
- `quality_summary`
- `cost_time_summary`
- `next_action`: download, review, approve retrain, retry, or stop.
- `debug_ref`: scoped debug artifact, not raw private corpus leakage.

## Required gates

- Thin-client gate: FigmaToCode and CanvaToCode send payload and receive
  result/artifacts only.
- Exact review phrase: thin clients send payload and receive result/artifacts.
- Exact boundary phrase: FigmaToCode and CanvaToCode are thin client surfaces
  with no training and no core model logic in clients.
- Core-logic gate: source normalization, orchestration, DPO (Direct Preference
  Optimization) authoring, training, evaluation, lifecycle, and promotion stay
  in `llm_mvp_core`.
- Payload contract gate: request/response schema must include source kind,
  payload ref/hash, viewport/state request, tier policy, privacy scope, user
  approvals, and artifact refs.
- Privacy gate: client must not upload private data for common/global training
  unless user approval and redaction policy allow it.
- Debug gate: debug surface may expose artifact refs and human-readable reasons,
  but not hidden model internals, private corpus rows, or raw unapproved data.
- Failure/status gate: client displays quarantine/needs_review/failed states
  honestly instead of pretending the conversion succeeded.

## Forbidden substitutes

- Client-side training logic is forbidden.
- Client-side semantic ranking, visual scoring, DPO row construction, or model
  checkpoint mutation is forbidden.
- A Figma/Canva plugin cannot be the canonical source of the common corpus.
- A client retry loop cannot bypass tier policy or cost stop condition.
- A client artifact viewer cannot claim model quality without core-held
  held-out eval and lifecycle proof.
- Raw project paths, user ids, account ids, or private payloads cannot be
  leaked in debug artifacts.

## How it feeds the future plan

The 8a details must define exact Figma/Canva payload and response contracts, plus
the thin-client boundary. The plan must ensure FigmaToCode and CanvaToCode only
call `llm_mvp_core`, receive artifacts, and present status. All training,
visual QA, DPO, lifecycle, promotion, tier policy, and core model decisions stay
inside the DesignConversionDomain/core side.
