# Task/Slice Golden Rule — S04 stages and unchanged other-slice procedures

## 1. S04 scope and task identity

Sections 1–7 and their verification literals below are the current S04 procedure. They control the S04 interpretation of the Golden Rule summaries in AGENTS.md and CLAUDE.md. The separately labelled other-slice procedure at the end does not apply to S04.

Keep the owner's requested scope and original catalogue IDs. For S04 a delivery unit is one original catalogue microtask with all its applicable predicates. An admitted product increment remains an internal source/test stage of that microtask, not a new task or acceptance boundary. Design, review, implementation, test execution, correction, publication and worker handoff are internal stages unless the owner separately requests one as an independent deliverable. A role's CURRENT TASK heading and an agent-created sublabel are not such an owner request.

For S04 the installed TASK-GOLDEN-RULE.md defines these unit/stage boundaries and GIT-PUBLICATION-CONTRACT.md defines its publication lifecycle. Read them as the detailed S04 implementation of the Golden Rule referenced by AGENTS.md and CLAUDE.md. The S04 files replace the previous recursive interpretation of “every review-fix/evidence/recovery task”; they do not override OP-65, actual host restrictions, data rights, effect budgets or original product predicates. Other slices retain their own explicit catalogue, review and publication contracts; this repair does not activate or re-scope them.

A standalone no-code deliverable explicitly assigned by the owner must still be reviewed where applicable and durably committed. A status poll, review return, unchanged-source check and handoff do not become new deliverables merely to create a commit. One review cannot recursively require a plan review of that review, followed by a review of its evidence record.

## 2. Current reads and authority

At each new delivery unit, freshly read live AGENTS.md, CLAUDE.md, this skill and applicable current sources:

- docs/skills/planning-session-startup-SKILL.md;
- docs/skills/architect-behavior-classifier-SKILL.md;
- docs/skills/architectural-decision-testing-SKILL.md;
- docs/skills/intent-to-plan-SKILL.md;
- docs/skills/authority-requirement-binding-SKILL.md;
- docs/skills/reference-inventory-and-source-map-SKILL.md;
- docs/skills/phase-planning-SKILL.md;
- docs/skills/plan-review-SKILL.md;
- docs/skills/code-examination-SKILL.md;
- docs/skills/generated-code-review-SKILL.md;
- docs/skills/human-readable-architecture-audit-SKILL.md;
- docs/skills/agent-output-format-SKILL.md;
- additional current role/task requirements, including applicable code-execution, .NET and test-integrity guidance for source work.

Record actual skill_load_status and source identities. A fresh worker reads its own necessary sections and affected complete definitions/callers; another worker's read is not its own. Do not substitute an old summary for changed authoritative source.

Within the same unit, the parent retains read context and verifies changes rather than restarting this complete stack after every poll, role return or evidence-only write. After compaction restore the current capsule and latest user instruction first, then reread material no longer available or no longer current. Newly required dependencies are loaded; unrelated historical quotas are not imported. A changed material work order requires revalidation of affected authority/scope.

## 3. Design admission, implementation and independent review

Establish the current requirement, actual owner/caller, minimal allowed source/test paths and expected positive/negative behavior. Reuse an unchanged independently admitted design with its exact applicable evidence. A real missing design is solved and independently reviewed before its dependent implementation. Missing review response alone is not missing design.

Once a bounded source/test algorithm and scope are independently admitted, assign its executor after slot release. Do not demand the future runtime implementation as a prerequisite for allowing that implementation. Source/test admission, executed enforcement evidence and permission for real model effects remain separate.

Executor implements and runs applicable tests; a separate code reviewer checks actual source and raw results. Code/test corrections within unchanged admitted design do not restart architecture/plan review. A genuine algorithm/ownership/dependency/safety change receives the relevant design delta review. Neither skipped verification nor repeated review rounds create PASS.

