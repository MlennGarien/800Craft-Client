using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.IO;
using System.Drawing;

namespace ManicDigger.Network
{
    public class PlayerSkinDownloader
    {
        [Inject]
        public IGameExit exit { get; set; }
        [Inject]
        public IThe3d the3d { get; set; }
        public string skinserver;
        Dictionary<string, byte[]> texturestoload = new Dictionary<string, byte[]>();
        Queue<string> texturestodownload = new Queue<string>();
        bool skindownloadthreadstarted = false;
        List<string> texturestodownloadlist = new List<string>();

        /// <summary> Strips all ampersand color codes, and unescapes doubled-up ampersands. </summary>
        public static string StripColors(string input)
        {
            if (input == null) throw new ArgumentNullException("input");
            if (input.IndexOf('&') == -1)
            {
                return input;
            }
            else
            {
                StringBuilder output = new StringBuilder(input.Length);
                for (int i = 0; i < input.Length; i++)
                {
                    if (input[i] == '&')
                    {
                        if (i == input.Length - 1)
                        {
                            break;
                        }
                        else if (input[i + 1] == '&')
                        {
                            output.Append('&');
                        }
                        i++;
                    }
                    else
                    {
                        output.Append(input[i]);
                    }
                }
                return output.ToString();
            }
        }
        public void Update(string[] players, Dictionary<string, int> playertextures, int playertexturedefault)
        {
            foreach (string name in players)
            {
                if (name == null)
                {
                    continue;
                }
                if (playertextures.ContainsKey(name)
                     || texturestodownloadlist.Contains(name))
                {
                    continue;
                }
                lock (texturestodownload)
                {
                    texturestodownload.Enqueue(name);
                    texturestodownloadlist.Add(name);
                }
            }
            lock (texturestoload)
            {
                foreach (var k in new List<KeyValuePair<string, byte[]>>(texturestoload))
                {
                    try
                    {
                        using (Bitmap bmp = new Bitmap(new MemoryStream(k.Value)))
                        {
                            playertextures[k.Key] = the3d.LoadTexture(bmp);
                            Console.WriteLine("Player skin loaded: {0}", k.Key);
                        }
                    }
                    catch (Exception e)
                    {
                        playertextures[k.Key] = playertexturedefault;
                        Console.WriteLine(e);
                    }
                }
                texturestoload.Clear();
            }
            if (!skindownloadthreadstarted)
            {
                new Thread(skindownloadthread).Start();
                skindownloadthreadstarted = true;
            }
        }
        void skindownloadthread()
        {
            WebClient c = new WebClient();
            for (; ; )
            {
                if (exit.exit) { return; }
                for (; ; )
                {
                    string name;
                    lock (texturestodownload)
                    {
                        if (texturestodownload.Count == 0)
                        {
                            break;
                        }
                        name = texturestodownload.Dequeue();
                    }
                    try
                    {
                        byte[] skindata = c.DownloadData("http://www.minecraft.net/skin/"+ StripColors(name)+".png");
                        lock (texturestoload)
                        {
                            texturestoload[name] = skindata;
                        }
                    }
                    catch (Exception e)
                    {
                        //Console.WriteLine(e);
                        continue;
                    }
                }
                Thread.Sleep(100);
            }
        }
    }
}
