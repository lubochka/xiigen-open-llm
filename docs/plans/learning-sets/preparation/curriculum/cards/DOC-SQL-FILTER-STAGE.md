<!-- CURRENT-PARALLEL-CONTRACT-20261026 -->
## Текущий порядок исполнения — 26.10.2026

Эта карточка используется в CREATE_ONLY-кампании по 1000 кандидатных пар на домен; её микротемы распределяют квоту, не добавляют ещё 1000 каждая. Старые профили/примеры ниже сохранены; текущие количества и IDs задаёт [реестр](../../campaign/registry.json).

**Не завершать работу после одной партии:** сохранить реальные строки, выполнить выданный `finish`, затем `next`; продолжать до квоты назначенной группы. Прямой STOP прекращает действия, прямой NEXT немедленно меняет задание. Не дописывать старый отчёт после переключения. Реальный внешний блок назвать точно, не объявлять DONE.

Общий статус: `C:/Projects/llm_mvp_core/learning-sets/campaigns/D1000-20261025/status.json`; состояние-владелец: соседний `state.json`. Перед записью получить назначение с уникальным `agent-id`; работать только со своим owner/token. После сохранения полных строк выполнить команду `progress`; после партии — `finish`. Вручную эти JSON не редактировать. Старое время отметки не разрешает захват чужой работы. Сам файл статуса не синхронизирует разные компьютеры/копии папки.

Точные пути к текущему prompt, commands и material берутся из ответа координатора. Для текущего владельца это `coordination-prompt.md`, `coordination-commands.md`, `coordination-material.required.json`; старый request/draft сохраняется. Новые наборы — `C:/Projects/llm_mvp_core/learning-sets`; инструкции — `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`.

Reason internally in English; do not output private reasoning. Прогресс по-русски, учебный текст по назначенному профилю, текущая модель Kiro без override. Нужные предметные примеры и локальные RAG-фрагменты включены в назначение. Только проектные источники либо конкретно подтверждённые MIT/Apache-2.0; чужую лицензию не выводить из названия репозитория. Не добавлять checksum, independent review, обучение, export/import и production CI к созданию.

Работающий tool: сохранить один handle, пользоваться его документированным wait/resume до terminal. Не перезапускать по пустому stdout, не подменять ожидание быстрым повтором запросов и не завершать раунд вместо доступного ожидания.

| Назначение | Домен / профиль | Пар в этой доле | Следующее в группе |
|---|---|---:|---|
| `A167` — `DOC-SQL-FILTER-STAGE` | `docs.sql` / `base` | 160 | `A168` |


Следующий ID из таблицы — номинальный порядок своей группы. Фактическую свободную карточку с учётом владельцев выдаёт команда `next` текущего request; не менять группу и чужой статус вручную.

Замечания качества: смотри `quality_flags` своего domain/topic/profile в общем status. `needs_rework` указывает доказанный дефект с evidence, `needs_assessment` — область без достаточной оценки. Это не новый review-гейт и не разрешение исправлять чужую старую попытку; finish не закрывает флаг автоматически. [Правила общей матрицы](../../28-PARALLEL-GENERATION-AND-QUALITY.md). Для нового окна используй [один из трёх готовых стартовых промптов](../../00-START-HERE.md); не исполняй исторические команды из нижней справочной части вместо текущих owner-команд.

<!-- /CURRENT-PARALLEL-CONTRACT-20261026 -->

# DOC-SQL-FILTER-STAGE — Описать состав строк до группировки и отбор готовых групп

<!-- DOMAIN-CAMPAIGN-AUTHORITY -->
> **Предметная справка к действующей кампании.** Квоты и продолжение задаёт [campaign/registry.json](../../campaign/registry.json), не старые команды ниже. 1000 пар относятся к домену в сумме. Прочитанный генератором рабочий prompt содержит только назначенную микротему.
>
> Данные: `C:/Projects/llm_mvp_core/learning-sets`. Инструкции: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. CREATE_ONLY: без review, checksum, production CI и обучения. Модель — текущая Kiro без override. Reason internally in English; do not output private reasoning. Отчёт по-русски.
>
> После партии выполнять готовый next до доменной квоты; не останавливаться с вопросом «продолжать?». При прямом NEXT/новом ID немедленно сменить задачу: запрещено дописывать/чинить предыдущую. STOP прекращает действия. Один живой handle ждать документированным wait/resume; не объявлять молчание падением. Старые попытки сохранять; выбранный request возобновлять, не пересоздавать.

