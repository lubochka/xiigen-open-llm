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
| `A162` — `DOC-CS-PIPELINE` | `docs.csharp` / `base` | 160 | `A163` |


Следующий ID из таблицы — номинальный порядок своей группы. Фактическую свободную карточку с учётом владельцев выдаёт команда `next` текущего request; не менять группу и чужой статус вручную.

Замечания качества: смотри `quality_flags` своего domain/topic/profile в общем status. `needs_rework` указывает доказанный дефект с evidence, `needs_assessment` — область без достаточной оценки. Это не новый review-гейт и не разрешение исправлять чужую старую попытку; finish не закрывает флаг автоматически. [Правила общей матрицы](../../28-PARALLEL-GENERATION-AND-QUALITY.md). Для нового окна используй [один из трёх готовых стартовых промптов](../../00-START-HERE.md); не исполняй исторические команды из нижней справочной части вместо текущих owner-команд.

<!-- /CURRENT-PARALLEL-CONTRACT-20261026 -->

# DOC-CS-PIPELINE — Описать next, короткое завершение и порядок эффектов конвейера

<!-- DOMAIN-CAMPAIGN-AUTHORITY -->
> **Предметная справка к действующей кампании.** Квоты и продолжение задаёт [campaign/registry.json](../../campaign/registry.json), не старые команды ниже. 1000 пар относятся к домену в сумме. Прочитанный генератором рабочий prompt содержит только назначенную микротему.
>
> Данные: `C:/Projects/llm_mvp_core/learning-sets`. Инструкции: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. CREATE_ONLY: без review, checksum, production CI и обучения. Модель — текущая Kiro без override. Reason internally in English; do not output private reasoning. Отчёт по-русски.
>
> После партии выполнять готовый next до доменной квоты; не останавливаться с вопросом «продолжать?». При прямом NEXT/новом ID немедленно сменить задачу: запрещено дописывать/чинить предыдущую. STOP прекращает действия. Один живой handle ждать документированным wait/resume; не объявлять молчание падением. Старые попытки сохранять; выбранный request возобновлять, не пересоздавать.

Точные доли этой предметной карточки в текущей кампании:
| A-ID | Домен | Профиль | Доля из доменных1000 | Следующий A-ID после доли |
|---|---|---|---:|---|
| A162 | docs.csharp | base | 160 | A163 |


Ниже — сохранённая предметная методика и исторические инструкции. [Полный прежний вариант](../../history/before-domain-campaign-2026-10-25/curriculum/cards/DOC-CS-PIPELINE.md). Старые числовые планы и команды в нём не запускаются вместо актуального request.
<!-- /DOMAIN-CAMPAIGN-AUTHORITY -->

## Узкая задача

Восстановить наблюдаемую последовательность конкретного конвейера по видимым await/return/throw; не приписывать ему скрытый ASP.NET lifecycle.

Родитель: `DOC-CS-03`. Предметные домены: `docs.csharp`. Профили: `base`. Один request содержит только назначенный профиль и эту микроцель.

Статус: авторский материал подготовки. Публичные пары ниже — reference; их нельзя копировать в новый корпус или выдавать за скрытый экзамен. Китайская генерация, оценка Astra и Student для нового материала не выполнялись; ожидаемые результаты не являются измерениями.

## Проверенные источники и применимость

- [RAG-027](../../rag/sources/RAG-027.md): закреплённая методика или исходная тема; конкретные новые случаи являются авторскими симуляциями.
- [RAG-028](../../rag/sources/RAG-028.md): закреплённая методика или исходная тема; конкретные новые случаи являются авторскими симуляциями.
- [RAG-029](../../rag/sources/RAG-029.md): закреплённая методика или исходная тема; конкретные новые случаи являются авторскими симуляциями.

[Разобранные good/bad примеры и логика](../../prompts/SOFTWARE-PIPELINE-WINDOW-READY-RU.md). Основное задание и все reference выбранного профиля автоматически входят в batch prompt.

## Подробный промпт исполнителю

Создавай маленькие C#-симуляции middleware-подобного конвейера. Используй полностью показанные локальные функции с Func<Task> next, явные await, один список trace и ровно одну точку запуска. Назови это изолированной симуляцией: без WebApplication, реального HTTP-запроса или Startup нельзя заявлять запуск ASP.NET Core host. Проверяй одну границу: код после await вызывается только после успешного завершения next; ветка без next не запускает внутренний обработчик; возврат из внутреннего обработчика не равен возврату из внешнего; finally предшествует внешнему catch. Для pending дай управляемый TaskCompletionSource и точные точки снимка. Не спрашивай о скрытой БД, auth, сети, потоках или зависимостях.

