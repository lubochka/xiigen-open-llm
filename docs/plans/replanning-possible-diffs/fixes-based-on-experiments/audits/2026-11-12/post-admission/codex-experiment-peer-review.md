# Независимая проверка экспериментального дополнения

Проверяющий: `/root/transfer_audit`; 12 ноября 2026. Проверка документов и исходников, без native запуска и без правок чужого пакета.

Прочитаны `codex-post-admission-experiment-review.md`, `codex-post-admission-experiments.py`, manifest и три новых вставки в planning. Выполнен скрипт по закреплённым входам в новый файл `experiment-audit/peer-transfer-experiment-recheck.json`. На варианте с 29 входами результат JSON точно совпал с опубликованным локальным результатом автора.

Подтверждены P48: 12/16 у обеих рук, три gains и три losses; P49: названный выигрыш B — известная new-TRAIN пара; P52: cold 0/4, constant ceiling 2/4, answer 4/4 в обеих руках при tense 1/4→3/4; P58: во всех трёх руках 1/1 на corrected строке e1, 0/7 на остальных literal sources. Семь строк не квалифицированы как независимые source families; future alias-ledger не выдаётся за исполненную проверку.

Лично просмотрены training-bearing Facts и helper-вызовы. Дополнительно прочитаны три producer helper на том же core f089a7d:

- `27.TranslationDomain/TranslationStructuredBatchBuilder.cs`, blob `e0f8fb71e1c53059a225ab2562a789222266c463`: три `examples.Add` на pair, строки 146/164/176.
- `experiments-v2/RP01-MICRO/P5801/src/CorrectiveLessonCounterExampleArms.cs`, blob `82e37c8bf2ae126b8d466e4e8593f82297a4c508`: передаёт одну corrective pair в общий builder.
- `experiments-v2/RP01-MICRO/P5201/src/RejectedAnnotationAddressArms.cs`, blob `9b450ae67bac8e2b94409cf52b6c4d66d7564501`: меняет поле аннотации, сохраняя число pairs.

Таким образом, 261 Unit lesson calls /104 direct output reads для полного P58 class и 36/28 для P52 подтверждаются при явно заявленном условии: каждый нынешний Fact один раз, полное успешное выполнение, без retries. Это не сумма исторического процесса, не native discovery, не число уникальных обучающих пар и не измерение полного процессного расхода. Эти границы в отчёте сохранены.

Проверены original prefix и новые SHA256 трёх planning-файлов: все исходные байты сохранены. Содержательного возражения, требующего остановить публикацию, не найдено. Единственное уточнение воспроизводимости передано автору: три helper должны войти в manifest, иначе изменение только producer не будет обнаружено скриптом с неизменными test files. Автору переданы файлы и SHA; эта записка фиксирует рассмотренную версию, не заявляет выполненный повтор после его будущих изменений.

## Дополнение 12 ноября 2026: повторная проверка v2

Замечание о неполной привязке множителей устранено. Прочитана новая функция `pinned_cardinalities`; три helper присутствуют в manifest с теми же Git blob SHA, которые проверяющий независимо прочитал ранее. Все 32 входа проверены при повторе. Множители теперь выводятся из закреплённых успешных путей: 3 Unit examples на pair; P58 setup 8×3=24, correction 1×3=3, всего 27 на плечо; P52 2×3=6 на плечо. Проверка формы ограничена этими исходниками и не выдаётся за C# исполнение или подсчёт всех вложенных runtime операций.

Выполнено: `python3 codex-post-admission-experiments.py --core core --output peer-transfer-experiment-recheck-v2.json` из каталога experiment-audit. Новый файл создан отдельно. Он побайтово совпал с `codex-post-admission-recomputed-v2.json`; суммы остались P58 261/104 и P52 36/28. Исторический локальный v1 `codex-post-admission-recomputed.json` побайтово совпадает с первым независимым peer-output: прежний результат сохранён.

P58 теперь явно использует `other_literal_source_*` и `partition_rule`: разделение только по буквальному `Source` относительно одной corrective строки. Из него не выводится отсутствие нормализованных aliases, одинаковых model keys или родственных source families. Таким образом, прежняя неточность названия не переносится в итоговый вывод. Приёмка alias-ledger остаётся будущей; новые native опыты не выполнялись.

Итог ограниченного повторного review: замечание о трёх producer pins закрыто, прежние оговорки сохранены, новых препятствий к публикации этого документного пакета не выявлено. Planning-файлы проверяющим не изменялись.
