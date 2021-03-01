using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IOTSensorDataDecoder
{
    public partial class Form1 : Form
    {
        System.Timers.Timer tmrDecode = new System.Timers.Timer();

        public Form1()
        {
            InitializeComponent();
            tmrDecode.Elapsed += TmrDecode_Elapsed;
        }

        private void TmrDecode_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                tmrDecode.Enabled = false;
                //ProcessSensorData(4165, "$,1726.2265,07822.0345,80.00,00.00,00.00,27.72,12.47,13.97,.000,.000,.000#");
                DataTable dtnewresp = FetchNewData();

                if(dtnewresp.Rows.Count>0)
                {
                    foreach (DataRow dr in dtnewresp.Rows)
                    {
                        var id = dr["ID"];
                        var ibdata = dr["InboxData"].ToString();

                        ProcessSensorData(Convert.ToInt32(id), ibdata);

                        //ProcessSensorData(4129, "$862549047740769,1726.2223,07822.0418,.00,00,00.00,28.93,11.32,12.72,.000,.000,.000#");
                    }                    
                }
                tmrDecode.Enabled = true;
            }
            catch (Exception ex)
            {
                tmrDecode.Enabled = true;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                btnstart.Enabled = true;
                btnstop.Enabled = false;
            }
            catch (Exception ex)
            {

            }

        }

        private void btnstart_Click(object sender, EventArgs e)
        {
            try
            {
                btnstart.Enabled = false;
                btnstop.Enabled = true;


                tmrDecode.Enabled = true;


            }
            catch (Exception ex)
            {

            }

        }

        private void btnstop_Click(object sender, EventArgs e)
        {
            try
            {
                btnstop.Enabled = false;
                btnstart.Enabled = true;
            }
            catch (Exception ex)
            {

            }

        }

        public DataTable FetchNewData()
        {
            DataTable dt = new DataTable();

            try
            {
                string query = "SELECT ID,InboxData FROM TT_SENSOR_INBOX WHERE Status='INITIALISED'";
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SQLConnection"].ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand(query, conn);
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }

            }
            catch (Exception ex)
            {
                dt = null;
            }
            return dt;
        }

        public void ProcessSensorData(int InboxID, string Frame)
        {
            try
            {
                if (Frame.StartsWith("$"))
                {
                    string[] InputParameters = Frame.ToString().Split(new char[] { ',' });

                    //string header = InputParameters[0].Remove(0,1);

                    string longitude = InputParameters[2];

                    string latitude = InputParameters[1];

                    string machineid = InputParameters[0].Remove(0, 1);

                    decimal a = Convert.ToDecimal(latitude.Substring(0, 2));

                    decimal b = (Convert.ToDecimal(latitude) - a * 100) / 60;

                    decimal latitude1 = a + b;

                    decimal c = Convert.ToDecimal(longitude.Substring(0, 3));

                    decimal d = (Convert.ToDecimal(longitude) - c * 100) / 60;

                    decimal longitude1 = c + d;

                    
                    string datetime = InputParameters[2];

                    string tds_sens1 = InputParameters[3];
                    string tds_sens2 = InputParameters[4];
                    string tds_sens3 = InputParameters[5];

                    string temp_sens1 = InputParameters[6];
                    string temp_sens2 = InputParameters[7];
                    string temp_sens3 = InputParameters[8];

                    string flow_sens1 = InputParameters[9];
                    string flow_sens2 = InputParameters[10];
                    string flow_sens3 = InputParameters[11].Remove(InputParameters[11].Length-1);

                    if(machineid!="")
                    {
                        string query = "UPDATE ET_SENSOR_DATA SET Longitude='" + longitude1.ToString() + "'," +
                                                              "Latitude='" + latitude1.ToString() + "'," +
                                                              "DateTime= GETDATE() ," +
                                                              "TDS_Sens1='" + tds_sens1 + "'," +
                                                              "TDS_Sens2='" + tds_sens2 + "'," +
                                                              "TDS_Sens3='" + tds_sens3 + "'," +
                                                              "TEMP_Sens1='" + temp_sens1 + "'," +
                                                              "TEMP_Sens2='" + temp_sens2 + "'," +
                                                              "TEMP_Sens3='" + temp_sens3 + "'," +
                                                              "FLOW_Sens1='" + flow_sens1 + "'," +
                                                              "FLOW_Sens2='" + flow_sens2 + "'," +
                                                              "FLOW_Sens3='" + flow_sens3 + "'" +
                                                              "WHERE Machine_ID='" + machineid + "'";
                        using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SQLConnection"].ConnectionString))
                        {
                            SqlCommand cmd = new SqlCommand(query, conn);
                            conn.Open();
                            cmd.ExecuteNonQuery();
                            conn.Close();
                        }

                        UpdateInboxStatus(InboxID, "PROCESSED");
                    }
                    else
                    {
                        UpdateInboxStatus(InboxID, "FAILED");
                    }
                }
                else
                {
                    UpdateInboxStatus(InboxID, "FAILED");
                }
            }
            catch (Exception ex)
            {
                UpdateInboxStatus(InboxID, "FAILED");
            }
        }

        private void UpdateInboxStatus(long InboxId, string Status)
        {
            try
            {
                string query = "UPDATE TT_SENSOR_INBOX SET Status = '" + Status + "' WHERE ID = " + InboxId;
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SQLConnection"].ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand(query, conn);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
