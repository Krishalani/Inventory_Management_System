using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace InventoryManagementSystem
{
    public partial class UserForm : Form
    {
        SqlConnection con = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=StockSpot;Integrated Security=True;Connect Timeout=30");
        SqlCommand cm = new SqlCommand();
        SqlDataReader dr;

        public UserForm()
        {
            InitializeComponent();
            CheckUserPermissions();
            LoadUser();
        }

        private void CheckUserPermissions()
        {
            if (LoginForm.CurrentUserRole != "SuperAdmin")
            {
                btnAdd.Enabled = false;
                btnAdd.Visible = false;
            }
        }

        public void LoadUser()
        {
            try
            {
                int i = 0;
                dgvUser.Rows.Clear();
                cm = new SqlCommand("SELECT * FROM tbUser ORDER BY role, username", con);
                con.Open();
                dr = cm.ExecuteReader();

                while (dr.Read())
                {
                    i++;
                    dgvUser.Rows.Add(
                        i,
                        dr["username"].ToString(),
                        dr["fullname"].ToString(),
                        dr["password"].ToString(),
                        dr["role"]?.ToString() ?? "User",
                        dr["phone"]?.ToString() ?? ""
                    // Edit and Delete buttons are automatically added based on column setup
                    );
                }
                dr.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading users: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                con.Close();
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (LoginForm.CurrentUserRole != "SuperAdmin")
            {
                MessageBox.Show("Only Super Administrator can add new users.", "Access Denied",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            UserModuleForm userModule = new UserModuleForm();
            userModule.btnSave.Enabled = true;
            userModule.btnUpdate.Enabled = false;
            userModule.txtUserName.Enabled = true;
            userModule.ShowDialog();
            LoadUser();
        }

       

        private void dgvUser_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvUser.Rows.Count) return;

            DataGridViewRow row = dgvUser.Rows[e.RowIndex];

            // Get values by column names instead of indices
            string username = GetCellValue(row, "User Name");
            string fullname = GetCellValue(row, "Full Name");
            string password = GetCellValue(row, "Password");
            string phone = GetCellValue(row, "Phone");

            string colName = dgvUser.Columns[e.ColumnIndex].Name;

            if (colName == "Edit" || dgvUser.Columns[e.ColumnIndex].HeaderText == "Edit")
            {
                if (LoginForm.CurrentUserRole != "SuperAdmin")
                {
                    MessageBox.Show("Only Super Administrator can edit users.", "Access Denied",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                UserModuleForm userModule = new UserModuleForm();
                userModule.txtUserName.Text = username;
                userModule.txtFullName.Text = fullname;
                userModule.txtPass.Text = password;
                userModule.txtRepass.Text = password;
                userModule.txtPhone.Text = phone;

                userModule.btnSave.Enabled = false;
                userModule.btnUpdate.Enabled = true;
                userModule.txtUserName.Enabled = false;
                userModule.ShowDialog();
                LoadUser();
            }
            else if (colName == "Delete" || dgvUser.Columns[e.ColumnIndex].HeaderText == "Delete")
            {
                // Delete logic using the variables above...
            }
        }

        // Helper method to get cell value by column header text
        private string GetCellValue(DataGridViewRow row, string columnHeader)
        {
            foreach (DataGridViewCell cell in row.Cells)
            {
                if (dgvUser.Columns[cell.ColumnIndex].HeaderText == columnHeader)
                {
                    return cell.Value?.ToString() ?? "";
                }
            }
            return "";
        }
        private void dgvUser_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
    }
}