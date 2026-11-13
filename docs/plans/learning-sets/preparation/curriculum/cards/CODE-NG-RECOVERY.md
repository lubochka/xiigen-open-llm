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
| `A189` — `CODE-NG-RECOVERY` | `code.angular` / `base` | 170 | `A190` |


Следующий ID из таблицы — номинальный порядок своей группы. Фактическую свободную карточку с учётом владельцев выдаёт команда `next` текущего request; не менять группу и чужой статус вручную.

Замечания качества: смотри `quality_flags` своего domain/topic/profile в общем status. `needs_rework` указывает доказанный дефект с evidence, `needs_assessment` — область без достаточной оценки. Это не новый review-гейт и не разрешение исправлять чужую старую попытку; finish не закрывает флаг автоматически. [Правила общей матрицы](../../28-PARALLEL-GENERATION-AND-QUALITY.md). Для нового окна используй [один из трёх готовых стартовых промптов](../../00-START-HERE.md); не исполняй исторические команды из нижней справочной части вместо текущих owner-команд.

<!-- /CURRENT-PARALLEL-CONTRACT-20261026 -->

# CODE-NG-RECOVERY — Реализовать восстановление одного запроса без завершения потока

<!-- DOMAIN-CAMPAIGN-AUTHORITY -->
> **Предметная справка к действующей кампании.** Квоты и продолжение задаёт [campaign/registry.json](../../campaign/registry.json), не старые команды ниже. 1000 пар относятся к домену в сумме. Прочитанный генератором рабочий prompt содержит только назначенную микротему.
>
> Данные: `C:/Projects/llm_mvp_core/learning-sets`. Инструкции: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. CREATE_ONLY: без review, checksum, production CI и обучения. Модель — текущая Kiro без override. Reason internally in English; do not output private reasoning. Отчёт по-русски.
>
> После партии выполнять готовый next до доменной квоты; не останавливаться с вопросом «продолжать?». При прямом NEXT/новом ID немедленно сменить задачу: запрещено дописывать/чинить предыдущую. STOP прекращает действия. Один живой handle ждать документированным wait/resume; не объявлять молчание падением. Старые попытки сохранять; выбранный request возобновлять, не пересоздавать.

Точные доли этой предметной карточки в текущей кампании:
| A-ID | Домен | Профиль | Доля из доменных1000 | Следующий A-ID после доли |
|---|---|---|---:|---|
| A189 | code.angular | base | 170 | A190 |


Ниже — сохранённая предметная методика и исторические инструкции. [Полный прежний вариант](../../history/before-domain-campaign-2026-10-25/curriculum/cards/CODE-NG-RECOVERY.md). Старые числовые планы и команды в нём не запускаются вместо актуального request.
<!-- /DOMAIN-CAMPAIGN-AUTHORITY -->

> Новая подготовка: `tools/batches.py` по [действующему контракту](../../17-CURRENT-PREPARATION-CONTRACT.md) и точной команде в конце карточки. RAG уже в llm_plan3. Обязательные checksum, fixed-profile receipts и production gates не применяются. Старые `tools/sets.py` и пути в основном тексте — история прежнего runner; не запускать их поверх нового request. При NEXT/новом ID немедленно сменить задачу, запрещено продолжать прежнюю или её отчёт; при STOP прекратить действия.

> Для нового запуска используйте раздел «Точные команды действующих потоков» в конце этой карточки: он воспроизводит полную execution-queue.json. Примеры старых run в основном тексте сохраняются как история/иллюстрация. Если работа уже начата, продолжайте её фактический request/state; этот блок не разрешает повторную генерацию или замену существующих файлов.

Родительская тема: `CODE-NG-02`. Домен: `code.angular`. Профиль: `base`. Это дополнение к существующей теме: прежние карточки и результаты сохраняются.

## Обучаемая граница

Изолировать error каждого inner, чтобы модель сохраняла обработку следующих запросов.

## Источники и смысл примеров

