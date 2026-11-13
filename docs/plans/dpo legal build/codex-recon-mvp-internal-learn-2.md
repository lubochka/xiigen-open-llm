# Рекон: «Внутреннее обучение v2» (internal learnin 2) для плана ILearningDynamicGraph

Корпус: mvp · slug: mvp-internal-learn-2 · роль: рекон-аналитик (evidence для плана, НЕ разрешение на реализацию).
Только реально найденные пути. Целевая область сильно зашумлена бинарными артефактами сборки
(`...\claude-export\...\legacy-docs-skills-from-old-code\runner\bin\Debug\net9.0\runtimes\...`),
поэтому читались только обзорные/смысловые файлы (защита от перегруза).

---

## 1. Что это и где (точные ключевые пути)

Корень: `C:/Users/lubao/Documents/xiigen/llm mvp/internal learnin 2`

Это рабочая область построения DPO‑корпуса для ВНУТРЕННЕГО (orchestration/planning/skills) обучения малой модели. Три верхних каталога:

- `run-20260508-internal-dpo-v1/` — ПЕРВЫЙ (детерминистический) прогон. Явно помечен как НЕ финальный.
  - `README.md` — счётчики источников (filesystem_sources 545470, total 1044943, zip_member 499473), счётчики пар по семействам B97/B86/B162(=40361)/… и итог `training-pairs-all.jsonl`: 49367 записей; `undefined-type-candidates.jsonl`: 607568.
  - `NOT-FINAL-DETERMINISTIC-INVENTORY.md` — самокритика: «код использован как классификатор — неверный инструмент»; эти файлы = только инвентарь/кандидаты/RAG, не финальный train‑ready.
  - `coverage-report.md`, `FINAL-VALIDATION-SUMMARY.md`, `undefined-type-candidates.md` (119556 б), `B162-AND-UNDEFINED-EXPLANATION.md`.
- `model-led-run-20260508-v1/` — ВТОРОЙ, актуальный, MODEL‑LED прогон (это и есть «v2» по смыслу). Главное здесь.
  - `00-METHOD.md` — метод: код только ищет/распаковывает/считает/копирует; решения о типе семейства, new‑type vs subtype, каноничности, A/B‑границе, train‑ready принимает МОДЕЛЬ по `XIIGEN-DPO-REGISTRY-GUIDE-v1.md`.
  - `RUN-STATUS.md` — последний прогон RUN‑479 / V309; версия `TRAIN-READY-V309-V307-PLUS-V308-PLAN-HEAD16`; rows 1404; avg accuracy 0.838, avg reward margin 0.0289, final accuracy 1.0; источник + SHA256.
  - `train-ready-v2/` — КАНОНИЧЕСКИЙ выход. Десятки `dpo-pairs-domain-v2xx-...-trainable.jsonl` (domain_focus: orchestration/plans/skills), плюс external/internal‑guide варианты; текущий рабочий файл V309: `dpo-pairs-domain-v309-v307-plus-v308-plan-head16-trainable.jsonl`. Указатели: `CURRENT-TRAIN-READY-POINTER.md` (пишет V192/3884 строк — РАСХОЖДЕНИЕ с RUN‑STATUS V309) и `.json`. Подкаталоги `ablation-v288b/…v306/` (head/tail/one‑PLAN ablation срезы), `deep-extracted/` (b1005‑orchestrator‑teaching, b1025‑teaching‑ledger‑runtime‑evidence, flow01‑48 session‑chain corpus и т.д.).
  - `evidence/RUN-419..479-*-ORCH-PLAN-TRAINING-REPORT.md` — серия отчётов раннера обучения (метрики DPO, ablation‑выводы).
  - Реестр/решения: `REGISTRY-DECISION-LEDGER.md` (~435 КБ), `ACCEPTED-MANIFEST.md`, `ADJUDICATION-B*-v1.md`, `CANDIDATE-REGISTRY-GUIDE-*`, `_stage-train-ready-v191-all-guides/`.
  - `accepted/`, `review/`, `source-surfaces/`, `01-foundational-inputs/`, `02-session-user-requests/RUN-01..37`.
- `learning runs internal sets/` — оболочки прогонов `run222.ps1 … run227.ps1` (PowerShell‑скрипты запуска; не читались как код).

---

## 2. Как устроено/работает (кратко)

Пайплайн в два слоя, разделение жёстко зафиксировано в `00-METHOD.md`:

