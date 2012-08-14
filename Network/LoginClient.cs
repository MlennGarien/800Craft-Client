using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Globalization;

namespace ManicDigger
{
    public class LoginData
    {
        public string serveraddress;
        public int port;
        public string mppass;
    }
    public interface ILoginClient
    {
        LoginData Login(string username, string password, string gameurl);
    }
    public class LoginClientDummy : ILoginClient
    {
        #region ILoginClient Members
        public LoginData Login(string username, string password, string gameurl)
        {
            return null;
        }
        #endregion
    }
    public class ServerInfo
    {
        public string Url;
        public string Name;
        public int Players;
        public int PlayersMax;
    }
    public class ProgressEventArgs : EventArgs
    {
        public int ProgressPercent;
    }
    public class LoginClientMinecraft : ILoginClient
    {
        public event EventHandler<ProgressEventArgs> Progress;
        //this section uses a method written by Fragmer from ChargedMinersLauncher
        ﻿// Part of ChargedMinersLaunher | Copyright (c) 2012 Matvei Stefarov <me@matvei.org> | BSD-3 | See LICENSE.txt
        public List<ServerInfo> ServerList(string username, string password)
        {
            ReportProgress(0);
            List<ServerInfo> l = new List<ServerInfo>();
            string url = "http://www.minecraft.net/classic/list";
            string html = LoginAndReadPage(username, password, url);
            Regex ServerListEntry = new Regex(@"<a href=""/classic/play/([0-9a-f]+)"">([^<]+)</a>\s+</td>\s+<td>(\d+)</td>\s+<td>(\d+)</td>\s+<td>(\d+\w)</td>");
            int matchNumber = 0;
            foreach (Match match in ServerListEntry.Matches(html))
            {
                string hash = match.Groups[1].Value;
                // minecraft.net escaping bug workaround
                string name = WebUtility.HtmlDecode( match.Groups[2].Value ).Replace("&hellip;", "…");
                int players;
                if (!Int32.TryParse(match.Groups[3].Value, out players))
                {
                    continue;
                }
                int maxPlayers;
                if (!Int32.TryParse(match.Groups[4].Value, out maxPlayers))
                {
                    continue;
                }
                l.Add(new ServerInfo() { Url = "http://www.minecraft.net/classic/play/" + hash, Name = name, Players = players, PlayersMax = maxPlayers });
                matchNumber++;
            }
            ReportProgress(1);
            return l;
        }
        private void ReportProgress(double progress)
        {
            if (Progress != null) { Progress(this, new ProgressEventArgs() { ProgressPercent = (int)(progress * 100) }); };
        }
        //Three Steps
        public LoginData Login(string username, string password, string gameurl)
        {
            string html = LoginAndReadPage(username, password, gameurl);
            string serveraddress = ReadValue(html.Substring(html.IndexOf("\"server\""), 40));
            string port = ReadValue(html.Substring(html.IndexOf("\"port\""), 40));
            string mppass = ReadValue(html.Substring(html.IndexOf("\"mppass\""), 80)); 
            
            return new LoginData() { serveraddress = serveraddress, port = int.Parse(port), mppass = mppass };
        }
        private string LoginAndReadPage(string username, string password, string gameurl)
        {
            if (this.username != username || this.password != password || loggedincookie.Count == 0)
            {
                this.username = username;
                this.password = password;
                //Step 1 and Step 2.
                LoginCookie();
            }
            //Step 3.
            //---
            //Go to game url and GET using JSESSIONID cookie and _uid cookie.
            //Parse the page to find server, port, mpass strings.
            //---
            WebRequest step3Request = (HttpWebRequest)HttpWebRequest.Create(gameurl);
            foreach (string cookie in loggedincookie)
            {
                step3Request.Headers.Add(cookie);
            }
            using (var s4 = step3Request.GetResponse().GetResponseStream())
            {
                var r = step3Request.GetResponse();
                foreach (string s in r.Headers.AllKeys)
                {
                    //update logged in cookie.
                    bool cleared = false;
                    if (s.Contains("Set-Cookie"))
                    {
                        if (!cleared)
                        {
                            loggedincookie.Clear();
                            cleared = true;
                        }
                        loggedincookie.Add("Cookie: " + r.Headers[s]);
                    }
                }
                string html = new StreamReader(s4).ReadToEnd();
                return html;
            }
        }
        private static string ReadValue(string s)
        {
            string start = "value=\"";
            string end = "\"";
            string ss = s.Substring(s.IndexOf(start) + start.Length);
            ss = ss.Substring(0, ss.IndexOf(end));
            return ss;
        }
        List<string> loggedincookie = new List<string>();
        string username;
        string password;
        
