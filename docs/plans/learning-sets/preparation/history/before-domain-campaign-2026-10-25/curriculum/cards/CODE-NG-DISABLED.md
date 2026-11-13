# CODE-NG-DISABLED — Написать boolean disabled binding

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

Родительская тема: `CODE-NG-01`. Предметные домены: `code.angular`. Это одна узкая обучаемая цель, без соседних навыков родительской карточки.

## Что должна усвоить модель

Реализовать ровно заданный boolean контракт отключения native button средствами Angular property binding.

## Контекст и источники

- Angular16.2 template expression
- Native disabled boolean property

Источники: `RAG-037`, `RAG-038`, `RAG-039`. Локаторы, commit/строки/хеши — [sources.lock.json](../../sources.lock.json). Источник может задавать метод подготовки; факты авторской симуляции находятся целиком в prompt.

## Предметный промпт

```text
Ответ — один <button type="button"> с текстом Save и [disabled] выражением. Prompt фиксирует типы полей и точное условие отключения. Все ветви truth table указываются в metadata для oracle, expected не выдаётся за наблюдённое. Rejected использует одну правдоподобную логическую ошибку: инверсия busy, потеря !canSave, && вместо || или потеря !force. Не добавляй event handlers, interpolation disabled="false", атрибутные/доступностные изменения и CSS в том же контрасте.

Работай только с данным objective_id и назначенными family_id/split/count из request.json. Пиши одну смысловую пару на назначенное семейство; chosen и rejected должны иметь одинаковый формат и отличаться одной проверяемой ошибкой. Все сведения, версии, исходные данные и договор ответа помещай в prompt: metadata learner не получает. Примеры ниже — открытые авторские reference seeds, не закрытый EXAM, не сгенерированный китайской моделью корпус и не измеренные результаты. Не копируй reference-строки в TRAIN под новым ID. Переименование переменных/таблиц и замена чисел не делают шаблон новым независимым семейством. Для документации объясняй именно показанный код, для генерации возвращай только запрошенный код. Не вставляй в ответы labels, rationale, source ID и предпочтительную позицию. Oracle expected — заранее назначенное ожидание; статус остаётся NOT_RUN, пока соответствующий runtime действительно не выполнен. Runtime oracle сейчас не запускай; сохрани его ожидания как NOT_RUN, не выдавай их за наблюдённый результат. Не переходи к другой теме внутри пакета. При NEXT немедленно используй следующий ID из реестра, при STOP прекрати новые действия. Production и старые эксперименты не менять; отчёт на русском по 08-REPORT-TEMPLATE.md, фактическая текущая Kiro-модель без override.
```

## Формат ответа

Один HTML button с type="button", текстом Save и единственным binding [disabled]; без объяснений, Markdown и других элементов.

## Оси новых сценариев

- **условие:** busy true; canSave false; busy или invalid; busy и не force
- **матрица:** все boolean комбинации

## Что не смешивать

- HTTP/RxJS
- форма/валидаторы
- visibility
- события кнопки
- дизайн/RTL

## Будущий этап: содержательная проверка (сейчас не запускать)

Метод: `executable_oracle`.

- Скомпилировать оба шаблона в идентичном fixture
- Проверить native disabled на всех boolean комбинациях
- Chosen совпадает с контрактом, rejected имеет хотя бы одно несовпадение
- Compilation и DOM execution учитывать отдельно, engine absent -> ORACLE_PENDING

## Полные публичные примеры

Это исходные опоры для понимания правила, не независимо сгенерированные TRAIN/EXAM. Контрпример — часть той же публичной семьи. Для каждого направления брать соответствующий профиль; не смешивать строки разных профилей в учебном пакете.

### CODE-NG-DISABLED.REF.01 / base

Семейство: `ref.ng.disabled.busy.01`.

**Условие:**

```text
Авторская симуляция. Angular 16.2 / TypeScript 5.1. Поле busy:boolean. Кнопка должна быть отключена ровно когда busy=true. Напиши только один native button type="button" с текстом Save и Angular [disabled] binding. Нет других bindings/host directives; после изменения полей вызывается fixture.detectChanges().
```

**Предпочтительный ответ:**

```text
<button type="button" [disabled]="busy">Save</button>
```

**Правдоподобный ошибочный ответ:**

