# PR_STATE_ACTION_RESOLVER - 2026-06-28

claim_scope:
- Bounded PR state action resolver packet for the five known DesignConversion PRs.
- Defines which future PR actions are safe or unsafe under the current evidence boundary: leave draft, update title/body, request review, close/reopen, mark ready, and merge.
- No PR mutation was performed.

evidence_scope:
- Read `codex-preflight-resolution-sweep-2026-06-28.md`.
- Read `codex-designconversion-handoff-index-2026-06-28.md`.
- Performed read-only GitHub metadata checks for the five known PRs through the GitHub connector.
- Did not run tests, builds, browser/runtime probes, external AI, gh mutation commands, git mutation commands, PR mutations, staging, commits, pushes, branch creation, or secret access.

count_claimed:
- resolver packet files created: 1
- known PRs covered: 5
- live read-only PR metadata checks: 5
- PR mutations performed: 0

independent_evidence_count:
- 8 current-turn evidence records: two source document reads, five live read-only PR metadata reads, and one target-file preexistence check.

subagent_id_or_NONE:
- PR_STATE_ACTION_RESOLVER

downgrade_needed:
- Yes for any broader claim. The evidence supports only a conservative PR action routing packet. It does not prove merge readiness, review readiness, product completion, clean repository state, full runtime proof, full visual fidelity, or Luba approval for any PR mutation.

## Required Boundary

Because the exact Luba selector/policy is absent, every PR-mutating action remains `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`.

Recommended default:
- `no PR mutation`
- keep all five PRs draft/open as handoff artifacts
- do not update titles or bodies
- do not request reviewers
- do not close or reopen
- do not mark ready
- do not merge
- wait for Luba to provide an `exact PR action selector`

## Known PR Metadata

Live read-only GitHub metadata observed in this worker:

| PR key | Repository | PR | State | Draft | Merged | Mergeable | Base | Head branch | Head SHA | Requested reviewers |
| --- | --- | ---: | --- | --- | --- | --- | --- | --- | --- | --- |
| core#6 | `lubochka/llm_mvp_core` | 6 | open | true | false | true | `main` | `DesignToCode-clean` | `642b67c2bb2330b35a2b5a4e3972f7c6657abff3` | none returned |
| platform#1 | `lubochka/xiigen-platform-dot-net-angular` | 1 | open | true | false | true | `main` | `DesignToCode` | `bf62e291eca3c366641345781ebda85c7d255f90` | none returned |
| flows#1 | `lubochka/xiigen-flows-dot-net-angular` | 1 | open | true | false | true | `main` | `DesignToCode` | `5e6836e7328cd896c686ae083e8014b8b94db09b` | none returned |
| figma#40 | `lubochka/FigmaToCode-design-gen` | 40 | open | true | false | true | `improve_group_css_user` | `DesignToCode` | `500fcab72e2df9ef7ce8999a63c786e26d24db47` | none returned |
| canva#1 | `lubochka/CanvaToCode` | 1 | open | true | false | true | `main` | `DesignToCode` | `2fa6629c9a1e3a70e5385fa4afa8109a8d264add` | none returned |

Boundary:
- These metadata checks show current PR state only.
- `mergeable: true` does not authorize merge.
- `draft: true` supports the conservative no-op default: leave draft/open.
- No live CI/check conclusion review was performed.
- No PR diff review was performed.
- No product readiness verification was performed.

## Action Decision Matrix

| Future action | Current safe status | Why | Required selector/policy before execution |
| --- | --- | --- | --- |
| Leave draft/open | Safe only as `no PR mutation` | All five PRs are already open drafts; preserving state is a no-op. | None for no-op preservation. If a worker would touch PR metadata, require selected PRs and expected head SHA confirmation. |
| Update title/body | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` | This mutates PR metadata and no exact title/body policy or target subset was supplied. | `exact PR action selector`, selected PRs, exact replacement text or patch instructions, expected head SHA policy, before/after verification fields. |
| Request review | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` | This mutates PR review state and no reviewer/team selector was supplied. | `exact PR action selector`, selected PRs, reviewer usernames or team slugs, expected head SHA policy, notification/ownership policy. |
| Close PR | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` | This mutates PR state and may discard/interrupt handoff flow. No close policy was supplied. | `exact PR action selector`, selected PRs, close reason, preservation/handoff consequence acceptance, expected head SHA policy. |
| Reopen PR | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` | All known PRs are currently open, so reopen is not applicable; any state mutation still needs explicit selection. | `exact PR action selector`, selected PRs, current closed-state confirmation, reopen reason, expected head SHA policy. |
| Mark ready | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` | This converts draft PRs to ready-for-review and no review-readiness or owner waiver policy was supplied. | `exact PR action selector`, selected PRs, review-readiness policy, required evidence/waiver statement, expected head SHA policy. |
| Merge | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` | Merge is publishing/destructive relative to draft handoff state and lacks merge method, readiness, CI, review, and owner policy. | `exact PR action selector`, selected PRs, merge method, required checks/reviews/waivers, expected head SHA policy, post-merge verification target. |

