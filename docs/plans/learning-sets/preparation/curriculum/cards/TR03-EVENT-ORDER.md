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
| `A035` — `TR03-EVENT-ORDER` | `tr.en` / `ru-en` | 20 | `A036` |
| `A036` — `TR03-EVENT-ORDER` | `tr.en` / `he-en` | 20 | `A037` |
| `A077` — `TR03-EVENT-ORDER` | `tr.ru` / `en-ru` | 20 | `A078` |
| `A078` — `TR03-EVENT-ORDER` | `tr.ru` / `he-ru` | 20 | `A079` |
| `A119` — `TR03-EVENT-ORDER` | `tr.he` / `en-he` | 20 | `A120` |
| `A120` — `TR03-EVENT-ORDER` | `tr.he` / `ru-he` | 20 | `A121` |


Следующий ID из таблицы — номинальный порядок своей группы. Фактическую свободную карточку с учётом владельцев выдаёт команда `next` текущего request; не менять группу и чужой статус вручную.

Замечания качества: смотри `quality_flags` своего domain/topic/profile в общем status. `needs_rework` указывает доказанный дефект с evidence, `needs_assessment` — область без достаточной оценки. Это не новый review-гейт и не разрешение исправлять чужую старую попытку; finish не закрывает флаг автоматически. [Правила общей матрицы](../../28-PARALLEL-GENERATION-AND-QUALITY.md). Для нового окна используй [один из трёх готовых стартовых промптов](../../00-START-HERE.md); не исполняй исторические команды из нижней справочной части вместо текущих owner-команд.

<!-- /CURRENT-PARALLEL-CONTRACT-20261026 -->

# TR03-EVENT-ORDER — До и после: порядок двух завершённых событий

<!-- DOMAIN-CAMPAIGN-AUTHORITY -->
> **Предметная справка к действующей кампании.** Квоты и продолжение задаёт [campaign/registry.json](../../campaign/registry.json), не старые команды ниже. 1000 пар относятся к домену в сумме. Прочитанный генератором рабочий prompt содержит только назначенную микротему.
>
> Данные: `C:/Projects/llm_mvp_core/learning-sets`. Инструкции: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. CREATE_ONLY: без review, checksum, production CI и обучения. Модель — текущая Kiro без override. Reason internally in English; do not output private reasoning. Отчёт по-русски.
>
> После партии выполнять готовый next до доменной квоты; не останавливаться с вопросом «продолжать?». При прямом NEXT/новом ID немедленно сменить задачу: запрещено дописывать/чинить предыдущую. STOP прекращает действия. Один живой handle ждать документированным wait/resume; не объявлять молчание падением. Старые попытки сохранять; выбранный request возобновлять, не пересоздавать.

Точные доли этой предметной карточки в текущей кампании:
| A-ID | Домен | Профиль | Доля из доменных1000 | Следующий A-ID после доли |
|---|---|---|---:|---|
| A035 | tr.en | ru-en | 20 | A036 |
| A036 | tr.en | he-en | 20 | A037 |
| A077 | tr.ru | en-ru | 20 | A078 |
| A078 | tr.ru | he-ru | 20 | A079 |
| A119 | tr.he | en-he | 20 | A120 |
| A120 | tr.he | ru-he | 20 | A121 |


Ниже — сохранённая предметная методика и исторические инструкции. [Полный прежний вариант](../../history/before-domain-campaign-2026-10-25/curriculum/cards/TR03-EVENT-ORDER.md). Старые числовые планы и команды в нём не запускаются вместо актуального request.
<!-- /DOMAIN-CAMPAIGN-AUTHORITY -->

## Узкая задача

Сохранять временную связь между двумя явно описанными событиями, даже когда порядок частей предложения отличается от порядка событий.

Родитель: `TR03`. Предметные домены: `tr.en`, `tr.ru`, `tr.he`. Профили: `ru-en`, `he-en`, `en-ru`, `he-ru`, `en-he`, `ru-he`. Один request содержит только назначенный профиль и эту микроцель.

Статус: авторский материал подготовки. Публичные пары ниже — reference; их нельзя копировать в новый корпус или выдавать за скрытый экзамен. Китайская генерация, оценка Astra и Student для нового материала не выполнялись; ожидаемые результаты не являются измерениями.

## Проверенные источники и применимость

