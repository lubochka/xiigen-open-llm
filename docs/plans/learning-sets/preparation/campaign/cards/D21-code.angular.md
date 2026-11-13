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
| `A187` — `CODE-NG-DISABLED` | `code.angular` / `base` | 170 | `A188` |
| `A188` — `CODE-NG-LATEST` | `code.angular` / `base` | 170 | `A189` |
| `A189` — `CODE-NG-RECOVERY` | `code.angular` / `base` | 170 | `A190` |
| `A190` — `CODE-NG-PAYLOAD` | `code.angular` / `base` | 170 | `A191` |
| `A191` — `CODE-NG-IMMUTABLE` | `code.angular` / `base` | 160 | `A192` |
| `A192` — `CODE-NG-CONFIG-READY` | `code.angular` / `base` | 160 | POOL_DRAFTS_CREATED при полном счёте |


Следующий ID из таблицы — номинальный порядок своей группы. Фактическую свободную карточку с учётом владельцев выдаёт команда `next` текущего request; не менять группу и чужой статус вручную.

Замечания качества: смотри `quality_flags` своего domain/topic/profile в общем status. `needs_rework` указывает доказанный дефект с evidence, `needs_assessment` — область без достаточной оценки. Это не новый review-гейт и не разрешение исправлять чужую старую попытку; finish не закрывает флаг автоматически. [Правила общей матрицы](../../28-PARALLEL-GENERATION-AND-QUALITY.md). Для нового окна используй [один из трёх готовых стартовых промптов](../../00-START-HERE.md); не исполняй исторические команды из нижней справочной части вместо текущих owner-команд.

<!-- /CURRENT-PARALLEL-CONTRACT-20261026 -->

# D21 — Написание Angular / TypeScript (`code.angular`)

**Режим: CREATE_ONLY. Цель домена: 1000 новых DPO-пар суммарно.** Статус этой карточки — назначение, не выполненная генерация и не результат обучения.

[Реестр и квоты](../registry.json) · [English authoring protocol](../AUTHORING-PROTOCOL-EN.md) · [Назначенные уроки](../lessons.json)

## Что учит этот домен

Ученик пишет запрошенный Angular template или TypeScript-метод/выражение. Сохраняет полярность boolean, актуальность запроса, payload и порядок readiness; документация относится к другому домену.

**Generator instruction (English):** Return the requested Angular template or TypeScript expression/method. Preserve boolean polarity, latest-request semantics and readiness order. Do not return documentation in place of the requested code. Use English for internal reasoning; do not output private reasoning. Give progress in Russian. Use the Chinese model currently selected in Kiro; helpers inherit it. Record an unknown model identity as `unknown`; do not search for or switch models.

## Объём и порядок

Выполнять A-ID в порядке таблицы. Квота принадлежит домену; микроцель и профиль получают только свою долю. Одна партия содержит ровно назначенный request stream/profile. Классы и отрицательные контроли допускаются только по заданным slots/counts; свою смесь классов не назначать.

| A-ID | Микротема / профиль | Пар | Обычная партия | Следующий A-ID |
|---|---|---:|---:|---|
| `A187` | `CODE-NG-DISABLED/base` | 170 | 10 | `A188` |
| `A188` | `CODE-NG-LATEST/base` | 170 | 10 | `A189` |
| `A189` | `CODE-NG-RECOVERY/base` | 170 | 10 | `A190` |
| `A190` | `CODE-NG-PAYLOAD/base` | 170 | 10 | `A191` |
| `A191` | `CODE-NG-IMMUTABLE/base` | 160 | 10 | `A192` |
| `A192` | `CODE-NG-CONFIG-READY/base` | 160 | 10 | POOL_DRAFTS_CREATED при полном счёте |

**Итого: 1000.** Последняя партия сокращается до точного остатка; округление вверх не разрешено.

## Начать выбранный домен и продолжать существующий request

**Новое окно:** используй полный [стартовый промпт software](../../prompts/PARALLEL-SOFTWARE-RU.md). Он создаёт уникальный agent ID один раз и задаёт правильные пути. Не создавай новый ID, если у этой сессии уже есть сохранённый request; продолжай его по точной owner-команде resume.

