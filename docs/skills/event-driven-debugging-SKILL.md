# Event-Driven Debugging

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

Debugging a consumer class when the message was never published.
Checking the wrong handler when the real break is two hops downstream.
Spending time on class internals when the IAgentMessageBus subscription shows the break immediately.

---

## Core Principle

Every user-triggering action produces a predictable chain of messages through IAgentMessageBus.
If the final outcome is missing, exactly one of these is true:

1. A message was never published (producer bug)
2. A message was published but no subscriber registered (subscription bug)
3. A subscriber ran but produced wrong downstream output (transformation bug)
4. All messages published but the read path returns wrong state (read/query bug)

The debug session finds WHICH of the four is the case before touching any code.

---

## Step 1 — Define the Expected Chain

Before looking at any log or code, write out the expected message chain:

```
User action: Session search triggered

Expected chain:
  1. AgentOrchestrator publishes TaskAssigned message
  2. VectorSearchHandler subscribes to TaskAssigned → publishes SearchResults
  3. HybridRetriever subscribes to SearchResults → publishes RankedResults
  4. SessionStorage.SaveAsync() called → final state written

Total: 3 messages, 2 subscribers, 1 storage write
```

---

## Step 2 — Find the Break Point (Binary Search)

Do not start at step 1. Start in the middle of the chain.

```csharp
// Add debug tracing to the midpoint subscriber
public async Task HandleSearchResults(SearchResultsMessage message)
{
    // DEBUG: confirm this handler is being reached
    System.Diagnostics.Debug.WriteLine($"DEBUG SearchResults handler: correlationId={message.CorrelationId}, resultCount={message.Results?.Count ?? -1}");

    // ... rest of handler
}
```

Run the scenario. Check debug output:
```
# In test output (use Debug.WriteLine in xUnit — visible with verbosity detailed)
dotnet test --filter "SearchFlow" -- --verbosity detailed 2>&1 | grep "DEBUG"
```

- If midpoint debug line appears → break is in steps 3-4 (after midpoint)
- If midpoint debug line is absent → break is in steps 1-2 (before midpoint)

Apply binary search recursively until the break is isolated to a single step.

---

## Step 3 — Classify the Break

Once isolated to one step (message N sent, message N+1 absent):

### Break Class A — Message Published, Subscriber Not Registered

Message exists in the bus. No subscriber registered for its type.

```bash
# Find all subscribers to the message type
grep -rn "AssignTaskAsync\|IAgentMessageBus\|Subscribe" Src/ --include="*.cs" | grep "SearchResults"
# Empty result = subscriber not registered in DI
```

Fix: Add subscriber registration in DI setup.

### Break Class B — Subscriber Registered, Handler Throws Before Publishing

Subscription exists. The handler receives the message. The handler fails before publishing the next message.

```csharp
// Add try-catch with logging to the handler to find the throw
public async Task<OperationResult<Unit>> HandleSearchResults(SearchResultsMessage msg)
{
    try
    {
        // ... handler body
        return OperationResult<Unit>.Ok(Unit.Value);
    }
    catch (Exception ex)
    {
        // Rule 1 violation — this catch reveals the throw we're hunting
        System.Diagnostics.Debug.WriteLine($"DEBUG handler threw: {ex.Message}\n{ex.StackTrace}");
        return OperationResult<Unit>.Fail(ex.Message, "HANDLER_EXCEPTION");
    }
}
```

Fix: Root-cause-tracing-SKILL.md — find the throw, fix the source.

### Break Class C — Handler Runs, Publishes Wrong Message Type

Handler runs successfully. The next message appears but with the wrong type string.

```bash
# Find all places a message type is published
grep -rn "AssignTaskAsync\|Publish\|new.*Message" Src/ --include="*.cs" | grep "class.*Message"
# Compare the published type name to the subscribed type name — case/name mismatch
```

Fix: Align the published message type name with the subscriber's expected type.

### Break Class D — All Messages Published, Read Returns Wrong State

All messages published. Storage write happened. But the read returns incorrect data.

```csharp
// Verify the storage write happened
var loadResult = await storage.LoadAsync(sessionId);
Debug.WriteLine($"DEBUG state after write: {System.Text.Json.JsonSerializer.Serialize(loadResult.Value)}");
// If state is correct here but the caller sees wrong data: cache or query issue
```

Fix: Not a message-chain problem. Check caching layer or query filter.

---

## Step 4 — Verify Fix Does Not Break Adjacent Chains

After fixing the identified break, run the full message-chain test AND a second scenario:

```csharp
[Fact]
public async Task MessageChain_FullFlow_AllStepsComplete()
{
    // Arrange: fresh in-process DI with real implementations
    var services = BuildRealServiceCollection();
    var orchestrator = services.GetRequiredService<AgentOrchestrator>();
    var traceCapture = new MessageTraceCapture(); // subscribe to all messages

    // Act: trigger the chain
    var result = await orchestrator.SearchMemoryAsync(testQuery);

    // Assert: all expected messages were published (chain verification)
    Assert.True(result.IsOk);
    Assert.Contains("TaskAssigned", traceCapture.MessageTypes);
    Assert.Contains("SearchResults", traceCapture.MessageTypes);
    Assert.Contains("RankedResults", traceCapture.MessageTypes);
}
```

---

## Quick Reference: Break Class by Symptom

| Symptom | Break Class | First Check |
|---------|------------|-------------|
| Message N+1 absent, message N present | A or B | grep for subscriber registration |
| Subscriber registered, message N+1 still absent | B | Add debug.writeline in handler, find throw |
| Message N+1 present but wrong type | C | Compare published vs subscribed type names |
| All messages present but final state wrong | D | Log storage state directly |
| Message N itself absent | Producer bug | Add debug to publisher before publish call |

---

## Anti-Patterns

1. **Starting by reading class internals before checking the chain** — check the chain first
2. **Checking a consumer when the producer never published** — binary search before blame
3. **Adding more tests before isolating which step fails** — classification first, tests after
4. **Fixing the wrong class because it's "nearby"** — the binary search locates the exact step
