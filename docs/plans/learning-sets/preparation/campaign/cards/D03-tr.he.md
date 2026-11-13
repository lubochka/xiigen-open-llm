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
| `A085` — `TR07-OMISSION` | `tr.he` / `ru-he` | 30 | `A086` |
| `A086` — `TR07-OMISSION` | `tr.he` / `en-he` | 30 | `A087` |
| `A087` — `TR07-TRANSPOSE` | `tr.he` / `ru-he` | 30 | `A088` |
| `A088` — `TR07-TRANSPOSE` | `tr.he` / `en-he` | 30 | `A089` |
| `A089` — `TR07-VALIDWORD` | `tr.he` / `ru-he` | 30 | `A090` |
| `A090` — `TR07-VALIDWORD` | `tr.he` / `en-he` | 30 | `A091` |
| `A091` — `TR08-LAYOUT` | `tr.he` / `ru-he` | 30 | `A092` |
| `A092` — `TR08-LAYOUT` | `tr.he` / `en-he` | 30 | `A093` |
| `A093` — `TR10-CONDITIONAL` | `tr.he` / `ru-he` | 30 | `A094` |
| `A094` — `TR10-CONDITIONAL` | `tr.he` / `en-he` | 30 | `A095` |
| `A095` — `TR01-NEGATION-SCOPE` | `tr.he` / `ru-he` | 30 | `A096` |
| `A096` — `TR01-NEGATION-SCOPE` | `tr.he` / `en-he` | 30 | `A097` |
| `A097` — `TR02-ROLE-ORDER` | `tr.he` / `ru-he` | 30 | `A098` |
| `A098` — `TR02-ROLE-ORDER` | `tr.he` / `en-he` | 30 | `A099` |
| `A099` — `TR03-COMPLETION` | `tr.he` / `ru-he` | 30 | `A100` |
| `A100` — `TR03-COMPLETION` | `tr.he` / `en-he` | 30 | `A101` |
| `A101` — `TR04-EXPLICIT-GENDER` | `tr.he` / `ru-he` | 20 | `A102` |
| `A102` — `TR04-EXPLICIT-GENDER` | `tr.he` / `en-he` | 20 | `A103` |
| `A103` — `TR05-CONTEXT-SENSE` | `tr.he` / `ru-he` | 20 | `A104` |
| `A104` — `TR05-CONTEXT-SENSE` | `tr.he` / `en-he` | 20 | `A105` |
| `A105` — `TR06-PLACEHOLDER` | `tr.he` / `ru-he` | 20 | `A106` |
| `A106` — `TR06-PLACEHOLDER` | `tr.he` / `en-he` | 20 | `A107` |
| `A107` — `TR09-IDENTIFIER` | `tr.he` / `ru-he` | 20 | `A108` |
| `A108` — `TR09-IDENTIFIER` | `tr.he` / `en-he` | 20 | `A109` |
| `A109` — `TR02-COREFERENCE` | `tr.he` / `ru-he` | 20 | `A110` |
| `A110` — `TR02-COREFERENCE` | `tr.he` / `en-he` | 20 | `A111` |
| `A111` — `TR05-MODALITY` | `tr.he` / `ru-he` | 20 | `A112` |
| `A112` — `TR05-MODALITY` | `tr.he` / `en-he` | 20 | `A113` |
| `A113` — `TR08-RESTORE-OR-PRESERVE` | `tr.he` / `ru-he` | 20 | `A114` |
| `A114` — `TR08-RESTORE-OR-PRESERVE` | `tr.he` / `en-he` | 20 | `A115` |
| `A115` — `TR07-REPEAT` | `tr.he` / `en-he` | 20 | `A116` |
| `A116` — `TR07-REPEAT` | `tr.he` / `ru-he` | 20 | `A117` |
| `A117` — `TR06-NUMBER-UNIT` | `tr.he` / `en-he` | 20 | `A118` |
| `A118` — `TR06-NUMBER-UNIT` | `tr.he` / `ru-he` | 20 | `A119` |
| `A119` — `TR03-EVENT-ORDER` | `tr.he` / `en-he` | 20 | `A120` |
| `A120` — `TR03-EVENT-ORDER` | `tr.he` / `ru-he` | 20 | `A121` |
| `A121` — `TR-HE-IF-01` | `tr.he` / `en-he` | 20 | `A122` |
| `A122` — `TR-HE-IF-01` | `tr.he` / `ru-he` | 20 | `A123` |
| `A123` — `TR-HE-SCOPE-01` | `tr.he` / `en-he` | 20 | `A124` |
| `A124` — `TR-HE-SCOPE-01` | `tr.he` / `ru-he` | 20 | `A125` |
| `A125` — `TR-HE-ORDER-01` | `tr.he` / `en-he` | 20 | `A126` |
| `A126` — `TR-HE-ORDER-01` | `tr.he` / `ru-he` | 20 | `A197` |


