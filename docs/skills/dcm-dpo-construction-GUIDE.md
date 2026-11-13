# DCM (DesignConversionMachine) - гайд по построению DPO-пар

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

**Версия:** 1.1
**Применимо к:** DCM server-side learning pipeline for "Figma/Canva design -> HTML+CSS"
**Аудитория:** архитектор, исполнитель, ревьюер DPO-данных
**Язык:** русский (первичный); поля кода на английском
**Статус:** plan-only; no implementation

---

## 0. Как используется этот skill

Используй этот гайд, когда нужно спроектировать, проверить или отбраковать DCM DPO-пары для `CriticUnit` и `ImproverUnit`.
Он задаёт human-readable contract для reviewer/архитектора: какие поля обязательны, кто имеет authority на `chosen/rejected`,
какие пары уходят в quarantine, и какие доказательства нужны до передачи данных в обучение.

Гайд не разрешает implementation сам по себе. Любая DPO-запись, batch или claim о качестве модели должны быть проверены
против этого документа, measurement-гайда и training-гайда. Если нет source/license gate, teacher evidence,
Render-QA/eval evidence или 50+50 train/held-out разделения, запись не может попасть в `Train*`.

---

## 1. Scope And Evidence Boundary

Этот guide задаёт DCM-специфичный контракт построения DPO-пар для двух обучаемых ролей модели: `CriticUnit` и `ImproverUnit`. DPO-пары учат модель предпочитать лучший результат преобразования дизайна в HTML+CSS или лучший verdict о качестве такого результата.

Новая идентичность данных и артефактов использует `dcm-` prefix: `dcm-critic-*`, `dcm-improver-*`, `dcm-source-*`, `dcm-batch-*`. Translation and old `design-conversion-*` artifacts are evidence-only, not foundation. Их можно читать как аналогии формата, но нельзя использовать как источник архитектуры, authority, tier semantics или правил принятия DPO для DCM.

Class names `DesignConversionPersonal` and `DesignConversionGlobal` остаются точными типами архитектуры DCM. Это не разрешает возвращаться к старым `design-conversion-*` artifact identities.

---

## 2. Architecture Contract

Exact architecture: `ILearningMachinePair<DesignConversionPersonal, DesignConversionGlobal>`.

`ILearningMachinePair<DesignConversionPersonal, DesignConversionGlobal>` содержит две ноги:

| Leg | Назначение | Ownership |
|---|---|---|
| `Personal leg` / `DesignConversionPersonal` | Адаптация под пользователя, его проекты, приватные дизайн-паттерны и стиль правок | Пользовательская область; не становится общим знанием без явного разрешения |
| `Global leg` / `DesignConversionGlobal` | Общая модель качества DCM, учится на разрешённых общих данных и анонимизированных сигналах | Серверная общая область |

Каждая leg обязана проходить одинаковый runtime chain:

```text
ILearningMachinePair<DesignConversionPersonal, DesignConversionGlobal>
  -> leg.GetOrchestratorAsync()
  -> ILogicalOrchestrator
  -> ILogicalFlow
  -> CriticUnit / ImproverUnit
```

Обязательная сокращённая форма для review packets: `GetOrchestratorAsync -> ILogicalOrchestrator -> ILogicalFlow -> CriticUnit/ImproverUnit`.

Юниты находятся внутри Flow; они не являются самостоятельными owners обучения или DPO-authority.

| Unit | Contract | Input | Output |
|---|---|---|---|
| `CriticUnit` | `(tree, html, renderImages, diff) -> QualityVerdict(score+defects)` | Нормализованное дерево дизайна, текущий HTML+CSS, render images на согласованных разрешениях, visual diff | `QualityVerdict`: score + defects with type/location/evidence |
| `ImproverUnit` | `(tree, html, defects) -> ImprovedHtmlCss` | Нормализованное дерево дизайна, текущий HTML+CSS, defects из `QualityVerdict` | `ImprovedHtmlCss`: полный HTML+CSS result, не patch |

`Optional SourceEncoder` allowed, but not required now. Если он появится, его роль ограничена кодированием `source_design_snapshot(figma/canva)` для Flow; он не меняет authority для `chosen/rejected`, не создаёт DPO пары самостоятельно и не заменяет `CriticUnit` или `ImproverUnit`.

