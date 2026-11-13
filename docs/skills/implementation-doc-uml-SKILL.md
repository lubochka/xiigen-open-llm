# Skill: Implementation Documentation — UML Diagrams (§0 + §1)

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

Every implementation documentation section must begin with two UML subsections:
- **§0 Class diagram** — technical reference for programmers (draw.io importable)
- **§1 Human-language flowcharts** — explain what happens in plain language, not method names

## §0 Class Diagram — required elements

1. **Mermaid `classDiagram` block** importable via draw.io Extras → Edit Diagram
   - For Human-Readable Architecture Artifact Gate audit reports, PlantUML is
     also acceptable when it is importable to draw.io and carries the same
     class responsibilities, relationship legend, and simulations.
2. Every class must have a `note for ClassName "..."` annotation in Russian explaining in 1-3 lines:
   - What this class does (not its method names)
   - Its status: ПОЛНОСТЬЮ РЕАЛИЗОВАНА / ЧАСТИЧНО / ЗАГЛУШКА / СКЕЛЕТ
3. All factual relationships in the current code/design must be present, using
   the correct arrow type for that relationship: inheritance (`--|>`),
   implementation (`<|..`), composition (`*--`), association (`-->`),
   dependency (`..>`). Do not add arrow types that do not exist in this
   code/design just to satisfy a checklist.
4. **Legend table** immediately after the mermaid block with columns: Класс | Что делает | Статус

**What the note for must say (illustrative form, not a current status claim about this real class):**
```
note for OrchestrationPlanMachine "ПОЛНОСТЬЮ РЕАЛИЗОВАНА\nПринимает промпт →\nвыбирает 1 из 3 режимов →\nвозвращает текстовый план оркестрации"
```

5. **Per-connection simulation table (REQUIRED).** For EVERY relationship arrow (connection) in the diagram, give at least one example simulation: a concrete sample input that flows across that connection AND the expected result/output on that path. Use this table immediately after the legend:

| Соединение (A → B) | Пример входа (симуляция) | Ожидаемый результат по этой ветке |
|---|---|---|
| OrchestrationPlanMachine → SkillSelectionMachine | Иллюстрация: программист просит «добавь вход через OAuth и проверь, что он работает» | Машина распознаёт задачу настройки входа, выбирает поток авторизации и добавляет проверку результата; кодовые ссылки (`FLOW-05`, `SkillSelectionMachine`) идут после смысла |

A connection without an example simulation and expected result is an incomplete §0.

**What NOT to do:**
- Class diagram without `note for` annotations (programmer sees method names, not meaning)
- `note for` that just repeats the class name or lists methods
- Omitting the legend table
- Connection/arrow without an example simulation and expected result (the reviewer cannot tell what data crosses it or what each branch produces)
- PlantUML/draw.io diagram that contains only boxes and arrows but no concrete
  data-flow example or expected branch outcome

## §1 Human-language flowcharts — required elements

1. Use `flowchart TD` (not `sequenceDiagram` for the primary human diagram)
2. Every decision node `{...}` must state the human question, not a method name:
   - ✅ `{По просьбе видно, что это вход через OAuth?\nИ этот поток разрешён?}`
   - ❌ `{grounding.Confidence >= 0.75}`
3. Every action node must explain the business meaning:
   - ✅ `["🎯 Берём проверенный план для входа через OAuth\nКодовая ссылка: FLOW-05"]`
   - ❌ `["RenderLearnedFlowPlan(pair, grounding, contract, prompt)"]`
4. Label arrows with the condition in human language:
   - ✅ `-->|"ДА\nЭто похоже на задачу входа через OAuth"|`
   - ❌ `-->|true|`
5. Include emoji status indicators: ✅⚠️🔶 on skeleton/partial components
6. **Per-branch simulation (REQUIRED).** For EVERY decision branch (each outcome of each `{...}` node), give a concrete simulation input + the expected result for that branch. Example: `Вход: «добавь вход через OAuth и проверь результат» → ветка ДА → результат: план настройки входа с проверкой`; `Вход: «улучши безопасность» без деталей → ветка НЕТ → результат: уточняем задачу или запускаем дополнительный разбор`. Every branch of every decision node must show one simulated input and its expected output.

All Mermaid/PlantUML notes, labels, legends, branch names, and expected-result
tables are human explanations even when they live inside a code fence. Exact
class names and flow ids may stay as secondary code references after the
operator-language meaning. Use the central two-layer contract and semantic probe
in `output-readability-gate-SKILL.md`.

**Что НЕ делать (ветки):** ветка решения без примера-симуляции входа и ожидаемого результата — нарушение OP-54 / FC-14 / FAIL-33.

**One flowchart per major decision path.** The human gate (input validation / intent classification) must have its own flowchart showing all branches.

## Validation gate

Before submitting a UML section, verify:
- [ ] Class diagram has `note for` on EVERY class
- [ ] Legend table present after class diagram
- [ ] Flowcharts use human-language labels (no raw method names as primary labels)
- [ ] Decision nodes ask human questions
- [ ] Mermaid/PlantUML syntax was checked by an actual parser/render/open step
  when claiming "opens in draw.io"; reading the text is only a syntax precheck
  and must be reported as not-render-verified
- [ ] PlantUML audit diagrams, when used, are importable to draw.io and have the same simulation coverage
- [ ] Every connection (relationship arrow) in §0 has ≥1 example simulation + expected result (per-connection table present)
- [ ] Every flowchart branch in §1 has a simulation input + expected result

## Integration

Load alongside: `implementation-doc-algorithms-SKILL.md`, `implementation-doc-code-refs-SKILL.md`
Required by: `planning-session-startup-SKILL.md` (pre-execution doc gate), `plan-review-SKILL.md` (FC-14, incl. per-connection simulation; FC-31 Human-Readable Architecture Artifact Gate), `AGENTS.md` (OP-54 and OP-60), `architect-behavior-classifier-SKILL.md` (FAIL-33), `CLAUDE.md` ("Part A Completeness Contract pattern" §0)
