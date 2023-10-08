using System;
using System.Data;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System.Web.Http;
using System.IO;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;

namespace RestaugesReport.Controllers
{
    public class ValuesController : ApiController
    {
        private string connectionString = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=127.0.0.1)(PORT=1521)) (CONNECT_DATA=(SERVICE_NAME=RESTOV40))); User Id=restonew;Password=restoteam;";


        //public string Get()
        //{
        //    //string pdfFilePath = "RestaugesWebApp.pdf";
        //    string pdfFilePath = @"C:\pdf\RestaugesWebApp.pdf";

        //    string base64Pdf = ConvertLocalPdfToBase64(pdfFilePath);

        //    if (base64Pdf != null)
        //    {
        //        return base64Pdf;
        //    }
        //    else
        //    {
        //        return "Conversion failed.";
        //    }
        //}

        public string Get()
        {
            string base64Pdf = GenerateAndReturnReportBase64();

            if (base64Pdf != null)
            {
                return base64Pdf;
            }
            else
            {
                return "Conversion failed.";
            }
        }

        public static string ConvertLocalPdfToBase64(string filePath)
        {
            try
            {
                // Read the PDF file as bytes
                byte[] pdfBytes = File.ReadAllBytes(filePath);

                // Convert the PDF bytes to a base64 string
                string base64Pdf = Convert.ToBase64String(pdfBytes);

                return base64Pdf;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error converting PDF to base64: " + ex.Message);
                return null;
            }
        }

        public string GenerateAndReturnReportBase64()
        {
            ReportDocument report = new ReportDocument();

            try
            {
                DateTime debut = Convert.ToDateTime("2020/01/10");
                DateTime fin = Convert.ToDateTime("2020/12/11");

                DataTable dtMenus = new DataTable();
                dtMenus = loadMenus(debut, fin);

                GC.Collect();
                GC.WaitForPendingFinalizers();

                int nb = 0;
                nb = dtMenus.Rows.Count;

                if (nb > 0)
                {

                    report.Load("C:\\Restauges\\etats\\menu\\rpt_menu_resto.rpt");
                    //report.Load(Server.MapPath("~/menus/rpt_menu_resto.rpt"));
                    report.SetDataSource(dtMenus);
                    setReportParameters(report, debut, fin);
                    //dynamic ParameterFieldInfo = getViewerParameters(debut, fin);
                    SetReportLoginInfo(report);

                }

                // Set up database connections, parameters, and other report settings as needed

                // Generate the report and export it to a PDF file
                // string pdfFilePath = "temp_report.pdf"; // Provide a temporary file path
                string pdfFilePath = @"C:\Restauges\pdf\temp_report.pdf"; // Provide a temporary file path
                report.ExportToDisk(ExportFormatType.PortableDocFormat, pdfFilePath);

                // Read the PDF file as bytes
                byte[] pdfBytes = File.ReadAllBytes(pdfFilePath);

                // Convert the PDF bytes to a base64 string
                string base64Pdf = Convert.ToBase64String(pdfBytes);

                // Clean up the temporary PDF file (optional)
                File.Delete(pdfFilePath);

                return base64Pdf;
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during report generation
                Console.WriteLine("Error generating and returning the report: " + ex.Message);
                return null;
            }
        }

        protected void setReportParameters(ReportDocument report, DateTime debut, DateTime fin)
        {
            ParameterFieldDefinitions crParameterFieldDefinitions;
            ParameterFieldDefinition crParameterFieldDefinition;
            ParameterValues crParameterValues = new ParameterValues();
            ParameterDiscreteValue crParameterDiscreteValue = new ParameterDiscreteValue();

            crParameterDiscreteValue.Value = debut;
            crParameterFieldDefinitions = report.DataDefinition.ParameterFields;
            crParameterFieldDefinition = crParameterFieldDefinitions["debut"];
            crParameterValues = crParameterFieldDefinition.CurrentValues;

            crParameterValues.Clear();
            crParameterValues.Add(crParameterDiscreteValue);
            crParameterFieldDefinition.ApplyCurrentValues(crParameterValues);

            crParameterDiscreteValue = new ParameterDiscreteValue();
            crParameterDiscreteValue = new ParameterDiscreteValue();
            crParameterDiscreteValue.Value = fin;
            crParameterFieldDefinitions = report.DataDefinition.ParameterFields;
            crParameterFieldDefinition = crParameterFieldDefinitions["fin"];
            crParameterValues = crParameterFieldDefinition.CurrentValues;


            crParameterValues.Clear();
            crParameterValues.Add(crParameterDiscreteValue);
            crParameterFieldDefinition.ApplyCurrentValues(crParameterValues);
        }

        public static void SetReportLoginInfo(ReportDocument report)
        {
            TableLogOnInfos crtableLogoninfos = new TableLogOnInfos();
            TableLogOnInfo crtableLogoninfo = new TableLogOnInfo();
            ConnectionInfo crConnectionInfo = new ConnectionInfo();
            Tables CrTables;

            crConnectionInfo.ServerName = ConfigurationManager.AppSettings["SERVER"];
            crConnectionInfo.DatabaseName = ConfigurationManager.AppSettings["DATABASE"];
            crConnectionInfo.UserID = ConfigurationManager.AppSettings["USERID"];
            crConnectionInfo.Password = ConfigurationManager.AppSettings["PASSWORD"];


            CrTables = report.Database.Tables;
            foreach (Table CrTable in CrTables)
            {
                crtableLogoninfo = CrTable.LogOnInfo;
                crtableLogoninfo.ConnectionInfo = crConnectionInfo;
                CrTable.ApplyLogOnInfo(crtableLogoninfo);
            }
        }

        public DataTable loadMenus(DateTime debut, DateTime fin)
        {
            DataTable dataTable = new DataTable();

            using (OracleConnection conn = new OracleConnection(connectionString))
            using (OracleCommand cmd = conn.CreateCommand())
            {
                try
                {
                    string query = "SELECT MENU.CODE_MENU, " +
                        "MENU.DATE_MENU," +
                        "ELEMENT_MENU.CODE_ELEMENT," +
                        "ELEMENT.LIBELLE_COURT_ELEMENT," +
                        "ELEMENT.LIBELLE_LONG_ELEMENT,NATURE.LIBELLE_NATURE," +
                        "NATURE.CODE_NATURE,MENU_TYPE.TYPE " +
                        "FROM MENU " +
                        "INNER JOIN ELEMENT_MENU ON MENU.CODE_MENU = ELEMENT_MENU.CODE_MENU " +
                        "INNER JOIN ELEMENT ON ELEMENT_MENU.CODE_ELEMENT = ELEMENT.CODE_ELEMENT " +
                        "INNER JOIN NATURE ON ELEMENT.CODE_NATURE = NATURE.CODE_NATURE " +
                        "INNER JOIN MENU_TYPE ON MENU.CODE_MENU = MENU_TYPE.CODE_MENU " +
                        "WHERE MENU.CODE_MENU = ELEMENT_MENU.CODE_MENU AND ELEMENT_MENU.CODE_ELEMENT = ELEMENT.CODE_ELEMENT " +
                        "AND (MENU.DATE_MENU >= TO_DATE('" + debut.ToShortDateString() + "', 'DD/MM/YYYY') " +
                        "AND MENU.DATE_MENU <= TO_DATE('" + fin.ToShortDateString() + "',  'DD/MM/YYYY'))" +
                        "ORDER BY MENU.DATE_MENU DESC, NATURE.CODE_NATURE";

                    cmd.CommandText = query;
                    conn.Open();

                    using (var dataReader = cmd.ExecuteReader())
                    {
                        dataTable.Load(dataReader);
                    }


                }
                catch (Exception ex)
                {
                    throw new Exception("Load Menus : " + ex.Source + " " + ex.Message);
                }
                finally
                {
                    conn.Close();
                }
            }
            return dataTable;
        }




    }
}