# Arbiter Registration

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

An arbiter that is described in a topology contract but not registered in the
validate.handler named checks list silently skips during execution. The topology
runs, scores appear, and there is no error — but the named check was never
evaluated. The score is computed without the constraint that the arbiter was
supposed to enforce. This skill defines the arbiter fixture format, the named
check registry, the arbiter replay gate that verifies each check fires correctly
before Phase B, and the severity levels that control scoring impact.

---

## When to Invoke

- Phase A (infrastructure setup) of any capability that declares arbiters
  in its topology contract
- When adding a new named check that doesn't exist in the validate.handler registry
- When arbiter replay gate fails before Phase B
- When a named check is in the registry but the arbiter never fires during runs

---

## Section 1 — Named Check Registry

Named checks are detection functions registered in the validate.handler.
An arbiter fixture's `CheckId` field MUST exactly match a key in this registry.
An unmatched CheckId is silently skipped — not an error.

| CheckId | What it detects |
|---|---|
| `operation_result_everywhere` | Public method returns void, bool, or primitive instead of OperationResult<T> — Rule 1 violation |
| `no_concrete_injection` | Constructor takes a concrete type instead of an interface — Rule 2 violation |
| `store_before_enqueue` | IAgentMessageBus.Publish() called before OperationResult.Ok() return — Rule 1 + ordering violation |
| `session_id_scope_isolation` | Cross-session data access without SessionId scope check — scope-isolation-arbiter-SKILL.md violation |
| `no_throw_for_business_logic` | `throw` statement found in non-test code for a business logic condition |
| `incremental_trainer_path_check` | IncrementalTrainer reads confidence from wrong field path (SILENT_FAILURE class) |
| `machine_constant_not_hardcoded` | A value classified as MACHINE by freedom-machine-SKILL.md is hardcoded in the method |
| `pii_not_in_event_payload` | Email, password, phone, or name field found in IAgentMessageBus event payload |

**If the arbiter requires a check not in this list:**
- This is a new named check that must be added to validate.handler before Phase A seeding
- Document it as a gap: "validate.handler missing named check: {CheckId}"
- Specify the detection logic: `Func<string, bool>` — input is the code under review

---

## Section 2 — Arbiter Fixture Format

One JSON file per capability in `docs/arbiters/{capability-id}-arbiters.json`.
One JSON object per arbiter:

```json
{
  "ArbiterId": "SCORING-DETAIL_GAP-routing-dna-001",
  "CapabilityId": "capability-id",
  "CapabilityClass": "SCORING",
  "Rule": "All generated scoring methods must return OperationResult<T>, never void",
  "CheckId": "operation_result_everywhere",
  "CheckTarget": "generated_code",
  "Severity": "score-zero",
  "Description": "Generated method must return OperationResult<ScoreResult> — void or bool is a Rule 1 violation"
}
```

**Required fields (all mandatory):**

| Field | Description |
|---|---|
| `ArbiterId` | Format: `{CapabilityClass}-{CapabilityId}-{short-name}-{NNN}` |
| `CapabilityId` | Which capability this arbiter applies to |
| `CapabilityClass` | ROUTING / SCORING / VALIDATION / TRANSACTION / ORCHESTRATION / SCHEDULED |
| `Rule` | Human-readable business rule this check enforces |
| `CheckId` | Must exactly match a key in Section 1 registry |
| `CheckTarget` | Always `"generated_code"` for code validation arbiters |
| `Severity` | `"score-zero"` \| `"build-failure"` \| `"warning"` |
| `Description` | What the check looks for in the generated code |

---

## Section 3 — Severity Levels and Scoring Impact

| Severity | Effect on Score |
|---|---|
| `score-zero` | The iron_rules or dna_compliance score component becomes 0, regardless of other checks. A single score-zero arbiter firing means the overall score cannot reach 0.85. |
| `build-failure` | Same as score-zero but flagged separately. Used for ordering violations (Rule 1 + store-before-enqueue). |
| `warning` | Score is reduced by arbiter weight (from topology quality gates) but not zeroed. Used for style and recommendation violations. |

