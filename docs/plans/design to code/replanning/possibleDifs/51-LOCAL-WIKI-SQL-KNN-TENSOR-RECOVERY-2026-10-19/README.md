# Восстановление local_wiki: SQL, реальные получатели и новые адресные пробелы

**19 октября 2026. Доказательное исследование и проекты малых опытов.** Первым приоритетом оставлена указанная пользователем успешная ветка local_wiki_planning. Новые гипотезы сверены с обоими прежними каталогами; защищённый core-каталог использован только для чтения.

## Начать с восстановления

**[Глава 0 — порядок восстановления local_wiki_planning](00-LOCAL-WIKI-RECOVERY-FIRST.md).** Сначала одинаковый state/consumer, сохранение исходной DPO-тройки, новый SQL-перенос и место адаптации neural policy. Условные дефекты, которые уже были в успешной ветке, стоят после этих проверок.

**[Готовое задание на первый новый опыт — AC-SQL01](05-SMALL-DOMAIN-EXPERIMENTS.md).** A00/A10 меняет только TRAIN-представление SQL table effects. Один общий checkpoint/reference; 50 train +50 heldout; сначала1 эпоха/10 updates на плечо. Exact EXAM, токены, маски и scorer одинаковы. Автор обеих финальных сторон — внешняя открытая китайская модель, выбор задачи и отдельная проверка — CODEX в запрошенной конфигурации.

Это проверка конкретного механизма Wiki metric learner. Она не объявляется причиной неудачи neural RP01 до доказательства общего вызывающего пути. Старые DPO preference receipts не подменяют собственные ответы обычного consumer.

## Что нового добавляет пакет

| ID | Новый адресный результат | Отношение к успешной W | Степень доказательства |
|---|---|---|---|
| **AC-SQL01** | Удалены условные SQL read/write/result признаки chosen и отдельный table-effects-focus curriculum. Count-признаки сохранились | **Прямое W→C различие.** Найдены поздние SQL110/111/112 с47/50 heldout, прослежены исходные пары | Source delta и история подтверждены; полезность возврата отдельных частей требует matched A/B |
| **AC-EV02** | Request/Evidence доступны rich pair и structural evaluator, но не neural AnswerComposer prompt | Унаследовано от W; новый конкретный conditioning seam, родственен прежним WikiTrace/P118 | Код и совпадение question-only input подтверждены; gain от добавления evidence ещё не измерен |
| **NK-01** | Повторный Index одного ChunkId накапливает document frequency и может сделать релевантный признак отрицательным | Тело кода одинаково W/C; активируется при повторной записи, которую first-build комментарий не предусматривает | Формула и Python proxy; actual обновляющий product caller не установлен |
| **NK-02** | Query null check требует NullLiteralExpression:7, а обычный if(x==null) создаёт роль7 у EqualsExpression, роль4 у null | Тело одинаково W/C; C# structural retrieval | Источник и аналитические keys; native Roslyn trace/влияние на первый результат не выполнены |
| **NK-03** | KnnSkillFilter после уверенного global topK может вернуть успешный пустой список допустимых skills | Унаследовано; default manager использует другой фильтр | Новый concrete consumer известного принципа scope-before-topK из P104.1; источник и proxy подтверждены |
| **N-SHAPE-01** | Mean с keepDims=false удаляет ось в forward, но backward передаёт старое число координат | Унаследованный API-дефект; обычный RP01 keepDims=true | Код, rank guard, математический пример и независимый finite difference; **P2, не активная RP01 regression** |
| **N-SHAPE-02** | MatMul с внутренним singleton в leading axes выбирает B через modulo и смешивает примеры | Унаследованный forward; проверенные RP01 callers не имеют проблемной формы | Код, точный численный контрпример и независимый эталон; **P2, caller должен быть подтверждён** |

Это **семь адресных находок и уточнений с разным статусом**, а не семь доказанных причин ухудшения качества. Новизна относится к сверенным каталогам и дополнительному draft; отсутствие идеи во всех когда-либо существовавших ветках не заявляется. [Полная карта всех запрошенных механизмов и прежних владельцев](06-COVERAGE-AND-NOVELTY.md).

## Документы

