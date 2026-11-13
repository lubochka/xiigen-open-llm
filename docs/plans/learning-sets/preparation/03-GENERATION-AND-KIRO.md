# Генерация и продолжение работы в Kiro

<!-- DOMAIN-CAMPAIGN-AUTHORITY -->
> **Текущая генерация:** [1000 на каждый из 24 доменов — 24 000 пар](campaign/registry.json), CREATE_ONLY, без отдельного review генератора. Новые данные: `C:/Projects/llm_mvp_core/learning-sets`; инструкции: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Ниже сохранён справочный материал прежних партий/стадий. Старые квоты, очереди и требования будущей приёмки не являются действующими командами. Активный запуск — tools/create_campaign.ps1; прежний сохранённый request продолжается по [порядку resume](18-BATCH-RUNBOOK.md#продолжение-next-и-занятость).
<!-- /DOMAIN-CAMPAIGN-AUTHORITY -->

**Действующий режим — только создание (`CREATE_ONLY`).** [Runbook](18-BATCH-RUNBOOK.md) задаёт цепочку координатора: `next` (либо точный `resume` выбранной попытки) → создание `draft.jsonl` текущей китайской моделью и выданный `progress` → выданный `finish` → выданный `next`. Команда `prepare` относится к прежнему batch-runner. Независимая проверка смысла, blind, accept, export и обучение сохранены как отдельные будущие этапы и не выполняются автоматически.

## Рабочие пути и входы

План находится в `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`; новые наборы D1000 — в `C:/Projects/llm_mvp_core/learning-sets`. Используй точную стартовую команду `tools/create_campaign.ps1` из runbook; последующие команды возвращает координатор. Старый явно выбранный request сохраняет свои пути и продолжается по правилам resume, не переносится автоматически. Не создавай альтернативный checkout, RAG или output root.

В новом запуске WorkRoot указан явно как `C:/Projects/llm_mvp_core/learning-sets`; при resume корень берётся из сохранённого request. Выбранные источники уже находятся в `preparation/rag/`; весь `main` скачивать не нужно. Прежнее вычисление output root от расположения `batches.py` относится только к отдельно назначенной legacy-дорожке и не задаёт путь D1000.

Назначения по A-ID находятся в [EXECUTION-QUEUE](EXECUTION-QUEUE.md); машинная квота — в `campaign/registry.json`. Сохранённый `request.json` задаёт тему, профиль, split, count, row_slots и исходники. Текущие `paths.prompt`, `paths.material_required`, `paths.commands` ведут к coordination-файлам владельца; старые prompt/commands не заменяют их. Генератор читает назначенный пакет, не ищет другую DPO-схему. `next` и `resume` не вызывают модель: фактические строки автор пишет в назначенный `draft.jsonl`.

[Общий промпт](prompts/MASTER-PROMPT-RU.md) и [контракт](17-CURRENT-PREPARATION-CONTRACT.md) применяют условия Любы: текущая выбранная модель без override, неизвестная identity как `unknown`, русский отчёт, без checksum-gates, обязательного reviewer и production-цепочки. Один помощник одновременно получает готовые абсолютные пути и тот же режим. Формат записи — `xiigen.dpo.v1` из [02](02-DATA-CONTRACT.md) и [JSON schema](schemas/dpo-candidate.schema.json), включённых в prompt; имя schema не является поводом снова исследовать workspace.

## Справка только для legacy xiigen.batch-request.v1

Таблица и команды этого раздела относятся к прежнему `batches.py` и явно выбранной последовательной legacy-попытке. Для D1000 не выполнять `prepare`, `create_sets.ps1`, новый run или `--previous` из этой справки: использовать [три случая resume и текущий цикл координатора](18-BATCH-RUNBOOK.md#продолжение-next-и-занятость). Старые файлы и команды сохранены как история, а не второй действующий путь D1000.

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

Предметные review, `blind`, `accept` и `export` выполняются только по отдельному прямому заданию с назначенным контрактом и командами. Текущий [18](18-BATCH-RUNBOOK.md) описывает CREATE_ONLY и не содержит обещанного прежними версиями раздела приёмки. Старый `tools/sets.py` и инструкция `history/before-batches-2026-10-25/03-GENERATION-AND-KIRO.md` сохраняют историю; они не являются обязательной ступенью D1000. Будущее обучение описано в [05](05-LEARNING-AND-CHECKPOINTS.md) и [10-NATIVE](10-NATIVE-CONTRACT-BOUNDARY.md), но настоящая сессия его не запускает.
