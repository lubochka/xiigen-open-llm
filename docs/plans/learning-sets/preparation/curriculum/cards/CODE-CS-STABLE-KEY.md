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
| `A179` — `CODE-CS-STABLE-KEY` | `code.csharp` / `base` | 160 | `A180` |


Следующий ID из таблицы — номинальный порядок своей группы. Фактическую свободную карточку с учётом владельцев выдаёт команда `next` текущего request; не менять группу и чужой статус вручную.

Замечания качества: смотри `quality_flags` своего domain/topic/profile в общем status. `needs_rework` указывает доказанный дефект с evidence, `needs_assessment` — область без достаточной оценки. Это не новый review-гейт и не разрешение исправлять чужую старую попытку; finish не закрывает флаг автоматически. [Правила общей матрицы](../../28-PARALLEL-GENERATION-AND-QUALITY.md). Для нового окна используй [один из трёх готовых стартовых промптов](../../00-START-HERE.md); не исполняй исторические команды из нижней справочной части вместо текущих owner-команд.

<!-- /CURRENT-PARALLEL-CONTRACT-20261026 -->

# CODE-CS-STABLE-KEY — Удалить повторы по полному ключу с заданным порядком

<!-- DOMAIN-CAMPAIGN-AUTHORITY -->
> **Предметная справка к действующей кампании.** Квоты и продолжение задаёт [campaign/registry.json](../../campaign/registry.json), не старые команды ниже. 1000 пар относятся к домену в сумме. Прочитанный генератором рабочий prompt содержит только назначенную микротему.
>
> Данные: `C:/Projects/llm_mvp_core/learning-sets`. Инструкции: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. CREATE_ONLY: без review, checksum, production CI и обучения. Модель — текущая Kiro без override. Reason internally in English; do not output private reasoning. Отчёт по-русски.
>
> После партии выполнять готовый next до доменной квоты; не останавливаться с вопросом «продолжать?». При прямом NEXT/новом ID немедленно сменить задачу: запрещено дописывать/чинить предыдущую. STOP прекращает действия. Один живой handle ждать документированным wait/resume; не объявлять молчание падением. Старые попытки сохранять; выбранный request возобновлять, не пересоздавать.

Точные доли этой предметной карточки в текущей кампании:
| A-ID | Домен | Профиль | Доля из доменных1000 | Следующий A-ID после доли |
|---|---|---|---:|---|
| A179 | code.csharp | base | 160 | A180 |


Ниже — сохранённая предметная методика и исторические инструкции. [Полный прежний вариант](../../history/before-domain-campaign-2026-10-25/curriculum/cards/CODE-CS-STABLE-KEY.md). Старые числовые планы и команды в нём не запускаются вместо актуального request.
<!-- /DOMAIN-CAMPAIGN-AUTHORITY -->

## Назначение

Построить устойчивый отбор по явно заданному ключу, политике равенства и первому/последнему вхождению.

Родитель: `CODE-CS-03`; домен `code.csharp`, профиль `base`. C# 12 / .NET 8.0; synchronous console fixture.

## Источники и граница доказательства

[RAG-027](../../rag/sources/RAG-027.md); [RAG-028](../../rag/sources/RAG-028.md).

Источники подтверждают методику построения пар и предметную область. Программы и исходные данные ниже — новые авторские симуляции. Ожидаемые ответы не называются результатами runtime. Reference-примеры доступны публично; они и производные не являются закрытым EXAM. Семейные связи сохранены, включая обратные варианты старых SQL-сцен.

## Подробный промпт генератору

Задавай небольшой record Item, сигнатуру метода, ключ, comparer, политику null, выбор первого либо последнего вхождения и порядок результата. В learner prompt дай минимум один различающий вход и требование работать для любых входов того же контракта. Не разрешай ответу зашить конкретные элементы. Rejected компилируется и нарушает одну границу: пропускает tenant-часть ключа, меняет ordinal на ignore-case, выбирает последнюю строку вместо первой, теряет порядок либо исключает допустимый null. Сохраняй исходный массив и объекты в обоих вариантах, если мутация не является целевым дефектом. Нельзя учить универсальному «первый всегда лучше»: отдельные случаи требуют последних вхождений в порядке их позиций.

