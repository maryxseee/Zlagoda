using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using Zlagoda.Classes;

namespace Zlagoda.Forms
{
    public partial class EditProductForm : Form
    {
        private readonly int _productId;
        private ComboBox cbCategory;
        private TextBox txtName, txtManufacturer, txtCharacteristics;
        private Button btnSave;

        public EditProductForm(int productId)
        {
            _productId = productId;
            InitializeCustomComponents();
            LoadCategories();
            LoadCurrentData();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Редагувати товар";
            this.Size = new Size(400, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            int x = 30, width = 320;

            Label lblCat = new Label() { Text = "Оберіть категорію:", Location = new Point(x, 20), Width = width };
            cbCategory = new ComboBox()
            {
                Location = new Point(x, 45),
                Width = width,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            Label lblName = new Label() { Text = "Назва товару:", Location = new Point(x, 85), Width = width };
            txtName = new TextBox() { Location = new Point(x, 110), Width = width };

            Label lblMan = new Label() { Text = "Виробник:", Location = new Point(x, 150), Width = width };
            txtManufacturer = new TextBox() { Location = new Point(x, 175), Width = width };

            Label lblChar = new Label() { Text = "Характеристики:", Location = new Point(x, 215), Width = width };
            txtCharacteristics = new TextBox()
            {
                Location = new Point(x, 240),
                Width = width,
                Multiline = true,
                Height = 60
            };

            btnSave = new Button()
            {
                Text = "ЗБЕРЕГТИ",
                Location = new Point(x, 330),
                Size = new Size(width, 45),
                BackColor = Color.ForestGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.Click += BtnSave_Click;

            this.Controls.AddRange(new Control[] { lblCat, cbCategory, lblName, txtName, lblMan, txtManufacturer, lblChar, txtCharacteristics, btnSave });
        }

        private void LoadCategories()
        {
            DataTable dt = DbHelper.ExecuteQuery("SELECT category_number, category_name FROM Category ORDER BY category_name");
            if (dt != null)
            {
                cbCategory.DataSource = dt;
                cbCategory.DisplayMember = "category_name";
                cbCategory.ValueMember = "category_number";
            }
        }

        private void LoadCurrentData()
        {
            string query = "SELECT category_number, product_name, manufacturer, characteristics FROM Product WHERE id_product = @id";
            DataTable dt = DbHelper.ExecuteQuery(query, new SqlParameter[] { new SqlParameter("@id", _productId) });

            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                cbCategory.SelectedValue = (int)row["category_number"];
                txtName.Text = row["product_name"].ToString();
                txtManufacturer.Text = row["manufacturer"].ToString();
                txtCharacteristics.Text = row["characteristics"].ToString();
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cbCategory.SelectedValue == null || string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Будь ласка, заповніть обов'язкові поля!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"UPDATE Product 
                             SET category_number = @catId, product_name = @name, 
                                 manufacturer = @manuf, characteristics = @char 
                             WHERE id_product = @id";

            SqlParameter[] parameters = {
                new SqlParameter("@catId", cbCategory.SelectedValue),
                new SqlParameter("@name",  txtName.Text.Trim()),
                new SqlParameter("@manuf", txtManufacturer.Text.Trim()),
                new SqlParameter("@char",  txtCharacteristics.Text.Trim()),
                new SqlParameter("@id",    _productId)
            };

            if (DbHelper.ExecuteNonQuery(query, parameters))
            {
                MessageBox.Show("Товар успішно оновлено!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}