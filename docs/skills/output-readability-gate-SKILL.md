# Output Readability Gate

## Luba Why / Loophole Repair-First Rule

When Luba asks "why" (or the equivalent in the operator's language), challenges an answer, or says a failure came
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

## Fresh Execution / Method / Humility Readability Hook

Human-readable output must make these gates visible:
- `REPEATED_CURRENT_COMMAND_IS_FRESH_EXECUTION_GATE`: repeated current command is a fresh execution trigger; old artifacts are baseline evidence, not completion.
- `TOO_FAST_HIGH_PASS_SUSPICION_GATE`: too-fast 50-pass completion is suspicious until per-pass sub-agent evidence is accepted.
- `LARGE_VOLUME_MODEL_HUMILITY_AND_FOCUS_GATE`: large-volume model humility requires bounded slices and focus ledger before completion.
- `EXPLICIT_METHOD_OVERRIDE_GATE`: Luba's method/model/reasoning/count/folders/target/sub-agent route is binding; output must not hide a method gap behind content evidence.
- `FINAL_ANSWER_FRESHNESS_GATE`: final/status must plainly say freshly executed this turn vs only verified and answer the latest command.
- `CURRENT_REPEATED_PACKAGE_COMMAND_GOVERNANCE_ADDENDUM`: readable output must show repeated extension-development package work as a fresh execution chain, not old artifact verification; reject too-fast N-pass/status-package claims without ordered packets/records; show large-volume model humility through a focus ledger with exact current task, not-the-task, files, pass/review counts, active worker, remaining actions, and latest-command freshness; state one active worker at a time when Luba flags parallel/too-fast or exact-count sub-agent work unless her current unquoted message authorizes parallelism; flag interrupted or zero-work spawned-worker packets using `NONE` instead of `subagent_id: UNKNOWN_IN_SUBAGENT` when platform id is hidden; preserve the parent local sidecar work ban; put human-readable phase counts before execution continuation; and keep final/status freshness tied to latest command, fresh-vs-old execution, actual packets/records, no parent self-review counted, and no product-readiness overclaim.
- `COMMAND_SCOPE_EXACTNESS_GATE`: readable output must flag current-task command violations. Current `allowed_commands` and `forbidden_commands` are binding; read-only convenience commands, alternate git diff modes, external AI/web, `codex.exe`, nested sub-agents, branch/git mutations, or shell variants outside the current allowed list are violations unless Luba's current unquoted instruction explicitly authorizes them. Evidence from an out-of-scope command is quarantine evidence only and cannot support completion.

## Sub-Agent Work-Order Completeness Gate

Every bounded sub-agent work order must include explicit skill-load guidance and the complete schema below. A generic prompt that only names a target file and broad instruction is invalid. If a sub-agent receives a generic prompt, the parent must interrupt and replace it before accepting work.

Required skill load guidance for DPO/DynamicGraph plan repair:

```text
SK-SESSION: planning-session-startup-SKILL.md + SESSION-START-PROMPT.md + AGENTS.md
SK-INTENT: architect-behavior-classifier-SKILL.md
SK-PLAN: authority-requirement-binding-SKILL.md + reference-inventory-and-source-map-SKILL.md + phase-planning-SKILL.md
SK-REVIEW: plan-review-SKILL.md + goal-delivery-completeness-SKILL.md + claim-verification-SKILL.md + audit-protocol-SKILL.md + specificity-calibration-SKILL.md + output-skepticism-SKILL.md + reconnaissance-gate-SKILL.md
SK-ARCH: trainable-architecture-decision-SKILL.md + no-product-decisions-SKILL.md + output-readability-gate-SKILL.md
SK-DOC/PARTA when plan touches existing code: implementation-doc-uml-SKILL.md + implementation-doc-algorithms-SKILL.md + implementation-doc-code-refs-SKILL.md + implementation-doc-status-SKILL.md + implementation-doc-hierarchy-SKILL.md
```

If a work order lacks this explicit skill block or an equivalent current-task-specific skill block, it is invalid and must be interrupted/replaced.

```text
CURRENT TASK:
NOT THE TASK:
EVIDENCE-ONLY:
required_skills_and_gates:
context_files_to_read:
implementation_or_artifact_details:
original_task_requirements_to_cover:
mental_simulation_required:
good_examples:
bad_examples:
allowed_files:
forbidden_files:
allowed_commands:
forbidden_commands:
verification_required:
return_packet_schema:
claim_scope/evidence_scope/count_claimed/independent_evidence_count/subagent_id_or_NONE/downgrade_needed:
```

Good sub-agent prompt: names exact skills, exact attachments, exact active files, exact requirements, good/bad examples, pass count, nonparallel rule, verification commands, return schema, and no git authority unless explicitly needed. Bad sub-agent prompt: `Fix active-codex plan readiness` or `Search NOT_READY and fix wording` without skills, task attachments, examples, coverage matrix, and simulation gates.

## Strict Plan Readiness / Original Task Simulation / Future Gate / Historical Status Gate

PLAN_READY means zero unresolved design/dependency blockers and complete coverage of the original task. The plan must be mentally simulated as if implemented and must cover every original-task requirement. Future Gate B may contain execution actions such as tests/builds/screenshots/mergeability checks. Future Gate B may NOT hide unchosen legal provider, missing input/output contracts, missing dual-path design, unresolved repo ownership, missing external API seam design, missing provenance policy, missing thresholds, missing model/checkpoint lifecycle, or missing source-to-plan mapping. If a future gate hides a design/dependency decision, verdict is PLAN_NOT_READY.

30 sequential simulation/review passes are required for this readiness class: one pass at a time, no parallelism. Each pass records `requested_model: gpt-5.5`, `requested_reasoning_effort: xhigh`, skill focus, original-task requirement checked, plan section checked, mental simulation step, coverage verdict, finding, repair/no-fix reason, and recheck result. A single worker doing 30 internal passes must call them sequential simulation/review passes, not 30 sub-agents or 30 independent sub-agent passes.

Future gates are execution-continuation checkpoints, not stop permissions. After Luba approves a truly ready plan, gates are executed through bounded sub-agents until the product is complete. If a gate fails, the next bounded sub-agent fixes/mitigates or records a true external blocker. Do not stop/finalize while safe bounded work remains.

Any historical/pre-repair/evidence-only `NOT_READY`, `BLOCKED`, `FAILED`, `READY`, `DONE`, or similar status retained as evidence must be labelled historical/pre-repair/evidence-only, with the current verdict nearby. If a current artifact still makes Luba ask whether stale `NOT_READY` remains, the repair is incomplete.

Good future gate: `Run dotnet tests after implementation; if red, bounded fix worker repairs failing slice.` This is execution, not a design decision. Bad future gate: `Choose legal provider during implementation` or `figure out FLOW-38 input contract later`; these are design blockers, not execution gates. Good historical status: `Attachment 429f reported the pre-repair plan as NOT_READY; current verdict after repair is PLAN_NOT_READY because provider remains unchosen.` Bad historical status: leaving `NOT_READY` in a source map without saying historical/pre-repair and without current verdict next to it.

## Sub-Agent Work-Order Completeness Gate

Every bounded sub-agent work order must include:

```text
CURRENT TASK:
NOT THE TASK:
EVIDENCE-ONLY:
required_skills_and_gates:
context_files_to_read:
implementation_or_artifact_details:
good_examples:
bad_examples:
allowed_files:
forbidden_files:
allowed_commands:
forbidden_commands:
verification_required:
return_packet_schema:
claim_scope/evidence_scope/count_claimed/independent_evidence_count/subagent_id_or_NONE/downgrade_needed:
```

A generic prompt that only names a target file and broad instruction is invalid.
If a sub-agent receives a generic prompt, the parent must interrupt and replace
it before accepting work. Good sub-agent prompt: names exact files, exact
skills, exact accepted context, exact implementation/design details, exact good
examples, exact bad examples, exact verification commands, exact return schema,
and no git authority unless explicitly needed. Bad sub-agent prompt: `Fix
active-codex plan readiness` with no skills, context, examples, or verification.

## Strict Plan Readiness / Future Gate / Historical Status Gate

PLAN_READY means zero unresolved design/dependency blockers. Design/dependency
decisions must be solved inside the plan before implementation. Future Gate B
may contain execution actions such as tests/builds/screenshots/mergeability
checks. Future Gate B may NOT hide unchosen legal provider, missing input/output
contracts, missing dual-path design, unresolved repo ownership, missing external
API seam design, missing provenance policy, missing thresholds, or missing
model/checkpoint lifecycle. If a future gate hides a design/dependency decision,
verdict is PLAN_NOT_READY, not PLAN_READY_FOR_LUBA_REVIEW.

Future gates are execution-continuation checkpoints, not stop permissions. After
Luba approves a truly ready plan, gates are executed through bounded sub-agents
until the product is complete. If a gate fails, the next bounded sub-agent
fixes/mitigates or records a true external blocker. Do not stop/finalize while
safe bounded work remains.

Any historical/pre-repair/evidence-only `NOT_READY`, `BLOCKED`, `FAILED`,
`READY`, `DONE`, or similar status retained as evidence must be labelled
historical/pre-repair/evidence-only, with the current verdict nearby. If a
current artifact still makes Luba ask whether stale `NOT_READY` remains, the
repair is incomplete.

Good future gate: `Run dotnet tests after implementation; if red, bounded fix
worker repairs failing slice.` This is execution, not a design decision. Bad
future gate: `Choose legal provider during implementation` or `figure out
FLOW-38 input contract later`; these are design blockers, not execution gates.
Good historical status: `Attachment 429f reported the pre-repair plan as
NOT_READY; current verdict after repair is PLAN_NOT_READY because provider
remains unchosen.` Bad historical status: leaving `NOT_READY` in a source map
without saying historical/pre-repair and without current verdict next to it.

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

## Core Problem

Generated output that contains shortcodes, status strings, or code-style
identifiers written as plain prose is unreadable to a human reviewer. The
same token that belongs in a code fence — `CF-12`, `CONTEXT_INSUFFICIENT`,
`Rule 1` — becomes noise when scattered through a paragraph. This skill
defines the four prohibited patterns, the exception for code fences, and
the gate check that must pass before any generated output is accepted.

---

## When to Invoke

- Before accepting any output from a generation node (ai-generate, multi-generate)
- When reviewing context package prose sections (not code sections)
- When writing plan documents, skill files, or decision records
- When a human reviewer reports that output is "hard to follow" or "reads like code"
- When writing project documentation, dashboard/API docs, API catalogs, or user-facing
  examples.

---

## Section 1 — The Core Rule

```
No shortcodes, status strings, or code-style identifiers may appear
in prose outside of triple-backtick code fences.

Exception: inside ``` code fences, any identifier is allowed.
```

This rule applies to ALL prose — plan text, skill descriptions, decision
records, comments, and AI-generated explanations. It does NOT apply to
code blocks, inline code spans with backticks, or table cells that display
code values.

---

## Section 2 — Four Prohibited Patterns

### Pattern 1 — LETTER(S)-DASH-NUMBER(S) identifiers in prose

```
Prohibited: CF-12, DNA-5, OP-7, BUG-ENGINE-001, D-01, LEARNING-001
Reason: Shortcode syntax belongs in reference tables and code, not sentences.
Allowed form: "Capability Framework Rule 12 (CF-12)" on first use; "CF-12" after.
              Or: write out the rule in full and cite the identifier in parentheses.
```

### Pattern 2 — Bare letter+number tokens in prose

```
Prohibited: R1, G3, Q2, T4, E1 (when used as identifier shortcuts in prose)
Reason: Ambiguous without expansion — R1 could mean Rule 1, Requirement 1,
        or Round 1 depending on context.
Allowed form: write the full term first: "Round 1 (R1)" or "Rule 1" without abbreviation.
```

### Pattern 3 — ALL_CAPS_UNDERSCORE status strings in prose sentences

```
Prohibited: "The pipeline returned CONTEXT_INSUFFICIENT so we escalated."
            "When the result is DEFERRED_CONSTRAINT the session may close."
Reason: Status strings written as prose identifiers are not English sentences.
Allowed form: "The pipeline returned a context-insufficient result, so we escalated."
              Or: use a code fence to reference the status value.
              Or: write: the `CONTEXT_INSUFFICIENT` signal (inline backtick is allowed).
```

### Pattern 4 — Version suffix codes in prose

```
Prohibited: "The fix was applied in v23.0+hotfix-2 of the context package."
Reason: Version identifiers belong in changelogs and code metadata, not description prose.
Allowed form: "The fix was applied in version 23.0 (hotfix 2) of the context package."
```

---

## Section 3 — .NET Project Equivalents

The same four patterns apply specifically to this project's identifiers:

```
Ban CF-N alone in prose sentences
  → Write "Capability Framework Rule N" or use inline backtick `CF-N`

Ban "Rule N" alone without a subject
  → Write "CLAUDE.md Rule N" or "DNA Rule N (no external ML frameworks)"

Ban CLAUDE.md rule shortcodes as prose adjectives
  → Not: "This is a Rule 1 violation in the return type."
  → Yes: "This violates Rule 1 (OperationResult<T> everywhere) — the method returns void."

Ban OperationResult status strings as prose
  → Not: "The method returned Fail with MISSING_CAPABILITY."
  → Yes: "The method returned a failure result with error code `MISSING_CAPABILITY`."

Ban pipeline signal type names as prose descriptions
  → Not: "The feedback node emits OUTCOME and DPO_TRIPLE."
  → Yes: "The feedback node emits an outcome signal and a DPO (Direct Preference Optimization) triple."
         (or use code fence / inline backtick for signal type names)
```

---

## Section 4 — Gate Check

Before accepting any prose output from a generation node:

```
□ No LETTER(S)-DASH-NUMBER(S) identifiers outside backticks or code fences
□ No bare letter+number tokens (R1, G3, Q2) used as identifier shortcuts in prose
□ No ALL_CAPS_UNDERSCORE status strings written as prose words
□ No version suffix codes in descriptive sentences
```

If any checkbox fails: return the output to the generation node with the
specific pattern identified. Do not rewrite the prose manually — instruct
the generator to rewrite the failing sentence using the allowed form.

## Section 5 — Dashboard And API Documentation Readability

Dashboard/API documentation is accepted only when a non-implementer can answer:
"what API is this?", "what do I type?", "what will I see?", and "what does this
evidence prove?" without reading source code first.

Required for each exposed API method:

```
□ Human name, not only method id
□ One realistic example input
□ Main parameters in plain language
□ Expected output in plain language
□ Evidence boundary: what the result proves and what it does not prove
```

Required for dashboard/project docs:

```
□ UML or flow diagram is accompanied by examples/simulations for the edges
□ No-code algorithms name the actual class and method/function
□ The text explains parameters and outputs before showing code quotations
□ Code quotations are short and tied to a "why this matters" explanation
□ Test evidence is separated by scope: unit, integration, view-model, full UI,
  host runtime, semantic/model readiness
□ UX automation evidence is separated from click-path UI automation: labels,
  accessible names/help text, examples, error wording, raw/debug safety,
  history/copy/export clarity, layout/bounds, and no internal primary-path jargon
□ No mojibake or broken encoding marker names remain in human-facing prose
□ `human-readable operator-language text must not contain mojibake`
□ `rendered/readback operator-language text must be human-readable, not only marker-clean`
□ `mojibake scan must include common UTF-8/Windows-1252 corruption markers and human readback of operator-language sections`
□ No code-style status noise replaces a human explanation
```

For operator-language sections, the gate fails if marker-name scans find Latin mojibake
bytes, CP1252/UTF-8 artifacts, or U+FFFD replacement characters in changed
active/governance files. Repair and read back the operator-language section before
final/status.

For any newly edited human-readable section, a worker cannot claim
`no mojibake` from a blind command, summary, or marker-only scan. The worker
must read back the exact changed lines in terminal/tool output and verify no
U+00D0, U+00D1, U+00C2, U+00E2, U+FFFD, or visibly broken encoding remains in
those lines. If parent readback finds mojibake in the changed section, the
packet is downgraded and recovery is mandatory before accepting the pass.
For current overlay/status paragraphs or current visible table/readback
sections, inherited or adjacent historical text is part of the visible
clean-readback scope unless the packet explicitly excludes it with a tight line
range.

For the API dashboard specifically, this gate must reject any document that:

- lists only technical method ids without human names;
- shows examples without explaining the expected output;
- calls view-model automation "full UI" evidence;
- calls click-path UI automation "UX automation" evidence;
- calls dashboard-to-core invocation "Claude/Cursor/Codex runtime" evidence;
- hides external AI, host runtime, semantic-quality, or trained-model boundaries.

## Section 6 — Architecture Audit Readability

Architecture, trainable, code-review, documentation, and audit artifacts must
also pass the Human-Readable Architecture Artifact Gate:

```
□ Primary explanation is in the operator's language unless Luba asked otherwise
□ Machine labels are secondary, not the explanation
□ UML is PlantUML or Mermaid classDiagram importable to draw.io
□ Data-flow simulations and expected branch outcomes are present
□ Algorithms are explained in no-code human language
□ Code quotes have exact file:line refs and short quotes
□ Every problem quote has an adjacent concrete solution
□ Concrete Repair Blueprint Gate is filled for dictionaries/counters/KNN/scorers/private selectors
□ No table-only, shortcode-heavy, English-only, schema-first, schema-only, or prompt/spec artifact
```

"Create schema", "chosen/rejected/quarantine", "wrap in ILogicalUnit", "add KNN",
or "make typed state" without concrete classes/records/methods/tests/data rows
is not a solution. Return the artifact to governance/authoring repair before
accepting it.

When Luba asks for iterative audit readability improvement, the readability
ledger must show minimum 6 cycles, preferably 10, with the skill/gate applied,
failure, fix, and why readability improved in every cycle.

---

## Section 7 — Audit Sufficiency Wording

Readable architecture/trainable audit prose must not overclaim completion.

Gate check:

```
□ "v1 scope completed" names the exact bounded scope
□ "Architecture audit sufficient/complete" appears only after green breadth challenge
□ "enough", "done", or "closed" does not describe a narrow audit as broad
□ Breadth challenge covers confidence claims, export accuracy claims,
  target-label leakage, runtime routing confidence floors,
  source/path/provenance leakage, and fallback-to-first-entry/default-candidate shortcuts
□ Parent review asks: "What would a broader audit likely find that this artifact missed?"
□ Later same-domain BLOCKING/HIGH findings create a Delta-Severity Feedback Gate record
```

If any row fails, the output is not readable enough for an architect because
the confidence boundary is false or missing. Return it to governance/review
repair before accepting the artifact.

---

## Section 8 — Sub-Agent Firewall Report Readability

Reports about sub-agent-only repair must make the role boundary visible in
plain language. A readable report cannot hide parent sidecar work behind
phrases such as "I verified locally", "while the agent worked", or "read-only
inspection" without saying when the inspection happened.

Gate check:

```
□ The report names the bounded sub-agent id that authored the changed files
□ The report lists exact changed files
□ The report says parent did not read/analyze governed files before packet return
□ Read-only verification is described as after-return verification
□ The report names the no local non-overlapping work escape
□ Exact files, not broad folder grep, prove the firewall repair
□ Sub-agent waiting is not described as a pause or completion boundary
□ Any final/stop before the sub-agent-required deliverable is complete is
  labelled a violation unless GOLD GATE asked for answer-only or stop
```

If any row fails, the report is not readable enough for Luba to verify whether
the parent stayed architect-only. Return it to review/packaging repair.

Parent Architect Subagent-Only Firewall compatibility: readable reports must
say that parent local sidecar work is forbidden, read-only verification is
allowed only after a sub-agent packet returns, the no local non-overlapping work
escape is disabled, and sub-agent waiting is not a stop.

## Section 9 — Human-Readable Plan/Execution Log Gate

Every phase packet, plan review packet, execution review packet, and completion
packet must be readable as a primary human status, not only as machine state.

Required human-readable fields:

```text
stage:
total_major_plan_blocks_or_current_phase_tasks:
completed_count_and_names:
current_plan_step:
current_active_task_and_owner:
remaining_count_and_names:
current_result_type: code | artifact | review | policy | evidence-only | commit boundary
what_changed:
why_question_root_cause_repair_trigger:
what_was_wrong:
files_or_rules_changed:
what_resumes_next:
what_remains:
claims_opened:
claims_closed:
claims_still_closed_and_why:
why_this_status_is_true:
next_required_action:
```

Dual-Path Same-Scenario UI Proof status rule: when visual evidence or host UI
proof is in scope, the status must say that the same scenario twice is required,
which scenario is being repeated through UI, and how many paths are complete:
zero of two, automated only, UI only, or both joined. It must explain the
automated path and real host UI path comparison in human language.
JSON/log/marker screenshot is not UI proof, and a status that only points to
JSON, logs, markers, debug artifacts, or host-open screenshots is not readable
proof of UI execution.
Readable status must use `Dual-Path Same-Scenario Coverage`, not
`product-ready count = 0 because full host UI not proven` or equivalent old
full-live-host wording. If Path B cannot complete, the status must name the
exact local blocker, the next scenario/product fix, and the next bounded action.

Host Session Auth Preflight and Browser Login Recovery status rule: when a
Cursor, Claude, Codex, or other signed-in host UI scenario is in scope, the
status must say whether the host session is signed in, anonymous, wrong account,
or unknown before the UI proof can count. If Cursor opens anonymously, the
status must name browser login recovery as the next action before the same
scenario twice run continues. The readable packet must include the preflight
status `SIGNED_IN_READY`, `LOGIN_RECOVERY_REQUIRED`, `HUMAN_LOGIN_REQUIRED`, or
`AUTH_BLOCKED_WITH_EVIDENCE`, visible signed-in/not-signed-in proof, correlation
to the following UI scenario, and the phrase `no secrets captured`.

Multi-Monitor Target Window Control Protocol readability rule: when a Cursor,
Claude, Codex, or other desktop host UI scenario sends keyboard or mouse input,
the visible packet must say whether target-window input focus is proven before
input. Required literals: Multi-Monitor Target Window Control Protocol; global
foreground alone is not enough; Claude on another monitor is not itself a
blocker; do not send scenario keystrokes unless target-window input focus is
proven; target-window input focus is proven; TARGET_WINDOW_CONTROL_READY;
TARGET_WINDOW_CONTROL_PROTOCOL_MISSING; TARGET_WINDOW_INPUT_NOT_VERIFIED;
TARGET_WINDOW_CONTROL_BLOCKED_WITH_EVIDENCE; HOST_FOREGROUND_NOT_VERIFIED; do
not bypass by moving UI proof to the end.
Required literal line: do not send scenario keystrokes unless target-window input focus is proven.
Required exact phrase: global foreground alone is not enough
Required exact phrase: Claude on another monitor is not itself a blocker
Required exact phrase: target-window input focus is proven
Required exact phrase: do not bypass by moving UI proof to the end
The packet must include `monitor_count`, `target_window_title`, `target_pid`,
`target_window_handle_if_available`, `target_window_bounds`,
`target_monitor_id_or_bounds`, `activation_method_used`, `input_focus_probe`,
`cursor_ui_reaction_observed`, and `status`. The readable status must explain
that global foreground alone is not enough and that Claude on another monitor
is not itself a blocker. `HOST_FOREGROUND_NOT_VERIFIED` is readable only as a
post-protocol status after target-window activation/input verification fails
with evidence. The packet must not infer auth/session/panel state until
target-window input focus is proven, must not replace real UI control with
JSON/log screenshot, must not require human foregrounding before trying
target-window/multi-monitor control, and must not bypass by moving UI proof to
the end.

No-Blind-Spot UI Scenario Script readability rule: a UI scenario status must
say the scenario id, purpose, required qa/testing skills, exact launch and
preflight steps, Multi-Monitor Target Window Control Protocol before host input, exact
auth/session branches, exact clicks/typing/expected UI state, exact prompt/task,
screenshot/video capture moments, Path A comparison, status meanings, and next
smallest action for every common branch. agent judgment is not a scenario step.
Before execution, the visible packet must show 50-Pass Scenario Clarity Review
status with a human-readable 50-check ledger or the status remains
plan-not-ready.

Plan-Not-Ready Is Repair Loop, Not Stop readability rule: plan not ready is not a stop. The visible status must say do not execute an unready plan, name the
gap, return to architecture/planning repair loop with required skills/context
and subagents when required, and resume the nearest authorized execution only
after plan-ready criteria pass.

Executor-readiness visibility rule: do not stop is not executor authorization.
If the plan is not closed/readable/exact or the product-readiness plan gate has
not passed, the readable message must say plan-not-ready returns to planning
repair, not execution. If a sub-agent/executor was started too early, the
readable message must say the premature executor must be stopped and replaced
by bounded governance repair, then the plan-readiness workflow resumes. Before
any execution continuation is visible as allowed, the message must include
human-readable phase counts before execution continuation: total phases/steps,
done, in-progress, remaining, and plain-language phase/step meaning before any
QSG/row/state/machine label.

Dual-path readiness readability rule: for extension/runtime/host UI work, the
readable message must use these exact meanings before any acronym or packet id:
`Dual-path same-scenario coverage is product readiness evidence`;
`full live host UI path is not the readiness definition unless explicitly re-approved by Luba`;
`same scenario Path A plus Path B`;
`NOT READY is a bounded sub-agent work queue, not a stop`;
`human-readable phase counts before execution continuation`. If the status is
NOT READY, not proven, blocked, needs fix, or "full live host UI path not
proven", the message must name the exact next bounded sub-agent action or the
sub-agent-proven external blocker and mitigation attempt.

Scenario prep readability rule: Scenario prep NEEDS_FIX blocks executor work
order. A status is not readable if it says "run scenario", "test host UI", or
"continue execution" while the scenario prep review is `NEEDS_FIX`. The readable
status must say that plan not closed/readable/exact is repair loop, not
execution and not stop, then name the missing prep rows in human language:
human-readable operator-language scenario prep, exact phase/step meaning, exact app/window
target, login/session preflight, exact clicks/typing/expected visible state,
screenshot/video evidence moments, monitor/window-overlap fallback, `no secrets
captured`, automated Path A exact commands or exact callable script entrypoints,
expected output, and exact evidence filenames. The next action must be prep
repair plus independent prep review PASS, not an executor run command.

Bad -> good example from the Row20 prep incident:

```text
Bad: "Run Row20 scenario / test host UI. Executor can continue; prep review is NEEDS_FIX."
Good: "Row20 does not run yet: scenario prep is not closed and not readable for Luba. We need to add the step description in the operator's language, exact Path A commands, exact clicks/typing/expected visible state for the UI, evidence file names, and hand the prep to an independent PASS-review. This is a repair loop, not execution and not stop."
```
Required scenario prep literals: `Scenario prep NEEDS_FIX blocks executor work order`; `human-readable operator-language scenario prep`; `exact clicks/typing/expected visible state`; `plan not closed/readable/exact is repair loop`.

Why Question Root-Cause Repair Trigger readability rule: for any "why/why did
you stop/why did this happen/pochemu" failure-cause response, why is not
a stop. The visible status must answer in human language, name the exact
missing/weak/contradictory skill/guide/agent-file rule, say that the agent must
repair skills/guides/agent files before continuation through the active role
boundary, list concrete files/rules changed, and explain human language then
continue the nearest safe already-authorized work. Do not present protocol
repair complete as a final report, do not claim readiness until the exact
artifact/work chain resumes, and say when sub-agent waiting is the next
non-stop orchestration step.

Stop-loophole human-readable status repair (2026-06-28): every report that says
`NOT READY`, blocker, failed live host proof, impossible UI surface, missing
evidence, or old product-readiness wording must immediately translate that
state into plain operator-language continuation wording. Required order before any
machine label: total phases/steps, done, in progress, remaining, what the
current phase solves, what the current step solves, exact evidence present,
exact evidence not proven, and next bounded sub-agent action. `NOT READY is a
bounded sub-agent work queue, not a stop`.

The old host-only readiness sentence `user in host UI -> extension -> core ->
answer in host UI` is readable only as optional bonus/live proof. The readable
primary gate is `Dual-path same-scenario coverage is product readiness
evidence`: same scenario Path A automated/API/core/adapter evidence plus Path B
real local host UI evidence, or exact Path B blocker with mitigation and next
bounded work order.
Required phrase: why is not a stop.
Required phrase: explain human language then continue.

Machine-only JSON, ledger rows, hidden logs, or status codes may be attached as
evidence, but they cannot be the primary explanation for Luba. If these fields
are absent, the packet fails readability even when the machine evidence is
valid.

`ROUTINE_PROGRESS_COMPACT_EVIDENCE_GATE`: ordinary commentary heartbeat/progress
updates are not worker packets. They must open with a short human paragraph or
compact human board and may omit the full claim/evidence schema. If evidence
fields appear in routine commentary, keep them to one short `Evidence:` sentence
or at most two short support lines. Full claim/evidence schema belongs to worker
return packets, acceptance/rejection reviews, final summaries, and direct
audit/evidence answers. A report that formally starts with `Phase...` but then
dumps a long raw evidence block is still `STATUS_FORMAT_HUMAN_GATE_FAILED` if
the visible message is not skimmable.
`HYBRID_STATUS_PACKET_DUMP_FORBIDDEN`: a formal human board plus a long raw
claim/evidence packet is unreadable hybrid status. Reject `Phase/Step/Status`
text or any formal human board / phase-step-status opening followed by
`claim_scope`, `evidence_scope`, `count_claimed`, `independent_evidence_count`,
`subagent_id_or_NONE`, `downgrade_needed`, raw worker packet, or machine
inventory as the main visible body. Reject it as
`STATUS_FORMAT_HUMAN_GATE_FAILED`. Parent output must summarize the worker
packet in human language first; routine progress may show only one short
`Evidence:` sentence or 1-2 compact support lines. Full schemas are reserved for
worker return packets, acceptance/rejection reviews, final summaries, or
explicit audit/evidence answers.

`PARENT_TRANSLATES_WORKER_MACHINE_FIELDS`: parent-visible status must translate
worker `claim_scope/evidence_scope` fields into human meaning, result, remaining
gap, and next bounded action before showing them to Luba. A spawned worker packet
with `subagent_id_or_NONE: NONE` is identity-defective; the worker uses
`UNKNOWN_IN_SUBAGENT` when the platform id is hidden, and the parent reconciles
the real platform id before accepting the packet.

`DIAGNOSTIC_STATUS_CLAIM_REQUIRES_VISIBLE_EVIDENCE`: when Luba asks why,
where the proof is, whether something was checked, or challenges a factual
status answer, readability fails if the answer gives only a conclusion. The
answer must include a compact proof in the operator's language:

```text
Conclusion: <plain conclusion>
Evidence: <command/readback/path checked> -> <observed result>
Limitation: <what this does not prove, if relevant>
```

`COMPACT_EVIDENCE_IS_REQUIRED_NOT_OPTIONAL`: ordinary heartbeat updates may use
short evidence, but direct diagnostic/status answers cannot omit evidence
entirely. If no command/readback was run, the answer must say "this was not
checked now" before inference.

Human gate for chat logs: a log/status fails if Luba cannot tell from the
visible message what is done, what is running, what remains, whether the current
result is code, artifact, review, policy, or evidence-only, which claims are
still closed, and what bounded action comes next. The repair is governance/skill
repair before product continuation, not a final stop while the next action is
known.

### Human-Readable Execution Board Gate

Every long plan execution status must include a plain operator-language progress board:

```text
Total: <total major plan blocks and/or current-phase tasks>
Done: <completed count>/<total>; <completed names>
In progress now: <current task and owner/sub-agent>
Remaining: <remaining count>; <remaining names>
Current result type: <code | artifact | review | policy | evidence-only | commit boundary>
Accepted evidence: <accepted evidence in human words>
Closed claims and why: <claim names + human reason they remain closed>
Open gaps: <open gaps>
Next action: <next bounded action>
Execution paused for plan-review visibility: yes/no
```

`STATUS_FORMAT_HUMAN_GATE_ALWAYS_REQUIRED`: every progress/status/review/final/
worker packet visible to Luba must pass the canonical operator-language human gate before
any machine block, raw packet, JSON, ledger row, STATE path, validator label, or
opaque status. `STATUS_FORMAT_CHALLENGE_REPAIR` is a special case: when Luba
challenges the format of a work status, including an operator-language phrase (e.g. `Формат статуса работы не
соответствует требованиям` or the misspelled `Формат статуса работы не соответсвует
требованиям`), answer first, classify `status_format_governance_gap`, patch or
verify the governing rule, and only then continue execution. Acceptance/final/
worker/audit packets use the full canonical operator-language block; ordinary commentary
progress may use the shorter human-first form from
`ROUTINE_PROGRESS_COMPACT_EVIDENCE_GATE`.

```text
Phase X of Y: <human-readable name>. Meaning of the phase: <what this phase solves for the project/user>.
Step X.Y of X.N: <human-readable name>. Meaning of the step: <what this step proves/fixes>.
Status: DONE | IN_PROGRESS | NEEDS_FIX | REVIEWING | BLOCKED_WITH_REASON — <plain explanation in the operator's language>.
Total: <total number of phases/steps/work this status refers to>.
Done: <what is actually completed, with evidence boundary>.
In progress: <what is running now and by whom/what>.
Remaining:
1. <remaining step and its meaning>.
2. <remaining step and its meaning>.
Review found: <what the check found, or "not checked yet">.
What was fixed: <what changed, or "nothing fixed yet">.
Next action: <next bounded action or NONE>.
final/status/report/commit-push/status-board is not a stop while authorized work remains: <required when remaining_count > 0>.
Evidence: <optional one short support sentence or 1-2 compact evidence lines for routine progress>.
```

If `remaining_count > 0`, omitting the exact literal
`final/status/report/commit-push/status-board is not a stop while authorized
work remains` is a readability failure. Machine logs, ledgers, JSON, file paths,
claim fields, or short progress prose may support the block, but cannot replace
it. In routine commentary, evidence support is capped at one `Evidence:`
sentence or 1-2 short lines; the full claim/evidence schema is reserved for
worker return packets, acceptance/rejection reviews, final summaries, and direct
audit/evidence answers. Reject as `STATUS_FORMAT_HUMAN_GATE_FAILED` any
machine-only packet, raw `claim_scope` block without the operator-language phase/step/
status board, bare label such as `NEEDS_FIX`, `packet accepted`, `QSG`,
`boundary pass`, STATE/JSON path, informal partial board, or formal `Phase...`
opening followed by a long raw evidence dump. A returned sub-agent packet is not
user-visible status until the parent translates it into this human-gate format.
If Luba says the status is unreadable (operator-language e.g. `не читабельно`, `нечитабельно`) or `format lost`, or
equivalent, treat it as a status-format challenge: answer first, classify the
old output as `STATUS_FORMAT_HUMAN_GATE_FAILED` or
`status_format_governance_gap`, patch or verify this rule, and then resume the
nearest authorized product work.

If the board is missing, the status is not readable enough even when STATE,
JSON, or validator output exists. The board must appear in the user-visible
message before or alongside machine evidence.

`status_board_counts_drive_continuation`: the visible board is execution
control, not a pause surface. If the board says `remaining_count > 0` and
`next_bounded_action_exists=true`, the readable output must say that this is
interim status and the parent must immediately continue by starting, waiting
for, reviewing, or repairing the next bounded sub-agent work order. A
final-style summary is readable only when Luba explicitly stops/pauses/asks for
answer-only or all active requirements are complete. Required literal:
`final/status/report/commit-push/status-board is not a stop while authorized
work remains`. Commit/push of an evidence packet or status board is not a
completion boundary when the status board says remaining groups exist.
Exact one-line required literal:
`final/status/report/commit-push/status-board is not a stop while authorized work remains`.
`accidental_final_recovery_requires_subagent_repair_and_resume`: after an
accidental final/status stop, the readable next-turn message must answer Luba
first, say what loophole was repaired through bounded sub-agents, and say which
previously known bounded action resumes without fresh approval unless Luba stops
or a real blocker/unsafe action exists.

### No Machine Log To User Rule

User-facing progress cannot be only `QSG-28 accepted`, `STATE goal_reached=true`,
`PASS_FOR_LUBA_REVIEW`, a JSON object, or an internal status label. Internal
labels may appear only with a human translation:

```text
Check: what was checked
Result: what passed or failed
What was fixed: repair made or NONE
What remains: remaining gap or NONE
```

If a status cannot be understood without opening raw logs, return it to
readability repair.

## Section 10 — Execution Plan Visibility Gate

Before runtime/code/training/host execution continues after governance repair,
the parent must present the execution plan and 10 Review Cycle Gate outcome in
chat in the human-readable operator's language. The visible message must include:

```text
current_stage:
current_plan_step:
10_review_cycle_outcome:
plan_sections_changed:
claims_opened:
claims_closed:
remaining_gaps:
execution_admission_reason:
```

A plan hidden in files, a JSON matrix, or a machine-only log does not satisfy
this gate. The chat packet must be understandable without reading the raw log.

### Plan Presentation Gate

If Luba says she cannot see the plan, cannot see the 10 reviews, or cannot
understand the current stage, the next user-visible packet must present the plan
in chat before execution resumes. This is a visibility/protocol failure to
repair, not a final stop by itself. Artifact-only plans, STATE paths, JSON logs,
and hidden matrices do not satisfy the gate.

### Ten-Review Evidence Gate

When 10 reviews are required, the readable packet must show records 1..10. Each
record includes reviewer role/id, requested model/effort, verdict, findings,
repair action, files/sections affected, whether the plan/skills changed, and
recheck result. A summary such as "10 reviews completed" is not readable
evidence.

### Sub-Agent Visibility Ledger

Any sub-agent work used as evidence must be summarized in human language:
agent id or nickname, requested model, requested effort, task, status, key
result, exact changed files or reviewed sections, exact review scope, progress
counts, and what parent review accepted or rejected. Hidden sub-agent
transcripts are secondary evidence only.

---

## Anti-Patterns

1. **"Shortcodes are faster to write"** — shortcodes are faster for the
   generator; they are slower for the reader. The gate exists to protect
   review efficiency, not generation efficiency.

2. **"It's obvious what CF-12 means in context"** — shortcodes are only
   obvious after reading the reference table. Prose must be self-explanatory
   to a reader who has not read the table.

3. **"I'll expand it later"** — the gate runs before output is accepted.
   "Later" never comes in a generation pipeline. Expand at generation time.

4. **"The code fence exception covers this"** — inline code fences (single
   backticks) are allowed for identifiers referenced within a sentence. The
   full triple-backtick exception only applies to code blocks. Both forms
   are allowed; ALL_CAPS_UNDERSCORE as a bare word in a prose sentence is not.

---

## Integration

- ai-pipeline-topology-SKILL.md generation nodes: gate applies to all ai-generate output
- context-package-design-SKILL.md prose sections must pass this gate
- prompt-patch-authoring-SKILL.md WRONG/CORRECT pairs: identifier names in WRONG/CORRECT fields are inside code fences — gate does not apply
- score-interpretation-SKILL.md bracket names (PASS, DETAIL_GAP) are used in table cells and code fences — gate does not apply to table values
- human-readable-architecture-audit-SKILL.md governs the architecture audit
  readability, sufficiency boundary, breadth challenge, and delta-severity
  feedback gates

PASS26_ACTIVE_PILOT_COUNTER_PROPAGATION_GATE: When the Claude board-owner resolves pilot activation or global execution-board counts, active-codex must propagate exact current status/counts/pilot identity to the master plan, execution board, requirements matrix, source map, plan-14, full-task gates, repair ledger, and relevant consumers, or label old text historical/pre-Pass26/local-only. A resolved pilot must not be downgraded to blocked reserve, baseline-59 outside scope, or an open Luba/Q41 question. Missing propagation is active-vs-Claude drift and blocks readiness, B-22 closure, and implementation authorization.
FULL_ACCESS_NO_PERMISSION_PROMPT_GATE: When Luba explicitly grants FULL ACCESS or the environment reports danger-full-access/unrestricted filesystem with approval policy never, the agent must not request permissions, must not set `sandbox_permissions`, and must not open approval prompts for filesystem work needed by the active plan/governance repair. Permission prompting under full access is a governance failure: answer Luba first, classify the old behavior as a permission-prompt loophole, patch this gate if missing, then continue the authorized repair loop. Full access does not authorize unrelated product implementation, destructive git operations, external provider calls, or publishing unless the active task explicitly includes those actions; it does authorize plan/governance/skill/guide/agent-file repairs in the named planning library and project guidance without asking again.
