# CP6.1 — опубликованный промежуточный результат пересмотра

Дата: 2026-10-13. Назначение: исправить действующий план, учитывая одновременные R01–R04. Это документный checkpoint автора плана, НЕ исполненный R01/R04/R10, не независимая экспертиза и не обучение.

## Уже установлено чтением актуальной ветки

1. README, PLAN-25-RUNS, START-NEXT-RUN и PARALLEL-R01-R03-JOIN закрепляют только параллельность R01–R03. Пользователь прямо добавил R04: независимый старт R04, его изолированная запись и включение в join должны появиться в действующих входах, а не только в приложении.
2. R01/FINDINGS имеет IN_PROGRESS. Сохранить F01–F13, refs, visited set, остаток; новый planning commit не причина начинать заново.
3. R02/FINDINGS сохраняет 52 исходных requirement IDs и уже разрешённое китайское авторство обеих final DPO sides. Повторно открывать это решение нельзя. Coldstart policy/reference margin — отдельный вопрос; перепроверить первичный RUN и actual consumer, не подменять одно другим.
4. R03/FINDINGS уже CP08, а не только CP07. Сохранены F01–F21 и добавлены F22/F23. В передаче учитывать запрет объявлять правильный deterministic ответ rejected только по происхождению; RAG traversal остаётся IN_PROGRESS.
5. Прямое чтение results/R04/FINDINGS.md вернуло 404. Это только отсутствие этого конкретного пути при чтении, не доказательство, что R04 не работает или результатов под другим именем нет. При сборке найти фактический carrier; чужой результат не создавать и completion не выдумывать.
6. SOURCE-ROUTES распределяет все WorkLeft/LeftToWork families, исходные prompts, S00–S10 и вложенные RAG. Большие архивные тела не становятся обязательной полной загрузкой для каждого worker.

Источники данного checkpoint: текущие README, START-NEXT-RUN, PLAN-25-RUNS, PARALLEL-R01-R03-JOIN; previous rounds thinking results.md (начальные разделы); SOURCE-ROUTES (начальные разделы и ownership); results/R01/FINDINGS (1–80), R02/FINDINGS (1–80), R03/FINDINGS (1–90); MODEL-RUN-PACKETS-CP5-01-10 (начальные разделы). Длинные ответы некоторых файлов ограничены выводом: полное чтение этих файлов не заявляется. Planning ref, прочитанный во время подготовки: de8da48af66094fcdcd803bd5ee97d780c8901ba. Источники будут уточняться адресно, не путём перезапуска workers.

## Неизменяемая предметная область

ILearningUnit — domain/vector/output schemas и typed many→many; ILearningFlow — отношения и коррекция; ILearningOrchestrator — asynchronous combinations; ILearningDynamicGraph — higher-order relations. Поведенческое сопоставление с ILogical*/ILearningMachine не разрешает уничтожать роль или создавать четыре новых trainer. Сохранить TensorCore, shared/project LoRA, domain/intent/stack, positive/negative/warning memory, final arbiter, source lineage и обычного потребителя результата.

DPO, dynamic domain kNN, vector tokenization/projection, связи алгоритмов, efficiency на ПК и продолжение обучения по feedback получают отдельные предметные поручения и различающие наблюдения. Coverage/Windows PASS не считается learning gain. Продуктовые tests/build/Actions/trainer этим пересмотром не запускаются; existing checks не отключаются.

## Пять этапов текущей правки документации

CP6.1: опубликован диагноз и границы сохранения.
CP6.2: применить независимый R04 и non-destructive R01–R04 join в действующих документах.
CP6.3: привести 25 model-ready поручений к одному понятному источнику запуска, с полными context/skills/source routes, A/B, хорошим/плохим/крайним примером и отдельной приёмкой.
CP6.4: закрепить предметные examination contracts: пары, domain laws, route/neighbor quality, coordinate/LoRA, interactions, bulk-to-bulk quality/time/footprint, feedback/retention и first-learning path.
CP6.5: документно сверить зависимости, перенос уникальных решений и опубликованные файлы; предъявить фактические commits и невыполненную эмпирическую часть.

Эти пять этапов авторской правки НЕ пять независимых model calls и НЕ замена базовых 10+5+5+5 проходов пользователя. Дополнительные реальные вызовы считаются отдельно. Checkpoint не назначает будущую фоновую работу и не доказывает применение оставшихся этапов.
