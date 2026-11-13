# N29-B — новая *причинная* гипотеза: ложное выравнивание первых слов загрязняет обучаемый переводческий словарь

**16.10.2026.** Core `codex/dpo-implementation@d5c2409b24292d361593b8262b0c54997b15f539`. `SOURCE-CONFIRMED HEURISTIC / PRODUCED-MAPPING PREDICTION / NATIVE C# NOT_RUN / END-TO-END QUALITY NOT_PROVEN`. Это дополнение к уже существующему исследованию [core 13-domain-context-and-dpo-subunits.md](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/13-domain-context-and-dpo-subunits.md), которое **уже замечало сам факт** first-word heuristic. **Не выдавать обнаружение строки кода за впервые найденное**: новое здесь — опровержимая модель её *вредного эффекта при изменении порядка слов и обратном порядке поступления уроков* + независимый микроэксперимент. Core research каталог — только чтение.

## Человеческое объяснение

Когда перевод правильный, это **не значит, что первое слово исходного предложения соответствует первому слову перевода**. В естественных языках субъект, дополнение и глагол могут перемещаться. Текущая программа автоматически записывает пару `first source token → first target word` как обучаемое лексическое правило, если нет более ранней записи того же ключа. Поэтому два **оба правильных** перевода могут обучить словарь противоречащим значениям. Потом более поздняя запись перезатирает предыдущую: это не ошибка LoRA, а скрытый детерминистический этап извлечения pseudo-label.

## Исходники с точной ответственностью

