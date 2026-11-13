# Нейронный путь: текущее состояние и новая гипотеза о стоимости выходной проекции

**Уточнение исполнения по a822 (20.10):** ссылки на оригинальные файлы ниже обозначают происхождение и адрес предлагаемого последующего переноса. Сначала проверяется точная экспериментальная копия A/B в существующем проекте `34.Experiments` или `35.ExperimentsV2`, с малым host в `experiments-v2/RP01-MICRO/<ID>/`. Оригинальный продукт и его тесты остаются read-only. Копия сохраняет настоящий алгоритм, зависимости, admission и mutation capabilities; их обход, поддельный lease или суррогатный learner недопустимы. До исполнения фиксируются copied-file hashes, A/B diff и actual binding вызовов. Перенос в основной код — отдельное решение после результата. Текущий запрет пользователя менять protected распространяется и на results: для возможного запуска выбирается разрешённый каталог вне protected. Подробнее: [финальная дельта](12-CONCURRENT-UPDATES-AND-NOVELTY.md).

Дата: 20.10.2026. Проверяемый core: `bf9b543e149ef197da85a767821d98c9b67916d2`. Снимок planning для дедупликации: `1ad622fe1f6d1e8b0860265d8c208aeada3fefb0`.

**READ_ONLY_RESEARCH.** Ни исходные алгоритмы, ни защищённая папка `docs/plans/replanning-possible-diffs`, ни веса не изменялись. Ни обучение, ни проверка ответа Student не запускались. Ниже доказанные свойства текущего кода отделены от гипотезы полезного эффекта.


## Актуальность и готовность после обновления 20.10

Текущие защищённые карточки перечитаны на `f5dd532045716495a2c7668573250e5d7cc46810`; новый полный23 и execution-policy проверены отдельно. Сравнение с исходным `bf9b543e149ef197da85a767821d98c9b67916d2` содержит 152 изменения только защищённых документов, без изменения рассматриваемого кода. Поэтому immutable code-ссылки ниже сохраняют исходный pin; ссылки на действующие правила/карточки обновлены.

[Правило20.10](https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/00-execution-policy-2026-10-20.md) отменяет общий approval-gate и обязательное новое личное подтверждение каждого ID/SHA. Подготовить доступные данные, уточнить native host и записать identities — работа исполнителя в пределах порученной программы. Эти проекты **PREPARABLE**, а не blanket BLOCKED; конкретный внешний blocker допустим только после документированной безуспешной попытки получить необходимый артефакт. `NOT_RUN` — отсутствие измерений. В нынешнем поручении результат — доказательный аудит и Markdown, поэтому здесь не выполнялись реализация/сборка/обучение; дополнительный approval-flow для завершения документов не требуется.

Проверки загрузки, восстановления и реального потребителя доказанно обученного состояния допустимы без повторного декоративного Train при точной цепочке зависимости и предложении исправления original code. PAD/HEAD — более узкий случай стоимости новых actual updates; их собственный A/B действительно обучает обе стороны.

## Что уже исправлено и что нельзя снова объявлять блокировкой

