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
| `A135` — `OR-DB-ATOMIC-CLAIM` | `or.database` / `base` | 340 | `A136` |
| `A136` — `OR-DB-CAPACITY-ATOMIC` | `or.database` / `base` | 330 | `A137` |
| `A137` — `OR-DB-02` | `or.database` / `base` | 330 | `A138` |


Следующий ID из таблицы — номинальный порядок своей группы. Фактическую свободную карточку с учётом владельцев выдаёт команда `next` текущего request; не менять группу и чужой статус вручную.

Замечания качества: смотри `quality_flags` своего domain/topic/profile в общем status. `needs_rework` указывает доказанный дефект с evidence, `needs_assessment` — область без достаточной оценки. Это не новый review-гейт и не разрешение исправлять чужую старую попытку; finish не закрывает флаг автоматически. [Правила общей матрицы](../../28-PARALLEL-GENERATION-AND-QUALITY.md). Для нового окна используй [один из трёх готовых стартовых промптов](../../00-START-HERE.md); не исполняй исторические команды из нижней справочной части вместо текущих owner-команд.

<!-- /CURRENT-PARALLEL-CONTRACT-20261026 -->

# D08 — Оркестрация изменений базы данных (`or.database`)

**Режим: CREATE_ONLY. Цель домена: 1000 новых DPO-пар суммарно.** Статус этой карточки — назначение, не выполненная генерация и не результат обучения.

[Реестр и квоты](../registry.json) · [English authoring protocol](../AUTHORING-PROTOCOL-EN.md) · [Назначенные уроки](../lessons.json)

## Что учит этот домен

Ученик выбирает действие координации базы данных по заданной атомарности, вместимости и области ключа идемпотентности. Нельзя предполагать скрытую транзакцию или ограничение уникальности.

**Generator instruction (English):** Choose the next database coordination action under the explicit atomicity, ownership and idempotency contract. Do not assume an unmentioned transaction or uniqueness constraint. Use English for internal reasoning; do not output private reasoning. Give progress in Russian. Use the Chinese model currently selected in Kiro; helpers inherit it. Record an unknown model identity as `unknown`; do not search for or switch models.

## Объём и порядок

Выполнять A-ID в порядке таблицы. Квота принадлежит домену; микроцель и профиль получают только свою долю. Одна партия содержит ровно назначенный request stream/profile. Классы и отрицательные контроли допускаются только по заданным slots/counts; свою смесь классов не назначать.

| A-ID | Микротема / профиль | Пар | Обычная партия | Следующий A-ID |
|---|---|---:|---:|---|
| `A135` | `OR-DB-ATOMIC-CLAIM/base` | 340 | 10 | `A136` |
| `A136` | `OR-DB-CAPACITY-ATOMIC/base` | 330 | 10 | `A137` |
| `A137` | `OR-DB-02/base` | 330 | 10 | `A138` |

**Итого: 1000.** Последняя партия сокращается до точного остатка; округление вверх не разрешено.

## Начать выбранный домен и продолжать существующий request

**Новое окно:** используй полный [стартовый промпт orchestration](../../prompts/PARALLEL-ORCHESTRATION-RU.md). Он создаёт уникальный agent ID один раз и задаёт правильные пути. Не создавай новый ID, если у этой сессии уже есть сохранённый request; продолжай его по точной owner-команде resume.

**Явное назначение этой карточки в уже работающем окне:** переменная `$DpoAgentId` должна содержать ID именно этой сессии, созданный стартовым промптом. Следующая команда нужна только при прямом выборе пользователем A135; обычное продолжение использует выданный next.

````powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_campaign.ps1" -Action next -Allocation A135 -WorkRoot "C:/Projects/llm_mvp_core/learning-sets" -AgentId $DpoAgentId -Pool orchestration
````

Читай **возвращённые** `paths.prompt`, `paths.material_required`, `paths.commands` и исходный request. Текущие исполнительные файлы — `coordination-prompt.md`, `coordination-material.required.json`, `coordination-commands.md`. Пиши реальные строки в `paths.draft`, отсутствующий материал — в `paths.family_material`. После сохранённой порции выполни выданный `progress`, затем `finish` и текущий `next` с привязкой к owner/token. Первоначальные команды из сохранённого request не являются актуальной поверхностью продолжения.

Общая матрица — `C:/Projects/llm_mvp_core/learning-sets/campaigns/D1000-20261025/status.json`, авторитет — соседний `state.json`. Не редактируй их вручную. `finish` подтверждает структуру и счёт кандидатов, не предметное качество. Существующие строки/семьи/IDs сохраняются; старый явно выбранный request не переносится в другой root.

Подробные команды status/resume и граница legacy — [runbook](../../18-BATCH-RUNBOOK.md). Legacy `xiigen.batch-request.v1` не участвует в общей защите claims и продолжается отдельно последовательно.

## Материал задачи

Для новой семьи сохранить конкретную ситуацию и непустые `semantic_facts`: действующие ограничения, наблюдаемое состояние и допустимые решения. Номер family ID не заменяет факты. Соседние topic/objective не смешивать, даже если в обоих встречается название одного стека. Ученик получает достаточный контекст в своём prompt; служебные metadata не становятся подсказкой маршрута или ответом.

## Три образца авторинга

