# N27-C — Три способа получить ложную «точность Unit»: negative-only Verifier, self-label Tokenizer, неверный язык оценки

**2026-10-16.** `SOURCE_CONFIRMED_EVALUATION_CONTRACT_SEAMS / MATH_PROXY_ONLY / NATIVE_NOT_RUN / CROSS-BRANCH_CAUSALITY_NOT_ESTABLISHED`. Core pinned commit: `8c7d41611aecba98d3d1bbdbf30d69bd784949ae`. Область: **структурный TranslationMachine**, не `CodeDocumentationSimulationDpoExperiment`/RP01 Student-only. Независимая CODEX/Astra оценка НЕ выполнялась.

## Почему это отдельный новый вопрос

Большой список возможных ошибок обучения бессмысленен, если «accuracy обученного Unit» сама по себе не проверяет способность учиться. В конкретном конструкторе `BuildGeneralBatch` найдено **асимметричное формирование validation target**: Verifier получает только ошибочные кандидатные ответы с единственным правильным классом `Pass=false`, а SemanticTokenizer ожидает собственную метку детерминистического tokenizer. Такой held-out способен *подтвердить деградацию как успех* и не обнаружить важные ошибочные ответы.

Это дополняет, но не повторяет [N21 self-label structured teaching](21-INDEPENDENT-STRUCTURED-LEARNING-GAPS-2026-10-16.md), [N23 verifier collisions](23-VERIFIER-ACCUMULATED-RULE-CONFLICT-AND-DIRECTION-SCOPE-2026-10-16.md) и [N26 direction-biased no-heldout coverage](26-NEW-DIFFERENCES-ROLE-POLARITY-AND-DIRECTION-METRIC-2026-10-16.md). Здесь новая связка именно **builder→heldout scoring**.

### N27-C1 — Verifier held-out содержит только отрицательный класс

**Первичная пара источников:**