Допустимые trainable уровни для описания DCM: `ILearningMachinePair`, `ILearningMachine`, `ILogicalOrchestrator`, `ILogicalFlow`, `ILogicalUnit`.
Нельзя вводить обучение, ownership или promotion status на произвольном helper, deterministic lookup, regex/BM25 layer, static validator или ad-hoc service.
Детерминированный shortcut может быть bootstrap/scaffold evidence, но не может маскироваться под обученный trainable unit. Вызовы `Train*`,
checkpoint lifecycle и promotion остаются Machine-owned.

---

## 3. Model Role

Модель имеет обе роли: `CriticUnit` и `ImproverUnit`.

`CriticUnit` оценивает результат: сравнивает source design, HTML+CSS, render images и diff, затем возвращает `QualityVerdict(score+defects)`.

`ImproverUnit` исправляет результат: получает source design, текущий HTML+CSS и defects, затем возвращает `ImprovedHtmlCss`.

Flow owns ordering. `ILogicalFlow` решает порядок вызовов, число итераций и остановку improvement loop. Юниты выполняют свои контракты, но не выбирают sequencing, не меняют tier policy и не присваивают acceptance status DPO-записи.

---

## 4. DPO Vector For DCM

DPO vector = internal unit state + source figma/canva + result.

DCM DPO-запись должна быть DCM-native, а не переводом `TranslationTrainReadyPair`. Минимальный DPO vector:

| Field | Meaning |
|---|---|
| `id` | `dcm-{unit}-{pair_family}-{run_id}` |
| `unit` | `CriticUnit` или `ImproverUnit` |
| `unit_internal_state_snapshot` | Снимок внутреннего состояния конкретного unit/leg перед candidate result; без приватных leakage fields |
| `source_design_snapshot(figma/canva)` | Нормализованный source design snapshot: tree, tokens, layout constraints, разрешённые image/diff references |
| `candidate_result` | Проверяемый unit output: `QualityVerdict` для Critic или `ImprovedHtmlCss` для Improver |
| `teacher_labels` | Render-QA labels + external AI labels where used: defect evidence, preference reason, quality class |
| `chosen` | Candidate/result, который teacher labels предпочли для обучения |
| `rejected` | Высококачественный rejected candidate/result, уступающий chosen по teacher labels |
| `teaching_point` | Одно конкретное предложение, почему chosen предпочтительнее rejected |
| `pair_family` | Класс различия: `spacing`, `color`, `typography`, `overflow`, `missing_element`, `z_order`, `responsive`, etc. |
| `curriculum_tier` | Учебная сложность дефекта 1-5; не зависит от user subscription tier |
| `validation_outcome` | `accepted`, `quarantine`, `rejected` |

Example shape:

```json
{
  "id": "dcm-critic-spacing-run0042",
  "unit": "CriticUnit",
  "unit_internal_state_snapshot": "{...}",
  "source_design_snapshot(figma/canva)": "{tree,tokens,breakpoints,diff_refs}",
  "candidate_result": "{\"score\":0.82,\"defects\":[...]}",
  "teacher_labels": {
    "render_qa": {"chosen": "candidate_a", "rejected": "candidate_b"},
    "external_ai": {"chosen": "candidate_a", "rejected": "candidate_b"},
    "reason": "candidate_a names the real spacing defect and candidate_b misses it"
  },
  "chosen": "{...candidate_a...}",
  "rejected": "{...candidate_b...}",
  "teaching_point": "Rejected misses the actual spacing defect while chosen localizes it with render evidence.",
  "pair_family": "spacing",
  "curriculum_tier": 2,
  "validation_outcome": "accepted"
}
```

---

## 5. Chosen/Rejected Authority Path

Server-side ILearningMachine задаёт `chosen/rejected` for DPO. More explicitly: server-side ILearningMachine pipeline owns chosen/rejected construction.

Authority path:

```text
source design + generated candidates
  -> render-QA
  -> optional external AI teacher
  -> teacher labels chosen/rejected
  -> local DB stores labelled evidence
  -> local DB builds per-unit DPO for CriticUnit/ImproverUnit
  -> Train* on ILearningMachine only
```

Teacher = external AI + render-QA labels chosen/rejected and fills DB. Внешний AI является teacher/labeler, а не runtime foundation и не продуктивным элементом пользовательского conversion loop. Render-QA даёт visual evidence; external AI teacher добавляет structured labels для сложных случаев, ambiguity и near-miss пар.

