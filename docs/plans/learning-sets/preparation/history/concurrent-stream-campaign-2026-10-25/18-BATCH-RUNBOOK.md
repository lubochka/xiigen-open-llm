# Создать одну DPO-партию и продолжить очередь

**Кампания продолжается до 192 000 пар по 192 потокам.** Одна партия — технический шаг. После finish сразу взять следующий недовыполненный поток; после END начать новый обход. Не задавать вопросы о продолжении, commit/push и увеличении batch: назначенный размер сохраняется, без политики публикации данные сохраняются локально. Полные группы, новые run IDs, -Previous и счёт без дублей — в [обязательном контракте продолжения](27-CAMPAIGN-CONTINUATION-CONTRACT.md). Это разрешение на следующие партии CREATE_ONLY, не на review/обучение.

Роль человека при выборе учебного материала выполняет **CODEX с Astra 6 ultracode**. Оценку качества DPO также выполняет **CODEX с Astra 6 ultracode**. Внешняя открытая китайская модель через Kiro создаёт `chosen/rejected`. Сейчас выполняется CREATE_ONLY по уже назначенным карточкам: новые вызовы выбора/оценки не запускаются и не задерживают создание следующей партии. Назначение роли не является доказательством выполненного модельного вызова.

**Текущий режим — CREATE_ONLY:** `prepare` → модель записывает `draft.jsonl` → `finish` → `DRAFT_CREATED` → следующий stream. Только создание кандидатов и минимальная проверка записи. Blind/review/accept/export, предметные runtime oracle, native import, Student и production-тесты не являются шагами этой сессии. Их команды сохранены ниже в отдельно обозначенном будущем этапе.

Текущая выбранная китайская модель Kiro создаёт пары; инструменты этого каталога её не вызывают. Модель не переключается автоматически, помощник наследует текущую. Не более одного помощника одновременно. Наблюдаемое имя фиксируется честно; если оно не раскрыто, используйте `unknown`. Checksum, подписанные receipts и фиксированный alias модели не требуются.

## 1. Использовать назначенные пути

| Назначение | Абсолютный путь |
|---|---|
| Репозиторий планов, ветка `llm_plan3` | `C:/Users/lubao/Documents/xiigen` |
| Корень создаваемых наборов | `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets` |
| План, инструменты, схемы и RAG | `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation` |
| Контракт записи | `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/02-DATA-CONTRACT.md` |
| JSON schema | `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/schemas/dpo-candidate.schema.json` |

`batches.py` без `--work-root` использует родитель `preparation`. При штатном расположении это ровно требуемый `learning-sets`, независимо от текущей рабочей папки. Не создавайте альтернативный короткий root и не ищите этот инструмент внутри `llm_mvp_core`. Для Linux-сопровождения корень также определяется относительно checkout; это не свидетельство записи на Windows-компьютер пользователя.

## 2. Выбрать один stream

Единственная общая очередь — [EXECUTION-QUEUE.md](EXECUTION-QUEUE.md) / `execution-queue.json`. Числовые позиции не являются ID. Прямое указание Любы имеет приоритет. Первый stream новой кампании — `TR07-OMISSION/ru-en`; существующую кампанию продолжайте по фактическим сохранённым состояниям.

```powershell
$xiigenQueue = Get-Content -Raw -LiteralPath "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/execution-queue.json" | ConvertFrom-Json
$xiigenQueue.streams | Select-Object order, stream_id, next_stream_id
$xiigenQueue.streams | Where-Object stream_id -EQ "TR07-OMISSION/ru-en" | Select-Object next_stream_id
```

Эти команды читают очередь средствами PowerShell, ничего не запускают и не требуют Python. Готовые finish/resume также возвращают `next_stream_id`. После `DRAFT_CREATED` переходите к следующему stream. Если Люба сказала «следующий» или назвала другой ID, прекратите работу над прежним отчётом/набором и перейдите сразу; сохранённые файлы не удаляйте.

## 3. Подготовить конкретное задание

