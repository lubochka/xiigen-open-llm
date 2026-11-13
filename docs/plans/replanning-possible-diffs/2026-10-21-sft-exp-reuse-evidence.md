# SFT-EXP-REUSE-01: первичные доказательства и закрытый документный рецепт

21 октября 2026. Worker `/root/rp01_primary_evidence`, один исследователь, без вложенной делегации. **READ_ONLY_RESEARCH + разрешённые Markdown/JSON.** Runtime, build/test/inference/Train/Encode/checkpoint load, создание host, изменение product/runner/raw/weights и Git mutations — 0. Документы ниже являются применённым результатом; работоспособность будущего host не проверена.

## Применённый срез

- [Карточка](experimets/cards/SFT-EXP-REUSE-01.md): №45 после IDENTITY-TRAIN-INDEX-01, NEXT=IDENTITY-M1-01; H=core/experiments-v2/RP01-MICRO/SEXP01, R=H/results/attempt-0001, один Run.ps1.
- [Standalone prompt](experimets/prompts/SFT-EXP-REUSE-01-executor.md): полный путь карточки, default session model, немедленные STOP/NEXT, guards и бюджеты.
- [Source contract](reconciliation/sexp-source-contract.json):87 точных native .cs,28 dependency metadata, отдельно future PA01 reader с provenance, A/B patch contract и28Forward/8Backward/8updates.
- Этот evidence; глобальные queue/index/registry/другие карточки принадлежат parent и этим worker не менялись.

Core HEAD при fresh read: `fb9175cc752d41e1cb525e0609b7c96140940217`; planning pin данного intake `fc0dd1b04d61a963906cdc1ab29dffd2ee9e520c` передан parent. Старые dd150aa/0ea в incoming — их snapshots. На момент metadata check21Oct PA01 и SEXP01 directories отсутствовали. Это existence check, не доказательство ABI или пригодности binaries.

## Первичный факт и отдельная гипотеза

[SupervisedTrainingExecution.cs:19–31](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/SupervisedTrainingExecution.cs:19) — настоящий production caller. [Trainer.cs:102–145](C:/Projects/llm_mvp_core/04.TrainingLoop/Trainer.cs:102) делает shifted inputs/targets, training Forward, CE, BackwardLoss, clip и AdamW. [BackwardLoss:408–419](C:/Projects/llm_mvp_core/04.TrainingLoop/Trainer.cs:408) вычисляет один и тот же MathF.Exp в denominator и gradient-loop, между ними logits не меняются. [426](C:/Projects/llm_mvp_core/04.TrainingLoop/Trainer.cs:426) вызывает настоящий autograd. Это подтверждает участие фактора в обучении; не подтверждает измеренный speedup.

B хранит Exp в одном float[V] на вызов, сохраняет float/порядок sum/division/scale/gradient и весь остальной learner. [CrossEntropy:1600–1624](C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorOps.cs:1600) отдельно делает ещё NV Exp и не меняется. Формула named loss operations3NV→2NV аналитическая; projections/attention/autograd не включены. При V256,N35,4updates на руку backward71680→35840, CE35840 у обеих; B добавляет4096bytes scratch payload плюс headers. Время/CPU/общие allocations остаются NOT_MEASURED.

Близкий [DpoTrainer:1072–1121](C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:1072) уже сохраняет Exp в expVals; N62-R2 перемещал allocation из position-loop, а не устранял повторный Exp. [Сохранённый R2 report:9–11](experimets/results/2026-10-20/N62-R2-01/report.md:9) — отдельный completed attempt, не результат SEXP. [MASK:91–97](experimets/cards/E-SFT-RESPONSE-MASK.md:91) меняет target weights/objective; SEXP сохраняет full-sequence SFT. LF03/GRAD-ACCUM/DLT обсуждены incoming как соседние факторы; их новый runtime review этим срезом не выполнялся.

