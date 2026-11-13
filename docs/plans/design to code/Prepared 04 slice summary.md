**I prepared S04 as a complete Luna `xhigh` execution pack: 15 smaller tasks, 60 task-specific role prompts, and one bounded authorization covering implementation, verification, recovery, and the real local lesson—without asking you to approve each intermediate step.**

The scope follows your attached queue: **one real CodeDocumentation lesson, using an accepted compatible model, with the pair and original baseline safely persisted, one update, and an honest improve/revert/unchanged result.** It must not train repeatedly or fabricate improvement to obtain a successful-looking outcome. 

## Downloads

### **[Download the complete S04 Luna xhigh pack](sandbox:/mnt/data/Kiro-Xiigen-S04-Luna-xhigh-Pack.zip)**

[All tasks and role prompts in one document](sandbox:/mnt/data/Kiro-S04-Luna-xhigh-All-Tasks.md) · [Installation and startup prompt](sandbox:/mnt/data/Kiro-S04-Luna-xhigh-Start.md) · [Task roadmap](sandbox:/mnt/data/Kiro-S04-Luna-xhigh-Roadmap.md) · [Guide and validation notes](sandbox:/mnt/data/Kiro-S04-Luna-xhigh-Guide.md)

**The combined document is for your reference—not something every worker should load.** Each worker receives its own task, its role-specific prompt, the applicable authorization, relevant skills and source context, and the necessary predecessor results.

## 1. What is different from the original S03 approach

**The authorization is part of S04 from the beginning.** It does not first prohibit real training and then require you to approve it again after the implementation is finished.

Once you adopt the startup prompt, it authorizes the bounded local experiment and the ordinary engineering operations needed to complete it: source and test corrections, candidate commits, CI fixes and retries, worker replacement, real calls to the **already approved local Qwen teacher/evaluator**, and the existing guarded local model-selection decision.

There are still separate internal reviews, but **the reviewers are the agents—not you being asked to press “continue” between tasks**.

Most importantly, the pack distinguishes two very different retry loops:

| Operation                                   | What an autonomous retry may do                                                                                                     |
| ------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------- |
| **Build, CI or test verification**          | Repair the demonstrated problem and rerun the appropriate checks against the corrected candidate.                                   |
| **An actual learning update**               | First establish whether the update already happened. An applied lesson resumes evaluation; it must not train that pair again.       |
| **A completed, valid negative measurement** | Preserve the result and the correct original model state. Do not reevaluate repeatedly until a favorable score appears.             |
| **An incomplete evaluation**                | Preserve the failed attempt and request accounting, then use only the bounded, same-condition recovery allowed by the run contract. |

That distinction preserves the existing coordinator’s recovery behavior: it reopens the saved baseline, pair, optimizer, RNG and trial bundle, then calls trial evaluation without invoking the trainer again.

## 2. The 15 tasks

**These are 15 work orders, not a claim that 15 implementations are missing.** Your queue explicitly requires reconciling each candidate with current source and evidence, retaining equivalent completed work instead of rebuilding it.  

### Preparation and implementation

| Task                                             | Deliverable                                                                                                                           | Essential verification                                                                                                                                |
| ------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------- |
| **S04.00 — Admit the actual objective**          | Identify the real S04 gaps, preserve completed S01–S03 work, and bind one experiment objective.                                       | No assumed upstream acceptance, duplicate implementation or hidden previous training attempt.                                                         |
| **S04.01 — Resolve assets, runtime and limits**  | Establish the accepted model, tokenizer, complete state, local teacher/evaluator, teaching data, executable host and resource limits. | CI availability is not mistaken for local model availability; missing assets are discovered **before mutation**.                                      |
| **S04.02 — Durable lesson session**              | Bind one objective to one run, one pair slot and one update budget across restarts.                                                   | Duplicate requests, worker replacement or new sessions cannot authorize another update.                                                               |
| **S04.03 — Original baseline before authoring**  | Measure, persist and reopen the original baseline; reuse it after the teaching pair is created.                                       | No second baseline calibration, no caller-injected scores, and no unnecessary pair when the baseline already satisfies the existing target predicate. |
| **S04.04 — One teaching-only pair**              | Use the existing authoring and dataset boundaries for exactly one new pair with actual provenance.                                    | Closed exam material stays out of authoring; cache hits make no teacher calls; accepted pairs are not replayed as updates.                            |
| **S04.05 — Complete pre-update barrier**         | Validate all applicable saved state and enforce one actual optimizer update.                                                          | Observe the actual counter/effect—not merely one invocation of a method that might loop internally.                                                   |
| **S04.06 — All measured outcomes**               | Retain complete evidence for improved, regressed and indistinguishable results, with correct model selection.                         | Invalid evaluation is not labelled “unchanged”; rejection of promotion does not discard the actual measurement.                                       |
| **S04.07 — Interruption and recovery rehearsal** | Test the lifecycle at its failure boundaries using synthetic fixtures and actual production storage/training paths.                   | Applied updates are not repeated; negative tests prove they reached the intended boundary.                                                            |
| **S04.08 — Executable operational route**        | Reuse or add the smallest real entrypoint that composes the existing services; verify its dry-run and complete synthetic rehearsal.   | No unused helper accepted as integration, no invented CLI command, and no scripted evaluator wired into real mode.                                    |

