# Skill: Implementation Documentation — Algorithm Descriptions (§2)

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

## Purpose

Every component in an implementation document must have a human-language algorithm description that explains HOW and WHY, not just WHAT. A programmer must understand the component without reading the source code.

## Required structure for each component description

For architecture/trainable/code audit artifacts, this is a no-code algorithm
section. It explains what happens, where, and in what order, with class/function
refs, but it does not dump code. The default language is the operator's language
unless Luba asks otherwise.

### 1. Human analogy first (mandatory)
Start with an analogy that a non-programmer can understand:
- ✅ "Picture a librarian who over years of work has memorized: when a request comes in with the words 'authentication, OAuth' — it is almost always a FLOW-05 task."
- ❌ "This class implements a K-nearest-neighbor search by the Jaccard measure."

### 2. What happens during training / initialization
Concrete description of what data the component stores and how it changes:
- Name the internal data structures (field names from the class)
- If the structure is a suspected shortcut, show it as a risk and repair path,
  not as the target algorithm. Example: "After 20 training pairs
  `_candidatePositionWeights = {0: 12, 1: 3, 2: 5}` means a risk of candidate
  order leakage; the target repair — `CandidateOrderLeakageGuardUnit` checks
  permutations and blocks positional features."

### 3. What happens during inference / request

For trainable-unit repair documents, the training/initialization section must
also explain every deterministic-to-trainable replacement in plain language:

- which concrete `ILogicalUnit` replaces each dictionary/counter/map;
- what typed trained state replaces the raw dictionary;
- what is loaded first from user/cache and second from repo/common fallback;
- how backup/export/import/share works for user state;
- how continue-training updates the loaded checkpoint instead of retraining
  from zero;
- which KNN/nearest methods exist and whether they are model memory, support
  evidence, diagnostics, or forbidden final authority;
- which DPO pairs train the unit, where the source material comes from, and
  whether external common-training material has MIT or Apache-2.0 license.

Step-by-step numbered list of what the component does:
1. What comes in (with a real example value, not just type name)
2. What transformation happens
3. What decision is made and why (threshold, condition)
4. What comes out (with real example value)

### 4. "What the model does NOT do" section (mandatory)
Explicitly state what the component does NOT use:
- ❌ regex/template/dictionary lookup as semantic decision
- ❌ exact prompt replay
- ❌ if-else rules for semantic choices

### 5. Parameter signatures with examples
After the text description, add a `**Signatures and data types:**` block:
- Show the main method signature
- Show what the return type contains with real field values
- Show example of the most important input type

**Example of correct parameter block:**
```csharp
// FlowGroundingResult is returned by TryGround:
FlowGroundingResult(
    FlowRef: "FLOW-05",            // which task type was found
    Confidence: 0.82,              // confidence 0..1
    EvidenceFeatures: ["oauth", "authentication"],  // what gave the signal
    PredictedPhaseSequence: ["planning", "implementation", "gate-b"])
```

## Status annotation

Every component description must end with its implementation status:
- **✅ Full** — all public methods implemented and tested
- **⚠️ Partial** — state exactly which methods work and which are stubs
- **🔶 Skeleton** — state what is saved (contract fields), what returns stub values, and WHY (for future training)

## Validation gate

Before submitting §2, verify each component description:
- [ ] Has a human analogy (not a technical summary)
- [ ] Shows concrete example values (not just type names)
- [ ] Has "What it does NOT do" section
- [ ] Has parameter block with real example values
- [ ] States implementation status
- [ ] For Human-Readable Architecture Artifact Gate outputs, explains the
  algorithm without code dumps and names concrete class/function refs
- [ ] For trainable repairs, passes the Concrete Repair Blueprint Gate instead
  of using schema-only or prompt/spec text

## Integration

Load alongside: `implementation-doc-uml-SKILL.md`, `implementation-doc-code-refs-SKILL.md`
Required by: `planning-session-startup-SKILL.md` (pre-execution doc gate), `plan-review-SKILL.md` (FC-15, FC-31), `human-readable-architecture-audit-SKILL.md`
