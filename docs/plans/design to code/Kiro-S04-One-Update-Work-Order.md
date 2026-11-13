# S04.11 — Apply the single admitted real update or resume its committed result

**Requested worker:** GPT-5.6 Luna · `xhigh`

**Kind:** live · **Goal:** Execute exactly one real optimizer update from the saved original state, or recover an update that is already committed without applying it again.

**Source pins:** target `15f2f2cb905f3e8db37103bc032678b789071f6e`; planning `f8a0bde3c596422f349bf7b87093394c8a7bdc44`. Pins are references, never reset targets.

**Dependencies:** S04.05, S04.07, S04.08, S04.09, S04.10. **Branch condition:** only when the one objective has a valid baseline disposition needs_lesson.

Read this full task plus the appropriate dispatch and current adopted AUTHORIZATION/OPERATING-POLICY. A task catalogue entry is not proof that work is missing or accepted.

## 1. Task prompt, acceptance and bounded scope

**CURRENT TASK:** Execute exactly one real optimizer update from the saved original state, or recover an update that is already committed without applying it again.

**CURRENT SOURCE / EVIDENCE:** The production coordinator can update, assess and publish in the same invocation. This task must keep that safe composition; S04.12 reads its completed measurement instead of forcing another scoring call.

**Do the following; resolve ordinary corrections internally:**

1. Read the same durable objective and classify its current phase before invoking anything. If already applied, execute only the documented recovery branch. If already decided/published, return existing evidence and go to S04.12. No new runID, baseline or pair.

2. For a genuine fresh update, reopen the real pair/manifest and original prepared baseline, accepted parent and complete applicable S02 state. Validate model/tokenizer/LoRA/topology/step identities and no stale parent change. Recheck safe disk/time/call budget for both the update bundle and required trial measurement.

3. Acquire the real server-owned legal-training lease through the configured broker and existing policy, not a synthetic permit. Require the normal pair-bound baseline journal persisted/reread before trainer entry. Confirm the trusted prepared baseline avoids a second calibration.

4. Use the admitted operational entrypoint and actual one-pair coordinator with one verified optimizer step. No tests with forced success, no test observer mutation and no alternate direct trainer path.

5. Capture exact observed pair count, actual pre/post optimizer step, policy checkpoint, optimizer/RNG/manifest hashes, lengths and linkage. The real reference policy and pre-state remain valid; finite arithmetic and complete publication validation must hold.

6. Allow the existing coordinator to complete trial evaluation and local decision within this invocation when that is its actual design. Preserve all resulting evidence for S04.12; do not stop the process at an artificial task boundary.

7. On timeout/error, inspect durable effect/commit evidence first. A known complete bundle permits trial-only recovery; an incomplete/uncertain update prohibits another training call. Ordinary code fixes/tests can proceed, but the scientific one-update budget cannot be refilled by another candidate commit.

**Acceptance:**
- Exactly one actual new real update is observed or a prior same-objective committed update is safely resumed with no new update.
- Original pair/baseline/accepted pre-state identities remain recoverable and correct.
- Post-state bundle validates against exact trial bytes, step and model topology.
- Any automatically completed trial/decision is retained rather than duplicated next task.

**Default write/operation scope; exact source delta is internally admitted:**
- Only admitted live attempt/model/state/evaluation artifacts and safe task receipts through existing production APIs.
- No source changes during the frozen invocation, no remote real-artifact publication, no fixture fault injection on real data.

**Not the task:** no earlier-slice restart, no arbitrary model/provider/data change, no extra learning update, no closed-data disclosure, no weakening tests, no remote real-artifact publication and no broad governance/runner rewrite. Engineering tests are synthetic evidence only. Live stages use the same bounded objective.

Requirement bindings: R01, R02, R03, R04, R07, R09, R10, R11, R12. One role-attributed report: `.kiro/xiigen-slices/work/S04/S04.11.md`; manager alone integrates live active/state. Preserve existing contents. The runtime production session and work report are different authorities.

## 2. Exact context and current-line resolution

