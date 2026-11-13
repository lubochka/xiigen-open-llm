# OR-ARCH-EVENT-CONTRACT — Передача канонического имени события через границу компонентов

<!-- CREATE-ONLY-START -->
> **Действующий режим: CREATE_ONLY — только создание.** Прочитай назначение, создай реальные пары в `draft.jsonl`, выполни один структурный `finish`, перейди к следующему stream ID из общей очереди. Не запускай blind/review/oracle/accept/export/обучение и не жди проверяющего. `DRAFT_CREATED` означает запись кандидатов, а не проверенное качество.
>
> Корень наборов: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets`. Подготовка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Точные команды ниже работают из любого текущего каталога; готовый request содержит абсолютные пути. Не искать инструменты в `llm_mvp_core`, не создавать пустой RAG и не восстанавливать содержимое по checksum.
>
> Модель — текущая выбранная в Kiro, без override; не более одного помощника одновременно. При NEXT/новом ID немедленно перейти к нему, не дописывать прошлую задачу; STOP прекращает действия. Живой процесс ждать по его handle штатным wait/resume; timeout транспорта не доказывает отказ. [Точный контракт](../../20-CREATE-ONLY-AND-PATHS.md). Старые правила приёмки ниже описывают только будущую стадию.
> Роль человека при выборе материала выполняет CODEX с Astra 6 ultracode; оценку качества DPO также выполняет CODEX с Astra 6 ultracode. Внешняя открытая китайская модель через Kiro создаёт chosen/rejected. В CREATE_ONLY назначение уже задано карточкой/request: новый вызов выбора и оценка сейчас не запускаются и не задерживают создание следующей партии. Это назначение ролей, не подтверждение выполненного вызова.
<!-- CREATE-ONLY-END -->

## Узкая задача

Сохранять точную идентичность события между producer, subscriber и registration, различая активный контракт, подпись и устаревшую версию.

Родитель: `OR-ARCH-02`. Предметные домены: `or.architecture`. Профили: `base`. Один request содержит только назначенный профиль и эту микроцель.

Статус: авторский материал подготовки. Публичные пары ниже — reference; их нельзя копировать в новый корпус или выдавать за скрытый экзамен. Китайская генерация, оценка Astra и Student для нового материала не выполнялись; ожидаемые результаты не являются измерениями.

## Проверенные источники и применимость

- [RAG-OR-007](../../rag/sources/RAG-OR-007.md): закреплённая методика или исходная тема; конкретные новые случаи являются авторскими симуляциями.
- [RAG-006](../../rag/sources/RAG-006.md): закреплённая методика или исходная тема; конкретные новые случаи являются авторскими симуляциями.

[Разобранные good/bad примеры и логика](../../prompts/ORCHESTRATION-EVENTS-AND-EVIDENCE-RU.md). Основное задание и все reference выбранного профиля автоматически входят в batch prompt.

## Подробный промпт исполнителю

Создавай handoff одного события. В prompt всегда пиши действующий wire_event, отдельно display_name/исторический вариант и правило сравнения. Ответ должен содержать точное поле wire_event. Не выводи нужное имя из русского перевода или общей похожести. В 10 парах запланируй 3 исторических переименования, 3 display-name ошибки, 2 версии и 2 различия регистра. Для каждого вида дай контрфактический контроль: если активный контракт явно меняется, правильным становится прежний rejected. Самопроверка: скопируй активное значение отдельно; сравни с chosen посимвольно и сравни с rejected. Chosen должен совпасть, rejected отличаться ровно по одному аспекту. Если контракт разрешает оба имени как alias и не требует канонического написания, нельзя сделать одно rejected: пометь ambiguity и создай другую новую пару. Это архитектурная согласованность данных handoff, не обучение переводам и не внедрение event bus.

Текущий режим — CREATE_ONLY: внешняя открытая китайская модель, выбранная в Kiro, создаёт кандидаты DPO. Роль человека при выборе материала и последующую оценку качества выполняет CODEX с Astra 6 ultracode; в этой партии не запускать review, обучение, экспорт, production CI и checksum-гейты. Имя реально вызванной модели не придумывать. Следовать одному objective_id/profile_id/split из request.json; не смешивать поддомены и не обучать router ответами исполнителя. Подготовить ровно 10 новых кандидатных пар либо count из request, по одной явно видимой ошибке между chosen/rejected. Обе альтернативы имеют один формат и решают одну задачу; комментарий о правильности, rationale и источник находятся вне ответов ученика. Не превращать другой разрешённый способ решения в rejected. Все факты, доступные действия, правило сравнения и значения контракта должны быть видны в prompt. После собственного простого предметного разбора записать draft; минимальная структурная проверка finish не означает оценку качества. Затем перейти к следующему stream из registry, не запускать reviewer. Старые данные и попытки сохранять; производные одного сценария, перестановки ответов, переименования и контрфактические варианты держать в одном origin/family/leakage_group и split. Показанные reference не превращать в скрытый экзамен. Команды, источники и output брать из абсолютных request.paths: корень наборов C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets, preparation рядом. Не искать runner в другом проекте и не сочинять недостающий источник. Скилы: 06-SOURCES-AND-SKILLS.md; отчёт — по-русски, кратко по 08-REPORT-TEMPLATE.md с числом реально созданных строк, путём, статусом not_requested для review и следующим ID. При NEXT/следующий/новый ID немедленно перейти к указанному ID и не продолжать прежнюю задачу. При прямом STOP/стоп прекратить действия немедленно. Для реально запущенной длительной работы использовать штатный wait/resume того же handle до terminal: не дублировать запуск, не устраивать частый polling, не завершать ход вместо ожидания. Отсутствие stdout не доказывает отказ. Ни одна симуляция ниже не является наблюдением обучения. Последующий учебный пилот назначается отдельно: нужен настоящий обучаемый механизм, восстановление совместимого B1 или однократный producer, достаточный бюджет и полезное измерение до/после; 0→0 правильных ответов не является валидным учебным результатом. Любое измеренное улучшение полезного показателя в валидном обучении — успех по этому показателю и домену, с обязательным показом остальных показателей и ухудшений.

## Форма ответа ученика

JSON {"wire_event":"точное активное значение из prompt"}; без display_name, комментариев и скрытых поправок регистра.

## Что варьировать

```json
[
  {
    "axis": "misleading_alternative",
    "values": [
      "historical_name",
      "display_name",
      "future_version",
      "case_variant"
    ]
  },
  {
    "axis": "active_contract",
    "values": [
      "old_explicitly_active",
      "new_explicitly_active"
    ]
  }
]
```

## Разобранные пары

### 1. OR-ARCH-EVENT-CONTRACT-REF-01 — base

**Задача ученику:**

~~~~text
Симуляция архитектурного handoff. Producer, subscriber и cross-flow registration обязаны использовать один wire_event, ровно как в активном контракте ниже. Имена display_name и исторические подписи не являются alias. Сравнение регистрозависимое, неявной миграции нет. Верни JSON wire_event для передачи всем трём участникам. Активный контракт: wire_event="user.onboarding.broadcast.sent". Display_name="OnboardingBroadcastSent". В старой сессии осталось "OnboardingCompleted".
~~~~

**Chosen — полный правильный ответ:**

~~~~text
{"wire_event":"user.onboarding.broadcast.sent"}
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
{"wire_event":"OnboardingCompleted"}
~~~~

**Почему есть предпочтение:** Источник имени — активный wire-контракт, а не старый рассказ о событии.

Различающий дефект: `historical_event_name_as_contract`. Семья: `event-evidence-ref.or-arch-event-contract.scenario01`.

### 2. OR-ARCH-EVENT-CONTRACT-REF-02 — base

**Задача ученику:**

~~~~text
Симуляция архитектурного handoff. Producer, subscriber и cross-flow registration обязаны использовать один wire_event, ровно как в активном контракте ниже. Имена display_name и исторические подписи не являются alias. Сравнение регистрозависимое, неявной миграции нет. Верни JSON wire_event для передачи всем трём участникам. Активный контракт этой симуляции: wire_event="OnboardingCompleted". Черновик будущей версии предлагает "user.onboarding.broadcast.sent", но миграция ещё не принята.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
{"wire_event":"OnboardingCompleted"}
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
{"wire_event":"user.onboarding.broadcast.sent"}
~~~~

**Почему есть предпочтение:** Будущее предложение не изменяет явно действующий контракт.

Различающий дефект: `future_event_name_as_current`. Семья: `event-evidence-ref.or-arch-event-contract.scenario01`.

### 3. OR-ARCH-EVENT-CONTRACT-REF-03 — base

**Задача ученику:**

~~~~text
Симуляция архитектурного handoff. Producer, subscriber и cross-flow registration обязаны использовать один wire_event, ровно как в активном контракте ниже. Имена display_name и исторические подписи не являются alias. Сравнение регистрозависимое, неявной миграции нет. Верни JSON wire_event для передачи всем трём участникам. Активный контракт: wire_event="order.payment.confirmed". UI подписывает это событие "PaymentConfirmed"; dispatcher не регистрирует UI подписи.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
{"wire_event":"order.payment.confirmed"}
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
{"wire_event":"PaymentConfirmed"}
~~~~

**Почему есть предпочтение:** Понятная подпись UI не зарегистрирована как машинный идентификатор.

Различающий дефект: `display_label_used_as_wire_type`. Семья: `event-evidence-ref.or-arch-event-contract.scenario01`.

### 4. OR-ARCH-EVENT-CONTRACT-REF-04 — base

**Задача ученику:**

~~~~text
Симуляция архитектурного handoff. Producer, subscriber и cross-flow registration обязаны использовать один wire_event, ровно как в активном контракте ниже. Имена display_name и исторические подписи не являются alias. Сравнение регистрозависимое, неявной миграции нет. Верни JSON wire_event для передачи всем трём участникам. Активный контракт: wire_event="PaymentConfirmed". "order.payment.confirmed" — только пример в старом руководстве, alias не задан.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
{"wire_event":"PaymentConfirmed"}
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
{"wire_event":"order.payment.confirmed"}
~~~~

**Почему есть предпочтение:** Наличие namespace не делает старый пример авторитетнее текущего контракта.

Различающий дефект: `example_namespace_overrides_contract`. Семья: `event-evidence-ref.or-arch-event-contract.scenario01`.

### 5. OR-ARCH-EVENT-CONTRACT-REF-05 — base

**Задача ученику:**

~~~~text
Симуляция архитектурного handoff. Producer, subscriber и cross-flow registration обязаны использовать один wire_event, ровно как в активном контракте ниже. Имена display_name и исторические подписи не являются alias. Сравнение регистрозависимое, неявной миграции нет. Верни JSON wire_event для передачи всем трём участникам. Активный контракт: wire_event="profile.updated.v2". Старый consumer-list ещё содержит "profile.updated.v1"; требуется обновить handoff, чтобы все три участника использовали активный тип.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
{"wire_event":"profile.updated.v2"}
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
{"wire_event":"profile.updated.v1"}
~~~~

**Почему есть предпочтение:** Последний сегмент является частью идентичности; v1 и v2 не объявлены совместимыми alias.

Различающий дефект: `wire_version_lost`. Семья: `event-evidence-ref.or-arch-event-contract.scenario01`.

### 6. OR-ARCH-EVENT-CONTRACT-REF-06 — base

**Задача ученику:**

~~~~text
Симуляция архитектурного handoff. Producer, subscriber и cross-flow registration обязаны использовать один wire_event, ровно как в активном контракте ниже. Имена display_name и исторические подписи не являются alias. Сравнение регистрозависимое, неявной миграции нет. Верни JSON wire_event для передачи всем трём участникам. Активный контракт: wire_event="profile.updated.v1". Версия v2 есть только в предложении, реализация и подписчики остаются на v1.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
{"wire_event":"profile.updated.v1"}
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
{"wire_event":"profile.updated.v2"}
~~~~

**Почему есть предпочтение:** Больший номер версии не означает действующую версию.

Различающий дефект: `unapproved_wire_version`. Семья: `event-evidence-ref.or-arch-event-contract.scenario01`.

### 7. OR-ARCH-EVENT-CONTRACT-REF-07 — base

**Задача ученику:**

~~~~text
Симуляция архитектурного handoff. Producer, subscriber и cross-flow registration обязаны использовать один wire_event, ровно как в активном контракте ниже. Имена display_name и исторические подписи не являются alias. Сравнение регистрозависимое, неявной миграции нет. Верни JSON wire_event для передачи всем трём участникам. Активный контракт: wire_event="EventEnded". В registry отсутствует "eventEnded". Dispatcher сравнивает строки с учётом регистра.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
{"wire_event":"EventEnded"}
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
{"wire_event":"eventEnded"}
~~~~

**Почему есть предпочтение:** При заданном case-sensitive сравнении изменение первой буквы меняет тип события.

Различающий дефект: `wire_case_changed`. Семья: `event-evidence-ref.or-arch-event-contract.scenario01`.

### 8. OR-ARCH-EVENT-CONTRACT-REF-08 — base

**Задача ученику:**

~~~~text
Симуляция архитектурного handoff. Producer, subscriber и cross-flow registration обязаны использовать один wire_event, ровно как в активном контракте ниже. Имена display_name и исторические подписи не являются alias. Сравнение регистрозависимое, неявной миграции нет. Верни JSON wire_event для передачи всем трём участникам. Активный контракт: wire_event="eventEnded". "EventEnded" — display_name, dispatcher сравнивает строки с учётом регистра.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
{"wire_event":"eventEnded"}
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
{"wire_event":"EventEnded"}
~~~~

**Почему есть предпочтение:** Правильное написание определено активным полем, не привычным стилем PascalCase.

Различающий дефект: `display_case_overrides_wire_case`. Семья: `event-evidence-ref.or-arch-event-contract.scenario01`.

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
| `OR-ARCH-EVENT-CONTRACT/base` | 158 | `OR-QA-SCORE-PRESENCE/base` |

## Точные команды действующих потоков

Подготовка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Команды работают из любого каталога. RAG уже внутри пакета. Выполнить только назначенный поток; остальные строки — точные альтернативы назначения.

### OR-ARCH-EVENT-CONTRACT/base

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "OR-ARCH-EVENT-CONTRACT/base" -Run "M001" -Count 10 -Split train -Start 0
```

DEV/EXAM не запускать попутно с TRAIN. Когда назначено отдельное создание этих split в изолированном контексте, точные команды сохранены в `curriculum/pilot-queue.json` как `prepare_first_dev` / `prepare_first_exam`.

Первая TRAIN-папка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/OR-ARCH-EVENT-CONTRACT/base/M001`. Следующий поток: `OR-QA-SCORE-PRESENCE/base`. Доля TRAIN: 1000 пар; первая партия 10. Пилот TRAIN/DEV/EXAM: 100/20/20; набор `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/datasets/OR-ARCH-EVENT-CONTRACT-base-v1`; B1 `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/baselines/or.architecture/base/OR-ARCH-EVENT-CONTRACT-v1/B1`. Это назначенные пути, не существующие веса.

<!-- GENERATED-QUEUE-END -->
