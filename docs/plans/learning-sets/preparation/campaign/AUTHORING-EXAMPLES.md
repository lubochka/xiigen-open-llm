# Примеры авторинга DPO для 21 домена

**Статус: публичные reference-примеры. Это не партии китайской модели, не принятый TRAIN и не результат обучения.**

63 пары: по три на каждый фактический `domain_id`. Цель кампании — 21 × 1000 = 21 000 кандидатных пар; темы и профили распределяют эту квоту. Эти примеры в квоту не входят.

Самопроверка ниже является частью составления строки. Отдельные review, обучение и runtime-проверки здесь не запускаются. `chosen` и `rejected` показывают корректный и ошибочный ответ на один видимый `prompt`; ловушка авторинга объясняет, как нельзя построить саму пару.

## Общие инструкции генератору

- Use English for generator instructions and private reasoning; give the user a concise Russian progress report. Never print private chain-of-thought.
- Write prompt, chosen and rejected as specified by the selected domain and profile. Keep explanation, source IDs and correction metadata outside learner-visible text.
- Treat each self-check as part of composing the current row. Do not start a separate review, training, runtime test, checksum campaign or production gate.
- The examples demonstrate authoring logic. Do not copy them, their paraphrases or translations into an independent TRAIN/EXAM family under a new ID.
- Use only assigned project sources or explicitly approved MIT/Apache-2.0 sources; a project methodology source is not measured translation gold.
- Keep lexical family, scenario origin and split group distinct. Typos and keyboard forms of one intended word may share its lexical family; two valid words or different grammatical meanings are not merged by edit distance.
- Do not turn a counterexample or authoring trap into a new class quota. Follow the request exactly and proceed to its next task after completing this batch.

## Один исходный сценарий в трёх направлениях

Семья `reference.authoring.book-on-his-table.v1`: книга находится на столе мужчины. Примеры `TR-EN-POSSESSION-01`, `TR-RU-POSSESSION-01`, `TR-HE-POSSESSION-01` сохраняют этот сценарий. `שולחנו` — его стол; `שולחננו` — наш стол. Совпадающий F-номер без явно заданного сценария не доказывает родство.

## tr.en

**Generator instruction (English):** Create preferences for translation into English. Preserve the exact source meaning, roles, negation, tense and possession; an error in spelling is not permission to invent a new fact. Use only the assigned mutation and target language.

### AUTHORING.TR-EN-DELETE-01

Objective: `TR07-OMISSION` · profile: `ru-en` · error: `single_deletion_wrong_lexeme`

**Prompt**

Переведи на английский, только перевод. В одном слове пропущена ровно одна буква. Контекст: бумажная почта родственнице. Исходный текст: Она отправила писмо бабушке.

**Chosen**

```text
She sent a letter to her grandmother.
```

**Rejected**

```text
She sent a parcel to her grandmother.
```

**Simple check (English):** письмо has 6 code points; delete ь at zero-based index 3 to obtain писмо with 5. Reinserting ь restores the exact source sentence; rejected changes only the mailed object.

**Authoring trap:** Calling письмо → пильмо a deletion: both have length 6 and с was replaced with л.

**Reference source:** `prompts/TYPO-AUTHORING-EXAMPLES-RU.md`; `RAG-024`, `RAG-023`. Полные локальные и upstream-пути — в JSON.

### AUTHORING.TR-EN-KEEP-01

Objective: `TR07-VALIDWORD` · profile: `ru-en` · error: `valid_word_overcorrected`

**Prompt**

Переведи на английский, только перевод. Исходный текст записан правильно, опечатка не заявлена: Она упомянула меч.

**Chosen**

```text
She mentioned a sword.
```

**Rejected**

```text
She mentioned a ball.
```

**Simple check (English):** меч is the correctly written source token and means sword. A one-letter neighbor мяч is a different word; similarity does not authorize replacement.

**Authoring trap:** Changing the visible input to мяч and silently turning a preserve-valid-word lesson into a typo-restoration lesson.

**Reference source:** `prompts/TYPO-AUTHORING-EXAMPLES-RU.md`; `RAG-024`, `RAG-023`. Полные локальные и upstream-пути — в JSON.

### AUTHORING.TR-EN-POSSESSION-01

Objective: `TR04` · profile: `he-en` · error: `possessor_changed`

**Prompt**

Переведи с иврита на английский, только перевод. Исходный текст: הספר על שולחנו.

**Chosen**

```text
The book is on his table.
```

**Rejected**

```text
The book is on our table.
```

**Simple check (English):** שולחנו means his table; our table is שולחננו. Preserve the book, location and possessor, and change none of them in chosen.

**Authoring trap:** Treating שולחנו and שולחננו as interchangeable spelling variants despite different possessors.

**Reference source:** `prompts/TYPO-AUTHORING-EXAMPLES-RU.md`; `RAG-024`, `RAG-023`. Полные локальные и upstream-пути — в JSON.

## tr.ru

**Generator instruction (English):** Create preferences for translation into Russian. Choose one semantic defect in rejected; a valid synonym is not a defect. Preserve correct source words unless the visible task explicitly declares a recoverable typo.

### AUTHORING.TR-RU-DELETE-01

Objective: `TR07-OMISSION` · profile: `en-ru` · error: `single_deletion_wrong_lexeme`

**Prompt**

Переведи на русский, только перевод. В одном слове пропущена ровно одна буква. Контекст: бумажное послание брату. Исходный текст: She wrote a leter to her brother.

**Chosen**

```text
Она написала письмо брату.
```

**Rejected**

```text
Она написала открытку брату.
```

**Simple check (English):** letter has 6 code points; deleting the t at index 3 produces leter with 5. Reinsert that t and confirm the whole source sentence is unchanged elsewhere.

**Authoring trap:** Accepting a same-length replacement as deletion or changing the recipient while repairing the token.

**Reference source:** `prompts/TYPO-AUTHORING-EXAMPLES-RU.md`; `RAG-024`, `RAG-023`. Полные локальные и upstream-пути — в JSON.

### AUTHORING.TR-RU-KEEP-01

Objective: `TR07-VALIDWORD` · profile: `en-ru` · error: `valid_word_overcorrected`

**Prompt**

Переведи на русский, только перевод. Исходный текст записан правильно, опечатка не заявлена: She mentioned a sword.

**Chosen**

```text
Она упомянула меч.
```

**Rejected**

```text
Она упомянула слово.
```

**Simple check (English):** The source is sword, not word. Deleting s would make a different valid word, but the task requests translation of the unchanged source.

**Authoring trap:** Adding five restore-damaged examples merely because the card shows a counterexample; the assigned class is preserve-valid-word.

**Reference source:** `prompts/TYPO-AUTHORING-EXAMPLES-RU.md`; `RAG-024`, `RAG-023`. Полные локальные и upstream-пути — в JSON.

### AUTHORING.TR-RU-POSSESSION-01

Objective: `TR04` · profile: `he-ru` · error: `possessor_changed`

**Prompt**

Переведи с иврита на русский, только перевод. Исходный текст: הספר על שולחנו.

**Chosen**

```text
Книга на его столе.
```

**Rejected**

```text
Книга на нашем столе.
```

**Simple check (English):** Read the exact ending: שולחנו means his table, whereas שולחננו means our table. Use the same book-on-his-table origin as the English and Hebrew reference counterparts.

**Authoring trap:** Using the same F-number for an unrelated milk scenario, or changing the possessor to make a smoother translation.

**Reference source:** `prompts/TYPO-AUTHORING-EXAMPLES-RU.md`; `RAG-024`, `RAG-023`. Полные локальные и upstream-пути — в JSON.

## tr.he

**Generator instruction (English):** Create preferences for translation into Hebrew in logical Unicode order. Keep gender, number and possession specified by the source; use the exact inflected token for a mutation and the supplied map for keyboard restoration.

### AUTHORING.TR-HE-TRANSPOSE-01

Objective: `TR07-TRANSPOSE` · profile: `ru-he` · error: `adjacent_transposition_wrong_lexeme`

**Prompt**

Переведи на иврит, только перевод. Обращение к одному мужчине. Контекст: просьба взять письменную ручку. В одном слове переставлены ровно две соседние буквы. Исходный текст: Возьми рукчу.

**Chosen**

```text
קח את העט.
```

**Rejected**

```text
קח את העיפרון.
```

**Simple check (English):** Take the exact inflected token ручку, then swap ч and к at indices 2 and 3 to get рукчу, still 5 letters. Reversing the swap restores Возьми ручку. without changing its case ending.

**Authoring trap:** Using the dictionary form ручка or producing рукча: both change the source inflection in addition to the assigned swap.

**Reference source:** `prompts/TYPO-AUTHORING-EXAMPLES-RU.md`; `RAG-024`, `RAG-023`. Полные локальные и upstream-пути — в JSON.

### AUTHORING.TR-HE-LAYOUT-01

Objective: `TR08-LAYOUT` · profile: `ru-he` · error: `layout_wrong_lexeme`

**Prompt**

Переведи восстановленное русское слово на иврит, только перевод. Контекст: проём в стене комнаты для света. Слово набрано в неверной раскладке. Для этой строки полная карта использованных клавиш задана так: j→о, r→к, y→н. Применяй карту слева направо к каждому символу. Исходный текст: jryj.

**Chosen**

```text
חלון
```

**Rejected**

```text
דלת
```

**Simple check (English):** Decode j,r,y,j in order as о,к,н,о: окно. Encoding окно with the reverse of the same map returns jryj; jrjy would instead decode to окон.

**Authoring trap:** Writing jrjy while claiming that the declared map restores окно, or correcting the map output by guessing a familiar word.

**Reference source:** `prompts/TYPO-AUTHORING-EXAMPLES-RU.md`; `RAG-024`, `RAG-023`. Полные локальные и upstream-пути — в JSON.

### AUTHORING.TR-HE-POSSESSION-01

Objective: `TR04` · profile: `en-he` · error: `possessor_changed`

