# N60 / E-DPO-VALID-LENGTH — конкретный проект изменения исходного пути

**Уточнение исполнения по a822 (20.10):** ссылки на оригинальные файлы ниже обозначают происхождение и адрес предлагаемого последующего переноса. Сначала проверяется точная экспериментальная копия A/B в существующем проекте `34.Experiments` или `35.ExperimentsV2`, с малым host в `experiments-v2/RP01-MICRO/<ID>/`. Оригинальный продукт и его тесты остаются read-only. Копия сохраняет настоящий алгоритм, зависимости, admission и mutation capabilities; их обход, поддельный lease или суррогатный learner недопустимы. До исполнения фиксируются copied-file hashes, A/B diff и actual binding вызовов. Перенос в основной код — отдельное решение после результата. Текущий запрет пользователя менять protected распространяется и на results: для возможного запуска выбирается разрешённый каталог вне protected. Подробнее: [финальная дельта](12-CONCURRENT-UPDATES-AND-NOVELTY.md).

Дата: 20.10.2026. Core `bf9b543e149ef197da85a767821d98c9b67916d2`. **READ_ONLY_RESEARCH · CODE_PROPOSAL_ONLY · NOT_IMPLEMENTED · NOT_RUN.** Это уточнение уже присланной гипотезы ND-DPO-PAD из активного [23][active-pad], не новое открытие. Защищённая папка опытов и core не изменялись. Данные ниже существуют только как текст этого документа; runtime dataset и runner не создавались.


## Актуальность и готовность после обновления 20.10

Текущие защищённые карточки перечитаны на `f5dd532045716495a2c7668573250e5d7cc46810`; новый полный23 и execution-policy проверены отдельно. Сравнение с исходным `bf9b543e149ef197da85a767821d98c9b67916d2` содержит 152 изменения только защищённых документов, без изменения рассматриваемого кода. Поэтому immutable code-ссылки ниже сохраняют исходный pin; ссылки на действующие правила/карточки обновлены.

[Правило20.10](https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/00-execution-policy-2026-10-20.md) отменяет общий approval-gate и обязательное новое личное подтверждение каждого ID/SHA. Подготовить доступные данные, уточнить native host и записать identities — работа исполнителя в пределах порученной программы. Эти проекты **PREPARABLE**, а не blanket BLOCKED; конкретный внешний blocker допустим только после документированной безуспешной попытки получить необходимый артефакт. `NOT_RUN` — отсутствие измерений. В нынешнем поручении результат — доказательный аудит и Markdown, поэтому здесь не выполнялись реализация/сборка/обучение; дополнительный approval-flow для завершения документов не требуется.

Проверки загрузки, восстановления и реального потребителя доказанно обученного состояния допустимы без повторного декоративного Train при точной цепочке зависимости и предложении исправления original code. PAD/HEAD — более узкий случай стоимости новых actual updates; их собственный A/B действительно обучает обе стороны.

## Что сравниваем и зачем

При обучении C#-документации на настоящих q/v LoRA один ответ иногда длиннее другого. Current exact material дополняет обе стороны до общего `T=max(ChosenLength,RejectedLength)`, хотя response mask короткой стороны равна нулю на хвосте. Нейронная модель всё равно вычисляет этот хвост. A сохраняет нынешний расчёт. B использует полную собственную длину каждой стороны **после** исходной валидации. Измеряем стоимость этих двух настоящих уроков и действие updates. Рост качества ответа не обещается; Generate в основной опыт не входит.

**Правило каждой карточки:** любое фактически измеренное полезное улучшение считается успехом по соответствующему показателю и домену; рост accuracy не обязателен. Другие изменения, включая ухудшения, показываются рядом. Если время улучшилось, а численная эквивалентность не выдержана, это разные результаты: ресурсный выигрыш по оси и опровергнутая эквивалентность. Нулевой действительный эффект обучения допустим. Frozen-forward, арифметическая демонстрация или hash без реального native обучения не заменяют этот опыт.

## Доказанная связь с обучением

