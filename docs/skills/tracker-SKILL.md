# Tracker — IProjectTrackerService

## Luba Why / Loophole Repair-First Rule

When Luba asks "why" (or the same in the operator's language, e.g. "почему"), challenges an answer, or says a failure came
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

## What This Skill Does

Provides session observability through a swappable tracker adapter.
Follows Rule 2 (Fabric First): the interface is fixed (MACHINE), the provider is swappable (FREEDOM).

Default is always local-file — no configuration needed.

---

## Interface (MACHINE — never changes)

```csharp
/// <summary>Project tracker adapter — fabric-first, provider is swappable.</summary>
public interface IProjectTrackerService
{
    /// <summary>Create a new tracker card for a session.</summary>
    Task<OperationResult<CardCreated>> CreateCardAsync(
        string phase,       // "P1", "P2", etc.
        string session,     // "session-1", "session-2", etc.
        string title,
        string? description = null);

    /// <summary>Update the status of an existing card.</summary>
    Task<OperationResult<Unit>> UpdateStatusAsync(
        string cardId,
        CardStatus status,  // InProgress, Complete, Blocked
        string? note = null);

    /// <summary>Add a comment to an existing card.</summary>
    Task<OperationResult<Unit>> AddCommentAsync(
        string cardId,
        string comment);
}

public record CardCreated(string CardId);
public enum CardStatus { InProgress, Complete, Blocked }
```

All methods return `OperationResult<T>` — never throw. (Rule 1)

---

## Provider Configuration (FREEDOM — swappable)

| Provider | Config | When to Use |
|----------|--------|-------------|
| `local-file` (default) | No config needed | Always — zero-config start |
| `linear` | `TRACKER_PROVIDER=linear` + `LINEAR_API_KEY` | When Linear is configured |
| `jira` | `TRACKER_PROVIDER=jira` + `JIRA_URL` + `JIRA_TOKEN` | When Jira is configured |
| `github` | `TRACKER_PROVIDER=github` + `GH_TOKEN` + `GH_REPO` | When GitHub Issues is configured |

Default is `local-file`. No environment variable = no external service = sessions still track locally.

---

## Local File Provider

Writes to `.claude/tracker/cards.json`.

```json
{
  "cards": [
    {
      "cardId": "P1-session-1-001",
      "phase": "P1",
      "session": "session-1",
      "title": "Phase 1: VectorSearch implementation",
      "status": "Complete",
      "createdAt": "2026-05-01T10:00:00Z",
      "updatedAt": "2026-05-01T14:00:00Z",
      "comments": [
        "Starting BruteForceIndex implementation",
        "Gate: 924 tests passing, 0 failed"
      ]
    }
  ]
}
```

Human-readable and durable. If the tracker code fails, Luba can read the JSON directly.

---

## Session Usage Protocol

**At session start:**
```csharp
// Create a card for this session's work
var result = await tracker.CreateCardAsync("P1", "session-3", "Phase 1 P3: HnswIndex");
var cardId = result.Value.CardId; // "P1-session-3-001"
```

**During session (at skill start):**
```csharp
// Record which skill is active
await tracker.AddCommentAsync(cardId, "Starting HnswIndex implementation — STEP 3");
```

**At session end (gate passed):**
```csharp
// Mark complete with test gate evidence
await tracker.UpdateStatusAsync(cardId, CardStatus.Complete, "Gate: 924 passed, 0 failed");
```

**At session end (blocked):**
```csharp
// Record the blocker
await tracker.UpdateStatusAsync(cardId, CardStatus.Blocked, "BLOCKED: HnswIndex graph builder not converging on sparse vectors");
```

---

## DI Registration

```csharp
// In Program.cs or module registration
services.AddSingleton<IProjectTrackerService>(provider =>
{
    // Read TRACKER_PROVIDER from environment — defaults to local-file
    var trackerProvider = Environment.GetEnvironmentVariable("TRACKER_PROVIDER") ?? "local-file";

    return trackerProvider switch
    {
        "linear"  => new LinearTrackerService(Environment.GetEnvironmentVariable("LINEAR_API_KEY")),
        "jira"    => new JiraTrackerService(...),
        "github"  => new GitHubTrackerService(...),
        _         => new LocalFileTrackerService(".claude/tracker/cards.json")
    };
});
```

---

## Anti-Patterns

1. **Update status before gate passes** — mark Complete only after `dotnet test` shows Failed: 0
2. **Use tracker as source of truth** — STATE-Pn.json is source of truth; tracker is observability only
3. **Hardcode the provider** — always read from environment variable so the provider is swappable
4. **Skip tracker when blocked** — a BLOCKED status with reason is more useful than silence