## Safe Default Packet

packet_id:
- `PR_STATE_ACTION_RESOLVER_KEEP_DRAFT_OPEN_NOOP_ALL_FIVE`

selected_action:
- leave current PR state unchanged

selected_targets:
- core#6
- platform#1
- flows#1
- figma#40
- canva#1

mutation_policy:
- `no PR mutation`

current result:
- resolved for conservative no-op only
- all mutating PR actions remain `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`

stop condition:
- If any future worker is asked to mutate PR state and any selected PR head SHA differs from the metadata table above, stop that PR action and report `HEAD_CHANGED_REQUIRES_OWNER_CONFIRMATION`.

## Future Bounded Work Orders

### 1. No-Op Preservation Worker

worker_id:
- `bounded-pr-state-noop-preservation-worker`

allowed action:
- Re-read selected PR metadata.
- Confirm PRs remain draft/open.
- Report only.

forbidden action:
- no title/body update
- no review request
- no close/reopen
- no mark-ready
- no merge
- no git mutation
- no tests/builds/runtime/browser

required output:
- before/current metadata table
- explicit `no PR mutation` statement
- downgrade if any PR changed state or head SHA

### 2. Title/Body Update Worker

worker_id:
- `bounded-pr-title-body-update-worker`

required Luba input:
- `exact PR action selector`
- selected PR subset
- exact new title/body or exact patch text
- expected head SHA confirmation policy

allowed action after approval:
- Update only selected PR title/body fields.
- Return before/after metadata and changed text summary.

blocked until then:
- `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`

### 3. Review Request Worker

worker_id:
- `bounded-pr-review-request-worker`

required Luba input:
- `exact PR action selector`
- selected PR subset
- reviewer usernames or team slugs
- notification/ownership policy
- expected head SHA confirmation policy

allowed action after approval:
- Request only the selected reviewers/teams on selected PRs.
- Return before/after requested-reviewer metadata.

blocked until then:
- `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`

### 4. Close/Reopen Worker

worker_id:
- `bounded-pr-close-reopen-worker`

required Luba input:
- `exact PR action selector`
- selected PR subset
- exact action: close or reopen
- reason and handoff consequence acceptance
- expected head SHA confirmation policy

allowed action after approval:
- Close or reopen only selected PRs.
- Return before/after state metadata.

blocked until then:
- `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`

### 5. Ready-For-Review Worker

worker_id:
- `bounded-pr-mark-ready-worker`

required Luba input:
- `exact PR action selector`
- selected PR subset
- readiness policy or explicit owner waiver
- required evidence references or explicit acceptance that readiness is not proven
- expected head SHA confirmation policy

allowed action after approval:
- Mark only selected draft PRs ready for review.
- Return before/after draft-state metadata.

blocked until then:
- `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`

### 6. Merge Worker

worker_id:
- `bounded-pr-merge-worker`

required Luba input:
- `exact PR action selector`
- selected PR subset
- merge method: merge, squash, or rebase
- required checks/reviews/waivers
- expected head SHA for each PR
- post-merge verification target

allowed action after approval:
- Merge only selected PRs whose current head SHA exactly matches approved expected SHA.
- Return before/after state and merge SHA metadata.

blocked until then:
- `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`

## Final Boundary

This packet intentionally preserves draft/open handoff state. It does not authorize any PR mutation.

Current bounded verification addendum:
- Required PR label literals: `PR #6`, `PR #1`, `PR #40`.
- Required boundary literals: not merge readiness; not review readiness; not product completion; not runtime readiness; not visual fidelity.

Required literals present:
- `PR_STATE_ACTION_RESOLVER`
- `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`
- `no PR mutation`
- `exact PR action selector`

files_changed:
- `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-pr-state-action-resolver-2026-06-28.md`

verification:
- Source documents were read.
- Five live read-only PR metadata checks were performed.
- Target file did not exist before this worker wrote it.
- No readback, tests, builds, runtime/browser checks, git mutations, or PR mutations were performed.
