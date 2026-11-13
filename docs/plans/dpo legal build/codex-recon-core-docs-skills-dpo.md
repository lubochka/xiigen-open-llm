# Рекон: релевантные DPO / trainable / graph скилы (core/docs/skills)

Цель рекона: собрать evidence для ПЛАНА проекта **ILearningDynamicGraph** — авто-построение
DPO для иерархии `ILearningUnit` / `ILearningFlow` / `ILearningOrchestrator` / `ILearningDynamicGraph`.
Это сбор материала, не разрешение на реализацию.

> Замечание по терминологии: в репо разрешённые trainable-уровни называются
> `ILogicalUnit` / `ILogicalFlow` / `ILogicalOrchestrator` / `ILearningMachine` / `ILearningMachinePair`
> (см. CLAUDE.md, namespace `AgentInfrastructure.Logical`). Имена `ILearningUnit/Flow/Orchestrator`
> из задачи рекона **не найдены как существующие контракты** — это, видимо, целевые имена нового плана.
> Скилы классифицируют именно по уровням `ILogical*` + `ILearningMachine*`. Пробел отмечен ниже.

---

## Что это и где (точные ключевые пути)

Каталог-индекс (когда какой скил грузить):
- `C:/Projects/llm_mvp_core/docs/HOW-TO-USE-SKILLS.md` — таблица activation triggers; задаёт,
  какие скилы обязательны для graph/DynamicGraph/DPO/teacher работы.

Прямо релевантные DPO/trainable/graph скилы (прочитаны):
- `C:/Projects/llm_mvp_core/docs/skills/trainable-architecture-decision-SKILL.md` — **ядро классификации**:
  что является trainable-уровнем, а что детерминистический хелпер; Graph/DynamicGraph Reconstruction Gate; Legal Teacher/Provenance Gate; Dictionary→ILogicalUnit матрица.
- `C:/Projects/llm_mvp_core/docs/skills/dpo-triple-format-SKILL.md` — **обязательная форма DPO-тройки** (6 групп полей, источник каждого поля, минимум троек на класс, promotion gate); раздел Trainable Architecture and Set Quarantine.
- `C:/Projects/llm_mvp_core/docs/skills/planning-dpo-authoring-SKILL.md` — DPO для **решений планирования/маршрутизации** (ROUTING/CALIBRATION/ESCALATION), GENERATED vs REINFORCEMENT, outcome-attribution, курирование исходников.
- `C:/Projects/llm_mvp_core/docs/skills/dcm-dpo-construction-GUIDE.md` — **конкретный рабочий пример** полного контура `ILearningMachinePair -> Orchestrator -> Flow -> Unit` + DPO-vector + teacher labels + quarantine + lifecycle (Figma/Canva→HTML).
- `C:/Projects/llm_mvp_core/docs/skills/design-conversion-dpo-authoring-SKILL.md` — родственный SKILL: context-vector строки, gates, forbidden substitutes, held-out split.

Релевантные, найдены но НЕ читались целиком (кандидаты на доисследование):
- `dcm-training-GUIDE.md`, `dcm-measurement-GUIDE.md` — lifecycle/measurement половина DCM-примера.
- `design-conversion-training-runbook-GUIDE.md`, `design-conversion-negative-dpo-quality-GUIDE.md`, `design-conversion-ilearningmachine-architecture-GUIDE.md`.
- `graph-backed-routing-SKILL.md`, `graph-entity-schema-SKILL.md`, `graph-extension-protocol-SKILL.md` — graph-структуры (не обязательно trainable, проверить).
- `learning-signal-capture-SKILL.md`, `learning-loop-closure-SKILL.md`, `learning-path-audit-SKILL.md`, `training-data-gap-audit-SKILL.md` — поставляют CurriculumTier, validity gate, аудит захвата сигнала.
- `human-readable-architecture-audit-SKILL.md`, `codex-planning-quality-GUIDE.md`, `plan-review-SKILL.md`, `reconnaissance-gate-SKILL.md` — обязательны в связке для graph/DynamicGraph плана (из триггер-таблицы).

---

## Как устроено / работает (кратко)

- **HOW-TO-USE-SKILLS.md** — диспетчер. Ключевые строки триггеров:
  - graph/DynamicGraph trainable план → грузить `trainable-architecture-decision` + `human-readable-architecture-audit` + `codex-planning-quality-GUIDE` + `plan-review`; обязателен разбор nodes/edges, input/output контрактов, many-to-many, fan-out/fan-in, multi-output edge contracts, async-исполнения, **deterministic-vs-trainable authority на каждом edge** и проверка скрытых детерминистических shortcut по edge.
  - DPO/distillation teacher → грузить trainable+planning-startup+plan-review; обязательны provenance-поля; Claude/OpenAI/Gemini **запрещены как distillation teachers**.
  - high-risk ILearningMachine/DynamicGraph → recon library gate (corpus `llm mvp` ≥20 проходов, `llm_mvp_core` docs+code ≥40).
