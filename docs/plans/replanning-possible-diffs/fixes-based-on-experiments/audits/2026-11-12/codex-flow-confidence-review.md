# Уверенность выбранного flow подтверждает сосед другого flow

## Вердикт и предел доказательства

**Подтверждён отдельный механизм смешения владельцев свидетельства в `TryGround`: к оценке выбранного FLOW-01 добавляется положительный сосед FLOW-02, хотя у FLOW-01 сходство с исходным запросом равно нулю.** Контрпример доведён по исходникам до обычного `OrchestrationPlanMachine.GeneratePlanAsync`, включая настоящий классификатор и composer. Вручную переданный `HumanIntentContract` для этого не нужен.

Это **статически выведенный достижимый путь и выполненный арифметический пересчёт**, а не выполненный C# тест. .NET SDK отсутствует; обучение, GeneratePlanAsync, сохранение/загрузка и исполнение плана в этой проверке не запускались. Число 0,966667 — результат формулы продукта, не измеренная точность и не вероятность правильности.

Проверен core `dcb931773147267c8864a532351d22b4c351265b`. Шесть исходников в [пересчёте](codex-flow-confidence-recompute.py) проверены по Git blob SHA до и после расчёта. [JSON](codex-flow-confidence-recomputed.json) содержит девять аналитических случаев, полные входы, токены, оценки и явный `native_runtime_executed=false`. Карточка48 отдельно сверена с blob `287143d663d1c8bdf9e4c984836ef14b24f65217` того же pin.