- [RAG-023](../../rag/sources/RAG-023.md): закреплённая методика или исходная тема; конкретные новые случаи являются авторскими симуляциями.
- [RAG-024](../../rag/sources/RAG-024.md): закреплённая методика или исходная тема; конкретные новые случаи являются авторскими симуляциями.

[Разобранные good/bad примеры и логика](../../prompts/TRANSLATION-SURFACE-QUANTITY-TIME-RU.md). Основное задание и все reference выбранного профиля автоматически входят в batch prompt.

## Подробный промпт исполнителю

Создавай короткие предложения с ровно двумя завершёнными событиями и явным before/after, до/после или לפני/אחרי. Участники и предметы названы; местоимение допускается только при однозначном референте. В learner prompt проси сохранить, какое событие было раньше. Chosen передаёт исходную последовательность. Rejected меняет только временную связь на противоположную: before↔after, до↔после, לפני↔אחרי. Не меняй участников, глаголы, числа, отрицание и завершённость. Согласования, необходимые для естественной грамматики, должны оставаться правильными.
Для партии из 10 назначенных сценариев используй 5 с before/до/לפני и 5 с after/после/אחרי. Внутри каждого класса варьируй главную/придаточную часть в начале или конце; не делай порядок слов всегда равным хронологии. Это одна тема временной связи, не смешение доменов. Не используй until/пока не/עד ש, условие if, одновременность, намерение или незавершённое действие: у них другие значения. Не создавай причинный вывод из одного временного порядка.
Самопроверка: отдельно запиши A и B; затем «раньше ___; позже ___» для source, chosen и rejected. Source и chosen должны дать одну пару, rejected — обратную. Перефраз «After A, B»→«B after A» сохраняет A→B и поэтому не годится как rejected. В metadata event_order и rejected_event_order хранят порядок фактов; они не подставляются в learner input. Формулировки оставляй естественными в target-языке. Самопроверка — часть создания, не отдельный review-gate и не измерение обучения.

Работа выполняется по одному request.json: один objective_id, topic_id, profile_id и split. Текущий режим CREATE_ONLY: внешняя открытая китайская модель, уже выбранная пользователем в Kiro, создаёт полные prompt/chosen/rejected. Не меняй модель и не выдумывай её название; фактически неизвестное имя укажи unknown. Роль человека при выборе материала выполняет CODEX с Astra 6 ultracode; качество DPO также оценивает CODEX с Astra 6 ultracode. Здесь это распределение ролей для отдельного будущего этапа, не подтверждение состоявшегося вызова и не review-gate перед созданием.

Прочти 17-CURRENT-PREPARATION-CONTRACT.md, 20-CREATE-ONLY-AND-PATHS.md, 02-DATA-CONTRACT.md, назначенную карточку и назначенные RAG-выдержки. Разобранные хорошие и плохие примеры находятся в prompts/TRANSLATION-SURFACE-QUANTITY-TIME-RU.md. Проектные скилы и точные референсы: 06-SOURCES-AND-SKILLS.md; для этой работы применяются human-readable-architecture-audit-SKILL.md, phase-planning-SKILL.md, plan-review-SKILL.md, code-examination-SKILL.md, prompt-patch-authoring-SKILL.md и human-communication-GLOSSARY.md. Эти скилы помогают составить материал и инструкции; они не запускают review созданных кандидатов. Источники — только xiigen-planning main и llm_plan3, уже включённые в подготовку. Старые внешние ссылки не являются заданием на загрузку. SHA/checksum, подписи, подтверждение alias, independent review и production CI не требуются для создания.

Выход находится строго в request.paths: корень C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets, инструменты внутри preparation. Не ищи runner в llm_mvp_core и не создавай заглушку источника. Составь реальные полные ответы в draft.jsonl, выполни один структурный finish из готового commands.md и перейди к следующему stream из execution-queue.json. После finish статус DRAFT_CREATED, review_status=not_requested, Student=NOT_RUN, учебные метрики NOT_MEASURED. Отчёт по-русски: ID, направление, число реально записанных пар/семей, абсолютный путь, структурный результат, следующий ID. Ответы ученика — только на языке назначения профиля.

