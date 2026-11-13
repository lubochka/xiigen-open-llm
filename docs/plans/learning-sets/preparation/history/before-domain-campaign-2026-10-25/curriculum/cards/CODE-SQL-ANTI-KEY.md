# CODE-SQL-ANTI-KEY — Построить антисоединение с точной политикой ключа и NULL

<!-- CREATE-ONLY-START -->
> **Действующий режим: CREATE_ONLY — только создание.** Прочитай назначение, создай реальные пары в `draft.jsonl`, выполни один структурный `finish`, перейди к следующему stream ID из общей очереди. Не запускай blind/review/oracle/accept/export/обучение и не жди проверяющего. `DRAFT_CREATED` означает запись кандидатов, а не проверенное качество.
>
> Корень наборов: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets`. Подготовка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Точные команды ниже работают из любого текущего каталога; готовый request содержит абсолютные пути. Не искать инструменты в `llm_mvp_core`, не создавать пустой RAG и не восстанавливать содержимое по checksum.
>
> Модель — текущая выбранная в Kiro, без override; не более одного помощника одновременно. При NEXT/новом ID немедленно перейти к нему, не дописывать прошлую задачу; STOP прекращает действия. Живой процесс ждать по его handle штатным wait/resume; timeout транспорта не доказывает отказ. [Точный контракт](../../20-CREATE-ONLY-AND-PATHS.md). Старые правила приёмки ниже описывают только будущую стадию.
> Роль человека при выборе материала выполняет CODEX с Astra 6 ultracode; оценку качества DPO также выполняет CODEX с Astra 6 ultracode. Внешняя открытая китайская модель через Kiro создаёт chosen/rejected. В CREATE_ONLY назначение уже задано карточкой/request: новый вызов выбора и оценка сейчас не запускаются и не задерживают создание следующей партии. Это назначение ролей, не подтверждение выполненного вызова.
<!-- CREATE-ONLY-END -->

## Назначение

Выбрать отсутствие совпадения по полному ключу и видимой политике NULL, сохраняя кратность левой стороны.

Родитель: `CODE-SQL-04`; домен `code.sql`, профиль `base`. T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON.

## Источники и граница доказательства

[RAG-027](../../rag/sources/RAG-027.md); [RAG-041](../../rag/sources/RAG-041.md).

Источники подтверждают методику построения пар и предметную область. Программы и исходные данные ниже — новые авторские симуляции. Ожидаемые ответы не называются результатами runtime. Reference-примеры доступны публично; они и производные не являются закрытым EXAM. Семейные связи сохранены, включая обратные варианты старых SQL-сцен.

## Подробный промпт генератору

Задай полные объявления A/B, данные, уникальность ключей или её отсутствие, список выходных столбцов, порядок и то, считается ли NULL равным NULL. Генерируй один SELECT без изменения данных. Контраст — ровно один дефект: неполный tenant-key; NOT IN с NULL; EXCEPT, удаляющий дубликаты; фильтрация только запрещённых активных строк; неверный nullable marker LEFT JOIN. Для каждого дефекта дай различающую строку, чтобы оба запроса не оказались эквивалентны на fixture. Не объявляй NOT IN всегда неправильным: при показанном NOT NULL он может быть правильным. Не добавляй транзакции, индексы, NOLOCK, неизвестные collation и измерения производительности.

Работай по одному request.json: один topic, objective, domain, profile=base и split. Создавай новые содержательные ситуации, а не переименования публичных примеров. Одна пара различается одним главным предметным дефектом; не делай rejected синтаксически сломанным, бессмысленным или вторым правильным ответом. Полный код, входы, версии, контракт зависимостей и формат ответа доступны в prompt: ученик не получает rationale и metadata. Сначала сформулируй ожидаемое поведение по условию, затем полный выбранный ответ и близкий ошибочный ответ.
Источники — только назначенные файлы xiigen-planning main/llm_plan3. Они подтверждают методику и тематику; новые мини-программы — авторские симуляции, не извлечённые результаты runtime. Все примеры этого дополнения имеют split=reference, training_allowed=false, oracle=NOT_RUN. Их нельзя переименовать в Chinese-generated TRAIN или скрытый EXAM. Родственные DOC/CODE/переводные версии, переименования и производные имеют один origin/leakage group и split. Новые family IDs не доказывают независимость содержания.
Роль человека при выборе материала выполняет CODEX с Astra 6 ultracode; оценку качества DPO также выполняет CODEX с Astra 6 ultracode. Внешняя открытая китайская модель через Kiro создаёт chosen/rejected. В CREATE_ONLY назначение уже задано карточкой/request: новый вызов выбора и оценка сейчас не запускаются и не задерживают создание следующей партии. Это назначение ролей, не подтверждение выполненного вызова.
Прочитай уже собранный prompt/request по абсолютным путям; не ищи другую схему в llm_mvp_core. Запиши реальные строки в request.paths.draft, выполни одну готовую команду finish и перейди к next_stream_id из полной execution-queue.json. PREPARED без draft не является созданием. Старые данные не удалять; существующий run продолжать через resume. При NEXT/новом ID сразу перейти к нему, не продолжать прежнюю задачу или её отчёт. STOP немедленно прекращает действия. Если есть живой handle, ждать штатным wait/resume до terminal в том же ходе: не запускать дубликаты, не опрашивать часто и не завершать ход словами «жду». Отчёт по-русски: тема, фактические строки/семьи, файл, модель, структурный статус и следующий ID.

## Матрица генерации

- смысл отсутствия: равенство SQL с NULL, NULL-safe равенство, полный составной ключ, только активное совпадение.
- кратность: сохранить дубликаты слева, ключи слева уникальны.
- форма: NOT EXISTS, NOT IN при явном NOT NULL, LEFT JOIN с ненулевым marker.

Форма ответа: Полное объявление запрошенной функции/запроса, без Markdown и объяснений; типы и окружение заданы в prompt.

## Полные предметные примеры

### 1. anti_result_deduplicated

**Задача ученику**

```text
Симуляция T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON. DECLARE @A TABLE(k int NOT NULL); DECLARE @B TABLE(k int NOT NULL); INSERT INTO @A VALUES(3),(4),(4); INSERT INTO @B VALUES(3); Напиши SELECT с NOT IN: все строки A, чей k отсутствует в B; дубликаты сохранить, ORDER BY k. Таблицы не менять.
```

**Chosen**

```text
SELECT a.k FROM @A AS a WHERE a.k NOT IN (SELECT b.k FROM @B AS b) ORDER BY a.k;
```

**Rejected**

```text
SELECT DISTINCT a.k FROM @A AS a WHERE a.k NOT IN (SELECT b.k FROM @B AS b) ORDER BY a.k;
```

Различие: NOT IN здесь допустим; единственный дефект rejected — потеря кратности.

Будущий различающий контроль: Проверить две строки 4; отличать мультимножество от множества. Статус: **NOT_RUN**.

Пара `CODE-SQL-ANTI-KEY.REF.01`, семейство `sim2.code-sql-04.f0004`; split `reference`, training_allowed `false`.

### 2. nullable_not_in_poison

**Задача ученику**

```text
Симуляция T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON. DECLARE @A TABLE(k int NOT NULL); DECLARE @B TABLE(k int NULL); INSERT INTO @A VALUES(7),(8); INSERT INTO @B VALUES(7),(NULL); Верни a.k без равного НЕ-NULL b.k, ORDER BY a.k. Только SELECT.
```

**Chosen**

```text
SELECT a.k FROM @A AS a WHERE NOT EXISTS (SELECT 1 FROM @B AS b WHERE b.k=a.k) ORDER BY a.k;
```

**Rejected**

```text
SELECT a.k FROM @A AS a WHERE a.k NOT IN (SELECT b.k FROM @B AS b) ORDER BY a.k;
```

Различие: NULL в подзапросе делает отсутствие 8 не истинным для NOT IN.

Будущий различающий контроль: Проверить наличие 8 при NULL в B; обе альтернативы на том же fixture. Статус: **NOT_RUN**.

Пара `CODE-SQL-ANTI-KEY.REF.02`, семейство `sim2.code-sql-04.f0001`; split `reference`, training_allowed `false`.

### 3. anti_missing_tenant

**Задача ученику**

```text
Симуляция T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON. DECLARE @A TABLE(tenant int NOT NULL,k int NOT NULL); DECLARE @B TABLE(tenant int NOT NULL,k int NOT NULL); INSERT INTO @A VALUES(1,5),(2,5),(2,6); INSERT INTO @B VALUES(1,5),(2,6); Верни строки A без B с теми же tenant И k, ORDER BY tenant,k. Только SELECT.
```

**Chosen**

```text
SELECT a.tenant,a.k FROM @A AS a WHERE NOT EXISTS (SELECT 1 FROM @B AS b WHERE b.tenant=a.tenant AND b.k=a.k) ORDER BY a.tenant,a.k;
```

**Rejected**

```text
SELECT a.tenant,a.k FROM @A AS a WHERE NOT EXISTS (SELECT 1 FROM @B AS b WHERE b.k=a.k) ORDER BY a.tenant,a.k;
```

Различие: Rejected исключает (2,5) по чужому tenant.

Будущий различающий контроль: Проверить пару (2,5), совпадающую по local k, но не по полному ключу. Статус: **NOT_RUN**.

Пара `CODE-SQL-ANTI-KEY.REF.03`, семейство `sim2.code-sql-04.f0002`; split `reference`, training_allowed `false`.

### 4. unrequested_null_safe_matching

**Задача ученику**

```text
Симуляция T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON. DECLARE @A TABLE(k int NULL); DECLARE @B TABLE(k int NULL); INSERT INTO @A VALUES(NULL),(1),(2); INSERT INTO @B VALUES(NULL),(1); По контракту совпадение существует только когда SQL-предикат b.k=a.k истинен: два NULL НЕ считаются совпадением. Верни все A без совпадения, ORDER BY a.k. Только SELECT.
```

**Chosen**

```text
SELECT a.k FROM @A AS a WHERE NOT EXISTS (SELECT 1 FROM @B AS b WHERE b.k=a.k) ORDER BY a.k;
```

**Rejected**

```text
SELECT a.k FROM @A AS a WHERE NOT EXISTS (SELECT 1 FROM @B AS b WHERE b.k=a.k OR (b.k IS NULL AND a.k IS NULL)) ORDER BY a.k;
```

Различие: Обратный контроль: NULL-safe условие здесь противоречит заданной семантике.

Будущий различающий контроль: Проверить, что левый NULL сохраняется; NULL-safe не универсальное улучшение. Статус: **NOT_RUN**.

Пара `CODE-SQL-ANTI-KEY.REF.04`, семейство `sim2.code-sql-04.f0003`; split `reference`, training_allowed `false`.

### 5. inactive_row_excludes_key

**Задача ученику**

```text
Симуляция T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON. DECLARE @A TABLE(k int NOT NULL); DECLARE @B TABLE(k int NOT NULL,active bit NOT NULL); INSERT INTO @A VALUES(1),(2),(3); INSERT INTO @B VALUES(1,0),(2,1),(3,0),(3,1); Верни k из A без АКТИВНОГО совпадения B, ORDER BY k. Только SELECT.
```

**Chosen**

```text
SELECT a.k FROM @A AS a WHERE NOT EXISTS (SELECT 1 FROM @B AS b WHERE b.k=a.k AND b.active=1) ORDER BY a.k;
```

**Rejected**

```text
SELECT a.k FROM @A AS a WHERE NOT EXISTS (SELECT 1 FROM @B AS b WHERE b.k=a.k) ORDER BY a.k;
```

Различие: Rejected запрещает и неактивные совпадения.

Будущий различающий контроль: Проверить 1 и исключение 3 с одновременно активной/неактивной строками. Статус: **NOT_RUN**.

Пара `CODE-SQL-ANTI-KEY.REF.05`, семейство `sim2.code-sql-04.f0005`; split `reference`, training_allowed `false`.

### 6. nullable_payload_as_absence_marker

**Задача ученику**

```text
Симуляция T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON. DECLARE @A TABLE(k int NOT NULL); DECLARE @B TABLE(id int NOT NULL,k int NOT NULL,note nvarchar(10) NULL); INSERT INTO @A VALUES(10),(20); INSERT INTO @B VALUES(1,10,NULL); Используй LEFT JOIN, чтобы вернуть a.k без равного b.k, ORDER BY a.k. Только SELECT.
```

**Chosen**

```text
SELECT a.k FROM @A AS a LEFT JOIN @B AS b ON b.k=a.k WHERE b.id IS NULL ORDER BY a.k;
```

**Rejected**

```text
SELECT a.k FROM @A AS a LEFT JOIN @B AS b ON b.k=a.k WHERE b.note IS NULL ORDER BY a.k;
```

Различие: NULL note есть у существующего совпадения; id гарантированно ненулевой.

Будущий различающий контроль: Проверить отсутствие ложной строки 10 и сохранение 20. Статус: **NOT_RUN**.

Пара `CODE-SQL-ANTI-KEY.REF.06`, семейство `sim2.code-sql-04.f0006`; split `reference`, training_allowed `false`.

## Объём, сохранение и последующее обучение

Первая партия — 10 кандидатных пар. План учебного пилота 100 TRAIN / 20 DEV / 20 EXAM; это назначение будущего объёма, не выполненный эксперимент. TRAIN-пилот входит в общий родительский пул 1000, а не прибавляется к нему. Содержание прежней parent-карточки сохранено; исполняется единая очередь без повторной генерации parent поверх objective.

Все создаваемые наборы: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets`. Подготовка: тот же путь плюс `/preparation`. Существующие attempts не удалять; продолжение по request/state, новые версии под новым run ID. Точные команды, next ID, пути набора и будущего B1 формирует общий блок ниже. Эти пути не доказывают наличие весов.

