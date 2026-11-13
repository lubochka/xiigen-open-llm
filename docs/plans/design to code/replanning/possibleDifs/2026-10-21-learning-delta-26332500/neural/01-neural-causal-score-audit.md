# Нейронный путь: новая гипотеза о лишних causal QK вычислениях

Дата аудита: 21.10.2026. Core: `26332500f76ea8345256c73ecfd44c0023b7e883`. Planning для проверки новизны: `14308ff181c5a93d837d92b85b2f35c41b863565`.

**Результат: найден один новый узкий фактор — `ATTN-CAUSAL-SCORES-01`.** В штатном forward обучения модель вычисляет все пары query/key, затем затирает будущие позиции каузальной маской. Гипотеза: можно уменьшить цену настоящего обучения, пропуская только эти заведомо неиспользуемые forward dot products. Сейчас это **документированный кандидат для эксперимента**. Нового запуска, нового host, чисел ускорения или доказанной причины ухудшения ответов нет.

Отдельный дополнительный проход по качеству и состоянию не дал достаточно нового подтверждённого дефекта для второго ID. Уже известные SiLU, GQA, RoPE, RMS, optimizer, response mask, tokenizer и selected-output-head не переименованы в новые находки.

## 1. Что непосредственно видно в текущем коде

| Участок | Факт и доказательство |
|---|---|
| Реальное обновление | `Trainer.TrainSupervisedNextTokenStep` получает shifted sequence, вызывает `model.Forward(training:true)`, CE, backward, clip и AdamW; после шага очищает gradients. [Trainer:102–147][train] |
| Связь с attention | `LlmModel.Forward` выполняет embedding → каждый TransformerBlock → norm → head. Обычный `CausalSelfAttention.Forward` строит Q/K/V, RoPE, GQA и вызывает `ScaledDotProduct`. [model:219–241][model]; [attention:129–170][attention] |
| Лишние элементы | `ScaledDotProduct` вызывает общий `MatMul(q,kT)` для `[B,h,T,T]`; затем scale и маску, которая заменяет **каждый** элемент `column>row` на −∞. Результат unmasked scores другому обучающему потребителю не передаётся. [attention:263–291][scores] |
| Внутренний цикл | `TensorOps.MatMul` направляет всю матрицу в `SimdOps.MatVecRowUnpublished`; `MatVecRowCore` перебирает **все** `col < bCols` для каждой строки. Каждый dot product имеет SIMD-часть и scalar remainder. [MatMul:593–642][matmul]; [SIMD:99–146][simd] |
| Производная маски | Backward копирует входной gradient только для `column<=row`; остальное остаётся нулём. Отсутствие влияния будущих scores на локальный training objective видно и в обратном проходе. [mask:1384–1452][mask] |
| Отдельный cached путь | `ForwardWithCache` использует `ScaledDotProductWithPast`, иной прямоугольный контекст и positionOffset. Этот путь не является фактором предложенной карточки. [attention:173–248][cache] |

Это не вывод об ошибке каузальности. Напротив, causal mask делает будущее недоступным. Недостаток рассматривается как лишняя работа внутри реально вызываемого обучения.

Для одной головы и длины `T` A считает `T²` dot products, B должен считать `T(T+1)/2`; разница `T(T−1)/2`. Это **алгебраический счёт операций**, не измеренная экономия времени или памяти. Нативные dense allocations, транспонирование, softmax, второй MatMul, полный backward и optimizer сохраняются, поэтому экономия полного шага может быть мала либо отсутствовать. При `T=1` этот фактор вообще не даёт пропуска.

## 2. Непрерывная цепь обучения и чтения результата

**Учебные bytes → штатный native Trainer → q/v LoRA → обычный causal attention → masked logits → CE/backward → AdamW → сохранённые trainable параметры + m/v + RNG → тот же model.Forward на фиксированной TRAIN/EXAM панели.**

Producer — настоящий `TrainSupervisedNextTokenStep`; proposed change находится **внутри** forward этого producer. Consumer — prompt-only forward того же обученного Student; измеряются complete one-byte answers, NLL/rank и сохранение приобретённых ответов. Стоимость считается вокруг настоящих четырёх update, а не в отдельном MatMul stopwatch. Новый искусственный learner, таблица ответов, детерминистический решатель знака или surrogate loss не допускаются.

Тема карточки — **язык программирования**, задача — знак результата двух небольших C# выражений. Механизм — native SFT + q/v LoRA; он не является отдельным доменом. Две метки `+` и `−` относятся к одной теме. Более широкое ускорение других тем возможно, потому что оператор общий, но до отдельных измерений это только переносимая инженерная гипотеза. Она не проверяет AST-aware kNN, порядок естественного предложения, DPO quality, восстановление исторического Flow или свободную многотокенную генерацию.

## 3. Единственное изменение и адрес возможного будущего исправления