В каждом prompt видны все условия, необходимые для выбора ответа. Rationale, правильная метка, canonical_source, ID семьи и ответы экзамена не подставляются в learner-visible input. Chosen и rejected решают одно и то же задание; в rejected ровно одно существенное нарушение. Другой допустимый перевод или синоним не становится rejected. Авторские примеры ниже — reference-симуляции, не цитаты корпуса, не принятые обучающие данные и не измерение обучения. Не копируй reference и их производные в запечатанный DEV/EXAM.

Один исходный сюжет, обратные переводы, перефразирования, опечатки и раскладки сохраняют общий origin/family/leakage и один split. Число строк не равно числу независимых семейств. Новое число, имя или номер партии не делает производный сюжет независимым. Лексическая семья соединяет подтверждённые формы одной лексемы; правильная и ошибочно подставленная другая лексема в ней не сливаются. Семья сценария и лексическая семья — разные связи. В иврите текст хранится в логическом Unicode-порядке без ручного разворота.

Не удаляй прежние данные, эксперименты и результаты: изменённый материал получает новую revision/run и derived_from. При прямом NEXT или новом ID немедленно переключись на указанную задачу; запрещено продолжать прошлую, дописывать её отчёт, чинить её тесты или повторять запуск. При STOP немедленно прекрати действия. Если модель или инструмент уже работает, сохрани один handle и используй его штатный wait/resume до terminal state; timeout транспорта не доказывает отказ. Не заверши ход вместо ожидания, не запускай дубликаты и не подменяй ожидание частым polling.

Обучение сейчас не запускается. Если позже назначен учебный пилот, сначала восстанови совместимый уже обученный checkpoint по 05-LEARNING-AND-CHECKPOINTS.md либо один раз получи и сохрани такое состояние на отдельном TRAIN. Бюджет должен допускать ненулевой учебный результат. 0→0 и малый сдвиг margin сами по себе не доказывают обучение. Любое измеренное улучшение полезного показателя на валидном учебном опыте — успех по этому показателю и домену; остальные показатели, включая ухудшения, показываются рядом. Перенос в production решается отдельно.

## Форма ответа ученика

Только полный перевод исходной строки на target-языке; объяснения и авторская самопроверка остаются вне learner prompt/chosen/rejected.

## Назначенные общие сценарии

- `train`: [curriculum/family-plans/translation-surface-quantity-time.train.json](../../curriculum/family-plans/translation-surface-quantity-time.train.json). `prepare` подставляет этот файл сам; его сюжет и языковые версии входят в `request.family_contracts`.

В одном профиле назначено 10 новых сцен; другие направления используют те же сцены и origin. Полные языковые версии доступны автору. В learner prompt попадает только исходный язык и видимое условие задачи. Следующий чанк должен содержать новые сцены, а DEV/EXAM получают отдельный непересекающийся план.

## Что варьировать

```json
[
  {
    "axis": "connector",
    "values": [
      "before",
      "after"
    ]
  },
  {
    "axis": "clause_order",
    "values": [
      "main_first",
      "subordinate_first"
    ]
  },
  {
    "axis": "agent_count",
    "values": [
      "one_explicit_agent",
      "two_named_agents"
    ]
  }
]
```

## Разобранные пары

### 1. TR03-EVENT-ORDER.ru-en.REF01 — ru-en

**Задача ученику:**

~~~~text
Переведи с языка «русский» на язык «английский». Выдай только перевод. Сохрани, какое из двух действий произошло раньше. Не меняй участников, предметы, завершённость или время действий.
Исходный текст: Майя открыла окно до того, как позвонила Дану.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
Maya opened the window before she called Dan.
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
Maya opened the window after she called Dan.
~~~~

**Почему есть предпочтение:** В источнике раньше происходит open_window, затем call_Dan. Rejected меняет только временную связь и обращает эту последовательность.

Различающий дефект: `reversed_temporal_relation`. Семья: `v3-ref.translation.event.window-call`.

### 2. TR03-EVENT-ORDER.ru-en.REF02 — ru-en

**Задача ученику:**

~~~~text
Переведи с языка «русский» на язык «английский». Выдай только перевод. Сохрани, какое из двух действий произошло раньше. Не меняй участников, предметы, завершённость или время действий.
Исходный текст: После того как Амир запер дверь, он выключил свет.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
After Amir locked the door, he switched off the light.
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
Before Amir locked the door, he switched off the light.
~~~~

**Почему есть предпочтение:** В источнике раньше происходит lock_door, затем switch_off_light. Rejected меняет только временную связь и обращает эту последовательность.