This skill does not grant a reviewer product-write authority, convert self-review into independence, or permit normal effects through an unadmitted path. Existing grants, data separation, capacity guards and real-run gates remain in force.

## 4. Durability and exact-candidate acceptance

For S04, publish changed semantic design or implementation bytes as a scoped DRAFT before independent review, after appropriate local focused checks. Keep returned verdicts immediately in the existing durable task record. An internal design-review PASS can lead directly to its admitted implementation stage without another design ACCEPT/report-review task; preserve and carry that review evidence into the next scoped publication. Do not recreate an already published stage acceptance.

A different delivery unit does not start before required acceptance/publication of its predecessor. A different role within the same unit may start once its own prerequisites and release proof hold. DRAFT.next_task_allowed remains false for a different unit; it does not forbid its current unit's admitted executor or reviewer.

For actual product acceptance preserve the required evidence-only ACCEPT child of the reviewed DRAFT, exact-final-head validation, normal push/readback and report. No semantic changes are hidden in ACCEPT and no review-of-ACCEPT is recursively commissioned. A changed semantic candidate is reviewed again where required. Preserve original test/run identities and verify applicable evidence; do not pretend old tests ran on a new SHA.

The entire unit, not each routine status action, has one acceptance boundary. Scoped intermediate DRAFTs remain necessary for changed semantic candidates and repair durability. Explicit standalone no-code tasks still commit their actual evidence. No vague work artifacts message or catch-up batching of unrelated completed tasks.

## 5. Git and worktree preservation

Use the canonical root/branch from live agents, currently C:\Projects\llm_mvp_core / codex/dpo-implementation. Before integration fetch only, inspect index/working/untracked paths and incoming overlap, and preserve all relevant bytes. Use EXACT_REMOTE_EQUIVALENT, TASK_DRAFT_PRESERVED, OWNER_SNAPSHOT_PRESERVED or NO_OVERLAP_PRESERVED with proof. A tracked diff or branch at HEAD is not a complete backup.

At new-unit admission and before publication verify fetched development/main ancestry and zero unmerged paths. If MERGE_HEAD exists, finish narrow reconciliation and its commit/push/readback before semantic work. No blind stash/autostash, reset --hard, clean, blanket restore or force push. Reconcile changes before acceptance; if candidate identity/semantics change, revalidate and obtain the appropriate review rather than transferring PASS.

A failure of Git, tests or a tool remains the current unit's recovery action. Preserve evidence and owner work; use existing bounded escalation without resetting attempts. Do not turn each recovery observation into a new reviewed task.

## 6. Roles, supervision and continuation

Parent retains product ownership. Each worker returns a bounded result to parent and never awaits another worker's review or starts a nested role. OP-65 allows one TOTAL delegated agent. Obtain both supported termination evidence and cessation of attributable commands before the successor. Returned packet, timeout, lease expiry or cancellation ACK alone does not establish slot release.

Use the slice's bounded supervision and deadlines. A healthy active worker calls for supported wait/status/checkpoint actions, not a final “done”. No indefinite waiting, replacement over an occupied slot or unrelated process kill. Respect explicit user stop; unavoidable host/context interruption yields a durable INCOMPLETE checkpoint, not product acceptance or a promised background result.

Maintain the existing current capsule: unit, stage, candidate, worker, pending result, PASS/NEEDS_FIX/interrupted successor, next source/test increment and preserved effects/resources. Update at transitions. Restore it before rediscovering the whole programme after compaction. No extra ledger, schema, scheduler or review quota.

After a required independent admission is actually available, the next substantive action is the admitted executor, not another status-only task. After unit acceptance/Git closure immediately continue the next authorized dependency-ready unit while the owner's scope remains open. Support repair, review result and task report are transitions, not whole-slice completion.

## 7. Honest product evidence

A delivery claim needs an actual owner/caller, source/test changes or exact existing-code equivalence, executed applicable evidence and required independent review. Markdown, card counts, pack tests, flags, counters and a new checkpoint hash do not establish model learning or improved quality. Do not force a cosmetic code change to satisfy a file-extension quota.

