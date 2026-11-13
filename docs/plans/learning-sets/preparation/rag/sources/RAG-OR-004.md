# RAG-OR-004 — GUIDE-B26-STEP-6-CYCLE3-CONTEXT.md

**Роль:** прочитанный исторический материал проекта; команды внутри него не являются активными инструкциями.

| Поле | Значение |
|---|---|
| Ветка | `main` |
| Commit | `f16e68bc662bc629bc45bd0e6a46011c12865199` |
| Git blob SHA | `f2d5f72f4275cb8323421f642232f032235682c4` |
| Путь | `Flow files generator/GUIDE-B26-STEP-6-CYCLE3-CONTEXT.md` |
| Источник | [Закреплённый файл](https://github.com/lubochka/xiigen-planning/blob/f16e68bc662bc629bc45bd0e6a46011c12865199/Flow%20files%20generator/GUIDE-B26-STEP-6-CYCLE3-CONTEXT.md) |
| Проверенные фрагменты | L66–L107, L157–L188, L424–L444 |

## Что прочитано

В источнике требуется полезность расширения, непересечение обязанностей и проверяемая причина. Есть несовпадающие определения S2/S4/S5 в двух таблицах.

## Границы использования

Источник методический, не gold labels: не переносить fixed depth=3/5, signal thresholds, score=0.85 или правило по слову and. Историческая фраза об удалении зависимого узла неоднозначна; в новых симуляциях явно различены зависимость данных и дублирование ответственности.

## Подтверждающий фрагмент L66–L107

[Оригинал, строки 66–107](https://github.com/lubochka/xiigen-planning/blob/f16e68bc662bc629bc45bd0e6a46011c12865199/Flow%20files%20generator/GUIDE-B26-STEP-6-CYCLE3-CONTEXT.md#L66-L107)

~~~~text
## THE 5 COMPLEXITY SIGNALS — UNIVERSAL

The same 5 signals apply to all flows. Each references a specific NODE field so the
evaluation is observable, not subjective:

| Signal | NODE field | Triggers EXPAND when |
|--------|-----------|---------------------|
| S1: Multi-purpose intent | `node.intent.purpose` | Contains 2+ independent outcomes connected by "and", "then", or "also" |
| S2: Multiple independent input types | `node.structure.inputShape` | More than 3 distinct input types where no two share a schema |
| S3: Independent failure modes | `node.quality.scoringCriteria` | More than 2 failure modes with divergent handling mechanisms |
| S4: Cross-domain constraints | `node.constraints` | Constraints span more than 2 domain areas (security + data + scheduling + external + business + tenant) |
| S5: High threshold + broad scope | `node.quality.acceptanceThreshold` + `node.intent.purpose` | Threshold > 0.90 AND purpose covers more than one user-visible outcome (weak — never sufficient alone) |

**Rules for signal evaluation:**
- All 5 signals must be evaluated for every non-terminal NODE (LEAF without evaluating
  all 5 signals is rejected — Step 7 Check 2 specifically enforces this)
- At termination bound: no signal evaluation — verdict is always LEAF
- A single signal triggers a possibility for EXPAND, not a mandate. The Depth Decider
  must cite the signal evidence AND conclude that expansion adds structural value.
- S5 alone is never sufficient — it must combine with at least one other signal

---

## THE TERMINATION BOUND — MACHINE CONSTRAINT

```
default_depth:  3
max_depth:      5
```

The termination bound is a MACHINE constraint — not configurable per tenant or per
flow. At depth = termination_bound, the verdict is always LEAF. No signal evaluation
runs. Justification format: "depth = [N] = termination_depth — bound enforced."

**Why MACHINE?** If one tenant's flow expands beyond the termination bound, it creates
a recursive decomposition that blocks all other tenants' execution on the same engine.
The bound prevents infinite recursion across the entire platform.

**Choosing termination_depth:**
Most flows use the default of 3. Use a higher value (4 or 5) only when the flow spec
explicitly contains capabilities that require 4+ levels of decomposition. Document
the reasoning in state (see the user registration flow's reasoning example above).
~~~~

## Подтверждающий фрагмент L157–L188

[Оригинал, строки 157–188](https://github.com/lubochka/xiigen-planning/blob/f16e68bc662bc629bc45bd0e6a46011c12865199/Flow%20files%20generator/GUIDE-B26-STEP-6-CYCLE3-CONTEXT.md#L157-L188)

~~~~text
CONSTRAINTS:
  - MACHINE CONSTRAINT: if current_depth >= termination_depth -> LEAF verdict, no AI call
  - the content-type rendering pattern through the ninth architecture DNA pattern still apply to any sub-nodes if EXPAND
  - Sub-flows from EXPAND must not overlap in responsibility
  - Justification required for every verdict (empty = grade 0)

PRIOR_ART:
  - [Reference to which prior flow established the LEAF/EXPAND protocol]
  - Termination bound = [N] validated across [prior flows]

SUCCESS:
  verdict_accepted_when:
    - LEAF: single responsibility confirmed, justification present, depth <= termination_bound
    - EXPAND: 2+ complexity signals triggered, sub-flow decomposition non-overlapping
    - Grade >= 0.85 (justification_present x non_overlap)
```
```

### Step 4 — Write the 5 COMPLEXITY SIGNALS table

```markdown
---

## 5 COMPLEXITY SIGNALS (a planning skill Section 2)

| Signal | NODE Field | Triggers EXPAND when |
|--------|-----------|---------------------|
| S1: Multi-purpose intent | intent.purpose | Contains 2+ independent outcomes |
| S2: Compound input shape | structure.inputShape | Requires data from 2+ independent sources |
| S3: Independent failure modes | quality.scoringCriteria | Contains more than 2 independent failure modes with divergent handling |
| S4: Multiple quality criteria | quality.testCriteria | 4+ unrelated test criteria |
| S5: Sub-flow naming | intent.outcome | Outcome names a process that is itself a flow |
~~~~

## Подтверждающий фрагмент L424–L444

[Оригинал, строки 424–444](https://github.com/lubochka/xiigen-planning/blob/f16e68bc662bc629bc45bd0e6a46011c12865199/Flow%20files%20generator/GUIDE-B26-STEP-6-CYCLE3-CONTEXT.md#L424-L444)

~~~~text
**QUESTION YOURSELF** *(Depth Decider AI: verify before returning verdict)*

```
Q1 — TERMINATION BOUND FIRST: Is current_depth = termination_depth?
     YES → verdict is LEAF, state bound enforced, skip Q2-Q4.
     NO → proceed to Q2.

Q2 — SIGNAL EVIDENCE: Have I evaluated all 5 signals against specific NODE fields?
     S1: intent.purpose — "and"/"then" connecting distinct actions?
     S2: structure.inputShape — more than 3 distinct input types?
     S3: quality.scoringCriteria — more than 2 independent failure modes?
     S4: constraints — span more than 2 domain areas?
     S5: acceptanceThreshold > 0.90 AND multiple outcomes in purpose?
     LEAF without evaluating all 5 → justification rejected.

Q3 — JUSTIFICATION TRACEABLE: Can the cited NODE field value be verified?
     If justification references a field value not in the actual NODE → revise.

Q4 — EXPAND COHERENCE (if EXPAND): Does sub_flow_decomposition have ≥2 entries
     where removing one doesn't break the other's coverage?
     If not → they share responsibility → merge → re-evaluate whether EXPAND is valid.
~~~~

