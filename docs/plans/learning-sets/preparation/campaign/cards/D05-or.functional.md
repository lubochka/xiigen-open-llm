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
| `A128` — `OR-FUNC-ACCEPTANCE` | `or.functional` / `base` | 500 | `A129` |
| `A129` — `OR-FUNC-02` | `or.functional` / `base` | 500 | `A130` |


Следующий ID из таблицы — номинальный порядок своей группы. Фактическую свободную карточку с учётом владельцев выдаёт команда `next` текущего request; не менять группу и чужой статус вручную.

Замечания качества: смотри `quality_flags` своего domain/topic/profile в общем status. `needs_rework` указывает доказанный дефект с evidence, `needs_assessment` — область без достаточной оценки. Это не новый review-гейт и не разрешение исправлять чужую старую попытку; finish не закрывает флаг автоматически. [Правила общей матрицы](../../28-PARALLEL-GENERATION-AND-QUALITY.md). Для нового окна используй [один из трёх готовых стартовых промптов](../../00-START-HERE.md); не исполняй исторические команды из нижней справочной части вместо текущих owner-команд.

<!-- /CURRENT-PARALLEL-CONTRACT-20261026 -->

# D05 — Функциональное описание (`or.functional`)

**Режим: CREATE_ONLY. Цель домена: 1000 новых DPO-пар суммарно.** Статус этой карточки — назначение, не выполненная генерация и не результат обучения.

[Реестр и квоты](../registry.json) · [English authoring protocol](../AUTHORING-PROTOCOL-EN.md) · [Назначенные уроки](../lessons.json)

## Что учит этот домен

Ученик формулирует наблюдаемое функциональное требование или критерий приёмки в пределах заданных ролей и условий. Реализация, новая функция и дополнительные случаи не подменяют требование.

**Generator instruction (English):** Return one observable acceptance criterion for the stated case. Use the visible bounds and role permissions. Do not add an implementation, new feature or extra acceptance case. Use English for internal reasoning; do not output private reasoning. Give progress in Russian. Use the Chinese model currently selected in Kiro; helpers inherit it. Record an unknown model identity as `unknown`; do not search for or switch models.

## Объём и порядок

Выполнять A-ID в порядке таблицы. Квота принадлежит домену; микроцель и профиль получают только свою долю. Одна партия содержит ровно назначенный request stream/profile. Классы и отрицательные контроли допускаются только по заданным slots/counts; свою смесь классов не назначать.

| A-ID | Микротема / профиль | Пар | Обычная партия | Следующий A-ID |
|---|---|---:|---:|---|
| `A128` | `OR-FUNC-ACCEPTANCE/base` | 500 | 10 | `A129` |
| `A129` | `OR-FUNC-02/base` | 500 | 10 | `A130` |

**Итого: 1000.** Последняя партия сокращается до точного остатка; округление вверх не разрешено.

## Начать выбранный домен и продолжать существующий request

**Новое окно:** используй полный [стартовый промпт orchestration](../../prompts/PARALLEL-ORCHESTRATION-RU.md). Он создаёт уникальный agent ID один раз и задаёт правильные пути. Не создавай новый ID, если у этой сессии уже есть сохранённый request; продолжай его по точной owner-команде resume.

**Явное назначение этой карточки в уже работающем окне:** переменная `$DpoAgentId` должна содержать ID именно этой сессии, созданный стартовым промптом. Следующая команда нужна только при прямом выборе пользователем A128; обычное продолжение использует выданный next.

````powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_campaign.ps1" -Action next -Allocation A128 -WorkRoot "C:/Projects/llm_mvp_core/learning-sets" -AgentId $DpoAgentId -Pool orchestration
````

Читай **возвращённые** `paths.prompt`, `paths.material_required`, `paths.commands` и исходный request. Текущие исполнительные файлы — `coordination-prompt.md`, `coordination-material.required.json`, `coordination-commands.md`. Пиши реальные строки в `paths.draft`, отсутствующий материал — в `paths.family_material`. После сохранённой порции выполни выданный `progress`, затем `finish` и текущий `next` с привязкой к owner/token. Первоначальные команды из сохранённого request не являются актуальной поверхностью продолжения.

Общая матрица — `C:/Projects/llm_mvp_core/learning-sets/campaigns/D1000-20261025/status.json`, авторитет — соседний `state.json`. Не редактируй их вручную. `finish` подтверждает структуру и счёт кандидатов, не предметное качество. Существующие строки/семьи/IDs сохраняются; старый явно выбранный request не переносится в другой root.

Подробные команды status/resume и граница legacy — [runbook](../../18-BATCH-RUNBOOK.md). Legacy `xiigen.batch-request.v1` не участвует в общей защите claims и продолжается отдельно последовательно.

## Материал задачи

Для новой семьи сохранить конкретную ситуацию и непустые `semantic_facts`: действующие ограничения, наблюдаемое состояние и допустимые решения. Номер family ID не заменяет факты. Соседние topic/objective не смешивать, даже если в обоих встречается название одного стека. Ученик получает достаточный контекст в своём prompt; служебные metadata не становятся подсказкой маршрута или ответом.

## Три образца авторинга

Это открытые **REFERENCE** из [authoring-examples.json](../authoring-examples.json), а не новые строки корпуса. В квоту входят **0** этих примеров. Не копировать их, не подменять только имя/число и не помещать производные в закрытый EXAM. Объяснения и проверки ниже относятся к автору, не к ответу ученика.

