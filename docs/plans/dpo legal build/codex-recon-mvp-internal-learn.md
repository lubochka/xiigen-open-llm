# Codex Recon — MVP «internal learn»: внутренние обучающие сеты и метод

Рекон для плана **ILearningDynamicGraph** (авто-построение DPO для ILearningUnit /
ILearningFlow / ILearningOrchestrator / ILearningDynamicGraph). Evidence-only; это
не разрешение на реализацию. Источник вопроса: «Внутренние обучающие сеты / метод».
Корпус: `C:/Users/lubao/Documents/xiigen/llm mvp/intelrnal learn`.

---

## 1. Что это и где (точные ключевые пути)

Область — это **внутренний (XIIGen-first) DPO-конвейер**: из истории сессий и
артефактов XIIGen строятся DPO-пары (chosen/rejected) для обучения оркестратора-
роутера и его юнитов. Делится на «метод» (гайды-руководства), «исполняемый метод»
(Python-скрипты) и «реестр источников».

**Метод / руководства (актуальные версии, корень папки):**
- `intelrnal learn/XIIGEN-DPO-INTERNAL-TEACHING-PLAN-v2.md` — главный план метода:
  6-шаговый цикл обработки одной сессии (scan → eligibility → compiler-sim →
  pair construction → batch assembly → measurement gate); зоны строк сессий;
  приоритеты P1–P5; гейты.
- `intelrnal learn/DPO-PAIR-AUTHORING-GUIDANCE-ARCH-PLAN-v8.md` (2399 строк) —
  как писать пары: механика reward margin, token-overlap test (`>0.60 = брак`),
  типы пар A–L, D/E (pre-made пары), confusion-таблицы, аннотация `[WRONG: ...]`.
- `intelrnal learn/EXAMINATION-METHOD-ORCHESTRATION-v15.md` — «document→training
  sets, orchestration edition»: source-family decision table, обязательные
  coverage-поля, failure-modes EM-*, DTO-гейты по семействам.
- `intelrnal learn/TEACHING-MEASUREMENT-GUIDANCE-v6.md` — как измерять, что
  обучение реально произошло (3 слоя evidence; DPO loss/margin/accuracy;
  canonicality). Прямо «Grounded in: AdaptiveLogicalOrchestrator.cs,
  DefaultLogicalCapabilityTrainer.cs, DpoTrainer.cs».
- `intelrnal learn/ORCHESTRATOR-TEACHING-PLAN-v5.md` — план обучения оркестратора
  (live-LoRA путь, в отличие от batch-DPO плана v2). (Скан заголовка по индексу;
  не открывался построчно — см. Пробелы.)

**Исполняемый метод (скрипты — это и есть «как строятся сеты»):**
- `intelrnal learn/prepare_internal_learning_sets.py` (1099 строк) — инвентаризация
  всех файлов и вложенных ZIP, эвристическая классификация в семейства реестра,
  авто-извлечение пар для высокоструктурных семейств, запись tier-1..5, eval,
  review-required, repair-required, model-led-authority кандидатов. **Прочитан полностью.**
- `intelrnal learn/augment_internal_learning_sets_tier3.py` — доп. аугментация
  (tier3; маршрутизация B162/B165 prompt-authority в model-led adjudication).
  (Не открывался построчно — см. Пробелы.)
- `intelrnal learn/__pycache__/*.pyc` — артефакты компиляции, не источник.

**Реестр источников / инвентари:**
- `intelrnal learn/XIIGEN-INTERNAL-SOURCE-REGISTRY-v11.md` — нумерованные Type 1–34
  + B01–B12; правило (строка 36–42) о routing-evidence.
- `intelrnal learn/XIIGEN-INTERNAL-SOURCE-REGISTRY-ADDITIONAL-TYPES-v1.md` —
  companion с A01-A51/B13-B43/… и DTO-гейтами.
- `intelrnal learn/XIIGEN-DPO-REGISTRY-GUIDE-v1.md` — три gateway-вопроса
  «новый тип vs новая версия».
- `intelrnal learn/XIIGEN-*-INVENTORY-*.json`, `*UNCOVERED-DOCUMENT-TYPES*` —
  инвентари ZIP/файлов и непокрытые типы (evidence-поверхности, НЕ DPO-строки).
- `intelrnal learn/INTERNAL-INVENTORY-EVIDENCE-HANDLING-2026-05-11.md`,
  `additional file types sessions.md` — handling-заметки.
