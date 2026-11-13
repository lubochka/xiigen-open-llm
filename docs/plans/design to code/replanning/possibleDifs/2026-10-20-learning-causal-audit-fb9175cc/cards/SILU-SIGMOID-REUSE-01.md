# SILU-SIGMOID-REUSE-01 — повторное использование знаменателя SiLU в настоящем обучении

## Статус и место в очереди

**AUTHOR_SETUP_REQUIRED · новая гипотеза · программирование · измерений A/B ещё нет.** Этот документ добавляет кандидата в исправления. Он не разрешает менять production, запускать обучение, повторять прошлые эксперименты или исправлять их тесты. Текущая работа — документация. Защищённую папку core/docs/plans/replanning-possible-diffs не менять; старые карточки, результаты и каталоги не удалять.

В реестре эта строка должна иметь priority_group=P2_NATIVE_TRAIN_COST, run_ready=false. Сначала идут действующие блокеры обучения и конфликты карточек. Текущий NEXT10=PARENT-ADOPTION-01 сохраняется: новая кандидатура его не подменяет. Эта карточка непосредственно связана с native training и поэтому располагается перед проверками, вообще не участвующими в обучении.

Модель исполнителя — default/inherited текущей сессии, без смены модели. Роли выбора примеров и оценки качества DPO принадлежат CODEX (Astra 6 ultracode), генерация DPO — внешней открытой китайской модели. Для данного численного SFT-эксперимента новые DPO не нужны: teacher calls=0, quality-model calls=0, generated DPO pairs=0. Отчёт и объяснения — по-русски; идентификаторы, API и имена файлов сохраняются.

**Обязательный запрет поведения:** при STOP/«закончить немедленно» немедленно прекратить действия. При NEXT/«следующий эксперимент» немедленно оставить прежнюю задачу и перейти только к указанному следующему эксперименту в разрешённой фазе. Нельзя продолжать прежнюю задачу ради дополнительного отчёта, проверки, хеша, коммита, исправления тестов, warm-up, повторного запуска или «завершения полного цикла». Доступный статус сообщить без новых действий над прежней задачей. Нельзя выдавать неначавшееся обучение за результат; build failure и оставшиеся файлы описываются как таковые.

## 1. Что предлагается проверить

Гипотеза: сохранение уже вычисленного знаменателя SiLU между forward и backward может уменьшить CPU-время всего native SFT update при сохранении тех же численных обновлений LoRA. Цена — дополнительный массив активаций и его время жизни в графе. Ускорение, снижение затрат или улучшение точности пока не измерены.

В текущем [TensorOps.Silu, строки 1129–1162][silu] выполняются две одинаковые экспоненты для каждого элемента, который участвует в backward:

    forward:  data[i] = x.Data[i] / (1f + MathF.Exp(-x.Data[i]))
    backward: sigmoid = 1f / (1f + MathF.Exp(-x.Data[i]))

Вариант B сохраняет именно знаменатель d = 1f + MathF.Exp(-x.Data[i]) во время forward и читает его в backward. Это конкретная гипотеза о затратах обучения, а не доказательство, что историческое снижение качества вызвано SiLU.

### Участие в обучении и место возможного будущего исправления

| Звено | Проверенный путь |
|---|---|
| Настоящий update | [Trainer.TrainSupervisedNextTokenStep][trainer]: model.Forward(training:true) → CE → BackwardLoss → ClipGradNorm → AdamW.Step → ClearGradients |
| Вход в Transformer | [LlmModel.Forward, строки 219–240][model]: embedding → все blocks → final norm → LM head |
| Путь к FFN | [TransformerBlock.Forward, строки 45–65][block]: attention с LoRA → residual → norm2 → FFN |
| Нужный вычислительный узел | [SwiGLUFeedForward.Forward, строки 40–51][ffn]: gate projection → TensorOps.Silu → multiply с up projection → down projection |
| Обучаемые параметры | [LoraLinear.Forward, строки 113–131][lora]: q/v LoRA участвует в forward; градиент через замороженный FFN нужен для обучения этих адаптеров |
| Файл возможного production-кандидата после решения пользователя | 00.TensorCore/Core/TensorOps.cs, метод Silu(Tensor, CancellationToken), только сохранение знаменателя для backward |

Основное плечо использует native LlmModel и четыре обучаемых массива q/v LoRA. Это не обучение полного RP01 Unit/Flow и не проверка интеграции Unit lifecycle. Общая достижимость SiLU через DPO forward не заменяет отдельного DPO-эксперимента.

### Новизна