Источники: `RAG-027`, `RAG-037`, `RAG-039`. Точные commit/blob и локальные выдержки — [sources.lock.json](../../sources.lock.json) и RAG main. Они поддерживают методику и проектный контекст; описанные здесь мини-программы — авторские самодостаточные симуляции. В них нет измеренных результатов обучения. Число reference-строк не равно числу независимых семейств.

## Запуск подготовки

Действующая команда и точный каталог находятся в разделе «Точные команды действующих потоков» в конце карточки. Подготовка — `tools/batches.py`, RAG встроен в llm_plan3. Каталог содержит request, prompt, commands, state и шаблон отчёта; следующий шаг определяется реальным состоянием попытки. Старые инструкции этой секции сохранены в history/legacy-card-launch-sections.jsonl и не выполняются поверх нового request.

## Промпт генератору

```text
Один навык — область catchError внутри уже заданного switchMap. Outer query$ не ошибается; ошибки только в load(q). Каждый сценарий содержит как минимум ошибку и следующий успешный запрос. Fallback тип согласован с Observable<string>: EMPTY либо of("fallback"). Rejected переносит тот же catchError на outer; других изменений нет. Не добавляй retry, shareReplay, debounce и новый subscribe. Генерируй DOC и CODE отдельно с общими source-family, не в одном учебном пакете.

Работай с одной назначенной микроцелью, одним domain_id/profile_id и точными family_id/split/count из request.json. Все исходные данные, версии, видимый код и ограничения ответа находятся в prompt; metadata ученик не получает. Chosen и rejected имеют одинаковый формат, а ошибочная сторона нарушает одну проверяемую границу. Не добавляй [WRONG], score, rationale или source ID в обучаемые ответы. Источники задают методику; авторские симуляции не выдавать за цитаты реального проекта, результаты китайской модели либо измеренный эффект обучения. Публичные reference-примеры и их потомки не помещать в DEV/EXAM. Семейство одного исходника общее для документации, кода и перефразирования; переименование и новые числа не создают независимости. Для новых пар сначала назначить происхождение и split; не наполнять квоту поверхностными дубликатами. Пиши отчёт на русском по 08-REPORT-TEMPLATE.md. Используй фактическую текущую модель Kiro без override, не угадывай имя/версию. Если точная модель не показана, сохраняй наблюдаемый selector и model_identity_observed=false. При явном NEXT сразу переходи к указанному пользователем ID; не возвращайся к прежней карточке. При STOP немедленно прекрати новые действия. Старые эксперименты и версии данных не удаляй; production не меняй. Один долгий процесс имеет один handle: обнаружь штатный wait/resume и дождись terminal в текущем раунде; не запускай заново вместо ожидания, не делай busy polling и не объявляй таймаут по отсутствию первых файлов. Oracle до фактического исполнения имеет NOT_RUN, result_observed=false. Runtime oracle сейчас NOT_RUN; это не препятствует записи кандидатов и не означает PASS. Учебный пилот позже восстанавливает совместимое обученное B1 с tokenizer/optimizer/RNG/split hashes; новое обучение B1 только по назначенному отдельному bootstrap-плану, с сохранением для повторного использования. 0 правильных до и после не доказывает обучение; loss/margin и число пар не заменяют полезный результат. Любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост точности не обязателен, изменения остальных показателей, включая ухудшения, показать рядом. Успешный опыт документируется кандидатом в исправления; перенос в production не выполняется автоматически.
```

## Договор ответа

DOC: JSON-массив next emissions. CODE: одно Observable-выражение.

## Оси генерации

- **fallback:** EMPTY; of fallback
- **последовательность:** error→success; success→error→success; error→error→success

## Не смешивать с соседними темами

- retry
- outer error handling
- новая подписка после каждого error
- изменение switchMap на другой flattening operator
- полный компонент/HTTP interceptor

## Проверка смысла

