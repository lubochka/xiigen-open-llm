# CODE-SQL-02 — Защищённое изменение и точное соединение

<!-- CREATE-ONLY-START -->
> **Действующий режим: CREATE_ONLY — только создание.** Прочитай назначение, создай реальные пары в `draft.jsonl`, выполни один структурный `finish`, перейди к следующему stream ID из общей очереди. Не запускай blind/review/oracle/accept/export/обучение и не жди проверяющего. `DRAFT_CREATED` означает запись кандидатов, а не проверенное качество.
>
> Корень наборов: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets`. Подготовка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Точные команды ниже работают из любого текущего каталога; готовый request содержит абсолютные пути. Не искать инструменты в `llm_mvp_core`, не создавать пустой RAG и не восстанавливать содержимое по checksum.
>
> Модель — текущая выбранная в Kiro, без override; не более одного помощника одновременно. При NEXT/новом ID немедленно перейти к нему, не дописывать прошлую задачу; STOP прекращает действия. Живой процесс ждать по его handle штатным wait/resume; timeout транспорта не доказывает отказ. [Точный контракт](../../20-CREATE-ONLY-AND-PATHS.md). Старые правила приёмки ниже описывают только будущую стадию.
> Роль человека при выборе материала выполняет CODEX с Astra 6 ultracode; оценку качества DPO также выполняет CODEX с Astra 6 ultracode. Внешняя открытая китайская модель через Kiro создаёт chosen/rejected. В CREATE_ONLY назначение уже задано карточкой/request: новый вызов выбора и оценка сейчас не запускаются и не задерживают создание следующей партии. Это назначение ролей, не подтверждение выполненного вызова.
<!-- CREATE-ONLY-END -->

> Новая подготовка: `tools/batches.py` по [действующему контракту](../../17-CURRENT-PREPARATION-CONTRACT.md) и точной команде в конце карточки. RAG уже в llm_plan3. Обязательные checksum, fixed-profile receipts и production gates не применяются. Старые `tools/sets.py` и пути в основном тексте — история прежнего runner; не запускать их поверх нового request. При NEXT/новом ID немедленно сменить задачу, запрещено продолжать прежнюю или её отчёт; при STOP прекратить действия.

> Эта родительская карточка сохранена как справочник. Для текущего запуска её профили уточнены objective-карточками в таблице полной очереди ниже; команды общего родительского примера не выполняются поверх них. Прямой ID пользователя имеет приоритет; ранее начатая работа продолжается по её request/state.

> Текущий режим — CREATE_ONLY: кандидатов создаёт выбранная модель Kiro, затем структурный finish и следующий поток. Независимая оценка сейчас не назначена.

## Назначение

Тема обучения: **code.sql**. Профиль `base`. Одна узкая задача: Не разрешать отрицательный остаток; не терять строки без совпадения. Стек этого пилота: **T-SQL / SQL Server 2019**. Версии не объявляются самыми новыми и не смешиваются с другими в одном пакете. Для другого стека создаётся новый профиль и отдельный pilot.

Получатель — предметная обучаемая Unit/Student данной темы, назначаемая через binding из [05](../../05-LEARNING-AND-CHECKPOINTS.md); это не утверждение о существовании одноимённого runtime-класса. Урок меняет её обучаемое состояние; приёмка JSON и oracle — подготовка/оценка, не замена урока.

## Источники

RAG: `RAG-041`, `RAG-042`. Точные repo/commit/blob/строки находятся в main `rag/manifest.json`. Примеры ниже — новые самодостаточные симуляции на основе этих методик и контрастов, не цитаты реального кода. Название старого train-ready не разрешает автоматический импорт. Известные ошибки старых sources отмечены в RAG; например Stock>0 не равно Stock>=qty, а nullable ALTER TABLE не доказывает отсутствие locks.

## Точный запуск подготовки

Действующая команда и точный каталог находятся в разделе «Точные команды действующих потоков» в конце карточки. Подготовка — `tools/batches.py`, RAG встроен в llm_plan3. Каталог содержит request, prompt, commands, state и шаблон отчёта; следующий шаг определяется реальным состоянием попытки. Старые инструкции этой секции сохранены в history/legacy-card-launch-sections.jsonl и не выполняются поверх нового request.

## Предметный промпт генератору

```text
Подготовь только CODE-SQL-02, domain_id=code.sql, profile_id=base.
Обучаемая граница: Не разрешать отрицательный остаток; не терять строки без совпадения.
Используй точные count/family_id/split из request.json. Не помещай другой предмет в этот пакет.
В prompt дай целиком небольшой код/схему/данные и договор об ответе; не заменяй их названием файла. Версию среды и диалект явно повтори в каждом prompt: metadata и текст карточки не попадут в строгую проекцию prompt/chosen/rejected.
Стек: T-SQL / SQL Server 2019. Недостающая версия или внешнее поведение означает unknown, а не разрешение придумать.
Создай chosen, проверив все входы контракта. Создай rejected с одной правдоподобной содержательной ошибкой.
Одинаковый формат ответа; без меток [WRONG], source ID, score и rationale в обучаемом ответе.
Объясни проверяемое отличие только в metadata/rationale. Пересечение текста не является причиной отбраковки.
Оси новых ситуаций: Stock0/qty0/отрицательный qty/qty=Stock/qty>Stock; 0/1/many orders. Полная транзакция отдельным расширением, не подмена одного UPDATE.
Подставленные имена/значения не делают копию независимым семейством. Потомки конкретного doc/code/rephrase seed наследуют общие metadata.origin_family_id и metadata.leakage_group_ids из назначенного family-plan и остаются в одном split. Это связь происхождения конкретных случаев, а не объединение всей предметной концепции в одно семейство.
Не используй внешний web; RAG — проектные выдержки и их ограничения. Симуляции честно пометь synthetic_from_repo.
Отчёт на русском по 08. Не присваивай train-ready без проверки настоящего получателя.
После NEXT перейти к указанному ID, предыдущую задачу не продолжать. При STOP немедленно прекратить новые действия.
Production и старые эксперименты не изменять; старые версии данных не удалять.
```

## Примеры для составления контрастов

### Пример 1

**Условие:** Авторская симуляция. Среда: T-SQL / SQL Server 2019; ANSI_NULLS ON. Все необходимые факты указаны ниже. Напиши один UPDATE Inventory(Id,Stock): для Id=@id уменьшить Stock на @qty только если запаса хватает. Параметр @qty заведомо положителен. Ограничений CHECK нет.

**Chosen:**

```text
UPDATE Inventory SET Stock=Stock-@qty WHERE Id=@id AND Stock>=@qty;
```

**Rejected:**

```text
UPDATE Inventory SET Stock=Stock-@qty WHERE Id=@id AND Stock>0;
```

**Почему отличается:** При Stock=2, qty=3 вторая версия уводит остаток в минус. Тип: `insufficient_quantity_guard`. Объяснение не включается в ответ.

### Пример 2

**Условие:** Авторская симуляция. Среда: T-SQL / SQL Server 2019; ANSI_NULLS ON. Все необходимые факты указаны ниже. Напиши SELECT всех Customers(id), включая без заказов, и соответствующих Orders(id,customer_id).

**Chosen:**

```text
SELECT c.id, o.id AS order_id FROM Customers c LEFT JOIN Orders o ON o.customer_id=c.id;
```

**Rejected:**

```text
SELECT c.id, o.id AS order_id FROM Customers c INNER JOIN Orders o ON o.customer_id=c.id;
```

**Почему отличается:** INNER теряет клиентов без заказов. Тип: `lost_unmatched_rows`. Объяснение не включается в ответ.

### Пример 3

**Условие:** Авторская симуляция. Среда: T-SQL / SQL Server 2019; ANSI_NULLS ON. Все необходимые факты указаны ниже. Нужен список Customers.id по одному разу для клиентов, имеющих хотя бы один Order. Customer.id уникален, DISTINCT не нужен при EXISTS.

**Chosen:**

```text
SELECT c.id FROM Customers c WHERE EXISTS (SELECT 1 FROM Orders o WHERE o.customer_id=c.id);
```

**Rejected:**

```text
SELECT c.id FROM Customers c JOIN Orders o ON o.customer_id=c.id;
```

**Почему отличается:** Вторая версия размножает клиента при нескольких заказах. Тип: `duplicate_customers`. Объяснение не включается в ответ.

### Пример 4

**Условие:** Авторская симуляция. Среда: T-SQL / SQL Server 2019; ANSI_NULLS ON. Все необходимые факты указаны ниже. Напиши условие WHERE для UPDATE Stock=Stock-@qty по Id=@id. Требуется qty>0 и Stock>=qty, значение qty может быть отрицательным.

**Chosen:**

```text
WHERE Id=@id AND @qty>0 AND Stock>=@qty
```

**Rejected:**

```text
WHERE Id=@id AND Stock>=@qty
```

**Почему отличается:** Без qty>0 отрицательное количество увеличивает запас. Тип: `negative_quantity`. Объяснение не включается в ответ.

## Невалидные пары и граничные случаи

Не принимать два эквивалентных ответа, отвергать только стиль или недостаточную длину, ссылаться на невидимую зависимость, придумывать версии и использовать ошибочную старую заготовку как истину. Не считать compilation PASS доказательством исполнения контракта. SQL без ORDER BY не требует конкретного порядка строк; NULL и дубликаты обязательны. RxJS teardown не доказывает физический rollback сервера. Для документации callee summaries должны присутствовать, если вывод от них зависит.

Матрица генерации: 40% обычные случаи, 30% границы, 20% правдоподобные почти правильные ответы, 10% недостаток сведений с явным корректным ограничением вывода. Проценты — первоначальная квота после проверки на этой теме; не размешивать разные предметные темы. Семейства отделить до ответа модели; DEV и EXAM не показывать TRAIN-генератору. Публичные reference примеры не являются закрытым EXAM.

## Как проверить обучением позднее

Использовать [05](../../05-LEARNING-AND-CHECKPOINTS.md): совместимое обученное B1; один изменяемый фактор; фактический lesson/update и before/after на новых семействах. Бюджет DEV-калибровки должен обеспечить минимальный ненулевой результат. Для docs — фактическая верность утверждений/побочных эффектов/версии; для code — одинаковые исполняемые контрактные случаи chosen/rejected. Время и память считаются на пути, участвующем в обучении.

Любое измеренное улучшение полезного показателя — успех по этому показателю и домену; рост точности не обязателен, все ухудшения показываются рядом. 0 правильных до и после, один margin/loss, число пар и статическая проверка не являются учебным эффектом. Сейчас: NOT_RUN для обучения, reference-only для этих примеров. Результат реального успешного опыта документируется кандидатом, полная цепочка исправления production не запускается.

Один долгий процесс — один handle; читать реальный каталог tools, использовать штатный wait/resume до terminal, не busy polling и не новый запуск вместо ожидания. Не завершать раунд вместо обработки результата. При NEXT/STOP соблюдать прямую команду, не возобновлять прошлую карточку. Следующая тема выбирается по `index.json`, не устаревшему номеру.
## Связь с каталогом родительских тем

Порядок: **35**. Устойчивый ID: `CODE-SQL-02`. Следующий ID: `CODE-NG-01`. Источник порядка — [index.json](../../index.json); прямое указание пользователя важнее номера. NEXT/новый ID запрещает продолжать прошлую задачу; STOP прекращает новые действия.

<!-- GENERATED-QUEUE-START -->
## Переходы единой очереди

Этот блок воспроизводится из общей очереди и задаёт текущий запуск. Прежние команды/номера в основном тексте сохраняются как история. Уже начатую попытку продолжать по её фактическому request/state, не запускать повторно. Прямой ID выше порядка. При NEXT немедленно перейти к названному ID; запрещено продолжать прежнюю задачу или дописывать её отчёт. При STOP немедленно прекратить действия. Живой процесс ждать через его реальный handle до terminal; не завершать раунд вместо ожидания.

Канонический обзор — [EXECUTION-QUEUE.md](../../EXECUTION-QUEUE.md); [бюджет](../../curriculum/scale-plan.json) делится внутри родительского пула, пилот входит в него. Выбранная модель не переопределяется. Checksum, фиксированный профиль модели и production CI не являются условиями подготовки. Режим CREATE_ONLY: запись draft.jsonl → finish → следующий поток; никаких review/oracle/accept/export/обучения.

| Поток | Общий порядок | Следующий поток |
|---|---:|---|
| `CODE-SQL-LEFT/base` | 92 | `DOC-SQL-GUARD/base` |
| `CODE-SQL-GUARD/base` | 94 | `DOC-NG-LATEST/base` |
| `CODE-SQL-SAVEPOINT/base` | 119 | `TR08-RESTORE-OR-PRESERVE/ru-en` |

## Точные команды действующих потоков

Подготовка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Команды работают из любого каталога. RAG уже внутри пакета. Эта родительская карточка уточнена микроцелями: исполнять их команды ниже, не прибавлять отдельный родительский корпус.

### CODE-SQL-LEFT/base

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "CODE-SQL-LEFT/base" -Run "D306" -Count 10 -Split train -Start 0
```