```text
<button type="button" [disabled]="!busy">Save</button>
```

**Доказательство различия:** Одна изменённая boolean операция нарушает явно заданную таблицу отключения; обе стороны имеют один формат.

**Контрпример / граница:** busy=false: expected disabled=false; rejected=true.

### CODE-NG-DISABLED.REF.02 / base

Семейство: `ref.ng.disabled.can-save.01`.

**Условие:**

```text
Авторская симуляция. Angular 16.2 / TypeScript 5.1. Поле canSave:boolean. Кнопка должна быть отключена ровно когда canSave=false. Напиши только один native button type="button" с текстом Save и Angular [disabled] binding. Нет других bindings/host directives; после изменения полей вызывается fixture.detectChanges().
```

**Предпочтительный ответ:**

```text
<button type="button" [disabled]="!canSave">Save</button>
```

**Правдоподобный ошибочный ответ:**

```text
<button type="button" [disabled]="canSave">Save</button>
```

**Доказательство различия:** Одна изменённая boolean операция нарушает явно заданную таблицу отключения; обе стороны имеют один формат.

**Контрпример / граница:** canSave=true: expected disabled=false; rejected=true.

### CODE-NG-DISABLED.REF.03 / base

Семейство: `ref.ng.disabled.or.01`.

**Условие:**

```text
Авторская симуляция. Angular 16.2 / TypeScript 5.1. Поля busy:boolean и invalid:boolean. Кнопка отключена, если хотя бы одно из них true. При обоих false она доступна. Напиши только один native button type="button" с текстом Save и Angular [disabled] binding. Нет других bindings/host directives; после изменения полей вызывается fixture.detectChanges().
```

**Предпочтительный ответ:**

```text
<button type="button" [disabled]="busy || invalid">Save</button>
```

**Правдоподобный ошибочный ответ:**

```text
<button type="button" [disabled]="busy && invalid">Save</button>
```

**Доказательство различия:** Одна изменённая boolean операция нарушает явно заданную таблицу отключения; обе стороны имеют один формат.

**Контрпример / граница:** busy=true, invalid=false: expected disabled=true; rejected=false.

### CODE-NG-DISABLED.REF.04 / base

Семейство: `ref.ng.disabled.force.01`.

**Условие:**

```text
Авторская симуляция. Angular 16.2 / TypeScript 5.1. Поля busy:boolean и force:boolean. Кнопка отключена ровно при busy=true и force=false. Force=true разрешает кнопку независимо от busy. Напиши только один native button type="button" с текстом Save и Angular [disabled] binding. Нет других bindings/host directives; после изменения полей вызывается fixture.detectChanges().
```

**Предпочтительный ответ:**

```text
<button type="button" [disabled]="busy && !force">Save</button>
```

**Правдоподобный ошибочный ответ:**

```text
<button type="button" [disabled]="busy && force">Save</button>
```

**Доказательство различия:** Одна изменённая boolean операция нарушает явно заданную таблицу отключения; обе стороны имеют один формат.

**Контрпример / граница:** busy=true, force=true: expected disabled=false; rejected=true.

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
| `CODE-NG-DISABLED/base` | 37 | `OR-AUTO-TERMINAL/base` |

## Точные команды действующих потоков

Подготовка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Команды работают из любого каталога. RAG уже внутри пакета. Выполнить только назначенный поток; остальные строки — точные альтернативы назначения.

### CODE-NG-DISABLED/base

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "CODE-NG-DISABLED/base" -Run "M012" -Count 10 -Split train -Start 0
```

DEV/EXAM не запускать попутно с TRAIN. Когда назначено отдельное создание этих split в изолированном контексте, точные команды сохранены в `curriculum/pilot-queue.json` как `prepare_first_dev` / `prepare_first_exam`.

Первая TRAIN-папка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/CODE-NG-DISABLED/base/M012`. Следующий поток: `OR-AUTO-TERMINAL/base`. Доля TRAIN: 1000 пар; первая партия 10. Пилот TRAIN/DEV/EXAM: 100/20/20; набор `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/datasets/CODE-NG-DISABLED-base-v1`; B1 `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/baselines/code.angular/base/CODE-NG-DISABLED-v1/B1`. Это назначенные пути, не существующие веса.

<!-- GENERATED-QUEUE-END -->
