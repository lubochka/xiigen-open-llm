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
| `A174` — `DOC-NG-CONFIG-READY` | `docs.angular` / `base` | 160 | `A175` |


Следующий ID из таблицы — номинальный порядок своей группы. Фактическую свободную карточку с учётом владельцев выдаёт команда `next` текущего request; не менять группу и чужой статус вручную.

Замечания качества: смотри `quality_flags` своего domain/topic/profile в общем status. `needs_rework` указывает доказанный дефект с evidence, `needs_assessment` — область без достаточной оценки. Это не новый review-гейт и не разрешение исправлять чужую старую попытку; finish не закрывает флаг автоматически. [Правила общей матрицы](../../28-PARALLEL-GENERATION-AND-QUALITY.md). Для нового окна используй [один из трёх готовых стартовых промптов](../../00-START-HERE.md); не исполняй исторические команды из нижней справочной части вместо текущих owner-команд.

<!-- /CURRENT-PARALLEL-CONTRACT-20261026 -->

# DOC-NG-CONFIG-READY — Описать фактическую готовность runtime config

<!-- DOMAIN-CAMPAIGN-AUTHORITY -->
> **Предметная справка к действующей кампании.** Квоты и продолжение задаёт [campaign/registry.json](../../campaign/registry.json), не старые команды ниже. 1000 пар относятся к домену в сумме. Прочитанный генератором рабочий prompt содержит только назначенную микротему.
>
> Данные: `C:/Projects/llm_mvp_core/learning-sets`. Инструкции: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. CREATE_ONLY: без review, checksum, production CI и обучения. Модель — текущая Kiro без override. Reason internally in English; do not output private reasoning. Отчёт по-русски.
>
> После партии выполнять готовый next до доменной квоты; не останавливаться с вопросом «продолжать?». При прямом NEXT/новом ID немедленно сменить задачу: запрещено дописывать/чинить предыдущую. STOP прекращает действия. Один живой handle ждать документированным wait/resume; не объявлять молчание падением. Старые попытки сохранять; выбранный request возобновлять, не пересоздавать.

Точные доли этой предметной карточки в текущей кампании:
| A-ID | Домен | Профиль | Доля из доменных1000 | Следующий A-ID после доли |
|---|---|---|---:|---|
| A174 | docs.angular | base | 160 | A175 |


Ниже — сохранённая предметная методика и исторические инструкции. [Полный прежний вариант](../../history/before-domain-campaign-2026-10-25/curriculum/cards/DOC-NG-CONFIG-READY.md). Старые числовые планы и команды в нём не запускаются вместо актуального request.
<!-- /DOMAIN-CAMPAIGN-AUTHORITY -->

## Узкая задача

Отличить вызов загрузки, завершение возвращённого Promise и фактическую готовность конфигурации по показанному коду.

Родитель: `DOC-NG-03`. Предметные домены: `docs.angular`. Профили: `base`. Один request содержит только назначенный профиль и эту микроцель.

Статус: авторский материал подготовки. Публичные пары ниже — reference; их нельзя копировать в новый корпус или выдавать за скрытый экзамен. Китайская генерация, оценка Astra и Student для нового материала не выполнялись; ожидаемые результаты не являются измерениями.

## Проверенные источники и применимость

- [RAG-027](../../rag/sources/RAG-027.md): закреплённая методика или исходная тема; конкретные новые случаи являются авторскими симуляциями.
- [RAG-028](../../rag/sources/RAG-028.md): закреплённая методика или исходная тема; конкретные новые случаи являются авторскими симуляциями.
- [RAG-037](../../rag/sources/RAG-037.md): закреплённая методика или исходная тема; конкретные новые случаи являются авторскими симуляциями.

[Разобранные good/bad примеры и логика](../../prompts/SOFTWARE-PIPELINE-WINDOW-READY-RU.md). Основное задание и все reference выбранного профиля автоматически входят в batch prompt.

## Подробный промпт исполнителю

