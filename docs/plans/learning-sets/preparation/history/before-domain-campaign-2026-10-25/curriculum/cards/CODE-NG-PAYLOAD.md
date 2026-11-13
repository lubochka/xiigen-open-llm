# CODE-NG-PAYLOAD — Построить точный runtime-payload формы

<!-- CREATE-ONLY-START -->
> **Действующий режим: CREATE_ONLY — только создание.** Прочитай назначение, создай реальные пары в `draft.jsonl`, выполни один структурный `finish`, перейди к следующему stream ID из общей очереди. Не запускай blind/review/oracle/accept/export/обучение и не жди проверяющего. `DRAFT_CREATED` означает запись кандидатов, а не проверенное качество.
>
> Корень наборов: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets`. Подготовка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Точные команды ниже работают из любого текущего каталога; готовый request содержит абсолютные пути. Не искать инструменты в `llm_mvp_core`, не создавать пустой RAG и не восстанавливать содержимое по checksum.
>
> Модель — текущая выбранная в Kiro, без override; не более одного помощника одновременно. При NEXT/новом ID немедленно перейти к нему, не дописывать прошлую задачу; STOP прекращает действия. Живой процесс ждать по его handle штатным wait/resume; timeout транспорта не доказывает отказ. [Точный контракт](../../20-CREATE-ONLY-AND-PATHS.md). Старые правила приёмки ниже описывают только будущую стадию.
> Роль человека при выборе материала выполняет CODEX с Astra 6 ultracode; оценку качества DPO также выполняет CODEX с Astra 6 ultracode. Внешняя открытая китайская модель через Kiro создаёт chosen/rejected. В CREATE_ONLY назначение уже задано карточкой/request: новый вызов выбора и оценка сейчас не запускаются и не задерживают создание следующей партии. Это назначение ролей, не подтверждение выполненного вызова.
<!-- CREATE-ONLY-END -->

> Новая подготовка: `tools/batches.py` по [действующему контракту](../../17-CURRENT-PREPARATION-CONTRACT.md) и точной команде в конце карточки. RAG уже в llm_plan3. Обязательные checksum, fixed-profile receipts и production gates не применяются. Старые `tools/sets.py` и пути в основном тексте — история прежнего runner; не запускать их поверх нового request. При NEXT/новом ID немедленно сменить задачу, запрещено продолжать прежнюю или её отчёт; при STOP прекратить действия.

Статус: **DATA_PREPARATION_ONLY**, 25.10.2026. Новые авторские reference-примеры; китайская генерация, oracle и обучение не выполнялись. Старые материалы сохранены.

| Поле | Значение |
|---|---|
| Тема / поддомен | Написание кода / `code.angular` |
| Родитель / микроцель / профиль | `CODE-NG-04` / `CODE-NG-PAYLOAD` / `base` |
| Первый run | `S504`, 10 новых пар |
| План пилота | 100 TRAIN, 20 DEV, 20 EXAM; это будущая квота, не готовый корпус |
| Модель | Фактическая модель по умолчанию текущей Kiro-сессии, без override |
| Отчёт | По-русски, [шаблон 08](../../08-REPORT-TEMPLATE.md) |

## Учебная граница

Выбирать value/raw/явную проекцию по договору о собственных ключах и позициях без изменения формы и без утечки скрытых полей.

Родительская карточка остаётся источником остальных случаев. Эта микроцель добавляет контрбаланс и явные поздние изменения; количество файлов не считается улучшением обучения. CODE реализует явно заданный договор. Две формы одного исходника имеют общее семейство до назначения split, даже если обучаются разные получатели.

## Прочитанные основания

RAG-027 и RAG-028 — правила документирования видимого кода и заданной версии стека. RAG-039 — проектный источник версионного Angular-контекста; он не является документацией API Reactive Forms. Источники подтверждают метод составления, а конкретные API-результаты ниже — проверяемые авторские ожидания, не найденные runtime-измерения. Точные пути/версии и прочитанные диапазоны находятся в [sources.lock.json](../../sources.lock.json). Разрешённые предметные источники — только xiigen-planning/main и llm_plan3.

## Подробный промпт китайской модели

```text
Составляй одну функцию toRequest вокруг полностью видимого Angular fixture. Условие задаёт точный набор разрешённых собственных ключей на каждом уровне и политику disabled: включить значение, исключить поле/поддерево либо сохранить все позиции массива. Требуй отсутствие мутации формы. Дай валидный компилируемый near-miss с одной ошибкой состава. Контрбалансируй парой контрактов одного семейства: все значения FormArray требуют raw; только enabled значения требуют value. Не смешивай trim, null-normalization, HTTP, валидацию и async. Не объявляй два корректных способа проекции противоположными ответами.

