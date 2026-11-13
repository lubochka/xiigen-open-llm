---
artifact_type: ilearningdynamicgraph_plan_block_12
authority_source: latest unquoted Luba work order for DPO legal build library
status: populated_now_planning_only
last_updated: 2026-06-23
write_scope: C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build\claude-*.md
canonical_filename: claude-plan-12-no-hidden-determinism.md
benchmark_read_only: codex-plan-12-no-hidden-determinism.md
phase_map: Блок плана 12 ≡ доска Ф8 (No-hidden-determinism), подшаги Ф8.1.1–2, Ф8.2.1–2
canon_counters_unchanged: 12 фаз / 26 подфаз / 59 базовых подшагов; нумерация Ф<P>.<SP>.<SS>
---

# Блок плана 12 — Замена скрытого детерминизма (классификация 9 механизмов + cutover на обучаемый авторитет ребра)

Автор блока: Claude Opus 4.8 MAX (роль — автор полностью решённого блока плана). Это
часть ПЛАНА проекта `ILearningDynamicGraph` (документация-решение), **не** разрешение
на реализацию продукта и **не** промпт исполнителю. Все цитаты кода — реальные,
проверены чтением файла в `C:/Projects/llm_mvp_core`, с `file:line`. Цитаты чужих
планов/транскриптов — evidence-only.

> Версия namespace `claude-*`. Бенчмарк `codex-plan-12-no-hidden-determinism.md`
> прочитан как образец (read-only) и **улучшен** в слабых местах. Бенчмарк — это 48
> строк: один интерфейс `IDynamicGraphEdge`, блок Canon, список Gate B и «Final
> Questions Only: None». Этого недостаточно для Part A-стандарта братских `claude-*`
> блоков (см. `claude-plan-02-edge.md` — 1071 строка). Конкретные улучшения над
> бенчмарком:
>
> 1. **Полный Part A** (§0 UML + per-connection/per-branch симуляции, §1 алгоритм с
>    классом+методом+параметрами+шагами, §2 «что НЕ делает» + примеры значений, §3
>    цитаты кода с `file:line` + сейчас/почему/должно стать/как менять, §4 статус,
>    §5 иерархия L5→L2). В бенчмарке Part A отсутствует.
> 2. **Заполнена таблица 9 механизмов** (`classified_mechanism_count == 9` — канон
>    R-07 / P-12). Бенчмарк только декларирует запрет, но не перечисляет и не
>    классифицирует механизмы. Здесь каждый из 9 имеет вердикт ∈ {OK_ALLOWED_LEVEL /
>    OK_INTERNAL_DETAIL / OK_MODEL_COMPONENT / BASELINE_MUST_BE_LABELLED /
>    FORBIDDEN_RUNTIME_SHORTCUT} с `file:line`.
> 3. **Заполненный per-method блюпринт замены** (`SemanticEdgeRouterUnit` NEW +
>    `SemanticEdgeRoutingState` typed + training-algo + excluded-leakage + DPO
>    позитив/rejected/quarantine) — анти-template FC-30. Бенчмарк даёт только
>    контракт-интерфейс без блюпринта.
> 4. **`CandidateOrderLeakageGuardUnit` (ADAPTED-FROM-EXISTING)** специфицирован с
>    negative-тестом. Честно: одноимённый guard **уже реализован** в основном дереве —
>    `43.SmartSkillsDomain/SkillSelectionLogicalHierarchy.cs:271` (UnitId
>    `candidate_order_leakage_guard`, Role `candidate_position_diagnostics_provenance_only`,
>    возвращает `CandidateOrderLeakageReport`), и он уже классифицирует позицию кандидата
>    как НЕ финальную власть (ровно инвариант R13). Edge-версия — **порт паттерна на
>    ребро**, не скелет с нуля. Это сильнее бенчмарка именно тем, что переиспользует
>    готовый прецедент, а не выдаёт его за новый.
> 5. **Механизм #4 = `OrchestrationScenarioPhasePlanner` (file:line, не «не найден»):**
>    9-й механизм подтверждён чтением кода как детерминированный keyword-планировщик фаз
>    (`OrchestrationScenarioPhasePlanner.cs:5/:21/:30`), вызываемый в рантайме
>    (`OrchestrationTaskDecompositionFlow.cs:70`, `OrchestrationAcceptanceGateEvaluator.cs:39`).
>    Это прямой скрытый детерминизм (A-LM-09/A-LM-13/R4), поэтому вердикт
>    `FORBIDDEN_RUNTIME_SHORTCUT`, а не «unverified». Все 9 строк теперь с `file:line`
>    (см. §3.B); в §13 остаётся только архитектурный вопрос (обучаемый план фаз vs
>    baseline).
> 6. **`TeacherProvenance` приведён к каноническим 20 полям** (13+5+2 RR6) с `JudgeModel`
>    ВНУТРИ записи (владелец канона — блок 08), как во всех братских `claude-*`.
> 7. **Явная §5 иерархия** L5→L4→L3→L2 с Input/Output и помеченными цепочками
>    (cutover/ablation/guard) — в бенчмарке отсутствует.
> 8. **Cutover-гейт усилен:** cutover запрещён, пока выбранный edge-unit не получил
>    `PASS_AS_TRAINED_FEATURE` по two-PASS блока 11 (held-out + ablation + lifecycle +
>    no-UNKNOWN). Бенчмарк упоминает «plan-11 two-PASS proof» одной строкой; здесь это
>    развёрнуто в проверяемый Gate B и failure-mode.

## CANON-инварианты (сохранены без изменений)

- `ILearningDynamicGraph = ILearningMachine<DynamicGraphDomain>` — уровень **L5**, без
  нового 6-го уровня (A-LM-12). В этом блоке **0 новых уровней**.
- Легальный teacher-гейт: closed-модель допустима **только как judge/eval**, НИКОГДА
  как teacher для дистилляции; контаминация → `DATASET_REBUILD_REQUIRED`.
- Нет скрытого детерминизма (A-LM-09 / A-LM-13 / R4): финальный авторитет выбора ребра —
  обучаемая `ILogicalUnit`; детерминированные scorer/router/счётчики/словари/порядок/
  индекс/путь помечены baseline/eval-only/internal/model-component, но **не финальная
  власть** после cutover.
- Канон-имена общие со всеми `claude-*`: `GraphEdgeKind` (РОВНО 5 членов
  {FanOut, FanIn, ManyToMany, MultiOutput, ReturnLoop}), `EdgeAuthorityUnit` /
  `SemanticEdgeRouterUnit` (обучаемый авторитет ребра), `EdgeAuthorityInput` /
  `EdgeAuthorityDecision`, `CandidateOrderLeakageGuardUnit`, `GraphDpoPair`,
  `TeacherProvenance` (20 полей; 13+5+2 RR6).
- Канон-счётчики доски сохранены: **12 фаз (Ф0…Ф11), 26 подфаз, 59 базовых подшагов**,
  нумерация `Ф<P>.<SP>.<SS>`. Данный блок плана 12 ≡ доска **Ф8** (No-hidden-determinism),
  подшаги **Ф8.1.1, Ф8.1.2, Ф8.2.1, Ф8.2.2** (`claude-EXECUTION-CONTROL-BOARD.md`
  §Ф8:898-937).

Маппинг ролей (CANON): `ILearningUnit` ≡ `ILogicalUnit` (L2,
`00.Core/LogicalContracts.cs:97`); `ILearningFlow` ≡ `ILogicalFlow` (L3,
`00.Core/LogicalContracts.cs:166`); `ILearningOrchestrator` ≡ `ILogicalOrchestrator`
(L4, `00.Core/LogicalContracts.cs:202`); `ILearningDynamicGraph` =
`ILearningMachine<DynamicGraphDomain>` (L5, `00.Core/LearningMachineContracts.cs:208`).

Скилы, по которым написан блок (загружены и применены): `plan-review-SKILL.md`
(Gate 0; Graph/DynamicGraph review gate; Deterministic evidence masquerade firewall;
FC-26 тест-стратегия; FC-30 anti-template), `trainable-architecture-decision-SKILL.md`
(5 уровней, Solved Architecture Block, Graph-Style Reconstruction, Required Review
Table, Dictionary-to-ILogicalUnit матрица, Legal Teacher Gate, verdicts),
`implementation-doc-uml-SKILL.md` (§0/§4 — `note for` на каждый класс + таблица
симуляции на каждую связь/ветку).

## Authority Anchor

```text
CURRENT TASK: Построить улучшенный claude-* блок плана 12 (no-hidden-determinism),
              читая codex-plan-12 как бенчмарк (read-only); PLAN-ONLY.
NOT THE TASK: Реализация/код/билд/тесты/git/обучение; правка любых codex-* файлов;
              правка старого каркаса claude-00..07; внешние AI/модели; ложные claim
              о выполненных циклах ревью.
EVIDENCE-ONLY: все codex-*.md (план Codex, в т.ч. codex-plan-12 бенчмарк, мастер
              codex-ILEARNINGDYNAMICGRAPH-PLAN-v1.md §A.4, codex-recon-*,
              codex-50-cycle-*, codex-skill-improvement-*), старый claude-00..07.
              Это материал для чтения, не источник задачи и не цель правки.
```

---

## 0. Solved Architecture Block (требование trainable-architecture-decision-SKILL)

```text
Solved Architecture Block (замена скрытого детерминизма на ребре):
  problem:
    После ввода типизированного ребра (блок 02) и обучаемого авторитета остаётся
    риск, что РЕАЛЬНЫЙ финальный выбор «кому отдать деталь» по-прежнему делает
    детерминированная формула/словарь/индекс/порядок — то есть обучаемый юнит есть
    «на бумаге», а власть на ребре скрыто держит детерминированный shortcut
    (A-LM-09/A-LM-13 нарушение, masquerade). Сегодня финал считают: фикс-формула
    MultiSubPhaseRouterUnit.Score (MultiSubPhaseRouterUnit.cs:283-284) и
    KeywordUnitScorer.Score (26.TrainableRouter/Training/KeywordUnitScorer.cs:38,49).
  selected_design:
    (1) Классифицировать ВСЕ 9 механизмов-кандидатов на скрытый детерминизм по
        вердиктам skill (§3.B). (2) Понизить детерминированные финалы до
        генераторов кандидатов / baseline / eval-only / model-component (НЕ удалять
        — нужны для ablation). (3) Финальный авторитет ребра = обучаемый
        SemanticEdgeRouterUnit : ILogicalUnit<EdgeAuthorityInput,EdgeAuthorityDecision>
        (он же EdgeAuthorityUnit из блока 02 — одно каноническое имя роли
        edge-authority). (4) Ввести CandidateOrderLeakageGuardUnit (ADAPTED-FROM-EXISTING,
        порт прецедента SkillSelectionLogicalHierarchy.cs:271 на ребро) —
        permutation-инвариантный guard, diagnostics, НЕ финал. (5) Cutover через
        latest-wins реестра (ResolveLatest, InMemoryLogicalUnitRegistry.cs:107),
        разрешён ТОЛЬКО когда обучаемый юнит имеет PASS_AS_TRAINED_FEATURE
        (two-PASS блока 11).
  rejected_alternatives:
    (a) Удалить детерминированные роутеры совсем — нельзя: нечем делать ablation и
        cold-start генерацию кандидатов; отклонено (понижаем до baseline).
    (b) Оставить Score()/KeywordUnitScorer финалом «потому что калибровано» —
        нарушает A-LM-09 (скрытый детерминизм как авторитет); отклонено.
    (c) Сделать guard финальным селектором — guard только блокирует утечку позиции,
        он не понимает смысл; отклонено (guard = diagnostics).
    (d) Ввести новый 6-й уровень под «edge router» — нарушает A-LM-12; отклонено.
    (e) Добавить Train* на флоу/юнит ради быстрого cutover — нарушает A-LM-02
        (Train* только на L5-машине); отклонено.
  UML: см. §4 (Mermaid classDiagram + per-connection simulation).
  data_flow: см. §3.A (рантайм cutover-проход) и §3.D (ablation-проход).
  current_code_refs: см. §3 и §6 (все с file:line, проверены чтением).
  future_files_classes_methods_signatures_dependencies: см. §5 (NEW-контракты).
  deterministic-vs-learned boundary: см. §3.B таблица 9 механизмов и §3.C граница.
    Граница: генерация кандидатов + topology-порядок + confidence-floor + K-NN-память
    + path-free нормализация = honest deterministic/helper/model-component; ФИНАЛЬНЫЙ
    выбор/порядок/уверенность = обучаемый SemanticEdgeRouterUnit.
  gates_and_tests: см. §8 (гейты) + §9 (тест-стратегия FC-26) + §10 (Gate B).
  failure_modes: см. §10.1.
  docs: см. §12 (затрагиваемые доки/реконы/решения).
```

### 0.1. Graph-Style Reconstruction (требование Graph-Style/DynamicGraph Reconstruction Gate)

