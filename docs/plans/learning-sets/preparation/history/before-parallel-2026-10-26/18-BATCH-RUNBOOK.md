# Команды текущей кампании DPO

Режим CREATE_ONLY, без review. [Реестр](campaign/registry.json) задаёт21домен×1000. Инструкции: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`; новые наборы: `C:/Projects/llm_mvp_core/learning-sets`. Текущая рабочая папка не меняет эти два корня.

## Новый запуск или продолжение кампании

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_campaign.ps1" -Action next -WorkRoot "C:/Projects/llm_mvp_core/learning-sets"
```

Если в кампании уже есть незаконченный request, команда вернёт его пути и не создаст дубль. После создания назначенных строк используй **finish, затем next из выданного commands.md**. Их нельзя заменять командой первой партии. Частичный draft сохраняется, а завершённая доля переходит к следующему A-ID.

## Прямо назначенная карточка

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_campaign.ps1" -Action next -Allocation A001 -WorkRoot "C:/Projects/llm_mvp_core/learning-sets"
```

A001 показан как конкретный первый ID. Если пользователь назвал другой ID, использовать точное соответствие из [очереди](EXECUTION-QUEUE.md). Не продолжать старый отчёт после NEXT. STOP прекращает действия.

## Состояние

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/tools/create_campaign.ps1" -Action status -WorkRoot "C:/Projects/llm_mvp_core/learning-sets"
```

Вывод содержит фактически созданные кандидаты, остаток по каждому домену, текущий request и общий остаток. CAMPAIGN_DRAFTS_CREATED означает21 000 созданных кандидатов; это не приёмка качества и не обученная модель.

## Конкретный существующий request

Нужный путь уже находится в выводе next и в commands.md. `create_campaign.ps1 -Action resume -Request <точный существующий абсолютный путь>` продолжает именно его. Для прежнего формата xiigen.batch-request.v1 runner возвращает команду старого batches.py resume; сами файлы и metadata не переносятся. Старые сохранённые command/request приоритетнее примеров новых запусков.

Старый `create_sets.ps1` оставлен для прямо назначенной legacy-партии. Его prepare тоже использует отдельный C:/Projects/... output root или явный WorkRoot. Он не является новой очередью21000. [Архив прежнего runbook](history/before-domain-campaign-2026-10-25/18-BATCH-RUNBOOK.md) сохранён целиком; blind/accept/export из архива не выполняются в CREATE_ONLY.

## Семейства и запись

Материал семей создаётся в family-material.jsonl только по назначенным IDs. material.required.json — перечень требуемого, а не готовые факты. Для существующих семей используй выданный материал; менять сюжет под старым ID нельзя. Точные class assignments не заменяются самостоятельно придуманной смесью. Источники и готовые DPO-примеры включены в prompt.

Reason internally in English; do not output private reasoning. Модель — текущая китайская Kiro, отчёт — по-русски, текст ученика — по профилю. Конкретные проверки буквы/формата выполняются при составлении. Независимый review, checksum, экспорт, обучение и production CI не запускаются.

Для живой асинхронной операции используй документацию фактически доступного инструмента, один handle и штатное ожидание до terminal. Зависимые чтения — после завершения записи. Не стартовать второй запуск и не объявлять transport yield отказом.
