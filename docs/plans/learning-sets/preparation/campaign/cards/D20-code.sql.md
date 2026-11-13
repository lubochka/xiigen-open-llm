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
| `A181` — `CODE-SQL-COUNT` | `code.sql` / `base` | 170 | `A182` |
| `A182` — `CODE-SQL-LEFT` | `code.sql` / `base` | 170 | `A183` |
| `A183` — `CODE-SQL-GUARD` | `code.sql` / `base` | 170 | `A184` |
| `A184` — `CODE-SQL-SAVEPOINT` | `code.sql` / `base` | 170 | `A185` |
| `A185` — `CODE-SQL-ANTI-KEY` | `code.sql` / `base` | 160 | `A186` |
| `A186` — `CODE-SQL-RANK-TIE` | `code.sql` / `base` | 160 | `A187` |


Следующий ID из таблицы — номинальный порядок своей группы. Фактическую свободную карточку с учётом владельцев выдаёт команда `next` текущего request; не менять группу и чужой статус вручную.

Замечания качества: смотри `quality_flags` своего domain/topic/profile в общем status. `needs_rework` указывает доказанный дефект с evidence, `needs_assessment` — область без достаточной оценки. Это не новый review-гейт и не разрешение исправлять чужую старую попытку; finish не закрывает флаг автоматически. [Правила общей матрицы](../../28-PARALLEL-GENERATION-AND-QUALITY.md). Для нового окна используй [один из трёх готовых стартовых промптов](../../00-START-HERE.md); не исполняй исторические команды из нижней справочной части вместо текущих owner-команд.

<!-- /CURRENT-PARALLEL-CONTRACT-20261026 -->

# D20 — Написание SQL (`code.sql`)

**Режим: CREATE_ONLY. Цель домена: 1000 новых DPO-пар суммарно.** Статус этой карточки — назначение, не выполненная генерация и не результат обучения.

[Реестр и квоты](../registry.json) · [English authoring protocol](../AUTHORING-PROTOCOL-EN.md) · [Назначенные уроки](../lessons.json)

## Что учит этот домен

Ученик пишет SQL для показанной схемы, данных и диалекта. Должны сохраняться требуемые строки, дубли, условия и семантика ключей; объяснение вместо запроса не является ответом этой темы.

**Generator instruction (English):** Return only the requested SQL statement against the supplied setup. Preserve all rows, duplicates and guards required by the contract. Do not provide a prose explanation instead of SQL. Use English for internal reasoning; do not output private reasoning. Give progress in Russian. Use the Chinese model currently selected in Kiro; helpers inherit it. Record an unknown model identity as `unknown`; do not search for or switch models.

## Объём и порядок

Выполнять A-ID в порядке таблицы. Квота принадлежит домену; микроцель и профиль получают только свою долю. Одна партия содержит ровно назначенный request stream/profile. Классы и отрицательные контроли допускаются только по заданным slots/counts; свою смесь классов не назначать.

| A-ID | Микротема / профиль | Пар | Обычная партия | Следующий A-ID |
|---|---|---:|---:|---|
| `A181` | `CODE-SQL-COUNT/base` | 170 | 10 | `A182` |
| `A182` | `CODE-SQL-LEFT/base` | 170 | 10 | `A183` |
| `A183` | `CODE-SQL-GUARD/base` | 170 | 10 | `A184` |
| `A184` | `CODE-SQL-SAVEPOINT/base` | 170 | 10 | `A185` |
| `A185` | `CODE-SQL-ANTI-KEY/base` | 160 | 10 | `A186` |
| `A186` | `CODE-SQL-RANK-TIE/base` | 160 | 10 | `A187` |

**Итого: 1000.** Последняя партия сокращается до точного остатка; округление вверх не разрешено.

## Начать выбранный домен и продолжать существующий request

**Новое окно:** используй полный [стартовый промпт software](../../prompts/PARALLEL-SOFTWARE-RU.md). Он создаёт уникальный agent ID один раз и задаёт правильные пути. Не создавай новый ID, если у этой сессии уже есть сохранённый request; продолжай его по точной owner-команде resume.