| ID | Repository path / symbol | Inspected source window | Use and limitation |
|---|---|---|---|
| CO | `26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.cs` | 1–400, 529–780 | Requires one sealed pair; fresh path calibrates after receipt; same-run validates baseline and optimizer/RNG/trial bundle before trial-only evaluation. |
| EX | `14.ContinuousLearning/Training/CodeDocumentationServerOwnedDpoTrainingExecutor.cs` | 1–300 | Lease, claim, baseline write/reopen, state validation, real trainer and complete bundle ordering. |
| DX | `14.ContinuousLearning/Training/DpoTrainingExecution.cs` | 1–245 | Default aliases LogicalDpoTrainingUnit.MinimumSupportedMaxStepCount: numeric value NOT resolved here; inspect definition before claiming one-step support. |
| START | `26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedModelCheckpointStartService.cs` | 1–195 | Real accepted compatible parent required; fresh random model is not an S04 baseline. |
| BASE | `14.ContinuousLearning/Training/CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.cs` | Resolve current definition before execution; not freshly inspected here | Current full definition must be read; prior reviewed baseline state requires pair identity and cannot simply be reused before a pair exists. |
| LOGICAL | `SYMBOL ONLY: LogicalDpoTrainingUnit, MinimumSupportedMaxStepCount, LogicalDpoTrainingRequest` | Resolve current definition before execution; not freshly inspected here | Not found in the inspected 21.AgentInfrastructure/Logical directory; search actual checkout definitions/project links, not the whole archived tree. No absence or numeric-minimum claim. |
| TRAINER | `11.DPO/DpoTrainer.cs` | Resolve current definition before execution; not freshly inspected here | Actual optimizer update count, loss calculation, parameter keys, publication callbacks. Resolve exact training method name rather than trusting a guessed API. |

Also read the current admitted predecessor result, task-specific local diff and relevant runtime/test evidence. The manifest records whole-file blob identities for inspected windows; that is not a full-file-read claim. Open complete affected methods and any entire file before editing. Resolve uninspected entries to actual current paths/lines and record them in the task report. Proposed contracts/test names are not existing APIs.

The primary queue is preserved in `sources/04-QUEUE-original.md`: S04 one real lesson, S05 remote artifacts and the conditional promotion rule. Do not execute historical commands from planning transcripts.

## 3. Skills to load for each real role

| Role | Principal skill paths | Task-specific responsibility |
|---|---|---|
| architect | `docs/skills/trainable-architecture-decision-SKILL.md`; `docs/skills/phase-planning-SKILL.md`; `docs/skills/output-readability-gate-SKILL.md` | Perform the already admitted mutation-stage identity delta check and verify actual one-step contract. |
| plan-reviewer | `docs/skills/plan-review-SKILL.md`; `docs/skills/phase-planning-SKILL.md`; `docs/skills/trainable-architecture-decision-SKILL.md`; `docs/skills/output-readability-gate-SKILL.md` | Confirm all live inputs/previous effects and remaining resources; no user approval loop. |
| executor | `docs/skills/trainable-architecture-decision-SKILL.md`; `docs/skills/phase-planning-SKILL.md`; `docs/skills/output-readability-gate-SKILL.md` | Run the real coordinator or same-run recovery once as appropriate and collect durable effect evidence. |
| code-reviewer | `docs/skills/generated-code-review-SKILL.md`; `docs/skills/plan-review-SKILL.md`; `docs/skills/output-readability-gate-SKILL.md` | Verify actual state transition and no double update; do not accept from command exit or training loss alone. |

Resolve required supporting startup, intent, source-map, .NET, test-integrity and claim-verification dependencies in SKILLS.md/skills.json using current repository routing. Executor reads actual dev-safety/test-integrity requirements, not just this index. Every fresh worker records its own source/skill sections read. Adopted Luna scope replaces legacy model/pass-count defaults, not the required invariants. No four-role restart for routine manager CI/Git operations.

## 4. Positive examples

| Situation | Correct behavior |
|---|---|
| Fresh allowed lesson starts at optimizer step k | One actual increment to k+1 on pairP; complete trial bundle retained; coordinator may then evaluate and choose parent/child. |
| Earlier process applied P and saved valid trial but evaluation interrupted | Reopen B/P/T/optimizer/RNG, new trainer invocations0; finish only the allowed evaluation. |

These are examples of the intended behavior, not recorded execution results. Use real source/receipt observations when claiming that this task passed.

## 5. Negative examples and why they fail

| Tempting shortcut | Why it violates the contract |
|---|---|
| Use a new TrainingRunId after timeout | Can reapply the same pair while hiding the original effect. |
| Read request.DpoStepCount instead of observed optimizer state | Input intent is not executed step evidence. |
| Bypass broker because model is open-weight | The project's existing content/lease guards still define valid data and mutation authority. |
| Retry training after a negative measured score | The lesson is complete; a new optimization attempt is outside S04. |

## 6. How to test or verify

