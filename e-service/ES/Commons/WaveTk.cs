using System.IO;

namespace ES.Commons
{
    /// <summary>
    /// Wav 音檔合併工具類
    /// </summary>
    public class WaveTk
    {
        /// <summary>
        /// Wav音檔總長度
        /// </summary>
        public int length;
        /// <summary>
        /// Wav音檔聲道數
        /// </summary>
        public short channels;
        /// <summary>
        /// Wav音檔取樣頻率
        /// </summary>
        public int samplerate;
        /// <summary>
        /// Wav音檔資料長度(不含Header區)
        /// </summary>
        public int DataLength;
        /// <summary>
        /// Wav音檔取樣bit數
        /// </summary>
        public short BitsPerSample;

        /// <summary>
        /// 讀取來源 Wav 檔的 Header 資訊, 並累加 DataLength
        /// </summary>
        /// <param name="spath"></param>
        private void WaveHeaderIN(string spath)
        {
            FileStream fs = new FileStream(spath, FileMode.Open, FileAccess.Read);

            BinaryReader br = new BinaryReader(fs);
            length = (int)fs.Length - 8;
            fs.Position = 22;
            channels = br.ReadInt16();
            fs.Position = 24;
            samplerate = br.ReadInt32();
            fs.Position = 34;

            BitsPerSample = br.ReadInt16();
            DataLength = (int)fs.Length - 44;
            br.Close();
            fs.Close();
        }

        /// <summary>
        /// 產生合併後 Wav 檔的 Header 資訊
        /// </summary>
        /// <returns></returns>
        private byte[] WaveHeaderOUT()
        {
            MemoryStream ms = new MemoryStream(44);
            ms.Position = 0;

            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write(new char[4] { 'R', 'I', 'F', 'F' });

            bw.Write(length);

            bw.Write(new char[8] { 'W', 'A', 'V', 'E', 'f', 'm', 't', ' ' });

            bw.Write((int)16);

            bw.Write((short)1);
            bw.Write(channels);

            bw.Write(samplerate);

            bw.Write((int)(samplerate * ((BitsPerSample * channels) / 8)));

            bw.Write((short)((BitsPerSample * channels) / 8));

            bw.Write(BitsPerSample);

            bw.Write(new char[4] { 'd', 'a', 't', 'a' });
            bw.Write(DataLength);
            bw.Close();

            return ms.ToArray();
        }

        /// <summary>
        /// 將指定的多個 Wav 檔案, 依序合併為單一Wav檔, 
        /// 並以 MemoryStream 方式回傳合併後的完整Wav檔內容
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public MemoryStream Merge(string[] files)
        {
            WaveTk wa_IN = new WaveTk();
            WaveTk wa_out = new WaveTk();

            wa_out.DataLength = 0;
            wa_out.length = 0;


            //Gather header data
            foreach (string path in files)
            {
                wa_IN.WaveHeaderIN(@path);
                wa_out.DataLength += wa_IN.DataLength;
                wa_out.length += wa_IN.length;
            }

            //Recontruct new header for output
            wa_out.BitsPerSample = wa_IN.BitsPerSample;
            wa_out.channels = wa_IN.channels;
            wa_out.samplerate = wa_IN.samplerate;
            byte[] wa_out_header = wa_out.WaveHeaderOUT();

            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write(wa_out_header);

            //Read all wav data (skip wav header) and append to MemoryStream 
            foreach (string path in files)
            {
                FileStream fs = new FileStream(@path, FileMode.Open, FileAccess.Read);
                byte[] arrfile = new byte[fs.Length - 44];
                fs.Position = 44;
                fs.Read(arrfile, 0, arrfile.Length);
                fs.Close();

                // append to MemoryStream
                bw.Write(arrfile);
            }

            return ms;
        }

    }
}