using System.Text;
using System.IO.Ports;

namespace PaperangSerial
{
    /// <summary>
    /// Drive PAPERANG P1 via BTSerial
    /// Original Data and Sample Code :
    /// https://github.com/ihciah/miaomiaoji-tool
    /// https://lang-ship.com/blog/work/m5stickc-esp32-bluetooth-paperang/
    /// </summary>
    public class Paperang
    {
        /// <summary>
        /// COM PORT
        /// </summary>
        public string comport ;

        /// <summary>
        /// SERIAL PORT
        /// </summary>
        public SerialPort SerialBT = null;

        /// <summary>
        /// Line Char Length
        /// </summary>
        const uint linelen = 48;

        /// <summary>
        /// Width Height Multiplyer (1 or 2)
        /// </summary>
        int doubleflag = 2;

        /// <summary>
        /// CRC Key const
        /// </summary>
        const uint standardKey = 0x35769521;
        const uint CRCKEY = (0x6968634 ^ 0x2e696d);

        /// <summary>
        /// PRINT Param
        /// </summary>
        const uint padding_line = 300;
        const uint max_send_msg_length = 2016;
        const uint max_recv_msg_length = 1024;
        public int linepadding = 8;

        /// <summary>
        /// PAPERANG COMMANDS
        /// </summary>
        public enum BtCommandByte
        {
            PRT_PRINT_DATA = 0,
            PRT_PRINT_DATA_COMPRESS = 1,
            PRT_FIRMWARE_DATA = 2,
            PRT_USB_UPDATE_FIRMWARE = 3,
            PRT_GET_VERSION = 4,
            PRT_SENT_VERSION = 5,
            PRT_GET_MODEL = 6,
            PRT_SENT_MODEL = 7,
            PRT_GET_BT_MAC = 8,
            PRT_SENT_BT_MAC = 9,
            PRT_GET_SN = 10,
            PRT_SENT_SN = 11,
            PRT_GET_STATUS = 12,
            PRT_SENT_STATUS = 13,
            PRT_GET_VOLTAGE = 14,
            PRT_SENT_VOLTAGE = 15,
            PRT_GET_BAT_STATUS = 16,
            PRT_SENT_BAT_STATUS = 17,
            PRT_GET_TEMP = 18,
            PRT_SENT_TEMP = 19,
            PRT_SET_FACTORY_STATUS = 20,
            PRT_GET_FACTORY_STATUS = 21,
            PRT_SENT_FACTORY_STATUS = 22,
            PRT_SENT_BT_STATUS = 23,
            PRT_SET_CRC_KEY = 24,
            PRT_SET_HEAT_DENSITY = 25,
            PRT_FEED_LINE = 26,
            PRT_PRINT_TEST_PAGE = 27,
            PRT_GET_HEAT_DENSITY = 28,
            PRT_SENT_HEAT_DENSITY = 29,
            PRT_SET_POWER_DOWN_TIME = 30,
            PRT_GET_POWER_DOWN_TIME = 31,
            PRT_SENT_POWER_DOWN_TIME = 32,
            PRT_FEED_TO_HEAD_LINE = 33,
            PRT_PRINT_DEFAULT_PARA = 34,
            PRT_GET_BOARD_VERSION = 35,
            PRT_SENT_BOARD_VERSION = 36,
            PRT_GET_HW_INFO = 37,
            PRT_SENT_HW_INFO = 38,
            PRT_SET_MAX_GAP_LENGTH = 39,
            PRT_GET_MAX_GAP_LENGTH = 40,
            PRT_SENT_MAX_GAP_LENGTH = 41,
            PRT_GET_PAPER_TYPE = 42,
            PRT_SENT_PAPER_TYPE = 43,
            PRT_SET_PAPER_TYPE = 44,
            PRT_GET_COUNTRY_NAME = 45,
            PRT_SENT_COUNTRY_NAME = 46,
            PRT_DISCONNECT_BT_CMD = 47,
            PRT_MAX_CMD = 48,
        };