- Virtual TestScheduler, no network
- После error дать новый success и проверить load вызван
- Одинаковый fallback в обоих плечах
- Chosen сохраняет future queries; rejected прекращает их
- Не называть отсутствие exceptions признаком сохранённого обучения

Oracle создаёт reviewer в подкаталоге `oracle/` этого run, с точными fixture из каждой принятой строки. Это следующий шаг подготовки; готового native oracle-run результата здесь нет. Harness не получает error_tag/chosen-label как вход бизнес-функции. Compiler PASS и ручное рассуждение не заменяют runtime. Для CODE исполняются две реализации при одинаковых входах; для DOC исполняется единственный показанный в prompt код, затем обе текстовые/JSON-гипотезы сверяются с его trace. Нельзя выдавать описание ошибочного кода за исходную программу или компилировать русский текст вместо наблюдения. Для каждого executable candidate свежая среда.

### RxJS fixture: точный состав и команда

Создать `oracle/verify.cjs`. Перед запуском обнаружить фактический абсолютный путь к package.json уже установленного RxJS 7.8.0 и записать его в `XIIGEN_RXJS_PACKAGE_JSON`; эта переменная не считается заранее настроенной. Script валидирует абсолютность, наличие файла и точное поле version=7.8.0. Загрузка зависимостей выполняется через `require("node:module").createRequire(process.env.XIIGEN_RXJS_PACKAGE_JSON)`, затем этим привязанным require загрузить `rxjs` и `rxjs/testing`; обычный require из C:/x/lsets не обещает найти пакет в другом repo. Отсутствие наблюдённого пути/пакета даёт ORACLE_PENDING, не попытку поставить latest. `node:assert/strict` загружается штатно. Версию и фактический package path записать в results. Script принимает обязательный абсолютный `--out`; относительный cwd не определяет место результата. TestScheduler создаёт outer/inner из metadata.oracle.timeline и сравнивает next/subscriptions. CODE исполняет оба выражения отдельными scheduler; DOC исполняет показанное выражение и сравнивает оба заявленных trace с одним наблюдением. Не использовать setTimeout/сеть.

```powershell
node C:/x/lsets/CODE-NG-02/base/D312/oracle/verify.cjs --out C:/x/lsets/CODE-NG-02/base/D312/oracle/results.json
```

## Полные публичные контрасты

### CODE-NG-RECOVERY.REF.01

Общее семейство источника: `v3.ref.ng.catch.inner-empty`.

**Условие:**

```text
Авторская симуляция. Angular 16 / RxJS 7.8. query$: Observable<string>; load(q: string): Observable<string>. Imports switchMap,catchError,of,EMPTY доступны. TestScheduler использует virtual frames, сеть отсутствует. query$ выдаёт A в frame 0, B в frame 4, complete в frame 10. load(A) errors через 1 frame; load(B) выдаёт b через 1 frame, complete через 2. Recovery заменяет ошибку каждого inner на EMPTY. Напиши одно выражение Observable. Ошибка A не должна завершать поток будущих запросов. Ошибочный inner не выдаёт fallback; B должен быть обработан. switchMap уже выбран. Меняй только место catchError: он должен обрабатывать inner. Ответ — выражение без Markdown.
```

**Предпочтительный ответ:**

```text
query$.pipe(switchMap(q => load(q).pipe(catchError(() => EMPTY))))
```

**Правдоподобный ошибочный ответ:**

```text
query$.pipe(switchMap(q => load(q)), catchError(() => EMPTY))
```

**Почему различаются:** Перехват внутри switchMap завершает заменой только ошибочный inner. Внешний catchError заменяет весь source pipeline, поэтому последующие q больше не обрабатываются.

**Контрпример:** После первого error нужен минимум один новый успешный запрос; иначе внутренний и внешний catch могут выглядеть одинаково.

### CODE-NG-RECOVERY.REF.02

Общее семейство источника: `v3.ref.ng.catch.inner-fallback`.

**Условие:**

