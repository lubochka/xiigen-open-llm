# 26 — Дополнительные source-подтверждённые расхождения: обучение границе ответа и выборка следующего токена

<!-- POSTQ011-2026-11-04:START -->
## POSTQ011 — Support, фактический ответ и ресурсное окно

Q009 даёт предметный пример: supportT1 2→1, P(gold)0,86879754→1, actual answer прежний, общая точность 2/4→2/4. GoldE1 вне support. Сохранённые sampler block-median ratiosB/A1,949–2,672 и allocation10 120→25 872/25 936 показывают цену этой процедуры, а не ускорение всего Generate. Probability не заменяет observedfrequency; strictK/ties и greedyreadout раздельны.

Q010 совпадает по 21 504Float32pairs/84argmax только в full-vs-emptycacheprefill. Широкое wall100,1104→35,3144 мс расходится с medianactualcalls7,0148→7,2893 мс одного прогона. Разность allocation91 712/4=22 928 не становится native-callinvariant. Будущий performanceclaim требует своего окна,одинаковой обвязки/её baseline,AB/BA,warmup иразброса. Принятые strict CDF/cache/R46 части сохранить. Printeddeadline в runner не ограничивает child; boundedwait/kill/reap проверить техническим искусственный child без модели. Новые PRIMARY не назначены.

Основание: [единый разбор](codex-2026-11-04-POSTQ011-TRANSFER-AND-EXPERIMENT-AUDIT.md), [core158](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-158-postq011-transfer-and-experiment-audit-2026-11-04.md) и [план 159](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-159-postq011-addressed-repair-plan-2026-11-04.md). В передаваемое поручение включить собственные положительный/отрицательный случаи, actual consumer и предел доказательства. Исходные результаты, данные, бюджет и очередь сохранены; текущая правка документная.
<!-- POSTQ011-2026-11-04:END -->


