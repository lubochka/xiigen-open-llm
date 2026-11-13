# TEST-ORDER — покрыть порядок DPO и SFT на одном сохранённом обученном состоянии

<!-- transfer-entry-reload -->
**Вход исполнителя.** [Q47](01-registry.md#work-test-order). Это отдельное задание на проверку обучения. Продуктовый исполнитель берёт его только по назначению на эту строку. Сравнивайте стадии на одном исходном выученном состоянии и фиксированных уроках OR01. Читать ответ после каждой стадии. Сохраните штатный порядок DPO → SFT. Перед каждым переносом и после восстановления контекста заново прочтите полностью агентские файлы, назначенные навыки, исправленный список Любы, текущий индекс и карточку. [Как применить прочитанное к этой работе](03-test-and-execution-contract.md#read-and-apply) · [Что взять сейчас](01-registry.md#executor-next).

<!-- EXECUTOR-CONTEXT-120:START -->
## Обязательный вход в выбранный перенос или owner-раздел

Обязателен [договор120](120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Область:** этот вход относится к каждому выбираемому ниже FIX/TEST/R/RF/C и его собственной приёмке, а не превращает весь файл в одно поручение. Уже принятые части не переоткрывать; сначала сверить current source и сохранённые receipts именно выбранного владельца.

**Применимые риски LEARN:** Сохранить назначенные lesson/RowId и before→factor→after по независимому gold; margin/hash/update не заменяют правильный ответ. Положительный контроль удерживает прежний правильный ответ, отрицательный показывает потерянную/неусвоенную строку; gained/lost TRAIN и EXAM раздельно.

**Ожидания от теста.** Используйте данные этого ID и обычный путь, указанный в его действующей строке. Исходные доказательства сохраняются. Дополнительную диагностику из прежнего плана открывайте при конкретном расхождении. Для учебного результата запишите, что было до урока и что стало после. Техническую проверку формата или маршрута так и назовите.

**Проверка исполнителя.** Соберите затронутый проект и выполните назначенный малый сценарий на своей сборке. Укажите вызванный метод, вход и фактический результат. Для обученного поведения сравните смысл ответа и выбранного получателя. Для ресурса измерьте названную величину за тот же вызов. Если проверка упала, сохраните вход, ожидание и ошибку. Исполнитель может исправить дефект теста по независимому критерию и повторить эту проверку. Отпечаток нужен при разборе происхождения файла или сборки. Дождитесь завершения запущенной проверки. Затем переходите по текущей очереди. Право выполнить перенос определяется действующим назначением.
<!-- EXECUTOR-CONTEXT-120:END -->


<!-- BEGIN audit-91-applicability -->
## Дополнение28.10: фактический выход и происхождение показаний

В учебной проверке actual logits/vector/mask от восстановленного состояния должны попадать в назначенный trainer/reader; canned tensor допускается только в отдельно названном component test. Нулевой optimizer step delta и неизменные веса не доказывают отсутствие Backward. Квитанция различает measured/structural/unknown; эту границу P08 consumer уже пришлось исправлять. Предусмотренное контрактом чтение reference cache или выученной памяти разрешено: проверяется фактический hit/miss и привязка к правильному state/key. Запрет касается подмены проверяемого производителя заранее подготовленным oracle, а не самого механизма FIX-LC.

Для TEST-ORDER исходные данные, дозы, модель, доменная область и oracle владельца сохраняются. Это адресное условие будущей приёмки, не свидетельство уже пройденной проверки и не новый запуск. Источник ошибки и точные ремонты — [91/92](92-positive-transfer-followup-plan-2026-10-28.md).
<!-- END audit-91-applicability -->


<!-- BEGIN audit-84-applicability -->
## Уточнение исполнения по аудиту P02/P08/P09

Для метрики DPO указывать единицу pair/step/panel, веса и фазу. Контрпример одного шага с margins [+1,-3]: доля положительных пар 0,5, признак положительного среднего 0; совпадение этих счётов на исходном опыте не делает их одной метрикой. Проверка вызывает полный writer с metric_definitions, а не только внутренний builder. Неизмеренное время SFT — null, не0; исторические receipts не переписывать.

Применимость: TEST-ORDER. Основание и адресные проверки — [план85](85-reporting-sampling-cache-repair-plan-2026-10-27.md), [общий договор03](03-test-and-execution-contract.md). Это усиление постановки, не утверждение о наличии дефекта в каждом данном исходнике и не новый запуск. Исходный эксперимент/бюджет/статус допуска сохраняются; продолжение пропущенного этапа использует тот же обученный state, новая самостоятельная программа обязана содержать назначенное реальное обучение. При STOP/NEXT следовать прямому указанию пользователя, не продолжать прежнюю задачу.
<!-- END audit-84-applicability -->


<!-- BEGIN audit-87-applicability -->
## Приёмка и состояние обучения: аудит 28 октября

Проверять по отдельности optimizer step, scheduler step и число реально применённых обновлений. Контрпример P12: импортированные моменты AdamW400 совместимы с новой эпохой scheduler0; P10, напротив, использует fresh DPO0. Не переносить правило fresh/import из соседнего опыта. После отказа сверять веса, моменты, RNG и исполненные Forward/Backward; отчёт о правильных ответах без назначенного обучения не закрывает учебную цель.

Применимость: TEST-ORDER. Контроль отчёта: точная панель, настоящий учебный этап, терминальное завершение и выполненные обязательные проверки — отдельные поля; успех одной оси не равен общему допуску. Требования по [плану88](88-gradient-warmup-transfer-repair-plan-2026-10-28.md) добавляются к исходному рецепту, не меняют исторические результаты и не назначают повтор обучения. Проверки сохранённого receipt используют raw уже выполненного обучения; новая самостоятельная программа обязана содержать назначенный Train. Если нужен новый вызов модели, сначала явно назначить полный бюджет, а не скрывать его в диагностике. Это профилактическая применимость, не установленный дефект каждого данного исходника. STOP/NEXT и ожидание исходного process handle из карточки сохраняются.
<!-- END audit-87-applicability -->

<!-- BEGIN audit-76-specific-controls -->
## Уточнение по новому аудиту 27 октября: TEST-ORDER

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** LEARN по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

Основание — [76](76-new-transfers-and-experiment-evidence-audit-2026-10-27.md), ремонт — [77](77-new-transfer-repair-plan-2026-10-27.md). Это защита от повторения проверенной ошибки другого переноса, **не утверждение о найденном дефекте реализации каждого ID**. Сохраняются собственные данные, алгоритм, критерии и лимиты этой карточки; дополнительные Train/Forward этой вставкой не назначены.

**M — метрика привязана к фактической стадии обучения.** Для обучающего loss/reward записать состояние, на котором он вычислен, отдельно от номера опубликованных после update весов. У DpoTrainer последняя StepMetrics — pre-update; не называть её оценкой конечного checkpoint. Пример P02: B уже стартовал с margin 0,002773 после SFT; отношение последних значений ×3,22 не равно отношению приростов DPO. Любое сравнение приобретения/ремонта использует собственное before каждого этапа и одинаковый reader. DPO Accuracy=(reward_margin>0) означает улучшение относительно reference, а не правильный ответ: T2 имеет policy chosen−rejected=−0,1827326 при положительном reward. TRAIN/EXAM correctness и сохранённые logP остаются отдельными полями. Если эта карточка сравнивает математически равные оптимизации, сохранить её A/B equality; не требовать искусственного роста точности и не добавлять post-update Forward, когда нужное чтение уже предусмотрено.

**W — отчёт должен воспроизводиться своим источником.** Не закрывать исправление только ручной заменой JSON/Markdown: проверить действующий writer, который создаст следующий отчёт. Attempted, completed updates, diagnostic/model calls и native memory fits различаются. Неизмеренный ресурс — null/NOT_MEASURED, измеренный ноль — отдельный исход. Контроль отказа проходит через реальный producer/обработчик результата; готовый hand-written JSON доказывает только формат. Состоявшееся обучение после ошибки отчёта не повторять. Unit-проверка producer без Train не является новым экспериментом; новая экспериментальная программа содержит назначенный учебный участок.
<!-- END audit-76-specific-controls -->

**Уточнение исполнения, 27 октября 2026.** Обе последовательности начинаются с одного полного S0, но внутри каждой руки состояние между DPO и SFT не сбрасывается. Иначе проверяется два независимых шага вместо порядка обучения. Gold и readout framing закрепляются до обеих рук и не берутся из последнего training target. Сохранённые промежуточные/final states и counters должны отличать потерянный первый шаг от допустимого близкого качества. Расчёт отчёта использует готовые readouts без ещё одного DPO/SFT.

Общие примеры правильной и ошибочной проверки — [контракт03§24](03-test-and-execution-contract.md#transfer-audit-20261027). Это дополнение плана; новые проверки сейчас не запускались.

Статус: **план учебного покрытия нативных стадий и ограниченный кандидат изменения порядка**. Основание — `E-DPO-SFT-ORDER / OR01`, состояние исходников и результатов `213f0c9b0568384632ab6bb5a67c57d6f17b3df9`. Дата: 22 октября 2026. Новый документ не назначает перенос нового порядка в production и не повторяет FIX-MC. Текущий этап — документация: product-код не изменялся; сборка, тесты и обучение не запускались.

ID `TEST-ORDER`; файл назначения — `25-dpo-sft-order-learning-coverage.md`. Обязательны [карта состояния](02-learning-state-and-fixtures.md), [контракт тестов и исполнения](03-test-and-execution-contract.md) и [порядок реестра](01-registry.md). Отчёт и промпт — по-русски; модель исполнителя — текущая модель сессии по умолчанию.

До baseline A выполнить [общую native build-предпосылку02§10](02-learning-state-and-fixtures.md#native-build-prerequisite): исключить архивные Rp01Micro source из35 только при их фактическом попадании в Compile, сохранить одинаковый build setup A/B и проверить происхождение используемых типов. Это план ограниченного ремонта сборки с0 обучения; в данной сессии он не исполнялся.

## 1. Что именно улучшилось

[Исходный отчёт](https://github.com/lubochka/llm_mvp_core/blob/213f0c9b0568384632ab6bb5a67c57d6f17b3df9/experiments-v2/RP01-MICRO/OR01/results/attempt-0001/report.md) называет результат практически неразличимым и отрицает необходимость изменения кода. По правилу пользователя это не обнуляет небольшие измеренные полезные изменения. Их надо сохранить как узкий результат при одновременном указании всех ухудшений.

Фактор — **порядок одних и тех же обновлений**: A выполняет DPO по двум парам, затем SFT по S1 и S2; B выполняет SFT по S1/S2, затем DPO. В обоих плечах SFT optimizer импортирован на шаге 1000, DPO optimizer новый на шаге 0. Это не сравнение reset/import из FIX-MC.

Общий обученный старт уже приобрёл четыре TRAIN строки. До/после обеих рук правильны все 168 целевых токенов:33 + 33 + 51 + 51. Число правильных знаков EXAM осталось 2/4. Свободная генерация этим результатом не доказана. Исходники и raw: [bridge](https://github.com/lubochka/llm_mvp_core/blob/213f0c9b0568384632ab6bb5a67c57d6f17b3df9/experiments-v2/RP01-MICRO/OR01/Common/ContinuousLearning/OR01ExperimentBridge.cs#L489-L735), [A metrics](https://github.com/lubochka/llm_mvp_core/blob/213f0c9b0568384632ab6bb5a67c57d6f17b3df9/experiments-v2/RP01-MICRO/OR01/results/attempt-0001/A/metrics.json), [B metrics](https://github.com/lubochka/llm_mvp_core/blob/213f0c9b0568384632ab6bb5a67c57d6f17b3df9/experiments-v2/RP01-MICRO/OR01/results/attempt-0001/B/metrics.json).

| EXAM, нормализованный отступ | До, оба плеча | A после | B после | B−A |
|---|---:|---:|---:|---:|
| D1 | −0.3759841919 | −0.3450298309 | −0.3450865746 | −0.0000567436 |
| D2 | +0.0704236031 | +0.0757131577 | +0.0763306618 | **+0.0006175041** |
| D3 | +0.2548995018 | +0.2577228546 | +0.2577686310 | **+0.0000457764** |
| D4 | −0.5447359085 | −0.5298433304 | −0.5296359062 | **+0.0002074242** |
| Сумма | −0.5953969955 | −0.5414371490 | −0.5406231880 | **+0.0008139610** |

D1 и D4 остаются неверными в обоих плечах. D4 улучшил свой отступ, но не приобрёл правильный знак; это не «ещё один правильный ответ». Источники: квитанции [A0](https://github.com/lubochka/llm_mvp_core/blob/213f0c9b0568384632ab6bb5a67c57d6f17b3df9/experiments-v2/RP01-MICRO/OR01/results/attempt-0001/A/evaluation/A0/evaluation-receipt.json), [A1](https://github.com/lubochka/llm_mvp_core/blob/213f0c9b0568384632ab6bb5a67c57d6f17b3df9/experiments-v2/RP01-MICRO/OR01/results/attempt-0001/A/evaluation/A1/evaluation-receipt.json), [B1](https://github.com/lubochka/llm_mvp_core/blob/213f0c9b0568384632ab6bb5a67c57d6f17b3df9/experiments-v2/RP01-MICRO/OR01/results/attempt-0001/B/evaluation/B1/evaluation-receipt.json).

| Другая ось | A | B | Граница |
|---|---:|---:|---|
| TRAIN NLL D-T1 после | 0.0010049138 | **0.0010041454** | Оба хуже обученного до 0.00085286517 |
| TRAIN NLL D-T2 после | **0.0011023176** | 0.0011023788 | B чуть хуже A; оба хуже до 0.0009121961 |
| TRAIN NLL S1 после | 0.00086734904 | **0.00086703146** | Оба хуже до 0.00083031284 |
| TRAIN NLL S2 после | 0.0007804842 | **0.00078022026** | Оба хуже до 0.0007639727 |
| Loss внутри DPO-стадии | **0.27748886** | 0.28123897 | У B выше; стадии стартуют после разного предшествующего обучения |
| Reward margin внутри DPO-стадии | **1.4838266** | 1.4638367 | У B ниже; DPO pair accuracy обеих стадий = 1. Это не EXAM after |
| Сумма allocated bytes трёх стадий | **4 446 214 024** | 4 446 680 160 | B дороже на 466136 байт |
| Время трёх стадий | 21361.8362 мс | 20646.1753 мс | B меньше на 715.6609 мс; один запуск |
| Whole-process peak working set | 1132277760 байт | 924786688 байт | B меньше на 207491072 байта; не изолированный пик обучения |
| Whole-process CPU | **144375 мс** | 152359.375 мс | B дороже на 7984.375 мс |

Время и peak — положительные наблюдения этой пары, без доказанного причинного устойчивого ускорения. Исторические whole-process CPU/peak включали также четыре Generate до и четыре после на каждую руку, всего 16 Generate; новый фазовый replay их не выполняет. Эти whole-process числа не являются изолированной стоимостью трёх обновлений. Разные финальные веса и меняющаяся loss не сами по себе полезный результат. Результат ограничен четырьмя приобретёнными строками, четырьмя близкими EXAM парами, SFT LR 0.001 и DPO LR 0.00001. Смена скоростей или их «выравнивание» — другой фактор и не назначается.

**Любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост accuracy не обязателен.** Эта карточка сохраняет такой успех и не превращает его в универсальную рекомендацию или production PASS.

## 2. Где находится код и почему сейчас выбран тестовый перенос

Реальный будущий получатель порядка — [`14.ContinuousLearning/Training/IncrementalTrainer.cs`](https://github.com/lubochka/llm_mvp_core/blob/213f0c9b0568384632ab6bb5a67c57d6f17b3df9/14.ContinuousLearning/Training/IncrementalTrainer.cs#L472-L674), `TrainCoreAsync`. Сейчас он:

1. Открывает транзакцию и отмечает начало mutation до первого изменения.
2. Присоединяет final LoRA objects, снимает base/adapter bits и строит frozen reference из текущей policy.
3. Вызывает DPO executor; проверяет неизменность reference, postDpoPermit и полноту DPO artifact.
4. Вызывает `ProductionSupervisedTrainingStepExecutor` для SFT строк, затем проверяет и сохраняет финальный adapter.

[`IncrementalTrainingSessionBuilder.Build`:404–407](https://github.com/lubochka/llm_mvp_core/blob/213f0c9b0568384632ab6bb5a67c57d6f17b3df9/14.ContinuousLearning/Training/IncrementalTrainingSession.cs#L404-L407) создаёт свежий SFT optimizer до стадий; старые SFT moments не импортируются. Native default — DPO→SFT, а не B.

Исторический OR отличается от этого consumer **не только возможностью переставить стадии**:

- Bridge 489–497 загружает S0 только в policy. `LoadModels` 1094–1133 оставляет reference на каноническом B0 (`a65887…`), тогда как product 544–551 клонирует уже текущую policy.
- Bridge 532–549 импортирует SFT 1000 и передаёт его нативному executor напрямую. Исследовательский A затем применяет эти moments после DPO. `ResumeSft` из FIX-MC это намеренно запрещает при промежуточной DPO mutation.
- OR использует исследовательские content/admission и собственные outputs. Они не являются принятыми production training transaction, checkpoint manifest или разрешением публикации модели.

**Решение этой карточки:** добавить постоянный учебный A/B тест настоящих нативных фаз по исходному рецепту. Не добавлять новый production order flag/endpoint, не переставлять текущие blocks, не расширять FIX-MC, не обходить `postDpoPermit`. Данные поддерживают узкий кандидат SFT→DPO; они не проверили новый mixed-stage authority/provenance/publication contract. Отдельная продуктовая интеграция потребовала бы: собственного SFT admission до первой mutation; обеих проверок revocation; закреплённой reference identity до любых стадий; полного stage ledger; recovery без второго обновления; final-state manifest с обеими стадиями. Исполнитель данной карточки этот новый продуктовый контракт не изобретает.

Тестовый перенос отличается от FIX-MC/PD/MS: он фиксирует **порядок двух разных objectives на одной policy**, новое семейство `h512-order-mixed/v1` и shared-state replay. Ни существующий тест оптимизатора, ни справедливость его импорта не проверяют это взаимодействие.

## 3. Точный сохранённый старт и входы

Источник только `experiments-v2/RP01-MICRO/_shared-learned-starts/h512-order-mixed/v1/` на закреплённом commit. Состояние COMPLETE; producer уже выполнил 1000 SFT updates. Новый тест выполняет **0 producer/acquisition updates** и не запускает `Prepare.ps1`, `--prepare-shared` или старый `Run-With-Learning-Signal.ps1`.

| Артефакт | Точная идентичность |
|---|---|
| S0.weights.bin | 47899519 байт; SHA256 `d0c1af83fc39ed99873e1e2d6099aabc30b9e0f8220471c2aaff9785e50811b0` |
| S0.optimizer.bin | 426788 байт; SHA256 `e26ae63d06c661482dfab15b9fa567786c4afdac2e567cb995bfae39c01554ba`; step 1000 |
| S0.rng.json | 1573 байта; SHA256 `f819248e615f50e41c22db16d5fac5116809857d195d3b9d149c9b6ec580ba9b` |
| data.source.jsonl | 894 байта; SHA256 `f384d42b0d4d0e2de795e034add9a789d6b2465d2e491985da7abdd7704610ef` |
| generation.json | SHA256 `e1f960b8c7d883f84c9b4e55d24c7284794cee2fecfd71beac5835d199746f40` |
| tokenizer.json | SHA256 `e51c91d85ec486863f1862e49b378fa9f34873ee31a993114ba41995f38e5066` |
| B0 reference | manifest SHA256 `c5f4f7da80f427b8e9aac1dabded5849f0cbc6cb444b99d2bf7daac44ec0246d`; weights SHA256 `a65887cc9d97c793d00e77632013adeba5e2d36efd7e6cebe7e49ba8a9b37911` |

**Точный адрес reference B0:** `training-artifacts/models/csharp-code-documentation/ilearning-unit/rp01-csharp-small/v1/checkpoints/000001/manifest.json`, веса — соседний `weights.bin` размером 47 899 519 байт. [Нативный manifest B0](https://github.com/lubochka/llm_mvp_core/blob/213f0c9b0568384632ab6bb5a67c57d6f17b3df9/training-artifacts/models/csharp-code-documentation/ilearning-unit/rp01-csharp-small/v1/checkpoints/000001/manifest.json) задаёт `InitialBaselineNoUpdate`, step 1, отсутствие parent и указанные выше identities. `manifestSha256` — каноническая identity manifest, её проверяет нативный reader; это не объявление SHA-256 произвольного JSON-текста.

**Настройки исполнения отдельно от file hash:** в сохранённом `generation.json` поле `runtimeCompatibility.configSha256` равно `8cf38d269f806356191c614ccbf3bcafdb3c7e339f02e9d77bdade4e9fcd68ae`, topology — `f220de4520adbec3ddb8c23eabb0d485f6bb052f0c0dc9a50a8a5d9c3448cbfb`. Это identity конфигурации V629/C2048/H512/L4/Q8/KV2/FF1408, q/v LoRA rank 8, alpha 16, dropout 0; base/adapter seed 20261014. SHA-256 bytes файла конфигурации остаётся `e1f960…`, а не `8cf38…`. Текущий `config/code-documentation/generation-v1.json` не подменяет закреплённую конфигурацию до проверки exact bytes и этих полей.

Источник идентичностей — [bundle](https://github.com/lubochka/llm_mvp_core/blob/213f0c9b0568384632ab6bb5a67c57d6f17b3df9/experiments-v2/RP01-MICRO/_shared-learned-starts/h512-order-mixed/v1/bundle.json) и [preparation](https://github.com/lubochka/llm_mvp_core/blob/213f0c9b0568384632ab6bb5a67c57d6f17b3df9/experiments-v2/RP01-MICRO/_shared-learned-starts/h512-order-mixed/v1/preparation.json). Эта документная проверка не загружала binary weights. При тестовом переносе проверить bytes/hash и zero-update cold load до обучения; Git LFS pointer не считается весами. Отсутствие/несовместимость — `FIXTURE_UNAVAILABLE`, без создания нового старта или тайного fallback.

Создать `Tests/Fixtures/ExperimentFixes/v1/Or01/manifest.json`, `train.jsonl`, `exam.jsonl`, `historical-readouts.json`. Manifest ссылается на существующие binary bytes; новая копия 47.9 МБ не нужна. Source/EXAM — точные [inputs.json](https://github.com/lubochka/llm_mvp_core/blob/213f0c9b0568384632ab6bb5a67c57d6f17b3df9/experiments-v2/RP01-MICRO/OR01/results/attempt-0001/inputs.json) и `inputs/exam.jsonl`, без нормализации и изменения текста.

TRAIN только D-T1,D-T2,S1,S2. D-T1/D-T2 — противоположные объяснения порядка вычитания в `Delta`; S1/S2 — соответствующие два C# исходника. Это исторический узкий набор о порядке вычитания в C#: две пары описаний кода и два урока воспроизведения соответствующего исходника. Обе формы задачи сохраняются дословно. Он не доказывает перенос между темами или качество общего маршрутизатора; добавлять сюда перевод, планирование либо новые отрицательные уроки нельзя. DPO framing — исходный `prompt+chosen/rejected`, SFT framing — `prompt+"\n"+response`. **DPO:** chosen содержит 124 токена, из них 33 целевых токена ответа. **SFT:** совместная последовательность содержит 68 токенов; исходный executor обучается на всех 67 сдвинутых целях. Маска 51 токена ответа нужна только для readout. Нельзя автоматически применить FIX-MS: обучение только по 51 response-target изменило бы второй фактор. Gold EXAM из авторских chosen/rejected, не из output B; EXAM ни в optimizer, ни в подбор LR не передаётся.

## 4. Нативный replay и точный бюджет

Новые файлы — `Tests/TrainingStageOrderLearningTests.cs` и test-only helper `Tests/ExperimentFixes/TrainingStageOrderFixture.cs`. Класс — `LlmFromScratch.Tests.TrainingStageOrderLearningTests`; единственный новый обучающий тест:
`RestoredMixedState_ReplaysBothOrdersAndPreservesLearnedResponses`.

Оба плеча используют product assemblies `04.TrainingLoop`, `11.DPO`, `14.ContinuousLearning`, `03.TransformerModel`, `10.LoRA` через существующий `Tests/LlmFromScratch.Tests.csproj`. Не линковать архивные `OR01/Common` реализации и не выполнять `OR01Runner.dll`. В helper переносится только сборка тестового входа, восстановление состояния, порядок вызовов и запись измерений; Trainer, DpoTrainer и AdamW остаются нативными.

### Точки подключения без поиска исполнителем

| Действие нового helper | Конкретный действующий API / образец |
|---|---|
| Загрузить независимую пару моделей | Создать два `LlmModel` с конфигурацией раздела 3; вызвать `LoraApplicator.ApplyFreshSeededPair` с root seed 20261014 и потоками `rp01-policy` / `rp01-reference`; для каждого из двух файлов вызвать `LlmModel.ValidateFullCheckpointArtifact(stream, config, new FullCheckpointLoraTopology(8, ["q_proj", "v_proj"]), stream.Length, token)`, потребовать `IsSuccess`, вернуть `stream.Position = 0`, затем `LoadCheckpoint(stream)` загружает S0 в policy и B0 в reference. Перегрузка validator без topology проверяет base-only и здесь не подходит; после validation stream находится в конце. Создание объектов и временная инициализация параметров не считаются обучением: все 55 тензоров заменяются закреплёнными весами до readout. Образец attach — [ExactRecipientFixture](https://github.com/lubochka/llm_mvp_core/blob/213f0c9b0568384632ab6bb5a67c57d6f17b3df9/Tests/DpoTrainerOptimizerStateHandoffTests.cs#L150-L163); full-artifact validation — [native evaluation](https://github.com/lubochka/llm_mvp_core/blob/213f0c9b0568384632ab6bb5a67c57d6f17b3df9/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoEvaluation.cs#L60-L66). |
| Восстановить tokenizer/RNG | `BpeTokenizer.Load(Stream)` читает точный tokenizer; **0 BpeTokenizer.Train** в основном replay. Policy получает `RestoreRandomState` из S0 с `LlmModelRandomStateIdentity(topology, loraConfiguration)`. LoraConfiguration SHA из preparation — `4df9bab2d5115d8368c3f36fdf6c4f31ba8ed43bb65200c41a5c563795d02c35`. Все reference parameters замораживаются; policy base заморожены, обучаемы только 16 adapter parameters. |
| Сформировать DPO материал | `LogicalDpoPreparedBatchMaterializer.Prepare` с двумя точными `LogicalDpoPreferenceExample`, затем `DpoExactPairMaterial.CaptureAndValidate`. Сохранить исходный `DeterministicGenerator` authorship обеих сторон; не притворяться новой внешней генерацией. Рецепт — [Materialize/TrainingCarriers](https://github.com/lubochka/llm_mvp_core/blob/213f0c9b0568384632ab6bb5a67c57d6f17b3df9/experiments-v2/RP01-MICRO/OR01/Common/ContinuousLearning/OR01ExperimentBridge.cs#L876-L956), алгоритм materializer остаётся product. |
| Разрешение только внутри теста | В новом helper объявить private `TestOrderMutationAdmissionIssuer : IDpoMutationAdmissionIssuer` по образцу [TestDpoMutationAdmissionLease](https://github.com/lubochka/llm_mvp_core/blob/213f0c9b0568384632ab6bb5a67c57d6f17b3df9/Tests/DpoTrainerOptimizerStateHandoffTests.cs#L987-L996); его `AcquireDpoTrainerMutationPermit` возвращает отдельный disposable permit. Вызывать `DpoMutationAdmissionCapability.Issue` штатно из friend test assembly. Existing private класс из другого теста недоступен и не вызывается. Это разрешение низкоуровневого теста, не product admission и не подмена learner. |
| Создать настоящий DPO trainer | `DpoProjectLoraRecipient.Capture(policy, reference, attachedAdapters, lora)` → `DpoExecutionProfile.Capture(batch, recipient.Bindings)` → `DpoTrainer.CreateForProjectLora`. Образец — [ExactRecipient_UpdatesOnlyInstalledAdaptersAndExportsActualProfile](https://github.com/lubochka/llm_mvp_core/blob/213f0c9b0568384632ab6bb5a67c57d6f17b3df9/Tests/DpoTrainerOptimizerStateHandoffTests.cs#L40-L65). DPO continuation proof = null, optimizer fresh 0. Recipient требует независимые объекты, одинаковые base bits и frozen reference, но разрешает разные LoRA weights у S0/B0; его `RequireValid` не ослаблять. |
| Создать настоящий SFT executor | `AdamWPortableState.FromBytes` → `TryImportPortableState` по отсортированным named bindings тех же final adapter objects; повторный export обязан побитово совпасть и иметь step 1000. Создать `IncrementalTrainingSession` и internal `ProductionSupervisedTrainingStepExecutor`: [SupervisedTrainingExecution.cs](https://github.com/lubochka/llm_mvp_core/blob/213f0c9b0568384632ab6bb5a67c57d6f17b3df9/14.ContinuousLearning/Training/SupervisedTrainingExecution.cs#L19-L32). Это прямой тест нативной стадии, без product registry/transaction. |
| Считать EXAM | Два вызова `DpoTrainer.EvaluateAverageNextTokenLogProbability` на пару, margin = chosen − rejected. Это тот же native scorer, что в [исходном оценщике](https://github.com/lubochka/llm_mvp_core/blob/213f0c9b0568384632ab6bb5a67c57d6f17b3df9/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoEvaluation.cs#L72-L88). Не копировать реализацию log-prob в тест. Для TRAIN один `LlmModel.Forward` на строку и независимая арифметика response NLL/argmax по закреплённым targets, как в `Score` bridge:959–992. На время readout выключить parameter RequiresGrad и затем восстановить flags, не меняя weights/RNG. |

Доступ к internal API уже предоставлен `InternalsVisibleTo("LlmFromScratch.Tests")` в [DPO.csproj](https://github.com/lubochka/llm_mvp_core/blob/213f0c9b0568384632ab6bb5a67c57d6f17b3df9/11.DPO/DPO.csproj#L25-L27) и [ContinuousLearning.csproj](https://github.com/lubochka/llm_mvp_core/blob/213f0c9b0568384632ab6bb5a67c57d6f17b3df9/14.ContinuousLearning/ContinuousLearning.csproj#L28-L30). Не делать APIs public и не менять product permissions ради теста. Для H512 используются сохранённые bytes; tiny tokenizer.Train из образца `ExactRecipientFixture` **не копируется**.

Checkpoint directories нативного DPO назначены заранее: `artifacts/fbe/TEST-ORDER/attempt-0001/A/checkpoints` и `B/checkpoints`. Сохранить observer исходного OR: `RepositoryOwnedDpoOptimizerStateFile.WriteAndReadBack` и `RepositoryOwnedDpoRandomStateFile.WriteCreateNew` с **`RepositoryOwnedDpoRandomStateFile.BuildFileName(step)`**; capture/restore RNG — нативные callbacks. Запись `after-sft.rng.json` этим писателем недопустима. В A файл DPO checkpoint — промежуточное состояние перед SFT, в B — после всех трёх стадий. Финальные A/B hashes всегда берутся из памяти после завершения каждой руки; DPO-файл A не выдаётся за итог A. Все файлы остаются test artifacts, не публикуются в registry.


Внутри одного теста — два независимых набора model/reference/tokenizer/trainer/optimizer. Между A/B общие только неизменяемые bytes входов. Это фазовый нативный replay: исторический расход измерялся в двух процессах. Новые process peak/CPU не сравнивать с ним как одинаковую область измерения.

Сначала загрузить обе независимые пары и завершить общий preflight пунктов 1–3 для A и B; только после него выполнять пункт 4 сначала для A, затем для B. Если BEFORE одного плеча не квалифицирован, **ни одно** не получает update. Номера ниже задают действия, а не два независимых автоматических запуска:

1. Проверить exact fixture bytes/config/topology; загрузить policy S0 / reference B0, восстановить RNG. Записать 39 base / 16 adapter tensor hashes и named parameter identities.
2. Выполнить четыре TRAIN чтения. До обучения требуется 168/168 и четыре whole teacher-forced response exact; иначе попытка не сравнима и оба update-плеча не запускаются.
3. Создать оба optimizer до мутаций, на тех же final LoRA objects. SFT импорт 1000 (AdamW beta1=0.9, beta2=0.95, weightDecay=0, eps=1e-8), DPO fresh 0; SFT Trainer BatchSize 1, SeqLen 2048, Warmup 0, TotalSteps 1004, MaxLr = MinLr = 0.001, GradAccum 1, GradClipNorm 1; DPO использует `DpoTrainer.ProductionOptimizerSettings`: LR 0.00001, weight decay 0.01 и прочие нативные AdamW defaults, которые записываются в manifest. Objective — `response-sum-softplus-signal-weighted-v3`; не заменять SUM на MEAN. SFT weight decay остаётся 0. Все настройки одинаковы в A/B. Установить исходные exact carrier/mask/digest, dropout 0. Scheduler SFT создаётся свежим с constant LR: AdamW step меняется 1000→1002, отдельный Trainer._supervisedStep — 0→2. Импорт optimizer не объявляется восстановлением scheduler/cursor; при постоянной скорости это совпадает с исходным OR.
4. Считать четыре EXAM пары. Выполнить A=DPO,S1,S2 либо B=S1,S2,DPO. DPO — один `StepVector` по 2 pairs, SFT — два `ProductionSupervisedTrainingStepExecutor.Execute`; никаких fake mutation и lookup-ответов.
5. Считать те же четыре TRAIN и четыре EXAM пары; проверить frozen reference/base, финальные шаги DPO = 1 и SFT = 1002, finite parameters, неизменность input fixtures. Записать final weights digest и all per-row readouts. Результат сохраняется только под artifacts/fbe, не в model registry.

| На плечо | Количество |
|---|---:|
| Producer/acquisition updates | 0 |
| DPO updates / SFT updates | 1 / 2 |
| DPO forward/backward | 8 / 4 |
| SFT forward/backward | 2 / 2 |
| TRAIN diagnostic forwards | 8 |
| EXAM scoring forwards (4 pairs × 2 sides × 2 states) | 16 |
| Generation / teacher / retry / warmup | 0 / 0 / 0 / 0 |

Итого на два плеча: **6 optimizer updates, 68 forward, 12 backward**. Загрузки четырёх моделей и проверки hashes не вызывают дополнительные forward; BPE.Train — 0. Общие data/score arithmetic не запускают learner повторно. Свободная генерация из исходного опыта здесь не повторяется: она не является положительным основанием кандидата. Все сравниваемые native outputs вычисляются после настоящих обновлений; тест hashes/counters без обучения не считается исполнением этой карточки.

Assertions: одинаковые начальные weights/RNG; все четыре приобретённые TRAIN строки сохраняются в A/B; 2/4 правильных знаков не выдаются за 4/4; D2 margin в B выше, чем в A, как закреплённая узкая полезная ось. Все остальные deltas записываются без отбрасывания отрицательных. Исторические NLL/margins — reference readouts, а не биты; для сопоставления per-score заранее назначить abs tolerance 1e−5, отдельно показывая сырой B−A. Дельту нельзя округлить до нуля или выдать за статистическую значимость. Если current native реализация не воспроизводит этот узкий знак улучшения, тест не перекрашивать подбором LR/данных/бюджета; сохранить `NOT_REPRODUCED` и exact current source diff. Historical gain остаётся историческим, native transfer PASS не ставится.

Конфликты с другими карточками решаются так: FIX-MC не используется для этого загрузчика и не получает разрешения продолжать SFT через DPO; FIX-MS остаётся в `AllTargets` для обеих рук; EXAM scorer не использует training reference cache. Если FIX-LC уже принят, каждая рука всё равно создаёт свежий DpoTrainer и видит две разные пары ровно один раз: кэш начинается пустым, reference forward остаётся 4 на плечо. Если FIX-PD уже принят, его одинаковый текущий native path применяется в A и B без экспериментальной альтернативы; source/version фиксируются, historical numeric tolerance проверяется честно. Несовпадение с историей означает `NOT_REPRODUCED`, не разрешает отключить принятое исправление или подобрать новые данные. Эти зависимости не требуют нового producer или автоматического изменения соседних планов.

Ресурсы записывать по явно названным границам каждой стадии. `StepVector` включает собственную запись DPO checkpoint, optimizer и RNG через observer — её не вычитать из стоимости нативной DPO-стадии. Подготовка итоговых A.json/B.json и readout находятся вне update timer. Исторические allocated bytes дополнительно включали извлечение adapter values после остановки timer; это различие зафиксировать в manifest и не выдавать новую узкую аллокационную область за побитово тот же historical scope. Не требовать исторических 715.66 мс/207 МБ как CI-порогов. Дополнительные попытки ради устойчивого speedup не назначены.

## 5. Что делать с существующими тестами и потенциальными failures

Свежий запуск production тестов не выполнялся; список реальных текущих падений не установлен. Не объявлять «падающими» tests по предположению. Следующие assertions сохраняются:

| Существующий test | Почему сохраняется |
|---|---|
| `IncrementalTrainerDpoWiringTests.DurableDpoReference_IsIndependentFrozenExactPolicySnapshot` | Native default reference остаётся snapshot текущей policy; historical B0 — только отдельный replay |
| `RevocationAndCancellationAfterDpoMutationPreventsSupervisedAndFinalAdapterMutation` | Post-DPO guard остаётся обязательным; TEST-ORDER не меняет порядок продукта |
| `RevocationBeforeSupervisedUpdateAllowsNoLaterAdapterMutation` | SFT mutation не получает обход authority |
| `AdmittedFakeDpoAndFakeSupervisedStep_CreateDurableCandidateWithoutFinalRegistryPublication` | Это plumbing test; сохранить, но не считать его доказательством learning |
| `DpoTrainerOptimizerStateHandoffTests`, `RetainedCheckpointBackedIncrementalTrainingSessionFactoryTests` | Их optimizer/factory recovery contracts не переписывать под research recipe |

Ссылки: [wiring tests](https://github.com/lubochka/llm_mvp_core/blob/213f0c9b0568384632ab6bb5a67c57d6f17b3df9/Tests/IncrementalTrainerDpoWiringTests.cs#L359-L440), [postDpo guards](https://github.com/lubochka/llm_mvp_core/blob/213f0c9b0568384632ab6bb5a67c57d6f17b3df9/Tests/IncrementalTrainerDpoWiringTests.cs#L2685-L2770).

Точный существующий filter выполняется один раз до добавления тестового покрытия и один раз после, с разными папками:

```powershell
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --filter "FullyQualifiedName=LlmFromScratch.Tests.IncrementalTrainerDpoWiringTests.DurableDpoReference_IsIndependentFrozenExactPolicySnapshot|FullyQualifiedName=LlmFromScratch.Tests.IncrementalTrainerDpoWiringTests.RevocationAndCancellationAfterDpoMutationPreventsSupervisedAndFinalAdapterMutation|FullyQualifiedName=LlmFromScratch.Tests.IncrementalTrainerDpoWiringTests.RevocationBeforeSupervisedUpdateAllowsNoLaterAdapterMutation" --logger "trx;LogFileName=guards.trx" --results-directory artifacts/fbe/TEST-ORDER/attempt-0001/baseline
```

Для второго прохода заменить только `baseline` на `compatibility`. Ровно 3 guard-теста на проход. Их `RecordingDpoExecutor` не выполняет model forward/backward/optimizer update: один возвращает synthetic failure, два пишут synthetic artifact; SFT executor calls = 0. Но каждый `TinyTrainingFixtureCL` вызывает `BpeTokenizer.Train("tiny tokenizer corpus", 256)` — **3 BPE Train на проход, 6 на оба**, без merges при словаре 256; они не скрываются за «0 обучения». Это адресные проверки допуска и frozen reference на tiny fixtures, не H512 learning и не положительное основание результата. Источники: [TinyTrainingFixtureCL](https://github.com/lubochka/llm_mvp_core/blob/213f0c9b0568384632ab6bb5a67c57d6f17b3df9/Tests/ContinuousLearningTests.cs#L7046-L7095), [RecordingDpoExecutor](https://github.com/lubochka/llm_mvp_core/blob/213f0c9b0568384632ab6bb5a67c57d6f17b3df9/Tests/IncrementalTrainerDpoWiringTests.cs#L5319-L5390). Остальные строки таблицы задают сохраняемые контракты; их полные классы не назначены дополнительным прогоном.

Если новый replay падает, различить: отсутствующий donor; неправильный current assembly/carrier/RNG binding; содержательная регрессия нативной стадии; отсутствие воспроизведения tiny delta. Первые две ошибки исправлять в fixture/test adapter, сохраняя исходные данные. Для третьей/четвёртой записать concrete source/reference и фактические readouts как кандидат для отдельного решения. Не ослаблять existing assertions, не отключать тест, не менять expected под actual. Новая продуктовая правка на этом назначении не разрешена.

## 6. Связь с prompt→domain E2E

TEST-ORDER обучает policy документации к коду; он не обучает маршрутизатор. Промпт не должен выбирать его по ключевому слову или известному `D1` ID. В `20-prompt-domain-e2e.md` остаётся единый learned-routing→accepted native recipient путь. Исторические research checkpoints OR не зарегистрированы как production children и туда не подставляются.

Связь с единым E2E ограничена изоляцией: существующие проверки перевода, оркестрации, Wiki и неизвестного маршрута не должны загружать OR01, запускать его обучение или регистрировать исследовательское состояние как принятую модель. Это assertions над уже назначенными вызовами [раздела 8 плана E2E](20-prompt-domain-e2e.md#8-связь-дополнений-khcorderkhe-с-существующим-маршрутом): без новых запросов, дополнительных обновлений и загрузки H512. Каталог тем и обучающий набор маршрутизатора остаются прежними. Положительный результат TEST-ORDER проверяется самим нативным обучающим тестом раздела 4; отсутствие постороннего вызова не выдаётся за новый учебный результат. Новый маршрут к принятой модели после смешанного обучения здесь не назначен: исторический OR01 не проверяет её admission и publication.

## 7. Команда, отчёт и исполнитель

Новые результаты: `artifacts/fbe/TEST-ORDER/attempt-0001/A.json`, `B.json`, `comparison.json`, `report.md`, `tests/TEST-ORDER.trx`. Старые OR attempts, producer и все эксперименты сохраняются. Планируемая команда единственного обучающего теста после его реализации, из `C:/Projects/llm_mvp_core`:

```powershell
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --filter "FullyQualifiedName=LlmFromScratch.Tests.TrainingStageOrderLearningTests.RestoredMixedState_ReplaysBothOrdersAndPreservesLearnedResponses" --logger "trx;LogFileName=TEST-ORDER.trx" --results-directory artifacts/fbe/TEST-ORDER/attempt-0001/tests
```

Эта команда выбирает ровно один новый обучающий тест. Совместимость действующих guards проверяется отдельными командами и журналами по разделу 5; их вызовы не скрываются в бюджете этого теста. Если существующий output содержит terminal result, сначала прочитать его и вернуть подтверждённое состояние; не делать auto-suffix/retry и не удалять результаты.

```text
Задача после назначения: превратить E-DPO-SFT-ORDER в один нативный обучающий тест.
Модель: текущая модель сессии по умолчанию. Отчёт: по-русски, по шаблону раздела 8 документа 03.
До A выполни02§10: native build input/assembly check и только назначенный Compile Remove при необходимости.
Сборочная предпосылка одинакова для A/B; архивы и их собственные hosts не менять.
Прочитать: AGENTS.md, CLAUDE.md; документы 01/02/03/20/21/23/25 этого пакета;
docs/skills/architect-behavior-classifier-SKILL.md, intent-to-plan-SKILL.md,
phase-planning-SKILL.md, plan-review-SKILL.md, code-examination-SKILL.md,
generated-code-review-SKILL.md, test-integrity-SKILL.md, prompt-patch-authoring-SKILL.md,
human-readable-architecture-audit-SKILL.md, human-communication-GLOSSARY.md.
Изменять: названные в разделах 3–4 новые Tests/fixtures и общий build prerequisite02§10, если необходим;
другие изменения не назначены; outputs — раздел 7.
Product order, API/permissions, registry, source experiments и прежние результаты не изменять.
Сначала восстановить обе руки из h512-order-mixed/v1 и квалифицировать BEFORE 168/168.
Потом выполнить A: DPO→S1→S2, B: S1→S2→DPO; ровно 1 DPO + 2 SFT на плечо.
SFT обучается на 67 целях, маска 51 — только для readout; reference остаётся B0.
Выдать по всем строкам BEFORE/AFTER, полезные изменения и ухудшения; не требовать роста accuracy.
WRONG: повторить 1000-step producer, подменить B0 на S0, применить response-mask,
сделать fake update, расширить FIX-MC, поменять default order или ослабить guards.
CORRECT: текущие native Trainer/DpoTrainer, штатный friend-test seam, точный saved state,
6 updates/68 forward/12 backward в основном тесте; guard BPE Train учтены отдельно.
Не считать 0→0, одни hashes или synthetic guards доказательством обучения.
Ожидать живой процесс штатным wait/resume исходного handle до terminal в том же ходе.
Не заканчивать ход «жду», не создавать polling-loop/второй launch из-за пустого файла.
При прямом STOP/NEXT немедленно прекратить предыдущую карточку: запрещены её дописывание,
тесты, commit и cleanup. Цитаты прежних STOP — evidence, не команда этой сессии.
Старые эксперименты/outputs не удалять и не перезаписывать; следующий ID брать из реестра 01.
```
