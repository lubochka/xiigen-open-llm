# Независимый bounded review: SiLU, карта механизмов и сохранённые кандидаты

20.10.2026. Reviewer: knn_restore. Пакет: `2026-10-20-learning-causal-audit-fb9175cc`. Scope: новая SiLU карточка, `01-mechanism-map.md`, native/translation разделы `02-saved-results-and-candidates.md`. Собственная HNSW карточка здесь не оценивалась. Runtime/сборки/новые исследования не запускались; документы пакета не изменялись. Дополнительно прочитаны уже доступные pinned source snapshots, tree metadata и сохранённая JSON сверка; арифметика/сопоставление manifest выполнены без модели.

## Вердикт

**Статус проекта `AUTHOR_SETUP_REQUIRED / NOT_RUN` честный. Перед публикацией желательно устранить три точечных author-contract несогласованности ниже.** Новых измеренных SiLU успехов карточка не приписывает; связь с настоящим обучением подтверждается. Source closure, материал и полный бюджет согласованы. В рассмотренных root разделах опасной подмены partial-token результата полным правильным ответом или neural способности успехом translation-memory не найдено.

## Адресные замечания

| Приоритет | Место | Проблема и готовая правка |
|---|---|---|
| P1 author contract | `cards/SILU-SIGMOID-REUSE-01.md`, §3, предложение «ImportPortableState должен вернуть true» | Native API называется **TryImportPortableState**: `core/04.TrainingLoop/Optimizers/AdamW.cs:234–236`; сохранённый RESTORE host использует его в112. Заменить имя на точное `optimizer.TryImportPortableState(bindings, portable)`; `portable = AdamWPortableState.FromBytes(File.ReadAllBytes(path)) ?? throw ...`. Иначе карточка заставляет автора искать несуществующий метод. Это документный typo, не дефект native API. |
| P1 author contract | SiLU §3 restore A/B, перед фразой про identical bytes | Не задан явно обязательный construction order. `LlmModel.LoadCheckpoint` читает записи в уже имеющиеся `BuildCheckpointEntries` (current source1140–1159), не создаёт LoRA topology. Добавить: для каждого arm создать новый LlmModel указанного cfg, ApplySeeded q/v с теми же rank4/alpha8/dropout0 и62002UL/stream, затем LoadCheckpoint(S0), проверить4trainable/12frozen и named hashes, создать такой же AdamW, TryImportPortableState. Existing RESTORE Program99–112 показывает ровно Apply-before-Load. Не применять LoRA после загрузки, иначе можно перезаписать обученные adapter weights. |
| P2 ясность paths | SiLU §4, строки174–178 против §8,237/255 | §4 фиксирует `results/SILU01-R1` и `s0/SILU01-R1`, а §8 перечисляет `results/status.json`, `s0/weights.bin`. Сделать единый контракт: все raw/report под `results/SILU01-R1/`, все donor files под `s0/SILU01-R1/`; либо явно объявить `results`/`s0` в §8 shorthand этих run directories. Самовольно менять run ID/путь исполнитель не должен. |

### Необязательные редакционные уточнения, без новых проверок

- В root02 BPE таблицу можно добавить сохранённый wall54.3095→34.3645ms и44.9896→38.1428ms: значения уже есть в reconciliation JSON. Это ещё одна полезная наблюдаемая ось, наряду с −71.144%/−69.108% allocation. Однократный elapsed не обещает устойчивый speedup; code CPU ухудшается, и это уже честно показано.
- У переводческих19/24/acceptedWrong3 указать «по сохранённой строгой рубрике». JSON уже объясняет, что cc01/cc04 — явные смысловые ошибки, cc03 — неоднозначный случай со strict failure. Не менять scorer/числители и не назначать повтор.

## Что подтверждено в SiLU карточке

