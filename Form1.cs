using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SteamTokenGenerator
{
    public partial class Form1 : Form
    {
        private static byte[] steamGuardCodeTranslations = new byte[] { 50, 51, 52, 53, 54, 55, 56, 57, 66, 67, 68, 70, 71, 72, 74, 75, 77, 78, 80, 81, 82, 84, 86, 87, 88, 89 };

        string SharedSecret;
        private static long _timeDifference = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        public static long GetSystemUnixTime()
        {
            return (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(_timeDifference))).TotalSeconds;
        }

        public long GetSteamTime()
        {
            _timeDifference = Convert.ToInt64(textBox2.Text);
            return GetSystemUnixTime() + _timeDifference + Convert.ToInt64(textBox3.Text);
        }

        public string GenerateSteamGuardCodeForTime(long time)
        {
            SharedSecret = textBox1.Text;

            if (this.SharedSecret == null || this.SharedSecret.Length == 0)
            {
                return "";
            }

            string sharedSecretUnescaped = Regex.Unescape(this.SharedSecret);
            byte[] sharedSecretArray = Convert.FromBase64String(sharedSecretUnescaped);
            byte[] timeArray = new byte[8];

            time /= 30L;

            for (int i = 8; i > 0; i--)
            {
                timeArray[i - 1] = (byte)time;
                time >>= 8;
            }

            HMACSHA1 hmacGenerator = new HMACSHA1();
            hmacGenerator.Key = sharedSecretArray;
            byte[] hashedData = hmacGenerator.ComputeHash(timeArray);
            byte[] codeArray = new byte[5];
            try
            {
                byte b = (byte)(hashedData[19] & 0xF);
                int codePoint = (hashedData[b] & 0x7F) << 24 | (hashedData[b + 1] & 0xFF) << 16 | (hashedData[b + 2] & 0xFF) << 8 | (hashedData[b + 3] & 0xFF);

                for (int i = 0; i < 5; ++i)
                {
                    codeArray[i] = steamGuardCodeTranslations[codePoint % steamGuardCodeTranslations.Length];
                    codePoint /= steamGuardCodeTranslations.Length;
                }
            }
            catch (Exception)
            {
                return null; //Change later, catch-alls are bad!
            }
            return Encoding.UTF8.GetString(codeArray);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox4.Text = GenerateSteamGuardCodeForTime(GetSteamTime());
        }
    }
}
