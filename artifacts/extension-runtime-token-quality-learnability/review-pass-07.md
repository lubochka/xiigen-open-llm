# Review/fix pass 7 - P49/P50 approval, source/license, target/rollback boundary

Verdict: PASS_WITH_FIX.

This is one real review/fix pass, not a ledger-only row. The review row is an index only; `ledger_rows_are_evidence=false`.

## Focus

Pass 7 checked whether the plan can safely move from plan-only into host runtime, token metrics, quality eval, DPO, training, fresh-load, checkpoint mutation, target promotion and rollback work without reusing old approvals or losing P49/P50 boundaries.

## Before finding

The plan already said that target and rollback were inactive until approval, and Section 10 listed minimum approval fields. That was not enough.

The gap was:

- approval was broad instead of track-separated;
- an old quoted transcript or state approval was not explicitly blocked inside the packet schema;
- source/license rules were present in final acceptance but not bound to the approval packet;
- target and rollback did not bind dataset sha, held-out sha, base sha, manifest, locator candidate, fresh-load, continue-training, held-out, ablation and rollback probes in one place;
- a missing approval for one track could accidentally become a global stop instead of a deferred decision for that track only;
- final full-extension functionality was not explicitly tied to packet-backed runtime/eval/training evidence.

## Concrete fix

Patched `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md`:

1. Added Section `4.5.1. Approval packet, source/license, target and rollback binding`.
2. Split approval into independent tracks:
   - `host_runtime_probe`
   - `token_metrics`
   - `quality_eval`
   - `local_dpo_creation`
   - `local_continue_training`
   - `common_training`
   - `checkpoint_mutation`
   - `target_promotion`
   - `rollback_probe`
3. Added packet fields for:
   - current Luba approval quote;
   - `quote_source: current_unquoted_message_only`;
   - `old_transcript_or_state_approval_reused: false`;
   - source/license and redaction review;
   - training and held-out dataset hashes;
   - base, target and rollback checkpoint paths and hashes;
   - manifest and locator candidate;
   - required fresh-load, continue-training, held-out, ablation and rollback probes;
   - `deferred_decision_queue`.
4. Added exact source/license gate:
   - common training only with verified MIT, Apache-2.0, or separately approved permissive license evidence;
   - private/project/user rows stay user-local unless separately redacted, license-reviewed and approved;
   - unknown, GPL/copyleft, commercial, private or missing-license material cannot enter common training.
5. Added target/rollback gate:
   - target and rollback paths must be named before mutation;
   - dataset shas and checkpoint shas must be recorded together;
   - probes must run against the named files;
   - locator update stays inactive until proof and separate target promotion approval.
6. Added rule that missing approval for one track is not a global stop: defer that track and continue safe independent approved tracks.
7. Expanded Section 10 with track-separated minimum approval fields.
8. Updated Section 8 row 7 to point to this packet.

## Recheck evidence

After the fix, the plan now requires:

- exact current Luba approval for each risky track;
- no reused transcript/state/old approval;
- separate gates for host runtime, token metrics, quality eval, DPO, training, checkpoint mutation, target promotion and rollback;
- source/license binding before common training;
- target/rollback binding before checkpoint mutation or promotion;
- fresh-load, continue-training, held-out, ablation and rollback probes before promotion;
- deferred decision queue instead of global stop when only one track lacks approval;
- no final full-extension claim without packet-backed runtime/eval/training evidence.

## Scope

No product code, runtime, training, tests/build, git command, commit or push was run in this pass.

Overall complete claimed: false.
