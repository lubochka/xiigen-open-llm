# Event Contract Design

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

An event schema without correlation fields produces events that cannot be traced
to a session. An event with PII in the payload leaks user data across subproject
boundaries. A forward event without a compensation schema cannot be rolled back.
A message bus event designed as HTTP endpoint coupling cannot be consumed by
a subscriber without coupling to the publisher's implementation. This skill defines
the minimum required schema for every `IAgentMessageBus` message type.

---

## When to Invoke

- Before writing any new `IAgentMessageBus` message type
- When domain-event-design-SKILL.md encounters a complex event with compensation requirements
- Before wiring any compensation (LIFO rollback) event chain
- When designing events that cross subproject boundaries

---

## Section 1 — Server Event Schema (required structure)

Every IAgentMessageBus server event must have these fields:

```csharp
public record AgentEvent
{
    // Correlation fields — always present, always named the same (MACHINE)
    public string EventType { get; init; }       // e.g. "ScoreUpdated" — const per event type
    public string SubprojectId { get; init; }    // e.g. "22.AgentAlgorithms"
    public Guid CorrelationId { get; init; }     // ties all events in one execution together
    public Guid SessionId { get; init; }         // session scope (replaces tenantId)
    public DateTime Timestamp { get; init; }     // when event was emitted
    public string Source { get; init; }          // "server" | "client"
    public string TraceParent { get; init; }     // W3C distributed trace (for cross-phase debugging)

    // Event-specific payload
    public object Data { get; init; }            // typed per event — no raw object in production code
}
```

**Three correlation fields are MACHINE (always present, always named the same):**
- `CorrelationId` — ties all events in one execution chain together
- `SessionId` — scope isolation (Rule 2: Fabric First applies to session context)
- `TraceParent` — distributed trace for cross-phase debugging

**No PII in any event payload. Ever.**

PII definition for this project: userId from external systems, email addresses,
user display names, IP addresses if not needed for routing.

If a downstream subscriber needs user context, it fetches from ISessionStorage —
the event carries only IDs and capability-level data.

---

## Section 2 — Compensation Event Rule

For every forward server event, generate a compensation event:

```
Forward:      {EventName}
Compensation: {EventName}Reverted | {EventName}RolledBack

The compensation schema is identical to the forward schema
with one additional field in Data:
  public string RollbackReason { get; init; }

EXCEPTIONS (terminal events — no compensation possible):
  - CapabilityPromotedToProven → no compensation (promotion = proven, can only demote by evidence)
  - SessionCompleted → no compensation (terminal state)
  - TrainingRunCompleted → compensation is TrainingRunReverted with specific rollback reason
```

Compensation events wire directly to the LIFO compensation chain defined in
e2e-test-matrix-SKILL.md Section 4 (Compensation scenario category).

---

## Section 3 — SLA Annotation

Every event gate in the message bus must specify timing:

```csharp
public record EventGateConfig
{
    public string EventType { get; init; }      // the event to wait for
    public string? TimeoutDuration { get; init; } // null = synchronous, no timeout
    public string? TimeoutEvent { get; init; }  // emitted if timeout occurs
    public RetryPolicy? RetryPolicy { get; init; }
}

public record RetryPolicy
{
    public string Owner { get; init; }          // "execution-unit" or "caller"
    public int MaxAttempts { get; init; }        // default 3
    public string Strategy { get; init; }       // "exponential_backoff" | "linear"
    public string OnMaxRetries { get; init; }   // "compensate" | "fail" | "escalate"
}
```

Events with no SLA (synchronous, sub-millisecond):
```csharp
new EventGateConfig { EventType = "ScoreComputed", TimeoutDuration = null }
```

---

## Section 4 — Fabric Interface Boundary Annotation

For each IAgentMessageBus subscriber, declare whether the handler is injectable
(customizable per session or experiment) or platform-only (fixed):

```
INJECTABLE:     experiment/session may provide a different handler implementation
PLATFORM_ONLY:  security-critical or DNA-invariant — no customization allowed
```

Example:
```csharp
// IScoreSubscriber: INJECTABLE — experiment may provide custom scoring handler
// ITrainingHandler: PLATFORM_ONLY — training run coordination is security-critical
```

Default for unmarked handlers: PLATFORM_ONLY.

---

## Section 5 — Stack-Neutral Event Contract Check

After designing CONSUMES/EMITS/BOUNDARY for any capability, verify:

```
□ All event type names are string constants — not C# interfaces or framework annotations
  CORRECT: "ScoreUpdated"   WRONG: IScoreUpdatedEvent (interface reference)

□ CONSUMES list references only IAgentMessageBus fabric events — no HTTP calls
  CORRECT: "ScoreUpdated (IAgentMessageBus — from 22.AgentAlgorithms)"
  WRONG: "POST /api/score/update" (HTTP coupling)

□ EMITS list contains only CloudEvents-compatible event names (string constants)
  CORRECT: "CapabilityPromoted"   WRONG: CapabilityPromotedEvent (class name)

□ BOUNDARY uses only "INJECTABLE" or "PLATFORM_ONLY" values
  CORRECT: "IScoreHandler: INJECTABLE"
  WRONG: "IScoreHandler: @Injectable() class ScoreHandler" (implementation detail)
```

If any check fails: produce a gap report before proceeding to implementation.

---

## Section 6 — Event Contract Document Format

Before any message handler implementation, produce this:

```markdown
## Event Contract — [CapabilityClass]

### Server events emitted:
- `ScoreUpdated` — emitted after scoring completes
  Required fields: CorrelationId, SessionId, Timestamp, Source="server", TraceParent
  Data: { Score: double, CapabilityClass: string, ObservationCount: int }
  Compensation: `ScoreReverted` — emitted on rollback
  
- `CapabilityPromoted` — emitted when confidence ≥ threshold
  Required fields: [same correlation fields]
  Data: { CapabilityClass: string, NewStatus: string, FromStatus: string }
  Terminal — no compensation

### Client events consumed (none for this capability — server-driven only)

### SLA:
  ScoreUpdated: synchronous, no timeout
  CapabilityPromoted: post-scoring, no timeout required

### Boundary:
  IScorePublisher: PLATFORM_ONLY — promotion logic is invariant
```

---

## Anti-Patterns

1. **"Event schema includes user email in Data field"** — PII violation. Remove it.
   If the subscriber needs user context, it reads from ISessionStorage.

2. **"No compensation event for ScoreUpdated"** — forward events that modify state
   need a compensation event. Add `ScoreReverted` before implementation.

3. **"Event type = class name reference (IScoreUpdatedEvent)"** — event schemas must
   be stack-neutral string constants. Use `"ScoreUpdated"` not `IScoreUpdatedEvent`.

4. **"CONSUMES list references HTTP endpoints"** — events must cross subproject
   boundaries via IAgentMessageBus, not HTTP. "POST /api/score" is coupling, not events.

5. **"SLA timeout field left empty by default"** — explicitly set `TimeoutDuration = null`
   for synchronous events. Empty implies unknown, which cannot be verified.

---

## Integration

- domain-event-design-SKILL.md governs naming conventions for events defined by this skill
- e2e-test-matrix-SKILL.md Section 4 (Compensation) tests the compensation events designed here
- IAgentMessageBus is the fabric interface that carries these events
- data-flow-integrity-SKILL.md verifies that CONSUMES/EMITS schemas are compatible
- event-driven-debugging-SKILL.md diagnoses broken event chains at runtime