ПРОСТАЯ САМОПРОВЕРКА:
1. Нарисуй стек вызовов по конкретным вызовам next; к отсутствующему next не добавляй внутренние шаги.
2. Добавляй trace в точном порядке. Для await останови внешнюю функцию до завершения внутренней.
3. Return завершает только текущую функцию; throw пропускает обычные операторы, пока не встретится соответствующий catch.
4. Для каждой finally сначала добавь её эффекты, затем переходи к внешнему catch.
5. Сравни списки chosen/rejected: одна пропущенная, добавленная либо переставленная причинная группа; не скрытый второй дефект.

Работай по одному готовому request.json: один topic_id, objective_id, domain_id, profile_id и split. В каждом prompt повтори среду, полный видимый код/данные, точку наблюдения и форму ответа: metadata не попадает в строгую проекцию prompt/chosen/rejected. Создай полный chosen и близкий rejected с одним основным содержательным дефектом, одинакового формата. Не заменяй rejected синтаксической ошибкой, шуткой, заведомо коротким ответом или другой правильной реализацией. Объяснение проверки, source_refs и error_tag записывай вне ответов.
Сначала выпиши маленькую таблицу состояния либо результата по условию, затем сравни с ней обе альтернативы. Это самопроверка автора кандидатов, не независимая оценка качества и не измерение обучения. Если по указанному входу ответы одинаковы, измени различающий вход в пределах задания; не придумывай победу. Смена имени или чисел сама по себе не создаёт независимую семью. Производные, обратные и DOC/CODE-варианты конкретного случая наследуют общий origin/leakage group и split. Публичные reference не являются закрытым EXAM.
Источники — только назначенные файлы xiigen-planning main/llm_plan3. Они дают методику и исходную тематику. Новые фрагменты являются авторскими симуляциями; runtime/oracle сейчас NOT_RUN. Не объявляй новую симуляцию извлечённым production-кодом, измеренным результатом или китайской генерацией.
Роль человека при выборе материала выполняет CODEX с Astra 6 ultracode; оценку качества DPO также выполняет CODEX с Astra 6 ultracode. Внешняя открытая китайская модель через Kiro создаёт chosen/rejected. Сейчас CREATE_ONLY: назначение уже дано карточкой/request. Не запускай новые selection/review/oracle/accept/export/Student/production CI и не задерживай следующую партию ради них. Модель — фактически текущая выбранная в Kiro, без override; неизвестное имя/session запиши unknown. Checksum, подписи, profile receipts и независимый review не нужны для создания.
Корень наборов C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets; подготовка находится в preparation внутри него. Возьми абсолютные входные и выходные пути из request.paths, не ищи runner в llm_mvp_core. После записи реальных JSONL-строк выполни одну готовую команду finish, получи структурный DRAFT_CREATED и перейди к next_stream_id полной execution-queue.json. PREPARED без draft не считается созданием. Старые данные/попытки не удалять; существующий run продолжать через resume.
При прямом NEXT/новом ID немедленно перейти к нему: запрещено продолжать предыдущую задачу, её отчёт или исправления. STOP прекращает действия сразу. Живой процесс ждать штатным wait/resume по единственному handle до terminal в том же ходе; не запускать дубликаты, не делать частый polling и не завершать ход словом «жду». Отчёт по-русски: тема, созданные строки/семьи, точный файл, фактическая модель, структурный статус, следующий ID. Нельзя приписывать вызов Astra текущей модели.

## Форма ответа ученика

Русский ответ: точный trace или две явно названные точки состояния и короткое объяснение видимого control-flow; не переписывать код.

## Что варьировать

```json
[
  {
    "axis": "выход внутреннего звена",
    "values": [
      "успех",
      "нет next",
      "условный return",
      "исключение",
      "pending"
    ]
  },
  {
    "axis": "наблюдение",
    "values": [
      "trace",
      "снимок до completion",
      "после завершения",
      "catch/finally"
    ]
  }
]
```

## Разобранные пары

### 1. DOC-CS-PIPELINE.REF.01 — base

**Задача ученику:**

~~~~text
Авторская изолированная симуляция C# 10 / .NET 6; using System; using System.Collections.Generic; using System.Linq; using System.Threading.Tasks; доступны. Это не запуск ASP.NET Core host. Фрагмент находится в async-методе. List.Add не выбрасывает исключений. var t=new List<string>(); async Task A(Func<Task> next){t.Add("A+");await next();t.Add("A-");} async Task B(Func<Task> next){t.Add("B+");await next();t.Add("B-");} Task C(){t.Add("C");return Task.CompletedTask;} await A(()=>B(C)); Назови t в порядке записей.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
["A+","B+","C","B-","A-"]. Внешние продолжения выполняются после успешного завершения внутренних вызовов.
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
["A+","B+","C","A-","B-"]. После C сначала завершается внешнее A, затем внутреннее B.
~~~~