- **trainable-architecture-decision-SKILL.md** — процедура из 9 шагов: назвать capability → заявленный паттерн → прочитать код и определить механизм (trained state / dictionary-counter / scorer / selector / classifier / router / KNN-BM25-Jaccard / regex / `TryDecide*`) → отнести к ровно одному разрешённому уровню → если не уровень, то «internal detail» с указанием enclosing-уровня → runtime authority vs candidate/eval helper → проверка lifecycle-evidence → verdict. Содержит **Graph/DynamicGraph Reconstruction Gate** и **Legal Teacher/Provenance Gate** (см. ниже). Дидактические replacement-примеры (`_candidatePositionWeights` → `CandidateOrderLeakageGuardUnit` и т.п.).
- **dpo-triple-format-SKILL.md** — DPO-тройка = 6 групп: Identity / Context(`PromptSystem` обязателен) / Rejected / Chosen / TeachingPoint / CurriculumTier. Тройка без context-package = «два сниппета», не обучающая запись. Тройка генерируется при переходе rejected(<0.85)→chosen(≥0.85). Promotion gate: минимум троек на класс перед ACTIVE. Раздел quarantine: бракуй строки, где разница chosen/rejected = id/path/порядок/exact-prompt/keyword-lookup.
- **planning-dpo-authoring-SKILL.md** — учит **сами решения** (routing/calibration/escalation), а не код. GENERATED (кросс-источник, считается к порогу) vs REINFORCEMENT (derived, не считается). OUTCOME_PENDING→VALIDATED по факту исхода фазы. Курирование исходников: gold/silver/rejected, trainable_common vs adaptive_only vs evidence_only.
- **dcm-dpo-construction-GUIDE.md** — эталон. Архитектура `ILearningMachinePair<Personal,Global>`; обе ноги идут одинаковым chain `GetOrchestratorAsync -> ILogicalOrchestrator -> ILogicalFlow -> Unit`. Юниты **внутри Flow**, не владельцы обучения. `Train*` только на Machine. DPO-vector = internal unit state + source snapshot + result + teacher labels + chosen/rejected + teaching_point + pair_family + curriculum_tier + validation_outcome. Teacher = external AI + render-QA (labeler, не runtime). Анти-leakage и quarantine + source/license gate. Batch 50+50 (disjoint by source).
- **design-conversion-dpo-authoring-SKILL.md** — формализует context-vector строки и 8 gates (completeness, chosen/rejected clarity, verdict-evidence, correction, provenance/hash, quarantine, held-out split, no-shortcut-leakage).

---

## Релевантность к ILearningDynamicGraph

| Потребность плана | Что даёт скил | Файл |
|---|---|---|
| Реконструкция графа (nodes/edges, контракты, fan-out/fan-in, many-to-many, async, authority по edge) | **Graph-Style/DynamicGraph Reconstruction Gate** — единственное место с graph-специфичными требованиями | `trainable-architecture-decision-SKILL.md` |
| Классификация: что обучается, а что детерминированный хелпер | 9-шаговая процедура + verdicts (`OK_ALLOWED_LEVEL`, `OK_INTERNAL_DETAIL`, `FORBIDDEN_RUNTIME_SHORTCUT`, `NEEDS_ILOGICALUNIT_WRAPPER`, `DATASET_REBUILD_REQUIRED`…) | `trainable-architecture-decision-SKILL.md` |
| Legal teacher / provenance для авто-построения DPO | **Legal Teacher/Provenance Gate**: поля `teacher_access_mode/hosting_provider/model_id/model_license/provider_terms_checked/closed_proprietary_model`; closed-proprietary teacher → `DATASET_REBUILD_REQUIRED` | `trainable-architecture-decision-SKILL.md`; триггер в `HOW-TO-USE-SKILLS.md` |
| Точная форма DPO-записи для каждого уровня | 6-группная тройка + источник полей + промоушн-гейт по количеству | `dpo-triple-format-SKILL.md` |
| DPO для решений Orchestrator/маршрутизации (а не только Unit-вывода) | три типа planning-сигналов + GENERATED/REINFORCEMENT + outcome-attribution | `planning-dpo-authoring-SKILL.md` |
| Полный эталон контура Machine→Orchestrator→Flow→Unit + авто-построение пар + lifecycle | DCM: authority path построения chosen/rejected, per-unit DPO, 50+50, checkpoint/export/import/fresh-load/continue-training | `dcm-dpo-construction-GUIDE.md` |
| Измерение/teacher-provenance (held-out accuracy, reward margin, leakage/permutation, ablation) | DCM раздел 11 + design-conversion gates; статусы `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` vs `PASS_AS_TRAINED_FEATURE` | `dcm-dpo-construction-GUIDE.md`, `design-conversion-dpo-authoring-SKILL.md` |

