# Аудит DPO / LoRA: новые учебные различия и незакрытые эксперименты

**Дата:** 19 октября 2026. **Статус:** READ_ONLY_SOURCE_AUDIT; NATIVE_TRAINING_NOT_RUN.  
**Core source snapshot:** 8e020052fae7efa694b8d33f90768648d6b67af9, ветка codex/dpo-implementation. Поздняя сверка c3f1d0d4fc7fb7949c51d2251e89d2fb0e590d72: изменились только docs; приведённый source код тот же.  
**Авторский сектор:** DPO material → LoRA forward/backward → optimizer → incremental continuation.  
**Защищённый каталог docs/plans/replanning-possible-diffs не изменён.** Карточки ниже — предложения в planning, а не исправленные оригиналы и не результаты запущенного обучения.

## 1. Вывод

По текущему коду выделены два узких кандидата, которые нельзя смешивать с уже известными темами «случайная B0», «нужна другая LoRA», «EOS отсутствует» или «teaching vector учитывает только плотность».

1. **ND-DPO-SFT — последовательность настоящего incremental обучения отличается от RP01 simulation.** IncrementalTrainer сначала выполняет DPO, затем обязательно обучает те же адаптеры на исходных C# файлах через full-sequence SFT с другим оптимизатором. Финальный адаптер представляет результат обоих этапов. Это может усиливать либо ослаблять достигнутую DPO-пользу. Направление эффекта не измерено. Данный путь не является прямым RP01 simulation, поэтому найденная последовательность не объявляется причиной его плохих ответов.
2. **ND-DPO-PAD — лишние дополненные позиции вычисляются внутри каждого настоящего DPO update.** Exact material дополняет обе стороны до длины более длинной; четыре forward и policy backward выполняются на полных тензорах, хотя objective исключает нулевой хвост. Для dropout=0 причинно обоснована гипотеза об уменьшении времени/памяти настоящего обучения посредством внутреннего execution view до собственной валидной длины стороны. Это не меняет выбранный человеком урок и не является standalone serving benchmark.

**SC-SFT-STATE** — дополнительное уточнение покрытия: новое создание Trainer и AdamW на каждой сессии — подтверждённый факт. В P12.5 уже есть опыт со сбросом scheduler при том же optimizer, но отдельного маленького опыта со сбросом именно SFT moments/step при неизменном LR в прочитанных карточках не найдено. Общая тема optimizer restore уже известна, поэтому это не третья «новая общая проблема».

Все предполагаемые изменения касаются выполняемого Train/Update. Ни один serving-only или file-only опыт не предлагается как эксперимент обучения.

## 2. Источники и причинные границы

Все ссылки этого раздела закреплены на core SHA 8e020052fae7efa694b8d33f90768648d6b67af9.

