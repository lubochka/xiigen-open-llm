# Codex Knowledge Library — корпус «llm mvp» (для проекта ILearningDynamicGraph)

Назначение: индекс-библиотека по 23 рекон-файлам корпуса `llm mvp`
(`codex-recon-mvp-*.md`) как evidence-база для ПЛАНА проекта
**ILearningDynamicGraph** — авто-построение DPO для
`ILearningUnit` / `ILearningFlow` / `ILearningOrchestrator` / `ILearningDynamicGraph`.

Роль файла: библиотекарь-индекс. Это НЕ план и НЕ разрешение на реализацию.
Только навигация по собранному evidence. Все пути — реально найденные рекон-файлы
в `C:/Users/lubao/Documents/xiigen/llm mvp/dpo legal build/`.

Замечание об именовании: во всём корпусе фактически используются термины
`ILearningMachine<TDomain>` / `ILogicalOrchestrator` / `ILogicalFlow` /
`ILogicalUnit` / `ILearningMachinePair` (из `00.Core/LearningMachineContracts.cs`).
Целевые имена плана `ILearningUnit/Flow/Orchestrator/DynamicGraph` в корпусе НЕ
встречаются — это новая надстройка; маппинг «концепт корпуса → целевой уровень»
делает сам план. `ILearningDynamicGraph` как контракт ни в одном файле не описан.

---

## 1. Краткое резюме корпуса для проекта ILearningDynamicGraph

Корпус `llm mvp` — это в основном **методология и evidence**, а не финальный код:
планы обучения, гайды авторинга DPO-пар, реестры источников/лицензий, прогоны
метрик и транскрипты проектных сессий. Реальный рантайм-код лежит в репозитории
`C:/Projects/llm_mvp_core` (его индексирует core-корпус) и в корпусе `llm mvp`
присутствует только цитатами/ссылками.

Что корпус даёт плану ILearningDynamicGraph напрямую:

- **Готовый прообраз графа.** Несколько областей описывают один и тот же
  9-узловой UML-конвейер оркестрации (Domain Splitter → Translator[array] →
  Planner → Processor[array] → Applier[array] → Result-Translator[array] →
  Arbiter-Selector → Arbiter-Units → Formatter, с reprocessing-петлёй).
  Это прямой прообраз DynamicGraph: узлы = роли-юниты, рёбра = маршруты с
  fan-out/fan-in и обратным ребром. Источники: `local-wiki`, `learning-sets`,
  `plan-set`, `orch-exp` (E53 node-00..09), `dyn-flow`.
- **Рабочий референс авто-построения DPO.** Есть исполняемые ETL-скрипты
  (`prepare_internal_learning_sets.py`, `build_dataset.py`) и каноническая схема
  пары `a_side / chosen / rejected / [WRONG:] / target_behavior / domain_focus`
  с провенанс-полями. Источники: `internal-learn`, `internal-learn-2`,
  `train-ready`, `all-skills` (формат + 6 learning-signal), `learning-sets`,
  `plan-set`.
- **Понимание метрики Accuracy.** Везде Accuracy в прогонах = доля DPO-пар с
  `margin>0` (live reward-margin), а НЕ held-out семантическая точность —
  подтверждено живым кодом `11.DPO/DpoTrainer.cs`. Для completion-evidence
  графа этого недостаточно: нужен held-out + ablation. Источники: `train-ready`,
  `experiments`, `teaching-ledger`, `internal-learn-2`.
- **Дисциплина легального teacher/provenance.** Готовый лицензионный гейт
  (MIT/Apache-2.0 preferred; BLOCK для CC BY-SA/AGPL/GPL и для
  model-output-provenance источников типа ShareGPT/Alpaca), анти-distillation
  принцип `external_role=a_side_style_only` (внешний корпус даёт только форму
  A-side, chosen всегда из XIIGen-контрактов). Источники: `external-sources`,
  `local-wiki`, `orch-teaching`.
- **Прецеденты малой обучаемой модели и заведения домена с нуля.** «Малой модели
  достаточно» обучать границы предпочтений (DPO), а не язык с нуля; новый домен =
  копия весов general-модели + fine-tune на доменном DPO-шарде. Источники:
  `small-model`, `novyy-domen`, `lang-translate`.

Сквозной красный флаг всего корпуса (БЛОКИРУЮЩИЙ для «dpo legal build»):
почти нигде НЕ заполнены поля Legal Teacher Gate (`teacher_access_mode`,
`hosting_provider`, `model_id`, `model_license`, `provider_terms_checked`,
`closed_proprietary_model`). Есть провенанс входной подсказки (user_request),
но не teacher-провенанс целевой стороны (chosen). Несколько областей содержат
сырые выводы закрытых моделей (Codex/GPT-5 rollout/session-логи) — по
Provenance Gate такой вывод запрещён как chosen/rejected/training до проверки
(иначе `DATASET_REBUILD_REQUIRED`).

