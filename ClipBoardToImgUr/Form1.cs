using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Xml;

namespace ClipBoardToImgUr
{
    public partial class Form1 : Form
    {
        string imgurApiKey = "GET YOUR OWN API KEY";

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IDataObject data = Clipboard.GetDataObject();
            if (data.GetDataPresent(DataFormats.Bitmap))
            {
                button1.Enabled = false;
                progressBar1.Value = 20; //20%
                progressBar1.Update();
                textBox1.Text = "Uploading...";
                textBox1.Update();
                Image image = (Image)data.GetData(DataFormats.Bitmap, true);
                textBox1.Text = PostToImgur(image, imgurApiKey);
                Clipboard.SetText(textBox1.Text);
                button1.Enabled = true;
            }
            else
            {
                progressBar1.Value = 0; //0%
                textBox1.Text = "No Image In Clipboard";
            }
        }

        public byte[] imageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }

        public string PostToImgur(Image image, string apiKey)
        {
            byte[] imageData = imageToByteArray(image);

            /*FileStream fileStream = File.OpenRead(imagFilePath);
            imageData = new byte[fileStream.Length];
            fileStream.Read(imageData, 0, imageData.Length);
            fileStream.Close();*/

            int progressValue = 20;

            

            const int MAX_URI_LENGTH = 32766;
            string base64img = System.Convert.ToBase64String(imageData);
            StringBuilder sb = new StringBuilder();

            int progressSetp = 60 / base64img.Length / MAX_URI_LENGTH;

            for (int i = 0; i < base64img.Length; i += MAX_URI_LENGTH)
            {
                sb.Append(Uri.EscapeDataString(base64img.Substring(i, Math.Min(MAX_URI_LENGTH, base64img.Length - i))));
                progressValue += progressSetp;
                progressBar1.Value = progressSetp;
                progressBar1.Update();
            }

            string uploadRequestString = "image=" + sb.ToString() + "&key=" + imgurApiKey;

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create("http://api.imgur.com/2/upload");
            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.ServicePoint.Expect100Continue = false;

            StreamWriter streamWriter = new StreamWriter(webRequest.GetRequestStream());
            streamWriter.Write(uploadRequestString);
            streamWriter.Close();

            WebResponse response = webRequest.GetResponse();
            Stream responseStream = response.GetResponseStream();
            StreamReader responseReader = new StreamReader(responseStream);

            string responseString = responseReader.ReadToEnd();
            progressBar1.Value = 100;
            progressBar1.Update();

            
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(responseString);

            string directLink = xmlDoc.SelectSingleNode("//original").InnerText;
            return directLink;
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
        }
    }
}
