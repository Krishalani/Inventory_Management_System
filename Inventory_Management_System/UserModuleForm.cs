using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace InventoryManagementSystem
{
    public partial class UserModuleForm : Form
    {
        SqlConnection con = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=StockSpot;Integrated Security=True;Connect Timeout=30");
        SqlCommand cm = new SqlCommand();

        public UserModuleForm()
        {
            InitializeComponent();
            btnUpdate.Enabled = false;
        }

        private void pictureBoxClose_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // SUPER ADMIN VERIFICATION - Only SuperAdmin can add users
            if (LoginForm.CurrentUserRole != "SuperAdmin")
            {
                MessageBox.Show("Only Super Administrator can create new users!", "Access Denied",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                if (txtPass.Text != txtRepass.Text)
                {
                    MessageBox.Show("Password did not match!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validate required fields
                if (string.IsNullOrEmpty(txtUserName.Text) || string.IsNullOrEmpty(txtFullName.Text) ||
                    string.IsNullOrEmpty(txtPass.Text) || string.IsNullOrEmpty(txtPhone.Text))
                {
                    MessageBox.Show("Please fill in all required fields!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check if username already exists
                if (IsUsernameExists(txtUserName.Text))
                {
                    MessageBox.Show("Username already exists! Please choose a different username.",
                        "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (MessageBox.Show("Are you sure you want to save this user?", "Saving Record",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    cm = new SqlCommand("INSERT INTO tbUser(username, fullname, password, phone, role) VALUES(@username, @fullname, @password, @phone, @role)", con);
                    cm.Parameters.AddWithValue("@username", txtUserName.Text);
                    cm.Parameters.AddWithValue("@fullname", txtFullName.Text);
                    cm.Parameters.AddWithValue("@password", txtPass.Text);
                    cm.Parameters.AddWithValue("@phone", txtPhone.Text);
                    cm.Parameters.AddWithValue("@role", "User"); // All new users get 'User' role

                    con.Open();
                    cm.ExecuteNonQuery();
                    con.Close();

                    MessageBox.Show("User has been successfully saved.");
                    Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving user: " + ex.Message);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            // Only SuperAdmin can update users
            if (LoginForm.CurrentUserRole != "SuperAdmin")
            {
                MessageBox.Show("Only Super Administrator can update users!", "Access Denied",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                if (txtPass.Text != txtRepass.Text)
                {
                    MessageBox.Show("Password did not match!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (MessageBox.Show("Are you sure you want to update this user?", "Update Record",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    cm = new SqlCommand("UPDATE tbUser SET fullname=@fullname, password=@password, phone=@phone WHERE username=@username", con);
                    cm.Parameters.AddWithValue("@fullname", txtFullName.Text);
                    cm.Parameters.AddWithValue("@password", txtPass.Text);
                    cm.Parameters.AddWithValue("@phone", txtPhone.Text);
                    cm.Parameters.AddWithValue("@username", txtUserName.Text);

                    con.Open();
                    cm.ExecuteNonQuery();
                    con.Close();

                    MessageBox.Show("User has been successfully updated!");
                    this.Dispose();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating user: " + ex.Message);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            Clear();
            btnSave.Enabled = true;
            btnUpdate.Enabled = false;
            txtUserName.Enabled = true;
            txtUserName.Focus();
        }

        public void Clear()
        {
            txtUserName.Clear();
            txtFullName.Clear();
            txtPass.Clear();
            txtRepass.Clear();
            txtPhone.Clear();
        }

        // Method to load user data for editing
        public void LoadUserData(string username, string fullname, string password, string phone)
        {
            txtUserName.Text = username;
            txtFullName.Text = fullname;
            txtPass.Text = password;
            txtRepass.Text = password;
            txtPhone.Text = phone;

            // Disable username field during update
            txtUserName.Enabled = false;
            btnSave.Enabled = false;
            btnUpdate.Enabled = true;
        }

        // Check if username already exists
        private bool IsUsernameExists(string username)
        {
            try
            {
                using (SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM tbUser WHERE username = @username", con))
                {
                    checkCmd.Parameters.AddWithValue("@username", username);
                    if (con.State == ConnectionState.Closed) con.Open();
                    int count = (int)checkCmd.ExecuteScalar();
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error checking username: " + ex.Message);
                return true;
            }
            finally
            {
                if (con.State == ConnectionState.Open) con.Close();
            }
        }
    }
}