Второй сквозной флаг: ни один документ корпуса не фиксирует **graph-контракты по
Graph-Style Gate** — many-to-many / multi-output edge / async-исполнение /
владелец авторитета и hidden-shortcut-check на КАЖДОМ ребре. Есть только
агрегатные `lost_edge=0` / `dependency_violation=0`. Это работа плана.

---

## 2. Таблицы по темам

Столбцы: Файл рекона (путь `codex-recon-*.md`) | Заголовок | Краткое | Релевантность.
Все пути в каталоге `C:/Users/lubao/Documents/xiigen/llm mvp/dpo legal build/`.

### Тема A. Контракты / иерархия ILearningMachine

| Файл рекона | Заголовок | Краткое | Релевантность | source_category | provenance_status | teacher_access_mode | legal_verdict | source_hash_or_ref | implementation_use | gap_owner |
|---|---|---|---|---|---|---|---|---|---|---|
| `codex-recon-mvp-novyy-domen.md` | MVP «новый домейн»: как заводится обучаемый домен с нуля | Транскрипт сессии: топология `ILearningMachine<TDomain>`; новый домен = копия весов base general-модели + fine-tune на доменном DPO-шарде; export/import через 25.ModelServer; honest scaffold/Bootstrap до обучения; KNN-by-structure и LoRA — компоненты внутри юнита. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| `codex-recon-mvp-small-model.md` | Рациональ малой обучающейся модели (TranslationDomain) | Раскладка уровней: routing flow = `ILogicalFlow`, direction-адаптеры = кандидаты `ILogicalUnit`/`ILearningMachinePair`; DPO учит границы предпочтений, не язык с нуля; KNN с accuracy 1.000 на held-out = меморизация (запрещено). | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| `codex-recon-mvp-dyn-flow.md` | Концепт динамического потока (предшественник динамического графа) | Дизайн-намерение: Orchestrator→`ILogicalOrchestrator`, локальный поток→`ILogicalFlow`, роли-юниты→`ILogicalUnit`, style adapter→`ILearningMachinePair`; KNN/budget/priority — детерминистические хелперы без финального авторитета; locked-граница D-13. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| `codex-recon-mvp-skills-loading.md` | Механизм загрузки скилов в расширение | `SkillSelectionMachine : ILearningMachine<SkillSelectionDomain>`; точка интеграции — passthrough-заглушка `OrchestrationPlanMachine.cs:65`, рядом контракт `LearningMachineContracts.cs:208`; единого плана нет (REJECT). | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |

### Тема B. DPO-построение (формат пар, авторинг, негативы)

| Файл рекона | Заголовок | Краткое | Релевантность | source_category | provenance_status | teacher_access_mode | legal_verdict | source_hash_or_ref | implementation_use | gap_owner |
|---|---|---|---|---|---|---|---|---|---|---|
| `codex-recon-mvp-internal-learn.md` | Внутренние обучающие сеты и метод (XIIGen-first DPO конвейер) | Исполняемый `prepare_internal_learning_sets.py` (инвентаризация→классификация→авто-извлечение пар→tier/eval/repair); гайды authoring v8, examination v15, measurement v6; целевой обучаемый объект = OrchestrationRoutingFlow/AdaptiveLogicalOrchestrator. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| `codex-recon-mvp-internal-learn-2.md` | Внутреннее обучение v2 (model-led DPO-корпус) | Канонический выход `train-ready-v2/*trainable.jsonl`; поля `a_side/chosen/rejected/[WRONG:]/target_behavior/domain_focus`; обучает Layer-0 prompt-compiler → PredicateIds+Confidence на узле/ребре; расхождение версий POINTER (V192/3884) vs RUN-STATUS (V309/1404). | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| `codex-recon-mvp-train-ready.md` | Готовые к обучению датасеты v2: формат и провенанс | Цель supplement = 12 пар; канонический корпус глубже (`internal learnin 2/.../train-ready-v2`, pointer V246/672 строки); Accuracy = доля margin>0 подтверждена `11.DPO/DpoTrainer.cs:64,106,120`; сталая ссылка раннера на несуществующий experiments-v2 путь. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| `codex-recon-mvp-all-skills.md` | Корпус скилов all-skills — DPO/обучение/измерение | DPO как «эксперимент-пара» (input/baseline=rejected/enhanced=chosen/delta); 6 типов learning-signal; реальное обучение помечено FUTURE/38.ContinuousLearning; даёт формат пар + гейты измерения. | medium | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| `codex-recon-mvp-doc-learning.md` | MVP «documentation learning» (DPO для документации legacy .NET) | Две DPO-цели: контент-документации и routing юнитов (`RoutingDpoPair`); партии по 50 пар, held-out, обязательный `[WRONG:]`; рантайм `CodeDocumentationRoutingFlow : ILogicalFlow` в 26.TrainableRouter. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| `codex-recon-mvp-learning-sets.md` | Структура обучающего сета (формат пары A→узел→B) | Supervised-пары `Runtime A: / Expected B: / Alternate A/B: / Source evidence:`; B одного узла = часть A следующего; негативы через failure/feedback/arbiter-flag; нет явных rejected-веток — нужно правило построения троек. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| `codex-recon-mvp-plan-set.md` | Как планируются обучающие сеты | Пер-узловые пары A→B для 9 узлов; A=точный рантайм-вход, B=точный рантайм-выход (не описание), каждая пара ≥2× (rephrasing), Node 8 обязан иметь pass-кейс, array-узлы — пара на элемент; EXAMINATION-METHOD-FINAL задаёт метод. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |

