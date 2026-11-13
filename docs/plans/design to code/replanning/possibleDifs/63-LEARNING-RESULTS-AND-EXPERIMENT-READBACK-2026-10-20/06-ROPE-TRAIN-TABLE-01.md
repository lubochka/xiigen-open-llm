# ROPE-TRAIN-TABLE-01 — повторно использовать позиционные таблицы в настоящем уроке

**Статус: гипотеза документирована; host не создан; build и обучение не выполнялись.** Карточка задаёт будущую подготовку и один маленький A/B, не заявляет готовую CLI и сама не разрешает запуск. Основной код и защищённая папка `docs/plans/replanning-possible-diffs` в текущем задании остаются без изменений.

**Критерий Любы:** любое измеренное улучшение полезного показателя — успех по этому показателю и предметной теме; рост точности не обязателен. Показывать остальные изменения, включая ухудшения. Для эквивалентной оптимизации отдельно проверять численную эквивалентность: её нарушение не скрывается экономией памяти. Один фиксированный A→B не устанавливает причинное ускорение по wall/CPU/whole-process peak; наблюдённые значения всё равно показываются, а не заменяются «не измерено».

**STOP/NEXT обязателен и для промпта:** при «заканчивай и переходи к следующему» немедленно оставить прежнее задание и перейти к уже указанному следующему в разрешённой фазе. Запрещено сначала дописывать отчёт, карточку, реестр, hashes, Git, review, повторять запуск или делать «последнюю проверку». При «стоп/закончить немедленно» прекратить работу полностью; допустимо только необходимое прекращение собственного ещё работающего процесса. Частичные данные и старые опыты не удалять. Конец назначенной очереди не разрешает возврат к прошлому опыту.

## Что проверяется и почему это обучение

Предметная тема — программирование C#. Профиль — предсказание знака целочисленного вычитания. Получатель — один `TransformerModel.LlmModel`, q/v-LoRA, native SFT и AdamW. RoPE — механизм представления позиции, не отдельный домен и не обучаемая Unit.