Работай по одному request.json: один topic, objective, domain, profile=base и split. Создавай новые содержательные ситуации, а не переименования публичных примеров. Одна пара различается одним главным предметным дефектом; не делай rejected синтаксически сломанным, бессмысленным или вторым правильным ответом. Полный код, входы, версии, контракт зависимостей и формат ответа доступны в prompt: ученик не получает rationale и metadata. Сначала сформулируй ожидаемое поведение по условию, затем полный выбранный ответ и близкий ошибочный ответ.
Источники — только назначенные файлы xiigen-planning main/llm_plan3. Они подтверждают методику и тематику; новые мини-программы — авторские симуляции, не извлечённые результаты runtime. Все примеры этого дополнения имеют split=reference, training_allowed=false, oracle=NOT_RUN. Их нельзя переименовать в Chinese-generated TRAIN или скрытый EXAM. Родственные DOC/CODE/переводные версии, переименования и производные имеют один origin/leakage group и split. Новые family IDs не доказывают независимость содержания.
Роль человека при выборе материала выполняет CODEX с Astra 6 ultracode; оценку качества DPO также выполняет CODEX с Astra 6 ultracode. Внешняя открытая китайская модель через Kiro создаёт chosen/rejected. В CREATE_ONLY назначение уже задано карточкой/request: новый вызов выбора и оценка сейчас не запускаются и не задерживают создание следующей партии. Это назначение ролей, не подтверждение выполненного вызова.
Прочитай уже собранный prompt/request по абсолютным путям; не ищи другую схему в llm_mvp_core. Запиши реальные строки в request.paths.draft, выполни одну готовую команду finish и перейди к next_stream_id из полной execution-queue.json. PREPARED без draft не является созданием. Старые данные не удалять; существующий run продолжать через resume. При NEXT/новом ID сразу перейти к нему, не продолжать прежнюю задачу или её отчёт. STOP немедленно прекращает действия. Если есть живой handle, ждать штатным wait/resume до terminal в том же ходе: не запускать дубликаты, не опрашивать часто и не завершать ход словами «жду». Отчёт по-русски: тема, фактические строки/семьи, файл, модель, структурный статус и следующий ID.

## Матрица генерации

- ключ: int, string Ordinal, string OrdinalIgnoreCase, nullable string, tenant+id.
- политика: первое в исходном порядке, последнее в порядке последних позиций.

Форма ответа: Полное объявление запрошенной функции/запроса, без Markdown и объяснений; типы и окружение заданы в prompt.

## Полные предметные примеры

### 1. last_payload_in_first_slot

**Задача ученику**

```text
Симуляция C# 12 / .NET 8.0. using System; using System.Collections.Generic; using System.Linq; доступны. Все методы выполняются синхронно. Дан record Item(int Id,string Value). Напиши static Item[] Keep(Item[] xs): оставить первое вхождение каждого Id в исходном порядке, xs не менять. Контроль: [(2,"first"),(1,"one"),(2,"later")]. Метод должен работать для любых таких входов. Только метод.
```

**Chosen**

```text
static Item[] Keep(Item[] xs) { var seen=new HashSet<int>(); var result=new List<Item>(); foreach (var x in xs) if (seen.Add(x.Id)) result.Add(x); return result.ToArray(); }
```

**Rejected**

```text
static Item[] Keep(Item[] xs) { var result=new List<Item>(); foreach (var x in xs) { int i=result.FindIndex(y=>y.Id==x.Id); if (i<0) result.Add(x); else result[i]=x; } return result.ToArray(); }
```

Различие: Rejected заменяет payload первого вхождения последним, сохраняя порядок ключей.

Будущий различающий контроль: Сравнить Value элемента Id=2 и неизменность исходного массива. Статус: **NOT_RUN**.

Пара `CODE-CS-STABLE-KEY.REF.01`, семейство `v6.ref.code-cs-stable-key.first-keeps-payload`; split `reference`, training_allowed `false`.

### 2. tenant_part_of_key_dropped

**Задача ученику**

```text
Симуляция C# 12 / .NET 8.0. using System; using System.Collections.Generic; using System.Linq; доступны. Все методы выполняются синхронно. Дан record Item(int Tenant,int Id,string Value). Напиши static Item[] Keep(Item[] xs): первое вхождение каждой пары (Tenant,Id), исходный порядок. Контроль: [(1,4,"A"),(2,4,"B"),(1,4,"C")]. xs не менять. Только метод.
```

**Chosen**

```text
static Item[] Keep(Item[] xs) { var seen=new HashSet<(int,int)>(); var result=new List<Item>(); foreach (var x in xs) if (seen.Add((x.Tenant,x.Id))) result.Add(x); return result.ToArray(); }
```

**Rejected**

```text
static Item[] Keep(Item[] xs) { var seen=new HashSet<int>(); var result=new List<Item>(); foreach (var x in xs) if (seen.Add(x.Id)) result.Add(x); return result.ToArray(); }
```

Различие: Rejected считает записи разных tenants одним ключом.