Дата: 2026-10-16. **DRAFT / SOURCE_CONFIRMED / NUMERICAL_SHADOW_ONLY / NATIVE_CSHARP_NOT_RUN / QUALITY_CAUSALITY_NOT_PROVED**. Исходный код: [llm_mvp_core, codex/dpo-implementation, snapshot b0afc08](https://github.com/lubochka/llm_mvp_core/tree/b0afc08dc0df110b10ed5ed38b518906665c16f8). Предназначено только для каталога planning/possibleDifs. НЕ изменять core/docs/plans/replanning-possible-diffs.

## Зачем эта отдельная карточка

После сверки [44 DIF](01-PRIORITIZED-DIFFERENCES.md), [существующего EOS-аудита](10-INFERENCE-STOP-TOKENS-AND-UNIT-TRANSFER.md), [DPO material/gradient](03-DPO-PAIR-AND-GRADIENT.md), [N16 KV parity](16-NEW-CAUSAL-GAPS-KNN-TIER-DPO-ZERO-WEIGHT-KV-2026-10-16.md) и [исторической реконструкции matrix→KNN](12-MATRIX-KNN-TRANSFER-PROVENANCE-AND-EVALUATION.md) остаются **два отдельные семантические вопроса**: чему именно обучают модель относительно конца ответа, и какой набор токенов допускает sampler в обычном runtime. Исследование не повторяет "неверен EOS ID" или "не совпадает full Forward/KV cache". 

**Принцип причинности:** кодовое подтверждение поведения ≠ измеренное ухудшение RP01. В RP01 simulation выбран отдельный случайный B0 Student + q/v LoRA, его исходный greedy sampler не затронут описанными ниже условными дефектами top-k/nucleus. Другие KNN/Units работают в ином recipient path. Не подменять пользовательскую точность train-margin, изменённым checkpoint или proxy.

## DS26-01 — DPO exact-v2 не учит явный символ окончания ответа

**Source-verified факт:** [DpoExactPairMaterial.cs L29–48, L64–78](https://github.com/lubochka/llm_mvp_core/blob/b0afc08dc0df110b10ed5ed38b518906665c16f8/11.DPO/Models/DpoExactPairMaterial.cs#L29-L78) конструирует sequence только из отдельно закодированных prompt + chosen/rejected response. Функция [Tokens L171–176 и Mask L178–181](https://github.com/lubochka/llm_mvp_core/blob/b0afc08dc0df110b10ed5ed38b518906665c16f8/11.DPO/Models/DpoExactPairMaterial.cs#L171-L181) не добавляет post-response target, и response-mask заканчивается на последнем токене текста. **Наблюдение ограничено этим exact-v2 материальным маршрутом**: это не свидетельство отсутствия stopping supervision во всех других учебных циклах.

В [BpeTokenizer.cs L37–68](https://github.com/lubochka/llm_mvp_core/blob/b0afc08dc0df110b10ed5ed38b518906665c16f8/01.Tokenizer/BpeTokenizer.cs#L37-L68) EosToken — последний обычный BPE merge ID, а не выделенная end-of-answer сущность. Поэтому его нельзя задним числом вставить как золотой terminator. [CodeDocumentationStudentGeneration.cs L61–75](https://github.com/lubochka/llm_mvp_core/blob/b0afc08dc0df110b10ed5ed38b518906665c16f8/14.ContinuousLearning/Training/CodeDocumentationStudentGeneration.cs#L61-L75) вызывает GenerateDetailed с verifiedEosToken=null; [TextGenerator.cs L203–252](https://github.com/lubochka/llm_mvp_core/blob/b0afc08dc0df110b10ed5ed38b518906665c16f8/09.Inference/TextGenerator.cs#L203-L252) останавливается по maxNewTokens, если нет подтверждённого EOS.

**Новый точный вопрос к уже известному X10-01:** даже если EOS в serving исправить, **в точном DPO objective нет позиции с явным учебным end-of-answer target**. Следовательно, нельзя требовать, что этот DPO этап сам научил отдельный сигнал завершения, особенно на случайной B0. Это дополнительная upstream грань существующей проблемы X10, а **не новый независимый корень**, который можно посчитать отдельно от X10.

**H26-01:** правильный полезный короткий ответ может продолжаться шумом до 256 токенов и быть целиком признан неправильным. Альтернативы: и начальные значимые токены неверны; причина вообще в отсутствии языковой компетенции, а не stop/answer shape. **RP01 observed C#0/3 и ENRU0/4 не доказывают H26-01.**

**Быстрый фальсификатор:** на том же замороженном saved Student (до/после DPO) получить raw 256-token generation для 3+4 исторических запросов и 12 дополнительных source-disjoint задач; отдельно оценить полноту/истинность first 8/16/32/64 tokens и полный ответ. Зафиксировать stop reason. Затем запустить *заранее определённый*, независимый от gold, доменно допустимый строковый stop policy. По тем же IDs сравнить correct-completed, critical omissions, false early stops. Если ни один предварительный префикс не соответствует полному доменному закону, версия о masking quality только за счёт stopping получает отрицательное свидетельство. Произвольный выбор красивого префикса ПОСЛЕ чтения gold запрещён.

**Возможный repair только при подтверждённом эффекте:** версионированный tokenizer/model с реальным special EOS + учётем его учебной позиции и новый, миграционно-совместимый training contract. НЕ менять BPE last merge, исходный checkpoint, источник оценочных примеров или сохранённые отчёты без отдельного design/review. Более дешёвый допустимый boundary stop-policy возможен, но не равен обучению нового EOS.

## DS26-02 — TopK и TopP сейчас взаимоисключающие, не пересекающиеся ограничения

**Source-verified факт:** [Sampler.cs L21–47](https://github.com/lubochka/llm_mvp_core/blob/b0afc08dc0df110b10ed5ed38b518906665c16f8/09.Inference/Sampler.cs#L21-L47) содержит последовательность:
~~~csharp
if (topK > 0) probs = TopKFilter(scaled, topK);
else if (topP < 1.0f) probs = TopPFilter(scaled, topP);
else probs = TensorOps.Softmax(scaled, dim: -1);
~~~
При K>0 управление никогда не попадает в nucleus-фильтрацию. Это может быть намеренным выбором контракта "приоритет topK", а не дефектом, пока потребитель не обещает совместное применение обоих параметров; **семантический риск — mismatch ожиданий и реально оценённого decoding policy**.

**Арифметическая проверка без native модели:** logits=[4,3,2,1,0], temperature=1, topK=3, topP=0.65. Softmax всей строки ≈[0.63641,0.23412,0.08613,0.03168,0.01166]. Nucleus сохраняет tokens {0,1}; TopK сохраняет {0,1,2}. Фактический код выбирает TopK и оставляет token 2 вероятность ≈0.09003 после перенормировки, а предполагаемый intersection K∩P дал бы 0. Это математический shadow по формулам, **не измеренная генерация/accuracy**.

**H26-02:** при будущем non-greedy exam отклонение от заявленного совместного top-k/top-p sampling приведёт к другой свободной генерации и оценке, хотя LoRA и DPO unchanged. **НЕ причина текущего RP01**, поскольку [generation-v1.json](https://github.com/lubochka/llm_mvp_core/blob/b0afc08dc0df110b10ed5ed38b518906665c16f8/config/code-documentation/generation-v1.json) задаёт temperature=0, topK=0, topP=1. Порядок исправления: сначала задокументировать, комбинировать фильтры только после явного выбора serving contract.

## DS26-03 — topK с совпадающими логитами может сохранять больше K токенов

**Source-verified:** [Sampler.TopKFilter L49–66](https://github.com/lubochka/llm_mvp_core/blob/b0afc08dc0df110b10ed5ed38b518906665c16f8/09.Inference/Sampler.cs#L49-L66) получает k-й порог и сохраняет каждый logit >= threshold. На logits=[0,0,0,0] при topK=2 поддержка равна всем четырём token IDs, а не двум. Это НЕ численная гипотеза и не требует RNG для доказательства набора допущенных токенов.

**Граница:** возможна явная политика fair-ties. Без контракта "строго K" нельзя утверждать баг; но если пользователь рассчитывает ограничить область токенов, фактический sampler этого не гарантирует. Не использовать этот seam для ретроспективного объяснения жёсткого greedy RP01.

**Проверка:** native вызов Sampler.Sample с тестовыми logits и seeded Random (доступен internal overload для friend-test), измерить допущенную поддержку через большое число прогонов или прямо через internal filter. Для аналитического случая достаточно exact support; для повышения качества нужны реальные tie/quantized token distributions и blind heldout.

## Дедупликация и относительный приоритет

| Наблюдение | Новизна относительно других документов | Доказательный статус | Обычный RP01 |
|---|---|---|---|
| DS26-01 end target отсутствует в exact-v2 | Детализация upstream training contract к X10-01; **не считать новым независимым EOS-root** | C / quality NOT_RUN | условно P1: сперва посмотреть префиксы 0/3 и 0/4 |
| DS26-02 topK преобладает над topP | Новый sampling-policy seam, не KV parity и не EOS | C + арифметический proxy; native NOT_RUN | OUT_OF_SCOPE для greedy |
| DS26-03 topK держит tie-overflow | Отдельный строго воспроизводимый predicate TopKFilter | C + математический контрпример; native NOT_RUN | OUT_OF_SCOPE для greedy |

Для первопричины RP01 **приоритет выше** остаётся у recipient mismatch, случайной B0, DPO pair semantic quality, masked objective, и отдельного сравнения с historical trained KNN/Translation Unit. См. [00-SYNTHESIS](00-MASTER-SYNTHESIS-AND-CONFLICTS.md) и [BRANCH-ARCHAEOLOGY](11-BRANCH-ARCHAEOLOGY-AND-LEARNING-PROOFS.md). Не превращать sampler fix в замену этих экспериментов.

## DPO authority и оценка

**CODEX с моделью Astra 6 UltraCode** выполняет человеческий выбор обучающих случаев и независимо оценивает семантическую правильность chosen/rejected и окончательных ответов; **внешняя разрешённая открытая китайская модель создаёт ОБЕ стороны chosen и rejected** в реальном DPO-authoring режиме. Действующий RP01 simulation с DeterministicGenerator не является таким режимом. Ни один автор документа не имеет права записать proxy/support counts как реальный CODEX/Astra semantic PASS.

Следующий исполнимый [MICRO-26](26-MICRO-STOP-TARGET-AND-SAMPLER-CAUSAL-PROTOCOL-2026-10-16.md); отдельный [сводный реестр ограничений](26-RECONCILIATION-AND-REVIEW-WORK-ORDER-2026-10-16.md).