DEV/EXAM не запускать попутно с TRAIN. Когда назначено отдельное создание этих split в изолированном контексте, точные команды сохранены в `curriculum/pilot-queue.json` как `prepare_first_dev` / `prepare_first_exam`.

Первая TRAIN-папка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/CODE-SQL-LEFT/base/D306`. Следующий поток: `DOC-SQL-GUARD/base`. Доля TRAIN: 334 пар; первая партия 10. Пилот TRAIN/DEV/EXAM: 100/20/20; набор `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/datasets/CODE-SQL-LEFT-base-v1`; B1 `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/baselines/code.sql/base/CODE-SQL-LEFT-v1/B1`. Это назначенные пути, не существующие веса.

### CODE-SQL-GUARD/base

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "CODE-SQL-GUARD/base" -Run "D308" -Count 10 -Split train -Start 0
```

DEV/EXAM не запускать попутно с TRAIN. Когда назначено отдельное создание этих split в изолированном контексте, точные команды сохранены в `curriculum/pilot-queue.json` как `prepare_first_dev` / `prepare_first_exam`.

Первая TRAIN-папка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/CODE-SQL-GUARD/base/D308`. Следующий поток: `DOC-NG-LATEST/base`. Доля TRAIN: 333 пар; первая партия 10. Пилот TRAIN/DEV/EXAM: 100/20/20; набор `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/datasets/CODE-SQL-GUARD-base-v1`; B1 `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/baselines/code.sql/base/CODE-SQL-GUARD-v1/B1`. Это назначенные пути, не существующие веса.

### CODE-SQL-SAVEPOINT/base

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "CODE-SQL-SAVEPOINT/base" -Run "CS401" -Count 12 -Split train -Start 0
```

DEV/EXAM не запускать попутно с TRAIN. Когда назначено отдельное создание этих split в изолированном контексте, точные команды сохранены в `curriculum/pilot-queue.json` как `prepare_first_dev` / `prepare_first_exam`.

Первая TRAIN-папка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/CODE-SQL-SAVEPOINT/base/CS401`. Следующий поток: `TR08-RESTORE-OR-PRESERVE/ru-en`. Доля TRAIN: 333 пар; первая партия 12. Пилот TRAIN/DEV/EXAM: 120/24/24; набор `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/datasets/CODE-SQL-SAVEPOINT-base-v1`; B1 `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/baselines/code.sql/base/CODE-SQL-SAVEPOINT-v1/B1`. Это назначенные пути, не существующие веса.

<!-- GENERATED-QUEUE-END -->
