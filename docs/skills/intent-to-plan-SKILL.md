# Intent to Plan

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

## Fresh Execution / Method / Humility Intent Hook

Intent parsing must preserve the latest unquoted command as fresh authority:
- `REPEATED_CURRENT_COMMAND_IS_FRESH_EXECUTION_GATE`: repeated current command is a fresh execution trigger; old artifacts are baseline evidence, not completion.
- `EXPLICIT_METHOD_OVERRIDE_GATE`: method/model/reasoning/count/folders/target/sub-agent route are intent clauses when Luba specifies them, not preferences.
- `TOO_FAST_HIGH_PASS_SUSPICION_GATE`: N-pass/high-pass/large-package requests require per-pass ordered evidence, not one generated ledger.
- `LARGE_VOLUME_MODEL_HUMILITY_AND_FOCUS_GATE`: broad or multi-focus intent must be sliced and tracked in a focus ledger before completion.
- `FINAL_ANSWER_FRESHNESS_GATE`: final wording must state freshly executed this turn vs only verified for the latest repeated command.

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

1. **Technology names in plan steps.** When planning "add BM25 scoring to
   HybridRetriever", the natural default is to write "create BM25Scorer class
   using Okapi BM25 formula in C#." That is a technology-scoped step, not a
   plain-language step. The initial plan must be infrastructure-neutral.

2. **Scope drift from sentence to implementation.** The INTENT field must be
   the user's request verbatim. Every paraphrase changes scope.

3. **Steps that combine responsibilities.** A step containing "and" usually
   contains two steps. Combined responsibilities make Gate B ambiguous — a step
   covering two things has no single measurable Gate B.

---

## When to Invoke

Load before writing any REFERENCE-PLAN or EXECUTION file phase descriptions.
Use alongside phase-planning-SKILL.md — this skill governs WHAT to put in each
plan step; phase-planning-SKILL.md governs the 8-gate validation.

---

## Section 1 — Parsing the User's Request

An **intent clause** is one distinct capability the user is requesting.
Parse the user sentence to identify all intent clauses before writing anything.

### Parsing rules

| Signal | What it means |
|--------|--------------|
| Every noun phrase naming a capability | One clause |
| Technology qualifiers ("in C#", "using xUnit") | Context — not a clause |
| Multiple verbs describing distinct actions | One clause per verb |
| "and" connecting two capabilities | Two clauses |
| Ambiguous phrase that could mean two things | Flag as ambiguous — ask |

### Clause table (fill before writing the plan)

| # | Clause | Source phrase | Ambiguous? | Question if ambiguous |
|---|--------|---------------|-----------|----------------------|
| 1 | | | | |

### Example — BM25 scoring

Request: "Add BM25 scoring to HybridRetriever so it can combine text and vector results."

| # | Clause | Source phrase | Ambiguous? | Question |
|---|--------|---------------|-----------|---------|
| 1 | BM25 scoring | "BM25 scoring" | No | — |
| 2 | combine text and vector results | "combine text and vector results" | No | — |

Result: 2 clauses.
- Minimum phases: max(2, 2) = 2 (formula), adjusted for complexity → 3
- Maximum phases: min(2×3, 8) = 6 (formula), adjusted → 4

State the reasoning. Do not use default ranges without justification.

---

## Section 2 — What a Valid Plan Phase Looks Like

Every phase in a plan must satisfy all 3 properties.

### Property 1 — PLAIN LANGUAGE

The phase names a capability, not a technology implementation.

```
Good: "Build the BM25 scoring interface and default implementation"
Bad:  "Create IBm25Scorer interface with Score(string query, string document) method"
      — contains specific interface names and method signatures

Good: "Integrate BM25 and vector scores into a combined ranked result"
Bad:  "Modify HybridRetriever.SearchAsync() to call IBm25Scorer.Score() for each result"
      — contains class names and method names
```

The technology-level detail belongs in the EXECUTION file, not the REFERENCE-PLAN.

### Property 2 — SINGLE RESPONSIBILITY

One phase = one distinct capability. "And", "also", "as well as" in the same
phase description is a candidate split point.

```
Good: "Define the BM25 scoring interface"
Bad:  "Define the BM25 scoring interface and integrate it into HybridRetriever"
      — two capabilities (define + integrate)

Split rule: If you can draw a line between two parts and each part makes sense
            alone — split the phase.
```

### Property 3 — GATE B DECLARED

Every phase has a measurable Gate B condition (Rule 8 from CLAUDE.md).
A phase without Gate B is not a valid plan phase.

```
Good: "Gate B: dotnet test --filter IBm25Scorer → 5+ tests pass, 0 fail"
Bad:  "Gate B: BM25 scoring works correctly"
      — "works correctly" is not a measurable condition

Gate B format: [command] → [exact expected value or comparison]
```

---

## Section 3 — Plan Reviewer Protocol

After writing a plan, apply these checks to each phase:

### Check 1 — COVERAGE

For each intent clause: does at least one phase address it?

```
Verdict per clause: COVERED | PARTIAL | MISSING
COVERED:  The clause is the primary subject of at least one phase
PARTIAL:  The clause is a side effect of another phase
MISSING:  No phase addresses this clause
```

### Check 2 — ABSTRACTION

For each phase: does it contain a class name, method signature, or C# keyword?

```
Technology list for this plan (derive per plan):
  - Class names: IBm25Scorer, BM25Scorer, HybridRetriever
  - Method names: Score(), SearchAsync(), AddSingleton()
  - C# keywords: interface, class, async, Task<T>

Verdict: CLEAN | VIOLATION
Plan fails abstraction if ANY phase has VIOLATION in the REFERENCE-PLAN.
(Technology detail belongs in EXECUTION files, not REFERENCE-PLAN.)
```

### Check 3 — SINGLE RESPONSIBILITY

For each phase: does it describe more than one distinct action?

Signal words: "and", "also", "as well as" → flag as compound.
Plan fails if more than 20% of phases are compound.

### Check 4 — GATE B COMPLETENESS

For each phase: is there exactly one Gate B with a measurable condition?

```
Verdict: PRESENT | MISSING | VAGUE
PRESENT: "[dotnet command] → [exact value]"
MISSING: No Gate B
VAGUE:   "works correctly", "tests pass", "implementation complete"
         → must be replaced with exact command + expected output
```

---

## Anti-Patterns

1. **Technology names in REFERENCE-PLAN** — REFERENCE-PLAN describes capabilities;
   EXECUTION files describe implementation steps with class names and methods.

2. **100% first-run coverage** — suspicious. Check whether the plan addresses the
   user's actual clauses, not a standard pattern.

3. **"While I'm here" phases** — adding phases not requested by the user. The plan
   must address the user's clauses and no more (no-product-decisions-SKILL.md).

4. **Vague Gate B** — "tests pass" is not Gate B. "dotnet test --filter BM25 →
   5+ tests pass, 0 fail" is Gate B.
