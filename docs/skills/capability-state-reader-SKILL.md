# Capability State Reader

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

Starting a session to build an interface that already exists wastes time.
Starting a session to design a new capability without knowing that a prerequisite
interface is missing causes execution failures in Phase 1. Planning a new phase
without a baseline of what already works means building on incorrect assumptions.
This skill defines the snapshot protocol that runs at the start of every session
that extends or connects to existing capabilities.

---

## When to Invoke

- At the start of every session that adds capabilities to an existing subproject
- Before planning a new phase that depends on previously built components
- When prerequisite-chain-SKILL.md needs to verify predecessor states
- When Luba asks "what's the current state of X?"

One capability snapshot before planning = zero "built on a missing interface" failures.

---

## Section 1 — The 4-Step Snapshot Protocol

### Step 1 — Query Interface Registry

Read the current state of all fabric interfaces:

```bash
# Check which interfaces are defined
grep -rn "^public interface I" src/ --include="*.cs" | sort

# For each interface, check if it has a concrete implementation
grep -rn "implements\|: I[A-Z]" src/ --include="*.cs" | sort
```

Classify each interface as:
- **ACTIVE** — interface defined, concrete class exists, registered in DI
- **STUB** — interface defined, concrete class exists with NotImplementedException methods
- **MISSING** — referenced in a plan but no file found

---

### Step 2 — Query Test Coverage

Read the current test counts per subproject:

```bash
dotnet test --list-tests 2>/dev/null | grep -c "Test"
# Or per-subproject:
grep -rn "\[Fact\]\|\[Theory\]" tests/ --include="*.cs" | wc -l
```

Record as a table: `[SubprojectName]: [N] passing tests`

---

### Step 3 — Query DomainModelRegistry

Read the current capability lifecycle states:

```bash
# Find all capabilities registered in DomainModelRegistry
grep -rn "DomainModelRegistry\|RegisterCapability\|CapabilityStatus" src/ --include="*.cs"
```

Classify each capability by its current status:
- **PROVEN** — confidence ≥ 0.85, observationCount ≥ 10
- **ACTIVE** — in regular use, confidence 0.70–0.85
- **OPTIONAL** — available but not yet confirmed performant
- **STUB** — registered but not yet implemented

---

### Step 4 — Produce Capability Snapshot

```markdown
## Capability Snapshot — [SessionId] — [Date]

### Fabric Interfaces
| Interface | Status | Provider | First Used By |
|---|---|---|---|
| ISessionStorage | ACTIVE | InMemorySessionStorage | Phase 5 |
| IVectorIndex | ACTIVE | BruteForceIndex, HnswIndex | Phase 5 |
| IBm25Scorer | ACTIVE | Bm25Scorer | Phase 5 |
| ISemanticCompiler29 | ACTIVE | SemanticCompilerPipeline | Phase 5 |
| IAgentMessageBus | ACTIVE | InMemoryMessageBus | Phase 2 |
| ICodeReader | ACTIVE | FileSystemCodeReader | Phase 1 |
| ICodeWriter | ACTIVE | FileSystemCodeWriter | Phase 1 |

### Test Baseline
| Subproject | Passing Tests |
|---|---|
| AgentInfrastructure (21) | [N] |
| AgentAlgorithms (22) | [N] |
| StageExplorer (23) | [N] |
| [other subprojects] | [N] |
Total: [N] passing, 0 failing

### DomainModelRegistry Capabilities
| Capability | Status | Confidence | Observations |
|---|---|---|---|
| [CapabilityName] | ACTIVE | 0.72 | 8 |

### Blockers Detected: [N]
| Blocker | Blocks |
|---|---|
| [InterfaceName] STUB — 3 methods NotImplementedException | Phase X start |
```

---

## Section 2 — Blocker Classification

For each identified gap, classify:

| Gap type | Classification | Action |
|---|---|---|
| Interface defined but all methods throw | STUB — BLOCKING if Phase depends on it | Implement before Phase starts |
| Interface referenced in plan but no file | MISSING — BLOCKING | Add to Phase plan as a prerequisite task |
| DI registration missing | WIRING — BLOCKING | Add registration before tests run |
| Test class exists, 0 [Fact] methods | STUB TESTS — NON-BLOCKING for non-dependent phases | Add tests in the same phase |
| Capability OPTIONAL with 0 observations | UNVALIDATED — NON-BLOCKING | Wire ContinuousLearningPipeline in next phase |

---

## Section 3 — Session Start Checklist

After producing the snapshot:

```
□ All BLOCKING gaps identified and listed as Phase prerequisites
□ Test baseline recorded (exact count) — this is the "before" for Gate B comparisons
□ DomainModelRegistry state recorded — this is the "before" for confidence tracking
□ No interface classified as ACTIVE when it is actually STUB
   (check with: grep -n "NotImplementedException" src/[File].cs)
□ No capability classified as PROVEN when observationCount < 10
```

---

## Anti-Patterns

1. **"I know the state from the last session"** — codebase state changes between sessions.
   The snapshot is not optional when it can be automated. Re-read every session.

2. **"The interface must be ACTIVE, we built it in Phase 3"** — "built in Phase 3" is a
   claim. Verify with grep. Phase 3 may have left a stub. Verification takes 30 seconds.

3. **"The test count was 916 last session, it's probably still 916"** — one session of
   debugging can delete a test accidentally. Record the actual current count.

4. **"STUB is fine, we'll implement it this phase"** — STUB status is fine only if the
   plan includes "implement the stub" as an explicit task. Silent assumption that a stub
   works blocks Phase execution at the first test run.

---

## Integration

- prerequisite-chain-SKILL.md uses the snapshot to classify prerequisites as PASS/FAIL/BLOCKING
- design-artifact-completeness-SKILL.md Check 1–5 provides the per-artifact verification that feeds the snapshot
- capability-improvement-cycle-SKILL.md Stage 1 (gap identified) is triggered by MISSING or STUB classifications
- DomainModelRegistry is the persistence layer for the capability status data in Step 3
- confidence-lifecycle-SKILL.md governs the promotion thresholds referenced in Step 3 classification
