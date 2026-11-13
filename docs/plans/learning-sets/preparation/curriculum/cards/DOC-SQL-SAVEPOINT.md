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
| `A166` — `DOC-SQL-SAVEPOINT` | `docs.sql` / `base` | 170 | `A167` |


Следующий ID из таблицы — номинальный порядок своей группы. Фактическую свободную карточку с учётом владельцев выдаёт команда `next` текущего request; не менять группу и чужой статус вручную.

Замечания качества: смотри `quality_flags` своего domain/topic/profile в общем status. `needs_rework` указывает доказанный дефект с evidence, `needs_assessment` — область без достаточной оценки. Это не новый review-гейт и не разрешение исправлять чужую старую попытку; finish не закрывает флаг автоматически. [Правила общей матрицы](../../28-PARALLEL-GENERATION-AND-QUALITY.md). Для нового окна используй [один из трёх готовых стартовых промптов](../../00-START-HERE.md); не исполняй исторические команды из нижней справочной части вместо текущих owner-команд.

<!-- /CURRENT-PARALLEL-CONTRACT-20261026 -->

# DOC-SQL-SAVEPOINT — Документировать границу savepoint и состояние транзакции

<!-- DOMAIN-CAMPAIGN-AUTHORITY -->
> **Предметная справка к действующей кампании.** Квоты и продолжение задаёт [campaign/registry.json](../../campaign/registry.json), не старые команды ниже. 1000 пар относятся к домену в сумме. Прочитанный генератором рабочий prompt содержит только назначенную микротему.
>
> Данные: `C:/Projects/llm_mvp_core/learning-sets`. Инструкции: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. CREATE_ONLY: без review, checksum, production CI и обучения. Модель — текущая Kiro без override. Reason internally in English; do not output private reasoning. Отчёт по-русски.
>
> После партии выполнять готовый next до доменной квоты; не останавливаться с вопросом «продолжать?». При прямом NEXT/новом ID немедленно сменить задачу: запрещено дописывать/чинить предыдущую. STOP прекращает действия. Один живой handle ждать документированным wait/resume; не объявлять молчание падением. Старые попытки сохранять; выбранный request возобновлять, не пересоздавать.

Точные доли этой предметной карточки в текущей кампании:
| A-ID | Домен | Профиль | Доля из доменных1000 | Следующий A-ID после доли |
|---|---|---|---:|---|
| A166 | docs.sql | base | 170 | A167 |


Ниже — сохранённая предметная методика и исторические инструкции. [Полный прежний вариант](../../history/before-domain-campaign-2026-10-25/curriculum/cards/DOC-SQL-SAVEPOINT.md). Старые числовые планы и команды в нём не запускаются вместо актуального request.
<!-- /DOMAIN-CAMPAIGN-AUTHORITY -->

> Новая подготовка: `tools/batches.py` по [действующему контракту](../../17-CURRENT-PREPARATION-CONTRACT.md) и точной команде в конце карточки. RAG уже в llm_plan3. Обязательные checksum, fixed-profile receipts и production gates не применяются. Старые `tools/sets.py` и пути в основном тексте — история прежнего runner; не запускать их поверх нового request. При NEXT/новом ID немедленно сменить задачу, запрещено продолжать прежнюю или её отчёт; при STOP прекратить действия.

Родительская тема: `DOC-SQL-03`. Домен: `docs.sql`. Профиль: `base`. Первый run: `DS401`. Добавление к прежним темам и примерам; старые материалы сохраняются. Статус: **AUTHOR_REFERENCE / ORACLE_NOT_RUN / LEARNING_NOT_RUN**.

## Обучаемая граница

По полному видимому SQL определить сохранённые/отменённые изменения и состояние транзакции до и после COMMIT; не менять описываемую программу.

## Источники и происхождение

Назначенные источники — `RAG-041` и `RAG-042`; точные commit/blob/выдержки закреплены в [sources.lock.json](../../sources.lock.json) и RAG в main. RAG-041 даёт методику компактного технического контраста; RAG-042 — проектный контекст многошаговой транзакционной записи. Ни один из этих двух excerpts не используется как технический authority механики savepoint. Точный источник SAVE TRANSACTION в назначенных excerpts не найден; полный видимый script здесь — **self-contained simulation / runtime-required**. Предсказанные trace не выдаются за выполненный SQL или за measured learning.

Известный предшественник — `DOC-SQL-03.REF.003` в [родительском reference-наборе](../../domains/documentation/reference-examples.jsonl). Все шесть примеров этой карточки, шесть соответствующих DOC/CODE-вариантов и производные предшественника связаны `origin_family_id=reference/DOC-SQL-03/003`. Шесть сценарных family ID не означают шесть независимых источников; все двенадцать публичных пар дают **один известный origin**. Их нельзя считать закрытым DEV/EXAM. `derived_from` и общие leakage_group_ids сохраняются при перефразировании, изменении чисел и переносе из DOC в CODE.

## Навыки и режим исполнителя

