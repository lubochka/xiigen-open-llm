# E16 — выполненный короткий shadow probe кодирования (НЕ запуск .NET)

**Дата:** 2026-10-16; `source_pin=llm_mvp_core@83590a462b2f8aedc9d3ab4cb775193eb9c25ce5`; **status:** `PYTHON_SHADOW_EXECUTED_4_OF_4 / NATIVE_DOTNET_NOT_RUN / TRAINING_NOT_RUN / QUALITY_GAIN_NOT_MEASURED`.

**Что исполнялось на самом деле:** независимый Python mini-reproducer по формуле из `27.TranslationDomain/TranslationDpoTrainerBridge.cs`: Unicode letter/number regex для данных англоязычных строк, NFC, lower, `SHA256`, первые 4 байта little-endian (`BitConverter.ToUInt32` на x86/x64), `id=4+(value%508)`; `ContextLength=64`, `BOS/SEP/EOS=1/2/3`, `prefixBudget=min(tokens,30)`, `responseBudget=61-prefixBudget`, padding0. Воспроизведён конкретный `BuildPromptPrefix` для того же направления/Unit на обеих сторонах. **Это shadow implementation источника, а не вызов actual C# binary**. Source code: [BuildRecord/EncodeSide](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationDpoTrainerBridge.cs#L180-L251) и [ToStableTokenId](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationDpoTrainerBridge.cs#L331-L344).

## Замороженные 4 случая и результаты

| Probe | Raw responses разные? | Первые token IDs | После Model encoding (полные массивы) | Raw SHA prefixes | Encoded SHA prefixes |
| --- | --- | --- | --- | --- | --- |
| HASH01 `accept` / `invalid` | да | 289 / 289 | **одинаковые** | `c125d039` / `f1234d75` | `9bde5bde` / `9bde5bde` |
| HASH02 `true` / `unauthorized` | да | 421 / 421 | **одинаковые** | `b5bea41b` / `e9d83f01` | `401e0e62` / `401e0e62` |
| TRUNC01 70×`neutral` + `allow/deny` | да | 322 / 322 | **одинаковые**: после 33 из 71 response-токенов suffix исчез | `48f8c7be` / `d1b3c4e5` | `9730aced` / `9730aced` |
| CONTROL01 `allow` / `deny` | да | 497 / 352 | **разные** | `41008373` / `3026a0ca` | `c9728808` / `f0771eb1` |

**Результат source-faithful prediction check:** `4/4`. Принципиально: модельный вход с обеих сторон идентичен в HASH01, HASH02 и TRUNC01. Контрольная пара различима. Длина общего prefix после regex — 28, все 28 сохранены в этом probe; `responseBudget=33`. Точное число kept tokens зависит от длины prefix; нельзя рекламировать универсально «31 response токен» без фактического counter.

## Уровень утверждений

**Доказано этим исполнением:** расчёт SHA256-хешей по версии source и логика ограничения контекста приводят к предсказанным равенствам на четырёх строках. **Ещё не проверено:** те же массивы из native `TranslationDpoTextFeaturizer.BuildRecord`, градиент, количество реально ущербных пар в одобренном corpus, наличие этого пути в неудачном RP01, качество на source-disjoint заданиях после исправления.

**Следующая дешёвая стадия:** выполнять только L0 native C# по [протоколу E16](18-E16-PAIR-CONTRAST-SURVIVAL-MICROEXPERIMENT.md), а L1 только если есть разрешённые парные примеры. Оригинальный Python trace доступен как самостоятельный репродуктор, но результат нельзя переименовать в C# или neural-learning эксперимент. Соседнее открытие [16 translation bridge erasure](16-TRANSLATION-DPO-BRIDGE-TOKEN-ERASURE-2026-10-16.md) описывает тот же класс source defects; не считать дважды.
