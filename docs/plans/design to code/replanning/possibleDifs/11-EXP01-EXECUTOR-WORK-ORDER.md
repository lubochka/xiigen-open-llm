# CODEX — одно ограниченное поручение реализовать и выполнить EXP01
**XIIGEN RP01 · экспериментальная задача, НЕ выполненная работа · 16.10.2026**

## Mission и критерий

Создать маленькую изолированную .NET9 консольную программу, которая покажет реальную разницу между (A) отсутствием обучения TranslationMachine, (B) старым TrainingBatch scaffold с фиктивным ростом accuracy, (C) четырьмя реально сохранёнными chosen-примерами внутри ExecutorEnRu **с отдельным C_before до обучения**, (D) холодным восстановлением состояния C. Оценить 14 одинаковых ordinary translations и записать наблюдаемое улучшение, отсутствие улучшения или ухудшение. **Не делать вид, что это запуск RP01 Student или доказательство общей обучаемости LLM.**

Экспериментальный контракт: [11-EXP01-UNIT-MEMORY-IMPACT.md](11-EXP01-UNIT-MEMORY-IMPACT.md), готовые данные [11-EXP01-CASES.json](11-EXP01-CASES.json). Задача исполняется только в core repo, ветка codex/dpo-implementation; исходный проверенный snapshot 83590a462b2f8aedc9d3ab4cb775193eb9c25ce5, перед работой зафиксировать ACTUAL HEAD и отличия от snapshot.

## Обязательные источники и skills

В llm_mvp_core/docs/skills/ прочитать реальные файлы: human-readable-architecture-audit-SKILL.md, trainable-architecture-decision-SKILL.md, experiment-design-SKILL.md, improvement-measurement-SKILL.md, code-examination-SKILL.md, phase-planning-SKILL.md, plan-review-SKILL.md, prompt-patch-authoring-SKILL.md и learning-loop-closure-SKILL.md. В handoff перечислить found/read/missing, не выдавать список за факт чтения.

Прочитать точные реализации в core:
- tools/Rp01TranslationDomainLawContrast/Program.cs:12–152 — образец существующего runner, API сохранения/перевода/отчёта.
- tools/Rp01TranslationDomainLawContrast/Rp01TranslationDomainLawContrast.csproj — net9.0 и ProjectReference.
- 27.TranslationDomain/TranslationMachine.cs:51–145, 552–653, 714–790 — различие training APIs, save/load, real runtime.
- 27.TranslationDomain/TranslationUnitContracts.cs:129–177 — конструкторы TranslationLearningExample, TranslationLearningBatch.
- 00.Core/LearningMachineContracts.cs:23–44 — конструкторы LearningBatch<TDomain> и LearningTrainingItem для sham arm.
- 27.TranslationDomain/TranslationTrainedUnits.cs:263–331,403–445 — фактический learned-memory state и обычный consumer.

Не проектировать новых ILogicalUnit/ILearningMachine/DPO classes. Не изменять Gemini/Astra/Qwen ownership, существующие DPO datasets, checkpoints, source-laws, scoring thresholds, frozen panels, production pointer.

## Единственный разрешённый пакет новых файлов
Создать в core:
1. tools/Rp01UnitMemoryLearningProbe/Rp01UnitMemoryLearningProbe.csproj — net9.0 с ProjectReference="../../27.TranslationDomain/27.TranslationDomain.csproj".
2. tools/Rp01UnitMemoryLearningProbe/Program.cs — четыре экземпляра/arm; JSON reader, обычный orchestrator caller, запись evidence receipts в новый output-dir без overwrite.
3. tools/Rp01UnitMemoryLearningProbe/fixtures.json — байт-в-байт копия 11-EXP01-CASES.json из planning.
4. tools/Rp01UnitMemoryLearningProbe/README.md — лишь команды/отметки режимов, не статус выполненного обучения.

Если проект не компилируется, исправить его локальный Program.cs/.csproj по непосредственному compiler error. Изменять общий TranslationMachine/ExecutorUnit разрешено только после точного доказательства отдельной ошибки (и тогда остановиться, не смешивать fix с неизменённым экспериментом). Нельзя добавить новый GitHub Actions workflow, полного Windows build, общую Test matrix.

## Как правильно создать trained arm

В основе C использовать new TranslationMachine() и только 4 TranslationLearningExample с UnitId = TranslationUnitIds.ExecutorEnRu. Для каждого source получить new TranslationSemanticTokenizer().Tokenize(source, "en").Signature; JSON сериализовать ExecutorInput(source, "en", "general"), ExecutorOutput(target, 0.9, empty WordMappings), rejection payload с явным marker PUBLIC_SYNTHETIC_ONLY. Пример конструктора — tools/Rp01TranslationDomainLawContrast/Program.cs:137–147; не придумывать структуру JSON. Единственный вызов: machine.TrainStructuredBatchAsync(new TranslationLearningBatch(batchId, TranslationMachine.DomainTag, examples, ["exp01-public"])). A/C/D используют один и тот же unmodified inference path.

B использует new TranslationMachine и flat LearningBatch<TranslationDomain> из тех же четырёх source/target для legacy TrainBatchAsync; сохраняет returned AccuracyBefore/After, но не считает это реальным результатом. Никакая сторона rejected здесь не проходит legal DPO admission и не маркируется как output approved Chinese teacher. Если B неожиданно меняет реальные пользовательские ответы, остановиться и проследить actual source (может отличаться от прочитанного snapshot).

