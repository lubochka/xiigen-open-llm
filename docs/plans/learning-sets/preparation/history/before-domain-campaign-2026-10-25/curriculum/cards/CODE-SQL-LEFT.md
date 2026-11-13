# CODE-SQL-LEFT — Написать LEFT JOIN с фильтром правой стороны

<!-- CREATE-ONLY-START -->
> **Действующий режим: CREATE_ONLY — только создание.** Прочитай назначение, создай реальные пары в `draft.jsonl`, выполни один структурный `finish`, перейди к следующему stream ID из общей очереди. Не запускай blind/review/oracle/accept/export/обучение и не жди проверяющего. `DRAFT_CREATED` означает запись кандидатов, а не проверенное качество.
>
> Корень наборов: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets`. Подготовка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Точные команды ниже работают из любого текущего каталога; готовый request содержит абсолютные пути. Не искать инструменты в `llm_mvp_core`, не создавать пустой RAG и не восстанавливать содержимое по checksum.
>
> Модель — текущая выбранная в Kiro, без override; не более одного помощника одновременно. При NEXT/новом ID немедленно перейти к нему, не дописывать прошлую задачу; STOP прекращает действия. Живой процесс ждать по его handle штатным wait/resume; timeout транспорта не доказывает отказ. [Точный контракт](../../20-CREATE-ONLY-AND-PATHS.md). Старые правила приёмки ниже описывают только будущую стадию.
> Роль человека при выборе материала выполняет CODEX с Astra 6 ultracode; оценку качества DPO также выполняет CODEX с Astra 6 ultracode. Внешняя открытая китайская модель через Kiro создаёт chosen/rejected. В CREATE_ONLY назначение уже задано карточкой/request: новый вызов выбора и оценка сейчас не запускаются и не задерживают создание следующей партии. Это назначение ролей, не подтверждение выполненного вызова.
<!-- CREATE-ONLY-END -->

> Новая подготовка: `tools/batches.py` по [действующему контракту](../../17-CURRENT-PREPARATION-CONTRACT.md) и точной команде в конце карточки. RAG уже в llm_plan3. Обязательные checksum, fixed-profile receipts и production gates не применяются. Старые `tools/sets.py` и пути в основном тексте — история прежнего runner; не запускать их поверх нового request. При NEXT/новом ID немедленно сменить задачу, запрещено продолжать прежнюю или её отчёт; при STOP прекратить действия.

> Для нового запуска используйте раздел «Точные команды действующих потоков» в конце этой карточки: он воспроизводит полную execution-queue.json. Примеры старых run в основном тексте сохраняются как история/иллюстрация. Если работа уже начата, продолжайте её фактический request/state; этот блок не разрешает повторную генерацию или замену существующих файлов.

Родительская тема: `CODE-SQL-02`. Домен: `code.sql`. Профиль: `base`. Это дополнение к существующей теме: прежние карточки и результаты сохраняются.

## Обучаемая граница

Сохранить левую строку без подходящей правой; не превращать фильтр совпадения в фильтр итоговых строк.

## Источники и смысл примеров

Источники: `RAG-040`, `RAG-041`. Точные commit/blob и локальные выдержки — [sources.lock.json](../../sources.lock.json) и RAG main. Они поддерживают методику и проектный контекст; описанные здесь мини-программы — авторские самодостаточные симуляции. В них нет измеренных результатов обучения. Число reference-строк не равно числу независимых семейств.

## Запуск подготовки

Действующая команда и точный каталог находятся в разделе «Точные команды действующих потоков» в конце карточки. Подготовка — `tools/batches.py`, RAG встроен в llm_plan3. Каталог содержит request, prompt, commands, state и шаблон отчёта; следующий шаг определяется реальным состоянием попытки. Старые инструкции этой секции сохранены в history/legacy-card-launch-sections.jsonl и не выполняются поверх нового request.

## Промпт генератору

```text
Генерируй только семантику LEFT JOIN и расположение одного фильтра правой стороны. Дай DDL, все строки, NULL-правила и ORDER BY. Один минимальный источник каждой семьи служит CODE и DOC с общим origin; независимые split выбираются до generation. Обязательные границы: нет детей; только неподходящие дети; NULL в фильтруемом поле; несколько подходящих детей; пустой левый вход как дополнительный контроль. Допустимы ON и эквивалентный filtered derived table; не делать корректную альтернативу rejected. Не добавляй агрегацию, transaction, DML и производительность. Rejected переносит фильтр в WHERE; тест обязан иметь null-extended родителя.

