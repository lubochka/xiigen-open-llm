# Freedom vs Machine Classification

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

## When to Apply

Before writing any constant, threshold, string, or configuration value into code.
Before deciding whether a value should be in an environment variable, config file, or hardcoded.
When a DNA Rule 2 (Fabric First) check finds a hardcoded value.

---

## The Classification Test

One question:

> **If this value changed, would the system's behavioral guarantees break?**

```
YES → MACHINE. This value enforces a system invariant.
      Hardcode it as a named constant. Test for its presence.
      Never put it in a config file — changing it changes what the system guarantees.

NO  → FREEDOM. This value is a configurable parameter.
      Read it from environment variable or config. Provide a safe default.
      Different configurations may reasonably use different values.
```

---

## Canonical MACHINE Values in This Project

| Value | Why MACHINE |
|-------|-------------|
| OperationResult return type requirement | Violating Rule 1 breaks error propagation for all callers |
| Fabric interface resolution (Rule 2) | Bypassing the interface breaks testability and swappability |
| xUnit test must pass gate | The Rule 4 zero-failures invariant — cannot be relaxed |
| BruteForceIndex cosine distance formula | Mathematical correctness — changing it produces wrong results |
| HNSW M parameter default | Graph structure invariant — changing randomly corrupts the index |
| SemanticCompilerPipeline stage order | Pipeline stages must run in fixed order (parse before classify) |
| Rule 3 comment requirement | Every production line — not configurable per file or developer |

---

## Canonical FREEDOM Values in This Project

| Value | Why FREEDOM |
|-------|-------------|
| HNSW M and efConstruction tuning values | Performance tradeoff — different datasets need different values |
| BM25 k1 and b parameters | Corpus-specific tuning — different corpora need different weights |
| Vector dimensions (128, 256, 384, etc.) | Model-specific — depends on embedding model in use |
| Session storage file path | Deployment-specific — dev vs test vs production paths differ |
| Logging verbosity level | Operational policy — not a correctness invariant |
| Test timeout (milliseconds) | CI environment speed — not a correctness invariant |

---

## The Borderline Cases

### Thresholds That Enforce Invariants vs Configure Behavior

```
MACHINE: Minimum cosine similarity threshold for "same vector" deduplication
  → Lowering this causes different vectors to be treated as identical
  → System guarantee changes: deduplication correctness breaks
  → Hardcode: COSINE_DEDUP_THRESHOLD = 0.999

FREEDOM: BM25 retrieval score threshold for "relevant document"
  → Changing from 0.5 to 0.3 changes recall/precision tradeoff
  → The rule "filter low-scoring results" is MACHINE
  → The specific threshold is FREEDOM: BM25_RELEVANCE_MIN_SCORE
```

The rule is MACHINE. The parameters of the rule may be FREEDOM.

### Fabric Interface vs Concrete Type

```
MACHINE: The public interface (IVectorIndex, ISessionStorage, ICodeReader)
  → Cannot be changed without breaking all callers
  → Define in an interface, never in a concrete type

FREEDOM: The implementation chosen at runtime
  → BruteForceIndex or HnswIndex — both implement IVectorIndex
  → Select via DI configuration, not hardcoded instantiation
```

---

## The Annotation Format

In code, annotate the classification explicitly:

```csharp
// MACHINE: cosine dedup threshold — changes break deduplication correctness
private const double COSINE_DEDUP_THRESHOLD = 0.999;

// FREEDOM: BM25 relevance minimum — read from config, different corpora need different values
private readonly double _bm25MinScore;
public HybridRetriever(IConfiguration config)
{
    // Read freedom value from config with safe default
    _bm25MinScore = config.GetValue<double>("Retrieval:Bm25MinScore", defaultValue: 0.5);
}
```

---

## Hardcoded Value Detection

Run periodically to find FREEDOM values incorrectly hardcoded:

```bash
# Find magic numbers in production code (potential FREEDOM values that should be config)
grep -rn "= 0\.[0-9]\|= [0-9][0-9][0-9]" Src/ --include="*.cs" | grep -v "const\|//" | grep -v "Tests"

# Find model names or specific strings that should be configurable
grep -rn "\"claude-\|\"gpt-\|\"gemini-\|\"text-embedding" Src/ --include="*.cs" | grep -v "//"
# Expected: 0 hits in production code (model names are FREEDOM values)
```

Every hit is a potential MACHINE/FREEDOM classification error.

---

## Anti-Patterns

1. **"It's just a default, we can change it later"** — defaults become dependencies; if it's FREEDOM, add config key now
2. **"The threshold is internal, no caller will change it"** — that's the definition of MACHINE territory; test it explicitly
3. **Using the same literal value in 5 places without annotation** — 0.85 appears everywhere; is it the same invariant?
4. **Putting fabric interface methods in concrete types** — interface = MACHINE (stable); implementation = FREEDOM (swappable)
5. **Reading MACHINE constants from environment variables** — a constant that can be changed by config is not a constant

---

## Connection to DNA Rules

| Rule | MACHINE/FREEDOM Application |
|------|-----------------------------|
| Rule 1 — OperationResult | The return type pattern is MACHINE — never bypass with nullable or raw bool |
| Rule 2 — Fabric First | The interface is MACHINE; the concrete class is FREEDOM |
| Rule 10 — No ML frameworks | This prohibition is MACHINE — no configuration can override it |
| Algorithm parameters (HNSW M, BM25 k1) | FREEDOM — read from config, provide safe defaults |
