# MVP Round-Trip Verification

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

A feature that passes unit tests but cannot be observed end-to-end has not been
delivered. Unit tests verify internal correctness; they do not verify that a
capability flows from its author through every integration point to a consumer
who can observe a result. This skill installs the full 8-step round-trip check
that closes the gap between "tests pass" and "feature delivered."

---

## When to Invoke

- Before declaring any Phase complete that introduces a new public capability
- Before writing a SESSION-BRIEF handoff that claims a feature is delivered
- When phase-completion-packager-SKILL asks for delivery evidence
- After any integration point is wired (new interface implementation, new service
  registration, new pipeline stage)

One round-trip check before STOP = no "works in unit test, broken in pipeline" claims.

---

## Section 1 — The 8-Step Round-Trip

Each step has a responsible actor, an observable output, and a verification command.

| Step | Name | Actor | Observable Output | Verification |
|------|------|-------|------------------|--------------|
| 1 | **Capability authored** | Developer | Class + interface file on disk | `ls src/[Project]/[ClassName].cs` |
| 2 | **Visible in project** | .NET build | Assembly references resolve | `dotnet build → 0 Error(s)` |
| 3 | **Published / registered** | Service wiring | Constructor injection configured | `grep -n "Register\|AddSingleton\|AddTransient" src/[Project]/Program.cs` |
| 4 | **Discoverable by consumer** | Project reference | Consumer project can resolve type | `dotnet build [ConsumerProject] → 0 Error(s)` |
| 5 | **Installed / callable** | Test fixture | Test can instantiate and call | `dotnet test --filter "[TestClass]" → ≥1 passed` |
| 6 | **Visible in result** | Test assertion | Return value observable in output | `dotnet test --filter "[SpecificTest]" → PASSED` |
| 7 | **Consumer runs to completion** | Full pipeline test | No unhandled OperationResult.Fail | `dotnet test --filter "[PipelineTest]" → passed, 0 failed` |
| 8 | **Result observable** | End-to-end assertion | Specific output matches expected | `dotnet test --filter "[E2ETest]" → PASSED; output verified` |

**Minimum delivery evidence: Steps 1–6 must all pass.**
Steps 7–8 are required for pipeline-level delivery claims.

---

## Section 2 — Nomination Format

A round-trip nomination identifies the capability being verified and assigns
each step a verification command.

```markdown
## MVP Round-Trip Nomination

Capability: [ClassName implementing IInterfaceName]
Phase: [N] — [phase name]
Delivery claim: [one sentence — what the consumer can now do]

Step 1 — Authored:
  Command: ls docs/skills/[skill-name]-SKILL.md
           OR ls src/[Project]/[ClassName].cs
  Expected: file exists
  Actual: [result]

Step 2 — Visible in project:
  Command: dotnet build src/[Project]/[Project].csproj
  Expected: 0 Error(s), 0 Warning(s)
  Actual: [result]

Step 3 — Registered:
  Command: grep -n "[ClassName]" src/[Project]/[RegistrationFile].cs
  Expected: at least 1 match
  Actual: [result]

Step 4 — Discoverable:
  Command: dotnet build src/[ConsumerProject]/[ConsumerProject].csproj
  Expected: 0 Error(s)
  Actual: [result]

Step 5 — Callable:
  Command: dotnet test --filter "FullyQualifiedName~[TestClass]"
  Expected: ≥1 passed, 0 failed
  Actual: [result]

Step 6 — Result visible:
  Command: dotnet test --filter "FullyQualifiedName~[SpecificTest]"
  Expected: PASSED; assertion verifies [specific value]
  Actual: [result]

Step 7 — Pipeline complete (if pipeline claim):
  Command: dotnet test --filter "FullyQualifiedName~[PipelineTest]"
  Expected: [N] passed, 0 failed
  Actual: [result]

Step 8 — Result observable end-to-end (if pipeline claim):
  Command: dotnet test --filter "FullyQualifiedName~[E2ETest]"
  Expected: PASSED; [specific assertion]
  Actual: [result]

Round-trip verdict: PASS | PARTIAL ([steps that failed]) | FAIL
```

---

## Section 3 — Round-Trip Record Schema

Stored in EXECUTION-LOG-Pn.json under `roundTrip`:

```json
{
  "roundTrip": {
    "capability": "[ClassName implementing IInterfaceName]",
    "deliveryClaim": "[one sentence]",
    "steps": {
      "authored": { "command": "ls ...", "expected": "file exists", "actual": "EXISTS", "result": "PASS" },
      "visible": { "command": "dotnet build ...", "expected": "0 Error(s)", "actual": "0 Error(s)", "result": "PASS" },
      "registered": { "command": "grep -n ...", "expected": "≥1 match", "actual": "2 matches", "result": "PASS" },
      "discoverable": { "command": "dotnet build [consumer]", "expected": "0 Error(s)", "actual": "0 Error(s)", "result": "PASS" },
      "callable": { "command": "dotnet test --filter ...", "expected": "≥1 passed", "actual": "5 passed", "result": "PASS" },
      "resultVisible": { "command": "dotnet test --filter ...", "expected": "PASSED", "actual": "PASSED", "result": "PASS" },
      "pipelineComplete": { "command": "dotnet test --filter ...", "expected": "N passed", "actual": "N passed", "result": "PASS" },
      "resultObservable": { "command": "dotnet test --filter ...", "expected": "PASSED", "actual": "PASSED", "result": "PASS" }
    },
    "verdict": "PASS",
    "minimumDelivery": true,
    "pipelineDelivery": true
  }
}
```

---

## Section 4 — Partial Round-Trip Protocol

When Steps 1–6 all pass but Steps 7–8 fail:
- Delivery claim is scoped to the component, not the pipeline
- SESSION-BRIEF states: "Component delivered; pipeline integration pending"
- Next phase must include Steps 7–8 as mandatory Gate B conditions

When any of Steps 1–6 fail:
- Phase is NOT complete
- The failing step is the next task — do not move to a different step
- Root cause is identified with systematic-debugging-SKILL before fixing

---

## Anti-Patterns

1. **"Tests pass, so it's delivered"** — passing unit tests satisfy Step 5/6 only.
   Steps 2–4 (build visibility, registration, consumer-discoverability) can fail
   independently of test results.

2. **"Registration is obvious, skip Step 3"** — constructor injection misconfiguration
   produces runtime `NullReferenceException`, not a build error. Step 3 is not optional.

3. **"I'll verify steps 7-8 later"** — "later" becomes "never" when the session ends.
   Steps 7-8 are required before any pipeline-level delivery claim.

4. **"Partial delivery counts as delivered"** — a SESSION-BRIEF that says "delivered"
   for a partial round-trip produces incorrect project state. Use "component delivered,
   pipeline integration pending."

---

## Integration

- phase-completion-packager-SKILL.md requires a round-trip nomination as delivery evidence
- improvement-measurement-SKILL.md uses Step 6/7/8 results as behavioral-layer evidence
- coverage-completeness-gate-SKILL.md reads the round-trip verdict to decide whether
  a pipeline-level delivery claim is authorized
- session-execution-log-SKILL.md stores the round-trip record in EXECUTION-LOG-Pn.json
