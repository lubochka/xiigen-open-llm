# Оркестрация, документация, код: предметные самопроверки автора DPO

**Дата: 26.10.2026. Режим: CREATE_ONLY. Это открытые образцы и методы самопроверки при создании; не новый review-процесс и не результат обучения.**

В [машиночитаемом приложении](software-quality-lessons-2026-10-26.json) — 77 новых образцов для 20 доменов: по три для 18 существующих программных доменов, 9 для разбиения prompt на доменные части и 14 для коротких предметных заданий.

## Как пользоваться

Читайте только примеры назначенного домена и цели из текущего request. Сначала составьте настоящий prompt ученика, затем полный chosen, затем близкий rejected с одной основной содержательной ошибкой. Выполните короткую проверку на указанном контрпримере. Она не требует production tests, checksum, второго оценщика, импорта или обучения.

Reason internally in English. Do not output private reasoning. Короткие результаты проверок и прогресс пишутся по-русски. Язык prompt/chosen/rejected следует назначению. Примерные языки этого документа не разрешают менять профиль.

Один JSON, правильное число строк и статус DRAFT_CREATED не подтверждают предметную правильность. В частности, исполняемый код может решать не ту задачу; оба синтаксически корректных ответа могут быть правильными, и тогда такой предпочтительной пары нет.

## Исполнение и параллельная работа