Различающий дефект: `reversed_temporal_relation`. Семья: `v3-ref.translation.event.lock-light`.

### 3. TR03-EVENT-ORDER.he-en.REF01 — he-en

**Задача ученику:**

~~~~text
Переведи с языка «иврит» на язык «английский». Выдай только перевод. Сохрани, какое из двух действий произошло раньше. Не меняй участников, предметы, завершённость или время действий.
Исходный текст: מאיה פתחה את החלון לפני שהתקשרה לדן.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
Maya opened the window before she called Dan.
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
Maya opened the window after she called Dan.
~~~~

**Почему есть предпочтение:** В источнике раньше происходит open_window, затем call_Dan. Rejected меняет только временную связь и обращает эту последовательность.

Различающий дефект: `reversed_temporal_relation`. Семья: `v3-ref.translation.event.window-call`.

### 4. TR03-EVENT-ORDER.he-en.REF02 — he-en

**Задача ученику:**

~~~~text
Переведи с языка «иврит» на язык «английский». Выдай только перевод. Сохрани, какое из двух действий произошло раньше. Не меняй участников, предметы, завершённость или время действий.
Исходный текст: אחרי שאמיר נעל את הדלת, הוא כיבה את האור.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
After Amir locked the door, he switched off the light.
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
Before Amir locked the door, he switched off the light.
~~~~

**Почему есть предпочтение:** В источнике раньше происходит lock_door, затем switch_off_light. Rejected меняет только временную связь и обращает эту последовательность.

Различающий дефект: `reversed_temporal_relation`. Семья: `v3-ref.translation.event.lock-light`.

### 5. TR03-EVENT-ORDER.en-ru.REF01 — en-ru

**Задача ученику:**

~~~~text
Переведи с языка «английский» на язык «русский». Выдай только перевод. Сохрани, какое из двух действий произошло раньше. Не меняй участников, предметы, завершённость или время действий.
Исходный текст: Maya opened the window before she called Dan.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
Майя открыла окно до того, как позвонила Дану.
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
Майя открыла окно после того, как позвонила Дану.
~~~~

**Почему есть предпочтение:** В источнике раньше происходит open_window, затем call_Dan. Rejected меняет только временную связь и обращает эту последовательность.

Различающий дефект: `reversed_temporal_relation`. Семья: `v3-ref.translation.event.window-call`.

### 6. TR03-EVENT-ORDER.en-ru.REF02 — en-ru

**Задача ученику:**

~~~~text
Переведи с языка «английский» на язык «русский». Выдай только перевод. Сохрани, какое из двух действий произошло раньше. Не меняй участников, предметы, завершённость или время действий.
Исходный текст: After Amir locked the door, he switched off the light.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
После того как Амир запер дверь, он выключил свет.
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
До того как Амир запер дверь, он выключил свет.
~~~~

**Почему есть предпочтение:** В источнике раньше происходит lock_door, затем switch_off_light. Rejected меняет только временную связь и обращает эту последовательность.

Различающий дефект: `reversed_temporal_relation`. Семья: `v3-ref.translation.event.lock-light`.

### 7. TR03-EVENT-ORDER.he-ru.REF01 — he-ru

**Задача ученику:**

~~~~text
Переведи с языка «иврит» на язык «русский». Выдай только перевод. Сохрани, какое из двух действий произошло раньше. Не меняй участников, предметы, завершённость или время действий.
Исходный текст: מאיה פתחה את החלון לפני שהתקשרה לדן.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
Майя открыла окно до того, как позвонила Дану.
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
Майя открыла окно после того, как позвонила Дану.
~~~~

**Почему есть предпочтение:** В источнике раньше происходит open_window, затем call_Dan. Rejected меняет только временную связь и обращает эту последовательность.

Различающий дефект: `reversed_temporal_relation`. Семья: `v3-ref.translation.event.window-call`.

### 8. TR03-EVENT-ORDER.he-ru.REF02 — he-ru

**Задача ученику:**

~~~~text
Переведи с языка «иврит» на язык «русский». Выдай только перевод. Сохрани, какое из двух действий произошло раньше. Не меняй участников, предметы, завершённость или время действий.
Исходный текст: אחרי שאמיר נעל את הדלת, הוא כיבה את האור.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
После того как Амир запер дверь, он выключил свет.
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
До того как Амир запер дверь, он выключил свет.
~~~~

