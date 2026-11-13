# System Intake

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

A design session that starts without extracting the existing system's structure,
intent, constraints, and quality gaps will propose a new capability that violates
existing invariants. This is the most common source of re-work: the design was
locally coherent but globally wrong because the designer didn't know what the
system already enforces. This skill defines the four properties to extract and
the extraction protocol for each source type, producing a domain context package
that feeds capability design.

---

## When to Invoke

- Before designing any new capability for an existing subproject
- Before writing a new phase plan that extends an existing service
- Before running capability-design-review-SKILL.md on a new capability
- Before capability-decomposition-SKILL.md on a capability that touches existing code

---

## Section 1 — Four Properties to Extract

Regardless of source type, every intake extracts the same four properties:

```
structure:   How the system is organized.
             Service class boundaries, dependency graph, entry points, fabric interfaces.
             Not file names — the conceptual architecture.

intent:      What the system does for users / tenants.
             Business rules, invariants, domain concepts (not what the code does —
             what the business requires of the code).

constraints: What must always be true.
             Performance bounds, data invariants, OperationResult<T> boundaries.
             Explicit (documented in CLAUDE.md/COMPONENT-INTERFACES.md) +
             implicit (inferred from guard clauses in existing code).

quality:     How well the system currently serves its intent.
             Known gaps, areas where behavior diverges from intent,
             brittle spots, missing test coverage.
```

---

## Section 2 — Extraction Protocol by Source Type

### C# Source Code (this project's primary source type)

```bash
# Step 1: Find entry points
grep -rn "public.*Result\|public.*Async" src/ --include="*.cs" | grep -v test
# These tell you what the service responds to

# Step 2: Map service boundaries
grep -rn "interface I\|: BaseService\|: I[A-Z]" src/ --include="*.cs"
# Look for: interfaces, BaseService inheritors, fabric injections

# Step 3: Extract business rules (guard clauses)
grep -rn "OperationResult.*Fail\|throw new\|if.*return.*Fail" src/ --include="*.cs" | grep -v test
# Each OperationResult.Fail() is a constraint

# Step 4: Identify event contracts
grep -rn "IAgentMessageBus\|Publish\|Subscribe" src/ --include="*.cs" | grep -v test
# These are the event/message contracts

# Step 5: Find quality signals
grep -rn "TODO\|FIXME\|HACK\|throw new NotImplementedException\|// stub" src/ --include="*.cs"
# Signals of known gaps or incomplete implementation
```

### Interface and Design Documents

```bash
# Read COMPONENT-INTERFACES.md sections for this subproject
grep -n "##.*[SubprojectNumber]\|##.*[CapabilityName]" docs/COMPONENT-INTERFACES.md

# Read locked decisions relevant to this capability
grep -in "[capability class]" docs/DECISIONS-LOCKED.md
```

### Legacy Code (older C# patterns without BaseService or OperationResult)

```
Step 1: Find the entry points (public methods)
Step 2: Identify data structures (what types are passed between methods)
Step 3: Extract business procedures (each public method is a candidate task type)
Step 4: Find hardcoded constants
  → Classify each: MACHINE (must stay fixed) or FREEDOM (should be configurable)
Step 5: Identify external dependencies
  → File I/O, database calls, any hard-coded endpoint strings
Step 6: Map procedures to modern capability archetypes
  → Input/output transformation → DATA_PIPELINE
  → Decision routing → ROUTING
  → Long-running process → SCHEDULED
  → Orchestrating multiple steps → ORCHESTRATION
```

---

## Section 3 — Domain Context Package Format

After extraction, produce a domain context package:

```csharp
public record DomainContextPackage
{
    public string SourceType { get; init; }    // "csharp" | "interface_docs" | "legacy" | "mixed"
    public List<string> SourceArtifacts { get; init; }   // file paths read

    public StructureRecord Structure { get; init; }
    public IntentRecord Intent { get; init; }
    public ConstraintsRecord Constraints { get; init; }
    public QualityRecord Quality { get; init; }
}

public record StructureRecord
{
    public List<string> ServiceBoundaries { get; init; }  // e.g. "WeightedScoringService owns scoring only"
    public List<string> Dependencies { get; init; }       // "WeightedScoring depends on IVectorIndex"
    public List<string> EntryPoints { get; init; }        // public method signatures
    public List<string> FabricInterfaces { get; init; }   // which interfaces are injected
}

public record IntentRecord
{
    public string Summary { get; init; }               // one sentence: what does this do?
    public List<string> BusinessRules { get; init; }   // invariants as plain-language sentences
    public List<string> DomainConcepts { get; init; }  // abstract concepts implemented
    public List<string> Invariants { get; init; }      // what must always be true about output
}

public record ConstraintsRecord
{
    public List<string> Explicit { get; init; }  // documented in CLAUDE.md / COMPONENT-INTERFACES.md
    public List<string> Implicit { get; init; }  // inferred from guard clauses
    public List<string> Unknowns { get; init; }  // questions requiring human judgment
}

public record QualityRecord
{
    public List<string> Gaps { get; init; }             // known functionality gaps
    public List<string> KnownFailures { get; init; }    // things that break silently
    public List<string> BrittleAreas { get; init; }     // areas likely to break on change
    public List<string> MissingCoverage { get; init; }  // untested paths
}
```

---

## Section 4 — Minimum Viable Intake

When time is constrained, produce at minimum:

```
□ Intent.Summary (one sentence — what does this capability do for the system?)
□ Intent.Invariants (3-5 most important things that must always be true)
□ Constraints.Implicit (what the code enforces that docs don't state explicitly)
□ Quality.KnownFailures (what breaks silently today)
```

A minimal intake is better than no intake. It prevents the most common mistake:
proposing a capability architecture that violates an existing invariant.

---

## Section 5 — CONTEXT_INSUFFICIENT During Intake

If the extraction cannot answer a question required for capability design:

```
Request type table:
  Missing interface contract from another subproject  → DOWNSTREAM_CONTRACT
  Missing API contract from external dependency      → EXTERNAL_CONTRACT
  Ambiguous business rule requiring a decision       → HUMAN_JUDGMENT (route to Luba)
  Conflicting documentation and code behavior        → SPEC_CONFLICT (document both, flag)
```

Do not guess. Do not assume. Emit the CONTEXT_INSUFFICIENT signal from
pipeline-position-check-SKILL.md Section 2 and wait.

---

## Anti-Patterns

1. **"Starting to design before completing the intake"** — "I'll figure out the constraints
   as I go" produces re-work when a violation surfaces in Gate B. Intake first. Design second.

2. **"Intake reads only the happy path"** — the most important constraints are in error
   handling and guard clauses. Every OperationResult.Fail() is a constraint.
   Read the failure paths first.

3. **"Intent.Summary describes the implementation"** —
   "The service uses IVectorIndex and IBm25Scorer" is structure, not intent.
   "The service retrieves the most relevant documents for a given natural language query" is intent.
   Domain-language intent, not stack-language description.

4. **"Skipping Quality section"** — known failures inform iron rules.
   Brittle areas become FREEDOM config targets. The Quality section is where the most
   valuable design decisions originate.

5. **"Treating COMPONENT-INTERFACES.md as ground truth without reading the code"** —
   the interface spec describes intended behavior. The code describes actual behavior.
   When they conflict: the code is truth. Note the conflict in Constraints.Unknowns.

---

## Integration

- context-package-design-SKILL.md uses the domain context package as its DOMAIN field input
- capability-design-review-SKILL.md Check 1 (intent completeness) builds on the intake's intent
- iron-rule-derivation-SKILL.md derives iron rules from Intent.Invariants and Quality.KnownFailures
- claim-as-hypothesis-SKILL.md verifies claims extracted during intake before acting on them
- pipeline-position-check-SKILL.md CONTEXT_INSUFFICIENT signal format applies to intake gaps
