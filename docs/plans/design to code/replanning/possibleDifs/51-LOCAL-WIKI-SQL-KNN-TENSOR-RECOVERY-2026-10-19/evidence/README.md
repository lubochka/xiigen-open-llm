# Доказательства и воспроизведение

Все сохранённые результаты здесь имеют явно ограниченный смысл. Это анализ точных исходников/исторических JSON и математические контрпримеры. Ни один скрипт не запускает native C#, Roslyn, обучение или внешний model API.

## Реестры

- [sources.json](sources.json):206 проверенных версий первичных исходников, historical receipts, RP01 и проектных навыков; repository/ref/path/blob SHA, размер и способ проверки.
- [prior-corpus.json](prior-corpus.json):1191 версия прежнего каталога поиска; отдельно7 документальных обновлений core649b9343 и10 файлов planning4151916f. Числа относятся к получению/полному поиску, не к полному построчному чтению.
- [latest-planning-compare.json](latest-planning-compare.json): параллельные N50/N51 перед публикацией и обновление общего index; crosswalk — в06.
- [latest-core-compare.json](latest-core-compare.json): подтверждённый docs-only delta текущей core ветки.
- [sql-source-ledger.md](sql-source-ledger.md) и [sql-source-ledger.json](sql-source-ledger.json): точные SQL assertions, read ranges, источники и ближайший prior.
- [knn-source-manifest.json](knn-source-manifest.json), [knn-semantic-parity.json](knn-semantic-parity.json), [knn-novelty-search.json](knn-novelty-search.json), [sql-novelty-search.txt](sql-novelty-search.txt): предметные проверки источников/повторов.

Git blob SHA вычисляется по точным bytes, включая BOM/CRLF. В некоторых временных копиях инструмент добавил последний LF; он удалялся только если это давало известный pinned Git hash. Ни комментарий, ни совпадение имени файла не заменяли проверку. Реестры не требуют доверять изменяемому branch name.

## TensorOps: самостоятельная математика

Из каталога пакета:

    python evidence/shape_proofs.py --output /tmp/wiki-shape-math.json

Такой запуск помечает source_verification_performed=false. Он не утверждает получение первоисточников. Для повторения опубликованного source-verified результата подготовить два checkout точных refs: snapshots/core=C4945b8e8 и snapshots/wiki=W219d7ba8. Затем:

    python evidence/shape_proofs.py --source-root /path/to/snapshots --output /tmp/wiki-shape-verified.json

Четыре требуемых файла и полные ref/blob SHA встроены в скрипт; оригинальные line endings должны сохраняться. [Сохранённый результат](shape_proofs.json) получен с проверкой четырёх blobs. Mean использует независимые finite differences; MatMul сравнивается с осевым эталоном и контрольными формами. Независимый reviewer отдельно использовал NumPy: [результат](independent-numeric-source-checks.json).

## KNN: аналитические term bags и provider replies

    python evidence/probe_retrieval.py

Скрипт записывает [probe_results.json](probe_results.json) рядом с собой и печатает результат. Он конструирует term bags, фиксированные scores и аналитические parent/child keys; парсер не запускается. Статистика при повторном ID, отрицательный IDF и empty skill selection проверяются как механизм. Влияние на конкретный ответ после исправления не измеряется.

## SQL: точные архивные файлы и source-equivalent input

Подготовить snapshots/current на commit4945b8e85c17b5f150eb5b840d16bbdc4b506242 и snapshots/wiki на commit219d7ba89dc3ef43e03e17f1461ad96154d3dd7a. Это читаемые checkout, не рабочие ветки реализации. Нужны только10 C# файлов из [units-source-tree.json](units-source-tree.json) и24 JSON/JSONL из [historical-manifests.json](historical-manifests.json); полные пути и ожидаемые hashes перечислены там и в sources.json.

    python evidence/units_probes.py --source-root /path/to/snapshots --out /tmp/wiki-sql-recheck.json

Скрипт читает current/<repo path> и wiki/<repo path>; manifest-файлы берёт рядом с собой. Перед разбором проверяет34 Git blobs. Пересчитывает preference по типам, противоречивые read-only chosen, различия110→113, состав curriculum и синтетическую проекцию evidence. Результат — [units-probe-receipt.json](units-probe-receipt.json). Он совпал с авторским результатом после переноса скрипта в этот пакет.

В текущей synthetic fixture append-only default prefixes **различаются**: поле append_only_at_default_context_still_collides=false. Первоначальная проза была исправлена при независимом review; полные имена записываемых таблиц и версия всё же выходят за48 слов. [Независимый SQL пересчёт](independent-sql-checks.json) фиксирует surviving tokens и позднюю историю.

## Результат проверки

Научный review отделён от авторства: [основной документ07](../07-REVIEW-AND-METHOD.md). Исторические elapsedMs остаются стоимостью записанного тогда DPO runner. Скорость Python не подставляется вместо train/inference latency. Документы с будущими опытами имеют статус PLAN/NOT_RUN; [05](../05-SMALL-DOMAIN-EXPERIMENTS.md) задаёт конкретные входы, конечный consumer, отрицательные контроли и условия опровержения.
