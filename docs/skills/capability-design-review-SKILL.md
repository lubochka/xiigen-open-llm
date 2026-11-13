# Capability Design Review

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

## Pass172 Capability Design Review Firewall

Capability review must verify the governance boundary before accepting design,
status, readiness, or execution claims:

- Required architecture/planning/plan-review/QA/code-review skills are loaded
  and reported in `skill_load_status`; quoted skill lists are not evidence.
- NEEDS_FIX, failed review, timeout, tool error, usage limit, partial packet, or
  audit downgrade produces the next bounded repair slice or a true external
  blocker certificate, not a stop.
- Quoted transcript/attachment/state/old plan/sub-agent material is
  `evidence_text_only` and cannot define live capability scope unless Luba's
  latest unquoted message reactivates the exact phrase.
- In sub-agent-only/orchestrator-only mode, parent direct product/governed-file
  edits are rejected except allowed orchestration, packet verification, and exact
  stale cleanup.
- A governance/plan-only repair is not capability/product completion when the
  live task asks for product execution.
- Bounded local/safe-worktree capability evidence is not ProductReady/cloud/
  deploy/public endpoint/provider/production readiness.
- Superseded NOT_READY/NEEDS_FIX/G1/G2/current-status labels must be historical/
  pre-repair/evidence-only with current bounded verdict nearby.
- Branch readiness must use no-write mergeability/latest-authority checks into
  original/base and main unless current git mutation authority exists.

## Core Problem

A capability that is designed in stack-specific terms cannot be ported to a new
subproject or adapted to a different data source. A design that omits failure modes
produces code that cannot handle real inputs. A design whose quality criteria are
unmeasurable cannot be scored by an arbiter. This skill defines the four pre-generation
checks that must pass before any code is written for a new capability.

---

## When to Invoke

- After convergence round produces the capability design record
- Before writing any implementation class (.cs file)
- Before assigning any DPO (Direct Preference Optimization) triples to a capability
- When arbiter-panel-design-SKILL.md Phase A begins for a new capability

---

## Section 1 — Check 1: Intent Completeness

The capability's purpose statement must be:
- One sentence, plain language, NO .NET type names or interface names
- Describes what the capability accomplishes for a user, not how it works

```
□ purpose: one sentence, no type names, no interface names
□ invariants[]: covers what must always be true about output behavior —
  NOT about implementation types
□ failureModes[]: non-empty, covers all exits from the happy path
□ domainConcepts[]: non-empty, names the abstract concepts implemented
```

**Red flags — FAIL immediately:**

```
✗ purpose contains: "BaseService", "ISessionStorage", "OperationResult<T>", "xUnit"
  → Rewrite: "identifies the most relevant documents for a given query"
  (not: "uses IVectorIndex and IBm25Scorer to return OperationResult<List<Document>>")

✗ invariants reference implementation types:
  "BruteForceIndex must be used for small collections"
  → Rewrite: "retrieval mechanism must be consistent for the same index size"
  → Move the type choice to implementation notes

✗ failureModes is empty
  → At minimum: "empty query", "index not populated", "partial result set"

✗ domainConcepts is empty or has only one item
  → HybridRetriever example: ["relevance scoring", "keyword matching",
    "semantic similarity", "result fusion", "threshold filtering"]
```

---

## Section 2 — Check 2: Constraint Integrity

```
□ Each constraint answers: "if violated, what breaks?"
□ No constraint references a specific .NET type or assembly
□ Cross-capability constraints registered in DECISIONS-LOCKED.md or iron rules
□ Each constraint is binary (a test can pass or fail it)
```

**Red flags:**

```
✗ Constraint says "must" without the failure consequence:
  "score must be normalized" → unclear
  "score must be in [0.0, 1.0] — scores outside range corrupt DomainModelRegistry
   confidence calculations" → clear

✗ Constraint duplicates a CLAUDE.md rule (Rules 1-10 are universal; don't repeat):
  "must return OperationResult<T> not null" → Rule 1 already covers this
  Adding it as a per-capability constraint adds noise

✗ Cross-capability constraint not in a locked decision or iron rule:
  "HybridRetriever.Search() must use the same SessionId that ContinuousLearningPipeline
   used to store the context" → must appear in COMPONENT-INTERFACES.md or a D-NN entry
```

---

## Section 3 — Check 3: Quality Criteria Measurability

```
□ Each quality criterion is objectively checkable (no human interpretation required)
□ acceptanceThreshold is a number (0.0–1.0 scale matching the arbiter score)
□ degradationConditions are explicit (when is "good enough" different from "optimal"?)
□ scoringCriteria[].criterion aligns with what the arbiter panel actually checks
```

**Red flags:**