Keep mechanism, real lesson, measured quality and ordinary consumer use separate. Required negative/effect/capacity/recovery checks remain; incomplete or unknown is not a valid negative result. Final closure still requires the original applicable slice criteria and handoff, or a genuine evidenced external blocker. No false PASS or waiver of safety to force progress.

## Verification literals — “task” has the scoped meaning in section 1

`ACTIVE_SUBAGENT_FINAL_FORBIDDEN_GATE`
`subagent waiting is not a turn boundary`
`progress update must be followed by wait/status/recovery in the same turn`
`wait timeout is not final`
`NEEDS_FIX/PARTIAL is continuation, not final`
`FETCH_BEFORE_PULL_DIRTY_WORKTREE_GATE`
`DIRTY_OVERLAP_MUST_BE_CLASSIFIED_AND_PRESERVED`
`NO_DESTRUCTIVE_CLEANUP_BEFORE_PRESERVATION_PROOF`
`OPEN_MERGE_IS_RECONCILE_ONLY`
`REVIEW_ONLY_REMOTE_DRAFT_SHA`
`AFTER_GOVERNANCE_RESUME_PRODUCT_PREDICATE`
`GOVERNANCE_IS_SUPPORT_NOT_PRODUCT_PROGRESS`
`TASK CLOSURE IS A TRANSITION, NOT SLICE COMPLETION`
`AFTER TASK GIT CLOSURE CONTINUE THE NEXT READY UNIT WHEN USER SCOPE IS THE SLICE`
`PER-TASK REPORT IS PROGRESS, NOT PARENT FINAL`
`EVERY TASK STARTS WITH FRESH AGENT+SKILL READS`
`EVERY ACCOMPLISHED TASK GETS ITS OWN COMMIT`
`EVERY TASK COMMIT IS PUSHED BEFORE THE NEXT TASK`
`EVERY PUSH IS REMOTE-READ-BACK BEFORE THE NEXT TASK`
`MAIN AND REMOTE DEVELOPMENT ANCESTRY ARE RECHECKED BEFORE EVERY TASK AND PUSH`
`NO UNRESOLVED CONFLICTS MAY CROSS A TASK BOUNDARY`
`NO-CODE TASK STILL COMMITS ITS DURABLE EVIDENCE/STATE DELTA`
`THE NEXT TASK CANNOT START UNTIL CURRENT TASK GIT CLOSURE IS COMPLETE`

## Other slices — retained existing procedure; NOT the S04 execution route

The following pre-existing generic procedure is retained unchanged for consumers outside S04. Its per-task, per-result and per-role wording, examples and legacy S04 addendum do not override Sections 1–7 above for S04. No other slice is started or re-scoped by this repair. S04 readers stop at this boundary; do not reload the following procedure into an S04 worker packet as additional S04 obligations.

## 0. Active-subagent turn-continuation gate

An active current-task subagent is a mandatory continuation state, not a valid
final/status boundary.

If actual subagent state is running/waiting/cancel_pending/unknown or terminal
evidence is absent, the parent must not end the turn. It may send a progress
update, but the **next action in the same turn must be another supported
wait/status/checkpoint/recovery action**. A timeout or no packet does not satisfy
completion.

A returned NEEDS_FIX/PARTIAL/FAIL packet routes to the next authorized
repair/review action after supported release; it does not authorize a final
answer while the task is open.

Pre-final fields are mandatory:
`active_task_open`, `active_subagent_state`,
`terminal_packet_received`, `terminal_execution_proven`,
`next_bounded_action_exists`,
`next_wait_or_recovery_action_executed_this_turn`,
`true_external_blocker_certificate`, `explicit_user_stop`.

Hard literals:
`ACTIVE_SUBAGENT_FINAL_FORBIDDEN_GATE`
`subagent waiting is not a turn boundary`
`progress update must be followed by wait/status/recovery in the same turn`
`wait timeout is not final`
`NEEDS_FIX/PARTIAL is continuation, not final`

