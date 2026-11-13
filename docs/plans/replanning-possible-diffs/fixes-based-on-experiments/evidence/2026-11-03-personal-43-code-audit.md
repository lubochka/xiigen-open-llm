# Личная проверка 43 ранее неопределённых переносов

Проверка 3 ноября 2026 года: **37 не реализованы, 6 реализованы частично; неопределённых строк нет**. Это 43 назначения из прежнего каталога 60, а не 43 утверждения об отсутствии любых алгоритмов. Например, native POS learner существует, но назначенное ему новое покрытие выполнено лишь частично.

Родитель лично прочитал указанные реализации, связанные вызовы и имеющиеся тесты; требования сопоставлены с предметными разделами карточек. `git blame` проверен для каждой из 43 опорных строк. Коммит ниже — происхождение прочитанного кода, **не коммит успешного переноса**. Итоговый учтённый срез — `918765f8ad4e486fab9629b9abb43b0e4c1f6651`. Во время чтения внешние исполнители изменяли CapabilityTraining и Wiki gate; эти изменения отдельно перечитаны. Они не добавили отсутствующие механизмы этих 43 строк. После смещения строк Wiki ссылка FIX-WK уточнена на текущий Featurize. Это сверка названного среза, а не обещание неизменности рабочей папки.

Сборки, тесты, inference и обучение здесь не запускались: разрешённая фаза — чтение и документация. Состояние кода определено; подтверждение работы по полному приёмочному сценарию отдельно остаётся необходимым. Названия будущих файлов проверялись вместе с фактическим поведением существующих методов, поэтому отсутствие переименованного класса само по себе не использовалось как доказательство.

## FIX-MC

**Код: не реализовано.** SFT продолжает обучение с новым AdamW при создании сессии; отдельного SupervisedOnly continuation/publication нет.

Доказательство: [14.ContinuousLearning/Training/IncrementalTrainingSession.cs:407](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/IncrementalTrainingSession.cs:407); происхождение строки: `254cf5ecb549`.

**Остаток:** Создать описанный в21 отдельный SFT continuation: допуск урока, keyed moments/RNG, транзакция и v3 publication. Наличие portable AdamW само по себе эту цепь не реализует.

## FIX-IL

**Код: частично.** Допуск 2…ContextLen+1 уже действует и в IncrementalTrainer, и в Trainer. Отдельный SFT-вход и SupervisedContextAdmission из карточки не сделаны.

Доказательство: [14.ContinuousLearning/Training/IncrementalTrainer.cs:402](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/IncrementalTrainer.cs:402); происхождение строки: `01b06b6c0334`.

**Остаток:** Сохранить имеющуюся границу длины; после MC подключить её к SFT-only пути и добавить адресное учебное покрытие. Не повторять уже исправленную проверку.

## FIX-SC

**Код: не реализовано.** Exact-пара хранит полный prompt+response, ContextLen проверяется до обычного полного Forward; профиль shifted input отсутствует.

Доказательство: [11.DPO/DpoTrainer.cs:1192](C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:1192); происхождение строки: `1640de60c1ae`.

**Остаток:** Добавить согласованный input[0..T-2]/target[1..T-1], допуск T=C+1, версию исполнения и идентичность cache/restore по57. Существующий next-token индекс ещё не этот перенос.

## FIX-SEP

**Код: не реализовано.** SFT кодирует Prompt+LF+Response; общего SupervisedTextSequence и соответствующего saved-state response reader нет.

Доказательство: [14.ContinuousLearning/Training/IncrementalTrainer.cs:402](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/IncrementalTrainer.cs:402); происхождение строки: `01b06b6c0334`.

**Остаток:** Вынести единый формат SFT и подключить к reader/диагностике остановки; exact-DPO без разделителя сохранить. Проверка по62: сохранённый старт,6Forward,0updates.

## FIX-FSR

**Код: не реализовано.** ActionLabelGenerator считает любой member access основанием null-check, не определяет статический receiver семантической моделью.

