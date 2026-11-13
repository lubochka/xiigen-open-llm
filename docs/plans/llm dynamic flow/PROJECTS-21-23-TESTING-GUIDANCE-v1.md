# Projects 21-23 Testing Guidance v1

## Overview

This document defines what to test, how to test it, what constitutes
acceptance, which learning sets to use, which sources to draw from, and
what negative examples are required — for each of the five components
being added to projects 21-23.

The five components, in implementation order:

1. `InternalLlmGateway` — routes to the internal model by domain
2. `InternalResultExaminer` — checks the internal model's own output
3. `call_external_ai` permission gate entry — gated approval for external calls
4. `ExternalAiRequestPackage` and `ExternalAiResultAdapter` — structured bridge to external AI
5. `LearningEventScheduler` — queues and drains DPO pairs at idle time

Each section follows the same structure: what the component does,
what tests to write, learning sets and sources, negative examples,
and the acceptance criteria (Gate B).

The baseline before any work begins: `900/900` passing tests.

---

## Component 1 — InternalLlmGateway

### What it does

Replaces the abstract `ILlmGateway` dependency in `ReactAgentLoop` and
`AgentPipeline` with a concrete implementation that:

1. calls `RouteClassifier` to determine the domain of the request,
2. loads the matching LoRA adapter from `IDomainModelRegistry` for that domain,
3. calls the internal `TextGenerator` (project 09) with the adapter active,
4. returns the response wrapped in `OperationResult<string>`.

The key rule: the internal model is always attempted first. No external
call is made inside this gateway.

### Test file

`Tests/AgentInfrastructureTests.cs` — add to the existing `AgentInfrastructureTests` class.

Use the existing `ScriptedLlmClient` and `InMemorySessionStorage` fakes
already in that file. Add one new fake: `StubDomainModelRegistry` that
returns a named adapter path for each domain.

### Tests to write

**`InternalLlmGateway_CodeQDomain_LoadsCodeQuestionAdapter`**

Setup: a registry with entries for CODE_Q, BUG, PLAN, ARCH, REFACTOR.
Input: `"What does BpeTokenizer.Encode return for an empty string?"`
Assert:
- the gateway calls the registry for domain CODE_Q,
- the adapter path returned matches the CODE_Q adapter path,
- `IsSuccess` is true,
- the response string is non-empty.

**`InternalLlmGateway_BugDomain_LoadsBugAdapter`**

Input: `"The tokenizer crashes on emoji input — why and how to fix it?"`
Assert: registry was queried for BUG domain. (This is the V06 case from
E51A — the compound diagnostic+repair prompt must route to BUG.)

**`InternalLlmGateway_UnknownDomain_FallsBackToGenericAdapter`**

Input: a prompt where `RouteClassifier` returns confidence below 0.30.
Assert: the gateway does not fail — it falls back to a generic adapter
and returns `IsSuccess = true`. A low-confidence result must never throw.

**`InternalLlmGateway_RegistryMissForDomain_StillSucceeds`**

Setup: a registry with no entry for ARCH.
Input: `"Architect a plugin system so tokenizers can be swapped at runtime."`
Assert: `IsSuccess = true`, response is non-empty. The gateway must
degrade gracefully when no domain-specific adapter exists.

**`InternalLlmGateway_DoesNotCallExternalAi`**

Setup: a spy that records whether any external HTTP call was made.
Input: any prompt.
Assert: the spy records zero external calls. This is a hard constraint
test — the internal gateway must never reach outside.

### Learning set for this component

Source: `01.Tokenizer` — BpeTokenizer.cs, RegexSplitter.cs, VocabEntry.cs.

Training pairs: ten examples, two per domain (CODE_Q, BUG, PLAN, ARCH,
REFACTOR). Each pair is a `(prompt, expected_adapter_name)` tuple.

Example pairs:

```
CODE_Q:  "What does Normalize return for camelCase input?"   → "code_q.lora"
BUG:     "The stemmer strips -ing from words under 5 chars"   → "bug.lora"
PLAN:    "Plan the steps to add trigram support"              → "plan.lora"
ARCH:    "Design a plugin interface for swappable tokenizers" → "arch.lora"
REFACTOR:"Simplify FamilyCollapser by removing dead branches" → "refactor.lora"
```

All five adapter names must be registered in the `StubDomainModelRegistry`
before any test runs.

