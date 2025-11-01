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

namespace InventoryManagementSystem
{
    public partial class ProductForm : Form
    {
        SqlConnection con = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=StockSpot;Integrated Security=True;Connect Timeout=30");
        SqlCommand cm = new SqlCommand();
        SqlDataReader dr;

        public ProductForm()
        {
            InitializeComponent();
            CreateProductTableIfNotExists(); // ✅ Auto-create table if not exists
            LoadProduct();
        }

        // ------------------ AUTO TABLE CREATION ------------------
        private void CreateProductTableIfNotExists()
        {
            try
            {
                using (SqlConnection dbCon = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=StockSpot;Integrated Security=True;Connect Timeout=30"))
                {
                    dbCon.Open();

                    // Step 1: Create tbProduct table if it doesn’t exist
                    string createTableQuery = @"
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='tbProduct' AND xtype='U')
                    CREATE TABLE tbProduct (
                        pid INT IDENTITY(1,1) PRIMARY KEY,
                        pname NVARCHAR(100),
                        pqty INT,
                        pprice DECIMAL(10,2),
                        pdescription NVARCHAR(200),
                        pcategory NVARCHAR(50)
                    );";
                    using (SqlCommand cmd = new SqlCommand(createTableQuery, dbCon))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Step 2: Optional — insert sample record
                    string insertSample = @"
                    IF NOT EXISTS (SELECT * FROM tbProduct WHERE pname='Sample Product')
                    INSERT INTO tbProduct (pname, pqty, pprice, pdescription, pcategory)
                    VALUES ('Sample Product', 10, 99.99, 'Default Item', 'General');";
                    using (SqlCommand cmd2 = new SqlCommand(insertSample, dbCon))
                    {
                        cmd2.ExecuteNonQuery();
                    }

                    dbCon.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Product table setup failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ------------------ LOAD PRODUCT ------------------
        public void LoadProduct()
        {
            int i = 0;
            dgvProduct.Rows.Clear();
            cm = new SqlCommand("SELECT * FROM tbProduct WHERE CONCAT(pid, pname, pprice, pdescription, pcategory) LIKE @search", con);
            cm.Parameters.AddWithValue("@search", "%" + txtSearch.Text + "%");
            con.Open();
            dr = cm.ExecuteReader();
            while (dr.Read())
            {
                i++;
                dgvProduct.Rows.Add(i, dr["pid"].ToString(), dr["pname"].ToString(), dr["pqty"].ToString(),
                    dr["pprice"].ToString(), dr["pdescription"].ToString(), dr["pcategory"].ToString());
            }
            dr.Close();
            con.Close();
        }

        // ------------------ ADD BUTTON ------------------
        private void btnAdd_Click(object sender, EventArgs e)
        {
            ProductModuleForm formModule = new ProductModuleForm();
            formModule.btnSave.Enabled = true;
            formModule.btnUpdate.Enabled = false;
            formModule.ShowDialog();
            LoadProduct();
        }

        // ------------------ DATAGRID ACTIONS ------------------
        private void dgvProduct_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            string colName = dgvProduct.Columns[e.ColumnIndex].Name;

            if (colName == "Edit")
            {
                ProductModuleForm productModule = new ProductModuleForm();
                productModule.lblPid.Text = dgvProduct.Rows[e.RowIndex].Cells[1].Value.ToString();
                productModule.txtPName.Text = dgvProduct.Rows[e.RowIndex].Cells[2].Value.ToString();
                productModule.txtPQty.Text = dgvProduct.Rows[e.RowIndex].Cells[3].Value.ToString();
                productModule.txtPPrice.Text = dgvProduct.Rows[e.RowIndex].Cells[4].Value.ToString();
                productModule.txtPDes.Text = dgvProduct.Rows[e.RowIndex].Cells[5].Value.ToString();
                productModule.comboCat.Text = dgvProduct.Rows[e.RowIndex].Cells[6].Value.ToString();

                productModule.btnSave.Enabled = false;
                productModule.btnUpdate.Enabled = true;
                productModule.ShowDialog();
            }
            else if (colName == "Delete")
            {
                if (MessageBox.Show("Are you sure you want to delete this product?", "Delete Record",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    con.Open();
                    cm = new SqlCommand("DELETE FROM tbProduct WHERE pid=@pid", con);
                    cm.Parameters.AddWithValue("@pid", dgvProduct.Rows[e.RowIndex].Cells[1].Value.ToString());
                    cm.ExecuteNonQuery();
                    con.Close();
                    MessageBox.Show("Record has been successfully deleted!");
                }
            }
            LoadProduct();
        }

        // ------------------ SEARCH BOX ------------------
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadProduct();
        }

    

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

    
    }
}
