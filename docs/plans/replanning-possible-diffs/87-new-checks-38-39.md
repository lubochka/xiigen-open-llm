# Дополнение к каталогу: P81–P91, поступление 38/39 и уточнение E13

16 октября 2026. **DESIGN_ONLY / NOT_RUN.** Все правила [81](81-validation-program.md), [82](82-result-reporting-and-examples.md), [83](83-check-catalog-and-prompts.md) и доменные протоколы [85](85-domain-protocols.md) действуют здесь. Числа задают стартовый различающий пилот, не гарантию, квоту или таймаут. Промпты исполняются только после прямой авторизации соответствующего опыта. Прочитаны применимые разделы helper-текстов для проектирования (`plan-source-read`); новые primary-source подтверждения, proxy, native runs и обучение не заявляются.

Новые вопросы взяты из поступления planning `1f42ba7c89→30f2078122`, пофайловый реестр [86](86-helper-30f20781-intake.md) и mapping [84](84-check-source-coverage.md). Точный путь 86 сверяется при приёмке пакета. E13 fair-training P91 дополнительно закрывает отдельный старый пункт отчёта11, который нельзя заменять новой expression-body проверкой.

## P81 — Предпочтение зависит от исходника или только от текста ответа

- **Гипотеза/источник:** одинаковый answer prior может скрываться под ростом DPO margin; [N38 counterfactual, раздел метрики][n81].
- **Домен/получатель/категория:** D01/CD, отдельно D04/TR при подходящем checkpoint; actual native scoring B0/C50; **C**, без optimizer. Новые forward calls не являются R/read-only оценкой уже сохранённого текста.
- **A/B/input:** сравнить B0 и C50 на тех же6–12 парах исходников q0/q1 с одним изменённым законом и противоположными правильными y0/y1. Внутри каждого checkpoint проверяется только замена q. Контроль — framing-only допустимый prompt; negative — meaning-preserving variable rename; edge — разная длина y.
- **Пилот/метрика:** 6 контрфактов ×2 checkpoint ×2 prompt ×2 response scores. `d(M,q)=logP(y0|q)−logP(y1|q)`; `C(M)=d(M,q0)−d(M,q1)`; `G=C(C50)−C(B0)` в log-probability units. Главная предметная мера — число пар с **d(q0)>0 и d(q1)<0** /6 ↑. Sum фиксирован, mean только диагностика длины. Цена — native forward calls, ms/pair.
- **Опровержение/предел:** C>0 без двух правильных знаков — слабый сигнал; сдвиг checkpoint не изолирует каждый training factor. Если native score не доступен, нет результата этой проверки.
- **Промпт:** «После разрешения C/P81 посчитай signed scores на одних q0/q1/y0/y1, без update и генерации; покажи оба знака, G и framing-only control».
- **GOOD/BAD:** «4/6 пар различают оба закона после C50» / «Средний margin вырос, значит ответы зависят от программы».

## P82 — E09: один файл по обе стороны split

- **Гипотеза/источник:** node-prefix split может разделить один файл между train/val; [N38 E09, H38-B и MICRO][n82].
- **Домен/получатель/категория:** D02, E09 actual AST extractor/predictor; R при сохранённых node maps, **C** для нового Roslyn parse, L-memory для нового обучения; U только с отдельным production caller.
- **A/B/input:** одна фиксированная unseen-file панель, same codec/k/distance; node-prefix training против целых train-only files с согласованным количеством transitions. Cross-file edges удалены одинаково в обоих arms, это другой P27. Negative — split ровно между файлами; edge — файл пересекает80% границу.
- **Пилот/метрика:** исходный manifest для исторической реконструкции; новый pilot4train/4exam files, 16 lawful transitions. File overlap/count, macro correct next-kind по файлам ↑; цена — train rows, parse ms, memory.
- **Опровержение:** historical file list отсутствует — `NOT_RECONSTRUCTABLE`; при sequential concat обычно straddles один boundary file, не вся панель.
- **Промпт:** «P82: сначала покажи cumulative node boundaries и actual overlap; новый корпус не называй восстановленным историческим».
- **GOOD/BAD:** «Один boundary file shared; влияние на качество ещё отдельно» / «Весь исторический PASS ложен по формуле split».

