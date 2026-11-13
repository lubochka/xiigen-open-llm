---
title: "Appendix B: Notation Summary"
source_pdf_pages: [797, 800]
book_pages: [761, 764]
batch: "016"
language: en
type: source-transcription
status: complete-for-page-range
continues_from_previous_file: false
continues_after_file: false
---

# Appendix B: Notation Summary

[Library index](../README.md) · [Source policy](../SOURCE.md)

> Editorial note: Full transcription of PDF 797–800. Every notation entry is retained as a searchable Markdown table. Printed dimensions, chapter references, overloaded symbols and literal question marks in the citation example are preserved rather than reconciled silently with earlier chapters. This appendix ends on PDF 800; Appendix C starts on PDF 801.

<a id="pdf-0797"></a>
## PDF 797 · Book page 761

<a id="appendix-b"></a>
## Appendix B — Notation Summary

This appendix gathers, in a single reference, the notation used throughout the volume. It is not a definition: definitions of all named objects are introduced in the chapters where they first occur. The aim here is to serve as a lookup table for the reader who has forgotten where a symbol was introduced. Items are grouped by category and ordered alphabetically within each category.

<a id="section-b-1"></a>
### B.1 Number sets and basic spaces

| Symbol | Meaning |
|---|---|
| $\mathbb R$ | The real numbers. |
| $\mathbb N$ | The natural numbers, including zero. |
| $\mathbb Z$ | The integers. |
| $\mathbb R^d$ | The standard real $d$-dimensional Euclidean space, with the canonical inner product $\langle x,y\rangle=\sum_{i=1}^d x_i y_i$ and induced norm $\lVert x\rVert=\sqrt{\langle x,x\rangle}$. |
| $\mathbb R^{n\times d}$ | The space of $n\times d$ real matrices. A matrix $X\in\mathbb R^{n\times d}$ is interpreted throughout as a sequence of $n$ row vectors of dimension $d$, one per token. |

<a id="section-b-2"></a>
### B.2 Matrix-algebraic operators

| Symbol | Meaning |
|---|---|
| $\operatorname{tr}(M)$ | Trace of the matrix $M\in\mathbb R^{d\times d}$. |
| $\operatorname{rank}(M)$ | Rank, abbreviated $\operatorname{rk}$. |
| $\operatorname{diag}(v)$ | The diagonal matrix with the entries of $v$ on the main diagonal. |
| $\operatorname{diag}(M)$ | The vector of diagonal entries of $M$, when the argument is a square matrix. |
| $\operatorname{sym}(M)$ | Symmetric part, $\operatorname{sym}(M)=\frac12(M+M^\top)$. |
| $\operatorname{asym}(M)$ | Antisymmetric part, $\operatorname{asym}(M)=\frac12(M-M^\top)$. Some chapters write this $\operatorname{anti}$. |
| $\operatorname{span}(\{v_1,\ldots,v_k\})$ | Linear span of vectors. |
| $\operatorname{supp}(\cdot)$ | Support of a vector or function: the indices on which it is non-zero, or the closure of the set where the function is non-zero. |
| $\operatorname{relint}(C)$ | Relative interior of a convex set $C$. |
| $\operatorname{conv}(\cdot)$ | Convex hull. |

<a id="pdf-0798"></a>
## PDF 798 · Book page 762

| Symbol | Meaning |
|---|---|
| $\operatorname{dist}(x,S)$ | Distance from a point to a set, $\operatorname{dist}(x,S)=\inf_{y\in S}\lVert x-y\rVert$. |
| $\operatorname{sgn}(\cdot)$ | Sign function, extended to vectors elementwise. |
| $\operatorname{argmin}_{x\in C}f(x)$ / $\operatorname{argmax}$ | Set of minimisers / maximisers, taken as a single element when unique. |
| $\lVert M\rVert_F$ | Frobenius (Hilbert–Schmidt) norm, $\lVert M\rVert_F=\sqrt{\operatorname{tr}(M^\top M)}$. |
| $\lVert M\rVert_{S_p}$ | Schatten $p$-norm, $\lVert M\rVert_{S_p}=(\sum_i\sigma_i(M)^p)^{1/p}$, where $\sigma_i(M)$ are the singular values of $M$ in decreasing order. |
| $\lVert M\rVert_2$ | Spectral norm, equal to $\sigma_1(M)$. |
| $M^\top$ | Transpose of $M$. |
| $M^\dagger$ | Moore–Penrose pseudoinverse. |
| $M\otimes N$ | Kronecker product. |

<a id="section-b-3"></a>
### B.3 Architectural objects