Команда работает из любой папки:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream TR07-OMISSION/ru-en -Run M001 -Count 10 -Split train -Start 0
```

Обёртка использует `$PSScriptRoot`, обнаруживает Python 3 через `py -3` либо `python` и запускает `batches.py prepare`; она не выбирает модель и не генерирует данные. `-Action prepare` — значение по умолчанию. Поддерживаются `-Split`, `-Start`, `-Previous`, `-FamilyPlan`; используйте назначенные карточкой значения, не меняя квоты и split самостоятельно. Для продолжения и завершения та же обёртка принимает `-Action resume` или `-Action finish` и точный `-Request`. Готовые команды внутри партии уже используют абсолютный обнаруженный интерпретатор; исполнителю не нужно искать его повторно.

Результат: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/TR07-OMISSION/ru-en/M001/`.

| Файл | Назначение |
|---|---|
| `prompt.md` | Полный prompt: текущий режим, request, JSON schema/пример, карточка, источники и выходные пути |
| `request.json` | Stream/topic/objective/profile/split, семьи, квоты, next_start и абсолютные paths |
| `state.json` | Подготовленное задание; генерация ещё не выполнена |
| `commands.md` | Точные команды finish/resume/следующего шага для этой попытки |
| `report.template.md` | Русский шаблон без выдуманных метрик |
| `draft.jsonl` | Создаётся моделью после prepare; prepare сам его не генерирует |
| `report.creation.md` | Создаётся finish; существующий отчёт сохраняется с версией |

`request.paths` содержит preparation root, learning sets root, card, contract, schema, draft, state, commands, sources (ID → абсолютный путь) и tools script, а также `python_executable`. Передавайте этот набор путей каждому помощнику и сохраняйте его после compaction. Не давайте задачу заново искать схему в соседнем проекте.

RAG читается из **этого же checkout**, `preparation/rag/sources`. Все назначенные выдержки реальны; выбранные originals из main уже перенесены в `rag/originals`. Источники содержания ограничены `xiigen-planning`, ветками `main` и `llm_plan3`. Отдельный `--rag-root` нужен только при прямо заданной другой локальной копии. Если файл отсутствует, используйте точный ID/path и карту переноса; пустой manifest и выдуманный заголовок не заменяют источник. Подбор текста под hash запрещён; checksum-gates для генерации отсутствуют.

Учитывайте фактически доступный контекст модели. Размер prompt указывается в символах, не токенах, и не гарантирует вместимость provider. Не обрезайте различающее условие, таблицы или ответы молча. При нехватке контекста сохраните неполный результат и причину; следующая меньшая партия требует отдельного request с сохранением семейств, не случайного переключения модели.

## 4. Создать draft и завершить запись

Откройте подготовленные `prompt.md` и `request.json`. Текущая модель создаёт ровно назначенное число строк `xiigen.dpo.v1` в указанном `draft.jsonl`: полное видимое условие, chosen/rejected по одной теме и один содержательный дефект. Reference-примеры не копируются под новыми ID. Содержательные правила [BATCH-GENERATE](prompts/BATCH-GENERATE-PROMPT-RU.md) обязательны при создании, но не разрешают добавлять отдельный review-процесс.

После записи вызовите **один** структурный finish; дополнительный отдельный `check` не требуется:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Action finish -Request "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/TR07-OMISSION/ru-en/M001/request.json" -InputPath "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/TR07-OMISSION/ru-en/M001/draft.jsonl" -Model unknown
```

`-InputPath` можно опустить, если draft находится по пути request. Вместо обёртки можно выполнить готовую команду из `commands.md`: в ней уже указан обнаруженный абсолютный Python и параметры `--request`, `--input`, `--model`. `unknown` заменяется только фактически известным именем модели; не выводите его из формата tool calls или названия Kiro. `finish` проверяет JSON, контракт, число строк/семейств, ID и заявленные связи, сохраняет state и короткий русский `report.creation.md`, затем сообщает следующий stream. Проверка не требует review, не вызывает model/provider/oracle, не принимает смысл и не обучает Student.

При успешном результате `DRAFT_CREATED` означает «кандидатный файл создан». Независимая смысловая приёмка не выполнена, `review_status=not_requested`, `training_status=NOT_RUN`, метрики обучения не измерены. После этого сразу создавайте следующий назначенный набор. Если проверка обнаружила структурную ошибку или неполное количество, исправляйте только создаваемый draft по request, сохраняя прежнюю версию; не объявляйте такую попытку завершённой и не запускайте production-тесты.

## 5. Продолжить существующую попытку

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Action resume -Request "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/TR07-OMISSION/ru-en/M001/request.json"
```

