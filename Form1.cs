using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApp6
{
    public partial class Form1 : Form
    {
        // Chuỗi kết nối SQL Server
        string connectionString = @"Data Source=VC\SQLEXPRESS;Initial Catalog=StoreSale;Integrated Security=True";

        public Form1()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter username and password", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Hash password nhập vào
                byte[] passwordHash = HashPassword(password);

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Lấy thêm cột Role
                    string sql = @"SELECT EmployeeID, EmployeeName, AuthorityLevel, Role
                                   FROM Employee
                                   WHERE Username = @Username AND PasswordHash = @PasswordHash";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = username;
                        cmd.Parameters.Add("@PasswordHash", SqlDbType.VarBinary, 32).Value = passwordHash;

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int employeeID = Convert.ToInt32(reader["EmployeeID"]);
                                string empName = reader["EmployeeName"].ToString();
                                int authorityLevel = Convert.ToInt32(reader["AuthorityLevel"]);
                                string role = reader["Role"].ToString(); // Lấy role từ DB

                                // Mở MainMenuForm và truyền role
                                MainMenuForm menu = new MainMenuForm(employeeID, empName, authorityLevel, role);
                                menu.Show();
                                this.Hide();
                            }
                            else
                            {
                                MessageBox.Show("Invalid username or password", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Hash password với SHA-256
        private byte[] HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.Unicode.GetBytes(password));
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
            
        }
    }
}