### The real lesson and final acceptance

| Task                                                | Deliverable                                                                                                    | Essential verification                                                                                          |
| --------------------------------------------------- | -------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------- |
| **S04.09 — Real original baseline**                 | Run the approved evaluator against the actual accepted student and persist/reopen the complete measurement.    | Actual model/evaluator receipts, complete fixed cases and repetitions, and zero training updates.               |
| **S04.10 — Real pair authoring**                    | Create or reopen the one real Qwen-authored teaching pair.                                                     | Actual authorship and sealed bytes, one new pair, no closed-exam exposure and no weight update yet.             |
| **S04.11 — One real update or recovery**            | Apply the admitted update, or resume its already-committed result.                                             | Complete original state, one actual new optimizer update, and a matching post-update bundle.                    |
| **S04.12 — Real comparison and disposition**        | Complete or reopen the same-condition trial measurement and verify the correct improve/retain/revert decision. | No repeated complete evaluation, no second update and no promotion from invalid evidence.                       |
| **S04.13 — Reopen the selected result**             | Reconstruct dependencies and load the actual selected parent or child with its matching state.                 | The result is usable from saved artifacts, not just an object retained in memory.                               |
| **S04.14 — Independent acceptance and S05 handoff** | Join every requirement to actual implementation, tests and real-run evidence; record the honest outcome.       | No synthetic-to-real claim, no invented improvement and no accidental remote publication of model or exam data. |

### Task boundaries do not force duplicate runtime calls

The existing coordinator can **train, evaluate and locally publish within one invocation**. Consequently, S04.11 may already produce the measurement needed by S04.12.

The pack explicitly instructs S04.12 to **read and verify that result**, not invoke the coordinator or evaluator again merely because another prompt has started. The current coordinator’s fresh path already connects training, trial reopening, quality evaluation and publication.

## 3. What every task and role prompt contains

Each task has your requested eight sections:

**Task and acceptance criteria; source context; skills; positive examples; negative examples and why they fail; test or verification procedures; roles and orchestration; edge cases and the permitted next action.**

Each task also has four separately written dispatch prompts:

```text
.kiro/xiigen-slices/luna/s04/
  tasks/S04.11.md
  dispatch/S04.11/architect.md
  dispatch/S04.11/plan-reviewer.md
  dispatch/S04.11/executor.md
  dispatch/S04.11/code-reviewer.md
```

The prompts give each role a different job.

| Role                       | Responsibility                                                                                                  |
| -------------------------- | --------------------------------------------------------------------------------------------------------------- |
| **Architect**              | Resolve actual ownership, interfaces, state ordering, minimal source scope and recovery behavior.               |
| **Plan reviewer**          | Challenge the concrete plan, its prerequisites, examples, failure branches and observable acceptance checks.    |
| **Executor**               | Implement or execute the admitted work, run the checks, repair ordinary defects and return actual evidence.     |
| **Code/evidence reviewer** | Independently inspect the final source and results, verify the real behavior, and return mandatory corrections. |

**A routine GitHub status read, cancellation or artifact download does not trigger four new workers.** For the live stages, the complete runtime plan is already admitted in S04.08; later checks examine actual changes to the candidate and inputs rather than reopening the whole design.

A missing worker can be replaced. The permitted manager-as-executor fallback remains explicitly labelled. **It never becomes permission for the author to manufacture an independent final review.**

## 4. Skills and source context

The five principal skill paths are included in every task’s role mapping:

```text
docs/skills/trainable-architecture-decision-SKILL.md
docs/skills/phase-planning-SKILL.md
docs/skills/plan-review-SKILL.md
docs/skills/generated-code-review-SKILL.md
docs/skills/output-readability-gate-SKILL.md
```

