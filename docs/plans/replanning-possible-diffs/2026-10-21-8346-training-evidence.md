# 8346: первичные учебные свидетельства N63C, 21 октября 2026

Фаза: **READ_ONLY_RESEARCH + назначенные Markdown**. Worker `/root/rp01_primary_evidence`, без вложенной делегации. Текущий bounded slice — восемь входных Markdown, N63C saved evidence и адресная проверка PA01/N63IL. Это не обзор всех40файлов поступления. Build/test/Train/inference/Generate и чужие процессы не запускались; host/source/raw/weights/Git не изменялись. Бинарные checkpoint не читались, не хэшировались и не загружались. Чужие выполненные calls ниже — сведения из сохранённых текстов, не действия этого worker.

На момент чтения core HEAD=`256f603e70032c11b1f4ffabbdd1fefdab642ea5` (read-only `git rev-parse HEAD`). Вложение сообщает planning `fc0dd1b04d..8346dfb697`; источник incoming ниже остаётся историческим cutoff, а живые документы и raw читаются отдельно. Model overrides, protected-scope и старые NEXT во входном аудите не отменяют текущую фазу, default session executor и live queue. Счётчики чужих review не становятся нашими проверками.

## 1. Что исправлено в живой карточке

В первом срезе изменена только [N63-correction-collision.md](experimets/cards/N63-correction-collision.md), включая её встроенный промпт: добавлены найденный первый probe raw, отдельные статусы первоначального A / завершённой первой probe / partial02 / build-blocked03; снято прежнее текущее «result/panel не найден». Уточнены полезные частичные показатели и регрессии, source-to-binary custody gap и full23/23 predicate. Mandatory30/31, default model, штатный wait, STOP/NEXT и запрет повторов сохранены. Последующее прямо порученное добавление HNSW и уточнение framing описаны в§9–12; это расширение документации, не runtime.

PA01/N63IL не редактировались: при единичном чтении metadata **оба actual host root отсутствовали**, что согласуется с [PA01:19](experimets/cards/PARENT-ADOPTION-01.md:19) DESIGNED_NOT_MATERIALIZED/NOT_RUN и [N63IL:19](experimets/cards/N63-INGRESS-LENGTH-LEARNED-01.md:19) DESIGNED_NOT_RUN. Это локальный факт отсутствия именно `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PA01` и `.../N63IL`, не доказательство, что никто нигде не запускал похожую работу. PA01 handoff1000 и N63IL consumer recipe остаются будущими; их source plan не выдан за result.

## 2. Первый probe: что действительно записано

[Первичный JSON](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63C-PROBE/results/n63c-probe.json) прочитан полностью и разобран структурно: все root fields,24 acquisition checkpoints, шесть панелей по три строки, arm metrics, budget и limitations. SHA256=`f0cf7a5dcff91120b386b78b6239fa9363d4cfad827823ab13e817f635bde9e9`.

- [Acquisition:17–195](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63C-PROBE/results/n63c-probe.json:17):600 SFT updates приLR.0001; correct T1 растёт0→1→4→7→13→17→20→21→22 и остаётся22/23 до600. Online full-sequence loss7.759556→0.000028034774 не равен response NLL и не доказывает whole-response exact.
- [W0:198; Wauth:230](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63C-PROBE/results/n63c-probe.json:198): T1 0/23→22/23, exact=false; response NLL7.307878→1.1099185; mean-logP margin0.2279687→1.1063861. EXAM Q1/Q2 correct0, знаменатели29/21.
- [A after DPO:262; B after DPO:326](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63C-PROBE/results/n63c-probe.json:262): записанные панели совпадают по всем полям; T1 22/23,false,NLL1.1079743,margin1.1252952. DPO checkpoint metrics обеих рук:step1,loss.13489959,accuracy1,objective=`response-sum-softplus-signal-weighted-v3`. Это равенство наблюдаемых outputs, не побайтная проверка всего learned/optimizer/RNG state.
- [A after SFT:294; B after SFT:358](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63C-PROBE/results/n63c-probe.json:294): после100 SFT каждый A20/23,false,NLL1.1158044,margin−.119724035; B21/23,false,NLL1.1042708,margin1.1151558. B относительно A имеет+1 correct token, NLL−.0115336,margin+1.234879835. Это полезные наблюдения частичного token/preference поведения.
- Регрессии рядом: оба плеча потеряли часть22/23; B EXAM NLL хуже A: Q1 7.074539 против6.987412, Q2 7.250893 против7.2149773. Q1/Q2 correct по-прежнему0. Нет освоенного полного ответа, free generation или доказанного EXAM transfer.
- [Budget:596–606](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63C-PROBE/results/n63c-probe.json:596):600 acquisition SFT+200 arm SFT+2 DPO=802 optimizer updates;60 score calls=24 acquisition checks+36 panel sides;Generate0. Whole run632.0084716s, process peak1180323840bytes. Время/allocations сохранены как наблюдения, без причинного speedup; полную стоимость всех model calls сверх явного raw budget не реконструируем по изменённому source.