Local DB builds per-unit DPO:

| Unit | Local DB builds |
|---|---|
| `CriticUnit` | пары `QualityVerdict` vs `QualityVerdict`, где качество означает правильность labels/defects/score относительно render-QA + teacher evidence |
| `ImproverUnit` | пары `ImprovedHtmlCss` vs `ImprovedHtmlCss`, где качество означает улучшение render result относительно source design и defects |

`Train*` only on Machine. Нельзя вводить `TrainCriticUnit`, `TrainImproverUnit` или local training на unit как owner. Допустимые формы обучения вызываются только на `ILearningMachine` / `ILearningMachinePair` через серверный pipeline.

---

## 6. Free/Paid/Pro Tier Policy

`free/paid/pro` = budget layer around improvement loop only; it must not change core DPO acceptance, chosen/rejected semantics, model quality thresholds, or `CurriculumTier`.

Subscription tier может ограничивать:

| Tier | Budget / iteration policy |
|---|---|
| `free` | Меньше improvement iterations, меньше или ноль paid external AI teacher calls, больше reliance on deterministic render-QA labels |
| `paid` | Больше improvement iterations, ограниченный external AI teacher budget |
| `pro` | Максимальный improvement-loop budget и расширенная teacher review quota |

Subscription tier не может менять:

- acceptance rule для DPO-записи;
- значение `chosen` или `rejected`;
- core model quality threshold;
- `CurriculumTier`;
- смысл `QualityVerdict.score`;
- право попадания пары в train/held-out.

Если один и тот же source/candidate получает разные subscription budgets, DPO semantics остаются одними и теми же. Отличается только количество попыток улучшения и стоимость teacher labeling.

---

## 7. Negative-Pair Quality Rules

Negative pairs must be high-quality. `rejected` не должен быть мусорным, тривиально сломанным или нерелевантным кандидатом. Лучшие DPO-сигналы приходят от high-quality near-miss pairs, где rejected выглядит правдоподобно, но teacher evidence показывает конкретную уступку chosen.

Batch `50+50`: каждый учебный пакет должен иметь 50 train pairs + 50 held-out pairs, disjoint by source design.

Общие правила:

- `chosen` и `rejected` строятся только из candidates/results с достаточной evidence.
- High-quality near-miss rejected valid even when оба результата находятся выше product threshold, если teacher labels показывают устойчивое предпочтение.
- Нельзя принимать пару, где chosen/rejected различаются только order/index/name leakage.
- Нельзя принимать пару, где external teacher и render-QA не дают объяснимого preference reason.
- Для `ImproverUnit` chosen должен лучше устранять defects или лучше сохранять source design constraints.
- Для `CriticUnit` качество - это correctness labels/defects/score, not necessarily higher self-reported score.

CriticUnit clarification:

```text
CriticUnit quality is label/defect correctness, not necessarily higher self-reported score.
```

Пример валидной near-miss CriticUnit пары:

| Candidate | Self-reported score | Defects | Teacher judgement |
|---|---:|---|---|
| chosen | 0.72 | correctly names real color defect with location | Accepted because labels match render diff |
| rejected | 0.91 | misses real color defect and reports clean layout | Rejected despite higher self-reported score |

Пороговые строки для product quality не запрещают такую пару: score внутри `QualityVerdict` является частью candidate output, а DPO preference задаётся server-side teacher evidence.

---

## 8. Taxonomy Of DCM Pair Families

| Pair family | Rejected near-miss example | Why chosen is better |
|---|---|---|
| `spacing` | Button shifted by 8px or card padding 12px instead of 16px | Chosen localizes the spacing defect and preserves expected layout tokens |
| `color` | `#0056D2` instead of `#0052CC` with plausible component structure | Chosen detects or fixes the exact token mismatch |
| `typography` | Correct text but wrong font weight/line-height | Chosen follows typography tokens and visual render evidence |
| `overflow` | Works at desktop but clips at 360px | Chosen handles all required breakpoints |
| `missing_element` | Omits icon/footer while rest of layout is strong | Chosen keeps all required design tree elements |
| `z_order` | Dropdown mostly correct but hidden behind content | Chosen restores layer hierarchy |
| `responsive` | Correct at 1280px but not at 360px/768px | Chosen preserves source constraints across breakpoints |

---

## 9. Concrete DPO Examples

### 9.1 CriticUnit Pair

