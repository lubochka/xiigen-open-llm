# Domain Event Design (IAgentMessageBus)

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

## What This Skill Prevents

Message type names that don't form a coherent vocabulary. Payload shapes that drop
fields across refactors, breaking downstream subscribers silently. Subscribers that
couple to internal state that should not cross boundaries.

---

## Rule 1 — Naming Convention

All messages follow: `[Entity][PastTenseVerb]Message`

**Entity** = the domain object that changed state.
**PastTenseVerb** = what happened to it.
**Message** suffix = always included (xUnit and C# convention for message types).

```csharp
// ✅ Correct naming
public record TaskAssignedMessage(string TaskId, string AgentId, DateTime AssignedAt);
public record SearchResultsReadyMessage(string SessionId, IReadOnlyList<SearchResult> Results);
public record SessionSavedMessage(string SessionId, int DocumentCount);

// ❌ Wrong naming
public record TaskMessage(...);          // no verb — what happened?
public record TaskEvent(...);            // "Event" suffix — use "Message"
public record TaskAssignment(...);       // noun — not past-tense verb
public record TaskWasAssigned(...);      // "Was" — use simple past
```

For state machine transitions, use the specific state name:
```
TaskCreatedMessage → TaskAssignedMessage → TaskStartedMessage → TaskCompletedMessage
NOT: TaskUpdatedMessage (ambiguous — what update?)
```

---

## Rule 2 — Payload Design

Every message payload carries only what subscribers NEED:

```csharp
// ✅ Correct: stable identifiers, no internal state, enough to act
public record TaskAssignedMessage(
    string TaskId,           // stable reference — subscribers can look up details
    string AgentId,          // stable reference — which agent received the task
    string TaskType,         // needed for routing decisions
    DateTime AssignedAt);    // needed for timeout tracking

// ❌ Wrong: internal state, redundant data, raw mutable objects
public record TaskAssignedMessage(
    int DatabaseRowId,       // internal — unstable if DB changes
    Task TaskObject,         // mutable reference — subscribers can mutate shared state
    byte[] SerializedState); // internal encoding — coupling to implementation
```

**Payload design rules:**

1. **Include stable identifiers, not internal IDs.** Subscribers should receive
   a `TaskId` (stable) not a `DbRowKey` (internal).

2. **Never include mutable objects.** Use `IReadOnlyList<T>`, not `List<T>`.
   Use records, not classes with setters.

3. **Include enough to act without a lookup.** If 80% of subscribers need `TaskType`
   alongside `TaskId`, include both. Subscribers should not synchronously call back
   to look up what they need.

4. **Design for additive evolution.** Future versions add fields. They never remove
   or rename existing fields — that would break existing subscribers.

---

## Rule 3 — Subscriber Count Awareness

When a message type has more than 5 subscribers, it becomes shared infrastructure
and needs careful design.

```csharp
// IAgentMessageBus subscriber registration (conceptual)
messageBus.Subscribe<TaskAssignedMessage>(vectorSearchHandler.HandleAsync);    // subscriber 1
messageBus.Subscribe<TaskAssignedMessage>(loggingHandler.HandleAsync);         // subscriber 2
messageBus.Subscribe<TaskAssignedMessage>(metricsHandler.HandleAsync);         // subscriber 3

// At 5+ subscribers: consider whether a more specific message type is better
// e.g., TaskAssignedForSearchMessage vs TaskAssignedForAnalyticsMessage
```

---

## Rule 4 — Cross-Component Message Contracts

When a message crosses a subproject boundary (22.AgentAlgorithms publishes →
21.AgentInfrastructure subscribes), it is a cross-component contract with stricter rules:

1. **Schema is frozen once a subscriber exists.** After InfrastructureHandler subscribes
   to `TaskAssignedMessage`, the payload cannot be changed without a version bump.

2. **Document in COMPONENT-INTERFACES.md.** Cross-component messages are part of the
   public interface of the publishing subproject.

3. **Include minimum viable payload for all current subscribers.** Do not find the
   lowest common denominator — include what all current subscribers need.

4. **Version when schema changes.** `TaskAssignedMessageV2` not a silent schema change.

---

## Step-by-Step: Design a Message

```
1. Name the entity and verb: Task + Assigned → TaskAssignedMessage
2. Apply naming check: PastTense? Clear entity? "Message" suffix? ✅
3. List who publishes: AgentOrchestrator on task assignment
4. List who subscribes: VectorSearchHandler, LoggingService
5. Subscriber count check: ≤5? ✅
6. Payload: What does each subscriber NEED? Start with minimum, add only required fields
7. Cross-component check: Does it cross a subproject boundary?
   YES → document in COMPONENT-INTERFACES.md, frozen-schema rule applies
8. Immutability: record type with IReadOnly collections? ✅
9. Register: document in COMPONENT-INTERFACES.md if cross-component
10. Verify naming consistency with existing message types in the codebase
```

---

## The Message Vocabulary

Maintain a consistent vocabulary. Before adding a new message type, verify:
- No existing message covers the same state transition
- The name follows Entity+PastVerb+Message pattern
- The granularity is consistent with similar messages in the project

```
Current message vocabulary (example):
  TaskAssignedMessage      ← 22.AgentAlgorithms, task assignment
  SearchResultsReadyMessage ← 22.AgentAlgorithms, search complete
  SessionSavedMessage       ← 21.AgentInfrastructure, persistence complete
```

---

## Anti-Patterns

1. **Generic message names** — `StatusChangedMessage` covers nothing specifically
2. **Mutable payloads** — `List<T>` in a message record can be mutated by subscribers
3. **Including internal IDs** — `DatabaseRowId` in a message crosses internal boundaries
4. **Cross-component messages not documented** — they are public contracts; document in COMPONENT-INTERFACES.md
5. **Removing a field from an existing message** — always additive; use versioning for breaking changes
