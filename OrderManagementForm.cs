using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp6
{
    public partial class OrderManagementForm : Form
    {
        string connectionString = @"Data Source=VC\SQLEXPRESS;Initial Catalog=StoreSale;Integrated Security=True";

        public OrderManagementForm()
        {
            InitializeComponent();
            LoadCustomers();
            LoadPaymentMethods();
            LoadEmployees();
            LoadOrders();
        }
        private void LoadOrders()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT o.OrderID, o.OrderDate, c.CustomerID, c.CustomerName, 
                        e.EmployeeName, p.MethodName, o.OrderStatus, o.TotalAmount
                 FROM Orders o
                 JOIN Customer c ON o.CustomerID = c.CustomerID
                 JOIN Employee e ON o.EmployeeID = e.EmployeeID
                 JOIN PaymentMethod p ON o.PaymentMethodID = p.PaymentMethodID
                 ORDER BY o.OrderID DESC";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dgvOrders.DataSource = dt;

                dgvOrders.Columns["CustomerID"].Visible = false;

               
                dgvOrders.Columns["TotalAmount"].DefaultCellStyle.Format = "N0"; 
                dgvOrders.Columns["TotalAmount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void OrderManagementForm_Load(object sender, EventArgs e)
        {
            LoadPaymentMethods();
            LoadCustomers();
            LoadEmployees();
            LoadOrders();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dgvOrders.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn một đơn hàng để cập nhật.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int orderId = Convert.ToInt32(dgvOrders.SelectedRows[0].Cells["OrderID"].Value);

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string updateQuery = @"UPDATE Orders 
                                           SET OrderStatus = @OrderStatus
                                           WHERE OrderID = @OrderID";

                    SqlCommand cmd = new SqlCommand(updateQuery, conn);
                    cmd.Parameters.AddWithValue("@OrderStatus", "Hoàn thành");
                    cmd.Parameters.AddWithValue("@OrderID", orderId);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();

                    MessageBox.Show("Cập nhật đơn hàng thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadOrders();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật đơn hàng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            LoadOrders();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                int customerID = 0;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                  
                    if (!string.IsNullOrWhiteSpace(txtNewCustomerName.Text))
                    {
                        string insertCustomer = "INSERT INTO Customer (CustomerName) VALUES (@CustomerName); SELECT SCOPE_IDENTITY();";
                        using (SqlCommand cmd = new SqlCommand(insertCustomer, conn))
                        {
                            cmd.Parameters.AddWithValue("@CustomerName", txtNewCustomerName.Text.Trim());
                            customerID = Convert.ToInt32(cmd.ExecuteScalar()); 
                        }
                    }
                    else
                    {
                        
                        if (cmbCustomer.SelectedValue == null)
                        {
                            MessageBox.Show("Vui lòng chọn khách hàng hoặc nhập tên khách hàng mới!");
                            return;
                        }
                        customerID = Convert.ToInt32(cmbCustomer.SelectedValue);
                    }

                    
                    if (cmbEmployee.SelectedValue == null || cmbPaymentMethod.SelectedValue == null)
                    {
                        MessageBox.Show("Vui lòng chọn nhân viên và phương thức thanh toán.");
                        return;
                    }

                    int employeeID = Convert.ToInt32(cmbEmployee.SelectedValue);
                    int paymentMethodID = Convert.ToInt32(cmbPaymentMethod.SelectedValue);
                    DateTime orderDate = dtpOrderDate.Value;
                    string orderStatus = "Pending";

                    // Thêm đơn hàng
                    string query = @"INSERT INTO Orders 
                             (CustomerID, EmployeeID, OrderDate, PaymentMethodID, OrderStatus)
                             VALUES
                             (@CustomerID, @EmployeeID, @OrderDate, @PaymentMethodID, @OrderStatus)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CustomerID", customerID);
                        cmd.Parameters.AddWithValue("@EmployeeID", employeeID);
                        cmd.Parameters.AddWithValue("@OrderDate", orderDate);
                        cmd.Parameters.AddWithValue("@PaymentMethodID", paymentMethodID);
                        cmd.Parameters.AddWithValue("@OrderStatus", orderStatus);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Thêm đơn hàng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadOrders();
                    LoadCustomers(); 
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm đơn hàng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvOrders.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn đơn hàng để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int orderId = Convert.ToInt32(dgvOrders.SelectedRows[0].Cells["OrderID"].Value);
            DialogResult confirm = MessageBox.Show($"Bạn có chắc muốn xóa đơn hàng {orderId}?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();

                        SqlCommand delDetails = new SqlCommand("DELETE FROM dbo.OrderDetails WHERE OrderID = @OrderID", conn);
                        delDetails.Parameters.AddWithValue("@OrderID", orderId);
                        delDetails.ExecuteNonQuery();

                        SqlCommand delOrder = new SqlCommand("DELETE FROM dbo.Orders WHERE OrderID = @OrderID", conn);
                        delOrder.Parameters.AddWithValue("@OrderID", orderId);
                        int rows = delOrder.ExecuteNonQuery();

                        if (rows > 0)
                        {
                            MessageBox.Show("Xóa đơn hàng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadOrders();
                        }
                        else
                        {
                            MessageBox.Show("Xóa đơn hàng thất bại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xóa đơn hàng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void lbCustomerName_Click(object sender, EventArgs e)
        {

        }

        private void lbOrderID_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void lbOrderDate_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void LoadCustomers()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT CustomerID, CustomerName FROM Customer";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    // Gán cho ComboBox duy nhất cmbCustomer
                    cmbCustomer.DataSource = dt;
                    cmbCustomer.DisplayMember = "CustomerName";  // Cột hiển thị tên khách
                    cmbCustomer.ValueMember = "CustomerID";      // Giá trị thực tế là CustomerID
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách khách hàng: " + ex.Message);
            }

        }
        private void cmbEmployee_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void LoadPaymentMethods()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT PaymentMethodID, MethodName FROM PaymentMethod";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    cmbPaymentMethod.DataSource = dt;
                    cmbPaymentMethod.DisplayMember = "MethodName";     // Cột hiển thị
                    cmbPaymentMethod.ValueMember = "PaymentMethodID";  // Giá trị thực tế
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải phương thức thanh toán: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


        }
        private void LoadEmployees()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT EmployeeID, EmployeeName FROM Employee";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    cmbEmployee.DataSource = dt;
                    cmbEmployee.DisplayMember = "EmployeeName";
                    cmbEmployee.ValueMember = "EmployeeID";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách nhân viên: " + ex.Message);
            }
        }

        private void cmbCustomerName_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (dgvOrders.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn một đơn hàng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

           
            int customerId = Convert.ToInt32(dgvOrders.SelectedRows[0].Cells["CustomerID"].Value);

       
            string newName = txtCustomerNameEdit.Text.Trim();
            if (string.IsNullOrEmpty(newName))
            {
                MessageBox.Show("Tên khách hàng không được để trống.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string updateQuery = "UPDATE Customer SET CustomerName = @Name WHERE CustomerID = @ID";

                    using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", newName);
                        cmd.Parameters.AddWithValue("@ID", customerId);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Tên khách hàng đã được cập nhật.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                
                LoadOrders();      
                LoadCustomers();   
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật tên khách hàng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {

        }
    }
}
