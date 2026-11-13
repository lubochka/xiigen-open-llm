# 26 — Архитектурный blueprint исправления: языковая идентичность выученного Unit, RuleTags и checkpoint

**16.10.2026 | HUMAN-READABLE SOLVED DESIGN / NO PRODUCT PATCH / NO NATIVE TEST / INDEPENDENT REVIEW PENDING.**  
Входит в исследование [N26-LANG/N26-TAGS](26-NEW-SEMANTIC-LANGUAGE-KEY-AND-RULETAG-PARITY-2026-10-16.md) и микропроверку [MICRO-26](26-MICRO-LANGUAGE-KEY-AND-RULETAG-CAUSAL-EXPERIMENT-2026-10-16.md). Не менять запрещённую папку core/docs/plans/replanning-possible-diffs.

## Что сейчас происходит и к чему прийти

На обучении программа получает переводческую строку, разбирает её в структурный шаблон, а затем записывает в SemanticTokenizerUnit поправку. На обычном запросе этот же Unit формирует сигнатуру, по которой другие Unit ищут выученные соответствия и проверяют доменные законы.

Проблема первая: одна и та же строка, написанная на разных языках, может занимать **одну ячейку** памяти SemanticTokenizer. Проблема вторая: когда ячейка памяти срабатывает, программа пересобирает адрес поиска, забывая часть русских контекстных тегов. Проблема третья: старый checkpoint сохраняет эту ячейку **без информации о языке**. Следовательно, после перезапуска восстановить утраченное второе соответствие из сохранённой ячейки нельзя.

**Выбранное направление, условное до микроэксперимента:** язык — часть identity обученного semantic correction; исправленный pattern не должен стирать ранее вычисленные ruleTags; checkpoint сохраняет новый identity явно; KNN memory адреса пересогласуются только после отдельной проверки реального потребителя. Не менять алгоритм DPO/LoRA, переиндексацию, learned routing и semantic law verifier одним патчем.

## Понятная архитектура

~~~mermaid
classDiagram
    class TranslationMachine {
       +TrainStructuredBatchAsync(batch)
       +GetOrchestratorAsync()
       +ExportCheckpoint()
       +ImportCheckpoint(checkpoint)
    }
    class TranslationStructuredBatchBuilder {
       +BuildGeneralBatch(pairs)
    }
    class SemanticTokenizerUnit {
       +TrainOnExample(example)
       +Tokenize(rawText, sourceLang)
       +ExportCheckpoint()
       +ImportCheckpoint(checkpoint)
    }
    class TranslationSemanticTokenizer {
       +Tokenize(rawText, sourceLang)
    }
    class TrainedTranslationAdapter {
       +TranslateAsync(request)
    }
    class ExecutorUnit {
       +TrainOnExample(example)
       +Execute(tokens, topicDomain)
    }
    class TranslationKnnStore {
       +AddPair(signature, source, target)
       +FindNearestCascade(full, prefix, structure)
    }
    class QualityVerifierUnit {
       +Verify(candidate, tokens)
       +TrainOnExample(example)
    }
    class TranslationMachineCheckpoint {
       +SchemaVersion
       +SemanticTokenizer
       +KnnStore
    }
    TranslationStructuredBatchBuilder --> TranslationMachine : 3 unit examples
    TranslationMachine --> SemanticTokenizerUnit : trains and owns
    TranslationMachine --> TrainedTranslationAdapter : serving flow
    TrainedTranslationAdapter --> SemanticTokenizerUnit : shared across directions
    TrainedTranslationAdapter --> ExecutorUnit : direction-specific
    TrainedTranslationAdapter --> QualityVerifierUnit : shared learned laws
    SemanticTokenizerUnit --> TranslationSemanticTokenizer : bootstrap classification
    ExecutorUnit --> TranslationKnnStore : training writes and runtime reads
    TranslationMachine --> TranslationMachineCheckpoint : versioned cold restart
~~~

