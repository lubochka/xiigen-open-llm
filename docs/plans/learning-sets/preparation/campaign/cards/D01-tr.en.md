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
| `A001` — `TR07-OMISSION` | `tr.en` / `ru-en` | 30 | `A002` |
| `A002` — `TR07-OMISSION` | `tr.en` / `he-en` | 30 | `A003` |
| `A003` — `TR07-TRANSPOSE` | `tr.en` / `ru-en` | 30 | `A004` |
| `A004` — `TR07-TRANSPOSE` | `tr.en` / `he-en` | 30 | `A005` |
| `A005` — `TR07-VALIDWORD` | `tr.en` / `ru-en` | 30 | `A006` |
| `A006` — `TR07-VALIDWORD` | `tr.en` / `he-en` | 30 | `A007` |
| `A007` — `TR08-LAYOUT` | `tr.en` / `ru-en` | 30 | `A008` |
| `A008` — `TR08-LAYOUT` | `tr.en` / `he-en` | 30 | `A009` |
| `A009` — `TR10-CONDITIONAL` | `tr.en` / `ru-en` | 30 | `A010` |
| `A010` — `TR10-CONDITIONAL` | `tr.en` / `he-en` | 30 | `A011` |
| `A011` — `TR01-NEGATION-SCOPE` | `tr.en` / `ru-en` | 30 | `A012` |
| `A012` — `TR01-NEGATION-SCOPE` | `tr.en` / `he-en` | 30 | `A013` |
| `A013` — `TR02-ROLE-ORDER` | `tr.en` / `ru-en` | 30 | `A014` |
| `A014` — `TR02-ROLE-ORDER` | `tr.en` / `he-en` | 30 | `A015` |
| `A015` — `TR03-COMPLETION` | `tr.en` / `ru-en` | 30 | `A016` |
| `A016` — `TR03-COMPLETION` | `tr.en` / `he-en` | 30 | `A017` |
| `A017` — `TR04-EXPLICIT-GENDER` | `tr.en` / `ru-en` | 20 | `A018` |
| `A018` — `TR04-EXPLICIT-GENDER` | `tr.en` / `he-en` | 20 | `A019` |
| `A019` — `TR05-CONTEXT-SENSE` | `tr.en` / `ru-en` | 20 | `A020` |
| `A020` — `TR05-CONTEXT-SENSE` | `tr.en` / `he-en` | 20 | `A021` |
| `A021` — `TR06-PLACEHOLDER` | `tr.en` / `ru-en` | 20 | `A022` |
| `A022` — `TR06-PLACEHOLDER` | `tr.en` / `he-en` | 20 | `A023` |
| `A023` — `TR09-IDENTIFIER` | `tr.en` / `ru-en` | 20 | `A024` |
| `A024` — `TR09-IDENTIFIER` | `tr.en` / `he-en` | 20 | `A025` |
| `A025` — `TR02-COREFERENCE` | `tr.en` / `ru-en` | 20 | `A026` |
| `A026` — `TR02-COREFERENCE` | `tr.en` / `he-en` | 20 | `A027` |
| `A027` — `TR05-MODALITY` | `tr.en` / `ru-en` | 20 | `A028` |
| `A028` — `TR05-MODALITY` | `tr.en` / `he-en` | 20 | `A029` |
| `A029` — `TR08-RESTORE-OR-PRESERVE` | `tr.en` / `ru-en` | 20 | `A030` |
| `A030` — `TR08-RESTORE-OR-PRESERVE` | `tr.en` / `he-en` | 20 | `A031` |
| `A031` — `TR07-REPEAT` | `tr.en` / `ru-en` | 20 | `A032` |
| `A032` — `TR07-REPEAT` | `tr.en` / `he-en` | 20 | `A033` |
| `A033` — `TR06-NUMBER-UNIT` | `tr.en` / `ru-en` | 20 | `A034` |
| `A034` — `TR06-NUMBER-UNIT` | `tr.en` / `he-en` | 20 | `A035` |
| `A035` — `TR03-EVENT-ORDER` | `tr.en` / `ru-en` | 20 | `A036` |
| `A036` — `TR03-EVENT-ORDER` | `tr.en` / `he-en` | 20 | `A037` |
| `A037` — `TR-EN-IF-01` | `tr.en` / `ru-en` | 20 | `A038` |
| `A038` — `TR-EN-IF-01` | `tr.en` / `he-en` | 20 | `A039` |
| `A039` — `TR-EN-SCOPE-01` | `tr.en` / `ru-en` | 20 | `A040` |
| `A040` — `TR-EN-SCOPE-01` | `tr.en` / `he-en` | 20 | `A041` |
| `A041` — `TR-EN-ORDER-01` | `tr.en` / `ru-en` | 20 | `A042` |
| `A042` — `TR-EN-ORDER-01` | `tr.en` / `he-en` | 20 | `A043` |