Работай с одной назначенной микроцелью, одним domain_id/profile_id и точными family_id/split/count из request.json. Все исходные данные, версии, видимый код и ограничения ответа находятся в prompt; metadata ученик не получает. Chosen и rejected имеют одинаковый формат, а ошибочная сторона нарушает одну проверяемую границу. Не добавляй [WRONG], score, rationale или source ID в обучаемые ответы. Источники задают методику; авторские симуляции не выдавать за цитаты реального проекта, результаты китайской модели либо измеренный эффект обучения. Публичные reference-примеры и их потомки не помещать в DEV/EXAM. Семейство одного исходника общее для документации, кода и перефразирования; переименование и новые числа не создают независимости. Для новых пар сначала назначить происхождение и split; не наполнять квоту поверхностными дубликатами. Пиши отчёт на русском по 08-REPORT-TEMPLATE.md. Используй фактическую текущую модель Kiro без override, не угадывай имя/версию. Если точная модель не показана, сохраняй наблюдаемый selector и model_identity_observed=false. При явном NEXT сразу переходи к указанному пользователем ID; не возвращайся к прежней карточке. При STOP немедленно прекрати новые действия. Старые эксперименты и версии данных не удаляй; production не меняй. Один долгий процесс имеет один handle: обнаружь штатный wait/resume и дождись terminal в текущем раунде; не запускай заново вместо ожидания, не делай busy polling и не объявляй таймаут по отсутствию первых файлов. Oracle до фактического исполнения имеет NOT_RUN, result_observed=false. Runtime oracle сейчас NOT_RUN; это не препятствует записи кандидатов и не означает PASS. Учебный пилот позже восстанавливает совместимое обученное B1 с tokenizer/optimizer/RNG/split hashes; новое обучение B1 только по назначенному отдельному bootstrap-плану, с сохранением для повторного использования. 0 правильных до и после не доказывает обучение; loss/margin и число пар не заменяют полезный результат. Любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост точности не обязателен, изменения остальных показателей, включая ухудшения, показать рядом. Успешный опыт документируется кандидатом в исправления; перенос в production не выполняется автоматически.
```

## Договор ответа

DOC: ordered tuples и краткое объяснение. CODE: один SELECT с указанными полями/ORDER BY.

## Оси генерации

- **правые строки:** нет; только false/NULL; несколько подходящих; разный порог у родителя
- **форма фильтра:** ON; derived table before LEFT JOIN

## Не смешивать с соседними темами

- COUNT/агрегация
- порядок строк без ORDER BY
- предположения об индексах/плане/скорости
- транзакции и обновления

## Проверка смысла

- Исполнять в SQL Server 2019 с ANSI_NULLS ON
- Свежие temp tables для каждого кандидата
- Chosen сохраняет всех родителей и multiplicity
- Rejected должен реально потерять null-extended строку
- Пустой left input не используется как единственное доказательство: там обе стороны равны

Oracle создаёт reviewer в подкаталоге `oracle/` этого run, с точными fixture из каждой принятой строки. Это следующий шаг подготовки; готового native oracle-run результата здесь нет. Harness не получает error_tag/chosen-label как вход бизнес-функции. Compiler PASS и ручное рассуждение не заменяют runtime. Для CODE исполняются две реализации при одинаковых входах; для DOC исполняется единственный показанный в prompt код, затем обе текстовые/JSON-гипотезы сверяются с его trace. Нельзя выдавать описание ошибочного кода за исходную программу или компилировать русский текст вместо наблюдения. Для каждого executable candidate свежая среда.

### SQL fixture: точный состав и команда

Создать `oracle/verify.sql` из metadata.oracle: свежие #temp tables на каждый candidate; BEGIN TRY/BEGIN TRAN; setup; query; записать фактические строки и сравнить с expected; ROLLBACK; END TRY/BEGIN CATCH с cleanup и THROW. Для UPDATE сохранить @@ROWCOUNT первым действием после UPDATE. Адрес уже настроенного SQL Server 2019 test instance находится в `XIIGEN_SQLSERVER`; Windows integrated auth. Без настроенного instance/Sqlcmd — ORACLE_PENDING, не запускать SQLite вместо него.

```powershell
sqlcmd -S "$env:XIIGEN_SQLSERVER" -E -b -i "C:/x/lsets/CODE-SQL-02/base/D306/oracle/verify.sql" -o "C:/x/lsets/CODE-SQL-02/base/D306/oracle/result.log"
```

## Полные публичные контрасты

### CODE-SQL-LEFT.REF.01

Общее семейство источника: `v3.ref.sql.left-join.filtered-right`.

**Условие:**

```text
Авторская симуляция. T-SQL / SQL Server 2019; ANSI_NULLS ON. Полный setup: CREATE TABLE #P (Id int NOT NULL PRIMARY KEY); CREATE TABLE #R (Id int NOT NULL PRIMARY KEY, ParentId int NOT NULL, Active bit NULL); INSERT #P VALUES (1),(2); INSERT #R VALUES (10,1,1),(11,1,0); Напиши ровно SELECT. Вернуть каждую строку #P, а из #R только Active=1; если подходящих #R нет, RightId=null. Поля Id,RightId; ORDER BY p.Id,r.Id. Ответ — SQL без Markdown; setup не повторять.
```

**Предпочтительный ответ:**

```text
SELECT p.Id, r.Id AS RightId FROM #P p LEFT JOIN #R r ON r.ParentId=p.Id AND r.Active=1 ORDER BY p.Id,r.Id;
```

**Правдоподобный ошибочный ответ:**

```text
SELECT p.Id, r.Id AS RightId FROM #P p LEFT JOIN #R r ON r.ParentId=p.Id WHERE r.Active=1 ORDER BY p.Id,r.Id;
```

**Почему различаются:** Фильтр правой стороны в ON/подзапросе ограничивает совпадения. Перенос в WHERE удаляет дополненные NULL строки и меняет договор.

**Контрпример:** Ожидается [(1,10),(2,null)]; вариант с WHERE даёт [(1,10)].

### CODE-SQL-LEFT.REF.02

Общее семейство источника: `v3.ref.sql.left-join.filtered-right`.

**Условие:**

```text
Авторская симуляция. T-SQL / SQL Server 2019; ANSI_NULLS ON. Полный setup: CREATE TABLE #P (Id int NOT NULL PRIMARY KEY); CREATE TABLE #R (Id int NOT NULL PRIMARY KEY, ParentId int NOT NULL, Active bit NULL); INSERT #P VALUES (1),(2); INSERT #R VALUES (10,1,0),(20,2,NULL); Напиши ровно SELECT. Сохранить обе родительские строки, хотя у них есть только Active=0/NULL. Только Active=1 даёт совпадение; NULL — не true. Поля Id,RightId; ORDER BY p.Id,r.Id. Ответ — SQL без Markdown; setup не повторять.
```

**Предпочтительный ответ:**

```text
SELECT p.Id, r.Id AS RightId FROM #P p LEFT JOIN #R r ON r.ParentId=p.Id AND r.Active=1 ORDER BY p.Id,r.Id;
```

**Правдоподобный ошибочный ответ:**

```text
SELECT p.Id, r.Id AS RightId FROM #P p LEFT JOIN #R r ON r.ParentId=p.Id WHERE r.Active=1 ORDER BY p.Id,r.Id;
```

**Почему различаются:** Фильтр правой стороны в ON/подзапросе ограничивает совпадения. Перенос в WHERE удаляет дополненные NULL строки и меняет договор.

**Контрпример:** Ожидается [(1,null),(2,null)]; вариант с WHERE даёт [].

### CODE-SQL-LEFT.REF.03

Общее семейство источника: `v3.ref.sql.left-join.derived-table-filter`.

**Условие:**

```text
Авторская симуляция. T-SQL / SQL Server 2019; ANSI_NULLS ON. Полный setup: CREATE TABLE #P (Id int NOT NULL PRIMARY KEY); CREATE TABLE #R (Id int NOT NULL PRIMARY KEY, ParentId int NOT NULL, Score int NULL); INSERT #P VALUES (1),(2); INSERT #R VALUES (10,1,5),(11,1,9),(12,1,9); Напиши ровно SELECT. Вернуть все #P и каждую подходящую #R со Score>=8, сохраняя повторы родителя по разным детям. Фильтровать #R в derived table; без DISTINCT/TOP. Поля Id,RightId; ORDER BY p.Id,r.Id. Ответ — SQL без Markdown; setup не повторять.
```

**Предпочтительный ответ:**

```text
SELECT p.Id, r.Id AS RightId FROM #P p LEFT JOIN (SELECT Id,ParentId FROM #R WHERE Score>=8) r ON r.ParentId=p.Id ORDER BY p.Id,r.Id;
```

**Правдоподобный ошибочный ответ:**

```text
SELECT p.Id, r.Id AS RightId FROM #P p LEFT JOIN #R r ON r.ParentId=p.Id WHERE r.Score>=8 ORDER BY p.Id,r.Id;
```

**Почему различаются:** Фильтр правой стороны в ON/подзапросе ограничивает совпадения. Перенос в WHERE удаляет дополненные NULL строки и меняет договор.

**Контрпример:** Ожидается [(1,11),(1,12),(2,null)]; вариант с WHERE даёт [(1,11),(1,12)].

### CODE-SQL-LEFT.REF.04

Общее семейство источника: `v3.ref.sql.left-join.optional-bound`.

**Условие:**

```text
Авторская симуляция. T-SQL / SQL Server 2019; ANSI_NULLS ON. Полный setup: CREATE TABLE #P (Id int NOT NULL PRIMARY KEY, MinScore int NOT NULL); CREATE TABLE #R (Id int NOT NULL PRIMARY KEY, ParentId int NOT NULL, Score int NULL); INSERT #P VALUES (1,8),(2,3),(3,7); INSERT #R VALUES (10,1,5),(20,2,4); Напиши ровно SELECT. Сохранить всех родителей. Условие подходящего ребёнка Score>=MinScore его родителя. Родитель без подходящего ребёнка даёт RightId=null. Поля Id,RightId; ORDER BY p.Id,r.Id. Ответ — SQL без Markdown; setup не повторять.
```

**Предпочтительный ответ:**

```text
SELECT p.Id, r.Id AS RightId FROM #P p LEFT JOIN #R r ON r.ParentId=p.Id AND r.Score>=p.MinScore ORDER BY p.Id,r.Id;
```

**Правдоподобный ошибочный ответ:**

```text
SELECT p.Id, r.Id AS RightId FROM #P p LEFT JOIN #R r ON r.ParentId=p.Id WHERE r.Score>=p.MinScore ORDER BY p.Id,r.Id;
```

**Почему различаются:** Фильтр правой стороны в ON/подзапросе ограничивает совпадения. Перенос в WHERE удаляет дополненные NULL строки и меняет договор.

**Контрпример:** Ожидается [(1,null),(2,20),(3,null)]; вариант с WHERE даёт [(2,20)].

## Учебный пилот и переход дальше

Подготовка здесь не запускает обучение. Последующий пилот выполняется по [05-LEARNING-AND-CHECKPOINTS.md](../../05-LEARNING-AND-CHECKPOINTS.md) и [10-NATIVE-CONTRACT-BOUNDARY.md](../../10-NATIVE-CONTRACT-BOUNDARY.md). Получатель, training/inference entrypoints, данные и hashes состояния задаются binding; не выдумывать runtime по domain ID. Восстановить совместимое ненулевое B1; один исходный checkpoint для обоих плеч. DEV-калибровка и бюджет должны обеспечить действительное обучение; 0→0 не является доказательством. Измерять полезную способность, время урока/число обновлений до неё, удержание старого навыка и ресурсы участвующего пути. Успех одного показателя допускается с регрессией другого, оба показываются. Не переносить изменения в production по этому документу.

Канонический порядок — `curriculum/index.json:stream_order`, следующая запись определяется по objective_id/profile_id, не по номеру родительской темы. Прямой ID пользователя имеет приоритет. При NEXT перейти немедленно и не дописывать прошлый эксперимент; при STOP остановить новые действия. Старые файлы сохраняются. Не завершать рабочий раунд вместо штатного ожидания действующего процесса. Отчёт на русском, фактическая текущая Kiro-модель.

<!-- GENERATED-QUEUE-START -->
## Переходы единой очереди

Этот блок воспроизводится из общей очереди и задаёт текущий запуск. Прежние команды/номера в основном тексте сохраняются как история. Уже начатую попытку продолжать по её фактическому request/state, не запускать повторно. Прямой ID выше порядка. При NEXT немедленно перейти к названному ID; запрещено продолжать прежнюю задачу или дописывать её отчёт. При STOP немедленно прекратить действия. Живой процесс ждать через его реальный handle до terminal; не завершать раунд вместо ожидания.

Канонический обзор — [EXECUTION-QUEUE.md](../../EXECUTION-QUEUE.md); [бюджет](../../curriculum/scale-plan.json) делится внутри родительского пула, пилот входит в него. Выбранная модель не переопределяется. Checksum, фиксированный профиль модели и production CI не являются условиями подготовки. Режим CREATE_ONLY: запись draft.jsonl → finish → следующий поток; никаких review/oracle/accept/export/обучения.

| Поток | Общий порядок | Следующий поток |
|---|---:|---|
| `CODE-SQL-LEFT/base` | 92 | `DOC-SQL-GUARD/base` |

## Точные команды действующих потоков

Подготовка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Команды работают из любого каталога. RAG уже внутри пакета. Выполнить только назначенный поток; остальные строки — точные альтернативы назначения.

### CODE-SQL-LEFT/base

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "CODE-SQL-LEFT/base" -Run "D306" -Count 10 -Split train -Start 0
```

DEV/EXAM не запускать попутно с TRAIN. Когда назначено отдельное создание этих split в изолированном контексте, точные команды сохранены в `curriculum/pilot-queue.json` как `prepare_first_dev` / `prepare_first_exam`.

Первая TRAIN-папка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/CODE-SQL-LEFT/base/D306`. Следующий поток: `DOC-SQL-GUARD/base`. Доля TRAIN: 334 пар; первая партия 10. Пилот TRAIN/DEV/EXAM: 100/20/20; набор `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/datasets/CODE-SQL-LEFT-base-v1`; B1 `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/baselines/code.sql/base/CODE-SQL-LEFT-v1/B1`. Это назначенные пути, не существующие веса.

<!-- GENERATED-QUEUE-END -->
