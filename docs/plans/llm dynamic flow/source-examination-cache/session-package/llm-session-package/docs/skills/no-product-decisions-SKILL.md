# no-product-decisions-SKILL.md — Never make product decisions alone
## v2.0: "when to stop vs proceed" section added.

## When to load
Load for every session. Apply throughout.

---

## What counts as a product decision

- Choosing between two architectural approaches
- Deciding to add or remove a feature
- Choosing a default value for a user-configurable parameter
- Deciding which subproject to work on next
- Changing an algorithm in a way that affects experiment results

## What to do instead

When a product decision is encountered:
1. Stop at that point in the plan
2. Present the two (or more) options with pros and cons
3. Wait for Luba's decision
4. Proceed only after explicit approval

Do not resolve product decisions by picking the "obviously better" option.
Do not resolve product decisions by reasoning that "Luba would want X."

---

## When to proceed without asking

Some decisions are NOT product decisions. Proceed without asking when:

- The decision is already in DECISIONS-LOCKED.md (it is closed — execute it)
- The decision is a direct instruction from Luba in this conversation
  (the human override protocol: execute first, state the constraint after)
- The decision is a technical implementation detail with no user-facing or
  architectural effect (for example, variable naming, loop style)
- The instruction "continue" or "proceed" was given and the next step
  is unambiguous

The test: "Does this require Luba to make a judgment call about goals,
priorities, or architecture?" If yes → stop and present options.
If no → proceed.

---

## Decisions already made — do not reopen

All decisions in DECISIONS-LOCKED.md are closed. Read that file before
any plan that touches architecture. If a plan proposes something that
contradicts a locked decision, the plan is wrong — not the decision.

Auth questions resolved in prior sessions are also closed.
Do not re-raise a question that was answered in a prior session.
If you are unsure whether a question was answered: check DECISIONS-LOCKED.md
and AGENTS.md before asking.
