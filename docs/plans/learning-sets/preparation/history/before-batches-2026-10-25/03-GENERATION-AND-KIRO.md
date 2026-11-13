# Как запустить подготовку в Kiro

## Новые задания с objective_id

Сначала выбрать точный поток в [единой очереди](EXECUTION-QUEUE.md), machine-readable источник — `execution-queue.json`. Для objective-потока используются [план микронаборов](11-MICROTOPICS-AND-BATCH-PLAN.md) и [curriculum](curriculum/INDEX.md). Команда `prepare` дополняется `--objective <ID>`; она включает узкую карточку вместо полного набора соседних навыков. Конкретные команды и пути каждого профиля записаны в `curriculum/pilot-queue.json`. Примеры роста исходного TR01 ниже остаются действительными для topic-only workflow, но не определяют следующий запуск объединённого пакета. Прямой ID пользователя имеет приоритет; покрытый objective-потоком parent/profile не запускается повторно по умолчанию.

Обычная начальная партия микронабора — 10 строк; TR10-CONDITIONAL — 5 семей × 2 строки; OR-ROUTE-INTENT — 21 строка, по одной на каждый класс. Последующий --start читается из предыдущего request того же objective/profile/split. Даже при одинаковом count он не равен числу уже полученных строк.

Для текущего `validate --blind --review --key --selection --review-receipt` нужны оба явных needs_* boolean, фактические предметные доказательства по review-промпту и квитанции реального CODEX selection/review по назначенному профилю. Старый ключ без blind_sha256 и старый review без доказательств не считать завершённой новой проверкой; сохранить их и сделать новый blind/review рядом. В accepted файлы не переносить demonstration/PENDING evidence.

## Папки и модели

Рабочая копия планов: `C:/x/xp`, ветка `llm_plan3`. RAG читается из второй рабочей копии `C:/x/xp-rag`, ветка `main`. Это назначенные короткие пути для этой программы; если существующий checkout расположен иначе, открыть в нём именно каталог `llm mvp/learning-sets/preparation`, не переносить историю экспериментов. Незакоммиченные изменения существующих копий не сбрасывать. Путь RAG задаёт `--rag-root`, путь результатов — `--work-root`; ни один из них не меняет checkout планов.

Данные и отчёты: `C:/x/lsets/<topic>/<profile>/<run>/`. Принятые непересекающиеся части корпуса (shards): `C:/x/lsets/releases/<shard-id>/`. Накопительные версии набора: `C:/x/lsets/datasets/<dataset-version>/`. Учебные состояния: см. [05](05-LEARNING-AND-CHECKPOINTS.md). Идентификаторы короткие ASCII, без пробелов и вложенных разделителей. Linux/macOS: явный `--work-root /tmp/xiigen-lsets` для временной проверки либо назначенное постоянное хранилище проекта.

Генератор использует текущую выбранную открытую китайскую модель Kiro без переопределения. Выбор материала и оценку качества выполняет CODEX с `astra 6 ultracode` по [10](10-CODEX-AND-CHINESE-ROLES.md). Если сессия открыта с не выбранной пользователем китайской моделью, не выдавать генерацию за китайскую: подготовить пакет, зафиксировать фактическую модель и дождаться выбранной среды в рамках исполнения задания. Утилита не вызывает провайдера, не требует API-ключей, не устанавливает модель. CLI Kiro намеренно не выдумывается: prompt открывается в существующей Kiro-сессии.

После `prepare` CODEX читает `selection.prompt.md` и сохраняет `selection.json` до генерации. После `blind` CODEX получает `blind.review.prompt.md` и blind без ключа, затем сохраняет `review.jsonl` и `review.receipt.json`. Шаблоны квитанций создаются со статусом PENDING: их нельзя объявить доказательством вызова. Поля и роль — в [10](10-CODEX-AND-CHINESE-ROLES.md).

## Проверка контекста перед генерацией

