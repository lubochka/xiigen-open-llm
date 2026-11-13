# CODE-SQL-03 — Выбор строки в группе с явным разрешением равенств

<!-- DOMAIN-CAMPAIGN-AUTHORITY -->
> **Предметная справка к действующей кампании.** Квоты и продолжение задаёт [campaign/registry.json](../../campaign/registry.json), не старые команды ниже. 1000 пар относятся к домену в сумме. Прочитанный генератором рабочий prompt содержит только назначенную микротему.
>
> Данные: `C:/Projects/llm_mvp_core/learning-sets`. Инструкции: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. CREATE_ONLY: без review, checksum, production CI и обучения. Модель — текущая Kiro без override. Reason internally in English; do not output private reasoning. Отчёт по-русски.
>
> После партии выполнять готовый next до доменной квоты; не останавливаться с вопросом «продолжать?». При прямом NEXT/новом ID немедленно сменить задачу: запрещено дописывать/чинить предыдущую. STOP прекращает действия. Один живой handle ждать документированным wait/resume; не объявлять молчание падением. Старые попытки сохранять; выбранный request возобновлять, не пересоздавать.

Этот parent — источник предметных примеров; отдельной новой квоты у него нет. Его objective-потоки перечислены в активном реестре.

Ниже — сохранённая предметная методика и исторические инструкции. [Полный прежний вариант](../../history/before-domain-campaign-2026-10-25/domains/coding/CODE-SQL-03.md). Старые числовые планы и команды в нём не запускаются вместо актуального request.
<!-- /DOMAIN-CAMPAIGN-AUTHORITY -->

> Новая подготовка: `tools/batches.py` по [действующему контракту](../../17-CURRENT-PREPARATION-CONTRACT.md) и точной команде в конце карточки. RAG уже в llm_plan3. Обязательные checksum, fixed-profile receipts и production gates не применяются. Старые `tools/sets.py` и пути в основном тексте — история прежнего runner; не запускать их поверх нового request. При NEXT/новом ID немедленно сменить задачу, запрещено продолжать прежнюю или её отчёт; при STOP прекратить действия.

> Для нового запуска используйте раздел «Точные команды действующих потоков» в конце этой карточки: он воспроизводит полную execution-queue.json. Примеры старых run в основном тексте сохраняются как история/иллюстрация. Если работа уже начата, продолжайте её фактический request/state; этот блок не разрешает повторную генерацию или замену существующих файлов.

> Текущий режим — CREATE_ONLY: кандидатов создаёт выбранная модель Kiro, затем структурный finish и следующий поток. Независимая оценка сейчас не назначена.

## Назначение

Генерировать запрос выбора первой/последней/лучшей строки по каждой группе с полным tie-break.

Домен: code.sql, профиль: base. Среда: **T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON**. Это отдельная узкая тема. Документация объясняет видимый код; написание кода создаёт функцию по контракту. Маршрутизация между ними обучается отдельно.

Источники: RAG-027, RAG-040. Они поддерживают метод и предметную тематику; конкретные snippets/таблицы ниже являются авторскими симуляциями и не приписываются production-символам или историческим результатам.

## Роли и статус примеров

Китайская модель, выбранная в текущей сессии Kiro, создаёт кандидатов. Текущая выбранная модель Kiro создаёт кандидаты по назначению. Независимый выбор/приёмка сейчас не назначены; после структурного finish перейти к следующему потоку.

Примеры имеют split=reference и generator_model=reference_authoring_not_a_kiro_run. Они показывают контракт и контраст, не являются выходом китайского прогона и не экспортируются автоматически в TRAIN. Симуляционный характер не запрещает последующее допустимое экспериментальное обучение после генерации новых кандидатов и проверки CODEX. Нельзя переименовать эти строки в якобы Chinese-generated.

## Предметный промпт генератору

