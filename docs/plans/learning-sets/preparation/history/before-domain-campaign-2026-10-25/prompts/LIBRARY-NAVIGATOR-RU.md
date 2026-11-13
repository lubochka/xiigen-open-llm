# Предметный навигатор библиотеки DPO для китайской модели

## Дополнение: 13 узких целей и общий сюжет

Текущий состав:148 карточек,192 потока,934 reference-пары. [Поставка и точные команды](../26-DOMAIN-GENERATION-DELIVERY.md). Прямой stream ID выше таблицы; остальные темы не подмешиваются в текущую партию.

| Модуль | Новые цели | Что читать |
|---|---|---|
| EN/RU/HE | TR07-REPEAT, TR06-NUMBER-UNIT, TR03-EVENT-ORDER | [Хорошие/плохие примеры](TRANSLATION-SURFACE-QUANTITY-TIME-RU.md), [30 общих TRAIN-сценариев](../curriculum/family-plans/translation-surface-quantity-time.train.json) |
| Оркестрация | OR-AUTO-EVENT-GATE, OR-DB-CAPACITY-ATOMIC, OR-ARCH-EVENT-CONTRACT, OR-QA-SCORE-PRESENCE | [Предметный модуль](ORCHESTRATION-EVENTS-AND-EVIDENCE-RU.md), [новые источники](../rag/DOMAIN-GENERATION-SOURCES-2026-10-25.md) |
| Документация | DOC-CS-PIPELINE, DOC-SQL-WINDOW-FRAME, DOC-NG-CONFIG-READY | [Полные примеры и контракты среды](SOFTWARE-PIPELINE-WINDOW-READY-RU.md) |
| Код | CODE-CS-ROUND-STAGE, CODE-SQL-RANK-TIE, CODE-NG-CONFIG-READY | [Полные примеры и контракты среды](SOFTWARE-PIPELINE-WINDOW-READY-RU.md) |

Для назначенного потока открой его подготовленный prompt: он уже содержит учебную цель, полные reference выбранного профиля и RAG. Под одинаковым family ID не придумывай разные сюжеты. В новой переводческой волне исходная ситуация заморожена сразу в EN/RU/HE; ученик получает только исходный язык. Остальные языки и разбор доступны автору. Это создание исходного материала, не review кандидатов.

Библиотека помогает подготовить данные, на которых ученик будет переводить, объяснять и писать код, а также выбирать действия программного проекта. Генератор создаёт для одной предметной задачи вход ученика и два полных ответа: предпочтительный и близкий ошибочный. Служебный приказ «создай набор DPO» не должен заменять предметный вход ученика.

**Если stream уже назначен, открыть его готовый `prompt.md` и `request.json`.** Все нужные схема, карточка, источники, семейства и абсолютные пути должны быть в назначенном пакете. Этот навигатор нужен для обзора и выбора материала до назначения; он не разрешает сменить задачу, искать другую схему в `llm_mvp_core` или снова обследовать всю библиотеку после получения request. Для следующего назначения используется [единая очередь](../execution-queue.json), а не порядок разделов этого файла.

Текущая стадия — **CREATE_ONLY**. Внешняя китайская модель, выбранная по умолчанию в Kiro, создаёт кандидаты без override. **CODEX с Astra 6 ultracode** выполняет выбор материала и оценку качества в отдельно назначенной стадии; генератор не заменяет CODEX и не принимает собственные пары. Сейчас создание не включает review, oracle, обучение, checksum или production CI. Отчёт на русском; реально созданные кандидаты, оценка качества и обучение имеют отдельные статусы.

Короткие рабочие ссылки: [общий промпт](MASTER-PROMPT-RU.md), [промпт партии](BATCH-GENERATE-PROMPT-RU.md), [контракт данных](../02-DATA-CONTRACT.md), [CREATE_ONLY и пути](../20-CREATE-ONLY-AND-PATHS.md), [навыки](../06-SOURCES-AND-SKILLS.md). Это ссылки для восстановления отсутствующего конкретного входа, не дополнительный круг чтения перед каждым batch.