        void LoginCookie()
        {
            //Step 1.
            //---
            //Go to http://www.minecraft.net/login and GET, you will receive JSESSIONID cookie.
            //---
            string loginurl = "http://www.minecraft.net/login";
            string data11 = string.Format("username={0}&password={1}", username, password);
            string sessionidcookie;
            {
                using (WebClient c = new WebClient())
                {
                    try
                    {
                        string html = c.DownloadString(loginurl);
                        sessionidcookie = c.ResponseHeaders[HttpResponseHeader.SetCookie];
                        string sessionid;
                        sessionid = sessionidcookie.Substring(0, sessionidcookie.IndexOf(";"));
                        sessionid = sessionid.Substring(sessionid.IndexOf("=") + 1);
                    }
                    catch
                    {
                        System.Windows.Forms.MessageBox.Show("Unable to log you in, check if minecraft.net is up");
                        return;
                    }
                }
            }
            ReportProgress(1.0 / 3);
            //Step 2.
            //---
            //Go to http://www.minecraft.net/login and POST "username={0}&password={1}" using JSESSIONID cookie.
            //You will receive logged in cookie ("_uid").
            //Because of multipart http page, HttpWebRequest has some trouble receiving cookies in step 2,
            //so it is easier to just use raw TcpClient for this.
            //---
            {
                using (TcpClient step2Client = new TcpClient("www.minecraft.net", 80))
                {
                    var stream = step2Client.GetStream();
                    StreamWriter sw = new StreamWriter(stream);

                    sw.WriteLine("POST /login HTTP/1.0");
                    sw.WriteLine("Host: www.minecraft.net");
                    sw.WriteLine("Content-Type: application/x-www-form-urlencoded");
                    sw.WriteLine("Set-Cookie: " + sessionidcookie);
                    sw.WriteLine("Content-Length: " + data11.Length);
                    sw.WriteLine("");
                    sw.WriteLine(data11);

                    sw.Flush();
                    StreamReader sr = new StreamReader(stream);
                    loggedincookie.Clear();
                    for (; ; )
                    {
                        var s = sr.ReadLine();
                        if (s == null)
                        {
                            break;
                        }
                        //System.Windows.Forms.MessageBox.Show(s);
                        if (s.Contains("Set-Cookie"))
                        {
                            loggedincookie.Add(s);
                        }
                    }
                }
            }
            for (int i = 0; i < loggedincookie.Count; i++)
            {
                loggedincookie[i] = loggedincookie[i].Replace("Set-", "");
            }
            ReportProgress(2.0 / 3);
        }
    }
    /// <summary>
    ///     Taken from System.Net in 4.0, useful until we move to .NET 4.0 - needed for Client Profile
    /// </summary>
    public static class WebUtility
    {
        // Fields
        private static char[] _htmlEntityEndingChars = new char[] { ';', '&' };

        // Methods
        public static string HtmlDecode(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            if (value.IndexOf('&') < 0)
            {
                return value;
            }
            StringWriter output = new StringWriter(CultureInfo.InvariantCulture);
            HtmlDecode(value, output);
            return output.ToString();
        }

        public static void HtmlDecode(string value, TextWriter output)
        {
            if (value != null)
            {
                if (output == null)
                {
                    throw new ArgumentNullException("output");
                }
                if (value.IndexOf('&') < 0)
                {
                    output.Write(value);
                }
                else
                {
                    int length = value.Length;
                    for (int i = 0; i < length; i++)
                    {
                        char ch = value[i];
                        if (ch == '&')
                        {
                            int num3 = value.IndexOfAny(_htmlEntityEndingChars, i + 1);
                            if ((num3 > 0) && (value[num3] == ';'))
                            {
                                string entity = value.Substring(i + 1, (num3 - i) - 1);
                                if ((entity.Length > 1) && (entity[0] == '#'))
                                {
                                    ushort num4;
                                    if ((entity[1] == 'x') || (entity[1] == 'X'))
                                    {
                                        ushort.TryParse(entity.Substring(2), NumberStyles.AllowHexSpecifier, (IFormatProvider)NumberFormatInfo.InvariantInfo, out num4);
                                    }
                                    else
                                    {
                                        ushort.TryParse(entity.Substring(1), NumberStyles.Integer, (IFormatProvider)NumberFormatInfo.InvariantInfo, out num4);
                                    }
                                    if (num4 != 0)
                                    {
                                        ch = (char)num4;
                                        i = num3;
                                    }
                                }
                                else
                                {
                                    i = num3;
                                    char ch2 = HtmlEntities.Lookup(entity);
                                    if (ch2 != '\0')
                                    {
                                        ch = ch2;
                                    }
                                    else
                                    {
                                        output.Write('&');
                                        output.Write(entity);
                                        output.Write(';');
                                        goto Label_0117;
                                    }
                                }
                            }
                        }
                        output.Write(ch);
                    Label_0117: ;
                    }
                }
            }
        }