**Prompt**

Переведи с английского на иврит, только перевод. Исходный текст: The book is on his table.

**Chosen**

```text
הספר על שולחנו.
```

**Rejected**

```text
הספר על שולחננו.
```

**Simple check (English):** His is third-person masculine singular, so שולחנו preserves the possessor. שולחננו means our table and changes one source fact.

**Authoring trap:** Merging possession changes into a typo family solely because the forms differ by one letter.

**Reference source:** `prompts/TYPO-AUTHORING-EXAMPLES-RU.md`; `RAG-024`, `RAG-023`. Полные локальные и upstream-пути — в JSON.

## or.routing

**Generator instruction (English):** Choose only a route or clarify from the visible closed catalog. Classify the requested action, not a stack keyword. Return the two-field routing JSON; do not perform the translation, documentation or coding task.

### AUTHORING.ROUTE-INTENT-006

Objective: `OR-ROUTE-INTENT` · profile: `base` · error: `docs_vs_code`

**Prompt**

Каталог допустимых получателей: {"tr.en": "перевести данный текст на английский", "tr.ru": "перевести данный текст на русский", "tr.he": "перевести данный текст на иврит", "docs.csharp": "объяснить данный код C#/.NET", "docs.sql": "объяснить данный SQL", "docs.angular": "объяснить данный Angular-код", "code.csharp": "написать C#/.NET код", "code.sql": "написать SQL", "code.angular": "написать Angular-код", "or.functional": "сформулировать функциональные требования", "or.architecture": "выбрать границы архитектуры", "or.code": "спланировать следующий шаг реализации", "or.database": "спланировать изменение базы данных", "or.client": "определить поведение клиента", "or.server": "определить порядок серверной обработки", "or.mobile": "спланировать мобильную синхронизацию", "or.design": "выбрать UI-поверхность", "or.ux": "выбрать понятное действие пользователя", "or.qa": "спланировать проверку результата", "or.automation": "управлять исполнением автоматизированного запуска"}. Если намерение не покрыто каталогом или недостаёт решающей информации, вернуть {"action":"clarify","domain_id":null}. Для выбора маршрута достаточно однозначного предметного намерения; отсутствие кода или данных для исполнения самой предметной задачи не требует clarify, если маршрут уже определён. Если запрос содержит несколько самостоятельных предметных действий, вернуть {"action":"clarify","domain_id":null} даже при заданном порядке «сначала/затем»: этот атомарный router не извлекает первый шаг и не строит граф. Каталог закрытый. Учебная задача — выбрать маршрут, предметный ответ выполнять не нужно.
Объясни, какие строки возвращает приведённый SQL JOIN.
Формат ответа: JSON action=route|clarify и domain_id; ни перевод, ни код, ни ответ документации.

**Chosen**

```text
{"action":"route","domain_id":"docs.sql"}
```

**Rejected**

```text
{"action":"route","domain_id":"code.sql"}
```

**Simple check (English):** The requested action is explain SQL behavior, so choose docs.sql. A missing snippet may block later execution, but it does not make this intent ambiguous.

**Authoring trap:** Route by the word SQL alone, return subject code from a routing lesson, or extract the first action from a compound request.

**Reference source:** `curriculum/routing.json`; `RAG-006`, `RAG-031`, `RAG-033`. Полные локальные и upstream-пути — в JSON.

### AUTHORING.ROUTE-INTENT-024

Objective: `OR-ROUTE-INTENT` · profile: `base` · error: `intent_vs_keyword`

**Prompt**

Каталог допустимых получателей: {"tr.en": "перевести данный текст на английский", "tr.ru": "перевести данный текст на русский", "tr.he": "перевести данный текст на иврит", "docs.csharp": "объяснить данный код C#/.NET", "docs.sql": "объяснить данный SQL", "docs.angular": "объяснить данный Angular-код", "code.csharp": "написать C#/.NET код", "code.sql": "написать SQL", "code.angular": "написать Angular-код", "or.functional": "сформулировать функциональные требования", "or.architecture": "выбрать границы архитектуры", "or.code": "спланировать следующий шаг реализации", "or.database": "спланировать изменение базы данных", "or.client": "определить поведение клиента", "or.server": "определить порядок серверной обработки", "or.mobile": "спланировать мобильную синхронизацию", "or.design": "выбрать UI-поверхность", "or.ux": "выбрать понятное действие пользователя", "or.qa": "спланировать проверку результата", "or.automation": "управлять исполнением автоматизированного запуска"}. Если намерение не покрыто каталогом или недостаёт решающей информации, вернуть {"action":"clarify","domain_id":null}. Для выбора маршрута достаточно однозначного предметного намерения; отсутствие кода или данных для исполнения самой предметной задачи не требует clarify, если маршрут уже определён. Если запрос содержит несколько самостоятельных предметных действий, вернуть {"action":"clarify","domain_id":null} даже при заданном порядке «сначала/затем»: этот атомарный router не извлекает первый шаг и не строит граф. Каталог закрытый. Учебная задача — выбрать маршрут, предметный ответ выполнять не нужно.
Напиши SQL-запрос по контракту: SELECT всех активных пользователей из Users(Id,Active); диалект T-SQL 2019. Объяснение не требуется.
Верни только JSON с action и domain_id.

**Chosen**

```text
{"action":"route","domain_id":"code.sql"}
```

**Rejected**

```text
{"action":"route","domain_id":"docs.sql"}
```

**Simple check (English):** The requested action is write SQL, so choose code.sql. The router returns a route JSON rather than a SELECT or an explanation.

**Authoring trap:** Route by the word SQL alone, return subject code from a routing lesson, or extract the first action from a compound request.

**Reference source:** `curriculum/routing.json`; `RAG-006`, `RAG-031`, `RAG-033`. Полные локальные и upstream-пути — в JSON.

### AUTHORING.ROUTE-INTENT-025

Objective: `OR-ROUTE-INTENT` · profile: `base` · error: `unsupported_or_compound`

**Prompt**

Каталог допустимых получателей: {"tr.en": "перевести данный текст на английский", "tr.ru": "перевести данный текст на русский", "tr.he": "перевести данный текст на иврит", "docs.csharp": "объяснить данный код C#/.NET", "docs.sql": "объяснить данный SQL", "docs.angular": "объяснить данный Angular-код", "code.csharp": "написать C#/.NET код", "code.sql": "написать SQL", "code.angular": "написать Angular-код", "or.functional": "сформулировать функциональные требования", "or.architecture": "выбрать границы архитектуры", "or.code": "спланировать следующий шаг реализации", "or.database": "спланировать изменение базы данных", "or.client": "определить поведение клиента", "or.server": "определить порядок серверной обработки", "or.mobile": "спланировать мобильную синхронизацию", "or.design": "выбрать UI-поверхность", "or.ux": "выбрать понятное действие пользователя", "or.qa": "спланировать проверку результата", "or.automation": "управлять исполнением автоматизированного запуска"}. Если намерение не покрыто каталогом или недостаёт решающей информации, вернуть {"action":"clarify","domain_id":null}. Для выбора маршрута достаточно однозначного предметного намерения; отсутствие кода или данных для исполнения самой предметной задачи не требует clarify, если маршрут уже определён. Если запрос содержит несколько самостоятельных предметных действий, вернуть {"action":"clarify","domain_id":null} даже при заданном порядке «сначала/затем»: этот атомарный router не извлекает первый шаг и не строит граф. Каталог закрытый. Учебная задача — выбрать маршрут, предметный ответ выполнять не нужно.
Переведи текст на иврит и затем спланируй автоматическую публикацию. Порядок действий задан словом «затем»; контракт этого атомарного роутера требует clarify для любого запроса с несколькими самостоятельными предметными действиями.
Верни только JSON с action и domain_id.

**Chosen**

```text
{"action":"clarify","domain_id":null}
```

**Rejected**

```text
{"action":"route","domain_id":"tr.he"}
```

**Simple check (English):** There are two independent actions: translation and publication planning. This atomic router explicitly requires clarify for compound requests, even when their order is given.

**Authoring trap:** Route by the word SQL alone, return subject code from a routing lesson, or extract the first action from a compound request.

**Reference source:** `curriculum/routing.json`; `RAG-006`, `RAG-031`, `RAG-033`. Полные локальные и upstream-пути — в JSON.

## or.functional

**Generator instruction (English):** Return one observable acceptance criterion for the stated case. Use the visible bounds and role permissions. Do not add an implementation, new feature or extra acceptance case.

### AUTHORING.OR-FUNC-ACCEPTANCE-REF-01

Objective: `OR-FUNC-ACCEPTANCE` · profile: `base` · error: `inclusive_boundary_inverted`

**Prompt**

Симуляция. В форме разрешены от 1 до 5 вложений включительно. Рассматривается только количество, все файлы допустимы. Запиши один критерий для пяти вложений в JSON given, when, then; не добавляй другие функции.

**Chosen**

```text
{"given":"Выбрано ровно 5 допустимых вложений","when":"Пользователь отправляет форму","then":"Форма принимает 5 вложений"}
```

**Rejected**

```text
{"given":"Выбрано ровно 5 допустимых вложений","when":"Пользователь отправляет форму","then":"Форма отклоняет 5 вложений"}
```

**Simple check (English):** Check the inclusive interval 1 through 5. The value 5 is allowed; do not turn <=5 into <5.

**Authoring trap:** Turning a visible boundary or permission into a different requirement.

**Reference source:** `curriculum/orchestration.json`; `RAG-004`, `RAG-005`, `RAG-OR-001`. Полные локальные и upstream-пути — в JSON.

### AUTHORING.OR-FUNC-ACCEPTANCE-REF-02

Objective: `OR-FUNC-ACCEPTANCE` · profile: `base` · error: `invented_export_side_effect`

**Prompt**