Scenario: source card has `padding:16px`; rendered HTML has `padding:12px`.

```json
{
  "id": "dcm-critic-spacing-run0001",
  "unit": "CriticUnit",
  "unit_internal_state_snapshot": "{critic_leg:'global', flow_step:'critic_pass_1'}",
  "source_design_snapshot(figma/canva)": "{card:{padding:'16px', width:'320px'}, breakpoints:[360,768,1280,1920]}",
  "candidate_result": "{\"score\":0.91,\"defects\":[{\"type\":\"spacing\",\"element\":\"card\",\"expected\":\"padding:16px\",\"actual\":\"padding:12px\"}]}",
  "teacher_labels": {
    "render_qa": {"chosen": "verdict_a", "rejected": "verdict_b"},
    "reason": "verdict_a identifies the real padding mismatch"
  },
  "chosen": "{\"score\":0.91,\"defects\":[{\"type\":\"spacing\",\"element\":\"card\",\"expected\":\"padding:16px\",\"actual\":\"padding:12px\"}]}",
  "rejected": "{\"score\":0.96,\"defects\":[]}",
  "teaching_point": "Rejected reports a cleaner result but misses the real spacing defect found by render-QA.",
  "pair_family": "spacing",
  "curriculum_tier": 2,
  "validation_outcome": "accepted"
}
```

Why valid: rejected is plausible and high-confidence, but wrong. The DPO signal teaches CriticUnit to prefer correct defect evidence over self-reported score inflation.

### 9.2 ImproverUnit Pair

Scenario: CriticUnit found `overflow` at 360px.

```json
{
  "id": "dcm-improver-overflow-run0003",
  "unit": "ImproverUnit",
  "unit_internal_state_snapshot": "{personal_leg:'user-abstracted', flow_step:'improve_pass_2'}",
  "source_design_snapshot(figma/canva)": "{card:{width:'320px', padding:'16px'}, breakpoints:[360,768,1280,1920]}",
  "candidate_result": "<div style=\"width:100%; max-width:320px; padding:16px; box-sizing:border-box;\"><h2 style=\"word-break:break-word\">...</h2></div>",
  "teacher_labels": {
    "render_qa": {"chosen": "html_a", "rejected": "html_b"},
    "reason": "html_a removes overflow at 360px while preserving source width constraints"
  },
  "chosen": "<div style=\"width:100%; max-width:320px; padding:16px; box-sizing:border-box;\"><h2 style=\"word-break:break-word\">...</h2></div>",
  "rejected": "<div style=\"width:320px; padding:16px;\"><h2>...</h2></div>",
  "teaching_point": "Rejected keeps a fixed-width layout that still overflows on mobile, while chosen preserves the design width as max-width.",
  "pair_family": "overflow",
  "curriculum_tier": 2,
  "validation_outcome": "accepted"
}
```

---

## 10. Anti-Leakage And Quarantine

Forbidden training features:

- user names, project identifiers, session identifiers;
- local render file paths or candidate filenames;
- candidate order/index;
- exact user prompt;
- raw private `.fig` / `.canva` filenames;
- subscription tier as a model feature;
- Translation-domain ids or legacy `design-conversion-*` ids as DCM foundations.

Allowed features:

- normalized source design tree;
- normalized tokens and constraints;
- render images/diff references without local path leakage;
- breakpoints;
- abstracted defects;
- unit internal state snapshot with private identifiers removed.

Quarantine required when:

- `chosen` / `rejected` preference changes under candidate permutation;
- teacher evidence is missing or contradictory;
- pair uses low-quality rejected that gives no useful preference signal;
- source design leaks private identifiers;
- source license does not allow the intended leg (`Personal leg` vs `Global leg`);
- subscription tier changes DPO semantics;
- old Translation or `design-conversion-*` artifact identity is used as foundation.

Quarantine pairs stay in DB for audit, but never enter `Train*`.

### Source And License Gates

Перед `accepted` каждая DPO-пара должна пройти source/license gate:

- `source_origin` известен: пользовательский приватный source, разрешённый open-source sample, synthetic sample или licensed dataset.
- Для `Personal leg` приватный source допустим только в пользовательской области и не становится Global knowledge без явного согласия.
- Для `Global leg` source должен иметь совместимую лицензию или разрешение на обучение и распространение производных сигналов; сомнительные источники уходят в `quarantine`.
- External AI teacher labels допустимы только если условия источника/провайдера позволяют использовать labels как training evidence.
- Raw private `.fig` / `.canva` файлы, локальные пути, URL проектов и exact prompts не попадают в trainable features.
- Отсутствие license evidence блокирует `accepted`, даже если Render-QA и teacher preference выглядят сильными.

