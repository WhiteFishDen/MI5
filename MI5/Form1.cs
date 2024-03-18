using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace MI5
{
    public partial class Form1 : Form
    {
        List<string> numb;
        public Form1()
        {
            InitializeComponent();
        }
        IPAddress serverAddress = IPAddress.Loopback;
        const int serverPort = 7777;
        string filename = "logFromServer.txt";


        private async void button1_Click(object sender, EventArgs e)
        {
            using var client = new TcpClient(serverAddress.ToString(), serverPort);
            var stream = client.GetStream();

            byte[] buf = new byte[65536];
            await ReadBytes(sizeof(long));
            long remainingLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt64(buf, 0));

            using var file = File.Create(filename);
            while (remainingLength > 0)
            {
                int lengthToRead = (int)Math.Min(remainingLength, buf.Length);
                await ReadBytes(lengthToRead);
                await file.WriteAsync(buf, 0, lengthToRead);
                remainingLength -= lengthToRead;
            }
            MessageBox.Show("Log created!");

            async Task ReadBytes(int howmuch)
            {
                int readPos = 0;
                while (readPos < howmuch)
                {
                    var actuallyRead = await stream.ReadAsync(buf, readPos, howmuch - readPos);
                    if (actuallyRead == 0)
                        throw new EndOfStreamException();
                    readPos += actuallyRead;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FindTelephoneNumbers(filename);
        }
        public void FindTelephoneNumbers(string path)
        {
            string file = File.ReadAllText(path);
            string regex = @"((\+7|8||\+ )[ ]?)?([(]?\d{3}[)]?[\- ]?)?(\d[ -]?){6,14}";
            Regex rxObject = new Regex(regex);
            MatchCollection matches = rxObject.Matches(file);
            numb = new();
            if(matches.Count>0)
            {
                foreach(Match m in matches)
                {
                    MessageBox.Show(m.Value, "Match Found!");
                    numb.Add(m.Value);
                }
            }
            else
                MessageBox.Show("No Matches Found!");
        }

        public void FindPassword()
        {

        }
    }
}