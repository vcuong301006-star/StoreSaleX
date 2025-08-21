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
    public partial class ProductManagementForm : Form
    {
        string connectionString = @"Data Source=VC\SQLEXPRESS;Initial Catalog=StoreSale;Integrated Security=True";

        public ProductManagementForm()
        {
            InitializeComponent();
            LoadCategories();
            LoadSuppliers();
            LoadProducts();
        }

        private void LoadCategories()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter("SELECT CategoryID, CategoryName FROM dbo.Category", conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    cbCategory.DisplayMember = "CategoryName";
                    cbCategory.ValueMember = "CategoryID";
                    cbCategory.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi load Categories: " + ex.Message);
            }
        }

        private void LoadSuppliers()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter("SELECT SupplierID, SupplierName FROM dbo.Supplier", conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    cbSupplier.DisplayMember = "SupplierName";
                    cbSupplier.ValueMember = "SupplierID";
                    cbSupplier.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi load Suppliers: " + ex.Message);
            }
        }

        private void LoadProducts(string filter = "")
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string sql = @"SELECT ProductID, ProductName, Price, CategoryID, SupplierID, StockQuantity, Description 
                           FROM dbo.Product";

                    if (!string.IsNullOrEmpty(filter))
                    {
                        sql += " WHERE ProductName LIKE @filter";
                    }

                    SqlCommand cmd = new SqlCommand(sql, conn);

                    if (!string.IsNullOrEmpty(filter))
                    {
                        cmd.Parameters.AddWithValue("@filter", "%" + filter + "%");
                    }

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgvProducts.DataSource = dt;

                    // Ẩn các cột ID nếu muốn
                    dgvProducts.Columns["ProductID"].Visible = false;
                    dgvProducts.Columns["CategoryID"].Visible = false;
                    dgvProducts.Columns["SupplierID"].Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi load Products: " + ex.Message);
            }
        }


        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtProductName.Text))
            {
                MessageBox.Show("Tên sản phẩm không được để trống.");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string sql = @"INSERT INTO dbo.Product (ProductName, Price, CategoryID, SupplierID, StockQuantity, Description)
                               VALUES (@name, @price, @categoryId, @supplierId, @stockQty, @desc)";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@name", txtProductName.Text.Trim());
                cmd.Parameters.AddWithValue("@price", nudPrice.Value);
                cmd.Parameters.AddWithValue("@categoryId", cbCategory.SelectedValue);
                cmd.Parameters.AddWithValue("@supplierId", cbSupplier.SelectedValue);
                cmd.Parameters.AddWithValue("@stockQty", (int)nudStockQuantity.Value);
                cmd.Parameters.AddWithValue("@desc", txtDescription.Text.Trim());

                int result = cmd.ExecuteNonQuery();

                if (result > 0)
                {
                    MessageBox.Show("Thêm sản phẩm thành công!");
                    LoadProducts();
                    ClearInputs();
                }
                else
                {
                    MessageBox.Show("Thêm sản phẩm thất bại.");
                }
            }
        }

        private void ClearInputs()
        {
            txtProductName.Clear();
            nudPrice.Value = 0;
            if (cbCategory.Items.Count > 0) cbCategory.SelectedIndex = 0;
            if (cbSupplier.Items.Count > 0) cbSupplier.SelectedIndex = 0;
            nudStockQuantity.Value = 0;
            txtDescription.Clear();
        }


        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn sản phẩm để xóa.");
                return;
            }

            int productId = Convert.ToInt32(dgvProducts.CurrentRow.Cells["ProductID"].Value);

            var confirm = MessageBox.Show("Bạn có chắc muốn xóa sản phẩm này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string sql = "DELETE FROM dbo.Product WHERE ProductID = @id";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@id", productId);

                    int result = cmd.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Xóa sản phẩm thành công!");
                        LoadProducts();
                        ClearInputs();
                    }
                    else
                    {
                        MessageBox.Show("Xóa sản phẩm thất bại.");
                    }
                }
            }
        }

        private void btbUpdate_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn sản phẩm để cập nhật.");
                return;
            }

            int productId = Convert.ToInt32(dgvProducts.CurrentRow.Cells["ProductID"].Value);

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string sql = @"UPDATE dbo.Product
                               SET ProductName = @name, Price = @price, CategoryID = @categoryId,
                                   SupplierID = @supplierId, StockQuantity = @stockQty, Description = @desc
                               WHERE ProductID = @id";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@name", txtProductName.Text.Trim());
                cmd.Parameters.AddWithValue("@price", nudPrice.Value);
                cmd.Parameters.AddWithValue("@categoryId", cbCategory.SelectedValue);
                cmd.Parameters.AddWithValue("@supplierId", cbSupplier.SelectedValue);
                cmd.Parameters.AddWithValue("@stockQty", (int)nudStockQuantity.Value);
                cmd.Parameters.AddWithValue("@desc", txtDescription.Text.Trim());
                cmd.Parameters.AddWithValue("@id", productId);

                int result = cmd.ExecuteNonQuery();

                if (result > 0)
                {
                    MessageBox.Show("Cập nhật sản phẩm thành công!");
                    LoadProducts();
                    ClearInputs();
                }
                else
                {
                    MessageBox.Show("Cập nhật sản phẩm thất bại.");
                }
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadProducts(txtSearch.Text.Trim());
        }

        private void btnRefesh_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();
            LoadProducts();
        }
    }
}