        public static string HtmlEncode(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            if (IndexOfHtmlEncodingChars(value, 0) == -1)
            {
                return value;
            }
            StringWriter output = new StringWriter(CultureInfo.InvariantCulture);
            HtmlEncode(value, output);
            return output.ToString();
        }

        public static unsafe void HtmlEncode(string value, TextWriter output)
        {
            if (value != null)
            {
                if (output == null)
                {
                    throw new ArgumentNullException("output");
                }
                int num = IndexOfHtmlEncodingChars(value, 0);
                if (num == -1)
                {
                    output.Write(value);
                }
                else
                {
                    int num2 = value.Length - num;
                    fixed (char* str = value)
                    {
                        char* chPtr = str;
                        char* chPtr2 = chPtr;
                        while (num-- > 0)
                        {
                            chPtr2++;
                            output.Write(chPtr2[0]);
                        }
                        while (num2-- > 0)
                        {
                            chPtr2++;
                            char ch = chPtr2[0];
                            if (ch <= '>')
                            {
                                switch (ch)
                                {
                                    case '&':
                                        {
                                            output.Write("&amp;");
                                            continue;
                                        }
                                    case '\'':
                                        {
                                            output.Write("&#39;");
                                            continue;
                                        }
                                    case '"':
                                        {
                                            output.Write("&quot;");
                                            continue;
                                        }
                                    case '<':
                                        {
                                            output.Write("&lt;");
                                            continue;
                                        }
                                    case '>':
                                        {
                                            output.Write("&gt;");
                                            continue;
                                        }
                                }
                                output.Write(ch);
                                continue;
                            }
                            if ((ch >= '\x00a0') && (ch < 'Ā'))
                            {
                                output.Write("&#");
                                output.Write(((int)ch).ToString(NumberFormatInfo.InvariantInfo));
                                output.Write(';');
                            }
                            else
                            {
                                output.Write(ch);
                            }
                        }
                    }
                }
            }
        }

        private static unsafe int IndexOfHtmlEncodingChars(string s, int startPos)
        {
            int num = s.Length - startPos;
            fixed (char* str = s)
            {
                char* chPtr = str;
                char* chPtr2 = chPtr + startPos;
                while (num > 0)
                {
                    char ch = chPtr2[0];
                    if (ch <= '>')
                    {
                        switch (ch)
                        {
                            case '&':
                            case '\'':
                            case '"':
                            case '<':
                            case '>':
                                return (s.Length - num);

                            case '=':
                                goto Label_0086;
                        }
                    }
                    else if ((ch >= '\x00a0') && (ch < 'Ā'))
                    {
                        return (s.Length - num);
                    }
                Label_0086:
                    chPtr2++;
                    num--;
                }
            }
            return -1;
        }

