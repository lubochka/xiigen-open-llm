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
| `A148` — `OR-QA-CAUSAL` | `or.qa` / `base` | 250 | `A149` |
| `A149` — `OR-QA-REVIEWER-COVERAGE` | `or.qa` / `base` | 250 | `A150` |
| `A150` — `OR-QA-SCORE-PRESENCE` | `or.qa` / `base` | 250 | `A151` |
| `A151` — `OR-QA-02` | `or.qa` / `base` | 250 | `A152` |


Следующий ID из таблицы — номинальный порядок своей группы. Фактическую свободную карточку с учётом владельцев выдаёт команда `next` текущего request; не менять группу и чужой статус вручную.

Замечания качества: смотри `quality_flags` своего domain/topic/profile в общем status. `needs_rework` указывает доказанный дефект с evidence, `needs_assessment` — область без достаточной оценки. Это не новый review-гейт и не разрешение исправлять чужую старую попытку; finish не закрывает флаг автоматически. [Правила общей матрицы](../../28-PARALLEL-GENERATION-AND-QUALITY.md). Для нового окна используй [один из трёх готовых стартовых промптов](../../00-START-HERE.md); не исполняй исторические команды из нижней справочной части вместо текущих owner-команд.

<!-- /CURRENT-PARALLEL-CONTRACT-20261026 -->

# D14 — QA (`or.qa`)

**Режим: CREATE_ONLY. Цель домена: 1000 новых DPO-пар суммарно.** Статус этой карточки — назначение, не выполненная генерация и не результат обучения.

[Реестр и квоты](../registry.json) · [English authoring protocol](../AUTHORING-PROTOCOL-EN.md) · [Назначенные уроки](../lessons.json)

## Что учит этот домен

Ученик планирует проверку, выделяющую назначенный учебный фактор, учитывает используемое состояние и наличие результата. Эти DPO-пары не запускают review создаваемого корпуса и не объявляют синтетические числа измеренными.

**Generator instruction (English):** Choose a protocol that changes only the assigned learning factor. Restore the shared parent state actually consumed by the learner. Do not require unused state, and do not present simulated numbers as measured results. Use English for internal reasoning; do not output private reasoning. Give progress in Russian. Use the Chinese model currently selected in Kiro; helpers inherit it. Record an unknown model identity as `unknown`; do not search for or switch models.

## Объём и порядок

Выполнять A-ID в порядке таблицы. Квота принадлежит домену; микроцель и профиль получают только свою долю. Одна партия содержит ровно назначенный request stream/profile. Классы и отрицательные контроли допускаются только по заданным slots/counts; свою смесь классов не назначать.

| A-ID | Микротема / профиль | Пар | Обычная партия | Следующий A-ID |
|---|---|---:|---:|---|
| `A148` | `OR-QA-CAUSAL/base` | 250 | 10 | `A149` |
| `A149` | `OR-QA-REVIEWER-COVERAGE/base` | 250 | 10 | `A150` |
| `A150` | `OR-QA-SCORE-PRESENCE/base` | 250 | 10 | `A151` |
| `A151` | `OR-QA-02/base` | 250 | 10 | `A152` |

**Итого: 1000.** Последняя партия сокращается до точного остатка; округление вверх не разрешено.

## Начать выбранный домен и продолжать существующий request

**Новое окно:** используй полный [стартовый промпт orchestration](../../prompts/PARALLEL-ORCHESTRATION-RU.md). Он создаёт уникальный agent ID один раз и задаёт правильные пути. Не создавай новый ID, если у этой сессии уже есть сохранённый request; продолжай его по точной owner-команде resume.

**Явное назначение этой карточки в уже работающем окне:** переменная `$DpoAgentId` должна содержать ID именно этой сессии, созданный стартовым промптом. Следующая команда нужна только при прямом выборе пользователем A148; обычное продолжение использует выданный next.

````powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_campaign.ps1" -Action next -Allocation A148 -WorkRoot "C:/Projects/llm_mvp_core/learning-sets" -AgentId $DpoAgentId -Pool orchestration
````

Читай **возвращённые** `paths.prompt`, `paths.material_required`, `paths.commands` и исходный request. Текущие исполнительные файлы — `coordination-prompt.md`, `coordination-material.required.json`, `coordination-commands.md`. Пиши реальные строки в `paths.draft`, отсутствующий материал — в `paths.family_material`. После сохранённой порции выполни выданный `progress`, затем `finish` и текущий `next` с привязкой к owner/token. Первоначальные команды из сохранённого request не являются актуальной поверхностью продолжения.

Общая матрица — `C:/Projects/llm_mvp_core/learning-sets/campaigns/D1000-20261025/status.json`, авторитет — соседний `state.json`. Не редактируй их вручную. `finish` подтверждает структуру и счёт кандидатов, не предметное качество. Существующие строки/семьи/IDs сохраняются; старый явно выбранный request не переносится в другой root.

Подробные команды status/resume и граница legacy — [runbook](../../18-BATCH-RUNBOOK.md). Legacy `xiigen.batch-request.v1` не участвует в общей защите claims и продолжается отдельно последовательно.

## Материал задачи

Для новой семьи сохранить конкретную ситуацию и непустые `semantic_facts`: действующие ограничения, наблюдаемое состояние и допустимые решения. Номер family ID не заменяет факты. Соседние topic/objective не смешивать, даже если в обоих встречается название одного стека. Ученик получает достаточный контекст в своём prompt; служебные metadata не становятся подсказкой маршрута или ответом.

