# Scope Isolation Arbiter

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

In a multi-user or multi-session system, code that writes shared state
without a session filter corrupts other sessions' data. Code that reads
global state as if it were session-scoped produces wrong results. The
difference between PRIVATE, SHARED, and PLATFORM scope is not always
visible from the data structure alone — it requires a deliberate
classification at design time. This skill defines the three-tier scoping
model, the mandatory arbiter role for every evaluation panel, and the
rules for mixed-scope index reads.

---

## When to Invoke

- Before implementing any new service that reads or writes ISessionStorage
- Before adding a new capability to DomainModelRegistry
- When designing a shared data structure that multiple subprojects will access
- As a mandatory arbiter role in every multi-reviewer evaluation panel
  (arbiter-panel-design-SKILL.md Role 5 — Scope Arbiter)
- When a test passes in isolation but fails when run alongside other tests

---

## Section 1 — Three-Tier Scoping Model

Every data access in this project falls into one of three tiers.

### Tier 1 — PRIVATE (session-scoped)

Data that belongs to one session only. No other session may read or write it.

```csharp
// ✅ PRIVATE — keyed by SessionId, never shared
var result = await _storage.GetAsync<SessionData>($"session:{sessionId}:data");
var write  = await _storage.SetAsync($"session:{sessionId}:data", value);

// ❌ NOT PRIVATE — key has no session component
var result = await _storage.GetAsync<SessionData>("global:data");
```

**Arbiter check:** Does the storage key include SessionId or a session-derived prefix?
If no → classify as SHARED or PLATFORM (not PRIVATE).

### Tier 2 — SHARED (cross-session readable, per-session write)

Data that any session may read, but only the owning session may write.

```csharp
// ✅ SHARED read — any session may query capability edges
var edges = await _domainRegistry.GetEdgesAsync(capabilityClass);

// ✅ SHARED write — session writes its own observation (keyed by sessionId)
await _domainRegistry.RecordObservationAsync(sessionId, capabilityClass, outcome);

// ❌ NOT SHARED — updating a global counter without a session filter
await _domainRegistry.IncrementGlobalCounterAsync(capabilityClass);
```

**Arbiter check:** If a write method has no SessionId parameter, it must be
reviewed before use. Global increments without session attribution corrupt
the learning signal.

### Tier 3 — PLATFORM (global, no session filter)

Data that belongs to the platform, not any session. Immutable at runtime or
written only at startup/migration.

```csharp
// ✅ PLATFORM — capability class registry loaded at startup
var classes = _capabilityRegistry.GetAllCapabilityClasses();

// ✅ PLATFORM — schema version checked once at boot
var version = _schemaVersion.Current;

// ❌ NOT PLATFORM — writing during a request handler
_capabilityRegistry.Register(newClass);  // only allowed at startup
```

**Arbiter check:** If a PLATFORM object is written during a request handler
(not startup): BLOCK — this is a race condition in disguise.

---

## Section 2 — Mixed-Scope Index Reads

Some reads require combining SHARED and PRIVATE data (e.g., retrieving
capability edges while also reading session-specific overrides).

```csharp
// Mixed-scope read pattern:
// 1. Read SHARED baseline first
var baseline = await _domainRegistry.GetEdgesAsync(capabilityClass);

// 2. Read PRIVATE session override (if any)
var sessionKey = $"session:{sessionId}:override:{capabilityClass}";
var overrideResult = await _storage.GetAsync<ConfidenceOverride>(sessionKey);

// 3. Merge: session override wins if present; baseline otherwise
var effective = overrideResult.IsSuccess
    ? overrideResult.Value
    : baseline;
```

**Rule:** In a mixed-scope read, PRIVATE always shadows SHARED. Never merge
by averaging — shadowing (last-write-wins) is the correct semantic.

**Arbiter check:** Does the read method document which scope takes precedence?
If not: BLOCK — ambiguous merge semantics corrupt session isolation.

---

## Section 3 — Arbiter Role in Evaluation Panels

The Scope Isolation Arbiter is a mandatory role in every multi-reviewer panel
(arbiter-panel-design-SKILL.md).

