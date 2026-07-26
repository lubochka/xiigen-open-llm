# Skill: Implementation Documentation — Implementation Status Table (§4)

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

## Purpose

Every implementation document must end with a status table showing the current implementation state of every component. This is the single source of truth for "what works, what doesn't" before any plan starts.

## Required table format

| Component | File | Status | What actually works now |
|---|---|---|---|

**Columns:**
- **Component** — class name with backtick formatting
- **File** — relative path from project root, no absolute paths
- **Status** — one of the four status symbols below
- **What actually works now** — concrete description, not "see above"

## Status symbols

| Symbol | Meaning | When to use |
|---|---|---|
| ✅ Full | All public methods implemented, tested, checkpoint works | Use when nothing returns stub/NOT_READY |
| ⚠️ Partial | Core path works, some ILearningMachine methods are stubs | Use when ExportAsync/ImportAsync return FAIL, or TrainBatchAsync is scaffold |
| 🔶 Skeleton | Contract-only — data types exist, all inference returns constant stub values | Use when Translate/Plan always returns "not_ready_no_..." status |
| ✅ Bootstrap | Deterministic wrapper — always Confidence=1.0, wraps static logic | Use for Bootstrap units that will be replaced by trained models |
| ✅ Hardcoded | Returns fixed values — not trained, not configurable | Use for registries/factories with hardcoded lists |

## What the "What actually works now" column must contain

- For ✅ Full: list the key working paths (e.g., "All 3 modes, checkpoint save/load, evaluate")
- For ⚠️ Partial: explicitly name working methods AND stub methods:
  - ✅ "`TrainMachineBatchAsync`, `DecideFromTrainedState`, checkpoint; `ExportAsync`/`ImportAsync` → FAIL"
  - ❌ "Partially implemented" (no detail)
- For 🔶 Skeleton: state what is SAVED (preserved contract fields) and what ALWAYS returns stub:
  - ✅ "`Translate()` preserves SHA256/language/constraints; `TaskType` is always `not-ready`"
  - ❌ "Not implemented"

## Validation gate

Before submitting §4:
- [ ] Every component mentioned in §1-§3 has a row
- [ ] No row uses a status symbol not in the allowed list
- [ ] "What actually works now" column contains specific method names or return values, not prose
- [ ] Partial and skeleton components explicitly list which methods work and which don't

## Integration

Required by: `planning-session-startup-SKILL.md` (pre-execution doc gate), `plan-review-SKILL.md` (FC-17)
