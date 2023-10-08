Integrating Crystal Reports into an ASP.NET Core application with Angular isn't a straightforward task since Crystal Reports is traditionally used with ASP.NET Web Forms projects rather than ASP.NET Core. However, here is a general approach to integrate Crystal Reports into an ASP.NET Core project with Angular :
1.	Create an ASP.NET project of the web API type, preferably a version earlier than Core.
2.	Create a route that will return our PDF encrypted in base64 format :
[HttpGet]
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
3.	 Retrieve the result of your Oracle query that you want to display in your report and convert it into a DataTable:
public DataTable LoadMenus()
{
    DataTable dataTable = new DataTable();

    using (OracleConnection conn = new OracleConnection(connectionString))
    using (OracleCommand cmd = conn.CreateCommand())
    {
        try
        {
            string query = "SELECT * FROM MENU ";
            cmd.CommandText = query;
            conn.Open();

            using (var dataReader = cmd.ExecuteReader())
            {
                dataTable.Load(dataReader);
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Load Menus: " + ex.Source + " " + ex.Message);
        }
        finally
        {
            conn.Close();
        }
    }
    return dataTable;
}

4.	Insert your data into the Crystal Report, then convert it into a PDF and encrypt it in base64:

public string GenerateAndReturnReportBase64()
{
    ReportDocument report = new ReportDocument();

    try
    {
        DataTable dtMenus = new DataTable();
        dtMenus = LoadMenus();

        GC.Collect();
        GC.WaitForPendingFinalizers();

        int nb = 0;
        nb = dtMenus.Rows.Count;

        if (nb > 0)
        {
            report.Load("C:\\Restauges\\etats\\menu\\rpt_menu_resto.rpt");
            report.SetDataSource(dtMenus);
            SetReportParameters(report);
            SetReportLoginInfo(report);
        }

        string pdfFilePath = @"C:\Restauges\pdf\temp_report.pdf"; // Provide a temporary file path
        report.ExportToDisk(ExportFormatType.PortableDocFormat, pdfFilePath);

        byte[] pdfBytes = File.ReadAllBytes(pdfFilePath);

        string base64Pdf = Convert.ToBase64String(pdfBytes);

        File.Delete(pdfFilePath);

        return base64Pdf;
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error generating and returning the report: " + ex.Message);
        return null;
    }
}
5.	Connect Crystal Reports to the Oracle database:
public static void SetReportLoginInfo(ReportDocument report) {
    // Set up your database connection info here
}

6.	Return the created report.
7.	In your Angular project, install ngx-extended-pdf-viewer, which allows you to view your PDF.
8.	In your report.component.html file, add the following code

  <ngx-extended-pdf-viewer [base64Src]="base64"
                         backgroundColor="#ffffff"
                         [height]="'90vh'"
                         [useBrowserLocale]="true"
                         [handTool]="false"
                         [showBorders]="true"
                         zoom="150%"
                         [showHandToolButton]="true">
</ngx-extended-pdf-viewer>

9.	In your report.component.ts file, add the following code:

base64: string = "";
constructor(private httpClient: HttpClient) { }
ngOnInit(): void {
  this.httpClient
    .get('http://localhost:57915/api/values)
    .pipe(
      map((response) => {
        this.base64 = response as string;
      })
    )
    .subscribe();
}

This guide outlines the steps required to integrate Crystal Reports into an ASP.NET Core application with Angular. Be sure to adjust the paths, database settings, and other details to match your specific project requirements.
