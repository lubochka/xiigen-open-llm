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
| `A159` — `DOC-CS-AWAIT` | `docs.csharp` / `base` | 170 | `A160` |


Следующий ID из таблицы — номинальный порядок своей группы. Фактическую свободную карточку с учётом владельцев выдаёт команда `next` текущего request; не менять группу и чужой статус вручную.

Замечания качества: смотри `quality_flags` своего domain/topic/profile в общем status. `needs_rework` указывает доказанный дефект с evidence, `needs_assessment` — область без достаточной оценки. Это не новый review-гейт и не разрешение исправлять чужую старую попытку; finish не закрывает флаг автоматически. [Правила общей матрицы](../../28-PARALLEL-GENERATION-AND-QUALITY.md). Для нового окна используй [один из трёх готовых стартовых промптов](../../00-START-HERE.md); не исполняй исторические команды из нижней справочной части вместо текущих owner-команд.

<!-- /CURRENT-PARALLEL-CONTRACT-20261026 -->

# DOC-CS-AWAIT — Документировать побочный эффект после await

<!-- DOMAIN-CAMPAIGN-AUTHORITY -->
> **Предметная справка к действующей кампании.** Квоты и продолжение задаёт [campaign/registry.json](../../campaign/registry.json), не старые команды ниже. 1000 пар относятся к домену в сумме. Прочитанный генератором рабочий prompt содержит только назначенную микротему.
>
> Данные: `C:/Projects/llm_mvp_core/learning-sets`. Инструкции: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. CREATE_ONLY: без review, checksum, production CI и обучения. Модель — текущая Kiro без override. Reason internally in English; do not output private reasoning. Отчёт по-русски.
>
> После партии выполнять готовый next до доменной квоты; не останавливаться с вопросом «продолжать?». При прямом NEXT/новом ID немедленно сменить задачу: запрещено дописывать/чинить предыдущую. STOP прекращает действия. Один живой handle ждать документированным wait/resume; не объявлять молчание падением. Старые попытки сохранять; выбранный request возобновлять, не пересоздавать.

Точные доли этой предметной карточки в текущей кампании:
| A-ID | Домен | Профиль | Доля из доменных1000 | Следующий A-ID после доли |
|---|---|---|---:|---|
| A159 | docs.csharp | base | 170 | A160 |


Ниже — сохранённая предметная методика и исторические инструкции. [Полный прежний вариант](../../history/before-domain-campaign-2026-10-25/curriculum/cards/DOC-CS-AWAIT.md). Старые числовые планы и команды в нём не запускаются вместо актуального request.
<!-- /DOMAIN-CAMPAIGN-AUTHORITY -->

> Новая подготовка: `tools/batches.py` по [действующему контракту](../../17-CURRENT-PREPARATION-CONTRACT.md) и точной команде в конце карточки. RAG уже в llm_plan3. Обязательные checksum, fixed-profile receipts и production gates не применяются. Старые `tools/sets.py` и пути в основном тексте — история прежнего runner; не запускать их поверх нового request. При NEXT/новом ID немедленно сменить задачу, запрещено продолжать прежнюю или её отчёт; при STOP прекратить действия.

> Для нового запуска используйте раздел «Точные команды действующих потоков» в конце этой карточки: он воспроизводит полную execution-queue.json. Примеры старых run в основном тексте сохраняются как история/иллюстрация. Если работа уже начата, продолжайте её фактический request/state; этот блок не разрешает повторную генерацию или замену существующих файлов.

Родительская тема: `DOC-CS-02`. Домен: `docs.csharp`. Профиль: `base`. Это дополнение к существующей теме: прежние карточки и результаты сохраняются.

## Обучаемая граница

Различать эффекты до await, только после успеха, в finally и в конкретном catch; сохранять outcome зависимости.

## Источники и смысл примеров

Источники: `RAG-027`, `RAG-028`, `RAG-029`. Точные commit/blob и локальные выдержки — [sources.lock.json](../../sources.lock.json) и RAG main. Они поддерживают методику и проектный контекст; описанные здесь мини-программы — авторские самодостаточные симуляции. В них нет измеренных результатов обучения. Число reference-строк не равно числу независимых семейств.

