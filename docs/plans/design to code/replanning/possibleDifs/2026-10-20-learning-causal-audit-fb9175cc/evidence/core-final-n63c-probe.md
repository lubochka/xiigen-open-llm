# Финальная delta N63C-PROBE на cutoff256f603e

**Фиксированный cutoff:** 256f603e70032c11b1f4ffabbdd1fefdab642ea5, 21.10.2026. Только чтение новой delta; запусков, сборок, inference и исправлений source/protected —0. Старый A-only N63C на d962f958 остаётся отдельной исторической попыткой.

## 1. Есть завершённый другой probe, с другим бюджетом

В [results/n63c-probe.json][raw] сохранены **оба плеча** и итоговый бюджет. Это не результат прежнего рецепта4updates/36readouts:

| Реальная фаза completed probe | Количество |
|---|---:|
| Приобретение правильного T1 через native SFT | 600 updates |
| Исправляющий DPO | 1 на A +1 на B =2 |
| Последующий SFT | 100 на A +100 на B =200 |
| Всего optimizer updates | **802** |
| Scoring forwards | **60**:36 в шести полных панелях +24 промежуточные проверки приобретения |
| Generate / external teacher calls | 0 /0 |

В raw acquisition.updatesUsed=600, progress содержит точки25,50,…,600; declaredDeviationsFromCard указывает acquire LR0.0001 вместо карточной1e-5 и100 SFT steps на плечо вместо1. Acquisition firstLoss7.759556→lastLoss0.000028034774. TRAIN T1 приобрёл **22/23** правильных teacher-forced токенов вместо0/23; целый ответ не появился.

Нельзя приписывать эти802updates старому A-only опыту, считать недостающий B старого запуска выполненным или переносить его статус на новый probe.

## 2. Общий старт: что подтверждено и чего недостаточно

По [отчёту probe][report] оба плеча восстановлены из одного authority-acquired.bin. Текущий [source:163–167,262–267][restore] также передаёт один authorityPath обоим arms, загружает его в policy; reference остаётся canonical B0. Приобретённые optimizer moments не импортируются: новый DPO/SFT optimizer — часть назначенной схемы, а не полное продолжение acquisition optimizer.

В raw **все сохранённые поля трёх строк after_dpo панелей A/B совпадают**, а не только loss. DPO metrics также одинаковы: loss0.13489959, rewardMargin2.02036, accuracy1, optimizerStep1. Это поддерживает сравнимость наблюдаемого S1. Отдельного доказательства побайтового равенства всех стартовых tensor/optimizer/RNG states в этом raw нет. Фраза отчёта «loss совпал — значит состояние одно» сама по себе такого доказательства не даёт.

## 3. Точные сохранённые до/после

Здесь S0=приобретённое W_auth, S1=после одного DPO, S2=после100 SFT. Исходная необученная база W0 имела T1 NLL7.307878, margin0.2279687 и0/23 correct. Все значения таблицы — из panels/retention первичного JSON.

| Показатель | S0, общий W_auth | S1, одинаковые A/B | S2 A, stale rejected | S2 B, corrected |
|---|---:|---:|---:|---:|
| T1 correct tokens / targets | 22/23 | 22/23 | **20/23** | **21/23** |
| T1 chosen NLL | 1.1099185 | 1.1079743 | 1.1158044 | **1.1042708** |
| T1 margin | 1.1063861 | 1.1252952 | **−0.119724035** | **1.1151558** |
| Q1 correct tokens / targets | 0/29 | 0/29 | 0/29 | 0/29 |
| Q1 chosen NLL | 6.993638 | 6.993915 | **6.987412** | **7.074539** |
| Q1 margin | 0.11092615 | 0.11090803 | 0.16082 | 0.13136864 |
| Q2 correct tokens / targets | 0/21 | 0/21 | 0/21 | 0/21 |
| Q2 chosen NLL | 7.1899514 | 7.189884 | 7.2149773 | 7.250893 |
| Q2 margin | 0.057255268 | 0.058226585 | **−0.21515656** | 0.028256893 |

**Полезный наблюдаемый эффект B относительно A:** T1 сохраняет на1/23 больше правильных токенов, chosen NLL ниже на0.0115336, margin выше на1.234879835 и остаётся положительным. У A после stale SFT margin меняет знак; rejected meanLogP становится−0.9960804 против−2.2332695 в S1. Это поддерживает узкую гипотезу о конфликте конкретного старого текста и исправляющего предпочтения в данном C# материале.

Ухудшения не исчезают:

- Оба плеча теряют T1 correct tokens относительно S1: A22→20, B22→21. B не сохранил все22токена.
- T1 SFT A ухудшает chosen NLL на+0.0078301; B улучшает на−0.0037035.
- Q1 SFT A улучшает NLL на−0.006503, B ухудшает на+0.080624; B хуже A по Q1 NLL на0.087127, хотя margin обоих выше S1.
- Q2 SFT ухудшает chosen NLL в обоих плечах: A+0.0250933, B+0.061009; B хуже A по этому NLL на0.0359157. Одновременно A переворачивает Q2 margin, B сохраняет его положительным, но уменьшает относительно S1.
- Mean gold rank T1 улучшается S1 1.652173913→A1.521739130/B1.478260870, несмотря на падение correct-token counts; эти метрики не взаимозаменяемы. Q1 rank ухудшается253.827586207→A254.862068966/B267.965517241.

**Whole teacher-forced response exact=false во всех этих состояниях T1/Q1/Q2; EXAM correct tokens остаются0.** Это приобретение и частичное удержание отдельных токенов/предпочтения, не полный правильный ответ и не доказательство обобщения. Полезные NLL/margin/token улучшения по правилу пользователя считаются успехами своих показателей, без обязательного полного exact. Production reachability такой коллизии не доказана.

## 4. Стоимость — отдельные наблюдения

| Поле raw | A | B |
|---|---:|---:|
| DPO wall ms | 7574.9688 | 3867.0874 |
|100 SFT wall ms |49653.7131|47131.9246|
| DPO allocated bytes |936867616|936771760|
| SFT allocated bytes |25259233592|25259197888|

Acquisition phase wall492728.1666ms; весь процесс632.0084716s; общий peak working set1180323840B. Acquisition phase включает промежуточные checks, поэтому это не чистый stopwatch600updates. Один процесс, A→B; независимого причинного доказательства speedup или меньшего peak отдельного плеча нет. SFT training loss A0.9449399→0.00040639666, B0.000027529903→0.000006034074 относится к **разным целевым текстам** и не сравнивается как общий answer-quality score.

## 5. Важная граница source и results-02

На том же cutoff **текущий** [N63CollisionProbe.cs:54–58][constants] уже задаёт acquire LR0.003, MaxAcquireUpdates60, AcquireCheckEvery10, ArmSftUpdates20 и arm LR0.003. Это **не настройки completed raw600/100/LR0.0001**. Нельзя заявлять, что чтение этого current source воспроизводит exact measured binary первого probe. Локальные loops/restoration понятны, но attribution конкретного completed результата его исходнику требует отдельного producer/build receipt; новый запуск для этого здесь не назначается.

[Сохранённый поздний evidence][evidence] отдельно описывает Run-Probe.ps1 с results-02. Для этой другой попытки completed n63c-probe.json/report в проверенном наборе не установлен; статус **PARTIAL/UNKNOWN, не PASS**. Изменённые60/20 constants и presence source/build не равны завершённому учебному результату. Успех completed results/ не переносится на results-02 и не даёт разрешения повторять ни одну попытку.

## 6. Что уже изменилось в отчётности

На cutoff256 существует [N63C/results/candidate.md][candidate]: это кандидат замены фактически передаваемого SFT.Response при совпадении domain/prompt и побайтовом равенстве DPO.Rejected, до binding/IncrementalTrainer, с сохранением SourceIds. Candidate ссылается на новый probe и прямо сохраняет отсутствие доказанной product reachability, нулевые EXAM и неполный ответ. Поэтому прежний blanket вывод «кандидата нет / полезных метрик нет» больше не описывает состояние всего пакета.

Текущий [evidence][evidence] сообщает исправление live N63C card/embedded prompt: ATTEMPT_CONSUMED / PARTIAL_A / LEARNING_OBJECTIVE_OPEN / CORRECTED_DESIGN_REQUIRED, исправленный RNG path и независимый учёт stages. Это документная коррекция старого A-only протокола; она не завершает старый B и не меняет его raw. Новый завершённый probe документируется отдельно с полным своим бюджетом.

**Итог cutoff:** completed probe даёт измеренный узкий кандидат по сохранению T1 preference/NLL/токенов с указанными регрессиями. Полный ответ и EXAM learning остаются открыты; exact measured source lineage не считается проверенным, отдельный results-02 не считается завершённым. Исторический документ10 сохраняется, эта новая delta добавляется отдельно.

[raw]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/experiments-v2/RP01-MICRO/N63C-PROBE/results/n63c-probe.json
[report]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/experiments-v2/RP01-MICRO/N63C-PROBE/report.md
[restore]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/35.ExperimentsV2/Experiments/Rp01Micro/N63C/Common/ContinuousLearning/N63CollisionProbe.cs#L262-L267
[constants]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/35.ExperimentsV2/Experiments/Rp01Micro/N63C/Common/ContinuousLearning/N63CollisionProbe.cs#L49-L58
[evidence]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/docs/plans/replanning-possible-diffs/2026-10-21-n63c-rng-learning-evidence.md
[candidate]: https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/experiments-v2/RP01-MICRO/N63C/results/candidate.md

