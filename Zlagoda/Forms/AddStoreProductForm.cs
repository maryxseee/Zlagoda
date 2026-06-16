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
using Zlagoda.Classes;

namespace Zlagoda.Forms
{
    public partial class AddStoreProductForm : Form
    {
        private TextBox txtUPC;
        private ComboBox cbProduct;
        private NumericUpDown numPrice;
        private NumericUpDown numQuantity;
        private CheckBox chkIsPromotional;
        private Button btnSave;

        public AddStoreProductForm()
        {
            InitializeCustomComponents();
            LoadProducts();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Додати товар у магазин";
            this.Size = new Size(400, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            int x = 30, width = 320;

            AddLabel("UPC (12 цифр):", x, 20);
            txtUPC = new TextBox() { Location = new Point(x, 45), Width = width, MaxLength = 12 };

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
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnSave.Click += BtnSave_Click;

            this.Controls.AddRange(new Control[] { txtUPC, cbProduct, numPrice, numQuantity, chkIsPromotional, btnSave });
        }

        private void AddLabel(string text, int x, int y)
        {
            Label lbl = new Label() { Text = text, Location = new Point(x, y), Width = 300 };
            this.Controls.Add(lbl);
        }

        private void LoadProducts()
        {
            string query = "SELECT id_product, product_name FROM Product ORDER BY product_name";
            DataTable dt = DbHelper.ExecuteQuery(query);
            if (dt != null)
            {
                cbProduct.DataSource = dt;
                cbProduct.DisplayMember = "product_name";
                cbProduct.ValueMember = "id_product";
            }
        }

        private void ChkIsPromotional_CheckedChanged(object sender, EventArgs e)
        {
            if (chkIsPromotional.Checked)
            {
                numPrice.Value = numPrice.Value * 0.8m;
            }
            else
            {
                numPrice.Value = numPrice.Value / 0.8m;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUPC.Text) || txtUPC.Text.Length > 12)
            {
                MessageBox.Show("UPC має містити до 12 символів!");
                return;
            }

            string query = @"INSERT INTO Store_Product (UPC, id_product, selling_price, products_number, promotional_product) 
                            VALUES (@upc, @id, @price, @num, @prom)";

            SqlParameter[] parameters = {
                new SqlParameter("@upc", txtUPC.Text.Trim()),
                new SqlParameter("@id", cbProduct.SelectedValue),
                new SqlParameter("@price", numPrice.Value),
                new SqlParameter("@num", (int)numQuantity.Value),
                new SqlParameter("@prom", chkIsPromotional.Checked)
            };

            if (DbHelper.ExecuteNonQuery(query, parameters))
            {
                MessageBox.Show("Товар успішно додано в магазин!");
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