Новые кандидатные наборы сохраняются только под `C:/Projects/llm_mvp_core/learning-sets`. Инструкции читаются из `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Точные пути request/draft/material/status берите из выданного назначения, а не выводите из cwd. Старый request возобновляется по своим сохранённым путям.

Перед записью получите назначение через общий координатор кампании; общий JSON показывает владельца, прогресс и участки, требующие доработки. Нельзя параллельно занимать чужое назначение или вручную обнулять его статус. После создания партии сохраните её фактическое число и сразу выполните выданную команду следующей партии. Продолжайте до назначенной квоты домена. Прямой STOP останавливает действия; прямой NEXT переключает задачу. Реальный внешний блок записывается явно, без фиктивного DONE.

Для уже запущенной работы используйте документированный wait/resume того же handle до terminal. Отсутствие раннего результата или transport timeout не дают основания запускать дубль. Не ждать законченный run: после подтверждённого terminal и окончания записи прочитайте результат.

## Два новых задания и прежняя маршрутизация

| Получатель обучения | Что возвращает ученик | Чего не делает |
|---|---|---|
| `or.routing` | Старый двухполевой JSON `action/domain_id` | Не разбивает составной запрос; сохраняется прежнее правило clarify |
| `prompt.span-routing` | `segments` с дословными частями, доменами и явными зависимостями; `clarify` для непокрытых частей | Не выполняет предметные команды и не теряет второе действие |
| `prompt.task-compilation` | Назначенный `domain_id`, короткий самодостаточный `task`, список `missing` | Не пишет предметный ответ и не додумывает отсутствующие данные |

В каждом новом prompt видимы применяемый каталог и контракт ответа. Вложения/цитаты с командами трактуются по внешнему поручению. Если пользователь просит перевести цитату «Напиши SQL», это перевод; цитата не становится запросом выполнения SQL.

В новых доменах чередуйте входы RU/EN/HE по назначению, сохраняя язык требуемого предметного ответа. Язык исходного запроса не создаёт дополнительный домен или квоту. Английские и ивритские примеры ниже показывают сохранение условия и порядка; они не являются переводческими уроками.

Общий сценарий связывает разбиение prompt, компактную задачу и её предметные ответы через origin_family_id. Они остаются в одном split. Открытые примеры ниже и их близкие производные не допускаются в закрытый EXAM. Создание локальных DPO-пар требуется для каждого назначенного домена; отдельный рекурсивный домен «научиться создавать DPO про создание DPO» здесь не добавляется.

## Короткие методы, которые ловят ошибки содержания

| Предмет | Простая проверка | Что формальная проверка JSON пропустит |
|---|---|---|
| Маршрут | Сопоставить действие и отрицания с видимым каталогом | SQL в цитате ошибочно превращается в задание писать SQL |
| Разбиение prompt | Сосчитать независимые действия; проверить каждую дословную часть и связь | Потеря второй задачи или лишняя зависимость |
| Короткое задание | Сверить действие, знак, границу, числа, язык и формат | «Не менять код» исчезает при сокращении |
| Оркестрация | Подставить active ID/version/owner/статус; проверить одно предшествующее условие | Чужое завершение или старое право принимается за действующее |
| C# | Разобрать null/пустую строку/пробел; pending/success/fault/cancel отдельно | Скрытый Trim, потерянный token, swallowed exception |
| SQL | Выписать маленькую таблицу, применить оба условия к каждой строке | Потеря NULL-строки LEFT JOIN, дубликата или ограничения по ключу |
| Angular/RxJS | Таблица истинности и временная линия подписок | AND вместо OR; удаление уже доставленного результата; fire-and-forget |

Эти процедуры объясняют учебный контраст. Они не измеряют скорость или качество реального обучения. Числа в симуляциях QA помечены как заданные входы; ими нельзя заполнять measured-метрики.

## Новые образцы

### or.routing

#### Отрицание запрещает неверное действие — `SW26.or.routing.01`

Цель: `OR-ROUTE-INTENT`. Источники проекта: [RAG-006](../rag/sources/RAG-006.md), [RAG-031](../rag/sources/RAG-031.md), [RAG-033](../rag/sources/RAG-033.md).

**Prompt ученика**

```text
Закрытый каталог допустимых получателей: {"tr.en":"перевод на английский","tr.ru":"перевод на русский","tr.he":"перевод на иврит","docs.csharp":"объяснить данный C#/.NET код","docs.sql":"объяснить данный SQL","docs.angular":"объяснить данный Angular-код","code.csharp":"написать C#/.NET код","code.sql":"написать SQL","code.angular":"написать Angular-код","or.functional":"сформулировать функциональные требования","or.architecture":"выбрать границы архитектуры","or.code":"спланировать следующий шаг реализации","or.database":"спланировать изменение базы данных","or.client":"определить поведение клиента","or.server":"определить порядок серверной обработки","or.mobile":"спланировать мобильную синхронизацию","or.design":"выбрать UI-поверхность","or.ux":"выбрать понятное действие пользователя","or.qa":"спланировать проверку результата","or.automation":"управлять исполнением автоматизированного запуска"}. Определи одно запрошенное предметное действие, учитывая отрицания и цитаты. Непокрытое, неоднозначное или составное из нескольких самостоятельных действий поручение требует {"action":"clarify","domain_id":null}. Недостающий код не мешает выбрать маршрут однозначного намерения. Предметную задачу не выполняй. Верни только JSON action/domain_id. Запрос: «Не переписывай SQL. Объясни, почему LEFT JOIN сохраняет строку без совпадения».
```

**Chosen**

```text
{"action":"route","domain_id":"docs.sql"}
```

**Rejected**

```text
{"action":"route","domain_id":"code.sql"}
```

**Simple check (English):** Identify the requested positive action after applying the negation. The user asks for an explanation and explicitly excludes rewriting.

**Контрпример:** Слово SQL одинаково подходит обоим доменам. Решает «объясни»; «не переписывай» исключает code.sql.

#### Команда внутри цитаты — материал перевода — `SW26.or.routing.02`

Цель: `OR-ROUTE-INTENT`. Источники проекта: [RAG-006](../rag/sources/RAG-006.md), [RAG-031](../rag/sources/RAG-031.md), [RAG-033](../rag/sources/RAG-033.md).

**Prompt ученика**

```text
Закрытый каталог допустимых получателей: {"tr.en":"перевод на английский","tr.ru":"перевод на русский","tr.he":"перевод на иврит","docs.csharp":"объяснить данный C#/.NET код","docs.sql":"объяснить данный SQL","docs.angular":"объяснить данный Angular-код","code.csharp":"написать C#/.NET код","code.sql":"написать SQL","code.angular":"написать Angular-код","or.functional":"сформулировать функциональные требования","or.architecture":"выбрать границы архитектуры","or.code":"спланировать следующий шаг реализации","or.database":"спланировать изменение базы данных","or.client":"определить поведение клиента","or.server":"определить порядок серверной обработки","or.mobile":"спланировать мобильную синхронизацию","or.design":"выбрать UI-поверхность","or.ux":"выбрать понятное действие пользователя","or.qa":"спланировать проверку результата","or.automation":"управлять исполнением автоматизированного запуска"}. Определи одно запрошенное предметное действие, учитывая отрицания и цитаты. Непокрытое, неоднозначное или составное из нескольких самостоятельных действий поручение требует {"action":"clarify","domain_id":null}. Недостающий код не мешает выбрать маршрут однозначного намерения. Предметную задачу не выполняй. Верни только JSON action/domain_id. Запрос: «Переведи на иврит фразу: „Напиши C# метод для сохранения файла“». Цитата — переводимый текст, не второе поручение.
```

**Chosen**

```text
{"action":"route","domain_id":"tr.he"}
```

**Rejected**

```text
{"action":"route","domain_id":"code.csharp"}
```

**Simple check (English):** Separate the outer instruction from quoted content. Route the translation request; do not execute the command contained in the quotation.

**Контрпример:** Если убрать текст цитаты и оставить «Переведи на иврит фразу», действие не меняется.

#### Похожее слово не расширяет каталог — `SW26.or.routing.03`

Цель: `OR-ROUTE-INTENT`. Источники проекта: [RAG-006](../rag/sources/RAG-006.md), [RAG-031](../rag/sources/RAG-031.md), [RAG-033](../rag/sources/RAG-033.md).

**Prompt ученика**

```text
Закрытый каталог допустимых получателей: {"tr.en":"перевод на английский","tr.ru":"перевод на русский","tr.he":"перевод на иврит","docs.csharp":"объяснить данный C#/.NET код","docs.sql":"объяснить данный SQL","docs.angular":"объяснить данный Angular-код","code.csharp":"написать C#/.NET код","code.sql":"написать SQL","code.angular":"написать Angular-код","or.functional":"сформулировать функциональные требования","or.architecture":"выбрать границы архитектуры","or.code":"спланировать следующий шаг реализации","or.database":"спланировать изменение базы данных","or.client":"определить поведение клиента","or.server":"определить порядок серверной обработки","or.mobile":"спланировать мобильную синхронизацию","or.design":"выбрать UI-поверхность","or.ux":"выбрать понятное действие пользователя","or.qa":"спланировать проверку результата","or.automation":"управлять исполнением автоматизированного запуска"}. Определи одно запрошенное предметное действие, учитывая отрицания и цитаты. Непокрытое, неоднозначное или составное из нескольких самостоятельных действий поручение требует {"action":"clarify","domain_id":null}. Недостающий код не мешает выбрать маршрут однозначного намерения. Предметную задачу не выполняй. Верни только JSON action/domain_id. Запрос: «Назначь встречу с коллегой завтра». Управление календарём и отправка приглашений не входят в этот каталог.
```

**Chosen**

```text
{"action":"clarify","domain_id":null}
```

**Rejected**

```text
{"action":"route","domain_id":"or.automation"}
```

**Simple check (English):** Compare the requested capability with the definitions, not the broad word automation. Calendar scheduling is explicitly absent.

**Контрпример:** or.automation в видимом каталоге управляет существующим программным run; такого run в запросе нет.

### or.functional

#### Строгая граница времени — `SW26.or.functional.01`

Цель: `OR-FUNC-ACCEPTANCE`. Источники проекта: [RAG-004](../rag/sources/RAG-004.md), [RAG-005](../rag/sources/RAG-005.md), [RAG-OR-001](../rag/sources/RAG-OR-001.md).

**Prompt ученика**

```text
Симуляция требований. Пользователь может отменить черновик только раньше чем через 30 минут после создания. Ровно в 30:00 отмена запрещена; других ограничений нет. Создание 10:00:00, попытка 10:30:00. Верни один критерий JSON given, when, then.
```

**Chosen**

```text
{"given":"Черновику ровно 30 минут","when":"Пользователь отменяет черновик","then":"Отмена отклонена; черновик сохранён"}
```

**Rejected**

```text
{"given":"Черновику ровно 30 минут","when":"Пользователь отменяет черновик","then":"Отмена выполнена; черновик удалён"}
```

**Simple check (English):** Compute elapsed time and compare with the strict inequality elapsed < 30 minutes. Equality does not satisfy it.

**Контрпример:** 30 < 30 ложно. Нельзя заменять «раньше» на «не позже».

#### Не добавлять отсутствующий побочный эффект — `SW26.or.functional.02`

Цель: `OR-FUNC-ACCEPTANCE`. Источники проекта: [RAG-004](../rag/sources/RAG-004.md), [RAG-005](../rag/sources/RAG-005.md), [RAG-OR-001](../rag/sources/RAG-OR-001.md).

**Prompt ученика**

```text
Симуляция требований. При сохранении того же значения адреса email профиль остаётся прежним и письмо не отправляется. Пользователь повторно сохраняет уже установленный a@example.test. Верни один критерий JSON given, when, then; других функций нет.
```

**Chosen**

```text
{"given":"Email уже равен a@example.test","when":"Сохранён тот же email","then":"Профиль не изменён; письмо не отправлено"}
```

**Rejected**

```text
{"given":"Email уже равен a@example.test","when":"Сохранён тот же email","then":"Профиль не изменён; письмо отправлено"}
```

**Simple check (English):** Compare old and new values before applying the notification rule. They are identical, so the explicit no-email branch applies.

**Контрпример:** Одинаковое значение не является изменением. Успех HTTP сам по себе не разрешает письмо.

#### Два условия соединены AND — `SW26.or.functional.03`

Цель: `OR-FUNC-ACCEPTANCE`. Источники проекта: [RAG-004](../rag/sources/RAG-004.md), [RAG-005](../rag/sources/RAG-005.md), [RAG-OR-001](../rag/sources/RAG-OR-001.md).

**Prompt ученика**

```text
Симуляция требований. Экспорт разрешён только когда роль=editor И состояние=approved. С ролью editor и состоянием draft экспорт запрещён. Верни один критерий JSON given, when, then для этого случая.
```

**Chosen**

```text
{"given":"Роль editor, состояние draft","when":"Запрошен экспорт","then":"Экспорт отклонён; файл не создан"}
```

**Rejected**

```text
{"given":"Роль editor, состояние draft","when":"Запрошен экспорт","then":"Экспорт разрешён; файл создан"}
```

**Simple check (English):** Evaluate both conditions. true AND false is false; one satisfied condition is insufficient.

**Контрпример:** editor=true, approved=false. Проверка только роли теряет второе условие.

### or.architecture

#### Часы могут идти назад — `SW26.or.architecture.01`

Цель: `OR-ARCH-FIFO`. Источники проекта: [RAG-006](../rag/sources/RAG-006.md), [RAG-050](../rag/sources/RAG-050.md).

**Prompt ученика**

```text
Симуляция архитектуры. A поступил раньше B. После A системные часы были переведены назад, поэтому wall-clock timestamp B меньше timestamp A. Требуется FIFO по фактическому поступлению. T сортирует только wall-clock timestamp. S атомарно выдаёт возрастающий sequence при приёме и извлекает минимальный sequence одним consumer. Оба исключают одновременный допуск. Выбери JSON owner: T или S.
```

**Chosen**

```text
{"owner":"S"}
```

**Rejected**

```text
{"owner":"T"}
```

**Simple check (English):** Try the stated backward-clock event. Arrival sequence remains ordered; wall-clock order reverses A and B.

**Контрпример:** A: sequence=1,time=10:00; B: sequence=2,time=09:59. Минимальное время выберет B вопреки FIFO.

#### FIFO только внутри ключа — `SW26.or.architecture.02`

Цель: `OR-ARCH-FIFO`. Источники проекта: [RAG-006](../rag/sources/RAG-006.md), [RAG-050](../rag/sources/RAG-050.md).

**Prompt ученика**

```text
Симуляция архитектуры. Гарантия: FIFO внутри tenant; между tenant порядок не задан. Стоимость надо минимизировать. P создаёт отдельную FIFO-очередь с одним consumer на tenant, стоимость 2. G создаёт глобальную FIFO-очередь с одним consumer, стоимость 7. Оба удовлетворяют всем ограничениям. Выбери JSON owner P или G.
```

**Chosen**

```text
{"owner":"P"}
```

**Rejected**

```text
{"owner":"G"}
```

**Simple check (English):** Apply the ordering scope and the explicit cost criterion. Cross-tenant ordering is not required; P satisfies the requirement at lower cost.

**Контрпример:** A1 перед A2 и B1 перед B2 достаточно; порядок A1 относительно B1 не требуется.

#### Отменённая запись не блокирует очередь — `SW26.or.architecture.03`

Цель: `OR-ARCH-FIFO`. Источники проекта: [RAG-006](../rag/sources/RAG-006.md), [RAG-050](../rag/sources/RAG-050.md).

**Prompt ученика**

```text
Симуляция архитектуры. FIFO требуется только для неотменённых задач. В очереди A(seq=1,cancelled), B(seq=2,ready), C(seq=3,ready). Qskip атомарно пропускает cancelled и допускает минимальный ready sequence. Qall допускает минимальный sequence независимо от cancelled. Верни JSON owner выбранного компонента.
```

**Chosen**

```text
{"owner":"Qskip"}
```

**Rejected**

```text
{"owner":"Qall"}
```

**Simple check (English):** Filter by the explicit eligibility rule before choosing the lowest sequence. The first eligible item is B.

**Контрпример:** Удаляется только кандидат A из допуска. Между B и C порядок 2 < 3 сохраняется.

### or.code

#### Нельзя передать имя контекста вместо условия — `SW26.or.code.01`

Цель: `OR-CODE-PROMPT-CONTEXT`. Источники проекта: [RAG-005](../rag/sources/RAG-005.md), [RAG-006](../rag/sources/RAG-006.md), [RAG-047](../rag/sources/RAG-047.md).

**Prompt ученика**

```text
Нужно передать исполнителю задачу обработки HTTP 409. Он видит только prompt и не открывает файлы. Утверждённое условие: сохранить введённый текст и предложить перечитать версию. Фактический prompt уже содержит «При HTTP 409 предложи перечитать актуальную версию», но ничего не говорит о черновике. next_step add_draft_retention добавляет только правило сохранения введённого текста; dispatch_worker отправляет prompt как есть. Верни JSON next_step.
```

**Chosen**

```text
{"next_step":"add_draft_retention"}
```

**Rejected**

```text
{"next_step":"dispatch_worker"}
```

**Simple check (English):** Compare the actual worker prompt with the approved rule. Recovery is present; draft retention is the one missing decisive field.

**Контрпример:** Добавить нужно ровно сохранение текста, а не заново исследовать проект или переписать весь prompt.

#### Нужна документация, а не переписывание — `SW26.or.code.02`

Цель: `OR-CODE-PROMPT-CONTEXT`. Источники проекта: [RAG-005](../rag/sources/RAG-005.md), [RAG-006](../rag/sources/RAG-006.md), [RAG-047](../rag/sources/RAG-047.md).

**Prompt ученика**

```text
Ученик планирует передачу задания. Пользователь просит только объяснить уже показанный C# метод; менять код запрещено. Worker prompt сейчас: «Перепиши метод с обработкой null». next_step: replace_with_explanation_task или dispatch_worker. Верни JSON next_step; код не писать.
```

**Chosen**

```text
{"next_step":"replace_with_explanation_task"}
```

**Rejected**

```text
{"next_step":"dispatch_worker"}
```

**Simple check (English):** Compare the requested action and the worker action. Explain and rewrite are different; the explicit no-change constraint rules out dispatching the current prompt.

**Контрпример:** Даже корректная реализация не выполняет поручение объяснить существующий метод.

#### Полный контекст не требует бесконечной упаковки — `SW26.or.code.03`

Цель: `OR-CODE-PROMPT-CONTEXT`. Источники проекта: [RAG-005](../rag/sources/RAG-005.md), [RAG-006](../rag/sources/RAG-006.md), [RAG-047](../rag/sources/RAG-047.md).

**Prompt ученика**

```text
Исполнитель получил полный текст задачи: SQL Server 2019; таблица T(v int NULL); вернуть COUNT(v), повторения учитывать, NULL пропускать; ответ один SELECT. Prompt и критерии доступны, неизвестных параметров нет. next_step: dispatch_worker или request_same_context_again. Выбери JSON next_step.
```

**Chosen**

```text
{"next_step":"dispatch_worker"}
```

**Rejected**

```text
{"next_step":"request_same_context_again"}
```

**Simple check (English):** Check that dialect, schema, counting rule, NULL policy and answer shape are already visible. No decisive field is missing.

**Контрпример:** Повторный запрос того же контекста не добавляет входов и не приближает исполнение.

### or.database

#### Устаревший владелец после перевыдачи — `SW26.or.database.01`

Цель: `OR-DB-ATOMIC-CLAIM`. Источники проекта: [RAG-DC-004](../rag/sources/RAG-DC-004.md), [RAG-006](../rag/sources/RAG-006.md), [RAG-OR-001](../rag/sources/RAG-OR-001.md).

**Prompt ученика**

```text
Симуляция БД. Claim K перевыдан W2 с fence=8. W1 сохранил fence=7. Бизнес-запись принимает только текущий fence, иначе должна отказать. W1 пытается записать данные с 7. next_action: reject_stale_writer или apply_old_writer. Верни JSON next_action.
```

**Chosen**

```text
{"next_action":"reject_stale_writer"}
```

**Rejected**

```text
{"next_action":"apply_old_writer"}
```

**Simple check (English):** Compare the supplied fence with the current one: 7 != 8. Former ownership does not authorize this write.

**Контрпример:** Проверка только имени старого run или когда-то выданного lease пропустит устаревшую запись.

#### Тот же ключ с другим содержимым — `SW26.or.database.02`

Цель: `OR-DB-ATOMIC-CLAIM`. Источники проекта: [RAG-DC-004](../rag/sources/RAG-DC-004.md), [RAG-006](../rag/sources/RAG-006.md), [RAG-OR-001](../rag/sources/RAG-OR-001.md).

**Prompt ученика**

```text
Симуляция БД. Для tenant A ключ K завершён с payload="one" и результатом R. Пришёл tenant A, K, payload="two". Контракт: повтор результата разрешён только при совпадении payload; иначе key_conflict без нового эффекта. next_action: report_key_conflict или reuse_result. Верни JSON next_action.
```

**Chosen**

```text
{"next_action":"report_key_conflict"}
```

**Rejected**

```text
{"next_action":"reuse_result"}
```

**Simple check (English):** Compare both key and payload. The key matches but payload does not; the contract explicitly selects key_conflict.

**Контрпример:** Повтор K не доказывает тождество операции, если one ≠ two.

#### Проверка остатка внутри атомарного изменения — `SW26.or.database.03`

Цель: `OR-DB-CAPACITY-ATOMIC`. Источники проекта: [RAG-OR-006](../rag/sources/RAG-OR-006.md), [RAG-OR-001](../rag/sources/RAG-OR-001.md).

**Prompt ученика**

```text
Симуляция БД. Capacity=1, available=1, SUM(confirmed.quantity)=0. Два worker хотят quantity=1. План A атомарно уменьшает available при available>=quantity и в той же транзакции добавляет confirmed запись; при любой ошибке обе записи откатываются. План B: оба читают available=1, затем каждый отдельно без повторной проверки уменьшает available на 1 и добавляет confirmed запись. Требуются available>=0 и available+SUM(confirmed.quantity)=capacity. Верни только JSON plan_id A или B.
```

**Chosen**

```text
{"plan_id":"A"}
```

**Rejected**

```text
{"plan_id":"B"}
```

**Simple check (English):** Under B, after both writes available=-1 and confirmed quantity=2. The sum remains 1 but nonnegative availability fails. Under A only one allocation commits.

**Контрпример:** A: available=0, SUM=1. B: available=-1, SUM=2; отрицательный остаток запрещён.

### or.client

#### Версия результата принадлежит активному запуску — `SW26.or.client.01`

Цель: `OR-CLIENT-RUN-STATE`. Источники проекта: [RAG-OR-009](../rag/sources/RAG-OR-009.md), [RAG-001](../rag/sources/RAG-001.md), [RAG-003](../rag/sources/RAG-003.md), [RAG-048](../rag/sources/RAG-048.md).

**Prompt ученика**

```text
Симуляция клиента. Active run=R2, ui_state=in_progress. Пришёл terminal_success для R1. Контракт: событие неактивного run не меняет экран R2. Верни JSON ui_state (in_progress или result) и active_run.
```

**Chosen**

```text
{"ui_state":"in_progress","active_run":"R2"}
```

**Rejected**

```text
{"ui_state":"result","active_run":"R2"}
```

**Simple check (English):** Compare event.run_id with active_run before using terminal_success. R1 != R2, so keep R2 unchanged.

**Контрпример:** Слово success не заменяет связь результата с запросом.

#### Пустой промежуточный список ещё не итог — `SW26.or.client.02`

Цель: `OR-CLIENT-RUN-STATE`. Источники проекта: [RAG-OR-009](../rag/sources/RAG-OR-009.md), [RAG-001](../rag/sources/RAG-001.md), [RAG-003](../rag/sources/RAG-003.md), [RAG-048](../rag/sources/RAG-048.md).

**Prompt ученика**

```text
Симуляция клиента. Active run=R3. Пришло {run_id:"R3",state:"running",items:[]}. Пустой список разрешён в прогрессе. Empty-result экран допустим только после terminal_success. Верни JSON ui_state (in_progress или empty_result), active_run.
```

**Chosen**

```text
{"ui_state":"in_progress","active_run":"R3"}
```

**Rejected**

```text
{"ui_state":"empty_result","active_run":"R3"}
```

**Simple check (English):** Check terminal status independently of item count. running plus an empty progress list is still in_progress.

**Контрпример:** items=[] встречается и до конца, и после; решает state.

#### Запрос отмены и подтверждение отмены различаются — `SW26.or.client.03`

Цель: `OR-CLIENT-RUN-STATE`. Источники проекта: [RAG-OR-009](../rag/sources/RAG-OR-009.md), [RAG-001](../rag/sources/RAG-001.md), [RAG-003](../rag/sources/RAG-003.md), [RAG-048](../rag/sources/RAG-048.md).

**Prompt ученика**

```text
Симуляция клиента. Active run=R4. Пользователь запросил отмену. Сервер для R4 вернул state=cancel_requested; по контракту операция ещё может завершиться. Экран показывает cancelling до terminal_cancelled. Верни JSON ui_state (cancelling или cancelled), active_run.
```

**Chosen**

```text
{"ui_state":"cancelling","active_run":"R4"}
```

**Rejected**

```text
{"ui_state":"cancelled","active_run":"R4"}
```

**Simple check (English):** Read the exact state: cancel_requested is not terminal_cancelled. Preserve the pending cancellation state.

**Контрпример:** Отправленная команда отмены не является подтверждением её выполнения.

### or.server

#### Незавершённый commit не разрешает публикацию — `SW26.or.server.01`

Цель: `OR-SERVER-OUTBOX`. Источники проекта: [RAG-DC-004](../rag/sources/RAG-DC-004.md), [RAG-006](../rag/sources/RAG-006.md), [RAG-OR-001](../rag/sources/RAG-OR-001.md).

**Prompt ученика**

```text
Симуляция сервера. Транзакция сохраняет документ и outbox, но commitTask ещё pending. Контракт разрешает relay только после подтверждённого commit. next_action: await_commit или publish_outbox. Верни JSON next_action.
```

**Chosen**

```text
{"next_action":"await_commit"}
```

**Rejected**

```text
{"next_action":"publish_outbox"}
```

**Simple check (English):** The persistence operation has started but has not completed. Do not infer durability from a pending Task.

**Контрпример:** Если commit далее откатится, раннее событие будет сообщать о несуществующем документе.

#### Не менять identity при повторной доставке — `SW26.or.server.02`

Цель: `OR-SERVER-OUTBOX`. Источники проекта: [RAG-DC-004](../rag/sources/RAG-DC-004.md), [RAG-006](../rag/sources/RAG-006.md), [RAG-OR-001](../rag/sources/RAG-OR-001.md).

**Prompt ученика**

```text
Симуляция сервера. Документ и outbox event E9 committed. Первая отправка E9 дала transport_timeout; приёмник использует event_id для идемпотентности. Контракт повтора: отправить тот же E9 с тем же payload. next_action: retry_same_event или generate_new_event_id. Верни JSON next_action.
```

**Chosen**

```text
{"next_action":"retry_same_event"}
```

**Rejected**

```text
{"next_action":"generate_new_event_id"}
```

**Simple check (English):** Keep the original event identity across a retry. A transport timeout does not prove non-delivery.

**Контрпример:** Если первая доставка дошла, новый event_id обойдёт дедупликацию и создаст второй эффект.

#### Результат сохранения уже подтверждён — `SW26.or.server.03`

Цель: `OR-SERVER-OUTBOX`. Источники проекта: [RAG-DC-004](../rag/sources/RAG-DC-004.md), [RAG-006](../rag/sources/RAG-006.md), [RAG-OR-001](../rag/sources/RAG-OR-001.md).

**Prompt ученика**

```text
Симуляция сервера. Документ и outbox E10 committed; relay имеет действующий claim; E10 pending. По контракту нет дополнительных зависимостей. next_action: publish_saved_event или wait_for_unrequired_review. Верни JSON next_action.
```

**Chosen**

```text
{"next_action":"publish_saved_event"}
```

**Rejected**

```text
{"next_action":"wait_for_unrequired_review"}
```

**Simple check (English):** Verify that every stated prerequisite is satisfied. Do not invent an additional review gate.

**Контрпример:** Ждать нужно незавершённую указанную зависимость; здесь такой зависимости нет.

### or.mobile

#### ACK относится к другой операции — `SW26.or.mobile.01`

Цель: `OR-MOBILE-ACK`. Источники проекта: [RAG-006](../rag/sources/RAG-006.md), [RAG-017](../rag/sources/RAG-017.md), [RAG-OR-003](../rag/sources/RAG-OR-003.md).

**Prompt ученика**

```text
Симуляция мобильной очереди. Ожидается Q8 с payload=P8. Пришёл проверенный ACK {operation_id:Q7,payload:P7,status:applied}; Q8 на сервере ещё queued. По контракту закрыть pending можно только по совпадающим operation_id,payload и applied. next_action: retain_pending или close_pending. Верни JSON next_action,operation_id ожидаемой операции.
```

**Chosen**

```text
{"next_action":"retain_pending","operation_id":"Q8"}
```

**Rejected**

```text
{"next_action":"close_pending","operation_id":"Q8"}
```

**Simple check (English):** Compare Q7 with Q8 before reading applied. The acknowledgement is valid but belongs to a different operation.

**Контрпример:** Подтверждение одной записи не подтверждает соседнюю запись очереди.

#### Окончательный отказ не является applied — `SW26.or.mobile.02`

Цель: `OR-MOBILE-ACK`. Источники проекта: [RAG-006](../rag/sources/RAG-006.md), [RAG-017](../rag/sources/RAG-017.md), [RAG-OR-003](../rag/sources/RAG-OR-003.md).

**Prompt ученика**

```text
Симуляция мобильной очереди. Ожидается Q9 с payload=P9. Сервер для Q9/P9 вернул terminal_rejected,applied=false. Контракт: сохранить локальную правку как failed, убрать только автоматические повторы этой операции; считать синхронизированной запрещено. next_action mark_failed_keep_draft сохраняет правку и отказ; close_as_applied удаляет pending как применённую. Верни JSON next_action,operation_id.
```

**Chosen**

```text
{"next_action":"mark_failed_keep_draft","operation_id":"Q9"}
```

**Rejected**

```text
{"next_action":"close_as_applied","operation_id":"Q9"}
```

**Simple check (English):** Read terminal_rejected and applied=false. The explicit policy preserves the draft as failed; it does not mark the operation synchronized.

**Контрпример:** Terminal означает конец попытки, а не успешное применение операции.

#### Новая попытка использует прежний ключ — `SW26.or.mobile.03`

Цель: `OR-MOBILE-ACK`. Источники проекта: [RAG-006](../rag/sources/RAG-006.md), [RAG-017](../rag/sources/RAG-017.md), [RAG-OR-003](../rag/sources/RAG-OR-003.md).

**Prompt ученика**

```text
Симуляция mobile sync. Для Q10 с payload=P10 сеть пропала после отправки; итог неизвестен. Сервер гарантирует идемпотентность по operation_id и payload. Контракт восстановления связи: повторить Q10/P10, сохранить pending до matching ACK applied. next_action retry_same сохраняет ID и payload; retry_new создаёт новый ID. Верни JSON next_action,operation_id=Q10.
```

**Chosen**

```text
{"next_action":"retry_same","operation_id":"Q10"}
```

**Rejected**

```text
{"next_action":"retry_new","operation_id":"Q10"}
```

**Simple check (English):** Preserve operation identity under an unknown outcome. Only the same operation_id uses the stated deduplication guarantee.

**Контрпример:** Если Q10 успела примениться, Q11 будет новой операцией и может примениться второй раз.

### or.design

#### Истёкшее делегирование — `SW26.or.design.01`

Цель: `OR-DESIGN-ROLE-PANEL`. Источники проекта: [RAG-OR-009](../rag/sources/RAG-OR-009.md), [RAG-003](../rag/sources/RAG-003.md), [RAG-048](../rag/sources/RAG-048.md), [RAG-049](../rag/sources/RAG-049.md).

**Prompt ученика**

```text
Симуляция дизайна экрана. Grant view действует в интервале [10:00,11:00), tenant совпадает. Сейчас ровно 11:00; другого права нет. Политика: действующий view→read_panel, иначе access_denied_panel. Верни JSON panel.
```

**Chosen**

```text
{"panel":"access_denied_panel"}
```

**Rejected**

```text
{"panel":"read_panel"}
```

**Simple check (English):** Evaluate the half-open interval. The endpoint 11:00 is excluded, so the grant is expired.

**Контрпример:** 10:59:59 входит, 11:00:00 не входит. Нельзя округлять правило по удобству.

#### Owner вне разрешённой области — `SW26.or.design.02`

Цель: `OR-DESIGN-ROLE-PANEL`. Источники проекта: [RAG-OR-009](../rag/sources/RAG-OR-009.md), [RAG-003](../rag/sources/RAG-003.md), [RAG-048](../rag/sources/RAG-048.md), [RAG-049](../rag/sources/RAG-049.md).

**Prompt ученика**

```text
Симуляция дизайна. U владелец объекта O в tenant B, но текущий security context разрешает только tenant A. Политика экрана сначала проверяет tenant; несовпадение даёт access_denied_panel даже владельцу. Верни JSON panel: edit_panel или access_denied_panel.
```

**Chosen**

```text
{"panel":"access_denied_panel"}
```

**Rejected**

```text
{"panel":"edit_panel"}
```

**Simple check (English):** Apply the tenant scope check before the ownership rule, in the order explicitly defined by the prompt.

**Контрпример:** Owner=true не устраняет tenant B != tenant A.

#### Доступен просмотр, изменение запрещено — `SW26.or.design.03`

Цель: `OR-DESIGN-ROLE-PANEL`. Источники проекта: [RAG-OR-009](../rag/sources/RAG-OR-009.md), [RAG-003](../rag/sources/RAG-003.md), [RAG-048](../rag/sources/RAG-048.md), [RAG-049](../rag/sources/RAG-049.md).

**Prompt ученика**

```text
Симуляция дизайна. Объект архивный. Действующее правило: архивный объект доступен разрешённому viewer только в archive_read_panel; edit_panel запрещён любой роли до восстановления. U имеет viewer, tenant совпадает. Верни JSON panel.
```

**Chosen**

```text
{"panel":"archive_read_panel"}
```

**Rejected**

```text
{"panel":"edit_panel"}
```

**Simple check (English):** Combine the permitted action with the object state. The visible contract allows read-only access to this archived object.

**Контрпример:** Наличие права просмотра не превращается в право редактирования архивного объекта.

### or.ux

#### Известная ошибка конкретного поля — `SW26.or.ux.01`

Цель: `OR-UX-RECOVERY`. Источники проекта: [RAG-003](../rag/sources/RAG-003.md), [RAG-048](../rag/sources/RAG-048.md), [RAG-049](../rag/sources/RAG-049.md).

**Prompt ученика**

```text
Симуляция UX. Сервер подтвердил NOT_SAVED: start_date позже end_date. Draft сохранён. Доступные actions: edit_dates, retry_same_values. Верни JSON message и action; сообщение должно называть доступное исправление без потери draft.
```

**Chosen**

```text
{"message":"Дата начала должна быть не позже даты окончания. Исправьте даты; черновик сохранён.","action":"edit_dates"}
```

**Rejected**

```text
{"message":"Дата начала должна быть не позже даты окончания. Исправьте даты; черновик сохранён.","action":"retry_same_values"}
```

**Simple check (English):** Test whether the chosen action changes the offending values. An unchanged retry cannot fix start_date > end_date.

**Контрпример:** Сообщение может быть правильным, но действие под ним обязано соответствовать причине.

#### Не обещать несохранённый черновик — `SW26.or.ux.02`

Цель: `OR-UX-RECOVERY`. Источники проекта: [RAG-003](../rag/sources/RAG-003.md), [RAG-048](../rag/sources/RAG-048.md), [RAG-049](../rag/sources/RAG-049.md).

**Prompt ученика**

```text
Симуляция UX. Серверный результат неизвестен после обрыва связи. Приложение подтверждает draft_saved=false. Доступен check_status; действие в обоих ответах должно быть check_status. Верни JSON message,action, не выдумывая сохранение draft.
```

**Chosen**

```text
{"message":"Ответ сервера не получен. Проверим состояние отправки.","action":"check_status"}
```

**Rejected**

```text
{"message":"Ответ сервера не получен. Черновик сохранён. Проверим состояние отправки.","action":"check_status"}
```

**Simple check (English):** List only confirmed facts. The prompt explicitly states draft_saved=false, so a saved-draft promise is false.

**Контрпример:** Отсутствие результата не даёт права подставить удобный текст «черновик сохранён».

#### Явное подтверждение успеха снимает ожидание — `SW26.or.ux.03`

Цель: `OR-UX-RECOVERY`. Источники проекта: [RAG-003](../rag/sources/RAG-003.md), [RAG-048](../rag/sources/RAG-048.md), [RAG-049](../rag/sources/RAG-049.md).

**Prompt ученика**

```text
Симуляция UX. Для текущего request сервер подтвердил terminal_success,order_id=O15. Повтор создавать запрещено; действие open_order доступно. Верни JSON message и action: open_order или retry_create.
```

**Chosen**

```text
{"message":"Заказ создан. Откройте его для просмотра.","action":"open_order"}
```

**Rejected**

```text
{"message":"Заказ создан. Откройте его для просмотра.","action":"retry_create"}
```

**Simple check (English):** Use the confirmed successful result. Repeating creation would contradict both the outcome and the explicit no-repeat rule.

**Контрпример:** После terminal_success действие должно использовать O15, а не создавать второй заказ.

### or.qa

#### Tokenizer входит в восстановление, если его читает learner — `SW26.or.qa.01`

Цель: `OR-QA-CAUSAL`. Источники проекта: [RAG-002](../rag/sources/RAG-002.md), [RAG-032](../rag/sources/RAG-032.md), [RAG-046](../rag/sources/RAG-046.md).

**Prompt ученика**

```text
Симуляция A/B учебного опыта. Обученный S даёт ненулевой правильный результат и содержит weights, adapter, tokenizer T1, optimizer и RNG. Learner читает все эти компоненты; T2 меняет номера токенов. Изменяемый фактор — выбор negatives. В протоколе A оба плеча восстанавливают полный S. В B второе плечо восстанавливает S, но заменяет T1 на T2. Прочие условия равны. Верни JSON protocol: A или B.
```

**Chosen**

```text
{"protocol":"A"}
```

**Rejected**

```text
{"protocol":"B"}
```

**Simple check (English):** List the consumed components and compare both restored states. Replacing the tokenizer changes a second factor beyond negative selection.

**Контрпример:** Одинаковые weights не компенсируют другую нумерацию токенов. Выбор tokenizer здесь участвует в обучении.

#### Адаптер не теряется при восстановлении — `SW26.or.qa.02`

Цель: `OR-QA-CAUSAL`. Источники проекта: [RAG-002](../rag/sources/RAG-002.md), [RAG-032](../rag/sources/RAG-032.md), [RAG-046](../rag/sources/RAG-046.md).

**Prompt ученика**

```text
Симуляция A/B. Обученный snapshot S содержит base weights, ненулевой LoRA adapter, tokenizer и optimizer; learner использует все четыре. Цель — сравнить ровно одну kNN-функцию. Протокол A: первое плечо восстанавливает всё S, второе только base/tokenizer/optimizer и обнуляет adapter. Протокол B: оба восстанавливают всё S. Данные, бюджет и RNG одинаковы. Верни JSON protocol.
```

**Chosen**

```text
{"protocol":"B"}
```

**Rejected**

```text
{"protocol":"A"}
```

**Simple check (English):** Check whether the already learned adapter is restored in both arms. Zeroing it changes the starting model as well as the kNN factor.

**Контрпример:** Фраза «веса восстановлены» недостаточна: использованный learner adapter должен быть тем же.

#### Обученная память и её индекс составляют одно состояние — `SW26.or.qa.03`

Цель: `OR-QA-CAUSAL`. Источники проекта: [RAG-002](../rag/sources/RAG-002.md), [RAG-032](../rag/sources/RAG-032.md), [RAG-046](../rag/sources/RAG-046.md).

**Prompt ученика**

```text
Симуляция A/B. Learner читает trained memory M и индекс I, построенный в embedding-space V1. S сохраняет M,I,V1 и RNG, результат до ненулевой. Фактор — одна новая функция ранжирования соседей. Протокол A восстанавливает M,I,V1,RNG в обоих плечах. В B второе плечо пересчитывает индекс в V2, а первое сохраняет V1. Остальные условия равны. Верни JSON protocol A или B.
```

**Chosen**

```text
{"protocol":"A"}
```

**Rejected**

```text
{"protocol":"B"}
```

**Simple check (English):** Compare the memory representation before comparing ranking. Rebuilding only one index in V2 changes another consumed state component.

**Контрпример:** Новая embedding-система меняет соседей независимо от изучаемой функции ранжирования.

### or.automation

#### Две ветви из трёх не полный результат — `SW26.or.automation.01`

Цель: `OR-AUTO-FANIN`. Источники проекта: [RAG-001](../rag/sources/RAG-001.md), [RAG-008](../rag/sources/RAG-008.md), [RAG-047](../rag/sources/RAG-047.md).

**Prompt ученика**

```text
Симуляция автоматизации. Группа G1 требует all_required: попытки A1,B1,C1. A1 и B1 terminal_success, C1 running с валидным handle hC1. Действие wait_C1 ждёт тот же C1 документированным wait(hC1); publish собирает результаты и объявляет группу готовой. Publish разрешён только после успеха всех трёх. Верни JSON next_action (wait_C1 или publish),group_id.
```

**Chosen**

```text
{"next_action":"wait_C1","group_id":"G1"}
```

**Rejected**

```text
{"next_action":"publish","group_id":"G1"}
```

**Simple check (English):** Compare required branches {A,B,C} with successful branches {A,B}. C remains an unmet required dependency.

**Контрпример:** 2/3 не равно 3/3. Прогресс не разрешает пометить весь запуск успешным.

#### Есть terminal success и законченный файл — `SW26.or.automation.02`

Цель: `OR-AUTO-TERMINAL`. Источники проекта: [RAG-DC-003](../rag/sources/RAG-DC-003.md), [RAG-001](../rag/sources/RAG-001.md), [RAG-032](../rag/sources/RAG-032.md), [RAG-047](../rag/sources/RAG-047.md).

**Prompt ученика**

```text
Симуляция автоматизации. Для R8 сервер подтвердил terminal_success; result.json для R8 полностью записан и опубликован. Ошибок доступа нет. read_result читает этот файл; wait_same_finished_run снова ждёт уже завершённый R8. Верни JSON action,run_id.
```

**Chosen**

```text
{"action":"read_result","run_id":"R8"}
```

**Rejected**

```text
{"action":"wait_same_finished_run","run_id":"R8"}
```

**Simple check (English):** Read both terminal status and artifact completion. Waiting for an already completed run adds no information; consume its result.

**Контрпример:** Правило уметь ждать не означает ждать после подтверждённого завершения.

#### Событие чужого объекта не открывает окно — `SW26.or.automation.03`

Цель: `OR-AUTO-EVENT-GATE`. Источники проекта: [RAG-OR-006](../rag/sources/RAG-OR-006.md).

**Prompt ученика**

```text
Симуляция event gate. Окно W9 закрыто. Для открытия нужен подтверждённый event type=data_ready для object=O9,tenant=T1. Получен подтверждённый E9: type=data_ready,object=O8,tenant=T1. wait_event сохраняет закрытое окно; open_window открывает W9; keep_open допустим только уже открытому W9. Верни JSON action (wait_event|open_window|keep_open),event_id=E9.
```

**Chosen**

```text
{"action":"wait_event","event_id":"E9"}
```

**Rejected**

```text
{"action":"open_window","event_id":"E9"}
```

**Simple check (English):** Compare window state, event type, object, tenant and confirmation. Object O8 differs from required O9, so the closed window must wait.

**Контрпример:** Совпали type,tenant,confirmation, но не object. Три совпадения из четырёх не открывают gate.

### docs.csharp

#### Пустая строка и null различаются — `SW26.docs.csharp.01`

Цель: `DOC-CS-NULL`. Источники проекта: [RAG-027](../rag/sources/RAG-027.md), [RAG-028](../rag/sources/RAG-028.md).

**Prompt ученика**

```text
Симуляция C# 10/.NET 6, nullable enabled. Метод string Text(string? s) => s ?? "fallback";. Вызовы Text(null), Text(""), Text("0") именно в этом порядке. Кратко по-русски назови точные результаты и правило подстановки. Новый код не писать.
```

**Chosen**

```text
Результаты по порядку: "fallback", "", "0". Подстановка действует только для null.
```

**Rejected**

```text
Результаты по порядку: "fallback", "fallback", "0". Подстановка действует для null и пустой строки.
```

**Simple check (English):** Apply ?? only to null. The empty string is a present value and remains empty.

**Контрпример:** Сравни второй результат отдельно: "" не равен null.

#### Передача токена не доказывает остановку — `SW26.docs.csharp.02`

Цель: `DOC-CS-CANCEL`. Источники проекта: [RAG-027](../rag/sources/RAG-027.md), [RAG-028](../rag/sources/RAG-028.md), [RAG-029](../rag/sources/RAG-029.md).

**Prompt ученика**

```text
Симуляция C# 10/.NET 6. Task<int> Forward(CancellationToken ct, Func<CancellationToken,Task<int>> read) => read(ct);. ct уже отменён. read лишь записывает аргумент и возвращает Task.FromResult(5), не проверяя token и не бросая. Опиши число вызовов, аргумент и результат await Forward; не дописывай проверку отмены.
```

**Chosen**

```text
read вызван один раз с исходным отменённым ct; await Forward возвращает 5.
```

**Rejected**

```text
read вызван один раз с CancellationToken.None; await Forward возвращает 5.
```

**Simple check (English):** Trace the actual argument read(ct), even when ct is already canceled. The delegate ignores cancellation and returns 5 in both alternatives; the rejected description substitutes None.

**Контрпример:** Отменённый ct остаётся исходным аргументом, не становится None. Само наличие токена не останавливает игнорирующий его callee.

#### finally выполняется и при ошибке — `SW26.docs.csharp.03`

Цель: `DOC-CS-FINALLY`. Источники проекта: [RAG-027](../rag/sources/RAG-027.md), [RAG-028](../rag/sources/RAG-028.md), [RAG-029](../rag/sources/RAG-029.md).

**Prompt ученика**

```text
Симуляция C# 10/.NET 6. Полный синхронный фрагмент: var trace=new List<string>(); void Run(){trace.Add("start");try{throw new InvalidOperationException();}finally{trace.Add("end");}} try{Run();}catch(InvalidOperationException){trace.Add("caught");}. Все using доступны; trace.Add не бросает. Кратко по-русски назови trace после внешнего catch. Код не менять.
```

**Chosen**

```text
После внешнего catch trace=["start","end","caught"]. Блок finally выполняется до передачи управления внешнему catch.
```

**Rejected**

```text
После внешнего catch trace=["start","caught"]. Блок finally пропускается при исключении из try.
```

**Simple check (English):** Follow stack unwinding: finally runs before control reaches the outer catch. No asynchronous call or unseen dependency exists.

**Контрпример:** Сначала start, затем finally добавляет end, затем внешний catch добавляет caught.

### docs.sql

#### COUNT(*) и COUNT(v) на одном наборе — `SW26.docs.sql.01`

Цель: `DOC-SQL-COUNT`. Источники проекта: [RAG-040](../rag/sources/RAG-040.md), [RAG-041](../rag/sources/RAG-041.md).

**Prompt ученика**

```text
Симуляция T-SQL/SQL Server 2019. Один batch: DECLARE @T TABLE(v int NULL); INSERT @T VALUES(NULL),(0),(0); SELECT COUNT(*) AS n FROM @T;. По-русски назови число строк результата, n и правило подсчёта.
```

**Chosen**

```text
Одна строка: n=3. COUNT(*) учитывает все строки, включая строку с NULL и оба нуля.
```

**Rejected**

```text
Одна строка: n=2. COUNT(*) пропускает строку с NULL и учитывает оба нуля.
```

**Simple check (English):** Count rows, not non-NULL values: the visible table has three rows. COUNT(*) has no expression whose NULL values could be excluded.

**Контрпример:** [NULL,0,0] — три строки. Правило COUNT(v) нельзя приписывать COUNT(*).

#### LEFT JOIN не уничтожает кратность — `SW26.docs.sql.02`

Цель: `DOC-SQL-LEFT`. Источники проекта: [RAG-040](../rag/sources/RAG-040.md), [RAG-041](../rag/sources/RAG-041.md).

**Prompt ученика**

```text
Симуляция T-SQL/SQL Server 2019, ANSI_NULLS ON. #P(Id)={(1),(2)}, #R(Id,ParentId)={(10,1),(11,1)}; Id уникальны внутри таблиц. SELECT p.Id,r.Id AS RightId FROM #P p LEFT JOIN #R r ON r.ParentId=p.Id ORDER BY p.Id,r.Id;. Назови упорядоченные пары (Id,RightId) и кратко по-русски объясни кратность.
```

**Chosen**

```text
[(1,10),(1,11),(2,null)]. Для родителя 1 есть два совпадения; родитель 2 сохранён без совпадений.
```

**Rejected**

```text
[(1,10),(2,null)]. Для родителя 1 оставлено только первое совпадение; родитель 2 сохранён без совпадений.
```

**Simple check (English):** Enumerate every matching right row for each parent. Parent 1 has two matches; no DISTINCT or aggregation removes either.

**Контрпример:** Для Id=1 нужны две строки, для Id=2 одна строка с null: всего 3.

#### Guard проверяет и ключ, и запас — `SW26.docs.sql.03`

Цель: `DOC-SQL-GUARD`. Источники проекта: [RAG-041](../rag/sources/RAG-041.md), [RAG-042](../rag/sources/RAG-042.md).

**Prompt ученика**

```text
Симуляция T-SQL/SQL Server 2019; нет триггеров, CHECK и конкуренции. #Stock(Id,Available)={(1,1),(2,5)}. @id=1,@qty=1. UPDATE #Stock SET Available=Available-@qty WHERE Id=@id AND Available>=@qty;. Сразу после UPDATE читается @@ROWCOUNT. По-русски назови rowcount и итоговые остатки по Id.
```

**Chosen**

```text
Rowcount=1; остатки: Id=1 → 0, Id=2 → 5.
```

**Rejected**

```text
Rowcount=2; остатки: Id=1 → 0, Id=2 → 4.
```

**Simple check (English):** Evaluate both predicates for each row. Id=2 fails the key predicate even though its stock is sufficient.

**Контрпример:** Остаток соседа 5 не даёт права менять соседа при @id=1.

### docs.angular

#### Отрицание в выражении binding — `SW26.docs.angular.01`

Цель: `DOC-NG-DISABLED`. Источники проекта: [RAG-037](../rag/sources/RAG-037.md), [RAG-038](../rag/sources/RAG-038.md), [RAG-039](../rag/sources/RAG-039.md).

**Prompt ученика**

```text
Симуляция Angular 16.2: native button [disabled]="!canSave". canSave:boolean принимает false, затем true; после каждого присваивания fixture.detectChanges(). Других bindings нет. По-русски назови button.disabled по шагам и кратко объясни формулу.
```

**Chosen**

```text
По шагам: disabled=true, затем disabled=false. Binding вычисляет отрицание canSave.
```

**Rejected**

```text
По шагам: disabled=false, затем disabled=true. Binding сохраняет значение canSave без отрицания.
```

**Simple check (English):** Substitute each input into !canSave. false becomes true; true becomes false.

**Контрпример:** Имя canSave не заменяет оператор !. Проверять нужно полное выражение.

#### Результат до нового запроса не исчезает — `SW26.docs.angular.02`

Цель: `DOC-NG-LATEST`. Источники проекта: [RAG-027](../rag/sources/RAG-027.md), [RAG-037](../rag/sources/RAG-037.md), [RAG-039](../rag/sources/RAG-039.md).

**Prompt ученика**

```text
Симуляция RxJS 7.8 TestScheduler. query$ выдаёт A в frame 0, B в frame 4, completion в frame 10. load(A) — cold Observable: next a через 2 frames, completion через 3; load(B): next b через 1, completion через 2. Ошибок нет. Код query$.pipe(switchMap(q=>load(q))). Верни только массив {frame,value} всех next; completion не включать.
```

**Chosen**

```text
[{"frame":2,"value":"a"},{"frame":5,"value":"b"}]
```

**Rejected**

```text
[{"frame":5,"value":"b"}]
```

**Simple check (English):** Place A emission at 0+2=2 and B arrival at 4. A was already delivered before switching; switchMap does not erase past emissions.

**Контрпример:** Frame 2 < 4: первый ответ уже дошёл до подписчика и остаётся частью истории.

#### Обработчик ошибки меняет только указанные поля — `SW26.docs.angular.03`

Цель: `DOC-NG-CONFIG-READY`. Источники проекта: [RAG-027](../rag/sources/RAG-027.md), [RAG-028](../rag/sources/RAG-028.md), [RAG-037](../rag/sources/RAG-037.md).

**Prompt ученика**

```text
Симуляция Angular 16.2.0/TypeScript 5.1.6, изолированный Promise-fixture. ready=false,error=null,snapshot=null. async function load(){try{snapshot=await fetchConfig();ready=true;}catch{error="load_failed";}}. fetchConfig возвращает Promise, который reject; внешних изменений нет. Выполнено await load(). Кратко по-русски назови ready,error,snapshot и причину состояния. Код не менять.
```

**Chosen**

```text
ready=false, error="load_failed", snapshot=null. Отклонённый await пропускает присваивание ready; catch меняет только error.
```

**Rejected**

```text
ready=true, error="load_failed", snapshot=null. После отклонённого await присваивание ready всё равно выполняется; catch меняет error.
```

**Simple check (English):** Follow the reject branch. ready=true is after the failed await and is skipped; catch only assigns error.

**Контрпример:** Перехваченное исключение делает load завершённым, но не делает ready=true.

### code.csharp

#### Пробельные строки по контракту сохраняются — `SW26.code.csharp.01`

Цель: `CODE-CS-NULL`. Источники проекта: [RAG-027](../rag/sources/RAG-027.md), [RAG-040](../rag/sources/RAG-040.md).

**Prompt ученика**

```text
Симуляция C# 10/.NET 6, nullable enabled. Напиши только метод string Label(string? s): null заменить на "n/a"; любую непустую, пустую и пробельную строку вернуть без Trim и других изменений.
```

**Chosen**

```text
string Label(string? s) => s ?? "n/a";
```

**Rejected**

```text
string Label(string? s) => s?.Trim() ?? "n/a";
```

**Simple check (English):** Use input "  A  " as a witness. It must remain exactly "  A  "; Trim changes required content.

**Контрпример:** Оба метода возвращают строку и компилируются; отличает их сохранение пробелов.

#### Token запроса не заменяется глобальным — `SW26.code.csharp.02`

Цель: `CODE-CS-CANCEL`. Источники проекта: [RAG-027](../rag/sources/RAG-027.md), [RAG-028](../rag/sources/RAG-028.md), [RAG-029](../rag/sources/RAG-029.md).

**Prompt ученика**

```text
Симуляция C# 10/.NET 6. Доступны using System; using System.Threading; using System.Threading.Tasks;. Напиши Task<int> Forward(CancellationToken requestToken,CancellationToken globalToken,Func<CancellationToken,Task<int>> read). requestToken и globalToken — разные активные token. Вызови read ровно один раз с requestToken, верни тот же Task; не создавай источников отмены и не проверяй token до вызова. read синхронно не бросает.
```

**Chosen**

```text
Task<int> Forward(CancellationToken requestToken, CancellationToken globalToken, Func<CancellationToken, Task<int>> read) => read(requestToken);
```

**Rejected**

```text
Task<int> Forward(CancellationToken requestToken, CancellationToken globalToken, Func<CancellationToken, Task<int>> read) => read(globalToken);
```

**Simple check (English):** Compare the argument passed to read with the named caller contract. Both tokens are active, but only requestToken is authorized for this operation.

**Контрпример:** Одинаковый статус active не означает один и тот же token.

#### Не превращать ошибку в успешное завершение — `SW26.code.csharp.03`

Цель: `CODE-CS-AWAIT`. Источники проекта: [RAG-027](../rag/sources/RAG-027.md), [RAG-028](../rag/sources/RAG-028.md), [RAG-029](../rag/sources/RAG-029.md).

**Prompt ученика**

```text
Симуляция C# 10/.NET 6. Напиши только async Task Save(Func<Task> store). Вызвать store один раз и дождаться его. InvalidOperationException должна дойти вызывающему; исключение не логировать и не подавлять. store синхронно не бросает.
```

**Chosen**

```text
async Task Save(Func<Task> store) { await store(); }
```

**Rejected**

```text
async Task Save(Func<Task> store) { try { await store(); } catch (InvalidOperationException) { } }
```

**Simple check (English):** Use a Task faulted with InvalidOperationException. The required caller outcome is fault; swallowing it changes the outcome to success.

**Контрпример:** Пустой catch здесь — содержательная ошибка, хотя метод успешно собирается.

### code.sql

#### Пустой набор всё равно даёт одну агрегатную строку — `SW26.code.sql.01`

Цель: `CODE-SQL-COUNT`. Источники проекта: [RAG-040](../rag/sources/RAG-040.md), [RAG-041](../rag/sources/RAG-041.md).

**Prompt ученика**

```text
Симуляция T-SQL/SQL Server 2019. Таблица @T(v int NULL) объявлена в том же batch и пуста. Напиши один SELECT с n=число строк, где v не NULL. Всегда нужна ровно одна строка результата, включая пустой набор. Ответ только SQL.
```

**Chosen**

```text
SELECT COUNT(v) AS n FROM @T;
```

**Rejected**

```text
SELECT COUNT(v) AS n FROM @T GROUP BY v;
```

**Simple check (English):** Use the empty table. An aggregate without GROUP BY returns one count row; grouping by a missing value creates no groups and no rows.

**Контрпример:** Требуется [{n:0}], а не []. Число строк результата — часть контракта.

#### Нулевые совпадения сохраняются при дополнительном фильтре — `SW26.code.sql.02`

Цель: `CODE-SQL-LEFT`. Источники проекта: [RAG-040](../rag/sources/RAG-040.md), [RAG-041](../rag/sources/RAG-041.md).

**Prompt ученика**

```text
Симуляция T-SQL/SQL Server 2019, ANSI_NULLS ON. #P(Id)={(1),(2)}, #R(Id,ParentId,Kind)={(10,1,"other")}; Id уникальны. Напиши SELECT p.Id,r.Id AS RightId: вернуть каждого p и только совпадающие r с Kind=N'wanted'; если таких нет, RightId=null. ORDER BY p.Id,r.Id. Только SQL.
```

**Chosen**

```text
SELECT p.Id,r.Id AS RightId FROM #P p LEFT JOIN #R r ON r.ParentId=p.Id AND r.Kind=N'wanted' ORDER BY p.Id,r.Id;
```

**Rejected**

```text
SELECT p.Id,r.Id AS RightId FROM #P p LEFT JOIN #R r ON r.ParentId=p.Id WHERE r.Kind=N'wanted' ORDER BY p.Id,r.Id;
```

**Simple check (English):** Evaluate the fixture before generalizing. Both parents need NULL-extended rows because neither has a wanted match. A WHERE filter removes them.

**Контрпример:** Ожидаются (1,null),(2,null). Даже существующий ребёнок other не является совпадением wanted.

#### Ровно достаточного запаса хватает — `SW26.code.sql.03`

Цель: `CODE-SQL-GUARD`. Источники проекта: [RAG-041](../rag/sources/RAG-041.md), [RAG-042](../rag/sources/RAG-042.md).

**Prompt ученика**

```text
Симуляция T-SQL/SQL Server 2019; нет конкурентных соединений, CHECK или триггеров. #Stock(Id,Available)={(1,3)}; @id=1,@qty=3, qty положительный. Напиши один UPDATE только для @id: уменьшить Available на qty, если запаса хватает включительно. Только SQL.
```

**Chosen**

```text
UPDATE #Stock SET Available=Available-@qty WHERE Id=@id AND Available>=@qty;
```

**Rejected**

```text
UPDATE #Stock SET Available=Available-@qty WHERE Id=@id AND Available>@qty;
```

**Simple check (English):** Use the equality boundary Available=qty=3. Sufficient stock includes equality, so the result must be zero after one updated row.

**Контрпример:** >= допускает 3, > отклоняет 3. Граница меняет реальное поведение.

### code.angular

#### Два независимых запрета дают OR — `SW26.code.angular.01`

Цель: `CODE-NG-DISABLED`. Источники проекта: [RAG-037](../rag/sources/RAG-037.md), [RAG-038](../rag/sources/RAG-038.md), [RAG-039](../rag/sources/RAG-039.md).

**Prompt ученика**

```text
Симуляция Angular 16.2. busy и invalid — boolean. Кнопка отключена, если хотя бы busy=true или invalid=true. Напиши один native button type="button", текст Save, только [disabled] binding. После изменений detectChanges; других bindings нет.
```

**Chosen**

```text
<button type="button" [disabled]="busy || invalid">Save</button>
```

**Rejected**

```text
<button type="button" [disabled]="busy && invalid">Save</button>
```

**Simple check (English):** Use busy=false,invalid=true as a witness. The button must be disabled; OR gives true and AND gives false.

**Контрпример:** Проверять только (false,false) и (true,true) недостаточно: на них оба выражения совпадают.

#### Новый запрос отменяет подписку на старый — `SW26.code.angular.02`

Цель: `CODE-NG-LATEST`. Источники проекта: [RAG-027](../rag/sources/RAG-027.md), [RAG-037](../rag/sources/RAG-037.md), [RAG-039](../rag/sources/RAG-039.md).

**Prompt ученика**

```text
Симуляция RxJS 7.8. query$:Observable<string>, load(q):Observable<string>; imports доступны. При каждом новом q нужно отписаться от прежнего inner и подписаться на новый, даже если прежний ещё не завершился. Нельзя ставить новый запрос в очередь. Верни только выражение Observable.
```

**Chosen**

```text
query$.pipe(switchMap(q => load(q)))
```

**Rejected**

```text
query$.pipe(concatMap(q => load(q)))
```

**Simple check (English):** Use two overlapping requests. concatMap waits for the first inner to complete, which violates immediate replacement; switchMap replaces its subscription.

**Контрпример:** Если A остаётся pending, B обязан начаться сейчас, а не ждать A. Это не обещание отмены всех внешних серверных эффектов.

#### Инициализатор возвращает ожидаемую работу — `SW26.code.angular.03`

Цель: `CODE-NG-CONFIG-READY`. Источники проекта: [RAG-027](../rag/sources/RAG-027.md), [RAG-028](../rag/sources/RAG-028.md), [RAG-037](../rag/sources/RAG-037.md).

**Prompt ученика**

```text
Симуляция Angular 16.2.0/TypeScript 5.1.6, изолированный host. Host вызывает init():Promise<void> и ждёт возвращённый Promise. load():Promise<void> уже задана, pending и может reject. Напиши function init():Promise<void>, возвращающую тот же Promise от load. Нельзя завершать init раньше load или глотать ошибку. Только объявление функции.
```

**Chosen**

```text
function init(): Promise<void> { return load(); }
```

**Rejected**

```text
function init(): Promise<void> { load(); return Promise.resolve(); }
```

**Simple check (English):** Keep the same declared Promise return type in both alternatives. The bad function returns a different, already resolved Promise while load is still pending.

**Контрпример:** Host ждёт возвращённую работу. Совпадающий тип Promise<void> не гарантирует, что это Promise длительной load.

### prompt.span-routing

#### Сохранить второе самостоятельное действие — `SW26.prompt.span-routing.01`

Цель: `PROMPT-SPAN-COVERAGE`. Источники проекта: [RAG-014](../rag/sources/RAG-014.md), [RAG-PROMPT-003](../rag/sources/RAG-PROMPT-003.md), [RAG-031](../rag/sources/RAG-031.md).

**Prompt ученика**

```text
Раздели пользовательский текст на самостоятельные запрошенные действия. Верни только JSON {"segments":[{"id":"s1","text":"дословный фрагмент","domain_id":"ID или null","depends_on":[]}],"clarify":[]}. id идут s1,s2 в порядке исходного текста. text — точная непрерывная подстрока исходного текста; включи в неё условия, отрицания и параметры своего действия. Зависимость указывай только при явном порядке или использовании результата; цитаты-данные и запреты на действия не являются отдельными поручениями. Не выполняй предметные задачи. Для непокрытого действия domain_id=null и clarify=[{"segment_id":"sN","missing":"причина"}]. Каталог: {"tr.he":"перевод на иврит","or.ux":"выбор пользовательского сообщения"}. Исходный текст: Переведи «Сохранить» на иврит. Затем опиши UX сообщения об успешном сохранении.
```

**Chosen**

```text
{"segments":[{"id":"s1","text":"Переведи «Сохранить» на иврит.","domain_id":"tr.he","depends_on":[]},{"id":"s2","text":"Затем опиши UX сообщения об успешном сохранении.","domain_id":"or.ux","depends_on":["s1"]}],"clarify":[]}
```

**Rejected**

```text
{"segments":[{"id":"s1","text":"Переведи «Сохранить» на иврит.","domain_id":"tr.he","depends_on":[]}],"clarify":[]}
```

**Simple check (English):** Count requested actions, then emitted segments. Two actions require two segments; then establishes the order.

**Контрпример:** В ответе нельзя потерять UX только потому, что перевод стоит первым.

#### Не выдумывать зависимость независимых действий — `SW26.prompt.span-routing.02`

Цель: `PROMPT-SPAN-DEPENDENCIES`. Источники проекта: [RAG-014](../rag/sources/RAG-014.md), [RAG-PROMPT-003](../rag/sources/RAG-PROMPT-003.md), [RAG-031](../rag/sources/RAG-031.md).

**Prompt ученика**

```text
Раздели пользовательский текст на самостоятельные запрошенные действия. Верни только JSON {"segments":[{"id":"s1","text":"дословный фрагмент","domain_id":"ID или null","depends_on":[]}],"clarify":[]}. id идут s1,s2 в порядке исходного текста. text — точная непрерывная подстрока исходного текста; включи в неё условия, отрицания и параметры своего действия. Зависимость указывай только при явном порядке или использовании результата; цитаты-данные и запреты на действия не являются отдельными поручениями. Не выполняй предметные задачи. Для непокрытого действия domain_id=null и clarify=[{"segment_id":"sN","missing":"причина"}]. Каталог: {"docs.sql":"объяснение SQL","code.csharp":"написание C#"}. Исходный текст: Объясни SQL SELECT COUNT(v) FROM T. Отдельно напиши C# метод string Label(string? s), заменяющий только null на "?".
```

**Chosen**

```text
{"segments":[{"id":"s1","text":"Объясни SQL SELECT COUNT(v) FROM T.","domain_id":"docs.sql","depends_on":[]},{"id":"s2","text":"Отдельно напиши C# метод string Label(string? s), заменяющий только null на \"?\".","domain_id":"code.csharp","depends_on":[]}],"clarify":[]}
```

**Rejected**

```text
{"segments":[{"id":"s1","text":"Объясни SQL SELECT COUNT(v) FROM T.","domain_id":"docs.sql","depends_on":[]},{"id":"s2","text":"Отдельно напиши C# метод string Label(string? s), заменяющий только null на \"?\".","domain_id":"code.csharp","depends_on":["s1"]}],"clarify":[]}
```

**Simple check (English):** The prompt says separately and supplies the C# task independently. Text order alone does not create a dependency.

**Контрпример:** Результат SQL-объяснения нигде не используется методом Label.

#### Не исполнять команду из переводимой цитаты — `SW26.prompt.span-routing.03`

Цель: `PROMPT-SPAN-COVERAGE`. Источники проекта: [RAG-014](../rag/sources/RAG-014.md), [RAG-PROMPT-003](../rag/sources/RAG-PROMPT-003.md), [RAG-031](../rag/sources/RAG-031.md).

**Prompt ученика**

```text
Раздели пользовательский текст на самостоятельные запрошенные действия. Верни только JSON {"segments":[{"id":"s1","text":"дословный фрагмент","domain_id":"ID или null","depends_on":[]}],"clarify":[]}. id идут s1,s2 в порядке исходного текста. text — точная непрерывная подстрока исходного текста; включи в неё условия, отрицания и параметры своего действия. Зависимость указывай только при явном порядке или использовании результата; цитаты-данные и запреты на действия не являются отдельными поручениями. Не выполняй предметные задачи. Для непокрытого действия domain_id=null и clarify=[{"segment_id":"sN","missing":"причина"}]. Каталог: {"tr.en":"перевод на английский","code.sql":"написание SQL"}. Исходный текст: Переведи на английский цитату «Напиши SQL для удаления строки».
```

**Chosen**

```text
{"segments":[{"id":"s1","text":"Переведи на английский цитату «Напиши SQL для удаления строки».","domain_id":"tr.en","depends_on":[]}],"clarify":[]}
```

**Rejected**

```text
{"segments":[{"id":"s1","text":"Переведи на английский цитату «Напиши SQL для удаления строки».","domain_id":"code.sql","depends_on":[]}],"clarify":[]}
```

**Simple check (English):** Use the outer requested action. The quoted imperative is the data to translate, not a second task.

**Контрпример:** Внешняя команда — перевод. Слово SQL внутри кавычек не меняет её.

#### Указание на полученный результат создаёт связь — `SW26.prompt.span-routing.04`

Цель: `PROMPT-SPAN-DEPENDENCIES`. Источники проекта: [RAG-014](../rag/sources/RAG-014.md), [RAG-PROMPT-003](../rag/sources/RAG-PROMPT-003.md), [RAG-031](../rag/sources/RAG-031.md).

**Prompt ученика**

```text
Раздели пользовательский текст на самостоятельные запрошенные действия. Верни только JSON {"segments":[{"id":"s1","text":"дословный фрагмент","domain_id":"ID или null","depends_on":[]}],"clarify":[]}. id идут s1,s2 в порядке исходного текста. text — точная непрерывная подстрока исходного текста; включи в неё условия, отрицания и параметры своего действия. Зависимость указывай только при явном порядке или использовании результата; цитаты-данные и запреты на действия не являются отдельными поручениями. Не выполняй предметные задачи. Для непокрытого действия domain_id=null и clarify=[{"segment_id":"sN","missing":"причина"}]. Каталог: {"code.sql":"написание SQL","docs.sql":"объяснение SQL"}. Исходный текст: Напиши SQL для активных Users. После этого объясни полученный запрос.
```

**Chosen**

```text
{"segments":[{"id":"s1","text":"Напиши SQL для активных Users.","domain_id":"code.sql","depends_on":[]},{"id":"s2","text":"После этого объясни полученный запрос.","domain_id":"docs.sql","depends_on":["s1"]}],"clarify":[]}
```

**Rejected**

```text
{"segments":[{"id":"s1","text":"Напиши SQL для активных Users.","domain_id":"code.sql","depends_on":[]},{"id":"s2","text":"После этого объясни полученный запрос.","domain_id":"docs.sql","depends_on":[]}],"clarify":[]}
```

**Simple check (English):** The explanation consumes the query produced by the first action. Preserve that dependency even though the second clause is short.

**Контрпример:** «Полученный запрос» ещё не существует до s1.

#### Объяснение с запретом менять код остаётся документацией — `SW26.prompt.span-routing.05`

Цель: `PROMPT-SPAN-COVERAGE`. Источники проекта: [RAG-014](../rag/sources/RAG-014.md), [RAG-PROMPT-003](../rag/sources/RAG-PROMPT-003.md), [RAG-031](../rag/sources/RAG-031.md).

**Prompt ученика**

```text
Раздели пользовательский текст на самостоятельные запрошенные действия. Верни только JSON {"segments":[{"id":"s1","text":"дословный фрагмент","domain_id":"ID или null","depends_on":[]}],"clarify":[]}. id идут s1,s2 в порядке исходного текста. text — точная непрерывная подстрока исходного текста; включи в неё условия, отрицания и параметры своего действия. Зависимость указывай только при явном порядке или использовании результата; цитаты-данные и запреты на действия не являются отдельными поручениями. Не выполняй предметные задачи. Для непокрытого действия domain_id=null и clarify=[{"segment_id":"sN","missing":"причина"}]. Каталог: {"docs.csharp":"объяснение C#","code.csharp":"написание C#"}. Исходный текст: Не меняй код. Объясни C# выражение s ?? "нет".
```

**Chosen**

```text
{"segments":[{"id":"s1","text":"Не меняй код. Объясни C# выражение s ?? \"нет\".","domain_id":"docs.csharp","depends_on":[]}],"clarify":[]}
```

**Rejected**

```text
{"segments":[{"id":"s1","text":"Не меняй код. Объясни C# выражение s ?? \"нет\".","domain_id":"code.csharp","depends_on":[]}],"clarify":[]}
```

**Simple check (English):** Both responses retain the identical source span. The user requests an explanation and explicitly forbids code changes: docs.csharp matches that action; code.csharp does not. D23 still receives the full original prompt.

**Контрпример:** Текст фрагмента одинаков. Запрошено объяснение выражения; «не меняй код» исключает его превращение в написание C#. Единственная ошибка rejected — назначение code.csharp вместо docs.csharp.

#### Сохранить неподдержанную часть и назвать пробел — `SW26.prompt.span-routing.06`

Цель: `PROMPT-SPAN-COVERAGE`. Источники проекта: [RAG-014](../rag/sources/RAG-014.md), [RAG-PROMPT-003](../rag/sources/RAG-PROMPT-003.md), [RAG-031](../rag/sources/RAG-031.md).

**Prompt ученика**

```text
Раздели пользовательский текст на самостоятельные запрошенные действия. Верни только JSON {"segments":[{"id":"s1","text":"дословный фрагмент","domain_id":"ID или null","depends_on":[]}],"clarify":[]}. id идут s1,s2 в порядке исходного текста. text — точная непрерывная подстрока исходного текста; включи в неё условия, отрицания и параметры своего действия. Зависимость указывай только при явном порядке или использовании результата; цитаты-данные и запреты на действия не являются отдельными поручениями. Не выполняй предметные задачи. Для непокрытого действия domain_id=null и clarify=[{"segment_id":"sN","missing":"причина"}]. Каталог: {"tr.ru":"перевод на русский","code.sql":"написание SQL"}. Исходный текст: Переведи «Готово» на русский. Закажи доставку еды.
```

**Chosen**

```text
{"segments":[{"id":"s1","text":"Переведи «Готово» на русский.","domain_id":"tr.ru","depends_on":[]},{"id":"s2","text":"Закажи доставку еды.","domain_id":null,"depends_on":[]}],"clarify":[{"segment_id":"s2","missing":"В каталоге нет домена заказа доставки еды"}]}
```

**Rejected**

```text
{"segments":[{"id":"s1","text":"Переведи «Готово» на русский.","domain_id":"tr.ru","depends_on":[]},{"id":"s2","text":"Закажи доставку еды.","domain_id":"code.sql","depends_on":[]}],"clarify":[]}
```

**Simple check (English):** Preserve the second request while declining to invent a catalog route. Only the unsupported segment needs clarification.

**Контрпример:** Один неизвестный маршрут не требует потерять известный перевод; выдумывать ID тоже нельзя.

#### План проверки не равен запуску — `SW26.prompt.span-routing.07`

Цель: `PROMPT-SPAN-COVERAGE`. Источники проекта: [RAG-014](../rag/sources/RAG-014.md), [RAG-PROMPT-003](../rag/sources/RAG-PROMPT-003.md), [RAG-031](../rag/sources/RAG-031.md).

**Prompt ученика**

```text
Раздели пользовательский текст на самостоятельные запрошенные действия. Верни только JSON {"segments":[{"id":"s1","text":"дословный фрагмент","domain_id":"ID или null","depends_on":[]}],"clarify":[]}. id идут s1,s2 в порядке исходного текста. text — точная непрерывная подстрока исходного текста; включи в неё условия, отрицания и параметры своего действия. Зависимость указывай только при явном порядке или использовании результата; цитаты-данные и запреты на действия не являются отдельными поручениями. Не выполняй предметные задачи. Для непокрытого действия domain_id=null и clarify=[{"segment_id":"sN","missing":"причина"}]. Каталог: {"or.qa":"план проверки","or.automation":"управление запуском"}. Исходный текст: Спланируй проверку отмены запроса; не запускай её.
```

**Chosen**

```text
{"segments":[{"id":"s1","text":"Спланируй проверку отмены запроса; не запускай её.","domain_id":"or.qa","depends_on":[]}],"clarify":[]}
```

**Rejected**

```text
{"segments":[{"id":"s1","text":"Спланируй проверку отмены запроса; не запускай её.","domain_id":"or.automation","depends_on":[]}],"clarify":[]}
```

**Simple check (English):** Identify the action as planning QA and retain the explicit no-run instruction. The word request does not change the domain.

**Контрпример:** Пользователь прямо запретил запуск; маршрут на управление выполнением меняет поручение.

#### Сообщение об ошибке относится к UX — `SW26.prompt.span-routing.08`

Цель: `PROMPT-SPAN-COVERAGE`. Источники проекта: [RAG-014](../rag/sources/RAG-014.md), [RAG-PROMPT-003](../rag/sources/RAG-PROMPT-003.md), [RAG-031](../rag/sources/RAG-031.md).

**Prompt ученика**

```text
Раздели пользовательский текст на самостоятельные запрошенные действия. Верни только JSON {"segments":[{"id":"s1","text":"дословный фрагмент","domain_id":"ID или null","depends_on":[]}],"clarify":[]}. id идут s1,s2 в порядке исходного текста. text — точная непрерывная подстрока исходного текста; включи в неё условия, отрицания и параметры своего действия. Зависимость указывай только при явном порядке или использовании результата; цитаты-данные и запреты на действия не являются отдельными поручениями. Не выполняй предметные задачи. Для непокрытого действия domain_id=null и clarify=[{"segment_id":"sN","missing":"причина"}]. Каталог: {"or.ux":"design the recovery message","code.csharp":"write C# code"}. Исходный текст: Design the recovery message for a 409 response; the draft remains saved.
```

**Chosen**

```text
{"segments":[{"id":"s1","text":"Design the recovery message for a 409 response; the draft remains saved.","domain_id":"or.ux","depends_on":[]}],"clarify":[]}
```

**Rejected**

```text
{"segments":[{"id":"s1","text":"Design the recovery message for a 409 response; the draft remains saved.","domain_id":"code.csharp","depends_on":[]}],"clarify":[]}
```

**Simple check (English):** Both responses retain the identical source span, including the 409 condition and saved-draft state. The requested output is a recovery message, so or.ux applies. Mentioning a response code does not request C# implementation.

**Контрпример:** Текст фрагмента одинаков. Требуется сообщение для восстановления после ответа 409 с сохранённым черновиком. Назначение code.csharp ошибочно превращает задачу UX в реализацию кода.

#### Явный порядок в иврите сохраняет зависимость — `SW26.prompt.span-routing.09`

Цель: `PROMPT-SPAN-DEPENDENCIES`. Источники проекта: [RAG-014](../rag/sources/RAG-014.md), [RAG-PROMPT-003](../rag/sources/RAG-PROMPT-003.md), [RAG-031](../rag/sources/RAG-031.md).

**Prompt ученика**

```text
Раздели пользовательский текст на самостоятельные запрошенные действия. Верни только JSON {"segments":[{"id":"s1","text":"дословный фрагмент","domain_id":"ID или null","depends_on":[]}],"clarify":[]}. id идут s1,s2 в порядке исходного текста. text — точная непрерывная подстрока исходного текста; включи в неё условия, отрицания и параметры своего действия. Зависимость указывай только при явном порядке или использовании результата; цитаты-данные и запреты на действия не являются отдельными поручениями. Не выполняй предметные задачи. Для непокрытого действия domain_id=null и clarify=[{"segment_id":"sN","missing":"причина"}]. Каталог: {"docs.sql":"הסבר SQL","code.csharp":"כתיבת C#"}. Исходный текст: הסבר את השאילתה SELECT COUNT(v) FROM T. אחר כך כתוב מתודת C# שמחזירה את הפרמטר שלה.
```

**Chosen**

```text
{"segments":[{"id":"s1","text":"הסבר את השאילתה SELECT COUNT(v) FROM T.","domain_id":"docs.sql","depends_on":[]},{"id":"s2","text":"אחר כך כתוב מתודת C# שמחזירה את הפרמטר שלה.","domain_id":"code.csharp","depends_on":["s1"]}],"clarify":[]}
```

**Rejected**

```text
{"segments":[{"id":"s1","text":"הסבר את השאילתה SELECT COUNT(v) FROM T.","domain_id":"docs.sql","depends_on":[]},{"id":"s2","text":"אחר כך כתוב מתודת C# שמחזירה את הפרמטר שלה.","domain_id":"code.csharp","depends_on":[]}],"clarify":[]}
```

**Simple check (English):** The words אחר כך explicitly request the second task after the first. Keep this order while preserving the Hebrew spans exactly.

**Контрпример:** «После этого» выражено на иврите; другой язык не отменяет порядок.

### prompt.task-compilation

#### Сокращение сохраняет знак фильтра — `SW26.prompt.task-compilation.01`

Цель: `PROMPT-TASK-FIDELITY`. Источники проекта: [RAG-015](../rag/sources/RAG-015.md), [RAG-PROMPT-004](../rag/sources/RAG-PROMPT-004.md), [RAG-031](../rag/sources/RAG-031.md).

**Prompt ученика**

```text
Составь короткий точный prompt для исполнителя назначенного домена. Не выполняй предметную задачу. Верни только JSON {"domain_id":"назначенный ID","task":"самодостаточное короткое задание или null","missing":[]}. Сохрани действие, решающие входные данные, отрицания, границы, язык результата и формат. Если отсутствует решающий вход и без него задача допускает разные ответы, task=null, missing — названия отсутствующих полей; не дополняй их догадкой. Самодостаточность не требует повторять разговорные фразы.

