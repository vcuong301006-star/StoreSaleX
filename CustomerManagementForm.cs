using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace WindowsFormsApp6
{
    public partial class CustomerManagementForm : Form
    {
        private string connectionString = @"Server=.\SQLEXPRESS;Database=StoreSale;Trusted_Connection=True;";
        private int selectedCustomerId = -1;

        public CustomerManagementForm()
        {
            InitializeComponent();
            LoadCustomerData();

           
            dgvCustomers.CellClick += dgvCustomers_CellClick;

           
            btnAdd.Enabled = true;
            btnUpdate.Enabled = false;
            btnDelete.Enabled = false;
        }

        private void LoadCustomerData(string search = "")
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlDataAdapter adapter = new SqlDataAdapter(
                @"SELECT CustomerID, CustomerName, Email, Phone, Address, CreatedAt 
          FROM dbo.Customer
          WHERE CustomerName LIKE @search OR Email LIKE @search
          ORDER BY CustomerName", conn))
            {
                adapter.SelectCommand.Parameters.AddWithValue("@search", $"%{search}%");

                DataTable dt = new DataTable();
                adapter.Fill(dt);
                dgvCustomers.DataSource = dt;

                if (dgvCustomers.Columns["CustomerID"] != null)
                    dgvCustomers.Columns["CustomerID"].Visible = false;

                dgvCustomers.AutoResizeColumns();
            }
        }


        private void dgvCustomers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvCustomers.Rows[e.RowIndex];

                if (row.Cells["CustomerID"].Value != null)
                {
                    selectedCustomerId = Convert.ToInt32(row.Cells["CustomerID"].Value);

                    
                    txtCustomerName.Text = row.Cells["CustomerName"].Value?.ToString();
                    txtEmail.Text = row.Cells["Email"].Value?.ToString();
                    txtPhone.Text = row.Cells["Phone"].Value?.ToString();
                    txtAddress.Text = row.Cells["Address"].Value?.ToString();

                 
                    btnAdd.Enabled = false;
                    btnUpdate.Enabled = true;
                    btnDelete.Enabled = true;
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCustomerName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên khách hàng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(
                @"INSERT INTO dbo.Customer (CustomerName, Email, Phone, Address, CreatedAt)
          VALUES (@name, @email, @phone, @address, SYSUTCDATETIME())", conn))
            {
                cmd.Parameters.AddWithValue("@name", txtCustomerName.Text);
                cmd.Parameters.AddWithValue("@email", string.IsNullOrWhiteSpace(txtEmail.Text) ? DBNull.Value : (object)txtEmail.Text);
                cmd.Parameters.AddWithValue("@phone", string.IsNullOrWhiteSpace(txtPhone.Text) ? DBNull.Value : (object)txtPhone.Text);
                cmd.Parameters.AddWithValue("@address", string.IsNullOrWhiteSpace(txtAddress.Text) ? DBNull.Value : (object)txtAddress.Text);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            ClearInputFields();
            LoadCustomerData();
            MessageBox.Show("Thêm khách hàng thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ClearInputFields()
        {
            selectedCustomerId = -1;
            txtCustomerName.Clear();
            txtEmail.Clear();
            txtPhone.Clear();
            txtAddress.Clear();

            // Reset nút
            btnAdd.Enabled = true;
            btnUpdate.Enabled = false;
            btnDelete.Enabled = false;
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedCustomerId == -1)
            {
                MessageBox.Show("Vui lòng chọn khách hàng để cập nhật.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtCustomerName.Text))
            {
                MessageBox.Show("Tên khách hàng không được để trống.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"UPDATE dbo.Customer
                                 SET CustomerName = @name, Email = @email, Phone = @phone, Address = @address
                                 WHERE CustomerID = @id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@name", txtCustomerName.Text);
                cmd.Parameters.AddWithValue("@email", (object)txtEmail.Text ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@phone", (object)txtPhone.Text ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@address", (object)txtAddress.Text ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@id", selectedCustomerId);

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }

            ClearInputFields();
            LoadCustomerData();
            MessageBox.Show("Cập nhật khách hàng thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedCustomerId == -1)
            {
                MessageBox.Show("Vui lòng chọn khách hàng để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirmResult = MessageBox.Show("Bạn có chắc chắn muốn xóa khách hàng này?",
                                     "Xác nhận xóa",
                                     MessageBoxButtons.YesNo);
            if (confirmResult == DialogResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "DELETE FROM dbo.Customer WHERE CustomerID = @id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", selectedCustomerId);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }

                ClearInputFields();
                LoadCustomerData();
                MessageBox.Show("Xóa khách hàng thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadCustomerData(txtSearch.Text.Trim());
        }

        private void btnClearSearch_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();
            LoadCustomerData();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearInputFields();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
