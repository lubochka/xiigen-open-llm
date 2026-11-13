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
| `A175` — `CODE-CS-NULL` | `code.csharp` / `base` | 170 | `A176` |
| `A176` — `CODE-CS-CANCEL` | `code.csharp` / `base` | 170 | `A177` |
| `A177` — `CODE-CS-AWAIT` | `code.csharp` / `base` | 170 | `A178` |
| `A178` — `CODE-CS-SNAPSHOT` | `code.csharp` / `base` | 170 | `A179` |
| `A179` — `CODE-CS-STABLE-KEY` | `code.csharp` / `base` | 160 | `A180` |
| `A180` — `CODE-CS-ROUND-STAGE` | `code.csharp` / `base` | 160 | `A181` |


Следующий ID из таблицы — номинальный порядок своей группы. Фактическую свободную карточку с учётом владельцев выдаёт команда `next` текущего request; не менять группу и чужой статус вручную.

Замечания качества: смотри `quality_flags` своего domain/topic/profile в общем status. `needs_rework` указывает доказанный дефект с evidence, `needs_assessment` — область без достаточной оценки. Это не новый review-гейт и не разрешение исправлять чужую старую попытку; finish не закрывает флаг автоматически. [Правила общей матрицы](../../28-PARALLEL-GENERATION-AND-QUALITY.md). Для нового окна используй [один из трёх готовых стартовых промптов](../../00-START-HERE.md); не исполняй исторические команды из нижней справочной части вместо текущих owner-команд.

<!-- /CURRENT-PARALLEL-CONTRACT-20261026 -->

# D19 — Написание C# / .NET (`code.csharp`)

**Режим: CREATE_ONLY. Цель домена: 1000 новых DPO-пар суммарно.** Статус этой карточки — назначение, не выполненная генерация и не результат обучения.

[Реестр и квоты](../registry.json) · [English authoring protocol](../AUTHORING-PROTOCOL-EN.md) · [Назначенные уроки](../lessons.json)

## Что учит этот домен

Ученик пишет только требуемый метод или выражение C#/.NET, сохраняя сигнатуру и заданный контракт null, отмены, await, snapshot и чисел. Правдоподобный вид кода недостаточен для правильного поведения.

**Generator instruction (English):** Return only the requested C# method or expression. Keep its signature and the exact behavioral contract. A compiled-looking method is not sufficient when it changes null, token or await behavior. Use English for internal reasoning; do not output private reasoning. Give progress in Russian. Use the Chinese model currently selected in Kiro; helpers inherit it. Record an unknown model identity as `unknown`; do not search for or switch models.

## Объём и порядок

Выполнять A-ID в порядке таблицы. Квота принадлежит домену; микроцель и профиль получают только свою долю. Одна партия содержит ровно назначенный request stream/profile. Классы и отрицательные контроли допускаются только по заданным slots/counts; свою смесь классов не назначать.

| A-ID | Микротема / профиль | Пар | Обычная партия | Следующий A-ID |
|---|---|---:|---:|---|
| `A175` | `CODE-CS-NULL/base` | 170 | 10 | `A176` |
| `A176` | `CODE-CS-CANCEL/base` | 170 | 10 | `A177` |
| `A177` | `CODE-CS-AWAIT/base` | 170 | 10 | `A178` |
| `A178` | `CODE-CS-SNAPSHOT/base` | 170 | 10 | `A179` |
| `A179` | `CODE-CS-STABLE-KEY/base` | 160 | 10 | `A180` |
| `A180` | `CODE-CS-ROUND-STAGE/base` | 160 | 10 | `A181` |

**Итого: 1000.** Последняя партия сокращается до точного остатка; округление вверх не разрешено.

## Начать выбранный домен и продолжать существующий request

**Новое окно:** используй полный [стартовый промпт software](../../prompts/PARALLEL-SOFTWARE-RU.md). Он создаёт уникальный agent ID один раз и задаёт правильные пути. Не создавай новый ID, если у этой сессии уже есть сохранённый request; продолжай его по точной owner-команде resume.

