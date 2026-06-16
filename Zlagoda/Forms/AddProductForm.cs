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
    public partial class AddProductForm : Form
    {
        private ComboBox cbCategory;
        private TextBox txtName;
        private TextBox txtManufacturer;
        private TextBox txtCharacteristics;
        private Button btnSave;

        public AddProductForm()
        {
            InitializeCustomComponents();
            LoadCategories();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Додати новий товар";
            this.Size = new Size(400, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            int labelX = 30, inputX = 30, width = 320;


            Label lblCat = new Label() { Text = "Оберіть категорію:", Location = new Point(labelX, 20), Width = width };
            cbCategory = new ComboBox()
            {
                Location = new Point(inputX, 45),
                Width = width,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            Label lblName = new Label() { Text = "Назва товару:", Location = new Point(labelX, 85), Width = width };
            txtName = new TextBox() { Location = new Point(inputX, 110), Width = width };

            Label lblMan = new Label() { Text = "Виробник:", Location = new Point(labelX, 150), Width = width };
            txtManufacturer = new TextBox() { Location = new Point(inputX, 175), Width = width };

            Label lblChar = new Label() { Text = "Характеристики:", Location = new Point(labelX, 215), Width = width };
            txtCharacteristics = new TextBox()
            {
                Location = new Point(inputX, 240),
                Width = width,
                Multiline = true,
                Height = 60
            };

            btnSave = new Button()
            {
                Text = "ЗБЕРЕГТИ",
                Location = new Point(inputX, 330),
                Size = new Size(width, 45),
                BackColor = Color.ForestGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnSave.Click += BtnSave_Click;

            this.Controls.AddRange(new Control[] { lblCat, cbCategory, lblName, txtName, lblMan, txtManufacturer, lblChar, txtCharacteristics, btnSave });
        }

        private void LoadCategories()
        {
            string query = "SELECT category_number, category_name FROM Category ORDER BY category_name";
            DataTable dt = DbHelper.ExecuteQuery(query);

            if (dt != null)
            {
                cbCategory.DataSource = dt;
                cbCategory.DisplayMember = "category_name";
                cbCategory.ValueMember = "category_number";
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // Валідація
            if (cbCategory.SelectedValue == null || string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Будь ласка, заповніть обов'язкові поля!");
                return;
            }

            string query = @"INSERT INTO Product (category_number, product_name, manufacturer, characteristics) 
                            VALUES (@catId, @name, @manuf, @char)";

            SqlParameter[] parameters = {
                new SqlParameter("@catId", cbCategory.SelectedValue),
                new SqlParameter("@name", txtName.Text.Trim()),
                new SqlParameter("@manuf", txtManufacturer.Text.Trim()),
                new SqlParameter("@char", txtCharacteristics.Text.Trim())
            };

            if (DbHelper.ExecuteNonQuery(query, parameters))
            {
                MessageBox.Show("Товар успішно додано!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
