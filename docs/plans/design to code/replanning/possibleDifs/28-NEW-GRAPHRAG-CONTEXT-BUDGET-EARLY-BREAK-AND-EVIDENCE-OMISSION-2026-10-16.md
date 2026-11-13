# N28-H2 — Новый потенциальный разрыв: RAG нашёл релевантное доказательство, но не передал его обучающему или inference consumer

**16.10.2026 | SOURCE_VERIFIED_CONTROL_FLOW / MATHEMATICAL_COUNTEREXAMPLE / LIVE_CALL_PATH_NOT_PROVEN / NATIVE_NOT_RUN / QUALITY_NOT_RUN.**  
Core `codex/dpo-implementation@d5c2409b24292d361593b8262b0c54997b15f539`. `possibleDifs` — единственная разрешённая директория записи; `core/docs/plans/replanning-possible-diffs` **не изменять**.

## Гипотеза — именно о передаче evidence, а не о плохом BM25

Механизм retrieval может правильно найти несколько chunks и передать список в `RetrievalContext.Results`, но `BuildContext` собирает для модели иной, меньший набор. Если более высокий по рангу chunk оказался слишком длинным, метод прекращает обработку **всех следующих** chunks, включая короткие и особенно нужные. Это head-of-line blocking: история знания хранится/найдена, но не видна следующему алгоритму. В случае RAG-grounded DPO он может обучать на неполном context; в генерации — не иметь вспомогательного правила, хотя retrieval был успешен.

**Не делать ложного вывода:** в настоящее время **не доказано**, что offline RP01 `CodeDocumentationSimulationDpoExperiment` обращается к этому именно `08.GraphRag.HybridRetriever`. Если call chain отсутствует, гипотеза не может объяснить 0/3 и 0/4 RP01. Это условный межслойный риск для тех потребителей, где RAG context реально включён.

## Первичные исходники

- [`HybridRetriever.Retrieve` L29–44](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/08.GraphRag/HybridRetriever.cs#L29-L44): `Results=merged`, но текст evidence строится `BuildContext(merged)`; `TokenCount=contextText.Length/4` — оценка символами, не фактический tokenizer.
- [`MergeAndRank` L46–86](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/08.GraphRag/HybridRetriever.cs#L46-L86): список кандидатов сортируется по `CombinedScore` и ограничивается `topK` **до** бюджета context.
- [`BuildContext` L88–105](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/08.GraphRag/HybridRetriever.cs#L88-L105): `MaxTokens=3000`, бюджет оценивается `3000*4` символов; условие `if(used + snippet.Length > MaxTokens * 4) break;` — не `continue`; `AppendLine` добавляет разделители, которые не входят в `used`.
- В исторических `language_translate` и `DesignToCode` тот же `break` обнаружен при сверке GitHub blob SHA `1f71fbbee44f41b956faf75a9e3f6093f1c16ed1`, т. е. это **давний потенциальный seam**, не доказанная новая регрессия между ветками.

### Простой детерминированный контрпример (алгебра, native не запускался)

Пусть уже отсортированные `SearchResult` после форматирования `"[Type] Name\nRawText"` имеют длины `11 990`, `30`, `5` символов при лимите `12 000`.

- `r1` помещается, `used=11990`.
- `r2` не помещается: `11990+30=12020>12000`, текущий метод делает **break**.
- `r3`, чей snippet длиной 5 **поместился бы** (`11990+5=11995`), никогда не просматривается. `Results` всё ещё содержит `r3`, но `ContextText` — нет.

Если `r1>12000`, `ContextText` остаётся совсем пустым, хотя в `Results` есть следующие короткие релевантные куски. Это свидетельство о control-flow, **не измеренный loss обучения**.

## Два независимых предметных риска

**H2a — пропуск короткого доказательства за длинным шумом (P1 только при реальном RAG consumer).** Результат ранжировщика не соответствует видимым unit фактам. При последующей паре chosen/rejected CODEX Astra видит адекватный закон в library, но actual Student training prompt его не видит. Для C#/SQL особый риск у коротких доменных guards после больших файлов. Для перевода возможно выпадение direction/gender/negation law; для DTC — render acceptance notes после объёмного Figma spec. Эти domain implications гипотетические и должны проверяться по настоящей цепочке `retriever→ContextText→final exact DPO prompt/consumer`.

**H2b — `4 chars/token` и неучтённые newline (P2, conditional).** В source нет вызова `BpeTokenizer.Encode` для `ContextText`, хотя budget назван MaxTokens; в языках с другой длиной BPE на символ возможна неэквивалентная фактическая токеновая ёмкость; сейчас оценка в `TokenCount` об этом не знает. Не утверждать, что русские/ивритские токены всегда больше — измерить на pinned tokenizer.

## Быстрый фальсификатор MICRO-N28-H2

**Gate 0 (обязательный):** найти прямой production/RP01 `new HybridRetriever`/dependency injection и использование именно `RetrievalContext.ContextText`; отдельно `Results` (список найденного) и `ContextText` (реальный prompt). Если путь отсутствует, `NOT_RELEVANT_TO_RP01`, не переписывать pipeline.

**Gate 1 (native 3 cases):** без модели сформировать 3 заранее отсортированных `SearchResult`, так чтобы длины **готовых snippet**, включая `[Type] Name`, были точно 11990/30/5. Вызвать реальный `Retrieve` или выделить internal pure `BuildContext`; измерить `Results.Count`, `ContextText.Contains(r3UniqueMarker)`, actual `BpeTokenizer.Encode(context).Count`. Контрфакт — **одно** изменение `break→continue` в test-only реализации, при остальных frozen `results` и score. Второй случай: первый snippet 12001 и следующий 10; третий: первые два всегда помещаются. Не считать точные длины rawText равными длине formatted snippet.

**Gate 2 (только если доказан вызов consumer):** 8 TRAIN RAG-supported cases + 4 DEV + 12 source-disjoint heldout, два arms A `break`, B `continue`, идентичные ranked chunks/Student/checkpoint/tokenizer/teacher-pairs; CODEX Astra 6 UltraCode по blind output независимо меряет **semantic domain-law correctness**, citation groundedness, false confidence, missed positive evidence и latency. Не смешивать с одним изменением ранжирования или tokenizer.

**Acceptance:** причинное улучшение допустимо заявить только если в реалистичных cases `r3` действительно релевантен, прошёл после skip, попал в фактический prompt, и consumer **верно исправил** предметный ответ без нового закона-нарушения; pure counterexample — только code seam.

## Пересечение с уже известными difs

Это **не** [N20 pool-normalized KNN](20-NEW-FINDINGS-FUSION-MEMORY-GROWTH-2026-10-16.md): там меняются scores до выбора; здесь релевантный `Result` уже найден, а потеря происходит при **сборке prompt**. Не [N21 BPE drift](21-INDEPENDENT-BPE-RETRAIN-VOCAB-DRIFT-2026-10-16.md): текст не меняет выученный vocab. Не [N27 DPO shared prefix](27-RP01-DPO-COMMON-PREFIX-GENERATION-GAP-2026-10-16.md): там ответные y+/y−, здесь пропущенная часть **input context**.

## DPO governance

**CODEX / GPT-6 Astra 6 UltraCode** выполняет человеческий выбор учебного случая. **Внешняя разрешённая открытая китайская модель** генерирует **chosen и rejected**. **CODEX / GPT-6 Astra 6 UltraCode** отдельно оценивает качество точной пары и итогового Student ответа. Никакие native, independent judge или external teacher вызовы этим документом **не выполнены**.
