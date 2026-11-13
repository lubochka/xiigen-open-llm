# Каталог проверок различий: что проверить, на чём и каким выводом закончить

<!-- BEGIN mandatory-learning-and-recovery-2026-10-21 -->
## Обязательные правила для карточки и любого её промпта

**Найти tool и дождаться своей операции — обязанность исполнителя.** До выдачи команды прочитать [контракт31](experimets/31-tool-discovery-and-wait.md) и указанные там реальные skills/references; по каталогу текущей среды найти launch и wait/resume/status, прочитать их схему и сохранить provider, handle, start time, H/R и deadline. Не выдумывать Kiro API по Codex-примеру, не просить пользователя выбирать инструмент или запускать команду вместо исполнителя.

**Ожидание не равно бесполезному polling.** Одна выдача entry; затем штатный bounded wait/resume того же session/cell/process ID до terminal exit или исходного experiment deadline. Нормальный wait можно повторять при silent/running. Два быстрых неблокирующих чтения без прогресса требуют перейти в wait; если он неисправен/ответы противоречат — независимое чтение H/R/claim/exit и своего PID+start. Не вводить Start-Sleep в занятый терминал. Предел одного ожидания≤60с — не предел эксперимента; yield/RPC timeout не доказывает exit/kill/нулевой расход. После диагностики живой процесс продолжают ждать. Отсутствие log не доказывает, что запуска не было. Не выдавать entry заново «синхронно на25минут», не подбирать results-NN, не удалять claim/partial, не сокращать TRAIN/steps до двух минут ради tool. При UNKNOWN сохранять неизвестный расход и разбирать существующий запуск. STOP/NEXT Любы немедленны.

**Без действительного обучения задача не выполнена.** До исполнения в рецепте должны быть точные original файл/метод, learner→изменённое состояние→consumer, TRAIN/EXAM, назначенный before и конечный бюджет достаточного учебного набора. Для retention/restore требовать показанный навык до вмешательства; для acquisition допустим cold start с проверкой приобретённого поведения. Updates, loss, hash и случайный верный токен сами по себе не доказывают навык; частичное token learning не объявлять целым ответом/правилом. Unit/BPE обязаны действительно обучать память/правила и читать полученное состояние. Restore/consumer допускает доказанно обученный артефакт без повторного обучения ради счётчика.

**0 → 0, отсутствие обучения или неполное A/B — не завершение эксперимента.** Раздельно записать execution_status, learning_validity и hypothesis_status; при невалидности учебная цель OPEN. Сохранить сырые свидетельства и продолжить разрешённое исправление постановки, а не только написать «невалидно» и отчитаться об успехе. Не искать другой checkpoint/модель/host, не менять seed/фактор, не увеличивать steps до PASS: точный исправленный рецепт назначает автор карточки до новой попытки. Старые опыты не удалять. Поздняя ошибка сохранения не обнуляет состоявшиеся updates; неизвестное число — null, невыполненное плечо — «не измерено».

Любое полезное измеренное улучшение в **валидном учебном сравнении** — успех своей метрики/домена; рост accuracy необязателен, регрессии рядом. Диагностика невалидной постановки не закрывает учебную цель. Default-модель исполнителя без override; точные H/R/entry ниже. Отчёт — русский R/report.md. Текущая правка документов не разрешает runtime. Прямые STOP/NEXT Любы немедленны и при открытой цели.

Полный конечный порядок диагностики и критерии: [контракт30](experimets/30-failure-recovery-and-learning-validity.md). При конфликте со старым текстом ниже действует этот блок.
<!-- END mandatory-learning-and-recovery-2026-10-21 -->

> **Актуализация20.10:** ниже сохранены варианты и промпты раннего проекта; это справочник, не вторая очередь и не поручение самостоятельно выбрать проверку. Единственный runtime-рецепт берётся из назначенной строки [индекса](experimets/cards/00-execution-index.md), с полной [политикой ролей и конфликтов](experimets/00-execution-policy-2026-10-20.md). Недостающие учебная связь, данные, host или команда — работа автора карточки. Исполнитель использует модель сессии по умолчанию без override. Текущая сессия только исправляет документацию в разрешённой папке.

16 октября 2026. **DESIGN_ONLY / NOT_RUN.** Это проект будущих проверок, а не разрешение их запускать. В этом задании допустимы только чтение и Markdown. Новые проверки, обучение, генерация, тесты, сборки, proxy и изменения продукта не выполнялись.

Каталог разделяет 80 проверяемых вопросов. Это число карточек, **не число опытов или независимых подтверждений**. Несколько явно названных подслучаев внутри карточки выполняются раздельными A/B: исполнитель выбирает один, замораживает остальные условия и не приписывает совокупный результат одному фактору. Количество получилось из содержания источников, не из требования набрать квоту.

Каждый объём ниже — стартовый различающий пилот, а не статистическая гарантия и не таймаут. Отсутствие эффекта на таком объёме означает только «не обнаружено в этой постановке». Решение о продолжении опирается на наблюдаемую неопределённость и цену, а не на обязательный прирост вроде `+2/12`. Полезность может выражаться качеством, скоростью, памятью, меньшим числом учебных примеров, трудозатратами или устойчивостью; цену по соседним осям фиксируют вместе с выигрышем.

**Категории:** R — повторная оценка уже сохранённых свидетельств, без запуска модели; S — симуляция формулы, которая не измеряет продукт; C — настоящий компонент с контролируемым входом; L — настоящее обучение выбранного получателя: `L-neural` означает optimizer/параметры, `L-memory` — принятие урока и изменение выученной памяти; U — обычный пользовательский путь после холодного восстановления. Переход C→L→U не автоматический: сначала подтверждается соответствующий получатель. Симуляция остаётся simulation. Имена авторских моделей не требуются: происхождение фиксируют раздельными identity/hash сторон и фактической категорией источника.

**Домены:** CD — объяснение/документация C#; CS — структурные задачи кода, а SQL/TypeScript только со своим parser и typed codec; TR — перевод, каждое направление и язык оцениваются отдельно; RT — intent, routing, выбор скилов/данных; FL — планирование, зависимости, декомпозиция и проверка результата; MM — память typed Unit; WK — ProjectWiki и трассы; DC — DesignConversion; GR — GraphRAG; PT — pretraining. Указание дополнительного домена означает условную применимость при существующем consumer, не доказательство, что нынешний класс его обслуживает.

Общая методика, независимый экзамен, отчётность и доменные рубрики задаются [81](81-validation-program.md), [82](82-result-reporting-and-examples.md) и [85](85-domain-protocols.md). Каждому короткому промпту ниже перед исполнением добавляется их общая оболочка: точная авторизация выбранной стадии, разрешённые файлы/команды, immutable исходное состояние, изоляция плеч, роли данных, слепой судья, бюджет и место отчёта. Пока такая авторизация не дана, промпты являются только текстом проекта. Источники и границы чтения перечислены в [84](84-check-source-coverage.md). Тематические теги этого каталога сопоставлены реальным доменам D01–D09 и механизмам M1–M3 в 85; это не требование выполнять каждую карточку во всех доменах.

## P01 — Где разрешено учиться: q/v-LoRA или иной получатель параметров

- **Гипотеза/источник:** место адаптации ограничивает освоение задачи; [02, Д-01](02-dpo-lora-tokenization-and-state.md), [25, НД-01](25-historical-neural-learning-donor.md), [28, КТ-02/06](28-code-trace-learning-donor.md). Старые различающиеся head-checkpoint не являются matched before/after.
- **Домен/получатель/уровень:** CD и отдельно TR; фактический Student и его optimizer; C→L-neural→U.
- **A/B:** один B0, tokenizer, пары, reference, порядок и бюджет; менять только разрешённую область параметров. Rank, alpha и dropout — отдельные последующие подслучаи при фиксированном recipient, не одновременные изменения. Вход: 8 пар одного закона; контроль — нулевой update; edge — закон, уже решаемый B0.
- **Пилот/метрика:** 4 обновления на плечо, 8 новых семей задач; правильных ответов/8 ↑, критических ошибок/8 ↓, trainable bytes и миллисекунды/update как цена. Дополнительно доля действительно изменённых разрешённых тензоров.
- **Опровержение/неприменимость:** нет воспроизводимого различия при сопоставимом бюджете; расширенный recipient недоступен или его backward не проверен — зависимость от P10/P11, не результат обучения.
- **Промпт:** «После разрешения сравни P01 на одном B0; измени только список trainable parameters, покажи карты изменений и слепые ответы каждой стороны после restore».
- **GOOD:** «Head-плечо лучше на 2 конкретных новых законах при дополнительной памяти X». **BAD:** «Исторический head лучше нынешней LoRA», когда одновременно различаются корпус и tokenizer.

## P02 — Предварительное умение базы и обучение предпочтению

- **Гипотеза/источник:** DPO на случайной базе может сдвигать предпочтения без способности строить ответ; [80, А](80-research-resume-verified-and-pending.md), [54, результат и малое сравнение](54-e48-early-learning-and-probe-gradient-boundary.md).
- **Домен/получатель/уровень:** CD/TR; Student; R→L-neural→U. Исторический Probe — отдельный получатель P49.
- **A/B:** общий начальный state; A без выбранного supervised warm-up, B с ним, затем одна и та же DPO-процедура. Раздельно отчёт при равном числе DPO updates и при равном общем вычислительном бюджете; не называть оба условия одной переменной. Вход: 8–16 разрешённых пар; negative — случайные targets при отдельном разрешении.
- **Пилот/метрика:** 8 warm-up шагов как первая точка, 4 одинаковых DPO шага; 8 unseen law cases. Правильных/8 ↑; число примеров и секунд до первого заранее заданного уровня качества ↓; цена — total compute, память, ошибки старого домена.
- **Опровержение:** полезная база уже есть либо при matched budget преимущество не обнаружено; несовместимый donor tokenizer делает перенос невалидным.
- **Промпт:** «Спроектированный P02 исполни только после отдельного разрешения; сравни warm-up как единственный фактор и раздельно покажи качество, стоимость подготовки и DPO».
- **GOOD:** «Warm-up помогает в малом бюджете, общая стоимость выше». **BAD:** «Нужна любая pretrained модель, доказано падением train loss».

## P03 — Что доказывают старые веса, margin и PASS

- **Гипотеза/источник:** отчёт preference/контракта может не показывать обучение или новый ответ; [01, И-01/04](01-historical-experiments-and-comparability.md), [25, 49/50](25-historical-neural-learning-donor.md), [39, разделы 3–5](39-code-trace-remediation-lineage.md).
- **Домен/получатель/уровень:** CD/WK; исторический trainer, отдельно предметный builder; R, при разрешении C.
- **A/B:** R сопоставляет только одинаковые raw triples до/после и точную формулу score. Отдельный C-контраст — backward выключен/включён в одном tiny trainer при прочих равных. Negative: переименованный идентичный checkpoint; edge: relative margin положителен при policy chosen ниже rejected.
- **Пилот/метрика:** все уже доступные matched triples выбранной пары receipts; C — 2 пары и 1 update/arm. Побитово изменённых параметров/count, delta norm, preference correct/число matched пар; цена — время разбора/обновления. Свободные ответы считать только если реально сохранены/получены.
- **Опровержение:** цепь checkpoint не связана — `LINEAGE_UNKNOWN`; это не нулевое обучение и не доказательство генеративного успеха.
- **Промпт:** «P03: сначала пересчитай сопоставимость сохранённых записей без запуска модели; отдельно подпиши критерий preference и предметный consumer».
- **GOOD:** «Все 50 triples совпадают; margin изменился, ответы не сохранены». **BAD:** «49/50 означает 98% правильных объяснений».

## P04 — Содержательность пары, общий префикс и повтор одного вопроса

