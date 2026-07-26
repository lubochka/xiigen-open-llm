# Arbiter Panel Design

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

A single evaluator has a single blind spot. A review that uses one set of criteria
at once — correctness AND security AND principles AND completeness — produces
verdicts where the dominant concern crowds out the others. An output that violates
a DNA rule can pass a single-judge review that was focused on business logic.
This skill defines an 8-role specialized panel where each arbiter has exactly one
expertise domain, exactly one context package, and no knowledge of what other
arbiters see or decide.

---

## When to Invoke

- Before authoring any evaluation handler that scores capability output
- Before writing any capability.handler or score.handler node
- When adding a new capability class to the plan
- When a convergence session needs challenger roles defined
- Whenever the INTENT or goal context changes — re-run Role 8 first

---

## Precedence Rule

**Role 8 (Goal Delivery) runs FIRST.** If Role 8 returns BLOCK, Roles 1-7 do not execute.
The panel rejects the plan for coverage reasons before correctness is evaluated.

Scope Isolation (see scope-isolation-arbiter-SKILL.md) also runs as a mandatory
pre-check alongside Role 8 for tenant-facing capabilities.

```
Panel execution order:
  Role 8 (Goal Delivery) RUNS FIRST
  Scope Isolation ALSO RUNS FIRST (for multi-user/tenant capabilities)
  ↓ (only if both PASS or CHALLENGE)
  Roles 1-7 (correctness arbiters)
```

---

## Section 1 — The 8 Arbiter Roles

Each arbiter is a separate evaluation pass. Each receives ONLY its own context
package. No arbiter ever sees another arbiter's verdict or the evaluator that
generated the code being reviewed.

---

### Role 1 — Business Logic Arbiter

**Expertise:** Iron rules + domain event contracts + behavioral invariants for this capability class

**Context package (ONLY these):**
- `contract.ironRules[]` — every iron rule for this capability class
- `node.intent.invariants[]` — business invariants from the design
- Upstream + downstream interface contracts for this capability class

**Verdict class:** must-pass-or-block

**Catches:** Iron rule violations, wrong event emissions, business invariant violations.

**Does NOT catch:** Security vulnerabilities, fabric interface misuse, DNA principle violations, output quality.

---

### Role 2 — Security Arbiter

**Expertise:** Isolation + auth + data exposure + failure modes

**Context package (ONLY these):**
- Rule 1 (OperationResult<T> everywhere — no throws)
- Rule 2 (Fabric First — interfaces, not concrete types)
- `contract.failureModes[]`
- Input validation requirements for this capability class

**Verdict class:** must-pass-or-block

**Catches:** Missing OperationResult<T> at boundaries, concrete type injection bypassing fabric, silent failure paths, data exposure between isolation domains.

---

### Role 3 — Skills and Patterns Arbiter

**Expertise:** Known patterns for this capability archetype

**Context package (ONLY these):**
- Prior implementations of this archetype in this codebase
- `DISTILLED_RULE` entries from DomainModelRegistry for this domain
- Fabric interface registry (which interfaces to use vs which to avoid)

**Verdict class:** challenge-or-pass

**Catches:** Code that reinvents what an existing skill already provides, incorrect fabric interface usage, patterns that contradict established domain conventions.

---

### Role 4 — DNA Compliance Arbiter

**Expertise:** CLAUDE.md Rules 1–10 adherence + BaseService pattern

**Context package (ONLY these):**
- Full text of Rules 1–10 from CLAUDE.md with concrete violation examples
- Fabric interface registry
- The implementation being reviewed

**Verdict class:** must-pass-or-block

**BLOCK threshold:** ANY Rule 1–10 violation is automatic BLOCK.

**Does NOT receive:** Iron rules, RAG patterns, security patterns, capability class name. Isolation is the enforcement mechanism.

---

### Role 5 — Iron Rules Arbiter

**Expertise:** Capability contract iron rules only — one job, no distractions

**Context package (ONLY these):**
- `contract.ironRules[]` as a numbered list
- The implementation being reviewed

**Verdict class:** must-pass-or-block

**Note:** Why separate from Role 1? Role 1 gets domain context (events, invariants) — its job is domain correctness. Role 5 gets only the specific rule strings — its job is verbatim rule implementation. One can pass while the other fails. Both must pass.

---

### Role 6 — Completeness Arbiter

**Expertise:** NODE structure completeness

**Context package (ONLY these):**
- `node.structure.inputShape`
- `node.structure.outputShape`
- `node.intent.failureModes[]`

**Verdict class:** challenge-or-pass

**Catches:** Implementation that handles some inputs but not all declared in the design, failure modes declared but with no handling path in the code.

---

### Role 7 — Test Coverage Arbiter

**Expertise:** xUnit [Fact]/[Theory] coverage completeness

**Context package (ONLY these):**
- Iron rules (one [Fact] per iron rule is required)
- Gate B condition for this phase
- The test file being reviewed

**Verdict class:** challenge-or-pass

**Catches:** Missing [Fact] tests for declared iron rules, Gate B condition with no corresponding test assertion, [Theory] parameters that don't cover boundary conditions.

---

### Role 8 — Goal Delivery Arbiter (RUNS FIRST)

**Expertise:** User-stated goals + plan coverage. Coverage, not correctness.

**Context package (ONLY these — strict two-input isolation is mandatory):**
- User's goal statement verbatim from the session goal context
- Plan's phase list (phase names and 1-line purposes — NOT implementation details)

**Does NOT receive:**
- Iron rules, RAG patterns, security patterns
- Capability class name or archetype
- Contract contents
- Any other arbiter's verdict

The two-input isolation is the feature. A coverage arbiter with broad context
produces verdicts that are hard to audit. Two-input produces verdicts any reviewer
can reproduce from two pieces of paper.