Следующий ID из таблицы — номинальный порядок своей группы. Фактическую свободную карточку с учётом владельцев выдаёт команда `next` текущего request; не менять группу и чужой статус вручную.

Замечания качества: смотри `quality_flags` своего domain/topic/profile в общем status. `needs_rework` указывает доказанный дефект с evidence, `needs_assessment` — область без достаточной оценки. Это не новый review-гейт и не разрешение исправлять чужую старую попытку; finish не закрывает флаг автоматически. [Правила общей матрицы](../../28-PARALLEL-GENERATION-AND-QUALITY.md). Для нового окна используй [один из трёх готовых стартовых промптов](../../00-START-HERE.md); не исполняй исторические команды из нижней справочной части вместо текущих owner-команд.

<!-- /CURRENT-PARALLEL-CONTRACT-20261026 -->

# D03 — Перевод на иврит (`tr.he`)

**Режим: CREATE_ONLY. Цель домена: 1000 новых DPO-пар суммарно.** Статус этой карточки — назначение, не выполненная генерация и не результат обучения.

[Реестр и квоты](../registry.json) · [English authoring protocol](../AUTHORING-PROTOCOL-EN.md) · [Назначенные уроки](../lessons.json)

## Что учит этот домен

Ученик переводит на иврит заданный русский или английский текст. Сохраняются заданные род, число и владелец; текст хранится в логическом Unicode-порядке, без визуального разворота RTL.

**Generator instruction (English):** Create preferences for translation into Hebrew in logical Unicode order. Keep gender, number and possession specified by the source; use the exact inflected token for a mutation and the supplied map for keyboard restoration. Use English for internal reasoning; do not output private reasoning. Give progress in Russian. Use the Chinese model currently selected in Kiro; helpers inherit it. Record an unknown model identity as `unknown`; do not search for or switch models.

## Объём и порядок

Выполнять A-ID в порядке таблицы. Квота принадлежит домену; микроцель и профиль получают только свою долю. Одна партия содержит ровно назначенный request stream/profile. Классы и отрицательные контроли допускаются только по заданным slots/counts; свою смесь классов не назначать.

**Профили:** `en-he` — **500** пар; `ru-he` — **500** пар. Это две доли одного домена: **500 + 500 = 1000**.