Доказательство: [12.SemanticCompiler/Prediction/FileActionKnn.cs:32](C:/Projects/llm_mvp_core/12.SemanticCompiler/Prediction/FileActionKnn.cs:32); происхождение строки: `b10027649079`.

**Остаток:** Различить static IFieldSymbol и instance receiver в генераторе метки; проверить через TrainFileActionKnn и Compile с полным учётом вложенных fits.

## FIX-PD

**Код: не реализовано.** Обе стороны выровнены до max length, DpoTrainer передаёт все padded IDs в model.Forward.

Доказательство: [11.DPO/DpoTrainer.cs:1206](C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:1206); происхождение строки: `d81a2f29a6f3`.

**Остаток:** Добавить внутреннее представление исполнения по valid length, сохранив исходные exact material/digest и полный target. Сравнить собственный DPO старт, не импортировать SFT moments.

## FIX-TS

**Код: не реализовано.** PrepareExecution вызывает BuildBatch в цикле; каждый exact CreateBatch сам Save/Load tokenizer.

Доказательство: [11.DPO/Models/DpoExactPairMaterial.cs:69](C:/Projects/llm_mvp_core/11.DPO/Models/DpoExactPairMaterial.cs:69); происхождение строки: `d81a2f29a6f3`.

**Остаток:** Один закрытый immutable snapshot на вызов PrepareExecution с предусмотренным observer fallback; не singleton и не cache между вызовами.

## FIX-FBC

**Код: не реализовано.** RequireValid по-прежнему сравнивает базу через Select(SingleToInt32Bits).SequenceEqual; проверки до/после optimizer присутствуют.

Доказательство: [11.DPO/DpoTrainer.cs:266](C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:266); происхождение строки: `1640de60c1ae`.

**Остаток:** Заменить только механизм полного bitwise сравнения, сохранить каждую точку RequireValid и отрицательные проверки мутации базы.

## FIX-OSR

**Код: не реализовано.** Для RNG-снимка по-прежнему читается ExportOptimizerState().Step, что экспортирует moments.

Доказательство: [11.DPO/DpoTrainer.cs:1441](C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:1441); происхождение строки: `5bd518548d73`.

**Остаток:** Добавить чтение step без экспорта moments; общий перенос пока требует выбора по пику памяти +6,35446%, либо отдельной квалификации ограниченного профиля.

## FIX-BWS

**Код: не реализовано.** new float[v] расположен внутри цикла строк BackwardLogProbs.

Доказательство: [11.DPO/DpoTrainer.cs:1302](C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:1302); происхождение строки: `fa152bc018c4`.

**Остаток:** Один scratch на вызов с полной перезаписью каждой строки; общий допуск зависит от решения по пику +15,91324%.

## FIX-IGC

**Код: не реализовано.** Backward(initialGradient) делает ToArray, затем TensorStorage копирует массив второй раз.

Доказательство: [00.TensorCore/Core/Tensor.cs:99](C:/Projects/llm_mvp_core/00.TensorCore/Core/Tensor.cs:99); происхождение строки: `195840c0c4f0`.

**Остаток:** Убрать внешнюю копию, сохранив owned storage; проверить отсутствие alias и неизменность фактического DPO backward/state.

## FIX-LFT

**Код: не реализовано.** Linear.Forward каждый раз транспонирует Weight; frozen transpose scope отсутствует.

Доказательство: [00.TensorCore/Layers/Linear.cs:49](C:/Projects/llm_mvp_core/00.TensorCore/Layers/Linear.cs:49); происхождение строки: `fd5a5a954121`.

**Остаток:** Ввести ограниченный SFT scope для immutable frozen weights; явно инвалидировать после изменения/restore, без общего долгоживущего cache.

## FIX-AGC

**Код: не реализовано.** AccumulateGrad для первого вклада делает ToArray до уже копирующего TensorStorage.

Доказательство: [00.TensorCore/Core/TensorOps.cs:1146](C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorOps.cs:1146); происхождение строки: `ff3edc7428fa`.

**Остаток:** Оставить одну защитную копию; accumulation последующих вкладов и владение памятью сохранить.

## FIX-FBM

