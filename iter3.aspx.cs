using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Newtonsoft.Json;

namespace Iteration3
{
    public partial class iter3 : System.Web.UI.Page
    {
        private static readonly HttpClient client = new HttpClient();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadDropdowns();
                LoadStoredValues();
                ShowNoResult();
            }
        }

        private const int PageSize = 5; // Number of rows per page
        private int CurrentPage
        {
            get => (int)(Session["CurrentPage"] ?? 0);
            set => Session["CurrentPage"] = value;
        }

        protected async void FetchData(object sender, EventArgs e)
        {
            if (city.SelectedIndex == 0 || businessLine.SelectedIndex == 0 ||
                string.IsNullOrWhiteSpace(timeStart.Text) || string.IsNullOrWhiteSpace(timeEnd.Text) ||
                string.IsNullOrWhiteSpace(maxResults.Text))
            {
                ShowMessage("Please enter data for all required fields.");
                ShowNoResult();
                return;
            }

            messageLabel.Visible = false;
            resultsTableBody.Visible = true;

            string cityValue = city.SelectedValue;
            string businessLineValue = businessLine.SelectedValue;
            string timeStartValue = timeStart.Text;
            string timeEndValue = timeEnd.Text;
            int maxResultsValue = int.Parse(maxResults.Text);

            Session["city"] = cityValue;
            Session["businessLine"] = businessLineValue;
            Session["timeStart"] = timeStartValue;
            Session["timeEnd"] = timeEndValue;
            Session["maxResults"] = maxResultsValue;

            resultsTableBody.Controls.Clear();

            var fetchedData = await GetDataFromApi(cityValue, businessLineValue, timeStartValue, timeEndValue, maxResultsValue);

            // Limit data to maxResultsValue before storing in session
            var limitedData = fetchedData.Take(maxResultsValue).ToList();

            Session["FetchedData"] = limitedData; // Store limited data in session
            CurrentPage = 0; // Reset to first page
            DisplayCurrentPage();
            ResetInputFields();
        }

        protected void Navigate(object sender, EventArgs e)
        {
            string direction = ((Button)sender).CommandArgument;
            if (direction == "Next")
            {
                CurrentPage++;
            }
            else if (direction == "Previous")
            {
                CurrentPage = Math.Max(CurrentPage - 1, 0); // Ensure not below 0
            }
            DisplayCurrentPage();
        }

        private void DisplayCurrentPage()
        {
            var fetchedData = Session["FetchedData"] as List<Company>;
            if (fetchedData == null || !fetchedData.Any())
            {
                ShowNoResult();
                return;
            }

            int startIndex = CurrentPage * PageSize;
            int maxResultsValue = (int)Session["maxResults"];

            // Ensure the pagination does not exceed the maxResultsValue
            var pagedData = fetchedData.Skip(startIndex).Take(PageSize).ToList();

            PopulateTable(pagedData);

            // Calculate total pages
            int totalPages = (int)Math.Ceiling((double)fetchedData.Count / PageSize);

            // Update the pagination control to show current page and total pages
            currentPageLabel.Text = $"{CurrentPage + 1}/{totalPages}";  // Current page is 1-based

            // Control visibility of navigation buttons
            prevButton.Visible = CurrentPage > 0;
            nextButton.Visible = startIndex + PageSize < Math.Min(fetchedData.Count, maxResultsValue);
        }

        private async Task<List<Company>> GetDataFromApi(string city, string businessLine, string timeStart, string timeEnd, int maxResults)
        {
            string baseUrl = $"https://avoindata.prh.fi/opendata-ytj-api/v3/companies?location={city}&businessLine={businessLine}&timeStart={timeStart}&timeEnd={timeEnd}";
            List<Company> allCompanies = new List<Company>();
            int currentPage = 0;
            int pageSize = 100;
            bool hasMoreData = true;

            try
            {
                while (hasMoreData && allCompanies.Count < maxResults)
                {
                    string apiUrl = $"{baseUrl}&page={currentPage}&pageSize={pageSize}";
                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    response.EnsureSuccessStatusCode();

                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(jsonResponse);

                    if (apiResponse.Companies == null || apiResponse.Companies.Count == 0)
                    {
                        hasMoreData = false;
                    }
                    else
                    {
                        allCompanies.AddRange(apiResponse.Companies);

                        if (allCompanies.Count >= maxResults)
                        {
                            break;
                        }
                        currentPage++;
                    }
                }

                return allCompanies.Take(maxResults).ToList();
            }
            catch (Exception ex)
            {
                ShowMessage($"Error fetching data: {ex.Message}");
                ShowNoResult();
                return new List<Company>();
            }
        }


        private void LoadDropdowns()
        {
            string[] cities = { "Kuopio", "Rovaniemi", "Espoo", "Porvoo", "Pori", "Hämeenlinna", "Mikkeli", "Lahti" };
            city.DataSource = cities;
            city.DataBind();

            string[] businessServices = { "Agriculture, forestry and fishing", "Mining and quarrying", "Manufacturing", "Electricity, gas, steam and air conditioning supply", "Construction", "Transportation and storage", "Financial and insurance activities", "Education" };
            businessLine.DataSource = businessServices;
            businessLine.DataBind();
        }

        private void LoadStoredValues()
        {
            if (Session["city"] != null) city.SelectedValue = Session["city"].ToString();
            if (Session["businessLine"] != null) businessLine.SelectedValue = Session["businessLine"].ToString();
            if (Session["timeStart"] != null) timeStart.Text = Session["timeStart"].ToString();
            if (Session["timeEnd"] != null) timeEnd.Text = Session["timeEnd"].ToString();
            if (Session["maxResults"] != null) maxResults.Text = Session["maxResults"].ToString();
        }

        private void PopulateTable(List<Company> companies)
        {
            if (companies.Count == 0)
            {
                TableRow noResultRow = new TableRow();
                noResultRow.Cells.Add(new TableCell { Text = "No result available", ColumnSpan = 4, HorizontalAlign = HorizontalAlign.Left });
                resultsTableBody.Controls.Add(noResultRow);
                return;
            }

            foreach (var company in companies)
            {
                TableRow row = new TableRow();

                row.Cells.Add(new TableCell { Text = company.BusinessId?.Value ?? "N/A" });

                var companyName = company.Names?.FirstOrDefault();
                row.Cells.Add(new TableCell { Text = companyName?.Name ?? "N/A" });

                var address = company.Addresses?.FirstOrDefault();
                if (address != null)
                {
                    string addressText = $"{address.Street} {address.BuildingNumber}, {address.PostCode}, {address.PostOffices?.FirstOrDefault()?.City ?? "N/A"}";
                    row.Cells.Add(new TableCell { Text = addressText });
                }
                else
                {
                    row.Cells.Add(new TableCell { Text = "No Address Available" });
                }

                resultsTableBody.Controls.Add(row);
            }
        }

        private void ResetInputFields()
        {
            city.SelectedIndex = 0;
            businessLine.SelectedIndex = 0;
            timeStart.Text = string.Empty;
            timeEnd.Text = string.Empty;
            maxResults.Text = string.Empty;
        }

        private void ShowNoResult()
        {
            resultsTableBody.Controls.Clear();
            TableRow noResultRow = new TableRow();
            noResultRow.Cells.Add(new TableCell { Text = "No result available", ColumnSpan = 4, HorizontalAlign = HorizontalAlign.Left });
            resultsTableBody.Controls.Add(noResultRow);
        }

        private void ShowMessage(string message)
        {
            messageLabel.Text = message;
            messageLabel.Visible = true;
        }

        public class ApiResponse
        {
            public int TotalResults { get; set; }
            public List<Company> Companies { get; set; }
        }

        public class Company
        {
            public BusinessId BusinessId { get; set; }
            public List<CompanyName> Names { get; set; }
            public MainBusinessLine MainBusinessLine { get; set; }
            public List<Address> Addresses { get; set; }
        }

        public class Address
        {
            public string Street { get; set; }
            public string PostCode { get; set; }
            public string BuildingNumber { get; set; }
            public List<PostOffice> PostOffices { get; set; }
        }

        public class PostOffice
        {
            public string City { get; set; }
        }

        public class BusinessId
        {
            public string Value { get; set; }
        }

        public class CompanyName
        {
            public string Name { get; set; }
        }

        public class MainBusinessLine
        {
            public string Type { get; set; }
        }
    }
}