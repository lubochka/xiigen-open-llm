# CODE-SQL-RANK-TIE — Выбрать точную строку группы и сохранить её payload

<!-- CREATE-ONLY-START -->
> **Действующий режим: CREATE_ONLY — только создание.** Прочитай назначение, создай реальные пары в `draft.jsonl`, выполни один структурный `finish`, перейди к следующему stream ID из общей очереди. Не запускай blind/review/oracle/accept/export/обучение и не жди проверяющего. `DRAFT_CREATED` означает запись кандидатов, а не проверенное качество.
>
> Корень наборов: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets`. Подготовка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Точные команды ниже работают из любого текущего каталога; готовый request содержит абсолютные пути. Не искать инструменты в `llm_mvp_core`, не создавать пустой RAG и не восстанавливать содержимое по checksum.
>
> Модель — текущая выбранная в Kiro, без override; не более одного помощника одновременно. При NEXT/новом ID немедленно перейти к нему, не дописывать прошлую задачу; STOP прекращает действия. Живой процесс ждать по его handle штатным wait/resume; timeout транспорта не доказывает отказ. [Точный контракт](../../20-CREATE-ONLY-AND-PATHS.md). Старые правила приёмки ниже описывают только будущую стадию.
> Роль человека при выборе материала выполняет CODEX с Astra 6 ultracode; оценку качества DPO также выполняет CODEX с Astra 6 ultracode. Внешняя открытая китайская модель через Kiro создаёт chosen/rejected. В CREATE_ONLY назначение уже задано карточкой/request: новый вызов выбора и оценка сейчас не запускаются и не задерживают создание следующей партии. Это назначение ролей, не подтверждение выполненного вызова.
<!-- CREATE-ONLY-END -->

## Узкая задача

Построить T-SQL запрос выбора строк по полному ключу группы, критерию, tie-break и политике отсутствия кандидата.

Родитель: `CODE-SQL-03`. Предметные домены: `code.sql`. Профили: `base`. Один request содержит только назначенный профиль и эту микроцель.

Статус: авторский материал подготовки. Публичные пары ниже — reference; их нельзя копировать в новый корпус или выдавать за скрытый экзамен. Китайская генерация, оценка Astra и Student для нового материала не выполнялись; ожидаемые результаты не являются измерениями.

## Проверенные источники и применимость

- [RAG-027](../../rag/sources/RAG-027.md): закреплённая методика или исходная тема; конкретные новые случаи являются авторскими симуляциями.
- [RAG-040](../../rag/sources/RAG-040.md): закреплённая методика или исходная тема; конкретные новые случаи являются авторскими симуляциями.

[Разобранные good/bad примеры и логика](../../prompts/SOFTWARE-PIPELINE-WINDOW-READY-RU.md). Основное задание и все reference выбранного профиля автоматически входят в batch prompt.

## Подробный промпт исполнителю

Создавай от 3 до 6 строк в явно объявленной таблице и запрос выбора победителя. Укажи выходные столбцы и финальную сортировку. Полностью задай ключ группы, направление сравнения, правило при равенстве, допустимость всех равных максимумов, фильтр кандидатов, NULL и необходимость сохранить пустую группу. Правильный оператор определяется контрактом: TOP(1), ROW_NUMBER и DENSE_RANK не являются безусловно хорошими или плохими. Rejected должен быть синтаксически допустимым и ошибаться на показанном наборе по одной основной причине: неверный tie-break, фильтр после ранжирования, укороченный ключ, потеря пустой группы или payload чужой строки.

Простая самопроверка:
1. Раздели строки по полному ключу группы.
2. Примени фильтр допустимости в указанной стадии.
3. Отсортируй ID внутри каждой группы по всем критериям.
4. Выпиши требуемые ID и payload из тех же строк.
5. Проверь chosen и rejected на одной таблице: rejected обязан дать другое наблюдаемое значение.

Работай по одному готовому request.json: один topic_id, objective_id, domain_id, profile_id и split. В каждом prompt повтори среду, полный видимый код/данные, точку наблюдения и форму ответа: metadata не попадает в строгую проекцию prompt/chosen/rejected. Создай полный chosen и близкий rejected с одним основным содержательным дефектом, одинакового формата. Не заменяй rejected синтаксической ошибкой, шуткой, заведомо коротким ответом или другой правильной реализацией. Объяснение проверки, source_refs и error_tag записывай вне ответов.
Сначала выпиши маленькую таблицу состояния либо результата по условию, затем сравни с ней обе альтернативы. Это самопроверка автора кандидатов, не независимая оценка качества и не измерение обучения. Если по указанному входу ответы одинаковы, измени различающий вход в пределах задания; не придумывай победу. Смена имени или чисел сама по себе не создаёт независимую семью. Производные, обратные и DOC/CODE-варианты конкретного случая наследуют общий origin/leakage group и split. Публичные reference не являются закрытым EXAM.
Источники — только назначенные файлы xiigen-planning main/llm_plan3. Они дают методику и исходную тематику. Новые фрагменты являются авторскими симуляциями; runtime/oracle сейчас NOT_RUN. Не объявляй новую симуляцию извлечённым production-кодом, измеренным результатом или китайской генерацией.
Роль человека при выборе материала выполняет CODEX с Astra 6 ultracode; оценку качества DPO также выполняет CODEX с Astra 6 ultracode. Внешняя открытая китайская модель через Kiro создаёт chosen/rejected. Сейчас CREATE_ONLY: назначение уже дано карточкой/request. Не запускай новые selection/review/oracle/accept/export/Student/production CI и не задерживай следующую партию ради них. Модель — фактически текущая выбранная в Kiro, без override; неизвестное имя/session запиши unknown. Checksum, подписи, profile receipts и независимый review не нужны для создания.
Корень наборов C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets; подготовка находится в preparation внутри него. Возьми абсолютные входные и выходные пути из request.paths, не ищи runner в llm_mvp_core. После записи реальных JSONL-строк выполни одну готовую команду finish, получи структурный DRAFT_CREATED и перейди к next_stream_id полной execution-queue.json. PREPARED без draft не считается созданием. Старые данные/попытки не удалять; существующий run продолжать через resume.
При прямом NEXT/новом ID немедленно перейти к нему: запрещено продолжать предыдущую задачу, её отчёт или исправления. STOP прекращает действия сразу. Живой процесс ждать штатным wait/resume по единственному handle до terminal в том же ходе; не запускать дубликаты, не делать частый polling и не завершать ход словом «жду». Отчёт по-русски: тема, созданные строки/семьи, точный файл, фактическая модель, структурный статус, следующий ID. Нельзя приписывать вызов Astra текущей модели.

## Форма ответа ученика

Только законченный T-SQL SELECT/CTE с предоставленными табличными переменными; без DDL, Markdown и пояснений.

## Что варьировать

```json
[
  {
    "axis": "выбор",
    "values": [
      "один победитель",
      "все максимумы",
      "самый ранний",
      "NULL последним"
    ]
  },
  {
    "axis": "ключ и фильтр",
    "values": [
      "один ключ",
      "составной ключ",
      "фильтр до ранжирования",
      "пустая группа"
    ]
  },
  {
    "axis": "результат",
    "values": [
      "ID",
      "payload выбранной строки",
      "явный порядок"
    ]
  }
]
```

## Разобранные пары

### 1. CODE-SQL-RANK-TIE.REF.01 — base

**Задача ученику:**

~~~~text
Авторская симуляция T-SQL / SQL Server 2019; compatibility level150; ANSI_NULLS ON. Табличные переменные пусты до INSERT; других строк нет. DECLARE @E TABLE(Id int PRIMARY KEY,UserId int NOT NULL,Seq int NOT NULL); INSERT @E VALUES(11,1,4),(12,1,4),(13,2,2); Напиши SELECT Id,UserId: по одной строке на UserId, максимальный Seq, при равенстве — максимальный Id. Вывод по UserId.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
;WITH R AS (SELECT Id,UserId,ROW_NUMBER() OVER(PARTITION BY UserId ORDER BY Seq DESC,Id DESC) AS rn FROM @E) SELECT Id,UserId FROM R WHERE rn=1 ORDER BY UserId;
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
;WITH R AS (SELECT Id,UserId,ROW_NUMBER() OVER(PARTITION BY UserId ORDER BY Seq DESC,Id ASC) AS rn FROM @E) SELECT Id,UserId FROM R WHERE rn=1 ORDER BY UserId;
~~~~

**Почему есть предпочтение:** При равном Seq rejected выбирает меньший Id.

Различающий дефект: `tie_break_reversed`. Семья: `sim2.code-sql-03.f0001`.

### 2. CODE-SQL-RANK-TIE.REF.02 — base

**Задача ученику:**

~~~~text
Авторская симуляция T-SQL / SQL Server 2019; compatibility level150; ANSI_NULLS ON. Табличные переменные пусты до INSERT; других строк нет. DECLARE @E TABLE(Id int PRIMARY KEY,GroupId int NOT NULL,Seq int NOT NULL); INSERT @E VALUES(21,1,2),(22,1,2),(23,1,3),(24,2,1); Напиши SELECT Id,GroupId: минимальный Seq, затем минимальный Id. Вывод по GroupId.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
;WITH R AS (SELECT Id,GroupId,ROW_NUMBER() OVER(PARTITION BY GroupId ORDER BY Seq ASC,Id ASC) AS rn FROM @E) SELECT Id,GroupId FROM R WHERE rn=1 ORDER BY GroupId;
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
;WITH R AS (SELECT Id,GroupId,ROW_NUMBER() OVER(PARTITION BY GroupId ORDER BY Seq ASC,Id DESC) AS rn FROM @E) SELECT Id,GroupId FROM R WHERE rn=1 ORDER BY GroupId;
~~~~

**Почему есть предпочтение:** Seq выбран правильно; из равных требуется меньший Id.

Различающий дефект: `earliest_tie_break_reversed`. Семья: `sim2.code-sql-03.f0002`.

### 3. CODE-SQL-RANK-TIE.REF.03 — base

**Задача ученику:**

~~~~text
Авторская симуляция T-SQL / SQL Server 2019; compatibility level150; ANSI_NULLS ON. Табличные переменные пусты до INSERT; других строк нет. DECLARE @S TABLE(Id int PRIMARY KEY,Team int NOT NULL,Score int NOT NULL); INSERT @S VALUES(1,1,9),(2,1,9),(3,1,7),(4,2,8); Верни SELECT Id,Team всех строк с максимальным Score своей Team, включая все равные максимумы. Вывод по Team,Id.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
;WITH R AS (SELECT Id,Team,DENSE_RANK() OVER(PARTITION BY Team ORDER BY Score DESC) AS r FROM @S) SELECT Id,Team FROM R WHERE r=1 ORDER BY Team,Id;
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
;WITH R AS (SELECT Id,Team,ROW_NUMBER() OVER(PARTITION BY Team ORDER BY Score DESC,Id ASC) AS r FROM @S) SELECT Id,Team FROM R WHERE r=1 ORDER BY Team,Id;
~~~~

**Почему есть предпочтение:** ROW_NUMBER оставляет одну строку вместо всех равных максимумов.

Различающий дефект: `ties_dropped`. Семья: `sim2.code-sql-03.f0004`.

### 4. CODE-SQL-RANK-TIE.REF.04 — base

**Задача ученику:**

~~~~text
Авторская симуляция T-SQL / SQL Server 2019; compatibility level150; ANSI_NULLS ON. Табличные переменные пусты до INSERT; других строк нет. DECLARE @A TABLE(Id int PRIMARY KEY,Job int NOT NULL,Seq int NOT NULL,Ok bit NOT NULL); INSERT @A VALUES(1,1,1,0),(2,1,2,1),(3,2,1,1); Напиши SELECT Id,Job первой успешной попытки каждого Job: только Ok=1; порядок Seq ASC,Id ASC. Вывод по Job.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
;WITH R AS (SELECT Id,Job,ROW_NUMBER() OVER(PARTITION BY Job ORDER BY Seq ASC,Id ASC) AS rn FROM @A WHERE Ok=1) SELECT Id,Job FROM R WHERE rn=1 ORDER BY Job;
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
;WITH R AS (SELECT Id,Job,Ok,ROW_NUMBER() OVER(PARTITION BY Job ORDER BY Seq ASC,Id ASC) AS rn FROM @A) SELECT Id,Job FROM R WHERE rn=1 AND Ok=1 ORDER BY Job;
~~~~

**Почему есть предпочтение:** Ранняя неуспешная попытка занимает rn=1 и скрывает позднюю успешную.

Различающий дефект: `eligibility_filtered_after_rank`. Семья: `sim2.code-sql-03.f0003`.

### 5. CODE-SQL-RANK-TIE.REF.05 — base

**Задача ученику:**

~~~~text
Авторская симуляция T-SQL / SQL Server 2019; compatibility level150; ANSI_NULLS ON. Табличные переменные пусты до INSERT; других строк нет. DECLARE @E TABLE(Id int PRIMARY KEY,Tenant int NOT NULL,UserId int NOT NULL,Seq int NOT NULL); INSERT @E VALUES(1,1,7,5),(2,2,7,9),(3,1,7,6); Напиши SELECT Id,Tenant,UserId: последняя строка каждой пары (Tenant,UserId), порядок Seq DESC,Id DESC. Вывод по Tenant,UserId.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
;WITH R AS (SELECT Id,Tenant,UserId,ROW_NUMBER() OVER(PARTITION BY Tenant,UserId ORDER BY Seq DESC,Id DESC) AS rn FROM @E) SELECT Id,Tenant,UserId FROM R WHERE rn=1 ORDER BY Tenant,UserId;
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
;WITH R AS (SELECT Id,Tenant,UserId,ROW_NUMBER() OVER(PARTITION BY UserId ORDER BY Seq DESC,Id DESC) AS rn FROM @E) SELECT Id,Tenant,UserId FROM R WHERE rn=1 ORDER BY Tenant,UserId;
~~~~

**Почему есть предпочтение:** Слиты две группы разных Tenant с одинаковым UserId.

Различающий дефект: `tenant_key_dropped`. Семья: `v7.ref.sql.compound-partition`.

### 6. CODE-SQL-RANK-TIE.REF.06 — base

**Задача ученику:**

~~~~text
Авторская симуляция T-SQL / SQL Server 2019; compatibility level150; ANSI_NULLS ON. Табличные переменные пусты до INSERT; других строк нет. DECLARE @G TABLE(GroupId int PRIMARY KEY); INSERT @G VALUES(1),(2); DECLARE @E TABLE(Id int PRIMARY KEY,GroupId int,Seq int); INSERT @E VALUES(10,1,2),(11,1,3); Верни GroupId,Id для каждой строки @G. Выбирай по Seq DESC,Id DESC. Если кандидатов нет, сохрани группу с Id=NULL. Вывод по GroupId.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
SELECT g.GroupId,e.Id FROM @G AS g OUTER APPLY (SELECT TOP(1) x.Id FROM @E AS x WHERE x.GroupId=g.GroupId ORDER BY x.Seq DESC,x.Id DESC) AS e ORDER BY g.GroupId;
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
SELECT g.GroupId,e.Id FROM @G AS g CROSS APPLY (SELECT TOP(1) x.Id FROM @E AS x WHERE x.GroupId=g.GroupId ORDER BY x.Seq DESC,x.Id DESC) AS e ORDER BY g.GroupId;
~~~~

**Почему есть предпочтение:** CROSS APPLY удаляет группу без кандидата.

Различающий дефект: `empty_group_lost`. Семья: `v7.ref.sql.keep-empty-group`.

### 7. CODE-SQL-RANK-TIE.REF.07 — base

**Задача ученику:**

~~~~text
Авторская симуляция T-SQL / SQL Server 2019; compatibility level150; ANSI_NULLS ON. Табличные переменные пусты до INSERT; других строк нет. DECLARE @R TABLE(Id int PRIMARY KEY,Doc int,Rev int,Label nvarchar(20)); INSERT @R VALUES(1,1,1,N'z'),(2,1,2,N'a'); Для сравнения Label задано z>a. Напиши SELECT Doc,Label из строки максимального Rev, при равенстве — Id DESC. Вывод по Doc.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
;WITH X AS (SELECT Doc,Label,ROW_NUMBER() OVER(PARTITION BY Doc ORDER BY Rev DESC,Id DESC) AS rn FROM @R) SELECT Doc,Label FROM X WHERE rn=1 ORDER BY Doc;
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
SELECT Doc,MAX(Label) AS Label FROM @R GROUP BY Doc ORDER BY Doc;
~~~~

**Почему есть предпочтение:** MAX(Label) не обязан принадлежать строке максимального Rev.

Различающий дефект: `payload_taken_from_other_row`. Семья: `sim2.code-sql-03.f0006`.

### 8. CODE-SQL-RANK-TIE.REF.08 — base

**Задача ученику:**

~~~~text
Авторская симуляция T-SQL / SQL Server 2019; compatibility level150; ANSI_NULLS ON. Табличные переменные пусты до INSERT; других строк нет. DECLARE @O TABLE(Id int PRIMARY KEY,GroupId int,Price int NULL); INSERT @O VALUES(1,1,NULL),(2,1,7),(3,1,4),(4,2,NULL); Выбери одну строку каждой GroupId: наименьшая Price среди значений, отличных от NULL, затем Id ASC. NULL идёт после любого ненулевого в смысле SQL значения; если все Price равны NULL, нужен минимальный Id. Верни Id,GroupId по GroupId. Значение 0, если встретится, является обычной допустимой ценой.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
;WITH R AS (SELECT Id,GroupId,ROW_NUMBER() OVER(PARTITION BY GroupId ORDER BY CASE WHEN Price IS NULL THEN 1 ELSE 0 END,Price ASC,Id ASC) AS rn FROM @O) SELECT Id,GroupId FROM R WHERE rn=1 ORDER BY GroupId;
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
;WITH R AS (SELECT Id,GroupId,ROW_NUMBER() OVER(PARTITION BY GroupId ORDER BY Price ASC,Id ASC) AS rn FROM @O) SELECT Id,GroupId FROM R WHERE rn=1 ORDER BY GroupId;
~~~~

**Почему есть предпочтение:** Обычный ASC в T-SQL ставит NULL раньше значений; контракт требует обратное.

Различающий дефект: `null_order_policy_ignored`. Семья: `v7.ref.sql.null-last-winner`.

## Последующий учебный опыт — отдельно от создания

После отдельно назначенной приёмки использовать совместимый сохранённый baseline и [план обучения/повторного использования](../../05-LEARNING-AND-CHECKPOINTS.md). Нужны реальные обновления обучаемого механизма, достаточный набор, ненулевой осмысленный результат до/после и полный отчёт дельт. Любое измеренное полезное улучшение признаётся по своему показателю и домену; точность не обязана расти, ухудшения показываются рядом. JSON, metadata, структура каталогов и 0→0 не являются измеренным успехом обучения. Сейчас такой опыт не запускается.

## Скилы и отчёт

Применённые проектные скилы: human-readable-architecture-audit-SKILL.md, phase-planning-SKILL.md, plan-review-SKILL.md, code-examination-SKILL.md, prompt-patch-authoring-SKILL.md и human-communication-GLOSSARY.md. [Точные ссылки](../../06-SOURCES-AND-SKILLS.md). Их production-checksum, CI и review требования не задерживают CREATE_ONLY: текущее прямое поручение выше. Отчёт кратко по-русски: реальное число строк/семей, тема, файл, наблюдаемая модель или unknown, структурный статус, следующий ID.

<!-- GENERATED-QUEUE-START -->
## Переходы единой очереди

Этот блок воспроизводится из общей очереди и задаёт текущий запуск. Прежние команды/номера в основном тексте сохраняются как история. Уже начатую попытку продолжать по её фактическому request/state, не запускать повторно. Прямой ID выше порядка. При NEXT немедленно перейти к названному ID; запрещено продолжать прежнюю задачу или дописывать её отчёт. При STOP немедленно прекратить действия. Живой процесс ждать через его реальный handle до terminal; не завершать раунд вместо ожидания.

Канонический обзор — [EXECUTION-QUEUE.md](../../EXECUTION-QUEUE.md); [бюджет](../../curriculum/scale-plan.json) делится внутри родительского пула, пилот входит в него. Выбранная модель не переопределяется. Checksum, фиксированный профиль модели и production CI не являются условиями подготовки. Режим CREATE_ONLY: запись draft.jsonl → finish → следующий поток; никаких review/oracle/accept/export/обучения.

| Поток | Общий порядок | Следующий поток |
|---|---:|---|
| `CODE-SQL-RANK-TIE/base` | 163 | `DOC-NG-CONFIG-READY/base` |

## Точные команды действующих потоков

Подготовка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Команды работают из любого каталога. RAG уже внутри пакета. Выполнить только назначенный поток; остальные строки — точные альтернативы назначения.

### CODE-SQL-RANK-TIE/base

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "CODE-SQL-RANK-TIE/base" -Run "S704" -Count 10 -Split train -Start 0
```

DEV/EXAM не запускать попутно с TRAIN. Когда назначено отдельное создание этих split в изолированном контексте, точные команды сохранены в `curriculum/pilot-queue.json` как `prepare_first_dev` / `prepare_first_exam`.

Первая TRAIN-папка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/CODE-SQL-RANK-TIE/base/S704`. Следующий поток: `DOC-NG-CONFIG-READY/base`. Доля TRAIN: 1000 пар; первая партия 10. Пилот TRAIN/DEV/EXAM: 100/20/20; набор `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/datasets/CODE-SQL-RANK-TIE-base-v1`; B1 `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/baselines/code.sql/base/CODE-SQL-RANK-TIE-v1/B1`. Это назначенные пути, не существующие веса.

<!-- GENERATED-QUEUE-END -->