## 1. Fresh pre-task read gate — every task, no reuse by assumption

Before starting **each new task/unit**, the parent/manager must freshly read the live repository copies of:

- `AGENTS.md`
- `CLAUDE.md`
- `docs/skills/planning-session-startup-SKILL.md`
- `docs/skills/architect-behavior-classifier-SKILL.md`
- `docs/skills/architectural-decision-testing-SKILL.md`
- `docs/skills/intent-to-plan-SKILL.md`
- `docs/skills/authority-requirement-binding-SKILL.md`
- `docs/skills/reference-inventory-and-source-map-SKILL.md`
- `docs/skills/phase-planning-SKILL.md`
- `docs/skills/plan-review-SKILL.md`
- `docs/skills/code-examination-SKILL.md`
- `docs/skills/generated-code-review-SKILL.md`
- `docs/skills/human-readable-architecture-audit-SKILL.md`
- `docs/skills/agent-output-format-SKILL.md`
- this file, `docs/skills/task-slice-golden-rule-SKILL.md`
- every additional task-specific skill named by the current task card/work order.

Reading them once at slice start is not enough. A resume, next microtask, next code slice, or changed work order requires a fresh read. Record `skill_load_status` and the current task ID before delegation/implementation.

No architect, planner, reviewer, executor, manager, or recovery worker may act on a new task until this gate is complete.

## 2. Mandatory architecture/plan/review gate before implementation

For every task:
1. bind current authority and exact task scope;
2. resolve current source ownership/callers/consumers;
3. load architecture and planning obligations;
4. establish the smallest current plan;
5. run the applicable separate plan-review gate before code/effect execution;
6. for code work, load code-examination before editing and generated-code-review/code-review obligations before acceptance;
7. prepare the human-readable task explanation/status boundary.

A prior task's plan/review is reusable only when the current task explicitly proves it is unchanged and records that proof. Do not silently inherit a stale plan.

## 2A. Dirty-worktree preservation gate before reconciliation

A dirty canonical checkout is an execution state that must be preserved, not something to erase so Git can move.

Before any merge/rebase/`git pull`/restore/reset/clean/checkout-overwrite that can rewrite the working tree:
1. run `git fetch --prune origin` only;
2. record `git status --porcelain=v1 --untracked-files=all`;
3. record paths changed by fetched `origin/<current-branch>` and `origin/main` relative to local HEAD;
4. compute local/incoming overlap and classify local ownership;
5. compare overlapping local bytes with fetched remote bytes;
6. resolve as exactly one of: `EXACT_REMOTE_EQUIVALENT`, `TASK_DRAFT_PRESERVED`, `OWNER_SNAPSHOT_PRESERVED`; dirty non-overlap is `NO_OVERLAP_PRESERVED`;
7. only after preservation proof may reconciliation rewrite those paths;
8. after reconciliation require zero unmerged paths and re-run invalidated validation.

Task-owned differing work is preserved durably as the appropriate DRAFT/governance publication before reconciliation. Unrelated owner work is not smuggled into the task commit: preserve it losslessly outside the task publication and verify reapplication. Exact remote-equivalent bytes may be removed/restored only after their equivalence is recorded.

A Git abort saying tracked changes or untracked files would be overwritten routes to this gate. It is not permission for `git reset --hard`, `git clean -fd`, blanket restore/checkout, blind stash, owner escalation, or starting a different task.

Hard literals:
`FETCH_BEFORE_PULL_DIRTY_WORKTREE_GATE`
`DIRTY_OVERLAP_MUST_BE_CLASSIFIED_AND_PRESERVED`
`NO_DESTRUCTIVE_CLEANUP_BEFORE_PRESERVATION_PROOF`

## 3. Canonical workspace and branch gate — before every task