- `intelrnal learn/internal learning.zip` — упакованный набор источников (бинарь).
- `intelrnal learn/old/` и `intelrnal learn/internal-learning-extracted/` —
  предыдущие версии тех же гайдов (v1..v7 и т.д.) = историческое evidence.

**Внешняя живая база прогона (упоминается, вне этой папки — не открывалась):**
- baseline RUN-416 / V246: 672 строки, weighted accuracy **0.7068**, reward margin
  **0.016892**, домены orchestration 257 / plans 341 / skills 74.
- `…/internal learnin 2/model-led-run-20260508-v1/RUN-STATUS.json` (evidence прогона).
- `…/external sources learning 2/documentation/TRAINING-SET-BUILD-AND-TESTING-RUNBOOK-v1.md`.

---

## 2. Как устроено / работает (кратко)

**Цель метода:** дать оркестратору-роутеру (Layer-0 compiler + router и выше)
обучающие DPO-пары, где `chosen` — правильный ответ/маршрут/порядок, а `rejected` —
правдоподобно-неверный (не отрицание!). Ключевая метрика — `reward_margin =
log P(chosen) − log P(rejected)`; должен быть > 0. Провал калибровки 2026-05-06
(29/40 батчей отрицательны, loss=ln2=0.693) случился из-за шаблона-отрицания
«Do not generate a capability for sub-phase X» (overlap ~0.70).

**6-шаговый цикл (план v2):** scan сессии → eligibility-фильтр (нужен named
artifact / dependency-язык / технич. специфика) → compiler-simulation (9-предикатный
лексикон → `CompiledPredicateIds`, `TrainingText` строится из `compiledText`, НЕ
из сырого промпта) → конструирование пары (chosen+rejected+`[WRONG:]`) → сборка
батча (квоты: TypeA≥40%, TypeB≥20%, TypeC≥30% + 3 Stage-9 corruption строки) →
measurement-gate (reward_margin_mean≥0, loss<0.693, overlap-scan).

**Типы пар:** A (ARCH: граница-решение vs реализация), B (PLAN: правильный
`depends_on` порядок vs реверс), C (неверный домен), D (locked-decision: принятое
vs отвергнутая альтернатива — pre-made), E (актуальный контракт vs устаревший —
pre-made, самый низкий overlap 0.05–0.15), G/H (gap-fix / generated-code-debug),
I/J/K/L (каталоги/диаграммы-топология/config-schema/IaC+DB). Correction-events
(H0/A0/H1/A1 из истории) — приоритет P1, готовые пары без конструирования.

**Исполняемый метод (`prepare_internal_learning_sets.py`):**
- `iter_filesystem_sources` + `iter_zip_entries` — рекурсивный обход (ZIP до 5
  уровней), self-ingestion исключается, `external sources` помечается
  `external_excluded`.
- `classify_source` — эвристика по имени/расширению → `family()` запись
  (family/registry_status/target_type/priority_tier). Root-prompt/user-intent →
  `model_led_adjudication_required` (НЕ авто-демоут).
- `extract_*` (b160/b144/b85/b86/b97/b157/b158/b161/b131/heldout/b115/
  wrong_correct) — реальные построители пар: тянут `chosen` из канонического
  источника, `rejected` — типовой неверный (pre-renumber id, false_pass,
  evidence_free и т.д.), тип в `WRONG_ANNOTATION_EXPLANATIONS`.
- `normalize_pair_for_current_run` + `pair_validation_errors` — жёсткие гейты
  2026-05-11: `chosen != rejected`, нет mojibake, train_ready обязан нести
  `[WRONG: ...]`; иначе → `repair_required`.
- Выход: `training-pairs-all.jsonl`, `training-pairs-tier-{1..5}.jsonl`,
  `eval-heldout.jsonl`, `…-review-required`, `…-repair-required`,
  `model-led-authority-candidates.jsonl`, `manifest.json`, отчёты.

**Измерение (guidance v6):** строка обучения принимается (`ACCEPT_ROW`) только
если verdict=PASS **и** `dpo_reward_margin≥0` **и** `dpo_loss` уходит ниже 0.685
(≥0.693 = брак, random-guess). Кумулятивность: одна topology/flow/unit/trainer/
orchestrator-state живёт через все строки; каждый вызов учит 1 строку и
ре-тестит 2–3 прошлых.