**Never use `warning` severity for DNA Rule violations (Rules 1–10).** DNA
rule violations are score-zero or build-failure. A DNA violation that only
reduces the score is a violation that can be accepted — which defeats the rule.

---

## Section 4 — Arbiter Replay Gate

The arbiter replay gate MUST run before Phase B. An arbiter that is registered
but returns incorrect results will produce wrong scores for every Phase B run.

For each arbiter, run two tests:

```csharp
// Test 1: Arbiter fires on BAD code (should produce a violation)
[Fact]
public async Task Arbiter_FiresOnViolatingCode()
{
    var result = await _validateHandler.Execute(new ValidateRequest
    {
        CapabilityId = "scoring-capability",
        CheckId = "operation_result_everywhere",
        GeneratedCode = @"
            public void ComputeScore(string input)
            {
                // score logic
            }"
    });
    Assert.Contains(result.Value.Violations,
        v => v.CheckId == "operation_result_everywhere");
}

// Test 2: Arbiter passes on GOOD code (no violation)
[Fact]
public async Task Arbiter_PassesOnCompliantCode()
{
    var result = await _validateHandler.Execute(new ValidateRequest
    {
        CapabilityId = "scoring-capability",
        CheckId = "operation_result_everywhere",
        GeneratedCode = @"
            public OperationResult<ScoreResult> ComputeScore(string input)
            {
                return OperationResult<ScoreResult>.Ok(new ScoreResult { Score = 0.9 });
            }"
    });
    Assert.Empty(result.Value.Violations.Where(v => v.CheckId == "operation_result_everywhere"));
}
```

**Phase A gate passes when:** every declared arbiter fires on violating code
AND passes on compliant code. If any arbiter test fails: fix the named check
or the fixture before proceeding to Phase B.

---

## Section 5 — Hard Rules

```
NEVER create an arbiter fixture with a CheckId that doesn't exist in the registry
  → validate.handler silently skips unknown checks; no error, no detection

NEVER use "warning" severity for DNA Rule 1–10 violations
  → A warnable DNA violation can pass at 0.82 score
  → DNA rules are always score-zero or build-failure

NEVER skip the arbiter replay gate
  → A check that looks registered but returns wrong results corrupts all Phase B scores
  → Replay gate runs before Phase B, not just at Phase A end

NEVER assume CheckId matches by substring
  → "operation_result" does not match "operation_result_everywhere"
  → Exact string match only
```

---

## Anti-Patterns

1. **"The arbiter is in the fixture file, so it's registered"** — the fixture
   file records what the arbiter should check. The named check registry is
   where the detection logic lives. If the CheckId doesn't match a registry
   entry, the arbiter silently skips.

2. **"Arbiter replay test passes on bad code — the rule is too strict"** —
   if the bad-code test passes (no violation produced), the check logic is
   wrong or the test's bad code doesn't match the actual violation. Fix the
   check, not the test.

3. **"I'll add the named check in Phase B"** — the arbiter replay gate runs
   before Phase B. If the named check doesn't exist yet, Phase B cannot begin.
   Add named checks in Phase A.

---

## Integration

- ai-pipeline-topology-SKILL.md validate.handler node loads arbiter fixtures at topology execution time
- score-zero-investigation-SKILL.md investigates when a named check consistently fires with score-zero
- bug-to-tests-SKILL.md: arbiter replay gate failure is a Class A bug (Fabric Interface — validate.handler returns wrong shape or wrong data)
- three-level-verification-SKILL.md: arbiter registration fix must pass unit (replay gate) + integration + suite
- dna-compliance-guard-SKILL.md: CheckId `operation_result_everywhere`, `no_concrete_injection`, `store_before_enqueue` map to Rules 1, 2, and ordering requirements