Выполнен полный адресный поиск по 929 Markdown в possibleDifs на planning SHA fc0dd1b04d61a963906cdc1ab29dffd2ee9e520c и по 372 Markdown защищённого core-корпуса на core SHA fb9175cc752d41e1cb525e0609b7c96140940217. Проверялись Silu, SwiGLU, сочетания sigmoid с cache/reuse, русские сочетания повторных экспонент и кеширования активаций. Точные соседние материалы прочитаны. Сплошное содержательное чтение каждого документа не заявляется.

| Существующий материал | Почему это другой фактор |
|---|---|
| SFT-EXP-REUSE-01 | Повторные экспоненты softmax в Trainer.BackwardLoss; здесь SiLU между forward и backward |
| NEURAL-FROZEN-TRANSPOSE-01 | Повторные транспозиции замороженных Linear; здесь дополнительный массив знаменателя активации |
| RMS-COORD-ALLOC-01 | Координаты broadcast RMSNorm, другой узел и другой расход |
| Материалы об исправленном SiLU autograd | Подтверждают наличие backward; не предлагают повторно использовать знаменатель |

Предшествующая карточка именно этого фактора не найдена. Счётчик совпадений не является доказательством универсальной новизны вне проверенного корпуса.

## 2. Домены и минимальные данные

Основной домен — **программирование**, очень узкая задача: выдать один знак результата простого C#-совместимого арифметического выражения. Доказательство не переносится автоматически на длинную генерацию кода, перевод, документацию или orchestration. Для других доменов понадобится отдельное ненулевое обученное S0 и тот же native caller; SiLU, LoRA и KNN сами по себе доменами не называются.

Набор фиксирован и совпадает с прочитанным [LEARN-SANITY-PROBE/Program.cs][sanity-source].

| ID | Роль | Prompt — точные символы, без пробела после = | Единственный токен ответа |
|---|---|---|---|
| T+ | TRAIN | C# result sign: 9-4 = | + |
| T− | TRAIN | C# result sign: 4-9 = | - |
| E+ | EXAM | C# result sign: 8-3 = | + |
| E− | EXAM | C# result sign: 3-8 = | - |

Кодирование — UTF-8 bytes, int ID каждого байта, словарь 256, без BPE merges, без BOS/EOS и без дополнительного разделителя. Каждый prompt имеет 21 байт, prompt+answer — 22; один SFT update обучается на всех 21 shifted next-token targets, включая один токен ответа. IDs ответов: +=43, -=45. Нельзя сообщать, что loss маскирован только на ответ: исходный native Trainer учится на всей последовательности.

EXAM не входит в обучение, checkpoint selection, допуск A/B, LR tuning или подбор seed. Положительный EXAM readout — лишь две близкие строки. Статистическое обобщение на программирование не доказано.

## 3. Откуда взять обученное S0

**Готового пригодного H64 checkpoint с весами и optimizer state в проверенном LEARN-SANITY-PROBE не найдено.** В его дереве есть исходник, проект, журналы, JSON и отчёт. Program сохраняет JSON readouts; он не сохраняет checkpoint. Его положительный результат даёт рецепт подготовки, а не готовые восстановимые веса.

Проверен существующий [raw probe-result.json][sanity-raw]:

| Измерение в исторической пробе | W0, 0 updates | W400, 400 updates |
|---|---:|---:|
| TRAIN, правильных однотокенных ответов | 0/2 | 1/2 |
| TRAIN, средний NLL правильного токена | 7.960885496452843 | 1.8879445998230966 |
| TRAIN, средний gold rank | 206.5 | 1.5 |
| EXAM, правильных однотокенных ответов | 0/2 | 1/2 |
| EXAM, средний NLL правильного токена | 8.01533412366764 | 2.7988595276200154 |
| EXAM, средний gold rank | 215 | 2.5 |

Это положительные измерения полезных показателей именно обучения; они не требуют роста до 2/2. Результат 1/2 на каждом сбалансированном наборе равен constant-label baseline, поэтому не доказывает усвоения условного правила знака. При этом модель не освоила обе строки: после 400 updates TRAIN отвечает + на оба примера, EXAM отвечает - на оба. До 100 updates включительно полных правильных ответов на обоих наборах не было. Поэтому бюджет подготовки нельзя произвольно ужать до четырёх updates.

Запись RESTORE-MINIMAL-SET использует другую H16 модель и имеет 0/2 полных TRAIN ответов после восстановления; её нельзя подставлять как готовый донор этого H64 опыта. Наличие LFS pointer не доказывает наличие скачанного payload.

### Фиксированная авторская подготовка

Автор нового host обязан один раз подготовить S0 **внутри нового SILU01**, воспроизводя следующий bounded recipe. Старый LEARN-SANITY-PROBE не запускать заново и не редактировать.

