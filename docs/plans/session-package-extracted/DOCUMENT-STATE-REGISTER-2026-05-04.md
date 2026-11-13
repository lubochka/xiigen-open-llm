# Document State Register — Dynamic Flow Project
## Produced: 2026-05-04 | Session type: ARCHITECT
## Source: latest_dynamic_docs.zip (36 files) + round manifests rounds 01–07
## Test baseline at time of writing: 985/985 passing (from round-07 manifest)
## Purpose: authoritative record of every document's current state, open gaps,
##           how testing works across the set, and how success is measured.

---

## How to use this register

Read this file at the start of any session that touches the dynamic flow document set.
Every section below names exactly what is done, what is not done, what must happen
before the next commit, and what command confirms the change is complete.

---

## Round 07 blocker — this must be resolved before any other thread proceeds

Round 07 tests pass (985/985). The round is **not closed**. Two blockers remain.

### Blocker 1 — Rule 3 violation in `InternalFirstAiSupport.cs`

**File:** `22.AgentAlgorithms/Pipeline/InternalFirstAiSupport.cs`

**Violation:** No plain-language comments exist on any logic line in the file.
The following sections have zero comments:

| Section | Lines affected |
|---------|---------------|
| `FindArbiterRejection` — the foreach loop body | all logic lines |
| `KnownArbiterRules` dictionary — each named entry | all entry lines |
| `ContradictsLockedDuplicateRule` — all keyword detection logic | all logic lines |
| `ContradictsTokenSafetyRule` — all keyword detection logic | all logic lines |
| `Adapt` method — predicate extraction block | all logic lines |

**What the executor must do:**

Add a plain-language comment above or inline on every logic line. Comments must
describe what the code does in plain English — not restate the code in technical
terms.

Example of correct comment style:
```csharp
// Iterate the constraints declared by the caller and stop at the first rule
// that recognises a forbidden pattern in the external answer.
foreach (var constraint in package.ArbiterConstraints)
```

**Gate B for Blocker 1:**
```bash
grep -n "^[[:space:]]*[a-zA-Z]" 22.AgentAlgorithms/Pipeline/InternalFirstAiSupport.cs \
  | grep -v "^[[:space:]]*//"
# Expected: zero lines that are logic without a comment above or inline.
# Every hit is a violation that must be addressed before this check passes.
```

**After adding comments:**
```bash
dotnet test --filter "ExternalAiResultAdapter" Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -3
# Expected: 4/4 passed, 0 failed

dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
# Expected: 985 passed, 0 failed (count must not drop)
```

Recreate the round-07 zip with the corrected file. Update the round-07 manifest
to note that the Rule 3 violation was resolved.

---

### Blocker 2 — Three documents do not reflect the named rule-table design

Round 07 replaced the hardcoded `ContradictsLockedDuplicateRule` call with a
named arbiter constraint table in `FindArbiterRejection`. Two constraint names
are now live: `identity_consistency` and `token_safety`. Unknown constraint
names pass without rejection. This design is not described in any document.

#### Document 1: `EXTERNAL-AI-KNOWLEDGE-EDGE-AMENDMENT-v1.md`

**Location in file:** the section describing the external result adapter station
(station 4 in the 8-station path).

**What is currently there:** "The external result adapter converts outside advice
back into local dynamic-flow structure." No description of how it decides whether
to accept or reject.

**What must be added:** One paragraph after the station 4 description:

> The adapter applies a named arbiter constraint table before converting an
> external answer into local learning data. Each constraint in the caller's
> `ArbiterConstraints` array is looked up by name. If a matching rule exists and
> the external answer matches its rejection pattern, the adapter returns the
> answer as rejected rather than adapted. Constraint names that are not in the
> table pass without rejection. The table is the extension point for future
> arbiter enforcement at the adapter level.
>
> Two constraints are currently registered: `identity_consistency` rejects
> external guidance that would create duplicate accounts from repeated provider
> callbacks; `token_safety` rejects guidance to store session tokens in
> localStorage, query strings, URL parameters, or non-httpOnly cookies.

**Gate B for this document:**
```bash
grep -c "identity_consistency\|named arbiter constraint table" \
  EXTERNAL-AI-KNOWLEDGE-EDGE-AMENDMENT-v1.md
# Expected: 2 (one hit per search term)
```

