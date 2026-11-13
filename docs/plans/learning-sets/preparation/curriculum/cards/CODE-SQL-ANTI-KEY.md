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
| `A185` — `CODE-SQL-ANTI-KEY` | `code.sql` / `base` | 160 | `A186` |


Следующий ID из таблицы — номинальный порядок своей группы. Фактическую свободную карточку с учётом владельцев выдаёт команда `next` текущего request; не менять группу и чужой статус вручную.

Замечания качества: смотри `quality_flags` своего domain/topic/profile в общем status. `needs_rework` указывает доказанный дефект с evidence, `needs_assessment` — область без достаточной оценки. Это не новый review-гейт и не разрешение исправлять чужую старую попытку; finish не закрывает флаг автоматически. [Правила общей матрицы](../../28-PARALLEL-GENERATION-AND-QUALITY.md). Для нового окна используй [один из трёх готовых стартовых промптов](../../00-START-HERE.md); не исполняй исторические команды из нижней справочной части вместо текущих owner-команд.

<!-- /CURRENT-PARALLEL-CONTRACT-20261026 -->

# CODE-SQL-ANTI-KEY — Построить антисоединение с точной политикой ключа и NULL

<!-- DOMAIN-CAMPAIGN-AUTHORITY -->
> **Предметная справка к действующей кампании.** Квоты и продолжение задаёт [campaign/registry.json](../../campaign/registry.json), не старые команды ниже. 1000 пар относятся к домену в сумме. Прочитанный генератором рабочий prompt содержит только назначенную микротему.
>
> Данные: `C:/Projects/llm_mvp_core/learning-sets`. Инструкции: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. CREATE_ONLY: без review, checksum, production CI и обучения. Модель — текущая Kiro без override. Reason internally in English; do not output private reasoning. Отчёт по-русски.
>
> После партии выполнять готовый next до доменной квоты; не останавливаться с вопросом «продолжать?». При прямом NEXT/новом ID немедленно сменить задачу: запрещено дописывать/чинить предыдущую. STOP прекращает действия. Один живой handle ждать документированным wait/resume; не объявлять молчание падением. Старые попытки сохранять; выбранный request возобновлять, не пересоздавать.

Точные доли этой предметной карточки в текущей кампании:
| A-ID | Домен | Профиль | Доля из доменных1000 | Следующий A-ID после доли |
|---|---|---|---:|---|
| A185 | code.sql | base | 160 | A186 |


Ниже — сохранённая предметная методика и исторические инструкции. [Полный прежний вариант](../../history/before-domain-campaign-2026-10-25/curriculum/cards/CODE-SQL-ANTI-KEY.md). Старые числовые планы и команды в нём не запускаются вместо актуального request.
<!-- /DOMAIN-CAMPAIGN-AUTHORITY -->

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