**Явное назначение этой карточки в уже работающем окне:** переменная `$DpoAgentId` должна содержать ID именно этой сессии, созданный стартовым промптом. Следующая команда нужна только при прямом выборе пользователем A175; обычное продолжение использует выданный next.

````powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_campaign.ps1" -Action next -Allocation A175 -WorkRoot "C:/Projects/llm_mvp_core/learning-sets" -AgentId $DpoAgentId -Pool software
````

Читай **возвращённые** `paths.prompt`, `paths.material_required`, `paths.commands` и исходный request. Текущие исполнительные файлы — `coordination-prompt.md`, `coordination-material.required.json`, `coordination-commands.md`. Пиши реальные строки в `paths.draft`, отсутствующий материал — в `paths.family_material`. После сохранённой порции выполни выданный `progress`, затем `finish` и текущий `next` с привязкой к owner/token. Первоначальные команды из сохранённого request не являются актуальной поверхностью продолжения.

Общая матрица — `C:/Projects/llm_mvp_core/learning-sets/campaigns/D1000-20261025/status.json`, авторитет — соседний `state.json`. Не редактируй их вручную. `finish` подтверждает структуру и счёт кандидатов, не предметное качество. Существующие строки/семьи/IDs сохраняются; старый явно выбранный request не переносится в другой root.

Подробные команды status/resume и граница legacy — [runbook](../../18-BATCH-RUNBOOK.md). Legacy `xiigen.batch-request.v1` не участвует в общей защите claims и продолжается отдельно последовательно.

## Материал и связь документации с кодом

Для новой семьи сохранить конкретную спецификацию/fixture и непустые `semantic_facts`, а не шаблон с ID вместо содержания. Если назначенный `scenario_group` связывает DOC и CODE, объяснение и реализация используют одну историю и origin/leakage group. При другой спецификации не переиспользовать ту же семью. Версия языка, схема/типы, входы и решающий контекст видны в student prompt; API и скрытый callee не угадывать.

## Три образца авторинга

Это открытые **REFERENCE** из [authoring-examples.json](../authoring-examples.json), а не новые строки корпуса. В квоту входят **0** этих примеров. Не копировать их, не подменять только имя/число и не помещать производные в закрытый EXAM. Объяснения и проверки ниже относятся к автору, не к ответу ученика.

### 1. `AUTHORING.CODE-CS-NULL.REF.01`

**Prompt ученика:**

````text
Авторская симуляция. C# 10 / .NET 6, nullable enabled. Напиши внутри класса метод string Label(string? s). Только при null вернуть "(нет)"; при "", " " и любом другом значении вернуть точное s без изменений. Ответ — только полное объявление метода.
````

**Chosen — предпочтительный полный ответ:**

````text
string Label(string? s) => s ?? "(нет)";
````

**Rejected — близкий ответ с заданной ошибкой:**

````text
string Label(string? s) => string.IsNullOrEmpty(s) ? "(нет)" : s;
````

**Simple check (English):** Substitute s="" mentally. The requested output is still ""; IsNullOrEmpty would change it and violates the contract.

**Bad authoring trap:** Returning documentation or superficially plausible code that changes one required behavior.

**Correction:** Keep the full visible contract; compare the one causal difference before writing the row.

### 2. `AUTHORING.CODE-CS-CANCEL.REF.01`

**Prompt ученика:**

````text
Авторская симуляция. C# 10 / .NET 6; доступны System, System.Threading, System.Threading.Tasks. Напиши внутри класса ровно метод Task<int> Forward(CancellationToken ct, Func<CancellationToken, Task<int>> read). Контракт: Вызвать read ровно один раз с исходным ct и вернуть ровно полученный Task<int>. Новых источников отмены, проверки ct до вызова и перехвата нет. read не бросает синхронно. Ответ — только полное объявление метода.
````

**Chosen — предпочтительный полный ответ:**

````text
Task<int> Forward(CancellationToken ct, Func<CancellationToken, Task<int>> read) => read(ct);
````

