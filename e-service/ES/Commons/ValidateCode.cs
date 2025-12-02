using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace ES.Commons
{
    /// <summary>
    /// 圖型驗證碼工具
    /// </summary>
    public class ValidateCode
    {
        private int imageHeight = 40;
        private int imageWidth = -1;

        /// <summary>
        /// 驗證碼最大長度
        /// </summary>
        public int MaxLength
        {
            get { return 10; }
        }
        /// <summary>
        /// 驗證碼最小長度
        /// </summary>
        public int MinLength
        {
            get { return 1; }
        }
        /// <summary>
        /// 建立驗證碼
        /// </summary>
        /// <param name="length">指定驗證碼的長度</param>
        /// <param name="includeChar">驗證碼是否包含英文字元a-z</param>
        /// <returns></returns>
        public string CreateValidateCode(int length, bool includeChar = false)
        {
            string[] randMembersA = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            string[] randMembersC = { "a", "b", "c", "d", "e", "f", "g", "h", "j", "k", "m", "n", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z"};
            string[] randMembers;
            string[] validateNums = new string[length];
            string validateNumberStr = "";

            int seekSeek = unchecked((int)DateTime.Now.Ticks);
            Random seekRand = new Random(seekSeek);
            int beginSeek = (int)seekRand.Next(0, Int32.MaxValue - length * 10000);
            int[] seeks = new int[length];
            for (int i = 0; i < length; i++)
            {
                beginSeek += 10000;
                seeks[i] = beginSeek;
            }

            //產生隨機數字來源
            if(includeChar)
            {
                randMembers = new string[randMembersA.Length + randMembersC.Length];
                Array.Copy(randMembersA, randMembers, randMembersA.Length);
                Array.Copy(randMembersC, 0, randMembers, randMembersA.Length, randMembersC.Length);
            }
            else
            {
                randMembers = randMembersA;
            }

            //抽取隨機數字
            Random rand = new Random(DateTime.Now.Millisecond);
            for (int i = 0; i < length; i++)
            {
                int numPosition = rand.Next(0, randMembers.Length - 1);
                validateNums[i] = randMembers[numPosition];
            }

            //產生驗證碼
            for (int i = 0; i < length; i++)
            {
                validateNumberStr += validateNums[i];
            }
            return validateNumberStr;
        }

        /// <summary>
        /// 建立驗證碼圖片
        /// </summary>
        /// <param name="validateCode">驗證碼</param>
        public MemoryStream CreateValidateGraphic(string validateCode)
        {
            // 增加文字間距變化
            Random r = new Random();
            validateCode = validateCode.Insert(r.Next(100) % validateCode.Length, " ");
            validateCode = validateCode.Insert(r.Next(100) % validateCode.Length, " ");
            validateCode = validateCode.Insert(r.Next(100) % validateCode.Length, " ");

            if(this.imageWidth < 0)
            {
                this.imageWidth = validateCode.Length * 15;
            }
            Bitmap image = new Bitmap(this.imageWidth, this.imageHeight);
            Graphics g = Graphics.FromImage(image);
            try
            {
                Random random = new Random();

                //清空圖片背景色
                g.Clear(Color.White);

                //建立圖片干擾線
                for (int i = 0; i < 25; i++)
                {
                    int x1 = random.Next(image.Width);
                    int x2 = random.Next(image.Width);
                    int y1 = random.Next(image.Height);
                    int y2 = random.Next(image.Height);
                    if (i % 5 == 0)
                    {
                        g.DrawLine(new Pen(Color.BlueViolet, 1f), x1, y1, x2, y2);
                    }
                    else
                    {
                        g.DrawLine(new Pen(Color.Silver, 2f), x1, y1, x2, y2);
                    }
                }
                Font font = new Font("Arial", 18, (FontStyle.Bold | FontStyle.Italic));
                LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, image.Width, image.Height),
                 Color.Blue, Color.DarkRed, 1.2f, true);

                int randHieght = r.Next(2, 12); // 文字y軸位置變化
                g.DrawString(validateCode, font, brush, 3, randHieght);

                //建立圖片雜點
                for (int i = 0; i < 100; i++)
                {
                    int x = random.Next(image.Width);
                    int y = random.Next(image.Height);
                    image.SetPixel(x, y, Color.FromArgb(random.Next()));
                }

                //畫圖片的邊框
                g.DrawRectangle(new Pen(Color.Silver), 0, 0, image.Width - 1, image.Height - 1);

                //儲存圖片
                MemoryStream stream = new MemoryStream();
                image.Save(stream, ImageFormat.Jpeg);

                return stream;
            }
            finally
            {
                g.Dispose();
                image.Dispose();
            }
        }

        /// <summary>
        /// 取得或設定驗證碼圖片的長度(Pixel)
        /// </summary>
        /// <returns></returns>
        public int ImageWidth
        {
            get
            {
                return imageWidth;
            }
            set
            {
                imageWidth = value;
            }
        }

        /// <summary>
        /// 取得或設定驗證碼的高度(Pixel)
        /// </summary>
        /// <returns></returns>
        public int ImageHeight
        {
            get { return imageHeight; }
            set { imageHeight = value; }
        }

        /// <summary>
        /// 建立驗證碼 Wav Audio
        /// </summary>
        /// <param name="validateCode"></param>
        /// <param name="audioFilePath"></param>
        /// <returns></returns>
        public MemoryStream CreateValidateAudio(string validateCode, string audioFilePath)
        {
            if(string.IsNullOrWhiteSpace(validateCode))
            {
                throw new ArgumentNullException("validateCode is null");
            }
            List<string> audioFiles = new List<string>();
            for(int i=0; i<validateCode.Length; i++)
            {
                audioFiles.Add(audioFilePath + validateCode[i] + ".wav");
            }

            WaveTk wave = new WaveTk();
            return wave.Merge(audioFiles.ToArray());
        }

    }
}