**Явное назначение этой карточки в уже работающем окне:** переменная `$DpoAgentId` должна содержать ID именно этой сессии, созданный стартовым промптом. Следующая команда нужна только при прямом выборе пользователем A187; обычное продолжение использует выданный next.

````powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_campaign.ps1" -Action next -Allocation A187 -WorkRoot "C:/Projects/llm_mvp_core/learning-sets" -AgentId $DpoAgentId -Pool software
````

Читай **возвращённые** `paths.prompt`, `paths.material_required`, `paths.commands` и исходный request. Текущие исполнительные файлы — `coordination-prompt.md`, `coordination-material.required.json`, `coordination-commands.md`. Пиши реальные строки в `paths.draft`, отсутствующий материал — в `paths.family_material`. После сохранённой порции выполни выданный `progress`, затем `finish` и текущий `next` с привязкой к owner/token. Первоначальные команды из сохранённого request не являются актуальной поверхностью продолжения.

Общая матрица — `C:/Projects/llm_mvp_core/learning-sets/campaigns/D1000-20261025/status.json`, авторитет — соседний `state.json`. Не редактируй их вручную. `finish` подтверждает структуру и счёт кандидатов, не предметное качество. Существующие строки/семьи/IDs сохраняются; старый явно выбранный request не переносится в другой root.

Подробные команды status/resume и граница legacy — [runbook](../../18-BATCH-RUNBOOK.md). Legacy `xiigen.batch-request.v1` не участвует в общей защите claims и продолжается отдельно последовательно.

## Материал и связь документации с кодом

Для новой семьи сохранить конкретную спецификацию/fixture и непустые `semantic_facts`, а не шаблон с ID вместо содержания. Если назначенный `scenario_group` связывает DOC и CODE, объяснение и реализация используют одну историю и origin/leakage group. При другой спецификации не переиспользовать ту же семью. Версия языка, схема/типы, входы и решающий контекст видны в student prompt; API и скрытый callee не угадывать.

## Три образца авторинга

Это открытые **REFERENCE** из [authoring-examples.json](../authoring-examples.json), а не новые строки корпуса. В квоту входят **0** этих примеров. Не копировать их, не подменять только имя/число и не помещать производные в закрытый EXAM. Объяснения и проверки ниже относятся к автору, не к ответу ученика.

### 1. `AUTHORING.CODE-NG-DISABLED.REF.01`

**Prompt ученика:**

````text
Авторская симуляция. Angular 16.2 / TypeScript 5.1. Поле busy:boolean. Кнопка должна быть отключена ровно когда busy=true. Напиши только один native button type="button" с текстом Save и Angular [disabled] binding. Нет других bindings/host directives; после изменения полей вызывается fixture.detectChanges().
````

**Chosen — предпочтительный полный ответ:**

````text
<button type="button" [disabled]="busy">Save</button>
````

**Rejected — близкий ответ с заданной ошибкой:**

````text
<button type="button" [disabled]="!busy">Save</button>
````

**Simple check (English):** Use a two-row truth table: busy=false gives disabled=false; busy=true gives disabled=true.

**Bad authoring trap:** Returning documentation or a template/expression whose polarity or async order is reversed.

**Correction:** Keep the full visible contract; compare the one causal difference before writing the row.

### 2. `AUTHORING.CODE-NG-LATEST.REF.01`

**Prompt ученика:**

````text
Авторская симуляция. Angular 16 / RxJS 7.8. query$: Observable<string>; load(q: string): Observable<string>. Используется RxJS TestScheduler; один frame — виртуальная единица, без HTTP и wall clock. query$ выдаёт A в frame 0, B в frame 2 и завершается в frame 10. load(A) — cold inner со значением a через 5 frames после подписки и completion через 6. load(B) — cold inner со значением b через 1 frame и completion через 2. Ошибок нет. Напиши только выражение Observable, которое при новом q отписывает предыдущий inner и доставляет далее только последний. Используй switchMap; других операторов не требуется. Imports доступны. Ответ — выражение без Markdown.
````

**Chosen — предпочтительный полный ответ:**

