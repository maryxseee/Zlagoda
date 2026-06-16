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
    public partial class AddEmployeeForm : Form
    {
        private TextBox txtId, txtSurname, txtName, txtPatronymic, txtPhone, txtCity, txtStreet, txtZip, txtPassword;
        private ComboBox cbRole;
        private NumericUpDown numSalary;
        private DateTimePicker dtpBirth, dtpStart;
        private Button btnSave;

        public AddEmployeeForm()
        {
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Додати нового працівника";
            this.Size = new Size(500, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.AutoScroll = true;

            int x = 30, x2 = 250, width = 180;

            AddLabel("ID (Логін, до 10 симв.):", x, 20);
            txtId = new TextBox() { Location = new Point(x, 45), Width = width, MaxLength = 10 };

            AddLabel("Тимчасовий пароль:", x2, 20);
            txtPassword = new TextBox() { Location = new Point(x2, 45), Width = width, UseSystemPasswordChar = true };

            AddLabel("Прізвище:", x, 90);
            txtSurname = new TextBox() { Location = new Point(x, 115), Width = width };
            AddLabel("Ім'я:", x2, 90);
            txtName = new TextBox() { Location = new Point(x2, 115), Width = width };

            AddLabel("Посада:", x, 160);
            cbRole = new ComboBox() { Location = new Point(x, 185), Width = width, DropDownStyle = ComboBoxStyle.DropDownList };
            cbRole.Items.AddRange(new string[] { "Manager", "Cashier" });
            cbRole.SelectedIndex = 1;

            AddLabel("Зарплата:", x2, 160);
            numSalary = new NumericUpDown() { Location = new Point(x2, 185), Width = width, Maximum = 1000000, DecimalPlaces = 2 };

            AddLabel("Дата народження:", x, 230);
            dtpBirth = new DateTimePicker() { Location = new Point(x, 255), Width = width, Format = DateTimePickerFormat.Short };

            AddLabel("Дата початку роботи:", x2, 230);
            dtpStart = new DateTimePicker() { Location = new Point(x2, 255), Width = width, Format = DateTimePickerFormat.Short };

            AddLabel("Телефон (напр. +380...):", x, 300);
            txtPhone = new TextBox() { Location = new Point(x, 325), Width = width, MaxLength = 13 };

            AddLabel("Місто:", x2, 300);
            txtCity = new TextBox() { Location = new Point(x2, 325), Width = width };

            AddLabel("Вулиця:", x, 370);
            txtStreet = new TextBox() { Location = new Point(x, 395), Width = width };

            AddLabel("Індекс:", x2, 370);
            txtZip = new TextBox() { Location = new Point(x2, 395), Width = width, MaxLength = 9 };

            btnSave = new Button()
            {
                Text = "ЗБЕРЕГТИ ПРАЦІВНИКА",
                Location = new Point(30, 480),
                Size = new Size(400, 50),
                BackColor = Color.SteelBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnSave.Click += BtnSave_Click;

            this.Controls.AddRange(new Control[] { txtId, txtPassword, txtSurname, txtName, cbRole, numSalary, dtpBirth, dtpStart, txtPhone, txtCity, txtStreet, txtZip, btnSave });
        }

        private void AddLabel(string text, int x, int y)
        {
            Label lbl = new Label() { Text = text, Location = new Point(x, y), Width = 200 };
            this.Controls.Add(lbl);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtId.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("ID та Пароль є обов'язковими!");
                return;
            }

            int age = DateTime.Today.Year - dtpBirth.Value.Year;
            if (dtpBirth.Value.Date > DateTime.Today.AddYears(-age)) age--;
            if (age < 18)
            {
                MessageBox.Show("Працівнику має бути не менше 18 років!");
                return;
            }

            string hashedPassword = PasswordHasher.HashPassword(txtPassword.Text);

            string query = @"INSERT INTO Employee 
                (id_employee, empl_surname, empl_name, role, salary, date_of_birth, date_of_start, phone_number, city, street, zip_code, password_hash) 
                VALUES 
                (@id, @surname, @name, @role, @salary, @birth, @start, @phone, @city, @street, @zip, @pass)";

            SqlParameter[] parameters = {
                new SqlParameter("@id", txtId.Text.Trim()),
                new SqlParameter("@surname", txtSurname.Text.Trim()),
                new SqlParameter("@name", txtName.Text.Trim()),
                new SqlParameter("@role", cbRole.SelectedItem.ToString()),
                new SqlParameter("@salary", numSalary.Value),
                new SqlParameter("@birth", dtpBirth.Value),
                new SqlParameter("@start", dtpStart.Value),
                new SqlParameter("@phone", txtPhone.Text.Trim()),
                new SqlParameter("@city", txtCity.Text.Trim()),
                new SqlParameter("@street", txtStreet.Text.Trim()),
                new SqlParameter("@zip", txtZip.Text.Trim()),
                new SqlParameter("@pass", hashedPassword)
            };

            if (DbHelper.ExecuteNonQuery(query, parameters))
            {
                MessageBox.Show("Працівника успішно додано!");
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