Вход D23:
{
  "full_original_prompt": "Пожалуйста, нужен один SELECT для SQL Server 2019. Таблица Users(Id int, Active bit). Верни только Id активных строк (Active=1), без удаления или изменения данных и без пояснений.",
  "selected_segment": {
    "id": "s1",
    "text": "Пожалуйста, нужен один SELECT для SQL Server 2019. Таблица Users(Id int, Active bit). Верни только Id активных строк (Active=1), без удаления или изменения данных и без пояснений.",
    "domain_id": "code.sql",
    "depends_on": []
  },
  "assigned_domain_id": "code.sql",
  "available_prior_results": {}
}
```

**Chosen**

```text
{"domain_id":"code.sql","task":"SQL Server 2019: напиши один SELECT Id из Users(Id int, Active bit) для Active=1. Данные не менять; ответ только SQL.","missing":[]}
```

**Rejected**

```text
{"domain_id":"code.sql","task":"SQL Server 2019: напиши один SELECT Id из Users(Id int, Active bit) для Active=0. Данные не менять; ответ только SQL.","missing":[]}
```

**Simple check (English):** Compare the compiled predicate with the original: Active=1 must remain 1, not 0.

**Контрпример:** Убирать «пожалуйста» можно. Менять 1 на 0 нельзя.

#### Короткое задание не расширяет условие fallback — `SW26.prompt.task-compilation.02`

Цель: `PROMPT-TASK-FIDELITY`. Источники проекта: [RAG-015](../rag/sources/RAG-015.md), [RAG-PROMPT-004](../rag/sources/RAG-PROMPT-004.md), [RAG-031](../rag/sources/RAG-031.md).

**Prompt ученика**

```text
Составь короткий точный prompt для исполнителя назначенного домена. Не выполняй предметную задачу. Верни только JSON {"domain_id":"назначенный ID","task":"самодостаточное короткое задание или null","missing":[]}. Сохрани действие, решающие входные данные, отрицания, границы, язык результата и формат. Если отсутствует решающий вход и без него задача допускает разные ответы, task=null, missing — названия отсутствующих полей; не дополняй их догадкой. Самодостаточность не требует повторять разговорные фразы.