The canonical workspace is `C:\Projects\llm_mvp_core`. The canonical implementation branch remains the live branch required by `AGENTS.md` / `CLAUDE.md`, currently `codex/dpo-implementation`, unless Luba explicitly changes it in a current unquoted instruction.

Before every task:
1. `git fetch --prune origin`;
2. record repository root, current branch, local HEAD, `origin/<current-branch>`, `origin/main`, and `git status --short`;
3. current branch must match the canonical branch;
4. working tree must be understood; unrelated owner work must be preserved;
5. `origin/main` must be an ancestor of HEAD;
6. `origin/<current-branch>` must be an ancestor of HEAD;
7. if either check fails, reconcile/merge before task work;
8. conflicts must be resolved from actual source ownership; no blanket ours/theirs shortcut;
9. `git diff --name-only --diff-filter=U` must be empty before task execution.

A successful `git pull` is not sufficient evidence by itself. Record the actual ancestry checks.

## 4. Every accomplished task requires its own durable commit

Every accomplished task/unit must end in a **task-scoped commit**.

- If product/test/code changed: commit those reviewed owned changes plus the task work/evidence record.
- If the task is verification/evidence/no-code: commit its work/evidence/state/report delta.
- If literally no repository delta exists, the task is not "accomplished"; it may only be reported as verified/no-change after writing the durable work-state/evidence record and committing that record.
- Do not accumulate several completed task units into one later catch-up commit.
- Do not use vague commit messages such as `work artifacts`; include the task ID and bounded outcome.

Example: `S04.01A: verify parent state inventory`.

## 5. Validation + independent review before publication

Before the task commit is considered publishable:
1. inspect exact owned diff;
2. reject accidental generated/cache/build outputs unless explicitly product artifacts;
3. run required structured/static checks;
4. run required focused tests;
5. run required impacted/wider checks;
6. complete separate code/evidence review where required;
7. repair findings and re-run affected checks;
8. update the task work record with actual evidence.

No PASS from a worker substitutes for manager readback of the actual diff/results.

## 6. Conflict-free Git reconciliation before every push

Immediately before committing/pushing a completed task, fetch again because remote/main may have moved while the task was executing.

Then:
1. `git fetch --prune origin`;
2. reconcile `origin/<current-branch>` if it advanced;
3. reconcile `origin/main` if it advanced;
4. resolve all conflicts before proceeding;
5. prove `git diff --name-only --diff-filter=U` is empty;
6. prove `git merge-base --is-ancestor origin/main HEAD` succeeds;
7. prove `git merge-base --is-ancestor origin/<current-branch> HEAD` succeeds;
8. run the repository's main-compatibility/merge-tree validation when available and require a clean result;
9. re-run any checks invalidated by reconciliation.

A force push is not a reconciliation method.

## 7. Commit → push → remote readback → human report is one atomic task closure

For **every** completed task:
1. stage only the task-owned reviewed files;
2. commit with task ID + bounded outcome;
3. push the current canonical branch normally;
4. fetch/read back the remote;
5. prove the remote branch contains the task commit and expected HEAD lineage;
6. record local commit SHA and remote readback SHA;
7. report, in human-readable form:
   - task ID and meaning;
   - what changed;
   - validation/tests;
   - review result;
   - merge/reconciliation result;
   - commit SHA;
   - pushed remote SHA;
   - remaining work / next task.

**The next task may not start until all seven closure steps are complete.**

Local-only commits, unpushed work, remote mismatch, unresolved conflict, or missing report mean the current task is still open.

## 7A. Task closure is a transition, not slice/run completion

Section 7 is an atomic **task** closure gate. It does not authorize the parent to end a user request whose current unquoted scope is the whole active slice. After remote readback and the human-readable task report, if a safe dependency-ready task/review/repair/recovery action remains, the parent immediately repeats Sections 1–3 for that next unit and continues in the same run.

