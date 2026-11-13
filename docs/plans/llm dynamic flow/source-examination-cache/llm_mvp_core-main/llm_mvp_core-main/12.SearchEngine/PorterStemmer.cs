namespace SearchEngine;

// Porter Stemmer — pure-C# implementation of the Porter (1980) algorithm.
// Reference: https://tartarus.org/martin/PorterStemmer/  (public domain algorithm)
public sealed class PorterStemmer
{
    private char[] _b = [];
    private int    _k;   // index of last char
    private int    _j;   // general offset

    public string Stem(string word)
    {
        if (word.Length <= 2) return word;
        _b = word.ToCharArray();
        _k = word.Length - 1;
        Step1ab();
        Step1c();
        Step2();
        Step3();
        Step4();
        Step5();
        return new string(_b, 0, _k + 1);
    }

    // ─── helpers ────────────────────────────────────────────────────────────

    private bool IsConsonant(int i)
    {
        switch (_b[i])
        {
            case 'a': case 'e': case 'i': case 'o': case 'u': return false;
            case 'y': return i == 0 || !IsConsonant(i - 1);
            default:  return true;
        }
    }

    // Count consonant sequences (m) in b[0..j]
    private int M()
    {
        int n = 0, i = 0;
        while (true)
        {
            if (i > _j) return n;
            if (!IsConsonant(i)) break;
            i++;
        }
        i++;
        while (true)
        {
            while (true)
            {
                if (i > _j) return n;
                if (IsConsonant(i)) break;
                i++;
            }
            i++; n++;
            while (true)
            {
                if (i > _j) return n;
                if (!IsConsonant(i)) break;
                i++;
            }
            i++;
        }
    }

    private bool VowelInStem()
    {
        for (int i = 0; i <= _j; i++)
            if (!IsConsonant(i)) return true;
        return false;
    }

    private bool DoubleConsonant(int j)
        => j >= 1 && _b[j] == _b[j - 1] && IsConsonant(j);

    private bool CvcEnding(int i)
    {
        if (i < 2 || !IsConsonant(i) || IsConsonant(i - 1) || !IsConsonant(i - 2))
            return false;
        char c = _b[i];
        return c != 'w' && c != 'x' && c != 'y';
    }

    private bool EndsWith(string s)
    {
        int len = s.Length;
        if (len > _k + 1) return false;
        for (int i = 0; i < len; i++)
            if (_b[_k - len + 1 + i] != s[i]) return false;
        _j = _k - len;
        return true;
    }

    private void SetTo(string s)
    {
        int len = s.Length;
        for (int i = 0; i < len; i++) _b[_j + 1 + i] = s[i];
        _k = _j + len;
    }

    private void Replace(string s) { if (M() > 0) SetTo(s); }

    // ─── steps ──────────────────────────────────────────────────────────────

    private void Step1ab()
    {
        if (_b[_k] == 's')
        {
            if      (EndsWith("sses")) _k -= 2;
            else if (EndsWith("ies"))  SetTo("i");
            else if (_b[_k - 1] != 's') _k--;
        }
        if (EndsWith("eed")) { if (M() > 0) _k--; }
        else if ((EndsWith("ed") || EndsWith("ing")) && VowelInStem())
        {
            _k = _j;
            if      (EndsWith("at")) SetTo("ate");
            else if (EndsWith("bl")) SetTo("ble");
            else if (EndsWith("iz")) SetTo("ize");
            else if (DoubleConsonant(_k))
            {
                char c = _b[_k--];
                if (c == 'l' || c == 's' || c == 'z') _k++;
            }
            else if (M() == 1 && CvcEnding(_k)) SetTo("e");
        }
    }

    private void Step1c()
    {
        if (EndsWith("y") && VowelInStem()) _b[_k] = 'i';
    }

    private void Step2()
    {
        switch (_b[_k - 1])
        {
            case 'a':
                if      (EndsWith("ational")) Replace("ate");
                else if (EndsWith("tional"))  Replace("tion");
                break;
            case 'c':
                if      (EndsWith("enci")) Replace("ence");
                else if (EndsWith("anci")) Replace("ance");
                break;
            case 'e':
                if (EndsWith("izer")) Replace("ize");
                break;
            case 'l':
                if      (EndsWith("bli"))   Replace("ble");
                else if (EndsWith("alli"))  Replace("al");
                else if (EndsWith("entli")) Replace("ent");
                else if (EndsWith("eli"))   Replace("e");
                else if (EndsWith("ousli")) Replace("ous");
                break;
            case 'o':
                if      (EndsWith("ization")) Replace("ize");
                else if (EndsWith("ation"))   Replace("ate");
                else if (EndsWith("ator"))    Replace("ate");
                break;
            case 's':
                if      (EndsWith("alism"))   Replace("al");
                else if (EndsWith("iveness")) Replace("ive");
                else if (EndsWith("fulness")) Replace("ful");
                else if (EndsWith("ousness")) Replace("ous");
                break;
            case 't':
                if      (EndsWith("aliti")) Replace("al");
                else if (EndsWith("iviti")) Replace("ive");
                else if (EndsWith("biliti")) Replace("ble");
                break;
            case 'g':
                if (EndsWith("logi")) Replace("log");
                break;
        }
    }

    private void Step3()
    {
        switch (_b[_k])
        {
            case 'e':
                if      (EndsWith("icate")) Replace("ic");
                else if (EndsWith("ative")) Replace("");
                else if (EndsWith("alize")) Replace("al");
                break;
            case 'i':
                if (EndsWith("iciti")) Replace("ic");
                break;
            case 'l':
                if      (EndsWith("ical")) Replace("ic");
                else if (EndsWith("ful"))  Replace("");
                break;
            case 's':
                if (EndsWith("ness")) Replace("");
                break;
        }
    }

    private void Step4()
    {
        switch (_b[_k - 1])
        {
            case 'a':
                if (EndsWith("al")) break; else return;
            case 'c':
                if (EndsWith("ance") || EndsWith("ence")) break; else return;
            case 'e':
                if (EndsWith("er")) break; else return;
            case 'i':
                if (EndsWith("ic")) break; else return;
            case 'l':
                if (EndsWith("able") || EndsWith("ible")) break; else return;
            case 'n':
                if (EndsWith("ant") || EndsWith("ement") || EndsWith("ment") || EndsWith("ent")) break; else return;
            case 'o':
                if ((EndsWith("ion") && _j >= 0 && (_b[_j] == 's' || _b[_j] == 't'))
                    || EndsWith("ou")) break; else return;
            case 's':
                if (EndsWith("ism")) break; else return;
            case 't':
                if (EndsWith("ate") || EndsWith("iti")) break; else return;
            case 'u':
                if (EndsWith("ous")) break; else return;
            case 'v':
                if (EndsWith("ive")) break; else return;
            case 'z':
                if (EndsWith("ize")) break; else return;
            default: return;
        }
        if (M() > 1) _k = _j;
    }

    private void Step5()
    {
        _j = _k;
        if (_b[_k] == 'e')
        {
            int m = M();
            if (m > 1 || (m == 1 && !CvcEnding(_k - 1))) _k--;
        }
        if (_b[_k] == 'l' && DoubleConsonant(_k) && M() > 1) _k--;
    }
}
