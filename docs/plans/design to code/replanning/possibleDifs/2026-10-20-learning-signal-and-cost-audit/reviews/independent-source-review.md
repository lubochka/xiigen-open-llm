# Независимая финальная проверка LF-02, LF-03 и материала N60-E1

**20 октября 2026. Только чтение исходников и документов. Никакие build/test/training/inference не выполнялись.**

Проверены документы 01, 03, 04 нового planning-дополнения `2026-10-20-learning-signal-and-cost-audit`; поздняя policy `00-execution-policy-2026-10-20.md` и `23-additions-n60-small-learning.md` из среза f5dd; source ниже закреплён на `bf9b543e149ef197da85a767821d98c9b67916d2`. По сообщению основного аудитора source соответствующих файлов в f5dd идентичен; эта заметка сама не выполняла отдельного tree comparison. Защищённые документы и оригинальный source не менялись.

## 1. LF-02: per-target expVals — правка соответствует коду

[StepVectorCore:866–923][step] делает отдельные chosen/rejected policy graphs, вызывает два `BackwardLogProbs`, затем один `ApplyOptimizer`. [BackwardLogProbs:1072–1120][backward] действительно создаёт `new float[v]` внутри каждой ненулевой target-позиции; все v элементов полностью перезаписываются перед чтением. Перенос только этой allocation после `cnt > 0` и перед циклом позволяет сохранить арифметический порядок и graph semantics.

- Один локальный буфер **на один вызов**; не shared/static/pool и не совместно между chosen/rejected.
- Для N активных targets и V vocab теоретическая разница payload allocation равна `4*V*(N-1)` при N>0; суммировать отдельно по chosen/rejected. Это не замер времени, CPU или peak RAM.
- `N=1` не обещает экономии, N=0 с return не должен получать новую allocation.
- Предложенный actual StepVector A/B валиден как исследование стоимости обучения при условии достигнутого optimizer update; isolated softmax benchmark его не заменяет.
- Идентичность итогового состояния — проверяемое ожидание, не уже измеренный факт.

## 2. LF-03: двойная копия initialGradient — подтверждена

[Tensor.Backward:94–102][tensor-backward] вызывает `new Tensor(initialGradient.Data.ToArray(), initialGradient.Shape)`. [Tensor constructor:38–46][tensor-ctor] передаёт массив в `TensorStorage`; [TensorStorage:20–24][storage] снова делает `data.ToArray()`.

Удаление только внешнего `.ToArray()` оставляет constructor defensive copy. Grad не становится внешним alias; Shape semantics не меняются. Прямое `Grad=initialGradient` либо изменение TensorStorage было бы иной интервенцией. Доказанный caller из DPO — [logits.Backward(logits.Grad):1120][backward]. Удаляемый массив содержит весь gradient tensor, включая позиции с нулевым gradient, а не только response targets.

Ожидаемая payload-экономия — `4*M` на вызов, M=реальное число элементов initialGradient. Для одной DPO-пары сумма зависит от размеров двух logits graphs; реальные длины не приравнивать context capacity. Предполагается текущий синхронный owned graph без постороннего конкурентного изменения массива. Экономия/время требуют actual learning A/B, совместную LF-02 правку сюда не включать.

## 3. Общая поправка к measurement LF-02/LF-03

`StepVector` возвращает значения, рассчитанные policy/reference forwards **до** `ApplyOptimizer` ([891–914, 921–934][step]). Это не post-update objective. Если нужен объективный score после шага, отдельный native readout без update следует явно назвать, посчитать и вынести за границу timed update. Иначе post-update objective — `NOT_MEASURED`; actual state before/after и межплечевая parity остаются допустимыми доказательствами resource-контраста. Gradients сохранять до `_policy.ZeroGrad()` на строке 923. Основной аудитор уведомлён; поправка интегрирована в окончательные LF-02/LF-03.

## 4. N60-E1: исходные быстро/тихо не достигают записи verifier rule

Поздняя [карточка N60-E1:188–218][e1] предлагает V1 `Он быстро открыл дверь.` / `He quickly opened the door.` / `[WRONG:tense]`, V2 `Она тихо закрыла окно.` / `She quietly closed the window.` / `She closed the window.`. Это уже оговорённый карточкой риск достижимости, теперь статически разрешённый для данных точных строк.

Полная цепочка без пропуска альтернатив:

