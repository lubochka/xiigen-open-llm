# 02. Новые конкретные гипотезы и их опровержение

19 октября 2026. Новые кандидаты дополняют прежние исследования. **Первую очередь восстановления language_translate они не заменяют.** Исходники ниже закреплены за a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6; baseline — f6032aff81f1f83de7c09cf9b9efe6b058dd12b9.

## 1. Граница новизны и доказательства

Обследованный корпус: 391 файл planning/possibleDifs верхнего уровня, 92 файла защищённого core каталога, 14 Markdown во вложенных planning-пакетах и 168 Markdown во вложенном core/experimets — всего 665 текстовых файлов. Перед публикацией дополнительно сверены 26 новых файлов других агентов (включая [E26][latee26], [routing][laterouting], [ProductHeldOut][lateeval]); объединённый перечень содержит 691 разных файлов. Все paths и Git blob SHA приведены в [06-CORPUS-INVENTORY.json](06-CORPUS-INVENTORY.json). Выполнены извлечение, тематический поиск и адресное чтение; это не заявление о ручном построчном review всех 665 или всех бинарных архивов.

| ID | Точное новое содержание | Что уже было известно | Сила вывода |
|---|---|---|---|
| N48-01 | Uncalled nested guard подавляет внешний ADD_NULL_CHECK и становится обучающей меткой E14 | Общий scope AST: P26; lambda family/arity: P84 | Source chain и предметный контрпример; native/quality NOT_RUN |
| N48-02 | Finite gradient → Inf в сохранённом AdamW v → finite weights → нулевой adaptive update после restore | Nonfinite gradients, overflow нормы clipping, restore shapes, AdamW scale cancellation | Source chain + NumPy FP32 arithmetic observation; trigger RP01 неизвестен |
| N48-03 | Один text triplet меняет recipe TeachingVector current→replay, что условно меняет relative pair weighting | P06/N26; N38.02/P89 density/loss/stop | Новый upstream refinement; recipe подтверждён, active-count drift и quality не измерены |

**Все три механизма наследуются из baseline.** Поэтому это не три доказанных регрессии, внесённых RP01. Для объяснения ухудшения нужен изменившийся caller, состав уроков, распределение gradients или способ использования состояния. Результат отрицательного поиска означает только отсутствие отдельной такой цепочки в обследованном корпусе.

## 2. N48-01 — невызванный вложенный guard портит метку внешней функции

### 2.1. Предметная постановка и точный источник

Домен: программирование C#, возможно документирование поведения C# API. Для диагностической панели заранее задано: параметр может быть null; перед внешним dereference функция обязана корректно обработать null. Если внешней защиты нет, целевое следующее исправление — ADD_NULL_CHECK. Само по себе наличие object-параметра без этого контракта не обязывает любой API добавлять guard.

ActionLabelGenerator.Label просматривает все MethodDeclarationSyntax. Для каждого method.Body он независимо вычисляет:

- Есть ли **любой** SimpleMemberAccessExpression среди DescendantNodes.
- Есть ли **любой** IfStatementSyntax, чьё условие содержит NullLiteralExpression или IsPatternExpression.
- ADD_NULL_CHECK возвращается только при первом признаке и отсутствии второго.

DescendantNodes включает тела lambda/local function. Следовательно, условие внутри невызванной вложенной функции может удовлетворить hasNullCondition внешнего метода. Проверки receiver, области исполнения и доминирования guard над dereference нет. [Точный source L22–72][action].

### 2.2. Минимальный контрпример

**A. Внешняя функция без защиты:**

```csharp
class C
{
    int F(object? x)
    {
        return x.GetHashCode();
    }
}
```

**B. Добавлена невызванная lambda; внешнее поведение при null осталось тем же:**

```csharp
class C
{
    int F(object? x)
    {
        System.Action unused = () =>
        {
            if (x == null) return;
        };
        return x.GetHashCode();
    }
}
```

По source A получает ADD_NULL_CHECK, B его не получает. В B нет collection parameter и не больше шести разных statement kinds, поэтому достигается fallback ADD_METHOD. Это статически выведенный expected результат будущего native-вызова, **не исполненный здесь C# output**. Независимый semantic gold у обоих: outerDereferenceNullProtected=false.

**Контроль, запрещающий слишком простой ремонт:**

```csharp
class C
{
    int F(object? x)
    {
        void Guard()
        {
            if (x is null)
                throw new System.ArgumentNullException(nameof(x));
        }
        Guard();
        return x.GetHashCode();
    }
}
```

