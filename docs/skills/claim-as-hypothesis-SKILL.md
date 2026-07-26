# Claim-as-Hypothesis

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

A user's assertion about what exists in the codebase ("the design is done," "X already works,"
"that interface is implemented") is a hypothesis, not a fact. Planning built on
unverified user assertions fails when execution discovers the assertion was wrong.
This skill captures, verifies, and gates on every such assertion before planning proceeds.

---

## When to Invoke

- At session start — capture every user assertion about existing state
- When a user makes a new claim during the session (trigger phrases in Section 1)
- Before any plan step that depends on "X exists / works / is done"
- At every STOP — verify no PENDING claims remain

Zero pending claims at STOP = zero "built on sand" execution failures.

---

## Section 1 — Trigger Phrases

Any user message containing one of these patterns produces a PENDING entry in the
claim verification record:

| Pattern | Example | Why it matters |
|---|---|---|
| Existence claims | "that interface is already in the project" | Interface may be a stub with NotImplementedException |
| Completion claims | "the design is done" | Design file may exist but have empty fields |
| Operational claims | "that works" / "already works" | Test may be missing or disabled |
| Implementation claims | "the service is implemented" | Class may exist but all methods throw |
| Wiring claims | "X is wired to Y" | Constructor injection may not be registered |
| Readiness claims | "it's ready" | Undefined — ready means different things |

Phrases that do NOT produce claim entries:
- Goals ("I want X") — handled by goal-context-persistence-SKILL
- Preferences ("I prefer Y") — not about existing state
- Past decisions ("we decided to use X") — decision history is not current state

---

## Section 2 — Claim Verification Record Schema

```json
{
  "claims": [
    {
      "id": "C1",
      "statement": "[verbatim user assertion — exact quote]",
      "capturedAt": "session start / turn N",
      "status": "PENDING",
      "importance": "BLOCKING | INFORMATIONAL",
      "verificationAction": "[specific grep/Read command that will verify]",
      "verdict": null,
      "evidence": null
    }
  ]
}
```

After verification:
```json
{
  "id": "C1",
  "status": "VERIFIED | DISCONFIRMED | PARTIAL | DEFERRED",
  "verdict": "[precise statement of what was found]",
  "evidence": "[exact command run + result]"
}
```

**Status definitions:**
- **PENDING**: captured, verification action queued, not yet executed
- **VERIFIED**: command ran; evidence matches the claim as stated
- **DISCONFIRMED**: command ran; evidence contradicts the claim
- **PARTIAL**: evidence matches for some subset and not others (most common)
- **DEFERRED**: Luba explicitly approved leaving unverified (rare; recorded with reason)

**Importance:**
- **BLOCKING**: if the claim is wrong, the session's plan fundamentally changes — cannot STOP without resolution
- **INFORMATIONAL**: if the claim is wrong, the plan still holds — can STOP with acknowledgment

---

## Section 3 — Verification Protocol

Verification is a **read action**, not a reasoning action. Run a specific command; record the result.

| Claim shape | Verification action |
|---|---|
| "X file exists" | `ls` or `Glob` — record presence/absence |
| "X contains Y" | `grep -n [pattern] [path]` — record hit count and first 3 matches |
| "interface is implemented" | `grep -n "NotImplementedException\|throw new" [file]` — should return 0 hits |
| "service is wired" | `grep -n "[InterfaceName]" [registrationFile]` — should return ≥1 hit |
| "tests pass" | `dotnet test --filter "[filter]"` — record actual count |
| "method exists" | `grep -n "[MethodName]" [file]` — record presence/absence |

**What does NOT count as verification:**
- "I know X likely exists" — not a command result
- "A prior session mentioned X" — state changes between sessions
- "The path suggests X" — inference is not evidence
- User confirming their own claim — circular

The only valid verification: specific command, specific observable result, recorded verbatim.

---

## Section 4 — Gate Enforcement

### Before first synthesis output

1. Every user assertion captured in the claim record with status PENDING
2. BLOCKING claims have a queued verification action before synthesis proceeds
3. Any synthesis that depends on a claim references it by ID (C1, C2, ...)

### Before STOP

1. No BLOCKING claim remains in PENDING status
2. Every plan assumption that depends on a claim has the verdict integrated
3. DEFERRED claims carry a carry-forward flag for the next session

### Deferral rule

Only Luba can defer a claim. The session cannot self-defer.
Deferral recorded as:
```json
{
  "status": "DEFERRED",
  "deferredBy": "LUBA",
  "deferralReason": "[specific reason — what would be needed to verify]",
  "carryForwardFlag": true
}
```

---

## Section 5 — Worked Examples

### PARTIAL — "the design is done"

User says: "That subproject's interface design is done."

Verification:
```bash
ls src/[Project]/I[InterfaceName].cs  → file exists
grep -n "NotImplementedException" src/[Project]/I[InterfaceName].cs  → 0 hits
grep -n "I[InterfaceName]" src/[Project]/[ConcreteClass].cs  → 0 hits (not implemented)
```

Verdict: PARTIAL — interface file exists and is specified, but no concrete implementation registered.

Consequence: plan must include implementation task, not just "use the existing design."

### DISCONFIRMED — "the service is wired"

User says: "IBm25Scorer is wired into HybridRetriever."

Verification:
```bash
grep -n "IBm25Scorer" src/[Project]/HybridRetriever.cs  → 0 hits
grep -n "IBm25Scorer" src/[Project]/Program.cs  → 0 hits
```

Verdict: DISCONFIRMED — constructor injection not present. Field not injected.

Consequence: "wiring is done" becomes a task in the plan, not an assumption.

### VERIFIED — "the interface exists"

User says: "IVectorIndex is already defined."

Verification:
```bash
grep -rn "interface IVectorIndex" src/  → found at src/AgentAlgorithms/IVectorIndex.cs:5
```

Verdict: VERIFIED — interface defined at expected location.

---

## Anti-Patterns

1. **"The user confirmed it"** — user confirming their own claim doubles the assertion; does not verify it.

2. **"It's obviously true"** — obviousness is memory, not evidence. A file that "obviously exists" may be a stub.

3. **"Skip verification to save time"** — a 30-second grep prevents a 3-round plan failure. Never skip.

4. **"PARTIAL is close enough to VERIFIED"** — PARTIAL surfaces the gap that will fail during execution. Collapsing it hides the gap.

5. **"Verify once, assume forever"** — codebase state changes. Re-verify claims each session.

---

## Integration

- claim-verification-SKILL.md handles code-level claims during implementation (VERIFIED/INFERRED/ASSUMED)
- design-artifact-completeness-SKILL.md handles artifact population checks (file exists ≠ file populated)
- goal-context-persistence-SKILL.md handles the user's goals (separate from claims about existing state)
- prerequisite-chain-SKILL.md uses claim verification evidence when classifying prerequisites as PASS/FAIL/BLOCKING