| Настройка | Значение |
|---|---|
| LlmModel | VocabSize=256; ContextLen=64; HiddenDim=64; NumLayers=1; NumHeads=4; NumKvHeads=2; FfnHiddenDim=176 |
| Численные параметры | RopeTheta=10000f; RmsNormEps=1e-5f |
| База | Initializers.UseDeterministicSeed(62001UL) |
| LoRA | TargetModules=["q_proj","v_proj"]; Rank=4; Alpha=8f; Dropout=0f |
| LoRA seed и stream | ApplySeeded(...,62002UL,"frozen-transpose-probe-v1") |
| Trainable/frozen | Ровно 4 trainable parameter arrays и 12 frozen arrays, включая frozen lm_head |
| Trainer | BatchSize=1; SeqLen=64; WarmupSteps=0; TotalSteps=400; MaxLr=MinLr=0.001f; GradClipNorm=1f; GradAccumSteps=1; WeightDecay=0 |
| AdamW | beta1=0.9f; beta2=0.95f; eps=1e-8f; weightDecay=0 |
| Порядок | T+, T−, T+, T−, …, ровно 400 calls TrainSupervisedNextTokenStep, weight=1f |
| Прочие факторы | Linear.ExperimentReuseFrozenTranspose=false; SiLU reuse=false; нет иных оптимизаций из соседних карточек |

W0 и W400 измеряются на четырёх строках. S0 принимается для данного A/B только если W400 имеет минимум один правильный TRAIN ответ, изменившиеся trainable веса, конечные losses и optimizer step=400. EXAM полностью показывается при любом результате и не участвует в допуске S0/A/B; один фиксированный W400 не выбирается по EXAM. Это проверка пригодности уже фиксированного рецепта по TRAIN и состоянию: рассматривается только W400, EXAM остаётся неотфильтрованным наблюдением. Если критерий не выполнен, статус BASELINE_NOT_LEARNED_AT_FIXED_BOUND; сохранить результат и прекратить. Не увеличивать updates, learning rate, размер модели или число seeds. Не запускать A/B с 0/0 под видом обучения.

Сохранить веса S0 штатным LlmModel.SaveCheckpoint, portable optimizer штатными ExportPortableState(...).ToBytes(). Для bindings использовать GetCheckpointParameterEntries().Select(e => new AdamWParameterBinding(e.Key,e.Parameter)); не сопоставлять массивы по случайному порядку. API импорта/экспорта определены в [AdamW][adamw] и уже используются в [RESTORE-MINIMAL-SET/Program.cs][restore-source].

Для каждого плеча создать новый LlmModel той же конфигурации; до LoadCheckpoint применить LoraApplicator.ApplySeeded с теми же q/v targets, rank4/alpha8/dropout0, seed62002UL и stream "frozen-transpose-probe-v1". Затем выполнить LlmModel.LoadCheckpoint из S0 и проверить состав:4 trainable массива,12 frozen, те же named parameter keys/hashes. LoadCheckpoint читает существующую topology и сам не добавляет LoRA. После этого создать новый AdamW, построить bindings по GetCheckpointParameterEntries и выполнить TryImportPortableState из одних сохранённых optimizer bytes. Оба плеча получают те же веса и моменты; initial seed не заменяет сравнение загруженного S0. TryImportPortableState должен вернуть true; точные hashes всех named parameters и portable optimizer bytes до A/B должны совпасть с S0. Для каждого плеча ожидается optimizer step 400→404. Новый Trainer допустим при явно постоянном MaxLr=MinLr: его локальный scheduler counter не создаёт различие LR. Dropout=0; дополнительные случайные преобразования запрещены.

## 4. Полная спецификация нового host и замыкания исходников

**SILU01, Runner.csproj, Program.cs и Run.ps1 сейчас не существуют как проверенный runnable experiment.** Ниже — точное задание автору host. Runtime-исполнитель не должен искать альтернативный проект, угадывать флаги или запускать вымышленный CLI. Пока автор не добавил файлы, manifest, точную команду и статическую проверку dependency closure, статус остаётся AUTHOR_SETUP_REQUIRED.

Место автора заранее определено: C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SILU01. В нём должны находиться src/, Program.cs, Runner.csproj, Run.ps1 и короткие каталоги obj/, bin/, s0/, results/. Не воспроизводить вложение experiments-v2/RP01-MICRO дважды. Перед созданием вычислить длины всех путей; предел для каждого планируемого пути — 220 символов. Если корень отличается, требуется авторское обновление карточки с точным абсолютным корнем до runtime; исполнитель не ищет его сам.

Проверенное исходное замыкание — 36 .cs из:

    experiments-v2/RP01-MICRO/LFT01/experiments-v2/RP01-MICRO/NEURAL-FROZEN-TRANSPOSE-01/src