Главный вывод для плана: **graph-DPO не имеет отдельного скила** — graph-разбор берётся из Reconstruction Gate в `trainable-architecture-decision`, форма строки — из `dpo-triple-format`, решения-как-DPO — из `planning-dpo-authoring`, а целостный пример «как авто-строятся пары и крутится lifecycle» — из DCM-гайдов.

---

## Классификация уровней (по правилам скилов) — как применять в плане ILearningDynamicGraph

Разрешённые trainable-уровни (по `trainable-architecture-decision-SKILL.md` и CLAUDE.md):
- `ILearningMachine` — владелец `Train*`, checkpoint, export/import, promotion.
- `ILearningMachinePair<Personal,Global>` — две ноги (общая + пользовательская).
- `ILogicalOrchestrator` — маршрутизация/последовательность узлов.
- `ILogicalFlow` — порядок вызовов, итерации, остановка.
- `ILogicalUnit<TInput,TOutput>` — чистый inference-контракт (без `Train*`).

Детерминистические хелперы (НЕ являются уровнем; только internal detail / eval-only / baseline) — с правилом-ссылкой:
- scorer, selector, classifier, router, detector — `internal detail`, обязаны принадлежать enclosing-уровню (скил §«The only allowed trainable architecture levels» + Decision Procedure шаги 4–6).
- KNN / BM25 / Jaccard / retrieval — **audit-triggered, не запрещены**: допустимы как memory/feature/retriever **внутри** уровня; нарушение — KNN как самостоятельная «вся архитектура» или как unlabelled final authority (скил §«KNN/retrieval note», verdict `OK_MODEL_COMPONENT` vs `FORBIDDEN_RUNTIME_SHORTCUT`).
- regex/keyword/dictionary/counter, `TryDecide*` / `ChooseBestPattern` / `SelectBestPattern` / `CountMatchingSignals` — **не паттерн архитектуры**; если заявлены trainable — `NEEDS_ILOGICALUNIT_WRAPPER` или `BASELINE_MUST_BE_LABELLED` (скил Decision Procedure + Dictionary→ILogicalUnit матрица).
- **edge-shortcut в графе**: любой детерминированный shortcut на edge без явной пометки eval-only/helper/baseline и без ablation/no-shortcut теста = `FORBIDDEN_RUNTIME_SHORTCUT` (скил §Graph Reconstruction Gate: «Every edge must name who owns final authority»).
- teacher: closed-proprietary (Claude/OpenAI/Gemini) как distillation/DPO target → запрещён, `DATASET_REBUILD_REQUIRED`; допустим только как non-training judge/eval (скил §Legal Teacher/Provenance Gate).

Признаки, что строка DPO учит shortcut (→ quarantine/rebuild, не trainable), по `dpo-triple-format` §Quarantine и DCM §10:
source row id, pair id, filename/path, candidate order/index, exact prompt memory, project-specific execution detail (для universal-модели), keyword→skill lookup, subscription tier как фича, нестабильность chosen под permutation.

---

## Пробелы / что доисследовать

1. **Имена контрактов из задачи (`ILearningUnit/Flow/Orchestrator/ILearningDynamicGraph`) не найдены** в скилах — там везде `ILogical*` + `ILearningMachine*`. До плана надо подтвердить в коде (`00.Core/LearningMachineContracts.cs`) и в `docs/COMPONENT-INTERFACES.md`, существует ли реально `ILearningDynamicGraph` или это новое имя нового уровня; от этого зависит, как ложатся правила классификации на «граф как уровень».
2. **Нет отдельного graph-DPO скила.** Доисследовать `graph-backed-routing-SKILL.md`, `graph-entity-schema-SKILL.md`, `graph-extension-protocol-SKILL.md` — задают ли они node/edge schema, пригодную как контракт DPO-строки для рёбер графа (multi-output edge contracts).
3. **Lifecycle/measurement половина** прочитана только в обзоре DCM §11. Для плана авто-построения DPO нужны детали из `dcm-training-GUIDE.md` и `dcm-measurement-GUIDE.md` (checkpoint/fresh-load/continue-training/held-out метрики) — подтвердить применимость к graph-узлам, а не только к Critic/Improver.
4. **CurriculumTier и validity-gate** упомянуты, но таблицы тиров и кросс-модельные условия лежат в `learning-signal-capture-SKILL.md` / `training-data-gap-audit-SKILL.md` — не читались; нужны для «минимум троек на узел/ребро».
5. **Каталог обрезан** (прочитаны строки 1–433 из 782 в HOW-TO-USE-SKILLS.md). Вторая половина может содержать ещё triggers по trainable/graph — добрать при написании плана.
6. **Связка обязательных скилов для graph-плана** (`human-readable-architecture-audit`, `codex-planning-quality-GUIDE`, `plan-review`, `reconnaissance-gate`) только идентифицирована по триггерам, не прочитана — они нужны для admission плана, не для самого DPO-контракта.