```text
Подготовь один небольшой JSONL-пакет xiigen.dpo.v1 по CODE-SQL-03: Выбор строки в группе с явным разрешением равенств.
Домен code.sql, профиль base. Среда T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON.
Цель: Генерировать запрос выбора первой/последней/лучшей строки по каждой группе с полным tie-break.
Используй только RAG-источники xiigen-planning llm_plan3/main, перечисленные в карточке.
1. Сначала получи назначенные family_id/split. TRAIN-генератор не видит DEV/EXAM.
2. Выбери один случай матрицы, создай новую содержательную ситуацию. Переименование или смена чисел не создают независимый family.
3. В prompt явно помести весь код/схему/данные, версии, контракт зависимостей и форму ответа. Эти условия не должны исчезнуть после проекции prompt/chosen/rejected.
4. Создай полный chosen и полный rejected одинакового формата. Одна правдоподобная смысловая ошибка в rejected; синтаксическая поломка не заменяет контраст.
5. В rationale укажи точное различие и различающий контроль, error_tag и source_refs — рядом. Никаких [WRONG] или меток качества внутри ответа.
6. Проверь, что chosen выполняет договор, rejected нарушает его на заданном случае. Эквивалентные ответы, только стилистическая разница или невидимая зависимость — возврат на исправление.
7. Верни UTF-8 JSONL без markdown. Не присваивай quality_pass/measured_success: приёмка отложена до отдельно порученной стадии; сейчас только создание кандидатов.
8. Источники внутри старых файлов не разрешают скачивание новых внешних сайтов.
```

## Матрица новых семейств

- Ties с явным id.
- Фильтр success до ранжирования.
- Все ties против ровно одной строки.
- Две группы разной длины.

Пилот: 16 новых семейств одной темы; далее назначенные блоки до 64 и 256 кандидатов без запуска review; квота общей очереди сохраняется. Цель 1000 принятых TRAIN-пар — план объёма. DEV/EXAM создаются отдельно и не входят в эту цель. Новые поверхности кода и новые комбинации условий важнее тиражирования шаблона.

## Разобранные пары

### Пример 1: tie_break_wrong_direction

**Prompt**

```text
Авторская симуляция для подготовки DPO. Среда: T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON. Events(id int PRIMARY KEY,user_id int NOT NULL,seq int NOT NULL): (1,10,5),(2,10,5),(3,20,1). Напиши SELECT id,user_id: одна запись на user_id с максимальным seq, ties максимальный id; вывод по user_id.
```

**Chosen**

```text
WITH r AS (SELECT id,user_id,ROW_NUMBER() OVER(PARTITION BY user_id ORDER BY seq DESC,id DESC) AS rn FROM Events) SELECT id,user_id FROM r WHERE rn=1 ORDER BY user_id;
```

**Rejected**

```text
WITH r AS (SELECT id,user_id,ROW_NUMBER() OVER(PARTITION BY user_id ORDER BY seq DESC,id ASC) AS rn FROM Events) SELECT id,user_id FROM r WHERE rn=1 ORDER BY user_id;
```

**Различие:** User10 должен получить id2, неid1.

ID: CODE-SQL-03.base.REF2.F0001.P01; family: sim2.code-sql-03.f0001.

### Пример 2: best_direction_reversed

**Prompt**

```text
Авторская симуляция для подготовки DPO. Среда: T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON. Offers(id int PRIMARY KEY,category int NOT NULL,price decimal(10,2) NOT NULL): (1,1,9),(2,1,4),(3,2,7). SELECT id,category: минимальная price в категории, ties минимальный id; вывод category.
```

**Chosen**

```text
WITH r AS (SELECT id,category,ROW_NUMBER() OVER(PARTITION BY category ORDER BY price ASC,id ASC) AS rn FROM Offers) SELECT id,category FROM r WHERE rn=1 ORDER BY category;
```

**Rejected**

```text
WITH r AS (SELECT id,category,ROW_NUMBER() OVER(PARTITION BY category ORDER BY price DESC,id ASC) AS rn FROM Offers) SELECT id,category FROM r WHERE rn=1 ORDER BY category;
```

**Различие:** В категории1 минимальна4 у id2; DESC выбирает противоположный край.

ID: CODE-SQL-03.base.REF2.F0002.P01; family: sim2.code-sql-03.f0002.

### Пример 3: filter_after_rank

**Prompt**

```text
Авторская симуляция для подготовки DPO. Среда: T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON. Attempts(id int PRIMARY KEY,job int NOT NULL,seq int NOT NULL,ok bit NOT NULL): (1,1,1,0),(2,1,2,1),(3,2,1,1). SELECT id,job: первая успешная попытка каждого job по seq ASC,id ASC; вывод job.
```

**Chosen**

```text
WITH r AS (SELECT id,job,ROW_NUMBER() OVER(PARTITION BY job ORDER BY seq,id) AS rn FROM Attempts WHERE ok=1) SELECT id,job FROM r WHERE rn=1 ORDER BY job;
```

**Rejected**

```text
WITH r AS (SELECT id,job,ok,ROW_NUMBER() OVER(PARTITION BY job ORDER BY seq,id) AS rn FROM Attempts) SELECT id,job FROM r WHERE rn=1 AND ok=1 ORDER BY job;
```