Работай только с указанными objective_id, topic_id, domain_id и profile_id=base из request.json. Одна партия обучает одну границу. Все условия, импортируемые типы, версии, исходные значения, последовательность действий и формат ответа находятся в prompt: metadata ученик не получает. Chosen и rejected имеют одну форму и отличаются одним главным нарушением смысла; два допустимых решения нельзя объявлять контрастом. Исторические источники задают методику, а мини-программы — явно помеченные авторские симуляции, не факты production и не измеренные результаты.

Пиши UTF-8 JSONL по xiigen.dpo.v1 с metadata.learning_objective_id. Примеры карточки открыты и имеют split=reference; они и их производные не могут стать закрытым DEV/EXAM. Общий код, его документация, исправление, контрфактический контракт, перевод и переименование сохраняют общий origin/leakage group до назначения split. Не выдавай новые имена и числа за новое независимое семейство. Заполняй только назначенную квоту; сначала маленькая проверяемая партия, затем масштабирование.

Используй выбранную по умолчанию модель текущей Kiro-сессии, без override. Если наблюдается только selector, запиши его и model_identity_observed=false; не угадывай точную версию. Для подготовки не нужны checksum, подписи, предварительные selection receipts, full-build/coverage, два разных генератора или N формальных review-проходов. Нужны содержательное предпочтение, полный контекст и происхождение семейства. Независимая оценка относится к отдельному будущему поручению; генератор сохраняет кандидаты без утверждения о приёмке. Expected oracle — предсказание авторинга: status=NOT_RUN, result_observed=false до реального исполнения.

Применяй навыки планирования, проверки плана, архитектуры, проверки кода, промпт-инжиниринга и коммуникации из 06-SOURCES-AND-SKILLS.md в рамках подготовки данных. Отчёт на русском по 08-REPORT-TEMPLATE.md. При NEXT/«следующий»/новом ID немедленно перейти к указанной карточке: запрещено дописывать предыдущий опыт, возвращаться к нему или продолжать старое задание вместо нового. При STOP/«стоп» прекратить новые действия. Старые карточки, попытки и эксперименты не удалять и не перезаписывать.

