using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
//using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
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
            ChannelFactory<QAPortType> cfactory = null;
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
              

                //cfactory = new ChannelFactory<QAPortType>("QAPortType");
                ////var b = (BasicHttpBinding)cfactory.Endpoint.Binding;
                ////b.UseDefaultWebProxy = true;
                ////cfactory.Credentials.UserName.UserName = txtU.Text;
                ////cfactory.Credentials.UserName.Password = txtP.Text;
                
                //var proxy = cfactory.CreateChannel();
                //((IClientChannel) proxy).Open();


                QASearchResult searchResult;
                DoSearchRequest dsr = new DoSearchRequest();

                dsr.QASearch = addressesSearch;
                dsr.QAQueryHeader = header;
                

                //var dsres = proxy.DoSearch(dsr);
                //searchResult = dsres.QASearchResult;


                var channelFactory = new WcfChannelFactory<QAPortType>(new BasicHttpBinding("QASOnDemand"));
               // var channelFactory = new WcfChannelFactory<QAPortType>(new CustomBinding("NewBinding0"));
                var endpointAddress = @"https://ws3.ondemand.qas.com/ProOnDemand/V3/ProOnDemandService.asmx";

                // The call to CreateChannel() actually returns a proxy that can intercept calls to the
                // service. This is done so that the proxy can retry on communication failures.            
                QAPortType qasr = channelFactory.CreateChannel(new EndpointAddress(endpointAddress));

                var t = new DoCanSearchRequest();
                t.QAQueryHeader = header;
                t.QACanSearch = new QACanSearch{Country = "AUG",Engine = engine,Layout ="MedibankCRM" };

                var y = qasr.DoCanSearch(t);
                if (y.QASearchOk.IsOk)
                {
                    lblStatus.Invoke((MethodInvoker)(() => lblStatus.Text = ""));
                    lstResult.Invoke((MethodInvoker)(() => lstResult.Items.Clear()));
                    var sw = new Stopwatch();

                    sw.Start();
                    var returnMessage = qasr.DoSearch(dsr);
                    searchResult = returnMessage.QASearchResult;

                    foreach (var pl in searchResult.QAPicklist.PicklistEntry)
                    {
                        lstResult.Invoke((MethodInvoker)(() => lstResult.Items.Add(pl.Picklist)));

                    }

                    sw.Stop();

                   

                    var sts = String.Format("{0} Addresses found  in {1} milliseconds",
                                            searchResult.QAPicklist.PicklistEntry.Count(), sw.ElapsedMilliseconds.ToString());
                    lblStatus.Invoke((MethodInvoker)(() => lblStatus.Text = sts));
                }

               

               


               
            }
            catch (FaultException fx)
            {
                if (fx.Code.Name != "AuthorizationFailed")
                throw;
            }
            catch (Exception ex)
            {
                numericUpDown1.Value = numericUpDown1.Value + 1;
                //throw;
            }
            finally
            {
                // if (qas != null) qas.Dispose();
                if (cfactory != null)
                {
                    var channel = cfactory as ICommunicationObject;
                    cfactory = default(ChannelFactory<QAPortType>);
                    try
                    {
                        channel.Close();
                    }
                    catch (CommunicationException)
                    {
                        channel.Abort();
                    }
                    catch (TimeoutException)
                    {
                        channel.Abort();
                    }
                }
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

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                textBox1.Text = "";
               // MessageBox.Show("a");
                string soap = @"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/""><s:Header><h:QAQueryHeader xmlns:h=""http://www.qas.com/OnDemand-2011-03"" xmlns=""http://www.qas.com/OnDemand-2011-03"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""><QAAuthentication><Username>98a098f3-ee0</Username><Password>98aM3d12014</Password></QAAuthentication></h:QAQueryHeader></s:Header><s:Body xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""><QASearch RequestTag="""" xmlns=""http://www.qas.com/OnDemand-2011-03""><Country>AUG</Country><Engine PromptSet=""Alternate"">Intuitive</Engine><Layout>MedibankCRM</Layout><Search>600 Victoria street richmond</Search><FormattedAddressInPicklist>true</FormattedAddressInPicklist></QASearch></s:Body></s:Envelope>";

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(@"https://ws3.ondemand.qas.com/ProOnDemand/V3/ProOnDemandService.asmx");
                req.KeepAlive = false;
                req.ProtocolVersion = HttpVersion.Version11;
                req.Headers.Add("SOAPAction", @"http://www.qas.com/OnDemand-2011-03/DoSearch");
                req.ContentType = "text/xml;charset=\"utf-8\"";
                req.Accept = "text/xml";
                req.Method = "POST";
                
                //req.AllowAutoRedirect = false;
                //req.Credentials = CredentialCache.DefaultCredentials;
                //req.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US; rv:1.9.0.5) Gecko/2008120122 Firefox/3.0.5";
                //req.PreAuthenticate = true;
                //req.UseDefaultCredentials = true;

                //IWebProxy webProxy = WebRequest.DefaultWebProxy;
                //IWebProxy proxy1 = req.Proxy;
                //MessageBox.Show("req Proxy: {0}" , proxy1.GetProxy().);

                //WebProxy webProxy = new WebProxy()
                //{
                //    // Credentials = new NetworkCredential("AY-Admin", "Tuesday12", "IMCKESSON"),
                //    UseDefaultCredentials = true,

                //};

                //MessageBox.Show(" Proxy: {0}" + webProxy.GetProxy(req.RequestUri));

               // MessageBox.Show("b");
                //req.Proxy = webProxy;
              //  MessageBox.Show("c");
                using (Stream stm = req.GetRequestStream())
                {
                //    MessageBox.Show("d");
                
                    using (StreamWriter stmw = new StreamWriter(stm))
                    {
                     //   MessageBox.Show("e");
                        stmw.Write(soap);
                    }
                }
               // MessageBox.Show("m");
                using (WebResponse webResponse = req.GetResponse())
                {
                //    MessageBox.Show("1");
                    WebResponse response = req.GetResponse();
                //    MessageBox.Show("2");
                   Stream responseStream = response.GetResponseStream();
                 //   MessageBox.Show("3");
                    Encoding encoding = System.Text.Encoding.GetEncoding("utf-8");
                  //  MessageBox.Show("4");
                    StreamReader streamReader = new StreamReader(responseStream, encoding);
                    //MessageBox.Show("5");
                    string result = streamReader.ReadToEnd();
                    textBox1.Text = result;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
           

        }
    }
}
