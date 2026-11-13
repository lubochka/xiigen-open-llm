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
| `A043` — `TR07-OMISSION` | `tr.ru` / `en-ru` | 30 | `A044` |
| `A044` — `TR07-OMISSION` | `tr.ru` / `he-ru` | 30 | `A045` |
| `A045` — `TR07-TRANSPOSE` | `tr.ru` / `en-ru` | 30 | `A046` |
| `A046` — `TR07-TRANSPOSE` | `tr.ru` / `he-ru` | 30 | `A047` |
| `A047` — `TR07-VALIDWORD` | `tr.ru` / `en-ru` | 30 | `A048` |
| `A048` — `TR07-VALIDWORD` | `tr.ru` / `he-ru` | 30 | `A049` |
| `A049` — `TR08-LAYOUT` | `tr.ru` / `en-ru` | 30 | `A050` |
| `A050` — `TR08-LAYOUT` | `tr.ru` / `he-ru` | 30 | `A051` |
| `A051` — `TR10-CONDITIONAL` | `tr.ru` / `en-ru` | 30 | `A052` |
| `A052` — `TR10-CONDITIONAL` | `tr.ru` / `he-ru` | 30 | `A053` |
| `A053` — `TR01-NEGATION-SCOPE` | `tr.ru` / `en-ru` | 30 | `A054` |
| `A054` — `TR01-NEGATION-SCOPE` | `tr.ru` / `he-ru` | 30 | `A055` |
| `A055` — `TR02-ROLE-ORDER` | `tr.ru` / `en-ru` | 30 | `A056` |
| `A056` — `TR02-ROLE-ORDER` | `tr.ru` / `he-ru` | 30 | `A057` |
| `A057` — `TR03-COMPLETION` | `tr.ru` / `en-ru` | 30 | `A058` |
| `A058` — `TR03-COMPLETION` | `tr.ru` / `he-ru` | 30 | `A059` |
| `A059` — `TR04-EXPLICIT-GENDER` | `tr.ru` / `en-ru` | 20 | `A060` |
| `A060` — `TR04-EXPLICIT-GENDER` | `tr.ru` / `he-ru` | 20 | `A061` |
| `A061` — `TR05-CONTEXT-SENSE` | `tr.ru` / `en-ru` | 20 | `A062` |
| `A062` — `TR05-CONTEXT-SENSE` | `tr.ru` / `he-ru` | 20 | `A063` |
| `A063` — `TR06-PLACEHOLDER` | `tr.ru` / `en-ru` | 20 | `A064` |
| `A064` — `TR06-PLACEHOLDER` | `tr.ru` / `he-ru` | 20 | `A065` |
| `A065` — `TR09-IDENTIFIER` | `tr.ru` / `en-ru` | 20 | `A066` |
| `A066` — `TR09-IDENTIFIER` | `tr.ru` / `he-ru` | 20 | `A067` |
| `A067` — `TR02-COREFERENCE` | `tr.ru` / `en-ru` | 20 | `A068` |
| `A068` — `TR02-COREFERENCE` | `tr.ru` / `he-ru` | 20 | `A069` |
| `A069` — `TR05-MODALITY` | `tr.ru` / `en-ru` | 20 | `A070` |
| `A070` — `TR05-MODALITY` | `tr.ru` / `he-ru` | 20 | `A071` |
| `A071` — `TR08-RESTORE-OR-PRESERVE` | `tr.ru` / `en-ru` | 20 | `A072` |
| `A072` — `TR08-RESTORE-OR-PRESERVE` | `tr.ru` / `he-ru` | 20 | `A073` |
| `A073` — `TR07-REPEAT` | `tr.ru` / `en-ru` | 20 | `A074` |
| `A074` — `TR07-REPEAT` | `tr.ru` / `he-ru` | 20 | `A075` |
| `A075` — `TR06-NUMBER-UNIT` | `tr.ru` / `en-ru` | 20 | `A076` |
| `A076` — `TR06-NUMBER-UNIT` | `tr.ru` / `he-ru` | 20 | `A077` |
| `A077` — `TR03-EVENT-ORDER` | `tr.ru` / `en-ru` | 20 | `A078` |
| `A078` — `TR03-EVENT-ORDER` | `tr.ru` / `he-ru` | 20 | `A079` |
| `A079` — `TR-RU-IF-01` | `tr.ru` / `en-ru` | 20 | `A080` |
| `A080` — `TR-RU-IF-01` | `tr.ru` / `he-ru` | 20 | `A081` |
| `A081` — `TR-RU-SCOPE-01` | `tr.ru` / `en-ru` | 20 | `A082` |
| `A082` — `TR-RU-SCOPE-01` | `tr.ru` / `he-ru` | 20 | `A083` |
| `A083` — `TR-RU-ORDER-01` | `tr.ru` / `en-ru` | 20 | `A084` |
| `A084` — `TR-RU-ORDER-01` | `tr.ru` / `he-ru` | 20 | `A085` |