Симуляция. Экспорт списка должен создавать файл только при наличии хотя бы одной строки. Пустой результат должен показывать сообщение «Нет строк для экспорта», не создавая файл. Составь один критерий для пустого списка: JSON given, when, then.

**Chosen**

```text
{"given":"Список не содержит строк","when":"Пользователь нажимает экспорт","then":"Показано «Нет строк для экспорта»; файл не создан"}
```

**Rejected**

```text
{"given":"Список не содержит строк","when":"Пользователь нажимает экспорт","then":"Показано «Нет строк для экспорта»; пустой файл создан"}
```

**Simple check (English):** Read the empty-list branch: message and no file. Keep both parts; adding an empty file is an extra effect.

**Authoring trap:** Turning a visible boundary or permission into a different requirement.

**Reference source:** `curriculum/orchestration.json`; `RAG-004`, `RAG-005`, `RAG-OR-001`. Полные локальные и upstream-пути — в JSON.

### AUTHORING.OR-FUNC-ACCEPTANCE-REF-03

Objective: `OR-FUNC-ACCEPTANCE` · profile: `base` · error: `ungranted_role_capability`

**Prompt**

Симуляция. Правило редактирования: только автор может изменять свой комментарий; модератор без авторства может скрывать, но не редактировать текст. Пользователь M — модератор, автор комментария U. Нужен критерий попытки M изменить текст; JSON given, when, then.

**Chosen**

```text
{"given":"M не является автором комментария U","when":"M пытается сохранить изменённый текст","then":"Изменение отклонено; исходный текст сохранён"}
```

**Rejected**

```text
{"given":"M не является автором комментария U","when":"M пытается сохранить изменённый текст","then":"Изменение принято; новый текст сохранён"}
```

**Simple check (English):** Check authorship separately from moderation. M can hide but cannot edit this other user's text.

**Authoring trap:** Turning a visible boundary or permission into a different requirement.

**Reference source:** `curriculum/orchestration.json`; `RAG-004`, `RAG-005`, `RAG-OR-001`. Полные локальные и upstream-пути — в JSON.

## or.architecture

**Generator instruction (English):** Select the component or boundary that satisfies the stated guarantees and tie-break criteria. Preserve the given ownership and costs. Do not assume that a mutex provides FIFO or that more features are always better.

### AUTHORING.OR-ARCH-FIFO-REF-01

Objective: `OR-ARCH-FIFO` · profile: `base` · error: `mutex_as_fifo`

**Prompt**

Для допуска участников нужен строгий порядок прихода и не больше одного допуска одновременно. Component M даёт только mutex: первым его может получить любой ожидающий. Component Q выдаёт монотонный sequence при входе и атомарно извлекает минимальный sequence одним consumer. Назначь владельца гарантии порядка: M или Q.

**Chosen**

```text
{"owner":"Q"}
```

**Rejected**

```text
{"owner":"M"}
```

**Simple check (English):** List the required guarantees: one admission at a time and arrival order. Only Q explicitly provides both.

**Authoring trap:** Choosing a named component from a slogan instead of its explicit guarantee and selection criterion.

**Reference source:** `curriculum/orchestration.json`; `RAG-006`, `RAG-050`. Полные локальные и upstream-пути — в JSON.

### AUTHORING.OR-ARCH-FIFO-REF-02

Objective: `OR-ARCH-FIFO` · profile: `base` · error: `unstable_tie_break`

**Prompt**

Сервер при приёме назначает неубывающий timestamp; очередь сортируется по нему. У A и B одинаковое время, но A пришёл раньше. Требование — стабильный FIFO и при равном timestamp. Q1 разрешает равенство случайно. Q2 назначает sequence при поступлении и атомарно берёт минимальную пару (timestamp,sequence). Выбери owner: Q1 или Q2.

**Chosen**

```text
{"owner":"Q2"}
```

**Rejected**

```text
{"owner":"Q1"}
```

**Simple check (English):** Use the equal-timestamp case. Q2 has an arrival sequence; Q1 has a random tie-break and cannot guarantee stable FIFO.

**Authoring trap:** Choosing a named component from a slogan instead of its explicit guarantee and selection criterion.

**Reference source:** `curriculum/orchestration.json`; `RAG-006`, `RAG-050`. Полные локальные и upstream-пути — в JSON.

### AUTHORING.OR-ARCH-FIFO-REF-03

Objective: `OR-ARCH-FIFO` · profile: `base` · error: `unneeded_ordering_cost`

**Prompt**

Требуется только не выполнять два задания одновременно; порядок заданий явно неважен. Критерий выбора — самый дешёвый подходящий компонент. M обеспечивает mutex и стоит 1 условную единицу. Q обеспечивает mutex+FIFO и стоит 4. Выбери owner M или Q.

**Chosen**

```text
{"owner":"M"}
```

**Rejected**

```text
{"owner":"Q"}
```

**Simple check (English):** Use the prompt criterion, not a universal preference: order is irrelevant and minimum cost is required, so M is sufficient and cheaper.

**Authoring trap:** Choosing a named component from a slogan instead of its explicit guarantee and selection criterion.

**Reference source:** `curriculum/orchestration.json`; `RAG-006`, `RAG-050`. Полные локальные и upstream-пути — в JSON.

## or.code

**Generator instruction (English):** Select the next implementation handoff action. The worker receives only the stated prompt and accessible files. Add missing decisive context when it is absent; dispatch when the complete contract is already present. Do not write application code in this lesson.

### AUTHORING.OR-CODE-PROMPT-CONTEXT-REF-01

Objective: `OR-CODE-PROMPT-CONTEXT` · profile: `base` · error: `missing_failure_policy`

**Prompt**

Нужно передать worker задачу сохранить черновик формы при HTTP 409. Контракт уже утверждён: draft_retention="keep". Worker видит только фактический prompt: «Обработай HTTP 409 в форме». Он не имеет доступа к файлам. Допустимые next_step: add_draft_retention, dispatch_worker. Что сделать следующим?

**Chosen**

```text
{"next_step":"add_draft_retention"}
```

**Rejected**

```text
{"next_step":"dispatch_worker"}
```

**Simple check (English):** Compare the worker-visible prompt with the required draft_retention=keep. It is missing, so add it before dispatch.

**Authoring trap:** Writing code or repeating a completed handoff instead of choosing the requested next task.

**Reference source:** `curriculum/orchestration.json`; `RAG-005`, `RAG-006`, `RAG-047`. Полные локальные и upstream-пути — в JSON.

### AUTHORING.OR-CODE-PROMPT-CONTEXT-REF-02

Objective: `OR-CODE-PROMPT-CONTEXT` · profile: `base` · error: `repeat_complete_handoff`

**Prompt**

Worker должен вызвать SaveAsync. Утверждённая сигнатура: SaveAsync(string id, CancellationToken ct). Фактический prompt содержит эту сигнатуру полностью, id="A7", ct=providedToken, ожидаемый вызов и все критерии. Worker подтвердил получение этих байтов. Допустимые next_step: dispatch_worker, repackage_same_signature. Выбери один.

**Chosen**

```text
{"next_step":"dispatch_worker"}
```

**Rejected**

```text
{"next_step":"repackage_same_signature"}
```

**Simple check (English):** Check whether the complete signature, inputs and acceptance criteria are already visible. They are; dispatch rather than repackage the same content.

**Authoring trap:** Writing code or repeating a completed handoff instead of choosing the requested next task.

**Reference source:** `curriculum/orchestration.json`; `RAG-005`, `RAG-006`, `RAG-047`. Полные локальные и upstream-пути — в JSON.

### AUTHORING.OR-CODE-PROMPT-CONTEXT-REF-03

Objective: `OR-CODE-PROMPT-CONTEXT` · profile: `base` · error: `unreachable_version_context`

**Prompt**

Карточка назначила worker SQL Server 2022. Worker не видит карточку: ему отправляется только «Напиши запрос первых 10 строк; version_profile смотри в manifest». Manifest у worker недоступен. Разные SQL-диалекты дают разные конструкции. Допустимые next_step: add_dialect_to_prompt, dispatch_worker. Верни один next_step.

**Chosen**

```text
{"next_step":"add_dialect_to_prompt"}
```

**Rejected**

```text
{"next_step":"dispatch_worker"}
```

**Simple check (English):** The worker cannot open the manifest. Put the already assigned SQL dialect in the actual prompt; do not ask the worker to guess it.

**Authoring trap:** Writing code or repeating a completed handoff instead of choosing the requested next task.

**Reference source:** `curriculum/orchestration.json`; `RAG-005`, `RAG-006`, `RAG-047`. Полные локальные и upstream-пути — в JSON.

## or.database

**Generator instruction (English):** Choose the next database coordination action under the explicit atomicity, ownership and idempotency contract. Do not assume an unmentioned transaction or uniqueness constraint.

### AUTHORING.OR-DB-ATOMIC-CLAIM-REF-01

Objective: `OR-DB-ATOMIC-CLAIM` · profile: `base` · error: `non_atomic_claim`

**Prompt**

Симуляция БД. Два worker одновременно обрабатывают ключ K. Гарантия: бизнес-запись разрешена только единственному владельцу K. В таблице ключ отсутствует. claim_unique делает атомарный INSERT с уникальностью K и возвращает выигрыш только одному; read_then_insert — раздельные чтение и запись без ограничения уникальности. Выбери следующий шаг JSON next_action из claim_unique, read_then_insert.

**Chosen**

```text
{"next_action":"claim_unique"}
```

**Rejected**

```text
{"next_action":"read_then_insert"}
```

**Simple check (English):** Consider both workers reading before either writes. Only the stated atomic unique claim permits a single winner.

**Authoring trap:** Assuming atomicity, ownership or a new operation identity that the prompt does not grant.

**Reference source:** `curriculum/orchestration.json`; `RAG-006`, `RAG-OR-001`. Полные локальные и upstream-пути — в JSON.

### AUTHORING.OR-DB-ATOMIC-CLAIM-REF-02

