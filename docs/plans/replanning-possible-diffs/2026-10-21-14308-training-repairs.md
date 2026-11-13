# 14308: исправление учебных тел карточек, 21.10.2026

Фаза — READ_ONLY_RESEARCH и назначенные Markdown-правки. Worker: /root/rp01_primary_evidence. Новых агентов, runtime, сборок, тестов, Encode, inference, Train, записи runner/host/data/weights/raw и Git mutations не было. Ни одно новое S0 не получено и не объявлено квалифицированным. Результат этого среза — применённый документный дизайн, а не успешное обучение.

## 1. Что применено

В телах15 действующих учебных карточек изменены preparation, carrier, state custody, qualification, конечный бюджет и before/after. Синхронизированы5 самостоятельных prompts; остальные используют встроенные prompts и явную обязательную связь с новым разделом. Адресно уточнены P07 и P43. Общие index/registry/24/26/27/30/31/README, POS50 и SiLU изменяет родитель; этот worker их не редактировал.

Причина ремонта: incoming08:37–38 и latest-core-review:59–61 отличают смену статуса от изменения фактического cold2/4-рецепта. Новые1000H512 и400H64 — **DESIGN_CHOICE_NOT_MEASURED**, заранее конечные авторские пределы, а не числа, достаточность которых доказана источником. Исторический H64 sanity400 с1/2 не доказывает2/2 на новом наборе.

Рабочий приоритет PA01 → N63IL → открытая цель N63C сохранён. Consumed N63C не получил повторного entry. Не импортируется H16 вH512/H64. Существующие claim/partial/result нельзя обойти новым root. Полная программа и одиночный ID не разрешают автоматический NEXT при невалидном learning objective; прямые STOP/NEXT Любы немедленны.

## 2. Охват входящего пакета

Корень входящих источников: `C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-21-learning-mechanism-audit/`. Planning pin: `14308ff181c5a93d837d92b85b2f35c41b863565`.

Все8 назначенных narrative-файлов прочитаны полностью содержательно. Части, усечённые при первой выдаче, дочитаны отдельными диапазонами. Таблица ниже — инвентарь чтения, не8 независимых review cycles. Чужие review logs прочитаны как чужие meta-claims; их проверки не присвоены worker.

| Файл относительно входящего корня | Байты | SHA256 полного текста | Содержательное решение |
|---|---:|---|---|
| 01-learning-paths.md |15498|0b0cdc25b1ffa5aebee8205269a67b34b697ba4355488285c0b5a66272818a42|Разделить native learner, state и consumer; POS12 и rule29 не одно обучение.|
| 02-card-audit.md |27011|f2ff43822a2416f374ef109c7482d4dd8318a3219839ce278655f80a020185cf|Список15 будущих рецептов обработан в телах; consumed N63C отдельно.|
| 03-n63c-amendment.md |8883|ae7da91449d3c60f1856b802d80542d4e55554f489ab2b724de6dc3cae0a3aa0|22/23 не qualified23/23; без новой попытки и без выдачи partial за полный ответ.|
| 08-latest-reconciliation.md |17259|c83f31934c69c1d5fb9617b21f261e6f6b4f900e9e68cd1706ae264af6bbfef5|Критика header-only применена; старые snapshots очереди не импортированы.|
| 09-n63c-framing.md |10755|81914b5b2024d3c4d0fa1782492821f2965eb39c27d9fc66631b207ee1fc2ccb|Common SFT carrier должен совпасть с DPO materialized chosen; причинность22/23 не доказана.|
| reviews/final-card-review.md |15223|d0f282755ea23d2a8f4d06633ab9b7836fd50fe122061b7acb7bbab7878566aa|Не принимать чужой PASS как собственный; закрыты конкретные start/budget/body пробелы.|
| reviews/latest-core-review.md |21227|80ac77a7fd061ffc8a07f9194990761dc98bec9d326321a30412d61d100803aa|Проверены authority/profile и state restore, а не только новые status labels.|
| reviews/latest-wait-reconciliation.md |15501|064ad01c65f7d4fc169d91271939ffc3d78217b36bd27b2037169acee9b66407|Healthy bounded wait разрешён; два быстрых poll не означают конец процесса/опыта.|