D выполняет C.SaveCheckpointAsync(private unique path), затем TranslationMachine.LoadCheckpointAsync(path) в новом instance/process; ничего не обучает повторно. При save/load failure вердикт RESTORE_BOUNDARY_DIFF / BLOCKED с подробными логами, без подделки output.

Перед C.TrainStructuredBatchAsync обязательно снять C_before на всех 14 обычных запросах, иначе изменение нельзя надёжно приписать обучению того же экземпляра. Потом выполнить один training batch и сохранить C_after на тех же 14 inputs. Теперь 5 snapshots: A, B, C_before, C_after, D; всего **70 сырых ответов**. D надо запускать из второго процесса после сохранения C и фиксации checkpoint-файла.

Для всех 14 запросов — строго GetOrchestratorAsync().RunAsync(LogicalOrchestratorRequest(caseId, TranslationRoutingFlow.ScenarioId, source, LogicalExecutionFlags, metadata{source_language=en, target_language=ru, topic_domain=general})). Записать FinalHumanResult, FlowResult.Metrics["adapter_id"], OverallVerdict, warnings, Summary.Passed и time. Не смешивать с самостоятельным ExecutorUnit для grading; только ordinary machine route. Данные cases[].expected до всех result receipts не используются.

## Минимальная проверка и команды

После создания проекта:

    dotnet build tools/Rp01UnitMemoryLearningProbe/Rp01UnitMemoryLearningProbe.csproj -c Release
    dotnet run --project tools/Rp01UnitMemoryLearningProbe/Rp01UnitMemoryLearningProbe.csproj -c Release -- --fixture tools/Rp01UnitMemoryLearningProbe/fixtures.json --out <NEW_PRIVATE_OUTPUT_DIRECTORY> --phase train-evaluate
    dotnet run --project tools/Rp01UnitMemoryLearningProbe/Rp01UnitMemoryLearningProbe.csproj -c Release -- --fixture tools/Rp01UnitMemoryLearningProbe/fixtures.json --out <SAME_PRIVATE_OUTPUT_DIRECTORY> --phase restore-evaluate

Это предполагаемый CLI нового инструмента, не существующий сейчас; реализовать именно двухфазный command contract. В phase restore-evaluate нельзя повторно тренировать, а fixture/source SHA и checkpoint должны совпадать с train-evaluate. Записывать разные create-only filenames, никогда не перезаписывать первый receipt. Создать уникальный output directory, каждый исходный receipt писать create-new, отказ при повторе/overwrite. Один runner, один writer, никаких параллельных операций над одним checkpoint.

Сохранить: experiment.json с actual core git SHA + fixture SHA; arms.jsonl (14×5=70 normal requests) с input, raw generated response, parsed learned adapter/fallback; training-receipts.json (B returned fake metric, C learned Unit IDs; C_before checkpoint must be untrained); checkpoint.json (path/file SHA and before/after mode); paired-score.json с точными знаменателями 4 exact /4 near /4 contradiction /2 exploratory; human-verdict.md на русском с конкретным изменившимся запросом/ответом; process-cost.json с wall/CPU/memory только этого процесса. Не округлять голые counts в рекламные проценты, если малые знаменатели.

## Что считать хорошим и плохим наблюдением

**GOOD пример, НЕ прогноз:** A, B и C_before возвращают Bootstrap для "a box is not empty", C выдаёт правильное "коробка не пустая" через учтённую memory, отрицательный "the box is empty" не возвращает неверное "коробка не пустая", D воспроизводит C_after. Это локальный effect learned-memory+laws.

**BAD:** B сообщает +0.01 AccuracyAfter, а исполнитель объявляет обучение успешным, не прочитав ни одного ordinary user answer. Другой BAD — C копирует exact train answers, а отчёт объявляет new domain generalization.

**EDGE:** нет корректных near outputs, зато exact улучшился: MEMORIZATION_ONLY. Улучшился near, но появился принятый opposite-polarity ответ: UNSAFE_RETRIEVAL, не PASS. C изменился, а D после restore нет: RESTORE_GAP, не DPO bug. Не переписывать expected labels и не подбирать порог по результатам.

## Самопроверка и handoff

Перед публикацией: readback фактического нового JSON и отчёта, check 4×14 row identities, equal query bytes and source, exact A/B control, correctness + harmful accepted separately, complete checkpoint SHA, no true external model calls, no native DPO optimization. Independent CODEX/Astra review: NOT_PERFORMED до фактического отдельного вызова, не заменять собственной подписью.

Окончательный ответ: что реально собрано и выполнено (файлы/branch/commit), сколько новых правильных near, сколько опасных wrong accepted, воспроизвёлся ли cold restore, относительное время CPU, какое одно наблюдение меняет решение о RP01. Если не выполнялось — честно READY_TO_RUN/NOT_RUN, без выдуманного PASS. Результаты не означают принятие новой Student checkpoint в production.

**После положительного результата:** отдельный REC04/RP02 experiment на той же frozen C100 в реальном ENRU adapter с memory off/on; не смешивать два способа обучения в первом крошечном эксперименте.