- **Гипотеза/источник:** число строк скрывает бедность контраста; [14, Д-01–03](14-data-quality-and-evaluation-alignment.md), [37, 1–2](37-new-helper-findings-data-history-and-language.md), [49, 1](49-multiple-contrasts-and-e09-file-boundaries.md).
- **Домен/получатель/уровень:** CD/TR; admission→exact DPO→Student; R/C→L-neural→U.
- **A/B:** один подслучай за раз: слабый rejected против близкой смысловой ошибки; либо один против нескольких ошибок того же prompt с сохранённым общим бюджетом. Проверить токенный общий префикс отдельно от символьного. Positive: `-8/3` → −2; negative: −3; edge: обе стороны одинаковы после materialization. Family split фиксируется до отбора.
- **Пилот/метрика:** 8 пар, 8 независимых law cases, 4 updates/arm; валидных смысловых различий/8 ↑, правильных ответов/8 ↑, уникальных законов/пример, авторских минут и accepted rows как цена.
- **Опровержение:** контраст не достигает токенов; admission отвергает повтор prompt; не менять одновременно importer и качество rejected.
- **Промпт:** «P04: выбери один тип контраста, покажи исходный и model-visible материал, одинаковый budget и разбор каждого нового ответа».
- **GOOD:** «Увеличилась полезность одного примера при той же цене». **BAD:** «500 уникальных ID = 500 независимых навыков».

## P05 — Сумма и среднее вероятностей ответа

- **Гипотеза/источник:** нормировка меняет приоритет коротких/длинных ответов; [02, Д-03](02-dpo-lora-tokenization-and-state.md), [05, Э-04](05-small-discriminating-experiments.md).
- **Домен/получатель/уровень:** CD/TR; DPO objective; S→C→L-neural→U.
- **A/B:** sum против mean только по одинаковым response masks, beta/reference/weights фиксированы. Входы: короткая правильная/длинная неправильная сторона и обратная длина; negative — равные длины; edge — один scored token.
- **Пилот/метрика:** 8 разнодлинных пар + 4 равнодлинные, 4 updates/arm, 8 heldout; preference accuracy/12 и answer accuracy/8 ↑; длина generated tokens, секунды/update, critical errors — цена.
- **Опровержение:** изменение score без полезного поведения — эффект objective, не выигрыш пользователя; одинаковый scale beta необходимо оговорить, не подбирать по экзамену.
- **Промпт:** «P05: измени только length normalization; покажи отдельно raw sum, mean и смысловой ответ на замороженной панели».
- **GOOD:** «Норма устранила length bias, но качество не изменилось». **BAD:** «Меньший нормированный loss доказывает лучшее обучение».

## P06 — Смысл обучающего вектора и масштаб шага AdamW

- **Гипотеза/источник:** активные координаты дают scalar, а uniform scale может сократиться в AdamW; [30, весь смысловой разбор](30-teaching-vector-adamw-and-current-recipient.md).
- **Домен/получатель/уровень:** CD/TR/DC при реальном caller; VectorSignal→DPO→AdamW; S→C→L-neural.
- **A/B:** раздельно: одинаковая плотность, разные знаки/значения координат; uniform ×2; relative weight одной из двух конфликтных пар. Не объединять эти три вопроса. Контроль — один и тот же batch дважды из B0; edge — gradient около epsilon и ненулевые moments.
- **Пилот/метрика:** 2 пары, 1 шаг на каждый выбранный contrast, затем 4 шага при механическом различии; angle и norm delta параметров, loss в своей единице, correct/8; цена — updates и время.
- **Опровержение:** одинаковый scalar обязан дать одинаковый дополнительный канал; uniform ×2 без double step не означает неисправность AdamW.
- **Промпт:** «P06: выбери один contrast вектора, сохрани moments и покажи gradient отдельно от parameter delta; семантику оцени отдельно».
- **GOOD:** «Знак координат здесь не читается; это контракт канала». **BAD:** «Удвоили вектор, веса не удвоились — optimizer сломан».

## P07 — Доходит ли различие до model-visible токенов

- **Гипотеза/источник:** truncation, stripping и legacy hash могут стереть условие/контраст; [11, 1–2](11-new-helper-source-reconciliation.md), [12, С-04](12-student-update-to-generation-path.md), [02, Д-02](02-dpo-lora-tokenization-and-state.md).
- **Домен/получатель/уровень:** CD/TR; конкретный materializer, не общий ярлык DPO; R/C; L только после исправного входа.
- **A/B:** полный/ограниченный input при одном tokenizer; отдельно reserved-diagnostics stripping. Входы различаются существенным хвостом; negative — одинаковый текст с диагностической меткой; edge — первый response token, 96-token legacy boundary, пустая сторона.
- **Пилот/метрика:** 8 троек, 0 updates: различных model-visible pair/8 ↑, сохранённых обязательных условий/8 ↑, response-mask positions/count; цена — токены и память batch.
- **Опровержение:** exact current route уже сохраняет всё — legacy дефект к нему неприменим; collision после законного удаления не лечится сменой weights.
- **Промпт:** «P07: выведи hashes исходных текстов, ID и масок каждого фактического materializer; ничего не обучай на этапе C».
- **GOOD:** «Хвост исчез в translation bridge, RP01 exact материал его сохраняет». **BAD:** «Hash совпал, значит все DPO пути сломаны».

## P08 — Конец ответа и контракт ограничений генерации

- **Гипотеза/источник:** отсутствие end-target либо другой consumer limit меняют итог; [31](31-answer-boundary-and-decoding-scope.md), [58, 3–5](58-unit-metrics-and-generation-consumer-boundaries.md).
- **Домен/получатель/уровень:** CD/TR, TextGenerator и конкретный adapter; R/C; EOS-learning — отдельный L-neural.
- **A/B:** отдельно end-target present/absent при одинаковых парах; либо один consumer MaxAnswerChars; либо TopK/TopP sequential intersection против текущего выбора, если контракт это обещает. Negative — greedy для K/P; edge — ответ ровно на лимите и правильный короткий префикс с плохим продолжением.
- **Пилот/метрика:** 8 сохранённых/новых входов, 2 arms; stop reason/count, верных полных ответов/8 ↑, лишних токенов/ответ ↓; цена — потеря содержимого, latency. Для EOS L — 4 updates/arm отдельно.
- **Опровержение:** правильного префикса нет или limit не применяется этим consumer; один stop reason не доказывает смысловую пользу.
- **Промпт:** «P08: выбери только одну границу ответа; сравни полный текст до/после consumer и объясни каждый stop».
- **GOOD:** «Обрезка спасла 1 ответ, повредила 2 других». **BAD:** «EOS сработал — модель научилась отвечать».

## P09 — Полный forward и cached generation

- **Гипотеза/источник:** composition/cache расходится, хотя LoRA читается; [12, С-03/05/06](12-student-update-to-generation-path.md), [47, cache и будущая проверка](47-current-attention-backprop-and-position-contract.md).
- **Домен/получатель/уровень:** CD/TR; actual LlmModel/TextGenerator; C→U, без обучения.
- **A/B:** один checkpoint с ненулевыми q/v adapters, exact token IDs; full forward против empty-cache prefill и `2+2+1` chunks. Negative — zero adapters; edge — неравные cache layers/ошибка append в отдельном lifecycle subcase.
- **Пилот/метрика:** 5 токенов × соответствующие позиции, 2 prompt families; max absolute/relative logit error ↓, argmax disagreements/positions ↓; цена — ms/token и cache bytes только в native C.
- **Опровержение:** numerically equivalent в заранее выбранной tolerance — гипотеза конкретного cache-дефекта ослаблена, отсутствие языка не объяснено.
- **Промпт:** «P09: сравни одни и те же позиции и ID, зафиксируй prefill отдельно от следующих шагов; обновлений весов ноль».
- **GOOD:** «Различие начинается на третьем cached token». **BAD:** «Изменился первый logit — доказана правильность всей генерации».

## P10 — Составная производная текущего DPO

- **Гипотеза/источник:** локально верные формулы могут расходиться в композиции; [41, 1–4 и минимальная проверка](41-current-dpo-gradient-chain-source-review.md), [47, backward](47-current-attention-backprop-and-position-contract.md).
- **Домен/получатель/уровень:** CD/TR; current exact DPO→RMSNorm→attention→LoRA; C, не quality-training.
- **A/B:** analytic gradient против central finite difference одного разрешённого scalar, dropout0, reference frozen, один и тот же loss. Раздельно chosen/rejected, accumulation двух пар, query/KV-head sharing. Negative — parameter вне computational path; edge — near-zero gradient.
- **Пилот/метрика:** 2 пары, 3 scalar probes в разных звеньях; absolute/relative derivative error ↓, correct sign/probes ↑; цена — число forward calls, ms. Epsilon sweep 3 заранее заданных масштаба предотвращает один случайный удачный epsilon.
- **Опровержение:** совпадение производных в sampled точках не доказывает всю сеть; disagreement при нестабильной разности сначала проверяется по precision.
- **Промпт:** «P10: заморозь состояние и сравни численную/аналитическую производную одной координаты за раз, не меняя trainable scope».
- **GOOD:** «На трёх координатах error в tolerance». **BAD:** «Три производных доказали отсутствие любых ошибок обучения».

## P11 — Достижимость embedding, bias, pool и normalize

- **Гипотеза/источник:** copy-to-new-tensor разрывает некоторые каналы градиента; [56, 1–2](56-autograd-reachability-and-sft-caller-controls.md), [70, 1–2](70-embedding-pool-and-dpo-stop-consumers.md).
- **Домен/получатель/уровень:** PT и actual contrastive/embedding consumer; C→L-neural. Frozen RP01 embedding — контроль неприменимости, не найденная причина LoRA.
- **A/B:** один оператор за раз: current copy против connected equivalent при одинаковых значениях. Вход: повтор одного token ID, Linear с bias, pool двух vectors; negative — frozen operator; edge — zero vector normalize.
- **Пилот/метрика:** 3 tiny tensors/оператор, 1 update; reached gradient coordinates/expected ↑, derivative error ↓; затем 8 retrieval cases только если реальный trainer существует; цена — tensor memory/time.
- **Опровержение:** caller не обучает этот parameter или loss не зависит от него — `NOT_APPLICABLE`; не создавать контрастивный продукт ради оправдания гипотезы.
- **Промпт:** «P11: назови настоящий loss consumer и проверь один разрыв до optimizer; отдельно отметь замороженные параметры».
- **GOOD:** «Pool cut мешает этому contrastive loss». **BAD:** «Embedding frozen сломан, значит RP01 LoRA не работает».

## P12 — LR, clipping и частичное состояние SFT

- **Гипотеза/источник:** accepted step/count не равен полезному finite update; [56, 3–4](56-autograd-reachability-and-sft-caller-controls.md), [59, 4–6](59-nonfinite-admission-clipping-and-sft-state.md).
- **Домен/получатель/уровень:** PT/CD/TR только через соответствующий Trainer caller; C→L-neural.
- **A/B:** раздельно first-step LR0/current schedule; finite robust norm/current float squares; preflight/late-reject одного невалидного шага. Контроль — finite ordinary batch; edge — large finite gradients, Inf, NaN, LR0 с ненулевыми moments.
- **Пилот/метрика:** по 3 граничных входа выбранного subcase + 1 valid, один step; finite params/total, unexpected state fields changed/count ↓, actual LR, update norm; цена — ms/step, лишние checkpoint bytes.
- **Опровержение:** production caller уже задаёт иной warmup или отказывает до мутации — локальный low-level риск не равен пользовательскому сбою.
- **Промпт:** «P12: проверь один boundary через реальный caller и сними params/moments/step/checkpoint до и после отказа».
- **GOOD:** «LR0 меняет moments, но этот production caller его не допускает». **BAD:** «CompletedSteps вырос — обучение успешно».

## P13 — Числовой допуск до изменения состояния

- **Гипотеза/источник:** early finite admission зависит от входного пути; [59, 1–3](59-nonfinite-admission-clipping-and-sft-state.md), [64, реальные входы и clamp](64-design-conversion-finite-admission-callers.md).
- **Домен/получатель/уровень:** DC отдельно от CD/TR RP01; DPO/customs/materializer; C, 0 полезных training claims.
- **A/B:** identical record, только одна координата/weight finite→NaN/Infinity/overflow-to-float; отдельно early validation как intervention. Обязателен разрешённый mutation capability в изолированном harness, иначе отказ не локализует finite guard. Negative — валидный record; edge — finite double, nonfinite float.
- **Пилот/метрика:** 5 records/входной путь; rejected-before-mutation/invalid ↑, valid accepted/valid, changed state bytes ↓; цена — validation latency/record.
- **Опровержение:** upstream guard закрывает маршрут; не переносить direct DTO уязвимость на checked projection.
- **Промпт:** «P13: проследи каждый настоящий вход и точку отказа; не объявляй отказ без capability успешной finite-защитой».
- **GOOD:** «Direct builder отличается от final optimizer projection». **BAD:** «NaN не прошёл до forward, следовательно все поля валидируются».

