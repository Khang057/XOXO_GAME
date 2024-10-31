using System;                                 
using System.Collections.Generic;              
using System.ComponentModel;                  
using System.Data;                             
using System.Drawing;                          
using System.Linq;                             
using System.Text;                             
using System.Threading.Tasks;                  
using System.Windows.Forms;                    

namespace XOXO_GAME                            // Khai báo không gian tên cho ứng dụng
{
    public partial class Form1 : Form         // Định nghĩa lớp Form1 kế thừa từ Form
    {
        private string mode = "BOT";           // Chế độ chơi: BOT hoặc PLAYER
        private string crrPlayer;               // Người chơi hiện tại
        private int turnCount;                   // Số lượt chơi
        public static int turnCountLAN = 0;     // Số lượt chơi trong chế độ LAN
        private bool isWaitingBot = false;       // Kiểm tra xem bot có đang suy nghĩ không

        public Form1()                          // Constructor của Form1
        {
            InitializeComponent();              // Khởi tạo các thành phần giao diện
        }

        private void InitializeGame()            // Khởi tạo trò chơi
        {
            crrPlayer = "X";                     // Người chơi đầu tiên
            turnCount = 0;                       // Đặt số lượt về 0
            turnCountLAN = 0;                    // Đặt số lượt LAN về 0

            foreach (Control control in this.Controls) // Đặt lại văn bản cho tất cả các nút
            {
                if (control is Button button && button.Name.StartsWith("btnCell"))
                {
                    button.Text = string.Empty;
                }
            }
        }

        private async void Button_Click(object sender, EventArgs e) // Xử lý sự kiện khi nút được nhấn
        {
            Button clickedButton = (Button)sender; // Lấy nút đã nhấn

            if (isWaitingBot)                      // Kiểm tra nếu bot đang suy nghĩ
            {
                MessageBox.Show("Này này! Đợi máy suy nghĩ đã chứ.", "Cảnh báo");
                return;
            }

            if (string.IsNullOrEmpty(clickedButton.Text)) // Kiểm tra ô có trống không
            {
                if (mode == "PLAYER")               // Chế độ chơi người với người
                {
                    if (isHost == false)           // Nếu client
                    {
                        if (turnCountLAN % 2 == 0) return; // Kiểm tra lượt
                        await client.SendDataAsync($"O,{clickedButton.Name}"); // Gửi dữ liệu tới server
                        clickedButton.Text = "O";   // Cập nhật ô
                        turnCountLAN++;
                    }
                    else if (isHost == true)        // Nếu host
                    {
                        if (turnCountLAN % 2 == 1) return; // Kiểm tra lượt
                        await server.SendDataAsync($"X,{clickedButton.Name}"); // Gửi dữ liệu tới client
                        clickedButton.Text = "X";   // Cập nhật ô
                        turnCountLAN++;
                    }

                    if (CheckWinner())               // Kiểm tra xem có người thắng không
                    {
                        if (isHost)
                        {
                            MessageBox.Show("X thắng rồi hehe!", "Kết quả");
                            await server.SendDataAsync($"END");
                        }
                        else
                        {
                            MessageBox.Show("O thắng rồi hehe!", "Kết quả");
                            await client.SendDataAsync($"END");
                        }
                        ResetGame();                // Đặt lại trò chơi
                    }
                    else if (turnCountLAN == 9)    // Kiểm tra hòa
                    {
                        MessageBox.Show("Hoà mất tiuu, hicc!", "Kết quả");
                        if (isHost)
                        {
                            await server.SendDataAsync($"HOA");
                        }
                        else
                        {
                            await client.SendDataAsync($"HOA");
                        }
                        ResetGame();                // Đặt lại trò chơi
                    }
                }
                else                                 // Chế độ chơi với bot
                {
                    clickedButton.Text = crrPlayer; // Cập nhật ô
                    turnCount++;

                    if (CheckWinner())               // Kiểm tra thắng
                    {
                        MessageBox.Show($"{crrPlayer} thắng rồi hehe!", "Kết quả");
                        ResetGame();                // Đặt lại trò chơi
                    }
                    else if (turnCount == 9)        // Kiểm tra hòa
                    {
                        MessageBox.Show("Hoà mất tiuu, hicc!", "Kết quả");
                        ResetGame();                // Đặt lại trò chơi
                    }
                    crrPlayer = (crrPlayer == "X") ? "O" : "X"; // Đổi lượt
                    AIPlay();                       // Gọi bot chơi
                }
            }
            else
            {
                MessageBox.Show("Ô này người ta đánh rồi mà? Chơi ăn gian hả!", "Cảnh báo");
            }
        }

