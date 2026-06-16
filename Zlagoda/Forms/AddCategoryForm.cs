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
    public partial class AddCategoryForm : Form
    {
        private TextBox txtCategoryName;
        private Button btnSave;

        public AddCategoryForm()
        {
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Додати нову категорію";
            this.Size = new Size(350, 220);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            Label lblName = new Label()
            {
                Text = "Назва категорії:",
                Location = new Point(30, 30),
                Width = 270,
                Font = new Font("Segoe UI", 10)
            };

            txtCategoryName = new TextBox()
            {
                Location = new Point(30, 60),
                Width = 270,
                Font = new Font("Segoe UI", 12)
            };

            btnSave = new Button()
            {
                Text = "ЗБЕРЕГТИ",
                Location = new Point(30, 110),
                Size = new Size(270, 45),
                BackColor = Color.ForestGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnSave.Cursor = Cursors.Hand;
            btnSave.Click += BtnSave_Click;


            this.Controls.Add(lblName);
            this.Controls.Add(txtCategoryName);
            this.Controls.Add(btnSave);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            string catName = txtCategoryName.Text.Trim();

            if (string.IsNullOrWhiteSpace(catName))
            {
                MessageBox.Show("Введіть назву категорії!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = "INSERT INTO Category (category_name) VALUES (@name)";

            SqlParameter[] parameters = {
                new SqlParameter("@name", System.Data.SqlDbType.NVarChar) { Value = catName }
            };

            if (DbHelper.ExecuteNonQuery(query, parameters))
            {
                MessageBox.Show("Категорію успішно додано!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