A worker report, reviewer PASS/NEEDS_FIX, timeout, governance repair, Git closure, commit, push, or per-task human report may be a progress boundary but is not a parent-final boundary. Parent final/hold before slice completion requires a true external blocker certificate proving no other safe authorized action can advance the slice, unless the user explicitly orders a stop/pause.

Verification literals:
`TASK CLOSURE IS A TRANSITION, NOT SLICE COMPLETION`
`AFTER TASK GIT CLOSURE CONTINUE THE NEXT READY UNIT WHEN USER SCOPE IS THE SLICE`
`PER-TASK REPORT IS PROGRESS, NOT PARENT FINAL`

## Publication-state and planning-sync integrity gate

Before every task and again before publication, compare the installed live slice controls with the current planning-pack source of truth. Record the planning commit SHA. A mismatch in requested model/effort, Golden Rule, Git publication contract, slice continuation contract, packet common sources, receipt requirements, or execution-integrity contract is repaired before dependent work.

Every pushed task result is classified:
- `DRAFT <task-id>: ...` = durable author/executor output awaiting required review; push/readback required, next different task forbidden.
- `ACCEPT <task-id>: ...` = independently accepted closure; requires candidate-bound PASS and validation of the exact final SHA.
- `GOVERNANCE S04: ...` / `RECONCILE S04: ...` = durable repair only; cannot close the active product task.
- `META A+B: ...` = only for an explicitly authorized shared meta-task.

A merge/reconciliation result is a new candidate. Re-run required checks on the merge HEAD and bind the publication receipt to that SHA before acceptance.

Generic artifact commit messages, accidental cache artifacts, or committed textual conflict markers fail the publication gate.

For S04 use `.kiro/xiigen-slices/luna/s04/EXECUTION-INTEGRITY-CONTRACT.md` and its checker/schema.

## S04 bounded-merge / product-continuation addendum

For S04, these rules override any older workflow ordering:
- if `MERGE_HEAD` exists, finish reconciliation commit/push/readback before semantic executor/reviewer/advisor work;
- changed task-owned semantic bytes are committed/pushed/read back as DRAFT before independent review;
- NEEDS_FIX produces a new DRAFT SHA before rereview;
- after governance/reconcile publication, immediately resume the first dependency-ready S04 product predicate unless a distinct evidenced blocker prevents it;
- governance/skills/agent-file work is support-only and cannot count as the first real safe lesson or S04 completion.

`OPEN_MERGE_IS_RECONCILE_ONLY`
`REVIEW_ONLY_REMOTE_DRAFT_SHA`
`AFTER_GOVERNANCE_RESUME_PRODUCT_PREDICATE`
`GOVERNANCE_IS_SUPPORT_NOT_PRODUCT_PROGRESS`

## 8. Push/merge failure is part of the current task, not a new task and not an owner stop

If commit/push/main-compatibility/non-fast-forward/conflict handling fails:
- remain on the same task;
- do not start the next task;
- diagnose and reconcile immediately;
- use the architect/advisor escalation ladder after three materially different technical attempts if needed;
- do not ask Luba to perform routine Git repair;
- preserve owner work and all evidence;
- only a genuine external owner-only/access/security/policy blocker may leave the task blocked.

## 9. Verification literals

`EVERY TASK STARTS WITH FRESH AGENT+SKILL READS`
`EVERY ACCOMPLISHED TASK GETS ITS OWN COMMIT`
`EVERY TASK COMMIT IS PUSHED BEFORE THE NEXT TASK`
`EVERY PUSH IS REMOTE-READ-BACK BEFORE THE NEXT TASK`
`MAIN AND REMOTE DEVELOPMENT ANCESTRY ARE RECHECKED BEFORE EVERY TASK AND PUSH`
`NO UNRESOLVED CONFLICTS MAY CROSS A TASK BOUNDARY`
`NO-CODE TASK STILL COMMITS ITS DURABLE EVIDENCE/STATE DELTA`
`THE NEXT TASK CANNOT START UNTIL CURRENT TASK GIT CLOSURE IS COMPLETE`