```text
roles_and_hierarchy: L5 машина учит → L4 оркестратор выбирает ребро → L3 флоу ребра
  исполняет переход → L2 обучаемый SemanticEdgeRouterUnit решает финал; baseline-роутеры
  → генераторы кандидатов; guard-юнит → diagnostics.
DynamicGraph nodes: суб-фазы/узлы (ILogicalUnit), вне данного блока (claude-plan-01-node).
DynamicGraph edges: IDynamicGraphEdge (из блока 02) — типизированный переход; этот блок
  закрывает ВЛАСТЬ на ребре (кто финальный авторитет).
input_contracts: EdgeAuthorityInput (NEW, физически без candidate-index/source-path/
  exact-prompt) для финала; CandidateOrderProbeInput (NEW) для guard.
output_contracts: EdgeAuthorityDecision (финал); CandidateOrderLeakageReport (guard).
many_to_many_outputs / fan_out / fan_in: владелец — блок 02 (DynamicGraphEdgeFlow);
  этот блок только гарантирует, что ВЫБОР целей fan-out делает обучаемый юнит, не формула.
multi_output_edge_contracts: EdgeAuthorityDecision.OrderedTargets[] (из блока 02).
async_graph_execution: ParallelEligible (булев флаг ребра, НЕ член enum); параллельные
  ветки — AdaptiveLogicalOrchestrator.cs:462, зависимые — последовательно (:481).
deterministic_vs_trainable_authority_on_edges: финал РЕБРА = ОБУЧАЕМЫЙ
  SemanticEdgeRouterUnit (DeterministicFinalAuthority=false). 9 механизмов
  классифицированы (§3.B); каждый forbidden/baseline авторитет РЕБРА помечен и снабжён
  блюпринтом (§5.3). Механизм #4 (B-22, диспозиция спроектирована): детерминированный
  план фаз сценария (ВЫБОР `PlanPhases` + ПОРЯДОК `PhaseOrder/PhaseIndex/BuildPhases`) —
  отдельный L4-авторитет пути оркестрации, НЕ авторитет ребра; помечен
  **baseline-ordering / `BASELINE_MUST_BE_LABELLED`** (рекоменд. дефолт v1, прецедент Q14);
  инстанцированный обучаемый блюпринт `SemanticPhasePlanUnit` дан в §13 Q1; **RESOLVED(Luba
  2026-06-30): НЕ активировать в v1** — остаётся `BASELINE_MUST_BE_LABELLED`, обучаемый план
  фаз = deferred-to-later-phase (§3.0, §8.2, Gate B п.6, §7).
hidden_deterministic_shortcut_check_tied_to_each_edge: ablation §10 п.3 (выключить
  обучаемый авторитет → accuracy падает ≥ порога) + permutation-invariance §10 п.1
  (CandidateOrderLeakageGuardUnit) + no-source-path/no-candidate-index/no-exact-prompt §8.3.
```

---

## 1. Что это и зачем (роль блока в `ILearningDynamicGraph`)

Этот блок отвечает на один прямой вопрос: **точно ли финальное решение на ребре
принимает обученная модель, а не спрятанная детерминированная формула?** Блоки 01-02
вводят узел и ребро и обучаемый авторитет; но «ввести обучаемый юнит» ещё не значит
«он реально решает». Если после всех контрактов рантайм по-прежнему вызывает
`MultiSubPhaseRouterUnit.Score(...)` (`MultiSubPhaseRouterUnit.cs:283`) или
`KeywordUnitScorer.Score(...)` (`KeywordUnitScorer.cs:38`) как **последнее слово** —
это и есть **скрытый детерминизм** (masquerade), запрещённый A-LM-09/A-LM-13/R4.

Блок делает три вещи:

1. **Инвентаризация + классификация 9 механизмов** (Ф8.1.1). Берём все места в коде,
   которые могут оказаться скрытым авторитетом — scorer'ы, роутеры, реестр
   latest-wins, K-NN-память, path-free нормализацию, confidence-floor — и присваиваем
   каждому ровно один вердикт из набора skill (`OK_ALLOWED_LEVEL` / `OK_INTERNAL_DETAIL`
   / `OK_MODEL_COMPONENT` / `BASELINE_MUST_BE_LABELLED` / `FORBIDDEN_RUNTIME_SHORTCUT`).
   Канон: `classified_mechanism_count == 9` (R-07; `claude-EXECUTION-CONTROL-BOARD.md`
   §Ф8.1.1:926).

2. **Блюпринты замены для FORBIDDEN/BASELINE** (Ф8.1.2). Для каждого механизма, который
   сегодня — финальная власть, пишем заполненный per-method блюпринт: какой обучаемый
   класс его заменяет (`SemanticEdgeRouterUnit` NEW), какое typed-состояние
   (`SemanticEdgeRoutingState` NEW), алгоритм обновления обучения, список запрещённых
   leakage-сигналов, и три DPO-примера (положительный/отклонённый/карантин). Это
   анти-template требование FC-30: «wrap in ILogicalUnit» без имён классов/состояния/
   алгоритма — НЕ решение.

3. **Cutover + guard + ablation** (Ф8.2.1-2). Переиспользуем `CandidateOrderLeakageGuardUnit`
   (ADAPTED-FROM-EXISTING — одноимённый guard уже есть в
   `43.SmartSkillsDomain/SkillSelectionLogicalHierarchy.cs:271`; портируем паттерн на
   ребро, не пишем с нуля) — permutation-инвариантный страж, который ловит утечку «выбор
   по позиции кандидата». Затем делаем cutover: реестр `latest-wins`
   (`InMemoryLogicalUnitRegistry.ResolveLatest`, `:107`, сортировка по
   `RegistrationOrder`, `:118`) начинает отдавать обученный `SemanticEdgeRouterUnit`
   вместо baseline. Baseline-роутеры **не удаляются** — они нужны как генераторы
   кандидатов и как контроль для ablation. Cutover **запрещён**, пока обученный юнит не
   получил `PASS_AS_TRAINED_FEATURE` (two-PASS блока 11).

Почему это отдельный блок (Ф8 на доске), а не часть блока 02: блок 02 проектирует
**контракт** ребра и авторитета; блок 12 — это **операция cutover и анти-masquerade
аудит** над уже существующим кодом. Это разные exit-gate: 02 закрывается «контракт +
скелет авторитета описаны», 12 закрывается «9 механизмов классифицированы, baseline
понижены, обученный юнит — финал, ablation доказывает не-shortcut».

### 1.1. Границы блока (что НЕ решает блок 12)

- НЕ проектирует обучение узла (это блок 01) и не вводит сам контракт ребра/fan-in
  (это блок 02) — опирается на них.
- НЕ запускает обучение/тесты/cutover (PLAN-ONLY). Описывает условие, при котором
  cutover РАЗРЕШЁН.
- НЕ удаляет детерминированные хелперы. Удаление baseline = потеря ablation-контроля,
  это явная ошибка (см. §10.1).

---

## 2. Концепт простыми словами (аналогия, без кода)

Аналогия завода: узел = станок, поток = линия, ребро = лента + стрелочник у развилки
(см. блок 02). Блок 12 — это **приёмка стрелочника и его рокировка**.

Представь, что на завод поставили нового «умного» стрелочника (обучаемого), но
старый «тупой» стрелочник с бумажной таблицей всё ещё физически стоит у рычага. Пока
никто не переключил рычаг, деталь по-прежнему едет туда, куда показывает **бумажная
таблица**, а «умный» висит рядом без власти. Это и есть скрытый детерминизм: на словах
«у нас обучаемый выбор», на деле решает формула `0.55·… + 0.25·… + 0.20·… + 0.02·…`
(`MultiSubPhaseRouterUnit.cs:283`).

Блок 12 делает три вещи как начальник цеха:

1. **Обходит весь цех и переписывает, кто на что влияет** (классификация 9
   механизмов): этот рычаг — финальный (плохо, заменить), этот — только подсказывает
   кандидатов (ок как baseline, пометить), этот — память про похожие прошлые случаи
   (ок как model-component), этот — честный «не знаю» при тумане (ок как guard).
2. **Ставит контролёра честности у развилки** (`CandidateOrderLeakageGuardUnit`):
   переставляет кандидатов местами и проверяет, что выбор НЕ изменился. Если выбор
   меняется от перестановки — это значит стрелочник смотрит на «кто первый в списке»,
   а не на смысл. Контролёр поднимает флаг и блокирует такую фичу. Контролёр сам **не
   выбирает** — он только ловит жульничество.
3. **Переключает рычаг на умного — но только после экзамена** (cutover после
   two-PASS): умный стрелочник получает власть, лишь когда доказал на отложенной
   проверке (held-out), что он реально лучше бумажной таблицы, и что если его выключить
   — качество просядет (ablation). Бумажную таблицу не выбрасывают: она остаётся как
   «черновик-подсказчик» и как «эталон для сравнения».

Легальность учителя (Legal Teacher Gate): если «правильные ответы» для обучения умного
стрелочника берём у внешней модели-эксперта, эксперт обязан быть **легальным**
open-weight/permissive (Qwen3 Apache-2.0 / DeepSeek MIT / Kimi K2 / GLM / Mistral)
через нейтрального провайдера. **Claude / OpenAI / Gemini как teacher для дистилляции —
НИКОГДА.** Судья для оценки может быть сильнее, но судья не цель обучения.

---

## 3. Алгоритм/решение ПОШАГОВО (человеческим языком, полное решение)

Ниже — **полное решение**, а не «исполнитель сделает X». Названы конкретные
классы/методы (NEW и существующие с `file:line`), параметры, порядок.

### 3.0. Честное замечание о наличии классов (правило «нет нечестных ответов»)

Доска и бенчмарк называют **9** механизмов. **Все 9 классов подтверждены чтением**
файла в `C:/Projects/llm_mvp_core` (см. §3.B и §6), у каждого есть `file:line`. Девятый
механизм называется в коде не `ScenarioPhasePlanner`, а **`OrchestrationScenarioPhasePlanner`**
(префикс `Orchestration`): класс
`26.TrainableRouter/Training/OrchestrationScenarioPhasePlanner.cs:5`. Это **чистый
детерминированный keyword-планировщик**: захардкоженный массив `PhaseRules`
(`(string Phase, string[] Keywords)[]`, `:7-19`) и метод `PlanPhases` (`:21`), который
выбирает фазы по `text.Contains(keyword)` (`:30`). И это **не мёртвый код**: он вызывается
как рантайм-авторитет в `OrchestrationTaskDecompositionFlow.cs:70`
(`_phasePlanner.PlanPhases(request.Prompt, request.StepHints)`) и в
`OrchestrationAcceptanceGateEvaluator.cs:39`
(`_phasePlanner.PlanPhases(scenario.Prompt, scenario.ExpectedSteps)`). То есть это в
точности тот скрытый детерминированный shortcut (формула ключевых слов решает, какие
фазы оркестрации запускать), который A-LM-09/A-LM-13/R4 обязаны ловить. Предыдущая
пометка «класс не найден / только в worktree-копиях» была **фактически неверной** и
снята. `classified_mechanism_count == 9`, и теперь все 9 строк снабжены `file:line`
(Gate B §10 п.6). **B-22 (2026-06-30):** диспозиция #4 спроектирована (рекоменд. дефолт v1
= `BASELINE_MUST_BE_LABELLED` по прецеденту Q14 + инстанцированный обучаемый блюпринт
`SemanticPhasePlanUnit` в §13 Q1). **RESOLVED(Luba 2026-06-30):** обучаемый план фаз **НЕ
активируется в v1** — остаётся детерминированный `BASELINE_MUST_BE_LABELLED` baseline;
блюпринт исполняется только при будущей активации = deferred-to-later-phase.