| Предмет | Проверенное текущее поведение | Вывод для очереди |
|---|---|---|
| Дробные маски и знак DPO | В `BackwardLogProbs` есть `targetScale = scale * mask.Data[i + 1]`; градиент `(oneHot − softmax) × targetScale`, затем `logits.Backward`. [Код][dpo-backward] | Старое N38 о потере дробного множителя уже не является дефектом этой версии. Нельзя тратить новый опыт на прежнюю реализацию и приписывать результат current. |
| Устойчивая exact objective | `ExactObjective` использует ветвление по знаку margin, `exp(-abs(margin))` и компенсированное `LogOnePlusPositive`; отдельно сохраняется legacy objective. [Код][objective] | «DPO вообще не делает backward» и «всё считается старым log-sigmoid» — неверные описания current exact-пути. Качество обучения из этого не следует. |
| Старт SFT с warmup | Публичный `Trainer.TrainSupervisedNextTokenStep` действительно отвергает LR=0 на `_supervisedStep=0`. Но реальный `IncrementalTrainingSessionBuilder.Build` передаёт `trainingConfig with { WarmupSteps = 0 }`. [Trainer][sft]; [production caller][session] | Это известная условная граница API, а не доказанная текущая блокировка штатного incremental learner. Не исправлять её ради нового отчётного PASS. |
| Получатель SFT | `ProductionSupervisedTrainingStepExecutor.Execute` вызывает именно `session.Trainer.TrainSupervisedNextTokenStep(session.Model, session.Optimizer, ...)`. [Код][sft-caller] | Это реальное нейронное обучение, отличный от DPO путь. Любая его маска/state/порядок стадий должна измеряться отдельно от RP01 DPO. |
| Восстановление SFT optimizer | Session builder создаёт `new AdamW` при новой session; current N60 `SC-SFT-STATE` уже отделяет reset/carry moments при фиксированном LR. [Код][session]; [N60][n60-state] | Это prior art; новый номер гипотезы не нужен. |
| Матрицы LoRA | `LoraLinear.Forward` использует собственные A/B, dropout входа и `alpha/r`; B инициализируется нулями. [Код][lora] | Изменение A/B — наблюдение параметров; полезный результат и обычный получатель проверяются отдельно. |
| Малый exact DPO caller | `ValidOptions` текущего owner принимает calibration1 либо main500; CLI также закрепляет старые hashes/counts. [Owner][valid-options]; [CLI][cli] | Готовой команды для двух новых пар нет. Строка «запустите micro-paired» без реализации owner/CLI была бы выдуманной командой. |

## N-HEAD-01 — вычисление выходной проекции для позиций, не участвующих в DPO objective

**Статус: NEW_WITHIN_REVIEWED_CORPUS / SOURCE_CONFIRMED_COST_PATH / EFFECT_NOT_MEASURED.** Это гипотеза о расходе ресурсов настоящего обучения. Причиной нулевой точности RP01 она не объявляется.

Current выполняет полный Transformer, затем `_lmHead.Forward(x)` для каждого timestep. Результат имеет форму `[1, T, VocabSize]`. В `ComputeTargetLogProbs` только после этого читается `mask[i+1]`; при нуле соответствующий logit-ряд пропускается. Для exact carrier маска выбирает response targets. Обратный проход также записывает ненулевой входной градиент только в выбранные ряды logits. [LlmModel.Forward][lm-forward]; [вычисление DPO][dpo-forward]; [backward][dpo-backward]; [exact mask][exact].

Это не означает, что prefix-контекст лишний: все скрытые состояния prompt продолжают участвовать в attention и в градиентах response. Лишняя работа в этой гипотезе — именно полная проекция из hidden dimension в весь словарь на prompt-строках, которые objective затем не использует.

### Доказанная цепочка участия в обучении

1. `CodeDocumentationSimulationDpoExperiment.Run` передаёт приготовленные exact-пары в `DpoTrainer.StepVector`.
2. `StepVectorCore` вызывает два reference-forward и два policy-forward на каждую пару.
3. `LlmModel.Forward` выполняет последний `_lmHead` для всех `T` строк.
4. `ComputeTargetLogProbs` и `BackwardLogProbs` отбрасывают masked targets, но уже понесённая стоимость проекции не возвращается.
5. `ApplyOptimizer` меняет разрешённые q/v LoRA; параметры, AdamW и RNG сохраняются штатной транзакцией.

Таким образом, предлагаемый измеритель относится к реальным forward/backward внутри optimizer update, а не к отдельному serving benchmark. [Owner update][owner-update]; [DPO step][dpo-step].

### Одно предлагаемое вмешательство