| Звено current | Установленный факт | Место предлагаемого изменения |
|---|---|---|
| `DpoExactPairMaterial` [17–46,90–141,171–198][material] | Хранит неизменяемые исходные длины и digest; mutable IDs/masks обязаны полностью совпасть с полным carrier. Ёмкость проверяется по общему T. | **Не менять** построение, digest и `CaptureAndValidate`; B получает отдельный локальный view только после этих проверок. |
| `CodeDocumentationSimulationDpoExperiment.Run` [105–140][owner-material] | Исходные SHA/count/JSON, saved B0, tokenizer SHA, exact masks, typed provenance, legal content, LoRA recipient и execution profile проверяются перед получением mutation capability. | Узкий новый C# mode с двумя точными строками и той же цепью допуска. |
| `DpoTrainer.StepVectorCore` [832–930][step] | Capture всех tensor carriers → `RequireVectorPairAdmissions` → `AcquireMutationPermit` → четыре forwards на пару → два backward → один `ApplyOptimizer` на minibatch. | Только передача execution extent в четыре forward helper внутри этого метода. |
| `BatchLogProbs` / `ForwardPolicyBatchLogProbs` [1007–1029][forwards] | Передают полные padded IDs в `LlmModel.Forward`; reference inference и policy training используют ту же полную длину. | Согласованно выбрать IDs и **response mask** каждой собственной длины до вызова прежнего Forward. |
| `ComputeTargetLogProbs` / `BackwardLogProbs` [1035–1121][loss] | Target `ids[i+1]` использует logit row i; exact суммирует masked response log-probs. | Формулу, знак, сумму, Beta, вес пары и target mapping не менять. |
| Owner [175–218][owner-loop] | Настоящий trainer сохраняет candidate, AdamW и RNG, затем заново читает их штатными codecs. | Сохранить этот конечный получатель и его readback; не вводить surrogate learner. |

B не ищет конец по токену 0: это допустимый vocab ID и одновременно current filler. Источник длины — только проверенный `ExactMaterial`. Нельзя обрезать до `Length−1`: target с индексом `Length−1`, предсказываемый logit `Length−2`, должен остаться в IDs и mask. Нельзя выкинуть prompt: его hidden states участвуют в attention и backward. Crop до собственной длины не снимает исходное ограничение context capacity и не превращает невалидный carrier в валидный.

## Blueprint для исходных файлов, без готового патча или команды исполнения

### 1. `11.DPO/DpoTrainer.cs`

В этом же файле предложить internal execution extent с двумя значениями `Padded` и `ExactSideLength` и новый **internal** timed overload. Нынешние public overloads продолжают default `Padded`. `ContinuousLearning` уже является friend assembly [DPO.csproj][friend]; новый публичный способ обхода допуска не нужен.

`StepVectorCore` получает выбранный extent; после всех `CaptureBatch` и `RequireVectorPairAdmissions`, но до mutation permit, B дополнительно требует exact-material version у всех batches, существующие `_projectRecipient` и `_executionProfile`, действительный recipient и dropout0. Внутри `DpoProjectLoraRecipient` можно добавить узкий `RequireZeroDropoutExecution()`: сначала нынешний `RequireValid()`, затем отказ при `_config.Dropout != 0`. `RequireValid` уже сверяет `_config.Dropout` с фактически установленными LoRA [recipient][recipient]. Конфигурация из комментария не считается доказательством.

Проект локального helper, который вызывается только для захваченного и проверенного batch:

```csharp
// CODE PROPOSAL ONLY. Not applied or compiled.
private static (Tensor Ids, Tensor Mask) ForwardInput(
    DpoBatch captured, bool chosen, bool exactSideLength)
{
    var ids = chosen ? captured.ChosenIds : captured.RejectedIds;
    var mask = TargetMask(captured, chosen);
    if (!exactSideLength) return (ids, mask);
    if (captured.MaterialVersion != DpoExactPairMaterial.Version ||
        captured.ExactMaterial is not { } material)
        throw new DpoMutationAdmissionException(
            "Exact validated material is required for side-length execution.",
            DpoExactPairMaterial.InvalidErrorCode);
    var length = chosen ? material.ChosenLength : material.RejectedLength;
    // The FULL captured carrier has already passed original shape/digest checks.
    if (length == material.SequenceLength) return (ids, mask);
    return (
        new Tensor(ids.Data.AsSpan(0, length).ToArray(), new TensorShape(1, length)),
        new Tensor(mask.Data.AsSpan(0, length).ToArray(), new TensorShape(1, length)));
}
```

