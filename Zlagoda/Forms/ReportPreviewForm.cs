using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace Zlagoda.Forms
{
    public class ReportPreviewForm : Form
    {
        private readonly string _title;
        private readonly string _managerSurname;
        private readonly DataTable _data;
        private readonly PrintDocument _printDoc;

        private int _currentPage;
        private int _currentRow;
        private bool _headerPrinted;

        private const int MarginLeft = 60;
        private const int MarginRight = 60;
        private const int MarginTop = 60;
        private const int MarginBottom = 60;
        private const int HeaderHeight = 36;
        private const int FooterHeight = 28;
        private const int RowHeight = 22;
        private const int ColHeaderH = 26;

        public ReportPreviewForm(string title, string managerSurname, DataTable data)
        {
            _title = title;
            _managerSurname = managerSurname;
            _data = data;

            _printDoc = new PrintDocument();
            _printDoc.DefaultPageSettings.Margins =
                new Margins(MarginLeft, MarginRight, MarginTop, MarginBottom);
            _printDoc.PrintPage += PrintDoc_PrintPage;
            _printDoc.BeginPrint += (s, e) =>
            {
                _currentPage = 1;
                _currentRow = 0;
                _headerPrinted = false;
            }
            ;

            InitUI();
        }


        private void InitUI()
        {
            this.Text = $"Попередній перегляд — {_title}";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimizeBox = false;

            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 48,
                BackColor = Color.FromArgb(45, 45, 48)
            };

            var btnPrint = MakeButton("🖨  Друкувати", 10);
            btnPrint.Click += (s, e) =>
            {
                _currentRow = 0;
                _currentPage = 1;
                _headerPrinted = false;
                _printDoc.Print();
            }
            ;

            var btnClose = MakeButton("✕  Закрити", 160);
            btnClose.Click += (s, e) => this.Close();

            toolbar.Controls.Add(btnPrint);
            toolbar.Controls.Add(btnClose);

            var preview = new PrintPreviewControl
            {
                Dock = DockStyle.Fill,
                Document = _printDoc,
                Zoom = 1.0
            };

            this.Controls.Add(preview);
            this.Controls.Add(toolbar);

            _currentRow = 0;
            _currentPage = 1;
            _headerPrinted = false;
        }

        private Button MakeButton(string text, int x)
        {
            return new Button
            {
                Text = text,
                Location = new Point(x, 8),
                Size = new Size(140, 32),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 122, 204),
                Font = new Font("Segoe UI", 9f)
            };
        }

        private void PrintDoc_PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle body = e.MarginBounds;

            var fntTitle = new Font("Segoe UI", 14f, FontStyle.Bold);
            var fntDate = new Font("Segoe UI", 8f, FontStyle.Italic);
            var fntColHdr = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            var fntCell = new Font("Segoe UI", 8f);
            var fntFooter = new Font("Segoe UI", 8f, FontStyle.Italic);

            Brush brBlack = Brushes.Black;
            Brush brHeader = new SolidBrush(Color.FromArgb(45, 45, 48));
            Brush brWhite = Brushes.White;
            Brush brAlt = new SolidBrush(Color.FromArgb(240, 240, 245));
            Pen pnBorder = new Pen(Color.FromArgb(180, 180, 180), 0.5f);

            int y = body.Top;

            var titleRect = new RectangleF(body.Left, y, body.Width, HeaderHeight);
            g.FillRectangle(brHeader, titleRect);
            using (var sfCenter = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                g.DrawString(_title, fntTitle, brWhite, titleRect, sfCenter);
            y += HeaderHeight + 4;

            string dateStr = $"Дата формування: {DateTime.Now:dd.MM.yyyy HH:mm}";
            var dateSize = g.MeasureString(dateStr, fntDate);
            g.DrawString(dateStr, fntDate, brBlack,
                body.Right - dateSize.Width, y);
            y += (int)dateSize.Height + 6;

            g.DrawLine(Pens.Gray, body.Left, y, body.Right, y);
            y += 6;

            int cols = _data.Columns.Count;
            float colWidth = (float)body.Width / cols;

            var hdrRect = new RectangleF(body.Left, y, body.Width, ColHeaderH);
            g.FillRectangle(new SolidBrush(Color.FromArgb(70, 130, 180)), hdrRect);
            g.DrawRectangle(new Pen(Color.SteelBlue), body.Left, y, body.Width, ColHeaderH);

            using (var sfCol = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter
            })
            {
                for (int c = 0; c < cols; c++)
                {
                    float cx = body.Left + c * colWidth;
                    g.DrawString(_data.Columns[c].ColumnName, fntColHdr, brWhite,
                        new RectangleF(cx + 2, y, colWidth - 4, ColHeaderH), sfCol);
                    if (c > 0)
                        g.DrawLine(new Pen(Color.LightSteelBlue),
                            cx, y, cx, y + ColHeaderH);
                }
            }
            y += ColHeaderH;

            int footerY = body.Bottom - FooterHeight - 10;
            int availableH = footerY - y;

            using (var sfCell = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter
            })
            {
                while (_currentRow < _data.Rows.Count)
                {
                    if (y + RowHeight > footerY) break;

                    DataRow row = _data.Rows[_currentRow];
                    bool isAlt = (_currentRow % 2 == 1);
                    var rowRect = new RectangleF(body.Left, y, body.Width, RowHeight);
                    g.FillRectangle(isAlt ? brAlt : Brushes.White, rowRect);

                    for (int c = 0; c < cols; c++)
                    {
                        float cx = body.Left + c * colWidth;
                        string val = row[c]?.ToString() ?? "";
                        g.DrawString(val, fntCell, brBlack,
                            new RectangleF(cx + 3, y, colWidth - 6, RowHeight), sfCell);
                        if (c > 0)
                            g.DrawLine(pnBorder, cx, y, cx, y + RowHeight);
                    }

                    g.DrawLine(pnBorder, body.Left, y + RowHeight, body.Right, y + RowHeight);
                    y += RowHeight;
                    _currentRow++;
                }
            }

            g.DrawRectangle(Pens.SteelBlue,
                body.Left,
                body.Top + HeaderHeight + 4 + (int)g.MeasureString("X", fntDate).Height + 12,
                body.Width,
                y - (body.Top + HeaderHeight + 4 + (int)g.MeasureString("X", fntDate).Height + 12));

            int fy = body.Bottom - FooterHeight;
            g.DrawLine(Pens.Gray, body.Left, fy - 4, body.Right, fy - 4);

            g.DrawString($"Менеджер: {_managerSurname}", fntFooter, brBlack,
                body.Left, fy);

            string pageStr = $"Сторінка {_currentPage}";
            var pageSize = g.MeasureString(pageStr, fntFooter);
            g.DrawString(pageStr, fntFooter, brBlack,
                body.Right - pageSize.Width, fy);

            e.HasMorePages = (_currentRow < _data.Rows.Count);
            if (e.HasMorePages) _currentPage++;

            fntTitle.Dispose(); fntDate.Dispose(); fntColHdr.Dispose();
            fntCell.Dispose(); fntFooter.Dispose();
            brHeader.Dispose(); brAlt.Dispose();
        }
    }
}