---

#### Document 2: `DYNAMIC-FLOW-CANONICAL-INTEGRATION-SEQUENCE-v1.md`

**Location in file:** section 11, "External AI Help Path", step 4 — external
result adapter.

**What is currently there:** "The external result adapter converts outside advice
back into local dynamic-flow structure."

**What must be added:** One sentence immediately after that line:

> Before converting, the adapter checks the external answer against a named
> arbiter constraint table; answers that match a known rejection pattern are
> returned as rejected rather than adapted, preserving arbiter authority at the
> adapter boundary.

**Gate B for this document:**
```bash
grep -c "named arbiter constraint table\|rejection pattern" \
  DYNAMIC-FLOW-CANONICAL-INTEGRATION-SEQUENCE-v1.md
# Expected: 2 (one hit per search term)
```

---

#### Document 3: `DOCUMENT-ADAPTATION-ANALYSIS-v1.md`

**Location in file:** the "Maintenance Status" section at the top of the file.

**What is currently there:** The maintenance status records the document
adaptation round and the topology visual round. Round 07 is not mentioned.

**What must be added:** Two entries appended to the maintenance status section:

> **Round 07 — 2026-05-04:** `ExternalAiResultAdapter` in
> `22.AgentAlgorithms/Pipeline/InternalFirstAiSupport.cs` was generalized from
> a hardcoded duplicate-account check to a named arbiter constraint table
> (`FindArbiterRejection`). Two constraints registered: `identity_consistency`
> and `token_safety`. Unknown constraint names pass without rejection.
> `EXTERNAL-AI-KNOWLEDGE-EDGE-AMENDMENT-v1.md` and
> `DYNAMIC-FLOW-CANONICAL-INTEGRATION-SEQUENCE-v1.md` updated to reflect this.

> **Round 07 open blocker:** `InternalFirstAiSupport.cs` violates Rule 3.
> Plain-language comments are absent on all logic lines. The round is not
> committed until this is resolved and the zip is recreated.

**Gate B for this document:**
```bash
grep -c "Round 07\|FindArbiterRejection" DOCUMENT-ADAPTATION-ANALYSIS-v1.md
# Expected: 2 (one hit per search term)
```

---

## Document status table — all 36 files

