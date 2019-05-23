//Test Suite For Order Controller
//Charles Belcher - May 23 2019

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
    public class OrderTest
    { 
    // Create a new order in the db and make sure we get a 200 OK status code back

        public async Task<Order> createOrder(HttpClient client)
        {
        Order testOrder = new Order
            {
                CustomerId = 1,
                PaymentTypeId = 1,
                Archived = false
            };

            string testAsJSON = JsonConvert.SerializeObject(testOrder);


            HttpResponseMessage response = await client.PostAsync(
                "api/order",
                new StringContent(testAsJSON, Encoding.UTF8, "application/json")
            );

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            Order newOrder = JsonConvert.DeserializeObject<Order>(responseBody);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return newOrder;

        }

        // Delete an order in the database and make sure we get a no content status code back

        public async Task deleteOrder(Order testOrder, HttpClient client)
        {
            HttpResponseMessage deleteResponse = await client.DeleteAsync($"api/order/{testOrder.Id}?delete=True");
            deleteResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        }


        [Fact]
        public async Task Test_Get_All_Orders()
        {
            // Use the http client
            using (HttpClient client = new APIClientProvider().Client)
            {

                // Call the route to get all our students; wait for a response object
                HttpResponseMessage response = await client.GetAsync("api/order");

                // Make sure that a response comes back at all
                response.EnsureSuccessStatusCode();

                // Read the response body as JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Convert the JSON to a list of student instances
                List<Order> orderList = JsonConvert.DeserializeObject<List<Order>>(responseBody);

                // Did we get back a 200 OK status code?
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                // Are there any students in the list?
                Assert.True(orderList.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Get_Single_Order()
        {

            using (HttpClient client = new APIClientProvider().Client)
            {

                // Create a new customer
                Order order = await createOrder(client);

                // Try to get that customer from the database
                HttpResponseMessage response = await client.GetAsync($"api/order/{order.Id}");

                response.EnsureSuccessStatusCode();

                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Turn the JSON into C#
                Order convertedOrder = JsonConvert.DeserializeObject<Order>(responseBody);

                // Did we get back what we expected to get back? 
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(false, order.Archived);
                Assert.Equal(1, order.CustomerId);
                Assert.Equal(1, order.PaymentTypeId);

                // Clean up after ourselves- delete customer!
                deleteOrder(order, client);
            }
        }

        [Fact]
        public async Task Test_Get_NonExistent_Order_Fails()
        {

            using (var client = new APIClientProvider().Client)
            {
                // Try to get a student with an enormously huge Id
                HttpResponseMessage response = await client.GetAsync("api/order/999999999");

                // It should bring back a 204 no content error
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }


        [Fact]
        public async Task Test_Create_And_Delete_Order()
        {
            using (var client = new APIClientProvider().Client)
            {

                // Create a new Order
                Order order = await createOrder(client);

                // Make sure his info checks out
                Assert.Equal(false, order.Archived);
                //Assert.Equal(1, order.CustomerId);
                //Assert.Equal(1, order.PaymentType);

                // Clean up after ourselves - delete Test!
                deleteOrder(order, client);
            }
        }

        [Fact]
        public async Task Test_Delete_NonExistent_Order_Fails()
        {
            using (var client = new APIClientProvider().Client)
            {
                // Try to delete an Id that shouldn't exist in the DB
                HttpResponseMessage deleteResponse = await client.DeleteAsync("/api/order/600000");
                Assert.False(deleteResponse.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
            }
        }

        [Fact]
        public async Task Test_Modify_Order()
        {

            // We're going to change an order's archived status! This is the new status.
            int newCustomer = 3;

            using (HttpClient client = new APIClientProvider().Client)
            {

                // Create a new order
                Order order  = await createOrder(client);

                // Change their first name
                order.CustomerId = newCustomer;

                // Convert them to JSON
                string modifiedOrderAsJSON = JsonConvert.SerializeObject(order);

                // Make a PUT request with the new info
                HttpResponseMessage response = await client.PutAsync(
                    $"api/order/{order.Id}",
                    new StringContent(modifiedOrderAsJSON, Encoding.UTF8, "application/json")
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
                HttpResponseMessage getTestOrder = await client.GetAsync($"api/order/{order.Id}");
                getTestOrder.EnsureSuccessStatusCode();

                string getTestBody = await getTestOrder.Content.ReadAsStringAsync();
                Order modifiedOrder = JsonConvert.DeserializeObject<Order>(getTestBody);

                Assert.Equal(HttpStatusCode.OK, getTestOrder.StatusCode);

                // Make sure the archived status was in fact updated
                Assert.Equal(newCustomer, modifiedOrder.CustomerId);

                // Clean up after ourselves- delete him
                deleteOrder(modifiedOrder, client);
            }
        }
    }
}
