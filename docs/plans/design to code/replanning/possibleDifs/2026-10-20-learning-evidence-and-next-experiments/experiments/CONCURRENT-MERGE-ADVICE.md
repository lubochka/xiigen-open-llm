# Ограниченное согласование concurrent planning53cb

> **Исторический срез883/53cb.** На окончательном827 №01–06 завершены, следующий LF-03; P54 blanket запрет снят, NG01 имеет protected карточку и новый host, R1 report уже исправлен. Актуальный статус и оставшиеся gaps — [final827 update](../08-final-core827.md), действующие outputs — [root05](../05-executor-contract.md). Старый R2 wrapper/claims не запускать; этот receipt не назначает повтор.


> Согласование ниже уже применено в root04/root05 и execution-projection.json. Этот receipt не поручает повторять изменения или запускать опыты; действующий реестр — root04,38 ID/34 pending.

Прочитаны ровно пять назначенных GitHub файлов пакета `2026-10-20-learning-delta-audit-883496` на `53cbcabd3926cda714bab428d294eb666301e9e4`: реестр Markdown/JSON, output contract/map и concurrent reconciliation. Их Git blob SHA подтверждены по полученным байтам; manifest включён в [concurrent-merged-registry.json](concurrent-merged-registry.json). Это согласование документов перед публикацией, не новый аудит, не запуск и не исправление production.

## Готовое объединение

JSON содержит `rows` и `outputs`. **38 различных ID, 4 завершённых, 34 pending; NEXT=N62-R2.** Все прежние36 ID и их относительный порядок сохранены. После BPE-REALIZED добавлены GRAD и FROZEN **перед обоими dependent readers**:

**Исходные05–15 → 17–31 → ROPE-TRAIN-TABLE-01 → AST-SIB-CACHE-01 → NG01 → BPE-REALIZED-MERGE-COUNT-01 → GRAD-ACCUM-COPY-01 → FROZEN-BROADCAST-MAP-01 → исходный16 IDENTITY-M1-01 → исходный32 KN-ROUTER-LEARNED-KEY-01 → END.**

Сохранены все32 свежих output assignments, включая28 исходных и ROPE/AST/NG01/BPE. Значения `result_root`, `report`, `candidate`, `entry`, `source_url` у них не изменены. Добавлены только наши AGC1 и FBM1: итого34 outputs, все roots различны. SKNN остаётся `REFERENCE_ONLY_NO_EXTRA_RUN` alias AST. Старое имя `NG01-null-guard-label` — alias canonical NG01, не39-я строка.

## Что заменить в неопубликованном пакете

1. Root04/projection и все утверждения о действующей очереди использовать из merged `rows`. IDENTITY больше не стоит внутри прямого обучения. В обоих learning-файлах и shared contract убрать прежнюю последовательность05–31→GRAD→FROZEN→NG01; теперь GRAD→FROZEN→IDENTITY, NG01 стоит перед BPE-REALIZED. Для FROZEN явный NEXT теперь IDENTITY-M1-01 в назначенной фазе.
2. Root05/overlay используют canonical `outputs`, **без прежнего нового C:/xg/RP01 набора**. N62-R2 root теперь `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N62-R2/results`, без `/N62-R2-01`. Его единственная внешняя команда сохранена: `powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N62-R2/Run-With-Learning-Signal.ps1`. Не запускать рядом внутреннюю CLI второй раз.
3. NG01 сохраняет `C:/xg/NG01/r/attempt-0001`, а не прежний наш `/results`. ROPE/AST/NG01 сохраняют `candidate-fix.md`; Wiki28/29 — `report.ru.md`. Не заменять эти имена общим шаблоном. AST `<Planning>` — заданный sibling `xiigen-planning` относительно уже открытого SourceCore; это назначенный алгоритм пути, не произвольный поиск.
4. Старые host-owned claims/blueprint/manifests остаются в canonical host. **Не назначать конкурирующий общий claim и не создавать R преждевременно.** Наши AGC1/FBM1 сохраняют собственные `host/execution.claim` и `results/`.
5. В JSON сохранены исходные относительные `card_url/source_url` для доказательства неизменности; для переноса BPE-ссылки в другой пакет брать `resolved_card_url/resolved_source_url`. Новые `learning/...` относительны к корню нашего пакета, что явно указано в JSON.

Первые01–04 и их измерения не повторяются. Default model, ANY useful metric success, регрессии рядом, немедленный NEXT/STOP и protected read-only остаются. Проверка равенства унаследованных assignment-полей дала **ноль изменений**. Runtime-готовность wrappers/SDK/Windows artifacts этим согласованием не подтверждается.