**Явное назначение этой карточки в уже работающем окне:** переменная `$DpoAgentId` должна содержать ID именно этой сессии, созданный стартовым промптом. Следующая команда нужна только при прямом выборе пользователем A181; обычное продолжение использует выданный next.

````powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_campaign.ps1" -Action next -Allocation A181 -WorkRoot "C:/Projects/llm_mvp_core/learning-sets" -AgentId $DpoAgentId -Pool software
````

Читай **возвращённые** `paths.prompt`, `paths.material_required`, `paths.commands` и исходный request. Текущие исполнительные файлы — `coordination-prompt.md`, `coordination-material.required.json`, `coordination-commands.md`. Пиши реальные строки в `paths.draft`, отсутствующий материал — в `paths.family_material`. После сохранённой порции выполни выданный `progress`, затем `finish` и текущий `next` с привязкой к owner/token. Первоначальные команды из сохранённого request не являются актуальной поверхностью продолжения.

Общая матрица — `C:/Projects/llm_mvp_core/learning-sets/campaigns/D1000-20261025/status.json`, авторитет — соседний `state.json`. Не редактируй их вручную. `finish` подтверждает структуру и счёт кандидатов, не предметное качество. Существующие строки/семьи/IDs сохраняются; старый явно выбранный request не переносится в другой root.

Подробные команды status/resume и граница legacy — [runbook](../../18-BATCH-RUNBOOK.md). Legacy `xiigen.batch-request.v1` не участвует в общей защите claims и продолжается отдельно последовательно.

## Материал и связь документации с кодом

Для новой семьи сохранить конкретную спецификацию/fixture и непустые `semantic_facts`, а не шаблон с ID вместо содержания. Если назначенный `scenario_group` связывает DOC и CODE, объяснение и реализация используют одну историю и origin/leakage group. При другой спецификации не переиспользовать ту же семью. Версия языка, схема/типы, входы и решающий контекст видны в student prompt; API и скрытый callee не угадывать.

## Три образца авторинга

Это открытые **REFERENCE** из [authoring-examples.json](../authoring-examples.json), а не новые строки корпуса. В квоту входят **0** этих примеров. Не копировать их, не подменять только имя/число и не помещать производные в закрытый EXAM. Объяснения и проверки ниже относятся к автору, не к ответу ученика.

### 1. `AUTHORING.CODE-SQL-COUNT.REF.01`

**Prompt ученика:**

````text
Авторская симуляция. T-SQL / SQL Server 2019, ANSI_NULLS ON. Перед возвращённым SELECT в этом же одном T-SQL batch на той же connection выполняется setup:
DECLARE @T TABLE (v int NULL); INSERT INTO @T(v) VALUES (7),(NULL),(7);
Напиши один SELECT: n — число строк, в которых v не NULL; каждое повторение учитывается, числовой 0 учитывается. Должна быть ровно одна итоговая строка, в том числе n=0 для пустого набора. Ответ — только SELECT.
````

**Chosen — предпочтительный полный ответ:**

````text
SELECT COUNT(v) AS n FROM @T;
````

**Rejected — близкий ответ с заданной ошибкой:**

````text
SELECT COUNT(DISTINCT v) AS n FROM @T;
````

**Simple check (English):** Count the two non-NULL occurrences of 7 separately. Do not add DISTINCT when duplicates must count.

**Bad authoring trap:** Returning an explanation or a SQL statement with one extra semantic change.

**Correction:** Keep the full visible contract; compare the one causal difference before writing the row.

### 2. `AUTHORING.CODE-SQL-LEFT.REF.01`

**Prompt ученика:**

````text
Авторская симуляция. T-SQL / SQL Server 2019; ANSI_NULLS ON. Полный setup: CREATE TABLE #P (Id int NOT NULL PRIMARY KEY); CREATE TABLE #R (Id int NOT NULL PRIMARY KEY, ParentId int NOT NULL, Active bit NULL); INSERT #P VALUES (1),(2); INSERT #R VALUES (10,1,1),(11,1,0); Напиши ровно SELECT. Вернуть каждую строку #P, а из #R только Active=1; если подходящих #R нет, RightId=null. Поля Id,RightId; ORDER BY p.Id,r.Id. Ответ — SQL без Markdown; setup не повторять.
````

**Chosen — предпочтительный полный ответ:**

