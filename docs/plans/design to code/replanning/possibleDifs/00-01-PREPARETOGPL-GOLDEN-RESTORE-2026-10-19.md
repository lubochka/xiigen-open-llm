# ГЛАВА 0 — PrepareToGPL: прежде всего вернуть работающее обучение (19.10.2026)

**Назначение:** отдельная начальная глава каталога возможных различий XIIGEN. Не подменяет огромный реестр других исследователей: закрепляет положительный эталон, причинные механизмы, границы доказательств и порядок **малых** проверок. Новые узкие гипотезы исследования N55 описаны в соседней [карточке](55-NEW-SOURCE-SEAMS-EVALUATION-AND-POLICY-2026-10-19.md); опыты — в [исполняемом экспериментальном плане](55-RP01-DOMAIN-CAUSAL-MICROEXPERIMENTS-2026-10-19.md).

**Снимок сравнения:** [PrepareToGPL@31346d8](https://github.com/lubochka/llm_mvp_core/tree/31346d8928be4469b01aaf313780e8f950ff91e2), [текущая ветка codex/dpo-implementation@6d36492](https://github.com/lubochka/llm_mvp_core/tree/6d36492714ebc1b9ae5f31a5a1da00673f524d83). Общий предок df379c6249042b354fb49c3fcc65a055bd469f00; ветки **diverged**, current на 1190 коммитов впереди общего предка, GOLD имеет 19 неперенесённых коммитов. Различие файлов/контрактов не доказывает причину ухудшения, пока не проверены вход, recipient, обыкновенный consumer и frozen quality.

**Неприкосновенно:** llm_mvp_core/docs/plans/replanning-possible-diffs — read-only. Ни одно качество в этой записке **не измерялось заново 19.10**: native результаты ниже получены из ранее сохранённых отчётов и отделены от наших новых гипотез.

## 0.1. Главное: ПЯТЬ проверок восстановления уже доказанной способности

| Очередь | Что сохранять/восстанавливать, а не переписывать заново | Подтверждение | Минимальный различающий опыт |
| --- | --- | --- | --- |
| **GOLD-1** | Прежний translation checkpoint: четыре направления RU_EN/EN_RU/HE_EN/EN_HE → runtime из шести направлений. В schema 1 нет отдельной версии состава направлений. | [Археология GOLD и tracked asset](00-PREPARETOGPL-RECOVERY-FIRST-2026-10-18.md), [другой аудит](41-FOUR-VS-SIX-TRANSLATION-CHECKPOINT-MIGRATION-2026-10-18.md). Существующее расхождение уже найдено; **не новая N55**. | Один старый checkpoint: старый consumer, current импорт как есть, current с явной миграцией. Выученные старые пары и 12–24 прежних ответов должны сохраниться; новые направления остаются пустыми. |
| **GOLD-2** | Обученная память OrchestrationReasoning из старого v1 должна доходить до новой версии loader, а не заменяться пустым Unit. | [Проверенный source seam с 15 patterns](52-ORCHESTRATION-REASONING-CHECKPOINT-AND-ADMISSION-GAP-2026-10-19.md) и [GOLD experiments](52-ORCHESTRATOR-GOLD-DOMAIN-CAUSAL-MICROEXPERIMENTS-2026-10-19.md). | Одинаковые входы к orchestration с GOLD state, current with no migration, current explicit migration; сравнить выбранный шаг/обоснование, не только факт загрузки. |
| **GOLD-3, положительный контроль** | Реальную компоновку частей урока: Tools → phrase_mappings → Executor → Flow, без потери фраз и связей. | [Native matched A/B](https://github.com/lubochka/llm_mvp_core/blob/6d36492714ebc1b9ae5f31a5a1da00673f524d83/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/report.md): **11/24 → 19/24**, а новые сочетания 0/8 → 8/8. **Уже исправлено** в соответствующем текущем пути; это контроль от регрессии, не открытый дефект. | Сохранить 8 уроков, проверить 8 новых композиций и 5 негативных границ; запрещено выкидывать phrase mappings при других декомпозициях DPO. |
| **GOLD-4, положительный контроль** | Обученное состояние Executor после cold process: exact/source, 16 phrase maps, 24 KNN signatures, training identities и routes. | [Native P125](https://github.com/lubochka/llm_mvp_core/blob/6d36492714ebc1b9ae5f31a5a1da00673f524d83/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/report.md) **0/24 → 19/24**, [native P25 cold](https://github.com/lubochka/llm_mvp_core/blob/6d36492714ebc1b9ae5f31a5a1da00673f524d83/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/report.md): **24/24** идентичные ответы/route/confidence/verdict, 19/24 смысловая точность. | Запуск обученной памяти из GOLD asset и из нынешнего формата + matched sealed панели; не приписывать neural LoRA качества этим испытаниям. |
| **GOLD-5** | Исторически полезные алгоритмы структуры в правильном *носителе*: POS sequence KNN в переводе, AST признаки в коде, domain-specific context, KNN over matrix-invalid experiments; убедиться, что реально вызываются продуктом. | [E25 positive synthetics 34.38%→87.5% top1](50-EXTERNAL-DOMAINS-E25-NOVEL-POS-SEAMS-2026-10-19.md); [прямая parity файлов E09/E26/E43](51-GOLD-BRANCH-EXPERIMENT-CODE-PARITY-AND-E26-DETERMINISTIC-LOOKUP-2026-10-19.md); [matrix→KNN](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md). Исходники некоторых алгоритмов идентичны — нельзя объявлять их потерянными только по названию ветки. | Small A/B: same corpus + matched heldout, production consumer without/with POS/AST/word-order features, измерять также ошибки на неподходящих доменах. |

**Жёсткая граница GOLD-3/GOLD-4:** у native P125 из 22 принятых ответов три были ошибочными; «do not delete the file» превращалось в «удали файл», а вопрос о прошлом — в команду. Поэтому 19/24 — реальная, но **ограниченная** способность. Рост coverage без отслеживания false accepts не считается улучшением безопасности/семантики.

## 0.2. RP01: настоящий адрес сбоя

[RP01 code map](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-CURRENT-CODE-MAP.md) и [causal recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md) описывают **другого получателя обучения**: случайную нейронную base B0 и DPO + q/v LoRA, а не Executor KNN. C#500 = 25 смысловых шаблонов ×20; EN→RU500 = 20 категорий ×25; после реальных обновлений весов сохранённая free-response панель осталась C# **0/3→0/3** и EN→RU **0/4→0/4**. Прирост DPO margin и ненулевой tensor delta **не равны** правильным ответам.

**Требуемый контроль получателя:** четыре плеча на **одном заранее закрытом** наборе запросов: A — случайный neural Student; B — GOLD обученный TranslationExecutor; C — neural + тот же GOLD Unit; D — neural после DPO + Unit. Одинаковая оболочка продукта, два условия fresh-process и same-process; каждое решение сохраняет route, unit id, checkpoint id, memory version, policy version, model tensor/tokenizer hash и output. Иначе «хуже» может означать разные продукты/получателей, а не регрессию одного алгоритма.

## 0.3. Полный реестр проверяемых классов разрыва (не список доказанных причин)

| Область | Возможный разрыв / проверяемая инварианта | Релевантные темы обучения и тест |
| --- | --- | --- |
| 01 Данные/пары | chosen и rejected противопоставляют реальный смысловой закон, не лишь длину/стиль; независимый автор, отсутствие teacher annotations в model-visible тексте | C# docs, SQL, EN→RU: 8–12 same-prompt семантических противопоставлений, fidelity до обучения |
| 02 Независимость данных | 500 перефразировок не равны 500 независимым навыкам; group-disjoint split by source, author, template, family | C#, перевод, дизайн; leak-resistant heldout |
| 03 Математика DPO | chosen/rejected response masks не меняют смысл, correct length-normalization/gradient/reference policy; sign и finite-diff derivative | нейронный Student, короткие пары + неравные ответы |
| 04 Base/LoRA | Base B0 даёт язык/синтаксис; q/v LoRA действительно находится на backprop пути; rank/alpha/target projections, bias and embedding gradient | только neural; tiny chosen-only SFT vs DPO-only; измерить free-answer |
| 05 AdamW/update | nonfinite, clip scale, learning-rate, optimizer state/save не обнуляют обучение | neural, 1–4 настоящих updates + state hash |
| 06 Токенизация | BPE retrain, EOS/PAD/OOV/Unicode, max context, masked response bits, train vs inference token identity | C#, RU/HE/EN; roundtrip tokenizer/version |
| 07 Vector representation | order, POS, AST, graph edges и language direction не потеряны при hashing/pooling; encoder/index version pins | раздельные панели: natural language ≠ AST |
| 08 KNN retrieval | правильные domain/direction/predicate scope, candidate pool, k/topK, newest correction/replay; false positives при теряемом отрицании | перевод, code AST, wiki |
| 09 Matrix→KNN | исторический «плохой» matrix test может давать хороший ranking в KNN; проверять representation+recipient факторно | AST, sentence order, dynamic law |
| 10 DPO→Units | один исходный закон → несколько Unit с **сохранением** direction, roles, graph edge, chosen/rejected contrast; metadata and SourceId survive | перевод, design-to-code, planning |
| 11 Unit implementation | learned Unit вызывается из обычного Flow, training-state не притворяется метрикой quality | перевод, orchestration |
| 12 Graph/Flow/host | GraphRAG/tier routing не направляет запрос на bootstrap/deterministic substitute; фиксировать route provenance | wiki, orchestration, C#, IDE |
| 13 QualityVerifier | проверяются negation, questions, tense, subject/object, SQL constraints; rejected examples не превращаются в wrong target dictionary | перевод, SQL, docs |
| 14 Restore | checkpoint includes executor/phrase maps, KNN, weights, tokenizer, optimizer/RNG, model+policy provenance; old versions migrate explicitly | GOLD→current, fresh-process |
| 15 Continual learning | корректировки не стирают старые уроки; replay ratio/rank and early stopping определяются смысловой validation | многодоменные sequential A→B→A |
| 16 Evaluation | same domain, direction, recipient, frozen gold; report both correctness and false accepted; DPO score не semantics | все профили; новый узкий N55-01 |
| 17 Hidden deterministic layers | distinguish learned weights, learned memory, plain rule change and routing gain | negative controls: no-training vs memorized vs rule-on |
| 18 Provenance | same named checkpoint can change behavior if executable retrieval policy changes after save | все KNN; новый узкий N55-02 |

Детали многих пунктов **ранее обнаружены**: [25 групп/125 checks](44-REQUESTED-MECHANISMS-COVERAGE-2026-10-19.md), [исторический экспериментальный реестр](02-HISTORICAL-EXPERIMENTS.md), [реальный DPO bridge](02-DPO-DATA-OBJECTIVE-GRADIENT.md), [E09/E26/E43 caveats](51-GOLD-BRANCH-EXPERIMENT-CODE-PARITY-AND-E26-DETERMINISTIC-LOOKUP-2026-10-19.md). Повторное перечисление — coverage/integration, **не 18 новых открытий**.

## 0.4. Что делать сначала и чего НЕ делать

1. **Сначала GOLD checkpoint + настоящий consumer** и контрфактическая сохранённая способность (GOLD-1/2/4). Сравнивать один и тот же набор ответов до и после migrate.
2. Научить/проверить **один** конкретный Unit по работающей P125 цепочке, отделив новые комбинации знакомых частей от новых фактов.
3. Устранять semantic false accepts отдельным независимым quality score, не подгонкой tests, и не ломать verified 19/24.
4. Для RP01 отдельно проверить **tiny warm-base vs random B0, chosen-only SFT vs DPO**, а не автоматически переписывать переводческий kNN ради neural failures.
5. Только после recipient identity исследовать длинные много-доменные программы, replay, новый vector tokenizer и LoRA оптимизацию.

**Отказ от ложных причин:** совпадающий исходник алгоритма между GOLD/current означает, что надо искать wiring, data/state, ranker и consumer. Положительный эксперимент POS classifier **не** является доказательством neural LoRA. Результат failed matrix classifier **не** отменяет ценность соответствующего признака как KNN distance. DPO loss не является accuracy. Windows CI не является оценкой качества обученной системы.

## 0.5. Evidence / review packet

- **Confirmed historical native:** P125 0→19/24, ingress 11→19/24, P25 retained 24/24.
- **Confirmed source only:** 4→6 translation checkpoint compatibility seam, current reference paths, hard-coded evaluation direction (см. N55), executable KNN admission policy (см. N55).
- **Historical scoped but not native reproduced today:** E25 34.38→87.5 POS classifier synthetic; E09/E26/E43 transfer claims require carrier and denominator checks.
- **Hypotheses not validated on native code today:** N55-01/N55-02 and all new arms in the experimental protocol.
- **Out of scope:** product code edits, rewriting other agents' files, protected core docs, external model training, running costly Windows workflows.
- **Skill lens:** architectural boundaries → exact source/consumer; experiment design → baseline/enhanced/metric/tier/falsifier; review → honest evidence scope; human communication → «домен» = предмет обучения, EN→RU = направление/профиль, KNN/LoRA = механизмы.
