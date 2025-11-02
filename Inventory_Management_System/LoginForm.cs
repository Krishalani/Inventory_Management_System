using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace InventoryManagementSystem
{
    public partial class LoginForm : Form
    {
        SqlConnection con = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=StockSpot;Integrated Security=True;Connect Timeout=30");
        SqlCommand cm = new SqlCommand();
        SqlDataReader dr;

        // Public properties to store logged-in user info
        public static string CurrentUserRole { get; private set; }
        public static string CurrentUsername { get; private set; }
        public static string CurrentFullName { get; private set; }

        public LoginForm()
        {
            InitializeComponent();
            CreateDatabaseIfNotExists();
        }

        private void checkBoxPass_CheckedChanged(object sender, EventArgs e)
        {
            txtPass.UseSystemPasswordChar = !checkBoxPass.Checked;
        }

        private void lblClear_Click(object sender, EventArgs e)
        {
            txtName.Clear();
            txtPass.Clear();
        }

        private void pictureBoxClose_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Exit Application?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                cm = new SqlCommand("SELECT * FROM tbUser WHERE username=@username AND password=@password", con);
                cm.Parameters.AddWithValue("@username", txtName.Text);
                cm.Parameters.AddWithValue("@password", txtPass.Text);
                con.Open();
                dr = cm.ExecuteReader();
                if (dr.Read())
                {
                    // Store user information
                    CurrentUserRole = dr["role"]?.ToString() ?? "User";
                    CurrentUsername = dr["username"].ToString();
                    CurrentFullName = dr["fullname"].ToString();

                    string welcomeMessage = $"Welcome {CurrentFullName}";
                    if (CurrentUserRole == "SuperAdmin")
                    {
                        welcomeMessage += " (Super Administrator)";
                    }

                    MessageBox.Show(welcomeMessage, "ACCESS GRANTED", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    MainForm main = new MainForm();
                    this.Hide();
                    main.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Invalid username or password!", "ACCESS DENIED", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                con.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                con.Close();
            }
        }

        private void CreateDatabaseIfNotExists()
        {
            try
            {
                // Create database if not exists
                using (SqlConnection masterCon = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;Integrated Security=True"))
                {
                    masterCon.Open();
                    string createDbQuery = "IF DB_ID('StockSpot') IS NULL CREATE DATABASE StockSpot;";
                    using (SqlCommand cmd = new SqlCommand(createDbQuery, masterCon))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                // Create and update tbUser table
                using (SqlConnection dbCon = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=StockSpot;Integrated Security=True;Connect Timeout=30"))
                {
                    dbCon.Open();

                    // Create tbUser table if it doesn't exist (original structure)
                    string createTableQuery = @"
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='tbUser' AND xtype='U')
                    CREATE TABLE tbUser (
                        id INT IDENTITY(1,1) PRIMARY KEY,
                        fullname NVARCHAR(100) NOT NULL,
                        username NVARCHAR(50) UNIQUE NOT NULL,
                        password NVARCHAR(50) NOT NULL,
                        phone NVARCHAR(20)
                    );";
                    using (SqlCommand cmd2 = new SqlCommand(createTableQuery, dbCon))
                    {
                        cmd2.ExecuteNonQuery();
                    }

                    // ADD ROLE COLUMN if it doesn't exist
                    string addRoleColumnQuery = @"
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbUser') AND name = 'role')
                    BEGIN
                        ALTER TABLE tbUser ADD role NVARCHAR(20) DEFAULT 'User';
                    END";
                    using (SqlCommand cmd3 = new SqlCommand(addRoleColumnQuery, dbCon))
                    {
                        cmd3.ExecuteNonQuery();
                    }

                    // ADD PHONE COLUMN if it doesn't exist
                    string addPhoneColumnQuery = @"
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbUser') AND name = 'phone')
                    BEGIN
                        ALTER TABLE tbUser ADD phone NVARCHAR(20);
                    END";
                    using (SqlCommand cmd4 = new SqlCommand(addPhoneColumnQuery, dbCon))
                    {
                        cmd4.ExecuteNonQuery();
                    }

                    // Check if superadmin exists, if not insert it
                    string checkSuperAdminQuery = "SELECT COUNT(*) FROM tbUser WHERE username = 'superadmin'";
                    using (SqlCommand checkCmd = new SqlCommand(checkSuperAdminQuery, dbCon))
                    {
                        int superAdminCount = (int)checkCmd.ExecuteScalar();

                        if (superAdminCount == 0)
                        {
                            // Insert YOUR super admin credentials
                            string insertAdminQuery = @"
                            INSERT INTO tbUser (fullname, username, password, phone, role)
                            VALUES ('Super Administrator', 'superadmin', 'superadmin123', '0750336004', 'SuperAdmin')";
                            using (SqlCommand insertCmd = new SqlCommand(insertAdminQuery, dbCon))
                            {
                                insertCmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            // Update existing superadmin to ensure it has correct role
                            string updateAdminQuery = @"
                            UPDATE tbUser 
                            SET role = 'SuperAdmin', 
                                fullname = 'Super Administrator',
                                phone = '0750336004'
                            WHERE username = 'superadmin'";
                            using (SqlCommand updateCmd = new SqlCommand(updateAdminQuery, dbCon))
                            {
                                updateCmd.ExecuteNonQuery();
                            }
                        }
                    }

                    // Update any existing users without role to have 'User' role
                    string updateExistingUsersQuery = @"
                    UPDATE tbUser 
                    SET role = 'User' 
                    WHERE role IS NULL OR role = ''";
                    using (SqlCommand updateCmd = new SqlCommand(updateExistingUsersQuery, dbCon))
                    {
                        updateCmd.ExecuteNonQuery();
                    }

                    dbCon.Close();
                    MessageBox.Show("Welcome to Stock Spot Inventory Management System.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                
            }
        }
    }
}