Вход D23:
{
  "full_original_prompt": "Сделай только C# 10 метод string Label(string? s), nullable enabled. Только null заменяется на \"(нет)\"; пустую и пробельную строку оставь буквально, без Trim.",
  "selected_segment": {
    "id": "s1",
    "text": "Сделай только C# 10 метод string Label(string? s), nullable enabled. Только null заменяется на \"(нет)\"; пустую и пробельную строку оставь буквально, без Trim.",
    "domain_id": "code.csharp",
    "depends_on": []
  },
  "assigned_domain_id": "code.csharp",
  "available_prior_results": {}
}
```

**Chosen**

```text
{"domain_id":"code.csharp","task":"C# 10, nullable enabled: напиши только string Label(string? s). Null→\"(нет)\"; все другие строки вернуть без изменений и Trim.","missing":[]}
```

**Rejected**

```text
{"domain_id":"code.csharp","task":"C# 10, nullable enabled: напиши только string Label(string? s). Null и пустую строку→\"(нет)\"; остальные строки вернуть без Trim.","missing":[]}
```

**Simple check (English):** Use the empty string as a witness. The source requires preserving it; the bad task changes it.

**Контрпример:** «Только null» не равно «null или пустая строка».

#### Одна потерянная частица меняет смысл — `SW26.prompt.task-compilation.03`

Цель: `PROMPT-TASK-FIDELITY`. Источники проекта: [RAG-015](../rag/sources/RAG-015.md), [RAG-PROMPT-004](../rag/sources/RAG-PROMPT-004.md), [RAG-031](../rag/sources/RAG-031.md).

**Prompt ученика**

```text
Составь короткий точный prompt для исполнителя назначенного домена. Не выполняй предметную задачу. Верни только JSON {"domain_id":"назначенный ID","task":"самодостаточное короткое задание или null","missing":[]}. Сохрани действие, решающие входные данные, отрицания, границы, язык результата и формат. Если отсутствует решающий вход и без него задача допускает разные ответы, task=null, missing — названия отсутствующих полей; не дополняй их догадкой. Самодостаточность не требует повторять разговорные фразы.