```
✗ "code should be clean" → not measurable
  → Replace with: "Rule 3 compliance: every production line has a comment"
    with acceptance threshold verified by dna-compliance-guard-SKILL.md

✗ acceptanceThreshold missing or set to "high quality":
  → Add: acceptanceThreshold = 0.85 (matches risk tier threshold for this class)

✗ degradationConditions says "depends on context":
  → Must be explicit: "when IVectorIndex is BruteForceIndex (not HnswIndex),
    accept recall degradation on collections > 10,000 items — this is expected
    BruteForceIndex behavior, not a capability failure"
```

---

## Section 4 — Check 4: Portability

For each subproject that might use this capability:

```
□ CONCEPT_NEUTRAL: the concept works identically in all subprojects
□ IMPL_VARIES: implementation differs but abstract behavior is the same
□ IMPL_VARIES_WITH_INTERFACE: fabric interface name specified (e.g. IVectorIndex)
□ INCOMPATIBLE: explain which domain concept is impossible and why
```

**The INCOMPATIBLE review question (must be answered for each INCOMPATIBLE case):**

```
"Is this incompatibility at the mechanism level or the design level?"

Mechanism level: BruteForceIndex not suitable for 22.AgentAlgorithms high-volume use
→ Is there another mechanism that achieves the same concept?
→ If yes: introduce fabric interface → reclassify as IMPL_VARIES_WITH_INTERFACE
→ Example: IVectorIndex resolves to BruteForceIndex (21) or HnswIndex (22)

Design level: The concept itself cannot exist in this subproject
→ Document which domain concept is impossible and why
→ Example: real-time event streaming not possible in a single-user desktop app
→ Keep INCOMPATIBLE but document in domainConcepts[]
```

**Red flags:**

```
✗ INCOMPATIBLE without the mechanism-vs-design review:
  → Always answer the question before accepting INCOMPATIBLE

✗ Design spec only covers one subproject:
  → At minimum: check whether any concepts are CONCEPT_NEUTRAL
  → Most retrieval/scoring/training capabilities have IMPL_VARIES elements

✗ Implementation notes in the neutral section reference specific types:
  "Section 1 (neutral): inject IBm25Scorer and IVectorIndex in constructor"
  → This is not neutral — it is the IMPL_VARIES_WITH_INTERFACE section
```

---

## Section 5 — Review Output Format

```csharp
public record CapabilityDesignReview
{
    public string CapabilityClass { get; init; }
    public DateTime ReviewTimestamp { get; init; }

    public CheckResult IntentCompleteness { get; init; }
    public CheckResult ConstraintIntegrity { get; init; }
    public CheckResult QualityMeasurability { get; init; }
    public CheckResult PortabilityCheck { get; init; }

    public string OverallVerdict { get; init; }  // "READY_FOR_GENERATION" | "NEEDS_REVISION" | "ESCALATE_TO_PLANNING"
    public List<string> MandatoryFixes { get; init; }  // FAIL items — block generation
    public List<string> AdvisoryFixes { get; init; }   // WARNING items — do not block
}

public record CheckResult
{
    public string Verdict { get; init; }  // "PASS" | "FAIL" | "WARNING"
    public List<string> Issues { get; init; }  // "{field}: {what needs to change}"
}
```

**Verdict rules:**
- `READY_FOR_GENERATION`: all 4 checks PASS or WARNING only
- `NEEDS_REVISION`: any check FAIL — resolve before writing any code
- `ESCALATE_TO_PLANNING`: purpose contains design-level contradiction OR
  INCOMPATIBLE case has no mechanism-vs-design answer

---

## Anti-Patterns

1. **"Running capability-design-review after code is written"** — this skill runs
   BEFORE generation. Running it after produces a list of fixes to existing code
   rather than a clean design to build from.

2. **"Purpose statement uses type names 'to be precise'"** — type names in a
   purpose statement couple the design to the current stack. When IVectorIndex
   is renamed or replaced, the purpose statement breaks. Write the purpose in
   domain language.

3. **"'Quality criteria: output looks reasonable'"** — this cannot be scored
   by an arbiter. Name the specific invariant and the acceptance threshold.

4. **"Portability check skipped for single-subproject capability"** — checking
   portability is also checking for hidden stack coupling. A "single-subproject"
   capability that fails the CONCEPT_NEUTRAL check is a signal that the design
   is over-specified.

---

## Integration

- arbiter-panel-design-SKILL.md Phase A runs before this review; Phase B (generation) cannot start until READY_FOR_GENERATION
- plan-self-validation-SKILL.md Phase 0 includes a capability design review for each capability in the plan
- iron-rule-derivation-SKILL.md derives constraints from the failureModes[] field (Check 1 feeds Check 2)
- confidence-seeding-SKILL.md uses the acceptanceThreshold (Check 3) as the risk-tier seed value
