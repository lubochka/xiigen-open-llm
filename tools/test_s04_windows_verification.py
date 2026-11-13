"""Non-model regressions for the actual fixed push/origin helper and workflow caller."""
from __future__ import annotations
import ast
import base64
import zlib
import copy
import hashlib
import importlib.util
import io
from contextlib import redirect_stdout, redirect_stderr
import json
import os
import re
from pathlib import Path
import subprocess
import sys
import tempfile
import textwrap
import unittest
from unittest.mock import patch

sys.dont_write_bytecode = True
ROOT = Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location('s04_verification_under_test', ROOT / 'tools/s04_windows_verification.py')
m = importlib.util.module_from_spec(spec)
spec.loader.exec_module(m)
A, B, W = 'a' * 40, 'b' * 40, 'e' * 40
NEW = b'{"schema":"s04-windows-verification-request-v1","gate":"S03.04","request_id":"a5fb205a-27d2-42e4-8931-886dbab3cefe","candidate_origin":"push-event-after"}\n'
OLD = NEW.replace(b'a5fb205a-27d2-42e4-8931-886dbab3cefe', b'0f1b45d7-c0d8-4f71-8baf-d6ee46db8e03')


def environment(job='push-admission', event_name='push', workflow=A):
    return {'GITHUB_EVENT_NAME': event_name, 'GITHUB_REPOSITORY': 'owner/project', 'GITHUB_REPOSITORY_ID': '17',
            'GITHUB_REF': 'refs/heads/codex/dpo-implementation', 'GITHUB_SHA': A, 'GITHUB_WORKFLOW_SHA': workflow,
            'GITHUB_WORKFLOW_REF': 'owner/project/.github/workflows/s03-windows-verification.yml@refs/heads/codex/dpo-implementation',
            'GITHUB_RUN_ID': '98765', 'GITHUB_RUN_ATTEMPT': '3', 'GITHUB_JOB': job, 'CANDIDATE_SHA': A, 'GATE_NAME': 'S03.04'}


def event():
    return {'repository': {'full_name': 'owner/project', 'id': 17}, 'ref': 'refs/heads/codex/dpo-implementation',
            'before': B, 'after': A, 'created': False, 'deleted': False, 'forced': False, 'head_commit': {'id': A}}


