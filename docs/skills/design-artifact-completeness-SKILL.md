# Design Artifact Completeness

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

A C# file that exists on disk is not automatically an implemented capability.
An interface that is defined is not automatically an interface that is wired.
A test class that exists is not automatically a test class that has assertions.
This skill installs 5 orthogonal checks that distinguish "file exists" from
"file is a valid input to downstream processes."

---

## When to Invoke

- During reconnaissance, for every subproject the session's scope touches
- Before any plan step that depends on "X is implemented" or "X is designed"
- When claim-as-hypothesis-SKILL captures a claim about "X is done / complete / implemented"
- Before writing a phase plan that builds on existing artifacts

One completeness check before planning = zero "built on empty stub" execution failures.

---

## Section 1 — The 5 Checks

Each artifact is evaluated on 5 dimensions. Each check is independent —
an artifact can pass Check 1 and fail Check 2 (the classic stub pattern).

### Check 1 — Files Exist

**What it verifies:** all expected files for the capability are present on disk.

**Detection:**
```bash
ls src/[Project]/[ClassName].cs
ls src/[Project]/I[InterfaceName].cs
ls tests/[Project]/[ClassName]Tests.cs
```

**PASS** = file exists at expected path.
**FAIL** = missing.

**Failure mode caught:** capability referenced in plan but file never created.

---

### Check 2 — Fields/Methods Populated

**What it verifies:** the implementation has real content, not just a skeleton.

**Detection:**
```bash
# Check for throw-only method bodies (NotImplementedException stubs)
grep -n "NotImplementedException\|throw new\|TODO\|FIXME" src/[Project]/[ClassName].cs

# Check for empty methods
grep -n "{ }" src/[Project]/[ClassName].cs

# Check test class has at least 1 [Fact] or [Theory]
grep -c "\[Fact\]\|\[Theory\]" tests/[Project]/[ClassName]Tests.cs
```

**PASS** = 0 NotImplementedException hits; ≥1 [Fact]/[Theory] in test class.
**FAIL** = any NotImplementedException; 0 test facts.
**PARTIAL** = some methods implemented, some throw; tests exist but few.

**Failure mode caught:** the stub-as-implementation pattern — file exists, builds, but has no real behavior.

---

### Check 3 — Content Specific

**What it verifies:** the implementation is specific to this capability, not copy-pasted from another.

**Detection:**
```bash
# Compare method names to adjacent capabilities
grep -n "public.*OperationResult" src/[Project]/[ClassName].cs
# Verify method names match the capability's stated contract
```

**PASS** = method names match the capability spec; content is capability-specific.
**PARTIAL** = method signatures match a template; implementation body is generic.
**FAIL** = methods have identical names to another class with no distinction.

**Failure mode caught:** copy-paste code that compiles but does the wrong thing.

---

### Check 4 — Matches Specification

**What it verifies:** the implementation aligns with its declared interface.

**Detection:**
```bash
# Every interface method has a concrete implementation
grep -n "interface I[Name]" src/[Project]/I[Name].cs | grep -o "[A-Z][a-zA-Z]*("
# Verify each method name appears in the concrete class
grep -n "[MethodName]" src/[Project]/[ClassName].cs

# Verify the concrete class is registered
grep -n "[ClassName]\|I[Name]" src/[Project]/Program.cs
```

**PASS** = interface ↔ concrete class bidirectional match; class registered.
**PARTIAL** = some methods match; registration missing.
**FAIL** = mismatch without explanation; concrete class has extra/missing methods vs interface.

**Failure mode caught:** interface-implementation drift — contract says one thing, implementation does another.

---

### Check 5 — Reachable by Consumers

**What it verifies:** the capability can actually be called by the components that need it.

**Detection:**
```bash
# Project reference exists
grep -n "[ProjectName]" [ConsumerProject].csproj

# Consumer can resolve the type
dotnet build [ConsumerProject] → 0 Error(s)

# Tests exercise the capability through its interface
grep -n "I[InterfaceName]" tests/[ConsumerProject]/[ConsumerTest].cs
```

**PASS** = project reference present; build succeeds; tests call through interface.
**PARTIAL** = project reference present; build succeeds; no consumer tests yet.
**FAIL** = project reference missing; build fails; type not resolvable.