в core commit fb9175cc752d41e1cb525e0609b7c96140940217. Git tree SHA src: e448640bd1f2fe2c65adcc0b5ffcb02e7f5539e6. [Существующий Runner.csproj][lft-project] показывает компиляцию этих исходников с EnableDefaultCompileItems=false и target net9.0. Автор копирует **весь** src и сохраняет относительные пути, а не один TensorOps.cs. Новый Runner.csproj компилирует только src/**/*.cs и новый Program.cs; OutputType=Exe, AssemblyName=SILU01, TargetFramework=net9.0, EnableDefaultCompileItems=false; без ProjectReference на runtime, тесты, solution или dispatcher; ImportDirectoryBuildProps=false, ImportDirectoryBuildTargets=false, локальные bin/obj.

TensorOps.cs этого замыкания идентичен текущему core target по Git blob e1a20dca84d76ee85d0c559bd496c1fa3c96ac8b. Проверенные Tensor/Transformer/Trainer/AdamW/LoRA файлы совпадают с текущими tree entries, кроме Linear.cs: в копии есть экспериментальный frozen-transpose switch, который здесь всегда false. Это ограничение происхождения фиксируется в manifest, а не скрывается как полное равенство всего репозитория.

### Manifest обязательных исходников

Все следующие записи находятся относительно src/ указанного pinned subtree. Git blob SHA проверяется до авторского изменения единственного численного фактора; отдельный B diff сохраняется рядом.

| Source относительно src/ | Git blob SHA |
|---|---|
| 00.Core/OperationResult.cs | 2bfde76ca7d99f63eb657e736330da177838c644 |
| 00.Core/SnapshotExceptionGraphClassifier.cs | af630e9b64fa2c438f4321d3ba9216841d76d152 |
| 00.TensorCore/Core/SimdOps.cs | cf2a93a4486e141a595b658aa8346133721c842f |
| 00.TensorCore/Core/Tensor.cs | a2569eefe378fb0a841cbb2137c6a8ac001bdb3a |
| 00.TensorCore/Core/TensorOps.cs | e1a20dca84d76ee85d0c559bd496c1fa3c96ac8b |
| 00.TensorCore/Core/TensorShape.cs | 0879e0702f210e2dd45358af09bddff2a4ef9874 |
| 00.TensorCore/Core/TensorStorage.cs | 22aec3ba03b756530001ae786acbdce28af90104 |
| 00.TensorCore/Init/Initializers.cs | 30ff381b091c3ee7aaf56e9cdbab98be2652db1e |
| 00.TensorCore/Layers/Dropout.cs | 02cf86dc84854b163cb536cab5be40dd1b7f9d63 |
| 00.TensorCore/Layers/Embedding.cs | 26fc1a0b46fcbc6df305421bfc48c15cc70d11a4 |
| 00.TensorCore/Layers/Linear.cs | d8120e0bdcfc2673aa17862e5225d77420827db6 |
| 00.TensorCore/Layers/Module.cs | dc396e8fa5a23ce6d487ff30e1326cddc7df8efd |
| 00.TensorCore/Layers/Parameter.cs | 82f5ea5aebc01b70f9c3f651283a1481c1df9a26 |
| 00.TensorCore/Randomness/SplitMix64Random.cs | 11c6d483ed94829ece961af2d77f2cff91ce411a |
| 02.DatasetPipeline/DataLoader.cs | 9fb803ef90adbfc3a4ee61eb942d6ab671047996 |
| 02.DatasetPipeline/NpyReader.cs | dda1b8922699025413eb495cd1f0fc0aa9625c18 |
| 03.TransformerModel/Caching/ITransformerKvCache.cs | 4d12f26e03f50b519bff987b455015d4cc2fefdd |
| 03.TransformerModel/Config/ModelConfig.cs | 818877df9a8dda4b40dd67bb441489b8512200b1 |
| 03.TransformerModel/FullCheckpointLoraTopology.cs | 25f24c1a260b1421ec5ded03e6dc3a0a121ff984 |
| 03.TransformerModel/Layers/CausalSelfAttention.cs | 8106a98c5f2dba90a975abe2056e21b5550166c6 |
| 03.TransformerModel/Layers/RmsNorm.cs | 246bbc4781d4c06d3b71f617d51aec8860ac39f2 |
| 03.TransformerModel/Layers/RotaryEmbedding.cs | 48597599b46f884e74bf911c66189f77eb649e37 |
| 03.TransformerModel/Layers/SwiGLUFeedForward.cs | 5daa1c56bb54cac66bd95b8658df9cc0eb2ee56e |
| 03.TransformerModel/LlmModel.cs | bef968513391f3941e40f20393a4e5af30567ff8 |
| 03.TransformerModel/Randomness/LlmModelRandomState.cs | 96c79e56e176234e3cd8e777b2ae55fc70ae9572 |
| 03.TransformerModel/TransformerBlock.cs | 28eeaba3abf22ccca4bd748bf8c81470c327520c |
| 04.TrainingLoop/Optimizers/AdamW.cs | 05877706c68c8a76b63fb62842d91a2596070faf |
| 04.TrainingLoop/Optimizers/AdamWPortableState.cs | fef6af5366e412e2508a716118bd3f76759abc73 |
| 04.TrainingLoop/Optimizers/AdamWStateSnapshot.cs | 009983981b3ff213ff68d28c955027649ba83684 |
| 04.TrainingLoop/Schedulers/CosineWithWarmup.cs | 6020cbaa994468cbd09c12d5a9cce82a993051a1 |
| 04.TrainingLoop/Trainer.cs | b63d550ba43e24a2b51be2c836519aab537d1312 |
| 04.TrainingLoop/TrainingConfig.cs | 31dd056a15888e0f0a287bf9af56297b647c8b7d |
| 10.LoRA/LoraAdapterRandomSeeds.cs | 26c7a56ba5dda49671c84e1deb11571b02e73a78 |
| 10.LoRA/LoraApplicator.cs | 5eeaf38c5fc12fa2f88dc6c247a3fb2fc250716d |
| 10.LoRA/LoraConfig.cs | a0b81255846f37b5e9449d6ccc883d924c3ed5f5 |
| 10.LoRA/LoraLinear.cs | 1b321e99408c961b896896983d002e3b26425b85 |