Следующий ID из таблицы — номинальный порядок своей группы. Фактическую свободную карточку с учётом владельцев выдаёт команда `next` текущего request; не менять группу и чужой статус вручную.

Замечания качества: смотри `quality_flags` своего domain/topic/profile в общем status. `needs_rework` указывает доказанный дефект с evidence, `needs_assessment` — область без достаточной оценки. Это не новый review-гейт и не разрешение исправлять чужую старую попытку; finish не закрывает флаг автоматически. [Правила общей матрицы](../../28-PARALLEL-GENERATION-AND-QUALITY.md). Для нового окна используй [один из трёх готовых стартовых промптов](../../00-START-HERE.md); не исполняй исторические команды из нижней справочной части вместо текущих owner-команд.

<!-- /CURRENT-PARALLEL-CONTRACT-20261026 -->

# D02 — Перевод на русский (`tr.ru`)

**Режим: CREATE_ONLY. Цель домена: 1000 новых DPO-пар суммарно.** Статус этой карточки — назначение, не выполненная генерация и не результат обучения.

[Реестр и квоты](../registry.json) · [English authoring protocol](../AUTHORING-PROTOCOL-EN.md) · [Назначенные уроки](../lessons.json)

## Что учит этот домен

Ученик переводит на русский заданный английский или ивритский текст. Требуется точный смысл, роли, отрицание и владение; близость написания не разрешает заменять правильное слово другим.

**Generator instruction (English):** Create preferences for translation into Russian. Choose one semantic defect in rejected; a valid synonym is not a defect. Preserve correct source words unless the visible task explicitly declares a recoverable typo. Use English for internal reasoning; do not output private reasoning. Give progress in Russian. Use the Chinese model currently selected in Kiro; helpers inherit it. Record an unknown model identity as `unknown`; do not search for or switch models.

## Объём и порядок

Выполнять A-ID в порядке таблицы. Квота принадлежит домену; микроцель и профиль получают только свою долю. Одна партия содержит ровно назначенный request stream/profile. Классы и отрицательные контроли допускаются только по заданным slots/counts; свою смесь классов не назначать.

**Профили:** `en-ru` — **500** пар; `he-ru` — **500** пар. Это две доли одного домена: **500 + 500 = 1000**.

