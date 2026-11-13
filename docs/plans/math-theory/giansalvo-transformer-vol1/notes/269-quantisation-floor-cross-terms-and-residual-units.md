---
title: "269. Noise floor: перекрёстный член, предел по архитектурам и единицы residual"
source_pdf_pages: [665, 665]
book_pages: [629, 629]
batch: "014"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 269. Noise floor: перекрёстный член, предел по архитектурам и единицы residual

## Какой floor заявлен

Lemma 16.72 определяет dimensionless residual energy $\Gamma_{\mathrm{res}}=\|A_{\mathrm{res}}\|_F^2/\Gamma_0^2$ и заявляет для любой обученной архитектуры

$$
\sqrt{\mathbb E\Gamma_{\mathrm{res}}^{\mathrm{quant}}}
\ge\sigma_q(b)=\frac{\beta^*}{2^b\sqrt3}.
$$

Это RMS нормированного residual, а не $\mathbb E\sqrt{\Gamma_{\mathrm{res}}}$ и не нижняя граница самой энергии без квадрата. [Lemma 16.72 и (16.110)](../source/0665-0667-ch16-noise-floor-and-joint-deployment-bounds.md#lemma-16-72).

Доказательство пишет $A_{\mathrm{res}}^{\mathrm{quant}}=\widetilde A_{\mathrm{res}}+\Delta\mathcal B$, где первая часть — architectural residual относительно возмущённого оператора, а вторая — perturbation самого оператора. Такое разложение полезно, но из нижней границы второго слагаемого ещё не следует нижняя граница суммы. [Разложение (16.111)](../source/0665-0667-ch16-noise-floor-and-joint-deployment-bounds.md#equation-16-111).

## Где нужен контроль корреляции

Точное равенство:

$$
\mathbb E\|\widetilde A_{\mathrm{res}}+\Delta\mathcal B\|_F^2
=\mathbb E\|\widetilde A_{\mathrm{res}}\|_F^2
+\mathbb E\|\Delta\mathcal B\|_F^2
+2\mathbb E\langle\widetilde A_{\mathrm{res}},\Delta\mathcal B\rangle_F.
$$

Последний член может быть отрицательным: residual и operator perturbation зависят от одних и тех же quantised weights. Q3 контролирует только второе слагаемое. Для вывода floor нужно, например, неотрицательное ожидаемое скалярное произведение, подходящая независимость с нулевым средним либо иное ограничение cancellation.

Простой алгебраический пример показывает именно этот пробел: $\mathcal B=\operatorname{diag}(1,0)$, $\widetilde{\mathcal B}=\operatorname{diag}(1,\epsilon)$ и reconstruction $C=\operatorname{diag}(1,0)$. Тогда $\widetilde A=\widetilde{\mathcal B}-C=\operatorname{diag}(0,\epsilon)$, $\Delta\mathcal B=\operatorname{diag}(0,-\epsilon)$, а их сумма нулевая. Оба task operators PSD. Это **не полный контрпример ко всем Q1–Q3 и процедуре обучения**; это контрпример к переходу от размера одного слагаемого к размеру суммы без дополнительного условия.

## Что доказывает дополнительная малость residual

В тексте вводится условие $\|\widetilde A_{\mathrm{res}}\|_F\le\delta\|\Delta\mathcal B\|_F$. При нём обратное неравенство треугольника действительно даёт

$$
\|A_{\mathrm{res}}^{\mathrm{quant}}\|_F
\ge(1-\delta)\|\Delta\mathcal B\|_F.
$$

Но условие малости не следует само по себе из слов «trained to minimise». Затем автор разрешает $\delta\to0$ за счёт увеличения архитектуры. Такой аргумент может обосновать асимптотический liminf вдоль подходящей последовательности, если гипотезы выполнены равномерно; он не доказывает ту же границу для каждой конечной архитектуры. [Шаг малости и предельный переход](../source/0665-0667-ch16-noise-floor-and-joint-deployment-bounds.md#display-0665-small-architectural-residual-bound).

Кроме того, $\beta^*$ прямо назван architecture-dependent. Если gain меняется при увеличении числа голов, универсальный head-independent floor требует положительного нижнего ограничения на $\beta^*$ по всему рассматриваемому семейству.

## Согласование единиц

На PDF 653 и 659 источник использовал $\|A_{\mathrm{res}}\|_F^2/\Gamma_0$, а здесь делит на $\Gamma_0^2$ и явно называет результат dimensionless. При $\Gamma_0=\|\mathcal B\|_F$ эти выражения имеют разные масштабы. Remark 16.73 правильно поясняет текущую convention, но не делает предыдущие записи одинаковыми. [Remark 16.73](../source/0665-0667-ch16-noise-floor-and-joint-deployment-bounds.md#remark-16-73).

Перед использованием stopping rule нужно отдельно хранить абсолютную норму, нормированную норму и нормированную энергию. Нижняя граница quantisation error в task-operator metric также не автоматически становится границей validation loss или downstream accuracy.
