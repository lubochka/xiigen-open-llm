# CODE-CS-ROUND-STAGE — Реализовать точный момент округления decimal

<!-- CREATE-ONLY-START -->
> **Действующий режим: CREATE_ONLY — только создание.** Прочитай назначение, создай реальные пары в `draft.jsonl`, выполни один структурный `finish`, перейди к следующему stream ID из общей очереди. Не запускай blind/review/oracle/accept/export/обучение и не жди проверяющего. `DRAFT_CREATED` означает запись кандидатов, а не проверенное качество.
>
> Корень наборов: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets`. Подготовка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Точные команды ниже работают из любого текущего каталога; готовый request содержит абсолютные пути. Не искать инструменты в `llm_mvp_core`, не создавать пустой RAG и не восстанавливать содержимое по checksum.
>
> Модель — текущая выбранная в Kiro, без override; не более одного помощника одновременно. При NEXT/новом ID немедленно перейти к нему, не дописывать прошлую задачу; STOP прекращает действия. Живой процесс ждать по его handle штатным wait/resume; timeout транспорта не доказывает отказ. [Точный контракт](../../20-CREATE-ONLY-AND-PATHS.md). Старые правила приёмки ниже описывают только будущую стадию.
> Роль человека при выборе материала выполняет CODEX с Astra 6 ultracode; оценку качества DPO также выполняет CODEX с Astra 6 ultracode. Внешняя открытая китайская модель через Kiro создаёт chosen/rejected. В CREATE_ONLY назначение уже задано карточкой/request: новый вызов выбора и оценка сейчас не запускаются и не задерживают создание следующей партии. Это назначение ролей, не подтверждение выполненного вызова.
<!-- CREATE-ONLY-END -->

## Узкая задача

Перевести контракт округления в исполнимый decimal-код, различая сумму, отдельные позиции, масштаб и midpoint.

Родитель: `CODE-CS-04`. Предметные домены: `code.csharp`. Профили: `base`. Один request содержит только назначенный профиль и эту микроцель.

Статус: авторский материал подготовки. Публичные пары ниже — reference; их нельзя копировать в новый корпус или выдавать за скрытый экзамен. Китайская генерация, оценка Astra и Student для нового материала не выполнялись; ожидаемые результаты не являются измерениями.

## Проверенные источники и применимость

- [RAG-027](../../rag/sources/RAG-027.md): закреплённая методика или исходная тема; конкретные новые случаи являются авторскими симуляциями.
- [RAG-028](../../rag/sources/RAG-028.md): закреплённая методика или исходная тема; конкретные новые случаи являются авторскими симуляциями.

[Разобранные good/bad примеры и логика](../../prompts/SOFTWARE-PIPELINE-WINDOW-READY-RU.md). Основное задание и все reference выбранного профиля автоматически входят в batch prompt.

## Подробный промпт исполнителю

Создавай один короткий decimal-метод. В prompt укажи точную сигнатуру, nullable/диапазон, допустимость переполнения, число десятичных знаков, правило midpoint и момент округления. Назначь вход, на котором sum(round(x)) отличается от round(sum(x)), либо округление до умножения — от округления произведения. Используй decimal-литералы с m. Два ответа должны компилироваться с показанными импортами и отличаться одним правилом. Когда контракт требует округлять каждую позицию, это chosen; когда итог — chosen другой. Нельзя учить всегда округлять только в конце. Не меняй одновременно тип decimal, масштаб, знак и стадию.

ПРОСТАЯ САМОПРОВЕРКА:
1. Выпиши значения до округления без double: например 0.335+0.335=0.670.
2. Отдельно вычисли round каждого элемента и round суммы. При одинаковом ответе выбери различающий вход.
3. Для midpoint укажи обе соседние десятичные величины и правило выбора; для отрицательного числа «от нуля» увеличивает модуль.
4. Проверь масштаб и порядок операций по контракту, а не по названию функции.
5. У chosen/rejected одна и та же сигнатура; нужные using доступны в prompt.

Работай по одному готовому request.json: один topic_id, objective_id, domain_id, profile_id и split. В каждом prompt повтори среду, полный видимый код/данные, точку наблюдения и форму ответа: metadata не попадает в строгую проекцию prompt/chosen/rejected. Создай полный chosen и близкий rejected с одним основным содержательным дефектом, одинакового формата. Не заменяй rejected синтаксической ошибкой, шуткой, заведомо коротким ответом или другой правильной реализацией. Объяснение проверки, source_refs и error_tag записывай вне ответов.
Сначала выпиши маленькую таблицу состояния либо результата по условию, затем сравни с ней обе альтернативы. Это самопроверка автора кандидатов, не независимая оценка качества и не измерение обучения. Если по указанному входу ответы одинаковы, измени различающий вход в пределах задания; не придумывай победу. Смена имени или чисел сама по себе не создаёт независимую семью. Производные, обратные и DOC/CODE-варианты конкретного случая наследуют общий origin/leakage group и split. Публичные reference не являются закрытым EXAM.
Источники — только назначенные файлы xiigen-planning main/llm_plan3. Они дают методику и исходную тематику. Новые фрагменты являются авторскими симуляциями; runtime/oracle сейчас NOT_RUN. Не объявляй новую симуляцию извлечённым production-кодом, измеренным результатом или китайской генерацией.
Роль человека при выборе материала выполняет CODEX с Astra 6 ultracode; оценку качества DPO также выполняет CODEX с Astra 6 ultracode. Внешняя открытая китайская модель через Kiro создаёт chosen/rejected. Сейчас CREATE_ONLY: назначение уже дано карточкой/request. Не запускай новые selection/review/oracle/accept/export/Student/production CI и не задерживай следующую партию ради них. Модель — фактически текущая выбранная в Kiro, без override; неизвестное имя/session запиши unknown. Checksum, подписи, profile receipts и независимый review не нужны для создания.
Корень наборов C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets; подготовка находится в preparation внутри него. Возьми абсолютные входные и выходные пути из request.paths, не ищи runner в llm_mvp_core. После записи реальных JSONL-строк выполни одну готовую команду finish, получи структурный DRAFT_CREATED и перейди к next_stream_id полной execution-queue.json. PREPARED без draft не считается созданием. Старые данные/попытки не удалять; существующий run продолжать через resume.
При прямом NEXT/новом ID немедленно перейти к нему: запрещено продолжать предыдущую задачу, её отчёт или исправления. STOP прекращает действия сразу. Живой процесс ждать штатным wait/resume по единственному handle до terminal в том же ходе; не запускать дубликаты, не делать частый polling и не завершать ход словом «жду». Отчёт по-русски: тема, созданные строки/семьи, точный файл, фактическая модель, структурный статус, следующий ID. Нельзя приписывать вызов Astra текущей модели.

## Форма ответа ученика

Только полный требуемый C#-метод без Markdown и без пояснения; пояснение и таблица самопроверки в metadata/rationale.

## Что варьировать

```json
[
  {
    "axis": "стадия",
    "values": [
      "каждая позиция",
      "после суммы",
      "после умножения"
    ]
  },
  {
    "axis": "правило",
    "values": [
      "AwayFromZero",
      "ToEven",
      "усечение к нулю"
    ]
  },
  {
    "axis": "вход",
    "values": [
      "положительный midpoint",
      "отрицательный midpoint",
      "различающий набор",
      "нулевая/пустая сумма"
    ]
  }
]
```

## Разобранные пары

### 1. CODE-CS-ROUND-STAGE.REF.01 — base

**Задача ученику:**

~~~~text
Авторская симуляция C# 10 / .NET 6; using System; using System.Linq; доступны. Все вычисления decimal, входы не null, переполнение исключено. Напиши static decimal Total(decimal[] xs): сначала сложить исходные xs, затем один раз округлить сумму до 2 знаков, midpoint AwayFromZero. Контроль xs=[0.335m,0.335m].
~~~~

**Chosen — полный правильный ответ:**

~~~~text
static decimal Total(decimal[] xs) => Math.Round(xs.Sum(), 2, MidpointRounding.AwayFromZero);
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
static decimal Total(decimal[] xs) => xs.Sum(x => Math.Round(x, 2, MidpointRounding.AwayFromZero));
~~~~

**Почему есть предпочтение:** По контракту округляется сумма 0.670, не две позиции по 0.335.

Различающий дефект: `round_items_instead_of_total`. Семья: `sim2.code-cs-04.f0003`.

### 2. CODE-CS-ROUND-STAGE.REF.02 — base

**Задача ученику:**

~~~~text
Авторская симуляция C# 10 / .NET 6; using System; using System.Linq; доступны. Все вычисления decimal, входы не null, переполнение исключено. Напиши static decimal Invoice(decimal[] xs): округлить каждую позицию до 2 знаков AwayFromZero, затем сложить позиции. Контроль xs=[0.335m,0.335m].
~~~~

**Chosen — полный правильный ответ:**

~~~~text
static decimal Invoice(decimal[] xs) => xs.Sum(x => Math.Round(x, 2, MidpointRounding.AwayFromZero));
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
static decimal Invoice(decimal[] xs) => Math.Round(xs.Sum(), 2, MidpointRounding.AwayFromZero);
~~~~

**Почему есть предпочтение:** Здесь контракт намеренно противоположный: сначала округляется каждая позиция.

Различающий дефект: `round_total_instead_of_items`. Семья: `sim2.code-cs-04.f0003`.

### 3. CODE-CS-ROUND-STAGE.REF.03 — base

**Задача ученику:**

~~~~text
Авторская симуляция C# 10 / .NET 6; using System; using System.Linq; доступны. Все вычисления decimal, входы не null, переполнение исключено. Напиши static decimal R(decimal x): 2 знака, midpoint от нуля. Контроль x=-1.225m.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
static decimal R(decimal x) => Math.Round(x, 2, MidpointRounding.AwayFromZero);
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
static decimal R(decimal x) => Math.Round(x, 2, MidpointRounding.ToEven);
~~~~

**Почему есть предпочтение:** В rejected отрицательный midpoint округлён к чётному.

Различающий дефект: `negative_midpoint_wrong_mode`. Семья: `sim2.code-cs-04.f0001`.

### 4. CODE-CS-ROUND-STAGE.REF.04 — base

**Задача ученику:**

~~~~text
Авторская симуляция C# 10 / .NET 6; using System; using System.Linq; доступны. Все вычисления decimal, входы не null, переполнение исключено. Напиши static decimal R(decimal x): 2 знака, midpoint к чётной последней сохранённой цифре. Контроль x=1.225m.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
static decimal R(decimal x) => Math.Round(x, 2, MidpointRounding.ToEven);
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
static decimal R(decimal x) => Math.Round(x, 2, MidpointRounding.AwayFromZero);
~~~~

**Почему есть предпочтение:** Контракт к чётному делает 1.22 правильным, а не 1.23.

Различающий дефект: `even_midpoint_wrong_mode`. Семья: `sim2.code-cs-04.f0002`.

### 5. CODE-CS-ROUND-STAGE.REF.05 — base

**Задача ученику:**

~~~~text
Авторская симуляция C# 10 / .NET 6; using System; using System.Linq; доступны. Все вычисления decimal, входы не null, переполнение исключено. Напиши static decimal Charge(decimal price,int count): сначала price*count, затем 2 знака AwayFromZero. Контроль price=1.005m,count=2.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
static decimal Charge(decimal price, int count) => Math.Round(price * count, 2, MidpointRounding.AwayFromZero);
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
static decimal Charge(decimal price, int count) => Math.Round(price, 2, MidpointRounding.AwayFromZero) * count;
~~~~

**Почему есть предпочтение:** Округление преждевременно применено к цене.

Различающий дефект: `round_before_multiply`. Семья: `sim2.code-cs-04.f0005`.

### 6. CODE-CS-ROUND-STAGE.REF.06 — base

**Задача ученику:**

~~~~text
Авторская симуляция C# 10 / .NET 6; using System; using System.Linq; доступны. Все вычисления decimal, входы не null, переполнение исключено. Напиши static decimal Cut(decimal x): оставить 2 знака усечением к нулю. |x|<1000000m. Контроль x=-1.239m.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
static decimal Cut(decimal x) => Math.Truncate(x * 100m) / 100m;
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
static decimal Cut(decimal x) => Math.Floor(x * 100m) / 100m;
~~~~

**Почему есть предпочтение:** Floor для отрицательного значения уходит дальше от 0.

Различающий дефект: `floor_instead_of_toward_zero`. Семья: `sim2.code-cs-04.f0004`.

### 7. CODE-CS-ROUND-STAGE.REF.07 — base

**Задача ученику:**

~~~~text
Авторская симуляция C# 10 / .NET 6; using System; using System.Linq; доступны. Все вычисления decimal, входы не null, переполнение исключено. Напиши static decimal Mills(decimal x): округлить до 3 знаков AwayFromZero. Контроль x=0.0015m.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
static decimal Mills(decimal x) => Math.Round(x, 3, MidpointRounding.AwayFromZero);
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
static decimal Mills(decimal x) => Math.Round(x, 2, MidpointRounding.AwayFromZero);
~~~~

**Почему есть предпочтение:** Изменён единственный требуемый параметр разрядности.

Различающий дефект: `scale_two_instead_of_three`. Семья: `sim2.code-cs-04.f0006`.

### 8. CODE-CS-ROUND-STAGE.REF.08 — base

**Задача ученику:**

~~~~text
Авторская симуляция C# 10 / .NET 6; using System; using System.Linq; доступны. Все вычисления decimal, входы не null, переполнение исключено. Напиши static decimal ScaledTotal(decimal[] xs,decimal factor): сначала сумма, затем умножение на factor, затем округление до 2 знаков AwayFromZero. Контроль xs=[0.007m,0.008m],factor=2m.
~~~~

**Chosen — полный правильный ответ:**

~~~~text
static decimal ScaledTotal(decimal[] xs, decimal factor) => Math.Round(xs.Sum() * factor, 2, MidpointRounding.AwayFromZero);
~~~~

**Rejected — правдоподобная содержательная ошибка:**

~~~~text
static decimal ScaledTotal(decimal[] xs, decimal factor) => Math.Round(xs.Sum(), 2, MidpointRounding.AwayFromZero) * factor;
~~~~

**Почему есть предпочтение:** Один дефект — округлён промежуточный итог вместо произведения.

Различающий дефект: `round_subtotal_before_factor`. Семья: `v7.ref.decimal.sum-factor`.

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
| `CODE-CS-ROUND-STAGE/base` | 161 | `DOC-SQL-WINDOW-FRAME/base` |

## Точные команды действующих потоков

Подготовка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Команды работают из любого каталога. RAG уже внутри пакета. Выполнить только назначенный поток; остальные строки — точные альтернативы назначения.

### CODE-CS-ROUND-STAGE/base

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "CODE-CS-ROUND-STAGE/base" -Run "S702" -Count 10 -Split train -Start 0
```

DEV/EXAM не запускать попутно с TRAIN. Когда назначено отдельное создание этих split в изолированном контексте, точные команды сохранены в `curriculum/pilot-queue.json` как `prepare_first_dev` / `prepare_first_exam`.

Первая TRAIN-папка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/CODE-CS-ROUND-STAGE/base/S702`. Следующий поток: `DOC-SQL-WINDOW-FRAME/base`. Доля TRAIN: 1000 пар; первая партия 10. Пилот TRAIN/DEV/EXAM: 100/20/20; набор `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/datasets/CODE-CS-ROUND-STAGE-base-v1`; B1 `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/baselines/code.csharp/base/CODE-CS-ROUND-STAGE-v1/B1`. Это назначенные пути, не существующие веса.

<!-- GENERATED-QUEUE-END -->
