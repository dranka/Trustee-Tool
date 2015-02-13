using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using DCT.Parsing;
using DCT.Protocols.Http;

namespace Trustee_Tool
{
    public partial class Form1 : Form
    {
        public string server;
        public int ServerID;
        public string postdata;
        public Form1()
        {
            //Default c# command, displays all the graphical interface controls.
            InitializeComponent();
            //Quick work around for cross threaded calls to controls, since im too lazy to write the extra 3 - 4 lines of code.
            CheckForIllegalCrossThreadCalls = false;
        }

        //main function loops through all selected list view items and adds the RGA trustee for those accounts.
        internal void AddTrustee()
        {
            string input = textBox3.Text.ToLower();
            List<string> list = new List<string>(
                           input.Split(new string[] { "\r\n" },
                           StringSplitOptions.RemoveEmptyEntries));
            foreach (ListViewItem i in listView1.CheckedItems)
            {
                foreach (string a in list)
                {
                i.SubItems[2].Text = "Prepairing to add trustee";
                //loads quivers trustee page.
                string strhtml = HttpSocket.DefaultInstance.Get("http://" + server +"/trust.php?t_serv=" + ServerID);
                //Declares a new parser to go through the html data, this program uses typpos parser.
                Parser p = new Parser(strhtml);
                string security = p.Parse("form-nonce\" value=\"", "\" />");
                i.SubItems[2].Text = "Adding trustee...";
                postdata = HttpSocket.DefaultInstance.Post("http://" + server + "/trust.php", "t_serv=" + ServerID + "&t_char=" + i.SubItems[1].Text + "&add=" + a + "&form-nonce=" + security);
                if (postdata.IndexOf(a) > 0)
                {
                        i.SubItems[2].Text = "Done";
                }
                else
                {
                    i.SubItems[2].Text = "Error";
                }

                }
                if (chkPermissions.Checked == true)
                {
                UpdatePerms(i.SubItems[1].Text, postdata);
                }

            }

        }

        internal void UpdatePerms(string accountid, string postdata)
        {

            // parser new trustee to add permissions
            Parser x = new Parser(postdata);
            // getting new security token for permissions update
            string security = x.Parse("form-nonce\" value=\"", "\" />");
            //your current character id
            string charid = accountid;
            // narrow down page so its not looking through everything
            string character = x.Parse("Trustees for ", "Update Permissions");
            // new parser for just the section of page we need.
            Parser y = new Parser(character);
            // id of the rga you just trusteed.
            string post = "form-nonce=" + security + "&update=1&t_char=" + charid + "&t_serv=" + ServerID;
            foreach (string b in y.MultiParse("potion[", "]"))
            {
                if (b != "ERROR")
                {
                    post = post + "&potion%5B" + b + "%5D=1&lingbuff%5B" + b + "%5D=1&teleport%5B" + b + "%5D=1";
                }
            }

            //Post for all the data we just collected, adds all perms.
            HttpSocket.DefaultInstance.Post("http://" + server + "/trust.php", post);
        }

        //Called when the login button is pressed.
        internal void Login()
        {
            string strhtml = HttpSocket.DefaultInstance.Post("http://" + server + "/index.php", "serverid=" + ServerID + "&login_username=" + textBox1.Text + "&login_password=" + textBox2.Text + "&submitit.x=134&submitit.y=19");
            strhtml = HttpSocket.DefaultInstance.Get("http://" + server + "/trust.php?t_serv=" + ServerID);

            Parser p = new Parser(strhtml);
            string InnerHtml = p.Parse("onChange=\"changeCharacter();", "</select>");
            Parser p1 = new Parser(InnerHtml);
            foreach (string x in p1.MultiParse("<option value", "option>"))
            {
                Parser p2 = new Parser(x);
                string Accountname = p2.Parse(">", "</");
                string suid = p2.Parse("=\"", "\" ");
                if (Accountname != "ERROR")
                {
                    //Adds data to the listview of accounts based off found information in the html.
                ListViewItem lvi = listView1.Items.Add(Accountname);
                lvi.SubItems.Add(suid);
                lvi.SubItems.Add("Ready...");
                }

            }


        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (rbSigil.Checked == true)
            {
                server = "sigil.outwar.com";
                ServerID = 1;
            }
            else
            {
                server = "torax.outwar.com";
                ServerID = 2;
            }
            Thread t = new Thread(Login);
            t.IsBackground = true;
            t.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Thread t1 = new Thread(AddTrustee);
            t1.IsBackground = true;
            t1.Start();
        }

    }
}