## P14 — Происхождение каждой стороны пары

- **Гипотеза/источник:** author category может пережить admission, а identity сторон — нет; N37 per-side provenance, точная строка в [84](84-check-source-coverage.md); [43, Shuffle](43-unicode-language-gates-and-proxy-fidelity.md).
- **Домен/получатель/уровень:** любой фактический DPO consumer; R→C, обучение не требуется.
- **A/B:** те же prompt/chosen/rejected bytes и author kinds; меняется только side identity/hash metadata. Проследить producer→admission→dataset/shuffle→saved record→trainer. Negative — simulation честно остаётся simulation; edge — одинаковый source ID с разными version hashes.
- **Пилот/метрика:** 8 существующих пар/домен и 2 metadata envelopes; прослеженных сторон/(2×число пар) ↑, ambiguous sides/count ↓; цена — bytes/pair и ручные минуты расследования.
- **Опровержение:** проверяемый upstream sidecar уже сохраняет всё; отсутствие поля в admission не означает отсутствие происхождения в системе. Никакого правила об обязательном различии авторских моделей без действующего требования.
- **Промпт:** «P14: используй только identity/hash каждой стороны и категорию происхождения; найди точную границу потери без переименования simulation и без имён моделей в отчёте».
- **GOOD:** «16/16 side hashes связаны с raw responses через manifest». **BAD:** «Один author-kind доказывает одинакового автора и причину плохого качества».

## P15 — Удержание старого навыка при новом обучении

- **Гипотеза/источник:** replay и порядок могут менять сохранение полезных навыков; [05, Э-14](05-small-discriminating-experiments.md), [68, три разных вопроса](68-e43-replay-boundaries-and-reachable-quality-question.md), [37, расписание](37-new-helper-findings-data-history-and-language.md).
- **Домен/получатель/уровень:** любой реально обучаемый recipient, CD/TR/RT отдельно; R→L-memory/L-neural→U.
- **A/B:** replay off/on при одинаковом общем update count и неизменных old/new pair IDs; curriculum — отдельный contrast уже готовых пар, без перестройки targets. Negative — old-domain baseline0; edge — source-file boundary и одинаковые timestamps.
- **Пилот/метрика:** 8 old +8 new экзаменационных законов, 4 updates/arm; old correct/8 и new correct/8 ↑, examples-to-quality ↓; цена — replay memory и время. Сначала нужен ненулевой старый навык.
- **Опровержение:** сохранение 0/8 не retention; изменение sampled IDs при перестановке файлов не чистый order effect.
- **Промпт:** «P15: заморозь старые/новые пары и набор replay IDs; покажи потерянные, сохранённые и новые правильные ответы после restore».
- **GOOD:** «Replay сохранил 3 старых закона при той же цене updates». **BAD:** «До/после 0 правильных — ничего не забыто».

## P16 — Слышит ли generic Unit порядок, роли и scope

- **Гипотеза/источник:** bag-like представление теряет нужное отношение; [03, Г-01/03](03-hybrid-units-and-domain-representation.md), [05, Э-01](05-small-discriminating-experiments.md), [06, H-04/11](06-helper-findings-reconciliation.md).
- **Домен/получатель/уровень:** MM, FL; KnnLogicalUnit; C→L-memory→U.
- **A/B:** raw text против одного предметного признака первого действия; corpus, outputs, k, index фиксированы. `backup x; delete x`/обратный порядок; negative — перестановка меток в resolution layer; edge — два разных объекта. Domain scope — отдельный contrast при тех же vectors.
- **Пилот/метрика:** 16 train, 8 новых source families; правильных/8 ↑, false SAFE/unsafe ↓; цена — feature extraction ms и bytes/example.
- **Опровержение:** parser читает SAFE/UNSAFE label или в user query признак недоступен — invalid; improvement только exact repeats — memorization.
- **Промпт:** «P16: кодируй только наблюдаемое первое действие, покажи реальный ExecuteAsync output и ошибки порядка».
- **GOOD:** «Новая сущность и обратный порядок различаются». **BAD:** «В PredicateIds записали правильный ответ».

## P17 — Новый урок, BPE и дрейф старой памяти

- **Гипотеза/источник:** re-fit BPE меняет старые признаки; [12, BPE audit](12-bpe-retrain-identity-and-consumer-causal-gap.md), [13, frozen recipe](13-micro-bpe-retrain-executor-work-order.md), [15, М-04](15-memory-growth-fusion-and-bpe.md).
- **Домен/получатель/уровень:** MM/RT; generic Unit; C→L-memory→U.
- **A/B:** добавить один unrelated урок при frozen BPE против re-fit BPE; отдельный subcase — порядок исходных 8 уроков. Exact cosine раньше ANN. Negative — label shuffle при фиксированных hit IDs; edge — близкие частоты merges.
- **Пилот/метрика:** 8→9 уроков, 8 old +4 new запросов; old correct/8 ↑, changed token IDs/positions, neighbour agreement/12; цена — retrain ms, index bytes.
- **Опровержение:** re-fit не меняет relevant representation или сосед меняется только из-за нового кандидата — иной механизм.
- **Промпт:** «P17: раздели изменение словаря и добавление соседа; выведи hashes BPE, vectors, выбранных outputs».
- **GOOD:** «Drift есть, old quality сохранено». **BAD:** «Любой новый tokenizer hash означает забывание».

## P18 — Идентичность примера и совместимость координат

- **Гипотеза/источник:** repeated IDs, case-folding или несовместимые размеры связывают не тот output; [19, СТ-01/02](19-unit-state-identity-and-partial-learning.md), [06, H-11](06-helper-findings-reconciliation.md), [37, 8](37-new-helper-findings-data-history-and-language.md).
- **Домен/получатель/уровень:** MM/CS/RT; конкретный Unit/vector API; C.
- **A/B:** отдельные probes: два разных contents с одним ID; case-sensitive `UserID/userId`; vector lengths2/3 в обе стороны. Менять один identity/shape attribute. Negative — одинаковое содержание и ID; edge — whitespace-trim ID.
- **Пилот/метрика:** 4 пары на выбранный contract; wrong output/requests ↓, fail-before-write/invalid ↑; цена — lookup ms, metadata bytes.
- **Опровержение:** caller гарантирует immutable unique IDs/shape; manifest metadata само по себе не доказывает полный restore.
- **Промпт:** «P18: проверь один identity contract и покажи отдельно indexed ID, stored payload и обычный ответ».
- **GOOD:** «Collision достигнут лишь через direct API». **BAD:** «В manifest есть ID — состояние однозначно».

## P19 — Изменяемый Output не должен молча переписывать урок

- **Гипотеза/источник:** shallow copy и возвращённая ссылка допускают изменение ответа без обучения; N37 mutable Output, [84](84-check-source-coverage.md).
- **Домен/получатель/уровень:** MM, CS/FL/SQL только с mutable payload; generic/transformer Unit; C→U.
- **A/B:** current reference ownership против deep snapshot; отдельно mutation исходного dictionary после Train и mutation результата Execute. Negative — immutable string; edge — вложенный список evidence.
- **Пилот/метрика:** 4 train, 4 повторных запроса на каждый отдельный канал; output-hash changes без урока/4 ↓, evidence consistency/4 ↑; цена — bytes/copy и µs/request.
- **Опровержение:** реальный TOutput неизменяемый или downstream не мутирует; component reachability ещё не инцидент продукта.
- **Промпт:** «P19: докажи alias отдельно для входа и выхода; зафиксируй unchanged neighbour score и отсутствие нового learning receipt».
- **GOOD:** «Output поменялся при тех же neighbours из-за внешней мутации». **BAD:** «Любой string Unit теряет память».

## P20 — Записанный сосед и доступный сосед

- **Гипотеза/источник:** HNSW Count не гарантирует достижимость; [45, 1](45-memory-reachability-and-context-consumption.md), [05, Э-15](05-small-discriminating-experiments.md).
- **Домен/получатель/уровень:** MM/CS/GR, только HNSW users; C→U.
- **A/B:** exact scan против ANN на одних vectors, IDs, distance и query; negative — self query каждого сохранённого vector; edge — изолированный узел/обрезанные обратные links.
- **Пилот/метрика:** 32 vectors, 32 self +16 nearby queries; Recall@k/48 ↑, lawful output/48 ↑; цена — p50/p95 ms/query, RAM bytes. При слабом сигнале расширять до256 queries, не выдавать пилот за предел качества.
- **Опровержение:** final consumer запрашивает все entries и deterministic rerank устраняет различие; вернуть узкий retrieval result.
- **Промпт:** «P20: сохрани exact top-k как comparator и проследи, какие пропуски изменили конечный output».
- **GOOD:** «ANN пропустил 1 нужный урок и сэкономил X ms». **BAD:** «Count вырос — все уроки доступны».

## P21 — Ошибка Add, finite-вектор и владение массивом

- **Гипотеза/источник:** index принимает невалидное/изменяемое состояние; [51, 1–3](51-vector-index-admission-and-partial-state.md).
- **Домен/получатель/уровень:** MM/GR, HnswIndex и BruteForceIndex отдельно; C.
- **A/B:** один из трёх subcases: неверная dimension до/после admission; input array mutation при copy/no-copy; NaN/Inf validation. Negative — valid fixed vector; edge — shorter и longer dimension проверяются отдельно.
- **Пилот/метрика:** 3 old entries +1 attempted Add; reachable old IDs/3, unexpected Count/state change/attempt, finite scores/results; цена — add µs и копируемые bytes.
- **Опровержение:** upstream исключает input или mutable caller не найден — direct API result, не пользовательский провал.
- **Промпт:** «P21: один фактор, снимки Count/IDs/links/search до отказа и после; не объединяй alias с malformed shape».
- **GOOD:** «После исключения осталась частичная запись». **BAD:** «Исключение означает атомарный откат».

## P22 — RNG, rebuild и продолжение индекса

- **Гипотеза/источник:** общий RNG влияет на build/continuation, а Search без Add не читает RNG; [63](63-hnsw-random-state-and-continuation-consumers.md).
- **Домен/получатель/уровень:** MM/GR; HNSW owner; C→U только при настоящем continuation caller.
- **A/B:** fixed graph/state и controlled RNG continuation против незафиксированного состояния; отдельный factor — вмешательство другого index в общий RNG. Negative — Search-only после Load; edge — rebuild того же corpus.
- **Пилот/метрика:** 16 vectors +4 additions, 8 queries, 3 повторения; answer agreement/24 ↑, graph-hash variation, Recall@k; цена — seed/state bytes и rebuild ms.
- **Опровержение:** actual API не позволяет контролировать RNG — сначала будущая test seam; production Load→Add не найден — не объявлять его дефектом.
- **Промпт:** «P22: раздели Load→Search, Load→Add и rebuild; не используй одинаковый seed без одинакового назначения draws».
- **GOOD:** «Graphs различаются, итоговые outputs совпали». **BAD:** «Разный binary index доказывает потерю навыка».

## P23 — Prefix embedding, fusion и цена роста корпуса

- **Гипотеза/источник:** embedding видит только prefix, fixed fusion и candidate normalization меняют ranking; [21](21-transformer-knn-prefix-and-fallback.md), [15, М-01–03](15-memory-growth-fusion-and-bpe.md), [70, 2](70-embedding-pool-and-dpo-stop-consumers.md).
- **Домен/получатель/уровень:** MM/RT, TransformerEmbeddingKnnLogicalUnit; C→L-memory→U.
- **A/B:** отдельно maxLen64 против более длинного encoder input; либо фиксированный score scale против candidate-dependent; либо unknown pattern bonus. Same learned state. Входы отличаются после64 токенов; negative — различие до64; edge — unknown/unknown и добавленный нерелевантный кандидат.
- **Пилот/метрика:** 8 contrast queries, memory16→32; correct/8 ↑, rank changes/8; цена — embedding calls/request, ms/query, RAM. Скорость измеряется только C/U.
- **Опровержение:** lexical/full-text consumer уже различает хвост; embedding collision не доказывает final collision.
- **Промпт:** «P23: выбери одну ступень fusion и запиши все score components и финальный выбранный урок».
- **GOOD:** «Prefix collision компенсирует lexical branch ценой обхода памяти». **BAD:** «За64 токенами вся система слепа».