---

## 11. Training And Measurement

After local DB builds per-unit DPO and validates a `50+50` batch, training may be invoked only through Machine-owned APIs:

```text
ILearningMachinePair<DesignConversionPersonal, DesignConversionGlobal>
  -> Train* on Machine
  -> held-out measurement
  -> SaveCheckpointAsync + backup
  -> fresh-load verification
  -> ExportAsync metadata / ImportAsync metadata-only sync
  -> continue-training check
  -> locator update
```

Expected measurement:

- held-out preference accuracy;
- reward/preference margin before vs after training;
- defect-family coverage;
- regression checks by source design;
- separate Personal leg and Global leg reporting.

Нельзя делать semantic/product claim вроде "trained", "улучшает вёрстку", "готово для тарифа" или "заменяет Bootstrap", пока нет QA/eval evidence:
числовых held-out метрик, ablation без checkpoint, leakage/permutation checks, fresh-load воспроизведения и continue-training результата.
Если evidence неполная, статус остаётся `OPEN`, `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` или `PROMOTION_BLOCKED`, но не `PASS_AS_TRAINED_FEATURE`.

This guide does not authorize implementation. It is a plan-only content artifact describing the DCM DPO construction contract.

## Pass40/41 Real-S3 Approval-Validation Boundary

Before a real-S3 DPO construction worker creates or consumes `source-manifest.json`,
`dpo-rows.jsonl`, `heldout-measurements.json`, or `checkpoint-manifest.json`, it
must run the local `approval-validation` route classifier over the supplied
approval packet.

Route meanings:

| route | meaning |
|---|---|
| `approval_packet_missing` | no approval packet was supplied; real-S3 source/DPO/checkpoint construction remains blocked |
| `template_not_approval` | the packet is a template, scaffold, placeholder, or checklist; it is not approval |
| `approval_validation_failed` | required approval fields, rights, scope, or provenance authority failed validation |
| `approval_valid_but_not_product_ready` | approval is structurally valid only for resuming real-data artifact construction |

Even after `approval_valid_but_not_product_ready`, `real_data_still_required`
remains true until the actual `source-manifest.json`, `dpo-rows.jsonl`,
`heldout-measurements.json`, and `checkpoint-manifest.json` exist and pass their
own gates. `schema/template/scaffold/focused tests do not prove` source quality,
DPO readiness, held-out quality, checkpoint promotion, upload authority, or
product readiness.

---

## 12. Метрика полезности DPO-пары для пользователя (R-09)

Каждая принятая DPO-пара получает `usefulness_score` для показа пользователю в панели «Качество обучения» (требование R-09). Метрика сообщает пользователю, насколько пара полезна для обучения модели.

Формула:

```
usefulness_score = preference_margin × difficulty_weight × novelty_weight   (диапазон 0..~2)
```

- `preference_margin` (0..1) — сила предпочтения chosen над rejected по teacher labels:
  - для `ImproverUnit` = разница визуальной точности рендера chosen и rejected;
  - для `CriticUnit` = разница корректности критики (F1 по дефектам): `F1(chosen) − F1(rejected)`.
- `difficulty_weight` — по `curriculum_tier`: tier1 = 0.5, tier3 = 1.0, tier5 = 1.5.
- `novelty_weight` — редкий `pair_family` (< 5 записей в хранилище) = 1.3, иначе 1.0.

Пример (пара `dcm-improver-overflow-run0003`): preference_margin = 0.27, tier 2 → difficulty = 1.0, novelty = 1.3 ⇒ `usefulness_score = 0.27 × 1.0 × 1.3 ≈ 0.35`.

Показ пользователю — человеческой фразой, например: «Пара подняла предпочтение на 27%, редкий класс дефекта (overflow). Полезность: 0.35».

Не показывать пользователю (см. §10): внутренние id пар, пути к файлам и рендерам, имена кандидатов, индекс кандидата, exact-prompt. Метрика только информирует пользователя — она НЕ влияет на acceptance, на `chosen`/`rejected` и НЕ является обучающим признаком.
