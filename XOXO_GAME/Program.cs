using System;                       
using System.Collections.Generic;  
using System.Linq;                
using System.Threading.Tasks;      
using System.Windows.Forms;        

namespace XOXO_GAME                // Khai báo không gian tên cho ứng dụng
{
    internal static class Program  // Khai báo lớp tĩnh với phạm vi nội bộ
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]                 // Chỉ định luồng đơn cho giao diện người dùng
        static void Main()          // Phương thức chính của ứng dụng
        {
            Application.EnableVisualStyles();  // Kích hoạt kiểu dáng hiện đại
            Application.SetCompatibleTextRenderingDefault(false); // Sử dụng chế độ vẽ văn bản mới
            Application.Run(new Form1());  // Bắt đầu vòng lặp chính và hiển thị cửa sổ
        }
    }
}