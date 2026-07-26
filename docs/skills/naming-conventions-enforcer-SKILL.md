# Naming Conventions Enforcer

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

## Why This Skill Exists

`Src/15.Analysis/Processor.cs` tells a reader nothing.
`Src/15.SyntaxAnalysis/SemanticTokenProcessor.cs` tells them exactly what
capability lives in that file.

`"testId": "T47"` in a log tells a reader nothing.
`"testName": "BM25ScorerScoresQueryAgainstDocument (T47)"` tells them what the test does.

This skill enforces naming standards across:
1. C# file and class names
2. Interface and implementation names
3. Test class and method names
4. STATE-Pn.json and REFERENCE-PLAN entries
5. Commit messages

---

## Rule 1 — File Names Must Describe Capability, Not Sequence Number

**NEVER:**
```
Src/12.Learning/Processor1.cs
Src/15.Analysis/Helper.cs
Src/22.AgentAlgorithms/Module.cs
```

**ALWAYS:**
```
Src/12.ContinuousLearning/IncrementalTrainer.cs
Src/15.SyntaxAnalysis/SemanticTokenProcessor.cs
Src/22.AgentAlgorithms/AgentOrchestrator.cs
```

**Rule:** The subproject number belongs in the folder name and `.csproj` name.
The file name must describe the DOMAIN CAPABILITY, never echo the number.

---

## Rule 2 — Interface Names Follow I-Prefix Convention

**NEVER:**
```csharp
interface Scorer           // missing I-prefix
interface BM25             // abbreviation without expansion
interface VecSearch        // truncated name
```

**ALWAYS:**
```csharp
interface IBm25Scorer          // I + Capitalized domain name
interface IVectorIndex         // I + full noun phrase
interface ISessionStorage      // I + clear responsibility
interface IAgentMessageBus     // I + clear responsibility
```

**Rule:** Interfaces start with `I`, followed by a capitalized noun phrase that
names the RESPONSIBILITY, not the implementing technology. Exceptions per Rule 9
(No abbreviations without expansion): first use of BM25 in comments must include
"BM25 (Best Matching 25)" expansion.

---

## Rule 3 — Implementation Names Must Match Their Interface

For an interface `IThingDoer`, the default implementation is `ThingDoer` or
`[Qualifier]ThingDoer` — never a completely different name.

```csharp
// ✅
interface IVectorIndex     → BruteForceIndex, HnswIndex
interface IBm25Scorer      → BM25Scorer
interface ISessionStorage  → SessionStateStore

// ❌
interface IVectorIndex     → SearchProvider   // no relation to interface name
interface IBm25Scorer      → ScoringEngine    // unrecognizable as IBm25Scorer impl
```

---

## Rule 4 — Test Class Names Include the Class Being Tested

**NEVER:**
```csharp
class TestSuite1
class VerificationTests
class SearchTests
```

**ALWAYS:**
```csharp
class BM25ScorerTests               // one class under test
class HybridRetrieverIntegrationTests  // integration tests for HybridRetriever
class IncrementalTrainerTests       // one class under test
```

**Rule:** Test class name = `[TestedClass]Tests` or `[TestedClass]IntegrationTests`.
Test method names: `[Method]_[Scenario]_[ExpectedResult]`.

---

## Rule 5 — STATE-Pn.json Must Include Human-Readable Phase Name

```json
// ❌ machine-only
{
  "phase": "P5",
  "status": "DONE"
}

// ✅ machine + human readable
{
  "phase": "P5",
  "phaseName": "IncrementalTrainer — LoRA fine-tuning via LoraApplicator",
  "status": "DONE"
}
```

Every STATE-Pn.json must include `phaseName` alongside the phase number.

---

## Rule 6 — Commit Messages Must Name the Phase and Files

**NEVER:**
```
fix: update stuff
phase 6 done
tests added
```

**ALWAYS (per phase-git-report-SKILL.md):**
```
phase 6: capability — Src/12.Learning/IncrementalTrainer.cs, Tests/IncrementalTrainerTests.cs
```

Format: `phase N: short capability description — comma-separated file list`

---

## Enforcement Gates

### At code-writing time:
```bash
# Verify interface names follow I-prefix
grep -rn "^public interface [^I]" Src/ --include="*.cs"
# Expected: 0 matches

# Verify test classes name the class under test
grep -rn "^public class [A-Z].*Tests$" Tests/ --include="*.cs" | head -20
# Expected: each match follows [ClassName]Tests pattern
```

### At commit time (dna-compliance-guard-SKILL.md Rule 6):
```bash
# Verify file names are descriptive
grep -l "Helper\|Processor\|Module\|Manager\|Service" Src/ -r --include="*.cs" | head -10
# Review: are these names specific enough? "SemanticProcessor" is fine; "Processor" is not.
```

---

## Naming Lint Quick Reference

| Artifact type | Pattern | Example |
|---|---|---|
| Interface | `I[CapabilityNoun]` | `IVectorIndex`, `IBm25Scorer` |
| Implementation | `[Qualifier][NounFromInterface]` | `BruteForceIndex`, `BM25Scorer` |
| Test class | `[ClassName]Tests` or `[ClassName]IntegrationTests` | `HybridRetrieverTests` |
| Test method | `[Method]_[Scenario]_[Expected]` | `Score_EmptyDocument_ReturnsFail` |
| Source file | `[CapabilityNoun].cs` | `SemanticTokenProcessor.cs` |
| State file | `STATE-P[N].json` | `STATE-P6.json` |
| Phase in STATE | `phaseName` with full description | see Rule 5 |
| Commit | `phase N: description — files` | see Rule 6 |
