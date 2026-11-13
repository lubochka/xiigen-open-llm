# Поздняя адресная дельта core: 9622b575 → 67abfabe

> **Историческое дополнение на `67abfabe`.** После него вышел `ab583032`: LF-03 fixture/бюджет уже исправлены в N62, N-KU уже зарегистрирован внешним коммитом. Описанные ниже на `67abfabe` пробелы не следует переносить на текущий статус. См. [финальное дополнение](05-final-publication-delta.md). Документное размещение будущих LF-03 outputs вне protected остаётся применимым; runtime не запускался.

**Это дополнение изменяет текущий статус N62: прежнее утверждение «сборочная и capability-схема ещё не назначена» для R1/R2/E-OPT-STEP устарело. Новый текст задаёт конкретную схему, но не подтверждает её сборку, ABI, наличие B0 или выполнение опыта.** Также добавленный LF-03 пока не получил обещанной собственной fixture и согласованного отдельного бюджета. Основной [coverage.json](coverage.json) намеренно остаётся неизменным снимком `9622b575a6aabb79939b12cc9f5d96313f1bf370`; его строки нельзя молча читать как состояние нового HEAD.

## Область повторной проверки

Новый core SHA: `67abfabe6def8a002984be80b999824601ad273c`, один commit после старого, сообщение **“1st experiment results and conflict in plan fixes”**. [Сравнение двух закреплённых версий](https://github.com/lubochka/llm_mvp_core/compare/9622b575a6aabb79939b12cc9f5d96313f1bf370...67abfabe6def8a002984be80b999824601ad273c) показывает **147 изменённых файлов**, все внутри `docs/plans/replanning-possible-diffs`: 128 карточек и 19 файлов малых проб. Изменений production-кода и raw результатов в этом compare нет. Пять entries P95–P99 идут без текстового patch и с 0 additions/deletions; им не приписывается выдуманная содержательная дельта.

Адресно прочитаны новые N62-neural-update-costs, N61-dpo-reference-reuse, N61-mode1-retention-and-identity, N-KU-01, P01, common method, N60 additions и execution policy; для N62 дополнительно прочитан полный Git blob `848a10952fd92ed3d34b69ceebcf79c0e0c5e06d`. Это ограниченный delta-review, не повторный обход 662 planning-файлов или всех результатов. Машинный перечень изменённых путей: [late-compare-inventory.json](late-compare-inventory.json).

Ни protected-файлы в remote, ни production-код, ни результаты не менялись этим аудитом. Новых Train/Generate/build/test запусков нет.

## Что стало иначе

| Объект | Старый вывод на 9622b575 | Что действительно есть на 67abfabe | Текущий вывод |
|---|---|---|---|
| N62 R1/R2/E-OPT-STEP: source/capability closure | Не задан полный список зависимостей и связка скопированного trainer/owner | Три полные копии библиотек, manifest, явные DLL references, identity/friend requirements, bridge внутри ContinuousLearning, точный lifecycle и CLI | **Старое отсутствие спецификации исправлено в тексте.** Подготовка реализации/сборки ещё не выполнена |
| N62 build/runtime | Not run | Новый текст прямо говорит: DLL load/ABI, compile и B0 custody не проверены; host ещё не существует | Не объявлять READY-compiled или измеренный успех |
| N62 LF-03 | В этой карточке отсутствовал | Добавлена строка нового фактора, source/host и TensorCore boundary, один урок на arm | **Protected покрытие частичное:** fixture и бюджет в нём противоречат друг другу; точное additive разрешение с прежней fixture приведено ниже |
| N61 reference cache | Source-linked/capability closure открыта | Тело по-прежнему прямо говорит, что closure не определена | Старый остаток сохраняется, его дописывает автор карточки |
| N61 Mode1 / N62 training context | Program glue остаётся specification / product POS writer не доказан | Эти два файла не входят в изменённый список compare | Старые адресные выводы не отменены этой дельтой |
| P01/common headers | Executor сам выбирает/дополняет рецепт | Автор обязан заранее задать данные, host, dependencies, command, conflicts; executor только материализует | Правило усилено правильно; незаполненные паспорта от этого не заполнились |
| N-KU-01 | Успешный candidate с majority limitation и регрессией | Уточнено, как регистрировать candidate и сохранить лишние исторические операции отдельно | Выигрыш/регрессия и current product defect не изменились |
| Правило успеха | Любой измеренный полезный эффект — успех по метрике/домену | Сохранено; не добавлено требования роста accuracy | Выводы positive candidates не отменяются |

## N62: какие прежние замечания действительно закрыты

Новый рецепт больше не отправляет executor искать «минимальный граф». Он задаёт **101 исходный файл** в копиях TrainingLoop/DPO/ContinuousLearning, для LF-03 отдельно TensorCore; перечисляет .props/.csproj, выходные каталоги A/B, 27 неизменяемых DLL из одного binary boundary, запрещает подмешивать оригинальные DPO/TrainingLoop/ContinuousLearning и сохраняет настоящую identity/internal capability связь. Эти числа — содержимое новой спецификации; этот аудит не выполнял новый native dependency build. [Точный source/dependency contract](https://github.com/lubochka/llm_mvp_core/blob/67abfabe6def8a002984be80b999824601ad273c/docs/plans/replanning-possible-diffs/experimets/cards/N62-neural-update-costs.md#L91-L122).

Bridge помещён в **ContinuousLearning**, откуда доступны её настоящие internal issuers. Названы Inspect, canonical loader, native governance/content/material preparation, recipient/profile, реальные authority/lease/mutation capability, observer и CreateForProjectLora с non-null RNG callbacks. Задан finite paired lifecycle с issued/completed, state0 serialization, native readback и без дополнительных Generate/Evaluate. [Bridge и настоящий допуск](https://github.com/lubochka/llm_mvp_core/blob/67abfabe6def8a002984be80b999824601ad273c/docs/plans/replanning-possible-diffs/experimets/cards/N62-neural-update-costs.md#L124-L155); [Lifecycle](https://github.com/lubochka/llm_mvp_core/blob/67abfabe6def8a002984be80b999824601ad273c/docs/plans/replanning-possible-diffs/experimets/cards/N62-neural-update-costs.md#L159-L171).

Поэтому фразу из старого audit «N62 full source/dependency/capability graph не закрыт» теперь следует читать только как исторический статус. **Текущий статус R1/R2/E-OPT-STEP: SPECIFIED / NOT_MATERIALIZED / NOT_COMPILED / BINARY_INPUTS_UNVERIFIED.** Копии, проекты, данные и runner ещё должны быть материализованы в будущем разрешённом эксперименте; actual refusal/loader/build результат не выдумывается. Сам документ это признаёт. [Ограничение проверки ABI/B0](https://github.com/lubochka/llm_mvp_core/blob/67abfabe6def8a002984be80b999824601ad273c/docs/plans/replanning-possible-diffs/experimets/cards/N62-neural-update-costs.md#L118-L122); [Назначенные команды и NOT_BUILT](https://github.com/lubochka/llm_mvp_core/blob/67abfabe6def8a002984be80b999824601ad273c/docs/plans/replanning-possible-diffs/experimets/cards/N62-neural-update-costs.md#L173-L191).

## LF-03: новый остаток, который нельзя закрыть одним ID

Новая верхняя таблица определяет LF-03 как удаление внешней копии `initialGradient` при сохранении защитной TensorStorage-копии, **один урок на плечо / два StepVector всего**. Далее текст говорит, что собственная существующая однострочная fixture находится «ниже» и что D-T1/D-T2 к ней не добавляются. [Новый фактор и объявленный бюджет](https://github.com/lubochka/llm_mvp_core/blob/67abfabe6def8a002984be80b999824601ad273c/docs/plans/replanning-possible-diffs/experimets/cards/N62-neural-update-costs.md#L7-L13); [Обещание отдельного материала](https://github.com/lubochka/llm_mvp_core/blob/67abfabe6def8a002984be80b999824601ad273c/docs/plans/replanning-possible-diffs/experimets/cards/N62-neural-update-costs.md#L43-L47).

Но в полном новом файле нет самостоятельной секции LF-03 с этой fixture: после E-OPT-STEP идут общий бюджет, шаблон отчёта и источники. Общий бюджет по-прежнему сформулирован для **одного выбранного опыта** и задаёт два урока/два updates на arm, четыре updates суммарно; A1/B1 описаны после второго committed update. [Общий бюджет и state semantics](https://github.com/lubochka/llm_mvp_core/blob/67abfabe6def8a002984be80b999824601ad273c/docs/plans/replanning-possible-diffs/experimets/cards/N62-neural-update-costs.md#L250-L270).

Это конкретный конфликт документа, а не отрицательный результат LF-03. До runtime handoff **автор карточки** должен вписать exact fixture/ссылку на существующий источник, однофакторную A/B правку, два вызова суммарно и A1/B1 после первого committed update, а общий двухурочный бюджет явно ограничить R1/R2/E-OPT-STEP. Нельзя разрешить executor выбирать между одним и двумя уроками, сочинять fixture или превращать общий текст в выполненный опыт. Исправлять protected в этом задании запрещено; остаток зарегистрирован здесь, без дистанционного изменения.

### Готовое документное разрешение LF-03: существующая fixture и одношаговый бюджет

**Это дополнение к существующему LF-03, не новый эксперимент и не изменение protected-карточки.** Используется уже назначенный в N62 на `67abfabe` LF-03 host/source boundary, а материал и бюджет берутся из существующей [LF-03-секции planning:92–114](https://github.com/lubochka/xiigen-planning/blob/de270758087502a350c4fcf460acdf1638bdd318/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-signal-and-cost-audit/01-new-findings-and-small-experiments.md#L92-L114). Для LF-03 общая двухурочная таблица N62:250–266 **не применяется**; она относится к R1/R2/E-OPT-STEP.

**Единственный TRAIN-файл:** [исходная fixture `fixtures/csharp-one-pair.jsonl`](https://github.com/lubochka/xiigen-planning/blob/de270758087502a350c4fcf460acdf1638bdd318/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-signal-and-cost-audit/fixtures/csharp-one-pair.jsonl#L1), ID `simulation-csharp-doc-f06-008`, dataset `rp01-simulation-csharp-code-to-doc-500-v1`. Скопировать байт-в-байт в `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/LF-03/train.jsonl`. UTF-8 без BOM, одна JSONL-строка с завершающим LF, **1274 bytes**, SHA256 **`5b265faaa2046a49449de0f5a44423a28df65bcf79b2c5fabc55e7d6796ba1f6`**, проверенный Git blob SHA1 **`ef682eda6dd73cfce94c823db46561421957f0b1`**. Внутри JSON значение `prompt` содержит escape `\n`; не заменять его на отдельную физическую JSONL-строку. Полные bytes представлены ниже; дополнительный prompt-prefix из R1/R2 и D-T1/D-T2 не добавляются.

```json
{"id":"simulation-csharp-doc-f06-008","dataset_version":"rp01-simulation-csharp-code-to-doc-500-v1","domain":"csharp","pair_kind":"CODE_TO_DOC","provenance":"simulation","prompt":"C# 12 / .NET 8. Explain this code's observable behavior in one sentence.\nint x=10; int y=x++; System.Console.Write(y);","chosen":"It writes 10 because postfix increment assigns the old value to y.","rejected":"It writes 11 because postfix increment assigns the new value to y.","metadata":{"split":"TRAIN","family_id":"csharp-law-06-postfix_increment","semantic_template_id":"csharp-template-06-v1","instance_index":8,"language_version":"C# 12","framework_version":".NET 8","snippet":"int x=10; int y=x++; System.Console.Write(y);","law":"Postfix increment returns the old value.","expected_stdout":"10","rejected_stdout":"11","why_rejected_is_wrong":"Postfix increment returns the old value. The rejected explanation contradicts this distinction.","execution_contract":"Top-level C# statements; invariant observable integer/string/Boolean output; no inputs, threads, overflow except explicit unchecked byte conversion; no culture-sensitive fractional output.","derivation":"Authored semantic template expanded with declared instance parameters; simulation.","csharp_runtime_verified":false}}
```

Исходная запись прямо помечает `provenance=simulation` и `csharp_runtime_verified=false`; этот документ не меняет её происхождение и не утверждает новый C# runtime-test.

**Назначенные существующей N62-карточкой пути сохраняются:** source `C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/Rp01Micro/LF-03/`; host `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/LF-03/Runner.csproj`; независимые A/B варианты с дополнительной TensorCore-копией. Новый runner/dispatcher не изобретается. Для native `Inspect` expectedCount = **1**, конечный цикл имеет ровно одну итерацию в arm, native batch size 1.

**Один фактор:** A сохраняет `new Tensor(initialGradient.Data.ToArray(), initialGradient.Shape)`; B в экспериментальной копии `00.TensorCore/Core/Tensor.cs`, `Backward(Tensor)`, заменяет только эту строку на `new Tensor(initialGradient.Data, initialGradient.Shape)`. Защитное копирование внутри TensorStorage остаётся. Нельзя одновременно применять R2/LF-02, отдавать исходный alias напрямую в Grad или менять autograd/optimizer/admission. Это ровно прежнее [доказательство и A/B LF-03](https://github.com/lubochka/xiigen-planning/blob/de270758087502a350c4fcf460acdf1638bdd318/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-signal-and-cost-audit/01-new-findings-and-small-experiments.md#L96-L108).

| Операция LF-03 | A | B | Всего |
|---|---:|---:|---:|
| Уникальная общая TRAIN-пара / предъявления | 1 / 1 | 1 / 1 | 1 общая пара / 2 предъявления |
| CreateForProjectLora | 1 | 1 | 2 |
| StepVector / optimizer update | 1 / 1 | 1 / 1 | 2 / 2 |
| Policy forward / backward | 2 / 2 | 2 / 2 | 4 / 4 |
| Reference forward | 2 | 2 | 4 |
| Все native model forward | 4 | 4 | 8 |
| Generate / extra Evaluate / teacher / warmup / retry | 0 | 0 | 0 |

A0/B0 — одинаковые полные B0/R0/optimizer/RNG identities до единственного шага. **A1/B1 — после первого committed update и штатного readback**, а не после второго. Число forward/backward — структурный бюджет выбранной native пары; actual counts сохраняются. Загрузка, admission и штатная публикация/readback не скрываются из стоимости и не считаются дополнительными уроками.

Метрики — real update allocated bytes, backward/StepVector time, CPU/GC/peak RAM отдельно, LoRA и m/v/step/RNG до/после; достигнутый `Tensor.Backward(Tensor)` должен быть показан. Returned Step loss относится к pre-update forward. Final heldout objective и free-answer accuracy — **NOT_MEASURED**, поскольку Generate/Evaluate budget равен нулю. Любое измеренное полезное улучшение признаётся успехом своей метрики и C#-домена; численные различия и все остальные ухудшения остаются рядом. Теоретические `4M` bytes не выдаются за измеренный выигрыш.

**Точный текст дополнения к будущему executor prompt:** «LF-03 выполняется по уже назначенному N62 LF-03 host, с указанной однострочной fixture/hash и expectedCount=1. По одному StepVector/optimizer update в A и B, всего два, без D-T1/D-T2, общего prompt-prefix, второго шага или других cost факторов. A1/B1 снять после первого committed update. Общая двухурочная таблица N62 к LF-03 не применяется. Сохрани native learner/admission/publication и protective TensorStorage copy. Любой полезный измеренный выигрыш — успех по оси/домену без обязательной accuracy; регрессии рядом. Сейчас поручено только документирование, никаких запусков или protected-правок».

Таким образом, **data/budget-конфликт разрешён здесь конкретным источником и bytes**, а protected-текст остаётся неизменным. Его физическое отсутствие в protected не маскируется формулировкой «всё уже внесено». Ограничение защищённого output-root рассмотрено отдельно ниже; это дополнение не разрешает запись туда.


## N61 и общие паспорта: что осталось открытым

N61 reference reuse сохранил прямую фразу о неопределённом полном source-linked наборе и capability binding. Новая инструкция лишь точнее назначает эту работу **автору**, до передачи runtime-исполнителю. Поэтому N62 closure нельзя автоматически считать closure N61 cache. [Незакрытый N61 контракт](https://github.com/lubochka/llm_mvp_core/blob/67abfabe6def8a002984be80b999824601ad273c/docs/plans/replanning-possible-diffs/experimets/cards/N61-dpo-reference-reuse.md#L61-L67).

В P01 новый первый пункт снимает с executor выбор host/data/fix/conflicts и возлагает конкретный рецепт на автора. Это соответствует пользовательскому правилу. Однако ниже остаётся заголовок «паспорт — заполняет исполнитель», признание незаполненного caller/command и фраза «исполнитель сам заполняет доступные поля». Приоритет для научных/архитектурных решений имеет новый конкретный пункт: executor материализует уже названный рецепт, а **не выбирает отсутствующий design**. Текущая карточка всё ещё справочник вариантов до авторского выбора и законченного паспорта; общий header не превращает 125 каталогов в готовые команды. [Новая ответственность автора](https://github.com/lubochka/llm_mvp_core/blob/67abfabe6def8a002984be80b999824601ad273c/docs/plans/replanning-possible-diffs/experimets/cards/P01-trainable-recipient.md#L9-L15); [Сохранившийся исторический паспорт](https://github.com/lubochka/llm_mvp_core/blob/67abfabe6def8a002984be80b999824601ad273c/docs/plans/replanning-possible-diffs/experimets/cards/P01-trainable-recipient.md#L30-L48).

Новый общий post-arms текст запрещает самовольные warmup/equivalence/cosine/B→A/reviewer rerun, требует сохранить уже выполненные лишние операции отдельно и разрешает редакционную запись нового hash отчёта без повторного обучения. Наш аудит не запускал эти операции; обратный порядок N-KU-01 оставлен как **уже существующий** дополнительный результат, а не как разрешение продолжать. [Post-arms правило](https://github.com/lubochka/llm_mvp_core/blob/67abfabe6def8a002984be80b999824601ad273c/docs/plans/replanning-possible-diffs/experimets/cards/P01-trainable-recipient.md#L11-L19).

## N-KU-01 и запрет менять protected

Новая N-KU карточка фиксирует ровно нужный candidate: actual A0/A1/B0/B1, ложные ADD_LOOP и пропуск необходимого цикла отдельно, majority k3 limitation, текущий `ActionLabelGenerator.Label`/DoStatement target, без повторов и без переноса в продукт. Это согласуется с [candidate-result.md](candidate-result.md). **Численный результат не устарел:** compare не изменил raw arms/gold или product code. [Новое содержание кандидата](https://github.com/lubochka/llm_mvp_core/blob/67abfabe6def8a002984be80b999824601ad273c/docs/plans/replanning-possible-diffs/experimets/cards/N-KU-01-do-loop-label.md#L100-L109).

При этом новый текст называет `required-changes.md` единственным реестром и предписывает дописать его в сессии регистрации результата. **Это не отменяет прямой запрет нынешнего пользователя менять всю protected-папку.** Здесь кандидат представлен в разрешённом planning dossier; core `required-changes.md` нами не изменён, его обновление не объявляется выполненным. Нельзя считать новый текст внешнего файла разрешением нарушить текущую границу. [Предписание реестра](https://github.com/lubochka/llm_mvp_core/blob/67abfabe6def8a002984be80b999824601ad273c/docs/plans/replanning-possible-diffs/experimets/cards/N-KU-01-do-loop-label.md#L104-L105).

Новые N62 output roots находятся внутри protected `experimets/results`. Для LF-03 настоящего дополнения авторский output contract задан здесь заранее: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/LF-03/results/2026-10-20/LF-03-01/`, с отдельными `A/` и `B/`. Host/source paths остаются прежними. В ещё не материализованном экспериментальном bridge/study.json этот literal root становится единственным допустимым root; проверка точного пути и create-new semantics сохраняется. Это явное документное назначение разрешённого места для нового экспериментального host, не отключение guard и не изменение product capability. Все receipts, native checkpoints, issued/completed и итоговый отчёт должны оставаться под этим root. Другие N62 ID настоящим дополнением не запускаются и не получают неявного переноса пути.

Назначенные команды LF-03 после будущей подготовки и отдельного поручения на выполнение:

```powershell
$Lf03TaskHost = "C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/LF-03"
$Lf03TaskResults = "$Lf03TaskHost/results/2026-10-20/LF-03-01"
dotnet build "$Lf03TaskHost/Runner.csproj" -c Release -p:Variant=A
if ($LASTEXITCODE -ne 0) { throw "LF-03 A build failed; no run." }
dotnet build "$Lf03TaskHost/Runner.csproj" -c Release -p:Variant=B
if ($LASTEXITCODE -ne 0) { throw "LF-03 B build failed; no run." }
dotnet "$Lf03TaskHost/bin/A/Release/net9.0/N62Runner.dll" --arm A "$Lf03TaskResults/A"
if ($LASTEXITCODE -ne 0) { throw "LF-03 A failed; preserve partial evidence; no B or retry." }
dotnet "$Lf03TaskHost/bin/B/Release/net9.0/N62Runner.dll" --arm B "$Lf03TaskResults/B"
if ($LASTEXITCODE -ne 0) { throw "LF-03 B failed; preserve partial evidence; no retry." }
```

Эти команды — документная спецификация, не заявление о готовом проекте или выполненной сборке. Исходная N62-карточка остаётся неизменной; запись в protected этим дополнением не разрешается. [Назначенные N62 result roots](https://github.com/lubochka/llm_mvp_core/blob/67abfabe6def8a002984be80b999824601ad273c/docs/plans/replanning-possible-diffs/experimets/cards/N62-neural-update-costs.md#L80-L87).

## Как читать итоговый пакет после этой дельты

1. `coverage.json` и основная таблица остаются воспроизводимым **baseline 9622b575**; они не перезаписаны вслед за движущимся HEAD.
2. Для текущего N62 использовать исправление статуса из этого документа; для N61 cache остаток сохраняется.
3. LF-03 имеет частичное новое protected-покрытие; настоящее дополнение закрывает конкретный data/budget-конфликт существующей fixture с hash и одним update/arm. Native host по-прежнему не собран и не запущен.
4. Positive N-KU, ingress, P125, restore и neural results не отменены документной дельтой. Любой полезный измеренный выигрыш остаётся успехом своей метрики/домена; регрессии и неопределённость рядом.
5. Никакая новая редакция карточки не создаёт новый экспериментальный результат, полномочие на повторный запуск или разрешение менять запрещённую папку.

