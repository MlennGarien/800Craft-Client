using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ManicDigger
{
    public class TextPart
    {
        public FastColor color;
        public string text;
    }
    public struct Text
    {
        public string text;
        public float fontsize;
        public FastColor color;
        public override int GetHashCode()
        {
            return text.GetHashCode() % fontsize.GetHashCode() % color.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj is Text)
            {
                Text other = (Text)obj;
                return other.fontsize.Equals(this.fontsize)
                    && other.color.Equals(this.color)
                    && other.text.Equals(this.text);
            }
            return base.Equals(obj);
        }
    }
    public class TextRenderer
    {
        public Bitmap MakeTextTexture(Text t)
        {
            Font font = ManicDiggerGameWindow.fn;
                t.fontsize = font.Size;
            var parts = DecodeColors(t.text, t.color);
            float totalwidth = 0;
            float totalheight = 0;
            List<SizeF> sizes = new List<SizeF>();
            using (Bitmap bmp = new Bitmap(1, 1))
            {
                for (int i = 0; i < parts.Count; i++)
                {
                    SizeF size = MeasureTextSize(parts[i].text, font);
                    if (size.Width == 0 || size.Height == 0)
                    {
                        continue;
                    }
                    totalwidth += size.Width;
                    totalheight = Math.Max(totalheight, size.Height);
                    sizes.Add(size);
                }
            }
            SizeF size2 = new SizeF(NextPowerOfTwo((uint)totalwidth), NextPowerOfTwo((uint)totalheight));
            Bitmap bmp2 = new Bitmap((int)size2.Width, (int)size2.Height);
            using (Graphics g2 = Graphics.FromImage(bmp2))
            {
                float currentwidth = 0;
                for (int i = 0; i < parts.Count; i++)
                {
                    parts[i].text = parts[i].text;
                    SizeF sizei = MeasureTextSize(parts[i].text, font);
                    if (sizei.Width == 0 || sizei.Height == 0)
                    {
                        continue;
                    }
                    g2.SmoothingMode = SmoothingMode.HighQuality;
                    g2.DrawString(parts[i].text, font, new SolidBrush(Color.Black), currentwidth + 1.3f, 1.3f);
                    g2.DrawString(parts[i].text, font, new SolidBrush(parts[i].color.ToColor()), currentwidth, 0);

                    currentwidth += sizei.Width;
                }
            }
            return bmp2;
        }
        GraphicsPath GetStringPath(string s, float emSize, RectangleF rect, Font font, StringFormat format)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddString(s, font.FontFamily, (int)font.Style, emSize, rect, format);
            return path;
        }
        private uint NextPowerOfTwo(uint x)
        {
            x--;
            x |= x >> 1;  // handle  2 bit numbers
            x |= x >> 2;  // handle  4 bit numbers
            x |= x >> 4;  // handle  8 bit numbers
            x |= x >> 8;  // handle 16 bit numbers
            x |= x >> 16; // handle 32 bit numbers
            x++;
            return x;
        }
        public List<TextPart> DecodeColors(String s, FastColor defaultcolor)
        {
            List<TextPart> parts = new List<TextPart>();
            int i = 0;
            FastColor currentcolor = defaultcolor;
            String currenttext = null;
            for (; ; )
            {
                if (i >= s.Length)
                {
                    if (currenttext != null)
                    {
                        parts.Add(new TextPart() { text = currenttext, color = currentcolor });
                    }
                    currenttext = "";
                    break;
                }
                if (s[i] == '&')
                {
                    if (i + 1 < s.Length)
                    {
                        int? color = HexToInt(s[i + 1]);
                        if (color != null)
                        {
                            if (currenttext != null)
                            {
                                parts.Add(new TextPart() { text = currenttext, color = currentcolor });
                            }
                            currenttext = "";
                            currentcolor = GetColor(color.Value);
                            i++;
                            goto next;
                        }
                    }
                    else
                    {
                    }
                }
                    currenttext += s[i];
            next:
                i++;
            }
            return parts;
        }
        private FastColor GetColor(int currentcolor)
        {
            switch (currentcolor)
            {
                case 0: { return new FastColor(Color.FromArgb(0, 0, 0)); }
                case 1: { return  new FastColor(Color.FromArgb(0, 0, 191)); }
                case 2: { return  new FastColor(Color.FromArgb(0, 191, 0)); }
                case 3: { return  new FastColor(Color.FromArgb(0, 191, 191)); }
                case 4: { return  new FastColor(Color.FromArgb(191, 0, 0)); }
                case 5: { return  new FastColor(Color.FromArgb(191, 0, 191)); }
                case 6: { return  new FastColor(Color.FromArgb(191, 191, 0)); }
                case 7: { return  new FastColor(Color.FromArgb(191, 191, 191)); }
                case 8: { return  new FastColor(Color.FromArgb(40, 40, 40)); }
                case 9: { return  new FastColor(Color.FromArgb(64, 64, 255)); }
                case 10: { return  new FastColor(Color.FromArgb(64, 255, 64)); }
                case 11: { return  new FastColor(Color.FromArgb(64, 255, 255)); }
                case 12: { return  new FastColor(Color.FromArgb(255, 64, 64)); }
                case 13: { return  new FastColor(Color.FromArgb(255, 64, 255)); }
                case 14: { return  new FastColor(Color.FromArgb(255, 255, 64)); }
                case 15: { return  new FastColor(Color.FromArgb(255, 255, 255)); }
                default: throw new Exception();
            }
        }

        public SizeF MeasureTextSize(string text, Font font)
        {
            using (Bitmap bmp = new Bitmap(1, 1))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    return g.MeasureString(text, font);
                }
            }
        }
        int? HexToInt(char c)
        {
            if (c == '0') { return 0; }
            if (c == '1') { return 1; }
            if (c == '2') { return 2; }
            if (c == '3') { return 3; }
            if (c == '4') { return 4; }
            if (c == '5') { return 5; }
            if (c == '6') { return 6; }
            if (c == '7') { return 7; }
            if (c == '8') { return 8; }
            if (c == '9') { return 9; }
            if (c == 'a') { return 10; }
            if (c == 'b') { return 11; }
            if (c == 'c') { return 12; }
            if (c == 'd') { return 13; }
            if (c == 'e') { return 14; }
            if (c == 'f') { return 15; }
            return null;
        }
        public bool NewFont = true;
    }
}