- [`TranslationStructuredBatchBuilder.BuildGeneralBatch` L128–141](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L128-L141): каждый verifier `TranslationLearningExample` получает `VerifierInput(pair.ASide, rejectedCandidate)`, а `ChosenOutputJson = VerifierOutput(..., Pass: false,...)`. Нет второго held-out verifier example с `Input.CandidateText=pair.Chosen`, `Pass:true`. Параметр `heldOut` распространяется на эту запись.
- [`QualityVerifierUnit.MeasureAccuracy` L1148–1170](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationTrainedUnits.cs#L1148-L1170) оценивает только долю `result.Pass == expected.Pass`; сравнения смысла ответа с golden text в этом методе нет.

**Математический diagnostic, не .NET-показатель:** на 4 независимых примерах, произведённых таким builder с `heldOut:true`, «всегда отказать» (`Pass=false`) получил бы **4/4 = 100%** по текущей Verifier-accuracy; но если добавить 4 истинно правильных ответа с `Pass=true`, сбалансированная accuracy стала бы **4/8 = 50%**. Следовательно высокий балл может означать `reject_all`, а не выученную способность отличать хорошие от плохих переводы.

**Ограничение:** настоящий `QualityVerifierUnit.Verify` не является константным `reject_all` (см. [L1052–1091](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationTrainedUnits.cs#L1052-L1091)); контрпример демонстрирует отсутствие различительной силы validation protocol, **не утверждает**, что текущий Unit действительно отказывает во всех задачах.

### N27-C2 — Tokenizer held-out повторяет собственную детерминистическую метку и игнорирует Signature

- [`BuildGeneralBatch` L89–115](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L89-L115) делает `sequence = new TranslationSemanticTokenizer().Tokenize(input,...)` и тут же выставляет `TokenizerOutput(sequence.Pattern, signature)` как **chosen** без независимой `Pattern`-разметки, даже при `heldOut:true`.
- [`SemanticTokenizerUnit.Tokenize` L88–120](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationTrainedUnits.cs#L88-L120) начинает с того же алгоритма `_tokenizer.Tokenize` и только потом накладывает `_patternCorrections/_signatureCorrections`.
- [`SemanticTokenizerUnit.MeasureAccuracy` L162–184](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationTrainedUnits.cs#L162-L184) считает совпадение **только `result.Pattern == expected.Pattern`**; `result.Signature`, `RuleTags`, `PrefixSignature`, `StructureSignature` и downstream KNN decision вообще не проверяет.

При неизменённом rule-based tokenizer и отсутствии ранее выученных conflicting corrections такой heldout **может получить 100% ещё до обучения**, а ошибочная структура Signature при правильном coarse `SentencePattern` останется незамеченной. Это прямое условное следствие кода; не заявлять, что вся математика валидатора всегда равна 100%.

### N27-C3 — Verifier измеряется с `sourceLang="ru"` даже для EN/HE/ES inputs

- [`QualityVerifierUnit.MeasureAccuracy` L1155–1167](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationTrainedUnits.cs#L1155-L1167) явно вызывает `tokenizer.Tokenize(input.SourceText, "ru")` для каждого held-out без чтения настоящего `SourceLanguage`.
- [`BuildGeneralBatch` L88–96](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L88-L96) знает `SourceLanguageFor(direction)` и выполняет семантическую токенизацию в исходном языке.
- [`TranslationMachine.MeasureHeldOutAccuracy` L662–672](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationMachine.cs#L662-L672) агрегирует Unit accuracy и способен включать этот verifier score.

Если source English/Hebrew/Spanish семантизируется иначе, class/pass может отличаться *из-за неверно выбранного языкового parser*, не из-за learner. Это отдельный measurement-path divergence от ранее задокументированного [N26 `EvaluateAsync` hardcoded RU_EN](26-NEW-DIFFERENCES-ROLE-POLARITY-AND-DIRECTION-METRIC-2026-10-16.md).

## Быстрый causal-experiment N27-C: 4+4 balanced Verifier / 4 Tokenizer / 2 языка

Проверка занимает единицы небольших Unit вызовов, **0 DPO/LoRA updates**, один seed и иммутабельный approved gold.

| Изменяется только | Контроль А | Альтернатива Б | Одна наблюдаемая метрика |
|---|---|---|---|
| Состав heldout Verifier | только 4 заведомо плохих кандидата builder | те же 4 плохих + 4 заранее независимо признанных хорошими `chosen` с `Pass=true` | `balanced_verifier_accuracy`, `false_reject_rate` при той же trained Unit |
| Оценка Tokenizer | текущий `Pattern`-only | те же 4 test source, independent gold по `Pattern+Signature/role/POS`, не по bootstrap | `signature_accuracy` и downstream corrected output (не только pattern match) |
| SourceLang в verifier evaluation | forced `"ru"` | из направления source row (`"en"` либо `"ru"`) | `Pass` disagreement на одной и той же trained law/candidate |

**Фальсификаторы:** (1) если реальный verifier heldout содержит `Pass=true`, этот negative-only вывод не относится к нему; (2) если выбранные пары дают одинаковые корректные token sequences при `ru/en`, локальный language effect равен 0; (3) если Signature не участвует в последующем KNN/Flow пути, нельзя приписывать потерю accuracy данному field. Все позитивные target labels должны быть **независимыми от bootstrap** и из same source-disjoint frozen eval. Контроль «always-reject» нужен только для оценки протокола, **не заменяет** реальный `QualityVerifierUnit`.

**Success criterion:** модельная/Unit метрика способна различить законный ответ и близкую по форме ошибку с нулём критических нарушений; вектор `[TP,FP,FN,TN, abstain, direction, source-family]` публикуется отдельно, общий балл не маскирует домены. Показать реальный `source→Unit→Flow→ordinary user result`. Первичное PASS/FAIL — валидность метрики; каузальное качество новой модели требует отдельного обучения/экзамена.

## Планировочная защита и роли

Не чинить scheduler, LoRA или сам KNN только ради улучшения proxy-accuracy. Сначала учредить independent source-family test/gold, затем сравнить настоящую trained policy при фиксированных данных и decoder. **CODEX в роли человека** (`astra 6 ultracode`) выбирает учебные случаи; внешняя открытая китайская модель, если создаются **новые настоящие DPO**, пишет **и chosen, и rejected**; отдельный CODEX (`astra 6 ultracode`) оценивает качество неизменённой полной пары. Никаких таких вызовов в этом source-audit **не было**.

**Связь с RP01:** методологическая угроза при переносе Unit-механизма и аттестации learned quality, **не** непосредственное доказательство Student free-generation 0/4.