Будущий различающий контроль: Проверить наличие (2,4,"B"), а не только длину первого tenant. Статус: **NOT_RUN**.

Пара `CODE-CS-STABLE-KEY.REF.02`, семейство `v6.ref.code-cs-stable-key.full-tenant-key`; split `reference`, training_allowed `false`.

### 3. case_sensitive_contract_weakened

**Задача ученику**

```text
Симуляция C# 12 / .NET 8.0. using System; using System.Collections.Generic; using System.Linq; доступны. Все методы выполняются синхронно. Дан record Item(string Key,int Value), Key не null. Напиши static Item[] Keep(Item[] xs): первое вхождение каждого Key по StringComparer.Ordinal, исходный порядок. Контроль [("a",1),("A",2),("a",3)]. xs не менять.
```

**Chosen**

```text
static Item[] Keep(Item[] xs) { var seen=new HashSet<string>(StringComparer.Ordinal); return xs.Where(x=>seen.Add(x.Key)).ToArray(); }
```

**Rejected**

```text
static Item[] Keep(Item[] xs) { var seen=new HashSet<string>(StringComparer.OrdinalIgnoreCase); return xs.Where(x=>seen.Add(x.Key)).ToArray(); }
```

Различие: Смена comparer ошибочно объединяет a и A.

Будущий различающий контроль: Проверить обе различные строки ключей; порядок не сортировать. Статус: **NOT_RUN**.

Пара `CODE-CS-STABLE-KEY.REF.03`, семейство `v6.ref.code-cs-stable-key.case-sensitive`; split `reference`, training_allowed `false`.

### 4. case_insensitive_contract_ignored

**Задача ученику**

```text
Симуляция C# 12 / .NET 8.0. using System; using System.Collections.Generic; using System.Linq; доступны. Все методы выполняются синхронно. Дан record Item(string Key,int Value), Key не null. Напиши static Item[] Keep(Item[] xs): первое вхождение каждого Key по StringComparer.OrdinalIgnoreCase, исходный порядок. Контроль [("ab",1),("AB",2),("b",3)]. xs не менять.
```

**Chosen**

```text
static Item[] Keep(Item[] xs) { var seen=new HashSet<string>(StringComparer.OrdinalIgnoreCase); return xs.Where(x=>seen.Add(x.Key)).ToArray(); }
```

**Rejected**

```text
static Item[] Keep(Item[] xs) { var seen=new HashSet<string>(StringComparer.Ordinal); return xs.Where(x=>seen.Add(x.Key)).ToArray(); }
```

Различие: Обратный контроль: Ordinal теперь нарушает явно заданную политику.

Будущий различающий контроль: Проверить, что AB удалён, но исходная форма ab сохранена. Статус: **NOT_RUN**.

Пара `CODE-CS-STABLE-KEY.REF.04`, семейство `v6.ref.code-cs-stable-key.case-insensitive`; split `reference`, training_allowed `false`.

### 5. valid_null_key_filtered

**Задача ученику**

```text
Симуляция C# 12 / .NET 8.0. using System; using System.Collections.Generic; using System.Linq; доступны. Все методы выполняются синхронно. Nullable включён. Дан record Item(string? Key,int Value). Напиши static Item[] Keep(Item[] xs): первое вхождение каждого Key; null является одним допустимым ключом; непустые ключи сравниваются Ordinal. Контроль [(null,1),("x",2),(null,3)]. Порядок исходный; xs не менять.
```

**Chosen**

```text
static Item[] Keep(Item[] xs) { var seen=new HashSet<string?>(); return xs.Where(x=>seen.Add(x.Key)).ToArray(); }
```

**Rejected**

```text
static Item[] Keep(Item[] xs) { var seen=new HashSet<string?>(); return xs.Where(x=>x.Key is not null && seen.Add(x.Key)).ToArray(); }
```

Различие: Rejected удаляет допустимое первое null-вхождение.

Будущий различающий контроль: Проверить наличие null=1, отсутствие null=3 и исходный порядок. Статус: **NOT_RUN**.

Пара `CODE-CS-STABLE-KEY.REF.05`, семейство `v6.ref.code-cs-stable-key.null-is-valid-key`; split `reference`, training_allowed `false`.

### 6. last_positions_returned_reversed

**Задача ученику**

```text
Симуляция C# 12 / .NET 8.0. using System; using System.Collections.Generic; using System.Linq; доступны. Все методы выполняются синхронно. Дан record Item(int Id,string Value). Напиши static Item[] KeepLast(Item[] xs): последнее вхождение каждого Id, результат в порядке позиций этих последних вхождений в xs. Контроль [(1,"old"),(2,"two"),(1,"new")]. xs не менять.
```