Objective: `OR-DB-ATOMIC-CLAIM` · profile: `base` · error: `duplicate_effect_after_completion`

**Prompt**

Симуляция БД. Для K хранится terminal=done и результат R; payload_hash совпадает с повторным запросом. Контракт повторной операции: вернуть R без новой бизнес-записи. reuse_result возвращает R; claim_new_id создаёт новую операцию с тем же payload. JSON next_action: reuse_result или claim_new_id.

**Chosen**

```text
{"next_action":"reuse_result"}
```

**Rejected**

```text
{"next_action":"claim_new_id"}
```

**Simple check (English):** Match the key and payload hash and read done. Reuse R; a new ID would duplicate the same logical effect.

**Authoring trap:** Assuming atomicity, ownership or a new operation identity that the prompt does not grant.

**Reference source:** `curriculum/orchestration.json`; `RAG-006`, `RAG-OR-001`. Полные локальные и upstream-пути — в JSON.

### AUTHORING.OR-DB-ATOMIC-CLAIM-REF-03

Objective: `OR-DB-ATOMIC-CLAIM` · profile: `base` · error: `write_without_claim`

**Prompt**

Симуляция БД. K находится в processing, owner=W1, lease действителен. Запрос обрабатывает W2. По контракту только owner с действующим lease может менять бизнес-запись; W2 должен подписаться на завершение K. wait_existing делает это; apply_business пишет данные сейчас. Верни JSON next_action.

**Chosen**

```text
{"next_action":"wait_existing"}
```

**Rejected**

```text
{"next_action":"apply_business"}
```

**Simple check (English):** Read owner=W1 and a valid lease. W2 cannot write yet, so wait for the existing operation.

**Authoring trap:** Assuming atomicity, ownership or a new operation identity that the prompt does not grant.

**Reference source:** `curriculum/orchestration.json`; `RAG-006`, `RAG-OR-001`. Полные локальные и upstream-пути — в JSON.

## or.client

**Generator instruction (English):** Map a confirmed event for the active run to its UI state. Accepted, completed-empty and completed-failed are distinct. A stale run cannot finish the active run.

### AUTHORING.OR-CLIENT-RUN-STATE-REF-01

Objective: `OR-CLIENT-RUN-STATE` · profile: `base` · error: `accepted_as_result`

**Prompt**

В интерфейсе активен run C11. API вернул 202 и {run_id:C11,state:running}; по контракту 202 означает принято в обработку, результат ещё не готов. Допустимые ui_state: in_progress, result. Верни JSON ui_state, active_run.

**Chosen**

```text
{"ui_state":"in_progress","active_run":"C11"}
```

**Rejected**

```text
{"ui_state":"result","active_run":"C11"}
```

**Simple check (English):** Read state=running for C11. The prompt defines 202 as accepted, so the UI is in_progress.

**Authoring trap:** Treating acceptance, empty success or a different run as the active run's final result.

**Reference source:** `curriculum/orchestration.json`; `RAG-001`, `RAG-003`, `RAG-048`. Полные локальные и upstream-пути — в JSON.

### AUTHORING.OR-CLIENT-RUN-STATE-REF-02

Objective: `OR-CLIENT-RUN-STATE` · profile: `base` · error: `empty_success_as_error`

**Prompt**

В интерфейсе активен run C12. Пришло {run_id:C12,state:terminal_success,items:[]}. Контракт поиска разрешает ноль совпадений; это завершённый успешный запрос. Допустимые ui_state: empty_result, error. Верни JSON.

**Chosen**

```text
{"ui_state":"empty_result","active_run":"C12"}
```

**Rejected**

```text
{"ui_state":"error","active_run":"C12"}
```

**Simple check (English):** Read terminal_success and the allowed empty result. Zero items gives empty_result, not error.

**Authoring trap:** Treating acceptance, empty success or a different run as the active run's final result.

**Reference source:** `curriculum/orchestration.json`; `RAG-001`, `RAG-003`, `RAG-048`. Полные локальные и upstream-пути — в JSON.

### AUTHORING.OR-CLIENT-RUN-STATE-REF-03

Objective: `OR-CLIENT-RUN-STATE` · profile: `base` · error: `stale_run_overwrites_current`

**Prompt**

Пользователь запустил C14 после C13; active_run=C14, его состояние in_progress. Позднее получен terminal_success для C13. Контракт допускает изменение UI только событиями active_run. Допустимые ui_state: in_progress, result. Верни JSON с active_run=C14.

**Chosen**

```text
{"ui_state":"in_progress","active_run":"C14"}
```

**Rejected**

```text
{"ui_state":"result","active_run":"C14"}
```

**Simple check (English):** Compare the event run_id C13 with active_run C14. They differ, so keep the current in_progress state.

**Authoring trap:** Treating acceptance, empty success or a different run as the active run's final result.

**Reference source:** `curriculum/orchestration.json`; `RAG-001`, `RAG-003`, `RAG-048`. Полные локальные и upstream-пути — в JSON.

## or.server

**Generator instruction (English):** Choose the next server action from the given persistence and publication contract. Distinguish pending, committed and aborted. Do not infer a successful write from an attempted call.

### AUTHORING.OR-SERVER-OUTBOX-REF-01

Objective: `OR-SERVER-OUTBOX` · profile: `base` · error: `publish_before_durable_state`

**Prompt**

Симуляция сервера. Нужно сохранить заказ D и сообщить событие E. Контракт: транзакция commit_document_and_outbox атомарно сохраняет D и pending-запись E; relay может публиковать E только после commit. Ни D, ни E ещё не сохранены. publish_first отправляет E до commit. Выбери JSON next_action из commit_document_and_outbox, publish_first.

**Chosen**

```text
{"next_action":"commit_document_and_outbox"}
```

**Rejected**

```text
{"next_action":"publish_first"}
```

**Simple check (English):** Neither record is durable yet. Commit document and pending outbox atomically before any publication.

**Authoring trap:** Treating an attempted or aborted write as a committed basis for publication.

**Reference source:** `curriculum/orchestration.json`; `RAG-006`, `RAG-OR-001`. Полные локальные и upstream-пути — в JSON.

### AUTHORING.OR-SERVER-OUTBOX-REF-02

Objective: `OR-SERVER-OUTBOX` · profile: `base` · error: `event_after_transaction_abort`

**Prompt**

Симуляция сервера. Транзакция D+outbox E вернула aborted; по контракту обе записи отсутствуют. Публикация допустима только по committed outbox. record_failure сохраняет terminal-ошибку попытки без публикации; dispatch_event публикует E сейчас. Повторный запуск этой попытки не разрешён. Верни JSON next_action.

**Chosen**

```text
{"next_action":"record_failure"}
```

**Rejected**

```text
{"next_action":"dispatch_event"}
```

**Simple check (English):** Read aborted: both records are absent by the given contract. Record failure instead of publishing an unsupported event.

**Authoring trap:** Treating an attempted or aborted write as a committed basis for publication.

**Reference source:** `curriculum/orchestration.json`; `RAG-006`, `RAG-OR-001`. Полные локальные и upstream-пути — в JSON.

### AUTHORING.OR-SERVER-OUTBOX-REF-03

Objective: `OR-SERVER-OUTBOX` · profile: `base` · error: `waiting_after_durable_precondition`

**Prompt**

Симуляция сервера. D и pending E подтверждённо committed. Relay владеет записью E, других блокировок нет. Контракт требует доставить pending E; dispatch_outbox публикует сохранённое событие, wait_for_document ждёт сохранения D. Верни JSON next_action.

**Chosen**

```text
{"next_action":"dispatch_outbox"}
```

**Rejected**

```text
{"next_action":"wait_for_document"}
```

**Simple check (English):** Read confirmed commit and pending event with an owned relay lease. Dispatch now; the persistence prerequisite is already satisfied.

**Authoring trap:** Treating an attempted or aborted write as a committed basis for publication.

**Reference source:** `curriculum/orchestration.json`; `RAG-006`, `RAG-OR-001`. Полные локальные и upstream-пути — в JSON.

## or.mobile

**Generator instruction (English):** Choose a mobile synchronization action for the exact pending operation. Local persistence and server acceptance do not mean applied. Close only a matching applied acknowledgement, and do not resend an already acknowledged operation.

### AUTHORING.OR-MOBILE-ACK-REF-01

Objective: `OR-MOBILE-ACK` · profile: `base` · error: `local_queue_as_server_ack`

**Prompt**

Симуляция мобильной синхронизации. Операция Q записана в локальную очередь, сеть offline, сервер Q не видел. Правило: удалять Q можно только по связанному ACK applied; retain_pending оставляет Q и локальные данные, close_pending удаляет Q как синхронизированную. Верни JSON next_action, operation_id=Q.

**Chosen**

```text
{"next_action":"retain_pending","operation_id":"Q"}
```

**Rejected**

```text
{"next_action":"close_pending","operation_id":"Q"}
```

**Simple check (English):** The server has not seen Q. Local queue persistence is not applied acknowledgement, so retain_pending.

**Authoring trap:** Equating local storage or queued acceptance with an applied server acknowledgement.

**Reference source:** `curriculum/orchestration.json`; `RAG-006`, `RAG-017`, `RAG-OR-003`. Полные локальные и upstream-пути — в JSON.

### AUTHORING.OR-MOBILE-ACK-REF-02

Objective: `OR-MOBILE-ACK` · profile: `base` · error: `accepted_as_applied`

**Prompt**

Симуляция мобильной синхронизации. Для операции R сервер ответил accepted, meaning=queued_not_applied. По контракту только applied закрывает локальный pending. retain_pending сохраняет ожидание R; close_pending считает R применённой. Верни JSON next_action, operation_id=R.

**Chosen**

```text
{"next_action":"retain_pending","operation_id":"R"}
```

**Rejected**

```text
{"next_action":"close_pending","operation_id":"R"}
```

**Simple check (English):** The prompt defines accepted as queued_not_applied. Keep R pending until applied is confirmed.

