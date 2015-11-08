using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace AutoRename
{
    public class AutoRename : Form
    {
        //===================================================================== CONSTANTS
        private readonly string DEFAULT_DIR = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        private const int MARGIN = 10;

        //===================================================================== VARIABLES
        private GroupBox _grpFormat = new GroupBox();
        private RegexTextBox _txtOldFormat = new RegexTextBox();
        private RegexTextBox _txtNewFormat = new RegexTextBox();
        private ComboBox _comStoredRegex = new ComboBox();

        private ListView _lstLog = new ListView();

        private PathTextBox _txtDirectory = new PathTextBox();

        private Button _btnRename = new Button();

        private List<RegexPair> _storedRegex = new List<RegexPair>();
        private int _previousRenamed = 0;
        private int _tmpInt; // used for addition support

        //===================================================================== INITIALIZE
        public AutoRename()
        {
            this.ClientSize = new Size(550, 400);

            // format
            _grpFormat.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            _grpFormat.Location = new Point(MARGIN, MARGIN);
            _grpFormat.Width = ClientSize.Width - MARGIN * 2;
            _grpFormat.Text = "Format";
            _grpFormat.Controls.Add(_txtOldFormat);
            _grpFormat.Controls.Add(_txtNewFormat);
            _grpFormat.Controls.Add(_comStoredRegex);
            _grpFormat.Paint += grpFormat_Paint;
            _txtOldFormat.Width = _txtNewFormat.Width = _comStoredRegex.Width = 160;
            _txtOldFormat.Location = new Point(6, 20 + this.Font.Height + 6);
            _txtOldFormat.TextChanged += txtFormat_TextChanged;
            _txtNewFormat.Location = new Point(_txtOldFormat.Right + 6, _txtOldFormat.Top);
            _txtNewFormat.TextChanged += txtFormat_TextChanged;
            _comStoredRegex.DropDownStyle = ComboBoxStyle.DropDownList;
            _comStoredRegex.Location = new Point(_txtNewFormat.Right + 6, _txtOldFormat.Top);
            _comStoredRegex.SelectedIndexChanged += comStoredRegex_SelectedIndexChanged;
            _grpFormat.Height = _txtOldFormat.Bottom + 20;

            // list
            _lstLog.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
            _lstLog.Columns.Add("Current Name", 250);
            _lstLog.Columns.Add("New Name", 250);
            _lstLog.Font = SystemFonts.IconTitleFont;
            _lstLog.FullRowSelect = true;
            _lstLog.GridLines = true;
            _lstLog.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            _lstLog.Location = new System.Drawing.Point(MARGIN, _grpFormat.Bottom + 6);
            _lstLog.MultiSelect = false;
            _lstLog.Size = new System.Drawing.Size(ClientSize.Width - MARGIN * 2, 200);
            _lstLog.View = View.Details;
            _lstLog.KeyDown += lstLog_KeyDown;

            // rename
            GroupBox grpRename = new GroupBox();
            _btnRename.Enabled = false;
            _btnRename.Location = new Point(6, 20);
            _btnRename.Text = "Rename";
            _btnRename.Click += btnRename_Click;
            grpRename.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            grpRename.Size = new Size(_btnRename.Width + 6 * 2, _btnRename.Bottom + 20);
            grpRename.Location = new Point(ClientSize.Width - grpRename.Width - MARGIN, _lstLog.Bottom + this.Font.Height + 6 + 6);
            grpRename.Controls.Add(_btnRename);

            // directory
            GroupBox grpDirectory = new GroupBox();
            Button btnBrowse = new Button();
            grpDirectory.Anchor = AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;
            grpDirectory.Location = new Point(MARGIN, grpRename.Top);
            grpDirectory.Size = new Size(grpRename.Left - 6 - MARGIN, grpRename.Height);
            grpDirectory.Text = "Directory";
            grpDirectory.Controls.Add(_txtDirectory);
            grpDirectory.Controls.Add(btnBrowse);
            btnBrowse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBrowse.Size = new Size(30, _txtDirectory.Height);
            btnBrowse.Location = new Point(grpDirectory.Width - btnBrowse.Width - 6 - 6, 20);
            btnBrowse.Text = "...";
            btnBrowse.Click += btnBrowse_Click;
            _txtDirectory.DirectoryChanged += txtDirectory_DirectoryChanged;
            _txtDirectory.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            _txtDirectory.Location = new Point(6, btnBrowse.Top);
            _txtDirectory.Text = DEFAULT_DIR; // Set default directory: desktop
            _txtDirectory.Width = btnBrowse.Left - 6 - 6;

            this.ClientSize = new Size(ClientSize.Width, grpRename.Bottom + MARGIN);
            this.DoubleBuffered = true;
            this.Icon = new Icon(Assembly.GetCallingAssembly().GetManifestResourceStream("AutoRename.Icon.ico"));
            this.KeyPreview = true;
            this.MinimumSize = this.Size;
            this.Text = "Auto Rename";
            this.Controls.Add(_grpFormat);
            this.Controls.Add(_lstLog);
            this.Controls.Add(grpDirectory);
            this.Controls.Add(grpRename);

            InitializeStoredRegex();
        }
        private void InitializeStoredRegex()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + @"\regex.txt";
            if (File.Exists(path))
            {
                string[] data = File.ReadAllText(path).Split(new string[] { "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string regex in data)
                {
                    _storedRegex.Add(new RegexPair(regex.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)));
                    _comStoredRegex.Items.Add(_storedRegex[_storedRegex.Count - 1].Name);
                }
            }
        }

        //===================================================================== FUNCTIONS
        private string GetCurrentFileName(string str)
        {
            return Path.GetFileNameWithoutExtension(str);
        }
        private string GetExtension(string str)
        {
            return Path.GetExtension(str);
        }

        private void RenameFile(string oldPath, string newPath)
        {
            if (!File.Exists(oldPath)) throw new Exception(string.Format("Cannot rename \"{0}\"\r\nFile not found", GetCurrentFileName(oldPath)));
            else if (File.Exists(newPath)) throw new Exception(string.Format("Cannot rename \"{0}\" to \"{1}\"\r\n\"{1}\" already exists", GetCurrentFileName(oldPath), GetCurrentFileName(newPath)));
            else File.Move(oldPath, newPath);
        }

        private string GetNewFileName(string fileName, string oldPattern, string newPattern)
        {
            string newFileName; // holds new filename
            switch (newPattern)
            {
                case "$UPPER": newFileName = Regex.Replace(fileName, oldPattern, UpperCase); break;
                case "$LOWER": newFileName = Regex.Replace(fileName, oldPattern, LowerCase); break;
                default: newFileName = Regex.Replace(fileName, oldPattern, newPattern); break;
            }

            // temp addition support
            if (Regex.IsMatch(newPattern, @"\A\$ADD\d+\Z"))
            {
                _tmpInt = int.Parse(newPattern.Substring(4));
                newFileName = Regex.Replace(fileName, oldPattern, Add);
            }

            // only return renamable if filename is valid
            if (fileName == newFileName) return string.Empty;
            else if (newFileName.Length == 0) return string.Empty;
            else return newFileName;
        }
        private string UpperCase(Match m)
        {
            return m.Value.ToUpper();
        }
        private string LowerCase(Match m)
        {
            return m.Value.ToLower();
        }
        private string Add(Match m)
        {
            if (Regex.IsMatch(m.Value, @"\A\d+\Z"))
                return (int.Parse(m.Value) + _tmpInt).ToString().PadLeft(3, '0');
            else
                return m.Value;
        }

        private void GenerateLogList()
        {
            _lstLog.Items.Clear();

            foreach (string item in Directory.GetFiles(_txtDirectory.Text))
            {
                string fileName = GetCurrentFileName(item);
                string extension = GetExtension(item);
                _lstLog.Items.Add(new ListViewItem(new string[] { fileName + extension, string.Empty }));
            }
        }
        private void UpdateLogList()
        {
            bool isValid = (_txtOldFormat.IsValid && _txtNewFormat.IsValid);
            foreach (ListViewItem item in _lstLog.Items)
            {
                if (isValid)
                {
                    string fileName = GetCurrentFileName(item.SubItems[0].Text);
                    string extension = GetExtension(item.SubItems[0].Text);
                    string newFileName = GetNewFileName(fileName, _txtOldFormat.Text, _txtNewFormat.Text);
                    item.ForeColor = (newFileName.Length > 0 ? Color.RoyalBlue : Color.IndianRed);
                    item.SubItems[1].Text = (newFileName.Length > 0 ? newFileName + extension : newFileName);
                }
                else
                {
                    item.ForeColor = Color.IndianRed;
                    item.SubItems[1].Text = string.Empty;
                }
            }
            _btnRename.Enabled = isValid;
        }

        //===================================================================== EVENTS
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawString(string.Format("Renamed {0} files on previous run", _previousRenamed), this.Font, Brushes.Black, MARGIN, _lstLog.Bottom + 6);
            base.OnPaint(e);
        }
        private void grpFormat_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawString("Old Format:", this.Font, Brushes.Black, _txtOldFormat.Left, 20);
            e.Graphics.DrawString("New Format:", this.Font, Brushes.Black, _txtNewFormat.Left, 20);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            _txtOldFormat.Width = _txtNewFormat.Width = (_grpFormat.Width - 18) / 3;
            _txtNewFormat.Left = _txtOldFormat.Right + 6;
            _comStoredRegex.Left = _txtNewFormat.Right + 6;
            foreach (ColumnHeader column in _lstLog.Columns) column.Width = (ClientSize.Width - 50) / 2;

            _grpFormat.Invalidate();
            this.Invalidate();
        }

        private void comStoredRegex_SelectedIndexChanged(object sender, EventArgs e)
        {
            RegexPair pair = _storedRegex[_comStoredRegex.SelectedIndex];
            _txtOldFormat.Text = pair.OldFormat;
            _txtNewFormat.Text = pair.NewFormat;
        }
        private void txtFormat_TextChanged(object sender, EventArgs e)
        {
            UpdateLogList();
        }

        private void lstLog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
            {
                if (_lstLog.SelectedItems.Count > 0)
                    Clipboard.SetText(_lstLog.SelectedItems[0].SubItems[0].Text);
            }
        }

        private void txtDirectory_DirectoryChanged(object sender, EventArgs e)
        {
            GenerateLogList();
            UpdateLogList();
        }
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select Directory:";
                dialog.ShowNewFolderButton = false; // disable new folder creation
                dialog.SelectedPath = _txtDirectory.Text; // set last selected path in dialog
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    _txtDirectory.Text = dialog.SelectedPath; // set new directory
            }
        }

        private void btnRename_Click(object sender, EventArgs e)
        {
            _previousRenamed = 0;
            foreach (ListViewItem item in _lstLog.Items)
            {
                if (item.SubItems[1].Text != string.Empty)
                {
                    string oldPath = _txtDirectory.Text + "\\" + item.SubItems[0].Text;
                    string newPath = _txtDirectory.Text + "\\" + item.SubItems[1].Text;
                    try
                    {
                        RenameFile(oldPath, newPath);
                        _previousRenamed++;

                        // update list
                        item.ForeColor = Color.MediumSeaGreen;
                        item.SubItems[0].Text = item.SubItems[1].Text;
                        item.SubItems[1].Text = string.Empty;
                    }
                    catch (Exception ex)
                    {
                        item.ForeColor = Color.Maroon;
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            _btnRename.Enabled = false;
            this.Invalidate();
        }
    }
}