**Почему есть предпочтение:** В источнике раньше происходит lock_door, затем switch_off_light. Rejected меняет только временную связь и обращает эту последовательность.

Различающий дефект: `reversed_temporal_relation`. Семья: `v3-ref.translation.event.lock-light`.

### 9. TR03-EVENT-ORDER.en-he.REF01 — en-he

**Задача ученику:**

~~~~text
Переведи с языка «английский» на язык «иврит». Выдай только перевод. Сохрани, какое из двух действий произошло раньше. Не меняй участников, предметы, завершённость или время действий.
Исходный текст: Maya opened the window before she called Dan.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
מאיה פתחה את החלון לפני שהתקשרה לדן.
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
מאיה פתחה את החלון אחרי שהתקשרה לדן.
~~~~

**Почему есть предпочтение:** В источнике раньше происходит open_window, затем call_Dan. Rejected меняет только временную связь и обращает эту последовательность.

Различающий дефект: `reversed_temporal_relation`. Семья: `v3-ref.translation.event.window-call`.

### 10. TR03-EVENT-ORDER.en-he.REF02 — en-he

**Задача ученику:**

~~~~text
Переведи с языка «английский» на язык «иврит». Выдай только перевод. Сохрани, какое из двух действий произошло раньше. Не меняй участников, предметы, завершённость или время действий.
Исходный текст: After Amir locked the door, he switched off the light.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
אחרי שאמיר נעל את הדלת, הוא כיבה את האור.
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
לפני שאמיר נעל את הדלת, הוא כיבה את האור.
~~~~

**Почему есть предпочтение:** В источнике раньше происходит lock_door, затем switch_off_light. Rejected меняет только временную связь и обращает эту последовательность.

Различающий дефект: `reversed_temporal_relation`. Семья: `v3-ref.translation.event.lock-light`.

### 11. TR03-EVENT-ORDER.ru-he.REF01 — ru-he

**Задача ученику:**

~~~~text
Переведи с языка «русский» на язык «иврит». Выдай только перевод. Сохрани, какое из двух действий произошло раньше. Не меняй участников, предметы, завершённость или время действий.
Исходный текст: Майя открыла окно до того, как позвонила Дану.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
מאיה פתחה את החלון לפני שהתקשרה לדן.
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
מאיה פתחה את החלון אחרי שהתקשרה לדן.
~~~~

**Почему есть предпочтение:** В источнике раньше происходит open_window, затем call_Dan. Rejected меняет только временную связь и обращает эту последовательность.

Различающий дефект: `reversed_temporal_relation`. Семья: `v3-ref.translation.event.window-call`.

### 12. TR03-EVENT-ORDER.ru-he.REF02 — ru-he

**Задача ученику:**

~~~~text
Переведи с языка «русский» на язык «иврит». Выдай только перевод. Сохрани, какое из двух действий произошло раньше. Не меняй участников, предметы, завершённость или время действий.
Исходный текст: После того как Амир запер дверь, он выключил свет.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
אחרי שאמיר נעל את הדלת, הוא כיבה את האור.
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
לפני שאמיר נעל את הדלת, הוא כיבה את האור.
~~~~

**Почему есть предпочтение:** В источнике раньше происходит lock_door, затем switch_off_light. Rejected меняет только временную связь и обращает эту последовательность.

Различающий дефект: `reversed_temporal_relation`. Семья: `v3-ref.translation.event.lock-light`.

## Последующий учебный опыт — отдельно от создания

После отдельно назначенной приёмки использовать совместимый сохранённый baseline и [план обучения/повторного использования](../../05-LEARNING-AND-CHECKPOINTS.md). Нужны реальные обновления обучаемого механизма, достаточный набор, ненулевой осмысленный результат до/после и полный отчёт дельт. Любое измеренное полезное улучшение признаётся по своему показателю и домену; точность не обязана расти, ухудшения показываются рядом. JSON, metadata, структура каталогов и 0→0 не являются измеренным успехом обучения. Сейчас такой опыт не запускается.

## Скилы и отчёт

Применённые проектные скилы: human-readable-architecture-audit-SKILL.md, phase-planning-SKILL.md, plan-review-SKILL.md, code-examination-SKILL.md, prompt-patch-authoring-SKILL.md и human-communication-GLOSSARY.md. [Точные ссылки](../../06-SOURCES-AND-SKILLS.md). Их production-checksum, CI и review требования не задерживают CREATE_ONLY: текущее прямое поручение выше. Отчёт кратко по-русски: реальное число строк/семей, тема, файл, наблюдаемая модель или unknown, структурный статус, следующий ID.

