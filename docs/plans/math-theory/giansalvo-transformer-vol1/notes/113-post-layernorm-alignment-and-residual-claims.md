---
title: "113. Post-LN energy, alignment κ и пределы residual-аргумента"
source_pdf_pages: [380, 382]
book_pages: [344, 346]
batch: "008"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 113. Post-LN energy, alignment κ и пределы residual-аргумента

[Индекс библиотеки](../README.md) · [§11.5.4](../source/0379-0384-ch11-gap-opening-and-symmetry-breaking.md#section-11-5-4) · [Предыдущая карточка](112-gap-opening-threshold-and-perturbation-limits.md) · [Следующая карточка](114-gaussian-initialisation-moments-and-concentration.md)

## Какая часть оценки доказана непосредственно

Proposition 11.19 принимает нормировку $\lVert X_{\mathrm{LN}}\rVert_F=\sqrt{nd}$ и определяет

$$
\Gamma_{\mathrm{LN}}
=\lVert X_{\mathrm{LN}}M_aX_{\mathrm{LN}}^\top\rVert_F.
$$

Тогда субмультипликативность действительно даёт

$$
\Gamma_{\mathrm{LN}}
\le\lVert X_{\mathrm{LN}}\rVert_F^2\lVert M_a\rVert_F
=nd\,\lVert M_a\rVert_F.
$$

При $M_a\ne0$ корректно и определение

$$
\kappa_{\mathrm{LN}}
=\frac{\Gamma_{\mathrm{LN}}}{nd\,\lVert M_a\rVert_F}\in[0,1].
$$

Это надёжная алгебраическая часть пунктов (i)–(ii), при точно указанных условиях. В исходнике случай $M_a=0$ оставляет $0/0$; если в измерениях выбирается значение 0, его надо явно обозначить как дополнительную конвенцию. [Proposition 11.19, (11.11)–(11.12)](../source/0379-0384-ch11-gap-opening-and-symmetry-breaking.md#proposition-11-19).

## Нормировка √nd не описывает любой LayerNorm

Для идеального row-wise LN без affine parameters, с $\varepsilon=0$ и ненулевой дисперсией каждой строки сумма квадратов нормированной строки равна $d$. Именно в этом случае получается $\sqrt{nd}$.

При обычной записи

$$
\operatorname{LN}(x)
=\gamma\odot\frac{x-\mu\mathbf1}{\sqrt{\sigma^2+\varepsilon}}+\beta
$$

участвуют learned gain $\gamma$, bias $\beta$ и стабилизатор $\varepsilon$. Даже при $\gamma=\mathbf1,\beta=0$ квадрат нормы нормированной строки равен $d\sigma^2/(\sigma^2+\varepsilon)$, а не всегда $d$. Для произвольных gain/bias нужна фактическая норма $X_{\mathrm{LN}}$ или отдельная верхняя оценка. [Идеализированное определение в Remark 11.18](../source/0379-0384-ch11-gap-opening-and-symmetry-breaking.md#remark-11-18), [карточка 87 о LN](87-feedforward-gates-and-layernorm-motors.md).

Нормировка активаций сама по себе не ограничивает $\lVert M_a^{(h,l)}\rVert_F$ равномерно по слоям или времени. Если фиксировать допустимый $X_{\mathrm{LN}}$ с ненулевым $X_{\mathrm{LN}}AX_{\mathrm{LN}}^\top$ и взять $M_a^{(l)}=lA$, то $\Gamma_{\mathrm{LN}}^{(l)}$ растёт линейно при той же норме входа. Поэтому фраза “bounded for all depths — it does not diverge” требует дополнительного uniform bound на параметры, $n,d$ и нормировку; она не следует из (11.11) без него. [Пункт (i)](../source/0379-0384-ch11-gap-opening-and-symmetry-breaking.md#equation-11-11).

## Монотонность κ не следует из верхней оценки

Пункт (iii) ссылается на гипотезы Proposition 11.33 и утверждает неубывание $\kappa_{\mathrm{LN}}$ при обучении. Его sketch объясняет это тем, что loss gradient якобы направлен по ведущим Gram-направлениям. Однако матрица $X^\top G_LX$ зависит от **самого $G_L$**; из знания $X^\top X$ нельзя вывести ни её ведущие направления, ни нужный знак изменения $\kappa$. При сравнении с настоящим skew-gradient также нужно явно взять антисимметричную проекцию. [Пункт (iii) и доказательство на PDF 381](../source/0379-0384-ch11-gap-opening-and-symmetry-breaking.md#pdf-0381).

При $\Gamma_{\mathrm{LN}}>0$, $M_a\ne0$ и фиксированных $n,d$ дифференцирование даёт редакторскую проверочную формулу:

$$
\dot\kappa_{\mathrm{LN}}
=
\frac{\langle\mathcal D,\dot{\mathcal D}\rangle_F}
{nd\,\lVert M_a\rVert_F\,\lVert\mathcal D\rVert_F}
-
\kappa_{\mathrm{LN}}
\frac{\langle M_a,\dot M_a\rangle_F}{\lVert M_a\rVert_F^2},
\qquad
\mathcal D=X_{\mathrm{LN}}M_aX_{\mathrm{LN}}^\top.
$$

Если меняются представления, $\dot{\mathcal D}$ включает и производные $X_{\mathrm{LN}}$. Для неотрицательного результата нужно контролировать оба члена; рост одной нормы или общий термин “coherence chain” такого контроля не заменяет. Это ограничение приведённого доказательства, а не утверждение, что ни при каких усиленных гипотезах монотонность невозможна.

Источник также меняет независимую переменную: формальная нормовая оценка сравнивает слои $l$, а пункт (iii) говорит о шагах обучения $t$. Послойный график не является непосредственной проверкой монотонности по времени.

## Residual не гарантирует сохранение информации или положительный cross-term

Remark 11.21 правильно отделяет параметр $M_a$ от активации: residual-соединение изменяет представления, на которых действует параметр. Однако алгебра

$$
X^{(l+1)}=X^{(l)}+F^{(l)}(X^{(l)})
$$

не гарантирует, что слагаемое $X^{(l)}$ сохранится без компенсации. При $F(X)=-X$ выход нулевой. Перекрёстные произведения могут быть положительными, отрицательными или нулевыми; само наличие знака «+» этого не решает. [Remark 11.21](../source/0379-0384-ch11-gap-opening-and-symmetry-breaking.md#remark-11-21).

Без skip connection информация о входе тоже не обязана исчезать: обычное отображение $F(X)=X$ сохраняет её полностью. Поэтому утверждение «без residual cross-terms vanish» не является общим тождеством. Для residual amplification нужны условия на величины, направления и корреляции обновлений. Это не опровергает возможную пользу residual в конкретных архитектурах; ограничивается именно универсальный вывод из одной записи суммы.

Термин “rank-neutral” в начале §11.5.4 также требует условий. Идеальный LN для $X=I_d$, $d>1$, выполняет центрирование строк, дающее ранг $d-1$; residual с $F=-I$ даёт ранг 0. Следовательно, ни операция LN, ни произвольное residual-сложение не сохраняют ранг автоматически. [Вводный абзац §11.5.4](../source/0379-0384-ch11-gap-opening-and-symmetry-breaking.md#section-11-5-4).

## Что установлено численным абзацем

Remark 11.20 сообщает **синтетическую** симуляцию с $d=64,n=32$ и 12 слоями: post-LN energy около $103\pm4$, pre-LN energy 1.1→1.5, названные автором ростом на 35%. В PDF напечатано именно **103**, не $10^3$. По округлённым конечным числам редакторское отношение $1.5/1.1-1$ составляет около 36.4%; точные неокруглённые значения не предоставлены на этой странице. [Remark 11.20](../source/0379-0384-ch11-gap-opening-and-symmetry-breaking.md#remark-11-20).

Этот пример не является доказательством uniform boundedness, монотонности $\kappa$ по training time или универсального поведения BERT. Метка “BERT-base synthetic simulation” сохранена буквально; она не заменяет параметры и воспроизводимые данные реальной обученной модели.

