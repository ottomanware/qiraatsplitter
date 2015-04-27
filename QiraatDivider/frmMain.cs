using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Xml;
using System.IO; 

namespace QiraatDivider
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }
        
        private void LoadLanguage()
        {
            lblFile.Text = Helper.GetString("FILE_NAME");
            lblLang.Text = Helper.GetString("LANGUAGE");
            btnChoose.Text = Helper.GetString("CHOOSE");
            lblSpeed.Text = Helper.GetString("SPEED");
            lblSuraNo.Text = Helper.GetString("SURA_NO");
            btnPut.Text = Helper.GetString("PUT_MARK");
            btnGo.Text = Helper.GetString("MOVE_TO_SELECTED_POSITION");
            btnDelete.Text = Helper.GetString("DELETE_SELECTED");
            btnClear.Text = Helper.GetString("CLEAR_MARKS");
            btnImport.Text = Helper.GetString("IMPORT_MARKS");
            btnExport.Text = Helper.GetString("EXPORT_MARKS");
            btnCue.Text = Helper.GetString("EXPORT_CUE");
            this.Text = Helper.GetString("TITLE");
            od.Filter = Helper.GetString("MP3_FILES");
            sd.Filter = Helper.GetString("XML_FILES");
            odXml.Filter = Helper.GetString("XML_FILES");
            sdCue.Filter = Helper.GetString("CUE_FILES");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ComboBoxItem ci;
            ci = new ComboBoxItem("English", "en-US");
            cmbLang.Items.Add(ci);
            ci = new ComboBoxItem("Türkçe", "tr-TR");
            cmbLang.Items.Add(ci);
            ci = new ComboBoxItem("Polski", "pl-PL");
            cmbLang.Items.Add(ci);

            string culture = Properties.Settings.Default.Language;
            if (culture == "") culture = "en-US";

            foreach (ComboBoxItem c in cmbLang.Items)
            {
                if (String.Compare(c.Value.ToString(), culture) == 0)
                {
                    cmbLang.SelectedItem = c;
                    break;
                }
            }

            KeyboardListener.s_KeyEventHandler += new EventHandler(KeyboardListener_s_KeyEventHandler);
            lstDivisions.Items.Clear();
        }

        private void KeyboardListener_s_KeyEventHandler(object sender, EventArgs e)
        {
            KeyboardListener.UniversalKeyEventArgs eventArgs = (KeyboardListener.UniversalKeyEventArgs)e;

            if (eventArgs.m_Msg == 256)
            {
                if (eventArgs.KeyCode == Keys.F10)
                {
                    PutMark();
                }
            }
        }

        private void btnPut_Click(object sender, EventArgs e)
        {
            PutMark();
        }

        private void PutMark()
        {
            Item i = new Item();
            i.Text = (lstDivisions.Items.Count + 1).ToString() + ") " +  mp.Ctlcontrols.currentPositionString;
            i.Tag = mp.Ctlcontrols.currentPosition.ToString();

            lstDivisions.Items.Add(i);
        }

        private void btnChoose_Click(object sender, EventArgs e)
        {
            DialogResult d = od.ShowDialog();

            if (d == System.Windows.Forms.DialogResult.OK)
            {
                txtFileName.Text = od.FileName;
                mp.Ctlcontrols.stop();
                mp.URL = od.FileName;
                mp.Ctlcontrols.stop();
                lstDivisions.Items.Clear();
            }
        }

        private void tt_Tick(object sender, EventArgs e)
        {
            lblTime.Text = mp.Ctlcontrols.currentPositionString;
            try
            {
                double d = Math.Floor(mp.currentMedia.duration - mp.Ctlcontrols.currentPosition);
                int sec = (int)(d % 60);
                int min = (int)((d-sec) / 60);

                lblTime2.Text = min.ToString("D2") + ":" + sec.ToString("D2");
            }
            catch
            {
                lblTime2.Text = "";
            }
        }

        private void tb_Scroll(object sender, EventArgs e)
        {
            double r = 1.0;

            r = 0.4d + (((double)tb.Value) / 10d);

            mp.settings.rate = r;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(Helper.GetString("ARE_YOU_SURE"), Helper.GetString("CONFIRM"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.No) return;
            DeleteSelectedMarks();
        }

        private void DeleteSelectedMarks()
        {
            for (int i = lstDivisions.SelectedItems.Count - 1; i >= 0; i--)
            {
                Item itm = (Item)lstDivisions.SelectedItems[i];

                for (int j = 0; j < lstDivisions.Items.Count; j++)
                {
                    Item itm2 = (Item)lstDivisions.Items[j];
                    if (itm.Text == itm2.Text && itm.Tag == itm2.Tag)
                    {
                        lstDivisions.Items.Remove(lstDivisions.Items[j]);
                        break;
                    }
                }
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (txtFileName.Text == "") return;

            DialogResult dr = sd.ShowDialog();

            if (dr == System.Windows.Forms.DialogResult.Cancel) return;

            try
            {

                string FileName = Path.GetFileName(txtFileName.Text);

                FileName = txtFileName.Text;

                XmlDocument d = new XmlDocument();
                XmlNode rootnode = d.CreateNode(XmlNodeType.Element, "Sura", "");
                XmlAttribute a = d.CreateAttribute("FileName");
                a.Value = FileName;
                rootnode.Attributes.Append(a);
                d.AppendChild(rootnode);

                for (int i = 0; i < lstDivisions.Items.Count; i++)
                {
                    Item itm = (Item)lstDivisions.Items[i];
                    string txt = itm.Text.Substring(itm.Text.IndexOf(")") + 2);
                    XmlNode divNode = d.CreateNode(XmlNodeType.Element, "Stop", "");
                    XmlAttribute a1 = d.CreateAttribute("TimeText");
                    a1.Value = txt;
                    XmlAttribute a2 = d.CreateAttribute("TimeValue");
                    a2.Value = itm.Tag.ToString();
                    divNode.Attributes.Append(a1);
                    divNode.Attributes.Append(a2);
                    rootnode.AppendChild(divNode);

                }

                string xml = "<?xml version=\"1.0\"?>\n" + d.OuterXml;


                System.IO.StreamWriter file = new System.IO.StreamWriter(sd.FileName);
                file.WriteLine(xml);

                file.Close();

                MessageBox.Show(Helper.GetString("SAVE_COMPLETED"), Helper.GetString("STATUS"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
                MessageBox.Show(Helper.GetString("ERROR_OCCURRED"), Helper.GetString("ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            DialogResult dr = odXml.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.Cancel) return;

            try
            {
                System.IO.StreamReader file = new System.IO.StreamReader(odXml.FileName);

                string xml = file.ReadToEnd();

                XmlDocument d = new XmlDocument();
                d.LoadXml(xml);

                txtFileName.Text = d.ChildNodes[1].Attributes["FileName"].Value;

                lstDivisions.Items.Clear();

                Application.DoEvents();

                for (int x = 0; x < d.ChildNodes[1].ChildNodes.Count; x++)
                {
                    XmlNode n = d.ChildNodes[1].ChildNodes[x];
                    if (n.NodeType == XmlNodeType.Element && n.Attributes["TimeText"].Value != "")
                    {
                        Item i = new Item();
                        i.Text = (lstDivisions.Items.Count + 1).ToString() + ") " + n.Attributes["TimeText"].Value;
                        i.Tag = n.Attributes["TimeValue"].Value;

                        lstDivisions.Items.Add(i);
                    }
                }

                Item itm = (Item)lstDivisions.Items[lstDivisions.Items.Count - 1];
                double pos = Convert.ToDouble(itm.Tag);

                mp.Ctlcontrols.stop();
                mp.URL = txtFileName.Text;
                mp.Ctlcontrols.stop();
                mp.Ctlcontrols.currentPosition = pos;

                MessageBox.Show(Helper.GetString("LOAD_COMPLETED"), Helper.GetString("STATUS"), MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch
            {
                MessageBox.Show(Helper.GetString("ERROR_OCCURRED"), Helper.GetString("ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                lstDivisions.Items.Clear();
            }
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            Item itm = (Item)lstDivisions.SelectedItem;
            if (itm == null) return;

            int idx = lstDivisions.SelectedIndex;

            for (int i = lstDivisions.Items.Count - 1; i >= idx; i--)
                lstDivisions.Items.Remove(lstDivisions.Items[i]);

            double pos = Convert.ToDouble(itm.Tag);
            mp.Ctlcontrols.stop();
            mp.Ctlcontrols.currentPosition = pos;
        }


        private void btnCue_Click(object sender, EventArgs e)
        {
            if (txtSuraNo.Text == "")
            {
                MessageBox.Show(Helper.GetString("ENTER_SURA_NO"), Helper.GetString("ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtSuraNo.Focus();
                return;
            }
            DialogResult d = sdCue.ShowDialog();
            if (d == System.Windows.Forms.DialogResult.Cancel) return;

            try
            {

                string FName = sdCue.FileName;
                string mp3name = "";

                string cuetext = "FILE \"" + Path.GetFileName(txtFileName.Text) + "\" BINARY\n";

                string start = "00:00:00";
                string end = "";

                for (int i = 0; i < lstDivisions.Items.Count; i++)
                {
                    Item itm = (Item)lstDivisions.Items[i];

                    double x = Convert.ToDouble(itm.Tag);

                    long l = (long)(10000000 * x);

                    TimeSpan t = new TimeSpan(l);
                    decimal dm = (decimal)t.Milliseconds;

                    dm = Math.Round(dm * (decimal)0.075);

                    end = string.Format("{0:00}:{1:00}:{2:00}", t.Minutes, t.Seconds, (int)dm);

                    mp3name = string.Format("{0:000}{1:000}", Convert.ToInt32(txtSuraNo.Text), i);

                    DateTime dt = new DateTime(2012, 01, 01);

                    DateTime dateValue = dt + t;


                    cuetext += String.Format("TRACK {0:00} AUDIO\n\tTITLE \"{1}\"\n\tINDEX 01 {2}\n", i + 1, mp3name, start);

                    start = end;
                }

                mp3name = string.Format("{0:000}{1:000}", Convert.ToInt32(txtSuraNo.Text), lstDivisions.Items.Count);

                cuetext += String.Format("TRACK {0:00} AUDIO\n\tTITLE \"{1}\"\n\tINDEX 01 {2}\n", lstDivisions.Items.Count + 1, mp3name, start);

                System.IO.StreamWriter file = new System.IO.StreamWriter(FName);
                file.WriteLine(cuetext);

                file.Close();

                MessageBox.Show(Helper.GetString("EXPORT_COMPLETED"), Helper.GetString("STATUS"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
                MessageBox.Show(Helper.GetString("ERROR_OCCURRED"), Helper.GetString("ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(Helper.GetString("ARE_YOU_SURE"), Helper.GetString("CONFIRM"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.No) return;
            lstDivisions.Items.Clear();
        }


        private void lstDivisions_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Delete)
            {
                DeleteSelectedMarks();
            }
        }

        private void cmbLang_SelectedIndexChanged(object sender, EventArgs e)
        {
            string culturename = (cmbLang.SelectedItem as ComboBoxItem).Value.ToString();
            Properties.Settings.Default.Language = culturename;
            Helper.ChangeCulture(culturename);
            LoadLanguage();
            Properties.Settings.Default.Save();
        }

        private void txtSuraNo_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
            try
            {
                int x = Convert.ToInt32(txtSuraNo.Text);
                if (x < 0 || x > 114)
                    txtSuraNo.Text = "";
            }
            catch
            {
                txtSuraNo.Text = "";
            }
        }

        private void txtSuraNo_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int x = Convert.ToInt32(txtSuraNo.Text);
                if (x <0 || x > 114)
                    txtSuraNo.Text = "";
            }
            catch
            {
                txtSuraNo.Text = "";
            }
        }

        private void mp_MediaChange(object sender, AxWMPLib._WMPOCXEvents_MediaChangeEvent e)
        {
            double r = 1.0;

            r = 0.4d + (((double)tb.Value) / 10d);

            mp.settings.rate = r;
        }
    }
}