## P83 — E13 expression body превращается в Empty

- **Гипотеза/источник:** Body-only extractor теряет expression-bodied методы; [N38 E13 expression, source proof/MICRO][n83].
- **Домен/получатель/категория:** D02, MethodExtractor→MethodKnnPredictor; C→L-memory→U при caller. D01 только если его consumer действительно читает этот результат.
- **A/B/input:** current Body-only против ExpressionBody fallback при неизменных codec/k/pairs. `async Task<int> Fetch()=>await ReadAsync();` и `int Get()=>Calculate();`; positive counterparts — block body; edge — `=>` с null/guard expression.
- **Пилот/метрика:**4 native syntax fixtures, затем12train methods/12source-disjoint exam; Empty bags/methods ↓, semantic next-method correct/12 ↑; цена — extractor ms, feature bytes, ошибки block controls.
- **Опровержение:** native version даёт другой AST либо gain только вtrain — mechanism/overfit; не менять labels одновременно.
- **Промпт:** «P83: выведи BodyIsNull, ExpressionBody, real kinds и bags; измени только путь извлечения body».
- **GOOD/BAD:** «Expression-body стал различим без изменения block outputs» / «JS bag proxy доказал качество Roslyn consumer».

## P84 — Lambda family и сохранённая arity

- **Гипотеза/источник:** SimpleLambda/ParenthesizedLambda разделяют общую семантику, но arity может быть существенной; [N38 E13 lambda H38-L][n84].
- **Домен/получатель/категория:** D02 MethodKnn; C→L-memory. D10 только со своим AST parser.
- **A/B/input:** current categories против общего Lambda при fixed other features. Второй независимый contrast — добавление arity к уже canonicalized category. `x=>x+1`/`(x)=>x+1`; negative — `(x,y)=>x+y`, где arity влияет на overload; edge — async lambda.
- **Пилот/метрика:**8train contexts,8new contexts; correct next-type/8 ↑, unsafe arity conflations/controls ↓; цена — feature bytes, query time.
- **Опровержение:** общий marker ухудшает значимую arity — зафиксировать цену, не требовать безусловной унификации.
- **Промпт:** «P84: B1 только family, B2 arity отдельным A/B; не смешивай оба изменения в одном объяснении».
- **GOOD/BAD:** «Canonical family помогает эквивалентным формам, arity сохраняет важный контраст» / «Все lambda одинаковы по смыслу».

## P85 — Single-label достаточен только для своего контракта

- **Гипотеза/источник:** canonical Guard скрывает Await, если downstream требует оба; [N38 E13 H38-C][n84].
- **Домен/получатель/категория:** D02/D08, actual method-label consumer; C→L-memory→U.
- **A/B/input:** unchanged legacy label плюс optional composite obligations versus legacy-only; gold выбирается по требованию consumer, а не переписывает валидный старый E13. `{If,Ret}`, `{If,Ret,Await}`, `{Try,Await}`; negative — consumer требует только priority class; edge — Loop+Await+Guard.
- **Пилот/метрика:**6 composite cases+4single controls; fulfilled required obligations/required ↑, correct route/cases; цена — extra labels, memory/latency.
- **Опровержение:** downstream не требует composite — NOT_APPLICABLE, legacy top1 остаётся законным.
- **Промпт:** «P85: докажи потребность consumer в двух свойствах; оцени полный набор требований отдельно от исходной single-label accuracy».
- **GOOD/BAD:** «Guard верен как класс, но async obligation потеряна downstream» / «Старый gold Guard неправильный всегда».

## P86 — Fractional legacy mask: forward и backward одной функции