### Контракт Program и запуска после авторской готовности

Program выполняет ровно последовательность W0 → common preparation 400 → W400/S0 → restore A → A0 → 4 updates A → A1 → restore B → B0 → 4 updates B → B1 → report. A и B получают один S0 и один порядок T+,T−,T+,T−. Состояние A не становится началом B.

В экспериментальной копии TensorOps используется один явный switch для варианта; все общие измерители одинаковы. На подготовке, A и readout forwards switch=false; только во время четырёх B updates он true. Это область эксперимента о training cost. Продуктовая политика кеширования при inference здесь не проверяется.

Run.ps1 — заранее назначенная единственная точка запуска будущего host, без поискового dispatcher. Автор обязан вписать в карточку **проверенную команду этого созданного файла**, SDK версию, точные out/obj пути и один run ID перед переводом в RUN_READY. Не добавлять в карточку будто существующие флаги native runtime. Run.ps1 должен однократно собирать только этот проект, создавать результаты без overwrite и выполнять этот fixed plan; до ready никаких runtime команд не разрешено.

Контракт создания Run.ps1 уже назначен: без входных параметров, repo root строго C:/Projects/llm_mvp_core, единственный run ID SILU01-R1, results/SILU01-R1 и s0/SILU01-R1. Автор реализует именно этот интерфейс. После создания и готовности **будущая** команда должна иметь следующий вид; сейчас это спецификация отсутствующего host, запуск запрещён:

    pwsh -NoProfile -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SILU01/Run.ps1

Внутри будущего скрипта — одна Release build Runner.csproj с TargetFramework=net9.0, затем запуск созданного SILU01.dll без аргументов, с рабочим каталогом SILU01. Точный установленный SDK автор фиксирует в run-contract.md до RUN_READY; net9.0 обозначает target framework, а не проверенную версию SDK. Runtime executor не выбирает flags, paths, fixture, модель или runner. Если автор не смог создать именно этот контракт, карточка остаётся AUTHOR_SETUP_REQUIRED с одной конкретной причиной; альтернативного запуска нет.

Если out уже существует, исходники не совпали, compile завершился ошибкой, portable restore не прошёл или обнаружен неизвестный runtime конфликт — остановить этот опыт с конкретным статусом. Существующие файлы не удалять, run ID не менять автоматически. Допустимые экспериментальные исключения — короткий host, source copy, common instrumentation и локальный switch; они не становятся разрешением менять production или отправлять код.

Ранняя защита бюджета назначена точно: после успешной собственной build, но **до запуска SILU01.dll и любого W0 forward/400-step preparation**, Run.ps1 атомарно создаёт один файл C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SILU01/execution-SILU01-R1.claim через FileMode.CreateNew. До этого проверяет, что S и R указанного run не содержат старых результатов. Claim содержит run ID, pin, пути и PID owner; Program проверяет этот уже созданный claim и не создаёт второй. Claim и partial остаются при ошибке/STOP/timeout; следующий запуск с этим claim завершается EXISTS_OR_UNKNOWN до модели/обучения, без удаления и скрытого run02. Нельзя заменить ранний claim поздним CreateNew(report/raw) после 408 updates. Собственную сборку до claim можно подготовить без учебных вызовов; она не выдается за обучение.

