using System;
using System.Windows.Forms;
using DoAnCuoiKy_Dijkstra.UI; // Khai báo sử dụng file UI của chúng ta

namespace DoAnCuoiKy_Dijkstra
{
    static class Program
    {
        /// <summary>
        /// Điểm bắt đầu của toàn bộ ứng dụng.
        /// </summary>
        [STAThread] // Bắt buộc phải có với ứng dụng WinForms
        static void Main()
        {
            // Cài đặt phong cách giao diện mặc định
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            Welcome welcome=new Welcome();
            if (welcome.ShowDialog()==DialogResult.OK)//khi nhấn bắt đầu chạy giao diện MainForm
            {
                 Application.Run(new MainForm());
            }
        }
    }
}