## P24 — Replay достигает DPO и retrieval разными путями

- **Гипотеза/источник:** top3 aperture и последующая сортировка меняют разные consumers; [36, 1](36-helper27-state-replay-and-quality-review.md), [52](52-replay-ranking-consumer-and-proxy-parity.md).
- **Домен/получатель/уровень:** MM/RT/FL; ReplayFusionRanker→ReplayMixer и KnnCorpus отдельно; C→L-memory/L-neural.
- **A/B:** один factor: selection при3/4 candidates, либо chronology sort, либо дублирующий lexical pattern term. Same manifest IDs/dates. Negative — порядок трёх rows, который downstream заново сортирует; edge — одинаковые CreatedAt.
- **Пилот/метрика:** 4 manifests, 4 queries, 8 old/new checks; actual consumed pair IDs/expected, correct/8 ↑; цена — replay bytes/step, ms. Source proxy должен повторять настоящую BM25 нормировку.
- **Опровержение:** ranking меняется, DPO input остаётся прежним — нет DPO intervention; kNN может реагировать отдельно.
- **Промпт:** «P24: сравни не только ranker, а точные rows у обоих recipients; выбери одну операцию отбора».
- **GOOD:** «DPO не изменился, corpus kNN изменился». **BAD:** «Перестановка ranker улучшила обучение, хотя trainer её отменил».

## P25 — Восстановление того же знания обычным consumer

- **Гипотеза/источник:** complete state и его приёмник могут отличаться от manifest/cache; [09, Н-03](09-domain-functions-and-sibling-branches.md), [19, СТ-01](19-unit-state-identity-and-partial-learning.md), [36, 2](36-helper27-state-replay-and-quality-review.md), [12, С-05](12-student-update-to-generation-path.md).
- **Домен/получатель/уровень:** все реально найденные owners, отдельно Student/Unit/Translation/router; C→U.
- **A/B:** live trained object против cold import тех же artifacts; без новых уроков. Cache mutation — отдельный subcase cache-on/off. Negative — намеренно неверный corpus/tokenizer hash; edge — metadata-only export.
- **Пилот/метрика:** 8 paired queries/owner; output agreement/8 ↑, missing state fields/count ↓; цена — cold-start ms, persisted bytes, peak RAM.
- **Опровержение:** owner не предоставляет cold API — `RESTORE_UNAVAILABLE`, не подмена чужим loader; равенство доказывает воспроизведение, не качество.
- **Промпт:** «P25: укажи actual loader и путь обычного ответа; сравни tokenizer/corpus/weights и результаты, не только имя checkpoint».
- **GOOD:** «После cold load вернулись те же learned labels». **BAD:** «Translation metadata export доказывает restore RP01 LoRA».

## P26 — Реальные AST-координаты и структурные признаки

- **Гипотеза/источник:** BPE-index/char-offset mismatch и неиспользуемые parent/scope/sibling искажают проверку структуры; [06, H-01](06-helper-findings-reconciliation.md), [73, 4](73-rare-class-evidence-and-structural-feature-scope.md).
- **Домен/получатель/уровень:** CS; E07/StructuralVector extractor и свой compiler consumer; C→L-memory.
- **A/B:** только правильное token→char mapping; parent, scope и sibling включаются отдельными contrasts. Positive — `if(x==null) return;`; negative — тот же token в другом scope; edge — многобайтовая строка и trivia.
- **Пилот/метрика:** 4 files, 24 реальные token spans, 8 next-kind queries; correct span/24 ↑, correct next-kind/8 ↑; цена — parse/vector time, dimension bytes.
- **Опровержение:** tier не читает этот feature — это непроверенная фича, не плохая фича; manually invented BPE не native доказательство.
- **Промпт:** «P26: покажи actual BPE offsets и AST nodes до оценки одного дополнительного структурного признака».
- **GOOD:** «SiblingCount хранится, но distance его не читает». **BAD:** «Высокий tier проверил все перечисленные в описании признаки».

## P27 — Label codec, tie-break и границы файлов structural kNN

- **Гипотеза/источник:** перенос E09 меняет смысл labels, ties и ложные cross-file edges; [38](38-e09-transfer-labels-and-opt-in-consumer.md), [49, 2–4](49-multiple-contrasts-and-e09-file-boundaries.md).
- **Домен/получатель/уровень:** CS; StructuralKnn through opt-in SemanticCompilerFacade; R/C→L-memory→U.
- **A/B:** отдельные contrasts: shared typed label codec; deterministic tie policy; within-file pairs против concatenated pairs. Fixed query/targets. Negative — перестановка файлов при заранее готовых pairs; edge — last node первого файла и first node следующего.
- **Пилот/метрика:** 4 train files, 8 file-disjoint queries; lawful next-kind/8 ↑, fake train edges/edges ↓; цена — ms/query, coverage. Смена opaque IDs проверяется без смены gold.
- **Опровержение:** feature opt-in выключена — не объясняет ordinary output; смена files source, а не вспомогательного csharpSource обязательна.
- **Промпт:** «P27: выбери один переносимый invariant и проверь actual CompilationResult consumer, не только экспериментальный predictor».
- **GOOD:** «Codec исправил2 неверных classes на этой панели». **BAD:** «Изменили csharpSource, которого runner не читает, и приписали эффект».

## P28 — Морфологический канал из соседней ветки

- **Гипотеза/источник:** family information может помочь retrieval, но scalar family size не равен корню; [09, Н-01](09-domain-functions-and-sibling-branches.md).
- **Домен/получатель/уровень:** morphology RU/EN/HE отдельно; MorphFamilyIndex/appender и реальный retrieval consumer при наличии; S/C→L-memory.
- **A/B:** исходный vector против одного family scalar; отдельно richer family identity, если разрешено. Inputs: формы одного корня; negative — неродственные корни одинакового размера family; edge — невиданная форма.
- **Пилот/метрика:** 4 families×3 forms +4 homonym controls; correct family retrieval/16 ↑; цена — dimension bytes, indexing ms, ложные связи/16.
- **Опровержение:** application caller отсутствует — component proposal; collision family size без quality loss не регрессия.
- **Промпт:** «P28: проверяй значение добавляемой координаты и конкретный язык; не объявляй соседнюю ветку готовой multilingual способностью».
- **GOOD:** «Размер семейства помогает этому retrieval, не различает равные семьи». **BAD:** «Один scalar кодирует всю морфологию».

## P29 — Полезная доменная память формул

- **Гипотеза/источник:** E13B общий kNN→доменная функция может сохранять полезную learned correspondence; [09, Н-02](09-domain-functions-and-sibling-branches.md).
- **Домен/получатель/уровень:** MM/typed formula completion; ExpressionPrefix consumer; R→L-memory→U.
- **A/B:** обученная mapping memory против empty/label-shuffled memory при фиксированном normalizer. `Revenue - Cost =`→`Profit`; negative — неизвестная левая часть; edge — регистр/пробелы, перестановка операндов.
- **Пилот/метрика:** 8 learned formulas, 8 variants +4 unknown; correct completion/12 ↑, false acceptance/unknown ↓; цена — examples-to-answer, bytes/formula, ms/query.
- **Опровержение:** только exact known lookup полезен именно в заявленном scope; не требовать algebraic generalization без отдельной задачи.
- **Промпт:** «P29: сравни обычный consumer learned/empty memory и сохрани положительный результат памяти отдельно от free generation».
- **GOOD:** «8 известных формул воспроизводятся после restore». **BAD:** «Детерминированный lookup означает, что обучения не было».

## P30 — Обученный коэффициент смеси вместо фиксированного

- **Гипотеза/источник:** tuned alpha E34V2 приносит доменную пользу при тех же признаках; [03, Г-02](03-hybrid-units-and-domain-representation.md), [05, Э-02](05-small-discriminating-experiments.md).
- **Домен/получатель/уровень:** RT; episode/intent ranker; R/S→C→L-memory→U.
- **A/B:** фиксированный alpha против выбранного только на train/dev при одинаковом candidate corpus и components. Negative — перемешанные labels; edge — равные scores и zero component.
- **Пилот/метрика:** 16 train/dev, 8 новых intents; Hit@1/8 ↑, required episode coverage/8 ↑; цена — tuning calls, ms/query, false matches. Старые разные evaluation subsets сначала пересчитываются R.
- **Опровержение:** эффект только от другой панели/feature set — не alpha effect.
- **Промпт:** «P30: заморозь components и экзамен, учи только alpha на dev; покажи ranking и downstream episode».
- **GOOD:** «Tuned alpha уменьшил число ненужных эпизодов при том же Hit@1». **BAD:** «Старые+7,69% — чистый эффект alpha без одинаковой панели».

## P31 — Доверие источнику: learned memory против fixed sum

- **Основание:** [10, Ф-01](10-historical-function-transfer-and-learning.md), [73, 2](73-rare-class-evidence-and-structural-feature-scope.md): E40V2 и production имеют разные механизмы; положительные исторические панели сохраняются.
- **Получатель:** RT/DC, TrustScorer; R→C→L-memory→U.
- **A/B/input:** те же7 features; kNN labels против fixed sum. Новые source families, все LOW/MEDIUM/HIGH; negative — shuffled labels; edge — near-threshold trust.
- **Пилот/метрика:** 18 train, 9 exam; macro-F1 по3классам ↑, false HIGH/LOW ↓; цена — examples, ms/decision, memory bytes.
- **Отказ:** иной feature set делает contrast неидентифицируемым; missing class нельзя приписывать архиву, где он присутствовал.
- **Промпт:** «P31: сохрани старые receipts и сравни два scorer на одинаковых новых patterns с per-class support».
- **GOOD/BAD:** «Память точнее поLOW при большей RAM» / «E40 былFAIL, значит учиться доверию бессмысленно».

## P32 — Доходит ли оценка полезности скила до выбора

- **Основание:** [10, Ф-02](10-historical-function-transfer-and-learning.md): score может не потребляться; historical category известна только evaluator.
- **Получатель:** RT, SkillRetriever→router; C→L-memory→U.
- **A/B/input:** один существующий score consumer off/on при frozen catalogue; category-input добавляется отдельным опытом. Запросы на repair/review/translation; negative — shuffled skill tags; edge — низкая similarity и AlwaysRequired.
- **Пилот/метрика:** 8 intents, 12 skill entries; required-skill recall/required ↑, loaded unnecessary/count ↓; цена — load bytes, ms, пропущенные обязательные проверки.
- **Отказ:** scorer не подключён — component claim only; oracle category не пользовательский вход.
- **Промпт:** «P32: покажи score→selected IDs→фактически загруженные скилы; меняй только подключение scorer».
- **GOOD/BAD:** «При тех же обязательных скилах загрузили меньше текста» / «Score рассчитан, значит routing обучился».

## P33 — Обученная доменная маршрутизация и её настоящий overload

- **Основание:** [20, ДР-01–08](20-domain-routing-positive-mechanism.md): labels, feature versions, unknown-distance, overload и cold loader различаются.
- **Получатель:** RT, DomainClassifierUnit/RouteClassifier/PromptDomainClassifier по отдельности; C→L-memory→U.
- **A/B/input:** один subcase: trained versus seed memory; независимые labels против self-label; explicit abstain distance; либо correct load. Не менять overload незаметно. Queries: code/security/translation и unrelated; edge — RU/EN и version mismatch.
- **Пилот/метрика:** 12 train, 8 known+4unknown; macro correct/12 ↑, unknown false-route/4 ↓; цена — labelled examples, ms/query, bytes.
- **Отказ:** string[] overload не читает `_train` — он не измеряет память int[] overload.
- **Промпт:** «P33: назови instance, overload и encoder version на Train и user Classify; сравни один канал».
- **GOOD/BAD:** «Этот overload использует learned labels» / «Класс обучен, значит любой Classify читает обучение».

## P34 — Положительный E22 и перенос функции меток

