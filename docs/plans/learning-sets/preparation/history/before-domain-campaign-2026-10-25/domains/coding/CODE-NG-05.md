# CODE-NG-05 — Ожидание runtime config по явному контракту

<!-- CREATE-ONLY-START -->
> **Действующий режим: CREATE_ONLY — только создание.** Прочитай назначение, создай реальные пары в `draft.jsonl`, выполни один структурный `finish`, перейди к следующему stream ID из общей очереди. Не запускай blind/review/oracle/accept/export/обучение и не жди проверяющего. `DRAFT_CREATED` означает запись кандидатов, а не проверенное качество.
>
> Корень наборов: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets`. Подготовка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Точные команды ниже работают из любого текущего каталога; готовый request содержит абсолютные пути. Не искать инструменты в `llm_mvp_core`, не создавать пустой RAG и не восстанавливать содержимое по checksum.
>
> Модель — текущая выбранная в Kiro, без override; не более одного помощника одновременно. При NEXT/новом ID немедленно перейти к нему, не дописывать прошлую задачу; STOP прекращает действия. Живой процесс ждать по его handle штатным wait/resume; timeout транспорта не доказывает отказ. [Точный контракт](../../20-CREATE-ONLY-AND-PATHS.md). Старые правила приёмки ниже описывают только будущую стадию.
> Роль человека при выборе материала выполняет CODEX с Astra 6 ultracode; оценку качества DPO также выполняет CODEX с Astra 6 ultracode. Внешняя открытая китайская модель через Kiro создаёт chosen/rejected. В CREATE_ONLY назначение уже задано карточкой/request: новый вызов выбора и оценка сейчас не запускаются и не задерживают создание следующей партии. Это назначение ролей, не подтверждение выполненного вызова.
<!-- CREATE-ONLY-END -->

**Режим CREATE_ONLY:** создать кандидатные пары, выполнить одну готовую команду структурного `finish` и перейти к следующему stream. Не запускать blind/review/oracle/accept/export/Student и не ждать проверяющего. `DRAFT_CREATED` подтверждает создание файла, а не проверенное качество.

Роль человека при выборе материала выполняет **CODEX с Astra 6 ultracode**; оценку качества DPO также выполняет **CODEX с Astra 6 ultracode**. Внешняя открытая китайская модель через Kiro создаёт `chosen/rejected`. Назначение уже дано карточкой/request; новые вызовы выбора и оценки не выполняются на этой стадии. Текущая модель Kiro наследуется без override; неизвестное имя записывается `unknown`.

## Назначение

Домен `code.angular`, профиль `base`. Это написание TypeScript-кода для явно заданного проектного контракта загрузки конфигурации. Документация того же поведения относится к отдельному `docs.angular`; маршрутизация между задачами обучается отдельно.

Среда: **Angular 16.2.0 / TypeScript 5.1.6**. Используются локальные интерфейсы и Promise-fixture, полностью показанные в prompt. Это не заявление о наличии `provideAppInitializer` в данной версии и не выполненный bootstrap Angular. Контракт host в каждой паре прямо сообщает, какой возвращённый Promise он ожидает.

Текущая узкая цель: [CODE-NG-CONFIG-READY/base](../../curriculum/cards/CODE-NG-CONFIG-READY.md). Полный предметный промпт и разобранные контрасты — [SOFTWARE-PIPELINE-WINDOW-READY-RU](../../prompts/SOFTWARE-PIPELINE-WINDOW-READY-RU.md). Источник карточки — [addition](../../curriculum/additions/software-pipeline-window-ready.json).

## Что создавать

Одна пара содержит короткую функцию или метод и одну границу:

- `ready=true` допускается после фактического завершения загрузки и явно перечисленных зависимостей.
- Initializer возвращает Promise загрузки, а не отдельный немедленно завершённый Promise.
- Getter snapshot с явным guard не читается до `whenReady`.
- Reload сбрасывает готовность, если это прямо требует контракт.
- Ошибка передаётся вызывающему после логирования, когда fallback не разрешён.
- Два потребителя разделяют один `pending` и один вызов fetch, когда назначен single-flight.
- Зависимый auth запускается после config, а не одновременно с ним через Promise.all.

Все поля, сигнатуры, начальные состояния, ошибки и точки наблюдения находятся в prompt. Оба ответа синтаксически допустимы и имеют одинаковый формат; rejected отличается одним главным дефектом. Пояснения, source refs и признаки качества не включаются в обучаемые ответы.

### Короткий образец

В prompt дано: `type Init=()=>Promise<void>; interface Loader { load():Promise<void> }`. Host ждёт возвращённый Promise; load пока pending. Нужно написать `makeInit(s:Loader):Init`.

**Chosen:**

```typescript
function makeInit(s: Loader): Init { return () => s.load(); }
```

**Rejected:**

```typescript
function makeInit(s: Loader): Init {
  return () => { void s.load(); return Promise.resolve(); };
}
```

Простая самопроверка: пока load pending, Promise chosen тоже pending. Promise rejected завершится независимо от load. Оба типа подходят, поэтому различие касается ожидания, а не синтаксиса. Этот образец относится к публичной семье `v7.ref.config.initializer-return`; его DOC-вариант и переименования остаются в той же семье и split.

## Источники и доказательства

Используются только проектные [RAG-027](../../rag/sources/RAG-027.md), [RAG-028](../../rag/sources/RAG-028.md) и [RAG-037](../../rag/sources/RAG-037.md). Они дают правила полного контекста, версионность и проектный договор `load/whenReady/snapshot`. Новые мини-программы — авторские симуляции. `oracle=NOT_RUN`, генерация китайской моделью и Student `NOT_RUN`; ожидаемые значения не являются измеренными.

Checksum, подписи, `profile_verified`, независимый review и production CI не нужны для создания. Содержательные условия одной темы, полной пары и семейной связи сохраняются. Не скачивать весь main и не обращаться к внешним ссылкам из исторических источников.

## Квоты и последующее обучение

План нового родителя: **1000 TRAIN-пар**. Это квота будущего корпуса, не число созданных или принятых пар. Пилот микроцели: 100 TRAIN, 20 DEV, 20 EXAM; пилотный TRAIN входит в родительский бюджет. Первая партия — 10 кандидатов. DEV/EXAM скрыты от TRAIN-генератора; публичные reference не становятся закрытым EXAM.

Последующее обучение использует [05-LEARNING-AND-CHECKPOINTS](../../05-LEARNING-AND-CHECKPOINTS.md): совместимое сохранённое B1, минимальный достаточный набор, реальное обновление Student и до/после. Улучшение любого полезного показателя признаётся успехом данного показателя и домена; ухудшения показываются рядом. `0→0`, только margin/loss или структурная проверка JSON не считаются учебным эффектом.

## Пути, запуск и переход

Корень наборов: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets`. Preparation находится внутри него. Не искать runner в core. Первое новое назначение, если run ещё не существует:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "CODE-NG-CONFIG-READY/base" -Run "S706" -Count 10 -Split train -Start 0
```

Prepare создаёт задание, но не генерирует пары. Прочитай `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/CODE-NG-CONFIG-READY/base/S706/prompt.md` и `request.json`; запиши реальные строки по `request.paths.draft`, затем используй точную команду finish из `commands.md`.

Если эта попытка уже есть:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Action resume -Request "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/CODE-NG-CONFIG-READY/base/S706/request.json"
```

