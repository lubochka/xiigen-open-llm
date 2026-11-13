---
title: "178. Два algebraic instances: типы проекторов, границы residual и условная выразительность"
source_pdf_pages: [516, 517]
book_pages: [480, 481]
batch: "011"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 178. Два algebraic instances: типы проекторов, границы residual и условная выразительность

[Индекс](../README.md)

## Два способа конкретизировать шаблон

[§14.6.2 и Proposition 14.11](../source/0516-0517-ch14-subsumption-and-algebraic-instances.md#proposition-14-11) описывают prototype-gated residual

$$
R_{\mathrm{att}}(X)=\sum_{k=1}^{K_\ell}g_k(X)P_{a_k}U_k(X).
$$

[§14.6.3 и Proposition 14.12](../source/0516-0517-ch14-subsumption-and-algebraic-instances.md#proposition-14-12) сохраняют multi-head parameterisation, но назначают каждой голове atom, role и budget:

$$
h\mapsto(a(h),c(h),b(h)),\qquad c(h)\in\{S,A,M\}.
$$

Оба утверждения имеют условный характер принадлежности классу: если реализация обеспечивает dictionary coverage, соответствующие gates, S/A roles и residual bounds, она реализует заявленные требования. Они не доказывают существование допустимой реализации при любых порогах, оптимальность найденной assignment или улучшение loss.

## Signature atom сам по себе не определяет feature projector

В предыдущем разделе $a_k$ — точка пространства сигнатур. Чтобы построить $P_{a_k}$ на residual vectors, требуется отображение из этого пространства в направления или подпространства residual space. Совпадение имени atom не даёт такого отображения автоматически; различаться могут даже размерности.

Если отдельно задан ненулевой вектор $a\in\mathbb R^d$, обычный ортопроектор имеет вид

$$
P_a=\frac{aa^\top}{a^\top a},
\qquad P_a^2=P_a=P_a^\top,
\qquad\|P_a\|_{\mathrm{op}}=1.
$$

При row-vector convention feature projection реализуется как $UP_a$, тогда как напечатанное $P_aU$ требует согласованной ориентации или другого пространства действия. Это вопрос типов, а не выбора обозначений.

S/A projections действуют на **матрицы**: $\Pi_S(M)=(M+M^\top)/2$ и $\Pi_A(M)=(M-M^\top)/2$. Это линейные ортопроекторы в Frobenius space матриц. Они не являются автоматически двумя обычными $d\times d$ проекторами на token-feature vectors. Связь с prototype residual map должна быть определена отдельно.

Источник допускает также «soft projection». Для неё идемпотентность, self-adjointness и norm bound уже не следуют из названия; нужные свойства надо записать.

## Ограниченный gate не ограничивает весь residual без остальных норм

Для настоящих ортопроекторов и согласованной матричной нормы справедливо

$$
\left\|\sum_k g_kP_{a_k}U_k\right\|_F
\le\sum_k |g_k|\,\|U_k\|_F.
$$

Если $|g_k|\le g_{\max}$ и $\|U_k\|_F\le u$, это даёт $K_\ell g_{\max}u$. Без bound на $U_k$ даже gate в $[0,1]$ не ограничивает amplitude. Если подпространства перекрываются, cross terms не исчезают; для soft projectors нужен множитель $\|P_{a_k}\|_{\mathrm{op}}$.

Поэтому условие Proposition 14.11 «residual injection obeys the bound» является существенной предпосылкой. Оно не выводится только из наличия dictionary labels и sigmoid gates. Аналогично penalising duplicate assignments в Proposition 14.12 не гарантирует их отсутствия при конечном весе penalty; назначенная роль должна быть связана с реальной параметризацией, а не лишь с label.

## Algebraic inclusion и approximation — разные, совместимые шаги

[Начало §14.6](../source/0516-0517-ch14-subsumption-and-algebraic-instances.md#section-14-6) правильно различает точное восстановление конкретного block map и плотность класса функций. Если для одной области, одного target space и одной topology установлено

$$
\mathcal F_{\mathrm{std}}\subseteq\mathcal F_{\mathrm{rat}},
$$

то автоматически $\overline{\mathcal F_{\mathrm{std}}}\subseteq\overline{\mathcal F_{\mathrm{rat}}}$. Если стандартный класс плотен в заданном target class, расширенный класс наследует эту плотность. Это корректный set-theoretic вывод. Включение только в closure также может быть достаточным для плотности, но его нельзя называть точным конечнопараметрическим восстановлением.

Нужны все условия исходного approximation result: допустимые входы и длины, positional information, mask, activation, доступная глубина/ширина и смысл approximation. Один блок не автоматически реализует теорему о семействе сетей произвольной глубины. Дополнительные жёсткие ограничения rationalised class также должны допускать конфигурацию или limit, использованные для embedding.

[177](177-prenorm-recovery-and-the-postnorm-template.md) показывает, что Proposition 14.10 не устанавливает необходимое включение для буквального post-LN шаблона. Это дефект предпосылки, а не опровержение логического принципа наследования после действительного включения.

## Граница этого файла

На PDF 517 начинается [§14.6.4 Universal approximation status](../source/0516-0517-ch14-subsumption-and-algebraic-instances.md#section-14-6-4). Его первое предложение заканчивается на данной странице словами “under a specified set of” и продолжается на [PDF 518](../source/0518-0522-ch14-static-geography-closure.md#pdf-0518). Корректный статус последующего corollary обсуждается в [179](179-conditional-inheritance-and-empirical-dictionary-threshold.md); данная карточка ограничена определениями и аргументами PDF 516–517.