**Chosen**

```text
static Item[] KeepLast(Item[] xs) { var seen=new HashSet<int>(); var result=new List<Item>(); for (int i=xs.Length-1;i>=0;i--) if (seen.Add(xs[i].Id)) result.Add(xs[i]); result.Reverse(); return result.ToArray(); }
```

**Rejected**

```text
static Item[] KeepLast(Item[] xs) { var seen=new HashSet<int>(); var result=new List<Item>(); for (int i=xs.Length-1;i>=0;i--) if (seen.Add(xs[i].Id)) result.Add(xs[i]); return result.ToArray(); }
```

Различие: Оба ответа выбирают последние значения; rejected переворачивает их требуемый порядок.

Будущий различающий контроль: Проверить последовательность Id 2,1 и неизменность xs. Статус: **NOT_RUN**.

Пара `CODE-CS-STABLE-KEY.REF.06`, семейство `v6.ref.code-cs-stable-key.last-position-order`; split `reference`, training_allowed `false`.

## Объём, сохранение и последующее обучение

Первая партия — 10 кандидатных пар. План учебного пилота 100 TRAIN / 20 DEV / 20 EXAM; это назначение будущего объёма, не выполненный эксперимент. TRAIN-пилот входит в общий родительский пул 1000, а не прибавляется к нему. Содержание прежней parent-карточки сохранено; исполняется единая очередь без повторной генерации parent поверх objective.

Все создаваемые наборы: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets`. Подготовка: тот же путь плюс `/preparation`. Существующие attempts не удалять; продолжение по request/state, новые версии под новым run ID. Точные команды, next ID, пути набора и будущего B1 формирует общий блок ниже. Эти пути не доказывают наличие весов.

При отдельно назначенном учебном опыте восстановить один совместимый обученный B1 для обоих плеч, обеспечить достаточный TRAIN и ненулевую предметную компетенцию, измерить обучение до/после и свежую загрузку. 0→0 не является валидным учебным результатом. Любой измеренный полезный выигрыш — успех по этому показателю и домену; рядом показать остальные дельты, регрессии и стоимость. Предметный learner сначала проверяется при верном маршруте, затем prompt→router→домен→ответ отдельно. Создание JSON само по себе не измеряет обучение. Решение о production-интеграции остаётся за пользователем.

<!-- GENERATED-QUEUE-START -->
## Переходы единой очереди

Этот блок воспроизводится из общей очереди и задаёт текущий запуск. Прежние команды/номера в основном тексте сохраняются как история. Уже начатую попытку продолжать по её фактическому request/state, не запускать повторно. Прямой ID выше порядка. При NEXT немедленно перейти к названному ID; запрещено продолжать прежнюю задачу или дописывать её отчёт. При STOP немедленно прекратить действия. Живой процесс ждать через его реальный handle до terminal; не завершать раунд вместо ожидания.

Канонический обзор — [EXECUTION-QUEUE.md](../../EXECUTION-QUEUE.md); [бюджет](../../curriculum/scale-plan.json) делится внутри родительского пула, пилот входит в него. Выбранная модель не переопределяется. Checksum, фиксированный профиль модели и production CI не являются условиями подготовки. Режим CREATE_ONLY: запись draft.jsonl → finish → следующий поток; никаких review/oracle/accept/export/обучения.

| Поток | Общий порядок | Следующий поток |
|---|---:|---|
| `CODE-CS-STABLE-KEY/base` | 131 | `DOC-SQL-FILTER-STAGE/base` |

## Точные команды действующих потоков

Подготовка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Команды работают из любого каталога. RAG уже внутри пакета. Выполнить только назначенный поток; остальные строки — точные альтернативы назначения.

### CODE-CS-STABLE-KEY/base

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "CODE-CS-STABLE-KEY/base" -Run "S602" -Count 10 -Split train -Start 0
```

DEV/EXAM не запускать попутно с TRAIN. Когда назначено отдельное создание этих split в изолированном контексте, точные команды сохранены в `curriculum/pilot-queue.json` как `prepare_first_dev` / `prepare_first_exam`.

Первая TRAIN-папка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/CODE-CS-STABLE-KEY/base/S602`. Следующий поток: `DOC-SQL-FILTER-STAGE/base`. Доля TRAIN: 1000 пар; первая партия 10. Пилот TRAIN/DEV/EXAM: 100/20/20; набор `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/datasets/CODE-CS-STABLE-KEY-base-v1`; B1 `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/baselines/code.csharp/base/CODE-CS-STABLE-KEY-v1/B1`. Это назначенные пути, не существующие веса.

<!-- GENERATED-QUEUE-END -->
