# N63-AR-CAP — общий FIFO вытесняет историю другого адаптера

**Статус карточки: SPECIFIED_NOT_IMPLEMENTED / NATIVE_NOT_RUN.** Ни runner, ни его CLI не объявляются существующими. Здесь полностью задан контракт их небольшой реализации. Защищённая папка `docs/plans/replanning-possible-diffs` не является местом записи. Гипотеза условна: реальный общий междоменный lifetime `16.ReplayBuffer` не установлен; [доказательство и граница](01-NEW-REPLAY-CAPACITY-AND-HISTORICAL-EVIDENCE.md).

## Вопрос и предмет

Сохраняет ли небольшой резерв старых `CODE_Q` примеров внутри **того же общего лимита** полезный учебный сигнал при возврате к C#-объяснениям после обучения другого адаптера `BUG`? Проверяется реальный loss ученика после настоящих обновлений LoRA. `CODE_Q/BUG` — ключи адаптеров, а предмет первой проверки — C# guards и возвращаемые значения. Это не опыт общей языковой компетентности, перевода или AST kNN.

Причинная цепь: `ReplayBuffer.Add` → `Sample` → `AutonomousTrainer.PrepareBatch.MixedExamples` → `IncrementalTrainer` supervised executor → `Trainer.TrainSupervisedNextTokenStep` → новые веса → loss на прежних и новых C# правилах. Точные ссылки исходников приведены в основном документе. Будущий адрес исправления, только после решения пользователя: `16.AutonomousLearning/Training/ReplayBuffer.cs`, `Add`, выбор удаляемого элемента; фильтр Domain в `Sample` сохраняется.

## Фиксированные файлы и место запуска

Рабочий репозиторий исполнителя: `llm_mvp_core`, исходный commit `ce890a48efda7f00d0ee040f2383cc2d06428ea6`. Команды ниже выполняются из его корня. Для этого опыта создаётся **одна** изолированная ветвь рабочей копии без переключения текущей:

```sh
git worktree add --detach .worktrees/n63-ar-cap ce890a48efda7f00d0ee040f2383cc2d06428ea6
```

После перехода в `.worktrees/n63-ar-cap` единственный новый проект находится в уже принятом пространстве экспериментов:

`experiments-v2/RP01-MICRO/N63-AR-CAP/`

Только новые файлы: `N63ArCapRunner.csproj`, `Program.cs`, `NativeLearner.cs`, `Contracts.cs`, `ReplayBuffer.SourceA.cs`, `ReplayBuffer.SourceB.cs`, `fixtures/n63-ar-cap.json`, `source-provenance.json`, `README.md`, `results/run-01/**` и обычные build outputs этого worktree. Fixture берётся побайтно из [приложения](fixtures/n63-ar-cap.json), SHA256 `2d0f8316703c5064826c6e80831f60377b59321f4f0697d79642d6b71c5a3edc`.

Не редактировать существующий `35.ExperimentsV2.csproj`, dispatcher, Tests, skills или production source. Новый project находится вне glob существующего 35 и не меняет его сборку. Новые результаты не направляются в защищённую программу. Исследовательский host вызывает настоящий leaf learner прямо и не публикует адаптер в registry.

### Точный состав проекта

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <AssemblyName>N63ArCapRunner</AssemblyName>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="Program.cs" />
    <Compile Include="NativeLearner.cs" />
    <Compile Include="Contracts.cs" />
    <Compile Include="ReplayBuffer.SourceA.cs" />
    <Compile Include="ReplayBuffer.SourceB.cs" />
    <ProjectReference Include="../../../01.Tokenizer/Tokenizer.csproj" />
    <ProjectReference Include="../../../03.TransformerModel/TransformerModel.csproj" />
    <ProjectReference Include="../../../04.TrainingLoop/TrainingLoop.csproj" />
    <ProjectReference Include="../../../10.LoRA/LoRA.csproj" />
  </ItemGroup>