До выбора `--count` прочитать фактический идентификатор выбранной Kiro-модели и доступные в этой среде ограничения входа/выхода. Оценить полный запрос: карточку, назначенные RAG-фрагменты, общий контракт, семейный план, доступный объём истории сессии и ожидаемые полные пары JSONL. Использовать счётчик токенов модели/провайдера, если он доступен; оценку по символам так и назвать оценкой, не выдавать её за точное число токенов. Оставить место для полного ответа, а не только для входа.

Сохранить фактическую модель, известные лимиты, способ оценки и выбранный размер пакета в `report.md` текущего run; в каждой паре `provenance.generator_model` содержит фактический ID. Не подставлять название китайской модели по предположению и не менять модель ради более длинного контекста. Если лимит среды не раскрыт, записать эту неопределённость и уменьшить пакет до небольшой проверяемой порции до генерации.

Сначала уменьшать число пар и объём новых сценариев в пакете: для обычного независимого topic-only пакета, например, `--count 5` вместо 25; для objective сохранять кратность и классовые ограничения назначенной карточки. Все обязательные источники, условия, версии, callee-контракты и семейные связи остаются в запросе. Перед отправкой проверить уже собранный `prompt.md`: молча обрезать его, заменять назначенные источники непроверенным пересказом или досочинять недостающие факты запрещено. Если даже минимальный пакет не помещается, сохранить подготовленный run со статусом `CONTEXT_NOT_FIT`; требуется явное разбиение темы/источников, без генерации повреждённых пар. Не исправлять существующий `request.json` задним числом: новое назначение получает новый run и ссылку на причину переноса, исходные файлы сохраняются.

## Исторический пример topic-only пакета TR01 (не начало текущей очереди)

Из каталога подготовки:

```powershell
python tools/sets.py list
python tools/sets.py prepare --topic TR01 --profile ru-en --run R001 --count 25 --split train --start 0 --work-root C:/x/lsets
```

Команда создаёт `C:/x/lsets/TR01/ru-en/R001/prompt.md`, `request.json`, `state.json` и шаблон выбора. Сначала CODEX с `astra 6 ultracode` выполняет `selection.prompt.md` и сохраняет реальный `selection.json` для этого request; затем открыть `prompt.md` в Kiro и поручить генератору выполнить его. Прочитать назначенные RAG-фрагменты; утилита умеет включить их из `--rag-root "C:/x/xp-rag/llm mvp/learning-sets/preparation"`. Не загружать все документы и историю экспериментов в каждый запрос.

Модель сохраняет ровно 25 строк в `draft.jsonl` этого же каталога. Для нового пакета `R002` взять `--start` строго из `request.json.next_start` предыдущего пакета (число 25 не подставлять: пропущенные по split номера делают next_start больше count); для восстановления прерванного — читать существующий `state.json` и `request.json`, не создавать новую генерацию поверх старой. Имя R001 в карточке — первый запуск; повтор с тем же существующим каталогом отказывает без удаления данных. Следующий свободный ID выбирается явно из журнала, без автоматического скрытого restart.

```powershell
python tools/sets.py validate --input C:/x/lsets/TR01/ru-en/R001/draft.jsonl --request C:/x/lsets/TR01/ru-en/R001/request.json
python tools/sets.py blind --input C:/x/lsets/TR01/ru-en/R001/draft.jsonl --out C:/x/lsets/TR01/ru-en/R001/blind.jsonl
```

`blind` создаёт также закрытый от проверяющего `blind.key.json`; проверяющий получает только `blind.jsonl`, [промпт проверки](prompts/REVIEW-PROMPT-RU.md) и свидетельства из prompt. Он пишет `review.jsonl`: `pair_id`, `winner` (`A`, `B` или `tie`), `valid`, `reason`, `checks` (семантика/условие/версия/единственная ошибка/семейство). Проверяющий — CODEX с `astra 6 ultracode`; самопроверка китайской модели остаётся черновой. Для перевода требуется связанный языковой review; для code.* — исполняемый oracle в изолированной среде. Для docs.* также обязателен предметный evidence: execution при зависимости от поведения/API/SQL/Angular, source_grounded только для неисполняемого факта источника. Точная схема, статусы и хеши заданы в [REVIEW](prompts/REVIEW-PROMPT-RU.md). Подготовка reference и draft с NOT_RUN допускается; смысловая приёмка до необходимой проверки не назначается.

