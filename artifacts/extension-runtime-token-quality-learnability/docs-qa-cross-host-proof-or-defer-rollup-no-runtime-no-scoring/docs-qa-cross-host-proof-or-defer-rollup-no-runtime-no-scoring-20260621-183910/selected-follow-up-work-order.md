# Selected follow-up work order

Selected work order:

`DOCS-QA-FULL-HOST-UI-CHAT-PROOF-WORK-ORDER-BUNDLE-PREP-NO-RUNTIME-NO-SCORING-05`

## Purpose

Prepare a safe bounded work-order bundle for future full UI/chat proof packets across Claude, Cursor and Codex.

This follow-up is docs-QA/support-only. It must not launch Claude, Cursor or Codex runtime, must not run scoring/eval/proof runners, must not train or mutate checkpoints/models/locators/targets, and must not read raw-private request/response/trace contents.

## Why this is next

The most actionable non-PASS gap after the cross-host rollup is full host UI/chat proof:

- Claude has support-only hook/local-entrypoint/core-boundary evidence; full Desktop/chat UI proof is deferred.
- Cursor has support-only Extension Host command/panel/helper and handler evidence; clean full chat participant/panel UI proof is deferred.
- Codex has support-only installed-cache/local adapter and core-boundary evidence; full desktop/app/chat visible conversation proof is deferred.

Runtime, scoring and training are not authorized by the current rollup packet. Therefore the safe independent next step is to prepare future bounded work orders with exact authorization gates, redaction boundaries, evidence requirements, no-overclaim checks and parent commit/push ownership.

## Required output shape for the follow-up

- Human-readable Russian work-order bundle.
- Per-host future proof requirements for Claude, Cursor and Codex.
- Explicit runtime authorization gate for each future proof packet.
- Explicit raw-private redaction boundary.
- Explicit claim-lock scan.
- No token savings, quality improvement, semantic quality, learnability, product readiness or final readiness claim.
- `runtime_launch_authorized=false`.
- `scoring_authorized=false`.
- `training_authorized=false`.
- `raw_private_reads_authorized=false`.
- `git_authority=forbidden`.
- `parent_will_perform_commit_push_after_review=true`.
