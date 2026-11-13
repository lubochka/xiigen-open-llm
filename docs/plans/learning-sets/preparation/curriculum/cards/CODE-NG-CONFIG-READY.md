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
| `A192` — `CODE-NG-CONFIG-READY` | `code.angular` / `base` | 160 | `POOL_END; проверить оставшиеся квоты` |


Следующий ID из таблицы — номинальный порядок своей группы. Фактическую свободную карточку с учётом владельцев выдаёт команда `next` текущего request; не менять группу и чужой статус вручную.

Замечания качества: смотри `quality_flags` своего domain/topic/profile в общем status. `needs_rework` указывает доказанный дефект с evidence, `needs_assessment` — область без достаточной оценки. Это не новый review-гейт и не разрешение исправлять чужую старую попытку; finish не закрывает флаг автоматически. [Правила общей матрицы](../../28-PARALLEL-GENERATION-AND-QUALITY.md). Для нового окна используй [один из трёх готовых стартовых промптов](../../00-START-HERE.md); не исполняй исторические команды из нижней справочной части вместо текущих owner-команд.

<!-- /CURRENT-PARALLEL-CONTRACT-20261026 -->

# CODE-NG-CONFIG-READY — Реализовать ожидание runtime config по видимому контракту

<!-- DOMAIN-CAMPAIGN-AUTHORITY -->
> **Предметная справка к действующей кампании.** Квоты и продолжение задаёт [campaign/registry.json](../../campaign/registry.json), не старые команды ниже. 1000 пар относятся к домену в сумме. Прочитанный генератором рабочий prompt содержит только назначенную микротему.
>
> Данные: `C:/Projects/llm_mvp_core/learning-sets`. Инструкции: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. CREATE_ONLY: без review, checksum, production CI и обучения. Модель — текущая Kiro без override. Reason internally in English; do not output private reasoning. Отчёт по-русски.
>
> После партии выполнять готовый next до доменной квоты; не останавливаться с вопросом «продолжать?». При прямом NEXT/новом ID немедленно сменить задачу: запрещено дописывать/чинить предыдущую. STOP прекращает действия. Один живой handle ждать документированным wait/resume; не объявлять молчание падением. Старые попытки сохранять; выбранный request возобновлять, не пересоздавать.

Точные доли этой предметной карточки в текущей кампании:
| A-ID | Домен | Профиль | Доля из доменных1000 | Следующий A-ID после доли |
|---|---|---|---:|---|
| A192 | code.angular | base | 160 | CAMPAIGN_COMPLETE |


Ниже — сохранённая предметная методика и исторические инструкции. [Полный прежний вариант](../../history/before-domain-campaign-2026-10-25/curriculum/cards/CODE-NG-CONFIG-READY.md). Старые числовые планы и команды в нём не запускаются вместо актуального request.
<!-- /DOMAIN-CAMPAIGN-AUTHORITY -->

## Узкая задача

Написать короткий TypeScript-код с фактическим ожиданием config, точным состоянием ready, передачей ошибок и общим pending Promise.

Родитель: `CODE-NG-05`. Предметные домены: `code.angular`. Профили: `base`. Один request содержит только назначенный профиль и эту микроцель.

Статус: авторский материал подготовки. Публичные пары ниже — reference; их нельзя копировать в новый корпус или выдавать за скрытый экзамен. Китайская генерация, оценка Astra и Student для нового материала не выполнялись; ожидаемые результаты не являются измерениями.

## Проверенные источники и применимость

- [RAG-027](../../rag/sources/RAG-027.md): закреплённая методика или исходная тема; конкретные новые случаи являются авторскими симуляциями.
- [RAG-028](../../rag/sources/RAG-028.md): закреплённая методика или исходная тема; конкретные новые случаи являются авторскими симуляциями.
- [RAG-037](../../rag/sources/RAG-037.md): закреплённая методика или исходная тема; конкретные новые случаи являются авторскими симуляциями.

[Разобранные good/bad примеры и логика](../../prompts/SOFTWARE-PIPELINE-WINDOW-READY-RU.md). Основное задание и все reference выбранного профиля автоматически входят в batch prompt.

## Подробный промпт исполнителю

