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
    public partial class OrderForm : Form
    {
        SqlConnection con = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=StockSpot;Integrated Security=True;Connect Timeout=30");
        SqlCommand cm = new SqlCommand();
        SqlDataReader dr;

        public OrderForm()
        {
            InitializeComponent();
            CreateTablesIfNotExists(); // ✅ Auto check & create tables
            LoadOrder();
        }

        // ------------------ AUTO CREATE TABLES ------------------
        private void CreateTablesIfNotExists()
        {
            try
            {
                con.Open();

                // Customer Table
                string createCustomer = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='tbCustomer' AND xtype='U')
                CREATE TABLE tbCustomer(
                    cid INT IDENTITY(1,1) PRIMARY KEY,
                    cname NVARCHAR(100),
                    cphone NVARCHAR(20)
                )";
                new SqlCommand(createCustomer, con).ExecuteNonQuery();

                // Product Table
                string createProduct = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='tbProduct' AND xtype='U')
                CREATE TABLE tbProduct(
                    pid INT IDENTITY(1,1) PRIMARY KEY,
                    pname NVARCHAR(100),
                    pqty INT,
                    pprice FLOAT,
                    pdescription NVARCHAR(255),
                    pcategory NVARCHAR(100)
                )";
                new SqlCommand(createProduct, con).ExecuteNonQuery();

                // Order Table
                string createOrder = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='tbOrder' AND xtype='U')
                CREATE TABLE tbOrder(
                    orderid INT IDENTITY(1,1) PRIMARY KEY,
                    odate DATE,
                    pid INT FOREIGN KEY REFERENCES tbProduct(pid),
                    cid INT FOREIGN KEY REFERENCES tbCustomer(cid),
                    qty INT,
                    price FLOAT,
                    total FLOAT
                )";
                new SqlCommand(createOrder, con).ExecuteNonQuery();

                con.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Auto table creation failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
        }

        // ------------------ LOAD ORDERS ------------------
        public void LoadOrder()
        {
            double total = 0;
            int i = 0;
            dgvOrder.Rows.Clear();
            cm = new SqlCommand(@"SELECT orderid, odate, O.pid, P.pname, O.cid, C.cname, qty, price, total  
                                  FROM tbOrder AS O 
                                  JOIN tbCustomer AS C ON O.cid = C.cid 
                                  JOIN tbProduct AS P ON O.pid = P.pid 
                                  WHERE CONCAT(orderid, odate, O.pid, P.pname, O.cid, C.cname, qty, price) LIKE @search", con);
            cm.Parameters.AddWithValue("@search", "%" + txtSearch.Text + "%");

            con.Open();
            dr = cm.ExecuteReader();
            while (dr.Read())
            {
                i++;
                dgvOrder.Rows.Add(i,
                    dr[0].ToString(),
                    Convert.ToDateTime(dr[1].ToString()).ToString("dd/MM/yyyy"),
                    dr[2].ToString(),
                    dr[3].ToString(),
                    dr[4].ToString(),
                    dr[5].ToString(),
                    dr[6].ToString(),
                    dr[7].ToString(),
                    dr[8].ToString());
                total += Convert.ToDouble(dr[8]);
            }
            dr.Close();
            con.Close();

            lblQty.Text = i.ToString();
            lblTotal.Text = total.ToString("N2");
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            OrderModuleForm moduleForm = new OrderModuleForm();
            moduleForm.ShowDialog();
            LoadOrder();
        }

        private void dgvUser_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            string colName = dgvOrder.Columns[e.ColumnIndex].Name;

            if (colName == "Delete")
            {
                if (MessageBox.Show("Are you sure you want to delete this order?", "Delete Record", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    con.Open();
                    cm = new SqlCommand("DELETE FROM tbOrder WHERE orderid = @id", con);
                    cm.Parameters.AddWithValue("@id", dgvOrder.Rows[e.RowIndex].Cells[1].Value.ToString());
                    cm.ExecuteNonQuery();
                    con.Close();

                    MessageBox.Show("Record has been successfully deleted!", "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Restore stock
                    cm = new SqlCommand("UPDATE tbProduct SET pqty = (pqty + @pqty) WHERE pid = @pid", con);
                    cm.Parameters.AddWithValue("@pqty", Convert.ToInt16(dgvOrder.Rows[e.RowIndex].Cells[7].Value.ToString()));
                    cm.Parameters.AddWithValue("@pid", dgvOrder.Rows[e.RowIndex].Cells[3].Value.ToString());
                    con.Open();
                    cm.ExecuteNonQuery();
                    con.Close();
                }
            }
            LoadOrder();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadOrder();
        }

      

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
    }
}