# Immutable public Windows390 runner metadata (not model/auth/history data).
# Captured run38296566758, 93346 bytes; original SHA256 below. Embedded compressed
# bytes avoid a test dependency on an untracked audit extraction. This preserves
# every original field, full filters and full commands, rather than synthetic padding.
CAPTURED_RUN_METADATA = (
    'c-'
    'rk<U31$;mVHlE{sYTTyH$}!AF*RkY91&`vb7^w8d0`)w^W6KfF_9uM02|VNu1f_x9_>PyFrkqY)K?IQ?)k_Nlbk7!R_1kt8X9w<H'
    'sL5omWX*q*)r5Dzc+E%8E%<zWQ_L)&9Gky?1ZoH*a@etKFMl277zIyc-PO?e4#Oy_4+i58^c0eVhEU|LP}xOPrKdoJUqyCgC@|;2'
    'pDvqa>b`l~Iv;$jT_uslw}a++C(B3S}Z~X@oCXk?MQ<YVJ~Ok`Z0}&o@8MUnt@+z3A2Zto%^jbh=5I>B4q)cj%wJpWnQjzdakrL%'
    '#n$EB5yMHEp9**~DZMdYG=BWr@*N56X^5gBNb^Z13&t|NM6L(48{&(FZ<WHJ;ht@lU0?EL0hujEwr9O4(AS^p1D9dw=)s{GB$v#a'
    '&kCRlqjP^-Y{_|7La3@$c#QM}O)Vo#)5e+{Y&E<WK-xUc@Ctw{&zj!H*6dZd)>8{*J5|=w+7^T@12eW#WMOQspYP%D<mrMH8ccQw'
    'i(k75;4hbefNkjUM+CLx{Ke$L++@eg3(DPAaR?XprTlGW6temFLsH;Uls^rqYklg+F>ga)=wpswjE<GyITutyR|2Z&43--'
    '#BQ8Do*pPP~8FEcVca&Qe4e16%@6wAN56S?L?dMe^(smB9DtpMW1U=ic$@gIjizAo8(!-'
    '*Uus9L9Xv#G!EZhPTym;X`Bzb(8gX=H+Xs1WaVhA$}Bn1Rgu~kz5g(S3~Zbq$61c`djV}!=1$oQgT-W~N5*Iam76`$Ez-ufuVYM}'
    'pMI4jxIU;FH=diuNcFT`Dhu(hI%$u6cP}mymFJIs+?9#50y5w!u%oV>7D?o1zXu~>`^sd>9#(Gp)upomC#l6l_y#;}FJc2UPx^vO'
    '1(aE-&Gh`fQ0d`BGk~lZ9u?_CXGO^?Y010(vUJ6LH2^+*0yyab7BDRqciXv1vjWrK$I51#djPwKL8D{HY1E$<<%sm|QVp<V?SUF-'
    'Ok-'
    'K9@r3lF7UpsGKu)1I(pjQjj`4slUb2br<(WrVs8dWU<~W{ARC=i^_``X0HFyv63ziFRI~ojVvNk5N_o}FH7tNpff+ef$B@B~O2ZF'
    'V#be=ndn5t~(^<SmuFjjaE{)g~+MfMX4!4~PpfSnX~7$bG!VClqGd&BAgZF=Uev?)Fnw}rk33~f?{N6Gw+;$o;s%Lmy5*$>;xNt7'
    'GI1*4G1)3eN4{0T<V;W;<i5clUF)g4V_;%%MUoV?EB2bfP?!0_b<Q;f{MViG5CU|li;u^DbqOjvlzW?q@0T{;PGxx*W6J+tIaoJc'
    'odq83qpf(7`X;UFkJF|784q<@=D;DhyD2CaKJtQtZe|Hhx)ZHpMbWC+}k9uP2|J0<TyZHAis*d;|4SBRZ~>`)D2h^lK$JcSJ#!w)'
    '&jnr;N3w@rT9Y6tsK7fDb$fc#=~F=U7cjYoDP$e{P9gOei3E1TV^V}n6xT3Mp?f?(6X_NffYAC}qYz38L5Z+Jd(#6j(yQ>JBMvIN'
    'R?LuU3=4dZ0`0WN9%=o+Z{co(~vQ%V)O2=*qWlhA24&PwZN_b5(A6fYx>V@s=&TUfTo1L$?Z3(g1XURtv0&T90G!t*e)gq#<rMJ%'
    'RBW_eE+B@(h0L1BcXG88NJ@)+Sb87-@h9x_XqPIYn%NJu##MEEB-Ygm1x$EB$tNtSbq;4b5P?*U2QS#Ur@X8%f;Sc*oyuaKw0TC*'
    'e(^OCn}M2BgM;6Bc>E(*5TOvO3H;#jYi#DuHU*@W5;DyM)W<)28DErL70ao<m463QF0v8SpSmLngj)=qN<a8uy^BOx8LpUT1R8<>'
    'cRGUb%L!WpuTahxAnRC?S*^6LG(`(Ed%D%jt*hHEzp(aqlLH>_}MHJlTIOGsP@w2AR|-'
    'u<%sdXGMrAv7%+&kvh1yzUIM<};2fl&6x%GRh|cd}1P4I|NK+2$TBNV2~vlC%}3<$rWa*PchITYB)E!b67UI8W~*;N0{^>ejw|cX'
    'Uo&nwL^BIy;3m#boGHfDQZ)7Xjgv$U~!U1A=8yIV|va-Ox-'
    ')3rJsuU4#u0l%BNl7r5lwzRhAG5^kBTRq{^#t1gVx#43+lF*p$^|`4_n_`KwYBOca!hFa#Y6!dW#3dco(MV11Vq!??IYn`mD2QT+'
    'ihyhjfRb8zY8e4`jYA;`jO&TnY_LH4z(r(X*=3NEM)&OVgoWJL@10?clb%US9cH61vc6kk4~SVKLO5tqv9uh}TrfN=p4vd}S&bZ%'
    'gU;VkeL4pt(chK>=2pais8!IEd~RdAf?^q{iS2rAI!YPc(D_?lm>2D~OiLcdX<z8DiqL_rOpmxV676;{5<{*FjkqoQT#tGs*Kw3@'
    'n=6jHFyyJ7uD&1&E0-'
    'Bq*b7l3o~>Ri<<MfCANkSj^AwhV}<CRk)@vmU`{>}xnpi)fd?m}){I?dJEKO}mJ6{k4(DT<bCLmMY{mu7Cv;lyLc6>=enhD=p2!&'
    '<|SQt|iLjjl()kZRXZ04Pegzz+_M|KaJr!gPzrx0S3|PThc_tf*S|tt8bqQDulR4Yk_w2EcViTIj*2vuDUdjCZOo1<BSbM<O&ZiL'
    '%gUA)p!BtJ!w}#TN{Te$>>JgE}-BZ@L!83ty`v7vAtzw8lcJqFy}=+ZMPgaLlm^A7S$!qB#=2Pi_d{%CaXof&qU-axNrhP4h(aeD'
    '(?~+FjrHbXJTsZe^GTP_C~^~Iav)m;<Ak07^G0!m~ED@4spST>f0*@ZqJ=GdKYGGaT6>&P^*!zAi2JX4LF-'
    '%zQWI6BkjkiIxOyrIhZx-b#0P8vmibbuwRxnp!t&cISLXngt7LzZY`s-'
    'qHKq&XVIniF(G7dnX(rVuL!EcaRD`y3fcz71!w17=J#4Y#CGI%TvP@%9@@5%1fev_lx!)0<PMfyHEPd5Mf>)+!%W+D&>mg%Wuy^r'
    '1%Dg@A!o*Hb61g)14^s7E)FGZ+mZx;p+Etjc&I2X!H=#p;8>}YngmK-4jb}H0ahLDJF^S5`fp`)5YB?-'
    'b(<h|Q`mf>c1+t)3yunrt>OkRpLb4BxNj%K@nAxPQwO4TECvz9_hEH-'
    'V{s8@=G<`bsLbY#tH(_JFurDr{+>HGpN*Q?MuKM~vKb{4)946QCF8;iE-'
    '}x?&Affknre9{Jd4mQ8iiJ3W9G;%aO)5%^$sw*rq&na0x~~#9S{1Tk;6qFE1sJ}Rb(pl9(_Z=77T@p&)lA;TN<0`w^=~t`_@8hSt'
    'EkAE_lV(kXki>{g!!Hz(c<Im~tJqOFdM`XfS)FZ&iVqkvABz-'
    '7+D2%FFuYsCvFHQ>A~qR|StyYfX5>O<BWskz_6R4<4Xew#mUi!Q&YcPuQu8<|5LS)#m$%4ytshO8$7L42CB}pSi-_>bSgiz3-'
    '&hWR_pGIxGwu8|Q|>N4@?SEwMLoa@*#}E=2Y<u&z_JwQd7H?sUjvhMGj^o3}1FJjxLhLI|N%iG9#N1I<G1D8#Dl1u0ggRndCaRKR'
    '7~7hXUkROxfEI`Ut=A?ev*8g-'
    'Cnx*GGo{w9UuvIkclC4a{`P0PNGUK=3Pn4$~n+{bOTL7mk^C{GUgY1@;v9Gnl)>*hM1G%75&+2pz$3RzftL}m4Qt<mb5#Mr>9)oQ'
    'blSc_&6NU(uEXyGZ$SL@q(2`U$KQ)Qvic;amK3C^ah*tziRrS{AUTg$d!xyT;F7^-'
    'Q_(#N{n;a@bxAy|J@s3b))AKrSbnn8aDByC|ohScpC9LM7<pB_=k$ZZ)A+8X^v8p6MUk~h+DtSmeTh-Wz50UHyOr~$FfMe_|{7eO'
    'iu^%39i@AvU&<Auw~{j$Z1*Ee9!ZN&RyG2UmmiT`I5%o%T3DUD+!Ar2x)wejKd#JW=46*1S|3|K=$#(I;f=N-'
    'WbGHl{{REFz_xP4RAe8^q*T!lSbtbqisj$1$Y1RXWexbtkB0`+vz#XhMu9l1jcrFh@8HI!e2`@HWxGWKfmvT!KpEY2~NWBeRi-'
    '^u9s*+$K5g0qyU%&y%Udf4v$ONB`T!nbh)LA`17Rnp*P+Jag9KEg1=T<)Y~D@bd8wXA8&?xiTvtAx9)_3g_4m7(DWl_C_}dCQ)I$'
    'A1}AE?xa6VQM-zhESLPZ78Vlu{JiXD@N2Wx6QQy!vE0WR{vdIziiVpF63<I2JzU@5I3!TIe(0)2`XH3$$pKKvwm8<uqfl0O*SuVm'
    'T1dJ=!6Iz=22gtWnib`inuL<@VjoI38OyjcS0-+b-'
    's2)lT@rfhF<^^4hr}KF^3tvILg@ccC@UNpCBG8;`vU;mcexM`~>w;zfsAHZgcr7<)$?f?;H_ob4?uw6G}cYag*gMZNRT#q0h_Ax5'
    '`pKH=pqfmFCww`|V7YFTO-Rl0G2?$#bl0YS-jItZ}~Iy#`9u!d})q9KYA-'
    'E8hOp0p;xElJSB>rH$0iZHe?%JZ>44Tk3NQoP)ucYoVF*vTSR~lV;E3a_W6Cnp*1ZAT@XGz<fgVG{#Yqk(Xp!E1sQ|I}qA0F&kqX'
    '1J&h0X39~QPJ=4dbAGs80AXLK=JB8-'
    's5XHC!bv#vI09Te_0!WXlBQ17E68nh_$_P8CWA!N4wA5UW1|boSrQy%vs_|uy$}q6OPwE3>f}9^8fPo*ar5%wJ=Utk3i^hmsK<u%'
    'h-R)`I1NAL^tT76R3PwTF}Z`d498(+5w?qn-KF=EzS`lD#Yj6C3%6T;n|x#qfR2j03@4<=%?*5ibmq|T^^%T#Q$r27Jf;UZ4w<@+'
    '5jF?ygQ}C;Ud$~R{$Q@>UGnf;w&r8z&v9P0rQl6u{shVRh7#P6U65yu3;d8zJyP1yqmvqVEz?o853D{zI#=>RGyEElA3hZeTc<qt'
    'R13Va7JRCY5S=suvfYJ8c*y7+QY04~3jNo>or6rM_&b{0n%2_$6Tep57g)cDCw8PuFBV@BC4&W;C&g~Q?(^I@np!GHvB3!&GmG=t'
    'q_>0T3^V1TvD!lU01^d&+pt;Ry`i{uwu$bH6fL~=QMrhbO4xMwELRMiI_^H-'
    'h*+rm))A4`0MINd@Bx9FwPRegO&xFF&A673Xx8$B_Fy8cgNWi>535k}=ROQSyO%W&UaOcWSJR2k**5t`;9AdIO2Ab@57>nWts)8y'
    'he(WQwKv)Ij&WLR^d$yA2DB`oT$M@WDHG@Vxmmps7Yr7ScwbTX1BEQcC%Lw55$PP25K?7d?x?ldS%O)q#uX;p#pA|s=c3Cj^phi('
    'jt8!#);W>Y7-*h=qdP>6a7fXTKkZ6kfLr|Dd9*5xrcrEeH}Wav%*-'
    'V!4wZBJE=g%QXLH;!w0LS&>BCN=^)9{2#tMNEg_mP&sANPV<a{B~9kz=Lb&uTH>p5uNwR#c;pRo97pk+a^D)|-'
    'fVe~6=hg&SKa87M&M6KmYwa*BrNqnLP$Awr33KcU?7x|6z*;(mU1GSmUNGsm5bC4R26xB7;RDEZ|KWggoO=c#%Rd^>j$qTQAgW^;'
    'oin)UBX#6O9!_+x(@*#L8D>;sBNqLrfpq7tzWygWH*W;j7Ast`nP(TlRTQP1ukuHuTOa~QL(>X3;73yhhT{1S~6Q@&c`CRDgaY=)'
    'C)XM^a(6B&qk|j29&D6Kln^}JI9(iR&hu&sqP5SUNu5?fz%}!@Lr>)`)%^g9^flR^k65CUKuW%j#%ar0Mu6_H;ra#G0HyEL_$5a1'
    'iW3-)>&*uK+^Nb4|uiv9ZeGbK{YZCB1&YHg)x2@yN&qulB$icGbJbMqg!Q%W>>j_oZ7hw2U%J5Nf)CP#4no{I2FI1wNrG)-'
    '?^wNEmXN~4<lFycC6q+={`i>q88of{9RH5?6zRVk8CYm{VpgK`j2DuP5xF@=E%)I_A3M$4J4(s3ZT@zi}t?EsaoSUE=_b}<_qh!B'
    'xFYn)HSU%ASmG82oMEFqg9^8x`@q93N+N+{j?*_Nd%(k$;%_;iF^E%I;`G#dZD#9)-?83q>EbPL<E-'
    'dWA!Y(ZA!on^r?83q>EbPL<E-dWA!Y(ZA!on^r?83q>EbPL<E-dWA!Y(ZA!on^r?83q>EbPL<E-dWA!Y(ZA!on^r?83q>EbPL<E-'
    'dWA!Y(ZA!on^r?83q>EbPL<E-dWA!Y(ZA!on^r?83q>EbPL<E-dWA!Y(ZA!on^r?83q>EbPL<E-dWA!Y(ZA!on^r?83q>EbPL<E-'
    'dWA!Y(ZA!on^r?83q>EbPL<E-dWA!Y(ZA!on^r?83q>EbPLI?7~k*3U)Q47-r5zc?>bpsGVYd{5JYzm*OMsoK0W?K!Qg-'
    '$xMHH4pke+-'
    '3hfJ%nA#$lhIDtj}t8Xv4R>t`E_6KK~MqMqOBt?vVpR03&N8!ljA2Q=Kjw9_Ri1#N1NV8rdC<HJk0e?oNxbz9gB|V*zLUA-'
    'r3&Yefv`f3}(5Xx()@r7Dtr;$G?nO2~fm{P=Ds3j`69;a5`|{oxlm*L4Cn?GCuEscf%%5JM>&0Pk!y7rk|Nk!n|r7$fbifb<p57r'
    'kB`GRb&VsJ7Jt{KQ7^jCf-'
    '5a6QEQNBTHL?sRfRI&|SE1;NakwH#<8&`5yx={JQ&gXNP|MeaUN6(xb*Z_ji`uq&fVL61Qa=jWOFKh5>DgZrvak-MV2r^6K$t$MH'
    'i%ciISNS0l@1KZ3UN^V`?Iyjk)b?og49DcFx#a0Ko<_YLDogtjTrD%an$_j>2ChYSu|<iuXZw=Khfmyn|+t)}n2cjdRO-'
    '}KFvW+IJWJ2A6qZ22d)7-<KSX{H_Hh}O~$R#Yc()*f>V8;oihtL2g*ZvmggL)?>78q^0U+keG{e^Y!`%>P7n_P=0jSp9nkx~!-'
    '&+3}Y;0bZdy7bl0Ef$#kABi>malQM-URi$gh;V(4Q#ALRDvTbR!$o<-'
    'J_ix?6bf~Q~v$S)SZp9SfLJ)3y7N4FwUZgFoWjk9tW0VHx)YUjsP2;7yEL7R?zjSz6|7od7NGv4u+pD16H&vFWjW_V0zyEw^Yb$A'
    'HmK|T>!S99M3c!B)YF&k4NDONl#r?tI-r916@}27^@yRbx$wv);Rqqnf-'
    '0)r*_jSR|RHlF<AWp7{Ud|%^oN+Gl*mE*i;&B;Lr%7IBlfXm&4YJDbkHfc@)A!VRhH*aV0!NWtuUZ$O=V;8muaxB37rozAs9XY48'
    'MT;waQXvpFnhvv$AxcS$C!@Ky-H}Y4ywkDI1D!%sh&1R&%5fRJ@(z-sf0OBcnT36V|~|7izLJrJ)_amqi8>(-'
    'n8(=kP}bu%{4A}rn}|8jzHMnh3#F~-i7U5*xrThUD)1*?OoX3h3#F~-i7U5*xrThUD)1*?OoX3h3#F~-'
    'i7U5*xrThUD)1*?OoX3h3#F~-i7U5*xrThUD)1*?OoX3h3#F~-i7U5*xrThUD)1*?OoX3h3#F~-i7U5*xrThUD)1*?OoX3h3#F~-'
    'i7U5*xrThUD)1*?OoX3h3#F~-i7U5*xrThUD)1*?OoX3h3#F~-i7U5*xrThUD)1*?OoX3h3#F~-i7U5*xrThUD)1*?OoX3h3#F~-'
    'v6Joy<fiq-!B5fA@lu~=Hb^<JtQa3m_6t4Hh$+D4O8`5ur3>J(w^d(!#`hN$Jf^pagSbK+nxQbz)X$kyIzFX*Zy7n9vJ-'
    'L3m(1p)X((zHb7s$dbNaL{9lV6nU8!f(BOWLIrcd$w62fBw}ER`4cxYlH$NZcl2aMWp7R7p{5JrcYCWM%hy@rv+%$Ze9_15Dr(94'
    ')2&zj}%vsj#PmnL&dV1Dq-fa7Xl18CPGi(yjhC-wFDdH?t{@9m!KgC=;M-Nm=%gP{^F412&Q|ZWy{c#-'
    '>)iE4Qr2ne`h*9oV^`@X{r#>-9&rD1BQ9k6km-p{8XnAx(70xUv5n)xlO*vzbJRc06_Nr*s-'
    'oo8Qvt7P#)2{yUyypC8zF}D#j}R~m0kaS=3jwnbFbe^*5HJe?vk))~0kaS=3jwnbFbe^*5HJe?vk))~0kaS=3jwnbFbe^*5HJe?v'
    'k))~0kaS=3jwnbFbe^*5HJe?vk))~0kaS=3jwnbFbe^*5HJe?vk))~0kaS=3jwnbFbe^*5HJe?vk))~0kaS=3jwnbFbe^*5HJe?v'
    'k))~0kaS=3jwnbFbe^*5HJe?vk))~0kaS=3jwnbFbe^*5HJe?vk))~0kaS=3jwnbFbe^*5HJe?vk))~0kaS=3jy;Y0rQiQf?drhh'
    'M99w9z#qtYNwbVzl}cGrT9oYXA_tJkl;~IGSlClL)FG{cS3Clv%<pcWV93Z;{*$Tte}QZe%;r55L5uRXzPfJY@n>$g7`LXbzp;D3'
    'vqRL*-MzK>t~lxR}*vf??3+V`;R~TABVq@fd'
)

