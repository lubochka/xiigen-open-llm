# Skill: Implementation Documentation — Algorithm Descriptions (§2)

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

## Purpose

Every component in an implementation document must have a human-language algorithm description that explains HOW and WHY, not just WHAT. A programmer must understand the component without reading the source code.

## Required structure for each component description

For architecture/trainable/code audit artifacts, this is a no-code algorithm
section. It explains what happens, where, and in what order, with class/function
refs, but it does not dump code. The default language is Russian unless Luba
asks otherwise.

### 1. Human purpose first; analogy only when it clarifies
Start with the product purpose and human meaning. Add an analogy only when it
helps a non-programmer understand the behavior; do not force a metaphor when a
plain description is clearer:
- ✅ "Компонент смотрит на просьбу программиста и объясняет её как задачу входа через OAuth: нужно выбрать план настройки входа и добавить проверку результата. Кодовая ссылка после смысла: `FLOW-05`."
- ❌ "Этот класс реализует поиск K ближайших соседей по мере Жаккара."

### 2. What happens during setup, storage, training, or initialization
Concrete description of what data the component stores and how it changes:
- Name the internal data structures (field names from the class)
- If the structure is a suspected shortcut, show it as a risk and repair path,
  not as the target algorithm. Example: "После 20 учебных пар видно, что модель
  слишком часто выбирает первый вариант не потому, что он лучше, а потому что он
  стоит первым. Целевое исправление: проверочный блок переставляет варианты
  местами и не принимает решение, если ответ меняется только из-за порядка.
  Технические ссылки после смысла: `_candidatePositionWeights`,
  `CandidateOrderLeakageGuardUnit`."

### 3. What happens during inference / request

For trainable-unit repair documents, the training/initialization section must
also explain every deterministic-to-trainable replacement in plain language:

- which concrete `ILogicalUnit` replaces each dictionary/counter/map;
- what typed trained state replaces the raw dictionary;
- how storage is routed by the current contract. If the current contract says
  user state loads before repo/common fallback, document that. If it does not,
  do not force a cache fallback into the algorithm;
- how backup/export/import/share works for user state;
- how continue-training updates the loaded checkpoint instead of retraining
  from zero;
- which KNN/nearest methods exist and whether they are model memory, support
  evidence, diagnostics, or forbidden final authority;
- which DPO pairs train the unit, where the source material comes from, and
  whether external common-training material has MIT or Apache-2.0 license.

Step-by-step numbered list of what the component does:
1. What comes in, with a real observed value or a clearly labelled test/
   illustrative value, not just a type name
2. What transformation happens
3. What decision is made and why (threshold, condition)
4. What comes out, with a real observed value or a clearly labelled test/
   illustrative value

### 4. Compact boundary: what this algorithm must not be confused with
When the component could be mistaken for training, semantic reasoning, learned
quality, or a different storage route, state the boundary compactly. This is a
short clarification, not a mandatory wall of negative claims:
- ❌ regex/template/dictionary lookup as semantic decision
- ❌ exact prompt replay
- ❌ if-else rules for semantic choices

### 5. Parameter signatures with labelled examples
After the text description, add a `**Сигнатуры и типы данных:**` block:
- Show the main method signature
- Show what the return type contains with real observed field values or clearly
  labelled test/illustrative values
- Show example of the most important input type

**Example of correct parameter block:**
```csharp
// FlowGroundingResult возвращается TryGround:
FlowGroundingResult(
    FlowRef: "FLOW-05",            // какой тип задачи найден
    Confidence: 0.82,              // уверенность 0..1
    EvidenceFeatures: ["oauth", "аутентификация"],  // что дало сигнал
    PredictedPhaseSequence: ["planning", "implementation", "gate-b"])
```

## Status annotation

Every component description must end with its implementation status:
- **✅ Полная** — all public methods implemented and tested
- **⚠️ Частичная** — state exactly which methods work and which are stubs
- **🔶 Скелет** — state what is saved (contract fields), what returns stub values, and WHY (for future training)

## Validation gate

Before submitting §2, verify each component description:
- [ ] Starts with human product purpose; analogy is present only if it improves
  clarity
- [ ] Shows concrete example values and labels them as real observed, test, or
  illustrative; invented values are not presented as measurements
- [ ] Has a compact "do not confuse this with..." boundary when training,
  quality, storage route, or semantic authority could be overclaimed
- [ ] Has parameter block with labelled example values
- [ ] States implementation status
- [ ] For Human-Readable Architecture Artifact Gate outputs, explains the
  algorithm without code dumps and names concrete class/function refs
- [ ] For trainable repairs, passes the Concrete Repair Blueprint Gate instead
  of using schema-only or prompt/spec text
- [ ] Code-fence/table syntax is not used as a readability escape: comments,
  diagram labels, table explanations, branch names, and expected outcomes remain
  human-readable; exact identifiers appear as secondary references after meaning
- [ ] For a post-challenge or large rewrite, the first compact live fragment
  passes the semantic probe from `output-readability-gate-SKILL.md`

## Integration

Load alongside: `implementation-doc-uml-SKILL.md`, `implementation-doc-code-refs-SKILL.md`
Required by: `planning-session-startup-SKILL.md` (pre-execution doc gate), `plan-review-SKILL.md` (FC-15, FC-31), `human-readable-architecture-audit-SKILL.md`