### Negative examples

These prompts must NOT load the wrong adapter:

- `"fix the crash"` — must route to BUG, not PLAN. (The word "fix" is
  a BUG_FIX signal even without "why".)
- `"explain how BPE works"` — must route to CODE_Q, not ARCH. (Explanation
  without design intent stays CODE_Q.)
- `"refactor the encode method to use LINQ"` — must route to REFACTOR,
  not CODE_Q. (Imperative form with a structural change intent.)

Each negative example needs a test that asserts the wrong adapter was
NOT loaded.

### Acceptance criteria (Gate B)

```bash
dotnet test --filter "InternalLlmGateway" --no-build 2>&1 | tail -3
# Expected: 5 passed, 0 failed

grep "code_q.lora\|bug.lora\|plan.lora" \
  Tests/AgentInfrastructureTests.cs | wc -l
# Expected: >= 3  (adapter names appear in test assertions)
```

---

## Component 2 — InternalResultExaminer

### What it does

After the `InternalLlmGateway` returns a response, the examiner runs a
second call to the internal model asking it to evaluate the response
against three criteria:

1. Which required predicates from the request's logical structure are
   present in the response?
2. Which arbiter constraints are satisfied?
3. What is the overall confidence score (0.0 to 1.0)?

It returns an `ExaminationResult` record:

```csharp
public sealed record ExaminationResult(
    double ConfidenceScore,
    IReadOnlyList<string> PresentPredicates,
    IReadOnlyList<string> MissingPredicates,
    string? SuggestedCorrection,   // null when score >= threshold
    FeedbackSignal FeedbackSignal  // Accepted or Edited
);
```

If `ConfidenceScore >= 0.70` (configurable), the signal is `Accepted`.
If below, the signal is `Edited` and `SuggestedCorrection` contains the
examiner's improved version.

### Test file

`Tests/AgentAlgorithmsTests.cs` — add to `AgentAlgorithmsTests`.

Use the existing `ScriptedLlmGateway` fake. The examiner's second call
to the internal model is scripted to return a structured JSON examination
result.

### Tests to write

**`ResultExaminer_HighConfidence_ReturnsAccepted`**

Setup: examiner scripted to return confidence 0.85.
Required predicates: `["NULL_GUARD", "RETURN", "FAIL"]`.
Assert:
- `FeedbackSignal == Accepted`,
- `SuggestedCorrection == null`,
- `ConfidenceScore >= 0.70`.

**`ResultExaminer_LowConfidence_ReturnsEdited`**

Setup: examiner scripted to return confidence 0.45.
Assert:
- `FeedbackSignal == Edited`,
- `SuggestedCorrection` is non-null and non-empty,
- `MissingPredicates` is non-empty.

**`ResultExaminer_AllPredicatesPresent_MaxConfidence`**

Required predicates: `["ITERATE", "ASSIGN", "RETURN"]`.
Response text: contains all three predicate signals.
Assert: `PresentPredicates.Count == 3`, `MissingPredicates.Count == 0`.

**`ResultExaminer_NoneOfThePredicatesPresent_MinConfidence`**

Required predicates: `["NULL_GUARD", "FAIL"]`.
Response text: contains neither.
Assert: `MissingPredicates.Count == 2`, `ConfidenceScore < 0.50`.

**`ResultExaminer_RoutesAcceptedToFeedbackRouter`**

Integration test. Wire examiner output through `FeedbackRouter.Route()`.
Assert: when examiner returns `Accepted`, the feedback router accumulates
one positive `TrainingExample`. When examiner returns `Edited`, the router
accumulates one `DpoPair` with the original response as `rejected` and
the suggested correction as `chosen`.

### Learning set for this component

Source: E51A evidence traces — the ten V-records from
`run-2026-05-04-080712.json`.

Each trace provides:
- `produced_predicates` — what the pipeline actually produced
- `code_predicates` — what was required
- `predicate_overlap` — the ground-truth confidence proxy

Use these to create ten examiner training examples:

```
V01: required=[NULL_GUARD,FAIL,RETURN], produced=[NULL_GUARD,FAIL,RETURN]
     → overlap=1.0 → confidence=1.0 → Accepted
V10: required=[DECLARE,RETURN,BRANCH], produced=[DECLARE,RETURN,BRANCH]
     → overlap=1.0 → confidence=1.0 → Accepted
```

