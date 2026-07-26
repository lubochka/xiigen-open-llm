# Capability Fleet Completion

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

A session that improves 3 capabilities without knowing there are 48 has not made
fleet-level progress. It has made local progress on an unknown fraction of work.
Without a definition of done at the fleet level, work accumulates on already-examined
capabilities while unexamined ones remain untouched. This skill defines the minimum
examined state per capability, the 5 capability states, the only valid fleet progress
metric, and the work priority protocol.

---

## When to Invoke

- At the start of any session claiming fleet-level capability progress
- Before any STOP that reports a percentage improvement across capabilities
- When deciding which capabilities to examine next
- When closing out a batch to verify it meets minimum examined state

---

## Section 1 — Minimum Examined State Per Capability

A capability is considered **examined** when all three criteria are met:

### Criterion 1 — Examination Record

File: `docs/capability-examination/{capability-id}-examination.md`

Must contain all of:
- `WHO:` field — specific user role using this capability (one sentence)
- `VERB:` field — one action the user or system is here to perform
- `GRAMMAR:` field — G1–G7 type declared (if capability has a UI surface)
- `FEEL:` field — one sentence on the expected output quality register
- At least one test audit result (test class + pass/fail count)
- `overall_grade:` field — `APPROVED | NEEDS_REVISION | NEEDS_PURPOSE_BUILT_FORM`

A file that exists but is missing any required field counts as PARTIAL, not EXAMINED.

### Criterion 2 — Design Context

File: `docs/design-context/{capability-id}/.spec.md`

Must exist and be populated. An empty file does not satisfy this criterion.
If absent: the capability design review (capability-design-review-SKILL.md) has not
run for this capability.

### Criterion 3 — Audit Trail

File: `docs/capability-audit/{capability-id}-audit-trail.md`

Must contain:
- Test class declared
- All BLOCKING findings cleared (or none present)
- Test audit record attached
- `.spec.md` presence confirmed

An audit trail with open BLOCKING findings does not satisfy Criterion 3.

---

## Section 2 — Capability States

| State | Definition |
|-------|-----------|
| **NOT_STARTED** | No examination record exists |
| **PARTIAL** | Examination record exists but missing ≥1 required field, OR `.spec.md` absent, OR audit trail absent/incomplete |
| **EXAMINED** | All three criteria met |
| **BLOCKED** | Open issue prevents work — do not examine until issue resolves |
| **APPROVED** | Examined + `overall_grade = APPROVED` |

An EXAMINED capability with grade NEEDS_REVISION is examined but not approved.
An EXAMINED capability with grade NEEDS_PURPOSE_BUILT_FORM requires redesign before
it can reach APPROVED. Both count toward examined count, not approved count.

---

## Section 3 — The Only Valid Fleet Progress Metric

```
Fleet progress = N_examined / (N_total - N_blocked)
```

Where:
- `N_examined` = count of capabilities at EXAMINED or better (including APPROVED)
- `N_blocked` = count of capabilities with open blocking issues
- Denominator = eligible fleet

**This is the only valid fleet progress metric.** Examples of invalid metrics:
- "Capability pass rate improved from 62% to 71%" — session-level metric
- "5 capabilities improved this session" — no denominator
- "Phase 3 batch is 50% done" — batch-level, not fleet-level

Valid fleet progress statement:
```
Fleet progress: [N_examined] of [N_total - N_blocked] eligible capabilities examined
  ([N_examined / (N_total - N_blocked) * 100]% of eligible fleet)
  Blocked: [N_blocked] — [list issue IDs]
  Last updated: [date]
```

---

## Section 4 — Batch Structure

Organize capabilities into batches of 5–10. A batch is not done until every
non-blocked capability in it reaches EXAMINED state.

**Batch-level done:** every non-blocked capability in the batch is EXAMINED.
**Fleet-level done:** all batches are batch-level done.

---

