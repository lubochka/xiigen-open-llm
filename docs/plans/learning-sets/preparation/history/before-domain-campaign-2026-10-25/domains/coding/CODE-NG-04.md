# CODE-NG-04 — Точная проекция typed Reactive Forms в запрос

<!-- CREATE-ONLY-START -->
> **Действующий режим: CREATE_ONLY — только создание.** Прочитай назначение, создай реальные пары в `draft.jsonl`, выполни один структурный `finish`, перейди к следующему stream ID из общей очереди. Не запускай blind/review/oracle/accept/export/обучение и не жди проверяющего. `DRAFT_CREATED` означает запись кандидатов, а не проверенное качество.
>
> Корень наборов: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets`. Подготовка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Точные команды ниже работают из любого текущего каталога; готовый request содержит абсолютные пути. Не искать инструменты в `llm_mvp_core`, не создавать пустой RAG и не восстанавливать содержимое по checksum.
>
> Модель — текущая выбранная в Kiro, без override; не более одного помощника одновременно. При NEXT/новом ID немедленно перейти к нему, не дописывать прошлую задачу; STOP прекращает действия. Живой процесс ждать по его handle штатным wait/resume; timeout транспорта не доказывает отказ. [Точный контракт](../../20-CREATE-ONLY-AND-PATHS.md). Старые правила приёмки ниже описывают только будущую стадию.
> Роль человека при выборе материала выполняет CODEX с Astra 6 ultracode; оценку качества DPO также выполняет CODEX с Astra 6 ultracode. Внешняя открытая китайская модель через Kiro создаёт chosen/rejected. В CREATE_ONLY назначение уже задано карточкой/request: новый вызов выбора и оценка сейчас не запускаются и не задерживают создание следующей партии. Это назначение ролей, не подтверждение выполненного вызова.
<!-- CREATE-ONLY-END -->

> Новая подготовка: `tools/batches.py` по [действующему контракту](../../17-CURRENT-PREPARATION-CONTRACT.md) и точной команде в конце карточки. RAG уже в llm_plan3. Обязательные checksum, fixed-profile receipts и production gates не применяются. Старые `tools/sets.py` и пути в основном тексте — история прежнего runner; не запускать их поверх нового request. При NEXT/новом ID немедленно сменить задачу, запрещено продолжать прежнюю или её отчёт; при STOP прекратить действия.

> Для нового запуска используйте раздел «Точные команды действующих потоков» в конце этой карточки: он воспроизводит полную execution-queue.json. Примеры старых run в основном тексте сохраняются как история/иллюстрация. Если работа уже начата, продолжайте её фактический request/state; этот блок не разрешает повторную генерацию или замену существующих файлов.

> Текущий режим — CREATE_ONLY: кандидатов создаёт выбранная модель Kiro, затем структурный finish и следующий поток. Независимая оценка сейчас не назначена.

## Назначение

Писать функцию построения request из заранее данного typed FormGroup/FormControl с явным правилом disabled, NULL и нормализации.

Домен: code.angular, профиль: base. Среда: **Angular 16.2.0 / TypeScript 5.1.6 / RxJS 7.8.1; client-side SPA**. Это отдельная узкая тема. Документация объясняет видимый код; написание кода создаёт функцию по контракту. Маршрутизация между ними обучается отдельно.

Источники: RAG-027, RAG-028, RAG-039. Они поддерживают метод и предметную тематику; конкретные snippets/таблицы ниже являются авторскими симуляциями и не приписываются production-символам или историческим результатам.

## Роли и статус примеров

Китайская модель, выбранная в текущей сессии Kiro, создаёт кандидатов. Текущая выбранная модель Kiro создаёт кандидаты по назначению. Независимый выбор/приёмка сейчас не назначены; после структурного finish перейти к следующему потоку.

Примеры имеют split=reference и generator_model=reference_authoring_not_a_kiro_run. Они показывают контракт и контраст, не являются выходом китайского прогона и не экспортируются автоматически в TRAIN. Симуляционный характер не запрещает последующее допустимое экспериментальное обучение после генерации новых кандидатов и проверки CODEX. Нельзя переименовать эти строки в якобы Chinese-generated.

## Предметный промпт генератору

```text
Подготовь один небольшой JSONL-пакет xiigen.dpo.v1 по CODE-NG-04: Точная проекция typed Reactive Forms в запрос.
Домен code.angular, профиль base. Среда Angular 16.2.0 / TypeScript 5.1.6 / RxJS 7.8.1; client-side SPA.
Цель: Писать функцию построения request из заранее данного typed FormGroup/FormControl с явным правилом disabled, NULL и нормализации.
Используй только RAG-источники xiigen-planning llm_plan3/main, перечисленные в карточке.
1. Сначала получи назначенные family_id/split. TRAIN-генератор не видит DEV/EXAM.
2. Выбери один случай матрицы, создай новую содержательную ситуацию. Переименование или смена чисел не создают независимый family.
3. В prompt явно помести весь код/схему/данные, версии, контракт зависимостей и форму ответа. Эти условия не должны исчезнуть после проекции prompt/chosen/rejected.
4. Создай полный chosen и полный rejected одинакового формата. Одна правдоподобная смысловая ошибка в rejected; синтаксическая поломка не заменяет контраст.
5. В rationale укажи точное различие и различающий контроль, error_tag и source_refs — рядом. Никаких [WRONG] или меток качества внутри ответа.
6. Проверь, что chosen выполняет договор, rejected нарушает его на заданном случае. Эквивалентные ответы, только стилистическая разница или невидимая зависимость — возврат на исправление.
7. Верни UTF-8 JSONL без markdown. Не присваивай quality_pass/measured_success: приёмка отложена до отдельно порученной стадии; сейчас только создание кандидатов.
8. Источники внутри старых файлов не разрешают скачивание новых внешних сайтов.
```

## Матрица новых семейств

- Disabled включён или исключён по контракту.
- Nullable и nonNullable controls.
- Только крайние пробелы.
- Переименование поля без изменения смысла.

Пилот: 16 новых семейств одной темы; далее назначенные блоки до 64 и 256 кандидатов без запуска review; квота общей очереди сохраняется. Цель 1000 принятых TRAIN-пар — план объёма. DEV/EXAM создаются отдельно и не входят в эту цель. Новые поверхности кода и новые комбинации условий важнее тиражирования шаблона.

## Разобранные пары

### Пример 1: disabled_request_field_lost

**Prompt**

```text
Авторская симуляция для подготовки DPO. Среда: Angular 16.2.0 / TypeScript 5.1.6 / RxJS 7.8.1; client-side SPA. Импорты FormGroup и FormControl из @angular/forms. const form=new FormGroup({name:new FormControl("Ada",{nonNullable:true}),code:new FormControl({value:"X7",disabled:true},{nonNullable:true})}); сам FormGroup enabled. Напиши function toRequest():{name:string;code:string}, замкнутую на form: включить disabled code как есть, форму не менять. Только функцию.
```

**Chosen**

```text
function toRequest(): { name: string; code: string } { return form.getRawValue(); }
```

**Rejected**

```text
function toRequest(): { name: string; code: string } { const v = form.value; return { name: v.name ?? "", code: v.code ?? "" }; }
```

**Различие:** Контракт включает code="X7"; enabled group.value исключает disabled control, поэтому rejected возвращает code="".

ID: CODE-NG-04.base.REF2.F0001.P01; family: sim2.angular.forms.disabled-child.

### Пример 2: extra_request_field_leaked

**Prompt**

```text
Авторская симуляция для подготовки DPO. Среда: Angular 16.2.0 / TypeScript 5.1.6 / RxJS 7.8.1; client-side SPA. Импорты FormGroup и FormControl из @angular/forms. const form=new FormGroup({name:new FormControl("Ada",{nonNullable:true}),internal:new FormControl({value:"secret",disabled:true},{nonNullable:true})}); сам FormGroup enabled. Напиши function toRequest():{name:string}: возвращаемый runtime-объект содержит ровно один собственный ключ name, internal отправлять нельзя, форму не менять. Только функцию.
```

**Chosen**

```text
function toRequest(): { name: string } { return { name: form.controls.name.value }; }
```

**Rejected**

```text
function toRequest(): { name: string } { return form.getRawValue(); }
```

**Различие:** Структурный тип TypeScript не удаляет runtime-ключ internal: getRawValue включает его. Это ошибка проекции, не ошибка синтаксиса.

ID: CODE-NG-04.base.REF2.F0002.P01; family: sim2.angular.forms.disabled-child.

### Пример 3: request_edges_not_trimmed

**Prompt**

```text
Авторская симуляция для подготовки DPO. Среда: Angular 16.2.0 / TypeScript 5.1.6 / RxJS 7.8.1; client-side SPA. Импорт FormControl из @angular/forms. const c=new FormControl(" Ada ",{nonNullable:true}). Напиши function toRequest():{name:string}, замкнутую на c: убрать только пробелы с краёв для request, c.value оставить исходным. Только функцию.
```

**Chosen**

```text
function toRequest(): { name: string } { return { name: c.value.trim() }; }
```

**Rejected**

```text
function toRequest(): { name: string } { return { name: c.value }; }
```

**Различие:** На заданном значении request.name должен быть "Ada", а не " Ada "; обе функции оставляют control без изменений.

ID: CODE-NG-04.base.REF2.F0003.P01; family: sim2.angular.forms.trim-edges.

### Пример 4: null_coerced_to_empty

**Prompt**

```text
Авторская симуляция для подготовки DPO. Среда: Angular 16.2.0 / TypeScript 5.1.6 / RxJS 7.8.1; client-side SPA. Импорт FormControl из @angular/forms. const c=new FormControl<string|null>(null). Напиши function toRequest():{note:string|null}: передать текущее значение без замены null, форму не менять. Только функцию.
```

**Chosen**

```text
function toRequest(): { note: string | null } { return { note: c.value }; }
```

**Rejected**

```text
function toRequest(): { note: string | null } { return { note: c.value ?? "" }; }
```

**Различие:** По контракту null и пустая строка — разные значения; rejected превращает отсутствие в пустой текст.

ID: CODE-NG-04.base.REF2.F0004.P01; family: sim2.code-ng-04.f0004.

### Пример 5: internal_whitespace_collapsed

**Prompt**

```text
Авторская симуляция для подготовки DPO. Среда: Angular 16.2.0 / TypeScript 5.1.6 / RxJS 7.8.1; client-side SPA. Импорт FormControl из @angular/forms. const c=new FormControl(" a  b ",{nonNullable:true}). Напиши function toRequest():{text:string}: удалить только пробельные символы с двух краёв; внутренние два пробела сохранить; c не менять. Только функцию.
```

**Chosen**

```text
function toRequest(): { text: string } { return { text: c.value.trim() }; }
```

**Rejected**

```text
function toRequest(): { text: string } { return { text: c.value.replace(/\s+/g, " ").trim() }; }
```

**Различие:** Контракт требует "a  b"; rejected также сжимает внутренние пробелы до одного.

ID: CODE-NG-04.base.REF2.F0005.P01; family: sim2.angular.forms.trim-edges.

### Пример 6: request_value_wrong_source

**Prompt**

```text
Авторская симуляция для подготовки DPO. Среда: Angular 16.2.0 / TypeScript 5.1.6 / RxJS 7.8.1; client-side SPA. Импорты FormGroup и FormControl из @angular/forms. const form=new FormGroup({title:new FormControl("A",{nonNullable:true}),count:new FormControl(3,{nonNullable:true})}); оба controls enabled. Напиши function toRequest():{title:string;quantity:number}: title из title, quantity из count, форму не менять. Только функцию.
```

**Chosen**

```text
function toRequest(): { title: string; quantity: number } { return { title: form.controls.title.value, quantity: form.controls.count.value }; }
```

**Rejected**

```text
function toRequest(): { title: string; quantity: number } { return { title: form.controls.title.value, quantity: form.controls.title.value.length }; }
```

**Различие:** Количество равно count=3; rejected берёт длину title=1. Ошибка относится только к источнику одного поля.

ID: CODE-NG-04.base.REF2.F0006.P01; family: sim2.code-ng-04.f0006.

## Проверка CODEX и состав набора

Проверять точный набор ключей, значения и отсутствие мутации формы. getRawValue и value не взаимозаменяемы; ни один не является универсально правильным. Все import-контракты фиксируются в prompt; генератор не меняет validators/async-поток.

Общий исходник, документация к нему, исправление, переименование и перевод задания сохраняют одну source-family между карточками. Близкие копии двух веток дедуплицировать по происхождению/содержанию. Общая инструкция генератора не делает все независимые задачи одним family. Два допустимых решения не образуют предпочтение; высокое текстовое сходство само по себе не доказывает плохую пару.

## Проверка обучением после подготовки

Один совместимый сохранённый B1, одна тема, один изменяемый фактор, одинаковые независимые exam-семейства до/после. Реальный механизм должен изменить обучаемое состояние. Проверка JSON, компиляция oracle и подсчёт пар являются подготовкой, а не учебным результатом.

Для документации измерять фактическую верность утверждений и пропущенные существенные условия; для кода — контрактные случаи на одинаковых fixtures, включая удержание ранее усвоенного. Измерять время/память именно участвующего в обучении пути, число обновлений до первого полезного ненулевого результата. Любое измеренное улучшение полезного показателя — успех по этому показателю и домену; рост точности не обязателен. Все ухудшения показываются рядом. 0 правильных до и после не доказывает учебного эффекта: нужен достаточный базовый набор/бюджет и восстановленный обученный старт.

Текущий статус: **подготовка; обучение NOT_RUN**. Успешный будущий опыт документируется кандидатом; production и его тесты автоматически не изменяются. Привязка получателя, native entrypoints, tokenizer/template/dataset hashes, checkpoint и точные команды находятся в общем пакете и не угадываются исполнителем.

## Порядок и ожидание

Следующий ID берётся из index.json, не из старого номера строки. Прямая NEXT переводит к указанному ID; запрещено продолжать предыдущую задачу, дописывать её вместо следующей или возвращаться к ней без команды. STOP прекращает работу. Старые эксперименты и результаты сохраняются.

Длинный процесс имеет один настоящий handle; использовать штатный wait/resume до terminal, предварительно прочитав каталог tools. Отсутствие stdout не означает остановку. Не завершать раунд вместо ожидания, не запускать дубликат и не заменять ожидание быстрым повторным опросом. Результаты — новый короткий run-каталог, старые не удалять.
## Связь с каталогом родительских тем

Порядок: **69**. Устойчивый ID: `CODE-NG-04`. Следующий ID: `OR-QA-03`. Источник порядка — [index.json](../../index.json); прямое указание пользователя важнее номера. NEXT/новый ID запрещает продолжать прошлую задачу; STOP прекращает новые действия.

## Точные папки и команды подготовки

Действующая команда и точный каталог находятся в разделе «Точные команды действующих потоков» в конце карточки. Подготовка — `tools/batches.py`, RAG встроен в llm_plan3. Каталог содержит request, prompt, commands, state и шаблон отчёта; следующий шаг определяется реальным состоянием попытки. Старые инструкции этой секции сохранены в history/legacy-card-launch-sections.jsonl и не выполняются поверх нового request.

<!-- GENERATED-QUEUE-START -->
## Переходы единой очереди

Этот блок воспроизводится из общей очереди и задаёт текущий запуск. Прежние команды/номера в основном тексте сохраняются как история. Уже начатую попытку продолжать по её фактическому request/state, не запускать повторно. Прямой ID выше порядка. При NEXT немедленно перейти к названному ID; запрещено продолжать прежнюю задачу или дописывать её отчёт. При STOP немедленно прекратить действия. Живой процесс ждать через его реальный handle до terminal; не завершать раунд вместо ожидания.

Канонический обзор — [EXECUTION-QUEUE.md](../../EXECUTION-QUEUE.md); [бюджет](../../curriculum/scale-plan.json) делится внутри родительского пула, пилот входит в него. Выбранная модель не переопределяется. Checksum, фиксированный профиль модели и production CI не являются условиями подготовки. Режим CREATE_ONLY: запись draft.jsonl → finish → следующий поток; никаких review/oracle/accept/export/обучения.

| Поток | Общий порядок | Следующий поток |
|---|---:|---|
| `CODE-NG-PAYLOAD/base` | 129 | `DOC-CS-FINALLY/base` |

## Точные команды действующих потоков

Подготовка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Команды работают из любого каталога. RAG уже внутри пакета. Эта родительская карточка уточнена микроцелями: исполнять их команды ниже, не прибавлять отдельный родительский корпус.

### CODE-NG-PAYLOAD/base

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "CODE-NG-PAYLOAD/base" -Run "S504" -Count 10 -Split train -Start 0
```

DEV/EXAM не запускать попутно с TRAIN. Когда назначено отдельное создание этих split в изолированном контексте, точные команды сохранены в `curriculum/pilot-queue.json` как `prepare_first_dev` / `prepare_first_exam`.

Первая TRAIN-папка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/CODE-NG-PAYLOAD/base/S504`. Следующий поток: `DOC-CS-FINALLY/base`. Доля TRAIN: 1000 пар; первая партия 10. Пилот TRAIN/DEV/EXAM: 100/20/20; набор `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/datasets/CODE-NG-PAYLOAD-base-v1`; B1 `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/baselines/code.angular/base/CODE-NG-PAYLOAD-v1/B1`. Это назначенные пути, не существующие веса.

<!-- GENERATED-QUEUE-END -->