**Authoring trap:** Equating local storage or queued acceptance with an applied server acknowledgement.

**Reference source:** `curriculum/orchestration.json`; `RAG-006`, `RAG-017`, `RAG-OR-003`. Полные локальные и upstream-пути — в JSON.

### AUTHORING.OR-MOBILE-ACK-REF-03

Objective: `OR-MOBILE-ACK` · profile: `base` · error: `resend_after_matched_applied_ack`

**Prompt**

Симуляция мобильной синхронизации. Для операции S с payload_hash=H получен ACK {operation_id:S,payload_hash:H,status:applied}. ACK проверен и соответствует ожидаемому серверу. Правило: применённую операцию закрыть локально без повторной отправки. close_pending закрывает S; resend повторно ставит S на отправку. Верни JSON next_action, operation_id=S.

**Chosen**

```text
{"next_action":"close_pending","operation_id":"S"}
```

**Rejected**

```text
{"next_action":"resend","operation_id":"S"}
```

**Simple check (English):** Compare operation ID and payload hash, then read applied. Close S without resending it.

**Authoring trap:** Equating local storage or queued acceptance with an applied server acknowledgement.

**Reference source:** `curriculum/orchestration.json`; `RAG-006`, `RAG-017`, `RAG-OR-003`. Полные локальные и upstream-пути — в JSON.

## or.design

**Generator instruction (English):** Choose the specified UI surface for the actual role, tenant, owner and grant. Do not grant rights from a role name alone. Return only the requested panel decision.

### AUTHORING.OR-DESIGN-ROLE-PANEL-REF-01

Objective: `OR-DESIGN-ROLE-PANEL` · profile: `base` · error: `owner_mapped_to_viewer_surface`

**Prompt**

Пользователь U8 — owner документа D8 в tenant A. Политика экрана: owner своего документа видит edit_panel; viewer видит read_panel. Иных ограничений нет. Выбери panel для U8 на D8.

**Chosen**

```text
{"panel":"edit_panel"}
```

**Rejected**

```text
{"panel":"read_panel"}
```

**Simple check (English):** Match the user, document ownership and role rule. Owner U8 is editing their own D8, so use edit_panel.

**Authoring trap:** Inferring broader rights from a role name while ignoring tenant, ownership or grant.

**Reference source:** `curriculum/orchestration.json`; `RAG-003`, `RAG-048`, `RAG-049`. Полные локальные и upstream-пути — в JSON.

### AUTHORING.OR-DESIGN-ROLE-PANEL-REF-02

Objective: `OR-DESIGN-ROLE-PANEL` · profile: `base` · error: `role_name_overrides_tenant_scope`

**Prompt**

U10 — tenant_admin только tenant A. Открыт документ tenant B. Политика не допускает межтенантного чтения и предписывает access_denied_panel; admin_panel показан администратору своего tenant. Выбери panel.

**Chosen**

```text
{"panel":"access_denied_panel"}
```

**Rejected**

```text
{"panel":"admin_panel"}
```

**Simple check (English):** Compare tenant A with tenant B before using the admin role. The scope differs, so use access_denied_panel.

**Authoring trap:** Inferring broader rights from a role name while ignoring tenant, ownership or grant.

**Reference source:** `curriculum/orchestration.json`; `RAG-003`, `RAG-048`, `RAG-049`. Полные локальные и upstream-пути — в JSON.

### AUTHORING.OR-DESIGN-ROLE-PANEL-REF-03

Objective: `OR-DESIGN-ROLE-PANEL` · profile: `base` · error: `delegated_view_expanded_to_edit`

**Prompt**

U11 получил делегирование только на просмотр отчёта R11 до 18:00; сейчас 17:00, tenant совпадает. Политика: действующее view-only делегирование даёт report_read_panel, редактирование требует edit grant и report_edit_panel. Edit grant отсутствует. Выбери panel.

**Chosen**

```text
{"panel":"report_read_panel"}
```

**Rejected**

```text
{"panel":"report_edit_panel"}
```

**Simple check (English):** Read the granted operation: view only. The grant is current but contains no edit permission, so use report_read_panel.

**Authoring trap:** Inferring broader rights from a role name while ignoring tenant, ownership or grant.

**Reference source:** `curriculum/orchestration.json`; `RAG-003`, `RAG-048`, `RAG-049`. Полные локальные и upstream-пути — в JSON.

## or.ux

**Generator instruction (English):** Give the user one truthful message and a useful next action. Preserve the draft when the contract says it exists. Unknown server outcome must remain unknown until checked.

### AUTHORING.OR-UX-RECOVERY-REF-01

Objective: `OR-UX-RECOVERY` · profile: `base` · error: `retry_without_fixing_input`

**Prompt**

Сервер вернул VALIDATION: поле age должно быть целым от 1 до 120; введено 150. Черновик сохранён локально. Доступные action: edit_age, retry_unchanged. Составь одно сообщение и действие.

**Chosen**

```text
{"message":"Возраст должен быть от 1 до 120. Черновик сохранён.","action":"edit_age"}
```

**Rejected**

```text
{"message":"Возраст должен быть от 1 до 120. Черновик сохранён.","action":"retry_unchanged"}
```

**Simple check (English):** Test the unchanged value 150 against the visible maximum 120. Retrying unchanged cannot help; select edit_age.

**Authoring trap:** Giving a plausible-sounding message that invents a server outcome or loses the saved draft.

**Reference source:** `curriculum/orchestration.json`; `RAG-003`, `RAG-048`, `RAG-049`. Полные локальные и upstream-пути — в JSON.

### AUTHORING.OR-UX-RECOVERY-REF-02

Objective: `OR-UX-RECOVERY` · profile: `base` · error: `recovery_loses_saved_draft`

**Prompt**

Сессия пользователя истекла до отправки формы; сервер подтвердил NOT_SUBMITTED. Черновик D21 сохранён локально и после входа должен быть продолжен. Action: sign_in_resume_draft, sign_in_blank_form. Составь JSON.

**Chosen**

```text
{"message":"Сессия истекла. Войдите, чтобы продолжить заполнение.","action":"sign_in_resume_draft"}
```

**Rejected**

```text
{"message":"Сессия истекла. Войдите, чтобы продолжить заполнение.","action":"sign_in_blank_form"}
```

**Simple check (English):** Use the confirmed NOT_SUBMITTED outcome and saved draft. Reauthenticate while preserving that draft; do not discard it.

**Authoring trap:** Giving a plausible-sounding message that invents a server outcome or loses the saved draft.

**Reference source:** `curriculum/orchestration.json`; `RAG-003`, `RAG-048`, `RAG-049`. Полные локальные и upstream-пути — в JSON.

### AUTHORING.OR-UX-RECOVERY-REF-03

Objective: `OR-UX-RECOVERY` · profile: `base` · error: `unknown_outcome_as_failure`

**Prompt**

После отправки заявки связь прервалась. Серверный итог неизвестен; доступен check_status по request_id. Повтор до проверки может создать вторую заявку. Допустимый action=check_status в обоих ответах. Напиши сообщение без утверждения неизвестного результата.

**Chosen**

```text
{"message":"Ответ пока не получен. Проверим состояние заявки.","action":"check_status"}
```

**Rejected**

```text
{"message":"Заявка не создана. Проверим состояние заявки.","action":"check_status"}
```

**Simple check (English):** The outcome is unknown and check_status is available. The message must not assert that creation failed.

**Authoring trap:** Giving a plausible-sounding message that invents a server outcome or loses the saved draft.

**Reference source:** `curriculum/orchestration.json`; `RAG-003`, `RAG-048`, `RAG-049`. Полные локальные и upstream-пути — в JSON.

## or.qa

**Generator instruction (English):** Choose a protocol that changes only the assigned learning factor. Restore the shared parent state actually consumed by the learner. Do not require unused state, and do not present simulated numbers as measured results.

### AUTHORING.OR-QA-CAUSAL-REF-01

Objective: `OR-QA-CAUSAL` · profile: `base` · error: `different_parent_confound`

**Prompt**

Сравниваются два способа выбора negatives. Совместимый обученный snapshot S даёт 6/10 правильных ответов на калибровке. Требуется изолировать только выбор negatives. A: оба плеча клонируют полный S. B: первое плечо клонирует S, второе заново инициализируется. Данные, EXAM и бюджет одинаковы. Выбери protocol A или B.

**Chosen**

```text
{"protocol":"A"}
```

**Rejected**

```text
{"protocol":"B"}
```

**Simple check (English):** Both arms must start from the same full S. A isolates negative selection; B also changes initialization.

**Authoring trap:** Using an A/B comparison that changes both the assigned factor and a consumed parent state.

**Reference source:** `curriculum/orchestration.json`; `RAG-002`, `RAG-032`, `RAG-046`. Полные локальные и upstream-пути — в JSON.

### AUTHORING.OR-QA-CAUSAL-REF-02

Objective: `OR-QA-CAUSAL` · profile: `base` · error: `optimizer_reset_changes_parent`

**Prompt**

Обучающий consumer использует weights и optimizer moments; momentum ненулевой и влияет на следующий update. Snapshot S сохраняет оба. A: второе плечо восстанавливает только weights и обнуляет moments. B: оба плеча восстанавливают weights+moments из S. Исследуемый фактор — состав одной DPO-пары; данные/EXAM/бюджет совпадают. Выбери protocol.

**Chosen**

```text
{"protocol":"B"}
```

**Rejected**

```text
{"protocol":"A"}
```

**Simple check (English):** The prompt says moments are consumed by the next update. Restoring weights alone changes a second factor; choose B.

**Authoring trap:** Using an A/B comparison that changes both the assigned factor and a consumed parent state.

**Reference source:** `curriculum/orchestration.json`; `RAG-002`, `RAG-032`, `RAG-046`. Полные локальные и upstream-пути — в JSON.

### AUTHORING.OR-QA-CAUSAL-REF-03

