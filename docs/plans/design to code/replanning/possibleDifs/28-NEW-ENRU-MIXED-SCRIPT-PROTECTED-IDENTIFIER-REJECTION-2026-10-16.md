# N28-S — Утрата выученного перевода из-за несовместимых EN→RU фильтров латиницы

**16.10.2026 · SOURCE-CONFIRMED CONDITIONAL PATH, NATIVE QUALITY NOT_RUN**. Код `llm_mvp_core@d5c2409b24292d361593b8262b0c54997b15f539`.

## Новая причинная гипотеза

Урок «перевести текст и сохранить технический идентификатор verbatim» законен, например `Check API requests → Проверьте запросы API`. Но `ExecutorUnit` применяет два разных фильтра к **одному и тому же** корректному target:

- для `KNN` и `WordMap`: `ContainsCyrillic && !ContainsLatinLetter && !LooksLikeCopiedSource`;
- для `PhraseMap`: `ContainsCyrillic && !LooksLikeCopiedSource` — латиница допускается;
- для `exact source`: фильтр target **вообще не вызывается** до return.

Поэтому `Проверьте API` проходит PhraseMap, но отбрасывается KNN/WordMap только по ASCII `API`. Это конфликт с правилом `CODE_IDENTIFIER` — его надо оставлять в исходном виде, и сам Executor так поступает.

## Исходные доказательства

| Маршрут | Прямой код |
|---|---|
| Strict EN_RU target gate | [TranslationTrainedUnits.cs:924–940](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationTrainedUnits.cs#L924-L940) |
| Relaxed PhraseMap EN_RU gate | [TranslationTrainedUnits.cs:942–960](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationTrainedUnits.cs#L942-L960) |
| KNN проверяет strict target до выдачи `-knn` | [TranslationTrainedUnits.cs:333–364](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationTrainedUnits.cs#L333-L364) |
| WordMap проверяет strict target | [TranslationTrainedUnits.cs:366–400](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationTrainedUnits.cs#L366-L400) |
| ExactSource возвращается раньше фильтров | [TranslationTrainedUnits.cs:263–277](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationTrainedUnits.cs#L263-L277) |
| PhraseMap возвращает `-phrase-map`, adapter/Verifier решают отдельно | [TranslationTrainedUnits.cs:798–859](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationTrainedUnits.cs#L798-L859), [TranslationDirectionAdapters.cs:117–140](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationDirectionAdapters.cs#L117-L140) |
| Контракт сохраняет CODE_IDENTIFIER verbatim | [TranslationTrainedUnits.cs:295–330](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationTrainedUnits.cs#L295-L330) |

Source-only proxy ([исходник](28-MICRO-UNICODE-AND-SCRIPT-SOURCE-PROXY.mjs), [receipt](28-MICRO-UNICODE-AND-SCRIPT-SOURCE-PROXY-RESULT.json)) дал для `Проверьте API`: **strict=false, phrase=true**; для `Проверьте интерфейс`: true/true; для непереведённого `Check API`: false/false. Proxy **не моделирует** `LooksLikeCopiedSource`, selection of nearest KNN, verifier и actual regular consumer: для причинного вывода нужен native.

## Область, историческая граница, решение

**HIGH conditional:** EN→RU техническая документация, C#/API/SQL, имена `HttpClient`/`JSON`, пользовательские строки с защищёнными названиями. Возможно неприменимо для общих переводов без protected identifiers. Код присутствует также в `DesignToCode@8f20ceb8...`: это **старый унаследованный seam**, не доказанная RP01 regression. Neural RP01 напрямую не вызывает эту функцию в показанном DPO→TextGenerator маршруте.

[16 — Unit/KNN gaps](16-INDEPENDENT-TRANSLATION-UNIT-AND-KNN-LEARNING-GAPS-2026-10-16.md) и [27 — role/gender](27-NEW-ROLE-BLIND-MORPHOLOGY-AND-QUOTE-NEGATIVE-COLLAPSE-2026-10-16.md) уже исследовали другие механизмы. Здесь **новое** доказательство несовместимых target-script gates **внутри одного Executor**.

Нельзя просто удалить `!ContainsLatinLetter`: тогда незавершённый английский перевод может пройти. B-arm [EXP-28-S](28-MICRO-UNICODE-AND-MIXED-SCRIPT-LEARNING-EXPERIMENTS-2026-10-16.md) принимает ASCII латиницу только внутри **типизированных и проверенных protected spans**, не ослабляя source-copy, negation/roles и domain law. A/B на *одном и том же* already-retrieved candidate, потом обычный adapter и cold restore.

**Роли DPO:** CODEX Astra 6 UltraCode — human selection и independent pair+Student quality; внешняя разрешённая открытая китайская модель — **генерация обеих DPO сторон**. Не исполнялись.