**Failure mode caught:** capability built in isolation that never reaches the consuming components.

---

## Section 2 — Per-Artifact Output Format

```markdown
## Design Artifact Completeness (design-artifact-completeness-SKILL)

Capability: [CapabilityName]

| Artifact | C1 (exists) | C2 (populated) | C3 (specific) | C4 (matches spec) | C5 (reachable) | Status |
|---|---|---|---|---|---|---|
| I[Name].cs | PASS | PASS | PASS | — | — | PASS |
| [Name].cs | PASS | PARTIAL | PASS | PARTIAL | FAIL | INCOMPLETE |
| [Name]Tests.cs | PASS | FAIL (0 facts) | — | — | — | INCOMPLETE |

Overall: INCOMPLETE
Blocking: C2 on [Name].cs (NotImplementedException in 3 methods), C2 on [Name]Tests.cs (0 [Fact])
Non-blocking: C4 PARTIAL (registration missing — add to Program.cs)
```

### Overall Status Categories

| Status | Meaning |
|---|---|
| COMPLETE | All 5 checks PASS across all artifacts |
| DESIGN_COMPLETE_NOT_WIRED | Checks 1–4 PASS, Check 5 FAIL (implementation done, not registered) |
| PARTIAL | Some checks PASS, some FAIL, no structural blockers |
| STUB | Check 2 FAIL — file exists but throws NotImplementedException |
| MISSING | Check 1 FAIL on any required artifact |

---

## Section 3 — Gate Enforcement

**Blocking checks:** Check 1 (missing file) or Check 2 (stub/empty) blocks treating the artifact as a complete input to plans.

Plan options when a referenced artifact fails blocking checks:
1. **Implement first** — add a task to the plan to implement the stub before proceeding
2. **Scope around the gap** — acknowledge in the plan: "this phase proceeds without X; consequence is Y"
3. **Mark DEFERRED via claim-as-hypothesis-SKILL** — Luba approves with explicit reason

Silent assumption that a stub is usable is not an option.

For architecture, trainable, code-review, documentation, or audit artifacts,
completeness also requires the Human-Readable Architecture Artifact Gate. The
artifact is incomplete if it is table-only, shortcode-heavy, English-only when
Russian is required, schema-only, or prompt/spec. A code quote without an
adjacent concrete repair blueprint is incomplete even when the file exists.

Concrete Repair Blueprint Gate is required for suspicious Dictionary/counter/
KNN/scorer/selector/private decision methods: exact classes, records, methods,
tests, data rows, sets/checkpoints/manifests, and lifecycle paths must be named.
Generic "wrap in ILogicalUnit", "add KNN", or "chosen/rejected/quarantine" text
не является решением.

**Non-blocking checks:** Check 3 (content-specific), Check 4 (matches spec), Check 5 (reachable)
are surfaced as CONCERNs but do not block the plan from proceeding.

---

## Anti-Patterns

1. **"The file exists, so the design exists"** — Check 1 alone is not completeness. `Check 2` is the difference between existence and usability.

2. **"The stub will be implemented later"** — "later" is a task. If the stub must be implemented before the plan works, the plan has a task to implement it first.

3. **"NotImplementedException is fine while prototyping"** — a stub marked as STUB status blocks treatment as a complete input. A plan that depends on a STUB must implement it first or explicitly acknowledge the gap.

4. **"Tests exist"** — a test class with 0 `[Fact]` attributes is not a test class. Check 2 on tests requires `≥1 [Fact]`.

5. **"Wiring is trivial, skip Check 5"** — constructor injection that is not registered produces `NullReferenceException` at runtime. Check 5 is not optional.

---

## Integration

- claim-as-hypothesis-SKILL.md handles user claims about artifact completeness; this skill is the verification action
- prerequisite-chain-SKILL.md uses Check 1–2 results to classify prerequisites as PASS/FAIL/BLOCKING
- mvp-round-trip-verification-SKILL.md Step 3 (registered) is equivalent to this skill's Check 5
- plan-self-validation-SKILL.md Check 2 (phantom skills) is a specific instance of this skill's Check 1 applied to skill files
