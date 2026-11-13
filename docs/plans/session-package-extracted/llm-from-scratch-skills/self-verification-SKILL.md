# Self-Verification — LlmFromScratch
## For: Before declaring any fix "done"

---

## When to Invoke

- Before declaring any fix "done"
- Before any phase-end gate (one commit per phase)
- After any algorithm, BaseService, Core.cs, or Fabric/Interfaces.cs change
- When a test passes but confidence in the fix level is low

---

## The 6 Rules

| # | Rule | What it catches |
|---|------|----------------|
| 1 | `fresh-fixtures` | Using minimal hand-crafted input instead of real-usage examples |
| 2 | `categorize` | Wrong change category — treating a DNA rule fix as a minor refactor |
| 3 | `right-level` | Verifying at the wrong test level — unit only when integration is required |
| 4 | `cascade` | Missing downstream effects — `Core.cs` change → all subprojects |
| 5 | `non-renderable` | Claiming algorithm correctness without running it on real input |
| 6 | `predict` | Not predicting which subprojects and experiments are affected before changing |

---

## Rule 1: Fresh Fixtures

**Real usage inputs only.** Integration tests must use inputs that mirror
actual LlmFromScratch usage patterns, not minimal synthetic strings.

```csharp
// WRONG — hand-crafted minimum input that may not exercise the algorithm
var input = new PipelineInput("x", null);

// RIGHT — input that mirrors real Claude Code usage
var input = new PipelineInput("validate null check in user authentication loop", null);

// RIGHT — input from actual experiment E-N test cases
var input = new PipelineInput("implement dependency injection for service registry", null);
```

**Check:** Does my integration test input resemble what actually flows through
this algorithm in production sessions?

---

## Rule 2: Categorize

**Classify the change before testing.** Every change is one of:

| Category | Definition | Test requirement |
|----------|------------|-----------------|
| `DNA_VIOLATION_FIX` | Fixes Rule 1–10 violation | three-level-verification: L1 + L2 + L3 mandatory |
| `ALGORITHM_CHANGE` | Changes algorithm output | L1 + L2 + L3 mandatory |
| `REFACTOR` | Same output, different structure | L1; verify L2/L3 still pass |
| `NEW_EXPERIMENT` | Adds E-N algorithm | L1 + L2; L3 for composite pipeline |
| `INFRASTRUCTURE` | Core.cs, BaseService, Interfaces | L1 + L2 + L3 — all subprojects |

Wrong category → wrong test level chosen → false confidence.

**Infrastructure changes are the highest risk:** changing `OperationResult<T>` in
`Core.cs` affects every public method in every subproject. Treat as `INFRASTRUCTURE`.

---

## Rule 3: Right Level

**Match the test level to the blast radius.**

| Change type | Minimum level |
|-------------|--------------|
| Single algorithm method | L1 unit (isolated algorithm) |
| Algorithm that feeds another | L2 integration (real caller chain) |
| `Core.cs` or `BaseService` | L2 + verify all subprojects compile |
| `Fabric/Interfaces.cs` | L2 + verify all subprojects compile and test |
| Composite pipeline | L3 full suite |

**Stop rule:** If L1 passes but the blast radius requires L2 → the change is NOT verified.
Do not proceed to the next phase.

---

## Rule 4: Cascade

**Trace the full effect chain before and after every change.**

```
Core.cs change (OperationResult<T>) → every public method in every subproject
BaseService change              → every class inheriting BaseService
Fabric/Interfaces.cs change     → every class implementing the changed interface
Algorithm output shape change   → every caller in the same pipeline
Experiment E-N output change    → every experiment that depends on E-N output
```

**Check before changing:** "What are all the things that use the thing I'm changing?"
Run this:

```bash
# Find everything that uses a changed interface or class
grep -rn "InterfaceName\|ClassName" . --include="*.cs" | grep -v "/bin/\|/obj/"
```

List every file. Verify each still compiles and tests still pass.

---

## Rule 5: Non-Renderable

**Algorithm correctness cannot be verified by reading the code.**
An algorithm that looks correct may produce wrong output on specific inputs.

**Required steps for any algorithm change:**
1. Run L1 unit test against the specific input that triggered the bug
2. Run L1 unit test against boundary inputs (empty string, single word, very long input)
3. Run L2 with real-usage input
4. If the algorithm is part of a composite pipeline: run the full pipeline
5. Check `OperationResult<T>.IsSuccess` is true AND `result.Value` contains the expected data

"Looks correct to me" is not verification.

---

## Rule 6: Predict

**Before changing, predict the effects. After changing, verify the predictions.**

Pre-change prediction checklist:
```
☐ Which DNA rules (1–10) does this change affect?
☐ Which algorithms in the same subproject call the thing being changed?
☐ Which other subprojects depend on this subproject?
☐ Does the experiment it belongs to still produce the same Gate B result?
☐ Does `dotnet build LlmFromScratch.sln` still pass (zero errors)?
☐ Are there sibling algorithms that need the same change?
```

Post-change: confirm each prediction was correct or explain why it wasn't.

---

## Anti-Patterns

1. **"All unit tests pass" declared as done.** Unit tests use Null* stubs.
   They do not verify integration, real input handling, or composition.

2. **"I reviewed the code and it looks correct."** Visual inspection is not
   verification. Run the algorithm on real input, check the output.

3. **Skip cascade check because "it's a small change."** Small changes in
   `Core.cs` affect all subprojects. Small changes in a shared algorithm affect
   every experiment that uses it.

4. **Verify at L1 only because L2 "needs more setup."** Setup cost is one-time.
   False confidence cost is measured in sessions of debugging.

---

## Integration

```
self-verification
  → confirms fix is verified before three-level-verification declares done
  → confirms cascade before bug-to-tests closes the bug
  → generates predict checklist for planning-skill phase gates
```