1. [Builder:94–104,128–141][builder] получает required terms quickly/quietly через [ExtractProductMarker:181–188 и regex:560–561][marker]. Он создаёт chosen verifier output с `Pass=false` и issue `expected '...' for translated quality marker`.
2. [Tokenizer:89–95][tokenizer] не содержит быстро/тихо в allowlist. Оба suffix helper ([413–429,574–585][tokenizer-suffix]) не совпадают с `ро`/`хо`; pronoun/preposition/code/verb/genitive branches тоже не подходят. Итог для этих двух слов — NOUN, UNKNOWN.
3. RuleTags **не пусты**: он/она дают subject gender, открыл/закрыла — past verb tags ([153–177][tags]). Следовательно, permissive fallback `RuleTags.Count == 0` не срабатывает.
4. [QualityVerifier.TrainOnExample:1094–1145][verifier-train] сначала вызывает `TryParseIssue`. [Enum SemanticConcept:59–78][enum] прочитан полностью: ADD_OP, VALIDATE_CHECK, NULL_LITERAL, METHOD_REF, CODE_IDENTIFIER, LOG_OP, WRITE_OP, THREAD_REF, FLOW_REF, STREAM_REF, ERROR_EXCEPTION, PROCESSING_OF, DATA_ENTITY, LAUNCH_OP, RETURN_OP, HANDLE_OP, UNKNOWN. В остатке ` for translated quality marker` нет ни одного имени. [Parser:1217–1246][parser] возвращает false. Первая ветвь записи signature/concept rule исключена.
5. `TryParseRequiredTerm` успешен. Однако `qualityMarkerIssue=true` пропускает запись `_requiredTermsBySignature` ([1120–1132][verifier-train]).
6. `ExtractSourceMarker` вызывает **новый базовый русский tokenizer**, не обученный semantic unit ([1265–1279][verifier-marker]). ADV нет; все оставшиеся unknown nouns отфильтрованы непустыми RuleTags и helper [1289–1299][verifier-marker], который содержит mojibake suffixes и ly, но не нормальный кириллический `о`. Возвращается пустая строка; marker dictionary тоже не изменяется.

**Вывод ограничен verifier rule-state и этим точным материалом.** Успешный вызов batch не означает ненулевую мутацию каждой внутренней памяти. SemanticTokenizerUnit пишет corrections ([124–140][semantic-train]), Executor тоже имеет собственное обучение. Нельзя писать «во всём batch нет обучения». Не следует называть три verifier TrainOnExample calls тремя подтверждёнными изменениями rule-state.

### Аналитическая панель исходной карточки — не запуск

Предположения: пустой QualityVerifier в обоих плечах; точные строки карточки; другие learned rules не загружаются; inputs/schema валидны и calls достигнуты.

| Новое source/candidate | Gold | A0 | A1 | B0 | B1 |
|---|---|---|---|---|---|
| Он быстро закрыл окно. / He swiftly closed the window. | Pass | Pass | Pass | Pass | Pass |
| То же source / He closed the window. | Fail | Pass | Pass | Pass | Pass |
| Она тихо открыла дверь. / She quietly opened the door. | Pass | Pass | Pass | Pass | Pass |
| То же source / She opened the door. | Fail | Pass | Pass | Pass | Pass |

Все predicted Verify scores =1, Issues пусты. Предсказанная точность 2/4, false rejects0/2, false accepts2/2 на каждом состоянии. Это статический вывод из пустых rule dictionaries, не runtime measurement и не измеренный отрицательный learning result. Предложенный admission factor не может воздействовать через заявленные verifier rules на этих exact source rows.

## 5. Узкий ремонт материала без смешения с N57 tokenizer patch

Подготовить **одинаковую замену материала в A/B**, до заморозки эксперимента:

| ID | Source | Chosen | Rejected carrier |
|---|---|---|---|
| V1 | Он нежно открыл дверь. | He gently opened the door. | [WRONG:tense] |
| V2 | Она горестно закрыла окно. | She sadly closed the window. | She closed the window. |

Нежно/горестно входят в allowlist и распознаются ADV существующим source. `ExtractSourceMarker` выбирает первый ADV напрямую, раньше fallback: нежно / горестно. ExtractProductMarker извлекает gently / sadly. Нового semantic concept в issue по-прежнему нет; запись идёт строго в `_requiredTermsBySourceMarker`.

