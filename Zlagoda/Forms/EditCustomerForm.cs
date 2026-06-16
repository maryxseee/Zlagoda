using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using Zlagoda.Classes;

namespace Zlagoda.Forms
{
    public partial class EditCustomerForm : Form
    {
        private readonly string _cardNumber;
        private TextBox txtSurname, txtName, txtPatronymic, txtPhone, txtCity, txtStreet, txtZip;
        private NumericUpDown numPercent;
        private Button btnSave;

        public EditCustomerForm(string cardNumber)
        {
            _cardNumber = cardNumber;
            InitializeCustomComponents();
            LoadCurrentData();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Редагувати карту клієнта";
            this.Size = new Size(450, 580);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            int x = 30, width = 370;

            AddLabel("Номер карти (не змінюється):", x, 20);
            var txtCardReadonly = new TextBox()
            {
                Location = new Point(x, 45),
                Width = width,
                Text = _cardNumber,
                ReadOnly = true,
                BackColor = Color.WhiteSmoke
            };

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
                Text = "ЗБЕРЕГТИ ЗМІНИ",
                Location = new Point(x, 490),
                Size = new Size(width, 45),
                BackColor = Color.Teal,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.Click += BtnSave_Click;

            this.Controls.AddRange(new Control[] {
                txtCardReadonly, txtSurname, txtName, txtPatronymic,
                txtPhone, txtCity, txtStreet, txtZip, numPercent, btnSave
            });
        }

        private void AddLabel(string text, int x, int y)
        {
            this.Controls.Add(new Label() { Text = text, Location = new Point(x, y), Width = 300 });
        }

        private void LoadCurrentData()
        {
            string query = @"SELECT cust_surname, cust_name, cust_patronymic, 
                                    phone_number, city, street, zip_code, perthent 
                             FROM Customer_Card WHERE card_number = @card";

            DataTable dt = DbHelper.ExecuteQuery(query, new SqlParameter[] {
                new SqlParameter("@card", _cardNumber)
            });

            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                txtSurname.Text = row["cust_surname"].ToString();
                txtName.Text = row["cust_name"].ToString();
                txtPatronymic.Text = row["cust_patronymic"] == DBNull.Value ? "" : row["cust_patronymic"].ToString();
                txtPhone.Text = row["phone_number"].ToString();
                txtCity.Text = row["city"] == DBNull.Value ? "" : row["city"].ToString();
                txtStreet.Text = row["street"] == DBNull.Value ? "" : row["street"].ToString();
                txtZip.Text = row["zip_code"] == DBNull.Value ? "" : row["zip_code"].ToString();
                numPercent.Value = Convert.ToDecimal(row["perthent"]);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSurname.Text) || string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Прізвище та ім'я є обов'язковими!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"UPDATE Customer_Card 
                             SET cust_surname    = @surname,
                                 cust_name       = @name,
                                 cust_patronymic = @patr,
                                 phone_number    = @phone,
                                 city            = @city,
                                 street          = @street,
                                 zip_code        = @zip,
                                 perthent        = @perc
                             WHERE card_number = @card";

            SqlParameter[] parameters = {
                new SqlParameter("@surname", txtSurname.Text.Trim()),
                new SqlParameter("@name",    txtName.Text.Trim()),
                new SqlParameter("@patr",    string.IsNullOrWhiteSpace(txtPatronymic.Text) ? (object)DBNull.Value : txtPatronymic.Text.Trim()),
                new SqlParameter("@phone",   txtPhone.Text.Trim()),
                new SqlParameter("@city",    string.IsNullOrWhiteSpace(txtCity.Text)   ? (object)DBNull.Value : txtCity.Text.Trim()),
                new SqlParameter("@street",  string.IsNullOrWhiteSpace(txtStreet.Text) ? (object)DBNull.Value : txtStreet.Text.Trim()),
                new SqlParameter("@zip",     string.IsNullOrWhiteSpace(txtZip.Text)    ? (object)DBNull.Value : txtZip.Text.Trim()),
                new SqlParameter("@perc",    (int)numPercent.Value),
                new SqlParameter("@card",    _cardNumber)
            };

            if (DbHelper.ExecuteNonQuery(query, parameters))
            {
                MessageBox.Show("Дані клієнта успішно оновлено!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}