def workflow_run_body(source, job_id, step_name):
    """Fixed workflow fixture extraction, not a general YAML parser/dependency.

    Resolve exactly one job and step, retain the complete literal-block value.
    Verification separately compares this value to the real full YAML parser.
    """
    jobs = list(re.finditer(r'^  ([A-Za-z0-9_-]+):\n', source, re.M))
    selected = [i for i, match in enumerate(jobs) if match[1] == job_id]
    assert len(selected) == 1, 'missing/duplicate workflow job'
    index = selected[0]
    section = source[jobs[index].end():jobs[index + 1].start() if index + 1 < len(jobs) else len(source)]
    candidates = list(re.finditer(r'^( +)- name: ([^\r\n]+)\n', section, re.M))
    assert candidates, 'missing workflow steps'
    margin = min(len(match[1]) for match in candidates)
    steps = [match for match in candidates if len(match[1]) == margin]
    selected = [i for i, match in enumerate(steps) if match[2] == step_name]
    assert len(selected) == 1, 'missing/duplicate workflow step'
    index = selected[0]
    step = section[steps[index].end():steps[index + 1].start() if index + 1 < len(steps) else len(section)]
    headers = list(re.finditer(r'^' + ' ' * (margin + 2) + r'run: \|\n', step, re.M))
    assert len(headers) == 1, 'missing/duplicate literal run header'
    lines = step[headers[0].end():].splitlines(keepends=True)
    first = next((line for line in lines if line.strip()), None)
    assert first is not None, 'empty/truncated run body'
    content_margin = len(first) - len(first.lstrip(' '))
    assert content_margin > margin + 2, 'invalid literal margin'
    decoded = []
    for line in lines:
        if line.strip():
            assert line.startswith(' ' * content_margin), 'truncated literal run body'
            decoded.append(line[content_margin:])
        else:
            decoded.append(line[content_margin:] if line.startswith(' ' * content_margin) else '\n')
    assert decoded and decoded[-1].endswith('\n'), 'truncated literal final newline'
    # The selected workflow uses plain | (clip); retain its single final LF.
    return ''.join(decoded).rstrip('\n') + '\n'


