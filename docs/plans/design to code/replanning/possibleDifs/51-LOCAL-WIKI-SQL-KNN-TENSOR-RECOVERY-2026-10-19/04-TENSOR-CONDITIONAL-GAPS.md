# Два условных дефекта тензорного API: приложение P2

19 октября 2026. [Приоритет восстановления Wiki](00-LOCAL-WIKI-RECOVERY-FIRST.md) · [Внутреннее устройство модели](01-LEARNING-INTERNALS-BEFORE-AFTER.md).

**Эти свойства уже присутствовали в local_wiki_planning; проверенный RP01 их не активирует.** Новизна относится к конкретным формам тензоров и индексным формулам в проверенном корпусе, а не к установленной причине ухудшения обучения. Сначала надо найти реального вызывающего потребителя с такими формами. Python ниже — математические контрпримеры, без native C# и без измерения качества ответов.

Core C: 4945b8e85c17b5f150eb5b840d16bbdc4b506242; Wiki W: 219d7ba89dc3ef43e03e17f1461ad96154d3dd7a.

## 3. N-SHAPE-01: Mean удаляет ось в forward, но не удаляет её индекс в backward

**Сейчас.** `Mean(x, axis, keepDims:false)` корректно создаёт форму без выбранной оси: `resultDims.Where((_, i) => i != normalizedAxis)`. Но `BroadcastGradAlongAxis` получает координаты исходного tensor, заменяет одну координату на 0 и передаёт прежнее количество индексов в уменьшенную форму. `TensorShape.FlatIndex` отвергает это количество до записи градиента.

