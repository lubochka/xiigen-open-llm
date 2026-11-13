# 16 — Новые различия: потеря DPO-контраста при формировании токенов в Translation DPO

**Дата:** 2026-10-16. **Статус:** `CODE_CONFIRMED / SOURCE-FAITHFUL HASH REPRODUCED / NATIVE_DPO_NOT_RUN / RP01_CAUSALITY_NOT_PROVEN`.
**Проверенный core:** `codex/dpo-implementation@83590a462b2f8aedc9d3ab4cb775193eb9c25ce5`; компонент `27.TranslationDomain/TranslationDpoTrainerBridge.cs` (Translation DPO legacy bridge, НЕ direct RP01 `segmented-byte-BPE-v2`). Цель — найти новую потенциально потерянную причину слабого обучения; это не подтверждённая причина именно C#500/ENRU500 RP01.

## Короткое объяснение человеку

Учитель может дать два разных ответа: хороший и плохой. Однако до градиента оба текста иногда превращаются в **одну и ту же последовательность токенов**. Оптимизатор не способен обучиться *отличию*, которого не видит. Есть два независимых пути такого разрушения: коллизии хеш-токенов и молчаливое обрезание конца ответа. Отдельно здесь живёт legacy loss по всей последовательности, а не response-only v2 loss.

## N16-A — доказанная хеш-коллизия разных ответов

