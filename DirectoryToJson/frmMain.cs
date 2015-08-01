using System;
using System.IO;
using System.Windows.Forms;


namespace DirectoryToJson
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnProcess_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                string fullPath = Path.GetFullPath(txtPath.Text);
                Directories d = new Directories(fullPath, txtFilters.Text, rdbSuppressEmpty.Checked, rdbWriteChildren.Checked );
                txtJSON.Text = d.GetJsonPretty();
                File.WriteAllText(Path.Combine(fullPath, "directory.json"), d.GetJson());
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
            

        }
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                txtPath.Text = folderBrowser.SelectedPath;
            }
        }
    }
}
