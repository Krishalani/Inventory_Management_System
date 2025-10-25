using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace InventoryManagementSystem
{
    public partial class LoginForm : Form
    {
        // Connection string to the StockSpot database
        SqlConnection con = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=StockSpot;Integrated Security=True;Connect Timeout=30");
        SqlCommand cm = new SqlCommand();
        SqlDataReader dr;

        public LoginForm()
        {
            InitializeComponent();
            CreateDatabaseIfNotExists(); // Automatically create DB and table
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
                    MessageBox.Show("Welcome " + dr["fullname"].ToString(), "ACCESS GRANTED", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        // -------------------- Auto Database Creation --------------------
        private void CreateDatabaseIfNotExists()
        {
            try
            {
                // Step 1: Create database if not exists
                using (SqlConnection masterCon = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;Integrated Security=True"))
                {
                    masterCon.Open();
                    string createDbQuery = "IF DB_ID('StockSpot') IS NULL CREATE DATABASE StockSpot;";
                    using (SqlCommand cmd = new SqlCommand(createDbQuery, masterCon))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                // Step 2: Create tbUser table and default admin
                using (SqlConnection dbCon = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=StockSpot;Integrated Security=True;Connect Timeout=30"))
                {
                    dbCon.Open();

                    // Create tbUser table if it doesn't exist
                    string createTableQuery = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='tbUser' AND xtype='U')
                CREATE TABLE tbUser (
                    id INT IDENTITY(1,1) PRIMARY KEY,
                    fullname NVARCHAR(100),
                    username NVARCHAR(50) UNIQUE,
                    password NVARCHAR(50)
                );";
                    using (SqlCommand cmd2 = new SqlCommand(createTableQuery, dbCon))
                    {
                        cmd2.ExecuteNonQuery();
                    }

                    // Insert default admin if not exists
                    string insertAdminQuery = @"
                IF NOT EXISTS (SELECT * FROM tbUser WHERE username='admin')
                INSERT INTO tbUser (fullname, username, password)
                VALUES ('Administrator', 'admin', '1234');";
                    using (SqlCommand cmd3 = new SqlCommand(insertAdminQuery, dbCon))
                    {
                        cmd3.ExecuteNonQuery();
                    }

                    dbCon.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database setup failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}