- **Основание:** [65–66](65-e22-positive-late-run-and-label-transfer.md): 0.4709→0.5646 сохранён; actual corpus/binary lineage неполна.
- **Получатель:** RT/CS, PredicateDatasetRouter; R→C→L-memory.
- **A/B/input:** AST-aware labels против string labels при одних predicates/files/split; independent law-gold фиксирован. Negative — filenames переименованы; edge — один source с несколькими dataset buckets.
- **Пилот/метрика:** 24 source groups, 16train/8exam; macro-F1 по buckets ↑, label disagreement/rows; цена — extraction ms, ручные минуты gold.
- **Отказ:** старые450rows не восстановлены — новый контроль механизма, не replay исторического run; ordinary caller неизвестен — U pending.
- **Промпт:** «P34: зафиксируй ordered corpus и обе label functions; сохранённый positive не стирай, новый опыт называй новым».
- **GOOD/BAD:** «Новые8cases проверяют перенос label contract» / «Повторили E22 без исходного corpus manifest».

## P35 — E22 при числе соседей меньше k

- **Основание:** N37 E22, раздел majority cliff; первичная независимая проверка остаётся открытой, [84](84-check-source-coverage.md).
- **Получатель:** RT, PredicateDatasetRouter; S→C→L-memory, U только при caller.
- **A/B/input:** denominator configured k против actual n при том же ranking; n=0,1,2,3 и k=3. Один identical query с положительным bucket; negative — отрицательные labels; edge — tie/even n.
- **Пилот/метрика:** 4 memory sizes×2 label panels; correct bucket decisions/all decisions ↑, false all/none; цена — число уроков до usable decision.
- **Отказ:** consumer всегда train360, n≥k — one-lesson cliff неприменим; это не опровержение положительного E22.
- **Промпт:** «P35: выведи votes,n,k,threshold и actual Predict; не выдавай формулу за реальный learning gain».
- **GOOD/BAD:** «При n1 исправлен порог, n3 parity сохранён» / «Этот edge объясняет RP01 free generation».

## P36 — E22 Train заменяет память или добавляет урок

- **Основание:** N37 E22 replacement, [65](65-e22-positive-late-run-and-label-transfer.md).
- **Получатель:** RT, actual Train caller; C→L-memory→U при существующем incremental caller.
- **A/B/input:** caller передаёт delta против full accepted corpus; Train implementation одинаков. Old A/B datasets, затем новый C; negative — один batch rebuild; edge — исправление старой метки.
- **Пилот/метрика:** 3old+1new lesson, 4queries; old correct/3 и new correct/1, retained IDs/count; цена — rebuild ms и memory.
- **Отказ:** прямой delta caller не найден — будущий contract risk; replacement законен при batch/rebuild.
- **Промпт:** «P36: сначала установи обещание caller; сравни его actual corpus после второго Train».
- **GOOD/BAD:** «Incremental wrapper потерял3oldrows» / «Любой Clear в Train является багом».

## P37 — E22 запрос без признаков

- **Основание:** N37 E22 empty-query/order; [84](84-check-source-coverage.md).
- **Получатель:** RT, PredicateDatasetRouter; S→C→U.
- **A/B/input:** одна память, пустой query; только порядок равнодалёких rows меняется. Отдельное intervention — abstain on no evidence. Negative — informative query; edge — empty query+empty training predicates.
- **Пилот/метрика:** 6rows, 3orders, 4queries; false confident buckets/empty requests ↓, informative accuracy/controls; цена — abstain rate и fallback latency.
- **Отказ:** upstream гарантирует непустой evidence или явный default по контракту; произвольный tie сам по себе не semantic error.
- **Промпт:** «P37: покажи что выбран при нулевой информации и что consumer делает с abstain».
- **GOOD/BAD:** «No-evidence правильно передан fallback» / «Первое семейство корпуса является найденным смыслом».

## P38 — E24: сегментация, trained router и метрика длины

- **Основание:** [46, 2–5](46-e24-rehabilitation-and-unused-trained-router.md), [26](26-routing-profiles-and-generated-flow-boundary.md).
- **Получатель:** RT/FL, PromptRoutingInputCompilerUnit→segment router; R/C→L-memory→U.
- **A/B/input:** learned router consumed/off при одном compiler; отдельно length proxy против exact relevant-span gold, без изменения outputs. Mixed code+translation request; negative — untrained memory; edge — один segment/empty relevant span.
- **Пилот/метрика:** 8 requests, 16annotated spans; correct domain/spans ↑, relevant-span precision/recall; цена — discarded needed chars, ms/route.
- **Отказ:** current segment router игнорирует trained object; A50 сменил контракт, старый FAIL не отменяется задним числом.
- **Промпт:** «P38: trace Train instance→actual route instance и оцени содержание spans, не только длину».
- **GOOD/BAD:** «Сегменты короче, но потеряли обязательную часть» / «Уменьшение текста = правильная маршрутизация».

## P39 — E51: память, общие координаты и oracle outputs

- **Основание:** [06, H-02/03](06-helper-findings-reconciliation.md), [05, Э-05/06/07](05-small-discriminating-experiments.md).
- **Получатель:** CS/RT/FL; E51A RunTrace, E51B Compile/Nearest/RunHeldOutTrace отдельно; C→L-memory→U.
- **A/B/input:** раздельно memory/empty+label-shuffle; shared vocabulary/per-prompt; actual output supplier/oracle. Новые source contexts; negative — CH-ID permutation; edge — unseen vocabulary.
- **Пилот/метрика:** 8 train, 8 exam; predicates F1, route correctness/8, actual field correctness/fields ↑; цена — memory bytes и extraction ms.
- **Отказ:** нет actual supplier — MISSING_CONSUMER; copied oracle проверяет wiring, не learned answer.
- **Промпт:** «P39: выбери один subcase, подпиши кто создал каждый field и исключи label/ID подсказку».
- **GOOD/BAD:** «Memory улучшила predicates на новых contexts» / «Oracle reply доказывает генеративное обучение».

## P40 — Зависимости E35 и адресат feedback E36

- **Основание:** [14, coupled RNG](14-e35-e36-coupled-rng-false-knn-validation.md), [18](18-feedback-routing-and-historical-split.md), [10, Ф-03](10-historical-function-transfer-and-learning.md).
- **Получатель:** FL/RT; dependency rule и feedback kNN раздельно; R/C→L-memory для E36.
- **A/B/input:** independent split seed при fixed cases сначала R/C; затем learned feedback mapping/empty memory либо один dependency feature. Negative — нет evidence зависимости; edge — все5feedback classes и редкий sequential case.
- **Пилот/метрика:** по2exam на каждый класс, 10cases; macro-F1 ↑, false parallel/sequential ↓; цена — feature ms, новых labels/count.
- **Отказ:** class отсутствует — cannot assess; E35 rule без learner не называть kNN training.
- **Промпт:** «P40: покажи train/val class table до сравнения; dependency и feedback имеют отдельных получателей».
- **GOOD/BAD:** «E36 полезен для2классов,3не проверены» / «Десять PARALLELcases доказали все зависимости».

## P41 — Что значит правильно разделить и вовремя остановиться

- **Основание:** [50, E21](50-historical-e05-e20-e21-source-lineage.md), [34, E32](34-decomposition-quality-and-execution-verdicts.md), [29, E33](29-e33-failed-status-versus-useful-rule.md).
- **Получатель:** FL, конкретный Decomposer/StopDetector; R/C→L-memory если tuning/память.
- **A/B/input:** один механизм за раз: learned split threshold/fixed; kNN subtask count/median; stop rule/kNN. Gold содержит boundary spans+dependencies, не один count. Negative — атомарная задача; edge — nested conjunction, review continuation.
- **Пилот/метрика:** 12 requests:4atomic/4sequential/4independent; boundary F1 и lawful completion/12 ↑, false stop/12 ↓; цена — number of unnecessary steps, ms.
- **Отказ:** baseline100%=enhanced100% — no incremental gain, не бесполезность baseline.
- **Промпт:** «P41: сравни один splitter/stop rule и проверь реальные границы и обязательное продолжение».
- **GOOD/BAD:** «Число верно, две границы неверны» / «Правильное число шагов означает правильный план».

## P42 — Checker, arbiter и событие обучения

- **Основание:** [34, E39/checker](34-decomposition-quality-and-execution-verdicts.md), [40, E52/veto/queue](40-e52-contract-success-and-answer-independent-veto.md).
- **Получатель:** FL, QualityOrchestrator/InternalFirstAiSupport/LearningEventScheduler отдельно; R/C/U.
- **A/B/input:** один subcase: checker heuristic против frozen semantic gold; request-only veto против answer-sensitive rule; queue event против actual trainer receipt как наблюдаемость. Запрос о запрете duplicate callback, ответы соблюдает/нарушает; negative — quoted bad requirement; edge — пустой answer.
- **Пилот/метрика:** 4requests×2answers; false accepts/rejects/8 ↓, required checks recall; цена — judge ms и human minutes.
- **Отказ:** нет optimizer callback — DidRun относится очереди; semantic check не подменять наличием `return`.
- **Промпт:** «P42: покажи независимость или зависимость verdict от ответа и точный смысл события; не обучай ради проверки очереди».
- **GOOD/BAD:** «Вето одинаково для хорошего и плохого ответа» / «E52PASS означает Student научился».

## P43 — POS/словарная память и контекст слова

- **Основание:** [17, К-01–03](17-positive-pos-knn-versus-production-rules.md), [23, 1–2](23-language-context-and-learned-rule-scope.md).
- **Получатель:** TR/POS, E25/E26 versus production lookup; C→L-memory→U.
- **A/B/input:** одна fixed POS/context representation, learned neighbours против текущего словаря/правила. `book` noun/verb, один surface в разных contexts; negative — shuffled tags; edge — unseen word и language alias.
- **Пилот/метрика:** 12train, 12context-disjoint exam; macro POS/translation accuracy ↑; цена — labels required, bytes, ms/token.
- **Отказ:** E26 contract не совпадает с production key — сначала matched input, не перенос исторического процента.
- **Промпт:** «P43: покажи context, который видят и learner, и user-query; оцени word decision в предложении».
- **GOOD/BAD:** «Контекст изменил перевод неоднозначного слова» / «Одноимённый класс воспроизводит старый E26».

## P44 — Проекция выбора шаблона E23

- **Основание:** [27, измерение и production difference](27-e23-template-projection-and-production-difference.md).
- **Получатель:** CS/FL, template selector/Beautifier; R/C→L-memory→U.
- **A/B/input:** same template library, только learned projection versus fixed mapping; negative — shuffled template labels; edge — unseen template family, равные distances.
- **Пилот/метрика:** 16train, 8heldout families; correct template/8 ↑, valid instantiated output/8 ↑; цена — memory, milliseconds и unsupported-template rate.
- **Отказ:** choosing from fixed library не free code generation; semantic template correctness отделить от formatting.
- **Промпт:** «P44: заморозь библиотеку и покажи ID выбранного шаблона плюс конечную подстановку».
- **GOOD/BAD:** «Точнее выбрали известный template» / «100% selection доказывает произвольное программирование».

## P45 — Перестановка структуры предложения E27

- **Основание:** [33, подтверждение и counterexample](33-e27-structure-transfer-and-wordmap-counterexample.md).
- **Получатель:** TR, ApplyE27Reorder в фактически достигнутой ветке; C→L-memory→U.
- **A/B/input:** один reorder function on/off при frozen word/phrase maps и parser. Полностью и частично покрытое предложение; negative — порядок уже правильный; edge — `null check` против общего subject/object порядка.
- **Пилот/метрика:** 8sentences/направление; role/order correct/8 ↑; цена — испорченные ранее верные переводы и ms/answer.
- **Отказ:** narrow technical reorder решает свою задачу, не универсальный syntax transfer; точный branch trace обязателен.
- **Промпт:** «P45: покажи до/после reorder и выбранный путь wordmap/phrase/kNN».
- **GOOD/BAD:** «Правило помогает null-check, иной word order не покрывает» / «Старый английский порядок универсален для RU/SQL».

## P46 — E29: чему соответствует ценность DPO-пары