Создавай короткую TypeScript-симуляцию runtime config по проектной границе из RAG-037. В prompt всегда покажи локальный host-контракт: что initializer возвращает, какой Promise host ожидает и когда разрешено читать snapshot. Не используй provideAppInitializer как будто этот API есть во всех версиях Angular. Достаточны локальные функции с полными интерфейсами.
Документация должна точно описывать и неправильный показанный loader: преждевременный ready, потерянный return Promise, проглоченную ошибку. Не подменяй фактическое поведение кодом, который должен был быть написан. Для нескольких потребителей покажи полный single-flight код и счётчик fetch. Для reload задай начальное состояние. Не заявляй серверный rollback, скрытую последовательность guards либо реальный запуск host.

Простая самопроверка:
1. Выпиши состояние до вызова, при pending, после fulfilled и после rejected.
2. Проверь, какой именно Promise получает host, либо что он получает undefined.
3. Пройди каждый catch до конца: есть ли throw, return или только логирование.
4. При двух потребителях отдельно сосчитай вызовы fetch.
5. Проверь getter snapshot по его телу; одно наличие cached не отменяет guard.

Работай по одному готовому request.json: один topic_id, objective_id, domain_id, profile_id и split. В каждом prompt повтори среду, полный видимый код/данные, точку наблюдения и форму ответа: metadata не попадает в строгую проекцию prompt/chosen/rejected. Создай полный chosen и близкий rejected с одним основным содержательным дефектом, одинакового формата. Не заменяй rejected синтаксической ошибкой, шуткой, заведомо коротким ответом или другой правильной реализацией. Объяснение проверки, source_refs и error_tag записывай вне ответов.
Сначала выпиши маленькую таблицу состояния либо результата по условию, затем сравни с ней обе альтернативы. Это самопроверка автора кандидатов, не независимая оценка качества и не измерение обучения. Если по указанному входу ответы одинаковы, измени различающий вход в пределах задания; не придумывай победу. Смена имени или чисел сама по себе не создаёт независимую семью. Производные, обратные и DOC/CODE-варианты конкретного случая наследуют общий origin/leakage group и split. Публичные reference не являются закрытым EXAM.
Источники — только назначенные файлы xiigen-planning main/llm_plan3. Они дают методику и исходную тематику. Новые фрагменты являются авторскими симуляциями; runtime/oracle сейчас NOT_RUN. Не объявляй новую симуляцию извлечённым production-кодом, измеренным результатом или китайской генерацией.
Роль человека при выборе материала выполняет CODEX с Astra 6 ultracode; оценку качества DPO также выполняет CODEX с Astra 6 ultracode. Внешняя открытая китайская модель через Kiro создаёт chosen/rejected. Сейчас CREATE_ONLY: назначение уже дано карточкой/request. Не запускай новые selection/review/oracle/accept/export/Student/production CI и не задерживай следующую партию ради них. Модель — фактически текущая выбранная в Kiro, без override; неизвестное имя/session запиши unknown. Checksum, подписи, profile receipts и независимый review не нужны для создания.
Корень наборов C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets; подготовка находится в preparation внутри него. Возьми абсолютные входные и выходные пути из request.paths, не ищи runner в llm_mvp_core. После записи реальных JSONL-строк выполни одну готовую команду finish, получи структурный DRAFT_CREATED и перейди к next_stream_id полной execution-queue.json. PREPARED без draft не считается созданием. Старые данные/попытки не удалять; существующий run продолжать через resume.
При прямом NEXT/новом ID немедленно перейти к нему: запрещено продолжать предыдущую задачу, её отчёт или исправления. STOP прекращает действия сразу. Живой процесс ждать штатным wait/resume по единственному handle до terminal в том же ходе; не запускать дубликаты, не делать частый polling и не завершать ход словом «жду». Отчёт по-русски: тема, созданные строки/семьи, точный файл, фактическая модель, структурный статус, следующий ID. Нельзя приписывать вызов Astra текущей модели.

## Форма ответа ученика

Короткий русский текст с точным состоянием/числом вызовов по заданной точке наблюдения; без исправления кода.

## Что варьировать

```json
[
  {
    "axis": "стадия",
    "values": [
      "pending",
      "fulfilled",
      "rejected",
      "зависимости pending",
      "reload"
    ]
  },
  {
    "axis": "связь с host",
    "values": [
      "Promise возвращён",
      "Promise потерян",
      "ошибка проглочена",
      "single-flight"
    ]
  }
]
```

## Разобранные пары

