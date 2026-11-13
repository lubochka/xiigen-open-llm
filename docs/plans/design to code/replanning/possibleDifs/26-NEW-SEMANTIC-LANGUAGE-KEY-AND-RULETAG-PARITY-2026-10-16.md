# 26 — Новые точечные расхождения: языковая идентичность исправления и потеря RU rule-tags на границе SemanticTokenizer → KNN

<!-- POST107-ROOT-AUDIT-2026-11-02:START -->
## Уточнение после P107 — 2 ноября 2026

**Нормализация должна сохранять признаки языка и грамматики.** В P103 приведение к нижнему регистру теряет пять из шести маркерных форм; получатель определяется до этой потери. В P106 исправленные Pattern и Signature дают 2/2, но «проверь» остаётся существительным, а «файл» — глаголом. Поэтому проверяются роли и леммы конкретных слов, отрицание, время и смысловой ключ. Присутствия ACTION/OBJECT где-либо в строке недостаточно.

PairId хранится как происхождение урока; RULE после него сохраняется при миграции. Добавление, удаление или замена учебного номера при той же семантике не меняет основание допуска. Неподдержанная морфология остаётся явно неизвестной, без выдуманного времени.

Положительные и отрицательные минимальные пары проходят обычный tokenizer, semantic unit и verifier. Порча роли конкретного токена должна быть обнаружена при прежней длине массива. Пунктуационные 0/2 P106 сохраняются как ограничение результата; точные обучающие контроли не называются обобщением.

Основание: [POST107-аудит](codex-2026-11-02-POST107-TRANSFER-AND-EXPERIMENT-AUDIT.md) и [проверка реального кода](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-123-post107-transfer-and-experiment-audit-2026-11-02.md). Исторические результаты и бюджеты сохранены; это условия будущего переноса, не новый запуск.
<!-- POST107-ROOT-AUDIT-2026-11-02:END -->


**Дата:** 2026-10-16. **Статус:** SOURCE-CONFIRMED CONDITIONS / QUALITY-IMPACT HYPOTHESES / NATIVE EXPERIMENT NOT RUN / INDEPENDENT REVIEW NOT RUN.  
**Область:** структурные переводческие Units (27.TranslationDomain); **не** прямой DPO→LoRA→TextGenerator путь RP01.  
**Source:** lubochka/llm_mvp_core, ветка codex/dpo-implementation, непосредственно прочитанные файлы: TranslationTrainedUnits.cs blob 1742abfc9fb617dfc66e4d618e0c169da01a6ccc; TranslationMachine.cs blob fd0e736de3739236b4dde2d6b9d000f5ebae8416. **Target:** planning llm_plan3/possibleDifs. В core/docs/plans/replanning-possible-diffs НИЧЕГО НЕ ИЗМЕНЯТЬ.

## Понятно человеку

У алгоритма есть две разные идентичности: *что именно выучено* и *куда обратится программа, когда встретит такой же смысл*. В текущем SemanticTokenizer можно обучить правильную поправку, однако при применении она (А) может быть взята из другого языка с тем же написанием, (Б) может потерять признаки русского грамматического контекста, которые входили в адрес KNN при обучении.

Это **две самостоятельные условные ошибки представления**, а не доказательство того, что именно они вызвали научный результат RP01 0/3 и 0/4. Их надо проверять на реальном обычном TranslationRoutingFlow, не на правдоподобии названий.

## N26-LANG-01 (P1): ключ exact-исправления не содержит source language

**Прямые первичные свидетельства:**