- **Основание:** [24, постановка и соседство](24-e29-pair-value-and-probe-objective.md).
- **Получатель:** CS/RT, historical Probe objective отдельно от RP01 DPO; R/C→L-neural.
- **A/B/input:** те же заранее построенные переходы; только pair-selection/value policy. Не создавать targets после shuffle. Negative — случайные weights; edge — повторённый current с разными next.
- **Пилот/метрика:** 16pairs, 8fixed exam, 4updates; correct next/8 ↑, gain/example, seconds-to-quality; цена — selection compute и coverage.
- **Отказ:** опыт обучает next-predicate вместо chosen/rejected preference — нельзя обобщать на все DPO-пары.
- **Промпт:** «P46: сначала выпиши точный loss/targets; сравни ценность отбора на неизменных готовых парах».
- **GOOD/BAD:** «Этот отбор полезен Probe переходам» / «Название DPO pair value доказывает DPO learning».

## P47 — E30: условие перехода потерялось в адресе памяти

- **Основание:** [32, ошибка учебного объекта](32-e30-lost-transition-context-and-historical-comparison.md).
- **Получатель:** CS, PredicateCache; C→L-memory, U только при caller.
- **A/B/input:** FileVector против FileVector+Current; same pairs, threshold/tie/fallback. Один файл с двумя разными переходами; negative — один transition/file; edge — конфликтующие records и order как отдельный contrast.
- **Пилот/метрика:** 4files, 12transitions, 8query transitions; correct next/8 ↑, quality among hits/counted hits, abstain/8; цена — key bytes и ms.
- **Отказ:** новый V2 решает другую задачу либо consumer не найден — local cache result.
- **Промпт:** «P47: добавь только Current в ключ и покажи collision разрешился ли на обычном cache lookup».
- **GOOD/BAD:** «Один file-vector больше не смешивает две позиции» / «Поменяли targets и приписали успех cache key».

## P48 — E44/E45: вес и curriculum без смены targets

- **Основание:** [57](57-e44-e45-target-confound-and-future-controls.md), [37, 4](37-new-helper-findings-data-history-and-language.md).
- **Получатель:** CS/RT, historical Probe, затем свой recipient; R→L-neural.
- **A/B/input:** сначала materialize fixed `(x,y)`; только relative weight×10 либо order, никогда оба. Same heldout и seed-to-example assignment. Negative — no change; edge — rare domain с2examples.
- **Пилот/метрика:** 24fixed pairs, 8old+8target exam, 4updates; per-domain correct/8 ↑, retention delta в процентных пунктах; цена — exposures и ms.
- **Отказ:** targets различаются — confounded; singleton support не заменять general accuracy.
- **Промпт:** «P48: приложи equality of pair IDs/targets до сравнения weights или curriculum».
- **GOOD/BAD:** «При fixed targets вес помог rare law ценой general accuracy» / «Исторический FAIL доказывает вред именно×10».

## P49 — E48: ранняя экономия примеров и порядок backward

- **Основание:** [54, intermediate gain и Probe gradient](54-e48-early-learning-and-probe-gradient-boundary.md).
- **Получатель:** CS/RT, ProbeNeuralNetwork; R/C→L-neural.
- **A/B/input:** warm versus cold при matched target-exposures; numerical backward current in-place/old-weight — отдельный C прежде causal learning. Negative — unrelated warm corpus; edge — performance curves пересекаются.
- **Пилот/метрика:** budgets4/8/16 примеров, 8sealed cases; quality versus examples, examples-to-threshold ↓; цена — warm compute + RAM. C:2inputs,1step.
- **Отказ:** итоговая последняя точка FAIL не стирает ранний gain; warmSrcNet, который не вызывается, не treatment.
- **Промпт:** «P49: покажи learning curve и использованный warm state; производную Probe не копируй в RP01 без проверки».
- **GOOD/BAD:** «Нужный уровень достигается8вместо16примеров» / «Последний FAIL означает отсутствие любого обучения».

## P50 — Графовые эвристики E49/E50 и устойчивость

- **Основание:** [37, 5](37-new-helper-findings-data-history-and-language.md).
- **Получатель:** FL, dependency/expansion controller; R/C/U, L только если есть actual learner.
- **A/B/input:** один fixed Jaccard dependency rule off/on; robustness отдельно на meaning-preserving perturbations, не arbitrary changed tasks. Negative — независимые цели; edge — общие слова без зависимости.
- **Пилот/метрика:** 8graphs+8equivalent paraphrases; lawful dependency precision/recall ↑, equivalent-output agreement/8 ↑; цена — лишние edges, critical missing edges, ms.
- **Отказ:** эвристика полезна, но неизменяемая — не называть её обученной; changed-law output обязан иногда меняться.
- **Промпт:** «P50: сравни один rule и раздельно инвариантные/изменяющие смысл perturbations».
- **GOOD/BAD:** «Граф изменился на новый смысл и сохранился на парафраз» / «Любая неизменность означает robust intelligence».

## P51 — Ключ перевода: тема, язык, регистр и операторы

- **Основание:** [13, 1–2](13-domain-context-and-dpo-subunits.md), [19, СТ-02](19-unit-state-identity-and-partial-learning.md), [43, 1–2](43-unicode-language-gates-and-proxy-fidelity.md), [53, 4–5](53-translation-composition-and-direction-consumers.md).
- **Получатель:** TR, exact/phrase/word/KNN owners; C→L-memory→U.
- **A/B/input:** один атрибут ключа: TopicDomain, direction, case, Unicode normalization или operator. `x==y/x!=y`, `UserID/userId`, один source в двух направлениях; negative — безвредный whitespace; edge — Ñ/NFD и смешанный identifier.
- **Пилот/метрика:** 4contrasts+4same-meaning controls на атрибут; correct output/12 ↑, key collisions/contrasts ↓; цена — memory size, missed exact hits, µs/key.
- **Отказ:** normalization законна для этого языка; один общий store не автоматически смешивает конечные ответы — trace filters.
- **Промпт:** «P51: выбери один смысловой атрибут и покажи training/query keys и ветку финального перевода».
- **GOOD/BAD:** «Context убрал конкретную неоднозначность» / «Любое совпадение normalized keys вредно».

## P52 — Что создаётся из правильной переводческой пары

- **Основание:** [16, П-04/05](16-translation-lesson-admission-and-meaning-loss.md), [44, 1–4](44-structured-lessons-roles-keys-and-word-alignment.md), [13, 3](13-domain-context-and-dpo-subunits.md).
- **Получатель:** TR, StructuredBatchBuilder→tokenizer/executor/verifier; C→L-memory→U.
- **A/B/input:** отдельно role parser, RuleTags из rejected, trained/base tokenizer mismatch, first-word alignment, GrammaticalRole mapping. Пример `I am ready`→«Я готов»; negative — wrong-side annotation; edge — word order отличается, PREP/identifier first.
- **Пилот/метрика:** 6pairs×созданные Unit rows; correct field mappings/rows ↑, wrong learned links/count ↓; 8ordinary queries; цена — ручные corrections и bytes/lesson.
- **Отказ:** chosen правильный, derived word link неправильный — quality pair не спасает converter; не менять весь builder сразу.
- **Промпт:** «P52: выбери одну конверсию, покажи source pair→каждое поле lesson→прочитавший Unit».
- **GOOD/BAD:** «Первая пара слов неверна при правильной полной фразе» / «Три Unit rows означают три освоенных навыка».

## P53 — Atomic batch, неизвестный Unit и readiness после no-op

- **Основание:** [11, 3](11-new-helper-source-reconciliation.md), [12, N23.02](12-translation-unit-case-identity-and-batch-atomicity.md), [36, 3](36-helper27-state-replay-and-quality-review.md).
- **Получатель:** TR, TrainStructuredBatchAsync; C; useful lesson uptake — L-memory отдельно.
- **A/B/input:** valid first row+invalid second; prevalidation/late failure. Отдельно all-heldout/unknown-Unit batch against existing trained machine. Negative — полностью valid batch; edge — repeated ID и no trainable rows.
- **Пилот/метрика:** 4batch types, state hashes before/after; unexpected partial changes/failed batches ↓, actual applied/declared rows, readiness consistency; цена — staging bytes и validation ms.
- **Отказ:** partial commit является явным контрактом — оценивать documented semantics; receipt success без calls не learning.
- **Промпт:** «P53: покажи Units изменённые до исключения и readiness после no-op; не запускай neural trainer».
- **GOOD/BAD:** «Batch отказал, но первый словарь изменён» / «Исключение автоматически вернуло весь state».

## P54 — Сборка фразы и потерянные позиции

- **Основание:** [16, П-02/03](16-translation-lesson-admission-and-meaning-loss.md), [53, 1–3 и N33](53-translation-composition-and-direction-consumers.md).
- **Получатель:** TR, phrase/word composer→adapter; C→U.
- **A/B/input:** один subcase: greedy leftmost segmentation/global valid segmentation; filtered ordinal/source ordinal; preserve unknown required token. `close user session`, punctuated tokens и `delete -> archive`; negative — exact-source path; edge — многословные overlapping mappings.
- **Пилот/метрика:** 8sentences, complete required meaning/8 ↑, omitted critical tokens/count ↓; цена — composition ms и candidate count.
- **Отказ:** exact map скрывает composer — нужны trace и nonexact query; перестановка уроков не меняет leftmost algorithm сама по себе.
- **Промпт:** «P54: выведи исходные/filtered positions, выбранные spans и финальный текст для одного composer factor».
- **GOOD/BAD:** «После punctuation взяли неверный source token» / «Полный exact перевод доказывает правильность phrase composer».

## P55 — Отрицание, лицо и узкая защитная SourceLaw

- **Основание:** [37, 7](37-new-helper-findings-data-history-and-language.md), [53, 2](53-translation-composition-and-direction-consumers.md), [72, 1](72-negation-confidence-and-e46-feedback-consumers.md).
- **Получатель:** EN→RU прежде всего, другие направления отдельно; source tokenizer/wordmap/KNN→Flow; C→L-memory→U.
- **A/B/input:** retain semantic negation против current normalization при same state. `can/can't`, `delete/never delete`, `I/you`; negative — `the box is not empty` для известной SourceLaw; edge — unknown contraction/двойное отрицание.
- **Пилот/метрика:** 6polarity/role contrasts+4noncritical controls; meaning correct/16 ↑, polarity violations/12 ↓; цена — false abstain и ms.
- **Отказ:** исправление языка меняет gold; imperative нельзя оценивать как утверждение о завершённом действии.
- **Промпт:** «P55: проследи пропущенный токен до обычного Flow и проверь narrow-law positive control».
- **GOOD/BAD:** «Never потерялся вне защищённого copular шаблона» / «Одна защита not покрывает любое отрицание».

## P56 — Verifier, контекст правил и уверенность0.99

- **Основание:** [13, 4–5](13-domain-context-and-dpo-subunits.md), [23, 3](23-language-context-and-learned-rule-scope.md), [36, 4](36-helper27-state-replay-and-quality-review.md), [72, 1–2/4](72-negation-confidence-and-e46-feedback-consumers.md).
- **Получатель:** TR, QualityVerifier→adapter→Flow; C→L-memory→U.
- **A/B/input:** отдельно context-scoped rules, conflicting accumulated requirements, empty-verifier confidence uplift, Pass/threshold acceptance. Good/bad candidate одной фразы; negative — verifier без правил; edge — one requirement другого domain.
- **Пилот/метрика:** 4sources×2candidates; false accept/reject/8 ↓, confidence calibration bins с фактическими counts; цена — abstain/8, ms/check. E46 five-vector classifier — отдельный recipient, не тот же scalar.
- **Отказ:** не найден current caller E46 — не выдавать его за удалённый translation gate.
- **Промпт:** «P56: выбери одно правило принятия, сохрани raw candidate, Pass/Issues и конечную confidence раздельно».
- **GOOD/BAD:** «Пустые issues подняли score без доказательства смысла» / «0.99 означает99% семантической правильности».

## P57 — Метрика перевода и реальное качество