В [исходном `GetFrequencies:50–72`](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/03.TransformerModel/Layers/RotaryEmbedding.cs#L50-L72) заново вычисляются frequency/position/outer/cos/sin при каждом forward. [Реальный `TrainSupervisedNextTokenStep:102–145`](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/04.TrainingLoop/Trainer.cs#L102-L145) вызывает именно этот Transformer, затем backward и optimizer. [`CausalSelfAttention:146–153`](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/03.TransformerModel/Layers/CausalSelfAttention.cs#L146-L153) передаёт таблицы Q/K, а [`ApplyRotaryEmbedding:1277–1363`](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/00.TensorCore/Core/TensorOps.cs#L1277-L1363) читает их и в native backward. Это расход внутри настоящего обучения, а не тест наличия метода.

**A:** исходные таблицы вычисляются заново на каждом TRAIN forward. **B:** внутри каждого `RotaryEmbedding` хранится одна пара cos/sin для последней положительной `seqLen≤64`, только для training и offset0. При той же длине повторно использовать её, при другой длине заново вызвать исходный `GetFrequencies` и заменить запись. Одновременных кэшированных длин — одна. Начальные EXAM не заполняют кэш; его первое заполнение оплачивается внутри первого TRAIN. public `GetFrequencies`, inference и KV-cache offset path сохраняют исходное поведение.

## Конкретное изменение только в копии

1. В копии `RotaryEmbedding.cs` добавить internal метод `GetTrainingFrequencies(int seqLen, CancellationToken ct)`. A-ветвь возвращает исходный `GetFrequencies(seqLen,ct)`; B-ветвь повторно использует только одну приватную запись данного экземпляра. Переключатель `internal static bool ExperimentReuseTrainingTables` действует только в этой экспериментальной сборке; перед TRAIN host задаёт A=false/B=true. Создание записи ленивое, внутри измеренного TRAIN. `_dim` и `_theta` readonly, offset всегда0; ключ seqLen достаточен только в этой узкой области. Для seqLen>64 вызывать исходный код без сохранения. Перед lookup сохранять проверку отмены; при invalid seqLen вызывать исходную валидацию. Не менять формулы, порядок float-операций, TensorOps или данные таблицы.
2. В копии `CausalSelfAttention.Forward` заменить **только** получение частот: `training ? _rope.GetTrainingFrequencies(seq, cancellationToken) : _rope.GetFrequencies(seq, cancellationToken)`. Остальной forward и весь `ForwardWithCache` сохраняются. Cos/sin из приватного метода доступны только этому внутреннему caller, который их читает. Кэш не меняет таблицы на месте: при другой длине создаёт новые, чтобы текущий backward мог читать ранее захваченные tensors.
3. Счётчики `TrainingFrequencyBuilds`, `TrainingFrequencyReuses` и retained float-payload снять по уже выполненным TRAIN-вызовам. В A считать фактические сборки; в B — реальные misses/hits, а не заранее заполненное ожидание. Они описывают механизм, не заменяют allocated bytes.
4. Не включать frozen-transpose/RMS/PAD/reference-cache, pooling, изменение dtype, векторизации, learning rate, числа уроков или loss. Native36 исходники берутся из **0b76ab950d82534f5904005dda096545505ae307**, не из уже изменённой LFT01 source-copy. Совпадение текущего исходника с историческим scaffold не предполагается.

## Данные, начальное состояние и бюджет

| Поле | Назначено |
|---|---|
| TRAIN0 | prompt `C# result sign: 9-4 =`, answer `+` |
| TRAIN1 | prompt `C# result sign: 4-9 =`, answer `-` |
| EXAM0 | prompt `C# result sign: 8-3 =`, expected token `+` |
| EXAM1 | prompt `C# result sign: 3-8 =`, expected token `-` |
| Encoding | UTF-8 bytes0–255, без BPE Train и EOS; это выбранный вход native SFT, не измерение токенизатора |
| Student | Vocab256, Context64, Hidden64, NumLayers1, NumHeads4, NumKvHeads2, FFN176, theta10000, RMS eps1e-5 |
| Base | `Initializers.UseDeterministicSeed(62001UL)` при construction в каждом независимом процессе |
| LoRA | `ApplySeeded`, root62002UL, stream `rope-training-table-probe-v1`, rank4/alpha8/dropout0/q_proj+v_proj |
| SFT | fresh Trainer; BatchSize1, SeqLen64, Warmup0, TotalSteps4, MaxLr=MinLr=.001, GradClip1, GradAccum1, WeightDecay0 |
| Optimizer | fresh AdamW(.9,.95,0,1e-8) в обоих плечах |
| Schedule | TRAIN0→TRAIN1→TRAIN0→TRAIN1; один `TrainSupervisedNextTokenStep` на строку, weight1 |
| EXAM | по одному readout обеих EXAM до и после4 updates;8 readout forwards на весь A/B, отдельно от TRAIN-fit |
| DIAG_TRAIN | те же две существующие TRAIN строки в A0/A1/B0/B1; ещё8 training:false forwards вне TRAIN timer; обновлений не добавляется |
| Actual budget | 4 SFT+optimizer/arm, всего8; training forwards8, backwards8; readout forwards16 (8 TRAIN-fit +8 EXAM); DPO0; внешний teacher0; warmups/repeats0 |

Пример зависит от обучаемого веса: `[prompt+answer]` даёт next-token targets, native backward меняет q/v-LoRA, EXAM считывает последние logits. Это короткий профиль, не доказательство освоения арифметики. Равное0/2 exact само по себе не доказывает сохранный или сломанный навык; отдельно показать фактическое движение NLL и learned state. Удлинять до400 шагов, пока не получится успех, запрещено.

## Где создать будущий host, как собрать и как разрешить конфликт

**Host:** `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RPE01/a001`. **Results:** тот же root + `/results`. Это новые каталоги вне защищённой папки. **Отчёт:** `results/report.md`; успешный кандидат для решения Любы — `results/candidate.md`, затем документная сессия добавляет его в разрешённый planning registry. Исполнитель не пишет в защищённый core каталог.

Использовать native36-path список `source_paths_36.json` из этого пакета; ровно эти файлы материализовать из закреплённого Git-объекта в `src/<native path>` через `git show <SHA>:<path>` с двоичной записью stdout. Не копировать bin/obj, весь solution, готовые LFT01 source-copy или исходники другой ветки. Это чтение Git-объектов, не checkout/fetch/push.

Источник готовой формы host — [проверенный существующий Program LFT01](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/experiments-v2/RP01-MICRO/LFT01/experiments-v2/RP01-MICRO/NEURAL-FROZEN-TRANSPOSE-01/Program.cs). Скопировать только `Program.cs` в новый root как основу, добавить `using TransformerModel.Layers;`, убрать все обращения к `Linear.Experiment*`, назначить новый ID/stream/CoreCommit из карточки и счётчики RoPE из предыдущего раздела. Сохранить таблицы, native4 calls, before/after EXAM, параметры, costs и автоматические state receipts. До `Observe(model)` установить `RotaryEmbedding.ExperimentReuseTrainingTables=false`; непосредственно перед измеренным TRAIN — armB; EXAM после использует training=false и не пополняет кэш. Строку receipt `exactly native BPE with zero merges` заменить на `UTF-8 bytes0..255; no tokenizer Train/Encode invoked; no EOS`: фактический host использует `Encoding.UTF8`, а не вызов токенизатора. Подпись `unit_count` заменить на `native_learner_instances=1`, `Unit/Flow=не используются`. Указанная строка receipt — описание поля `byte_tokenizer`; утверждение об эквивалентности настоящему BPE не наследовать.

**Точное дополнение DIAG_TRAIN из позднего883496:** обобщить существующий `Observe(LlmModel model)` до `Observe(LlmModel model, IReadOnlyList<(string Prompt, string Answer)> rows)` и заменить внутри только `Exam.Select` на `rows.Select`. Перед timer записать `before=Observe(model, Exam)` и `train_before=Observe(model, Train)`, после timer и существующих state receipts — `after=Observe(model, Exam)` и `train_after=Observe(model, Train)`. Итого16 forwards на A/B, ровно8 новых; ни одного нового backward/update/Generate. Существующие ответы состоят из одного UTF-8 token, поэтому argmax exact здесь одновременно full-response exact для данного контракта. В том же уже вычисленном последнем256-logits добавить `target_rank = 1 + count(logit > targetLogit or (logit == targetLogit and tokenId < goldId))`, без ещё одного forward. Raw отдельно содержит TRAIN-fit NLL/rank/exact и EXAM NLL/rank/exact; TRAIN-fit не называть обобщением. Все эти вызовы `training:false`, training-only RoPE cache не заполняют и в его TRAIN cost не входят. Команда run.py,4 updates/arm, модель, данные и source pin остаются прежними; DPO-specific wrapper к этому самостоятельному SFT host не добавляется.

`Runner.csproj` — net9.0 console, AssemblyName `ROPE-TRAIN-TABLE-01`, implicit usings и nullable enabled, default Compile disabled; compile `src/**/*.cs` и `Program.cs`; Directory.Build.props/targets импорт отключён. Стандартный Microsoft.NET.Sdk. Список36 задаёт native зависимости по форме существующего host; наличие всех36 current файлов проверено чтением, новая сборка не выполнялась. `PackageReference`, NUnit и product `.sln` в рецепт не входят. В `run.py` задать единственный result-root `root/results`; одна сборка≤120с, два свежих процесса A→B по≤60с, общий run deadline240с (120с build + 60с A + 60с B; каждый локальный лимит также действует). Все logs/receipts открывать create-new. Exact будущие команды внутри runner:

```text
dotnet build Runner.csproj -c Release --output bin -p:ImportDirectoryBuildProps=false -p:ImportDirectoryBuildTargets=false
dotnet bin/ROPE-TRAIN-TABLE-01.dll A code-sign results/A/code-sign.json
dotnet bin/ROPE-TRAIN-TABLE-01.dll B code-sign results/B/code-sign.json
```

Единственная пользовательская команда **после порученной подготовки**:

```powershell
Set-Location 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RPE01/a001'
& 'C:/Python314/python.exe' ./run.py
```

Подготовка должна записать точный source manifest/one-factor.diff и future runner, но сама не запускает build. Если source object или dependency отсутствует — PREPARATION_FAILED, назвать точный файл; не искать другой host и не чинить product. Если a001 содержит чужие файлы либо results/bin/obj уже существуют — сохранить их, отметить существующую попытку и перейти по назначению; не очищать, не придумывать a002/resume и не обходить ограничение другим ID. При timeout/сбое сохранить известный stage; отсутствие receipt означает `actual_updates=не измерено`, не0. Build failure означает training не началось. Допустимы throw/try-catch/void, plain DTO и один Program в копии; продуктовые запреты, полный CI, DI-слои, PR и тестовые ремонты к этому эксперименту не применяются.

## Измерение, отчёт и следующий пункт

Снять A0/B0 и A1/B1: full final256 logits каждого TRAIN-fit и EXAM отдельно, target-NLL, target-rank, argmax token/full-response exact; native4 pre-update losses; actual optimizer step, initial/final trainable state и final optimizer state из уже записанных автоматических receipts; frozen state unchanged. Градиенты после ZeroGrad не измерять и не выдавать за сохранённые raw. Стоимость — только интервал четырёх TRAIN: allocated bytes текущего потока, wall, CPU после Refresh, GC0/1/2. Retained cos/sin payload — отдельная строка. Whole-process peak до окончания TRAIN — отдельная область; не называть его retained heap. Подготовка, сборка, readouts, state export и отчёт вне TRAIN-cost.

Показать ΔA=A1−A0, ΔB=B1−B0 и ΔB−ΔA для NLL/exact. TRAIN-cost — цены одинаковых переходов A0→A1/B0→B1; не «0до→затраты после». Число сгенерированных таблиц само по себе не измеряет экономию памяти/секунд. При expected equivalent optimization все4 loss и конечные состояния/readouts сравниваются; различия показываются явно. Положительное улучшение по одной оси сохраняется как таковое даже при ухудшении другой, без автоматического разрешения переноса в основной код.

Русский `report.md`: до350 слов плюс таблица фактических метрик и raw links. Разделы: что меняли → на чём учились → A0/A1/B0/B1 и цены TRAIN → выигрыши/ухудшения → границы/предлагаемый diff. При успехе `candidate.md` до120 слов: методы выше, метрика выигрыша, ухудшения, область проверки, ссылка на report. Одно readback, только фактические/ссылочные исправления, затем завершение. Никаких новых checksum/обучения/Git ради отчёта. Незаписанное измерение обозначать «не измерено». Формат примера: «В настоящих4 уроках allocations A___ B___; retained tables B___; NLL A0___ A1___ B0___ B1___; наблюдаемые wall/CPU___; state эквивалентен/различается___». Числа заполняются только после будущего реального запуска.

Порядок закреплён в [04-REGISTRY.md](04-REGISTRY.md): существующие прямые пункты05–15 и17–31 → **ROPE-TRAIN-TABLE-01** → **AST-SIB-CACHE-01** → **NG01** → зависимые readers16 `IDENTITY-M1-01` и32 `KN-ROUTER-LEARNED-KEY-01`. Первые01–04 завершены. Это добавочная программа на случай её поручения; защищённая очередь не изменена, её текущий next остаётся №05 N62-R2, а перестановка reader16 — адресная поправка planning-реестра. При поручении только этого ID после назначенной фазы закончить. При явно порученной дополненной программе следующий после ROPE — AST-SIB-CACHE-01 в назначенной фазе; не возвращаться к BPE/N-KU/frozen transpose. Если поручена только документная подготовка, «следующий» означает следующий документ, а не запуск обучения.

## Промпт исполнителю

Используй модель текущей сессии по умолчанию, без override; один worker и последовательный reviewer с его default, фактическую модель отметить, неизвестную не выдумывать. Не ищи скил/шаблон заново: локальные `docs/skills/architect-behavior-classifier-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `code-examination-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `output-readability-gate-SKILL.md`, `human-communication-GLOSSARY.md`. Уже прочитанные неизменные скилы повторно не обходи.

Текущая фаза документная. При будущем поручении реализации подготовь только RPE01/a001, 36 закреплённых native sources и изменения двух методов по карточке; сборку/обучение выполняй лишь в назначенной фазе. При поручении запуска — один A/B,4 native SFT updates на плечо, точные2+2 строки,16 назначенных readouts (8 TRAIN-fit и8 EXAM) и budget выше, без чужих опытов, оценок сверх назначенного бюджета/прогрева/400steps. Показатель обучения или его реальной стоимости обязан участвовать в measured transition; вызов `GetFrequencies` вне урока не заменяет эксперимент. Любое полезное измеренное улучшение — успех по оси/домену; точность расти не обязана, ухудшения и численные расхождения рядом. Один русский report и кандидат; для дополненной программы следующий AST-SIB-CACHE-01, для одиночного поручения — конец; перенос в production решает Люба. При «следующий» немедленно прекрати прошлую работу без дописывания/проверок/новых вызовов; при «стоп» останови работу, кроме необходимого прекращения собственного процесса. Старые эксперименты, raw и failed attempts сохраняй.