Пиши одну короткую функцию или метод заданной сигнатуры. Все поля, типы и внешние функции должны присутствовать в prompt. Это TypeScript-реализация явного проектного контракта для Angular 16.2.0, а не универсальный API bootstrap.
Всегда укажи: когда разрешён ready=true; что происходит при rejected fetch; какой Promise ожидает host; нужен ли single-flight; должна ли зависимость config→auth выполняться последовательно. Rejected сохраняет типизацию и меняет одну границу ожидания. Для преждевременного завершения initializer можно вернуть Promise.resolve(): оба ответа остаются Promise<void>, но лишь один действительно ждёт load.

Простая самопроверка:
1. Оставь fetch pending: ready должен соответствовать условию, зависимый auth ещё не запускается.
2. Разреши fetch: присвой config и дождись всех обязательных зависимостей перед ready=true.
3. Отклони fetch: не превращай ошибку в успех, если контракт требует её передать.
4. В initializer проследи возвращаемый Promise до load.
5. При single-flight два вызова whenReady должны вызывать fetch один раз.

Работай по одному готовому request.json: один topic_id, objective_id, domain_id, profile_id и split. В каждом prompt повтори среду, полный видимый код/данные, точку наблюдения и форму ответа: metadata не попадает в строгую проекцию prompt/chosen/rejected. Создай полный chosen и близкий rejected с одним основным содержательным дефектом, одинакового формата. Не заменяй rejected синтаксической ошибкой, шуткой, заведомо коротким ответом или другой правильной реализацией. Объяснение проверки, source_refs и error_tag записывай вне ответов.
Сначала выпиши маленькую таблицу состояния либо результата по условию, затем сравни с ней обе альтернативы. Это самопроверка автора кандидатов, не независимая оценка качества и не измерение обучения. Если по указанному входу ответы одинаковы, измени различающий вход в пределах задания; не придумывай победу. Смена имени или чисел сама по себе не создаёт независимую семью. Производные, обратные и DOC/CODE-варианты конкретного случая наследуют общий origin/leakage group и split. Публичные reference не являются закрытым EXAM.
Источники — только назначенные файлы xiigen-planning main/llm_plan3. Они дают методику и исходную тематику. Новые фрагменты являются авторскими симуляциями; runtime/oracle сейчас NOT_RUN. Не объявляй новую симуляцию извлечённым production-кодом, измеренным результатом или китайской генерацией.
Роль человека при выборе материала выполняет CODEX с Astra 6 ultracode; оценку качества DPO также выполняет CODEX с Astra 6 ultracode. Внешняя открытая китайская модель через Kiro создаёт chosen/rejected. Сейчас CREATE_ONLY: назначение уже дано карточкой/request. Не запускай новые selection/review/oracle/accept/export/Student/production CI и не задерживай следующую партию ради них. Модель — фактически текущая выбранная в Kiro, без override; неизвестное имя/session запиши unknown. Checksum, подписи, profile receipts и независимый review не нужны для создания.
Корень наборов C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets; подготовка находится в preparation внутри него. Возьми абсолютные входные и выходные пути из request.paths, не ищи runner в llm_mvp_core. После записи реальных JSONL-строк выполни одну готовую команду finish, получи структурный DRAFT_CREATED и перейди к next_stream_id полной execution-queue.json. PREPARED без draft не считается созданием. Старые данные/попытки не удалять; существующий run продолжать через resume.
При прямом NEXT/новом ID немедленно перейти к нему: запрещено продолжать предыдущую задачу, её отчёт или исправления. STOP прекращает действия сразу. Живой процесс ждать штатным wait/resume по единственному handle до terminal в том же ходе; не запускать дубликаты, не делать частый polling и не завершать ход словом «жду». Отчёт по-русски: тема, созданные строки/семьи, точный файл, фактическая модель, структурный статус, следующий ID. Нельзя приписывать вызов Astra текущей модели.

## Форма ответа ученика

Только требуемая TypeScript-функция или метод с заданной сигнатурой; без Markdown и пояснений. Все зависимости доступны в prompt.

## Что варьировать