Следующий ID из таблицы — номинальный порядок своей группы. Фактическую свободную карточку с учётом владельцев выдаёт команда `next` текущего request; не менять группу и чужой статус вручную.

Замечания качества: смотри `quality_flags` своего domain/topic/profile в общем status. `needs_rework` указывает доказанный дефект с evidence, `needs_assessment` — область без достаточной оценки. Это не новый review-гейт и не разрешение исправлять чужую старую попытку; finish не закрывает флаг автоматически. [Правила общей матрицы](../../28-PARALLEL-GENERATION-AND-QUALITY.md). Для нового окна используй [один из трёх готовых стартовых промптов](../../00-START-HERE.md); не исполняй исторические команды из нижней справочной части вместо текущих owner-команд.

<!-- /CURRENT-PARALLEL-CONTRACT-20261026 -->

# D01 — Перевод на английский (`tr.en`)

**Режим: CREATE_ONLY. Цель домена: 1000 новых DPO-пар суммарно.** Статус этой карточки — назначение, не выполненная генерация и не результат обучения.

[Реестр и квоты](../registry.json) · [English authoring protocol](../AUTHORING-PROTOCOL-EN.md) · [Назначенные уроки](../lessons.json)

## Что учит этот домен

Ученик переводит на английский заданный русский или ивритский текст. Исправляет только явно назначенное повреждение и сохраняет все остальные факты; служебные инструкции генератору в ответ не входят.

**Generator instruction (English):** Create preferences for translation into English. Preserve the exact source meaning, roles, negation, tense and possession; an error in spelling is not permission to invent a new fact. Use only the assigned mutation and target language. Use English for internal reasoning; do not output private reasoning. Give progress in Russian. Use the Chinese model currently selected in Kiro; helpers inherit it. Record an unknown model identity as `unknown`; do not search for or switch models.

## Объём и порядок

Выполнять A-ID в порядке таблицы. Квота принадлежит домену; микроцель и профиль получают только свою долю. Одна партия содержит ровно назначенный request stream/profile. Классы и отрицательные контроли допускаются только по заданным slots/counts; свою смесь классов не назначать.

**Профили:** `he-en` — **500** пар; `ru-en` — **500** пар. Это две доли одного домена: **500 + 500 = 1000**.

