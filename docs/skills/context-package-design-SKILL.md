# Context Package Design

## Luba Why / Loophole Repair-First Rule

When Luba asks "why" (or its operator-language equivalent, e.g. "почему"), challenges an answer, or says a failure came
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

A document that tells an AI system what steps to execute is an instruction file.
A document that tells an AI system what it needs to know to make a decision is a
context package. Confusing the two produces AI calls that either have too little
context (wrong decisions) or are over-constrained by instructions (no genuine
decision-making). This skill defines the 5-field structure of a valid context
package and how to verify it before use.

---

## When to Invoke

- Before writing any context input passed to ContinuousLearningPipeline
- Before authoring any IncrementalTrainer training context
- Before designing any DomainModelRegistry query package
- When reviewing another agent's context input and verifying it is not an instruction file

---

## Section 1 — The Fundamental Distinction

| Instruction file | Context package |
|---|---|
| Tells the system: do step 1, then step 2 | Tells the system: here is the intent, constraints, context, and success criteria |
| Output is prescribed — system executes it | Output is decided by the system — context enables the decision |
| Fails if a step is ambiguous | Fails if a field is missing or mis-filled |
| System is the actor | The learning pipeline is the actor |
| Self-containment check: can the system run this alone? | Self-containment check: can the pipeline make a defensible decision from this alone? |

**The one-sentence test:**
If the document tells the system WHAT to produce — it is an instruction file.
If the document tells the system WHAT IT NEEDS TO KNOW to decide — it is a context package.

Practical test: remove all field labels. Read the document. Does it sound like a
recipe (do this, then do that) or a briefing (here is the situation, constraints,
what success looks like)? A recipe is an instruction file. A briefing is a context package.

---

## Section 2 — The 5-Field Structure

Every context package has exactly these 5 fields. No more. No fewer.
If a field is absent, the package is incomplete and must not be used.

### INTENT

| | |
|--|--|
| **Definition** | The exact thing this pipeline call is trying to understand or decide |
| **What belongs** | The user's request or system goal verbatim — no rephrasing |
| **What does NOT belong** | Interpretation, clarification, scope expansion |
| **Empty fails because** | The pipeline solves what is written. Empty = invented problem. Rephrased = different problem. |

### DOMAIN

| | |
|--|--|
| **Definition** | What this subproject does, in 2–3 sentences |
| **What belongs** | Capability classes, user-facing behaviors, domain rules |
| **What does NOT belong** | Technology names (.NET, xUnit, EF Core), class names, method signatures |
| **Empty fails because** | Without domain context, the pipeline produces generic output unconstrained by what the subproject actually does. |

### CONSTRAINTS

| | |
|--|--|
| **Definition** | Machine-enforced invariants — things that must hold regardless of what the pipeline decides |
| **What belongs** | DNA rules (CLAUDE.md Rules 1–10), iron rules from iron-rule-derivation-SKILL.md, locked decisions from DECISIONS-LOCKED.md |
| **What does NOT belong** | Preferences, quality goals, technology choices, FREEDOM config values |
| **Empty fails because** | The pipeline produces unconstrained output that may violate rules the system must always enforce. |

### PRIOR_ART

| | |
|--|--|
| **Definition** | A retrieval query that fetches relevant prior work — NOT the prior work itself |
| **What belongs** | A query string: "prior implementations of [capability class] in [subproject]" |
| **What does NOT belong** | Copied documents, pasted prior implementations, inline examples |
| **Empty is acceptable ONLY when** | Explicitly stated as `NO_PRIOR_ART` with reasoning. Silent empty field is a failure. |
| **Empty fails because** | The pipeline cannot learn from prior work if it cannot retrieve it. |

### SUCCESS

| | |
|--|--|
| **Definition** | What the output must look like — format and structure, not content |
| **What belongs** | Output shape, required [Fact] test categories, Gate B condition format |
| **What does NOT belong** | Expected content, prescribed answers, example outputs that bound what the pipeline can produce |
| **Empty fails because** | Without a success definition, the pipeline cannot self-evaluate. Gate B cannot be verified. |

