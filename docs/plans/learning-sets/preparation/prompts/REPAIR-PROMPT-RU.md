# Исправить один дефект кандидатной партии

> **Только для отдельно назначенного будущего repair.** CREATE_ONLY не открывает этот цикл после draft: выполнить finish и перейти к следующему потоку по [MASTER](MASTER-PROMPT-RU.md). Текущий открытый аудит данных и чтение этой страницы не разрешают запуск check, blind, accept или oracle. Команды ниже — инструкция на случай отдельного назначения этой процедуры.

Действующая дорожка — [18-BATCH-RUNBOOK](../18-BATCH-RUNBOOK.md) и `tools/batches.py`. Старый текст с receipts сохранён [в истории](../history/before-batches-2026-10-25/prompts/REPAIR-PROMPT-RU.md). Обязательные checksums, профили и квитанции провайдера для этой подготовки не нужны.

Входы: исходные draft.jsonl, request.json, полученный review.json, вывод check и та же карточка. Исправляй только назначенную категорию ошибки. Сохрани **полный** draft.v2.jsonl с тем же count и family coverage. Неизменённые строки сохраняют pair_id; исправленные получают новый pair_id и metadata.derived_from. Split, family_id, origin_family_id и leakage_group_ids не меняются для обхода проверки. Изменение состава требует нового назначенного request, а не скрытой правки старого.

Назови конкретную причину отклонения. Удаление [WRONG] не делает плохой rejected содержательным: обе стороны должны решать одно задание. При tie нужен обоснованный видимым контекстом контраст, а не произвольная смена меток. При недостатке контекста добавь факты в prompt. Неразрешённые случаи остаются в полном черновике с причиной; пакет пока не принят. Исходные файлы не удаляй и не перезаписывай.

При отдельно назначенном repair проверь draft.v2.jsonl с тем же request и создай новый blind **всей версии**; прежний review не копируется как новая проверка. Пример использует существующий библиотечный M001 по [20](../20-CREATE-ONLY-AND-PATHS.md), а не другую legacy-партию с тем же run ID в core. Пути абсолютные, текущий каталог не важен. Интерпретатор и tools script берутся только из сохранённого request, без поиска:

```powershell
$repairRequestPath = "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/TR07-OMISSION/ru-en/M001/request.json"
$repairRequest = Get-Content -LiteralPath "$repairRequestPath" -Raw | ConvertFrom-Json
$repairPython = [string]$repairRequest.paths.python_executable
$repairTools = [string]$repairRequest.paths.tools_script
& "$repairPython" "$repairTools" check --input "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/TR07-OMISSION/ru-en/M001/draft.v2.jsonl" --request "$repairRequestPath"
& "$repairPython" "$repairTools" blind --input "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/TR07-OMISSION/ru-en/M001/draft.v2.jsonl" --request "$repairRequestPath" --out "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/TR07-OMISSION/ru-en/M001/blind.v2.jsonl"
```

При будущем выполнении blind создаст blind.v2.key.json, blind.v2.review.template.json и blind.v2.review.prompt.md. Отдельный reviewer получает только blind, его prompt и назначенные источники и записывает review.v2.json. Ключ, исходные chosen/rejected и rationale ему не передаются. Evidence по [BATCH-REVIEW](BATCH-REVIEW-PROMPT-RU.md) относится к новой проверяемой версии; технический статус не подменяет реальный результат. Следующий блок выполняется только после фактического завершения назначенного review. Он заново читает пути из request, поэтому работает и в новой PowerShell-сессии:

```powershell
$repairRequestPath = "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/TR07-OMISSION/ru-en/M001/request.json"
$repairRequest = Get-Content -LiteralPath "$repairRequestPath" -Raw | ConvertFrom-Json
$repairPython = [string]$repairRequest.paths.python_executable
$repairTools = [string]$repairRequest.paths.tools_script
& "$repairPython" "$repairTools" accept --input "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/TR07-OMISSION/ru-en/M001/draft.v2.jsonl" --request "$repairRequestPath" --blind "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/TR07-OMISSION/ru-en/M001/blind.v2.jsonl" --key "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/TR07-OMISSION/ru-en/M001/blind.v2.key.json" --review "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/TR07-OMISSION/ru-en/M001/review.v2.json" --out "C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/TR07-OMISSION/ru-en/M001/accepted.v2.jsonl"
```

Для другого stream/run использовать его реальные пути из request/commands. Если прежняя версия уже accepted, экспорт явно выбирает одну версию; не включать обе как независимые примеры. Межпартийная проверка `--against` включает остальные партии и исключает версию, которую данный снимок заменяет. Все версии сохраняются.

Отчёт по-русски по [08](../08-REPORT-TEMPLATE.md). До/после здесь относится к качеству данных; обучение Student — NOT_RUN. Текущая модель не меняется, production не исправляется. При NEXT/новом ID запрещено продолжать этот ремонт, отчёт или запуск; сразу перейти к указанной задаче. При STOP прекратить действия. Живой oracle ожидается по единственному штатному handle/wait до terminal без частого polling и без завершения хода вместо ожидания.