Здесь локальная функция реально вызвана до dereference и защищает тот же x. Простое исключение всех вложенных тел из анализа теряет полезную защиту. Для общего ремонта нужны scope-aware summaries и связь вызова/receiver/порядка, а не blanket pruning. Этот пример — самостоятельный отрицательный контроль; качество будущего анализатора не считается заранее доказанным.

### 2.3. Как ложная метка доходит до обучения

SemanticCompilerFacade.TrainFileActionKnn строит пары FeatureVector(source), Label(source), затем заполняет FileActionKnnPredictor. Predictor.Train сохраняет эти labels. E14 driver создаёт training **и validation labels тем же генератором**, поэтому хороший score может означать хорошее воспроизведение неправильного правила. [Facade training][facade], [KNN training][actiontrain], [E14 driver][e14driver].

UseFileActionKnn по умолчанию false, prediction требует C# и минимум 4 examples. Facade возвращает прогноз, но downstream consumer ещё должен его использовать. Не найдено оснований объявлять этот producer источником DPO в прямом RP01 Student. [Options][compileroptions], [predicate gates][facadeconsumer].

FileActionKnn.cs совпадает с language_translate после copyright. Это **inherited conditional supervision defect**, а не новое изменение расстояния KNN. Общая тема AST scope уже есть в [P26][p26] и [P84][p84]; новизна — конкретное false suppression E14 label.

### 2.4. Маленький опыт и условие отказа от гипотезы

T0: вызвать actual Label на A/B и четырёх контрольных файлах — настоящий outer guard, uncalled local guard, invoked Guard, guard другого receiver. Заранее зафиксировать semantic gold и отдельно action-policy gold. Никакого обучения на этом шаге.

T1: A/B меняет **только generator обучающих labels**, сохраняя FeatureVector, k, корпус и порядок. Использовать небольшие разные source families; EXAM gold готовится независимо от обоих generators. Писать state labels, prediction и корректность нужного исправления. Actual ordinary consumer проверяется отдельно после local predictor.

Опровержение: actual pinned source не даёт заявленного flip; либо выбранный route не обучает/не читает этот predictor. Если labels исправились, но обученное качество не выросло, статус — SUPERVISION_FIXED_ONLY. Возможно, существующие features не различают scopes; расширение features тогда отдельный последующий фактор, а не скрытая часть первого A/B. [Подробный протокол](03-SMALL-DOMAIN-EXPERIMENTS.md).

## 3. N48-02 — внутренний AdamW moment может заморозить обучение при конечных весах

### 3.1. Цепочка на уровне одной координаты

AdamW хранит m и v в float[]. Формула current совпадает с baseline:

```text
m = beta1*m + (1-beta1)*g
v = beta2*v + (1-beta2)*g*g
w *= (1 - lr*weightDecay)
w -= lr*mHat/(sqrt(vHat)+epsilon)
```

Конечное g может быть настолько большим, что промежуточный FP32 second moment станет +Infinity. Тогда adaptive term равен нулю, а последующие обычные gradients не возвращают v к конечности. Сам вес остаётся конечным и меняется только от decay. Проверка «все weights конечны» или «weight hash изменился» этого не различает. [Current AdamW][adam], [baseline AdamW][oldadam].

Текущий DpoTrainer вызывает BackwardLogProbs, затем ApplyOptimizer; в показанной цепочке между ними нет clipping. ApplyOptimizer вызывает AdamW.Step. Portable parameter state и import проверяют names/shapes/lengths, но не finite m/v и v>=0. [DPO caller][dpostep], [optimizer call][adamcaller], [portable state][adamportable], [import validation][adamimport].

Сохранение проходит через CodeDocumentationSimulationDpoExperiment → RepositoryOwnedDpoOptimizerStateFile.WriteAndReadBack → AdamWPortableState.FromBytes. Bytes/hash validation и manifest map validation удостоверяют данные и форму, но не конечность moment. Следовательно, возникшая численная проблема может пережить штатный restart. [Simulation callback][simulation], [state file][optimizerfile], [manifest validation][optimizermanifest].

**Что пока не найдено:** actual RP01 gradient нужного масштаба, Inf moment в его checkpoint или утрата качества из-за этого механизма. Отсутствие clipping в source не доказывает, что такие значения возникали. Приоритет — условный P2 после восстановления baseline consumers.

### 3.2. Выполненный арифметический расчёт

