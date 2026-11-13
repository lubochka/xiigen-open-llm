---
title: "100. Эволюция параметрического balance и фактический поток Q/K"
source_pdf_pages: [351, 353]
book_pages: [315, 317]
batch: "008"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 100. Эволюция параметрического balance и фактический поток Q/K

[Индекс библиотеки](../README.md) · [Предыдущая карточка](99-two-flow-gradient-and-factorised-training.md) · [Следующая карточка](101-joint-diagnostic-and-weak-signature-velocity.md)

## Что именно вычисляет Corollary 10.11

На PDF 351 продолжается §10.9.1: здесь используется **параметрическое**, а не input-dependent определение

$$
A=\lVert M_a\rVert_F^2,\quad S=\lVert M_s\rVert_F^2,\quad
E=A+S>0,\quad\alpha_{\mathrm{param}}=A/E.
$$

Имя $\alpha_{\mathrm{param}}$ — редакторское; в источнике стоит общее $\alpha_{\mathrm{SA}}$. Различие с последующей empirical convention принципиально: при изменении $X$ score balance может измениться даже при фиксированном $M$. [Corollary 10.11](../source/0351-0354-ch10-two-flow-balance-and-signature-diagnostic.md#corollary-10-11); [предыдущее сравнение определений](97-canonical-qk-sa-geometry-and-balance-definitions.md).

Quotient rule и Frobenius-ортогональность symmetric/skew subspaces дают

$$
\nabla_M\alpha_{\mathrm{param}}
=\frac{2(1-\alpha_{\mathrm{param}})M_a-2\alpha_{\mathrm{param}}M_s}{E}.
$$

Это корректная производная при $M\ne0$. Она показывает, что balance зависит от **относительного** изменения двух энергий: рост $\lVert M_a\rVert_F$ сам по себе не гарантирует роста доли, если symmetric channel растёт быстрее. [Полное доказательство](../source/0351-0354-ch10-two-flow-balance-and-signature-diagnostic.md#display-10-alpha-gradient).

## Производная, шаг и learning rate

Источник пишет $\partial_t\alpha$ с множителем $\eta$ и remainder $O(\eta^2)$, а затем прямо называет выражение *descent increment*. Для одного прямого шага $\Delta M=-\eta G_M$ согласованная запись:

$$
\Delta\alpha_{\mathrm{param}}
=-\frac{2\eta}{E}\left[(1-\alpha_{\mathrm{param}})\langle M_a,G_a\rangle_F
-\alpha_{\mathrm{param}}\langle M_s,G_s\rangle_F\right]+O(\eta^2).
$$

В continuous gradient flow $\dot M=-G_M$ производная имеет ту же правую часть **без** множителя $\eta$ и без step remainder. Время может быть и переопределено, но это должно быть явным соглашением. Контроль Taylor error требует гладкости на окрестности, не пересекающей $E=0$; малость самого $\eta$ без такого контроля не задаёт uniform bound. [Формула и заключение доказательства](../source/0351-0354-ch10-two-flow-balance-and-signature-diagnostic.md#display-10-alpha-descent-increment).

## Как использовать фактическую скорость факторов

Для произвольной дифференцируемой траектории $M(t)$ положим $V=\dot M$, $V_s=P_s(V)$, $V_a=P_a(V)$. Тогда редакторская общая форма:

$$
\dot\alpha_{\mathrm{param}}
=\frac{2}{E}\left[(1-\alpha_{\mathrm{param}})\langle M_a,V_a\rangle_F
-\alpha_{\mathrm{param}}\langle M_s,V_s\rangle_F\right].
$$

Она не предполагает, что $V$ совпадает с отрицательным внешним Frobenius gradient. **Remark 10.12** теперь явно требует сначала протолкнуть actual parameter update в $\partial_tM$; это необходимое уточнение области corollary. [Remark 10.12](../source/0351-0354-ch10-two-flow-balance-and-signature-diagnostic.md#remark-10-12).

При обычном одновременном SGD $W_Q,W_K$ индуцированная первая скорость определяется

$$
H=G_MW_KW_K^\top+W_QW_Q^\top G_M,
\qquad\Delta M=-\eta H+O(\eta^2).
$$

В quotient formula тогда подставляют $P_s(H),P_a(H)$ вместо $G_s,G_a$. Это не отменяет ортогональность проекций, но меняет вычисляемое движение balance. Полный вывод и counterexample сохранены в [карточке 99](99-two-flow-gradient-and-factorised-training.md).

## Saturation coordinate не является доказанным attractor

Remark 10.13 предлагает использовать balance как динамическую координату. Figure 10.6 показывает **toy loss** $\frac12\lVert M-M_\star\rVert_F^2$, отдельно отличённый автором от MLM loss. Для свободного continuous flow редакторское решение $M(t)=M_\star+e^{-t}(M(0)-M_\star)$ действительно стремится к ненулевому target и его balance; это не универсальная динамика Transformer. [Remark 10.13 и Figure 10.6](../source/0351-0354-ch10-two-flow-balance-and-signature-diagnostic.md#remark-10-13).

Даже нулевая первая производная balance на $\alpha=0$ или $1$ не доказывает invariant boundary: новый ортогональный компонент может появиться в первом порядке по $M$, а его squared norm — во втором порядке по шагу. Поэтому выводы об устойчивости и неизбежности режима требуют отдельного анализа, как признаёт [§10.17](../source/0361-0364-ch10-empirical-status-and-synthesis.md#section-10-17).
