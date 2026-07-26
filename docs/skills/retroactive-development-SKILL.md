# Retroactive Development

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

## Core Principle

**Fix the engine that produces wrong output, not the output itself.**

If SemanticCompilerPipeline generates a wrong predicate, fix the pipeline class.
If BruteForceIndex returns wrong nearest neighbours, fix the distance function.
Patching the output file and leaving the generator broken ships the same bug next time.

---

## Fix Propagation Protocol

### Step 1 — Identify the Source

Trace backward from the symptom to the generator:

```
Symptom: Generated predicate has wrong field name
  ↓ trace
Immediate cause: PredicateBuilder.BuildFromTokens() produces wrong field
  ↓ trace
Root cause: TokenClassifier misclassifies "fieldName" token as VALUE not IDENTIFIER
  ↓ source found
Fix location: Src/29.SemanticCompiler/TokenClassifier.cs
```

Apply root-cause-tracing-SKILL.md if the trace is non-obvious.

### Step 2 — Fix the Engine

Fix the source class. Do not patch downstream consumers or output files.

```csharp
// ✅ Fix in TokenClassifier — the generator
private TokenType ClassifyToken(string token) {
    // Check for known identifier pattern before checking value pattern
    if (IsIdentifierPattern(token)) return TokenType.IDENTIFIER;
    if (IsValuePattern(token)) return TokenType.VALUE;
    return TokenType.UNKNOWN;
}

// ❌ Patch in the output — treating the symptom
// Manually editing generated .cs file
// Adding a post-process cleanup step that replaces wrong field names
```

### Step 3 — Re-validate All Flows Using the Same Component

After fixing the engine, run all tests that exercise the fixed class:

```bash
# Find all tests that use the fixed component
grep -rn "TokenClassifier\|SemanticCompilerPipeline\|BuildFromTokens" Tests/ --include="*.cs" -l

# Run those test classes
dotnet test --filter "FullyQualifiedName~TokenClassifier OR FullyQualifiedName~SemanticCompiler"
→ Must show: Failed: 0
```

### Step 4 — Verify Fix Is Generative, Not Cosmetic

The fix must cause the engine to produce the right output for ALL inputs of the same type,
not just the one input that was reported as broken.

```
Cosmetic fix: handles "fieldName" specifically → other identifiers still broken
Generative fix: fixes the classification rule → all identifiers now classified correctly
```

**Test with at least 3 different inputs of the same type:**
```csharp
[Theory]
[InlineData("fieldName", TokenType.IDENTIFIER)]
[InlineData("recordId", TokenType.IDENTIFIER)]
[InlineData("entityType", TokenType.IDENTIFIER)]
public void TokenClassifier_IdentifierTokens_ClassifiedCorrectly(string token, TokenType expected)
{
    // Arrange: use real TokenClassifier, not mock
    var classifier = new TokenClassifier();

    // Act: classify the token
    var result = classifier.Classify(token);

    // Assert: correct type returned
    Assert.Equal(expected, result);
}
```

---

## Blast Radius After Engine Fix

An engine fix can silently break callers that relied on the wrong output.

After fixing the engine:
```
1. Find all callers of the fixed method:
   grep -rn "TokenClassifier\|ClassifyToken" Src/ --include="*.cs"

2. For each caller, verify its tests still pass:
   dotnet test --filter "FullyQualifiedName~[CallerClass]Tests"

3. Run full suite:
   dotnet test --no-build 2>&1 | tail -10
   → Failed: 0
```

---

## When Output Files Must Be Updated

Sometimes fixing the engine means previously-generated output files are now wrong.
This is correct — they should be regenerated.

```
Steps:
1. Fix the engine
2. Re-run the generation to produce new output
3. Verify the new output is correct (not just different)
4. Commit both the engine fix AND the regenerated output
5. Document in commit message: "fix TokenClassifier; regenerate Predicates/*.cs"
```

Do not leave stale generated output after an engine fix.

---

## Anti-Patterns

1. **Patch the output**: Editing a generated file instead of fixing the generator
2. **One-case fix**: Only handling the specific input that was reported, not the class of inputs
3. **Skip blast radius**: Fixing the engine without verifying callers still pass
4. **Silent output staleness**: Fixing the engine and not regenerating the output that depends on it
5. **Post-hoc test**: Writing the regression test after the fix passes (see bug-to-tests-SKILL.md)