## Перевод: целевые языки и направления

По действующему [каталогу тем](../domains/translation/topic-index.json) данные разделены по целевому языку. Направление — профиль внутри темы перевода; оно не задаёт новый механизм или отдельную модель само по себе.

| Целевой домен | Профили направления | Чему учится ученик | Где предметный пример |
|---|---|---|---|
| `tr.en` — перевод на английский | `ru-en`, `he-en` | Сохранять смысл исходного русского/ивритского текста в английском ответе | [Условие и направление связи](../domains/translation/TR-EN-IF-01.md); общие [отрицание](../curriculum/cards/TR01-NEGATION-SCOPE.md) и [исправление пропуска буквы](../curriculum/cards/TR07-OMISSION.md), выбрать пример нужного профиля |
| `tr.ru` — перевод на русский | `en-ru`, `he-ru` | Передавать смысл на русском, сохраняя участников, время и ограничения | [Область отрицания и количества](../domains/translation/TR-RU-SCOPE-01.md); [порядок ролей](../curriculum/cards/TR02-ROLE-ORDER.md), примеры `en-ru`/`he-ru` |
| `tr.he` — перевод на иврит | `en-he`, `ru-he` | Передавать смысл на иврите с явно данным родом и числом, не угадывая недостающий контекст | [Условная связь](../domains/translation/TR-HE-IF-01.md); [явный род](../curriculum/cards/TR04-EXPLICIT-GENDER.md), примеры `en-he`/`ru-he` |

Основная методическая опора перевода: [RAG-023](../rag/sources/RAG-023.md), [RAG-024](../rag/sources/RAG-024.md). Дополнительный контекст и границы значения: [RAG-043](../rag/sources/RAG-043.md), [RAG-TR-001](../rag/sources/RAG-TR-001.md). Источники задают методику; открытые авторские переводы не объявляются автоматически оценёнными CODEX.

### Опечатки и неверная раскладка принадлежат своему семейству

Восстановленная форма и её повреждённое написание должны оставаться в одной лексической семье, если контекст действительно показывает ту же лексему и смысл. Сходство строк само по себе не разрешает объединять разные слова. Лексическая семья (`lexical_family_ids`) и происхождение сценария (`family_id`/`origin_family_id`/`leakage_group_ids`) решают разные задачи: первая связывает формы слова, второе предотвращает утечку близких историй между split. Одного поля metadata недостаточно, чтобы обучить новый механизм токенизации: повреждённая форма должна быть видна в prompt, а правильный смысл — в chosen.

| Узкий навык | Конкретная карточка и открытая опора | Источники |
|---|---|---|
| Восстановить ровно одну пропущенную букву | [TR07-OMISSION](../curriculum/cards/TR07-OMISSION.md); `TR07-OMISSION.ru-en.REF01` | [RAG-024](../rag/sources/RAG-024.md), [RAG-023](../rag/sources/RAG-023.md) |
| Восстановить перестановку соседних букв | [TR07-TRANSPOSE](../curriculum/cards/TR07-TRANSPOSE.md); `TR07-TRANSPOSE.ru-en.REF01` | [RAG-024](../rag/sources/RAG-024.md), [RAG-023](../rag/sources/RAG-023.md) |
| Сохранить самостоятельное похожее слово | [TR07-VALIDWORD](../curriculum/cards/TR07-VALIDWORD.md); `TR07-VALIDWORD.ru-en.REF01` | [RAG-024](../rag/sources/RAG-024.md), [RAG-023](../rag/sources/RAG-023.md) |
| Восстановить слово по явно заданной карте раскладки | [TR08-LAYOUT](../curriculum/cards/TR08-LAYOUT.md); `TR08-LAYOUT.ru-en.REF01.v2` | [RAG-024](../rag/sources/RAG-024.md), [RAG-023](../rag/sources/RAG-023.md) |
| По контексту отличить повреждённое слово от буквального идентификатора | [TR08-RESTORE-OR-PRESERVE](../curriculum/cards/TR08-RESTORE-OR-PRESERVE.md); `TR08-RESTORE-OR-PRESERVE.ru-en.REF01` | [RAG-024](../rag/sources/RAG-024.md), [RAG-023](../rag/sources/RAG-023.md) |