- **Гипотеза/источник:** weighted mean forward и positive-position mean backward расходятся; [N38 fractional mask, математика/границы][n86].
- **Домен/получатель/категория:** только фактический legacy DPO caller; S→C, L-neural отдельно. Current RP01 exact-v2 binary — контроль неприменимости.
- **A/B/input:** same logits[-1,+1], mask[.25,1]; analytic gradient versus finite difference реального loss. Negative — [1,1], exact-v2; edge — zero upstream, zero mask admission. Исправление backward не смешивать со сменой objective.
- **Пилот/метрика:**2 coordinates ×3 masks ×3 epsilon scales; derivative error по правилу P10 ↓, sign agreement; цена — forward calls. Никакого optimizer на C; один реальный update уже L-neural.
- **Опровержение:** upstream не допускает дробные masks — latent contract only; binary parity не доказывает fractional support.
- **Промпт:** «P86: покажи native forward score, mask normalization и numerical derivative; не трогай guarded exact-v2 ради legacy edge».
- **GOOD/BAD:** «При дробной mask error есть, binary control проходит» / «Этот legacy edge объясняет RP01 exact campaign».

## P87 — Две objective в одной optimizer transaction

- **Гипотеза/источник:** generic StepMany может смешивать legacy mean и exact sum; [N38 mixed objective, G0–G3][n87].
- **Домен/получатель/категория:** реальный generic DPO caller; C admission/gradient, L-neural если optimizer; CD/TR только с compatible material.
- **A/B/input:** same admitted semantic pairs, all-exact versus one exact/one legacy; сначала guarded profile, затем permitted generic path только если существует. Negative — two exact; edge — ObjectiveVersion null, разные lengths. Reference/LR/permit одинаковы.
- **Пилот/метрика:**2pairs, 1transaction; reject-before-mutation/mixed ↑ для guarded contract; per-pair gradient norm/share, version attribution, params changed/count; цена — calls/time. Quality requires отдельные8domain cases, не aggregate mixed loss.
- **Опровержение:** guarded RP01 отвергает legacy — защита работает; generic never reached — не product cause.
- **Промпт:** «P87: установи кто принимает mix, покажи loss отдельно по objective и точку отказа до update».
- **GOOD/BAD:** «Generic route допускает mix, current RP01 запрещает» / «Средний loss разных objective показывает улучшение».

## P88 — T35 получает новый контекст или выдаёт прежний oracle

- **Гипотеза/источник:** fixed packets полезны для parity, но могут быть неверно приняты за general input-sensitive Unit; [N38 T35, 1–4][n88].
- **Домен/получатель/категория:** D08, T34TransitionDomainSplitterUnit и T34LogicalFlowAdapter отдельно; C/U, без training claim.
- **A/B/input:** canonical PhaseContextPacket против того же packet с удалённым решением dynamic runtime; второй contrast — contradictory decision. Flags неизменны. Negative — UseContextCarryForward=false; edge — новый domain без oracle packet.
- **Пилот/метрика:**canonical+2counterfactual+1gate control; source-supported fields/all returned ↑, invented covered decisions/count ↓; цена — processing time. Confidence0.92 описывается как constant, не эмпирическая accuracy.
- **Опровержение:** продукт обещает только fixed canonical scenario — narrow parity valid, generalization requirement неприменимо.
- **Промпт:** «P88: покажи output difference при удалении upstream evidence и отдельно preserved original parity».
- **GOOD/BAD:** «Вне canonical scenario появились unsupported packets» / «T35 значит все Units проекта не обучаются».

## P89 — Плотность teaching vector меняет решение stop через loss scale

- **Гипотеза/источник:** density→reported weighted loss→absolute-delta stop; [N38 gaps, N38.02][n89], [30](30-teaching-vector-adamw-and-current-recipient.md), [70](70-embedding-pool-and-dpo-stop-consumers.md).
- **Домен/получатель/категория:** generated capability monitor, не direct RP01; S→C, L-neural для реального training effect.
- **A/B/input:** сначала одинаковая сохранённая unweighted loss trajectory и разные lawful density scales; only monitor input scale меняется. Native L отдельно меняет density при fixed data/state, показывает одновременно изменённый gradient и loss — не выдаёт это за чистый stop-only effect. Negative — normalized trajectory; edge — delta ровноthreshold.
- **Пилот/метрика:**5step traces×2scales; stop step, actual updates saved, ms-to-same-quality;8law cases только L. Цена — лишние updates/преждевременно потерянное качество.
- **Опровержение:** real caller не использует этот monitor/сохраняет нормированный score — неприменимо; AdamW scale invariance не обязана быть точной.
- **Промпт:** «P89: раздели чистый monitor replay и реальное density-learning; покажи путь конкретного loss field к stop».
- **GOOD/BAD:** «На той же траектории другое scale изменило stop» / «Density изменила и gradients, но весь эффект приписали monitor».

