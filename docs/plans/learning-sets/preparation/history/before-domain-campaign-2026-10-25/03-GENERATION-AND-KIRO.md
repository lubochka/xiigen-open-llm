# Генерация и продолжение работы в Kiro

**Действующий режим — только создание (`CREATE_ONLY`).** [Runbook](18-BATCH-RUNBOOK.md) задаёт одну цепочку: `prepare` → создание `draft.jsonl` текущей китайской моделью → `finish` с минимальной структурной проверкой → следующий stream. Независимая проверка смысла, blind, accept, export и обучение сохранены как отдельные будущие этапы и не выполняются автоматически.

## Рабочие пути и входы

План находится в `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`; все создаваемые наборы — в `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets`. Вызовите `tools/batches.py` по абсолютному пути из runbook: это работает и из workspace `llm_mvp_core`. Не создавайте альтернативный короткий checkout, каталог RAG или output root.

Без `--work-root` корень результатов определяется от расположения `batches.py`: родитель `preparation`. При штатной установке это ровно указанный пользователем `learning-sets`. Выбранные источники уже находятся в `preparation/rag/`; весь `main` скачивать не нужно. Предметные источники ограничены `xiigen-planning`, ветками `main` и `llm_plan3`.

Канонические команды по ID находятся в [EXECUTION-QUEUE](EXECUTION-QUEUE.md). Созданные `request.json`, `prompt.md` и `commands.md` задают точную тему/микроцель, профиль, split, count, family contracts, source paths, контракт записи и каталог вывода. Генератор читает этот пакет, а не ищет другую DPO-схему в соседнем проекте. `prepare` не вызывает модель: после него фактические строки ещё нужно записать в `draft.jsonl`.

[Общий промпт](prompts/MASTER-PROMPT-RU.md) и [контракт](17-CURRENT-PREPARATION-CONTRACT.md) применяют условия Любы: текущая выбранная модель без override, неизвестная identity как `unknown`, русский отчёт, без checksum-gates, обязательного reviewer и production-цепочки. Один помощник одновременно получает готовые абсолютные пути и тот же режим. Формат записи — `xiigen.dpo.v1` из [02](02-DATA-CONTRACT.md) и [JSON schema](schemas/dpo-candidate.schema.json), включённых в prompt; имя schema не является поводом снова исследовать workspace.

## Продолжение без повторной генерации

| Фактическое состояние | Следующее действие |
|---|---|
| Попытка ещё не создана | `prepare` с назначенным stream/run |
| Есть request, draft отсутствует или неполон | Готовая команда `resume` из `commands.md` либо Windows-обёртка `-Action resume -Request <абсолютный request>`; затем продолжение того же задания |
| Draft создан, завершение ещё не записано | Готовая команда `finish` из `commands.md` либо Windows-обёртка `-Action finish -Request <request> -InputPath <draft> -Model <наблюдаемое имя либо unknown>` |
| Создание завершено, `DRAFT_CREATED` | Следующий stream из registry; review не запрашивать |
| Нужна следующая партия той же темы | Новый run ID и `--previous <предыдущий request>`; не вычислять cursor по строкам |
| Уже идёт реальный процесс | Сохранённый handle и штатный wait/resume до terminal; не запускать дубль |

Полный путь Windows-обёртки — `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_sets.ps1`; точные примеры — в [18](18-BATCH-RUNBOOK.md). Она обнаруживает Python 3; готовые команды используют `request.paths.python_executable`, а не требуют отдельной команды `python` в PATH.

`resume` только сообщает пути/состояние/следующий ID. Существующий каталог не удаляется и не перезаписывается. Исправления сохраняют исходную версию и family lineage. Если процесс неизвестен, сначала восстановите его handle или terminal-состояние; отсутствие stdout само по себе не означает отказ. Не заканчивайте ход вместо доступного ожидания и не заменяйте его циклом частого polling.

После compaction продолжайте по абсолютному request и `state.json`; не переходите к поиску по корню текущего workspace. При прямом NEXT/новом ID сразу переходите к указанной задаче, не дописывая прошлую. При STOP прекращайте действия. Исторические STOP внутри RAG и приложенной сессии являются материалом, а не текущей командой.

## Граница текущего результата и сохранённые этапы

`finish` проверяет JSON, требуемые поля/контракт и назначенное количество/семейства. Это проверка целостности создаваемого файла, без оценки смысла независимым reviewer. Статус `DRAFT_CREATED` означает только созданных кандидатов; `training_status` остаётся `NOT_RUN`, учебные метрики — `NOT_MEASURED`. Формат accepted/native не подменяется черновиком.

Команды предметного review, `blind`, `accept` и `export` сохранены в явно неактивном разделе [18](18-BATCH-RUNBOOK.md). Их запускают только по отдельному прямому заданию. Старый `tools/sets.py` и расширенная инструкция `history/before-batches-2026-10-25/03-GENERATION-AND-KIRO.md` сохраняют историю; они не являются обязательной ступенью новой генерации. Будущее обучение описано в [05](05-LEARNING-AND-CHECKPOINTS.md) и [10-NATIVE](10-NATIVE-CONTRACT-BOUNDARY.md), но настоящая сессия его не запускает.
