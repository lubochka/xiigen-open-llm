# 58. Балл Unit, конечный перевод и ограничение длины Student

2026-10-16. **READ_ONLY_RESEARCH**. Прочитан код и сохранённые ответы; новых вызовов Student, обучения, тестов и сборок нет. Это адресная проверка N33, а не новый экзамен. Core source: `019cb5a43e2fb817d4353c4218f6d5a90342ea0c`; helper — редакция после `4778f38b21`.

Главный результат: **ограничение в 4096 символов не объясняет C# 0/3** в проверенных шести сохранённых ответах. Второй результат: усреднение по Unit действительно существует, но найденные продуктовые runners получают в поле `AccuracyAfter` другую величину — охват обученных Unit, поскольку передают batch без held-out. Название поля само по себе не определяет метрику.

## 1. Что означает AccuracyAfter

В [TrainStructuredBatchAsync](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationMachine.cs:103) `AccuracyBefore` берётся из прошлого `_accuracyAfterLastBatch`. Это не повторная оценка исходного состояния на текущем наборе. После обучения выбирается одна из двух формул:

| Вход | Что считается | Что нельзя из этого вывести |
|---|---|---|
| Есть записи `HeldOut=true` | Среднее результатов отдельных групп `UnitId`, каждой группе одинаковый вес | Долю правильных пользовательских переводов или выигрыш на том же наборе до/после |
| Held-out отсутствует | Доля затронутых Unit из тройки SemanticTokenizer, ExecutorRuEn, QualityVerifier | Точность решения задач; тем более точность других языковых направлений |

Источник: [выбор формулы и сохранение](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationMachine.cs:124), [macro mean](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationMachine.cs:662), [coverage](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationMachine.cs:674). Известные раньше проблемы предыдущего балла и RU_EN coverage не считаются новыми открытиями N33.

Пример helper `1/1` у Unit X и `0/9` у Unit Y правильно даёт macro=0.50 и micro=0.10. Но эти десять записей — **задания для Unit**, не автоматически десять пользовательских запросов. Разные Unit решают разные задачи, а один запрос может проходить несколько Unit. Поэтому фраза «90% пользовательских запросов неверны» не следует из этой арифметики без отдельного соответствия запросам. Даже micro mean нельзя без проверки смешивать из проверки rule tag, перевода и проверки плохого кандидата.

## 2. Найден реальный потребитель, но важно проследить его вход

`TranslationProductHeldOutRunner` проверяет `AccuracyAfter >= AccuracyBefore + .02` вместе с runtimeProductAccuracy≥.95, ограничением bootstrap и проверками направления, алфавита, emoji и имён. Однако он создаёт batch с **heldOut:false** и свежую TranslationMachine. Значит в этой цепочке условие +.02 относится к coverage, а не к описанному macro mean. То же относится к его общей многоязычной ветке.

- [RU_EN batch и вызов](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationProductHeldOutRunner.cs:125), [составной gate](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationProductHeldOutRunner.cs:222).
- [Общий batch](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationProductHeldOutRunner.cs:308), [общий gate](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationProductHeldOutRunner.cs:408).
- `TranslationBatch029Runner` отдельно строит held-out batch, но в TrainStructuredBatch передаёт только trainBatch; held-out затем проверяется обычным orchestrator. Его gate также соединяет `.02`, runtimeProductAccuracy и bootstrap. [Построение и вызов](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationBatch029Runner.cs:152), [gate](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationBatch029Runner.cs:238).

Таким образом, найдено использование поля при определении `MeetsAccuracyTargets`, но **не доказано принятие модели только по macro mean**. Runners возвращают отчёт; фактическое продвижение checkpoint этим условием здесь не установлено. Отдельные проверки конечных ответов существуют, хотя их marker-based метрика требует собственной предметной оценки. Нельзя ни игнорировать этот контроль, ни называть его универсальным доказательством смысла.

## 3. MaxAnswerChars — ограничение потребителя, а не декодера

[Общий генератор](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/CodeDocumentationStudentGeneration.cs:61) проверяет положительность MaxAnswerChars и включает его в hash настроек, но передаёт декодеру MaxNewTokens, temperature, topK/topP и seed. Комментарий строки 75 прямо оставляет ограничение выдачи потребителю.