        // Nested Types
        private static class HtmlEntities
        {
            // Fields
            private static string[] _entitiesList = new string[] { 
        "\"-quot", "&-amp", "'-apos", "<-lt", ">-gt", "\x00a0-nbsp", "\x00a1-iexcl", "\x00a2-cent", "\x00a3-pound", "\x00a4-curren", "\x00a5-yen", "\x00a6-brvbar", "\x00a7-sect", "\x00a8-uml", "\x00a9-copy", "\x00aa-ordf", 
        "\x00ab-laquo", "\x00ac-not", "\x00ad-shy", "\x00ae-reg", "\x00af-macr", "\x00b0-deg", "\x00b1-plusmn", "\x00b2-sup2", "\x00b3-sup3", "\x00b4-acute", "\x00b5-micro", "\x00b6-para", "\x00b7-middot", "\x00b8-cedil", "\x00b9-sup1", "\x00ba-ordm", 
        "\x00bb-raquo", "\x00bc-frac14", "\x00bd-frac12", "\x00be-frac34", "\x00bf-iquest", "\x00c0-Agrave", "\x00c1-Aacute", "\x00c2-Acirc", "\x00c3-Atilde", "\x00c4-Auml", "\x00c5-Aring", "\x00c6-AElig", "\x00c7-Ccedil", "\x00c8-Egrave", "\x00c9-Eacute", "\x00ca-Ecirc", 
        "\x00cb-Euml", "\x00cc-Igrave", "\x00cd-Iacute", "\x00ce-Icirc", "\x00cf-Iuml", "\x00d0-ETH", "\x00d1-Ntilde", "\x00d2-Ograve", "\x00d3-Oacute", "\x00d4-Ocirc", "\x00d5-Otilde", "\x00d6-Ouml", "\x00d7-times", "\x00d8-Oslash", "\x00d9-Ugrave", "\x00da-Uacute", 
        "\x00db-Ucirc", "\x00dc-Uuml", "\x00dd-Yacute", "\x00de-THORN", "\x00df-szlig", "\x00e0-agrave", "\x00e1-aacute", "\x00e2-acirc", "\x00e3-atilde", "\x00e4-auml", "\x00e5-aring", "\x00e6-aelig", "\x00e7-ccedil", "\x00e8-egrave", "\x00e9-eacute", "\x00ea-ecirc", 
        "\x00eb-euml", "\x00ec-igrave", "\x00ed-iacute", "\x00ee-icirc", "\x00ef-iuml", "\x00f0-eth", "\x00f1-ntilde", "\x00f2-ograve", "\x00f3-oacute", "\x00f4-ocirc", "\x00f5-otilde", "\x00f6-ouml", "\x00f7-divide", "\x00f8-oslash", "\x00f9-ugrave", "\x00fa-uacute", 
        "\x00fb-ucirc", "\x00fc-uuml", "\x00fd-yacute", "\x00fe-thorn", "\x00ff-yuml", "Œ-OElig", "œ-oelig", "Š-Scaron", "š-scaron", "Ÿ-Yuml", "ƒ-fnof", "ˆ-circ", "˜-tilde", "Α-Alpha", "Β-Beta", "Γ-Gamma", 
        "Δ-Delta", "Ε-Epsilon", "Ζ-Zeta", "Η-Eta", "Θ-Theta", "Ι-Iota", "Κ-Kappa", "Λ-Lambda", "Μ-Mu", "Ν-Nu", "Ξ-Xi", "Ο-Omicron", "Π-Pi", "Ρ-Rho", "Σ-Sigma", "Τ-Tau", 
        "Υ-Upsilon", "Φ-Phi", "Χ-Chi", "Ψ-Psi", "Ω-Omega", "α-alpha", "β-beta", "γ-gamma", "δ-delta", "ε-epsilon", "ζ-zeta", "η-eta", "θ-theta", "ι-iota", "κ-kappa", "λ-lambda", 
        "μ-mu", "ν-nu", "ξ-xi", "ο-omicron", "π-pi", "ρ-rho", "ς-sigmaf", "σ-sigma", "τ-tau", "υ-upsilon", "φ-phi", "χ-chi", "ψ-psi", "ω-omega", "ϑ-thetasym", "ϒ-upsih", 
        "ϖ-piv", " -ensp", " -emsp", " -thinsp", "‌-zwnj", "‍-zwj", "‎-lrm", "‏-rlm", "–-ndash", "—-mdash", "‘-lsquo", "’-rsquo", "‚-sbquo", "“-ldquo", "”-rdquo", "„-bdquo", 
        "†-dagger", "‡-Dagger", "•-bull", "…-hellip", "‰-permil", "′-prime", "″-Prime", "‹-lsaquo", "›-rsaquo", "‾-oline", "⁄-frasl", "€-euro", "ℑ-image", "℘-weierp", "ℜ-real", "™-trade", 
        "ℵ-alefsym", "←-larr", "↑-uarr", "→-rarr", "↓-darr", "↔-harr", "↵-crarr", "⇐-lArr", "⇑-uArr", "⇒-rArr", "⇓-dArr", "⇔-hArr", "∀-forall", "∂-part", "∃-exist", "∅-empty", 
        "∇-nabla", "∈-isin", "∉-notin", "∋-ni", "∏-prod", "∑-sum", "−-minus", "∗-lowast", "√-radic", "∝-prop", "∞-infin", "∠-ang", "∧-and", "∨-or", "∩-cap", "∪-cup", 
        "∫-int", "∴-there4", "∼-sim", "≅-cong", "≈-asymp", "≠-ne", "≡-equiv", "≤-le", "≥-ge", "⊂-sub", "⊃-sup", "⊄-nsub", "⊆-sube", "⊇-supe", "⊕-oplus", "⊗-otimes", 
        "⊥-perp", "⋅-sdot", "⌈-lceil", "⌉-rceil", "⌊-lfloor", "⌋-rfloor", "〈-lang", "〉-rang", "◊-loz", "♠-spades", "♣-clubs", "♥-hearts", "♦-diams"
     };
            private static Dictionary<string, char> _lookupTable = GenerateLookupTable();

            // Methods
            private static Dictionary<string, char> GenerateLookupTable()
            {
                Dictionary<string, char> dictionary = new Dictionary<string, char>(StringComparer.Ordinal);
                foreach (string str in _entitiesList)
                {
                    dictionary.Add(str.Substring(2), str[0]);
                }
                return dictionary;
            }

            public static char Lookup(string entity)
            {
                char ch;
                _lookupTable.TryGetValue(entity, out ch);
                return ch;
            }
        }
    }
}