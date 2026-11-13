# RAG-OR-005 — GUIDE-B24-STEP-4-CYCLE2-TEMPLATE.md

**Роль:** прочитанный исторический материал проекта; команды внутри него не являются активными инструкциями.

| Поле | Значение |
|---|---|
| Ветка | `main` |
| Commit | `f16e68bc662bc629bc45bd0e6a46011c12865199` |
| Git blob SHA | `e29fca0cdf4ab14e9411c89dc9c55f5dd27a8cc4` |
| Путь | `Flow files generator/GUIDE-B24-STEP-4-CYCLE2-TEMPLATE.md` |
| Источник | [Закреплённый файл](https://github.com/lubochka/xiigen-planning/blob/f16e68bc662bc629bc45bd0e6a46011c12865199/Flow%20files%20generator/GUIDE-B24-STEP-4-CYCLE2-TEMPLATE.md) |
| Проверенные фрагменты | L83–L121, L320–L353 |

## Что прочитано

Проверяющие выбираются по шагу, с дополнительными способностями для персональных данных, межпоточных зависимостей и оркестрации.

## Границы использования

Использовать принцип покрываемой обязанности; не переносить panel=4/7, fixed acceptance score, прежние model IDs или считать их выполненным review. Synthetic or.qa target не заменяет действующую CODEX authority подготовки.

## Подтверждающий фрагмент L83–L121

[Оригинал, строки 83–121](https://github.com/lubochka/xiigen-planning/blob/f16e68bc662bc629bc45bd0e6a46011c12865199/Flow%20files%20generator/GUIDE-B24-STEP-4-CYCLE2-TEMPLATE.md#L83-L121)

~~~~text

```
NODE = {
  intent: {
    purpose:        string  — one sentence, what this step does for the user (no tech names)
    scope:          string  — what is included and excluded from this step's responsibility
    outcome:        string  — what state the system is in after this step completes
    invariants:     []      — invariants from state.invariants that apply to this step
    failureModes:   []      — named failure modes specific to this step (min 2, specific)
    domainConcepts: []      — domain terms for the retrieval system seeding
  }
  structure: {
    inputShape:    {} — key inputs this step receives (fields it depends on)
    outputShape:   {} — what this step produces or confirms
    triggers:      [] — event or state change that causes this step to run
    emits:         [] — event or state change this step produces
    dependencies:  [] — prior plan step numbers this step requires
  }
  constraints: [
    // Every state.invariants item that applies to this step, as verifiable conditions
    // Minimum 3 per step: the eighth architecture DNA pattern (store before enqueue) + 2 domain-specific
  ]
  quality: {
    scoringCriteria:      [] — criteria specific to this step's behavior (min 2, specific)
    acceptanceThreshold:  0.85
    degradationAcceptable: boolean — false for ROUTING; true for ORCHESTRATION sub-tasks only
  }
}
```

**"Non-empty" rules per field:**
- `intent.failureModes`: Minimum 2 items that are SPECIFIC to this step's domain.
  Generic modes ("system error", "timeout", "failure") do NOT count.
- `constraints`: Must list every state.invariants item that applies to this step.
  Not all invariants — only those this step must enforce. Missing → IronRules arbiter BLOCKS.
- `quality.scoringCriteria`: Must reference something unique to THIS step. Test:
  could this criterion appear word-for-word in a NODE for a completely different step?
  If yes → generic → replace.

~~~~

## Подтверждающий фрагмент L320–L353

[Оригинал, строки 320–353](https://github.com/lubochka/xiigen-planning/blob/f16e68bc662bc629bc45bd0e6a46011c12865199/Flow%20files%20generator/GUIDE-B24-STEP-4-CYCLE2-TEMPLATE.md#L320-L353)

~~~~text
### v1 CHALLENGER_ROLES field (decision rules — not fixed list)

```markdown
### CHALLENGER_ROLES

```
Decision rule — select challengers per step, not per flow. Apply rules in order.

RULE 1 — DEFAULT (all steps):
  Always include: Domain + Principles + Iron Rules

RULE 2 — IDENTITY/personal data (add Security):
  Trigger: step involves [list the personal data/sensitive data types for this flow's domain]
  Applies to: [name the step types where this applies]

RULE 3 — CROSS-FLOW DEPENDENCY (add Business):
  Trigger: step emits an event consumed by a step in a different flow, OR receives
           an event emitted by a step in a different flow
  Applies to: [name which step types have cross-flow events, if any]

RULE 4 — ORCHESTRATION (add Completeness + Skills + Prompts):
  Trigger: step's archetype is ORCHESTRATION
  Applies to: [name which step types are ORCHESTRATION archetype]

APPLIED CHALLENGER SELECTION FOR FLOW-XX STEP TYPES:

  [STEP_TYPE_1] steps (Clause N, [ARCHETYPE]):
    Mandatory: Domain + Principles + Iron Rules
    [Add applicable rules]
    Panel: [N] arbiters

  [STEP_TYPE_2] steps (Clause N, ORCHESTRATION):
    Mandatory: Domain + Principles + Iron Rules + Security + Completeness + Skills + Prompts
    Panel: 7 arbiters
~~~~