</Project>
```

Это compile contract, не выполненная проверка сборки. Транзитивные dependencies исходных проектов остаются их собственными; не импортировать 16/14/35 ради сборки полного pipeline. Public native learner используется тот же, что в production executor [27–31][executor]. Новый trainer, bigram/Python scorer вместо него и reflection для вмешательства в private state не нужны.

### Source-copy адаптер, одинаковый для A и B

В обе копии целиком перенести `16.AutonomousLearning/Training/ReplayBuffer.cs` исходного SHA; сохранить лицензию. Разрешённые одинаковые технические изменения: namespace `N63.ArCap.SourceA` / `SourceB`; два usings заменить на `using N63.ArCap.Contracts;`; убрать `: BaseService`; из конструктора убрать `ISessionStorage storage` и `: base(storage)`. `BaseService` не используется в `Add`/`Sample`. В `Contracts.cs` воспроизвести **только immutable** `TrainingExample` record, поля которого полностью совпадают с [оригиналом 13–19][dto]. Это локальный транспорт research fixture, не фиктивная production provenance.

`SourceA` не имеет иных изменений. В `SourceB` меняется только `Add` после добавления элемента. При превышении общего `maxCapacity=10` посчитать Domain counts, найти первый элемент в insertion order, чей Domain имеет больше 5 записей, и удалить этот элемент. Разрешённые fixture domains заранее ровно `CODE_Q` и `BUG`; иной Domain — ошибка входа до тренировок. При 11 элементах хотя бы один из этих domains имеет больше 5; если кандидат не найден, сохранить failure без подстановки другой политики. Пока размер≤10, удаления нет. FIFO внутри каждого домена остаётся; `Sample`, Random, replayRatio и фильтр Domain не изменяются. Это reservation5+5 внутри общего capacity10, не увеличение памяти до 20.

В provenance сохранить оригинальный Git blob `45ed25e29ee5d33bf90e325e3e3aa5aad23886b6`, SHA256 двух копий, исходные bytes, точный diff и hashes native dependency source files. Нормализацию лицензии, алгоритма Sample или порядка сравнения нельзя скрывать под namespace adaptation.

## Данные и единственное вмешательство

Fixture содержит 10 старых C# guard-уроков A01–A10,10 уроков другого адаптера B01–B10,5 новых C# result-уроков N01–N05 и 8 EXAM:4 прежних guard families,4 новых result families. Все строки и gold зафиксированы до измерений. Во всех строках Weight1; DPO отсутствует. Нет внешнего teacher call и фиктивного утверждения «это продуктовые approved пары». Это явно synthetic research fixture. Исходные code strings разделены; родственные шаблоны сохранены намеренно. Обобщение на независимые реальные проекты здесь не доказано.

Сравнение имеет три arms:

| Arm | Память после A→B | Возврат к A | Назначение |
|---|---|---|---|
| A current | Только 10 B, старых A0 | `Sample(N01..N05)` →5 новых | Точное нынешнее поведение source-copy |
| B reservation | A5+B5 | Та же `Sample` →5 новых+2 старых A | Единственное изменение eviction policy |
| C update-count control | Память current как A | Batch A, затем ещё N01,N02 в этом порядке |7 реальных updates, чтобы дополнительный бюджет B не выдавать за пользу именно старых уроков |

A/B отвечает на эффект политики с её фактической ценой. B/C сравнивает состав двух дополнительных предъявлений при одинаковом числе updates. C явно экспериментальный контроль, не proposed production algorithm. Число обученных токенов зависит от выбранных текстов и показывается отдельно:7 updates не означает одинаковые FLOPs или одинаковую сумму токенов. Не скрывать эту оставшуюся разницу. Семплирование единственное, seed42; фактические old IDs определяет исходный .NET Sample и они сохраняются, а не выбираются после взгляда на результаты.

## Настоящий learner и общая инициализация

`NativeLearner.cs` — тонкий orchestration adapter для публичных неизменённых `LlmModel`, `LoraApplicator`, `Trainer`, `AdamW`, `BpeTokenizer`. Он не реализует собственный backward/update.

1. Один BPE: `Train("",256)`, без merges, сохраняется один tokenizer artifact. Все fixtures ASCII; это намеренно общий byte vocabulary, не исследуемое вмешательство. Текст урока точно `Prompt + "\n" + Response`. Проверить, что `Encode(Prompt+"\n")` является префиксом `Encode(full)` и длина full≤257; иначе завершить данный запуск с `INVALID_INPUT`, без обрезания данных.
2. Общая модель: VocabSize256, ContextLen256, HiddenDim32, NumLayers1, NumHeads4, NumKvHeads2, FfnHiddenDim64; остальные значения `ModelConfig` текущего SHA. Один раз создать base, сохранить `base.bin`; отдельные модели загружать из этих bytes. Не сравнивать разные случайные base initialization. Сам initial seed базового конструктора не обещается: повторяемость обеспечивается сохранёнными bytes и их hash.
3. Применить native `LoraApplicator.ApplySeeded`, Rank8, Alpha16, Dropout0, TargetModules `["q_proj","v_proj"]`, rootSeed42, streamDomain `"n63-ar-cap"`; base parameters frozen. Проверить по actual parameter flags, что обучаемы только adapter параметры. Это фиксация состава learner, не отдельный эксперимент. Все arm models получают те же adapters из одного checkpoint.
4. `TrainingConfig`: BatchSize1, SeqLen256, WarmupSteps0, TotalSteps1000, MaxLr=MinLr=.001f, GradAccumSteps1, GradClipNorm1, WeightDecay0. `AdamW(beta1:.9f,beta2:.95f,weightDecay:0f,eps:1e-8f)`. Одинаковые значения во всех arms. Constant LR делает reset private scheduler step при новой thin wrapper нейтральным к LR; optimizer moments и step сохраняются.
5. Общая подготовка: один раз на отдельном CODE_Q learner последовательно выполнить 10 native steps по A01–A10. Сохранить `A-warm.bin` и portable AdamW state. Отдельный BUG learner из общего base+initial adapter получает 10 steps B01–B10. Ни одного B-gradient на CODE_Q model. После реального успеха каждого полного cohort добавить его строки во все соответствующие source-copy buffers. Failure любого native шага прекращает эту попытку и записывает фактический count; нельзя заменять результат successful stub.
6. Перед возвратом к CODE_Q для каждого A/B/C выполнить буквальную последовательность: `new LlmModel(config)` → `LoraApplicator.ApplySeeded(freshModel, fixedLoraConfig, 42, "n63-ar-cap")` → `model.LoadCheckpoint(A-warm.bin)` → `new AdamW(fixedArguments)` → импорт portable moments. `ApplySeeded` вызывается до загрузки полного checkpoint на свежих slots: loader не создаёт topology адаптеров. Portable moments импортировать через `AdamW.ExportPortableState`/`TryImportPortableState` и stable bindings `model.GetCheckpointParameterEntries().Select(e => new AdamWParameterBinding(e.Key,e.Parameter))`. Импорт должен вернуть true; иначе не переходить к update. Не применять owner-bound `RestoreState` от другого optimizer.
7. На каждом arm выполнить ровно свой batch через **`Trainer.TrainSupervisedNextTokenStep(model,optimizer,tokens,weight)`**. Сохранить checkpoint после обновлений; cold-load того же checkpoint для уже назначенного post measurement: создать свежую LlmModel, установить те же seeded adapters на свежих slots, затем вызвать LoadCheckpoint(final.bin). Импорт optimizer для read-only post measurement не нужен; повторного обучения нет. Portable optimizer после update сохраняется для аудита; дальнейший run не планируется.

Перенос полных WA weights/moments/step во все arms — общий исследовательский контроль. Default IncrementalTrainingSession factory создаёт новый AdamW на вызов; этот leaf-host не воспроизводит буквально весь её lifecycle. Вывод относится к replay policy при фиксированном полном optimizer state; перенос эффекта на pipeline с reset moments отдельно не доказан. Из-за этой границы новые arms или budget сюда не добавляются.

Начальный base создаётся один раз, общее предварительное обучение 20 calls выполняется один раз, затем 5+7+7=19 calls. Итого 39 вызовов native supervised step; фактические issued/completed calls логируются вокруг API, не только рассчитываются из длины массива. Нет повторов по seeds, дополнительного warmup, DPO, исправлений всей тестовой цепочки или автоматического следующего tier.

## Измерения до и после

Основной полезный показатель — **unweighted mean response-token negative log likelihood** на четырёх старых EXAM, отдельно на четырёх новых. Для каждого примера teacher-forcing forward на `Prompt + "\n" + gold Response` при `training:false`; в сумму входят только предсказания response tokens. Формула: среднее `logsumexp(logits[t,:])-logits[t,target]`; logits[t] предсказывает token[t+1]. Вначале response token соответствует t=prefixLength−1. Делитель — число оценённых response tokens, после чего отдельно вывести micro-token mean и per-example macro mean. Ни золотые ответы, ни EXAM gradient не попадают в обучение.

Фазы: общий W0 до A-обучения; общий WA после 10 A updates и после BUG pressure (CODE_Q state unchanged); cold-restored Wafter для каждого A/B/C. Общие W0 и WA оцениваются по 8 задачам один раз и используются одинаковыми численными baseline всех arms; потом по 8 задач на каждом из трёх cold-restored states. Итого 40 EXAM forward calls. Warm state self consistency проверяется сохранением canonical parameter hash при load, без лишнего quality panel. Если hash не совпал, результат недействителен для сравнения политики и причина сохраняется; источник восстановления не чинится в production.

Для каждого slice показать `L(W0)`, `L(WA)`, `L(Wafter)`, acquisition `L(W0)-L(WA)`, последующую потерю/улучшение `L(Wafter)-L(WA)` и contrast B−A/B−C. Если первоначального уменьшения old loss нет, не называть дальнейший contrast «сохранением уже усвоенного навыка»; это всё ещё измерение адаптации/likelihood на данных fixture, которое имеет более узкий смысл. Точность свободного текста здесь не измеряется и не требуется для успеха по loss. Нельзя выдать loss decrease за рост качества свободного ответа.

Дополнительно: per-example values, actual learner calls и optimizer-step deltas, consumed token counts, per-arm train wall time, checkpoint bytes, buffer counts по domains, replay IDs, изменившиеся adapter hash и frozen-base hash. Один Program выполняет общую подготовку и все arms в одном процессе; `PeakWorkingSet64` публикуется только как **whole-process peak**. Его накопленное значение после B нельзя вычитать из значения после A и называть RAM gain/regression конкретного arm; такой per-arm RAM показатель в этом запуске не измеряется. Loss, возвращённый native `TrainSupervisedNextTokenStep`, вычислен **до** данного update [117–147][native]; назвать его `train_loss_before_update`, а после-update quality взять из назначенного нового forward. Не сравнивать weighted training loss со unweighted EXAM loss как одну метрику.

**Правило успеха:** любое фактически измеренное улучшение полезного показателя признаётся успехом для этого показателя и домена, даже если остальные метрики ухудшились. В той же строке отчёта показать все ухудшения и цену. Для причинного утверждения о старых примерах полезнее B/C, а A/B включает отличие update budget. Hash change/membership count — только свидетельство механизма; отдельно они не являются успехом обучения. Нулевой gain сохраняется как `NO_OBSERVED_GAIN` **только для данной метрики и домена**; им нельзя отменять успех другой полезной метрики или назначать всему опыту неуспех. Отсутствие выигрыша не вызывает изменение fixtures или добавление бюджетов.

## CLI и конфликтные ситуации

После реализации ровно указанного host:

```sh
dotnet build experiments-v2/RP01-MICRO/N63-AR-CAP/N63ArCapRunner.csproj --configuration Release
dotnet run --project experiments-v2/RP01-MICRO/N63-AR-CAP/N63ArCapRunner.csproj --configuration Release --no-build -- --fixture experiments-v2/RP01-MICRO/N63-AR-CAP/fixtures/n63-ar-cap.json --out experiments-v2/RP01-MICRO/N63-AR-CAP/results/run-01
```

`Program` принимает ровно `--fixture` и `--out`; fixture ID проверяется на `N63-AR-CAP`, параметры программы не переопределяются аргументами. Выходы: `provenance.json`, `batches.json`, `steps.jsonl`, `metrics.json`, `answers.md` (для каждого EXAM gold и raw model NLL, не придуманный free answer), `report.md`, checkpoint/tokenizer/portable optimizer файлы и `failure.json` при ошибке.

| Конфликт | Заранее заданное действие |
|---|---|
| Worktree уже существует | Сверить pin и сохранённый provenance. При совпадении продолжить только незавершённую подготовку того же опыта. Иной pin/fixture — записать конфликт вне его results и не перезаписывать; не создавать скрытый второй run |
| Входной файл уже существует | Если bytes идентичны назначенным, использовать; если различаются, сохранить diff/конфликт и завершить подготовку. Старый файл не исправлять автоматически |
| Native dependency не собирается | Сохранить build log/exit code; не подменять learner, не чинить production, не запускать всю Tests suite. Сборка не считается завершённой по наличию DLL |
| .NET9 отсутствует | Статус `ENVIRONMENT_BLOCKED`,0 native updates; без Python/JS imitation вместо обучения |
| Run01 содержит завершённый result | Прочитать и проверить его provenance; не тренировать повторно для «подтверждения». Анализ имеющегося результата не создаёт новый run |
| Процесс оборвался | Actual logs определяют completed stage; не считать timeout success/failure качества. Не повторять уже завершённые updates; если нельзя продолжить точное состояние, отметить неполноту, сохранить всё и не выбирать другой arm/budget |
| Корпус/HEAD позже изменился | Frozen worktree и fixture текущего опыта остаются прежними; не обновлять pin в середине |

## Десять правил этой карточки и промпта

1. Выполнить маленькую проверку обучения в указанном предмете C#, только затем обсуждать перенос в другие domains.
2. Любое измеренное улучшение полезной метрики — успех по этой метрике/домену; рост accuracy не обязателен, регрессии и стоимость показать рядом.
3. Membership, hashes и арифметика ratio без реального learner не заменяют учебный эксперимент; отдельную программу таких измерений не запускать.
4. Для входов/restore сохранять связь с native update и назначенным EXAM; оригинальный адрес будущего исправления — `ReplayBuffer.Add`, фильтр Domain неизменен.
5. Этот условный P2 не задерживает разблокирование действующего обучения; обязательные обновления пользователя имеют приоритет над несвязанными карточками.
6. Работать только в новом experiment worktree/source copies, не менять production и не чинить всю test chain до доказательства пользы и решения пользователя.
7. Место, source dependencies, CLI, budget и решения конфликтов заданы выше; исполнитель не ищет другой runner/каталог/политику.
8. Только добавлять новые файлы; старые эксперименты, результаты и исходные копии не удалять и не заменять.
9. Очередь: разблокирование обучения → конфликты карточек → обновления пользователя → лишь затем проверки без прямого участия в обучении.
10. Допускается исследовательский прямой вызов native learner и синтетические fixture; эти исключения не распространяются на product training, admission, publishing/submitting. Не фабриковать teacher/provenance receipts.

## Готовый промпт исполнителю

> Реализуй и выполни только N63-AR-CAP по замороженной карточке. Используй указанный worktree, новый csproj, fixture с указанным hash и две source copies ReplayBuffer. Сохрани native Trainer/LoRA/AdamW без правок. Выполни общую подготовку 20 и A/B/C=5/7/7 supervised calls; измерь назначенные 40 EXAM forwards. Получи current FIFO против reservation5+5, а B/C используй как контроль количества updates. Никаких отдельных buffer-only опытов, дополнительного warmup, seeds, generators, повторов уже завершённого run и полной тестовой цепочки. Отчёт честно ограничь C# response NLL в условной общей памяти, а не существующим RP01 caller. Соблюди все десять правил карточки: (1) малый предметный learner-first опыт; (2) любой полезный metric gain — успех на этой оси, accuracy не обязательна, потери рядом; (3) без нерелевантных обучению proxy; (4) точная цепь вход/restore→update→EXAM и исходный адрес ремонта; (5) не задерживать разблокирование текущего learner; (6) только эксперимент до решения пользователя о production; (7) ровно назначенные paths/CLI/conflict resolution; (8) только добавления; (9) learning→конфликты→пользовательские обновления→indirect; (10) экспериментальные исключения не переносить в prod/submission и не создавать фиктивных receipts. Параметры, gold и бюджеты после результата не менять. Успех по отдельной метрике не даёт права самостоятельно внедрить изменение.

[executor]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/14.ContinuousLearning/Training/SupervisedTrainingExecution.cs#L27-L31
[dto]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/14.ContinuousLearning/Core/Core.cs#L13-L19
[native]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/04.TrainingLoop/Trainer.cs#L117-L147