````text
SELECT p.Id, r.Id AS RightId FROM #P p LEFT JOIN #R r ON r.ParentId=p.Id AND r.Active=1 ORDER BY p.Id,r.Id;
````

**Rejected — близкий ответ с заданной ошибкой:**

````text
SELECT p.Id, r.Id AS RightId FROM #P p LEFT JOIN #R r ON r.ParentId=p.Id WHERE r.Active=1 ORDER BY p.Id,r.Id;
````

**Simple check (English):** Keep Active=1 in the matching condition. Putting it in WHERE would discard the NULL-extended row for parent 2.

**Bad authoring trap:** Returning an explanation or a SQL statement with one extra semantic change.

**Correction:** Keep the full visible contract; compare the one causal difference before writing the row.

### 3. `AUTHORING.CODE-SQL-GUARD.REF.01.v2`

**Prompt ученика:**

````text
Авторская симуляция. T-SQL / SQL Server 2019; ANSI_NULLS ON; триггеров и CHECK нет, конкурентных соединений в этом fixture нет. Setup: CREATE TABLE #Stock (Id int NOT NULL PRIMARY KEY, Available int NOT NULL); INSERT #Stock VALUES (1,2),(2,10); DECLARE @id int=1,@qty int=3; Напиши ровно один UPDATE без setup. Изменить только строку @id и только если Available>=@qty. @qty положительный и проверен вызывающим кодом. Не менять соседнюю строку. При допуске уменьшить Available ровно на @qty; другие поля не менять. Ответ — только SQL.
````

**Chosen — предпочтительный полный ответ:**

````text
UPDATE #Stock SET Available=Available-@qty WHERE Id=@id AND Available>=@qty;
````

**Rejected — близкий ответ с заданной ошибкой:**

````text
UPDATE #Stock SET Available=Available-@qty WHERE Id=@id AND Available>0;
````

**Simple check (English):** Try Available=2 and qty=3. The guard must reject the update; Available>0 is too weak.

**Bad authoring trap:** Returning an explanation or a SQL statement with one extra semantic change.

**Correction:** Keep the full visible contract; compare the one causal difference before writing the row.

## Источники и завершение

Предметные опоры из назначенного реестра: [RAG-040](../../rag/sources/RAG-040.md) · [RAG-041](../../rag/sources/RAG-041.md). Полный assigned список и готовые выдержки текущей партии уже находятся в request; нет необходимости скачивать main или заново искать материалы. Исторические инструкции внутри источников являются материалом, а не новыми приказами.

**CREATE_ONLY:** создавать `prompt/chosen/rejected` и сохранять требуемые JSON/контракт/счётчики. Не добавлять checksum, независимый review, blind, accept, export, обучение, production CI или исправления production-кода. Содержательные простые проверки выполняются при написании пары; отдельный новый этап review не назначается.

**Продолжение обязательно:** первая партия и первый stream не завершают домен. Продолжать по `commands.next` до **1000** в этом домене, затем переходить дальше по реестру без запроса «продолжать?». Пользовательский **NEXT/новый ID немедленно меняет активное задание**: запрещено дописывать отчёт, ремонтировать или перезапускать прошлую задачу. **STOP немедленно прекращает действия.** Реальный недоступный инструмент/файл указать точно; не объявлять частичный результат завершённым. Старые попытки сохранять.

Для реально работающего инструмента прочитать документацию доступных tools, сохранить один handle и использовать предусмотренный wait/resume до terminal. Не выдавать yield, молчащий stdout или раннее отсутствие файла за отказ; не запускать дубль и не заканчивать ход вместо доступного ожидания. Нечитаемый вывод перечитать через UTF-8; здоровые исходники не перекодировать. При бессвязных повторах не принимать ответ за данные и не переносить хвост в продолжение; причина сбоя без доказательств не утверждается.

**Первый вход этой карточки:** `A181` / `CODE-SQL-COUNT/base`. **Следующий A-ID после его полной доли:** `A182` / `CODE-SQL-LEFT/base`.


**После 1000 пар:** следующая карточка своего пула — [code.angular](D21-code.angular.md), `A187`. Используй актуальный выданный `next`; прямое назначение пользователя выше очереди.
