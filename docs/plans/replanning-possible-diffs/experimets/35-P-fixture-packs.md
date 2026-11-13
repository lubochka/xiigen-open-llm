# Точные компактные наборы для новых PRIMARY P-экспериментов

<!-- EXECUTOR-CONTEXT-120:START -->
## Обязательный вход перед новым аудитом или заданием

Обязателен [договор120](../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

До действий прочитать выбранную карточку целиком и её локальную секцию120, source/raw и фактический маршрут producer→ordinary caller→learner→consumer; local HEAD, published SHA и dirty bytes различаются. Списки навыков ниже — дополнительные, не освобождение от полного свежего стека120. Исторические Tier3/vocab/split/UI/Git/model правила не расширяют нынешний паспорт и авторизацию.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.

**Сохранение постановки:** exact recorded result не повторять, old NOT_RUN не выше текущей Q-строки. Полный outer/nested бюджет и независимый gold остаются у карточки; пересчёт raw/структурный guard сTrain0 не заменяет обязательный учебный цикл и не получает фиктивный Train. Команды, API и фильтры не додумывать исполнителю. Вход обязателен также в передаваемом промпте выбранного ID.
<!-- EXECUTOR-CONTEXT-120:END -->

## Дополнение 30.10.2026: разбиение по действительному входу

Для SEQ-CODE и его потребителей P46/P48/P49 недостаточно разных file IDs. В P49 четыре EXAM `(Input,Gold)` уже встречаются в TRAIN; learner получает Input без file/project. Это допустимая панель удержания известных отображений, но не model-input generalization.

В manifest fixture записать schema после преобразований и отдельно source/model-input/pair/family overlap. Conflicting inputs получают свой ceiling; constant/rule baseline и target topology сохраняются. Weighted factor не должен одновременно менять target transitions. Коррекция трактовки уже сохранённого raw выполняется без Train. Новый disjoint EXAM назначается только для отдельного оставшегося вопроса. [Проверки39](39-transfer-evidence-contract-2026-10-30.md).

## Translation DTO: точный общий ingress для P51–P67, P104, P106, P117

Если паспорт не назначает другой learner, используется native `27.TranslationDomain/TranslationMachine.cs::TrainStructuredBatchAsync` и `ExecutorUnit.TrainOnExample`, а не заполнение словаря вручную. Направление EN_RU; RU_EN — только когда оно прямо задано паспортом. UnitId = `TranslationUnitIds.ExecutorFor(direction)`; FlowId = `TranslationRoutingFlow.FlowId`; SourceId = `pNN-simulation-v1`; HeldOut=false; уникальные ExampleId `Pnn-t01` и далее в порядке таблицы. InputJson — native `ExecutorInput(source, sourceLang, topic)`, topic=`general`, если таблица не задаёт другой. ChosenOutputJson — native `ExecutorOutput(target,.95,null,phraseMappings)`, null phraseMappings по умолчанию. RejectedOutputJson — `ExecutorOutput("[WRONG: missing translation]",.1,null)`; RejectedType=`synthetic_missing_translation`. SemanticSignature = native Tokenize(source,sourceLang).Signature, кроме явно назначенных controlled-key опытов.

Каждая строка TRAIN предъявляется один раз; никакой оптимизатор/LoRA не добавляется. Before — пустая native модель; after — результат настоящего Train. Для reader A/B выучить один S0, сохранить H/shared/S0.json через ExportCheckpoint и клонировать ImportCheckpoint. Для изменения producer выполнить независимые A/B из одинакового пустого состояния. Сохранить SHA состояния, native version, список записанных owners и все отказанные строки. Admission не обходить; отказ означает INVALID_START/NO_CONTRAST, а не разрешение записать map вручную.

Обычный путь — `GetOrchestratorAsync → ExecuteAsync` с source_language, target_language, topic_domain как в P125 native harness: [Program.cs](results/2026-10-18/P125-enru-memory-01/harness/Program.cs). Этот harness — источник способа вызова, не разрешение повторить его целиком. Если паспорт назначает компонентное чтение Executor/KnnStore/Verifier, оно получает **только состояние, произведённое native Train**; назвать уровень проверки и не приписывать ему результат полного Flow. Ранний exact/phrase hit, который не дошёл до изменённого KNN, фиксируется как KNN_NOT_REACHED.

В каждой панели отдельно считать точные TRAIN-controls, новые композиции, законность action/object/polarity и отказы. Перед reader-only сравнением требуются ненулевые правильные TRAIN-controls. Одна тема и одно направление на самостоятельный опыт. Результат 0→0 не превращается в учебный успех из-за изменения bytes/времени.

## Переиспользуемый квалифицированный переводческий S0

Для P59 и прямого исторического readback P125 назначен уже существующий:
`results/2026-10-18/P125-enru-memory-01/states/B_trained.json`,
SHA256 `125c15ddb400dc00cfddb2367e1ea811917a87fb641c4e58ade22da2bea2e56c`.
Соседние `train.json`, `eval-inputs.json`, `panel-gold.json` и `harness/Program.cs` задают точные 8 уроков и 24 запроса. P25 доказал cold parity на этом артефакте; это историческое свидетельство, не запуск текущей документной работы. При несовместимости import не заменять его новой случайной моделью: записать точный контрактный отказ и перейти по индексу.



## R-NATIVE: обученная маршрутизация native Domain

Тема — оркестрация задач по C# коду. Native Domain из проекта29; это controlled classifier fixture, не свободная семантика строк.
TRAIN tuples: ([10,11],CODE_Q),([10,12],CODE_Q),([20,21],PLAN),([20,22],PLAN),([30,31],BUG),([30,32],BUG).
EXAM tuples/text для сопоставления overload:
([10,11],"explain method")→CODE_Q;
([10,12],"describe return")→CODE_Q;
([20,21],"plan implementation")→PLAN;
([20,22],"build workflow")→PLAN;
([30,31],"fix null error")→BUG;
([30,32],"debug exception")→BUG.
IDs локального controlled fixture не выдаются за output Roslyn. Перестановка labels TRAIN на один класс — отдельный diagnostic state с учтёнными6lessons, если назначен. Native keyword overload может совпасть с gold без обучения: проверка зависимости от train-state нужна отдельно.

## E22-CODE: AST-производитель и шесть native buckets

Native DatasetNames = Generic,CSharp,TypeScript,BugFix,Orchestration,Tests. В P34 меняется **только BugFix label producer**; остальные пять битов фиксируются исходным AutoLabel и не используются как независимый gold для этой гипотезы.
TRAIN:
f1 `class A { void M(){ throw new Exception(); } }` → BugFix=true;
f2 `class B { void M(){ var s = "throw new Exception();"; } }` → false;
f3 `class C { void M(){ try { F(); } catch(Exception) {} } }` → true;
f4 `class D { /* catch (Exception) */ void M(){} }` → false.
AST criterion выбранного эксперимента: ThrowStatementSyntax или CatchClauseSyntax в actual syntax nodes; тексты строк и комментариев не являются такими узлами. Не расширять этот criterion на все способы обнаружения bug.
EXAM —те же четыре структурных семейства с class Q1/Q2/Q3/Q4 и method Check, иными whitespace/trivia; gold[true,false,true,false]. Это проверка переименования/контекста, не доказательство unseen-structure generalization. Predicate IDs получить одним неизменным LogicExtractor на обеих сторонах; filename не feature/gold. Constant Bool≤2/4.



Дата:27 октября2026. Это **данные постановки в Markdown**, не созданные runtime datasets. Future host материализует только назначенный набор внутри собственного H. ID набора не доказывает существование checkpoint. TRAIN и EXAM разделены; исполняющий агент не добавляет строки и не меняет gold для получения PASS. Контракт исполнения — [34](34-P-experiment-execution-contract.md).

## U-OR: память порядка действий одной темы оркестрации

Typed output содержит `route` и `steps`; для эксперимента мутабельности это class с изменяемым списком, иначе независимые immutable значения. Все запросы — оркестрация работы с кодом, не обучение программированию/переводу.

| ID | TRAIN input | route | steps |
|---|---|---|---|
| u1 | review code then implement change | review-first | review,implement |
| u2 | inspect code then implement change | review-first | review,implement |
| u3 | review module before editing it | review-first | review,implement |
| u4 | inspect module before editing it | review-first | review,implement |
| u5 | implement change then review code | build-first | implement,review |
| u6 | implement change then inspect code | build-first | implement,review |
| u7 | edit module before reviewing it | build-first | implement,review |
| u8 | edit module before inspecting it | build-first | implement,review |

EXAM: e1 `review code before implementing change`→review-first; e2 `inspect code before implementing change`→review-first; e3 `implement change before reviewing code`→build-first; e4 `implement change before inspecting code`→build-first. Контроль сохранности памяти: все8 TRAIN inputs с их исходным gold. Constant-label даёт не больше2/4 EXAM. Отдельный negative input `translate a greeting` не входит в обучение и должен быть no-match только в карточках, назначающих refusal; нельзя требовать этот отказ от алгоритма без reject-контракта.

Один memory fit —8 реальных lessons через указанный native writer. Если underlying Train заменяет весь corpus, это один batch из8, не8 вызовов replace. S0 сохраняется после этого fit вместе с config/tokenizer. Обычный readout —8 control +4 EXAM =12 queries. Cold before использует те же12 входов: для native KnnLogicalUnit это12 наблюдений NO_TRAINED_UNIT до регистрации, без невозможного Train(empty) и без retrieval. Реальный baseline Flow используется только там, где он прямо назначен карточкой. Cold observation, native query и ответ baseline Flow считать отдельно. Для representation A/B каждый вариант обучает свои8lesson states, всего16lesson presentations; для reader-only S0 обучается один раз8lessons, оба плеча получают независимое восстановление. Дополнительные негативные cases и изменения данных указаны индивидуально.

## R-CODE: один домен, три класса намерения работы с C# кодом

Native router/topic = оркестрация задач по C#, labels `explain`, `implement`, `test`. Это три класса одной задачи, а не смесь обучения разных специалистов.

TRAIN: r1 `explain this C# method`→explain; r2 `describe the C# return value`→explain; r3 `implement this C# method`→implement; r4 `write a C# null guard`→implement; r5 `test this C# method`→test; r6 `add C# null guard tests`→test.
EXAM: q1 `explain the C# null guard`→explain; q2 `describe this C# method`→explain; q3 `implement a C# null guard`→implement; q4 `write this C# method`→implement; q5 `test the C# null guard`→test; q6 `add tests for this C# method`→test. Constant class максимум2/6. Пустой запрос и `translate hello` —negative controls, без добавления в TRAIN.

Для int[] router/fixed predicate router нельзя выдать строки как его родной вход: experimental fixture adapter использует заданную карту `explain|describe→0`, `implement|write→1`, `test|tests→2`, `C#→3`, `method→4`, `null|guard→5`. Карта одинакова во всех плечах, не читает gold. В опыте ordinary caller adapter допустим только как fixture для соответствующего native overload; проверка продуктового parser идёт отдельной строкой с его фактическим output. Не выдавать этот контролируемый mapping за научившийся natural-language router. Actual ordinary overload/consumer записать отдельно.

## M-CODE: MethodKNN и AST на C#

TRAIN состоит из двух файлов, каждый даёт четыре file-local соседних пары. Полный текст:
```csharp
public class LoopSeed {
  int Start() { return 1; }
  void A() { for (int i=0; i<2; i++) {} }
  void B() { for (int i=0; i<3; i++) {} }
  void C() { for (int i=0; i<4; i++) {} }
  void D() { for (int i=0; i<5; i++) {} }
}
```
```csharp
public class GuardSeed {
  int Start() { return 1; }
  void A(int x) { if (x<0) return; }
  void B(int x) { if (x<0) return; }
  void C(int x) { if (x<0) return; }
  void D(int x) { if (x<0) return; }
}
```
На этом корпусе не заявляется универсальное различение по одному и тому же Start: next у Start конфликтует. EXAM использует методы с различающимися bodies:
`public class Q1 { void M(){for(int j=0;j<7;j++) {}} }`→Loop;
`public class Q2 { void M(){for(int j=1;j<9;j++) {}} }`→Loop;
`public class Q3 { void M(int n){if(n<0)return;} }`→Guard;
`public class Q4 { void M(int n){if(n<1)return;} }`→Guard.
k=1 для bounded MethodKNN профиля, ties прежние. Constant-label максимум2/4. Контроли —те же bodies с иными именами class/method. AST kind/offset карточки используют независимый Roslyn token/node gold, не эти MethodKNN labels. Изменённые lambda/expression-body данные назначаются соответствующим паспортом отдельно.

## SEQ-CODE: реальные переходы с file/project boundaries

Одна задача —предсказание следующего предиката C# метода. Experimental typed vocab:0=NULL_CHECK,1=RETURN,2=LOAD,3=SAVE,4=CALL,5=AWAIT; числа —локальный codec набора, не обещание равенства product enum IDs.

TRAIN files:
f1/A:[0,1,0,1]; f2/A:[2,3,2,3]; f3/B:[0,3,0,3]; f4/B:[4,5,4,5].
Каждая запись содержит projectId и fileId; transitions только соседние внутри файла, всего12pairs. TEST source-disjoint files:
t1/A:[0,1]; t2/A:[2,3]; t3/B:[0,3]; t4/B:[4,5].
Gold queries:(A,0)→1;(A,2)→3;(B,0)→3;(B,4)→5.
Без domain conditioning общий0 имеет два допустимых TRAIN target; это намеренный конфликт. Fixed order f1,f2,f3,f4; tie first-wins, если native owner не требует другого явно записанного правила.
Для file-boundary опыта дополнительный pair1→2 между f1/f2 не является законом; контроль одного файла f1 содержит3lawful transitions.
Для Probe variant использовать one-hot6 input, one-hot projectA/B/UNKNOWN при назначенном conditioning; общийshape одинаковыйA/B. Если native Probe fixed22 inputs, заполнить оставшиеся координаты нулями. Seed42; скрытый32; lr0.05;200epochs×12pairs=2400updates на плечо. Это назначенный конечный budget, не доказательство усвоения до запуска. Gold4queries +12TRAIN-control. Warm preparation, если нужна, имеет отдельный бюджет паспорта и сохраняется один раз.

## T-ENRU: обучаемая переводческая память

Тема —перевод EN→RU, только одна направленная память. TRAIN:
t1 `Anna opened the door`→`Анна открыла дверь`;
t2 `Anna closed the window`→`Анна закрыла окно`;
t3 `Boris opened the window`→`Борис открыл окно`;
t4 `Boris closed the door`→`Борис закрыл дверь`;
t5 `Anna did not open the door`→`Анна не открыла дверь`;
t6 `Anna did not close the window`→`Анна не закрыла окно`;
t7 `Boris did not open the window`→`Борис не открыл окно`;
t8 `Boris did not close the door`→`Борис не закрыл дверь`.

EXAM:
e1 `Anna opened the window`→`Анна открыла окно`;
e2 `Anna closed the door`→`Анна закрыла дверь`;
e3 `Boris opened the door`→`Борис открыл дверь`;
e4 `Boris closed the window`→`Борис закрыл окно`;
e5 `Anna did not open the window`→`Анна не открыла окно`;
e6 `Anna did not close the door`→`Анна не закрыла дверь`;
e7 `Boris did not open the door`→`Борис не открыл дверь`;
e8 `Boris did not close the window`→`Борис не закрыл окно`.

Все8 TRAIN —также отдельная retention panel. Ни один единственный постоянный ответ не покрывает более1/8 EXAM. Exact text и смысловые slots subject/action/object/polarity измеряются отдельно. Wrong target language —не правильный перевод. Reroute/normalization/case/Unicode variants задаются карточкой, не добавляются к TRAIN. Не утверждать, что generic word map уже умеет морфологию: actual errors сохраняются, set не подгоняется. Existing qualified historical translation fixture имеет приоритет только если паспорт прямо назначает его вместо T-ENRU.

## FORMULA: четыре формулы одной предметной темы

TRAIN exact mappings:
`Revenue - Cost`→`Profit`;
`Assets - Liabilities`→`Equity`;
`Price * Quantity`→`Revenue`;
`Profit / Revenue`→`Margin`.
EXAM formatting variants: `Revenue-Cost`→Profit; `Assets-Liabilities`→Equity; `Price*Quantity`→Revenue; `Profit/Revenue`→Margin.
Unknown `Revenue+Cost` и `Price/Quantity` должны быть no-match, когда назначен reject contract. Это retrieval обучения формул, не алгебраический вывод. Shuffle control циклически сдвигает labels на1; он отдельный диагностический state и его4lessons учитываются явно, если назначены.

## DOC-FACT: факты одного проекта и чтение обученной памяти

Один домен —документация проекта Cedar. TRAIN4facts:
`Cedar owner`→`Dana`;
`Cedar status`→`approved`;
`Cedar release`→`Friday`;
`Cedar language`→`C#`.
EXAM4questions: `Who owns Cedar?`→Dana; `What is Cedar status?`→approved; `When is Cedar released?`→Friday; `What language does Cedar use?`→C#.
Gold training questions/aliases к facts доступны только training materializer, не reader scoring. Не подставлять answers в output supplier. Для восстановленного exact-memory proof допустим TRAIN readback4/4; EXAM4heldout оцениваются отдельно, а lookup не называется свободной генерацией. Смена status наcancelled должна быть отдельным новым уроком и не стирать три других facts.
