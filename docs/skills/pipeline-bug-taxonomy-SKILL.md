# Pipeline Bug Taxonomy

## Luba Why / Loophole Repair-First Rule

When Luba asks "why", "почему", challenges an answer, or says a failure came
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

A bug report that says "the pipeline doesn't work" cannot be fixed because
it does not identify what layer is wrong. The same symptom (wrong output)
can originate in six different layers, and the fix at each layer is
different. Fixing the wrong layer produces a cosmetic result — the symptom
disappears locally while the root cause compounds. This skill defines the
six bug classes, the taxonomy decision rules that distinguish them when
two classes look similar, and the FIX template that every bug report must
use before applying bug-to-tests-SKILL.md.

---

## When to Invoke

- When a confirmed bug needs to be classified before writing failing tests
- When reviewing a fix to ensure it addresses the correct class of failure
- When a single symptom could belong to two different classes

---

## Section 1 — Six Bug Classes

| Class | Name | Description | .NET Example |
|-------|------|-------------|-------------|
| A | Fabric Interface | An interface implementation returns wrong shape, wrong data, or wrong OperationResult status | `IVectorIndex.Search()` returns `OperationResult<List<float[]>>` instead of `OperationResult<List<SearchResult>>` |
| B | Event Bus | IAgentMessageBus queue coordination failure: event lost, duplicate processing, wrong order, deduplication failure | Subscriber receives same event twice; IAgentMessageBus.Subscribe() handler fires zero times |
| C | DNA Pattern | Generated or handwritten code violates one of CLAUDE.md Rules 1-10 — violation found in output | Service method returns `void` instead of `OperationResult<T>` (Rule 1 violation); concrete type injected instead of interface (Rule 2 violation) |
| D | Pipeline Stage | A ContinuousLearningPipeline or IncrementalTrainer stage uses stale, wrong, or missing data from upstream — OR the root cause of a Class C violation is traceable to a specific pipeline stage | `IncrementalTrainer.Update()` reads confidence from wrong DomainModelRegistry path; genesis stage uses wrong archetype context causing Rule 1 violation |
| E | Cross-Subproject | Two subprojects publish or consume the same interface or event, causing collisions or duplicate registrations | Subproject 21 and 22 both register a concrete implementation of IBm25Scorer; downstream consumers process the same learning signal twice |
| F | Contract | An OperationResult<T> contract has wrong generic type, missing fields, or wrong error codes | `OperationResult<int>` where `OperationResult<ScoreResult>` is required; ErrorCode missing from Fail() call |

---

## Section 2 — Taxonomy Decision Rules

### Q1 — Wrong return shape: Class F or Class A?

These are two separate bugs:

```
CLASS F (contract wrong): fix the return type declaration or OperationResult generic parameter
CLASS A (interface wrong): fix the implementation method body that produces the wrong shape
```

Example: `IVectorIndex.Search()` is declared to return `OperationResult<List<float[]>>` but
the domain requires `OperationResult<List<SearchResult>>`.

- If the interface signature itself is wrong: **CLASS F** — fix the contract declaration
- If the interface signature is correct but the implementation returns wrong data: **CLASS A** — fix the implementation

### Q2 — DNA violation in generated output: Class C or Class D?

Class C = what (the violation exists), Class D = why (which stage caused it):

```
CLASS C: violation found by dna-compliance-guard-SKILL.md scan — fix the code that violates the rule
CLASS D: root cause is traceable to a specific pipeline stage decision (wrong context injected →
         genesis stage generated bad output) — fix the pipeline stage
```

The same underlying bug can be filed first as Class C (output fix) then as Class D (root cause
fix), or directly as Class D if the root cause is immediately clear from the stack trace.

### Q3 — Wrong skill injection causing DNA violation: Class D subtype

Classifying skill injection failure as a new class is incorrect.
`IncrementalTrainer selecting wrong capability context → ContinuousLearningPipeline generating
bad output` is a Class D (Pipeline Stage) failure. File it with subtype `D-skill-injection`
in the ROOT CAUSE field.

---

## Section 3 — FIX Template

Every bug report uses this format. All 8 fields are required before
writing the first failing test.

```
BUG-ENGINE-NN: [one-sentence description of the wrong behavior]
CLASS: A | B | C | D | E | F
ROOT CAUSE: [mechanism — what the code did wrong, not just symptoms]
WRONG ASSUMPTION: [what the code assumed that was false]
FIX: [what changed in the implementation — file:line — not just the output]
DNA RULES AFFECTED: Rule N (or N/A)
FILES: [file:line of the fix]
REGRESSION RISK: [which subprojects or tests could be affected — or N/A]
TESTS: [unit + integration + full suite — see bug-to-tests-SKILL.md]
```

### Worked Example

```
BUG-ENGINE-001: IncrementalTrainer reads confidence score from wrong DomainModelRegistry path
CLASS: D
ROOT CAUSE: IncrementalTrainer.Update() reads confidence from
  domainResult.Context.ConfidenceScore but DomainModelRegistry.GetEdges() writes
  it to domainResult.Edges[n].Confidence (top-level field, not nested under Context).
  IncrementalTrainer always receives 0.0 and falls back to SEED confidence.
WRONG ASSUMPTION: Confidence is nested under Context; it is a top-level edge field.
FIX: IncrementalTrainer.cs line 84 — change domainResult.Context?.ConfidenceScore → domainResult.Edges.First().Confidence
DNA RULES AFFECTED: N/A
FILES: src/22.AgentAlgorithms/IncrementalTrainer.cs:84
REGRESSION RISK: ContinuousLearningPipeline (reads IncrementalTrainer output), DomainModelRegistry tests
TESTS: unit (IncrementalTrainer returns expected confidence) + integration (full learning loop) + suite (dotnet test 0 failures)
```

---

## Anti-Patterns

1. **"Filing a bug without classifying it"** — "the pipeline doesn't work"
   produces no actionable fix target. Classify before writing tests.

2. **"Treating Class C (output violation) and Class D (stage root cause) as
   the same bug"** — fixing the output (Class C) while leaving the stage
   that caused it (Class D) means the violation reappears on the next
   pipeline run. File both if the root cause is a stage.

3. **"Class F used for implementation wrong, Class A for contract wrong"** —
   Class F = wrong contract declaration; Class A = wrong implementation.
   Read the interface definition before classifying.

4. **"FIX field describes the symptom, not the mechanism"** — "fixed the
   confidence score" is not a fix description. "Changed field path from
   Context.ConfidenceScore to Edges.First().Confidence at line 84" is.

5. **"Filing a Class E collision without verifying both registration sites"** —
   a cross-subproject collision requires identifying both subprojects and
   both registration points. A single site identified is a Class A, not a
   Class E.

---

## Integration

- bug-to-tests-SKILL.md requires a classified defect and discriminating regression coverage with appropriate controls; no fixed quota of three failing tests
- three-level-verification-SKILL.md verifies the fix at unit + integration + suite levels
- dna-compliance-guard-SKILL.md is the Class C detection tool
- root-cause-tracing-SKILL.md produces the ROOT CAUSE and WRONG ASSUMPTION fields
- data-flow-integrity-SKILL.md maps the producer/consumer chain for Class B (event bus) failures