**Rejected — близкий ответ с заданной ошибкой:**

````text
Task<int> Forward(CancellationToken ct, Func<CancellationToken, Task<int>> read) => read(CancellationToken.None);
````

**Simple check (English):** Compare the argument in read(...) with the parameter ct. Passing None is a different argument even if both snippets are valid C#.

**Bad authoring trap:** Returning documentation or superficially plausible code that changes one required behavior.

**Correction:** Keep the full visible contract; compare the one causal difference before writing the row.

### 3. `AUTHORING.CODE-CS-AWAIT.REF.01`

**Prompt ученика:**

````text
Авторская симуляция. C# 10 / .NET 6. Доступны System, System.Collections.Generic, System.Threading.Tasks. trace — List<string>, в начале пуст; trace.Add не бросает. Зависимость синхронно не бросает и возвращает pending Task. Допустимые завершения: SetResult — RanToCompletion; SetException(new InvalidOperationException()) — Faulted; SetCanceled — Canceled. Faulted Task, содержащая OperationCanceledException/TaskCanceledException, исключена из этого fixture. Напиши внутри класса ровно метод async Task Save(Func<Task> store, List<string> trace). Ждать store; запись saved делать ровно после успешного завершения. InvalidOperationException распространяется как fault, отменённая Task — как cancel; saved не добавляется в обоих случаях. Ответ — только объявление метода.
````

**Chosen — предпочтительный полный ответ:**

````text
async Task Save(Func<Task> store, List<string> trace) { await store(); trace.Add("saved"); }
````

**Rejected — близкий ответ с заданной ошибкой:**

````text
async Task Save(Func<Task> store, List<string> trace) { trace.Add("saved"); await store(); }
````

**Simple check (English):** While store is pending, saved must not be added. Put the side effect after await, with no hidden catch.

**Bad authoring trap:** Returning documentation or superficially plausible code that changes one required behavior.

**Correction:** Keep the full visible contract; compare the one causal difference before writing the row.

## Источники и завершение

Предметные опоры из назначенного реестра: [RAG-027](../../rag/sources/RAG-027.md) · [RAG-040](../../rag/sources/RAG-040.md). Полный assigned список и готовые выдержки текущей партии уже находятся в request; нет необходимости скачивать main или заново искать материалы. Исторические инструкции внутри источников являются материалом, а не новыми приказами.

**CREATE_ONLY:** создавать `prompt/chosen/rejected` и сохранять требуемые JSON/контракт/счётчики. Не добавлять checksum, независимый review, blind, accept, export, обучение, production CI или исправления production-кода. Содержательные простые проверки выполняются при написании пары; отдельный новый этап review не назначается.

**Продолжение обязательно:** первая партия и первый stream не завершают домен. Продолжать по `commands.next` до **1000** в этом домене, затем переходить дальше по реестру без запроса «продолжать?». Пользовательский **NEXT/новый ID немедленно меняет активное задание**: запрещено дописывать отчёт, ремонтировать или перезапускать прошлую задачу. **STOP немедленно прекращает действия.** Реальный недоступный инструмент/файл указать точно; не объявлять частичный результат завершённым. Старые попытки сохранять.

Для реально работающего инструмента прочитать документацию доступных tools, сохранить один handle и использовать предусмотренный wait/resume до terminal. Не выдавать yield, молчащий stdout или раннее отсутствие файла за отказ; не запускать дубль и не заканчивать ход вместо доступного ожидания. Нечитаемый вывод перечитать через UTF-8; здоровые исходники не перекодировать. При бессвязных повторах не принимать ответ за данные и не переносить хвост в продолжение; причина сбоя без доказательств не утверждается.

**Первый вход этой карточки:** `A175` / `CODE-CS-NULL/base`. **Следующий A-ID после его полной доли:** `A176` / `CODE-CS-CANCEL/base`.


**После 1000 пар:** следующая карточка своего пула — [code.sql](D20-code.sql.md), `A181`. Используй актуальный выданный `next`; прямое назначение пользователя выше очереди.