`BatchLogProbs` и `ForwardPolicyBatchLogProbs` получают optional extent/default padded и используют один этот helper. Сначала выбираются оба согласованных tensor, потом вызывается прежний `Forward` или `ForwardPolicyLogProbs`. В `StepVectorCore` передать extent во все четыре существующих вызова, а не только в reference или только chosen. Остальные `Step`, `StepMany`, read-only score остаются current пока отдельное изменение этих callers не входит в выбранный фактор. Это design-точки, а не заявление, что вся 4-тысячестрочная реализация переписана или собрана.

Точный математический мотив — causal prefix не зависит от будущего padding. Численно одинаковый FP32 результат не гарантируется: форма может переключить ветку ядра и порядок reductions. Поэтому dropout0, одинаковый backend/config и заранее выбранные tolerances обязательны для отдельного вывода об эквивалентности. B не включает response-only lm_head из новой N-HEAD-01: это другой фактор.

### 2. `14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs`

В `ValidOptions` [223–230][valid] добавить только два непересекающихся режима `pad-baseline` / `pad-valid-length` для `CodeDocumentation`, точного SHA ниже, `ExpectedPairCount=2`, `BatchSize=1`, `MaximumUpdates=2`, один проход. Все текущие calibration/main/ENRU predicates сохранить с прежними значениями. Общие guards RunId/hash/finite budget должны продолжать применяться ко всем веткам; добавление `or` не должно обходить их из-за precedence.

В строке 186 выбирать internal overload extent по явно проверенному mode. `CreateForProjectLora` остаётся **один раз на плечо**: optimizer/RNG не сбрасываются перед вторым уроком. `LoadModels`, materializer, recipient, capability, transaction, private publication и full readback остаются штатными. Marker уже включает mode+input SHA, поэтому различные A/B modes разделяют две заранее названные попытки; новые RunId не разрешают повтор использованного плеча.

Владелец получает права из прежней server-owned lease/composition. Не создавать новую сборку с именем `Rp01SimulationDpo` ради доступа к internals, не конструировать fake permit или подставлять `true` вместо нынешнего admission. Подключение относится к существующему trusted owner, не к новому trainer.

У current финального guard есть отдельный дефект наблюдения: completed native steps без различимой FP32-дельты LoRA помечаются общей ошибкой эффекта/readback [200–208][owner-loop]. Для корректного завершения наблюдения нужен отдельный reviewable split integrity/observation, описанный в соседнем аудите. Нулевая дельта не становится выигрышем, но не должна стирать реально измеренный update cost; целостность base/reference/readback/optimizer сохраняет жёсткие проверки. Сам этот split не объявляется обучающим A/B.

### 3. `tools/Rp01SimulationDpo/Program.cs`

Действующая CLI [122–130][cli] не имеет 2-pair PAD режима. Предлагаемый, ещё **не существующий** dispatch: `train-pad ROOT INPUT SHA RUN_ID baseline|valid-length none|SECONDS` (семь аргументов), который только формирует перечисленные строгие RunOptions и вызывает штатный owner. Сохраняется нынешний canonical root guard. Используется прежний `LoadModels` [315–332][loader], никаких новых весов, уменьшенного Student или cold fallback.

После реализации в пределах назначенной программы и подготовки проверенных входов будущая команда могла бы иметь следующий вид. **NOT_EXISTING_CLI: эти строки сейчас не исполнять.**