### Тема C. Обучение и continue-training

| Файл рекона | Заголовок | Краткое | Релевантность | source_category | provenance_status | teacher_access_mode | legal_verdict | source_hash_or_ref | implementation_use | gap_owner |
|---|---|---|---|---|---|---|---|---|---|---|
| `codex-recon-mvp-orch-teaching.md` | Обучение оркестратора (DPO/teaching plan) | Одна пара за раз, LoRA + `DpoTrainer.Step`, кумулятивное обучение с continue-training из родительского чекпойнта, trust-машина Probationary/Trusted/Rejected через `InMemoryLogicalUnitRegistry.ResolveLatest`; 5 обучаемых юнитов; источник пар = XIIGen-сессии. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| `codex-recon-mvp-orch-impl.md` | Дизайн/реализация оркестратора (AdaptiveLogicalOrchestrator) | Конвейер домен→компилятор→MultiSubPhaseRouter→OrchestratorExpander(E49)→DomainGraphClosure(E50)→ветви→PhaseTransitionSignal→агрегация; обучающий путь knowledge-edge→60-слот-вектор→DPO/LoRA→Probationary; решающие механизмы детерминированные; «проект 21 = остров». | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| `codex-recon-mvp-small-model.md` | Рациональ малой обучающейся модели | Батчи по 50 пар + held-out после каждого; DPO учит границы; раскладка уровней как прецедент для авто-DPO графа. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| `codex-recon-mvp-experiments.md` | Эксперименты v2: метрики обучения (DPO reward-margin runner) | Целевая папка `xiigen/experiments-v2` пуста; реальный раннер в `llm_mvp_core/experiments-v2/INTERNAL-TRAIN-READY-V191` (Program.cs + DPO.dll); метрики Loss/ChosenReward/RejectedReward/RewardMargin/Accuracy; raннер = eval/helper. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |

### Тема D. Чекпойнты / манифест / lifecycle

| Файл рекона | Заголовок | Краткое | Релевантность | source_category | provenance_status | teacher_access_mode | legal_verdict | source_hash_or_ref | implementation_use | gap_owner |
|---|---|---|---|---|---|---|---|---|---|---|
| `codex-recon-mvp-novyy-domen.md` | MVP «новый домейн» | export/import/upgrade/rollback домена через `25.ModelServer` + `ILearningMachineRepository`; LoRA-механика (rank=4, delta_W=A×B, заморозка Bootstrap) — только как идея в диалоге, чекпойнты не подтверждены. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| `codex-recon-mvp-teaching-ledger.md` | teaching-ledger — измерение скорости/точности/качества | checkpoint_bytes как ось стоимости; `dpo_step_*.json` (Loss/RewardMargin/Accuracy); registry_before/after (flow/unit counts) + ACCEPT_ROW + trust_state; везде только Step 1 (нет траектории Step 2..N); manifest/locator/export-import не видны. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| `codex-recon-mvp-orch-teaching.md` | Обучение оркестратора | continue-training из родительского чекпойнта; trust-машина и `ResolveLatest`; сигнатуры планов (2026-05-05) расходятся с кодом (Rank 2 vs декларированный Rank 4). | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| `codex-recon-mvp-train-ready.md` | Готовые к обучению датасеты v2 | `CURRENT-TRAIN-READY-POINTER.json` как указатель canonical-чекпойнта данных; не проверено, что 12 пар попадают в реальный loader/манифест. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |

### Тема E. Измерение скорости / точности / качества