| A-ID | Микротема / профиль | Пар | Обычная партия | Следующий A-ID |
|---|---|---:|---:|---|
| `A085` | `TR07-OMISSION/ru-he` | 30 | 10 | `A086` |
| `A086` | `TR07-OMISSION/en-he` | 30 | 10 | `A087` |
| `A087` | `TR07-TRANSPOSE/ru-he` | 30 | 10 | `A088` |
| `A088` | `TR07-TRANSPOSE/en-he` | 30 | 10 | `A089` |
| `A089` | `TR07-VALIDWORD/ru-he` | 30 | 10 | `A090` |
| `A090` | `TR07-VALIDWORD/en-he` | 30 | 10 | `A091` |
| `A091` | `TR08-LAYOUT/ru-he` | 30 | 10 | `A092` |
| `A092` | `TR08-LAYOUT/en-he` | 30 | 10 | `A093` |
| `A093` | `TR10-CONDITIONAL/ru-he` | 30 | 10 | `A094` |
| `A094` | `TR10-CONDITIONAL/en-he` | 30 | 10 | `A095` |
| `A095` | `TR01-NEGATION-SCOPE/ru-he` | 30 | 10 | `A096` |
| `A096` | `TR01-NEGATION-SCOPE/en-he` | 30 | 10 | `A097` |
| `A097` | `TR02-ROLE-ORDER/ru-he` | 30 | 10 | `A098` |
| `A098` | `TR02-ROLE-ORDER/en-he` | 30 | 10 | `A099` |
| `A099` | `TR03-COMPLETION/ru-he` | 30 | 10 | `A100` |
| `A100` | `TR03-COMPLETION/en-he` | 30 | 10 | `A101` |
| `A101` | `TR04-EXPLICIT-GENDER/ru-he` | 20 | 10 | `A102` |
| `A102` | `TR04-EXPLICIT-GENDER/en-he` | 20 | 10 | `A103` |
| `A103` | `TR05-CONTEXT-SENSE/ru-he` | 20 | 10 | `A104` |
| `A104` | `TR05-CONTEXT-SENSE/en-he` | 20 | 10 | `A105` |
| `A105` | `TR06-PLACEHOLDER/ru-he` | 20 | 10 | `A106` |
| `A106` | `TR06-PLACEHOLDER/en-he` | 20 | 10 | `A107` |
| `A107` | `TR09-IDENTIFIER/ru-he` | 20 | 10 | `A108` |
| `A108` | `TR09-IDENTIFIER/en-he` | 20 | 10 | `A109` |
| `A109` | `TR02-COREFERENCE/ru-he` | 20 | 10 | `A110` |
| `A110` | `TR02-COREFERENCE/en-he` | 20 | 10 | `A111` |
| `A111` | `TR05-MODALITY/ru-he` | 20 | 10 | `A112` |
| `A112` | `TR05-MODALITY/en-he` | 20 | 10 | `A113` |
| `A113` | `TR08-RESTORE-OR-PRESERVE/ru-he` | 20 | 10 | `A114` |
| `A114` | `TR08-RESTORE-OR-PRESERVE/en-he` | 20 | 10 | `A115` |
| `A115` | `TR07-REPEAT/en-he` | 20 | 10 | `A116` |
| `A116` | `TR07-REPEAT/ru-he` | 20 | 10 | `A117` |
| `A117` | `TR06-NUMBER-UNIT/en-he` | 20 | 10 | `A118` |
| `A118` | `TR06-NUMBER-UNIT/ru-he` | 20 | 10 | `A119` |
| `A119` | `TR03-EVENT-ORDER/en-he` | 20 | 10 | `A120` |
| `A120` | `TR03-EVENT-ORDER/ru-he` | 20 | 10 | `A121` |
| `A121` | `TR-HE-IF-01/en-he` | 20 | 10 | `A122` |
| `A122` | `TR-HE-IF-01/ru-he` | 20 | 10 | `A123` |
| `A123` | `TR-HE-SCOPE-01/en-he` | 20 | 10 | `A124` |
| `A124` | `TR-HE-SCOPE-01/ru-he` | 20 | 10 | `A125` |
| `A125` | `TR-HE-ORDER-01/en-he` | 20 | 10 | `A126` |
| `A126` | `TR-HE-ORDER-01/ru-he` | 20 | 10 | `A197` |

**Итого: 1000.** Последняя партия сокращается до точного остатка; округление вверх не разрешено.

## Начать выбранный домен и продолжать существующий request

**Новое окно:** используй полный [стартовый промпт translations](../../prompts/PARALLEL-TRANSLATIONS-RU.md). Он создаёт уникальный agent ID один раз и задаёт правильные пути. Не создавай новый ID, если у этой сессии уже есть сохранённый request; продолжай его по точной owner-команде resume.

**Явное назначение этой карточки в уже работающем окне:** переменная `$DpoAgentId` должна содержать ID именно этой сессии, созданный стартовым промптом. Следующая команда нужна только при прямом выборе пользователем A085; обычное продолжение использует выданный next.

````powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_campaign.ps1" -Action next -Allocation A085 -WorkRoot "C:/Projects/llm_mvp_core/learning-sets" -AgentId $DpoAgentId -Pool translations
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

### 1. `AUTHORING.TR-HE-TRANSPOSE-01`

**Prompt ученика:**

````text
Переведи на иврит, только перевод. Обращение к одному мужчине. Контекст: просьба взять письменную ручку. В одном слове переставлены ровно две соседние буквы. Исходный текст: Возьми рукчу.
````