Objective: `OR-QA-CAUSAL` · profile: `base` · error: `rng_state_not_restored`

**Prompt**

Исследуется один новый вариант kNN update. Consumer читает обученную память M и случайно выбирает tie-break через RNG. S сохраняет M и RNG; усвоение уже ненулевое. A: оба плеча восстанавливают M+RNG из S. B: память совпадает, но второе плечо берёт новый RNG seed. Прочие условия равны. Выбери protocol.

**Chosen**

```text
{"protocol":"A"}
```

**Rejected**

```text
{"protocol":"B"}
```

**Simple check (English):** The prompt says RNG affects tie-breaks. Restore M and RNG in both arms; a fresh seed adds a second factor.

**Authoring trap:** Using an A/B comparison that changes both the assigned factor and a consumed parent state.

**Reference source:** `curriculum/orchestration.json`; `RAG-002`, `RAG-032`, `RAG-046`. Полные локальные и upstream-пути — в JSON.

## or.automation

**Generator instruction (English):** Choose wait, read-result or retry from the confirmed state of the same run. Transport timeout is not process failure. Wait only while the process is nonterminal; a terminal failure needs its failure result, not endless waiting.

### AUTHORING.OR-AUTO-TERMINAL-REF-01

Objective: `OR-AUTO-TERMINAL` · profile: `base` · error: `partial_as_terminal`

**Prompt**

Обучающий run L41 имеет handle h41. Последнее подтверждённое состояние сервера running. progress.json содержит steps=8, но terminal.json ещё нет. API wait(h41) штатно ждёт до следующего состояния. Допустимые action: wait, read_final. Верни JSON action, run_id=L41.

**Chosen**

```text
{"action":"wait","run_id":"L41"}
```

**Rejected**

```text
{"action":"read_final","run_id":"L41"}
```

**Simple check (English):** Read the confirmed state: running. Progress is partial, so choose wait with L41; do not create a new run.

**Authoring trap:** Treating silence or transport timeout as process failure, or waiting forever after a terminal state.

**Reference source:** `curriculum/orchestration.json`; `RAG-001`, `RAG-032`, `RAG-047`. Полные локальные и upstream-пути — в JSON.

### AUTHORING.OR-AUTO-TERMINAL-REF-02

Objective: `OR-AUTO-TERMINAL` · profile: `base` · error: `transport_timeout_as_process_failure`

**Prompt**

Run L42 уже запущен. Документированный wait(h42) вернул transport_timeout через 30 секунд; последняя heartbeat сервера говорит state=running, heartbeat_age=2s. Контракт разрешает повторно ждать тот же handle; start_new разрешён только после terminal_failure. Допустимые action: wait, start_new. Верни JSON action и run_id=L42.

**Chosen**

```text
{"action":"wait","run_id":"L42"}
```

**Rejected**

```text
{"action":"start_new","run_id":"L42"}
```

**Simple check (English):** Read heartbeat=running and age=2s. The timeout belongs to transport; keep L42 and wait on the same handle.

**Authoring trap:** Treating silence or transport timeout as process failure, or waiting forever after a terminal state.

**Reference source:** `curriculum/orchestration.json`; `RAG-001`, `RAG-032`, `RAG-047`. Полные локальные и upstream-пути — в JSON.

### AUTHORING.OR-AUTO-TERMINAL-REF-03

Objective: `OR-AUTO-TERMINAL` · profile: `base` · error: `wait_after_terminal`

**Prompt**

Run L43 завершился terminal_failure до первого учебного шага: build_exit=1. failure.json полностью записан и содержит run_id=L43. Доступные action: read_failure, wait. Пользователь не отменял задачу; нужен достоверный статус этой попытки. Верни JSON action, run_id.

**Chosen**

```text
{"action":"read_failure","run_id":"L43"}
```

**Rejected**

```text
{"action":"wait","run_id":"L43"}
```

**Simple check (English):** Read terminal_failure and a complete matching failure artifact. Choose read_failure; another wait cannot restart this finished process.

**Authoring trap:** Treating silence or transport timeout as process failure, or waiting forever after a terminal state.

**Reference source:** `curriculum/orchestration.json`; `RAG-001`, `RAG-032`, `RAG-047`. Полные локальные и upstream-пути — в JSON.

## docs.csharp

**Generator instruction (English):** Explain the displayed C# code and exact inputs using its pinned version. Return documentation or requested values, not a replacement implementation. Do not invent trim, cancellation checks, catch blocks or side effects.

### AUTHORING.DOC-CS-NULL.REF.01

Objective: `DOC-CS-NULL` · profile: `base` · error: `empty_treated_as_null`

**Prompt**

Авторская симуляция. C# 10 / .NET 6, nullable enabled. Внутри класса объявлен метод: string Label(string? s) => s ?? "(нет)"; Других операций нет. Опиши результаты вызовов Label(null), Label(""), Label(" "), Label("A") в таком порядке и правило подстановки.

**Chosen**

```text
Результаты: "(нет)", "", " ", "A". Подстановка выполняется только для null; пустые и пробельные строки сохраняются.
```

**Rejected**

```text
Результаты: "(нет)", "(нет)", " ", "A". Подстановка выполняется для null и пустой строки; пробельная строка сохраняется.
```

**Simple check (English):** Compare null, empty string and a one-space string separately. Only null selects the fallback; preserve the exact other strings.

**Authoring trap:** Inventing an operation absent from the shown method or answering with replacement code.

**Reference source:** `curriculum/doc-code.json`; `RAG-027`, `RAG-028`. Полные локальные и upstream-пути — в JSON.

### AUTHORING.DOC-CS-CANCEL.REF.01

Objective: `DOC-CS-CANCEL` · profile: `base` · error: `wrong_cancellation_token`

**Prompt**

Авторская симуляция. C# 10 / .NET 6; using System; using System.Threading; using System.Threading.Tasks. Метод внутри класса: Task<int> Forward(CancellationToken ct, Func<CancellationToken, Task<int>> read) => read(ct); Делегат read записывает точные аргументы, возвращает заранее созданный Task и не бросает синхронно. Опиши, какой token передан, сколько вызовов и что возвращает wrapper. Не утверждай физическую остановку хранилища.

**Chosen**

```text
read вызывается один раз с исходным ct; возвращается тот же Task<int>. Сам wrapper не проверяет отмену и не перехватывает ошибку.
```

**Rejected**

```text
read вызывается один раз с CancellationToken.None; возвращается тот же Task<int>. Сам wrapper не проверяет отмену и не перехватывает ошибку.
```

**Simple check (English):** Trace the single call read(ct). The original token and returned Task are forwarded; no local cancellation test is present.

**Authoring trap:** Inventing an operation absent from the shown method or answering with replacement code.

**Reference source:** `curriculum/doc-code.json`; `RAG-027`, `RAG-028`, `RAG-029`. Полные локальные и upstream-пути — в JSON.

### AUTHORING.DOC-CS-AWAIT.REF.01

Objective: `DOC-CS-AWAIT` · profile: `base` · error: `wrong_async_side_effect_path`

**Prompt**

Авторская симуляция. C# 10 / .NET 6. Доступны System, System.Collections.Generic, System.Threading.Tasks. trace — List<string>, в начале пуст; trace.Add не бросает. Зависимость синхронно не бросает и возвращает pending Task. Допустимые завершения: SetResult — RanToCompletion; SetException(new InvalidOperationException()) — Faulted; SetCanceled — Canceled. Faulted Task, содержащая OperationCanceledException/TaskCanceledException, исключена из этого fixture. Внутри класса дан метод: async Task Save(Func<Task> store, List<string> trace) { await store(); trace.Add("saved"); } Опиши точную последовательность trace и outcome при success/fault/cancel; не добавляй скрытых catch.

**Chosen**

```text
При успехе trace получает saved после завершения store. При InvalidOperationException и отменённой Task запись saved не добавляется; Task Save завершается соответственно faulted/canceled.
```

**Rejected**

```text
При успехе trace получает saved после завершения store. При InvalidOperationException и отменённой Task запись saved тоже добавляется; Task Save завершается соответственно faulted/canceled.
```

**Simple check (English):** Place trace.Add after successful await. Fault and cancellation leave trace empty under this fixture.

**Authoring trap:** Inventing an operation absent from the shown method or answering with replacement code.

**Reference source:** `curriculum/doc-code.json`; `RAG-027`, `RAG-028`, `RAG-029`. Полные локальные и upstream-пути — в JSON.

## docs.sql

**Generator instruction (English):** Explain the displayed SQL and exact fixture. Preserve NULL, duplicate, join and predicate semantics. Return the requested rows or explanation; do not silently rewrite the query.

### AUTHORING.DOC-SQL-COUNT.REF.01

Objective: `DOC-SQL-COUNT` · profile: `base` · error: `invented_distinct`

**Prompt**

Авторская симуляция. T-SQL / SQL Server 2019, ANSI_NULLS ON. Выполняется только этот batch:
DECLARE @T TABLE (v int NULL); INSERT INTO @T(v) VALUES (7),(NULL),(7); SELECT COUNT(v) AS n FROM @T;
Опиши число строк результата, значение n и какие строки учитываются. Не выполняй изменения внешней БД.

**Chosen**

```text
Одна строка: n=2. Оба значения 7 учитываются, NULL исключается.
```

**Rejected**

```text
Одна строка: n=1. Повторяющееся значение 7 учитывается один раз, NULL исключается.
```

**Simple check (English):** Mark both non-NULL rows in [7,NULL,7]. There are two; DISTINCT does not appear in the given query.

**Authoring trap:** Explaining a different query by silently adding DISTINCT, moving a filter or weakening a predicate.

**Reference source:** `curriculum/doc-code.json`; `RAG-040`, `RAG-041`. Полные локальные и upstream-пути — в JSON.

### AUTHORING.DOC-SQL-LEFT.REF.01

Objective: `DOC-SQL-LEFT` · profile: `base` · error: `right_filter_removes_left_rows`

