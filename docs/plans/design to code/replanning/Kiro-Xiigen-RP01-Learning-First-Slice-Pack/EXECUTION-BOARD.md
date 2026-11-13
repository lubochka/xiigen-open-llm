# RP01 — доска actual Main500 и следующего доменного шага

**B500-LIVE-RUN-REPAIR-20261015.** Единственный dispatcher: [RP01-EXECUTE-NOW](RP01-EXECUTE-NOW.md). Canonical tasks: [пять B500 карточек](tasks/batch500/README.md); машинный индекс: [PACKAGE-MANIFEST](PACKAGE-MANIFEST.json). Наличие документа не переводит runtime-задачу в DONE.

## Ближайшее действие определяется состоянием, а не номером

**Сначала B500.03/03T: прочитать актуальный handoff исходного владельца уже выданного `simulation-csharp-main500-20261015-01`.** В core K58 есть offline runner, exact500, C0 artifacts и partial completed-step observation. Это не live-PC status и не итоговое качество. Active/unknown → никакого второго main, calibration, reset или удаления marker. Completed → reuse actual checkpoint/readback, затем только недостающие T/grade.

| Область | Сейчас переиспользовать | Новая работа только при условии |
|---|---|---|
| [01 DATA](tasks/batch500/RP01.B500.01-DATA.md) | Existing C#500, source/J/semantic receipts по exact binding;25templates×20, actual simulation | Новый домен/материал; CODEX S, отдельный A обеих сторон и отдельный CODEX J |
| [02 ADMISSION](tasks/batch500/RP01.B500.02-ADMISSION.md) | Implemented fixed-C# simulation route и exact F1/F2 delta review | Подтверждён новый неподдержанный source/domain contract, один existing-owner adapter |
| [03 TRAIN](tasks/batch500/RP01.B500.03-TRAIN.md) | Calibration и actual Main500 journal/state/stage clocks | Ownership/effect reconciliation; действительно missing readback; следующий один hotspot или supported continuation seam |
| [04 EXAM](tasks/batch500/RP01.B500.04-EXAM.md) | Original frozen25 preference+3 free subset, saved C0/совместимые T | Только отсутствующие original T/independent blind grade; новые rubric/cases отдельно следующему опыту |
| [05 DOMAINS](tasks/batch500/RP01.B500.05-DOMAINS.md) | Actual domain/representation/kNN owners и existing Student states | ENRU consumer+500 новых пар/retention; один выбранный law-aware causal contrast |

Current frozen main:500 pairs, batch10, max50 committed updates,1epoch, fixed B0/reference, wall none. Не добавлять mandatory first50 streaming, 0/50/100/250/500 behavioral curve, DEV10 или40+20 exam. Generic resume и ENRU training в сегодняшней CLI не подтверждены; не изобретать команды.

## Параллельность без лишней нагрузки

03T reconciliation и05A read-only domain preparation независимы. DATA следующего домена не ждёт C# gain и не получает закрытый gold. Выделять тяжёлые PC процессы по реальному ресурсному бюджету: архитектурная независимость не означает одновременное потребление всей RAM двумя trainer. Один mutable-state owner; один writer на пересекающиеся source paths. Read/reuse не требует нового согласования уже разрешённого действия.

Не500 старых train-and-hold и не новый полный цикл R01–R25. Existing references/skills читать по одному вопросу. Исторические one-pair recovery/negative verdict остаются в своей области и не становятся default для current Main.

## Наблюдаемый прогресс

Отдельно: source compatible; real committed step; state saved/reopened; preference change; free-answer semantic change; retention/domain transfer; actual ordinary adoption. Положительная margin не доказывает свободный навык, маленький meaningful gain не доказывает весь домен,0gain не причина перезаписывать критерий. Новый threshold только до следующего опыта.

Время: actual preprocessing/load, train-step total, nested serialization/optimizer-RNG persistence, reopen, scoring/generation, external S/A/J/E и recovery. Не приписывать весь step optimizer и не складывать nested durations. Измерение одного процесса не whole-PC feasibility. Завершённых пар/updates может быть меньше максимума — сообщать actual.

## Не потерять предметный результат

RP02 — domain/intent/version, semantic roles/vector, law-aware positive/negative/warning kNN, learned rerank/k, real consumer/final arbiter. RP03 — typed Unit many→many и learned Flow corrections. RP04 — async Orchestrator и higher-order Graph. Остальные WorkLeft domains, feedback/PC и DTC/IDE сохраняют owners [стратегии](../R01-R25-TO-CODE-SLICES-STRATEGY.md). Один C# или ENRU опыт их не закрывает.

CODEX `astra 6 ultracode` — человеческий selector и отдельный exact pair judge; independent CODEX Astra — Student examiner. Dev author честно симулирует обе стороны китайского Teacher; current deterministic-template simulation не переписывается как real Qwen/Astra calls. Local Qwen author/evaluator не входит в этот inner loop.

## Проверки и завершение поручения

Документы/read/reuse: **0 новых product tests**. Изменённый deterministic риск: ближайшая необходимая проверка свойства, не coverage quota. Windows matrix не gate обучения; права/материал/masks/state/serving safeguards сохраняются. Current examination определяется B500.04 и actual frozen protocol, а не старой LEARNING-EXAMINATION страницей по умолчанию.

Вернуть один packet: actual mode/protocol/owner, reused/new inputs, effect/quality/cost/unknowns отдельно, exact publication/readback и один следующий шаг. Плановый ремонт не является independent Astra review или проведённым experiment. [Предыдущая доска](https://github.com/lubochka/xiigen-planning/blob/1bbb8141a5bccfba54e93bf73cff046c3a72b77c/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/EXECUTION-BOARD.md) сохранена для истории; её first50 defaults не current runtime command.
