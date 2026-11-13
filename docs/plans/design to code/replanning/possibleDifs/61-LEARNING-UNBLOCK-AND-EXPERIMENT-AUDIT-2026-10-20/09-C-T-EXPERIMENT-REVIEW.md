# Проверка C/T-карточек по актуальной границе обучения

**Срез документов:** core `f5dd532045716495a2c7668573250e5d7cc46810`. **Срез original code:** `bf9b543e149ef197da85a767821d98c9b67916d2`; по compare к f5dd code не изменён. Проверка статическая, native обучение и inference не выполнялись. Защищённый `docs/plans/replanning-possible-diffs` не редактировался.

## 1. Действующее правило и важная коррекция вывода

Latest [03-translation L3–9](https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/03-translation.md#L3-L9) разрешает зависимые звенья до/после update: загрузку, подготовку, восстановление и использование выученного состояния. Отдельного Train ради галочки не требуется. Общая блокировка до повторного подтверждения каждого ID/SHA отменена. Учебные изменения остаются первыми по приоритету.

Поэтому ранняя оценка старого T как OUT_OF_LEARNING только из-за query-фактора **отозвана**. В T reader использует записи, созданные настоящим AddEntry, и его поведение зависит от них. Корректное название результата — способность использовать выученную контекстную память. Такой результат не равен улучшению алгоритма усвоения уроков.

Ссылка на конкретный источник при проверке зависимого звена обязательна. Само имя kNN/restore/Train не доказывает причинность. Если actual production caller не найден, допустимый компонентный вывод не расширяется до диагноза RP01.

## 2. Семантическая оценка прочитанных карточек

| Карточка / фактор | Native обучаемый получатель и зависимость | Оценка |
|---|---|---|
| [02 C: SyntacticRole](https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/02-programming.md#L17-L53) | StructuralVector → StructuralKnnPredictor.Train → stored pairs → Predict | Корректный компонентный фактор представления на TRAIN и query. 2 Train/16 Predict; k1 одинаков. PositionInParent уже различает операнды, no effect роли допустим. Не измеряет генерацию программы |
| 02 SQL ON/WHERE | Карточка сама не устанавливает native trainer/consumer | Предметный материал конкретен, но source-путь пока условен. SQL interpreter может быть oracle, не learner. Отсутствие готового host — задача подготовки; не общий запрет и не результат эксперимента |
| [02 RX1/RX2](https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/02-programming.md#L82-L114) | В документе заявлен preprocessing → Facade.Compile → FileActionKnn read после raw-source обучения | Сохранившееся основание OUT_OF_LEARNING «query-only» устарело по правилу20.10. Это может быть проверка использования обученного состояния, если original call-chain/fix подтверждены. Данное замечание не возобновляет конкретный старый отменённый запуск |
| 02 OBS1 / case comparer / receiver key | Заявлены source extraction → MethodFrequencyTable counts → RankMethods | Учебный фактор обоснован при достижении named writer. В этом подаудите source этих extractors полностью не перечитывался; независимый статус не повышен до CODE_VERIFIED |
| [03 T query POS](https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/03-translation.md#L48-L92) | Native ContextualDictionary.AddEntry → _entries → TranslateWithContext | Допустимый опыт использования выученного состояния. A/B отличается reader-input, сама запись уроков одинакова. Production writer/context-loss не установлен. Ни ingestion gain, ни LoRA improvement не следуют |
| [03 T-compose](https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/03-translation.md#L95-L121) | TrainStructuredBatchAsync → Executor.TrainOnExample → learned phrase memory → ordinary Flow | Допустимая acquisition-проверка: A без batch, B с двумя уроками. Пустой A — контрфактический контроль, его не нужно обучать фиктивно. Не является сравнением двух алгоритмов обучения; exact readiness/route нужно показать |
| [20 N57-ARG](https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/20-additions-n57-argument-and-verifier.md) | Дополнительный ArgumentOrdinal хранится в реальных TRAIN vectors и читается distance/Predict | Корректный узкий training representation A/B. Actual source-derived vectors/labels нужны из native Roslyn; k1 фиксирован; 2 Train/16 Predict. По source признаки wrapper сейчас стирают номер аргумента. Это известный N57, не новая гипотеза |
| 20 N57-VER | BuildRuEnGeneralBatch → две verifier rows → QualityVerifier.TrainOnExample → marker rules → Verify | Корректный writer+reader фактор одной общей функции. 4 TrainOnExample/16 Verify, не generation. Русское о отсутствует в текущем helper; native no-ADV/RuleTags contrast всё равно надо сохранить в receipt |
| [23 H60-A/H60-B](https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/23-additions-n60-small-learning.md#L19-L52) | Score/timing → actual selection/admission → TrainRowsAsync → TrainStructuredBatchAsync | Настоящий возможный curriculum-фактор. Законная история и различающий отбор ещё не предъявлены; нельзя выдумать score0.90/0.50 и назвать это реальным history. Один и тот же EXAM, DEV отдельно |
| [23 N60-E1](https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/23-additions-n60-small-learning.md#L188-L218) | Admission совпавшего rejected candidate → конкретное verifier rule → Verify | Causal seam корректен, но исходные быстро/тихо fixtures не гарантируют нужный marker. Нужна source-compatible подготовка материала; подробности §3 |
| [23 N60-E2](https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/23-additions-n60-small-learning.md#L220-L230) | Proposed versioned source-support при записи + новый согласованный reader | Правильно помечено OPEN_STATE_AND_READER: такого готового контракта код ещё не обещает. Не выдавать две положительные пары за усвоенный смысл отрицания/вопроса |

Программирование в этих C-карточках — C# AST/наблюдения вызовов, перевод в T — конкретное направление и задача. «kNN», «LoRA» и «memory» — механизмы, а не предметные домены.

### Какие code-границы прочитаны непосредственно

**Structural:** [StructuralKnn.cs L26–35](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/12.SemanticCompiler/Prediction/StructuralKnn.cs#L26-L35) distance; [L42–101](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/12.SemanticCompiler/Prediction/StructuralKnn.cs#L42-L101) actual features/codec; [L117–161](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/12.SemanticCompiler/Prediction/StructuralKnn.cs#L117-L161) writer/predict. `PositionInParent` Identifier внутри ArgumentSyntax равен позиции среди детей wrapper, а не номеру Argument в ArgumentList. Роль всех `ArgumentSyntax` одинакова, [L193–200](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/12.SemanticCompiler/Prediction/StructuralKnn.cs#L193-L200).

**Context dictionary:** [ContextualDictionary.cs L13–27 и68–112](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/12.SemanticCompiler/Translation/ContextualDictionary.cs#L13-L112). Это native learned entry list и его reader; слово/контекст записываются именно здесь. Автоматический text→POS training writer этого класса не установлен.

**Translation Units:** [TranslationMachine L89–145](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/27.TranslationDomain/TranslationMachine.cs#L89-L145); [Executor Train L404–457](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/27.TranslationDomain/TranslationTrainedUnits.cs#L404-L457); [QualityVerifier Train L1095–1146](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/27.TranslationDomain/TranslationTrainedUnits.cs#L1095-L1146). Эти recipients не читают rejected как градиентный DPO-step.

**Curriculum:** [TatoebaPilotLoopRunner L403–454](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/27.TranslationDomain/TatoebaPilotLoopRunner.cs#L403-L454): promote определяется до RunCycle, max score обновляется после него, actual TrainRows вызывается при promote и после ранней проверки PassesTarget. Статическая цепь существует, конкретное различающее tiny состояние пока не измерялось.

## 3. N60-E1: не смешать admission-fix с N57 и ложным POS

На latest23 E1 предложены:

- V1 `Он быстро открыл дверь.` / `He quickly opened the door.` / `[WRONG:tense]`.
- V2 `Она тихо закрыла окно.` / `She quietly closed the window.` / `She closed the window.`.

У `быстро` и `тихо` нет подходящего current ADV-суффикса и их нет в [RuAdverbAllowList L89–95](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/27.TranslationDomain/TranslationSemanticTokenizer.cs#L89-L95). UTF8 detector [L574–585](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/27.TranslationDomain/TranslationSemanticTokenizer.cs#L574-L585) проверяет но/ко/ло/тельно/щенно/ённо/енно. В source создаются RU subject/verb tags; тогда fallback verifier допускает неизвестный token только при `LooksLikeMannerMarker`, а этот helper содержит mojibake окончания вместо нормального о: [TrainedUnits L1265–1299](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/27.TranslationDomain/TranslationTrainedUnits.cs#L1265-L1299).

По source V1 рискует не создать marker вообще. V2 имеет дополнительную ловушку: `окно` подходит грубому суффиксу но и может стать ложным ADV; marker может оказаться `окно`, а не `тихо`. Поэтому утверждение «оба точно пусты» неверно. Это статический вывод о риске материала, не результат native запуска.

Место исправления admission остаётся [builder L128–141](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L128-L141): не создавать verifier lesson, если его candidate совпал с правильным chosen и не содержит отдельного отрицательного текста. Переписывать marker helper одновременно нельзя: это был бы N57 плюс admission, два фактора.

Для подготовки E1 выбирается материал с source-supported ADV, без слов, ошибочно перехватывающих marker. В родительском пакете отдельный reviewer готовит единое дополнение `10-CURRENT-EXPERIMENT-OVERLAYS.md` с бесшумно/silently/noiselessly и громко/loudly. Здесь вторая альтернативная E1-карточка не создаётся. Факт полезности любой исправленной fixture остаётся NOT_RUN до реальных native learning calls; выигрыши false reject и потери omission detection показываются раздельно.

## 4. Verifier positive correction — только наблюдение кода

В current `QualityVerifierUnit.TrainOnExample` тело mutation достижимо только при `!chosen.Pass && chosen.Issues is not null`: [L1095–1145](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/27.TranslationDomain/TranslationTrainedUnits.cs#L1095-L1145). Явный выбранный `Pass=true` lesson сейчас не меняет ни одну из трёх rule tables.

Это позволяет поставить **условный вопрос**: если поздний teacher исправит прежний ложный fail на pass, как отменяется ошибочное требование? Однако обычный [BuildGeneralBatch L128–140](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L128-L140) сам выдаёт только chosen.Pass=false. Существование production caller, который должен учить положительной отмене, в прочитанной цепи не найдено.

Близкое prior art уже есть: [P56.2 — contradictory required terms](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/plans/replanning-possible-diffs/experimets/cards/P56-verifier-context.md#L59-L62) требует versioned scoped correction; P52/N28 разбирают ложные отрицательные targets и одноклассовую верификацию. Точную no-op ветвь можно добавить как evidence к этой теме, но **уникальная новая production-гипотеза и причинность деградации RP01 не доказаны**. Ни отдельный runtime опыт, ни implementation-fix этого интерфейса не назначены.

## 5. Что добавлено этим подаудитом

[03-T-TRAIN-CONTEXT-SMALL-EXPERIMENT.md](05-T-TRAIN-CONTEXT-SMALL-EXPERIMENT.md) с тремя JSON fixture и полным промптом представляет маленький direct-learning фактор: одинаковый input corpus, context стирается/сохраняется только при native AddEntry, query inputs неизменны. Это дополнительная локализация известной P43 темы; исходный latest T с query intervention сохраняется как самостоятельная проверка использования усвоенного состояния.

Все метрики остаются фактически неизмеренными. Правило успеха в карточке и её промпте повторено: любая measured польза своей оси/домена считается успехом без обязательного accuracy gain; регрессии показываются рядом. Качество whole Flow, отсутствие source caller или ready runtime не заменяется успешным surrogate.