Пять JSON прочитаны полностью и структурно разобраны со всеми записями, без утверждения, что все referenced files семантически проверены:

| Относительный путь | Байты | SHA256 | Обработанный scope |
|---|---:|---|---|
| data/queue-inventory.json |67610|b97cff2526ff0bdc400756c93b5a4d46f4d223abd6ec66632561f93cb4729e5a|47 исторических rows;16 explicit-unqualified, из них15future и consumedN63C.|
| data/card-inventory.json |114068|a4f9743aa8d7433303406f3a7b413ca6a912fb8e562b28d842fb5a5445271c17|Root и все rows/fields; historical168 не fresh semantic review168.|
| data/latest-core-readback.json |9930|4042546caceaf26cb01bba5fe8a257c24a3656668017dc8fd03c0c5458334fbe|Все37 file entries.|
| data/latest-wait-readback.json |1210|014aec5646c725975795d6c756667b69d6814221e70ed4ff2dd8eb83feb1cdda|Все5 file entries.|
| data/n63c-primary-receipt.json |2376|ca6df691aacc86dae82091bd1956f1aa9df9d7917c731d8a5941acae88d5f1e6|Все9 file entries; receipt не доказательство binary custody.|

## 3. Конкретная общая подготовка10 H512

W0 загружается только штатным LoadModels с неизменными Step1/no-parent/InitialBaselineNoUpdate, config/tokenizer,55tensor/39base/16adapter guards. Frozen reference остаётся B0. OriginalA выполняет ровно1000 SFT updates на назначенных в каждой карточке строках; затем общий S0 fork вA/B. Константы: Batch1, SeqLen2048, LR=minLR=.001, Warmup0, TotalSteps1004, Accum1, Clip1, weight1, AdamW .9/.95/decay0/eps1e-8. Цикл ограничивает caller; TotalSteps не служит скрытым разрешением дополнительных calls.

DPO common carrier — сохранённые materialized ChosenIds до ExactMaterial.ChosenLength, без padding/LF/BOS/EOS/повторного BPE. MASK/MOMENT сохраняют собственный joint Prompt+LF+Chosen и response spans; ORDER отдельно сохраняет оба carrier. Common full-sequence SFT loss и masked response NLL не объявляются одной метрикой.

Каждая W0/S0 строка получает ровно один causal shifted forward: input IDs доL−2, target ID i+1, logit row i; metrics только назначенных response targets. Cost-карточки требуют хотя бы одну полную TRAIN response false→true. Это узкое запоминание, не превосходство над constant-answer baseline. MASK/MOMENT требуют полные2/2; ORDER полные4/4 и хотя бы один приобретённый ответ. Все строки и регрессии сохраняются.

`common/preparation.json.learning_qualified` — обязательный boolean. Только exit0 иtrue допускают arms. False после1000updates — PREPARATION_COMPLETED_LEARNING_UNQUALIFIED, учебная цельOPEN, без arms/retry/подбора нового seed/data/checkpoint. Это finite design, не обещание PASS.

S0.weights.bin сохраняется/валидируется/загружается native API; сравниваются все55 keyed tensors и39frozenbase. Portable AdamW сопоставляется по16реальным bindings, m/v bits, shapes иstep. MASK/MOMENT/ORDER импортируют SFTstep1000; DPO cost/PAD arms намеренно начинают новый DPO optimizer0, SFTm/v не импортируют. Это weights-only SFT→DPO исследовательская композиция, не canonical DPO continuation/adoption. ORDER SFTконец1002/DPO1; MOMENT A после сброса заканчиваетstep1, Bstep1002.

## 4. Источниковое замыкание authority, assembly и RNG