### 1. DOC-NG-CONFIG-READY.REF.01 — base

**Задача ученику:**

~~~~text
Авторская симуляция Angular 16.2.0 / TypeScript 5.1.6. Это изолированный Promise-fixture, а не выполненный Angular bootstrap. По умолчанию type Config={apiBaseUrl:string}; если задача явно расширяет тип, используется показанное расширение. Локальный host ждёт только возвращённый Promise initializer; если возвращён undefined, он не ждёт загрузку. Указанные завершённые состояния наблюдаются после await соответствующей задачи; pending означает, что соответствующий Promise ещё не завершён. Иных конкурентных изменений нет. let ready=false; let snapshot:Config|null=null; async function load(){const c=await fetchConfig();snapshot=c;ready=true;} Функция fetchConfig():Promise<Config> задана окружением. Вызов var loadTask=load(); её Promise ещё pending. Назови ready/snapshot до завершения fetch. Затем fetch успешно возвращает {apiBaseUrl:"/v1"}; выполнено await loadTask. Назови новое состояние.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
До завершения fetch: ready=false,snapshot=null. После успешного завершения loadTask: ready=true,snapshot={apiBaseUrl:"/v1"}.
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
До завершения fetch: ready=true,snapshot=null. После успешного завершения loadTask: ready=true,snapshot={apiBaseUrl:"/v1"}.
~~~~

**Почему есть предпочтение:** Флаг изменяется после await, а не при начале вызова.

Различающий дефект: `ready_inferred_from_invocation`. Семья: `v7.ref.config.load-transition`.

### 2. DOC-NG-CONFIG-READY.REF.02 — base

**Задача ученику:**

~~~~text
Авторская симуляция Angular 16.2.0 / TypeScript 5.1.6. Это изолированный Promise-fixture, а не выполненный Angular bootstrap. По умолчанию type Config={apiBaseUrl:string}; если задача явно расширяет тип, используется показанное расширение. Локальный host ждёт только возвращённый Promise initializer; если возвращён undefined, он не ждёт загрузку. Указанные завершённые состояния наблюдаются после await соответствующей задачи; pending означает, что соответствующий Promise ещё не завершён. Иных конкурентных изменений нет. let ready=false; let snapshot:Config|null=null; async function load(){ready=true;snapshot=await fetchConfig();} Функция fetchConfig():Promise<Config> ещё pending. Вызван load(), fetch ещё не завершён. Назови состояние и поясни, доказывает ли ready наличие config в этом коде.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
ready=true,snapshot=null. Показанный loader выставляет ready преждевременно; этот флаг здесь не доказывает наличие config.
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
ready=true,snapshot={apiBaseUrl:"/v1"}. Ready=true гарантирует, что присваивание после await уже выполнено.
~~~~

**Почему есть предпочтение:** Rejected утверждает, что присваивание после await уже выполнено.

Различающий дефект: `flag_treated_as_unconditional_truth`. Семья: `v7.ref.config.early-ready`.

### 3. DOC-NG-CONFIG-READY.REF.03 — base

**Задача ученику:**

~~~~text
Авторская симуляция Angular 16.2.0 / TypeScript 5.1.6. Это изолированный Promise-fixture, а не выполненный Angular bootstrap. По умолчанию type Config={apiBaseUrl:string}; если задача явно расширяет тип, используется показанное расширение. Локальный host ждёт только возвращённый Promise initializer; если возвращён undefined, он не ждёт загрузку. Указанные завершённые состояния наблюдаются после await соответствующей задачи; pending означает, что соответствующий Promise ещё не завершён. Иных конкурентных изменений нет. Функция load():Promise<void> задана окружением и завершается только после загрузки config. const init=()=>{void load();}; Host вызвал init(); load ещё pending. Ждёт ли host загрузку по данному контракту?
~~~~

**Chosen — полный правильный ответ:**

~~~~text
Нет. Init возвращает undefined; вызов load есть, но его Promise не возвращён host.
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
Да. Сам вызов load внутри init автоматически заставляет host дождаться его Promise.
~~~~

**Почему есть предпочтение:** Наличие вызова не равнозначно возвращению его Promise.

Различающий дефект: `promise_not_returned_to_host`. Семья: `v7.ref.config.initializer-return`.