В экспериментальной source-copy добавить узкий `TensorOps.MatMulCausalScores` для square rank4 QK score shapes. Его forward оставляет native shape validation, transpose B, dense result allocation и точный внутренний dot accumulation; меняется только верхний предел столбца — `column<=row`. Остальные ячейки свежего массива остаются нулевыми до **неизменённой** scale+causal mask, которая запишет в них −∞. Весь текущий `MatMul` backward переносится без изменения порядка суммирования. Новый **internal static** bounded wrapper `SimdOps.MatVecRowCausalUnpublished` сохраняет native validation и вызывает **private static** `MatVecRowCausalCore`; `TensorOps` вызывает только internal wrapper, как в существующей паре MatVecRowUnpublished/MatVecRowCore. Узкий `TensorOps.MatMulCausalScores(Tensor a, Tensor b, CancellationToken cancellationToken)` объявляется public static и возвращает Tensor для вызова из CausalSelfAttention; private core недоступен другим классам. В `CausalSelfAttention.cs:276` переключатель A/B выбирает общий или узкий метод.

Нельзя выдавать новую функцию за обычное полное матричное умножение: её верхняя часть намеренно не вычисляется, и законный consumer — только немедленная causal mask. Нельзя одновременно оптимизировать backward, GQA expansion, RoPE, transpose, head, PAD, softmax или выделение памяти. Иначе это будет несколько факторов. Требование конечных исходных Q/K и сохранение исходных отказов/границ отмены указываются явно.

Адрес возможного переноса после результата: `03.TransformerModel/Layers/CausalSelfAttention.cs::ScaledDotProduct`; новая узкая функция в `00.TensorCore/Core/TensorOps.cs`; internal bounded wrapper и его private core в `00.TensorCore/Core/SimdOps.cs`. Сейчас исходный код read-only. Конкретный author work-order и данные: [карточка](ATTN-CAUSAL-SCORES-01.md), [промпт автору](ATTN-CAUSAL-SCORES-01-executor.md), [36 входных файлов с актуальными хешами](CS01-SOURCE-COPY-INPUTS.json).

## 4. Проверка новизны и каноническое покрытие

Проверены поиском **976 Markdown** в `possibleDifs` и **406 Markdown** в защищённой `docs/plans/replanning-possible-diffs`; выражения и совпадения сохранены в [receipt](source-and-novelty.json). Дополнительно семантически сопоставлены ближайшие neural findings/card owners. Это ограниченная проверка корпуса на указанных SHA, **не гарантия отсутствия идеи в любом файле, архиве, другом агенте или ветке**.

| Ближайшее существующее описание | Что оно меняет | Почему не закрывает новый фактор |
|---|---|---|
| [ATTN-TRANSPOSE-STRIDES-01][transpose-card] | Координатные allocation/strides при перестановке attention heads | Все causal QK dot products остаются вычисляемыми. |
| [DPO-SHIFTED-CONTEXT-01][shifted] | Не подаёт в Transformer последнюю неиспользуемую input position DPO | Меняет `T`; новый фактор при том же `T` пропускает только верхние scores. |
| [DPO padding blueprint][padding] | Убирает правый padding у exact carrier | Не заменяет полный QK multiplication внутри оставшегося контекста. |
| [N-HEAD-01][head] | Проецирует в словарь только scored hidden rows | Уменьшает LM-head работу после всех Transformer blocks; верхние scores attention остаются. |
| [47 current attention contract][contract] | Проверяет локальную корректность mask/RoPE/GQA/backward | Это полезное контрдоказательство утверждению «attention уже сломан»; learning-cost A/B такого типа здесь нет. |
| [Neural audit fb9175cc][prior-audit] | LoRA/autograd/restore/tokenizer и SiLU reuse | Дедуплицирует множество старых факторов, но не задаёт описанное вмешательство. |

**Canonical coverage:** отдельной карточки `ATTN-CAUSAL-SCORES-01` и опыта с этим A/B factor на зафиксированном canonical снимке не найдено. Поэтому этот пакет — добавление **в possibleDifs**, а не утверждение, что защищённая папка уже покрывает его. Защищённая папка не изменялась. После будущего валидного выигрыша потребуется запись candidate с полными соседними метриками; это не является автоматическим переносом в production.

## 5. Дополнительный проход по семантике: что не было объявлено новым

| Проверенная зона | Результат прохода и граница |
|---|---|
| Causal/RoPE/GQA gradients | На обычном finite path новый обрыв edge/перепутанная маска не подтверждён; существующий contract47 и P11 уже владеют этой проверкой. Это не численное доказательство всей модели. |
| RMS reduction / Mean keepDims | Обычный RmsNorm сохраняет ось; проблема `keepDims:false` ранее описана как N-SHAPE-01. Её не переносим на текущий caller без условий. |
| LoRA zero-B и dropout | Стартовый нулевой gradient A при B=0 следует из low-rank формулы; B может обучаться. RP01/code-sign dropout0 не позволяет объявить stochastic mismatch причиной результата. Другие targets/config требуют отдельного опыта. |
| SFT prompt loss, warmup, scheduler, continuation | E-SFT-RESPONSE-MASK, E-SFT-MOMENT-CONTINUITY, P12 уже описывают. Для нового опыта constantLR и Warmup0 исключают известный first-step blocker и не подменяют восстановление AdamW step. |
| Tensor cost | SiLU sigmoid reuse, RMS row reduction, RoPE table, frozen Linear transpose, GQA materialization, SFT exp reuse — уже существующие факторы, не новые ID. |
| Tokenizer/data ingress | BPE copies/weighted chunks/realized merges, token identity snapshot, EOS, context admission, stride/uint16/EOD уже имеют предыдущие owners. Не объявлено доказательств, что каждый из них объяснил RP01. |
| Full state restore | Новый опыт явно восстанавливает named weights, moments, step и RNG. Наличие export/import API само по себе не доказывает, что конкретный исторический run использовал все компоненты. |

