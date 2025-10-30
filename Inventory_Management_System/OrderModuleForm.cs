using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace InventoryManagementSystem
{
    public partial class OrderModuleForm : Form
    {
        SqlConnection con = new SqlConnection(
            @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=StockSpot;Integrated Security=True;Connect Timeout=30");
        SqlCommand cm = new SqlCommand();
        SqlDataReader dr;
        int availableQty = 0;

        public OrderModuleForm()
        {
            InitializeComponent();
            CreateTablesIfNotExists();
            LoadCustomer();
            LoadProduct();

            // Ensure UDQty hooks into the correct event
            this.UDQty.ValueChanged += new EventHandler(this.UDQty_ValueChanged);
        }

        // ------------------ AUTO CREATE TABLES ------------------
        private void CreateTablesIfNotExists()
        {
            try
            {
                con.Open();

                string createCustomer = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='tbCustomer' AND xtype='U')
                CREATE TABLE tbCustomer(
                    cid INT IDENTITY(1,1) PRIMARY KEY,
                    cname NVARCHAR(100),
                    cphone NVARCHAR(20)
                )";
                new SqlCommand(createCustomer, con).ExecuteNonQuery();

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
                if (con.State == ConnectionState.Open) con.Close();
            }
        }

        // ------------------ LOAD CUSTOMER ------------------
        public void LoadCustomer()
        {
            try
            {
                dgvCustomer.Rows.Clear();
                cm = new SqlCommand("SELECT cid, cname FROM tbCustomer WHERE CONCAT(cid, cname) LIKE @search", con);
                cm.Parameters.AddWithValue("@search", "%" + txtSearchCust.Text + "%");
                con.Open();
                dr = cm.ExecuteReader();
                int i = 0;
                while (dr.Read())
                {
                    i++;
                    dgvCustomer.Rows.Add(i, dr["cid"].ToString(), dr["cname"].ToString());
                }
                dr.Close();
                con.Close();
            }
            catch
            {
                if (con.State == ConnectionState.Open) con.Close();
            }
        }

        // ------------------ LOAD PRODUCT ------------------
        public void LoadProduct()
        {
            try
            {
                dgvProduct.Rows.Clear();
                cm = new SqlCommand("SELECT * FROM tbProduct WHERE CONCAT(pid, pname, pprice, pdescription, pcategory) LIKE @search", con);
                cm.Parameters.AddWithValue("@search", "%" + txtSearchProd.Text + "%");
                con.Open();
                dr = cm.ExecuteReader();
                int i = 0;
                while (dr.Read())
                {
                    i++;
                    dgvProduct.Rows.Add(i,
                        dr["pid"].ToString(),
                        dr["pname"].ToString(),
                        dr["pqty"].ToString(),
                        dr["pprice"].ToString(),
                        dr["pdescription"].ToString(),
                        dr["pcategory"].ToString());
                }
                dr.Close();
                con.Close();
            }
            catch
            {
                if (con.State == ConnectionState.Open) con.Close();
            }
        }

        private void txtSearchCust_TextChanged(object sender, EventArgs e) => LoadCustomer();
        private void txtSearchProd_TextChanged(object sender, EventArgs e) => LoadProduct();

        // ------------------ GET AVAILABLE QUANTITY ------------------
        public void GetAvailableQty()
        {
            if (string.IsNullOrEmpty(txtPid.Text)) { availableQty = 0; return; }
            cm = new SqlCommand("SELECT pqty FROM tbProduct WHERE pid=@pid", con);
            cm.Parameters.AddWithValue("@pid", txtPid.Text);
            con.Open();
            dr = cm.ExecuteReader();
            if (dr.Read()) availableQty = Convert.ToInt32(dr["pqty"]);
            dr.Close();
            con.Close();
        }

        // ------------------ CALCULATE TOTAL ------------------
        private void UDQty_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                GetAvailableQty();

                if (UDQty.Value > availableQty)
                {
                    MessageBox.Show("Instock quantity is not enough!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    UDQty.Value = availableQty;
                    return;
                }

                if (double.TryParse(txtPrice.Text, out double price))
                {
                    double total = price * Convert.ToDouble(UDQty.Value);
                    txtTotal.Text = total.ToString("0.00");
                }
                else
                {
                    txtTotal.Text = "0.00";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error calculating total: " + ex.Message);
            }
        }

        // ------------------ SELECT CUSTOMER ------------------
        private void dgvCustomer_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                txtCId.Text = dgvCustomer.Rows[e.RowIndex].Cells[1].Value.ToString();
                txtCName.Text = dgvCustomer.Rows[e.RowIndex].Cells[2].Value.ToString();
            }
        }

        // ------------------ SELECT PRODUCT ------------------
        private void dgvProduct_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                txtPid.Text = dgvProduct.Rows[e.RowIndex].Cells[1].Value.ToString();
                txtPName.Text = dgvProduct.Rows[e.RowIndex].Cells[2].Value.ToString();
                txtPrice.Text = dgvProduct.Rows[e.RowIndex].Cells[4].Value.ToString();
                GetAvailableQty();

                // auto calculate total
                UDQty_ValueChanged(null, null);
            }
        }

        // ------------------ INSERT ORDER ------------------
        private void btnInsert_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtCId.Text) || string.IsNullOrEmpty(txtPid.Text) || UDQty.Value <= 0)
            {
                MessageBox.Show("Please select customer, product, and quantity!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!double.TryParse(txtPrice.Text, out double price) || !double.TryParse(txtTotal.Text, out double total))
            {
                MessageBox.Show("Price or Total is invalid!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show("Are you sure you want to insert this order?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                SqlTransaction transaction = null;
                try
                {
                    con.Open();
                    transaction = con.BeginTransaction();

                    cm = new SqlCommand("INSERT INTO tbOrder(odate, pid, cid, qty, price, total) VALUES(@odate, @pid, @cid, @qty, @price, @total)", con, transaction);
                    cm.Parameters.AddWithValue("@odate", dtOrder.Value);
                    cm.Parameters.AddWithValue("@pid", Convert.ToInt32(txtPid.Text));
                    cm.Parameters.AddWithValue("@cid", Convert.ToInt32(txtCId.Text));
                    cm.Parameters.AddWithValue("@qty", Convert.ToInt32(UDQty.Value));
                    cm.Parameters.AddWithValue("@price", price);
                    cm.Parameters.AddWithValue("@total", total);
                    cm.ExecuteNonQuery();

                    cm = new SqlCommand("UPDATE tbProduct SET pqty = pqty - @qty WHERE pid=@pid", con, transaction);
                    cm.Parameters.AddWithValue("@qty", Convert.ToInt32(UDQty.Value));
                    cm.Parameters.AddWithValue("@pid", Convert.ToInt32(txtPid.Text));
                    cm.ExecuteNonQuery();

                    transaction.Commit();
                    MessageBox.Show("Order has been successfully inserted!");

                    Clear();
                    LoadProduct();
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    MessageBox.Show("Order insertion failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    con.Close();
                }
            }
        }

        // ------------------ CLEAR ------------------
        public void Clear()
        {
            txtCId.Clear();
            txtCName.Clear();
            txtPid.Clear();
            txtPName.Clear();
            txtPrice.Clear();
            UDQty.Value = 0;
            txtTotal.Clear();
            dtOrder.Value = DateTime.Now;
        }

        private void btnClear_Click(object sender, EventArgs e) => Clear();
        private void pictureBoxClose_Click(object sender, EventArgs e) => this.Dispose();
    }
}