Другие смысловые границы не смешиваются с опечатками в одной назначенной партии: [референт местоимения](../curriculum/cards/TR02-COREFERENCE.md), [значение по контексту](../curriculum/cards/TR05-CONTEXT-SENSE.md), [разрешение и способность](../curriculum/cards/TR05-MODALITY.md), [заполнители](../curriculum/cards/TR06-PLACEHOLDER.md), [технические имена](../curriculum/cards/TR09-IDENTIFIER.md), [уточнение при недостаточном контексте](../curriculum/cards/TR10-CONDITIONAL.md). Правила и опорные строки находятся непосредственно в каждой карточке.

## Оркестрация: задачи программного проекта

Ученик принимает локальное управляющее решение по видимому состоянию и доступным действиям. Он не пишет предметный код, перевод или документацию вместо назначенного решения. Названия бизнеса в условиях — контекст одной задачи оркестрации; они не разрешают смешать самостоятельные учебные темы.

| Поддомен | Чему учится ученик | Конкретная карточка и пример | Источники |
|---|---|---|---|
| `or.functional` — функциональное описание | Сформулировать наблюдаемый критерий, сохранив требование | [OR-FUNC-ACCEPTANCE](../curriculum/cards/OR-FUNC-ACCEPTANCE.md); `OR-FUNC-ACCEPTANCE-REF-01` | [RAG-004](../rag/sources/RAG-004.md), [RAG-005](../rag/sources/RAG-005.md), [RAG-OR-001](../rag/sources/RAG-OR-001.md) |
| `or.architecture` — архитектура | Выбрать полезное разбиение узла и сохранить его обязательства | [OR-ARCH-DECOMPOSE](../curriculum/cards/OR-ARCH-DECOMPOSE.md); `OR-ARCH-DECOMPOSE-REF-01` | [RAG-OR-004](../rag/sources/RAG-OR-004.md) |
| `or.code` — план реализации | Передать исполнителю фактически необходимый контекст задачи | [OR-CODE-PROMPT-CONTEXT](../curriculum/cards/OR-CODE-PROMPT-CONTEXT.md); `OR-CODE-PROMPT-CONTEXT-REF-01` | [RAG-005](../rag/sources/RAG-005.md), [RAG-006](../rag/sources/RAG-006.md), [RAG-047](../rag/sources/RAG-047.md) |
| `or.database` — базы данных | Выбрать следующий шаг по доказанному владению операцией | [OR-DB-ATOMIC-CLAIM](../curriculum/cards/OR-DB-ATOMIC-CLAIM.md); `OR-DB-ATOMIC-CLAIM-REF-01` | [RAG-006](../rag/sources/RAG-006.md), [RAG-OR-001](../rag/sources/RAG-OR-001.md) |
| `or.client` — клиент | Изменять клиентское состояние по подтверждению нужного запуска | [OR-CLIENT-RUN-STATE](../curriculum/cards/OR-CLIENT-RUN-STATE.md); `OR-CLIENT-RUN-STATE-REF-01` | [RAG-001](../rag/sources/RAG-001.md), [RAG-003](../rag/sources/RAG-003.md), [RAG-048](../rag/sources/RAG-048.md) |
| `or.server` — сервер | Согласовать сохранение изменения и доставку события | [OR-SERVER-OUTBOX](../curriculum/cards/OR-SERVER-OUTBOX.md); `OR-SERVER-OUTBOX-REF-01` | [RAG-006](../rag/sources/RAG-006.md), [RAG-OR-001](../rag/sources/RAG-OR-001.md) |
| `or.mobile` — мобильные приложения | Закрывать локальную операцию по подходящему подтверждению | [OR-MOBILE-ACK](../curriculum/cards/OR-MOBILE-ACK.md); `OR-MOBILE-ACK-REF-01` | [RAG-006](../rag/sources/RAG-006.md), [RAG-017](../rag/sources/RAG-017.md), [RAG-OR-003](../rag/sources/RAG-OR-003.md) |
| `or.design` — дизайн | Выбирать панель по роли, объекту и области доступа | [OR-DESIGN-ROLE-PANEL](../curriculum/cards/OR-DESIGN-ROLE-PANEL.md); `OR-DESIGN-ROLE-PANEL-REF-01` | [RAG-003](../rag/sources/RAG-003.md), [RAG-048](../rag/sources/RAG-048.md), [RAG-049](../rag/sources/RAG-049.md) |
| `or.ux` — пользовательский путь | Выбирать понятное действие восстановления по известной причине | [OR-UX-RECOVERY](../curriculum/cards/OR-UX-RECOVERY.md); `OR-UX-RECOVERY-REF-01` | [RAG-003](../rag/sources/RAG-003.md), [RAG-048](../rag/sources/RAG-048.md), [RAG-049](../rag/sources/RAG-049.md) |
| `or.qa` — проверка результата | Планировать сравнение, которое выделяет один учебный фактор | [OR-QA-CAUSAL](../curriculum/cards/OR-QA-CAUSAL.md); `OR-QA-CAUSAL-REF-01` | [RAG-002](../rag/sources/RAG-002.md), [RAG-032](../rag/sources/RAG-032.md), [RAG-046](../rag/sources/RAG-046.md) |
| `or.automation` — автоматизация | Выбирать продолжение одного длительного запуска по его состоянию | [OR-AUTO-TERMINAL](../curriculum/cards/OR-AUTO-TERMINAL.md); `OR-AUTO-TERMINAL-REF-01` | [RAG-001](../rag/sources/RAG-001.md), [RAG-032](../rag/sources/RAG-032.md), [RAG-047](../rag/sources/RAG-047.md) |
| `or.routing` — маршрутизация намерения | Назначать получателя по запрошенному действию из видимого закрытого каталога | [OR-ROUTE-INTENT](../curriculum/cards/OR-ROUTE-INTENT.md); `ROUTE-INTENT-001` | [RAG-006](../rag/sources/RAG-006.md), [RAG-031](../rag/sources/RAG-031.md), [RAG-033](../rag/sources/RAG-033.md) |

