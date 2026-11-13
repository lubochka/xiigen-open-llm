# Очередь создания — текущая кампания

Актуальные 217 назначений находятся в [EXECUTION-QUEUE.md](EXECUTION-QUEUE.md), машинные квоты — в [campaign/registry.json](campaign/registry.json). 24 домена × 1000 = 24 000 кандидатных пар. Не запускай из старого `generation-queue.json` новые M001 поверх текущей кампании.

Три готовых промпта: [переводы/опечатки](prompts/PARALLEL-TRANSLATIONS-RU.md), [оркестрация/prompt](prompts/PARALLEL-ORCHESTRATION-RU.md), [код/документация](prompts/PARALLEL-SOFTWARE-RU.md). Каждый использует уникальный agent ID, общие state/status, затем точные owner/token-команды координатора.

Исторический JSON-каталог сохраняется для явно выбранной старой попытки. Её пути, IDs и данные не переносятся и не засчитываются автоматически. Самостоятельный legacy runner не участвует в защите текущих параллельных claims.