## Назначенный совместимый before и оставшийся учебный предел

Incoming H64/sign S0 не предъявлен. Авторское решение не отправляет исполнителя искать его: использовать уже назначенную PA01 цепь H16 learned snapshot → truthful accepted SFT bundle → PrepareStartLatest. Механизм Trainer не содержит H64-specific ветви; смена общей topology для **нового** опыта допустима, но его результат не станет результатом исходного H64/sign предложения.

[RESTORE Program:37–60](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/Program.cs:37) задаёт H16/V256/context64/qvLoRA2 и literal One/Two/Three/Four/Pos. [84–113](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/Program.cs:84) — исторические1000updates, SaveCheckpoint, ExportPortableState, fresh attach-before-Load, TryImport. [123–129](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/Program.cs:123) — отдельные исторические200continuation. Ничего из этого не выполнялось повторно.

В [raw JSON](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/results/restore-minimal-set.json), `measurement.afterPriorTraining`: One2/20+Two1/20=3/40, whole-response exact0/2, mean response NLL4.766696420187372, rank51.825. `afterRestore` воспроизводит ту же панель из5строк; всего три панели×5=15records. `afterContinuedTraining`:4/40, exact0/2,NLL4.711598363502413; Pos NLL5.8079633717111525→5.857194149707198 ухудшается. Сохранённые частичные токены не являются полными ответами/обобщением. Восстановление старого обученного состояния не гарантирует новый gain за4updates.

[Score:228–261](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/Program.cs:228) даёт полный контекст, shifted row t→gold[t+1], response mask t+1≥prefix, strict-greater rank, lowestID argmax ties, double NLL. Это20новых scoring Forward на A0/A1/B0/B1 по5 строк;8training Forward, всего28. Negative controls уже внутри панели; Generate0. Training full-sequence не превращается в response-only objective. UTF8 literal lengths36/36/40/38/44 и response20/20/22/21/28 проверены обычным byte-count текстов, без native Encode.

PA01 reader — future module [контракт:86–121](experimets/cards/PARENT-ADOPTION-01.md:86), не уже существующий native SFT API. Он выбирает accepted000002/optimizer1000, читает exact handoff и полный truthful schema, не fake DPO/no-update baseline. SEXP byte-copies тот же фактически выполненный модуль; PA01 `technicalStatus=COMPLETED`/`handoffReady=true` — конкретная зависимость, сейчас не выполненная. RNG identity остаётся PA01 `LoraPairRandomSeedAuthority(20261020UL,"restore-minimal-set","restore-minimal-set")` и native topology/lora hash, adoption epoch ResetAtAdoptionDropoutZero. Не придумывается historical cursor и не вызывается ApplySeeded после Load. Сравнение m/v включает каждую float32 coordinate/keys/dims/step, не только bool import. Новый flat scheduler counter0 не reset AdamW: [CosineWithWarmup:28–35](C:/Projects/llm_mvp_core/04.TrainingLoop/Schedulers/CosineWithWarmup.cs:28) при одинаковых min/max даёт .0003.

## Closure и lifecycle — конкретные исправления входящего рецепта

Incoming36 source whitelist недостаточен для принятого PA01 reader. Live87=81CL+6TrainingLoop;28 metadata deps исключают исходный TrainingLoop.dll. Native six-file blobs подтверждены `git ls-tree`/`rev-parse` при current core pin. Все87 text bytes прочитаны и SHA сверены для documentary manifest; это не semantic review каждого CL файла и не build.

TrainingLoop отдельная library с прежними identity и source IVT LlmFromScratch.Tests. Copied CL и Runner используют ProjectReference **того же** A/B TrainingLoop, чтобы reader optimizer/TrainingConfig и Runner Trainer не расходились по типам. [N63I CL project:6–44](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63I/Native/ContinuousLearning.csproj:6) подтверждает identity/IVT/explicit dependency boundary. Source list/deps закрыты, actual ABI и newly authored helper/runner compilation остаются непроверенными. DLL/payload bytes не читались/не хэшировались; их SHA — inherited metadata для будущего automatic check.