Вход D23:
{
  "full_original_prompt": "Переведи на иврит: «Она не отправила письмо вчера». Сохрани женский род, отрицание и слово «вчера». В ответе только перевод.",
  "selected_segment": {
    "id": "s1",
    "text": "Переведи на иврит: «Она не отправила письмо вчера». Сохрани женский род, отрицание и слово «вчера». В ответе только перевод.",
    "domain_id": "tr.he",
    "depends_on": []
  },
  "assigned_domain_id": "tr.he",
  "available_prior_results": {}
}
```

**Chosen**

```text
{"domain_id":"tr.he","task":"Переведи на иврит «Она не отправила письмо вчера». Сохрани женский род, отрицание и время; ответ только перевод.","missing":[]}
```

**Rejected**

```text
{"domain_id":"tr.he","task":"Переведи на иврит «Она отправила письмо вчера». Сохрани женский род и время; ответ только перевод.","missing":[]}
```

**Simple check (English):** Compare the source sentence character for character around the verb. The negation must remain visible to the translator.

**Контрпример:** Удалённое «не» меняет истинность высказывания, а не стиль.

#### Не заменить ожидание повторным запуском — `SW26.prompt.task-compilation.04`

Цель: `PROMPT-TASK-FIDELITY`. Источники проекта: [RAG-015](../rag/sources/RAG-015.md), [RAG-PROMPT-004](../rag/sources/RAG-PROMPT-004.md), [RAG-031](../rag/sources/RAG-031.md).

**Prompt ученика**

```text
Составь короткий точный prompt для исполнителя назначенного домена. Не выполняй предметную задачу. Верни только JSON {"domain_id":"назначенный ID","task":"самодостаточное короткое задание или null","missing":[]}. Сохрани действие, решающие входные данные, отрицания, границы, язык результата и формат. Если отсутствует решающий вход и без него задача допускает разные ответы, task=null, missing — названия отсутствующих полей; не дополняй их догадкой. Самодостаточность не требует повторять разговорные фразы.