Полный предметный список — [каталог оркестрации](../domains/orchestration/topic-index.json). Узкие продолжения:

- [Порядок FIFO](../curriculum/cards/OR-ARCH-FIFO.md) — RAG-006/RAG-050; взаимное исключение не подменяет гарантию порядка.
- [Логический ключ операции](../domains/orchestration/OR-DB-02.md) — RAG-OR-001/RAG-006; повтор и независимая операция различаются по видимому контракту.
- [Сбор группы ветвей](../curriculum/cards/OR-AUTO-FANIN.md) — RAG-001/RAG-008/RAG-047; учитывается политика обязательных результатов.
- [Временное окно агрегации](../curriculum/cards/OR-AUTO-TIME-WINDOW.md) и [полнота выборки](../curriculum/cards/OR-AUTO-RESULT-COVERAGE.md) — RAG-050; это отдельные задачи выбора периода и решения о полном результате, без выдуманного API пагинации.
- [Состав предметной проверки](../curriculum/cards/OR-QA-REVIEWER-COVERAGE.md) — RAG-OR-005; ученик планирует покрытие проверки. Эта учебная тема не является приказом запускать review текущего DPO-пакета.

`or.routing` учится маршруту. Например, запрос на перевод отправляется получателю перевода, но сама маршрутная DPO-пара не должна содержать выполненный перевод вместо JSON выбора. Закрытый каталог и правило неоднозначности берутся из конкретного prompt; навигатор не добавляет скрытых маршрутов.