1. Детерминистический инвентарь (v1): обход трёх корней (`C:\Projects\xiigen mvp`, `C:\Projects\llm_mvp_core`, `C:\Users\lubao\Documents\xiigen`), распаковка ZIP, классификация по семействам B*, генерация кандидатных пар по тирам. Помечен НЕ финальным.
2. Model‑led решения (v2): модель типизирует семейства (3 gateway‑вопроса: уникальность структурного шаблона / consumer‑action / A‑side вопроса), фиксирует каноничность, target‑behavior, DPO‑механизм и пишет ledger; только после ревью строки попадают в `accepted/` и в `train-ready-v2/*trainable.jsonl`.

Формат DPO‑пары (подтверждено чтением V309‑trainable, первая строка): поля
`id, origin, normalized_family, bucket, tier, pair_kind, source_status, source, source_license_state, a_side, chosen, rejected, target_behavior, notes, domain_focus`.
Пример (pair_kind=`adaptive_routing_layer_pair`, source = `external sources learning 2\new sets\Layer-0-A-PromptCompiler.md`):
- `a_side`: рантайм user_request + флаги `UseLearnedUnitWhenAvailable: true`, `UseKnowledgeEdgeDetection: true`.
- `chosen`: конверт скомпилированного документа + `[unit result envelope] Confidence: 0.92 PredicateIds: ["ARCHITECTURE"]`.
- `rejected` + `[WRONG: ...]`: тот же вход, но `PredicateIds: ["IMPLEMENT"]`, с разбором, почему IMPLEMENT‑триггеры тут не сработали и почему верен ARCHITECTURE.

Эмпирика обучения (из ORCH‑PLAN отчётов): малая модель ПЛОХО учится на длинной flow‑прозе и «generic compact» (V249/V250 — reject, V250 даже вредна, margin отрицательный); ХОРОШО — на конкретном низко‑сложностном контрасте (V251/V252 acc 0.86–0.88): A‑side = один практический запрос; chosen = domain+sub_phase+короткая причина+gate/order; rejected = соседний неверный маршрут/ADR‑альтернатива/неверная currentness/преждевременный implement; `[WRONG:]` = короткое функциональное последствие. Зафиксировано правило: «unit‑test pass rate ≠ DPO accuracy»; валидный отчёт обязан идти от раннера (rows, batch, loss, accuracy, reward margin, CPU, RAM‑delta, elapsed). Ablation по одной строке отбраковывает «schema‑shaped» строки (V308: исключены поздние 3.5/3.6 и 21‑module schema строки как вредные).

---

## 3. Релевантность к ILearningDynamicGraph (DPO/обучение/граф/оркестрация/измерение/teacher‑provenance)

ОЧЕНЬ ВЫСОКАЯ. Это фактически уже существующий, прогнанный через раннер DPO‑корпус именно для orchestration/planning‑слоя, т.е. для тех решений на узлах/рёбрах графа, которые ILearningDynamicGraph должен авто‑строить и обучать.

- DPO/обучение: готовые `*trainable.jsonl` с полями chosen/rejected/`[WRONG:]`/target_behavior + метрики раннера и checkpoint (V309 ~4.77 МБ) — прямой образец формата пар и измерения (accuracy, reward margin, loss, ablation).
- Граф/оркестрация: пары `adaptive_routing_layer_pair` и источник `Layer-0-A-PromptCompiler` описывают слой‑0 компиляции промпта → предикаты с Confidence — это решение «ребра» маршрутизации (predicate routing) и кандидат на узел графа. domain_focus делит корпус на orchestration / plans / skills (V309: 501 / 829 / 74).
- Измерение: `RUN-STATUS.md` + `evidence/*ORCH-PLAN*` дают готовую методику gate (avg/final accuracy, reward margin, CPU/RAM/elapsed, ablation per‑row) — пригодно как Gate‑B для обучаемых узлов.
- Teacher‑provenance / лицензии: строки внешнего происхождения помечены `source_status=external_license_checked`, `source_license_state`, `attribution_required`; внутренние правила (`00-METHOD`, NOT‑FINAL) разделяют train‑ready vs RAG/limited‑authority. Это совпадает с Legal Teacher/Provenance Gate скила (см. §4). НО: происхождение самих chosen/rejected (кто их сгенерировал — модель‑учитель?) в прочитанных файлах НЕ раскрыто — пробел.

---

## 4. Классификация (по `trainable-architecture-decision-SKILL.md`)

Это набор данных и отчётов, не рантайм‑код проекта, поэтому классифицирую по тому, ЧТО пары обучают и какие механизмы упоминают.