- [Simulation evaluation](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoEvaluation.cs:90) сохраняет raw receipt. Собственной семантической оценки в этой операции нет.
- [SavedModelAssessment](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/CodeDocumentationSavedModelAssessmentService.cs:548) сначала получает и при включённом capture сохраняет полный ответ, затем при превышении MaxAnswerChars возвращает GenerationFailed. Это отказ, не обрезка текста.

Разные SettingsSha256 поэтому не обязательно означают разные параметры декодирования. Различие контрактов действительно существует, но само по себе не доказывает ошибку реализации, утрату веса или ухудшение обучения. ENRU использует отдельный TranslationGeneration delegate; перенос вывода на него без проверки не выполнен.

## 4. Проверка на настоящих сохранённых C# ответах

Прочитаны шесть `generation-0000..0002.json` и два `intent.json` в каталогах:

- [C0](C:/Projects/llm_mvp_core/training-artifacts/experiments/code-documentation-simulation/simulation-csharp-c0-free3-20261015-01/intent.json)
- [Trial](C:/Projects/llm_mvp_core/training-artifacts/experiments/code-documentation-simulation/simulation-csharp-t-free3-20261015-01/intent.json)

Файловые SHA256 всех шести receipts и SHA256 текста каждого ответа совпали с [сохранённой таблицей соответствия](C:/Projects/llm_mvp_core/.kiro/xiigen-slices/work/results/RP01-simulation-main500-execution-20261015/unblind-mapping.private.json). Тексты закрытых заданий и ответов в этот отчёт не копируются. Это проверка сохранённого материала, не новая слепая оценка и не подтверждение всех binary pins.

| Случай | UTF-16 длина C0 / Trial | UTF-8 bytes C0 / Trial | Generated tokens | StopReason |
|---|---:|---:|---:|---|
| near-004 | 168 / 168 | 192 / 192 | 48 / 48 | LENGTH_LIMIT |
| near-012 | 152 / 152 | 170 / 170 | 48 / 48 | LENGTH_LIMIT |
| near-022 | 153 / 153 | 177 / 177 | 48 / 48 | LENGTH_LIMIT |

В обоих intent: maxAnswerChars=4096, maxNewTokens=48, temperature=0, topK=0, topP=1, seed=20261014. Ответы одной строки таблицы побайтно совпадают между C0 и Trial.

**Следствие:** конкретный char-limit отказ не сработал бы ни на одном из этих текстов; он не объясняет прежний результат C# 0/3. Наблюдаемый token-limit — другой вопрос. Из него нельзя заключить, что продолжение выдачи стало бы правильным, или пересчитать старый вердикт. Причина одинаковых свободных ответов остаётся предметом расследования.

## 5. Поправки к будущему эксперименту

Прочитаны полностью [N33 metric finding](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/33-NOVEL-UNIT-SCORE-AND-STUDENT-LENGTH-PARITY-2026-10-16.md>) и [предложенный micro protocol](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/33-MICRO-RECIPIENT-AND-METRIC-QUALITY-EXPERIMENT-2026-10-16.md>). Они не исполнялись.

Для будущей проверки знаменателя достаточно сохранять отдельно UnitId, число заданий и правильных решений каждой группы, тип метрики и реальные результаты Flow. Один и тот же frozen panel нужно оценивать на обоих состояниях. Coverage не переименовывать в accuracy. Аргумент 1/1 против 0/9 — арифметический контроль, а не доказательство качества реального перевода.

Для будущей проверки длины сначала определить контракт raw/served. Повторная дорогая генерация для объяснения уже сохранённых шести C# ответов не нужна: relevant char-limit гипотеза здесь исключена чтением. Проверка двух искусственно малых лимитов продемонстрирует политику потребителя, но не новое обучение.

В helper объединены независимые вопросы: Unit-обучение, restore, усреднение, char-limit и следующее занятие. Их не надо автоматически превращать в один обязательный запуск. Порог «+2/8» принадлежит предложению helper; он не принят пользователем как новая квота успеха и не переносится на RP01. Любое исполнение требует отдельного прямого разрешения. Чтение соответствующих tests не равно состоявшемуся историческому native запуску.

Открыто: caller с настоящим смешанным held-out, принимающий решение именно по macro mean; распространённость этой ошибки интерпретации в исторических отчётах; предел достижимого качества при текущем B0, DPO-материале и recipient. Эти вопросы не закрываются данной контрпроверкой.