Root execution.claim до Prepare/build; никакого common model stage. Arm CreateNew до PrepareStartLatest/constructor/load/panel. Любой existing R/study/claim/arm/raw/partial, даже без root claim, останавливает попытку. Direct build/arm не обход: active wrapper nonce/PID/starttime + exactpath guard. Все source/project/scripts должны быть завершены до одной Run цепочки; hidden owned processes, bounded timeouts, no retry/namespace escape. State/parity tests относятся к будущему protocol; они не выполнены этой документацией.

## Intake — смысл каждого назначенного файла

Корень incoming: `C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-evidence-audit-dd150aa5`.

| Файл / semantic scope | Что учтено |
|---|---|
| cards/SFT-EXP-REUSE-01.md, полный109строк | Один полезный новый фактор; H64 unknownS0,36fileclosure,16readouts, .001/TotalSteps4 и oldNEXT не перенесены автоматически. Назначены H16/PA01,87+28closure,.0003/2000,20readouts, route45→M1. Source operation-count не превращён в performance result. |
| 02-neural-diagnostic-conflict.md, полный41строка | Readout не доказывает fullskill; исходный H512 B0 не подменять H16. Предложенный pre-run NEEDS_DIAGNOSTIC_TRAINING_RECIPE не вводится поверх live29: fixed acquisition/resource attempts допустимы; whole-skill objective OPEN и retention требует measured before. Нет доказательства, что2updates обязательно дадут0. |
| 06-new-hypotheses-and-novelty.md, полный65строк | Ограниченная новизна SFT Exp, отличия R2/MASK, analytical memory tradeoff; reject log не повышен в новые ID. 1281 — inherited search coverage, не наш semantic review. |
| data/independent-review.md, полный61строка | Чужой reviewer `/root/units_retrieval` и его LF03 числа сохраняются как attributed historical review, не наш новый проход. Banner completed08/NEXT09 не закрывает пользовательскую учебную цель08 и не заменяет текущий маршрут. |
| data/sft-exp-novelty-search-receipt.json, полностью parsed/traversed | Все base/delta query/hit records разобраны структурно; поля path/line/text, counts/pins просмотрены. Это search receipt, не1248/1281 независимых semantic reviews. |

Свежий локальный адресный поиск до создания SEXPdocs охватил1313Markdown planning possibleDifs+canonicaldocs четырьмя regex. По BackwardLoss/exponentialSum21matches/8files, precise-repeat-Exp4matches/3files (в incoming), expVals40matches/26files. Exactrepeat-Exp вне входящего пакета не найден этим запросом. Четвёртый широкий SFT/softmax/reuse query дал контекстные hits; его полный output был усечён, точного count здесь не заявляю. Отобранные primary/ближайшие MASK/R2 прочитаны, не все1313файлов по смыслу. Это ограниченное отсутствие найденного дубля, не глобальная новизна softmax optimization.

## Границы и readback

Новый model/call budget назначен автором заранее, не пользователем; warmup доPASS, full-answer gate и новый checkpoint search не добавлены. Полный навык остаётсяOPEN при0/2→0/2, но измеренные cost/NLL/rank/partial tokens остаются своими фактами. Any measured useful improvement принимается по своей оси без нового minimum-gain порога; numerical tolerance служит equivalence check, не фильтром успеха. Ресурсная разница одного A→B не доказывает устойчивый speedup.

Context-mode execute/index первоначально вернул только индексированные sections; subsequent search/execute не завершились и были остановлены как зависшие tool cells. Дальше использовано bounded PowerShell чтение/анализ текста. Не заявляется успешный context search или экономия ранее прочитанного input. Canonical governance/skills были прочитаны байтово с извлечением targeted headings/guards; большой вывод усечён, полный semantic reload не заявляется. Phase/model/default/ownership правила взяты из прямого текущего поручения и live instructions.