A — нынешняя полная выходная проекция. B — **полный тот же Transformer prefix**, затем дифференцируемый выбор hidden rows `i`, для которых `responseMask[i+1] > 0`, и проекция `_lmHead` только этих рядов. Выбранный row всегда связан с исходным target `ids[i+1]`; первая response-цель, последняя цель и коэффициенты маски сохраняются.

Пары, полная длина Transformer, attention, RoPE, tokenizer, params, reference, LoRA targets/rank/alpha/dropout, optimizer, prompt и response masks остаются теми же. В этот опыт **не включается** PAD-обрезка: иначе невозможно выделить вклад выходной проекции.

Для одного forward математическая стоимость dense head пропорциональна `T × H × V`, предлагаемая — `R × H × V`, где `R` — число scored targets. Это расчёт объёма операции, не измеренное ускорение. Время валидации, attention, копирования выбранных строк, сохранения и прочие затраты остаются; поэтому реальный эффект может быть небольшим или отрицательным. Память только для logits меняется с `T × V` на `R × V`; общая пиковая память процесса из этого не выводится.

### Что конкретно надо менять в оригинале

| Original owner | Предлагаемая работа | Почему необходима |
|---|---|---|
| `03.TransformerModel/LlmModel.cs:219–265` | Добавить узкий forward для selected output rows, использующий тот же `ForwardHidden(..., training)` и тот же `_lmHead`; обычный `Forward` сохранить. | Нельзя создавать другой Student, новый encoder или урезать контекст prompt. |
| `00.TensorCore/Core/TensorOps.cs` | Добавить дифференцируемый row-select с gradient scatter-add по исходным индексам; фиксировать shape и index bounds. | Существующий `Gather` возвращает новый Tensor без gradient edge. Его прямое применение оборвёт обучение LoRA. Это **риск предложенной реализации**, не утверждение, что current DPO уже использует такой Gather. [Код][gather] |
| `11.DPO/DpoTrainer.cs:1007–1120` | Передать явное отображение selected row → original target; вычислять ту же exact log-prob sum и backward на выбранных logits; оставить `CaptureBatch`/admission/recipient/optimizer/publication. | Нельзя использовать нынешний цикл `i < t-1` без адаптации после уменьшения `t`: иначе потеряется последняя цель. |
| `14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs` и `tools/Rp01SimulationDpo/Program.cs` | Узкий двухпарный dispatch с фиксированными source/config/B0 и явной ветвью A/B; record actual row counts и mode. | При нынешних 1/500 guards такого исследовательского запуска нет. Не ослаблять существующие режимы. |

Вещественная арифметика математически эквивалентна при корректном scatter градиента: masked output rows имели нулевой upstream. Побитовая эквивалентность из этого не гарантируется из-за порядка floating-point reductions. Её нельзя утверждать без actual paired learning.

### Малый доменный паспорт N-HEAD-01

Тема — C#-документация; Student — **тот же saved `rp01-csharp-small/v1`**, vocab629, hidden512, 4 слоя, q/v LoRA rank8/alpha16/dropout0, указан в действующей карточке D. Сохранённая B0 должна быть получена через существующий `LoadModels`, с независимой reference. Физические файлы весов в этом документном проходе не загружались и не проверялись; их поиск и верификация остаются подготовкой исполнителя, а не доказанной внешней блокировкой; не заменять B0 новой случайной моделью или уменьшенной архитектурой. [Паспорт D][d-card]; [saved-root loader][load].

Два simulation урока берутся дословно из N60 D-T1/D-T2:

| ID | Prompt после общего prefix | Chosen | Rejected |
|---|---|---|---|
| D-T1 | `int Delta(int first,int second){return first-second;}` | `Returns the first argument minus the second argument.` | `Returns the second argument minus the first argument.` |
| D-T2 | `int Delta(int first,int second){return second-first;}` | `Returns the second argument minus the first argument.` | `Returns the first argument minus the second argument.` |

Общий prefix: `Explain in one short English sentence what the method returns. Describe argument order. Code: `.