I read their relevant kickoff sections at the inspected revision. The pack also routes workers to the applicable startup, intent, source-map, .NET, test-integrity and claim-verification dependencies. It does not claim that the entire skills library was loaded or audited during preparation.

The code context is anchored to:

```text
Target:
15f2f2cb905f3e8db37103bc032678b789071f6e

Planning:
f8a0bde3c596422f349bf7b87093394c8a7bdc44
```

These are **inspection references, not reset targets**. The target commit’s message concerns S03.03 evidence; it is not evidence that all S03 work is already accepted. The startup task must read the actual current predecessor results.

Examples of the inspected source windows included in the package:

| Source                                                                 | Inspected context                                                                                    |
| ---------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------- |
| `CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.cs` | Lines **1–400** and **529–780**: fresh attempt, one-pair selection and same-run recovery.            |
| `CodeDocumentationRepositoryOwnedModelCheckpointStartService.cs`       | Lines **1–195**: accepted compatible checkpoint loading and the distinct cold-start branch.          |
| `CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.cs`     | Lines **1–415**: calibration, sufficiency, comparison and promotion conditions.                      |
| `CodeDocumentationServerOwnedDpoTrainingExecutor.cs`                   | Lines **1–300**: lease, baseline persistence, training and post-state validation.                    |
| `CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.cs`               | Lines **240–570**: request preparation, cache-first authoring, permits, provenance and pair binding. |
| `DpoTrainingExecution.cs`                                              | Lines **1–245**: actual logical training composition and state-file handoff.                         |

Secondary definitions whose current locations or complete bodies were not inspected are **explicitly marked for local resolution**. They do not get invented line numbers.

## 5. Two source-level issues are handled explicitly

### Baseline before pair authoring—without measuring it twice

The active training program uses the original assessment to decide whether a pair is needed and what teaching context is appropriate. But the inspected coordinator currently takes the sealed pair before it performs fresh baseline calibration.

S04.03 therefore must reconcile that ordering through an existing trusted prepared-baseline mechanism—or the smallest necessary addition.

It must **not** solve this by authoring an unnecessary pair first, calibrating twice, or accepting arbitrary scores from a CLI argument.

### Non-improvement must retain actual measurements

The inspected quality service produces detailed evidence for accepted improvement, while other comparisons can leave through a generic `NotImproved` error.

S04.06 checks what the current post-S03 implementation already preserves. Where necessary, it adds the narrow evidence path needed to retain a **complete valid negative comparison**, while preserving the refusal to promote that model.

This is not permission to turn every error into a successful “unchanged” outcome.

## 6. Example: the real-update work order

**[Read the full S04.11 prompt](sandbox:/mnt/data/Kiro-S04-One-Update-Work-Order.md)**

Its positive case is:

```text
Accepted parent A and complete compatible pre-state
+ original baseline B, already saved and reopened
+ one new sealed pair P
+ one stable objective and TrainingRunId

→ validate all identities
→ persist and reopen the pair-bound baseline journal
→ acquire the actual training lease
→ apply one actual optimizer update
→ validate matching trial weights, optimizer, RNG and manifest
→ complete the existing evaluation/selection route
→ preserve the actual result
```

The expected observations include the actual optimizer counter transition, pair identity, baseline identity and matching state-bundle hashes—not only a successful method return.

Its recovery case is:

```text
The same objective already has a valid applied trial bundle
→ reopen and validate that bundle and the original baseline
→ perform only the permitted unfinished trial evaluation
→ additional optimizer updates = 0
```

The task rejects these shortcuts:

| Incorrect action                                                              | Why it fails                                                                                        |
| ----------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------- |
| Allocate another `TrainingRunId` after a timeout.                             | May apply the same lesson again while hiding the first attempt.                                     |
| Count one `TrainAndPublishAsync` invocation as proof of one optimizer update. | The actual implementation may perform multiple internal steps; the counter/effect must be verified. |
| Replace missing optimizer state with a fresh optimizer.                       | Missing continuation state is not the same as a legitimate first-ever initialization.               |
| Repeat training after a complete negative result.                             | Changes the experiment from one lesson into unrecorded optimization until success.                  |
| Use a routing score or reduced training loss as the quality verdict.          | Neither is the required independent saved-model task assessment.                                    |

The distinction from the bulk routing runner is important: that runner explicitly works with **50 training and 50 evaluation examples and routing-profile scores**. It is not automatically the one-lesson saved-model execution route.

## 7. What happens without your intervention

The pack includes both a **CI playbook** and a **durable-effect recovery playbook**.