        /// <summary>
        /// CRC32 TABLE
        /// </summary>
        uint[] crc32tab = {
          0x00000000, 0x77073096, 0xee0e612c, 0x990951ba,
          0x076dc419, 0x706af48f, 0xe963a535, 0x9e6495a3,
          0x0edb8832, 0x79dcb8a4, 0xe0d5e91e, 0x97d2d988,
          0x09b64c2b, 0x7eb17cbd, 0xe7b82d07, 0x90bf1d91,
          0x1db71064, 0x6ab020f2, 0xf3b97148, 0x84be41de,
          0x1adad47d, 0x6ddde4eb, 0xf4d4b551, 0x83d385c7,
          0x136c9856, 0x646ba8c0, 0xfd62f97a, 0x8a65c9ec,
          0x14015c4f, 0x63066cd9, 0xfa0f3d63, 0x8d080df5,
          0x3b6e20c8, 0x4c69105e, 0xd56041e4, 0xa2677172,
          0x3c03e4d1, 0x4b04d447, 0xd20d85fd, 0xa50ab56b,
          0x35b5a8fa, 0x42b2986c, 0xdbbbc9d6, 0xacbcf940,
          0x32d86ce3, 0x45df5c75, 0xdcd60dcf, 0xabd13d59,
          0x26d930ac, 0x51de003a, 0xc8d75180, 0xbfd06116,
          0x21b4f4b5, 0x56b3c423, 0xcfba9599, 0xb8bda50f,
          0x2802b89e, 0x5f058808, 0xc60cd9b2, 0xb10be924,
          0x2f6f7c87, 0x58684c11, 0xc1611dab, 0xb6662d3d,
          0x76dc4190, 0x01db7106, 0x98d220bc, 0xefd5102a,
          0x71b18589, 0x06b6b51f, 0x9fbfe4a5, 0xe8b8d433,
          0x7807c9a2, 0x0f00f934, 0x9609a88e, 0xe10e9818,
          0x7f6a0dbb, 0x086d3d2d, 0x91646c97, 0xe6635c01,
          0x6b6b51f4, 0x1c6c6162, 0x856530d8, 0xf262004e,
          0x6c0695ed, 0x1b01a57b, 0x8208f4c1, 0xf50fc457,
          0x65b0d9c6, 0x12b7e950, 0x8bbeb8ea, 0xfcb9887c,
          0x62dd1ddf, 0x15da2d49, 0x8cd37cf3, 0xfbd44c65,
          0x4db26158, 0x3ab551ce, 0xa3bc0074, 0xd4bb30e2,
          0x4adfa541, 0x3dd895d7, 0xa4d1c46d, 0xd3d6f4fb,
          0x4369e96a, 0x346ed9fc, 0xad678846, 0xda60b8d0,
          0x44042d73, 0x33031de5, 0xaa0a4c5f, 0xdd0d7cc9,
          0x5005713c, 0x270241aa, 0xbe0b1010, 0xc90c2086,
          0x5768b525, 0x206f85b3, 0xb966d409, 0xce61e49f,
          0x5edef90e, 0x29d9c998, 0xb0d09822, 0xc7d7a8b4,
          0x59b33d17, 0x2eb40d81, 0xb7bd5c3b, 0xc0ba6cad,
          0xedb88320, 0x9abfb3b6, 0x03b6e20c, 0x74b1d29a,
          0xead54739, 0x9dd277af, 0x04db2615, 0x73dc1683,
          0xe3630b12, 0x94643b84, 0x0d6d6a3e, 0x7a6a5aa8,
          0xe40ecf0b, 0x9309ff9d, 0x0a00ae27, 0x7d079eb1,
          0xf00f9344, 0x8708a3d2, 0x1e01f268, 0x6906c2fe,
          0xf762575d, 0x806567cb, 0x196c3671, 0x6e6b06e7,
          0xfed41b76, 0x89d32be0, 0x10da7a5a, 0x67dd4acc,
          0xf9b9df6f, 0x8ebeeff9, 0x17b7be43, 0x60b08ed5,
          0xd6d6a3e8, 0xa1d1937e, 0x38d8c2c4, 0x4fdff252,
          0xd1bb67f1, 0xa6bc5767, 0x3fb506dd, 0x48b2364b,
          0xd80d2bda, 0xaf0a1b4c, 0x36034af6, 0x41047a60,
          0xdf60efc3, 0xa867df55, 0x316e8eef, 0x4669be79,
          0xcb61b38c, 0xbc66831a, 0x256fd2a0, 0x5268e236,
          0xcc0c7795, 0xbb0b4703, 0x220216b9, 0x5505262f,
          0xc5ba3bbe, 0xb2bd0b28, 0x2bb45a92, 0x5cb36a04,
          0xc2d7ffa7, 0xb5d0cf31, 0x2cd99e8b, 0x5bdeae1d,
          0x9b64c2b0, 0xec63f226, 0x756aa39c, 0x026d930a,
          0x9c0906a9, 0xeb0e363f, 0x72076785, 0x05005713,
          0x95bf4a82, 0xe2b87a14, 0x7bb12bae, 0x0cb61b38,
          0x92d28e9b, 0xe5d5be0d, 0x7cdcefb7, 0x0bdbdf21,
          0x86d3d2d4, 0xf1d4e242, 0x68ddb3f8, 0x1fda836e,
          0x81be16cd, 0xf6b9265b, 0x6fb077e1, 0x18b74777,
          0x88085ae6, 0xff0f6a70, 0x66063bca, 0x11010b5c,
          0x8f659eff, 0xf862ae69, 0x616bffd3, 0x166ccf45,
          0xa00ae278, 0xd70dd2ee, 0x4e048354, 0x3903b3c2,
          0xa7672661, 0xd06016f7, 0x4969474d, 0x3e6e77db,
          0xaed16a4a, 0xd9d65adc, 0x40df0b66, 0x37d83bf0,
          0xa9bcae53, 0xdebb9ec5, 0x47b2cf7f, 0x30b5ffe9,
          0xbdbdf21c, 0xcabac28a, 0x53b39330, 0x24b4a3a6,
          0xbad03605, 0xcdd70693, 0x54de5729, 0x23d967bf,
          0xb3667a2e, 0xc4614ab8, 0x5d681b02, 0x2a6f2b94,
          0xb40bbe37, 0xc30c8ea1, 0x5a05df1b, 0x2d02ef8d,
        };