```text
dotnet run --project tools/Rp01SimulationDpo/Rp01SimulationDpo.csproj -c Release -- train-pad C:/Projects/llm_mvp_core C:/Projects/llm_mvp_core/training-artifacts/inputs/pad2-v1.jsonl 47f8fbc80dd9bdbb92f31c95dce6273385b5e236dfecfdb7aedcbb81b7a54af7 pad2-A-v1 baseline none
dotnet run --project tools/Rp01SimulationDpo/Rp01SimulationDpo.csproj -c Release -- train-pad C:/Projects/llm_mvp_core C:/Projects/llm_mvp_core/training-artifacts/inputs/pad2-v1.jsonl 47f8fbc80dd9bdbb92f31c95dce6273385b5e236dfecfdb7aedcbb81b7a54af7 pad2-B-v1 valid-length none
```

Проект target — net9.0. Наличие SDK не закрывает непроверенные физические B0/config/tokenizer/custody и ещё не реализованного dispatch. Это документационный проект команд, не проведённый preflight и не обещание запуска на нынешнем Linux scratch.

## Полный малый паспорт PAD2-review-2026-10-20-v1

**Предметная тема:** C#-документация, объяснение порядка двух аргументов при вычитании. **Student:** существующий `LlmModel rp01-csharp-small/v1`, saved root, четыре блока и текущая q/v LoRA topology; не исполнитель-LLM. **A/B:** два независимых запуска из одного подтверждённого полного B0 и одинакового начального состояния AdamW/RNG; два урока в порядке PAD1→PAD2. Ключевое изменение B только собственная валидная вычисляемая длина.

Current CLI содержит такие identity pins [15–17,315–332][loader]; это обнаруженные требования loader, **не подтверждённая доступность файлов**:

| Объект | Требуемое значение / статус |
|---|---|
| Config SHA256 | `e1f960b8c7d883f84c9b4e55d24c7284794cee2fecfd71beac5835d199746f40` |
| B0 weights SHA256 | `a65887cc9d97c793d00e77632013adeba5e2d36efd7e6cebe7e49ba8a9b37911` |
| B0 manifest SHA256 | `c5f4f7da80f427b8e9aac1dabded5849f0cbc6cb444b99d2bf7daac44ec0246d` |
| Saved root | Step1, ParentCheckpoint=null, InitialBaselineNoUpdate; 55 named tensors, 39 base и 16 adapter |
| Physical config/tokenizer/manifest/weights и custody | **PREPARABLE / NOT_VERIFIED_IN_THIS_SESSION**; пути берет настоящий verified manifest |
| Tokenizer SHA и actual BPE lengths PAD1/PAD2 | **PREPARABLE**; текущий23 сообщает tokenizer SHA `e51c91d85ec486863f1862e49b378fa9f34873ee31a993114ba41995f38e5066`, actual lengths необходимо получить штатным materializer |
| Initial AdamW/RNG portable bytes/hash | **PREPARABLE**; одинаковые проверенные состояние и настройки в обоих плечах |
| Native implementation/binary SHA/current executable command | **NOT_IMPLEMENTED / NOT_EXISTING_CLI** |
| Output | Proposed `training-artifacts/experiments/code-documentation-simulation/pad2-A-v1` и `pad2-B-v1`; сейчас не созданы |
| Поручение и готовность | Настоящее поручение — документационный аудит; техническая подготовка **PREPARABLE**, повторное личное подтверждение ID/SHA не требуется |

LR=`1e-5`, Beta=`0.1` — нынешние явные настройки owner; portable optimizer settings/step надо снять фактически, не угадывать остальные значения. Сам факт saved model Step1 не означает, что optimizer уже сделал одно учебное обновление: этот root отмечен InitialBaselineNoUpdate; current C# owner начинает новые global updates с 0. Точная начальная optimizer/RNG схема остается полем самостоятельной подготовки, а не выводится из имени файла.

### Точные тексты и байты

Две строки ниже — simulation, без внешнего teacher и без заимствованного автора. JSON compact; UTF-8 без BOM, LF после каждой строки, включая последнюю; никаких пустых строк внутри. 958 байт; SHA256 **`47f8fbc80dd9bdbb92f31c95dce6273385b5e236dfecfdb7aedcbb81b7a54af7`**. Digest рассчитан только для видимого документационного payload; dataset на runtime path не создавался.