```
Scope Arbiter receives:
  - The new or modified code being evaluated
  - The storage key patterns used in the code
  - The ISessionStorage and DomainModelRegistry interface definitions
  - The three-tier scoping model (this document, Section 1)

Scope Arbiter does NOT receive:
  - Business logic iron rules (that is Business Logic role's domain)
  - Security vulnerability patterns (that is Security role's domain)
  - Other reviewers' verdicts

Scope Arbiter verdict format:
  VerdictClass: BLOCK | PASS
  Violations: list of ScopeViolation records
  Passed: list of every scope check run (not just violations)
```

```csharp
public record ScopeVerdict
{
    public string VerdictClass { get; init; }       // "BLOCK" or "PASS"
    public List<ScopeViolation> Violations { get; init; }
    public List<string> Passed { get; init; }        // every check, not just violations
}

public record ScopeViolation
{
    public string Tier { get; init; }              // "PRIVATE" / "SHARED" / "PLATFORM"
    public string Rule { get; init; }              // which scope rule was violated
    public string Evidence { get; init; }          // line number + code snippet
    public string Severity { get; init; }          // always "BLOCK" for scope violations
}
```

---

## Section 4 — Scope Declaration (Required on Every New Service)

Every new service class that accesses ISessionStorage or DomainModelRegistry
must include a scope declaration comment at the class level:

```csharp
// Scope:
//   Reads:  SHARED (DomainModelRegistry capability edges — any session)
//   Writes: PRIVATE (session:{sessionId}:score:{capabilityClass})
//   Platform: none
public class WeightedScoringService : BaseService
{
    ...
}
```

**Arbiter check:** Is the scope declaration present and accurate?
Accurate = each Read/Write matches the actual key patterns in the code.
If declaration is absent: BLOCK.
If declaration is inaccurate: BLOCK.

---

## Section 5 — Isolation Test Pattern

Every service that mixes PRIVATE and SHARED scope must have an isolation
test that verifies session A's writes do not affect session B's reads:

```csharp
[Fact]
public async Task SessionA_Write_DoesNotAffect_SessionB_Read()
{
    // Arrange: two independent sessions
    var sessionA = Guid.NewGuid().ToString();
    var sessionB = Guid.NewGuid().ToString();

    // Act: session A writes private data
    await _service.RecordAsync(sessionA, capabilityClass, outcome);

    // Assert: session B reads shared baseline (unaffected by A's write)
    var resultB = await _service.GetAsync(sessionB, capabilityClass);
    Assert.Equal(baseline, resultB.Value);
}
```

If the service modifies SHARED data without session attribution, this test
will fail. That failure is the correct outcome — it exposes the scope violation.

---

## Anti-Patterns

1. **"Storage key has no SessionId — but it's 'logically' per-session"** —
   a key without a SessionId component is SHARED or PLATFORM regardless of
   intent. The arbiter checks keys, not intentions.

2. **"Mixed-scope read merges by averaging"** — PRIVATE shadows SHARED.
   Averaging leaks session A's data into session B's view.

3. **"PLATFORM object written during request handling"** — PLATFORM objects
   are immutable at runtime. Writing them during a request creates a race
   condition that xUnit's parallel test runner will expose immediately.

4. **"Scope declaration says PRIVATE but key has no SessionId"** — the
   declaration is checked against the actual key patterns. A mismatch is
   a BLOCK regardless of the declared intent.

5. **"Isolation test omitted — scope isolation is 'obvious'"** — the test
   documents the invariant and will catch future regressions. Write it.

---

## Integration

- arbiter-panel-design-SKILL.md Role 5 (Scope Arbiter) is the runtime instance of this skill
- dna-compliance-guard-SKILL.md Rule 2 (Fabric First) covers interface injection; this skill covers data scope
- BaseService(ISessionStorage) provides the storage fabric; scope classification governs how it's used
- e2e-test-matrix-SKILL.md Category 6 (recovery scenarios) includes cross-session isolation tests
- event-contract-design-SKILL.md PII rule is a PRIVATE-scope rule for event payloads