**Граница scope #4 — детерминированный ПОРЯДОК фаз (не только выбор).** Помимо
keyword-ВЫБОРА фаз (`PlanPhases`), на том же пути оркестрации есть детерминированная
власть над финальным **ПОРЯДКОМ** фаз: статический массив `PhaseOrder[]`
(`OrchestrationTaskDecompositionFlow.cs:18-30`) → `PhaseIndex` (`:197`) →
`OrderBy(PhaseIndex)` (`:75`) → `BuildPhases` (`:148`). Чтобы не ломать канон-счётчик
`classified_mechanism_count == 9` (R-07) лишней 10-й строкой, этот порядок **явно
включён в scope механизма #4 и в §13 Q1**: он помечен как **baseline-ordering**
(детерминированный порядок фаз), а **НЕ финальная семантическая власть**. Его
диспозиция следует за #4 = **спроектирована (B-22)**: рекоменд. дефолт v1 =
`BASELINE_MUST_BE_LABELLED` (детерминированный порядок фаз остаётся честным baseline без
claim, прецедент Q14); инстанцированный обучаемый блюпринт `SemanticPhasePlanUnit` (§13 Q1)
покрывает ОБА — и выбор `PlanPhases`, и порядок `PhaseOrder/PhaseIndex/BuildPhases` — на
случай будущей активации. **RESOLVED(Luba 2026-06-30): активация в v1 НЕ дана** — порядок и
выбор фаз остаются помеченными `BASELINE_MUST_BE_LABELLED`; при будущей активации оба финала
понижаются в baseline-генератор (как #1/#2, не удалять, нужны для ablation), а власть
переходит обучаемому юниту. Активация = deferred-to-later-phase.
Так заявление §0.1/§3.B «немаркированных финальных авторитетов на ребре — 0» остаётся
истинным: порядок фаз — это L4-путь оркестрации (область #4), а не власть на ребре, и он
помечен, а не оставлен немаркированным.

### 3.A. Рантайм-проход ребра ПОСЛЕ cutover (инференс, кто реально решает)

Контекст вызова сегодня: `AdaptiveLogicalOrchestrator.RunAsync`
(`AdaptiveLogicalOrchestrator.cs:85`) → `RouteAsync` (`:316`) → юнит-резолвинг через
реестр `InMemoryLogicalUnitRegistry.ResolveLatest` (`:107`). Решение (как должно стать):

1. **Шаг A — генерация кандидатов (baseline, детерминированная).** Поток ребра
   (`DynamicGraphEdgeFlow`, блок 02) вызывает понижённые baseline-роутеры как
   `IEdgeCandidateProvider.Propose`: словарь `SubPhaseAnchorTerms`
   (`MultiSubPhaseRouterUnit.cs:23`) + формула `Score` (`:283`) + `KeywordUnitScorer.Score`
   (`KeywordUnitScorer.cs:38,49`). Выход — **неупорядоченный** список кандидатов с
   `BaselineScore`, помечен `authority_role="baseline_candidate_generator"`,
   `isLearned=false`. Это НЕ финал.

2. **Шаг B — guard-проба порядка (diagnostics).** `CandidateOrderLeakageGuardUnit`
   (NEW) прогоняет вход с N перестановками кандидатов и фиксирует
   `CandidateOrderLeakageReport { PermutationInvariant; PermutationCount;
   BlockedFeatures }`. Если выбор менялся бы от перестановки — guard ставит
   `PermutationInvariant=false` и **блокирует** позиционную фичу (она не уйдёт в финал).
   Прецедент уже есть в проекте: `43.SmartSkillsDomain/SkillSelectionContracts.cs:35`
   (`PermutationSelectionInvariant`), `:45` (`CandidateOrderPermutationProbe`),
   `SkillSelectionLogicalHierarchy.cs:150` (`candidate_position_scaffold_final_authority=false`).

3. **Шаг C — анти-leakage нормализация входа.** `EdgeAuthorityInput` (NEW) строится
   так, чтобы **физически не содержать**: (а) индекс/позицию кандидата
   (permutation-invariance), (б) `SourcePath` как фичу скоринга (повторяем образец
   `MultiSubPhaseRouterUnit.BuildPathFreeScoringText`, `:291`; путь остаётся **только**
   как warning `source_path_used_for_scoring=false`, `:373`), (в) exact-prompt/source-id
   replay. Несёт только семантику: токены смысла, предикаты, контекст-сигналы.

4. **Шаг D — обучаемый финал.** Реестр `ResolveLatest` (`:107`,
   `OrderByDescending(RegistrationOrder)`, `:118`) отдаёт **обученный**
   `SemanticEdgeRouterUnit` (NEW, `IsLearned=true`), а не baseline. Юнит:
   - ранжирует кандидатов **по выходу обученной модели**; K-NN (K-Nearest Neighbours)-
     ретривер по обученному индексу — допустимый **model-component** ВНУТРИ уровня
     (прецедент `KnnLogicalUnit.ExecuteAsync` → `_index.Search(query, _k)`,
     `KnnLogicalUnit.cs:218`), а не exact-prompt replay;
   - возвращает `EdgeAuthorityDecision { OrderedTargets[(id,confidence)]; FanOutCount;
     KnowledgeEdge; AuthoritySource="trained_edge_unit"; AblationBaselineRank[] }`;
   - **confidence-floor честность**: если максимальная уверенность ниже порога — НЕ
     выдумывает выбор, ставит `KnowledgeEdge=true` (питает существующую ветку
     `AdaptiveLogicalOrchestrator.cs:504`).

5. **Шаг E — cutover-гейт (когда обученный получает власть).** До регистрации обученного
   юнита как `latest` проверяется two-PASS блока 11: held-out `PassesGate=true`,
   `RewardMargin>0`, `AccuracyAfter>AccuracyBefore` (без UNKNOWN) **И** lifecycle
   (fresh-load + continue-training) **И** ablation (§3.D). Только тогда регистрация даёт
   ему больший `RegistrationOrder`, и `ResolveLatest` начинает его отдавать (A-LM-03).

6. **Шаг F — baseline остаётся для ablation.** Понижённые роутеры не удаляются; они
   доступны как «обученный авторитет выключен» режим для §3.D.

### 3.B. Таблица 9 механизмов (Required Review Table; вердикт на каждый, file:line)

> Канон R-07 / P-12: `classified_mechanism_count == 9`; каждый вердикт ∈ {OK_ALLOWED_LEVEL,
> OK_INTERNAL_DETAIL, OK_MODEL_COMPONENT, BASELINE_MUST_BE_LABELLED, FORBIDDEN_RUNTIME_SHORTCUT};
> every quote has file:line.

| # | Механизм (класс/метод) | file:line (проверено) | Что вычисляет сейчас | Авторитет сейчас | Вердикт | Что должно стать |
|---|---|---|---|---|---|---|
| 1 | `MultiSubPhaseRouterUnit.Score` | `21.AgentInfrastructure/Logical/MultiSubPhaseRouterUnit.cs:283-284` | фикс-формула `0.55/0.25/0.20/0.02` ранжирует кандидата fan-out | финальный выбор | **FORBIDDEN_RUNTIME_SHORTCUT** (если оставить финалом) | понизить до `IEdgeCandidateProvider` baseline; финал → `SemanticEdgeRouterUnit` |
| 2 | `KeywordUnitScorer.Score` | `26.TrainableRouter/Training/KeywordUnitScorer.cs:38,49` | `BaseScore + hits + domainBoost + directionBoost`, `OrderByDescending(:31)` | финальный ранг юнита | **FORBIDDEN_RUNTIME_SHORTCUT** (если финал) → **BASELINE_MUST_BE_LABELLED** после понижения | baseline-генератор кандидатов; `isLearned:false` |
| 3 | `SubPhaseRouterUnit` (`SubPhaseRoutingDecision`) | `21.AgentInfrastructure/Logical/SubPhaseRouterUnit.cs:21`; `KnowledgeEdge:29` | строит «решение ребра» (одиночный роутинг) + честный флаг неуверенности | частичный финал (одиночное ребро) | **BASELINE_MUST_BE_LABELLED** (роутинг) + **OK_INTERNAL_DETAIL** (`KnowledgeEdge`) | роутинг → baseline; `KnowledgeEdge` сохранить как honest-degraded |
| 4 | `OrchestrationScenarioPhasePlanner.PlanPhases` (планировщик фаз сценария; префикс `Orchestration`) | класс `26.TrainableRouter/Training/OrchestrationScenarioPhasePlanner.cs:5`; keyword-массив `PhaseRules` `:7-19`; `PlanPhases` `:21`; `text.Contains(keyword)` `:30`. Рантайм-вызовы: `OrchestrationTaskDecompositionFlow.cs:70`, `OrchestrationAcceptanceGateEvaluator.cs:39` | захардкоженный массив `(Phase, Keywords)[]`; фаза выбирается, если текст содержит любое ключевое слово (`text.Contains`) | финальный план фаз оркестрации (рантайм-авторитет) | **FORBIDDEN_RUNTIME_SHORTCUT** (пока остаётся финалом плана фаз) | понизить до baseline/eval-генератора кандидат-фаз (`isLearned=false`, как #1/#2; НЕ удалять — нужен для ablation); финальный план фаз/сценария → обучаемое L4/L5-решение |
| 5 | `RoutingProfileTrainer.Train` | `26.TrainableRouter/Training/RoutingProfileTrainer.cs:5,22` | строит keyword-профили (`Dictionary<string,HashSet<string>>:33`) из примеров | offline-построитель состояния (не рантайм-финал) | **OK_INTERNAL_DETAIL** (offline builder) → но обучает **детерминированный** scorer, не L5 | переориентировать: обучение авторитета — на L5 `ILearningMachine.TrainBatchAsync`; этот тренер остаётся для baseline-профилей |
| 6 | `KnnLogicalUnit` (`_index.Search`) | `21.AgentInfrastructure/Logical/KnnLogicalUnit.cs:218` | поиск k ближайших обученных примеров | поддержка решения (память) | **OK_MODEL_COMPONENT** | оставить как model-component ВНУТРИ `SemanticEdgeRouterUnit`; НЕ exact-prompt replay (тест leakage) |
| 7 | `InMemoryLogicalUnitRegistry.ResolveLatest` | `21.AgentInfrastructure/Logical/InMemoryLogicalUnitRegistry.cs:107`; `OrderByDescending(RegistrationOrder):118` | выбирает новейший доверенный юнит по роли/домену/типам | механизм cutover (инфраструктура) | **OK_ALLOWED_LEVEL** (latest-wins, A-LM-03) | оставить; гейт cutover §3.A Шаг E (two-PASS перед регистрацией latest) |
| 8 | `MultiSubPhaseRouterUnit.BuildPathFreeScoringText` / source-path warnings | `21.AgentInfrastructure/Logical/MultiSubPhaseRouterUnit.cs:291`; warning `:373` | намеренно исключает путь из скоринга; путь → только provenance-warning | анти-leakage хелпер | **OK_INTERNAL_DETAIL** (образец для повтора) | повторить инвариант в `EdgeAuthorityInput`; расширить запретом candidate-index |
| 9 | Confidence-floor (`decision.KnowledgeEdge` ветка) | `21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs:504`; флаг `SubPhaseRouterUnit.cs:29` | при низкой уверенности возвращает честный «knowledge edge», не выдуманный выбор | honest-degraded guard | **OK_INTERNAL_DETAIL** (антимаскарад) | сохранить; питать тем же `EdgeAuthorityDecision.KnowledgeEdge` |

Сводка вердиктов: FORBIDDEN (финал) — 1, 2, 4 (до понижения); BASELINE_MUST_BE_LABELLED —
2 (после), 3 (роутинг), 4 (после понижения); OK_INTERNAL_DETAIL — 3 (KnowledgeEdge), 5,
8, 9; OK_MODEL_COMPONENT — 6; OK_ALLOWED_LEVEL — 7. **Немаркированных финальных
авторитетов на РЕБРЕ после плана — 0** (Gate B R-07). Все 9 строк имеют `file:line`
(Gate B §10 п.6). Область механизма #4 (план фаз сценария на пути оркестрации L4)
включает не только keyword-ВЫБОР фаз (`PlanPhases`), но и детерминированный финальный
**ПОРЯДОК** фаз (`OrchestrationTaskDecompositionFlow.cs:18-30 PhaseOrder`, `:75
OrderBy(PhaseIndex)`, `:197 PhaseIndex`, `:148 BuildPhases`) — помечен **baseline-ordering**,
**не** финальная семантическая власть; диспозиция следует за #4 = **спроектирована (B-22)**: рекоменд. дефолт v1 =
`BASELINE_MUST_BE_LABELLED` (прецедент Q14) + инстанцированный обучаемый блюпринт замены в
§13 Q1; **RESOLVED(Luba 2026-06-30): НЕ активировать в v1** — остаётся
`BASELINE_MUST_BE_LABELLED`, активация обучаемого плана фаз = deferred-to-later-phase
(§3.0, §13 Q1, §8.2).
Это L4-путь оркестрации, поэтому он не входит в счёт «9 механизмов» отдельной строкой и не
нарушает канон `classified_mechanism_count == 9`.

### 3.C. Граница deterministic vs learned (после cutover)

| Подшаг ребра | Владелец СЕЙЧАС | Владелец ДОЛЖЕН СТАТЬ | Класс/метод | Вердикт |
|---|---|---|---|---|
| Генерация кандидатов | фикс-формула + словарь + keyword-scorer | **остаётся** детерминированным baseline | `IEdgeCandidateProvider` над `MultiSubPhaseRouterUnit.cs:283` + `KeywordUnitScorer.cs:38` | `BASELINE_MUST_BE_LABELLED` |
| Проверка порядка/позиции | guard уже есть в SmartSkills (`SkillSelectionLogicalHierarchy.cs:271`), на ребре — нет | **порт guard на ребро** (diagnostics, блокирует позицию) | `CandidateOrderLeakageGuardUnit` (ADAPTED-FROM-EXISTING) | `OK_INTERNAL_DETAIL` (guard) |
| K-NN-память похожих случаев | `KnnLogicalUnit._index.Search` `:218` | model-component внутри финала | `SemanticEdgeRouterUnit` (NEW) | `OK_MODEL_COMPONENT` |
| Финальный выбор/порядок/уверенность | `MultiSubPhaseRouterUnit.Score` `:283` (фикс-веса) | **обучаемый** финал | `SemanticEdgeRouterUnit.ExecuteAsync` (NEW) | `NEEDS_ILOGICALUNIT_WRAPPER` → trained |
| Подмена baseline→trained | (нет) | latest-wins после two-PASS | `ResolveLatest` `:107` (A-LM-03) | `OK_ALLOWED_LEVEL` |
| Обучение авторитета | детерминированный `RoutingProfileTrainer` `:22` | **L5** `ILearningMachine.TrainBatchAsync` `:212` | через `ILogicalCapabilityTrainer.TrainAndRegisterAsync` | `OK_ALLOWED_LEVEL` |
| Confidence-floor / return-loop | `AdaptiveLogicalOrchestrator.cs:504` | остаётся honest-degraded | `EdgeAuthorityDecision.KnowledgeEdge` | `OK_INTERNAL_DETAIL` |

### 3.D. Ablation-проход (доказательство, что финал держит обученный, не shortcut)

1. Зафиксировать **тот же** source-disjoint held-out набор, что в §10 п.2.
2. **Прогон A (полный):** `SemanticEdgeRouterUnit` (обучен) — финал. Замерить accuracy.
3. **Прогон B (ablation):** обучаемый авторитет **выключен**, финал считает только
   baseline-формула (`MultiSubPhaseRouterUnit.Score`, `:283`). Замерить accuracy.
4. **Условие не-shortcut:** accuracy(A) − accuracy(B) ≥ `ablation_threshold_locked`
   = **0.02** абсолютной accuracy (2 п.п., Решение Luba 2026-06-29, Q11; единый порог
   unit/edge/graph; ранее `blocked_not_configured`, теперь сконфигурирован). Если просадка
   меньше 0.02 → финал фиктивен, реально решает baseline shortcut →
   `FORBIDDEN_RUNTIME_SHORTCUT`, cutover откатить.
5. **Прогон C (permutation):** через `CandidateOrderLeakageGuardUnit` — 5 перестановок
   входа дают идентичный `OrderedTargets`. Иначе order-leakage (провал §10 п.1).

### 3.E. Цикл обучения и DPO (тот же канон, что блок 02; ссылка)

Сбор `EdgeTraversalTrace` → построение `GraphDpoPair(Level="edge")` с
`ExcludedLeakageSignals=["candidate_index","source_path","exact_prompt"]`,
`CandidateOrderInvariant=true`, `TeacherProvenance{20 полей; 13+5+2 RR6}`, `DpoTrainReady` →
маппинг в `LearningTrainingItem` (`00.Core/LearningMachineContracts.cs:30`) → батч
`LearningBatch<DynamicGraphDomain>` (`:18`) → `ILearningMachine.TrainBatchAsync` (`:212`)
→ `ILogicalCapabilityTrainer.TrainAndRegisterAsync` → новый `SemanticEdgeRouterUnit`
(`IsLearned=true`) → cutover latest-wins (`ResolveLatest`, `:107`). Метрики —
`LearningBatchResult` (`:43`), гейт — `LearningEvaluationResult.PassesGate` (`:100`).
Lifecycle: user/cache checkpoint первым → repo/common вторым → честный untrained если
нет ни одного → `ExportAsync`/`ImportAsync` (`:222/:227`) сохраняют provenance →
continue-training **от** checkpoint, не с нуля.

### 3.F. Graph-DPO Artifact Triad (требование Graph-DPO Artifact Triad Gate)

Ни одна DPO-строка cutover не train-ready, пока не существует триада:

```text
raw_comparison_event:
  graph_path_ids: [edge:<edgeId>, from=<nodeFrom>, to_candidates=[5.1,4.4,4.1]]
  prompt_or_request_id_if_allowed: req-<hash>  (только если разрешено провенансом)
  source_license_authority: <source_ref + license_ref>
  candidate_order_hash: <hash набора кандидатов БЕЗ позиции>  (perm-invariance)
  near_miss_reason: "rejected повернул на 4.1 по слову 'сервис', смысл = тест/баг"
legal_provenance_dpo_envelope:
  source: project-local trace | approved teacher
  license: Apache-2.0/MIT (teacher) | local (project)
  authority: human/Luba | legal-approved open-weight
  allowed_use_decision: train_common | train_personal | eval_only
  split_id: source-disjoint train|heldout
  terms_status: terms_snapshot_ref + provider_terms_checked
training_batch:
  batch_id: <id>
  graph_manifest_id: <semantic-edge-router manifest id>
  checkpoint_hash: <hash базового checkpoint, если continue-training>
  model_id: <teacher/judge model id, если применялся>
  sampling_params: <если кандидаты генерировались стохастически>
  seed: <seed>
  access_mode: hosted-open | self-hosted | project-local
```

---

## 4. UML-фрагмент компонента (Mermaid classDiagram)

```mermaid
classDiagram
    direction LR

    class ILogicalUnit~TInput,TOutput~ {
        <<interface, L2, EXISTING>>
        +ExecuteAsync(LogicalUnitRequest) LogicalUnitResult
    }
    note for ILogicalUnit~TInput,TOutput~ "СУЩЕСТВУЕТ 00.Core/LogicalContracts.cs:97. Финал ребра — обучаемая реализация. Чистый inference (Train НЕТ, A-LM-02). ПОЛНОСТЬЮ РЕАЛИЗОВАН как контракт."

    class SemanticEdgeRouterUnit {
        <<class, NEW, : ILogicalUnit>>
        +ExecuteAsync(LogicalUnitRequest~EdgeAuthorityInput~) EdgeAuthorityDecision
    }
    note for SemanticEdgeRouterUnit "NEW (СКЕЛЕТ, цель PASS_AS_TRAINED_FEATURE). ОБУЧАЕМЫЙ финальный авторитет ребра (= роль EdgeAuthorityUnit блока 02). Вход БЕЗ candidate-index/source-path. K-NN внутри как model-component. IsLearned=true."

    class CandidateOrderLeakageGuardUnit {
        <<class, NEW, : ILogicalUnit>>
        +ExecuteAsync(LogicalUnitRequest~CandidateOrderProbeInput~) CandidateOrderLeakageReport
    }
    note for CandidateOrderLeakageGuardUnit "ADAPTED-FROM-EXISTING. Аналогичный guard УЖЕ реализован: 43.SmartSkillsDomain/SkillSelectionLogicalHierarchy.cs:271 (UnitId candidate_order_leakage_guard :273, Role candidate_position_diagnostics_provenance_only :274, возвращает CandidateOrderLeakageReport). DIAGNOSTICS/GUARD, НЕ финал. Здесь — порт паттерна на ребро (CandidateOrderProbeInput -> CandidateOrderLeakageReport), не скелет с нуля."

    class IEdgeCandidateProvider {
        <<interface, NEW>>
        +Propose(EdgeTraversalInput) EdgeCandidate[]
    }
    note for IEdgeCandidateProvider "NEW (СКЕЛЕТ). ДЕТЕРМИНИРОВАННЫЙ baseline-генератор кандидатов (обёртка над понижёнными роутерами). authority_role=baseline_candidate_generator. НЕ финал."

    class MultiSubPhaseRouterUnit {
        <<class, EXISTING, baseline>>
        -Score() double
    }
    note for MultiSubPhaseRouterUnit "СУЩЕСТВУЕТ MultiSubPhaseRouterUnit.cs:8. Фикс-веса 0.55/0.25/0.20/0.02 (:283). FORBIDDEN как финал → ПОНИЖАЕТСЯ до baseline-генератора. НЕ удаляется (нужен для ablation)."

    class KeywordUnitScorer {
        <<class, EXISTING, baseline>>
        -Score() UnitScoringResult
    }
    note for KeywordUnitScorer "СУЩЕСТВУЕТ 26.TrainableRouter/Training/KeywordUnitScorer.cs:6. score=BaseScore+hits+boosts (:49). FORBIDDEN как финал → BASELINE_MUST_BE_LABELLED (isLearned:false)."

    class KnnLogicalUnit {
        <<class, EXISTING, model-component>>
        +ExecuteAsync() LogicalUnitResult
    }
    note for KnnLogicalUnit "СУЩЕСТВУЕТ 21.AgentInfrastructure/Logical/KnnLogicalUnit.cs. _index.Search(:218). OK_MODEL_COMPONENT внутри SemanticEdgeRouterUnit. НЕ exact-prompt replay."

    class InMemoryLogicalUnitRegistry {
        <<class, EXISTING>>
        +ResolveLatest() ILogicalUnit
    }
    note for InMemoryLogicalUnitRegistry "СУЩЕСТВУЕТ InMemoryLogicalUnitRegistry.cs:30. ResolveLatest (:107) сортирует по RegistrationOrder (:118). OK_ALLOWED_LEVEL (latest-wins cutover, A-LM-03). Гейт: two-PASS перед регистрацией latest."

    class ILearningMachine~DynamicGraphDomain~ {
        <<interface, L5, EXISTING>>
        +TrainBatchAsync(LearningBatch) LearningBatchResult
    }
    note for ILearningMachine~DynamicGraphDomain~ "СУЩЕСТВУЕТ LearningMachineContracts.cs:208. ЕДИНСТВЕННЫЙ владелец Train* (A-LM-02). Учит SemanticEdgeRouterUnit. Контракт РЕАЛИЗОВАН; DynamicGraphDomain — СКЕЛЕТ."

    ILogicalUnit~TInput,TOutput~ <|.. SemanticEdgeRouterUnit : implements
    ILogicalUnit~TInput,TOutput~ <|.. CandidateOrderLeakageGuardUnit : implements
    IEdgeCandidateProvider ..> MultiSubPhaseRouterUnit : wraps as baseline (понижен)
    IEdgeCandidateProvider ..> KeywordUnitScorer : wraps as baseline (понижен)
    SemanticEdgeRouterUnit o-- KnnLogicalUnit : K-NN model-component (internal)
    CandidateOrderLeakageGuardUnit ..> SemanticEdgeRouterUnit : probes (diagnostics, не финал)
    InMemoryLogicalUnitRegistry ..> SemanticEdgeRouterUnit : ResolveLatest -> trained (после two-PASS)
    InMemoryLogicalUnitRegistry ..> MultiSubPhaseRouterUnit : baseline остаётся для ablation
    ILearningMachine~DynamicGraphDomain~ ..> SemanticEdgeRouterUnit : TrainBatchAsync -> publish (latest-wins)
```

### 4.1. Легенда

| Класс | Что делает | Статус |
|---|---|---|
| `ILogicalUnit<TInput,TOutput>` | L2-контракт юнита, реализуется финалом ребра | EXISTING (контракт) |
| `SemanticEdgeRouterUnit` | ОБУЧАЕМЫЙ финал ребра (= роль EdgeAuthorityUnit) | NEW (СКЕЛЕТ → цель trained) |
| `CandidateOrderLeakageGuardUnit` | permutation-guard, diagnostics, НЕ финал | ADAPTED-FROM-EXISTING (`SkillSelectionLogicalHierarchy.cs:271`) → порт на ребро |
| `IEdgeCandidateProvider` | детерминированный baseline-генератор кандидатов | NEW (СКЕЛЕТ) |
| `MultiSubPhaseRouterUnit` | сегодняшний фикс-формульный финал → понижен в baseline | EXISTING → baseline |
| `KeywordUnitScorer` | keyword-scorer → понижен в baseline | EXISTING → baseline |
| `KnnLogicalUnit` | K-NN-память (model-component внутри финала) | EXISTING (model-component) |
| `InMemoryLogicalUnitRegistry` | latest-wins резолвинг (механизм cutover, A-LM-03) | EXISTING |
| `ILearningMachine<DynamicGraphDomain>` | L5, единственный владелец Train*, учит финал | EXISTING (контракт) |

Обозначения связей: `<|..` implements · `o--` композиция-владение · `-->` поток вызова
· `..>` зависимость.

### 4.2. Таблица симуляции (на каждую связь/ветку: пример входа → ожидаемый результат)

| Связь / ветка | Пример входа | Ожидаемый результат |
|---|---|---|
| `IEdgeCandidateProvider ..> MultiSubPhaseRouterUnit` (генерация, понижен) | «исправь падающий unit-тест в backend-сервисе оплаты» | кандидаты {5.1, 4.4, 4.1} **без порядка**, `BaselineScore` заполнен, `authority_role=baseline_candidate_generator` |
| `IEdgeCandidateProvider ..> KeywordUnitScorer` (генерация, понижен) | тот же | вклад в кандидаты через `hits/boosts` (`:49`); `isLearned:false`, не финал |
| `CandidateOrderLeakageGuardUnit ..> SemanticEdgeRouterUnit` (guard, инвариантность) | те же кандидаты в порядке [4.1,5.1,4.4] **и** [4.4,5.1,4.1] | `CandidateOrderLeakageReport{PermutationInvariant=true}`; **тот же** выбор; позиционная фича блокирована |
| guard — ветка УТЕЧКИ (negative) | искусственно: выбор зависит от позиции | `PermutationInvariant=false` → guard **блокирует** фичу, тест FAIL ожидаемо |
| анти-leakage вход (нормализация) | тот же + `SourcePath=/payments/x.cs` | `EdgeAuthorityInput` БЕЗ позиции и БЕЗ пути; путь → warning `source_path_used_for_scoring=false` (`:373`) |
| `ResolveLatest ..> SemanticEdgeRouterUnit` (cutover, обучен) | реестр содержит baseline + обученный (two-PASS пройден) | `ResolveLatest` (`:107`) отдаёт обученный (больший `RegistrationOrder`, `:118`); финал — `trained_edge_unit` |
| `SemanticEdgeRouterUnit o-- KnnLogicalUnit` (K-NN model-component) | нормализованный вход | k ближайших обученных примеров (`_index.Search`, `:218`) поддерживают решение; НЕ точный реплей промпта |
| финал (смысл, не слово) | нормализованный вход выше | `OrderedTargets=[(5.1,0.86),(4.4,0.74)]` (смысл «тест/баг», не слово «сервис») |
| confidence-floor (честность) | «сделай что-нибудь хорошее» (нет сигнала) | `KnowledgeEdge=true`, выбор не выдуман (`AdaptiveLogicalOrchestrator.cs:504`) |
| `ResolveLatest ..> MultiSubPhaseRouterUnit` (baseline для ablation) | held-out набор, обучаемый авторитет ВЫКЛЮЧЕН | финал считает только формула `:283` → accuracy **просаживается** ≥ порога (доказывает не-shortcut) |
| `ILearningMachine ..> SemanticEdgeRouterUnit` (обучение) | батч из 100 `GraphDpoPair(Level=edge)` | `LearningBatchResult{AccuracyAfter>Before, RewardMargin>0, без UNKNOWN}`; trained-юнит публикуется (latest-wins) |

---

## 5. Контракты/типы (предлагаемые сигнатуры; NEW vs существующее)

Существующее (на это опираемся, не меняем сигнатуры; все проверены чтением файла —
см. §6/§12):
- `interface ILogicalUnit<TInput,TOutput>` — `00.Core/LogicalContracts.cs:97`.
- `record SubPhaseRoutingDecision` — `SubPhaseRouterUnit.cs:21` (поле `KnowledgeEdge:29`).
- `class MultiSubPhaseRouterUnit` — `MultiSubPhaseRouterUnit.cs:8` (`Score:283`, словарь `:23`, path-free `:291`, warning `:373`, HybridRanker `:361`).
- `class KeywordUnitScorer : IUnitScorer` — `26.TrainableRouter/Training/KeywordUnitScorer.cs:6` (`Score:38`, формула `:49`, `OrderByDescending:31`).
- `class RoutingProfileTrainer` — `26.TrainableRouter/Training/RoutingProfileTrainer.cs:5` (`Train:22`, keyword-map `:33`).
- `class KnnLogicalUnit` — `KnnLogicalUnit.cs` (`_index.Search:218`).
- `class InMemoryLogicalUnitRegistry` — `InMemoryLogicalUnitRegistry.cs:30` (`ResolveLatest:107`, `OrderByDescending(RegistrationOrder):118`, дескриптор `IsLearned:21`).
- `interface ILearningMachine<TDomain>` — `LearningMachineContracts.cs:208` (Train:212 / Export:222 / Import:227); DPO-формат `LearningTrainingItem:30`; метрики `LearningBatchResult:43`, `PassesGate:100`.
- Прецедент permutation-guard — `43.SmartSkillsDomain/SkillSelectionContracts.cs:35` (`PermutationSelectionInvariant`), `:45` (`CandidateOrderPermutationProbe`); `SkillSelectionLogicalHierarchy.cs:150` (`candidate_position_scaffold_final_authority=false`).

NEW (проектируется здесь; в коде НЕТ — пометка NEW):

```csharp
// NEW. Вход обучаемого финала ребра: ФИЗИЧЕСКИ без leakage-сигналов.
// (Тот же контракт, что в блоке 02 §5; повторён для самодостаточности блока.)
public sealed record EdgeAuthorityInput(
    string EdgeId,
    string[] SemanticTokens,                 // смысл заявки, НЕ путь
    string[] PredicateIds,
    ContextSignals Context,                   // EXISTING тип (контекст-ридер)
    IReadOnlyList<EdgeCandidate> Candidates,  // порядок несущественен (perm-invariant)
    string[] ExcludedLeakageSignals);         // ["candidate_index","source_path","exact_prompt"]

// NEW. Решение обучаемого финала ребра.
public sealed record EdgeAuthorityDecision(
    IReadOnlyList<(string TargetSubPhaseId, double Confidence)> OrderedTargets,
    int FanOutCount,
    bool KnowledgeEdge,                        // честный "не знаю" при пробитии floor
    string AuthoritySource,                   // = "trained_edge_unit"
    IReadOnlyList<string> AblationBaselineRank);

// NEW. ОБУЧАЕМЫЙ финальный авторитет ребра (учится на L5).
// Каноническая роль edge-authority (= EdgeAuthorityUnit блока 02). Имя класса для
// cutover-блока: SemanticEdgeRouterUnit (мастер §A; доска §Ф8). Это одна роль, не два юнита.
public sealed class SemanticEdgeRouterUnit : ILogicalUnit<EdgeAuthorityInput, EdgeAuthorityDecision>
{
    public const string Id = "unit-semantic-edge-router-v1";
    public const string Role = "edge-authority";          // та же роль, что EdgeAuthorityUnit
    public Task<OperationResult<LogicalUnitResult<EdgeAuthorityDecision>>> ExecuteAsync(
        LogicalUnitRequest<EdgeAuthorityInput> request, CancellationToken ct = default);
}

// NEW. Типизированное обучаемое состояние финала (НЕ сырой вложенный словарь).
public sealed record SemanticEdgeRoutingState(
    IReadOnlyDictionary<string, int> SemanticVocabulary,   // токен смысла -> id
    IReadOnlyDictionary<string, int> PredicateVocabulary,  // предикат -> id
    IReadOnlyDictionary<string, int> TargetVocabulary,     // целевая суб-фаза -> id
    float[] LearnedWeights,                                 // веса DPO
    float[] Calibration,                                    // калибровка уверенности
    KnnMemoryRef KnnMemory,                                 // ссылка на обученный индекс
    TeacherProvenance Provenance);                          // 20 полей; 13+5+2 RR6 (см. ниже)

// NEW. Вход guard-пробы порядка кандидатов.
public sealed record CandidateOrderProbeInput(
    string EdgeId,
    EdgeAuthorityInput BaseInput,
    int PermutationCount);                                  // напр. 5

// ADAPTED-FROM-EXISTING. Отчёт guard'а (diagnostics; НЕ финальный селектор).
// Тип-имя CandidateOrderLeakageReport УЖЕ существует в проекте (возвращается guard'ом
// 43.SmartSkillsDomain/SkillSelectionLogicalHierarchy.cs:271); здесь — edge-scoped
// форма с тем же смыслом. При желании переиспользовать общий тип — пометить shared.
public sealed record CandidateOrderLeakageReport(
    bool PermutationInvariant,
    int PermutationCount,
    IReadOnlyList<string> BlockedFeatures,                  // напр. ["candidate_index"]
    IReadOnlyList<CandidateOrderPermutationProbe> Probes);  // EXISTING тип прецедента

// ADAPTED-FROM-EXISTING. Guard-юнит: ловит order-leakage, блокирует позиционные фичи,
// НЕ выбирает. Аналог УЖЕ реализован в основном дереве:
//   43.SmartSkillsDomain/SkillSelectionLogicalHierarchy.cs:271
//   class CandidateOrderLeakageGuardUnit : ILogicalUnit<SkillSelectionRequest, CandidateOrderLeakageReport>
//   UnitId="candidate_order_leakage_guard" (:273),
//   Role="candidate_position_diagnostics_provenance_only" (:274).
// Здесь — порт того же паттерна на ребро (вход CandidateOrderProbeInput вместо
// SkillSelectionRequest), а НЕ скелет с нуля. Если нужен отдельный edge-scoped тип —
// он ADAPTED-FROM-EXISTING с источником выше.
public sealed class CandidateOrderLeakageGuardUnit
    : ILogicalUnit<CandidateOrderProbeInput, CandidateOrderLeakageReport>
{
    public const string Id = "unit-candidate-order-leakage-guard-v1";
    public const string Role = "edge-leakage-guard";       // diagnostics, не edge-authority
    public Task<OperationResult<LogicalUnitResult<CandidateOrderLeakageReport>>> ExecuteAsync(
        LogicalUnitRequest<CandidateOrderProbeInput> request, CancellationToken ct = default);
}

// NEW. Детерминированный baseline-генератор кандидатов (обёртка над понижёнными роутерами).
public interface IEdgeCandidateProvider
{
    OperationResult<EdgeCandidate[]> Propose(EdgeTraversalInput input);   // EdgeTraversalInput/EdgeCandidate — блок 02 §5
}

// NEW. Канон DPO-пары (общий для блоков unit/edge/graph; утвердить у Luba — §13).
public sealed record GraphDpoPair(
    string PairId,
    string Level,                  // "edge" для этого блока
    string NodeOrEdgeId,
    string InputContext,
    string Chosen,
    string Rejected,
    string[] WrongMarkers,         // ["[WRONG: ...]"]
    string Reason,
    TeacherProvenance TeacherProvenance,
    string[] SourceIds,
    bool HeldOut,
    bool CandidateOrderInvariant,
    string[] ExcludedLeakageSignals,
    bool DpoTrainReady);

// NEW. Провенанс учителя (в коде/данных СЕЙЧАС НЕТ — построить).
// КАНОН-набор = 20 полей (13 базовых + 5 NEW + 2 RR6); JudgeModel — ВНУТРИ этой записи (владелец канона — блок 08).
public sealed record TeacherProvenance(
    // 13 базовых:
    string TeacherAccessMode,
    string HostingProvider,
    string ModelId,
    string ModelLicense,
    string ModelVersion,
    string RequestId,
    bool   ProviderTermsChecked,
    bool   TeacherOutputUsedForTraining,
    bool   ClosedProprietaryModel,   // ДОЛЖНО быть false для дистилляции
    string SourceRef,
    string SourceSha256,
    string[] ContaminationMarkers,
    string JudgeModel,               // судья ВНУТРИ провенанса; не цель обучения
    // 5 NEW:
    string TermsSnapshotRef,
    string GeographyExportConstraint,
    string SourceCategory,
    bool   RawPrivateTextCopied,
    string RedactionPolicyRef,
    // 2 RR6 (service-mode ban / grounding), владелец — блок 08 §3.1:
    string ServiceMode,              // RR6: "plain"|"grounding"|"web_search"|"retrieval_tool"; обязан быть "plain" для train/judge
    bool   GroundingUsed);           // RR6: был ли подмешан сторонний веб-контент (источник — transport-seam)
// Предусловие DpoTrainReady=true: provenance_field_presence_rate == 1.0 по всем 20 полям (RR6 ServiceMode/GroundingUsed в знаменателе).
// Строка с пустыми RR6-полями ⇒ presence_rate<1.0 ⇒ НЕ DpoTrainReady (R4: пустые RR6 не пропускаются в обучение).
```

### 5.3. Dictionary-to-ILogicalUnit матрица замены (FC-30 anti-template, заполнено)

> Требование FC-30: на каждый словарь/счётчик/формулу-авторитет — одна строка с
> конкретной заменой + ЗАПОЛНЕННЫЕ DPO-примеры. Ниже — для главного FORBIDDEN-механизма
> (фикс-формула финала) и для guard.

```text
old code quote: MultiSubPhaseRouterUnit.cs:283 — score = predicateScore*0.55 + exactSubPhaseMention*0.25 + tokenScore*0.20 + hybridScore*0.02
current runtime calculation: линейная сумма с фикс-весами выбирает победителя fan-out
  как ФИНАЛ; KeywordUnitScorer.cs:49 даёт аналогичный keyword-ранг.
why it is weak/leaky: фикс-веса не учат смысл; уязвимы к случайному совпадению слова
  («сервис»); риск, что добавят candidate-index/SourcePath как фичу.
replacement allowed level: ILogicalUnit (L2), обучается на ILearningMachine (L5).
replacement ILogicalUnit: SemanticEdgeRouterUnit.
input contract: EdgeAuthorityInput (SemanticTokens, PredicateIds, Context, Candidates
  без позиции, ExcludedLeakageSignals).
output contract: EdgeAuthorityDecision (OrderedTargets[(id,confidence)], FanOutCount,
  KnowledgeEdge, AuthoritySource, AblationBaselineRank).
typed trained state: SemanticEdgeRoutingState { SemanticVocabulary, PredicateVocabulary,
  TargetVocabulary, LearnedWeights, Calibration, KnnMemory, Provenance } — именованная
  typed-запись, НЕ сырой вложенный словарь.
training update algorithm:
  input example: GraphDpoPair(Level=edge, Chosen=[5.1,4.4], Rejected=[4.1 WRONG],
    InputContext="исправь падающий unit-тест в backend-сервисе оплаты").
  state mutation: сдвинуть LearnedWeights в сторону Chosen vs Rejected (DPO reward
    margin); добавить нормализованный пример в KnnMemory с целевой меткой.
  checkpoint save: ExportAsync -> checkpoint + manifest (SemanticEdgeRouterUnit v1).
KNN/nearest methods:
  signature: IReadOnlyList<Neighbor> Search(float[] query, int k)  (прецедент
    KnnLogicalUnit.cs:218 _index.Search).
  role: model component (поддержка решения ближайшими примерами), НЕ exact-prompt replay.
positive DPO row:
  source: project-local trace + human/Luba label.
  input: "исправь падающий unit-тест в backend-сервисе оплаты"
  chosen: переход на [5.1 (тест/баг), 4.4]
  rejected: переход на [4.1] [WRONG: повернул по слову «сервис», смысл = тест/баг]
  teaches: смысл «тест/баг» важнее случайного слова «сервис».
rejected DPO row:
  wrong signal: выбор по позиции «4.1 первый в списке».
  why chosen is better: chosen стабилен при перестановке кандидатов (perm-invariant).
quarantine row:
  leakage reason: chosen предсказуем из candidate_index или из SourcePath.
  rebuild action: QUARANTINE_DO_NOT_TRAIN; пересобрать с perm-парами и без пути.
affected sets: edge-authority train + held-out (source-disjoint).
tests:
  exact test name: SemanticEdgeRouterUnit_PermutationInvariance.
  assertion: OrderedTargets идентичны для 5 перестановок одного входа.
  exact test name: SemanticEdgeRouterUnit_NoSourcePathAuthority.
  assertion: изменение SourcePath не меняет OrderedTargets; warning присутствует.
  exact test name: SemanticEdgeRouterUnit_AblationDrop.
  assertion: выключение обученного авторитета роняет accuracy ≥ порога.
load/cache/repo lifecycle: user/cache checkpoint -> repo/common fallback -> honest
  untrained если нет ни одного.
backup/export/import/share lifecycle: ExportAsync/ImportAsync сохраняют
  TeacherProvenance (20 полей; 13+5+2 RR6) и typed state.
continue-training lifecycle: загрузить существующий checkpoint, затем TrainBatchAsync
  (не с нуля).
source/license examples:
  candidate source: project-local trace (local) для rejected/baseline; teacher
    Qwen3 (Apache-2.0) / DeepSeek (MIT) для chosen — после проверки license-файла.
  license file/ref to verify: LICENSE Apache-2.0 / MIT в репозитории модели.
  common vs user-adaptive leg: common = общие смысловые пары; user-adaptive =
    приватные пользовательские трассы (train_personal).
old method replaced: MultiSubPhaseRouterUnit.Score / KeywordUnitScorer.Score как ФИНАЛ
  (оба остаются baseline-генераторами кандидатов).
```

```text
old code quote: (нет явного класса) — позиционная утечка «выбор по индексу кандидата»
current runtime calculation: риск, что финал коррелирует с позицией кандидата в списке.
why it is weak/leaky: порядок кандидатов — не смысл задачи; это order leakage.
replacement allowed level: ILogicalUnit (L2), diagnostics/guard (НЕ финал).
replacement ILogicalUnit: CandidateOrderLeakageGuardUnit.
input contract: CandidateOrderProbeInput (BaseInput, PermutationCount).
output contract: CandidateOrderLeakageReport (PermutationInvariant, BlockedFeatures, Probes).
typed trained state: (нет обучения) — guard детерминированный по построению; не учится,
  не является финалом. OK_INTERNAL_DETAIL.
training update algorithm: N/A (guard не обучается).
KNN/nearest methods: N/A.
positive DPO row: N/A (guard не строит DPO; он валидирует пары на perm-инвариантность).
rejected DPO row: N/A.
quarantine row: пара, где chosen меняется при перестановке → QUARANTINE_DO_NOT_TRAIN.
affected sets: все edge-DPO наборы фильтруются guard'ом перед train.
tests:
  exact test name: CandidateOrderLeakageGuardUnit_BlocksPositionFeature.
  assertion: при искусственной позиционной фиче PermutationInvariant=false и фича блокирована.
load/cache/repo lifecycle: N/A (детерминированный guard).
old method replaced: неявная позиционная уязвимость → явный страж.
```

---

## 6. Цитаты кода «как сейчас» (file:line) + сейчас / почему / должно стать / как менять

> Все цитаты ниже проверены чтением файла в `C:/Projects/llm_mvp_core` (см. §12).

### 6.1. Фикс-формула финала выбора (главный FORBIDDEN, механизм #1)

`21.AgentInfrastructure/Logical/MultiSubPhaseRouterUnit.cs:283-284`:
```csharp
var score = contextSignals.IsEmpty
    ? ((predicateScore * 0.55) + (exactSubPhaseMention * 0.25) + (tokenScore * 0.20) + (hybridScore * 0.02))
    : ((predicateScore * 0.50) + (exactSubPhaseMention * 0.20) + (tokenScore * 0.15) + (hybridScore * 0.05) + (contextScore * 0.10));
```
- **Сейчас:** финальный балл кандидата на развилке fan-out — ручная линейная формула.
- **Почему плохо:** калиброванный честный baseline ОК, пока нет claim «умный выбор».
  Плохо станет, если объявить семантическим/обученным финалом (A-LM-09: скрытый
  детерминизм как власть). Именно это блок 12 запрещает на ребре.
- **Должно стать:** финал считает обучаемый `SemanticEdgeRouterUnit` (NEW). Формула
  становится `BaselineScore` внутри `IEdgeCandidateProvider` и участвует только в ablation.
- **Как менять:** обернуть в `IEdgeCandidateProvider.Propose`
  (`authority_role="baseline_candidate_generator"`); финал — из
  `EdgeAuthorityDecision.OrderedTargets`. Не удалять формулу — пометить baseline.

### 6.2. Keyword-scorer как финальный ранг (механизм #2)

`26.TrainableRouter/Training/KeywordUnitScorer.cs:49`:
```csharp
var score = profile.BaseScore + hits + domainBoost + directionBoost;
```
(сортировка финала — `OrderByDescending(result => result.Score)`, `:31`.)
- **Сейчас:** ранг юнита = база + совпадения ключевых слов + бусты; `OrderByDescending`
  даёт «победителя».
- **Почему плохо:** keyword-matching как ФИНАЛ — детерминированный shortcut (A-LM-09).
- **Должно стать:** генератор кандидатов внутри `IEdgeCandidateProvider`,
  `isLearned:false`; финал — обучаемый юнит.
- **Как менять:** оставить класс, перенаправить его выход в кандидаты; в обучаемый
  `EdgeAuthorityInput` keyword-веса как фичу **не** прокидывать.

### 6.3. Permutation-guard прецедент (НЕ менять — переиспользовать как канон, механизм-страж)

Сам guard-КЛАСС уже реализован в основном дереве —
`43.SmartSkillsDomain/SkillSelectionLogicalHierarchy.cs:271`:
```csharp
public sealed class CandidateOrderLeakageGuardUnit : ILogicalUnit<SkillSelectionRequest, CandidateOrderLeakageReport>
{
    public const string UnitId = "candidate_order_leakage_guard";                 // :273
    public const string Role = "candidate_position_diagnostics_provenance_only";  // :274
```
и его контракты/инвариант — `43.SmartSkillsDomain/SkillSelectionContracts.cs:35,45`,
`SkillSelectionLogicalHierarchy.cs:150`:
```csharp
bool PermutationSelectionInvariant,                 // SkillSelectionContracts.cs:35
public sealed record CandidateOrderPermutationProbe( // SkillSelectionContracts.cs:45
["candidate_position_scaffold_final_authority"] = bool.FalseString,  // SkillSelectionLogicalHierarchy.cs:150
```
- **Сейчас:** в SmartSkills **уже есть готовый guard-юнит** (`:271`, Role
  `candidate_position_diagnostics_provenance_only`, возвращает `CandidateOrderLeakageReport`)
  + честный инвариант «позиция кандидата НЕ финальная власть» + проба перестановки. Это
  ровно тот R13-инвариант, который нужен на ребре.
- **Почему ок:** это правильный, уже работающий образец анти-order-leakage. Не
  изобретаем — переиспользуем.
- **Должно стать:** `CandidateOrderLeakageGuardUnit` для ребра — **ADAPTED-FROM-EXISTING**
  (порт паттерна `:271` на ребро): вход `CandidateOrderProbeInput` вместо
  `SkillSelectionRequest`, тот же инвариант — перестановка кандидатов не меняет
  `OrderedTargets`.
- **Как менять:** не ломать SmartSkills; переиспользовать тип
  `CandidateOrderPermutationProbe` для отчёта guard'а.

### 6.4. Latest-wins реестр (механизм cutover #7, НЕ менять — добавить гейт)

`21.AgentInfrastructure/Logical/InMemoryLogicalUnitRegistry.cs:107,118`:
```csharp
public OperationResult<ILogicalUnit<TInput, TOutput>> ResolveLatest<TInput, TOutput>(string unitRole, string domainTag, LogicalUnitTrustState minimumTrustState = LogicalUnitTrustState.Probationary)
// ...
    .OrderByDescending(candidate => candidate.Descriptor.RegistrationOrder)
```
- **Сейчас:** реестр отдаёт новейший доверенный юнит по роли/домену/типам (latest-wins).
- **Почему ок:** это легальный механизм cutover (A-LM-03), OK_ALLOWED_LEVEL.
- **Должно стать:** cutover (регистрация обученного как latest) РАЗРЕШЁН только после
  two-PASS блока 11 (held-out + ablation + lifecycle + no-UNKNOWN).
- **Как менять:** не менять `ResolveLatest`; добавить **предусловие регистрации**
  (гейт §3.A Шаг E) — обучаемый юнит получает больший `RegistrationOrder` лишь по PASS.

### 6.5. K-NN как model-component (механизм #6, НЕ менять — пометить роль)

`21.AgentInfrastructure/Logical/KnnLogicalUnit.cs:218`:
```csharp
var hits = _index.Search(query, _k);
```
- **Сейчас:** поиск k ближайших обученных примеров.
- **Почему ок:** KNN/retrieval — допустимый model-component ВНУТРИ уровня (skill: «KNN
  audit-triggered, not forbidden»).
- **Должно стать:** model-component внутри `SemanticEdgeRouterUnit`; не финал сам по себе,
  не exact-prompt replay.
- **Как менять:** ничего не ломать; тест leakage (eval-промпт=train-промпт → fail) и
  отчёт exact-memory hit count отдельно от generalization.

### 6.6. Честный confidence-floor (механизм #9, НЕ менять — сохранить)

`21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs:504`:
```csharp
if (decision.KnowledgeEdge)
```
- **Сейчас:** при низкой уверенности — честный «knowledge edge», не выдуманный выбор.
- **Почему ок:** антимаскарад (honest-degraded), OK_INTERNAL_DETAIL.
- **Должно стать:** сохранить; `EdgeAuthorityDecision.KnowledgeEdge` питает этот же путь.
- **Как менять:** ничего не ломать; пробросить новый флаг в существующую ветку.

### 6.7. Path-free нормализация (механизм #8, НЕ менять — повторить инвариант)

`21.AgentInfrastructure/Logical/MultiSubPhaseRouterUnit.cs:373`:
```csharp
var warnings = new List<string> { "source_path_used_for_scoring=false" };
```
(метод `BuildPathFreeScoringText`, `:291`, исключает путь из скоринга.)
- **Сейчас:** путь источника НЕ участвует в скоринге; держится как warning.
- **Почему ок:** правильная защита от path-leakage. Образец для повтора.
- **Должно стать:** тот же инвариант обязателен для `EdgeAuthorityInput` + запрет
  candidate-index (permutation-invariance).
- **Как менять:** перенести инвариант в построение `EdgeAuthorityInput`; добавить
  `ExcludedLeakageSignals=["candidate_index","source_path","exact_prompt"]`.

---

## 7. Статус (что работает, что заглушка) — честно

| Элемент | Статус | Двух-PASS |
|---|---|---|
| Фикс-формула финала (`MultiSubPhaseRouterUnit.Score:283`) | РАБОТАЕТ, честно `isLearned:false` | `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` (как baseline) |
| Keyword-scorer (`KeywordUnitScorer.cs:49`) | РАБОТАЕТ как детерминированный scorer | baseline (после понижения) |
| Latest-wins реестр (`ResolveLatest:107`) | РАБОТАЕТ (A-LM-03) | n/a (инфраструктура) |
| K-NN-память (`_index.Search:218`) | РАБОТАЕТ как model-component | n/a (компонент) |
| Path-free нормализация / warning (`:291/:373`) | РАБОТАЕТ как образец | — |
| Confidence-floor честность (`:504`) | РАБОТАЕТ (honest-degraded) | honest-degraded |
| Permutation-guard прецедент (`SkillSelectionContracts.cs:35,45`) | РАБОТАЕТ в SmartSkills | — |
| `SemanticEdgeRouterUnit` (обучаемый финал) | ОТСУТСТВУЕТ (NEW, СКЕЛЕТ) | целевой `PASS_AS_TRAINED_FEATURE` |
| `CandidateOrderLeakageGuardUnit` (страж) | АНАЛОГ УЖЕ СУЩЕСТВУЕТ (`SkillSelectionLogicalHierarchy.cs:271`, UnitId `candidate_order_leakage_guard`, Role `candidate_position_diagnostics_provenance_only`); edge-версия — ADAPTED-FROM-EXISTING (порт паттерна на ребро) | — |
| `IEdgeCandidateProvider` (обёртка baseline) | ОТСУТСТВУЕТ (NEW, СКЕЛЕТ) | — |
| `SemanticEdgeRoutingState` / `GraphDpoPair` / `TeacherProvenance` (20; 13+5+2 RR6) | ОТСУТСТВУЮТ (NEW) | — |
| Cutover-гейт / ablation-прогон | ОТСУТСТВУЕТ (PLAN-ONLY) | — |
| `OrchestrationScenarioPhasePlanner.PlanPhases` (механизм #4, `OrchestrationScenarioPhasePlanner.cs:5/:21/:30`) | РАБОТАЕТ как детерминированный keyword-планировщик фаз; вызывается в рантайме (`OrchestrationTaskDecompositionFlow.cs:70`, `OrchestrationAcceptanceGateEvaluator.cs:39`) | **РЕКОМ. ДЕФОЛТ v1 = `BASELINE_MUST_BE_LABELLED`** (honest deterministic baseline без claim «обученный план фаз», `isLearned=false`, `phase_plan_semantic_quality_claimed=false`), по прецеденту Q14. **Диспозиция #4 теперь СПРОЕКТИРОВАНА (B-22, не DEFERRED):** инстанцированный обучаемый путь `SemanticPhasePlanUnit` + `PhasePlanState` + DPO training-update + ablation `≥0.02` + forbidden-leakage готов в §13 Q1 на случай активации. Вердикт `FORBIDDEN_RUNTIME_SHORTCUT` применяется ТОЛЬКО если объявить текущий детерминированный финал «обученным» без cutover. **RESOLVED(Luba 2026-06-30): НЕ активировать в v1** — план фаз остаётся `BASELINE_MUST_BE_LABELLED`, обучаемая замена = deferred-to-later-phase. Baseline НЕ удаляется (нужен для ablation); `classified_mechanism_count == 9` сохранён |

Главный честный вывод: сегодня финал на ребре считает **детерминированная формула,
честно помеченная baseline**; **обучаемого финала ребра нет, cutover не сделан**. Это
`PASS_AS_HONEST_UNTRAINED_SCAFFOLD`, не `PASS_AS_TRAINED_FEATURE`. Репортить «у нас
обученный no-determinism выбор» сейчас = masquerade.

---

## 8. Гейты

### 8.1. Легальный teacher (Legal Teacher / Provenance Gate)
Перед тем как строка станет `Chosen/Rejected`, заполнить `TeacherProvenance` (NEW, 20
полей; 13+5+2 RR6; `provenance_field_presence_rate == 1.0` по 20; пустые RR6
`service_mode`/`grounding_used` ⇒ presence_rate<1.0 ⇒ НЕ train-ready, R4). Разрешены только open-weight/permissive
через нейтральных провайдеров (Qwen3 Apache-2.0 / DeepSeek MIT / Kimi K2 / GLM /
Mistral). Если `ClosedProprietaryModel=true` И `TeacherOutputUsedForTraining=true` →
`DATASET_REBUILD_REQUIRED`. Claude/OpenAI/Gemini как teacher — НИКОГДА. Контаминированные
строки → quarantine. Судья (`JudgeModel`, внутри провенанса) может быть сильнее, но не
цель обучения.

### 8.2. Запрет скрытого детерминизма (A-LM-09 / A-LM-13 / R4) — ядро блока
Финал на ребре — обучаемый `SemanticEdgeRouterUnit` (`DeterministicFinalAuthority=false`).
Все 9 механизмов классифицированы (§3.B); `classified_mechanism_count == 9`;
немаркированных финальных авторитетов **на РЕБРЕ** после плана — **0**. Удаление формулы
≠ покрытие: план называет trainable unit + typed state + checkpoint/manifest +
export/import + locator + fresh-load + continue-training + ablation (§5.3 + §10).

**Граница блока 12 по #4 (B-22 — диспозиция спроектирована, активация = Luba):** механизм
#4 (`OrchestrationScenarioPhasePlanner.PlanPhases`, план фаз сценария) — это **отдельный
L4-авторитет на пути оркестрации**, а не авторитет ребра. После шага B-22 его диспозиция
**больше не DEFERRED**: применён прецедент Q14 (детерминированный финал-порядок) —
рекомендуемый дефолт v1 = **`BASELINE_MUST_BE_LABELLED`** (honest deterministic baseline
без claim «обученный план фаз»), а §13 Q1 содержит **инстанцированный** обучаемый блюпринт
замены (`SemanticPhasePlanUnit` + `PhasePlanState` + DPO training-update + ablation `>=0.02`
+ forbidden-leakage), покрывающий ОБА финала (и выбор `PlanPhases`, и порядок
`PhaseOrder/PhaseIndex/BuildPhases`). Поэтому: (1) утверждение «немаркированных финальных
авторитетов — 0» теперь верно и для ребра, и для L4-пути #4 (#4 ПОМЕЧЕН baseline-labelled,
не оставлен немаркированным); (2) вердикт `FORBIDDEN_RUNTIME_SHORTCUT` применяется к #4
ТОЛЬКО если кто-то объявит текущий детерминированный план фаз «обученным» без cutover;
(3) архитектурная **активация** обучаемого плана фаз — **RESOLVED(Luba 2026-06-30): НЕ
активировать в v1**; план фаз остаётся `BASELINE_MUST_BE_LABELLED`, обучаемая замена =
deferred-to-later-phase (готовый блюпринт сохранён). Gate B
п.6 покрывает #4 как «классифицирован + помечен baseline-labelled + имеет инстанцированный
блюпринт по требованию» (см. строку #4 в §7-статусе).

### 8.3. Анти-leakage
- D-17: `exactPromptLeakageRate=0.0` во всех eval-гейтах; eval-промпт = train-промпт → `PassesGate=false`.
- Permutation-invariance: `CandidateOrderLeakageGuardUnit` — переставь кандидатов → выбор не меняется.
- Запрещённые фичи: candidate-index/позиция, `SourcePath` в скоринге, source-id/exact-prompt replay.
- `ExcludedLeakageSignals` в `GraphDpoPair` и `EdgeAuthorityInput` — обязательны.

### 8.4. Измерение
Численные метрики из `LearningBatchResult` (`LearningMachineContracts.cs:43`:
`AccuracyBefore/After`, `RewardMargin`, `CpuMs`, `PeakRamMb`, `ElapsedMs`) и
`LearningEvaluationResult.PassesGate` (`:100`). Без UNKNOWN/static/validation-only для
claim «trained feature». Ablation-дельта (§3.D) — обязательна.

### 8.5. Cutover-гейт (two-PASS блока 11)
Регистрация обученного юнита как `latest` (cutover) РАЗРЕШЕНА только если выбранный
edge-unit имеет `PASS_AS_TRAINED_FEATURE`: held-out `PassesGate=true` + `RewardMargin>0`
+ `AccuracyAfter>AccuracyBefore` (без UNKNOWN) + lifecycle (fresh-load + continue-training)
+ ablation-просадка ≥ порога. Иначе baseline остаётся финалом (честный untrained).

---

## 9. Тест-стратегия (FC-26 — все слои, обе фазы untrained/trained)

| Слой | Что проверяем |
|---|---|
| unit | `SemanticEdgeRouterUnit.ExecuteAsync` на фикстуре → правильный `OrderedTargets`; `CandidateOrderLeakageGuardUnit` блокирует позиционную фичу |
| integration | `IEdgeCandidateProvider` (понижённые роутеры) → guard → нормализация → обучаемый финал через реальные реестры |
| e2e-host | проход через `AdaptiveLogicalOrchestrator.RunAsync` (`:85`) с обученным `SemanticEdgeRouterUnit` (latest-wins подмена baseline через `ResolveLatest:107`) |
| QA-automation | prompt-suite: разные смыслы дают разные рёбра; путь и позиция не влияют |
| negative | искусственная позиционная фича → guard FAIL (блокирует); closed-teacher в train → `DATASET_REBUILD_REQUIRED` |
| untrained-honesty | без checkpoint финал честно `KnowledgeEdge`/not-ready; **не** прячет детерминизм (perm-invariance, no-position, no-source-id, no-exact-prompt) → `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` |
| trained-acceptance | checkpoint + held-out (`PassesGate=true`) + **ablation** (выключение финала роняет accuracy ≥ порога) + fresh-load + continue-training → `PASS_AS_TRAINED_FEATURE` |
| regression | baseline ≥ 1488 тестов зелёный; router/floor/path-warning/SmartSkills permutation не сломаны; baseline-роутеры НЕ удалены |

---

## 10. Gate B — измеримое условие готовности

Блок 12 (no-hidden-determinism cutover) считается готовым, когда **все** выполнены:

1. **Permutation-инвариантность:** `CandidateOrderLeakageGuardUnit` подтверждает, что для
   5 перестановок одного входа `EdgeAuthorityDecision.OrderedTargets` идентичен
   (`PermutationInvariant=true`); искусственная позиционная фича → guard FAIL и блокировка.
2. **Held-out:** `LearningEvaluationResult.PassesGate == true`, `RewardMargin > 0`,
   `AccuracyAfter > AccuracyBefore` (без UNKNOWN).
3. **Ablation (§3.D):** accuracy(полный) − accuracy(baseline-only) ≥
   `ablation_threshold_locked` (Ф0 Q11). **Решение Luba 2026-06-29 (RESOLVED):**
   `ablation_threshold_locked = 0.02` абсолютной accuracy (2 процентных пункта), **ЕДИНЫЙ
   порог для unit/edge/graph**; до lock гейт возвращал `blocked_not_configured` — теперь
   **сконфигурирован = 0.02** (симметрия с залоченным lift-гейтом +0.02, claude-plan-11
   §6.5). Иначе `FORBIDDEN_RUNTIME_SHORTCUT`.
4. **Анти-leakage:** `exactPromptLeakageRate == 0.0` (D-17) и
   `source_path_used_for_scoring=false` в warnings каждого решения.
5. **Cutover-гейт:** `ResolveLatest` отдаёт обученный `SemanticEdgeRouterUnit` поверх
   baseline ТОЛЬКО после `PASS_AS_TRAINED_FEATURE`; baseline-роутеры **остались** в
   реестре (для ablation), не удалены.
6. **Классификация:** `classified_mechanism_count == 9`; каждый вердикт ∈ набора skill;
   0 немаркированных финальных авторитетов **и на ребре, и на L4-пути #4**; каждый
   forbidden/baseline авторитет имеет блюпринт. **Механизм #4 (B-22, диспозиция
   спроектирована):** `OrchestrationScenarioPhasePlanner.PlanPhases` — отдельный
   L4-авторитет плана фаз сценария; рекомендуемый дефолт v1 = **`BASELINE_MUST_BE_LABELLED`**
   (прецедент Q14), и §13 Q1 содержит **инстанцированный** обучаемый блюпринт замены
   (`SemanticPhasePlanUnit` + `PhasePlanState` + DPO + ablation `>=0.02` + forbidden-leakage)
   на случай активации. Gate B п.6 для #4 = «классифицирован + помечен baseline-labelled +
   имеет инстанцированный блюпринт по требованию». **RESOLVED(Luba 2026-06-30):** активация
   обучаемого плана фаз в v1 **НЕ даётся** — остаётся `BASELINE_MUST_BE_LABELLED`, обучаемая
   замена = deferred-to-later-phase (см. §8.2 и строку #4 в §7).
7. **Lifecycle:** fresh-load из checkpoint = результат сразу после train; continue-training
   стартует **от** checkpoint; export→import сохраняет `TeacherProvenance` (20 полей; 13+5+2 RR6).
8. **Легальность:** каждая обучающая строка `ClosedProprietaryModel=false` (или явный
   `DATASET_REBUILD_REQUIRED`), `provenance_field_presence_rate == 1.0`.

Формулировки вида «работает корректно» — НЕ Gate B.

### 10.1. Failure modes (что считаем провалом блока)

- Выбор меняется при перестановке кандидатов → order leakage (провал п.1).
- `AccuracyAfter ≈ AccuracyBefore` или `RewardMargin ≤ 0` → нет обучения (провал п.2).
- Ablation не роняет accuracy → финал фиктивен, реально решает baseline shortcut
  (провал п.3, `FORBIDDEN_RUNTIME_SHORTCUT`) — это и есть скрытый детерминизм.
- `SourcePath`/candidate-index влияет на выбор → leakage (провал п.4).
- **baseline-роутеры УДАЛЕНЫ** → нечем делать ablation, нельзя доказать не-shortcut
  (провал п.5; явная архитектурная ошибка — понижать, не удалять).
- cutover сделан до `PASS_AS_TRAINED_FEATURE` → masquerade (провал п.5).
- `classified_mechanism_count != 9` или остался немаркированный финал (провал п.6).
- Fresh-load даёт другой результат → checkpoint не воспроизводим (провал п.7).
- В батче `ClosedProprietaryModel=true & TeacherOutputUsedForTraining=true` →
  `DATASET_REBUILD_REQUIRED` (провал п.8).

---

## 11. §5 Иерархия уровней (Input/Output + помеченные цепочки исполнения)

> FC-18: когда план трогает ILearningMachine/ILogicalOrchestrator/ILogicalFlow/
> ILogicalUnit — обязателен §5 со всеми уровнями, типами Input/Output и помеченными
> цепочками. (В бенчмарке отсутствовал — здесь явный.)

```text
L5  ILearningMachine<DynamicGraphDomain>            (LearningMachineContracts.cs:208)
      Input  : LearningBatch<DynamicGraphDomain>     (:18)  [GraphDpoPair → LearningTrainingItem :30]
      Output : LearningBatchResult                   (:43)  / LearningEvaluationResult.PassesGate (:100)
      Роль   : ЕДИНСТВЕННЫЙ владелец Train* (A-LM-02); учит SemanticEdgeRouterUnit; export/import (:222/:227)
        │  публикует обученный юнит (latest-wins, A-LM-03)
        ▼
L4  ILogicalOrchestrator                            (LogicalContracts.cs:202)
      Input  : LogicalOrchestratorRequest
      Output : LogicalOrchestratorResult (BranchResults :190)
      Роль   : резолвит юнит через ResolveLatest (:107) — ПОСЛЕ cutover отдаёт обученный
        │  выбирает ребро и его финальный авторитет
        ▼
L3  ILogicalFlow<EdgeTraversalInput>  ← DynamicGraphEdgeFlow (блок 02)   (контракт LogicalContracts.cs:166)
      Input  : LogicalFlowRequest<EdgeTraversalInput> (:105)
      Output : LogicalFlowResult                      (:131) + LogicalContextPacket (:158)
      Роль   : генерация кандидатов (baseline) → guard-проба → анти-leakage вход → финал → fan-out/fan-in
        │  вызывает финальный авторитет (и guard как diagnostics)
        ▼
L2  ILogicalUnit<EdgeAuthorityInput, EdgeAuthorityDecision>  ← SemanticEdgeRouterUnit (NEW)  (контракт LogicalContracts.cs:97)
      Input  : LogicalUnitRequest<EdgeAuthorityInput>  (leakage-free)
      Output : LogicalUnitResult<EdgeAuthorityDecision>
      Роль   : ОБУЧАЕМЫЙ финал ребра; K-NN model-component внутри (KnnLogicalUnit.cs:218)
    L2' ILogicalUnit<CandidateOrderProbeInput, CandidateOrderLeakageReport> ← CandidateOrderLeakageGuardUnit (ADAPTED-FROM-EXISTING, прецедент SkillSelectionLogicalHierarchy.cs:271)
      Роль   : diagnostics/guard (НЕ финал); блокирует позиционные фичи
```

Помеченные цепочки исполнения:
- **Цепочка cutover (рантайм после two-PASS):** L4 `ResolveLatest` (`:107`) → обученный
  `SemanticEdgeRouterUnit` (L2) → финал; baseline-роутеры понижены до кандидатов на L3.
- **Цепочка ablation (доказательство):** тот же held-out → прогон A (обучен) vs прогон B
  (baseline-only финал) → дельта ≥ порога доказывает не-shortcut.
- **Цепочка guard (честность порядка):** L3 даёт кандидаты → `CandidateOrderLeakageGuardUnit`
  (L2') прогоняет N перестановок → `PermutationInvariant`; при утечке блокирует фичу.
- **Цепочка обучения (offline):** трасса L3 → `GraphDpoPair(Level=edge)` →
  `LearningTrainingItem` → L5 `TrainBatchAsync` → новый `SemanticEdgeRouterUnit`
  публикуется (latest-wins) → подменяет baseline на L2.

---

## 12. Референсы (Stage 2): затрагиваемые проекты/доки/реконы/гайды

Код (с путями, все цитаты проверены чтением в `C:/Projects/llm_mvp_core`):
- `00.Core/LogicalContracts.cs` (L2/L3/L4 контракты, `:97/:166/:202`; конверты `:105/:131/:158/:190`).
- `00.Core/LearningMachineContracts.cs` (L5, DPO-формат, метрики; `:18/:30/:43/:100/:208/:212/:222/:227`).
- `21.AgentInfrastructure/Logical/MultiSubPhaseRouterUnit.cs` (механизмы #1/#8; `:8/:23/:283/:291/:361/:373`).
- `26.TrainableRouter/Training/KeywordUnitScorer.cs` (механизм #2; `:6/:31/:38/:49`).
- `26.TrainableRouter/Training/RoutingProfileTrainer.cs` (механизм #5; `:5/:22/:33`).
- `21.AgentInfrastructure/Logical/SubPhaseRouterUnit.cs` (механизм #3; `SubPhaseRoutingDecision:21`, `KnowledgeEdge:29`).
- `21.AgentInfrastructure/Logical/KnnLogicalUnit.cs` (механизм #6, K-NN model-component; `:218`).
- `21.AgentInfrastructure/Logical/InMemoryLogicalUnitRegistry.cs` (механизм #7, cutover; `IsLearned:21`, `ResolveLatest:107`, `OrderByDescending(RegistrationOrder):118`).
- `21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs` (механизм #9, рантайм; `:85/:316/:504`).
- `43.SmartSkillsDomain/SkillSelectionContracts.cs` (прецедент guard; `PermutationSelectionInvariant:35`, `CandidateOrderPermutationProbe:45`).
- `43.SmartSkillsDomain/SkillSelectionLogicalHierarchy.cs` (**существующий guard-класс** `CandidateOrderLeakageGuardUnit:271`, UnitId `candidate_order_leakage_guard:273`, Role `candidate_position_diagnostics_provenance_only:274`; `candidate_position_scaffold_final_authority=false:150`). Источник для edge-версии (ADAPTED-FROM-EXISTING).
- `21.AgentInfrastructure/Logical/LogicalCapabilityTrainingContracts.cs` (`ILogicalCapabilityTrainer.TrainAndRegisterAsync` — мост обучения).
- `25.ModelServer` + `IModelRepository` / `IRemoteUnitLocator` (checkpoint/manifest/locator авторитета ребра).
- `docs/DECISIONS-LOCKED.md` (A-LM-02/A-LM-03/A-LM-09/A-LM-12/A-LM-13, D-15, D-17).
- `26.TrainableRouter/Training/OrchestrationScenarioPhasePlanner.cs` (механизм #4, детерминированный keyword-планировщик фаз; `:5/:7-19/:21/:30`).
- `26.TrainableRouter/Training/OrchestrationTaskDecompositionFlow.cs` (рантайм-вызов планировщика фаз `:70`; детерминированный финальный ПОРЯДОК фаз — `PhaseOrder[]:18-30`, `OrderBy(PhaseIndex):75`, `BuildPhases:148`, `PhaseIndex:197`; область #4/§13 Q1, помечен baseline-ordering).
- `26.TrainableRouter/Training/OrchestrationAcceptanceGateEvaluator.cs` (рантайм-вызов планировщика фаз; `:39`).

Доки/скилы:
- `docs/skills/trainable-architecture-decision-SKILL.md`, `implementation-doc-uml-SKILL.md`,
  `implementation-doc-algorithms-SKILL.md`, `implementation-doc-hierarchy-SKILL.md`,
  `plan-review-SKILL.md`, `dpo-triple-format-SKILL.md`.
- `CLAUDE.md` (двух-PASS, Part A Contract, anti-shortcut, Deterministic-Boundary Code Review Gate).

Реконы/гайды/братские артефакты (evidence-only, база `C:/Users/lubao/Documents/xiigen/llm mvp/dpo legal build/`):
- `claude-04-dpo-legal-model-policy.md`, `claude-05-dpo-data-training-measurement-guide.md`,
  `claude-06-ilearningdynamicgraph-architecture-brief.md`, `claude-07-execution-gates-open-questions.md`,
  `claude-plan-02-edge.md` (братский блок — канон имён/контрактов/`TeacherProvenance`/`GraphDpoPair`),
  `claude-EXECUTION-CONTROL-BOARD.md` §Ф8:898-937 (источник подшагов Ф8.1.1–2, Ф8.2.1–2).
- `codex-plan-12-no-hidden-determinism.md` — **бенчмарк этого блока, read-only**.
- `codex-ILEARNINGDYNAMICGRAPH-PLAN-v1.md` §A.4 / §C (мастер-канон 9 механизмов, R-07, счётчики 12/26/59; read-only).
- `codex-recon-core-21-infra.md`, `codex-recon-core-26-router.md`, `codex-recon-core-17-dynamic.md`,
  `codex-recon-core-43-smartskills.md`.

---

## 13. Открытые вопросы для Luba (в финальную секцию плана)

1. **Авторитет механизма #4 (план фаз сценария: ВЫБОР + ПОРЯДОК).** Класс
   подтверждён чтением (`OrchestrationScenarioPhasePlanner.cs:5/:21/:30`) и вызывается в
   рантайме (`OrchestrationTaskDecompositionFlow.cs:70`,
   `OrchestrationAcceptanceGateEvaluator.cs:39`) как детерминированный keyword-планировщик
   фаз. **Область вопроса включает два детерминированных финала на одном L4-пути:**
   (а) keyword-**ВЫБОР** фаз `PlanPhases` (`:21/:30`); (б) детерминированный финальный
   **ПОРЯДОК** фаз — `PhaseOrder[]` (`OrchestrationTaskDecompositionFlow.cs:18-30`) →
   `PhaseIndex` (`:197`) → `OrderBy(PhaseIndex)` (`:75`) → `BuildPhases` (`:148`). Открытый
   вопрос **архитектурный, не «существует ли класс»**: разрешено ли финальному плану фаз
   сценария (и его порядку) оставаться детерминированным baseline/eval-генератором
   (как #1/#2), или этот выбор+порядок фаз обязан перейти в обучаемый L4/L5-юнит (тогда
   нужен блюпринт замены, как в §5.3, покрывающий ОБА: и выбор, и порядок)? От ответа
   зависит, остаётся ли вердикт `FORBIDDEN_RUNTIME_SHORTCUT` (если финал) или становится
   `BASELINE_MUST_BE_LABELLED` (после понижения).
   > **Закрытие механизма #4 (B-22 / R13-reconcile, RESOLVED-как-проект, 2026-06-30).**
   > Прежний статус «DEFERRED, блюпринта нет» был НЕЧЕСТНОЙ дырой относительно канон-гейта
   > R4/A-LM-09: один из 9 механизмов оставался без диспозиции, тогда как Q14 (Решение Luba
   > 2026-06-29) уже задал **точный прецедент** для ровно такого случая (детерминированный
   > финал-порядок). Применяю прецедент Q14 к #4 (порядок ФАЗ — это тот же класс задачи,
   > что порядок РЁБЕР, только на L4-пути оркестрации), поэтому диспозиция теперь
   > спроектирована, а не отложена:
   > - **РЕКОМЕНДУЕМЫЙ ДЕФОЛТ v1 (как Q14):** и ВЫБОР фаз (`PlanPhases`), и ПОРЯДОК фаз
   >   (`PhaseOrder/PhaseIndex/OrderBy/BuildPhases`) остаются **детерминированным честным
   >   baseline** с вердиктом **`BASELINE_MUST_BE_LABELLED`** (НЕ `FORBIDDEN`, пока нет claim
   >   «обученный план фаз»), `isLearned=false`, `phase_plan_semantic_quality_claimed=false`.
   >   Это законно по правилу «honest deterministic baseline без claim» (B.2 contradiction
   >   sweep), точно как порядок рёбер Q14. Финал #4 на L4 помечается, baseline НЕ удаляется
   >   (нужен для ablation), `classified_mechanism_count == 9` остаётся.
   > - **Инстанцированный блюпринт замены (НЕ промпт исполнителю), когда/если Luba активирует
   >   обучаемый план фаз** — по образцу §5.3 и `GraphOrderResolverUnit` (B-10): обучаемый
   >   `SemanticPhasePlanUnit : ILogicalUnit<PhasePlanInput, PhasePlanDecision>` (роль
   >   `phase-plan-authority`), typed trained-state `PhasePlanState { PhaseVocabulary:
   >   Dictionary<string,int>; PhaseSelectMargins: Dictionary<string,double>;
   >   PhaseOrderMargins: Dictionary<OrderPairKey,double>; FeatureWeights: float[];
   >   KnnPrototypes (K=5, capability-кластеры) }`. **Training-update (вход-строка → мутация
   >   → checkpoint):** `GraphDpoPair(Level="phase_plan", Chosen=выбранный+упорядоченный
   >   набор фаз, Rejected=keyword-набор/неверный порядок [WRONG: выбрал «deploy» по слову
   >   «service», смысл = test/fix])` → DPO-сдвиг `PhaseSelectMargins`/`PhaseOrderMargins`
   >   через `DpoTrainer.cs:44/119` → `ExportAsync` checkpoint+manifest (schema
   >   `graph-phase-plan/1`), continue-training от checkpoint (не replay). **Роль KNN:**
   >   только признак похожести capability-кластеров (`OK_MODEL_COMPONENT`, сырые prompt не
   >   хранятся → нет exact-prompt replay), не финальная власть. **Ablation:**
   >   детерминированный baseline-план (текущий `OrchestrationScenarioPhasePlanner`) vs
   >   обученный финал на ОДНОМ source-disjoint held-out; просадка ≥ **`0.02`** (Q11
   >   `ablation_threshold_locked`, единый порог) — иначе финал фиктивен =
   >   `FORBIDDEN_RUNTIME_SHORTCUT`, cutover откат. **Forbidden leakage:**
   >   candidate-index/позиция фазы в списке, array-order правил, `SourcePath`, source-id,
   >   exact-prompt, `BaselinePlanHash`-как-признак (исключаются из `PhasePlanInput`,
   >   перм-инвариантность по набору правил-фаз). Cutover-гейт #4 = тот же two-PASS блока 11
   >   (§8.5), плюс legal Teacher Gate (§8.1) на chosen-фаз-планы. Размещение чекпойнта:
   >   common → `llm_mvp_core/models/<DynamicGraphGlobal>/phase-plan/...` (Q6/B-17).
   > - **Что остаётся за Luba (residual, НЕ выдумано как факт):** один архитектурный выбор —
   >   **активировать** ли обучаемый `SemanticPhasePlanUnit` в каком-либо проекте/фазе (тогда
   >   блюпринт выше исполняется), или **оставить** детерминированный план фаз baseline для
   >   v1 (рекомендуемый дефолт). Это R2/R4-решение масштаба домена, как Q41 — поэтому
   >   спроектирован рекомендуемый дефолт + готовый блюпринт, а назначение активации = Luba.
   >   Закрытие B-22: вердикт #4 больше НЕ «DEFERRED без блюпринта»; он = **baseline-labelled
   >   v1 + инстанцированный обучаемый путь по требованию**, что устраняет последнюю
   >   немаркированную диспозицию среди 9 механизмов. *(Источник: шаг починки B-22 /
   >   R13-reconcile; прецедент Q14 §E.0; образец блюпринта §5.3 + B-10.)*
2. **Имя обучаемого финала ребра.** Подтвердить, что `SemanticEdgeRouterUnit` (имя
   cutover-блока 12) и `EdgeAuthorityUnit` (имя контракт-блока 02) — это **одна роль**
   `edge-authority`, а не два разных юнита. Я свёл их в одну роль; нужно явное «да».
3. **Порог ablation** (`ablation_threshold_locked`, Gate B п.3): назначить численно
   (минимальная просадка accuracy при выключении обученного финала, например ≥ X
   процентных пунктов). Без числа Gate B п.3 не измерим.
   > **Решение Luba 2026-06-29 (RESOLVED, Q11):** `ablation_threshold_locked = 0.02`
   > абсолютной accuracy (2 процентных пункта), **ЕДИНЫЙ порог для unit/edge/graph**; до lock
   > Gate B п.3 был `blocked_not_configured` — теперь **сконфигурирован = 0.02** (симметрия с
   > залоченным lift-гейтом +0.02, claude-plan-11 §6.5). Gate B п.3 измерим.
   > *(Источник: мастер §E.0 / Q11; задание Luba 2026-06-29.)*
4. **Где хранить `SemanticEdgeRoutingState` checkpoint/manifest (R5/A-LM-10):**
   common — в `llm_mvp_core` (`models/`+`training-data/`), user-adaptive — на стороне
   пользователя (`.xiigen/smart-skills/`)? Подтвердить директорию/манифест/локатор.
5. **`TeacherProvenance` — 20 полей (13+5+2 RR6) и размещение `JudgeModel`.** Подтвердить, что
   `JudgeModel` живёт ВНУТРИ `TeacherProvenance`, и что владелец канона числа полей —
   блок 08. Где физически хранить (строка `GraphDpoPair` / батч / манифест экспорта)?
6. **Удаление vs понижение baseline.** Подтвердить правило: детерминированные роутеры
   (#1, #2) **не удаляются**, а понижаются до генераторов кандидатов (нужны для
   ablation). Это в плане как hard-rule; нужно «да», что это не временный костыль.

> **Кросс-ссылка Q14 (порядок рёбер) — Решение Luba 2026-06-29 (RESOLVED, мастер §E.0):**
> v1 = детерминистический честный **baseline** для порядка обхода/зависимостей рёбер
> (OK_INTERNAL_DETAIL); обучаемость живёт в `EdgeAuthorityUnit`. Обучаемый предиктор
> зависимостей (Jaccard E49/E50) **отложен**; при вводе позже — ТОЛЬКО как настоящий
> trainable `ILogicalUnit` (типизированное состояние + checkpoint + ablation gate с порогом
> 0.02, R4), а детерминистический порядок служит ablation-baseline. Это подтверждает
> детерминистический baseline порядка рёбер для v1 (отдельно от L4-вопроса #4/§13.1, который
> про порядок ФАЗ оркестрации). **B-22 (2026-06-30):** тот же прецедент Q14 применён к #4 —
> #4 больше НЕ DEFERRED: дефолт v1 = `BASELINE_MUST_BE_LABELLED` + инстанцированный
> обучаемый блюпринт `SemanticPhasePlanUnit` (§13 Q1). **RESOLVED(Luba 2026-06-30): активация
> НЕ даётся в v1** — остаётся baseline, обучаемая замена = deferred-to-later-phase.
> *(Источник: мастер §E.0 / Q14 + Q-mech4; задание Luba 2026-06-29/2026-06-30; шаг B-22.)*

---

## §ADD-12 — Additive: двух-осевой ablation для KNN-памяти (закрывает HIGH: KNN-memorization маскируется под trained authority)

> **Additive**: усиливает ablation-гейт §10 для `KnnMemory` в `SemanticEdgeRoutingState`;
> прежний одноосевой ablation не удаляется, добавляется вторая ось.

**Правило (LOCKED-add, KNN Model-Component + memorization ablation):** `KnnMemory` в
`SemanticEdgeRoutingState` — обучаемый вход Confidence, значит должен быть (а) инстанцирован
как model-component и (б) отдельно аблируем.

```text
// (а) KNN инстанцирование (LOCKED-add, как §ADD-04.2):
//   feature→vector: CapabilityTags → TagCentroid (one-hot среднее, L2-норма);
//   distance: 1 − cos; K=5; tie-break: наим. prototype.Id; cluster threshold cos>=0.60.
// (б) Двух-осевой ablation (LOCKED-add): §10 п.3 «disable trained authority = baseline-formula only»
//   ablирует обученные веса, но НЕ ablирует KNN-индекс. Добавить ВТОРУЮ ось:
//     ось A (веса): zero веса, оставить KNN → lift_A;
//     ось B (KNN): keep веса, DISABLE KnnMemory (пустой индекс) → lift_B.
//   Gate B: юнит проходит ТОЛЬКО если lift_A >= 0.02 (обобщение реально), НЕ когда весь lift = lift_B
//     (значит вклад = near-exact KNN-retrieval = memorization).
//   lift_A < 0.02 при lift_full >= 0.02 → KNN_MEMORIZATION_MASQUERADE → карантин.
// Требует per-домен accuracy-функцию для edge-route (§ADD-11.4) — иначе ablation неисчислим.
```
