# DOC-CS-FINALLY — Описать возврат и наблюдаемые эффекты finally

<!-- CREATE-ONLY-START -->
> **Действующий режим: CREATE_ONLY — только создание.** Прочитай назначение, создай реальные пары в `draft.jsonl`, выполни один структурный `finish`, перейди к следующему stream ID из общей очереди. Не запускай blind/review/oracle/accept/export/обучение и не жди проверяющего. `DRAFT_CREATED` означает запись кандидатов, а не проверенное качество.
>
> Корень наборов: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets`. Подготовка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Точные команды ниже работают из любого текущего каталога; готовый request содержит абсолютные пути. Не искать инструменты в `llm_mvp_core`, не создавать пустой RAG и не восстанавливать содержимое по checksum.
>
> Модель — текущая выбранная в Kiro, без override; не более одного помощника одновременно. При NEXT/новом ID немедленно перейти к нему, не дописывать прошлую задачу; STOP прекращает действия. Живой процесс ждать по его handle штатным wait/resume; timeout транспорта не доказывает отказ. [Точный контракт](../../20-CREATE-ONLY-AND-PATHS.md). Старые правила приёмки ниже описывают только будущую стадию.
> Роль человека при выборе материала выполняет CODEX с Astra 6 ultracode; оценку качества DPO также выполняет CODEX с Astra 6 ultracode. Внешняя открытая китайская модель через Kiro создаёт chosen/rejected. В CREATE_ONLY назначение уже задано карточкой/request: новый вызов выбора и оценка сейчас не запускаются и не задерживают создание следующей партии. Это назначение ролей, не подтверждение выполненного вызова.
<!-- CREATE-ONLY-END -->

## Назначение

Отдельно проследить вычисление возвращаемого значения, выполнение finally и то, что наблюдает вызывающий код.

Родитель: `DOC-CS-05`; домен `docs.csharp`, профиль `base`. C# 12 / .NET 8.0; synchronous console fixture.

## Источники и граница доказательства

[RAG-027](../../rag/sources/RAG-027.md); [RAG-028](../../rag/sources/RAG-028.md).

Источники подтверждают методику построения пар и предметную область. Программы и исходные данные ниже — новые авторские симуляции. Ожидаемые ответы не называются результатами runtime. Reference-примеры доступны публично; они и производные не являются закрытым EXAM. Семейные связи сохранены, включая обратные варианты старых SQL-сцен.

## Подробный промпт генератору

Создавай маленький полностью видимый синхронный метод с try/finally, иногда с catch, вложенным try или using. Для каждого prompt задай точный способ наблюдения: значение return, свойства возвращённого объекта, порядок Trace либо тип пойманной ошибки. Выбери одну границу: вычисленное value не меняется последующим присваиванием локальной переменной; возвращённая ссылка остаётся ссылкой на изменяемый объект; finally выполняется до внешнего catch; выброшенная из finally ошибка отменяет return. Для using покажи весь Dispose и точную область видимости. Не добавляй async, внешние callee, БД, непоказанный logger или предположения о GC. Требуй объяснить данный код, не предложить его «улучшение». Чередуй нормальный выход, return и ошибку; не учи правилу «finally всегда успешно возвращает результат».

Работай по одному request.json: один topic, objective, domain, profile=base и split. Создавай новые содержательные ситуации, а не переименования публичных примеров. Одна пара различается одним главным предметным дефектом; не делай rejected синтаксически сломанным, бессмысленным или вторым правильным ответом. Полный код, входы, версии, контракт зависимостей и формат ответа доступны в prompt: ученик не получает rationale и metadata. Сначала сформулируй ожидаемое поведение по условию, затем полный выбранный ответ и близкий ошибочный ответ.
Источники — только назначенные файлы xiigen-planning main/llm_plan3. Они подтверждают методику и тематику; новые мини-программы — авторские симуляции, не извлечённые результаты runtime. Все примеры этого дополнения имеют split=reference, training_allowed=false, oracle=NOT_RUN. Их нельзя переименовать в Chinese-generated TRAIN или скрытый EXAM. Родственные DOC/CODE/переводные версии, переименования и производные имеют один origin/leakage group и split. Новые family IDs не доказывают независимость содержания.
Роль человека при выборе материала выполняет CODEX с Astra 6 ultracode; оценку качества DPO также выполняет CODEX с Astra 6 ultracode. Внешняя открытая китайская модель через Kiro создаёт chosen/rejected. В CREATE_ONLY назначение уже задано карточкой/request: новый вызов выбора и оценка сейчас не запускаются и не задерживают создание следующей партии. Это назначение ролей, не подтверждение выполненного вызова.
Прочитай уже собранный prompt/request по абсолютным путям; не ищи другую схему в llm_mvp_core. Запиши реальные строки в request.paths.draft, выполни одну готовую команду finish и перейди к next_stream_id из полной execution-queue.json. PREPARED без draft не является созданием. Старые данные не удалять; существующий run продолжать через resume. При NEXT/новом ID сразу перейти к нему, не продолжать прежнюю задачу или её отчёт. STOP немедленно прекращает действия. Если есть живой handle, ждать штатным wait/resume до terminal в том же ходе: не запускать дубликаты, не опрашивать часто и не завершать ход словами «жду». Отчёт по-русски: тема, фактические строки/семьи, файл, модель, структурный статус и следующий ID.

## Матрица генерации

- путь выхода: return из try, обычное завершение, ошибка из try, return из catch, ошибка из finally.
- наблюдаемая сущность: скаляр, возвращённый объект, порядок Trace, Dispose, вложенный finally.

Форма ответа: Русский текст с точным результатом и одним объяснением по показанному коду. Не переписывать код.

## Полные предметные примеры

### 1. return_value_recomputed

**Задача ученику**

```text
Симуляция C# 12 / .NET 8.0. using System; using System.Collections.Generic; using System.Linq; доступны. Все методы выполняются синхронно. В классе объявлено: static int F(List<string> t) { int n=4; try { return n; } finally { n=9; t.Add(n.ToString()); } } Вызов: var t=new List<string>(); var r=F(t); Назови r и t, затем объясни порядок.
```

**Chosen**

```text
r=4; t=["9"]. Значение return вычислено до finally; присваивание n=9 меняет локальную переменную после вычисления результата.
```

**Rejected**

```text
r=9; t=["9"]. Присваивание n=9 в finally меняет возвращаемое значение перед выходом.
```

Различие: Rejected ошибочно вычисляет return повторно после finally.

Будущий различающий контроль: Выполнить F с новым списком; сравнить r и Trace. Статус: **NOT_RUN**.

Пара `DOC-CS-FINALLY.REF.01`, семейство `v6.ref.doc-cs-finally.value-captured`; split `reference`, training_allowed `false`.

### 2. reference_mistaken_for_value_copy

**Задача ученику**

```text
Симуляция C# 12 / .NET 8.0. using System; using System.Collections.Generic; using System.Linq; доступны. Все методы выполняются синхронно. Объявлены: sealed class Box { public int Value; } static Box F() { var b=new Box { Value=4 }; try { return b; } finally { b.Value=9; } } Вызывающий код: var r=F(); var v=r.Value; Назови v и объясни, какой объект возвращён.
```

**Chosen**

```text
v=9. Возвращается ссылка на тот же Box; finally меняет его Value до чтения свойства вызывающим кодом.
```

**Rejected**

```text
v=4. Return сохраняет независимую копию Box до выполнения finally.
```

Различие: Ссылка вычислена до finally, но объект не копируется.

Будущий различающий контроль: После F прочитать Value; сравнить 9 с утверждением о копии. Статус: **NOT_RUN**.

Пара `DOC-CS-FINALLY.REF.02`, семейство `v6.ref.doc-cs-finally.returned-reference`; split `reference`, training_allowed `false`.

### 3. catch_before_inner_finally

**Задача ученику**

```text
Симуляция C# 12 / .NET 8.0. using System; using System.Collections.Generic; using System.Linq; доступны. Все методы выполняются синхронно. var t=new List<string>(); try { try { t.Add("work"); throw new InvalidOperationException(); } finally { t.Add("cleanup"); } } catch (InvalidOperationException) { t.Add("caught"); } Назови t в порядке записей.
```

**Chosen**

```text
["work","cleanup","caught"]. Внутренний finally выполняется при выходе по ошибке до внешнего catch.
```

**Rejected**

```text
["work","caught","cleanup"]. Сначала внешний catch обрабатывает ошибку, затем выполняется внутренний finally.
```

Различие: Одна ошибка — переставлены обработка ошибки и cleanup.

Будущий различающий контроль: Исполнить фрагмент с новым t; сравнить последовательность. Статус: **NOT_RUN**.

Пара `DOC-CS-FINALLY.REF.03`, семейство `v6.ref.doc-cs-finally.outer-catch`; split `reference`, training_allowed `false`.

### 4. finally_only_on_exception

**Задача ученику**

```text
Симуляция C# 12 / .NET 8.0. using System; using System.Collections.Generic; using System.Linq; доступны. Все методы выполняются синхронно. var t=new List<string>(); try { t.Add("body"); } finally { t.Add("cleanup"); } t.Add("after"); Назови t. Исключений в Add нет.
```

**Chosen**

```text
["body","cleanup","after"]. Finally выполняется и при обычном завершении try, до следующего оператора.
```

**Rejected**

```text
["body","after"]. При отсутствии исключения finally пропускается.
```

Различие: Rejected пропускает один обязательный эффект.

Будущий различающий контроль: Проверить все три записи без вызова исключения. Статус: **NOT_RUN**.

Пара `DOC-CS-FINALLY.REF.04`, семейство `v6.ref.doc-cs-finally.normal-exit`; split `reference`, training_allowed `false`.

### 5. nested_finally_order_reversed

**Задача ученику**

```text
Симуляция C# 12 / .NET 8.0. using System; using System.Collections.Generic; using System.Linq; доступны. Все методы выполняются синхронно. static int F(List<string> t) { try { try { return 5; } finally { t.Add("inner"); } } finally { t.Add("outer"); } } var t=new List<string>(); var r=F(t); Назови r и t.
```

**Chosen**

```text
r=5; t=["inner","outer"]. При return сначала завершается внутренний finally, затем внешний.
```

**Rejected**

```text
r=5; t=["outer","inner"]. При return сначала завершается внешний finally, затем внутренний.
```

Различие: Одна ошибка — порядок двух видимых эффектов.

Будущий различающий контроль: Сравнить порядок двух Add при том же r=5. Статус: **NOT_RUN**.

Пара `DOC-CS-FINALLY.REF.05`, семейство `v6.ref.doc-cs-finally.nested-cleanup`; split `reference`, training_allowed `false`.

### 6. using_scope_extended_to_caller

**Задача ученику**

```text
Симуляция C# 12 / .NET 8.0. using System; using System.Collections.Generic; using System.Linq; доступны. Все методы выполняются синхронно. Объявлены: sealed class R : IDisposable { readonly List<string> t; public R(List<string> t0) { t=t0; } public void Dispose() { t.Add("dispose"); } } static void F(List<string> t) { using var r=new R(t); t.Add("body"); } var t=new List<string>(); F(t); t.Add("caller"); Назови t.
```

**Chosen**

```text
["body","dispose","caller"]. Using declaration освобождает r при выходе из F, до следующего оператора вызывающего кода.
```

**Rejected**

```text
["body","caller","dispose"]. Using declaration откладывает Dispose до завершения вызывающего кода.
```

Различие: Граница жизни ресурса определена телом F.

Будущий различающий контроль: Выполнить F; проверить момент видимого Dispose, не ждать GC. Статус: **NOT_RUN**.

Пара `DOC-CS-FINALLY.REF.06`, семейство `v6.ref.doc-cs-finally.using-scope`; split `reference`, training_allowed `false`.

### 7. return_survives_throwing_finally

**Задача ученику**

```text
Симуляция C# 12 / .NET 8.0. using System; using System.Collections.Generic; using System.Linq; доступны. Все методы выполняются синхронно. static int F() { try { return 3; } finally { throw new InvalidOperationException("cleanup"); } } string observed; try { observed=F().ToString(); } catch (InvalidOperationException e) { observed=e.Message; } Назови observed.
```

**Chosen**

```text
observed="cleanup". Ошибка из finally прерывает возврат значения 3 и перехватывается внешним catch.
```

**Rejected**

```text
observed="3". Уже вычисленное значение return сохраняет приоритет перед ошибкой из finally.
```

Различие: Rejected объявляет результат, который не передаётся вызывающему коду.

Будущий различающий контроль: Исполнить фрагмент и прочитать observed; не подменять ошибку успешным return. Статус: **NOT_RUN**.

Пара `DOC-CS-FINALLY.REF.07`, семейство `v6.ref.doc-cs-finally.cleanup-fails`; split `reference`, training_allowed `false`.

### 8. catch_return_skips_finally

**Задача ученику**

```text
Симуляция C# 12 / .NET 8.0. using System; using System.Collections.Generic; using System.Linq; доступны. Все методы выполняются синхронно. static int F(List<string> t) { try { throw new FormatException(); } catch (FormatException) { t.Add("catch"); return 2; } finally { t.Add("finally"); } } var t=new List<string>(); var r=F(t); Назови r и t.
```

**Chosen**

```text
r=2; t=["catch","finally"]. Return из catch также проходит через finally.
```

**Rejected**

```text
r=2; t=["catch"]. Return из catch обходит finally.
```

Различие: Один пропущенный эффект cleanup.

Будущий различающий контроль: Сравнить r и обе записи t после вызова. Статус: **NOT_RUN**.

Пара `DOC-CS-FINALLY.REF.08`, семейство `v6.ref.doc-cs-finally.catch-return`; split `reference`, training_allowed `false`.

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
| `DOC-CS-FINALLY/base` | 130 | `CODE-CS-STABLE-KEY/base` |

## Точные команды действующих потоков

Подготовка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Команды работают из любого каталога. RAG уже внутри пакета. Выполнить только назначенный поток; остальные строки — точные альтернативы назначения.

### DOC-CS-FINALLY/base

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "DOC-CS-FINALLY/base" -Run "S601" -Count 10 -Split train -Start 0
```

DEV/EXAM не запускать попутно с TRAIN. Когда назначено отдельное создание этих split в изолированном контексте, точные команды сохранены в `curriculum/pilot-queue.json` как `prepare_first_dev` / `prepare_first_exam`.

Первая TRAIN-папка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/DOC-CS-FINALLY/base/S601`. Следующий поток: `CODE-CS-STABLE-KEY/base`. Доля TRAIN: 1000 пар; первая партия 10. Пилот TRAIN/DEV/EXAM: 100/20/20; набор `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/datasets/DOC-CS-FINALLY-base-v1`; B1 `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/baselines/docs.csharp/base/DOC-CS-FINALLY-v1/B1`. Это назначенные пути, не существующие веса.

<!-- GENERATED-QUEUE-END -->
