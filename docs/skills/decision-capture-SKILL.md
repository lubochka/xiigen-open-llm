# Decision Capture

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

A decision made in a planning session and not recorded is re-derived in the next
session. Re-derivation costs planning time and often produces a different answer —
not because the situation changed, but because the reasoning context was lost.
This skill defines what counts as a significant decision, how to record it in
the DESIGN_REASONING format, and when to write it to `docs/DECISIONS-LOCKED.md`
vs when to keep it as a session-level note.

---

## When to Invoke

- At Gate B of every phase — before declaring a phase complete
- During any planning session when a significant decision is made (capture immediately)
- When plan-self-validation-SKILL.md Phase 0 finds an undocumented design choice
- When a future session references "a prior decision" that is not in DECISIONS-LOCKED.md

---

## Section 1 — What Counts as a Significant Decision

A decision is significant if a future session planning similar work would need to
know it to avoid rework.

**The one-sentence test:**
> "If a future session encountered a similar capability, would knowing this decision
> save at least one phase of replanning?"

If YES → capture it.

**Decision types that are always significant:**

| Type | Description | Example |
|---|---|---|
| `CAPABILITY_CLASSIFICATION` | four-tier-decision classification result with non-obvious reasoning | `HybridRetriever` classified as LEARNABLE because score weights are per-session data, not hardcoded |
| `INTERFACE_INTRODUCTION` | Why a new fabric interface was added instead of extending existing one | `IHybridRetriever` added instead of extending `IBm25Scorer` — different contract, different lifecycle |
| `DEPENDENCY_DIRECTION` | Why a capability depends on another in this direction | `ContinuousLearningPipeline` depends on `DomainModelRegistry` not the reverse — registry is infrastructure |
| `IRON_RULE_DERIVATION` | Non-obvious rule traced to a specific failure mode | Token revocation rule derived from concurrent validation race condition |
| `ARCHITECTURE_PATTERN` | A general pattern established for future capabilities | "If N > 2 capabilities depend on X, X registers with DomainModelRegistry as PROVEN first" |
| `LOCKED_DECISION_OVERRIDE` | Why a locked decision needed an ADR before changing | D-09 (max_weight=1.0) overridden for DPO triple quality score — different semantics |

**What is NOT significant:**

- Decisions that follow directly from CLAUDE.md Rules 1–10 (no reasoning needed)
- File naming or test naming choices (implementation details)
- Test framework usage (xUnit [Fact]/[Theory] is decided — not a per-session decision)
- Individual iron rules (captured in contracts, not in architectural decisions)

---

## Section 2 — The DESIGN_REASONING Format

```csharp
// This record maps directly to a DECISIONS-LOCKED.md entry
public record DesignReasoning
{
    public string DecisionId { get; init; }    // "D-NN" — next sequential number
    public string Type { get; init; }          // capability-classification, interface-introduction, etc.
    public string Question { get; init; }      // what was decided? (the retrievable key)
    public string Reasoning { get; init; }     // why? what was considered and rejected?
    public string Principle { get; init; }     // which DNA Rule or locked decision this applies
    public string Outcome { get; init; }       // what is now true as a result
}
```

**The Question field is the retrieval key.** Future sessions match against `Question`
when searching prior decisions. A vague question produces no match.

```
BAD:  Question = "HybridRetriever design"
GOOD: Question = "Should HybridRetriever use a single interface or separate BM25 + vector interfaces?"

BAD:  Question = "Wave assignment"
GOOD: Question = "Should ContinuousLearningPipeline register before or after DomainModelRegistry?"
```

**The Reasoning field is what teaches future sessions.** A decision that says
"we decided X" teaches nothing. A decision that says "we considered A and B;
A was rejected because it creates wrong dependency direction; B was chosen
because it preserves IncrementalTrainer's ability to update independently..." gives
future sessions the classification logic, not just the classification.

---

## Section 3 — DECISIONS-LOCKED.md Entry Format

```markdown
## D-NN: [One-sentence decision statement]

**Type:** CAPABILITY_CLASSIFICATION | INTERFACE_INTRODUCTION | DEPENDENCY_DIRECTION |
          IRON_RULE_DERIVATION | ARCHITECTURE_PATTERN | LOCKED_DECISION_OVERRIDE

**Question:** [Exact retrievable question — stated as a question]

**Reasoning:** [What was considered and rejected. Why this option was chosen.
               Which failure mode this prevents. Two to five sentences.]

**Principle:** [Which DNA Rule, prior locked decision, or architectural pattern applies]

**Outcome:** [What is now true. What this decision enables or prevents.]

**NEVER reopen without Luba approval.** [Add this line if the decision has
high blast radius — i.e., 3+ capabilities depend on it]
```

**Minimum 2 decisions per planning session.** A session with fewer than 2 significant
decisions either had trivial scope or decisions were missed.

---

## Section 4 — When to Write to DECISIONS-LOCKED.md

Not every significant decision belongs in DECISIONS-LOCKED.md. The criteria:

| Write to DECISIONS-LOCKED.md when | Keep as session note when |
|---|---|
| Decision affects ≥ 2 capability classes | Decision is specific to one implementation |
| Decision sets a pattern for future phases | Decision is about current session scope only |
| Decision would be challenged without documentation | Decision is obvious from the code |
| Decision requires "NEVER reopen without Luba" | Decision can safely be revisited |
| Decision overrides a prior locked decision | Decision supplements but does not override |

---

## Section 5 — Retroactive Capture

For phases that ran before this skill existed, capture decisions retroactively from:
- Git commit messages (phase commit messages often contain reasoning)
- CLAUDE.md and AGENTS.md (contain settled decisions from prior sessions)
- Code comments (Rule 3 — every line has a comment — often captures the why)

Retroactive capture is less valuable than real-time capture (some context is lost),
but it is far better than nothing. Every captured decision reduces future replanning.

---

## Anti-Patterns

1. **"The decision is obvious from the code"** — code shows what was decided.
   `docs/DECISIONS-LOCKED.md` records why. Future sessions need the why.

2. **"I'll capture it at the end of the session"** — decisions captured from memory
   at session end are thinner than decisions captured immediately. Record as you go.

3. **"Decision type = GENERAL"** — every significant decision fits one of the 6 types.
   A decision typed GENERAL was not classified. Type forces the reasoning structure.

4. **"Question field written as a statement"** — `Question = "The HybridRetriever design"`
   is not retrievable by a future session searching for the decision. Ask it as a question:
   `Question = "Should HybridRetriever expose one interface or two?"`

5. **"Fewer than 2 decisions per session"** — a planning session with fewer than 2 significant
   decisions either had trivial scope or decisions were missed. Re-read the session.

---

## Integration

- docs/DECISIONS-LOCKED.md is the persistence layer for decisions captured by this skill
- decision-reopening-SKILL.md governs when a locked decision must be revisited
- planning-session-startup-SKILL.md Q2 reads DECISIONS-LOCKED.md before any plan is written
- audit-protocol-SKILL.md Point 2 (blast radius) uses decisions to identify scope
- plan-self-validation-SKILL.md Phase 0 verifies that every significant decision is recorded
