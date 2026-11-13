# plan-review-SKILL.md — Check a plan before implementation
## v2.0: FC-1 through FC-12 added; original five questions preserved.

## When to load
Load for ARCHITECT sessions before writing a plan.
Load for REVIEW sessions when evaluating a submitted plan.

---

## Five original questions — required before writing any code

1. Does every placement have a Gate B? (measurable bash pass condition)
2. Does every placement name exactly which file changes?
3. Are new tests listed by name and expected assertion?
4. Is the commit message specified?
5. Is the test count baseline stated?

If any answer is "no" — add the missing element before starting.

---

## No product decisions rule

If a plan requires choosing between two design approaches that affect
architecture or user behaviour: STOP — present both options to Luba,
wait for the decision. Do not self-select between design alternatives.

---

## The twelve plan-check questions — run before approving any plan

### Plan check 1 — Count drift
Every test count appears exactly once as a running total.
877 → 880 → 885. Each number in exactly one place. No contradictions.

### Plan check 2 — Path errors
Every file path is verified against the actual codebase.
Run `grep -n "class BaseService" path/to/file.cs` before writing
"extends BaseService" in a plan.

### Plan check 3 — Phantom components
Every component mentioned in the plan exists in exactly one phase.
No component appears in two phases. No phase creates a component
not listed in the component table.

### Plan check 4 — Duplicate test numbers
No test count appears twice (except the baseline in the pre-condition
and the Round Contract).

### Plan check 5 — Missing coverage
Every component in the component map has at least one test.
No phase completes without a test that verifies its output.

### Plan check 6 — Stale numbers
Baseline is read live from `dotnet test` — not copied from memory
or from a JSON state file.
If a parallel plan ran, recalculate all target counts from the new baseline.

### Plan check 7 — Phase placement
Every phase has: deliverable + Gate B + commit message.
No phase ends without all three.

### Plan check 8 — Self-containment
No plan references "see master plan" or "per CLAUDE.md Rule X"
without inlining the rule. Plans are self-contained documents.

### Plan check 9 — Gate B as literal bash
Every Gate B is a bash command that returns a number or string,
not English prose.
"Verify it works" is not a Gate B.
`dotnet test --filter "SC29_" 2>&1 | grep "passed"` is a Gate B.

### Plan check 10 — Type contract propagation
Every type used in the plan is verified to exist with that exact name
and signature.
Run `grep -rn "public interface ISemanticCompiler"` before using
ISemanticCompiler in a plan.

### Plan check 11 — Overview-detail consistency
The summary table and the phase detail sections must agree on file
names and test counts.
If the table says +5 tests but the detail shows 4 test methods — fail.

### Plan check 12 — Principles compliance
Rule 1: every public method returns OperationResult<T>.
Rule 2: every external dependency accessed through an interface (Fabric First).
Rule 3: every BaseService subclass has `(ISessionStorage storage) : base(storage)`.

---

## Verdict

All twelve checks passed → the plan is ready for EXECUTOR.
Any check failed → state which check, what is missing, and what to add.