- **Основание:** [22](22-tatoeba-primary-review-and-domain-scope.md), [43, 3–4](43-unicode-language-gates-and-proxy-fidelity.md), [58, 1–2](58-unit-metrics-and-generation-consumer-boundaries.md), [36, 4–5](36-helper27-state-replay-and-quality-review.md).
- **Получатель:** TR, Tatoeba/ProductHeldOut/Unit scoring отдельно; R→C, без обучения для сравнения scoring.
- **A/B/input:** те же saved outputs, metric versus frozen semantic rubric. Отдельно macro Unit-average/micro-row average, old stored AccuracyBefore/current-panel baseline, coverage/quality. `Hello` против `Hello 🙂`, wrong number, negation, script+identifier; edge — noheldout.
- **Пилот/метрика:** 12saved output pairs; metric false accept/reject/12 ↓, confusion table, per-unit denominators; цена — grading minutes. Native UTF-16 ChrF1 отдельно от Python Unicode proxy.
- **Отказ:** меняется панель — это новый evaluation, не model improvement.
- **Промпт:** «P57: пересуди те же тексты и покажи каждую несовместимость proxy с предметным законом».
- **GOOD/BAD:** «Coverage100% при2/12смысловых ответов» / «AccuracyAfter выросло — перевод улучшился без той же панели».

## P58 — Feedback стал уроком или только статусом

- **Основание:** [75, 1–4](75-translation-feedback-learning-consumer.md), [16, П-06](16-translation-lesson-admission-and-meaning-loss.md), [11](11-new-helper-source-reconciliation.md).
- **Получатель:** TR, Tatoeba loop/structured learning bridge, actual launcher отдельно; C→L-memory→U.
- **A/B/input:** один factor: сохранять actual erroneous answer как материал corrective lesson versus corpus-built rejected; либо promotion before/after successful application как accounting. Negative — stop before Train; edge — old flat +0.01 без Units.
- **Пилот/метрика:** 4scored errors+4controls, 8exam; applied lessons/promoted ↑, corrected laws/4 ↑; цена — teacher/judge calls, examples and seconds.
- **Отказ:** launcher отсутствует — current source pathway only; corpus enrichment полезно, но не DPO на собственной ошибке.
- **Промпт:** «P58: trace ActualText→pair→Unit call→state→answer; покажи где feedback исчезает».
- **GOOD/BAD:** «Promoted4, фактически применено3» / «Флаг promotion доказывает обучение всех4».

## P59 — Новый урок после restore виден в обычном переводе

- **Основание:** [78, новый урок и границы probe](78-translation-restored-memory-positive-receipt.md).
- **Получатель:** TR, restored TranslationMachine и actual adapter; R→L-memory→U.
- **A/B/input:** один initial checkpoint; add new safe-002 lesson versus no add. Queries к новому уроку exact и nonexact, opposite action/object; negative — старый известный запрос; edge — правильная direction label.
- **Пилот/метрика:** 1new lesson, 2new+2contrast+4old queries; new correct/4 ↑, old correct/4, source selected ID; цена — training seconds/lesson и persisted bytes.
- **Отказ:** ответ выбран из старой памяти или source помечен неверным языком — не доказан новый переводческий навык.
- **Промпт:** «P59: сохрани исторический positive replay, затем проверь именно новый урок после второго холодного открытия».
- **GOOD/BAD:** «Старые4сохранены, новый nonexact стал верным» / «Проверка старого запроса доказывает выученность нового».

## P60 — Full/prefix alias съедает top-k

- **Основание:** [60, условия и малое различение](60-knn-signature-alias-and-candidate-budget.md).
- **Получатель:** TR, TranslationKnnStore→Executor; C→L-memory→U.
- **A/B/input:** duplicate insertion identical full/prefix key versus dedup, те же4lessons/k/order. Negative — разные keys; edge — повтор одного concept3раза, считать occurrences, не Distinct.
- **Пилот/метрика:** 4lessons, 6queries; unique lessons/top-k ↑, correct answer/6 ↑; цена — dedup ms/bytes, coverage.
- **Отказ:** labels/key aliases не возникают с real tokenizer — synthetic contract only.
- **Промпт:** «P60: выведи реальные full/prefix keys и unique lesson IDs до downstream filter».
- **GOOD/BAD:** «Один урок занял2из5мест» / «Две разные концепции всегда означают alias».

## P61 — UNKNOWN-хвост и true/false не входят в ключ

- **Основание:** N37 unknown-tail/BOOL, [84](84-check-source-coverage.md); plan-source-read, не новая primary verification.
- **Получатель:** TR technical text, TranslationSemanticTokenizer→KNN; C→L-memory→U.
- **A/B/input:** только boolean/required-tail-sensitive key versus current. `null customerId42 true/false`; unknown «without consent». Negative — безвредная добавка; edge — actual training signature с PAIR/RuleTags и exact-map shortcut.
- **Пилот/метрика:** 4contrasts+4controls; collision/contrasts ↓, wrong-law acceptance/8 ↓, correct/8 ↑; цена — extra key bytes, abstain.
- **Отказ:** actual keys не равны или user path не достигает KNN — механизм не достигается в этой постановке.
- **Промпт:** «P61: сначала native Tokenize обеих сторон и training/query keys; только затем один memory intervention».
- **GOOD/BAD:** «Совпали реальные ключи противоположных boolean laws» / «В proxy придумали отсутствующие TRUE_LITERAL/FALSE_LITERAL».

## P62 — Дополнительный тег ослабляет source guard

- **Основание:** N37 signature monotonicity, [84](84-check-source-coverage.md).
- **Получатель:** TR, FindNearestCascade; S→C→U.
- **A/B/input:** current tag-dependent threshold против fixed source guard при same keys/corpus; механический probe POS-only key затем тот же+rule tag. `blue widget/green vehicle`; negative — tag только на query; edge — actual RU tags с UNKNOWN concepts.
- **Пилот/метрика:** 4negative+4valid queries; accepted wrong/8 ↓, accepted valid/valid; цена — abstain и µs/guard.
- **Отказ:** native tokenizer не создаёт equal keys/route не достигнут — synthetic predicate only.
- **Промпт:** «P62: докажи threshold0.35→0 на реальных inputs и отдельно проверь пользу постоянного guard».
- **GOOD/BAD:** «Добавление тега изменило gate при тех же source texts» / «Более длинная signature всегда информативнее».

## P63 — Одностороннее containment и чужие оговорки

- **Основание:** N37 containment, [53](53-translation-composition-and-direction-consumers.md).
- **Получатель:** TR, SourceSimilarity→Executor; S→C→U.
- **A/B/input:** containment versus symmetric matching с frozen dev threshold, остальные candidate laws прежние. `delete records` и `delete records after backup is verified by administrator`; negative — harmless paraphrase; edge — reverse query/source.
- **Пилот/метрика:** 6condition-changing+6equivalent pairs; false acceptance/6 ↓, correct coverage/6 ↑; цена — abstain и ms/query.
- **Отказ:** Jaccard меньше сам по себе не semantic improvement; при обоих scores выше порога gate не меняется.
- **Промпт:** «P63: запиши actual token sets, обе направленные оценки и обязательные условия gold».
- **GOOD/BAD:** «Чужая оговорка перестала подменять запрос ценой2отказов» / «Симметрия автоматически даёт правильный смысл».

## P64 — Каскад прекращается до проверки допустимости target

- **Основание:** N37 postfilter cascade, [84](84-check-source-coverage.md).
- **Получатель:** TR, full/prefix/structure cascade→TrySelectKnnCandidate; C→U.
- **A/B/input:** current stop-on-nonempty versus continue-only-if-all-targets-invalid, все фильтры неизменны. Full has2wrong-language targets, prefix has1valid; negative — full has1valid; edge — всеtiers invalid.
- **Пилот/метрика:** 6frozen bucket states; valid final answers/6 ↑, forbidden accept/count; цена — tiers visited/query и ms.
- **Отказ:** нельзя обходить SourceLaw/verifier; переход ради низкого score пригодного кандидата — другая гипотеза.
- **Промпт:** «P64: покажи rejected candidates и причину, затем посещён ли следующий tier».
- **GOOD/BAD:** «Fallback после2недопустимых дал допустимый ответ» / «Берём первый lower-tier независимо от закона».

## P65 — RU→EN history отбрасывает новый смысл как дубликат

- **Основание:** N37 RUEN history, H37-A; [16, П-01](16-translation-lesson-admission-and-meaning-loss.md).
- **Получатель:** RU→EN Batch029/structured builder; S→C→L-memory→U.
- **A/B/input:** lexical novelty/current versus semantic-delta-safe admission при same history; «файл доступен/не доступен», порядок проверки/публикации, три/четыре. Negative — пунктуационный duplicate; edge — Jaccard ровно0.70.
- **Пилот/метрика:** 3meaning contrasts+3duplicates+2new-topic; accepted useful/3 ↑, accepted duplicates/3 ↓; затем4heldout laws и corrected/4; цена — training rows, judge minutes.
- **Отказ:** больше accepted rows без user gain — admission effect only; не снимать leakage protection.
- **Промпт:** «P65: фиксируй history, покажи rejected lesson IDs и проверь только допуск смысловой поправки».
- **GOOD/BAD:** «Отрицание дошло доUnit и нового ответа» / «Все похожие строки разрешены как новые уроки».

## P66 — Экзамен должен видеть минимальный смысловой контраст

- **Основание:** N37 RUEN H37-B; разные train `>=.70` и heldout `>.70` границы, [84](84-check-source-coverage.md).
- **Получатель:** TR, Batch029 validation/evaluator; R/C, без обучения для диагностики панели.
- **A/B/input:** lexical-overlap exclusion против source-family-independent exam protocol, outputs/state одинаковы. Opposite polarity с общими словами; negative — exact train duplicate; edge — score0.70.
- **Пилот/метрика:** 8exam candidates; eligible independent contrasts/8 ↑, true leakage accepted/count ↓; цена — gold/source audit minutes. Scores разных панелей не model delta.
- **Отказ:** source independence не доказана — не выпускать строку из sealed роли.
- **Промпт:** «P66: отдели смысловой контраст от provenance leakage, не удаляй guard ради красивого score».
- **GOOD/BAD:** «Контраст независим поsource, сохранён в отдельной диагностике» / «Знакомый train answer переименован в heldout».

## P67 — Supplemental и исправление: последний автор записи

- **Основание:** N37 RUEN H37-C; [45, 4](45-memory-reachability-and-context-consumption.md) — отдельный аналог PairId first-writer в WK.
- **Получатель:** TR exact source memory; WK PairId store проверяется отдельно; C→L-memory→U.
- **A/B/input:** same lesson set, только order valid correction/obsolete row; alternative conflict rule отдельным опытом. Negative — identical content duplicate; edge — corpus versus supplemental versus within-batch.
- **Пилот/метрика:** 3conflicting keys×2orders, 6queries; current correct/6 ↑, obsolete output/6 ↓; цена — version metadata bytes и manual conflict decisions.
- **Отказ:** last writer корректно применил ремонт — не баг; first-writer в другом store не переносить автоматически.
- **Промпт:** «P67: перечисли все producer paths и выбранный lesson version; сравни только порядок».
- **GOOD/BAD:** «Поздняя правильная поправка стала видна» / «Любая перезапись означает потерю знаний».

## P68 — Pretraining: overlap, EOD, хвост и старые shards

- **Основание:** [69, таблица/consumer/shards](69-pretraining-data-boundaries-and-consumer-scope.md).
- **Получатель:** PT, реальный production DataLoader/Trainer; C→L-neural→U для созданной базы.
- **A/B/input:** один factor: stride; EOD code; tail inclusion; либо immutable shard manifest вместо enumerate-directory. Negative — corpus короче окна; edge — второй build имеет меньше shards.
- **Пилот/метрика:** 3tiny documents, lengths around block boundary; repeated targets/targets, cross-document transitions/count; затем16updates,8fixed exam. Цена — unique tokens/update, seconds/epoch, disk bytes. Равные updates и равные unique token exposures — два анализа.
- **Отказ:** RP01 direct DPO этот loader не вызывает — другой recipient; низкий loss на overlap не generalization.
- **Промпт:** «P68: покажи точный sample stream перед обучением и меняй один параметр данных».
- **GOOD/BAD:** «Stride сэкономил compute при равном качестве» / «Повторённые окна считаются независимыми примерами».

## P69 — GraphRAG: текст embedding, seeds и реально переданный context