## Запуск подготовки

Действующая команда и точный каталог находятся в разделе «Точные команды действующих потоков» в конце карточки. Подготовка — `tools/batches.py`, RAG встроен в llm_plan3. Каталог содержит request, prompt, commands, state и шаблон отчёта; следующий шаг определяется реальным состоянием попытки. Старые инструкции этой секции сохранены в history/legacy-card-launch-sections.jsonl и не выполняются поверх нового request.

## Промпт генератору

```text
Составляй один маленький async wrapper с полностью показанными trace-эффектами и одним await. Каждая пара учит одну границу success/fault/cancel либо конкретного catch. Fault в fixture — конкретный InvalidOperationException; для typed catch дополнительно FormatException. Cancellation создаётся SetCanceled, а не SetException(OperationCanceledException): async wrapper может преобразовать faulted Task с cancellation exception в Canceled, поэтому общий тезис о неизменности Task status без ограничения не допускается. trace.Add не бросает. Не смешивай с передачей CancellationToken, конкурентностью потоков, сетевым retry или реальной БД. Chosen для DOC описывает каждый путь показанного кода, для CODE реализует явно заданные пути. Rejected компилируется, но переносит ровно один эффект или расширяет один catch. Обязательно pending→success и pending→Faulted(InvalidOperationException)/Canceled, иначе ранний эффект может пройти незамеченным.

Работай с одной назначенной микроцелью, одним domain_id/profile_id и точными family_id/split/count из request.json. Все исходные данные, версии, видимый код и ограничения ответа находятся в prompt; metadata ученик не получает. Chosen и rejected имеют одинаковый формат, а ошибочная сторона нарушает одну проверяемую границу. Не добавляй [WRONG], score, rationale или source ID в обучаемые ответы. Источники задают методику; авторские симуляции не выдавать за цитаты реального проекта, результаты китайской модели либо измеренный эффект обучения. Публичные reference-примеры и их потомки не помещать в DEV/EXAM. Семейство одного исходника общее для документации, кода и перефразирования; переименование и новые числа не создают независимости. Для новых пар сначала назначить происхождение и split; не наполнять квоту поверхностными дубликатами. Пиши отчёт на русском по 08-REPORT-TEMPLATE.md. Используй фактическую текущую модель Kiro без override, не угадывай имя/версию. Если точная модель не показана, сохраняй наблюдаемый selector и model_identity_observed=false. При явном NEXT сразу переходи к указанному пользователем ID; не возвращайся к прежней карточке. При STOP немедленно прекрати новые действия. Старые эксперименты и версии данных не удаляй; production не меняй. Один долгий процесс имеет один handle: обнаружь штатный wait/resume и дождись terminal в текущем раунде; не запускай заново вместо ожидания, не делай busy polling и не объявляй таймаут по отсутствию первых файлов. Oracle до фактического исполнения имеет NOT_RUN, result_observed=false. Runtime oracle сейчас NOT_RUN; это не препятствует записи кандидатов и не означает PASS. Учебный пилот позже восстанавливает совместимое обученное B1 с tokenizer/optimizer/RNG/split hashes; новое обучение B1 только по назначенному отдельному bootstrap-плану, с сохранением для повторного использования. 0 правильных до и после не доказывает обучение; loss/margin и число пар не заменяют полезный результат. Любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост точности не обязателен, изменения остальных показателей, включая ухудшения, показать рядом. Успешный опыт документируется кандидатом в исправления; перенос в production не выполняется автоматически.
```

## Договор ответа

DOC: русский текст с trace и outcome. CODE: только требуемое объявление метода.

## Оси генерации

- **размещение эффекта:** after success; finally; typed catch; before+after await
- **завершение:** pending; SetResult success; FormatException in typed-catch case; InvalidOperationException; SetCanceled

## Не смешивать с соседними темами

- retry
- thread scheduling performance
- настоящие БД/HTTP
- подмена outcome только потому, что UI хочет success

## Проверка смысла

- Одинаковый TaskCompletionSource harness для обеих сторон
- Без сна: управляемое завершение и await wrapper
- Сравнить trace ordinal и task outcome по каждому пути
- Одна основная ошибка на rejected, остальные части договора сохранены