Источники C: [Mean, 544–580](https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/00.TensorCore/Core/TensorOps.cs#L544-L580), [индекс backward, 1049–1060](https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/00.TensorCore/Core/TensorOps.cs#L1049-L1060), [FlatIndex rejection, 44–48](https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/00.TensorCore/Core/TensorShape.cs#L44-L48). Та же ошибка на W: [Mean, 400–431](https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/00.TensorCore/Core/TensorOps.cs#L400-L431), [backward index, 759–769](https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/00.TensorCore/Core/TensorOps.cs#L759-L769).

**Минимальный контрпример.** Для x`[2,3]` с данными `[1,3,5,2,4,6]`, axis1, получаем `[3,4]`. Для upstream `[2,5]` правильный gradient `[2/3,2/3,2/3,5/3,5/3,5/3]`. Forward при `keepDims:true` даёт те же два числа в `[2,1]`, backward работает. При `false` backward пытается вызвать `FlatIndex([0,0])` у формы `[2]` и получает rank mismatch. Это следует прямо из guard; выполненный Python mirror воспроизвёл ту же ветвь и отдельно проверил ожидаемые производные central finite difference.

**Почему это может мешать обучению.** Любой реальный обучаемый pooling/score, который использует удаление оси, сможет вычислить loss, но не сможет провести этот gradient к входу. Это отказ вычисления, а не скрытый quality gain/loss. **Обычный RP01 `RmsNorm` использует `keepDims:true` и не активирует дефект**: [RmsNorm, 29–40](https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/03.TransformerModel/Layers/RmsNorm.cs#L29-L40). В просмотренном model/DPO/SFT path caller с `keepDims:false` не найден.

**Должно стать.** В существующем math helper `BroadcastGradAlongAxis` различить сохранённую и удалённую ось: при одинаковом rank оставить координату 0; при reduced rank удалить координату `axis` перед `gradOutShape.FlatIndex`. Либо передать `keepDims` явно из `Mean`. Изменять `TensorShape.FlatIndex`, ослабляя проверку, не нужно: его отказ выявляет ошибку mapping. Вход/выход остаются `Tensor → Tensor`; новый learned Unit не требуется. Это внутренний вычислительный алгоритм, а не самостоятельный semantic selector. Состояние модели, DPO пары, tokenizer и checkpoint format не меняются. Отдельно задать scalar-конвенцию для rank1 input: нынешний TensorShape не принимает rank0; пример `[2,3]` этого дополнительного вопроса не затрагивает.

**Быстрый опыт EXP-SHAPE-01.** В disposable native harness вызвать существующий Mean на `[2,3]` и `[2,3,4]`, оси 0/1/−1, `keepDims=true/false`, с явно заданным nonuniform upstream. A — текущий helper, B — только исправление индексного mapping. Сравнить forward equality, exception, полный gradient и finite difference. Отрицательный контроль `RequiresGrad=false` оставляет forward одинаковым; сохранение оси — контроль существующего работающего пути. Предметный этап на 8 уроках C# или EN→RU имеет смысл **только после нахождения actual обучаемого caller с удалением оси**. До этого никаких обещаний роста качества или обучения нового pooling ради теста.

## 4. N-SHAPE-02: MatMul путает batch и head при singleton внутри ведущих осей

**Сейчас.** `TensorOps.MatMul` строит ведущие размеры ответа исключительно из A и выбирает матрицу B по `batch % bBatchSize`. По осевому broadcast значение1 может находиться посередине leading shape; modulo забывает, *какая* ось общая. Неверная привязка повторяется и в нынешнем backward. Источники C: [shape/forward mapping, 599–625](https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/00.TensorCore/Core/TensorOps.cs#L599-L625), [gradient mapping, 649–696](https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/00.TensorCore/Core/TensorOps.cs#L649-L696). На W та же формула в forward: [456–470](https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/00.TensorCore/Core/TensorOps.cs#L456-L470). Старый backward устроен иначе; тождество всех старых/current производных не утверждается.

**Минимальный контрпример без численного шума.** Все внутренние матрицы 1×1. A имеет форму `[2,2,1,1]` и значения `[1,2,3,4]`: два примера по две головы. B имеет форму `[2,1,1,1]` и значения `[10,100]`: коэффициент своего примера, общий для его голов.

| Величина | Код C с modulo | Осевая привязка B к своему примеру |
|---|---|---|
| Индексы B для четырёх матриц A | `[0,1,0,1]` | `[0,0,1,1]` |
| Forward | `[10,200,30,400]` | `[10,20,300,400]` |
| dA при loss=sum(output) | `[10,100,10,100]` | `[10,10,100,100]` |
| dB при loss=sum(output) | `[4,6]` | `[3,7]` |

Все числа конечны, shape совпадает, исключение не требуется: это **тихая подстановка коэффициента другого примера**. Ещё одна форма того же дефекта: A`[2,1,1,1]`, B`[1,2,1,1]` должны дать `[2,2,1,1]`, а нынешний `resultDims` сохраняет `[2,1,1,1]` и выдаёт только два результата.

**Где именно не активен.** В current `CausalSelfAttention` K/V явно расширяются `RepeatInterleave` до числа Q-heads; затем leading shapes Q/K и attention/V совпадают. q/v/FFN/head projections используют двумерную правую матрицу. Эти случаи корректны относительно рассматриваемого broadcast mapping. [Расширение heads, 140–158](https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/03.TransformerModel/Layers/CausalSelfAttention.cs#L140-L158), [MatMul callers, 270–291](https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/03.TransformerModel/Layers/CausalSelfAttention.cs#L270-L291). Наличие padding или нескольких SFT examples само по себе не превращает эти формы в failing example: [SFT/pretraining `[B,T]` construction, 212–233](https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/04.TrainingLoop/Trainer.cs#L212-L233). `StepMany/StepVector` обрабатывает отдельные pair tensors по очереди, а не создаёт данную mixed batch/head проекцию.

**Должно стать.** Внутри `TensorOps.MatMul` построить общую right-aligned broadcast shape leading axes, проверить совместимость каждой оси, вычислить отдельные `aBatchIndex/bBatchIndex` для каждой result-coordinate. Теми же mapping пользоваться в backward, суммируя вклады в реально shared исходные матрицы. Вспомогательная структура `MatMulBatchPlan` с result leading shape и двумя index maps может быть private/internal; модельных уровней и learned состояния она не создаёт. Более узкий допустимый контракт — явный отказ на неподдерживаемом broadcast до вычислений, если ни одному caller он не требуется. Молчаливое modulo при заявленном общем контракте оставлять нельзя.

**Быстрый опыт EXP-SHAPE-02.** На действительном native operator сравнить batch execution с конкатенацией четырёх отдельно вычисленных 1×1 products из таблицы; затем сравнить gradient с independent scalar derivative. A/B меняет только mapping; формы, значения, loss и upstream одинаковы. Controls: одинаковые leading shapes; B двумерный; B`[1,2,1,1]` при A`[2,2,1,1]` — modulo в этих конкретных случаях совпадает с нужной привязкой. Невозможные для broadcast ведущие размеры должны отклоняться до чтения/записи результата.

**Предметная применимость условная.** Риск появляется у реальной batched per-example/per-domain projection B`[batch,1,input,output]`, общей внутри heads данного примера. Для C# это могли бы быть разные методные контексты, для EN→RU — разные роли/полярности предложений. Это пример вызывающего контракта, **не утверждение, что такой caller уже есть в RP01**. Если найден такой caller, первым quality-adjacent опытом должен быть тот же смысловой набор по одному и совместно в batch, включая перестановку порядка examples. Собственные ответы должны сохраняться при неизменном learned state. Только после установленного расхождения сравнивать 8 одинаковых уроков и отложенные ответы; без такого caller этот пункт остаётся API-приложением P2.

## 5. Новизна, измеренное и что осталось открытым

| Пункт | Ближайшие прежние материалы | Что добавлено здесь | Статус |
|---|---|---|---|
| N-SHAPE-01 | P11 detached Embedding/bias/MeanPooling, core41 review RmsNorm `keepDims:true` | Конкретный reduced-rank backward `Mean(...false)` + source guard + минимальный пример | Новый API-факт в просмотренном корпусе; inherited; active RP01 caller не найден |
| N-SHAPE-02 | P10 composed gradients; core41/47 MatMul проверял 2D B или одинаковое число/форму leading matrices | Inner leading singleton `[2,2,…]×[2,1,…]` неверно адресует B, и current gradients идут к неверному примеру | Новый условный API-факт; inherited forward; active RP01 caller не найден |
| Head vs q/v | P01, core25,41, planning E-HEAD | Только заново проверенная таблица реальных механизмов и значения форм | PRIOR, восстановительное сравнение |
| Factor scale A→cA, B→B/c | `2026-10-19-language-restore/04a-lora-factor-experiment.md` | Ничего нового | Исключён из новой квоты |
| Перенос первого момента C#→ENRU | package43 N-NEURAL-02; DLT-K1; R06 | Ничего нового | Исключён |
| DPO→CE конфликт, filename-only context | Полученный draft `dpo-lora.md` следующего пакета | Ничего нового | Исключены |

Выполнены Python source-index mirrors, independent mean finite difference и три MatMul controls. Результат находится в [shape_proofs.json](evidence/shape_proofs.json); воспроизводящий код — [shape_proofs.py](evidence/shape_proofs.py). **Ноль новых тренировок, ноль free-answer измерений, ноль native C# runs.** В локальном окружении `dotnet`/`csc` не найдены; математический результат не переименован в native execution. Новизна ограничена двумя сверенными каталогами и предоставленным draft; просмотр всех когда-либо существовавших веток не заявляется.

Самостоятельный повтор математики: `python evidence/shape_proofs.py --output shape_proofs.json`. Дополнительная сверка исходников: `python evidence/shape_proofs.py --source-root <snapshots> --output shape_proofs.json`, где `<snapshots>/core` и `<snapshots>/wiki` содержат четыре пути, перечисленные в скрипте. Сохранённый исследовательский JSON получен во втором режиме и имеет `source_verification_performed=true`; режим без исходников честно отмечает `false`. Ни один режим не вызывает C# operator.

Более широкий аудит мог бы найти других TensorOps callers вне прочитанного model/DPO/SFT участка, иной backend или реальные failing-shape данные. Эти зоны остаются открытыми. Прежняя bounded оценка core41/47 конкретных обычных форм этими контрпримерами не опровергается.

Для будущих предметных опытов действует [единый договор ролей](05-SMALL-DOMAIN-EXPERIMENTS.md#roles): CODEX с запрошенной моделью Astra 6 ultracode выбирает задачу и материал; внешняя открытая китайская модель пишет обе финальные стороны DPO; отдельный CODEX judge проверяет точную пару и отдельно вслепую оценивает ответы ученика. Таких вызовов в этом исследовании не было. Математические fixtures — диагностика вычислений, не принятые обучающие пары.