```json
[
  {
    "axis": "обязанность",
    "values": [
      "ready после await",
      "возврат Promise",
      "передача ошибки",
      "single-flight",
      "config перед auth"
    ]
  },
  {
    "axis": "состояние",
    "values": [
      "первая загрузка",
      "reload",
      "pending",
      "rejected",
      "два потребителя"
    ]
  }
]
```

## Разобранные пары

### 1. CODE-NG-CONFIG-READY.REF.01 — base

**Задача ученику:**

~~~~text
Авторская симуляция Angular 16.2.0 / TypeScript 5.1.6. Это изолированный Promise-fixture, а не выполненный Angular bootstrap. По умолчанию type Config={apiBaseUrl:string}; если задача явно расширяет тип, используется показанное расширение. Локальный host ждёт только возвращённый Promise initializer; если возвращён undefined, он не ждёт загрузку. Указанные завершённые состояния наблюдаются после await соответствующей задачи; pending означает, что соответствующий Promise ещё не завершён. Иных конкурентных изменений нет. В классе Loader заданы ready=false, snapshot:Config|null=null и поле fetch:()=>Promise<Config>. Напиши метод async load():Promise<void>: присвоить полученный config, затем ready=true. Пока fetch pending, ready=false; ошибку fetch передать вызывающему. Это первый запуск.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
async load(): Promise<void> { const c = await this.fetch(); this.snapshot = c; this.ready = true; }
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
async load(): Promise<void> { this.ready = true; const c = await this.fetch(); this.snapshot = c; }
~~~~

**Почему есть предпочтение:** Флаг выставлен до получения config.

Различающий дефект: `ready_before_fetch_completion`. Семья: `v7.ref.config.load-transition`.

### 2. CODE-NG-CONFIG-READY.REF.02 — base

**Задача ученику:**

~~~~text
Авторская симуляция Angular 16.2.0 / TypeScript 5.1.6. Это изолированный Promise-fixture, а не выполненный Angular bootstrap. По умолчанию type Config={apiBaseUrl:string}; если задача явно расширяет тип, используется показанное расширение. Локальный host ждёт только возвращённый Promise initializer; если возвращён undefined, он не ждёт загрузку. Указанные завершённые состояния наблюдаются после await соответствующей задачи; pending означает, что соответствующий Promise ещё не завершён. Иных конкурентных изменений нет. Даны type Init=()=>Promise<void>; interface Loader{load():Promise<void>}. Напиши function makeInit(s:Loader):Init. Host должен ожидать именно окончания s.load(), а не отдельный немедленно завершённый Promise.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
function makeInit(s: Loader): Init { return () => s.load(); }
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
function makeInit(s: Loader): Init { return () => { void s.load(); return Promise.resolve(); }; }
~~~~

**Почему есть предпочтение:** Оба ответа имеют правильный тип; rejected возвращает не Promise загрузки.

Различающий дефект: `initializer_returns_unrelated_promise`. Семья: `v7.ref.config.initializer-return`.

### 3. CODE-NG-CONFIG-READY.REF.03 — base

**Задача ученику:**

~~~~text
Авторская симуляция Angular 16.2.0 / TypeScript 5.1.6. Это изолированный Promise-fixture, а не выполненный Angular bootstrap. По умолчанию type Config={apiBaseUrl:string}; если задача явно расширяет тип, используется показанное расширение. Локальный host ждёт только возвращённый Promise initializer; если возвращён undefined, он не ждёт загрузку. Указанные завершённые состояния наблюдаются после await соответствующей задачи; pending означает, что соответствующий Promise ещё не завершён. Иных конкурентных изменений нет. В этом случае type Config={apiBaseUrl:string,editor:boolean}; interface S{whenReady():Promise<Config>;readonly snapshot:Config}. Getter snapshot выбрасывает not-ready, пока config не получен. Напиши async function canEdit(s:S):Promise<boolean>: дождаться whenReady и вернуть editor. При вызове whenReady ещё pending.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
async function canEdit(s: S): Promise<boolean> { const c = await s.whenReady(); return c.editor; }
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
async function canEdit(s: S): Promise<boolean> { return s.snapshot.editor; }
~~~~

**Почему есть предпочтение:** Rejected читает getter до загрузки.

Различающий дефект: `consumer_reads_snapshot_before_ready`. Семья: `v7.ref.config.snapshot-guard`.