При отдельном поручении только SILU-SIGMOID-REUSE-01 его NEXT=END. В явно разрешённой общей авторской очереди A4→A5 HNSW-QUERY-NORM-REUSE-01; разрешение фазы не расширяется автоматически.

## 5. Единственный фактор A/B

A сохраняет исходную арифметику SiLU.

B только в ветке x.RequiresGrad:

1. Создать float[] denominator длиной x.Shape.TotalElements, принадлежащий этому forward.
2. В forward вычислить float d = 1f + MathF.Exp(-x.Data[i]); затем data[i] = x.Data[i] / d; затем denominator[i] = d.
3. В backward вычислить sigmoid = 1f / denominator[i].
4. Оставить формулу grad, порядок умножений, Children, AccumulateGrad, CancellationToken checks и формы неизменными.
5. Когда x.RequiresGrad=false, не выделять denominator и сохранять исходный forward.

Нельзя заменить x/d на x*(1/d), восстанавливать sigmoid через result/x, кешировать между шагами или моделями, менять точность, менять входные x после forward, вводить approximate Exp, mask, новый optimizer, LR, LoRA target или tokenizer. Нельзя одновременно включать SFT-EXP-REUSE или frozen-transpose cache.

Проверка float parity проводится на фактически обученных A/B: loss каждого из четырёх updates, named weights после updates, portable optimizer bytes и все сохранённые before/after logits. Эти сравнения не требуют новых forward или optimizer calls. Если обнаружены отличия, report содержит max abs/relative error и количество разных float bit patterns; численный характер варианта становится отдельным риском, а «тот же update за меньшее время» не утверждается.

## 6. Измерения до и после

Основное измерение — **суммарное elapsed_ms четырёх полных вызовов native TrainSupervisedNextTokenStep** каждого плеча. Внутри интервала остаются forward, loss, backward, clipping, optimizer и очистка gradients. Вычислять длительность только MathF.Exp или Silu и выдавать её за выигрыш обучения запрещено.

| Группа | Что сохранять и как сравнивать |
|---|---|
| Обучение | 4 returned loss каждого плеча; model/optimizer hashes и steps; изменение LoRA, неизменность frozen weights |
| Ответы | На T+/T−/E+/E− отдельно A0,A1,B0,B1: predicted token, exact, gold token NLL, gold rank; raw 256 final-position logits |
| Скорость | total native Train elapsed_ms; process CPU delta; per-update elapsed; targets/sec при фактическом знаменателе 84 targets/плечо |
| Память | GC.GetTotalAllocatedBytes(precise:true) delta для native Train; managed heap до/после; общий peak working set с честной областью измерения |
| Цена кеша | Число реально созданных denominator arrays; их payload bytes в Train; максимум payload одного forward-графа |
| Достижимость | Число native updates, backward calls и SiLU graph elements; instrumentation читает тот же путь, отдельный helper-probe запрещён |
| Учёт затрат | Создание модели, 400 preparation updates, checkpoint/restore, scoring и report отдельно от сравниваемого native Train |

При 21 позиции, одном слое и FF176 ожидаемый payload одного denominator array равен 21×176×4=14784 байта; четыре B updates дают 59136 байт payload, если узел достигнут один раз на update. Это расчёт ожидаемого дополнительного массива, **не измеренное сокращение памяти**. Заголовки массива, GC и время жизни графа тоже имеют цену. Перестать использовать граф после backward не означает, что GC уже освободил массив.

Process.PeakWorkingSet64 накопителен: после A→B его нельзя выдавать за два независимых пика плеч. Показать общий процессный peak и честные snapshots. При фиксированном A→B и одном коротком замере timing имеет порядок и шум; не объявлять причинное ускорение на всех доменах. Дополнительные повторы для получения удобного знака результата запрещены.

Readout точно повторяет смысл существующего Observe: model.Forward(prompt,training:false), logits последней позиции prompt, argmax по 256 IDs с первым максимальным ID, NLL=logsumexp(logits)-gold_logit, rank=1+число logits строго больше gold. Однотокенный ответ полностью измеряется этой генерацией; это не teacher-forced многотокенный exact. Нельзя считать совпадение logits между A и B доказательством ненулевого обучения: отдельно нужны пригодное S0, actual updates и ответы.

**Правило успеха:** любое измеренное улучшение полезного показателя считается успехом по этому показателю и данному домену; рост точности для этого не обязателен. Рядом обязательно показать все остальные изменения, включая ухудшения. Например, native training elapsed снизился, accuracy прежняя, allocations выросли — это успех по elapsed с ценой памяти, а не общий безусловный успех. Число изменённых строк исходника и арифметически предсказанное уменьшение Exp сами по себе не доказывают выигрыш скорости или качества.

## 7. Полный бюджет и терминальные состояния