---

## 3. Релевантность к ILearningDynamicGraph

**Высокая — это прямой прототип «авто-построения DPO» и измерения для графа/оркестрации.**

- **Авто-построение DPO (ядро задачи):** `prepare_internal_learning_sets.py` уже
  реализует автогенерацию chosen/rejected из артефактов — это рабочий референс
  «как машинно строить пары» для уровней Unit/Flow/Orchestrator/Graph.
- **Граф / оркестрация / роутинг:** реестр v11 (стр. 36–42) и план v2 (Priority 5)
  задают **целевой обучаемый объект** для роутинга:
  `OrchestrationRoutingFlow : ILogicalFlow<LogicalOrchestratorRequest>` или
  поведение `AdaptiveLogicalOrchestrator` (compiled document + routing decisions +
  knowledge-edge state + flow/factory resolution + final verdict). Это прямой
  аналог рёбер/узлов ILearningDynamicGraph и «кто владеет финальной authority на
  ребре» из скил-гейта DynamicGraph-реконструкции.
- **Уровни иерархии:** A-side для уровня = полный `LogicalOrchestratorRequest`;
  B-side = маршрутные решения. Совпадает с input/output-контрактами рёбер графа.
- **Измерение (DynamicGraph metrics):** guidance v6 даёт готовую метрику-рамку
  (loss/margin/accuracy + overlap + canonicality + ресурсы) для оценки, что
  обучение узла/ребра реально произошло.
- **Teacher-provenance:** correction-events = **человеческие** правки (H1), не
  выход закрытой модели → проходят Legal Teacher/Provenance Gate как
  legal-teacher данные. Это важный «легальный» источник для DPO графа.
- **Stage-9 corruption / knowledge-edge:** corruption-строки (wrong-predicate,
  ambiguous-predicate, knowledge-edge-suppressed) — готовый паттерн негативов и
  «края знаний» для рёбер графа (узел/флоу не зарегистрирован → KNOWLEDGE_EDGE).
- **Анти-shortcut совпадение:** правило 2026-05-11 №1 («скрипты не решают
  semantic authority по имени файла/пути») = ровно то же, что
  `FORBIDDEN_RUNTIME_SHORTCUT` / запрет детерминистической финальной authority
  в скиле. Полезно перенести в гейты плана графа.

---

## 4. Классификация (уровень обучаемой архитектуры vs детерминистический хелпер)

По правилам `trainable-architecture-decision-SKILL.md` (только ILearningMachine /
ILogicalOrchestrator / ILogicalFlow / ILogicalUnit / ILearningMachinePair —
обучаемые уровни; всё прочее — внутренняя деталь).

| Артефакт корпуса | Классификация | Почему (со ссылкой на правило) |
|---|---|---|
| `prepare_internal_learning_sets.py` (`classify_source`, `extract_*`, регэкспы по именам) | **Внутренний детерминистический хелпер** — corpus/fixture builder, НЕ обучаемый уровень | Скил: детерминистические helper'ы допустимы только для «corpus labeling, audit, fixture generation, evaluation setup». Эвристика по filename/regex — именно это; её нельзя делать рантайм-authority роутинга. Сам метод это и фиксирует (правило 2026-05-11 №1). |
| `infer_canonicality()` / `canonical_confirmation_for()` (canonical по подстроке пути) | **Детерминистический хелпер, слабый/leaky** | Решает canonicality по подстроке `final/old/stale` в пути — это path/provenance-сигнал. По Minimum Breadth Challenge — кандидат на «source/path/provenance leakage». Допустимо как provenance-пометка, НЕ как обучающий сигнал. |
| `OrchestrationRoutingFlow : ILogicalFlow<LogicalOrchestratorRequest>` (целевой объект роутинга) | **ILogicalFlow** (обучаемый уровень) | Это явный `ILogicalFlow<TInput>`; реестр требует учить именно его, а unit-only строки помечать `PAIRSET_READY_ROUTING_FLOW_PENDING`. Соответствует уровню Flow графа. |
| `AdaptiveLogicalOrchestrator` (поведение) | **ILogicalOrchestrator** (обучаемый уровень) | Оркестрация маршрута = Level 4; владеет финальным routing-verdict. |
| `PromptRoutingInputCompilerUnit`, `MultiSubPhaseRouterUnit`, Layer-0-A/0-B compiler/router | **ILogicalUnit-кандидаты, но station-level evidence only** | Реестр v11 прямо: это «station-level evidence only»; сами по себе НЕ доказывают «routing learned». То есть это юнит-детали внутри Flow/Orchestrator, не финальная authority. |
| `DefaultLogicalCapabilityTrainer`, `DpoTrainer` | **Инфраструктура обучения** (тренер), не уровень инференса | Тренер обновляет состояние обучаемого уровня; в иерархию инференс-контрактов не входит (A-LM-02: Train* только на ILearningMachine). |
| 9-предикатный лексикон / `CompiledPredicateIds` (keyword→predicate) | **Детерминистический keyword-matching хелпер** | Скил: «regex/keyword matching … не паттерн архитектуры»; допустимо как фичо-источник/симуляция внутри уровня, не как обучаемый уровень и не как финальный роут. |
| DPO-строки (chosen/rejected/`[WRONG:]`) в `training-pairs-*.jsonl` | **Обучающие данные (датасет)** | Это training data lifecycle-evidence, не код-уровень. Оценивать по Required Dataset Table (KEEP_FOR_TRAINING / EVAL_ONLY / QUARANTINE / REWRITE_TO_DPO). |
| Inventory/ledger/status/coverage `*.json(l)`, `*.md` отчёты | **Evidence-поверхности, НЕ DPO-строки** | Правило 2026-05-11 №4: «ledger, status, audit, inventory, progress — evidence surfaces; не DPO-строки». |