## Документация: объяснить показанный код

Здесь `prompt` содержит код, версию, исходные данные и вопрос о поведении. `chosen/rejected` — два объяснения одного фрагмента. От ученика не требуется написать новый код, если этого нет в контракте. Используется версия из задания, а не неподтверждённая «последняя версия».

| Домен | Чему учится ученик | Конкретная карточка и пример | Источники |
|---|---|---|---|
| `docs.csharp` — C# / .NET | Отличать null от пустой и пробельной строки в объяснении метода | [DOC-CS-NULL](../curriculum/cards/DOC-CS-NULL.md); `DOC-CS-NULL.REF.01` | [RAG-027](../rag/sources/RAG-027.md), [RAG-028](../rag/sources/RAG-028.md) |
| `docs.sql` — SQL | Объяснять различие COUNT по всем строкам и nullable-столбцу | [DOC-SQL-COUNT](../curriculum/cards/DOC-SQL-COUNT.md); `DOC-SQL-COUNT.REF.01` | [RAG-040](../rag/sources/RAG-040.md), [RAG-041](../rag/sources/RAG-041.md) |
| `docs.angular` — Angular | Объяснять boolean disabled binding в заданном компоненте | [DOC-NG-DISABLED](../curriculum/cards/DOC-NG-DISABLED.md); `DOC-NG-DISABLED.REF.01` | [RAG-037](../rag/sources/RAG-037.md), [RAG-038](../rag/sources/RAG-038.md), [RAG-039](../rag/sources/RAG-039.md) |

Дополнительные конкретные случаи: C# — [отмена](../curriculum/cards/DOC-CS-CANCEL.md), [побочный эффект после await](../curriculum/cards/DOC-CS-AWAIT.md), [граница снимка](../curriculum/cards/DOC-CS-SNAPSHOT.md); SQL — [LEFT JOIN](../curriculum/cards/DOC-SQL-LEFT.md), [защищённый UPDATE](../curriculum/cards/DOC-SQL-GUARD.md), [savepoint](../curriculum/cards/DOC-SQL-SAVEPOINT.md); Angular — [последний запрос](../curriculum/cards/DOC-NG-LATEST.md), [восстановление потока](../curriculum/cards/DOC-NG-RECOVERY.md), [фактический payload формы](../curriculum/cards/DOC-NG-PAYLOAD.md). Каждая карточка содержит свои source IDs, условия и разобранные публичные reference.

## Написание кода: реализовать показанное требование

Здесь `prompt` задаёт поведение, типы, версию и ограничения. `chosen/rejected` — полные сопоставимые реализации; ошибочная отличается одним заданным дефектом поведения. Объяснение кода и его написание имеют разные учебные ответы, даже когда используют общий исходный сценарий. Общая история сохраняет origin/leakage group между этими представлениями.

| Домен | Чему учится ученик | Конкретная карточка и пример | Источники |
|---|---|---|---|
| `code.csharp` — C# / .NET | Реализовать подстановку только для null по заданному контракту | [CODE-CS-NULL](../curriculum/cards/CODE-CS-NULL.md); `CODE-CS-NULL.REF.01` | [RAG-027](../rag/sources/RAG-027.md), [RAG-040](../rag/sources/RAG-040.md) |
| `code.sql` — SQL | Написать нужный агрегат с явной политикой NULL | [CODE-SQL-COUNT](../curriculum/cards/CODE-SQL-COUNT.md); `CODE-SQL-COUNT.REF.01` | [RAG-040](../rag/sources/RAG-040.md), [RAG-041](../rag/sources/RAG-041.md) |
| `code.angular` — Angular | Реализовать boolean binding, сохраняющий заданное состояние | [CODE-NG-DISABLED](../curriculum/cards/CODE-NG-DISABLED.md); `CODE-NG-DISABLED.REF.01` | [RAG-037](../rag/sources/RAG-037.md), [RAG-038](../rag/sources/RAG-038.md), [RAG-039](../rag/sources/RAG-039.md) |