```jsonl
{"id":"PAD1","dataset_version":"simulation-csharp-pad-2-v1","domain":"csharp","pair_kind":"CODE_TO_DOC","provenance":"simulation","prompt":"Explain in one short English sentence what the method returns. Describe argument order. Code: int Delta(int first,int second){return first-second;}","chosen":"Returns the first argument minus the second argument.","rejected":"Returns the second argument minus the first argument and then takes the absolute value of that result."}
{"id":"PAD2","dataset_version":"simulation-csharp-pad-2-v1","domain":"csharp","pair_kind":"CODE_TO_DOC","provenance":"simulation","prompt":"Explain in one short English sentence what the method returns. Describe argument order. Code: int Delta(int first,int second){return second-first;}","chosen":"Returns the second argument minus the first argument without taking the absolute value or changing the sign afterward.","rejected":"Returns the first argument minus the second argument."}
```

При подготовке запуска в пределах назначенной программы штатный materializer должен подтвердить lossless roundtrip каждого сегмента, неизменность tokenizer, capacity и хотя бы одну отличающуюся chosen/rejected token length. Если actual lengths равны, это **NO_CONTRAST** для этого набора. Скрыто удлинять ответ или менять tokenization после результата нельзя. Третья equal-length пара не входит в эти два урока.

### Реальное обучение и счёт

План всего A+B: **4 StepVector, 4 вызова optimizer**, по два на плечо, batch1. Внутри этих вызовов **16 model forwards: 8 policy + 8 reference; 8 policy backward**. Вложенные числа не суммируются с optimizer как дополнительные уроки. **Generate=0, внешних evaluator/teacher calls=0, дополнительного frozen score panel=0.** Accuracy собственного ответа — **NOT_MEASURED**, не 0 и не unchanged. Промежуточный checkpoint после урока1 нужен для двух действительных обновлений; readback не новый урок. A0/A1/B0/B1 фиксируются независимо, исходные весовые SHA сравниваются.

Measured before/after для learning state: именованные LoRA, portable AdamW m/v/step, RNG и текущие native DPO metrics после каждого урока. Наличие ненулевого веса не обязательный критерий успешного исполнения; native optimizer должен действительно быть вызван на этих валидных уроках, а его нулевой эффект сохранён в отчёте.

### Что и как измерять

| Показатель | Что именно сравнить | Ограничение вывода |
|---|---|---|
| Цена настоящего обучения | Время StepVector/полного training_step_total по двум шагам, CPU, phase timing; загрузка/JIT/save/readback отдельно | Стоимость B включает prefix-copy и добавленные проверки. Вложенные стадии не складывать дважды. |
| RAM | Пик процесса A и B в отдельных одинаково настроенных свежих процессах; loaded model count и наблюдаемый baseline рядом | Общий cumulative peak после A не становится пиком B. Два урока могут не различить эффект с данным разрешением. |
| Состояние | Численные ΔLoRA и m/v/step после каждого optimizer; frozen base/reference, same live/readback | State update подтверждает достигнутый learner, не правильность свободного ответа. |
| Objective | Текущие exact valid-target loss, chosen/rejected reward и margin в одних и тех же native шагах | Не подменять chosen likelihood только margin; отрицательная/нулевая дельта допустима. |
| Численная эквивалентность | Плановый критерий для сопоставимых скаляров/тензорных элементов `abs(a-b) <= 1e-5 + 1e-4*max(abs(a),abs(b))`; counts/shape/IDs exact | Это заранее предложенная tolerance для будущего review, не измеренная гарантия. Её нарушение сообщает о различии и не стирает отдельный ресурсный выигрыш. |
| Ответы/обобщение | Не измеряются этим ресурсным бюджетом | D1–D4/Generate из04 — отдельное явно выбранное расширение в пределах программы, не скрытые вызовы этого опыта. |