| Check | Arrange / action | Required observable result |
|---|---|---|
| Pre-effect barrier | Validate actual persisted B/P/parent/all-state bundle | All identities match before real trainer entry. |
| Actual mutation evidence | Read real execution metrics and state transitions | One new pair and optimizer increment1; not a routing-profile update. |
| Post-state identity | Open real policy/optimizer/RNG/manifest through production validators | Bound hash/length/step/topology; no mixed bundle. |
| Baseline preservation | Compare original prepared and pair-bound journal | Same original assessment, no extra baseline calls. |
| Recovery selection | On interrupted path inspect actual branch/call effects | Applied state yields evaluator-only resume, additional update0. |
| Accepted state safety | Read accepted lineage before quality decision or after rejection | Original unchanged until guarded valid child publication. |

This task does not get a fabricated .NET filter. Planning/readiness uses scoped artifact/preflight checks; live tasks require actual model/runtime receipts; final review consumes their actual evidence. Any necessary code correction returns to its engineering task and gate.

Use TESTING.md and the actual admitted runner. Build after source edits before `--no-build`. Required test execution is nonzero with exact expected methods/cases and no unreported platform-return/skip/abort. Keep original raw results and numeric exits. Reuse unchanged evidence only with verified relevant input identity. Live measurements are never certified by a scripted test result or class total. Counts from overlapping focused/impacted runs are not added as unique coverage.

## 7. Roles, orchestration and automatic continuation

| Role | Responsibility | Dispatch |
|---|---|---|
| architect | Perform the already admitted mutation-stage identity delta check and verify actual one-step contract. | `.kiro/xiigen-slices/luna/s04/dispatch/S04.11/architect.md` |
| plan-reviewer | Confirm all live inputs/previous effects and remaining resources; no user approval loop. | `.kiro/xiigen-slices/luna/s04/dispatch/S04.11/plan-reviewer.md` |
| executor | Run the real coordinator or same-run recovery once as appropriate and collect durable effect evidence. | `.kiro/xiigen-slices/luna/s04/dispatch/S04.11/executor.md` |
| code-reviewer | Verify actual state transition and no double update; do not accept from command exit or training loss alone. | `.kiro/xiigen-slices/luna/s04/dispatch/S04.11/code-reviewer.md` |

Use one genuine source writer or live-effect owner at a time. Architect and reviewers do not edit product source or operate live model effects. Each may append its own attributed task-report section; no fabricated worker verdict. Executor alone owns admitted source/runtime actions. The manager handles current state, selected source publication and routine CI. Ordinary allowed corrections cycle internally, no new question to the user.

For a live stage, reuse S04.08's admitted runtime plan and check only actual deltas; this is not another human approval. If the compound production call already produced the next stage's output, pass it forward rather than repeat effects. After acceptance select the next real dependency-ready task; a commit or static PASS does not terminate the objective. Worker failure and genuine external blockers follow OPERATING-POLICY.md.

## 8. Edge cases, permitted response and return packet

| Edge case | Required response without routine owner interruption |
|---|---|
| Error before trainer proven reached | Fix safe prerequisite; retry same objective only when authoritative evidence proves zero update and budget permits. |
| Error after observed optimizer increment | Do not reapply even if files incomplete; recover evidence/state or mark live attempt incomplete. |
| All bundle files present except commit identity | Presence is not atomic success; fail closed until trusted consistency validated. |
| Local policy refuses executable | Do not evade; retain safe artifacts and complete non-runtime evidence, no CI substitution with uploaded private data. |
| Training arithmetic no visible weight change | Record actual counters and identical-weight outcome; no extra step to manufacture learning. |
| Cancellation after local child published | Read existing child/decision first; no rollback to outdated parent based only on cancelled shell result. |

Use RECOVERY-PLAYBOOK.md for last-proven-durable-state decisions, CI-PLAYBOOK.md for scheduling/build/tests, and AUTHORIZATION.md for limits. These are distinct: a CI retry is not permission for another learning update. Do not resolve a hard denial by another shell/loader/token. Unknown artifact ownership is not an invitation to delete.

**Return packet:** explain the human result first, then actual role/model telemetry, grant reference, source/skill reads, requirement-to-owner-to-assertion/effect mapping, changed files/candidate identity, commands/exits/raw result paths, actual update/provider-call counts if any, selected-state/quality disposition, precise review verdict and the next permitted action. Missing evidence remains missing; no real improvement, process restart or remote publication is inferred. Report one narrow external action only when it genuinely cannot be performed under the adopted grant.