CAPTURED_806_WATCHDOG_SHA256 = 'e8d3e16fd5cccabe88f691f5ab0610a1385259584139aa432d957534fa0ebcb7'
CAPTURED_806_WATCHDOG = (
    'c%1E6O^@3)5WNS;e=u}v(b|wDTUi-sfuQRGY0+kpIB3x-'
    'AZT%Hb0d)|IkLS7^4|+mqAl5ycQ@Tb(dHyu^ZocZL+a!6XM=&Kk*>j;4ZM|1OAXBX*1bmidhzO)`89d{Zcz(%iWUVZ0(*`mmCPuO'
    'gGnV66!{ifGErt(B`lOoL2Hsqfs~6J1eyRlE|}cu;jLT_<?<F%^iK2@P~=ja8vgKR{&F!VZ{NMWng4n{za9*~AH39>uf<JexsX4}'
    '<!kHG$lic1b69K?P<9gHRxSxer0E8jHyfZT;amZflB%;JBSla-H}y-'
    '=p=mVI45%mzqJU~CZ0Qu!4U_93@*HG|L+9kM3{5+lT5yjrmIBEVR#F+9Cl55BD+H69f3Dvwfg(a);0YW}?G0r_mukzmQt4i;ze-S'
    'd_dr+YY4bCl{xlkcI>W}(Jl<F41~5isiLJm6Pg3JaG^A26<1_N>f(vd+iY8%D{f9lICDLUEq@cL5#F{0^jgo6c^TCfi1DzyHq5yQ'
    'y5$xfFK^mz-'
    '8<&)r8ZUt#M8mN^90!XaoCV{VACJOu9L90@jqlHVzxKd@vW$x!yU^NA!+3f%jaznw0`h@p8Q}~Ba=i0qgO3$br$zqbGM}Q*4j<_U'
    'yEI`OyqHWTeiYYcM*UM&rL8ar+(=WASMTi6&pgY%GG!}pn_U5XXQ=SC*RYf&7M3KQLrrR|U^5-'
    'AH1WOp@2?i~>srAPH<e<=Ro>GNPyrnGo`YS<U4y>^_r7~cs<%{uKvE0i;HoJUG`B7Id^MhANk4m8{XZ_c=vh_2Y2!FcQkfSS*imy'
    'p`BbObJ)FFvM>ojRt9r9{f_$<94&+pdjnofD`L}9nC5gV%2zeMJNuP&DLL3C~bn<A3E32o4xcX6bT+?$U(u~v`KQtCprAY0>nhjP'
    'o)6V~W@sDyYoJKVQ4#VjzLA$x$H@ZW|cDyiatd^U9r+c=$O#k0krY)eOO4qpLQiIysDc)#Chh5@nYO*k=wgVHq$_7~P3>y2RaTLY'
    'zxHW@10qleD0*Gdt<%OMKEr#CD;RnIn18}4Kod8A{=oAE_0?GYxPfavYkW((WSU=c#XD*EcQS0E=uE%IqN2}fpk1A(esSrQg@~)?'
    'La3y#gSE3+}$6@q^EB{BXgpcD&JedaL*#E+n|1MV=7q`|ZQ*uW#o^z8#FpWAiE&}E;PS-'
    '+e<musTc0EfHnk2;BUXw)o;n00w*n;9Kp4!2fB=y&hB-v^NjYH#Vl*zSs<{9=}+iU4DE~@?K2dsaLQ0={=yF-'
    '*Ce?5)NTw<A`TpR#s-'
    '$yhuQ0k&eN((F%ki7aH<E{7}*l>A=FkF_a6)3N*SG^=g!5%WF(=ck~lhZv(H`Fq0>`$(yFQQg5m*mdgdGE9DlN<XwQFaItM$?vqG'
    'X=lf-UaEkcM5*XzPsBv<9?~JHQ$~<U0%%lbQ*rnPl0V8=kn@8TodM8eE7Q8+QgrpKl>XIM-Cw'
)


class GitTranscript:
    """Literal Git responses, never Git init/clone or an installed capability fixture."""
    def __init__(self, directory):
        self.before = Path(directory) / 'before'
        self.after = Path(directory) / 'after'
        self.evidence = Path(directory) / 'evidence'
        self.evidence.mkdir()
        for root in (self.before, self.after):
            (root / '.git').mkdir(parents=True)
        self.files, self.raw, self.calls = {}, {}, []
        self.heads = {self.before: B, self.after: A}
        self.add(self.before, B, '.github/workflows/s03-windows-verification.yml', b'name: Old\n')
        self.add(self.after, A, '.github/workflows/s03-windows-verification.yml', b'name: New\n')
        self.add(self.after, A, '.kiro/xiigen-slices/luna/s04/windows-verification-request.json', NEW)
        self.add(self.after, A, 'tools/s04_windows_verification.py', (ROOT / 'tools/s04_windows_verification.py').read_bytes())

    def add(self, root, commit, path, raw, mode='100644', physical=True):
        oid = hashlib.sha1(b'blob ' + str(len(raw)).encode() + b'\0' + raw).hexdigest()
        self.files[root, commit, path] = (mode, oid)
        self.raw[oid] = raw
        if physical:
            disk = root / path
            disk.parent.mkdir(parents=True, exist_ok=True)
            disk.write_bytes(raw)
        return oid

    def git(self, root, args, maximum=4096):
        self.calls.append((root, args, maximum))
        if args == ['rev-parse', '--verify', 'HEAD']:
            return (self.heads[root] + '\n').encode()
        if args[0] == 'ls-tree':
            found = self.files.get((root, args[2], args[4]))
            return b'' if found is None else (found[0] + ' blob ' + found[1] + '\t' + args[4] + '\0').encode()
        if args[:2] == ['cat-file', '-s']:
            return (str(len(self.raw[args[2]])) + '\n').encode()
        if args[:2] == ['cat-file', 'blob']:
            return self.raw[args[2]]
        raise AssertionError(('unexpected Git command', args))


class VerificationTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(prefix='s04-pure-origin-')
        self.addCleanup(self.temp.cleanup)
        self.f = GitTranscript(self.temp.name)
        self.git = patch.object(m, 'git', self.f.git)
        self.git.start()
        self.addCleanup(self.git.stop)

    def receipt(self):
        return m.admission(event(), m.context(environment()), self.f.before, self.f.after, 'd' * 64)

    def push_env(self, receipt, raw):
        env = environment('windows-verification')
        env.update(ADMISSION_ADMITTED='true', ADMISSION_AFTER=A, ADMISSION_BEFORE=B,
                   ADMISSION_REQUEST_ID=receipt['after_marker']['request_id'], ADMISSION_RUN_ID='98765', ADMISSION_RUN_ATTEMPT='3',
                   ADMISSION_LOGICAL_JOB='push-admission', ADMISSION_WORKFLOW_SHA=A, ADMISSION_RECEIPT_SHA256=m.digest(raw))
        env['ADMISSION_EVENT_CANONICAL_SHA256'] = receipt['event_canonical_sha256']
        return env

    def budget_cli_fixture(self, manual_gate=None):
        if manual_gate is None:
            receipt = self.receipt(); raw = json.dumps(receipt).encode()
            env = self.push_env(receipt, raw); payload = event()
            receipt_path = self.f.evidence / 'receipt.json'; receipt_path.write_bytes(raw)
        else:
            env = environment('windows-verification', 'workflow_dispatch', W)
            env.update(GITHUB_SHA=B, GATE_NAME=manual_gate)
            payload = {'repository': {'full_name': 'owner/project', 'id': 17}, 'inputs': {'candidate_sha': A, 'gate': manual_gate}}
            self.f.add(self.f.after, W, m.HELPER, Path(m.__file__).read_bytes(), physical=False)
            # C has no helper; the real CLI must use the authenticated executing W bytes.
            self.f.files.pop((self.f.after, A, m.HELPER))
            raw = None; receipt_path = None
        event_raw = json.dumps(payload).encode()
        event_path = self.f.evidence / 'event.json'; event_path.write_bytes(event_raw)
        env['GITHUB_EVENT_PATH'] = str(event_path)
        origin = m.windows_origin(payload, m.context(env), self.f.after, Path(m.__file__), raw, env, m.digest(event_raw))
        (self.f.evidence / 'verification-origin.json').write_text(json.dumps(origin))
        args = ['helper', 'focused-budget', '--root', str(self.f.after), '--helper-disk', m.__file__, '--evidence-root', str(self.f.evidence)]
        if receipt_path: args += ['--receipt', str(receipt_path)]
        return env, origin, args

    def invoke_cli(self, env, args, suffix):
        output = self.f.evidence / (suffix + '.json')
        with patch.dict(os.environ, env, clear=True), patch.object(sys, 'argv', args + ['--output', str(output)]), redirect_stdout(io.StringIO()), redirect_stderr(io.StringIO()):
            result = m.main()
        return result, json.loads(output.read_bytes()) if output.exists() else None

    def test_closed_focused_policy_preserves_every_normal_gate_and_rejects_arbitrary_types(self):
        gates = ['S03.%02d' % n for n in range(1,13)] + ['S03.08.PERF','S04.02A.C01-IDENTITY-SUBSTRATE']
        self.assertEqual(14, len(gates))
        for gate in gates:
            with self.subTest(gate=gate): self.assertEqual(1800 if gate == 'S03.04' else 600, m.focused_policy(gate))
        for gate in [m.NATIVE_GATE, 'S03.13', 's03.04', 'S03.04 ', '', None, True, 1800, 600.0, [], {}]:
            with self.subTest(gate=gate), self.assertRaises(m.Refused): m.focused_policy(gate)

    def test_actual_budget_cli_revalidates_push_origin_receipt_and_closed_gate(self):
        env, origin, args = self.budget_cli_fixture()
        status, selected = self.invoke_cli(env,args,'selected')
        self.assertEqual(0,status); self.assertEqual(1800,selected['focused_seconds'])
        self.assertEqual(m.canonical_event_digest(origin), selected['origin_sha256'])
        for key,value in [('GATE_NAME','S03.01'),('ADMISSION_RUN_ATTEMPT','4'),('GITHUB_REPOSITORY_ID','18')]:
            bad = dict(env); bad[key] = value
            with self.subTest(key=key): self.assertEqual((2,None),self.invoke_cli(bad,args,'bad-'+key))
        p = self.f.evidence / 'verification-origin.json'
        p.write_text(json.dumps(dict(origin,gate='S03.01')))
        self.assertEqual((2,None),self.invoke_cli(env,args,'forged-origin'))
        p.write_text(json.dumps(origin)); bad = dict(env); bad['FOCUSED_WATCHDOG_SECONDS']='1'
        self.assertEqual(1800,self.invoke_cli(bad,args,'ignored-env')[1]['focused_seconds'])
        with patch.object(sys,'argv',args+['--seconds','1','--output',str(self.f.evidence/'caller-seconds.json')]), redirect_stderr(io.StringIO()), self.assertRaises(SystemExit) as error:
            m.main()
        self.assertEqual(2,error.exception.code)

    def test_actual_budget_cli_manual_old_candidate_absent_helper_uses_distinct_W(self):
        env, origin, args = self.budget_cli_fixture('S03.01')
        status, selected = self.invoke_cli(env,args,'manual-selected')
        self.assertEqual((0,600),(status,selected['focused_seconds']))
        self.assertEqual((A,B,W),(origin['resolved_candidate'],origin['event_sha'],origin['workflow_sha']))
        self.assertFalse((self.f.after,A,m.HELPER) in self.f.files)
        self.assertTrue(any(call[1][:3] == ['ls-tree','-z',W] for call in self.f.calls))
        event_path = Path(env['GITHUB_EVENT_PATH']); payload=json.loads(event_path.read_bytes())
        payload['inputs']['gate']=m.NATIVE_GATE; event_path.write_text(json.dumps(payload)); env['GATE_NAME']=m.NATIVE_GATE
        # Even a genuinely correlated manual native origin cannot enter the normal focused lane.
        native=m.windows_origin(payload,m.context(env),self.f.after,Path(m.__file__),None,env,m.digest(event_path.read_bytes()))
        (self.f.evidence/'verification-origin.json').write_text(json.dumps(native))
        self.assertEqual((2,None),self.invoke_cli(env,args,'native-refused'))

    def test_focused_correspondence_is_strict_numeric_origin_bound_and_not_outcome_acceptance(self):
        _, origin, _ = self.budget_cli_fixture()
        selected=m.focused_budget(origin)
        metadata={'gate':'S03.04','test_process_watchdog_seconds':{'focused':1800,'impacted':1800}}
        watchdog={'stage':'focused','watchdog_timeout_seconds':1800,'state':'TIMED_OUT','timed_out':True,'wrapper_exit_code':124,'native_exit_code':None}
        m.validate_focused_correspondence(origin,metadata,watchdog,selected,dict(selected))
        self.assertEqual(('TIMED_OUT',124,None),(watchdog['state'],watchdog['wrapper_exit_code'],watchdog['native_exit_code']))
        for value in [None, True, False, '1800',1800.0,-1800,0,600]:
            for field in ['declared','observed','receipt']:
                a,b,c=copy.deepcopy(metadata),copy.deepcopy(watchdog),dict(selected)
                if field=='declared': a['test_process_watchdog_seconds']['focused']=value
                elif field=='observed': b['watchdog_timeout_seconds']=value
                else: c['focused_seconds']=value
                with self.subTest(field=field,value=value),self.assertRaises(m.Refused):m.validate_focused_correspondence(origin,a,b,c,selected)
        for bad in [dict(selected,gate='S03.01'),dict(selected,origin_sha256='0'*64),dict(selected,extra=True),{}]:
            with self.assertRaises(m.Refused):m.validate_focused_correspondence(origin,metadata,watchdog,selected,bad)
        for bad in [{},dict(metadata,gate='S03.01'),dict(metadata,test_process_watchdog_seconds={})]:
            with self.assertRaises(m.Refused):m.validate_focused_correspondence(origin,bad,watchdog,selected,selected)

    def test_actual_strict_cli_checks_full_metadata_captured_806_budget_and_duplicate_numeric_fields(self):
        env,origin,args=self.budget_cli_fixture()
        status,selected=self.invoke_cli(env,args,'focused-budget')
        self.assertEqual(0,status)
        focus=self.f.evidence/'focused';focus.mkdir()
        (focus/'budget.json').write_text(json.dumps(selected))
        for name in ('initialization.json','source-sha.json','workflow-identity.json'):
            record={k:origin[k] for k in ('trigger_kind','candidate_origin','raw_dispatch_candidate','resolved_candidate')}
            record['actual_sha' if name=='source-sha.json' else 'actual_source_sha']=A
            if name=='workflow-identity.json':record.update(verification_origin=origin,executed_helper=origin['executed_helper'])
            (self.f.evidence/name).write_text(json.dumps(record))
        (self.f.evidence/'executed-helper-materialization.json').write_text(json.dumps({'workflow_sha':A,'executed_helper':origin['executed_helper']}))
        legacy_raw=zlib.decompress(base64.b85decode(CAPTURED_RUN_METADATA))
        self.assertEqual('fee5054fecf05724dccbdedc5f82a3147123630774102ef78587df4c24baa3fe',m.digest(legacy_raw))
        current=json.loads(legacy_raw.decode('utf-8-sig'))
        # Explicit fixture transformation: historical raw remains unchanged; this is not Windows runtime proof.
        current.update({k:origin[k] for k in ('trigger_kind','candidate_origin','raw_dispatch_candidate','resolved_candidate','actual_source_sha')})
        current['test_process_watchdog_seconds']['focused']=1800
        metadata=json.dumps(current).encode();self.assertGreater(len(metadata),93000)
        (self.f.evidence/'run-metadata.json').write_bytes(metadata)
        captured=zlib.decompress(base64.b85decode(CAPTURED_806_WATCHDOG))
        self.assertEqual(CAPTURED_806_WATCHDOG_SHA256,m.digest(captured))
        watchdog=json.loads(captured.decode('utf-8-sig'));self.assertEqual(('TIMED_OUT',600,124),(watchdog['state'],watchdog['watchdog_timeout_seconds'],watchdog['wrapper_exit_code']))
        (focus/'watchdog.json').write_bytes(captured)
        strict_args=args.copy();strict_args[1]='check-records'
        self.assertEqual((2,None),self.invoke_cli(env,strict_args,'historical-600-refused'))
        controlled=copy.deepcopy(watchdog);controlled['watchdog_timeout_seconds']=1800
        raw=json.dumps(controlled).encode();(focus/'watchdog.json').write_bytes(raw)
        self.assertEqual(0,self.invoke_cli(env,strict_args,'current-correspondence')[0])
        # check-records grants only origin/budget correspondence, never outcome acceptance.
        self.assertEqual(('TIMED_OUT',124,None),(controlled['state'],controlled['wrapper_exit_code'],controlled['native_exit_code']))
        (focus/'watchdog.json').write_bytes(raw.replace(b'"watchdog_timeout_seconds": 1800',b'"watchdog_timeout_seconds": 1800,"watchdog_timeout_seconds": 1800'))
        self.assertEqual((2,None),self.invoke_cli(env,strict_args,'duplicate-refused'))
        (focus/'watchdog.json').write_bytes(raw)
        (self.f.evidence/'run-metadata.json').write_bytes(metadata.replace(b'"focused": 1800',b'"focused": "1800"',1))
        self.assertEqual((2,None),self.invoke_cli(env,strict_args,'string-refused'))
        self.assertEqual(captured,zlib.decompress(base64.b85decode(CAPTURED_806_WATCHDOG)))

    def test_actual_workflow_budget_selection_feeds_wait_metadata_and_observation(self):
        source=(ROOT/'.github/workflows/s03-windows-verification.yml').read_text()
        metadata=workflow_run_body(source,'windows-verification','Capture runner SDK and command configuration')
        focused=workflow_run_body(source,'windows-verification','Run focused admitted gate')
        strict=workflow_run_body(source,'windows-verification','Generate strict validation report')
        for body in (metadata,focused):
            self.assertEqual(1,body.count("'focused-budget'"))
            self.assertIn("'--helper-disk',$env:VERIFICATION_HELPER",body)
            self.assertIn("'push-admission/push-admission.json'",body)
            self.assertIn("if ($LASTEXITCODE -ne 0) { throw 'Focused policy origin refused.' }",body)
        self.assertLess(focused.index("'focused-budget'"),focused.index('$processJob = [S03ProcessJob]::Create()'))
        self.assertIn('focused = $focusedWatchdogSeconds;',metadata)
        self.assertIn('.AddSeconds($focusedWatchdogSeconds)',focused)
        self.assertIn('[TimeSpan]::FromSeconds($focusedWatchdogSeconds)',focused)
        self.assertIn('watchdog_timeout_seconds = $focusedWatchdogSeconds',focused)
        self.assertNotIn('AddMinutes(10)',focused);self.assertNotIn('FromMinutes(10)',focused)
        self.assertIn("'check-records'",strict)
        self.assertIn("$state -ne 'COMPLETED_GREEN'",strict)
        self.assertIn('$exitCode -ne 0',strict)
        self.assertIn('if ($trxFiles.Count -ne 1)',strict)

    def test_exact_absent_before_admits_changed_raw_marker_and_all_blob_identities(self):
        r = self.receipt()
        self.assertEqual({'present': False}, r['before_marker'])
        self.assertEqual(A, r['actual_after_head'])
        self.assertEqual(B, r['actual_before_head'])
        self.assertEqual(hashlib.sha256(NEW).hexdigest(), r['after_marker']['sha256'])
        self.assertEqual('push-admission', r['logical_job'])
        self.assertEqual(21, len(r))
        self.assertTrue(any(c[1][:2] == ['cat-file', 'blob'] for c in self.f.calls))

    def test_existing_before_new_uuid_and_unchanged_uuid_rejection(self):
        self.f.add(self.f.before, B, m.MARKER, OLD)
        self.assertNotEqual(self.receipt()['before_marker']['request_id'], self.receipt()['after_marker']['request_id'])
        self.f.add(self.f.before, B, m.MARKER, NEW)
        with self.assertRaisesRegex(m.Refused, 'unchanged request UUID'):
            self.receipt()

    def test_unknown_malformed_before_or_missing_after_is_never_absence(self):
        for raw in (b'{', b'{"schema":"unknown"}', NEW + b'false', b'\xff'):
            with self.subTest(raw=raw[:16]):
                self.f.add(self.f.before, B, m.MARKER, raw)
                with self.assertRaises(m.Refused): self.receipt()
        self.f.files.pop((self.f.after, A, m.MARKER))
        with self.assertRaises(m.Refused): self.receipt()

    def test_marker_closed_finite_strict_utf8_duplicate_and_uuid_rules(self):
        wrong = [NEW[:-2] + b',"extra":1}', NEW.replace(b'"gate":"S03.04"', b'"gate":"S03.04","gate":"S03.04"'),
                 b'\xef\xbb\xbf' + NEW, b'\xff', NEW + b' ' * 4096, NEW.replace(b'"S03.04"', b'false'),
                 NEW.replace(b'a5fb205a-27d2-42e4-8931-886dbab3cefe', b'A5FB205A-27D2-42E4-8931-886DBAB3CEFE'),
                 NEW.replace(b'push-event-after', b'candidate-sha-input'), b'{"schema":NaN}']
        self.assertEqual('a5fb205a-27d2-42e4-8931-886dbab3cefe', m.marker(NEW)['request_id'])
        for raw in wrong:
            with self.subTest(raw=raw[:40]), self.assertRaises(m.Refused): m.marker(raw)

    def test_push_event_foreign_flags_heads_and_context_reject(self):
        for path, value in [(('forced',), True), (('created',), True), (('deleted',), True), (('forced',), 0),
                            (('before',), '0' * 40), (('after',), B), (('before',), A),
                            (('head_commit', 'id'), B), (('repository', 'id'), 18), (('ref',), 'refs/heads/main')]:
            changed = event()
            target = changed
            for key in path[:-1]: target = target[key]
            target[path[-1]] = value
            with self.subTest(path=path, value=value), self.assertRaises(m.Refused):
                m.push_event(changed, m.context(environment()))
        with self.assertRaises(m.Refused): m.push_event(event(), m.context(environment(workflow=W)))

    def test_git_modes_size_oid_disk_drift_and_checkout_head_refuse(self):
        key = (self.f.after, A, m.MARKER)
        original = self.f.files[key]
        for mode in ('120000', '160000', '040000'):
            self.f.files[key] = (mode, original[1])
            with self.subTest(mode=mode), self.assertRaises(m.Refused): self.receipt()
        self.f.files[key] = original
        raw = self.f.raw[original[1]]
        self.f.raw[original[1]] = raw + b' '
        with self.assertRaises(m.Refused): self.receipt()
        self.f.raw[original[1]] = raw
        (self.f.after / m.MARKER).write_bytes(OLD)
        with self.assertRaisesRegex(m.Refused, 'disk/Git'): self.receipt()
        (self.f.after / m.MARKER).write_bytes(NEW)
        self.f.heads[self.f.after] = W
        with self.assertRaisesRegex(m.Refused, 'checkout HEAD'): self.receipt()

    def test_git_reader_failure_cannot_be_known_absence(self):
        with patch.object(m, 'git', side_effect=m.Refused('Git read failed/overbound')):
            with self.assertRaisesRegex(m.Refused, 'Git read failed'): self.receipt()

    def test_windows_push_rereads_after_helper_event_receipt_outputs(self):
        r = self.receipt()
        raw = json.dumps(r).encode()
        env = self.push_env(r, raw)
        origin = m.windows_origin(event(), m.context(env), self.f.after, self.f.after / m.HELPER, raw, env, 'd' * 64)
        self.assertIsNone(origin['raw_dispatch_candidate'])
        self.assertFalse(origin['before_read_by_windows'])
        self.assertEqual('push-event-after', origin['candidate_origin'])
        self.assertEqual(r['executed_helper'], origin['executed_helper'])
        for key in ('ADMISSION_RUN_ID', 'ADMISSION_RUN_ATTEMPT', 'ADMISSION_RECEIPT_SHA256', 'ADMISSION_LOGICAL_JOB'):
            bad = dict(env, **{key: 'wrong'})
            with self.subTest(key=key), self.assertRaises(m.Refused):
                m.windows_origin(event(), m.context(bad), self.f.after, self.f.after / m.HELPER, raw, bad, 'd' * 64)

    def test_receipt_foreign_run_attempt_event_job_marker_and_helper_refuse(self):
        r = self.receipt()
        for key, value in (('run_id', '123'), ('run_attempt', '1'), ('logical_job', 'windows-verification'),
                           ('event_canonical_sha256', 'f' * 64), ('workflow_sha', W), ('admitted', False), ('before_marker', {'present': None}), ('before_marker', {'present': 0})):
            bad = copy.deepcopy(r); bad[key] = value
            with self.subTest(key=key), self.assertRaises(m.Refused):
                m.validate_receipt(bad, event(), m.context(environment('windows-verification')))

    def test_cross_platform_full_event_binding_accepts_format_not_changed_values(self):
        e = event()
        e['unknown_payload'] = {'text': '章', 'array': [1, None, True], 'extra': {'z': 'value', 'a': 2}}
        linux_raw = (json.dumps(e, ensure_ascii=False, indent=2) + '\n').encode('utf-8')
        reordered = dict(reversed(list(e.items())))
        windows_raw = (json.dumps(reordered, ensure_ascii=True, indent=4) + '\r\n').replace('\n', '\r\n').encode('utf-8')
        linux = m.strict_json(linux_raw, m.MAX_EVENT)
        windows = m.strict_json(windows_raw, m.MAX_EVENT)
        self.assertNotEqual(m.digest(linux_raw), m.digest(windows_raw))
        r = m.admission(linux, m.context(environment()), self.f.before, self.f.after, m.digest(linux_raw))
        receipt_raw = json.dumps(r).encode()
        env = self.push_env(r, receipt_raw)
        origin = m.windows_origin(windows, m.context(env), self.f.after, self.f.after / m.HELPER, receipt_raw, env, m.digest(windows_raw))
        self.assertEqual(m.digest(linux_raw), origin['push_admission']['event_sha256'])
        self.assertEqual(m.digest(windows_raw), origin['event_sha256'])
        self.assertEqual(origin['event_canonical_sha256'], r['event_canonical_sha256'])
        changed = copy.deepcopy(windows)
        changed['unknown_payload']['extra']['z'] = 'changed'
        with self.assertRaisesRegex(m.Refused, 'full canonical event'):
            m.windows_origin(changed, m.context(env), self.f.after, self.f.after / m.HELPER, receipt_raw, env, m.digest(windows_raw))

    def test_manual_old_candidate_missing_helper_uses_exact_workflow_blob(self):
        env = environment('windows-verification', 'workflow_dispatch', W)
        env['GITHUB_SHA'] = B
        env['GATE_NAME'] = 'S03.01'
        self.f.files.pop((self.f.after, A, m.HELPER))
        # The physical candidate helper is absent. W exists only as a controlled Git blob.
        candidate_helper = self.f.after / m.HELPER
        candidate_helper.unlink()
        raw = (ROOT / 'tools/s04_windows_verification.py').read_bytes()
        self.f.add(self.f.after, W, m.HELPER, raw, physical=False)
        disk = self.f.evidence / 's04_windows_verification.executed.py'
        disk.write_bytes(raw)
        e = {'repository': {'full_name': 'owner/project', 'id': 17}, 'inputs': {'candidate_sha': A, 'gate': 'S03.01'}}
        origin = m.windows_origin(e, m.context(env), self.f.after, disk, None, env, 'd' * 64)
        self.assertFalse(candidate_helper.exists())
        self.assertEqual((A, B, W), (origin['resolved_candidate'], origin['event_sha'], origin['workflow_sha']))
        self.assertEqual(A, origin['raw_dispatch_candidate'])
        self.assertTrue(any(c[1][:3] == ['ls-tree', '-z', W] for c in self.f.calls))
        self.assertFalse(any(c[1][:3] == ['ls-tree', '-z', A] and c[1][-1] == m.HELPER for c in self.f.calls))
        disk.write_bytes(raw + b'# changed\n')
        with self.assertRaisesRegex(m.Refused, 'disk/Git'):
            m.windows_origin(e, m.context(env), self.f.after, disk, None, env, 'd' * 64)

    def test_producer_null_manual_origin_and_materialized_bytes_are_binding(self):
        r = self.receipt(); raw = json.dumps(r).encode(); env = self.push_env(r, raw)
        o = m.windows_origin(event(), m.context(env), self.f.after, self.f.after / m.HELPER, raw, env, 'd' * 64)
        records = {}
        for name in ('initialization.json', 'source-sha.json', 'run-metadata.json', 'workflow-identity.json'):
            p = {k: o[k] for k in ('trigger_kind', 'candidate_origin', 'raw_dispatch_candidate', 'resolved_candidate')}
            p['actual_sha' if name == 'source-sha.json' else 'actual_source_sha'] = A
            if name == 'workflow-identity.json': p.update(verification_origin=o, executed_helper=o['executed_helper'])
            records[name] = p
            (self.f.evidence / name).write_text(json.dumps(p))
        (self.f.evidence / 'executed-helper-materialization.json').write_text(json.dumps({'workflow_sha': A, 'executed_helper': o['executed_helper']}))
        m.check_records(o, self.f.evidence)
        for name, p in records.items():
            bad = dict(p, raw_dispatch_candidate=A)
            (self.f.evidence / name).write_text(json.dumps(bad))
            with self.subTest(name=name), self.assertRaises(m.Refused): m.check_records(o, self.f.evidence)
            (self.f.evidence / name).write_text(json.dumps(p))

    def test_full_captured_run_metadata_is_bounded_complete_and_origin_checked(self):
        legacy_raw = zlib.decompress(base64.b85decode(CAPTURED_RUN_METADATA))
        self.assertEqual(93346, len(legacy_raw))
        self.assertEqual('fee5054fecf05724dccbdedc5f82a3147123630774102ef78587df4c24baa3fe', hashlib.sha256(legacy_raw).hexdigest())
        legacy = json.loads(legacy_raw.decode('utf-8-sig'))
        self.assertEqual((22498, 22907), (len(legacy['focused_filter']), len(legacy['impacted_filter'])))
        self.assertIn(legacy['focused_filter'], legacy['commands']['focused'])
        self.assertIn(legacy['impacted_filter'], legacy['commands']['impacted'])
        r = self.receipt(); receipt_raw = json.dumps(r).encode(); env = self.push_env(r, receipt_raw)
        origin = m.windows_origin(event(), m.context(env), self.f.after, self.f.after / m.HELPER, receipt_raw, env, 'd' * 64)
        for name in ('initialization.json', 'source-sha.json', 'workflow-identity.json'):
            record = {k: origin[k] for k in ('trigger_kind', 'candidate_origin', 'raw_dispatch_candidate', 'resolved_candidate')}
            record['actual_sha' if name == 'source-sha.json' else 'actual_source_sha'] = A
            if name == 'workflow-identity.json': record.update(verification_origin=origin, executed_helper=origin['executed_helper'])
            (self.f.evidence / name).write_text(json.dumps(record))
        (self.f.evidence / 'executed-helper-materialization.json').write_text(json.dumps({'workflow_sha': A, 'executed_helper': origin['executed_helper']}))
        # Preserve the full captured producer, adding only this controlled current origin.
        current = copy.deepcopy(legacy)
        current.update({k: origin[k] for k in ('trigger_kind', 'candidate_origin', 'raw_dispatch_candidate', 'resolved_candidate', 'actual_source_sha')})
        raw = (json.dumps(current, ensure_ascii=False, indent=2) + '\n').encode('utf-8')
        path = self.f.evidence / 'run-metadata.json'
        path.write_bytes(raw)
        self.assertGreater(len(raw), 93000)
        self.assertGreater(len(raw), m.MAX_RECEIPT)
        # Observe actual production full read AND full parse, including late command/filter fields.
        with patch.object(m, 'read_file', wraps=m.read_file) as reads, patch.object(m, 'strict_json', wraps=m.strict_json) as parses:
            m.check_records(origin, self.f.evidence)
            reads.assert_any_call(path, m.MAX_RUN_METADATA)
            parses.assert_any_call(raw, m.MAX_RUN_METADATA)
        self.assertEqual(raw, path.read_bytes())
        parsed = json.loads(path.read_bytes())
        self.assertEqual(legacy['commands'], parsed['commands'])
        self.assertEqual(legacy['focused_filter'], parsed['focused_filter'])
        self.assertEqual(legacy['impacted_filter'], parsed['impacted_filter'])
        bad = dict(current, raw_dispatch_candidate=A)
        path.write_text(json.dumps(bad))
        with self.assertRaisesRegex(m.Refused, 'producer origin binding'):
            m.check_records(origin, self.f.evidence)
        # A real full JSON payload beyond the finite limit refuses; no truncation/fallback.
        overbound = raw + b' ' * (m.MAX_RUN_METADATA + 1 - len(raw))
        path.write_bytes(overbound)
        with self.assertRaisesRegex(m.Refused, 'regular bounded file'):
            m.check_records(origin, self.f.evidence)
        self.assertEqual(overbound, path.read_bytes())
        # The three other producers retain their tighter bound even when metadata is valid.
        path.write_bytes(raw)
        small = self.f.evidence / 'initialization.json'
        small_raw = small.read_bytes()
        small.write_bytes(small_raw + b' ' * (m.MAX_RECEIPT + 1 - len(small_raw)))
        with self.assertRaisesRegex(m.Refused, 'regular bounded file'):
            m.check_records(origin, self.f.evidence)

    def test_actual_workflow_materializer_executes_fixed_W_even_when_C_helper_absent(self):
        source = (ROOT / '.github/workflows/s03-windows-verification.yml').read_text()
        body = workflow_run_body(source, 'windows-verification', 'Materialize and execute fixed helper from executing workflow W')
        tree = ast.parse(body)
        start = next(i for i, node in enumerate(tree.body) if isinstance(node, ast.Assign) and any(isinstance(t, ast.Name) and t.id == 'tree' for t in node.targets))
        raw = (ROOT / 'tools/s04_windows_verification.py').read_bytes()
        self.f.files.pop((self.f.after, A, m.HELPER))
        (self.f.after / m.HELPER).unlink()
        self.f.add(self.f.after, W, m.HELPER, raw, physical=False)
        env = environment('windows-verification', 'workflow_dispatch', W)
        env.update(GITHUB_WORKSPACE=str(self.f.after), EVIDENCE_ROOT=str(self.f.evidence), GITHUB_ENV=str(self.f.evidence / 'github-env'))
        namespace = {'__name__': 'actual_workflow_bootstrap'}
        class Executed(Exception): pass
        with patch.dict(os.environ, env), patch.object(os, 'execv', side_effect=Executed) as execute:
            exec(compile(ast.Module(body=tree.body[:start], type_ignores=[]), '<workflow bootstrap>', 'exec'), namespace)
            namespace['git'] = self.f.git
            with self.assertRaises(Executed):
                exec(compile(ast.Module(body=tree.body[start:], type_ignores=[]), '<workflow bootstrap>', 'exec'), namespace)
        args = execute.call_args.args[1]
        self.assertEqual(['-I', '-B'], args[1:3])
        self.assertEqual(str(self.f.evidence / 's04_windows_verification.executed.py'), args[3])
        self.assertEqual('windows', args[4])
        self.assertNotIn('--receipt', args)
        self.assertFalse((self.f.after / m.HELPER).exists())
        materialized = json.loads((self.f.evidence / 'executed-helper-materialization.json').read_text())
        self.assertEqual(W, materialized['workflow_sha'])
        self.assertEqual(hashlib.sha256(raw).hexdigest(), materialized['executed_helper']['sha256'])

    def test_actual_workflow_materializer_refuses_changed_bytes_before_execution(self):
        source = (ROOT / '.github/workflows/s03-windows-verification.yml').read_text()
        tree = ast.parse(workflow_run_body(source, 'windows-verification', 'Materialize and execute fixed helper from executing workflow W'))
        start = next(i for i, node in enumerate(tree.body) if isinstance(node, ast.Assign) and any(isinstance(t, ast.Name) and t.id == 'tree' for t in node.targets))
        raw = (ROOT / 'tools/s04_windows_verification.py').read_bytes()
        self.f.add(self.f.after, W, m.HELPER, raw, physical=False)
        env = environment('windows-verification', 'workflow_dispatch', W)
        env.update(GITHUB_WORKSPACE=str(self.f.after), EVIDENCE_ROOT=str(self.f.evidence), GITHUB_ENV=str(self.f.evidence / 'github-env'))
        namespace = {'__name__': 'actual_workflow_bootstrap'}
        original = Path.read_bytes
        def changed(path):
            return raw + b'# drift\n' if path.name == 's04_windows_verification.executed.py' else original(path)
        with patch.dict(os.environ, env), patch.object(os, 'execv') as execute, patch.object(Path, 'read_bytes', changed):
            exec(compile(ast.Module(body=tree.body[:start], type_ignores=[]), '<workflow bootstrap>', 'exec'), namespace)
            namespace['git'] = self.f.git
            with self.assertRaisesRegex(ValueError, 'materialized W helper changed'):
                exec(compile(ast.Module(body=tree.body[start:], type_ignores=[]), '<workflow bootstrap>', 'exec'), namespace)
            execute.assert_not_called()

    def test_actual_matrix_cli_has_exact_unique_strict_set_and_no_native_effects(self):
        with patch.object(m, 'git', side_effect=AssertionError('matrix must not read or launch Git')), patch.object(m.subprocess, 'Popen', side_effect=AssertionError('matrix must not launch any process')):
            self.assertTrue(all(c['passed'] for c in m.origin_matrix()))
        output = self.f.evidence / 'matrix.json'
        result = subprocess.run([sys.executable, '-I', '-B', str(ROOT / 'tools/s04_windows_verification.py'), 'matrix', '--output', str(output)],
                                capture_output=True, timeout=30)
        self.assertEqual(0, result.returncode, result.stderr.decode())
        data = json.loads(output.read_text())
        self.assertTrue(data['valid'])
        self.assertEqual(28, data['case_count'])
        names = [c['name'] for c in data['cases']]
        self.assertEqual(28, len(set(names)))
        self.assertTrue(all(c['passed'] for c in data['cases']))
        source = (ROOT / '.github/workflows/s03-windows-verification.yml').read_text()
        expected = source.split('$expectedProvenanceCases = @(', 1)[1].split(')', 1)[0]
        self.assertTrue(all("'" + name + "'" in expected for name in names))
        self.assertIn("'reject-candidate_sha_input-mismatch'", expected)
        self.assertIn('$cases.Count -eq 52', source)
        self.assertNotIn('dotnet', result.stdout.decode())

    def test_actual_job_if_preserves_manual_skip_and_rejects_cancelled_routes(self):
        source = (ROOT / '.github/workflows/s03-windows-verification.yml').read_text()
        normal = source.split('  windows-verification:\n', 1)[1].split('    name:', 1)[0]
        expression = next(line.strip()[4:] for line in normal.splitlines() if line.strip().startswith('if: '))
        for event_name, gate, result, admitted, cancelled, expected in (
            ('workflow_dispatch', 'S03.01', 'skipped', '', False, 'normal'),
            ('workflow_dispatch', 'S04.04A-NATIVE-RESOURCE', 'skipped', '', False, 'native'),
            ('push', 'S03.04', 'success', 'true', False, 'normal'),
            ('push', 'S03.04', 'failure', 'true', False, None),
            ('push', 'S03.04', 'success', '', False, None),
            ('workflow_dispatch', 'S03.01', 'skipped', '', True, None),
            ('push', 'S03.04', 'success', 'true', True, None),
            ('pull_request', 'S03.04', 'success', 'true', False, None),
        ):
            converted = expression.replace('always()', 'True').replace('!cancelled()', str(not cancelled)).replace('&&', ' and ').replace('||', ' or ')
            for field, value in [('github.event_name', event_name), ('inputs.gate', gate), ('needs.push-admission.result', result), ('needs.push-admission.outputs.admitted', admitted)]:
                converted = converted.replace(field, repr(value))
            parsed = ast.parse(converted, mode='eval')
            self.assertFalse(any(isinstance(n, (ast.Call, ast.Name, ast.Attribute)) for n in ast.walk(parsed)))
            with self.subTest(event=event_name, cancelled=cancelled, gate=gate):
                self.assertEqual(expected == 'normal', eval(compile(parsed, '<actual GitHub job if>', 'eval'), {'__builtins__': {}}))
                self.assertEqual(expected, m.route(event_name, gate, result, admitted, cancelled))
        native = source.split('  native-resource-verification:\n',1)[1].split('    runs-on:',1)[0]
        self.assertIn("if: github.event_name == 'workflow_dispatch' && inputs.gate == 'S04.04A-NATIVE-RESOURCE'", native)

    def test_actual_raw_workflow_fits_github_500000_byte_limit(self):
        raw = (ROOT / '.github/workflows/s03-windows-verification.yml').read_bytes()
        self.assertLessEqual(len(raw), 500000)

    def _produce_exact_route_mutant(self, name, key, wrong_route):
        """One literal full five-argument fault through the real matrix CLI producer.

        Other keys, including cancellation, delegate to the unpatched production
        route. Expected refusal is asserted independently below, never from route.
        """
        original = m.route
        matched = []
        def altered(event_name, gate, result, admitted, workflow_cancelled=False):
            actual_key = (event_name, gate, result, admitted, workflow_cancelled)
            if actual_key == key:
                matched.append(actual_key)
                return wrong_route
            return original(event_name, gate, result, admitted, workflow_cancelled)
        output = self.f.evidence / ('oracle-' + name + '.json')
        stdout, stderr = io.StringIO(), io.StringIO()
        with patch.object(m, 'route', altered), patch.object(m, 'git', side_effect=AssertionError('matrix must not read Git')), \
                patch.object(m.subprocess, 'Popen', side_effect=AssertionError('matrix must not launch a process')), \
                patch.object(sys, 'argv', ['helper', 'matrix', '--output', str(output)]), redirect_stdout(stdout), redirect_stderr(stderr):
            status = m.main()
        raw = output.read_bytes()
        return status, json.loads(raw), matched, raw, stdout.getvalue(), stderr.getvalue()

    def _assert_exact_route_mutant_refused(self, name, key, wrong_route):
        status, data, matched, _, _, _ = self._produce_exact_route_mutant(name, key, wrong_route)
        self.assertEqual([key], matched)
        self.assertEqual(28, data['case_count'])
        cases = {case['name']: case for case in data['cases']}
        self.assertEqual(28, len(cases))
        self.assertFalse(cases[name]['passed'], 'wrong literal route must fail its production matrix row')
        self.assertFalse(data['valid'], 'wrong literal route must fail the whole actual producer JSON')
        self.assertEqual(2, status, 'wrong literal route must refuse through the actual main() exit contract')
        self.assertEqual([name], [case['name'] for case in data['cases'] if not case['passed']])
        for cancellation in ('reject-route-cancelled-manual', 'reject-route-cancelled-push'):
            self.assertEqual((False, False, True), (cases[cancellation]['expected_pass'], cases[cancellation]['actual_pass'], cases[cancellation]['passed']))

    def test_route_oracle_rejects_skipped_push_wrongly_admitted_as_normal(self):
        self._assert_exact_route_mutant_refused('reject-route-skipped-push', ('push', 'S03.04', 'skipped', 'true', False), 'normal')

    def test_route_oracle_rejects_failed_push_wrongly_admitted_as_normal(self):
        self._assert_exact_route_mutant_refused('reject-route-failed-push', ('push', 'S03.04', 'failure', 'true', False), 'normal')

    def test_route_oracle_rejects_unadmitted_push_wrongly_admitted_as_normal(self):
        self._assert_exact_route_mutant_refused('reject-route-unadmitted-push', ('push', 'S03.04', 'success', '', False), 'normal')

    def test_route_oracle_rejects_unknown_event_wrongly_admitted_as_normal(self):
        self._assert_exact_route_mutant_refused('reject-route-unknown-event', ('pull_request', 'S03.04', 'success', 'true', False), 'normal')

    def test_route_oracle_rejects_manual_normal_changed_to_native(self):
        self._assert_exact_route_mutant_refused('positive-route-manual-normal', ('workflow_dispatch', 'S03.01', 'skipped', '', False), 'native')

    def test_route_oracle_rejects_manual_native_changed_to_normal(self):
        self._assert_exact_route_mutant_refused('positive-route-manual-native', ('workflow_dispatch', 'S04.04A-NATIVE-RESOURCE', 'skipped', '', False), 'normal')

    def test_route_oracle_rejects_lost_admitted_push(self):
        self._assert_exact_route_mutant_refused('positive-route-admitted-push', ('push', 'S03.04', 'success', 'true', False), None)

    def test_route_oracle_correct_baseline_has28_unique_rows_and_cancellation_contract(self):
        output = self.f.evidence / 'oracle-correct-baseline.json'
        with patch.object(m, 'git', side_effect=AssertionError('matrix must not read Git')), \
                patch.object(m.subprocess, 'Popen', side_effect=AssertionError('matrix must not launch a process')), \
                patch.object(sys, 'argv', ['helper', 'matrix', '--output', str(output)]), redirect_stdout(io.StringIO()), redirect_stderr(io.StringIO()):
            status = m.main()
        data = json.loads(output.read_bytes())
        self.assertEqual((0, True, 28), (status, data['valid'], data['case_count']))
        cases = {case['name']: case for case in data['cases']}
        self.assertEqual(28, len(cases))
        self.assertTrue(all(case['passed'] for case in cases.values()))
        self.assertTrue(all(case['expected_pass'] == case['actual_pass'] for case in cases.values()))
        for name in ('reject-route-cancelled-manual', 'reject-route-cancelled-push'):
            self.assertEqual((False, False, True), (cases[name]['expected_pass'], cases[name]['actual_pass'], cases[name]['passed']))


if __name__ == '__main__':
    unittest.main(verbosity=2)