| A-ID | Микротема / профиль | Пар | Обычная партия | Следующий A-ID |
|---|---|---:|---:|---|
| `A001` | `TR07-OMISSION/ru-en` | 30 | 10 | `A002` |
| `A002` | `TR07-OMISSION/he-en` | 30 | 10 | `A003` |
| `A003` | `TR07-TRANSPOSE/ru-en` | 30 | 10 | `A004` |
| `A004` | `TR07-TRANSPOSE/he-en` | 30 | 10 | `A005` |
| `A005` | `TR07-VALIDWORD/ru-en` | 30 | 10 | `A006` |
| `A006` | `TR07-VALIDWORD/he-en` | 30 | 10 | `A007` |
| `A007` | `TR08-LAYOUT/ru-en` | 30 | 10 | `A008` |
| `A008` | `TR08-LAYOUT/he-en` | 30 | 10 | `A009` |
| `A009` | `TR10-CONDITIONAL/ru-en` | 30 | 10 | `A010` |
| `A010` | `TR10-CONDITIONAL/he-en` | 30 | 10 | `A011` |
| `A011` | `TR01-NEGATION-SCOPE/ru-en` | 30 | 10 | `A012` |
| `A012` | `TR01-NEGATION-SCOPE/he-en` | 30 | 10 | `A013` |
| `A013` | `TR02-ROLE-ORDER/ru-en` | 30 | 10 | `A014` |
| `A014` | `TR02-ROLE-ORDER/he-en` | 30 | 10 | `A015` |
| `A015` | `TR03-COMPLETION/ru-en` | 30 | 10 | `A016` |
| `A016` | `TR03-COMPLETION/he-en` | 30 | 10 | `A017` |
| `A017` | `TR04-EXPLICIT-GENDER/ru-en` | 20 | 10 | `A018` |
| `A018` | `TR04-EXPLICIT-GENDER/he-en` | 20 | 10 | `A019` |
| `A019` | `TR05-CONTEXT-SENSE/ru-en` | 20 | 10 | `A020` |
| `A020` | `TR05-CONTEXT-SENSE/he-en` | 20 | 10 | `A021` |
| `A021` | `TR06-PLACEHOLDER/ru-en` | 20 | 10 | `A022` |
| `A022` | `TR06-PLACEHOLDER/he-en` | 20 | 10 | `A023` |
| `A023` | `TR09-IDENTIFIER/ru-en` | 20 | 10 | `A024` |
| `A024` | `TR09-IDENTIFIER/he-en` | 20 | 10 | `A025` |
| `A025` | `TR02-COREFERENCE/ru-en` | 20 | 10 | `A026` |
| `A026` | `TR02-COREFERENCE/he-en` | 20 | 10 | `A027` |
| `A027` | `TR05-MODALITY/ru-en` | 20 | 10 | `A028` |
| `A028` | `TR05-MODALITY/he-en` | 20 | 10 | `A029` |
| `A029` | `TR08-RESTORE-OR-PRESERVE/ru-en` | 20 | 10 | `A030` |
| `A030` | `TR08-RESTORE-OR-PRESERVE/he-en` | 20 | 10 | `A031` |
| `A031` | `TR07-REPEAT/ru-en` | 20 | 10 | `A032` |
| `A032` | `TR07-REPEAT/he-en` | 20 | 10 | `A033` |
| `A033` | `TR06-NUMBER-UNIT/ru-en` | 20 | 10 | `A034` |
| `A034` | `TR06-NUMBER-UNIT/he-en` | 20 | 10 | `A035` |
| `A035` | `TR03-EVENT-ORDER/ru-en` | 20 | 10 | `A036` |
| `A036` | `TR03-EVENT-ORDER/he-en` | 20 | 10 | `A037` |
| `A037` | `TR-EN-IF-01/ru-en` | 20 | 10 | `A038` |
| `A038` | `TR-EN-IF-01/he-en` | 20 | 10 | `A039` |
| `A039` | `TR-EN-SCOPE-01/ru-en` | 20 | 10 | `A040` |
| `A040` | `TR-EN-SCOPE-01/he-en` | 20 | 10 | `A041` |
| `A041` | `TR-EN-ORDER-01/ru-en` | 20 | 10 | `A042` |
| `A042` | `TR-EN-ORDER-01/he-en` | 20 | 10 | `A043` |

**Итого: 1000.** Последняя партия сокращается до точного остатка; округление вверх не разрешено.

## Начать выбранный домен и продолжать существующий request

**Новое окно:** используй полный [стартовый промпт translations](../../prompts/PARALLEL-TRANSLATIONS-RU.md). Он создаёт уникальный agent ID один раз и задаёт правильные пути. Не создавай новый ID, если у этой сессии уже есть сохранённый request; продолжай его по точной owner-команде resume.

**Явное назначение этой карточки в уже работающем окне:** переменная `$DpoAgentId` должна содержать ID именно этой сессии, созданный стартовым промптом. Следующая команда нужна только при прямом выборе пользователем A001; обычное продолжение использует выданный next.

````powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_campaign.ps1" -Action next -Allocation A001 -WorkRoot "C:/Projects/llm_mvp_core/learning-sets" -AgentId $DpoAgentId -Pool translations
````