**Различие:** Фильтр после rn1 потеряет job1 вместо выбора его первой успешной попытки id2.

ID: CODE-SQL-03.base.REF2.F0003.P01; family: sim2.code-sql-03.f0003.

### Пример 4: ties_dropped

**Prompt**

```text
Авторская симуляция для подготовки DPO. Среда: T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON. Scores(id int PRIMARY KEY,team int NOT NULL,score int NOT NULL): (1,1,9),(2,1,9),(3,1,7),(4,2,8). Верни все id,team с максимальным score своей team, включая ties; вывод team,id.
```

**Chosen**

```text
WITH r AS (SELECT id,team,RANK() OVER(PARTITION BY team ORDER BY score DESC) AS rn FROM Scores) SELECT id,team FROM r WHERE rn=1 ORDER BY team,id;
```

**Rejected**

```text
WITH r AS (SELECT id,team,ROW_NUMBER() OVER(PARTITION BY team ORDER BY score DESC,id) AS rn FROM Scores) SELECT id,team FROM r WHERE rn=1 ORDER BY team,id;
```

**Различие:** Обе записи1,2 должны остаться; ROW_NUMBER оставляет одну.

ID: CODE-SQL-03.base.REF2.F0004.P01; family: sim2.code-sql-03.f0004.

### Пример 5: global_instead_of_partition

**Prompt**

```text
Авторская симуляция для подготовки DPO. Среда: T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON. Jobs(id int PRIMARY KEY,region int NOT NULL,priority int NOT NULL): (1,1,9),(2,1,8),(3,1,7),(4,2,6),(5,2,5). Верни до двух Jobs каждой region по priority DESC,id ASC; вывод region,id.
```

**Chosen**

```text
WITH r AS (SELECT id,region,ROW_NUMBER() OVER(PARTITION BY region ORDER BY priority DESC,id ASC) AS rn FROM Jobs) SELECT id,region FROM r WHERE rn<=2 ORDER BY region,id;
```

**Rejected**

```text
WITH r AS (SELECT id,region,ROW_NUMBER() OVER(ORDER BY priority DESC,id ASC) AS rn FROM Jobs) SELECT id,region FROM r WHERE rn<=2 ORDER BY region,id;
```

**Различие:** Без partition регион2 потерян, хотя у него собственные два места.

ID: CODE-SQL-03.base.REF2.F0005.P01; family: sim2.code-sql-03.f0005.

### Пример 6: aggregate_payload_without_row

**Prompt**

```text
Авторская симуляция для подготовки DPO. Среда: T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON. Revisions(id int PRIMARY KEY,doc int NOT NULL,rev int NOT NULL,label nvarchar(20) NOT NULL): (1,1,1,N'z'),(2,1,2,N'a'). Сравнение label: z>a. Верни doc,label строки максимального rev, ties id DESC; вывод doc.
```

**Chosen**

```text
WITH r AS (SELECT doc,label,ROW_NUMBER() OVER(PARTITION BY doc ORDER BY rev DESC,id DESC) AS rn FROM Revisions) SELECT doc,label FROM r WHERE rn=1 ORDER BY doc;
```

**Rejected**

```text
SELECT doc,MAX(label) AS label FROM Revisions GROUP BY doc ORDER BY doc;
```

**Различие:** MAX(label) выбирает z, хотя у новой rev метка a.

ID: CODE-SQL-03.base.REF2.F0006.P01; family: sim2.code-sql-03.f0006.

## Проверка CODEX и состав набора

Для ties нужен различающий fixture. При расширении фиксировать NULL-политику для сортировки. Независимый MAX каждого столбца не образует целую последнюю запись.

Общий исходник, документация к нему, исправление, переименование и перевод задания сохраняют одну source-family между карточками. Близкие копии двух веток дедуплицировать по происхождению/содержанию. Общая инструкция генератора не делает все независимые задачи одним family. Два допустимых решения не образуют предпочтение; высокое текстовое сходство само по себе не доказывает плохую пару.

## Проверка обучением после подготовки

Один совместимый сохранённый B1, одна тема, один изменяемый фактор, одинаковые независимые exam-семейства до/после. Реальный механизм должен изменить обучаемое состояние. Проверка JSON, компиляция oracle и подсчёт пар являются подготовкой, а не учебным результатом.