### 4. DOC-NG-CONFIG-READY.REF.04 — base

**Задача ученику:**

~~~~text
Авторская симуляция Angular 16.2.0 / TypeScript 5.1.6. Это изолированный Promise-fixture, а не выполненный Angular bootstrap. По умолчанию type Config={apiBaseUrl:string}; если задача явно расширяет тип, используется показанное расширение. Локальный host ждёт только возвращённый Promise initializer; если возвращён undefined, он не ждёт загрузку. Указанные завершённые состояния наблюдаются после await соответствующей задачи; pending означает, что соответствующий Promise ещё не завершён. Иных конкурентных изменений нет. let ready=false; let snapshot:Config|null=null; async function load(){const c=await fetchConfig();snapshot=c;ready=true;} fetchConfig():Promise<Config> отклоняется Error("load"). Host ожидает load() и обрабатывает отказ снаружи. Каковы состояние и статус Promise load?
~~~~

**Chosen — полный правильный ответ:**

~~~~text
Ready остаётся false,snapshot остаётся null; load отклонён той же ошибкой. Код после await не выполнен.
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
Ready становится true,snapshot остаётся null; load отклонён той же ошибкой. Флаг выставляется при любом завершении fetch.
~~~~

**Почему есть предпочтение:** После rejected await обычное продолжение пропускается.

Различающий дефект: `rejection_treated_as_success_completion`. Семья: `v7.ref.config.load-transition`.

### 5. DOC-NG-CONFIG-READY.REF.05 — base

**Задача ученику:**

~~~~text
Авторская симуляция Angular 16.2.0 / TypeScript 5.1.6. Это изолированный Promise-fixture, а не выполненный Angular bootstrap. По умолчанию type Config={apiBaseUrl:string}; если задача явно расширяет тип, используется показанное расширение. Локальный host ждёт только возвращённый Promise initializer; если возвращён undefined, он не ждёт загрузку. Указанные завершённые состояния наблюдаются после await соответствующей задачи; pending означает, что соответствующий Promise ещё не завершён. Иных конкурентных изменений нет. let ready=false; let snapshot:Config|null=null; const log:string[]=[]; async function load(){try{snapshot=await fetchConfig();ready=true;}catch{log.push("failed");}} fetchConfig():Promise<Config> отклоняется. Host выполнил await load(). Назови ready, статус load и log. Можно ли назвать config готовой?
~~~~

**Chosen — полный правильный ответ:**

~~~~text
Ready=false;load выполнен успешно;log=["failed"]. Ошибка проглочена, поэтому fulfilled load здесь не доказывает готовность config.
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
Ready=false;load отклонён;log=["failed"]. Catch автоматически передаёт ошибку ожидающему host.
~~~~

**Почему есть предпочтение:** В catch нет throw; Promise load успешно завершается.

Различающий дефект: `swallowed_error_mistaken_for_rethrow`. Семья: `v7.ref.config.swallowed-failure`.

### 6. DOC-NG-CONFIG-READY.REF.06 — base

**Задача ученику:**

~~~~text
Авторская симуляция Angular 16.2.0 / TypeScript 5.1.6. Это изолированный Promise-fixture, а не выполненный Angular bootstrap. По умолчанию type Config={apiBaseUrl:string}; если задача явно расширяет тип, используется показанное расширение. Локальный host ждёт только возвращённый Promise initializer; если возвращён undefined, он не ждёт загрузку. Указанные завершённые состояния наблюдаются после await соответствующей задачи; pending означает, что соответствующий Promise ещё не завершён. Иных конкурентных изменений нет. let calls=0; let pending:Promise<Config>|undefined; function fetchConfig(){calls++;return new Promise<Config>(()=>{});} function whenReady(){return pending??=fetchConfig();} const a=whenReady(); const b=whenReady(); Назови calls и a===b; Promise ещё pending.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
calls=1;a===b равно true. Второй потребитель получает уже сохранённый pending Promise.
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
calls=2;a===b равно false. Каждый вызов whenReady запускает новый fetch.
~~~~

**Почему есть предпочтение:** Не учтено условное присваивание pending??=.

Различающий дефект: `shared_promise_ignored`. Семья: `v7.ref.config.single-flight`.