For ordinary compiler, fixture, workflow, discovery or validator errors, the executor diagnoses the cause, fixes the limited owning code, reruns the affected checks and obtains the required separate review. It does not ask you whether it may correct a method.

For a lost worker, the manager establishes whether it still owns a process or source edit before assigning a replacement. There must not be two writers or two model-update operations competing over the same state.

For a missing runtime result, the manager first reads the actual saved state. **A failed command is not proof that no update happened.**

For a complete negative comparison, the agent records the outcome and verifies the exact correct retained state. **It does not reinterpret your request for autonomy as permission to keep training.**

There are still genuine limits: a prompt cannot supply missing private model bytes, an unavailable executable host, unknown credentials or an exhausted budget. The package requires discovering those inputs early, completing safe independent work and reporting the **one exact remaining external action**, rather than creating another chain of permission documents.

### Operational limits included in the startup grant

These are **proposed operating defaults**, not numbers stated by the queue:

| Limit                                                  | Default                                                                                                                                         |
| ------------------------------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------- |
| Real learning objective                                | **One**, preserved across sessions and retries.                                                                                                 |
| New sealed pair                                        | **At most one.**                                                                                                                                |
| Actual new optimizer update                            | **At most one.**                                                                                                                                |
| Authoring requests                                     | At most three, only for bounded protocol/transport recovery before sealing; no selection among several valid candidates.                        |
| Evaluation retries                                     | Only for genuinely incomplete or invalid series; no retry of a complete valid result to improve its score.                                      |
| Cumulative evaluation allowance                        | At most `4 × N × R`, subject to stricter existing limits; each successful baseline/trial series retains its full required `N × R` observations. |
| Real supervised runtime                                | Four hours cumulatively for this objective, not reset by restarting.                                                                            |
| GitHub Actions                                         | **Remaining balance of the existing shared $20 allowance**, not another $20 for S04.                                                            |
| New cloud model spending or remote weights publication | **Not included.**                                                                                                                               |

The actual model configuration, case count, repeat policy and tighter existing limits are resolved **before** real effects—not guessed midway through the run.

## 8. Installation and the one startup message

Merge only:

```text
overlay/.kiro/xiigen-slices/luna/s04/
```

into the existing repository. The package does not replace S03, the current `active.md`/`state.json`, your original skills, application source, or model artifacts.

Select **GPT‑5.6 Luna → `xhigh`** in Kiro’s native controls. The inspected Kiro documentation lists `xhigh` for Luna and exposes effort selection in IDE/CLI; writing it inside a prompt alone does not establish the runtime setting. ([Kiro][1])

Then send:

```text
Use .kiro/xiigen-slices/luna/s04/START-PROMPT.md as my current work order.

I adopt its AUTHORIZATION.md and operating/run/recovery policies for S04.

This grants the bounded engineering and CI work and one real local
CodeDocumentation lesson using the existing approved Qwen teacher/evaluator:
at most one new pair, one optimizer update, fixed-condition evaluation,
and guarded local improve/retain/revert disposition.

No middle human approvals are required for covered operations.
Separate internal plan/code/evidence reviews remain mandatory.

Carry the remaining shared USD20 Actions budget; this does not refill it.
No new paid model service, private-data upload, security bypass or remote
weights/pair publication is authorized.

Preserve completed S00–S03 and current work. Start from RESUME.md and
the earliest actual unmet requirement, not from a forced restart.
Read each selected task and its four role-specific prompts.

Complete engineering readiness before real calls. Keep one durable
objective, original baseline and pair across restarts. An applied update
resumes only evaluation; a complete valid negative result is not retried
until green.

If the coordinator already evaluated or published during the update call,
verify that existing result instead of repeating it in the next task.

Proceed through S04.14 and prepare the original S05 handoff.
Do not stop merely after installing the pack or obtaining a static PASS.
```

## Validation performed

The written package passed **408 structural checks** covering its 15 task briefs, 60 role prompts, eight-section structure, dependency graph, requirement mapping, source classifications, JSON and Markdown structure. The original attached queue was preserved byte-for-byte, and the ZIP contents and per-file checksums were verified.

**These are package checks—not application tests or independent execution review.** I did not change the repository, dispatch CI, call a teacher/evaluator, execute a real lesson or benchmark Luna while preparing this pack.

**The result is a complete S04 work chain with the real-run authorization and failure handling included from the start. It preserves your requirement to work autonomously without turning “finish the task” into “keep changing the experiment until the model improves.”**

[1]: https://kiro.dev/docs/models/effort/ "https://kiro.dev/docs/models/effort/"

Please remember to use gpt 5.6 luna extra high for sub agents