Для документации измерять фактическую верность утверждений и пропущенные существенные условия; для кода — контрактные случаи на одинаковых fixtures, включая удержание ранее усвоенного. Измерять время/память именно участвующего в обучении пути, число обновлений до первого полезного ненулевого результата. Любое измеренное улучшение полезного показателя — успех по этому показателю и домену; рост точности не обязателен. Все ухудшения показываются рядом. 0 правильных до и после не доказывает учебного эффекта: нужен достаточный базовый набор/бюджет и восстановленный обученный старт.

Текущий статус: **подготовка; обучение NOT_RUN**. Успешный будущий опыт документируется кандидатом; production и его тесты автоматически не изменяются. Привязка получателя, native entrypoints, tokenizer/template/dataset hashes, checkpoint и точные команды находятся в общем пакете и не угадываются исполнителем.

## Порядок и ожидание

Следующий ID берётся из index.json, не из старого номера строки. Прямая NEXT переводит к указанному ID; запрещено продолжать предыдущую задачу, дописывать её вместо следующей или возвращаться к ней без команды. STOP прекращает работу. Старые эксперименты и результаты сохраняются.

Длинный процесс имеет один настоящий handle; использовать штатный wait/resume до terminal, предварительно прочитав каталог tools. Отсутствие stdout не означает остановку. Не завершать раунд вместо ожидания, не запускать дубликат и не заменять ожидание быстрым повторным опросом. Результаты — новый короткий run-каталог, старые не удалять.
## Связь с каталогом родительских тем

Порядок: **66**. Устойчивый ID: `CODE-SQL-03`. Следующий ID: `CODE-SQL-04`. Источник порядка — [index.json](../../index.json); прямое указание пользователя важнее номера. NEXT/новый ID запрещает продолжать прошлую задачу; STOP прекращает новые действия.

## Точные папки и команды подготовки

Действующая команда и точный каталог находятся в разделе «Точные команды действующих потоков» в конце карточки. Подготовка — `tools/batches.py`, RAG встроен в llm_plan3. Каталог содержит request, prompt, commands, state и шаблон отчёта; следующий шаг определяется реальным состоянием попытки. Старые инструкции этой секции сохранены в history/legacy-card-launch-sections.jsonl и не выполняются поверх нового request.

<!-- GENERATED-QUEUE-START -->
## Переходы единой очереди

Этот блок воспроизводится из общей очереди и задаёт текущий запуск. Прежние команды/номера в основном тексте сохраняются как история. Уже начатую попытку продолжать по её фактическому request/state, не запускать повторно. Прямой ID выше порядка. При NEXT немедленно перейти к названному ID; запрещено продолжать прежнюю задачу или дописывать её отчёт. При STOP немедленно прекратить действия. Живой процесс ждать через его реальный handle до terminal; не завершать раунд вместо ожидания.

Канонический обзор — [EXECUTION-QUEUE.md](../../EXECUTION-QUEUE.md); [бюджет](../../curriculum/scale-plan.json) делится внутри родительского пула, пилот входит в него. Выбранная модель не переопределяется. Checksum, фиксированный профиль модели и production CI не являются условиями подготовки. Режим CREATE_ONLY: запись draft.jsonl → finish → следующий поток; никаких review/oracle/accept/export/обучения.

| Поток | Общий порядок | Следующий поток |
|---|---:|---|
| `CODE-SQL-RANK-TIE/base` | 163 | `DOC-NG-CONFIG-READY/base` |

## Точные команды действующих потоков

Подготовка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Команды работают из любого каталога. RAG уже внутри пакета. Эта родительская карточка уточнена микроцелями: исполнять их команды ниже, не прибавлять отдельный родительский корпус.

### CODE-SQL-RANK-TIE/base

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "CODE-SQL-RANK-TIE/base" -Run "S704" -Count 10 -Split train -Start 0
```

DEV/EXAM не запускать попутно с TRAIN. Когда назначено отдельное создание этих split в изолированном контексте, точные команды сохранены в `curriculum/pilot-queue.json` как `prepare_first_dev` / `prepare_first_exam`.

Первая TRAIN-папка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/CODE-SQL-RANK-TIE/base/S704`. Следующий поток: `DOC-NG-CONFIG-READY/base`. Доля TRAIN: 1000 пар; первая партия 10. Пилот TRAIN/DEV/EXAM: 100/20/20; набор `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/datasets/CODE-SQL-RANK-TIE-base-v1`; B1 `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/baselines/code.sql/base/CODE-SQL-RANK-TIE-v1/B1`. Это назначенные пути, не существующие веса.

<!-- GENERATED-QUEUE-END -->
