using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zlagoda.Classes;
using Label = System.Windows.Forms.Label;

namespace Zlagoda.Forms
{
    public partial class CashierForm : Form
    {
        private string cashierId;
        private string cashierSurname;
        private DateTimePicker dtpStart, dtpEnd;
        private DataGridView dgvChecksList, dgvCheckDetails;

        private TabControl mainTabControl;
        private DataGridView dgvProducts, dgvCustomers, dgvChecks;
        private TextBox txtSearchProduct, txtSearchCustomer;

        private ComboBox cbCategoryFilter;
        private ComboBox cbPromoFilter;

        private DataGridView dgvCart;
        private DataTable cartTable;
        private TextBox txtUPCInput, txtCustomerCard;
        private Label lblTotal, lblVAT, lblDiscount;

        public CashierForm(string id, string surname)
        {
            this.cashierId = id;
            this.cashierSurname = surname;
            InitializeCustomComponents();
            InitCart();
        }

        private void InitializeCustomComponents()
        {
            this.Text = $"ZLAGODA - Касир: {cashierSurname} (ID: {cashierId})";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            mainTabControl = new TabControl() { Dock = DockStyle.Fill };

            TabPage tabSale = new TabPage("Оформлення продажу");
            SetupSaleTab(tabSale);

            TabPage tabProducts = new TabPage("Товари та пошук");
            SetupProductsTab(tabProducts);

            TabPage tabCustomers = new TabPage("Постійні клієнти");
            SetupCustomersTab(tabCustomers);

            TabPage tabChecks = new TabPage("Мої чеки");
            SetupChecksTab(tabChecks);

            TabPage tabMe = new TabPage("Мій профіль");
            SetupMeTab(tabMe);

            mainTabControl.TabPages.AddRange(new TabPage[] { tabSale, tabProducts, tabCustomers, tabChecks, tabMe });
            this.Controls.Add(mainTabControl);
        }

        private void SetupSaleTab(TabPage page)
        {
            Panel pnlInput = new Panel() { Dock = DockStyle.Top, Height = 100, BackColor = Color.LightSteelBlue };

            Label lblUPC = new Label() { Text = "Введіть UPC товару:", Location = new Point(20, 20), Width = 150 };
            txtUPCInput = new TextBox() { Location = new Point(20, 45), Width = 150, Font = new Font("Segoe UI", 12) };
            txtUPCInput.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) AddToCart(); }
            ;

            Button btnAdd = new Button() { Text = "Додати в чек", Location = new Point(180, 43), Size = new Size(100, 30) };
            btnAdd.Click += (s, e) => AddToCart();

            Label lblCard = new Label() { Text = "Карта клієнта:", Location = new Point(350, 20), Width = 150 };
            txtCustomerCard = new TextBox() { Location = new Point(350, 45), Width = 150, Font = new Font("Segoe UI", 12) };
            txtCustomerCard.TextChanged += (s, e) => RecalculateTotal();

            pnlInput.Controls.AddRange(new Control[] { lblUPC, txtUPCInput, btnAdd, lblCard, txtCustomerCard });

