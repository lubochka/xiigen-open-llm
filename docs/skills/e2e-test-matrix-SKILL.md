# End-to-End Test Matrix

## Luba Why / Loophole Repair-First Rule

When Luba asks "why" (or its operator-language equivalent, e.g. "почему"), challenges an answer, or says a failure came
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

A subproject that has unit tests but no end-to-end test scenarios has untested
integration paths. The E2E test matrix defines what the system must do in concrete
scenarios before code is written. Without it, happy-path tests exist but idempotency,
compensation, and time-based behavior are uncovered.

---

## When to Invoke

- When designing tests for any subproject phase
- When writing Phase plans for a new capability
- When a subproject processes external events, timers, or long-running operations
- Before Gate B is declared for any pipeline-stage capability

One test matrix before coding = no "tests pass but the time-based case is untested" results.

---

## Section 1 — The 8 Mandatory Scenario Categories

Every subproject needs all applicable categories. Mark N/A only with justification.

### Category 1 — Happy Path (required, ≥1 per entry point)

**What it verifies:** the capability works end-to-end in the success case.

**Structure:** trigger → all expected outputs in sequence → terminal result

```csharp
[Fact]
public async Task CapabilityName_HappyPath_ReturnsExpectedResult()
{
    // Arrange: valid input
    // Act: invoke the capability
    // Assert: OperationResult.IsSuccess == true, Value matches expected
    //         all intermediate side effects observable
}
```

### Category 2 — Idempotency (required for any capability with writes or state changes)

**What it verifies:** calling the same capability twice with the same input produces
the same result, not doubled effects.

**Structure:** trigger same request twice → assert result appears once, not twice

```csharp
[Fact]
public async Task CapabilityName_CalledTwice_ResultNotDoubled()
{
    // Arrange: identical input with same idempotency key
    // Act: invoke twice
    // Assert: storage shows 1 entry, not 2; second call returns same result
}
```

### Category 3 — Rate Limit / Quota (required for entry-point services with call limits)

**What it verifies:** limit+1 calls produce rejection, not silent double-processing.

**Structure:** send (limit) requests → (limit+1)th → assert OperationResult.Fail with RATE_LIMITED code

### Category 4 — Compensation Chain (required for multi-step operations with rollback)

**What it verifies:** if step N fails, steps 1…N-1 are rolled back in reverse order.

**Structure:** succeed through N steps → fail at step N+1 → assert N-1 rollback events in LIFO order

```csharp
[Fact]
public async Task CapabilityName_FailsAtStep3_RollsBackSteps2And1()
{
    // Arrange: succeed steps 1-2, simulate step 3 failure
    // Act: run the pipeline
    // Assert: rollback fired for step 2 BEFORE step 1 (LIFO)
    //         OperationResult.IsSuccess == false
}
```

**Critical:** LIFO order must be verified. Not just "rollback fired."

### Category 5 — Time-Based / Delayed (REQUIRED for any capability with timers or TTLs)

**What it verifies:** time-dependent behavior fires at the correct time.

**When required:**
- Any capability with a timeout, TTL, or SLA
- Any capability that uses `DateTime.UtcNow` for expiry checks
- Any scheduled/periodic operation

**Structure:** set up state → advance system clock → assert time-based event fires

```csharp
[Fact]
public async Task CapabilityName_ExpiresAfterTtl_EmitsExpiredResult()
{
    // Arrange: create item with TTL = 1 hour
    var createdAt = DateTime.UtcNow;
    // Inject a controlled clock (do NOT use Thread.Sleep or real time)
    _mockClock.AdvanceBy(TimeSpan.FromHours(1).Add(TimeSpan.FromSeconds(1)));
    // Act: check for expiry
    // Assert: OperationResult shows EXPIRED state
}
```

**Rule:** NEVER use `Thread.Sleep` or real-time delays in time-based tests.
Always inject a controllable clock through `IConfiguration` or a mock.

### Category 6 — State Recovery (required for long-running or multi-step operations)