| Фаза | Native updates | Полные encoded sequence bytes | Shifted SFT targets | Дополнительные scoring forwards |
|---|---:|---:|---:|---:|
| W0 и W400, common preparation | 400 | 8800 | 8400 | 8 |
| A0 → A1 | 4 | 88 | 84 | 8 |
| B0 → B1 | 4 | 88 | 84 | 8 |
| Всего | 408 | 8976 | 8568 | 24 |

Каждый scoring forward имеет 21 prompt position. Все 408 updates и 24 readout forwards включены в бюджет; дополнительных warm-up, pilot, LR/seed grid, «почти готовой модели» другого размера и backfill-обучения нет. Подготовка S0 не бесплатна и не прячется за словом restore. 400 updates — фиксированная верхняя и целевая граница подготовки, не обучение до достижения произвольного критерия.

Автор задаёт safety wall cap 300 секунд на весь native phase с preparation и A/B; это аварийная граница, не обоснование урезать минимальный set. При достижении cap статус BUDGET_EXHAUSTED с фактически выполненными calls и metrics; автоматически не расширять budget. Compile ограничивается отдельно 120 секундами. В текущей фазе документации исполнять ни один из этих этапов нельзя.

Терминальные статусы: AUTHOR_SETUP_REQUIRED; AUTHOR_READY/RUN_READY после полного host contract; BASELINE_NOT_LEARNED_AT_FIXED_BOUND; SOURCE_MISMATCH; BUILD_FAILED; RESTORE_FAILED; BUDGET_EXHAUSTED; STOPPED_BY_USER; MEASURED_SUCCESS_BY_METRIC; MEASURED_NO_IMPROVEMENT; NUMERICAL_DIVERGENCE_REQUIRES_USER_DECISION. Ни один из статусов не разрешает автоматический production fix. После одного разрешённого запуска — один report и кандидат пользователю; не проходить полную цепочку исправлений до решения пользователя.

## 8. Файлы отчёта и точный формат

Авторский набор нового host: source-manifest.tsv, factor.patch, run-contract.md. Единые абсолютные корни: S=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SILU01/s0/SILU01-R1; R=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SILU01/results/SILU01-R1. Во время будущего разрешённого запуска: S/weights.bin, S/optimizer.bin, S/manifest.json; R/status.json, R/raw.json, R/per-row.csv, R/costs.csv и R/report.md. Здесь S и R — именно эти полностью назначенные пути, без выбора дополнительного run ID. Все новые, без overwrite старых. В пользовательский possibleDifs добавляются Markdown report и ссылки на exact raw/source SHA; защищённая core-папка остаётся read-only.

report.md по-русски содержит ровно нужные разделы: «Статус и решение», «Гипотеза и домен», «Какое S0 действительно восстановлено», «Один фактор и реальный обучающий путь», «До/после по каждому набору и метрике», «Стоимость подготовки и native updates», «Улучшения рядом с ухудшениями», «Ограничения и кандидат исправления», «Фактические counts, pins и raw artifacts». Unknown значения писать NOT_MEASURED, не 0. Отдельно фиксировать stop/next, если они поступили.

### Встроенный промпт исполнителю