        private async void AIPlay()                // Phương thức cho bot chơi
        {
            lbPlayer1.Text = "NGƯỜI CHƠI (CHỜ)";
            lbPlayer2.Text = "MÁY AI SIÊU CẤP ĐỊA CẦU (ĐẾN LƯỢT)";

            isWaitingBot = true;                  // Đánh dấu bot đang suy nghĩ

            for (int i = 1; i <= 2; i++)          // Đếm thời gian suy nghĩ
            {
                lbPlayer2.Text = $"MÁY AI SIÊU CẤP ĐỊA CẦU (SUY NGHĨ - {i}s)";
                await Task.Delay(1000);          // Đợi 1 giây
            }

            isWaitingBot = false;                 // Đánh dấu bot đã suy nghĩ xong

            if (turnCount == 1)                   // Nếu lượt đầu tiên
            {
                Button cell5 = (Button)this.Controls["btnCell5"]; // Tìm ô giữa
                if (cell5.Text == "")
                {
                    cell5.Text = "O";             // Đánh ô giữa
                }
                else
                {
                    Button cell1 = (Button)this.Controls["btnCell1"]; // Nếu giữa đã đánh, đánh ô khác
                    cell1.Text = "O";
                }
                turnCount++;

                if (CheckWinner())               // Kiểm tra thắng
                {
                    MessageBox.Show($"{crrPlayer} thắng rồi hehe!", "Kết quả");
                    ResetGame();
                    return;
                }
                else if (turnCount == 9)        // Kiểm tra hòa
                {
                    MessageBox.Show("Hoà mất tiuu, hicc!", "Kết quả");
                    ResetGame();
                    return;
                }

                lbPlayer1.Text = "NGƯỜI CHƠI (ĐẾN LƯỢT)";
                lbPlayer2.Text = "MÁY AI SIÊU CẤP ĐỊA CẦU (CHỜ)";
                crrPlayer = "X"; // Đổi lượt
                return;
            }

            for (int i = 1; i <= 9; i++)          // Kiểm tra thắng
            {
                Button button = (Button)this.Controls["btnCell" + i];

                if (button.Text == "")
                {
                    button.Text = "O";

                    if (CheckWinner())               // Kiểm tra thắng
                    {
                        MessageBox.Show($"{crrPlayer} thắng rồi hehe!", "Kết quả");
                        ResetGame();
                        return;
                    }
                    else
                    {
                        button.Text = "";           // Đặt lại nếu không thắng
                    }
                }
            }

            Random random = new Random();          // Chọn ngẫu nhiên
            List<Button> emptyCells = new List<Button>();

            for (int i = 1; i <= 9; i++)          // Tìm ô trống
            {
                Button button = (Button)this.Controls["btnCell" + i];
                if (button.Text == "")
                {
                    emptyCells.Add(button);
                }
            }

            if (emptyCells.Count > 0)              // Nếu có ô trống
            {
                int index = random.Next(emptyCells.Count);
                emptyCells[index].Text = "O";    // Đánh ô ngẫu nhiên
                turnCount++;
            }

            if (CheckWinner())                      // Kiểm tra thắng
            {
                MessageBox.Show($"{crrPlayer} thắng rồi hehe!", "Kết quả");
                ResetGame();
            }
            else if (turnCount == 9)               // Kiểm tra hòa
            {
                MessageBox.Show("Hoà mất tiuu, hicc!", "Kết quả");
                ResetGame();
            }

            lbPlayer1.Text = "NGƯỜI CHƠI (ĐẾN LƯỢT)";
            lbPlayer2.Text = "MÁY AI SIÊU CẤP ĐỊA CẦU (CHỜ)";
            crrPlayer = "X"; // Đổi lượt
        }

        private bool CheckWinner()                 // Kiểm tra ai thắng
        {
            return
                (Check(btnCell1, btnCell2, btnCell3) || // Kiểm tra hàng
                 Check(btnCell4, btnCell5, btnCell6) ||
                 Check(btnCell7, btnCell8, btnCell9) ||
                 Check(btnCell1, btnCell4, btnCell7) || // Kiểm tra cột
                 Check(btnCell2, btnCell5, btnCell8) ||
                 Check(btnCell3, btnCell6, btnCell9) ||
                 Check(btnCell1, btnCell5, btnCell9) || // Kiểm tra chéo
                 Check(btnCell3, btnCell5, btnCell7));
        }

