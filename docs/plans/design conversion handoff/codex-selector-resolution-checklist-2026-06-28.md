# Codex Selector Resolution Checklist - 2026-06-28

claim_scope:
- Created this documentation-only selector-resolution checklist.
- Used the packet execution sweep as the evidence source for selector/action ambiguity.
- Listed packet families blocked by `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`.
- Listed Packet Group 7 separately because the sweep classified it as missing secret/external-account input, not selector-preflight-only failure.
- Did not execute packet actions.
- Did not edit product/source repositories, AGENTS.md, handoff index, plans, tests, evidence files, or runtime artifacts.
- Did not stage, commit, push, mutate PRs, merge, close, archive, delete, restore, stash, run tests, run browser/runtime probes, use credentials, or call external AI.

evidence_scope:
- Source evidence: `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-packet-execution-sweep-2026-06-28.md`.
- The sweep states that no packet action passed preflight because concrete selectors, policies, destinations, scenarios, or external-provider inputs were missing.
- This checklist is a resolution aid only.

count_claimed:
- 1 checklist file created.
- 9 packet families listed as selector/action preflight failures.
- 1 related external-account packet family listed separately.
- 0 packet actions executed.

independent_evidence_count:
- 1 packet execution sweep read.

subagent_id_or_NONE: SELECTOR_RESOLUTION_CHECKLIST

downgrade_needed:
- Yes for product completion, packet execution, merge readiness, clean-repo readiness, runtime proof, visual-fidelity proof, external AI proof, package provenance, trace sensitivity, owner decision resolution, or branch policy resolution.

## Not Product Completion

This is not product completion.

This checklist does not prove runtime readiness, visual fidelity, merge readiness, clean repository state, external AI judging, trace sensitivity, package provenance, owner approval, or any executed packet action.

The evidence supports only this narrow claim: packet selector/action ambiguity was converted into a checklist of missing fields and future bounded work orders.

## Selector/Action Ambiguity Identified

The packet execution sweep found that packet families offered multiple mutually exclusive actions, but the execution packets did not supply binding owner selectors for:

- exact packet family;
- exact target set;
- selected action;
- approval target;
- safety policy;
- mutation policy;
- verification evidence;
- rollback or preservation policy where mutation is possible.

Because those fields were absent, the safe execution state was:

`PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`

The selector must be explicit enough that a future worker can decide before tool use whether the requested action is allowed, in scope, and reversible or intentionally irreversible.

## Recommended Selector Format

Use one selector block per packet family. Do not combine unrelated families into one vague instruction.

```yaml
packet_selector:
  packet_family_id: "<group-number-and-name>"
  selected_action: "<one allowed action from that packet family>"
  selected_targets:
    - "<exact repo/pr/path/category/branch/scenario target>"
  approval_target: "<what Luba is approving, stated literally>"
  required_current_state:
    - "<state that must be re-verified before mutation>"
  mutation_policy:
    allowed: true
    exact_mutations:
      - "<single permitted mutation>"
    forbidden_mutations:
      - "<explicitly forbidden mutation>"
  preservation_or_rollback_policy:
    backup_required: true
    backup_destination: "<exact path/ref, or not-applicable>"
    rollback_instruction: "<exact rollback or not-applicable>"
  verification_required:
    - "<exact readback/evidence after action>"
  stop_condition:
    - "<condition that must block execution>"
```

Recommended owner approval wording:

```text
I approve Packet Group <N> only.
Selected action: <exact action>.
Selected targets: <exact targets>.
Approval target: <exact approval target>.
Required policy fields: <all required fields for this group>.
No other packet action is approved.
```

Example for PR actions:

```yaml
packet_selector:
  packet_family_id: "1-pr-state-actions"
  selected_action: "mark-ready"
  selected_targets:
    - repo: "lubochka/llm_mvp_core"
      pr_number: 6
      required_head_sha: "1555cc58f50d4c10fa486bede15bd0ddd31f37b1"
  approval_target: "mark only this exact PR ready for review if live metadata still matches"
  required_current_state:
    - "state=open"
    - "draft=true"
    - "merged=false"
    - "head_sha=1555cc58f50d4c10fa486bede15bd0ddd31f37b1"
  mutation_policy:
    allowed: true
    exact_mutations:
      - "mark pull request ready for review"
    forbidden_mutations:
      - "merge"
      - "close"
      - "retarget"
      - "edit branch refs"
  verification_required:
    - "read PR metadata after mutation"
  stop_condition:
    - "live PR metadata does not match required state"
```

Example for documentation-only defer:

```yaml
packet_selector:
  packet_family_id: "6-root-trace-json"
  selected_action: "defer"
  selected_targets:
    - "xiigen-codex-request.json"
    - "xiigen-codex-response.json"
    - "xiigen-codex-trace.json"
  approval_target: "record trace JSON as unresolved and do not inspect contents"
  mutation_policy:
    allowed: false
    exact_mutations: []
    forbidden_mutations:
      - "content inspection"
      - "redaction"
      - "archive"
      - "delete"
      - "stage"
      - "commit"
      - "push"
  verification_required:
    - "readback that unresolved boundary is documented"
```

Example for runtime proof:

```yaml
packet_selector:
  packet_family_id: "8-runtime-design-proof"
  selected_action: "require-more-proof"
  selected_targets:
    - repo: "<exact repo path>"
    - scenario_script: "<exact command or script entrypoint>"
    - evidence_dir: "<exact output directory>"
  approval_target: "run only this exact scenario and collect only the named evidence"
  required_current_state:
    - "approved ports/services/window targets supplied"
    - "expected visible states supplied"
    - "pass thresholds supplied"
  mutation_policy:
    allowed: true
    exact_mutations:
      - "create scenario evidence files in approved evidence directory"
    forbidden_mutations:
      - "source code edit"
      - "PR mutation"
      - "git stage/commit/push"
  verification_required:
    - "scenario command output"
    - "evidence file existence"
    - "pass/fail summary against supplied thresholds"
```

## Packet Family Checklist