В этом аудите выполнен один NumPy float32 proxy формулы, с явным округлением. Это **не CLR, не canonical portable parser, не Student и не обучение на DPO**. Полные значения сохранены в [07-FP32-ARITHMETIC-OBSERVATION.json](07-FP32-ARITHMETIC-OBSERVATION.json).

Параметры: beta1=0.9f, beta2=0.95f, lr=1e-5f, weightDecay=0.01f, epsilon=1e-8f; начальные w=1, m=v=0. Gradients: 1e20f, 1f, −1f, 1f. После первого шага выполнен обычный float32 byte roundtrip second moment.

| Шаг | Gradient конечен | v | Adaptive update | Конечный вес |
|---|---|---|---:|---:|
| 1 | Да | +Inf | 0 | 0.9999998807907104 |
| 2 | Да | +Inf | 0 | 0.9999997615814209 |
| 3 | Да | +Inf | 0 | 0.9999996423721313 |
| 4 | Да | +Inf | 0 | 0.9999995231628418 |

Byte roundtrip сохранил Inf. Fresh small-gradient control g=1 дал обычное adaptive update около 1e-5; диагностические FP64 moments сохранили ненулевое обновление. FP64 arm не является автоматически рекомендуемым product patch: это численный контроль для изоляции переполнения.

Старые [nonfinite/clipping исследования][prevnumerics] уже разбирали overflow нормы, в том числе на конечных gradients. Новизна здесь — **внутренний сохранённый v**, конечные веса и длительная потеря adaptive update. [AdamW scaling cancellation][prevscale] — другой, нормальный режим работы optimizer.

### 3.3. Native falsifier и предметный опыт

T0: один actual Parameter и AdamW; Step1(g=1e20f), затем actual ExportPortableState → ToBytes → FromBytes → TryImportPortableState в новый optimizer с тем же весом и восстановленными m/v/step. В новом optimizer выполнить Step2–4(g=1,−1,1). Снимать finite flags для g/m/v/w и adaptive delta отдельно от decay. Контроль — fresh state на обычных gradients. Native execution NOT_RUN.

Сначала read-only проверить доступные реальные optimizer artifacts. Если moments конечны и наблюдаемые gradients не достигают опасного диапазона, не тратить следующий большой learning budget на эту гипотезу. Малый численный guard всё ещё может быть полезен для целостности, но причинное объяснение RP01 отсутствует.

Если trigger найден, A/B начинать из **одного последнего корректного checkpoint**. Единственный learning-factor — заранее определённое безопасное ограничение gradient с устойчивым вычислением нормы; B-guard на finite prospective moments/export/import отдельно предотвращает невалидное состояние. Не обнулять испорченный v молча и не выдавать новый optimizer за точное continuation.

После native trigger достаточно коротких независимых панелей документации C# и технического перевода. Считать новые правильные ответы, удержание старых и стоимость. Устранение Inf при прежнем качестве — NUMERICAL_REPAIR_ONLY; оно не подтверждает рост качества.

## 4. N48-03 — один текстовый урок меняет recipe TeachingVector при replay

### 4.1. Что именно отличается

ProjectCurrent получает TeachingVector из SemanticTeachingPromptCompiler, который вызывает typed Build(DevelopmentPromptCompilation). Typed builder включает DevelopmentPredicateIds/PipelinePredicates и roots исходного/нормализованного текста. Затем Prompt расширяется полями branch_result, sub_phase, semantic_compressed и печатным teaching_vector_60. [Current projection][projectcurrent], [compiler][teachingcompiler], [typed/string overloads][vectorbuilder].

ProjectAuthorizedReplay в одноветочном случае сохраняет тот же Prompt. Но ProjectReplayPayload вызывает строковый Build(trainingText) по полному расширенному тексту. Mixer передаёт именно этот recomputed vector. Строковый builder выводит predicate channels только по своему root lookup и выбирает первые 32 уникальных roots. [Replay projection][projectreplay], [mixer][replaymixer].

Печатные числа teaching_vector_60 не распарсиваются обратно в float[]: NormalizeRoots делит текст по небуквенным символам. Например, explicit DOCUMENT/TEST channels могут исчезнуть при string recipe, если они были в actual typed compilation; новые metadata roots могут компенсировать число исчезнувших slots. Это смена представления, а не потеря точности числовой сериализации. [Builder и predicate slots][vectorbuilderfull].

**Контроль идентичности — одинаковые bytes Prompt/Chosen/Rejected, а не прежняя идентичность допущенной пары.** Вектор входит в digest. Новый B recipe должен быть выпущен владельцем с соответствующей attestation до admission; подмена после подписи не является допустимым опытом. [Payload digest][replaydigest].