Фактор в raw: A SFT.Response=`static int Abs(int n)=>n<0?n:-n;`, B=`static int Abs(int n)=>n<0?-n:n;`, DPO chosen/rejected сохранены. Fixture SHA=`fcf2e07aa283ad40622b7c05eee738d0c490c5c2448f686fde935775ecff1992`, exact collision=true. Это не утверждение о production teacher provenance или достижимости такой пары в текущем Incremental pipeline.

**Qualified before не закрыт.** До SFT обе руки имеют22/23,E=false, а карточка проверяет сохранение приобретённого полного TRAIN-ответа23/23,E=true. Partial evidence нельзя обнулить или выдать за full fit. Для claim «DPO приобрёл ответ» нужен также !E0&&E1; SFT-pretrained E0=true дал бы иной, retention claim. Отдельная probe не завершает старое B и не превращает исходные4updates в802.

## 3. Текущий source и сохранённая provenance

Путь: [N63CollisionProbe.cs](C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/Rp01Micro/N63C/Common/ContinuousLearning/N63CollisionProbe.cs). Полные байты прочитаны дляSHA; смысловой readback —43–521 и адресные caller/receipt участки. SHA256=`ddeefb1fd199a494c270e8881ea4e933d4ff8cebeeb514244694156b3305b242`.

| Native causal chain / ограничение | Точные строки |
|---|---|
| Current constants60/.003/20 против raw600/.0001/100 |54–58|
| Полный SFT joint encode, Trainer и AdamW, реальные acquisition TrainSupervisedNextTokenStep |117–150|
| Save authority weights, Wauth panel; обе руки вызываются при acquiredExact=false |155–167|
| Поздний CreateNew result, verdict C>0 «ACQUIRED» |173–201|
| Arm native LoadModels, weights-only authority restore, PrepareBatches |253–267|
| Реальные governance/content/lease/capability, DPO publication observer и native RNG callback |270–341|
| DPO StepVector, затем fresh SFT AdamW, joint encode,100/20source-defined steps, final weights-only save |341–375|
| Score вызывает model.Forward(training:false), logits row i→target ids[i+1], mask[i+1], first-ID argmax и strict-greater rank |434–471|
| Native start из B0 Step1/no-parent/InitialBaselineNoUpdate, не accepted SFT parent |477–505|

Current source нельзя объявить точным source исполнившего первый probe binary: budget/constants уже другие, shared CL source-copy менялась. [Program.cs:15–16](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63C-PROBE/Program.cs:15) вызывает этот bridge; [Probe.csproj](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63C-PROBE/Probe.csproj) ссылается на N63C libraries. Их наличие не восстанавливает historical build custody. Final keyed m/v/RNG bundle и его equality не подтверждены. Одинаковый первый loss/DPO JSON не заменяет такую проверку.

[Report probe:22–23](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63C-PROBE/report.md:22) выводит same initial state из одинакового DPO loss — это сильнее измерений. [Исходный N63C report:73–89](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63C/results/report.md:73) одновременно объявляет старое margin/NLL движение непригодным как любая находка и новую гипотезу верной по22/23. Live card теперь отделяет diagnostic evidence, partial preference evidence и открытую full-learning цель. Эти внешние исторические reports не переписывались.

