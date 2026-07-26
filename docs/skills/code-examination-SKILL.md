# Code Examination

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

## Core Problem

A file modified without being fully read produces two failure modes: the
fix corrects the symptom at the call site while the root cause is in the
caller; or the fix is correct in isolation but breaks a consumer that
depended on the old behavior. CLAUDE.md Rule 5 (Read before touching)
requires reading. This skill defines the 5-step examination protocol that
makes reading effective: read completely, map callers, map consumers, form
a hypothesis before reading tests, then read tests.

---

## When to Invoke

- Before modifying any service class, interface, or test in this project
- Before any Phase implementation (Gate 0 in phase-planning-SKILL.md)
- When a bug report references a file not read this session
- When root-cause-tracing-SKILL.md identifies a deep call-chain source

---

## The Rule

> **Understand before touching.**
> Never modify a file you have not read and understood in the context of
> its callers and consumers.

## Sub-Agent-Only Parent Boundary

When Luba says the parent/current session is sub-agent-only, architect-only, or
forbidden to become executor, this examination protocol belongs inside the
bounded sub-agent work order. The parent must not use code examination as local
sidecar work while the sub-agent runs.

Parent may read governed files or code only after the sub-agent packet returns
and only for verification/integration of that packet. Reading AGENTS.md,
CLAUDE.md, skill indexes, plans, code, reports, or state to independently find
loopholes or prepare fixes before packet return is a role-firewall failure,
even when it is read-only.

Parent Architect Subagent-Only Firewall compatibility: Sub-agent waiting is not a stop. Code examination belongs inside the bounded sub-agent until the packet returns.

---

## Section 1 — Five-Step Protocol

### Step 1 — Read the File Completely

Read the whole file. Not the method the bug is in. The whole file.

```bash
# Read the target file
cat src/[SubprojectN]/[ClassName].cs

# Read its interface contract
cat src/[SubprojectN]/Interfaces/I[ClassName].cs
```

Do not skip lines. Do not scan for the relevant section. A hidden
assumption three methods above the bug site is often the actual cause.

### Step 2 — Map Upstream Callers

```bash
# Find every caller
grep -rn "[ClassName]\|[MethodName]" src/ --include="*.cs" | grep -v "\.cs:[0-9]*://.*"

# Find interface injectors
grep -rn "I[ClassName]" src/ --include="*.cs"
```

A change to an output shape (OperationResult<T> generic type, return
fields) affects every downstream consumer. A change to a method signature
affects every caller. Know both before touching anything.

### Step 3 — Map Downstream Consumers

For service methods: identify what reads this method's return value.
For IAgentMessageBus events: identify every subscriber.

```bash
# For IAgentMessageBus event consumers
grep -rn "Subscribe\|\.On\|\.Handle" src/ --include="*.cs" | grep "[EventType]"
```

Build a chain:
```
IncrementalTrainer.Train() → DomainModelRegistry.UpdateEdge() →
ContinuousLearningPipeline.Observe()
```

A change to the middle of the chain affects both ends.

### Step 4 — Form a Hypothesis BEFORE Reading Tests

State in plain language: "I believe this code does X because Y. If I
change Z, then W will break."

Write it in your working notes before opening the test file. Tests
confirm or refute your hypothesis. If your hypothesis is wrong after
reading the tests, re-read the implementation code — the mental model
needs correction before the code does.

### Step 5 — Read the Test File

```bash
cat tests/[SubprojectN]/[ClassNameTests].cs
```

Verify: does the test match your mental model? If the test asserts
behavior that your hypothesis says should not happen: your model is
wrong. Fix the model first. Then modify the code.

### Step 6 - Final Diff And Structured-File Recheck

After edits, examine the final diff for the files you touched. If you edited
JSON, YAML, XML, project files, or generated reports, run the matching parse or
build check after the final state update, not before it.

For project-boundary work, the final diff must show that production code lives
in the requested project and tests reference that project normally. A test-only
`Compile Include`, source-link, temporary project, or script adapter is evidence
only; it is not the implementation boundary unless the work order explicitly
asked for that artifact type.

