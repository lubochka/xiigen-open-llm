# N40 · PrepareToGPL-first: что именно нужно восстановить, чтобы вернуть качество обучения

**18.10.2026 · SOURCE-READ + FROZEN-RECEIPT RECHECK; НЕ новый learning run.**
Целевой репозиторий: xiigen-planning, ветка llm_plan3, папка possibleDifs. Контрольная ветка: [llm_mvp_core/PrepareToGPL](https://github.com/lubochka/llm_mvp_core/tree/PrepareToGPL). Текущая ветка: [codex/dpo-implementation](https://github.com/lubochka/llm_mvp_core/tree/codex/dpo-implementation). Ещё просмотрена соседняя [language_translate](https://github.com/lubochka/llm_mvp_core/tree/language_translate); в ней нет отдельного каталога 27.TranslationDomain, поэтому она не является прямым донором современного TranslationMachine. Дополнительно сопоставлена ветка codex/dpo-implementation-s04.

**Смысл**: успешное обучение в PrepareToGPL — важнейший baseline, но оно не обязательно означало улучшение генерации одного и того же q/v-LoRA Student. Исторические результаты подтверждают *несколько разных learned recipients* (KNN, контекстная память, replay). Их нельзя автоматически переводить в «весовая нейросеть стала лучше». Для возврата качества восстановить именно **полезное обучаемое поведение в обычном consumer**; считать подлинным улучшением только paired exam по единой задаче, домену и неизменному контролю.

## 1. Эталонные положительные результаты — непосредственно прочитанные JSON PrepareToGPL

| Приоритет | Домен, получатель | Историческая baseline → enhanced | Материал / применимость и ограничение | Первичный receipt |
|---|---|---|---|---|
| **GOLD-P0.1** | C# AST / structural kNN, E09 | top-1 **0.2413 → 0.3507**; k=5, 4492 train pairs | Результат E09 Tier2 01.05.26; enhanced 763.5 ms vs baseline 14.5 ms; реальный end-user CodeDocumentation/RP01 consumer этим не доказан | [E09 JSON](https://github.com/lubochka/llm_mvp_core/blob/PrepareToGPL/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json) |
| **GOLD-P0.2** | Перевод / contextual E26 dictionary | disambiguation **0.40 → 1.00** | Всего 15 cases, 6 hard; нельзя переносить 100% на незнакомый корпус; важен перенос из word-map в обычный перевод | [E26 JSON](https://github.com/lubochka/llm_mvp_core/blob/PrepareToGPL/35.ExperimentsV2/results/E26-CONTEXTUAL-DICTIONARY/results/run-2026-05-01-003.json) |
| **GOLD-P0.3** | Continual learning / E43 replay | retained old-set accuracy **0.3226 → 0.3978** | OldPairs=465, newPairs=414, replay=177, улучшение на старых закономерностях; file boundaries могут смешивать training examples, см. existing N34-E43 | [E43 JSON](https://github.com/lubochka/llm_mvp_core/blob/PrepareToGPL/35.ExperimentsV2/results/E43-REPLAY-BUFFER/results/run-2026-05-01-001.json) |
| **GOLD-P1.4** | Оркестрация / E22 predicate multi-dataset router | macro-F1 **0.4709 → 0.5646** | Поздний 07.05.26: 360 train/90 val; TypeScript, Tests = 0; corpus/label definition отличались от более ранних опытов | [E22 поздний JSON](https://github.com/lubochka/llm_mvp_core/blob/PrepareToGPL/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/E22-DATASET-ROUTER/results/run-2026-05-07-002.json) |
| **NOT GOLD** | E22 более ранний, другой corpus | macro-F1 **0.4892 → 0.4944**, FAIL | Не подменять поздним PASS; на разных corpus нельзя считать A/B | [E22 ранний JSON](https://github.com/lubochka/llm_mvp_core/blob/PrepareToGPL/35.ExperimentsV2/results/E22-DATASET-ROUTER/results/run-2026-05-01-002.json) |
| **NEGATIVE CONTROL** | E07 AST-parent matrix experiment | top-1 **0.1502 → 0.1310** | FAIL, fake dimension/tier contract уже разобран; провал E07 НЕ доказывает бесполезность AST-feature в kNN E09 | [E07 JSON](https://github.com/lubochka/llm_mvp_core/blob/PrepareToGPL/35.ExperimentsV2/results/E07-STRUCTURAL-CONTEXT-TOKEN/results/run-2026-05-01-002.json) |
| **NEGATIVE CONTROL** | E14 context-action KNN | top-1 **0.8824 → 0.8676** | Не переносить KNN на всякую задачу без доменного benchmark: базовый частотный механизм здесь сильнее | [E14 JSON](https://github.com/lubochka/llm_mvp_core/blob/PrepareToGPL/35.ExperimentsV2/results/E14-CONTEXT-ACTION-KNN/results/run-2026-05-01-002.json) |

Эти JSON являются историческими результатами, **не повторно выполненным обучением**, не paired репликацией на текущем железе. Положительная строка означает факт сохранённого измерения указанного эксперимента, а не общесистемный паритет.

## 2. Прямое сравнение блобов исходного кода веток: где regression не следует приписывать отсутствию алгоритма

Контроль: GitHub fetch_file обеих веток с сопоставлением SHA blob, 18 октября.

| Файл | PrepareToGPL blob | current blob | Доказательный вывод |
|---|---|---|---|
| 27.TranslationDomain/TranslationTrainedUnits.cs | 1742abfc9fb6 | **1742abfc9fb6** | Класс KNN/word/phrase/exact learned units **побайтно тот же**; искать consumer/входные данные/включение/оценку, а не утверждать «этот файл переписали плохо» |
| 27.TranslationDomain/TatoebaPilotLoopRunner.cs | 1cc0ff0e9540 | **1cc0ff0e9540** | Та же механика resume, и унаследованные риски; не доказанный новый regression |
| 27.TranslationDomain/TranslationCheckpointCache.cs | 72e81ad2a | **72e81ad2a** | Не новый cache regression по содержимому этого файла |
| 35.ExperimentsV2/Experiments/E22DatasetRouter.cs | 12ee3fe2c | **12ee3fe2c** | Алгоритм E22 сейчас не пропал из исходника; проверить actual production invocation |
| 35.ExperimentsV2/Experiments/E26ContextualDictionary.cs | cce1972b8 | **cce1972b8** | E26 algorithm same; проверять перенос training → active consumer |
| 35.ExperimentsV2/Experiments/E07StructuralContextToken.cs | a7ec490fb | **a7ec490fb** | Historical E07 defect унаследован, не причина branch diff |
| 35.ExperimentsV2/Experiments/E09StructuralKnn.cs | 743b1a399 | 13b8124bf | Разница сверенных строк — **перевод комментариев/строк сообщений**, не другая модель kNN |
| 27.TranslationDomain/TranslationStructuredBatchBuilder.cs | 40d35475c | 7ceed82476 | В сверенной дельте добавлены только ES_EN/EN_ES source-lang routes; прежняя проверка novelty осталась |
| 27.TranslationDomain/TranslationMachine.cs | 4b94eb796 | fd0e736de | Добавлены preflight checkpoint validation, честный RewardMargin=0 вместо заглушки 0.03, корректировка Bootstrap gate и др.; **не объявлять новый 0.0 margin регрессией обучения**, это устранение неподтверждённой метрики |
| 27.TranslationDomain/TranslationRoutingFlow.cs | c0878f182 | ae9da139a | Добавлены bootstrap non-pass gate и ES routes; качественный regression через этот diff **не подтверждён** |

Ссылки на blob-path доступны по тому же relative path в [PrepareToGPL](https://github.com/lubochka/llm_mvp_core/tree/PrepareToGPL) и [current](https://github.com/lubochka/llm_mvp_core/tree/codex/dpo-implementation); перед исправлением надо читать **конкретного caller** на pinned ref.

## 3. Почему RP01 нельзя объявить эквивалентным successful PrepareToGPL

[RP01 recovery / source and frozen evidence](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md): C#500 Student free 0/3→0/3, EN→RU500 free 0/4→0/4, хоть AdamW реально менял 16 LoRA tensors. 500 C# examples включали 25 параметризованных laws, EN→RU — 20 категорий. В базовой генеративной модели B0 не установлена предобученная языковая способность. Поэтому GOLD E09/E26/E43 **не равен** RP01 quality gain; нет единого matched heldout и learned recipient.

Сверять пять отдельных переходов:
1. **Материал обучения**: legal teacher и проверенная семантика chosen/rejected; version, law, direction, negative conditions.
2. **Представление**: AST/role/order/negation должны реально входить в train и retrieval features там, где это полезно; обычный bag-of-token не эквивалентен E09.
3. **Получатель градиента / памяти**: Student q/v LoRA, learned KNN store, exact/phrase map, Unit, graph edge и selector — отдельные получатели.
4. **Consumer / gating**: реальный Generate/Translate/Route должен читать обновлённый learned state, а не canned rule или default-off adaptor.
5. **Cold restore + learning continuity**: тот же checkpoint → новый процесс → обычный ответ → следующий урок; сохранение файла без проверки содержания не равно восстановлению качества.

Existing foundational evidence: [matrix≠KNN and exact E07/E09 limitations](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md), [existing history evidence](11-CROSS-BRANCH-REAL-LEARNING-RESULTS.md), [current causal prioritization](39-RP01-CROSSWALK-NOVELTY-PRIORITIES-AND-MICRO-EXPERIMENTS-2026-10-16.md), [protected independent research — READ ONLY](https://github.com/lubochka/llm_mvp_core/tree/codex/dpo-implementation/docs/plans/replanning-possible-diffs).

## 4. В самом начале очереди: эксперименты восстановления алгоритма, а не тестов ради тестов

**GOLD-EX01 · C# E09 actual AST KNN transfer.** Same 12 unseen C# files (группировка по файлам/семействам), frozen AST parser/labels/tokenizer: (A) historical E09 clone; (B) current generic hashed KNN; (C) E09 feature adapter in target actual consumer; (D) BPE/lexical-only negative. Оценить lawful top-1, changed ordinary code answer, scope/AST/order inversion, latency, RAM; E09 structural features полезны прежде всего code/design-to-code, а не все языковые домены.

**GOLD-EX02 · Translation E26 + order/negation.** 12 heldout EN→RU/RU→EN cases с парой отрицание/утверждение, род/роль, редкими омонимами: (A) baseline memory, (B) E26 context/phrase + domain-key, (C) current learned executor ordinary, (D) cold-restored state. Score смыслового перевода и ошибок операторов, не только exact match. Один learned correction должен менять новый контекст, не просто exact known source.

**GOLD-EX03 · E43 replay.** 2 короткие чередующиеся партии разных law families, 8 ранее изученных вопросов и 8 новых; без replay / replay по старому алгоритму / replay file-boundary-safe. До/после старых и новых laws, chronological no-leak split, одинаковые updates/token budget, memory footprint. Нельзя объявлять исторический E43 result недействительным без нового causal run.

**GOLD-EX04 · E22 routing.** Frozen одинаковые manual labels, 6 bucket classes с nonzero examples, file-disjoint holdout; historical kNN vs current dataset route vs majority. Проверить право на production invocation. Соотношение auto labels и 0% классов фиксировать отдельно. Не переносить в перевод без domain-routing callsite.

**GOLD-EX05 · Student warm-start.** Реальная generation способность: random B0 + DPO vs same baseline + chosen-only SFT warmup + DPO (при одинаковой суммарной вычислительной квоте); независимая free generation и контрфакты. Это уже запланировано, **не новая гипотеза N40**: [RP01 current repair](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md). Не пытаться скормить несопоставимые старые E09 vectors непосредственно модели с другим vocab.

**Очерёдность восстановления: GOLD-EX01/02 сначала для learned hybrid behavior, GOLD-EX03 для retention, GOLD-EX05 отдельно для neural free answers, GOLD-EX04 для задач роутинга.** Нет основания применять AST features к RU prose по умолчанию; word order и negation важны в переводе; replay — для всех обновляющихся доменов с контролем забывания.

## 5. Неопределённости, которым запрещено становиться «фактом»

- «Плохая матрица была перенесена в успешный kNN» — важное свидетельство постановки задачи, **точная commit-level идентичность E07/E12→E09 ещё не доказана**; не выдавать гипотезу за подтверждённое перемещение.
- Source SHA и historical JSON не гарантируют тот же runtime binary, использованный в мае.
- Native .NET / full tokenizer / free answer / cold-restored matched A/B не запускались в данном N40.
- Новые **унаследованные** потенциальные дефекты batch-admission и checkpoint corpus identity — в [N40 new hypotheses](40-NEW-CORPUS-IDENTITY-AND-INTRABATCH-ADMISSION-2026-10-18.md) и [micro experiments](40-MICRO-CAUSAL-EXPERIMENTS-BY-DOMAIN-2026-10-18.md); они не объясняют непосредственно веточный regression без реального affected caller.

**Изменять разрешено только planning/possibleDifs.** Protected llm_mvp_core/docs/plans/replanning-possible-diffs/** остаётся только источником.