- [NativeTrainer](C:/Projects/llm_mvp_core/04.TrainingLoop/Trainer.cs:50): constructor TrainingConfig;67–147 public TrainSupervisedNextTokenStep, shifted targets102–109, Forward117/CE118/Backward125/optimizer135/clear144. TARGETED_READ32–147 и1–76.
- [PortableAdamW](C:/Projects/llm_mvp_core/04.TrainingLoop/Optimizers/AdamW.cs:188): Export/TryImport188–290. Успех TryImport не заменяет повторный export и побитное сравнение m/v/keys/step. TARGETED_READ.
- [Random state schema](C:/Projects/llm_mvp_core/03.TransformerModel/Randomness/LlmModelRandomState.cs:11): полный record1–39 прочитан; [Capture/Restore](C:/Projects/llm_mvp_core/03.TransformerModel/LlmModel.cs:286), TARGETED_READ286–375. Native Save/Load/Validate anchors использованы, бинарные checkpoint не загружались.
- [NativeDPOwriter](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/RepositoryOwnedDpoRandomStateFile.cs:46): TARGETED_READ46–97, BuildFileName и canonical filename refusal. `S0.rng.json` не передаётся этому writer.
- [N63CollisionProbe](C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/Rp01Micro/N63C/Common/ContinuousLearning/N63CollisionProbe.cs:264): TARGETED_READ253–340. LoadModels264 → freeze265 → LoadCheckpoint266 → PrepareBatches267 → governance279/content280 → recipient286/profile287 → LegalTeacher289/ServerIssue291 → lease304/mutation308 → CreateForProjectLora332 с native callbacks334/335. Это source proof допустимого порядка, не runtime PASS нового recipe.
- [DpoTrainer profile/recipient](C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:134): TARGETED_READ120–282. Profile связан с material/framing/objective иbinding keys/count, не с неизменными W0 adapter bytes. Recipient требует прежние instances/topology/frozen reference/base, допускает новые adapter values. В каждом новом recipe recipient/profile захватываются после S0 load; W0captured object не переиспользуется. [Import533–560](C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:533) требует DPOcontinuation proof, поэтому SFTm/v туда не выдаются.
- [IncrementalTrainingSession](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/IncrementalTrainingSession.cs:1): TARGETED_READ1–110; typed SFT Model/Tokenizer/Trainer/Optimizer owner сохранён, MASK/MOMENT не получают DPOlease.

Во всех14common recipes назначен явный private JSON codec: SerializeToUtf8Bytes полного LlmModelRandomStateSnapshot → FileMode.CreateNew; Deserialize того же native type → RestoreRandomState с исходной topology/lora identity → Capture и сравнение всех stream identities/seeds/cursors/metadata. Step1000H512 или400H64 является SFTmetadata. Первый DPO callback получает собственный step1 с сохранёнными cursor, filename только BuildFileName(bundle.Step). Это не поддельный DPOparent.

Closure проверена адресно, не одной формулой27DLL для всех: N61 —3libraries/101files,6TrainingLoop присутствуют в его составе189–198; NHEAD —5libraries/124files,6TrainingLoop перечислены226–231; fourE —3/101; GRADB иAGC/FBM —4/114;TS —3/101. Каждый новый PrepareLearnedStartAsync находится в точно назначенном copiedCL bridge; Program только диспетчеризует --prepare-learned. Метод/CLI будущие, не существующие проверенные executables. Existing library identities/friends сохраняются; новые grants не назначены. Selected Git comparison: DpoTrainer наab583/883496/current имеет blob0a7fd4d088fd37824aa213ddf0c6189675fe7dc2; Trainer —b63d550ba43e24a2b51be2c836519aab537d1312. Это проверка двух исходников, не всей binary ABI.

## 5. Четыре H64 и отдельный SC01

RMSCOORD/RMSROW/ROPE/ATTN: прежние native H64/V256/L1/Q4/KV2/FF176/Context64 и фиксированные streams, base62001/LoRA62002; никакого H16import. Common originalA ровно400SFT —200циклов буквальных `C# result sign: 9-4 =+` и `C# result sign: 4-9 =-`, byte IDs, безLF/BOS/EOS. LR=.001, Warmup0, TotalSteps404, clip1, AdamW.9/.95/decay0/eps1e-8. W0/S0 readout4F. Minimum одна полная однотокенная строка false→true;1/2 не выше constant1/2, второй ответ и регрессии всегда записываются. False → OPEN/noarms. Оба arms restore weights/optimizer400/RNG, по4updates до404. Snapshot codec и ранние claims такие же, полеlearning_qualified обязательное.