**Prompt**

Авторская симуляция. T-SQL / SQL Server 2019; ANSI_NULLS ON. Полный setup: CREATE TABLE #P (Id int NOT NULL PRIMARY KEY); CREATE TABLE #R (Id int NOT NULL PRIMARY KEY, ParentId int NOT NULL, Active bit NULL); INSERT #P VALUES (1),(2); INSERT #R VALUES (10,1,1),(11,1,0); Запрос: SELECT p.Id, r.Id AS RightId FROM #P p LEFT JOIN #R r ON r.ParentId=p.Id AND r.Active=1 ORDER BY p.Id,r.Id; Назови точные строки (Id,RightId) в заданном ORDER BY и объясни, почему родитель без подходящего ребёнка сохраняется.

**Chosen**

```text
[(1,10),(2,null)]. Родители без подходящих детей сохраняются с RightId=null.
```

**Rejected**

```text
[(1,10)]. Родители без подходящих детей исключаются.
```

**Simple check (English):** Enumerate parent 1 and parent 2. Parent 2 has no matching child and must remain with RightId=null.

**Authoring trap:** Explaining a different query by silently adding DISTINCT, moving a filter or weakening a predicate.

**Reference source:** `curriculum/doc-code.json`; `RAG-040`, `RAG-041`. Полные локальные и upstream-пути — в JSON.

### AUTHORING.DOC-SQL-GUARD.REF.01

Objective: `DOC-SQL-GUARD` · profile: `base` · error: `guard_predicate_boundary`

**Prompt**

Авторская симуляция. T-SQL / SQL Server 2019; ANSI_NULLS ON; триггеров и CHECK нет, конкурентных соединений в этом fixture нет. Setup: CREATE TABLE #Stock (Id int NOT NULL PRIMARY KEY, Available int NOT NULL); INSERT #Stock VALUES (1,2),(2,10); DECLARE @id int=1,@qty int=3; Запрос: UPDATE #Stock SET Available=Available-@qty WHERE Id=@id AND Available>=@qty; Сразу после UPDATE читается @@ROWCOUNT, затем остатки по Id. Назови rowcount и итоговые остатки. Не утверждай rollback других операций.

**Chosen**

```text
Изменено 0 строк; остатки: Id=1 → 2, Id=2 → 10.
```

**Rejected**

```text
Изменена 1 строка; остатки: Id=1 → -1, Id=2 → 10.
```

**Simple check (English):** Evaluate Available>=qty as 2>=3: false. No row changes and both original stock values remain.

**Authoring trap:** Explaining a different query by silently adding DISTINCT, moving a filter or weakening a predicate.

**Reference source:** `curriculum/doc-code.json`; `RAG-041`, `RAG-042`. Полные локальные и upstream-пути — в JSON.

## docs.angular

**Generator instruction (English):** Explain the displayed Angular/TypeScript fixture using its stated version and timeline. Distinguish property binding, inner subscription and pending Promise state. Do not claim that an isolated fixture is an observed application run.

### AUTHORING.DOC-NG-DISABLED.REF.01

Objective: `DOC-NG-DISABLED` · profile: `base` · error: `binding_polarity`

**Prompt**

Авторская симуляция. Angular 16.2 / TypeScript 5.1. Native button, обычный TestBed fixture; после каждого присваивания вызывается fixture.detectChanges(). Других bindings и host directives нет. Template: <button type="button" [disabled]="busy">Save</button>. Поля/порядок: busy:boolean; значения по шагам false, затем true. Опиши native button.disabled по шагам.

**Chosen**

```text
По шагам: disabled=false, затем disabled=true. Кнопка отключается ровно при busy=true.
```

**Rejected**

```text
По шагам: disabled=true, затем disabled=false. Кнопка отключается ровно при busy=false.
```

**Simple check (English):** Read the expression busy, not !busy. Substitute false and true in the same order as the prompt.

**Authoring trap:** Explaining a different binding, subscription policy or readiness timeline.

**Reference source:** `curriculum/doc-code.json`; `RAG-037`, `RAG-038`, `RAG-039`. Полные локальные и upstream-пути — в JSON.

### AUTHORING.DOC-NG-LATEST.REF.01

Objective: `DOC-NG-LATEST` · profile: `base` · error: `stale_inner_emission`

**Prompt**

Авторская симуляция. Angular 16 / RxJS 7.8. query$: Observable<string>; load(q: string): Observable<string>. Используется RxJS TestScheduler; один frame — виртуальная единица, без HTTP и wall clock. query$ выдаёт A в frame 0, B в frame 2 и завершается в frame 10. load(A) — cold inner со значением a через 5 frames после подписки и completion через 6. load(B) — cold inner со значением b через 1 frame и completion через 2. Ошибок нет. Код: query$.pipe(switchMap(q => load(q))). Дай JSON-массив объектов {frame,value} для всех next emissions в порядке доставки. Completion в массив не включать; объяснений не добавлять.

**Chosen**

```text
[{"frame":3,"value":"b"}]
```

**Rejected**

```text
[{"frame":3,"value":"b"},{"frame":5,"value":"a"}]
```

**Simple check (English):** A would emit at frame 5, but B arrives at frame 2 and unsubscribes A. B emits at 2+1=3.

**Authoring trap:** Explaining a different binding, subscription policy or readiness timeline.

**Reference source:** `curriculum/doc-code.json`; `RAG-027`, `RAG-037`, `RAG-039`. Полные локальные и upstream-пути — в JSON.

### AUTHORING.DOC-NG-CONFIG-READY.REF.01

Objective: `DOC-NG-CONFIG-READY` · profile: `base` · error: `ready_inferred_from_invocation`

**Prompt**

Авторская симуляция Angular 16.2.0 / TypeScript 5.1.6. Это изолированный Promise-fixture, а не выполненный Angular bootstrap. По умолчанию type Config={apiBaseUrl:string}; если задача явно расширяет тип, используется показанное расширение. Локальный host ждёт только возвращённый Promise initializer; если возвращён undefined, он не ждёт загрузку. Указанные завершённые состояния наблюдаются после await соответствующей задачи; pending означает, что соответствующий Promise ещё не завершён. Иных конкурентных изменений нет. let ready=false; let snapshot:Config|null=null; async function load(){const c=await fetchConfig();snapshot=c;ready=true;} Функция fetchConfig():Promise<Config> задана окружением. Вызов var loadTask=load(); её Promise ещё pending. Назови ready/snapshot до завершения fetch. Затем fetch успешно возвращает {apiBaseUrl:"/v1"}; выполнено await loadTask. Назови новое состояние.

**Chosen**

```text
До завершения fetch: ready=false,snapshot=null. После успешного завершения loadTask: ready=true,snapshot={apiBaseUrl:"/v1"}.
```

**Rejected**

```text
До завершения fetch: ready=true,snapshot=null. После успешного завершения loadTask: ready=true,snapshot={apiBaseUrl:"/v1"}.
```

**Simple check (English):** Before await resolves, assignments after await have not run. After await loadTask, snapshot is assigned and ready is true.

**Authoring trap:** Explaining a different binding, subscription policy or readiness timeline.

**Reference source:** `curriculum/doc-code.json`; `RAG-027`, `RAG-028`, `RAG-037`. Полные локальные и upstream-пути — в JSON.

## code.csharp

**Generator instruction (English):** Return only the requested C# method or expression. Keep its signature and the exact behavioral contract. A compiled-looking method is not sufficient when it changes null, token or await behavior.

### AUTHORING.CODE-CS-NULL.REF.01

Objective: `CODE-CS-NULL` · profile: `base` · error: `empty_treated_as_null`

**Prompt**

Авторская симуляция. C# 10 / .NET 6, nullable enabled. Напиши внутри класса метод string Label(string? s). Только при null вернуть "(нет)"; при "", " " и любом другом значении вернуть точное s без изменений. Ответ — только полное объявление метода.

**Chosen**

```text
string Label(string? s) => s ?? "(нет)";
```

**Rejected**

```text
string Label(string? s) => string.IsNullOrEmpty(s) ? "(нет)" : s;
```

**Simple check (English):** Substitute s="" mentally. The requested output is still ""; IsNullOrEmpty would change it and violates the contract.

**Authoring trap:** Returning documentation or superficially plausible code that changes one required behavior.

**Reference source:** `curriculum/doc-code.json`; `RAG-027`, `RAG-040`. Полные локальные и upstream-пути — в JSON.

### AUTHORING.CODE-CS-CANCEL.REF.01

Objective: `CODE-CS-CANCEL` · profile: `base` · error: `wrong_cancellation_token`

**Prompt**

Авторская симуляция. C# 10 / .NET 6; доступны System, System.Threading, System.Threading.Tasks. Напиши внутри класса ровно метод Task<int> Forward(CancellationToken ct, Func<CancellationToken, Task<int>> read). Контракт: Вызвать read ровно один раз с исходным ct и вернуть ровно полученный Task<int>. Новых источников отмены, проверки ct до вызова и перехвата нет. read не бросает синхронно. Ответ — только полное объявление метода.

**Chosen**

```text
Task<int> Forward(CancellationToken ct, Func<CancellationToken, Task<int>> read) => read(ct);
```

**Rejected**

```text
Task<int> Forward(CancellationToken ct, Func<CancellationToken, Task<int>> read) => read(CancellationToken.None);
```

**Simple check (English):** Compare the argument in read(...) with the parameter ct. Passing None is a different argument even if both snippets are valid C#.

**Authoring trap:** Returning documentation or superficially plausible code that changes one required behavior.

**Reference source:** `curriculum/doc-code.json`; `RAG-027`, `RAG-028`, `RAG-029`. Полные локальные и upstream-пути — в JSON.

### AUTHORING.CODE-CS-AWAIT.REF.01

Objective: `CODE-CS-AWAIT` · profile: `base` · error: `wrong_async_side_effect_path`

**Prompt**

