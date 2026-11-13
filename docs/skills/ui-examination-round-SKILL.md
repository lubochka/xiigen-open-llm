# UI Examination Round

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

## Core Problem

A UI review that examines "the form" as a single unit misses 80% of the
quality surface. A form has multiple features, multiple user roles, and
multiple states — and a fix that improves one cell can silently regress
another. Systemic problems affect many cells and should be fixed before
per-cell fixes, because per-cell patches applied before systemic fixes
will be overwritten. This skill defines the cell model, the five-phase
examination structure, the convergence conditions, and the rules that
govern improvement claims.

---

## When to Invoke

- Before any UI commit that affects more than one panel or form feature
- When ui-quality-standard-SKILL.md checker protocol passes but the UI "still doesn't feel right"
- When a UI change has been made and the result needs to be scored and verified
- When starting a new UI examination session after a significant form change

---

## Section 1 — The Cell Model

A cell is NOT "a form." A cell is defined by four dimensions:

```
Cell = Form × Feature × UserRole × State
```

Example cells for the Stage Explorer form:

```
Cell 1: StageExplorer × CapabilityBrowser × Developer × Idle
Cell 2: StageExplorer × CapabilityBrowser × Developer × Loading
Cell 3: StageExplorer × RunHistory × Developer × Empty
Cell 4: StageExplorer × RunHistory × Developer × Populated
Cell 5: StageExplorer × PipelineStatus × Developer × Running
Cell 6: StageExplorer × PipelineStatus × Developer × Failed
```

A form with 3 features × 1 user role × 3 states = 9 cells minimum.
All primary cells must be examined before a form-level improvement claim is made.

---

## Section 2 — Five-Phase Examination Structure

### Phase 1 — Baseline

```
Purpose: Establish a score for every primary cell before touching anything.
Output:  A baseline table: Cell ID / Score / Issues found

Rule: NO improvement claims before a baseline exists.
      If you have not scored the before-state, you cannot claim improvement.
```

Baseline table format:

```
| Cell ID | Form | Feature | State | Baseline Score | Issues |
|---------|------|---------|-------|----------------|--------|
```

Score each cell using ui-quality-standard-SKILL.md Phase 1-4 checker protocol.
Record the score as a decimal (0.00–1.00) and list all issues found.

### Phase 2 — Systemic Fixes

```
Purpose: Identify and fix problems that affect multiple cells.
Rule:    Systemic fixes BEFORE per-cell fixes.
         A per-cell fix applied before a systemic fix will be overwritten
         when the systemic fix changes the shared component.
```

Systemic issues (affecting ≥3 cells):

```
- Spacing unit inconsistency (affects all panels using the shared margin constant)
- Accent color not applied (affects all status indicators)
- Font hierarchy wrong (affects all forms using the shared label style)
- BAN violation (BAN 1-5 from ui-quality-standard-SKILL.md) present in shared component
```

Apply all systemic fixes first. Re-read all affected cells after systemic fixes
to confirm the fix did not introduce new issues.

### Phase 3 — Per-Cell Fixes

```
Purpose: Fix issues specific to one cell that are not shared with others.
Input:   Baseline table with systemic issues already resolved.
Rule:    Only address issues not already resolved by Phase 2.
```

Per-cell fix record format:

```
Cell ID: [cell identifier]
Issue:   [specific issue in this cell]
Fix:     [what was changed — file:line]
Risk:    [which other cells could be affected — or NONE]
```

### Phase 4 — Rescore

```
Purpose: Measure the delta from baseline for every cell touched in Phase 2-3.
Output:  Updated score table with before / after / delta for each cell.
```

Rescore table format:

```
| Cell ID | Baseline | After Fix | Delta | Verdict |
|---------|----------|-----------|-------|---------|
```

Verdict: IMPROVED (delta > 0.05) / STABLE (delta ≤ 0.05) / REGRESSED (delta < 0)

### Phase 5 — Convergence Check

```
Purpose: Determine whether the examination round is complete.
```

Three convergence conditions — ALL must be true to close the round:

```
Condition 1: Score delta < 1% across all cells since last fix pass
             (no more cells are improving — fixing has plateaued)

Condition 2: All primary cells have been examined (Phase 1 baseline exists for each)

Condition 3: Special-case cells marked NEEDS_PURPOSE_BUILT_UI have been verified
             as acknowledged (not just noted as unexamined)
```

If any condition is false: continue examination. State which condition failed
and what step is needed to satisfy it.

---

## Section 3 — Improvement Claim Rules

An improvement claim is a statement that "the UI is better." Improvement
claims have four scopes and corresponding evidence requirements:

```
Form-level claim: "This form is improved"
  Requires: all primary cells rescored with positive or stable delta;
            no REGRESSED verdict anywhere on the form.

Feature-group-level claim: "The capability browser feature is improved"
  Requires: all cells in that feature group rescored;
            no REGRESSED verdict in the feature group.

Fleet-level claim: "All forms in the explorer are improved"
  Requires: form-level claims for every form in the fleet;
            systemic fix verified to not regress any form.

Convergence claim: "This examination round is complete"
  Requires: all three convergence conditions satisfied (Section 2 Phase 5).
```

**A claim at any scope without the required evidence is invalid.**
"The form looks better" without a rescore table is not an improvement claim.

---

## Section 4 — NEEDS_PURPOSE_BUILT_UI Cells

Some cells cannot be improved by systematic fixes — they require a
purpose-built UI design that is out of scope for the current change:

```
Mark as: NEEDS_PURPOSE_BUILT_UI
Provide: one sentence explaining why the standard approach does not apply
Action:  acknowledge the cell as a known limitation; do not block convergence on it
```

Example:

```
Cell 6: StageExplorer × PipelineStatus × Developer × Failed
NEEDS_PURPOSE_BUILT_UI: Error display requires a custom diagnostic panel
                         not available in the current Windows Forms component set.
                         Out of scope for this examination round.
```

A cell marked NEEDS_PURPOSE_BUILT_UI satisfies Convergence Condition 3.
It does NOT satisfy Convergence Condition 2 if primary cells remain unexamined.

---

## Anti-Patterns

1. **"I reviewed the form and it looks better"** — a form-level improvement
   claim requires a rescore table. "Looks better" without delta values is not
   an examination result.

2. **"I fixed the per-cell issue before the systemic fix"** — the per-cell fix
   will be overwritten when the systemic fix changes the shared component.
   Systemic fixes first; per-cell fixes second.

3. **"I only scored the cells I changed"** — baseline scoring must cover all
   primary cells. Cells you did not change can still be regressed by systemic
   fixes. Score all primary cells in Phase 1.

4. **"NEEDS_PURPOSE_BUILT_UI means this cell is done"** — it means it is
   acknowledged, not fixed. The convergence claim must note this cell as an
   open limitation, not as a resolved item.

5. **"The delta is 0.03 — that counts as improvement"** — delta ≤ 0.05 is
   STABLE, not IMPROVED. The cell is not better; the noise in the scoring
   is that large. Claim improvement only at delta > 0.05.

---

## Integration

- ui-quality-standard-SKILL.md checker protocol (Phases 1-4) provides the per-cell scoring method
- ui-testing-SKILL.md handles functional correctness; this skill handles design quality rounds
- verification-before-completion-SKILL.md fires before any completion claim — a UI phase requires a convergence claim from this skill
- Three-level verification (unit + integration + suite) from three-level-verification-SKILL.md verifies functional correctness; this skill verifies design quality
