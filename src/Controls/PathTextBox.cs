using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace AutoRename
{
    public class PathTextBox : TextBox
    {
        //===================================================================== EVENTS
        public event EventHandler DirectoryChanged;

        //===================================================================== VARIABLES
        private string _oldPath;

        //===================================================================== FUNCTIONS
        private bool IsValidDirectory(string path)
        {
            if (!Directory.Exists(path)) return false;
            if (new DirectoryInfo(path).Attributes.HasFlag(FileAttributes.System)) return false;
            return true;
        }

        //===================================================================== PROPERTIES
        public override string Text
        {
            get { return base.Text; }
            set
            {
                if (IsValidDirectory(value))
                {
                    base.Text = value;
                    this.SelectionStart = this.TextLength;
                    if (DirectoryChanged != null) DirectoryChanged(this, new EventArgs());
                }
            }
        }

        //===================================================================== EVENTS
        protected override void OnTextChanged(EventArgs e)
        {
            if (IsValidDirectory(this.Text))
            {
                _oldPath = this.Text;
                if (DirectoryChanged != null) DirectoryChanged(this, new EventArgs());
            }
            base.OnTextChanged(e);
        }
        protected override void OnEnter(EventArgs e)
        {
            _oldPath = this.Text;
            base.OnEnter(e);
        }
        protected override void OnValidated(EventArgs e)
        {
            if (!IsValidDirectory(this.Text)) this.Text = _oldPath;
            base.OnValidated(e);
        }
    }
}
