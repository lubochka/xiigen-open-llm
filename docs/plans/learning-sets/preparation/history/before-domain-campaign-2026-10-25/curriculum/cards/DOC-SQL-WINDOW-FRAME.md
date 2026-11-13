# DOC-SQL-WINDOW-FRAME — Описать строки, partition и границы оконного агрегата

<!-- CREATE-ONLY-START -->
> **Действующий режим: CREATE_ONLY — только создание.** Прочитай назначение, создай реальные пары в `draft.jsonl`, выполни один структурный `finish`, перейди к следующему stream ID из общей очереди. Не запускай blind/review/oracle/accept/export/обучение и не жди проверяющего. `DRAFT_CREATED` означает запись кандидатов, а не проверенное качество.
>
> Корень наборов: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets`. Подготовка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Точные команды ниже работают из любого текущего каталога; готовый request содержит абсолютные пути. Не искать инструменты в `llm_mvp_core`, не создавать пустой RAG и не восстанавливать содержимое по checksum.
>
> Модель — текущая выбранная в Kiro, без override; не более одного помощника одновременно. При NEXT/новом ID немедленно перейти к нему, не дописывать прошлую задачу; STOP прекращает действия. Живой процесс ждать по его handle штатным wait/resume; timeout транспорта не доказывает отказ. [Точный контракт](../../20-CREATE-ONLY-AND-PATHS.md). Старые правила приёмки ниже описывают только будущую стадию.
> Роль человека при выборе материала выполняет CODEX с Astra 6 ultracode; оценку качества DPO также выполняет CODEX с Astra 6 ultracode. Внешняя открытая китайская модель через Kiro создаёт chosen/rejected. В CREATE_ONLY назначение уже задано карточкой/request: новый вызов выбора и оценка сейчас не запускаются и не задерживают создание следующей партии. Это назначение ролей, не подтверждение выполненного вызова.
<!-- CREATE-ONLY-END -->

## Узкая задача

Вычислить результат конкретного оконного запроса и отличить исходные строки, вход окна после WHERE/GROUP BY и его рамку.

Родитель: `DOC-SQL-04`. Предметные домены: `docs.sql`. Профили: `base`. Один request содержит только назначенный профиль и эту микроцель.

Статус: авторский материал подготовки. Публичные пары ниже — reference; их нельзя копировать в новый корпус или выдавать за скрытый экзамен. Китайская генерация, оценка Astra и Student для нового материала не выполнялись; ожидаемые результаты не являются измерениями.

## Проверенные источники и применимость

- [RAG-027](../../rag/sources/RAG-027.md): закреплённая методика или исходная тема; конкретные новые случаи являются авторскими симуляциями.
- [RAG-040](../../rag/sources/RAG-040.md): закреплённая методика или исходная тема; конкретные новые случаи являются авторскими симуляциями.

[Разобранные good/bad примеры и логика](../../prompts/SOFTWARE-PIPELINE-WINDOW-READY-RU.md). Основное задание и все reference выбранного профиля автоматически входят в batch prompt.

## Подробный промпт исполнителю

Создавай таблицу из 3–6 строк и один T-SQL SELECT. Все типы, NULL, ключи и данные видимы. Для окна явно задай PARTITION BY, ORDER BY и ROWS/RANGE либо явно отсутствие каждого из них. Финальный ORDER BY задаёт печатный порядок отдельно от порядка внутри окна. Сначала определяй строки после WHERE/GROUP BY, затем окно; не учи правилу «окно всегда сохраняет число строк исходной таблицы», если до него была группировка. Чередуй cumulative, bounded frame, peers RANGE, NULL и window over grouped rows. Одна пара меняет один смысловой вывод, а не диалект.

ПРОСТАЯ САМОПРОВЕРКА:
1. Выпиши входные строки окна после WHERE и GROUP BY. Сосчитай их до вычисления сумм.
2. Для каждой выходной строки перечисли ID, попавшие в её partition/frame.
3. ROWS считает позиции; RANGE CURRENT ROW включает peers с одинаковым полным ключом оконного ORDER BY.
4. Для пустой рамки SUM даёт NULL; COUNT(Amount) не считает NULL.
5. Выведи столько строк, сколько осталось на входе окна, затем применяй только явный финальный ORDER BY.

Работай по одному готовому request.json: один topic_id, objective_id, domain_id, profile_id и split. В каждом prompt повтори среду, полный видимый код/данные, точку наблюдения и форму ответа: metadata не попадает в строгую проекцию prompt/chosen/rejected. Создай полный chosen и близкий rejected с одним основным содержательным дефектом, одинакового формата. Не заменяй rejected синтаксической ошибкой, шуткой, заведомо коротким ответом или другой правильной реализацией. Объяснение проверки, source_refs и error_tag записывай вне ответов.
Сначала выпиши маленькую таблицу состояния либо результата по условию, затем сравни с ней обе альтернативы. Это самопроверка автора кандидатов, не независимая оценка качества и не измерение обучения. Если по указанному входу ответы одинаковы, измени различающий вход в пределах задания; не придумывай победу. Смена имени или чисел сама по себе не создаёт независимую семью. Производные, обратные и DOC/CODE-варианты конкретного случая наследуют общий origin/leakage group и split. Публичные reference не являются закрытым EXAM.
Источники — только назначенные файлы xiigen-planning main/llm_plan3. Они дают методику и исходную тематику. Новые фрагменты являются авторскими симуляциями; runtime/oracle сейчас NOT_RUN. Не объявляй новую симуляцию извлечённым production-кодом, измеренным результатом или китайской генерацией.
Роль человека при выборе материала выполняет CODEX с Astra 6 ultracode; оценку качества DPO также выполняет CODEX с Astra 6 ultracode. Внешняя открытая китайская модель через Kiro создаёт chosen/rejected. Сейчас CREATE_ONLY: назначение уже дано карточкой/request. Не запускай новые selection/review/oracle/accept/export/Student/production CI и не задерживай следующую партию ради них. Модель — фактически текущая выбранная в Kiro, без override; неизвестное имя/session запиши unknown. Checksum, подписи, profile receipts и независимый review не нужны для создания.
Корень наборов C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets; подготовка находится в preparation внутри него. Возьми абсолютные входные и выходные пути из request.paths, не ищи runner в llm_mvp_core. После записи реальных JSONL-строк выполни одну готовую команду finish, получи структурный DRAFT_CREATED и перейди к next_stream_id полной execution-queue.json. PREPARED без draft не считается созданием. Старые данные/попытки не удалять; существующий run продолжать через resume.
При прямом NEXT/новом ID немедленно перейти к нему: запрещено продолжать предыдущую задачу, её отчёт или исправления. STOP прекращает действия сразу. Живой процесс ждать штатным wait/resume по единственному handle до terminal в том же ходе; не запускать дубликаты, не делать частый polling и не завершать ход словом «жду». Отчёт по-русски: тема, созданные строки/семьи, точный файл, фактическая модель, структурный статус, следующий ID. Нельзя приписывать вызов Astra текущей модели.

## Форма ответа ученика

Русский ответ с точными строками в заданном порядке и коротким объяснением рамки; не заменять результат советом исправить SQL.

## Что варьировать

```json
[
  {
    "axis": "окно",
    "values": [
      "полный partition",
      "накопление ROWS",
      "скользящая рамка",
      "RANGE peers",
      "пустая рамка"
    ]
  },
  {
    "axis": "вход окна",
    "values": [
      "исходные строки",
      "после WHERE",
      "после GROUP BY",
      "nullable values"
    ]
  }
]
```

## Разобранные пары

### 1. DOC-SQL-WINDOW-FRAME.REF.01 — base

**Задача ученику:**

~~~~text
Авторская симуляция T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON. Табличные переменные пусты до INSERT; других строк нет. DECLARE @T TABLE(Id int PRIMARY KEY,Dept char(1),Amount int); INSERT @T VALUES(1,'A',10),(2,'A',5),(3,'B',7); SELECT Id,SUM(Amount) OVER(PARTITION BY Dept) AS Total FROM @T ORDER BY Id; Укажи все пары (Id,Total).
~~~~

**Chosen — полный правильный ответ:**

~~~~text
(1,15),(2,15),(3,7). Сумма повторяется на каждой строке соответствующего Dept.
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
(1,15),(3,7). Оконная сумма сворачивает каждое Dept до одной строки.
~~~~

**Почему есть предпочтение:** Окно не заменено GROUP BY; потеряна строка Id=2.

Различающий дефект: `window_collapsed_rows`. Семья: `sim2.doc-sql-04.f0006`.

### 2. DOC-SQL-WINDOW-FRAME.REF.02 — base

**Задача ученику:**

~~~~text
Авторская симуляция T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON. Табличные переменные пусты до INSERT; других строк нет. DECLARE @T TABLE(Id int PRIMARY KEY,Amount int); INSERT @T VALUES(1,10),(2,5),(3,7); SELECT Id,SUM(Amount) OVER(ORDER BY Id ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS Total FROM @T ORDER BY Id; Укажи (Id,Total).
~~~~

**Chosen — полный правильный ответ:**

~~~~text
(1,10),(2,15),(3,22). Рамка заканчивается на текущей строке.
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
(1,22),(2,22),(3,22). Каждая строка получает сумму всей таблицы.
~~~~

**Почему есть предпочтение:** В rejected исчезла правая граница CURRENT ROW.

Различающий дефект: `current_row_boundary_ignored`. Семья: `sim2.doc-sql-04.f0001`.

### 3. DOC-SQL-WINDOW-FRAME.REF.03 — base

**Задача ученику:**

~~~~text
Авторская симуляция T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON. Табличные переменные пусты до INSERT; других строк нет. DECLARE @T TABLE(Id int PRIMARY KEY,Score int,Amount int); INSERT @T VALUES(1,1,10),(2,1,5),(3,2,7); SELECT Id,SUM(Amount) OVER(ORDER BY Score RANGE BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS Total FROM @T ORDER BY Id; Укажи (Id,Total).
~~~~

**Chosen — полный правильный ответ:**

~~~~text
(1,15),(2,15),(3,22). Оба Score=1 — peers, поэтому первая рамка уже включает обе строки.
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
(1,10),(2,15),(3,22). Первая рамка включает только физически первую строку Score=1.
~~~~

**Почему есть предпочтение:** RANGE CURRENT ROW ошибочно трактуется как одна позиция ROWS.

Различающий дефект: `range_treated_as_rows`. Семья: `v7.ref.sql.range-peers`.

### 4. DOC-SQL-WINDOW-FRAME.REF.04 — base

**Задача ученику:**

~~~~text
Авторская симуляция T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON. Табличные переменные пусты до INSERT; других строк нет. DECLARE @T TABLE(Id int PRIMARY KEY,Amount int); INSERT @T VALUES(1,2),(2,4),(3,8); SELECT Id,SUM(Amount) OVER(ORDER BY Id ROWS BETWEEN 1 PRECEDING AND CURRENT ROW) AS Total FROM @T ORDER BY Id; Укажи (Id,Total).
~~~~

**Chosen — полный правильный ответ:**

~~~~text
(1,2),(2,6),(3,12). Последняя рамка содержит только строки 2 и 3.
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
(1,2),(2,6),(3,14). Последняя рамка содержит все предшествующие строки.
~~~~

**Почему есть предпочтение:** 1 PRECEDING ошибочно расширено до UNBOUNDED PRECEDING.

Различающий дефект: `bounded_frame_made_unbounded`. Семья: `sim2.doc-sql-04.f0003`.

### 5. DOC-SQL-WINDOW-FRAME.REF.05 — base

**Задача ученику:**

~~~~text
Авторская симуляция T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON. Табличные переменные пусты до INSERT; других строк нет. DECLARE @T TABLE(Id int PRIMARY KEY,Amount int); INSERT @T VALUES(1,10),(2,20),(3,30); SELECT Id,SUM(Amount) OVER(ORDER BY Id ROWS BETWEEN 1 PRECEDING AND 1 PRECEDING) AS PreviousAmount FROM @T ORDER BY Id; Укажи (Id,PreviousAmount).
~~~~

**Chosen — полный правильный ответ:**

~~~~text
(1,NULL),(2,10),(3,20). У первой строки рамка пуста; SUM пустой рамки равен NULL.
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
(1,0),(2,10),(3,20). У первой строки SUM пустой рамки автоматически заменяется нулём.
~~~~

**Почему есть предпочтение:** В запросе нет COALESCE; NULL подменён нулём.

Различающий дефект: `empty_sum_coalesced_without_code`. Семья: `v7.ref.sql.empty-previous-frame`.

### 6. DOC-SQL-WINDOW-FRAME.REF.06 — base

**Задача ученику:**

~~~~text
Авторская симуляция T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON. Табличные переменные пусты до INSERT; других строк нет. DECLARE @T TABLE(Id int PRIMARY KEY,Active bit,Amount int); INSERT @T VALUES(1,1,10),(2,0,100),(3,1,5); SELECT Id,SUM(Amount) OVER() AS Total FROM @T WHERE Active=1 ORDER BY Id; Укажи (Id,Total).
~~~~

**Chosen — полный правильный ответ:**

~~~~text
(1,15),(3,15). WHERE удаляет Id2 до вычисления окна.
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
(1,115),(3,115). Окно сначала суммирует все строки, затем WHERE скрывает Id2.
~~~~

**Почему есть предпочтение:** Сумма включает исключённую строку.

Различающий дефект: `where_applied_after_window`. Семья: `v7.ref.sql.window-after-filter`.

### 7. DOC-SQL-WINDOW-FRAME.REF.07 — base

**Задача ученику:**

~~~~text
Авторская симуляция T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON. Табличные переменные пусты до INSERT; других строк нет. DECLARE @T TABLE(Id int PRIMARY KEY,Amount int NULL); INSERT @T VALUES(1,10),(2,NULL),(3,5); SELECT Id,COUNT(*) OVER() AS N,COUNT(Amount) OVER() AS NN FROM @T ORDER BY Id; Укажи (Id,N,NN).
~~~~

**Chosen — полный правильный ответ:**

~~~~text
(1,3,2),(2,3,2),(3,3,2). COUNT(*) считает строки; COUNT(Amount) — ненулевые значения.
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
(1,3,3),(2,3,3),(3,3,3). Оба COUNT считают все строки независимо от NULL.
~~~~

**Почему есть предпочтение:** Один дефект: NULL посчитан как значение Amount.

Различающий дефект: `nullable_count_counts_null`. Семья: `v7.ref.sql.window-null-count`.

### 8. DOC-SQL-WINDOW-FRAME.REF.08 — base

**Задача ученику:**

~~~~text
Авторская симуляция T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON. Табличные переменные пусты до INSERT; других строк нет. DECLARE @T TABLE(Id int PRIMARY KEY,Dept char(1),Amount int); INSERT @T VALUES(1,'A',10),(2,'A',5),(3,'B',7); SELECT Dept,SUM(Amount) AS DeptTotal,SUM(SUM(Amount)) OVER() AS AllTotal FROM @T GROUP BY Dept ORDER BY Dept; Укажи (Dept,DeptTotal,AllTotal).
~~~~

**Chosen — полный правильный ответ:**

~~~~text
(A,15,22),(B,7,22). GROUP BY сначала создаёт две группы; окно суммирует два групповых итога.
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
(A,10,22),(A,5,22),(B,7,22). Наличие OVER сохраняет все три исходные строки несмотря на GROUP BY.
~~~~

**Почему есть предпочтение:** Окно ошибочно отменяет уже выполненную группировку.

Различающий дефект: `window_restores_pre_group_rows`. Семья: `sim2.doc-sql-04.f0006`.

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
| `DOC-SQL-WINDOW-FRAME/base` | 162 | `CODE-SQL-RANK-TIE/base` |

## Точные команды действующих потоков

Подготовка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Команды работают из любого каталога. RAG уже внутри пакета. Выполнить только назначенный поток; остальные строки — точные альтернативы назначения.

### DOC-SQL-WINDOW-FRAME/base

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "DOC-SQL-WINDOW-FRAME/base" -Run "S703" -Count 10 -Split train -Start 0
```

DEV/EXAM не запускать попутно с TRAIN. Когда назначено отдельное создание этих split в изолированном контексте, точные команды сохранены в `curriculum/pilot-queue.json` как `prepare_first_dev` / `prepare_first_exam`.

Первая TRAIN-папка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/DOC-SQL-WINDOW-FRAME/base/S703`. Следующий поток: `CODE-SQL-RANK-TIE/base`. Доля TRAIN: 1000 пар; первая партия 10. Пилот TRAIN/DEV/EXAM: 100/20/20; набор `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/datasets/DOC-SQL-WINDOW-FRAME-base-v1`; B1 `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/baselines/docs.sql/base/DOC-SQL-WINDOW-FRAME-v1/B1`. Это назначенные пути, не существующие веса.

<!-- GENERATED-QUEUE-END -->
