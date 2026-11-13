---
title: "185. Fixed-position experiment: что показывают channel ablations и learned gates"
source_pdf_pages: [530, 533]
book_pages: [494, 497]
batch: "011"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 185. Fixed-position experiment: что показывают channel ablations и learned gates

[Индекс](../README.md) · [Источник и редакционная политика](../SOURCE.md)

## Задача и контролируемый фактор

[§14.11](../source/0530-0534-ch14-channel-interventions-and-toy-illustrations.md#pdf-0530) сравнивает стандартный и rationalised encoder на фиксированном layout $[\mathrm{CLS},M_1,a,M_2,b,\ldots]$. Labels — равенство остатков по модулю $4$ и отношение $a<b$. Фиксированные позиции уменьшают неопределённость поиска аргументов, но не делают вычисление labels доказательством необходимости конкретного attention-сектора.

Автор сообщает одинаковые width/layers/heads и **явный weak channel-alignment bias**: S-gate поощряется для symmetric task, A-gate — для oriented. Поэтому эксперимент проверяет управляемую организацию каналов при таком bias; он не показывает самопроизвольное возникновение той же специализации. Размеры модели сами по себе не свидетельствуют о равных параметрах, FLOPs или стоимости диагностики.

## Числа основного сравнения

[Table 14.7](../source/0530-0534-ch14-channel-interventions-and-toy-illustrations.md#table-14-7) сообщает mean ± standard deviation по трём seeds. У обеих архитектур accuracy округлена до $1.000\pm0.000$. Среднее residual снижается с $0.310$ до $0.268$ на symmetric task и с $0.234$ до $0.132$ на oriented, а coverage растёт с $0.127$ до $0.170$ и с $0.110$ до $0.163$ соответственно. Это описательное сравнение приведённых средних. Оно не устанавливает статистическую значимость или улучшение на каждом seed без исходных парных результатов.

[Figure 14.2](../source/0530-0534-ch14-channel-interventions-and-toy-illustrations.md#figure-14-2) показывает меньший residual rationalised варианта, особенно oriented, в течение приведённого обучения. Это reported trace, а не воспроизведённый нами запуск. Значения loss, округлённые до нуля, не нужно интерпретировать как доказанное точное нулевое математическое значение.

## Интервенции действительно различают каналы

| Сравнение из Table 14.8 | Accuracy | Что непосредственно следует |
|---|---:|---|
| symmetric: full → no-S | 1.000 → 0.705 | В этой обученной конфигурации отключение S сильно ухудшает результат. |
| symmetric: full → no-A | 1.000 → 1.000 | Отдельное отключение A не меняет округлённую accuracy. |
| oriented: S-only / A-only | 0.815 / 0.978 | Из этих двух одиночных каналов A сохраняет результат лучше. |
| oriented: mix-only / no-A | 0.984 / 0.962 | Mixed канал тоже несёт существенную oriented информацию. |

[Источник всех значений](../source/0530-0534-ch14-channel-interventions-and-toy-illustrations.md#table-14-8). Разница между опубликованными $1.000-0.815=0.185$ и напечатанным drop $0.184$ может объясняться округлением исходных средних; без них нельзя уверенно объявить ошибку арифметики. Разница oriented residual $0.132$ в Table 14.7 и $0.133$ для full в Table 14.8 также требует знания агрегации и точности.

Residual не является самостоятельной мерой качества: symmetric no-S имеет $\Gamma=0.039$, заметно меньше full $0.268$, но accuracy падает до $0.705$. Само отключение вычисления может снижать инъекцию, ухудшая задачу. Проверять надо jointly task performance и diagnostic constraints.

## Семантика S/A/mix и граница причинного вывода

В этом опыте S/A выделяются из **эмпирического attention operator**, а не обязательно из feature-space motor. Эти операции различны; см. [184](184-static-diagnostics-procedures-and-intervention-semantics.md). Если все три канала используют один и тот же value/output путь, то $R_{\mathrm{mix}}=R_S+R_A$, и gate expression сводится к $(g_S+g_{\mathrm{mix}})R_S+(g_A+g_{\mathrm{mix}})R_A$. Тогда mixed branch не является третьей линейно независимой компонентой, а no-A не удаляет весь skew-вклад, проходящий через mix. При разных дополнительных projections точное равенство нужно проверять по реализации; описание страниц этого не фиксирует.

[Table 14.9](../source/0530-0534-ch14-channel-interventions-and-toy-illustrations.md#table-14-9) считает $3\times2\times8=48$ head instances на задачу; это не 48 независимых обучений. Все они названы S-dominant или A-dominant соответственно. Gate magnitudes не равны functional contributions: существенны нормы и взаимные компенсации branches, downstream map и mixed path.

Автор в [§14.11.5](../source/0530-0534-ch14-channel-interventions-and-toy-illustrations.md#section-14-11-5) прямо ограничивает вывод engineering validation этого интерфейса и не заявляет benchmark/language-model superiority или universal unbiased emergence. Текущие ablations измеряют чувствительность фиксированной обученной сети; они не доказывают, что сеть без соответствующего канала не смогла бы обучиться иначе. Параметры alignment bias, точная реализация branches, выборки и статистика нужны для отдельного воспроизведения.
