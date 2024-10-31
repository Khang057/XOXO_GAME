using System;                                 
using System.Collections.Generic;              
using System.Linq;                             
using System.Net.Sockets;                      
using System.Net;                              
using System.Text;                             
using System.Threading;                         
using System.Threading.Tasks;                  
using System.Windows.Forms;                    

namespace XOXO_GAME                            
{
    class GameServer                          // Định nghĩa lớp GameServer
    {
        private TcpListener listener;          // Đối tượng TcpListener để lắng nghe kết nối đến
        private TcpClient client;                // Đối tượng TcpClient cho kết nối mạng
        private NetworkStream stream;           // Luồng mạng để gửi/nhận dữ liệu

        public async Task StartServerAsync()    // Khởi động server
        {
            listener = new TcpListener(IPAddress.Any, 8888); // Lắng nghe trên cổng 8888
            listener.Start();                   // Bắt đầu lắng nghe

            MessageBox.Show("Server lên rồi, chờ tí thằng khác vô"); // Thông báo server đã khởi động

            client = await listener.AcceptTcpClientAsync(); // Chấp nhận kết nối từ client
            MessageBox.Show("CẢNH BÁO! Có 1 cao nhân đã vào!!!"); // Thông báo có client kết nối

            stream = client.GetStream();       // Lấy luồng mạng từ client

            await ReceiveDataAsync();           // Bắt đầu nhận dữ liệu từ client
        }

        private async Task ReceiveDataAsync()    // Nhận dữ liệu từ client
        {
            byte[] buffer = new byte[1024];     // Bộ đệm để lưu dữ liệu nhận
            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0) // Đọc dữ liệu
            {
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead); // Chuyển đổi dữ liệu sang chuỗi

                if (message == "END")              // Nếu nhận được tín hiệu kết thúc
                {
                    MessageBox.Show("O thắng rồi hehe! - Nhận từ client", "Kết quả"); // Thông báo kết quả
                    Form1 form = (Form1)Application.OpenForms["Form1"]; // Lấy đối tượng Form1
                    form.ResetGame();            // Đặt lại trò chơi
                }
                else if (message == "HOA")         // Nếu hòa
                {
                    MessageBox.Show("Hoà mất tiêu rùi! - Nhận từ client", "Kết quả"); // Thông báo hòa
                    Form1 form = (Form1)Application.OpenForms["Form1"];
                    form.ResetGame();            // Đặt lại trò chơi
                }
                else
                    ProcessReceivedData(message);   // Xử lý dữ liệu nhận được
            }
        }

        private void ProcessReceivedData(string data) // Xử lý dữ liệu nhận
        {
            string[] parts = data.Split(',');       // Tách dữ liệu thành các phần
            if (parts.Length == 2)                   // Kiểm tra số phần
            {
                string mark = parts[0];             // Lấy ký hiệu
                string cellId = parts[1];           // Lấy ID ô
                UpdateBoardOnServer(cellId, mark);  // Cập nhật bảng trên server
            }
            Form1.turnCountLAN++;                    // Tăng số lượt
        }

        private void UpdateBoardOnServer(string cellId, string mark) // Cập nhật bảng trò chơi
        {
            Form1 form = (Form1)Application.OpenForms["Form1"]; // Lấy đối tượng Form1
            if (form != null)
            {
                if (form.InvokeRequired)               // Kiểm tra xem có cần gọi từ luồng khác không
                {
                    form.Invoke(new Action(() => UpdateBoardOnServer(cellId, mark))); // Gọi lại phương thức trong luồng chính
                }
                else
                {
                    Button targetButton = form.Controls.Find(cellId, true).FirstOrDefault() as Button; // Tìm nút theo ID
                    if (targetButton != null)          // Nếu tìm thấy nút
                    {
                        targetButton.Text = mark;      // Cập nhật văn bản của nút
                    }
                }
            }
        }

        public async Task SendDataAsync(string message) // Gửi dữ liệu tới client
        {
            if (stream != null)                       // Kiểm tra luồng có tồn tại không
            {
                byte[] data = Encoding.UTF8.GetBytes(message); // Chuyển đổi chuỗi thành mảng byte
                await stream.WriteAsync(data, 0, data.Length); // Gửi dữ liệu
            }
        }

        public void StopServer()                     // Dừng server
        {
            stream?.Close();                         // Đóng luồng
            client?.Close();                         // Đóng client
            listener?.Stop();                       // Dừng lắng nghe
            Console.WriteLine("Server stopped.");   // Thông báo server đã dừng
        }
    }
}