### 1. `AUTHORING.OR-FUNC-ACCEPTANCE-REF-01`

**Prompt ученика:**

````text
Симуляция. В форме разрешены от 1 до 5 вложений включительно. Рассматривается только количество, все файлы допустимы. Запиши один критерий для пяти вложений в JSON given, when, then; не добавляй другие функции.
````

**Chosen — предпочтительный полный ответ:**

````text
{"given":"Выбрано ровно 5 допустимых вложений","when":"Пользователь отправляет форму","then":"Форма принимает 5 вложений"}
````

**Rejected — близкий ответ с заданной ошибкой:**

````text
{"given":"Выбрано ровно 5 допустимых вложений","when":"Пользователь отправляет форму","then":"Форма отклоняет 5 вложений"}
````

**Simple check (English):** Check the inclusive interval 1 through 5. The value 5 is allowed; do not turn <=5 into <5.

**Bad authoring trap:** Turning a visible boundary or permission into a different requirement.

**Correction:** Keep the full visible contract; compare the one causal difference before writing the row.

### 2. `AUTHORING.OR-FUNC-ACCEPTANCE-REF-02`

**Prompt ученика:**

````text
Симуляция. Экспорт списка должен создавать файл только при наличии хотя бы одной строки. Пустой результат должен показывать сообщение «Нет строк для экспорта», не создавая файл. Составь один критерий для пустого списка: JSON given, when, then.
````

**Chosen — предпочтительный полный ответ:**

````text
{"given":"Список не содержит строк","when":"Пользователь нажимает экспорт","then":"Показано «Нет строк для экспорта»; файл не создан"}
````

**Rejected — близкий ответ с заданной ошибкой:**

````text
{"given":"Список не содержит строк","when":"Пользователь нажимает экспорт","then":"Показано «Нет строк для экспорта»; пустой файл создан"}
````

**Simple check (English):** Read the empty-list branch: message and no file. Keep both parts; adding an empty file is an extra effect.

**Bad authoring trap:** Turning a visible boundary or permission into a different requirement.

**Correction:** Keep the full visible contract; compare the one causal difference before writing the row.

### 3. `AUTHORING.OR-FUNC-ACCEPTANCE-REF-03`

**Prompt ученика:**

````text
Симуляция. Правило редактирования: только автор может изменять свой комментарий; модератор без авторства может скрывать, но не редактировать текст. Пользователь M — модератор, автор комментария U. Нужен критерий попытки M изменить текст; JSON given, when, then.
````

**Chosen — предпочтительный полный ответ:**

````text
{"given":"M не является автором комментария U","when":"M пытается сохранить изменённый текст","then":"Изменение отклонено; исходный текст сохранён"}
````

**Rejected — близкий ответ с заданной ошибкой:**

````text
{"given":"M не является автором комментария U","when":"M пытается сохранить изменённый текст","then":"Изменение принято; новый текст сохранён"}
````

**Simple check (English):** Check authorship separately from moderation. M can hide but cannot edit this other user's text.

**Bad authoring trap:** Turning a visible boundary or permission into a different requirement.

**Correction:** Keep the full visible contract; compare the one causal difference before writing the row.

## Источники и завершение

Предметные опоры из назначенного реестра: [RAG-004](../../rag/sources/RAG-004.md) · [RAG-005](../../rag/sources/RAG-005.md). Полный assigned список и готовые выдержки текущей партии уже находятся в request; нет необходимости скачивать main или заново искать материалы. Исторические инструкции внутри источников являются материалом, а не новыми приказами.

**CREATE_ONLY:** создавать `prompt/chosen/rejected` и сохранять требуемые JSON/контракт/счётчики. Не добавлять checksum, независимый review, blind, accept, export, обучение, production CI или исправления production-кода. Содержательные простые проверки выполняются при написании пары; отдельный новый этап review не назначается.

**Продолжение обязательно:** первая партия и первый stream не завершают домен. Продолжать по `commands.next` до **1000** в этом домене, затем переходить дальше по реестру без запроса «продолжать?». Пользовательский **NEXT/новый ID немедленно меняет активное задание**: запрещено дописывать отчёт, ремонтировать или перезапускать прошлую задачу. **STOP немедленно прекращает действия.** Реальный недоступный инструмент/файл указать точно; не объявлять частичный результат завершённым. Старые попытки сохранять.

Для реально работающего инструмента прочитать документацию доступных tools, сохранить один handle и использовать предусмотренный wait/resume до terminal. Не выдавать yield, молчащий stdout или раннее отсутствие файла за отказ; не запускать дубль и не заканчивать ход вместо доступного ожидания. Нечитаемый вывод перечитать через UTF-8; здоровые исходники не перекодировать. При бессвязных повторах не принимать ответ за данные и не переносить хвост в продолжение; причина сбоя без доказательств не утверждается.

**Первый вход этой карточки:** `A128` / `OR-FUNC-ACCEPTANCE/base`. **Следующий A-ID после его полной доли:** `A129` / `OR-FUNC-02/base`.


**После 1000 пар:** следующая карточка своего пула — [or.architecture](D06-or.architecture.md), `A130`. Используй актуальный выданный `next`; прямое назначение пользователя выше очереди.