## Section 5 — Fleet-Level Done Conditions

Fleet capability work is **done** when:

```
N_examined == (N_total - N_blocked)
AND
N_blocked == 0 OR all blocked capabilities have a resolution path in their issue record
```

Capabilities that remain BLOCKED indefinitely without a resolution path are not
"done" — they are abandoned.

**APPROVED vs DONE:**
- DONE means examined (all three criteria met, any grade)
- APPROVED means examined with grade = APPROVED
- Track both counts: N_examined and N_approved

---

## Section 6 — Claim Authority Rules

| Session covers | Permitted claim |
|---------------|-----------------|
| 1 capability, fully audited | "Capability X improved" |
| N capabilities, all fully audited | "These N capabilities improved" |
| ≥ 20% of eligible fleet, all fully audited | "Capability fleet is improving" |
| < 20% of eligible fleet | Capability-level claims only — no fleet-level claims |
| Any session without scope record | No improvement claims at any level |

"Fully audited" means: test audit complete + audit trail produced + `.spec.md` present.
A session that ran the 4-phase checker on 3 of 5 capabilities it touched has fully
audited 3 capabilities, not 5.

---

## Section 7 — Work Priority Protocol

When multiple capabilities are available, work in this order:

1. **NOT_STARTED capabilities in incomplete batches** — most valuable: advances both
   batch completion and fleet coverage simultaneously
2. **PARTIAL capabilities** — already started; completing is lower marginal cost than
   starting new
3. **EXAMINED capabilities needing revision** — important but does not advance fleet
   coverage count (stays EXAMINED, grade changes)
4. **Re-examining APPROVED capabilities** — lowest priority; fleet is already served

Do not re-examine an APPROVED capability when NOT_STARTED capabilities exist in the
same or other batches.

---

## Section 8 — Scope Inventory Before Work

Before any session touches an individual capability:

```markdown
## Scope Inventory — Capability Fleet

Fleet total: [N] capabilities
Examined (APPROVED):  [N] capabilities
Examined (other grade): [N] capabilities
Partial:  [N] capabilities
Blocked:  [N] capabilities — [list IDs]
Not started: [N] capabilities

This session covers: [list capability IDs]
Session count: [N] of [N_total - N_blocked] eligible
Fleet coverage: [N / (N_total - N_blocked) * 100]% of eligible fleet

Claim authority:
  Fleet-level: [ESTABLISHED (≥20%) | NOT ESTABLISHED (<20%)]
  Capability-level: ESTABLISHED for [list fully audited this session]
```

---

## Anti-Patterns

1. **"We've done 8 capabilities — that's good progress"** — fleet progress requires the
   denominator. 8 of 45 eligible capabilities is 17.8%. State it as such.

2. **"The examination record exists so the capability is done"** — all three criteria must
   be met. A record missing WHO or GRAMMAR is partial, not examined.

3. **"We'll come back to the blocked capabilities"** — blocked capabilities with no
   resolution path are abandoned, not deferred. Every blocking issue must have a
   resolution path before the fleet can reach done.

4. **"This batch is done"** — a batch is done only when every non-blocked capability in it
   is EXAMINED. Check all capabilities before claiming batch-level done.

5. **"Improvement percentage = fleet progress"** — improvement percentage measures delta
   from a baseline. Fleet progress measures how much of the total scope has been
   examined. They are different dimensions.

---

## Integration

- work-scope-inventory-SKILL.md: establishes denominator before work; this skill defines
  what the done/partial/not-started categories mean
- improvement-measurement-SKILL.md: flow-level improvement claims require EXAMINED state
  as baseline; unexamined capability has no baseline to compare against
- coverage-completeness-gate-SKILL.md: "fleet capability work done" claim requires this
  skill's completion criteria to be met for all non-blocked capabilities
- capability-design-review-SKILL.md: produces `.spec.md` (Criterion 2) before any
  implementation begins
