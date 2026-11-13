# P29 parent review - non-training SkillSelection gate design

Вердикт: P29 принят как non-training gate design. Training, eval execution, checkpoint mutation и promotion по-прежнему не авторизованы.

Что проверено:

- P29 использовал 8 строк P27 только как local-private candidate rows;
- P29 честно указал, что 8 строк достаточно только для gate/design, но недостаточно для meaningful candidate-only training/eval;
- source-disjoint split design разложен по `source_family`, `pair_id`, `source_hash`, `targetAdapter`, selected skill и prompt-summary features;
- authorization matrix явно блокирует training до fresh Luba approval, отдельного P30 work order, state flags, parent-accepted split/batch card и leakage/lifecycle gates;
- leakage plan запрещает exact prompt, source id, path, filename, pair id, row ordinal, candidate order, selected-skill position и target adapter shortcut;
- P29 не запускал training, eval execution, dotnet test/build, checkpoint mutation, promotion, common replacement или external AI/network;
- parent repair поправил две Markdown-опечатки, где tab ломал `targetAdapter` и `trainingAuthorizedNow`.

Почему это принимается:

P29 не делает вид, что маленький набор из 8 строк уже пригоден для качества. Он закрывает только gate-design слой и показывает, почему следующий безопасный шаг должен расширить/подготовить source-disjoint material, а не обучать модель.

Что еще не закрыто:

P29 не разрешает training. Перед training нужны дополнительные source-disjoint candidate rows или явно помеченный mechanics-smoke без semantic-quality claim, плюс отдельная авторизация.

Следующее действие:

P30 должен быть safe non-training slice: найти/подготовить дополнительные source-disjoint SkillSelection repair candidates из разрешенных локальных skill/guide sources и обновленных skills, с license/local-private/quarantine ledger, без training, eval execution или checkpoint mutation.
