# 04. Малый путь настоящего DPO и недостающий учёт двух плеч

**Статус: PREPARABLE в будущей порученной программе; здесь выполнены чтение и проектирование.** Это техническое дополнение к [существующей D-карточке](https://github.com/lubochka/llm_mvp_core/blob/a8222d1b15745fe6207f36cd294fd3fb61c01d3b/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/04-documentation-neural.md), а не уже реализованная CLI-команда. Подготовку host, чтение доступных исходных артефактов и фиксацию параметров делает исполнитель; незаполненное поле не является новым требованием личного разрешения.

## Место будущего опыта и граница переноса

Актуальное [правило экспериментальной копии](https://github.com/lubochka/llm_mvp_core/blob/a8222d1b15745fe6207f36cd294fd3fb61c01d3b/docs/plans/replanning-possible-diffs/experimets/cards/P01-trainable-recipient.md#L3-L19) требует сначала проверить A/B в подходящем существующем `34.Experiments` или `35.ExperimentsV2`; тонкий runner — `experiments-v2/RP01-MICRO/<ID>/`. Копируются только необходимые исходники механизма. A сохраняет исходную реализацию; B отличается одним фактором. До исполнения фиксируются исходный commit, пути/методы, hashes точных копий, общие зависимости и A/B diff. Учится настоящий скопированный learner; ручная имитация ожидаемого ответа или подмена вычислений недопустимы.

Основной код и продуктовые тесты остаются только источником. Перечисленные ниже оригинальные методы — происхождение кода и будущие адреса переноса после результатов и отдельного решения Любы. Для опыта собирается только экспериментальный проект; полный CI, Main500, другой S-слайс и автоматический commit/push не входят в него. В данном проходе создаются только документы. Запрет пользователя менять защищённую core-папку сохраняется; наши отчёты размещаются в разрешённом planning-каталоге.

## Что уже есть

`SimulationDpoRunOptions` содержит ExpectedPairCount, BatchSize, MaximumUpdates и LoadModels; `Run` использует существующий exact materializer, server-owned admission, project-LoRA recipient, StepVector и full checkpoint/readback. [Контракт и Run](https://github.com/lubochka/llm_mvp_core/blob/de4ef0f14172dada4daf142a4c66ba19e96bb6d4/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L25-L28), [настоящий update](https://github.com/lubochka/llm_mvp_core/blob/de4ef0f14172dada4daf142a4c66ba19e96bb6d4/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L159-L205).

Сейчас `ValidOptions` допускает **calibration=1** либо **main=500**. Штатный CLI дополнительно фиксирует hash каждого корпуса; для EN→RU фиксированы continuation C50, corpus500, batch10 и 50 updates. `Inspect(..., expectedCount:2)` допускает такой размер для C#, но Inspect не даёт запустить Run. Нельзя раздувать два урока до 500 или вызывать calibration с постоянным reset. Точная экспериментальная копия исходного trainer допустима и требуется текущим правилом; самостоятельная упрощённая переимплементация вместо него не отвечает этому опыту. [ValidOptions](https://github.com/lubochka/llm_mvp_core/blob/de4ef0f14172dada4daf142a4c66ba19e96bb6d4/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L224-L230), [CLI](https://github.com/lubochka/llm_mvp_core/blob/de4ef0f14172dada4daf142a4c66ba19e96bb6d4/tools/Rp01SimulationDpo/Program.cs#L116-L130).

## Уже выбранные учебные данные

Предметная тема — **документация C#, порядок аргументов вычитания**. Повторно используется материал D-карточки, не новый семантический benchmark. Обе стороны — `simulation`, не ответы внешнего Chinese teacher.

Точный общий prefix: `Explain in one short English sentence what the method returns. Describe argument order. Code: `, включая последний пробел. Он соединяется с кодом ровно один раз. Materializer получает prompt/chosen/rejected раздельными полями и сам устанавливает exact joint framing/target masks.

| ID | Код после prefix | Chosen | Rejected |
|---|---|---|---|
| D-T1 | `int Delta(int first,int second){return first-second;}` | `Returns the first argument minus the second argument.` | `Returns the second argument minus the first argument.` |
| D-T2 | `int Delta(int first,int second){return second-first;}` | `Returns the second argument minus the first argument.` | `Returns the first argument minus the second argument.` |

При будущей материализации — существующая JSONL schema: id, dataset_version, domain=`csharp`, pair_kind=`CODE_TO_DOC`, provenance=`simulation`, prompt, chosen, rejected; metadata только допустимого исходного формата. Байты, UTF-8/no hidden normalization, порядок и SHA фиксируются до update. В этом документном проходе runtime-файлы корпуса не создавались и native tokenizer не запускался.

**R1 и R2 используют только эти два настоящих урока.** Они не меняют тексты, objective, beta, нормировку, обучение обоих плеч или число шагов. Для ресурсных вопросов exam generation не нужен.

## Исходный Student и состояние

[Конфигурация](https://github.com/lubochka/llm_mvp_core/blob/de4ef0f14172dada4daf142a4c66ba19e96bb6d4/config/code-documentation/generation-v1.json): `TransformerModel.LlmModel`, `rp01-csharp-small/v1`; vocab629, context2048, hidden512,4 слоя,8 heads/2 KV heads,FFN1408; LoRA rank8,alpha16,dropout0,q_proj/v_proj. Base и frozen reference — независимые копии одного сохранённого B0. Это не модель исполнителя.

В [CLI LoadModels](https://github.com/lubochka/llm_mvp_core/blob/de4ef0f14172dada4daf142a4c66ba19e96bb6d4/tools/Rp01SimulationDpo/Program.cs#L315-L332) уже заданы источники и pins: B0 weights `a65887cc9d97c793d00e77632013adeba5e2d36efd7e6cebe7e49ba8a9b37911`, manifest `c5f4f7da80f427b8e9aac1dabded5849f0cbc6cb444b99d2bf7daac44ec0246d`; config file hash `e1f960b8c7d883f84c9b4e55d24c7284794cee2fecfd71beac5835d199746f40`. Здесь это **проверенные константы исходника**, а не новое чтение файлов весов на машине пользователя.

Исполнитель сохраняет этот loader в точной экспериментальной копии и фиксирует фактические identities. В обоих плечах одинаковы initial weights, tokenizer, reference, optimizer settings/m/v/step и RNG snapshot. Начальный seed не заменяет снимок текущего курсора RNG. Не менять random base на иной checkpoint или уменьшенную модель внутри R1/R2: это был бы другой фактор. Если выбран новый базовый профиль, он получает отдельное сравнение и честное имя.

## N03: одной новой ветки count=2 недостаточно

**Независимо выявленная деталь подготовки A/B; к моменту публикации тот же вопрос уже описан в пакете61.** [Сверка и авторство](10-concurrent-planning-update.md). В `Run` строка 86 создаёт marker по формуле `experimentRoot / (domain-prefix + Mode + InputSha256 + .issued.json)`. RunId используется для каталога результата, но не этого marker. Строка 87 отказывает при его наличии. `WriteNew` marker выполняется до первого optimizer effect. [Исходник](https://github.com/lubochka/llm_mvp_core/blob/de4ef0f14172dada4daf142a4c66ba19e96bb6d4/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L80-L88), [durable issuance](https://github.com/lubochka/llm_mvp_core/blob/de4ef0f14172dada4daf142a4c66ba19e96bb6d4/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L169-L186).

Следствие из кода: если будущий host только добавит `Mode=micro-paired` и размер 2, оба плеча A/B на одинаковых bytes получат один marker; второе будет отвергнуто. Это правильная защита нынешнего once-only режима, но неполный проект **нового** двухплечевого режима. Это не измеренная причина прежнего качества RP01 и не самостоятельный эксперимент.

Конкретное дополнение host:

1. В экспериментальной копии необходимых `Run`/`ValidOptions` и host-интеграции добавить отдельный конечный C# профиль:2 пары,batch1,2 update на плечо,ровно два плеча. Существующий trusted `tools/Rp01SimulationDpo/Program.cs` читается как источник loader/dispatch; оригинал не редактируется. Для internal API исполнитель заранее фиксирует необходимую скопированную границу сборки и тот же контракт admission; public обход guards не допускается. `micro-paired` здесь — предлагаемое имя, не готовая команда.
2. До любого эффекта связать два заранее объявленных arm IDs с одним experiment ID, исходными corpus/base/reference/tokenizer pins, выбранным одним factor/version и общим бюджетом 4 updates. Оба плеча начинают независимые одинаковые состояния.
3. Учёт once-per-arm должен отличать **эти два заранее предусмотренных применения**, сохраняя запрет повторного запуска того же плеча после неизвестного эффекта. Marker создаётся до update, create-new/custody/journal проверки остаются.
4. Сохранить прежний смысл calibration/main/ENRU markers. Новый RunId, переименование lesson ID, изменение текста, новый произвольный manifest после ошибки или удаление marker не разрешают повтор.
5. Сохранить в экспериментальной копии существующие lease/admission/rollback/checkpoint-publication и exact mask/tokenizer проверки. Не вводить публичный mutation API и не ослаблять происхождение данных ради маленького размера.

Это ограниченная задача подготовки обычного исполнителя внутри разрешённой программы, а не повод вернуть общий `APPROVAL_PENDING`.

## Бюджет и наблюдения R1/R2

Для **одной** выбранной карточки: два isolated arms, в каждом один trainer и D-T1→D-T2; **4 StepVector/optimizer updates суммарно**,4 presentations при 2 уникальных исходных уроках, без повторов внутри плеча. Каждый StepVector на одной паре имеет по исходнику 2 reference и 2 policy forwards;16 forwards для 4 завершённых шагов — **структурный расчёт**, пока не измеренный runtime counter. Нельзя выдавать количество уроков, подопераций и checkpoints за независимые эксперименты.

Состояния A0/A1/B0/B1 фиксируются как минимум до первого и после второго обновления. Сохраняются завершённые/незавершённые действия, optimizer/RNG, параметры, exact material identity и full readback. Для R1/R2: **Generate=0**, teacher=0, external judge=0, дополнительных pair-score вызовов нет; внутренние training log-probs остаются частью StepVector. Точность свободного ответа — **не измерена**, а не 0.

**Граница учебных метрик:** StepVector вычисляет возвращаемые loss/margin до ApplyOptimizer данного шага. Они подписываются как pre-update logs соответствующего урока, а не как оценка окончательного A1/B1. В R1/R2 дополнительных Evaluate/EvaluateMany нет; финальные loss/margin A1/B1 на отдельной панели — NOT_MEASURED. Реальные состояния весов/optimizer/RNG до и после остаются обязательными. Ресурсный эффект определяется стоимостью одинаковой завершённой учебной работы. [Порядок вычислений](https://github.com/lubochka/llm_mvp_core/blob/de4ef0f14172dada4daf142a4c66ba19e96bb6d4/11.DPO/DpoTrainer.cs#L890-L925).

Снимать одинаковым способом длительность полного StepVector, CPU delta, allocations delta, GC counts, сохранённые bytes, процессный peak RAM с областью измерения. Загрузка, сохранение и generation не прячутся под названием optimizer kernel. Не снимать A/B в одном накопительном peak-счётчике без разделения процессов. Небольшое число операций не гарантирует короткое время: backend всё ещё может считать context2048. Не увеличивать число уроков после просмотра результата ради желаемого знака.

## Если нужен именно семантический D-опыт

Это отдельный выбранный профиль с собственным фактором, а не скрытое расширение R1/R2. Четыре новых задания D1–D4 из исходной карточки: `Gap(a,b){return a-b;}`, `Gap(a,b){return b-a;}`, `Diff(left,right)=>left-right`, `Diff(left,right)=>right-left`; полные сигнатуры и gold — в [D-карточке](https://github.com/lubochka/llm_mvp_core/blob/a8222d1b15745fe6207f36cd294fd3fb61c01d3b/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/04-documentation-neural.md#L112-L139). Они проверяют близкий перенос на новые имена/форму метода, не широкое обобщение на C#.

Фактический consumer: `CodeDocumentationSimulationDpoExperiment.Evaluate → BindVerified → CodeDocumentationStudentGeneration.Generate`. Для C# baseline null CandidatePath означает saved B0; ENRU так выбирать B0 нельзя. Candidate должен пройти hash/full-schema/custody проверки. [Evaluate](https://github.com/lubochka/llm_mvp_core/blob/de4ef0f14172dada4daf142a4c66ba19e96bb6d4/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoEvaluation.cs#L24-L70).

В D-карточке settings seed17/MaxAnswerChars160. Штатная CLI evaluate передаёт seed20261014/MaxAnswerChars4096, поэтому её нельзя назвать готовой командой с настройками карточки. Публичный Evaluate принимает Settings, и экспериментальный host должен передать заявленные значения явно, не меняя исходную CLI. `MaxAnswerChars` не подтверждён как фактически реализованный clipping;160 не обещается как действительный cap. [CLI evaluate](https://github.com/lubochka/llm_mvp_core/blob/de4ef0f14172dada4daf142a4c66ba19e96bb6d4/tools/Rp01SimulationDpo/Program.cs#L103-L113), [Generate](https://github.com/lubochka/llm_mvp_core/blob/de4ef0f14172dada4daf142a4c66ba19e96bb6d4/14.ContinuousLearning/Training/CodeDocumentationStudentGeneration.cs).

Четыре Evaluate для A0/A1/B0/B1 по 4 rows =16 генераций и 32 side-score computations. Это не бюджет resource-карточек. Если MaxNewTokens=0, generation не выполняется; нельзя писать, что ответы «остались прежними», когда они не измерялись.

**Правило:** любое измеренное полезное улучшение — успех по своему показателю и домену; точность не обязана расти. Остальные изменения и ухудшения рядом. Для dependent restore/reader можно использовать один доказанно обученный checkpoint без нового Train; текущие R1/R2, напротив, требуют настоящих updates, поскольку меняют их вычисление.
