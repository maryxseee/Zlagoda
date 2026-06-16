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
    public partial class AddCustomerForm : Form
    {
        private TextBox txtCardNumber, txtSurname, txtName, txtPatronymic, txtPhone, txtCity, txtStreet, txtZip;
        private NumericUpDown numPercent;
        private Button btnSave;

        public AddCustomerForm()
        {
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Реєстрація карти клієнта";
            this.Size = new Size(450, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            int x = 30, width = 370;

            AddLabel("Номер карти (13 цифр):", x, 20);
            txtCardNumber = new TextBox() { Location = new Point(x, 45), Width = width, MaxLength = 13 };

            AddLabel("Прізвище:", x, 85);
            txtSurname = new TextBox() { Location = new Point(x, 110), Width = width };

            AddLabel("Ім'я:", x, 150);
            txtName = new TextBox() { Location = new Point(x, 175), Width = width };

            AddLabel("По батькові (необов'язково):", x, 215);
            txtPatronymic = new TextBox() { Location = new Point(x, 240), Width = width };

            AddLabel("Телефон (+380...):", x, 280);
            txtPhone = new TextBox() { Location = new Point(x, 305), Width = width, MaxLength = 13 };

            AddLabel("Місто, вулиця, індекс:", x, 345);
            txtCity = new TextBox() { Location = new Point(x, 370), Width = 120 };
            txtStreet = new TextBox() { Location = new Point(x + 125, 370), Width = 150 };
            txtZip = new TextBox() { Location = new Point(x + 280, 370), Width = 90 };

            AddLabel("Відсоток знижки (%):", x, 410);
            numPercent = new NumericUpDown()
            {
                Location = new Point(x, 435),
                Width = 100,
                Minimum = 0,
                Maximum = 100
            };

            btnSave = new Button()
            {
                Text = "ЗБЕРЕГТИ КЛІЄНТА",
                Location = new Point(x, 490),
                Size = new Size(width, 45),
                BackColor = Color.Teal,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnSave.Click += BtnSave_Click;

            this.Controls.AddRange(new Control[] {
                txtCardNumber, txtSurname, txtName, txtPatronymic,
                txtPhone, txtCity, txtStreet, txtZip, numPercent, btnSave
            });
        }

        private void AddLabel(string text, int x, int y)
        {
            Label lbl = new Label() { Text = text, Location = new Point(x, y), Width = 300 };
            this.Controls.Add(lbl);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCardNumber.Text) || txtCardNumber.Text.Length > 13)
            {
                MessageBox.Show("Номер карти повинен містити до 13 символів!");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtSurname.Text) || string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Прізвище та ім'я є обов'язковими!");
                return;
            }

            string query = @"INSERT INTO Customer_Card 
                (card_number, cust_surname, cust_name, cust_patronymic, phone_number, city, street, zip_code, perthent) 
                VALUES 
                (@card, @surname, @name, @patr, @phone, @city, @street, @zip, @perc)";

            SqlParameter[] parameters = {
                new SqlParameter("@card", txtCardNumber.Text.Trim()),
                new SqlParameter("@surname", txtSurname.Text.Trim()),
                new SqlParameter("@name", txtName.Text.Trim()),
                new SqlParameter("@patr", (object)txtPatronymic.Text.Trim() ?? DBNull.Value),
                new SqlParameter("@phone", txtPhone.Text.Trim()),
                new SqlParameter("@city", (object)txtCity.Text.Trim() ?? DBNull.Value),
                new SqlParameter("@street", (object)txtStreet.Text.Trim() ?? DBNull.Value),
                new SqlParameter("@zip", (object)txtZip.Text.Trim() ?? DBNull.Value),
                new SqlParameter("@perc", (int)numPercent.Value)
            };

            if (DbHelper.ExecuteNonQuery(query, parameters))
            {
                MessageBox.Show("Карту клієнта успішно зареєстровано!");
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