Точные доли этой предметной карточки в текущей кампании:
| A-ID | Домен | Профиль | Доля из доменных1000 | Следующий A-ID после доли |
|---|---|---|---:|---|
| A167 | docs.sql | base | 160 | A168 |


Ниже — сохранённая предметная методика и исторические инструкции. [Полный прежний вариант](../../history/before-domain-campaign-2026-10-25/curriculum/cards/DOC-SQL-FILTER-STAGE.md). Старые числовые планы и команды в нём не запускаются вместо актуального request.
<!-- /DOMAIN-CAMPAIGN-AUTHORITY -->

## Назначение

Различать WHERE над исходными строками и HAVING над вычисленными группами, сохраняя фактические агрегаты.

Родитель: `DOC-SQL-05`; домен `docs.sql`, профиль `base`. T-SQL / SQL Server 2019; compatibility level 150; ANSI_NULLS ON.

## Источники и граница доказательства

[RAG-027](../../rag/sources/RAG-027.md); [RAG-040](../../rag/sources/RAG-040.md); [RAG-041](../../rag/sources/RAG-041.md).

Источники подтверждают методику построения пар и предметную область. Программы и исходные данные ниже — новые авторские симуляции. Ожидаемые ответы не называются результатами runtime. Reference-примеры доступны публично; они и производные не являются закрытым EXAM. Семейные связи сохранены, включая обратные варианты старых SQL-сцен.

## Подробный промпт генератору

Покажи полный T-SQL batch с DECLARE TABLE, INSERT, SELECT/GROUP BY/WHERE/HAVING и ORDER BY. Никаких скрытых view или данных БД. Спрашивай результирующие строки и короткое объяснение того, на каком этапе действует заданный фильтр. Rejected меняет один факт: агрегирует отфильтрованную строку, удаляет строку вместо целой группы, включает группу, не прошедшую HAVING, либо применяет SQL UNKNOWN как TRUE. Для контрольных случаев HAVING может быть без GROUP BY, а допустимый групповой фильтр не обязан уменьшать число групп. Не превращай документацию в переписывание SQL. Дубликаты и NULL задавай явно. Равноправные объяснения не объявлять ошибкой только из-за формулировки.

Работай по одному request.json: один topic, objective, domain, profile=base и split. Создавай новые содержательные ситуации, а не переименования публичных примеров. Одна пара различается одним главным предметным дефектом; не делай rejected синтаксически сломанным, бессмысленным или вторым правильным ответом. Полный код, входы, версии, контракт зависимостей и формат ответа доступны в prompt: ученик не получает rationale и metadata. Сначала сформулируй ожидаемое поведение по условию, затем полный выбранный ответ и близкий ошибочный ответ.
Источники — только назначенные файлы xiigen-planning main/llm_plan3. Они подтверждают методику и тематику; новые мини-программы — авторские симуляции, не извлечённые результаты runtime. Все примеры этого дополнения имеют split=reference, training_allowed=false, oracle=NOT_RUN. Их нельзя переименовать в Chinese-generated TRAIN или скрытый EXAM. Родственные DOC/CODE/переводные версии, переименования и производные имеют один origin/leakage group и split. Новые family IDs не доказывают независимость содержания.
Роль человека при выборе материала выполняет CODEX с Astra 6 ultracode; оценку качества DPO также выполняет CODEX с Astra 6 ultracode. Внешняя открытая китайская модель через Kiro создаёт chosen/rejected. В CREATE_ONLY назначение уже задано карточкой/request: новый вызов выбора и оценка сейчас не запускаются и не задерживают создание следующей партии. Это назначение ролей, не подтверждение выполненного вызова.
Прочитай уже собранный prompt/request по абсолютным путям; не ищи другую схему в llm_mvp_core. Запиши реальные строки в request.paths.draft, выполни одну готовую команду finish и перейди к next_stream_id из полной execution-queue.json. PREPARED без draft не является созданием. Старые данные не удалять; существующий run продолжать через resume. При NEXT/новом ID сразу перейти к нему, не продолжать прежнюю задачу или её отчёт. STOP немедленно прекращает действия. Если есть живой handle, ждать штатным wait/resume до terminal в том же ходе: не запускать дубликаты, не опрашивать часто и не завершать ход словами «жду». Отчёт по-русски: тема, фактические строки/семьи, файл, модель, структурный статус и следующий ID.

## Матрица генерации