        private bool Check(Button btn1, Button btn2, Button btn3) // Kiểm tra ba nút
        {
            if (mode == "PLAYER")                  // Chế độ chơi người
            {
                if (isHost)
                {
                    if (btn1.Text == "X" && btn2.Text == "X" && btn3.Text == "X")
                        return true;              // Nếu X thắng
                }
                else
                {
                    if (btn1.Text == "O" && btn2.Text == "O" && btn3.Text == "O")
                        return true;              // Nếu O thắng
                }
            }
            else                                    // Chế độ chơi với bot
            {
                if (btn1.Text == crrPlayer && btn2.Text == crrPlayer && btn3.Text == crrPlayer)
                {
                    return true;                  // Nếu người chơi thắng
                }
            }
            return false;                           // Không có ai thắng
        }

        public void ResetGame()                   // Đặt lại trò chơi
        {
            InitializeGame();                     // Khởi tạo lại trò chơi
        }

        private void btnPlayWithBot_Click(object sender, EventArgs e) // Bắt đầu chơi với bot
        {
            this.mode = "BOT";
            lbPlayer1.Text = "NGƯỜI CHƠI (ĐẾN LƯỢT)";
            lbPlayer2.Text = "MÁY AI SIÊU CẤP ĐỊA CẦU (CHỜ)";
            ResetGame();                         // Đặt lại trò chơi
        }

        private GameServer server;                // Đối tượng server
        private GameClient client;                // Đối tượng client
        public static bool isHost = false;        // Kiểm tra xem có phải host không

        private void btnPlayWithPlayer_Click(object sender, EventArgs e) // Bắt đầu chơi với người
        {
            if (MessageBox.Show("Bạn là Host thì bấm Yes nhó!!", "Phải Host hông?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                isHost = true;                     // Nếu là host
                server = new GameServer();         // Khởi tạo server
                _ = Task.Run(async () => await server.StartServerAsync()); // Bắt đầu server
            }
            else
            {
                isHost = false;                    // Nếu là client
                client = new GameClient();
                _ = Task.Run(async () => await client.ConnectAsync("127.0.0.1")); // Kết nối tới server
            }

            mode = "PLAYER";                      // Chế độ chơi
            string playerRole = isHost ? "HOST" : "CLIENT"; // Lấy vai trò người chơi
            lbPlayer1.Text = Message($"NGƯỜI CHƠI ({playerRole})");
            lbPlayer2.Text = Message($"NGƯỜI CHƠI VIP PRO NÀO ĐẤY");
            ResetGame();                         // Đặt lại trò chơi
        }

        private static string NetworkID = "";     // Khai báo NetworkID
        public static string GetNetworkID()       // Lấy NetworkID
        {
            if (NetworkID == "")
                NetworkID = GenerateRandomString(3); // Tạo NetworkID ngẫu nhiên
            return NetworkID;
        }

        public static string GenerateRandomString(int n) // Tạo chuỗi ngẫu nhiên
        {
            Random random = new Random();
            const string chars = "0123456789"; // Chỉ số
            char[] result = new char[n];

            for (int i = 0; i < n; i++)
            {
                result[i] = chars[random.Next(chars.Length)];
            }

            return new string(result);           // Trả về chuỗi ngẫu nhiên
        }

        public static string Message(string s)   // Trả về chuỗi thông điệp
        {
            return s;                            // Trả về thông điệp
        }

        // Thêm sự kiện nhấp chuột cho tất cả các nút trong trò chơi
        private void btnCell1_Click(object sender, EventArgs e) => Button_Click(sender, e);
        private void btnCell2_Click(object sender, EventArgs e) => Button_Click(sender, e);
        private void btnCell3_Click(object sender, EventArgs e) => Button_Click(sender, e);
        private void btnCell4_Click(object sender, EventArgs e) => Button_Click(sender, e);
        private void btnCell5_Click(object sender, EventArgs e) => Button_Click(sender, e);
        private void btnCell6_Click(object sender, EventArgs e) => Button_Click(sender, e);
        private void btnCell7_Click(object sender, EventArgs e) => Button_Click(sender, e);
        private void btnCell8_Click(object sender, EventArgs e) => Button_Click(sender, e);
        private void btnCell9_Click(object sender, EventArgs e) => Button_Click(sender, e);
    }
}