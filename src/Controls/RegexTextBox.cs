using System;
using System.ComponentModel;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace AutoRename
{
    public class RegexTextBox : TextBox
    {
        //===================================================================== VARIABLES
        private ContextMenu _menu = new ContextMenu();

        //===================================================================== INITIALIZE
        public RegexTextBox()
        {
            _menu.MenuItems.Add("Word Match", menuWordMatch_Click);
            _menu.MenuItems.Add("Numeric Match", menuNumericMatch_Click);
            _menu.MenuItems.Add("Any Match", menuAnyMatch_Click);
            this.ContextMenu = _menu;
        }

        //===================================================================== TERMINATE
        protected override void Dispose(bool disposing)
        {
            _menu.Dispose();
            base.Dispose(disposing);
        }

        //===================================================================== FUNCTIONS
        private bool IsRegexValid(string pattern)
        {
            if (pattern == string.Empty) return false;
            try { Regex.Match(string.Empty, pattern); }
            catch { return false; }
            return true;
        }
        private void InsertTextAtSelection(string text)
        {
            int selStart = SelectionStart;
            this.Text = this.Text.Remove(selStart, SelectionLength);
            this.Text = this.Text.Insert(selStart, text);
            this.SelectionStart = selStart + text.Length;
        }

        //===================================================================== PROPERTIES
        public bool IsValid
        {
            get { return IsRegexValid(this.Text); }
        }

        //===================================================================== EVENTS
        protected override void OnTextChanged(EventArgs e)
        {
            this.BackColor = (IsRegexValid(this.Text) ? Color.White : Color.FromArgb(255, 210, 170));
            base.OnTextChanged(e);
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.S: menuWordMatch_Click(_menu, new EventArgs()); e.SuppressKeyPress = true; break;
                    case Keys.D: menuNumericMatch_Click(_menu, new EventArgs()); e.SuppressKeyPress = true; break;
                    case Keys.F: menuAnyMatch_Click(_menu, new EventArgs()); e.SuppressKeyPress = true; break;
                    case Keys.D1: InsertTextAtSelection("$1"); e.SuppressKeyPress = true; break;
                    case Keys.D2: InsertTextAtSelection("$2"); e.SuppressKeyPress = true; break;
                    case Keys.D3: InsertTextAtSelection("$3"); e.SuppressKeyPress = true; break;
                }
            }
            base.OnKeyDown(e);
        }

        private void menuWordMatch_Click(object sender, EventArgs e)
        {
            InsertTextAtSelection(@"(\w+)");
        }
        private void menuNumericMatch_Click(object sender, EventArgs e)
        {
            InsertTextAtSelection(@"(\d+)");
        }
        private void menuAnyMatch_Click(object sender, EventArgs e)
        {
            InsertTextAtSelection(@"(.+)");
        }
    }
}