**Verdict classes:**
- APPROVED — every goal element mapped to ≥ 1 phase with a Gate B verification step
- BLOCK-not-covered — ≥ 1 goal element has zero phases assigned
- BLOCK-no-verification — ≥ 1 goal element has phases but no Gate B condition
- BLOCK-wrong-artifact-shape — phase would produce evidence or harness files
  instead of the requested runtime/plugin/model artifact
- CHALLENGE — phase assignments are ambiguous

**Aggregate verdict:** worst per-goal-element verdict. Any BLOCK_* blocks the entire panel.

**Does NOT catch:** Security vulnerabilities, iron rule violations, fabric misuse, any correctness dimension.

**Governed by:** goal-delivery-completeness-SKILL.md

---

## Section 2 — Minimum Panel by Capability Archetype

Every archetype includes Role 8 (Goal Delivery). Role 8 runs first.

| Archetype | Structural pre-checks | Correctness roles | Total |
|---|---|---|---|
| ROUTING | Role 8 | Role 1 + Role 4 + Role 5 | 4 |
| DATA_PIPELINE | Role 8 | Role 1 + Role 2 + Role 4 + Role 5 | 5 |
| VALIDATION | Role 8 | Role 1 + Role 4 + Role 5 + Role 6 | 5 |
| SCORING | Role 8 | Role 1 + Role 4 + Role 5 + Role 7 | 5 |
| TRAINING | Role 8 | All Roles 1-7 | 8 |
| ORCHESTRATION | Role 8 | All Roles 1-7 | 8 |

**Rationale:**
- Role 8 is always-on: every capability advances some session goal — even a ROUTING
  capability must be traceable to a goal element
- Correctness roles are archetype-specific: not every capability needs all 7

---

## Section 3 — Arbiter Config Declaration

Declare in the capability contract before execution:

```csharp
public record ArbiterConfig
{
    // STRUCTURAL PRE-CHECKS (run first)
    public ArbiterRole GoalDelivery { get; init; }     // Role 8 — runs first
    public ArbiterRole? ScopeIsolation { get; init; }  // for multi-user capabilities

    // CORRECTNESS ARBITERS (run if pre-checks pass)
    public ArbiterRole BusinessLogic { get; init; }   // Role 1
    public ArbiterRole? Security { get; init; }        // Role 2 — when required
    public ArbiterRole? SkillsPatterns { get; init; } // Role 3 — when applicable
    public ArbiterRole DnaCompliance { get; init; }   // Role 4 — always included
    public ArbiterRole IronRules { get; init; }       // Role 5 — always included
    public ArbiterRole? Completeness { get; init; }   // Role 6 — when structure complex
    public ArbiterRole? TestCoverage { get; init; }   // Role 7 — always for scored capabilities

    public string BlockSemantics { get; init; }       // "ANY_BLOCK_CLASS_REJECTS"
}
```

---

## Section 4 — Upper Evaluator and Dynamic Panel Expansion

When the base panel cannot resolve a disagreement (2+ roles CHALLENGE the same
output section with conflicting reasons), the upper evaluator activates:

1. Identify the expertise domain causing the conflict
2. If a more specialized role would resolve it: spawn a one-time evaluation
3. Log the new role as a DPO triple: what conflict triggered it, what expertise resolved it
4. If still unresolved: route to Luba (human judgment required)

**Panel growth rule:** If the same expertise domain is spawned 3+ times across
different capabilities, it becomes a candidate for the permanent panel.
Luba decides whether to add it as an additional standard role.

---

## Section 5 - Architect Goal Preservation

For architect and reviewer sessions, Role 8 must treat Luba's full user goal as
the coverage source. Architect cannot cut corners, simplify the user goal, or
replace the requested outcome with a narrower shape because architect errors
have high blast radius across every downstream work order and review.

If the goal is ambiguous, mark CHALLENGE and return to the architect plan
repair path instead of approving a convenient interpretation.

Role 8 must also compare requested artifact class to planned artifact shape. If
Luba asks for extension plugin code, runtime host integration, trainable-unit
code, or project model assets, a plan that produces mostly proof harnesses,
JSON reports, PowerShell scripts, validators, or static checks is
BLOCK-wrong-artifact-shape even when those artifacts have gates.

---

## Anti-Patterns

1. **"One evaluator covers all 5 dimensions"** — single-pass evaluation lets dominant
   concerns crowd out others. Use specialized roles.

2. **"Arbiters share context packages"** — each arbiter receives ONLY its defined subset.
   Overlap = contamination. The DNA Compliance arbiter must not see iron rules.

3. **"Role 8 runs last as a sanity check"** — correctness review of a plan that misses
   user goals is wasted effort. Role 8 runs first. If it BLOCKs, Roles 1-7 do not run.

4. **"Goal Delivery arbiter receives the full plan for context"** — Goal Delivery receives
   ONLY the goal statement verbatim + phase list. More context reduces auditability.
   Two-input isolation is the feature.

5. **"Same panel for every capability"** — ROUTING uses 4 roles, ORCHESTRATION uses 8.
   Adding unnecessary roles inflates evaluation time without adding signal.

---

## Integration

- goal-delivery-completeness-SKILL.md governs Role 8 (Goal Delivery) verdict classes
- ai-pipeline-design-SKILL.md Section 2 — arbiters are roles 1-7 in the AI pipeline
- multi-reviewer-design-SKILL.md provides the template for multi-pass review sessions
- diagnostic-escalation-SKILL.md governs escalation when the upper evaluator cannot resolve
- DomainModelRegistry REQUIRES_ARBITER edges determine which roles are mandatory per archetype