**What it verifies:** if the process is interrupted and resumed, state is correct.

**Structure:** execute through N steps → simulate interruption → resume → assert correct state

```csharp
[Fact]
public async Task CapabilityName_AfterInterruption_RestoresCorrectState()
{
    // Arrange: run through N steps, persist state
    // Act: simulate new session (new service instance, same storage)
    // Assert: recovered state matches expected; no steps are double-executed
}
```

### Category 7 — Boundary Assertions (required for capabilities with cross-component contracts)

**What it verifies:** components that depend on this capability's output get the correct data.

**Structure:** invoke capability → read output → assert consumer receives exact expected data

```csharp
[Fact]
public async Task CapabilityName_Output_MatchesConsumerExpectedShape()
{
    // Assert exact field values that consumers read
    // Not just IsSuccess — the specific values in Value
}
```

### Category 8 — Security Boundary (required for multi-tenant or access-controlled capabilities)

**What it verifies:** access control, tenant isolation, auth enforcement.

**Structure:** request from wrong context → assert OperationResult.Fail with AUTH_FAILED/UNAUTHORIZED code

```csharp
[Fact]
public async Task CapabilityName_WrongTenant_ReturnsUnauthorized()
{
    // Assert OperationResult.IsSuccess == false, ErrorCode == "UNAUTHORIZED"
    // Assert no data leak across tenant boundaries
}
```

---

## Section 2 — Test Matrix Format

Document which categories apply before writing any tests:

```markdown
## Test Matrix — [CapabilityName]

| Scenario | Category | virtualClock | Gate B |
|---|---|---|---|
| Happy path — valid input | 1 (Happy Path) | No | dotnet test → 1 passed |
| Same input twice → single output | 2 (Idempotency) | No | dotnet test → 1 passed |
| Session interrupted → resume | 6 (State Recovery) | No | dotnet test → 1 passed |
| Token expires after TTL | 5 (Time-Based) | Yes — inject mock clock | dotnet test → 1 passed |
| Wrong session context | 8 (Security) | No | dotnet test → 1 passed |

N/A categories with justification:
- Category 3 (Rate Limit): capability has no rate limit — N/A
- Category 4 (Compensation): capability is read-only — N/A
- Category 7 (Boundary): capability is a leaf node — N/A
```

---

## Section 3 — Time-Based Test Protocol

Time-based scenarios require a controllable clock. Never use real time.

**Recommended pattern:**
```csharp
// Inject IDateTimeProvider instead of using DateTime.UtcNow directly
public interface IDateTimeProvider { DateTime UtcNow { get; } }

// In production: reads DateTime.UtcNow
public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow; // reads from system clock
}

// In tests: controllable value
public class MockDateTimeProvider : IDateTimeProvider
{
    private DateTime _current = DateTime.UtcNow;
    public DateTime UtcNow => _current;
    public void AdvanceBy(TimeSpan duration) { _current += duration; }
}
```

---

## Anti-Patterns

1. **"Happy path passes, skip the rest"** — idempotency, compensation, and time-based failures are not visible from the happy path. Each category covers a distinct failure mode.

2. **"Use Thread.Sleep for time-based tests"** — real-time tests are slow, flaky, and non-deterministic. Always inject a controllable clock.

3. **"Compensation is tested by checking the final state"** — LIFO order must be explicitly asserted. A compensation that fires in the wrong order can leave data in an inconsistent state even if the final state looks correct.

4. **"Category N/A without justification"** — every N/A requires a one-line justification. Unlabeled N/A categories hide missing test coverage.

---

## Integration

- user-journey-testing-SKILL.md provides scenario-first testing (write scenarios before implementation)
- improvement-measurement-SKILL.md requires behavioral test evidence — the test matrix is the behavioral layer
- mvp-round-trip-verification-SKILL.md Step 5–6 verifies Category 1 (happy path) and Step 7 verifies Category 7 (boundary assertions)
- plan-self-validation-SKILL.md Check 1 (goal delivery completeness) verifies that the test matrix covers all stated goal elements