Oracle создаёт reviewer в подкаталоге `oracle/` этого run, с точными fixture из каждой принятой строки. Это следующий шаг подготовки; готового native oracle-run результата здесь нет. Harness не получает error_tag/chosen-label как вход бизнес-функции. Compiler PASS и ручное рассуждение не заменяют runtime. Для CODE исполняются две реализации при одинаковых входах; для DOC исполняется единственный показанный в prompt код, затем обе текстовые/JSON-гипотезы сверяются с его trace. Нельзя выдавать описание ошибочного кода за исходную программу или компилировать русский текст вместо наблюдения. Для каждого executable candidate свежая среда.

### .NET fixture: точный состав и команда

Создать `oracle/Oracle.csproj`: SDK Microsoft.NET.Sdk, OutputType Exe, TargetFramework net6.0, LangVersion 10.0, Nullable enable. В CODE `Program.cs` содержит две отдельные test class со сторонами пары; в DOC — один показанный метод и проверку обеих гипотез о его trace. Общий spy/TaskCompletionSource harness выполняет проверки metadata.oracle. Program обязан разобрать обязательный `--out` как абсолютный путь, создать его родительский каталог и писать туда JSON с pair_id, фактическими traces/outcomes. Не вычислять output относительно cwd: команда запускается из preparation. Exit nonzero при ошибке chosen либо эквивалентности rejected по всем обязательным случаям. Использовать уже установленный .NET 6 SDK; не подменять target framework ради запуска.

```powershell
dotnet run --project C:/x/lsets/DOC-CS-02/base/D303/oracle/Oracle.csproj --configuration Release --no-launch-profile -- --out C:/x/lsets/DOC-CS-02/base/D303/oracle/results.json
```

## Полные публичные контрасты

### DOC-CS-AWAIT.REF.01

Общее семейство источника: `v3.ref.cs.await.success-audit`.

**Условие:**

```text
Авторская симуляция. C# 10 / .NET 6. Доступны System, System.Collections.Generic, System.Threading.Tasks. trace — List<string>, в начале пуст; trace.Add не бросает. Зависимость синхронно не бросает и возвращает pending Task. Допустимые завершения: SetResult — RanToCompletion; SetException(new InvalidOperationException()) — Faulted; SetCanceled — Canceled. Faulted Task, содержащая OperationCanceledException/TaskCanceledException, исключена из этого fixture. Внутри класса дан метод: async Task Save(Func<Task> store, List<string> trace) { await store(); trace.Add("saved"); } Опиши точную последовательность trace и outcome при success/fault/cancel; не добавляй скрытых catch.
```

**Предпочтительный ответ:**

```text
При успехе trace получает saved после завершения store. При InvalidOperationException и отменённой Task запись saved не добавляется; Task Save завершается соответственно faulted/canceled.
```

**Правдоподобный ошибочный ответ:**

```text
При успехе trace получает saved после завершения store. При InvalidOperationException и отменённой Task запись saved тоже добавляется; Task Save завершается соответственно faulted/canceled.
```

**Почему различаются:** Ошибка меняет путь побочного эффекта или границу перехвата; для доказательства нужно фактическое завершение pending Task, а не только чтение кода.

**Контрпример:** Начальный trace=[]; store — pending TaskCompletionSource. До SetResult trace пуст; после успеха [saved]. SetException(new InvalidOperationException()) даёт trace=[] и Faulted; SetCanceled() даёт trace=[] и Canceled. Faulted Task с OperationCanceledException здесь не создаётся.

### DOC-CS-AWAIT.REF.02

Общее семейство источника: `v3.ref.cs.await.finally-cleanup`.

**Условие:**

```text
Авторская симуляция. C# 10 / .NET 6. Доступны System, System.Collections.Generic, System.Threading.Tasks. trace — List<string>, в начале пуст; trace.Add не бросает. Зависимость синхронно не бросает и возвращает pending Task. Допустимые завершения: SetResult — RanToCompletion; SetException(new InvalidOperationException()) — Faulted; SetCanceled — Canceled. Faulted Task, содержащая OperationCanceledException/TaskCanceledException, исключена из этого fixture. Внутри класса дан метод: async Task Use(Func<Task> work, List<string> trace) { try { await work(); } finally { trace.Add("cleanup"); } } Опиши точную последовательность trace и outcome при success/fault/cancel; не добавляй скрытых catch.
```