> Ты работаешь с SILU-SIGMOID-REUSE-01 на default/inherited модели текущей сессии. Отвечай и документируй по-русски. Сейчас разрешена только документация; статус AUTHOR_SETUP_REQUIRED. Не запускай обучение, не меняй production, защищённую docs/plans/replanning-possible-diffs или старые эксперименты. Текущий NEXT10=PARENT-ADOPTION-01 не заменять этой кандидатурой.
>
> Сначала используй перечисленные конкретные skills: docs/skills/architect-behavior-classifier-SKILL.md, phase-planning-SKILL.md, plan-review-SKILL.md, generated-code-review-SKILL.md, experiment-design-SKILL.md, prompt-patch-authoring-SKILL.md, human-readable-architecture-audit-SKILL.md и human-communication-GLOSSARY.md. Правила пользователя об экспериментальных исключениях, документации и STOP/NEXT имеют приоритет. Не ищи другую модель: CODEX (Astra 6 ultracode) — роль выбора/качества DPO, внешний открытый китайский model — генерация DPO, но здесь все такие calls равны нулю.
>
> Единственный новый фактор: в копии TensorOps.Silu сохранить forward denominator 1f+MathF.Exp(-x) для backward, сохранив исходный порядок x/d и производную. Native learner: Trainer.TrainSupervisedNextTokenStep → LlmModel → TransformerBlock → SwiGLU → SiLU → native backward/AdamW; никакой helper-only benchmark. Core pin fb9175cc752d41e1cb525e0609b7c96140940217. Source closure — все 36 файлов таблицы из LFT01/src, дерево e448640bd1f2fe2c65adcc0b5ffcb02e7f5539e6. Место нового host строго C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SILU01.
>
> При последующем отдельном разрешении авторской реализации создай только этот host по полному контракту карточки. Нельзя объявлять Run.ps1 или какой-либо runtime CLI существующим, пока файл и exact command не добавлены. До готовности runtime ничего не ищет и не запускает. Не выбирай другой scaffold. Конфликт путей/версии/существующего output фиксируй; автоматического удаления, нового ID и retry нет.
>
> Единственный допустимый рецепт S0: H64/V256/L1/Q4/KV2/FF176, context64, q/v LoRA rank4 alpha8 dropout0, seeds62001/62002, stream frozen-transpose-probe-v1, LR0.001, AdamW0.9/0.95/eps1e-8, warmup0, clip1, weightDecay0. T+: "C# result sign: 9-4 ="→"+"; T−: "C# result sign: 4-9 ="→"-"; EXAM 8-3 и 3-8. UTF-8 byte IDs, без разделителя и EOS. Подготовить ровно 400 SFT updates в новом host, затем сохранить модель и моменты шага 400. Исторический raw даёт рецепт с 1/2 TRAIN и 1/2 EXAM, но готового H64 checkpoint не найдено. H16 restore с 0/2 не заменяет S0. Если после фиксированных 400 нет хотя бы 1 правильного TRAIN — сохранить BASELINE_NOT_LEARNED_AT_FIXED_BOUND и не выполнять A/B. EXAM измерить и показать без отбора; он не gate допуска и не выбор checkpoint.
>
> До исполнения Run.ps1 создаёт execution-SILU01-R1.claim через CreateNew до первого forward/400 steps; существующий claim/raw не удалять и не обходить новым ID. Program использует тот же claim. При отдельном поручении только этого ID NEXT=END; общий следующий A5 действует лишь в разрешённой очереди.
>
> При последующем разрешении исполнения оба плеча восстанови из одних bytes: new LlmModel→ApplySeeded q/v LoRA→LoadCheckpoint→4/12 named-state check→TryImportPortableState; optimizer400→404. A и B: ровно 4 native updates T+,T−,T+,T−, один фактор. Всего 408 updates/8568 targets/24 scoring forwards, максимум 300 секунд native phase; нет grid/warm-up/автоповтора. Главная метрика — полные native Train elapsed_ms; рядом CPU, allocations, payload дополнительных arrays, losses, logits/weights/optimizer parity, exact/NLL/rank TRAIN и EXAM до и после. Любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; роста accuracy не требуется. Все ухудшения показывай рядом. Не называй расчёт Exp или helper-probe улучшением native training.
>
> Один report.md по шаблону раздела 8, raw.json/per-row.csv/costs.csv/status.json только в R=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SILU01/results/SILU01-R1; checkpoint files только в S=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SILU01/s0/SILU01-R1; unknown=NOT_MEASURED. Только документируй результат как кандидата пользователю. Не исправляй production или тесты и не проходи полный цикл интеграции.
>
> Обязательный запрет: STOP/«закончить немедленно» — немедленно остановить действия. NEXT/«следующий эксперимент» — немедленно оставить предыдущую задачу и перейти только к явно заданной следующей в разрешённой фазе. Никакого продолжения предыдущего report/review/hash/git/warm-up/повтора ради «завершения». При явном STOP сообщи лишь доступный статус без новых действий. Не превращай build failure, 0 выполненных updates или 0/0 ответов в успешное обучение.

[silu]: https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/00.TensorCore/Core/TensorOps.cs#L1129-L1162
[trainer]: https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/04.TrainingLoop/Trainer.cs#L117-L147
[model]: https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/03.TransformerModel/LlmModel.cs#L219-L240
[block]: https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/03.TransformerModel/TransformerBlock.cs#L45-L65
[ffn]: https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/03.TransformerModel/Layers/SwiGLUFeedForward.cs#L40-L51
[lora]: https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/10.LoRA/LoraLinear.cs#L113-L131
[adamw]: https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/04.TrainingLoop/Optimizers/AdamW.cs#L188-L289
[sanity-source]: https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/experiments-v2/RP01-MICRO/LEARN-SANITY-PROBE/Program.cs
[sanity-raw]: https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/experiments-v2/RP01-MICRO/LEARN-SANITY-PROBE/probe-result.json
[restore-source]: https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/Program.cs
[lft-project]: https://github.com/lubochka/llm_mvp_core/blob/fb9175cc752d41e1cb525e0609b7c96140940217/experiments-v2/RP01-MICRO/LFT01/35.ExperimentsV2/Experiments/Rp01Micro/NEURAL-FROZEN-TRANSPOSE-01/Runner.csproj

