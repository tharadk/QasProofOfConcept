using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
//using System.Threading.Tasks;
using System.ServiceModel;
using System.Windows.Forms;
//using Proxy.QAS;
using Timer = System.Threading.Timer;

namespace QasProofOfConceptx
{
    public partial class FrmQASPOC : Form
    {
        
        EngineType engine = new EngineType
        {
            Value = EngineEnumType.Intuitive,
            //Timeout = "1000",
            Intensity = EngineIntensityType.Extensive,
            Flatten = false,
            FlattenSpecified = false,
            PromptSet = PromptSetType.Alternate,
            PromptSetSpecified = true,
            IntensitySpecified = false
        };

        public string QASsearchText;
        public string GooglesearchText;
        public Timer QASsearchTimeout;
        public Timer GooglesearchTimeout;

        public FrmQASPOC()
        {
            InitializeComponent();
           

        }
        
        private void SearchViaQAS()
        {
            //QASOnDemandIntermediary qas = null;
            QAPortTypeClient qas = null;
            try
            {

               // qas = new QASOnDemandIntermediary();
                 qas = new QAPortTypeClient();
                
                var authentication = new QAAuthentication
                    {
                        Password = "98aM3d12014",
                        Username = "98a098f3-ee0"
                    };

                //            Username: ws_609_ext 
                //Password: m3d1b2NK2O14


                var header = new QAQueryHeader {QAAuthentication = authentication};
                //qas.QAQueryHeaderValue = header;
                //WebProxy proxy = (WebProxy)WebProxy.GetDefaultProxy();

                //if (proxy.Address !=  null)
                //MessageBox.Show(proxy.Address.AbsoluteUri);

                var x = new QAGetLayouts();
                x.Country = "AUS";

                QASearch addressesSearch = new QASearch
                    {
                        Engine = engine,
                        Country = "AUG",
                        FormattedAddressInPicklist = true,
                        Search = QASsearchText,
                        Layout = "MedibankCRM",
                        RequestTag = "",
                        // Localisation = "AUS"
                    };
                var sw = new Stopwatch();

                sw.Start();

                var cfactory = new ChannelFactory<QAPortType>("QAPortType");
                var proxy = cfactory.CreateChannel();
                ((IClientChannel)proxy).Open();

                //  qas.UseDefaultCredentials = false;
                //   qas.Credentials = new NetworkCredential(@"IMCKESSON\AY-admin", "Tuesday12", "IMCKESSON");
                //qas.PreAuthenticate = true;
               // qas.Proxy = WebRequest.DefaultWebProxy;
               // qas.Credentials = new NetworkCredential(@"IMCKESSON\AY-admin", "Tuesday12", "IMCKESSON");
               //// qas.UseDefaultCredentials = true;
               // qas.Proxy.Credentials = new NetworkCredential(@"IMCKESSON\AY-admin", "Tuesday12", "IMCKESSON");
                //qas.Proxy.
                QASearchResult searchResult;
                DoSearchRequest dsr = new DoSearchRequest();
                dsr.QASearch = addressesSearch;
                dsr.QAQueryHeader = header;

                var dsres = proxy.DoSearch(dsr);
                searchResult = dsres.QASearchResult;

                sw.Stop();

                lblStatus.Invoke((MethodInvoker) (() => lblStatus.Text = ""));
                lstResult.Invoke((MethodInvoker) (() => lstResult.Items.Clear()));


                foreach (var pl in searchResult.QAPicklist.PicklistEntry)
                {
                    lstResult.Invoke((MethodInvoker) (() => lstResult.Items.Add(pl.Picklist)));

                }

                

                var sts = String.Format("{0} Addresses found  in {1} milliseconds",
                                        searchResult.QAPicklist.PicklistEntry.Count(), sw.ElapsedMilliseconds.ToString());
                lblStatus.Invoke((MethodInvoker) (() => lblStatus.Text = sts));
            }
            catch (Exception ex)
            {
               
                throw;
            }
            finally
            {
               // if (qas != null) qas.Dispose();
            }
        }

   
        private void Search_Click(object sender, EventArgs e)
        {
            QASsearchText = txtSearchTest.Text;

            lblStatus.Text = "";
            lstResult.Items.Clear();

            //Task.Factory.StartNew(SearchViaQAS);
            SearchViaQAS();
        }

        private void Form_Load(object sender, EventArgs e)
        {
    //        qas = new QASOnDemandIntermediary
    //        {
    //            // Credentials = new NetworkCredential("ws_609_ext", "m3d1b2NK2O14")
    //        };

    //        var authentication = new QAAuthentication
    //        {
    //            Password = "98aM3d12014",
    //            Username = "98a098f3-ee0"
    //        };

    ////            Username: ws_609_ext 
    ////Password: m3d1b2NK2O14


    //        var header = new QAQueryHeader { QAAuthentication = authentication };
    //        qas.QAQueryHeaderValue = header;

    //        var x = new QAGetLayouts();
    //        x.Country = "AUS";
        }

        private void txtTypeAhead_TextChanged(object sender, EventArgs e)
        {
            if ((txtTypeAhead.Text.Trim().Length) < 8)
                return;
            lstResult.Items.Clear();
            QASsearchText = txtTypeAhead.Text;

            if (QASsearchTimeout != null)
            {
                QASsearchTimeout.Dispose();
            }

            QASsearchTimeout = new System.Threading.Timer(state => SearchViaQAS(), null, 1000, System.Threading.Timeout.Infinite);
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            txtSearchTest.Text = "600 Victoria street richmond";
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            txtSearchTest.Text = "20 lemon grove cranbourne";
        }
    }
}
