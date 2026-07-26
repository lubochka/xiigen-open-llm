# Infrastructure Discovery

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

## When to Run

Run before Gate 1 of phase-planning-SKILL.md. This is Gate 0.
Discovery must be fresh — not cached from a previous session.

---

## The 10-Step Protocol

### Step 0 — Pre-Existing Failure Audit

Run the full test suite fresh and record the baseline:

```bash
dotnet test LlmFromScratch.sln 2>&1 | tail -10
```

Record:
```
Pre-existing baseline: [timestamp]
  Total: [N] tests
  Passed: [N]
  Failed: [N]  ← This number is the allowed floor. Any NEW failure = blocker.
  Skipped: [N]
```

If there are pre-existing failures, list them explicitly. They are NOT regressions
for this phase — but any new failure introduced by this phase IS a regression.

### Step 1 — Live Artifact Boundaries

Read the following files completely (Rule 5 — record proof of reading for each):

```
docs/SUBPROJECT-MAP.md     → which subprojects exist, their status
docs/COMPONENT-INTERFACES.md → which interfaces exist with signatures
docs/ARCHITECTURE.md       → dependency graph and module boundaries
CLAUDE.md                  → test baseline, quick reference counts
```

Do not rely on memory. The files may have changed since the last session.

### Step 2 — Code Structure Search

For each subproject referenced in the plan, verify the directory structure:

```bash
# List all .csproj files
find Src/ -name "*.csproj" | sort

# List all public interfaces
grep -rn "public interface I" Src/ --include="*.cs" | sort

# List all class definitions that extend a base class
grep -rn "class.*:.*Base\|class.*:.*Service\|class.*:.*Store" Src/ --include="*.cs"
```

### Step 3 — History Docs

Read docs/DECISIONS-LOCKED.md to understand what has been locked and why.
Check if the plan's approach conflicts with any locked decision.

### Step 4 — Architecture Map

Identify every dependency edge the plan will create or modify:

```
New edge: [ClassA in project X] → [IInterfaceB in project Y]
Existing edge (unchanged): [ClassC] → [IInterfaceD]
Removed edge: [ClassE] no longer depends on [IInterfaceF]
```

Verify these edges are consistent with the module boundaries in ARCHITECTURE.md.

### Step 5 — Read Key Files in Full

Read every file the plan will change, completely (Rule 5).

For each file, record proof of reading — one specific fact from the end of the file:

```
Read: Src/09.VectorSearch/BruteForceIndex.cs
Proof: Last method is Search() at line 87, returns OperationResult<List<SearchResult>>
```

Do not skim. Read the whole file. The end of the file often has the facts that matter most.

### Step 6 — Completeness Map

List every deliverable in the plan and verify it can be found or placed:

```
DELIVERABLE-1: Src/09.VectorSearch/HnswIndex.cs
  Status: Does not exist yet (NEW)
  Directory exists: YES (Src/09.VectorSearch/)
  Interface to implement: IVectorIndex (confirmed at line 3 of COMPONENT-INTERFACES.md)

DELIVERABLE-2: Src/09.VectorSearch/IVectorIndex.cs (modify)
  Status: EXISTS at Src/09.VectorSearch/IVectorIndex.cs
  Current signature: [record from Step 5 reading]
  Plan change: add SearchRange() method
```

### Step 7 — Data Format Audit

For any data structures the plan creates or modifies:

```
Verify:
- OperationResult<T> wraps every return type (Rule 1)
- Record<string,unknown> or concrete C# records used (not anonymous types for API contracts)
- No nullable reference types without explicit handling
```

### Step 8 — Fabric-First Dependency Check

For every class in the plan that has dependencies:

```bash
# Find classes that instantiate concrete types directly
grep -rn "= new [A-Z]" Src/ --include="*.cs" | grep -v "// Fabric:"
```

Any direct instantiation of a fabric type in a non-test file is a Rule 2 violation.
Record them in the Out-of-Scope Issues list for the preflight.

### Step 9 — Rewrite Plan Elements with Real References

After completing Steps 0–8, update every plan reference to include `file:line`:

```
Before: "Add search method to BruteForceIndex"
After:  "Add Search() method to Src/09.VectorSearch/BruteForceIndex.cs:87"

Before: "Update IVectorIndex interface"
After:  "Add SearchRange() to Src/09.VectorSearch/IVectorIndex.cs:14"
```

No plan element should reference a file without a verified path.

---

## Discovery Completion Checklist

```
□ Step 0: Pre-existing failure count recorded with timestamp
□ Step 1: SUBPROJECT-MAP, COMPONENT-INTERFACES, ARCHITECTURE, CLAUDE.md all read
□ Step 2: Directory structure verified — all referenced paths exist
□ Step 3: DECISIONS-LOCKED.md checked — no conflicts
□ Step 4: New dependency edges documented and consistent with architecture
□ Step 5: Every file to be changed has been read completely with proof
□ Step 6: Every deliverable verified as placeable (NEW) or locatable (MODIFY)
□ Step 7: Data formats compliant with Rule 1
□ Step 8: No unrecorded fabric violations in scope
□ Step 9: All plan references updated with real file:line citations
```

Only after this checklist is complete: proceed to Gate 1 (DNA check).

---

## Anti-Patterns

1. **Reading only the relevant section** — Rule 5 requires reading the whole file
2. **Trusting the plan's file paths** — verify every path exists with a directory check
3. **Skipping Step 0** — pre-existing failures must be recorded to distinguish new regressions
4. **Reusing discovery from last session** — code changes between sessions; discover fresh