**Почему есть предпочтение:** Переставлены два продолжения после await.

Различающий дефект: `continuation_order_reversed`. Семья: `v7.ref.pipeline.nested-success`.

### 2. DOC-CS-PIPELINE.REF.02 — base

**Задача ученику:**

~~~~text
Авторская изолированная симуляция C# 10 / .NET 6; using System; using System.Collections.Generic; using System.Linq; using System.Threading.Tasks; доступны. Это не запуск ASP.NET Core host. Фрагмент находится в async-методе. List.Add не выбрасывает исключений. var t=new List<string>(); async Task A(Func<Task> next){t.Add("A+");await next();t.Add("A-");} Task B(Func<Task> next){t.Add("Bstop");return Task.CompletedTask;} Task C(){t.Add("C");return Task.CompletedTask;} await A(()=>B(C)); Назови t.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
["A+","Bstop","A-"]. B не вызывает next, поэтому C не выполняется; B успешно завершается, и A продолжает работу.
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
["A+","Bstop","C","A-"]. После Bstop конвейер автоматически запускает C, затем продолжает A.
~~~~

**Почему есть предпочтение:** Видимый B вообще не вызывает C.

Различающий дефект: `implicit_next_invented`. Семья: `v7.ref.pipeline.short-circuit`.

### 3. DOC-CS-PIPELINE.REF.03 — base

**Задача ученику:**

~~~~text
Авторская изолированная симуляция C# 10 / .NET 6; using System; using System.Collections.Generic; using System.Linq; using System.Threading.Tasks; доступны. Это не запуск ASP.NET Core host. Фрагмент находится в async-методе. List.Add не выбрасывает исключений. var t=new List<string>(); bool blocked=true; async Task A(Func<Task> next){t.Add("A+");await next();t.Add("A-");} async Task B(Func<Task> next){t.Add("B+");if(blocked){t.Add("blocked");return;}await next();t.Add("B-");} Task C(){t.Add("C");return Task.CompletedTask;} await A(()=>B(C)); Назови t.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
["A+","B+","blocked","A-"]. Return завершает B; её Task успешен, поэтому A выполняет своё продолжение.
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
["A+","B+","blocked"]. Return из B завершает также A, поэтому A- отсутствует.
~~~~

**Почему есть предпочтение:** Return внутренней функции ошибочно перенесён на вызывающую.

Различающий дефект: `inner_return_exits_caller`. Семья: `v7.ref.pipeline.conditional-return`.

### 4. DOC-CS-PIPELINE.REF.04 — base

**Задача ученику:**

~~~~text
Авторская изолированная симуляция C# 10 / .NET 6; using System; using System.Collections.Generic; using System.Linq; using System.Threading.Tasks; доступны. Это не запуск ASP.NET Core host. Фрагмент находится в async-методе. List.Add не выбрасывает исключений. var t=new List<string>(); async Task A(Func<Task> next){t.Add("A+");try{await next();}catch(InvalidOperationException){t.Add("caught");}t.Add("A-");} Task B(){t.Add("B!");throw new InvalidOperationException();} await A(B); Назови t.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
["A+","B!","caught","A-"]. A перехватывает указанную ошибку и продолжает после catch.
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
["A+","B!","caught"]. Перехваченная ошибка всё равно не допускает выполнения A-.
~~~~

**Почему есть предпочтение:** Пропущено обычное продолжение после успешно завершённого catch.

Различающий дефект: `caught_exception_still_exits`. Семья: `v7.ref.pipeline.caught-continues`.

### 5. DOC-CS-PIPELINE.REF.05 — base

**Задача ученику:**

~~~~text
Авторская изолированная симуляция C# 10 / .NET 6; using System; using System.Collections.Generic; using System.Linq; using System.Threading.Tasks; доступны. Это не запуск ASP.NET Core host. Фрагмент находится в async-методе. List.Add не выбрасывает исключений. var t=new List<string>(); async Task A(Func<Task> next){t.Add("A+");try{await next();}finally{t.Add("cleanup");}} Task B(){t.Add("B!");throw new InvalidOperationException();} try{await A(B);}catch(InvalidOperationException){t.Add("outer");} Назови t.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
["A+","B!","cleanup","outer"]. Finally внутри A выполняется до передачи ошибки внешнему catch.
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
["A+","B!","outer","cleanup"]. Сначала внешний catch получает ошибку, затем завершается finally A.
~~~~

**Почему есть предпочтение:** Одна ошибка — порядок очистки и внешнего обработчика.

Различающий дефект: `outer_catch_before_finally`. Семья: `v7.ref.pipeline.finally-unwind`.