Current `DpoStepVectorTiming` содержит только scalar phase time/counts; он **не** предоставляет valid logits или raw gradients. `ZeroGrad` очищает градиенты сразу после optimizer [step][step]. Поэтому обещание исходной N60/23 «сравнить gradient до ApplyOptimizer» пока **не подключено**. Предлагаемый узкий observer должен быть внутри native trainer до ApplyOptimizer, одинаковым в A/B, без способного мутировать callback, с ограниченным заранее форматом для именованных разрешённых LoRA. Один norm/hash не доказывает численную близость всех элементов; либо есть сопоставимые полные gradient observations/строгое поэлементное сравнение, либо отчет пишет **GRADIENT_PARITY_NOT_MEASURED**. Loss/updated-state наблюдения допустимы, но не объявляются выполнением отсутствующего raw-gradient наблюдения. Соответствующая версия паспорта должна явно фиксировать выбранный контракт до измерения; сейчас это ещё одно **PREPARABLE** поле.

Expected outcome — установить изменение цены **реальных** двух уроков при сохранении или изменении их численного действия. Actuals: elapsed/CPU/RAM/loss/gradient/weights/moments — **NOT_MEASURED**. Build/train/model calls этого аудита — **0**. Не строить график с придуманными нулями.

## Область применимости

Causal-padding механизм не использует синтаксическое дерево C# или естественный порядок слов как отдельную эвристику. Он потенциально применим и к текстовой документации, EN→RU или SQL на том же exact autoregressive learner, если carrier, tokenizer, dropout и прием материала идентичного типа. Но measured conclusion остаётся только за фактически проверенным C# профилем и долей padding. EN→RU bridge и SQL tiny caller нельзя объявить подключёнными без проверки. Длинный одинаковый chosen/rejected дает меньше contrast; маленькая доля tail может не дать различимого выигрыша.


## Модель исполнителя и инструкции в теле паспорта

Запрошенная модель будущего исполнителя/проверяющего — **gpt-6-astra, reasoning ultra**, Student указан отдельно выше. Текущий документ не запускает исполнителя. Применять перечисленные versioned skills; таблица фиксирует требования к будущему действию и не заявляет нового полного перечитывания всех skills. Прямые правила пользователя и актуальная execution-policy20.10 имеют приоритет над историческими Tier/Gate/approval ограничениями.

| Skill | SHA256 доступного полного файла | Применение |
|---|---|---|
| [architect-behavior-classifier-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/skills/architect-behavior-classifier-SKILL.md) | `a66bbed2b77defd80f06cf9d9446c866e62253ec51d6487e1a494c9115e7a4a2` | Назвать реального learner и область причинного вывода. |
| [phase-planning-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/skills/phase-planning-SKILL.md) | `d791484d3964f3553811c7964b4206e51a88ec3a2edeeec5618f7c6e5f94d4de` | Разделить текущий аудит, подготовку и фактическое исполнение; один изменяемый фактор. |
| [plan-review-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/skills/plan-review-SKILL.md) | `a1e50fa762a994796640c471d8419f51abaccb587c7e62e5896d826a399853a4` | Сверить current caller, исходные условия и наблюдаемый результат. |
| [code-examination-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/skills/code-examination-SKILL.md) | `d314e7af9d213c91f845fd4b5dcc523988c4d3f42c936edd13108eef7f8f94ae` | Читать original изменение и всех его callers до реализации; не подменять код интуицией. |
| [prompt-patch-authoring-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/skills/prompt-patch-authoring-SKILL.md) | `1fde99b237856cff0a314eb3967827b06a95faa08b0cea36ca2eb5a3c01b631b` | Одно узкое исправление и полный видимый исполнительский промпт. |
| [human-readable-architecture-audit-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/skills/human-readable-architecture-audit-SKILL.md) | `1ed20703e5312fd0a253c81a38f3819db0ad630839cfdddd0bc93c0b25f56622` | Объяснить механизм обычным языком; предметная тема отдельно от технического механизма. |
| [human-communication-GLOSSARY.md](https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/skills/human-communication-GLOSSARY.md) | `4e33a09e80032891589a238c22e873bcbb31c3082ddf34b99e9db5fcfc507c6f` | Не называть hash/optimizer доменом; ожидание и измерение различать. |