| Symbol | Meaning |
|---|---|
| $X\in\mathbb R^{n\times d}$ | Hidden state matrix at the input of a Transformer block: $n$ tokens, each represented by a row of dimension $d$. |
| $X^{(\ell)}$ | Hidden state at the output of layer $\ell$. |
| $d_{\mathrm{model}}$ | Model (representation) dimension. Abbreviated $d$ where there is no ambiguity. |
| $d_k$ | Per-head key/query dimension. |
| $d_v$ | Per-head value dimension. In BERT-base, $d_k=d_v=d/H=64$. |
| $d_{\mathrm{ff}}$ | Hidden dimension of the feed-forward block. In BERT-base, $d_{\mathrm{ff}}=4d=3072$. |
| $n$ | Sequence length. |
| $L$ | Number of stacked Transformer layers. |
| $H$ | Number of attention heads per layer. |
| $W_Q,W_K,W_V\in\mathbb R^{d\times d_k}$ | Per-head query, key, value projection matrices. |
| $W_O\in\mathbb R^{Hd_v\times d}$ | Per-block output projection. |
| $S=QK^\top/\sqrt{d_k}\in\mathbb R^{n\times n}$ | Score matrix. |
| $M=W_QW_K^\top\in\mathbb R^{d\times d}$ | Per-head score operator in the input frame: $S=XMX^\top/\sqrt{d_k}$. |
| $A=\operatorname{softmax}(S)\in\mathbb R^{n\times n}$ | Attention matrix (row-stochastic). |
| $\operatorname{LN}(\cdot)$ | LayerNorm operator. |
| $\operatorname{MHA}(\cdot)$ | Multi-head attention operator. |
| $\operatorname{FFN}(\cdot)$ / $\operatorname{MLP}(\cdot)$ | Feed-forward block / multi-layer perceptron. |

<a id="pdf-0799"></a>
## PDF 799 · Book page 763

<a id="section-b-4"></a>
### B.4 Motor formalism

The motor formalism of Chapters 9–11 splits the score operator $M$ into symmetric and antisymmetric sectors and tracks their evolution as motors on the Lie algebra $\mathfrak{so}(d)$.

| Symbol | Meaning |
|---|---|
| $M_s=\operatorname{sym}(M)$ | Symmetric sector of the score operator. |
| $M_a=\operatorname{asym}(M)$ | Antisymmetric sector. Lives in $\mathfrak{so}(d)$, the space of $d\times d$ skew-symmetric matrices. |
| $\Gamma$ | Directional energy: a scalar that measures the cumulative antisymmetric contribution of a stack of heads. |
| $\Gamma_{\mathrm{res}}$ | Residual directional energy: the part of $\Gamma$ not explained by the dictionary of canonical regimes. |
| $A_{\mathrm{res}}$ | Residual attention operator: the part of $A$ not in the span of the canonical regimes (see Chapter 7). |

<a id="section-b-5"></a>
### B.5 Signature and regime dictionary

| Symbol | Meaning |
|---|---|
| $\kappa$ | Canonical regime label, one of eight values from the dictionary of Chapter 6. |
| $\mathcal I_{\mathrm{cone}}$ | Cone index of a token cloud (Chapter 7). |
| $\sigma^{(h,\ell)}$ | Signature of head $h$ at layer $\ell$: the 8-tuple of regime coefficients. |

<a id="section-b-6"></a>
### B.6 Training dynamics and INCRT controller

| Symbol | Meaning |
|---|---|
| $\theta$ | Concatenated network parameters. |
| $\mathcal L(\theta)$ | Empirical or population loss. |
| $\Theta(x,x')$ | Neural Tangent Kernel (NTK). |
| $K$ | INCRT head count at the current training step. |
| $K^\star$ | Converged head count (Universal Architecture Complexity Theorem, Chapter 16). |
| $\theta_w$ | INCRT warm-start threshold. |
| $\varphi_g$ | INCRT gate phase. |
| $\kappa_T$ | INCRT temperature constant. |
| $\eta$ | Learning rate. |

<a id="section-b-7"></a>
### B.7 Subscripts, accents, and decorations

| Symbol | Meaning |
|---|---|
| $x_{\mathrm{res}}$ | “Residual” part of $x$ (after removing the canonical or dictionary component). |
| $x_{\mathrm{eff}}$ | Effective version of a quantity (e.g. effective rank). |
| $x_{\mathrm{init}}$ | Value at initialisation (random Xavier or pretrained MLM warm-start, specified locally). |
| $x^{(h,\ell)}$ | Quantity attached to head $h$ at layer $\ell$. |

<a id="pdf-0800"></a>
## PDF 800 · Book page 764

| Symbol | Meaning |
|---|---|
| $\bar x$ | Mean of $x$ over a relevant index (tokens, heads, layers). |
| $\hat x$ | Estimator of $x$ from data, or normalised version. |
| $\widetilde x$ | Modified or transformed $x$ (specified locally). |
| $\dot x$ | Time derivative of $x$ along gradient flow. |

<a id="section-b-8"></a>
### B.8 Cross-referencing conventions

Theorems, lemmas, propositions, definitions, and remarks share a single counter that is reset at the start of each chapter. The notation “Theorem $k.\ell$” refers to the $\ell$-th theorem-class object of Chapter $k$ in chronological order within the chapter. Equations are numbered separately, also per chapter, as “$k.\ell$”.

Citations follow the author-year convention: “?” expands to “Author (2024)” and “(?)” to “(Author, 2024)”. Multiple citations are separated by semicolons: “(Author, 2024; Smith and Jones, 2025)”.

The bibliography is unified at the end of the volume; there are no per-chapter reference lists.



> Editorial navigation: [Previous source](0792-0796-appendix-a-mathematical-background.md#pdf-0796) · [Continuation instructions: PDF 801–818](../USAGE.md)
