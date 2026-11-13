# Доказательства, новизна, охват и границы вывода

**Итоговый статус перед публикацией:** после трёх проверок параллельных добавлений corpus имеет 555 Markdown-путей. N-RESTORE-01 — независимое подтверждение; N-NEURAL-01 — новый фактический пересчёт; N-NEURAL-02 — уточнение уже описанной в DLT-K1 гипотезы отдельным m-only сравнением; N-NEURAL-03 — условная граница API. История ниже сохраняет промежуточные срезы; [последняя сверка](#сверка-параллельной-публикации-412d3364) имеет приоритет для утверждений о новизне.

## 1. Замороженные источники

| Источник | Commit | Как использован |
|---|---|---|
| llm_mvp_core / codex/dpo-implementation | `a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6` | Нынешние source owners, оба neural states/receipts, актуальные native отчёты, защищённый каталог только для чтения |
| llm_mvp_core / PrepareToGPL | `31346d8928be4469b01aaf313780e8f950ff91e2` | Конкретный translation-v1 state, historical JSON, neural/source before |
| llm_mvp_core / DesignToCode | `8f20ceb8a9bfbadab2ad309226c47dd76e714548` | Адресное сравнение neural trainer/LoRA/BPE/RmsNorm; не весь пользовательский опыт ветки |
| llm_mvp_core / language_translate | `f6032aff81f1f83de7c09cf9b9efe6b058dd12b9` | Прямое сравнение тела E26ContextualDictionary experiment; не всей переводческой реализации |
| xiigen-planning / llm_plan3 | `a10bbde116515b1453d9ec6bfb103725ad99d477` | RP01/replanning/RAG/parent design context и существующий possibleDifs corpus |

Список GitHub branches также зафиксирован, включая DesignToCode-clean/external_domains/main и другие. Само перечисление branch SHA не означает аудит всех этих веток. Запрос охватывает потенциально большой исторический граф; здесь подтверждены названные пути, а не заявлена исчерпывающая проверка каждого commit/бинарника.

Исходные деревья каталогов: planning possibleDifs=`2c64ee880d5d0007a96d14ee63274b06254fb378`; protected=`c25dbb665b53d1e849ef490ccc798d915d78b682`. В inventory включены все 532 Markdown blobs этих деревьев, с исходным Git SHA. Бинарные, generated и прочие файлы не приравнены к семантически просмотренным документам.

## 2. Что проверялось по первичному содержимому

Neural: actual `01.Tokenizer`, `03.TransformerModel`, TensorCore operations, `04.TrainingLoop`, `10.LoRA`, `11.DPO`, RP01 experiment/evaluation/continuation/descriptor callers. Translation: `TranslationMachine`, contracts, structured builder, semantic tokenizer, trained Units, KNN contracts, Flow, research adapter и Tools ingestion/continue. Historical transfer: E07/E09/E22/E26 primary run JSON, E09 source, current StructuralKnn/SemanticCompiler facade и flags, sibling E26.

Planning: RP01 recovery/code map, [SOURCE-ROUTES](https://github.com/lubochka/xiigen-planning/blob/a10bbde116515b1453d9ec6bfb103725ad99d477/llm%20mvp/design%20to%20code%20session/replanning/SOURCE-ROUTES.md), [RECOVERED-DESIGN-CONTRACT](https://github.com/lubochka/xiigen-planning/blob/a10bbde116515b1453d9ec6bfb103725ad99d477/llm%20mvp/design%20to%20code%20session/replanning/RECOVERED-DESIGN-CONTRACT.md), [DPO conformance research](https://github.com/lubochka/xiigen-planning/blob/a10bbde116515b1453d9ec6bfb103725ad99d477/llm%20mvp/design%20to%20code%20session/09.09.26WorkLeft-RESEARCH-03-dpo-algorithm-conformance.md), master RAG index и branch pointers, parent KNN integration plan и locked decisions. Большой master index использован для поиска маршрутов/релевантных фрагментов; полного построчного прочтения всего RAG не заявляем.

Из обоих каталогов выполнен полный text search; ближайшие к новым тезисам записи прочитаны отдельно. Новизна не устанавливается отсутствием одного точного имени в поиске: она сопоставлена с причинным вмешательством и материалом старой карточки.

## 3. Проверка, что новые пункты не дублируют известные

| Пункт аудита | Ближайшее существующее знание | Что именно отличается | Контрдоказательство/предел |
|---|---|---|---|
| N-RESTORE-01 | P25 restore/current6→6, source-language key migration; planning16/26/30 о shared six-direction KNN | **Число обязательных executor records изменилось4→6 при schema 1; точный legacy checkpoint остался тем же blob** | Текущая atomic validation полезна. Native old4 load не исполнен. Не причина neural0/24 без отдельного caller evidence |
| N-NEURAL-01 | Planning32 предлагает absolute chosen; protected12 говорит о small relative gain; P03 score lineage | **Пересчитаны именно retained scores**:20/20 chosen↓,19/20 rejected↓,11/20 margin↑,C# иной знак | Небольшой teacher-forced сдвиг, не новая blind generation grade и не доказательство конкретной причины |
| N-NEURAL-02 | P12 schedule/partial optimizer, P15 replay, P25 restore, P105 reference, P119 LR/beta/batch; поздний DLT-K1 уже описывает old-moment interference/cosine | **Один m50-only switch** при той жеv50/step/C50/reference на observed C#→ENRU boundary | Slots в LFS не прочитаны, конфликт не измерен. Перенос momentum может и помогать |
| N-NEURAL-03 | Общая topology/full-state completeness и alpha discussions | **Bare tensor fingerprint ≠ function identity при live Alpha mismatch**; опыт residual ratio сB≠0 | Реальный RP01 descriptor проверяет полный identity. Conditional API boundary, не установленная production причина |

Для legacy проверены точные термины `EXECUTOR_DIRECTIONS_INCOMPLETE`, `missingDirections`, `ValidateCheckpointStructure`, а также семантические темы4/6directions/restore/migration. В532 Markdown нет первых трёх точных термов; упоминания общих шести направлений есть и **не** считаются отсутствующими. В P25 уже есть другие migration/state вопросы. Поэтому здесь отдельная причинная граница, а не переименование всей темы восстановления.

Для neural сверены P03/P05/P12/P15/P25/P105/P119, planning26/32 и ближайшие источник-разборы score/AdamW. Процент вероятности новизны не выдумывается: результат относится к закреплённому corpus, не к недоступным чатам других агентов или будущим изменениям.

## 4. Выполнено в этой работе

| Действие | Доказательство | Что оно не доказывает |
|---|---|---|
| Exact source/data comparison legacy4→6 | [JSON](evidence/legacy-checkpoint-result.json), [Python](evidence/legacy_checkpoint_probe.py); исходные3 Git blobs и checkpointSHA проверены | Не native loader/migration execution |
| Пересчёт45paired score rows | [JSON](evidence/neural/likelihood-components.json), [Python](evidence/neural/recompute_likelihood_components.py),6raw receipts | Не новые Student calls/optimizer/generation/evaluator и не semantic retraining gain |
| Независимый повтор обоих offline расчётов и source review | [Review](07-INDEPENDENT-REVIEW.md) | Не запуск предлагаемых quality experiments |
| Read-only inspection текущих native отчётов18октября | P125/P25/transport/mask/embedding links в [05](05-COVERAGE-AND-EXISTING-CARDS.md) | Не наши новые runs; не все возможные домены |

**Во всей этой сессии новые training/generation/teacher/evaluator calls=0.** Имена предпочитаемых пользователем моделей в будущих опытах — роль/настройка, не runtime attestation нынешнего исследователя. В защищённых native отчётах также сохранены собственные границы attestations; они не усилены нашим пересказом.

Исходные6 receipts сохранены побайтно, включаяCRLF и отсутствие terminal newline; пересчёт проверяет их original Git blob. GitHub JSON transport для публикации должен сохранять эти строки точно. На остальных downloaded scratch source copies обычный extra newline не используется как доказательство исходного blob; exact legacy probe получает отдельно извлечённые исходные bytes.

## 5. Применённые skills и независимая проверка

По просьбе пользователя использованы project skills архитектурного аудита и решений об обучаемой архитектуре, intent-to-plan/phase-planning, plan-review/self-validation, generated-code-review, prompt-patch-authoring и human-communication glossary из [docs/skills](https://github.com/lubochka/llm_mvp_core/tree/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/skills). Применение означает: назвать state owner и ordinary consumer, различить source evidence/native result/hypothesis, сохранить одну причинную переменную, учесть предметные признаки и объяснить before/after понятным языком. Изменения самих skills/AGENTS/политик не выполнялись.

Один независимый агент проверил neural часть и позже reviewed подготовленные root документы; root отдельно перепроверил neural арифметику и источники. Этот пакет не выдаёт два сообщения одного процесса за два native независимых обучения. Итог review и исправления записаны в [07](07-INDEPENDENT-REVIEW.md).

## 6. Что осталось неизвестным

- Actual native old4→current6 load и предложенная миграция: пока source/data proof и конкретный опыт.
- Actual C50 optimizer moments: полученLFSpointer, неpayload; никаких выводов о cosine реальных gradients без этого объекта.
- Полный список конкретных успешных пользовательских neural runs и их исполнявшихся binaries: examined donor paths не заменяют отсутствующий run receipt.
- Точная единая Git-цепочка для всех историй «failed matrix→successful KNN function»: дляE09 доказан transfer, дляE07/E09 нельзя автоматически объявить один и тот же run/metric.
- Реальный production consumer всех opt-in functions: наличие class/Train method не доказывает использование ответа.
- Causal improvement всего RP01 от любого одного перечисленного пункта: ни один source-only audit этого не устанавливает.

Эти неизвестные не препятствуют использованию найденного legacy mismatch, сохранённых положительных memory результатов и конкретных коротких A/B. Они определяют, какое наблюдение нужно получить следующим.


## Дополнение перед публикацией: учтены параллельные главы40–42

За время аудита planning обновился до `a4a3e709f8fb5310baab6daa16501b5d4394bd72`. Дополнительно прочитаны/сопоставлены11 новых Markdown документов и новая версия README; core остался на `a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6`. Теперь актуальный набор каталогов имеет283 planning + 260 protected = 543 различных Markdown-пути. Первоначальный inventory532 сохранён как честный исходный срез; [publication delta](evidence/publication-delta.json) перечисляет14 изменённых blobs, включая2 не-Markdown файла и README.

Другой агент уже опубликовал [N41-T1 legacy4→6](https://github.com/lubochka/xiigen-planning/blob/a4a3e709f8fb5310baab6daa16501b5d4394bd72/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/41-FOUR-VS-SIX-TRANSLATION-CHECKPOINT-MIGRATION-2026-10-18.md). Поэтому **N-RESTORE-01 не заявляется новым относительно состояния перед публикацией**. Наш вклад — независимое подтверждение, точные bytes реально сохранённого checkpoint в обеих ветках, совпадение с историческим SHA/двумя probes и zero-training исходный fixture для опыта. Не нужно сначала создавать новый old-branch checkpoint обучением, чтобы установить этот конкретный разрыв.

В новых40–42не найдены фактическая декомпозиция20/20 chosen и отдельное `m50→0` вмешательство; N-NEURAL-01/02 сохраняют ограниченный статус новых. Общие LoRA/warm-SFT сравнения в41уже есть; они не объявлены нашими новыми гипотезами. Conditional bare Alpha boundary отделена от защищённого RP01.

Дополнительные чужие находки остаются в работе и имеют отдельные источники: [corpus identity и intrabatch admission](https://github.com/lubochka/xiigen-planning/blob/a4a3e709f8fb5310baab6daa16501b5d4394bd72/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/40-NEW-CORPUS-IDENTITY-AND-INTRABATCH-ADMISSION-2026-10-18.md), [rejected-side annotations для positive memory](https://github.com/lubochka/xiigen-planning/blob/a4a3e709f8fb5310baab6daa16501b5d4394bd72/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/41-REJECTED-SIDE-ANNOTATION-CONTROLS-POSITIVE-EXECUTOR-MEMORY-2026-10-18.md), [TopicDomain input/key erasure](https://github.com/lubochka/xiigen-planning/blob/a4a3e709f8fb5310baab6daa16501b5d4394bd72/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/41-TRANSLATION-TOPICDOMAIN-INPUT-ERASURE-AND-MAP-COLLISION-2026-10-18.md), [orchestration restore/provenance и microexperiments](https://github.com/lubochka/xiigen-planning/blob/a4a3e709f8fb5310baab6daa16501b5d4394bd72/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/42-NEW-CAUSAL-HYPOTHESES-AND-DOMAIN-MICROEXPERIMENTS-2026-10-18.md). Это дополнения других авторов, не новые результаты нашей проверки и не заменённые документы.

## Последняя проверка перед записью ветки

Непосредственно перед публикацией другой автор добавил ещё [6 документов восстановления](https://github.com/lubochka/xiigen-planning/blob/004c4a79f3ec0c1dba387ba713abad108b7568fe/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-18-preparetogpl-recovery/README.md); новый parent planning commit — `004c4a79f3ec0c1dba387ba713abad108b7568fe`. Проверка lease остановила запись поверх прежнего HEAD. Пакет перенесён поверх новых изменений, без их удаления или переписывания. Последний сопоставленный набор имеет **289 planning + 260 protected = 549 различных Markdown-путей**; первоначальные 532 и промежуточные 543 остаются историческими срезами inventory.

В новом [translation-разборе](https://github.com/lubochka/xiigen-planning/blob/004c4a79f3ec0c1dba387ba713abad108b7568fe/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-18-preparetogpl-recovery/01-translation-restore-and-unit-lineage.md) также уже есть конкретный архивный checkpoint и тот же legacy4→6 разрыв. Поэтому ни механизм, ни сам факт существования этого файла здесь не считаются исключительным новым обнаружением: это независимое подтверждение N41-T1 и U-R1. Наш exact-byte скрипт и zero-training способ проверки оставлены как воспроизводимое дополнение к общему выводу.

Новая [neural-глава другого автора](https://github.com/lubochka/xiigen-planning/blob/004c4a79f3ec0c1dba387ba713abad108b7568fe/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-18-preparetogpl-recovery/02-neural-and-lora.md) рассматривает N-L1: намеренный импорт legacy base с сохранением A/B receiver и риск неправильного full-restore intent caller. Это отличается от нашей условной N-NEURAL-03: одинаковый full-v2 payload, но другой live Alpha. Обе темы имеют собственные guards и не устанавливают RP01 root cause. Ни actual20/20chosen decomposition, ни first-moment-only m50 comparison в этих шести документах не найдены; bounded novelty N-NEURAL-01/02 сохранена.

Дополнительные файлы и их exact Git blobs перечислены в `finalPrepublicationCheck` [publication-delta.json](evidence/publication-delta.json). Полнотекстовый поиск выполнен во всех шести; наиболее близкие translation/neural/novelty разделы сопоставлены содержательно. Это проверка новых материалов, не повторное независимое исполнение их source/native экспериментов.

## Сверка параллельной публикации 412d3364

19 октября 2026 перед записью ref обнаружен ещё один commit другого автора — `412d3364cc94b5aa1cdf516f829960da152826cb`, parent `004c4a79f3ec0c1dba387ba713abad108b7568fe`. Добавлены шесть Markdown и пять файлов доказательств; общий README изменён. Все прежние файлы сохраняются. На этом срезе **295 planning + 260 protected = 555 различных Markdown-путей**, без включения ещё не опубликованного нашего пакета. Шесть новых документов включены в поиск; близкие механизмы сопоставлены по содержимому.

[DLT-K1](https://github.com/lubochka/xiigen-planning/blob/412d3364cc94b5aa1cdf516f829960da152826cb/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/PREPARETOGPL-DPO-LORA-TOKENS-2026-10-18.md#6-полезные-уточнения-известных-тем--не-увеличивают-число-новых-находок) уже описывает перенос C50 moments в ENRU, возможный конфликт первого момента с новым градиентом, cosine/sign diagnostics и сравнение carried state с новым optimizer при одном reference C50. **Эта общая гипотеза и её диагностика больше не заявляются здесь новыми.** N-NEURAL-02/R06 — отдельное уточнение: обнуляется только m50, а v50, step50 и остальные условия сохраняются. Это позволяет отделить направление первого момента от preconditioning и bias-correction, которые также меняются при создании нового optimizer. Новизна ограничена этим различающим вмешательством; актуальные slots и causal quality gain ещё не получены.

В новых [экспериментах](https://github.com/lubochka/xiigen-planning/blob/412d3364cc94b5aa1cdf516f829960da152826cb/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/PREPARETOGPL-EXPERIMENTS-2026-10-18.md) и [novelty-карте](https://github.com/lubochka/xiigen-planning/blob/412d3364cc94b5aa1cdf516f829960da152826cb/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/PREPARETOGPL-EVIDENCE-AND-NOVELTY-2026-10-18.md) не найден тот же фактический пересчёт N-NEURAL-01 — chosen↓20/20, rejected↓19/20, relative margin↑при chosen↓11/20. Наличие общей рекомендации смотреть absolute scores не приравнивается к этому измерению. Условная N-NEURAL-03 про live Alpha при одинаковом полном payload также отличается от DLT-1/RNG, DLT-2/FP32 reduction, DLT-3/one-key attention и N-L1/legacy base-only import.

Legacy4→6, точный старый checkpoint и zero-learning восстановление уже независимо рассмотрены несколькими авторами, включая новый [KNN/history](https://github.com/lubochka/xiigen-planning/blob/412d3364cc94b5aa1cdf516f829960da152826cb/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/PREPARETOGPL-KNN-HISTORY-2026-10-18.md). Наш R01 остаётся воспроизводимым подтверждением и первым опытом восстановления; ни находка файла, ни zero-learning идея не считаются исключительным открытием. Остальные новые результаты этого пакета принадлежат его авторам и не присвоены нашей работе.

Точные пути и Git blobs этого третьего delta находятся в `thirdPrepublicationCheck` [publication-delta.json](evidence/publication-delta.json). Публикация выполняется поверх данного parent с проверкой ожидаемого HEAD; обновление ref не должно переписывать параллельные результаты.