## 4. Незавершённые paths и ошибка ожидания

Это один frozen filesystem/readout snapshot, не наблюдение ongoing процесса:

1. [results-02/progress.log:1–17](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63C-PROBE/results-02/progress.log:1): acquisition60, Wauth22/23,false; A DPO закончен, затем SFT5/20 и10/20. [A DPO JSON](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63C-PROBE/results-02/A_stale_rejected_text/checkpoints/dpo_step_000001.json) содержитstep1/loss.24438997. Final `results-02/n63c-probe.json` отсутствовал. Итог/полный расходUNKNOWN, не0.
2. [cycle.log:2–6](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63C-PROBE/cycle.log:2) уже выбираетresults-03, затемBUILD_BLOCKED. В [build.log:48–51](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63C-PROBE/build.log:48) MSB3027/MSB3021: ContinuousLearning.dll locked by .NET Host13004. Это PID из сохранённого лога, не проверенный живой процесс. Result03directory/JSON отсутствовали; training03 не подтверждён. `run.log` пустой, что само по себе не доказывает never-started предыдущих путей.
3. [Run-Probe.ps1:14–21](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63C-PROBE/Run-Probe.ps1:14) сначала перезаписывает cycle.log, затем выбирает first-free results02..99;25–34 обновляет shared source list;39–48build→run. Это конкретный конфликт нынешнему контракту31: неизвестную попытку нельзя обходить новымroot или повтором entry. Runtime script не менялся.
4. Source comment:49–53 утверждает двухминутный предел background и объясняет уменьшение бюджета. Комментарий не является документацией tool/API и не доказывает kill процесса. Нормальный bounded wait того же handle продолжается; два быстрых пустых poll требуют перейти к wait/адресной диагностике, не третьей попытке или user-run. Текущий worker не управлял чужим PID и не запускал инструмент для проверки.

## 5. RNG и счётчики исходного N63C

Native [RepositoryOwnedDpoRandomStateFile.cs:417–428](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/RepositoryOwnedDpoRandomStateFile.cs:417) требует canonical `BuildFileName(step)`; `after-sft.rng.json` не принимается. Уже назначенное документное исправление — отдельный owner-only `sft-state/dpo_random_state_step_000001.bin` для SFT, без collision с DPO checkpoint и без ослабления writer.

Сохранённый исходный A имеет1 DPO+1 SFT и18scores до late save error; B отсутствует. Итоговые0/0counter при error не доказывают отсутствие уже returned updates. Карточка сохраняет monotonic issued/returned/applied и отдельныйsaveStatus; неизвестное applied не превращается в0. Current study не повторяет updates ради исправленного receipt. Стоимостной residual CPU/alloc scoring включает setup/snapshot сверх wall ScoreAll, поэтому его нельзя переносить как чистую стоимость18 Forward. Это source/readback correction, не новый эксперимент.

## 6. Восемь входных файлов: полный смысловой охват

