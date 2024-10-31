using System;                                 
using System.Collections.Generic;              
using System.Linq;                             
using System.Net.Sockets;                      
using System.Text;                             
using System.Threading;                         
using System.Threading.Tasks;                  
using System.Windows.Forms;                    

namespace XOXO_GAME                            
{
    class GameClient                          // Định nghĩa lớp GameClient
    {
        private TcpClient client;              // Đối tượng TcpClient cho kết nối mạng
        private NetworkStream stream;           // Luồng mạng để gửi/nhận dữ liệu

        public async Task ConnectAsync(string serverIP) // Kết nối tới server
        {
            client = new TcpClient();          // Khởi tạo TcpClient
            await client.ConnectAsync(serverIP, 8888); // Kết nối đến IP server trên cổng 8888
            stream = client.GetStream();       // Lấy luồng mạng từ client
            MessageBox.Show(Message("Bạn là Client đó, cẩn thận thằng bên kia đánh cháy lắm!")); // Thông báo cho người dùng

            await ReceiveDataAsync();           // Bắt đầu nhận dữ liệu từ server
        }

        public async Task ReceiveDataAsync()    // Nhận dữ liệu từ server
        {
            byte[] buffer = new byte[1024];     // Bộ đệm để lưu dữ liệu nhận
            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0) // Đọc dữ liệu
            {
                string data = Encoding.UTF8.GetString(buffer, 0, bytesRead); // Chuyển đổi dữ liệu sang chuỗi

                if (data == "END")                  // Nếu nhận được tín hiệu kết thúc
                {
                    MessageBox.Show(Message("X thắng rồi hehe - Nhận từ server!"), Message("Kết quả")); // Thông báo kết quả
                    Form1 form = (Form1)Application.OpenForms["Form1"]; // Lấy đối tượng Form1
                    form.ResetGame();            // Đặt lại trò chơi
                }
                else if (data == "HOA")           // Nếu hòa
                {
                    MessageBox.Show(Message("Hoà mất tiêu rùi - Nhận từ server!"), Message("Kết quả")); // Thông báo hòa
                    Form1 form = (Form1)Application.OpenForms["Form1"];
                    form.ResetGame();            // Đặt lại trò chơi
                }
                else
                    ProcessReceivedData(data);   // Xử lý dữ liệu nhận được
            }
        }

        private void ProcessReceivedData(string data) // Xử lý dữ liệu nhận
        {
            string[] parts = data.Split(',');       // Tách dữ liệu thành các phần
            if (parts.Length == 2)                   // Kiểm tra số phần
            {
                string mark = parts[0];             // Lấy ký hiệu
                string cellId = parts[1];           // Lấy ID ô
                UpdateBoard(cellId, mark);          // Cập nhật bảng
            }
            Form1.turnCountLAN++;                    // Tăng số lượt
        }

        private void UpdateBoard(string cellId, string mark) // Cập nhật bảng trò chơi
        {
            Form1 form = (Form1)Application.OpenForms["Form1"]; // Lấy đối tượng Form1
            if (form != null)
            {
                if (form.InvokeRequired)               // Kiểm tra xem có cần gọi từ luồng khác không
                {
                    form.Invoke(new Action(() => UpdateBoard(cellId, mark))); // Gọi lại phương thức trong luồng chính
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

        public async Task SendDataAsync(string message) // Gửi dữ liệu tới server
        {
            if (stream != null)                       // Kiểm tra luồng có tồn tại không
            {
                byte[] data = Encoding.ASCII.GetBytes(message); // Chuyển đổi chuỗi thành mảng byte
                await stream.WriteAsync(data, 0, data.Length); // Gửi dữ liệu
            }
        }

        public void Disconnect()                     // Ngắt kết nối
        {
            stream?.Close();                         // Đóng luồng
            client?.Close();                         // Đóng client
        }

        private static string NetworkID = "";       // Khai báo biến lưu NetworkID
        public static string GetNetworkID()         // Phương thức lấy NetworkID
        {
            return Form1.GetNetworkID();            // Gọi phương thức từ Form1
        }

        public static string Message(string s)       // Phương thức trả về chuỗi
        {
            return s;                                // Trả về chuỗi đầu vào
        }
    }
}