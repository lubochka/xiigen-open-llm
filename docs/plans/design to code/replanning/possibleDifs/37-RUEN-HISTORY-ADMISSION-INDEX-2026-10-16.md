# N37 — вход в новые материалы расследования RP01 / RU→EN

**16 октября 2026, опубликовано в planning llm_plan3.**

Этот индекс отличает **новый source-confirmed риск** от нескольких сотен уже собранных агентами различий. Новое означает новую адресную цепь RU→EN Batch029; **не** новую доказанную причину 0/3 и 0/4 в direct Student RP01.

1. [N37 — исходники, новая гипотеза и конкурирующие объяснения](37-NEW-RUEN-HISTORY-ADMISSION-AND-CONTRAST-BLINDSPOT-2026-10-16.md).
2. [M37 — точный однокомпонентный RU→EN native A/B work order](37-MICRO-RUEN-CONTRASTIVE-REPAIR-CAUSAL-EXPERIMENT-2026-10-16.md).
3. [Пять доменных микропроверок C#, RU→EN, EN→RU, SQL, RU/HE](37-MICRO-MULTIDOMAIN-NEURAL-STRUCTURE-KNN-AND-HISTORY-EXAM-2026-10-16.md).
4. [N37 master — все основные классы расхождений и исторические положительные механизмы](37-RP01-MASTER-CROSSWALK-POSITIVE-HISTORY-NOVELTY-AND-FAST-DECISIONS-2026-10-16.md).
5. [Исполненный Python source-equivalent proxy](37-ruen-history-gate-source-proxy.py) и [его JSON результат](37-ruen-history-gate-source-proxy-result.json).

**Короткий факт:** [TranslationStructuredBatchBuilder.cs L201–255](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1ff7e6672/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L201-L255) удаляет candidate при overlap Jaccard >=0.70 по token sets; отрицание и порядок не получают семантического исключения. [Batch029Runner.cs L86–124](https://github.com/lubochka/llm_mvp_core/blob/793d7b755b23aac90445da3ac6661b1ff7e6672/27.TranslationDomain/TranslationBatch029Runner.cs#L86-L124) использует этот фильтр для RU_EN corpus history и отдельную heldout similarity protection.

**Интерпретация:** срабатывание фильтра и его пропуск на контрастах продемонстрированы source-equivalent Python proxy. Реальное .NET выполнение, Unit-learning A/B, изменение качества Student и независимая CODEX/Astra проверка **NOT_RUN**. Защищённый core docs/plans/replanning-possible-diffs каталог не изменялся.
