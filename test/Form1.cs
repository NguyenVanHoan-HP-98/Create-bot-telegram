using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace Auto_Query_Version1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        #region Khai báo
        string chuoi = "";
        MessageEventArgs e_input = null;
        static TelegramBotClient bl = new TelegramBotClient("1895210001:AAEv_Nb6cI1t7PpOlasKptyWGMxkIHxxdeM");
        string[] result_input;
        string[] data_query = new string[10000];
        #endregion

        // Hàm có chứa phương thức gửi tin nhắn
        public static async void SendTelegram(string noidung, MessageEventArgs e, int trangthai)
        {
            try
            {
                var id = e.Message.Chat;
                var ttnd = e.Message.From.Username;
                if (trangthai == 1)
                {
                    var msg = noidung;
                    var msg2 = noidung;
                    msg2 = msg2.Replace("'|| ", "");
                    msg2 = msg2.Replace(" ||'", "");
                   var msg3 = msg2.Replace("p_phanvung_id", "vphanvung_id");
                   var msg4 = msg.Replace("p_phanvung_id", "vphanvung_id");

                    // var id = Uri.EscapeDataString(group_id);
                    // await bl.SendTextMessageAsync(id, "Thưa đại ca " + e.Message.Chat.Username + " chuỗi có được" + Environment.NewLine + msg);


                    //    /*

                    if (noidung == "Bảng của anh không có trường phanvung_id")
                    {
                        await bl.SendTextMessageAsync(chatId: id, "Thật tiếc anh " + ttnd + " à " + Environment.NewLine + msg);
                    }
                    else
                    {                
                        await bl.SendTextMessageAsync(chatId: id, Environment.NewLine + msg);
                        await bl.SendTextMessageAsync(chatId: id,   Environment.NewLine + msg2);
                        await bl.SendTextMessageAsync(chatId: id, Environment.NewLine + msg3);
                        await bl.SendTextMessageAsync(chatId: id, Environment.NewLine + msg4);
                    }
                   //      */
                }   
                if(trangthai == 2)
                {
                    // await bl.SendTextMessageAsync(id, "Thưa đại ca " + e.Message.Chat.Username + " chuỗi có được" + Environment.NewLine + msg);
                    await bl.SendTextMessageAsync(chatId: id, Environment.NewLine + noidung);
                }
                if(trangthai == 3)
                {
                    if (int.Parse(noidung)==1)
                    {
                            await bl.SendTextMessageAsync(chatId: id, "Ngay lúc này anh "+ ttnd + " không nên uống cafe");
                    } 
                    else
                    {
                        await bl.SendTextMessageAsync(chatId: id, "Ngay lúc này anh " + ttnd + " nên uống cafe");
                    }    
                    
                }
                if(trangthai == 4)
                {
                    await bl.SendTextMessageAsync(chatId: id, "Thưa anh " + ttnd + " bot đã Refresh lại Database");
                }

            }
            catch (Exception ex)
            {
                return;
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            ConnectionDatabase conn = new ConnectionDatabase();
            var table = conn.Get_Table();
            int i = 0;
            foreach (DataRow dr in table.Rows)
            {
                data_query[i] = dr[0].ToString().Trim();
                i++;
            }
            bl.StartReceiving();
            bl.OnMessage += Bl_OnMessage;
            if (chuoi != "")
            {
                Call_SendTelegram(chuoi, e_input);
                chuoi = "";
            }

        }

        // Xử lý các chuỗi phanvung_id
        private string XuLy(string input)
        {
            string kq = "";
            input = input.Replace("\n", "");
            result_input = input.ToString().ToUpper().Split(",");
            for (int i = 0; i < result_input.Length; i++)
            {
                if (result_input[i].ToString().Trim().IndexOf('.') != -1)
                {
                    string[] save = result_input[i].ToString().Split('.');
                    if (save[1].ToString().Trim().Split(' ').Length > 1)
                    {
                        string[] save1 = save[1].ToString().Split(' ');
                        int idex_save1 = Array.IndexOf(data_query, save1[0].ToString().Trim());
                        if (idex_save1 != -1)
                        {
                            kq = kq + "AND " + save1[1].ToString().ToLower().Trim() + ".phanvung_id" + " = '|| p_phanvung_id ||'" + Environment.NewLine;

                        }
                    }
                    else
                    {
                        int idex_save = Array.IndexOf(data_query, save[1].ToString().Trim());
                        if (idex_save != -1)
                        {
                            kq = kq + "AND " + save[1].ToString().ToLower().Trim() + ".phanvung_id" + " = '|| p_phanvung_id ||'" + Environment.NewLine;

                        }
                    }
                }
                else
                {
                    if (result_input[i].ToString().Trim().Split(' ').Length > 1)
                    {
                        string[] save2 = result_input[i].ToString().Trim().Split(' ');
                        int idex_save2 = Array.IndexOf(data_query, save2[0].ToString());
                        if (idex_save2 != -1)
                        {
                            kq = kq + "AND " + save2[1].ToString().ToLower().Trim() + ".phanvung_id" + " = '|| p_phanvung_id ||'" + Environment.NewLine;

                        }
                    }
                    else
                    {
                        int idex_save2 = Array.IndexOf(data_query, result_input[i].ToString().ToUpper().Trim());
                        if (idex_save2 != -1)
                        {
                            kq = kq + "AND " + result_input[i].ToString().ToLower().Trim() + ".phanvung_id" + " = '|| p_phanvung_id ||'" + Environment.NewLine;

                        }
                    }

                }

            }
            if (kq == "")
            {
                return "Bảng của anh không có trường phanvung_id";
            }
            else
            {
                return kq;
            }    
               
        }
 
        // Lấy các mes gửi trên telegram và xủa lý
        private void Bl_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.Text)
            {
                var chuoinhanduoc = e.Message.Text;
                string[] chuoicat = chuoinhanduoc.Split(' ');
                switch (chuoicat[0])
                {
                    case "/s":
                        {
                            ConnectionDatabase conn = new ConnectionDatabase();
                            var table = conn.Get_Table();
                            int i = 0;
                            foreach (DataRow dr in table.Rows)
                            {
                                data_query[i] = dr[0].ToString().Trim();
                                i++;
                            }

                            var result = e.Message.Text;
                            string[] chuoi = result.Split("/s");
                            string sql = chuoi[1].ToString();
                            string kq = XuLy(sql);
                            SendTelegram( kq, e, 1);
                           
                            break;
                        }
                    case "/gs":
                     {
                           
                            var result = e.Message.Text;
                            ConnectionDatabase bd = new ConnectionDatabase();
                            result = bd.XuLychuoi(result);
                            if(result == "")
                            {
                                SendVideo(e);
                            }
                            else
                            {
                                SendTelegram(result, e, 2);
                             
                            }                  
                            break;
                      }
                    case "/cafe":
                        {
                            Random nd = new Random();
                            int kq = nd.Next(1, 3);
                            SendTelegram(kq.ToString(), e, 3);
                            break;
                        }
                    case "/f":
                        {
                            ConnectionDatabase conn = new ConnectionDatabase();
                            var table = conn.Get_Table();
                            int i = 0;
                            foreach (DataRow dr in table.Rows)
                            {
                                data_query[i] = dr[0].ToString().Trim();
                                i++;
                            }
                            SendTelegram("", e, 4);
                            break;
                        }
                    default:
                        SendVideo(e);
                        break;
                }
            }
        }

        // button xử lý trên form
        private void button1_Click(object sender, EventArgs e)
        {
            textBox2.Text = XuLy(textBox1.Text);
            var chuoi = textBox1.Text;
            Call_Telegaram_BotChua(chuoi);

        }
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                button1.PerformClick();
            }
            if (e.KeyChar == 13)
            {
                button1.PerformClick();
            }
        }

        // Xử lý trước khi gọi đến hàm gửi tn.
        private static void Call_SendTelegram(string chuoi, MessageEventArgs e)
        {
            var chatid = e.Message.Chat;
            //SendTelegram("1466248387", chuoi, e);

        }
        private static void Call_Telegaram_BotChua(string chuoi)
        {
            string[] catchuoi = chuoi.Split(' ');
            switch(catchuoi[0])
            {
                case "/gs":
                    {
                        ConnectionDatabase db = new ConnectionDatabase();
                        var tb1 = db.XuLychuoi(chuoi.Replace("/gs","").ToString());
                        break;
                    }
            }
        }
        // Hàm gửi video
        public static async void SendVideo(MessageEventArgs e)
        {
            try
            {
                var id = e.Message.Chat;
                Random random = new Random();
                int rd = random.Next(1, 5);
                FileStream fs = File.OpenRead(@"C:\Users\Hoan\Pictures\Telegram_Send\" + rd + ".gif");
                InputOnlineFile Funny = new InputOnlineFile(fs, "Funny.gif");
                await bl.SendVideoAsync(chatId: id , video: Funny, caption: "Trầm cảm luôn !!!!");
            }
            catch (Exception ex)
            {
                return;
            }
        }
    }
}