| Файл рекона | Заголовок | Краткое | Релевантность | source_category | provenance_status | teacher_access_mode | legal_verdict | source_hash_or_ref | implementation_use | gap_owner |
|---|---|---|---|---|---|---|---|---|---|---|
| `codex-recon-mvp-teaching-ledger.md` | teaching-ledger | 3 оси: точность (verdict KNOWLEDGE_EDGE→PASS + verdict_correct), скорость/стоимость (elapsed_ms/cpu/working_set/disk/checkpoint_bytes), качество DPO-шага (Loss/RewardMargin/Accuracy); подозрительно ровные Confidence 0.85/0.86 → проверить leakage/floor. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| `codex-recon-mvp-experiments.md` | Эксперименты v2 | Per-step/per-batch/summary метрики; Accuracy=доля margin>0 (live preference-margin, не held-out); десятки версий v191..v244+ без delta-разбора тренда. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| `codex-recon-mvp-orch-exp.md` | Эксперименты оркестрации (E53 Dynamic Flow): что мерили | Мерили (A) стабильность 12 метрик оркестрации (lost_edge=0, dependency_violation=0, live_external_calls=0) + тесты 12/12 и 997/997 без дрейфа; (B) качественную способность через оракул-контракты; numeric learned-метрик НЕТ (мерили drift=0, не качество обучения). | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| `codex-recon-mvp-all-skills.md` | Корпус скилов all-skills | Строгие гейты измерения: наблюдаемая дельта, 6 типов learning-signal, eval-харнесс измерения; численные гейты завершения в отдельных скилах (не дочитаны). | medium | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |

### Тема F. Граф (GraphEngine / GraphRag / DynamicDomains / топология узлов-рёбер)

| Файл рекона | Заголовок | Краткое | Релевантность | source_category | provenance_status | teacher_access_mode | legal_verdict | source_hash_or_ref | implementation_use | gap_owner |
|---|---|---|---|---|---|---|---|---|---|---|
| `codex-recon-mvp-dyn-flow.md` | Концепт динамического потока | Самомодифицирующаяся сеть маленьких юнитов поверх редактируемого графа потока; узлы=роли, рёбра=маршруты с fan-out/fan-in и петлями; реестры, провенанс, arbiter-aware KNN; топология `dynamic-flow-v2-node-topology.csv`, `dynamic-flow-high-level-topology-v2.mmd`. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| `codex-recon-mvp-orch-exp.md` | Эксперименты оркестрации (E53) | Конвейер 9 узлов node-00..09; per-edge authority/async/multi-output edge contracts ЯВНО не описаны (только агрегатные lost_edge/dependency_violation); `dynamic_flow_runtime_uml.svg`. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| `codex-recon-mvp-local-wiki.md` | Локальное вики-обучение: per-node наборы для UML-графа оркестрации | Граф 9 узлов splitter→...→formatter с fan-out/fan-in и обратным ребром = прообраз DynamicGraph; правила (B=полный вывод не описание, ledger≠training data, f(x)=rephrase, derived-маркировка, банлист) = анти-leakage для DPO. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| `codex-recon-mvp-learning-sets.md` | Структура обучающего сета | Корпус рядов для тех же 9 узлов; B→A-цепочка между узлами; основа для I/O-контрактов и графа (nodes/edges/fan-out/петля арбитров); `01-UML-FRAME.md` = binding frame рёбер (не открыт). | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| `codex-recon-mvp-orch-ext.md` | Расширение оркестратора (wiki над кодом) | Иерархия IOrchestrator→ILogicalFlow→ILogicalUnit; relation/evidence-граф; готовые узлы/рёбра; BM25/SearchEngineFacade/ProjectEvidenceIndex/HybridRetriever/WikiTraceAggregator = детерминистические хелперы. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |

### Тема G. Оркестрация и async-комбинации

| Файл рекона | Заголовок | Краткое | Релевантность | source_category | provenance_status | teacher_access_mode | legal_verdict | source_hash_or_ref | implementation_use | gap_owner |
|---|---|---|---|---|---|---|---|---|---|---|
| `codex-recon-mvp-orch-impl.md` | Дизайн/реализация оркестратора | Топология 31 саб-фаза + 36 переходов; контракт фич E51B; не закрыты per-edge авторитет/hidden-shortcut, lifecycle юнитов, teacher-provenance. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| `codex-recon-mvp-orch-teaching.md` | Обучение оркестратора | 5 обучаемых компонентов (compiler, router, domain splitter/translator, scheme planner) = `ILogicalUnit` внутри `ILogicalOrchestrator`; DynamicGraph many-to-many/fan-in/async-рёбра в планах НЕ описаны — выписать заново по Graph-Style Gate. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| `codex-recon-mvp-orch-exp.md` | Эксперименты оркестрации (E53) | Async-исполнение графа и multi-output/many-to-many edge contracts явно не описаны; есть оракул-контракты сценариев (domain split, граф зависимостей, поимка инвертированной зависимости арбитром). | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| `codex-recon-mvp-local-wiki.md` | Локальное вики-обучение | Протокол 3 сессий, превращающий документ+сессию в пары A→B по каждому узлу; many-to-many и async не зафиксированы; arbiter-selector ОБЯЗАН быть обучаемым юнитом, иначе FORBIDDEN_RUNTIME_SHORTCUT на ребре. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| `codex-recon-mvp-plan-set.md` | Как планируются обучающие сеты | array-узлы (translator/processor/applier/result-translator/arbiter-units) = fan-out по одному элементу; reprocessing-loop; pass-кейс на Node 8. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |

### Тема H. Teacher / provenance / легальный гейт

| Файл рекона | Заголовок | Краткое | Релевантность | source_category | provenance_status | teacher_access_mode | legal_verdict | source_hash_or_ref | implementation_use | gap_owner |
|---|---|---|---|---|---|---|---|---|---|---|
| `codex-recon-mvp-external-sources.md` | external sources learning 2: лицензии/provenance/формат | Реестр 14 семейств; гейт MIT/Apache-2.0 (+CC BY 4.0 с атрибуцией, +RFC/CWE/NIST public domain); BLOCK/QUARANTINE для CC BY-SA/AGPL/GPL и model-output-provenance (ShareGPT/WizardLM/UltraChat/Alpaca/Dolly/FLAN-raw); анти-distillation `external_role=a_side_style_only` — chosen всегда из XIIGen. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| `codex-recon-mvp-local-wiki.md` | Локальное вики-обучение | Сырой лог Codex/GPT-5 (закрытый teacher) в области → Provenance Gate запрещает его вывод как chosen/rejected/training; проверить, P-01..P-16/N-01..N-07 — человеческие промпты Любы (можно) или сгенерировано (нельзя). | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| `codex-recon-mvp-orch-teaching.md` | Обучение оркестратора | Главный legal-блокер: проверить, не является ли «architect response» в XIIGen-сессиях выводом закрытой модели → иначе `DATASET_REBUILD_REQUIRED`. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| `codex-recon-mvp-teaching-ledger.md` | teaching-ledger | Teacher-провенанс отсутствует (нет полей Legal Teacher Gate); есть только провенанс входной подсказки (user_request); артефакты намекают на шаблонный result_text — доказать, что chosen синтезируется раннером. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| `codex-recon-mvp-train-ready.md` | Готовые к обучению датасеты v2 | Все 12 строк `source_status=accepted_model_led_*`, но НЕТ `teacher_access_mode/hosting_provider/model_id/model_license/provider_terms_checked`; закрытый teacher → `DATASET_REBUILD_REQUIRED`. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| `codex-recon-mvp-obuchenie.md` | Обучение языку (RU): метод и сеты | Teacher-провенанс первичных пар (runs/translator-*) не зафиксирован — решающее для допуска в DPO; если closed proprietary → `DATASET_REBUILD_REQUIRED`. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |

### Тема I. Скилы и гайды

| Файл рекона | Заголовок | Краткое | Релевантность | source_category | provenance_status | teacher_access_mode | legal_verdict | source_hash_or_ref | implementation_use | gap_owner |
|---|---|---|---|---|---|---|---|---|---|---|
| `codex-recon-mvp-all-skills.md` | Корпус скилов all-skills | dpo-training-data, planning-dpo-authoring, dpo-triples-to-experiment-pairs, learning-loop-closure, learning-signal-capture, improvement-measurement, static-to-graph-refactoring, graph-rag-integration, flow-design-cycle; формат DPO + 6 сигналов + гейты петли. | medium | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| `codex-recon-mvp-internal-learn.md` | Внутренние обучающие сеты и метод | Гайды teaching-plan v2, authoring v8, examination v15, measurement v6, registry v11; классификационный скил источников; рабочий метод DPO + provenance/anti-shortcut/measurement гейты. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| `codex-recon-mvp-skills-loading.md` | Механизм загрузки скилов в расширение | Двухпроходный skill-loading: детерминистический retriever (BM25/registry) даёт кандидатов, финал решает `SkillSelectionMachine : ILearningMachine`; ResultGapInspector + DPO-эмиссия на 2-м проходе; дрейф каталога (~159 vs живых 164). | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| `codex-recon-mvp-doc-learning.md` | MVP «documentation learning» | Гайды: REGISTRY-GUIDE, PAIR-AUTHORING-GUIDANCE, EXAMINATION-METHOD, EXECUTOR-PROMPT 50-pair; метод обучения снизу вверх D1→D2→D3→D4. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |

### Тема J. Ассеты / данные / тесты

