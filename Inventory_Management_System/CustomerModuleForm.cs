using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace InventoryManagementSystem
{
    public partial class CustomerModuleForm : Form
    {
        // ✅ Use the database name directly instead of attaching the MDF file
        SqlConnection con = new SqlConnection(
            @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=StockSpot;Integrated Security=True;Connect Timeout=30");

        public CustomerModuleForm()
        {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("Are you sure you want to save this customer?",
                    "Saving Record", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    using (SqlCommand cm = new SqlCommand(
                        "INSERT INTO tbCustomer(cname, cphone) VALUES(@cname, @cphone)", con))
                    {
                        cm.Parameters.AddWithValue("@cname", txtCName.Text);
                        cm.Parameters.AddWithValue("@cphone", txtCPhone.Text);

                        con.Open();
                        cm.ExecuteNonQuery();
                        con.Close();
                    }

                    MessageBox.Show("Customer has been successfully saved.");
                    Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                if (con.State == ConnectionState.Open) con.Close();
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("Are you sure you want to update this customer?",
                    "Update Record", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    using (SqlCommand cm = new SqlCommand(
                        "UPDATE tbCustomer SET cname=@cname, cphone=@cphone WHERE cid=@cid", con))
                    {
                        cm.Parameters.AddWithValue("@cname", txtCName.Text);
                        cm.Parameters.AddWithValue("@cphone", txtCPhone.Text);
                        cm.Parameters.AddWithValue("@cid", lblCId.Text);

                        con.Open();
                        cm.ExecuteNonQuery();
                        con.Close();
                    }

                    MessageBox.Show("Customer has been successfully updated!");
                    this.Dispose();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                if (con.State == ConnectionState.Open) con.Close();
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            Clear();
            btnSave.Enabled = true;
            btnUpdate.Enabled = false;
        }

        public void Clear()
        {
            txtCName.Clear();
            txtCPhone.Clear();
        }

        private void pictureBoxClose_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
    }
}