1. [`TranslationStructuredBatchBuilder.BuildWordMappings`, строки 328–394](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L328-L394) добавляет `firstSourceToken = ... FirstOrDefault(POS != PREP/CODE_IDENTIFIER)`, `firstTargetWord = ExtractFirstTargetWord(chosen)`, после чего `AddMapping(firstSourceToken.Word, firstSourceToken.Concept, firstTargetWord)`. Проверка `seen` запрещает конфликт *внутри одной пары*, но не проверяет подтверждённое bilingual alignment.
2. [`ExtractFirstTargetWord`, строки 471–483](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L471-L483) просто возвращает первое Regex-слово выбранного ответа. [`IsTargetWordCompatible`, строки 480–495](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L480-L495) проверяет направление/письменность, но **не семантическую связь** слов.
3. [`BuildGeneralBatch`, строки 90–125](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L90-L125) помещает результат в `ExecutorOutput.WordMappings`; [`ExecutorUnit.TrainOnExample`, строки 399–456](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationTrainedUnits.cs#L399-L456) принимает записи и вызывает `AddWordMapping`.
4. [`ExecutorUnit.AddWordMapping`, строки 744–755](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationTrainedUnits.cs#L744-L755): словари `_wordMap[(word, concept)] = target` и `_surfaceWordMap[word] = target` **заменяют** прежнее значение, без evidence support, контекстного POS-окна или проверки альтернативных переводов.
5. [`ExecutorUnit.Execute`, строки 264–330](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationTrainedUnits.cs#L264-L330): перед KNN может сработать `TryComposeFromWordMappings`; фильтр >=2 сопоставлений и coverage>=0.5 означает, что видимость пользовательского дефекта **условна**, поэтому нужен consumer trace. [`ImportCheckpoint`, строки 512–547](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationTrainedUnits.cs#L512-L547) переносит получившуюся WordMap и SurfaceMap при сохранении; если правила допуска изменились между версиями, возможна дополнительная фильтрация при import, проверять отдельно.

## Небольшой мысленный контрпример — НЕ измеренный C# output

В RU→EN возьмём два **правильных, но инвертированных по порядку слов** урока с явным наречием `долго` (оно входит в `RuAdverbAllowList` настоящего токенизатора, [строки 88–96](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationSemanticTokenizer.cs#L88-L96)):

| Источник | Правильный перевод | Потенциальный ошибочный positional mapping |
| --- | --- | --- |
| `Книгу долго читал я.` | `I read the book for a long time.` | `книгу → I` |
| `Книгу долго читал он.` | `He read the book for a long time.` | `книгу → He` |

**Почему выбраны именно эти строки:** менее тщательно выбранные `Книгу я прочитал` / `Книгу он прочитал` **не изолируют positional heuristic**: [`ExtractSourceMarker`, строки 517–525](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L517-L525) сам может взять `Книгу` (NOUN/UNKNOWN), а [`ExtractRequiredTerm` и `ExtractProductMarker`, строки 181–188, 258–267](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L258-L267) также дают first target word. Эта **более ранняя marker-эвристика** тогда создаёт тот же ложный ключ до first-position, и ablation positional ничего не показывает. Наречие `долго` должно занять `ExtractSourceMarker`; `Книгу` остаётся для отдельного positional fallback. **Первым условием native L0** остаётся фактическая проверка POS/RuleTags, origins и формируемых `WordMappings`.

У source `Книгу` noun/unknown concept, ни `I`, ни `He` не являются его значением. Если раньше не построен иной mapping именно для этого token/concept, positional heuristic создаёт ложную запись; `ExecutorUnit` может перезаписать её последней. Более ранняя marker-эвристика при этом способна записывать `долго→I/He` — **отдельный источник загрязнения**, оставляемый идентичным в A/B и показываемый как конфаундер. Потребитель может скрыть ошибку своим exact/phrase/KNN приоритетом.

**Далее** закрытый source-disjoint запрос `Книгу долго читали мы.` → `We read the book for a long time.` (перед запуском утвердить независимый gold): оценщик проверяет subject/role, не буквальное совпадение строк. Exact-source train ответы **исключены** из primary оценки.

## Чем это отличается от уже известных выводов

- [N27-ROLE](27-NEW-ROLE-BLIND-MORPHOLOGY-AND-QUOTE-NEGATIVE-COLLAPSE-2026-10-16.md): ложные гендерные `-л/-ла` и marker из последнего слова TargetPrefix — **другие** эвристики. Их микроэксперимент не измеряет first-word source/chosen misalignment.
- [Core 13-domain-context-and-dpo-subunits](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/13-domain-context-and-dpo-subunits.md): уже перечислял саму first-word heuristic; здесь впервые предлагается **отдельная двухурочная перестановка, state proof и falsifiable ordinary held-out**. Не заявляется абсолютная новизна наблюдаемого кода.
- [N23 POS context](23-NEW-E26-POS-CONTEXT-TO-LEARNED-UNIT-2026-10-16.md): потерянные признаки grammatical-role/POS, а не доказанное двуязычное word-alignment.
- Исторические E09/E13/E26 помогают увидеть пользу KNN/контекстных словарей, но **не доказывают**, что извлечение этой positional heuristic было успешным историческим механизмом. Сравнивать нужно фактическую old branch и реальные paired tasks; результат `DesignToCode` содержит ту же `BuildWordMappings` схему, что подтверждает **унаследованный** seam, не «испортил RP01».

## Применимость и тест

Приоритет **P1 conditional для перевода с переставленным порядком слов**, особенно RU→EN, Hebrew→EN и прочих направлений с topikalization; **P3 для строго позиционных машинных ключей**, где first token действительно должен совпадать с first output token, и **N/A для прямого LoRA DPO**. Нельзя механически выключить её для всех доменов без проверки.

Микроэксперимент: [N29-MICRO](29-MICRO-REJECTED-KEY-AND-WORD-ORDER-CAUSAL-EXPERIMENT-2026-10-16.md), **Arm B**. Заморозить 2 урока и 8–12 source-family-disjoint контрольных примеров, сравнить current first-token heuristic с one-factor вариантом «позиционный fallback не создавать, только explicit / high-confidence contextual matches». Кандидат обязан иметь ту же исходную память, строки lessons, unit/flow, quality rubric; цель — **меньше ложных словарных правил и больше семантически верных ответов**, не `tests pass`.

**Никакие native .NET, real model, independent CODEX Astra 6 и quality AB не запускались.** Описание воспроизводит читаемый код, а не выдаёт мысленный пример за экспериментальный успех.
