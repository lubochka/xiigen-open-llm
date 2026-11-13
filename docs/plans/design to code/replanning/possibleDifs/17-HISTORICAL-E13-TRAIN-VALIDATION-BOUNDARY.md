# 17 — E13: граница train/validation разная у baseline и KNN; Tier2 «успех» нельзя переносить без replay

<!-- EFFECTIVE-AUDIT-2026-11-02-POST102:START -->
## POST102 — P91 подтверждает потерю пары, но не переписывает архивный E13

**NAMED_CHECK: POST102-E13-PAIR-PARITY, 02.11.2026.** [Сохранённый отчёт P9101](https://github.com/lubochka/llm_mvp_core/blob/15745b357e055055ee9bc58b52cc374d90580961/experiments-v2/RP01-MICRO/P9101/results/attempt-0001/report.md) относится к новому набору 11 методов/8 пар: исходный predictor получил 5 учебных пар вместо 6; точное предъявление 6 пар исправило 2 ответа из 2 при k=1. Экзамен одноклассовый, постоянная «защита» тоже даёт 2/2; при k=5 оба плеча дают 0/2. Это сохранённые данные, не новый native replay. Общий адрес продолжения: [аудит POST102](2026-11-02-POST102-TRANSFER-AND-EXECUTOR-AUDIT.md).

**WRONG:** «P91 дал 2/2 — исторические 5/7 E13 воспроизведены, утечка доказана, default consumer исправлен». **CORRECT:** N17-A различает missing train pair и попадание validation в train. Зафиксировать IDs и порядок **самих пар**, целевые Next IDs, file/family и фактические пары после loader/Train; один процент 0.8 или число методов не доказывает равенство. В P91 пропуск есть, утечка по его панели 0; про исторические 48 методов это ничего не вычисляет.

**Проверка существующего R0/A/B:** при доступных исходных данных сравнить `trainPairs`, `actualKnnTrainPairs`, `valPairs` по полным идентичностям, сохранив оба направления разности и пересечение. Файл с одним методом не создаёт пару; граница файлов не создаёт соседство; правильная метка относится к Next. В A/B фиксируются k, distance, tie-break, extractor, gold и одни case IDs; исправляется только подача пар. Отдельно указать продуктовый k и уже измеренный результат при нём. Контроли нельзя добавлять сверх паспорта после получения желательного score; все прежние дополнительные Train/Predict учитываются в факте расходов.

**Предел и продолжение:** владелец N17 сначала сверяет архивный corpus/input order; при отсутствии — INPUT_NOT_RECOVERED, без подстановки нового пилота под старые 5/7. Проверяющий пары и итогового scorer используют одну схему идентичности и одинаковый знаменатель; сообщаются wrong→right/right→wrong и baseline constant/majority. Результат компонента не закрывает связь с RP01 без обычного потребителя. Исторические расширения панели и новые обучения ниже этим блоком не назначены.
<!-- EFFECTIVE-AUDIT-2026-11-02-POST102:END -->

**Дата:** 16.10.2026. **Статус:** SOURCE_CONFIRMED_SPLIT_MISMATCH + HISTORICAL_REPORTED_METRIC, причинное влияние на RP01 **NOT PROVED**. Это новая адресная проверка наряду с уже документированными E09/E34/E51 confounds ([09](09-HISTORICAL-EXPERIMENT-VALIDITY-AND-FUSION-DRIFT.md), [11-E51](11-E51-KNN-ORACLE-SPLIT-AND-POSITIVE-MECHANISMS-2026-10-16.md)); не заменяет их.

## Архивное доказательство: два разных результата

- [E13 2026-05-07 Tier2, сохранённый результат](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/E13-METHOD-KNN/results/run-2026-05-07-001.json) фиксирует **48 methods, 31 adjacent pairs, trainPairs=24, validation=7, baseline 0.2857 (2/7), enhanced 0.7143 (5/7)**; эксперимент помечен PASSED. Это реальная сохранённая запись с небольшой выборкой, не выдуманный результат.
- [E13 2026-05-01](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/35.ExperimentsV2/results/E13-METHOD-KNN/results/run-2026-05-01-001.json) уже отмечен в [01-EXPERIMENT-BASELINE-RECONCILIATION](01-EXPERIMENT-BASELINE-RECONCILIATION.md) как 0.3333→0.3333. Эти разные корпуса/прогоны **нельзя сравнивать как before/after обновления модели**.

## N17-A — непосредственная асимметрия набора обучения

В [E13MethodLevelKnn.Run, 47–80](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/35.ExperimentsV2/Experiments/E13MethodLevelKnn.cs#L47-L80) программа сначала составляет `pairs = [(Current, Next)]` внутри файла, затем:
- `trainPairs=pairs.Take((int)(pairs.Count*0.8))` — именно из **пар** вычисляется baseline majority class;
- `valPairs=pairs.Skip(split)` — проверяются **пары**;
- но `knn.Train(allMethods.Take((int)(allMethods.Count*0.8)).ToList())` — для улучшенного метода подаётся срез **методов**, а не тот же `trainPairs`.

[MethodKnnPredictor.Train, 120–131](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/12.SemanticCompiler/Prediction/MethodKnn.cs#L120-L131) строит новые соседние пары внутри полученного списка; [Predict, 134–150](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/12.SemanticCompiler/Prediction/MethodKnn.cs#L134-L150) действительно голосует по **типу Next**. Следовательно, гипотеза «predictor обучали типу текущего метода вместо следующего» **опровергнута исходником**. Реальный риск другой: начальное множество train pairs для baseline и KNN **не обязано совпадать**, некоторые пары validation могут попасть в KNN training (при соответствующем расположении границы по methods). Точный overlap без восстановления исходных 48 методов **не измерен**.

В отличие от E09/E34 здесь наблюдается не только выбор разных validation subsets, но **разная функция split для train**; результат 5/7 может объясняться настоящим structural KNN, избытком/пересечением данных или обоими одновременно. Не объявлять 150% относительного выигрыша причинным результатом без одинаковой train/val границы.

## N17-B — смешение file families в малом последовательном split

[MethodExtractor, 20–47](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/12.SemanticCompiler/Prediction/MethodKnn.cs#L20-L47) сохраняет `FileId`, `IndexInFile`. Однако baseline и KNN [E13Run 43–53](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/35.ExperimentsV2/Experiments/E13MethodLevelKnn.cs#L43-L53) создают val по порядку `pairs`, не по file-family или repository-family. При разбиении внутри одного C# файла примеры общего разработческого шаблона могут остаться по обе стороны границы. Наличие такого источникового overlap в конкретном Tier2 архиве необходимо **проверить**, а не вывести из одного лишь кода.

## MICRO-E13-TRAINBOUNDARY-01 — самый дешёвый причинный эксперимент

**Цель:** проверить, что наблюдаемый E13 gain связан с KNN при честном одинаковом train, а не с неэквивалентными срезами и запоминанием знакомого файла. Ноль LoRA/DPO, ноль Windows GitHub Actions, один локальный net9 console/xUnit scientific probe поверх существующего E13 и `MethodKnnPredictor`.

**Инструментальный R0 (до каких-либо модификаций):**
1. Загрузить **тот же архивный Tier2 corpus/input order**, откуда получены 48 методов (если источник отсутствует — `INPUT_NOT_RECOVERED`, результат не воспроизводим; нельзя подставлять похожие файлы и говорить «тот же запуск»).
2. Записать список `(FileId, Current.IndexInFile, Next.IndexInFile, SHA(PackedString))` для `trainPairs`, `valPairs`, `actualKnnTrainPairs` из реконструкции `allMethods.Take(38)`.
3. Опубликовать `overlap=|actualKnnTrainPairs∩valPairs|`, `trainOnlyBaseline`, `trainOnlyKnn`, 7 индивидуальных prediction/output. Если overlap=0, **не** заявлять leakage; mismatch набора train может оставаться.

**Основной A/B (одно изменение):** A = исторический `MethodKnnPredictor.Train(allMethods.Take(38))`; B = точно тот же predictor/distance/k/fixed evaluation, но `TrainPairs(trainPairs)` с неизменяемым sealed набором пар, обходя reconstruction из списка методов. Все `valPairs` одинаковы в A/B. Контроль C = B с **переставленными только Next label** внутри train; D = большинство из **того же** `trainPairs`. При невозможности восстановить исходный corpus запускать новый **независимый** file-disjoint набор и публиковать как новый эксперимент, не ретрокоррекцию May07.

**Первичная метрика:** correct next method type на *одних и тех же* val pairs; вторичные: exact pair overlap, hit@5, per-file-family accuracy, cost. **Сильная фальсификация:** если B≈A и обе лучше D, а C деградирует, полезный learned structural signal поддержан. Если A проходит только за счёт overlap, а B≈D, архивный gain не переносится. Если B≈C, обучение нужной метке не доказано. n=7 — только диагностический пилот, затем расширить на новые file-disjoint 20–30 случаев.

**Важное для RP01:** этот тест проверяет **старый структурный memory learner** (predict next method type), а не качество генерации DPO Student. После подтверждённого положительного сигнала делать отдельный same-input `trained memory off/on` на **ordinary RP01 consumer**; без него причинная цепь к 0/3 остаётся H.

## Действия после проверки

Не менять старые JSON и не переписывать `passed`; рядом сохранить `new-e13-train-boundary-replay.json` с snapshot SHA, источником corpus, расшифровкой train/val, fixed seeds, метриками и verdict. На уровне design закрепить единый sealed split (`SourceFamilyId / FileId / MethodPairId`) и запрет различия набора train между baseline и enhanced без явного статуса `UNMATCHED_TRAIN`.

**Evidence grade:** actual `Train`/split mismatch **C**, historical 2/7→5/7 **M**, реальное попадание validation пары в train **H/UNKNOWN**, объяснение неудачи RP01 **H/UNPROVED**, новый runner/новое обучение/независимый CODEX review **NOT EXECUTED**.