<!-- GENERATED-QUEUE-START -->
## Переходы единой очереди

Этот блок воспроизводится из общей очереди и задаёт текущий запуск. Прежние команды/номера в основном тексте сохраняются как история. Уже начатую попытку продолжать по её фактическому request/state, не запускать повторно. Прямой ID выше порядка. При NEXT немедленно перейти к названному ID; запрещено продолжать прежнюю задачу или дописывать её отчёт. При STOP немедленно прекратить действия. Живой процесс ждать через его реальный handle до terminal; не завершать раунд вместо ожидания.

Канонический обзор — [EXECUTION-QUEUE.md](../../EXECUTION-QUEUE.md); [бюджет](../../curriculum/scale-plan.json) делится внутри родительского пула, пилот входит в него. Выбранная модель не переопределяется. Checksum, фиксированный профиль модели и production CI не являются условиями подготовки. Режим CREATE_ONLY: запись draft.jsonl → finish → следующий поток; никаких review/oracle/accept/export/обучения.

| Поток | Общий порядок | Следующий поток |
|---|---:|---|
| `TR03-EVENT-ORDER/ru-en` | 150 | `TR03-EVENT-ORDER/he-en` |
| `TR03-EVENT-ORDER/he-en` | 151 | `TR03-EVENT-ORDER/en-ru` |
| `TR03-EVENT-ORDER/en-ru` | 152 | `TR03-EVENT-ORDER/he-ru` |
| `TR03-EVENT-ORDER/he-ru` | 153 | `TR03-EVENT-ORDER/en-he` |
| `TR03-EVENT-ORDER/en-he` | 154 | `TR03-EVENT-ORDER/ru-he` |
| `TR03-EVENT-ORDER/ru-he` | 155 | `OR-AUTO-EVENT-GATE/base` |

## Точные команды действующих потоков

Подготовка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Команды работают из любого каталога. RAG уже внутри пакета. Выполнить только назначенный поток; остальные строки — точные альтернативы назначения.

### TR03-EVENT-ORDER/ru-en

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "TR03-EVENT-ORDER/ru-en" -Run "M001" -Count 10 -Split train -Start 0
```

Для этой цели поставлен только общий TRAIN-план сценариев. DEV/EXAM пока не назначены; `prepare_first_dev` / `prepare_first_exam` равны null. При отдельном поручении сначала создать непересекающиеся сценарии в изолированном контексте и передать реальный файл через `-FamilyPlan`. Не запускать команду со ссылкой на несуществующий план и не превращать TRAIN-примеры в экзамен.

Первая TRAIN-папка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/TR03-EVENT-ORDER/ru-en/M001`. Следующий поток: `TR03-EVENT-ORDER/he-en`. Доля TRAIN: 500 пар; первая партия 10. Пилот TRAIN/DEV/EXAM: 100/20/20; набор `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/datasets/TR03-EVENT-ORDER-ru-en-v1`; B1 `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/baselines/tr.en/ru-en/TR03-EVENT-ORDER-v1/B1`. Это назначенные пути, не существующие веса.

### TR03-EVENT-ORDER/he-en

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "TR03-EVENT-ORDER/he-en" -Run "M001" -Count 10 -Split train -Start 0
```

Для этой цели поставлен только общий TRAIN-план сценариев. DEV/EXAM пока не назначены; `prepare_first_dev` / `prepare_first_exam` равны null. При отдельном поручении сначала создать непересекающиеся сценарии в изолированном контексте и передать реальный файл через `-FamilyPlan`. Не запускать команду со ссылкой на несуществующий план и не превращать TRAIN-примеры в экзамен.

Первая TRAIN-папка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/TR03-EVENT-ORDER/he-en/M001`. Следующий поток: `TR03-EVENT-ORDER/en-ru`. Доля TRAIN: 500 пар; первая партия 10. Пилот TRAIN/DEV/EXAM: 100/20/20; набор `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/datasets/TR03-EVENT-ORDER-he-en-v1`; B1 `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/baselines/tr.en/he-en/TR03-EVENT-ORDER-v1/B1`. Это назначенные пути, не существующие веса.

