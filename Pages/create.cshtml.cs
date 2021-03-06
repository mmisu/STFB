using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class CreateModel : PageModel {
    /*
      Request URL for Login action
     */
    public string login_url = "/index.php?page=api&module=auth&action=login";
    public string login_method = "POST";
    public string login_request = "";

    /*
      Request URL for Dictionaries action
      Getting Customers, Items, Warehouses, Employees and Terms dictionaries
     */
    public string dictionaries_url = "/index.php?page=api&module=dictionaries&list=Customers,Items,Warehouses,Employees,Terms&session_id=";
    public string dictionaries_method = "GET";

    /*
      Request URL for getEmpty header action
     */
    public string emptyheader_url = "/index.php?page=api&module=forms&path=AccountsReceivable/OrderProcessing/ViewOrders&action=emptyRecord&session_id=";
    public string emptyheader_method = "GET";
    
    /*
      Request URL for getEmpty detail action
     */
    public string emptydetail_url = "/index.php?page=api&module=forms&path=AccountsReceivable/OrderProcessing/ViewOrdersDetail&action=emptyRecord&session_id=";
    public string emptydetail_method = "GET";

    /*
      Request URL for Create header action
     */
    public string createheader_url = "/index.php?page=api&module=forms&path=AccountsReceivable/OrderProcessing/ViewOrders&action=create&session_id=";
    public string createheader_method = "POST";

    /*
      Request URL for Create detail action
     */
    public string createdetail_url = "/index.php?page=api&module=forms&path=AccountsReceivable/OrderProcessing/ViewOrdersDetail&action=createMany&session_id=";
    public string createdetail_method = "POST";

    /* 
       Request URL for List action
       For this example we using AccountsReceivable/OrderProcessing/ViewOrders Enterprise screen, but you can use any screen from list in file EnterpriseScreens.json
     */
    public string list_url = "/index.php?page=api&module=forms&path=AccountsReceivable/OrderProcessing/ViewOrders&action=list&session_id=";
    public string list_method = "GET";

    static HttpClient myAppHTTPClient = new HttpClient();

    public CreateModel(){
        APIRequests();
    }

    public async void APIRequests(){
        dynamic body = new JObject();
        /*Credentials for Login request*/
        body.CompanyID = "DINOS";
        body.DivisionID = "DEFAULT";
        body.DepartmentID = "DEFAULT";
        body.EmployeeID = "Demo";
        body.EmployeePassword = "Demo";
        body.language = "english";

        /*
          Login request. Request Body is JSON, Response body is JSON
          Response is json like:
          {
          "session_id": "aud8s4l449frcnponmv1ithvoo",
          "companies": [],
          "message": "ok"
          }
          Where session_id is uuid, which used for any other API request
         */
        dynamic sessionResult = JObject.Parse(await(API.doRequest(this.login_method, this.login_url, this.login_request = body.ToString())));
        Console.WriteLine(sessionResult);

        
        /*
          Dictionaries Request.
          Getting dictionaries
         */
        dynamic dictionaries = JObject.Parse(await(API.doRequest(dictionaries_method, dictionaries_url + sessionResult.session_id, null)));
        string CustomerID = "", EmployeeID = "", TermsID = "", WarehouseID = "";
        JToken  Item = new JObject(); 
        Console.WriteLine(dictionaries);
        foreach (var Customer in dictionaries.Customers.values)
        {
            CustomerID = Customer.CustomerID.Value.ToString();
            break;
        }
        //Console.WriteLine(CustomerID);
        foreach (var dItem in dictionaries.Items.values)
        {
            Item = dItem;
            break;
        }
        //Console.WriteLine(ItemID);
        foreach (var Terms in dictionaries.Terms)
        {
            TermsID = Terms.Value.value.Value.ToString();
            break;
        }
        //Console.WriteLine(TermsID);
        foreach (var Warehouse in dictionaries.Warehouses)
        {
            WarehouseID = Warehouse.Value.value.Value.ToString();
            break;
        }
        //Console.WriteLine(WarehouseID);
        foreach (var Employee in dictionaries.Employees)
        {
            EmployeeID = Employee.Value.value.Value.ToString();
            break;
        }
        //Console.WriteLine(EmployeeID);

        /*
          Forms getEmpty header Request.
          Getting empty record for inserting to Order Header
         */
        dynamic header = JObject.Parse(await(API.doRequest(emptyheader_method, emptyheader_url + sessionResult.session_id, null)));
        //filling Header record with data from Dictionaries
        header.CustomerID = CustomerID;
        header.EmployeeID = EmployeeID;
        header.TermsID = TermsID;
        header.WarehouseID = WarehouseID;
        
        Console.WriteLine(header);        
        /*
          Forms getEmpty detail Request.
          Getting empty record for inserting to Order Header
         */
        dynamic detail = JObject.Parse(await(API.doRequest(emptydetail_method, emptydetail_url + sessionResult.session_id, null)));
        //creating Header record and getting created record with binded OrderNumber and other system generated fields
        header = JObject.Parse(await(API.doRequest(createheader_method, createheader_url + sessionResult.session_id, header.ToString())));
        
        //filling Detail record with data from Dictionaries
        detail.OrderNumber = header.OrderNumber;
        detail.ItemID = Item["ItemID"];
        detail.Description = Item["ItemID"];
        detail.ItemUnitPrice = detail.ItemCost = Item["Price"];
        detail.OrderQty = 20;
        Console.WriteLine(detail);

        //creating Detail record
        dynamic details = new JArray();
        //adding two same detail records, but you can to add any numbers of different details
        details.Add(detail);
        details.Add(detail);
        await(API.doRequest(createdetail_method, createdetail_url + sessionResult.session_id, details.ToString()));
        /*
          Forms List Request.
          Getting list of all Opened Orders
         */
        Console.WriteLine(await(API.doRequest(list_method, list_url + sessionResult.session_id, null)));
    }
}