Один проход, batch1, два actual updates на плечо. Всего **4 StepVector / 4 optimizer updates**, 16 model forwards и 8 backwards; вложенные вызовы не складываются как новые уроки. Никаких дополнительных повторов ради положительного результата. Данные EXAM для необязательного вопроса качества — D1–D4 действующего N60, но основной ресурсный вариант содержит **0 Generate**; accuracy тогда **NOT_MEASURED**, а не unchanged/0.

Основной отчёт — A0/A1/B0/B1: identities начальных и конечных params/AdamW/RNG; actual scored rows и токены; per-step loss/rewards; время полного `StepVector`, отдельные forward/backward; CPU и peak RAM собственного процесса; выделенные bytes и сохранённый state size, если реально инструментированы. Два свежих процесса A/B нужны для раздельного process peak: пик накопительного A+B процесса нельзя выдавать за пиковую память плеча B. Runtime/JIT/build identity фиксируется до запуска; сборка и загрузка отдельно от обучения.

Numerical guard: до optimizer — одинаково снятые actual valid logits и градиенты разрешённых параметров; после — полные checkpoint tensors и portable optimizer. Tolerance и правила finite задаются в паспорте **до** запуска. Scalar hash не доказывает близость градиентов. Если наблюдателя до `ZeroGrad` нет, поле остаётся `NOT_INSTRUMENTED`, и numerical parity не объявляется доказанной. Zero parameter delta при actual completed updates — допустимый нулевой исход обучения, при сохранении всех integrity checks; это не основание для фиктивной метрики прироста.

**Правило успеха этой карточки:** любое фактически измеренное полезное улучшение считается успехом по этому показателю и теме. Рост accuracy не обязателен. Остальные изменения, включая ухудшения, показываются рядом. Неразличимый эффект, отсутствие изменения и ухудшение — допустимые наблюдаемые результаты. Ответы другого Unit, proxy-формула или отдельный forward без actual обучения не являются этим экспериментом.

**Промпт исполнителю:** «Работай сначала на точной экспериментальной копии исходного механизма по правилу a822: A — неизменённый алгоритм, B — одна правка в копии, общие зависимости и состояния одинаковы. Основной код и продуктовые тесты не меняй; оригинальные пути ниже — provenance и будущий адрес переноса. Сохрани исходные admission и mutation authority, не имитируй их. Покажи результаты и точный diff переноса; даже успех не разрешает автоматически менять основной код или запускать продуктовый цикл. Защищённый каталог остаётся read-only; runtime evidence размещай в разрешённом каталоге вне него. В назначенной программе самостоятельно подготовьте B0/input/runtime/observer/output identity и зафиксируйте выбранную версию; новый личный approval каждого SHA не требуется. В текущем документном поручении результатом остается этот паспорт; для реализации и исполнения следуйте фактическому объему порученной программы. Реализуйте только selected LM-head rows внутри настоящего exact DPO. Сохраните полный Transformer prefix, dropout0, исходные пары/маски/objective, server admission и transaction guards. Обучите A и B по двум указанным урокам одним проходом; измерьте фактическую стоимость и численный эффект тех же updates. Любой измеренный полезный выигрыш — успех по его метрике и теме без обязательного роста accuracy; потери рядом. Без Generate accuracy не измерена. Не добавляйте PAD, другой Student, SFT, новый tokenizer, искусственные лишние уроки или standalone operator-test как замену обучению. Доступные незаполненные runtime поля подготовьте сами в разрешённой программе; readiness PREPARABLE не требует нового личного SHA-approval. Только фактически недоступный обязательный вход после конкретной попытки отмечайте NEEDS_INPUT; не выдумывайте команду или checkpoint».


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

### Где применяется, где не следует переносить