| Файл рекона | Заголовок | Краткое | Релевантность | source_category | provenance_status | teacher_access_mode | legal_verdict | source_hash_or_ref | implementation_use | gap_owner |
|---|---|---|---|---|---|---|---|---|---|---|
| `codex-recon-mvp-lang-translate.md` | MVP language-translate DPO sets, метод, формат | 2 плоскости: translator (4 LoRA RU_EN/EN_RU/HE_EN/EN_HE) + `TranslationRoutingFlow : ILogicalFlow` (R1 direction→R2 topic→R3 register/risk→R4 adapter); on-disk JSONL: translator `source/chosen_translation/rejected_translation/defect_type` (SYNTHETIC_SEED, без `[WRONG:]`), router — вложенный по спеке; измерение reward margin/BLEU-4/chrF. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| `codex-recon-mvp-obuchenie.md` | Обучение языку (RU): метод и сеты | `build_dataset.py` флэттенит 38 батчей в `game-pairs.json` (1849 пар: 1000 RU→EN, 849 EN→RU, 7 типов дефектов); схема `{id,dir,src,ok,bad,type,why}`; 44 курированные RU-пары; детерминированный фронтенд-игра, без ILearning*-уровней. | high (как данные/ETL)  source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| `codex-recon-mvp-internal-learn-2.md` | Внутреннее обучение v2 | Канонические `train-ready-v2/*trainable.jsonl` (V309: 1404 строки, avg accuracy 0.838, reward margin 0.0289, final accuracy 1.0); evidence-отчёты RUN-419-423, RUN-478-479; область зашумлена build-артефактами под claude-export/runner/bin. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| `codex-recon-mvp-train-ready.md` | Готовые к обучению датасеты v2 | DPO JSONL (15 полей: +notes/repaired_from/repair_reason); supplement v197 = строгое подмножество; провенанс `source/source_span/source_status`. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| `codex-recon-mvp-orch-exp.md` | Эксперименты оркестрации (E53) | `LogicalContracts.cs` (round-zip) реализует `ILogicalUnit`, совпадает с иерархией CLAUDE.md; тесты 12/12 E53 и 997/997; clean-batch-001/002/003 — источник пар (провенанс проследить). | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| `codex-recon-mvp-extensions.md` | Архитектура расширений XIIGen (имена API ядра) | Ядро `42.ProjectWikiDomain`; транспорты: in-process `IWikiOrchestrator` (`WikiOrchestratorFactory.Create`) для VS, бинарь `wiki-context-hook.exe` для Claude/Cursor/Codex/VSCode; ядро сейчас детерминированное (honest untrained scaffold). | medium | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| `codex-recon-mvp-first-ext.md` | Ранние заметки по расширениям (Token Optimizer Plugin) | 2 .md-плана; детерминированный плагин из 5 хуков-exe (−50-60% токенов БЕЗ модели); ни DPO, ни обучения, ни графа; единственный объект уровня — `MultiSubPhaseRouterUnit` (Level 2 ILogicalUnit-оболочка с детерминированным нутром). | low | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |

---

## 3. Самое релевантное к ILearningDynamicGraph

Приоритетный список evidence-файлов под конкретную задачу плана (узлы/рёбра графа +
авто-построение DPO + lifecycle + легальный teacher):

1. **`codex-recon-mvp-local-wiki.md`** — самый прямой прообраз DynamicGraph:
   9-узловой граф с fan-out/fan-in и обратным ребром + готовый набор анти-leakage
   правил DPO (B=полный вывод, ledger≠training, f(x)=rephrase, derived-маркировка,
   банлист). Прямо ставит требование: arbiter-selector обязан быть обучаемым
   юнитом на ребре, иначе FORBIDDEN_RUNTIME_SHORTCUT.
2. **`codex-recon-mvp-internal-learn.md`** + **`codex-recon-mvp-internal-learn-2.md`**
   — рабочий референс авто-построения DPO (исполняемые скрипты + канонический
   формат пары `a_side/chosen/rejected/[WRONG:]/target_behavior/domain_focus`,
   обучающий PredicateIds+Confidence на узле/ребре). Берём метод; решаем
   расхождение версий и teacher-provenance.
3. **`codex-recon-mvp-orch-exp.md`** — что и как реально мерили на 9-узловом
   графе (E53): агрегатные edge-метрики (lost_edge=0, dependency_violation=0),
   тесты без дрейфа, оракул-контракты сценариев. Показывает потолок текущего
   evidence (нет numeric learned-метрик и per-edge authority).
4. **`codex-recon-mvp-dyn-flow.md`** — концептуальный предшественник графа +
   locked-граница авторитета D-13 (KNN/priority — guidance/helper, не финальный
   авторитет; arbiter решает приёмку; external AI только advisory) + файлы
   топологии v2.
