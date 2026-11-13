# N56 — независимый аудит RP01: сохранять доказанное обучение, а не подменять его метриками

**Дата:** 2026-10-19. **Роль:** code/architecture/plan review + сверка существующих native receipts; **не новый запуск .NET, не обучение и не независимая репликация**. **Разрешённая запись:** только `xiigen-planning@llm_plan3/.../replanning/possibleDifs`. **Запрещённая запись:** весь `llm_mvp_core/docs/plans/replanning-possible-diffs/**`, production-код, datasets, веса, чекпойнты. Этот документ не даёт разрешения запускать все карточки.

## 1. Что является собственно потерей качества

Тема обучения (домен) — **перевод, документация, программирование, оркестрация, интерфейсы**; языковые направления и kNN/DPO/LoRA — соответственно *профили* и *механизмы*, а не самостоятельные домены. Требуется цепь: `TRAIN -> конкретный learner/Unit/Student -> изменение сохранённого состояния -> тот же ordinary consumer -> независимый EXAM до/после`. Сравнение разных checkpoint, evaluation wrapper и кэша без идентичных профиля, заданий, scorer и consumer — **не доказательство** ухудшения из-за алгоритма.

Классы свидетельств: `SOURCE_CONFIRMED` (свойство кода), `HISTORICAL_NATIVE` (имеющиеся сохранённые результаты), `EXPERIMENT_DESIGNED`, `SOURCE_EQUIVALENT_PROXY`, `CAUSAL_NATIVE_AB` (проверка вмешательства, реального consumer и фиксированного EXAM). В настоящем проходе **новых CAUSAL_NATIVE_AB не исполнено**.

### Самое важное положительное свидетельство — 18 октября

[Реестр выполненных опытов](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/README.md) содержит **другой статус, чем карточки NOT_RUN**:

| Домен/профиль | Реально обучаемый получатель | Сохранённое наблюдение | Граница доказательства |
|---|---|---|---|
| Перевод EN→RU, P125.1 | ExecutorUnit exact/phrase/word/KNN memory через ordinary Flow | **0/24 → 19/24** в B, контроль C 13/24; независимый replay | По одной панели, остаются ошибки смысловых контрастов; **не** LoRA и не широкий языковой перенос |
| Перевод EN→RU, P25.2 | Сохранённая память + cold restore того же Flow | **19/24** совпали после восстановления на тех же 24 задачах | Сохранение поведения, включая ошибки, не обучение после рестарта; перенос в другие направления не проверен |
| Перевод EN→RU, доставка урока | Training reader/builder → Executor memory → ordinary Flow | **11/24 → 19/24**, mappings **0 → 16** | Доказывает пользу доставки structured mapping в этом домене; не нейросетевые веса |
| Neural SFT embedding (8 уроков) | Full C50 neural checkpoint | **0/24 → 0/24**, 38 400 embedding weights изменились | Нет наблюдаемого предметного gain; это не тот же ordinary Executor |
| Neural bias и fractional DPO mask (8 уроков каждый) | Отдельные native весовые/оптимизаторные пути | **0/24 → 0/24**, документированы градиенты/моменты | Исправление производной/изменение весов **не** равно новому умению; это не production RP01 end-to-end |

Отчёт указывает пять native neural arm-runs, 40 optimizer updates, 120 новых ответов для последних нейронных сравнений; это авторские receipts с независимым review, **не наши запускавшиеся сейчас тесты**. Не превращать факт `19/24` в «общая модель умеет переводить на 79%»: он относится к строго определённому memory/Flow EXAM и содержит ошибки.

## 2. Gold-first: сравнивать не ветки по дате, а действовавшие механизмы

| Donor / evidence | Что обязательно перенести в сверку | Что нельзя объявлять доказанной регрессией |
|---|---|---|
| `PrepareToGPL@31346d8928be4469b01aaf313780e8f950ff91e2` | сохранённые learned translation Units, 4→6 миграция направлений, модель/LoRA state, ordinary answer, semantic maps | Нельзя трактовать унаследованный `EvaluateAsync` RU→EN hardcode как **новое** изменение ветки; см. [N55](55-NOVEL-DIRECTION-EVAL-AND-METRIC-STATE-2026-10-19.md) |
| `DesignToCode-clean@e346b88a3cc1fcce48aefa626bfc7b00509eefd6` | валидные старые checkpoints и category/critic learning; DPO preference receipts только в собственном смысле | Legacy manifest v1 не проходит **новый publish validator**, но это ещё не доказательство неуспеха прямого Import; см. [N54](54-DESIGNTOCODE-CLEAN-DIRECT-DIFFS-AND-NOVEL-BOUNDARIES-2026-10-19.md) |
| `orchestrator-extensions@b293045bd89b1eb96d3e32fedd341f1a4170adbc` | v1 OrchestrationReasoning 15 patterns vs current v2 load, реальный план/consumer | Не считать счётчик observations продолжением весов или готовой оркестрацией; см. [N52](52-ORCHESTRATOR-NEW-GAPS-AND-EVIDENCE-2026-10-19.md) |
| Исторические E09, E22, E25, E26, E43 и прочие | Разделить «experiment verdict» и **полезный отдельный learned submechanism**, включая перенос отвергнутого matrix-prior в kNN function | Нельзя складывать разные denominators, `BuildDefault` словарь и обучение на новом примере в одну кривую |
| RP01 `CodeDocumentationSimulationDpoExperiment` | exact DPO pair material → independent frozen reference → project-LoRA update → private checkpoint + optimizer/RNG → Student generation/evaluate | Сохранённая `DpoMetrics.Accuracy` — знак preference margin, не accuracy обучения документов; private candidate не равно serving adoption |