```powershell
python tools/sets.py validate --input C:/x/lsets/TR01/ru-en/R001/draft.jsonl --request C:/x/lsets/TR01/ru-en/R001/request.json --blind C:/x/lsets/TR01/ru-en/R001/blind.jsonl --review C:/x/lsets/TR01/ru-en/R001/review.jsonl --key C:/x/lsets/TR01/ru-en/R001/blind.key.json --selection C:/x/lsets/TR01/ru-en/R001/selection.json --review-receipt C:/x/lsets/TR01/ru-en/R001/review.receipt.json
```

Результат `validation.json` описывает только качество/полноту данных. Не создавать фальшивую метрику обучения. При rejected/ambiguous результатах сохраняются исходный draft и `report.md`; исправления — `draft.v2.jsonl` и новый review с хешами, согласно [промпту ремонта](prompts/REPAIR-PROMPT-RU.md). Никаких бесконечных автоперегенераций: один ремонтный пакет после разбора категории дефектов; если проблема в контракте — остановить данную тему с точным конфликтом, продолжить другую разрешённую тему только по реестру или указанию пользователя.

### Ремонт полного пакета

`draft.v2.jsonl` содержит весь исходный пакет: неизменённые строки остаются без изменений и с прежним pair_id; только исправленные получают новый pair_id и `metadata.derived_from`. Число строк, назначенные family_id, origin/leakage связи и split сохраняются. Исходный request не редактируется. Нельзя отправить только исправленные строки под исходным request: валидатор требует полное покрытие count и family_ids. Если исправление отдельной строки пока невозможно, весь пакет остаётся непринятым; новый состав назначается отдельно.

Для того же исторического R001 конкретные команды:

```powershell
python tools/sets.py validate --input C:/x/lsets/TR01/ru-en/R001/draft.v2.jsonl --request C:/x/lsets/TR01/ru-en/R001/request.json
python tools/sets.py blind --input C:/x/lsets/TR01/ru-en/R001/draft.v2.jsonl --out C:/x/lsets/TR01/ru-en/R001/blind.v2.jsonl
```

Проверяющий получает новый `blind.v2.jsonl` и пишет `review.v2.jsonl` **для всех строк**, включая неизменённые. CODEX выполняет review по [10](10-CODEX-AND-CHINESE-ROLES.md) и сохраняет новую `review.v2.receipt.json`, связанную с input, blind и review v2. Исходный selection остаётся допустимым только при неизменном request. Новые языковые/oracle-доказательства привязываются к его `blind_sha256`. Старые ключ/review не подходят к новым байтам input. Только после этого:

```powershell
python tools/sets.py validate --input C:/x/lsets/TR01/ru-en/R001/draft.v2.jsonl --request C:/x/lsets/TR01/ru-en/R001/request.json --blind C:/x/lsets/TR01/ru-en/R001/blind.v2.jsonl --review C:/x/lsets/TR01/ru-en/R001/review.v2.jsonl --key C:/x/lsets/TR01/ru-en/R001/blind.v2.key.json --selection C:/x/lsets/TR01/ru-en/R001/selection.json --review-receipt C:/x/lsets/TR01/ru-en/R001/review.v2.receipt.json
```

Отчёт этой версии — `draft.v2.validation.json`; исходные артефакты сохраняются. Для текущего objective использовать ровно его `first_output_directory` из очереди или сохранённый `request.json.output_directory` текущего run; имена v2-файлов те же. В `audit`/`merge` передавать принятый полный `draft.v2.jsonl`, а не вместе с его старой версией.

## Большие объёмы