```text
Авторская симуляция. Angular 16 / RxJS 7.8. query$: Observable<string>; load(q: string): Observable<string>. Imports switchMap,catchError,of,EMPTY доступны. TestScheduler использует virtual frames, сеть отсутствует. query$ выдаёт A в frame 0, B в frame 4, complete в frame 10. load(A) errors через 1 frame; load(B) выдаёт b через 1 frame, complete через 2. Recovery для каждого ошибочного inner — of("fallback"). Напиши одно выражение Observable. При ошибке дать fallback один раз и продолжить обработку следующих q; не завершать весь query pipeline. switchMap уже выбран. Меняй только место catchError: он должен обрабатывать inner. Ответ — выражение без Markdown.
```

**Предпочтительный ответ:**

```text
query$.pipe(switchMap(q => load(q).pipe(catchError(() => of("fallback")))))
```

**Правдоподобный ошибочный ответ:**

```text
query$.pipe(switchMap(q => load(q)), catchError(() => of("fallback")))
```

**Почему различаются:** Перехват внутри switchMap завершает заменой только ошибочный inner. Внешний catchError заменяет весь source pipeline, поэтому последующие q больше не обрабатываются.

**Контрпример:** После первого error нужен минимум один новый успешный запрос; иначе внутренний и внешний catch могут выглядеть одинаково.

### CODE-NG-RECOVERY.REF.03

Общее семейство источника: `v3.ref.ng.catch.success-then-error-then-success`.

**Условие:**

```text
Авторская симуляция. Angular 16 / RxJS 7.8. query$: Observable<string>; load(q: string): Observable<string>. Imports switchMap,catchError,of,EMPTY доступны. TestScheduler использует virtual frames, сеть отсутствует. query$ выдаёт A в frame 0, B в frame 4, C в frame 8, complete в frame 12. load(A) выдаёт a через 1 frame, complete через 2; load(B) errors через 1 frame; load(C) выдаёт c через 1 frame, complete через 2. Recovery — of("fallback"). Напиши одно выражение Observable. Сохранить уже доставленный a; ошибку B заменить fallback; после этого C должен дать c. switchMap уже выбран. Меняй только место catchError: он должен обрабатывать inner. Ответ — выражение без Markdown.
```

**Предпочтительный ответ:**

```text
query$.pipe(switchMap(q => load(q).pipe(catchError(() => of("fallback")))))
```

**Правдоподобный ошибочный ответ:**

```text
query$.pipe(switchMap(q => load(q)), catchError(() => of("fallback")))
```

**Почему различаются:** Перехват внутри switchMap завершает заменой только ошибочный inner. Внешний catchError заменяет весь source pipeline, поэтому последующие q больше не обрабатываются.

**Контрпример:** После первого error нужен минимум один новый успешный запрос; иначе внутренний и внешний catch могут выглядеть одинаково.

### CODE-NG-RECOVERY.REF.04

Общее семейство источника: `v3.ref.ng.catch.each-query-isolated`.

**Условие:**

```text
Авторская симуляция. Angular 16 / RxJS 7.8. query$: Observable<string>; load(q: string): Observable<string>. Imports switchMap,catchError,of,EMPTY доступны. TestScheduler использует virtual frames, сеть отсутствует. query$ выдаёт A в frame 0, B в frame 4, C в frame 8, complete в frame 12. load(A) и load(B) errors через 1 frame после соответствующей подписки. load(C) выдаёт c через 1 frame, complete через 2. Recovery — of("fallback"). Напиши одно выражение Observable. Для каждого ошибочного запроса отдельный fallback, затем продолжение новых q. Никакого retry старого запроса. switchMap уже выбран. Меняй только место catchError: он должен обрабатывать inner. Ответ — выражение без Markdown.
```

**Предпочтительный ответ:**

```text
query$.pipe(switchMap(q => load(q).pipe(catchError(() => of("fallback")))))
```

**Правдоподобный ошибочный ответ:**

```text
query$.pipe(switchMap(q => load(q)), catchError(() => of("fallback")))
```