**Код: не реализовано.** MultiplyBroadcast выделяет mappedBIndices до проверки RequiresGrad, в том числе для полностью замороженной операции.

Доказательство: [00.TensorCore/Core/TensorOps.cs:169](C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorOps.cs:169); происхождение строки: `8a8cad4a2302`.

**Остаток:** Создавать карту только при наличии gradient path; обе ветви backward и значения forward сохранить.

## FIX-AC

**Код: не реализовано.** Softmax сначала копирует все x в resultData и лишь затем перезаписывает результат exp/normalization.

Доказательство: [00.TensorCore/Core/TensorOps.cs:952](C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorOps.cs:952); происхождение строки: `fd5a5a954121`.

**Остаток:** В квалифицированном last-axis пути читать x напрямую и один раз писать выход; сохранить generic fallback, порядок float и градиент.

## FIX-RPE

**Код: не реализовано.** Каждый CausalSelfAttention.Forward заново вызывает GetFrequencies; внутри заново строятся frequency/position/outer/cos/sin.

Доказательство: [03.TransformerModel/Layers/CausalSelfAttention.cs:148](C:/Projects/llm_mvp_core/03.TransformerModel/Layers/CausalSelfAttention.cs:148); происхождение строки: `fd5a5a954121`.

**Остаток:** Добавить training-only reuse с явным сроком жизни и идентичностью dim/theta/length/offset; inference и offset контракт не менять.

## FIX-SSR

**Код: не реализовано.** Silu вычисляет sigmoid в forward и заново в backward; training opt-in нет.

Доказательство: [00.TensorCore/Core/TensorOps.cs:1363](C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorOps.cs:1363); происхождение строки: `8a8cad4a2302`.

**Остаток:** Сохранить sigmoid в явно выбранном training режиме; RequiresGrad не использовать как единственный переключатель режима.

## FIX-SEXP

**Код: не реализовано.** BackwardLoss вычисляет MathF.Exp дважды для каждого token: в знаменателе и в softmax.

Доказательство: [04.TrainingLoop/Trainer.cs:422](C:/Projects/llm_mvp_core/04.TrainingLoop/Trainer.cs:422); происхождение строки: `41dccce8edea`.

**Остаток:** Один float[V] scratch на вызов с reuse exp по позиции; прежние scale/gradient и готовый SFT donor сохранить.

## FIX-GB

**Код: не реализовано.** Правый градиент MatMul всё ещё обходит inner→column→row.

Доказательство: [00.TensorCore/Core/TensorOps.cs:688](C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorOps.cs:688); происхождение строки: `8a8cad4a2302`.

**Остаток:** Перенести локальный kernel gradB из опыта; не менять gradA/SIMD и остальной порядок обучения.

## FIX-NH

**Код: не реализовано.** Policy и reference вызывают обычный полный model.Forward; выбранные scored rows в отдельную выходную голову не передаются.

Доказательство: [11.DPO/DpoTrainer.cs:1223](C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:1223); происхождение строки: `8a8cad4a2302`.

**Остаток:** После полного context выбирать строки для output head с обратным scatter; не сокращать входной контекст.

## FIX-RMS

**Код: не реализовано.** MultiplyBroadcast вызывает FlatToIndices для каждого элемента, создавая массив координат.

Доказательство: [00.TensorCore/Core/TensorOps.cs:174](C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorOps.cs:174); происхождение строки: `8a8cad4a2302`.

**Остаток:** Вычислять индекс broadcast без массива на элемент; сохранить mapped backward и RMS числа.

## FIX-BR

**Код: не реализовано.** BPE выбирает MaxBy по CountPairs; явного objective исполнимых неперекрывающихся слияний нет.

Доказательство: [01.Tokenizer/BpeTokenizer.cs:164](C:/Projects/llm_mvp_core/01.Tokenizer/BpeTokenizer.cs:164); происхождение строки: `ff3edc7428fa`.

**Остаток:** Добавить только явный opt-in objective с сохранением legacy default; по опыту часть EXAM хуже, поэтому общий default не разрешён.

## FIX-KHC

**Код: не реализовано.** Фасад отдельно вызывает FeatureVector(source) и Label(source); оба метода независимо ParseText.