| Document | Status | Open gap |
|----------|--------|----------|
| `DECISIONS-LOCKED.md` | Current — D-01 through D-15 present | None |
| `WHAT-TO-DO-WHAT-NOT-TO-DO-v2.md` | Current — v2 corrections applied | None |
| `DYNAMIC-FLOW-ARCHITECTURE-PLAN-v1_0-DRAFT.md` | Current — Principle 5 qualified, 6 new roles added, 60-slot naming added | Does not mention rule-table design — acceptable, this is architecture-level |
| `DYNAMIC-FLOW-HIGH-LEVEL-DESIGN-AMENDMENT-v1.md` | Current — 20-node flow, post-E51 note, visual gap closed | None |
| `DYNAMIC-FLOW-CANONICAL-INTEGRATION-SEQUENCE-v1.md` | Needs one sentence — see Blocker 2 above | Rule-table sentence missing in section 11 |
| `ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md` | Current — E51B CH-02 reference, domain-scoped KNN, wrong-domain guard | None |
| `EXTERNAL-AI-KNOWLEDGE-EDGE-AMENDMENT-v1.md` | Needs one paragraph — see Blocker 2 above | Rule-table paragraph missing in station 4 |
| `PROMPT-AND-DATASET-LOGICAL-STRUCTURE-EXAMINATION-v1.md` | Current — E51B reference, 60-slot named, 4 arbiter families scoped | None |
| `EXAMINATION-METHOD-FINAL.md` | Current — node-00 guidance, 60-slot shape, arbiter-aware KNN note, InternalResultExaminer connection | None |
| `FLOW-01-TRAINING-DATASET-PLAN-v4.md` | Current — B-format warning, E51B registration proof | None — training pair files not yet produced (Thread 3) |
| `DOCUMENT-EXAMINATION-TO-TRAINING-SETS-PLAN-v1.md` | Current — superseded-by note added | None |
| `DOCUMENT-ADAPTATION-ANALYSIS-v1.md` | Needs two entries — see Blocker 2 above | Round 07 entries missing in maintenance status |
| `dynamic-flow-v2-node-topology.csv` | Current — N00 and N03B added | None |
| `dynamic-flow-high-level-topology-v2.mmd` | Current — logical structure compiler and budget/confidence gate nodes added | None |
| `E51-END-TO-END-PIPELINE-PLAN-v1.md` | Complete historical record | No changes expected |
| `E51-DUAL-TRACK-EXPERIMENT-STRATEGY-v1.md` | Complete historical record | No changes expected |
| `E51A-TIER2-COVERAGE-FIRST-PLAN-v1.md` | Complete historical record | No changes expected |
| `DYNAMIC-FLOW-MINI-E2E-EXPERIMENT-PLAN-v1_0.md` | Complete historical record | No changes expected |
| `DYNAMIC-FLOW-LOCKED-DECISION-D13-AUTHORITY-v1.md` | Complete — D-13 authority document | No changes expected |
| `PROJECTS-21-23-TESTING-GUIDANCE-v1.md` | Complete historical record — all 29 tests written and passing | No changes expected |
| `logical-structure-compiler-node-map-v1.csv` | Current reference document | None |
| `document-examination-method-v2.md` | Superseded by `EXAMINATION-METHOD-FINAL.md` | No changes expected |
| `flow-01-domain-splitter-training-full.md` | Superseded by final examination method pair format | No changes expected |
| `FLOW-01-TRAINING-DATASET-PLAN-v3.md` | Superseded by v4 | No changes expected |
| `SESSION-START-PROMPT-ROUND-ARTIFACT-ZIPPING-v1.md` | Current — round zip protocol | None |
| Round manifests (7 files) | Accurate records of what each round produced | Round-07 manifest needs a note that Rule 3 violation was found and resolved (after blocker 1 is closed) |
| `improve algo.drawio.xml` | Architecture diagram source | No document update needed |
| `llm_pipeline_corrected.svg` | Architecture diagram export | No document update needed |
| `previos session.txt` | Historical session transcript | No changes expected |
| `WHAT-TO-DO-WHAT-NOT-TO-DO-v1.md` | Superseded by v2 | No changes expected |

---

## Open threads

### Thread 1 — Close round 07 (blocker — must be first)

**Depends on:** nothing. All other threads are blocked until this is done.

**What needs to happen:**

Phase 1 — Add Rule 3 comments to `InternalFirstAiSupport.cs`

File: `22.AgentAlgorithms/Pipeline/InternalFirstAiSupport.cs`
Add plain-language comments on every logic line in the five sections listed under
Blocker 1. Comments must describe intent in plain English.

Gate B:
```bash
grep -n "^[[:space:]]*[a-zA-Z]" 22.AgentAlgorithms/Pipeline/InternalFirstAiSupport.cs \
  | grep -v "^[[:space:]]*//"
# Expected: zero hits (every logic line has a comment)

dotnet test --filter "ExternalAiResultAdapter" Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -3
# Expected: 4/4 passed, 0 failed

dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
# Expected: 985 passed, 0 failed
```

Commit message: `round 07: add Rule 3 comments to InternalFirstAiSupport.cs`

Phase 2 — Update the three documents (Blocker 2)

Files:
- `EXTERNAL-AI-KNOWLEDGE-EDGE-AMENDMENT-v1.md` — add paragraph to station 4
- `DYNAMIC-FLOW-CANONICAL-INTEGRATION-SEQUENCE-v1.md` — add sentence to section 11 step 4
- `DOCUMENT-ADAPTATION-ANALYSIS-v1.md` — add two entries to maintenance status

Gate B:
```bash
grep -c "identity_consistency\|named arbiter constraint table" \
  EXTERNAL-AI-KNOWLEDGE-EDGE-AMENDMENT-v1.md
# Expected: 2

grep -c "named arbiter constraint table\|rejection pattern" \
  DYNAMIC-FLOW-CANONICAL-INTEGRATION-SEQUENCE-v1.md
# Expected: 2

grep -c "Round 07\|FindArbiterRejection" DOCUMENT-ADAPTATION-ANALYSIS-v1.md
# Expected: 2
```

Phase 3 — Recreate round-07 zip and update manifest