**Почему различаются:** Перехват внутри switchMap завершает заменой только ошибочный inner. Внешний catchError заменяет весь source pipeline, поэтому последующие q больше не обрабатываются.

**Контрпример:** После первого error нужен минимум один новый успешный запрос; иначе внутренний и внешний catch могут выглядеть одинаково.

## Учебный пилот и переход дальше

Подготовка здесь не запускает обучение. Последующий пилот выполняется по [05-LEARNING-AND-CHECKPOINTS.md](../../05-LEARNING-AND-CHECKPOINTS.md) и [10-NATIVE-CONTRACT-BOUNDARY.md](../../10-NATIVE-CONTRACT-BOUNDARY.md). Получатель, training/inference entrypoints, данные и hashes состояния задаются binding; не выдумывать runtime по domain ID. Восстановить совместимое ненулевое B1; один исходный checkpoint для обоих плеч. DEV-калибровка и бюджет должны обеспечить действительное обучение; 0→0 не является доказательством. Измерять полезную способность, время урока/число обновлений до неё, удержание старого навыка и ресурсы участвующего пути. Успех одного показателя допускается с регрессией другого, оба показываются. Не переносить изменения в production по этому документу.

Канонический порядок — `curriculum/index.json:stream_order`, следующая запись определяется по objective_id/profile_id, не по номеру родительской темы. Прямой ID пользователя имеет приоритет. При NEXT перейти немедленно и не дописывать прошлый эксперимент; при STOP остановить новые действия. Старые файлы сохраняются. Не завершать рабочий раунд вместо штатного ожидания действующего процесса. Отчёт на русском, фактическая текущая Kiro-модель.

<!-- GENERATED-QUEUE-START -->
## Переходы единой очереди

Этот блок воспроизводится из общей очереди и задаёт текущий запуск. Прежние команды/номера в основном тексте сохраняются как история. Уже начатую попытку продолжать по её фактическому request/state, не запускать повторно. Прямой ID выше порядка. При NEXT немедленно перейти к названному ID; запрещено продолжать прежнюю задачу или дописывать её отчёт. При STOP немедленно прекратить действия. Живой процесс ждать через его реальный handle до terminal; не завершать раунд вместо ожидания.

Канонический обзор — [EXECUTION-QUEUE.md](../../EXECUTION-QUEUE.md); [бюджет](../../curriculum/scale-plan.json) делится внутри родительского пула, пилот входит в него. Выбранная модель не переопределяется. Checksum, фиксированный профиль модели и production CI не являются условиями подготовки. Режим CREATE_ONLY: запись draft.jsonl → finish → следующий поток; никаких review/oracle/accept/export/обучения.

| Поток | Общий порядок | Следующий поток |
|---|---:|---|
| `CODE-NG-RECOVERY/base` | 98 | `OR-FUNC-ACCEPTANCE/base` |

## Точные команды действующих потоков

Подготовка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Команды работают из любого каталога. RAG уже внутри пакета. Выполнить только назначенный поток; остальные строки — точные альтернативы назначения.

### CODE-NG-RECOVERY/base

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "CODE-NG-RECOVERY/base" -Run "D312" -Count 10 -Split train -Start 0
```

DEV/EXAM не запускать попутно с TRAIN. Когда назначено отдельное создание этих split в изолированном контексте, точные команды сохранены в `curriculum/pilot-queue.json` как `prepare_first_dev` / `prepare_first_exam`.

Первая TRAIN-папка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/CODE-NG-RECOVERY/base/D312`. Следующий поток: `OR-FUNC-ACCEPTANCE/base`. Доля TRAIN: 500 пар; первая партия 10. Пилот TRAIN/DEV/EXAM: 100/20/20; набор `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/datasets/CODE-NG-RECOVERY-base-v1`; B1 `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/baselines/code.angular/base/CODE-NG-RECOVERY-v1/B1`. Это назначенные пути, не существующие веса.

<!-- GENERATED-QUEUE-END -->