- [Исходный тип настроек, строки 13–15](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationDpoTrainerBridge.cs#L13-L15): `VocabSize=512`, `ContextLength=64` по умолчанию.
- [Токенизация, строки 331–344](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationDpoTrainerBridge.cs#L331-L344): Unicode regex → lowercase/NFC → `SHA256(token)` → `BitConverter.ToUInt32(hash,0)` → `4 + (uint % (VocabSize - 4))`.
- [Создание model-visible batches, строки 180–216](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationDpoTrainerBridge.cs#L180-L216): chosen/rejected токенизируются отдельно, затем записываются в `DpoBatch`; побайтовые fingerprints считаются по исходным строкам, а не проверяют, что модели достались разные IDs.
- [Admitted pair проверяется до токенизации, строки 913–977](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationDpoTrainerBridge.cs#L913-L977): проверяются текст, авторство, паспорт и fingerprint **raw** пары; проверки `chosen.TokenIds != rejected.TokenIds` в показанном `BuildRecord` нет.

**Минимальный независимый расчёт (не C# run):** SHA256 little-endian первых четырёх байт, `id=4+(value%508)`:

| Разные слова | uint32 A | uint32 B | id A | id B |
| --- | ---: | ---: | ---: | ---: |
| `accept` / `invalid` | 969942465 | 1967989745 | **289** | **289** |
| `true` / `unauthorized` | 463781557 | 20961513 | **421** | **421** |
| `reject` / `deny` | 177819144 | 3399493168 | **352** | **352** |
| Контроль: `allow` / `deny` | 1937965121 | 3399493168 | 497 | 352 |

Следовательно, при равных prefix/context одиночные response `accept` и `invalid` дают равные model-visible token IDs, несмотря на разные исходные строки и fingerprints. **Проверенный факт:** хеш-функция теряет различие. **Условная модельная импликация:** при одинаковом полном input и выключенном dropout чистый DPO-контраст выбранной/отвергнутой стороны равен нулю; при ненулевом stochastic noise поведение не становится осмысленным preference signal. Наличие именно этих слов в реальном dataset **не установлено**.

## N16-B — доказанная возможность полного уничтожения отличия из-за truncation

[EncodeSide, строки 230–251](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationDpoTrainerBridge.cs#L230-L251) задаёт `maxBodyTokens = ContextLength - 3`, берёт `prefixTokens.Take(prefixBudget)` и `responseTokens.Take(responseBudget)`; ни ошибок, ни счётчика потерь, ни сверки отличающихся response-tail. При default `ContextLength=64` вся часть prefix+response ограничена 61 токеном.

**Конструктивный контрпример:** `chosen = repeat("neutral ", 70) + "allow"`; `rejected = repeat("neutral ",70)+"deny"`, один prefix. Последняя лексема заведомо дальше 61-го token position и не входит ни в одну сторону; оба сериализованных response ID-набора совпадают независимо от хеширования. Штампы raw текста будут разными; model-visible DPO preference может полностью исчезнуть.

**Чёткое ограничение:** это не дефект RP01 exact-v2, где [DpoExactPairMaterial.CreateBatch](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/11.DPO/Models/DpoExactPairMaterial.cs#L64-L89) отдельно проверяет `chosenIds.SequenceEqual(rejectedIds)` и ёмкость, а не silent truncation. Точный путь, где translation bridge влияет на обычный RP01/ENRU Student, требует invocation graph.

## N16-C — две разные математические цели под общим названием DPO

`TranslationDpoTextFeaturizer.BuildRecord` конструирует batch с обычными `ChosenMask`/`RejectedMask` и **не задаёт** `MaterialVersion=segmented-byte-BPE-v2` и `ChosenResponseMask`/`RejectedResponseMask`. В [DpoTrainer.TargetMask и ComputeTargetLogProbs, строки 1002–1069](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/11.DPO/DpoTrainer.cs#L1002-L1069) non-exact material берёт **общую** маску и считает **среднюю** по всем немаскированным следующим токенам, тогда как exact-v2 берёт response-only sum. Это подтверждённое Objective/Framing отличие; причинное влияние на конкретный run не измерено. При неравной длине chosen/rejected даже общий prefix получает различающий вес нормировки.

## Риск, степень доказательства и связь с предыдущими аудитами

| ID | Source/code | Уже доказано | Ещё не доказано |
| --- | --- | --- | --- |
| N16-A | C | Много разных UTF-8 слов отображается в один ID; приведены конкретные вычисления | Что эти collisions встретились в frozen DPO train и ухудшили ordinary translation |
| N16-B | C | Разные raw ответы могут стать равными после усечения | Частота таких пар в фактическом dataset |
| N16-C | C | Данный bridge использует legacy mean/full-sequence objective, в отличие от RP01 exact-v2 | Численный градиент, роль этого bridge в RP01 и прирост качества после исправления |

**Не дублирует:** общий DIF-10 / token economy, DIF-21 / legacy objective, DIF-22 / boundary, DIF-23 / gradient и sibling branch DPO schema. Новизна — два *конкретных модельно-невидимых preference-pair counterexample* в **действующем translation bridge** и bounded эксперимент. Не менять прежние результаты и не называть эту находку причиной деградации без train/consumer trace.

**Проверка:** [18 — E16 быстрый причинный опыт](18-E16-PAIR-CONTRAST-SURVIVAL-MICROEXPERIMENT.md); [JSON fixture](18-E16-PAIR-CONTRAST-FIXTURE.json). **Другой слой:** [17 — пропуск обучаемого Unit](17-STRUCTURED-UNIT-TRAINING-SILENT-DISPATCH.md).

## Сверка с параллельным агентом (не удваивать доказательства)

В соседнем [16-TRANSLATION-DPO-BRIDGE-TOKEN-ERASURE](16-TRANSLATION-DPO-BRIDGE-TOKEN-ERASURE-2026-10-16.md) **уже независимо описаны** совпадающие механизмы hashed collision и response-tail truncation, а также отдельные risks потери prompt context и diagnostic stripping. **Считать коллизию + truncation двумя общими механизмами, не четырьмя независимыми открытиями из двух документов.** Отличительный вклад этой записи: конкретные human-readable лексемные collisions (`accept`/`invalid`, `true`/`unauthorized`), отдельный контроль `allow`/`deny` и воспроизводимый 4-case E16 fixture. Обе записи `SOURCE-ONLY`, native C# и causal RP01 **NOT_RUN**.