## Полный видимый промпт будущему исполнителю

> Работай сначала на точной экспериментальной копии исходного механизма по правилу a822: A — неизменённый алгоритм, B — одна правка в копии, общие зависимости и состояния одинаковы. Основной код и продуктовые тесты не меняй; оригинальные пути ниже — provenance и будущий адрес переноса. Сохрани исходные admission и mutation authority, не имитируй их. Покажи результаты и точный diff переноса; даже успех не разрешает автоматически менять основной код или запускать продуктовый цикл. Защищённый каталог остаётся read-only; runtime evidence размещай в разрешённом каталоге вне него. 
>
> В пределах порученной программы сам подготовь физические B0/config/tokenizer/optimizer/RNG identities, native dispatch и точный контракт наблюдения PAD2-review-2026-10-20-v1. Нового личного approval каждого ID/SHA не требуется; незаполненные доступные поля — PREPARABLE, конкретно недоступный вход после безуспешной попытки — NEEDS_INPUT. Нынешнее поручение состоит в аудите и Markdown, поэтому в этом проходе исполнения нет. Protected docs/plans/replanning-possible-diffs не меняй. Применяй навыки архитектуры, планирования, проверки плана/кода, prompt authoring и human communication из именованных исходников, учитывая прямые правила Любы выше исторических правил skills.
>
> Единственный learner — прежний saved LlmModel с установленными q/v LoRA, `CreateForProjectLora` и `StepVector` в существующем trusted owner. A padded; B после всей original admission и exact validation использует own-length prefixes **IDs и response mask**. Не меняй carrier/digest/targets/Objective/Beta/вес пары, base/reference/tokenizer, dropout0, topology, исходный AdamW/RNG, авторство simulation. Не заменяй saved root random/tiny Student. Native capability и полный readback сохрани. Новый mode/command не считай существующим до фактической реализации и source review.
>
> Используй только две точные JSONL строки с SHA 47f8fbc80dd9bdbb92f31c95dce6273385b5e236dfecfdb7aedcbb81b7a54af7, PAD1→PAD2, один проход batch1: два настоящих optimizer шага на плечо, четыре всего. Не сбрасывай trainer между уроками и не обходи consumed markers новым RunId. Generate/teacher/evaluator=0; ответная accuracy NOT_MEASURED. Нельзя заменить обучающий A/B frozen forward или добавить декоративный Train к отдельному scorer. Реальная связь с обучением доказана вызовами `StepVector→Forward→Backward→ApplyOptimizer→checkpoint/AdamW/RNG readback`; покажи фактические counters и четыре состояния.
>
> Любое измеренное полезное улучшение считается успехом по своему показателю и домену, включая время/CPU/RAM actual Train; рост точности для этого не обязателен. Другие изменения и ухудшения покажи рядом. Нулевой действительный эффект допустим; no contrast/noise честно обозначь. Материальная ошибка/admission failure не становится успехом, а законченное обучение с нулевой FP32-дельтой не становится corruption. Для equivalence используй заранее принятые tolerances и выбранный реальный observer; не считай очищенный gradient или один hash доказательством его численной близости. Результаты отдельно от ожиданий, никакого повторения до выигрыша и расширения бюджета по ходу.

[active-pad]: https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/23-additions-n60-small-learning.md#L158-L170
[material]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/Models/DpoExactPairMaterial.cs#L17-L198
[owner-material]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L105-L140
[step]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/DpoTrainer.cs#L832-L930
[forwards]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/DpoTrainer.cs#L1007-L1029
[loss]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/DpoTrainer.cs#L1035-L1121
[owner-loop]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L175-L218
[friend]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/DPO.csproj#L18-L27
[recipient]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/DpoTrainer.cs#L189-L265
[valid]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L223-L230
[cli]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/tools/Rp01SimulationDpo/Program.cs#L122-L130
[loader]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/tools/Rp01SimulationDpo/Program.cs#L315-L332
