# Recon: teaching-ledger — измерение скорости/точности/качества обучения

Корпус: `mvp` · slug: `mvp-teaching-ledger` · роль: рекон-аналитик (Opus 4.8 MAX)
Вопрос: как в teaching-ledger меряют скорость / точность / качество обучения.

---

## 1. Что это и где (точные ключевые пути)

Область: `C:/Users/lubao/Documents/xiigen/llm mvp/teaching-ledger/`. Это **выходные артефакты раннера обучения (teaching runner)**: для каждой пользовательской подсказки (prompt) система сначала пробует ответить «как есть» (baseline), затем обучает/регистрирует юнит+флоу и повторяет (retry/teaching), и записывает измерения. Markdown-документации нет (Glob по `**/*.md` пуст) — измерение читается только из JSON/JSONL.

Две независимые сессии с одинаковой схемой:

- **XIIGen продакшн-сессия (first-party prompt):**
  - `xiigen-sessions/2026-05-05-last-product-session-internal-first/teaching-ledger.jsonl` — построчный леджер (1 строка = 1 обученная способность).
  - `.../cycle-summary.json` — полный разбор одного цикла (baseline vs teaching_and_retry, router_output, registered_units/flows).
  - `.../checkpoints/xiigen-cycle-001/3-3/dpo_step_000001.json` — **DPO-метрики шага обучения**.
- **Legacy claude-export батч (внешний источник):**
  - `claude-export/batch-0000/legacy-docs-skills-from-old-code/teaching-ledger.jsonl` — 7 строк, та же схема + поле `source_bundle`.
  - `.../cycle-summary-cumulative-20260505173713.json` — кумулятивный прогон с `registry_before`/`registry_after` и `decision: ACCEPT_ROW`.
  - множество `checkpoints*/<row>/<phase-sub>/dpo_step_000001.json` (после lexicon / anchor-filtering / cumulative — это варианты предобработки одного и того же ряда).

Примечание о перегрузе: бóльшая часть путей в этой папке — это бинарные артефакты сборки раннера (`runner/bin/.../e_sqlite3.*`, `Microsoft.CodeAnalysis*.dll`, `*.resources.dll`) и **к измерению обучения не относятся** — игнорировать.

---

## 2. Как устроено/работает (кратко) — три оси измерения

Измерение разнесено по трём уровням файлов:

**A. Точность/качество обучения (correctness) — на уровне ряда (teaching-ledger.jsonl, cycle-summary):**
- `baseline_verdict` — вердикт без обучения. Характерное значение `KNOWLEDGE_EDGE` («модель не знает», `selected_flow_id: knowledge-edge-missing-flow`, `knowledge_edge_count: 1`).
- `retry_verdict` / `teaching_and_retry.verdict` / `actual_verdict` — вердикт после обучения, ожидается `PASS`, `knowledge_edge_count: 0`.
- `expected_verdict` vs `actual_verdict` + булево `verdict_correct` — прямая проверка «обучение дало правильный ответ» (ledger XIIGen, строка 1).
- Качество = **переход** `KNOWLEDGE_EDGE → PASS` (transition_type `..._learning_cycle`), а не абсолютный балл.

**B. Скорость/стоимость обучения (cost) — измеряется как baseline vs teaching:**
- `baseline.elapsed_ms` (~40–76 мс) против `teaching.elapsed_ms` / `teaching_elapsed_ms` / `teaching_and_retry.elapsed_ms` (~2.5–14.7 тыс. мс) и `TrainingTimeMs`/`TrainingTimeMs`(в `registered_this_row`).
- Ресурсы: `cpu_percent`/`process_cpu_percent`, `working_set_delta_mb`, `managed_allocation_delta_mb`, `disk_delta_mb`, `checkpoint_bytes`. То есть «скорость» = время + потребление CPU/RAM/диска на обучение одного ряда, сравнённое с дешёвым baseline.

