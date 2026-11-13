# Session State Crystallization

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

## When to Invoke

At the end of every planning, investigation, or analysis session — before reporting complete.
The snapshot is the LAST deliverable of every session.

Code sessions (Claude Code) produce a commit and STATE-Pn.json update instead —
the crystallization step is the STATE-Pn.json write.

---

## The ≤40-Line Rule

The snapshot must be ≤40 lines. Hard cap, not a guideline.

If you cannot fit session state in 40 lines:
1. You are repeating information already in a prior snapshot
2. Write only what CHANGED since the last snapshot
3. Cut anything derivable from a command (test count from dotnet test, not memorized)

A 200-line snapshot is a documentation replacement, not a snapshot.
The snapshot must stay lean enough that the next session reads it in 30 seconds.

---

## Snapshot Format

File: `docs/SESSION-STATE-SNAPSHOT--[YYYY-MM-DD].md`
Replaces: any prior snapshot (only the latest is active)

```markdown
# SESSION-STATE-SNAPSHOT — [YYYY-MM-DD]
# Session type: [PLANNING | INVESTIGATION | CODE | ANALYSIS]
# Produced by: [brief session description]

## EXECUTION POSITION
Next work:    [one sentence — the next executable action]
Can start:    [YES | NO — if NO, list what is blocking]

## BLOCKING PREREQUISITES
[For each blocker: description + verify command + expected result]
Example:
  BruteForceIndex SearchRange method must be merged first
  Verify: grep -n "SearchRange" Src/09.VectorSearch/IVectorIndex.cs
  Expected: line present

## ACTIVE BUGS
[BUG-NN | one-line description | status: OPEN/FIXED/DEFERRED]
Example:
  BUG-01 | BruteForceIndex zero-vector returns wrong result | FIXED — cosine guard added
  BUG-02 | HybridRetriever NaN on high BM25 score | OPEN

## ARTIFACT BOUNDARIES
Next subproject:  [NN — next number in sequence]
Test count:       [N — run: dotnet test --no-build 2>&1 | tail -3]
Last commit:      [hash] [message first line]

## SUBPROJECT STATUS
[NN.Name | status: COMPLETE/ACTIVE/PENDING/BLOCKED | one-line reason if not COMPLETE]
Example:
  09.VectorSearch | COMPLETE | BruteForceIndex + HnswIndex, 24 tests passing
  12.HybridRetriever | ACTIVE | Integration tests pending
  15.SemanticCompiler | BLOCKED | Waiting for 12 to complete

## DECISIONS MADE THIS SESSION
[one sentence per decision — only decisions made IN THIS SESSION]
Example:
  D-2026-05-01-1: Use HNSW for all vector indexes >10k entries (D-12 in DECISIONS-LOCKED.md)
  D-2026-05-01-2: ISessionStorage serializes to JSON, not binary, for human readability

## SKILLS LOADED
Skills consulted this session: [list]
```

---

## How to Produce Each Field

### EXECUTION POSITION
From: the last explicitly decided "what to do next" in this session.
Not from memory — state what was decided in this session only.
If not decided: write "UNKNOWN — determine at next session start."

### BLOCKING PREREQUISITES
Only unmet prerequisites. Copy the verification command literally.
A paraphrased command cannot be run at the next session start.

### ACTIVE BUGS
From: the BUG-LOG or the current session's bug discoveries.
FIXED = fixed in this session (include the fix method).
DEFERRED = explicit decision to defer (include the reason).
OPEN = unresolved.

### ARTIFACT BOUNDARIES
Run fresh at the end of the session — never from memory:
```bash
dotnet test --no-build 2>&1 | tail -3
git log --oneline -1
```

### SUBPROJECT STATUS
From: what was explicitly verified in this session.
A subproject is COMPLETE only when dotnet test shows all its tests passing.

### DECISIONS MADE THIS SESSION
ONLY decisions made in this session. Historical decisions go in DECISIONS-LOCKED.md.
Format: `D-[YYYY-MM-DD]-[N]: [one sentence]`

---

## The Gate

A session cannot claim complete without producing this snapshot.

Line count check before reporting done:
```bash
# Count lines in the snapshot (Windows PowerShell)
(Get-Content "docs\SESSION-STATE-SNAPSHOT--$(Get-Date -Format 'yyyy-MM-dd').md").Count
# Expected: ≤ 40
# If > 40: rewrite with only what changed since last snapshot
```

The snapshot replaces:
- Manually maintained "current state" notes
- Hard-to-find information about session execution position

The snapshot does NOT replace:
- STATE-Pn.json (code session phase state — different purpose)
- DECISIONS-LOCKED.md (full decision records with rationale)
- COMPONENT-INTERFACES.md (full interface reference)

---

## Anti-Patterns

1. **Snapshot > 40 lines** — rewrite with only what changed
2. **Verification commands paraphrased** — copy literal commands; paraphrases cannot be run
3. **"No decisions made this session"** — every session makes at least one decision; if genuinely none, write "No architectural decisions — [session type] session"
4. **Snapshot produced after session claims complete** — the snapshot IS the last step; produce it before claiming done
5. **Copying subproject status from the last snapshot without verifying** — status must come from what was actually checked in this session