1. [SemanticTokenizerUnit, TranslationTrainedUnits.cs:72–145](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L72-L145): словарь _signatureCorrections создан с StringComparer.OrdinalIgnoreCase. Обучение пишет по _signatureCorrections[Normalize(input.RawText)] = chosen. При инференсе чтение — TryGetValue(Normalize(rawText), out exactCorrection). Поле TokenizerInput.SourceLang существует, но в КЛЮЧ не входит. Normalize выполняет whitespace trim/collapse; сама case-insensitive природа dictionary добавляет коллизии.
2. [TranslationMachine.CreateRuntime:714–790](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationMachine.cs#L714-L790): один и тот же объект SemanticTokenizerUnit передаётся всем шести TrainedTranslationAdapter — RU_EN, EN_RU, HE_EN, EN_HE, ES_EN, EN_ES. Состояние не отделено по adapter или языку.
3. [TrainedTranslationAdapter.TranslateAsync:116–139](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationDirectionAdapters.cs#L116-L139): при обычном пользовательском запросе вызывается _semanticTokenizer.Tokenize(request.InputText, request.SourceLanguage); затем executor и verifier получают этот результат.
4. [BuildGeneralBatch:82–118](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L82-L118): каждая train-ready пара строит TokenizerInput(raw, sourceLanguage) и TokenizerOutput(pattern, signature), где ExactPair может добавить |PAIR:<id> к подписи. Таким образом, даже если исходные SentencePattern совпадают, точная сохранённая signature одной языковой записи может заместить signature другой.

**Фальсифицируемое предсказание.** Если обучить два semantic example с одинаковой строкой, но разными SourceLang и различающимися разрешёнными signatures, словарь сохранит лишь одну запись для данного Normalize(raw). Для обоих языков при lookup будет выдан выбранный *последний* signature. Смена порядка обучения перевернёт выбранную подпись. Это проверка source-level механики, а НЕ автоматическое доказательство неправильного конечного перевода.

**Возможный путь ухудшения:** чужая signature меняет адрес последующего KNN-каскада и/или [QualityVerifierUnit.Verify, строки 1052–1094](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L1052-L1094), который проверяет _requiredTermsBySignature по sourceTokens.Signature. При наличии независимого закона для той подписи возможен чужой ложный FAIL или пропуск собственного требуемого слова; затем [adapter](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationDirectionAdapters.cs#L116-L139) может уйти в fallback. Это только **условный путь**: он требует реальных конфликтующих записей, соответствующего обученного закона и фактического consumer call.

**Противоулики / границы:** для разных направлений ExecutorUnit отдельный, поэтому сам факт общего tokenizer НЕ означает смешения его exact-source словарей. RU/EN или ES/EN запреты на целевой script могут отфильтровать некоторые ошибочные KNN-кандидаты. Большинство обычных английских/испанских токенов могут получить одинаковый bootstrap pattern; тогда функциональный эффект ограничится signature/verification, а не изменением pattern. Обнаруженная проблема *не* равна уже известной RU против ru (регистру поля source_language в tokenizer): здесь правильно написаны оба языка, но ключ их вообще не содержит.

**Контрфактическая корректировка для эксперимента:** индекс исправлений по (canonical sourceLang, NormalizedRawText), не по одной строке; миграция legacy checkpoint записей явно требует решения о языке и не может безошибочно угадывать его из старого ключа. Не менять public checkpoint contract до измерения.

## N26-TAGS-02 (P1/P2): обученное исправление удаляет RuleTags из prefix/structure retrieval keys

**Прямые первичные свидетельства:**

1. [TranslationSemanticTokenizer.Tokenize:111–155](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationSemanticTokenizer.cs#L111-L155): base tokenizer вычисляет ruleTags; для RU BuildRuleTags возвращает контекстные маркеры, например RU:SUBJECT:...; добавляет их в Signature, PrefixSignature (до трёх тегов) и StructureSignature.
2. [SemanticTokenizerUnit.Tokenize:89–121, BuildPrefixSignature/BuildStructureSignature:150–163](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L89-L163): ветка exact-correction и ветка changed-pattern rebuild вызывают внутренние BuildPrefixSignature и BuildStructureSignature. Первая использует только первые два известные Concepts, вторая — только POS. Ни одна не присоединяет seq.RuleTags.
3. [ExecutorUnit.TrainOnExample:404–433](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L404-L433): обучение пишет пары в общий TranslationKnnStore под тремя адресами: example.SemanticSignature; затем prefix/structure из **свежего базового TranslationSemanticTokenizer** с RuleTags.
4. [ExecutorUnit.TrySelectKnnCandidate:333–365](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L333-L365): обычный KNN lookup использует tokens.Signature/PrefixSignature/StructureSignature из **обёрнутого** SemanticTokenizerUnit, а не заново вычисляет исходный контекстный адрес.

**Схема адресов (структурный пример, НЕ выданная моделью строка):**

~~~text
basePrefix = PATTERN|CONCEPT_A|RU:SUBJECT:FEM
unitPrefixAfterExactCorrection = PATTERN|CONCEPT_A

baseStructure = PATTERN|PRON|VERB|RU:SUBJECT:FEM
unitStructureAfterExactCorrection = PATTERN|PRON|VERB
~~~

Конкретные значения pattern, concepts, POS и имени тега для реальных фраз нужно брать из runtime, не подбирать под желаемый результат. Кодовый инвариант: в rebuild seq.RuleTags нет.

**Предсказание:** на входе с непустыми RuleTags до обучения и с зарегистрированным exact/corrected-path после него, PrefixSignature/StructureSignature изменяются **даже если выбранный Pattern тот же**. Поэтому fallback/cascade lookup по ним может промахнуться относительно адресов сохранённой KNN-памяти либо смешать контексты, которые ранее различались.

**Критическое ограничение причинности:** ExecutorUnit.Execute сначала смотрит _exactSourceMap, потом phrase/word maps, и только затем KNN. Для известной исходной строки exact-map может **полностью замаскировать** эффект потери RuleTags. При стандартном BuildGeneralBatch chosen.Pattern копирует base pattern; на незнакомой фразе без exact correction ветка changed-pattern может вообще не вызваться. Следовательно, один прямой тест разницы signatures — это баг адреса/контракта, **но не доказательство падения пользовательской точности**. Для quality effect требуется трасса, где active KNN/Verifier реально потребляет изменившуюся сигнатуру.

**Возможная адресная правка после подтверждения:** сохранить base seq.RuleTags в обоих re-builder, учитывать mismatch base-pattern/learned-pattern при построении prefixes, а при изменении learned pattern пересогласовывать индексацию KNN. Пока это проектная альтернатива, НЕ выполнение задачи.

## N26-REINDEX-03 (производная гипотеза, не отдельный доказанный баг)

При последующем самостоятельном исправлении SentencePattern base-index, собранный во время ExecutorUnit.TrainOnExample, остаётся на старом pattern/keys, а будущий SemanticTokenizerUnit строит исправленные. Видимого автоматического reindex в названных методах нет. Это **дальнейшее следствие N21-01 и N26-TAGS-02**, а не самостоятельная уникальная находка без полного поиска иных callers. Проверять лишь если в конкретном запуске имеются teacher-confirmed changed-pattern и реальный KNN consumer. Иначе не плодить дефекты.

## Проверка новизны и дедупликация с соседними агентами

| Уже документировано | В этом документе действительно новый проверяемый контраст |
| --- | --- |
| [N21 — semantic self-label и несовпадающий |PAIR key](21-INDEPENDENT-STRUCTURED-LEARNING-GAPS-2026-10-16.md) | Два SourceLang, один Normalize(raw): overwriting **между языками** при shared SemanticTokenizer; отдельная losing RuleTags ветка |
| [N23 — RU vs ru и потеря POS/межпредложенческий prevConcept](23-INDEPENDENT-LANGUAGE-ROUTING-POS-AND-CROSS-CLAUSE-GAPS-2026-10-16.md) | EN vs ES / RU vs иная SourceLang даже при **правильном регистре языка** и совпадающем raw |
| [N21/N23 — case-key collisions, silent Unit dispatch, non-atomic batches](23-NEW-UNIT-CASE-IDENTITY-AND-NONATOMIC-TRAINING-2026-10-16.md) | Не ExampleId/Trainer registry и не регистр слова, а состав ключа (SourceLang отсутствует) |
| [20 — BM25/fusion nonlocal ranking](20-LESSON-ADDITION-RERANKER-NONLOCAL-QUALITY-GAPS-2026-10-16.md) | До candidate rank: изменение lookup address из-за утратившего RuleTags собственного Unit |
| [RP01 — direct Student recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/README.md) | **Другой** structured translated Unit graph; не приписывать ему научные 0/3, 0/4 |

Это независимое уточнение после сопоставления основных индексных и тематических документов. **Утверждения, что никто из всех других агентов никогда не упоминал эти конкретные условия, нет:** 117 документов могли обновляться параллельно. Следующее ревью должно выполнить exact-text дедуп по двум ключам и seq.RuleTags на свежем HEAD.

## Доказательство vs гипотеза

| ID | Source condition | User quality | Historical regression | RP01 causality |
| --- | --- | --- | --- | --- |
| N26-LANG-01 | CONFIRMED (key + shared runtime) | NOT MEASURED | NOT ESTABLISHED | NOT ESTABLISHED |
| N26-TAGS-02 | CONFIRMED (two rebuild branches) | NOT MEASURED; may be masked by exact map | NOT ESTABLISHED | NOT ESTABLISHED |
| N26-REINDEX-03 | CONDITIONAL / needs true consumer | NOT MEASURED | NOT ESTABLISHED | NOT ESTABLISHED |

## Минимальный следующий шаг

Реализовать [26-MICRO-LANGUAGE-KEY-AND-RULETAG-CAUSAL-EXPERIMENT-2026-10-16.md](26-MICRO-LANGUAGE-KEY-AND-RULETAG-CAUSAL-EXPERIMENT-2026-10-16.md): сначала короткая source-parity диагностика без обучения модели, затем отдельно один matched A/B с реальными обученными Unit и слепым независимым переводческим законом. Запрещено возвращаться к 500 DPO/Windows GitHub matrix только для проверки этих двух швов.

## Skill/review contract

Применены как документарные рубрики: [trainable-architecture-decision](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/trainable-architecture-decision-SKILL.md) (прямой trainable vs deterministic fallback), [learning-path-audit](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/learning-path-audit-SKILL.md) (Train→Consumer), [experiment-design](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/experiment-design-SKILL.md) (paired same-split), [plan-review](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/plan-review-SKILL.md), [generated-code-review](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/generated-code-review-SKILL.md), [prompt-patch-authoring](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/prompt-patch-authoring-SKILL.md) и [human-readable-architecture-audit](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/human-readable-architecture-audit-SKILL.md). Роль независимого CODEX/Astra reviewer здесь НЕ исполнялась.

**Human-facing result:** Существуют два конкретных способа потерять обучаемые правила при переходе между языком, токенизатором и индексом. Код подтверждает условия, но пользовательскую регрессию ещё нужно измерить. Факт улучшения или исправления не заявлен.