После parent targeted review исправлены any-existing-R refusal, active owner guard для direct build/arm, отсутствие minimum NLL gain gate и ограничение роли Astra только текущим research поручением. Это правки текста, не новые экспериментальные проверки. Остаются реальные unverified: PA01 будущий module/status/handoff, actual dependency binary matching/ABI, сборка нового host, runtime cost/parity/learning outcome. Ни один из этих gaps не поручает исполнителю выбрать другой data/model/budget.

## Инвентарь фактически прочитанных текстов

Ниже SHA256 полного прочитанного text byte content; semantic scope отдельно. Кроме таблицы,87native text paths/hashes полностью перечислены в source contract: их byte verification не равно87семантическимreviews. Metadata28DLL не входят в text bytes inventory как прочитанные binaries.

| Полный путь | Read scope | SHA256 |
|---|---|---|
| C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-evidence-audit-dd150aa5/cards/SFT-EXP-REUSE-01.md | Полный semantic text,109lines | 4ba8de6a01c11e2c9f51f195f182d804744c453ccab2edaaf296e1d135250472 |
| C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-evidence-audit-dd150aa5/02-neural-diagnostic-conflict.md | Полный semantic text,41lines | 3633a3217f589c2d0d0d80b03b6141a9af95c76fa8b2c5148dfae6398e30bc95 |
| C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-evidence-audit-dd150aa5/06-new-hypotheses-and-novelty.md | Полный semantic text,65lines | 3b01013978a02faed07d5d3dcdd6cab8022f2221986c806498e925b8e5280e3d |
| C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-evidence-audit-dd150aa5/data/independent-review.md | Полный semantic text,61lines; attributed historical | ed0792b15dc103c235772f8b18dfbd9941690d676738855d7760010377361b8b |
| C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-evidence-audit-dd150aa5/data/sft-exp-novelty-search-receipt.json | Полный JSON parse и traversal всех query/hit records | 01bdc5c7aadbeadd1c16e06f5d2664fc1a7aa107521a8e52812c0b9fcbe5f2fb |
| C:/Projects/llm_mvp_core/04.TrainingLoop/Trainer.cs | Полные bytes; semantic1–147,377–430 | 3d82a7edd6afa6303e09f8a4cdd7a20abc05d457ac2dc33ea41b27aba761fb60 |
| C:/Projects/llm_mvp_core/04.TrainingLoop/TrainingLoop.csproj | Полный semantic project | 39db4a8f90493ec4e2e52b2f5b1fb31f52e83020a1b77d18e600fba674c30816 |
| C:/Projects/llm_mvp_core/04.TrainingLoop/TrainingConfig.cs | Полный semantic record | a57c3f820717504832e4ec1222f084e0d3d27d375de7f9b9d6db96b0f07d1a1b |
| C:/Projects/llm_mvp_core/04.TrainingLoop/Schedulers/CosineWithWarmup.cs | Полный semantic source | 3a1b6d7e9d1dca2bccb1665980db24a1962066bc908a36598f610b31e06ecfab |
| C:/Projects/llm_mvp_core/04.TrainingLoop/Optimizers/AdamW.cs | Полные bytes; targeted API/constructor/export/import references | 4ff288aef879a110dbdbc0da356d5839f1087e219d128a06cb987bf251abe3ea |
| C:/Projects/llm_mvp_core/04.TrainingLoop/Optimizers/AdamWPortableState.cs | Полные bytes/hash; codec not independently semantically re-reviewed this slice | 375c89181713679ac34a46839144a71299797e931127ad67951080e5490b563c |
| C:/Projects/llm_mvp_core/04.TrainingLoop/Optimizers/AdamWStateSnapshot.cs | Полные bytes/hash; no independent semantic rereview | d621e588d73080fee8a784380884b270d683bd17523785ef41043c35fdc9ff5b |
| C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/SupervisedTrainingExecution.cs | Полный semantic source1–32 | 279319decfd9241b7ed3013e07c9ce7c35ce983af078d9274059860e165357fe |
| C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorOps.cs | Полные bytes; semantic1582–1630 | e46bc0a9e6089b2712691c23beb9c5e5025fbf17a227b79924efd06890af4899 |
| C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs | Полные bytes; semantic1072–1127 | 09d03fc15b783384f730796773538dc60add748a35718058b5274c0c60200b48 |
| C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/Program.cs | Полные bytes; semantic30–140,207–279 | d003d02792b704fdfd248a3250f704ccc31fd47a83953a501e2de2addea29980 |
| C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/results/restore-minimal-set.json | Полный JSON parse; all15panel records+restore verification inspected; losses structurally parsed | bda100b0109e9024b2ca6ee3ddaf563ea81ddc16258910c1a7316f2d10e56e2b |
| C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63I/Native/ContinuousLearning.csproj | Полный semantic project1–47 | 162d51bab5c23159d6a3cda12472a6e40b2d7b5f7c48fe96c00527f5dcfe7fcb |
| C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/reconciliation/pa01-source-contract.json | Полный JSON parse;81source entries/29deps/9metadata traversed | feab69fa070c78a269d59fe40d28c261d3a3e5b762185561886678f83a43f1a5 |
| C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/reconciliation/sc01-source-contract.json | Полный JSON parse; targeted6TrainingLoop entries | 426b404d341a991e5ac87d010aae4b4a2eb12299314d35a69b80fba83f0567c4 |
| C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/PARENT-ADOPTION-01.md | Полные bytes; semantic ownership/module/profile/restore; truncated initial output recovered55–137 | 74d20910541ac884a15cd3501177cbdb98f9211b5ff8b5661226032fa297c4db |
| C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/E-SFT-RESPONSE-MASK.md | Полные bytes; semantic75–105 | b3cfd9b2fc44e17ca57f3c1cd3d2b8fe957df318f78727b9319900d1436376fb |
| C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N62-R2-01/report.md | Полные bytes; semantic1–18 and search73/75 | 2a886c8b76c8e89f085bc6ed709ac48470d5888f8dc95b50bd63f0bcea812943 |
| C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/29-learned-start-and-parent-continuation.md | Полный semantic text1–53 | f35e2c7c6892241d14064ecaf8e9f3260e51877478c1975ddb86fbfb9d1996d8 |

