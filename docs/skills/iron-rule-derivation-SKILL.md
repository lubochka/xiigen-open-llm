# Iron Rule Derivation

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

A test that catches the wrong violation wastes cycles. A test that passes
because the rule it checks is vague produces false confidence. Iron rules —
domain-specific behavioral invariants — are only valuable if they are derived
from specific failure modes, stated without implementation detail, and carry
a testable failure consequence. This skill defines the derivation sequence
that produces rules meeting that standard.

---

## When to Invoke

- Before writing any new test class for a capability
- After a capability's failure modes have been identified
- When a test is failing for "obvious reasons" that have never been written down
- When capability-improvement-cycle-SKILL.md Stage 2 (design) produces a new interface

Iron rules derived before implementation = tests that catch the actual failure mode,
not incidental surface behaviors.

---

## Section 1 — The Derivation Sequence

For each new capability, apply these steps exactly:

### Step 1: List failure modes

Start with the capability's stated contract. Then ask:

```
- What happens if this operation runs twice? (idempotency)
- What if storage succeeds but the downstream event does not? (atomicity)
- What if a resource is replaced without releasing the old one? (lifecycle race)
- What if a caller gets data from the wrong tenant? (isolation)
- What if a configuration value changes at runtime? (MACHINE vs FREEDOM)
- What if the operation completes but no one finds out? (observability gap)
- What if two callers request the same resource simultaneously? (concurrency)
```

Write each failure mode as a sentence:
"Caller invokes BM25Score twice with the same input, receiving doubled results."

---

### Step 2: For each failure mode, find the invariant

Ask: "What condition, if always true, would prevent this failure?"

```
Failure:    BM25Score called twice → doubled term frequency counts
Invariant:  Score computation is stateless — same input always produces same output
```

The invariant must be implementation-neutral. No C# types, no class names. It describes
what must be true, not how to make it true.

---

### Step 3: Express as an iron rule

Format:
```
Rule N: [subject] MUST [verb] [condition] — [failure consequence if violated]
```

The consequence clause is mandatory. A rule without a stated consequence cannot
be evaluated — a test cannot detect whether the rule is violated.

Examples:
```
Rule 1: BM25Score MUST be stateless — same input at time T1 and T2 returns identical output
        — violation produces non-deterministic retrieval ranking
Rule 2: Tenant ID MUST be included in every storage query
        — violation returns cross-tenant data to callers
Rule 3: OperationResult.Fail MUST be returned on any validation failure — never throw
        — violation breaks the DNA Rule 1 contract silently at call sites
Rule 4: IConfiguration MUST be the source for all threshold values
        — violation hardcodes a value that operators cannot change without redeployment
```

---

### Step 4: Classify as MACHINE or FREEDOM

**MACHINE:** the invariant must always hold; no configuration can relax it.
Hardcode in tests. Test for presence. Never in appsettings.json.

Example: "Store before enqueue on every state transition" — this cannot be
tenant-configurable because partial failure recovery requires the outbox pattern.

**FREEDOM:** the mechanism enforcing the invariant is configurable.
Read from `IConfiguration` with a safe default.

Example: the rule "rate-limit BM25 calls" is MACHINE, but the specific window
(10 requests/minute vs 100) is FREEDOM. Read from:
```csharp
_config.GetValue<int>("Bm25Scorer:RateLimitWindow", 10)
```

Annotate each rule:
```
Rule 3: Threshold MUST be read from IConfiguration             [MACHINE: rule]
        Key: "Scoring:MatchWeight" default 0.25                [FREEDOM: value]
```

---

## Section 2 — The Quality Test

A well-derived iron rule passes all four checks:

```
□ Can this rule be violated without the system throwing an exception?
  If YES → the rule is important (silent violations are the dangerous ones)
  If NO  → the rule is already enforced structurally; does not need an iron rule

□ Is the rule expressed without C# class names or implementation detail?
  If NO  → rewrite: "score before emit" not "await _scorer.ComputeAsync() before _bus.PublishAsync()"
  The rule must apply regardless of which class implements it

□ Does the rule have a testable failure consequence?
  If NO  → add the consequence: what breaks if the rule is violated?
  Without the consequence, a test cannot verify compliance

□ Does the rule derive from a specific failure mode?
  If NO  → trace it back: which failure does this prevent? If unanswerable,
           the rule may not apply to this domain
```

---

