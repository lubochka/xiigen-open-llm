# Сохранённые успехи обучения, ухудшения и пределы сравнения

Дата: 20.10.2026. Это повторное чтение и проверка существующих результатов. Новых вызовов модели, обучения или внешнего оценщика в данном аудите нет. Исторические результаты не переписаны.

## 1. Нейронный RP01 нельзя описывать как отсутствие любого улучшения

В первичных scientific-result receipts есть реальные обновления LoRA и небольшой положительный сдвиг предпочтения. Одновременно маленькие панели свободных ответов остались неуспешными. Обе части результата существенны.

| Показатель | Документация C#: B0 → C50 | Перевод EN→RU: C50 → C100 |
|---|---:|---:|
| Учебные пары / новые updates | 500 / 50 | 500 / 50 |
| Изменившиеся LoRA-тензоры | 16 из 16 разрешённых | 16 |
| Изменившиеся base-тензоры | 0 из 39 | 0 из 39 |
| Средний normalized preference margin до | 0.03733304977416992 | 0.012293052673339844 |
| После | 0.03856058120727539 | 0.012403059005737304 |
| Парная средняя дельта | **+0.0012275314331054651** | **+0.00011000633239746093** |
| Margin вырос / снизился / равен | 14 / 11 / 0 | 11 / 9 / 0 |
| Chosen предпочитается до → после | 16/25 → 16/25 | 12/20 → 12/20 |
| Правильные свободные ответы | 0/3 → 0/3 | 0/4 → 0/4 |