SC01 не получил H512/H64common. Сохранены H16donor3/40/full0/2, Prepare → Calibrate200 → CompareB200,400updates/1672F/808B. Учебная единица — две разные terminal labels One→1,Two→2, обе correct after и минимум одна false→true относительно before того же carrier. Это не whole-response fit. PositiveCalibrate не gate Compare другого carrier; terminal0→0 оставляет цельOPEN. Бюджет/данные/seed не расширяются. Existing phase deadlines: build60s×2, prepare120s×2, Calibrate600s, CompareA120s/B600s; это отдельные фазовые пределы, не двухминутный срок всего опыта.

## 6. Полные будущие бюджеты

Forwards включают подготовку, обучение и уже назначенные readouts; Prepare/Encode/Save/Load не скрываются под Forward и измеряются отдельно. Все числа ниже — пределы назначенного будущего протокола, **не фактический расход**. При qualification failure arm budget не расходуется; runtime partial может быть меньше. Нельзя добавлять модельные вызовы ради отчёта.

| ID | Updates common+arms | Forward всего | Backward | Generate | Whole deadline |
|---|---:|---:|---:|---:|---|
| LC-REF-CACHE |1000+4=1004|1072 при cache; консервативно≤1076|1016|0|14400s|
| E-SFT-RESPONSE-MASK |1000+4=1004|1048 без generation;≤2072 с generation|1004|16,≤1024tokens|14400s|
| E-SFT-MOMENT-CONTINUITY |1000+4=1004|1048 без generation;≤2072 с generation|1004|16,≤1024tokens|14400s|
| E-DPO-VALID-LENGTH |1000+4=1004|1028|1008|0|14400s|
| E-DPO-SFT-ORDER |1000+6=1006|1076 без generation;≤2100 с generation|1012|16,≤1024tokens|14400s|
| GRADB-LOCALITY-01 |1000+4=1004|1028|1008|0|14400s|
| DPO-TOKENIZER-SNAPSHOT-01 |1000+2=1002|1028|1008|0|14400s|
| N-HEAD-01 |1000+4=1004|1028|1008|0|14400s|
| GRAD-ACCUM-COPY-01 |1000+2=1002|1014=1008training+6diag|1004|0|14400s|
| FROZEN-BROADCAST-MAP-01 |1000+2=1002|1014=1008training+6diag|1004|0|14400s|
| RMS-COORD-ALLOC-01 |400+8=408|428=408training+20diag|408|0|1800s|
| ROPE-TRAIN-TABLE-01 |400+8=408|428|408|0|1800s|
| RMS-ROW-REDUCE-01 |400+8=408|428|408|0|1800s|
| ATTN-TRANSPOSE-STRIDES-01 |400+8=408|428|408|0|1800s|
| DPO-SHIFTED-CONTEXT-01 |200+200=400|1672|808|0|Назначенные phase limits выше|

H512 deadline распределён: preparation≤10800s, build≤600sкаждый, arm≤900sкаждый, summary≤120s;14400 ограничивает сумму. H64:prep≤1200s/build≤180s/arm≤120s/summary≤60s и общий1800. Healthy wait≤60s за вызов не меняет trainingbudget.

N61 trainingpolicy16/reference12 +EvaluateMany32 +DIAG8 +common1000training/4readout=1072F. Cachefailure может дать ещё4referenceF, не успех фактора. Native [EvaluateMany767–786](C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:767) и собственная таблица N61 подтверждают роли calls. Для3SFT/ORDERкарточек [TextGenerator191–243](C:/Projects/llm_mvp_core/09.Inference/TextGenerator.cs:191) прочитан:1prefill и не более63decode приMaxNewTokens64; поэтому16Generate≤1024дополнительных forward. Native [StudentGeneration61–75](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/CodeDocumentationStudentGeneration.cs:61) прочитан адресно. Никаких генераций здесь не выполнено.