### 4.2. Когда это действительно меняет обучение

DpoTrainer использует:

```text
VectorSignal(v) = clamp(1 + count(abs(v_i) > 1e-6)/60, 1, 2)
```

Он не читает здесь направления/знаки отдельных semantic coordinates. Только разное число активных slots может поменять этот scale. Scale входит в pair contribution и оба реальных backward, затем выполняется общий optimizer step. [VectorSignal][vectorsignal], [StepVector/backward][dpostep].

Следовательно:

- Разные массивы с одинаковыми active counts дают одинаковый вклад данного сигнала. Для такого урока причинное предположение опровергнуто.
- Направление изменения density заранее неизвестно.
- Общий scale одной пары может почти сократиться AdamW. Нужен контроль одной пары и смесь хотя бы двух разных gradients.
- У проверенного ordinary logical caller действует LegacyVersion: ExecuteDpoTeachingAsync не переопределяет версию, constructor выбирает legacy route. Для него density не умножает немедленный reported loss; такое умножение в L909 только exact. В legacy может измениться последующая траектория после actual update. [Legacy caller][legacycaller], [constructor][legacyconstructor], [DPO branch][dpostep].
- Переключение legacy→exact меняет objective/masks; оно не входит в A/B только recipe.

Успешный language_translate уже сохранял current vector и пересчитывал replay из manifest.TrainingText. Поэтому N48-03 — **inherited upstream refinement**, вне P0 восстановления. Близкие существующие темы — [P06.1/P06.3][p06], [N26][prevscale], дополнительно [N38.02][prevdensity] и [P89][p89]. Новым является конкретный lifecycle producer разницы одного text triplet. [Baseline replay][oldreplaymixer].

### 4.3. Самый короткий опыт

T0: один реально выпущенный single-branch урок и его authorized replay. Зафиксировать text hashes, успешность typed path/fallback, оба вектора, active counts и VectorSignal. Если counts равны, остановить этот кандидат на данной паре. Native trace NOT_RUN.

T1: одинаковая policy/reference/tokenizer/optimizer state, один новый и один replay lesson. A — current recipe; B — версионированный единый recipe с корректным admission. Порядок, pair weights, steps и material version неизменны. Сравнить реальные contributions, итоговый gradient и actual AdamW update. Различие loss без нужного изменения update — не доказательство обучения.

T2: учитывать настоящий output. Показанный BuildChosen возвращает общий текст capability/subphase. Для этого пути маленький предметный pilot проверяет **выбор нужной подфазы и retention** на новых перефразах задач документации C# или SQL-миграций. Утверждать улучшение объяснения закона C#/SQL можно только после нахождения consumer, который действительно выдаёт такие ответы; новый answer-producing helper не добавляется в recipe A/B. [BuildChosen][chosenformat].

## 5. Проверенные кандидаты, которые не вошли в список новых причин

| Кандидат | Что подтвердилось | Почему не стал новой доказанной причиной |
|---|---|---|
| Current игнорирует заранее compiled prompt | Контракт trainer действительно изменился | Ordinary baseline orchestrator тоже компилировал raw новым compiler; переноса learned override в этом caller не было |
| Restore WordMap фильтрует старые значения | Current import guard способен отбрасывать часть legacy mappings | В двух проверенных phase38 checkpoints word_map/surface_word_map пусты; active потеря не обнаружена |
| Нужно удалить duplicate-ID guard для возврата старого KNN | Old branch действительно позволял collisions | Индекс и dictionary тогда могли описывать разные строки; корректный recovery требует уникальных identities |
| Random dropout испортил RP01 | LoRA в принципе имеет dropout | Именно RP01 config и preflight требуют dropout=0 |
| Любой фиксированный алгоритм — ошибка | Некоторые fixed rules заменили другой learned consumer | Детерминированные предметные функции могут быть полезны; важен actual contract и measured result |

Источники: [old compiled contract][oldcompiled], [new contract][newcompiled], [actual baseline caller][oldcompileruser], [WordMap import][wordimport], [old IDs][oldexampleids], [old Train][oldknntrain], [new Train][knntrain], [RP01 config][rpconfig], [simulation][simulation].

Независимый source review N48-01 и N48-03 включил указанные контрпримеры и ограничения. Итоговая проверка пакета и приоритеты — в [04](04-SOURCES-COVERAGE-AND-REVIEW.md). Все будущие native/learning опыты остаются NOT_RUN.