## Финальный адресный readback

Прочитаны текущие собственные card/prompt/evidence целиком как текст, source contract parsed. В card:5 default model и ограничение Astra именно текущим research; :36–46 exact87+28 и variant assembly references; :50–58 PA01/1000 и0/2полных ответов; :76 ceiling28Forward/8Backward; :86 early claims/any-existing-R/direct owner; :90–99 single command chain/finite limits; :110 any useful improvement без нового minimum-gain. Prompt:7/11/13 согласует guards/start/budget. JSON87entries/28deps, original TrainingLoop.dll0, TrainerA/B destinations совпадают с карточкой, queue45→IDENTITY-M1-01. Локальные Markdown-ссылки этих трёх собственных документов проверены на существование: broken0. Это статический readback документации, не compile/runtime proof и не approval всего корпуса.

Инвентарь выше содержит24 именованных текстовых источника с SHA;87 native source entries раскрыты отдельно в JSON и частично пересекаются с этой таблицей. Incoming novelty JSON: base query hits4/0/3/36, delta0/0/1/0 —44hit records всего. Чужая review-история не посчитана как наш новый независимый проход. Изменены ровно4 собственных live documentation paths; experiments/hosts/product/raw не менялись. Следующие действия исполнителя уже назначены рецептом, но отдельной execution authority этот пакет не даёт.