Доказательство: [12.SemanticCompiler/SemanticCompilerFacade.cs:248](C:/Projects/llm_mvp_core/12.SemanticCompiler/SemanticCompilerFacade.cs:248); происхождение строки: `a6d9ef996088`.

**Остаток:** Добавить общий parse/extraction для одной строки и использовать в настоящем фасаде. Метки не менять в этой оптимизации.

## FIX-JO

**Код: не реализовано.** В TrainOnExample исполнителя и проверяющего по-прежнему создаётся новый JsonSerializerOptions; общий checkpoint policy не покрывает эти вызовы.

Доказательство: [27.TranslationDomain/TranslationTrainedUnits.cs:462](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationTrainedUnits.cs:462); происхождение строки: `23c68ae11894`.

**Остаток:** Общая неизменяемая JSON policy для указанных учебных readers; совместимость enum/restore сохранить.

## FIX-IDX

**Код: не реализовано.** Каждый урок проверяет дубликат через stagedIdentities.Any по всему списку.

Доказательство: [27.TranslationDomain/TranslationMachine.cs:328](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationMachine.cs:328); происхождение строки: `a28bc3551464`.

**Остаток:** Добавить локальный HashSet пары ExampleId/UnitId рядом со staged list; порядок первого появления и атомарную публикацию сохранить.

## FIX-N60

**Код: не реализовано.** Builder безусловно добавляет отрицательный verifier lesson для rejectedCandidate, даже если он равен chosen.

Доказательство: [27.TranslationDomain/TranslationStructuredBatchBuilder.cs:154](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationStructuredBatchBuilder.cs:154); происхождение строки: `b31e40a5b759`.

**Остаток:** Оборачивать только verifier Add в ordinal guard chosen!=rejectedCandidate; не снижать общую кратность других builder. По опыту false accepts0→1: расширение допуска требует отдельного решения/ремонта.

## FIX-VA

**Код: не реализовано.** TryParseIssue использует Split по апострофу и required=parts[1]; внутренний апостроф обрывает термин.

Доказательство: [27.TranslationDomain/TranslationTrainedUnits.cs:1486](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationTrainedUnits.cs:1486); происхождение строки: `23c68ae11894`.

**Остаток:** Извлекать весь quoted term по границам формата. Проверить положительные и отрицательные смысловые случаи: исторические false rejects2/5 не скрывать.

## FIX-NKU

**Код: не реализовано.** hasLoop перечисляет foreach/for/while, но не DoStatement.

Доказательство: [12.SemanticCompiler/Prediction/FileActionKnn.cs:50](C:/Projects/llm_mvp_core/12.SemanticCompiler/Prediction/FileActionKnn.cs:50); происхождение строки: `b10027649079`.

**Остаток:** Добавить do-while в генератор метки, сохранить контроль действительно нужного цикла. Утрата1→0 в прежнем опыте не разрешает общий перенос без устранения/выбора цены.

## FIX-NG

**Код: не реализовано.** Любое IsPatternExpression внутри if считается null-condition, включая прямой int-pattern.

Доказательство: [12.SemanticCompiler/Prediction/FileActionKnn.cs:33](C:/Projects/llm_mvp_core/12.SemanticCompiler/Prediction/FileActionKnn.cs:33); происхождение строки: `b10027649079`.

**Остаток:** Сузить распознавание null guard до предусмотренного pattern; отдельные Train до/после и прежние семейства меток сохранить.

## FIX-RET

**Код: не реализовано.** Train перезаписывает _weights[FlowRef]=BuildWeights(pair), не сохраняет совместимые признаки прежнего парафраза.

Доказательство: [42.ProjectWikiDomain/PromptFlowGroundingModel.cs:23](C:/Projects/llm_mvp_core/42.ProjectWikiDomain/PromptFlowGroundingModel.cs:23); происхождение строки: `09509e960b34`.

**Остаток:** Объединять признаки только совместимых уроков одного flow; correction не смешивать с paraphrase. Проверить ordinary plan consumer и restore.

## FIX-WK

