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
using Label = System.Windows.Forms.Label;

namespace Zlagoda.Forms
{
    public partial class ManagerForm : Form
    {
        private string managerSurname;
        private DataGridView dataGridView;
        private Panel actionPanel;
        private Panel filterPanel;

        private ComboBox cbCategoryFilter;
        private TextBox txtUPCSearch;
        private ComboBox cbPromoFilter;

        private ComboBox cbRoleFilter;
        private TextBox txtSurnameSearch;
        private Label lblSurname, lblRole, lblCat, lblUPC, lblPromo;
        private NumericUpDown numPercentFilter;
        private Label lblPercent;

        private DateTimePicker dtpStart, dtpEnd;
        private ComboBox cbCashierFilter, cbProductFilter;
        private Label lblTotalSum, lblTotalQty;
        private DataGridView dgvCheckDetails;
        private Button btnApply;

        public ManagerForm(string surname)
        {
            this.managerSurname = surname;
            InitializeCustomComponents();
            LoadProducts();
        }

        private void InitializeCustomComponents()
        {
            this.Text = $"ZLAGODA - Панель Менеджера (Вітаємо, {managerSurname})";
            this.Size = new Size(1150, 750);
            this.StartPosition = FormStartPosition.CenterScreen;

            Panel menuPanel = new Panel() { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(45, 45, 48) };
            filterPanel = new Panel() { Dock = DockStyle.Top, Height = 60, BackColor = Color.WhiteSmoke, Visible = false };
            actionPanel = new Panel() { Dock = DockStyle.Right, Width = 180, BackColor = Color.LightGray };

            dataGridView = new DataGridView()
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            dataGridView.SelectionChanged += DataGridView_SelectionChanged;

            dgvCheckDetails = new DataGridView()
            {
                Dock = DockStyle.Bottom,
                Height = 180,
                BackgroundColor = Color.GhostWhite,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Visible = false
            };

            lblCat = new Label() { Text = "Категорія:", Location = new Point(10, 20), AutoSize = true };
            cbCategoryFilter = new ComboBox() { Location = new Point(80, 17), Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };
            cbCategoryFilter.SelectedIndexChanged += (s, e) => { if (cbCategoryFilter.Focused) LoadProducts(); };

            lblUPC = new Label() { Text = "UPC:", Location = new Point(230, 20), AutoSize = true };
            txtUPCSearch = new TextBox() { Location = new Point(270, 17), Width = 100 };
            txtUPCSearch.TextChanged += (s, e) => LoadStoreProducts();

            lblPromo = new Label() { Text = "Тип:", Location = new Point(390, 20), AutoSize = true };
            cbPromoFilter = new ComboBox() { Location = new Point(430, 17), Width = 110, DropDownStyle = ComboBoxStyle.DropDownList };
            cbPromoFilter.Items.AddRange(new string[] { "Всі", "Акційні", "Звичайні" });
            cbPromoFilter.SelectedIndex = 0;
            cbPromoFilter.SelectedIndexChanged += (s, e) => LoadStoreProducts();

            lblRole = new Label() { Text = "Посада:", Location = new Point(10, 20), AutoSize = true, Visible = false };
            cbRoleFilter = new ComboBox() { Location = new Point(70, 17), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList, Visible = false };
            cbRoleFilter.Items.AddRange(new string[] { "Всі", "Cashier", "Manager" });
            cbRoleFilter.SelectedIndex = 0;
            cbRoleFilter.SelectedIndexChanged += (s, e) => LoadEmployees();

            lblSurname = new Label() { Text = "Прізвище:", Location = new Point(190, 20), AutoSize = true, Visible = false };
            txtSurnameSearch = new TextBox() { Location = new Point(260, 17), Width = 120, Visible = false };
            txtSurnameSearch.TextChanged += (s, e) => LoadEmployees();

            lblPercent = new Label() { Text = "Знижка %:", Location = new Point(10, 20), AutoSize = true, Visible = false };
            numPercentFilter = new NumericUpDown() { Location = new Point(80, 17), Width = 50, Minimum = 0, Maximum = 100, Visible = false };
            numPercentFilter.ValueChanged += (s, e) => LoadCustomers();

            Label lblFrom = new Label() { Text = "З:", Location = new Point(10, 22), AutoSize = true, Visible = false };
            dtpStart = new DateTimePicker() { Location = new Point(30, 17), Width = 95, Format = DateTimePickerFormat.Short, Visible = false };
            Label lblTo = new Label() { Text = "По:", Location = new Point(132, 22), AutoSize = true, Visible = false };
            dtpEnd = new DateTimePicker() { Location = new Point(155, 17), Width = 95, Format = DateTimePickerFormat.Short, Visible = false };
            dtpEnd.Value = DateTime.Now;

            Label lblCashier = new Label() { Text = "Касир:", Location = new Point(260, 22), AutoSize = true, Visible = false };
            cbCashierFilter = new ComboBox() { Location = new Point(305, 17), Width = 130, DropDownStyle = ComboBoxStyle.DropDownList, Visible = false };

            Label lblProduct = new Label() { Text = "Товар:", Location = new Point(445, 22), AutoSize = true, Visible = false };
            cbProductFilter = new ComboBox() { Location = new Point(490, 17), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList, Visible = false };

            btnApply = new Button() { Text = "Пошук", Location = new Point(650, 15), Size = new Size(65, 28), Visible = false };
            btnApply.Click += (s, e) => LoadChecks();

            lblTotalSum = new Label() { Text = "Сума: 0", Location = new Point(725, 10), Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = true, Visible = false };
            lblTotalQty = new Label() { Text = "К-сть: 0", Location = new Point(725, 30), Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = true, Visible = false };

            filterPanel.Controls.AddRange(new Control[] {
                lblCat, cbCategoryFilter, lblUPC, txtUPCSearch, lblPromo, cbPromoFilter,
                lblRole, cbRoleFilter, lblSurname, txtSurnameSearch,
                lblPercent, numPercentFilter,
                lblFrom, dtpStart, lblTo, dtpEnd,
                lblCashier, cbCashierFilter, lblProduct, cbProductFilter,
                btnApply, lblTotalSum, lblTotalQty
            });

            AddMenuButton(menuPanel, "Товари", 10, (s, e) => { ShowFilter("Product"); LoadProducts(); });
            AddMenuButton(menuPanel, "Товари в магазині", 120, (s, e) => { ShowFilter("Store_Product"); LoadStoreProducts(); });
            AddMenuButton(menuPanel, "Категорії", 270, (s, e) => { ShowFilter("None"); LoadCategories(); });
            AddMenuButton(menuPanel, "Працівники", 380, (s, e) => { ShowFilter("Employee"); LoadEmployees(); });
            AddMenuButton(menuPanel, "Клієнти", 500, (s, e) => { ShowFilter("Customer_Card"); LoadCustomers(); });
            AddMenuButton(menuPanel, "Чеки", 600, (s, e) => { ShowFilter("Check"); LoadChecks(); });
            AddMenuButton(menuPanel, "Аналітика", 710, (s, e) => { new AnalyticsForm().ShowDialog(); });

            Button btnLogout = new Button()
            {
                Text = "Вихід",
                Dock = DockStyle.Right,
                Width = 80,
                BackColor = Color.IndianRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnLogout.Click += (s, e) => { this.Close(); new LoginForm().Show(); }
            ;
            menuPanel.Controls.Add(btnLogout);

            AddActionButton("Додати", 20, btnAdd_Click);
            AddActionButton("Редагувати", 70, btnEdit_Click);
            AddActionButton("Видалити", 120, btnDelete_Click);
            AddActionButton("Друк звіту", 200, btnPrint_Click);

            this.Controls.Add(dataGridView);
            this.Controls.Add(dgvCheckDetails);
            this.Controls.Add(filterPanel);
            this.Controls.Add(actionPanel);
            this.Controls.Add(menuPanel);

            LoadCategoriesForFilter();
        }

        private void ShowFilter(string mode)
        {
            filterPanel.Visible = (mode != "None");
            bool isCheckMode = (mode == "Check");

            lblCat.Visible = (mode == "Product");
            cbCategoryFilter.Visible = (mode == "Product");

            lblUPC.Visible = (mode == "Store_Product");
            txtUPCSearch.Visible = (mode == "Store_Product");
            lblPromo.Visible = (mode == "Store_Product");
            cbPromoFilter.Visible = (mode == "Store_Product");

            lblRole.Visible = (mode == "Employee");
            cbRoleFilter.Visible = (mode == "Employee");
            lblSurname.Visible = (mode == "Employee");
            txtSurnameSearch.Visible = (mode == "Employee");

            lblPercent.Visible = (mode == "Customer_Card");
            numPercentFilter.Visible = (mode == "Customer_Card");

            dtpStart.Visible = dtpEnd.Visible = isCheckMode;
            cbCashierFilter.Visible = cbProductFilter.Visible = isCheckMode;
            btnApply.Visible = lblTotalSum.Visible = lblTotalQty.Visible = isCheckMode;
            dgvCheckDetails.Visible = isCheckMode;

            if (isCheckMode)
            {
                LoadCashiersIntoFilter();
                LoadProductsIntoFilter();
            }
        }

        private void LoadCategoriesForFilter()
        {
            DataTable dt = DbHelper.ExecuteQuery("SELECT category_number, category_name FROM Category ORDER BY category_name");
            if (dt != null)
            {
                DataRow dr = dt.NewRow();
                dr["category_number"] = 0;
                dr["category_name"] = "Всі категорії";
                dt.Rows.InsertAt(dr, 0);
                cbCategoryFilter.DataSource = dt;
                cbCategoryFilter.DisplayMember = "category_name";
                cbCategoryFilter.ValueMember = "category_number";
            }
        }

        private void AddMenuButton(Panel parent, string text, int x, EventHandler onClick)
        {
            Button btn = new Button()
            {
                Text = text,
                Location = new Point(x, 10),
                Size = new Size(100, 40),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btn.Click += onClick;
            parent.Controls.Add(btn);
        }

        private void AddActionButton(string text, int y, EventHandler onClick)
        {
            Button btn = new Button()
            {
                Text = text,
                Location = new Point(15, y),
                Size = new Size(150, 40),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btn.Click += onClick;
            actionPanel.Controls.Add(btn);
        }


        private void LoadProducts()
        {
            int catId = (cbCategoryFilter.SelectedValue != null) ? (int)cbCategoryFilter.SelectedValue : 0;

            string query = @"SELECT p.id_product AS [ID], p.product_name AS [Назва], 
                            c.category_name AS [Категорія], p.manufacturer AS [Виробник] 
                            FROM Product p 
                            JOIN Category c ON p.category_number = c.category_number
                            WHERE (@catId = 0 OR p.category_number = @catId)
                            ORDER BY p.product_name";

            SqlParameter[] p = { new SqlParameter("@catId", catId) };
            dataGridView.DataSource = DbHelper.ExecuteQuery(query, p);
            dataGridView.Tag = "Product";
        }

        private void LoadStoreProducts()
        {
            string upc = txtUPCSearch.Text.Trim();
            int promoMode = cbPromoFilter.SelectedIndex;

            string query = @"SELECT sp.UPC, p.product_name AS [Товар], p.characteristics AS [Характеристики],
                            sp.selling_price AS [Ціна], sp.products_number AS [К-сть], 
                            CASE WHEN sp.promotional_product = 1 THEN 'Yes' ELSE 'No' END AS [Акційний]
                            FROM Store_Product sp
                            JOIN Product p ON sp.id_product = p.id_product
                            WHERE (@upc = '' OR sp.UPC LIKE @upc)";

            if (promoMode == 1) query += " AND sp.promotional_product = 1";
            else if (promoMode == 2) query += " AND sp.promotional_product = 0";

            query += " ORDER BY sp.products_number";

            SqlParameter[] p = { new SqlParameter("@upc", "%" + upc + "%") };
            dataGridView.DataSource = DbHelper.ExecuteQuery(query, p);
            dataGridView.Tag = "Store_Product";
        }

        private void LoadEmployees()
        {
            if (cbRoleFilter.SelectedItem == null) return;

            string roleFilter = cbRoleFilter.SelectedItem.ToString();
            string surname = txtSurnameSearch.Text.Trim();
            string surnameParam = string.IsNullOrEmpty(surname) ? "%" : "%" + surname + "%";

            string query = @"SELECT 
                id_employee AS [ID], 
                empl_surname AS [Прізвище], 
                empl_name AS [Ім'я], 
                role AS [Посада], 
                phone_number AS [Телефон], 
                city + ', ' + street + ' (' + zip_code + ')' AS [Адреса],
                salary AS [Зарплата],
                date_of_start AS [Початок роботи]
             FROM Employee
             WHERE (@role = N'Всі' OR role = @role)
             AND (empl_surname LIKE @surname)
             ORDER BY empl_surname";

            SqlParameter[] parameters = {
                new SqlParameter("@role", SqlDbType.NVarChar) { Value = roleFilter },
                new SqlParameter("@surname", SqlDbType.NVarChar) { Value = surnameParam }
            };

            DataTable dt = DbHelper.ExecuteQuery(query, parameters);
            if (dt != null)
            {
                dataGridView.DataSource = dt;
                dataGridView.Tag = "Employee";
            }
        }

        private void LoadCategories()
        {
            string query = "SELECT category_number AS [№], category_name AS [Назва] FROM Category ORDER BY category_name";
            dataGridView.DataSource = DbHelper.ExecuteQuery(query);
            dataGridView.Tag = "Category";
        }

        private void LoadCustomers()
        {
            int targetPercent = (int)numPercentFilter.Value;

            string query = @"SELECT 
                    card_number AS [Карта],
                    cust_surname AS [Прізвище],
                    cust_name AS [Ім'я],
                    cust_patronymic AS [По батькові],
                    phone_number AS [Телефон],
                    city + ' ' + street AS [Адреса],
                    perthent AS [Знижка %]
                FROM Customer_Card
                WHERE (@perc = 0 OR perthent = @perc)
                ORDER BY cust_surname";

            SqlParameter[] parameters = {
                new SqlParameter("@perc", SqlDbType.Int) { Value = targetPercent }
            };

            dataGridView.DataSource = DbHelper.ExecuteQuery(query, parameters);
            dataGridView.Tag = "Customer_Card";
        }

        private void LoadChecks()
        {
            try
            {
                string cashierId = "0";
                if (cbCashierFilter.SelectedValue != null)
                    cashierId = cbCashierFilter.SelectedValue.ToString();

                DateTime start = dtpStart.Value.Date;
                DateTime end = dtpEnd.Value.Date.AddDays(1).AddSeconds(-1);

                dataGridView.Tag = "Check";

                string query = @"SELECT c.check_number AS [№ Чека], e.empl_surname AS [Касир], 
                                c.print_date AS [Дата], c.sum_total AS [Сума], c.vat AS [ПДВ]
                         FROM [Check] c
                         JOIN Employee e ON c.id_employee = e.id_employee
                         WHERE (c.print_date BETWEEN @start AND @end)
                         AND (@empId = '0' OR c.id_employee = @empId)
                         ORDER BY c.print_date DESC";

                dataGridView.DataSource = DbHelper.ExecuteQuery(query, new SqlParameter[] {
                    new SqlParameter("@start", start),
                    new SqlParameter("@end", end),
                    new SqlParameter("@empId", cashierId)
                });

                // Загальна сума — окремі параметри, бо SqlParameter не можна передавати двічі
                string sumQuery = @"SELECT SUM(sum_total) FROM [Check] 
                                    WHERE (print_date BETWEEN @start AND @end) 
                                    AND (@empId = '0' OR id_employee = @empId)";
                DataTable dtSum = DbHelper.ExecuteQuery(sumQuery, new SqlParameter[] {
                    new SqlParameter("@start", start),
                    new SqlParameter("@end", end),
                    new SqlParameter("@empId", cashierId)
                });

                if (dtSum != null && dtSum.Rows.Count > 0 && dtSum.Rows[0][0] != DBNull.Value)
                    lblTotalSum.Text = "Загальна сума: " + Convert.ToDecimal(dtSum.Rows[0][0]).ToString("F2") + " грн";
                else
                    lblTotalSum.Text = "Загальна сума: 0.00 грн";

                // Кількість обраного товару
                if (cbProductFilter.SelectedValue != null && (int)cbProductFilter.SelectedValue != 0)
                {
                    string qtyQuery = @"SELECT SUM(s.product_number) FROM Sale s 
                                JOIN [Check] c ON s.check_number = c.check_number
                                JOIN Store_Product sp ON s.UPC = sp.UPC
                                WHERE sp.id_product = @prodId AND (c.print_date BETWEEN @start AND @end)";

                    SqlParameter[] pQty = {
                        new SqlParameter("@prodId", cbProductFilter.SelectedValue),
                        new SqlParameter("@start", start),
                        new SqlParameter("@end", end)
                    };

                    DataTable dtQty = DbHelper.ExecuteQuery(qtyQuery, pQty);
                    if (dtQty != null && dtQty.Rows.Count > 0 && dtQty.Rows[0][0] != DBNull.Value)
                        lblTotalQty.Text = "Продано одиниць: " + dtQty.Rows[0][0].ToString();
                    else
                        lblTotalQty.Text = "Продано одиниць: 0";
                }
                else
                {
                    lblTotalQty.Text = "Продано одиниць: 0";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка завантаження чеків: " + ex.Message);
            }
        }

        private void LoadCashiersIntoFilter()
        {
            DataTable dt = DbHelper.ExecuteQuery("SELECT id_employee, empl_surname FROM Employee WHERE role = 'Cashier' ORDER BY empl_surname");
            if (dt != null)
            {
                DataRow dr = dt.NewRow();
                dr["id_employee"] = "0";
                dr["empl_surname"] = "Всі касири";
                dt.Rows.InsertAt(dr, 0);
                cbCashierFilter.DataSource = dt;
                cbCashierFilter.DisplayMember = "empl_surname";
                cbCashierFilter.ValueMember = "id_employee";
            }
        }

        private void LoadProductsIntoFilter()
        {
            DataTable dt = DbHelper.ExecuteQuery("SELECT id_product, product_name FROM Product ORDER BY product_name");
            if (dt != null)
            {
                DataRow dr = dt.NewRow();
                dr["id_product"] = 0;
                dr["product_name"] = "Оберіть товар для статистики";
                dt.Rows.InsertAt(dr, 0);
                cbProductFilter.DataSource = dt;
                cbProductFilter.DisplayMember = "product_name";
                cbProductFilter.ValueMember = "id_product";
            }
        }

        private void DataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView.Tag?.ToString() == "Check" && dataGridView.SelectedRows.Count > 0)
            {
                var cellValue = dataGridView.SelectedRows[0].Cells[0].Value;
                if (cellValue == null || cellValue == DBNull.Value) return;

                string checkNum = cellValue.ToString();
                string query = @"SELECT p.product_name AS [Товар], s.product_number AS [Кількість], s.selling_price AS [Ціна]
                         FROM Sale s
                         JOIN Store_Product sp ON s.UPC = sp.UPC
                         JOIN Product p ON sp.id_product = p.id_product
                         WHERE s.check_number = @num";
                dgvCheckDetails.DataSource = DbHelper.ExecuteQuery(query, new SqlParameter[] { new SqlParameter("@num", checkNum) });
            }
        }
        private void btnAdd_Click(object sender, EventArgs e)
        {
            string currentTable = dataGridView.Tag?.ToString();

            if (currentTable == "Category")
            {
                AddCategoryForm catForm = new AddCategoryForm();
                if (catForm.ShowDialog() == DialogResult.OK)
                    LoadCategories();
            }
            else if (currentTable == "Product")
            {
                AddProductForm prodForm = new AddProductForm();
                if (prodForm.ShowDialog() == DialogResult.OK)
                    LoadProducts();
            }
            else if (currentTable == "Store_Product")
            {
                AddStoreProductForm spForm = new AddStoreProductForm();
                if (spForm.ShowDialog() == DialogResult.OK)
                    LoadStoreProducts();
            }
            else if (currentTable == "Employee")
            {
                AddEmployeeForm empForm = new AddEmployeeForm();
                if (empForm.ShowDialog() == DialogResult.OK)
                    LoadEmployees();
            }
            else if (currentTable == "Customer_Card")
            {
                AddCustomerForm custForm = new AddCustomerForm();
                if (custForm.ShowDialog() == DialogResult.OK)
                    LoadCustomers();
            }
            else
            {
                MessageBox.Show("Для цієї таблиці форму додавання ще не створено.");
            }
        }
        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                string currentTable = dataGridView.Tag?.ToString();
                if (string.IsNullOrEmpty(currentTable)) return;

                DataRow row = ((DataRowView)dataGridView.SelectedRows[0].DataBoundItem).Row;
                Form editForm = null;

                switch (currentTable)
                {
                    case "Employee":
                        string empId = dataGridView.SelectedRows[0].Cells[0].Value.ToString();
                        editForm = new EditEmployeeForm(empId);
                        break;

                    case "Customer_Card":
                        string cardNumber = dataGridView.SelectedRows[0].Cells[0].Value.ToString();
                        editForm = new EditCustomerForm(cardNumber);
                        break;

                    case "Category":
                        int catId = Convert.ToInt32(row["№"]);
                        string catName = row["Назва"].ToString();
                        editForm = new EditCategoryForm(catId, catName);
                        break;

                    case "Product":
                        int rowtId = Convert.ToInt32(row["ID"]);
                        editForm = new EditProductForm(rowtId);
                        break;

                    case "Store_Product":
                        string upcName = row["upc"].ToString();

                        editForm = new EditStoreProductForm(upcName);
                        break;
                }

                if (editForm != null)
                {
                    if (editForm.ShowDialog() == DialogResult.OK)
                    {
                        switch (currentTable)
                        {
                            case "Employee": LoadEmployees(); break;
                            case "Customer_Card": LoadCustomers(); break;
                            case "Category": LoadCategories(); break;
                            case "Product": LoadProducts(); break;
                            case "Store_Product": LoadStoreProducts(); break;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Редагування для цієї таблиці ще не реалізовано.");
                }
            }
            else
            {
                MessageBox.Show("Будь ласка, оберіть рядок для редагування!", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Оберіть рядок для видалення.");
                return;
            }

            string table = dataGridView.Tag?.ToString();
            if (string.IsNullOrEmpty(table))
            {
                MessageBox.Show("Не визначено таблицю.");
                return;
            }

            string pkName;
            switch (table)
            {
                case "Category": pkName = "category_number"; break;
                case "Product": pkName = "id_product"; break;
                case "Employee": pkName = "id_employee"; break;
                case "Store_Product": pkName = "UPC"; break;
                case "Customer_Card": pkName = "card_number"; break;
                case "Check": pkName = "check_number"; break;
                default:
                    MessageBox.Show("Видалення для цієї таблиці не підтримується.");
                    return;
            }

            string id = dataGridView.SelectedRows[0].Cells[0].Value.ToString();

            if (MessageBox.Show("Ви впевнені, що хочете видалити цей запис?", "Видалення",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                string query = $"DELETE FROM [{table}] WHERE {pkName} = @id";
                SqlParameter[] p = { new SqlParameter("@id", id) };
                DbHelper.ExecuteNonQuery(query, p);

                switch (table)
                {
                    case "Category": LoadCategories(); break;
                    case "Product": LoadProducts(); break;
                    case "Employee": LoadEmployees(); break;
                    case "Store_Product": LoadStoreProducts(); break;
                    case "Customer_Card": LoadCustomers(); break;
                    case "Check": LoadChecks(); break;
                }
            }
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            DataTable dt = dataGridView.DataSource as DataTable;

            if (dt == null || dt.Rows.Count == 0)
            {
                MessageBox.Show("Немає даних для друку.", "Звіт",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string table = dataGridView.Tag != null
                ? dataGridView.Tag.ToString()
                : "";

            string title = "Звіт";

            switch (table)
            {
                case "Employee":
                    title = "Звіт: Працівники";
                    break;

                case "Customer_Card":
                    title = "Звіт: Постійні клієнти";
                    break;

                case "Category":
                    title = "Звіт: Категорії товарів";
                    break;

                case "Product":
                    title = "Звіт: Товари";
                    break;

                case "Store_Product":
                    title = "Звіт: Товари у магазині";
                    break;

                case "Check":
                    title = "Звіт: Чеки";
                    break;
            }

            ReportPreviewForm preview = new ReportPreviewForm(title, managerSurname, dt);
            preview.ShowDialog();
        }
    }
}