Recreate `round-07-arbiter-constraint-adapter-2026-05-04.zip` containing the
corrected `InternalFirstAiSupport.cs`, the three updated documents,
`ROUND-MANIFEST-ROUND-07.md` (updated to note Rule 3 resolved and documents
updated), and `SESSION-START-PROMPT-ROUND-ARTIFACT-ZIPPING-v1.md`.

Gate B:
```bash
unzip -l round-07-arbiter-constraint-adapter-2026-05-04.zip | grep "InternalFirstAiSupport\|AMENDMENT\|SEQUENCE\|ADAPTATION"
# Expected: all four files present in the zip listing
```

---

### Thread 2 — Topology visual (complete)

Closed in the topology visual round. `dynamic-flow-v2-node-topology.csv` has N00
and N03B. `dynamic-flow-high-level-topology-v2.mmd` shows both nodes with correct
edges. No further work needed.

---

### Thread 3 — First training set pairs (ready to start after Thread 1 is closed)

**Depends on:** Thread 1 closed and worktree clean.

**Source authority:** `EXAMINATION-METHOD-FINAL.md` — read the node-00 section
before writing any pair.

**Domain:** Flow-01 user registration (validated by E51B, 6/6 trace success).

**First file to produce:**
`flow-01-training-sets-v1/node-00-logical-structure-compiler-set.md`

**What A and B look like:**
- A: a raw registration prompt — what a real user types, or what a document
  line says, written as a runtime input
- B: the compiled logical structure written out in full — source role, predicate
  list from the 22-item vocabulary, normalized roots, confidence score, any
  missing-coverage signal

**Rules that apply to every pair (from the examination method):**
- B must be the actual output, not a description of the output
- Every pair appears at least twice with different phrasing
- Blank lines and boundary markers get rows

**Plan shape:**

Phase 1 — Confirm source authority and 60-slot feature shape

Read `EXAMINATION-METHOD-FINAL.md` node-00 section completely.
Read `FLOW-01-TRAINING-DATASET-PLAN-v4.md` maintenance note.
Confirm the 22-predicate vocabulary and 60-slot output shape before writing.

Gate B:
```bash
grep -c "60-slot\|22.*predicate\|node-00" EXAMINATION-METHOD-FINAL.md
# Expected: >= 3 (confirms the feature shape is documented)
```

Phase 2 — Produce first 5 A/B pairs (registration prompts)

Create: `flow-01-training-sets-v1/node-00-logical-structure-compiler-set.md`
Pairs cover: sign up with email, sign up with Google, verify email link, reset
password during registration, confirm terms and conditions.
Each pair appears twice with different phrasing.

Gate B: peer review of B entries — each B shows the compiled logical structure
written out in full, not described.

Phase 3 — Produce 5 A/B pairs (knowledge-edge and boundary prompts)

Add to same file. Pairs cover: biometric passport registration (CH-01 type),
duplicate account creation (CH-02 type), blank input, multi-step registration
interrupted midway.

Gate B: same as Phase 2 — B entries are full compiled structures, not descriptions.

Phase 4 — Write simulation check

Create: `flow-01-training-sets-v1/10-SIMULATION-CHECK.md`
Answer all 10 simulation questions from the examination method. Name missing
parts honestly. A missing-parts section is required.

Gate B:
```bash
grep -c "missing\|gap\|not covered" flow-01-training-sets-v1/10-SIMULATION-CHECK.md
# Expected: >= 1 (the simulation must name what is missing — a zero count means
#            the check was not done honestly)
```

Phase 5 — Stop and review

Present the pair file and simulation check. Wait for review before producing
any other node file.

---

## How testing works

**The command:**
```bash
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

This is the only source of truth for the test count. Memory, JSON files, and
state documents are records of past runs — the live command is what the current
state is.

**The baseline:** 985 passing, 0 failing. This is the floor. No phase is complete
if the count drops below 985 or if any failure appears.

**Focused runs for verification:**
```bash
# Verify only the adapter tests
dotnet test --filter "ExternalAiResultAdapter" Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -3
# Expected: 4/4 passed

