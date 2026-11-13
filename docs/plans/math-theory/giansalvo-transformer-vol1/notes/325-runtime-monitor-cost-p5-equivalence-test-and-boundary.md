---
title: "325. Monitor cost, Prediction P5 и точная граница PDF 750"
source_pdf_pages: [750, 750]
book_pages: [714, 714]
batch: "015"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 325. Monitor cost, Prediction P5 и точная граница PDF 750

## Вопрос

Выведены ли 3% monitor overhead и equivalence при $p>0.05$, и где именно заканчивается batch 015?

**3% остаётся авторской runtime estimate, не следствием одних приведённых big-O формул; отсутствие statistical significance не доказывает equivalence.** Prediction P5 сформулирована полностью, но следующий за ней абзац об общей сводке продолжается на PDF 751. [PDF 750](../source/0749-0750-ch18-runtime-control-results-curriculum-and-prediction-p5.md#pdf-0750), [Prediction P5](../source/0749-0750-ch18-runtime-control-results-curriculum-and-prediction-p5.md#prediction-p5).

## Проверка FLOP масштаба

При $N=2048$, $d_{\mathrm{model}}=64$, $L=256$, $t_{\mathrm{chk}}=500$,

$$
Nd_{\mathrm{model}}^2=8\,388\,608,\qquad
24Ld_{\mathrm{model}}^2+4L^2d_{\mathrm{model}}
=41\,943\,040.
$$

Если записать совокупную стоимость monitor как $C\,Nd_{\mathrm{model}}^2$ с неизвестной константой $C$, то amortised отношение к **одному такому forward pass на шаг** составляет

$$
\frac{C\,Nd_{\mathrm{model}}^2}
{t_{\mathrm{chk}}\,(24Ld_{\mathrm{model}}^2+4L^2d_{\mathrm{model}})}
=0.0004C.
$$

Для иллюстративных двух unit-cost членов $C=2$ это 0.08%, а 3% соответствует $C=75$. Эти числа **не опровергают** измеренное wall-clock значение: реальная SVD, data movement, synchronisation, CPU/GPU placement, batching и прочие costs не заданы big-O константой. Также training step включает backward passes и actor/critic operations, а не один forward.

Поэтому из этих страниц можно экспортировать асимптотический масштаб monitoring и авторский reported overhead, но не точный предсказанный runtime для Xiigen. Для измерения 3% нужны аппаратная конфигурация, одинаковый training budget, timing methodology и uncertainty. Связывать эту оценку с “nearly free” улучшением на 59 points без этих данных преждевременно: quality difference и relative compute cost — разные quantities.

## P5 требует теста equivalence, а не неудачи теста различия

P5 предсказывает head counts в пределах $\lceil\log_2(1/\varepsilon_{\mathrm{prune}})\rceil$ и accuracies “indistinguishable at $p>0.05$”. Большое $p$ может означать малую мощность, большой noise или небольшую выборку; оно не устанавливает достаточно малое различие.

Для осмысленного equivalence criterion сначала задают практически допустимый accuracy margin $\delta$ и проверяют, что доверительный интервал разности лежит внутри $[-\delta,\delta]$, при подходящих предпосылках и design. Это отдельный вопрос от null hypothesis точного равенства. Для head count надо проверять **заявленный buffer**, а не просто наличие статистически ненулевой разности.

Например, хорошо измеренная разница в одну голову может быть highly significant и всё же удовлетворять buffer 10 при $\varepsilon_{\mathrm{prune}}=10^{-3}$. Поэтому “any statistically significant deviation” не всегда falsifies написанный count conclusion. И наоборот, nonsignificant difference с широким interval не подтверждает его. Связь Proposition 18.3 с performance metrics отдельно ограничена в [321](321-offline-runtime-equivalence-conditions-and-head-count-buffer.md).

Source прямо признаёт, что предложенный classification rerun ещё не выполнен в control case study. P5 остаётся **предсказанием для будущей проверки**, не новым доказанным результатом или воспроизведённым экспериментом. Его anchor отдельный; он не прибавляется к numbered propositions главы.

## Точная точка продолжения

Последние слова PDF 750:

> “The chapter-level synthesis of all”

PDF 751 начинается продолжением “five experimental campaigns (Tables 18.7, 18.8, and the extended summary table 18.9) is deferred to Section 18.4.3, …”. Preview использован только для boundary; содержание следующей страницы в source этого batch не перенесено.

На PDF 750 **P5 завершена**. Незавершён именно последующий абзац общей сводки; §18.3.3 и глава 18 продолжаются. §18.3.4 начинается лишь на PDF 751, после дополнительного текста предыдущего раздела. Следующий диапазон чтения — **PDF 751–800 / печатные страницы 715–764**. Card 326 относится к следующему пакету, а текущий сегмент заканчивается карточкой 325.