**Код: не реализовано.** Unigram token и bigram token_token помещаются в один словарь без раздельного codec.

Доказательство: [42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs:558](C:/Projects/llm_mvp_core/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs:558); происхождение строки: `986438b893de`.

**Остаток:** Явный legacy/v2 codec и checkpoint migration; общий default требует решения по allocations+42,89%, snapshot+4,09%.

## FIX-WT

**Код: не реализовано.** В Labels остаются15 классов; SystemBoundaryMapperTraining/CrossStackBusinessFlowGraphTraining отсутствуют.

Доказательство: [42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs:25](C:/Projects/llm_mvp_core/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs:25); происхождение строки: `986438b893de`.

**Остаток:** Добавить два класса и явный NotProjectWiki fallback, миграцию15→17 с повторной оценкой gate. Провал gate и отрицательного контроля запрещает объявлять общий перенос принятым.

## FIX-E2E

**Код: частично.** OrchestrationPlanOrchestrator генерирует PlanText и predicted_flow_ref, но не исполняет специалиста по каталогу выбранного FlowRef.

Доказательство: [42.ProjectWikiDomain/OrchestrationPlanOrchestrator.cs:32](C:/Projects/llm_mvp_core/42.ProjectWikiDomain/OrchestrationPlanOrchestrator.cs:32); происхождение строки: `1f925da57378`.

**Остаток:** Подключить назначенный prompt→domain→unit dispatcher и повторить одну панель после cold restore. Генерация плана и direct KNN tests не заменяют весь маршрут.

## FIX-N63C

**Код: не реализовано.** TrainCoreAsync сохраняет исходные examples и кодирует их Response после DPO; ordinal замены rejected на chosen нет.

Доказательство: [14.ContinuousLearning/Training/IncrementalTrainer.cs:186](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/IncrementalTrainer.cs:186); происхождение строки: `e79ad635c4ce`.

**Остаток:** Добавить correction projection после допуска и до provenance/tokenSequences, записать обе source identities. Не путать с существующим DpoPairTrainingContentAuthority.Project: он проверяет DPO-пару, не переписывает SFT-урок.

## TEST-COLLISION-REPAIR

**Код: не реализовано.** Назначенных CollisionRepairLearningTests/Fixture и общего сравнения completed receipts в Tests нет; native Trainer остаётся доступным.

Доказательство: [04.TrainingLoop/Trainer.cs:67](C:/Projects/llm_mvp_core/04.TrainingLoop/Trainer.cs:67); происхождение строки: `01b06b6c0334`.

**Остаток:** Добавить только назначенное покрытие damaged donor по59/63:58Forward/10updates. Не создавать новый scheduler и не менять gold.

## FIX-N63R

**Код: не реализовано.** ReplayBuffer.Add удаляет общий старейший диапазон; защита последнего урока домена отсутствует.

Доказательство: [16.AutonomousLearning/Training/ReplayBuffer.cs:36](C:/Projects/llm_mvp_core/16.AutonomousLearning/Training/ReplayBuffer.cs:36); происхождение строки: `dcc1a5c206e77`.

**Остаток:** При переполнении сначала вытеснять старейшую строку домена с дублями, затем общий FIFO если все уникальны; Sample/ratio/seed сохранить. Историческая цена и разная доза остаются в отчёте.

## FIX-MS

**Код: не реализовано.** TrainSupervisedNextTokenStep обучает все targets с weight/sequenceLength; response mask и явного target-mode нет.

Доказательство: [04.TrainingLoop/Trainer.cs:143](C:/Projects/llm_mvp_core/04.TrainingLoop/Trainer.cs:143); происхождение строки: `01b06b6c0334`.

**Остаток:** Ввести только выбранный response-target opt-in с identity в SFT continuation. Не менять AllTargets/default; вместе с MC смена цели требует NewSftEpoch.

## TEST-POS

**Код: частично.** Есть native Train и построчная панель128/32 с проверкой11→28 правильных; это покрытие FIX-PQ, а не назначенный balanced6+4 fixture карточки34.

