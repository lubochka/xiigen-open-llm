---
title: "296. Bias, iteration floor и накопление ошибок последовательной дефляции"
source_pdf_pages: [706, 707]
book_pages: [670, 671]
batch: "015"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 296. Bias, iteration floor и накопление ошибок последовательной дефляции

## Почему конечная итерация может лежать выше bias bound

Figure 17.6 показывает авторский plateau около $8\cdot10^{-7}$ и линейный участок при больших amplitudes. Lemma 17.10 ограничивает расстояние между точными spectral subspaces исходного и возмущённого операторов. На графике измеряется finite-iteration estimate $W$; его ошибка включает ещё optimisation residual. [Figure 17.6 и описание опыта](../source/0706-0709-ch17-subspace-bias-and-conditional-convergence.md#figure-17-6).

Для ортогональных projectors:

$$
\|P_W-P_V\|_2
\le\|P_W-P_{\widetilde V}\|_2
+\|P_{\widetilde V}-P_V\|_2.
$$

Первое слагаемое — незавершённое решение perturbed задачи, второе — собственно bias. Поэтому зелёные точки выше красного предельного bound при малой $\varepsilon_{\mathrm{reg}}$ не опровергают spectral lemma; они показывают, что её одной недостаточно для полной ошибки алгоритма.

Source связывает plateau с 300 шагами Stiefel flow при learning rate $0.05$, но исходные матрица, perturbation, seed и trajectory не даны. В этом пакете этот эксперимент **не воспроизводился**. Интервал $[10^{-12},10^{-2}]$ охватывает десять порядков, хотя текст говорит о шести; исходная формулировка сохранена.

## Выбор amplitude требует согласованных единиц

Bias bound имеет масштаб $\varepsilon_{\mathrm{reg}}\|R\|_2/\delta_{\mathrm{out}}$. Поэтому default $\sqrt{\varepsilon_{\mathrm{mach}}}\|A_{\mathrm{res}}\|$ не даёт автоматически angular bias $\sqrt{\varepsilon_{\mathrm{mach}}}$: остаются отношения $\|A\|/\delta_{\mathrm{out}}$ и $\|R\|_2$. Нормировка Gaussian perturbation в prescription не зафиксирована.

Совет выбирать amplitude «на декаду выше iteration floor» тоже требует перевода единиц. Floor угла безразмерен, amplitude имеет масштаб оператора. Для сравнимого angular contribution ориентир должен учитывать factor $\delta_{\mathrm{out}}/\|R\|_2$. Повышение amplitude выше текущей optimisation error увеличивает bias; оно полезно лишь при отдельно установленном выигрыше solver dynamics. Из одной верхней perturbation bound такой выигрыш не следует. [Choice of epsilon и рекомендация](../source/0706-0709-ch17-subspace-bias-and-conditional-convergence.md#pdf-0706), [продолжение](../source/0706-0709-ch17-subspace-bias-and-conditional-convergence.md#pdf-0707).

Нельзя также превратить $\sqrt{\varepsilon_{\mathrm{mach}}}\|A\|$ в универсальный минимально различимый eigengap без модели rounding, residual accuracy и требуемого угла. Это полезная scale heuristic, не установленный здесь аппаратный порог для всех матриц.

## Возврат к unperturbed A не обнуляет ошибку projector

Источник утверждает, что bias затрагивает только один event и не накапливается, поскольку дефляция применяется к исходному $A$. Но использованный projector всё равно приближённый. Для двух ортогональных projectors $P,\widetilde P$,

$$
\|\widetilde P A\widetilde P-PAP\|_2
\le2\|A\|_2\|\widetilde P-P\|_2.
$$

Это следует после добавления и вычитания $\widetilde P AP$. Следующие residuals уже зависят от этого ошибочного результата. Для последовательности шагов естественная telescoping estimate содержит сумму локальных ошибок; чтобы заменить её равномерным bound, нужны contraction, correction или summability conditions.

Поэтому проверяемое ядро — локальная perturbation estimate, отдельно optimisation residual и накопление projector errors. Само возвращение от $A+\varepsilon R$ к $A$ перед очередным шагом не доказывает отсутствие накопления.