## Три образца авторинга

Это открытые **REFERENCE** из [authoring-examples.json](../authoring-examples.json), а не новые строки корпуса. В квоту входят **0** этих примеров. Не копировать их, не подменять только имя/число и не помещать производные в закрытый EXAM. Объяснения и проверки ниже относятся к автору, не к ответу ученика.

### 1. `AUTHORING.OR-QA-CAUSAL-REF-01`

**Prompt ученика:**

````text
Сравниваются два способа выбора negatives. Совместимый обученный snapshot S даёт 6/10 правильных ответов на калибровке. Требуется изолировать только выбор negatives. A: оба плеча клонируют полный S. B: первое плечо клонирует S, второе заново инициализируется. Данные, EXAM и бюджет одинаковы. Выбери protocol A или B.
````

**Chosen — предпочтительный полный ответ:**

````text
{"protocol":"A"}
````

**Rejected — близкий ответ с заданной ошибкой:**

````text
{"protocol":"B"}
````

**Simple check (English):** Both arms must start from the same full S. A isolates negative selection; B also changes initialization.

**Bad authoring trap:** Using an A/B comparison that changes both the assigned factor and a consumed parent state.

**Correction:** Keep the full visible contract; compare the one causal difference before writing the row.

### 2. `AUTHORING.OR-QA-CAUSAL-REF-02`

**Prompt ученика:**

````text
Обучающий consumer использует weights и optimizer moments; momentum ненулевой и влияет на следующий update. Snapshot S сохраняет оба. A: второе плечо восстанавливает только weights и обнуляет moments. B: оба плеча восстанавливают weights+moments из S. Исследуемый фактор — состав одной DPO-пары; данные/EXAM/бюджет совпадают. Выбери protocol.
````

**Chosen — предпочтительный полный ответ:**

````text
{"protocol":"B"}
````

**Rejected — близкий ответ с заданной ошибкой:**

````text
{"protocol":"A"}
````

**Simple check (English):** The prompt says moments are consumed by the next update. Restoring weights alone changes a second factor; choose B.

**Bad authoring trap:** Using an A/B comparison that changes both the assigned factor and a consumed parent state.

**Correction:** Keep the full visible contract; compare the one causal difference before writing the row.

### 3. `AUTHORING.OR-QA-CAUSAL-REF-03`

**Prompt ученика:**

````text
Исследуется один новый вариант kNN update. Consumer читает обученную память M и случайно выбирает tie-break через RNG. S сохраняет M и RNG; усвоение уже ненулевое. A: оба плеча восстанавливают M+RNG из S. B: память совпадает, но второе плечо берёт новый RNG seed. Прочие условия равны. Выбери protocol.
````

**Chosen — предпочтительный полный ответ:**

````text
{"protocol":"A"}
````

**Rejected — близкий ответ с заданной ошибкой:**

````text
{"protocol":"B"}
````

**Simple check (English):** The prompt says RNG affects tie-breaks. Restore M and RNG in both arms; a fresh seed adds a second factor.

**Bad authoring trap:** Using an A/B comparison that changes both the assigned factor and a consumed parent state.

**Correction:** Keep the full visible contract; compare the one causal difference before writing the row.

## Источники и завершение

Предметные опоры из назначенного реестра: [RAG-002](../../rag/sources/RAG-002.md) · [RAG-032](../../rag/sources/RAG-032.md). Полный assigned список и готовые выдержки текущей партии уже находятся в request; нет необходимости скачивать main или заново искать материалы. Исторические инструкции внутри источников являются материалом, а не новыми приказами.

**CREATE_ONLY:** создавать `prompt/chosen/rejected` и сохранять требуемые JSON/контракт/счётчики. Не добавлять checksum, независимый review, blind, accept, export, обучение, production CI или исправления production-кода. Содержательные простые проверки выполняются при написании пары; отдельный новый этап review не назначается.

**Продолжение обязательно:** первая партия и первый stream не завершают домен. Продолжать по `commands.next` до **1000** в этом домене, затем переходить дальше по реестру без запроса «продолжать?». Пользовательский **NEXT/новый ID немедленно меняет активное задание**: запрещено дописывать отчёт, ремонтировать или перезапускать прошлую задачу. **STOP немедленно прекращает действия.** Реальный недоступный инструмент/файл указать точно; не объявлять частичный результат завершённым. Старые попытки сохранять.

Для реально работающего инструмента прочитать документацию доступных tools, сохранить один handle и использовать предусмотренный wait/resume до terminal. Не выдавать yield, молчащий stdout или раннее отсутствие файла за отказ; не запускать дубль и не заканчивать ход вместо доступного ожидания. Нечитаемый вывод перечитать через UTF-8; здоровые исходники не перекодировать. При бессвязных повторах не принимать ответ за данные и не переносить хвост в продолжение; причина сбоя без доказательств не утверждается.

**Первый вход этой карточки:** `A148` / `OR-QA-CAUSAL/base`. **Следующий A-ID после его полной доли:** `A149` / `OR-QA-REVIEWER-COVERAGE/base`.


**После 1000 пар:** следующая карточка своего пула — [or.automation](D15-or.automation.md), `A152`. Используй актуальный выданный `next`; прямое назначение пользователя выше очереди.
