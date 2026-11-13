# N28-U — Новая потеря Unicode-семантики и ложный mojibake на Ñ

**Дата: 2026-10-16.** Core pin `d5c2409b24292d361593b8262b0c54997b15f539`. **Source-confirmed, source-only proxy выполнен, native/.NET и blind quality NOT_RUN.** Речь об обучаемом `TranslationMachine/ExecutorUnit`, **не** непосредственном RP01 Student q/v-LoRA (связь с его 0/3 и 0/4 не доказана).

## Что теряется

**U1 / P1 / ES_EN:** `sí` (U+00ED, испанское «да») и канонически эквивалентное `si\u0301` (U+0069 U+0301) должны попадать в один ключ. Но `NormalizeMapKey` исключает combining mark U+0301: `si\u0301` становится `si` — другим испанским словом «если». В `_exactSourceMap` два законных урока `si→if` и `si\u0301→yes` получают **один** ключ; второй перезаписывает первый.

**U2 / P1 conditional / ES:** `LooksLikeUtf8Mojibake` срабатывает по **самому факту присутствия "Ð", "Ñ" или "Ã"**, в том числе по законной заглавной испанской `Ñ`. В особой ветке не удаляются внутренние операторы, в обычной удаляются: `SEÑAL x!=y` → `señal x!=y`, но `señal x!=y` → `señal xy`. При одинаковом значении и различии только регистра обучение и serving могут выбрать разные записи.

## Прямая цепь, зафиксированный исходник

| Участок | Доказательство |
|---|---|
| `TrainOnExample` пишет `_exactSourceMap[NormalizeMapKey(input.SourceText)] = chosen.Text`, перезаписывая прежнее значение | [TranslationTrainedUnits.cs:404–422](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationTrainedUnits.cs#L404-L422) |
| `Execute` сначала ищет тот же ключ по `token.Word`; PhraseMap/KNN идут позже | [TranslationTrainedUnits.cs:263–277](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationTrainedUnits.cs#L263-L277) |
| Фильтр `char.IsLetterOrDigit` без `Normalize(FormC)` и fallback по символам mojibake | [TranslationTrainedUnits.cs:1008–1028](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationTrainedUnits.cs#L1008-L1028) |
| Actual ordinary route: tokenizer→executor→verifier→fallback | [TranslationDirectionAdapters.cs:117–140](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationDirectionAdapters.cs#L117-L140) |
| `ES_EN/EN_ES` зарегистрированы | [TranslationMachine.cs:714–755](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationMachine.cs#L714-L755), [TranslationRoutingFlow.cs:131–157](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationRoutingFlow.cs#L131-L157) |

[Запускаемый source-proxy](28-MICRO-UNICODE-AND-SCRIPT-SOURCE-PROXY.mjs) и [сохранённый результат](28-MICRO-UNICODE-AND-SCRIPT-SOURCE-PROXY-RESULT.json) доказывают **ветвление/коллизию ключей**, а НЕ native learner или actual gain.

## История, дедупликация, scope

Прямое чтение `DesignToCode@8f20ceb8a9bfbadab2ad309226c47dd76e714548` показывает ту же функцию: **унаследовано**, не доказано, что это регрессия от RP01. В `language_translate` такого C# файла по пути нет; parity другой реализации **не проверена**.

[16 — стирание операторов](16-INDEPENDENT-TRANSLATION-UNIT-AND-KNN-LEARNING-GAPS-2026-10-16.md) и [21 — symbol-blind отбор](21-NEW-GAPS-SYMBOL-BLIND-LESSON-ADMISSION-AND-DIRECTION-MISROUTING-2026-10-16.md) уже описаны; **новый** фактор — NFD/NFC семантическая коллизия и ложный ветвитель по испанской `Ñ`. Применимость прежде всего ES_EN и source с комбинированными диакритиками; для C#/ENRU RP01 neural-only **NOT_APPLICABLE пока caller не доказан**. Нельзя приравнивать отдельные буквы русского `ё/е` к NFC/NFD.

## Проверка, способная опровергнуть эффект

[EXP-28-U1/U2](28-MICRO-UNICODE-AND-MIXED-SCRIPT-LEARNING-EXPERIMENTS-2026-10-16.md) — два **разных** однофакторных native A/B на 2 уроках, source-disjoint exam и cold restore. Проверять real `TranslatedText`, collision_count и false positives, а не «тест зелёный». Одновременная правка Unicode и операторов сделает вывод невалидным.

**Обязательные роли:** CODEX с Astra 6 UltraCode выполняет человеческий выбор случая; разрешённая внешняя открытая китайская модель порождает **обе** стороны DPO chosen/rejected; CODEX с Astra 6 UltraCode независимо оценивает качество пары и отдельное качество Student. Никакая из этих ролей здесь не выполнялась.