            dgvCart = new DataGridView()
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false
            };

            Panel pnlTotal = new Panel() { Dock = DockStyle.Bottom, Height = 120, BackColor = Color.WhiteSmoke };
            lblTotal = new Label() { Text = "РАЗОМ: 0.00 грн", Font = new Font("Segoe UI", 16, FontStyle.Bold), Location = new Point(700, 20), Size = new Size(300, 30) };
            lblVAT = new Label() { Text = "ПДВ (20%): 0.00 грн", Location = new Point(700, 55), Size = new Size(300, 20) };
            lblDiscount = new Label() { Text = "Знижка: 0.00 грн", Location = new Point(700, 75), Size = new Size(300, 20) };

            Button btnFinish = new Button()
            {
                Text = "ЗАКРИТИ ЧЕК",
                BackColor = Color.Green,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Size = new Size(200, 50),
                Location = new Point(20, 35)
            };
            btnFinish.Click += btnFinishCheck_Click;

            pnlTotal.Controls.AddRange(new Control[] { lblTotal, lblVAT, lblDiscount, btnFinish });

            page.Controls.Add(dgvCart);
            page.Controls.Add(pnlInput);
            page.Controls.Add(pnlTotal);
        }

        private void InitCart()
        {
            cartTable = new DataTable();
            cartTable.Columns.Add("UPC");
            cartTable.Columns.Add("Назва");
            cartTable.Columns.Add("Ціна");
            cartTable.Columns.Add("Кількість", typeof(int));
            cartTable.Columns.Add("Сума", typeof(decimal));
            dgvCart.DataSource = cartTable;
        }

        private void AddToCart()
        {
            string upc = txtUPCInput.Text.Trim();
            if (string.IsNullOrEmpty(upc)) return;

            DataRow[] existingRows = cartTable.Select($"UPC = '{upc}'");

            string query = @"SELECT sp.selling_price, sp.products_number, p.product_name 
                     FROM Store_Product sp JOIN Product p ON sp.id_product = p.id_product 
                     WHERE sp.UPC = @upc";

            SqlParameter[] p = { new SqlParameter("@upc", upc) };
            DataTable res = DbHelper.ExecuteQuery(query, p);

            if (res.Rows.Count > 0)
            {
                decimal price = Convert.ToDecimal(res.Rows[0]["selling_price"]);
                string name = res.Rows[0]["product_name"].ToString();
                int stock = Convert.ToInt32(res.Rows[0]["products_number"]);

                if (existingRows.Length > 0)
                {
                    DataRow row = existingRows[0];
                    int currentQty = Convert.ToInt32(row["Кількість"]);

                    if (currentQty + 1 > stock)
                    {
                        MessageBox.Show($"Немає більше одиниць товару! (В наявності: {stock})");
                        return;
                    }

                    row["Кількість"] = currentQty + 1;
                    row["Сума"] = (currentQty + 1) * price;
                }
                else
                {
                    if (stock <= 0)
                    {
                        MessageBox.Show("Товару немає в наявності!");
                        return;
                    }
                    cartTable.Rows.Add(upc, name, price, 1, price);
                }

                RecalculateTotal();
                txtUPCInput.Clear();
                txtUPCInput.Focus();
            }
            else
            {
                MessageBox.Show("Товар не знайдено!");
            }
        }

        private void dgvCart_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && dgvCart.CurrentRow != null)
            {
                dgvCart.Rows.Remove(dgvCart.CurrentRow);
                RecalculateTotal();
            }
        }

        private void RecalculateTotal()
        {
            decimal sum = 0;
            foreach (DataRow row in cartTable.Rows)
                sum += Convert.ToDecimal(row["Сума"]);

            int discountPercent = 0;
            if (!string.IsNullOrEmpty(txtCustomerCard.Text))
            {
                DataTable dt = DbHelper.ExecuteQuery("SELECT perthent FROM Customer_Card WHERE card_number = @c",
                               new SqlParameter[] { new SqlParameter("@c", txtCustomerCard.Text) });
                if (dt.Rows.Count > 0) discountPercent = Convert.ToInt32(dt.Rows[0]["perthent"]);
            }

            decimal discountAmount = sum * (discountPercent / 100m);
            decimal finalSum = sum - discountAmount;
            decimal vat = finalSum * 0.2m;

            lblTotal.Text = $"РАЗОМ: {finalSum:F2} грн";
            lblVAT.Text = $"ПДВ (20%): {vat:F2} грн";
            lblDiscount.Text = $"Знижка ({discountPercent}%): {discountAmount:F2} грн";
        }

        private void SetupProductsTab(TabPage page)
        {
            Panel pnlTop = new Panel() { Dock = DockStyle.Top, Height = 80, BackColor = Color.WhiteSmoke };

            Label lblSearch = new Label() { Text = "Пошук за назвою:", Location = new Point(10, 10), AutoSize = true };
            txtSearchProduct = new TextBox() { Location = new Point(10, 30), Width = 180 };
            txtSearchProduct.TextChanged += (s, e) => ApplyProductFilters();

            Label lblCat = new Label() { Text = "Категорія:", Location = new Point(210, 10), AutoSize = true };
            cbCategoryFilter = new ComboBox()
            {
                Location = new Point(210, 30),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cbCategoryFilter.SelectedIndexChanged += (s, e) => ApplyProductFilters();

            Label lblPromo = new Label() { Text = "Тип товару:", Location = new Point(380, 10), AutoSize = true };
            cbPromoFilter = new ComboBox()
            {
                Location = new Point(380, 30),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cbPromoFilter.Items.AddRange(new string[] { "Всі товари", "Тільки акційні", "Тільки не акційні" });
            cbPromoFilter.SelectedIndex = 0;
            cbPromoFilter.SelectedIndexChanged += (s, e) => ApplyProductFilters();

            pnlTop.Controls.AddRange(new Control[] { lblSearch, txtSearchProduct, lblCat, cbCategoryFilter, lblPromo, cbPromoFilter });

            dgvProducts = new DataGridView()
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                AllowUserToAddRows = false
            };

            page.Controls.Add(dgvProducts);
            page.Controls.Add(pnlTop);

            LoadCategoriesToFilter();
            ApplyProductFilters();
        }

        private void LoadCategoriesToFilter()
        {
            string query = "SELECT category_number, category_name FROM Category ORDER BY category_name";
            DataTable dt = DbHelper.ExecuteQuery(query);

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

        private void ApplyProductFilters()
        {
            string searchText = txtSearchProduct.Text.Trim();
            int categoryId = (cbCategoryFilter.SelectedValue != null) ? (int)cbCategoryFilter.SelectedValue : 0;
            int promoIndex = cbPromoFilter.SelectedIndex;

            string query = @"SELECT sp.UPC, p.product_name AS [Назва], 
                            c.category_name AS [Категорія], 
                            sp.selling_price AS [Ціна], 
                            sp.products_number AS [К-сть],
                            CASE WHEN sp.promotional_product = 1 THEN 'Yes' ELSE 'No' END AS [Акція]
                     FROM Store_Product sp
                     JOIN Product p ON sp.id_product = p.id_product
                     JOIN Category c ON p.category_number = c.category_number
                     WHERE p.product_name LIKE @search";

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@search", "%" + searchText + "%"));

            if (categoryId > 0)
            {
                query += " AND c.category_number = @catId";
                parameters.Add(new SqlParameter("@catId", categoryId));
            }

            // Фільтр за акцією
            if (promoIndex == 1)
            {
                query += " AND sp.promotional_product = 1";
            }
            else if (promoIndex == 2)
            {
                query += " AND sp.promotional_product = 0";
            }

            query += " ORDER BY p.product_name";

            dgvProducts.DataSource = DbHelper.ExecuteQuery(query, parameters.ToArray());
        }

        private void LoadProducts(string search)
        {
            string query = @"SELECT sp.UPC, p.product_name, sp.selling_price, sp.products_number 
                             FROM Store_Product sp JOIN Product p ON sp.id_product = p.id_product 
                             WHERE p.product_name LIKE @s ORDER BY p.product_name";
            dgvProducts.DataSource = DbHelper.ExecuteQuery(query, new SqlParameter[] { new SqlParameter("@s", "%" + search + "%") });
        }

        private void SetupCustomersTab(TabPage page)
        {
            Panel pnl = new Panel() { Dock = DockStyle.Top, Height = 70, BackColor = Color.WhiteSmoke };

            Label lblSearch = new Label()
            {
                Text = "Пошук за прізвищем:",
                Location = new Point(20, 10),
                AutoSize = true
            };

            txtSearchCustomer = new TextBox()
            {
                Location = new Point(20, 30),
                Width = 200
            };
            txtSearchCustomer.TextChanged += (s, e) => LoadCustomers(txtSearchCustomer.Text);

            Button btnAddCust = new Button()
            {
                Text = "Додати клієнта",
                Location = new Point(240, 27),
                Size = new Size(130, 28),
                BackColor = Color.LightGreen,
                FlatStyle = FlatStyle.Flat
            };
            btnAddCust.Click += (s, e) =>
    {
        if (new AddCustomerForm().ShowDialog() == DialogResult.OK)
            LoadCustomers("");
    }
            ;

            Button btnEditCust = new Button()
            {
                Text = "Редагувати клієнта",
                Location = new Point(380, 27),
                Size = new Size(150, 28),
                BackColor = Color.LightSkyBlue,
                FlatStyle = FlatStyle.Flat
            };
            btnEditCust.Click += (s, e) =>
    {
        if (dgvCustomers.SelectedRows.Count == 0)
        {
            MessageBox.Show("Оберіть клієнта зі списку для редагування.",
                "Підказка", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var cardCell = dgvCustomers.SelectedRows[0].Cells["Номер карти"].Value;
        if (cardCell == null || cardCell == DBNull.Value) return;

        string cardNumber = cardCell.ToString();
        var form = new EditCustomerForm(cardNumber);
        if (form.ShowDialog() == DialogResult.OK)
            LoadCustomers(txtSearchCustomer.Text);
    }
            ;

            pnl.Controls.AddRange(new Control[] { lblSearch, txtSearchCustomer, btnAddCust, btnEditCust });

            dgvCustomers = new DataGridView()
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White
            };

            page.Controls.Add(dgvCustomers);
            page.Controls.Add(pnl);

            LoadCustomers("");
        }

        private void LoadCustomers(string surname)
        {
            string query = @"SELECT 
                        card_number AS [Номер карти], 
                        cust_surname AS [Прізвище], 
                        cust_name AS [Ім'я], 
                        cust_patronymic AS [По батькові], 
                        phone_number AS [Телефон], 
                        perthent AS [Знижка %] 
                     FROM Customer_Card 
                     WHERE cust_surname LIKE @s 
                     ORDER BY cust_surname";

            SqlParameter[] parameters = {
                new SqlParameter("@s", "%" + surname + "%")
            };

            dgvCustomers.DataSource = DbHelper.ExecuteQuery(query, parameters);
        }


        private void btnFinishCheck_Click(object sender, EventArgs e)
        {
            if (cartTable.Rows.Count == 0) return;

            string cardNumber = txtCustomerCard.Text.Trim();
            object dbCardNumber = DBNull.Value;

            if (!string.IsNullOrEmpty(cardNumber))
            {
                DataTable cardCheck = DbHelper.ExecuteQuery(
                    "SELECT card_number FROM Customer_Card WHERE card_number = @c",
                    new SqlParameter[] { new SqlParameter("@c", cardNumber) }
                );

                if (cardCheck.Rows.Count > 0)
                {
                    dbCardNumber = cardNumber;
                }
                else
                {
                    MessageBox.Show("Помилка: Карти з таким номером не існує в базі клієнтів!", "Помилка карти", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            string checkNum = GenerateCheckNumber();
            decimal total = GetCurrentTotal();
            decimal vat = total * 0.2m;

            string qCheck = "INSERT INTO [Check] (check_number, id_employee, card_number, print_date, sum_total, vat) " +
                            "VALUES (@num, @emp, @card, @date, @total, @vat)";

            SqlParameter[] pCheck = {
                new SqlParameter("@num", checkNum),
                new SqlParameter("@emp", cashierId),
                new SqlParameter("@card", dbCardNumber),
                new SqlParameter("@date", DateTime.Now),
                new SqlParameter("@total", total),
                new SqlParameter("@vat", vat)
            };

            if (DbHelper.ExecuteNonQuery(qCheck, pCheck))
            {
                foreach (DataRow row in cartTable.Rows)
                {
                    string upc = row["UPC"].ToString();
                    int qty = Convert.ToInt32(row["Кількість"]);
                    decimal price = Convert.ToDecimal(row["Ціна"]);

                    DbHelper.ExecuteNonQuery("INSERT INTO Sale (UPC, check_number, product_number, selling_price) VALUES (@u, @c, @q, @p)",
                        new SqlParameter[] {
                            new SqlParameter("@u", upc), new SqlParameter("@c", checkNum),
                            new SqlParameter("@q", qty), new SqlParameter("@p", price)
                        });

                    DbHelper.ExecuteNonQuery("UPDATE Store_Product SET products_number = products_number - @q WHERE UPC = @u",
                        new SqlParameter[] { new SqlParameter("@q", qty), new SqlParameter("@u", upc) });
                }

                MessageBox.Show($"Чек №{checkNum} успішно створено!");
                cartTable.Clear();
                txtCustomerCard.Clear();
                RecalculateTotal();
            }
        }

        private string GenerateCheckNumber() { return DateTime.Now.ToString("HHmmssff"); }

        private decimal GetCurrentTotal()
        {
            string t = lblTotal.Text.Replace("РАЗОМ: ", "").Replace(" грн", "");
            return decimal.Parse(t);
        }

        private void SetupChecksTab(TabPage page)
        {
            Panel pnlFilter = new Panel() { Dock = DockStyle.Top, Height = 60, BackColor = Color.WhiteSmoke };

            Label lblFrom = new Label() { Text = "З:", Location = new Point(10, 20), Width = 20 };
            dtpStart = new DateTimePicker() { Location = new Point(35, 18), Width = 120, Format = DateTimePickerFormat.Short };
            dtpStart.Value = DateTime.Today;

            Label lblTo = new Label() { Text = "По:", Location = new Point(170, 20), Width = 25 };
            dtpEnd = new DateTimePicker() { Location = new Point(200, 18), Width = 120, Format = DateTimePickerFormat.Short };

            Button btnSearch = new Button()
            {
                Text = "Показати чеки",
                Location = new Point(340, 15),
                Size = new Size(120, 30),
                BackColor = Color.LightSkyBlue,
                FlatStyle = FlatStyle.Flat
            };
            btnSearch.Click += (s, e) => LoadChecksForPeriod();

            pnlFilter.Controls.AddRange(new Control[] { lblFrom, dtpStart, lblTo, dtpEnd, btnSearch });

            SplitContainer splitContainer = new SplitContainer()
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 250
            };

            dgvChecksList = new DataGridView()
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };
            dgvChecksList.SelectionChanged += DgvChecksList_SelectionChanged;

            dgvCheckDetails = new DataGridView()
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.GhostWhite
            };

            GroupBox grpDetails = new GroupBox() { Text = "Склад обраного чека (Товари)", Dock = DockStyle.Fill };
            grpDetails.Controls.Add(dgvCheckDetails);

            splitContainer.Panel1.Controls.Add(dgvChecksList);
            splitContainer.Panel2.Controls.Add(grpDetails);

            page.Controls.Add(splitContainer);
            page.Controls.Add(pnlFilter);

            LoadChecksForPeriod();
        }

        private void LoadChecksForPeriod()
        {
            DateTime startDate = dtpStart.Value.Date;
            DateTime endDate = dtpEnd.Value.Date.AddDays(1).AddSeconds(-1);

            string query = @"SELECT check_number AS [№ Чека], print_date AS [Дата/Час], 
                            sum_total AS [Загальна сума], vat AS [ПДВ (20%)]
                     FROM [Check]
                     WHERE id_employee = @cashierId 
                     AND print_date BETWEEN @start AND @end
                     ORDER BY print_date DESC";

            SqlParameter[] parameters = {
        new SqlParameter("@cashierId", cashierId),
        new SqlParameter("@start", startDate),
        new SqlParameter("@end", endDate)
    };

            dgvChecksList.DataSource = DbHelper.ExecuteQuery(query, parameters);

            if (dgvCheckDetails != null) dgvCheckDetails.DataSource = null;
        }

        private void DgvChecksList_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvChecksList.SelectedRows.Count > 0)
            {
                string checkNumber = dgvChecksList.SelectedRows[0].Cells["№ Чека"].Value.ToString();

                string query = @"SELECT p.product_name AS [Назва товару], 
                                s.product_number AS [Кількість], 
                                s.selling_price AS [Ціна за од.],
                                (s.product_number * s.selling_price) AS [Сума]
                         FROM Sale s
                         JOIN Store_Product sp ON s.UPC = sp.UPC
                         JOIN Product p ON sp.id_product = p.id_product
                         WHERE s.check_number = @checkNum";

                SqlParameter[] parameters = {
            new SqlParameter("@checkNum", checkNumber)
        };

                dgvCheckDetails.DataSource = DbHelper.ExecuteQuery(query, parameters);
            }
        }


        private void SetupMeTab(TabPage page)
        {
            page.BackColor = Color.White;

            string query = "SELECT * FROM Employee WHERE id_employee = @id";
            DataTable dt = DbHelper.ExecuteQuery(query, new SqlParameter[] { new SqlParameter("@id", cashierId) });

            if (dt.Rows.Count > 0)
            {
                DataRow r = dt.Rows[0];

                Panel card = new Panel()
                {
                    Location = new Point(50, 50),
                    Size = new Size(500, 450),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.AliceBlue
                };

                Label lblHeader = new Label()
                {
                    Text = "ОСОБИСТА КАРТКА ПРАЦІВНИКА",
                    Font = new Font("Segoe UI", 14, FontStyle.Bold),
                    Location = new Point(20, 20),
                    Size = new Size(400, 30)
                };

                int y = 70;
                Action<string, string> addInfo = (label, value) =>
                {
                    card.Controls.Add(new Label() { Text = label, Location = new Point(20, y), Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = true });
                    card.Controls.Add(new Label() { Text = value, Location = new Point(180, y), Font = new Font("Segoe UI", 10), AutoSize = true });
                    y += 30;
                }
                ;

                addInfo("ID Працівника:", r["id_employee"].ToString());
                addInfo("Прізвище:", r["empl_surname"].ToString());
                addInfo("Ім'я:", r["empl_name"].ToString());
                addInfo("Посада:", r["role"].ToString());
                addInfo("Зарплата:", r["salary"].ToString() + " грн");
                addInfo("Дата народження:", Convert.ToDateTime(r["date_of_birth"]).ToShortDateString());
                addInfo("Дата початку:", Convert.ToDateTime(r["date_of_start"]).ToShortDateString());
                addInfo("Телефон:", r["phone_number"].ToString());
                addInfo("Адреса:", $"{r["city"]}, {r["street"]}, {r["zip_code"]}");

                y += 20;
                Label lblStat = new Label() { Text = "Статистика за сьогодні:", Font = new Font("Segoe UI", 11, FontStyle.Underline), Location = new Point(20, y), Size = new Size(200, 25) };
                y += 35;

                string statQuery = "SELECT COUNT(*) as cnt, SUM(sum_total) as total FROM [Check] WHERE id_employee = @id AND CAST(print_date AS DATE) = CAST(GETDATE() AS DATE)";
                DataTable dtStat = DbHelper.ExecuteQuery(statQuery, new SqlParameter[] { new SqlParameter("@id", cashierId) });

                string checksCount = dtStat.Rows[0]["cnt"].ToString();
                string totalSum = dtStat.Rows[0]["total"] != DBNull.Value ? dtStat.Rows[0]["total"].ToString() : "0";

                addInfo("Оформлено чеків:", checksCount);
                addInfo("Загальна сума:", totalSum + " грн");

                card.Controls.Add(lblHeader);
                card.Controls.Add(lblStat);
                page.Controls.Add(card);
            }
        }
    }
}