| Тема/путь | Применимость |
|---|---|
| C# → краткое объяснение | Прямое достижение указанного exact DPO learner; потенциал экономии выше при длинном code prompt и коротком ответе. |
| SQL/оркестрация с neural exact carrier | Отдельный будущий профиль после подтверждения фактического того же learner и exact masks. Wiki hashed metric DPO нельзя молча заменить этим Student. |
| EN→RU | Условно для совместимого exact neural learner; экономия зависит от отношения длины prompt/response. Не распространять рецепт на word-hash bridge или TranslationTrainedUnits. |
| kNN/память Unit без LlmModel | Не применяется: LM-head там не участвует в операции обучения. |

### Дедупликация и предел новизны

Просмотрены текстовые совпадения по всем **555 Markdown** current possibleDifs. Именованные ближайшие работы — q/v-vs-lm_head (смена обучаемых параметров), DPO masks (смена objective/targets), ND-DPO-PAD (срез trailing padding). Они не описывают selected output projection при неизменном полном Transformer. Три широких поисковых совпадения относятся к этим известным соседним темам; повтор такой же гипотезы не найден. Это граница новизны по зафиксированному корпусу и поиску, а не заявление, что алгоритм неизвестен мировой литературе или что все RAG документы прочитаны полностью.

52 материализованных файла TensorCore/Transformer/TrainingLoop/LoRA/DPO сверены по Git blob SHA с current деревьями. Адресное смысловое чтение использовано для вывода, полного перепрочтения всего 160-КБ transaction-класса не заявляем. Реестр hashes и поисковые receipts находятся в рабочих материалах аудита.

[dpo-backward]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/DpoTrainer.cs#L1072-L1121
[objective]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/DpoTrainer.cs#L1131-L1151
[sft]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/04.TrainingLoop/Trainer.cs#L102-L145
[session]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/14.ContinuousLearning/Training/IncrementalTrainingSession.cs#L403-L419
[sft-caller]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/14.ContinuousLearning/Training/SupervisedTrainingExecution.cs#L19-L31
[n60-state]: https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/23-additions-n60-small-learning.md#L148-L156
[lora]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/10.LoRA/LoraLinear.cs#L107-L125
[valid-options]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L223-L230
[cli]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/tools/Rp01SimulationDpo/Program.cs#L116-L134
[lm-forward]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/03.TransformerModel/LlmModel.cs#L219-L265
[dpo-forward]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/DpoTrainer.cs#L1007-L1070
[exact]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/Models/DpoExactPairMaterial.cs#L29-L48
[owner-update]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L175-L186
[dpo-step]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/DpoTrainer.cs#L832-L943
[gather]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/00.TensorCore/Core/TensorOps.cs#L1477-L1492
[d-card]: https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/04-documentation-neural.md
[load]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/tools/Rp01SimulationDpo/Program.cs#L315-L336


## Уточнение независимого обзора: граница индексов и связность градиента

При `p = PromptLength`, `L = sideLength` первые scored targets находятся в позициях `p…L−1`; соответствующие hidden rows — `p−1…L−2`. Первая выбранная hidden row является последней позицией prompt и предсказывает первый токен ответа. Общее правило для взвешенной маски остаётся `mask[i+1] > 0`, с сохранением исходного множителя маски.

Наличие differentiable row-select/scatter-add и autograd edge к исходному hidden state проверяется по реализации отдельно от численной погрешности. Одно `atol=1e−5` может скрыть исчезновение маленького градиента и не доказывает связность графа. Потерянный edge — дефект реализации, а не допустимый «нулевой полезный эффект». Это уточнение не добавляет отдельного runtime опыта или требования роста accuracy. Независимый обзор: [NEURAL-REVIEW.md](reviews/NEURAL-REVIEW.md).

Индексы выбранных строк должны быть неизменяемыми на время forward/backward. Реализация сохраняет `RequiresGrad`, связь с исходным Tensor и backward-функцию; обратный перенос накапливает градиент (scatter-add), а не заменяет предыдущий вклад. Эти условия относятся к будущей экспериментальной копии; продуктовый Gather здесь не изменялся.