### TR03-EVENT-ORDER/en-ru

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "TR03-EVENT-ORDER/en-ru" -Run "M001" -Count 10 -Split train -Start 0
```

Для этой цели поставлен только общий TRAIN-план сценариев. DEV/EXAM пока не назначены; `prepare_first_dev` / `prepare_first_exam` равны null. При отдельном поручении сначала создать непересекающиеся сценарии в изолированном контексте и передать реальный файл через `-FamilyPlan`. Не запускать команду со ссылкой на несуществующий план и не превращать TRAIN-примеры в экзамен.

Первая TRAIN-папка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/TR03-EVENT-ORDER/en-ru/M001`. Следующий поток: `TR03-EVENT-ORDER/he-ru`. Доля TRAIN: 500 пар; первая партия 10. Пилот TRAIN/DEV/EXAM: 100/20/20; набор `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/datasets/TR03-EVENT-ORDER-en-ru-v1`; B1 `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/baselines/tr.ru/en-ru/TR03-EVENT-ORDER-v1/B1`. Это назначенные пути, не существующие веса.

### TR03-EVENT-ORDER/he-ru

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "TR03-EVENT-ORDER/he-ru" -Run "M001" -Count 10 -Split train -Start 0
```

Для этой цели поставлен только общий TRAIN-план сценариев. DEV/EXAM пока не назначены; `prepare_first_dev` / `prepare_first_exam` равны null. При отдельном поручении сначала создать непересекающиеся сценарии в изолированном контексте и передать реальный файл через `-FamilyPlan`. Не запускать команду со ссылкой на несуществующий план и не превращать TRAIN-примеры в экзамен.

Первая TRAIN-папка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/TR03-EVENT-ORDER/he-ru/M001`. Следующий поток: `TR03-EVENT-ORDER/en-he`. Доля TRAIN: 500 пар; первая партия 10. Пилот TRAIN/DEV/EXAM: 100/20/20; набор `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/datasets/TR03-EVENT-ORDER-he-ru-v1`; B1 `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/baselines/tr.ru/he-ru/TR03-EVENT-ORDER-v1/B1`. Это назначенные пути, не существующие веса.

### TR03-EVENT-ORDER/en-he

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "TR03-EVENT-ORDER/en-he" -Run "M001" -Count 10 -Split train -Start 0
```

Для этой цели поставлен только общий TRAIN-план сценариев. DEV/EXAM пока не назначены; `prepare_first_dev` / `prepare_first_exam` равны null. При отдельном поручении сначала создать непересекающиеся сценарии в изолированном контексте и передать реальный файл через `-FamilyPlan`. Не запускать команду со ссылкой на несуществующий план и не превращать TRAIN-примеры в экзамен.

Первая TRAIN-папка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/TR03-EVENT-ORDER/en-he/M001`. Следующий поток: `TR03-EVENT-ORDER/ru-he`. Доля TRAIN: 500 пар; первая партия 10. Пилот TRAIN/DEV/EXAM: 100/20/20; набор `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/datasets/TR03-EVENT-ORDER-en-he-v1`; B1 `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/baselines/tr.he/en-he/TR03-EVENT-ORDER-v1/B1`. Это назначенные пути, не существующие веса.

### TR03-EVENT-ORDER/ru-he

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "TR03-EVENT-ORDER/ru-he" -Run "M001" -Count 10 -Split train -Start 0
```

Для этой цели поставлен только общий TRAIN-план сценариев. DEV/EXAM пока не назначены; `prepare_first_dev` / `prepare_first_exam` равны null. При отдельном поручении сначала создать непересекающиеся сценарии в изолированном контексте и передать реальный файл через `-FamilyPlan`. Не запускать команду со ссылкой на несуществующий план и не превращать TRAIN-примеры в экзамен.

Первая TRAIN-папка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/TR03-EVENT-ORDER/ru-he/M001`. Следующий поток: `OR-AUTO-EVENT-GATE/base`. Доля TRAIN: 500 пар; первая партия 10. Пилот TRAIN/DEV/EXAM: 100/20/20; набор `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/datasets/TR03-EVENT-ORDER-ru-he-v1`; B1 `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/baselines/tr.he/ru-he/TR03-EVENT-ORDER-v1/B1`. Это назначенные пути, не существующие веса.

<!-- GENERATED-QUEUE-END -->