Режим — подготовка наборов и документации, не production implementation. Для соответствующих ролей использовать проектные `docs/skills/architect-behavior-classifier-SKILL.md`, `planning-dpo-authoring-SKILL.md`, `plan-review-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `dpo-triple-format-SKILL.md`, `human-communication-GLOSSARY.md` из [библиотеки навыков core](https://github.com/lubochka/llm_mvp_core/tree/codex/dpo-implementation/docs/skills). Проверяющий SQL применяет `generated-code-review-SKILL.md` из переданного `01-all-skills.zip`/проектной библиотеки к двум полным кандидатам и видимому контракту. Применимые разделы выбираются по текущей роли; старые требования двух разных генераторов, production gates или обязательного результата к концу сессии не создают ложных вызовов и измерений. Приоритеты разрешены в [07-CONFLICTS-AND-REQUIREMENTS.md](../../07-CONFLICTS-AND-REQUIREMENTS.md).

Материал выбирает и оценивает **CODEX в фактически выбранной модели отдельной сессии**, пары генерирует уже выбранная **китайская модель Kiro без override**. Имя среды не подменяет имя модели. Профиль, selection и review подтверждаются фактической записью по [контракту ролей](../../10-CODEX-AND-CHINESE-ROLES.md); эта карточка не является такой квитанцией. Отчёт пишется на русском по [08-REPORT-TEMPLATE.md](../../08-REPORT-TEMPLATE.md).

## Точное назначение и команды

Действующая команда и точный каталог находятся в разделе «Точные команды действующих потоков» в конце карточки. Подготовка — `tools/batches.py`, RAG встроен в llm_plan3. Каталог содержит request, prompt, commands, state и шаблон отчёта; следующий шаг определяется реальным состоянием попытки. Старые инструкции этой секции сохранены в history/legacy-card-launch-sections.jsonl и не выполняются поверх нового request.

## Промпт генератору

```text
Одна микроцель — документировать наблюдаемое действие ROLLBACK к savepoint, полного ROLLBACK и последующего COMMIT в T-SQL / SQL Server 2019. Дай полностью видимый script: session assumptions, DDL, начальные строки, каждый BEGIN/SAVE/UPDATE/DELETE/INSERT/ROLLBACK/COMMIT и SELECT наблюдения. Условия начального @@TRANCOUNT=0, отсутствия ошибок/конкуренции, локальной #temp и setup до BEGIN обязаны быть видимы. Вопрос просит описать фактически показанный script, а не исправлять поведение ради желаемого бизнеса. Ответы — два полных JSON trace одинаковой схемы с rows и состоянием транзакции на всех видимых stage. Чётко отличай состояние до финального COMMIT и после него. Минимальный batch 6: по одному независимому назначенному сценарию каждой из шести категорий; назначенный batch 12: по два. Сохрани полный rollback как положительный контроль, чтобы не обучать правилу «всегда rollback к savepoint». Не обобщай эти однопоточные примеры на locks, производительность, ошибки, distributed или другие SQL-диалекты. Нельзя переписывать SQL в объяснении и затем считать trace переписанного SQL ответом.

Работай с одной назначенной микроцелью, одним domain_id/profile_id и точными family_id/split/count из request.json. Все факты, setup, версии и ограничения ответа дай внутри learner-visible prompt: metadata ученик не получает. Chosen и rejected имеют один формат; rejected нарушает одну границу управления транзакцией. Не добавляй [WRONG], source ID, rationale, оценку или метку chosen внутрь ответов. Источники RAG-041/042 задают методику и транзакционный контекст, но не являются техническим доказательством SAVE TRANSACTION; эти примеры — авторская simulation/runtime-required, не цитаты проекта или измерения. Все публичные reference этой карточки и DOC/CODE-пары сохраняют общий origin reference/DOC-SQL-03/003; новые числа и имена не дают независимого семейства. Публичные примеры и их производные не помещать в DEV/EXAM. Новые семейства и split назначаются до генерации; для независимого holdout отдельная назначенная сессия выбирает новые структуры сценария, а не перефразирует этот текст.

Сначала прочитай назначенные источники и обоснуй выбор одной учебной границы. Затем текущая выбранная китайская модель Kiro создаёт draft; после структурного finish переходи к следующему потоку без review. Записывай только наблюдаемую модель и фактические результаты. Формальные selection receipts и подтверждение заранее заданного профиля не требуются.

Отчёт на русском по 08-REPORT-TEMPLATE.md. При прямом NEXT немедленно перейти к указанному ID и не дописывать предыдущую задачу; без ID брать execution-queue.json. При STOP немедленно прекратить новые действия. Старые эксперименты, данные и версии не удалять. Production не менять. Для уже запущенного долгого процесса найти штатный wait/resume для его одного handle и дождаться terminal в текущем раунде; не заканчивать раунд вместо ожидания, не делать busy polling и не создавать новый запуск или монитор вместо ожидания. Пауза инструмента не доказывает отказ процесса; фактический exit и result читаются после terminal.