**Вывод по классификации:** корпус = легальный датасет + детерминистический
fixture-builder. Обучаемые уровни (Flow/Orchestrator) здесь только как **целевые**
объекты обучения (что учить), а не реализованы в этой папке. Для плана
ILearningDynamicGraph материал годен как (а) источник легальных DPO-пар и метода
их автогенерации, (б) образец гейтов provenance/anti-shortcut/measurement.
Любое прямое использование `classify_source`/canonicality-эвристики как рантайм-
решения было бы `FORBIDDEN_RUNTIME_SHORTCUT`.

---

## 5. Пробелы / что доисследовать

- **Не прочитаны построчно** (только заголовок/индекс — защита от перегруза, могут
  менять детали для плана): `EXAMINATION-METHOD-ORCHESTRATION-v15.md` (тело
  source-family table и EM-failure-modes), `TEACHING-MEASUREMENT-GUIDANCE-v6.md`
  (Layer 2/3 routing+graph метрики), `XIIGEN-INTERNAL-SOURCE-REGISTRY-v11.md`
  (тело Type 1–34), `augment_internal_learning_sets_tier3.py`,
  `ORCHESTRATOR-TEACHING-PLAN-v5.md`, companion `…-ADDITIONAL-TYPES-v1.md`.
- **Нет рантайм-кода уровней** в этой папке: `AdaptiveLogicalOrchestrator.cs`,
  `DefaultLogicalCapabilityTrainer.cs`, `DpoTrainer.cs`,
  `OrchestrationRoutingFlow`, `PromptRoutingInputCompilerUnit`,
  `MultiSubPhaseRouterUnit` только **упомянуты** — фактические сигнатуры/контракты
  для графа надо смотреть в `C:/Projects/llm_mvp_core` (21.AgentInfrastructure/
  Logical, 15.OrchestratorModel) — отдельный рекон.
- **Живой прогон вне папки:** `…/internal learnin 2/model-led-run-20260508-v1/
  RUN-STATUS.json` и runbook не открывались — фактические числа/гейты baseline
  стоит подтвердить там.
- **Граф как объект:** материал описывает Flow/Orchestrator роутинг, но **явной
  ILearningDynamicGraph-модели (узлы/рёбра/fan-out/fan-in/many-to-many)** в корпусе
  нет — это новая надстройка; из корпуса берём метод DPO и provenance-гейты, а
  graph-контракты надо проектировать (по DynamicGraph-Reconstruction-Gate скила).
- **Мojibake-риск:** гайды содержат битую кодировку (`â€”` и т.п.) в самих .md —
  это косметика документов; на DPO-строки скрипт ставит anti-mojibake-гейт.
- **Дубли версий** в `old/` и `internal-learning-extracted/` — при планировании
  брать только корневые актуальные версии (v8/v15/v6/v11/v2), остальное history.
