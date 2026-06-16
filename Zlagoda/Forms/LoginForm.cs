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
    public partial class LoginForm : Form
    {
        private TextBox txtLogin;
        private TextBox txtPassword;
        private Button btnLogin;
        private Label lblTitle;
        private Label lblLogin;
        private Label lblPassword;

        public LoginForm()
        {
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Авторизація - ZLAGODA";
            this.Size = new Size(350, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.WhiteSmoke;

            lblTitle = new Label()
            {
                Text = "Вхід | ZLAGODA",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, 30),
                Size = new Size(300, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };

            lblLogin = new Label() { Text = "ID працівника (Логін):", Location = new Point(50, 100), Size = new Size(250, 20) };
            txtLogin = new TextBox() { Location = new Point(50, 125), Size = new Size(250, 30), Font = new Font("Segoe UI", 12) };

            lblPassword = new Label() { Text = "Пароль:", Location = new Point(50, 170), Size = new Size(250, 20) };
            txtPassword = new TextBox()
            {
                Location = new Point(50, 195),
                Size = new Size(250, 30),
                Font = new Font("Segoe UI", 12),
                UseSystemPasswordChar = true
            };

            btnLogin = new Button()
            {
                Text = "УВІЙТИ",
                Location = new Point(50, 260),
                Size = new Size(250, 45),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnLogin.Cursor = Cursors.Hand;
            btnLogin.Click += BtnLogin_Click;

            this.Controls.Add(lblTitle);
            this.Controls.Add(lblLogin);
            this.Controls.Add(txtLogin);
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);
            this.Controls.Add(btnLogin);
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string id = txtLogin.Text.Trim();
            string rawPassword = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(rawPassword))
            {
                MessageBox.Show("Будь ласка, заповніть усі поля!", "Попередження", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string hashedPassword = PasswordHasher.HashPassword(rawPassword);

            string query = "SELECT role, empl_surname FROM Employee WHERE id_employee = @id AND password_hash = @pass";

            SqlParameter[] parameters = {
                new SqlParameter("@id", SqlDbType.NVarChar) { Value = id },
                new SqlParameter("@pass", SqlDbType.NVarChar) { Value = hashedPassword }
            };

            DataTable result = DbHelper.ExecuteQuery(query, parameters);

            if (result != null && result.Rows.Count > 0)
            {
                string role = result.Rows[0]["role"].ToString();
                string surname = result.Rows[0]["empl_surname"].ToString();

                MessageBox.Show($"Вітаємо, {role} {surname}!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.Hide();
                if (role == "Manager")
                {
                    ManagerForm managerForm = new ManagerForm(surname);
                    managerForm.Show();
                }
                else
                {
                    CashierForm cashierForm = new CashierForm(id, surname);
                    cashierForm.Show();
                }
            }
            else
            {
                MessageBox.Show("Невірний ID або пароль!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