Более узкие реализации: C# — [CancellationToken](../curriculum/cards/CODE-CS-CANCEL.md), [порядок после await](../curriculum/cards/CODE-CS-AWAIT.md), [снимок данных](../curriculum/cards/CODE-CS-SNAPSHOT.md); SQL — [точное соединение](../curriculum/cards/CODE-SQL-LEFT.md), [защищённое изменение](../curriculum/cards/CODE-SQL-GUARD.md), [откат до savepoint](../curriculum/cards/CODE-SQL-SAVEPOINT.md); Angular — [актуальный запрос](../curriculum/cards/CODE-NG-LATEST.md), [восстановление после ошибки](../curriculum/cards/CODE-NG-RECOVERY.md), [проекция формы в payload](../curriculum/cards/CODE-NG-PAYLOAD.md).

Если запрос пользователя звучит «объясни», используется документационный ответ; если «напиши/исправь реализацию», кодовый; если «выбери исполнителя/следующий шаг», оркестрационный. Это описание различий назначений, а не разрешение генератору поменять уже выданный stream.

## Три мини-примера: приказ генератору и вход ученика

Ниже дословно переиспользованы опубликованные публичные reference-пары по указанным ID. Они **не новые строки**, не отдельный TRAIN/EXAM и не результаты запуска китайской модели. Внешний приказ автору отделён от предметного `prompt`, который получает ученик. Rationale и source IDs остаются вне ученического ответа.

### Перевод: `TR07-OMISSION.ru-en.REF01`

Источник пары: [TR07-OMISSION](../curriculum/cards/TR07-OMISSION.md); машинная запись — в [curriculum/translation.json](../curriculum/translation.json). Открытая семья: `v2-ref.translation.ticket-on-table`. Методические источники: [RAG-024](../rag/sources/RAG-024.md), [RAG-023](../rag/sources/RAG-023.md).

**Внешний приказ генератору, не часть ученического prompt:**

```text
Составляй только назначенные в request новые DPO-пары для перевода RU→EN с одной пропущенной буквой, как требует карточка. В prompt ученика помести повреждённое предложение и достаточный предметный контекст; в chosen/rejected — два полных перевода с одной различающей смысловой ошибкой. Показанную ниже открытую историю не копируй в новый TRAIN/EXAM.
```

**Предметный `prompt` ученика — дословная открытая опора:**

```text
Переведи с языка «русский» на язык «английский». Выдай только перевод. Контекст: документ для проезда в автобусе. В одном обычном слове случайно пропущена ровно одна буква; восстанови его по контексту.
Исходный текст: Блет лежит на столе.
```

**`chosen` — полный предпочтительный ответ:**

```text
The ticket is on the table.
```

**`rejected` — полный близкий ошибочный ответ:**

```text
The receipt is on the table.
```

**Что различается:** Пропуск буквы в слове «билет» не превращает документ в квитанцию; остальные части предложения одинаковы.

Ученик переводит повреждённое предложение. Он не получает приказ «создай DPO», не перечисляет файлы и не пишет отчёт генератора.

### Объяснение кода: `DOC-CS-NULL.REF.01`

Источник пары: [DOC-CS-NULL](../curriculum/cards/DOC-CS-NULL.md); машинная запись — в [curriculum/doc-code.json](../curriculum/doc-code.json). Открытая семья: `ref.cs.null-fallback.literal.01`. Методические источники: [RAG-027](../rag/sources/RAG-027.md), [RAG-028](../rag/sources/RAG-028.md).

**Внешний приказ генератору, не часть ученического prompt:**