По итогам этого ограниченного прохода нет основания писать «все причины найдены» или «нейронная математика гарантированно исправна». За пределами моей зоны остаются качество пар и учителя, настоящий historical dataset lineage, кНН/Unit/Flow routing, исчерпывающий аудит остальных веток и исполнение новых гипотез.

## 6. Уточнение native restore для автора будущего опыта

У нового S0 будут full base+LoRA weights. Текущий overload `ValidateFullCheckpointArtifact(stream,cfg,...)` сохраняет **base-only** schema. Нужен overload с `new FullCheckpointLoraTopology(4, ["q_proj", "v_proj"])`, с проверкой `IsSuccess` и `Value==true`. Это следует из [LlmModel:902–965][validate]. Для no-overwrite использовать `SaveCheckpointToStream` с caller-owned `FileMode.CreateNew`, а не полагаться на `SaveCheckpoint(path)`, который выполняет `File.Move(...overwrite:true)`. [LlmModel:553–675][save]

Это уточнение API для воспроизводимого harness, не новый результат об ухудшении обучения. RNG сохраняется частным JSON полного native snapshot, не DPO writer с неканоническим filename. Ненулевой before получается реальным общим обучением или валидным exact checkpoint; в этой карточке выбран и полностью учтён первый вариант.

## 7. Строка для родительского registry

| Stable ID | Приоритет | Current status | До исполнения | Следующий ID |
|---|---|---|---|---|
| ATTN-CAUSAL-SCORES-01 | После разблокировки обучения, разрешения конфликтов карточек и назначенных обновлений; до опытов без связи с обучением | AUTHOR_WORK_ORDER_ONLY / NOT_IMPLEMENTED / NOT_RUN | Автор делает source-copy host; далее один explicitly dispatched run по карточке | Только stable ID из актуального общего registry или прямое указание пользователя; не угадывать номер |

Skills применены по роли: архитектор — различение producer/consumer и общего ядра; планирование — данные/состояние/бюджет; plan review — отсутствие скрытого ready/runtime; code examination — реальный caller; prompt authoring — один фактор и WRONG/CORRECT; human communication — тема отдельно от механизма и успех отдельно от интеграции. Статус нового опыта остаётся единым во всех трёх документах.

[train]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/04.TrainingLoop/Trainer.cs#L102-L147
[model]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/03.TransformerModel/LlmModel.cs#L219-L241
[attention]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/03.TransformerModel/Layers/CausalSelfAttention.cs#L129-L170
[scores]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/03.TransformerModel/Layers/CausalSelfAttention.cs#L263-L291
[matmul]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/00.TensorCore/Core/TensorOps.cs#L593-L704
[simd]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/00.TensorCore/Core/SimdOps.cs#L99-L146
[mask]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/00.TensorCore/Core/TensorOps.cs#L1384-L1452
[cache]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/03.TransformerModel/Layers/CausalSelfAttention.cs#L173-L248
[validate]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/03.TransformerModel/LlmModel.cs#L902-L965
[save]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/03.TransformerModel/LlmModel.cs#L553-L675
[transpose-card]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/experimets/cards/ATTN-TRANSPOSE-STRIDES-01.md
[shifted]: https://github.com/lubochka/xiigen-planning/blob/14308ff181c5a93d837d92b85b2f35c41b863565/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-evidence-dd150aa/neural/01-DPO-SHIFTED-CONTEXT-01.md
[padding]: https://github.com/lubochka/xiigen-planning/blob/14308ff181c5a93d837d92b85b2f35c41b863565/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/61-LEARNING-UNBLOCK-AND-EXPERIMENT-AUDIT-2026-10-20/03-DPO-PAD-IMPLEMENTATION-BLUEPRINT.md
[head]: https://github.com/lubochka/xiigen-planning/blob/14308ff181c5a93d837d92b85b2f35c41b863565/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/61-LEARNING-UNBLOCK-AND-EXPERIMENT-AUDIT-2026-10-20/02-NEURAL-CURRENT-STATE-AND-NOVEL-HEAD.md
[contract]: https://github.com/lubochka/llm_mvp_core/blob/26332500f76ea8345256c73ecfd44c0023b7e883/docs/plans/replanning-possible-diffs/47-current-attention-backprop-and-position-contract.md
[prior-audit]: https://github.com/lubochka/xiigen-planning/blob/14308ff181c5a93d837d92b85b2f35c41b863565/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-causal-audit-fb9175cc/07-neural-source-audit.md
