using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace InventoryManagementSystem
{
    public partial class CustomerForm : Form
    {
        SqlConnection con = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=StockSpot;Integrated Security=True;Connect Timeout=30");
        SqlCommand cm = new SqlCommand();
        SqlDataReader dr;

        public CustomerForm()
        {
            InitializeComponent();
            EnsureCustomerTable();   // <-- create table if it doesn't exist
            LoadCustomer();
        }

        // Ensure the tbCustomer table exists; if not, create it
        private void EnsureCustomerTable()
        {
            string sql = @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tbCustomer]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[tbCustomer](
        cid INT IDENTITY(1,1) PRIMARY KEY,
        cname NVARCHAR(100) NOT NULL,
        cphone NVARCHAR(50) NULL
    );
END
";
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error ensuring tbCustomer table: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (con.State == ConnectionState.Open) con.Close();
            }
        }

        public void LoadCustomer()
        {
            int i = 0;
            dgvCustomer.Rows.Clear();
            try
            {
                cm = new SqlCommand("SELECT cid, cname, cphone FROM tbCustomer", con);
                con.Open();
                dr = cm.ExecuteReader();
                while (dr.Read())
                {
                    i++;
                    dgvCustomer.Rows.Add(i, dr["cid"].ToString(), dr["cname"].ToString(), dr["cphone"].ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading customers: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (dr != null) dr.Close();
                if (con.State == ConnectionState.Open) con.Close();
            }
        }

        private void btnAdd_Click_1(object sender, EventArgs e)
        {
            CustomerModuleForm moduleForm = new CustomerModuleForm();
            moduleForm.btnSave.Enabled = true;
            moduleForm.btnUpdate.Enabled = false;
            moduleForm.ShowDialog();
            LoadCustomer();
        }

        private void dgvCustomer_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return; // header click protection

            string colName = dgvCustomer.Columns[e.ColumnIndex].Name;
            if (colName == "Edit")
            {
                CustomerModuleForm customerModule = new CustomerModuleForm();
                customerModule.lblCId.Text = dgvCustomer.Rows[e.RowIndex].Cells[1].Value.ToString();
                customerModule.txtCName.Text = dgvCustomer.Rows[e.RowIndex].Cells[2].Value.ToString();
                customerModule.txtCPhone.Text = dgvCustomer.Rows[e.RowIndex].Cells[3].Value.ToString();

                customerModule.btnSave.Enabled = false;
                customerModule.btnUpdate.Enabled = true;
                customerModule.ShowDialog();
            }
            else if (colName == "Delete")
            {
                if (MessageBox.Show("Are you sure you want to delete this customer?", "Delete Record", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    // Use parameterized query to avoid SQL injection
                    string id = dgvCustomer.Rows[e.RowIndex].Cells[1].Value.ToString();
                    try
                    {
                        using (SqlCommand del = new SqlCommand("DELETE FROM tbCustomer WHERE cid = @cid", con))
                        {
                            del.Parameters.AddWithValue("@cid", Convert.ToInt32(id));
                            con.Open();
                            int affected = del.ExecuteNonQuery();
                            MessageBox.Show(affected > 0 ? "Record has been successfully deleted!" : "No record deleted.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error deleting record: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        if (con.State == ConnectionState.Open) con.Close();
                    }
                }
            }
            LoadCustomer();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }


    }
}