# Verify all algorithm tests
dotnet test --filter "AgentAlgorithmsTests" Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -3
# Expected: current count passed, 0 failed
```

**When to run:** before starting any work (to capture the baseline), and after
any code change (to confirm no regression before the zip or commit).

**Document changes do not require a test run.** They require Gate B text searches
(grep commands) as specified per document above.

---

## How success is measured

### For code rounds

| Criterion | How to verify |
|-----------|--------------|
| Tests pass at or above baseline | `dotnet test … \| tail -5` — count must be >= 985 |
| Focused filter matches expected count | `dotnet test --filter "…" … \| tail -3` |
| Rule 3 satisfied | grep for uncommented logic lines — zero hits required |
| Rule 1 satisfied | grep for public methods not returning OperationResult — zero hits required |
| Round zip recreated with corrected files | `unzip -l round-NN.zip` lists all changed files |
| Manifest updated | grep for the change description in the manifest file |

### For document rounds

| Criterion | How to verify |
|-----------|--------------|
| Named text present | grep for the specific phrase added — count >= 1 |
| No prior content removed | diff against prior version — deletions require justification |
| Maintenance status updated | grep for round identifier in DOCUMENT-ADAPTATION-ANALYSIS-v1.md |

### For training set pairs (Thread 3)

| Criterion | How to verify |
|-----------|--------------|
| B is full output, not description | peer review — B must be text the node would actually produce |
| Rephrasing rule satisfied | every pair appears at least twice with different phrasing |
| Simulation check names missing parts | grep for "missing" in 10-SIMULATION-CHECK.md — count >= 1 |
| Node-00 section of examination method was read first | stated in session record |

---

## Key numbers to carry

| Item | Value |
|------|-------|
| Current test count | 985/985 |
| Baseline for next session | 985 (read live before starting) |
| Locked decisions | D-01 through D-15 — all present in DECISIONS-LOCKED.md |
| Confidence threshold (examiner and KNN guard) | 0.50 |
| Feature vector width | 60 slots |
| Predicate vocabulary size | 22 items |
| LearningEventScheduler DPO threshold | 5 pairs |
| Rollback gate error code | ACCURACY_NOT_IMPROVED |
| Training pair files produced | 0 — Thread 3 not yet started |
| E51A result | 27 training / 13 test / k=5 / 100% domain accuracy / 100% trace success |
| E51B result | 12 teaching / 4 held-out / 2 challenges / 6 of 6 trace success |
| E52 result | 4 of 4 cycle gates passed / 0% baseline / 100% enhanced |
| Round 07 adapter test count | 4 of 4 (ExternalAiResultAdapter filter) |

---

## File locations

### Code repository: `C:\Projects\llm_mvp_core`

```
docs/DECISIONS-LOCKED.md
docs/SUBPROJECT-MAP.md
docs/EXPERIMENT-RESULTS-MAP.md

22.AgentAlgorithms/Pipeline/InternalFirstAiSupport.cs   ← Rule 3 violation, round 07 blocker
Tests/AgentAlgorithmsTests.cs                           ← 3 new adapter tests (round 07)
Tests/AgentInfrastructureTests.cs
Tests/OssEquivalentsTests.cs
Tests/DynamicFlowE51Tests.cs
Tests/AutonomousLearningTests.cs
Tests/ContinuousLearningTests.cs
```

### Dynamic flow working folder: `C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\`

```
DECISIONS-LOCKED.md                              — dynamic flow copy, D-01 through D-15
DYNAMIC-FLOW-ARCHITECTURE-PLAN-v1_0-DRAFT.md    — current
DYNAMIC-FLOW-HIGH-LEVEL-DESIGN-AMENDMENT-v1.md  — current
DYNAMIC-FLOW-CANONICAL-INTEGRATION-SEQUENCE-v1.md — needs one sentence (Blocker 2)
ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md           — current
EXTERNAL-AI-KNOWLEDGE-EDGE-AMENDMENT-v1.md      — needs one paragraph (Blocker 2)
PROMPT-AND-DATASET-LOGICAL-STRUCTURE-EXAMINATION-v1.md — current
EXAMINATION-METHOD-FINAL.md                     — current, authoritative pair-writing method
FLOW-01-TRAINING-DATASET-PLAN-v4.md             — current
DOCUMENT-ADAPTATION-ANALYSIS-v1.md              — needs two entries (Blocker 2)
dynamic-flow-v2-node-topology.csv               — current (N00, N03B added)
dynamic-flow-high-level-topology-v2.mmd         — current (logical compiler, budget gate added)
```