Первичные документы прочитаны в этом проходе: [C# scientific-result](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/.kiro/xiigen-slices/work/results/RP01-simulation-main500-execution-20261015/scientific-result.json), [ENRU scientific-result](https://github.com/lubochka/llm_mvp_core/blob/8e020052fae7efa694b8d33f90768648d6b67af9/.kiro/xiigen-slices/work/results/RP01-enru-continuation-20261016/runtime-20261016-01/analysis-20261016-01/scientific-result.json). Их provenance — simulation-материал; receipts фиксируют реальные native updates и повторное открытие состояния. Это не арифметическая демонстрация без модели.

По правилу пользователя рост margin — измеренный успех **по этой оси и этим панелям**; рост accuracy не нужен для его признания. Он не доказывает появление способности объяснять код или переводить свободным ответом. Понижение margin на 11 и 9 отдельных случаях не скрывается. Статистическая значимость не установлена. C# panel — новые экземпляры тех же parameterized templates; независимость семантических семейств не доказана. ENRU panel уже раскрыта и остаётся диагностической. Нормированный score не переименовывается в использованный trainer response-sum DPO loss.

Положительный margin может расти и при ухудшении chosen: более сильное падение rejected тоже увеличивает разность. Уже существующий [разбор компонентов ENRU](https://github.com/lubochka/xiigen-planning/blob/1ad622fe1f6d1e8b0860265d8c208aeada3fefb0/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/60-TRAINING-PATH-AND-EXPERIMENT-VALIDITY-2026-10-19/01-DPO-LORA-NEW-TRAINING-DIFFERENCES.md#L231-L258) приводит отдельные chosen/rejected дельты. Здесь эти 40 score-файлов повторно не пересчитаны; этот вывод не присваивается как новая находка.

Состояние B0 в первичном ENRU отчёте названо random/untrained. Это существенное отличие от обучения уже владеющего языком Student, но не доказательство единственной причины неудачи. Малыми опытами PAD/HEAD проверяется стоимость настоящего обновления при том же B0; подмена его случайно созданным новым B0 либо другим pretrained donor одновременно меняет вопрос опыта.

## 2. Улучшение переводческой памяти подтверждается сохранёнными ответами

В `translation-memory-ingress-01` проверено настоящее звено загрузки учебных данных. Исходный corpus уже содержал соответствия фраз, но прежний reader/builder не доставлял их в `ExecutorOutput`. Исправленный преобразователь передал те же соответствия в обучение памяти; обычный Flow затем использовал новое состояние. Это соответствует пункту пользователя о допустимости загрузки данных, от которой зависит результат обучения.

Источник: [полный report](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/report.md), [native harness](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/harness/Program.cs), [evidence validation](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/evidence-validation.json).

| Одинаковая контрольная группа | Старый reader после 8 уроков | Исправленный reader после тех же 8 уроков |
|---|---:|---:|
| Точные учебные формулировки | 8/8 | 8/8 |
| Близкие формулировки | 3/3 | 3/3 |
| Новые сочетания усвоенных частей | **0/8** | **8/8** |
| Пограничные смысловые случаи | 0/5 | 0/5 |
| Всего правильных | **11/24 — 45.83%** | **19/24 — 79.17%** |
| Принятых ответов | 12/24 | 22/24 |
| Правильных среди принятых | 11/12 — 91.67% | 19/22 — 86.36% |
| Ошибочно принятых | **1** | **3** |

Здесь выигрыш общей correctness составляет 8/24, то есть 33.33 процентного пункта. Правильность среди принятых снизилась примерно на 5.30 процентного пункта. Ни одного прежде правильного ответа не потеряно; восемь новых правильных — `ev01`–`ev08`. Ошибки принятия усилились: `do not delete the file` превратилось в `удали файл`; `did you delete the file` также превратилось в команду. Третий ошибочно принятый ответ — `validate the file and the cache` → `проверь файл кэш`, не проходящий сохранённый список допустимых смысловых формулировок.

### Что проверено заново

Прочитаны 24 raw outputs и 24 scored rows каждого плеча, frozen gold и код оценивания. Повторно выполнена только чистая функция `assess` из сохранённого scorer: она не загружает модель, не пишет в обученную память и не создаёт новых ответов. Получены **0 расхождений** со всеми сохранёнными полями оценок. Совпадают ID, исходные тексты и группы 24 пар. Агрегаты выше заново восстановлены из этих строк. Машинный результат: [retained-learning-result-reanalysis.json](inventory/retained-learning-result-reanalysis.json).

Источники: [before raw](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/before/raw-outputs.json), [after raw](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/after/raw-outputs.json), [root scorer](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/score.cjs), [frozen assessor](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/inputs/score.cjs), [gold](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/inputs/panel-gold.json).

`inputs/score.cjs` исторически содержит оболочку A/B/C; реальный root `score.cjs` извлекает из него одинаковую функцию assess и корректно обрабатывает before/after. Это проверено; несовпадение оболочек **не объявлено новым дефектом**. `comparison.json` доступен через Git как LFS pointer, поэтому его payload не использовался для пересчёта: парность восстановлена по доступным raw/scored строкам.

Начальная идентичность state, общий material за исключением доставки `phrase_mappings` и одинаковые Domain DLL подтверждены сохранённым `evidence-validation.json`. В этом проходе бинарники и обучение не перезапускались. Это честная граница доказательства: перепроверка сохранённых ответов/рубрики, а не новая независимая оценка всех допустимых русских переводов.

### Какой механизм действительно научился

Цепь опыта: private corpus reader/builder в `27.TranslationDomain.Tools/Program.cs` → `TranslationMachine.TrainStructuredBatchAsync` → память Executor и обычный `GetOrchestratorAsync().RunAsync`. Native harness вызывает исходные reader/builder через reflection; полный CLI с registry/manifest не проверялся. Нейронных model instances и optimizer updates было 0. Исправление передачи фраз уже внесено 18 октября, поэтому повторно предлагать его как ещё отсутствующую реализацию нельзя.

Результат применим к переводческой памяти и ограниченной композиции команд EN→RU. Он не доказывает нейронное DPO/LoRA-обучение и не доказывает усвоение отрицания, вопросительности или полного порядка предложения. Точный учебный пример и композиция частей — разные уровни; эти группы оставлены раздельно.

## 3. Восстановление обученной памяти — полезное отдельное звено

В [P125-enru-memory-01](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/report.md) существующее memory обучение дало 19/24. В [P25-enru-cold-01](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/report.md) после cold restore сохранились 19/24 и совпали все 24 ответа/маршрута. Это положительная проверка сохранности уже приобретённого поведения. Новых тренировок в restore-проверке нет и не требуется. Три прежние ошибки принятия при восстановлении тоже сохраняются.

Для нового restore-эксперимента нужны конкретный дефект оригинального loader/state/consumer и его исправление. Простое повторение уже успешного cold readback не отвечает на новый вопрос о том, почему RP01 Student не дал правильный свободный ответ.

## 4. Стоимость не смешивается между различными опытами

В ingress сохранены outer training elapsed 38.5685 → 34.5068 ms и allocations 645008 → 663872 bytes. Это однократные наблюдения в двух процессах; JIT/порядок не выровнены. Цифры полезны как расходы этих запусков, но не позволяют приписать causal speedup исправлению данных. Аналогично C# main 6272.808539 s и ENRU main 2330.4531116 s относятся к разной работе; делить их для заявления ускорения модели нельзя.

Размер обучаемого получателя также показывается честно: в memory run 8 объявленных Unit и 1 Flow, нейронные слои — N/A; фактическая внутренняя глубина Unit не инструментирована. В neural run отдельно различаются layers модели, число LoRA-тензоров, trainable elements и optimizer calls. Это не взаимозаменяемые масштабы.

## 5. Успехи соседних веток и перенос matrix → kNN

Адресный разбор исторических механизмов, source parity и сырого E34 находится в [истории kNN](08-KNN-HISTORY-AND-EXPERIMENT-REVIEW.md). Вывод не «все неудачные матрицы выбросить»: проверяется, стал ли тот же полезный признак частью расстояния/голосования и достигает ли его текущий caller. Совпадение кода в ветках не доказывает совпадение данных, обученного состояния или consumer. Положительные AST/POS/order результаты учитываются по своим темам и представлениям.

Во всех будущих карточках сохраняется правило: любое измеренное полезное улучшение — успех по этой оси и предметной теме без обязательного роста accuracy; остальные изменения, включая ухудшения, публикуются рядом. Положительный результат одного получателя не переносится на другой только из-за общего слова «DPO» или «обучение».

## 6. E40V2: успешная память и детерминированная аппроксимация

Дополнительно перечитаны два первичных receipts и два оригинальных source-файла. В [E40-v1](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/35.ExperimentsV2/results/E40-TRUST-SCORER/results/run-2026-05-01-002.json) validation содержит только LOW: 0/20 у enhanced, baseline тоже 0. В [E40V2](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/35.ExperimentsV2/results/E40V2-TRUST-SCORER/results/run-2026-05-01-002.json) после стратифицированного разделения 79 train/21 validation получено baseline 0.6667 и enhanced 1.0; LOW 4/4, MEDIUM 8/8, HIGH 9/9. Отрицательный результат первой версии не опровергает положительный результат второй. Старый `improvement_pct=50` — относительная метрика receipt; абсолютная разница долей приблизительно 33.33 процентного пункта.

[E40TrustScorerV2](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/35.ExperimentsV2/Experiments/E40TrustScorerV2.cs#L30-L79) действительно сохраняет размеченные семимерные train-векторы, выбирает пять соседей по Euclidean distance и голосует. Source генерирует материал из 14 повторяемых шаблонов; 21/21 не является доказательством качества на новых независимых семействах. Нейронного обучения здесь нет, но зависимость от усвоенной размеченной памяти есть.

Нынешний [TrustScorer.ScoreCore](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/16.AutonomousLearning/TrustScorer/TrustScorer.cs#L29-L101) использует фиксированные веса и пороги 0.70/0.30. Он не хранит те train-векторы и не исполняет прежнее k=5 голосование. Это конкретное алгоритмическое различие, а не обвинение любой детерминированной функции. Например, official=true и domainMatch=true при остальных false/0 дают 0.45 → Medium, хотя соответствующий source-template V2 размечен High. Это статическое следствие формулы, **не новый runtime result**.

Эта тема уже разобрана с историческими commits и optional capability consumer в [существующем исследовании переноса](https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/10-historical-function-transfer-and-learning.md). Здесь подтверждены current код и две исходные цифры; новизна не заявляется. Назначение — доверие к источнику/учебному кандидату, а не объяснение C# или перевод предложения. Наличие активного вызова этого scorer в RP01 simulation не установлено; самостоятельный эксперимент без такого учебного consumer не ставится первым. Точное восстановление пользовательского случая «одна и та же неудачная матрица стала успешной функцией kNN» этим примером также не объявляется завершённым: версии E40 различаются и составом validation.