## 7. N63C/P07 и POS — ограниченные выводы

[P07:170](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/P07-materialized-token-contrast.md:170) теперь адресно связывает известный материализационный фактор с N63C. Probe common124/treatment357 кодируют Prompt+LF+Chosen; DPOmaterial20–21/72–78/171–176 задаёт раздельные segments без separator/EOS. Common repair получает именно materialized ChosenIds доExactLength. Это carrier/segmentation correction, не новая гипотеза или доказанная причина22/23. Treatment не изменяется одновременно молча.

Свежий read-only parse `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63C-PROBE/results/n63c-probe.json` дал SHA256 `f0cf7a5dcff91120b386b78b6239fa9363d4cfad827823ab13e817f635bde9e9`. Сохранённые802updates=600commonSFT+200armSFT+2DPO;60scoringF;0Generate. Wauth22/23false, затем A20/23/B21/23; полного qualified23/23 нет. Current source constants60/.003/20 отличаются от raw600/.0001/100: binary/source custody не замкнута. Нельзя назначать существующему consumed root новый запуск или объявлять найденным полный learned before.

[P43:213](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/P43-contextual-pos.md:213) различает [project12 PatternClassifier13–86](C:/Projects/llm_mvp_core/12.SemanticCompiler/Translation/PatternClassifier.cs:13), Train34/42 и learned k3/Jaccard; [project29 PatternClassifier11–65](C:/Projects/llm_mvp_core/29.SemanticCompiler/Translation/PatternClassifier.cs:11), fixedInferPos безTrain; [fusion213–245](C:/Projects/llm_mvp_core/18.CapabilityTraining/LlmIngredientLogicalUnits.cs:213), vector.45/lexical.35/pattern.20. Эти источники прочитаны адресно. [E25:17–44](C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/E25PatternClassifier.cs:17) показывает Train до timer,160samples/split.8/32eval. Исторические11/32→28/32 — цитата incoming и отдельно прочитанного родителем raw, не новый runtime или самостоятельный raw-review worker. Новый POS50 родителя не выдаётся за ремонт project29consumer и не смешивается с этими15.

## 8. Изменённые live paths и readback

Все пути ниже относительно `C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/`. Начало нового common body у14карточек — строка18; у10H512 exactauthority20/RNG22/newCLI34–36/budget38–40. SC01 target qualification —118–122. Это удобные anchors текущего снимка, не утверждение неизменности номеров при внешней публикации.