**C. DPO-сигнал обучения (собственно градиентный шаг) — `dpo_step_000001.json`:**
- Поля: `Step`, `Loss` (~0.69 ≈ ln2 на шаге 1), `ChosenReward`, `RejectedReward`, `RewardMargin` (chosen − rejected), `Accuracy` (1.0 = chosen получил больший reward, чем rejected).
- Это и есть метрика «учится ли модель»: растущий `RewardMargin` и `Accuracy`, падающий `Loss`. Хранится по пути `<checkpoints>/<row>/<phase>-<sub>/dpo_step_NNNNNN.json` — по одному на ветку (phase-sub) ряда.

**D. Ёмкость/рост (capacity) — `cycle-summary*cumulative*.json`:**
- `registry_before` / `registry_after` с `flow_count`, `unit_count`, `generated_flow_count`, `learned_unit_count` (видно рост 31→36→41→42 флоу, 0→5→10→11 юнитов).
- `decision: ACCEPT_ROW` — гейт принятия ряда. В ledger XIIGen есть `decision.trust_state: Probationary`, `outcome: promoted_to_probationary` — **жизненный цикл доверия к обученному юниту**.

**E. Маршрутизация/уверенность (как выбирается, чему учить):**
- `router_output[]`: `PredictedPhaseId`/`PredictedSubPhaseId`, `Confidence` (0.85–0.92), `EvidencePredicates`, `KnowledgeEdge`, `NearestWrongSubPhaseScore` (зазор до ближайшей неверной под-фазы).
- `compiler_output.CompiledPredicateIds` (ARCHITECTURE/DOCUMENT/IMPLEMENT/TEST/UI/LEARNING…) + `Confidence` — предикаты, по которым роутятся под-фазы.
- В метриках веток — `learned_unit_confidence` (0.94–0.99) и `learned_unit_selected: True`.

---

## 3. Релевантность к ILearningDynamicGraph

Высокая. Это готовый, уже работавший **прецедент измерения авто-построения DPO и динамического графа способностей** — ровно предмет планируемого проекта:

- **Граф/оркестрация:** один prompt → fan-out на несколько под-фаз (`branch_count` 1–7, массив `branches[]`), каждая ветка = узел `generated-capability-flow-<phase>-<sub>` с собственными метриками и `OverallVerdict`. Это прямой аналог узлов/рёбер ILearningDynamicGraph и many-to-many выходов — сразу даёт схему `fan_out`/`fan_in`, контракты выхода ветки и async-исполнение (Graph-Style Reconstruction Gate скила).
- **DPO/обучение:** `dpo_step_*.json` = эталон формата метрик обучаемого юнита (Loss/RewardMargin/Accuracy) для гейта «trained-feature vs honest-untrained». `teaching-ledger.jsonl` = формат ряда «до/после».
- **Измерение:** готовый набор полей для требования «скорость/точность/качество»: точность=verdict-переход+verdict_correct; скорость=elapsed/cpu/ram/disk baseline-vs-teaching; качество обучения=DPO RewardMargin/Accuracy; рост=registry before/after; доверие=trust_state/ACCEPT_ROW.
- **Teacher/provenance (юридический гейт DPO):** в данных провенанс **есть, но слабый и НЕ teacher-провенанс**. Есть `SourceTier: runtime_prompt`, `LicenseOrAuthority: user_request`, `DocumentType: user_prompt`, `source_document`/`source_bundle`. Это происхождение *входной подсказки*, а не *учителя-модели*. Полей из Legal Teacher Gate скила (`teacher_access_mode`, `hosting_provider`, `model_id`, `model_license`, `provider_terms_checked`, `closed_proprietary_model`) — **нет ни в одном файле**. Важно для папки `dpo legal build`: chosen/rejected тут синтезированы раннером (`result_text` = шаблон «Generated capability for sub-phase …»), не выходом закрытой модели — но это надо подтвердить по коду раннера, а не по артефактам.

---

## 4. Классификация (правила trainable-architecture-decision-SKILL.md)

Это **артефакты-данные (output леджера), а не исполняемый код** — поэтому классифицирую *роль* механизмов, отражённых в полях, относительно разрешённых уровней. Код раннера здесь не читался (только его бинарники), поэтому это вывод по foll-ам, требующий подтверждения по исходникам.