`resume` показывает пути, состояние и следующий ID без генерации. Если есть request без draft, создайте draft. Если есть частичный файл, продолжите ровно его назначение. Если draft полон без итогового состояния, выполните finish. Если уже `DRAFT_CREATED`, переходите дальше. Не запускайте prepare поверх существующего run и не удаляйте его, чтобы обойти конфликт. Новый retry получает новый run ID; исходные файлы и причина остаются.

Если старый run создан вне назначенного learning-sets, его данные не удаляются и не выдаются за уже перенесённые. Зафиксируйте исходный путь и продолжайте создание новых наборов под правильным корнем; перенос прежнего содержимого не является скрытым побочным действием генератора. При специально порученном переносе сохраняется весь run и его зависимости, а не только draft.

Живой процесс ждите через один сохранённый handle и документированный wait/resume до terminal. Ранний yield, transport timeout и тишина stdout не доказывают отказ. Не запускайте дубли, частый polling или второй monitor и не заканчивайте ход вместо доступного ожидания. Если процесса нет, не придумывайте его. Прямой STOP останавливает действия; исторические STOP из attachment/RAG текущей командой не являются.

## 6. Следующая партия той же темы и связанные семейства

Для новой партии **того же** stream в пределах прежнего плана используйте новый run и previous request. При смене на новый реально созданный файл общих сценариев — новый run, `-FamilyPlan` и `-Start 0` **без `-Previous`**. Не обнуляйте cursor в старом файле ради повторения исчерпанных сценариев; подробности — [контракт 27](27-CAMPAIGN-CONTINUATION-CONTRACT.md). Пример продолжения прежнего потока:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream TR07-OMISSION/ru-en -Run M002 -Count 10 -Split train -Previous "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/TR07-OMISSION/ru-en/M001/request.json"
```

`-Previous` (параметр runner `--previous`) берёт `next_start` и сверяет stream/split. Индекс не равен количеству строк: отбор по split пропускает номера, некоторые семьи имеют две строки. У другого split собственный cursor. Для следующего **другого** stream берите его команду из registry, не previous чужой темы.

Для переводов общего сюжета, typo/layout и родственных DOC/CODE-тем используйте общий `-FamilyPlan` (параметр runner `--family-plan`), назначенный до генерации. Образец структуры — `schemas/family-plan.example.json`; он публичный и не является скрытым EXAM. План содержит family/origin/leakage groups, темы, профили, split и видимые факты. Одна связанная группа не может одновременно принадлежать TRAIN и EXAM даже в разных темах.

Обычная тема имеет одну пару на семью. `TR10-CONDITIONAL` имеет ambiguous/resolved, `TR08-RESTORE-OR-PRESERVE` — restore/preserve и поле `metadata.span_treatment`. Количество и классы берутся из `pairs_per_family`, `family_class_field`, `family_class_values` request. Два класса одного сюжета не считаются независимыми семьями. Пилот входит в родительскую квоту TRAIN; цели одного пула не удваивают бюджет. Публичные reference-примеры и их потомки не входят в закрытый EXAM.

## Только по отдельному последующему заданию: review, приёмка и экспорт

**Не выполнять разделы A–C в CREATE_ONLY.** Они сохранены для отдельно назначенного этапа и не являются условием перехода к следующему stream. Команды ниже выполняются из preparation; их output исправлен на реальный learning-sets. Перед будущим использованием перейдите в точный каталог:

```powershell
Set-Location -LiteralPath 'C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation'
$xiigenRequest = Get-Content -Raw -LiteralPath 'C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/TR07-OMISSION/ru-en/M001/request.json' | ConvertFrom-Json
$xiigenPython = $xiigenRequest.paths.python_executable
```

`$xiigenPython` берётся из реально созданного request; название/путь интерпретатора не угадывается. Следующие команды сохранены для будущего отдельного поручения, а не для текущего создания.

### Будущий этап A. Создать слепой пакет после отдельного поручения

```powershell
& $xiigenPython "tools/batches.py" check --input "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/TR07-OMISSION/ru-en/M001/draft.jsonl" --request "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/TR07-OMISSION/ru-en/M001/request.json"
& $xiigenPython "tools/batches.py" blind --input "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/TR07-OMISSION/ru-en/M001/draft.jsonl" --request "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/TR07-OMISSION/ru-en/M001/request.json" --out "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/TR07-OMISSION/ru-en/M001/blind.jsonl"
```

`check` проверяет требуемые поля, тему/профиль/split, семейные квоты, источники, дубликаты, конфликтующие ответы и объявленные связи между split. Успешный результат — **STRUCTURE_ONLY**, не «качество доказано» и не «обучение удалось».

`blind` создаёт `blind.jsonl` с A/B, закрытый `blind.key.json`, `blind.review.template.json` и `blind.review.prompt.md`. Закрытый ключ сохраняет точные исходные строки и позиции: приёмка обнаруживает изменение текста после ослепления без обязательных checksum. Это контроль случайной подмены версии, не криптографическое доказательство личности модели.

Reviewer получает только blind, его prompt, критерии и назначенные источники. Не передавать ему key, исходный draft, rationale или метки chosen/rejected. В blind доступны family/split/lineage для проверки родства, но скрыты канонический ответ, исправленная форма и классовые подсказки генератора.

Если уже существуют принятые партии, к check и accept добавляется `--against` с путями остальных accepted JSONL. Например:

```powershell
& $xiigenPython "tools/batches.py" check --input "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/TR07-OMISSION/ru-en/M002/draft.jsonl" --request "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/TR07-OMISSION/ru-en/M002/request.json" --against "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/TR07-OMISSION/ru-en/M001/accepted.jsonl"
```

Проверка ограничена перечисленными файлами и объявленной lineage. Она не может автоматически доказать, что переименованные смысловые парафразы независимы; это задача предметного review. Не называть проверку одной партии аудитом всего корпуса.

### Будущий этап B. Провести содержательную проверку

CODEX с Astra 6 ultracode читает [BATCH-REVIEW-PROMPT-RU.md](prompts/BATCH-REVIEW-PROMPT-RU.md), заполняет шаблон и сохраняет `review.json` при отдельно назначенном этапе оценки. В метаданных указываются запрошенная и наблюдаемая модели и отдельная session ID; независимость требует отдельного контекста без ключа. Если интерфейс не раскрывает внутреннюю модель, это записывается честно и не считается подтверждением identity. Китайский генератор не исполняет роль оценщика от имени CODEX.

По каждой строке нужны A/B verdict, конкретная причина и проверки смысла, достаточности контекста, одного дефекта, версии, семейства, правдоподобия и совпадения задания. Незавершённый шаблон, tie, неверный winner, просмотренный ключ и самопроверка той же сессии не дают приёмку.

| Предмет | Достаточное свидетельство проверки данных |
|---|---|
| Перевод | Bilingual review с конкретным объяснением смысла/контекста; модель не выдаётся за носителя |
| Оркестрация | Проигрывание состояний и следующего действия по одному сценарию |
| Код | Маленький исполняемый oracle: выигравший A/B проходит требуемое поведение, второй проваливает конкретную целевую проверку |
| Документация поведения | Исполнение исходного сценария/кода и сравнение каждого текста A/B с наблюдаемым результатом; prose не исполняется |
| Неисполняемый факт документации | Ссылка на назначенный источник и объяснение, с явным `depends_on_execution:false` |

Для execution сохраняются команда, среда и существующий сырой вывод; поля называются `winner_passed` и `other_failed_target_assertion`, потому что reviewer не знает скрытой метки chosen. Языковой или source-grounded review требует предметного текста, но отдельная подпись/хеш/профиль модели не требуются. Если objective объявляет `review_requirements.kind=executable_oracle`, приёмка требует execution независимо от слов reviewer `depends_on_execution:false`: методический RAG не подменяет runtime. Флаги без фактической проверки не разрешены.

Долгая проверка использует один process handle и штатный wait/resume. Тихий stdout, `running` и истёкший yield не являются отказом. Не создавать второй процесс или второй monitor и не заканчивать ход вместо ожидания. При явном «стоп» прекратить действия; при «следующий» перейти к новому ID, не заниматься прошлым отчётом.

### Будущий этап C. Принять одну версию и экспортировать

```powershell
& $xiigenPython "tools/batches.py" accept --input "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/TR07-OMISSION/ru-en/M001/draft.jsonl" --request "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/TR07-OMISSION/ru-en/M001/request.json" --blind "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/TR07-OMISSION/ru-en/M001/blind.jsonl" --key "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/TR07-OMISSION/ru-en/M001/blind.key.json" --review "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/TR07-OMISSION/ru-en/M001/review.json" --out "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/TR07-OMISSION/ru-en/M001/accepted.jsonl"
& $xiigenPython "tools/batches.py" export --inputs "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/TR07-OMISSION/ru-en/M001/accepted.jsonl" --out "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/TR07-OMISSION/ru-en/M001/export"
```

`accept` создаёт `accepted.jsonl` и `accepted.acceptance.json` только при полном review всех строк. Статус — `DATA_REVIEWED_NOT_NATIVE_TRAIN_READY`. Шаблон/PENDING не проходят. При исправлении сохранить `draft.v2.jsonl`, вызвать blind с `--out .../blind.v2.jsonl`, получить **новый** `review.v2.json` и принять в `accepted.v2.jsonl`. Старые данные/ключ/review сохраняются. Пользователь решает о переносе результатов в исходный код отдельно.

Для большого набора `export --inputs <accepted1> <accepted2> ... --out <новая версия>` объединяет уже принятые партии **одного domain/topic/profile/objective**. TRAIN/DEV/EXAM остаются разными файлами. Добавить `--against <другие партии>` для проверки связей с остальным известным корпусом; не передавать тот же accepted одновременно в inputs и against.

Экспорт сохраняет `dpo.train.jsonl`, `dpo.dev.jsonl`, `dpo.exam.jsonl` только с `prompt/chosen/rejected`; метки, rationale, canonical_source и review не попадают в learner text. Полная provenance хранится рядом. `export.json` указывает `INTERCHANGE_FORMAT_ONLY`, `native_import_verified:false`, `training_status:NOT_RUN`. При переносе рабочей папки сохранить исходный draft/request/blind/key/review; acceptance содержит относительные пути к этим версиям, когда они находятся на одном диске, и export перечитывает их. Можно перенести весь run, сохранив внутреннюю структуру; при внешних зависимостях на другом Windows-диске остаются абсолютные пути, которые также должны быть доступны. Это предотвращает принятие изменённого текста по старому review.

Native `DpoDataset` и repository-owned authoring имеют свои контракты, описанные в [10-NATIVE-CONTRACT-BOUNDARY.md](10-NATIVE-CONTRACT-BOUNDARY.md). Обменный JSONL не объявляется нативным входом. Реальный learning binding, совместимость checkpoint и авторство проверяются при будущем учебном задании; ограничения native не обходятся переименованием provenance.

## Сохранённые свидетельства проверки прежней версии инструментов

Эти результаты относятся к предыдущей поставке до изменения CREATE_ONLY; они не заявляют новый прогон и не являются обязанностями генератора. В [verification/batches-tool-checks.json](verification/batches-tool-checks.json) зафиксированы **37 локальных проверок**: подготовка шести разных потоков, повтор run, следующий cursor, чужой previous, утечка во всём family plan, некорректные source refs, отсутствие origin, неверный objective, переименованная семья между split, незавершённый и устаревший review, неверный победитель, та же сессия, экспорт полей, изменение accepted, перенос полного run с относительными связями два обобщённых family-класса и невозможность подменить обязательный runtime oracle заявлением source-only.

Воспроизведение для сопровождающего инструментов, **не обязательный этап каждой партии**:

```powershell
& $xiigenPython "tools/check_batches.py"
```

Сохранённый отчёт предыдущей версии фиксирует, что все 169 потоков прошли реальную сборку prompt без вызова модели; [отдельный отчёт](verification/batch-prepare-all-streams.json) фиксирует количество пар/семей и размеры промптов. В предметных инструкциях нет обязательных старых профилей, receipts или команд sets.py; исторические RAG-фрагменты остаются источниками данных.

Тест создаёт временные технические fixtures, включая заведомо синтетические записи reviewer, чтобы проверить ветви валидации. Они не публикуются как корпус и не являются реальным review. **Provider calls: 0; training runs: 0; независимых предметных review: 0.** Это проверка инструментов подготовки, а не доказательство качества перевода или обучения. Для урока остаются требования пользователя: восстановленное совместимое обученное состояние, достаточный набор/бюджет, независимое до/после, запрет считать `0→0` успешным обучением и отдельные улучшения/ухудшения по каждой полезной метрике.