Это открытые **REFERENCE** из [authoring-examples.json](../authoring-examples.json), а не новые строки корпуса. В квоту входят **0** этих примеров. Не копировать их, не подменять только имя/число и не помещать производные в закрытый EXAM. Объяснения и проверки ниже относятся к автору, не к ответу ученика.

### 1. `AUTHORING.OR-DB-ATOMIC-CLAIM-REF-01`

**Prompt ученика:**

````text
Симуляция БД. Два worker одновременно обрабатывают ключ K. Гарантия: бизнес-запись разрешена только единственному владельцу K. В таблице ключ отсутствует. claim_unique делает атомарный INSERT с уникальностью K и возвращает выигрыш только одному; read_then_insert — раздельные чтение и запись без ограничения уникальности. Выбери следующий шаг JSON next_action из claim_unique, read_then_insert.
````

**Chosen — предпочтительный полный ответ:**

````text
{"next_action":"claim_unique"}
````

**Rejected — близкий ответ с заданной ошибкой:**

````text
{"next_action":"read_then_insert"}
````

**Simple check (English):** Consider both workers reading before either writes. Only the stated atomic unique claim permits a single winner.

**Bad authoring trap:** Assuming atomicity, ownership or a new operation identity that the prompt does not grant.

**Correction:** Keep the full visible contract; compare the one causal difference before writing the row.

### 2. `AUTHORING.OR-DB-ATOMIC-CLAIM-REF-02`

**Prompt ученика:**

````text
Симуляция БД. Для K хранится terminal=done и результат R; payload_hash совпадает с повторным запросом. Контракт повторной операции: вернуть R без новой бизнес-записи. reuse_result возвращает R; claim_new_id создаёт новую операцию с тем же payload. JSON next_action: reuse_result или claim_new_id.
````

**Chosen — предпочтительный полный ответ:**

````text
{"next_action":"reuse_result"}
````

**Rejected — близкий ответ с заданной ошибкой:**

````text
{"next_action":"claim_new_id"}
````

**Simple check (English):** Match the key and payload hash and read done. Reuse R; a new ID would duplicate the same logical effect.

**Bad authoring trap:** Assuming atomicity, ownership or a new operation identity that the prompt does not grant.

**Correction:** Keep the full visible contract; compare the one causal difference before writing the row.

### 3. `AUTHORING.OR-DB-ATOMIC-CLAIM-REF-03`

**Prompt ученика:**

````text
Симуляция БД. K находится в processing, owner=W1, lease действителен. Запрос обрабатывает W2. По контракту только owner с действующим lease может менять бизнес-запись; W2 должен подписаться на завершение K. wait_existing делает это; apply_business пишет данные сейчас. Верни JSON next_action.
````

**Chosen — предпочтительный полный ответ:**

````text
{"next_action":"wait_existing"}
````

**Rejected — близкий ответ с заданной ошибкой:**

````text
{"next_action":"apply_business"}
````

**Simple check (English):** Read owner=W1 and a valid lease. W2 cannot write yet, so wait for the existing operation.

**Bad authoring trap:** Assuming atomicity, ownership or a new operation identity that the prompt does not grant.

**Correction:** Keep the full visible contract; compare the one causal difference before writing the row.

## Источники и завершение

Предметные опоры из назначенного реестра: [RAG-DC-004](../../rag/sources/RAG-DC-004.md) · [RAG-006](../../rag/sources/RAG-006.md). Полный assigned список и готовые выдержки текущей партии уже находятся в request; нет необходимости скачивать main или заново искать материалы. Исторические инструкции внутри источников являются материалом, а не новыми приказами.

**CREATE_ONLY:** создавать `prompt/chosen/rejected` и сохранять требуемые JSON/контракт/счётчики. Не добавлять checksum, независимый review, blind, accept, export, обучение, production CI или исправления production-кода. Содержательные простые проверки выполняются при написании пары; отдельный новый этап review не назначается.

**Продолжение обязательно:** первая партия и первый stream не завершают домен. Продолжать по `commands.next` до **1000** в этом домене, затем переходить дальше по реестру без запроса «продолжать?». Пользовательский **NEXT/новый ID немедленно меняет активное задание**: запрещено дописывать отчёт, ремонтировать или перезапускать прошлую задачу. **STOP немедленно прекращает действия.** Реальный недоступный инструмент/файл указать точно; не объявлять частичный результат завершённым. Старые попытки сохранять.

Для реально работающего инструмента прочитать документацию доступных tools, сохранить один handle и использовать предусмотренный wait/resume до terminal. Не выдавать yield, молчащий stdout или раннее отсутствие файла за отказ; не запускать дубль и не заканчивать ход вместо доступного ожидания. Нечитаемый вывод перечитать через UTF-8; здоровые исходники не перекодировать. При бессвязных повторах не принимать ответ за данные и не переносить хвост в продолжение; причина сбоя без доказательств не утверждается.

**Первый вход этой карточки:** `A135` / `OR-DB-ATOMIC-CLAIM/base`. **Следующий A-ID после его полной доли:** `A136` / `OR-DB-CAPACITY-ATOMIC/base`.


**После 1000 пар:** следующая карточка своего пула — [or.client](D09-or.client.md), `A138`. Используй актуальный выданный `next`; прямое назначение пользователя выше очереди.