        /// <summary>
        /// CRC32 CALC
        /// </summary>
        /// <param name="p"></param>
        /// <param name="len"></param>
        /// <param name="crcinit"></param>
        /// <returns></returns>
        uint crc32(byte[] p, int pctr, int len, uint crcinit)
        {
            uint crc = 0;
            crc = crcinit ^ 0xFFFFFFFF;
            for (; len-- > 0; pctr++)
            {
                crc = ((crc >> 8) & 0x00FFFFFF) ^ crc32tab[(crc ^ (p[pctr])) & 0xFF];
            }
            return crc ^ 0xFFFFFFFF;
        }

        /// <summary>
        /// BT Serial RX Callback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SerialDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int r = SerialBT.BytesToRead;
            if (r > 0) {
                byte[] msg = new byte[r];
                SerialBT.Read(msg, 0, r);
                lcb?.Invoke("RECV:" + bytetoprnstr(msg));
            }
        }

        /// <summary>
        /// BT Serial LOG Callback TYPE
        /// </summary>
        /// <param name="msg">LOG Message</param>
        public delegate void LogCallback(string msg);

        /// <summary>
        /// LOG Callback Hook
        /// </summary>
        event LogCallback lcb = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="adr"></param>
        public Paperang(string com, LogCallback msg)
        {
            comport = com;
            SerialBT = new SerialPort(com, 9600);
            SerialBT.DataReceived += SerialDataReceived;
            lcb = msg;
        }

        /// <summary>
        /// Close Port
        /// </summary>
        public void Close()
        {
            if (SerialBT != null)
            {
                SerialBT.Close();
            }
        }

        /// <summary>
        /// Create PAPERANG Data Packet
        /// </summary>
        /// <param name="bytes">Source Data Array</param>
        /// <param name="len">Source Data Length</param>
        /// <param name="control_command">Command Type</param>
        /// <param name="blockno">Split Block No</param>
        /// <param name="crcinit">CRC Initialize value</param>
        /// <returns>Packet Data</returns>
        byte[] packPerBytes(byte[] bytes, int len, BtCommandByte control_command, byte blockno, uint crcinit = CRCKEY)
        {
            byte[] result = new byte[len + 10];
            result[0] = 0x02; // STX
            result[1] = (byte)control_command; // Command
            result[2] = blockno; // Block No(0)
            result[3] = (byte)(len & 0xFF); // Length(L)
            result[4] = (byte)((len >> 8) & 0xFF); // Length(H)
            for (int i = 0; i < len; i++) {
                result[5 + i] = bytes[i];
            }
            uint c = crc32(result, 5, len, crcinit);
            result[5 + len] = (byte)(c & 0xFF); // CRC32(0)
            result[6 + len] = (byte)((c >> 8) & 0xFF);  // CRC32(1)
            result[7 + len] = (byte)((c >> 16) & 0xFF);  // CRC32(2)
            result[8 + len] = (byte)((c >> 24) & 0xFF);  // CRC32(3)
            result[9 + len] = 0x03; // ETX
            return result;
        }