```text
Составляй только назначенные новые DPO-пары документации C#/.NET. В prompt покажи полный небольшой метод, версию, входы и вопрос о поведении. Оба ответа должны объяснять тот же метод; rejected содержит одну ошибку о null/пустой строке. Не подменяй объяснение заданием написать новый метод.
```

**Предметный `prompt` ученика — дословная открытая опора:**

```text
Авторская симуляция. C# 10 / .NET 6, nullable enabled. Внутри класса объявлен метод: string Label(string? s) => s ?? "(нет)"; Других операций нет. Опиши результаты вызовов Label(null), Label(""), Label(" "), Label("A") в таком порядке и правило подстановки.
```

**`chosen` — полный предпочтительный ответ:**

```text
Результаты: "(нет)", "", " ", "A". Подстановка выполняется только для null; пустые и пробельные строки сохраняются.
```

**`rejected` — полный близкий ошибочный ответ:**

```text
Результаты: "(нет)", "(нет)", " ", "A". Подстановка выполняется для null и пустой строки; пробельная строка сохраняется.
```

**Что различается:** Оператор ?? выбирает fallback только при null; дополнительная проверка длины отсутствует.

Ученик описывает результаты существующего метода. Автор создаёт две альтернативы этого описания и сохраняет их как данные.

### Оркестрация: `OR-AUTO-TERMINAL-REF-01`

Источник пары: [OR-AUTO-TERMINAL](../curriculum/cards/OR-AUTO-TERMINAL.md); машинная запись — в [curriculum/orchestration.json](../curriculum/orchestration.json). Открытая семья: `OR-AUTO-TERMINAL-RF01`. Методические источники: [RAG-001](../rag/sources/RAG-001.md), [RAG-032](../rag/sources/RAG-032.md), [RAG-047](../rag/sources/RAG-047.md).

**Внешний приказ генератору, не часть ученического prompt:**

```text
Составляй только назначенные новые DPO-пары управления одним длительным запуском. В prompt перечисли наблюдаемое состояние, handle и смысл допустимых действий; chosen/rejected должны быть одним и тем же JSON-форматом следующего шага. Нужны противоположные состояния, где ожидание уместно и где оно уже лишнее.
```

**Предметный `prompt` ученика — дословная открытая опора:**

```text
Обучающий run L41 имеет handle h41. Последнее подтверждённое состояние сервера running. progress.json содержит steps=8, но terminal.json ещё нет. API wait(h41) штатно ждёт до следующего состояния. Допустимые action: wait, read_final. Верни JSON action, run_id=L41.
```

**`chosen` — полный предпочтительный ответ:**

```text
{"action":"wait","run_id":"L41"}
```

**`rejected` — полный близкий ошибочный ответ:**

```text
{"action":"read_final","run_id":"L41"}
```

**Что различается:** Промежуточные шаги не являются terminal-результатом; продолжение того же handle сохраняет один процесс.

Ученик выбирает управляющее действие по состоянию. Это синтетическая учебная ситуация, а не реальная команда запустить или ждать процесс из примера.

## Как добраться до источника без нового поиска по проекту

Source ID в таблицах ведёт к локальной выдержке в `preparation/rag/sources`. [RAG-индекс](../rag/INDEX.md) и [manifest](../rag/manifest.json) указывают исходный путь, ветку и просмотренные границы. Используются материалы `xiigen-planning/main` и `xiigen-planning/llm_plan3`; core skills — рекомендации процесса, а не дополнительный предметный корпус. Историческая команда внутри RAG не является активным приказом генератору.

При назначенном request читать только его готовый prompt и конкретный недостающий вход. Корень наборов — `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets`, подготовка — его `preparation`; абсолютный путь batch берётся из request. Не создавать вторую копию корпуса ради навигации. Следующий ID — из общей очереди либо прямого указания пользователя; NEXT прекращает прежнюю работу, STOP прекращает действия. Навигатор не заменяет готовое поручение и не назначает новый цикл review.