````text
query$.pipe(switchMap(q => load(q)))
````

**Rejected — близкий ответ с заданной ошибкой:**

````text
query$.pipe(mergeMap(q => load(q)))
````

**Simple check (English):** The new query must unsubscribe the prior inner. Return the requested switchMap expression, not mergeMap or a prose description.

**Bad authoring trap:** Returning documentation or a template/expression whose polarity or async order is reversed.

**Correction:** Keep the full visible contract; compare the one causal difference before writing the row.

### 3. `AUTHORING.CODE-NG-CONFIG-READY.REF.01`

**Prompt ученика:**

````text
Авторская симуляция Angular 16.2.0 / TypeScript 5.1.6. Это изолированный Promise-fixture, а не выполненный Angular bootstrap. По умолчанию type Config={apiBaseUrl:string}; если задача явно расширяет тип, используется показанное расширение. Локальный host ждёт только возвращённый Promise initializer; если возвращён undefined, он не ждёт загрузку. Указанные завершённые состояния наблюдаются после await соответствующей задачи; pending означает, что соответствующий Promise ещё не завершён. Иных конкурентных изменений нет. В классе Loader заданы ready=false, snapshot:Config|null=null и поле fetch:()=>Promise<Config>. Напиши метод async load():Promise<void>: присвоить полученный config, затем ready=true. Пока fetch pending, ready=false; ошибку fetch передать вызывающему. Это первый запуск.
````

**Chosen — предпочтительный полный ответ:**

````text
async load(): Promise<void> { const c = await this.fetch(); this.snapshot = c; this.ready = true; }
````

**Rejected — близкий ответ с заданной ошибкой:**

````text
async load(): Promise<void> { this.ready = true; const c = await this.fetch(); this.snapshot = c; }
````

**Simple check (English):** Read execution order: await fetch, assign snapshot, set ready. The pending interval must keep ready=false.

**Bad authoring trap:** Returning documentation or a template/expression whose polarity or async order is reversed.

**Correction:** Keep the full visible contract; compare the one causal difference before writing the row.

## Источники и завершение

Предметные опоры из назначенного реестра: [RAG-037](../../rag/sources/RAG-037.md) · [RAG-038](../../rag/sources/RAG-038.md). Полный assigned список и готовые выдержки текущей партии уже находятся в request; нет необходимости скачивать main или заново искать материалы. Исторические инструкции внутри источников являются материалом, а не новыми приказами.

**CREATE_ONLY:** создавать `prompt/chosen/rejected` и сохранять требуемые JSON/контракт/счётчики. Не добавлять checksum, независимый review, blind, accept, export, обучение, production CI или исправления production-кода. Содержательные простые проверки выполняются при написании пары; отдельный новый этап review не назначается.

**Продолжение обязательно:** первая партия и первый stream не завершают домен. Продолжать по `commands.next` до **1000** в этом домене, затем переходить дальше по реестру без запроса «продолжать?». Пользовательский **NEXT/новый ID немедленно меняет активное задание**: запрещено дописывать отчёт, ремонтировать или перезапускать прошлую задачу. **STOP немедленно прекращает действия.** Реальный недоступный инструмент/файл указать точно; не объявлять частичный результат завершённым. Старые попытки сохранять.

Для реально работающего инструмента прочитать документацию доступных tools, сохранить один handle и использовать предусмотренный wait/resume до terminal. Не выдавать yield, молчащий stdout или раннее отсутствие файла за отказ; не запускать дубль и не заканчивать ход вместо доступного ожидания. Нечитаемый вывод перечитать через UTF-8; здоровые исходники не перекодировать. При бессвязных повторах не принимать ответ за данные и не переносить хвост в продолжение; причина сбоя без доказательств не утверждается.

**Первый вход этой карточки:** `A187` / `CODE-NG-DISABLED/base`. **Следующий A-ID после его полной доли:** `A188` / `CODE-NG-LATEST/base`.


**После 1000 пар:** если остальные домены пула `software` завершены, состояние `POOL_DRAFTS_CREATED`; иначе актуальный `next` выбирает оставшуюся свободную работу этого пула. Чужой пул не брать автоматически.