| Packet family | Sweep blocker | Missing selector/approval target fields | Future bounded work order once resolved |
| --- | --- | --- | --- |
| 1 - PR state actions for five PRs | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`: missing PR set/action selector; merge missing merge method and ready/waiver policy. | `packet_family_id`; exact PR set; selected action from `leave-draft`, `mark-ready`, `merge`, `close`; required head SHA per PR; approval target per PR; merge method if merge; draft-ready or waiver policy; post-action metadata verification; stop condition for SHA/state drift. | Worker: `bounded-pr-state-action-worker`. Re-read live metadata for selected PRs only, verify repo/PR/head/state/draft/merged/mergeable fields, execute only the selected PR action, then return before/after metadata and no unrelated PR mutation. |
| 2 - Core branch policy | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`: missing branch policy selector and preservation/ref-mutation policy. | `packet_family_id`; selected branch policy from `keep DesignToCode-clean`, `rename/move to exact DesignToCode`, `preserve both and create exact-name PR`, `preserve both only`, `defer`; exact source ref; exact destination ref; remote/local scope; preservation policy; ref-mutation approval; fetch/prune permission if needed; rollback ref. | Worker: `bounded-core-branch-policy-worker`. Re-read local and remote refs, verify selected source/destination SHAs, perform only the approved branch/ref or documentation action, then return ref readback and rollback evidence. |
| 3 - Core evidence and residue | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`: missing category/action selectors and sensitivity/provenance/archive/cleanup/commit policy inputs. | `packet_family_id`; exact evidence/residue categories; selected action from `commit`, `archive`, `cleanup`, `waive`, `defer`; sensitivity inspection approval; provenance inspection approval; archive destination; cleanup backup policy; commit message; push policy; exact paths; verification counts. | Worker: `bounded-core-evidence-residue-worker`. Re-run targeted status/count probes, inspect only approved paths if sensitivity/provenance approval exists, execute only selected action for selected categories, then return path list, counts, and mutation/readback evidence. |
| 4 - Package artifacts | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`: missing package action selector and required package/provenance/archive/commit/cleanup inputs. | `packet_family_id`; selected package set; selected action from `restore platform packages`, `archive selected packages`, `commit after provenance verification`, `cleanup untracked flows packages`, `defer`; provenance rule; hash acceptance rule; archive destination; cleanup policy; commit message; push policy; exact package paths. | Worker: `bounded-package-artifact-worker`. Re-hash selected packages, apply the selected restore/archive/commit/cleanup/defer action only after provenance policy passes, then return before/after status, hashes, and destination or commit evidence. |
| 5 - Wiki-domain generated corpora | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`: missing repo/action selector and required archive/cleanup/commit/sensitivity inputs. | `packet_family_id`; selected repo set; selected action from `commit`, `archive`, `cleanup`, `waive`, `defer`; archive destination; redaction policy; cleanup backup policy; commit message; sensitivity/provenance check policy; file count/byte verification target. | Worker: `bounded-wiki-domain-corpora-worker`. Re-count selected corpus trees, inspect only approved samples or metadata, execute the selected action only for selected repos, then return counts, archive/cleanup/commit/waiver evidence, and no unrelated repo mutation. |
| 6 - Root trace JSON | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`: missing explicit trace action selector and sensitivity/redaction/archive/cleanup/commit policy. | `packet_family_id`; selected trace files; selected action from `sensitivity inspect`, `commit after sensitivity review`, `archive outside repo`, `clean/delete`, `waive`, `defer`; sensitivity/redaction approval; archive destination; cleanup/delete approval; commit message; push policy; no-secret-print rule. | Worker: `bounded-trace-json-worker`. Re-read file metadata, inspect contents only if explicitly approved, redact/archive/commit/delete/defer only as selected, never print secrets, then return metadata, redaction summary, and destination or status evidence. |
| 8 - Runtime/design proof options | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`: missing runtime/design proof policy and exact scenario commands/targets/thresholds. | `packet_family_id`; selected proof policy from `require more proof`, `accept saved-output boundary for handoff only`, `waive additional proof`, `defer`; exact repo/service commands; browser/host UI permission; evidence directory; ports/services/window targets; scenario script path; expected visible states; pass thresholds; allowed artifact writes. | Worker: `bounded-runtime-design-proof-worker`. Run only the approved scenario or documentation action, collect only named evidence, compare to supplied thresholds, then return command output, screenshots/files, and pass/fail summary without source/git/PR mutation. |
| 9 - Figma nested worktree `romantic-dewdney` | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`: missing explicit worktree policy and git mutation/preservation inputs. | `packet_family_id`; selected worktree policy from `preserve/exclude`, `commit-push`, `stash`, `cleanup/reset`; owner intent; exact worktree path; stash message if stash; target remote/branch/PR if commit-push; backup/ref preservation policy; exact cleanup scope; rollback/ref recovery instruction. | Worker: `bounded-figma-nested-worktree-worker`. Re-read nested worktree status, verify selected owner policy, execute only preserve/stash/commit-push/cleanup action as approved, then return status, refs, stash/commit/backup evidence, and no unrelated cleanup. |
| 10 - Figma fixture status-only residue | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`: missing fixture policy selector and mutation approval for refresh/restore. | `packet_family_id`; exact fixture paths; selected policy from `refresh index`, `waive`, `restore`, `owner-inspect`; index mutation approval for refresh; working-tree mutation approval for restore; owner inspection scope; verification command; stop condition if content diff appears. | Worker: `bounded-figma-fixture-residue-worker`. Re-run status and no-content-diff probes for exact fixture paths, execute only approved refresh/waive/restore/inspect action, then return status/diff readback and no unrelated fixture mutation. |

## Related Non-Selector Preflight Case

Packet Group 7 - External AI visual judge was blocked by:

`PACKET_BLOCKED_BY_MISSING_SECRET_OR_EXTERNAL_ACCOUNT`

It is not counted in the 9 `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` selector/action families above.

Missing fields before any future visual judge work:

- provider;
- endpoint;
- model;
- approved secret environment variable name;
- credential availability confirmation without printing the value;
- data-sharing approval;
- design reference inputs;
- artifacts to judge;
- rubric;
- pass threshold;
- local/offline judge permission if external provider is not approved.

Future bounded work order:

Worker: `bounded-visual-judge-config-worker`. Create or verify a doc-only visual judge configuration packet. Do not read or print secret values. Do not call external AI. Do not upload artifacts. Return only the resolved configuration checklist or the exact missing input list.

## Selector Resolution Gate

A future packet worker may execute only if all required selector fields for its packet family are present and internally consistent.

Required pre-execution readback:

- packet family id;
- selected action;
- selected targets;
- approval target;
- exact allowed mutation;
- exact forbidden mutations;
- current-state checks;
- rollback or preservation policy;
- verification evidence names;
- stop conditions.

If any required field is absent, vague, contradictory, stale, or broader than the worker scope, the worker must stop before mutation and report:

`PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`

## Future Bounded Work Orders

1. `bounded-pr-state-action-worker`
   - Scope: Packet Group 1 only.
   - Execute only selected PR action after live PR metadata preflight passes.
   - No branch/ref mutation unless separately authorized.

2. `bounded-core-branch-policy-worker`
   - Scope: Packet Group 2 only.
   - Execute only selected branch/ref policy after local/remote ref preflight passes.
   - No PR state mutation unless separately authorized.

3. `bounded-core-evidence-residue-worker`
   - Scope: Packet Group 3 only.
   - Execute only selected evidence/residue action after selector, sensitivity, archive, cleanup, commit, and push policy fields are supplied.
   - No unapproved content inspection.

4. `bounded-package-artifact-worker`
   - Scope: Packet Group 4 only.
   - Execute only selected package action after provenance and selected package set are supplied.
   - No unapproved restore, cleanup, archive, stage, commit, or push.

5. `bounded-wiki-domain-corpora-worker`
   - Scope: Packet Group 5 only.
   - Execute only selected corpus action after repo set, sensitivity policy, archive/cleanup policy, and verification targets are supplied.
   - No unapproved content inspection or delete.

6. `bounded-trace-json-worker`
   - Scope: Packet Group 6 only.
   - Execute only selected trace action after sensitivity/redaction/archive/cleanup/commit policy is supplied.
   - No secret printing.

7. `bounded-runtime-design-proof-worker`
   - Scope: Packet Group 8 only.
   - Execute only selected runtime/design proof action after exact scenario commands, expected states, thresholds, and evidence directory are supplied.
   - No source/git/PR mutation.

8. `bounded-figma-nested-worktree-worker`
   - Scope: Packet Group 9 only.
   - Execute only selected worktree policy after owner intent, preservation, and exact mutation fields are supplied.
   - No unrelated cleanup/reset/stash.

9. `bounded-figma-fixture-residue-worker`
   - Scope: Packet Group 10 only.
   - Execute only selected fixture policy after exact paths and mutation approval are supplied.
   - No unrelated fixture mutation.

10. `bounded-visual-judge-config-worker`
    - Scope: Packet Group 7 configuration only.
    - Prepare or verify visual judge inputs.
    - No credential value readback, no external AI call, no artifact upload.

## Verification Checklist For This File

Targeted readback should verify:

- literal `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`;
- packet families 1, 2, 3, 4, 5, 6, 8, 9, and 10;
- Packet Group 7 separately as `PACKET_BLOCKED_BY_MISSING_SECRET_OR_EXTERNAL_ACCOUNT`;
- selector fields: packet family id, selected action, selected targets, approval target, mutation policy, preservation or rollback policy, verification required, stop condition;
- future bounded work orders for all listed families;
- statement that this is not product completion.

must_stop_after_report=true