- **Основание:** [45, 2](45-memory-reachability-and-context-consumption.md), [71](71-graphrag-prefix-fallback-and-seed-boundaries.md).
- **Получатель:** GR/WK при настоящем caller; Indexer/HybridRetriever/PageRank отдельно от WikiContextRetriever; C→U.
- **A/B/input:** отдельно RawText versus content-preserving embedding input; valid-seed filtering; context truncation. 4nodes, same prefix64 but different tail; negative — no/absent seeds; edge — valid seed плюс zero-rank fillers, empty RawText→Signature.
- **Пилот/метрика:** 8queries; relevant retrieved@k, relevant chars delivered/available ↑, unsupported evidence/answers ↓; цена — context tokens, ms и RAM.
- **Отказ:** нет actual ContextText consumer — component claim; Results не равно delivered context.
- **Промпт:** «P69: trace source fields→embedding→ranked nodes→actual context bytes→answer».
- **GOOD/BAD:** «Результат найден, но его текст не передан» / «Непустой PageRank при пустых seeds доказывает релевантность».

## P70 — WikiTrace: исходная трасса и шаблон типа ошибки

- **Основание:** [76, 1–4](76-wiki-trace-donor-input-loss-and-checkpoint-identity.md), [28/39](39-code-trace-remediation-lineage.md).
- **Получатель:** WK, converter→neural trainer отдельно от deterministic aggregator/builder; R/C→L-neural→U.
- **A/B/input:** exact trace/chosen/rejected versus error-type template при одной task panel и tokenizer. 4traces одного error class с разными owners/order; negative — shuffled owner; edge —40/50 exact train overlap в архиве, metadata manifest не binary identity.
- **Пилот/метрика:** 8train,8source-disjoint exam,4updates; trace-conditioned correct/8 ↑, distinguishing material fingerprints/count; цена — input tokens, compute.
- **Отказ:** downstream получает только fixed aggregator — neural contribution не установлена; archive scores не пересчитать как новые generations.
- **Промпт:** «P70: покажи исчезнувшие поля доtrainer и назови supplier предметного результата отдельно отcheckpoint».
- **GOOD/BAD:** «Template потерял owner distinction» / «ManifestLoRA означает, что этот binary содержитLoRA».

## P71 — E17: perplexity на разных алфавитах

- **Основание:** N37 E17 H37-B1/B2/B3, [84](84-check-source-coverage.md).
- **Получатель:** CS, Level2/Level3 representation→typed predictor; R/S→C→L-memory.
- **A/B/input:** same file-disjoint source tasks, только representation; fixed lawful next-kind evaluator. Negative — all symbols CONST; edge — Level3 falls back toLevel2, parser finds0predicates.
- **Пилот/метрика:** 8train files,8exam tasks; correct typed next/8 ↑, parser coverage/count; цена — token count, vector bytes, query ms. PPL сообщается внутри собственного event space, не вычитается как semantic gain.
- **Отказ:** low PPL+zero discrimination опровергает достаточность метрики, не полезность всех predicates.
- **Промпт:** «P71: закрепи общий смысловой экзамен и логируй representation/fallback каждого примера».
- **GOOD/BAD:** «Level3 лучше next-kind при той же панели» / «190→5PPL доказывает97%лучше программирование».

## P72 — E28: project conditioning без test leakage и ложных edges

- **Основание:** N37 E28, [84](84-check-source-coverage.md); свежая primary verification не заявлена.
- **Получатель:** CS, project-adaptor kNN; R/C→L-memory.
- **A/B/input:** train-only global против train-only project gate; global fallback — отдельный factor. Fake cross-file edges — отдельный extraction contrast. Negative — leaked allFiles arm только диагностический; edge — unseen project.
- **Пилот/метрика:** 3projects×4files, 8sealed source-group cases; lawful next/8 ↑, cross-project contamination/queries ↓; цена — abstain, ms, per-project bytes.
- **Отказ:** сравнение leaked arm сclean не causal project benefit; SQL/TS требуют своих extractors, не Roslyn перенос.
- **Промпт:** «P72: докажи train-only membership и within-file pair identity, затем меняй один domain gate».
- **GOOD/BAD:** «Project gate помогбездоступаexam» / «Generic baseline уже виделtest, поэтому project learning плох».

## P73 — E31: домен в предикторе или только в порядке

- **Основание:** N37 E31 H37-C1/C2; [84](84-check-source-coverage.md).
- **Получатель:** CS/RT, Probe/typed memory, own consumer; S→C→L-neural/L-memory.
- **A/B/input:** f(x) против f(x,domain) на одинаковых ordered `(x,y)`; group-order effect отдельно. Один x имеет разные lawful y в двух domains; negative — shuffled train domain; edge — unseen domain и неверный basename label.
- **Пилот/метрика:** 16pairs,8conflicted+4shared exam; macro correct/12 ↑, false route/count; цена — bytes/domain, examples, time.
- **Отказ:** typed codec отсутствует — нет lawful cross-domain comparison; не включать role вместе сdomain без отдельной ablation.
- **Промпт:** «P73: покажи domain реально в learner input и separate outcomes конфликтных/shared cases».
- **GOOD/BAD:** «Domain-only conditioning различает2закона» / «Сортировка поdomain доказала бесполезностьусловнойпамяти».

## P74 — E42: сохранённые признаки и доступные законы

- **Основание:** N37 E42 H37-D1/D2, [84](84-check-source-coverage.md).
- **Получатель:** MM/GR/FL только owner, чью память реально вытесняет policy; S/C→L-memory→U.
- **A/B/input:** FIFO/current против value-based при same capacity10/order/lessons; law-aware policy отдельно. Negative — targets заменены wrong при неизменных marker sets; edge — rare corrected law и obsolete version.
- **Пилот/метрика:** 20lessons,8old+4new exam; correct retained law/12 ↑, rare-law recall; цена — bytes, eviction/query ms, domain coverage. Predicate coverage secondary.
- **Отказ:** owner не связан сeviction — нельзя приписать ему потерю RP01 checkpoint; coverage1 не восстановленные ответы.
- **Промпт:** «P74: покажи какие lesson bindings сохранились и ответы послеcoldrestore, не только множество типов».
- **GOOD/BAD:** «Markercoverage1 при3потерянныхправильныхзаконах» / «Все12типовсохранены—все знанияцелы».

## P75 — Float32 residual выбирает запрещённый token

- **Основание:** N37 sampler mask escape, [84](84-check-source-coverage.md).
- **Получатель:** stochastic CD/TR, actual Sampler; S→C→U, не обучение.
- **A/B/input:** native62logits:61равны0,last−100,topK61,r=.99999999; current fallback versus last positive-support token. Negative — r=.25 и greedy; edge — last allowed/last EOS.
- **Пилот/метрика:** 4controlled draws×3support configs; support violations/draws ↓, prematureEOS/count; цена — sampler µs. Реальная частота определяется отдельной репрезентативной sampling панелью, не этим forced r.
- **Отказ:** production greedy — неприменимо; JS Float32 proxy не native Softmax.
- **Промпт:** «P75: зафиксируй probability mass/support/draw/selectedID и проверь actual native branch, без Student-training».
- **GOOD/BAD:** «Forced residual нарушилsupport; частотанеизвестна» / «Редкий stochastic edge объяснил greedy RP01».

## P76 — UTF-8: хвост и повреждения внутри ответа

- **Основание:** [62](62-utf8-tail-hypothesis-and-retained-student-answers.md).
- **Получатель:** CD/TR, BPE bytes→Decode→consumer; R/C→U.
- **A/B/input:** exact same generated bytes, current cutoff против complete-scalar cutoff; zero new model calls in C. Negative — valid complete Cyrillic/emoji; edge — invalid interior sequence и literal U+FFFD.
- **Пилот/метрика:** 8byte sequences +14saved texts R; replacement chars/interior/tail count ↓, preserved required characters/count; цена — bytes/tokens retained and latency.
- **Отказ:** saved receipt не содержит token IDs/bytes — точную причину U+FFFD не восстановить; tail repair не лечит interior corruption.
- **Промпт:** «P76: сначала отдели tail/interior и literal replacement отdecode error; не генераторныйgainизcleanup».
- **GOOD/BAD:** «Конец сталвалидным,смыслнеулучшился» / «Все14поврежденийобъясняютсяпоследнимбайтом».

## P77 — Редкий класс и знаменатель оценки

- **Основание:** [73, 1–3](73-rare-class-evidence-and-structural-feature-scope.md), [18](18-feedback-routing-and-historical-split.md).
- **Получатель:** любой classifier, каждый domain отдельно; R→C, L лишь отдельный lesson contrast.
- **A/B/input:** то же frozen state, original panel против независимой панели сrare-law support; нельзя назвать score delta обучением. Negative — singleton train-only class; edge — secVal≤1 и substituted general metric.
- **Пилот/метрика:**2independent exam cases/rare class плюс4common; per-class numerator/denominator, macro recall, unknown support; цена — gold acquisition minutes.
- **Отказ:** independent rare examples отсутствуют — `INSUFFICIENT_SUPPORT`, не zero failure и не100%.
- **Промпт:** «P77: покажи support perclass/sourcefamily и отличи missing metric от реального качества».
- **GOOD/BAD:** «LOWестьво всех3классахpositiveE40panel» / «Другойsplitриск обнуляетвсеpositivearchives».

## P78 — Loss plateau и фактически полученный бюджет обучения

- **Основание:** [70, 3/66](70-embedding-pool-and-dpo-stop-consumers.md), [74, micro corrections](74-helper-master-reconciliation-and-research-priorities.md).
- **Получатель:** generated capability DPO, не direct RP01 batchloop; C→L-neural→U.
- **A/B/input:** window1→5 только вexistingmetadata при fixedmax5/threshold. Same policy/reference/data. Negative — loss flat ноknowncorrect; edge — падаетloss безnewlaw и delayedgain.
- **Пилот/метрика:** максимум5updates/arm,8same exam; actual updates, stopreason, correct/8, seconds/examples-to-quality; цена — additionalcompute. Не требовать+2/12.
- **Отказ:** caller monitor не использует — неприменимо; большеupdates сами по себе неsuccess.
- **Промпт:** «P78: измени толькоstopwindow, сохраниdecision накаждомstep и сравниquality/стоимость».
- **GOOD/BAD:** «Та жеqualityза3вместо5updates» / «Smalllossdeltaдоказываетдостигнутыйнавык».

## P79 — E05 grammar видел проверочные исходники

- **Основание:** [50, E05 и поправка протокола](50-historical-e05-e20-e21-source-lineage.md).
- **Получатель:** CS, GrammarMaskFilter+fixed predictor; R/C→L-memory.
- **A/B/input:** grammar built train-only против train+exam, predictor unchanged; leaked arm diagnostic. Negative — grammar-permitted ноsemanticwrong transition; edge — unseen valid ASTpair.
- **Пилот/метрика:**4train/4sealedfiles,16next-kindcases; correct/16, allowed-valid/valid, false-block/count; цена — tablebytes/lookupµs.
- **Отказ:** current predictor отличается отoldtrigram — неarchiveparity; grammar legality неprogramcorrectness.
- **Промпт:** «P79: заморозьpredictor ипротоколmembership grammar; leakageсчитайотдельно».
- **GOOD/BAD:** «Testgrammarподнялаscore,validbaselineтолькоtrain» / «Новыйpredictor+oldgrammarэтоодинфактор».

## P80 — E20: сохранность markers с правильным знаменателем

- **Основание:** [50, E20](50-historical-e05-e20-e21-source-lineage.md).
- **Получатель:** RT/CS stack detection/normalizer; R→C.
- **A/B/input:** одинnormalizer current/on-off наоднихinputs; compare preserved markers/actually present, отдельноpossible6-slotcoverage. Negative — marker отсутствовал; edge — case/punctuation/alias.
- **Пилот/метрика:**8inputs,каждыйсизвестными1–3markers; retained present/present↑, spurious markers/predicted↓; цена — extractionµs иroutingerrors.
- **Отказ:** denominator6×rows измеряетcoverage, неloss; этотscoreнепоказывает72%потерю.
- **Промпт:** «P80: выпишиmarkersдо/после иобаdenominator,затемпроверьфактическийroute».
- **GOOD/BAD:** «Сохранено10из10присутствовавшихmarkers» / «27,78%coverageзначитпотеряли72%имеющихсяпризнаков».

## Новое поступление 38/39

Карточки P81 и далее находятся в [87 — новые проверки](87-new-checks-38-39.md). Они дополняют, а не перенумеровывают P01–P80. Свежий плановый разбор helper-текста не является повторной проверкой названного им product source или выполнением его proxy.
