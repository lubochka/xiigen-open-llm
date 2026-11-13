# 10 — Пропущенный участок: decoder/STOP/EOS, исходная модель и перенос обучения в реальные Units

<!-- POSTMERGE-AUDIT-2026-11-02:START -->
## После объединения веток: целый ответ может состоять из фрагментов символа

**Контракт переноса X10-01/X10-02:** байтовый токен не обязан самостоятельно декодироваться в целый Unicode-символ. Например, UTF-8 «ж» состоит из `[208,182]`: каждый байт отдельно неполон, оба вместе законны. Новый `AnswerReadingRejectsIncompleteCharacterIdentifiersTests` проверяет `Encode(Decode([id]))==[id]`; на словаре 256 этот критерий оставляет 128 ASCII-байтов. Такая проверка не является общей политикой допустимости генерации.

Сохранить различие допустимого незавершённого префикса, незаконной последовательности и законченного ответа. Приёмка использует настоящий tokenizer, накопленные bytes/IDs и decoder state обычного `TextGenerator`. Положительные примеры: ASCII, кириллица из нескольких byte-token и из BPE-merge. Отрицательные: continuation byte без начала, unknown ID, EOS/лимит после незавершённого многобайтного префикса. Terminal refusal/описание неполноты задаётся явным API-контрактом; отдельный legal prefix не объявляется ошибкой.

Четырёх/пятипараметрный `Sampler.Sample` без tokenizer и префикса не получает скрытого знания о символах. Не менять все generative paths под односимвольный P101; его сохранённый результат остаётся узким свидетельством.

Основание: [Аудит нового среза](codex-2026-11-02-POSTMERGE-TRANSFER-AND-EXPERIMENT-AUDIT.md); неизменные исходные результаты и назначенные бюджеты сохраняются.
<!-- POSTMERGE-AUDIT-2026-11-02:END -->

<!-- EFFECTIVE-AUDIT-2026-11-02-POST102:START -->
## POST102 — текущая остановка, фактические токены и проверка целого ответа