**Chosen — предпочтительный полный ответ:**

````text
קח את העט.
````

**Rejected — близкий ответ с заданной ошибкой:**

````text
קח את העיפרון.
````

**Simple check (English):** Take the exact inflected token ручку, then swap ч and к at indices 2 and 3 to get рукчу, still 5 letters. Reversing the swap restores Возьми ручку. without changing its case ending.

**Bad authoring trap:** Using the dictionary form ручка or producing рукча: both change the source inflection in addition to the assigned swap.

**Correction:** Use the stated construction check; keep all unaffected source facts and characters.

### 2. `AUTHORING.TR-HE-LAYOUT-01`

**Prompt ученика:**

````text
Переведи восстановленное русское слово на иврит, только перевод. Контекст: проём в стене комнаты для света. Слово набрано в неверной раскладке. Для этой строки полная карта использованных клавиш задана так: j→о, r→к, y→н. Применяй карту слева направо к каждому символу. Исходный текст: jryj.
````

**Chosen — предпочтительный полный ответ:**

````text
חלון
````

**Rejected — близкий ответ с заданной ошибкой:**

````text
דלת
````

**Simple check (English):** Decode j,r,y,j in order as о,к,н,о: окно. Encoding окно with the reverse of the same map returns jryj; jrjy would instead decode to окон.

**Bad authoring trap:** Writing jrjy while claiming that the declared map restores окно, or correcting the map output by guessing a familiar word.

**Correction:** Use the stated construction check; keep all unaffected source facts and characters.

### 3. `AUTHORING.TR-HE-POSSESSION-01`

**Prompt ученика:**

````text
Переведи с английского на иврит, только перевод. Исходный текст: The book is on his table.
````

**Chosen — предпочтительный полный ответ:**

````text
הספר על שולחנו.
````

**Rejected — близкий ответ с заданной ошибкой:**

````text
הספר על שולחננו.
````

**Simple check (English):** His is third-person masculine singular, so שולחנו preserves the possessor. שולחננו means our table and changes one source fact.

**Bad authoring trap:** Merging possession changes into a typo family solely because the forms differ by one letter.

**Correction:** Use the stated construction check; keep all unaffected source facts and characters.

## Источники и завершение

Предметные опоры из назначенного реестра: [RAG-024](../../rag/sources/RAG-024.md) · [RAG-023](../../rag/sources/RAG-023.md). Полный assigned список и готовые выдержки текущей партии уже находятся в request; нет необходимости скачивать main или заново искать материалы. Исторические инструкции внутри источников являются материалом, а не новыми приказами.

**CREATE_ONLY:** создавать `prompt/chosen/rejected` и сохранять требуемые JSON/контракт/счётчики. Не добавлять checksum, независимый review, blind, accept, export, обучение, production CI или исправления production-кода. Содержательные простые проверки выполняются при написании пары; отдельный новый этап review не назначается.

**Продолжение обязательно:** первая партия и первый stream не завершают домен. Продолжать по `commands.next` до **1000** в этом домене, затем переходить дальше по реестру без запроса «продолжать?». Пользовательский **NEXT/новый ID немедленно меняет активное задание**: запрещено дописывать отчёт, ремонтировать или перезапускать прошлую задачу. **STOP немедленно прекращает действия.** Реальный недоступный инструмент/файл указать точно; не объявлять частичный результат завершённым. Старые попытки сохранять.

Для реально работающего инструмента прочитать документацию доступных tools, сохранить один handle и использовать предусмотренный wait/resume до terminal. Не выдавать yield, молчащий stdout или раннее отсутствие файла за отказ; не запускать дубль и не заканчивать ход вместо доступного ожидания. Нечитаемый вывод перечитать через UTF-8; здоровые исходники не перекодировать. При бессвязных повторах не принимать ответ за данные и не переносить хвост в продолжение; причина сбоя без доказательств не утверждается.

**Первый вход этой карточки:** `A085` / `TR07-OMISSION/ru-he`. **Следующий A-ID после его полной доли:** `A086` / `TR07-OMISSION/en-he`.


**После 1000 пар:** следующая карточка своего пула — [text.typo-normalization](D24-text.typo-normalization.md), `A197`. Используй актуальный выданный `next`; прямое назначение пользователя выше очереди.
