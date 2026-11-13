# Финальная QA: существенные документальные конфликты перед публикацией

20 октября 2026. Проверены публикационные04/05/10/15, новые06–09, их ссылки и согласованность с вновь обнаруженным planning HEAD `9d311be43f4e108f6fa1cda1774549d54941c3c5`. Из concurrent63 прочитаны04-REGISTRY,12-CONCURRENT-RECONCILIATION,14-CORE883-PROTOCOL-DELTA и точные области AST/semantic review. Это source/document review, без build, обучения, внешних записей или изменения output.

**Вердикт текущего readback: перед публикацией требуется только документальная reconciliation нового HEAD.** Технические исправления новых рецептов уже проверены; запуск для этой QA не нужен. Нельзя публиковать старые35-row/два новых фактора как актуальный объединённый статус после9d311.

## 1. Единый маршрут:36 различных пунктов,32 будущих

Последний уже опубликованный [registry63 L7/47–51](https://github.com/lubochka/xiigen-planning/blob/9d311be43f4e108f6fa1cda1774549d54941c3c5/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/63-LEARNING-RESULTS-AND-EXPERIMENT-READBACK-2026-10-20/04-REGISTRY.md#L7) и [delta14 L9–11](https://github.com/lubochka/xiigen-planning/blob/9d311be43f4e108f6fa1cda1774549d54941c3c5/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/63-LEARNING-RESULTS-AND-EXPERIMENT-READBACK-2026-10-20/14-CORE883-PROTOCOL-DELTA.md#L9-L11) откладывают IDENTITY-M1-01 как зависимый reader. Сохраняющий этот маршрут вариант с нашим единственным дополнительным фактором:

**05–15 → 17–31 → S01 ROPE-TRAIN-TABLE-01 → S02 AST-SIB-CACHE-01 → S03 NG01 → S04 BPE-REALIZED-MERGE-COUNT-01 → исходный16 IDENTITY-M1-01 → исходный32 KN-ROUTER-LEARNED-KEY-01 → END.**

Исходные32 пункта + ROPE + AST + ранее добавленный NG01 + новый BPE = **36 distinct rows**. Четыре завершены; **32 pending**, NEXT неизменно **05 N62-R2**. SKNN не добавляет37-й пункт. Номера исходных ID сохраняются; локальные32a/32b из нашего черновика становятся историческими alias, не второй очередью.

Точные переходы для handoff:15→17;31→ROPE;ROPE→AST;AST→NG01;NG01→BPE;BPE→IDENTITY;IDENTITY→KN-ROUTER;KN→END. При назначении только одного ID — END после его назначенной фазы, без автозапуска следующего.

В прочитанном output ещё устарели06:L19,07:L5,08:L5,09:L3,05:L23–24/39–43,10:L38/60 и METHOD-KNN addendum.next. Их нужно согласовать с одним реестром, а не оставить читателю выбирать цепочку. В04 заголовок «Единственная действующая очередь» L42 следует ограничить **источником protected snapshot**, затем явно сослаться на effective05 нового пакета. Число32 исходных строк в историческом аудите04 само по себе верно и не должно механически заменяться36.

## 2. SKNN — duplicate factor, альтернативный рецепт

[AST63 L19–25/51–55](https://github.com/lubochka/xiigen-planning/blob/9d311be43f4e108f6fa1cda1774549d54941c3c5/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/63-LEARNING-RESULTS-AND-EXPERIMENT-READBACK-2026-10-20/07-AST-SIB-CACHE-01.md#L19-L55) уже задаёт тот же per-parent cache sibling count/ordinal при неизменных vectors/distance/labels. Различается учебная точка и рецепт: AST использует `TrainFromSource`, narrow+wide,4Train/128Predict; наш SKNN — facade TrainStructuralKnn,2Train/32logical query. Это **альтернативные протоколы одного фактора**, не два независимых новых открытия и не суммируемый бюджет.

Конкретное решение: существующий S02 AST остаётся canonical scheduled experiment;06/07 сохраняются как подробный альтернативный рецепт, **не активная команда на второй запуск**. В шапке06/07 явно поставить это решение и effective registry. Нельзя объединять cache seams/fixtures/budgets двух recipes в новый третий вариант. Наш source review по6bc4 остаётся верным для своего pin; финальная новизна должна дополнительно учитывать9d311 в README/ledger/audit-index и опубликованном novelty review.

## 3. Output mapping не должен создать второе место попытки того же ID

Наш15 и пять addenda сейчас назначают новый набор `C:/xexp/...`, хотя latest63 уже содержит28 точных разрешённых R и entry-команды на883496. [Delta14 L27–64](https://github.com/lubochka/xiigen-planning/blob/9d311be43f4e108f6fa1cda1774549d54941c3c5/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/63-LEARNING-RESULTS-AND-EXPERIMENT-READBACK-2026-10-20/14-CORE883-PROTOCOL-DELTA.md#L27-L64) уже разрешает старые protected-output конфликты. WT01/WM01 изначально имеют допустимые outputs, а [NG01 current assignment L20–24](https://github.com/lubochka/xiigen-planning/blob/9d311be43f4e108f6fa1cda1774549d54941c3c5/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/63-LEARNING-RESULTS-AND-EXPERIMENT-READBACK-2026-10-20/12-CONCURRENT-RECONCILIATION.md#L20-L24) сохраняет `C:/xg/NG01/r/attempt-0001`.

Решение без замены прежних опубликованных operational документов: перенести в наш current mapping **те же28 R из latest14**; ROPE/AST/NG оставить их existing R. Новый активный путь только у BPE: `C:/xg/br01/r/0001`. SKNN alternative path не становится дополнительным scheduled result-root. Синхронизировать15,output-path-map.json и пять prompt addenda. Report filenames тоже не нормализовать молча: latest11 L64 сохраняет `report.ru.md` для WIKI28/29 и `report.md` остальных.

Полезное уточнение нашего15:L9 сохранить: host-owned early execution.claim/blueprint/source manifests остаются там, где их назначает protocol; result-root не создаётся до controller prepare. Один wrapper, один lifecycle owner, никаких двойных prepare/build/arm launches. Existing issued/partial marker в **любом прежнем назначенном месте этого ID** нельзя обходить выбором нового пути. Уже выполненные01–04 не перемещать и не повторять.

## Проверки, которые уже пройдены

- В04/05/06–10/15 проверены18 относительных Markdown-ссылок: отсутствующих local targets не найдено.
- Публикационные06–09 совпадали с независимо разобранными recipes; изменения относительно notes — только термин домена C# и исправленная ссылка09→08. Rewind/CLI ImportDirectoryBuild flags/BPE pre-build attempt guard и SKNN resumeExactBuild остаются на месте.
- KH-COST learner/FileAction и Q0guard уточнены; P54 не выдан за ready host; measured R1 отделён от будущего R2 и causal claim; первые три результата честно названы report-derived в нашем audit.
- Protected остаётся только для чтения. Новые hosts прямо не выданы за собранные. Любой полезный measured gain, регрессии, default модель, русский отчёт и немедленный STOP/NEXT сохранены.

Это список необходимых изменений документации после внешнего HEAD, а не основание запускать эксперимент, повторять старый результат или переписывать чужой пакет63.