- A после native training имеет нежно→gently и горестно→sadly.
- B, пропускающий только V1 verifier row по совпадению candidate=chosen, имеет горестно→sadly.
- Семантические/executor rows для обоих V остаются одинаковыми в обоих плечах. Не изменять tokenizer, marker helper, synonyms, verifier reader или остальные admission rules.
- Сам one-factor source repair остаётся guard вокруг добавления verifier example в `TranslationStructuredBatchBuilder.BuildGeneralBatch` около 128–141: skip только при ordinal equality уже рассчитанного rejectedCandidate и pair.Chosen. Не удалять целый source pair.
- Защищённая карточка не меняется; этот proposed replacement оформляется в разрешённом дополнении.

### Аналитическая панель исправленного материала — не запуск

Те же empty-state предположения, четыре новых VerifierInput, две source recombinations:

| Source / candidate | Gold | A0 | A1 | B0 | B1 |
|---|---|---|---|---|---|
| Он нежно закрыл окно. / He tenderly closed the window. | Pass | Pass | Fail | Pass | Pass |
| То же source / He closed the window. | Fail | Pass | Fail | Pass | Pass |
| Она горестно открыла дверь. / She sadly opened the door. | Pass | Pass | Pass | Pass | Pass |
| То же source / She opened the door. | Fail | Pass | Fail | Pass | Fail |

Predicted A1 scores [0.85,0.85,1,0.85], B1 [1,1,1,0.85]; each Fail carries one missing-marker issue. Final A/B correctness обе 3/4; false reject снижается 1/2→0/2, false accept растёт 0/2→1/2. Следовательно, эта постановка способна показать ровно требуемую пользователем картину: полезный выигрыш по одной оси без обязательного роста accuracy и с регрессией рядом. Никакое значение таблицы пока не измерено.

Native бюджет остаётся: 2 batch calls; 11 вложенных Unit.TrainOnExample calls (A6/B5), из них 3 verifier calls (A2/B1);16 Verify. После data repair ожидаются 3 rule additions, но actual mutations должны быть записаны отдельно. Ordinary Flow/Translate=0; это поведение обучаемого verifier, не доказательство свободного перевода.

## 6. Согласованность с поздней policy

[Policy от 20.10][policy] отменяет blanket approval и допускает learned-output readout с 0 новых updates при доказанной зависимости от trained state. Документ 04 нового дополнения по существу соответствует этому. `NOT_RUN` обозначает отсутствие запуска; незаполненные host/state поля сами по себе не внешний blocker. Новизна константного N-KU-01 уже поглощена поздней карточкой 23: оставлять как известное уточнение, не новую находку. Основной аудитор интегрирует поздние изменения; эта проверка его файлы не редактировала.

[step]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/DpoTrainer.cs#L866-L934
[backward]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/DpoTrainer.cs#L1072-L1120
[tensor-backward]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/00.TensorCore/Core/Tensor.cs#L94-L102
[tensor-ctor]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/00.TensorCore/Core/Tensor.cs#L38-L46
[storage]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/00.TensorCore/Core/TensorStorage.cs#L20-L24
[e1]: https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/23-additions-n60-small-learning.md
[builder]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L94-L141
[marker]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L181-L188
[tokenizer]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/27.TranslationDomain/TranslationSemanticTokenizer.cs#L89-L95
[tokenizer-suffix]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/27.TranslationDomain/TranslationSemanticTokenizer.cs#L413-L429
[tags]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/27.TranslationDomain/TranslationSemanticTokenizer.cs#L153-L177
[verifier-train]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/27.TranslationDomain/TranslationTrainedUnits.cs#L1094-L1145
[enum]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/27.TranslationDomain/TranslationUnitContracts.cs#L59-L78
[parser]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/27.TranslationDomain/TranslationTrainedUnits.cs#L1217-L1246
[verifier-marker]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/27.TranslationDomain/TranslationTrainedUnits.cs#L1265-L1299
[semantic-train]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/27.TranslationDomain/TranslationTrainedUnits.cs#L124-L140
[policy]: https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/00-execution-policy-2026-10-20.md

## Дополнительная проверка публикационной дельты planning64aa3c75

Независимо перечитаны OCT20-R2, README пакета62 и его positive-feedback candidate на64aa3c75e378ddf32af517ab139f17dea1f0ed15. LF-02 совпадает с R2 по source method, allocation move и ограничениям; разница2/4 total updates — бюджет варианта, не новая гипотеза. Самостоятельная новизна снята. LF-03 не совпадает с export AdamW ради Step. LF-04 использует установленный builder/chosen.Pass=false и теряет marker; соседний conditional candidate требует другого positive producer/chosen.Pass=true и не заменяет LF-04. Корневой агент дополнительно получил и адресно проверил все26 Markdown этой дельты. Runtime не выполнялся.