1. Для текущего objective/profile брать размер первой партии и TRAIN/DEV/EXAM-квоты из `curriculum/pilot-queue.json`; для parent-only — строку `execution-queue.json`. После 50 принятых строк пересмотреть состав; при требовании полных групп брать первый допустимый размер не меньше 50 (для router — 63), не разрывать группу ради круглой суммы.
2. Продолжить непересекающимися shards до назначенного пилота; DEV/EXAM имеют отдельные семьи, назначенные до обучения. Обычный objective начинает со 100 TRAIN, router — со 126. Это не гарантия достаточного учебного бюджета; он определяется на TRAIN/DEV до закрытого EXAM.
3. После валидного урока расширять до доли потока в `curriculum/scale-plan.json`. Общие 128 000 — бюджет родительских пулов, из которого выделяются дочерние цели, а не прибавка к ним. Более крупный выпуск требует нового явного плана; не увеличивать квоту автоматически.
4. Один пакет остаётся маленьким. 25 строк ниже — исторический topic-only пример TR01, не общая квота objective. При уменьшении партии до генерации сохранить кратность `pairs_per_family` и полный набор router-классов; count 5 недопустим для conditional/router. Число принятых строк берётся из фактического release, а не из назначения.
5. `audit` проверяет family/ID/текстовые дубликаты по каталогу всех принятых shards этой библиотеки и **только новым** входным файлам. `merge` принимает эти новые пары одной темы/профиля в новый непересекающийся shard и регистрирует его в catalog. Старые принятые строки повторно через `merge` не передавать.
6. `compose` собирает накопительный снимок из явно перечисленных зарегистрированных shards одной темы/профиля: проверяет хеши и дубликаты, пишет `accepted.jsonl` и `release.json`. Снимок не добавляется в corpus catalog: это новая сборка уже учтённых строк. Старые shards, snapshots, reviews и неудачные draft сохраняются.

### Первые 50 строк

После первого пакета из предыдущего раздела подготовить второй. `next_start` читается из реального назначения, поскольку отбор по split пропускает часть номеров:

```powershell
$tr01NextStart = (Get-Content -Raw C:/x/lsets/TR01/ru-en/R001/request.json | ConvertFrom-Json).next_start
python tools/sets.py prepare --topic TR01 --profile ru-en --run R002 --count 25 --split train --start $tr01NextStart --work-root C:/x/lsets
```

Выполнить prompt, структурную проверку, `blind`, содержательный review и итоговый `validate --blind --review --key --selection --review-receipt` для R002 тем же способом, что для R001; все его файлы находятся в `C:/x/lsets/TR01/ru-en/R002/`. Следующие команды выполняются только после приёмки обоих пакетов. При ремонте указывать фактически принятые `draft.v2.jsonl` и их проверку, не исходный отклонённый draft.

```powershell
# init-catalog — один раз при создании новой библиотеки, существующий файл не перезаписывать
python tools/sets.py init-catalog --path C:/x/lsets/corpus-catalog.json
python tools/sets.py audit --catalog C:/x/lsets/corpus-catalog.json --inputs C:/x/lsets/TR01/ru-en/R001/draft.jsonl C:/x/lsets/TR01/ru-en/R002/draft.jsonl --out C:/x/lsets/TR01-ru-en-S001-audit.json
python tools/sets.py merge --inputs C:/x/lsets/TR01/ru-en/R001/draft.jsonl C:/x/lsets/TR01/ru-en/R002/draft.jsonl --audit C:/x/lsets/TR01-ru-en-S001-audit.json --out C:/x/lsets/releases/TR01-ru-en-S001/accepted.jsonl
python tools/sets.py compose --catalog C:/x/lsets/corpus-catalog.json --inputs C:/x/lsets/releases/TR01-ru-en-S001/accepted.jsonl --out C:/x/lsets/datasets/TR01-ru-en-v1/accepted.jsonl
python tools/sets.py export --input C:/x/lsets/datasets/TR01-ru-en-v1/accepted.jsonl --out C:/x/lsets/datasets/TR01-ru-en-v1/export
```