Add four synthetic low-confidence examples where the pipeline returned
the wrong predicates — these become the Edited training cases.

### Negative examples

These behaviors must be rejected by the examiner or trigger Edited:

- Response that contains the required predicates in the wrong order — the
  examiner must check presence, not order, so this should still return
  Accepted. A test that asserts this is also a correctness guard.
- Response that adds predicates not in the required set (over-generation) —
  examiner must not penalize this; score depends on what is present from
  required, not on what extra predicates appear.
- Empty response — examiner must return `FeedbackSignal.Edited` with a
  non-null suggestion, never crash.
- Examiner's own call fails — `InternalResultExaminer` must return a
  degraded `ExaminationResult` with confidence 0.0 and `Edited` signal,
  never propagate the failure as an exception.

### Acceptance criteria (Gate B)

```bash
dotnet test --filter "ResultExaminer" --no-build 2>&1 | tail -3
# Expected: 5 passed, 0 failed

dotnet test --filter "ResultExaminer_RoutesAcceptedToFeedbackRouter" --no-build 2>&1 | tail -3
# Expected: 1 passed, 0 failed
```

---

## Component 3 — `call_external_ai` permission gate entry

### What it does

Adds `call_external_ai` to the `RequiresApproval` tool set in
`PermissionGate`. When the `InternalResultExaminer` produces a
confidence score below 0.50 AND names a specific missing competence
(not just "low confidence"), the agent loop calls
`PermissionGate.Check("call_external_ai", args)` before making any
external call.

The gate returns `RequiresApproval`. The caller presents the request
package to the user. If the user approves, the external call proceeds.
If the user denies, the best internal answer is returned with the gap
noted.

The approval is single-use — it does not grant persistent permission
for the session. Every external call requires its own approval.

### Test file

`Tests/AgentInfrastructureTests.cs` — add to existing `AgentInfrastructureTests`.

### Tests to write

**`PermissionGate_CallExternalAi_RequiresApproval`**

Assert: `gate.Check("call_external_ai", args)` returns
`RequiresApproval`. This is the new entry parallel to `edit_file`.

**`PermissionGate_CallExternalAi_SingleUse_SecondCallStillRequiresApproval`**

Setup: simulate two sequential `call_external_ai` checks.
Assert: both return `RequiresApproval`. The gate must not remember
a prior approval.

**`PermissionGate_CallExternalAi_WithoutMissingCompetence_StillRequiresApproval`**

Setup: args dict has no `missing_competence` key.
Assert: `RequiresApproval` still returned. The gate does not inspect args
for semantic content — that is the examiner's responsibility.

**`PermissionGate_CallExternalAi_WithMaskedSensitiveContext_RequiresApproval`**

Setup: args dict contains a `sensitive_data` key marked for masking.
Assert: `RequiresApproval`. The gate passes the approval decision to the
caller — it does not inspect or modify the args content.

**`AgentLoop_WhenExternalCallRequired_PausesForApproval`**

Integration test. Wire `AgentLoop` with an `InternalResultExaminer` that
returns confidence 0.40 and a missing competence string. Assert:
- the loop emits a `PendingApproval` event (or equivalent signal),
- the loop does not call any external gateway,
- the loop does not return `TASK_COMPLETE` while approval is pending.

### Negative examples — what the gate must not do

- Do NOT auto-approve `call_external_ai`. Any test that calls `Check` and
  asserts `Allowed` for this tool name is a failing test by design.
- Do NOT deny `call_external_ai`. A test that asserts `Denied` is also
  wrong — the tool is valid, it just requires human approval.
- Do NOT make the approval persistent across loop iterations. A test that
  checks two sequential calls and asserts the second is auto-allowed is
  a regression.

### Acceptance criteria (Gate B)

```bash
dotnet test --filter "PermissionGate_CallExternalAi" --no-build 2>&1 | tail -3
# Expected: 4 passed, 0 failed

grep "call_external_ai" \
  21.AgentInfrastructure/Tools/PermissionGate.cs | wc -l
# Expected: >= 1  (the tool name appears in the RequiresApproval set)
```

---

## Component 4 — ExternalAiRequestPackage and ExternalAiResultAdapter

### Post-Round 07 implementation note

Round 07 changed the adapter from a single hardcoded duplicate-account
check into a named arbiter rule table. The implemented adapter receives
the approved package, reads `ArbiterConstraints`, checks only the named
rules it knows, and rejects on the first violated known rule.