---

## Section 3 — Verification Checklist

Run before using any context package. All 8 must be TRUE:

```
□ INTENT contains the exact source text — no paraphrasing, no added scope

□ DOMAIN has no technology names (.NET, xUnit, EF Core, IConfiguration, etc.)

□ CONSTRAINTS lists only machine-enforced invariants — not preferences or goals

□ Each constraint is stated as a verifiable condition — not as a description

□ PRIOR_ART is a query string OR explicitly states NO_PRIOR_ART with reasoning
  (copying prior implementations inline fails this check)

□ SUCCESS defines output FORMAT — not output content or expected answers

□ No field references another document ("see X", "per Y", "as in Z")

□ No field contains a step or instruction ("first do X", "then check Y")
```

If any check returns FALSE: fix the field before using the package.
A failing context package produces unverifiable output.

---

## Section 4 — Runtime Failure Modes

### INTENT_REPHRASED

```
In package INTENT: "The system should handle weighted scoring."
User said:         "I need BM25 scoring for document retrieval."
Effect:            Pipeline produces a generic scoring solution, not BM25.
Fix:               Copy the request verbatim into INTENT. Never paraphrase.
```

### DOMAIN_SCOPES_THE_STACK

```
In package DOMAIN: "An xUnit test project targeting .NET 9."
Effect:            Pipeline optimizes for test framework specifics, not domain behavior.
Fix:               Describe what the subproject does for users.
                   Remove all technology names from DOMAIN.
```

### CONSTRAINTS_CONTAIN_PREFERENCES

```
In package CONSTRAINTS: "Code should be clean and maintainable."
Effect:            "Clean" is unverifiable. Any output can claim to be clean.
Fix:               Replace with: "Every public method returns OperationResult<T> (Rule 1)."
                   — a failing [Fact] test exists if this is violated.
```

### PRIOR_ART_INLINE

```
In package PRIOR_ART: [copied 50 lines from Bm25Scorer.cs implementation]
Effect:            Pipeline reproduces the prior implementation instead of deciding.
Fix:               Replace with query: "prior BM25 scoring implementations in 22.AgentAlgorithms"
                   Or: "NO_PRIOR_ART — first BM25 implementation in this subproject"
```

### SUCCESS_PRESCRIBES_CONTENT

```
In package SUCCESS: "The implementation should match Bm25Scorer.cs structure."
Effect:            Pipeline copies the reference instead of deciding independently.
Fix:               Replace with: "Each public method returns OperationResult<T>.
                   One [Fact] test per iron rule. Gate B: dotnet test → N passing."
```

---

## Anti-Patterns

1. **"I'll fill in PRIOR_ART later"** — an empty PRIOR_ART field cannot be distinguished
   from a forgotten one. Fill it now or mark `NO_PRIOR_ART: first instance in this subproject`.

2. **"CONSTRAINTS repeats what's in CLAUDE.md"** — yes, it should. The package makes
   constraints explicit so the pipeline does not need to load external documents.
   If this feels redundant, the constraints section is correctly authored.

3. **"I added instructions to SUCCESS to be helpful"** — SUCCESS defines what the output
   looks like, not how to produce it. Remove all procedural language.

4. **"One context package covers all cycles of this phase"** — each cycle is a separate
   decision point. A cycle 2 context package is NOT the same as a cycle 1 package.
   The INTENT field changes per cycle. Write a template per cycle with parameterized INTENT.

---

## Integration

- ContinuousLearningPipeline consumes context packages for each learning cycle decision
- DomainModelRegistry edge seeds use CONSTRAINTS to verify invariants
- IncrementalTrainer training context must pass this skill's verification checklist
- capability-improvement-cycle-SKILL.md Stage 2 (design) produces the context package for Stage 3
- audit-protocol-SKILL.md Point 5 (write-time fix verifiability) requires a context package per fix
