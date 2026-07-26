# Principles Arbiter

## Luba Why / Loophole Repair-First Rule

When Luba asks "why" (or the equivalent in the operator's language), challenges an answer, or says a failure came
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

AI systems forget principles that were stated earlier in their context window.
A phase that declares "Rule 1: OperationResult<T> everywhere" will sometimes
produce code that returns null or throws. The Business Logic reviewer will not
catch this — it is not its domain. The Security reviewer may miss it — it is
a consistency issue, not a vulnerability. Only a reviewer whose ENTIRE context
is the principles document, and whose ONLY task is checking compliance, reliably
catches violations. This skill defines the isolation rule, the verdict format,
and the self-reinforcement check for the principles arbiter role.

---

## When to Invoke

- During any multi-reviewer evaluation of generated code (arbiter-panel-design-SKILL.md)
- Before Gate B of any implementation phase (verify Rule 1-10 compliance)
- After implementing any new service class (DNA compliance check)
- When dna-compliance-guard-SKILL.md needs a structured verdict format

---

## Section 1 — The Isolation Rule

**The Principles Arbiter receives ONLY:**
1. Full text of CLAUDE.md Rules 1–10 with concrete violation examples
2. The BaseService pattern description
3. The OperationResult<T> namespace separation (3 separate namespaces)
4. The fabric interface registry (ISessionStorage, IAgentMessageBus, etc.)
5. The code being evaluated

**The Principles Arbiter does NOT receive:**
- Iron rules for this capability class (that is Business Logic role's domain)
- Prior implementations or RAG patterns (that is Skills role's domain)
- Security vulnerability patterns (that is Security role's domain)
- Capability class name, archetype, or subproject ID
- Any other reviewer's verdict
- Test file contents (that is Test Coverage role's domain)

**Why the isolation is mandatory:**

If the Principles Arbiter knows it is evaluating a `HybridRetriever`, it may
implicitly assume Rule 2 (Fabric First) is less relevant because hybrid retrieval
has no external dependencies to abstract. That assumption is wrong — IVectorIndex
and IBm25Scorer are both fabric interfaces. The isolation removes these cognitive
biases. The arbiter has one context, one task.

Any addition to the context package is a contamination that reduces reliability.

---

## Section 2 — Verdict Format

```csharp
public record PrinciplesVerdict
{
    public string VerdictClass { get; init; }     // "BLOCK" or "PASS" — nothing else
    public List<PrincipleViolation> Violations { get; init; }
    public List<string> Passed { get; init; }      // ALL rules checked, not just violations
}

public record PrincipleViolation
{
    public string Rule { get; init; }        // e.g. "Rule 1 — OperationResult<T> everywhere"
    public string Clause { get; init; }      // exact clause from CLAUDE.md
    public string Evidence { get; init; }    // line number and exact code that violates
    public string Severity { get; init; }   // always "BLOCK" for principles violations
}
```

**The Passed list MUST include every rule checked.** Not just violations. Not a count.
Every Rule 1-10, explicitly listed as PASS or VIOLATION.

```
Example Passed list: ["Rule 1: PASS", "Rule 2: PASS", "Rule 3: PASS", "Rule 4: PASS",
                      "Rule 5: N/A (read-only review)", "Rule 6: N/A (not a commit gate)",
                      "Rule 7: PASS", "Rule 8: PASS", "Rule 9: PASS", "Rule 10: PASS"]
```

A missing entry means "not checked" — which is the same problem as "forgotten."

**Verdict class is always BLOCK or PASS.** Never ADVISORY. Never CHALLENGE.
If a Rule 1-10 violation is present: BLOCK. If all pass: PASS. No middle ground.

---

## Section 3 — Self-Reinforcement Rule

The Principles Arbiter checks whether the current evaluation included a
Principles Arbiter role in the panel. This is how the rule enforces itself:

```
In the principles check, the arbiter evaluates:
  Rule:     "arbiter-panel-design-SKILL.md requires DNA Compliance role in all panels"
  Check:    Is this evaluation running as part of a multi-reviewer panel?
  Evidence: Panel config shows DNA Compliance role with isolated=true
  Result:   PASS if isolated=true; BLOCK if panel exists but role is absent or contaminated
```

If the panel has no DNA Compliance role: BLOCK.
If the role exists but received non-principles context: BLOCK.

---

## Section 4 — Growth Rule

When new rules are added to CLAUDE.md:

1. The next run of the Principles Arbiter includes them automatically — the context
   package is loaded fresh from the current CLAUDE.md at runtime
2. No fixture change required
3. The `Passed` list in the verdict includes the new rule on the next run
4. If the new rule catches violations that prior runs missed: this is correct behavior

This is the compound return: every new rule in CLAUDE.md makes the Principles Arbiter
more capable on the next run without any additional wiring.

**Do NOT:**
- Hardcode the rule list in the arbiter prompt (defeats the growth rule)
- Abbreviate the rules for "efficiency" (loses the detail needed to catch violations)
- Add domain context "to help it evaluate better" (breaks the isolation rule)

---

## Section 5 — Example Violations This Skill Catches

```
Rule 1 violation (OperationResult<T>):
  Code:     "throw new ArgumentException('Invalid weight');"
  Evidence: "Line 47: throw statement in public method WeightedScore.Calculate()"
  Verdict:  BLOCK

Rule 2 violation (Fabric First):
  Code:     "new InMemorySessionStorage()" in a service constructor
  Evidence: "Line 12: concrete InMemorySessionStorage injected, not ISessionStorage"
  Verdict:  BLOCK

Rule 10 violation (no external ML frameworks):
  Code:     "using Microsoft.ML.Transforms.Text;"
  Evidence: "Line 3: ML.NET import in BM25Scorer"
  Verdict:  BLOCK

BaseService pattern violation:
  Code:     public BM25Scorer(IBm25Index index, ISessionStorage storage)
  Evidence: "Constructor has two parameters; BaseService requires storage as first and only ctor parameter"
  Verdict:  BLOCK
```

---

## Anti-Patterns

1. **"Adding iron rules to the principles arbiter context 'for completeness'"** — iron rules
   are Business Logic role's domain. Adding them contaminates the isolation and may cause
   the arbiter to miss a principles violation while reviewing an iron rule instead.

2. **"Verdict class ADVISORY for Rule 3 (every line has a comment)"** — Rule 3 violations
   are BLOCK. All Rules 1-10 violations are BLOCK. No ADVISORY class for principles.

3. **"Passed list shows only 'all N rules passed'"** — the passed list must name every rule
   explicitly. "All rules passed" is the same as "not checked" from an audit perspective.

4. **"Principles arbiter receives the task type name 'to understand context'"** — the
   isolation rule exists because context causes cognitive bias. Remove the task type name.

5. **"Self-reinforcement check skipped because we know the panel is correct"** — the
   self-reinforcement check is how the panel requirement enforces itself. Run it every time.

---

## Integration

- arbiter-panel-design-SKILL.md Role 4 (DNA Compliance Arbiter) is the runtime instance of this skill
- dna-compliance-guard-SKILL.md runs a pre-commit version of this check (less structured)
- CLAUDE.md Rules 1-10 are the complete input to this arbiter (loaded at runtime)
- escalation-orchestrator-SKILL.md Rule 1 (ANY BLOCK → reject) applies to BLOCK verdicts from this arbiter