| Документ | Для чего читать |
|---|---|
| [00-LOCAL-WIKI-RECOVERY-FIRST.md](00-LOCAL-WIKI-RECOVERY-FIRST.md) | Первая очередь: подтверждённые успехи W, потерянный сигнал, state/ordinary consumer и порядок опытов |
| [01-LEARNING-INTERNALS-BEFORE-AFTER.md](01-LEARNING-INTERNALS-BEFORE-AFTER.md) | Tokenizer/BPE, точные матрицы, q/v-LoRA, получатель градиента, DPO objective/masks, policy/reference/AdamW |
| [02-NEW-SQL-UNIT-GAPS.md](02-NEW-SQL-UNIT-GAPS.md) | Два новых AnswerComposer пункта; 106→113 история, read-only против write, input evidence, falsifiers |
| [03-NEW-KNN-CONSUMER-GAPS.md](03-NEW-KNN-CONSUMER-GAPS.md) | Три конкретных retrieval/skill риска, связь с AST и пределы доменного переноса |
| [04-TENSOR-CONDITIONAL-GAPS.md](04-TENSOR-CONDITIONAL-GAPS.md) | Mean/MatMul контрпримеры, controls и условия, при которых появится actual learner risk |
| [05-SMALL-DOMAIN-EXPERIMENTS.md](05-SMALL-DOMAIN-EXPERIMENTS.md) | Готовое задание первого опыта, общий протокол и малые SQL/C#/EN→RU/orchestration панели |
| [06-COVERAGE-AND-NOVELTY.md](06-COVERAGE-AND-NOVELTY.md) | DPO, LoRA, vectors, KNN, Units, Flow, restore, evaluation: что новое, что известно, что ещё не закрыто |
| [07-REVIEW-AND-METHOD.md](07-REVIEW-AND-METHOD.md) | Независимая проверка, исправленные замечания и границы полученного доказательства |
| [evidence/README.md](evidence/README.md) | Скрипты, результаты, manifests и воспроизведение проверенной арифметики/истории |

## Самые существенные факты

**SQL47/50 предшествует read-only correction.** В110 уже .94, но12 chosen ещё противоречивы; в111/112 противоречий0, accuracy та же.111→112 содержит те же100 троек и continuation state. В113 после расширения chosen результат44/50; его нельзя причинно объяснить только добавленными маркерами.106 имеет structural PASS, но full loop summary=false и publication skipped. Эти уточнения предотвращают повторное неверное прочтение «неудачного эксперимента». [Raw history и источники](02-NEW-SQL-UNIT-GAPS.md).

**У Wiki metric learner и RP01 разные получатели градиента.** На W при обычном backward ранний graph был отсоединён RmsNorm, достигалась lm_head; RP01 обучает 53248 q/v-LoRA floats в16 тензорах, head заморожена. Это известный восстановительный контраст, подтверждённый заново. Возвращать старые ошибки знака и autograd не предлагается. [Полная таблица](01-LEARNING-INTERNALS-BEFORE-AFTER.md).

**Evidence и полезный сигнал надо видеть после токенизации.** У Wiki default prefix48 слов; в выполненном synthetic append-примере prefixes уже различимы, но все необходимые facts в них не поместились. Это не равнозначно полной доставке evidence. Оценка до обучения отделяет эффект входа от эффекта уроков. [AC-EV02](02-NEW-SQL-UNIT-GAPS.md).

## Версии, охват и фактическое исполнение

- W:219d7ba89dc3ef43e03e17f1461ad96154d3dd7a. C:4945b8e85c17b5f150eb5b840d16bbdc4b506242. Исходный planning:4ca770a2c7354d7985f34c516f087e4b49cd01df.
- Дополнительно проверен core649b9343cfda5b81cc54396a3840ad5db5e0b179: один commit,7 docs-only файлов, программные источники C не изменились.
- Перед публикацией planning дополнительно проверен до4151916fac86becea8b7c2fd8cb6e73abb49cd79:12 commits,10 changed files. Новые N50/N51 сверены отдельно; предыдущая классификация E26 как статического lookup сохранена.
- Проверены bytes **1191 версии файлов прежнего корпуса**, затем7 обновлённых core-версий и10 planning-версий, и206 primary-source версий исследования. Это не число построчно прочитанных файлов и не число экспериментов.
- Выполнены три Python proxy: SQL receipt/input анализ, KNN arithmetic/control flow, TensorOps index/maths. Результаты повторены при финальной сборке; независимый reviewer проверил SQL и численные эталоны отдельно.
- **Native C#/Roslyn runs:0; новых тренировок:0; новых free-answer измерений:0; внешних teacher/judge model calls:0.** Исторические positive receipts сохранены как исторические; они не выданы за сегодняшние запуски.
- Для фактов доступны immutable GitHub ссылки и Git blob SHA. Binary исторического run не аттестован одним только SHA ветки, в которой лежит его отчёт.

Проектные навыки архитектурного аудита, планирования, проверки плана и кода, prompt authoring, experiment design и человеческой коммуникации применены к структуре доказательств и постановке опытов. Фактическая глубина чтения и независимый review описаны в07; большие skill/RAG документы не объявлены целиком прочитанными.
