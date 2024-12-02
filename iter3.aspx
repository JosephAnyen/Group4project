<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="iter3.aspx.cs" Inherits="Iteration3.iter3" Async="true" %>

<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Company Information Retriever</title>
    <link rel="stylesheet" href="style1.css" />
    <link href="https://fonts.googleapis.com/css2?family=Roboto:wght@400;500;700&display=swap" rel="stylesheet">
</head>
<body>
    <div class="container">
        <h1>Company Information Retriever</h1>
        <div class="container2">
            <form id="form1" runat="server" method="post">
                <div class="options">
                    <div class="city-businessline">
                        <div class="form-group">
                            <label for="city"><h2>City</h2></label>
                            <asp:DropDownList ID="city" runat="server" AppendDataBoundItems="true">
                                <asp:ListItem Text="-- Select City --" Value="" />
                            </asp:DropDownList>
                        </div>
                
                        <div class="form-group">
                            <label for="businessLine"><h2>Business Line</h2></label>
                            <asp:DropDownList ID="businessLine" runat="server" AppendDataBoundItems="true">
                                <asp:ListItem Text="-- Select Business Line --" Value="" />
                            </asp:DropDownList>
                        </div>
                    </div>

                    <div class="timeStart-End">
                        <div class="form-group">
                            <label for="timeStart"><h2>Time Start</h2></label>
                            <asp:TextBox ID="timeStart" runat="server" TextMode="Date" />
                        </div>
                        <div class="form-group">
                            <label for="timeEnd"><h2>Time End</h2></label>
                            <asp:TextBox ID="timeEnd" runat="server" TextMode="Date" />
                        </div>
                        <div class="form-group">
                            <label for="maxResults"><h2>Max Results</h2></label>
                            <asp:TextBox ID="maxResults" runat="server" TextMode="Number" />
                        </div>
                    </div>
                </div>

                <div class="button-container">
                    <asp:Button ID="fetchBtn" runat="server" Text="Fetch" OnClick="FetchData" />
                </div>
        
                <asp:Label ID="messageLabel" runat="server" ForeColor="Red" Visible="false"
                    Style="display: block; text-align: center; margin-top: 10px;"></asp:Label>

                <div class="table-inside">
                    <table>
                        <thead>
                            <tr>
                                <th>Business ID</th>
                                <th>Company Name</th>
                                <th>Address</th>
                            </tr>
                        </thead>
                        <tbody id="resultsTableBody" runat="server">
                            <!-- Rows will be populated here -->
                        </tbody>
                    </table>

                    <div class="pagination">
                        <asp:Button ID="prevButton" runat="server" Text="<" CommandArgument="Previous" CssClass="pagination-button" OnClick="Navigate" Visible="false" />
    
                        <!-- Current Page Label -->
                        <asp:Label ID="currentPageLabel" runat="server" Text="1/1" CssClass="pagination-text" />

                        <asp:Button ID="nextButton" runat="server" Text=">" CommandArgument="Next" CssClass="pagination-button" OnClick="Navigate" Visible="false" />
                    </div>
                </div>
            </form>
        </div>
    </div>
</body>
</html>