| ID | Проверяемый факт | Первичный источник |
|---|---|---|
| S01 | ProjectIndexer сканирует C#; SFT Prompt — имя файла, Response — полный исходник | [ProjectIndexer.cs, 42–78](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/14.ContinuousLearning/Indexer/ProjectIndexer.cs#L42-L78) |
| S02 | Pipeline передаёт indexed domain examples и DPO пары вместе | [ContinuousLearningPipeline.cs, 173–181](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/14.ContinuousLearning/Pipeline/ContinuousLearningPipeline.cs#L173-L181), [208–212](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/14.ContinuousLearning/Pipeline/ContinuousLearningPipeline.cs#L208-L212), [239–243](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/14.ContinuousLearning/Pipeline/ContinuousLearningPipeline.cs#L239-L243) |
| S03 | Вход IncrementalTrainer требует непустые examples; пустой SFT набор не является штатным способом оставить только DPO | [IncrementalTrainer.cs, 1149–1185](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/14.ContinuousLearning/Training/IncrementalTrainer.cs#L1149-L1185) |
| S04 | SFT последовательность кодируется как Prompt + LF + Response | [IncrementalTrainer.cs, 385–419](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/14.ContinuousLearning/Training/IncrementalTrainer.cs#L385-L419) |
| S05 | На session.Model применяются LoRA и parent adapter; далее создаётся независимый reference | [IncrementalTrainer.cs, 502–550](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/14.ContinuousLearning/Training/IncrementalTrainer.cs#L502-L550) |
| S06 | DPO executor получает session.Model, reference и tokenizer | [IncrementalTrainer.cs, 581–595](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/14.ContinuousLearning/Training/IncrementalTrainer.cs#L581-L595) |
| S07 | После DPO проходит отдельный SFT update для каждого example на тех же adapterParameters | [IncrementalTrainer.cs, 645–671](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/14.ContinuousLearning/Training/IncrementalTrainer.cs#L645-L671) |
| S08 | После обоих этапов сохраняется session.Model как финальный adapter | [IncrementalTrainer.cs, 737–761](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/14.ContinuousLearning/Training/IncrementalTrainer.cs#L737-L761) |
| S09 | Каждая стандартная checkpoint-backed session получает новый Trainer с WarmupSteps=0 и новый AdamW | [IncrementalTrainingSession.cs, 403–421](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/14.ContinuousLearning/Training/IncrementalTrainingSession.cs#L403-L421) |
| S10 | Supervised executor передаёт session.Optimizer; он не использует приватный optimizer DpoTrainer | [SupervisedTrainingExecution.cs, 19–31](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/14.ContinuousLearning/Training/SupervisedTrainingExecution.cs#L19-L31), [DpoTrainer.cs, 470–476](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/11.DPO/DpoTrainer.cs#L470-L476) |
| S11 | SFT targets охватывают все next-token позиции; CE усредняется по всей длине, затем backward, clipping, AdamW.Step | [Trainer.cs, 102–147](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/04.TrainingLoop/Trainer.cs#L102-L147) |
| S12 | Actual RP01 simulation вызывает StepVector напрямую, dropout != 0 запрещён | [CodeDocumentationSimulationDpoExperiment.cs, 103–112](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L103-L112), [160–190](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L160-L190) |
| S13 | Exact material хранит ChosenLength, RejectedLength, общий max SequenceLength, отдельные бинарные response masks | [DpoExactPairMaterial.cs, 31–45](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/11.DPO/Models/DpoExactPairMaterial.cs#L31-L45) |
| S14 | Padded tensors заполняются нулями только после фактического конца; digest включает оригинальные IDs/маски и тексты | [DpoExactPairMaterial.cs, 171–202](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/11.DPO/Models/DpoExactPairMaterial.cs#L171-L202) |
| S15 | StepVector исполняет два reference и два policy forward, два backward, затем один optimizer update | [DpoTrainer.cs, 878–924](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/11.DPO/DpoTrainer.cs#L878-L924) |
| S16 | Forward получает целый side tensor; scored targets читаются как ids[i+1], mask[i+1] | [DpoTrainer.cs, 1002–1030](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/11.DPO/DpoTrainer.cs#L1002-L1030), [1044–1068](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/11.DPO/DpoTrainer.cs#L1044-L1068) |
| S17 | Backward нулевой для targets с mask=0; exact sum не делится на длину padded tensor | [DpoTrainer.cs, 1080–1117](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/11.DPO/DpoTrainer.cs#L1080-L1117) |
| S18 | Attention строит dense [B,h,T,T], затем зануляет будущие позиции через -Infinity; нет доступа valid prefix к будущему padding | [CausalSelfAttention.cs, 129–168](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/03.TransformerModel/Layers/CausalSelfAttention.cs#L129-L168), [263–291](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/03.TransformerModel/Layers/CausalSelfAttention.cs#L263-L291) |
| S19 | Causal mask backward пропускает градиент только до текущей позиции | [TensorOps.cs, 1411–1448](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/00.TensorCore/Core/TensorOps.cs#L1411-L1448) |
| S20 | RoPE positions 0..T−1 имеют те же значения при уменьшении только правой границы | [RotaryEmbedding.cs, 26–68](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/03.TransformerModel/Layers/RotaryEmbedding.cs#L26-L68) |
| S21 | RMSNorm агрегирует последнюю hidden-ось, FFN и residual работают по позициям | [RmsNorm.cs, 32–43](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/03.TransformerModel/Layers/RmsNorm.cs#L32-L43), [TransformerBlock.cs, 47–66](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/03.TransformerModel/TransformerBlock.cs#L47-L66) |
| S22 | Dropout=0 возвращает input, не расходуя случайный поток | [Dropout.cs, 56–60](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/00.TensorCore/Layers/Dropout.cs#L56-L60) |
| S23 | AdamW m/v и step определяют следующий update; перенос состояния по стабильным bindings реализован | [AdamW.cs, 69–107](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/04.TrainingLoop/Optimizers/AdamW.cs#L69-L107), [182–307](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/04.TrainingLoop/Optimizers/AdamW.cs#L182-L307) |
| S24 | Logical DPO owner заново определяет step-zero reference: грузит parent в обе модели либо копирует текущую policy | [LlmIngredientLogicalUnits.cs, 1260–1283](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L1260-L1283), [2193–2199](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L2193-L2199), [2235–2248](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L2235-L2248) |
| S25 | В mixed IncrementalTrainer MaterialVersion не задан: request default LegacyVersion; stable exact-v2 ограничен domain code-documentation | [DpoTrainingExecution.cs, 17–39](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/14.ContinuousLearning/Training/DpoTrainingExecution.cs#L17-L39), [421–445](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/14.ContinuousLearning/Training/DpoTrainingExecution.cs#L421-L445), S06 |
| S26 | Direct exact DPO composition существует: recipient/profile, настоящий lease/capability и transactional StepVector; это пример структуры, не готовый mixed-order harness | [CodeDocumentationSimulationDpoExperiment.cs, 120–141](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L120-L141), [160–190](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L160-L190) |
| S27 | Project recipient фиксирует reference hash, независимость storage, allowlist и equality BASE; он не требует equality LoRA policy/reference после каждого шага | [DpoTrainer.cs, 198–208](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/11.DPO/DpoTrainer.cs#L198-L208), [245–261](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/11.DPO/DpoTrainer.cs#L245-L261), [450–455](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/11.DPO/DpoTrainer.cs#L450-L455) |

Факт существования исходного пути отделён от факта его запуска: для ND1 кодовая цепь связана, но trace исполненного пользователем incremental-run в этой сессии не получен. Для ND2 путь совпадает с прямым RP01 StepVector; стоимость padded tail и полезность её удаления пока не измерены.

## 3. ND-DPO-SFT: после предпочтений алгоритм учится дописывать исходные файлы

### Что происходит

В штатной incremental цепи источник SFT — файл C#, а не выбранный вариант DPO-ответа. Например, для Tiny.cs ProjectIndexer делает вход «// file: Tiny.cs», выход — содержимое Tiny.cs. Отдельные feedback DPO пары могут учить объяснению nullable-ветки этого кода. Pipeline передаёт обе группы в один вызов. После DPO следует SFT для каждого файла, затем сохраняется финальная LoRA. S01–S11 показывают каждый участок.

Поэтому сообщение «DPO update завершился» не определяет последнее воздействие на опубликованный adapter. После него адаптер мог получить много градиентов другой задачи с другим optimizer. В defaults DPO имеет LR 1e−5 и decay 0.01 ([DpoTrainer, 363–376](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/11.DPO/DpoTrainer.cs#L363-L376)); стандартный TrainingConfig имеет MaxLr 3e−4 и decay 0.1 ([TrainingConfig, 13–22](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/04.TrainingLoop/TrainingConfig.cs#L13-L22)). Фактическая factory может переопределить config, поэтому «SFT всегда ровно в 30 раз сильнее» не утверждается; это не следует ни из LR ratio, ни из AdamW.

**Гипотеза:** порядок DPO → full-file SFT может менять сохранение документационного навыка относительно SFT → DPO, даже если количество уроков, обе задачи, их оптимизаторы и старт одинаковы. Это не ошибка формулы LoRA. Возможны польза, вред или отсутствие заметного эффекта.

**Релевантный домен:** C# code/documentation, прежде всего CODE_Q через прочитанный ProjectIndexer. Для SQL, Angular и EN→RU такой же combined caller не доказан; перенос ND1 туда только после собственного trace. RP01 simulation не имеет этого последующего SFT (S12).

**Ограничение найдено независимым review:** простая перестановка стадий внутри нынешнего StableDpoTrainingExecutor не реализует чистый order-only опыт. Его LogicalDpoTrainingUnit на входе копирует текущую policy в reference, либо загружает parent в обе модели, и требует FullStatesMatch (S24). После предварительного SFT такой вызов изменит R0, перезагрузит policy либо остановится. Поставить флаг для обхода этой проверки недопустимо. Кроме того, найденный mixed CODE_Q вызов использует default legacy material; stable exact-v2 требует domain code-documentation (S25). Поэтому ниже предлагается **отдельный, ещё отсутствующий research harness с заранее выбранным exact-v2 profile**, а его A не называется полным baseline existing CODE_Q owner. В нём воспроизводятся две учебные задачи и порядок, а вывод ограничивается собственным research profile.

### Маленькая карточка E-DPO-SFT-ORDER

**Статус:** NOT_RUN / RESEARCH_HARNESS_NOT_IMPLEMENTED; новая проверка порядка реальных учебных stages, не drop-in перестановка production caller.  
**Единственный фактор:** порядок двух одинаковых наборов learning updates.  
**Уроки:** два коротких допустимых C# исходника и четыре заранее оценённые DPO пары по одному закону, например «null и пустая строка идут разными ветками, хотя обе дают 0». DPO chosen/rejected создаёт внешняя открытая китайская модель; CODEX с Astra 6 ultracode выбирает случаи и независимо оценивает их качество. Файлы SFT и DPO пары — разные виды материала, это явно сохраняется в отчёте.

**Общий старт:** идентичные копии warm B0, одинаковые q/v LoRA, tokenizer, dropout=0, отдельный frozen R0, снятый ДО любых стадий. Exact-v2 material/framing/objective/profile одинаковы в обоих плечах; они явно фиксируются как профиль этого исследования. Одинаковые два optimizer-состояния: одно DPO, другое SFT; те же hyperparameters и LR sequences внутри соответствующей стадии. R0 запрещено заново снимать после SFT только в одном плече: это превратит опыт порядка в опыт смены reference. Research DPO trainer, recipient/profile и state snapshots подготавливаются при общем B0 до первого SFT; нельзя после SFT заново входить в production reference-capture owner, назвав это тем же плечом.

- **A:** один настоящий DPO StepVector на четырёх парах → два supervised updates по двум исходникам.
- **B:** те же два supervised updates в том же порядке → тот же один DPO StepVector на четырёх парах.
- Три optimizer calls и наборы material одинаковы. Actual tokens, время и allocations считаются, а не предполагаются равными.
- Показать baseline, состояние после первой стадии и итог после второй в каждом плече. Окончательные выводы относятся к итогам после трёх updates; промежуточные точки объясняют, на каком этапе возник эффект.

**Панель:** восемь новых C# вопросов по тому же смысловому закону, но с source/algorithm family split относительно **обоих** наборов — SFT-файлов и DPO source; одного переименования/числа недостаточно. Плюс четыре ранее правильных контрольных случая из прежнего обучения, исключённых из текущих новых уроков. Проверяются объяснение ветви, выдуманное исключение, полнота ответа и code-completion task отдельно. До и после используется один и тот же consumer и одна независимая оценка. R0 hash сверяется до/после каждой стадии; SFT не переустанавливает LoRA, оба optimizers привязаны к тем же разрешённым policy parameters, штатное применение и очищение gradients сохраняется.

**Метрики:** response-region NLL; число объяснённых ветвей и выдуманных исключений; полные допустимые ответы/все вопросы; absolute chosen/rejected likelihood и preference margin; code completion validity; время и пиковая память всего учебного цикла; DPO и SFT update norms отдельно; старые gained/lost ответы.

**Успех:** любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост точности для этого не обязателен. Рядом показываются изменения остальных показателей, включая ухудшения. Прирост margin не переименовывается в доказанную способность объяснять код. Если B ускорился, но хуже объясняет nullable, это записывается как успех по скорости и ухудшение по объяснениям.

**Контроли и предел:** итог DPO-only может быть сохранён как промежуточное A-состояние, но не выдаваться за равноценный по compute финал B. A/B изолирует порядок только внутри объявленного research harness; эквивалентность его A всему existing IncrementalTrainer/StableDpoTrainingExecutor не заявляется. Для вывода о mixed CODE_Q нужен отдельный proof совпадения material/objective/profile и recipient с реальным caller; его default legacy нельзя незаметно заменить exact-v2. Для другого домена сначала нужен own combined caller. Не менять DPO authoring, число уроков, LoRA targets или optimizer handoff в том же A/B.

**Минимальное будущее изменение:** один явно новый исследовательский harness, который компонует настоящий DpoTrainer.CreateForProjectLora/StepVector и существующий supervised executor, создавая оба stage owners и общий R0 до первого update. Существующий direct composition S26 показывает, что необходимые части уже есть; S27 допускает изменение только policy LoRA при сохранённых base/reference. Это source-feasible, но не реализованный и не проверенный harness. Полные exact validation, teacher-authorship/content admission, live server-issued mutation capability, keyed recipient/profile, rollback и публикация checkpoint сохраняются; конструктор без capability не является запасным способом обучения. Programmatic simulation teacher-policy из S26 нельзя использовать для подмены требуемого внешнего автора chosen/rejected: нужны соответствующие реальные admission receipts. Production reference-capture guards не ослабляются и фиктивный continuation proof не создаётся.

**Промпт исполнителю:** «Выполни только E-DPO-SFT-ORDER для C# documentation/source-completion на объявленном exact-v2 research profile в разрешённой изоляции из двух одинаковых B0. Нужен отдельно реализованный research harness: действующие DpoTrainer.StepVector и supervised executor, общий R0 и оба owners до первого update. Простая перестановка вызовов StableDpoTrainingExecutor не подходит: он recaptures reference. Реально исполни один DPO и два SFT updates на каждое плечо, меняя только порядок; сохраняй stage-local optimizer settings/state, full exact admission, issued mutation capability, recipient checks и исходные material hashes. Не называй A эквивалентным whole CODE_Q baseline: найденный production request выбирает legacy profile. CODEX с Astra 6 ultracode выбирает случаи и оценивает DPO; обе стороны создаёт внешняя открытая китайская модель. Измерь одинаковую панель до и после, stage outputs, полезные показатели и все ухудшения. Любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост точности не обязателен. Не запускай измерения без реального Train/Update. Если harness или соответствующий admission не готовы, верни конкретную недостающую связь и NOT_RUN; production guards не ослабляй».

## 4. ND-DPO-PAD: не обучать нулевой хвост дорого даже при правильном mask

### Что происходит

Для prompt длины P, chosen длины C и rejected длины R, exact material использует общий T = P + max(C,R). Обе стороны направляются в Forward длины T. Если C < R, позиции chosen от P+C до T−1 не имеют response targets, но модель всё равно создаёт там hidden activations, проекции и attention. Две policy graph структуры живут до backward. S13–S19 подтверждают эту последовательность.

Математическая оценка, **не benchmark**: один dense attention score tensor содержит B·H·T² элементов. Пер-side длины Tc и Tr вместо общего T требуют B·H·(Tc²+Tr²) вместо 2·B·H·T² на один policy/reference комплект. Это не оценка полного времени: кроме attention остаются projection/FFN, проверки, копирование и checkpoint. Если Tc=Tr, сокращения tail нет.

### Почему prefix crop сохраняет учебный смысл при заданных условиях

1. **Не меняется материал.** Только для DpoExactPairMaterial.Version current CaptureAndValidate сначала проверяет оригинальные admitted tensors/digest/shape. Internal execution view читает первые ChosenLength или RejectedLength элементов уже проверенных IDs и соответствующей mask. Длины берутся из validated ExactMaterial, никогда из поиска ID0: нулевой ID может быть настоящим токеном внутри последовательности. LegacyVersion остаётся на исходном пути. Оригинальный carrier, fingerprint, тексты, vocab, framing и proof не меняются. Создаётся отдельный execution view, не новый DPO lesson.
2. **Не теряется первый ответ.** При prompt P первый response target имеет индекс P и предсказывается logit P−1. Crop сохраняет исходный prefix с позицией 0 и полный ответ.
3. **Не теряется последний ответ.** До crop последняя валидная цель имеет индекс L−1, logit L−2. В коротком view длины L loop i<L−1 включает L−2. Нельзя дополнительно срезать последний input без отдельного переноса targets.
4. **Causal attention закрывает будущие padding keys.** Для query i<L модель видит только key≤i; padding после L−1 недоступен. Mask использует −Infinity и backward исключает верхний треугольник (S18–S19).
5. **RoPE не меняет значения сохранённых positions.** GetFrequencies получает offset 0 и positionData[index]=index; уменьшение правой границы не перенумеровывает valid tokens (S20).
6. **Нет нормировки по T.** RMSNorm агрегирует hidden axis, FFN и residual работают по token positions (S21). Exact response objective суммирует только mask=1; vectorWeight, VectorSignal, Beta и objective derivative остаются теми же (S15–S17).
7. **Отброшенные выходы имеют нулевой loss gradient.** Они не получают upstream из masked targets; future token не может влиять на прошлый. Теоретически contribution этих позиций в shared parameter gradient равен нулю.
8. **Dropout должен быть нулевым.** При ненулевом dropout изменение tensor size меняет расход RNG и последующие masks; это другой эксперимент. RP01 simulation уже требует dropout=0 (S12, S22).
9. **Конечность и конец ответа сохраняются.** Обоснование предполагает конечные weights, activations и gradients на обоих путях; арифметика 0×NaN не является нулём. EOS, если он входит в валидные исходные IDs/targets, остаётся в prefix; crop не добавляет EOS и не исправляет его отсутствие. Нельзя менять относительные positions, левое обрезание, batch composition или нормировку objective одновременно.

Это аргумент по функции и цепи производных. Он **не заменяет** нативное подтверждение gradients/AdamW updates: другие reduction sizes могут слегка изменить float32 округление. Здесь такое измерение не выполнялось. Оно должно быть встроено в каждый настоящий update будущего A/B, а не состоять из отдельного фиктивного no-training benchmark.

### Маленькая карточка E-DPO-VALID-LENGTH

**Статус:** NOT_RUN; источник bottleneck подтверждён, gain неизвестен.  
**Единственный фактор:** правая граница internal execution view каждой стороны внутри существующего StepVector.  
**Плечо A:** текущие padded IDs/masks длины max(Tc,Tr).  
**Плечо B:** после той же полной admission/материальной проверки forward/backward на prefixes длины Tc и Tr соответственно. Original material+hash неизменны.

**Уроки и бюджет:** четыре пары одного домена, два последовательных настоящих updates по две пары без изменения порядка и количества. Две пары имеют разные длины выбранного/отвергнутого ответа; одна пара меняет направление асимметрии, одна имеет равные длины. Стороны отличаются смыслом, а не только искусственно добавленными пробелами. Оба плеча получают одинаковые новые законные пары в отдельно оформленном matched research сравнении; уже consumed production pairs не переименовываются для обхода applied-once.

**Домены — отдельные короткие запуски:**

| Домен | Учебное различие | До/после для обычного ответа |
|---|---|---|
| C# документация | Короткое верное объяснение nullable-ветки против более длинного с выдуманным исключением; обратная длина в другой паре | Правильные ветви, неподтверждённые утверждения, полнота, response NLL |
| EN→RU, условный отдельный exact-v2 research профиль | Сохранение «не должен» или субъект/объект; корректный перевод может быть короче либо длиннее rejected | Полярность, роли, модальность, полнота и пригодность русского ответа; нужен отдельно подтверждённый direct exact caller и его admission |
| SQL / Angular / другие | Только при доказанном same exact-v2 neural training caller | Собственная предметная панель; данный опыт не переносится на discrete Unit |

**Общий старт:** B0/policy/reference/keyed weights, optimizer m/v/step, RNG, LoRA targets/rank/alpha/dropout=0, tokenizer/framing/objective, vector weights и decoding одинаковы. Source family TEST не попадает в TRAIN. В каждом домене восемь frozen heldout cases плюс четыре retention cases.

**Доменный guard не обходится:** stable executor разрешает exact-v2 только для code-documentation (S25). Нынешний [TranslationDpoTrainerBridge, строки 180–201](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/27.TranslationDomain/TranslationDpoTrainerBridge.cs#L180-L201) создаёт другой carrier без ExactMaterial. Поэтому EN→RU не считается уже подключённым к proposed crop через stable/translation bridge. Существующая прямая simulation composition S26 показывает возможного владельца exact обучения, но для нового опыта требуется подтвердить отдельный разрешённый research caller, его materials и admission; до этого EN→RU HOST_MISSING. Не переименовывать перевод в code-documentation, чтобы пройти guard.

**Что снять внутри каждого actual update:** фактические Tc/Tr/T и targets count; valid-target logits, objective и gradients; allowed Δweights и optimizer state; число вызовов update; время StepVector целиком, forward/backward/optimizer по уже имеющемуся DpoStepVectorTiming; allocations и peak process memory. Gradients снимаются одинаковым observer **после native backward, перед ApplyOptimizer**, а Δweights и portable optimizer state — после update. Текущий [DpoTrainer, строки 920–923](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/11.DPO/DpoTrainer.cs#L920-L923) вызывает ZeroGrad до возврата StepVector, поэтому постфактум чтение gradient не выполнит этот контроль. Если hook отсутствует, его минимальная одинаковая инструментализация входит в будущую реализацию. Никакого отдельного no-update backward или замещающего forward; логируются одинаковые valid coordinates, а не разные по размеру dumps padding.

**Границы стоимости:** один свежий процесс на плечо, одинаковые environment/startup и baseline evaluation, заранее фиксированный порядок A/B. Измеряется одинаковый interval actual Train/Update, включающий B view-copy и одинаковые admission/capture/checkpoint/publication/observation. Подготовка, оценка и общая программа отражаются отдельными границами; нельзя сравнивать A с записью файлов и B без неё. Если использован общий процесс, нужны per-region allocation counters и сопоставимый memory baseline; cumulative process high-water mark после A→B нельзя назвать независимым B peak. JIT/GC, таймер и две маленькие точки ограничивают precision: при неразличимом эффекте писать INCONCLUSIVE по стоимости, без дополнительного standalone benchmark или скрытого расширения бюджета.

**Что снять до и после обучения:** настоящие ответы одного consumer на общей панели, domain metrics, response-region NLL, learned-state deltas и retained/lost cases. При необходимости проверить свежую загрузку как подтверждение модели, но она не заменяет два actual updates.

**Основной полезный результат:** меньше секунд на один реальный update, меньше выделенных байтов или ниже пик памяти при том же количестве уроков. Exact equality функции ожидается по коду, но нативная tolerance задаётся до опыта и разница не скрывается. Если сохранение objective/gradient не подтвердилось, нельзя объявлять B доказанно эквивалентным; измеренные скорость, изменение качества и регрессии всё равно сообщаются как отдельные оси.

**Успех:** любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост точности для этого не обязателен. Рядом показываются изменения остальных показателей, включая ухудшения. Ускорение настоящего обучения полезно самостоятельно; совпавшая точность не делает эксперимент неуспешным.

**Контроли:** equal-length lesson делает view одинаковым; первый/последний target проверяются в actual step, Unicode/BPE boundaries не перекодируются; legitimate multiline ответ сохраняется целиком. Нельзя crop по ответу gold, по выбранной длине generated output или по лучшему результату TEST.

**Минимальное будущее изменение:** внутренний exact-v2-only view helper в DpoTrainer, вызванный после original CaptureAndValidate и использованный согласованно в BatchLogProbs и ForwardPolicyBatchLogProbs; длины только из validated ExactMaterial. Input IDs/masks — не trainable, поэтому копия prefix допустима. Не использовать detached Slice hidden activations. Model.Forward и автоград остаются реальными. Изменять exact material schema для этого опыта не требуется. Второй небольшой элемент — одинаковое наблюдение gradient до ApplyOptimizer/ZeroGrad, если существующий hook не найден.

**Промпт исполнителю:** «Выполни только E-DPO-VALID-LENGTH внутри настоящего exact-v2 DpoTrainer.StepVector. A использует current padded execution, B — validated side prefixes по длинам ExactMaterial, не по ID0; оригинальные admitted IDs/masks/digest не меняй, legacy не затрагивай. По два actual updates на плечо из одинаковых policy/reference/AdamW/RNG и dropout=0, те же уроки и target positions. C# documentation — первый профиль; EN→RU только через отдельно подтверждённый lawful exact research caller, не через подмену domain guard или неподходящий Translation bridge. CODEX с Astra 6 ultracode выбирает случаи и оценивает DPO; chosen/rejected создаёт внешняя открытая китайская модель. Сними gradients перед ApplyOptimizer/ZeroGrad, weights/state после шага; один свежий процесс на плечо, одинаковые cost boundaries включая view-copy, admission и checkpoint. Измерь sec/update, allocations, peak memory и общую domain-панель до/после. Любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост точности не обязателен, все ухудшения показывай рядом. Не подменяй этот опыт frozen forward, serving benchmark, file-size сравнением или измерением таблицы. Не объявляй причинность плохих RP01 ответов по одному cost result».

## 5. SC-SFT-STATE: отдельный пропуск покрытия optimizer continuity

**Факт:** per-call factory заново создаёт AdamW (S09), а его состояние непосредственно определяет второй и последующие updates (S23). Получение новой session вызывается внутри TrainCoreAsync ([IncrementalTrainer, 286–306](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/14.ContinuousLearning/Training/IncrementalTrainer.cs#L286-L306)). Parent adapter может быть полностью восстановлен (S05), но восстановление weights не тождественно продолжению SFT optimizer.

**Не новый generic тезис:** optimizer reset уже известен. P12.5 меняет Trainer при том же model/optimizer; P25 проверяет cold loaded state без нового урока; поздние N-NEURAL-02 / DLT-K1 относятся к DPO C#→ENRU moments. Они не измеряют конкретный SFT per-call reset при постоянном LR и том же порядке новых уроков.

### Маленькая карточка E-SFT-MOMENT-CONTINUITY

**Статус:** NOT_RUN / MISSING_PRECISE_SUBCASE.  
**Домен:** C# source completion и документационная retention на том же LoRA Student.  
**Единственный фактор:** перенос SFT AdamW m/v/step между двумя учебными вызовами.

A соответствует текущей factory: после первого урока сохранить model, для второго создать fresh AdamW. B начинает второй урок с тех же весов и перенесёнными m/v/step первого урока. В обоих плечах LR постоянен, MaxLr=MinLr, WarmupSteps=0, параметры clipping/decay и dropout=0 одинаковы. Так исключается уже известный P12.5 scheduler confound. Оба проходят два настоящих supervised updates на двух новых исходниках; никакого DPO между ними.

Снять до обучения, после первого и после второго урока: response-region CE, code completion validity и документационные ошибки на общей восьмивопросной панели, retained/lost случаи, optimizer moments/step и Δweights, sec/update и memory. Второй урок должен затрагивать уже активные parameters; если все gradients zero, объяснить отсутствие различающего условия.

**Успех:** любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост точности не обязателен, изменения остальных показателей, включая ухудшения, показываются рядом. Различие Δweights доказывает влияние state boundary, а не автоматически полезность продолжения.

**Промпт исполнителю:** «Выполни E-SFT-MOMENT-CONTINUITY как два настоящих supervised updates на каждое плечо. Сохрани одинаковые weights после первого урока, одинаковые новые lessons и fixed LR; только A reset AdamW m/v/step перед вторым уроком, B переносит их через существующий keyed portable API. Не меняй одновременно scheduler или DPO reference. CODEX с Astra 6 ultracode выбирает учебные случаи и оценивает качество; если понадобятся DPO-пары, их обе стороны создаёт внешняя открытая китайская модель, но добавление DPO в этот опыт запрещено его однофакторным scope. Любое измеренное улучшение полезного показателя — успех по показателю и домену, accuracy рост не обязателен; все ухудшения покажи. Только actual Train/Update и его outcomes, без standalone restore-only опыта».

### Prompt CE — известная деталь, неполная карточка

Полный SFT prompt+response objective уже явно описан в possibleDifs/12-QUICK-CAUSAL-MICROEXPERIMENTS-2026-10-16.md и MINI01. Это KNOWN, не новая находка. P05.2 сравнивает masks DPO mean; он не равен SFT masking. Если родитель сохраняет эту деталь в общий реестр, ей требуется отдельный SFT full-sequence-versus-response-only future comparison с согласованными CE/backward target masks и одинаковым содержанием. Не прикрывать отсутствие такого эксперимента ссылкой на P05.2. В ND1 намеренно используется существующий full-sequence SFT, чтобы не менять два фактора.

## 6. Сверка с защищёнными карточками

**Предметное покрытие сверено на c3f1 и ранее. Последующая внешняя редакция f50 исключила самостоятельные zero-learning проверки из учебной очереди; их природа от этого не изменилась. Текущий допуск и снятые претензии — в [аудите покрытия](03-EXPERIMENT-COVERAGE-AND-VALIDITY.md).**

Пути — относительно docs/plans/replanning-possible-diffs/experimets/cards. Факт наличия карточки не равен её выполнению или корректному охвату каждого условия.

| Тема | Нынешняя карточка | Статус в этом аудите |
|---|---|---|
| LoRA recipient/rank/alpha/dropout | P01-trainable-recipient.md | KNOWN, не переизобретать |
| Cold base и SFT→DPO warmup | P02-warm-base.md, .1–.3 | KNOWN; порядок DPO→mandatory indexed-SFT прямо не сравнивается |
| DPO sum/mean/prompt-mask | P05-response-length-objective.md, .1–.2 | KNOWN; не покрывает стоимость ignored padding и не является SFT mask experiment |
| TeachingVector density/AdamW scale | P06-teaching-vector-adamw.md, .1–.3 | KNOWN |
| EOS training target | P08-answer-boundary.md, .1 | KNOWN; active-training вариант |
| Char limit / sampler / EOS-serving / string slice | P08, .2–.5 | STANDALONE_NONLEARNING; только связь с последующим learning selection/update могла бы сделать отдельный новый опыт валидным |
| Full forward / cache prefill / append | P09-cached-forward.md, .1–.3 | STANDALONE_NONLEARNING; в строке 7 прямо No training. ND2 относится к обучению, не к этой карточке |
| Autograd / composed gradient | P10-composed-gradient.md; P11-autograd-reachability.md | KNOWN; корректность градиента применяется как контроль actual ND2 updates, а не отдельная нулевая тренировка |
| SFT LR/reset/partial state/clip | P12-optimizer-boundaries.md, .1–.6 | PARTIAL для SC-SFT-STATE: .5 сохраняет optimizer, меняет Trainer |
| Cold restore | P25-cold-restore.md, .1–.5 | PARTIAL: .2 имеет 0 lessons/updates; не проверяет следующий SFT update |
| Frozen reference | P105-frozen-reference.md | KNOWN; ND1 фиксирует R0 специально, чтобы не подмешать reference change |
| Numeric tensor cost | P110-numeric-matrix-cost.md | RELATED, другой фактор numeric kernel; не side execution length |
| LR/beta/batch policy | P119-learning-dynamics.md | RELATED, фиксируется в ND1/ND2; не меняется |
| Short pretraining shards/padding targets | P68-pretrain-pipeline.md | RELATED, другой owner и задача: обучить короткий shard, а не убрать нулевой tail exact DPO |

Защищённые parent документы 56-autograd-reachability-and-sft-caller-controls.md и 59-nonfinite-admission-clipping-and-sft-state.md уже отмечают новый Trainer/AdamW, но в контексте LR/error-state isolation. Поэтому fresh-AdamW факт не объявлен найденным впервые; новое здесь — точное недостающее обучающее A/B и его связь с next-update trajectory.

## 7. Границы достоверности и handoff

При исследовании прочитаны и сохранены указанные source файлы; не выполнялись build, training, toy metric simulation, external teacher/CODEX model API calls или изменение production. Публикация данного пакета planning является отдельной документарной операцией, не запуском обучения. Точные требования пользователя к ролям записаны в карточках и промптах; фактический вызов Astra 6 ultracode в этой документарной работе не заявляется.

В scope применены generated-code-review и learning-path-audit: кодовая причинность отделена от measured quality; документ не делает ProductReady/PASS-training claims. Имеющиеся нативные результаты чужих программ не присваиваются новым ND1/ND2.

**Новизна bounded:** выполнен поиск по полному выгруженному корпусу — 525 planning Markdown, 125 protected cards и дополнительные protected reports — с ручным чтением смыслово близких мест; отдельно проверены 11 файлов позднего planning delta. Это не заявление о построчном глубоком аудите всех 525 документов. Конкретные near-match источники и окончательный статус приведены ниже. Отсутствие одного имени метода не использовалось как единственное доказательство новизны.

**Статус после проверки:** source review ND1 и ND2 выполнен, замечания о reference/profile и границах instrumentation учтены в карточках. Они публикуются в разрешённом planning; защищённый каталог не изменяется. Следующий предметный шаг — реализация выбранного малого native опыта, без заявления о его выполнении данным аудитом.

## 8. Итоговая сверка новизны, включая поздние публикации

**Corpus pins:** planning 993ac5cf5803d9903a58b71eca7c85b94d3116f4; полный possibleDifs tree 1db4f427ede897c288ea9d134dcf4b702a1e6b78, truncated=false; 525 Markdown. Поздняя граница — planning 5333d9efa76982330e8be0b59facf304105e300b, дополнительно получены и рассмотрены 11 изменённых Markdown. Защищённый core набор содержит 125 карточек; поздний core c3f1d0d4fc7fb7949c51d2251e89d2fb0e590d72 не меняет production source. Это локальная граница проверки, а не обещание отсутствия будущих параллельных находок.

Поиск включал английские/русские смысловые варианты DPO/SFT/supervised/order/после, IncrementalTrainer, padded/padding/crop/valid length/нулевой хвост, AdamW/moments/новый optimizer. Найденные близкие записи сравнивались по владельцу update, точному вмешательству и исходу, а не только по названию.

| Ближайший существующий материал | Что уже известно | Отличие/решение |
|---|---|---|
| [MINI01, строки 13–24](https://github.com/lubochka/xiigen-planning/blob/993ac5cf5803d9903a58b71eca7c85b94d3116f4/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/12-RP01-MINI01-COLD-START-CAUSAL-EXPERIMENT.md#L13-L24), protected P02 | SFT-only и SFT→DPO warm-up; больше общего compute у подготовленного плеча; reference для DPO берётся из SFT checkpoint | ND-DPO-SFT меняет порядок одинакового набора DPO и indexed-file SFT updates при одном исходном R0; такой contrast не найден. NEW_BOUNDED |
| Protected reports 56, 59; P12.5 | Новый Trainer/AdamW per-call; schedule reset и failure isolation; P12.5 намеренно сохраняет optimizer | Факт известен. E-SFT-MOMENT-CONTINUITY — MISSING_PRECISE_SUBCASE, не новая generic находка |
| [Planning43: N-NEURAL-02, строки 75–111](https://github.com/lubochka/xiigen-planning/blob/993ac5cf5803d9903a58b71eca7c85b94d3116f4/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/43-PREPARETOGPL-RESTORATION-AND-NEURAL-DIFFS-2026-10-18/02-NEURAL-SCORES-AND-STATE.md#L75-L111), DLT-K1 | Перенос DPO moments между C# и ENRU, отдельный m50-only reset при прежних v50/step50 | Подтверждает приоритет чужого generic state finding. SFT continuation с фиксированным LR — другой конкретный consumer, отмеченный refinement |
| [Planning02, строка 33](https://github.com/lubochka/xiigen-planning/blob/993ac5cf5803d9903a58b71eca7c85b94d3116f4/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/02-DPO-DATA-OBJECTIVE-GRADIENT.md#L33), protected P05 | Ошибки EOS/padding targets, sum/mean и mask policy | ND-DPO-PAD сохраняет все targets и objective; меняет только вычисляемый нулевой хвост внутри настоящего update |
| [Planning53, строка 110](https://github.com/lubochka/xiigen-planning/blob/993ac5cf5803d9903a58b71eca7c85b94d3116f4/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/53-EXTERNAL-DOMAINS-RECOVERY-AND-SPARSE-KNN-REFINEMENTS-2026-10-19/03-MECHANISM-COVERAGE-AND-DEDUP.md#L110) | Fixed-p padding/RNG correlation при ненулевом dropout | ND-DPO-PAD фиксирует dropout=0; проверяет sec/update и memory при сохранённой функции. Это иной фактор, NEW_BOUNDED |
| Protected P09, P68, P110 | Serving cache parity без обучения; short-shard coverage; numeric kernel cost | Ни один не задаёт side-specific execution length в exact DPO со сравнением actual updates. Связанные темы, не полное покрытие ND2 |
| [Поздний planning58 DPO/LoRA delta](https://github.com/lubochka/xiigen-planning/blob/5333d9efa76982330e8be0b59facf304105e300b/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/58-SIBLING-DPO-LORA-SOURCE-DELTAS-2026-10-19.md) | Historical sm−oh vs oh−sm, default dropout Forward flag, recipients | Уточняет P10/P01/P11/P12. ND1 stage order и ND2 valid-prefix training cost там не предложены |
| Поздние 58-HUMAN-APPROVAL-EXPERIMENT-PASSPORTS, 58-SOURCE-EXPERIMENT-CROSSWALK и containment package | Аналитические controls и исторические/Unit контрасты | Не создают недостающие ND1/ND2 карточки. Их no-update варианты не используются как эксперименты обучения в данном отчёте |

**Закрытие:** два source-grounded NEW_BOUNDED кандидата — ND-DPO-SFT и ND-DPO-PAD. Один MISSING_PRECISE_SUBCASE — SFT optimizer continuity. SFT prompt CE, общий restore, dropout, LoRA recipients, sign и scale-cancellation остаются KNOWN. Новизна постановки не означает доказанную пользу, дефект продукта или установленную причину RP01.

## 9. RP01: положительные measured margins нельзя обнулить из-за free-answer failure

Ниже — повторное чтение существующих первичных scientific-result receipts, а не новый training run и не переписанный historical result. Их provenance помечен simulation; внутри receipts при этом зафиксированы реальные завершённые optimizer updates, изменённые LoRA tensors и повторно открытое сохранённое состояние. Не следует путать такую provenance с отдельной игрушечной арифметикой без обучения.

| Измерение | C# saved-root → C50 | ENRU C50 → C100 |
|---|---:|---:|
| Реально законченные новые DPO updates | 50 | 50 |
| Уникальные представленные учебные пары | 500 | 500 |
| Число score-panel cases | 25 | 20 |
| Средний normalized preference margin до | 0.03733304977416992 | 0.012293052673339844 |
| Средний normalized preference margin после | 0.03856058120727539 | 0.012403059005737304 |
| Сохранённый paired mean delta | **+0.0012275314331054651** | **+0.00011000633239746093** |
| Case margins выросли / снизились / равны | 14 / 11 / 0 | 11 / 9 / 0 |
| Chosen preferred до → после | 16/25 → 16/25 | 12/20 → 12/20 |
| Свободные семантически правильные ответы до → после | 0/3 → 0/3 | 0/4 → 0/4 |
| Invalid free answers до → после | 3/3 → 3/3 | 4/4 → 4/4 |

Первичные источники: [C# training/preference, строки 8–34](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/.kiro/xiigen-slices/work/results/RP01-simulation-main500-execution-20261015/scientific-result.json#L8-L34), [C# free outputs, строки 36–53](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/.kiro/xiigen-slices/work/results/RP01-simulation-main500-execution-20261015/scientific-result.json#L36-L53); [ENRU training, строки 11–31](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/.kiro/xiigen-slices/work/results/RP01-enru-continuation-20261016/runtime-20261016-01/analysis-20261016-01/scientific-result.json#L11-L31), [ENRU semantic/preference, строки 33–72](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/.kiro/xiigen-slices/work/results/RP01-enru-continuation-20261016/runtime-20261016-01/analysis-20261016-01/scientific-result.json#L33-L72).

**Корректное решение по правилу пользователя:** есть измеренный успех по normalized preference-margin в двух названных панелях и доменах. Рост accuracy для такого вывода не обязателен. Одновременно нет улучшения свободной семантической correctness в этих малых панелях; это отдельный неизменившийся показатель, а 11 C# и 9 ENRU case margins ухудшились. Фраза «вообще никакого улучшения» противоречит сохранённым данным. Фраза «доказано полезное умение объяснять C# или переводить ENRU» тоже ими не подтверждается.

**Scope score panel:** C# receipt прямо называет новые экземпляры тех же parameterized templates, без установленного числа независимых semantic families. Это near-transfer evidence, не доказательство нового закона на независимых семействах. ENRU receipt называет 20 diagnostic cases и 40 source score files; после раскрытия панель нельзя вновь объявить blind confirmatory, число независимых семей определяется не количеством строк. Эти оценки не равны TRAIN loss: scorer normalized margins нельзя переименовывать в exact response-sum DPO loss или утверждать, что именно он уменьшился на указанную величину. Источники: C# строки 19–34; [ENRU limitations, строки 144–149](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/.kiro/xiigen-slices/work/results/RP01-enru-continuation-20261016/runtime-20261016-01/analysis-20261016-01/scientific-result.json#L144-L149).

**Изменения других полезных показателей уже изучались отдельно:** существующий [planning43 разбор score компонентов, строки 25–38](https://github.com/lubochka/xiigen-planning/blob/993ac5cf5803d9903a58b71eca7c85b94d3116f4/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/43-PREPARETOGPL-RESTORATION-AND-NEURAL-DIFFS-2026-10-18/02-NEURAL-SCORES-AND-STATE.md#L25-L38) сообщает для ENRU mean chosen logP −7.210505723953247 → −7.211509537696839, delta −0.0010038137435913086; mean rejected logP delta −0.0011138200759887695. Поэтому margin вырос в среднем за счёт более сильного уменьшения rejected; chosen ухудшился на 20/20. Это чужой уже опубликованный offline разбор, здесь исходные 40 score files заново не пересчитаны и новизна ему не присваивается. Его следует показывать рядом с успехом margin, а не отменять одним показателем другой.

**Raw-output предел:** C# receipt фиксирует byte-identical до/после ответы на всех трёх случаях и шесть проверенных original-to-blind bindings. ENRU receipt фиксирует полные сохранённые raw outputs и восемь generate calls для четырёх случаев на плечо; отдельные строки всех сгенерированных ответов в этом секторе аудита не перечитывались, используется первичная агрегированная аттестация. C# retention после ENRU остаётся 0/3 → 0/3 invalid и byte-identical; это не доказывает сохранение изначально полезного навыка. [ENRU retention/output limits/calls, строки 74–94 и 119–142](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/.kiro/xiigen-slices/work/results/RP01-enru-continuation-20261016/runtime-20261016-01/analysis-20261016-01/scientific-result.json#L74-L142).

**Cost не даёт A/B speedup между этими доменами:** C# main-owner 6272.808539 s и training-step total 6238.0783751 s; ENRU main-native-owner 2330.4531116 s. Это различные workloads. C# save subtimings уже включены в total, optimizer-only не измерен; ENRU peak 2430550016 bytes относится к owned native processes, не whole-PC. ExternalCalls=0 относится только к нативным фазам, полный orchestration/teacher/evaluator cost не totalized. Новая ND2 должна измерить две одинаковые actual learning workloads, а не делить эти исторические числа. [C# cost, строки 55–67](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/.kiro/xiigen-slices/work/results/RP01-simulation-main500-execution-20261015/scientific-result.json#L55-L67), [ENRU cost, строки 96–118](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/.kiro/xiigen-slices/work/results/RP01-enru-continuation-20261016/runtime-20261016-01/analysis-20261016-01/scientific-result.json#L96-L118).

