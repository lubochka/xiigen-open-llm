# DOC-NG-DISABLED — Документировать boolean disabled binding

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

Родительская тема: `DOC-NG-01`. Предметные домены: `docs.angular`. Это одна узкая обучаемая цель, без соседних навыков родительской карточки.

## Что должна усвоить модель

Читать значение HTMLButtonElement.disabled из заданного Angular property binding после detectChanges.

## Контекст и источники

- Angular property binding
- Boolean truth table
- TestBed fixture with normal change detection

Источники: `RAG-037`, `RAG-038`, `RAG-039`. Локаторы, commit/строки/хеши — [sources.lock.json](../../sources.lock.json). Источник может задавать метод подготовки; факты авторской симуляции находятся целиком в prompt.

## Предметный промпт

```text
Только [disabled] на native button и явно заданные boolean поля компонента. Prompt содержит полный рассматриваемый template fragment, типы полей и последовательность назначений. Для каждого шага после fixture.detectChanges() определить disabled. Не трактовать disabled как скрытие DOM. Rejected имеет одну ошибку polarity или AND/OR, но остальные утверждения верны. Не добавляй HTTP, async pipe, form validation implementation, *ngIf, events, routing или CSS. Четыре комбинации двух booleans должны быть представлены, чтобы постоянный ответ не выглядел навыком.

Работай только с данным objective_id и назначенными family_id/split/count из request.json. Пиши одну смысловую пару на назначенное семейство; chosen и rejected должны иметь одинаковый формат и отличаться одной проверяемой ошибкой. Все сведения, версии, исходные данные и договор ответа помещай в prompt: metadata learner не получает. Примеры ниже — открытые авторские reference seeds, не закрытый EXAM, не сгенерированный китайской моделью корпус и не измеренные результаты. Не копируй reference-строки в TRAIN под новым ID. Переименование переменных/таблиц и замена чисел не делают шаблон новым независимым семейством. Для документации объясняй именно показанный код, для генерации возвращай только запрошенный код. Не вставляй в ответы labels, rationale, source ID и предпочтительную позицию. Oracle expected — заранее назначенное ожидание; статус остаётся NOT_RUN, пока соответствующий runtime действительно не выполнен. Runtime oracle сейчас не запускай; сохрани его ожидания как NOT_RUN, не выдавай их за наблюдённый результат. Не переходи к другой теме внутри пакета. При NEXT немедленно используй следующий ID из реестра, при STOP прекрати новые действия. Production и старые эксперименты не менять; отчёт на русском по 08-REPORT-TEMPLATE.md, фактическая текущая Kiro-модель без override.
```

## Формат ответа

Русский ответ: список disabled=true/false по заданным шагам и одно короткое объяснение формулы; без нового кода.

## Оси новых сценариев

- **binding:** busy; !canSave; busy || invalid; busy && !force
- **состояния:** false→true; true→false; все 4 пары boolean

## Что не смешивать