### Step 6A - Code Review Completeness Scope

Before a code slice can be accepted, the final examination/review must name the
exact files, sections, hosts, and claims inspected. It must explicitly check
project coding standards, allowed tests or Gate B, whether core logic remains
inside `llm_mvp_core`, whether Claude/Cursor/Codex host adapters stay thin,
whether hidden deterministic shortcuts exist, whether deterministic helpers are
labelled eval-only/helper/internal baseline instead of runtime/trainable
authority, and whether docs/QA/dashboard/user-visible status must change.
Passing tests or a machine label alone is not a complete code review.

### Step 7 - Independent Review/Fix Cycle Count

When the active plan or Luba asks for several, multiple, or repeated
code-review/fix cycles, the minimum is two independent review passes after the
implementation or final fix.

Rules:

```text
review_cycle_1:
  reviewer_or_pass_id:
  scope:
  findings:
  fixes_applied_or_zero_findings_evidence:

review_cycle_2:
  reviewer_or_pass_id:
  scope:
  findings:
  fixes_applied_or_zero_findings_evidence:
```

A single final summary review is not "several cycles". If review_cycle_1 finds
issues and a fix is applied, review_cycle_2 must run after that fix. Completion
is blocked until every finding is fixed or each pass records explicit
zero-findings evidence for its scope.

---

## Section 2 - Diagnostic Routing Table

When you observe a symptom, choose the next action from this table:

| Symptom | First action | Then |
|---------|-------------|------|
| OperationResult.Fail() returned unexpectedly | Read guard clauses in implementation | Trace backward to caller (root-cause-tracing-SKILL.md) |
| ISessionStorage returns wrong value | Check write site and key naming | Verify SessionId scope (scope-isolation-arbiter-SKILL.md) |
| DomainModelRegistry returns no edges | Check GetEdges() filter parameters | Verify edge was written with correct Relationship field |
| IAgentMessageBus event not received | Binary search through subscriber chain | event-driven-debugging-SKILL.md |
| IncrementalTrainer score not improving | Examine DPO triple validity gate | Check OUTCOME signal is present in feedback node |
| dotnet test passes but behavior is wrong | Check for stub returning hardcoded value | test-integrity-SKILL.md FM-1 (stub-fix masking) |

---

## Section 3 — Reading Checklist

Before declaring a file "read":

```
□ Read the complete file — not just the method under investigation
□ Identified all callers of the method being changed
□ Identified all consumers of the method's return type
□ Written a hypothesis in plain language before opening tests
□ Read the test file — does it match the hypothesis?
□ If hypothesis was wrong: re-read the implementation
□ Final diff reviewed after edits
□ Code-review completeness scope recorded: standards, tests, core boundary,
  thin host adapters, deterministic shortcut sweep, deterministic labels, and
  docs/QA/dashboard impact
□ Structured files parsed after the final state/report edit
□ New project boundaries use real project references, not test-only source links
□ When several review/fix cycles were required, at least two independent
  post-implementation review passes are recorded after the final fix
```

---

## Anti-Patterns

1. **"Grepping for the bug location then editing only the matching line"** —
   the matching line may be correct; the caller may be setting the wrong
   field. Read the full chain before editing any line.

2. **"Reading tests before reading implementation"** — tests describe
   behavior, not mechanism. Reading tests first gives you a model of the
   contract, not of how the implementation achieves it. Read implementation
   first, tests second.

3. **"I know this class — I don't need to re-read it"** — code changes.
   Session context does not persist across conversations. Re-read every
   file intended for modification, every session.

4. **"Modifying without a written hypothesis"** — if you cannot state in
   one sentence what the code does and what your change will affect, you
   are not ready to modify it. Write the hypothesis. Then modify.

5. **"Mapping only the direct caller, not the full chain"** — an
   OperationResult<T> generic change at the bottom of a chain can
   produce a compiler error six levels up. Map the chain before changing
   the shape.