| Поле/механизм в артефакте | Что отражает | Классификация | Почему (правило скила) |
|---|---|---|---|
| `registered_units[]` (`IsLearned:true`, Input/OutputType, `TrustState`) | обучаемый юнит-способность | кандидат на **ILogicalUnit** | по сути обучаемый юнит с типизированным in/out и lifecycle (trust); «allowed level» если есть checkpoint/export/import/locator |
| `registered_flows[]` / `generated-capability-flow-*` (`branch_count`, `branches[]`, fan-out) | маршрут/последовательность веток | кандидат на **ILogicalFlow** (а оркестрация fan-out — **ILogicalOrchestrator**) | routing/sequence должен жить на orchestration-уровне (вердикт `NEEDS_ILOGICALFLOW_OR_ORCHESTRATOR_WRAPPER` если нет) |
| `dpo_step_*.json` (Loss/RewardMargin/Accuracy) | сигнал обучения DPO | **lifecycle-evidence обучаемого уровня** (не отдельный уровень) | это «learning signal + evaluation» из чек-листа lifecycle, а не архитектурный паттерн |
| `router_output[].Confidence`, `NearestWrongSubPhaseScore`, `learned_unit_confidence` | скор уверенности выбора под-фазы | **внутренний детерминистический/скоринговый хелпер ИЛИ модельный компонент** | scorer/selector «не паттерн архитектуры»; допустим только *внутри* allowed level. Нужен audit на скрытый детерминизм и confidence-floor (Minimum Breadth Challenge) |
| `compiler_output.CompiledPredicateIds` (keyword/predicate match) | предикаты по тексту | **внутренний детерминистический хелпер** | regex/keyword/dictionary matching прямо назван «implementation detail only», финальной trainable-властью быть не может |
| `baseline_verdict=KNOWLEDGE_EDGE`, `decision=ACCEPT_ROW`, `trust_state` | гейт honest-untrained и приёмки | механизм измерения/жизненного цикла | поддерживает разделение `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` vs `PASS_AS_TRAINED_FEATURE` |

Главное предупреждение для плана: на рёбрах графа (router→flow→unit) **финальную власть нельзя отдавать `Confidence`/predicate-match** — это `FORBIDDEN_RUNTIME_SHORTCUT`, если он не помечен eval-only/baseline и не покрыт ablation/no-shortcut тестом. `learned_unit_confidence: 0.99` почти на каждом ряду — потенциальный признак «уверенность нарисована, не обучена»: требует проверки на target-label leakage и confidence-floor.

---

## 5. Пробелы / что доисследовать

1. **Код раннера не читался** (только бинарники `runner/bin/...`). Как именно вычисляются `Confidence`, `RewardMargin`, выбор ветки и `ACCEPT_ROW` — подтвердить по исходникам раннера (вероятно в `llm_mvp_core`, напр. `15.OrchestratorModel` / model-server), а не по артефактам.
2. **Teacher-провенанс отсутствует** — нет полей Legal Teacher Gate. Для `dpo legal build` критично выяснить: chosen/rejected генерятся раннером (шаблон) или приходят от внешней модели. Сейчас данные указывают на синтез раннером (`result_text` шаблонный), но это не доказано.
3. **Только `dpo_step_000001.json` (Step 1)** во всех проверенных ветках — нет траектории обучения (Step 2..N). Невозможно увидеть динамику Loss↓/Accuracy↑/RewardMargin↑ во времени; неясно, обучение это или один-шаговая инициализация. Доисследовать: есть ли многошаговые чекпойнты где-либо в корпусе.
4. **Подозрительно ровные значения** `Confidence` (0.85/0.86) и `learned_unit_confidence` (0.94–0.99) — проверить, не константы/детерминированный скоринг ли это (анти-masquerade, ablation).
5. **Связь с export/import/locator/manifest не видна** в этих файлах (есть `checkpoint_bytes`, но нет manifest/locator). Для требования lifecycle ILearningDynamicGraph (fresh-load, continue-training, share) доисследовать `25.ModelServer`/`IRemoteUnitLocator` в `llm_mvp_core`.
6. **Варианты предобработки** (`after-lexicon`, `after-anchor-filtering`, `cumulative`) намекают на пайплайн фильтрации DPO-пар — стоит изучить отдельно, как фильтрация влияет на качество (это прямой рычаг качества обучения).