**NAMED_CHECK: POST102-INFERENCE-RECEIPT, 02.11.2026.** На core `15745b357e055055ee9bc58b52cc374d90580961` [BpeTokenizer.EosToken уже возвращает null](https://github.com/lubochka/llm_mvp_core/blob/15745b357e055055ee9bc58b52cc374d90580961/01.Tokenizer/BpeTokenizer.cs#L108-L120), а [GenerateCore берёт только verifiedEosToken вызывающего](https://github.com/lubochka/llm_mvp_core/blob/15745b357e055055ee9bc58b52cc374d90580961/09.Inference/TextGenerator.cs#L225-L272). Описание `vocabSize−1` ниже — историческое. Это исправление контракта не доказывает обученный STOP и не устанавливает причину прежних 0/3 или 0/4. Продолжение: [адресный аудит POST102](2026-11-02-POST102-TRANSFER-AND-EXECUTOR-AUDIT.md).

**WRONG:** `Encode(decodedText)` назвать фактически выбранными ID; TF 37/47 и own 7/47 назвать одной точностью; `LENGTH_LIMIT` считать либо безусловной смысловой ошибкой, либо разрешением принять любое правильное начало. **CORRECT:** отдельно сохранить actual generated IDs/bytes, текст, stop reason, caller EOS и лимит. Если есть только перекодирование, явно назвать его производной реконструкцией; первый фактический токен не заменяет полный trace. Teacher-forced совпадения всех позиций, длина общего начала и смысловая правильность полного ответа — три отдельные меры.

**Проверка X10-01/X10-02:** до модельного вызова сопоставить canonical gold length, prompt framing и stop policy. В P102 лимит 46 не вмещает назначенный canonical EXAM gold 47; минимальная токенизация по всему словарю не установлена, поэтому абсолютную невозможность более короткого ответа не утверждать. Первая ошибка на позиции 7 не объясняется этим хвостовым ограничением. Один callable смыслового оценщика применяется к калибровке и ответам: верная перефразировка проходит, неверное число и gold с противоречащим хвостом — нет. Полнота, UTF-8 и смысл учитываются раздельно.

**Адресное продолжение:** владелец X10-01 сначала уточняет четыре сохранённых ответа P102 и их предел доказательства без новой генерации. Полные actual IDs, если их нет, могут появиться только в отдельно назначенном чтении сохранённых состояний; их нельзя восстановить утверждением о re-encode. Для X10-05/X10-06 назвать вызванный Unit/reader и принятое состояние: isolated TextGenerator не доказывает потребление KNN/Flow/Graph. Исторические Q1/Q2 и бюджеты ниже этим уточнением не переоткрываются.
<!-- EFFECTIVE-AUDIT-2026-11-02-POST102:END -->

**Статус:** CODE_CONFIRMED в конкретных методах, causal impact NOT_MEASURED. Дополняет 03, 05, 06; **не** заявляет, что EOS — единственная причина провала RP01.

## X10-01 — EOS в byte-BPE не является выделенным специальным токеном

[Исходный tokenizer](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/01.Tokenizer/BpeTokenizer.cs#L35-L68):
- `BpeTokenizer.EosToken => _vocab.Count-1` (строка 38).
- `Train` стартует с 256 настоящих байтов и **присваивает ID 256+ новым статистическим merge byte sequences**, без специальной регистрации EOS в показанном коде (строки 47–68).
- Поэтому **последний ID словаря не доказывает EOS**: при обычном BPE-train он обозначает обученный кусок текста. Нельзя назначать `vocabSize−1` EOS просто чтобы генерация заканчивалась. Это приведёт к досрочной остановке на обычном подслове.

[TextGenerator.GenerateCore](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/09.Inference/TextGenerator.cs#L212-L249) выбирает stop-token двумя путями:
```csharp
int? eosToken = detailed ? verifiedEosToken : tokenizer.EosToken;
// inside loop:
if (nextToken == eosToken || generated.Count == maxNewTokens) break;
```
**Следствие:** `GenerateDetailed(..., verifiedEosToken:null)` никогда не останавливается по EOS, достигает `maxNewTokens` (или failure/cancel); обычный `Generate` в отличие от него может ошибочно завершиться при генерации последнего обычного BPE merge. Это **разные контракты остановки для одной модели**.

[CodeDocumentationStudentGeneration.Generate](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/14.ContinuousLearning/Training/CodeDocumentationStudentGeneration.cs#L61-L75) явно передаёт `verifiedEosToken:null` с комментарием «Current verified manifests declare no EOS». [ENRU research Student.Generate](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/14.ContinuousLearning/Training/SimulationEnRuContinuation.cs#L110-L124) также передаёт `null`. Настройки [generation-v1.json](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/config/code-documentation/generation-v1.json#L26-L40): **maxNewTokens=256, temperature=0, topK=0, topP=1, maxAnswerChars=4096**. Никакого самостоятельного конца ответа у этого exam-mode не задано.

**Это не объясняет автоматом 0/3 и 0/4:** модель может сгенерировать полезный текст даже с `LENGTH_LIMIT`. Но semantic examiner мог отклонять шумные бесконечные продолжения; требуется посмотреть `StopReason`, конец ответов, процент первых корректных значащих токенов и рубрику.

**Однофакторный тест:** с теми же frozen weights/tokenizer/seed сравнить (A) first 1/8/32 valid tokens, (B) complete 256, (C) отдельный `stop-sequence` на уровне **строки/формата** без фиктивного ID. Не ре-обучать уже потреблённые DPO-pairs; доказать, что examiner не перепутал неизбежный `LENGTH_LIMIT` с полной неспособностью. Проектировать EOS можно только как **версионированный tokenizer+model training target**; он меняет checkpoint compatibility и требует migration.

## X10-02 — Декодер и objective учат разные вещи при неизменном prompt
[Current code documentation framing](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/14.ContinuousLearning/Training/CodeDocumentationStudentGeneration.cs#L40-L57) объявляет `owner-input-verbatim-no-prefix-v1`; генерация кодирует `OwnerInput` **verbatim** [строки 61–73](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/14.ContinuousLearning/Training/CodeDocumentationStudentGeneration.cs#L61-L73). [DPO materialization](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L114-L125) берет `row.Prompt`, `row.Chosen`, `row.Rejected` без внешнего шаблона. Наличие канонического exact masking **не доказывает**, что `Prompt` в JSONL совпадал с exam `OwnerInput` по instruction framing, ролям, завершающему разделителю, локали, требуемому типу ответа или типу единицы.

**Diff probe:** (1) bytes/hash source `Prompt`; (2) precise tokenizer IDs for prefix; (3) exam owner-input IDs; (4) common prefix + next-token target distribution. Замена формата при оценке без соответствующего обучения может убить способность даже при правильном DPO loss. Не добавлять в закрытый exam обучающие слова.

## X10-03 — Ответные параметры ограничения длины не все исполняются в данном методе
[CodeDocumentationStudentGeneration.Generate](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/14.ContinuousLearning/Training/CodeDocumentationStudentGeneration.cs#L61-L76) проверяет `MaxAnswerChars>0`, но зовёт `GenerateDetailed` с **MaxNewTokens**, а `MaxAnswerChars` не участвует в вызове. Возможно, ограничение соблюдает вызывающий consumer; данный метод не гарантирует соблюдение chars-budget. **Проверка:** request with huge token decoder expansion, verify bounded receipt downstream, return typed truncation; не считать форматную ошибку semantic regression.

## X10-04 — Случайная B0 и выбор только q/v LoRA могут не дать нового языка
[RP01 recovery](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md#L18-L32) фиксирует **random B0**. [model config](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/config/code-documentation/generation-v1.json#L7-L16): hidden512×4layers, vocab629, context2048; [lora settings](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/config/code-documentation/generation-v1.json#L34-L48): rank8, alpha16, dropout0, targets q_proj/v_proj. [LoraLinear](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/10.LoRA/LoraLinear.cs#L39-L60) стартует A Kaiming, B нули и вычисляет residual adapter.

**Природа риска:** DPO может сместить относительную вероятность двух готовых строк, но не обязан научить с нуля синтаксис C#, английский и русский. Замороженные embedding/lm head/FFN при LoRA q/v ограничивают пути записи новых знаний. Это **гипотеза expressivity/capacity**, не признак математически неверного LoRA.

**Нужны две ортогональные оси:** 1) pretrained/warm-start vs random B0; 2) q/v vs q/k/v/o vs adapter+output layers при одинаковой базе. Начать с tiny overfit и честного held-out before costly architecture change.

## X10-05 — Потерянный ранний обучающий механизм мог быть НЕ нейронным
В раннем переводческом контуре [SemanticTokenizerUnit](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L68-L141) после `TrainOnExample` запоминает `SemanticSignature→Pattern` и `NormalizedRawText→TokenizerOutput`. [ExecutorUnit](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L232-L315) сначала делает exact-source, phrase, word mapping, затем KNN и law/order fallback. В [TrainOnExample](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L403-L445) chosen side попадает в `_exactSourceMap`, `TranslationKnnStore`, E26 contextual maps; [E27 reorder](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L314-L315) фиксирует структурный порядок. [QualityVerifierUnit](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L1040-L1092) хранит concept/rule constraints.

**Это контент-обучение дискретной памяти и правил, но не обновление LoRA.** Если прежний «работающий эксперимент» обеспечивался dictionary/KNN/regex/graph adapter, а RP01 проверяет только private `LlmModel` DPO logits, результат будет различаться даже при исправной LoRA. Обратная крайность — назвать все rule maps «скрытым запрещённым детерминизмом»: rules могут быть constraints или честными learned lookup; их training/update и serving owner должны быть явными.

**Действие:** паспортизировать старый успешный case `Input→(Tokenizer pattern, concept)→(SourceMap / KNN / E26)→(E27 reorder)→(Verifier)→Output` и сравнить 4 arms на SAME inputs: raw random Student, original dictionary+KNN units, Student+original Units, Student after DPO+original Units. Раздельно учитывать quality/type/latency/learning-specific retention; это позволит локализовать пропавший компонент, не априорно объявляя LoRA неправильной.

## X10-06 — Вложенное retrieval не даёт автоматически новое знание в Student
[TransformerEmbeddingKnnLogicalUnit.ExecuteAsync](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L181-L210) возвращает `example.Output` **verbatim**; [KnnLogicalUnit](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L221-L256) возвращает top1-typed Output. Эти Units могут исправлять ответ consumer без какого-либо обновления языковых logits. Если exam генерирует непосредственно `TextGenerator`, он их **не вызывает**, и хороший результат старого retrieval нельзя ждать от standalone LM.

**Критерий доказательства:** одна и та же новая задача проходит в production read path и в isolated Student; отчёт обязан отдельно показывать `retrieval_hit`, `unit_output_used`, `consumer_output`, `LoRA_state_id`. Запретить headline «the model learned» при одном лишь удачном kNN hit.

## X10-07 — Стоп-механизм и переводческие units меняют смысл качественной оценки
[RP01 audit](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md) отдельно отмечает узкий deterministic ENRU law guard: **wrong 4→0, correct 2→3, abstain 0→3 на шести OPEN**. Это может быть полезная защита продукта, но абстиненция не равна выученной способности переводить. Не смешивать её score с free-generation 0/4. Утверждение «все детерминированные алгоритмы должны быть заменены LoRA» неверно: learned authority выбирает допустимые варианты, checker проверяет инварианты.

## Два быстрых опыта без CI
**Q1 (read-only, 10–20 inputs):** одно frozen checkpoint, exact prompts, detailed vs simple stop semantics, `StopReason`, prefix-correct tokens, response validity и EOS/tokenizer manifests. **Признак обнаружения:** устойчивая потеря корректного короткого префикса из-за неправильной остановки или failure по length. Если никаких отличий нет — закрыть EOS как объяснение именно данного RP01 failure.

**Q2 (learning owner, 8–12 структурных контрастов):** сравнить оригинальный legacy Unit route и isolated Student/DPO route на совпадающих source/language/task/metric. **Признак обнаружения:** старый контур применяет обученные mappings/кандидатов, а RP01 их никогда не потребляет; тогда лечить надо state/adoption или дизайн L2 а не градиент. Не менять teacher/judge/heldout и не переиспользовать consumed DPO пары.

**Итог:** уникальный конкретный пробел, который нельзя закрыть только 500 DPO-pairs: **путь формирования ответа (EOS, framing, tokenizer, retrieval, discrete unit state) может отличаться от оптимизируемого Student**, и исторические gains относятся к компонентам вне Student.