        /// <summary>
        /// Register New CRC Key
        /// </summary>
        public bool RegisterKey()
        {
            byte[] key = new byte[4];
            uint newkey = CRCKEY ^ standardKey;
            key[0] = (byte)(newkey & 0xFF);
            key[1] = (byte)((newkey >> 8) & 0xFF);
            key[2] = (byte)((newkey >> 16) & 0xFF);
            key[3] = (byte)((newkey >> 24) & 0xFF);
            var msg = packPerBytes(key, 4, BtCommandByte.PRT_SET_CRC_KEY, 0, standardKey);
            return SerialWrite(msg);
        }

        /// <summary>
        /// LineFeed
        /// </summary>
        /// <param name="i">Feed Length</param>
        public bool LineFeed(int i)
        {
            byte[] img = new byte[2];
            img[0] = (byte)(i & 0xFF);
            img[1] = (byte)((i >> 8) & 0xFF);
            var msg = packPerBytes(img, 2, BtCommandByte.PRT_FEED_LINE, 0);
            return SerialWrite(msg);
        }

        /// <summary>
        /// Byte To Hex (for LOG)
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        string bytetoprnstr(byte[] m)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var i in m)
            {
                sb.Append(i.ToString("X2"));
                sb.Append(":");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Send bytes to PAPERANG
        /// </summary>
        /// <param name="msg"></param>
        bool SerialWrite(byte[] msg)
        {
            if (!SerialBT.IsOpen)
            {
                try
                {
                    SerialBT.Open();
                }
                catch (System.IO.IOException)
                {
                    lcb?.Invoke("BT Serial Open Error");
                    return false;
                }
            }
            try
            {
                SerialBT.Write(msg, 0, msg.Length);
                lcb?.Invoke("SEND:" + bytetoprnstr(msg));
            }
            catch (System.IO.IOException)
            {
                lcb?.Invoke("BT Serial Connection Error (Serial Write)");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Print One Line
        /// </summary>
        /// <param name="s">ANK Data</param>
        void printString(byte[] s,int slen)
        {
            /// <summary>
            /// Horizontal Double Size Table
            /// </summary>
            byte[] bitdouble = {
                0b00000000,
                0b00000011,
                0b00001100,
                0b00001111,
                0b00110000,
                0b00110011,
                0b00111100,
                0b00111111,
                0b11000000,
                0b11000011,
                0b11001100,
                0b11001111,
                0b11110000,
                0b11110011,
                0b11111100,
                0b11111111,
            };

            int imglen = 48;
            byte[] img = new byte[imglen];
            for (int y = 0; y < 8; y++)
            {
                for (int i = 0; i < imglen; i += doubleflag)
                {
                    if (i < slen * doubleflag)
                    {
                        if (doubleflag == 1)
                        {
                            img[i] = Font.FONT[s[i] & 0x7F, y];
                        }
                        else
                        {
                            img[i + 0] = bitdouble[(Font.FONT[s[i >> 1] & 0x7F, y] & 0xF0) >> 4];
                            img[i + 1] = bitdouble[Font.FONT[s[i >> 1] & 0x7F, y] & 0x0F];
                        }
                    }
                    else
                    {
                        if (doubleflag == 1)
                        {
                            img[i] = 0x00;
                        }
                        else
                        {
                            img[i + 0] = 0x00;
                            img[i + 1] = 0x00;
                        }
                    }
                }
                var msg = packPerBytes(img, imglen, BtCommandByte.PRT_PRINT_DATA, 0);
                SerialWrite(msg);
                SerialWrite(msg); // so Double Size (Vertical)
            }
        }


        /// <summary>
        /// Line Buffer and Counter
        /// </summary>
        byte[] line = new byte[linelen + 1];
        byte linectr = 0;

        /// <summary>
        /// Print One Byte
        /// </summary>
        /// <param name="d8">Data Byte</param>
        public void Print(byte d8)
        {
            if (d8 >= 0x20)
            {
                line[linectr++] = d8;
            }
            if ((linectr >= linelen / doubleflag) || (d8 == 0x0A))
            {
                line[linectr] = 0x00;
                printString(line,linectr);
                linectr = 0;
                LineFeed(linepadding);
            }
        }

        /// <summary>
        /// Print each bytes of string
        /// </summary>
        /// <param name="s">string</param>
        public void Print(string s)
        {
            byte[] b = Encoding.ASCII.GetBytes(s);
            foreach (var i in b) Print(i);
            Print(0x0A);
        }


    }
}
