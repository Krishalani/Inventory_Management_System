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
            SetupDataGridViewColumns();
            LoadUser();
        }

        public void LoadUser()
        {
            try
            {
                int i = 0;
                dgvUser.Rows.Clear();
                cm = new SqlCommand("SELECT * FROM tbUser", con);
                con.Open();
                dr = cm.ExecuteReader();

                while (dr.Read())
                {
                    i++;
                    // Add only the data columns (No, User Name, Full Name, Password, Phone)
                    // The button columns will automatically show "Edit"/"Delete" text
                    dgvUser.Rows.Add(
                        i,
                        dr["username"].ToString(),
                        dr["fullname"].ToString(),
                        dr["password"].ToString(),
                        dr["phone"].ToString()
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
            UserModuleForm userModule = new UserModuleForm();
            userModule.btnSave.Enabled = true;
            userModule.btnUpdate.Enabled = false;
            userModule.txtUserName.Enabled = true; // Ensure username is enabled for new users
            userModule.ShowDialog();
            LoadUser();
        }
        private void dgvUser_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Ensure we're clicking on a valid row and on button columns
            if (e.RowIndex < 0 || e.RowIndex >= dgvUser.Rows.Count) return;

            string colName = dgvUser.Columns[e.ColumnIndex].Name;

            if (colName == "Edit")
            {
                // Get the data from the row
                DataGridViewRow row = dgvUser.Rows[e.RowIndex];

                UserModuleForm userModule = new UserModuleForm();
                userModule.txtUserName.Text = row.Cells["colUserName"].Value.ToString();
                userModule.txtFullName.Text = row.Cells["colFullName"].Value.ToString();
                userModule.txtPass.Text = row.Cells["colPassword"].Value.ToString();
                userModule.txtRepass.Text = row.Cells["colPassword"].Value.ToString();
                userModule.txtPhone.Text = row.Cells["colPhone"].Value?.ToString() ?? "";

                userModule.btnSave.Enabled = false;
                userModule.btnUpdate.Enabled = true;
                userModule.txtUserName.Enabled = false; // Disable username editing
                userModule.ShowDialog();

                LoadUser(); // Refresh the list
            }
            else if (colName == "Delete")
            {
                DataGridViewRow row = dgvUser.Rows[e.RowIndex];
                string username = row.Cells["colUserName"].Value.ToString();
                string fullname = row.Cells["colFullName"].Value.ToString();

                if (MessageBox.Show($"Are you sure you want to delete user '{fullname}' ({username})?",
                    "Delete Record", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        con.Open();
                        cm = new SqlCommand("DELETE FROM tbUser WHERE username=@username", con);
                        cm.Parameters.AddWithValue("@username", username);
                        cm.ExecuteNonQuery();
                        con.Close();
                        MessageBox.Show("User has been successfully deleted!");

                        LoadUser(); // Refresh the list
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error deleting user: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        con.Close();
                    }
                }
            }
        }
        private void dgvUser_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            // Suppress data error messages
            e.ThrowException = false;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
    }
}