**Новизна ограничена.** В [доске B12:285–297](https://github.com/lubochka/llm_mvp_core/blob/dcb931773147267c8864a532351d22b4c351265b/docs/plans/replanning-possible-diffs/claude-audit-board-2026-11-11.md#L285-L297) уже запланирована отрицательная пара другого маршрута; её основной разбор — empty/empty, zero overlap и порядок PairId. В просмотренных аудите11ноября, repair-plan, followup/addendum, доске B12 и карточке48 не найден именно этот численный контрпример: **чужой положительный сосед меняет допуск выбранного flow до чтения тела**. Это новая конкретизация известной соседней границы B12, не утверждение о полной уникальности во всей истории репозитория.

Прежний FIX-IM правилен: найденное тело принадлежит выбранному маршруту. Эта проверка не отменяет его приёмку и не переоткрывает опыт IDENTITY-M1-01. Карточка48:125 прямо требовала сохранять глобальный поиск для TryGround — значит, нужно явно уточнить план, а не объявить добросовестное исполнение старого узкого ремонта ошибкой.

## Источники и реальная цепочка

Все ссылки ниже закреплены на одном source SHA.

| Звено | Что установлено в исходнике |
|---|---|
| [HumanIntentBoundaryClassifier:12–32,48–92,111–117](https://github.com/lubochka/llm_mvp_core/blob/dcb931773147267c8864a532351d22b4c351265b/42.ProjectWikiDomain/HumanIntentBoundaryClassifier.cs#L12-L117) | `FLOW-01` распознаётся как span и нормализуется в AllowedFlowScope. Ни одна ранняя особая ветвь для выбранного prompt не срабатывает. |
| [TrainOrchestrationBatchAsync:195–205](https://github.com/lubochka/llm_mvp_core/blob/dcb931773147267c8864a532351d22b4c351265b/42.ProjectWikiDomain/OrchestrationPlanMachine.cs#L195-L205), [PromptFlowGroundingModel.Train:17–62](https://github.com/lubochka/llm_mvp_core/blob/dcb931773147267c8864a532351d22b4c351265b/42.ProjectWikiDomain/PromptFlowGroundingModel.cs#L17-L62) | Два обычных Mode1 урока с разными PairId/FlowRef попадают в KNN и веса. Не нужны импорт поддельных weights, повреждённый snapshot или смена редакции цели. |
| [BuildWeights/Query:254–328](https://github.com/lubochka/llm_mvp_core/blob/dcb931773147267c8864a532351d22b4c351265b/42.ProjectWikiDomain/PromptFlowGroundingModel.cs#L254-L328), [Tokenize/Jaccard:62–78](https://github.com/lubochka/llm_mvp_core/blob/dcb931773147267c8864a532351d22b4c351265b/42.ProjectWikiDomain/OrchestrationKnnStore.cs#L62-L78) | Prompt даёт входные признаки, FlowRef — scope-признаки; токены уникальны. Query содержит prompt, allowed scopes, requested spans, constraints. |
| [TryGround:70–148](https://github.com/lubochka/llm_mvp_core/blob/dcb931773147267c8864a532351d22b4c351265b/42.ProjectWikiDomain/PromptFlowGroundingModel.cs#L70-L148) | Выбор по input+scope; при равенстве выигрывает больший scope. Затем двухаргументный поиск на строке115 проверяет соседа **без FlowRef**. |
| [GeneratePlanAsync:73–137](https://github.com/lubochka/llm_mvp_core/blob/dcb931773147267c8864a532351d22b4c351265b/42.ProjectWikiDomain/OrchestrationPlanMachine.cs#L73-L137), [AllowsLearnedGrounding:490–498](https://github.com/lubochka/llm_mvp_core/blob/dcb931773147267c8864a532351d22b4c351265b/42.ProjectWikiDomain/OrchestrationPlanMachine.cs#L490-L498) | Confidence не ниже0,75 и разрешённый выбранный FlowRef пропускают learned branch. Владелец соседа в этой проверке не участвует. |
| [FindLearnedPair:154–168](https://github.com/lubochka/llm_mvp_core/blob/dcb931773147267c8864a532351d22b4c351265b/42.ProjectWikiDomain/PromptFlowGroundingModel.cs#L154-L168), [composer:10–35,111–116](https://github.com/lubochka/llm_mvp_core/blob/dcb931773147267c8864a532351d22b4c351265b/42.ProjectWikiDomain/TrainedRouteComposerUnit.cs#L10-L116) | Тело уже правильно ограничено выбранным FlowRef. Composer выводит его title и полный PlanText. Это и есть наблюдаемый выход этой задачи; выполнение шагов внешним исполнителем не доказано. |

## Контрпример, дошедший до обычного GeneratePlanAsync

Это предлагаемая малая синтетическая fixture, а не найденные исторические TRAIN-строки. У каждой пары `Mode=Mode1LearnedFlowGrounding`, `Chosen.FlowRef=FlowRef`, отдельный immutable PairId, `IsHeldOut=false`; chosen title/body различимы. Expected-поля заполняются по договору пары, не подаются в запрос. По одному уроку на flow; конфликтов редакций нет.

| Пара | PairId | FlowRef | Prompt | Уникальный маркер PlanText |
|---|---|---|---|---|
| A | witness-a | FLOW-01 | alpha | ALPHA_BODY_ONLY |
| B | witness-b | FLOW-02 | beta | BETA_BODY_ONLY |

Запрос: **`beta FLOW-01`**. Исходный вариант `route-a` из гипотезы заменён на распознаваемое имя FLOW-01 именно для проверки фасада. Классификатор даёт:

~~~json
{
  "Kind": "KnownFlowPlanning",
  "AllowedFlowScope": ["FLOW-01"],
  "ForbiddenFlowScope": [],
  "PhaseScope": [],
  "RequiresApprovalGate": false,
  "RequiresSaveState": true,
  "HumanConstraints": [],
  "RequestedFlowTextSpans": ["FLOW-01"]
}
~~~

BuildQueryText создаёт `beta FLOW-01 FLOW-01 FLOW-01`. Повторы не увеличивают веса: Tokenize возвращает множество **{beta, flow, 01}**. `FLOW-01` даёт scope-токены {flow,01}, `FLOW-02` — {flow,02}.

| Кандидат | Совпавшие input-токены | InputScore | ScopeScore | Сумма |
|---|---|---:|---:|---:|
| A / FLOW-01 | нет | 0 | 2 | 2 |
| B / FLOW-02 | beta | 1 | 1 | 2 |

При равенстве суммы A выигрывает по ScopeScore2>1. Это не зависимость от порядка добавления или PairId: обратный порядок B,A в пересчёте даёт тот же результат.

**Состояние только A.** Jaccard({beta,flow,01},{alpha})=0. Глобальный ближайший сосед — A со значением0. TryGround возвращает null; GeneratePlanAsync переходит в Mode2, где Mode2-store в этой fixture пуст. Выученное тело A не выводится.

**Состояние A+B.** A и его собственные признаки не изменились. Глобальный сосед теперь B:
Jaccard({beta,flow,01},{beta})=1/3. Подстановка в [действующую формулу:330–346](https://github.com/lubochka/llm_mvp_core/blob/dcb931773147267c8864a532351d22b4c351265b/42.ProjectWikiDomain/PromptFlowGroundingModel.cs#L330-L346):

~~~text
min(1, 0.70 + 2 × 0.10 + (1/3) × 0.20)
= 29/30
≈ 0.9666666666666666
~~~

Проходят оба существующих порога:0,50 в TryGround и0,75 у фасада. AllowedFlowScope содержит A, поэтому AllowsLearnedGrounding разрешает A, хотя B в этот scope не входит.

После этого BuildRuntimeEvidenceFeatures(A) даёт {01,alpha,flow}. Для body lookup добавляется alpha; его query становится {01,alpha,beta,flow}, Jaccard с A=1/4. FindLearnedPair уже ограничен FLOW-01 и возвращает A. Composer печатает `flow_ref=FLOW-01`, confidence округлённо0,967 и **ALPHA_BODY_ONLY**. Ни чужое тело B, ни нарушение FIX-IM для контрпримера не нужны. Дополнительное сходство body lookup не является независимым подтверждением запроса: alpha попало в него из выбранных weights.

| Аналитический контроль | Текущая формула/вызов | Предлагаемый flow-scoped witness |
|---|---|---|
| Только A; beta FLOW-01 | Mode2, learned body отсутствует | тот же результат |
| A+B; beta FLOW-01 | LearnedFlowGrounding A, confidence29/30, bodyA | Mode2, learned body отсутствует |
| A+B; alpha FLOW-01 | LearnedFlowGrounding A, confidence1 | тот же результат |
| A+B; beta FLOW-02 | LearnedFlowGrounding B, confidence1 | тот же результат |

Эти четыре строки — расчёт по исходникам. Нельзя записать их в native receipt как состоявшийся запуск.

## Как усилить карточку и что именно ремонтировать

**Предлагаемый инвариант:** после выбора f оценка соседа, принимаемая как положительное свидетельство этого выбора, должна получаться среди Mode1-пар того же f. Добавление урока другого flow не должно превращать нулевое собственное свидетельство f в положительное. Владелец candidate, confidence witness и body должны быть наблюдаемы раздельно.

Минимальная форма ремонта — использовать уже существующий overload в TryGround:

~~~csharp
var nearest = _knn.FindNearestWithScore(
    queryText,
    PairMode.Mode1LearnedFlowGrounding,
    best.FlowRef);
~~~

Это предложение, не сделанное изменение. Сохранить candidate ranking, построение query, конечность чисел, условие nearest.Score>0, текущую формулу и оба threshold. Не добавлять flow или gold-body в исходный запрос. Не возвращаться к глобальному соседу, когда свой отсутствует/нулевой. Уже реализованный FlowRef filter в FindLearnedPair сохраняется.

В [карточке48:125](https://github.com/lubochka/llm_mvp_core/blob/dcb931773147267c8864a532351d22b4c351265b/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/48-learned-route-body-identity.md#L125) датированно уточнить, что сохранение global search было границей прежнего body-only FIX-IM; новый отдельный контроль закрывает доверие к чужому соседу. В B12 указать данный обычный witness, чтобы его не заменили одним empty/empty тестом. В карточке15 сохранить границу: это не исправление active revision; в fixture нет смены chosen одного flow. Будущий revision-ремонт должен отдельно решить, входит ли confidence witness в membership active revision.

Такой ремонт не доказывает семантической релевантности каждого same-flow урока и не калибрует confidence. Scope-токены по-прежнему участвуют в query и score. Он закрывает конкретное смешение FlowRef, не всю задачу качества grounding.

## Предложенная native приёмка — UNRUN

Новый тест должен вызывать публичные `TrainOrchestrationBatchAsync` и `GeneratePlanAsync`, не подставлять contract и не редактировать weights/checkpoint вручную. Зафиксировать до исполнения буквальные A/B, source+assembly SHA, положительный/отрицательный oracle и полный расход. `ExpectedIntentKind` для учебных prompt alpha/beta — ImplementationExecutionRequest; для query с FLOW-01 классификация ожидается KnownFlowPlanning. Тело и заголовок задаются непосредственно в fixture, expected не получается через candidate SerializeChosen.

Один небольшой пакет можно выполнить с **2 одноэлементными batches / 2 пары / 2 вызова PromptFlowGroundingModel.Train**, **17 GeneratePlanAsync**, **2 SaveCheckpointAsync**, **5 LoadCheckpointAsync**:

| Стадия | Состояние/операция | GeneratePlanAsync |
|---|---|---:|
| Исходный код, подготовка A | Train A; отрицательный query и positive A; сохранить S-A | 2 |
| Исходный код, подготовка A+B | Train B; отрицательный query, positive A, positive B; сохранить S-AB | 3 |
| Candidate | Загрузить S-A; negative и positive A | 2 |
| Candidate | Загрузить S-AB; negative, positive A/B | 3 |
| Отдельный свежий candidate | Загрузить S-AB в новый объект; те же3query | 3 |
| Обратная мутация | Вернуть только двухаргументный global lookup; загрузить S-AB; negative обязан покраснеть | 1 |
| Восстановленный candidate | Загрузить S-AB; negative и positive A/B | 3 |
| **Итого** | **Учебные состояния подготавливаются единожды; после load Train=0** | **17** |

На исходном коде отрицательный запрос A+B должен нарушить независимый oracle. Поэтому сначала собрать actual outputs всех трёх запросов этой стадии, сохранить S-AB и её receipt, и только затем предъявить различающее failing assertion либо завершить baseline с явно записанным ожидаемым FAIL. Если assertion прервёт стадию до сохранения, дальнейшие candidate loads невозможны и пакет остаётся INCOMPLETE. Этот порядок не меняет oracle и не превращает наблюдённый baseline FAIL в PASS; он сохраняет материал для уже назначенного сравнения без повторного Train.

До/после сравнить RouteMode, FlowRef, confidence, полный title/body и warnings. Отрицательный oracle — прежний Mode2 без ALPHA_BODY_ONLY/BETA_BODY_ONLY; положительные — собственный полный learned payload. Записать actual candidate score и владельца witness диагностикой теста, не добавлять public production API ради отчёта. Отсутствие механизма наблюдения не оправдывает выдуманные runtime поля: тогда отдельно оставить доказанную исходником причину и реальные публичные результаты.

Это адресная проверка связи памяти и чтения, не новая neural acquisition программа. BPE/Grammar/teacher/neural Train в перечисленных исходниках этого пути нет. Расход существующих regression-классов раскрывается отдельно; он не скрывается внутри17. Если понадобится дополнительный порядок уроков или case другого scope, назначить отдельные calls до запуска. Сохранённые доказательства не перезаписываются.

## Воспроизведение выполненного пересчёта

~~~bash
python docs/plans/replanning-possible-diffs/fixes-based-on-experiments/audits/2026-11-12/codex-flow-confidence-recompute.py \
  --core . \
  --output /tmp/codex-flow-confidence-new.json
~~~

Запускать из core checkout указанного pin; output должен быть новым. Скрипт отказал бы при несовпадении любого из шести source blobs и не перезаписывает существующий output. В текущей проверке завершился успешно:6проверенных source blobs,9аналитических случаев, native calls0. Использует только ASCII-часть токенизатора; C# regex классификатора читается из pinned файла и вычисляется Python только на этих простых ASCII-входах. Это осознанная граница переносимого расчёта, а не .NET-эквивалентность всей реализации.

