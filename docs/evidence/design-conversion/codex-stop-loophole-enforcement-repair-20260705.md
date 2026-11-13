# Codex stop loophole enforcement repair - 20260705

## Claim boundary

claim_scope: governance/skills/guides/agent-rule enforcement repair only.
evidence_scope: direct edits to allowed governance/docs files in `C:\Projects\llm_mvp_core`, marker readback, conflict scan, mojibake scan for new marker sections, and `git diff --check` over changed files.
count_claimed: 0 product passes; 0 product completion; 0 claimed 50-pass completion.
independent_evidence_count: 1 governance repair packet from this worker plus command readbacks listed below.
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT.
downgrade_needed: no product readiness claim is allowed from this repair.

## Loophole classification

loophole_classification: `premature_final_after_governance_repair`.

The old rule text said governance repair was not product continuation evidence, but status/final acceptance could still stop after the repair because the acceptance path did not require a pre-final continuation check proving either:

- the next bounded product/review worker was started, waited for, or reviewed in the same continuation chain; or
- a true external blocker certificate existed with exact proof.

`rule_text_present_but_not_enforced_is_failure`: a rule's presence is not enough unless final/status acceptance is blocked when the next bounded action exists.

## Repaired enforcement

The patch makes these markers enforceable across the active governing surfaces:

- `pre_final_continuation_enforcement_required`
- `rule_text_present_but_not_enforced_is_failure`
- `governance_repair_acceptance_requires_next_worker_started_or_true_blocker`
- `NO_PATCH_requires_product_continuation_check`
- `final_packet_forbidden_when_next_bounded_action_exists`
- `parent_must_run_pre_final_check_before_status_final`
- `subagent_wait_heartbeat_required_before_wait`
- `one_active_agent_does_not_mean_wait_silently`
- `after_governance_repair_spawn_or_wait_next_product_review_worker`

The required pre-final check fields are:

```text
active_task_open:
next_bounded_product_review_or_governance_action_exists:
active_or_returned_subagent_packet_pending_verification:
required_passes_remaining:
next_subagent_started_or_waited_this_turn:
true_external_blocker_certificate:
```

Acceptance rule: if active task is open and any next bounded product/review/governance action exists, any returned packet is pending verification, or fewer requested passes exist than required, final/status/completion/hold/no-safe-work is forbidden unless exact true external blocker proof exists.

## Shrink-verified files changed

- `AGENTS.md`
- `docs/skills/plan-review-SKILL.md`
- `docs/skills/goal-delivery-completeness-SKILL.md`
- `docs/skills/claim-verification-SKILL.md`
- `docs/RUSSIAN-STATUS-AND-EXECUTION-LOG-TEMPLATE.md`
- `docs/evidence/design-conversion/codex-stop-loophole-enforcement-repair-20260705.md`

Earlier interrupted broad edits touched other governance files that were inside the original work order's allowed list. This shrink recovery packet does not rely on or claim those broader edits; it reverified only the six files above.

## Mandatory next continuation slice

next_product_or_review_slice: `final review pass 4/50`.

This governance repair is not a completion boundary. The parent must continue to `final review pass 4/50` by starting, waiting for, or reviewing that next bounded product/review worker, or record a true external blocker with exact proof. `NO_PATCH`, PASS, NEEDS_FIX, timeout, usage limit, partial packet, worker silence, and stale status are not stop permissions.

## Verification commands

- marker readback: `rg -n "pre_final_continuation_enforcement_required|rule_text_present_but_not_enforced_is_failure|governance_repair_acceptance_requires_next_worker_started_or_true_blocker|NO_PATCH_requires_product_continuation_check|final_packet_forbidden_when_next_bounded_action_exists|parent_must_run_pre_final_check_before_status_final|subagent_wait_heartbeat_required_before_wait|one_active_agent_does_not_mean_wait_silently|after_governance_repair_spawn_or_wait_next_product_review_worker" AGENTS.md docs\skills\claim-verification-SKILL.md docs\skills\goal-delivery-completeness-SKILL.md docs\skills\plan-review-SKILL.md docs\RUSSIAN-STATUS-AND-EXECUTION-LOG-TEMPLATE.md docs\evidence\design-conversion\codex-stop-loophole-enforcement-repair-20260705.md` returned all required literals in the shrink-allowed changed files.
- changed line readback: marker readback returned the changed sections at `AGENTS.md:327-335`, `docs/skills/claim-verification-SKILL.md:48-56`, `docs/skills/goal-delivery-completeness-SKILL.md:80-88`, `docs/skills/plan-review-SKILL.md:141-149`, `docs/RUSSIAN-STATUS-AND-EXECUTION-LOG-TEMPLATE.md:31-39`, and this evidence file.
- `git diff --check -- AGENTS.md docs/skills/claim-verification-SKILL.md docs/skills/goal-delivery-completeness-SKILL.md docs/skills/plan-review-SKILL.md docs/RUSSIAN-STATUS-AND-EXECUTION-LOG-TEMPLATE.md docs/evidence/design-conversion/codex-stop-loophole-enforcement-repair-20260705.md`: `git_diff_check_exit=0`.
- conflict marker scan: `rg -n "^(<<<<<<<|=======|>>>>>>>)" ...` returned no matches; `conflict_scan_exit=1` from `rg` means no conflict marker match.
- new marker-section mojibake scan: added-line scan for `Ð|Ñ|Â|â€™|â€œ|â€�|�` returned `added_line_mojibake_scan=0 matches`.