- этап: WHERE до GROUP BY, HAVING после GROUP BY, HAVING без GROUP BY.
- различающий случай: отфильтрованная строка влияет на SUM, COUNT nullable, UNKNOWN в WHERE, порог равенства.

Форма ответа: Русский текст с точным результатом и одним объяснением по показанному коду. Не переписывать код.

## Полные предметные примеры

### 1. where_applied_after_count

**Задача ученику**

```text
Симуляция T-SQL / SQL Server 2019, compatibility level 150, ANSI_NULLS ON. Выполняется только показанный batch. Назови результирующие строки по указанному ORDER BY и объясни действие фильтра. DECLARE @t TABLE (d char(1), active bit); INSERT INTO @t VALUES ('A',1),('A',1),('A',0),('B',1); SELECT d,COUNT(*) AS n FROM @t WHERE active=1 GROUP BY d HAVING COUNT(*)>=2 ORDER BY d;
```

**Chosen**

```text
Строка: (A,2). WHERE удаляет неактивную строку до COUNT; HAVING затем исключает группу B с одним элементом.
```

**Rejected**

```text
Строка: (A,3). COUNT включает все исходные строки A, после чего HAVING оставляет эту группу.
```

Различие: Rejected включает одну строку, которую WHERE уже исключил.

Будущий различающий контроль: Исполнить batch; сравнить COUNT группы A. Статус: **NOT_RUN**.

Пара `DOC-SQL-FILTER-STAGE.REF.01`, семейство `v6.ref.doc-sql-filter-stage.active-rows-first`; split `reference`, training_allowed `false`.

### 2. having_uses_prefilter_sum

**Задача ученику**

```text
Симуляция T-SQL / SQL Server 2019, compatibility level 150, ANSI_NULLS ON. Выполняется только показанный batch. Назови результирующие строки по указанному ORDER BY и объясни действие фильтра. DECLARE @t TABLE (d char(1), n int); INSERT INTO @t VALUES ('A',6),('A',-5),('B',4); SELECT d,SUM(n) AS s FROM @t WHERE n>0 GROUP BY d HAVING SUM(n)>=5 ORDER BY d;
```

**Chosen**

```text
Строка: (A,6). Отрицательное значение исключено WHERE; HAVING сравнивает сумму оставшихся строк и исключает B.
```

**Rejected**

```text
Строк нет. HAVING проверяет сумму A=1 по всем строкам A, поэтому A не проходит порог 5.
```

Различие: Rejected использует сумму до WHERE вместо фактической суммы группы.

Будущий различающий контроль: Проверить строку A и её сумму после фильтра n>0. Статус: **NOT_RUN**.

Пара `DOC-SQL-FILTER-STAGE.REF.02`, семейство `v6.ref.doc-sql-filter-stage.positive-before-sum`; split `reference`, training_allowed `false`.

### 3. having_count_null_as_row

**Задача ученику**

```text
Симуляция T-SQL / SQL Server 2019, compatibility level 150, ANSI_NULLS ON. Выполняется только показанный batch. Назови результирующие строки по указанному ORDER BY и объясни действие фильтра. DECLARE @t TABLE (d char(1), n int NULL); INSERT INTO @t VALUES ('A',NULL),('A',5),('B',1),('B',2); SELECT d,SUM(n) AS s FROM @t GROUP BY d HAVING COUNT(n)>=2 ORDER BY d;
```

**Chosen**

```text
Строка: (B,3). HAVING COUNT(n)>=2 считает только ненулевые значения n; A содержит лишь одно такое значение.
```

**Rejected**

```text
Строки: (A,5), (B,3). HAVING COUNT(n)>=2 считает обе строки A, включая NULL.
```

Различие: Одна ошибка — COUNT(n) подменён COUNT(*).

Будущий различающий контроль: Проверить исключение группы A с NULL и ненулевой 5. Статус: **NOT_RUN**.

Пара `DOC-SQL-FILTER-STAGE.REF.03`, семейство `v6.ref.doc-sql-filter-stage.count-nonnull`; split `reference`, training_allowed `false`.

### 4. unknown_treated_as_true

**Задача ученику**

```text
Симуляция T-SQL / SQL Server 2019, compatibility level 150, ANSI_NULLS ON. Выполняется только показанный batch. Назови результирующие строки по указанному ORDER BY и объясни действие фильтра. DECLARE @t TABLE (d char(1), state varchar(8) NULL); INSERT INTO @t VALUES ('A','open'),('A',NULL),('A','closed'),('B','open'); SELECT d,COUNT(*) AS n FROM @t WHERE state<>'closed' GROUP BY d HAVING COUNT(*)>=2 ORDER BY d;
```

