using System;
using Newtonsoft.Json;
using BangazonAPI.Models;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
using TestStudentExercisesAPI;

namespace TestBangazonAPI
{
    public class TestCustomer
    {

        // Create a new customer in the db and make sure we get a 200 OK status code back
        
        public async Task<Customer> createCustomer(HttpClient client)
        {
            Customer testCustomer = new Customer
            {
                FirstName = "Happy",
                LastName = "Customer",
                AccountCreated = DateTime.Today,
                LastActive = DateTime.Today,
                Archived = false
            };

            string testAsJSON = JsonConvert.SerializeObject(testCustomer);


            HttpResponseMessage response = await client.PostAsync(
                "api/customer",
                new StringContent(testAsJSON, Encoding.UTF8, "application/json")
            );

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            Customer newCustomer = JsonConvert.DeserializeObject<Customer>(responseBody);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return newCustomer;

        }

        // Delete a student in the database and make sure we get a no content status code back
        
        public async Task deleteCustomer(Customer testCustomer, HttpClient client)
        {
            HttpResponseMessage deleteResponse = await client.DeleteAsync($"api/customer/{testCustomer.Id}");
            deleteResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        }


        [Fact]
        public async Task Test_Get_All_Customer()
        {
            // Use the http client
            using (HttpClient client = new APIClientProvider().Client)
            {

                // Call the route to get all our students; wait for a response object
                HttpResponseMessage response = await client.GetAsync("api/customer");

                // Make sure that a response comes back at all
                response.EnsureSuccessStatusCode();

                // Read the response body as JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Convert the JSON to a list of student instances
                List<Customer> customerList = JsonConvert.DeserializeObject<List<Customer>>(responseBody);

                // Did we get back a 200 OK status code?
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                // Are there any students in the list?
                Assert.True(customerList.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Get_Single_Customer()
        {

            using (HttpClient client = new APIClientProvider().Client)
            {

                // Create a new customer
                Customer customer = await createCustomer(client);

                // Try to get that customer from the database
                HttpResponseMessage response = await client.GetAsync($"api/customer/{customer.Id}");

                response.EnsureSuccessStatusCode();

                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Turn the JSON into C#
                Customer convertedCustomer = JsonConvert.DeserializeObject<Customer>(responseBody);

                // Did we get back what we expected to get back? 
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("Happy", customer.FirstName);
                Assert.Equal("Customer", customer.LastName);

                // Clean up after ourselves- delete customer!
                deleteCustomer(customer, client);
            }
        }

        [Fact]
        public async Task Test_Get_NonExistent_Customer_Fails()
        {

            using (var client = new APIClientProvider().Client)
            {
                // Try to get a student with an enormously huge Id
                HttpResponseMessage response = await client.GetAsync("api/customer/999999999");

                // It should bring back a 204 no content error
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }


        [Fact]
        public async Task Test_Create_And_Delete_Customer()
        {
            using (var client = new APIClientProvider().Client)
            {

                // Create a new David
                Customer customer = await createCustomer(client);

                // Make sure his info checks out
                Assert.Equal("Happy", customer.FirstName);
                Assert.Equal("Customer", customer.LastName);
                Assert.Equal(DateTime.Today, customer.AccountCreated);

                // Clean up after ourselves - delete Test!
                deleteCustomer(customer, client);
            }
        }

        [Fact]
        public async Task Test_Delete_NonExistent_Customer_Fails()
        {
            using (var client = new APIClientProvider().Client)
            {
                // Try to delete an Id that shouldn't exist in the DB
                HttpResponseMessage deleteResponse = await client.DeleteAsync("/api/customer/600000");
                Assert.False(deleteResponse.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
            }
        }

        [Fact]
        public async Task Test_Modify_Customer()
        {

            // We're going to change a customer's name! This is their new name.
            string newFirstName = "Unhappy";

            using (HttpClient client = new APIClientProvider().Client)
            {

                // Create a new student
                Customer customer = await createCustomer(client);

                // Change their first name
                customer.FirstName = newFirstName;

                // Convert them to JSON
                string modifiedCustomerAsJSON = JsonConvert.SerializeObject(customer);

                // Make a PUT request with the new info
                HttpResponseMessage response = await client.PutAsync(
                    $"api/customer/{customer.Id}",
                    new StringContent(modifiedCustomerAsJSON, Encoding.UTF8, "application/json")
                );


                response.EnsureSuccessStatusCode();

                // Convert the response to JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // We should have gotten a no content status code
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                /*
                    GET section
                 */
                // Try to GET the customer we just edited
                HttpResponseMessage getTest = await client.GetAsync($"api/customer/{customer.Id}");
                getTest.EnsureSuccessStatusCode();

                string getTestBody = await getTest.Content.ReadAsStringAsync();
                Customer modifiedCustomer = JsonConvert.DeserializeObject<Customer>(getTestBody);

                Assert.Equal(HttpStatusCode.OK, getTest.StatusCode);

                // Make sure his name was in fact updated
                Assert.Equal(newFirstName, modifiedCustomer.FirstName);

                // Clean up after ourselves- delete him
                deleteCustomer(modifiedCustomer, client);
            }
        }
    }
}

