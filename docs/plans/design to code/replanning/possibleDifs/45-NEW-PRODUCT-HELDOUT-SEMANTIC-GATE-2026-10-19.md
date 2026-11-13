# N45 · Два новых source-level blind spot в ProductHeldOut: смысл неправильный, а оценка положительная; supplementary emoji выпадают из метрики

**19.10.2026. Классификация:** N45-A = SOURCE_CONFIRMED_ACCEPTANCE_RULE / QUALITY_IMPACT_NOT_RUN; N45-B = SOURCE_CONFIRMED_UTF16_PREDICATE / DOTNET_NOT_RUN / QUALITY_IMPACT_NOT_RUN. Это исследование другого оценщика, **TranslationProductHeldOutRunner**, не исторического TatoebaPilotLoopRunner и не RP01 LoRA evaluator. Branch pin: [codex/dpo-implementation @ a392b6be](https://github.com/lubochka/llm_mvp_core/tree/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6). Repo core — исключительно READ ONLY.

## Почему это важно для восстановления language_translate

Нельзя верно установить, удалось ли восстановить историческое полезное обучение, если «успешный перевод» в итоге измеряется лишь алфавитом, отсутствием слова BOOTSTRAP и несколькими заранее указанными строковыми маркерами. На EN↔RU отрицание, обязанность/возможность, агент действия и направление перевода могут быть неверны при наличии всех required words. Такая метрика особенно опасна при сравнении old kNN/wordmap с новой LoRA, потому что разные механизмы могут одинаково проходить несемантический gate. Этот документ **не утверждает**, что конкретный RP01 C#0/3 или ENRU0/4 возник из этой метрики: RP01 direct Student имеет собственный evaluator.

## N45-A (NEW CANDIDATE, P0 for evaluation): ProductMatch не использует ExpectedTranslation

**Улика 1 — вход содержит ожидаемый текст.** [TranslationProductHeldOutRunner.cs L15–26](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain/TranslationProductHeldOutRunner.cs#L15-L26): case имеет ExpectedTranslation, RequiredMarkers и опциональные ForbiddenMarkers/EmojiMarkers/NamedEntities.

**Улика 2 — реальный consumer получает ответ обычного Flow.** [L125–158](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain/TranslationProductHeldOutRunner.cs#L125-L158): создаётся fresh TranslationMachine, structured training, GetOrchestratorAsync, затем FinalHumanResult. Это измерение **живого flow после обучения**, но не proof, что deployed cold-restored checkpoint даёт тот же ответ.

**Улика 3 — два различных качества отчёта.** [L158–198](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain/TranslationProductHeldOutRunner.cs#L158-L198):
- ExactMatch реально сравнивает normalized ActualTranslation и ExpectedTranslation;
- BLEU1/ChrF1 тоже измеряются отдельно;
- **ProductMatch** определяется только !bootstrap, всеми RequiredMarkers, отсутствием ForbiddenMarkers, допустимым target script, !copiedSource, сохранением перечисленных emoji/имён;
- **ни ExactMatch, ни BLEU1, ни ChrF1, ни семантическая законность перевода в ProductMatch не входят**.

**Улика 4 — gate использует слабый показатель как основной.** [L205–265](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain/TranslationProductHeldOutRunner.cs#L205-L265) требует RuntimeProductAccuracy >= 0.95, но не требует порога для RuntimeExactAccuracy/BLEU/ChrF. Аналогичная формула находится в multi-direction branch [L340–416](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain/TranslationProductHeldOutRunner.cs#L340-L416). Нет независимого подписанного семантического судьи на этом shown path.

**Строгий аналитический контрпример к формуле (НЕ output модели):**
- input EN: “Do not delete the file”; expected RU: “Не удаляй файл”; RequiredMarkers: [“файл”]; ForbiddenMarkers, EmojiMarkers, NamedEntities: пустые.
- actual RU: “Удаляй файл” (противоположное действие), без слова BOOTSTRAP.
- target script = RU, source EN, marker “файл” присутствует, источник не скопирован. **ProductMatch = true**, **ExactMatch = false**, корректность закона = false. Результат непосредственно следует из набора условий; native .NET runner здесь не выполнялся.

Этот пример не доказывает, что реальные heldout markers недостаточны для всех кейсов. Если для каждой пары RequiredMarkers/ForbiddenMarkers заранее доказывают всю семантику и покрывают все ошибки, риск на таком замороженном наборе может не проявиться. Но даже тогда метрика *не проверяет эту достаточность*: это должен делать отдельный versioned domain-law judge.

**Новая гипотеза N45-A1:** когда члены пары отличаются полярностью, подлежащим, порядком аргументов или модальностью, ProductMatch переоценивает качество по сравнению с независимым semantic-law score, даже после активации реальной trained kNN memory.

**Новая гипотеза N45-A2 (scope):** запуск ProductHeldOutRunner с fresh train перед evaluation может давать оптимистичную цифру относительно cold-restored deployed artifact, потому что [L132–139, L201–204](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain/TranslationProductHeldOutRunner.cs#L132-L139) сначала обучает in-memory машину и только **после** evaluation экспортирует checkpoint; restore→ordinary-consumer на этой же панели не показан. Само различие «свежий training vs cold restore» уже поднималось ранее для других путей, поэтому считать A2 **уточнением evidence coverage**, не новой общей архитектурной причиной.

### Нативный микроэксперимент N45-SEMANTIC-GATE-01

**Домен EN→RU и RU→EN, 12 контрастов**, 6 пар законов: not/delete, may/must, actor vs patient, before/after, singular/plural, technical bank/bank; 2 независимые source families на каждый закон. Заморозить gold/rubric до вызова evaluator. Train 4–8 разрешённых коротких уроков через existing TranslationMachine; две группы holdout: (a) source-disjoint перевод; (b) adversarial **frozen candidates**, намеренно нарушающие один закон, при правильном алфавите и required-marker.

A — точное вычисление ProductMatch по нынешним условиям. B — параллельная независимая проверка сохранения полярности/агента/квантора/контекста с locked semantic-law rubric (не подставлять verdict в сам ответ и не менять обучение). C — замена output на заведомо неверный candidate того же алфавита и маркеров (negative control). Отдельно frozen correct positive control. На всех предъявлениях фиксировать source, expected, actual, every marker, exact, BLEU, chrF, ProductMatch, law-correct, использованный adapter, neighbor evidence, duration.

**Primary:** false-positive semantic acceptance count / number of negative contrasts; secondary: positive acceptance, abstentions, per-domain direction, old-law retention, bootstrap, latency. **Доказательство регрессии качества** появится только если обычный trained Flow генерирует такие ответы и ProductMatch поощряет их; static shadow counterexample доказывает только слепую точку оценщика. Критерий остановки: если ProductMatch и семантический вердикт совпадают на всех 12 замороженных предъявлениях, локальная гипотеза эффекта не подтверждена (не делать глобальный вывод о безошибочности).

## N45-B (NEW CANDIDATE, P1): показатель EmojiPreservationAccuracy пропускает большинство supplementary Unicode emoji

**Кодовые основания:**
1. [L167–170](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain/TranslationProductHeldOutRunner.cs#L167-L170): EmojiPreserved = All для предоставленного EmojiMarkers. Если список пуст, логически true, даже когда emoji во входе утрачен.
2. [L211–218](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain/TranslationProductHeldOutRunner.cs#L211-L218): subset “emojiCases” выбирается по HasAnyEmoji(SourceText), пустой subset даёт accuracy 1.0.
3. [L581–582](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/27.TranslationDomain/TranslationProductHeldOutRunner.cs#L581-L582): проверяется foreach(char ch) + Char.GetUnicodeCategory(ch) == OtherSymbol. Но UTF-16 char не является полным Unicode scalar для U+1F44B (“👋”), U+1F600 (“😀”): обе половины surrogate pair имеют категорию Surrogate, **не OtherSymbol**. BMP U+2603 (“☃”) распознаётся. Это механическое следствие UTF-16, не измерение конкретного native runner.

**Выполнен лишь отдельный source-logic proxy**, не .NET тест: для “👋” UTF-16 code units [D83D, DC4B], Unicode categories [Cs,Cs]; для “☃” [2603], [So]. Это объясняет, почему в обычном C# цикле по char флаг HasAnyEmoji отличается. Реальная строка с составными emoji/ZWJ/variation-selector требует полноценного Rune/Unicode sequence-aware контракта, а не только простой замены enum.

**Гипотеза N45-B1:** по source “Привет 👋” и actual “Hello” без 👋 с явным EmojiMarkers=[“👋”] item EmojiPreserved=false, но общий EmojiPreservationAccuracy может остаться 1.0, потому что этот item не включён в emojiCases. По source “Привет ☃”, marker ☃, counterpart считается в denominator и нарушение уменьшает метрику.

**Гипотеза N45-B2:** если EmojiMarkers пусты, та же потеря 👋 вообще не уменьшает ProductMatch из-за vacuous All. Нужно различать отсутствующую спецификацию ожидаемых emoji и Unicode classifier.

### Нативный микроэксперимент N45-UNICODE-EMOJI-01

Минимальный isolated .NET harness под той же сборкой: HasAnyEmoji-equivalent на “☃”, “👋”, “😀”, “❤️”, “👩‍💻”, затем existing ProductHeldOutRunner на 4 direction-appropriate heldout cases с **frozen** входом/ответом и фиксированными EmojiMarkers. A=current source filter; B=shadow Rune.EnumerateRunes + категориальное распознавание scalar/sequence, без изменения trained model. Cases: supplementary missing, BMP missing, complete retained, emoji-free. Основные метрики: inclusion/denominator, false-pass emoji-rate, case ProductMatch, false abstention. Для честного результата отличать изменения метрики от обучения; в новом dataset с другими emoji проверять поведение дополнительно. **Native test не запускался; нет права говорить, что production чек был пройден/провален.**

## Почему это действительно другой объект, чем прежние метрики

- [protected 22 — TatoebaPilotLoopRunner metric](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/22-tatoeba-primary-review-and-domain-scope.md) анализирует **TatoebaPilotLoopRunner ProductMatch** с exact/similarity. Нынешний N45-A — **другой класс ProductHeldOutRunner**, с другой формулой без expected similarity.
- [protected 58 — unit metrics](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/58-unit-metrics-and-generation-consumer-boundaries.md) уже отмечает fresh batch и coverage-as-accuracy в этом классе. Это ранее известный аспект, здесь он не выдан как новый.
- Соседние [planning N26](26-NEW-DIFFERENCES-ROLE-POLARITY-AND-DIRECTION-METRIC-2026-10-16.md) и [planning N23](23-INDEPENDENT-LANGUAGE-ROUTING-POS-AND-CROSS-CLAUSE-GAPS-2026-10-16.md) посвящены другим failure mechanisms. В просмотренных первичных сводках не найден exact ProductMatch/emoji-utf16 case; **полное отрицание повторов по всем сотням документов не доказано**.

## Приёмка исследования и безопасность утверждений

- **C (code/source)**: формулы ProductMatch, emoji subset и Unicode-char guard непосредственно прочитаны на pinned SHA.
- **P (logic proxy)**: аналитические контрпримеры и UTF-16 категориальная проверка; **не native product run**.
- **H (causal impact)**: отсутствует; нужны native Flow и проверенные source-disjoint semantic labels.
- **M (measured quality gain)**: отсутствует; нельзя вписывать +XX% до реального A/B.
- **Ремонт после causal confirmation:** разнести ProductMatch (checks/protocol) и SemanticTranslationCorrectness (law-based adjudication) как отдельные scored fields с жёсткой рубрикой и версии judge; emoji detection переделать на Unicode scalar/sequence, сохранить fallback/coverage metrics в диагностике, не переписывать прежние receipts/thresholds.

Связанные документы: [главная историческая глава](44-LANGUAGE_TRANSLATE-SUCCESS-FIRST-RECOVERY-2026-10-19.md), [порядок быстрых опытов](46-MICRO-LANGUAGE-TRANSLATE-RESTORATION-AND-EVAL-2026-10-19.md).