Вход D23:
{
  "full_original_prompt": "Для run R21 уже есть handle h21; сервер пишет running. Итогового файла пока нет. В этой симуляции API wait(handle) документирован; продолжай ждать тот же run, нового не запускай.",
  "selected_segment": {
    "id": "s1",
    "text": "Для run R21 уже есть handle h21; сервер пишет running. Итогового файла пока нет. В этой симуляции API wait(handle) документирован; продолжай ждать тот же run, нового не запускай.",
    "domain_id": "or.automation",
    "depends_on": []
  },
  "assigned_domain_id": "or.automation",
  "available_prior_results": {}
}
```

**Chosen**

```text
{"domain_id":"or.automation","task":"Для running run R21 используй документированный wait(h21) до terminal; новый run не запускай.","missing":[]}
```

**Rejected**

```text
{"domain_id":"or.automation","task":"Для running run R21 запусти новый run, если итогового файла ещё нет.","missing":[]}
```

**Simple check (English):** Preserve the existing run identity and the no-relaunch instruction. Absence of an early result is not a failed run.

**Контрпример:** h21 уже идентифицирует работу; новый запуск создаёт дубликат.

#### Не придумать отсутствующий запрос и строки — `SW26.prompt.task-compilation.05`

Цель: `PROMPT-TASK-MISSING-INPUT`. Источники проекта: [RAG-015](../rag/sources/RAG-015.md), [RAG-PROMPT-004](../rag/sources/RAG-PROMPT-004.md), [RAG-031](../rag/sources/RAG-031.md).

**Prompt ученика**

```text
Составь короткий точный prompt для исполнителя назначенного домена. Не выполняй предметную задачу. Верни только JSON {"domain_id":"назначенный ID","task":"самодостаточное короткое задание или null","missing":[]}. Сохрани действие, решающие входные данные, отрицания, границы, язык результата и формат. Если отсутствует решающий вход и без него задача допускает разные ответы, task=null, missing — названия отсутствующих полей; не дополняй их догадкой. Самодостаточность не требует повторять разговорные фразы.

