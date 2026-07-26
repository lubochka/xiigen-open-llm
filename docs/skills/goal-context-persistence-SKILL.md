# Goal Context Persistence

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

In a long session, the user's original goal drifts out of active attention.
By turn 10 the session answers proxies for the goal rather than the goal itself.
Each individual hop feels rational. The cumulative drift is invisible from inside
the session and obvious to Luba after 20 rounds.

**This skill prevents that by making the goal a first-class persistent artifact.**

---

## When to Invoke

- At session start, before first synthesis
- At every STOP gate (Goal Reminder block required at top of output)
- At designated checkpoints for sessions exceeding 10 turns
- Whenever output direction feels unclear — re-read the goal

One explicit goal load at session start + reminder at every STOP = zero
"we forgot what you were asking for" cycles.

---

## Section 1 — Goal Loading Protocol

At session start:

1. **Capture goal statement verbatim** — exact characters Luba typed, not a
   paraphrase. Preserving original word choice matters: "make X visible" and
   "surface X" look synonymous but can encode different constraints.

2. **If no explicit goal is stated** — ask Luba for one before proceeding.
   Do not infer. Do not guess. A session without a declared goal has no anchor
   against which drift can be detected.

3. **If multiple goals are stated** — capture them as a list and track each
   separately. Multi-goal sessions where goals are not tracked independently
   are exactly how 3-of-4 coverage failures happen.

4. **Goal statement is immutable** for the duration of the session. If Luba
   restates or refines the goal, that is a new session, not an edit to the current one.

---

## Section 2 — Goal Reminder Block (required at every STOP)

Every session output at a STOP begins with:

```markdown
## Goal Reminder (goal-context-persistence-SKILL)

Luba's stated goal for this session:
"[verbatim goal statement]"

This session advances the goal by:
[ONE SENTENCE — concrete observable progress, not abstract framing]
"By [verb] [specific artifact], [specific goal element] moves from [prior state] to [new state]."

What remains to complete the goal: [specific outstanding items or "COMPLETE"]
```

**The "This session advances the goal by" field is mandatory.** If the session
cannot state concrete observable progress, it produced no goal-relevant output.
Surface that explicitly.

Phrases that FAIL the concrete-progress test:
- "Working on goal X" (no verb of completion)
- "Making progress on Y" (no observable delta)
- "Explored implications of Z" (no state change)

Phrases that PASS:
- "By adding IBm25Scorer and BM25Scorer, hybrid search moves from vector-only to BM25+vector."
- "By writing 35 tests for IncrementalTrainer, training pipeline moves from 0 to 1 tested component."
- "By pushing phase 11, skill library moves from 43 to 51 translated skills."

---

## Section 3 — Goal Checkpoint for Long Sessions

Sessions exceeding 10 turns are subject to drift regardless of individual turn quality.

| Turn | Action |
|------|--------|
| 5 | Re-read goal statement. Note alignment verdict. |
| 10 | Re-read. Note alignment verdict. |
| 15, 20, 25, ... | Re-read. Checkpoint each 5 turns. |

### Alignment Verdicts

- **ON_GOAL** — recent turns produced output traceable to the goal. Continue.
- **DRIFTING** — recent turns respond to prior output rather than the goal.
  Warning. Next turn must re-anchor to the goal.
- **OFF_GOAL** — recent turns answer a different question entirely. Stop and
  ask Luba for redirection — do not silently continue.

---

## Section 4 — Goal Context Record Format

Record in STATE-Pn.json or session notes:

```json
{
  "goalContext": {
    "statement": "[verbatim goal statement]",
    "capturedAt": "2026-05-01",
    "goalElements": [
      "element 1",
      "element 2"
    ],
    "checkpoints": [
      {
        "turn": 5,
        "progressStatement": "[what objectively changed toward the goal]",
        "alignmentVerdict": "ON_GOAL"
      }
    ],
    "goalComplete": false
  }
}
```

---

## Section 5 — Goal Drift Signals

Self-check these signals before every STOP:

| Signal | Threshold | Action |
|--------|-----------|--------|
| Rounds since last goal reference in output | ≥ 5 | Re-anchor |
| Consecutive DRIFTING checkpoints | ≥ 2 | STOP, re-anchor |
| Session length exceeds 10 turns without checkpoint | any | Force checkpoint |
| "This advances the goal by" is empty or abstract | any | Rewrite turn |
| Luba's recent messages contain correction ("no — X", "stop doing Y") | any | STOP, re-anchor immediately |

The last signal is the most reliable drift detector. If Luba corrects direction,
the session has drifted, regardless of the session's internal sense of alignment.

---

## Anti-Patterns

1. **"The goal is obvious, I don't need to quote it"** — obvious goals drift
   fastest because no one watches them.

2. **"I'll paraphrase the goal for cleanliness"** — the user's word choice
   matters. Capture verbatim.

3. **"Luba hasn't objected, so we must be on-track"** — silence is not
   alignment. Run checkpoints regardless.

4. **"Progress is abstract"** — if the turn's progress is abstract, the turn
   produced little goal-relevant output. Surface that rather than hiding it
   in vague language.
