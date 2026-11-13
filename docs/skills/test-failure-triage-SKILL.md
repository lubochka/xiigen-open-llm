# Test Failure Triage

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

## What This Skill Prevents

Для учебных проверок и переносов читать [договор45: постановка, сборка и эскалация](../plans/replanning-possible-diffs/experimets/45-learning-claim-and-native-acceptance-contract-2026-11-03.md). Если прежний запуск неизвестен, это не доказательство «раньше проходило» или «тест новый». Сначала фактическая сборка затронутого продукта и тестового проекта; затем назначенные тесты той же сборки. При разрешённой реализации отсутствие запуска не позволяет закрыть работу. Ошибка compiler/discovery/setup отделяется от падения assertion.

Отсутствующий урок — отдельный TEST_DESIGN_GAP для утверждения об обучении, а не автоматическое оправдание неверного ответа или нарушенного обязательного отказа. Сохранять observed input/output, gold и отрицательный контроль; архитектор назначает отдельный учебный опыт вместо ручного продуктового исключения. Пакет эскалации содержит команду, версию реально вызванной DLL, полный журнал, expected/actual и следующий ограниченный шаг.

Treating all test failures identically. Modifying production code to silence a
fragile test. Patching the wrong layer and leaving the real bug untouched.

Rule 4 requires `Failed: 0`. Without classification, ALL failures get the same
response (fix production code). This is wrong when the test itself is wrong, or
when the infrastructure changed beneath the code.

---

## Step 1 — Capture the Failure Report

```bash
dotnet test <explicit-test-project> --configuration <same-configuration> --logger "trx;LogFileName=triage.trx"
# Look for: test names, error messages, stack traces
grep -E "Failed|Error|at " /tmp/test-output.txt | head -30
```

---

## Step 2 — Classify Each Failure

For each failing test, apply this decision tree:

```
1. Which stage failed: build, discovery, setup, Act, assertion or teardown?
   This identifies the symptom's location, not its cause.
2. Does an independent approved contract justify the expected value and fixture?
   NO → repair the incorrect test within the assigned repair (CLASS 3), or
        escalate a genuinely disputed requirement. Do not copy actual into gold.
   UNKNOWN → investigate the contract and reachable path; preserve the failure.
   YES → trace the actual cause:
         broken fixture/environment → CLASS 4;
         changed dependency/registration → CLASS 2;
         product violates the contract → PRODUCT_CONTRACT_DEFECT / CLASS 1.
3. Was the same case passing on a matching earlier build and environment?
   YES → a regression is established; identify the responsible change by evidence.
   NO or UNKNOWN → do not call it a new regression. A failing baseline or new
                   test can reveal an existing defect. Keep chronology explicit.
```

---

## Class 1 — Product Contract Defect / Established Regression

**Definition:** Production behavior violates an independently established contract. Call it a regression only when matching earlier passing evidence establishes that the invariant previously held. A new test can reveal an old defect; it does not prove that the latest change introduced it.

**Detection:**

```bash
# Compare changed files to find what was modified
git diff --name-only HEAD
# Then read each changed file completely (Rule 5)
# Find which change broke the invariant the test was verifying
```

**Response:**
1. DO NOT modify the test — the test is correct
2. Find what changed in the production class and why the invariant broke
3. Fix the production class to satisfy the invariant again
4. Apply root-cause-tracing-SKILL.md if the break is not obvious
5. Re-run the specific test after the fix

---

## Class 2 — Infrastructure Regression

**Definition:** A dependency, DI registration, or environment changed. The production
code and the test are both correct. The test fails because the environment changed beneath them.

**Detection:**

```bash
# Check if DI registrations changed
git diff --name-only HEAD | grep -i "program\|startup\|module\|registration"

# Check if an interface or constructor signature changed
git diff HEAD -- Src/ | grep "public.*interface\|public.*class\|public.*\(.*IService"
# If interface changed: all implementations may need updating
```