- Целевая способность пар = обучаемый компилятор‑промпта / адаптивный слой маршрутизации, выдающий PredicateIds + Confidence на узле и выбор маршрута на ребре. Корректный уровень‑владелец: `ILogicalUnit` (юнит компиляции/предиката) внутри `ILogicalFlow`/`ILogicalOrchestrator` (последовательность/маршрут). В терминологии плана — ILearningUnit/ILearningFlow/ILearningOrchestrator/ILearningDynamicGraph; в репо канонично это `ILogicalUnit/ILogicalFlow/ILogicalOrchestrator/ILearningMachine` (см. CLAUDE.md, A‑LM‑02). Это допустимые уровни по скилу (раздел «only allowed trainable architecture levels»).
- Флаг `UseKnowledgeEdgeDetection` и `[unit result envelope] Confidence` ⇒ граф с обнаружением рёбер и confidence на ребре. Скил (Graph‑Style/DynamicGraph Reconstruction Gate) требует: для КАЖДОГО ребра назвать владельца финальной власти (детерминистический хелпер vs trainable unit/flow/orchestrator) и проверить скрытый детерминистический shortcut на ребре (`FORBIDDEN_RUNTIME_SHORTCUT`, если не помечен eval‑only). В данных это НЕ зафиксировано — план должен это реконструировать.
- Предикат‑роутинг по «триггер‑фрагментам» (как в `[WRONG:]`: «IMPLEMENT требует ‘implement the’/‘integration layer’») — это keyword/pattern matching. По скилу это ВНУТРЕННИЙ ДЕТЕРМИНИСТИЧЕСКИЙ ХЕЛПЕР, допустимый ТОЛЬКО как baseline/eval/candidate, помеченный явно; он НЕ может быть финальной рантайм‑властью семантического маршрута без обучаемого юнита и lifecycle‑доказательств. DPO‑пары как раз и нужны, чтобы обучаемый юнит заменил/перекрыл этот детерминизм — это согласуется со скилом, но требует ablation/no‑shortcut gate.
- Сам корпус как данные: по `00-METHOD`/NOT‑FINAL — v1 = `KEEP_FOR_BASELINE_ONLY`/`KEEP_FOR_EVAL_ONLY`/инвентарь; v2 model‑led `accepted/*` и `train-ready-v2/*trainable.jsonl` = `KEEP_FOR_TRAINING` (с оговорками per‑row). «Schema‑shaped» строки (V308 ablation) → де‑факто `QUARANTINE_DO_NOT_TRAIN`.
- Legal Teacher / Provenance Gate: внешние строки помечены license‑checked; но если chosen/rejected порождены ЗАКРЫТОЙ проприетарной моделью‑учителем (Claude/OpenAI/Gemini), скил даёт безусловный `DATASET_REBUILD_REQUIRED` для DPO‑targets. Происхождение генерации в прочитанном НЕ доказано — обязательная проверка для плана.

Вывод классификации: `OK_ALLOWED_LEVEL` для целевой архитектуры (юнит/флоу/оркестратор/граф) ПРИ условии (а) явной разметки детерминистического predicate‑matching как baseline/eval и его перекрытия обучаемым юнитом с ablation, (б) реконструкции владельца власти на каждом ребре графа, (в) подтверждения teacher‑provenance пар.

---

## 5. Пробелы / что доисследовать

- Расхождение указателей версии: `train-ready-v2/CURRENT-TRAIN-READY-POINTER.md` = V192/3884 строк, а `RUN-STATUS.md` = V309/1404 строки. Нужно определить, какой файл — текущий канонический train‑ready для плана.
- Teacher‑provenance: ни в одном прочитанном файле не сказано, какой моделью/процессом сгенерированы chosen/rejected (особенно `[WRONG:]`). Критично для Legal Teacher Gate — доисследовать `REGISTRY-DECISION-LEDGER.md` и `_stage-train-ready-v191-all-guides/` (не читались целиком из‑за размера ~0.4–0.5 МБ).
- Граф‑контракты: фактические узлы/рёбра, fan‑out/fan‑in, many‑to‑many выходы, async‑исполнение и владелец власти на рёбрах в данных не описаны — взять из кода `15.OrchestratorModel`/`42.ProjectWikiDomain` (вне этой области) при планировании.
- Не прочитаны (намеренно, защита от перегруза, но релевантны): `REGISTRY-DECISION-LEDGER.md`, `ACCEPTED-MANIFEST.md`, серия `deep-extracted/*flow01..48*session-chain*` (потенциальные узлы графа сессий/флоу), `learning runs internal sets/run22x.ps1` (контракт раннера: как именно считаются accuracy/margin/checkpoint).
- `XIIGEN-DPO-REGISTRY-GUIDE-v1.md` (управляющий гайд решений) в этой области по имени не найден — присутствует ссылками; найти его фактический путь.
- `HOW-TO-USE-SKILLS.md` в `C:/Projects/llm_mvp_core/docs/` существует (Glob подтвердил), но в этом проходе не открывался (приоритет отдан классификационному скилу и цели).