Для начатой кампании продолжай фактический request/state, а не запускай S706 снова. Следующий ID берётся из полной [execution-queue.json](../../execution-queue.json); не из номера старого отчёта.

**NEXT/новый ID:** сразу перейти к нему; запрещено продолжать предыдущую задачу, её отчёт и исправления. **STOP:** немедленно прекратить действия. Старые данные, эксперименты и попытки не удалять. При живом процессе дождаться terminal штатным wait/resume по одному handle в том же ходе: без повторного запуска, частого polling и преждевременного окончания ответа.

Отчёт по-русски: objective/profile, фактическое число строк и семей, абсолютный файл, наблюдаемая модель, `DRAFT_CREATED` и следующий ID. Не выдавать подготовку задания или собственную самопроверку за созданный корпус, независимую оценку Astra или результат обучения.

<!-- GENERATED-QUEUE-START -->
## Переходы единой очереди

Этот блок воспроизводится из общей очереди и задаёт текущий запуск. Прежние команды/номера в основном тексте сохраняются как история. Уже начатую попытку продолжать по её фактическому request/state, не запускать повторно. Прямой ID выше порядка. При NEXT немедленно перейти к названному ID; запрещено продолжать прежнюю задачу или дописывать её отчёт. При STOP немедленно прекратить действия. Живой процесс ждать через его реальный handle до terminal; не завершать раунд вместо ожидания.

Канонический обзор — [EXECUTION-QUEUE.md](../../EXECUTION-QUEUE.md); [бюджет](../../curriculum/scale-plan.json) делится внутри родительского пула, пилот входит в него. Выбранная модель не переопределяется. Checksum, фиксированный профиль модели и production CI не являются условиями подготовки. Режим CREATE_ONLY: запись draft.jsonl → finish → следующий поток; никаких review/oracle/accept/export/обучения.

| Поток | Общий порядок | Следующий поток |
|---|---:|---|
| `CODE-NG-CONFIG-READY/base` | 165 | `TR-EN-IF-01/ru-en` |

## Точные команды действующих потоков

Подготовка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Команды работают из любого каталога. RAG уже внутри пакета. Эта родительская карточка уточнена микроцелями: исполнять их команды ниже, не прибавлять отдельный родительский корпус.

### CODE-NG-CONFIG-READY/base

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "CODE-NG-CONFIG-READY/base" -Run "S706" -Count 10 -Split train -Start 0
```

DEV/EXAM не запускать попутно с TRAIN. Когда назначено отдельное создание этих split в изолированном контексте, точные команды сохранены в `curriculum/pilot-queue.json` как `prepare_first_dev` / `prepare_first_exam`.

Первая TRAIN-папка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/CODE-NG-CONFIG-READY/base/S706`. Следующий поток: `TR-EN-IF-01/ru-en`. Доля TRAIN: 1000 пар; первая партия 10. Пилот TRAIN/DEV/EXAM: 100/20/20; набор `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/datasets/CODE-NG-CONFIG-READY-base-v1`; B1 `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/baselines/code.angular/base/CODE-NG-CONFIG-READY-v1/B1`. Это назначенные пути, не существующие веса.

<!-- GENERATED-QUEUE-END -->