| A-ID | Микротема / профиль | Пар | Обычная партия | Следующий A-ID |
|---|---|---:|---:|---|
| `A043` | `TR07-OMISSION/en-ru` | 30 | 10 | `A044` |
| `A044` | `TR07-OMISSION/he-ru` | 30 | 10 | `A045` |
| `A045` | `TR07-TRANSPOSE/en-ru` | 30 | 10 | `A046` |
| `A046` | `TR07-TRANSPOSE/he-ru` | 30 | 10 | `A047` |
| `A047` | `TR07-VALIDWORD/en-ru` | 30 | 10 | `A048` |
| `A048` | `TR07-VALIDWORD/he-ru` | 30 | 10 | `A049` |
| `A049` | `TR08-LAYOUT/en-ru` | 30 | 10 | `A050` |
| `A050` | `TR08-LAYOUT/he-ru` | 30 | 10 | `A051` |
| `A051` | `TR10-CONDITIONAL/en-ru` | 30 | 10 | `A052` |
| `A052` | `TR10-CONDITIONAL/he-ru` | 30 | 10 | `A053` |
| `A053` | `TR01-NEGATION-SCOPE/en-ru` | 30 | 10 | `A054` |
| `A054` | `TR01-NEGATION-SCOPE/he-ru` | 30 | 10 | `A055` |
| `A055` | `TR02-ROLE-ORDER/en-ru` | 30 | 10 | `A056` |
| `A056` | `TR02-ROLE-ORDER/he-ru` | 30 | 10 | `A057` |
| `A057` | `TR03-COMPLETION/en-ru` | 30 | 10 | `A058` |
| `A058` | `TR03-COMPLETION/he-ru` | 30 | 10 | `A059` |
| `A059` | `TR04-EXPLICIT-GENDER/en-ru` | 20 | 10 | `A060` |
| `A060` | `TR04-EXPLICIT-GENDER/he-ru` | 20 | 10 | `A061` |
| `A061` | `TR05-CONTEXT-SENSE/en-ru` | 20 | 10 | `A062` |
| `A062` | `TR05-CONTEXT-SENSE/he-ru` | 20 | 10 | `A063` |
| `A063` | `TR06-PLACEHOLDER/en-ru` | 20 | 10 | `A064` |
| `A064` | `TR06-PLACEHOLDER/he-ru` | 20 | 10 | `A065` |
| `A065` | `TR09-IDENTIFIER/en-ru` | 20 | 10 | `A066` |
| `A066` | `TR09-IDENTIFIER/he-ru` | 20 | 10 | `A067` |
| `A067` | `TR02-COREFERENCE/en-ru` | 20 | 10 | `A068` |
| `A068` | `TR02-COREFERENCE/he-ru` | 20 | 10 | `A069` |
| `A069` | `TR05-MODALITY/en-ru` | 20 | 10 | `A070` |
| `A070` | `TR05-MODALITY/he-ru` | 20 | 10 | `A071` |
| `A071` | `TR08-RESTORE-OR-PRESERVE/en-ru` | 20 | 10 | `A072` |
| `A072` | `TR08-RESTORE-OR-PRESERVE/he-ru` | 20 | 10 | `A073` |
| `A073` | `TR07-REPEAT/en-ru` | 20 | 10 | `A074` |
| `A074` | `TR07-REPEAT/he-ru` | 20 | 10 | `A075` |
| `A075` | `TR06-NUMBER-UNIT/en-ru` | 20 | 10 | `A076` |
| `A076` | `TR06-NUMBER-UNIT/he-ru` | 20 | 10 | `A077` |
| `A077` | `TR03-EVENT-ORDER/en-ru` | 20 | 10 | `A078` |
| `A078` | `TR03-EVENT-ORDER/he-ru` | 20 | 10 | `A079` |
| `A079` | `TR-RU-IF-01/en-ru` | 20 | 10 | `A080` |
| `A080` | `TR-RU-IF-01/he-ru` | 20 | 10 | `A081` |
| `A081` | `TR-RU-SCOPE-01/en-ru` | 20 | 10 | `A082` |
| `A082` | `TR-RU-SCOPE-01/he-ru` | 20 | 10 | `A083` |
| `A083` | `TR-RU-ORDER-01/en-ru` | 20 | 10 | `A084` |
| `A084` | `TR-RU-ORDER-01/he-ru` | 20 | 10 | `A085` |

**Итого: 1000.** Последняя партия сокращается до точного остатка; округление вверх не разрешено.

## Начать выбранный домен и продолжать существующий request

**Новое окно:** используй полный [стартовый промпт translations](../../prompts/PARALLEL-TRANSLATIONS-RU.md). Он создаёт уникальный agent ID один раз и задаёт правильные пути. Не создавай новый ID, если у этой сессии уже есть сохранённый request; продолжай его по точной owner-команде resume.

**Явное назначение этой карточки в уже работающем окне:** переменная `$DpoAgentId` должна содержать ID именно этой сессии, созданный стартовым промптом. Следующая команда нужна только при прямом выборе пользователем A043; обычное продолжение использует выданный next.

````powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_campaign.ps1" -Action next -Allocation A043 -WorkRoot "C:/Projects/llm_mvp_core/learning-sets" -AgentId $DpoAgentId -Pool translations
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

### 1. `AUTHORING.TR-RU-DELETE-01`

**Prompt ученика:**

