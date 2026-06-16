using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using Zlagoda.Classes;

namespace Zlagoda.Forms
{
    public partial class EditStoreProductForm : Form
    {
        private readonly string _upc;
        private ComboBox cbProduct;
        private NumericUpDown numPrice, numQuantity;
        private CheckBox chkIsPromotional;
        private Button btnSave;

        public EditStoreProductForm(string upc)
        {
            _upc = upc;
            InitializeCustomComponents();
            LoadProducts();
            LoadCurrentData();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Редагувати товар у магазині";
            this.Size = new Size(400, 460);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            int x = 30, width = 320;

            AddLabel("UPC (не змінюється):", x, 20);
            var txtUPCReadonly = new TextBox()
            {
                Location = new Point(x, 45),
                Width = width,
                Text = _upc,
                ReadOnly = true,
                BackColor = Color.WhiteSmoke
            };

            AddLabel("Оберіть товар:", x, 85);
            cbProduct = new ComboBox()
            {
                Location = new Point(x, 110),
                Width = width,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            AddLabel("Ціна продажу (з ПДВ):", x, 150);
            numPrice = new NumericUpDown()
            {
                Location = new Point(x, 175),
                Width = width,
                DecimalPlaces = 2,
                Maximum = 1000000
            };

            AddLabel("Кількість одиниць:", x, 215);
            numQuantity = new NumericUpDown()
            {
                Location = new Point(x, 240),
                Width = width,
                Maximum = 100000
            };

            chkIsPromotional = new CheckBox()
            {
                Text = "Це акційний товар (знижка 20%)",
                Location = new Point(x, 285),
                Width = width
            };
            chkIsPromotional.CheckedChanged += ChkIsPromotional_CheckedChanged;

            btnSave = new Button()
            {
                Text = "ЗБЕРЕГТИ",
                Location = new Point(x, 350),
                Size = new Size(width, 45),
                BackColor = Color.ForestGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.Click += BtnSave_Click;

            this.Controls.AddRange(new Control[] { txtUPCReadonly, cbProduct, numPrice, numQuantity, chkIsPromotional, btnSave });
        }

        private void AddLabel(string text, int x, int y)
        {
            this.Controls.Add(new Label() { Text = text, Location = new Point(x, y), Width = 300 });
        }

        private void LoadProducts()
        {
            DataTable dt = DbHelper.ExecuteQuery("SELECT id_product, product_name FROM Product ORDER BY product_name");
            if (dt != null)
            {
                cbProduct.DataSource = dt;
                cbProduct.DisplayMember = "product_name";
                cbProduct.ValueMember = "id_product";
            }
        }

        private void LoadCurrentData()
        {
            string query = "SELECT id_product, selling_price, products_number, promotional_product FROM Store_Product WHERE UPC = @upc";
            DataTable dt = DbHelper.ExecuteQuery(query, new SqlParameter[] { new SqlParameter("@upc", _upc) });

            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];

                chkIsPromotional.CheckedChanged -= ChkIsPromotional_CheckedChanged;

                cbProduct.SelectedValue = (int)row["id_product"];
                numPrice.Value = Convert.ToDecimal(row["selling_price"]);
                numQuantity.Value = Convert.ToDecimal(row["products_number"]);
                chkIsPromotional.Checked = Convert.ToBoolean(row["promotional_product"]);

                chkIsPromotional.CheckedChanged += ChkIsPromotional_CheckedChanged;
            }
        }

        private void ChkIsPromotional_CheckedChanged(object sender, EventArgs e)
        {
            if (chkIsPromotional.Checked)
                numPrice.Value = numPrice.Value * 0.8m;
            else
                numPrice.Value = numPrice.Value / 0.8m;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            string query = @"UPDATE Store_Product 
                             SET id_product = @id, selling_price = @price, 
                                 products_number = @num, promotional_product = @prom 
                             WHERE UPC = @upc";

            SqlParameter[] parameters = {
                new SqlParameter("@id",   cbProduct.SelectedValue),
                new SqlParameter("@price", numPrice.Value),
                new SqlParameter("@num",   (int)numQuantity.Value),
                new SqlParameter("@prom",  chkIsPromotional.Checked),
                new SqlParameter("@upc",   _upc)
            };

            if (DbHelper.ExecuteNonQuery(query, parameters))
            {
                MessageBox.Show("Товар у магазині успішно оновлено!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}