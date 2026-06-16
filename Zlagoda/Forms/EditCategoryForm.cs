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
    public partial class EditCategoryForm : Form
    {
        private int _categoryId;
        private TextBox txtCategoryName;
        private Button btnSave;

        public EditCategoryForm(int id, string currentName)
        {
            _categoryId = id;
            InitializeCustomComponents();
            txtCategoryName.Text = currentName;
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Редагувати категорію";
            this.Size = new Size(350, 220);
            this.StartPosition = FormStartPosition.CenterParent;

            Label lblName = new Label() { Text = "Назва категорії:", Location = new Point(30, 30), Width = 270 };
            txtCategoryName = new TextBox() { Location = new Point(30, 60), Width = 270, Font = new Font("Segoe UI", 12) };

            btnSave = new Button()
            {
                Text = "ОНОВИТИ",
                Location = new Point(30, 110),
                Size = new Size(270, 45),
                BackColor = Color.Orange,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnSave.Click += BtnSave_Click;

            this.Controls.AddRange(new Control[] { lblName, txtCategoryName, btnSave });
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            string query = "UPDATE Category SET category_name = @name WHERE category_number = @id";
            SqlParameter[] parameters = {
            new SqlParameter("@name", txtCategoryName.Text.Trim()),
            new SqlParameter("@id", _categoryId)
        };

            if (DbHelper.ExecuteNonQuery(query, parameters))
            {
                MessageBox.Show("Дані оновлено!");
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