````text
Переведи на русский, только перевод. В одном слове пропущена ровно одна буква. Контекст: бумажное послание брату. Исходный текст: She wrote a leter to her brother.
````

**Chosen — предпочтительный полный ответ:**

````text
Она написала письмо брату.
````

**Rejected — близкий ответ с заданной ошибкой:**

````text
Она написала открытку брату.
````

**Simple check (English):** letter has 6 code points; deleting the t at index 3 produces leter with 5. Reinsert that t and confirm the whole source sentence is unchanged elsewhere.

**Bad authoring trap:** Accepting a same-length replacement as deletion or changing the recipient while repairing the token.

**Correction:** Use the stated construction check; keep all unaffected source facts and characters.

### 2. `AUTHORING.TR-RU-KEEP-01`

**Prompt ученика:**

````text
Переведи на русский, только перевод. Исходный текст записан правильно, опечатка не заявлена: She mentioned a sword.
````

**Chosen — предпочтительный полный ответ:**

````text
Она упомянула меч.
````

**Rejected — близкий ответ с заданной ошибкой:**

````text
Она упомянула слово.
````

**Simple check (English):** The source is sword, not word. Deleting s would make a different valid word, but the task requests translation of the unchanged source.

**Bad authoring trap:** Adding five restore-damaged examples merely because the card shows a counterexample; the assigned class is preserve-valid-word.

**Correction:** Use the stated construction check; keep all unaffected source facts and characters.

### 3. `AUTHORING.TR-RU-POSSESSION-01`

**Prompt ученика:**

````text
Переведи с иврита на русский, только перевод. Исходный текст: הספר על שולחנו.
````

**Chosen — предпочтительный полный ответ:**

````text
Книга на его столе.
````

**Rejected — близкий ответ с заданной ошибкой:**

````text
Книга на нашем столе.
````

**Simple check (English):** Read the exact ending: שולחנו means his table, whereas שולחננו means our table. Use the same book-on-his-table origin as the English and Hebrew reference counterparts.

**Bad authoring trap:** Using the same F-number for an unrelated milk scenario, or changing the possessor to make a smoother translation.

**Correction:** Use the stated construction check; keep all unaffected source facts and characters.

## Источники и завершение

Предметные опоры из назначенного реестра: [RAG-024](../../rag/sources/RAG-024.md) · [RAG-023](../../rag/sources/RAG-023.md). Полный assigned список и готовые выдержки текущей партии уже находятся в request; нет необходимости скачивать main или заново искать материалы. Исторические инструкции внутри источников являются материалом, а не новыми приказами.

**CREATE_ONLY:** создавать `prompt/chosen/rejected` и сохранять требуемые JSON/контракт/счётчики. Не добавлять checksum, независимый review, blind, accept, export, обучение, production CI или исправления production-кода. Содержательные простые проверки выполняются при написании пары; отдельный новый этап review не назначается.

**Продолжение обязательно:** первая партия и первый stream не завершают домен. Продолжать по `commands.next` до **1000** в этом домене, затем переходить дальше по реестру без запроса «продолжать?». Пользовательский **NEXT/новый ID немедленно меняет активное задание**: запрещено дописывать отчёт, ремонтировать или перезапускать прошлую задачу. **STOP немедленно прекращает действия.** Реальный недоступный инструмент/файл указать точно; не объявлять частичный результат завершённым. Старые попытки сохранять.

Для реально работающего инструмента прочитать документацию доступных tools, сохранить один handle и использовать предусмотренный wait/resume до terminal. Не выдавать yield, молчащий stdout или раннее отсутствие файла за отказ; не запускать дубль и не заканчивать ход вместо доступного ожидания. Нечитаемый вывод перечитать через UTF-8; здоровые исходники не перекодировать. При бессвязных повторах не принимать ответ за данные и не переносить хвост в продолжение; причина сбоя без доказательств не утверждается.

**Первый вход этой карточки:** `A043` / `TR07-OMISSION/en-ru`. **Следующий A-ID после его полной доли:** `A044` / `TR07-OMISSION/he-ru`.


**После 1000 пар:** следующая карточка своего пула — [tr.he](D03-tr.he.md), `A085`. Используй актуальный выданный `next`; прямое назначение пользователя выше очереди.
