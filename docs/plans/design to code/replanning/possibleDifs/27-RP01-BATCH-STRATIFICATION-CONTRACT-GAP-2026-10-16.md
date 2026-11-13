# RP01 / N27-S — Нарушение контракта стратификации учебных минибатчей

**Дата:** 2026-10-16. **Статус:** SOURCE_CONFIRMED_CONTRACT_DIFF + FROZEN_SOURCE_PROXY_EXECUTED; TRAINING_QUALITY_CAUSALITY_NOT_PROVED; NATIVE_STUDENT_A_B_NOT_RUN; INDEPENDENT_CODEX_ASTRA_REVIEW_NOT_RUN.

**Pinned source:** [llm_mvp_core@8c7d41611aecba98d3d1bbdbf30d69bd784949ae](https://github.com/lubochka/llm_mvp_core/tree/8c7d41611aecba98d3d1bbdbf30d69bd784949ae), ветка codex/dpo-implementation. Публикация только в xiigen-planning/llm_plan3/replanning/possibleDifs. Core/docs/plans/replanning-possible-diffs — READ ONLY.

## 1. Доказанное отличие между источником и выполнением

| Звено | Буквально проверенный код/артефакт | Что это доказывает |
| --- | --- | --- |
| Автор корпуса | [materialize_dataset.py L227–236](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/.kiro/xiigen-slices/work/results/RP01-astra-synthetic-dev-csharp-500-20261015/materialize_dataset.py#L227-L236): `records.sort(key=lambda r: digest(r["prompt"].encode("utf-8")))`; комментарий «Storage order is deliberately noncurricular. A runner must freeze its own stratified batch schedule before effects; this is not an optimizer schedule.» | SHA256 лексический порядок **не предназначен** как очередность градиентных обновлений. Источник требует отдельной стратификации. |
| Контракт корпуса | [dataset-manifest.json, consumer_contract.schedule](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/.kiro/xiigen-slices/work/results/RP01-astra-synthetic-dev-csharp-500-20261015/dataset-manifest.json) | 500 пар ≠ 500 оптимизаторских шагов; расписание должно быть зафиксировано до исполнения. Внимание: эта строка НЕ утверждает, что текущий алгоритм стратификации существует. |
| Реальный исполнитель | [CodeDocumentationSimulationDpoExperiment.cs L175–187](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L175-L187): `for (offset ...)`, `selected = batches.Skip(offset).Take(count)`, `trainer.StepVector(selected, step, timing)` | По подтверждённому пути порядок сохранённого JSONL **без перестановки/стратификации** становится порядком 50 обновлений при batchSize=10. |
| Реальный optimizer | [DpoTrainer.cs L832–930](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/11.DPO/DpoTrainer.cs#L832-L930): суммирует вклад пар текущего списка, затем один `ApplyOptimizer`. | Разделение на минибатчи меняет, какие семейства конкурируют за один градиентный шаг, а не просто текст журнала. |

**Важно:** это *специфический подтверждённый подслучай уже существующего DIF-40* из [01-PRIORITIZED-DIFFERENCES.md](01-PRIORITIZED-DIFFERENCES.md), а не ложное объявление совершенно новой категории «fixed schedule».

## 2. Новый количественный source-level анализ всех 500 строк

Прямое чтение текущего `train500.jsonl` целиком: 500 валидных JSON-записей; 25 `semantic_template_id`, по 20 параметризованных вариантов; неизменный SHA-256 **c78a620a01cd2ab1d35962eb3521a9e87ee51deeedc325dfc809112b0e035935**, зафиксирован в manifest. Прокси-группировка строго по реальным последовательным десяткам.

| Показатель исходных 50 батчей | Значение |
| --- | ---: |
| Батчи, где одно семейство представлено более одного раза | **40 / 50** |
| Минимум разных семейств в одном батче из 10 | **6** |
| Максимум разных семейств | 10 |
| Среднее число семейств | **8.4** |
| Максимум строк одной семьи в одном батче | **3** |
| Батчи 1 и 2: разных семей | 9 и 8 |
| Статус настоящего DPO Student-переобучения по иной группировке | **NOT_RUN** |

Эти числа **не** являются доказательством плохих градиентов: обычное перемешивание тоже даёт повторы. Различие состоит в невыполненной отдельной стратификации, требуемой авторским договором данных, при ограниченном количестве шагов. Не переписывать frozen C# 0/3→0/3 и ENRU 0/4→0/4.

## 3. Каким образом могла потеряться полезная обучаемость

У Student случайная B0, LoRA только q/v; при одном проходе каждая запись используется ровно однажды. Когда батч содержит 2–3 варианта той же law-family, другие семейства в этом шаге отсутствуют. Это способно изменить траекторию AdamW, баланс противоположных признаков и перенос на новые вариации закона. Однако отличия из-за слабого B0, objective/LoRA или неправильной оценки могут доминировать; schedule — **гипотеза H27-S**, не установленная причина 0/3.

Деталь причинного контроля: при одинаковом наборе всех 500 строк, но разных минибатчах, изменение метрики можно связывать с **распределением по шагам** только если модель, начальные веса, optimizer/RNG, порядок шагов, total pairs, tokenizer и holdout неизменны. Дополнительные изменения target modules, rank, learning rate, prompt или самой пары запрещены в таком контрасте.

## 4. Минимальный воспроизводимый эксперимент

[Замороженная JSON-фикстура: 20 конкретных IDs, оба одинаковых множества](27-RP01-STRATIFIED-20-CASE-MICRO-FIXTURE-2026-10-16.json).

1. **0-й этап без тренировки (уже выполнен source-level):** реплеить строки 1–20 и сравнить порядок A STORAGE с B STRATIFIED: A имеет **9+8** разных семей в двух шагах; B имеет **10+10**. Оба содержат строго те же 20 IDs и **16** разных семей суммарно, включая четыре пары повторных семейств; ровно два шага по 10, без дополнения/удаления.
2. **A/B native Student (НЕ выполнен):** из одного замороженного B0 со сброшенными optimizer и RNG выполнить A: `StepVector` дважды по arm A, B: те же две операции по arm B. Не использовать основной journal/accepted checkpoint; отдельные disposable кандидаты и explicit rights/admission. Не запускать 500×, Windows CI или полный проект ради микропробы.
3. **До/после обеих рук:** зафиксировать 20 train preference margins, per-family log-odds, norm/finite grads, восстановленный checkpoint, одинаковое число токенов в training multiset, а также независимые заранее запечатанные 8–12 C# new-source cases (поблизости и из новых law families, **не** те же 20).
4. **Настоящее качество:** один и тот же декодер и слепая независимая оценка correctness/critical error/abstention в обычном `CodeDocumentationStudentGeneration.Generate`, отдельно teacher-forced метрики. Временные затраты/память и число Steps измерять. Не считать увеличение margin самостоятельным успехом.
5. **Результат:** B>A по независимому free-law correctness без новой критической ошибки — сигнал влияния batch-grouping, нуждается в репликации на других disjoint blocks/seed. A≈B или обе модели 0 полезных ответов — schedule не доказан, **вернуться к cold-start/P0**, не масштабировать расписание. Снижение качества B означает отказаться от предполагаемой «правильной» стратегии и расследовать.

### Стоп-условие и защита эксперимента

Если миниатюрный Student вообще не может увеличить likelihood на train-парах, пометить `INCONCLUSIVE_UNLEARNABLE_BASE` и использовать [REC02](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/tasks/recovery/RP01.REC02-DPO-GRADIENT.md)/[REC03](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/tasks/recovery/RP01.REC03-CAPABILITY.md), а не увеличивать N. Фактические времена мини-шагов могут быть существенными на CPU; называть сравнение «быстрым» только относительно 500-парной кампании, не обещая секунд.

## 5. Разделение фактов и ревью

- **CONFIRMED:** прямой конфликт «storage noncurricular» vs `Skip(offset).Take(count)`, статистика 500 JSONL и offline-фикстура.
- **NOT PROVED:** выигрыш стратификации, причинная связь с 0/3→0/3, тренировка B в нативном .NET.
- **ALTERNATIVES:** hash-sorted data похожи на псевдослучайную выборку; один epoch может быть недостаточен даже при идеальном расписании; LoRA/cold B0 могут полностью маскировать schedule.
- **CODEX/Astra 6 UltraCode human reviewer:** не вызывался; статус этой записи DRAFT, не APPROVED. Реальный внешний китайский teacher здесь не привлекается: выбранные/отклонённые стороны только те же существующие simulation-пары, никаких фальшивых новых DPO authorship claims.
- **Публикация ≠ качество:** никакой Student, product и core/plans файл этим документом не исправлен.

**Навыки, применённые в работе:** human-readable-architecture-audit, phase-planning, plan-review, code-examination, experiment-design, prompt-patch-authoring, conversation-intent-and-self-reflection, learning-path-audit. Критерии: named source seam → caller → consumer, один изменяемый фактор, evidence-versus-hypothesis, bounded micro-run, user-readable вывод.