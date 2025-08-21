using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp6
{
    public partial class MainMenuForm : Form
    {
        private int employeeID;
        private string employeeName;
        private int authorityLevel;
        private string currentUserRole;

        public MainMenuForm(int employeeID, string employeeName, int authorityLevel, string role)
        {
            InitializeComponent();

            this.employeeID = employeeID;
            this.employeeName = employeeName;
            this.authorityLevel = authorityLevel;
            this.currentUserRole = role;

            ApplyRolePermissions();  // Gọi hàm phân quyền ngay khi mở form
        }

        // ===== Hàm phân quyền =====
        private void ApplyRolePermissions()
        {
            if (currentUserRole == "Admin")
            {
                btnManageProducts.Enabled = true;
                btnManageCustomers.Enabled = true;
                btnManageOrders.Enabled = true;
                btnEmployeeManagement.Enabled = true;
            }
            else if (currentUserRole == "SalesStaff")
            {
                btnManageProducts.Enabled = true;
                btnManageCustomers.Enabled = true;    
                btnManageOrders.Enabled = true;          
                btnEmployeeManagement.Enabled = false;   
            }
            else if (currentUserRole == "WarehouseStaff")
            {
                btnManageProducts.Enabled = true;      
                btnManageCustomers.Enabled = false;  
                btnManageOrders.Enabled = false;         
                btnEmployeeManagement.Enabled = false;   
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
            CustomerManagementForm f = new CustomerManagementForm();
            f.ShowDialog();
        }

        private void btnManageProducts_Click(object sender, EventArgs e)
        {
            ProductManagementForm f = new ProductManagementForm();
            f.ShowDialog();
        }

        private void btnManageOrders_Click(object sender, EventArgs e)
        {
            OrderManagementForm f = new OrderManagementForm();
            f.ShowDialog();

        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            this.Hide();

            // Mở lại form đăng nhập
            Form1 loginForm = new Form1();
            loginForm.Show();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnEmployeeManagement_Click(object sender, EventArgs e)
        {
            if (currentUserRole != "Admin")
            {
                MessageBox.Show("Bạn không có quyền truy cập chức năng này!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            EmployeeManagementForm empForm = new EmployeeManagementForm();
            empForm.ShowDialog();
        }


    }
}