### 4. CODE-NG-CONFIG-READY.REF.04 — base

**Задача ученику:**

~~~~text
Авторская симуляция Angular 16.2.0 / TypeScript 5.1.6. Это изолированный Promise-fixture, а не выполненный Angular bootstrap. По умолчанию type Config={apiBaseUrl:string}; если задача явно расширяет тип, используется показанное расширение. Локальный host ждёт только возвращённый Promise initializer; если возвращён undefined, он не ждёт загрузку. Указанные завершённые состояния наблюдаются после await соответствующей задачи; pending означает, что соответствующий Promise ещё не завершён. Иных конкурентных изменений нет. В классе Loader заданы ready=true, snapshot:Config|null={apiBaseUrl:"/old"} и fetch:()=>Promise<Config>. Напиши async reload():Promise<void>: сразу ready=false, затем дождаться нового config, присвоить snapshot и ready=true. При отказе ready остаётся false, прежний snapshot может остаться, ошибка передаётся. Контроль: fetch pending, затем rejected.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
async reload(): Promise<void> { this.ready = false; const c = await this.fetch(); this.snapshot = c; this.ready = true; }
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
async reload(): Promise<void> { const c = await this.fetch(); this.snapshot = c; this.ready = true; }
~~~~

**Почему есть предпочтение:** Нет сброса готовности в начале новой загрузки.

Различающий дефект: `reload_keeps_stale_ready`. Семья: `v7.ref.config.reload-reset`.

### 5. CODE-NG-CONFIG-READY.REF.05 — base

**Задача ученику:**

~~~~text
Авторская симуляция Angular 16.2.0 / TypeScript 5.1.6. Это изолированный Promise-fixture, а не выполненный Angular bootstrap. По умолчанию type Config={apiBaseUrl:string}; если задача явно расширяет тип, используется показанное расширение. Локальный host ждёт только возвращённый Promise initializer; если возвращён undefined, он не ждёт загрузку. Указанные завершённые состояния наблюдаются после await соответствующей задачи; pending означает, что соответствующий Promise ещё не завершён. Иных конкурентных изменений нет. В классе Loader заданы ready=false, snapshot:Config|null=null, fetch:()=>Promise<Config> и log:string[]=[]. Напиши async load():Promise<void>: при успехе присвоить snapshot и ready=true; при ошибке добавить "failed" в log и передать исходную ошибку вызывающему. Fallback не разрешён.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
async load(): Promise<void> { try { this.snapshot = await this.fetch(); this.ready = true; } catch (e) { this.log.push("failed"); throw e; } }
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
async load(): Promise<void> { try { this.snapshot = await this.fetch(); this.ready = true; } catch (e) { this.log.push("failed"); } }
~~~~

**Почему есть предпочтение:** Rejected сохраняет лог, но превращает отказ load в успешное завершение.

Различающий дефект: `error_swallowed`. Семья: `v7.ref.config.swallowed-failure`.

### 6. CODE-NG-CONFIG-READY.REF.06 — base

**Задача ученику:**

~~~~text
Авторская симуляция Angular 16.2.0 / TypeScript 5.1.6. Это изолированный Promise-fixture, а не выполненный Angular bootstrap. По умолчанию type Config={apiBaseUrl:string}; если задача явно расширяет тип, используется показанное расширение. Локальный host ждёт только возвращённый Promise initializer; если возвращён undefined, он не ждёт загрузку. Указанные завершённые состояния наблюдаются после await соответствующей задачи; pending означает, что соответствующий Promise ещё не завершён. Иных конкурентных изменений нет. В классе Loader заданы pending:Promise<Config>|undefined и fetch:()=>Promise<Config>. Напиши синхронный метод whenReady():Promise<Config>. Два вызова подряд до завершения загрузки должны разделять тот же pending Promise и вызвать fetch ровно один раз. Политика retry после завершения не является частью задачи.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
whenReady(): Promise<Config> { return this.pending ??= this.fetch(); }
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
whenReady(): Promise<Config> { return this.fetch(); }
~~~~

**Почему есть предпочтение:** Без сохранения pending каждый вызов fetch создаёт новую загрузку.

Различающий дефект: `single_flight_not_cached`. Семья: `v7.ref.config.single-flight`.