---

## Integration

- CLAUDE.md Rule 5 (Read before touching) is the mandate; this skill is the protocol
- phase-preflight-SKILL.md Gate 0 requires this examination before any implementation
- root-cause-tracing-SKILL.md resumes from Step 3 (chain map) when a fix propagates incorrectly
- mental-debug-SKILL.md provides the diagnostic rules; this skill provides the reading order
- test-integrity-SKILL.md FM-1 handles the case where Step 5 reveals a stub is masking failure

## Trainable Architecture Examination Add-On

When examining learned, semantic, trainable, adaptive, DPO, selector, router,
classifier, scorer, retrieval, KNN, dictionary, counter, or pattern-matching
code, also search for:

- `TryDecide`;
- `ChooseBestPattern`;
- `SelectBestPattern`;
- `CountMatchingSignals`;
- `NormalizeSignal`;
- `FindNearest`;
- `Jaccard`;
- `BM25`;
- `_candidatePositionWeights`;
- `Dictionary<string, int>`;
- `Regex`;
- `Contains`.

For each hit, trace whether the method is:

- inside `ILearningMachine`;
- inside `ILogicalOrchestrator`;
- inside `ILogicalFlow`;
- inside `ILogicalUnit`;
- inside `ILearningMachinePair`;
- or outside the allowed hierarchy.

Report every finding in this form:

```text
Human explanation in the operator's language (plain language; Russian for the original author):
Code quote with exact file:line:
now:
why bad/ok:
should become:
Allowed location:
Current location:
Dataset risk:
Test/doc impact:
Verdict:
Concrete repair:
  current -> target:
  replacement allowed level:
  replacement class/unit:
  replacement algorithm:
  input/output contracts:
  typed trained state:
  KNN/nearest methods and role:
  training source roots and licenses:
  DPO pair-building rule:
  load/cache/repo fallback:
  backup/export/import/share:
  continue-training from checkpoint:
  code-shape change:
  affected training sets:
  set rebuild rule:
  affected tests:
  documentation/report wording:
```

Do not accept "make it trainable", "wrap it", "add tests", or "mark as
provenance-only" as a complete repair. The repair must name the actual unit or
flow shape and the algorithm it will run. For example, a candidate-position
counter must either be removed from final authority or converted into a
candidate-order leakage guard `ILogicalUnit` that tests permutation sensitivity
and blocks position features; it must not remain the selector.

For architecture/trainable/code audit reports, also apply the Human-Readable
Architecture Artifact Gate and Concrete Repair Blueprint Gate. A schema-only,
table-only, English-only, or prompt/spec finding is not an examination result.
It is not a decision until it names concrete files/classes/records/methods,
data rows, tests/e2e/qa, affected sets/checkpoints/manifests, and lifecycle
paths. When Luba asks for iterative readability improvement, require minimum 6
cycles, preferably 10, with a cycle ledger.

If an examination report claims "Architecture audit sufficient/complete",
"enough", "done", or "closed", it must also pass the Audit Sufficiency
Boundary Gate: bounded scope, green breadth challenge over adjacent
trainable/runtime authority zones, the parent question "What would a broader
audit likely find that this artifact missed?", and Delta-Severity Feedback Gate
repair for later same-domain BLOCKING/HIGH misses.

For every learned dictionary/counter/map, require one concrete replacement row.
Example mappings that must be considered in `SkillSelectionMachine`:

```text
_candidatePositionWeights -> CandidateOrderLeakageGuardUnit
_promptUnderstandingSignals -> PromptUnderstandingSkillUnit
_taskUnderstandingSignals -> TaskUnderstandingSkillUnit
_promptContextSignals -> PromptContextSkillSelectionUnit
_intermediateChecksSignals -> IntermediateChecksSkillUnit
```

If the implementation keeps a nested dictionary as storage inside typed state,
verify that it is not the public architecture shape and that reports label the
unit as baseline until held-out, ablation, fresh-load, and continue-training
evidence prove stronger quality.