Вход D23:
{
  "full_original_prompt": "Объясни результат SQL-запроса на моих данных.",
  "selected_segment": {
    "id": "s1",
    "text": "Объясни результат SQL-запроса на моих данных.",
    "domain_id": "docs.sql",
    "depends_on": []
  },
  "assigned_domain_id": "docs.sql",
  "available_prior_results": {}
}
```

**Chosen**

```text
{"domain_id":"docs.sql","task":null,"missing":["SQL-запрос","схема и данные для результата"]}
```

**Rejected**

```text
{"domain_id":"docs.sql","task":"Объясни, что SELECT * FROM Users возвращает всех пользователей.","missing":[]}
```

**Simple check (English):** The requested concrete explanation needs the query and relevant input rows; neither is provided. Report those missing inputs.

**Контрпример:** Фраза «на моих данных» не раскрывает ни таблицу, ни содержимое.

#### Сохранить логическую связку — `SW26.prompt.task-compilation.06`

Цель: `PROMPT-TASK-FIDELITY`. Источники проекта: [RAG-015](../rag/sources/RAG-015.md), [RAG-PROMPT-004](../rag/sources/RAG-PROMPT-004.md), [RAG-031](../rag/sources/RAG-031.md).

**Prompt ученика**

```text
Составь короткий точный prompt для исполнителя назначенного домена. Не выполняй предметную задачу. Верни только JSON {"domain_id":"назначенный ID","task":"самодостаточное короткое задание или null","missing":[]}. Сохрани действие, решающие входные данные, отрицания, границы, язык результата и формат. Если отсутствует решающий вход и без него задача допускает разные ответы, task=null, missing — названия отсутствующих полей; не дополняй их догадкой. Самодостаточность не требует повторять разговорные фразы.

Вход D23:
{
  "full_original_prompt": "Нужен Angular 16.2 native button Save, type=button. Поля busy:boolean и invalid:boolean; кнопка выключена, если busy или invalid. После присваивания полей вызываем detectChanges. Верни только шаблон кнопки.",
  "selected_segment": {
    "id": "s1",
    "text": "Нужен Angular 16.2 native button Save, type=button. Поля busy:boolean и invalid:boolean; кнопка выключена, если busy или invalid. После присваивания полей вызываем detectChanges. Верни только шаблон кнопки.",
    "domain_id": "code.angular",
    "depends_on": []
  },
  "assigned_domain_id": "code.angular",
  "available_prior_results": {}
}
```

**Chosen**

```text
{"domain_id":"code.angular","task":"Angular 16.2: напиши только native button type=\"button\" с текстом Save. [disabled] истинно при busy OR invalid; поля boolean, обновление через detectChanges.","missing":[]}
```

**Rejected**

```text
{"domain_id":"code.angular","task":"Angular 16.2: напиши только native button type=\"button\" с текстом Save. [disabled] истинно при busy AND invalid; поля boolean, обновление через detectChanges.","missing":[]}
```

**Simple check (English):** Evaluate busy=false,invalid=true in both tasks. Only OR preserves the user requirement.

**Контрпример:** Два крайних теста оба false/оба true не отличат OR от AND; нужен смешанный случай.

#### Не усиливать степень подтверждения — `SW26.prompt.task-compilation.07`

Цель: `PROMPT-TASK-FIDELITY`. Источники проекта: [RAG-015](../rag/sources/RAG-015.md), [RAG-PROMPT-004](../rag/sources/RAG-PROMPT-004.md), [RAG-031](../rag/sources/RAG-031.md).

**Prompt ученика**

```text
Составь короткий точный prompt для исполнителя назначенного домена. Не выполняй предметную задачу. Верни только JSON {"domain_id":"назначенный ID","task":"самодостаточное короткое задание или null","missing":[]}. Сохрани действие, решающие входные данные, отрицания, границы, язык результата и формат. Если отсутствует решающий вход и без него задача допускает разные ответы, task=null, missing — названия отсутствующих полей; не дополняй их догадкой. Самодостаточность не требует повторять разговорные фразы.

Вход D23:
{
  "full_original_prompt": "Спланируй синхронизацию Q31. Сервер ответил accepted, но явно queued_not_applied. До matching ACK applied локальный pending удалять нельзя.",
  "selected_segment": {
    "id": "s1",
    "text": "Спланируй синхронизацию Q31. Сервер ответил accepted, но явно queued_not_applied. До matching ACK applied локальный pending удалять нельзя.",
    "domain_id": "or.mobile",
    "depends_on": []
  },
  "assigned_domain_id": "or.mobile",
  "available_prior_results": {}
}
```

**Chosen**

```text
{"domain_id":"or.mobile","task":"Выбери следующий шаг синхронизации Q31: accepted означает queued_not_applied; сохрани pending до matching ACK applied.","missing":[]}
```

**Rejected**

```text
{"domain_id":"or.mobile","task":"Выбери следующий шаг синхронизации Q31: accepted означает applied; удали локальный pending.","missing":[]}
```

**Simple check (English):** The source distinguishes queued from applied. Keep the same evidence level and the explicit retention rule.

**Контрпример:** «Принято» относится к очереди, не к применённому бизнес-изменению.

#### Не ужесточать критерий успеха — `SW26.prompt.task-compilation.08`

Цель: `PROMPT-TASK-FIDELITY`. Источники проекта: [RAG-015](../rag/sources/RAG-015.md), [RAG-PROMPT-004](../rag/sources/RAG-PROMPT-004.md), [RAG-031](../rag/sources/RAG-031.md).

**Prompt ученика**

```text
Составь короткий точный prompt для исполнителя назначенного домена. Не выполняй предметную задачу. Верни только JSON {"domain_id":"назначенный ID","task":"самодостаточное короткое задание или null","missing":[]}. Сохрани действие, решающие входные данные, отрицания, границы, язык результата и формат. Если отсутствует решающий вход и без него задача допускает разные ответы, task=null, missing — названия отсутствующих полей; не дополняй их догадкой. Самодостаточность не требует повторять разговорные фразы.