**Пояснение:** это НЕ шесть отдельных SemanticTokenizerUnit. В [TranslationMachine.CreateRuntime](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationMachine.cs#L714-L790) один Unit передаётся всем шести языковым адаптерам. Именно поэтому ключ должен содержать язык. Runtime-граф использует обучаемые текстовые карты / KNN как компоненты Unit; правила normalisation и направленность — детерминистические вспомогательные функции, не самостоятельный доказанный neural learner.

## Как должно работать по шагам (без кода)

1. Легально подготовленная пара с SourceLang и собственным sourceId допускается в structured teaching только после independent law/semantic verification. Нельзя получать teacher-chosen Pattern из самого текущего bootstrap tokenizer, объявляя это независимой меткой.
2. SemanticTokenizerUnit получает *canonical* язык и нормализованную исходную строку. Теперь это единый адрес learnt correction. EN/pan и ES/pan сохраняются независимо; EN/pan и EN/pan с другим регистром пользуются прежним контролируемым правилом нормализации.
3. При новом пользовательском запросе TrainedTranslationAdapter выбирает нужное направление, передаёт **тот же** canonical SourceLang в shared Unit и извлекает только соответствующую этому языку correction.
4. Tokenizer уже построил base Tokens/RuleTags. Если corrected Pattern меняет классификацию, prefix/structure строятся из **исходных Tokens плюс RuleTags** с новым Pattern, без придумывания нового gold. Если changed Pattern требует пересчёта самих Tokens/RuleTags, это отдельный trainer/version contract; нельзя скрывать несогласованность.
5. ExecutorUnit выбирает learned candidate по разрешённым exact/phrase/word/KNN уровням. QualityVerifierUnit проверяет контекст и закон; candidate остаётся лишь предложением до domain-law admission. В эксперименте логируются sourceLang, signature bucket, hit, selector и окончательный пользовательский ответ.
6. Состояние новых языковых ключей сохраняется явно, cold reload проверяет эквивалентность. Для legacy checkpoint без SourceLang нужен безопасный migration decision, а не автоматическое назначение "ru" или "en".
7. Только after M26-C matched independent heldout разрешается менять production path. Если KNN не вызывается, не приписывать пользователю эффект от изменения его адреса.

## Code quote → смысл → конкретная правка: N26-LANG-01

**Сейчас (реальный код, краткие цитаты):**  
[TranslationTrainedUnits.cs:84–103, 126–143](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L84-L143)

~~~csharp
_signatureCorrections[Normalize(input.RawText)] = chosen;
_signatureCorrections.TryGetValue(Normalize(rawText), out var exactCorrection)
~~~

**Почему плохо:** SourceLang записан в TokenizerInput, но в key не попадает. Shared object может принять EN/pan и ES/pan последовательно, одна коррекция вытеснит другую. LAST-WRITE не является алгоритмом изучения семантики языка.

**Должно стать (проект, НЕ применён):**
- Новый внутренний typed key SemanticCorrectionKey(SourceLanguageCanonical, NormalizedRawText). Его equality должен быть детерминирован и совпадать для train/infer. Не смешивать language casing contract с content casing.
- Внутри SemanticTokenizerUnit.TrainOnExample читать оба поля TokenizerInput; Reject/Quarantine для пустого/unsupported SourceLang вместо неявного общего key. При Tokenize использовать exactly same canonical language function.
- Сохранить текущий ILearningUnit ownership: SemanticTokenizerUnit владеет learned pattern/signature; TranslationMachine — временем обучения и atomic publication; TrainedTranslationAdapter — только использованием. Не вводить независимый второй tokenizer state per language, который потребует двух несовместимых checkpoints.
- Input: TokenizerInput(RawText, SourceLang) и independent approved TokenizerOutput(Pattern, Signature); output: SemanticTokenSequence. Training update: один валидный пример меняет только адрес соответствующего языка, другие языки неизменны.
- Acceptance: **NAMED_CHECK M26-LANG-01**; WRONG — две записи с одним raw, разными языками дают один последний signature; CORRECT — каждая возвращает собственную выученную signature при любом train order. Negative — same-lang normalization remains stable. Read-only old artifacts.
- Sets: прежние training rows не считать автоматически законными для новой source identity; пересобрать только из сохранённых проверенных source+language. Tests: 2 training orders, 2 distinct raw controls, 1 zero-trained, 1 cold restore and 12-case independent consumer panel.

**Отвергнутые shortcuts:** добавлять литерал "|es" в строку без typed key / версии checkpoint; ослаблять law verifier; дублировать все шесть Unit без lifecycle; исправлять слепо глобальным StringComparer.Ordinal без законной миграции.

## Code quote → смысл → конкретная правка: N26-TAGS-02

**Сейчас:**  
[TranslationSemanticTokenizer.cs:118–147](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationSemanticTokenizer.cs#L118-L147) добавляет RuleTags к PrefixSignature и StructureSignature в base.  
[SemanticTokenizerUnit, TranslationTrainedUnits.cs:91–121, 145–164](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L91-L164) при correction строит ключ заново из только Concepts/POS.

~~~text
current corrected prefix    = PATTERN | first two Concepts
base prefix                = PATTERN | first two Concepts | RuleTags(first 3)
current corrected structure = PATTERN | POS sequence
base structure              = PATTERN | POS sequence | RuleTags
~~~

**Почему плохо:** обученный контекст может иметь другой retrieval address, чем training-side KNN AddPair с base tokenizer. Это контрактое несоответствие, но пользовательский эффект зависит от того, достигнут ли KNN после exact-source/phrase/word map.

**Должно стать (отдельный patch, НЕ вместе с N26-LANG-01):**
- Один общий pure signature composer TranslationSemanticSignatureComposer.Build(pattern, tokens, ruleTags) с выходом full/prefix/structure; base Tokenize и SemanticTokenizerUnit correction/rebuild используют одинаковое правило включая последовательность тегов и количество.
- Existing SemanticTokenSequence сохраняет RuleTags; re-builder меняет Pattern и согласованную сигнатуру, но НЕ выкидывает контекст. При исправлении самого анализа Tokens — отдельный versioned learnable Unit design, не подмена одной pure helper-функцией.
- Предметная KNN-память принадлежит ExecutorUnit и TranslationKnnStore; не отдавать helper права на positive/negative semantic ranking. После изменения signature schema требуются пересборка индекса из разрешённых исходных train examples и совместимый snapshot, если эксперимент подтвердит обращение туда.
- **NAMED_CHECK M26-TAGS-02**; WRONG — base.RuleTags nonempty, after trained correction prefix/structure лишены их; CORRECT — сохранена та же последовательность RuleTags с применённым Pattern. Positive/negative controls: feminine vs masculine RU source, negation, unknown tags, exact map masks KNN.
- API tests: direct Unit signatures, exact/phrase/KNN branch trace, reload parity и независимый semantic-law evaluator. Если нет KNN path — STATE_KEY_PARITY_ONLY, не GENERALIZATION_GAIN.

## N26-RESTORE-04: доказательство о потерянном языке внутри checkpoint

**Первичный record:** [TranslationCheckpointContracts.cs:69–94](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationCheckpointContracts.cs#L69-L94) сохраняет SemanticTokenizerSignatureCorrectionCheckpoint( NormalizedRawText, Pattern, TokenizerSignature ), **без SourceLang**. [TranslationTrainedUnits.cs:178–220](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L178-L220) экспортирует/восстанавливает этот же key. [TranslationMachine.cs:282–325, 352–377](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationMachine.cs#L282-L377) записывает SchemaVersion:1 и при ImportCheckpoint требует 1.

**Source-confirmed:** сами сохранённые данные не содержат явного исходного языка для exact correction. Если до экспорта обучали разным языкам с одинаковым text key, второй record уже вытеснен in-memory. Следовательно, cold reload из одного такого старого checkpoint **не может однозначно восстановить обе языковые записи**. Это не доказательство, что такая пара встречалась в RP01 или historical corpus; это строгая информационная потеря при указанном входе.

**Предлагаемый migration blueprint:**
- Версионировать TranslationMachineCheckpoint.SchemaVersion и SemanticTokenizerSignatureCorrectionCheckpoint (v2: SourceLanguageCanonical, NormalizedRawText, Pattern, TokenizerSignature), плюс identity digest по языку и тексту. Не менять semantics старого v1 silently.
- Для v1 без проверенных provenance rows: либо оставить legacy single-language read-only mode с явной пометкой UNVERIFIED_SCOPE, либо quarantine и запросить legal original trainer source, НЕ угадывать язык из текста или target Script.
- Если source corpus с SourceLang действительно сохранён: deterministic rebuild в новый v2 state с полной проверкой foreign language collision и новых RuleTags; отдельный checkpoint hash и test-only cold process readback.
- Проверить ExportCheckpoint→ImportCheckpoint→GetOrchestratorAsync same input/route/key under v2. Нельзя путать это с RP01 LlmModel LoRA checkpoint, его восстановление подтверждено другим экспертом на своей границе.

## Доказательная лестница и критерии review

| Ступень | Что сейчас есть | Что должен создать MICRO-26 |
| --- | --- | --- |
| Source | readback C# key, shared runtime, incomplete re-builder, v1 schema | pinned SHA и frozen fixture digest |
| Mechanism | логическое следствие кода | короткий **нативный** M26-A/B, обе последовательности train, exact output |
| Consumer | существующие caller names/порядок | реально достигнутые KNN/Verifier/Flow route IDs |
| Quality | NOT MEASURED | blind judge approved 12+ cases, matched A/B and controls |
| Persistence | v1 schema loses language key | v2 trial restore parity; legacy migration risk review |
| Independent review | NONE | named reviewer, exact reviewed commit, PASS/NEEDS_FIX |

**Plan review (author self-check, not independent approval):** One bottleneck per patch; сначала language key, потом RU RuleTags только при отдельном evidence; branch/paths/owners определены; нет обязательной Windows CI или искусственного test coverage. Current status DRAFT; final code choices не приняты без M26 results.

**Potential failure modes:** чужой checkpoint восстановил только один язык; legacy strings невозможны к однозначной миграции; новая identity меняет law scope; исправление tags без переиндексации перестаёт находить уже сохранённые exemplars; неожиданно нет active KNN path; wrong teacher labels с текущего bootstrap снова обучают копию правила; independent gold отсутствует. Любой из них блокирует quality approval, но не блокирует сам дешёвый source probe.

## Что вернётся пользователю после реального микроэксперимента

Не «все тесты зелёные», а четыре ясных ответа: сохранили ли EN и ES разные learned signatures, дошёл ли запрос до нужной learned memory, правильнее ли стал итоговый перевод на скрытых законах и одинаково ли он отвечает после cold restore. Именно эту разницу между записью данных и качественным обучением требуется подтвердить.

**Documentation result:** архитектурная правка описана; никакой собственно patch, независимый CODEX/Astra review или semantic quality gain пока не выполнены.