[action]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/12.SemanticCompiler/Prediction/FileActionKnn.cs#L22-L72
[facade]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/12.SemanticCompiler/SemanticCompilerFacade.cs#L179-L215
[actiontrain]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/12.SemanticCompiler/Prediction/FileActionKnn.cs#L110-L141
[e14driver]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/35.ExperimentsV2/Experiments/E14ContextActionKnn.cs#L23-L54
[compileroptions]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/12.SemanticCompiler/SemanticCompilerOptions.cs
[facadeconsumer]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/12.SemanticCompiler/SemanticCompilerFacade.cs#L295-L334
[p26]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/cards/P26-structural-features.md
[p84]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/cards/P84-lambda-family.md
[adam]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/04.TrainingLoop/Optimizers/AdamW.cs#L56-L106
[oldadam]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/04.TrainingLoop/Optimizers/AdamW.cs#L54-L71
[dpostep]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/11.DPO/DpoTrainer.cs#L864-L925
[adamcaller]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/11.DPO/DpoTrainer.cs#L1219-L1227
[adamportable]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/04.TrainingLoop/Optimizers/AdamWPortableState.cs
[adamimport]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/04.TrainingLoop/Optimizers/AdamW.cs#L261-L307
[simulation]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L108-L156
[optimizerfile]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/14.ContinuousLearning/Training/RepositoryOwnedDpoOptimizerStateFile.cs#L110-L127
[optimizermanifest]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/14.ContinuousLearning/Training/CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifest.cs#L735-L802
[prevnumerics]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/59-nonfinite-admission-clipping-and-sft-state.md
[prevscale]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/30-teaching-vector-adamw-and-current-recipient.md
[projectcurrent]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/21.AgentInfrastructure/Logical/LogicalCapabilityTrainingPreferenceProjector.cs#L102-L156
[teachingcompiler]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/23.SemanticCompilerAdapter/SemanticTeachingPromptCompiler.cs#L24-L37
[vectorbuilder]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/23.SemanticCompilerAdapter/TrainingVectorBuilder.cs#L19-L61
[projectreplay]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/21.AgentInfrastructure/Logical/LogicalCapabilityTrainingPreferenceProjector.cs#L186-L254
[replaymixer]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/LogicalExperimentOptimizations.cs#L213-L249
[vectorbuilderfull]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/23.SemanticCompilerAdapter/TrainingVectorBuilder.cs#L77-L171
[replaydigest]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/21.AgentInfrastructure/Logical/LogicalCapabilityTrainingPreferenceProjector.cs#L270-L300
[vectorsignal]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/11.DPO/DpoTrainer.cs#L1159-L1169
[legacycaller]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L2275-L2299
[legacyconstructor]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L2015-L2036
[p06]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/cards/P06-teaching-vector-adamw.md
[prevdensity]: https://github.com/lubochka/xiigen-planning/blob/9cea3930067f216ccb812ecc5e412aec673f5058/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/38-CODEX-NEW-CAUSAL-GAPS-GENERIC-KNN-DPO-LOSS-REPLAY-FUSION-2026-10-16.md
[p89]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/cards/P89-density-stop.md
[oldreplaymixer]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/18.CapabilityTraining/LogicalExperimentOptimizations.cs#L153-L173
[chosenformat]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/21.AgentInfrastructure/Logical/LogicalCapabilityTrainingPreferenceProjector.cs#L356-L360
[oldcompiled]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L137-L140
[newcompiled]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L365-L367
[oldcompileruser]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs#L178-L188
[wordimport]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain/TranslationTrainedUnits.cs#L524-L584
[oldexampleids]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L762-L767
[oldknntrain]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L85-L98
[knntrain]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L100-L161
[rpconfig]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/config/code-documentation/generation-v1.json

[latee26]: https://github.com/lubochka/xiigen-planning/blob/3215267c0ee2c2beafebec950b45b7fee326daf7/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/44-E26-UNKNOWN-CONTEXT-AND-ORDER-2026-10-19.md

[laterouting]: https://github.com/lubochka/xiigen-planning/blob/3215267c0ee2c2beafebec950b45b7fee326daf7/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/44-NEW-LANGUAGE-ROUTING-CONTRACT-DIFF-2026-10-19.md

[lateeval]: https://github.com/lubochka/xiigen-planning/blob/3215267c0ee2c2beafebec950b45b7fee326daf7/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/45-NEW-PRODUCT-HELDOUT-SEMANTIC-GATE-2026-10-19.md