**Предпочтительный ответ:**

```text
cleanup добавляется один раз в finally при успехе, InvalidOperationException и отменённой Task work. Wrapper завершается соответственно RanToCompletion/Faulted/Canceled.
```

**Правдоподобный ошибочный ответ:**

```text
cleanup добавляется один раз только при успехе work. При InvalidOperationException и отменённой Task cleanup отсутствует; wrapper завершается соответственно Faulted/Canceled.
```

**Почему различаются:** Ошибка меняет путь побочного эффекта или границу перехвата; для доказательства нужно фактическое завершение pending Task, а не только чтение кода.

**Контрпример:** SetException(new InvalidOperationException()): trace после await равен [cleanup], wrapper Faulted. SetCanceled(): [cleanup], wrapper Canceled. SetResult(): [cleanup], wrapper RanToCompletion. Faulted Task с OperationCanceledException не создаётся.

### DOC-CS-AWAIT.REF.03

Общее семейство источника: `v3.ref.cs.await.typed-catch`.

**Условие:**

```text
Авторская симуляция. C# 10 / .NET 6. Доступны System, System.Collections.Generic, System.Threading.Tasks. trace — List<string>, в начале пуст; trace.Add не бросает. Зависимость синхронно не бросает и возвращает pending Task. Допустимые завершения: SetResult — RanToCompletion; SetException(new InvalidOperationException()) — Faulted; SetCanceled — Canceled. Faulted Task, содержащая OperationCanceledException/TaskCanceledException, исключена из этого fixture. Только для этой пары дополнительно проверяется SetException(new FormatException()) — Faulted у зависимости. Внутри класса дан метод: async Task Record(Func<Task> work, List<string> trace) { try { await work(); trace.Add("ok"); } catch (FormatException) { trace.Add("format"); } } Опиши точную последовательность trace и outcome при success/fault/cancel; не добавляй скрытых catch.
```

**Предпочтительный ответ:**

```text
При успехе добавляется ok. FormatException заменяется записью format и успешным завершением Record. InvalidOperationException даёт Faulted, а отменённая Task — Canceled без изменения trace.
```

**Правдоподобный ошибочный ответ:**

```text
При успехе добавляется ok. FormatException, InvalidOperationException и отменённая Task заменяются записью format и успешным завершением Record.
```

**Почему различаются:** Ошибка меняет путь побочного эффекта или границу перехвата; для доказательства нужно фактическое завершение pending Task, а не только чтение кода.

**Контрпример:** Четыре отдельные pending задачи: SetResult→[ok]/RanToCompletion; SetException(new FormatException())→[format]/RanToCompletion; SetException(new InvalidOperationException())→[]/Faulted; SetCanceled→[]/Canceled. Faulted Task с OperationCanceledException не создаётся.

### DOC-CS-AWAIT.REF.04

Общее семейство источника: `v3.ref.cs.await.prefix-and-success`.

**Условие:**

```text
Авторская симуляция. C# 10 / .NET 6. Доступны System, System.Collections.Generic, System.Threading.Tasks. trace — List<string>, в начале пуст; trace.Add не бросает. Зависимость синхронно не бросает и возвращает pending Task. Допустимые завершения: SetResult — RanToCompletion; SetException(new InvalidOperationException()) — Faulted; SetCanceled — Canceled. Faulted Task, содержащая OperationCanceledException/TaskCanceledException, исключена из этого fixture. Внутри класса дан метод: async Task Run(Func<Task> work, List<string> trace) { trace.Add("start"); await work(); trace.Add("done"); } Опиши точную последовательность trace и outcome при success/fault/cancel; не добавляй скрытых catch.
```

**Предпочтительный ответ:**

```text
start записывается до ожидания work. При успехе затем добавляется done. При InvalidOperationException и отменённой Task trace остаётся [start], wrapper завершается соответственно Faulted/Canceled.
```

**Правдоподобный ошибочный ответ:**

