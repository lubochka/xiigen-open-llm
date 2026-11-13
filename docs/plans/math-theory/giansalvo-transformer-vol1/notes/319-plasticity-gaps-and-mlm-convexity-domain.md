---
title: "319. Plasticity gap и MLM convexity: метрика протокола не определяет landscape"
source_pdf_pages: [737, 743]
book_pages: [701, 707]
batch: "015"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 319. Plasticity gap и MLM convexity: метрика протокола не определяет landscape

## Вопрос

Доказывает ли различие probe/fine-tune результатов свойство optimisation landscape и является ли MLM cross-entropy near-convex по весам encoder?

**Ни одно из этих свойств не следует только из названия loss или нескольких accuracy.** В кейсе есть полезные наблюдения о двух evaluation protocols, но их причинная и геометрическая интерпретация сильнее доступных данных. [Pilot gap](../source/0735-0738-ch18-proteome-mlm-mechanism-and-ablation.md#pdf-0737), [Lessons и convexity claims](../source/0739-0742-ch18-corpus-alignment-and-proteome-transfer-limits.md#section-18-2-5).

## Два разных значения слова gap

Для одного encoder естественно определить $G(V)=\mathrm{FT}(V)-\mathrm{Probe}(V)$. По расширенной серии $G(V9)=-5.25$, $G(V10)=-6.97$, $G(V11)=-4.00$, $G(V9')=+7.64$ pp.

Для сравнения архитектур другой diagnostic —

$$
D=\bigl(\mathrm{FT}(V9)-\mathrm{FT}(V10)\bigr)
-\bigl(\mathrm{Probe}(V9)-\mathrm{Probe}(V10)\bigr).
$$

Пилот даёт заявленное $D=8.66-3.15=5.51$ pp. Но Table 18.5 даёт $D=(70.78-63.01)-(76.03-69.98)=7.77-6.05=1.72$ pp. Поэтому 5.51 — результат **пилотной серии**, который заключение повторяет после расширенного опыта; это не универсальная постоянная для тех же названий V9/V10. [Table 18.5](../source/0735-0738-ch18-proteome-mlm-mechanism-and-ablation.md#table-18-5).

Fine-tuning не обязан сохранять test accuracy frozen probe. Меняются обучаемые параметры, optimisation error, regularisation, sample variance и ранняя остановка. Даже если более широкий класс функций включает frozen solution, минимизация training loss не гарантирует меньшую test error. Поэтому отрицательный $G$ не доказывает, что probe уже извлёк весь сигнал, а положительный — что дополнительные layers обязательно кодируют именно межвидовую вариативность. Эти версии требуют дополнительных измерений representations и сравнимого tuning budget.

## Cross-entropy выпукла по logits, не автоматически по параметрам

Для softmax logits $z$ Hessian cross-entropy равен $\operatorname{diag}(p)-pp^\top\succeq0$. Но при $z=z(\vartheta)$ Hessian по $\vartheta$ содержит дополнительный член:

$$
\nabla_\vartheta^2\ell
=J^\top\nabla_z^2\ell\,J
+\sum_j(\partial_{z_j}\ell)\nabla_\vartheta^2z_j.
$$

Второй член может быть indefinite. Простейший пример без transformer complexity: binary logit $z=ab$, label 1,

$$
\ell(a,b)=\log(1+e^{-ab}),\qquad
\nabla^2\ell(0,0)=
\begin{pmatrix}0&-1/2\\-1/2&0\end{pmatrix}.
$$

Eigenvalues равны $-1/2$ и $+1/2$. То есть даже двухслойная scalar parametrisation превращает convex-in-logit loss в non-convex-in-weights objective.

Это не измерение Hessian конкретного белкового encoder и не опровержение возможности хорошо обусловленной локальной области. Это точный контрпример **автоматическому выводу** из cross-entropy. Для “near-convex on the basin of standard initialisations” надо определить basin, допустимую отрицательную кривизну и проверить свойство на рассматриваемой параметризации. В source такого определения и анализа нет. [Заявление PDF 742](../source/0739-0742-ch18-corpus-alignment-and-proteome-transfer-limits.md#pdf-0742), [начало control contrast](../source/0743-0745-ch18-euler-lagrange-control-and-deployment-equivalence.md#section-18-3).

Следовательно, размещение protein MLM в “convex half-plane” остаётся авторской гипотезой о режиме, а не проверенной предпосылкой Proposition 18.3. Данные о превосходстве одной модели на human Pfam также не оценивают intrinsic manifold dimension и не доказывают overparameterisation всех больших baselines. Для проекта следует хранить protocol metrics отдельно от утверждений об optimisation geometry и от условных теорем, которые этой геометрией пользуются.