Читай **возвращённые** `paths.prompt`, `paths.material_required`, `paths.commands` и исходный request. Текущие исполнительные файлы — `coordination-prompt.md`, `coordination-material.required.json`, `coordination-commands.md`. Пиши реальные строки в `paths.draft`, отсутствующий материал — в `paths.family_material`. После сохранённой порции выполни выданный `progress`, затем `finish` и текущий `next` с привязкой к owner/token. Первоначальные команды из сохранённого request не являются актуальной поверхностью продолжения.

Общая матрица — `C:/Projects/llm_mvp_core/learning-sets/campaigns/D1000-20261025/status.json`, авторитет — соседний `state.json`. Не редактируй их вручную. `finish` подтверждает структуру и счёт кандидатов, не предметное качество. Существующие строки/семьи/IDs сохраняются; старый явно выбранный request не переносится в другой root.

Подробные команды status/resume и граница legacy — [runbook](../../18-BATCH-RUNBOOK.md). Legacy `xiigen.batch-request.v1` не участвует в общей защите claims и продолжается отдельно последовательно.

## Канонический материал семей и точные повреждения

Каждой отсутствующей семье назначаются реальные `scenario` и `semantic_facts`, а также **те же факты в `source_text_by_language.en`, `.ru`, `.he`**. Указать участников, действие, владельца, время и число. Остальные направления используют эти же канонические тексты и facts; под тем же family ID нельзя придумать другую историю. Уже сохранённый материал брать из request/ledger без смены значения.

У общего правила о трёх одинаково устроенных канонических текстах есть **два узких исключения**, которые определяются назначенным `family_material_kind` в request:

- **`TR10-CONDITIONAL` / `source_ambiguity`:** неоднозначность принадлежит исходному языку. `scenario_group=TR10-CONDITIONAL-src-<source>` объединяет только два направления из этого языка. В `source_text_by_language` нужен назначенный исходный язык; в `sense_translations` — минимум два разных смысла с эквивалентами `en`, `ru`, `he`. Не создавать мнимую неоднозначность в обратном направлении: перевод одного из смыслов может быть однозначным. У классов `ambiguous` и `resolved` один исходный текст, а различающий достаточный контекст задаётся в `learner_visible_context_by_class`; только контекст своего класса и исходного языка попадает ученику.
- **`TR08-RESTORE-OR-PRESERVE` / `layout_counterfactual`:** вместо общей канонической строки использовать `source_text_by_class_language.restore` и `.preserve`, каждое с `en`, `ru`, `he`. В `restore` канонический текст содержит восстановленное обычное слово, в `preserve` — исходный литеральный идентификатор. Внутри двух строк одной семьи `source_raw` и повреждённый токен одинаковы, но видимый контекст задаёт обычное слово либо идентификатор. `metadata.canonical_source` берётся из текущего класса; нельзя навязать двум классам одинаковое восстановление. Поля `scenario`, `semantic_facts`, family/origin и split остаются общими, а ответы и контексты следуют назначенному классу.

`family_id`/`origin_family_id` связывают сценарий, а `lexical_family_ids` — лексему с её опечатками и неверной раскладкой. «молоко» и `window` не становятся одной лексической семьёй по совпавшему номеру. В ученический prompt попадает только назначенный исходный язык и разрешённая вариация; перевод на другие языки, canonical restoration и rationale не подсказывают ответ ученику.

Для удаления длина уменьшается ровно на один code point и удаление указанной позиции воспроизводит raw: `письмо → писмо` — удаление, `письмо → пильмо` — замена. Для перестановки меняются местами две соседние разные буквы при той же длине и составе; повтор добавляет ровно назначенную букву. Раскладка проверяется по назначенной карте клавиш, не по догадке. VALIDWORD сохраняет исходное правильное слово и не превращается в дополнительный набор исправлений. После изменения читать всё предложение: субъект, отрицание, род, число, место и владелец сохраняются.

## Три образца авторинга

Это открытые **REFERENCE** из [authoring-examples.json](../authoring-examples.json), а не новые строки корпуса. В квоту входят **0** этих примеров. Не копировать их, не подменять только имя/число и не помещать производные в закрытый EXAM. Объяснения и проверки ниже относятся к автору, не к ответу ученика.

### 1. `AUTHORING.TR-EN-DELETE-01`

**Prompt ученика:**

