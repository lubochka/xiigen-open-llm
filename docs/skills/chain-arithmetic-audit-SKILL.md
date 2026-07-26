# Chain Arithmetic Audit

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

Verifies that `baseline + Σ(phase deltas) = final` for every tracked metric
across all sessions and phases. claim-verification-SKILL.md checks single-document
consistency; this skill checks end-to-end chain math.

---

## When to Invoke

- At session end when writing a Section A summary — verify "Before" equals the
  previous session's "After"
- Any time a plan spans multiple sessions or phases with incremental counts
- Whenever a test count or capability metric has been reported across sessions
- Before claiming "N tests passing" — verify the number chains back to baseline

---

## Rules

1. Extract all count references from STATE-Pn.json, REFERENCE-PLAN, and every
   SESSION or EXECUTION file
2. Build directed chain: `baseline → phase1 delta → phase2 delta → ... → final`
3. Verify every edge: `node[n].after == node[n+1].before`
4. Verify terminal: `baseline + Σ(all deltas) == final_expected`
5. Any gap or mismatch = **CHAIN ARITHMETIC FAIL** with exact location and value

---

## Detection Commands

```bash
# Get the live test count to anchor the chain
dotnet test --no-build 2>&1 | tail -3
# This is the authoritative "after" for the current session

# Cross-check against what STATE-Pn.json records
grep -n "testCount\|passed\|baseline" docs/STATE-*.json | head -20

# Find all phase deltas recorded across state files
grep -rn "\"before\"\|\"after\"\|\"delta\"\|\"baseline\"" docs/STATE-*.json
```

---

## Verification Table Format

| Transition | Before | Delta | After | Verified |
|-----------|--------|-------|-------|---------|
| Start of phase 4 | 865 | +16 | 881 | ✅ |
| Phase 4 → phase 5 | 881 | +35 | 916 | ✅ |
| Phase 5 → current | 916 | +N | 916+N | run dotnet test |

Every row's After must equal the next row's Before.

---

## STATE-Pn.json Chain Fields

Record these in every STATE-Pn.json at session end:

```json
{
  "testChain": {
    "sessionStart": 916,
    "sessionEnd": 932,
    "delta": 16,
    "previousSessionEnd": 916,
    "chainVerified": true
  }
}
```

If `sessionStart != previousSessionEnd` → chain is broken → investigate before
claiming test counts.

---

## Anti-Pattern

Plan said `expected_test_count: 950` but phase deltas summed to 948. claim-verification-SKILL.md
passed (each individual document was internally consistent). Chain arithmetic failed because
one phase's "after" was recorded as 932 but the next phase started at 934. This skill
catches what single-document verification misses: the edges between documents.

---

## Integration

- Extends plan-review-SKILL.md count drift check
- Invoked by prerequisite-chain-SKILL.md at session start (verify test baseline matches
  the previous session's end count)
- Results feed into Section A "zero-regression delta" at session end
- Feeds into phase-git-report-SKILL.md (test delta must chain cleanly to the commit)