- HTTP/RxJS
- form validation implementation
- visibility/*ngIf
- click handlers
- a11y/RTL как отдельный фактор

## Будущий этап: содержательная проверка (сейчас не запускать)

Метод: `executable_oracle`.

- Создать TestBed fixture на Angular16.2/TypeScript5.1 с данным template
- Каждое присваивание завершить detectChanges
- Читать nativeElement.querySelector("button").disabled, не наличие атрибута
- Зафиксировать фактический boolean массив; DOM assertion не подменять string search

## Полные публичные примеры

Это исходные опоры для понимания правила, не независимо сгенерированные TRAIN/EXAM. Контрпример — часть той же публичной семьи. Для каждого направления брать соответствующий профиль; не смешивать строки разных профилей в учебном пакете.

### DOC-NG-DISABLED.REF.01 / base

Семейство: `ref.ng.disabled.busy.01`.

**Условие:**

```text
Авторская симуляция. Angular 16.2 / TypeScript 5.1. Native button, обычный TestBed fixture; после каждого присваивания вызывается fixture.detectChanges(). Других bindings и host directives нет. Template: <button type="button" [disabled]="busy">Save</button>. Поля/порядок: busy:boolean; значения по шагам false, затем true. Опиши native button.disabled по шагам.
```

**Предпочтительный ответ:**

```text
По шагам: disabled=false, затем disabled=true. Кнопка отключается ровно при busy=true.
```

**Правдоподобный ошибочный ответ:**

```text
По шагам: disabled=true, затем disabled=false. Кнопка отключается ровно при busy=false.
```

**Доказательство различия:** Property получает само busy, а не !busy.

**Контрпример / граница:** Назначенные состояния [{"busy": false}, {"busy": true}] должны дать [false, true].

### DOC-NG-DISABLED.REF.02 / base

Семейство: `ref.ng.disabled.can-save.01`.

**Условие:**

```text
Авторская симуляция. Angular 16.2 / TypeScript 5.1. Native button, обычный TestBed fixture; после каждого присваивания вызывается fixture.detectChanges(). Других bindings и host directives нет. Template: <button type="button" [disabled]="!canSave">Save</button>. Поля/порядок: canSave:boolean; значения по шагам false, затем true. Опиши native button.disabled по шагам.
```

**Предпочтительный ответ:**

```text
По шагам: disabled=true, затем disabled=false. Отключение задаётся отрицанием canSave.
```

**Правдоподобный ошибочный ответ:**

```text
По шагам: disabled=false, затем disabled=true. Отключение повторяет canSave.
```

**Доказательство различия:** В binding присутствует !; он меняет оба результата.

**Контрпример / граница:** Назначенные состояния [{"canSave": false}, {"canSave": true}] должны дать [true, false].

### DOC-NG-DISABLED.REF.03 / base

Семейство: `ref.ng.disabled.or.01`.

**Условие:**

```text
Авторская симуляция. Angular 16.2 / TypeScript 5.1. Native button, обычный TestBed fixture; после каждого присваивания вызывается fixture.detectChanges(). Других bindings и host directives нет. Template: <button type="button" [disabled]="busy || invalid">Save</button>. Поля/порядок: busy:boolean и invalid:boolean; пары (false,false), (true,false), (false,true), (true,true). Опиши native button.disabled по шагам.
```

**Предпочтительный ответ:**

```text
По шагам: disabled=false, true, true, true. Достаточно любого из двух признаков busy или invalid.
```

**Правдоподобный ошибочный ответ:**

```text
По шагам: disabled=false, false, false, true. Отключение требует одновременно busy и invalid.
```

**Доказательство различия:** Оператор || не требует обоих признаков сразу.

**Контрпример / граница:** Назначенные состояния [{"busy": false, "invalid": false}, {"busy": true, "invalid": false}, {"busy": false, "invalid": true}, {"busy": true, "invalid": true}] должны дать [false, true, true, true].

### DOC-NG-DISABLED.REF.04 / base

Семейство: `ref.ng.disabled.force.01`.

**Условие:**

```text
Авторская симуляция. Angular 16.2 / TypeScript 5.1. Native button, обычный TestBed fixture; после каждого присваивания вызывается fixture.detectChanges(). Других bindings и host directives нет. Template: <button type="button" [disabled]="busy && !force">Save</button>. Поля/порядок: busy:boolean и force:boolean; пары (false,false), (true,false), (false,true), (true,true). Опиши native button.disabled по шагам.
```

**Предпочтительный ответ:**

```text
По шагам: disabled=false, true, false, false. Отключение требует busy=true и force=false.
```

**Правдоподобный ошибочный ответ:**

```text
По шагам: disabled=false, false, false, true. Отключение требует busy=true и force=true.
```

**Доказательство различия:** В показанном выражении force отрицается.

**Контрпример / граница:** Назначенные состояния [{"busy": false, "force": false}, {"busy": true, "force": false}, {"busy": false, "force": true}, {"busy": true, "force": true}] должны дать [false, true, false, false].

## Точные потоки и пути

Действующая команда и точный каталог находятся в разделе «Точные команды действующих потоков» в конце карточки. Подготовка — `tools/batches.py`, RAG встроен в llm_plan3. Каталог содержит request, prompt, commands, state и шаблон отчёта; следующий шаг определяется реальным состоянием попытки. Старые инструкции этой секции сохранены в history/legacy-card-launch-sections.jsonl и не выполняются поверх нового request.

## Правила исполнения и последующего обучения

Модель — фактически выбранная модель текущей Kiro-сессии; override и выдуманное имя запрещены. Генератор, исполнитель и обучаемая модель различаются в отчёте. Скилы и точные ссылки — [06](../../06-SOURCES-AND-SKILLS.md): intent-to-plan, plan-self-validation, human-readable-architecture-audit, code-examination, prompt-patch-authoring, human-communication; дополнительные предметные скилы там же. Отчёт на русском по [08](../../08-REPORT-TEMPLATE.md).

Формат — [02](../../02-DATA-CONTRACT.md), запуск — [11](../../11-MICROTOPICS-AND-BATCH-PLAN.md). Пиши draft.jsonl, затем выполни структурный finish и переходи к следующему потоку. DRAFT_CREATED не означает приёмку или обучение. Пробелы, регистр, Unicode, код и исходные условия сохраняются; не вставлять rationale, score, error_tag или метку правильности в ответы. Если нельзя доказать предпочтение, сохранить пару с причиной вне принятого набора.

Публичные опоры и их потомки не превращать в DEV/EXAM. Все варианты одного исходника/сценария сохраняют family, origin и leakage groups до split. Между языками/опечатками/раскладкой/документацией-кодом использовать общий family-plan. Номер строки и новое имя не доказывают независимость семьи.

Это задание готовит материал. Сейчас генерация Teacher, runtime oracle и обучение имеют NOT_RUN. Для будущего учебного сравнения восстановить совместимый сохранённый B1; если его нет, выполнить отдельное назначенное producer-задание один раз. Путь B1 для каждого профиля указан ниже; [05](../../05-LEARNING-AND-CHECKPOINTS.md) и [10](../../10-NATIVE-CONTRACT-BOUNDARY.md) задают native binding. Не выдумывать новый runner, его флаги или уже сохранённые веса. Обычный вывод до/после/после restore измеряет этот же навык на новых семьях. 0→0 правильных ответов не доказывает обучение; loss/margin или скорость сборки не заменяют предметный результат.

Любое измеренное улучшение полезного показателя на валидном учебном опыте — успех по этому показателю и домену; accuracy не обязана расти. Показать все остальные изменения, включая ухудшения, и документировать кандидата для решения Любы. Production-код, тесты продукта и старые эксперименты не менять; старые данные не удалять.

До долгого запуска прочитать схему реально доступного инструмента процесса. Один запуск — один handle; штатный wait/resume до terminal, без busy polling, дублирования и нового монитора. Тишина stdout или окончание yield не означают отказ. Не завершать ход вместо обработки результата. При NEXT немедленно перейти к указанному ID: запрещено продолжать предыдущую задачу, дописывать её отчёт или повторять запуск. При STOP немедленно прекратить новые действия. Существующие результаты сохраняются.

<!-- GENERATED-QUEUE-START -->
## Переходы единой очереди

Этот блок воспроизводится из общей очереди и задаёт текущий запуск. Прежние команды/номера в основном тексте сохраняются как история. Уже начатую попытку продолжать по её фактическому request/state, не запускать повторно. Прямой ID выше порядка. При NEXT немедленно перейти к названному ID; запрещено продолжать прежнюю задачу или дописывать её отчёт. При STOP немедленно прекратить действия. Живой процесс ждать через его реальный handle до terminal; не завершать раунд вместо ожидания.

Канонический обзор — [EXECUTION-QUEUE.md](../../EXECUTION-QUEUE.md); [бюджет](../../curriculum/scale-plan.json) делится внутри родительского пула, пилот входит в него. Выбранная модель не переопределяется. Checksum, фиксированный профиль модели и production CI не являются условиями подготовки. Режим CREATE_ONLY: запись draft.jsonl → finish → следующий поток; никаких review/oracle/accept/export/обучения.

| Поток | Общий порядок | Следующий поток |
|---|---:|---|
| `DOC-NG-DISABLED/base` | 36 | `CODE-NG-DISABLED/base` |

## Точные команды действующих потоков

Подготовка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Команды работают из любого каталога. RAG уже внутри пакета. Выполнить только назначенный поток; остальные строки — точные альтернативы назначения.

### DOC-NG-DISABLED/base

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "DOC-NG-DISABLED/base" -Run "M011" -Count 10 -Split train -Start 0
```

DEV/EXAM не запускать попутно с TRAIN. Когда назначено отдельное создание этих split в изолированном контексте, точные команды сохранены в `curriculum/pilot-queue.json` как `prepare_first_dev` / `prepare_first_exam`.

Первая TRAIN-папка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/DOC-NG-DISABLED/base/M011`. Следующий поток: `CODE-NG-DISABLED/base`. Доля TRAIN: 1000 пар; первая партия 10. Пилот TRAIN/DEV/EXAM: 100/20/20; набор `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/datasets/DOC-NG-DISABLED-base-v1`; B1 `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/baselines/docs.angular/base/DOC-NG-DISABLED-v1/B1`. Это назначенные пути, не существующие веса.

<!-- GENERATED-QUEUE-END -->