При отдельно назначенном учебном опыте восстановить один совместимый обученный B1 для обоих плеч, обеспечить достаточный TRAIN и ненулевую предметную компетенцию, измерить обучение до/после и свежую загрузку. 0→0 не является валидным учебным результатом. Любой измеренный полезный выигрыш — успех по этому показателю и домену; рядом показать остальные дельты, регрессии и стоимость. Предметный learner сначала проверяется при верном маршруте, затем prompt→router→домен→ответ отдельно. Создание JSON само по себе не измеряет обучение. Решение о production-интеграции остаётся за пользователем.

<!-- GENERATED-QUEUE-START -->
## Переходы единой очереди

Этот блок воспроизводится из общей очереди и задаёт текущий запуск. Прежние команды/номера в основном тексте сохраняются как история. Уже начатую попытку продолжать по её фактическому request/state, не запускать повторно. Прямой ID выше порядка. При NEXT немедленно перейти к названному ID; запрещено продолжать прежнюю задачу или дописывать её отчёт. При STOP немедленно прекратить действия. Живой процесс ждать через его реальный handle до terminal; не завершать раунд вместо ожидания.

Канонический обзор — [EXECUTION-QUEUE.md](../../EXECUTION-QUEUE.md); [бюджет](../../curriculum/scale-plan.json) делится внутри родительского пула, пилот входит в него. Выбранная модель не переопределяется. Checksum, фиксированный профиль модели и production CI не являются условиями подготовки. Режим CREATE_ONLY: запись draft.jsonl → finish → следующий поток; никаких review/oracle/accept/export/обучения.