5. **`codex-recon-mvp-novyy-domen.md`** — как заводится обучаемый домен (копия
   base-весов + fine-tune на DPO-шарде; export/import через 25.ModelServer) —
   рецепт lifecycle узла-исполнителя графа.
6. **`codex-recon-mvp-external-sources.md`** — готовый легальный teacher/provenance
   гейт (для каталога «dpo legal build» это центральный документ): какие
   источники ACCEPT/QUARANTINE/BLOCK и анти-distillation `a_side_style_only`.
7. **`codex-recon-mvp-train-ready.md`** + **`codex-recon-mvp-experiments.md`** +
   **`codex-recon-mvp-teaching-ledger.md`** — как измеряется обучение и где
   живут реальные числа; ключевой факт: Accuracy=доля margin>0 (live), не held-out.
8. **`codex-recon-mvp-orch-impl.md`** + **`codex-recon-mvp-orch-teaching.md`** —
   обучаемая оркестрация (5 юнитов, continue-training, trust-машина) и явный
   пробел: many-to-many/fan-in/async edge-контракты не описаны.
9. **`codex-recon-mvp-learning-sets.md`** + **`codex-recon-mvp-plan-set.md`** —
   per-node формат пар A→B (B одного узла = A следующего) и метод планирования
   сетов (rephrasing ≥2×, pass-кейс на Node 8, array-узлы = пара на элемент).
10. **`codex-recon-mvp-skills-loading.md`** — пример allowed-уровня на ребре
    (детерминистический retriever даёт кандидатов, обучаемая
    `ILearningMachine` решает) — паттерн «helper готовит, обучаемый юнит решает».

Файлы medium/low релевантности (контекст границ, не ядро графа):
`codex-recon-mvp-extensions.md`, `codex-recon-mvp-all-skills.md`,
`codex-recon-mvp-doc-learning.md`, `codex-recon-mvp-lang-translate.md`,
`codex-recon-mvp-obuchenie.md`, `codex-recon-mvp-small-model.md`,
`codex-recon-mvp-orch-ext.md`, `codex-recon-mvp-first-ext.md`.

---

## 4. Сводные пробелы (across-corpus gaps)

Сквозные пробелы, которые корпус НЕ закрывает и которые обязан решить план
ILearningDynamicGraph (агрегировано из gaps всех 23 рекон-файлов):

1. **Legal Teacher Gate не заполнен почти нигде (БЛОКИРУЮЩЕЕ для «dpo legal
   build»).** Поля `teacher_access_mode / hosting_provider / model_id /
   model_license / provider_terms_checked / closed_proprietary_model` отсутствуют
   в рядах данных (`train-ready`, `lang-translate`, `internal-learn-2`,
   `teaching-ledger`, `external-sources` как обязательные поля строки реестра).
   Несколько областей содержат сырой вывод закрытых моделей Codex/GPT-5
   (`local-wiki`, `orch-impl/orch-teaching` XIIGen «architect response»,
   `novyy-domen`/`obuchenie` источники). Без доказательства, что chosen
   синтезируется раннером/из XIIGen-контрактов/из MIT-Apache источников →
   `DATASET_REBUILD_REQUIRED`.
2. **Graph-контракты по Graph-Style Gate отсутствуют везде.** Ни один документ не
   фиксирует many-to-many / multi-output edge / async-исполнение графа /
   владельца авторитета и hidden-shortcut-check на КАЖДОМ ребре. Есть только
   агрегатные `lost_edge=0 / dependency_violation=0` (`orch-exp`). E49/E50
   (OrchestratorExpander/DomainGraphClosure) сейчас финальный детерминированный
   авторитет → потенциальный FORBIDDEN_RUNTIME_SHORTCUT (`orch-impl`).
   `ILearningDynamicGraph` как контракт нигде не описан — это новая надстройка.
3. **Lifecycle обучаемых юнитов не доказан end-to-end.** Нет подтверждённой
   цепочки checkpoint → manifest → `IRemoteUnitLocator` → fresh-load →
   continue-training → ablation. В `teaching-ledger` везде только Step 1
   dpo_step (нет траектории Loss↓/Accuracy↑ Step 2..N); manifest/locator/
   export-import не видны. LoRA-механика (rank, delta_W=A×B, заморозка Bootstrap)
   дана только как идея в диалоге (`novyy-domen`, `orch-teaching`), реализация и
   чекпойнты не подтверждены.
4. **Метрика «качество» подменяется live reward-margin.** Везде Accuracy = доля
   пар с margin>0 (`train-ready`, `experiments`, `teaching-ledger`,
   `internal-learn-2`), а НЕ held-out семантическая точность. Для
   completion-evidence графа нужен held-out + ablation + permutation-invariance.
   KNN/exact-prompt с accuracy 1.000 на held-out = меморизация (`small-model`,
   `orch-teaching` риск exact-prompt leakage).