Один длительный процесс имеет один handle. Обнаружь в каталоге инструментов штатный wait/resume и дождись terminal в текущем раунде; не перезапускай процесс из-за отсутствия первых файлов, не подменяй ожидание busy polling и не заканчивай раунд словами «жду». Впоследствии учебный пилот восстанавливает совместимый ранее обученный B1. Новое bootstrap-обучение допускается только при отсутствии подходящего состояния по явному плану с сохранением для повторного применения. Проверяется реальное изменение обучаемого состояния; 0 правильных до и после не доказывает учебный результат. Любое измеренное улучшение полезного показателя — успех для этого показателя и домена; остальные изменения, включая ухудшения, показать рядом. Результат успешного опыта — кандидат в исправления, без автоматической production-интеграции.
```

## Точное создание задания

Действующая команда и точный каталог находятся в разделе «Точные команды действующих потоков» в конце карточки. Подготовка — `tools/batches.py`, RAG встроен в llm_plan3. Каталог содержит request, prompt, commands, state и шаблон отчёта; следующий шаг определяется реальным состоянием попытки. Старые инструкции этой секции сохранены в history/legacy-card-launch-sections.jsonl и не выполняются поверх нового request.

## Договор ответа

Только полное объявление запрошенной функции/метода, без markdown и пояснений.

## Публичные опорные контрасты

### CODE-NG-PAYLOAD.REF.01

Общее семейство: `v5.ref.angular.payload.disabled-root`.

**Условие:**

```text
Авторская симуляция. Angular 16.2.0 / TypeScript 5.1.6 / RxJS 7.8.1; client-side SPA. FormControl, FormGroup и FormArray импортированы из @angular/forms. Операторы выполняются последовательно, без пользовательских validators и async-кода. Все FormControl созданы с nonNullable:true. const form=new FormGroup({name:new FormControl("Ada",{nonNullable:true}),code:new FormControl({value:"X7",disabled:true},{nonNullable:true})}); form.disable({emitEvent:false}); Напиши function toRequest(): object, замкнутую на показанную form либо tags. Вернуть объект с ровно name="Ada" и code="X7", несмотря на disabled root; форму не менять. Функция должна читать текущие значения при каждом вызове, а не возвращать константы. После первого вызова внешний код выполнит form.controls.name.setValue("Mira"); form.controls.code.setValue("X8"); без изменения enabled/disabled и состава controls; второй вызов должен отразить эти новые значения при том же правиле ключей/позиций. Контракт действует и для других значений этих же controls. Только полное объявление функции.
```

**Предпочтительный ответ:**

```text
function toRequest(): object { return form.value; }
```

**Правдоподобный ошибочный ответ:**

```text
function toRequest(): object { return { name: form.controls.name.value }; }
```

**Почему различаются:** Группа явно целиком disabled; правило enabled parent об исключении disabled children здесь не применяется.

**Различающий случай:** Выполнить toRequest(); проверить JSON.stringify результата, отсортированные Object.keys на каждом уровне и отсутствие мутации forms state. Ожидаемый объект: {"name":"Ada","code":"X7"}.

### CODE-NG-PAYLOAD.REF.02

Общее семейство: `v5.ref.angular.payload.nested-child-excluded`.

**Условие:**

```text
Авторская симуляция. Angular 16.2.0 / TypeScript 5.1.6 / RxJS 7.8.1; client-side SPA. FormControl, FormGroup и FormArray импортированы из @angular/forms. Операторы выполняются последовательно, без пользовательских validators и async-кода. Все FormControl созданы с nonNullable:true. const form=new FormGroup({profile:new FormGroup({name:new FormControl("Ada",{nonNullable:true}),internal:new FormControl({value:"I1",disabled:true},{nonNullable:true})}),active:new FormControl(true,{nonNullable:true})}); Корневая form и вложенная profile enabled. Напиши function toRequest(): object, замкнутую на показанную form либо tags. Вернуть ровно {profile:{name:"Ada"},active:true}; internal запрещён даже во вложенном объекте. Форму не менять. Функция должна читать текущие значения при каждом вызове, а не возвращать константы. После первого вызова внешний код выполнит form.controls.profile.controls.name.setValue("Mira"); form.controls.active.setValue(false); без изменения enabled/disabled и состава controls; второй вызов должен отразить эти новые значения при том же правиле ключей/позиций. Контракт действует и для других значений этих же controls. Только полное объявление функции.
```

**Предпочтительный ответ:**

```text
function toRequest(): object { return form.value; }
```

**Правдоподобный ошибочный ответ:**

```text
function toRequest(): object { return form.getRawValue(); }
```

**Почему различаются:** Enabled вложенная группа строит value без disabled internal; getRawValue включает его.

**Различающий случай:** Выполнить toRequest(); проверить JSON.stringify результата, отсортированные Object.keys на каждом уровне и отсутствие мутации forms state. Ожидаемый объект: {"profile":{"name":"Ada"},"active":true}.

### CODE-NG-PAYLOAD.REF.03

Общее семейство: `v5.ref.angular.payload.disabled-subgroup`.

**Условие:**

```text
Авторская симуляция. Angular 16.2.0 / TypeScript 5.1.6 / RxJS 7.8.1; client-side SPA. FormControl, FormGroup и FormArray импортированы из @angular/forms. Операторы выполняются последовательно, без пользовательских validators и async-кода. Все FormControl созданы с nonNullable:true. const profile=new FormGroup({name:new FormControl("Ada",{nonNullable:true})}); const form=new FormGroup({profile,active:new FormControl(true,{nonNullable:true})}); profile.disable({emitEvent:false}); Корневая form остаётся enabled благодаря enabled active. Напиши function toRequest(): object, замкнутую на показанную form либо tags. Вернуть только текущее active, без собственного ключа profile. Все disabled поддеревья должны отсутствовать, форму не менять. Функция должна читать текущие значения при каждом вызове, а не возвращать константы. После первого вызова внешний код выполнит form.controls.active.setValue(false); без изменения enabled/disabled и состава controls; второй вызов должен отразить эти новые значения при том же правиле ключей/позиций. Контракт действует и для других значений этих же controls. Только полное объявление функции.
```

**Предпочтительный ответ:**

```text
function toRequest(): object { return form.value; }
```

**Правдоподобный ошибочный ответ:**

```text
function toRequest(): object { return form.getRawValue(); }
```

**Почему различаются:** Parent enabled и его disabled child — вся группа profile. Отсутствие ключа profile отличается от пустого объекта profile.

**Различающий случай:** Выполнить toRequest(); проверить JSON.stringify результата, отсортированные Object.keys на каждом уровне и отсутствие мутации forms state. Ожидаемый объект: {"active":true}.

### CODE-NG-PAYLOAD.REF.04

Общее семейство: `v5.ref.angular.payload.explicit-allowlist`.

**Условие:**

```text
Авторская симуляция. Angular 16.2.0 / TypeScript 5.1.6 / RxJS 7.8.1; client-side SPA. FormControl, FormGroup и FormArray импортированы из @angular/forms. Операторы выполняются последовательно, без пользовательских validators и async-кода. Все FormControl созданы с nonNullable:true. const form=new FormGroup({title:new FormControl("Report",{nonNullable:true}),internal:new FormControl("I1",{nonNullable:true})}); form и оба controls enabled. Напиши function toRequest(): { title: string }, замкнутую на показанную form либо tags. Вернуть runtime-объект с ровно одним собственным ключом title и его текущим значением. internal запрещён независимо от enabled; форму не менять. Функция должна читать текущие значения при каждом вызове, а не возвращать константы. После первого вызова внешний код выполнит form.controls.title.setValue("Next"); без изменения enabled/disabled и состава controls; второй вызов должен отразить эти новые значения при том же правиле ключей/позиций. Контракт действует и для других значений этих же controls. Только полное объявление функции.
```

**Предпочтительный ответ:**

```text
function toRequest(): { title: string } { return { title: form.controls.title.value }; }
```

**Правдоподобный ошибочный ответ:**

```text
function toRequest(): { title: string } { return form.getRawValue(); }
```

**Почему различаются:** Структурный TypeScript type разрешает дополнительные поля у возвращаемого значения и не преобразует runtime-объект.

**Различающий случай:** Выполнить toRequest(); проверить JSON.stringify результата, отсортированные Object.keys на каждом уровне и отсутствие мутации forms state. Ожидаемый объект: {"title":"Report"}.

### CODE-NG-PAYLOAD.REF.05

Общее семейство: `v5.ref.angular.payload.array-preserve-positions`.

**Условие:**

```text
Авторская симуляция. Angular 16.2.0 / TypeScript 5.1.6 / RxJS 7.8.1; client-side SPA. FormControl, FormGroup и FormArray импортированы из @angular/forms. Операторы выполняются последовательно, без пользовательских validators и async-кода. Все FormControl созданы с nonNullable:true. const tags=new FormArray([new FormControl("red",{nonNullable:true}),new FormControl({value:"hidden",disabled:true},{nonNullable:true}),new FormControl("blue",{nonNullable:true})]); tags enabled. Напиши function toRequest(): { tags: string[] }, замкнутую на показанную form либо tags. Вернуть {tags:string[]} со всеми тремя текущими значениями, сохранив исходные позиции, включая disabled control; tags не менять. Функция должна читать текущие значения при каждом вызове, а не возвращать константы. После первого вызова внешний код выполнит tags.at(0).setValue("green"); tags.at(2).setValue("gold"); без изменения enabled/disabled и состава controls; второй вызов должен отразить эти новые значения при том же правиле ключей/позиций. Контракт действует и для других значений этих же controls. Только полное объявление функции.
```

**Предпочтительный ответ:**

```text
function toRequest(): { tags: string[] } { return { tags: tags.getRawValue() }; }
```

**Правдоподобный ошибочный ответ:**

```text
function toRequest(): { tags: string[] } { return { tags: tags.value }; }
```

**Почему различаются:** В enabled FormArray value — компактный массив enabled значений; raw сохраняет все элементы.

**Различающий случай:** Выполнить toRequest(); проверить JSON.stringify результата, отсортированные Object.keys на каждом уровне и отсутствие мутации forms state. Ожидаемый объект: {"tags":["red","hidden","blue"]}.

### CODE-NG-PAYLOAD.REF.06

Общее семейство: `v5.ref.angular.payload.array-preserve-positions`.

**Условие:**

```text
Авторская симуляция. Angular 16.2.0 / TypeScript 5.1.6 / RxJS 7.8.1; client-side SPA. FormControl, FormGroup и FormArray импортированы из @angular/forms. Операторы выполняются последовательно, без пользовательских validators и async-кода. Все FormControl созданы с nonNullable:true. const tags=new FormArray([new FormControl("red",{nonNullable:true}),new FormControl({value:"hidden",disabled:true},{nonNullable:true}),new FormControl("blue",{nonNullable:true})]); tags enabled. Напиши function toRequest(): { tags: string[] }, замкнутую на показанную form либо tags. Вернуть {tags:string[]} только с enabled значениями в относительном порядке, без пустых позиций и без disabled значения; tags не менять. Функция должна читать текущие значения при каждом вызове, а не возвращать константы. После первого вызова внешний код выполнит tags.at(0).setValue("green"); tags.at(2).setValue("gold"); без изменения enabled/disabled и состава controls; второй вызов должен отразить эти новые значения при том же правиле ключей/позиций. Контракт действует и для других значений этих же controls. Только полное объявление функции.
```

**Предпочтительный ответ:**

```text
function toRequest(): { tags: string[] } { return { tags: tags.value }; }
```

**Правдоподобный ошибочный ответ:**

```text
function toRequest(): { tags: string[] } { return { tags: tags.getRawValue() }; }
```

**Почему различаются:** Контракт требует именно enabled значения; тот же getRawValue, который подходит для полного снимка, здесь отправляет запрещённый элемент.

**Различающий случай:** Выполнить toRequest(); проверить JSON.stringify результата, отсортированные Object.keys на каждом уровне и отсутствие мутации forms state. Ожидаемый объект: {"tags":["red","blue"]}.

## Будущий этап: проверка результата и учебный пилот (сейчас не запускать)

1. Сначала независимый reviewer сравнивает все видимые условия и ответы. При неполном условии вернуть конкретную строку на исправление. Равнозначные варианты не принимаются как DPO-контраст.
2. Для DOC выполнить только программу из prompt, затем сравнить обе гипотезы с фактическим выводом. Для CODE выполнить обе полные функции на одинаковом fixture. Runtime-версия остаётся Angular 16.2.0 / TypeScript 5.1.6 / RxJS 7.8.1; client-side SPA. Проверять значения и структуру; успешная компиляция сама по себе не подтверждает предпочтение.
3. Путь предметного oracle для каждого run: `oracle/`; результат `oracle/results.json` содержит pair_id, фактическую версию среды, наблюдаемые значения, статус каждой стороны. Если соответствующая среда отсутствует, оставить ORACLE_PENDING; не подменять её иной версией или ручным рассуждением.
4. Будущий корпус: `C:/x/lsets/datasets/CODE-NG-PAYLOAD-base-v1/`. Повторно используемый обученный старт: `C:/x/lsets/baselines/code.angular/base/CODE-NG-PAYLOAD-v1/B1/`. Первое действие учебного запуска — восстановить совместимое B1 и измерить ненулевое исходное умение на независимых новых семьях. Файлы reference не становятся EXAM.
5. Сравнить равный бюджет/одинаковый B1 для одной изменённой причины. Измерить правильность предметного ответа, число обновлений до полезного результата, время обучения/проверки, память и сохранение старого навыка. 0→0 невалидно как учебное доказательство; достаточность bootstrap-набора и бюджета планируется до запуска. Любой полезный измеренный выигрыш фиксируется рядом со всеми регрессиями, без требования обязательного роста accuracy.
6. Точные native recipient/entrypoints берутся из [10-NATIVE-CONTRACT-BOUNDARY](../../10-NATIVE-CONTRACT-BOUNDARY.md). Если получатель не определён, корпус остаётся подготовленным кандидатом; не изобретай production adapter или training CLI.

## Порядок, NEXT/STOP и ожидание

Действующий порядок и следующий ID — [EXECUTION-QUEUE.md](../../EXECUTION-QUEUE.md), машинный источник — [execution-queue.json](../../execution-queue.json). Прямая команда пользователя выше очереди. При NEXT немедленно перейти к названному новому ID; запрещено продолжать прежний опыт или дописывать его вместо следующего. При STOP прекратить новые действия. Нельзя удалять старые эксперименты.

Один запуск — один process/task handle. Найди поддерживаемый wait/resume в каталоге инструментов и дождись terminal в текущем раунде. Не объявляй отказ по отсутствию stdout, не заменяй ожидание повторным запуском, частым polling или завершением ответа «ожидаю». Незавершённый процесс не получает измеренный статус.

<!-- GENERATED-QUEUE-START -->
## Переходы единой очереди

Этот блок воспроизводится из общей очереди и задаёт текущий запуск. Прежние команды/номера в основном тексте сохраняются как история. Уже начатую попытку продолжать по её фактическому request/state, не запускать повторно. Прямой ID выше порядка. При NEXT немедленно перейти к названному ID; запрещено продолжать прежнюю задачу или дописывать её отчёт. При STOP немедленно прекратить действия. Живой процесс ждать через его реальный handle до terminal; не завершать раунд вместо ожидания.

Канонический обзор — [EXECUTION-QUEUE.md](../../EXECUTION-QUEUE.md); [бюджет](../../curriculum/scale-plan.json) делится внутри родительского пула, пилот входит в него. Выбранная модель не переопределяется. Checksum, фиксированный профиль модели и production CI не являются условиями подготовки. Режим CREATE_ONLY: запись draft.jsonl → finish → следующий поток; никаких review/oracle/accept/export/обучения.

| Поток | Общий порядок | Следующий поток |
|---|---:|---|
| `CODE-NG-PAYLOAD/base` | 129 | `DOC-CS-FINALLY/base` |

## Точные команды действующих потоков

Подготовка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Команды работают из любого каталога. RAG уже внутри пакета. Выполнить только назначенный поток; остальные строки — точные альтернативы назначения.

### CODE-NG-PAYLOAD/base

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "CODE-NG-PAYLOAD/base" -Run "S504" -Count 10 -Split train -Start 0
```

DEV/EXAM не запускать попутно с TRAIN. Когда назначено отдельное создание этих split в изолированном контексте, точные команды сохранены в `curriculum/pilot-queue.json` как `prepare_first_dev` / `prepare_first_exam`.

Первая TRAIN-папка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/CODE-NG-PAYLOAD/base/S504`. Следующий поток: `DOC-CS-FINALLY/base`. Доля TRAIN: 1000 пар; первая партия 10. Пилот TRAIN/DEV/EXAM: 100/20/20; набор `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/datasets/CODE-NG-PAYLOAD-base-v1`; B1 `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/baselines/code.angular/base/CODE-NG-PAYLOAD-v1/B1`. Это назначенные пути, не существующие веса.

<!-- GENERATED-QUEUE-END -->
