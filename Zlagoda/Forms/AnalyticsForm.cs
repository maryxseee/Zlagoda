using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Zlagoda.Classes;

namespace Zlagoda.Forms
{
    public partial class AnalyticsForm : Form
    {
        private DataGridView dgvQuery1;
        private Button btnRunQuery1;
        private Label lblQuery1Desc;

        public AnalyticsForm()
        {
            InitializeAnalyticsComponents();
        }

        private void InitializeAnalyticsComponents()
        {
            this.Text = "ZLAGODA – Аналітичні звіти";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.WhiteSmoke;

            Label lblTitle = new Label()
            {
                Text = "Запити",
                Dock = DockStyle.Top,
                Height = 45,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White
            };

            TabControl tabControl = new TabControl()
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10)
            };

            tabControl.TabPages.Add(BuildQuery1Tab());

            this.Controls.Add(tabControl);
            this.Controls.Add(lblTitle);
        }

        private TabPage BuildQuery1Tab()
        {
            TabPage tab = new TabPage("Запит 1 – Групування");

            lblQuery1Desc = new Label()
            {
                Text = "Умова: Для кожної категорії товарів вивести кількість різних найменувань товарів,\n" +
                       "середню ціну продажу в магазині та загальну кількість одиниць на складі.\n" +
                       "Враховувати лише ті категорії, де загальний залишок на складі перевищує 0 одиниць.\n" +
                       "Результат впорядкувати за спаданням загального залишку.",
                Location = new Point(10, 10),
                Size = new Size(960, 80),
                Font = new Font("Segoe UI", 9),
                BackColor = Color.LightYellow,
                BorderStyle = BorderStyle.FixedSingle
            };

            btnRunQuery1 = new Button()
            {
                Text = "Виконати запит",
                Location = new Point(10, 100),
                Size = new Size(160, 36),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnRunQuery1.Click += BtnRunQuery1_Click;

            dgvQuery1 = new DataGridView()
            {
                Location = new Point(10, 150),
                Size = new Size(960, 420),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false
            };

            tab.Controls.AddRange(new Control[] { lblQuery1Desc, btnRunQuery1, dgvQuery1 });
            return tab;
        }

        private void BtnRunQuery1_Click(object sender, EventArgs e)
        {
            string sql = @"
                SELECT
                    c.category_name                          AS [Категорія],
                    COUNT(DISTINCT p.id_product)             AS [Кількість найменувань],
                    CAST(AVG(sp.selling_price) AS DECIMAL(10,2))  AS [Середня ціна, грн],
                    SUM(sp.products_number)                  AS [Залишок на складі, шт]
                FROM Category c
                JOIN Product p
                    ON p.category_number = c.category_number
                JOIN Store_Product sp
                    ON sp.id_product = p.id_product
                GROUP BY c.category_number, c.category_name
                HAVING SUM(sp.products_number) > 0
                ORDER BY SUM(sp.products_number) DESC";

            try
            {
                DataTable result = DbHelper.ExecuteQuery(sql);

                if (result != null && result.Rows.Count > 0)
                {
                    dgvQuery1.DataSource = result;
                    dgvQuery1.Columns["Категорія"].DefaultCellStyle.Font =
                        new Font("Segoe UI", 9, FontStyle.Bold);
                }
                else
                {
                    dgvQuery1.DataSource = null;
                    MessageBox.Show("Запит не повернув жодного рядка.", "Результат",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка виконання запиту:\n" + ex.Message,
                    "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}