| Live path | Применённое изменение |
|---|---|
| experimets/cards/N61-dpo-reference-reuse.md |CommonS0;точный3library bridge;cachefullbudget;embeddedno-extraSFT ограниченarm.|
| experimets/cards/E-SFT-RESPONSE-MASK.md |JointS0,2/2qual,optimizer1000,maskedvsfullarmfactor,оплаченнаяgeneration.|
| experimets/cards/E-SFT-MOMENT-CONTINUITY.md |JointS0,2/2qual;Areset1001→1/Bcarry→1002;keyedm/vcomparison.|
| experimets/cards/E-DPO-VALID-LENGTH.md |ИменноPADchosen/fullvalidlength common,новыйDPOepoch0;SFTspanpreflightнеперенесёнвPAD.|
| experimets/cards/E-DPO-SFT-ORDER.md |Четыре точныхcarrier,4/4qual,DPO0/SFT1000,final1/1002,сохранёнorderfactor.|
| experimets/cards/GRADB-LOCALITY-01.md |CommonS0,точный4librarybridge,actualR исправленвbody,ENRUнеактивирован.|
| experimets/cards/DPO-TOKENIZER-SNAPSHOT-01.md |Commonmaterializationотдельно;armPreparefactorнеподменён;common4+arm8readout.|
| experimets/cards/N-HEAD-01.md |CommonS0,5libs/124,selected-headfactorбезновойархитектуры.|
| experimets/cards/GRAD-ACCUM-COPY-01.md |Run-With soleowner→internalRunnonce;1002updates/1014F;embeddedbudgetисправлен.|
| experimets/cards/FROZEN-BROADCAST-MAP-01.md |Та жеfinitechain;frozen-mapfactorсохранён;старыйqueuecountубран.|
| experimets/cards/RMS-COORD-ALLOC-01.md |H64bytecommon400,portable400→404;wrapper→run.pyowner;20diag.|
| experimets/cards/ROPE-TRAIN-TABLE-01.md |H64common400,fullsingle-tokenacquisition;стараяonefactorropeтаблица.|
| experimets/cards/RMS-ROW-REDUCE-01.md |H64common400;originalApreparation,тотжеS0дляA/B.|
| experimets/cards/ATTN-TRANSPOSE-STRIDES-01.md |H64common400;stridefactorнеизменён;total408/428.|
| experimets/cards/DPO-SHIFTED-CONTEXT-01.md |Отдельныйterminalacquisitioncontract;400/1672/808неподмененыcommon1000.|
| experimets/cards/P07-materialized-token-contrast.md |АдресныйN63Cframingrepair,22/23/custodylimits.|
| experimets/cards/P43-contextual-pos.md |Trained12vsrule29,consumer/factorразделение.|
| experimets/prompts/GRAD-ACCUM-COPY-01-executor.md |Commonbudget/qualification/soleentry,defaultmodel/waitсохранены.|
| experimets/prompts/FROZEN-BROADCAST-MAP-01-executor.md |То же,точныйFBMroot.|
| experimets/prompts/RMS-ROW-REDUCE-01-executor.md |400common+8arm,optimizer400,privateRNG.|
| experimets/prompts/ATTN-TRANSPOSE-STRIDES-01-executor.md |400common+8arm,deadline1800,qualification.|
| experimets/prompts/DPO-SHIFTED-CONTEXT-01-executor.md |Terminallabelsacquisition,неwholefit,безновогоgateCalibrate.|

Это17карточек+5prompt, не полныйsemanticreview168карточек. Evidence — дополнительный23-й Markdown-файл. Readback включает точные изменённые разделы и их оперативные связи; не утверждается новый независимый review всего корпуса.

## 9. Skills, изменяющееся состояние и ограничения

TARGETED_READ в `C:/Projects/llm_mvp_core/docs/skills/`: phase-planning-SKILL.md26–72;plan-review-SKILL.md2406–2434;code-examination-SKILL.md110–165;experiment-design-SKILL.md43–78;prompt-patch-authoring-SKILL.md40–105;output-readability-gate-SKILL.md1–30/173–190;human-communication-GLOSSARY.md1–35. Это адресное применение, не полный свежийreloadAGENTS/CLAUDE/всехskills. Skills/AGENTS не редактировались.

Во время работы внешний coreHEAD изменялся. Worker наблюдал4a195dd390de61f5640becb380da11265590ad6e и финальным read-onlyrev-parse20b30f781d395d705e1d6c1bc6f97c88bc7d1dee. Родитель сообщил другие промежуточные внешниеpublication/WT01/WM01; worker этиexperiments не запускал, ихполныйrawaudit не присваивает. Gitcommit/push/pull/checkout не выдавались. Sourcepins карточек сохранены, movingHEAD не подменяет их. Внешняя публикация раннихdocs не означает авторствоGitоперации worker.

Остаётся непроверенным: build/ABI/privatefutureCLI, фактическая доступность каждогоbinarycarrier, успешность1000/400qualification, fullskilltransfer/heldoutgeneralization,23/23N63Ccheckpoint иsource/binarycustodyprobe. Проверены исходниковые контракты и применены конкретные documentrecipes; actual modelstate не исследовался исполнением. Если заранее конечная подготовка не квалифицируется, это открытая учебная задача с сохранёнными результатами, не основание увеличить budget или поставить PASS.

Финальный статический readback worker:23 собственных Markdown-файла,511 локальных Markdown-ссылок,0 отсутствующих targets. Проверка путей не доказывает semantic correctness/API/runtime. В пяти standalone prompts раздел конкретного учебного рецепта начинается со строки18. Никакого нового полного review исторических карточек этим счётчиком не заявлено.