The first two implemented rules are:

- `identity_consistency` — rejects repeated provider callbacks that would
  create duplicate accounts.
- `token_safety` — rejects external answers that place session tokens in
  local storage, query strings, URL parameters, or non-httpOnly cookies.

Unknown arbiter names are open-world by default: they do not reject unless
the adapter has an explicit named rule for them. This is intentional so
future constraints can be added with focused tests rather than accidental
blocking behavior.

### What they do

`ExternalAiRequestPackage` is a record that captures everything needed
for an informed approval decision and a structured external call:

```csharp
public sealed record ExternalAiRequestPackage(
    string OriginalRequest,
    string InternalAttempt,
    double InternalConfidence,
    string MissingCompetenceStatement,   // must be named, not "low confidence"
    IReadOnlyList<string> PresentPredicates,
    IReadOnlyList<string> MissingPredicates,
    string RequiredAnswerShape,
    IReadOnlyList<string> ArbiterConstraints,
    string SensitivityLevel              // "public" | "masked" | "blocked"
);
```

`ExternalAiResultAdapter` takes the external AI's natural-language
response and converts it into `PreprocessedLearningData` for the local
learning path:
- `InternalAttempt` = the internal model's original attempt
- `ExternalAnswer` = the approved external answer before adaptation
- `AdaptedAnswer` = either the adapted local answer or the arbiter rejection
- `SignalType` = adapted for DPO learning, or rejected by arbiter authority

Before producing adapted learning data, it checks the package's named
arbiter constraints through the known-rule table. After that, it extracts
local predicates from the compiled logical structure so the learning
record remains connected to the same internal representation used by the
dynamic flow.

### Test file

`Tests/AgentAlgorithmsTests.cs` — add to `AgentAlgorithmsTests`.

### Tests to write

**`RequestPackage_MissingCompetenceIsNamed_NotGeneric`**

Build a package from a CH-01-style input (biometric passport request).
Assert: `MissingCompetenceStatement` contains specific domain terms
("biometric", "passport") — not just "low confidence" or "unknown".

**`RequestPackage_SensitivityLevel_MasksPersonalData`**

Build a package where the original request contains a user email address.
Assert: `SensitivityLevel == "masked"` and the `OriginalRequest` field
in the package does not contain the raw email string.

**`ResultAdapter_ExternalResponseRunsThroughCompiler`**

Setup: a scripted external response that contains known predicate signals
in natural language.
Assert: the adapter extracts at least one predicate from the external
response before producing the `FeedbackEvent`. A response that produces
zero predicates after compilation should emit `Signal = Rejected` rather
than `Edited`.

**`ResultAdapter_ExternalResponseContradicts_LockedDecision_IsRejected`**

Setup: an external response that proposes creating a second account for
a repeated callback (the CH-02 case).
Assert: the adapter's output `FeedbackEvent.Signal == Rejected`. The
`identity_consistency` arbiter fires and blocks the adapted result.

**`ResultAdapter_TokenSafetyConstraint_RejectsUnsafeTokenStorage`**

Setup: an external response that proposes storing a token in local storage,
a query string, a URL parameter, or a non-httpOnly cookie.
Assert: the adapter marks the answer as rejected by arbiter authority and
the rejection message names `token_safety`.

**`ResultAdapter_UnknownConstraint_DoesNotReject`**

Setup: a package contains an arbiter constraint name the adapter does not
yet know, while the answer text would have violated a different known rule.
Assert: the adapter produces adapted learning data rather than an arbiter
rejection. Unknown names are not treated as hidden deny rules.

**`ResultAdapter_ValidExternalResponse_ProducesDpoPair`**

Setup: a valid external response that adds missing predicates.
Assert: the `FeedbackEvent` produced has `Signal = Edited`,
`ModelResponse` matches the internal attempt, `UserCorrection` is the
adapted external answer, `Domain` is set correctly.

**`RequestPackage_ToExternalCallShape_ContainsRequiredFields`**

Assert: the package serialized to the external call format contains:
- the original request,
- the exact question the external AI must answer,
- the required answer shape,
- the arbiter constraints that still apply.
Assert it does NOT contain: raw file contents, internal adapter paths,
session IDs, or any field marked sensitivity "blocked".

### Learning set for this component

