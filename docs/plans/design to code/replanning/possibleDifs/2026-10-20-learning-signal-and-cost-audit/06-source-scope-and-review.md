# Источники, навыки и пределы доказательства

Машиночитаемые pins, статусы и результаты проверки источников — [06-source-manifest.json](06-source-manifest.json). Новый материал опубликован только в planning/possibleDifs. Защищённый core каталог нами не записывался; его внешние изменения явно отражены ревизиями.

| Срез | Проверка |
|---|---|
| Planning 1ad622fe |869 файлов в метаданных;555 Markdown скачаны и адресно проверены на новизну. Это не полное смысловое чтение 555 документов |
| Core bf9b543e, experimets |156 Markdown вне результатов;125 P-карточек структурно,470 подслучаев по якорям;179 адресных строк active crosswalk |
| Core f5dd5320 |152 изменённых Markdown скачаны отдельно; diff/структура/поиск и содержательный review ключевых поздних разделов |
| Core 62bcc8f2 |145 изменённых Markdown в compare, только docs; targeted read новой N-KU и изменённой 23, без заявления полного reread145 |
| Planning fb9aadf6 |24 изменённых Markdown из двух параллельных пакетов и навигации; novelty search и адресный join новых рецептов |
| Core7c5718 |148 changed Markdown после62bcc, runtime source без изменений;6 адресных snapshots hash-verified, новые N61 и N-KU preregistration разобраны |
| Planning64aa3c75 |61 changed paths,26 Markdown получены и проверены поиском; LF-02 совпала с OCT20-R2, заявление самостоятельной новизны снято |
| Исторические ветки |Первичные E09/E25/E43 результаты и код; V309 report/runner; RP01, P125/P25/ingress receipts. Не полный аудит всех ветвей |

Сохранённые snapshots сверены с Git blob hashes. Захват через инструмент мог добавить один конечный LF; проверка допускает удаление **только этого одного LF в памяти**, не переписывает protected источник. Для четырёх документов захват также нормализовал CRLF: исходные bytes повторно получены по pinned ref и точно совпали с Git blob SHA; содержимое снимков совпадает после явной нормализации окончаний строк. [Адресная проверка](inventory/line-ending-recheck.json). Полный список проверок и исключений — [source-integrity-and-counts.json](inventory/source-integrity-and-counts.json). Базовые corpora перечислены в [planning CSV](inventory/planning-novelty-corpus.csv) и [core CSV](inventory/protected-baseline-corpus.csv).

## Применение запрошенных навыков

Из docs/skills прочитаны и применены релевантные разделы следующих файлов; длинные материалы не объявляются целиком заново прочитанными:

- [architect-behavior-classifier-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/skills/architect-behavior-classifier-SKILL.md)
- [planning-session-startup-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/skills/planning-session-startup-SKILL.md)
- [plan-review-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/skills/plan-review-SKILL.md)
- [code-examination-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/skills/code-examination-SKILL.md)
- [generated-code-review-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/skills/generated-code-review-SKILL.md)
- [prompt-patch-authoring-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/skills/prompt-patch-authoring-SKILL.md)
- [human-readable-architecture-audit-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/skills/human-readable-architecture-audit-SKILL.md)
- [human-communication-GLOSSARY.md](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/skills/human-communication-GLOSSARY.md)
- [learning-loop-closure-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/skills/learning-loop-closure-SKILL.md)
- [experiment-design-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/skills/experiment-design-SKILL.md)
- [improvement-measurement-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/skills/improvement-measurement-SKILL.md)
- [learning-path-audit-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/skills/learning-path-audit-SKILL.md)
- [output-readability-gate-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/skills/output-readability-gate-SKILL.md)

Архитектурная проверка различает real learner, deterministic helper и consumer. Планирование сохраняет одного владельца изменения, actual dependencies и малый бюджет. Проверка плана/кода прослеживает исходный сигнал до state/consumer, в том числе недостижимую E1 ветвь. Промпты содержат один фактор, точный original fix и единый критерий успеха. Человеческая коммуникация отделяет известное, гипотезу, статический прогноз и измерение.

Прямые указания пользователя имеют приоритет над старыми skill-требованиями обязательного accuracy gain, Tier escalation, повторного личного одобрения или несвязанной test/build матрицы. Сами skills не редактировались. Заимствованные поздние решения других исполнителей отмечены ссылками в 07.

## Независимая проверка

[Source review LF-02/LF-03/E1](reviews/independent-source-review.md) независимо проверил две allocation-цепочки, исключил альтернативный concept parser E1 и проверил предложенные ADV fixtures. После замечания Step metrics помечены как pre-update; дополнительный objective readout не скрыт в бюджете. Последний core delta проверен другим адресным проходом. Заключительный независимый consistency review README,03,07 и четырёх полных LF-prompts не обнаружил существенных противоречий с62bcc-policy, обязательного six-row gate или вымышленных измерений. Исторические JSON здесь анализировались как evidence; наши таблицы не выдаются за новое исполнение продукта.

Главное ограничение: источник подтверждает, что механизм существует и чем его можно изменить, но без actual native A/B нельзя обещать измеренный рост качества или скорость. Это не обесценивает сохранённые исторические успехи. Неустановленная единственная причина RP01 и точные отдельные source gaps могут одновременно оставаться честными выводами.

Перед записью независимый reviewer отдельно сопоставил OCT20-R2, optimizer Step-read и positive-feedback candidate нового пакета62. Подтверждены полное совпадение LF-02 с R2, иной механизм LF-03 и отдельная конкретная недостижимость LF-04. Это уточнение supersedes прежний отрицательный novelty search по LF-02.