Доказательство: [Tests/ExperimentFixes/PosQueryBigramPanelTests.cs:112](C:/Projects/llm_mvp_core/Tests/ExperimentFixes/PosQueryBigramPanelTests.cs:112); происхождение строки: `c913304a9647`.

**Остаток:** Сохранить существующую панель; добавить собственные6+4, противопоставление порядка и facade-проверку по34. Не приписывать их старому тесту.

## TEST-SK

**Код: частично.** Есть native TrainFromSource и16 построчных до/после ответов; назначенная двухсемейная6+4 панель/фасад карточки37 отсутствуют.

Доказательство: [Tests/ExperimentFixes/StructuralAstAnswerPanelTests.cs:126](C:/Projects/llm_mvp_core/Tests/ExperimentFixes/StructuralAstAnswerPanelTests.cs:126); происхождение строки: `b4fb340fb5cf`.

**Остаток:** Добавить отсутствующий acquisition/facade contrast по37, сохранить уже существующую historical AST панель. Не объявлять сам StructuralKnn отсутствующим.

## TEST-ORDER

**Код: не реализовано.** Native IncrementalTrainer исполняет DPO, затем SFT; назначенных TrainingStageOrderLearningTests и Or01 fixture в Tests нет.

Доказательство: [14.ContinuousLearning/Training/IncrementalTrainer.cs:658](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/IncrementalTrainer.cs:658); происхождение строки: `5f98eec4bc29`.

**Остаток:** Добавить prescribed restored-state покрытие стадий по25. Порядок production DPO→SFT сохранить; узкий SFT→DPO кандидат не общий default.

## TEST-TCTX

**Код: не реализовано.** Существующие tests читают BuildDefault и непустую строку; назначенного AddEntry acquisition до/после поTTrainContext01 нет.

Доказательство: [Tests/SemanticCompilerTests.cs:216](C:/Projects/llm_mvp_core/Tests/SemanticCompilerTests.cs:216); происхождение строки: `7f4e3bbacc1f`.

**Остаток:** Создать закреплённые train/exam/gold и ContextualDictionaryLearningTests; проверять обучение контекстному выбору, не только BuildDefault.

## TEST-TMEM

**Код: частично.** Native EN→RU обучение/перевод и checkpoint tests существуют; отдельной закреплённой24-row панели двух learned starts из19 нет.

Доказательство: [Tests/TranslationDomainTests.cs:412](C:/Projects/llm_mvp_core/Tests/TranslationDomainTests.cs:412); происхождение строки: `53101093cb64`.

**Остаток:** Добавить собственный EN→RU before/after/cold panel без подмены ответов checkpoint gold; учитывать актуальный CAS/PAIR контракт.

## TEST-E22

**Код: частично.** Есть native dataset-router обучение трём маскам и отказ пустому запросу; исходники/AutoLabel/6+4 acquisition panel карточки27 не покрыты этой проверкой.

Доказательство: [Tests/ExperimentFixes/DatasetRouterLearningStateTests.cs:81](C:/Projects/llm_mvp_core/Tests/ExperimentFixes/DatasetRouterLearningStateTests.cs:81); происхождение строки: `d3df5e257237`.

**Остаток:** Добавить закреплённые6 source lessons+4 exam, AutoLabel/provenance и native extractor. Исторический450-row corpus не выдавать за восстановленный.

## Как не повторить ошибку индекса

Наличие portable optimizer не означает SFT continuation; сохранённая метка маршрута не означает вызов специалиста; общий JSON policy для checkpoint не означает reuse в учебном ingress. В карточке исполнитель обязан назвать собственный вызываемый путь, а проверяющий — прочитать его до конечного потребителя. Уже существующий тест с другим набором сохраняется и учитывается ровно в своей области; его нельзя ни объявлять отсутствующим, ни использовать для закрытия ненаписанного сценария.

Одна работа имеет один основной статус. «Не запускали тесты» — характеристика доказательств, а не замена статуса реализации. При наличии частичного кода следующий шаг называет конкретно отсутствующий участок. При техническом пробеле не требуется решение Любы; решение нужно только для явно названной цены или изменения поведения вне уже разрешённых условий.