Source: E51B evidence — CH-01 trace from `run-2026-05-04-082822.json`.

The CH-01 trace provides the ground-truth shape for the request package:

```
original_request: "I want biometric passport verification during signup."
internal_attempt: ""  (no answer produced — knowledge edge)
missing_competence: "The local registration learning set knows provider
  sign-in, email verification, duplicate prevention, and token safety.
  It does not yet know how biometric passport verification changes the
  registration flow."
required_answer_shape: "explain how biometric passport verification
  should change the registration flow and which arbiters apply"
arbiter_constraints: ["identity_consistency", "source_authority"]
```

Use this as the positive example. Create three more positive examples
from different knowledge-edge scenarios to build the training set.

Negative examples for the learning set:

- A package where `MissingCompetenceStatement` says only "low confidence"
  — this must fail validation before the package is shown to the user.
- A package where `RequiredAnswerShape` is empty — must fail validation.
- A package where `SensitivityLevel` is not one of the three allowed
  values — must fail validation.

### Acceptance criteria (Gate B)

```bash
dotnet test --filter "RequestPackage|ResultAdapter" --no-build 2>&1 | tail -3
# Expected: 6 passed, 0 failed

dotnet test --filter "ResultAdapter_ExternalResponseContradicts" --no-build 2>&1 | tail -3
# Expected: 1 passed, 0 failed  (the arbiter-rejection test is a hard gate)
```

---

## Component 5 — LearningEventScheduler

### What it does

Sits between the `FeedbackRouter` and the `AutonomousLearningPipeline`.
Watches the DPO pair accumulation. When a domain accumulates at least
`MinPairsThreshold` (default: 5) new pairs, it enqueues a learning event
for that domain. Learning events drain only when `IdleDetector` says the
machine is safe to train.

```csharp
public sealed class LearningEventScheduler
{
    public int MinPairsThreshold { get; }
    public int QueuedEventCount  { get; }

    public OperationResult<bool> OnFeedbackRouted(
        string domain, int currentPairCount);

    public OperationResult<IReadOnlyList<string>> GetReadyDomains(
        IdleDetectorResult idleState);

    public OperationResult<bool> AcknowledgeDomain(string domain);
}
```

`GetReadyDomains` returns enqueued domains only when
`idleState == SafeToTrain`. When the machine is active it returns an
empty list — never blocks, never trains.

### Test file

`Tests/AgentInfrastructureTests.cs` — add to `AgentInfrastructureTests`.

### Tests to write

**`LearningScheduler_BelowThreshold_DoesNotEnqueue`**

Simulate 4 DPO pairs routed for domain "CODE_Q" (threshold is 5).
Assert: `QueuedEventCount == 0`.

**`LearningScheduler_AtThreshold_EnqueuesOneDomain`**

Simulate 5 DPO pairs for "CODE_Q".
Assert: `QueuedEventCount == 1`, domain "CODE_Q" is in the queue.

**`LearningScheduler_TwoDomains_EnqueuesIndependently`**

Simulate 5 pairs for "CODE_Q" and 5 pairs for "BUG" separately.
Assert: `QueuedEventCount == 2`, both domains are in the queue.

**`LearningScheduler_MachineActive_GetReadyReturnsEmpty`**

Enqueue one domain. Call `GetReadyDomains(SystemActive)`.
Assert: empty result. Queue count unchanged.

**`LearningScheduler_MachineIdle_GetReadyReturnsDomains`**

Enqueue one domain. Call `GetReadyDomains(SafeToTrain)`.
Assert: result contains the enqueued domain.

**`LearningScheduler_AcknowledgeDomain_RemovesFromQueue`**

Enqueue "CODE_Q". Call `AcknowledgeDomain("CODE_Q")`.
Assert: `QueuedEventCount == 0`.

**`LearningScheduler_IntegrationWithFeedbackRouter_EndToEnd`**

Full integration test:
1. Route 5 Edited feedback events for domain "BUG" through `FeedbackRouter`.
2. Pass the DPO pair count to `LearningEventScheduler.OnFeedbackRouted`.
3. Assert the scheduler enqueues "BUG".
4. Set idle state to `SafeToTrain`.
5. Call `GetReadyDomains`.
6. Assert "BUG" is in the ready list.
7. Acknowledge "BUG".
8. Assert queue is empty.