`merge` требует положительный `validation.json` с review для каждого файла и проверяет его SHA-256. Структурно валидный или просто названный accepted файл этого не обходит. Готовый shard снабжается `release.json`, хешами, счётчиками по семействам/ошибкам/профилям и ссылками на reviewer records. `export` одинаково принимает зарегистрированный shard либо снимок от `compose`; оба являются interchange-данными, не доказательством обучения.

### Продолжение до 100 строк

R003 добавляет только 25 назначенных строк: после его успешной приёмки общий объём мог бы стать 75, а не 1000. Для снимка из 100 строк здесь предусмотрены R003 **и** R004, по 25 новых строк каждый:

```powershell
$tr01NextStart = (Get-Content -Raw C:/x/lsets/TR01/ru-en/R002/request.json | ConvertFrom-Json).next_start
python tools/sets.py prepare --topic TR01 --profile ru-en --run R003 --count 25 --split train --start $tr01NextStart --work-root C:/x/lsets
$tr01NextStart = (Get-Content -Raw C:/x/lsets/TR01/ru-en/R003/request.json | ConvertFrom-Json).next_start
python tools/sets.py prepare --topic TR01 --profile ru-en --run R004 --count 25 --split train --start $tr01NextStart --work-root C:/x/lsets
```

После генерации и полной приёмки R003/R004 в их собственных каталогах:

```powershell
python tools/sets.py audit --catalog C:/x/lsets/corpus-catalog.json --inputs C:/x/lsets/TR01/ru-en/R003/draft.jsonl C:/x/lsets/TR01/ru-en/R004/draft.jsonl --out C:/x/lsets/TR01-ru-en-S002-audit.json
python tools/sets.py merge --inputs C:/x/lsets/TR01/ru-en/R003/draft.jsonl C:/x/lsets/TR01/ru-en/R004/draft.jsonl --audit C:/x/lsets/TR01-ru-en-S002-audit.json --out C:/x/lsets/releases/TR01-ru-en-S002/accepted.jsonl
python tools/sets.py compose --catalog C:/x/lsets/corpus-catalog.json --inputs C:/x/lsets/releases/TR01-ru-en-S001/accepted.jsonl C:/x/lsets/releases/TR01-ru-en-S002/accepted.jsonl --out C:/x/lsets/datasets/TR01-ru-en-v2/accepted.jsonl
python tools/sets.py export --input C:/x/lsets/datasets/TR01-ru-en-v2/accepted.jsonl --out C:/x/lsets/datasets/TR01-ru-en-v2/export
```

В новый audit передаются только R003/R004: S001 уже читается через catalog. В `compose` передаются S001/S002: файл снимка v1 не является новым shard и повторно не добавляется. Имена v1/v2 обозначают версии; количество строк подтверждается их манифестами.

### Цель 1000 строк после учебного пилота

Если каждый пакет реально содержит 25 принятых строк, R005–R040 дают ещё 900 строк. При объединении по два пакета получатся S003–S020 по 50 строк. Каждый следующий `prepare` получает `next_start` предыдущего request; каждый shard проходит новый `audit` и `merge`. Если контекст или ремонт изменили число принятых строк, пересчитать план по фактическим counts. Не объявлять эту цель выполненной по номеру run.

Только после действительного появления всех перечисленных shards и подтверждения 1000 строк собрать новую версию. Явный список не включает случайные соседние результаты или старые snapshots:

```powershell
$tr01ShardIds = @('S001','S002','S003','S004','S005','S006','S007','S008','S009','S010','S011','S012','S013','S014','S015','S016','S017','S018','S019','S020')
$tr01ShardPaths = $tr01ShardIds | ForEach-Object { "C:/x/lsets/releases/TR01-ru-en-$_/accepted.jsonl" }
$tr01AcceptedCount = ($tr01ShardIds | ForEach-Object { (Get-Content -Raw "C:/x/lsets/releases/TR01-ru-en-$_/release.json" | ConvertFrom-Json).rows } | Measure-Object -Sum).Sum
if ($tr01AcceptedCount -ne 1000) { throw "Ожидалось 1000 принятых строк; найдено $tr01AcceptedCount. Уточните список shards по фактическим результатам." }
python tools/sets.py compose --catalog C:/x/lsets/corpus-catalog.json --inputs $tr01ShardPaths --out C:/x/lsets/datasets/TR01-ru-en-v3/accepted.jsonl
python tools/sets.py export --input C:/x/lsets/datasets/TR01-ru-en-v3/accepted.jsonl --out C:/x/lsets/datasets/TR01-ru-en-v3/export
```