````text
Переведи на английский, только перевод. В одном слове пропущена ровно одна буква. Контекст: бумажная почта родственнице. Исходный текст: Она отправила писмо бабушке.
````

**Chosen — предпочтительный полный ответ:**

````text
She sent a letter to her grandmother.
````

**Rejected — близкий ответ с заданной ошибкой:**

````text
She sent a parcel to her grandmother.
````

**Simple check (English):** письмо has 6 code points; delete ь at zero-based index 3 to obtain писмо with 5. Reinserting ь restores the exact source sentence; rejected changes only the mailed object.

**Bad authoring trap:** Calling письмо → пильмо a deletion: both have length 6 and с was replaced with л.

**Correction:** Use the stated construction check; keep all unaffected source facts and characters.

### 2. `AUTHORING.TR-EN-KEEP-01`

**Prompt ученика:**

````text
Переведи на английский, только перевод. Исходный текст записан правильно, опечатка не заявлена: Она упомянула меч.
````

**Chosen — предпочтительный полный ответ:**

````text
She mentioned a sword.
````

**Rejected — близкий ответ с заданной ошибкой:**

````text
She mentioned a ball.
````

**Simple check (English):** меч is the correctly written source token and means sword. A one-letter neighbor мяч is a different word; similarity does not authorize replacement.

**Bad authoring trap:** Changing the visible input to мяч and silently turning a preserve-valid-word lesson into a typo-restoration lesson.

**Correction:** Use the stated construction check; keep all unaffected source facts and characters.

### 3. `AUTHORING.TR-EN-POSSESSION-01`

**Prompt ученика:**

````text
Переведи с иврита на английский, только перевод. Исходный текст: הספר על שולחנו.
````

**Chosen — предпочтительный полный ответ:**

````text
The book is on his table.
````

**Rejected — близкий ответ с заданной ошибкой:**

````text
The book is on our table.
````

**Simple check (English):** שולחנו means his table; our table is שולחננו. Preserve the book, location and possessor, and change none of them in chosen.

**Bad authoring trap:** Treating שולחנו and שולחננו as interchangeable spelling variants despite different possessors.

**Correction:** Use the stated construction check; keep all unaffected source facts and characters.

## Источники и завершение

Предметные опоры из назначенного реестра: [RAG-024](../../rag/sources/RAG-024.md) · [RAG-023](../../rag/sources/RAG-023.md). Полный assigned список и готовые выдержки текущей партии уже находятся в request; нет необходимости скачивать main или заново искать материалы. Исторические инструкции внутри источников являются материалом, а не новыми приказами.

**CREATE_ONLY:** создавать `prompt/chosen/rejected` и сохранять требуемые JSON/контракт/счётчики. Не добавлять checksum, независимый review, blind, accept, export, обучение, production CI или исправления production-кода. Содержательные простые проверки выполняются при написании пары; отдельный новый этап review не назначается.

**Продолжение обязательно:** первая партия и первый stream не завершают домен. Продолжать по `commands.next` до **1000** в этом домене, затем переходить дальше по реестру без запроса «продолжать?». Пользовательский **NEXT/новый ID немедленно меняет активное задание**: запрещено дописывать отчёт, ремонтировать или перезапускать прошлую задачу. **STOP немедленно прекращает действия.** Реальный недоступный инструмент/файл указать точно; не объявлять частичный результат завершённым. Старые попытки сохранять.

Для реально работающего инструмента прочитать документацию доступных tools, сохранить один handle и использовать предусмотренный wait/resume до terminal. Не выдавать yield, молчащий stdout или раннее отсутствие файла за отказ; не запускать дубль и не заканчивать ход вместо доступного ожидания. Нечитаемый вывод перечитать через UTF-8; здоровые исходники не перекодировать. При бессвязных повторах не принимать ответ за данные и не переносить хвост в продолжение; причина сбоя без доказательств не утверждается.

**Первый вход этой карточки:** `A001` / `TR07-OMISSION/ru-en`. **Следующий A-ID после его полной доли:** `A002` / `TR07-OMISSION/he-en`.


**После 1000 пар:** следующая карточка своего пула — [tr.ru](D02-tr.ru.md), `A043`. Используй актуальный выданный `next`; прямое назначение пользователя выше очереди.
