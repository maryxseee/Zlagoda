using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using Zlagoda.Classes;

namespace Zlagoda.Forms
{
    public partial class EditEmployeeForm : Form
    {
        private readonly string _employeeId;
        private TextBox txtSurname, txtName, txtPhone, txtCity, txtStreet, txtZip;
        private TextBox txtNewPassword;
        private CheckBox chkChangePassword;
        private ComboBox cbRole;
        private NumericUpDown numSalary;
        private DateTimePicker dtpBirth, dtpStart;
        private Button btnSave;

        public EditEmployeeForm(string employeeId)
        {
            _employeeId = employeeId;
            InitializeCustomComponents();
            LoadCurrentData();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Редагувати працівника";
            this.Size = new Size(500, 680);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.AutoScroll = true;

            int x = 30, x2 = 250, width = 180;

            AddLabel("ID працівника (не змінюється):", x, 20);
            var txtIdReadonly = new TextBox()
            {
                Location = new Point(x, 45),
                Width = width,
                Text = _employeeId,
                ReadOnly = true,
                BackColor = Color.WhiteSmoke
            };

            AddLabel("Прізвище:", x, 90);
            txtSurname = new TextBox() { Location = new Point(x, 115), Width = width };

            AddLabel("Ім'я:", x2, 90);
            txtName = new TextBox() { Location = new Point(x2, 115), Width = width };

            AddLabel("Посада:", x, 160);
            cbRole = new ComboBox()
            {
                Location = new Point(x, 185),
                Width = width,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cbRole.Items.AddRange(new string[] { "Manager", "Cashier" });
            cbRole.SelectedIndex = 1;

            AddLabel("Зарплата:", x2, 160);
            numSalary = new NumericUpDown()
            {
                Location = new Point(x2, 185),
                Width = width,
                Maximum = 1000000,
                DecimalPlaces = 2
            };

            AddLabel("Дата народження:", x, 230);
            dtpBirth = new DateTimePicker()
            {
                Location = new Point(x, 255),
                Width = width,
                Format = DateTimePickerFormat.Short
            };

            AddLabel("Дата початку роботи:", x2, 230);
            dtpStart = new DateTimePicker()
            {
                Location = new Point(x2, 255),
                Width = width,
                Format = DateTimePickerFormat.Short
            };

            AddLabel("Телефон (напр. +380...):", x, 300);
            txtPhone = new TextBox() { Location = new Point(x, 325), Width = width, MaxLength = 13 };

            AddLabel("Місто:", x2, 300);
            txtCity = new TextBox() { Location = new Point(x2, 325), Width = width };

            AddLabel("Вулиця:", x, 370);
            txtStreet = new TextBox() { Location = new Point(x, 395), Width = width };

            AddLabel("Індекс:", x2, 370);
            txtZip = new TextBox() { Location = new Point(x2, 395), Width = width, MaxLength = 9 };

            chkChangePassword = new CheckBox()
            {
                Text = "Змінити пароль",
                Location = new Point(x, 445),
                Width = 180,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            chkChangePassword.CheckedChanged += (s, e) =>
            {
                txtNewPassword.Visible = chkChangePassword.Checked;
            };

            AddLabel("Новий пароль:", x2, 435);
            txtNewPassword = new TextBox()
            {
                Location = new Point(x2, 458),
                Width = width,
                UseSystemPasswordChar = true,
                Visible = false
            };

            btnSave = new Button()
            {
                Text = "ЗБЕРЕГТИ ЗМІНИ",
                Location = new Point(30, 510),
                Size = new Size(400, 50),
                BackColor = Color.SteelBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.Click += BtnSave_Click;

            this.Controls.AddRange(new Control[] {
                txtIdReadonly, txtSurname, txtName, cbRole, numSalary,
                dtpBirth, dtpStart, txtPhone, txtCity, txtStreet, txtZip,
                chkChangePassword, txtNewPassword, btnSave
            });
        }

        private void AddLabel(string text, int x, int y)
        {
            this.Controls.Add(new Label() { Text = text, Location = new Point(x, y), Width = 200 });
        }

        private void LoadCurrentData()
        {
            string query = @"SELECT empl_surname, empl_name, role, salary, 
                                    date_of_birth, date_of_start, 
                                    phone_number, city, street, zip_code
                             FROM Employee WHERE id_employee = @id";

            DataTable dt = DbHelper.ExecuteQuery(query, new SqlParameter[] {
                new SqlParameter("@id", _employeeId)
            });

            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow r = dt.Rows[0];

                txtSurname.Text = r["empl_surname"].ToString();
                txtName.Text = r["empl_name"].ToString();
                txtPhone.Text = r["phone_number"].ToString();
                txtCity.Text = r["city"].ToString();
                txtStreet.Text = r["street"].ToString();
                txtZip.Text = r["zip_code"].ToString();
                numSalary.Value = Convert.ToDecimal(r["salary"]);
                dtpBirth.Value = Convert.ToDateTime(r["date_of_birth"]);
                dtpStart.Value = Convert.ToDateTime(r["date_of_start"]);

                string role = r["role"].ToString();
                cbRole.SelectedIndex = cbRole.Items.IndexOf(role);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSurname.Text) || string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Прізвище та ім'я є обов'язковими!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int age = DateTime.Today.Year - dtpBirth.Value.Year;
            if (dtpBirth.Value.Date > DateTime.Today.AddYears(-age)) age--;
            if (age < 18)
            {
                MessageBox.Show("Працівнику має бути не менше 18 років!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (chkChangePassword.Checked && string.IsNullOrWhiteSpace(txtNewPassword.Text))
            {
                MessageBox.Show("Введіть новий пароль!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query;
            SqlParameter[] parameters;

            if (chkChangePassword.Checked)
            {
                string hashedPassword = PasswordHasher.HashPassword(txtNewPassword.Text);

                query = @"UPDATE Employee SET
                            empl_surname  = @surname,
                            empl_name     = @name,
                            role          = @role,
                            salary        = @salary,
                            date_of_birth = @birth,
                            date_of_start = @start,
                            phone_number  = @phone,
                            city          = @city,
                            street        = @street,
                            zip_code      = @zip,
                            password_hash = @pass
                          WHERE id_employee = @id";

                parameters = new SqlParameter[] {
                    new SqlParameter("@surname", txtSurname.Text.Trim()),
                    new SqlParameter("@name",    txtName.Text.Trim()),
                    new SqlParameter("@role",    cbRole.SelectedItem.ToString()),
                    new SqlParameter("@salary",  numSalary.Value),
                    new SqlParameter("@birth",   dtpBirth.Value),
                    new SqlParameter("@start",   dtpStart.Value),
                    new SqlParameter("@phone",   txtPhone.Text.Trim()),
                    new SqlParameter("@city",    txtCity.Text.Trim()),
                    new SqlParameter("@street",  txtStreet.Text.Trim()),
                    new SqlParameter("@zip",     txtZip.Text.Trim()),
                    new SqlParameter("@pass",    hashedPassword),
                    new SqlParameter("@id",      _employeeId)
                };
            }
            else
            {
                query = @"UPDATE Employee SET
                            empl_surname  = @surname,
                            empl_name     = @name,
                            role          = @role,
                            salary        = @salary,
                            date_of_birth = @birth,
                            date_of_start = @start,
                            phone_number  = @phone,
                            city          = @city,
                            street        = @street,
                            zip_code      = @zip
                          WHERE id_employee = @id";

                parameters = new SqlParameter[] {
                    new SqlParameter("@surname", txtSurname.Text.Trim()),
                    new SqlParameter("@name",    txtName.Text.Trim()),
                    new SqlParameter("@role",    cbRole.SelectedItem.ToString()),
                    new SqlParameter("@salary",  numSalary.Value),
                    new SqlParameter("@birth",   dtpBirth.Value),
                    new SqlParameter("@start",   dtpStart.Value),
                    new SqlParameter("@phone",   txtPhone.Text.Trim()),
                    new SqlParameter("@city",    txtCity.Text.Trim()),
                    new SqlParameter("@street",  txtStreet.Text.Trim()),
                    new SqlParameter("@zip",     txtZip.Text.Trim()),
                    new SqlParameter("@id",      _employeeId)
                };
            }

            if (DbHelper.ExecuteNonQuery(query, parameters))
            {
                MessageBox.Show("Дані працівника успішно оновлено!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}