Oracle до выполнения имеет NOT_RUN и result_observed=false. SQL Server oracle относится к будущей проверке; сейчас oracle=NOT_RUN и не блокирует создание кандидатов. Следующий учебный пилот восстанавливает совместимое обученное ненулевое B1 с tokenizer/optimizer/RNG и hashes split; новый bootstrap только по отдельному назначенному плану с сохранением состояния для повторного использования. 0 правильных до и после не доказывает обучение; loss/margin и число пар не заменяют полезный результат. Любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост точности не обязателен. Рядом показать изменения всех других показателей, включая ухудшения. Только измеренные улучшения документируются кандидатом в исправления; автоматической production-интеграции нет.
```

## Договор ответа

Только JSON-массив; каждый объект содержит stage, trancount, xact_state, rows. Наблюдения в порядке script, rows по Id. Без ремонта SQL и без объяснительных полей.

## Состав партии и большого набора

Назначено 12 пар в первой партии: по две для каждой из шести категорий. Минимальная сокращённая партия при ограничении контекста — 6 пар, по одной каждой категории; уменьшение назначается новым request до model call с сохранением прежнего каталога. Ни full rollback, ни сложный случай двух BEGIN нельзя выкинуть ради количества. Полные scripts и session assumptions не сокращаются.

Пилотный план: 120 TRAIN / 24 DEV / 24 EXAM — соответственно 20 / 4 / 4 по каждой категории, `pairs_per_family=1`. Это план новых проверенных сценариев, **не уже созданный набор** и не обещание достаточного учебного бюджета. Public reference не входят в эти числа. TRAIN относится к доле родительского бюджета из [scale-plan](../scale-plan.json), а не прибавляется к ней. В закрытые splits берутся отдельно назначенные происхождения с новыми структурами операций; замена имён/чисел или DOC→CODE не создаёт независимость. Классы одной savepoint-темы разрешены внутри набора; разные предметные темы не смешиваются.

## Оси генерации

- **transaction_boundary:** Две строки по разные стороны savepoint; Одна строка изменена до и после savepoint; Полный ROLLBACK как положительный контроль; Выбор между двумя видимыми savepoint; Отмена INSERT/DELETE и продолжение записи; Savepoint внутри двух BEGIN и счётчик завершения

- **visible_observation:** rows до завершающего COMMIT; @@TRANCOUNT и XACT_STATE на каждом stage; rows после завершения; дополнительная запись после savepoint rollback

- **controlled_wrong_choice:** полный rollback вместо savepoint; COMMIT сохраняет отменяемый suffix; неверная из двух существующих точек; savepoint вместо требуемого полного rollback


## Не смешивать с соседними темами

- Ошибка/неcommittable transaction, XACT_ABORT ON и TRY/CATCH как новый учебный фактор

- Distributed/ambient transactions, гонки, locks, isolation performance, durability after crash

- PostgreSQL/SQLite/MySQL savepoints или интерпретация T-SQL как другой диалект

- ORM/saga/outbox/migration и скрытые trigger/constraint effects

- Внешняя BEGIN/ROLLBACK-обёртка oracle, меняющая начальный @@TRANCOUNT

- Автоматическая приёмка public reference в TRAIN/DEV/EXAM и разбивка родственных DOC/CODE на разные split


## Будущий этап: содержательная проверка (сейчас не запускать)

- Каждый prompt содержит SQL Server 2019, initial @@TRANCOUNT=0/XACT_STATE=0, локальную #temp до BEGIN, все statement и отсутствие ошибок/конкуренции; metadata не восполняет условия.

- Свежая отдельная connection на каждый candidate; запрещена внешняя тестовая BEGIN/ROLLBACK-обёртка, поскольку она меняет проверяемый @@TRANCOUNT.

- Сопоставить rows, @@TRANCOUNT и XACT_STATE на каждом stage; одного совпавшего конечного значения недостаточно.

- Для DOC исполняется только фактически показанный script; chosen/rejected — гипотезы о его trace, а не альтернативные исходные программы.

- Для CODE сравнить полный текст обоих кандидатов: ровно один изменённый control statement; исполнить их отдельно при идентичном setup, отвергнутый обязан нарушать явную цель.

- Оставить случаи полного rollback и двух savepoint; нет словесного shortcut по наличию имени kept или одинакового шаблона preferred-команды.

- Oracle лог и expected различаются: NOT_RUN/result_observed=false до запуска; сохранённый ожидаемый trace не считается фактическим.

- DOC/CODE variants и известный predecessor связаны origin/leakage; все 12 references принадлежат одному известному source-origin и не составляют закрытый holdout.


## Native oracle: точная граница исполнения

Oracle готовится reviewer в `oracle/` назначенного run из полных visible scripts каждой строки. **Не использовать общий тестовый outer BEGIN/ROLLBACK:** он изменит исходный @@TRANCOUNT=0 и может превратить корректный материал в неправильный тест. Каждому candidate требуется собственное свежее соединение SQL Server 2019. Setup #S выполняется в autocommit до первого BEGIN именно как написано; затем script сам управляет своей транзакцией. Закрытие соединения очищает временные объекты. Не подставлять постоянную БД/таблицы или SQLite.

Для DOC один script даёт result sets stage→rows; обе JSON-гипотезы сравниваются с одним trace. Для CODE оба полных script исполняются отдельно при идентичном setup; единственный текстовый diff — control statement. JSON expected и rejected_expected в metadata пока авторские предсказания. Результат каждого запуска должен содержать runtime/version, фактические result sets, exit status и хеш точного candidate; проверка сначала выполняется слепо, затем связывается с pair через review. SQL исполнения не получают error_tag/rationale как данные тестируемой программы.

Сначала reviewer сохраняет извлечённые неизменённые candidate scripts в `C:/x/lsets/DOC-SQL-03/base/DS401/oracle/`. `manifest.json` содержит массив `cases`: у каждой записи `script` — имя полного `.sql` внутри oracle, `output` — новое имя соответствующего `.actual.log`, `pair_id` и blind-side для связи с проверкой. Для DOC один script на пару, для CODE два script A/B; key и rationale в manifest не копируются. Имена результата не переиспользуются. Имя уже настроенного SQL Server 2019 instance находится в `XIIGEN_SQLSERVER`, Windows integrated auth. Следующий назначенный запуск делает **отдельный sqlcmd-процесс и отдельную connection на каждый полный script**:

```powershell
$savepointOracleDirectory = "C:/x/lsets/DOC-SQL-03/base/DS401/oracle"
$savepointManifest = Get-Content -Raw "$savepointOracleDirectory/manifest.json" | ConvertFrom-Json
foreach ($savepointCase in $savepointManifest.cases) {
    $savepointScript = Join-Path $savepointOracleDirectory $savepointCase.script
    $savepointLog = Join-Path $savepointOracleDirectory $savepointCase.output
    if (Test-Path -LiteralPath $savepointLog) { throw "Result already exists: $savepointLog" }
    & sqlcmd -S "$env:XIIGEN_SQLSERVER" -E -b -i "$savepointScript" -o "$savepointLog"
    if ($LASTEXITCODE -ne 0) { throw "SQL execution failed for $savepointScript; exit=$LASTEXITCODE" }
}
```

После terminal reviewer читает все stage/rows из каждого `.actual.log` и сверяет с A/B и learner-visible контрактом. Exit 0 сам по себе не означает верное поведение. Связанные хеши input/blind/scripts/logs и вывод по каждой паре записываются в native evidence. Существующий output — повод продолжить проверку прежнего результата или оформить новую версию запуска, а не удалить/перезаписать его.

Этот блок — назначенный будущий шаг, не свидетельство запуска и не утверждение наличия manifest/scripts. До его фактической подготовки/исполнения `oracle.status=NOT_RUN`, `result_observed=false`. Нет настроенного instance или Sqlcmd — ORACLE_PENDING; не изобретать PASS по рассуждению или успешному разбору JSON. Здесь нет результатов SQL Server, генератора и Student.

## Полные публичные контрасты

### DOC-SQL-SAVEPOINT.REF.01 — Две строки по разные стороны savepoint

Сценарное family: `v4.ref.sql.savepoint.two-rows`. Общий origin: `reference/DOC-SQL-03/003`. Статус oracle: `NOT_RUN`; result_observed: `false`.

**Полное условие ученику:**

````text
Авторская самодостаточная симуляция. T-SQL / SQL Server 2019. Свежое отдельное соединение: до первой строки @@TRANCOUNT=0 и XACT_STATE()=0; внешней, ambient или distributed транзакции нет. IMPLICIT_TRANSACTIONS OFF, XACT_ABORT OFF, ANSI_NULLS ON. Используется только локальная временная #S; она создана и заполнена до первого BEGIN TRANSACTION. Триггеров, внешних ключей, дополнительных CHECK, ошибок, обрывов и конкурентных соединений нет. Все Id различны, все int-операции допустимы. Нет скрытых statement или вызовов. После SELECT со stage следующий SELECT возвращает rows как пары [Id, Val] по возрастанию Id. trancount — значение @@TRANCOUNT; xact_state — XACT_STATE(): 0 означает отсутствие транзакции, 1 — активную транзакцию, допускающую COMMIT.

Фактически выполняется следующий полный script:
```sql
SET NOCOUNT ON;
SET IMPLICIT_TRANSACTIONS OFF;
SET XACT_ABORT OFF;
SET ANSI_NULLS ON;
CREATE TABLE #S (Id int NOT NULL PRIMARY KEY, Val int NOT NULL);
INSERT #S (Id, Val) VALUES (1,10),(2,20);
BEGIN TRANSACTION;
UPDATE #S SET Val=Val+5 WHERE Id=1;
SAVE TRANSACTION kept;
UPDATE #S SET Val=Val+7 WHERE Id=2;
-- control
ROLLBACK TRANSACTION kept;
SELECT N'before_finish' AS stage, @@TRANCOUNT AS trancount, XACT_STATE() AS xact_state;
SELECT Id, Val FROM #S ORDER BY Id;
IF @@TRANCOUNT > 0 COMMIT TRANSACTION;
SELECT N'after_finish' AS stage, @@TRANCOUNT AS trancount, XACT_STATE() AS xact_state;
SELECT Id, Val FROM #S ORDER BY Id;
```

Опиши наблюдаемое состояние на каждом явно напечатанном stage именно этого script. Не исправляй SQL и не заменяй управляющий statement. Ответ — только JSON-массив объектов в порядке stage; каждый объект имеет ровно stage, trancount, xact_state и rows. rows — упорядоченный массив пар [Id, Val].
````

**Предпочтительный ответ:**

```json
[
  {
    "stage": "before_finish",
    "trancount": 1,
    "xact_state": 1,
    "rows": [
      [
        1,
        15
      ],
      [
        2,
        20
      ]
    ]
  },
  {
    "stage": "after_finish",
    "trancount": 0,
    "xact_state": 0,
    "rows": [
      [
        1,
        15
      ],
      [
        2,
        20
      ]
    ]
  }
]
```

**Правдоподобный ошибочный ответ:**

```json
[
  {
    "stage": "before_finish",
    "trancount": 0,
    "xact_state": 0,
    "rows": [
      [
        1,
        10
      ],
      [
        2,
        20
      ]
    ]
  },
  {
    "stage": "after_finish",
    "trancount": 0,
    "xact_state": 0,
    "rows": [
      [
        1,
        10
      ],
      [
        2,
        20
      ]
    ]
  }
]
```

**Почему различаются:** Откат к kept восстанавливает состояние на границе: +5 до неё сохраняется, +7 после неё отменяется. Полный ROLLBACK отменил бы обе записи и завершил транзакцию. Это один различающийся управляющий оператор, а не два разных входных набора.

**Контрпример/граница вывода:** Если видимый договор требовал бы отменить весь BEGIN-блок, правильным был бы ROLLBACK TRANSACTION без имени; требование сохранить +5 делает его ошибочным здесь.

### DOC-SQL-SAVEPOINT.REF.02 — Одна строка изменена до и после savepoint

Сценарное family: `v4.ref.sql.savepoint.same-row`. Общий origin: `reference/DOC-SQL-03/003`. Статус oracle: `NOT_RUN`; result_observed: `false`.

**Полное условие ученику:**

````text
Авторская самодостаточная симуляция. T-SQL / SQL Server 2019. Свежое отдельное соединение: до первой строки @@TRANCOUNT=0 и XACT_STATE()=0; внешней, ambient или distributed транзакции нет. IMPLICIT_TRANSACTIONS OFF, XACT_ABORT OFF, ANSI_NULLS ON. Используется только локальная временная #S; она создана и заполнена до первого BEGIN TRANSACTION. Триггеров, внешних ключей, дополнительных CHECK, ошибок, обрывов и конкурентных соединений нет. Все Id различны, все int-операции допустимы. Нет скрытых statement или вызовов. После SELECT со stage следующий SELECT возвращает rows как пары [Id, Val] по возрастанию Id. trancount — значение @@TRANCOUNT; xact_state — XACT_STATE(): 0 означает отсутствие транзакции, 1 — активную транзакцию, допускающую COMMIT.

Фактически выполняется следующий полный script:
```sql
SET NOCOUNT ON;
SET IMPLICIT_TRANSACTIONS OFF;
SET XACT_ABORT OFF;
SET ANSI_NULLS ON;
CREATE TABLE #S (Id int NOT NULL PRIMARY KEY, Val int NOT NULL);
INSERT #S (Id, Val) VALUES (1,100);
BEGIN TRANSACTION;
UPDATE #S SET Val=Val-10 WHERE Id=1;
SAVE TRANSACTION kept;
UPDATE #S SET Val=Val-7 WHERE Id=1;
-- control
ROLLBACK TRANSACTION kept;
SELECT N'before_finish' AS stage, @@TRANCOUNT AS trancount, XACT_STATE() AS xact_state;
SELECT Id, Val FROM #S ORDER BY Id;
IF @@TRANCOUNT > 0 COMMIT TRANSACTION;
SELECT N'after_finish' AS stage, @@TRANCOUNT AS trancount, XACT_STATE() AS xact_state;
SELECT Id, Val FROM #S ORDER BY Id;
```

Опиши наблюдаемое состояние на каждом явно напечатанном stage именно этого script. Не исправляй SQL и не заменяй управляющий statement. Ответ — только JSON-массив объектов в порядке stage; каждый объект имеет ровно stage, trancount, xact_state и rows. rows — упорядоченный массив пар [Id, Val].
````

**Предпочтительный ответ:**

```json
[
  {
    "stage": "before_finish",
    "trancount": 1,
    "xact_state": 1,
    "rows": [
      [
        1,
        90
      ]
    ]
  },
  {
    "stage": "after_finish",
    "trancount": 0,
    "xact_state": 0,
    "rows": [
      [
        1,
        90
      ]
    ]
  }
]
```

**Правдоподобный ошибочный ответ:**

```json
[
  {
    "stage": "before_finish",
    "trancount": 0,
    "xact_state": 0,
    "rows": [
      [
        1,
        83
      ]
    ]
  },
  {
    "stage": "after_finish",
    "trancount": 0,
    "xact_state": 0,
    "rows": [
      [
        1,
        83
      ]
    ]
  }
]
```

**Почему различаются:** Состояние savepoint определяется моментом, а не отдельным набором строк: у той же строки сохраняется -10, отменяется -7. COMMIT сохранил бы оба уменьшения и завершил единственный уровень транзакции.

**Контрпример/граница вывода:** Если требовалось сохранить оба уменьшения, COMMIT дал бы 83. Здесь требуется состояние 90 до и после финального завершения, поэтому сохранение suffix недопустимо.

### DOC-SQL-SAVEPOINT.REF.03 — Полный ROLLBACK как положительный контроль

Сценарное family: `v4.ref.sql.savepoint.full-rollback`. Общий origin: `reference/DOC-SQL-03/003`. Статус oracle: `NOT_RUN`; result_observed: `false`.

**Полное условие ученику:**

````text
Авторская самодостаточная симуляция. T-SQL / SQL Server 2019. Свежое отдельное соединение: до первой строки @@TRANCOUNT=0 и XACT_STATE()=0; внешней, ambient или distributed транзакции нет. IMPLICIT_TRANSACTIONS OFF, XACT_ABORT OFF, ANSI_NULLS ON. Используется только локальная временная #S; она создана и заполнена до первого BEGIN TRANSACTION. Триггеров, внешних ключей, дополнительных CHECK, ошибок, обрывов и конкурентных соединений нет. Все Id различны, все int-операции допустимы. Нет скрытых statement или вызовов. После SELECT со stage следующий SELECT возвращает rows как пары [Id, Val] по возрастанию Id. trancount — значение @@TRANCOUNT; xact_state — XACT_STATE(): 0 означает отсутствие транзакции, 1 — активную транзакцию, допускающую COMMIT.

Фактически выполняется следующий полный script:
```sql
SET NOCOUNT ON;
SET IMPLICIT_TRANSACTIONS OFF;
SET XACT_ABORT OFF;
SET ANSI_NULLS ON;
CREATE TABLE #S (Id int NOT NULL PRIMARY KEY, Val int NOT NULL);
INSERT #S (Id, Val) VALUES (1,10),(2,20);
BEGIN TRANSACTION;
UPDATE #S SET Val=Val+5 WHERE Id=1;
SAVE TRANSACTION kept;
UPDATE #S SET Val=Val+7 WHERE Id=2;
-- control
ROLLBACK TRANSACTION;
SELECT N'before_finish' AS stage, @@TRANCOUNT AS trancount, XACT_STATE() AS xact_state;
SELECT Id, Val FROM #S ORDER BY Id;
IF @@TRANCOUNT > 0 COMMIT TRANSACTION;
SELECT N'after_finish' AS stage, @@TRANCOUNT AS trancount, XACT_STATE() AS xact_state;
SELECT Id, Val FROM #S ORDER BY Id;
```

Опиши наблюдаемое состояние на каждом явно напечатанном stage именно этого script. Не исправляй SQL и не заменяй управляющий statement. Ответ — только JSON-массив объектов в порядке stage; каждый объект имеет ровно stage, trancount, xact_state и rows. rows — упорядоченный массив пар [Id, Val].
````

**Предпочтительный ответ:**

```json
[
  {
    "stage": "before_finish",
    "trancount": 0,
    "xact_state": 0,
    "rows": [
      [
        1,
        10
      ],
      [
        2,
        20
      ]
    ]
  },
  {
    "stage": "after_finish",
    "trancount": 0,
    "xact_state": 0,
    "rows": [
      [
        1,
        10
      ],
      [
        2,
        20
      ]
    ]
  }
]
```

**Правдоподобный ошибочный ответ:**

```json
[
  {
    "stage": "before_finish",
    "trancount": 1,
    "xact_state": 1,
    "rows": [
      [
        1,
        15
      ],
      [
        2,
        20
      ]
    ]
  },
  {
    "stage": "after_finish",
    "trancount": 0,
    "xact_state": 0,
    "rows": [
      [
        1,
        15
      ],
      [
        2,
        20
      ]
    ]
  }
]
```

**Почему различаются:** В этом контрольном случае нужна отмена всего BEGIN-блока. Полный ROLLBACK возвращает исходные две строки и @@TRANCOUNT=0; откат к kept сохранил бы +5 и активную транзакцию. Правильная сторона не обязана всегда содержать имя savepoint.

**Контрпример/граница вывода:** В задаче, требующей сохранить +5, именованный ROLLBACK был бы правильным. Здесь явный договор требует отменить +5 и +7 вместе.

### DOC-SQL-SAVEPOINT.REF.04 — Выбор между двумя видимыми savepoint

Сценарное family: `v4.ref.sql.savepoint.two-points`. Общий origin: `reference/DOC-SQL-03/003`. Статус oracle: `NOT_RUN`; result_observed: `false`.

**Полное условие ученику:**

````text
Авторская самодостаточная симуляция. T-SQL / SQL Server 2019. Свежое отдельное соединение: до первой строки @@TRANCOUNT=0 и XACT_STATE()=0; внешней, ambient или distributed транзакции нет. IMPLICIT_TRANSACTIONS OFF, XACT_ABORT OFF, ANSI_NULLS ON. Используется только локальная временная #S; она создана и заполнена до первого BEGIN TRANSACTION. Триггеров, внешних ключей, дополнительных CHECK, ошибок, обрывов и конкурентных соединений нет. Все Id различны, все int-операции допустимы. Нет скрытых statement или вызовов. После SELECT со stage следующий SELECT возвращает rows как пары [Id, Val] по возрастанию Id. trancount — значение @@TRANCOUNT; xact_state — XACT_STATE(): 0 означает отсутствие транзакции, 1 — активную транзакцию, допускающую COMMIT.

Фактически выполняется следующий полный script:
```sql
SET NOCOUNT ON;
SET IMPLICIT_TRANSACTIONS OFF;
SET XACT_ABORT OFF;
SET ANSI_NULLS ON;
CREATE TABLE #S (Id int NOT NULL PRIMARY KEY, Val int NOT NULL);
INSERT #S (Id, Val) VALUES (1,10);
BEGIN TRANSACTION;
UPDATE #S SET Val=Val+2 WHERE Id=1;
SAVE TRANSACTION early_point;
UPDATE #S SET Val=Val+3 WHERE Id=1;
SAVE TRANSACTION late_point;
UPDATE #S SET Val=Val+5 WHERE Id=1;
-- control
ROLLBACK TRANSACTION late_point;
SELECT N'before_finish' AS stage, @@TRANCOUNT AS trancount, XACT_STATE() AS xact_state;
SELECT Id, Val FROM #S ORDER BY Id;
IF @@TRANCOUNT > 0 COMMIT TRANSACTION;
SELECT N'after_finish' AS stage, @@TRANCOUNT AS trancount, XACT_STATE() AS xact_state;
SELECT Id, Val FROM #S ORDER BY Id;
```

Опиши наблюдаемое состояние на каждом явно напечатанном stage именно этого script. Не исправляй SQL и не заменяй управляющий statement. Ответ — только JSON-массив объектов в порядке stage; каждый объект имеет ровно stage, trancount, xact_state и rows. rows — упорядоченный массив пар [Id, Val].
````

**Предпочтительный ответ:**

```json
[
  {
    "stage": "before_finish",
    "trancount": 1,
    "xact_state": 1,
    "rows": [
      [
        1,
        15
      ]
    ]
  },
  {
    "stage": "after_finish",
    "trancount": 0,
    "xact_state": 0,
    "rows": [
      [
        1,
        15
      ]
    ]
  }
]
```

**Правдоподобный ошибочный ответ:**

```json
[
  {
    "stage": "before_finish",
    "trancount": 1,
    "xact_state": 1,
    "rows": [
      [
        1,
        12
      ]
    ]
  },
  {
    "stage": "after_finish",
    "trancount": 0,
    "xact_state": 0,
    "rows": [
      [
        1,
        12
      ]
    ]
  }
]
```

**Почему различаются:** late_point находится после +3, поэтому откат к нему сохраняет 15. early_point находится до +3 и восстанавливает 12. Обе альтернативы оставляют @@TRANCOUNT=1, так что верный счётчик сам по себе не доказывает верную границу.

**Контрпример/граница вывода:** Если требовалось отменить +3 вместе с +5, правильной точкой была бы early_point; этот запрос явно требует сохранить +3.

### DOC-SQL-SAVEPOINT.REF.05 — Отмена INSERT/DELETE и продолжение записи

Сценарное family: `v4.ref.sql.savepoint.rowset-continue`. Общий origin: `reference/DOC-SQL-03/003`. Статус oracle: `NOT_RUN`; result_observed: `false`.

**Полное условие ученику:**

````text
Авторская самодостаточная симуляция. T-SQL / SQL Server 2019. Свежое отдельное соединение: до первой строки @@TRANCOUNT=0 и XACT_STATE()=0; внешней, ambient или distributed транзакции нет. IMPLICIT_TRANSACTIONS OFF, XACT_ABORT OFF, ANSI_NULLS ON. Используется только локальная временная #S; она создана и заполнена до первого BEGIN TRANSACTION. Триггеров, внешних ключей, дополнительных CHECK, ошибок, обрывов и конкурентных соединений нет. Все Id различны, все int-операции допустимы. Нет скрытых statement или вызовов. После SELECT со stage следующий SELECT возвращает rows как пары [Id, Val] по возрастанию Id. trancount — значение @@TRANCOUNT; xact_state — XACT_STATE(): 0 означает отсутствие транзакции, 1 — активную транзакцию, допускающую COMMIT.

Фактически выполняется следующий полный script:
```sql
SET NOCOUNT ON;
SET IMPLICIT_TRANSACTIONS OFF;
SET XACT_ABORT OFF;
SET ANSI_NULLS ON;
CREATE TABLE #S (Id int NOT NULL PRIMARY KEY, Val int NOT NULL);
INSERT #S (Id, Val) VALUES (1,10),(2,20);
BEGIN TRANSACTION;
UPDATE #S SET Val=Val+1 WHERE Id=1;
SAVE TRANSACTION kept;
DELETE #S WHERE Id=2;
INSERT #S (Id, Val) VALUES (3,30);
-- control
ROLLBACK TRANSACTION kept;
SELECT N'before_finish' AS stage, @@TRANCOUNT AS trancount, XACT_STATE() AS xact_state;
SELECT Id, Val FROM #S ORDER BY Id;
UPDATE #S SET Val=Val+2 WHERE Id=2;
IF @@TRANCOUNT > 0 COMMIT TRANSACTION;
SELECT N'after_finish' AS stage, @@TRANCOUNT AS trancount, XACT_STATE() AS xact_state;
SELECT Id, Val FROM #S ORDER BY Id;
```

Опиши наблюдаемое состояние на каждом явно напечатанном stage именно этого script. Не исправляй SQL и не заменяй управляющий statement. Ответ — только JSON-массив объектов в порядке stage; каждый объект имеет ровно stage, trancount, xact_state и rows. rows — упорядоченный массив пар [Id, Val].
````

**Предпочтительный ответ:**

```json
[
  {
    "stage": "before_finish",
    "trancount": 1,
    "xact_state": 1,
    "rows": [
      [
        1,
        11
      ],
      [
        2,
        20
      ]
    ]
  },
  {
    "stage": "after_finish",
    "trancount": 0,
    "xact_state": 0,
    "rows": [
      [
        1,
        11
      ],
      [
        2,
        22
      ]
    ]
  }
]
```

**Правдоподобный ошибочный ответ:**

```json
[
  {
    "stage": "before_finish",
    "trancount": 0,
    "xact_state": 0,
    "rows": [
      [
        1,
        10
      ],
      [
        2,
        20
      ]
    ]
  },
  {
    "stage": "after_finish",
    "trancount": 0,
    "xact_state": 0,
    "rows": [
      [
        1,
        10
      ],
      [
        2,
        22
      ]
    ]
  }
]
```

**Почему различаются:** Откат к kept отменяет как DELETE, так и INSERT после точки, сохраняя более ранний UPDATE. Затем новый UPDATE выполняется внутри оставшейся активной транзакции. После полного rollback префикс был бы потерян, а поздний UPDATE при IMPLICIT_TRANSACTIONS OFF выполнился бы отдельно в autocommit.

**Контрпример/граница вывода:** Одинаковое конечное Val=22 у Id=2 не делает альтернативы эквивалентными: состояние Id=1 и @@TRANCOUNT в before_finish различаются. Если требовался полный откат с отдельной последующей записью, договор был бы другим.

### DOC-SQL-SAVEPOINT.REF.06 — Savepoint внутри двух BEGIN и счётчик завершения

Сценарное family: `v4.ref.sql.savepoint.nested-count`. Общий origin: `reference/DOC-SQL-03/003`. Статус oracle: `NOT_RUN`; result_observed: `false`.

**Полное условие ученику:**

````text
Авторская самодостаточная симуляция. T-SQL / SQL Server 2019. Свежое отдельное соединение: до первой строки @@TRANCOUNT=0 и XACT_STATE()=0; внешней, ambient или distributed транзакции нет. IMPLICIT_TRANSACTIONS OFF, XACT_ABORT OFF, ANSI_NULLS ON. Используется только локальная временная #S; она создана и заполнена до первого BEGIN TRANSACTION. Триггеров, внешних ключей, дополнительных CHECK, ошибок, обрывов и конкурентных соединений нет. Все Id различны, все int-операции допустимы. Нет скрытых statement или вызовов. После SELECT со stage следующий SELECT возвращает rows как пары [Id, Val] по возрастанию Id. trancount — значение @@TRANCOUNT; xact_state — XACT_STATE(): 0 означает отсутствие транзакции, 1 — активную транзакцию, допускающую COMMIT.

Фактически выполняется следующий полный script:
```sql
SET NOCOUNT ON;
SET IMPLICIT_TRANSACTIONS OFF;
SET XACT_ABORT OFF;
SET ANSI_NULLS ON;
CREATE TABLE #S (Id int NOT NULL PRIMARY KEY, Val int NOT NULL);
INSERT #S (Id, Val) VALUES (1,30);
BEGIN TRANSACTION;
UPDATE #S SET Val=Val+4 WHERE Id=1;
BEGIN TRANSACTION;
SAVE TRANSACTION kept;
UPDATE #S SET Val=Val+9 WHERE Id=1;
-- control
ROLLBACK TRANSACTION kept;
SELECT N'before_finish' AS stage, @@TRANCOUNT AS trancount, XACT_STATE() AS xact_state;
SELECT Id, Val FROM #S ORDER BY Id;
IF @@TRANCOUNT > 1 COMMIT TRANSACTION;
SELECT N'after_inner_commit' AS stage, @@TRANCOUNT AS trancount, XACT_STATE() AS xact_state;
SELECT Id, Val FROM #S ORDER BY Id;
IF @@TRANCOUNT > 0 COMMIT TRANSACTION;
SELECT N'after_finish' AS stage, @@TRANCOUNT AS trancount, XACT_STATE() AS xact_state;
SELECT Id, Val FROM #S ORDER BY Id;
```

Опиши наблюдаемое состояние на каждом явно напечатанном stage именно этого script. Не исправляй SQL и не заменяй управляющий statement. Ответ — только JSON-массив объектов в порядке stage; каждый объект имеет ровно stage, trancount, xact_state и rows. rows — упорядоченный массив пар [Id, Val].
````

**Предпочтительный ответ:**

```json
[
  {
    "stage": "before_finish",
    "trancount": 2,
    "xact_state": 1,
    "rows": [
      [
        1,
        34
      ]
    ]
  },
  {
    "stage": "after_inner_commit",
    "trancount": 1,
    "xact_state": 1,
    "rows": [
      [
        1,
        34
      ]
    ]
  },
  {
    "stage": "after_finish",
    "trancount": 0,
    "xact_state": 0,
    "rows": [
      [
        1,
        34
      ]
    ]
  }
]
```

**Правдоподобный ошибочный ответ:**

```json
[
  {
    "stage": "before_finish",
    "trancount": 1,
    "xact_state": 1,
    "rows": [
      [
        1,
        43
      ]
    ]
  },
  {
    "stage": "after_inner_commit",
    "trancount": 1,
    "xact_state": 1,
    "rows": [
      [
        1,
        43
      ]
    ]
  },
  {
    "stage": "after_finish",
    "trancount": 0,
    "xact_state": 0,
    "rows": [
      [
        1,
        43
      ]
    ]
  }
]
```

**Почему различаются:** ROLLBACK к savepoint не завершает уровень BEGIN: счётчик остаётся 2, а suffix +9 отменяется. Ошибочный COMMIT уменьшает счётчик до 1 и сохраняет +9; условный последующий COMMIT >1 тогда пропускается. Вложенный BEGIN здесь не объявляется независимой транзакцией.

**Контрпример/граница вывода:** Если требовалось сохранить +9 и закрыть внутренний уровень, COMMIT был бы подходящим оператором. Здесь явно нужны отмена +9 и @@TRANCOUNT=2 в первом наблюдении.

## Последующий учебный пилот и отчёт

После подготовки и независимой приёмки следующий учебный запуск связывает получателя с [05-LEARNING-AND-CHECKPOINTS.md](../../05-LEARNING-AND-CHECKPOINTS.md) и [10-NATIVE-CONTRACT-BOUNDARY.md](../../10-NATIVE-CONTRACT-BOUNDARY.md). Не выдумывать native runtime по domain_id. Перед A/B восстановить один совместимый ненулевой B1: веса, optimizer, RNG, tokenizer/family map, registry и split hashes. Путь B1 указан выше; если его нет, отдельный bootstrap-план создаёт и сохраняет переиспользуемое состояние, а не скрыто переобучает перед каждым сравнением.

Измерить before/after на новых закрытых семействах: правильный полный trace DOC или удовлетворение заданному trace у CODE, удержание освоенных контрольных сценариев, число уроков/время до первого полезного результата и ресурсы участвующего учебного пути. На TRAIN/DEV обеспечить ненулевую освоенную способность; 0→0 не принимать за эксперимент, margin/loss не заменяет её. Любое измеренное улучшение полезного показателя — успех именно этого показателя/домена; точность не обязана вырасти, а ухудшения остальных метрик показываются рядом. Измеренное улучшение заносится кандидатом в исправления, без автоматического изменения production.

## Порядок перехода и ожидание

Каноническая следующая запись определяется по `DOC-SQL-SAVEPOINT/base` в [execution-queue.json](../../execution-queue.json); локальный curriculum не заменяет полную очередь. Числовой next здесь намеренно не дублируется. Прямой NEXT с ID пользователя имеет приоритет: **перейти немедленно и не продолжать предыдущую задачу**. NEXT без ID использует следующую строку полной очереди. STOP прекращает новые действия сразу. Старые эксперименты/версии сохраняются.

Если процесс уже идёт, найти доступный native wait/resume, сохранить один handle и дождаться terminal в текущем рабочем раунде; не заканчивать ответ вместо ожидания, не делать частый пустой polling, не создавать повторный запуск/монитор, не объявлять сбой только потому, что первый файл ещё не появился. Во время реального bounded wait можно выполнять независимую разрешённую подготовку и сообщать краткий статус. Если процесса нет, PENDING selection/oracle не превращается в искусственный цикл ожидания.

<!-- GENERATED-QUEUE-START -->
## Переходы единой очереди

Этот блок воспроизводится из общей очереди и задаёт текущий запуск. Прежние команды/номера в основном тексте сохраняются как история. Уже начатую попытку продолжать по её фактическому request/state, не запускать повторно. Прямой ID выше порядка. При NEXT немедленно перейти к названному ID; запрещено продолжать прежнюю задачу или дописывать её отчёт. При STOP немедленно прекратить действия. Живой процесс ждать через его реальный handle до terminal; не завершать раунд вместо ожидания.

Канонический обзор — [EXECUTION-QUEUE.md](../../EXECUTION-QUEUE.md); [бюджет](../../curriculum/scale-plan.json) делится внутри родительского пула, пилот входит в него. Выбранная модель не переопределяется. Checksum, фиксированный профиль модели и production CI не являются условиями подготовки. Режим CREATE_ONLY: запись draft.jsonl → finish → следующий поток; никаких review/oracle/accept/export/обучения.

| Поток | Общий порядок | Следующий поток |
|---|---:|---|
| `DOC-SQL-SAVEPOINT/base` | 118 | `CODE-SQL-SAVEPOINT/base` |

## Точные команды действующих потоков

Подготовка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Команды работают из любого каталога. RAG уже внутри пакета. Выполнить только назначенный поток; остальные строки — точные альтернативы назначения.

### DOC-SQL-SAVEPOINT/base

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1" -Stream "DOC-SQL-SAVEPOINT/base" -Run "DS401" -Count 12 -Split train -Start 0
```

DEV/EXAM не запускать попутно с TRAIN. Когда назначено отдельное создание этих split в изолированном контексте, точные команды сохранены в `curriculum/pilot-queue.json` как `prepare_first_dev` / `prepare_first_exam`.

Первая TRAIN-папка: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/DOC-SQL-SAVEPOINT/base/DS401`. Следующий поток: `CODE-SQL-SAVEPOINT/base`. Доля TRAIN: 500 пар; первая партия 12. Пилот TRAIN/DEV/EXAM: 120/24/24; набор `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/datasets/DOC-SQL-SAVEPOINT-base-v1`; B1 `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/baselines/docs.sql/base/DOC-SQL-SAVEPOINT-v1/B1`. Это назначенные пути, не существующие веса.

<!-- GENERATED-QUEUE-END -->