```text
start записывается до ожидания work. Затем done добавляется при любом указанном завершении work, включая InvalidOperationException и отменённую Task; wrapper завершается соответственно Faulted/Canceled.
```

**Почему различаются:** Ошибка меняет путь побочного эффекта или границу перехвата; для доказательства нужно фактическое завершение pending Task, а не только чтение кода.

**Контрпример:** До завершения pending work trace=[start]. При SetResult: [start,done]/RanToCompletion. При SetException(new InvalidOperationException()): [start]/Faulted. При SetCanceled: [start]/Canceled. Faulted Task с OperationCanceledException не создаётся.

## Учебный пилот и переход дальше

Подготовка здесь не запускает обучение. Последующий пилот выполняется по [05-LEARNING-AND-CHECKPOINTS.md](../../05-LEARNING-AND-CHECKPOINTS.md) и [10-NATIVE-CONTRACT-BOUNDARY.md](../../10-NATIVE-CONTRACT-BOUNDARY.md). Получатель, training/inference entrypoints, данные и hashes состояния задаются binding; не выдумывать runtime по domain ID. Восстановить совместимое ненулевое B1; один исходный checkpoint для обоих плеч. DEV-калибровка и бюджет должны обеспечить действительное обучение; 0→0 не является доказательством. Измерять полезную способность, время урока/число обновлений до неё, удержание старого навыка и ресурсы участвующего пути. Успех одного показателя допускается с регрессией другого, оба показываются. Не переносить изменения в production по этому документу.

Канонический порядок — `curriculum/index.json:stream_order`, следующая запись определяется по objective_id/profile_id, не по номеру родительской темы. Прямой ID пользователя имеет приоритет. При NEXT перейти немедленно и не дописывать прошлый эксперимент; при STOP остановить новые действия. Старые файлы сохраняются. Не завершать рабочий раунд вместо штатного ожидания действующего процесса. Отчёт на русском, фактическая текущая Kiro-модель.

<!-- GENERATED-QUEUE-START -->
## Переходы единой очереди

Этот блок воспроизводится из общей очереди и задаёт текущий запуск. Прежние команды/номера в основном тексте сохраняются как история. Уже начатую попытку продолжать по её фактическому request/state, не запускать повторно. Прямой ID выше порядка. При NEXT немедленно перейти к названному ID; запрещено продолжать прежнюю задачу или дописывать её отчёт. При STOP немедленно прекратить действия. Живой процесс ждать через его реальный handle до terminal; не завершать раунд вместо ожидания.

Канонический обзор — [EXECUTION-QUEUE.md](../../EXECUTION-QUEUE.md); [бюджет](../../curriculum/scale-plan.json) делится внутри родительского пула, пилот входит в него. Выбранная модель не переопределяется. Checksum, фиксированный профиль модели и production CI не являются условиями подготовки. Режим CREATE_ONLY: запись draft.jsonl → finish → следующий поток; никаких review/oracle/accept/export/обучения.

| Поток | Общий порядок | Следующий поток |
|---|---:|---|
| `DOC-CS-AWAIT/base` | 89 | `CODE-CS-AWAIT/base` |

## Точные команды действующих потоков

Подготовка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Команды работают из любого каталога. RAG уже внутри пакета. Выполнить только назначенный поток; остальные строки — точные альтернативы назначения.

### DOC-CS-AWAIT/base

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "DOC-CS-AWAIT/base" -Run "D303" -Count 10 -Split train -Start 0
```

DEV/EXAM не запускать попутно с TRAIN. Когда назначено отдельное создание этих split в изолированном контексте, точные команды сохранены в `curriculum/pilot-queue.json` как `prepare_first_dev` / `prepare_first_exam`.

Первая TRAIN-папка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/DOC-CS-AWAIT/base/D303`. Следующий поток: `CODE-CS-AWAIT/base`. Доля TRAIN: 500 пар; первая партия 10. Пилот TRAIN/DEV/EXAM: 100/20/20; набор `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/datasets/DOC-CS-AWAIT-base-v1`; B1 `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/baselines/docs.csharp/base/DOC-CS-AWAIT-v1/B1`. Это назначенные пути, не существующие веса.

<!-- GENERATED-QUEUE-END -->