Этот пример считает TRAIN для TR01/ru-en. DEV/EXAM требуют собственных семейных назначений и приёмки; их число и список фиксируются отдельно. Добавлять их в общий снимок можно через их зарегистрированные shards той же темы/профиля, сохраняя split. До первого измерения зафиксировать окончательный список: он не меняется вслед за результатами EXAM.

## Ожидание и управление задачей

До долгого запуска прочитать схему фактически доступного инструмента процесса и способ wait/resume. Записать provider, tool, handle, timeout semantics и terminal marker в `state.json`. Запустить один раз; `running`, тишина stdout и истёкший yield не означают отказ. Использовать штатное блокирующее/event ожидание того же handle с интервалом до 60 секунд. Не дублировать запуск, не создавать фоновые циклы опроса, второй monitor или полезную работу вместо ожидания. Ошибка аргументов — не состоявшееся ожидание; исправить binding по каталогу tools. Не завершать раунд словами «жду», пока исходная операция выполняется и штатное продолжение доступно.

Завершение wrapper не доказывает завершение дочернего обучения: нужны его terminal exit и артефакты именно текущего run. Ни один Kiro tool не объявляется здесь заведомо синхронным на 25 минут. Реальная схема конкретного provider имеет приоритет.

**STOP:** немедленно прекратить новые действия; сообщить известное состояние, без дописывания предыдущего отчёта. **NEXT/конкретный ID:** заморозить предыдущую тему и перейти к указанному ID; не «закрывать ещё один хвост». При работающем процессе сохранить handle и явно обозначить его состояние; не запускать конфликтующий второй процесс. Новый запуск не разрешает удалять старую папку.

## Общие семьи между темами

Для TR07/TR08, обратного перевода или doc/code одного исходника сначала назначить один origin-family в `family-plan.json`. Использовать [пример](schemas/family-plan.example.json), сохраняя смысловой сценарий и связанные группы. Это публичная опора, только TRAIN/reference; для DEV/EXAM создавать новые сценарные семьи до генерации. Пример нельзя переименовать ради получения независимого экзамена.

```powershell
python tools/sets.py prepare --topic TR07 --profile ru-en --run FAMILY001 --count 1 --start 0 --split train --family-plan schemas/family-plan.example.json --work-root C:/x/lsets
python tools/sets.py prepare --topic TR08 --profile ru-en --run FAMILY001 --count 1 --start 0 --split train --family-plan schemas/family-plan.example.json --work-root C:/x/lsets
```

`family_plan` выбирает заранее объявленные eligible семьи, а не заново хеширует TR07/TR08. Скопировать `origin_family_id` и `leakage_group_ids` из request в metadata. Для полностью новых независимых сценариев достаточно обычного prepare; генератор обязан не переиспользовать чужие смысловые семьи под новыми ID. Общий `audit` проверяет объявленные связи, а семантический reviewer — скрытые дубликаты. Автоматической гарантии смысловой независимости нет.

Общий catalog относится ко **всем принятым непересекающимся shards этой новой библиотеки**; накопительные снимки в него не добавляются. Старые архивы источников не объявлены закрытым EXAM. Каждый merge требует свежий audit и последовательно дописывает каталог; параллельные merge не выполнять. Изменение зарегистрированного shard обнаруживается по SHA. Обновлённый каталог делает старый audit непригодным для следующего merge. `compose` проверяет перечисленные зарегистрированные shards и создаёт новый снимок, сохраняя предыдущие версии и их происхождение.