1. **36 из36 source SHA совпадают.** Таблица карточки сопоставлена с `audit-cache/micro-LFT01.json`, отфильтрован весь `/src/**/*.cs`; missing/extra/mismatches=0. Nested source path действительно присутствует; это provenance старой копии, не назначение воспроизвести длинный nested рабочий root. Отдельный experimental Linear switch явно false и не замаскирован как полностью неизменённый runtime.
2. **Native call chain действительный.** `Trainer.cs:102–147` создаёт `tokenIds.Count−1` targets, вызывает training forward, CE, BackwardLoss, clipping, AdamW и ClearGradients. `TensorOps.cs:1132–1162` содержит Exp в forward и в backward. В FFN градиент нужен для upstream q/v LoRA даже при frozen FFN. B сохраняет знаменатель и порядок x/d; не делает иной sigmoid forward/derivative и не добавляет другой objective.
3. **Материал/targets точны.** Строка prompt имеет21 UTF8 bytes, prompt+sign22; native input21, targets21. 400+4+4=408updates; 408×22=8976encodedbytes;408×21=8568targets. На arm4×21=84targets. Четыре строки при W0,W400,A0,A1,B0,B1 дают24readout forwards. Никаких скрытых дополнительных train/readout программа не требует.
4. **Плата за cache показана.** 21×176×4=14784B payload одного массива,4updates→59136B; заголовки/GC/lifetime отдельно, не fake allocation saving. Общий PeakWorkingSet64 не сравнивается как два независимых пика. Fixed A→B/JIT/шум отмечены. Нет объявленного устойчивого ускорения или универсальной доменной пользы.
5. **Donor честный.** LEARN-SANITY Program сохраняет только JSON. Его source47–78 подтверждает cfg/seeds/qv4/LR/400steps,142–168 — exact one-token readout. В root02 и карточке отмечены1/2TRAIN/EXAM и constant answers, поэтому освоение условного sign rule не утверждается. H16 checkpoint не подставляется вH64. Авторская подготовка400updates включена в полный бюджет; это новое планируемое действие, а не выполненный повтор старой пробы.
6. **Нет invented existing CLI.** Run.ps1 помечен отсутствующим future host; root, run ID, framework, Program order и будущая команда заданы. SDK ещё не проверен и будет записан автором. До готовности нет runtime команды к существующему producer/dispatcher. Source-copy экспериментальные исключения не переносятся в production.
7. **Пользовательские ограничения сохранены.** Default/inherited model, CODEX selection/quality роль,0external calls для SFT, русский report, skills перечислены. STOP/NEXT запрещают продолжать прежний report/review/hash/CI/run. Неуспех/budget failure не разрешают произвольный retry, изменения production или удаление старых результатов. Любое полезное measured improvement сохраняется по оси с ухудшениями рядом.

## Root01: mechanism/source claims

Карта явно различает наличие P-family и готовность конкретного подслучая; не заявляет, что тематическая ссылка доказывает причину RP01. DPO exact materialization, translation pair→subunits, generic KNN Unit и AST/POS learners не слиты в один алгоритм. Frozen reference, optimizer continuity, order/roles, source-label и restore отмечены отдельными владельцами. Утверждение об ограничении крупного E09 на KNN499 позициях соответствует известному `Take(500)`/следующему token target; малый позитив не стёрт. P25/metadata export vs full restore разграничены. На root01 полномасштабной проверки всех исходников каждого P-family в этом bounded review не заявляю.

## Root02: сохранённые результаты и границы

- P125: correct B0→19/24, corruptedC0→13/24, acceptedWrong3/9 согласованы с reconciliation; A без обучения0→0 обозначен control, не успешным приобретением. Ingress11→19/24, composition0→8/8, wrong1→3 и общая24panel сохранены. P25 сохраняет same24outputs/19correct/3strictwrong/newTrain0: это retention ранее выученного state, не новое обучение.
- RESTORE-MINIMAL-SET: сохранены useful NLL/rank/token deltas,0/2full exact и0/43EXAMtoken; rows-with-any-token2/2 не выданы за два верных ответа. Наличие loader/provenance ограничения не превращается в запрет private экспериментов.
- NEURAL-FROZEN-TRANSPOSE: −1507392B/−11.045%, retained+249856B, ухудшение wall/CPU и0→0full outputs стоят рядом. A/B parity не выдана за learning acquisition.
- N63 replay: исходный consumed9updates не смешан с отдельным4600update probe. Неравные total tokens/work и регрессии новых/другихстарых строк показаны. C уравниваетupdates, не token budget. Полные teacher-forced exact остаются0; partial-retention candidate назван адресно.

**Итог после трёх небольших document corrections:** пакет допустим как документированный анализ и план будущих опытов, без подтверждения run-ready и без автоматического production переноса. Новые исследования или runtime для этого review не нужны.
