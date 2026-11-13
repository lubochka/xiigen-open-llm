# Skill: Implementation Documentation — Code References with Explanations (§3)

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

Code reference sections must explain WHY and HOW, not just WHAT. A code block that only restates what the syntax says is useless documentation.

## Code Reference Evidence Classifier

Any code reference or quote in a plan, review, or implementation document must
have one current evidence class in the internal review packet. Luba-facing text
names the source in human language first; machine labels such as
`PATH_EXISTS_ONLY` and `LINE_REF_VERIFIED` may appear only after that meaning or
inside the internal packet.

- `PATH_EXISTS_ONLY`: the file path exists, but no line number or quote was verified in the current pass.
- `LINE_REF_VERIFIED`: the exact source line or range was opened and read in the current pass or in a cited owner document whose evidence boundary is preserved.
- `QUOTE_VERIFIED`: the exact quoted text was opened and read in the current pass or in a cited owner document whose evidence boundary is preserved.
- `NEEDS_CODE_REF_INVENTORY`: the plan needs exact anchors before implementation, but the current pass did not verify them.
- `HISTORICAL_UNVERIFIED`: old, benchmark, attachment, ledger, or owner-doc text that may contain useful anchors but is not current line/quote proof.

Do not invent `file:line` anchors, quotes, method signatures, or implementation status from plan prose. A source path check can prove only `PATH_EXISTS_ONLY`; it cannot prove `LINE_REF_VERIFIED`, `QUOTE_VERIFIED`, or that implementation exists.

Code references are refreshed against the current canonical repository tree.
Do not require a frozen copy in a `DPOLegalLearning` workdir as a universal
precondition. If a separate copied workdir is explicitly ordered for a task, its
inventory must say it is a copy and must not replace canonical-tree verification.

## Three layers every code block must have

### Layer 1 — WHY (mandatory, most important)
Explain why this condition, threshold, or decision exists:
- ✅ "> **Почему порог 0.75:** выбран так, чтобы Mode1 активировался только при достаточной уверенности. Ниже 0.75 — риск выдать конкретный план для неправильного FlowRef."
- ❌ `// confidence check`

### Layer 2 — HOW (mandatory when calling another component)
When a code block calls a function, explain what happens INSIDE that function:
- ✅ "> **Что происходит внутри `Classify` пошагово:** 1. Текст в lowercase 2. Regex ищет..."
- ❌ `// calls HumanIntentBoundaryClassifier`

### Layer 3 — WHAT (secondary, the code shows this)
The code itself shows WHAT. Comments inside code blocks should focus on values and outcomes:
- ✅ `// grounding.Confidence = 0.82, FlowRef = "FLOW-05" → уверены, идём Mode1`
- ❌ `// checks confidence`

## Format rules

1. Use `>` blockquote blocks for WHY/HOW explanations — they visually separate explanation from code
2. Inside codeblocks: show concrete example values in comments:
   ```csharp
   // candidate_order_report.OrderSensitive = true → позиция кандидата влияет на результат
   // SkillSelectionMachine must suppress candidate_position features before final selection
   var report = await CandidateOrderLeakageGuardUnit.ExecuteAsync(request);
   ```
3. When a condition blocks something, show ALL blocking conditions with reasons:
   ```csharp
   // Блокировка Mode1 когда:
   // 1. Kind = DocumentationMigration/PhaseContinuation → контекстно-зависимые
   // 2. FlowRef в ForbiddenScope → пользователь явно исключил ("3 not now")
   // 3. AllowedScope непустой И не содержит FlowRef → вне списка пользователя
   ```

For Human-Readable Architecture Artifact Gate audit reports, every problem quote
must use this adjacent structure:

```text
file + line + short quote:
сейчас:
почему плохо/ок:
должно стать:
какие файлы/classes/records/methods/data rows:
как обучается/работает:
какие sets/checkpoints/manifests:
какие tests/e2e/qa:
lifecycle:
```

The concrete solution must appear immediately under the quote. A later appendix
or table-only reference is not enough.

## What NOT to do

- ❌ Code block + comment that just renames the method call: `// классификатор читает промпт`
- ❌ `// returns result` — what result? show it
- ❌ Calling a function and explaining nothing about its internals if they are non-obvious
- ❌ Threshold/constant without explaining why that value was chosen
- ❌ "Иначе — Mode2" without explaining what Mode2 does and why it's the right fallback
- ❌ schema-only or prompt/spec text such as "create schema",
  "chosen/rejected/quarantine", "wrap in ILogicalUnit", "add KNN", or
  "make typed state" without concrete files/classes/records/methods/tests/data
  rows. It не является решением.

## Validation gate

For every code block in §3, verify:
- [ ] Has at least one WHY explanation (blockquote or comment)
- [ ] If calls another function → internals explained or linked to §2
- [ ] All thresholds/constants have "почему X" explanation
- [ ] All conditions show ALL branches with reasons
- [ ] Example values shown in comments (not just type names)
- [ ] Human-Readable Architecture Artifact Gate quote structure is present when
  the artifact is an architecture/trainable/code audit
- [ ] Concrete Repair Blueprint Gate is filled next to each suspicious quote

For learned dictionary/counter/map code, the validation gate also requires a
concrete "current -> target" explanation: replacement `ILogicalUnit`, typed
trained state, KNN/nearest role, DPO source and pair rule, storage route from
the current contract, backup/export/import/share, continue-training from
checkpoint when the contract supports it, affected sets, and tests. Do not force
user/cache fallback when the current contract does not say so. "Make trainable"
is not enough.

## Integration

Load alongside: `implementation-doc-uml-SKILL.md`, `implementation-doc-algorithms-SKILL.md`
Required by: `planning-session-startup-SKILL.md` (pre-execution doc gate), `plan-review-SKILL.md` (FC-16, FC-31), `human-readable-architecture-audit-SKILL.md`