**Chosen**

```text
Строк нет. WHERE не пропускает NULL, поскольку сравнение даёт UNKNOWN; у A и B остаётся по одной строке.
```

**Rejected**

```text
Строка: (A,2). NULL не равен closed и потому остаётся в WHERE вместе с open.
```

Различие: Rejected пропускает UNKNOWN как истинный предикат.

Будущий различающий контроль: Проверить, что NULL не делает A двухстрочной группой. Статус: **NOT_RUN**.

Пара `DOC-SQL-FILTER-STAGE.REF.04`, семейство `v6.ref.doc-sql-filter-stage.unknown-filter`; split `reference`, training_allowed `false`.

### 5. where_reinterpreted_as_having

**Задача ученику**

```text
Симуляция T-SQL / SQL Server 2019, compatibility level 150, ANSI_NULLS ON. Выполняется только показанный batch. Назови результирующие строки по указанному ORDER BY и объясни действие фильтра. DECLARE @t TABLE (d char(1), n int); INSERT INTO @t VALUES ('A',3),('A',3),('B',7); SELECT d,SUM(n) AS s FROM @t WHERE n>5 GROUP BY d ORDER BY d;
```

**Chosen**

```text
Строка: (B,7). WHERE проверяет каждый n до группировки; обе строки A не проходят, хотя их сумма была бы 6.
```

**Rejected**

```text
Строки: (A,6), (B,7). WHERE n>5 отбирает группы по их сумме.
```

Различие: Rejected заменяет отбор строк отбором групп.

Будущий различающий контроль: Проверить отсутствие группы A, а не только сумму B. Статус: **NOT_RUN**.

Пара `DOC-SQL-FILTER-STAGE.REF.05`, семейство `v6.ref.doc-sql-filter-stage.row-threshold-not-group`; split `reference`, training_allowed `false`.

### 6. having_requires_explicit_group_by

**Задача ученику**

```text
Симуляция T-SQL / SQL Server 2019, compatibility level 150, ANSI_NULLS ON. Выполняется только показанный batch. Назови результирующие строки по указанному ORDER BY и объясни действие фильтра. DECLARE @t TABLE (n int); INSERT INTO @t VALUES (2),(4),(9); SELECT COUNT(*) AS n FROM @t WHERE n<9 HAVING COUNT(*)=2;
```

**Chosen**

```text
Одна строка: n=2. WHERE оставляет два значения, агрегат формирует одну группу, и HAVING пропускает её.
```

**Rejected**

```text
Строк нет. HAVING без GROUP BY не может пропустить агрегированную строку.
```

Различие: Rejected придумывает обязательное условие отсутствующее у запроса.

Будущий различающий контроль: Исполнить агрегат без GROUP BY; сравнить единственную строку. Статус: **NOT_RUN**.

Пара `DOC-SQL-FILTER-STAGE.REF.06`, семейство `v6.ref.doc-sql-filter-stage.single-implicit-group`; split `reference`, training_allowed `false`.

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
| `DOC-SQL-FILTER-STAGE/base` | 132 | `CODE-SQL-ANTI-KEY/base` |

## Точные команды действующих потоков

Подготовка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Команды работают из любого каталога. RAG уже внутри пакета. Выполнить только назначенный поток; остальные строки — точные альтернативы назначения.

### DOC-SQL-FILTER-STAGE/base

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "DOC-SQL-FILTER-STAGE/base" -Run "S603" -Count 10 -Split train -Start 0
```

DEV/EXAM не запускать попутно с TRAIN. Когда назначено отдельное создание этих split в изолированном контексте, точные команды сохранены в `curriculum/pilot-queue.json` как `prepare_first_dev` / `prepare_first_exam`.

Первая TRAIN-папка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/DOC-SQL-FILTER-STAGE/base/S603`. Следующий поток: `CODE-SQL-ANTI-KEY/base`. Доля TRAIN: 1000 пар; первая партия 10. Пилот TRAIN/DEV/EXAM: 100/20/20; набор `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/datasets/DOC-SQL-FILTER-STAGE-base-v1`; B1 `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/baselines/docs.sql/base/DOC-SQL-FILTER-STAGE-v1/B1`. Это назначенные пути, не существующие веса.

<!-- GENERATED-QUEUE-END -->