5. **Анти-leakage / hidden-determinism не проверен на рёбрах.** Подозрительно
   ровные Confidence (0.85/0.86) и learned_unit_confidence (0.94-0.99) в
   `teaching-ledger` — проверить на confidence-floor/детерминизм. В routing-парах
   (`doc-learning`) проверить, что выбор юнита не предсказуем из stack/version-
   подстроки prompt-slice. Детерминистические хелперы (BM25, SearchEngineFacade,
   ProjectEvidenceIndex, HybridRetriever, KNN, predicate-matching) везде должны
   быть помечены eval-only/helper/baseline, не финальный авторитет.
6. **Формат пар не унифицирован.** Сосуществуют: каноническая тройка
   `a_side/chosen/rejected/[WRONG:]` (`internal-learn-2`, `train-ready`);
   плоский translator-формат `source/chosen_translation/rejected_translation/
   defect_type` без `[WRONG:]` (`lang-translate`); supervised-пары
   `Runtime A:/Expected B:/Alternate/Source evidence:` без явных rejected
   (`learning-sets`); игровой `{id,dir,src,ok,bad,type,why}` (`obuchenie`).
   План должен задать единую каноническую DPO-тройку и правило конверсии
   failure/feedback/arbiter-flag → rejected.
7. **Расхождения версий и код↔доки.** train-ready POINTER (V192/3884 или V246/672)
   vs RUN-STATUS (V309/1404) — неясен canonical-файл (`internal-learn-2`,
   `train-ready`). Сталые ссылки раннера на несуществующий `experiments-v2` путь;
   реальный код `11.DPO/DpoTrainer.cs`. Транскрипт `orch-impl` утверждает «проект
   21 = остров» и что PromptDomainClassifier не вызывался в runtime, тогда как v7
   описывает это подключённым — сверить по живому коду в `C:/Projects/llm_mvp_core`.
   Декларированный LoRA Rank 4 vs фактический Rank 2 (`orch-teaching`).
8. **Целевой нейминг не сопоставлен.** В корпусе только
   `ILearningMachine/Flow/Orchestrator/Unit/Pair`; целевые
   `ILearningUnit/Flow/Orchestrator/DynamicGraph` не встречаются. Маппинг
   «узел метода → уровень/класс» и определение, что есть «ребро» (routing) vs
   «узел-исполнитель» (контент), — работа плана (`doc-learning` две DPO-цели;
   `plan-set`/`learning-sets` архитектурно-нейтральны).
9. **common vs user-adaptive leg не закреплён.** Источники local-wiki/learning-sets
   — приватные бизнес-документы → user-adaptive/private, не common-training без
   одобрения (R5); возможно нужен `ILearningMachinePair`. Лицензии исходных
   XIIGen-документов для common-training не верифицированы (`local-wiki`,
   `learning-sets`).
10. **Не открыты тяжёлые/бинарные/zip-артефакты** (защита от перегруза), но они
    релевантны: `35.ExperimentsV2.zip`, `test hypothesys.zip` (`small-model`);
    `FLOW-01-TRAINING-DATASET-PLAN-v4.md`, E51*/E52/E53* схемы доказательств,
    `dynamic-flow-v2-node-topology.csv` (`dyn-flow`); `01-UML-FRAME.md` binding
    frame рёбер (`learning-sets`); `REGISTRY-DECISION-LEDGER.md`,
    `ACCEPTED-MANIFEST.md` (`internal-learn-2`); тела examination v15 / measurement
    v6 / registry v11 (`internal-learn`); per-row чистота .jsonl рядов
    (`external-sources`). Требуют отдельного прицельного бюджета чтения.
11. **Несколько файлов-источников не найдены в своих областях** (доисследовать
    глубже/в zip): `CODE-DOC-SOURCE-REGISTRY-v1.md`, `CODE-DOC-STACK-VERSION-
    AND-CALL-TRANSIT-GATE-v1.md`, `CODE-DOC-DPO-REJECTED-PROMPTS-v1.md`
    (`doc-learning`); `improve algo.drawio.xml` UML (`local-wiki`/`learning-sets`);
    `PLAN-XIIGEN-SKILL-SELECTION-MACHINE-EXTENSION-v1.md` (лежит вне зоны, в
    `extensions reasearch`, `skills-loading`); `XIIGEN-DPO-REGISTRY-GUIDE-v1.md`
    (имя не найдено в `internal-learn-2`).

---

Конец индекса. 23 рекон-файла корпуса `llm mvp` проиндексированы по 10 темам.