### 7. DOC-NG-CONFIG-READY.REF.07 — base

**Задача ученику:**

~~~~text
Авторская симуляция Angular 16.2.0 / TypeScript 5.1.6. Это изолированный Promise-fixture, а не выполненный Angular bootstrap. По умолчанию type Config={apiBaseUrl:string}; если задача явно расширяет тип, используется показанное расширение. Локальный host ждёт только возвращённый Promise initializer; если возвращён undefined, он не ждёт загрузку. Указанные завершённые состояния наблюдаются после await соответствующей задачи; pending означает, что соответствующий Promise ещё не завершён. Иных конкурентных изменений нет. let ready=false; const cached:Config={apiBaseUrl:"/default"}; function getSnapshot(){if(!ready)throw new Error("not-ready");return cached;} До load вызывается getSnapshot(). Что наблюдает вызывающий код?
~~~~

**Chosen — полный правильный ответ:**

~~~~text
Ошибка "not-ready". Наличие cached не отменяет явную проверку ready.
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
Объект {apiBaseUrl:"/default"}. Поле cached уже заполнено, поэтому getter возвращает его без загрузки.
~~~~

**Почему есть предпочтение:** В rejected пропущен показанный throw.

Различающий дефект: `cached_value_bypasses_guard`. Семья: `v7.ref.config.snapshot-guard`.

### 8. DOC-NG-CONFIG-READY.REF.08 — base

**Задача ученику:**

~~~~text
Авторская симуляция Angular 16.2.0 / TypeScript 5.1.6. Это изолированный Promise-fixture, а не выполненный Angular bootstrap. По умолчанию type Config={apiBaseUrl:string}; если задача явно расширяет тип, используется показанное расширение. Локальный host ждёт только возвращённый Promise initializer; если возвращён undefined, он не ждёт загрузку. Указанные завершённые состояния наблюдаются после await соответствующей задачи; pending означает, что соответствующий Promise ещё не завершён. Иных конкурентных изменений нет. let ready=false; let snapshot:Config|null=null; async function load(){const c=await fetchConfig();snapshot=c;await reloadFlags(c);ready=true;} Функции fetchConfig():Promise<Config> и reloadFlags(c:Config):Promise<void> даны окружением. Fetch уже вернул {apiBaseUrl:"/v1"}, reloadFlags ещё pending. Назови состояние. В этой симуляции договор готовности включает завершение reloadFlags; это явная вариация, а не утверждение, что исходный ADR использует асинхронный reloadFlags.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
Snapshot={apiBaseUrl:"/v1"};ready=false. Конфигурация присвоена, но обязательный reloadFlags ещё не завершён.
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
Snapshot={apiBaseUrl:"/v1"};ready=true. Получения config достаточно, чтобы закончить load до reloadFlags.
~~~~

**Почему есть предпочтение:** Rejected пропускает обязательную вторую async-зависимость.

Различающий дефект: `dependency_completion_omitted`. Семья: `v7.ref.config.dependent-flags`.

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
| `DOC-NG-CONFIG-READY/base` | 164 | `CODE-NG-CONFIG-READY/base` |

## Точные команды действующих потоков

Подготовка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Команды работают из любого каталога. RAG уже внутри пакета. Выполнить только назначенный поток; остальные строки — точные альтернативы назначения.

### DOC-NG-CONFIG-READY/base

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "DOC-NG-CONFIG-READY/base" -Run "S705" -Count 10 -Split train -Start 0
```

DEV/EXAM не запускать попутно с TRAIN. Когда назначено отдельное создание этих split в изолированном контексте, точные команды сохранены в `curriculum/pilot-queue.json` как `prepare_first_dev` / `prepare_first_exam`.

Первая TRAIN-папка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/DOC-NG-CONFIG-READY/base/S705`. Следующий поток: `CODE-NG-CONFIG-READY/base`. Доля TRAIN: 1000 пар; первая партия 10. Пилот TRAIN/DEV/EXAM: 100/20/20; набор `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/datasets/DOC-NG-CONFIG-READY-base-v1`; B1 `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/baselines/docs.angular/base/DOC-NG-CONFIG-READY-v1/B1`. Это назначенные пути, не существующие веса.

<!-- GENERATED-QUEUE-END -->