**Response:**
1. DO NOT modify the production algorithm
2. Fix the infrastructure: update DI registration, update interface implementation, fix setup
3. Re-run tests after the infrastructure fix only
4. Record in issue inventory: "FIXED: infrastructure regression — [what changed]"

---

## Class 3 — Test Fragility

**Definition:** The assertion or fixture conflicts with an independently approved
contract, relies on unsupported assumptions, or checks incidental implementation
details. Do not claim it was always fragile or passed by accident without history.

**Detection markers:**
- Test asserts on exact method call counts (`Times(3)`)
- Test asserts on internal state that should not be observable from outside
- Test depends on a specific execution order
- Test uses exact string matching on messages that may change
- Test mocks the class under test instead of the class's dependencies

These are investigation clues, not automatic defects. Exact refusal codes,
ordering and call counts can be explicit contracts (for example, no mutation
after refusal or a fixed training dose). Preserve those checks. An assertion
after Act may report a failed setup result; its line number does not exclude
CLASS 4. File ownership also does not establish cause or regression chronology.

```bash
# Look for fragility indicators in changed test files
grep -n "Times\|CallCount\|Invocations\|Sequence\|InOrder" Tests/ -r --include="*.cs"
# Behavioral tests should assert outcomes, not call counts
```

**Response:**
1. Fix the test to assert behavior, not implementation details
2. Apply the independent-contract rule from bug-to-tests-SKILL.md and QA46: adapt an incorrect assertion within the assigned repair, recording its authority and preserved control. A change to the user requirement needs a decision; routine test correction does not need renewed permission.
3. After rewriting: run the test to confirm it passes for the right reason
4. Record: "FIXED: test fragility — rewritten to assert behavior"

---

## Class 4 — Test Environment Failure

**Definition:** The test setup itself is broken. The test cannot reach the code under
test because DI setup failed, required service is not registered, or test data is missing.

**Detection:**

```csharp
// Common Class 4 error patterns in xUnit:
// - InvalidOperationException: No service for type 'IVectorIndex' has been registered
// - NullReferenceException at line 10 of test (before any Act code)
// - System.TypeInitializationException in setup code
```

```bash
# Verify the test setup by running in verbose mode
dotnet test --filter "SpecificTestClass" -- --verbosity detailed 2>&1 | head -50
# Look for setup errors before the test body runs
```

**Response:**
1. DO NOT modify the production class
2. DO NOT apply a patch to "fix" the test body
3. Fix the test setup: add missing DI registration, provide missing test data
4. Confirm the setup fix by running the test — the class being tested should now be reached

**Diagnostic clue, not a classifier:** setup often fails before Act, but product defects may also throw during construction or Act. Trace the cause and contract; do not classify solely by the exception's line number.

---

## Step 3 — Record Classification

Before fixing anything, record the classification:

```
Failure triage: [date]

Test: TokenizerTests.Tokenize_EmptyInput_ReturnsEmptyList
  Class: CLASS_3_TEST_FRAGILITY
  Evidence: Test asserts Times(1) on internal mock call — not a behavioral assertion
  Response: Rewrite to assert result is empty list, not call count

Test: HybridRetrieverTests.Search_ReturnsTopK
  Class: CLASS_2_INFRASTRUCTURE_REGRESSION
  Evidence: IVectorIndex not registered after DI refactor in Program.cs
  Response: Add HnswIndex registration to test ServiceCollection setup
```

---

## Anti-Patterns

1. **Patching Class 3 failures by changing production code** — makes tests pass but code is now wrong
2. **Fixing Class 1 failures by changing the assertion** — violates the Iron Rule of bug-to-tests-SKILL.md
3. **Not classifying before acting** — fixing the wrong layer every time
4. **Treating "test failed" as "code is wrong"** — Class 2, 3, 4 failures are not code bugs
5. **Committing with unclassified failures** — classification is the first step, never skip it