This test proves the full path: feedback → accumulation → idle gate →
domain ready → acknowledged.

**`LearningScheduler_BeforeAfterAccuracy_NewAdapterInstalled_OnlyIfImproved`**

Integration test using `AutonomousLearningPipeline`:
1. Set a before-accuracy stub that returns 0.70.
2. Set an after-accuracy stub that returns 0.75.
3. Run one learning cycle for "CODE_Q".
4. Assert the new adapter is registered in `IDomainModelRegistry`.

Negative: if after-accuracy (0.65) is lower than before (0.70), assert
the new adapter is NOT registered. The old adapter stays active.

### Learning set for this component

Source: `14.ContinuousLearning` tests — `ContinuousLearningTests.cs` already
has `FeedbackRouter` patterns. Extend them.

Training scenario for the integration test:

Use the E51A V-records as the source of DPO pairs. Ten traces, each one
becomes a `FeedbackEvent`:

- Traces with `predicate_overlap == 1.0` → `Signal = Accepted`
  (5 examples: V01, V02, V03, V04, V05)
- Traces with `predicate_overlap < 1.0` (synthetic) → `Signal = Edited`
  with a corrected version that includes the missing predicates
  (5 examples: synthetic, one per domain)

Route all ten through `FeedbackRouter`. The five Edited examples produce
five DPO pairs. With `MinPairsThreshold = 5`, exactly one learning event
should be queued after all ten are routed.

### Negative examples

- Routing 4 feedback events must not trigger scheduling (below threshold).
- A domain with only Accepted signals (no DPO pairs) must never queue a
  learning event — positive examples alone do not trigger retraining,
  only DPO pairs do.
- Calling `GetReadyDomains` when the machine is active must return empty
  and must not dequeue pending domains (they stay queued for the next
  idle window).
- A domain where after-accuracy is lower than before-accuracy must not
  install the new adapter — the rollback behavior is a hard gate.

### Acceptance criteria (Gate B)

```bash
dotnet test --filter "LearningScheduler" --no-build 2>&1 | tail -3
# Expected: 8 passed, 0 failed

dotnet test --filter "LearningScheduler_BeforeAfterAccuracy" --no-build 2>&1 | tail -3
# Expected: 1 passed, 0 failed  (the rollback test is a hard gate)

dotnet test LlmFromScratch.sln --no-build 2>&1 | tail -3
# Expected: 900 + (5 + 5 + 4 + 6 + 8) = 928 passed, 0 failed
```

---

## Execution Order and Baseline Rules

Build the five components in the order listed. Each component must hold
the full suite green before the next begins.

| Step | Component | New tests | Expected total |
|------|-----------|-----------|----------------|
| Start | baseline | — | 900/900 |
| 1 | InternalLlmGateway | 5 | 905/905 |
| 2 | InternalResultExaminer | 5 | 910/910 |
| 3 | call_external_ai gate | 5 | 915/915 |
| 4 | RequestPackage + Adapter | 6 | 921/921 |
| 5 | LearningEventScheduler | 8 | 929/929 |

The full suite must pass at each step. A broken test from a prior
component is not acceptable — it means the new component changed a
behavioral contract.

---

## D-14 — Locked Decision

The following rule governs all five components and must be added to
`docs/DECISIONS-LOCKED.md` before the first EXECUTOR session:

> D-14 | Internal LLM always first; external AI requires explicit
> single-use approval with a named missing-competence statement;
> all results examined by the internal model before return; all
> examined results stored as training signals; training runs at
> idle time only under IdleDetector gate; a new adapter installs
> only if after-accuracy exceeds before-accuracy.

---

## Sources Summary

| Component | Primary source | Secondary source |
|-----------|---------------|-----------------|
| InternalLlmGateway | 01.Tokenizer C# files | 21.AgentInfrastructure/Fabric/Interfaces.cs |
| InternalResultExaminer | E51A evidence traces (V01-V13) | 22.AgentAlgorithms/Routing/RouteClassifier.cs |
| call_external_ai gate | 21.AgentInfrastructure/Tools/PermissionGate.cs | E51B CH-01 trace |
| RequestPackage + Adapter | E51B CH-01/CH-02 traces | 14.ContinuousLearning/Feedback/FeedbackRouter.cs |
| LearningEventScheduler | 14.ContinuousLearning/Pipeline | 16.AutonomousLearning/Pipeline |