## Section 3 — Rules That Come From DNA

The 10 DNA rules apply universally. Do not repeat them as per-capability iron rules.
Reference them instead:

```
DNA Rule 1 applies: return OperationResult.Fail, never throw
→ State in test: "verifies DNA Rule 1 compliance"
→ Do NOT write a new capability-specific iron rule for this — it is already a universal rule

DNA Rule 2 applies: resolve through interface, not concrete type
→ State in constructor parameter type: verify via test that injected type is an interface
```

Domain-specific rules are the ones that require derivation. Universal rules are DNA.

The DNA rules most likely to need capability-specific expression:
- DNA Rule 1 (OperationResult): what specific error codes must this capability return?
- DNA Rule 2 (Fabric First): which external dependencies must be behind an interface here?
- DNA Rule 10 (No external ML): what computation must be done in pure C# for this capability?

---

## Section 4 — Example Derivation

**Capability:** `IBm25Scorer` / `Bm25Scorer`

| Failure mode | Invariant | Iron rule | Class |
|---|---|---|---|
| Same input scores differently on retry | Score is deterministic | Rule 1: Score MUST be deterministic — same input → same output at any time | MACHINE |
| Weight changes without config key | All weights configurable | Rule 2: Term weights MUST come from IConfiguration — hardcoded weights break operator tuning | MACHINE: rule, FREEDOM: value |
| Cross-tenant documents returned | Tenant scope on every call | Rule 3: Every query MUST include tenant ID — violation returns cross-tenant data | MACHINE |
| Score computation throws on empty input | Graceful empty handling | Rule 4: Empty corpus MUST return OperationResult.Ok with score=0.0 — never throw | MACHINE |

**Result: 4 iron rules derived from 4 distinct failure modes.**
None copied from a template. Each traces to a specific failure consequence.

---

## Section 5 — Iron Rule Test Pattern

Each iron rule produces a [Fact] test that verifies the rule directly:

```csharp
[Fact]
public void Bm25Score_SameInput_ReturnsDeterministicResult()
{
    // Verifies Rule 1: determinism
    var result1 = _scorer.Score("token1 token2", new[] { "doc1 token1", "doc2 token2" });
    var result2 = _scorer.Score("token1 token2", new[] { "doc1 token1", "doc2 token2" });

    Assert.Equal(result1.Value, result2.Value); // same input → same output
}

[Fact]
public void Bm25Score_WeightFromConfig_NotHardcoded()
{
    // Verifies Rule 2: FREEDOM config
    var customWeight = 0.9;
    var config = new ConfigurationBuilder()
        .AddInMemoryCollection(new[] {
            new KeyValuePair<string, string>("Bm25:K1Weight", customWeight.ToString())
        }).Build();
    var scorer = new Bm25Scorer(config);
    var result = scorer.Score("test", new[] { "test doc" });

    // If weight is hardcoded, result does not change; if configurable, it must
    Assert.True(result.IsSuccess);
}
```

---

## Anti-Patterns

1. **"Must handle errors correctly"** — not a rule. What specific error? What handling?
   Correct: "MUST return OperationResult.Fail('EMPTY_CORPUS') when document list is empty."

2. **Copying iron rules from another capability** — different capabilities have different
   failure modes. A rule that applies to IBm25Scorer may not apply to IVectorIndex.
   Derive from this capability's failures, not another's.

3. **Listing DNA rules as iron rules** — DNA Rule 1 is universal. Writing "MUST return
   OperationResult" as Rule 3 for this capability is noise. Reference DNA Rule 1 instead.

4. **Iron rules without failure consequence** — "Token must be managed correctly" passes
   no gate. Every rule must state what breaks if violated.

5. **Implementation-specific rules** — "MUST call `_storage.SetAsync()` before `_bus.PublishAsync()`"
   is a C# detail, not a domain invariant. Correct: "MUST persist state before emitting events."

---

## Integration

- capability-improvement-cycle-SKILL.md Stage 2 (design) produces the failure modes that feed Step 1
- e2e-test-matrix-SKILL.md Category 4 (compensation) and Category 2 (idempotency) often correspond to iron rules derived from those failure modes
- design-artifact-completeness-SKILL.md Check 3 (content specific) verifies that iron rule tests are capability-specific, not generic
- multi-reviewer-design-SKILL.md Security Reviewer verifies that failure modes include security boundary violations