## P90 — E47: exact tags и настоящий fallback без tags

- **Гипотеза/источник:**12 tag-friendly cases не проверяют нужность BM25 fallback; [N39 E47, scope/MICRO][n90].
- **Домен/получатель/категория:** D07, ModelHubSearcher/BM25EpisodeMatcher actual path; R/C/U.
- **A/B/input:** exact tag baseline против conditional fallback только при отсутствии однозначного exact hit. BM25-only — отдельный comparator. Original12 cases regression, новые tag-absent и ambiguous frozen до выбора threshold; negative — unrelated topic; edge — security+mobile query.
- **Пилот/метрика:**12existing+10new(no-tags)+5ambiguous; top1/top3 по каждой группе ↑, unsafe route/ambiguous ↓; цена — no-hit/abstain, ms/query и RAM.
- **Опровержение:** fallback не помогает и на tag-absent — нет поддержки на данном scope; historical12→11 не стирать и не распространять на все retrieval.
- **Промпт:** «P90: докажи отсутствие literal/normalized tags в новой группе и сравни exact-preserving fallback на одной панели».
- **GOOD/BAD:** «Fallback улучшил no-tag queries ценой latency» / «BM25 проиграл12tag cases — весь семантический retrieval бесполезен».

## P91 — E13: одинаковые учебные пары у baseline и kNN

- **Гипотеза/источник:**80% методов и80% пар соседних методов дают разный learned material; [11, раздел9](11-new-helper-source-reconciliation.md).
- **Домен/получатель/категория:** D02, E13MethodLevelKnn; R при достаточном historical manifest, C extraction, L-memory сравнение.
- **A/B/input:** current unequal construction против одной заранее materialized train-pair list для обоих methods; та же sealed exam panel. Negative — набор, где границы совпадают; edge — одиночные методы/последняя пара файла. Features/label codec без изменений.
- **Пилот/метрика:**4files,12methods,8fixed next-method cases; train-pair overlap/count и macro correct/8; цена — число доступных примеров/bytes. Старые5/7vs2/7 остаются receipt своего режима.
- **Опровержение:** exact48methods недоступны — historical leakage count unknown; новый контроль не опровергает архив автоматически.
- **Промпт:** «P91: покажи actual train pair IDs двух algorithms до quality comparison и не смешивай это с expression-bodyP83».
- **GOOD/BAD:** «Разница состава устранена, полезность kNN осталась» / «Любой найденный split risk отменяет положительную память».

[n81]: <C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-NEW-RP01-INPUT-INSENSITIVITY-COUNTERFACTUAL-DPO-2026-10-16.md>
[n82]: <C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-NEW-E09-WITHIN-FILE-HOLDOUT-LEAKAGE-2026-10-16.md>
[n83]: <C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-NEW-E13-EXPRESSION-BODY-COLLAPSE-2026-10-16.md>
[n84]: <C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-NEW-E13-LAMBDA-FAMILY-AND-COMPOSITE-LABEL-2026-10-16.md>
[n86]: <C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-NEW-LEGACY-FRACTIONAL-RESPONSE-MASK-GRADIENT-MISMATCH-2026-10-16.md>
[n87]: <C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-NEW-MIXED-EXACT-LEGACY-DPO-OBJECTIVE-IN-SAME-STEP-2026-10-16.md>
[n88]: <C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-NEW-T35-UNIT-INPUT-ERASURE-AND-CONFIDENCE-2026-10-16.md>
[n89]: <C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-CODEX-NEW-CAUSAL-GAPS-GENERIC-KNN-DPO-LOSS-REPLAY-FUSION-2026-10-16.md>
[n90]: <C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/39-NEW-E47-LEXICAL-ORACLE-CONFOUND-AND-DOMAIN-ROUTING-2026-10-16.md>