Авторская симуляция. C# 10 / .NET 6. Доступны System, System.Collections.Generic, System.Threading.Tasks. trace — List<string>, в начале пуст; trace.Add не бросает. Зависимость синхронно не бросает и возвращает pending Task. Допустимые завершения: SetResult — RanToCompletion; SetException(new InvalidOperationException()) — Faulted; SetCanceled — Canceled. Faulted Task, содержащая OperationCanceledException/TaskCanceledException, исключена из этого fixture. Напиши внутри класса ровно метод async Task Save(Func<Task> store, List<string> trace). Ждать store; запись saved делать ровно после успешного завершения. InvalidOperationException распространяется как fault, отменённая Task — как cancel; saved не добавляется в обоих случаях. Ответ — только объявление метода.

**Chosen**

```text
async Task Save(Func<Task> store, List<string> trace) { await store(); trace.Add("saved"); }
```

**Rejected**

```text
async Task Save(Func<Task> store, List<string> trace) { trace.Add("saved"); await store(); }
```

**Simple check (English):** While store is pending, saved must not be added. Put the side effect after await, with no hidden catch.

**Authoring trap:** Returning documentation or superficially plausible code that changes one required behavior.

**Reference source:** `curriculum/doc-code.json`; `RAG-027`, `RAG-028`, `RAG-029`. Полные локальные и upstream-пути — в JSON.

## code.sql

**Generator instruction (English):** Return only the requested SQL statement against the supplied setup. Preserve all rows, duplicates and guards required by the contract. Do not provide a prose explanation instead of SQL.

### AUTHORING.CODE-SQL-COUNT.REF.01

Objective: `CODE-SQL-COUNT` · profile: `base` · error: `invented_distinct`

**Prompt**

Авторская симуляция. T-SQL / SQL Server 2019, ANSI_NULLS ON. Перед возвращённым SELECT в этом же одном T-SQL batch на той же connection выполняется setup:
DECLARE @T TABLE (v int NULL); INSERT INTO @T(v) VALUES (7),(NULL),(7);
Напиши один SELECT: n — число строк, в которых v не NULL; каждое повторение учитывается, числовой 0 учитывается. Должна быть ровно одна итоговая строка, в том числе n=0 для пустого набора. Ответ — только SELECT.

**Chosen**

```text
SELECT COUNT(v) AS n FROM @T;
```

**Rejected**

```text
SELECT COUNT(DISTINCT v) AS n FROM @T;
```

**Simple check (English):** Count the two non-NULL occurrences of 7 separately. Do not add DISTINCT when duplicates must count.

**Authoring trap:** Returning an explanation or a SQL statement with one extra semantic change.

**Reference source:** `curriculum/doc-code.json`; `RAG-040`, `RAG-041`. Полные локальные и upstream-пути — в JSON.

### AUTHORING.CODE-SQL-LEFT.REF.01

Objective: `CODE-SQL-LEFT` · profile: `base` · error: `right_filter_removes_left_rows`

**Prompt**

Авторская симуляция. T-SQL / SQL Server 2019; ANSI_NULLS ON. Полный setup: CREATE TABLE #P (Id int NOT NULL PRIMARY KEY); CREATE TABLE #R (Id int NOT NULL PRIMARY KEY, ParentId int NOT NULL, Active bit NULL); INSERT #P VALUES (1),(2); INSERT #R VALUES (10,1,1),(11,1,0); Напиши ровно SELECT. Вернуть каждую строку #P, а из #R только Active=1; если подходящих #R нет, RightId=null. Поля Id,RightId; ORDER BY p.Id,r.Id. Ответ — SQL без Markdown; setup не повторять.

**Chosen**

```text
SELECT p.Id, r.Id AS RightId FROM #P p LEFT JOIN #R r ON r.ParentId=p.Id AND r.Active=1 ORDER BY p.Id,r.Id;
```

**Rejected**

```text
SELECT p.Id, r.Id AS RightId FROM #P p LEFT JOIN #R r ON r.ParentId=p.Id WHERE r.Active=1 ORDER BY p.Id,r.Id;
```

**Simple check (English):** Keep Active=1 in the matching condition. Putting it in WHERE would discard the NULL-extended row for parent 2.

**Authoring trap:** Returning an explanation or a SQL statement with one extra semantic change.

**Reference source:** `curriculum/doc-code.json`; `RAG-040`, `RAG-041`. Полные локальные и upstream-пути — в JSON.

### AUTHORING.CODE-SQL-GUARD.REF.01.v2

Objective: `CODE-SQL-GUARD` · profile: `base` · error: `guard_predicate_boundary`

**Prompt**

Авторская симуляция. T-SQL / SQL Server 2019; ANSI_NULLS ON; триггеров и CHECK нет, конкурентных соединений в этом fixture нет. Setup: CREATE TABLE #Stock (Id int NOT NULL PRIMARY KEY, Available int NOT NULL); INSERT #Stock VALUES (1,2),(2,10); DECLARE @id int=1,@qty int=3; Напиши ровно один UPDATE без setup. Изменить только строку @id и только если Available>=@qty. @qty положительный и проверен вызывающим кодом. Не менять соседнюю строку. При допуске уменьшить Available ровно на @qty; другие поля не менять. Ответ — только SQL.

**Chosen**

```text
UPDATE #Stock SET Available=Available-@qty WHERE Id=@id AND Available>=@qty;
```

**Rejected**

```text
UPDATE #Stock SET Available=Available-@qty WHERE Id=@id AND Available>0;
```

**Simple check (English):** Try Available=2 and qty=3. The guard must reject the update; Available>0 is too weak.

**Authoring trap:** Returning an explanation or a SQL statement with one extra semantic change.

**Reference source:** `curriculum/doc-code.json`; `RAG-041`, `RAG-042`. Полные локальные и upstream-пути — в JSON.

## code.angular

**Generator instruction (English):** Return the requested Angular template or TypeScript expression/method. Preserve boolean polarity, latest-request semantics and readiness order. Do not return documentation in place of the requested code.

### AUTHORING.CODE-NG-DISABLED.REF.01

Objective: `CODE-NG-DISABLED` · profile: `base` · error: `binding_polarity`

**Prompt**

Авторская симуляция. Angular 16.2 / TypeScript 5.1. Поле busy:boolean. Кнопка должна быть отключена ровно когда busy=true. Напиши только один native button type="button" с текстом Save и Angular [disabled] binding. Нет других bindings/host directives; после изменения полей вызывается fixture.detectChanges().

**Chosen**

```text
<button type="button" [disabled]="busy">Save</button>
```

**Rejected**

```text
<button type="button" [disabled]="!busy">Save</button>
```

**Simple check (English):** Use a two-row truth table: busy=false gives disabled=false; busy=true gives disabled=true.

**Authoring trap:** Returning documentation or a template/expression whose polarity or async order is reversed.

**Reference source:** `curriculum/doc-code.json`; `RAG-037`, `RAG-038`, `RAG-039`. Полные локальные и upstream-пути — в JSON.

### AUTHORING.CODE-NG-LATEST.REF.01

Objective: `CODE-NG-LATEST` · profile: `base` · error: `stale_inner_emission`

**Prompt**

Авторская симуляция. Angular 16 / RxJS 7.8. query$: Observable<string>; load(q: string): Observable<string>. Используется RxJS TestScheduler; один frame — виртуальная единица, без HTTP и wall clock. query$ выдаёт A в frame 0, B в frame 2 и завершается в frame 10. load(A) — cold inner со значением a через 5 frames после подписки и completion через 6. load(B) — cold inner со значением b через 1 frame и completion через 2. Ошибок нет. Напиши только выражение Observable, которое при новом q отписывает предыдущий inner и доставляет далее только последний. Используй switchMap; других операторов не требуется. Imports доступны. Ответ — выражение без Markdown.

**Chosen**

```text
query$.pipe(switchMap(q => load(q)))
```

**Rejected**

```text
query$.pipe(mergeMap(q => load(q)))
```

**Simple check (English):** The new query must unsubscribe the prior inner. Return the requested switchMap expression, not mergeMap or a prose description.

**Authoring trap:** Returning documentation or a template/expression whose polarity or async order is reversed.

**Reference source:** `curriculum/doc-code.json`; `RAG-027`, `RAG-037`, `RAG-039`. Полные локальные и upstream-пути — в JSON.

### AUTHORING.CODE-NG-CONFIG-READY.REF.01

Objective: `CODE-NG-CONFIG-READY` · profile: `base` · error: `ready_before_fetch_completion`

**Prompt**

Авторская симуляция Angular 16.2.0 / TypeScript 5.1.6. Это изолированный Promise-fixture, а не выполненный Angular bootstrap. По умолчанию type Config={apiBaseUrl:string}; если задача явно расширяет тип, используется показанное расширение. Локальный host ждёт только возвращённый Promise initializer; если возвращён undefined, он не ждёт загрузку. Указанные завершённые состояния наблюдаются после await соответствующей задачи; pending означает, что соответствующий Promise ещё не завершён. Иных конкурентных изменений нет. В классе Loader заданы ready=false, snapshot:Config|null=null и поле fetch:()=>Promise<Config>. Напиши метод async load():Promise<void>: присвоить полученный config, затем ready=true. Пока fetch pending, ready=false; ошибку fetch передать вызывающему. Это первый запуск.

**Chosen**

```text
async load(): Promise<void> { const c = await this.fetch(); this.snapshot = c; this.ready = true; }
```

**Rejected**

```text
async load(): Promise<void> { this.ready = true; const c = await this.fetch(); this.snapshot = c; }
```

**Simple check (English):** Read execution order: await fetch, assign snapshot, set ready. The pending interval must keep ready=false.

**Authoring trap:** Returning documentation or a template/expression whose polarity or async order is reversed.

**Reference source:** `curriculum/doc-code.json`; `RAG-027`, `RAG-028`, `RAG-037`. Полные локальные и upstream-пути — в JSON.