Все пути таблицы относительно **C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-causal-audit-fb9175cc/**. Каждый Markdown прочитан целиком; хэш полных байтов не является дополнительным semantic review.

| Файл | Рассмотренное содержание | SHA256 |
|---|---|---|
|07-neural-source-audit.md|Native DPO/SFT chains, SANITY/RESTORE/N63I/R receipts, новый SiLU factor; не смешивать topology/partial/full и source сresult|23a838d5672e7561d0ae7dc68564060b5fa7ab94240657d6f081bbdcf8c6175c|
|10-late-core-delta-d962f958.md|Исходный N63C partialA, RNG-name, late-counter, scoring-cost scope; PA01/SC01/SEXP designs не выполнены|510e7a7252665e4cb54bb3bf4a08e781a322efc51e17d03b89c7ebd7b99eaa4f|
|11-final-cutoff-256f603e.md|Первый complete probe802/60,22/23, changedsource иpartial02; старые очередь/modellabels не переносить|a9a65250eaee3866f40774fbe9b61eef1ff77e9df940ec856e10375572cf1bc0|
|evidence/core-delta-n63c.md|Подробные A metrics и отрицательные границы; механический RNG repair не создаёт learnedbefore|9365047ea7ea3bc468c9d9d5793bc77af62a19fca783dd0f28c19856ff0eed47|
|evidence/core-delta-parent-sft.md|Truthful SFT-adoption schema future, ordinal2/step1000, N63IL200, SEXP8; не fakeDPO и не результат плана|66ca0b9fac66d56dd7e2d14e5499fdf51994052bbcac08a31d32dacb1aec2e09|
|evidence/core-final-n63c-probe.md|Панели и aggregate budget первой probe; источник нынешних constants расходится;EXAM0 иfullfalse|cea2f9052b54e33a7249f7fb722aa817e6f17941625e101bf6e8aa8cff3db238|
|evidence/core-final-candidates.md|RNG/counters/collision candidate уже известны; registration не означает approved transfer|fd24beee2e797b64e888d2ff33ccc8835dcbc9f3251cd05da19084b441c3e3c5|
|evidence/review-final-learning.md|Чужой review — meta-claims, включая HNSW manifest parity и N63I block; не наши циклы или независимые runtime checks|7648d2ef9617d4e885ca5156a548882bc92b3ffcdf1d353daabf038cc6279367|

Вложение `C:/Users/lubao/.codex/attachments/4366446e-012c-453c-bf03-01bbbec051ae/Pasted text.txt` прочитано полностью:91строка, pull summary40files/create paths. Enumerating55objects — не число файлов/опытов/reviews. Остальные файлы пакета читает parent; этот8-file slice не заявляет их semantic coverage.

## 7. Первичные источники: точный read inventory

Префикс P=`C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/`. All перечисленные JSON прочитаны полностью и распарсены; для иных файлов «полный» означает весь текст семантически. Source hash не доказываетbinarycustody.

| Путь отP | Охват | SHA256 полных байтов |
|---|---|---|
|N63C-PROBE/results/n63c-probe.json|Полный JSON, все записи/поля|f0cf7a5dcff91120b386b78b6239fa9363d4cfad827823ab13e817f635bde9e9|
|N63C-PROBE/report.md|Полный102строки; overclaims квалифицированы выше|44027c9c6c924333ba2e1258f2562dc4f676c58c9187a95ec0b5b52f0f19abf6|
|N63C-PROBE/Program.cs|Полный34строки; Run/return/catch|8bef080ad5a6a3e4968ac7f39a287cf3f0362233018d68e12f073cc1eb5401e5|
|N63C-PROBE/Probe.csproj|Полный30строк; copied project references|111e524bca30c1523290fd6b61093ab6421a38d0539a2b5370eb69e093b001e5|
|N63C-PROBE/Run-Probe.ps1|Полный48строк; retry-root/build/shared-copy mechanics|0bb5f36f7a00763a98428c45e763a27746e33e84d1b671bbd948a8315e0ec4b3|
|N63C-PROBE/cycle.log|Полный6строк|d26a4eb40676565216b44311d93c107952e3ea625fa905afbfc3ac24e35a498b|
|N63C-PROBE/run.log|Пустой0bytes|e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855|
|N63C-PROBE/results-02/progress.log|Полный17строк; partialcalls|f08016713cdfda06886f39ae1645e353338872f12e6c99615f1fd31ee1c6a899|
|N63C-PROBE/results/A_stale_rejected_text/checkpoints/dpo_step_000001.json|Полный JSON|d327c8bd385a41de0b68bf8900bcd1faeee079e875693dca3addb62b06dc4405|
|N63C-PROBE/results/B_corrected_text/checkpoints/dpo_step_000001.json|Полный JSON|d327c8bd385a41de0b68bf8900bcd1faeee079e875693dca3addb62b06dc4405|
|N63C-PROBE/results-02/A_stale_rejected_text/checkpoints/dpo_step_000001.json|Полный JSON|1e113afb6a5bee1fec904c3261e4a043c82a7c30a905dc43417f8bb5842796e3|
|N63C/results/report.md|Полный107строк; исходный A и поздняя probe interpretation|7591f259fc1517b1afb499c8ac9c7c9113784eb3ff7c8b1829003d0794a9c2c1|
|N63C/results/candidate.md|Полный21строка; существующий кандидат|8ede8e91f88baf7a22df86534ba1f9a8487f008c81233c252d83d2c78b5de414|
|N63C-PROBE/build.log|Только error48–51/95–96; не полный semantic read|Не заявлен|

Дополнительно: N63CollisionProbe.cs scope/hash в§3; native RepositoryOwnedDpoRandomStateFile.cs414–434; live N63C card целиком с частями source recipe/embeddedprompt; PA01/N63IL только адресные status/parent/budget paragraphs, не полное повторное review. Их `pa01-source-contract.json` не включён в semantic inventory: попытка вывода была обрезана и не является прочтённым доказательством. Исходный A raw был прочитан в предыдущем packet; текущий новый slice опирается на его явно процитированный readback и текущий source/report, не выдаёт его за повторную полную проверку.

## 8. Оставшиеся точно ограниченные gaps

Полного23/23 before на совместимом H512/V629/qv8 T1 не предъявлено; first probe source-to-binary custody и full final optimizer/RNG не замкнуты; окончательный result02/его агрегатUNKNOWN; actual PA01/N63IL не materialized. Это поля автора следующей постановки, не задание исполнителю искать другой checkpoint/бюджет/host. Новый runtime рецепт в данном slice не назначен. Current evidence не доказывает поломку всех parent selectors: native DPO путь и future truthful SFT path различаются. Useful partial candidate сохраняется с регрессиями; полный учебный objective N63C остаётсяOPEN.

## 9. Следующий порученный срез: HNSW query norm

Полностью прочитана входная карточка `C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-causal-audit-fb9175cc/cards/HNSW-QUERY-NORM-REUSE-01.md`:239строк,46868bytes, SHA256=`b5e8c9cfb565bb700432d41bd4b524337ab775a4f9da430841e83cd341a94b93`. Итого здесь **девять incoming Markdown** прочитаны полностью, не40пакетных файлов. Чужие novelty/review counts остались атрибутированными incoming, не новыми нашими проходами.

Созданы [живая карточка](experimets/cards/HNSW-QUERY-NORM-REUSE-01.md) и [standalone промпт](experimets/prompts/HNSW-QUERY-NORM-REUSE-01-executor.md). Host H=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/HNQ01; R=H/results; внешний entry `pwsh -NoProfile -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/HNQ01/Run-HNQ01.ps1`. Это **будущие** места/команда; файл host не создавался и не запускался. Маршрут47→48IDENTITY→49KN; currentNEXT10PA01 не изменён.

Первичная причинность подтверждена чтением: [KnnLogicalUnit.cs:132–154](C:/Projects/llm_mvp_core/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs:132) действительно Train BPE320→HNSW Add всех lessons; [216–256](C:/Projects/llm_mvp_core/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs:216) reader использует learned tokenizer/index/exampleMap. [HnswIndex.cs:49–97](C:/Projects/llm_mvp_core/06.VectorSearch/HnswIndex.cs:49) вызывает query methods, [167/175/198/215/228](C:/Projects/llm_mvp_core/06.VectorSearch/HnswIndex.cs:164) — ровно пять cosine sites. [Distances.cs:10–20](C:/Projects/llm_mvp_core/06.VectorSearch/Distances.cs:10) каждый раз суммирует query squares. B ограничен local per-method norm, не global cache/alias repair/SIMD. Восемь nodes не покрывают reverse pruning, observer не считает readout work как training work.

Полные bytes всех10native sources и3fixtures извлечены readonly `git show fb9175...:<path>`, Git blob hashes проверены и совпали с таблицами входной карточки и working bytes на момент чтения. Native callbacks не исполнялись. Полный semantic read: HnswIndex245строк,Distances41,KnnLogicalUnit323,OperationResult26,SemanticCore114,RegexSplitter22,VocabEntry11,IVectorIndex14. LogicalContracts1–112 и BpeTokenizer21–86,553–626 прочитаны адресно; поздний вывод521–553 был частично обрезан, поэтому полный повторный semantic review всех helpers/canonical loader не заявляется. Все три JSON распарсены:8training examples со всеми полями,24eval cases,24gold cases со всеми accepted variants. Вложенные inputJson/chosenOutputJson отдельно разобраны: targets — настоящие русские строки, не буквальные Unicode escape text.

Назначены ясные документные исправления incoming:

- Внешний root заменён HNQ01/work; сохранены10sourcecopies/3fixtures. Единственный внешний controller владеет Prepare→две build→A→B→report; global MSBuild properties действуют доSdk.props. Source closure не объявлен compile proof.
- Early H/execution.claim и R/study.claim до copy/build/native; child nonce/PID/start и per-arm issued.marker доTrain. H claim открыт FileShare.Read для child read, без разрешения чужой записи. Любой existingR даже безclaim — refusal, не новыйroot. Late rawCreateNew недостаточен.
- Qualification before≥2 разных correct old labels из4 превышает constant1/4; after≥1 новый correct exact изx05..x08 каждой руки. Это авторский критерий реального learned readout, не threshold resource success. Основной конечный бюджет не увеличен:4Train,24Add,96Execute,4Save. Failure сохраняет objectiveOPEN; ни поискseed, ни дополнительныеcalls не разрешены.
- Manifest TrainingTimeMs/ManagedAllocationBytes исключены из semantic parity. Norm elements/builds — derived arithmetic по actual Add counters; не CPU timing. ОдинA→B не устойчивый speedup. Данные P125 используются supervised genericUnit, не neuralDPO/TranslationMachine.
- Полные mandatory30/31, default model, три status fields, ordinary candidate registration≤120слов и немедленный STOP/NEXT присутствуют в обоих документах. Внешнее protected правило не запрещает текущий allowed Markdown.

Попытка одной составной shell-команды записи Markdown была отклонена автоматической проверкой до старта: `blocked by policy`, иной причины не предоставлено. Этой командой ничего не создано. Разрешённые отдельные документы затем записаны адресными apply_patch; host/runner/scripts не создавались.

## 10. Адресный независимый readback SILU, без редакции чужих файлов

Полностью прочитаны текущие [SILU card](experimets/cards/SILU-SIGMOID-REUSE-01.md)299строк и [prompt](experimets/prompts/SILU-SIGMOID-REUSE-01-executor.md)42строки первой прочитанной редакции; затем адресно обновлённые пункты. Native [TensorOps.cs:1129–1162](C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorOps.cs:1129), [Trainer.cs:117–147](C:/Projects/llm_mvp_core/04.TrainingLoop/Trainer.cs:117), [SwiGLUFeedForward.cs:40–51](C:/Projects/llm_mvp_core/03.TransformerModel/Layers/SwiGLUFeedForward.cs:40) подтверждают factor/caller;36sources manifest прочитан как документ, не повторно все36source files. Неверный сокращённый путь LFT01/src был моей неуспешной read-попыткой, а не отсутствием точного nested subtree в карточке; эта попытка не считается source read.

Parent применил три адресные замечания, fresh readback подтверждает:

1. [Card:124](experimets/cards/SILU-SIGMOID-REUSE-01.md:124), prompt:32: требование хотя бы одной той же TRAIN строки W0exact=false→W400exact=true. Без новых readouts, без заявления, что1/2 превосходит constant baseline или осваивает всё правило.
2. [Card:203](experimets/cards/SILU-SIGMOID-REUSE-01.md:203), prompt:34: execution claim доbuild сnonce/PID/start; отдельный Program study CreateNew доW0; existingS/R/claim сохраняется; leafS/R создаёт Program послеguard. Direct Program repeat не разрешается старымclaim.
3. [Card:264](experimets/cards/SILU-SIGMOID-REUSE-01.md:264), prompt:38: ordinary useful completion обязательно регистрирует candidate≤120слов вrequired-changes; STOP/NEXT выше оформления.

В этом узком readback иных существенных противоречий factor/budget не найдено:400common+4A+4B=408updates,8568shiftedtargets,24readout; denominator в B сохраняетx/d и читает его вbackward, cachepayload14784bytes за21×176float — расчёт, не измерение. Build/ABI/performance/успех обучения не проверены. Это не approval всего корпуса или runtime readiness.

## 11. Позднее первичное уточнение N63C: preparation и scorer имеют разные context

Дополнительно адресно прочитана **именно копия** `C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/Rp01Micro/N63C/Common/DPO/Models/DpoExactPairMaterial.cs`:15–25,63–85,123–153,153–181 и property matches37–59/87. Полные bytes дляSHA256=`d1688024db221f8a4ea7d4a28b0238671d7a057a0713d6e70905c947321f9a7f`,10485bytes; полный semantic review файла не заявляется. DpoBatch.cs property ExactMaterial:25 также проверен.

Факты source: FramingVersion:21=`prompt-response-segments-no-separator-v2`;72–78 отдельно EncodeSegment(prompt)/chosen/rejected;140–146 native encode/decode validity;171–176 конкатенируют IDs без LF/BOS/EOS. ChosenLength=PromptLength+chosenResponseLength, SequenceLength=max(chosen,rejected); response mask покрываетPromptLength..ChosenLength. В N63CollisionProbe.cs:124 common preparation кодирует Prompt+LF+Chosen;357 treatment кодируетPrompt+LF+Response. Panel:419–420 и SideScore:445–449 читают DPO IDs/response mask, rowi→targeti+1. **Низкий SFT loss здесь не проверяет тот же context, который квалифицируется scorer.**

Это возможное объяснение22/23, не доказанная causal причина: raw не даёт per-target proof, source-to-binary custody первогоcompleteprobe остаётсяunknown. Не делается новый runtime/новая гипотеза, не предлагается бесконечно увеличиватьsteps.

В N63C card и embeddedprompt назначен конкретный document repair **common preparation**: после прежнего PrepareBatches взять T1batch.ExactMaterial.ChosenLength и IDs из T1batch.ChosenIds.Data[0..ChosenLength), проверитьnative material/целочисленные IDs, один раз создатьList<int>. Не padding/LF/BOS/EOS, не повторныйBPEEncode склеенного текста. Прежний NativeTrainer всё ещё full-sequence, scorer всё ещё исходный maskedresponse; loss иresponseNLL не смешиваются. Treatment остаётся Prompt+LF+armResponse и ARejected/BChosen — молча его framing/productionпуть не чинится вторымфактором. Это закрывает конкретный выборpreparationinput, но **не** предъявляет full23/23 snapshot или новую execution authority. Остальные gaps§8 сохраняются.

## 12. Финальный readback и фактические границы

Изменены четыре Markdown: N63C card, HNSW card, HNSW standaloneprompt и этотevidence. PA01/N63IL иSILU не редактировались worker. Адресно прочитаны live N63C newraw/context sections иembeddedprompt, HNSWqualification/claim/budget/status/entry/report paragraphs. Статическая проверка трёхcard/promptfiles:0broken Markdown links,0externalGitHub sourceURLs; mandatory30/31 текст совпадает с текущим SFT-EXP-REUSE после нормализации CRLF/LF. Raw bytes блока различаются только переносамистрок, поэтому побайтноеравенство не заявляется. Читабельность qualification иprompt поправлена без изменениясмысла.

Parent позднее сообщил externaldoc-only HEAD=`003083feab72157e54184bb66d0d940f48d836fd` и229docpaths относительно256cutoff. Это атрибутированное сообщениеparent, не мой новыйGitdiff или публикация; initial incomingcutoff сохранён. Worker не выполнялcommit/push или повторныйfullcorpusreview. Полный пакет40 иglobal49route проверяетparent; здесь ограниченный substantive readback назначенныхисточников/документов. Экспериментальные вызовы этогоworker:0.