Источники: [RP01 source map](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-CURRENT-CODE-MAP.md), [сквозной мастер-реестр](00-MASTER-SYNTHESIS-AND-CONFLICTS.md), [gold branch](https://github.com/lubochka/llm_mvp_core/tree/31346d8928be4469b01aaf313780e8f950ff91e2), [DPO source lines 650–699](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/11.DPO/DpoTrainer.cs#L650-L699), [текущий Student](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/14.ContinuousLearning/Training/CodeDocumentationStudentGeneration.cs#L61-L76).

## 3. Установленные семьи расхождений и принадлежность прежним открытиям

| Приоритет | Механизм | Уже есть доказательства / подготовленный опыт | Domain scope и актуальный статус |
|---|---|---|---|
| P0 | **DPO выбранный/отвергнутый, conditioning, повторяемость, ответная маска, тренируемый recipient** | [02/03](02-DPO-DATA-OBJECTIVE-GRADIENT.md), [protected P04/P07/P08/P81/P86/P87](https://github.com/lubochka/llm_mvp_core/tree/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/cards), [native fractional-run](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/README.md) | C# docs, EN→RU; текущий exact-v2 бинарный mask, дробная legacy-ветка не доказывает дефект RP01; нужен actual Student/independent examiner |
| P0 | **LoRA topology, normalization, optimizer/AdamW, frozen base, warm start, checkpoint** | [03](03-LORA-TOKENIZATION-VECTOR-SPACE.md), [N41](41-LORA-BRANCH-PARITY-AND-REAL-NEURAL-QUALITY-BOUNDARY-2026-10-18.md), protected P02/P11/P12/P119 | neural documentation / neural translation; 18-Oct веса действительно меняются, но результат 0/24 |
| P0 | **Положительная memory, kNN functions, кандидатные tiers, состав признаков, направление и fallback** | [N55 register](55-CROSSBRANCH-GAP-REGISTER-2026-10-19.md), protected P16/P26/P27/P35/P54/P60–P67/P117/P125 и 18-Oct P125 | структурный C# AST, языковой перевод и оркестрация **не взаимозаменяемы**; 19/24 — сильный donor для конкретного EN→RU Executor |
| P0 | **DPO → semantic sub-units → trained graph/Flow; правдивое использование state/restore** | [N17](17-STRUCTURED-UNIT-TRAINING-SILENT-DISPATCH.md), [N27](27-NEW-STRUCTURED-READINESS-RESET-AFTER-NOOP-BATCH-2026-10-16.md), protected P18/P19/P25/P53/P92/P125 | translated Executor/Verifier, DTC Critic/Improver, orchestrator learning: проверять каждого recipient и реальный output |
| P1 | **Tokenizer/BPE, semantic vector identity, domain-specific syntax/word order** | protected P17/P26/P28/P43/P51/P76/P98; [31 EOS boundary](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/31-answer-boundary-and-decoding-scope.md) | AST roles нужны коду; agreement, negation и порядок — переводу; decoder EOS already known, not new |
| P1 | **Исторический эксперимент, leakage, scored wrong recipient, wrong version/scorer** | [N55](55-NOVEL-DIRECTION-EVAL-AND-METRIC-STATE-2026-10-19.md), [N52](52-ORCHESTRATOR-NEW-GAPS-AND-EVIDENCE-2026-10-19.md), protected P03/P57/P66/P81/P82/P112 | валидный score должен указывать family-split, actual profile и consumer; без этого no regression conclusion |
| P1 | **Документированная производительность и факт promotion** | [новая узкая delta N56](56-NEW-METRIC-PROMOTION-SEAMS-2026-10-19.md) | affects cost/performance interpretation and delivery, **не установлено**, что уже уменьшало subject accuracy |

Эти группы — **маршрутизация и приоритизация**, не перечень «вновь найденных» багов. Переименовывать ранее найденное E22/E26/E43, N17, N27 или N55 как N56 запрещено.

## 4. Приоритет короткого подтверждения качества

1. **Gold-trained translation Executor memory -> ordinary Flow -> cold restore**: повторить уже зарегистрированный P125+P25 в pinned environment, затем 2–4 новых независимых контраста по направлению, отрицанию и перестановке ролей; фиксировать собственные ответы, negative transfer.
2. **Neural RP01 `D`**: включить строго ограниченный native `micro-paired` caller (пока отсутствует), 2 контрастных урока, 4 независимых EXAM; A0/A1/B0/B1, frozen tokenizer/base/reference/decoder; логировать actual LoRA deltas, ordinary Student outputs и стоимость. До этого нельзя объявлять «2-lesson DPO испытанным».
3. **Структурный C# kNN E09 и переводы POS/E26**: проверять roles/order именно в **их** обучаемом predictor/consumer, а не считать статический словарь успешным neural training. Усложнение под другой домен только после отдельного значимого A/B.
4. **Restore/adoption**: одинаковый checkpoint bytes и source state, проверка правильного import/registry/ordinary answer; после восстановления не подменять модель старой deterministic lookup map.

**Критерий завершения:** источник → исходная и новая ветка/sha → проверяемая гипотеза → один независимый доменный A/B → сырые ответы до/после + доказанная мутация нужного recipient + независимый review. Если какой-то элемент отсутствует — `OPEN`, не `PASS`.

Связанные новые deliverables: [N56 narrow hypotheses](56-NEW-METRIC-PROMOTION-SEAMS-2026-10-19.md), [audit protected coverage](56-PROTECTED-EXPERIMENT-VALIDITY-2026-10-19.md), [micro-experiment packets](56-CAUSAL-MICROEXPERIMENTS-BY-DOMAIN-2026-10-19.md), [parent RAG provenance](56-PARENT-RAG-PROVENANCE-AND-EXPERIMENT-CROSSWALK-2026-10-19.md).