### 6. DOC-CS-PIPELINE.REF.06 — base

**Задача ученику:**

~~~~text
Авторская изолированная симуляция C# 10 / .NET 6; using System; using System.Collections.Generic; using System.Linq; using System.Threading.Tasks; доступны. Это не запуск ASP.NET Core host. Фрагмент находится в async-методе. List.Add не выбрасывает исключений. var t=new List<string>(); async Task A(Func<Task> next){t.Add("A+");await next();t.Add("A-");} async Task B(Func<Task> next){t.Add("B+");await next();t.Add("B-");} Task C(){t.Add("C!");throw new InvalidOperationException();} try{await A(()=>B(C));}catch(InvalidOperationException){t.Add("caught");} Назови t.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
["A+","B+","C!","caught"]. Ошибка C прерывает оба обычных продолжения после await.
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
["A+","B+","C!","B-","A-","caught"]. Перед передачей ошибки завершаются обычные продолжения обоих обработчиков.
~~~~

**Почему есть предпочтение:** Обычные Add после await ошибочно выполнены как finally.

Различающий дефект: `ordinary_continuations_treated_as_finally`. Семья: `v7.ref.pipeline.unhandled-fault`.

### 7. DOC-CS-PIPELINE.REF.07 — base

**Задача ученику:**

~~~~text
Авторская изолированная симуляция C# 10 / .NET 6; using System; using System.Collections.Generic; using System.Linq; using System.Threading.Tasks; доступны. Это не запуск ASP.NET Core host. Фрагмент находится в async-методе. List.Add не выбрасывает исключений. var t=new List<string>(); bool enabled=false; async Task A(Func<Task> next){if(!enabled){await next();return;}t.Add("A+");await next();t.Add("A-");} Task C(){t.Add("C");return Task.CompletedTask;} await A(C); Назови t.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
["C"]. Выключена дополнительная логика A, но в выключенной ветке явно вызывается next.
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
[]. При enabled=false весь конвейер прекращается и C не запускается.
~~~~

**Почему есть предпочтение:** Имя enabled заменило чтение фактической ветки.

Различающий дефект: `disabled_wrapper_mistaken_for_stop`. Семья: `v7.ref.pipeline.disabled-pass-through`.

### 8. DOC-CS-PIPELINE.REF.08 — base

**Задача ученику:**

~~~~text
Авторская изолированная симуляция C# 10 / .NET 6; using System; using System.Collections.Generic; using System.Linq; using System.Threading.Tasks; доступны. Это не запуск ASP.NET Core host. Фрагмент находится в async-методе. List.Add не выбрасывает исключений. var t=new List<string>(); var gate=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously); async Task B(){t.Add("B+");await gate.Task;t.Add("B-");} async Task A(){t.Add("A+");await B();t.Add("A-");} var task=A(); var before=t.ToArray(); gate.SetResult(true); await task; var after=t.ToArray(); Назови before и after.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
before=["A+","B+"]; after=["A+","B+","B-","A-"]. Before снят, пока gate не завершён; after — после await task.
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
before=["A+","B+","B-","A-"]; after=["A+","B+","B-","A-"]. Сам вызов A полностью исполняет конвейер до снимка before.
~~~~

**Почему есть предпочтение:** Вызов async-функции ошибочно принят за ожидание pending-задачи.

Различающий дефект: `invocation_mistaken_for_completion`. Семья: `v7.ref.pipeline.pending-snapshot`.

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
| `DOC-CS-PIPELINE/base` | 160 | `CODE-CS-ROUND-STAGE/base` |

## Точные команды действующих потоков

Подготовка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Команды работают из любого каталога. RAG уже внутри пакета. Выполнить только назначенный поток; остальные строки — точные альтернативы назначения.

### DOC-CS-PIPELINE/base

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "DOC-CS-PIPELINE/base" -Run "S701" -Count 10 -Split train -Start 0
```

DEV/EXAM не запускать попутно с TRAIN. Когда назначено отдельное создание этих split в изолированном контексте, точные команды сохранены в `curriculum/pilot-queue.json` как `prepare_first_dev` / `prepare_first_exam`.

Первая TRAIN-папка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/DOC-CS-PIPELINE/base/S701`. Следующий поток: `CODE-CS-ROUND-STAGE/base`. Доля TRAIN: 1000 пар; первая партия 10. Пилот TRAIN/DEV/EXAM: 100/20/20; набор `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/datasets/DOC-CS-PIPELINE-base-v1`; B1 `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/baselines/docs.csharp/base/DOC-CS-PIPELINE-v1/B1`. Это назначенные пути, не существующие веса.

<!-- GENERATED-QUEUE-END -->