| Поток | Общий порядок | Следующий поток |
|---|---:|---|
| `CODE-SQL-ANTI-KEY/base` | 133 | `DOC-NG-ROUTE-BOUNDARY/base` |

## Точные команды действующих потоков

Подготовка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Команды работают из любого каталога. RAG уже внутри пакета. Выполнить только назначенный поток; остальные строки — точные альтернативы назначения.

### CODE-SQL-ANTI-KEY/base

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "CODE-SQL-ANTI-KEY/base" -Run "S604" -Count 10 -Split train -Start 0
```

DEV/EXAM не запускать попутно с TRAIN. Когда назначено отдельное создание этих split в изолированном контексте, точные команды сохранены в `curriculum/pilot-queue.json` как `prepare_first_dev` / `prepare_first_exam`.

Первая TRAIN-папка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/CODE-SQL-ANTI-KEY/base/S604`. Следующий поток: `DOC-NG-ROUTE-BOUNDARY/base`. Доля TRAIN: 1000 пар; первая партия 10. Пилот TRAIN/DEV/EXAM: 100/20/20; набор `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/datasets/CODE-SQL-ANTI-KEY-base-v1`; B1 `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/baselines/code.sql/base/CODE-SQL-ANTI-KEY-v1/B1`. Это назначенные пути, не существующие веса.

<!-- GENERATED-QUEUE-END -->