### 7. CODE-NG-CONFIG-READY.REF.07 — base

**Задача ученику:**

~~~~text
Авторская симуляция Angular 16.2.0 / TypeScript 5.1.6. Это изолированный Promise-fixture, а не выполненный Angular bootstrap. По умолчанию type Config={apiBaseUrl:string}; если задача явно расширяет тип, используется показанное расширение. Локальный host ждёт только возвращённый Promise initializer; если возвращён undefined, он не ждёт загрузку. Указанные завершённые состояния наблюдаются после await соответствующей задачи; pending означает, что соответствующий Promise ещё не завершён. Иных конкурентных изменений нет. В классе Loader заданы ready=false, snapshot:Config|null=null, fetch:()=>Promise<Config>, reloadFlags:(c:Config)=>Promise<void>. Напиши async load():Promise<void>: fetch, присвоить snapshot, дождаться reloadFlags с этим config, только затем ready=true. Ошибки передать. Контроль: fetch resolved, reloadFlags pending. Асинхронный reloadFlags — явно заданная вариация симуляции, а не факт об исходном ADR.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
async load(): Promise<void> { const c = await this.fetch(); this.snapshot = c; await this.reloadFlags(c); this.ready = true; }
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
async load(): Promise<void> { const c = await this.fetch(); this.snapshot = c; void this.reloadFlags(c); this.ready = true; }
~~~~

**Почему есть предпочтение:** Rejected не ждёт обязательную обработку флагов.

Различающий дефект: `required_dependency_not_awaited`. Семья: `v7.ref.config.dependent-flags`.

### 8. CODE-NG-CONFIG-READY.REF.08 — base

**Задача ученику:**

~~~~text
Авторская симуляция Angular 16.2.0 / TypeScript 5.1.6. Это изолированный Promise-fixture, а не выполненный Angular bootstrap. По умолчанию type Config={apiBaseUrl:string}; если задача явно расширяет тип, используется показанное расширение. Локальный host ждёт только возвращённый Promise initializer; если возвращён undefined, он не ждёт загрузку. Указанные завершённые состояния наблюдаются после await соответствующей задачи; pending означает, что соответствующий Promise ещё не завершён. Иных конкурентных изменений нет. Даны loadConfig:()=>Promise<void> и auth:()=>Promise<void>. Auth читает snapshot и сразу ошибается, если loadConfig не завершён. Напиши async function start():Promise<void>: загрузить config, затем выполнить auth. Успех — после обоих шагов. При старте loadConfig pending.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
async function start(): Promise<void> { await loadConfig(); await auth(); }
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
async function start(): Promise<void> { await Promise.all([loadConfig(), auth()]); }
~~~~

**Почему есть предпочтение:** Общего ожидания двух Promise недостаточно: второй шаг должен начаться после первого.

Различающий дефект: `dependent_jobs_started_in_parallel`. Семья: `v7.ref.config.config-before-auth`.

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
| `CODE-NG-CONFIG-READY/base` | 165 | `TR-EN-IF-01/ru-en` |

## Точные команды действующих потоков

Подготовка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Команды работают из любого каталога. RAG уже внутри пакета. Выполнить только назначенный поток; остальные строки — точные альтернативы назначения.

### CODE-NG-CONFIG-READY/base

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "CODE-NG-CONFIG-READY/base" -Run "S706" -Count 10 -Split train -Start 0
```

DEV/EXAM не запускать попутно с TRAIN. Когда назначено отдельное создание этих split в изолированном контексте, точные команды сохранены в `curriculum/pilot-queue.json` как `prepare_first_dev` / `prepare_first_exam`.

Первая TRAIN-папка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/CODE-NG-CONFIG-READY/base/S706`. Следующий поток: `TR-EN-IF-01/ru-en`. Доля TRAIN: 1000 пар; первая партия 10. Пилот TRAIN/DEV/EXAM: 100/20/20; набор `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/datasets/CODE-NG-CONFIG-READY-base-v1`; B1 `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/baselines/code.angular/base/CODE-NG-CONFIG-READY-v1/B1`. Это назначенные пути, не существующие веса.

<!-- GENERATED-QUEUE-END -->