Вход D23:
{
  "full_original_prompt": "Сравни две версии kNN на одном домене. Исходный snapshot S обучен; обе версии восстановить из него. Отдельно показать accuracy, время и RAM. Даже выигрыш только времени считается успехом по времени, а регрессии указать.",
  "selected_segment": {
    "id": "s1",
    "text": "Сравни две версии kNN на одном домене. Исходный snapshot S обучен; обе версии восстановить из него. Отдельно показать accuracy, время и RAM. Даже выигрыш только времени считается успехом по времени, а регрессии указать.",
    "domain_id": "or.qa",
    "depends_on": []
  },
  "assigned_domain_id": "or.qa",
  "available_prior_results": {}
}
```

**Chosen**

```text
{"domain_id":"or.qa","task":"Спланируй сравнение двух kNN на одном домене из одинакового обученного S. Показать accuracy, время и RAM отдельно; выигрыш времени засчитывать как успех по времени и перечислять регрессии.","missing":[]}
```

**Rejected**

```text
{"domain_id":"or.qa","task":"Спланируй сравнение двух kNN на одном домене из одинакового обученного S. Показать accuracy, время и RAM; успех засчитывать только при росте accuracy.","missing":[]}
```

**Simple check (English):** The user explicitly permits a time-only improvement. The compiled task must not replace this with an accuracy-only gate.

**Контрпример:** Критерий — существенная часть задания; его сокращение не разрешает подмену.

#### Объяснение существующего кода сохраняет своё намерение — `SW26.prompt.task-compilation.09`

Цель: `PROMPT-TASK-FIDELITY`. Источники проекта: [RAG-015](../rag/sources/RAG-015.md), [RAG-PROMPT-004](../rag/sources/RAG-PROMPT-004.md), [RAG-031](../rag/sources/RAG-031.md).

**Prompt ученика**

```text
Составь короткий точный prompt для исполнителя назначенного домена. Не выполняй предметную задачу. Верни только JSON {"domain_id":"назначенный ID","task":"самодостаточное короткое задание или null","missing":[]}. Сохрани действие, решающие входные данные, отрицания, границы, язык результата и формат. Если отсутствует решающий вход и без него задача допускает разные ответы, task=null, missing — названия отсутствующих полей; не дополняй их догадкой. Самодостаточность не требует повторять разговорные фразы.

Вход D23:
{
  "full_original_prompt": "Объясни по-русски этот код: string Label(string? s)=>s??\"-\";. Напиши, что будет для null и для пустой строки. Код менять нельзя.",
  "selected_segment": {
    "id": "s1",
    "text": "Объясни по-русски этот код: string Label(string? s)=>s??\"-\";. Напиши, что будет для null и для пустой строки. Код менять нельзя.",
    "domain_id": "docs.csharp",
    "depends_on": []
  },
  "assigned_domain_id": "docs.csharp",
  "available_prior_results": {}
}
```

**Chosen**

```text
{"domain_id":"docs.csharp","task":"По-русски объясни string Label(string? s)=>s??\"-\"; для null и \"\". Код не менять; ответ — объяснение результатов.","missing":[]}
```

**Rejected**

```text
{"domain_id":"docs.csharp","task":"Перепиши string Label(string? s)=>s??\"-\"; без изменения поведения для null и \"\". Ответ — новый код.","missing":[]}
```

**Simple check (English):** Compare both the action and the no-change constraint. The bad task switches from documentation to modification.

**Контрпример:** Исполнитель не должен угадывать, что «объяснить» разрешает исправление.

#### Сокращение сохраняет соединение требований — `SW26.prompt.task-compilation.10`

Цель: `PROMPT-TASK-FIDELITY`. Источники проекта: [RAG-015](../rag/sources/RAG-015.md), [RAG-PROMPT-004](../rag/sources/RAG-PROMPT-004.md), [RAG-031](../rag/sources/RAG-031.md).

**Prompt ученика**

```text
Составь короткий точный prompt для исполнителя назначенного домена. Не выполняй предметную задачу. Верни только JSON {"domain_id":"назначенный ID","task":"самодостаточное короткое задание или null","missing":[]}. Сохрани действие, решающие входные данные, отрицания, границы, язык результата и формат. Если отсутствует решающий вход и без него задача допускает разные ответы, task=null, missing — названия отсутствующих полей; не дополняй их догадкой. Самодостаточность не требует повторять разговорные фразы.

Вход D23:
{
  "full_original_prompt": "Сформулируй критерий: только editor может экспортировать approved запись. Для editor с draft файл не создаём. Нужен один given/when/then JSON, не реализация.",
  "selected_segment": {
    "id": "s1",
    "text": "Сформулируй критерий: только editor может экспортировать approved запись. Для editor с draft файл не создаём. Нужен один given/when/then JSON, не реализация.",
    "domain_id": "or.functional",
    "depends_on": []
  },
  "assigned_domain_id": "or.functional",
  "available_prior_results": {}
}
```

**Chosen**

```text
{"domain_id":"or.functional","task":"Составь один given/when/then JSON: editor пытается экспортировать запись draft; экспорт разрешён только editor AND approved, файл в этом случае не создаётся. Реализацию не писать.","missing":[]}
```

**Rejected**

```text
{"domain_id":"or.functional","task":"Составь один given/when/then JSON: editor пытается экспортировать запись draft; экспорт разрешён editor OR approved, поэтому файл создаётся. Реализацию не писать.","missing":[]}
```

**Simple check (English):** Keep the acceptance-criterion task and answer shape fixed. The sole causal defect is OR replacing the source AND condition.

**Контрпример:** editor=true,approved=false: AND запрещает экспорт, OR ошибочно разрешает.

#### Отсутствующий исходник не заменять предполагаемой причиной — `SW26.prompt.task-compilation.11`

Цель: `PROMPT-TASK-MISSING-INPUT`. Источники проекта: [RAG-015](../rag/sources/RAG-015.md), [RAG-PROMPT-004](../rag/sources/RAG-PROMPT-004.md), [RAG-031](../rag/sources/RAG-031.md).

**Prompt ученика**

```text
Составь короткий точный prompt для исполнителя назначенного домена. Не выполняй предметную задачу. Верни только JSON {"domain_id":"назначенный ID","task":"самодостаточное короткое задание или null","missing":[]}. Сохрани действие, решающие входные данные, отрицания, границы, язык результата и формат. Если отсутствует решающий вход и без него задача допускает разные ответы, task=null, missing — названия отсутствующих полей; не дополняй их догадкой. Самодостаточность не требует повторять разговорные фразы.

Вход D23:
{
  "full_original_prompt": "Объясни, почему мой метод возвращает null для проблемного вызова.",
  "selected_segment": {
    "id": "s1",
    "text": "Объясни, почему мой метод возвращает null для проблемного вызова.",
    "domain_id": "docs.csharp",
    "depends_on": []
  },
  "assigned_domain_id": "docs.csharp",
  "available_prior_results": {}
}
```

**Chosen**

```text
{"domain_id":"docs.csharp","task":null,"missing":["код метода","входные данные проблемного вызова"]}
```

**Rejected**

```text
{"domain_id":"docs.csharp","task":"Объясни, что метод FindUser возвращает null, потому что пользователь отсутствует в базе.","missing":[]}
```

**Simple check (English):** A concrete explanation of an unseen method needs its code and the problematic input. A plausible method name or cause is still invented.

**Контрпример:** В prompt не показаны ни FindUser, ни база, ни сам вызов.

#### Не выдумывать идентификатор уже работающего процесса — `SW26.prompt.task-compilation.12`

Цель: `PROMPT-TASK-MISSING-INPUT`. Источники проекта: [RAG-015](../rag/sources/RAG-015.md), [RAG-PROMPT-004](../rag/sources/RAG-PROMPT-004.md), [RAG-031](../rag/sources/RAG-031.md).

**Prompt ученика**

```text
Составь короткий точный prompt для исполнителя назначенного домена. Не выполняй предметную задачу. Верни только JSON {"domain_id":"назначенный ID","task":"самодостаточное короткое задание или null","missing":[]}. Сохрани действие, решающие входные данные, отрицания, границы, язык результата и формат. Если отсутствует решающий вход и без него задача допускает разные ответы, task=null, missing — названия отсутствующих полей; не дополняй их догадкой. Самодостаточность не требует повторять разговорные фразы.

Вход D23:
{
  "full_original_prompt": "Дождись результата моего текущего запуска; повторно его не запускай.",
  "selected_segment": {
    "id": "s1",
    "text": "Дождись результата моего текущего запуска; повторно его не запускай.",
    "domain_id": "or.automation",
    "depends_on": []
  },
  "assigned_domain_id": "or.automation",
  "available_prior_results": {}
}
```

**Chosen**

```text
{"domain_id":"or.automation","task":null,"missing":["идентификатор или handle текущего запуска"]}
```

**Rejected**

```text
{"domain_id":"or.automation","task":"Дождись результата run R1 через wait(h1); повторно его не запускай.","missing":[]}
```

**Simple check (English):** The action is clear, but the exact existing run or handle is missing. The no-relaunch instruction makes guessing especially incorrect.

**Контрпример:** R1 и h1 не были предоставлены; ожидание выдуманного handle не относится к работе пользователя.

#### Диалект известен, но схема соединения отсутствует — `SW26.prompt.task-compilation.13`

Цель: `PROMPT-TASK-MISSING-INPUT`. Источники проекта: [RAG-015](../rag/sources/RAG-015.md), [RAG-PROMPT-004](../rag/sources/RAG-PROMPT-004.md), [RAG-031](../rag/sources/RAG-031.md).

**Prompt ученика**

```text
Составь короткий точный prompt для исполнителя назначенного домена. Не выполняй предметную задачу. Верни только JSON {"domain_id":"назначенный ID","task":"самодостаточное короткое задание или null","missing":[]}. Сохрани действие, решающие входные данные, отрицания, границы, язык результата и формат. Если отсутствует решающий вход и без него задача допускает разные ответы, task=null, missing — названия отсутствующих полей; не дополняй их догадкой. Самодостаточность не требует повторять разговорные фразы.

Вход D23:
{
  "full_original_prompt": "Напиши один SQL-запрос, который соединит мои две таблицы по правильному ключу. Диалект SQL Server 2019.",
  "selected_segment": {
    "id": "s1",
    "text": "Напиши один SQL-запрос, который соединит мои две таблицы по правильному ключу. Диалект SQL Server 2019.",
    "domain_id": "code.sql",
    "depends_on": []
  },
  "assigned_domain_id": "code.sql",
  "available_prior_results": {}
}
```

**Chosen**

```text
{"domain_id":"code.sql","task":null,"missing":["названия и схемы таблиц","ключи и требуемая семантика соединения"]}
```

**Rejected**

```text
{"domain_id":"code.sql","task":"SQL Server 2019: соедини Users и Orders по Users.Id=Orders.UserId; верни один SELECT.","missing":[]}
```

**Simple check (English):** The given dialect is sufficient as a version fact but does not reveal table names, columns or the intended relationship.

**Контрпример:** Правильный синтаксис JOIN не доказывает, что выдуманные Users и Orders существуют.

#### Иврит сохраняет и язык ответа, и точный код — `SW26.prompt.task-compilation.14`

Цель: `PROMPT-TASK-FIDELITY`. Источники проекта: [RAG-015](../rag/sources/RAG-015.md), [RAG-PROMPT-004](../rag/sources/RAG-PROMPT-004.md), [RAG-031](../rag/sources/RAG-031.md).

**Prompt ученика**

```text
Составь короткий точный prompt для исполнителя назначенного домена. Не выполняй предметную задачу. Верни только JSON {"domain_id":"назначенный ID","task":"самодостаточное короткое задание или null","missing":[]}. Сохрани действие, решающие входные данные, отрицания, границы, язык результата и формат. Если отсутствует решающий вход и без него задача допускает разные ответы, task=null, missing — названия отсутствующих полей; не дополняй их догадкой. Самодостаточность не требует повторять разговорные фразы.

Вход D23:
{
  "full_original_prompt": "הסבר בעברית את SQL: SELECT COUNT(v) FROM T; הטבלה T(v int NULL) מכילה [NULL,0,0]. אל תשנה את השאילתה; החזר הסבר בלבד.",
  "selected_segment": {
    "id": "s1",
    "text": "הסבר בעברית את SQL: SELECT COUNT(v) FROM T; הטבלה T(v int NULL) מכילה [NULL,0,0]. אל תשנה את השאילתה; החזר הסבר בלבד.",
    "domain_id": "docs.sql",
    "depends_on": []
  },
  "assigned_domain_id": "docs.sql",
  "available_prior_results": {}
}
```

**Chosen**

```text
{"domain_id":"docs.sql","task":"הסבר בעברית את SELECT COUNT(v) FROM T עבור T(v int NULL) עם [NULL,0,0]. אל תשנה SQL; החזר הסבר בלבד.","missing":[]}
```

**Rejected**

```text
{"domain_id":"docs.sql","task":"הסבר בעברית את SELECT COUNT(*) FROM T עבור T(v int NULL) עם [NULL,0,0]. אל תשנה SQL; החזר הסבר בלבד.","missing":[]}
```

**Simple check (English):** Preserve the original SQL expression as well as the requested Hebrew explanation. COUNT(v) and COUNT(*) differ for the visible NULL row.

**Контрпример:** Язык входа не меняет docs.sql на другой домен; замена v на * меняет объясняемый запрос.

## Граница доказательств

Образцы созданы как авторские симуляции по уже включённым проектным источникам. Они не скопированы из стороннего корпуса и не получают выдуманную MIT/Apache-лицензию. Ссылки объясняют методику и контракт; они не утверждают, что именно эти новые примеры раньше успешно обучили модель.

Исторические аудиты описывают своё зафиксированное состояние. Текущие Git LFS pointers без содержимого недостаточны для вывода о смысле строк. Настоящий аудит новых committed рядов должен ссылаться на прочитанные тела строк, их IDs и конкретные ошибки; отсутствие доступного тела означает needs_assessment, а не needs_rework или accepted.

Никакие старые пары, requests, отчёты, карточки или IDs этим приложением не удаляются. Исправления существующего кандидата создаются по явному назначению новой версии, с сохранением происхождения; открытый пример не подставляется вместо самостоятельной генерации.
